using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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

        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string shotPath = null;
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], "--screenshot", StringComparison.OrdinalIgnoreCase))
                {
                    shotPath = args[i + 1];
                }
            }

            string prefsShot = null;
            int prefsTab = 0;            for (int i = 0; i < args.Length - 1; i++)
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
            if (shotPath == null)
            {
                if (Array.IndexOf(args, "--list") >= 0)
                {
                    // diagnostic: what the program sees in its folder
                    var stack = new Stack();
                    Language.Folder = stack.LangFolder;
                    Language.Load(ForcedLanguage ?? stack.Settings.Get("preferences", "Lang",
                        stack.Settings.Get("laragon", "Language", Language.English)));
                    Console.WriteLine("root: " + stack.Root + "  (" + stack.RootSource + ")");
                    Console.WriteLine("ini:  " + stack.Settings.FilePath);
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
