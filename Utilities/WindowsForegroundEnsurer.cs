using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace KeePassWinHello.Utilities
{
    class WindowsForegroundEnsurer
    {
        public static bool EnsureForeground(IntPtr currentWindowHandle, string targetWindowClass, string targetWindowTitle, int timeoutMs)
        {
            if (timeoutMs < 1)
                throw new ArgumentOutOfRangeException("attemptsCount");

            const int waitTimeMs = 25;
            var attemptsCount = timeoutMs / waitTimeMs;
            if (attemptsCount < 1)
                attemptsCount = 1;

            IntPtr targetWindowHandle = IntPtr.Zero;
            for (int i = 0; i < attemptsCount && targetWindowHandle == IntPtr.Zero; i++)
            {
                targetWindowHandle = WinAPI.FindWindowEx(currentWindowHandle, IntPtr.Zero, targetWindowClass, targetWindowTitle);
                if (targetWindowHandle == IntPtr.Zero)
                {
                    ThrowIfErrorPersists();
                    Thread.Sleep(waitTimeMs);
                }
            }
            targetWindowHandle.CheckAndPass();
            return EnsureForeground(currentWindowHandle, targetWindowHandle);
        }

        public static bool EnsureForeground(IntPtr currentWindowHandle, string targetWindowClass, string targetWindowTitle)
        {
            var targetWindowHandle = WinAPI.FindWindowEx(currentWindowHandle, IntPtr.Zero, targetWindowClass, targetWindowTitle)
                    .CheckAndPass();
            return EnsureForeground(currentWindowHandle, targetWindowHandle);
        }

        public static bool EnsureForeground(IntPtr currentWindowHandle, IntPtr targetWindowHandle)
        {
            if (currentWindowHandle != IntPtr.Zero)
                SilentlyBecomeForeground(currentWindowHandle);
            return BringToFrontAndActivate(targetWindowHandle);
        }

        private static void ThrowIfErrorPersists()
        {
            const int ERROR_ACCESS_DENIED = 0x5;

            int errorNum = Marshal.GetLastWin32Error();
            if (errorNum == ERROR_ACCESS_DENIED)
                throw new Win32Exception(errorNum);
        }

        private static void SilentlyBecomeForeground(IntPtr winHandle)
        {
            WinAPI.SetForegroundWindow(winHandle);
            MinimizeWindow(winHandle);
        }

        private static bool BringToFrontAndActivate(IntPtr winHandle)
        {
            ResotoreAndFocus(winHandle);
            return WinAPI.SetForegroundWindow(winHandle);
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
        }
    }
}
