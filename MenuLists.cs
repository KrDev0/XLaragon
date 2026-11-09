using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Laragon
{
    /// <summary>
    /// The lists the original fills in code, all that is left of the empty placeholders its form
    /// carries. Every value below was read from the decompiled original this round; the function
    /// that establishes it is named in the comment above each one.
    ///
    ///   "www" (MnuWWWClick)   FUN_100052140 at 0x100052140 appends, after the form's own
    ///                         "localhost" row: a "Share" submenu, "Explore", a separator, one
    ///                         row per project folder, a separator and "Switch Document Root".
    ///   "Ngrok" (MnuShareClick) FUN_10006d9c0 at 0x10006d9c0 builds the same submenu: one
    ///                         "Share -> &lt;project&gt;" row per project, a separator, "Your
    ///                         Authtoken", the "Region" list and "Auto-generated QR Code".
    ///   templates             nothing in the original fills MnuSiteTemplate (it has no caption
    ///                         and no handler); the "Site template" submenu stays as this
    ///                         rebuild's own addition over usr\tpl.
    ///
    /// Nothing here starts a program, a server or a page: every row acts on a click only, so
    /// --screenshot and --menudump stay quiet.
    /// </summary>
    public partial class MainForm
    {
        private const string WwwPrefix = "Www|";
        private const string SharePrefix = "Share|";
        private const string RegionPrefix = "Region|";
        private const string TemplatePrefix = "SiteTemplate|";
        private const string RootPrefix = "Root|";

        // FUN_100094c20 / FUN_100094bb0: [preferences] DocumentRootList, joined with '|'.
        private const string RootListSection = "preferences";
        private const string RootListKey = "DocumentRootList";
        private const string RootListSeparator = "|";

        /// <summary>
        /// Fills the lists straight away. The menus do this themselves when they open; the
        /// --menudump switch calls it so the rows can be read without a mouse.
        /// </summary>
        internal void FillListsNow()
        {
            FillWwwMenu();
            FillQuickAppMenu();
            FillSiteTemplateMenu();
            FillShareMenu();
            FillAllVersionItems();

            // the rows those fills made are not in the designer, so they take their picture here
            ApplyMenuIcons();
        }

        /// <summary>
        /// The thirteen Version submenus, in the order the original's refresh names them
        /// (FUN_100061fc0 sets the caption of each parent row, php at form offset +0xc10, nodejs
        /// at +0xc00, mysql at +0xce8 and so on).
        /// </summary>
        private static readonly string[][] VersionItems =
        {
            new string[] { "MnuApacheVersionClick", "apache" },
            new string[] { "MnuNginxVersionClick", "nginx" },
            new string[] { "MnuMySQLVersionClick", "mysql" },
            new string[] { "MnuPostgreSQLVersionClick", "postgresql" },
            new string[] { "MnuMongoDBVersionClick", "mongodb" },
            new string[] { "MnuRedisVersionClick", "redis" },
            new string[] { "MnuMemcachedVersionClick", "memcached" },
            new string[] { "MnuPHPVersionClick", "php" },
            new string[] { "MnuPythonVersionClick", "python" },
            new string[] { "MnuNodeJsVersionClick", "nodejs" },
            new string[] { "MnuRubyVersionClick", "ruby" },
            new string[] { "MnuGoVersionClick", "go" },
            new string[] { "MnuJavaVersionClick", "java" },
        };

        /// <summary>
        /// When each Version list is built. This is the rule the decompilation settled, and it is
        /// what stops the dividers from multiplying:
        ///
        /// * the periodic refresh does NOT rebuild these lists. The original's refresh,
        ///   FUN_100061fc0 (0x100061fc0, 324 lines), only re-captions each parent row to
        ///   "Version [&lt;in use&gt;]" (the pieces are "Version", " [" at 0x1003977d0, the version
        ///   from FUN_1000ab430 and "]" at 0x100393360) and sets Enabled on a long list of
        ///   controls; it never touches a submenu's rows.
        /// * the lists are built when the person reaches for them — the thirteen fillers are event
        ///   handlers reached only through the form's method table (no direct callers at all), and
        ///   the whole form has exactly one OnPopup handler, so they are the OnClick of each
        ///   parent row, not a popup hook and not a timer.
        ///
        /// The rebuild hangs the fill on the drop-down opening instead of the parent's click,
        /// because a WinForms menu asks for the rows when it is shown — same moment from the
        /// person's point of view, and it keeps a --screenshot run from filling anything.
        /// </summary>
        private void HookVersionLists()
        {
            foreach (string[] pair in VersionItems)
            {
                ToolStripMenuItem item = FindItem(pair[0]);
                if (item == null)
                {
                    continue;
                }

                string key = pair[1];

                // two ways in, on purpose. A row of a list that code added can be replaced between
                // the mouse going down and coming up, and then the row's own Click is never raised;
                // the drop-down's DropDownItemClicked is raised by the owner instead and always
                // arrives. EnsureVersionItems keeps the rows where they are unless something really
                // changed, so the common case does not touch them at all.
                item.DropDownOpening += delegate { EnsureVersionItems(item, key); };
                item.DropDownOpening += delegate { IconiseMenu(item); };
                item.DropDownItemClicked += delegate(object sender, ToolStripItemClickedEventArgs ev)
                {
                    VersionRowClicked(ev == null ? null : ev.ClickedItem);
                };
            }
        }

        /// <summary>
        /// Rebuilds a Version list only when what it is built from has changed: the folder in use or
        /// the folders under bin\&lt;subfolder&gt;. When neither moved, the rows already there are left
        /// exactly as they are — which is what lets the person's click land on the row they aimed at.
        /// </summary>
        private void EnsureVersionItems(ToolStripMenuItem item, string key)
        {
            string stamp = VersionStamp(key);
            string known;
            if (_versionStamps.TryGetValue(key, out known) && string.Equals(known, stamp, StringComparison.Ordinal))
            {
                return;
            }

            FillVersionItems(item, key);
        }

        /// <summary>
        /// What a list was built from: the folder in use, then the folders under bin\&lt;subfolder&gt;.
        /// Cheap enough to take on every open (one folder listing), and it changes the moment a
        /// version is switched or a folder is added by hand.
        /// </summary>
        private string VersionStamp(string key)
        {
            Component component = _stack.Find(key);
            string folder = Path.Combine(_stack.Bin, component == null ? key : component.SubFolder);
            string[] folders;
            try
            {
                folders = Directory.Exists(folder) ? Directory.GetDirectories(folder) : new string[0];
            }
            catch
            {
                folders = new string[0];
            }

            return (component == null ? string.Empty : component.Folder) + "|" + string.Join(",", folders);
        }

        /// <summary>Forgets what every Version list was built from, so the next open rebuilds it.</summary>
        internal void ForgetVersionItems()
        {
            _versionStamps.Clear();
        }

        /// <summary>
        /// Fills all thirteen lists at once, for --menudump (nothing is opened) and so a dump can
        /// be compared with another. It is not called by the refresh.
        /// </summary>
        private void FillAllVersionItems()
        {
            foreach (string[] pair in VersionItems)
            {
                ToolStripMenuItem item = FindItem(pair[0]);
                FillVersionItems(item, pair[1]);
            }
        }

        private bool _listsHooked;

        // what each Version list was built from, so an unchanged list is never rebuilt while the
        // person is pointing at it (MenuLists.EnsureVersionItems)
        private readonly Dictionary<string, string> _versionStamps = new Dictionary<string, string>();

        /// <summary>
        /// The lists are rebuilt every time their menu opens. Filling them once at start-up
        /// would clear a drop-down while the program is still walking the designer's rows to
        /// attach their handlers, which trips "Collection was modified" — so the work waits for
        /// the opening event.
        /// </summary>
        private void HookLists()
        {
            if (_listsHooked)
            {
                return;
            }

            _listsHooked = true;
            HookMenu("MnuWWWClick", delegate { FillWwwMenu(); });
            HookMenu("MnuShareClick", delegate { FillShareMenu(); });
            HookMenu("MnuQuickAppClick", delegate { FillQuickAppMenu(); FillSiteTemplateMenu(); });
            HookVersionLists();
            ApplyShareVisibility();

            // the pictures in front of the menu's rows, drawn in MenuIcons.cs
            ApplyMenuIcons();
        }

        /// <summary>
        /// The form hides the Ngrok row (MnuShare.Visible = FALSE), so something in code has to
        /// show it again. Inferred: the original reveals it once there is a tunnel program to
        /// run. Nothing is started; only a visibility flag is set, so a --screenshot run stays
        /// the same picture.
        /// </summary>
        private void ApplyShareVisibility()
        {
            ToolStripMenuItem share = FindItem("MnuShareClick");
            if (share != null)
            {
                share.Visible = File.Exists(NgrokExecutable());
            }
        }

        private void HookMenu(string name, EventHandler opening)
        {
            ToolStripMenuItem item = FindItem(name);
            if (item != null)
            {
                item.DropDownOpening += opening;

                // what the fill has just added is not in the designer, so it is pictured here
                item.DropDownOpening += delegate { IconiseMenu(item); };
            }
        }

        // ------------------------------------------------------------ the www projects

        /// <summary>
        /// FUN_100052140 at 0x100052140, the whole "www" menu. It reads the ini key "IgnoreList"
        /// (FUN_100094b60 at 0x100094b60) and the project folders of the document root
        /// (FUN_10008fc20 at 0x10008fc20 gives the root, FUN_10009a210 at 0x10009a210 lists its
        /// sub-folders, skipping names that start with a dot), then appends to the drop-down:
        /// "Share" (id 220), "Explore" (id 612), a separator, one row per folder, a separator,
        /// and "Switch Document Root" (id 237). With no folder at all it appends one disabled
        /// row captioned "&lt;&lt;Empty&gt;&gt;" instead of the project rows.
        /// </summary>
        private void FillWwwMenu()
        {
            ToolStripMenuItem host = FindItem("MnuWWWClick");
            if (host == null)
            {
                return;
            }

            host.DropDownItems.Clear();

            // the form's own first child, MnuWWWEmpty, whose caption is "localhost"
            ToolStripMenuItem root = new ToolStripMenuItem(Language.T("localhost"));
            root.Tag = WwwPrefix;
            root.Click += WwwRowClick;
            host.DropDownItems.Add(root);

            // the "Share" submenu the code builds from the Ngrok handler (FUN_10006d9c0)
            ToolStripMenuItem share = new ToolStripMenuItem(Language.T("Share"));
            share.Tag = SharePrefix + "menu";
            share.ToolTipText = NgrokExecutable();
            share.DropDownOpening += delegate { FillShareItems(share); IconiseMenu(share); };
            host.DropDownItems.Add(share);

            ToolStripMenuItem explore = new ToolStripMenuItem(Language.T("Explore"));
            explore.Tag = WwwPrefix + ":explore";
            explore.Click += ExploreClick;
            host.DropDownItems.Add(explore);

            host.DropDownItems.Add(new ToolStripSeparator());

            string[] sites = SiteFolders();
            if (sites.Length == 0)
            {
                // the original's own words for an empty list, and it does not translate them
                ToolStripMenuItem empty = new ToolStripMenuItem("<<Empty>>");
                empty.Enabled = false;
                host.DropDownItems.Add(empty);
            }
            else
            {
                foreach (string site in sites)
                {
                    ToolStripMenuItem item = new ToolStripMenuItem(site);
                    item.Tag = WwwPrefix + site;
                    item.Click += WwwRowClick;
                    host.DropDownItems.Add(item);
                }
            }

            host.DropDownItems.Add(new ToolStripSeparator());

            ToolStripMenuItem switchRoot = new ToolStripMenuItem(Language.T("Switch Document Root"));
            switchRoot.Tag = RootPrefix + "menu";
            switchRoot.DropDownOpening += delegate { FillRootItems(switchRoot); IconiseMenu(switchRoot); };
            host.DropDownItems.Add(switchRoot);
        }

        private void WwwRowClick(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            string tag = item == null ? null : item.Tag as string;
            if (tag == null || !tag.StartsWith(WwwPrefix, StringComparison.Ordinal))
            {
                return;
            }

            string site = tag.Substring(WwwPrefix.Length);
            OpenUrl(LocalUrl(site.Length == 0 ? string.Empty : site.Replace(" ", "%20") + "/"));
        }

        /// <summary>The "Explore" row: the document root in Explorer, as the original's FUN_1000526e0 does.</summary>
        private void ExploreClick(object sender, EventArgs e)
        {
            ExploreFolder(_stack.Www);
        }

        private void ExploreFolder(string path)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                Say(Unavailable);
                return;
            }

            StartProcess("explorer.exe", "/\"" + path + "\"", _stack.Root, false);
        }

        /// <summary>
        /// The project folders of the document root. FUN_10009a210 at 0x10009a210 keeps the
        /// sub-folders whose name does not start with a dot; FUN_100051c70 at 0x100051c70 drops
        /// the ones named in the ini key "IgnoreList", a pipe-separated list of whole names
        /// (FUN_100094b60 at 0x100094b60 reads it with a default of 0).
        /// </summary>
        private string[] SiteFolders()
        {
            try
            {
                string www = _stack.Www;
                if (!Directory.Exists(www))
                {
                    return new string[0];
                }

                List<string> ignored = IgnoredNames();
                List<string> names = new List<string>();
                foreach (string folder in Directory.GetDirectories(www))
                {
                    string name = Path.GetFileName(folder);
                    if (name.StartsWith(".", StringComparison.Ordinal) || ignored.Contains(name))
                    {
                        continue;
                    }

                    names.Add(name);
                }

                names.Sort(StringComparer.OrdinalIgnoreCase);
                return names.ToArray();
            }
            catch (Exception)
            {
                return new string[0];
            }
        }

        /// <summary>The ini key "IgnoreList", a pipe-separated list of project names to hide.</summary>
        private List<string> IgnoredNames()
        {
            List<string> names = new List<string>();
            string raw = _stack.Settings.Get("laragon", "IgnoreList", string.Empty);
            foreach (string part in raw.Split('|'))
            {
                string name = part.Trim();
                if (name.Length > 0)
                {
                    names.Add(name);
                }
            }

            return names;
        }

        // --------------------------------------------------------------- Share / Ngrok

        /// <summary>The Ngrok row of the main menu, hidden by the form (MnuShare.Visible = FALSE).</summary>
        private void FillShareMenu()
        {
            ToolStripMenuItem host = FindItem("MnuShareClick");
            if (host != null)
            {
                FillShareItems(host);
            }
        }

        /// <summary>
        /// FUN_10006d9c0 at 0x10006d9c0, the Ngrok menu: one row per project folder captioned
        /// "Share -> &lt;name&gt;" (the caption is the translation id 220 followed by " -> " and
        /// the folder name), then a separator, "Your Authtoken", the "Region" list and
        /// "Auto-generated QR Code" (translation id 159, enabled only while the ini key
        /// "NgrokAutoQRCode" says so — FUN_10009b060 at 0x10009b060). An empty folder list gives
        /// one disabled row captioned "&lt;&lt;Empty&gt;&gt;".
        /// </summary>
        private void FillShareItems(ToolStripMenuItem host)
        {
            host.DropDownItems.Clear();

            string[] sites = SiteFolders();
            if (sites.Length == 0)
            {
                ToolStripMenuItem empty = new ToolStripMenuItem("<<Empty>>");
                empty.Enabled = false;
                host.DropDownItems.Add(empty);
            }
            else
            {
                foreach (string site in sites)
                {
                    ToolStripMenuItem item = new ToolStripMenuItem(Language.T("Share") + " -> " + site);
                    item.Tag = SharePrefix + site;
                    item.Click += ShareRowClick;
                    host.DropDownItems.Add(item);
                }
            }

            host.DropDownItems.Add(new ToolStripSeparator());

            ToolStripMenuItem token = new ToolStripMenuItem(Language.T("Your Authtoken"));
            token.Tag = SharePrefix + ":token";
            token.Click += TokenClick;
            host.DropDownItems.Add(token);

            ToolStripMenuItem region = new ToolStripMenuItem(Language.T("Region"));
            region.Tag = RegionPrefix + "menu";
            foreach (string[] pair in Regions())
            {
                ToolStripMenuItem row = new ToolStripMenuItem(pair[1]);
                row.Tag = RegionPrefix + pair[0];
                row.Checked = string.Equals(NgrokRegion(), pair[0], StringComparison.OrdinalIgnoreCase);
                row.Click += RegionClick;
                region.DropDownItems.Add(row);
            }

            host.DropDownItems.Add(region);

            ToolStripMenuItem qr = new ToolStripMenuItem(Language.T("Auto-generated QR Code"));
            qr.Tag = SharePrefix + ":qr";
            qr.Checked = _stack.Settings.GetBool("laragon", "NgrokAutoQRCode", false);
            qr.Click += QrClick;
            host.DropDownItems.Add(qr);

            host.DropDownItems.Add(new ToolStripSeparator());
        }

        /// <summary>
        /// The four regions of the original's list (the table at 0x1003418c8): the caption is
        /// what the menu shows, the code is the first two letters, which is both what the ini
        /// key "NgrokRegion" stores and what --region gets.
        /// </summary>
        private static string[][] Regions()
        {
            return new string[][]
            {
                new string[] { "ap", "ap - Asia/Pacific" },
                new string[] { "au", "au - Australia" },
                new string[] { "eu", "eu - Europe" },
                new string[] { "us", "us - United States" }
            };
        }

        /// <summary>"NgrokRegion", read by FUN_100094cf0 at 0x100094cf0 with a default of "eu".</summary>
        private string NgrokRegion()
        {
            string region = _stack.Settings.Get("laragon", "NgrokRegion", "eu");
            return region.Length == 0 ? "eu" : region;
        }

        private void RegionClick(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            string tag = item == null ? null : item.Tag as string;
            if (tag == null || !tag.StartsWith(RegionPrefix, StringComparison.Ordinal))
            {
                return;
            }

            _stack.Settings.Set("laragon", "NgrokRegion", tag.Substring(RegionPrefix.Length));
            _stack.Settings.Save();
        }

        private void QrClick(object sender, EventArgs e)
        {
            bool on = !_stack.Settings.GetBool("laragon", "NgrokAutoQRCode", false);
            _stack.Settings.Set("laragon", "NgrokAutoQRCode", on);
            _stack.Settings.Save();
        }

        /// <summary>
        /// "Your Authtoken": the token a tunnel needs, kept as the ini key "NgrokAuthToken"
        /// (FUN_100094d20 at 0x100094d20, default empty). Asked for in a small box rather than
        /// on the command line, which is this rebuild's own way of doing it.
        /// </summary>
        private void TokenClick(object sender, EventArgs e)
        {
            string current = _stack.Settings.Get("laragon", "NgrokAuthToken", string.Empty);
            string token = Ask("Your Authtoken", "Ngrok authtoken", current, true);
            if (token == null)
            {
                return;
            }

            _stack.Settings.Set("laragon", "NgrokAuthToken", token);
            _stack.Settings.Save();
        }

        /// <summary>
        /// A "Share -> &lt;project&gt;" row. FUN_100055350 at 0x100055350 strips the caption back
        /// to the project name and hands it to TNgrok (FUN_1000857b0 at 0x1000857b0), which runs
        /// the command line built by FUN_1000b65e0 at 0x1000b65e0:
        /// <c>/K title &lt;name&gt; &amp;&amp; &lt;ngrok&gt;  http  --host-header=rewrite
        /// --authtoken=&lt;token&gt; &lt;host&gt;:&lt;port&gt; --region=&lt;region&gt;</c>
        /// with the project's host from the ini key "HostnameFormat" and the web port the Web
        /// button uses (FUN_10008fdc0 at 0x10008fdc0).
        /// </summary>
        private void ShareRowClick(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            string tag = item == null ? null : item.Tag as string;
            if (tag == null || !tag.StartsWith(SharePrefix, StringComparison.Ordinal))
            {
                return;
            }

            string site = tag.Substring(SharePrefix.Length);
            string exe = NgrokExecutable();
            if (!File.Exists(exe))
            {
                Say(Unavailable);
                return;
            }

            string host = _stack.HostFormat.Replace("{name}", site);
            string line = "/k title " + site + " && \"" + exe + "\"  http  --host-header=rewrite"
                          + " --authtoken=" + _stack.Settings.Get("laragon", "NgrokAuthToken", string.Empty)
                          + " " + host + ":" + _stack.WebPort()
                          + " --region=" + NgrokRegion();
            StartProcess("cmd.exe", line, _stack.Root, true);
        }

        /// <summary>bin\ngrok\ngrok.exe, the path the original composes in FUN_100052140.</summary>
        private string NgrokExecutable()
        {
            return Path.Combine(_stack.Bin, "ngrok", "ngrok.exe");
        }

        // ------------------------------------------------------ switch document root

        /// <summary>
        /// "Switch Document Root", rebuilt from FUN_10005b0a0 (0x10005b0a0). The roots the
        /// original remembers live in the ini section [preferences] under the key
        /// "DocumentRootList", as one string joined with '|' — read by FUN_100094c20
        /// (0x100094c20), split on '|' by FUN_100090c00 (0x100090c00) and written back, after
        /// de-duplication, by FUN_100094bb0 (0x100094bb0). A remembered root that no longer
        /// exists on disk is dropped from the menu (FUN_10001e550 tests it), a repeat is skipped
        /// (IndexOf), and the root in use is ticked. With nothing remembered the submenu holds
        /// the original's own disabled "&lt;&lt;Empty&gt;&gt;" row. The last row is "Select another"
        /// (FUN_10005c450 at 0x10005c450), which opens the folder picker of FUN_10008a2c0
        /// (0x10008a2c0); its prompt is "Select directory:" and after picking it both switches to
        /// the folder and adds it to the list through FUN_1000951f0 (0x1000951f0).
        /// </summary>
        private void FillRootItems(ToolStripMenuItem host)
        {
            host.DropDownItems.Clear();

            List<string> roots = RememberedRoots();
            if (roots.Count == 0)
            {
                ToolStripMenuItem none = new ToolStripMenuItem("<<Empty>>");
                none.Enabled = false;
                host.DropDownItems.Add(none);
            }

            foreach (string root in roots)
            {
                ToolStripMenuItem row = new ToolStripMenuItem(root);
                row.Tag = RootPrefix + "use";
                row.ToolTipText = root;
                row.Checked = SameFolder(root, _stack.Www);
                row.Click += UseRootClick;
                host.DropDownItems.Add(row);
            }

            host.DropDownItems.Add(new ToolStripSeparator());

            ToolStripMenuItem another = new ToolStripMenuItem(Language.T("Select another"));
            another.Tag = RootPrefix + "pick";
            another.Click += PickRootClick;
            host.DropDownItems.Add(another);
        }

        /// <summary>The [preferences] roots, trimmed, existing only, each once, in file order.</summary>
        private List<string> RememberedRoots()
        {
            var roots = new List<string>();
            string raw = _stack.Settings.Get(RootListSection, RootListKey, string.Empty);

            foreach (string part in raw.Split('|'))
            {
                string root = part.Trim();
                if (root.Length == 0 || !Directory.Exists(root))
                {
                    continue;
                }

                bool known = false;
                foreach (string seen in roots)
                {
                    if (SameFolder(seen, root))
                    {
                        known = true;
                        break;
                    }
                }

                if (!known)
                {
                    roots.Add(root);
                }
            }

            return roots;
        }

        /// <summary>FUN_1000951f0: append the folder to the list with a '|' and write it back.</summary>
        private void RememberRoot(string folder)
        {
            string raw = _stack.Settings.Get(RootListSection, RootListKey, string.Empty).Trim();
            _stack.Settings.Set(RootListSection, RootListKey,
                raw.Length == 0 ? folder : raw + RootListSeparator + folder);
            _stack.Settings.Save();
        }

        private static bool SameFolder(string left, string right)
        {
            return string.Equals(left.TrimEnd('\\'), right.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// FUN_10005b740 (0x10005b740): a remembered row whose folder has gone says "No folder!"
        /// and the path; otherwise it switches, which ends with the balloon "Document Root:" and
        /// the new path (FUN_100083cf0 at 0x100083cf0).
        /// </summary>
        private void UseRootClick(object sender, EventArgs e)
        {
            ToolStripMenuItem row = sender as ToolStripMenuItem;
            if (row != null)
            {
                SwitchRoot(row.Text);
            }
        }

        /// <summary>FUN_10008a2c0: pick a folder, ignore it when it is the one already in use.</summary>
        private void PickRootClick(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = Language.T("Select directory:");
                dialog.SelectedPath = _stack.Www;
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                if (SameFolder(dialog.SelectedPath, _stack.Www))
                {
                    return;
                }

                SwitchRoot(dialog.SelectedPath);
            }
        }

        /// <summary>
        /// FUN_100083cf0: point the web server records at the folder ([apache] and [nginx]
        /// DocumentRoot, the key FUN_1000afc50 (0x1000afc50) reads back), rewrite the two
        /// configuration files through ConfigWriter (FUN_10007a0d0 at 0x10007a0d0 →
        /// FUN_100099700 at 0x100099700), restart whichever web server was already running — the
        /// original restarts it too — then remember the folder and say so.
        /// </summary>
        private void SwitchRoot(string folder)
        {
            if (!Directory.Exists(folder))
            {
                MessageBox.Show(this, Language.T("No folder!") + Environment.NewLine + folder, Brand.Name,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string previous = _stack.Www;
            if (SameFolder(folder, previous))
            {
                return;
            }

            _stack.Settings.Set("apache", "DocumentRoot", folder);
            _stack.Settings.Set("laragon", "DocumentRoot", folder);

            // the original only touches nginx's record when nginx itself is switched on (the flag
            // at +0x48 of the service record that FUN_1000abc80 tests)
            Component nginx = _stack.Find("nginx");
            if (nginx != null && nginx.Installed)
            {
                _stack.Settings.Set("nginx", "DocumentRoot", folder);
            }

            _stack.Settings.Save();

            // the files themselves: httpd.conf's DocumentRoot line and nginx.conf's root line
            IList<string> rewritten = ConfigWriter.Rewrite(_stack, folder);

            // a running web server keeps the folder it started with, so the original restarts it
            RestartWebServers();

            RememberRoot(folder);

            string message = Language.T("Document Root:") + Environment.NewLine + folder;
            if (rewritten.Count > 0)
            {
                message += Environment.NewLine + Language.T("Updated:") + " "
                    + string.Join(", ", new List<string>(rewritten).ConvertAll(Path.GetFileName).ToArray());
            }

            Say(message);
        }

        /// <summary>
        /// FUN_1000adb10: after the folder changed, the web server is asked to start again with
        /// the new configuration. Only a server that was already running is restarted, and only
        /// because the person just clicked the new root; nothing is started here otherwise.
        /// </summary>
        private void RestartWebServers()
        {
            foreach (string key in new[] { "apache", "nginx" })
            {
                Component web = _stack.Find(key);
                if (web == null || !web.Installed || !web.IsRunning)
                {
                    continue;
                }

                _services.Stop(web);
                _services.Start(web);
            }
        }

        // ------------------------------------------------- installed versions (submenus)

        private const string VersionRowPrefix = "Version:";
        private const string VersionDirRow = "dir";

        // the row that closes the list, and the separator after it, carry their own tags so a
        // later fill can take them away again: an untagged row is invisible to the removal loop
        // below and piles up, one more line every time the window refreshes (the person's
        // "the separators are multiplying").
        private const string VersionSepRow = "sep";

        // the greyed row a list with nothing installed carries; its own prefix keeps it out of
        // the version-row count (tools\count.py versions) and still lets a later fill remove it
        private const string VersionNotePrefix = "VersionNote:";

        /// <summary>
        /// One row per installed version folder, under each software's Version submenu.
        /// FUN_100069ba0 (0x100069ba0) is the Node.js copy of one routine the original repeats
        /// once for every software — the thirteen are 0x100056740, 0x100056b10, 0x100059590
        /// (PHP), 0x100059e90, 0x10005aa60, 0x100065e00, 0x100066200, 0x100068b80, 0x100068f60,
        /// 0x1000693a0, 0x1000697a0, 0x100069ba0 (Node.js) and 0x10006c940; each one differs
        /// only in the folder literal it lists ("bin\php" in the PHP copy, "bin\nodejs" here),
        /// the form field holding its submenu (PHP's is +0xc10) and the component record. This is
        /// what they do, read off the decompilation: suspend drawing of the submenu
        /// (FUN_10009bf20), list bin\&lt;subfolder&gt; (FUN_10009a210), and for every entry that
        /// exists as a folder (FUN_10001e550 at 0x10001e550) add a menu row whose caption is the
        /// folder's own name (FUN_100149200) and which keeps that name on itself (FUN_10005fd40);
        /// the row whose name equals the version in use (FUN_1000ab430 gives it, FUN_10000a700 is
        /// the compare, 0 when equal) gets two flags: FUN_1001492a0 and FUN_100149350 with 1 (the
        /// second is the tick; the first is a second flag this round did not name), every other
        /// row gets both with 0. Each row's OnClick is one shared dispatcher, FUN_100059260
        /// (0x100059260) — FUN_1000592d0 for PHP. The list then closes with three rows: a menu
        /// row captioned "-" (the literal at 0x100394c88) whose Enabled is set to false and which
        /// has no OnClick — the original's separator is an ordinary menu row, not a separate
        /// control — then "dir: &lt;version in use&gt;" (built from "dir: " at 0x100395c48, always
        /// Enabled = true, with the folder picture FUN_100147c90/FUN_1001494a0, OnClick
        /// FUN_1000564a0) which opens that folder, and then a second "-" row. Drawing is
        /// resumed (FUN_10009bf90) at the end. Nothing in it removes a row first: the original
        /// appends, which is why it may only be run once per list — see the note on the trigger
        /// over <see cref="HookVersionLists"/> and the rebuild in this method.
        /// </summary>
        private void FillVersionItems(ToolStripMenuItem item, string key)
        {
            if (item == null)
            {
                return;
            }

            Component component = _stack.Find(key);
            string inUse = component != null && component.Installed
                ? Path.GetFileName(component.Folder)
                : string.Empty;

            // Everything this routine ever put here goes first — the version rows, the "dir:"
            // row AND the separator after it — so the list is rebuilt from empty every time
            // instead of growing. The separator the designer declares (mnuSepMnuPHPDir and its
            // brothers) carries no tag and stays exactly where the form put it, which is what
            // makes the original's "rows, -, dir, -" order come out right.
            for (int i = item.DropDownItems.Count - 1; i >= 0; i--)
            {
                string tag = item.DropDownItems[i].Tag as string;
                if (tag != null
                    && (tag.StartsWith(VersionRowPrefix, StringComparison.Ordinal)
                        || tag.StartsWith(VersionNotePrefix, StringComparison.Ordinal)))
                {
                    item.DropDownItems.RemoveAt(i);
                }
            }

            int at = 0;
            if (component != null)
            {
                foreach (string version in component.Versions)
                {
                    ToolStripMenuItem row = new ToolStripMenuItem(version);
                    row.Tag = VersionRowPrefix + key + "|" + version;
                    row.Checked = string.Equals(version, inUse, StringComparison.OrdinalIgnoreCase);
                    row.Click += VersionRowClick;
                    item.DropDownItems.Insert(at++, row);
                }

                // nothing installed: the original's list comes out with no version row at all, which
                // looks like a dead submenu. One greyed row says why, and it is not a version row, so
                // it is not counted as one.
                if (component.Versions.Count == 0)
                {
                    ToolStripMenuItem note = new ToolStripMenuItem(Language.T("No version installed"));
                    note.Tag = VersionNotePrefix + key;
                    note.Enabled = false;
                    item.DropDownItems.Insert(at++, note);
                }
            }

            // then the two rows that close the list: "dir: <version in use>", then the separator.
            // The original sets Enabled = true on this row whatever the in-use folder is
            // (FUN_100149420 with 1 in every one of the thirteen copies), so it stays clickable
            // even when there is nothing installed yet.
            ToolStripMenuItem open = new ToolStripMenuItem("dir: " + inUse);
            open.Tag = VersionRowPrefix + key + "|" + VersionDirRow;
            open.Click += VersionDirClick;
            item.DropDownItems.Add(open);

            ToolStripSeparator close = new ToolStripSeparator();
            close.Tag = VersionRowPrefix + key + "|" + VersionSepRow;
            item.DropDownItems.Add(close);

            // remember what this list was built from (MenuLists.EnsureVersionItems)
            _versionStamps[key] = VersionStamp(key);
        }

        private void VersionRowClick(object sender, EventArgs e)
        {
            VersionRowClicked(sender as ToolStripItem);
        }

        /// <summary>
        /// What a clicked row of a Version submenu does, whichever way the click arrived: the row's
        /// own Click (hover/open path) or the drop-down's DropDownItemClicked (fires even when the
        /// row object was replaced under the mouse).
        /// </summary>
        private void VersionRowClicked(ToolStripItem row)
        {
            string[] parts = SplitVersionTag(row == null ? null : row.Tag as string);
            if (parts == null || parts[1] == VersionDirRow)
            {
                return;
            }

            SwitchVersion(parts[0], parts[1]);
        }

        /// <summary>
        /// Runs the real click path for one version row, without a mouse, and prints what each
        /// step of it produced — the ini, the record, the window's caption and the file on disk.
        /// This is how a version switch is checked; nothing is started (ScreenshotOnly is on).
        /// </summary>
        internal void TryVersion(string key, string version)
        {
            ToolStripMenuItem parent = null;
            foreach (string[] pair in VersionItems)
            {
                if (string.Equals(pair[1], key, StringComparison.OrdinalIgnoreCase))
                {
                    parent = FindItem(pair[0]);
                }
            }

            if (parent == null)
            {
                Console.WriteLine("no Version submenu for " + key);
                return;
            }

            Component before = _stack.Find(key);
            Console.WriteLine("submenu: " + parent.Name + "  caption: " + parent.Text);
            Console.WriteLine("before:  ini=" + _stack.Settings.Get(key, "Version", "<none>")
                + "  folder=" + (before == null ? "<none>" : before.Folder));
            Console.WriteLine("rows:    " + string.Join(", ", RowCaptions(parent)));

            // the same two fills a drop-down does when it is opened twice: the second must leave the
            // rows alone, or the click the person is aiming at lands on a row that no longer exists
            EnsureVersionItems(parent, key);
            ToolStripItem firstRow = parent.DropDownItems.Count > 0 ? parent.DropDownItems[0] : null;
            EnsureVersionItems(parent, key);
            ToolStripItem secondRow = parent.DropDownItems.Count > 0 ? parent.DropDownItems[0] : null;
            Console.WriteLine("filled:  " + string.Join(", ", RowCaptions(parent)));
            Console.WriteLine("stable:  second open kept the same rows: " + object.ReferenceEquals(firstRow, secondRow));

            ToolStripMenuItem row = null;
            foreach (ToolStripItem item in parent.DropDownItems)
            {
                if (string.Equals(item.Tag as string, VersionRowPrefix + key + "|" + version, StringComparison.Ordinal))
                {
                    row = item as ToolStripMenuItem;
                }
            }

            if (row == null)
            {
                Console.WriteLine("no row for " + version + " (not an installed folder, or the row is missing)");
                return;
            }

            // the click itself. Two routes are wired in the form: the row's own Click, and the
            // drop-down's DropDownItemClicked, which still arrives when the row was replaced
            row.PerformClick();
            VersionRowClicked(row);

            Component after = _stack.Find(key);
            Console.WriteLine("after:   ini=" + _stack.Settings.Get(key, "Version", "<none>")
                + "  folder=" + (after == null ? "<none>" : after.Folder)
                + "  version=" + (after == null ? "<none>" : after.Version)
                + "  caption=" + parent.Text);

            // and what a fresh start would read back off the disk
            var fresh = new Stack();
            Console.WriteLine("on disk: ini=" + fresh.Settings.Get(key, "Version", "<none>")
                + "  folder=" + (fresh.Find(key) == null ? "<none>" : fresh.Find(key).Folder));
        }

        private static List<string> RowCaptions(ToolStripMenuItem parent)
        {
            var captions = new List<string>();
            foreach (ToolStripItem item in parent.DropDownItems)
            {
                captions.Add(item is ToolStripSeparator ? "<sep>" : item.Text);
            }

            return captions;
        }

        private void VersionDirClick(object sender, EventArgs e)
        {
            string[] parts = SplitVersionTag(sender == null ? null : (sender as ToolStripItem).Tag as string);
            if (parts == null)
            {
                return;
            }

            Component component = _stack.Find(parts[0]);
            if (component == null || !component.Installed)
            {
                return;
            }

            OpenFolder(component.Folder);
        }

        /// <summary>"Version:php|php-8.4.20-…" -> { "php", "php-8.4.20-…" }.</summary>
        private static string[] SplitVersionTag(string tag)
        {
            if (tag == null || !tag.StartsWith(VersionRowPrefix, StringComparison.Ordinal))
            {
                return null;
            }

            string rest = tag.Substring(VersionRowPrefix.Length);
            int bar = rest.IndexOf('|');
            if (bar <= 0)
            {
                return null;
            }

            return new[] { rest.Substring(0, bar), rest.Substring(bar + 1) };
        }

        // ------------------------------------------------------- site templates

        /// <summary>
        /// Nothing in the original fills MnuSiteTemplate: the item has no caption and no handler
        /// in the decompiled program, and no function reads a template folder into a menu. This
        /// submenu is therefore this rebuild's own addition over the folders of usr\tpl — a row
        /// copies that folder into the document root under the name the person gives, which is
        /// the folder-only half of the original's quick create (FUN_100054500 at 0x100054500).
        /// </summary>
        private void FillSiteTemplateMenu()
        {
            ToolStripMenuItem host = FindItem("MnuQuickAppClick");
            if (host == null)
            {
                return;
            }

            // Whatever the last fill added is taken away first, so calling this twice — the
            // window refreshes it every two seconds, and the menu is also refilled whenever it
            // opens — cannot leave a second separator and a second "Site template" behind.
            // Only our own row and the one separator right above it go: the "-" rows that
            // FillQuickAppMenu puts between its groups are not ours to touch.
            for (int i = host.DropDownItems.Count - 1; i >= 0; i--)
            {
                string tag = host.DropDownItems[i].Tag as string;
                if (tag != null && tag.StartsWith(TemplatePrefix, StringComparison.Ordinal))
                {
                    bool separatorAbove = i > 0 && host.DropDownItems[i - 1] is ToolStripSeparator;
                    host.DropDownItems.RemoveAt(i);
                    if (separatorAbove)
                    {
                        host.DropDownItems.RemoveAt(i - 1);
                    }
                }
            }

            if (host.DropDownItems.Count > 0)
            {
                host.DropDownItems.Add(new ToolStripSeparator());
            }

            ToolStripMenuItem templates = new ToolStripMenuItem(Language.T("Site template"));
            templates.Tag = TemplatePrefix;
            host.DropDownItems.Add(templates);

            string[] found = TemplateFolders();
            if (found.Length == 0)
            {
                ToolStripMenuItem none = new ToolStripMenuItem(Language.T("None"));
                none.Enabled = false;
                templates.DropDownItems.Add(none);
                return;
            }

            foreach (string folder in found)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(Path.GetFileName(folder));
                item.Tag = TemplatePrefix + folder;
                item.Click += TemplateRowClick;
                templates.DropDownItems.Add(item);
            }
        }

        private void TemplateRowClick(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            string tag = item == null ? null : item.Tag as string;
            if (tag == null || !tag.StartsWith(TemplatePrefix, StringComparison.Ordinal))
            {
                return;
            }

            string template = tag.Substring(TemplatePrefix.Length);
            if (!Directory.Exists(template))
            {
                Say(Unavailable);
                return;
            }

            string name = Ask("New Project", "Project name", Path.GetFileName(template), false);
            if (name == null)
            {
                return;
            }

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

            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                Say("The name is not valid! " + name);
                return;
            }

            name = name.Replace(' ', '-');
            string target = Path.Combine(_stack.Www, name);
            if (Directory.Exists(target))
            {
                Say("Project exists!\r\nIf you really want to create the project, delete the project's folder and try again");
                return;
            }

            try
            {
                CopyFolder(template, target);
                Say("Created: " + target);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Brand.Name, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>The folders of usr\tpl: a template the original could copy into www.</summary>
        private string[] TemplateFolders()
        {
            try
            {
                string tpl = Path.Combine(_stack.Root, "usr", "tpl");
                if (!Directory.Exists(tpl))
                {
                    return new string[0];
                }

                List<string> folders = new List<string>(Directory.GetDirectories(tpl));
                folders.Sort(StringComparer.OrdinalIgnoreCase);
                return folders.ToArray();
            }
            catch (Exception)
            {
                return new string[0];
            }
        }

        /// <summary>Copies a folder and everything under it, without following links twice.</summary>
        private static void CopyFolder(string from, string to)
        {
            Directory.CreateDirectory(to);
            foreach (string file in Directory.GetFiles(from))
            {
                File.Copy(file, Path.Combine(to, Path.GetFileName(file)), true);
            }

            foreach (string folder in Directory.GetDirectories(from))
            {
                CopyFolder(folder, Path.Combine(to, Path.GetFileName(folder)));
            }
        }
    }
}
