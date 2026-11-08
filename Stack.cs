using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Laragon
{
    /// <summary>
    /// One piece of software Laragon manages: PHP, Apache, MySQL, Nginx, Node.js ...
    /// "Installed" means the folder is on disk; "IsRunning" comes from the process list.
    /// </summary>
    public sealed class Component
    {
        public string Name;
        public string Key;
        public string Version;
        public string Folder;
        public string Executable;
        public string ProcessName;
        public bool IsService;
        public int Port;

        /// <summary>
        /// The folder under bin\\ this software lives in ("php", "nodejs"): FUN_1000aaf30
        /// (0x1000aaf30) builds the path as bin\\ + this + "\\" + the version folder.
        /// </summary>
        public string SubFolder = string.Empty;

        /// <summary>
        /// Every installed version folder in bin\\&lt;subfolder&gt;, in the order the folder listing
        /// gives them (alphabetical, which is what the original's own listing returns). This is
        /// what fills each menu's Version submenu — FUN_100069ba0 (0x100069ba0) and its twelve
        /// twins put one row in it per folder.
        /// </summary>
        public readonly List<string> Versions = new List<string>();

        /// <summary>The ini switches this web server to https (the original keeps it at +0x80).</summary>
        public bool Ssl;

        public bool Installed
        {
            get { return !string.IsNullOrEmpty(Folder); }
        }

        public bool IsRunning
        {
            get { return !string.IsNullOrEmpty(ProcessName) && Stack.CountProcesses(ProcessName) > 0; }
        }

        public string Display
        {
            get { return string.IsNullOrEmpty(Version) ? Name : Name + " " + Version; }
        }
    }

    /// <summary>
    /// The Laragon folder: bin\ (the software), www\ (sites), data\, etc\, usr\.
    /// If this program was copied somewhere with no bin folder next to it, an existing
    /// install under C:\laragon is used instead (read only: settings stay beside this program).
    /// </summary>
    public sealed class Stack
    {
        public const string DefaultRootHint = @"C:\laragon";

        private readonly List<Component> _components = new List<Component>();

        public string Root { get; private set; }
        public string RootSource { get; private set; }
        public Settings Settings { get; private set; }
        public string ExeFolder { get; private set; }

        public Stack()
        {
            ExeFolder = AppContext.BaseDirectory.TrimEnd('\\');
            Settings = new Settings(Path.Combine(ExeFolder, "usr", "laragon.ini"));
            Settings.Load();

            Root = ExeFolder;
            RootSource = "this program's folder";
            if (!Directory.Exists(Path.Combine(Root, "bin")) && Directory.Exists(Path.Combine(DefaultRootHint, "bin")))
            {
                Root = DefaultRootHint;
                RootSource = "an existing Laragon install";
            }

            Detect();
        }

        public string Bin
        {
            get { return Path.Combine(Root, "bin"); }
        }

        /// <summary>
        /// &lt;root&gt;\bin\laragon\lang: the folder the original reads its language packs from.
        /// FUN_10008a690 (0x10008a690) lists "*.txt" in it to fill the Language drop-down, and
        /// FUN_100095400 (0x100095400) tests the file a chosen name points at, answering
        /// "English" when it is not there. The install on this machine has 33 packs in it.
        /// </summary>
        public string LangFolder
        {
            get { return Path.Combine(Bin, "laragon", "lang"); }
        }

        public string Etc
        {
            get { return Path.Combine(Root, "etc"); }
        }

        public string Www
        {
            // The original keeps the web root on the web server itself: the person's own
            // laragon.ini has [apache] DocumentRoot=C:\laragon\www. It is read from
            // [laragon] first (where this rebuild writes it) and from [apache] second.
            get
            {
                return Settings.Get("laragon", "DocumentRoot",
                    Settings.Get("apache", "DocumentRoot", Path.Combine(Root, "www")));
            }
        }

        public string DataDir
        {
            // Also the original's own section: the person's ini has [preferences] DataDir.
            get
            {
                return Settings.Get("preferences", "DataDir",
                    Settings.Get("laragon", "DataDir", Path.Combine(Root, "data")));
            }
        }

        public string HostFormat
        {
            get { return Settings.Get("laragon", "HostnameFormat", "{name}.test"); }
        }

        public int Port(string key, int fallback)
        {
            return Settings.GetInt(key, "Port", fallback);
        }

        public int ApachePort { get { return Port("apache", 80); } }

        /// <summary>
        /// The https port. The original reads it from the same ini section under the key
        /// "SSLPort" (FUN_1000abea0 at 0x1000abea0) and its default is 443, except 8443 for
        /// nginx when an Apache is installed as well.
        /// </summary>
        public int SslPort(string key, int fallback)
        {
            return Settings.GetInt(key, "SSLPort", fallback);
        }

        public int ApacheSslPort { get { return SslPort("apache", 443); } }

        public int NginxPort { get { return Port("nginx", 8080); } }

        public int NginxSslPort
        {
            get
            {
                Component apache = Find("apache");
                bool apacheThere = apache != null && apache.Installed;
                return SslPort("nginx", apacheThere ? 8443 : 443);
            }
        }

        /// <summary>
        /// "http://" or "https://", decided the way FUN_10008fee0 (0x10008fee0) does: https as
        /// soon as the running web server has SSL switched on; with no web server running, https
        /// only when every web server is set to SSL.
        /// </summary>
        public string Scheme()
        {
            Component apache = Find("apache");
            Component nginx = Find("nginx");
            bool apacheUp = apache != null && apache.IsRunning;
            bool nginxUp = nginx != null && nginx.IsRunning;
            bool apacheSsl = apache != null && apache.Ssl;
            bool nginxSsl = nginx != null && nginx.Ssl;

            if ((apacheUp && apacheSsl) || (nginxUp && nginxSsl))
            {
                return "https://";
            }

            if (!apacheUp && !nginxUp)
            {
                return apacheSsl || nginxSsl ? "https://" : "http://";
            }

            return "http://";
        }

        /// <summary>
        /// The port the browser should use, the way FUN_10008fdc0 (0x10008fdc0) picks it:
        /// https takes the SSL port of the web server in use, http its plain port, and when
        /// both web servers are up a port that belongs to only one of them decides which.
        /// </summary>
        public int WebPort()
        {
            Component apache = Find("apache");
            Component nginx = Find("nginx");
            bool apacheUp = apache != null && apache.IsRunning;
            bool nginxUp = nginx != null && nginx.IsRunning;
            bool https = Scheme() == "https://";

            int apachePort = https ? ApacheSslPort : ApachePort;
            int nginxPort = https ? NginxSslPort : NginxPort;

            if (apacheUp && nginxUp)
            {
                if (apachePort == nginxPort)
                {
                    return nginxUp ? nginxPort : apachePort;
                }

                return apachePort != nginxPort && !apacheUp ? nginxPort : apachePort;
            }

            return nginxUp ? nginxPort : apachePort;
        }

        public int MySqlPort { get { return Port("mysql", 3306); } }
        public int PostgreSqlPort { get { return Port("postgresql", 5432); } }
        public int MongoPort { get { return Port("mongodb", 27017); } }
        public int RedisPort { get { return Port("redis", 6379); } }
        public int MemcachedPort { get { return Port("memcached", 11211); } }
        public int MailpitPort { get { return Port("mailpit", 1025); } }
        /// <summary>MariaDB's own port key, so its row in Preferences edits MariaDB alone.</summary>
        public int MariaDbPort { get { return Port("mariadb", 3306); } }

        public int PhpServerPort { get { return Port("php", 8000); } }

        public IList<Component> Components
        {
            get { return _components; }
        }

        public Component Find(string key)
        {
            return _components.FirstOrDefault(c => string.Equals(c.Key, key, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<Component> Services
        {
            get { return _components.Where(c => c.IsService && c.Installed); }
        }

        public bool AnyServiceRunning
        {
            get { return _components.Any(c => c.IsService && c.IsRunning); }
        }

        public static int CountProcesses(string processName)
        {
            if (string.IsNullOrEmpty(processName))
            {
                return 0;
            }

            try
            {
                return Process.GetProcessesByName(processName).Length;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>bin\&lt;stack&gt;\&lt;version folder&gt; layout, newest version wins.</summary>
        /// <summary>
        /// Reads every service record again, the way the original re-reads them after a version or
        /// a setting changed (its own refresh work lands in FUN_100061fc0 at 0x100061fc0).
        /// </summary>
        public void Reload()
        {
            Detect();
        }

        /// <summary>bin\&lt;stack&gt;\&lt;version folder&gt;: one record per software.</summary>
        private void Detect()
        {
            _components.Clear();

            // PHP is a service: the original's Start All starts it with `php -S`
            // (FUN_10004eb00), so its row has an "Enabled" tick like the servers and its
            // port lives under [php] Port.
            Add("PHP", "php", "php", "php-", "php.exe", "php", true, PhpServerPort);
            Add("Apache", "apache", "apache", "httpd-", @"bin\httpd.exe", "httpd", true, ApachePort);
            Add("Nginx", "nginx", "nginx", "nginx-", "nginx.exe", "nginx", true, NginxPort);
            Add("MySQL", "mysql", "mysql", "mysql-", @"bin\mysqld.exe", "mysqld", true, MySqlPort);
            Add("MariaDB", "mariadb", "mariadb", "mariadb-", @"bin\mariadbd.exe", "mariadbd", true, MariaDbPort);
            Add("PostgreSQL", "postgresql", "postgresql", "postgresql-", @"bin\pg_ctl.exe", "postgres", true, PostgreSqlPort);
            Add("MongoDB", "mongodb", "mongodb", "mongodb-", @"bin\mongod.exe", "mongod", true, MongoPort);
            Add("Redis", "redis", "redis", "redis-", "redis-server.exe", "redis-server", true, RedisPort);
            Add("Memcached", "memcached", "memcached", "memcached-", "memcached.exe", "memcached", true, MemcachedPort);
            Add("Mailpit", "mailpit", "mailpit", string.Empty, "mailpit.exe", "mailpit", true, MailpitPort);
            Add("Node.js", "nodejs", "nodejs", "node-", "node.exe", "node", false, 0);
            Add("Python", "python", "python", "python-", "python.exe", "python", false, 0);
            Add("Ruby", "ruby", "ruby", "ruby-", @"bin\ruby.exe", "ruby", false, 0);
            Add("Go", "go", "go", "go", @"bin\go.exe", "go", false, 0);
            Add("Java", "java", "java", "jdk", @"bin\java.exe", "java", false, 0);
            Add("Git", "git", "git", string.Empty, @"cmd\git.exe", "git", false, 0);
            Add("Composer", "composer", "composer", string.Empty, "composer.bat", string.Empty, false, 0);
            Add("phpMyAdmin", "phpmyadmin", "phpmyadmin", string.Empty, string.Empty, string.Empty, false, 0);
        }

        private void Add(string name, string key, string subFolder, string prefix, string relativeExe,
                         string processName, bool isService, int port)
        {
            var component = new Component
            {
                Name = name,
                Key = key,
                ProcessName = processName,
                IsService = isService,
                Port = port,
                Ssl = Settings.GetBool(key, "SSL", false),
                Version = string.Empty,
                Folder = string.Empty,
                Executable = string.Empty
            };

            string baseFolder = Path.Combine(Bin, subFolder);
            component.SubFolder = subFolder;
            if (!Directory.Exists(baseFolder))
            {
                _components.Add(component);
                return;
            }

            // Every folder in bin\<subfolder> is one installed version, and they all go into
            // component.Versions so the Version submenu can list them. The original applies no
            // name test at all here: FUN_100069ba0 (0x100069ba0) takes every entry of the listing
            // that exists as a folder, which is why bin\nodejs\node-v22 is a version even though
            // its name has no dotted number in it.
            string[] children = Directory.GetDirectories(baseFolder);
            foreach (string child in children)
            {
                component.Versions.Add(Path.GetFileName(child));
            }

            string versionFolder = PickVersionFolder(baseFolder, key, component.Versions);
            if (versionFolder == null)
            {
                _components.Add(component);
                return;
            }

            component.Folder = versionFolder;
            component.Version = VersionOf(prefix, Path.GetFileName(versionFolder));

            if (!string.IsNullOrEmpty(relativeExe))
            {
                string exe = Path.Combine(versionFolder, relativeExe);
                if (File.Exists(exe))
                {
                    component.Executable = exe;
                }
            }

            _components.Add(component);
        }

        /// <summary>
        /// Which version folder is in use. FUN_1000ab430 (0x1000ab430) is the original's answer and
        /// it looks in two places, in this order:
        ///   1. the ini's Version key for this software (FUN_10009fa90 reads
        ///      [&lt;key&gt;] Version), used only when bin\\&lt;key&gt;\\&lt;that name&gt; still exists
        ///      (FUN_10001e550 at 0x10001e550 is the "is it there" test);
        ///   2. otherwise the folder listing itself, where it walks the entries and keeps the last
        ///      one that exists.
        /// The rebuild keeps the second half's spirit but picks the highest dotted version rather
        /// than the alphabetically last name, so php-8.4.20 beats php-8.10.0 — a deliberate
        /// difference, written down in MECHANICS.md §33.
        /// </summary>
        private string PickVersionFolder(string baseFolder, string key, IList<string> versions)
        {
            if (versions.Count == 0)
            {
                // no version folder: bin\git, bin\composer
                return baseFolder;
            }

            // 1. the ini's [<key>] Version, but only while that folder is still there
            //    (FUN_10009fa90 reads it, FUN_10001e550 tests it)
            string wanted = Settings.Get(key, "Version", string.Empty);
            if (!string.IsNullOrEmpty(wanted))
            {
                string found = versions.FirstOrDefault(v =>
                    string.Equals(v, wanted, StringComparison.OrdinalIgnoreCase));
                if (found != null)
                {
                    return Path.Combine(baseFolder, found);
                }
            }

            // 2. otherwise the newest of the installed folders. The original keeps the last entry
            //    its listing hands back; the rebuild sorts the ones whose name carries a dotted
            //    version number instead, so php-8.4.20 beats php-8.10.0 (a folder such as
            //    bin\git\mingw64 has no number in its name and is never picked).
            string best = null;
            string bestKey = null;
            foreach (string name in versions)
            {
                if (!Regex.IsMatch(name, @"\d+\.\d+"))
                {
                    continue;
                }

                string sortKey = VersionSortKey(name);
                if (bestKey == null || string.CompareOrdinal(sortKey, bestKey) > 0)
                {
                    bestKey = sortKey;
                    best = name;
                }
            }

            return best == null ? OnlyOne(baseFolder, versions) : Path.Combine(baseFolder, best);
        }

        /// <summary>
        /// A listing with exactly one folder and no number in its name — bin\nodejs\node-v22 — is
        /// that folder: the original takes its listing's last entry, which is the same thing when
        /// there is only one. bin\git, whose "versions" are Git's own bin/cmd/etc, is left alone
        /// because it never has exactly one.
        /// </summary>
        private static string OnlyOne(string baseFolder, IList<string> versions)
        {
            return versions.Count == 1 ? Path.Combine(baseFolder, versions[0]) : baseFolder;
        }

        /// <summary>"php-8.4.20-..." -> "00008.00004.00020" so plain text order is version order.</summary>
        private static string VersionSortKey(string folderName)
        {
            int[] key = VersionKey(folderName);
            if (key.Length == 0)
            {
                return string.Empty;
            }

            return string.Join(".", key.Select(part => part.ToString("D5")));
        }

        private static int[] VersionKey(string folderName)
        {
            Match match = Regex.Match(folderName, @"\d+\.\d+(\.\d+)*");
            if (!match.Success)
            {
                match = Regex.Match(folderName, @"\d+");
            }

            if (!match.Success)
            {
                return new int[0];
            }

            return match.Value.Split('.').Select(int.Parse).ToArray();
        }

        /// <summary>php-8.4.20-Win32-vs17-x64 -> 8.4.20 ; redis-x64-5.0.14.1 -> 5.0.14.1</summary>
        public static string VersionOf(string prefix, string folderName)
        {
            if (string.IsNullOrEmpty(folderName))
            {
                return string.Empty;
            }

            string rest = folderName;
            if (!string.IsNullOrEmpty(prefix) &&
                rest.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                rest = rest.Substring(prefix.Length);
            }

            // Prefer a dotted version ("5.0.14.1", never the "64" of "x64"), fall back to plain digits.
            Match match = Regex.Match(rest, @"\d+\.\d+(\.\d+)*");
            if (match.Success)
            {
                return match.Value;
            }

            match = Regex.Match(rest, @"\d+");
            return match.Success ? match.Value : string.Empty;
        }
    }
}
