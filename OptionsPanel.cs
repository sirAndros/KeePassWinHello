using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace KeePassWinHello
{
	public partial class OptionsPanel : UserControl
	{
		/// <summary>Intialize with the config.</summary>
		public OptionsPanel(KeePassWinHelloExt plugin)
		{
			InitializeComponent();

			SetStyle(ControlStyles.SupportsTransparentBackColor, true);
			BackColor = Color.Transparent;

			var config = KeePassWinHelloExt.Host.CustomConfig;

			autoPromptCheckBox.Checked = config.GetBool(WinHelloProvider.CfgAutoPrompt, true);

			validPeriodComboBox.SelectedIndex = PeriodToIndex(
				config.GetULong(
					WinHelloProvider.CfgValidPeriod,
					WinHelloProvider.VALID_DEFAULT
				)
			);
		}

		/// <summary>Converts the combobox index to a valid period.</summary>
		/// <param name="index">Index of the combobox.</param>
		/// <returns>The valid periods in seconds.</returns>
		private ulong IndexToPeriod(int index)
		{
			switch (index)
			{
				case 0: return WinHelloProvider.VALID_UNLIMITED;
				case 1: return WinHelloProvider.VALID_1MINUTE;
				case 2: return WinHelloProvider.VALID_5MINUTES;
				case 3: return WinHelloProvider.VALID_10MINUTES;
				case 4: return WinHelloProvider.VALID_15MINUTES;
				case 5: return WinHelloProvider.VALID_30MINUTES;
				case 6: return WinHelloProvider.VALID_1HOUR;
				case 7: return WinHelloProvider.VALID_2HOURS;
				case 8: return WinHelloProvider.VALID_6HOURS;
				case 9: return WinHelloProvider.VALID_12HOURS;
				case 10: return WinHelloProvider.VALID_1DAY;
				case 11: return WinHelloProvider.VALID_7DAYS;
				case 12: return WinHelloProvider.VALID_MONTH;
                default:return WinHelloProvider.VALID_DEFAULT;
			}
		}

		/// <summary>Converts the valid period to the combobox item index.</summary>
		/// <param name="period">The valid period in secons.</param>
		/// <returns>The index of the combobox item.</returns>
		private int PeriodToIndex(ulong period)
		{
			switch (period)
			{
				case WinHelloProvider.VALID_UNLIMITED: return 0;
				case WinHelloProvider.VALID_1MINUTE: return 1;
				case WinHelloProvider.VALID_5MINUTES: return 2;
				case WinHelloProvider.VALID_10MINUTES: return 3;
				case WinHelloProvider.VALID_15MINUTES: return 4;
				case WinHelloProvider.VALID_30MINUTES: return 5;
				case WinHelloProvider.VALID_1HOUR: return 6;
				case WinHelloProvider.VALID_2HOURS: return 7;
				case WinHelloProvider.VALID_6HOURS: return 8;
				case WinHelloProvider.VALID_12HOURS: return 9;
				case WinHelloProvider.VALID_1DAY: return 10;
				case WinHelloProvider.VALID_7DAYS: return 11;
				case WinHelloProvider.VALID_MONTH: return 12;
                default: return 10;
			}
		}

		/// <summary>Register for the FormClosing event.</summary>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (ParentForm != null)
			{
				// Save the settings on FormClosing.
				ParentForm.FormClosing += delegate (object sender2, FormClosingEventArgs e2)
				{
					if (ParentForm.DialogResult == DialogResult.OK)
					{
						var config = KeePassWinHelloExt.Host.CustomConfig;

						config.SetBool(
							WinHelloProvider.CfgAutoPrompt,
							autoPromptCheckBox.Checked
						);
						config.SetULong(
							WinHelloProvider.CfgValidPeriod,
							IndexToPeriod(validPeriodComboBox.SelectedIndex)
						);
					}
				};
			}

            if (!WinHello.IsAvailable())
            {
                autoPromptCheckBox.Enabled = false;
                validPeriodComboBox.Enabled = false;
                winHelloDisabled.Visible = true;
            }
		}
	}
}
