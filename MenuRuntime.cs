using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Laragon
{
    /// <summary>
    /// The three submenus the original builds when it starts instead of in the form.
    ///
    /// Decoded from laragon.exe: the "Quick app" rows (PHP, Wordpress, Laravel, Python, Ruby,
    /// Node.js, Go, Java, Docker, Kubernetes) and the four "Quick settings" rows carry no OnClick
    /// in the form resource, so the original wires them in code. Its handler for every one of
    /// them is the same function, sub_1000549a0 at 0x1000549a0: it asks for a project name,
    /// refuses an empty one ("Please specify Project Name!"), an invalid one ("The name is not
    /// valid!") and one holding an underscore ("The name is not valid. It must not contain
    /// \"_\"!"), refuses an existing folder ("Project exists!"), creates the folder under the
    /// document root, and, when the row carries a command, prefixes it with "composer " when it
    /// holds "create-project" (string 0x1003956f0) or runs it as "laravel new" (0x100395728) in a
    /// console. The empty-command rows only make the folder, which is what the same function does
    /// through sub_100054500 at 0x100054500.
    ///
    /// PHP > Extensions lists the .dll files the installed PHP keeps in its ext folder and ticks
    /// the ones php.ini switches on, the way the original fills its one placeholder row.
    /// </summary>
    public partial class MainForm
    {
        private const string QuickAppPrefix = "QuickApp|";

        /// <summary>Fills the three run-time submenus. Called once, after the rows exist.</summary>
        private void FillRuntimeMenus()
        {
            FillQuickAppMenu();
            FillQuickSettingsMenu();
            FillPhpExtensionsMenu();
            HookLists();
            // the www, Ngrok and site-template lists are rebuilt when their menu opens;
            // clearing them here would empty a list while the designer's rows are being wired.
        }

        // ---------------------------------------------------------------- Quick app

        private void FillQuickAppMenu()
        {
            ToolStripMenuItem host = FindItem("MnuQuickAppClick");
            if (host == null)
            {
                return;
            }

            // caption, command; an empty command makes the folder only
            string[][] rows =
            {
                new string[] { "PHP", string.Empty },
                new string[] { "Wordpress", string.Empty },
                new string[] { "Laravel", "composer create-project laravel/laravel" },
                new string[] { "-", string.Empty },
                new string[] { "Python", string.Empty },
                new string[] { "Ruby", string.Empty },
                new string[] { "Node.js", string.Empty },
                new string[] { "Go", string.Empty },
                new string[] { "Java", string.Empty },
                new string[] { "-", string.Empty },
                new string[] { "Docker", string.Empty },
                new string[] { "Kubernetes", string.Empty }
            };

            host.DropDownItems.Clear();
            foreach (string[] row in rows)
            {
                if (row[0] == "-")
                {
                    host.DropDownItems.Add(new ToolStripSeparator());
                    continue;
                }

                ToolStripMenuItem item = new ToolStripMenuItem(Language.T(row[0]));
                item.Tag = QuickAppPrefix + row[0] + "|" + row[1];
                item.Click += QuickAppRowClick;
                host.DropDownItems.Add(item);
            }
        }

        private void QuickAppRowClick(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            string tag = item == null ? null : item.Tag as string;
            if (string.IsNullOrEmpty(tag) || !tag.StartsWith(QuickAppPrefix, StringComparison.Ordinal))
            {
                return;
            }

            string rest = tag.Substring(QuickAppPrefix.Length);
            int bar = rest.IndexOf('|');
            string framework = bar < 0 ? rest : rest.Substring(0, bar);
            string command = bar < 0 ? string.Empty : rest.Substring(bar + 1);
            QuickApp(framework, command);
        }

        /// <summary>The original's quick create, step by step (sub_1000549a0, 0x1000549a0).</summary>
        private void QuickApp(string framework, string command)
        {
            string name = Ask("Quick create - " + framework, "Project Name", framework, false);
            if (name == null)
            {
                return;
            }

            name = name.Trim();
            if (name.Length == 0)
            {
                Say("Please specify Project Name!");
                return;
            }

            if (name.IndexOf('_') >= 0)
            {
                Say("The name is not valid. It must not contain \"_\"!");
                return;
            }

            foreach (char c in Path.GetInvalidFileNameChars())
            {
                if (name.IndexOf(c) >= 0)
                {
                    Say("The name is not valid! " + name);
                    return;
                }
            }

            name = name.Replace(' ', '-');

            string folder = Path.Combine(_stack.Www, name);
            if (Directory.Exists(folder))
            {
                Say("Project exists!\r\nIf you really want to create the project, delete the project's folder and try again");
                return;
            }

            try
            {
                Directory.CreateDirectory(folder);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Brand.Name, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(command))
            {
                Say("Created: " + folder);
                return;
            }

            if (command.IndexOf("create-project", StringComparison.OrdinalIgnoreCase) >= 0
                && !command.StartsWith("composer ", StringComparison.OrdinalIgnoreCase))
            {
                command = "composer " + command;
            }

            command = command.Replace("%s", name);
            if (command.IndexOf(name, StringComparison.Ordinal) < 0)
            {
                command = command + " " + name;
            }

            StartProcess("cmd.exe", "/k " + command, _stack.Www, true);
        }

        // ---------------------------------------------------------------- PHP > Quick settings

        private void FillQuickSettingsMenu()
        {
            ToolStripMenuItem host = FindItem("MnuPHPQuickSettingsClick");
            if (host == null)
            {
                return;
            }

            // the original's four rows, exactly as the form writes them
            string[] settings =
            {
                "max_execution_time = 0",
                "xdebug",
                "upload_max_filesize = 2G",
                "memory_limit = 512M"
            };

            host.DropDownItems.Clear();
            foreach (string setting in settings)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(setting);
                item.Tag = "PhpSetting|" + setting;
                item.Click += QuickSettingClick;
                host.DropDownItems.Add(item);
            }
        }

        private void QuickSettingClick(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            string tag = item == null ? null : item.Tag as string;
            if (string.IsNullOrEmpty(tag) || !tag.StartsWith("PhpSetting|", StringComparison.Ordinal))
            {
                return;
            }

            SetPhpIni(tag.Substring("PhpSetting|".Length));
        }

        // ---------------------------------------------------------------- PHP > Extensions

        private void FillPhpExtensionsMenu()
        {
            ToolStripMenuItem host = FindItem("MnuPHPExtensionsClick");
            Component php = C("php");
            if (host == null || php == null || !php.Installed)
            {
                return;
            }

            host.DropDownItems.Clear();

            string ext = Path.Combine(php.Folder, "ext");
            if (!Directory.Exists(ext))
            {
                ToolStripMenuItem none = new ToolStripMenuItem(Language.T("dir: ext"));
                none.Enabled = false;
                host.DropDownItems.Add(none);
                return;
            }

            string ini = Path.Combine(php.Folder, "php.ini");
            string text = File.Exists(ini) ? File.ReadAllText(ini) : string.Empty;

            List<string> names = new List<string>();
            foreach (string dll in Directory.GetFiles(ext, "*.dll"))
            {
                string simple = Path.GetFileNameWithoutExtension(dll);
                if (simple.StartsWith("php_", StringComparison.OrdinalIgnoreCase))
                {
                    simple = simple.Substring(4);
                }

                if (!names.Contains(simple))
                {
                    names.Add(simple);
                }
            }

            names.Sort(StringComparer.OrdinalIgnoreCase);
            foreach (string name in names)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(name);
                item.Tag = "PhpExtension|" + name;
                item.Checked = IsPhpExtensionOn(text, name);
                item.Click += PhpExtensionClick;
                host.DropDownItems.Add(item);
            }
        }

        private static bool IsPhpExtensionOn(string iniText, string name)
        {
            foreach (string line in iniText.Split('\n'))
            {
                string trimmed = line.Trim();
                if (trimmed.StartsWith(";", StringComparison.Ordinal) || trimmed.Length == 0)
                {
                    continue;
                }

                if (!trimmed.StartsWith("extension", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (trimmed.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private void PhpExtensionClick(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            string tag = item == null ? null : item.Tag as string;
            if (string.IsNullOrEmpty(tag) || !tag.StartsWith("PhpExtension|", StringComparison.Ordinal))
            {
                return;
            }

            string name = tag.Substring("PhpExtension|".Length);
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

            try
            {
                List<string> lines = new List<string>(File.ReadAllLines(ini));
                bool on = item.Checked;
                bool written = false;

                for (int i = 0; i < lines.Count; i++)
                {
                    string probe = lines[i].TrimStart(';', ' ', '\t');
                    if (!probe.StartsWith("extension", StringComparison.OrdinalIgnoreCase)
                        || probe.IndexOf(name, StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        continue;
                    }

                    if (!written)
                    {
                        if (on)
                        {
                            lines[i] = ";" + lines[i];
                        }
                        else
                        {
                            lines[i] = probe;
                        }

                        written = true;
                    }

                    break;
                }

                if (!written && !on)
                {
                    lines.Add("extension=" + name);
                }

                File.WriteAllLines(ini, lines);
                item.Checked = !on;
                Say("php.ini: extension " + name + (on ? " is off" : " is on"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Brand.Name, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
