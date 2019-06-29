using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KeePassWinHello
{
	public partial class OptionsPanel : UserControl
	{
        private const long VALID_UNLIMITED = Settings.VALID_UNLIMITED_PERIOD;
        private const long VALID_1MINUTE = 60 * 1000;
        private const long VALID_5MINUTES = VALID_1MINUTE * 5;
        private const long VALID_10MINUTES = VALID_5MINUTES * 2;
        private const long VALID_15MINUTES = VALID_5MINUTES * 3;
        private const long VALID_30MINUTES = VALID_15MINUTES * 2;
        private const long VALID_1HOUR = VALID_30MINUTES * 2;
        private const long VALID_2HOURS = VALID_1HOUR * 2;
        private const long VALID_6HOURS = VALID_2HOURS * 3;
        private const long VALID_12HOURS = VALID_6HOURS * 2;
        private const long VALID_1DAY = VALID_12HOURS * 2;
        private const long VALID_7DAYS = VALID_1DAY * 7;
        private const long VALID_MONTH = VALID_1DAY * 30;
        private const long VALID_DEFAULT = Settings.VALID_PERIOD_DEFAULT;

        private readonly bool _isAvailable;
        private bool _initialized = false;

        private static long IndexToPeriod(int index)
        {
            switch (index)
            {
                case 0: return VALID_UNLIMITED;
                case 1: return VALID_1MINUTE;
                case 2: return VALID_5MINUTES;
                case 3: return VALID_10MINUTES;
                case 4: return VALID_15MINUTES;
                case 5: return VALID_30MINUTES;
                case 6: return VALID_1HOUR;
                case 7: return VALID_2HOURS;
                case 8: return VALID_6HOURS;
                case 9: return VALID_12HOURS;
                case 10: return VALID_1DAY;
                case 11: return VALID_7DAYS;
                case 12: return VALID_MONTH;
                default: return VALID_DEFAULT;
            }
        }

        private static int PeriodToIndex(TimeSpan timeSpan)
        {
            switch ((long)timeSpan.TotalMilliseconds)
            {
                case VALID_UNLIMITED: return 0;
                case VALID_1MINUTE: return 1;
                case VALID_5MINUTES: return 2;
                case VALID_10MINUTES: return 3;
                case VALID_15MINUTES: return 4;
                case VALID_30MINUTES: return 5;
                case VALID_1HOUR: return 6;
                case VALID_2HOURS: return 7;
                case VALID_6HOURS: return 8;
                case VALID_12HOURS: return 9;
                case VALID_1DAY: return 10;
                case VALID_7DAYS: return 11;
                case VALID_MONTH: return 12;
                default: return 10;
            }
        }

        internal static void AddTab(TabControl m_tabMain, ImageList imageList, bool isAvailable)
        {
            Debug.Assert(m_tabMain != null);
            if (m_tabMain == null)
                return;

            if (imageList == null)
            {
                if (m_tabMain.ImageList == null)
                    m_tabMain.ImageList = new ImageList();
                imageList = m_tabMain.ImageList;
            }

            var imageIndex = imageList.Images.Add(Properties.Resources.windows_hello16x16, Color.Transparent);
            var optionsPanel = new OptionsPanel(isAvailable);

            var newTab = new TabPage(Settings.OptionsTabName)
            {
                UseVisualStyleBackColor = true,
                ImageIndex = imageIndex
            };

            newTab.Controls.Add(optionsPanel);
            optionsPanel.Dock = DockStyle.Fill;

            m_tabMain.TabPages.Add(newTab);
            m_tabMain.Multiline = false;
        }

        OptionsPanel(bool isAvailable)
        {
            InitializeComponent();

            _isAvailable = isAvailable;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (_initialized)
                return;

            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;

            Debug.Assert(ParentForm != null);
            if (ParentForm != null)
                ParentForm.FormClosing += OnClosing;

            LoadValuesFromSettings();

            bool isEnabled = Settings.Instance.Enabled;
            if (!_isAvailable || !isEnabled)
            {
                validPeriodComboBox.Enabled = false;
                winKeyStorageCheckBox.Enabled = false;
                btnRevokeAll.Enabled = false;

                if (!_isAvailable)
                {
                    isEnabledCheckBox.Enabled = false;
                    winHelloDisabledPanel.Visible = true;
                }
            }
            else
            {
                bool isElevated = UAC.IsCurrentProcessElevated;
                winKeyStorageCheckBox.Enabled = isElevated;
                splitContainer1.Panel1Collapsed = isElevated;
                DrawUacShild();
            }

            _initialized = true;
        }

        private void LoadValuesFromSettings()
        {
            isEnabledCheckBox.Checked = Settings.Instance.Enabled;
            winKeyStorageCheckBox.Checked = Settings.Instance.WinStorageEnabled;
            validPeriodComboBox.SelectedIndex = PeriodToIndex(Settings.Instance.InvalidatingTime);
        }

        private void OnClosing(object sender, FormClosingEventArgs e)
        {
            if (ParentForm.DialogResult == DialogResult.OK)
            {
                var settings = Settings.Instance;
                settings.Enabled = isEnabledCheckBox.Checked;
                if (isEnabledCheckBox.Checked)
                {
                    settings.InvalidatingTime =
                        TimeSpan.FromMilliseconds(IndexToPeriod(validPeriodComboBox.SelectedIndex));
                    settings.WinStorageEnabled = winKeyStorageCheckBox.Enabled;
                }
            }
        }

        private void isEnabledCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            bool isEnabled = isEnabledCheckBox.Checked;
            btnRevokeAll.Enabled = isEnabled;
            validPeriodComboBox.Enabled = isEnabled;
            winKeyStorageCheckBox.Enabled = isEnabled && UAC.IsCurrentProcessElevated;
        }

        private void DrawUacShild()
        {
            if (!isNotElevatedPanel.Visible)
                return;

            const int IDI_SHIELD = 32518;
            const int SM_CXSMICON = 49;
            const int SM_CYSMICON = 50;

            int cx = WinAPI.GetSystemMetrics(SM_CXSMICON);
            int cy = WinAPI.GetSystemMetrics(SM_CYSMICON);
            IntPtr hShieldIcon;
            int result = WinAPI.LoadIconWithScaleDown(IntPtr.Zero, IDI_SHIELD, cx, cy, out hShieldIcon);
            if (result >= 0)
            {
                uacIco.Image = Bitmap.FromHicon(hShieldIcon);
            }
        }


        private static class WinAPI
        {
            [DllImport("comctl32.dll", SetLastError = true)]
            public static extern int LoadIconWithScaleDown(IntPtr hinst, int pszName, int cx, int cy, out IntPtr phico);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern int GetSystemMetrics(int nIndex);
        }
    }
}
