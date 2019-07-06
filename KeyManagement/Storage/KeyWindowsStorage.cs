using KeePassLib.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Xml.Serialization;

namespace KeePassWinHello
{
    class KeyWindowsStorage : IKeyStorage
    {
        [DllImport("advapi32", SetLastError = true)/*, SuppressUnmanagedCodeSecurityAttribute*/]
        static extern int OpenProcessToken(
System.IntPtr ProcessHandle, // handle to process
int DesiredAccess, // desired access to process
ref IntPtr TokenHandle // handle to open access token
);

        [DllImport("kernel32", SetLastError = true)/*, SuppressUnmanagedCodeSecurityAttribute*/]
        static extern bool CloseHandle(IntPtr handle);
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public extern static bool DuplicateToken(IntPtr ExistingTokenHandle,
        int SECURITY_IMPERSONATION_LEVEL, ref IntPtr DuplicateTokenHandle);

        public const int TOKEN_DUPLICATE = 2;
        public const int TOKEN_QUERY = 0X00000008;
        public const int TOKEN_IMPERSONATE = 0X00000004;

        internal const int SecurityImpersonation = 2;

        #region Credential Manager
        internal const int ERROR_NOT_FOUND = 0x490;

        internal const int CRED_TYPE_GENERIC = 0x1;

        internal const int CRED_PERSIST_LOCAL_MACHINE = 0x2;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct CREDENTIAL
        {
            public UInt32 Flags;
            public UInt32 Type;
            public IntPtr TargetName;
            public IntPtr Comment;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
            public UInt32 CredentialBlobSize;
            public IntPtr CredentialBlob;
            public UInt32 Persist;
            public UInt32 AttributeCount;
            public IntPtr Attributes;
            public IntPtr TargetAlias;
            public IntPtr UserName;

        }

        [DllImport("advapi32.dll", EntryPoint = "CredDeleteW", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool CredDelete(string target, uint type, int reservedFlag);

        [DllImport("advapi32.dll", EntryPoint = "CredEnumerateW", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool CredEnumerate(string target, uint flags, out uint count, out IntPtr credentialsPtr);

        [DllImport("advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool CredRead(string target, uint type, int reservedFlag, out IntPtr CredentialPtr);

        [DllImport("advapi32.dll", EntryPoint = "CredWriteW", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool CredWrite([In] ref CREDENTIAL userCredential, uint flags);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool CredFree([In] IntPtr cred);

        #endregion

        IntPtr GetProcessToken(IntPtr processHandle)
        {
            IntPtr hToken = IntPtr.Zero;
            IntPtr dupeTokenHandle = IntPtr.Zero;
            // For simplicity I'm using the PID of System here
            //Process proc = Process.GetProcessById(4);
            if (OpenProcessToken(processHandle, TOKEN_QUERY | TOKEN_IMPERSONATE | TOKEN_DUPLICATE, ref hToken) != 0)
            {
#if DEBUG
                WindowsIdentity newId = new WindowsIdentity(hToken);
                Debug.WriteLine(newId.Owner);
#endif
                try
                {
                    DuplicateToken(hToken, SecurityImpersonation, ref dupeTokenHandle);
                    if (IntPtr.Zero == dupeTokenHandle)
                    {
                        string s = String.Format("Dup failed {0}, privilege not held",
                        Marshal.GetLastWin32Error());
                        throw new Exception(s);
                    }
#if DEBUG
                    WindowsImpersonationContext impersonatedUser =
                    newId.Impersonate();
                    IntPtr accountToken = WindowsIdentity.GetCurrent().Token;
                    Debug.WriteLine("Token number is: " + accountToken.ToString());
                    Debug.WriteLine("Windows ID Name is: " +
                    WindowsIdentity.GetCurrent().Name);
                    impersonatedUser.Undo();
#endif
                }
                finally
                {
                    CloseHandle(hToken);
                }
            }
            else
            {
                string s = String.Format("OpenProcess Failed {0}, privilege not held", Marshal.GetLastWin32Error());
                throw new Exception(s);
            }

            return dupeTokenHandle;
        }

        private const int _maxBlobSize = 512 * 5;
        private const string _credPrefix = "KeePassWinHello_";
        private readonly IntPtr _systemToken;

        public KeyWindowsStorage()
        {
            if (!UAC.IsCurrentProcessElevated)
                throw new Exception("Process is not elevated");

            //Privileges.EnablePrivilege(SecurityEntity.SE_DEBUG_NAME);
            Process.EnterDebugMode();

            try
            {
                var winlogons = Process.GetProcessesByName("lsass"); //"winlogon.exe" ?
                if (winlogons.Length == 0)
                    throw new Exception("winlogon not found");

                _systemToken = GetProcessToken(winlogons[0].Handle);
            }
            finally
            {
                Process.LeaveDebugMode();
            }

            // TODO: check if token has system rights:
            //if (GetTokenInformation(hToken, TokenUser, data, sizeof(data), &dwNeed))
            //{
            //    system = IsWellKnownSid(pTokenUser->User.Sid, WinLocalSystemSid);
            //}
        }

        private string GetTarget(string path)
        {
            return _credPrefix + path;
        }

        public void AddOrUpdate(string dbPath, ProtectedKey protectedKey)
        {
            var stream = new MemoryStream();
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, protectedKey);

            byte[] data = stream.ToArray();
            try
            {
                if (data.Length > _maxBlobSize)
                    throw new ArgumentOutOfRangeException("protectedKey", "protectedKey blob has exceeded 2560 bytes");

                var ncred = new CREDENTIAL();
                try
                {
                    ncred.Type = CRED_TYPE_GENERIC;
                    ncred.Persist = CRED_PERSIST_LOCAL_MACHINE;
                    ncred.UserName = Marshal.StringToCoTaskMemUni("dummy");
                    ncred.TargetName = Marshal.StringToCoTaskMemUni(GetTarget(dbPath));
                    ncred.CredentialBlob = Marshal.AllocCoTaskMem(data.Length);
                    Marshal.Copy(data, 0, ncred.CredentialBlob, data.Length);
                    ncred.CredentialBlobSize = (uint)data.Length;

                    using (var context = WindowsIdentity.Impersonate(_systemToken))
                    {
                        if (!CredWrite(ref ncred, 0))
                            throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                }
                finally
                {
                    Marshal.FreeCoTaskMem(ncred.UserName);
                    Marshal.FreeCoTaskMem(ncred.TargetName);
                    Marshal.FreeCoTaskMem(ncred.CredentialBlob); // TODO: Zero data
                }
            }
            finally
            {
                MemUtil.ZeroByteArray(data);
            }
        }

        public bool ContainsKey(string dbPath)
        {
            IntPtr ncredPtr = IntPtr.Zero;
            try
            {
                using (var context = WindowsIdentity.Impersonate(_systemToken))
                    return CredRead(GetTarget(dbPath), CRED_TYPE_GENERIC, 0, out ncredPtr);
            }
            finally
            {
                if (ncredPtr != IntPtr.Zero)
                    CredFree(ncredPtr);
            }
        }

        public void Purge()
        {
            IntPtr ncredsPtr = IntPtr.Zero;
            uint count = 0;

            using (var context = WindowsIdentity.Impersonate(_systemToken))
            {
                if (!CredEnumerate(GetTarget("*"), 0, out count, out ncredsPtr))
                {
                    var lastError = Marshal.GetLastWin32Error();
                    if (lastError == ERROR_NOT_FOUND)
                        return;

                    throw new Win32Exception(lastError);
                }
            }

            var credsToRemove = new List<string>();
            try
            {
                for (int i = 0; i != count; ++i)
                {
                    IntPtr ncredPtr = Marshal.ReadIntPtr(ncredsPtr, i * IntPtr.Size);
                    var ncred = (CREDENTIAL)Marshal.PtrToStructure(ncredPtr, typeof(CREDENTIAL));
                    bool isValid = true;
                    try
                    {
                        var createdDate = DateTime.FromFileTime((long)(((ulong)ncred.LastWritten.dwHighDateTime << 32) | (ulong)ncred.LastWritten.dwLowDateTime));
                        if (DateTime.UtcNow - createdDate >= Settings.Instance.InvalidatingTime)
                            isValid = false;
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        isValid = false;
                    }

                    if (!isValid)
                        credsToRemove.Add(Marshal.PtrToStringUni(ncred.TargetName));
                }
            }
            finally
            {
                CredFree(ncredsPtr);
            }

            if (credsToRemove.Count == 0)
                return;

            using (var context = WindowsIdentity.Impersonate(_systemToken))
            {
                foreach (var target in credsToRemove)
                    CredDelete(target, CRED_TYPE_GENERIC, 0);
            }
        }

        public void Remove(string dbPath)
        {
            using (var context = WindowsIdentity.Impersonate(_systemToken))
            {
                if (!CredDelete(GetTarget(dbPath), CRED_TYPE_GENERIC, 0))
                    throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        public bool TryGetValue(string dbPath, out ProtectedKey protectedKey)
        {
            protectedKey = null;
            IntPtr ncredPtr = IntPtr.Zero;

            using (var context = WindowsIdentity.Impersonate(_systemToken))
            {
                if (!CredRead(GetTarget(dbPath), CRED_TYPE_GENERIC, 0, out ncredPtr))
                {
                    Debug.Assert(Marshal.GetLastWin32Error() == ERROR_NOT_FOUND);
                    return false;
                }
            }

            byte[] data = null;
            try
            {
                var ncred = (CREDENTIAL)Marshal.PtrToStructure(ncredPtr, typeof(CREDENTIAL));
                data = new byte[ncred.CredentialBlobSize];
                Marshal.Copy(ncred.CredentialBlob, data, 0, data.Length);

                var stream = new MemoryStream();
                stream.Write(data, 0, data.Length);
                stream.Position = 0;

                var formatter = new BinaryFormatter();
                formatter.Binder = new ProtectedKey();
                protectedKey = (ProtectedKey)formatter.Deserialize(stream);
            }
            catch
            {
                return false;
            }
            finally
            {
                CredFree(ncredPtr);
                MemUtil.ZeroByteArray(data);
            }
            return true;
        }
    }
}
