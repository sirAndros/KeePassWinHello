﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using KeePassWinHello.Utilities;

namespace KeePassWinHello
{
    public partial class OptionsPanel : UserControl
    {
        private readonly IKeyManager _keyManager;
        private bool _initialized;

        private OptionsPanel(IKeyManager keyManager)
        {
            InitializeComponent();

            _keyManager = keyManager;
            uacIcoPanel.Paint += OnPaint_ElevatedIconPanel;
            keyCreateIcoPanel.Paint += OnPaint_KeyCreateIconPanel;
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

            InitializeControls();

            _initialized = true;
        }

        private void InitializeControls()
        {
            bool isEnabled = Settings.Instance.Enabled;
            bool isAvailable = _keyManager != null;

            LoadValuesFromSettings();
            ProcessControlsVisibility(isEnabled && isAvailable);

            if (!isAvailable)
            {
                isEnabledCheckBox.Enabled = false;
                winHelloDisabledPanel.Visible = true;
            }
        }

        private void LoadValuesFromSettings()
        {
            isEnabledCheckBox.Checked = Settings.Instance.Enabled;
            revokeOnCancel.Checked = Settings.Instance.RevokeOnCancel;
            winKeyStorageCheckBox.Checked = Settings.Instance.WinStorageEnabled;
            validPeriodComboBox.SelectedIndex = PeriodToIndex(Settings.Instance.InvalidatingTime);
        }

        private void OnClosing(object sender, FormClosingEventArgs e)
        {
            if (ParentForm.DialogResult == DialogResult.OK)
            {
                Settings settings = Settings.Instance;
                SaveSettings(settings);
            }
            ParentForm.FormClosing -= OnClosing;
        }

        private void isEnabledCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            bool isEnabled = isEnabledCheckBox.Checked;
            ProcessControlsVisibility(isEnabled);
        }

        private void WinKeyStorageCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            keyCreatePanel.Visible = winKeyStorageCheckBox.Checked && !Settings.Instance.WinStorageEnabled;
        }

        private void BtnRevokeAll_Click(object sender, EventArgs e)
        {
            RevokeAllKeys();
        }

        private void SaveSettings(Settings settings)
        {
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

                settings.RevokeOnCancel = revokeOnCancel.Checked;

                if (settings.WinStorageEnabled != winKeyStorageCheckBox.Checked)
                {
                    settings.WinStorageEnabled = winKeyStorageCheckBox.Checked;
                    if (_keyManager != null)
                    {
                        var authCacheType = settings.GetAuthCacheType();
                        using (AuthProviderUIContext.With(Settings.KeyCreationConfirmationMessage, this.Handle))
                        {
                            try
                            {
                                _keyManager.ClaimCurrentCacheType(authCacheType);
                            }
                            catch (AuthProviderUserCancelledException)
                            {
                                _keyManager.ClaimCurrentCacheType(AuthCacheType.Local);
                                MessageBox.Show(AuthProviderUIContext.Current, "Creating persistent key for Credential Manager has been canceled", Settings.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            catch (Exception ex)
                            {
                                _keyManager.ClaimCurrentCacheType(AuthCacheType.Local);
                                ErrorHandler.ShowError(ex);
                            }
                        }
                    }
                }
            }
        }

        private void ProcessControlsVisibility(bool isEnabled)
        {
            validPeriodLabel.Enabled = isEnabled;
            validPeriodComboBox.Enabled = isEnabled;
            revokeOnCancel.Enabled = isEnabled;
            winKeyStorageCheckBox.Enabled = isEnabled;

            keyCreatePanel.Visible = isEnabled
                                 && winKeyStorageCheckBox.Checked
                                 && !Settings.Instance.WinStorageEnabled;

            ProcessStoredKeysVisibility(isEnabled);
        }

        private void ProcessStoredKeysVisibility(bool isEnabled)
        {
            bool isAvailable = _keyManager != null;
            int keysCount = isAvailable ? _keyManager.KeysCount : 0;
            bool savedKeysExists = keysCount > 0;

            storedKeysInfoPanel.Visible = isAvailable;
            btnRevokeAll.Enabled = savedKeysExists;
            storedKeysInfoLabel.Enabled = savedKeysExists || isEnabled;
            storedKeysCountLabel.Enabled = savedKeysExists || isEnabled;
            storedKeysCountLabel.Text = keysCount.ToString();
        }

        private void RevokeAllKeys()
        {
            if (_keyManager != null)
            {
                try
                {
                    _keyManager.RevokeAll();
                }
                catch (Exception ex)
                {
                    ErrorHandler.ShowError(ex);
                }
            }
            ProcessStoredKeysVisibility(isEnabledCheckBox.Checked);
        }


        #region Icons
        private void OnPaint_KeyCreateIconPanel(object sender, PaintEventArgs e)
        {
            DrawIcon(e.Graphics, WinAPI.SHSTOCKICONID.SIID_INFO);
        }
        private void OnPaint_ElevatedIconPanel(object sender, PaintEventArgs e)
        {
            DrawIcon(e.Graphics, WinAPI.SHSTOCKICONID.SIID_SHIELD);
        }

        private static void DrawIcon(Graphics graphics, WinAPI.SHSTOCKICONID iconId)
        {
            var sii = new WinAPI.SHSTOCKICONINFO();
            sii.cbSize = (UInt32)Marshal.SizeOf(typeof(WinAPI.SHSTOCKICONINFO));

            WinAPI.SHGetStockIconInfo(iconId, WinAPI.SHGSI.SHGSI_ICON | WinAPI.SHGSI.SHGSI_SMALLICON, ref sii)
                .ThrowOnError();

            IntPtr hIcon = sii.hIcon;

            var hdc = graphics.GetHdc();
            try
            {
                WinAPI.DrawIconEx(hdc, 0, 0, hIcon, 0, 0, 0, IntPtr.Zero, 0x0003)
                    .ThrowOnError("DrawIconEx");
            }
            finally
            {
                graphics.ReleaseHdc(hdc);
                WinAPI.DestroyIcon(hIcon);
            }
        }


        private static class WinAPI
        {
            [DllImport("User32.dll", SetLastError = true)]
            public static extern BOOL DrawIconEx(IntPtr hdc,
                                                  int xLeft,
                                                  int yTop,
                                                  IntPtr hIcon,
                                                  int cxWidth,
                                                  int cyWidth,
                                                  uint istepIfAniCur,
                                                  IntPtr hbrFlickerFreeDraw,
                                                  uint diFlags);

            [DllImport("User32.dll", SetLastError = true)]
            public static extern BOOL DestroyIcon(IntPtr hIcon);


            [DllImport("Shell32.dll", SetLastError = false)]
            public static extern HRESULT SHGetStockIconInfo(SHSTOCKICONID siid, SHGSI uFlags, ref SHSTOCKICONINFO psii);

            [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct SHSTOCKICONINFO
            {
                public UInt32 cbSize;
                public IntPtr hIcon;
                public Int32 iSysIconIndex;
                public Int32 iIcon;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
                public string szPath;
            }

            [Flags]
            public enum SHGSI : uint
            {
                SHGSI_ICONLOCATION = 0,
                SHGSI_ICON = 0x000000100,
                SHGSI_SYSICONINDEX = 0x000004000,
                SHGSI_LINKOVERLAY = 0x000008000,
                SHGSI_SELECTED = 0x000010000,
                SHGSI_LARGEICON = 0x000000000,
                SHGSI_SMALLICON = 0x000000001,
                SHGSI_SHELLICONSIZE = 0x000000004
            }

            public enum SHSTOCKICONID : uint
            {
                SIID_FIND = 22,
                SIID_HELP = 23,
                SIID_SHARE = 28,
                SIID_LINK = 29,
                SIID_RECYCLER = 31,
                SIID_RECYCLERFULL = 32,
                SIID_LOCK = 47,
                SIID_SHIELD = 77,
                SIID_WARNING = 78,
                SIID_INFO = 79,
                SIID_ERROR = 80,
                SIID_KEY = 81,
                SIID_DELETE = 84,
            }
        }
        #endregion

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
        private const long VALID_3DAYS = VALID_1DAY * 3;
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
                case 11: return VALID_3DAYS;
                case 12: return VALID_7DAYS;
                case 13: return VALID_MONTH;
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
                case VALID_3DAYS: return 11;
                case VALID_7DAYS: return 12;
                case VALID_MONTH: return 13;
                default: return 10;
            }
        }

        #endregion InvalidatingTimeProcessing
    }
}
