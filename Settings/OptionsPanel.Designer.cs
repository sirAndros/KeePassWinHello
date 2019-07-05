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
            this.isEnabledCheckBox = new System.Windows.Forms.CheckBox();
            this.winKeyStorageToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.winKeyStorageCheckBox = new System.Windows.Forms.CheckBox();
            this.winHelloDisabledPanel = new System.Windows.Forms.Panel();
            this.winHelloDisabledLabel = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.isNotElevatedPanel = new System.Windows.Forms.Panel();
            this.isNotElevatedLabel = new System.Windows.Forms.Label();
            this.btnRevokeAll = new System.Windows.Forms.Button();
            this.infoLabel = new System.Windows.Forms.Label();
            this.validPeriodComboBox = new System.Windows.Forms.ComboBox();
            this.uacIcoPanel = new System.Windows.Forms.Panel();
            this.winHelloDisabledPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.isNotElevatedPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // isEnabledCheckBox
            // 
            this.isEnabledCheckBox.AutoSize = true;
            this.isEnabledCheckBox.Checked = true;
            this.isEnabledCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.isEnabledCheckBox.Location = new System.Drawing.Point(3, 13);
            this.isEnabledCheckBox.Name = "isEnabledCheckBox";
            this.isEnabledCheckBox.Size = new System.Drawing.Size(334, 17);
            this.isEnabledCheckBox.TabIndex = 8;
            this.isEnabledCheckBox.Text = "Use quick unlock via Windows Hello authorization if it is available";
            this.isEnabledCheckBox.UseVisualStyleBackColor = true;
            this.isEnabledCheckBox.CheckedChanged += new System.EventHandler(this.isEnabledCheckBox_CheckedChanged);
            // 
            // winKeyStorageCheckBox
            // 
            this.winKeyStorageCheckBox.AutoSize = true;
            this.winKeyStorageCheckBox.Location = new System.Drawing.Point(3, 36);
            this.winKeyStorageCheckBox.Name = "winKeyStorageCheckBox";
            this.winKeyStorageCheckBox.Size = new System.Drawing.Size(247, 17);
            this.winKeyStorageCheckBox.TabIndex = 9;
            this.winKeyStorageCheckBox.Text = "Store keys in the Windows Credential Manager";
            this.winKeyStorageToolTip.SetToolTip(this.winKeyStorageCheckBox, "Use Windows Credential Manager for store databases access keys while KeePass is n" +
        "ot running.\r\nRequires for KeePass process to be running as Administrator.");
            this.winKeyStorageCheckBox.UseVisualStyleBackColor = true;
            // 
            // winHelloDisabledPanel
            // 
            this.winHelloDisabledPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(253)))), ((int)(((byte)(251)))), ((int)(((byte)(172)))));
            this.winHelloDisabledPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.winHelloDisabledPanel.Controls.Add(this.winHelloDisabledLabel);
            this.winHelloDisabledPanel.ForeColor = System.Drawing.SystemColors.ScrollBar;
            this.winHelloDisabledPanel.Location = new System.Drawing.Point(2, 272);
            this.winHelloDisabledPanel.Name = "winHelloDisabledPanel";
            this.winHelloDisabledPanel.Size = new System.Drawing.Size(552, 24);
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
            // splitContainer1
            // 
            this.splitContainer1.Location = new System.Drawing.Point(3, 59);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.isNotElevatedPanel);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.btnRevokeAll);
            this.splitContainer1.Panel2.Controls.Add(this.infoLabel);
            this.splitContainer1.Panel2.Controls.Add(this.validPeriodComboBox);
            this.splitContainer1.Size = new System.Drawing.Size(553, 100);
            this.splitContainer1.SplitterDistance = 25;
            this.splitContainer1.TabIndex = 40;
            // 
            // isNotElevatedPanel
            // 
            this.isNotElevatedPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(253)))), ((int)(((byte)(251)))), ((int)(((byte)(172)))));
            this.isNotElevatedPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.isNotElevatedPanel.Controls.Add(this.uacIcoPanel);
            this.isNotElevatedPanel.Controls.Add(this.isNotElevatedLabel);
            this.isNotElevatedPanel.ForeColor = System.Drawing.SystemColors.ScrollBar;
            this.isNotElevatedPanel.Location = new System.Drawing.Point(0, 1);
            this.isNotElevatedPanel.Name = "isNotElevatedPanel";
            this.isNotElevatedPanel.Size = new System.Drawing.Size(550, 24);
            this.isNotElevatedPanel.TabIndex = 39;
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
            // btnRevokeAll
            // 
            this.btnRevokeAll.Location = new System.Drawing.Point(476, 2);
            this.btnRevokeAll.Name = "btnRevokeAll";
            this.btnRevokeAll.Size = new System.Drawing.Size(75, 23);
            this.btnRevokeAll.TabIndex = 39;
            this.btnRevokeAll.Text = "Revoke all";
            this.btnRevokeAll.UseVisualStyleBackColor = true;
            this.btnRevokeAll.Visible = false;
            // 
            // infoLabel
            // 
            this.infoLabel.AutoSize = true;
            this.infoLabel.Location = new System.Drawing.Point(2, 7);
            this.infoLabel.Name = "infoLabel";
            this.infoLabel.Size = new System.Drawing.Size(162, 13);
            this.infoLabel.TabIndex = 37;
            this.infoLabel.Text = "Saved keys get invalidated after:";
            // 
            // validPeriodComboBox
            // 
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
            this.validPeriodComboBox.Location = new System.Drawing.Point(192, 4);
            this.validPeriodComboBox.Name = "validPeriodComboBox";
            this.validPeriodComboBox.Size = new System.Drawing.Size(136, 21);
            this.validPeriodComboBox.TabIndex = 38;
            // 
            // uacIcoPanel
            // 
            this.uacIcoPanel.Location = new System.Drawing.Point(3, 3);
            this.uacIcoPanel.Name = "uacIcoPanel";
            this.uacIcoPanel.Size = new System.Drawing.Size(16, 16);
            this.uacIcoPanel.TabIndex = 42;
            // 
            // OptionsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.winHelloDisabledPanel);
            this.Controls.Add(this.winKeyStorageCheckBox);
            this.Controls.Add(this.isEnabledCheckBox);
            this.Name = "OptionsPanel";
            this.Size = new System.Drawing.Size(556, 298);
            this.winHelloDisabledPanel.ResumeLayout(false);
            this.winHelloDisabledPanel.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.isNotElevatedPanel.ResumeLayout(false);
            this.isNotElevatedPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.CheckBox isEnabledCheckBox;
        private System.Windows.Forms.ToolTip winKeyStorageToolTip;
        private System.Windows.Forms.Panel winHelloDisabledPanel;
        private System.Windows.Forms.Label winHelloDisabledLabel;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.CheckBox winKeyStorageCheckBox;
        private System.Windows.Forms.Panel isNotElevatedPanel;
        private System.Windows.Forms.Label isNotElevatedLabel;
        private System.Windows.Forms.Button btnRevokeAll;
        private System.Windows.Forms.Label infoLabel;
        private System.Windows.Forms.ComboBox validPeriodComboBox;
        private System.Windows.Forms.Panel uacIcoPanel;
    }
}
