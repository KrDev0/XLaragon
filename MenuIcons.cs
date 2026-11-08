using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Laragon
{
    /// <summary>
    /// The small pictures that stand in front of the rows of the tray menu.
    ///
    /// They are drawn here with GDI+ rather than loaded from <c>Images\</c> or a resource file:
    /// those belong to the owner's hand edits, and a picture drawn in code cannot go missing,
    /// cannot be overwritten by a designer save, and keeps the program to one file per concern.
    /// Every picture is 16x16, the size WinForms gives a menu row by default.
    /// </summary>
    internal static class MenuGlyphs
    {
        private static readonly Dictionary<string, Image> Cache =
            new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase);

        /// <summary>The picture for a name, made once and then handed out again.</summary>
        internal static Image Get(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            Image picture;
            if (Cache.TryGetValue(key, out picture))
            {
                return picture;
            }

            picture = Build(key);
            Cache.Add(key, picture);
            return picture;
        }

        private static Image Build(string key)
        {
            switch (key.ToLowerInvariant())
            {
                case "logo":
                    return Icons.Logo(16, 16);
                case "globe":
                    return Icons.Globe(16);
                case "database":
                    return Icons.Database(16);
                case "terminal":
                    return Icons.Terminal(16);
                case "folder":
                    return Icons.Folder(16);
                case "gear":
                    return Icons.Gear(16, Color.FromArgb(104, 112, 124));
                case "play":
                    return Icons.Play(16, Color.FromArgb(58, 142, 62));
                case "stop":
                    return Icons.Stop(14, Color.FromArgb(183, 28, 28));
                case "cloud":
                    return Cloud();
                case "person":
                    return Person();
                case "bolt":
                    return Bolt();
                case "exit":
                    return Exit();
                case "tools":
                    return Wrench();
                case "log":
                    return Page(Color.FromArgb(126, 148, 176), false);
                case "cmd":
                    return Page(Color.FromArgb(96, 168, 104), true);
                default:
                    return Page(Color.FromArgb(150, 158, 168), false);
            }
        }

        private static Bitmap Draw(Action<Graphics> paint)
        {
            var bitmap = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                paint(g);
            }

            return bitmap;
        }

        /// <summary>A white page with a coloured spine and a few lines of writing.</summary>
        private static Bitmap Page(Color accent, bool prompt)
        {
            return Draw(g =>
            {
                using (var paper = new SolidBrush(Color.FromArgb(252, 252, 252)))
                using (var edge = new Pen(Color.FromArgb(146, 154, 166), 1f))
                using (var spine = new SolidBrush(accent))
                using (var ink = new Pen(Color.FromArgb(150, 158, 168), 1f))
                {
                    g.FillRectangle(paper, 3, 1, 10, 14);
                    g.DrawRectangle(edge, 3, 1, 9, 13);
                    g.FillRectangle(spine, 3, 1, 3, 14);

                    if (prompt)
                    {
                        using (var mark = new Pen(accent, 1.4f))
                        {
                            g.DrawLine(mark, 8, 6, 10, 8);
                            g.DrawLine(mark, 10, 8, 8, 10);
                            g.DrawLine(mark, 11, 11, 13, 11);
                        }
                    }
                    else
                    {
                        g.DrawLine(ink, 8, 5, 12, 5);
                        g.DrawLine(ink, 8, 8, 12, 8);
                        g.DrawLine(ink, 8, 11, 11, 11);
                    }
                }
            });
        }

        /// <summary>Two soft humps and a base: the shape a menu row uses for "share".</summary>
        private static Bitmap Cloud()
        {
            return Draw(g =>
            {
                using (var body = new SolidBrush(Color.FromArgb(168, 198, 232)))
                using (var edge = new Pen(Color.FromArgb(98, 132, 178), 1f))
                {
                    g.FillEllipse(body, 1, 6, 8, 7);
                    g.FillEllipse(body, 5, 3, 9, 9);
                    g.FillEllipse(body, 9, 7, 6, 6);
                    g.FillRectangle(body, 3, 10, 11, 3);
                    g.DrawArc(edge, 1, 6, 8, 7, 90, 180);
                    g.DrawArc(edge, 5, 3, 9, 9, 180, 150);
                    g.DrawArc(edge, 9, 7, 6, 6, 270, 110);
                    g.DrawLine(edge, 3, 13, 13, 13);
                }
            });
        }

        /// <summary>A head and shoulders, for the profile row.</summary>
        private static Bitmap Person()
        {
            return Draw(g =>
            {
                using (var body = new SolidBrush(Color.FromArgb(74, 111, 165)))
                {
                    g.FillEllipse(body, 5, 1, 6, 6);
                    g.FillEllipse(body, 2, 9, 12, 11);
                }
            });
        }

        /// <summary>A lightning bolt, for a quick action.</summary>
        private static Bitmap Bolt()
        {
            return Draw(g =>
            {
                using (var body = new SolidBrush(Color.FromArgb(226, 160, 56)))
                {
                    g.FillPolygon(body, new[]
                    {
                        new Point(9, 1), new Point(4, 9), new Point(8, 9),
                        new Point(6, 15), new Point(12, 7), new Point(8, 7)
                    });
                }
            });
        }

        /// <summary>A door with an arrow leaving it, for the exit row.</summary>
        private static Bitmap Exit()
        {
            return Draw(g =>
            {
                using (var door = new Pen(Color.FromArgb(96, 104, 116), 1.4f))
                using (var leaving = new Pen(Color.FromArgb(178, 59, 59), 1.6f))
                using (var head = new SolidBrush(Color.FromArgb(178, 59, 59)))
                {
                    g.DrawRectangle(door, 2, 2, 6, 12);
                    g.DrawLine(leaving, 8, 8, 13, 8);
                    g.FillPolygon(head, new[]
                    {
                        new Point(15, 8), new Point(11, 5), new Point(11, 11)
                    });
                }
            });
        }

        /// <summary>A spanner, for the tools row.</summary>
        private static Bitmap Wrench()
        {
            return Draw(g =>
            {
                using (var steel = new Pen(Color.FromArgb(112, 122, 136), 3f))
                using (var ring = new Pen(Color.FromArgb(112, 122, 136), 1.6f))
                {
                    g.DrawLine(steel, 4, 13, 10, 7);
                    g.DrawEllipse(ring, 9, 2, 6, 6);
                }
            });
        }
    }

    public partial class MainForm
    {
        /// <summary>
        /// Gives the tray menu its pictures: the rows the person opens first (the menu's own
        /// top level) and the rows of the "Laragon" submenu that name a service, a folder or a
        /// file. Rows the owner's design already settled keep exactly what they had.
        /// </summary>
        internal void ApplyMenuIcons()
        {
            // the window's own menu: every row, and the rows of every submenu below it
            if (mnuLaragon != null)
            {
                Iconise(mnuLaragon.Items, 0);
            }

            // the tray popup: in this window it is the very same menu (ContextMenuStrip is set to
            // mnuLaragon), so it is only walked again when it is a different one
            if (ContextMenuStrip != null && !ReferenceEquals(ContextMenuStrip, mnuLaragon))
            {
                Iconise(ContextMenuStrip.Items, 0);
            }
        }

        /// <summary>
        /// Pictures every captioned row of a menu, then does the same for the rows of each
        /// submenu down to <paramref name="depth"/> levels. A separator has no caption, so it
        /// is left alone and keeps drawing as a line.
        /// </summary>
        /// <summary>
        /// Pictures the rows a fill has just put into a submenu. The rows a list carries are
        /// made while the menu opens, long after <see cref="ApplyMenuIcons"/> has run, so each
        /// fill asks for them to be pictured again.
        /// </summary>
        internal void IconiseMenu(ToolStripMenuItem menu)
        {
            if (menu != null)
            {
                Iconise(menu.DropDownItems, 1);
            }
        }

        private void Iconise(ToolStripItemCollection items, int depth)
        {
            if (items == null || depth > 6)
            {
                return;
            }

            foreach (ToolStripItem item in items)
            {
                // a separator is a row without a caption: it has nothing to put a picture on
                if (string.IsNullOrEmpty(item.Text) || item.Text == "-")
                {
                    continue;
                }

                Image picture = MenuGlyphs.Get(PictureFor(item));
                if (picture != null)
                {
                    item.Image = picture;
                }

                var menu = item as ToolStripMenuItem;
                if (menu != null && menu.DropDownItems.Count > 0)
                {
                    Iconise(menu.DropDownItems, depth + 1);
                }
            }
        }

        /// <summary>
        /// Which picture a row gets. The name a designer gave the row decides first (it does
        /// not change with the language), and the caption is only a fallback.
        /// </summary>
        internal static string PictureFor(ToolStripItem item)
        {
            string tag = item.Tag as string ?? string.Empty;
            string mark = (item.Name + " " + tag).ToLowerInvariant();

            // rows the program itself adds: the first word of the tag says what they are
            if (tag.StartsWith("Www|", StringComparison.OrdinalIgnoreCase))
            {
                return "globe";
            }

            if (tag.StartsWith("Share|", StringComparison.OrdinalIgnoreCase))
            {
                return "cloud";
            }

            if (tag.StartsWith("SiteTemplate|", StringComparison.OrdinalIgnoreCase))
            {
                return "folder";
            }

            if (tag.StartsWith("Region|", StringComparison.OrdinalIgnoreCase))
            {
                return "globe";
            }

            if (tag.StartsWith("Root|", StringComparison.OrdinalIgnoreCase))
            {
                return "folder";
            }

            if (tag.StartsWith("QuickApp|", StringComparison.OrdinalIgnoreCase))
            {
                return "page";
            }

            if (mark.Contains("mnularagonclick"))
            {
                return "logo";
            }

            if (mark.Contains("mnularagonlogclick"))
            {
                return "log";
            }

            if (mark.Contains("mnularagoniniclick"))
            {
                return "gear";
            }

            if (mark.Contains("mnumenularagondiruserclick") || mark.Contains("mnuroot2click"))
            {
                return "folder";
            }

            if (mark.Contains("mnuuserprofilecmdclick"))
            {
                return "cmd";
            }

            if (mark.Contains("mnuquickappclick"))
            {
                return "bolt";
            }

            if (mark.Contains("mnuquickcloudserverclick") || mark.Contains("mnushareclick"))
            {
                return "cloud";
            }

            if (mark.Contains("mnutoolsclick"))
            {
                return "tools";
            }

            if (mark.Contains("mnustartallclick"))
            {
                return "play";
            }

            if (mark.Contains("mnuexitclick"))
            {
                return "exit";
            }

            if (mark.Contains("mnupreferencesclick"))
            {
                return "gear";
            }

            if (mark.Contains("mnudatabase2click") || mark.Contains("mnysqlclick")
                || mark.Contains("mariadb"))
            {
                return "database";
            }

            if (mark.Contains("mnushell2click") || mark.Contains("terminal"))
            {
                return "terminal";
            }

            if (mark.Contains("mnuweb2click") || mark.Contains("mnuwwwclick"))
            {
                return "globe";
            }

            if (mark.Contains("mnularavelclick") || mark.Contains("mnujavaclick"))
            {
                return "page";
            }

            if (mark.Contains("mnuprofileclick"))
            {
                return "person";
            }

            // last resort: read the caption, which is in the person's language
            string text = (item.Text ?? string.Empty).ToLowerInvariant();
            if (text.StartsWith("dir:", StringComparison.Ordinal))
            {
                return "folder";
            }

            if (text.Contains("laragon.ini") || text.Contains("setting file"))
            {
                return "gear";
            }

            if (text.Contains("laragon.log") || text.EndsWith(".log", StringComparison.Ordinal))
            {
                return "log";
            }

            if (text.Contains(".cmd") || text.Contains("procfile"))
            {
                return "cmd";
            }

            if (text.Contains("database"))
            {
                return "database";
            }

            if (text.Contains("terminal"))
            {
                return "terminal";
            }

            if (text.Contains("web") || text.Contains("www"))
            {
                return "globe";
            }

            if (text.Contains("cloud"))
            {
                return "cloud";
            }

            if (text.StartsWith("start", StringComparison.Ordinal))
            {
                return "play";
            }

            if (text.StartsWith("stop", StringComparison.Ordinal))
            {
                return "stop";
            }

            if (text.Contains("preferences") || text.Contains("tools"))
            {
                return "gear";
            }

            if (text == "exit" || text == "quit" || text == "salir"
                || text.StartsWith("exit ", StringComparison.Ordinal)
                || text.StartsWith("quit ", StringComparison.Ordinal))
            {
                return "exit";
            }

            if (text.Contains("profile"))
            {
                return "person";
            }

            // the rows that only name a language, a program or a way in
            if (text == "php" || text == "wordpress" || text == "laravel" || text == "python"
                || text == "ruby" || text == "node.js" || text == "go" || text == "java"
                || text == "docker" || text == "kubernetes")
            {
                return "page";
            }

            if (text.Contains("ssh"))
            {
                return "terminal";
            }

            if (text.Contains("linux"))
            {
                return "cloud";
            }

            if (text.Contains("sublime") || text.Contains("notepad") || text.Contains("winscp")
                || text.Contains("netstat") || text.Contains("path") || text.Contains("ruta"))
            {
                return "tools";
            }

            return null;
        }
    }
}
