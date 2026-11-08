using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Laragon
{
    /// <summary>
    /// Neutral placeholder artwork drawn with GDI+ (circles, bars, a leaf): the original
    /// program's own icons and logo are not copied.
    /// </summary>
    internal static class Icons
    {
        public static Bitmap Logo(int width, int height)
        {
            var bitmap = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var back = new SolidBrush(Color.FromArgb(52, 58, 54)))
                {
                    g.FillRectangle(back, 0, 0, width, height);
                }

                int cx = width / 2;
                int cy = height / 2;
                using (var leaf = new SolidBrush(Color.FromArgb(124, 196, 74)))
                {
                    var petal = new Point[]
                    {
                        new Point(cx, cy - height / 4),
                        new Point(cx + width / 4, cy),
                        new Point(cx, cy + height / 4),
                        new Point(cx - width / 4, cy)
                    };
                    g.FillPolygon(leaf, petal);
                }

                using (var stem = new Pen(Color.FromArgb(236, 240, 232), Math.Max(1f, width / 24f)))
                {
                    g.DrawLine(stem, cx, cy, cx, cy + height / 3);
                }
            }

            return bitmap;
        }

        public static Bitmap Globe(int size)
        {
            var bitmap = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(Color.FromArgb(0, 102, 204), 1.6f))
                {
                    g.DrawEllipse(pen, 1, 1, size - 3, size - 3);
                    g.DrawEllipse(pen, size / 4f, 1, size / 2f, size - 3);
                    g.DrawLine(pen, 1, size / 2f, size - 2, size / 2f);
                }
            }

            return bitmap;
        }

        public static Bitmap Database(int size)
        {
            var bitmap = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(Color.FromArgb(120, 130, 200)))
                using (var pen = new Pen(Color.FromArgb(60, 68, 130), 1.4f))
                {
                    int h = size / 4;
                    g.FillEllipse(brush, 1, 1, size - 3, h);
                    g.DrawEllipse(pen, 1, 1, size - 3, h);
                    g.FillRectangle(brush, 1, h / 2 + 1, size - 3, size - h - 2);
                    g.DrawLine(pen, 1, h / 2 + 1, 1, size - h / 2);
                    g.DrawLine(pen, size - 2, h / 2 + 1, size - 2, size - h / 2);
                    g.FillEllipse(brush, 1, size - h - 1, size - 3, h);
                    g.DrawEllipse(pen, 1, size - h - 1, size - 3, h);
                    g.DrawArc(pen, 1, size / 2 - h / 2, size - 3, h, 0, 180);
                }
            }

            return bitmap;
        }

        public static Bitmap Terminal(int size)
        {
            var bitmap = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                using (var back = new SolidBrush(Color.FromArgb(32, 34, 38)))
                {
                    g.FillRectangle(back, 1, 2, size - 2, size - 4);
                }

                using (var pen = new Pen(Color.FromArgb(132, 220, 120), 1.8f))
                {
                    g.DrawLine(pen, size * 0.22f, size * 0.32f, size * 0.42f, size * 0.5f);
                    g.DrawLine(pen, size * 0.42f, size * 0.5f, size * 0.22f, size * 0.68f);
                    g.DrawLine(pen, size * 0.52f, size * 0.68f, size * 0.76f, size * 0.68f);
                }
            }

            return bitmap;
        }

        public static Bitmap Folder(int size)
        {
            var bitmap = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(Color.FromArgb(240, 190, 90)))
                {
                    g.FillRectangle(brush, 1, size * 0.28f, size - 2, size * 0.55f);
                    g.FillRectangle(brush, 1, size * 0.2f, size * 0.45f, size * 0.14f);
                }
            }

            return bitmap;
        }

        public static Bitmap Play(int size, Color colour)
        {
            var bitmap = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(colour))
                {
                    g.FillPolygon(brush, new[]
                    {
                        new Point(size / 4, size / 6),
                        new Point(size - size / 5, size / 2),
                        new Point(size / 4, size - size / 6)
                    });
                }
            }

            return bitmap;
        }

        public static Bitmap Stop(int size, Color colour)
        {
            var bitmap = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(colour))
                {
                    g.FillRectangle(brush, size / 4, size / 4, size / 2, size / 2);
                }
            }

            return bitmap;
        }

        public static Bitmap Gear(int size, Color colour)
        {
            var bitmap = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(colour, 2f))
                {
                    float radius = size / 2f - 3;
                    g.DrawEllipse(pen, size / 2f - radius, size / 2f - radius, radius * 2, radius * 2);
                    g.DrawEllipse(pen, size * 0.36f, size * 0.36f, size * 0.28f, size * 0.28f);
                    for (int i = 0; i < 8; i++)
                    {
                        double angle = Math.PI * i / 4.0;
                        float x1 = (float)(size / 2f + Math.Cos(angle) * radius);
                        float y1 = (float)(size / 2f + Math.Sin(angle) * radius);
                        float x2 = (float)(size / 2f + Math.Cos(angle) * (radius + 2.5f));
                        float y2 = (float)(size / 2f + Math.Sin(angle) * (radius + 2.5f));
                        g.DrawLine(pen, x1, y1, x2, y2);
                    }
                }
            }

            return bitmap;
        }

        public static Icon AppIcon()
        {
            using (Bitmap bitmap = Logo(32, 32))
            {
                IntPtr handle = bitmap.GetHicon();
                return Icon.FromHandle(handle);
            }
        }
    }
}
