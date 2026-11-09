namespace Laragon
{
    partial class PreferencesForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.ToolTip tipHints;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PreferencesForm));
            panelButtons = new Panel();
            panelButtonsLine = new Panel();
            btnOK = new Button();
            btnCancel = new Button();
            btnApply = new Button();
            lblSettingsFile = new Label();
            tabPrefs = new TabControl();
            tabGeneral = new TabPage();
            lblRunAtStartup = new Label();
            chkRunAtStartup = new CheckBox();
            lblMinimize = new Label();
            chkMinimize = new CheckBox();
            lblAutoStart = new Label();
            chkAutoStart = new CheckBox();
            lblLang = new Label();
            cboLang = new ComboBox();
            lblDocumentRoot = new Label();
            txtDocumentRoot = new TextBox();
            btnDocumentRoot = new Button();
            lblDataDir = new Label();
            txtDataDir = new TextBox();
            btnDataDir = new Button();
            lblAutoVirtualHosts = new Label();
            chkAutoVirtualHosts = new CheckBox();
            lblHostname = new Label();
            txtHostname = new TextBox();
            lblHostnameHint = new Label();
            lblBackup = new Label();
            chkAutoBackup = new CheckBox();
            lblBackupInterval = new Label();
            numBackupInterval = new NumericUpDown();
            lblBackupHint = new Label();
            lblAutoUpdate = new Label();
            chkAutoUpdate = new CheckBox();
            lineSep1 = new Panel();
            lblHeaderAdvanced = new Label();
            lblMailCatcher = new Label();
            chkMailCatcher = new CheckBox();
            lblMailSeconds = new Label();
            numMailSeconds = new NumericUpDown();
            lblMailSecondsUnit = new Label();
            lblMailSender = new Label();
            chkMailSender = new CheckBox();
            lblGmailAddress = new Label();
            txtGmailAddress = new TextBox();
            lblGmailPassword = new Label();
            txtGmailPassword = new TextBox();
            btnTestMail = new Button();
            lblMailNote = new Label();
            tabServices = new TabPage();
            lblColService = new Label();
            lblColVersion = new Label();
            lblColPort = new Label();
            lblColSslPort = new Label();
            lblColSsl = new Label();
            lblColEnabled = new Label();
            lblSvc1Name = new Label();
            lblSvc1Ver = new Label();
            numSvc1Port = new NumericUpDown();
            numSvc1Ssl = new NumericUpDown();
            chkSvc1Ssl = new CheckBox();
            chkSvc1 = new CheckBox();
            lblSvc2Name = new Label();
            lblSvc2Ver = new Label();
            numSvc2Port = new NumericUpDown();
            numSvc2Ssl = new NumericUpDown();
            chkSvc2Ssl = new CheckBox();
            chkSvc2 = new CheckBox();
            lblSvc3Name = new Label();
            lblSvc3Ver = new Label();
            numSvc3Port = new NumericUpDown();
            numSvc3Ssl = new NumericUpDown();
            chkSvc3Ssl = new CheckBox();
            chkSvc3 = new CheckBox();
            lblSvc4Name = new Label();
            lblSvc4Ver = new Label();
            numSvc4Port = new NumericUpDown();
            numSvc4Ssl = new NumericUpDown();
            chkSvc4Ssl = new CheckBox();
            chkSvc4 = new CheckBox();
            lblSvc5Name = new Label();
            lblSvc5Ver = new Label();
            numSvc5Port = new NumericUpDown();
            numSvc5Ssl = new NumericUpDown();
            chkSvc5Ssl = new CheckBox();
            chkSvc5 = new CheckBox();
            lblSvc6Name = new Label();
            lblSvc6Ver = new Label();
            numSvc6Port = new NumericUpDown();
            numSvc6Ssl = new NumericUpDown();
            chkSvc6Ssl = new CheckBox();
            chkSvc6 = new CheckBox();
            lblSvc7Name = new Label();
            lblSvc7Ver = new Label();
            numSvc7Port = new NumericUpDown();
            numSvc7Ssl = new NumericUpDown();
            chkSvc7Ssl = new CheckBox();
            chkSvc7 = new CheckBox();
            lblSvc8Name = new Label();
            lblSvc10Name = new Label();
            lblSvc9Name = new Label();
            lblSvc8Ver = new Label();
            lblSvc10Ver = new Label();
            lblSvc9Ver = new Label();
            numSvc8Port = new NumericUpDown();
            numSvc10Port = new NumericUpDown();
            numSvc9Port = new NumericUpDown();
            numSvc8Ssl = new NumericUpDown();
            numSvc10Ssl = new NumericUpDown();
            numSvc9Ssl = new NumericUpDown();
            chkSvc8Ssl = new CheckBox();
            chkSvc10Ssl = new CheckBox();
            chkSvc9Ssl = new CheckBox();
            chkSvc8 = new CheckBox();
            chkSvc10 = new CheckBox();
            chkSvc9 = new CheckBox();
            lineSep2 = new Panel();
            lblServicesHint = new Label();
            tipHints = new ToolTip(components);
            panelButtons.SuspendLayout();
            tabPrefs.SuspendLayout();
            tabGeneral.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numBackupInterval).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numMailSeconds).BeginInit();
            tabServices.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numSvc1Port).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numSvc1Ssl).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numSvc2Port).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numSvc2Ssl).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numSvc3Port).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numSvc3Ssl).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numSvc4Port).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numSvc4Ssl).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numSvc5Port).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numSvc5Ssl).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numSvc6Port).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numSvc6Ssl).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numSvc7Port).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numSvc7Ssl).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numSvc8Port).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numSvc10Port).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numSvc9Port).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numSvc8Ssl).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numSvc10Ssl).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numSvc9Ssl).BeginInit();
            SuspendLayout();
            // 
            // panelButtons
            // 
            panelButtons.BackColor = Color.White;
            panelButtons.Controls.Add(panelButtonsLine);
            panelButtons.Controls.Add(btnOK);
            panelButtons.Controls.Add(btnCancel);
            panelButtons.Controls.Add(btnApply);
            panelButtons.Controls.Add(lblSettingsFile);
            panelButtons.Dock = DockStyle.Bottom;
            panelButtons.Location = new Point(0, 576);
            panelButtons.Name = "panelButtons";
            panelButtons.Size = new Size(626, 52);
            panelButtons.TabIndex = 1;
            // 
            // panelButtonsLine
            // 
            panelButtonsLine.BackColor = Color.FromArgb(210, 214, 220);
            panelButtonsLine.Dock = DockStyle.Top;
            panelButtonsLine.Location = new Point(0, 0);
            panelButtonsLine.Name = "panelButtonsLine";
            panelButtonsLine.Size = new Size(626, 1);
            panelButtonsLine.TabIndex = 0;
            // 
            // btnOK
            // 
            btnOK.Location = new Point(12, 12);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(90, 28);
            btnOK.TabIndex = 107;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(110, 12);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(90, 28);
            btnCancel.TabIndex = 108;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnApply
            // 
            btnApply.Location = new Point(208, 12);
            btnApply.Name = "btnApply";
            btnApply.Size = new Size(90, 28);
            btnApply.TabIndex = 109;
            btnApply.Text = "Apply";
            btnApply.UseVisualStyleBackColor = true;
            btnApply.Click += btnApply_Click;
            // 
            // lblSettingsFile
            // 
            lblSettingsFile.Location = new Point(316, 17);
            lblSettingsFile.Name = "lblSettingsFile";
            lblSettingsFile.Size = new Size(312, 18);
            lblSettingsFile.TabIndex = 110;
            lblSettingsFile.Text = "Setting file: laragon.ini";
            lblSettingsFile.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // tabPrefs
            // 
            tabPrefs.Controls.Add(tabGeneral);
            tabPrefs.Controls.Add(tabServices);
            tabPrefs.Location = new Point(12, 12);
            tabPrefs.Name = "tabPrefs";
            tabPrefs.SelectedIndex = 0;
            tabPrefs.Size = new Size(602, 552);
            tabPrefs.TabIndex = 0;
            // 
            // tabGeneral
            // 
            tabGeneral.AutoScroll = true;
            tabGeneral.BackColor = Color.White;
            tabGeneral.Controls.Add(lblRunAtStartup);
            tabGeneral.Controls.Add(chkRunAtStartup);
            tabGeneral.Controls.Add(lblMinimize);
            tabGeneral.Controls.Add(chkMinimize);
            tabGeneral.Controls.Add(lblAutoStart);
            tabGeneral.Controls.Add(chkAutoStart);
            tabGeneral.Controls.Add(lblLang);
            tabGeneral.Controls.Add(cboLang);
            tabGeneral.Controls.Add(lblDocumentRoot);
            tabGeneral.Controls.Add(txtDocumentRoot);
            tabGeneral.Controls.Add(btnDocumentRoot);
            tabGeneral.Controls.Add(lblDataDir);
            tabGeneral.Controls.Add(txtDataDir);
            tabGeneral.Controls.Add(btnDataDir);
            tabGeneral.Controls.Add(lblAutoVirtualHosts);
            tabGeneral.Controls.Add(chkAutoVirtualHosts);
            tabGeneral.Controls.Add(lblHostname);
            tabGeneral.Controls.Add(txtHostname);
            tabGeneral.Controls.Add(lblHostnameHint);
            tabGeneral.Controls.Add(lblBackup);
            tabGeneral.Controls.Add(chkAutoBackup);
            tabGeneral.Controls.Add(lblBackupInterval);
            tabGeneral.Controls.Add(numBackupInterval);
            tabGeneral.Controls.Add(lblBackupHint);
            tabGeneral.Controls.Add(lblAutoUpdate);
            tabGeneral.Controls.Add(chkAutoUpdate);
            tabGeneral.Controls.Add(lineSep1);
            tabGeneral.Controls.Add(lblHeaderAdvanced);
            tabGeneral.Controls.Add(lblMailCatcher);
            tabGeneral.Controls.Add(chkMailCatcher);
            tabGeneral.Controls.Add(lblMailSeconds);
            tabGeneral.Controls.Add(numMailSeconds);
            tabGeneral.Controls.Add(lblMailSecondsUnit);
            tabGeneral.Controls.Add(lblMailSender);
            tabGeneral.Controls.Add(chkMailSender);
            tabGeneral.Controls.Add(lblGmailAddress);
            tabGeneral.Controls.Add(txtGmailAddress);
            tabGeneral.Controls.Add(lblGmailPassword);
            tabGeneral.Controls.Add(txtGmailPassword);
            tabGeneral.Controls.Add(btnTestMail);
            tabGeneral.Controls.Add(lblMailNote);
            tabGeneral.Location = new Point(4, 24);
            tabGeneral.Name = "tabGeneral";
            tabGeneral.Padding = new Padding(3);
            tabGeneral.Size = new Size(594, 524);
            tabGeneral.TabIndex = 0;
            tabGeneral.Text = "General";
            // 
            // lblRunAtStartup
            // 
            lblRunAtStartup.Location = new Point(12, 10);
            lblRunAtStartup.Name = "lblRunAtStartup";
            lblRunAtStartup.Size = new Size(300, 18);
            lblRunAtStartup.TabIndex = 0;
            lblRunAtStartup.Text = "Run Laragon when Windows starts";
            lblRunAtStartup.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // chkRunAtStartup
            // 
            chkRunAtStartup.Location = new Point(470, 8);
            chkRunAtStartup.Name = "chkRunAtStartup";
            chkRunAtStartup.Size = new Size(110, 20);
            chkRunAtStartup.TabIndex = 11;
            chkRunAtStartup.Text = "Enabled";
            tipHints.SetToolTip(chkRunAtStartup, "Run Laragon automatically when Windows starts");
            chkRunAtStartup.UseVisualStyleBackColor = true;
            // 
            // lblMinimize
            // 
            lblMinimize.Location = new Point(12, 32);
            lblMinimize.Name = "lblMinimize";
            lblMinimize.Size = new Size(300, 18);
            lblMinimize.TabIndex = 12;
            lblMinimize.Text = "Run minimized";
            lblMinimize.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // chkMinimize
            // 
            chkMinimize.Location = new Point(470, 30);
            chkMinimize.Name = "chkMinimize";
            chkMinimize.Size = new Size(110, 20);
            chkMinimize.TabIndex = 13;
            chkMinimize.Text = "Enabled";
            tipHints.SetToolTip(chkMinimize, "Minimize Laragon to the System Tray.\r\nLeft click Laragon's icon in the system tray to show Laragon");
            chkMinimize.UseVisualStyleBackColor = true;
            // 
            // lblAutoStart
            // 
            lblAutoStart.Location = new Point(12, 54);
            lblAutoStart.Name = "lblAutoStart";
            lblAutoStart.Size = new Size(300, 18);
            lblAutoStart.TabIndex = 14;
            lblAutoStart.Text = "Start All automatically";
            lblAutoStart.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // chkAutoStart
            // 
            chkAutoStart.Location = new Point(470, 52);
            chkAutoStart.Name = "chkAutoStart";
            chkAutoStart.Size = new Size(110, 20);
            chkAutoStart.TabIndex = 15;
            chkAutoStart.Text = "Enabled";
            tipHints.SetToolTip(chkAutoStart, "Automatically start all selected Services when Laragon runs");
            chkAutoStart.UseVisualStyleBackColor = true;
            // 
            // lblLang
            // 
            lblLang.Location = new Point(12, 82);
            lblLang.Name = "lblLang";
            lblLang.Size = new Size(130, 18);
            lblLang.TabIndex = 16;
            lblLang.Text = "Language";
            lblLang.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // cboLang
            // 
            cboLang.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLang.FormattingEnabled = true;
            cboLang.Items.AddRange(new object[] { "English", "Spanish" });
            cboLang.Location = new Point(148, 84);
            cboLang.Name = "cboLang";
            cboLang.Size = new Size(160, 23);
            cboLang.TabIndex = 17;
            cboLang.SelectedIndexChanged += cboLang_SelectedIndexChanged;
            // 
            // lblDocumentRoot
            // 
            lblDocumentRoot.Location = new Point(12, 110);
            lblDocumentRoot.Name = "lblDocumentRoot";
            lblDocumentRoot.Size = new Size(130, 18);
            lblDocumentRoot.TabIndex = 18;
            lblDocumentRoot.Text = "Document Root";
            lblDocumentRoot.TextAlign = ContentAlignment.MiddleLeft;
            tipHints.SetToolTip(lblDocumentRoot, "Click to change Document Root");
            // 
            // txtDocumentRoot
            // 
            txtDocumentRoot.Location = new Point(148, 112);
            txtDocumentRoot.Name = "txtDocumentRoot";
            txtDocumentRoot.Size = new Size(380, 21);
            txtDocumentRoot.TabIndex = 19;
            tipHints.SetToolTip(txtDocumentRoot, "Click to change Document Root");
            // 
            // btnDocumentRoot
            // 
            btnDocumentRoot.Location = new Point(534, 111);
            btnDocumentRoot.Name = "btnDocumentRoot";
            btnDocumentRoot.Size = new Size(40, 25);
            btnDocumentRoot.TabIndex = 20;
            btnDocumentRoot.Text = "...";
            tipHints.SetToolTip(btnDocumentRoot, "Click to change Document Root");
            btnDocumentRoot.UseVisualStyleBackColor = true;
            btnDocumentRoot.Click += btnDocumentRoot_Click;
            // 
            // lblDataDir
            // 
            lblDataDir.Location = new Point(12, 138);
            lblDataDir.Name = "lblDataDir";
            lblDataDir.Size = new Size(130, 18);
            lblDataDir.TabIndex = 21;
            lblDataDir.Text = "Data Dir";
            lblDataDir.TextAlign = ContentAlignment.MiddleLeft;
            tipHints.SetToolTip(lblDataDir, "Click to change MySQL's DataDir");
            // 
            // txtDataDir
            // 
            txtDataDir.Location = new Point(148, 140);
            txtDataDir.Name = "txtDataDir";
            txtDataDir.Size = new Size(380, 21);
            txtDataDir.TabIndex = 22;
            tipHints.SetToolTip(txtDataDir, "Click to change MySQL's DataDir");
            // 
            // btnDataDir
            // 
            btnDataDir.Location = new Point(534, 139);
            btnDataDir.Name = "btnDataDir";
            btnDataDir.Size = new Size(40, 25);
            btnDataDir.TabIndex = 23;
            btnDataDir.Text = "...";
            tipHints.SetToolTip(btnDataDir, "Click to change MySQL's DataDir");
            btnDataDir.UseVisualStyleBackColor = true;
            btnDataDir.Click += btnDataDir_Click;
            // 
            // lblAutoVirtualHosts
            // 
            lblAutoVirtualHosts.Location = new Point(12, 166);
            lblAutoVirtualHosts.Name = "lblAutoVirtualHosts";
            lblAutoVirtualHosts.Size = new Size(300, 18);
            lblAutoVirtualHosts.TabIndex = 24;
            lblAutoVirtualHosts.Text = "Auto virtual hosts";
            lblAutoVirtualHosts.TextAlign = ContentAlignment.MiddleLeft;
            tipHints.SetToolTip(lblAutoVirtualHosts, "Just put a folder in Document Root and reload Apache, Laragon will auto create correspond hostname in hosts file and Apache's virtual hosts");
            // 
            // chkAutoVirtualHosts
            // 
            chkAutoVirtualHosts.Location = new Point(470, 164);
            chkAutoVirtualHosts.Name = "chkAutoVirtualHosts";
            chkAutoVirtualHosts.Size = new Size(110, 20);
            chkAutoVirtualHosts.TabIndex = 25;
            chkAutoVirtualHosts.Text = "Enabled";
            tipHints.SetToolTip(chkAutoVirtualHosts, "Just put a folder in Document Root and reload Apache, Laragon will auto create correspond hostname in hosts file and Apache's virtual hosts");
            chkAutoVirtualHosts.UseVisualStyleBackColor = true;
            // 
            // lblHostname
            // 
            lblHostname.Location = new Point(12, 192);
            lblHostname.Name = "lblHostname";
            lblHostname.Size = new Size(130, 18);
            lblHostname.TabIndex = 26;
            lblHostname.Text = "Hostname";
            lblHostname.TextAlign = ContentAlignment.MiddleLeft;
            tipHints.SetToolTip(lblHostname, "Hostname format.\r\nex: if your project name is Laragon\r\n{name}.test -> http://laragon.test\r\n{name}.me  -> http://laragon.me");
            // 
            // txtHostname
            // 
            txtHostname.Location = new Point(148, 194);
            txtHostname.Name = "txtHostname";
            txtHostname.Size = new Size(170, 21);
            txtHostname.TabIndex = 27;
            tipHints.SetToolTip(txtHostname, "Hostname format.\r\nex: if your project name is Laragon\r\n{name}.test -> http://laragon.test\r\n{name}.me  -> http://laragon.me");
            // 
            // lblHostnameHint
            // 
            lblHostnameHint.ForeColor = Color.Gray;
            lblHostnameHint.Location = new Point(326, 194);
            lblHostnameHint.Name = "lblHostnameHint";
            lblHostnameHint.Size = new Size(250, 18);
            lblHostnameHint.TabIndex = 28;
            lblHostnameHint.Text = "ex: if your project name is Laragon";
            lblHostnameHint.TextAlign = ContentAlignment.MiddleLeft;
            tipHints.SetToolTip(lblHostnameHint, "Hostname format.\r\nex: if your project name is Laragon\r\n{name}.test -> http://laragon.test\r\n{name}.me  -> http://laragon.me");
            // 
            // lblBackup
            // 
            lblBackup.Location = new Point(12, 218);
            lblBackup.Name = "lblBackup";
            lblBackup.Size = new Size(300, 18);
            lblBackup.TabIndex = 29;
            lblBackup.Text = "Auto Backup";
            lblBackup.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // chkAutoBackup
            // 
            chkAutoBackup.Location = new Point(470, 216);
            chkAutoBackup.Name = "chkAutoBackup";
            chkAutoBackup.Size = new Size(110, 20);
            chkAutoBackup.TabIndex = 30;
            chkAutoBackup.Text = "Enabled";
            tipHints.SetToolTip(chkAutoBackup, "Laragon automatically backs up your data directory every 8 hours, storing the backups in your Laragon backups folder and retaining the 5 most recent versions for safekeeping");
            chkAutoBackup.UseVisualStyleBackColor = true;
            // 
            // lblBackupInterval
            // 
            lblBackupInterval.Location = new Point(12, 242);
            lblBackupInterval.Name = "lblBackupInterval";
            lblBackupInterval.Size = new Size(130, 18);
            lblBackupInterval.TabIndex = 31;
            lblBackupInterval.Text = "Backup Interval (hours)";
            lblBackupInterval.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // numBackupInterval
            // 
            numBackupInterval.Location = new Point(148, 244);
            numBackupInterval.Maximum = new decimal(new int[] { 720, 0, 0, 0 });
            numBackupInterval.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numBackupInterval.Name = "numBackupInterval";
            numBackupInterval.Size = new Size(60, 21);
            numBackupInterval.TabIndex = 32;
            numBackupInterval.Value = new decimal(new int[] { 8, 0, 0, 0 });
            // 
            // lblBackupHint
            // 
            lblBackupHint.ForeColor = Color.Gray;
            lblBackupHint.Location = new Point(216, 244);
            lblBackupHint.Name = "lblBackupHint";
            lblBackupHint.Size = new Size(360, 18);
            lblBackupHint.TabIndex = 33;
            lblBackupHint.Text = "keeps the 5 most recent versions";
            lblBackupHint.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblAutoUpdate
            // 
            lblAutoUpdate.Location = new Point(12, 268);
            lblAutoUpdate.Name = "lblAutoUpdate";
            lblAutoUpdate.Size = new Size(300, 18);
            lblAutoUpdate.TabIndex = 34;
            lblAutoUpdate.Text = "Auto Update";
            lblAutoUpdate.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // chkAutoUpdate
            // 
            chkAutoUpdate.Location = new Point(470, 266);
            chkAutoUpdate.Name = "chkAutoUpdate";
            chkAutoUpdate.Size = new Size(110, 20);
            chkAutoUpdate.TabIndex = 35;
            chkAutoUpdate.Text = "Enabled";
            tipHints.SetToolTip(chkAutoUpdate, "Laragon checks, downloads, and configures the latest PHP versions automatically - keeping you up to date effortlessly");
            chkAutoUpdate.UseVisualStyleBackColor = true;
            // 
            // lineSep1
            // 
            lineSep1.BackColor = Color.FromArgb(210, 214, 220);
            lineSep1.Location = new Point(12, 290);
            lineSep1.Name = "lineSep1";
            lineSep1.Size = new Size(576, 1);
            lineSep1.TabIndex = 36;
            // 
            // lblHeaderAdvanced
            // 
            lblHeaderAdvanced.Font = new Font("Arial", 9.75F, FontStyle.Bold);
            lblHeaderAdvanced.Location = new Point(12, 296);
            lblHeaderAdvanced.Name = "lblHeaderAdvanced";
            lblHeaderAdvanced.Size = new Size(300, 20);
            lblHeaderAdvanced.TabIndex = 37;
            lblHeaderAdvanced.Text = "Advanced";
            lblHeaderAdvanced.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblMailCatcher
            // 
            lblMailCatcher.Location = new Point(12, 320);
            lblMailCatcher.Name = "lblMailCatcher";
            lblMailCatcher.Size = new Size(300, 18);
            lblMailCatcher.TabIndex = 38;
            lblMailCatcher.Text = "Mail Catcher";
            lblMailCatcher.TextAlign = ContentAlignment.MiddleLeft;
            tipHints.SetToolTip(lblMailCatcher, "When mail() function is called, Laragon will display genarated email info\r\nin a small Window on the bottom right of your screen.");
            // 
            // chkMailCatcher
            // 
            chkMailCatcher.Location = new Point(470, 318);
            chkMailCatcher.Name = "chkMailCatcher";
            chkMailCatcher.Size = new Size(110, 20);
            chkMailCatcher.TabIndex = 39;
            chkMailCatcher.Text = "Enabled";
            tipHints.SetToolTip(chkMailCatcher, "When mail() function is called, Laragon will display genarated email info\r\nin a small Window on the bottom right of your screen.");
            chkMailCatcher.UseVisualStyleBackColor = true;
            // 
            // lblMailSeconds
            // 
            lblMailSeconds.Location = new Point(12, 344);
            lblMailSeconds.Name = "lblMailSeconds";
            lblMailSeconds.Size = new Size(190, 18);
            lblMailSeconds.TabIndex = 40;
            lblMailSeconds.Text = "The Window will be displayed in";
            lblMailSeconds.TextAlign = ContentAlignment.MiddleLeft;
            tipHints.SetToolTip(lblMailSeconds, "This feature helps you view email content quickly.\r\nYou can also view content of the latest email at any time in:\r\n\r\nMenu > PHP > MailCatcher");
            // 
            // numMailSeconds
            // 
            numMailSeconds.Location = new Point(206, 346);
            numMailSeconds.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numMailSeconds.Name = "numMailSeconds";
            numMailSeconds.Size = new Size(60, 21);
            numMailSeconds.TabIndex = 41;
            tipHints.SetToolTip(numMailSeconds, "This feature helps you view email content quickly.\r\nYou can also view content of the latest email at any time in:\r\n\r\nMenu > PHP > MailCatcher");
            numMailSeconds.Value = new decimal(new int[] { 3, 0, 0, 0 });
            // 
            // lblMailSecondsUnit
            // 
            lblMailSecondsUnit.ForeColor = Color.Gray;
            lblMailSecondsUnit.Location = new Point(274, 346);
            lblMailSecondsUnit.Name = "lblMailSecondsUnit";
            lblMailSecondsUnit.Size = new Size(60, 18);
            lblMailSecondsUnit.TabIndex = 42;
            lblMailSecondsUnit.Text = "second(s)";
            lblMailSecondsUnit.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblMailSender
            // 
            lblMailSender.Location = new Point(12, 370);
            lblMailSender.Name = "lblMailSender";
            lblMailSender.Size = new Size(300, 18);
            lblMailSender.TabIndex = 43;
            lblMailSender.Text = "Mail Sender";
            lblMailSender.TextAlign = ContentAlignment.MiddleLeft;
            tipHints.SetToolTip(lblMailSender, "Your Gmail Password will be encrypted from curious eyes.\r\nWhen enabled, you can send email to Internet easily. Just one line:\r\nmail($to, $subject, $message);");
            // 
            // chkMailSender
            // 
            chkMailSender.Location = new Point(470, 368);
            chkMailSender.Name = "chkMailSender";
            chkMailSender.Size = new Size(110, 20);
            chkMailSender.TabIndex = 44;
            chkMailSender.Text = "Enabled";
            tipHints.SetToolTip(chkMailSender, "Your Gmail Password will be encrypted from curious eyes.\r\nWhen enabled, you can send email to Internet easily. Just one line:\r\nmail($to, $subject, $message);");
            chkMailSender.UseVisualStyleBackColor = true;
            // 
            // lblGmailAddress
            // 
            lblGmailAddress.Location = new Point(12, 394);
            lblGmailAddress.Name = "lblGmailAddress";
            lblGmailAddress.Size = new Size(130, 18);
            lblGmailAddress.TabIndex = 45;
            lblGmailAddress.Text = "Gmail Address";
            lblGmailAddress.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtGmailAddress
            // 
            txtGmailAddress.Location = new Point(148, 396);
            txtGmailAddress.Name = "txtGmailAddress";
            txtGmailAddress.Size = new Size(300, 21);
            txtGmailAddress.TabIndex = 46;
            // 
            // lblGmailPassword
            // 
            lblGmailPassword.Location = new Point(12, 420);
            lblGmailPassword.Name = "lblGmailPassword";
            lblGmailPassword.Size = new Size(130, 18);
            lblGmailPassword.TabIndex = 47;
            lblGmailPassword.Text = "Gmail Password";
            lblGmailPassword.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtGmailPassword
            // 
            txtGmailPassword.Location = new Point(148, 422);
            txtGmailPassword.Name = "txtGmailPassword";
            txtGmailPassword.PasswordChar = '*';
            txtGmailPassword.Size = new Size(300, 21);
            txtGmailPassword.TabIndex = 48;
            tipHints.SetToolTip(txtGmailPassword, "Your Gmail Password will be encrypted from curious eyes.\r\nWhen enabled, you can send email to Internet easily. Just one line:\r\nmail($to, $subject, $message);");
            // 
            // btnTestMail
            // 
            btnTestMail.Location = new Point(148, 446);
            btnTestMail.Name = "btnTestMail";
            btnTestMail.Size = new Size(180, 26);
            btnTestMail.TabIndex = 49;
            btnTestMail.Text = "Test Sending Mail";
            btnTestMail.UseVisualStyleBackColor = true;
            btnTestMail.Click += btnTestMail_Click;
            // 
            // lblMailNote
            // 
            lblMailNote.ForeColor = Color.Gray;
            lblMailNote.Location = new Point(12, 476);
            lblMailNote.Name = "lblMailNote";
            lblMailNote.Size = new Size(570, 30);
            lblMailNote.TabIndex = 50;
            lblMailNote.Text = "Your Gmail Password will be encrypted from curious eyes. When enabled, you can send email to Internet easily. Just one line: mail($to, $subject, $message);";
            lblMailNote.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // tabServices
            // 
            tabServices.BackColor = Color.White;
            tabServices.Controls.Add(lblColService);
            tabServices.Controls.Add(lblColVersion);
            tabServices.Controls.Add(lblColPort);
            tabServices.Controls.Add(lblColSslPort);
            tabServices.Controls.Add(lblColSsl);
            tabServices.Controls.Add(lblColEnabled);
            tabServices.Controls.Add(lblSvc1Name);
            tabServices.Controls.Add(lblSvc1Ver);
            tabServices.Controls.Add(numSvc1Port);
            tabServices.Controls.Add(numSvc1Ssl);
            tabServices.Controls.Add(chkSvc1Ssl);
            tabServices.Controls.Add(chkSvc1);
            tabServices.Controls.Add(lblSvc2Name);
            tabServices.Controls.Add(lblSvc2Ver);
            tabServices.Controls.Add(numSvc2Port);
            tabServices.Controls.Add(numSvc2Ssl);
            tabServices.Controls.Add(chkSvc2Ssl);
            tabServices.Controls.Add(chkSvc2);
            tabServices.Controls.Add(lblSvc3Name);
            tabServices.Controls.Add(lblSvc3Ver);
            tabServices.Controls.Add(numSvc3Port);
            tabServices.Controls.Add(numSvc3Ssl);
            tabServices.Controls.Add(chkSvc3Ssl);
            tabServices.Controls.Add(chkSvc3);
            tabServices.Controls.Add(lblSvc4Name);
            tabServices.Controls.Add(lblSvc4Ver);
            tabServices.Controls.Add(numSvc4Port);
            tabServices.Controls.Add(numSvc4Ssl);
            tabServices.Controls.Add(chkSvc4Ssl);
            tabServices.Controls.Add(chkSvc4);
            tabServices.Controls.Add(lblSvc5Name);
            tabServices.Controls.Add(lblSvc5Ver);
            tabServices.Controls.Add(numSvc5Port);
            tabServices.Controls.Add(numSvc5Ssl);
            tabServices.Controls.Add(chkSvc5Ssl);
            tabServices.Controls.Add(chkSvc5);
            tabServices.Controls.Add(lblSvc6Name);
            tabServices.Controls.Add(lblSvc6Ver);
            tabServices.Controls.Add(numSvc6Port);
            tabServices.Controls.Add(numSvc6Ssl);
            tabServices.Controls.Add(chkSvc6Ssl);
            tabServices.Controls.Add(chkSvc6);
            tabServices.Controls.Add(lblSvc7Name);
            tabServices.Controls.Add(lblSvc7Ver);
            tabServices.Controls.Add(numSvc7Port);
            tabServices.Controls.Add(numSvc7Ssl);
            tabServices.Controls.Add(chkSvc7Ssl);
            tabServices.Controls.Add(chkSvc7);
            tabServices.Controls.Add(lblSvc8Name);
            tabServices.Controls.Add(lblSvc10Name);
            tabServices.Controls.Add(lblSvc9Name);
            tabServices.Controls.Add(lblSvc8Ver);
            tabServices.Controls.Add(lblSvc10Ver);
            tabServices.Controls.Add(lblSvc9Ver);
            tabServices.Controls.Add(numSvc8Port);
            tabServices.Controls.Add(numSvc10Port);
            tabServices.Controls.Add(numSvc9Port);
            tabServices.Controls.Add(numSvc8Ssl);
            tabServices.Controls.Add(numSvc10Ssl);
            tabServices.Controls.Add(numSvc9Ssl);
            tabServices.Controls.Add(chkSvc8Ssl);
            tabServices.Controls.Add(chkSvc10Ssl);
            tabServices.Controls.Add(chkSvc9Ssl);
            tabServices.Controls.Add(chkSvc8);
            tabServices.Controls.Add(chkSvc10);
            tabServices.Controls.Add(chkSvc9);
            tabServices.Controls.Add(lineSep2);
            tabServices.Controls.Add(lblServicesHint);
            tabServices.Location = new Point(4, 24);
            tabServices.Name = "tabServices";
            tabServices.Padding = new Padding(3);
            tabServices.Size = new Size(594, 524);
            tabServices.TabIndex = 1;
            tabServices.Text = "Services && Ports ";
            // 
            // lblColService
            // 
            lblColService.ForeColor = Color.Gray;
            lblColService.Location = new Point(12, 12);
            lblColService.Name = "lblColService";
            lblColService.Size = new Size(120, 18);
            lblColService.TabIndex = 0;
            lblColService.Text = "Service";
            lblColService.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblColVersion
            // 
            lblColVersion.ForeColor = Color.Gray;
            lblColVersion.Location = new Point(140, 12);
            lblColVersion.Name = "lblColVersion";
            lblColVersion.Size = new Size(100, 18);
            lblColVersion.TabIndex = 1;
            lblColVersion.Text = "Version";
            lblColVersion.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblColPort
            // 
            lblColPort.ForeColor = Color.Gray;
            lblColPort.Location = new Point(250, 12);
            lblColPort.Name = "lblColPort";
            lblColPort.Size = new Size(60, 18);
            lblColPort.TabIndex = 2;
            lblColPort.Text = "Port";
            lblColPort.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblColSslPort
            // 
            lblColSslPort.ForeColor = Color.Gray;
            lblColSslPort.Location = new Point(320, 12);
            lblColSslPort.Name = "lblColSslPort";
            lblColSslPort.Size = new Size(70, 18);
            lblColSslPort.TabIndex = 3;
            lblColSslPort.Text = "SSL port";
            lblColSslPort.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblColSsl
            // 
            lblColSsl.ForeColor = Color.Gray;
            lblColSsl.Location = new Point(400, 12);
            lblColSsl.Name = "lblColSsl";
            lblColSsl.Size = new Size(60, 18);
            lblColSsl.TabIndex = 4;
            lblColSsl.Text = "SSL";
            lblColSsl.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblColEnabled
            // 
            lblColEnabled.ForeColor = Color.Gray;
            lblColEnabled.Location = new Point(490, 12);
            lblColEnabled.Name = "lblColEnabled";
            lblColEnabled.Size = new Size(100, 18);
            lblColEnabled.TabIndex = 5;
            lblColEnabled.Text = "Enabled";
            lblColEnabled.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblSvc1Name
            // 
            lblSvc1Name.Location = new Point(12, 38);
            lblSvc1Name.Name = "lblSvc1Name";
            lblSvc1Name.Size = new Size(120, 18);
            lblSvc1Name.TabIndex = 6;
            lblSvc1Name.Text = "Apache";
            lblSvc1Name.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblSvc1Ver
            // 
            lblSvc1Ver.ForeColor = Color.DimGray;
            lblSvc1Ver.Location = new Point(140, 38);
            lblSvc1Ver.Name = "lblSvc1Ver";
            lblSvc1Ver.Size = new Size(100, 18);
            lblSvc1Ver.TabIndex = 7;
            lblSvc1Ver.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // numSvc1Port
            // 
            numSvc1Port.Location = new Point(250, 36);
            numSvc1Port.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numSvc1Port.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numSvc1Port.Name = "numSvc1Port";
            numSvc1Port.Size = new Size(60, 21);
            numSvc1Port.TabIndex = 59;
            numSvc1Port.Value = new decimal(new int[] { 80, 0, 0, 0 });
            // 
            // numSvc1Ssl
            // 
            numSvc1Ssl.Location = new Point(320, 36);
            numSvc1Ssl.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numSvc1Ssl.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numSvc1Ssl.Name = "numSvc1Ssl";
            numSvc1Ssl.Size = new Size(60, 21);
            numSvc1Ssl.TabIndex = 60;
            numSvc1Ssl.Value = new decimal(new int[] { 443, 0, 0, 0 });
            // 
            // chkSvc1Ssl
            // 
            chkSvc1Ssl.Location = new Point(400, 38);
            chkSvc1Ssl.Name = "chkSvc1Ssl";
            chkSvc1Ssl.Size = new Size(70, 20);
            chkSvc1Ssl.TabIndex = 61;
            chkSvc1Ssl.Text = "SSL";
            chkSvc1Ssl.UseVisualStyleBackColor = true;
            // 
            // chkSvc1
            // 
            chkSvc1.Location = new Point(490, 38);
            chkSvc1.Name = "chkSvc1";
            chkSvc1.Size = new Size(110, 20);
            chkSvc1.TabIndex = 62;
            chkSvc1.Text = "Enabled";
            chkSvc1.UseVisualStyleBackColor = true;
            // 
            // lblSvc2Name
            // 
            lblSvc2Name.Location = new Point(12, 68);
            lblSvc2Name.Name = "lblSvc2Name";
            lblSvc2Name.Size = new Size(120, 18);
            lblSvc2Name.TabIndex = 63;
            lblSvc2Name.Text = "Nginx";
            lblSvc2Name.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblSvc2Ver
            // 
            lblSvc2Ver.ForeColor = Color.DimGray;
            lblSvc2Ver.Location = new Point(140, 68);
            lblSvc2Ver.Name = "lblSvc2Ver";
            lblSvc2Ver.Size = new Size(100, 18);
            lblSvc2Ver.TabIndex = 64;
            lblSvc2Ver.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // numSvc2Port
            // 
            numSvc2Port.Location = new Point(250, 66);
            numSvc2Port.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numSvc2Port.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numSvc2Port.Name = "numSvc2Port";
            numSvc2Port.Size = new Size(60, 21);
            numSvc2Port.TabIndex = 65;
            numSvc2Port.Value = new decimal(new int[] { 8080, 0, 0, 0 });
            // 
            // numSvc2Ssl
            // 
            numSvc2Ssl.Location = new Point(320, 66);
            numSvc2Ssl.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numSvc2Ssl.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numSvc2Ssl.Name = "numSvc2Ssl";
            numSvc2Ssl.Size = new Size(60, 21);
            numSvc2Ssl.TabIndex = 66;
            numSvc2Ssl.Value = new decimal(new int[] { 443, 0, 0, 0 });
            // 
            // chkSvc2Ssl
            // 
            chkSvc2Ssl.Location = new Point(400, 68);
            chkSvc2Ssl.Name = "chkSvc2Ssl";
            chkSvc2Ssl.Size = new Size(70, 20);
            chkSvc2Ssl.TabIndex = 67;
            chkSvc2Ssl.Text = "SSL";
            chkSvc2Ssl.UseVisualStyleBackColor = true;
            // 
            // chkSvc2
            // 
            chkSvc2.Location = new Point(490, 68);
            chkSvc2.Name = "chkSvc2";
            chkSvc2.Size = new Size(110, 20);
            chkSvc2.TabIndex = 68;
            chkSvc2.Text = "Enabled";
            chkSvc2.UseVisualStyleBackColor = true;
            // 
            // lblSvc3Name
            // 
            lblSvc3Name.Location = new Point(12, 98);
            lblSvc3Name.Name = "lblSvc3Name";
            lblSvc3Name.Size = new Size(120, 18);
            lblSvc3Name.TabIndex = 69;
            lblSvc3Name.Text = "MySQL";
            lblSvc3Name.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblSvc3Ver
            // 
            lblSvc3Ver.ForeColor = Color.DimGray;
            lblSvc3Ver.Location = new Point(140, 98);
            lblSvc3Ver.Name = "lblSvc3Ver";
            lblSvc3Ver.Size = new Size(100, 18);
            lblSvc3Ver.TabIndex = 70;
            lblSvc3Ver.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // numSvc3Port
            // 
            numSvc3Port.Location = new Point(250, 96);
            numSvc3Port.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numSvc3Port.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numSvc3Port.Name = "numSvc3Port";
            numSvc3Port.Size = new Size(60, 21);
            numSvc3Port.TabIndex = 71;
            numSvc3Port.Value = new decimal(new int[] { 3306, 0, 0, 0 });
            // 
            // numSvc3Ssl
            // 
            numSvc3Ssl.Location = new Point(320, 96);
            numSvc3Ssl.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numSvc3Ssl.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numSvc3Ssl.Name = "numSvc3Ssl";
            numSvc3Ssl.Size = new Size(60, 21);
            numSvc3Ssl.TabIndex = 72;
            numSvc3Ssl.Value = new decimal(new int[] { 3306, 0, 0, 0 });
            // 
            // chkSvc3Ssl
            // 
            chkSvc3Ssl.Location = new Point(400, 98);
            chkSvc3Ssl.Name = "chkSvc3Ssl";
            chkSvc3Ssl.Size = new Size(70, 20);
            chkSvc3Ssl.TabIndex = 73;
            chkSvc3Ssl.Text = "SSL";
            chkSvc3Ssl.UseVisualStyleBackColor = true;
            // 
            // chkSvc3
            // 
            chkSvc3.Location = new Point(490, 98);
            chkSvc3.Name = "chkSvc3";
            chkSvc3.Size = new Size(110, 20);
            chkSvc3.TabIndex = 74;
            chkSvc3.Text = "Enabled";
            chkSvc3.UseVisualStyleBackColor = true;
            // 
            // lblSvc4Name
            // 
            lblSvc4Name.Location = new Point(12, 128);
            lblSvc4Name.Name = "lblSvc4Name";
            lblSvc4Name.Size = new Size(120, 18);
            lblSvc4Name.TabIndex = 75;
            lblSvc4Name.Text = "PostgreSQL";
            lblSvc4Name.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblSvc4Ver
            // 
            lblSvc4Ver.ForeColor = Color.DimGray;
            lblSvc4Ver.Location = new Point(140, 128);
            lblSvc4Ver.Name = "lblSvc4Ver";
            lblSvc4Ver.Size = new Size(100, 18);
            lblSvc4Ver.TabIndex = 76;
            lblSvc4Ver.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // numSvc4Port
            // 
            numSvc4Port.Location = new Point(250, 126);
            numSvc4Port.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numSvc4Port.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numSvc4Port.Name = "numSvc4Port";
            numSvc4Port.Size = new Size(60, 21);
            numSvc4Port.TabIndex = 77;
            numSvc4Port.Value = new decimal(new int[] { 5432, 0, 0, 0 });
            // 
            // numSvc4Ssl
            // 
            numSvc4Ssl.Location = new Point(320, 126);
            numSvc4Ssl.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numSvc4Ssl.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numSvc4Ssl.Name = "numSvc4Ssl";
            numSvc4Ssl.Size = new Size(60, 21);
            numSvc4Ssl.TabIndex = 78;
            numSvc4Ssl.Value = new decimal(new int[] { 5432, 0, 0, 0 });
            // 
            // chkSvc4Ssl
            // 
            chkSvc4Ssl.Location = new Point(400, 128);
            chkSvc4Ssl.Name = "chkSvc4Ssl";
            chkSvc4Ssl.Size = new Size(70, 20);
            chkSvc4Ssl.TabIndex = 79;
            chkSvc4Ssl.Text = "SSL";
            chkSvc4Ssl.UseVisualStyleBackColor = true;
            // 
            // chkSvc4
            // 
            chkSvc4.Location = new Point(490, 128);
            chkSvc4.Name = "chkSvc4";
            chkSvc4.Size = new Size(110, 20);
            chkSvc4.TabIndex = 80;
            chkSvc4.Text = "Enabled";
            chkSvc4.UseVisualStyleBackColor = true;
            // 
            // lblSvc5Name
            // 
            lblSvc5Name.Location = new Point(12, 158);
            lblSvc5Name.Name = "lblSvc5Name";
            lblSvc5Name.Size = new Size(120, 18);
            lblSvc5Name.TabIndex = 81;
            lblSvc5Name.Text = "MongoDB";
            lblSvc5Name.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblSvc5Ver
            // 
            lblSvc5Ver.ForeColor = Color.DimGray;
            lblSvc5Ver.Location = new Point(140, 158);
            lblSvc5Ver.Name = "lblSvc5Ver";
            lblSvc5Ver.Size = new Size(100, 18);
            lblSvc5Ver.TabIndex = 82;
            lblSvc5Ver.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // numSvc5Port
            // 
            numSvc5Port.Location = new Point(250, 156);
            numSvc5Port.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numSvc5Port.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numSvc5Port.Name = "numSvc5Port";
            numSvc5Port.Size = new Size(60, 21);
            numSvc5Port.TabIndex = 83;
            numSvc5Port.Value = new decimal(new int[] { 27017, 0, 0, 0 });
            // 
            // numSvc5Ssl
            // 
            numSvc5Ssl.Location = new Point(320, 156);
            numSvc5Ssl.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numSvc5Ssl.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numSvc5Ssl.Name = "numSvc5Ssl";
            numSvc5Ssl.Size = new Size(60, 21);
            numSvc5Ssl.TabIndex = 84;
            numSvc5Ssl.Value = new decimal(new int[] { 27017, 0, 0, 0 });
            // 
            // chkSvc5Ssl
            // 
            chkSvc5Ssl.Location = new Point(400, 158);
            chkSvc5Ssl.Name = "chkSvc5Ssl";
            chkSvc5Ssl.Size = new Size(70, 20);
            chkSvc5Ssl.TabIndex = 85;
            chkSvc5Ssl.Text = "SSL";
            chkSvc5Ssl.UseVisualStyleBackColor = true;
            // 
            // chkSvc5
            // 
            chkSvc5.Location = new Point(490, 158);
            chkSvc5.Name = "chkSvc5";
            chkSvc5.Size = new Size(110, 20);
            chkSvc5.TabIndex = 86;
            chkSvc5.Text = "Enabled";
            chkSvc5.UseVisualStyleBackColor = true;
            // 
            // lblSvc6Name
            // 
            lblSvc6Name.Location = new Point(12, 188);
            lblSvc6Name.Name = "lblSvc6Name";
            lblSvc6Name.Size = new Size(120, 18);
            lblSvc6Name.TabIndex = 87;
            lblSvc6Name.Text = "Memcached";
            lblSvc6Name.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblSvc6Ver
            // 
            lblSvc6Ver.ForeColor = Color.DimGray;
            lblSvc6Ver.Location = new Point(140, 188);
            lblSvc6Ver.Name = "lblSvc6Ver";
            lblSvc6Ver.Size = new Size(100, 18);
            lblSvc6Ver.TabIndex = 88;
            lblSvc6Ver.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // numSvc6Port
            // 
            numSvc6Port.Location = new Point(250, 186);
            numSvc6Port.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numSvc6Port.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numSvc6Port.Name = "numSvc6Port";
            numSvc6Port.Size = new Size(60, 21);
            numSvc6Port.TabIndex = 89;
            numSvc6Port.Value = new decimal(new int[] { 11211, 0, 0, 0 });
            // 
            // numSvc6Ssl
            // 
            numSvc6Ssl.Location = new Point(320, 186);
            numSvc6Ssl.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numSvc6Ssl.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numSvc6Ssl.Name = "numSvc6Ssl";
            numSvc6Ssl.Size = new Size(60, 21);
            numSvc6Ssl.TabIndex = 90;
            numSvc6Ssl.Value = new decimal(new int[] { 11211, 0, 0, 0 });
            // 
            // chkSvc6Ssl
            // 
            chkSvc6Ssl.Location = new Point(400, 188);
            chkSvc6Ssl.Name = "chkSvc6Ssl";
            chkSvc6Ssl.Size = new Size(70, 20);
            chkSvc6Ssl.TabIndex = 91;
            chkSvc6Ssl.Text = "SSL";
            chkSvc6Ssl.UseVisualStyleBackColor = true;
            // 
            // chkSvc6
            // 
            chkSvc6.Location = new Point(490, 188);
            chkSvc6.Name = "chkSvc6";
            chkSvc6.Size = new Size(110, 20);
            chkSvc6.TabIndex = 92;
            chkSvc6.Text = "Enabled";
            chkSvc6.UseVisualStyleBackColor = true;
            // 
            // lblSvc7Name
            // 
            lblSvc7Name.Location = new Point(12, 218);
            lblSvc7Name.Name = "lblSvc7Name";
            lblSvc7Name.Size = new Size(120, 18);
            lblSvc7Name.TabIndex = 93;
            lblSvc7Name.Text = "Redis";
            lblSvc7Name.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblSvc7Ver
            // 
            lblSvc7Ver.ForeColor = Color.DimGray;
            lblSvc7Ver.Location = new Point(140, 218);
            lblSvc7Ver.Name = "lblSvc7Ver";
            lblSvc7Ver.Size = new Size(100, 18);
            lblSvc7Ver.TabIndex = 94;
            lblSvc7Ver.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // numSvc7Port
            // 
            numSvc7Port.Location = new Point(250, 216);
            numSvc7Port.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numSvc7Port.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numSvc7Port.Name = "numSvc7Port";
            numSvc7Port.Size = new Size(60, 21);
            numSvc7Port.TabIndex = 95;
            numSvc7Port.Value = new decimal(new int[] { 6379, 0, 0, 0 });
            // 
            // numSvc7Ssl
            // 
            numSvc7Ssl.Location = new Point(320, 216);
            numSvc7Ssl.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numSvc7Ssl.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numSvc7Ssl.Name = "numSvc7Ssl";
            numSvc7Ssl.Size = new Size(60, 21);
            numSvc7Ssl.TabIndex = 96;
            numSvc7Ssl.Value = new decimal(new int[] { 6379, 0, 0, 0 });
            // 
            // chkSvc7Ssl
            // 
            chkSvc7Ssl.Location = new Point(400, 218);
            chkSvc7Ssl.Name = "chkSvc7Ssl";
            chkSvc7Ssl.Size = new Size(70, 20);
            chkSvc7Ssl.TabIndex = 97;
            chkSvc7Ssl.Text = "SSL";
            chkSvc7Ssl.UseVisualStyleBackColor = true;
            // 
            // chkSvc7
            // 
            chkSvc7.Location = new Point(490, 218);
            chkSvc7.Name = "chkSvc7";
            chkSvc7.Size = new Size(110, 20);
            chkSvc7.TabIndex = 98;
            chkSvc7.Text = "Enabled";
            chkSvc7.UseVisualStyleBackColor = true;
            // 
            // lblSvc8Name
            // 
            lblSvc8Name.Location = new Point(12, 248);
            lblSvc8Name.Name = "lblSvc8Name";
            lblSvc8Name.Size = new Size(120, 18);
            lblSvc8Name.TabIndex = 99;
            lblSvc8Name.Text = "Mailpit";
            lblSvc8Name.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblSvc10Name
            // 
            lblSvc10Name.Location = new Point(12, 308);
            lblSvc10Name.Name = "lblSvc10Name";
            lblSvc10Name.Size = new Size(120, 18);
            lblSvc10Name.TabIndex = 100;
            lblSvc10Name.Text = "MariaDB";
            lblSvc10Name.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblSvc9Name
            // 
            lblSvc9Name.Location = new Point(12, 278);
            lblSvc9Name.Name = "lblSvc9Name";
            lblSvc9Name.Size = new Size(120, 18);
            lblSvc9Name.TabIndex = 101;
            lblSvc9Name.Text = "PHP";
            lblSvc9Name.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblSvc8Ver
            // 
            lblSvc8Ver.ForeColor = Color.DimGray;
            lblSvc8Ver.Location = new Point(140, 248);
            lblSvc8Ver.Name = "lblSvc8Ver";
            lblSvc8Ver.Size = new Size(100, 18);
            lblSvc8Ver.TabIndex = 102;
            lblSvc8Ver.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblSvc10Ver
            // 
            lblSvc10Ver.ForeColor = Color.DimGray;
            lblSvc10Ver.Location = new Point(140, 308);
            lblSvc10Ver.Name = "lblSvc10Ver";
            lblSvc10Ver.Size = new Size(100, 18);
            lblSvc10Ver.TabIndex = 103;
            lblSvc10Ver.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblSvc9Ver
            // 
            lblSvc9Ver.ForeColor = Color.DimGray;
            lblSvc9Ver.Location = new Point(140, 278);
            lblSvc9Ver.Name = "lblSvc9Ver";
            lblSvc9Ver.Size = new Size(100, 18);
            lblSvc9Ver.TabIndex = 104;
            lblSvc9Ver.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // numSvc8Port
            // 
            numSvc8Port.Location = new Point(250, 246);
            numSvc8Port.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numSvc8Port.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numSvc8Port.Name = "numSvc8Port";
            numSvc8Port.Size = new Size(60, 21);
            numSvc8Port.TabIndex = 101;
            numSvc8Port.Value = new decimal(new int[] { 1025, 0, 0, 0 });
            // 
            // numSvc10Port
            // 
            numSvc10Port.Location = new Point(250, 306);
            numSvc10Port.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numSvc10Port.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numSvc10Port.Name = "numSvc10Port";
            numSvc10Port.Size = new Size(60, 21);
            numSvc10Port.TabIndex = 101;
            numSvc10Port.Value = new decimal(new int[] { 1025, 0, 0, 0 });
            // 
            // numSvc9Port
            // 
            numSvc9Port.Location = new Point(250, 276);
            numSvc9Port.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numSvc9Port.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numSvc9Port.Name = "numSvc9Port";
            numSvc9Port.Size = new Size(60, 21);
            numSvc9Port.TabIndex = 101;
            numSvc9Port.Value = new decimal(new int[] { 1025, 0, 0, 0 });
            // 
            // numSvc8Ssl
            // 
            numSvc8Ssl.Location = new Point(320, 246);
            numSvc8Ssl.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numSvc8Ssl.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numSvc8Ssl.Name = "numSvc8Ssl";
            numSvc8Ssl.Size = new Size(60, 21);
            numSvc8Ssl.TabIndex = 102;
            numSvc8Ssl.Value = new decimal(new int[] { 1025, 0, 0, 0 });
            // 
            // numSvc10Ssl
            // 
            numSvc10Ssl.Location = new Point(320, 306);
            numSvc10Ssl.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numSvc10Ssl.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numSvc10Ssl.Name = "numSvc10Ssl";
            numSvc10Ssl.Size = new Size(60, 21);
            numSvc10Ssl.TabIndex = 102;
            numSvc10Ssl.Value = new decimal(new int[] { 1025, 0, 0, 0 });
            // 
            // numSvc9Ssl
            // 
            numSvc9Ssl.Location = new Point(320, 276);
            numSvc9Ssl.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numSvc9Ssl.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numSvc9Ssl.Name = "numSvc9Ssl";
            numSvc9Ssl.Size = new Size(60, 21);
            numSvc9Ssl.TabIndex = 102;
            numSvc9Ssl.Value = new decimal(new int[] { 1025, 0, 0, 0 });
            // 
            // chkSvc8Ssl
            // 
            chkSvc8Ssl.Location = new Point(400, 248);
            chkSvc8Ssl.Name = "chkSvc8Ssl";
            chkSvc8Ssl.Size = new Size(70, 20);
            chkSvc8Ssl.TabIndex = 103;
            chkSvc8Ssl.Text = "SSL";
            chkSvc8Ssl.UseVisualStyleBackColor = true;
            // 
            // chkSvc10Ssl
            // 
            chkSvc10Ssl.Location = new Point(400, 308);
            chkSvc10Ssl.Name = "chkSvc10Ssl";
            chkSvc10Ssl.Size = new Size(70, 20);
            chkSvc10Ssl.TabIndex = 103;
            chkSvc10Ssl.Text = "SSL";
            chkSvc10Ssl.UseVisualStyleBackColor = true;
            // 
            // chkSvc9Ssl
            // 
            chkSvc9Ssl.Location = new Point(400, 278);
            chkSvc9Ssl.Name = "chkSvc9Ssl";
            chkSvc9Ssl.Size = new Size(70, 20);
            chkSvc9Ssl.TabIndex = 103;
            chkSvc9Ssl.Text = "SSL";
            chkSvc9Ssl.UseVisualStyleBackColor = true;
            // 
            // chkSvc8
            // 
            chkSvc8.Location = new Point(490, 248);
            chkSvc8.Name = "chkSvc8";
            chkSvc8.Size = new Size(110, 20);
            chkSvc8.TabIndex = 104;
            chkSvc8.Text = "Enabled";
            chkSvc8.UseVisualStyleBackColor = true;
            // 
            // chkSvc10
            // 
            chkSvc10.Location = new Point(490, 308);
            chkSvc10.Name = "chkSvc10";
            chkSvc10.Size = new Size(110, 20);
            chkSvc10.TabIndex = 104;
            chkSvc10.Text = "Enabled";
            chkSvc10.UseVisualStyleBackColor = true;
            // 
            // chkSvc9
            // 
            chkSvc9.Location = new Point(490, 278);
            chkSvc9.Name = "chkSvc9";
            chkSvc9.Size = new Size(110, 20);
            chkSvc9.TabIndex = 104;
            chkSvc9.Text = "Enabled";
            chkSvc9.UseVisualStyleBackColor = true;
            // 
            // lineSep2
            // 
            lineSep2.BackColor = Color.FromArgb(210, 214, 220);
            lineSep2.Location = new Point(12, 346);
            lineSep2.Name = "lineSep2";
            lineSep2.Size = new Size(576, 1);
            lineSep2.TabIndex = 105;
            // 
            // lblServicesHint
            // 
            lblServicesHint.ForeColor = Color.Gray;
            lblServicesHint.Location = new Point(12, 356);
            lblServicesHint.Name = "lblServicesHint";
            lblServicesHint.Size = new Size(560, 34);
            lblServicesHint.TabIndex = 106;
            lblServicesHint.Text = "A service is started by \"Start All\" only when it is Enabled here and its program exists in the install folder.";
            lblServicesHint.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // PreferencesForm
            // 
            // No font scaling here either: the numbers in this file are the numbers the
            // window draws with, so a control the person moves in the Visual Studio designer
            // stays where they put it. It used to be AutoScaleMode.Font with a 7x15 base,
            // which multiplied every Location and Size by a ratio against Arial 9 pt -- the
            // quiet reason a hand-placed control turned up somewhere else at run time.
            AutoScaleMode = AutoScaleMode.None;
            BackColor = Color.White;
            ClientSize = new Size(626, 628);
            Controls.Add(tabPrefs);
            Controls.Add(panelButtons);
            Font = new Font("Arial", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "PreferencesForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Preferences";
            panelButtons.ResumeLayout(false);
            tabPrefs.ResumeLayout(false);
            tabGeneral.ResumeLayout(false);
            tabGeneral.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numBackupInterval).EndInit();
            ((System.ComponentModel.ISupportInitialize)numMailSeconds).EndInit();
            tabServices.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)numSvc1Port).EndInit();
            ((System.ComponentModel.ISupportInitialize)numSvc1Ssl).EndInit();
            ((System.ComponentModel.ISupportInitialize)numSvc2Port).EndInit();
            ((System.ComponentModel.ISupportInitialize)numSvc2Ssl).EndInit();
            ((System.ComponentModel.ISupportInitialize)numSvc3Port).EndInit();
            ((System.ComponentModel.ISupportInitialize)numSvc3Ssl).EndInit();
            ((System.ComponentModel.ISupportInitialize)numSvc4Port).EndInit();
            ((System.ComponentModel.ISupportInitialize)numSvc4Ssl).EndInit();
            ((System.ComponentModel.ISupportInitialize)numSvc5Port).EndInit();
            ((System.ComponentModel.ISupportInitialize)numSvc5Ssl).EndInit();
            ((System.ComponentModel.ISupportInitialize)numSvc6Port).EndInit();
            ((System.ComponentModel.ISupportInitialize)numSvc6Ssl).EndInit();
            ((System.ComponentModel.ISupportInitialize)numSvc7Port).EndInit();
            ((System.ComponentModel.ISupportInitialize)numSvc7Ssl).EndInit();
            ((System.ComponentModel.ISupportInitialize)numSvc8Port).EndInit();
            ((System.ComponentModel.ISupportInitialize)numSvc10Port).EndInit();
            ((System.ComponentModel.ISupportInitialize)numSvc9Port).EndInit();
            ((System.ComponentModel.ISupportInitialize)numSvc8Ssl).EndInit();
            ((System.ComponentModel.ISupportInitialize)numSvc10Ssl).EndInit();
            ((System.ComponentModel.ISupportInitialize)numSvc9Ssl).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label lblRunAtStartup;
        private System.Windows.Forms.CheckBox chkRunAtStartup;
        private System.Windows.Forms.Label lblMinimize;
        private System.Windows.Forms.CheckBox chkMinimize;
        private System.Windows.Forms.Label lblAutoStart;
        private System.Windows.Forms.CheckBox chkAutoStart;
        private System.Windows.Forms.Label lblLang;
        private System.Windows.Forms.ComboBox cboLang;
        private System.Windows.Forms.Label lblDocumentRoot;
        private System.Windows.Forms.TextBox txtDocumentRoot;
        private System.Windows.Forms.Button btnDocumentRoot;
        private System.Windows.Forms.Label lblDataDir;
        private System.Windows.Forms.TextBox txtDataDir;
        private System.Windows.Forms.Button btnDataDir;
        private System.Windows.Forms.Label lblAutoVirtualHosts;
        private System.Windows.Forms.CheckBox chkAutoVirtualHosts;
        private System.Windows.Forms.Label lblHostname;
        private System.Windows.Forms.TextBox txtHostname;
        private System.Windows.Forms.Label lblHostnameHint;
        private System.Windows.Forms.Label lblBackup;
        private System.Windows.Forms.CheckBox chkAutoBackup;
        private System.Windows.Forms.Label lblBackupInterval;
        private System.Windows.Forms.NumericUpDown numBackupInterval;
        private System.Windows.Forms.Label lblBackupHint;
        private System.Windows.Forms.Label lblAutoUpdate;
        private System.Windows.Forms.CheckBox chkAutoUpdate;
        private System.Windows.Forms.Panel lineSep1;
        private System.Windows.Forms.Label lblHeaderAdvanced;
        private System.Windows.Forms.Label lblMailCatcher;
        private System.Windows.Forms.CheckBox chkMailCatcher;
        private System.Windows.Forms.Label lblMailSeconds;
        private System.Windows.Forms.NumericUpDown numMailSeconds;
        private System.Windows.Forms.Label lblMailSecondsUnit;
        private System.Windows.Forms.Label lblMailSender;
        private System.Windows.Forms.CheckBox chkMailSender;
        private System.Windows.Forms.Label lblGmailAddress;
        private System.Windows.Forms.TextBox txtGmailAddress;
        private System.Windows.Forms.Label lblGmailPassword;
        private System.Windows.Forms.TextBox txtGmailPassword;
        private System.Windows.Forms.Button btnTestMail;
        private System.Windows.Forms.Label lblMailNote;
        private System.Windows.Forms.Label lblColService;
        private System.Windows.Forms.Label lblColVersion;
        private System.Windows.Forms.Label lblColPort;
        private System.Windows.Forms.Label lblColSslPort;
        private System.Windows.Forms.Label lblColSsl;
        private System.Windows.Forms.Label lblColEnabled;
        private System.Windows.Forms.Label lblSvc1Name;
        private System.Windows.Forms.Label lblSvc1Ver;
        private System.Windows.Forms.NumericUpDown numSvc1Port;
        private System.Windows.Forms.NumericUpDown numSvc1Ssl;
        private System.Windows.Forms.CheckBox chkSvc1Ssl;
        private System.Windows.Forms.CheckBox chkSvc1;
        private System.Windows.Forms.Label lblSvc2Name;
        private System.Windows.Forms.Label lblSvc2Ver;
        private System.Windows.Forms.NumericUpDown numSvc2Port;
        private System.Windows.Forms.NumericUpDown numSvc2Ssl;
        private System.Windows.Forms.CheckBox chkSvc2Ssl;
        private System.Windows.Forms.CheckBox chkSvc2;
        private System.Windows.Forms.Label lblSvc3Name;
        private System.Windows.Forms.Label lblSvc3Ver;
        private System.Windows.Forms.NumericUpDown numSvc3Port;
        private System.Windows.Forms.NumericUpDown numSvc3Ssl;
        private System.Windows.Forms.CheckBox chkSvc3Ssl;
        private System.Windows.Forms.CheckBox chkSvc3;
        private System.Windows.Forms.Label lblSvc4Name;
        private System.Windows.Forms.Label lblSvc4Ver;
        private System.Windows.Forms.NumericUpDown numSvc4Port;
        private System.Windows.Forms.NumericUpDown numSvc4Ssl;
        private System.Windows.Forms.CheckBox chkSvc4Ssl;
        private System.Windows.Forms.CheckBox chkSvc4;
        private System.Windows.Forms.Label lblSvc5Name;
        private System.Windows.Forms.Label lblSvc5Ver;
        private System.Windows.Forms.NumericUpDown numSvc5Port;
        private System.Windows.Forms.NumericUpDown numSvc5Ssl;
        private System.Windows.Forms.CheckBox chkSvc5Ssl;
        private System.Windows.Forms.CheckBox chkSvc5;
        private System.Windows.Forms.Label lblSvc6Name;
        private System.Windows.Forms.Label lblSvc6Ver;
        private System.Windows.Forms.NumericUpDown numSvc6Port;
        private System.Windows.Forms.NumericUpDown numSvc6Ssl;
        private System.Windows.Forms.CheckBox chkSvc6Ssl;
        private System.Windows.Forms.CheckBox chkSvc6;
        private System.Windows.Forms.Label lblSvc7Name;
        private System.Windows.Forms.Label lblSvc7Ver;
        private System.Windows.Forms.NumericUpDown numSvc7Port;
        private System.Windows.Forms.NumericUpDown numSvc7Ssl;
        private System.Windows.Forms.CheckBox chkSvc7Ssl;
        private System.Windows.Forms.CheckBox chkSvc7;
        private System.Windows.Forms.Label lblSvc8Name;
        private System.Windows.Forms.Label lblSvc10Name;
        private System.Windows.Forms.Label lblSvc9Name;
        private System.Windows.Forms.Label lblSvc8Ver;
        private System.Windows.Forms.Label lblSvc10Ver;
        private System.Windows.Forms.Label lblSvc9Ver;
        private System.Windows.Forms.NumericUpDown numSvc8Port;
        private System.Windows.Forms.NumericUpDown numSvc10Port;
        private System.Windows.Forms.NumericUpDown numSvc9Port;
        private System.Windows.Forms.NumericUpDown numSvc8Ssl;
        private System.Windows.Forms.NumericUpDown numSvc10Ssl;
        private System.Windows.Forms.NumericUpDown numSvc9Ssl;
        private System.Windows.Forms.CheckBox chkSvc8Ssl;
        private System.Windows.Forms.CheckBox chkSvc10Ssl;
        private System.Windows.Forms.CheckBox chkSvc9Ssl;
        private System.Windows.Forms.CheckBox chkSvc8;
        private System.Windows.Forms.CheckBox chkSvc10;
        private System.Windows.Forms.CheckBox chkSvc9;
        private System.Windows.Forms.Panel lineSep2;
        private System.Windows.Forms.Label lblServicesHint;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Label lblSettingsFile;
        private System.Windows.Forms.Panel panelButtons;
        private System.Windows.Forms.Panel panelButtonsLine;
        private System.Windows.Forms.TabControl tabPrefs;
        private System.Windows.Forms.TabPage tabGeneral;
        private System.Windows.Forms.TabPage tabServices;
    }
}
