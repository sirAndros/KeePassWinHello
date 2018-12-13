using System;
using KeePass.App.Configuration;

namespace WinHelloQuickUnlock
{
    class Settings
    {
        private Settings()
        {
        }

        private const string CFG_VALID_PERIOD = "WindowsHello.QuickUnlock.ValidPeriod";
        private const string CFG_ENABLED = "WindowsHello.QuickUnlock.Enabled";
        private const long VALID_PERIOD_DEFAULT = 1000 * 60 * 60 * 24; // one day in ms

        private static Lazy<Settings> _instance = new Lazy<Settings>(() => new Settings(), true);
        public static Settings Instance = _instance.Value;

        private AceCustomConfig _customConfig;

        private void UpgradeConfig()
        {
            const string deprecatedCfgAutoPrompt = "WindowsHello_AutoPrompt";
            const string deprecatedCfgValidPeriod = "WindowsHello_ValidPeriod";

            _customConfig.SetString(deprecatedCfgAutoPrompt, null);

            long validPeriod = _customConfig.GetLong(deprecatedCfgValidPeriod, -1);
            if (validPeriod != -1)
            {
                _customConfig.SetString(deprecatedCfgValidPeriod, null);
                if (validPeriod == 0)
                    validPeriod = -1;
                else
                    validPeriod *= 1000;

                _customConfig.SetLong(CFG_VALID_PERIOD, validPeriod);
            }
        }

        public void Initialize(AceCustomConfig customConfig)
        {
            if (customConfig == null)
                throw new ArgumentNullException("customConfig");
            if (_customConfig != null)
                throw new InvalidOperationException("Settings have initialized already");

            _customConfig = customConfig;

            UpgradeConfig();
        }

        public TimeSpan InvalidatingTime
        {
            get
            {
                var ms = _customConfig.GetLong(CFG_VALID_PERIOD, VALID_PERIOD_DEFAULT);
                if (ms < 0)
                    return TimeSpan.MaxValue;
                return TimeSpan.FromMilliseconds(ms);
            }
            set
            {
                _customConfig.SetLong(CFG_VALID_PERIOD, (long)value.TotalMilliseconds);
            }
        }

        public bool Enabled
        {
            get
            {
                return _customConfig.GetBool(CFG_ENABLED, true);
            }
            set
            {
                _customConfig.SetBool(CFG_ENABLED, value);
            }
        }

        public const string ConfirmationMessage = "Authentication to access KeePass database";
        public const string OptionsTabName = "WindowsHello";
    }
}
