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
            this.infoLabel = new System.Windows.Forms.Label();
            this.validPeriodComboBox = new System.Windows.Forms.ComboBox();
            this.isEnabledCheckBox = new System.Windows.Forms.CheckBox();
            this.winHelloDisabled = new System.Windows.Forms.Label();
            this.btnRevokeAll = new System.Windows.Forms.Button();
            this.winKeyStorageCheckBox = new System.Windows.Forms.CheckBox();
            this.winKeyStorageToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.isNotElevatedLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // infoLabel
            // 
            this.infoLabel.AutoSize = true;
            this.infoLabel.Location = new System.Drawing.Point(0, 66);
            this.infoLabel.Name = "infoLabel";
            this.infoLabel.Size = new System.Drawing.Size(407, 13);
            this.infoLabel.TabIndex = 4;
            this.infoLabel.Text = "Select the time range after which a key protected by Windows Hello gets invalidat" +
    "ed:";
            // 
            // validPeriodComboBox
            // 
            this.validPeriodComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.validPeriodComboBox.FormattingEnabled = true;
            this.validPeriodComboBox.Items.AddRange(new object[] {
            "Unlimited",
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
            this.validPeriodComboBox.Location = new System.Drawing.Point(3, 82);
            this.validPeriodComboBox.Name = "validPeriodComboBox";
            this.validPeriodComboBox.Size = new System.Drawing.Size(186, 21);
            this.validPeriodComboBox.TabIndex = 10;
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
            // winHelloDisabled
            // 
            this.winHelloDisabled.AutoSize = true;
            this.winHelloDisabled.ForeColor = System.Drawing.Color.Red;
            this.winHelloDisabled.Location = new System.Drawing.Point(3, 275);
            this.winHelloDisabled.Name = "winHelloDisabled";
            this.winHelloDisabled.Size = new System.Drawing.Size(393, 13);
            this.winHelloDisabled.TabIndex = 9;
            this.winHelloDisabled.Text = "Windows Hello is disabled on your system. Please activate it in the system settin" +
    "gs";
            this.winHelloDisabled.Visible = false;
            // 
            // btnRevokeAll
            // 
            this.btnRevokeAll.Location = new System.Drawing.Point(460, 265);
            this.btnRevokeAll.Name = "btnRevokeAll";
            this.btnRevokeAll.Size = new System.Drawing.Size(75, 23);
            this.btnRevokeAll.TabIndex = 36;
            this.btnRevokeAll.Text = "Revoke all";
            this.btnRevokeAll.UseVisualStyleBackColor = true;
            this.btnRevokeAll.Visible = false;
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
        "ot running.\nRequires KeePass process to be running as Administrator.");
            this.winKeyStorageCheckBox.UseVisualStyleBackColor = true;
            // 
            // isNotElevatedLabel
            // 
            this.isNotElevatedLabel.AutoSize = true;
            this.isNotElevatedLabel.ForeColor = System.Drawing.Color.DarkGoldenrod;
            this.isNotElevatedLabel.Location = new System.Drawing.Point(259, 37);
            this.isNotElevatedLabel.Name = "isNotElevatedLabel";
            this.isNotElevatedLabel.Size = new System.Drawing.Size(279, 13);
            this.isNotElevatedLabel.TabIndex = 37;
            this.isNotElevatedLabel.Text = "Requires KeePass process to be running as Administrator.";
            this.isNotElevatedLabel.Visible = false;
            // 
            // OptionsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.isNotElevatedLabel);
            this.Controls.Add(this.winKeyStorageCheckBox);
            this.Controls.Add(this.btnRevokeAll);
            this.Controls.Add(this.winHelloDisabled);
            this.Controls.Add(this.isEnabledCheckBox);
            this.Controls.Add(this.infoLabel);
            this.Controls.Add(this.validPeriodComboBox);
            this.Name = "OptionsPanel";
            this.Size = new System.Drawing.Size(556, 298);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label infoLabel;
		private System.Windows.Forms.ComboBox validPeriodComboBox;
		private System.Windows.Forms.CheckBox isEnabledCheckBox;
        private System.Windows.Forms.Label winHelloDisabled;
        private System.Windows.Forms.Button btnRevokeAll;
        private System.Windows.Forms.CheckBox winKeyStorageCheckBox;
        private System.Windows.Forms.ToolTip winKeyStorageToolTip;
        private System.Windows.Forms.Label isNotElevatedLabel;
    }
}
