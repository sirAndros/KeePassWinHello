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
            this.infoLabel = new System.Windows.Forms.Label();
            this.validPeriodComboBox = new System.Windows.Forms.ComboBox();
            this.autoPromptCheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // infoLabel
            // 
            this.infoLabel.AutoSize = true;
            this.infoLabel.Location = new System.Drawing.Point(0, 33);
            this.infoLabel.Name = "infoLabel";
            this.infoLabel.Size = new System.Drawing.Size(317, 13);
            this.infoLabel.TabIndex = 4;
            this.infoLabel.Text = "Select the time range after which a WinHello key gets invalidated:";
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
            this.validPeriodComboBox.Location = new System.Drawing.Point(3, 49);
            this.validPeriodComboBox.Name = "validPeriodComboBox";
            this.validPeriodComboBox.Size = new System.Drawing.Size(186, 21);
            this.validPeriodComboBox.TabIndex = 5;
            // 
            // autoPromptCheckBox
            // 
            this.autoPromptCheckBox.AutoSize = true;
            this.autoPromptCheckBox.Checked = true;
            this.autoPromptCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.autoPromptCheckBox.Location = new System.Drawing.Point(3, 13);
            this.autoPromptCheckBox.Name = "autoPromptCheckBox";
            this.autoPromptCheckBox.Size = new System.Drawing.Size(256, 17);
            this.autoPromptCheckBox.TabIndex = 8;
            this.autoPromptCheckBox.Text = "Auto prompt for the WinHello key if it is available.";
            this.autoPromptCheckBox.UseVisualStyleBackColor = true;
            // 
            // OptionsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.autoPromptCheckBox);
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
		private System.Windows.Forms.CheckBox autoPromptCheckBox;
	}
}
