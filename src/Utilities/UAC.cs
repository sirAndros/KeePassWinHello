using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using Microsoft.Win32;

namespace KeePassWinHello
{
    static class UAC
    {
        private const string uacRegistryKey = "Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System";
        private const string uacRegistryValue = "EnableLUA";

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool OpenProcessToken(IntPtr processHandle, uint desiredAccess, out IntPtr TokenHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool GetTokenInformation(IntPtr tokenHandle, TOKEN_INFORMATION_CLASS tokenInformationClass, IntPtr tokenInformation, uint tokenInformationLength, out uint returnLength);

        enum TOKEN_INFORMATION_CLASS
        {
            TokenUser = 1,
            TokenGroups,
            TokenPrivileges,
            TokenOwner,
            TokenPrimaryGroup,
            TokenDefaultDacl,
            TokenSource,
            TokenType,
            TokenImpersonationLevel,
            TokenStatistics,
            TokenRestrictedSids,
            TokenSessionId,
            TokenGroupsAndPrivileges,
            TokenSessionReference,
            TokenSandBoxInert,
            TokenAuditPolicy,
            TokenOrigin,
            TokenElevationType,
            TokenLinkedToken,
            TokenElevation,
            TokenHasRestrictions,
            TokenAccessInformation,
            TokenVirtualizationAllowed,
            TokenVirtualizationEnabled,
            TokenIntegrityLevel,
            TokenUIAccess,
            TokenMandatoryPolicy,
            TokenLogonSid,
            MaxTokenInfoClass
        }

        enum TokenElevationType
        {
            TokenElevationTypeDefault = 1,
            TokenElevationTypeFull,
            TokenElevationTypeLimited
        }

        public static bool Enabled
        {
            get
            {
                using (RegistryKey uacKey = Registry.LocalMachine.OpenSubKey(uacRegistryKey, false))
                {
                    return uacKey.GetValue(uacRegistryValue).Equals(1);
                }
            }
        }

        public static bool IsCurrentProcessElevated
        {
            get
            {
                return _isCurrentProcessElevated.Value;
            }
        }
        private static readonly Lazy<bool> _isCurrentProcessElevated = new Lazy<bool>(CheckIsCurrentProcessElevated);

        private static bool CheckIsCurrentProcessElevated()
        {
            try
            {
                if (Enabled)
                {
                    IntPtr tokenHandle = OpenCurrentProcessTokenForRead();
                    try
                    {
                        var elevationType = GetElevationType(tokenHandle);
                        return elevationType == TokenElevationType.TokenElevationTypeFull;
                    }
                    finally
                    {
                        if (tokenHandle != IntPtr.Zero)
                            CloseHandle(tokenHandle);
                    }
                }
                else
                {
                    return IsCurrentPrincipalAdmin();
                }
            }
            catch (Exception ex)
            {
                if (IsAccessDeniedException(ex))
                    return false;
                throw;
            }
        }

        private static bool IsAccessDeniedException(Exception ex)
        {
            if (ex is SecurityException || ex is UnauthorizedAccessException)
                return true;

            const int ACCESS_DENIED = 0x5;
            var winEx = ex as Win32Exception;
            return winEx != null
                && winEx.NativeErrorCode == ACCESS_DENIED;
        }

        private static TokenElevationType GetElevationType(IntPtr tokenHandle)
        {
            int elevationResultSize = Marshal.SizeOf(Enum.GetUnderlyingType(typeof(TokenElevationType)));
            IntPtr elevationTypePtr = Marshal.AllocHGlobal(elevationResultSize);
            try
            {
                uint returnedSize = 0;
                bool success = GetTokenInformation(tokenHandle, TOKEN_INFORMATION_CLASS.TokenElevationType,
                                                           elevationTypePtr, (uint) elevationResultSize,
                                                           out returnedSize);
                if (!success)
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "Unable to determine the current elevation status.");

                return (TokenElevationType)Marshal.ReadInt32(elevationTypePtr);
            }
            finally
            {
                if (elevationTypePtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(elevationTypePtr);
            }
        }

        private static IntPtr OpenCurrentProcessTokenForRead()
        {
            IntPtr tokenHandle;
            using (var process = Process.GetCurrentProcess())
            {
                const uint STANDARD_RIGHTS_READ = 0x00020000;
                const uint TOKEN_QUERY = 0x0008;
                const uint TOKEN_READ = (STANDARD_RIGHTS_READ | TOKEN_QUERY);

                if (!OpenProcessToken(process.Handle, TOKEN_READ, out tokenHandle))
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not get process token.");
            }
            return tokenHandle;
        }

        private static bool IsCurrentPrincipalAdmin()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                const int DomainAdministratorRole = 0x200;
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator)
                    || principal.IsInRole(DomainAdministratorRole);
            }
        }
    }
}
