namespace KeePassWinHello
{
	partial class OptionsPanel
	{
		/// <summary> 
		/// Erforderliche Designervariable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Verwendete Ressourcen bereinigen.
		/// </summary>
		/// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Vom Komponenten-Designer generierter Code

		/// <summary> 
		/// Erforderliche Methode für die Designerunterstützung. 
		/// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.winKeyStorageToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.winHelloDisabledPanel = new System.Windows.Forms.Panel();
            this.winHelloDisabledLabel = new System.Windows.Forms.Label();
            this.invalidationPanel = new System.Windows.Forms.Panel();
            this.btnRevokeAll = new System.Windows.Forms.Button();
            this.infoLabel = new System.Windows.Forms.Label();
            this.validPeriodComboBox = new System.Windows.Forms.ComboBox();
            this.persistentStoragePanel = new System.Windows.Forms.Panel();
            this.winKeyStorageCheckBox = new System.Windows.Forms.CheckBox();
            this.isEnabledCheckBox = new System.Windows.Forms.CheckBox();
            this.isNotElevatedPanel = new System.Windows.Forms.Panel();
            this.uacIcoPanel = new System.Windows.Forms.Panel();
            this.isNotElevatedLabel = new System.Windows.Forms.Label();
            this.winHelloDisabledPanel.SuspendLayout();
            this.invalidationPanel.SuspendLayout();
            this.persistentStoragePanel.SuspendLayout();
            this.isNotElevatedPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // winHelloDisabledPanel
            // 
            this.winHelloDisabledPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(253)))), ((int)(((byte)(251)))), ((int)(((byte)(172)))));
            this.winHelloDisabledPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.winHelloDisabledPanel.Controls.Add(this.winHelloDisabledLabel);
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
            this.invalidationPanel.Controls.Add(this.btnRevokeAll);
            this.invalidationPanel.Controls.Add(this.validPeriodComboBox);
            this.invalidationPanel.Controls.Add(this.infoLabel);
            this.invalidationPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.invalidationPanel.Location = new System.Drawing.Point(0, 73);
            this.invalidationPanel.Name = "invalidationPanel";
            this.invalidationPanel.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.invalidationPanel.Size = new System.Drawing.Size(550, 31);
            this.invalidationPanel.TabIndex = 52;
            // 
            // btnRevokeAll
            // 
            this.btnRevokeAll.AutoSize = true;
            this.btnRevokeAll.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnRevokeAll.Location = new System.Drawing.Point(477, 4);
            this.btnRevokeAll.Margin = new System.Windows.Forms.Padding(3, 10, 3, 4);
            this.btnRevokeAll.Name = "btnRevokeAll";
            this.btnRevokeAll.Size = new System.Drawing.Size(68, 23);
            this.btnRevokeAll.TabIndex = 46;
            this.btnRevokeAll.Text = "Revoke all";
            this.btnRevokeAll.UseVisualStyleBackColor = true;
            this.btnRevokeAll.Visible = false;
            // 
            // infoLabel
            // 
            this.infoLabel.AutoSize = true;
            this.infoLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.infoLabel.Location = new System.Drawing.Point(0, 5);
            this.infoLabel.Margin = new System.Windows.Forms.Padding(8, 0, 3, 0);
            this.infoLabel.Name = "infoLabel";
            this.infoLabel.Padding = new System.Windows.Forms.Padding(5, 4, 2, 0);
            this.infoLabel.Size = new System.Drawing.Size(169, 17);
            this.infoLabel.TabIndex = 44;
            this.infoLabel.Text = "Saved keys get invalidated after:";
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
            // persistentStoragePanel
            // 
            this.persistentStoragePanel.AutoSize = true;
            this.persistentStoragePanel.Controls.Add(this.isNotElevatedPanel);
            this.persistentStoragePanel.Controls.Add(this.winKeyStorageCheckBox);
            this.persistentStoragePanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.persistentStoragePanel.Location = new System.Drawing.Point(0, 22);
            this.persistentStoragePanel.Name = "persistentStoragePanel";
            this.persistentStoragePanel.Size = new System.Drawing.Size(550, 51);
            this.persistentStoragePanel.TabIndex = 50;
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
            this.winKeyStorageToolTip.SetToolTip(this.winKeyStorageCheckBox, "Use Windows Credential Manager for store databases access keys while KeePass is n" +
        "ot running.\r\nRequires for KeePass process to be running as Administrator.");
            this.winKeyStorageCheckBox.UseVisualStyleBackColor = true;
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
            this.isEnabledCheckBox.TabIndex = 54;
            this.isEnabledCheckBox.Text = "Use quick unlock via Windows Hello authorization if it is available";
            this.isEnabledCheckBox.UseVisualStyleBackColor = true;
            this.isEnabledCheckBox.CheckedChanged += new System.EventHandler(this.isEnabledCheckBox_CheckedChanged);
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
            this.persistentStoragePanel.ResumeLayout(false);
            this.persistentStoragePanel.PerformLayout();
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
        private System.Windows.Forms.Button btnRevokeAll;
        private System.Windows.Forms.Label infoLabel;
        private System.Windows.Forms.ComboBox validPeriodComboBox;
        private System.Windows.Forms.Panel persistentStoragePanel;
        private System.Windows.Forms.CheckBox winKeyStorageCheckBox;
        private System.Windows.Forms.Panel isNotElevatedPanel;
        private System.Windows.Forms.Panel uacIcoPanel;
        private System.Windows.Forms.Label isNotElevatedLabel;
        private System.Windows.Forms.CheckBox isEnabledCheckBox;
    }
}
