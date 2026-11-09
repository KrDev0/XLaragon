using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Laragon
{
    /// <summary>
    /// The small on/off switch each service row starts with — a 23 x 14 rounded track with a
    /// round knob, the knob on the right when the service takes part in "Start All" and on the
    /// left when it does not.
    ///
    /// It is a CheckBox underneath, so the form designer treats it like any other control
    /// (Checked, Enabled, Click all behave as usual) and the rebuilding code can keep using one
    /// type for the whole row list. Only the painting is ours: the person's pictures show a
    /// switch, not a tick box with the word "Enabled" beside it.
    ///
    /// Evidence for the shape, from reference\picture-1.png measured pixel by pixel: the track
    /// is 23 x 14 at client x 121 (the row's own x) and the knob sits at its right end on the
    /// Apache and Nginx rows and at its left end on the MySQL row.
    /// </summary>
    internal sealed class PillSwitch : CheckBox
    {
        private bool _running;

        public PillSwitch()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            Text = string.Empty;
            Size = new Size(23, 14);
            Cursor = Cursors.Hand;
            TabStop = false;
        }

        /// <summary>
        /// Whether the service is running right now. The knob still shows the switch itself; this
        /// only colours the track, so the row keeps saying "up" or "down" now that the separate
        /// status dot of the earlier layout is gone.
        /// </summary>
        public bool Running
        {
            get { return _running; }
            set
            {
                if (_running == value)
                {
                    return;
                }

                _running = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle track = new Rectangle(0, 0, Width - 1, Height - 1);
            int radius = Height - 1;

            Color fill;
            if (!Enabled)
            {
                fill = Checked ? Color.FromArgb(196, 205, 196) : Color.FromArgb(226, 226, 226);
            }
            else if (Checked)
            {
                fill = _running ? Color.FromArgb(76, 175, 80) : Color.FromArgb(122, 122, 122);
            }
            else
            {
                fill = Color.FromArgb(205, 205, 205);
            }

            using (var path = new GraphicsPath())
            {
                path.AddArc(track.X, track.Y, radius, radius, 90, 180);
                path.AddArc(track.Right - radius, track.Y, radius, radius, 270, 180);
                path.CloseFigure();

                using (var brush = new SolidBrush(fill))
                {
                    g.FillPath(brush, path);
                }

                using (var pen = new Pen(Color.FromArgb(24, 0, 0, 0)))
                {
                    g.DrawPath(pen, path);
                }
            }

            int knob = Height - 5;
            int knobX = Checked ? Width - knob - 3 : 3;
            using (var brush = new SolidBrush(Color.White))
            {
                g.FillEllipse(brush, knobX, 2, knob, knob);
            }

            using (var pen = new Pen(Color.FromArgb(40, 0, 0, 0)))
            {
                g.DrawEllipse(pen, knobX, 2, knob, knob);
            }
        }
    }
}
