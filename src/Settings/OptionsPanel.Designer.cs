namespace KeePassWinHello
{
	partial class OptionsPanel
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

		#region Designer Code

		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.winKeyStorageToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.winKeyStorageCheckBox = new System.Windows.Forms.CheckBox();
            this.winHelloDisabledPanel = new System.Windows.Forms.Panel();
            this.winHelloDisabledLabel = new System.Windows.Forms.Label();
            this.invalidationPanel = new System.Windows.Forms.Panel();
            this.storedKeysInfoPanel = new System.Windows.Forms.Panel();
            this.storedKeysInfoLabel = new System.Windows.Forms.Label();
            this.storedKeysCountLabel = new System.Windows.Forms.Label();
            this.btnRevokeAll = new System.Windows.Forms.Button();
            this.validPeriodComboBox = new System.Windows.Forms.ComboBox();
            this.validPeriodLabel = new System.Windows.Forms.Label();
            this.persistentStoragePanel = new System.Windows.Forms.Panel();
            this.keyCreatePanel = new System.Windows.Forms.Panel();
            this.keyCreateIcoPanel = new System.Windows.Forms.Panel();
            this.keyCreateLabel = new System.Windows.Forms.Label();
            this.isNotElevatedPanel = new System.Windows.Forms.Panel();
            this.uacIcoPanel = new System.Windows.Forms.Panel();
            this.isNotElevatedLabel = new System.Windows.Forms.Label();
            this.isEnabledCheckBox = new System.Windows.Forms.CheckBox();
            this.winHelloDisabledPanel.SuspendLayout();
            this.invalidationPanel.SuspendLayout();
            this.storedKeysInfoPanel.SuspendLayout();
            this.persistentStoragePanel.SuspendLayout();
            this.keyCreatePanel.SuspendLayout();
            this.isNotElevatedPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // winKeyStorageCheckBox
            // 
            this.winKeyStorageCheckBox.AutoSize = true;
            this.winKeyStorageCheckBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.winKeyStorageCheckBox.Location = new System.Drawing.Point(0, 0);
            this.winKeyStorageCheckBox.Name = "winKeyStorageCheckBox";
            this.winKeyStorageCheckBox.Padding = new System.Windows.Forms.Padding(5, 5, 0, 5);
            this.winKeyStorageCheckBox.Size = new System.Drawing.Size(550, 27);
            this.winKeyStorageCheckBox.TabIndex = 11;
            this.winKeyStorageCheckBox.Text = "Store keys in the Windows Credential Manager";
            this.winKeyStorageToolTip.SetToolTip(this.winKeyStorageCheckBox, "Use Windows Credential Manager for storing databases access keys while KeePass is n" +
        "ot running.\r\nRequires for KeePass to create persistent Windows Hello key.");
            this.winKeyStorageCheckBox.UseVisualStyleBackColor = true;
            this.winKeyStorageCheckBox.CheckedChanged += new System.EventHandler(this.WinKeyStorageCheckBox_CheckedChanged);
            // 
            // winHelloDisabledPanel
            // 
            this.winHelloDisabledPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(253)))), ((int)(((byte)(251)))), ((int)(((byte)(172)))));
            this.winHelloDisabledPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.winHelloDisabledPanel.Controls.Add(this.winHelloDisabledLabel);
            this.winHelloDisabledPanel.Cursor = System.Windows.Forms.Cursors.No;
            this.winHelloDisabledPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.winHelloDisabledPanel.ForeColor = System.Drawing.SystemColors.ScrollBar;
            this.winHelloDisabledPanel.Location = new System.Drawing.Point(0, 274);
            this.winHelloDisabledPanel.Name = "winHelloDisabledPanel";
            this.winHelloDisabledPanel.Size = new System.Drawing.Size(550, 24);
            this.winHelloDisabledPanel.TabIndex = 39;
            this.winHelloDisabledPanel.Visible = false;
            // 
            // winHelloDisabledLabel
            // 
            this.winHelloDisabledLabel.AutoSize = true;
            this.winHelloDisabledLabel.ForeColor = System.Drawing.Color.Red;
            this.winHelloDisabledLabel.Location = new System.Drawing.Point(79, 5);
            this.winHelloDisabledLabel.Name = "winHelloDisabledLabel";
            this.winHelloDisabledLabel.Size = new System.Drawing.Size(393, 13);
            this.winHelloDisabledLabel.TabIndex = 10;
            this.winHelloDisabledLabel.Text = "Windows Hello is disabled on your system. Please activate it in the system settin" +
    "gs";
            // 
            // invalidationPanel
            // 
            this.invalidationPanel.Controls.Add(this.storedKeysInfoPanel);
            this.invalidationPanel.Controls.Add(this.validPeriodComboBox);
            this.invalidationPanel.Controls.Add(this.validPeriodLabel);
            this.invalidationPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.invalidationPanel.Location = new System.Drawing.Point(0, 97);
            this.invalidationPanel.Name = "invalidationPanel";
            this.invalidationPanel.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.invalidationPanel.Size = new System.Drawing.Size(550, 28);
            this.invalidationPanel.TabIndex = 52;
            // 
            // storedKeysInfoPanel
            // 
            this.storedKeysInfoPanel.AutoSize = true;
            this.storedKeysInfoPanel.Controls.Add(this.storedKeysInfoLabel);
            this.storedKeysInfoPanel.Controls.Add(this.storedKeysCountLabel);
            this.storedKeysInfoPanel.Controls.Add(this.btnRevokeAll);
            this.storedKeysInfoPanel.Dock = System.Windows.Forms.DockStyle.Right;
            this.storedKeysInfoPanel.Location = new System.Drawing.Point(395, 5);
            this.storedKeysInfoPanel.Name = "storedKeysInfoPanel";
            this.storedKeysInfoPanel.Size = new System.Drawing.Size(155, 23);
            this.storedKeysInfoPanel.TabIndex = 46;
            // 
            // storedKeysInfoLabel
            // 
            this.storedKeysInfoLabel.AutoSize = true;
            this.storedKeysInfoLabel.Dock = System.Windows.Forms.DockStyle.Right;
            this.storedKeysInfoLabel.Location = new System.Drawing.Point(0, 0);
            this.storedKeysInfoLabel.Name = "storedKeysInfoLabel";
            this.storedKeysInfoLabel.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.storedKeysInfoLabel.Size = new System.Drawing.Size(66, 18);
            this.storedKeysInfoLabel.TabIndex = 48;
            this.storedKeysInfoLabel.Text = "Stored keys:";
            // 
            // storedKeysCountLabel
            // 
            this.storedKeysCountLabel.AutoSize = true;
            this.storedKeysCountLabel.Dock = System.Windows.Forms.DockStyle.Right;
            this.storedKeysCountLabel.Location = new System.Drawing.Point(66, 0);
            this.storedKeysCountLabel.Name = "storedKeysCountLabel";
            this.storedKeysCountLabel.Padding = new System.Windows.Forms.Padding(0, 5, 5, 0);
            this.storedKeysCountLabel.Size = new System.Drawing.Size(18, 18);
            this.storedKeysCountLabel.TabIndex = 47;
            this.storedKeysCountLabel.Text = "0";
            // 
            // btnRevokeAll
            // 
            this.btnRevokeAll.AutoSize = true;
            this.btnRevokeAll.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnRevokeAll.Location = new System.Drawing.Point(84, 0);
            this.btnRevokeAll.Name = "btnRevokeAll";
            this.btnRevokeAll.Size = new System.Drawing.Size(71, 23);
            this.btnRevokeAll.TabIndex = 46;
            this.btnRevokeAll.Text = "Revoke all";
            this.btnRevokeAll.UseVisualStyleBackColor = true;
            this.btnRevokeAll.Click += new System.EventHandler(this.BtnRevokeAll_Click);
            // 
            // validPeriodComboBox
            // 
            this.validPeriodComboBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.validPeriodComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.validPeriodComboBox.FormattingEnabled = true;
            this.validPeriodComboBox.Items.AddRange(new object[] {
            "Never",
            "1 Minute",
            "5 Minutes",
            "10 Minutes",
            "15 Minutes",
            "30 Minutes",
            "1 Hour",
            "2 Hours",
            "6 Hours",
            "12 Hours",
            "1 Day",
            "Week",
            "Month"});
            this.validPeriodComboBox.Location = new System.Drawing.Point(169, 5);
            this.validPeriodComboBox.Name = "validPeriodComboBox";
            this.validPeriodComboBox.Size = new System.Drawing.Size(136, 21);
            this.validPeriodComboBox.TabIndex = 45;
            // 
            // validPeriodLabel
            // 
            this.validPeriodLabel.AutoSize = true;
            this.validPeriodLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.validPeriodLabel.Location = new System.Drawing.Point(0, 5);
            this.validPeriodLabel.Margin = new System.Windows.Forms.Padding(8, 0, 3, 0);
            this.validPeriodLabel.Name = "validPeriodLabel";
            this.validPeriodLabel.Padding = new System.Windows.Forms.Padding(5, 4, 2, 0);
            this.validPeriodLabel.Size = new System.Drawing.Size(169, 17);
            this.validPeriodLabel.TabIndex = 44;
            this.validPeriodLabel.Text = "Saved keys get invalidated after:";
            // 
            // persistentStoragePanel
            // 
            this.persistentStoragePanel.AutoSize = true;
            this.persistentStoragePanel.Controls.Add(this.keyCreatePanel);
            this.persistentStoragePanel.Controls.Add(this.isNotElevatedPanel);
            this.persistentStoragePanel.Controls.Add(this.winKeyStorageCheckBox);
            this.persistentStoragePanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.persistentStoragePanel.Location = new System.Drawing.Point(0, 22);
            this.persistentStoragePanel.Name = "persistentStoragePanel";
            this.persistentStoragePanel.Size = new System.Drawing.Size(550, 75);
            this.persistentStoragePanel.TabIndex = 50;
            // 
            // keyCreatePanel
            // 
            this.keyCreatePanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(217)))), ((int)(((byte)(254)))));
            this.keyCreatePanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.keyCreatePanel.Controls.Add(this.keyCreateIcoPanel);
            this.keyCreatePanel.Controls.Add(this.keyCreateLabel);
            this.keyCreatePanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.keyCreatePanel.ForeColor = System.Drawing.SystemColors.ScrollBar;
            this.keyCreatePanel.Location = new System.Drawing.Point(0, 51);
            this.keyCreatePanel.Name = "keyCreatePanel";
            this.keyCreatePanel.Size = new System.Drawing.Size(550, 24);
            this.keyCreatePanel.TabIndex = 55;
            this.keyCreatePanel.Visible = false;
            // 
            // keyCreateIcoPanel
            // 
            this.keyCreateIcoPanel.Location = new System.Drawing.Point(3, 3);
            this.keyCreateIcoPanel.Name = "keyCreateIcoPanel";
            this.keyCreateIcoPanel.Size = new System.Drawing.Size(16, 16);
            this.keyCreateIcoPanel.TabIndex = 42;
            // 
            // keyCreateLabel
            // 
            this.keyCreateLabel.AutoSize = true;
            this.keyCreateLabel.BackColor = System.Drawing.Color.Transparent;
            this.keyCreateLabel.ForeColor = System.Drawing.SystemColors.InfoText;
            this.keyCreateLabel.Location = new System.Drawing.Point(19, 5);
            this.keyCreateLabel.Name = "keyCreateLabel";
            this.keyCreateLabel.Size = new System.Drawing.Size(464, 13);
            this.keyCreateLabel.TabIndex = 38;
            this.keyCreateLabel.Text = "A persistent key will be created. You will be prompted to sign it with your biometry " +
    "via Windows Hello.";
            // 
            // isNotElevatedPanel
            // 
            this.isNotElevatedPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(253)))), ((int)(((byte)(251)))), ((int)(((byte)(172)))));
            this.isNotElevatedPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.isNotElevatedPanel.Controls.Add(this.uacIcoPanel);
            this.isNotElevatedPanel.Controls.Add(this.isNotElevatedLabel);
            this.isNotElevatedPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.isNotElevatedPanel.ForeColor = System.Drawing.SystemColors.ScrollBar;
            this.isNotElevatedPanel.Location = new System.Drawing.Point(0, 27);
            this.isNotElevatedPanel.Name = "isNotElevatedPanel";
            this.isNotElevatedPanel.Size = new System.Drawing.Size(550, 24);
            this.isNotElevatedPanel.TabIndex = 54;
            this.isNotElevatedPanel.Visible = false;
            // 
            // uacIcoPanel
            // 
            this.uacIcoPanel.Location = new System.Drawing.Point(3, 3);
            this.uacIcoPanel.Name = "uacIcoPanel";
            this.uacIcoPanel.Size = new System.Drawing.Size(16, 16);
            this.uacIcoPanel.TabIndex = 42;
            // 
            // isNotElevatedLabel
            // 
            this.isNotElevatedLabel.AutoSize = true;
            this.isNotElevatedLabel.BackColor = System.Drawing.Color.Transparent;
            this.isNotElevatedLabel.ForeColor = System.Drawing.SystemColors.InfoText;
            this.isNotElevatedLabel.Location = new System.Drawing.Point(19, 5);
            this.isNotElevatedLabel.Name = "isNotElevatedLabel";
            this.isNotElevatedLabel.Size = new System.Drawing.Size(294, 13);
            this.isNotElevatedLabel.TabIndex = 38;
            this.isNotElevatedLabel.Text = "Requires for KeePass process to be running as Administrator.";
            // 
            // isEnabledCheckBox
            // 
            this.isEnabledCheckBox.AutoSize = true;
            this.isEnabledCheckBox.Checked = true;
            this.isEnabledCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.isEnabledCheckBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.isEnabledCheckBox.Location = new System.Drawing.Point(0, 0);
            this.isEnabledCheckBox.Name = "isEnabledCheckBox";
            this.isEnabledCheckBox.Padding = new System.Windows.Forms.Padding(5, 5, 0, 0);
            this.isEnabledCheckBox.Size = new System.Drawing.Size(550, 22);
            this.isEnabledCheckBox.TabIndex = 1;
            this.isEnabledCheckBox.Text = "Use quick unlock via Windows Hello authorization if it is available";
            this.isEnabledCheckBox.UseVisualStyleBackColor = true;
            this.isEnabledCheckBox.CheckedChanged += new System.EventHandler(this.isEnabledCheckBox_CheckedChanged);
            // 
            // OptionsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.invalidationPanel);
            this.Controls.Add(this.persistentStoragePanel);
            this.Controls.Add(this.isEnabledCheckBox);
            this.Controls.Add(this.winHelloDisabledPanel);
            this.Name = "OptionsPanel";
            this.Size = new System.Drawing.Size(550, 298);
            this.winHelloDisabledPanel.ResumeLayout(false);
            this.winHelloDisabledPanel.PerformLayout();
            this.invalidationPanel.ResumeLayout(false);
            this.invalidationPanel.PerformLayout();
            this.storedKeysInfoPanel.ResumeLayout(false);
            this.storedKeysInfoPanel.PerformLayout();
            this.persistentStoragePanel.ResumeLayout(false);
            this.persistentStoragePanel.PerformLayout();
            this.keyCreatePanel.ResumeLayout(false);
            this.keyCreatePanel.PerformLayout();
            this.isNotElevatedPanel.ResumeLayout(false);
            this.isNotElevatedPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
        private System.Windows.Forms.ToolTip winKeyStorageToolTip;
        private System.Windows.Forms.Panel winHelloDisabledPanel;
        private System.Windows.Forms.Label winHelloDisabledLabel;
        private System.Windows.Forms.Panel invalidationPanel;
        private System.Windows.Forms.Label validPeriodLabel;
        private System.Windows.Forms.ComboBox validPeriodComboBox;
        private System.Windows.Forms.Panel persistentStoragePanel;
        private System.Windows.Forms.CheckBox winKeyStorageCheckBox;
        private System.Windows.Forms.Panel isNotElevatedPanel;
        private System.Windows.Forms.Panel uacIcoPanel;
        private System.Windows.Forms.Label isNotElevatedLabel;
        private System.Windows.Forms.CheckBox isEnabledCheckBox;
        private System.Windows.Forms.Panel keyCreatePanel;
        private System.Windows.Forms.Panel keyCreateIcoPanel;
        private System.Windows.Forms.Label keyCreateLabel;
        private System.Windows.Forms.Panel storedKeysInfoPanel;
        private System.Windows.Forms.Label storedKeysInfoLabel;
        private System.Windows.Forms.Label storedKeysCountLabel;
        private System.Windows.Forms.Button btnRevokeAll;
    }
}
