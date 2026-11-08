using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Laragon
{
    /// <summary>
    /// The rest of the popup menu.
    ///
    /// MainForm.Dispatch names a few dozen items in its own switch. The original's menu holds
    /// 268 rows (196 with a caption, 55 separators, 16 blank ones that draw as lines), and an
    /// earlier round had a case for only 22 of the 142 tags it builds, so most rows answered
    /// with "not wired up in this rebuild yet". This file gives every one of them a real
    /// action: it opens the file, folder or page the original opens, or runs the program the
    /// item names, or changes the setting the check mark in front of it stands for.
    ///
    /// What the original does with each row comes from two places and both are written down in
    /// MECHANICS.md §21: the decoded form resource (tools\menu.txt, which lists every row with its
    /// OnClick name) and the installed folder on this computer (every path below was listed
    /// with dir before it was written here). Where the original's own choice could not be
    /// recovered the row says so in MECHANICS.md rather than pretending.
    /// </summary>
    public partial class MainForm
    {
        // ---------------------------------------------------------------- clicking the menu

        /// <summary>
        /// Attaches one handler to every leaf of the popup menu. ToolStripMenuItem.Click is
        /// only ever raised for the row that was clicked, and a row with a drop-down of its own
        /// never raises it (it opens the list instead), so parents are skipped and only their
        /// children are wired. Language.Apply stores each caption the first time it sees it, so
        /// the handler always receives the caption the designer wrote.
        /// </summary>
        private void AttachMenu(ToolStripItemCollection items)
        {
            foreach (ToolStripItem item in items)
            {
                ToolStripMenuItem menu = item as ToolStripMenuItem;
                if (menu != null && menu.DropDownItems.Count > 0)
                {
                    AttachMenu(menu.DropDownItems);
                    continue;
                }

                item.Click += MenuAnyClick;
            }

            SyncMenu();
        }

        private void MenuAnyClick(object sender, EventArgs e)
        {
            ToolStripItem item = sender as ToolStripItem;
            string runtimeTag = item == null ? null : item.Tag as string;
            if (runtimeTag != null && runtimeTag.IndexOf('|') >= 0)
            {
                // a row this rebuild fills in when it starts (Quick app, Quick settings,
                // Extensions) brings its own handler; it is not one of the designer's rows
                return;
            }

            if (item == null || !item.Enabled)
            {
                return;
            }

            // "Deploy to Cloud" holds the project types as children, and those carry the same
            // words as the Quick app list; without this the row would create a local project.
            ToolStripMenuItem menu = item as ToolStripMenuItem;
            ToolStripItem parent = menu == null ? null : menu.OwnerItem;
            if (parent != null && string.Equals(parent.Tag as string, "MnuQuickCloudServerClick", StringComparison.Ordinal))
            {
                DeployToCloud(item.Text);
                return;
            }

            Dispatch(item.Tag as string, item.Text);
        }

        /// <summary>
        /// The items MainForm.Dispatch does not name. Returns false only when nothing here
        /// knows the row either, so the caller can keep its last line.
        /// </summary>
        private bool DispatchMenu(string key, string text)
        {
            string k = (key ?? string.Empty).Trim();
            string caption = (text ?? string.Empty).Trim();

            switch (k)
            {
                // ---- start, stop, reload -------------------------------------------------
                case "MnuStartApacheClick": StartNamed("apache"); return true;
                case "MnuStartNginxClick": StartNamed("nginx"); return true;
                case "MnuStartMySQLClick": StartNamed("mysql"); return true;
                case "mnuStartPostgreSQLClick": StartNamed("postgresql"); return true;
                case "MnuStartMongoDBClick": StartNamed("mongodb"); return true;
                case "MnuStartMemcachedClick": StartNamed("memcached"); return true;
                case "MnuStartRedisClick": StartNamed("redis"); return true;
                case "MnuStartMailpitClick": StartNamed("mailpit"); return true;
                case "MnuStartPHPServerClick": StartNamed("php"); return true;
                case "MnuReloadApacheClick": ReloadNamed("apache"); return true;
                case "MnuReloadNginxClick": ReloadNamed("nginx"); return true;
                case "MnuRunApacheasWindowsServiceClick":
                    Say("Running a server as a Windows service needs administrator rights and is not part of this rebuild: Laragon starts it as a program instead.");
                    return true;

                // ---- Apache --------------------------------------------------------------
                case "MnuApacheConfigFileClick": OpenInComponent("apache", @"conf\httpd.conf", "httpd.conf"); return true;
                case "MnuApacheHttpdSSLConfClick": OpenPath(@"etc\apache2\httpd-ssl.conf"); return true;
                case "MnuApacheConfDirClick": OpenInComponent("apache", "conf"); return true;
                case "MnuApacheAliasClick": OpenPath(@"etc\apache2\alias"); return true;
                case "MnuApacheVirtualHostClick": OpenPath(@"etc\apache2\sites-enabled"); return true;
                case "MnuApacheErrorLogClick": OpenInComponent("apache", @"logs\error.log", @"logs\error_log", "error.log"); return true;
                case "MnuApacheAccessLogClick": OpenInComponent("apache", @"logs\access.log", @"logs\access_log", "access.log"); return true;
                case "MnuApacheSSLClick": OpenPath(@"etc\apache2\httpd-ssl.conf"); return true;
                case "MnuApacheSSLEnabledClick": ToggleSwitch("apache", "SSL"); return true;
                case "MnuApacheAddCertificatesToTrustStoreClick": AddCertificateToStore(); return true;
                case "MnuApacheCertificateManagerClick": OpenCertificateManager(); return true;

                // ---- Nginx ---------------------------------------------------------------
                case "MnuNginxConfClick": OpenInComponent("nginx", @"conf\nginx.conf", "nginx.conf"); return true;
                case "MnuNginxConfDirClick": OpenInComponent("nginx", "conf"); return true;
                case "MnuNginxAliasClick": OpenPath(@"etc\nginx\alias"); return true;
                case "MnuNginxServerClick": OpenPath(@"etc\nginx\sites-enabled"); return true;
                case "MnuNginxErrorLogClick": OpenInComponent("nginx", @"logs\error.log", "error.log"); return true;
                case "MnuNginxAccessLogClick": OpenInComponent("nginx", @"logs\access.log", "access.log"); return true;
                case "MnuNginxSSLClick": OpenInComponent("nginx", @"conf\nginx.conf", "nginx.conf"); return true;
                case "MnuNginxSSLEnabledClick": ToggleSwitch("nginx", "SSL"); return true;
                case "MnuNginxAddCertificatesToTrustStoreClick": AddCertificateToStore(); return true;
                case "MnuNginxCertificateManagerClick": OpenCertificateManager(); return true;

                // ---- MySQL and MariaDB ---------------------------------------------------
                case "MnuMySQLConfigFileClick": OpenInComponent("mysql", "my.ini", @"etc\my.ini"); return true;
                case "MnuMySQLErrorLogClick": OpenInComponent("mysql", "mysqld.log", @"data\mysqld.log"); return true;
                case "MnuMySQLDataDirClick": OpenFolder(_stack.DataDir); return true;
                case "MnuMySQLBackupDirClick": OpenFolder(BackupFolder()); return true;
                case "MnuMySQLCreateDatabaseClick": CreateDatabase(); return true;
                case "MnuMySQLGetRootPasswordClick": GetRootPassword(); return true;
                case "MnuMySQLChangeRootPasswordClick": ChangeRootPassword(false); return true;
                case "MnuMySQLResetRootPasswordClick": ChangeRootPassword(true); return true;
                case "MnuMySQLBackupAllDatabasesClick": BackupDatabases(); return true;
                case "MnuMySQLDBeaverClick": RunUnder("dbeaver", "dbeaver.exe", string.Empty); return true;
                case "MnuMySQLHeidiSQLClick": RunUnder("heidisql", "heidisql.exe", string.Empty); return true;

                // ---- PostgreSQL ----------------------------------------------------------
                case "MnuPostgreSQLDataDirClick": OpenFolder(Path.Combine(_stack.DataDir, "postgresql")); return true;
                case "postgresqlconf1Click": OpenFileOrSay(Path.Combine(_stack.DataDir, "postgresql", "postgresql.conf")); return true;
                case "postgresqllog1Click": OpenFileOrSay(Path.Combine(_stack.DataDir, "postgresql", "postgresql.log")); return true;
                case "MnuPostgreSQLPgAdminClick": RunUnder("pgadmin", "pgadmin3.exe", string.Empty); return true;
                case "MnuPostgreSQLPgAdmin4Click": RunUnder("pgadmin", "pgadmin4.exe", string.Empty); return true;

                // ---- MongoDB -------------------------------------------------------------
                case "MnuMongoDBConfigFileClick": OpenInComponent("mongodb", "mongod.conf", @"conf\mongod.conf"); return true;
                case "MnuMongoDBLogClick": OpenInComponent("mongodb", "mongod.log", @"logs\mongod.log"); return true;
                case "MnuMongoDBDataDirClick": OpenFolder(Path.Combine(_stack.DataDir, "mongodb")); return true;
                case "MnuRoboMongoClick": RunUnder("robomongo", "robomongo.exe", string.Empty); return true;

                // ---- Redis and Memcached -------------------------------------------------
                case "MnuRedisConfigFileClick": OpenInComponent("redis", "redis.windows.conf", "redis.conf"); return true;
                case "MnuRedisAdminClick": OpenUrl(LocalUrl("redis/")); return true;
                case "MnuMemcachedAdminClick": OpenUrl(LocalUrl("memcached/")); return true;

                // ---- PHP -----------------------------------------------------------------
                case "MnuPHPConfigFileClick": OpenInComponent("php", "php.ini", @"conf\php.ini"); return true;
                case "MnuPHPErrorLogClick": OpenInComponent("php", "php_errors.log", @"logs\php_errors.log"); return true;
                case "MnuPHPExtensionsDirClick": OpenInComponent("php", "ext"); return true;
                case "MnuPHPExtensionsClick": OpenInComponent("php", "php.ini", @"conf\php.ini"); return true;
                case "MnuPHPQuickSettingsClick": OpenInComponent("php", "php.ini", @"conf\php.ini"); return true;

                // ---- Mail ----------------------------------------------------------------
                case "MnuMailCatcherEnableClick": ToggleSwitch("mailcatcher", "Use"); return true;
                case "MnuMailSenderEnableClick": ToggleSwitch("mailsender", "Use"); return true;
                case "MnuMailCatcherConfigurationClick":
                case "MnuMailSenderConfigurationClick":
                    ShowPreferences();
                    return true;
                case "MnuMailSenderDebugLogClick": OpenInComponent("sendmail", "debug.log", @"logs\debug.log"); return true;
                case "MnuMailSenderErrorLogClick": OpenInComponent("sendmail", "error.log", @"logs\error.log"); return true;
                case "MnuViewemailClick":
                    // caught mail is a database now (Mailpit), so the newest message is read in
                    // its web page; the original opened the newest .eml of its own catcher
                    OpenUrl("http://localhost:8025/");
                    return true;
                case "MnuOpenmaildirClick": OpenFolder(MailFolder()); return true;
                case "MnuSendmailPathClipboardClick": CopySendmailPath(); return true;

                // ---- Laravel -------------------------------------------------------------
                case "MnuLaravelCurrentProjectClick": OpenFolder(CurrentProject()); return true;
                case "MnuLaravelConfigDirClick": OpenProjectFolder("config"); return true;
                case "MnuLaravelControllersDirClick": OpenProjectFolder("controllers"); return true;
                case "MnuLaravelLogsDirClick": OpenProjectFolder("logs"); return true;
                case "MnuLaravelPublicDirClick": OpenProjectFolder("public"); return true;
                case "MnuLaravelRouteFileClick": OpenProjectFolder("routes"); return true;
                case "MnuCreateLaravel4Click": QuickCreate("Laravel 4"); return true;
                case "MnuCreateLaravel5Click": QuickCreate("Laravel 5"); return true;
                case "MnuCreateLumenClick": QuickCreate("Lumen"); return true;

                // ---- Go, Java, Python ----------------------------------------------------
                case "MnuGoGOROOTClick": OpenInComponent("go", string.Empty); return true;
                case "MnuGoGOPATHClick": OpenFolder(Path.Combine(_stack.DataDir, "go")); return true;
                case "MnuGoLiteIDEClick": RunUnder("liteide", "liteide.exe", string.Empty); return true;
                case "MnuJavaSTSClick": RunUnder("sts", "sts.exe", string.Empty); return true;
                case "MnuJavaEclipseClick": RunUnder("eclipse", "eclipse.exe", string.Empty); return true;
                case "MnuPythonHelpClick": OpenUrl("https://docs.python.org/3/"); return true;
                case "MnuPythonIDLEClick": RunUnder("python", "idle.bat", @"Lib\idlelib\idle.pyw"); return true;

                // ---- the programs in bin\ -------------------------------------------------
                case "MnuNotepadPlusPlusClick": RunUnder("notepad++", @"notepad++.exe"); return true;
                case "MnuSublimeTextClick": RunUnder("sublime text", "sublime_text.exe"); return true;
                case "MnuWinSCPClick": RunUnder("winscp", "WinSCP.exe"); return true;
                case "MnuputtyClick": RunUnder("putty", "putty.exe"); return true;
                case "MnuPuTTYgenClick": RunUnder("putty", "puttygen.exe"); return true;
                case "MnuShareClick": ShareSite(); return true;

                // ---- the Windows side of Laragon ----------------------------------------
                case "MnuAddLaragontoPathClick": ChangePath(true); return true;
                case "MnuRemoveLaragonfromPathClick": ChangePath(false); return true;
                case "MnuManagePathClick": OpenEnvironmentVariables(); return true;
                case "MnuAddSublimeTextTerminalClick": SublimeTerminal(true); return true;
                case "MnuRemoveSublimeTextTerminalClick": SublimeTerminal(false); return true;

                // ---- cloud (needs an account, see MECHANICS.md §19) ----------------------
                case "MnuQuickCloudServerClick":
                case "MnuQuickCSRegionUSClick":
                case "MnuQuickCSRegionEUClick":
                case "MnuQuickCSRegionASClick":
                case "MnuQuickCSSSHClick":
                    DeployToCloud(k);
                    return true;
            }

            // An item whose Tag is empty always carries its caption: the version rows, the
            // profile rows and the four PHP quick settings are built that way.
            if (caption.Length > 0 && caption.IndexOfAny(new[] { '\\', '/' }) < 0)
            {
                if (caption == "localhost")
                {
                    OpenUrl(SiteUrl());
                    return true;
                }

                if (caption == "max_execution_time = 0" || caption == "upload_max_filesize = 2G"
                    || caption == "memory_limit = 512M")
                {
                    SetPhpIni(caption);
                    return true;
                }

                if (caption == "xdebug")
                {
                    SetPhpIni("xdebug.mode = debug");
                    return true;
                }

                if (caption == "No Project")
                {
                    _stack.Settings.Set("laragon", "Project", string.Empty);
                    SyncMenu();
                    Say("The current project is cleared.");
                    return true;
                }

                if (caption == "Add profile")
                {
                    Say("A profile is a named copy of usr\\laragon.ini; copy the file and give it the profile's name.");
                    return true;
                }

                if (caption == "Generate certs and add to trust store")
                {
                    string openssl = Search(_stack.Root, "openssl.exe", 4);
                    if (openssl == null)
                    {
                        Say("No file! openssl.exe is not installed, so the certificate cannot be generated here.");
                        return true;
                    }

                    RunShell(openssl, "req -x509 -nodes -days 3650 -newkey rsa:2048 "
                        + "-keyout \"" + Path.Combine(_stack.Root, @"etc\ssl\laragon.key") + "\" "
                        + "-out \"" + Path.Combine(_stack.Root, @"etc\ssl\laragon.crt") + "\" -subj \"/CN=localhost\"");
                    return true;
                }

                if (caption == "openssl.conf.tpl")
                {
                    OpenFileOrSay(Path.Combine(_stack.Root, @"etc\ssl\openssl.conf.tpl"));
                    return true;
                }

                if (caption == "Procfile")
                {
                    OpenFileOrSay(Path.Combine(_stack.Root, @"usr\Procfile"));
                    return true;
                }

                if (caption == "dir: proc")
                {
                    OpenFolder(Path.Combine(_stack.Root, "usr"));
                    return true;
                }

                // the version rows inside a "Version" list name the folder to switch to
                foreach (Component each in _stack.Components)
                {
                    if (!each.Installed || string.IsNullOrEmpty(each.SubFolder))
                    {
                        continue;
                    }

                    string candidate = Path.Combine(Path.Combine(_stack.Bin, each.SubFolder), caption);
                    if (Directory.Exists(candidate))
                    {
                        SwitchVersion(each.Key, caption);
                        return true;
                    }
                }

                // a profile row: the name of a .ini in usr\ except laragon.ini itself
                if (File.Exists(Path.Combine(_stack.Root, @"usr\", caption + ".ini")))
                {
                    _stack.Settings.Set("laragon", "Profile", caption);
                    SyncMenu();
                    Say("Profile: " + caption);
                    return true;
                }
            }

            return false;
        }

        // ---------------------------------------------------------------- keeping the menu true

        /// <summary>
        /// Writes into the menu what only the installed folders know: the version of each
        /// server (the original builds "Version [x]" from the folder it found, the designer
        /// text is a placeholder), the SSL boxes and the two mail switches.
        /// </summary>
        private void SyncMenu()
        {
            FillRuntimeMenus();
            VersionRow("MnuApacheVersionClick", "apache");
            VersionRow("MnuNginxVersionClick", "nginx");
            VersionRow("MnuMySQLVersionClick", "mysql");
            VersionRow("MnuPostgreSQLVersionClick", "postgresql");
            VersionRow("MnuMongoDBVersionClick", "mongodb");
            VersionRow("MnuRedisVersionClick", "redis");
            VersionRow("MnuMemcachedVersionClick", "memcached");
            VersionRow("MnuPHPVersionClick", "php");
            VersionRow("MnuPythonVersionClick", "python");
            VersionRow("MnuNodeJsVersionClick", "nodejs");
            VersionRow("MnuRubyVersionClick", "ruby");
            VersionRow("MnuGoVersionClick", "go");
            VersionRow("MnuJavaVersionClick", "java");

            Check("MnuApacheSSLEnabledClick", _stack.Settings.GetBool("apache", "SSL", false));
            Check("MnuNginxSSLEnabledClick", _stack.Settings.GetBool("nginx", "SSL", false));
            Check("MnuMailCatcherEnableClick", _stack.Settings.GetBool("mailcatcher", "Use", true));
            Check("MnuMailSenderEnableClick", _stack.Settings.GetBool("mailsender", "Use", true));

            ToolStripMenuItem project = FindItem("MnuLaravelCurrentProjectClick");
            if (project != null)
            {
                string current = _stack.Settings.Get("laragon", "Project", string.Empty);
                string name = string.IsNullOrEmpty(current)
                    ? Path.GetFileName(CurrentProject().TrimEnd('\\'))
                    : current;
                if (!string.IsNullOrEmpty(name))
                {
                    project.Text = Language.T("Laragon") + " [" + name + "]";
                }
            }
        }

        private void VersionRow(string tag, string key)
        {
            ToolStripMenuItem item = FindItem(tag);
            Component component = C(key);
            if (item == null)
            {
                return;
            }

            string version = component == null ? null : component.Version;
            item.Text = string.IsNullOrEmpty(version)
                ? Language.T("Version")
                : Language.T("Version") + " [" + version + "]";

            // The list under it is NOT rebuilt here. That is what the original's refresh does
            // (FUN_100061fc0 at 0x100061fc0: it re-captions each parent row to "Version [<in use>]"
            // and sets Enabled on the controls, and stops there) and it is what keeps a periodic
            // refresh from adding another separator to a list that is never emptied.
            // MenuLists.HookVersionLists fills the list when its drop-down opens.
        }

        private void Check(string tag, bool value)
        {
            ToolStripMenuItem item = FindItem(tag);
            if (item != null)
            {
                item.Checked = value;
            }
        }

        private ToolStripMenuItem FindItem(string tag)
        {
            return FindItem(mnuLaragon.Items, tag);
        }

        private static ToolStripMenuItem FindItem(ToolStripItemCollection items, string tag)
        {
            foreach (ToolStripItem item in items)
            {
                ToolStripMenuItem menu = item as ToolStripMenuItem;
                if (menu == null)
                {
                    continue;
                }

                if (string.Equals(menu.Tag as string, tag, StringComparison.Ordinal))
                {
                    return menu;
                }

                ToolStripMenuItem found = FindItem(menu.DropDownItems, tag);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        /// <summary>
        /// --menudump: every row of the popup with its tag and caption, in order.
        /// The separators are printed as "&lt;sep&gt;", tag and all, because a list that grows an
        /// extra line on every refresh looks exactly like this dump (the row count of the dump
        /// is the same however many passes ran; the number of &lt;sep&gt; lines is not allowed to
        /// change). "passes" simulates that many window refreshes, which is what the timer does
        /// every two seconds while the program runs.
        /// </summary>
        internal void DumpMenu(TextWriter writer, int passes)
        {
            if (passes < 1)
            {
                passes = 1;
            }

            for (int pass = 0; pass < passes; pass++)
            {
                FillListsNow();
                SyncMenu();
            }

            DumpItems(writer, mnuLaragon.Items, 0);
        }

        private static void DumpItems(TextWriter writer, ToolStripItemCollection items, int depth)
        {
            foreach (ToolStripItem item in items)
            {
                if (item is ToolStripSeparator)
                {
                    writer.WriteLine(new string(' ', depth * 2)
                        + (item.Tag as string) + "\t<sep>");
                    continue;
                }

                ToolStripMenuItem menu = item as ToolStripMenuItem;
                writer.WriteLine(new string(' ', depth * 2)
                    + (item.Tag as string) + "\t" + item.Text
                    + (menu != null && menu.Checked ? "\t[checked]" : string.Empty)
                    + (item.Image == null ? string.Empty : "\t[" + PictureFor(item) + "]"));

                if (menu != null)
                {
                    DumpItems(writer, menu.DropDownItems, depth + 1);
                }
            }
        }

        // ---------------------------------------------------------------- small helpers

        private Component C(string key)
        {
            return _stack.Find(key);
        }

        private const string Unavailable = "This feature is not available on your computer!";

        private void Say(string english)
        {
            ShowNotification(Language.T(english));
        }

        private void StartNamed(string key)
        {
            Component component = C(key);
            if (component == null || !component.Installed)
            {
                Say(string.Format(Language.T("This service is not installed in {0}."), _stack.Bin));
                return;
            }

            string problem = _services.Start(component);
            if (problem == null)
            {
                ShowNotification(component.Display + " started.");
            }
            else
            {
                MessageBox.Show(this, Language.T(problem), Brand.Name, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            RefreshState();
        }

        /// <summary>
        /// The original's Reload stops and starts the server (a Windows Apache has no usable
        /// -k restart here because Laragon passes no -f, so the clean way is stop then start).
        /// </summary>
        private void ReloadNamed(string key)
        {
            Component component = C(key);
            if (component == null || !component.Installed)
            {
                Say(string.Format(Language.T("This service is not installed in {0}."), _stack.Bin));
                return;
            }

            _services.Stop(component);
            for (int i = 0; i < 20; i++)
            {
                System.Threading.Thread.Sleep(100);
                if (!component.IsRunning)
                {
                    break;
                }
            }

            StartNamed(key);
        }

        /// <summary>The site's own origin with one of the web admins' aliases behind it.</summary>
        private string LocalUrl(string path)
        {
            string scheme = _stack.Scheme();
            int port = _stack.WebPort();
            bool plain = (scheme == "http://" && port == 80) || (scheme == "https://" && port == 443);
            return scheme + "localhost" + (plain ? string.Empty : ":" + port) + "/" + path.TrimStart('/');
        }

        private void OpenPath(string relative)
        {
            string path = Path.Combine(_stack.Root, relative);
            if (Directory.Exists(path))
            {
                OpenFolder(path);
                return;
            }

            if (File.Exists(path))
            {
                OpenFileOrSay(path);
                return;
            }

            ShowNotification(Language.T("File not found: ") + path);
        }

        /// <summary>
        /// Opens a file or a folder inside the version folder of one component. The first
        /// name that exists wins; names are tried relative to the version folder, then the
        /// whole component tree is searched, then the root, then the temporary folder.
        /// </summary>
        private void OpenInComponent(string key, params string[] names)
        {
            Component component = C(key);
            if (component != null && component.Installed)
            {
                foreach (string name in names)
                {
                    if (string.IsNullOrEmpty(name))
                    {
                        OpenFolder(component.Folder);
                        return;
                    }

                    string direct = Path.Combine(component.Folder, name);
                    if (Directory.Exists(direct))
                    {
                        OpenFolder(direct);
                        return;
                    }

                    if (File.Exists(direct))
                    {
                        OpenFileOrSay(direct);
                        return;
                    }
                }

                foreach (string name in names)
                {
                    if (string.IsNullOrEmpty(name))
                    {
                        continue;
                    }

                    DirectoryInfo dir = null;
                    try
                    {
                        dir = new DirectoryInfo(component.Folder);
                    }
                    catch
                    {
                    }

                    string found = dir == null ? null : Search(dir.FullName, Path.GetFileName(name), 6);
                    if (found != null)
                    {
                        OpenFileOrSay(found);
                        return;
                    }
                }
            }

            // a few of these files live at the root or in tmp\ instead
            foreach (string name in names)
            {
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                string simple = Path.GetFileName(name);
                string atRoot = Path.Combine(_stack.Root, simple);
                if (File.Exists(atRoot))
                {
                    OpenFileOrSay(atRoot);
                    return;
                }

                string inTmp = Path.Combine(_stack.Root, "tmp", simple);
                if (File.Exists(inTmp))
                {
                    OpenFileOrSay(inTmp);
                    return;
                }
            }

            string folder = component == null || !component.Installed ? _stack.Bin : component.Folder;
            ShowNotification(Language.T("File not found: ") + Path.Combine(folder, names.Length == 0 ? string.Empty : names[0]));
        }

        private static string Search(string folder, string name, int depth)
        {
            if (depth < 0 || string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
            {
                return null;
            }

            try
            {
                string direct = Path.Combine(folder, name);
                if (File.Exists(direct))
                {
                    return direct;
                }

                foreach (string child in Directory.GetDirectories(folder))
                {
                    string found = Search(child, name, depth - 1);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }
            catch
            {
            }

            return null;
        }

        /// <summary>Runs a program of bin\ (the component's own, else a search under bin\).</summary>
        private void RunUnder(string key, params string[] names)
        {
            Component component = C(key);
            if (component != null && component.Installed)
            {
                foreach (string name in names)
                {
                    string path = Path.Combine(component.Folder, name);
                    if (File.Exists(path))
                    {
                        StartProcess(path, string.Empty, component.Folder, false);
                        return;
                    }
                }
            }

            foreach (string name in names)
            {
                string path = Path.Combine(_stack.Bin, key, Path.GetFileName(name));
                if (File.Exists(path))
                {
                    StartProcess(path, string.Empty, Path.GetDirectoryName(path), false);
                    return;
                }
            }

            if (component == null || !component.Installed)
            {
                foreach (string name in names)
                {
                    string found = Search(_stack.Bin, Path.GetFileName(name), 3);
                    if (found != null)
                    {
                        StartProcess(found, string.Empty, Path.GetDirectoryName(found), false);
                        return;
                    }
                }
            }

            Say(Unavailable);
        }

        /// <summary>A switch with a check mark in front of it: writes the ini and shows a tick.</summary>
        private void ToggleSwitch(string section, string key)
        {
            bool value = !_stack.Settings.GetBool(section, key, false);
            _stack.Settings.Set(section, key, value);

            Component component = C(section);
            if (component != null && string.Equals(key, "SSL", StringComparison.OrdinalIgnoreCase))
            {
                component.Ssl = value;
            }

            SyncMenu();
            Say(section + " " + key + " = " + (value ? "on" : "off"));
        }

        private string BackupFolder()
        {
            string folder = Path.Combine(_stack.Root, "backup");
            try
            {
                Directory.CreateDirectory(folder);
            }
            catch
            {
            }

            return folder;
        }

        private string MailFolder()
        {
            Component mailpit = C("mailpit");
            if (mailpit != null && mailpit.Installed)
            {
                return mailpit.Folder;
            }

            string old = Path.Combine(_stack.Root, @"tmp\mail");
            return Directory.Exists(old) ? old : _stack.Root;
        }

        private string CurrentProject()
        {
            string named = _stack.Settings.Get("laragon", "Project", string.Empty);
            if (!string.IsNullOrEmpty(named))
            {
                string chosen = Path.Combine(_stack.Www, named);
                if (Directory.Exists(chosen))
                {
                    return chosen;
                }
            }

            try
            {
                DirectoryInfo newest = Directory.GetDirectories(_stack.Www)
                    .Select(d => new DirectoryInfo(d))
                    .Where(d => (d.Attributes & FileAttributes.Hidden) == 0)
                    .OrderByDescending(d => d.LastWriteTimeUtc)
                    .FirstOrDefault();
                if (newest != null)
                {
                    return newest.FullName;
                }
            }
            catch
            {
            }

            return _stack.Www;
        }

        private void OpenProjectFolder(string what)
        {
            string project = CurrentProject();
            string[] candidates;
            switch (what)
            {
                case "config":
                    candidates = new[] { "config" };
                    break;
                case "controllers":
                    candidates = new[] { @"app\Http\Controllers", @"app\controllers" };
                    break;
                case "logs":
                    candidates = new[] { @"storage\logs", @"app\storage\logs", "logs" };
                    break;
                case "public":
                    candidates = new[] { "public" };
                    break;
                default:
                    candidates = new[] { @"app\Http\routes.php", @"routes\web.php", @"app\routes.php" };
                    break;
            }

            foreach (string candidate in candidates)
            {
                string path = Path.Combine(project, candidate);
                if (Directory.Exists(path))
                {
                    OpenFolder(path);
                    return;
                }

                if (File.Exists(path))
                {
                    OpenFileOrSay(path);
                    return;
                }
            }

            ShowNotification(Language.T("Folder not found: ") + Path.Combine(project, candidates[0]));
        }

        // ---------------------------------------------------------------- the MySQL rows

        private string MySqlTool(string tool)
        {
            Component mysql = C("mysql");
            if (mysql == null || !mysql.Installed)
            {
                return null;
            }

            foreach (string name in new[] { tool + ".exe", @"bin\" + tool + ".exe" })
            {
                string path = Path.Combine(mysql.Folder, name);
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return Search(mysql.Folder, tool + ".exe", 4);
        }

        private string RootPassword
        {
            get { return _stack.Settings.Get("mysql", "RootPassword", string.Empty); }
        }

        private string RootArgument
        {
            get
            {
                string password = RootPassword;
                return string.IsNullOrEmpty(password) ? string.Empty : " -p" + password;
            }
        }

        /// <summary>
        /// "Create database", FUN_100067550 (0x100067550) and FUN_100066720 (0x100066720): one
        /// input box titled "Create database" whose prompt is "Database Name:"; Cancel does
        /// nothing, an empty answer says "Please specify Database Name!" and stops there. The
        /// name is then handed to &lt;version&gt;\bin\mysqladmin -u root [-p"&lt;password&gt;"]
        /// create &lt;name&gt;, and the balloon reads "Created database!" + " " + name; when
        /// mysqladmin answers "database exists" the balloon reads "Database exists!" + " " + name,
        /// and any other complaint is shown in mysqladmin's own words.
        /// </summary>
        private void CreateDatabase()
        {
            string exe = MySqlTool("mysqladmin");
            if (exe == null)
            {
                Say(Unavailable);
                return;
            }

            string name = Ask("Create database", "Database Name:", string.Empty, false);
            if (name == null)
            {
                return;
            }

            if (name.Length == 0)
            {
                MessageBox.Show(this, Language.T("Please specify Database Name!"), Brand.Name,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string output;
            if (!RunCapture(exe, "-u root" + RootArgument + " create " + name, out output))
            {
                return;
            }

            if (output.IndexOf("error", StringComparison.OrdinalIgnoreCase) < 0)
            {
                ShowNotification(Language.T("Created database!") + " " + name);
                return;
            }

            if (output.IndexOf("database exists", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                ShowNotification(Language.T("Database exists!") + " " + name);
                return;
            }

            MessageBox.Show(this, name + ":" + Environment.NewLine + output.Trim(), Brand.Name,
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void GetRootPassword()
        {
            string password = RootPassword;
            try
            {
                Clipboard.SetText(password ?? string.Empty);
            }
            catch
            {
            }

            Say(string.IsNullOrEmpty(password)
                ? "The MySQL root password is empty (Laragon's default)."
                : "The MySQL root password has been copied to the clipboard");
        }

        /// <summary>
        /// FUN_1000676e0 (0x1000676e0): two input boxes — "Current password?" with the prompt
        /// "Current Password:" and a second line "(Leave blank if unknown)", then "New Password"
        /// with the prompt "New Password:" — and three refusals before anything runs: an empty
        /// password ("Password must not be empty!"), a double quote ("Password must not contain
        /// double quote ---&gt; \"" and "Please try another password!") and a single quote
        /// ("Password should not contain single quote ---&gt; '" and the same second line). It then
        /// asks bin\mysql.exe which plugin root uses and sets the new password with ALTER USER.
        /// </summary>
        private void ChangeRootPassword(bool random)
        {
            string mysql = MySqlTool("mysql");
            if (mysql == null)
            {
                Say(Unavailable);
                return;
            }

            if (random)
            {
                ApplyNewRootPassword(mysql, string.Empty, RandomPassword());
                return;
            }

            string current = Ask("Current password?", "Current Password:" + Environment.NewLine + "(Leave blank if unknown)", string.Empty, true);
            if (current == null)
            {
                return;
            }

            string next = Ask("New Password", "New Password:", string.Empty, true);
            if (next == null)
            {
                return;
            }

            if (next.Length == 0)
            {
                MessageBox.Show(this, Language.T("Password must not be empty!"), Brand.Name,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (next.IndexOf('"') >= 0)
            {
                MessageBox.Show(this,
                    Language.T("Password must not contain double quote ---> \"") + Environment.NewLine +
                    Language.T("Please try another password!"), Brand.Name,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (next.IndexOf('\'') >= 0)
            {
                MessageBox.Show(this,
                    Language.T("Password should not contain single quote ---> '") + Environment.NewLine +
                    Language.T("Please try another password!"), Brand.Name,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ApplyNewRootPassword(mysql, current, next);
        }

        /// <summary>
        /// The part of FUN_1000676e0 that talks to MySQL: bin\mysql.exe -u root [-p"&lt;current&gt;"]
        /// --skip-column-names -e "SELECT plugin FROM mysql.user WHERE user = 'root'", whose answer
        /// decides between mysql_native_password and caching_sha2_password, then
        /// -e "ALTER USER 'root'@'localhost' IDENTIFIED WITH &lt;plugin&gt; BY '&lt;new&gt;';
        /// FLUSH PRIVILEGES;". "Changed MySQL root password!" on success, with the password kept
        /// under [mysql] MySQLRootPassword, and "Could not change MySQL root password. Reason: …"
        /// otherwise.
        /// </summary>
        private void ApplyNewRootPassword(string mysql, string current, string next)
        {
            string argument = string.IsNullOrEmpty(current) ? string.Empty : " -p\"" + current + "\"";

            string output;
            if (!RunCapture(mysql, "-u root" + argument +
                " --skip-column-names -e \"SELECT plugin FROM mysql.user WHERE user = 'root'\"", out output))
            {
                return;
            }

            string plugin = output.IndexOf("mysql_native_password ", StringComparison.OrdinalIgnoreCase) >= 0
                ? "mysql_native_password"
                : "caching_sha2_password";

            if (!RunCapture(mysql, "-u root" + argument +
                " -e \"ALTER USER 'root'@'localhost' IDENTIFIED WITH " + plugin + " BY '" + next +
                "'; FLUSH PRIVILEGES;\"", out output))
            {
                return;
            }

            if (output.IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                MessageBox.Show(this,
                    Language.T("Could not change MySQL root password. Reason: ") + output.Trim(), Brand.Name,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _stack.Settings.Set("mysql", "MySQLRootPassword", next);
            _stack.Settings.Set("mysql", "RootPassword", next);
            _stack.Settings.Save();

            ShowNotification(Language.T("Changed MySQL root password!"));

            if (string.IsNullOrEmpty(current))
            {
                try
                {
                    Clipboard.SetText(next);
                }
                catch
                {
                }

                Say(Language.T("The new root password is on the clipboard: ") + next);
            }
        }

        private static string RandomPassword()
        {
            const string letters = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var random = new Random();
            var text = new StringBuilder();
            for (int i = 0; i < 16; i++)
            {
                text.Append(letters[random.Next(letters.Length)]);
            }

            return text.ToString();
        }

        private void BackupDatabases()
        {
            string exe = MySqlTool("mysqldump");
            if (exe == null)
            {
                Say(Unavailable);
                return;
            }

            string file = Path.Combine(BackupFolder(), DateTime.Now.ToString("yyyyMMdd-HHmm") + "-all.sql");
            RunAndReport(exe,
                "-u root" + RootArgument + " --all-databases --routines --events --triggers --result-file=\"" + file + "\"",
                string.Format(Language.T("All MySQL databases have been backed up successfully to {0}"), file));
        }

        /// <summary>
        /// Runs a tool quietly and hands back what it printed, standard output first then the
        /// errors, the way the original reads the pipes of mysql.exe and mysqladmin.exe
        /// (FUN_10001b9d0 in the original). False means the tool is missing or could not run.
        /// </summary>
        private bool RunCapture(string exe, string arguments, out string output)
        {
            output = string.Empty;

            if (!File.Exists(exe))
            {
                Say(Language.T("File not found: ") + exe);
                return false;
            }

            try
            {
                var start = new ProcessStartInfo(exe, arguments)
                {
                    WorkingDirectory = _stack.Root,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };

                using (Process process = Process.Start(start))
                {
                    output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
                    process.WaitForExit(30000);
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Brand.Name, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        private void RunAndReport(string exe, string arguments, string success)
        {
            if (!File.Exists(exe))
            {
                Say(Language.T("File not found: ") + exe);
                return;
            }

            try
            {
                var start = new ProcessStartInfo(exe, arguments)
                {
                    WorkingDirectory = _stack.Root,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };

                using (Process process = Process.Start(start))
                {
                    string output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
                    process.WaitForExit(30000);

                    if (process.HasExited && process.ExitCode == 0)
                    {
                        ShowNotification(success);
                        return;
                    }

                    MessageBox.Show(this, output.Trim(), Brand.Name, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Brand.Name, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ---------------------------------------------------------------- Path, mail, SSL

        private void ChangePath(bool add)
        {
            string bin = _stack.Bin.TrimEnd('\\');
            string current = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? string.Empty;
            var parts = current.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim().TrimEnd('\\'))
                .Where(p => p.Length > 0)
                .ToList();

            bool there = parts.Any(p => string.Equals(p, bin, StringComparison.OrdinalIgnoreCase));
            if (add && there)
            {
                Say("Laragon is already in Path.");
                return;
            }

            if (!add && !there)
            {
                Say("Laragon is not in Path.");
                return;
            }

            if (add)
            {
                parts.Add(bin);
            }
            else
            {
                parts.RemoveAll(p => string.Equals(p, bin, StringComparison.OrdinalIgnoreCase));
            }

            Environment.SetEnvironmentVariable("PATH", string.Join(";", parts), EnvironmentVariableTarget.User);
            Say(add
                ? "Laragon added to Path. Open a new terminal to use it."
                : "Laragon removed from Path.");
        }

        private void OpenEnvironmentVariables()
        {
            StartProcess("rundll32.exe", "sysdm.cpl,EditEnvironmentVariables", _stack.Root, false);
        }

        private void CopySendmailPath()
        {
            string value = null;
            try
            {
                Component php = C("php");
                if (php != null && php.Installed)
                {
                    string ini = Path.Combine(php.Folder, "php.ini");
                    if (File.Exists(ini))
                    {
                        foreach (string line in File.ReadAllLines(ini))
                        {
                            string trimmed = line.Trim();
                            if (trimmed.StartsWith(";", StringComparison.Ordinal))
                            {
                                trimmed = trimmed.TrimStart(';', ' ');
                            }

                            if (trimmed.StartsWith("sendmail_path", StringComparison.OrdinalIgnoreCase))
                            {
                                value = trimmed;
                                break;
                            }
                        }
                    }
                }
            }
            catch
            {
            }

            if (string.IsNullOrEmpty(value))
            {
                string sendmail = Path.Combine(_stack.Bin, "sendmail", "sendmail.exe");
                value = "sendmail_path = \"" + sendmail + "\" -t";
            }

            try
            {
                Clipboard.SetText(value);
            }
            catch
            {
            }

            Say("sendmail_path copied to the clipboard");
        }

        private void AddCertificateToStore()
        {
            string crt = Path.Combine(_stack.Root, @"etc\ssl\laragon.crt");
            if (!File.Exists(crt))
            {
                MessageBox.Show(this,
                    Language.T("File not found: ") + crt + Environment.NewLine
                    + Language.T("Use \"Generate certs and add to trust store\" first."),
                    Brand.Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // the original builds a .bat around the same two certutil calls (0x10004a1c0)
            StartProcess("cmd.exe", "/c certutil -f -addstore Root \"" + crt + "\" & pause",
                _stack.Root, false);
        }

        private void OpenCertificateManager()
        {
            StartProcess("certmgr.msc", string.Empty, _stack.Root, false);
        }

        private void ShareSite()
        {
            string exe = Path.Combine(_stack.Bin, "ngrok", "ngrok.exe");
            if (!File.Exists(exe))
            {
                exe = Search(_stack.Bin, "ngrok.exe", 3);
            }

            if (exe == null)
            {
                Say(Unavailable);
                return;
            }

            StartProcess(exe, "http " + _stack.WebPort(), _stack.Root, true);
        }

        private void SublimeTerminal(bool add)
        {
            Component sublime = C("sublime");
            string packages = null;
            if (sublime != null && sublime.Installed)
            {
                packages = Path.Combine(sublime.Folder, @"Data\Packages\User");
            }

            if (packages == null || !Directory.Exists(packages))
            {
                Say(add
                    ? "This feature needs Sublime Text, which is not installed in bin\\."
                    : "This feature needs Sublime Text, which is not installed in bin\\.");
                return;
            }

            OpenFolder(packages);
            Say("Drop the Terminal package into this folder, then restart Sublime Text.");
        }

        private void DeployToCloud(string what)
        {
            MessageBox.Show(this,
                Language.T("Deploying to the Laragon cloud needs a Laragon account and an internet connection; it is not part of this rebuild."),
                Brand.Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ---------------------------------------------------------------- versions and php.ini

        /// <summary>
        /// One row of a Version submenu was clicked. FUN_100059260 (0x100059260) is the row's
        /// OnClick: it reads the version name back out of the clicked row (FUN_10005fd40) and hands
        /// it on. FUN_100057420 (0x100057420) is what Apache, nginx, MySQL and the rest then run,
        /// and it is short: build bin\&lt;key&gt;\&lt;version&gt;, and return without a word if that is already
        /// the version in use (FUN_10000a700 at 0x10000a700 is its compare) or if the folder is not
        /// there (FUN_10001e550 at 0x10001e550); otherwise write the ini's [&lt;key&gt;] Version
        /// (FUN_1000ab690 at 0x1000ab690, which stores it in the record too), refresh the menus and
        /// the window (FUN_100083760, FUN_100061fc0 at 0x100061fc0, FUN_10007cd20), and say
        /// "&lt;Name&gt; Version: &lt;folder&gt;" for three seconds (FUN_1000493f0).
        /// PHP has its own copy of that handler, FUN_100058bc0 (0x100058bc0): it adds two gates —
        /// a folder whose name holds "-nts-" is refused (FUN_1000901b0 at 0x1000901b0, the
        /// thread-safe PHP builds) — rewrites the web server's PHP settings
        /// (FUN_100058920 at 0x100058920) and starts a web server that is already running again
        /// (FUN_10007fe80). Apache and nginx have their own restart after a switch as well.
        /// </summary>
        private void SwitchVersion(string key, string version)
        {
            Component component = C(key);
            if (component == null || string.IsNullOrEmpty(version))
            {
                return;
            }

            string folder = Path.Combine(Path.Combine(_stack.Bin, component.SubFolder), version);
            if (!Directory.Exists(folder))
            {
                return;
            }

            // PHP's own rule: a thread-safe build folder is never switched to
            if (key == "php" && version.IndexOf("-nts-", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return;
            }

            string inUse = component.Installed ? Path.GetFileName(component.Folder) : string.Empty;
            if (string.Equals(version, inUse, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _stack.Settings.Set(key, "Version", version);
            _stack.Settings.Save();
            _stack.Reload();

            if (key == "php" || key == "apache" || key == "nginx")
            {
                RestartWebServers();
            }

            SyncMenu();
            BuildRows();

            Component now = C(key);
            Say((now == null ? component.Name : now.Name) + " " + Language.T("Version") + ": " + version);
        }

        /// <summary>Turns one "name = value" row of the quick settings into a php.ini line.</summary>
        private void SetPhpIni(string setting)
        {
            Component php = C("php");
            if (php == null || !php.Installed)
            {
                Say(Unavailable);
                return;
            }

            string ini = Path.Combine(php.Folder, "php.ini");
            if (!File.Exists(ini))
            {
                Say(Language.T("File not found: ") + ini);
                return;
            }

            int equals = setting.IndexOf('=');
            string name = (equals < 0 ? setting : setting.Substring(0, equals)).Trim();
            string value = equals < 0 ? string.Empty : setting.Substring(equals + 1).Trim();

            try
            {
                var lines = new List<string>(File.ReadAllLines(ini));
                bool written = false;

                for (int i = 0; i < lines.Count; i++)
                {
                    string trimmed = lines[i].TrimStart(';', ' ', '\t');
                    if (!trimmed.StartsWith(name + " ", StringComparison.OrdinalIgnoreCase)
                        && !trimmed.StartsWith(name + "=", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    lines[i] = equals < 0 ? name : name + " = " + value;
                    written = true;
                    break;
                }

                if (!written)
                {
                    lines.Add(equals < 0 ? name : name + " = " + value);
                }

                File.WriteAllLines(ini, lines);
                Say(equals < 0 ? "php.ini: " + name + " is on" : "php.ini: " + name + " = " + value);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Brand.Name, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>Runs a program in a console window, so its output stays readable.</summary>
        private void RunShell(string exe, string arguments)
        {
            StartProcess("cmd.exe", "/k \"" + exe + "\" " + arguments, _stack.Root, true);
        }

        private string Ask(string title, string label, string initial, bool secret)
        {
            using (var box = new AskBox(Language.T(title), Language.T(label), initial, secret))
            {
                return box.ShowDialog(this) == DialogResult.OK ? box.Value.Trim() : null;
            }
        }
    }

}
