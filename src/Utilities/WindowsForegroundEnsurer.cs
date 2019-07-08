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
        public static bool AllowAllSetForeground()
        {
            const int ASFW_ANY = -1;
            return WinAPI.AllowSetForegroundWindow(ASFW_ANY);
        }

        public static bool IsWindowOnForeground(IntPtr windowHandle)
        {
            return windowHandle == WinAPI.GetForegroundWindow();
        }

        public static bool EnsureForeground(IntPtr windowHandle)
        {
            return EnsureForeground(IntPtr.Zero, windowHandle);
        }

        public static bool EnsureForeground(IntPtr currentWindowHandle, string targetWindowClass, string targetWindowTitle, int timeoutMs)
        {
            if (timeoutMs < 1)
                throw new ArgumentOutOfRangeException("timeoutMs");

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
                    //ThrowIfErrorPersists();
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
            if (IsWindowOnForeground(targetWindowHandle))
                return true;

            /*
              To set foreground we need at least one of this:
              * The process is the foreground process.
              * The process received the last input event.
              * There is no foreground process.
              * The foreground process is not a Modern Application or the Start Screen.
              * The foreground is not locked (see LockSetForegroundWindow).
              * The foreground lock time-out has expired (see SPI_GETFOREGROUNDLOCKTIMEOUT in SystemParametersInfo).
              * No menus are active.
              
              https://docs.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-setforegroundwindow
             */

            if (currentWindowHandle != IntPtr.Zero)
                SilentlyBecomeForeground(currentWindowHandle);
            UnlockForeground();
            return BringToFrontAndActivate(targetWindowHandle);
        }

        private static void ThrowIfErrorPersists()
        {
            const int ERROR_ACCESS_DENIED = 0x5;

            int errorNum = Marshal.GetLastWin32Error();
            if (errorNum == ERROR_ACCESS_DENIED)
                throw new Win32Exception(errorNum);
        }

        private static void UnlockForeground()
        {
            const byte ALT = 0xA4;
            const int EXTENDEDKEY = 0x1;
            const int KEYUP = 0x2;

            // Send ALT key to counteract LockSetForegroundWindow (https://docs.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-locksetforegroundwindow)
            WinAPI.keybd_event(ALT, 0x45, EXTENDEDKEY, UIntPtr.Zero);           // Simulate a key press
            WinAPI.keybd_event(ALT, 0x45, EXTENDEDKEY | KEYUP, UIntPtr.Zero);   // Simulate a key release
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

            [DllImport(User32, SetLastError = true)]
            public static extern IntPtr GetForegroundWindow();

            [DllImport(User32, SetLastError = true)]
            public static extern bool AllowSetForegroundWindow(Int32 pid);

            [DllImport(User32, SetLastError = true)]
            public static extern void keybd_event(Byte bVk, Byte bScan, UInt32 dwFlags, UIntPtr dwExtraInfo);

            [DllImport(User32, SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

            [DllImport(User32, SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lpClassName, string lpWindowName);
        }
    }
}
