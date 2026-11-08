using System;
using System.Windows.Forms;

namespace Laragon
{
    public partial class MainForm
    {
        /// <summary>
        /// Puts the product's own name in front of the person: the title bar, the tray icon's
        /// tooltip and balloon, and every caption and menu row that still says "Laragon".
        ///
        /// Run from <see cref="OnLoad"/>, so it happens after the language has been applied
        /// (a pack's Spanish is looked up by the original English caption, which this would
        /// otherwise hide) and before the window is first drawn.
        /// </summary>
        internal void ApplyBranding()
        {
            Text = Brand.Fix(Text);

            if (notifyIcon != null)
            {
                notifyIcon.Text = Brand.Name;
                notifyIcon.BalloonTipText = Brand.Name;
            }

            BrandCaptions(this);

            if (ContextMenuStrip != null)
            {
                BrandItems(ContextMenuStrip.Items);
            }
        }

        /// <summary>Rebrands the captions of a window's controls, and of their menus.</summary>
        internal static void BrandCaptions(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                // only controls that carry a caption: a TextBox or a grid holds a value, and a
                // path in one of those is lower case anyway and must stay as it is
                if (child is Label || child is Button || child is CheckBox || child is RadioButton
                    || child is GroupBox || child is TabPage || child is LinkLabel)
                {
                    child.Text = Brand.Fix(child.Text);
                }

                if (child.ContextMenuStrip != null)
                {
                    BrandItems(child.ContextMenuStrip.Items);
                }

                BrandCaptions(child);
            }
        }

        /// <summary>Rebrands every row of a menu, down through its submenus.</summary>
        internal static void BrandItems(ToolStripItemCollection items)
        {
            foreach (ToolStripItem item in items)
            {
                item.Text = Brand.Fix(item.Text);
                item.ToolTipText = Brand.Fix(item.ToolTipText);

                var menu = item as ToolStripMenuItem;
                if (menu != null && menu.DropDownItems.Count > 0)
                {
                    BrandItems(menu.DropDownItems);
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ApplyBranding();
        }
    }
}
