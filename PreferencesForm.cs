using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Laragon
{
    /// <summary>
    /// The Preferences window. Its two tabs, its captions, its hints and its defaults are
    /// the ones decompiled from the original: the routine that fills every caption
    /// (0x10008b540) and the routine that loads every value (0x10008c7d0).
    ///
    /// Every field reads and writes the original's own ini key:
    ///   laragon.RunAtStartup (0), laragon.MinimizeWhenRun (0), laragon.AutoStart (0),
    ///   laragon.Language, laragon.DocumentRoot, laragon.DataDir,
    ///   laragon.AutoVirtualHosts (1), laragon.HostnameFormat ("{name}.test"),
    ///   laragon.AutoBackup (1), laragon.BackupInterval (8), laragon.AutoUpdate (1),
    ///   &lt;service&gt;.Port, .SSLPort, .SSL, .Enabled,
    ///   mailcatcher.Use (1), mailcatcher.DisplayTime (1..100),
    ///   mailsender.Use (1), and the mail account under [preferences]:
    ///   preferences.MailSenderEmail and preferences.MailSenderPassword, the latter encoded
    ///   with the original's own XOR stream (MailSecret.cs).
    /// </summary>
    public partial class PreferencesForm : Form
    {
        /// <summary>The services the table shows, in the order the original starts them.</summary>
        private static readonly string[] ServiceKeys =
        {
            "apache", "nginx", "mysql", "postgresql", "mongodb", "memcached", "redis", "mailpit",
            "php", "mariadb"
        };

        private static readonly int[] DefaultPorts = { 80, 8080, 3306, 5432, 27017, 11211, 6379, 1025, 8000, 3306 };

        /// <summary>What the original shows instead of a stored Gmail password (0x968).</summary>
        private const string HiddenPassword = "********";

        private readonly Stack _stack;

        private Label[] _svcNames;
        private Label[] _svcVersions;
        private NumericUpDown[] _svcPorts;
        private NumericUpDown[] _svcSslPorts;
        private CheckBox[] _svcSsl;
        private CheckBox[] _svcEnabled;

        private string _savedPassword = string.Empty;

        /// <summary>What the designer builds; it holds no stack and loads nothing.</summary>
        public PreferencesForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Brings one of the two tabs to the front. Used by --screenshot-prefs so both
        /// pages can be photographed; nothing else calls it.
        /// </summary>
        public void SelectTab(int index)
        {
            if (index >= 0 && index < tabPrefs.TabCount)
            {
                tabPrefs.SelectedIndex = index;
            }
        }

        public PreferencesForm(Stack stack)
            : this()
        {
            _stack = stack;
            CollectServiceRows();
            LoadSettings();
        }

        /// <summary>
        /// Gathers the ten declared service rows into arrays. The controls themselves
        /// live in the designer file, so their columns can be moved in Visual Studio.
        /// Ten, because that is every service "Start All" can bring up: the eight servers
        /// of the original's table, PHP (which the original starts with `php -S`, see
        /// FUN_10004eb00) and MariaDB.
        /// </summary>
        private void CollectServiceRows()
        {
            _svcNames = new[] { lblSvc1Name, lblSvc2Name, lblSvc3Name, lblSvc4Name, lblSvc5Name, lblSvc6Name, lblSvc7Name, lblSvc8Name, lblSvc9Name, lblSvc10Name };
            _svcVersions = new[] { lblSvc1Ver, lblSvc2Ver, lblSvc3Ver, lblSvc4Ver, lblSvc5Ver, lblSvc6Ver, lblSvc7Ver, lblSvc8Ver, lblSvc9Ver, lblSvc10Ver };
            _svcPorts = new[] { numSvc1Port, numSvc2Port, numSvc3Port, numSvc4Port, numSvc5Port, numSvc6Port, numSvc7Port, numSvc8Port, numSvc9Port, numSvc10Port };
            _svcSslPorts = new[] { numSvc1Ssl, numSvc2Ssl, numSvc3Ssl, numSvc4Ssl, numSvc5Ssl, numSvc6Ssl, numSvc7Ssl, numSvc8Ssl, numSvc9Ssl, numSvc10Ssl };
            _svcSsl = new[] { chkSvc1Ssl, chkSvc2Ssl, chkSvc3Ssl, chkSvc4Ssl, chkSvc5Ssl, chkSvc6Ssl, chkSvc7Ssl, chkSvc8Ssl, chkSvc9Ssl, chkSvc10Ssl };
            _svcEnabled = new[] { chkSvc1, chkSvc2, chkSvc3, chkSvc4, chkSvc5, chkSvc6, chkSvc7, chkSvc8, chkSvc9, chkSvc10 };
        }

        private void LoadSettings()
        {
            if (_stack == null)
            {
                return;
            }

            Settings s = _stack.Settings;

            chkRunAtStartup.Checked = s.GetBool("preferences", "RunAtStartup", s.GetBool("laragon", "RunAtStartup", false));
            chkMinimize.Checked = s.GetBool("laragon", "MinimizeWhenRun", false);
            chkAutoStart.Checked = s.GetBool("laragon", "AutoStart", false);

            // The original keeps the chosen language's name under [preferences] Lang
            // (the person's own laragon.ini holds Lang=Spanish) and then uses the pack file
            // that name points at in <root>\bin\laragon\lang, answering "English" when it is
            // missing (FUN_100095400 at 0x100095400). Older rebuilds wrote [laragon]
            // Language, so that is read as a fallback.
            Language.Folder = _stack.LangFolder;
            string language = s.Get("preferences", "Lang", s.Get("laragon", "Language", Language.English));

            // FUN_10008a690 (0x10008a690) fills the list with the packs it finds in that
            // folder; the designer's two entries stay when the folder holds none.
            if (Language.HasPacks(Language.Folder))
            {
                cboLang.Items.Clear();
                foreach (string pack in Language.Available(Language.Folder))
                {
                    cboLang.Items.Add(pack);
                }
            }

            Language.Load(Program.ForcedLanguage ?? language);
            int languageIndex = cboLang.Items.IndexOf(Language.Current);
            cboLang.SelectedIndex = languageIndex >= 0 ? languageIndex : 0;

            txtDocumentRoot.Text = _stack.Www;
            txtDataDir.Text = _stack.DataDir;
            txtHostname.Text = s.Get("laragon", "HostnameFormat", _stack.HostFormat);

            chkAutoVirtualHosts.Checked = s.GetBool("preferences", "AutoVirtualHosts", s.GetBool("laragon", "AutoVirtualHosts", true));
            chkAutoBackup.Checked = s.GetBool("laragon", "AutoBackup", true);
            numBackupInterval.Value = Clamp(s.GetInt("laragon", "BackupInterval", 8), numBackupInterval);
            chkAutoUpdate.Checked = s.GetBool("laragon", "AutoUpdate", true);

            for (int i = 0; i < ServiceKeys.Length; i++)
            {
                string key = ServiceKeys[i];
                Component component = _stack.Find(key);
                _svcVersions[i].Text = component == null ? string.Empty : component.Version;
                _svcPorts[i].Value = Clamp(s.GetInt(key, "Port", DefaultPorts[i]), _svcPorts[i]);
                _svcSslPorts[i].Value = Clamp(s.GetInt(key, "SSLPort", DefaultPorts[i]), _svcSslPorts[i]);

                // Every row carries an https port, as in the original's table, where each
                // service has its own SSL box and SSL port (tools/form_TFORMPREF.txt).
                _svcSslPorts[i].Enabled = true;
                _svcSsl[i].Enabled = true;
                _svcSsl[i].Checked = s.GetBool(key, "SSL", false);
                _svcEnabled[i].Checked = s.GetBool(key, "Enabled", true);
            }

            chkMailCatcher.Checked = s.GetBool("mailcatcher", "Use", true);

            // FUN_10009f6b0(&value, "mailcatcher", "DisplayTime", DAT_1003a2950), and
            // DAT_1003a2950 at 0x1003a2950 holds "3" — the original starts at 3 seconds.
            numMailSeconds.Value = Clamp(s.GetInt("mailcatcher", "DisplayTime", 3), numMailSeconds);

            chkMailSender.Checked = s.GetBool("mailsender", "Use", true);

            // FUN_100095ae0 (0x100095ae0) reads [preferences] MailSenderEmail with an empty
            // default (FUN_10009f540, section "preferences" at 0x1003a1810), and
            // FUN_100095b10 (0x100095b10) reads [preferences] MailSenderPassword — not the
            // [mailsender] section this rebuild used to write. The password is stored encoded
            // (FUN_100095fa0 at 0x100095fa0): two upper-case hex digits per byte of an XOR
            // stream seeded at 0x1f4b, stepped by *0x302d-0x4cc0. MailSecret does that walk.
            txtGmailAddress.Text = s.Get("preferences", "MailSenderEmail", string.Empty);

            string stored = s.Get("preferences", "MailSenderPassword", string.Empty);
            if (stored.Length == 0)
            {
                // an ini this rebuild wrote before it learned the original's key and encoding:
                // that value is plain text, so it is shown as it stands
                stored = s.Get("mailsender", "Password", string.Empty);
            }

            _savedPassword = MailSecret.Decode(stored);
            txtGmailPassword.Text = _savedPassword.Length == 0 ? string.Empty : HiddenPassword;

            lblSettingsFile.Text = Language.T("Setting file: ") + s.FilePath;

            // The captions the designer holds are English; put them into the chosen
            // language before the window is shown.
            Language.Apply(this);

            // the designer's tooltips are plain English that names the program too
            RebrandHints();
        }

        /// <summary>
        /// The captions of this window are translated by <see cref="Language"/>, which puts the
        /// product's own name right in every string it hands back. A tooltip does not travel
        /// through it — the designer sets it with <c>SetToolTip</c> — so the hints are renamed
        /// here, without touching the designer file the owner keeps.
        /// </summary>
        internal void RebrandHints()
        {
            RebrandHints(this);
        }

        private void RebrandHints(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                string hint = tipHints.GetToolTip(child);
                if (!string.IsNullOrEmpty(hint))
                {
                    tipHints.SetToolTip(child, Brand.Fix(hint));
                }

                RebrandHints(child);
            }
        }

        /// <summary>
        /// The language is switched live: the window is redrawn in the new language at
        /// once, and OK or Apply stores the name in [preferences] Lang.
        /// </summary>
        private void cboLang_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            object item = cboLang.SelectedItem;
            if (item == null)
            {
                return;
            }

            Language.Load(item.ToString());
            Language.Apply(this);
            if (_stack != null)
            {
                lblSettingsFile.Text = Language.T("Setting file: ") + _stack.Settings.FilePath;
            }
        }

        private void Apply()
        {
            if (_stack == null)
            {
                return;
            }

            Settings s = _stack.Settings;

            // The original checks the display time itself and says so in these words.
            int seconds = (int)numMailSeconds.Value;
            if (seconds < 1 || seconds > 100)
            {
                MessageBox.Show(this,
                    string.Format(Language.T("The value: {0} is invalid. Display Time range is from {1} - {2}!"),
                        seconds, 1, 100),
                    Language.T("Preferences"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            string address = txtGmailAddress.Text.Trim();
            if (chkMailSender.Checked
                && !(address.Contains("@") && address.Contains(".") && address.Length > 5))
            {
                MessageBox.Show(this, Language.T("You must specify a valid Gmail address!"), Language.T("Preferences"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // These three are the original's own keys and live in its own sections
            // (the person's laragon.ini: [preferences] RunAtStartup=-1, AutoVirtualHosts=-1,
            // DataDir=C:\laragon\data; [apache] DocumentRoot=C:\laragon\www). A ticked box is
            // written as -1 and an empty one as 0, exactly as the Free Pascal program does.
            s.Set("preferences", "RunAtStartup", chkRunAtStartup.Checked ? "-1" : "0");
            PutBool(s, "laragon", "MinimizeWhenRun", chkMinimize.Checked);
            PutBool(s, "laragon", "AutoStart", chkAutoStart.Checked);
            s.Set("preferences", "Lang", Language.Resolve(Convert.ToString(cboLang.SelectedItem), Language.Folder));
            s.Set("apache", "DocumentRoot", txtDocumentRoot.Text.Trim());
            s.Set("laragon", "DocumentRoot", txtDocumentRoot.Text.Trim());
            s.Set("preferences", "DataDir", txtDataDir.Text.Trim());
            s.Set("laragon", "HostnameFormat", txtHostname.Text.Trim());
            s.Set("preferences", "AutoVirtualHosts", chkAutoVirtualHosts.Checked ? "-1" : "0");
            PutBool(s, "laragon", "AutoBackup", chkAutoBackup.Checked);
            s.Set("laragon", "BackupInterval", ((int)numBackupInterval.Value).ToString());
            PutBool(s, "laragon", "AutoUpdate", chkAutoUpdate.Checked);

            for (int i = 0; i < ServiceKeys.Length; i++)
            {
                string key = ServiceKeys[i];
                s.Set(key, "Port", ((int)_svcPorts[i].Value).ToString());

                if (_svcSslPorts[i].Enabled)
                {
                    s.Set(key, "SSLPort", ((int)_svcSslPorts[i].Value).ToString());
                    PutBool(s, key, "SSL", _svcSsl[i].Checked);
                }

                PutBool(s, key, "Enabled", _svcEnabled[i].Checked);
            }

            PutBool(s, "mailcatcher", "Use", chkMailCatcher.Checked);
            s.Set("mailcatcher", "DisplayTime", seconds.ToString());
            PutBool(s, "mailsender", "Use", chkMailSender.Checked);
            s.Set("preferences", "MailSenderEmail", address);

            // "********" means "the stored password, unchanged" — as the original shows it.
            if (txtGmailPassword.Text != HiddenPassword)
            {
                _savedPassword = txtGmailPassword.Text;
            }

            // The original's own key and encoding (FUN_100096350 at 0x100096350 writing through
            // FUN_100095e40 at 0x100095e40), so the original reads back what this writes.
            s.Set("preferences", "MailSenderPassword", MailSecret.Encode(_savedPassword));

            // FUN_10008d820 (0x10008d820) writes these three keys once, only when they are
            // missing: smtp_server=smtp.gmail.com, smtp_port=587, smtp_ssl=TLS. The mail test
            // script itself carries the same host, port and "tls" (0x1003aea48).
            PutIfMissing(s, "laragon", "smtp_server", "smtp.gmail.com");
            PutIfMissing(s, "laragon", "smtp_port", "587");
            PutIfMissing(s, "laragon", "smtp_ssl", "TLS");

            s.Save();
            lblSettingsFile.Text = Language.T("Setting file: ") + s.FilePath + Language.T("   (saved)");

            SyncStartupShortcut();
        }

        /// <summary>
        /// The original does not write a registry Run key: it saves (or deletes) a shortcut
        /// in the Windows Startup folder, and on failure it says
        /// "Could not create shortcut in Startup folder!" (0x1003a16a0) /
        /// "Could not delete the shortcut from Startup folder!" (0x1003a16e8).
        /// </summary>
        private void SyncStartupShortcut()
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            if (folder.Length == 0)
            {
                return;
            }

            string path = Path.Combine(folder, Brand.Name + ".lnk");

            try
            {
                if (chkRunAtStartup.Checked)
                {
                    if (File.Exists(path))
                    {
                        return;
                    }

                    string target = Environment.ProcessPath;
                    if (string.IsNullOrEmpty(target))
                    {
                        return;
                    }

                    Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                    if (shellType == null)
                    {
                        return;
                    }

                    object shell = Activator.CreateInstance(shellType);
                    object link = shellType.InvokeMember(
                        "CreateShortcut", BindingFlags.InvokeMethod, null, shell, new object[] { path });

                    Type linkType = link.GetType();
                    linkType.InvokeMember("TargetPath", BindingFlags.SetProperty, null, link, new object[] { target });
                    linkType.InvokeMember("WorkingDirectory", BindingFlags.SetProperty, null, link,
                        new object[] { Path.GetDirectoryName(target) ?? string.Empty });
                    linkType.InvokeMember("Description", BindingFlags.SetProperty, null, link,
                        new object[] { Brand.Name + " is awesome!" });
                    linkType.InvokeMember("Save", BindingFlags.InvokeMethod, null, link, null);
                }
                else if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception)
            {
                MessageBox.Show(this,
                    Language.T(chkRunAtStartup.Checked
                        ? "Could not create shortcut in Startup folder!"
                        : "Could not delete the shortcut from Startup folder!"),
                    Language.T("Preferences"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private static void PutBool(Settings settings, string section, string key, bool value)
        {
            settings.Set(section, key, value ? "1" : "0");
        }

        /// <summary>
        /// Writes a key only when it is absent, which is what FUN_10008d820 (0x10008d820) does
        /// with the three SMTP defaults: an existing value is never overwritten.
        /// </summary>
        private static void PutIfMissing(Settings settings, string section, string key, string value)
        {
            if (settings.Get(section, key, string.Empty).Length == 0)
            {
                settings.Set(section, key, value);
            }
        }

        private static decimal Clamp(int value, NumericUpDown box)
        {
            if (value < (int)box.Minimum)
            {
                return box.Minimum;
            }

            return value > (int)box.Maximum ? box.Maximum : value;
        }

        private void btnDocumentRoot_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Document Root";
                dialog.SelectedPath = txtDocumentRoot.Text;
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    txtDocumentRoot.Text = dialog.SelectedPath;
                }
            }
        }

        private void btnDataDir_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Data Dir";
                dialog.SelectedPath = txtDataDir.Text;
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    txtDataDir.Text = dialog.SelectedPath;
                }
            }
        }

        /// <summary>Sends a test message using the SMTP credentials entered in Preferences.</summary>
        private async void btnTestMail_Click(object sender, EventArgs e)
        {
            string address = txtGmailAddress.Text.Trim();

            // The box shows "********" while a password is stored, as the original does.
            string password = txtGmailPassword.Text == HiddenPassword
                ? _savedPassword
                : txtGmailPassword.Text;

            if (address.Length == 0 || password.Length == 0)
            {
                MessageBox.Show(this, Language.T("You must specify a valid Gmail address!"),
                    Language.T("Test Sending Mail"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Cursor previous = Cursor;
            Cursor = Cursors.WaitCursor;
            try
            {
                await System.Threading.Tasks.Task.Run(() => MailTest.Send(address, password));
                MessageBox.Show(this, Language.T("Test message sent successfully to ") + address,
                    Language.T("Test Sending Mail"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Language.T("Test Sending Mail"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                Cursor = previous;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Apply();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            Apply();
        }
    }
}
