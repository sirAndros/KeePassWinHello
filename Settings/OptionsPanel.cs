using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using KeePassWinHello.Utilities;

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
            uacIcoPanel.Paint += OptionsPanel_Paint;
        }

        private void OptionsPanel_Paint(object sender, PaintEventArgs e)
        {
            var sii = new WinAPI.SHSTOCKICONINFO();
            sii.cbSize = (UInt32)Marshal.SizeOf(typeof(WinAPI.SHSTOCKICONINFO));

            Marshal.ThrowExceptionForHR(WinAPI.SHGetStockIconInfo(WinAPI.SHSTOCKICONID.SIID_SHIELD,
                WinAPI.SHGSI.SHGSI_ICON | WinAPI.SHGSI.SHGSI_SMALLICON,
                ref sii));

            IntPtr hShieldIcon = sii.hIcon;

            var graphics = e.Graphics;
            var hdc = graphics.GetHdc();
            try
            {
                var r = WinAPI.DrawIconEx(hdc, 0, 0, hShieldIcon, 0, 0, 0, IntPtr.Zero, 0x0003);
                if (!r)
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            }
            finally
            {
                graphics.ReleaseHdc(hdc);
                WinAPI.DestroyIcon(hShieldIcon);
            }
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
            const int IMAGE_ICON = 1;
            const int SM_CXSMICON = 49;
            const int SM_CYSMICON = 50;

            int cx = 48; //WinAPI.GetSystemMetrics(SM_CXSMICON);
            int cy = 48; //WinAPI.GetSystemMetrics(SM_CYSMICON);
            IntPtr hShieldIcon;
            int result = WinAPI.LoadIconWithScaleDown(IntPtr.Zero, IDI_SHIELD, cx, cy, out hShieldIcon);

            //hShieldIcon = WinAPI.LoadImageW(IntPtr.Zero, IDI_SHIELD, IMAGE_ICON, cx, cy, 0x00008000); //0x00000040 | 
            //hShieldIcon.CheckAndPass();
            //if (result >= 0)
            //{
            //uacIco.Image = Bitmap.FromHicon(sii.hIcon); //ResizeImage(Bitmap.FromHicon(hShieldIcon), uacIco.Width, uacIco.Height);
                //var img = Bitmap.FromHicon(hShieldIcon);
                //uacIco.Width = img.Width;
                //uacIco.Height = img.Height;
                //uacIco.Image = img;
                //}
                //uacIco.Width = cx;
                //uacIco.Height = cy;
                //var destImage = new Bitmap(cx, cy);
                //using (var graphics = Graphics.FromImage(destImage))
                //{
                //    var hdc = graphics.GetHdc();
                //    try
                //    {
                //        var r = WinAPI.DrawIcon(hdc, 0, 0, hShieldIcon);
                //        if (!r)
                //            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                //    }
                //    finally
                //    {
                //        graphics.ReleaseHdc(hdc);
                //    }
                //}
                //uacIco.Image = destImage;
        }

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            //destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }



        private static class WinAPI
        {
            [DllImport("comctl32.dll", SetLastError = true)]
            public static extern int LoadIconWithScaleDown(IntPtr hinst, int pszName, int cx, int cy, out IntPtr phico);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern IntPtr LoadImageW(IntPtr hinst, int name, uint type, int cx, int cy, uint fuLoad);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern int GetSystemMetrics(int nIndex);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool DrawIcon(IntPtr hDC, int x, int y, IntPtr hIcon);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool DrawIconEx(IntPtr hdc,
  int xLeft,
  int yTop,
  IntPtr hIcon,
  int cxWidth,
  int cyWidth,
  uint istepIfAniCur,
  IntPtr hbrFlickerFreeDraw,
  uint diFlags);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool DestroyIcon(IntPtr hIcon);


            [DllImport("Shell32.dll", SetLastError = false)]
            public static extern Int32 SHGetStockIconInfo(SHSTOCKICONID siid, SHGSI uFlags, ref SHSTOCKICONINFO psii);

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
                SIID_DOCNOASSOC = 0,
                SIID_DOCASSOC = 1,
                SIID_APPLICATION = 2,
                SIID_FOLDER = 3,
                SIID_FOLDEROPEN = 4,
                SIID_DRIVE525 = 5,
                SIID_DRIVE35 = 6,
                SIID_DRIVEREMOVE = 7,
                SIID_DRIVEFIXED = 8,
                SIID_DRIVENET = 9,
                SIID_DRIVENETDISABLED = 10,
                SIID_DRIVECD = 11,
                SIID_DRIVERAM = 12,
                SIID_WORLD = 13,
                SIID_SERVER = 15,
                SIID_PRINTER = 16,
                SIID_MYNETWORK = 17,
                SIID_FIND = 22,
                SIID_HELP = 23,
                SIID_SHARE = 28,
                SIID_LINK = 29,
                SIID_SLOWFILE = 30,
                SIID_RECYCLER = 31,
                SIID_RECYCLERFULL = 32,
                SIID_MEDIACDAUDIO = 40,
                SIID_LOCK = 47,
                SIID_AUTOLIST = 49,
                SIID_PRINTERNET = 50,
                SIID_SERVERSHARE = 51,
                SIID_PRINTERFAX = 52,
                SIID_PRINTERFAXNET = 53,
                SIID_PRINTERFILE = 54,
                SIID_STACK = 55,
                SIID_MEDIASVCD = 56,
                SIID_STUFFEDFOLDER = 57,
                SIID_DRIVEUNKNOWN = 58,
                SIID_DRIVEDVD = 59,
                SIID_MEDIADVD = 60,
                SIID_MEDIADVDRAM = 61,
                SIID_MEDIADVDRW = 62,
                SIID_MEDIADVDR = 63,
                SIID_MEDIADVDROM = 64,
                SIID_MEDIACDAUDIOPLUS = 65,
                SIID_MEDIACDRW = 66,
                SIID_MEDIACDR = 67,
                SIID_MEDIACDBURN = 68,
                SIID_MEDIABLANKCD = 69,
                SIID_MEDIACDROM = 70,
                SIID_AUDIOFILES = 71,
                SIID_IMAGEFILES = 72,
                SIID_VIDEOFILES = 73,
                SIID_MIXEDFILES = 74,
                SIID_FOLDERBACK = 75,
                SIID_FOLDERFRONT = 76,
                SIID_SHIELD = 77,
                SIID_WARNING = 78,
                SIID_INFO = 79,
                SIID_ERROR = 80,
                SIID_KEY = 81,
                SIID_SOFTWARE = 82,
                SIID_RENAME = 83,
                SIID_DELETE = 84,
                SIID_MEDIAAUDIODVD = 85,
                SIID_MEDIAMOVIEDVD = 86,
                SIID_MEDIAENHANCEDCD = 87,
                SIID_MEDIAENHANCEDDVD = 88,
                SIID_MEDIAHDDVD = 89,
                SIID_MEDIABLURAY = 90,
                SIID_MEDIAVCD = 91,
                SIID_MEDIADVDPLUSR = 92,
                SIID_MEDIADVDPLUSRW = 93,
                SIID_DESKTOPPC = 94,
                SIID_MOBILEPC = 95,
                SIID_USERS = 96,
                SIID_MEDIASMARTMEDIA = 97,
                SIID_MEDIACOMPACTFLASH = 98,
                SIID_DEVICECELLPHONE = 99,
                SIID_DEVICECAMERA = 100,
                SIID_DEVICEVIDEOCAMERA = 101,
                SIID_DEVICEAUDIOPLAYER = 102,
                SIID_NETWORKCONNECT = 103,
                SIID_INTERNET = 104,
                SIID_ZIPFILE = 105,
                SIID_SETTINGS = 106,
                SIID_DRIVEHDDVD = 132,
                SIID_DRIVEBD = 133,
                SIID_MEDIAHDDVDROM = 134,
                SIID_MEDIAHDDVDR = 135,
                SIID_MEDIAHDDVDRAM = 136,
                SIID_MEDIABDROM = 137,
                SIID_MEDIABDR = 138,
                SIID_MEDIABDRE = 139,
                SIID_CLUSTEREDDRIVE = 140,
                SIID_MAX_ICONS = 175
            }
        }
    }
}
