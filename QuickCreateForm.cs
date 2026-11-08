using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Laragon
{
    /// <summary>
    /// The original's TFormQuickCreateWebsite ("Quick create website"): make www\&lt;name&gt;, point
    /// &lt;name&gt;.test at 127.0.0.1 and leave a starter page behind.
    /// </summary>
    public partial class QuickCreateForm : Form
    {
        private readonly Stack _stack;

        /// <summary>Needed by the Windows Forms designer.</summary>
        public QuickCreateForm()
            : this(null)
        {
        }

        public QuickCreateForm(Stack stack)
        {
            InitializeComponent();
            _stack = stack ?? new Stack();

            cboType.SelectedIndex = 0;
            txtName.TextChanged += delegate { UpdatePreview(); };
            cboType.SelectedIndexChanged += delegate { UpdatePreview(); };
            btnCreate.Click += delegate { Create(); };
            Load += delegate
            {
                UpdatePreview();
                txtName.SelectAll();
                txtName.Focus();
            };
        }

        public string SelectedType
        {
            get { return cboType.Text; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    int index = cboType.FindStringExact(value);
                    if (index >= 0)
                    {
                        cboType.SelectedIndex = index;
                    }
                }
            }
        }

        /// <summary>
        /// What was typed, trimmed. sub_1000549a0 (0x1000549a0) does not tidy the name, it
        /// refuses the ones it does not like: empty ("Please specify Project Name!"), holding
        /// "_" ("The name is not valid. It must not contain \"_\"!") or anything but letters,
        /// digits, '-' and '.' ("The name is not valid!"), each message followed by the name.
        /// </summary>
        private string TypedName
        {
            get { return txtName.Text.Trim(); }
        }

        private static bool NameIsValid(string name)
        {
            foreach (char c in name)
            {
                if (!char.IsLetterOrDigit(c) && c != '-' && c != '.')
                {
                    return false;
                }
            }

            return name.Length > 0;
        }

        private string HostName
        {
            get { return _stack.HostFormat.Replace("{name}", TypedName.ToLowerInvariant()); }
        }

        private void UpdatePreview()
        {
            string name = TypedName.Length == 0 ? "mysite" : TypedName;

            // "Quick create - %s", the title sub_1000549a0 gives the box it shows.
            Text = "Quick create - " + SelectedType;
            lblAddressValue.Text = "http://" + _stack.HostFormat.Replace("{name}", name.ToLowerInvariant()) + "/";
            lblFolderValue.Text = Path.Combine(_stack.Www, name);
        }

        private void Create()
        {
            string name = TypedName;

            if (name.Length == 0)
            {
                MessageBox.Show(this, Language.T("Please specify Project Name!"), Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtName.Focus();
                return;
            }

            if (name.IndexOf('_') >= 0)
            {
                MessageBox.Show(this, Language.T("The name is not valid. It must not contain \"_\"!") +
                    Environment.NewLine + name, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            if (!NameIsValid(name))
            {
                MessageBox.Show(this, Language.T("The name is not valid!") + Environment.NewLine + name,
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            string folder = Path.Combine(_stack.Www, name);

            if (Directory.Exists(folder))
            {
                MessageBox.Show(this,
                    Language.T("Project exists!") + Environment.NewLine + folder + Environment.NewLine +
                    Language.T("If you really want to create the project, delete the project's folder and try again"),
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // FUN_100054500 (0x100054500): AutoCreateDatabase decides whether a database is made
            // with the project. With the key on but MySQL down the original refuses and says so
            // instead of creating a folder the site could not use.
            bool autoDatabase = _stack.Settings.GetBool("preferences", "AutoCreateDatabase", false);
            Component mysql = _stack.Find("mysql");

            if (autoDatabase && (mysql == null || !mysql.IsRunning))
            {
                MessageBox.Show(this,
                    Language.T("AutoCreateDatabase is enabled, but the database hasn't been started. Please click [Start All] to launch it, or disable automatic database creation by setting (AutoCreateDatabase=False) in Menu > Quick App > Configuration..."),
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var notes = new StringBuilder();

            try
            {
                Directory.CreateDirectory(folder);
                string index = Path.Combine(folder, "index.php");
                if (!File.Exists(index))
                {
                    File.WriteAllText(index, StarterPage(name), new UTF8Encoding(false));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (autoDatabase && mysql != null)
            {
                notes.AppendLine(CreateDatabase(mysql, name));
            }

            if (_stack.Settings.GetBool("preferences", "AutoVirtualHosts",
                    _stack.Settings.GetBool("laragon", "AutoVirtualHosts", true)))
            {
                string problem = AddHostsEntry(HostName);
                if (problem != null)
                {
                    notes.AppendLine();
                    notes.AppendLine("The hosts file could not be changed: " + problem);
                    notes.AppendLine("Run Laragon as Administrator, or add this line by hand:");
                    notes.AppendLine("    127.0.0.1    " + HostName);
                }
            }

            MessageBox.Show(this,
                Language.T("Website created:") + Environment.NewLine +
                folder + Environment.NewLine + Environment.NewLine +
                Language.T("Open it at") + " http://" + HostName + "/" +
                (notes.Length > 0 ? Environment.NewLine + notes.ToString() : string.Empty),
                Language.T("Quick create website"), MessageBoxButtons.OK, MessageBoxIcon.Information);

            if (chkOpen.Checked)
            {
                try
                {
                    Process.Start(new ProcessStartInfo("explorer.exe", "\"" + folder + "\"") { UseShellExecute = true });
                }
                catch
                {
                    // opening Explorer is cosmetic
                }
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// FUN_100066720 (0x100066720): &lt;version&gt;\bin\mysqladmin -u root [-p"&lt;password&gt;"]
        /// create &lt;name&gt;. "Created database!" + name when it works, "Database exists!" + name
        /// when mysqladmin says the database is already there, and otherwise the tool's own words.
        /// </summary>
        private string CreateDatabase(Component mysql, string name)
        {
            string exe = Path.Combine(mysql.Folder, "bin", "mysqladmin.exe");
            string password = _stack.Settings.Get("mysql", "MySQLRootPassword",
                _stack.Settings.Get("mysql", "RootPassword", string.Empty));
            string arguments = "-u root" +
                (password.Length == 0 ? string.Empty : " -p\"" + password + "\"") + " create " + name;

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

                    if (output.IndexOf("error", StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        return Language.T("Created database!") + " " + name;
                    }

                    if (output.IndexOf("database exists", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return Language.T("Database exists!") + " " + name;
                    }

                    return name + ":" + Environment.NewLine + output.Trim();
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private static string AddHostsEntry(string hostName)
        {
            try
            {
                string windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                string hosts = Path.Combine(windows, @"System32\drivers\etc\hosts");
                string text = File.Exists(hosts) ? File.ReadAllText(hosts) : string.Empty;

                foreach (string line in text.Split('\n'))
                {
                    string trimmed = line.Trim();
                    if (trimmed.Length == 0 || trimmed.StartsWith("#"))
                    {
                        continue;
                    }

                    string[] parts = trimmed.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 1 && string.Equals(parts[1], hostName, StringComparison.OrdinalIgnoreCase))
                    {
                        return null;
                    }
                }

                File.AppendAllText(hosts, Environment.NewLine + "127.0.0.1\t" + hostName + Environment.NewLine);
                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private static string StarterPage(string name)
        {
            return "<?php\n" +
                   "?>\n" +
                   "<!DOCTYPE html>\n" +
                   "<html lang=\"en\">\n" +
                   "<head>\n" +
                   "  <meta charset=\"utf-8\">\n" +
                   "  <title>" + name + "</title>\n" +
                   "  <style>\n" +
                   "    body { font-family: Arial, Helvetica, sans-serif; margin: 60px; color: #222; }\n" +
                   "    h1 { font-weight: normal; }\n" +
                   "    code { background: #f2f2f2; padding: 2px 6px; }\n" +
                   "  </style>\n" +
                   "</head>\n" +
                   "<body>\n" +
                   "  <h1>" + name + " is ready</h1>\n" +
                   "  <p>PHP version: <code><?php echo PHP_VERSION; ?></code></p>\n" +
                   "  <p>Document root: <code>" + name + "</code></p>\n" +
                   "</body>\n" +
                   "</html>\n";
        }
    }
}
