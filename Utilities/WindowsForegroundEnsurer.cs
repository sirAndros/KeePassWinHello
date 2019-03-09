using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace KeePassWinHello.Utilities
{
    class WindowsForegroundEnsurer
    {
        public static void EnsureForeground(IntPtr currentWindowHandle, string targetWindowClass, string targetWindowTitle)
        {
            var targetWindowHandle = WinAPI.FindWindowEx(currentWindowHandle, IntPtr.Zero, targetWindowClass, targetWindowTitle)
                    .CheckAndPass();
            EnsureForeground(currentWindowHandle, targetWindowHandle);
        }

        public static void EnsureForeground(IntPtr currentWindowHandle, IntPtr targetWindowHandle)
        {
            SilentlyBecomeForeground(currentWindowHandle);
            BringToFrontAndActivate(targetWindowHandle);
        }

        private static void SilentlyBecomeForeground(IntPtr winHandle)
        {
            WinAPI.SetForegroundWindow(winHandle);
            MinimizeWindow(winHandle);
        }

        private static void BringToFrontAndActivate(IntPtr winHandle)
        {
            WinAPI.SetForegroundWindow(winHandle);
            ResotoreAndFocus(winHandle);
        }

        private static void ResotoreAndFocus(IntPtr winHandle)
        {
            const int WM_SYSCOMMAND = 0x01;
            const int SC_RESTORE = 0xF120;
            WinAPI.SendMessage(winHandle, WM_SYSCOMMAND, SC_RESTORE, 0);
        }

        private static void MinimizeWindow(IntPtr winHandle)
        {
            const int SW_SHOWMINIMIZED = 0x02;
            WinAPI.ShowWindow(winHandle, SW_SHOWMINIMIZED);
        }


        static class WinAPI
        {
            private const string User32 = "User32.dll";

            [DllImport(User32, SetLastError = true)]
            public static extern bool SetForegroundWindow(IntPtr hWnd);

            [DllImport(User32, SetLastError = true)]
            public static extern int SendMessage(IntPtr hWnd, Int32 msg, Int32 wParam, Int32 lParam);

            [DllImport(User32, SetLastError = true)]
            public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);


            [DllImport(User32, SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

            [DllImport(User32, SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lpClassName, string lpWindowName);


            [DllImport(User32, SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
        }
    }
}
