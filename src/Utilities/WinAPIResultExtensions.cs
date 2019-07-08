using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace KeePassWinHello.Utilities
{
    static class WinAPIResultExtensions
    {
        public static IntPtr CheckAndPass(this IntPtr resultHandle)
        {
            if (resultHandle == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());
            return resultHandle;
        }
    }
}
