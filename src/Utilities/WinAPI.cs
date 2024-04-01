using System.Runtime.InteropServices;

namespace KeePassWinHello
{
    internal static class WinAPI
    {
        private const string User32 = "User32.dll";
        private const string Kernel32 = "Kernel32.dll";

        [DllImport(Kernel32, SetLastError = true)]
        public static extern int GetCurrentThreadId();

        [DllImport(User32, SetLastError = true)]
        public static extern HDESK GetThreadDesktop(int dwThreadId);

        [DllImport(User32, SetLastError = true)]
        public static extern BOOL SetThreadDesktop(HDESK hDesktop);
    }
}
