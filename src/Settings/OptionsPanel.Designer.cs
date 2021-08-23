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
            this.keyStorageSettingsToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.winKeyStorageCheckBox = new System.Windows.Forms.CheckBox();
            this.revokeOnCancel = new System.Windows.Forms.CheckBox();
            this.winHelloDisabledPanel = new System.Windows.Forms.Panel();
            this.winHelloDisabledLabel = new System.Windows.Forms.Label();
            this.invalidationPanel = new System.Windows.Forms.Panel();
            this.storedKeysInfoPanel = new System.Windows.Forms.Panel();
            this.storedKeysInfoLabel = new System.Windows.Forms.Label();
            this.storedKeysCountLabel = new System.Windows.Forms.Label();
            this.revokeAllCheckBox = new System.Windows.Forms.CheckBox();
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
            this.linkToGitHub = new System.Windows.Forms.LinkLabel();
            this.bottomPanel = new System.Windows.Forms.Panel();
            this.forceKeysRevokeToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.winHelloDisabledPanel.SuspendLayout();
            this.invalidationPanel.SuspendLayout();
            this.storedKeysInfoPanel.SuspendLayout();
            this.persistentStoragePanel.SuspendLayout();
            this.keyCreatePanel.SuspendLayout();
            this.isNotElevatedPanel.SuspendLayout();
            this.bottomPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // winKeyStorageCheckBox
            // 
            this.winKeyStorageCheckBox.AutoSize = true;
            this.winKeyStorageCheckBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.winKeyStorageCheckBox.Location = new System.Drawing.Point(0, 0);
            this.winKeyStorageCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.winKeyStorageCheckBox.Name = "winKeyStorageCheckBox";
            this.winKeyStorageCheckBox.Padding = new System.Windows.Forms.Padding(7, 6, 0, 6);
            this.winKeyStorageCheckBox.Size = new System.Drawing.Size(733, 33);
            this.winKeyStorageCheckBox.TabIndex = 3;
            this.winKeyStorageCheckBox.Text = "Store keys in the Windows Credential Manager";
            this.keyStorageSettingsToolTip.SetToolTip(this.winKeyStorageCheckBox, "Use Windows Credential Manager for storing databases access keys while KeePass is" +
        " not running.\r\nRequires for KeePass to create persistent Windows Hello key.");
            this.winKeyStorageCheckBox.UseVisualStyleBackColor = true;
            // 
            // revokeOnCancel
            // 
            this.revokeOnCancel.AutoSize = true;
            this.revokeOnCancel.Checked = true;
            this.revokeOnCancel.CheckState = System.Windows.Forms.CheckState.Checked;
            this.revokeOnCancel.Dock = System.Windows.Forms.DockStyle.Top;
            this.revokeOnCancel.Location = new System.Drawing.Point(0, 27);
            this.revokeOnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.revokeOnCancel.Name = "revokeOnCancel";
            this.revokeOnCancel.Padding = new System.Windows.Forms.Padding(7, 6, 0, 0);
            this.revokeOnCancel.Size = new System.Drawing.Size(733, 27);
            this.revokeOnCancel.TabIndex = 2;
            this.revokeOnCancel.Text = "Revoke current key in case Windows Hello prompt was cancelled";
            this.keyStorageSettingsToolTip.SetToolTip(this.revokeOnCancel, "If enabled, you\'ll need to manually enter the password next time you unlock DB if" +
        " the Windows Hello prompt was previously cancelled by a user");
            this.revokeOnCancel.UseVisualStyleBackColor = true;
            // 
            // winHelloDisabledPanel
            // 
            this.winHelloDisabledPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(253)))), ((int)(((byte)(251)))), ((int)(((byte)(172)))));
            this.winHelloDisabledPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.winHelloDisabledPanel.Controls.Add(this.winHelloDisabledLabel);
            this.winHelloDisabledPanel.Cursor = System.Windows.Forms.Cursors.No;
            this.winHelloDisabledPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.winHelloDisabledPanel.ForeColor = System.Drawing.SystemColors.ScrollBar;
            this.winHelloDisabledPanel.Location = new System.Drawing.Point(0, 338);
            this.winHelloDisabledPanel.Margin = new System.Windows.Forms.Padding(4);
            this.winHelloDisabledPanel.Name = "winHelloDisabledPanel";
            this.winHelloDisabledPanel.Size = new System.Drawing.Size(733, 29);
            this.winHelloDisabledPanel.TabIndex = 39;
            this.winHelloDisabledPanel.Visible = false;
            // 
            // winHelloDisabledLabel
            // 
            this.winHelloDisabledLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.winHelloDisabledLabel.ForeColor = System.Drawing.Color.Red;
            this.winHelloDisabledLabel.Location = new System.Drawing.Point(0, 0);
            this.winHelloDisabledLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.winHelloDisabledLabel.Name = "winHelloDisabledLabel";
            this.winHelloDisabledLabel.Size = new System.Drawing.Size(731, 27);
            this.winHelloDisabledLabel.TabIndex = 10;
            this.winHelloDisabledLabel.Text = "Windows Hello is not available.";
            this.winHelloDisabledLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // invalidationPanel
            // 
            this.invalidationPanel.Controls.Add(this.storedKeysInfoPanel);
            this.invalidationPanel.Controls.Add(this.validPeriodComboBox);
            this.invalidationPanel.Controls.Add(this.validPeriodLabel);
            this.invalidationPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.invalidationPanel.Location = new System.Drawing.Point(0, 145);
            this.invalidationPanel.Margin = new System.Windows.Forms.Padding(4);
            this.invalidationPanel.Name = "invalidationPanel";
            this.invalidationPanel.Padding = new System.Windows.Forms.Padding(0, 6, 5, 0);
            this.invalidationPanel.Size = new System.Drawing.Size(733, 34);
            this.invalidationPanel.TabIndex = 52;
            // 
            // storedKeysInfoPanel
            // 
            this.storedKeysInfoPanel.AutoSize = true;
            this.storedKeysInfoPanel.Controls.Add(this.storedKeysInfoLabel);
            this.storedKeysInfoPanel.Controls.Add(this.storedKeysCountLabel);
            this.storedKeysInfoPanel.Controls.Add(this.revokeAllCheckBox);
            this.storedKeysInfoPanel.Dock = System.Windows.Forms.DockStyle.Right;
            this.storedKeysInfoPanel.Location = new System.Drawing.Point(534, 6);
            this.storedKeysInfoPanel.Margin = new System.Windows.Forms.Padding(4);
            this.storedKeysInfoPanel.Name = "storedKeysInfoPanel";
            this.storedKeysInfoPanel.Size = new System.Drawing.Size(194, 28);
            this.storedKeysInfoPanel.TabIndex = 46;
            // 
            // storedKeysInfoLabel
            // 
            this.storedKeysInfoLabel.AutoSize = true;
            this.storedKeysInfoLabel.Dock = System.Windows.Forms.DockStyle.Right;
            this.storedKeysInfoLabel.Location = new System.Drawing.Point(0, 0);
            this.storedKeysInfoLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.storedKeysInfoLabel.Name = "storedKeysInfoLabel";
            this.storedKeysInfoLabel.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.storedKeysInfoLabel.Size = new System.Drawing.Size(87, 23);
            this.storedKeysInfoLabel.TabIndex = 48;
            this.storedKeysInfoLabel.Text = "Stored keys:";
            // 
            // storedKeysCountLabel
            // 
            this.storedKeysCountLabel.AutoSize = true;
            this.storedKeysCountLabel.Dock = System.Windows.Forms.DockStyle.Right;
            this.storedKeysCountLabel.Location = new System.Drawing.Point(87, 0);
            this.storedKeysCountLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.storedKeysCountLabel.Name = "storedKeysCountLabel";
            this.storedKeysCountLabel.Padding = new System.Windows.Forms.Padding(0, 6, 7, 0);
            this.storedKeysCountLabel.Size = new System.Drawing.Size(23, 23);
            this.storedKeysCountLabel.TabIndex = 47;
            this.storedKeysCountLabel.Text = "0";
            // 
            // revokeAllCheckBox
            // 
            this.revokeAllCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
            this.revokeAllCheckBox.AutoSize = true;
            this.revokeAllCheckBox.Dock = System.Windows.Forms.DockStyle.Right;
            this.revokeAllCheckBox.Location = new System.Drawing.Point(110, 0);
            this.revokeAllCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.revokeAllCheckBox.Name = "revokeAllCheckBox";
            this.revokeAllCheckBox.Size = new System.Drawing.Size(84, 28);
            this.revokeAllCheckBox.TabIndex = 5;
            this.revokeAllCheckBox.Text = "Revoke all";
            this.keyStorageSettingsToolTip.SetToolTip(this.revokeAllCheckBox, "You can toggle this button to claim/withdraw intent to remove all stored keys on " +
        "settings confirmation with \"OK\" button");
            this.revokeAllCheckBox.UseVisualStyleBackColor = true;
            this.revokeAllCheckBox.CheckedChanged += new System.EventHandler(this.RevokeAll_CheckedChanged);
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
            "3 Days",
            "Week",
            "Month"});
            this.validPeriodComboBox.Location = new System.Drawing.Point(224, 6);
            this.validPeriodComboBox.Margin = new System.Windows.Forms.Padding(4);
            this.validPeriodComboBox.Name = "validPeriodComboBox";
            this.validPeriodComboBox.Size = new System.Drawing.Size(180, 24);
            this.validPeriodComboBox.TabIndex = 4;
            // 
            // validPeriodLabel
            // 
            this.validPeriodLabel.AutoSize = true;
            this.validPeriodLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.validPeriodLabel.Location = new System.Drawing.Point(0, 6);
            this.validPeriodLabel.Margin = new System.Windows.Forms.Padding(11, 0, 4, 0);
            this.validPeriodLabel.Name = "validPeriodLabel";
            this.validPeriodLabel.Padding = new System.Windows.Forms.Padding(7, 5, 3, 0);
            this.validPeriodLabel.Size = new System.Drawing.Size(224, 22);
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
            this.persistentStoragePanel.Location = new System.Drawing.Point(0, 54);
            this.persistentStoragePanel.Margin = new System.Windows.Forms.Padding(4);
            this.persistentStoragePanel.Name = "persistentStoragePanel";
            this.persistentStoragePanel.Size = new System.Drawing.Size(733, 91);
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
            this.keyCreatePanel.Location = new System.Drawing.Point(0, 62);
            this.keyCreatePanel.Margin = new System.Windows.Forms.Padding(4);
            this.keyCreatePanel.Name = "keyCreatePanel";
            this.keyCreatePanel.Size = new System.Drawing.Size(733, 29);
            this.keyCreatePanel.TabIndex = 55;
            this.keyCreatePanel.Visible = false;
            // 
            // keyCreateIcoPanel
            // 
            this.keyCreateIcoPanel.Location = new System.Drawing.Point(4, 4);
            this.keyCreateIcoPanel.Margin = new System.Windows.Forms.Padding(4);
            this.keyCreateIcoPanel.Name = "keyCreateIcoPanel";
            this.keyCreateIcoPanel.Size = new System.Drawing.Size(21, 20);
            this.keyCreateIcoPanel.TabIndex = 42;
            // 
            // keyCreateLabel
            // 
            this.keyCreateLabel.AutoSize = true;
            this.keyCreateLabel.BackColor = System.Drawing.Color.Transparent;
            this.keyCreateLabel.ForeColor = System.Drawing.SystemColors.InfoText;
            this.keyCreateLabel.Location = new System.Drawing.Point(25, 6);
            this.keyCreateLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.keyCreateLabel.Name = "keyCreateLabel";
            this.keyCreateLabel.Size = new System.Drawing.Size(639, 17);
            this.keyCreateLabel.TabIndex = 38;
            this.keyCreateLabel.Text = "A persistent key will be created. You will be prompted to sign it with your biome" +
    "try via Windows Hello.";
            // 
            // isNotElevatedPanel
            // 
            this.isNotElevatedPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(253)))), ((int)(((byte)(251)))), ((int)(((byte)(172)))));
            this.isNotElevatedPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.isNotElevatedPanel.Controls.Add(this.uacIcoPanel);
            this.isNotElevatedPanel.Controls.Add(this.isNotElevatedLabel);
            this.isNotElevatedPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.isNotElevatedPanel.ForeColor = System.Drawing.SystemColors.ScrollBar;
            this.isNotElevatedPanel.Location = new System.Drawing.Point(0, 33);
            this.isNotElevatedPanel.Margin = new System.Windows.Forms.Padding(4);
            this.isNotElevatedPanel.Name = "isNotElevatedPanel";
            this.isNotElevatedPanel.Size = new System.Drawing.Size(733, 29);
            this.isNotElevatedPanel.TabIndex = 54;
            this.isNotElevatedPanel.Visible = false;
            // 
            // uacIcoPanel
            // 
            this.uacIcoPanel.Location = new System.Drawing.Point(4, 4);
            this.uacIcoPanel.Margin = new System.Windows.Forms.Padding(4);
            this.uacIcoPanel.Name = "uacIcoPanel";
            this.uacIcoPanel.Size = new System.Drawing.Size(21, 20);
            this.uacIcoPanel.TabIndex = 42;
            // 
            // isNotElevatedLabel
            // 
            this.isNotElevatedLabel.AutoSize = true;
            this.isNotElevatedLabel.BackColor = System.Drawing.Color.Transparent;
            this.isNotElevatedLabel.ForeColor = System.Drawing.SystemColors.InfoText;
            this.isNotElevatedLabel.Location = new System.Drawing.Point(25, 6);
            this.isNotElevatedLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.isNotElevatedLabel.Name = "isNotElevatedLabel";
            this.isNotElevatedLabel.Size = new System.Drawing.Size(398, 17);
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
            this.isEnabledCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.isEnabledCheckBox.Name = "isEnabledCheckBox";
            this.isEnabledCheckBox.Padding = new System.Windows.Forms.Padding(7, 6, 0, 0);
            this.isEnabledCheckBox.Size = new System.Drawing.Size(733, 27);
            this.isEnabledCheckBox.TabIndex = 1;
            this.isEnabledCheckBox.Text = "Use quick unlock via Windows Hello authorization if it is available";
            this.isEnabledCheckBox.UseVisualStyleBackColor = true;
            // 
            // linkToGitHub
            // 
            this.linkToGitHub.AutoSize = true;
            this.linkToGitHub.Dock = System.Windows.Forms.DockStyle.Right;
            this.linkToGitHub.Location = new System.Drawing.Point(635, 0);
            this.linkToGitHub.Name = "linkToGitHub";
            this.linkToGitHub.Padding = new System.Windows.Forms.Padding(0, 0, 5, 0);
            this.linkToGitHub.Size = new System.Drawing.Size(98, 17);
            this.linkToGitHub.TabIndex = 53;
            this.linkToGitHub.TabStop = true;
            this.linkToGitHub.Text = "Project Home";
            this.linkToGitHub.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkToGitHub_LinkClicked);
            // 
            // bottomPanel
            // 
            this.bottomPanel.Controls.Add(this.linkToGitHub);
            this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bottomPanel.Location = new System.Drawing.Point(0, 316);
            this.bottomPanel.Name = "bottomPanel";
            this.bottomPanel.Size = new System.Drawing.Size(733, 22);
            this.bottomPanel.TabIndex = 54;
            // 
            // OptionsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.bottomPanel);
            this.Controls.Add(this.invalidationPanel);
            this.Controls.Add(this.persistentStoragePanel);
            this.Controls.Add(this.revokeOnCancel);
            this.Controls.Add(this.isEnabledCheckBox);
            this.Controls.Add(this.winHelloDisabledPanel);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "OptionsPanel";
            this.Size = new System.Drawing.Size(733, 367);
            this.winHelloDisabledPanel.ResumeLayout(false);
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
            this.bottomPanel.ResumeLayout(false);
            this.bottomPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolTip keyStorageSettingsToolTip;
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
        private System.Windows.Forms.CheckBox revokeAllCheckBox;
        private System.Windows.Forms.CheckBox revokeOnCancel;
        private System.Windows.Forms.LinkLabel linkToGitHub;
        private System.Windows.Forms.Panel bottomPanel;
        private System.Windows.Forms.ToolTip forceKeysRevokeToolTip;
    }
}
