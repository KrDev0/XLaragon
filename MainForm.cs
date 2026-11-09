using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Laragon
{
    public partial class MainForm : Form
    {
        private const int RowsPerColumn = 9;
        private const int RowHeight = 40;

        private static readonly string[] PreferredOrder =
        {
            "apache", "nginx", "mysql", "mariadb", "postgresql", "mongodb", "redis", "memcached",
            "mailpit", "php", "nodejs", "python", "ruby", "go", "java", "git", "composer", "phpmyadmin"
        };

        private readonly Stack _stack;
        private readonly ServiceManager _services;
        private readonly List<Component> _shown = new List<Component>();
        private readonly List<Label> _dots = new List<Label>();
        private readonly List<Label> _names = new List<Label>();
        private readonly List<Label> _versions = new List<Label>();

        /// <summary>One tick per service row: the ones Start All will bring up.</summary>
        private readonly List<CheckBox> _ticks = new List<CheckBox>();

        /// <summary>True while the rows are being filled in, so setting a tick does not save.</summary>
        private bool _fillingRows;

        public MainForm()
        {
            InitializeComponent();

            // The Visual Studio designer builds this window to show it, and then it must stay
            // exactly what the designer file says: no ini is read, no icon is loaded and no
            // service is started (the ini's AutoStart would otherwise bring Apache and MySQL up
            // on the developer's machine just by opening the form in the designer).
            if (DesignTime.IsDesigning)
            {
                return;
            }

            _stack = new Stack();
            _services = new ServiceManager(_stack);

            // The language chosen in Preferences, read before anything is drawn. The original
            // keeps it in its own ini as [preferences] Lang, holding the language's name (the
            // person's laragon.ini says Lang=Spanish); [laragon] Language is this rebuild's
            // older key, still read so an ini written by an earlier build keeps working.
            // The original reads [preferences] Lang and then uses the pack file that name
            // points at, answering "English" when there is none (FUN_100095400 at
            // 0x100095400). FUN_100095530 (0x100095530) parses the pack itself.
            Language.Folder = _stack.LangFolder;
            Language.Load(_stack.Settings.Get("preferences", "Lang",
                _stack.Settings.Get("laragon", "Language", Language.English)));

            // --lang on the command line wins over the ini for this run, so the window can be
            // drawn in English for a like-for-like comparison with the original's pictures
            // without writing Language=English into the person's settings.
            if (Program.ForcedLanguage != null)
            {
                Language.Load(Program.ForcedLanguage);
            }

            ApplyArtwork();
            WireEvents();
            Language.Apply(this);
            RefreshState();
            MinimizeIfAsked();
            AutoStartIfAsked();
        }

        /// <summary>
        /// "Run minimized" in the original's Preferences is the ini key <c>MinimizeWhenRun</c>
        /// (read by sub_10009afc0 at 0x10009afc0 with default 0, written by sub_100088750 at
        /// 0x100088750), and its own hint explains it as "Minimize Laragon to the System Tray.
        /// Left click Laragon's icon in the system tray to show Laragon". So it decides how
        /// the window appears when Laragon runs, not what the close button does.
        /// </summary>
        private void MinimizeIfAsked()
        {
            if (Program.ScreenshotOnly)
            {
                return;
            }

            if (_stack.Settings.GetBool("laragon", "MinimizeWhenRun", false))
            {
                WindowState = FormWindowState.Minimized;
                Hide();
            }
        }

        /// <summary>
        /// The original's preferences hold a single switch, "Start All automatically", stored in
        /// the ini as <c>AutoStart</c> (read by FUN_10009b040 at 0x10009b040, which asks
        /// FUN_10009bc20 for the key "AutoStart" with a default of 0; written by FUN_100088800
        /// at 0x100088800 from the Preferences checkbox CheckBoxAutoStart, set up in
        /// FUN_10008b540 at 0x10008b540 under the tooltip "Automatically start all selected
        /// Services when Laragon runs"). Off unless the ini says otherwise, and never during a
        /// --screenshot run.
        /// </summary>
        private void AutoStartIfAsked()
        {
            if (Program.ScreenshotOnly)
            {
                return;
            }

            if (_stack.Settings.GetBool("laragon", "AutoStart", false))
            {
                StartAll();
            }
        }

        /// <summary>Set before closing for real (Exit, or the screenshot run), so the
        /// minimise-to-tray behaviour does not keep the program alive.</summary>
        public bool ExitRequested { get; set; }

        private void ApplyArtwork()
        {

            btnStart.Image = Icons.Play(18, Color.FromArgb(30, 140, 60));
            btnStop.Image = Icons.Stop(16, Color.FromArgb(170, 40, 40));
            btnWeb.Image = Icons.Globe(20);
            btnDatabase.Image = Icons.Database(20);
            btnShell.Image = Icons.Terminal(20);
            btnDir.Image = Icons.Folder(20);
            string slogan = Slogan();
            lblSlogan.Text = slogan;
            // the original only shows a quotation when usr\slogan.txt holds one
            lblSlogan.Visible = slogan.Length > 0;
            lblProfile.Text = "[" + _stack.Settings.Get("laragon", "Profile", "default") + "]";
        }

        private string Slogan()
        {
            string file = Path.Combine(_stack.Root, "usr", "slogan.txt");
            try
            {
                if (File.Exists(file))
                {
                    string line = File.ReadAllLines(file).FirstOrDefault(l => l.Trim().Length > 0);
                    if (!string.IsNullOrEmpty(line))
                    {
                        return line.Trim();
                    }
                }
            }
            catch
            {
                // a slogan is decoration: never fail because of it
            }

            return string.Empty;
        }

        private void WireEvents()
        {
            picLogo.Click += delegate { ShowMainMenu(); };
            lblMenu.Click += delegate { ShowMainMenu(); };
            ContextMenuStrip = mnuLaragon;
            mnuLaragon.ItemClicked += MenuItemClick;

            // one handler for every remaining leaf (see MenuActions.cs); the rows this file's
            // own switch already answers keep answering it first
            AttachMenu(mnuLaragon.Items);

            btnStart.Click += delegate { StartAll(); };
            btnStop.Click += delegate { StopAll(); };
            btnWeb.Click += delegate { OpenWeb(); };
            btnDatabase.Click += delegate { OpenDatabase(); };
            btnShell.Click += delegate { OpenTerminal(); };
            btnDir.Click += delegate { OpenRoot(); };
            FitCaptions();

            lblHelp.Click += delegate { ShowHelp(); };
            lblHosts.Click += delegate { OpenHostsFile(); };
            lblProfile.Click += delegate { ShowProfiles(); };
            picPref.Click += delegate { ShowPreferences(); };
            btnNotificationOK.Click += delegate { panelNotification.Visible = false; };

            notifyIcon.DoubleClick += delegate
            {
                Show();
                WindowState = FormWindowState.Normal;
                Activate();
            };
            notifyIcon.ContextMenuStrip = mnuLaragon;
            notifyIcon.BalloonTipText = Brand.Name;

            timerRefresh.Tick += delegate { RefreshState(); };
            Resize += delegate
            {
                if (WindowState == FormWindowState.Minimized)
                {
                    Hide();
                }
            };
            FormClosing += delegate (object sender, FormClosingEventArgs e)
            {
                if (ExitRequested)
                {
                    notifyIcon.Visible = false;
                    return;
                }

                // The original keeps running in the system tray when its window is closed;
                // quitting goes through Menu > Exit (ExitRequested above).
                e.Cancel = true;
                Hide();
                return;
            };
        }

        private void ShowMainMenu()
        {
            mnuLaragon.Show(panelTopLeft, new Point(0, 58));
        }

        // ---------------------------------------------------------------- state

        private void RefreshState()
        {
            BuildRows();

            bool running = _stack.AnyServiceRunning;
            btnStart.Visible = !running;
            btnStop.Visible = running;

            string state;
            if (running)
            {
                string[] names = _stack.Components
                    .Where(c => c.IsService && c.IsRunning)
                    .Select(c => c.Name)
                    .ToArray();
                state = Language.T("Running: ") + string.Join(", ", names);
            }
            else
            {
                state = Language.T("All services are stopped.");
            }

            lblStatus.Text = state + "    root: " + _stack.Root;

            Component apache = _stack.Find("apache");
            Component nginx = _stack.Find("nginx");
            if (apache != null && apache.IsRunning)
            {
                lblStatus.Text += "    web: " + _stack.ApachePort;
            }
            else if (nginx != null && nginx.IsRunning)
            {
                lblStatus.Text += "    web: " + _stack.NginxPort;
            }
        }

        private void BuildRows()
        {
            _shown.Clear();

            // Every one of the eighteen software records takes one of the eighteen row slots,
            // installed or not. The owner asked for that directly ("los elementos del lblName13
            // al lblName18 no me los esta mostrando"): a row that is switched off at run time is
            // also a row whose labels he cannot place by hand in the designer, which is the whole
            // point of these controls.
            //
            // What a missing install costs is the row's usefulness, not the row: its version
            // column reads "-" and its switch is greyed (see below), so Start All still skips it
            // exactly as the original does — FUN_10008c470 (0x10008c470) tests each service's
            // program with FUN_10001e550 (0x10001e550) before showing its widgets, and
            // FUN_1000abc80 (0x1000abc80) makes the same test before starting anything.
            foreach (Component each in _stack.Components)
            {
                _shown.Add(each);
            }

            int order(Component c)
            {
                int index = Array.IndexOf(PreferredOrder, c.Key.ToLowerInvariant());
                return index < 0 ? 99 : index;
            }

            List<Component> inOrder = new List<Component>(_shown);
            inOrder.Sort((a, b) =>
            {
                int byOrder = order(a).CompareTo(order(b));
                return byOrder != 0 ? byOrder : _shown.IndexOf(a).CompareTo(_shown.IndexOf(b));
            });
            _shown.Clear();
            _shown.AddRange(inOrder);

            int need = Math.Min(_shown.Count, RowsPerColumn * 2);
            CollectRows();

            _fillingRows = true;
            try
            {
                for (int i = 0; i < _dots.Count; i++)
                {
                    bool used = i < need;
                    // the person's picture has no separate status dot beside a row: the switch at
                    // the row's left carries both "takes part in Start All" and "is running", so
                    // the dot stays in the designer (it can be moved there) but is never drawn
                    _dots[i].Visible = false;
                    _names[i].Visible = used;
                    _versions[i].Visible = used;
                    _ticks[i].Visible = used && _shown.Count > i;
                    if (!used)
                    {
                        continue;
                    }

                    Component component = _shown[i];
                    // Nothing here writes a position any more. The eighteen row slots keep the
                    // place MainForm.Designer.cs gives them, so moving lblVer (or any other
                    // control) in the Visual Studio designer is exactly where it shows up. What
                    // the designer holds is the same 40 px row grid measured on the person's
                    // pictures, which is why the window looks unchanged.
                    _dots[i].BackColor = component.IsRunning
                        ? Color.FromArgb(76, 175, 80)
                        : Color.FromArgb(205, 205, 205);

                    _names[i].Text = component.Name;

                    _versions[i].Text = string.IsNullOrEmpty(component.Version) ? "-" : component.Version;
                    _versions[i].ForeColor = component.IsService ? Color.DimGray : Color.Silver;

                    // the switch sits at the very left of the row and its knob, not the word
                    // "Enabled", is what says whether the service is chosen
                    var pill = _ticks[i] as PillSwitch;
                    if (pill != null)
                    {
                        pill.Running = component.IsRunning;
                    }

                    _ticks[i].Tag = component.Key;
                    // a service whose program is not installed can be seen but not switched on:
                    // the original tests the file on disk as well (FUN_10001e550 from FUN_1000abc80)
                    _ticks[i].Checked = component.Installed && IsTicked(component);
                    _ticks[i].Enabled = component.Installed && !component.IsRunning;
                    _ticks[i].BringToFront();
                }
            }
            finally
            {
                _fillingRows = false;
            }
        }

        /// <summary>
        /// Whether a service takes part in "Start All". The original keeps a per-service
        /// "Enabled" switch at offset +0x48 of each service record, and its Start All asks
        /// FUN_1000abc80 (0x1000abc80) before starting anything — that switch, plus the
        /// service's executable existing on disk. Ours stores the same switch in the settings
        /// file under the service's own section, key "Enabled"; untouched services are on.
        /// </summary>
        private bool IsTicked(Component component)
        {
            return _stack.Settings.GetBool(component.Key, "Enabled", true);
        }

        private void TickChanged(object sender, System.EventArgs e)
        {
            if (_fillingRows)
            {
                return;
            }

            var tick = sender as CheckBox;
            if (tick == null || tick.Tag == null)
            {
                return;
            }

            _stack.Settings.Set((string)tick.Tag, "Enabled", tick.Checked);
            try
            {
                _stack.Settings.Save();
            }
            catch
            {
                // a read-only install folder must not break the tick
            }
        }

        /// <summary>
        /// The eighteen row slots (two columns of nine) are declared in MainForm.Designer.cs, so
        /// the form designer shows every row with its dot, its name, its version and its
        /// "Enabled" tick, and they can be moved or restyled there. This collects them once and
        /// wires the ticks; BuildRows fills in the text and the colours.
        /// </summary>
        private void CollectRows()
        {
            if (_dots.Count > 0)
            {
                return;
            }

            _dots.AddRange(new[] { lblDot1, lblDot2, lblDot3, lblDot4, lblDot5, lblDot6, lblDot7, lblDot8, lblDot9, lblDot10, lblDot11, lblDot12, lblDot13, lblDot14, lblDot15, lblDot16, lblDot17, lblDot18 });
            _names.AddRange(new[] { lblName1, lblName2, lblName3, lblName4, lblName5, lblName6, lblName7, lblName8, lblName9, lblName10, lblName11, lblName12, lblName13, lblName14, lblName15, lblName16, lblName17, lblName18 });
            _versions.AddRange(new[] { lblVer1, lblVer2, lblVer3, lblVer4, lblVer5, lblVer6, lblVer7, lblVer8, lblVer9, lblVer10, lblVer11, lblVer12, lblVer13, lblVer14, lblVer15, lblVer16, lblVer17, lblVer18 });
            _ticks.AddRange(new[] { chkRow1, chkRow2, chkRow3, chkRow4, chkRow5, chkRow6, chkRow7, chkRow8, chkRow9, chkRow10, chkRow11, chkRow12, chkRow13, chkRow14, chkRow15, chkRow16, chkRow17, chkRow18 });

            foreach (CheckBox tick in _ticks)
            {
                tick.CheckedChanged += TickChanged;
            }

            _dots.ForEach(d => d.BringToFront());
            _names.ForEach(d => d.BringToFront());
            _versions.ForEach(d => d.BringToFront());
            lblStatus.BringToFront();
        }

        /// <summary>
        /// FUN_100084660 (0x100084660), applied by FUN_1000845a0 (0x1000845a0) to the six
        /// bottom buttons: when the caption measured with the button's own font
        /// (FUN_100084510, 0x100084510) is wider than the button, the caption is replaced by
        /// its first fifteen characters and the literal at 0x100395260, "...". The button
        /// keeps its size; only the word is cut.
        /// </summary>
        private void FitCaptions()
        {
            Button[] bottom = { btnStart, btnStop, btnWeb, btnDatabase, btnShell, btnDir };
            foreach (Button button in bottom)
            {
                if (button == null || string.IsNullOrEmpty(button.Text))
                {
                    continue;
                }

                int measured = TextRenderer.MeasureText(button.Text, button.Font).Width;
                if (button.Width < measured && button.Text.Length > CaptionChars)
                {
                    button.Text = button.Text.Substring(0, CaptionChars) + "...";
                }
            }
        }

        private const int CaptionChars = 0xf;

        private void ShowNotification(string text)
        {
            lblNotification.Text = Language.T(text);
            panelNotification.Visible = true;
            panelNotification.BringToFront();
        }

        // ---------------------------------------------------------------- actions

        private void StartAll()
        {
            // FUN_100082ae0 (0x100082ae0) refuses to build its configuration when the install
            // folder holds a space, because the generated httpd/nginx lines would be cut short.
            if (_stack.Root.IndexOf(' ') >= 0)
            {
                ShowNotification("Path must not have SPACE!");
                return;
            }

            var started = new List<string>();
            var problems = new List<string>();

            // only the services whose row is ticked are brought up, and only those whose
            // program is really installed: a row for a service that is not there can be seen
            // but not switched on (the original tests the file on disk too, FUN_1000abc80)
            var chosen = new HashSet<string>();
            foreach (Component component in _stack.Components)
            {
                if (component.IsService && component.Installed && IsTicked(component))
                {
                    chosen.Add(component.Key);
                }
            }

            if (chosen.Count == 0)
            {
                ShowNotification(Language.T("No service is enabled: tick 'Enabled' on the ones you want Start All to bring up."));
                return;
            }

            Cursor = Cursors.WaitCursor;
            int count;
            try
            {
                count = _services.StartAll(started, problems, chosen);
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            if (count == 0 && problems.Count == 0)
            {
                ShowNotification(string.Format(Language.T("No servers are installed under {0}."), _stack.Bin));
            }
            else
            {
                ShowNotification(count + " started: " + string.Join(", ", started));
            }

            if (problems.Count > 0)
            {
                MessageBox.Show(this, string.Join(Environment.NewLine, problems.Select(Language.T)), Brand.Name,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            RefreshState();
        }

        private void StopAll()
        {
            Cursor = Cursors.WaitCursor;
            int count;
            try
            {
                // the original says "Stopping..." while it works, then "Stopped."
                ShowNotification("Stopping...");
                count = _services.StopAll();
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            ShowNotification("Stopped.");
            RefreshState();
        }

        private void StartService(Component service)
        {
            if (service == null || !service.Installed)
            {
                MessageBox.Show(this,
                    string.Format(Language.T("This service is not installed in {0}."), _stack.Bin), Brand.Name,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string problem = _services.Start(service);
            if (problem == null)
            {
                ShowNotification(service.Name + " started.");
            }
            else if (problem != "already running")
            {
                MessageBox.Show(this, service.Name + ": " + Language.T(problem), Brand.Name,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            RefreshState();
        }

        private void StopService(Component service)
        {
            if (service == null)
            {
                return;
            }

            _services.Stop(service);
            ShowNotification(service.Name + " stopped.");
            RefreshState();
        }

        /// <summary>
        /// The Web button, as FUN_10004db40 (0x10004db40) does it: if the web server or MySQL
        /// is switched off in Preferences, say so and do nothing; if they are switched on but
        /// not running, start everything first and then open the site. The scheme and the port
        /// come from the running web server's SSL setting (FUN_10008fee0, FUN_10008fdc0).
        /// </summary>
        private void OpenWeb()
        {
            Component apache = _stack.Find("apache");
            Component nginx = _stack.Find("nginx");
            Component mysql = _stack.Find("mysql");

            bool webEnabled = (apache != null && apache.Installed) || (nginx != null && nginx.Installed);
            bool dbEnabled = mysql != null && mysql.Installed;

            if (!webEnabled || !dbEnabled)
            {
                ShowNotification("You must enable Apache/Nginx and MySQL services in Menu > Preferences > Services and Ports!");
                return;
            }

            bool anyRunning = (apache != null && apache.IsRunning) || (nginx != null && nginx.IsRunning);
            if (!anyRunning || (mysql != null && !mysql.IsRunning))
            {
                StartAll();
            }

            OpenUrl(SiteUrl());
        }

        /// <summary>The address of the local site, built the way the original builds it.</summary>
        private string SiteUrl()
        {
            string scheme = _stack.Scheme();
            int port = _stack.WebPort();
            bool plain = (scheme == "http://" && port == 80) || (scheme == "https://" && port == 443);
            return scheme + "localhost" + (plain ? string.Empty : ":" + port) + "/";
        }

        /// <summary>
        /// The Database button opens phpMyAdmin, not the site (the same handler in the original
        /// treats the control named "Database" separately).
        /// </summary>
        private void OpenDatabase()
        {
            // FUN_10004db40 (0x10004db40): the admin page the install really has, never a guess.
            OpenDatabasePage();
        }

        private void OpenTerminal()
        {
            // FUN_10004e860 (0x10004e860): Cmder in the document root, never a bare cmd window.
            LaunchTerminal();
        }

        private void OpenRoot()
        {
            // FUN_10004ddc0 (0x10004ddc0): open the folder, and say "No folder!" when there is none.
            OpenRootFolder();
        }

        private void OpenFolder(string path)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                ShowNotification("Folder not found: " + path);
                return;
            }

            StartProcess("explorer.exe", "\"" + path + "\"", _stack.Root, false);
        }

        private void OpenUrl(string url)
        {
            StartProcess(url, string.Empty, _stack.Root, false);
        }

        private void OpenHostsFile()
        {
            string windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            string hosts = Path.Combine(windows, @"System32\drivers\etc\hosts");
            OpenFileOrSay(hosts);
        }

        private void OpenFileOrSay(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                ShowNotification("File not found: " + path);
                return;
            }

            string editor = Path.Combine(_stack.Bin, "notepad++", "notepad++.exe");
            if (File.Exists(editor))
            {
                StartProcess(editor, "\"" + path + "\"", _stack.Root, false);
            }
            else
            {
                StartProcess("notepad.exe", "\"" + path + "\"", _stack.Root, false);
            }
        }

        private void StartProcess(string file, string arguments, string workingDirectory, bool shell)
        {
            try
            {
                var info = new ProcessStartInfo(file, arguments)
                {
                    UseShellExecute = shell,
                    WorkingDirectory = string.IsNullOrEmpty(workingDirectory) ? _stack.Root : workingDirectory
                };
                Process.Start(info);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Brand.Name, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ---------------------------------------------------------------- dialogs

        private void ShowPreferences()
        {
            using (var form = new PreferencesForm(_stack))
            {
                form.ShowDialog(this);
            }

            // the language may have been changed in there: read it again and redraw this window
            Language.Load(_stack.Settings.Get("preferences", "Lang",
                _stack.Settings.Get("laragon", "Language", Language.English)));
            Language.Apply(this);

            lblProfile.Text = "[" + _stack.Settings.Get("laragon", "Profile", "default") + "]";
            RefreshState();
        }

        private void QuickCreate()
        {
            using (var form = new QuickCreateForm(_stack))
            {
                form.ShowDialog(this);
            }

            RefreshState();
        }

        private void ShowVersions()
        {
            var lines = _stack.Components
                .Where(c => c.Installed)
                .Select(c => c.Name.PadRight(12) + (string.IsNullOrEmpty(c.Version) ? "(no version)" : c.Version)
                            + "    " + c.Folder)
                .ToArray();

            string text = lines.Length == 0
                ? string.Format(Language.T("Nothing installed under {0}"), _stack.Bin)
                : string.Join(Environment.NewLine, lines);

            MessageBox.Show(this, text, Language.T("Versions"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// What the Help label opens. It used to be a message box; it is now the window of
        /// AboutForm.Designer.cs, so its wording and its layout can both be changed in the
        /// Visual Studio designer without touching code. Nothing is read and nothing starts
        /// until this runs, i.e. until the person clicks the label.
        /// </summary>
        private void ShowHelp()
        {
            using (var about = new AboutForm(_stack))
            {
                about.ShowDialog(this);
            }
        }

        private void ShowProfiles()
        {
            MessageBox.Show(this,
                Language.T("Active profile:") + " " + _stack.Settings.Get("laragon", "Profile", "default") + Environment.NewLine + Environment.NewLine +
                Language.T("A profile is a set of chosen versions for PHP, MySQL, Apache and so on."),
                Language.T("Profile"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ---------------------------------------------------------------- menu

        private void MenuItemClick(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolStripItem item = e.ClickedItem;
            if (item == null || !item.Enabled)
            {
                return;
            }

            Dispatch(item.Tag as string, item.Text);
        }

        private void Dispatch(string handler, string caption)
        {
            string key = handler ?? string.Empty;
            string text = caption ?? string.Empty;

            switch (key)
            {
                case "MnuExitClick":
                    ExitRequested = true;
                    Close();
                    return;
                case "MnuPreferencesClick":
                    ShowPreferences();
                    return;
                case "MnuProfileClick":
                    ShowProfiles();
                    return;
                case "MnuStartAllClick":
                    StartAll();
                    return;
                case "MnuWeb2Click":
                    OpenWeb();
                    return;
                case "MnuDatabase2Click":
                    OpenDatabase();
                    return;
                case "MnuShell2Click":
                    OpenTerminal();
                    return;
                case "MnuRoot2Click":
                    OpenRoot();
                    return;
                case "MnuWWWClick":
                    OpenFolder(_stack.Www);
                    return;
                case "MnuQuickAppClick":
                    QuickCreate();
                    return;
                case "MnuLaragonIniClick":
                    OpenFileOrSay(Path.Combine(_stack.ExeFolder, "usr", "laragon.ini"));
                    return;
                case "MnuLaragonLogClick":
                    OpenFileOrSay(Path.Combine(_stack.ExeFolder, "usr", "laragon.log"));
                    return;
                case "MnuEditHostsFileClick":
                    OpenHostsFile();
                    return;
                case "MenuLaragonDirUserClick":
                    OpenFolder(Path.Combine(_stack.Root, "usr"));
                    return;
                case "MnuUserProfileCmdClick":
                    OpenFileOrSay(Path.Combine(_stack.Root, "usr", "user-profile.cmd"));
                    return;
                case "MnuNetStatClick":
                    StartProcess("cmd.exe", "/k netstat -ano", _stack.Root, true);
                    return;
                case "MnuMailpitWebUIClick":
                    OpenUrl("http://localhost:8025/");
                    return;
                case "MnuMySQLphpMyAdminClick":
                    OpenUrl("http://localhost:" + _stack.ApachePort + "/phpmyadmin/");
                    return;
                case "MnuMySQLPHPMyAdmin6Click":
                    OpenUrl("http://localhost:" + _stack.ApachePort + "/phpmyadmin6/");
                    return;
                case "MnuRedisCliClick":
                    OpenRedisCli();
                    return;
                case "MnuMailCatcherClick":
                case "MnuMailSenderClick":
                    MessageBox.Show(this,
                        Language.T("Mail is caught by Mailpit in current Laragon versions; see the Mailpit menu."),
                        Brand.Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
            }

            Component service = ServiceFromHandler(key);
            if (service != null)
            {
                if (key.ToLowerInvariant().Contains("stop"))
                {
                    StopService(service);
                }
                else
                {
                    StartService(service);
                }

                return;
            }

            if (key.EndsWith("VersionClick", StringComparison.OrdinalIgnoreCase))
            {
                ShowVersions();
                return;
            }

            string trimmed = text.Trim();

            if (trimmed.StartsWith("dir:", StringComparison.OrdinalIgnoreCase))
            {
                OpenFolder(FolderForShortcut(trimmed.Substring(4).Trim()));
                return;
            }

            if (LooksLikeFile(trimmed))
            {
                OpenFileOrSay(ResolveFile(Path.GetFileName(trimmed)));
                return;
            }

            if (QuickAppTypes.Contains(trimmed))
            {
                QuickCreate(trimmed);
                return;
            }

            if (DispatchMenu(key, trimmed))
            {
                return;
            }

            MessageBox.Show(this,
                string.Format(Language.T("{0} is not wired up in this rebuild yet."), "\"" + trimmed + "\"")
                + Environment.NewLine + Language.T("The original action is listed in MECHANICS.md."),
                Brand.Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static readonly string[] QuickAppTypes =
        {
            "PHP", "Wordpress", "Laravel", "Python", "Ruby", "Node.js", "Go", "Java",
            "Docker", "Kubernetes", "Lumen", "Laravel 5", "Laravel 4", "Quick Linux server"
        };

        private static bool LooksLikeFile(string text)
        {
            string[] endings = { ".log", ".ini", ".conf", ".tpl", ".cmd", ".bat", ".php", ".txt" };
            return endings.Any(e => text.EndsWith(e, StringComparison.OrdinalIgnoreCase));
        }

        private Component ServiceFromHandler(string handler)
        {
            if (string.IsNullOrEmpty(handler))
            {
                return null;
            }

            string h = handler.ToLowerInvariant();
            if (!h.Contains("start") && !h.Contains("stop"))
            {
                return null;
            }

            if (h.Contains("phpmyadmin") || h.Contains("allclick") || h.Contains("xdebug"))
            {
                return null;
            }

            string[,] map =
            {
                { "apache", "apache" }, { "nginx", "nginx" }, { "mysql", "mysql" },
                { "maria", "mariadb" }, { "postgres", "postgresql" }, { "mongo", "mongodb" },
                { "redis", "redis" }, { "memcached", "memcached" }, { "mailpit", "mailpit" }
            };

            for (int i = 0; i < map.GetLength(0); i++)
            {
                if (h.Contains(map[i, 0]))
                {
                    return _stack.Find(map[i, 1]);
                }
            }

            return null;
        }

        private string FolderForShortcut(string name)
        {
            switch (name.ToLowerInvariant())
            {
                case "data":
                    return _stack.DataDir;
                case "usr":
                    return Path.Combine(_stack.Root, "usr");
                case "www":
                    return _stack.Www;
                default:
                    string direct = Path.Combine(_stack.Root, name);
                    return Directory.Exists(direct) ? direct : _stack.Root;
            }
        }

        private string ResolveFile(string fileName)
        {
            var roots = new[]
            {
                _stack.Etc,
                Path.Combine(_stack.Root, "tmp"),
                _stack.Www,
                _stack.Root
            };

            foreach (string root in roots)
            {
                if (!Directory.Exists(root))
                {
                    continue;
                }

                string direct = Path.Combine(root, fileName);
                if (File.Exists(direct))
                {
                    return direct;
                }

                try
                {
                    string found = Directory
                        .EnumerateFiles(root, fileName, SearchOption.AllDirectories)
                        .FirstOrDefault();
                    if (found != null)
                    {
                        return found;
                    }
                }
                catch
                {
                    // unreadable folders are skipped
                }
            }

            return Path.Combine(_stack.Etc, fileName);
        }

        private void OpenRedisCli()
        {
            Component redis = _stack.Find("redis");
            if (redis == null || !redis.Installed)
            {
                ShowNotification("Redis is not installed.");
                return;
            }

            string cli = Path.Combine(redis.Folder, "redis-cli.exe");
            StartProcess(File.Exists(cli) ? cli : "cmd.exe", File.Exists(cli) ? string.Empty : "/k title Redis", redis.Folder, true);
        }

        private void QuickCreate(string type)
        {
            using (var form = new QuickCreateForm(_stack))
            {
                form.SelectedType = type;
                form.ShowDialog(this);
            }

            RefreshState();
        }
    }
}
