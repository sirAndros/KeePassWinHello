using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace KeePassWinHello
{
	public partial class OptionsPanel : UserControl
	{
        private readonly IKeyManager _keyManager;
        private bool _initialized = false;

        private OptionsPanel(IKeyManager keyManager)
        {
            InitializeComponent();

            _keyManager = keyManager;
        }

        internal static void AddTab(TabControl m_tabMain, ImageList imageList, IKeyManager keyManager)
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
            var optionsPanel = new OptionsPanel(keyManager);

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

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (_initialized)
                return;

            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;

            validPeriodComboBox.SelectedIndex = PeriodToIndex(Settings.Instance.InvalidatingTime);
            if (_keyManager != null)
                storedKeysCountLabel.Text = _keyManager.KeysCount.ToString();

            bool isEnabled = Settings.Instance.Enabled;
            bool isAvailable = _keyManager != null && _keyManager.IsAvailable;

            Debug.Assert(ParentForm != null);
            if (ParentForm != null)
                ParentForm.FormClosing += OnClosing;

            if (!isAvailable || !isEnabled)
            {
                isEnabledCheckBox.Checked = false;
                validPeriodComboBox.Enabled = false;
                btnRevokeAll.Enabled = false;

                if (!isAvailable)
                {
                    isEnabledCheckBox.Enabled = false;
                    winHelloDisabled.Visible = true;
                }
            }

            _initialized = true;
        }

        private void OnClosing(object sender, FormClosingEventArgs e)
        {
            if (ParentForm.DialogResult == DialogResult.OK)
            {
                Settings settings = Settings.Instance;
                if (settings.Enabled != isEnabledCheckBox.Checked)
                {
                    settings.Enabled = isEnabledCheckBox.Checked;
                    if (!isEnabledCheckBox.Checked)
                    {
                        RevokeAllKeys();
                    }
                }
              
                if (isEnabledCheckBox.Checked)
                {
                    var newInvalidatingTime = TimeSpan.FromMilliseconds(IndexToPeriod(validPeriodComboBox.SelectedIndex));
                    if (settings.InvalidatingTime != newInvalidatingTime)
                    {
                        settings.InvalidatingTime = newInvalidatingTime;
                    }
                }
            }
        }

        private void isEnabledCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            validPeriodComboBox.Enabled = isEnabledCheckBox.Checked;
            btnRevokeAll.Enabled = isEnabledCheckBox.Checked;
        }

        private void BtnRevokeAll_Click(object sender, EventArgs e)
        {
            RevokeAllKeys();
        }

        private void RevokeAllKeys()
        {
            if (_keyManager != null)
            {
                _keyManager.RevokeAll();
                storedKeysCountLabel.Text = _keyManager.KeysCount.ToString();
            }
        }


        #region InvalidatingTimeProcessing

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

        #endregion InvalidatingTimeProcessing
    }
}
