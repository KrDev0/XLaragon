namespace Laragon
{
    partial class QuickCreateForm
    {
        private System.ComponentModel.IContainer components = null;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QuickCreateForm));
            lblName = new Label();
            txtName = new TextBox();
            lblAddress = new Label();
            lblAddressValue = new Label();
            lblType = new Label();
            cboType = new ComboBox();
            lblFolder = new Label();
            lblFolderValue = new Label();
            chkOpen = new CheckBox();
            btnCreate = new Button();
            btnCancel = new Button();
            SuspendLayout();
            // 
            // lblName
            // 
            lblName.Location = new Point(16, 24);
            lblName.Name = "lblName";
            lblName.Size = new Size(70, 18);
            lblName.TabIndex = 0;
            lblName.Text = "Name";
            // 
            // txtName
            // 
            txtName.Location = new Point(92, 21);
            txtName.Name = "txtName";
            txtName.Size = new Size(220, 21);
            txtName.TabIndex = 1;
            txtName.Text = "mysite";
            // 
            // lblAddress
            // 
            lblAddress.Location = new Point(16, 54);
            lblAddress.Name = "lblAddress";
            lblAddress.Size = new Size(70, 18);
            lblAddress.TabIndex = 2;
            lblAddress.Text = "Address";
            // 
            // lblAddressValue
            // 
            lblAddressValue.ForeColor = Color.DimGray;
            lblAddressValue.Location = new Point(92, 54);
            lblAddressValue.Name = "lblAddressValue";
            lblAddressValue.Size = new Size(320, 18);
            lblAddressValue.TabIndex = 3;
            lblAddressValue.Text = "http://mysite.test/";
            // 
            // lblType
            // 
            lblType.Location = new Point(16, 84);
            lblType.Name = "lblType";
            lblType.Size = new Size(70, 18);
            lblType.TabIndex = 4;
            lblType.Text = "Type";
            // 
            // cboType
            // 
            cboType.DropDownStyle = ComboBoxStyle.DropDownList;
            cboType.Items.AddRange(new object[] { "PHP", "Wordpress", "Laravel", "Lumen", "Node.js", "Python", "Ruby", "Go", "Java", "Docker", "Kubernetes" });
            cboType.Location = new Point(92, 81);
            cboType.Name = "cboType";
            cboType.Size = new Size(220, 23);
            cboType.TabIndex = 5;
            // 
            // lblFolder
            // 
            lblFolder.Location = new Point(16, 114);
            lblFolder.Name = "lblFolder";
            lblFolder.Size = new Size(70, 18);
            lblFolder.TabIndex = 6;
            lblFolder.Text = "Folder";
            // 
            // lblFolderValue
            // 
            lblFolderValue.ForeColor = Color.DimGray;
            lblFolderValue.Location = new Point(92, 114);
            lblFolderValue.Name = "lblFolderValue";
            lblFolderValue.Size = new Size(330, 18);
            lblFolderValue.TabIndex = 7;
            lblFolderValue.Text = "www\\mysite";
            // 
            // chkOpen
            // 
            chkOpen.Checked = true;
            chkOpen.CheckState = CheckState.Checked;
            chkOpen.Location = new Point(92, 142);
            chkOpen.Name = "chkOpen";
            chkOpen.Size = new Size(320, 20);
            chkOpen.TabIndex = 8;
            chkOpen.Text = "Open the folder when it is created";
            chkOpen.UseVisualStyleBackColor = true;
            // 
            // btnCreate
            // 
            btnCreate.Location = new Point(264, 172);
            btnCreate.Name = "btnCreate";
            btnCreate.Size = new Size(76, 28);
            btnCreate.TabIndex = 9;
            btnCreate.Text = "Create";
            btnCreate.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(348, 172);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(76, 28);
            btnCancel.TabIndex = 10;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // QuickCreateForm
            // 
            AcceptButton = btnCreate;
            AutoScaleMode = AutoScaleMode.None;
            BackColor = Color.White;
            CancelButton = btnCancel;
            ClientSize = new Size(440, 214);
            Controls.Add(btnCancel);
            Controls.Add(btnCreate);
            Controls.Add(chkOpen);
            Controls.Add(lblFolderValue);
            Controls.Add(lblFolder);
            Controls.Add(cboType);
            Controls.Add(lblType);
            Controls.Add(lblAddressValue);
            Controls.Add(lblAddress);
            Controls.Add(txtName);
            Controls.Add(lblName);
            Font = new Font("Arial", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "QuickCreateForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Quick create website";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label lblAddress;
        private System.Windows.Forms.Label lblAddressValue;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.ComboBox cboType;
        private System.Windows.Forms.Label lblFolder;
        private System.Windows.Forms.Label lblFolderValue;
        private System.Windows.Forms.CheckBox chkOpen;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.Button btnCancel;
    }
}
