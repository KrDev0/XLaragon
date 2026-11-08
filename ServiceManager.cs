using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Laragon
{
    /// <summary>
    /// Starts and stops the servers the way the original does. The command lines, the waiting
    /// times and the order of the two "all" buttons were read from the original's own code
    /// (see MECHANICS.md section 10 for the function addresses).
    /// </summary>
    public sealed class ServiceManager
    {
        private readonly Stack _stack;

        public ServiceManager(Stack stack)
        {
            _stack = stack;
        }

        /// <summary>
        /// The order in which "Start All" brings things up, read one guard at a time from
        /// FUN_100050c70 (0x100050c70): apache, nginx, mysql, memcached, php, redis, mongodb,
        /// postgresql, mailpit — and nginx's PHP settings files rewritten last (FUN_10004efd0).
        /// </summary>
        private static readonly string[] StartOrder =
        {
            "apache", "nginx", "mysql", "mariadb", "memcached", "php", "redis", "mongodb",
            "postgresql", "mailpit"
        };

        /// <summary>
        /// The order in which "Stop" takes things down, from FUN_100051690 (0x100051690):
        /// MySQL first (it likes to be asked politely), then Apache, MongoDB, the small
        /// servers, nginx last, and MySQL is poked once more at the end.
        /// </summary>
        private static readonly string[] StopOrder =
        {
            "mysql", "mariadb", "apache", "mongodb", "postgresql", "redis", "memcached",
            "mailpit", "php", "nginx"
        };

        public string Start(Component service)
        {
            if (service == null || !service.Installed)
            {
                return "not installed";
            }

            if (service.IsRunning)
            {
                return "already running";
            }

            if (string.IsNullOrEmpty(service.Executable) || !File.Exists(service.Executable))
            {
                return "executable not found: " + service.Executable;
            }

            try
            {
                if (service.Key == "mysql" || service.Key == "mariadb")
                {
                    string problem = PrepareMyIni(service);
                    if (problem != null)
                    {
                        return problem;
                    }
                }

                ProcessStartInfo info = BuildStartInfo(service);
                Process process = Process.Start(info);
                if (process != null)
                {
                    process.EnableRaisingEvents = false;
                }

                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// The original copies <c>bin\mysql\my.ini</c> to <c>&lt;version&gt;\my.ini</c> the first
        /// time it starts MySQL (FUN_10004fb60 at 0x10004fb60); when neither file is there it
        /// says "File does not exist!" instead of starting a server that cannot read a config.
        /// </summary>
        private string PrepareMyIni(Component service)
        {
            string target = Path.Combine(service.Folder, "my.ini");
            if (File.Exists(target))
            {
                return null;
            }

            string[] templates =
            {
                Path.Combine(_stack.Bin, "mysql", "my.ini"),
                Path.Combine(_stack.Bin, "mariadb", "my.ini"),
                Path.Combine(_stack.Etc, "mysql", "my.ini")
            };

            string template = templates.FirstOrDefault(File.Exists);
            if (template == null)
            {
                // nothing to copy: the server can still start on its compiled-in defaults
                return null;
            }

            try
            {
                File.Copy(template, target, false);
                return null;
            }
            catch (Exception ex)
            {
                return "my.ini: " + ex.Message;
            }
        }

        public void Stop(Component service)
        {
            if (service == null)
            {
                return;
            }

            if (service.Key == "mysql" || service.Key == "mariadb")
            {
                StopMySql();
                return;
            }

            if (service.Key == "nginx")
            {
                // FUN_100051690: ask nginx to quit, kill what is left, then wait a moment
                string exe = service.Executable;
                if (!string.IsNullOrEmpty(exe) && File.Exists(exe))
                {
                    Run(exe, "-s stop", service.Folder);
                }

                KillByName(service.ProcessName);
                System.Threading.Thread.Sleep(10);
                return;
            }

            KillByName(service.ProcessName);
        }

        /// <summary>
        /// MySQL is stopped in three steps (FUN_100051330 at 0x100051330): ask
        /// <c>mysqladmin -u root "&lt;password&gt;" shutdown</c> and give it 2 seconds in 100 ms
        /// steps; if it is still there, force it with taskkill; if it survives that too, kill it.
        /// </summary>
        private void StopMySql()
        {
            Component mysql = _stack.Find("mysql") ?? _stack.Find("mariadb");
            if (mysql == null)
            {
                return;
            }

            string folder = mysql.Folder;
            string admin = string.IsNullOrEmpty(folder) ? null : Path.Combine(folder, "bin", "mysqladmin.exe");
            string password = _stack.Settings.Get("mysql", "Password", string.Empty);

            if (admin != null && File.Exists(admin))
            {
                Run(admin, "-u root \"" + password + "\" shutdown", folder);

                for (int i = 0; i < 20; i++)
                {
                    if (!mysql.IsRunning)
                    {
                        return;
                    }

                    System.Threading.Thread.Sleep(100);
                }
            }

            KillByName(mysql.ProcessName);
        }

        private static void KillByName(string processName)
        {
            if (string.IsNullOrEmpty(processName))
            {
                return;
            }

            foreach (Process process in Process.GetProcessesByName(processName))
            {
                try
                {
                    process.Kill(true);
                    process.WaitForExit(4000);
                }
                catch
                {
                    // a service owned by another user cannot be stopped: the original says
                    // "Service %s is running, BUT under other process."
                }
                finally
                {
                    process.Dispose();
                }
            }
        }

        private static void Run(string exe, string args, string workDir)
        {
            var info = new ProcessStartInfo(exe, args)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = string.IsNullOrEmpty(workDir) ? Path.GetDirectoryName(exe) : workDir
            };

            using (Process process = Process.Start(info))
            {
                if (process != null)
                {
                    process.WaitForExit(3000);
                }
            }
        }

        private ProcessStartInfo BuildStartInfo(Component service)
        {
            string workingDirectory = string.IsNullOrEmpty(service.Folder)
                ? Path.GetDirectoryName(service.Executable)
                : service.Folder;

            string args = ArgumentsFor(service);
            return new ProcessStartInfo(service.Executable, args)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = workingDirectory
            };
        }

        /// <summary>
        /// The exact command lines, taken from the functions that build them:
        /// Apache <c>&lt;version&gt;\bin\httpd </c> (FUN_10004f3b0), nginx <c>&lt;version&gt;\nginx -p "&lt;version&gt;"</c>
        /// (FUN_10004f530), MySQL <c>&lt;version&gt;\bin\mysqld  --log-error=&lt;version&gt;\mysqld.log</c>
        /// (FUN_10004fb60). The rest are the documented estimates.
        /// </summary>
        private string ArgumentsFor(Component service)
        {
            switch (service.Key)
            {
                case "apache":
                    // the original passes no arguments: httpd.conf sits beside the exe
                    return string.Empty;

                case "nginx":
                    return "-p \"" + service.Folder + "\"";

                case "mysql":
                case "mariadb":
                    return "--log-error=" + Path.Combine(service.Folder, "mysqld.log");

                case "postgresql":
                {
                    // FUN_100050060 (0x100050060): <version>\bin\pg_ctl with
                    // ` -D "<data folder>" -l <log file> -w start`
                    string data = Path.Combine(_stack.DataDir, "postgresql");
                    Directory.CreateDirectory(data);
                    return "-D \"" + data + "\" -l \"" + Path.Combine(data, "server.log") + "\" -w start";
                }

                case "mongodb":
                {
                    string conf = FirstExisting(Path.Combine(_stack.Etc, "mongodb", "mongod.conf"));
                    if (conf != null)
                    {
                        return "--config \"" + conf + "\"";
                    }

                    string data = Path.Combine(_stack.DataDir, "mongodb");
                    Directory.CreateDirectory(data);
                    return "--dbpath \"" + data + "\" --port " + service.Port;
                }

                case "redis":
                {
                    // FUN_100050510 (0x100050510): <version>\redis-server.exe "<conf>", using
                    // redis.windows.conf beside the exe (the original writes a default one when
                    // it is missing).
                    string conf = Path.Combine(service.Folder ?? string.Empty, "redis.windows.conf");
                    return "\"" + conf + "\"";
                }

                case "memcached":
                    // FUN_10004e990 (0x10004e990): <version>\memcached.exe with no arguments
                    return string.Empty;

                case "php":
                {
                    // FUN_10004eb00 (0x10004eb00): the PHP folder + `\php  -S localhost:` +
                    // the port, then ` -t ` (0x1003942b0) and the document root. The original
                    // passes the root without quotes; this build quotes it so a folder such as
                    // "C:\Users\John Smith\www" still works.
                    return "  -S localhost:" + service.Port + " -t \"" + _stack.Www + "\"";
                }

                case "mailpit":
                {
                    // FUN_1000862a0 (0x1000862a0): <version>\mailpit.exe with
                    // ` --smtp 127.0.0.1:<port> --log-file <version>/mailpit.log --quiet`
                    string log = Path.Combine(service.Folder ?? string.Empty, "mailpit.log");
                    return "--smtp 127.0.0.1:" + service.Port + " --log-file \"" + log + "\" --quiet";
                }

                default:
                    return string.Empty;
            }
        }

        private static string FirstExisting(params string[] candidates)
        {
            return candidates.FirstOrDefault(File.Exists);
        }

        private static int Rank(string[] order, Component c)
        {
            int index = Array.IndexOf(order, c.Key.ToLowerInvariant());
            return index < 0 ? order.Length : index;
        }

        /// <summary>
        /// Starts the services that are installed, not running yet and ticked by the person,
        /// in the original's order (FUN_100050c70). Pass null to start everything, which is what
        /// the original's own Start All does.
        /// </summary>
        public int StartAll(List<string> started, List<string> problems)
        {
            return StartAll(started, problems, null);
        }

        public int StartAll(List<string> started, List<string> problems, ICollection<string> only)
        {
            int count = 0;
            List<Component> services = _stack.Services.ToList();
            services.Sort((a, b) => Rank(StartOrder, a).CompareTo(Rank(StartOrder, b)));

            foreach (Component service in services)
            {
                if (only != null && !only.Contains(service.Key))
                {
                    continue;
                }

                if (service.IsRunning)
                {
                    continue;
                }

                string problem = Start(service);
                if (problem == null)
                {
                    count++;
                    started.Add(service.Name);
                }
                else if (problem != "already running")
                {
                    problems.Add(service.Name + ": " + problem);
                }
            }

            // nginx reads its PHP settings from a file the original rewrites after everything
            // else is up (FUN_10004efd0 at the end of FUN_100050c70); a restart is what our
            // rebuild does instead, because it does not generate those files.
            return count;
        }

        public int StopAll()
        {
            int count = 0;
            List<Component> services = _stack.Services.ToList();
            services.Sort((a, b) => Rank(StopOrder, a).CompareTo(Rank(StopOrder, b)));

            foreach (Component service in services)
            {
                if (service.IsRunning)
                {
                    Stop(service);
                    count++;
                }
            }

            return count;
        }
    }
}
