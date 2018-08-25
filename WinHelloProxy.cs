using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace KeePassWinHello
{
    public enum AuthStatus
    {
        whUnknown = -1,
        whSuccess = 0,
        whDisabled = 1,
        whCanceled = 2,
        whNotFound = 3
    }

    class WinHelloProxy
    {
        private const string DllFilePath = @"Libs\WinHelloAuth.dll";

        public delegate void AuthResultProc(AuthStatus result);

        [DllImport(DllFilePath)]
        public static extern bool IsHelloAuthAvailable();

        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        public static extern AuthStatus RequestHelloAuth(string i_Message);

        [DllImport(DllFilePath, CharSet = CharSet.Unicode)]
        public static extern bool RequestHelloAuthAsync(string i_Message, AuthResultProc i_ResultCb);
    }
}
