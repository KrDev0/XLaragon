using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Laragon
{
    internal static class Program
    {
        /// <summary>True while only saving a picture: nothing must be started then.</summary>
        internal static bool ScreenshotOnly;

        /// <summary>
        /// Set by --lang: draw this run's windows in that language without reading or writing
        /// the ini. The original's pictures are English, so this lets one screenshot be compared
        /// with them like for like even when the saved setting says Spanish.
        /// </summary>
        internal static string ForcedLanguage;

        /// <summary>
        /// Set by --dump: with --screenshot, print every control of the window with its bounds
        /// and caption, so what the code placed can be checked against the owner's pictures.
        /// </summary>
        internal static bool DumpUi;

        /// <summary>
        /// True when this copy is running without administrator rights because the person said No
        /// to Windows' question, so nothing here pretends the rights were granted.
        /// </summary>
        internal static bool StartedUnelevated;

        /// <summary>
        /// True for the switches that save a picture, list what the program sees, or perform one
        /// click and report it: those run from a build, from a plain command line, and must not
        /// put a Windows elevation prompt in the way.
        /// </summary>
        private static bool IsHeadless(string[] args)
        {
            foreach (string arg in args)
            {
                if (string.Equals(arg, "--screenshot", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(arg, "--screenshot-prefs", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(arg, "--screenshot-about", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(arg, "--list", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(arg, "--menudump", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(arg, "--mailsecret", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(arg, "--tryversion", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(arg, "--phpsync", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(arg, "--dump", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Whether this copy already has administrator rights.</summary>
        private static bool IsElevated()
        {
            try
            {
                using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
                {
                    return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Starts this program again through Windows' "run as administrator" prompt and answers
        /// true when that other copy was started. False means the person said No, or the prompt
        /// could not be shown, and the caller carries on unelevated.
        /// </summary>
        private static bool RelaunchAsAdministrator(string[] args)
        {
            try
            {
                var quoted = new StringBuilder();
                foreach (string arg in args)
                {
                    if (quoted.Length > 0)
                    {
                        quoted.Append(' ');
                    }

                    quoted.Append('"').Append(arg.Replace("\"", "\\\"")).Append('"');
                }

                var start = new ProcessStartInfo
                {
                    FileName = Application.ExecutablePath,
                    Arguments = quoted.ToString(),
                    WorkingDirectory = AppContext.BaseDirectory,
                    UseShellExecute = true,
                    Verb = "runas"
                };

                Process.Start(start);
                return true;
            }
            catch (Win32Exception)
            {
                // 1223: the person pressed No on the User Account Control question
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Starting and stopping Apache, MySQL and the rest needs administrator rights, which
            // is how the original is used too (the person runs laragon.exe "as administrator").
            // Rather than baking a manifest that would demand them from every copy of the program
            // — and break the quiet --screenshot / --tryversion runs the build is checked with —
            // the window asks Windows for them on the way up: the program starts itself again
            // elevated and the first copy steps aside. A headless run never asks.
            if (!IsHeadless(args) && !IsElevated())
            {
                if (RelaunchAsAdministrator(args))
                {
                    return;
                }

                // the person answered No to Windows' question: carry on as before
                StartedUnelevated = true;
            }

            string shotPath = null;
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], "--screenshot", StringComparison.OrdinalIgnoreCase))
                {
                    shotPath = args[i + 1];
                }
            }

            string prefsShot = null;
            int prefsTab = 0; for (int i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], "--screenshot-prefs", StringComparison.OrdinalIgnoreCase))
                {
                    prefsShot = args[i + 1];

                    // optional tab name after the file: "general" (the default) or "services"
                    if (i + 2 < args.Length)
                    {
                        if (string.Equals(args[i + 2], "services", StringComparison.OrdinalIgnoreCase))
                        {
                            prefsTab = 1;
                        }
                        else if (string.Equals(args[i + 2], "general", StringComparison.OrdinalIgnoreCase))
                        {
                            prefsTab = 0;
                        }
                    }
                }
            }

            // --screenshot-about <png>: a picture of the About window the Help label opens, so
            // its layout can be looked at without opening the window by hand
            string aboutShot = null;
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], "--screenshot-about", StringComparison.OrdinalIgnoreCase))
                {
                    aboutShot = args[i + 1];
                }
            }

            // --lang English | --lang Spanish: draw this run in that language without touching
            // the ini, so a Spanish install can still be photographed in English for comparison
            // with the original's own pictures. Any pack name will do: the name is kept as it
            // was typed and resolved against <root>\bin\laragon\lang once the folder is known,
            // which is the original's own test (FUN_100095400 at 0x100095400).
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], "--lang", StringComparison.OrdinalIgnoreCase))
                {
                    ForcedLanguage = args[i + 1].Trim();
                }
            }

            // flags with no value after them are looked for over the whole list: the count above
            // stops one short, so a bare flag in the last position would be missed
            if (Array.IndexOf(args, "--dump") >= 0)
            {
                DumpUi = true;
            }

            if (prefsShot != null)
            {
                RunPreferencesScreenshot(prefsShot, prefsTab);
                return;
            }

            if (aboutShot != null)
            {
                RunAboutScreenshot(aboutShot);
                return;
            }

            int menudump = Array.IndexOf(args, "--menudump");
            if (menudump >= 0)
            {
                // diagnostic: every row of the popup menu with its tag and caption, printed
                // after the language was applied, so the whole menu can be read at once.
                // An optional number after the switch is how many window refreshes to simulate
                // first: no row of the dump may change between 1 and 5 (that is the check for
                // rows piling up, which is what the person saw as multiplying separators).
                ScreenshotOnly = true;
                int passes = 1;
                if (menudump + 1 < args.Length)
                {
                    int.TryParse(args[menudump + 1], out passes);
                }

                using (var menuForm = new MainForm())
                {
                    Console.WriteLine("lang: " + Language.Current + "  passes: " + passes);
                    menuForm.DumpMenu(Console.Out, passes);
                }

                return;
            }
            int mailSecret = Array.IndexOf(args, "--mailsecret");
            if (mailSecret >= 0)
            {
                // diagnostic: prove the mail password encoding against the two test words whose
                // answers were worked out from the decompiled formula by tools\pwcheck.py, and
                // say how the ini's own values read back — by length only. The stored password
                // is never printed, so this is safe to run on a real install.
                ScreenshotOnly = true;
                Console.WriteLine("mail secret: FUN_100095e40 (0x100095e40) / FUN_100095fa0 (0x100095fa0)");
                string[,] vectors = { { "abc", "7E9BCE" }, { "hunter2", "77DDAFBCCFBBB7" } };
                for (int i = 0; i < vectors.GetLength(0); i++)
                {
                    string word = vectors[i, 0];
                    string want = vectors[i, 1];
                    string got = MailSecret.Encode(word);
                    string back = MailSecret.Decode(got);
                    Console.WriteLine("  encode(\"" + word + "\") = " + got +
                                      (got == want ? "  match" : "  MISMATCH, expected " + want) +
                                      (back == word ? "  decodes back" : "  DOES NOT DECODE BACK"));
                }

                var mailStack = new Stack();
                Console.WriteLine("  ini: " + mailStack.Settings.FilePath);
                string address = mailStack.Settings.Get("preferences", "MailSenderEmail", string.Empty);
                string stored = mailStack.Settings.Get("preferences", "MailSenderPassword", string.Empty);
                Console.WriteLine("  [preferences] MailSenderEmail: " + address.Length + " character(s)");
                if (stored.Length == 0)
                {
                    Console.WriteLine("  [preferences] MailSenderPassword: not set");
                }
                else if (MailSecret.TryDecode(stored, out string plain))
                {
                    Console.WriteLine("  [preferences] MailSenderPassword: " + stored.Length +
                                      " characters of the original's encoding, reads back as " +
                                      plain.Length + " character(s)");
                }
                else
                {
                    Console.WriteLine("  [preferences] MailSenderPassword: " + stored.Length +
                                      " characters, NOT the original's encoding (shown as it stands)");
                }

                return;
            }

            if (shotPath == null)
            {
                int tryVersion = Array.IndexOf(args, "--tryversion");
                if (tryVersion >= 0 && tryVersion + 2 < args.Length)
                {
                    // diagnostic: perform the very click a row of a Version submenu gets, then
                    // report what the settings, the record and the file say, so a switch can be
                    // checked without a mouse. Nothing is started: ScreenshotOnly is on.
                    ScreenshotOnly = true;
                    using (var tryForm = new MainForm())
                    {
                        tryForm.TryVersion(args[tryVersion + 1], args[tryVersion + 2]);
                    }

                    return;
                }

                if (Array.IndexOf(args, "--phpsync") >= 0)
                {
                    // repair, without starting anything: make every file the web server reads
                    // name the one PHP folder the settings name. This is what a version switch
                    // does too; here it can be run on its own when a config was left behind by an
                    // older install, and it prints what it changed. No server is started.
                    var syncStack = new Stack();
                    Console.WriteLine("root: " + syncStack.Root);
                    Component syncPhp = syncStack.Find("php");
                    Console.WriteLine("php:  " + (syncPhp == null || string.IsNullOrEmpty(syncPhp.Folder)
                        ? "(none found)"
                        : syncPhp.Folder));
                    if (syncPhp == null || string.IsNullOrEmpty(syncPhp.Folder))
                    {
                        Console.WriteLine("nothing to do: no PHP folder is installed in this root");
                        return;
                    }

                    IList<string> changed = PhpSettings.Sync(syncStack);
                    if (changed.Count == 0)
                    {
                        Console.WriteLine("already correct: every file the web server reads names "
                            + syncPhp.Folder);
                    }
                    else
                    {
                        foreach (string file in changed)
                        {
                            Console.WriteLine("wrote " + file);
                        }
                    }

                    Console.WriteLine("PATH for the servers: "
                        + PhpSettings.IsolatePath(syncPhp.Folder,
                            Environment.GetEnvironmentVariable("PATH") ?? string.Empty));
                    return;
                }

                if (Array.IndexOf(args, "--list") >= 0)
                {
                    // diagnostic: what the program sees in its folder
                    var stack = new Stack();
                    Language.Folder = stack.LangFolder;
                    Language.Load(ForcedLanguage ?? stack.Settings.Get("preferences", "Lang",
                        stack.Settings.Get("laragon", "Language", Language.English)));
                    Console.WriteLine("root: " + stack.Root + "  (" + stack.RootSource + ")");
                    Console.WriteLine("ini:  " + stack.Settings.FilePath);
                    Console.WriteLine("admin: " + (IsElevated() ? "yes" : StartedUnelevated ? "no (refused)" : "no"));
                    Console.WriteLine("lang: folder=" + Language.Folder
                        + "  packs=" + Language.Available(Language.Folder).Length
                        + "  ini=" + stack.Settings.Get("preferences", "Lang", "<none>")
                        + "  current=" + Language.Current
                        + "  T(Start All)=" + Language.T("Start All"));
                    foreach (Component c in stack.Components)
                    {
                        Console.WriteLine(string.Format("{0,-12} {1,-16} installed={2,-5} running={3,-5} versions={4} folder={5}",
                            c.Key, c.Display, c.Installed, c.IsRunning,
                            c.Versions.Count == 0 ? "(none)" : string.Join(",", c.Versions),
                            string.IsNullOrEmpty(c.Folder) ? "(none)" : c.Folder));
                    }

                    return;
                }

                // The original refuses a second copy: "Laragon is already running!"
                using (var mutex = new Mutex(false, "Laragon.SingleInstance.v1"))
                {
                    bool owned;
                    try
                    {
                        owned = mutex.WaitOne(0, false);
                    }
                    catch (AbandonedMutexException)
                    {
                        owned = true;
                    }

                    if (!owned)
                    {
                        MessageBox.Show(Language.T("Laragon is already running!"), Brand.Name,
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    Application.Run(new MainForm());
                }

                return;
            }

            RunScreenshot(shotPath);

        }

        /// <summary>
        /// Saves a PNG of the About window, so it can be looked at without opening it by hand.
        /// Nothing is started and nothing is read beyond where the program sits.
        /// </summary>
        private static void RunAboutScreenshot(string shotPath)
        {
            ScreenshotOnly = true;
            var stack = new Stack();
            var form = new AboutForm(stack);

            Console.WriteLine("lang: " + Language.Current);
            if (Program.DumpUi)
            {
                Describe(form, 0);
            }

            var timer = new System.Windows.Forms.Timer { Interval = 15 };
            int frames = 0;

            form.Shown += delegate
            {
                timer.Tick += delegate
                {
                    frames++;
                    if (frames < 60)
                    {
                        return;
                    }

                    timer.Stop();
                    try
                    {
                        string full = Path.GetFullPath(shotPath);
                        string folder = Path.GetDirectoryName(full);
                        if (!string.IsNullOrEmpty(folder))
                        {
                            Directory.CreateDirectory(folder);
                        }

                        Size size = form.Size;
                        using (var bitmap = new Bitmap(size.Width, size.Height))
                        {
                            form.DrawToBitmap(bitmap, new Rectangle(0, 0, size.Width, size.Height));
                            bitmap.Save(full, ImageFormat.Png);
                        }

                        Console.WriteLine("saved: " + full + " " + size.Width + "x" + size.Height);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("failed: " + ex.Message);
                    }
                    finally
                    {
                        form.Close();
                    }
                };

                timer.Start();
            };

            Application.Run(form);
        }

        /// <summary>
        /// Saves a PNG of the Preferences window, so its layout can be checked against the
        /// person's pictures the same way the main window is. Nothing is started.
        /// </summary>
        private static void RunPreferencesScreenshot(string shotPath, int tab)
        {
            ScreenshotOnly = true;
            var stack = new Stack();
            var form = new PreferencesForm(stack);
            form.SelectTab(tab);

            Console.WriteLine("lang: " + Language.Current);
            if (Program.DumpUi)
            {
                // the port rows: PHP and MariaDB are the two whose designer values are only
                // placeholders, so their ini values are printed beside the controls' own
                Console.WriteLine("ini: " + stack.Settings.FilePath
                    + "  php.Port=" + stack.Settings.GetInt("php", "Port", -1)
                    + "  mariadb.Port=" + stack.Settings.GetInt("mariadb", "Port", -1));
                Describe(form, 0);
            }

            var timer = new System.Windows.Forms.Timer { Interval = 15 };
            int frames = 0;

            form.Shown += delegate
            {
                timer.Tick += delegate
                {
                    frames++;
                    if (frames < 120)
                    {
                        return;
                    }

                    timer.Stop();
                    try
                    {
                        string full = Path.GetFullPath(shotPath);
                        string folder = Path.GetDirectoryName(full);
                        if (!string.IsNullOrEmpty(folder))
                        {
                            Directory.CreateDirectory(folder);
                        }

                        Size size = form.Size;
                        using (var bitmap = new Bitmap(size.Width, size.Height))
                        {
                            form.DrawToBitmap(bitmap, new Rectangle(0, 0, size.Width, size.Height));
                            bitmap.Save(full, ImageFormat.Png);
                        }

                        Console.WriteLine("saved " + full + " (" + size.Width + "x" + size.Height + ")");
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine("screenshot failed: " + ex.Message);
                    }

                    form.Close();
                };

                timer.Start();
            };

            Application.Run(form);
        }

        /// <summary>
        /// Starts the window, lets it draw about 120 frames, saves a PNG of it and exits.
        /// Used to check the result against the original's picture.
        /// </summary>
        private static void Describe(Control parent, int depth)
        {
            foreach (Control child in parent.Controls)
            {
                if (!string.IsNullOrEmpty(child.Text))
                {
                    Console.WriteLine(string.Format("{0}{1} vis={2} {3} '{4}'",
                        new string(' ', depth * 2), child.Name, child.Visible, child.Bounds, child.Text));
                }

                // a drop-down's rows are not controls, so they are counted here: the Language
                // list is filled from the packs found in bin\laragon\lang (FUN_10008a690)
                ComboBox combo = child as ComboBox;
                if (combo != null)
                {
                    Console.WriteLine(string.Format("{0}{1} items={2} '{3}'",
                        new string(' ', depth * 2), combo.Name, combo.Items.Count, combo.Text));
                }

                Describe(child, depth + 1);
            }
        }

        private static void RunScreenshot(string shotPath)
        {
            ScreenshotOnly = true;
            var form = new MainForm();

            // diagnostic: which language this run draws in (the ini's choice, or --lang)
            Console.WriteLine("lang: " + Language.Current);
            // --dump prints every control with its bounds and caption, which is how the row
            // slots and the code-written labels were checked against the pictures.
            if (Program.DumpUi)
            {
                Describe(form, 0);
            }
            var timer = new System.Windows.Forms.Timer { Interval = 15 };
            int frames = 0;

            form.Shown += delegate
            {
                timer.Tick += delegate
                {
                    frames++;
                    if (frames < 120)
                    {
                        return;
                    }

                    timer.Stop();
                    try
                    {
                        string full = Path.GetFullPath(shotPath);
                        string folder = Path.GetDirectoryName(full);
                        if (!string.IsNullOrEmpty(folder))
                        {
                            Directory.CreateDirectory(folder);
                        }

                        // DrawToBitmap includes the window frame (title bar and borders), so the
                        // bitmap is the whole window, not just the 1024x620 client area.
                        Size size = form.Size;
                        using (var bitmap = new Bitmap(size.Width, size.Height))
                        {
                            form.DrawToBitmap(bitmap, new Rectangle(0, 0, size.Width, size.Height));
                            bitmap.Save(full, ImageFormat.Png);
                        }

                        Console.WriteLine("saved " + full + " (" + size.Width + "x" + size.Height + ")");
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine("screenshot failed: " + ex.Message);
                    }

                    form.ExitRequested = true;
                    form.Close();
                };

                timer.Start();
            };

            Application.Run(form);
        }
    }
}
