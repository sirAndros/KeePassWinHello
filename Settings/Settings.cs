using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WinHelloQuickUnlock
{
    class Settings
    {
        private Settings()
        {
        }

        private static Lazy<Settings> _instance = new Lazy<Settings>(() => new Settings(), true);
        public static Settings Instance = _instance.Value;

        public void Initialize()
        {
            //todo
            InvalidatingTime = TimeSpan.FromMinutes(10);
        }


        public TimeSpan InvalidatingTime { get; set; }


        public const string ConfirmationMessage = "[TBD] Authentication to access KeePass database";
    }
}
