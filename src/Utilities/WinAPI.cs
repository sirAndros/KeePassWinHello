using System;
using System.Runtime.InteropServices;

namespace KeePassWinHello
{
    internal static class WinAPI
    {
        private const string User32 = "User32.dll";
        private const string Kernel32 = "Kernel32.dll";
        private const string ntdll = "ntdll.dll";

        [DllImport(Kernel32, SetLastError = true)]
        public static extern int GetCurrentThreadId();

        [DllImport(User32, SetLastError = true)]
        public static extern HDESK GetThreadDesktop(int dwThreadId);

        [DllImport(User32, SetLastError = true)]
        public static extern BOOL SetThreadDesktop(HDESK hDesktop);

        [DllImport(ntdll, SetLastError = true)]
        private static extern void RtlGetNtVersionNumbers(out uint lpMajorVersion, out uint lpMinorVersion, out uint lpBuildNumber);
        public static Version GetOsVersion()
        {
            uint major, minor, build;
            RtlGetNtVersionNumbers(out major, out minor, out build);
            if (build > int.MaxValue)
                build = 0;

            return new Version((int)major, (int)minor, (int)build);
        }
    }
}
