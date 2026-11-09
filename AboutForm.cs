using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace Laragon
{
    /// <summary>
    /// The "About XLaragon" window, which the Help label in the top right of the main window
    /// opens. The original has no such window (no "About" caption exists anywhere in
    /// laragon.exe), so this is the person's own addition rather than a copy of one of the
    /// original's forms: every word it shows is in AboutForm.Designer.cs, where it can be
    /// changed in the designer, and only the three values that are different on each computer
    /// (the document root, the settings file and the program with its version) are filled in
    /// here at run time.
    /// </summary>
    public partial class AboutForm : Form
    {
        /// <summary>The folders and the ini to show. May be null when the designer is open.</summary>
        private readonly Stack _stack;

        /// <summary>For the Visual Studio designer, which builds the form with no arguments.</summary>
        public AboutForm()
        {
            InitializeComponent();
        }

        /// <summary>Opened by the Help label: shows this computer's folders and version.</summary>
        public AboutForm(Stack stack)
            : this()
        {
            _stack = stack;
            Fill();
        }

        /// <summary>
        /// The few values that cannot be written in the designer because they depend on where
        /// the program sits and which version was built.
        /// </summary>
        private void Fill()
        {
            Text = Brand.Fix(Text);
            lblProduct.Text = Brand.Name;
            lblVersion.Text = Language.T("Version") + " " + VersionText();
            lblTagline.Text = Brand.Fix(Language.T(lblTagline.Text));

            if (_stack != null)
            {
                lblRootValue.Text = _stack.Root + " (" + Language.T(_stack.RootSource) + ")";
                lblIniValue.Text = _stack.Settings.FilePath;
            }

            lblProgramValue.Text = Path.GetFileName(Application.ExecutablePath)
                + ", .NET " + Environment.Version.ToString(2);

        }

        private static string VersionText()
        {
            try
            {
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                if (version != null)
                {
                    return version.Major + "." + version.Minor + "." + version.Build;
                }
            }
            catch
            {
            }

            return Application.ProductVersion;
        }

        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            try
            {
                string folder = Path.GetDirectoryName(Application.ExecutablePath);
                if (!string.IsNullOrEmpty(folder) && Directory.Exists(folder))
                {
                    Process.Start(new ProcessStartInfo(folder) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Brand.Name, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
