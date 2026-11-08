namespace Laragon
{
    /// <summary>
    /// The layout of the "About XLaragon" window, which the Help label at the top of the main
    /// window opens.
    ///
    /// Every control is declared here rather than made in code, so the whole window can be
    /// dragged, restyled and rewritten in the Visual Studio designer: the designer only shows a
    /// form whose controls live in a partial class with InitializeComponent like this one. The
    /// texts written below are the English words the person reads; AboutForm.cs only fills in
    /// the values that change at run time (the folder in use, the settings file, the program
    /// name and version) and lets Language.T translate the sentence list.
    /// </summary>
    partial class AboutForm
    {
        /// <summary>Required designer variable.</summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>Clean up any resources being used.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify the contents of this method with
        /// the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutForm));
            picLogo = new PictureBox();
            lblProduct = new Label();
            lblVersion = new Label();
            lblTagline = new Label();
            lineTop = new Panel();
            lblRootCaption = new Label();
            lblRootValue = new Label();
            lblIniCaption = new Label();
            lblIniValue = new Label();
            lblProgramCaption = new Label();
            lblProgramValue = new Label();
            lblButtonsCaption = new Label();
            lblButtonText = new Label();
            btnOpenFolder = new Button();
            btnClose = new Button();
            ((System.ComponentModel.ISupportInitialize)picLogo).BeginInit();
            SuspendLayout();
            // 
            // picLogo
            // 
            picLogo.Image = (Image)resources.GetObject("picLogo.Image");
            picLogo.Location = new Point(18, 16);
            picLogo.Name = "picLogo";
            picLogo.Size = new Size(64, 64);
            picLogo.SizeMode = PictureBoxSizeMode.Zoom;
            picLogo.TabIndex = 0;
            picLogo.TabStop = false;
            // 
            // lblProduct
            // 
            lblProduct.Font = new Font("Arial", 20.25F, FontStyle.Bold);
            lblProduct.Location = new Point(92, 14);
            lblProduct.Name = "lblProduct";
            lblProduct.Size = new Size(300, 32);
            lblProduct.TabIndex = 1;
            lblProduct.Text = "XLaragon";
            // 
            // lblVersion
            // 
            lblVersion.Font = new Font("Arial", 9F);
            lblVersion.Location = new Point(95, 46);
            lblVersion.Name = "lblVersion";
            lblVersion.Size = new Size(300, 18);
            lblVersion.TabIndex = 2;
            lblVersion.Text = "Version 1.0.0";
            // 
            // lblTagline
            // 
            lblTagline.Font = new Font("Arial", 8.25F);
            lblTagline.ForeColor = Color.DimGray;
            lblTagline.Location = new Point(95, 66);
            lblTagline.Name = "lblTagline";
            lblTagline.Size = new Size(360, 18);
            lblTagline.TabIndex = 3;
            lblTagline.Text = "Apache, Nginx, MySQL and PHP in one folder, started from one window.";
            // 
            // lineTop
            // 
            lineTop.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lineTop.BackColor = Color.FromArgb(216, 216, 216);
            lineTop.Location = new Point(18, 100);
            lineTop.Name = "lineTop";
            lineTop.Size = new Size(432, 1);
            lineTop.TabIndex = 4;
            // 
            // lblRootCaption
            // 
            lblRootCaption.Font = new Font("Arial", 9F);
            lblRootCaption.Location = new Point(18, 110);
            lblRootCaption.Name = "lblRootCaption";
            lblRootCaption.Size = new Size(110, 18);
            lblRootCaption.TabIndex = 5;
            lblRootCaption.Text = "Document root";
            // 
            // lblRootValue
            // 
            lblRootValue.AutoEllipsis = true;
            lblRootValue.Font = new Font("Arial", 9F);
            lblRootValue.Location = new Point(130, 110);
            lblRootValue.Name = "lblRootValue";
            lblRootValue.Size = new Size(320, 18);
            lblRootValue.TabIndex = 6;
            lblRootValue.Text = "(read when the window opens)";
            // 
            // lblIniCaption
            // 
            lblIniCaption.Font = new Font("Arial", 9F);
            lblIniCaption.Location = new Point(18, 132);
            lblIniCaption.Name = "lblIniCaption";
            lblIniCaption.Size = new Size(110, 18);
            lblIniCaption.TabIndex = 7;
            lblIniCaption.Text = "Settings file";
            // 
            // lblIniValue
            // 
            lblIniValue.AutoEllipsis = true;
            lblIniValue.Font = new Font("Arial", 9F);
            lblIniValue.Location = new Point(130, 132);
            lblIniValue.Name = "lblIniValue";
            lblIniValue.Size = new Size(320, 18);
            lblIniValue.TabIndex = 8;
            lblIniValue.Text = "(read when the window opens)";
            // 
            // lblProgramCaption
            // 
            lblProgramCaption.Font = new Font("Arial", 9F);
            lblProgramCaption.Location = new Point(18, 154);
            lblProgramCaption.Name = "lblProgramCaption";
            lblProgramCaption.Size = new Size(110, 18);
            lblProgramCaption.TabIndex = 9;
            lblProgramCaption.Text = "Program";
            // 
            // lblProgramValue
            // 
            lblProgramValue.AutoEllipsis = true;
            lblProgramValue.Font = new Font("Arial", 9F);
            lblProgramValue.Location = new Point(130, 154);
            lblProgramValue.Name = "lblProgramValue";
            lblProgramValue.Size = new Size(320, 18);
            lblProgramValue.TabIndex = 10;
            lblProgramValue.Text = "(read when the window opens)";
            // 
            // lblButtonsCaption
            // 
            lblButtonsCaption.Font = new Font("Arial", 9F, FontStyle.Bold);
            lblButtonsCaption.Location = new Point(18, 182);
            lblButtonsCaption.Name = "lblButtonsCaption";
            lblButtonsCaption.Size = new Size(200, 18);
            lblButtonsCaption.TabIndex = 11;
            lblButtonsCaption.Text = "What the buttons at the top do";
            // 
            // lblButtonText
            // 
            lblButtonText.Font = new Font("Arial", 8.25F);
            lblButtonText.Location = new Point(18, 202);
            lblButtonText.Name = "lblButtonText";
            lblButtonText.Size = new Size(432, 104);
            lblButtonText.TabIndex = 12;
            lblButtonText.Text = resources.GetString("lblButtonText.Text");
            // 
            // btnOpenFolder
            // 
            btnOpenFolder.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnOpenFolder.Location = new Point(248, 318);
            btnOpenFolder.Name = "btnOpenFolder";
            btnOpenFolder.Size = new Size(118, 26);
            btnOpenFolder.TabIndex = 13;
            btnOpenFolder.Text = "Open folder";
            btnOpenFolder.UseVisualStyleBackColor = true;
            btnOpenFolder.Click += btnOpenFolder_Click;
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnClose.DialogResult = DialogResult.Cancel;
            btnClose.Location = new Point(372, 318);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(78, 26);
            btnClose.TabIndex = 14;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            // 
            // AboutForm
            // 
            AutoScaleMode = AutoScaleMode.None;
            BackColor = Color.White;
            CancelButton = btnClose;
            ClientSize = new Size(468, 356);
            Controls.Add(btnClose);
            Controls.Add(btnOpenFolder);
            Controls.Add(lblButtonText);
            Controls.Add(lblButtonsCaption);
            Controls.Add(lblProgramValue);
            Controls.Add(lblProgramCaption);
            Controls.Add(lblIniValue);
            Controls.Add(lblIniCaption);
            Controls.Add(lblRootValue);
            Controls.Add(lblRootCaption);
            Controls.Add(lineTop);
            Controls.Add(lblTagline);
            Controls.Add(lblVersion);
            Controls.Add(lblProduct);
            Controls.Add(picLogo);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AboutForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "About XLaragon";
            ((System.ComponentModel.ISupportInitialize)picLogo).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.PictureBox picLogo;
        private System.Windows.Forms.Label lblProduct;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.Label lblTagline;
        private System.Windows.Forms.Panel lineTop;
        private System.Windows.Forms.Label lblRootCaption;
        private System.Windows.Forms.Label lblRootValue;
        private System.Windows.Forms.Label lblIniCaption;
        private System.Windows.Forms.Label lblIniValue;
        private System.Windows.Forms.Label lblProgramCaption;
        private System.Windows.Forms.Label lblProgramValue;
        private System.Windows.Forms.Label lblButtonsCaption;
        private System.Windows.Forms.Label lblButtonText;
        private System.Windows.Forms.Button btnOpenFolder;
        private System.Windows.Forms.Button btnClose;
    }
}
