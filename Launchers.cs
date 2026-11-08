using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Laragon
{
    /// <summary>
    /// What the bottom buttons of the main window really start, written from the decompiled
    /// original (see MECHANICS.md §22):
    ///   Terminal  FUN_10004e860 (0x10004e860) → FUN_10004e370 (0x10004e370)
    ///   Database  FUN_10004db40 (0x10004db40) → FUN_10009cb60 (0x10009cb60)
    ///   Root      FUN_10004ddc0 (0x10004ddc0) → FUN_10006a8c0 (0x10006a8c0)
    /// Nothing here runs while --screenshot is used: it only runs when a button is pressed.
    /// </summary>
    public partial class MainForm
    {
        private const int RestoreWindow = 9;

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr window);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr window, int command);

        // ------------------------------------------------------------------ Terminal (Cmder)

        /// <summary>
        /// The Terminal button. The original first looks for a Cmder window that is already
        /// open and brings it forward (FUN_10009c5f0/FUN_10009c520 in FUN_10004e860); otherwise
        /// it starts Cmder in the document root: "…\bin\cmder\Cmder.exe" when that and
        /// "bin\cmder\vendor\conemu-maximus5\ConEmu.exe" are both there, else
        /// "…\bin\cmder\cmder.bat" with the folder as its argument. When neither exists it
        /// shows "Oops! Missing file:" and the path it wanted (0x100394078).
        /// </summary>
        private void LaunchTerminal()
        {
            if (FocusCmderWindow())
            {
                return;
            }

            string home = Path.Combine(_stack.Bin, "cmder");
            string cmderExe = Path.Combine(home, "Cmder.exe");
            string batch = Path.Combine(home, "cmder.bat");
            string folder = TerminalFolder();

            // FUN_10004e370: the program comes first, then one argument, the folder to open in.
            if (File.Exists(cmderExe))
            {
                StartProcess(cmderExe, "\"" + folder + "\"", folder, true);
                return;
            }

            if (File.Exists(batch))
            {
                StartProcess(batch, "\"" + folder + "\"", folder, true);
                return;
            }

            ShowNotification(Language.T("Oops! Missing file:") + Environment.NewLine + cmderExe);
        }

        /// <summary>
        /// The folder Cmder opens in. The original asks FUN_10008fc20 (0x10008fc20) for the
        /// document root of the running web server, and only falls back to "&lt;root&gt;\www".
        /// </summary>
        private string TerminalFolder()
        {
            string folder = _stack.Www;
            if (!string.IsNullOrEmpty(folder) && Directory.Exists(folder))
            {
                return folder;
            }

            return _stack.Root;
        }

        /// <summary>Brings an already-running Cmder/ConEmu window forward, like FUN_10009c520.</summary>
        private static bool FocusCmderWindow()
        {
            string[] names = { "Cmder", "ConEmu", "ConEmu64" };

            foreach (string name in names)
            {
                try
                {
                    foreach (Process process in Process.GetProcessesByName(name))
                    {
                        try
                        {
                            IntPtr handle = process.MainWindowHandle;
                            if (handle == IntPtr.Zero)
                            {
                                continue;
                            }

                            ShowWindow(handle, RestoreWindow);
                            SetForegroundWindow(handle);
                            return true;
                        }
                        finally
                        {
                            process.Dispose();
                        }
                    }
                }
                catch
                {
                }
            }

            return false;
        }

        // ------------------------------------------------------------------ Database

        /// <summary>
        /// The Database button. The original (FUN_10004db40) opens the database admin page that
        /// is actually installed: "&lt;root&gt;\etc\apps\phpmyadmin" gives "/phpmyadmin/", and
        /// Adminer gives "/adminer/" (FUN_10009cb60). It needs the web server and MySQL, so it
        /// starts the stack first when they are only asleep, and says
        /// "You must enable Apache/Nginx and MySQL services in Menu > Preferences > Services and Ports!"
        /// when one of them is switched off. With no admin page installed it opens the manual.
        /// </summary>
        private void OpenDatabasePage()
        {
            string suffix = AdminPage();

            Component apache = _stack.Find("apache");
            Component nginx = _stack.Find("nginx");
            Component mysql = _stack.Find("mysql");

            bool webEnabled = (apache != null && apache.Installed) || (nginx != null && nginx.Installed);
            bool dbEnabled = mysql != null && mysql.Installed;

            if (!webEnabled || !dbEnabled)
            {
                ShowNotification(Language.T(
                    "You must enable Apache/Nginx and MySQL services in Menu > Preferences > Services and Ports!"));
                return;
            }

            bool webUp = (apache != null && apache.IsRunning) || (nginx != null && nginx.IsRunning);
            if (!webUp || (mysql != null && !mysql.IsRunning))
            {
                StartAll();
            }

            if (suffix == null)
            {
                OpenUrl("https://laragon.org/docs/operations");
                return;
            }

            OpenUrl(SiteUrl() + suffix);
        }

        /// <summary>The admin page the install really has, as FUN_10009cb60 picks it.</summary>
        private string AdminPage()
        {
            if (Directory.Exists(Path.Combine(_stack.Root, @"etc\apps\phpmyadmin")))
            {
                return "phpmyadmin/";
            }

            if (Directory.Exists(Path.Combine(_stack.Root, @"etc\apps\adminer")))
            {
                return "adminer/";
            }

            return null;
        }

        // ------------------------------------------------------------------ Root

        /// <summary>
        /// The Root button. It opens a folder in Explorer and complains with "No folder!" when
        /// there is nothing to open (FUN_10006a8c0 at 0x10006a8c0, reached from FUN_10004ddc0).
        /// The folder is the document root the whole window is about; when that is missing the
        /// install folder itself is opened.
        /// </summary>
        private void OpenRootFolder()
        {
            string folder = _stack.Www;
            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
            {
                folder = _stack.Root;
            }

            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
            {
                ShowNotification(Language.T("No folder!") + " " + folder);
                return;
            }

            StartProcess("explorer.exe", "\"" + folder + "\"", _stack.Root, false);
        }
    }
}
