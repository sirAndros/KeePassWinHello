using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace KeePassWinHello
{
    class WinMinimizer : IDisposable
    {
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", EntryPoint = "SendMessage", SetLastError = true)]
        static extern IntPtr SendMessage(IntPtr hWnd, Int32 Msg, IntPtr wParam, IntPtr lParam);

        const int WM_COMMAND = 0x111;
        const int MIN_ALL = 419;
        const int MIN_ALL_UNDO = 416;




        private readonly SafeHandle _handle;

        public WinMinimizer()
        {
            IntPtr lHwnd = FindWindow("Shell_TrayWnd", null);
            _handle = new Microsoft.Win32.SafeHandles.SafeFileHandle(lHwnd, true);
            SendMessage(lHwnd, WM_COMMAND, (IntPtr)MIN_ALL, IntPtr.Zero);
        }

        public void Dispose()
        {
            SendMessage(_handle.DangerousGetHandle(), WM_COMMAND, (IntPtr)MIN_ALL_UNDO, IntPtr.Zero);
            _handle?.Dispose();
        }
    }
}
