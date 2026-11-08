using System;
using System.Drawing;
using System.Windows.Forms;

namespace Laragon
{
    /// <summary>
    /// The one small question box this rebuild needs: the original asks for the database name
    /// and for the new root password in a modal input dialog, and there is no built-in one.
    ///
    /// Every control is declared in <c>AskBox.Designer.cs</c>, so the window can be opened and
    /// rearranged in the Visual Studio designer like any other form; this file only says what
    /// the box is asked and answers.
    /// </summary>
    internal partial class AskBox : Form
    {
        /// <summary>What the designer builds; it asks nothing and answers nothing.</summary>
        public AskBox()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The box the program opens: a title, the line above the field, the field's first text
        /// and whether what is typed has to be hidden (a password).
        /// </summary>
        internal AskBox(string title, string label, string initial, bool secret)
            : this()
        {
            Text = title;
            lblCaption.Text = label;
            txtValue.Text = initial ?? string.Empty;
            txtValue.UseSystemPasswordChar = secret;

            // the designer holds the English words; put them in the chosen language here
            btnOK.Text = Language.T("OK");
            btnCancel.Text = Language.T("Cancel");
        }

        /// <summary>What the person typed.</summary>
        internal string Value
        {
            get { return txtValue.Text; }
        }

        /// <summary>
        /// False at design time and in the screenshot run: the field must not steal the focus
        /// while the designer is laying the box out on screen.
        /// </summary>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (DesignTime.IsDesigning || Program.ScreenshotOnly)
            {
                return;
            }

            txtValue.SelectAll();
            txtValue.Focus();
        }
    }
}
