using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace KeePassWinHello
{
    class Win32Window : IWin32Window
    {
        public IntPtr Handle { get; private set; }

        private Win32Window(IntPtr handle)
        {
            Handle = handle;
        }

        #region Creation

        /// <summary>
        /// Returns null if not found
        /// </summary>
        public static Win32Window From(IntPtr handle)
        {
            return GetOrNull(new HWND(handle));
        }

        /// <summary>
        /// Throws exception if not found
        /// </summary>
        public static Win32Window Get(string @class, string name)
        {
            return Get(IntPtr.Zero, IntPtr.Zero, @class, name);
        }
        public static Win32Window Get(IntPtr parentHandle, IntPtr childAfter, string @class, string name)
        {
            var hwnd = WinAPI.FindWindowEx(parentHandle, childAfter, @class, name);
            return GetOrError(hwnd, "FindWindowEx");
        }

        public static Win32Window Get(string @class, string name, int timeoutMs)
        {
            var hwnd = FindWithRetry(IntPtr.Zero, IntPtr.Zero, @class, name, timeoutMs);
            return GetOrError(hwnd, "FindWindowEx");
        }

        /// <summary>
        /// Returns null if not found
        /// </summary>
        public static Win32Window Find(string @class, string name)
        {
            return Find(IntPtr.Zero, IntPtr.Zero, @class, name);
        }
        public static Win32Window Find(IntPtr parentHandle, IntPtr childAfter, string @class, string name)
        {
            var hwnd = WinAPI.FindWindowEx(parentHandle, childAfter, @class, name);
            return GetOrNull(hwnd);
        }
        public static Win32Window Find(string @class, string name, int timeoutMs)
        {
            var hwnd = FindWithRetry(IntPtr.Zero, IntPtr.Zero, @class, name, timeoutMs);
            return GetOrNull(hwnd);
        }

        private static HWND FindWithRetry(IntPtr parentHandle, IntPtr childAfter, string targetWindowClass, string targetWindowTitle, int timeoutMs)
        {
            if (timeoutMs < 1)
                throw new ArgumentOutOfRangeException("timeoutMs");

            const int waitTimeMs = 25;
            var attemptsCount = timeoutMs / waitTimeMs;
            if (attemptsCount < 1)
                attemptsCount = 1;

            HWND targetWindowHandle = new HWND();
            for (int i = 0; i < attemptsCount && targetWindowHandle.Value == IntPtr.Zero; i++)
            {
                targetWindowHandle = WinAPI.FindWindowEx(parentHandle, childAfter, targetWindowClass, targetWindowTitle);
                if (targetWindowHandle.Value == IntPtr.Zero)
                {
                    Thread.Sleep(waitTimeMs);
                }
            }

            return targetWindowHandle;
        }

        private static Win32Window GetOrError(HWND hwnd, string funcName)
        {
            hwnd.ThrowOnError(funcName);
            return new Win32Window(hwnd.Value);
        }

        private static Win32Window GetOrNull(HWND hwnd)
        {
            if (hwnd.IsValid)
                return new Win32Window(hwnd.Value);
            return null;
        }

        #endregion Creation


        public static void AllowAllSetForeground()
        {
            const int ASFW_ANY = -1;
            WinAPI.AllowSetForegroundWindow(ASFW_ANY)
                .ThrowOnError("AllowSetForegroundWindow", WinAPI.ERROR_ACCESS_DENIED);
        }

        public void Close()
        {
            const int WM_CLOSE = 0x0010;
            WinAPI.SendMessage(Handle, WM_CLOSE, 0, 0);
        }

        public bool IsWindowOnForeground()
        {
            return Handle == WinAPI.GetForegroundWindow().Value;
        }

        public void EnsureForeground()
        {
            EnsureForeground(IntPtr.Zero);
        }

        public void EnsureForeground(IntPtr currentProcessWindowHandle)
        {
            if (IsWindowOnForeground())
                return;

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

            if (currentProcessWindowHandle != IntPtr.Zero)
                SilentlyBecomeForeground(currentProcessWindowHandle);
            UnlockForeground();
            BringToFrontAndActivate();
        }

        public void SilentlyBecomeForeground()
        {
            SilentlyBecomeForeground(Handle);
        }

        public void BringToFrontAndActivate()
        {
            BringToFrontAndActivate(Handle);
        }

        public void ResotoreAndFocus()
        {
            ResotoreAndFocus(Handle);
        }

        public void MinimizeWindow()
        {
            MinimizeWindow(Handle);
        }

        private static void SilentlyBecomeForeground(IntPtr winHandle)
        {
            WinAPI.SetForegroundWindow(winHandle);
            MinimizeWindow(winHandle);
        }

        private static void BringToFrontAndActivate(IntPtr winHandle)
        {
            ResotoreAndFocus(winHandle);
            WinAPI.SetForegroundWindow(winHandle)
                .ThrowOnError("SetForegroundWindow");
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
            WinAPI.ShowWindow(winHandle, SW_SHOWMINIMIZED).ThrowOnError("ShowWindow");
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


        private static class WinAPI
        {
            private const string User32 = "User32.dll";

            public const int ERROR_ACCESS_DENIED = 5;

            [DllImport(User32, SetLastError = true)]
            public static extern BOOL SetForegroundWindow(IntPtr hWnd);

            [DllImport(User32, SetLastError = true)]
            public static extern int SendMessage(IntPtr hWnd, Int32 msg, Int32 wParam, Int32 lParam);

            [DllImport(User32, SetLastError = true)]
            public static extern BOOL ShowWindow(IntPtr hWnd, int nCmdShow);

            [DllImport(User32, SetLastError = true)]
            public static extern HWND GetForegroundWindow();

            [DllImport(User32, SetLastError = true)]
            public static extern BOOL AllowSetForegroundWindow(Int32 pid);

            [DllImport(User32, SetLastError = true)]
            public static extern void keybd_event(Byte bVk, Byte bScan, UInt32 dwFlags, UIntPtr dwExtraInfo);

            [DllImport(User32, SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern HWND FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lpClassName, string lpWindowName);
        }
    }
}
