using KeePassLib.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace KeePassWinHello
{
    internal class KeyWindowsStorage : IKeyStorage
    {
        #region Credential Manager API
        private const int ERROR_NOT_FOUND = 0x490;

        private const int CRED_TYPE_GENERIC = 0x1;

        private const int CRED_PERSIST_LOCAL_MACHINE = 0x2;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct CREDENTIAL
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
        private static extern BOOL CredDelete(string target, uint type, int reservedFlag);

        [DllImport("advapi32.dll", EntryPoint = "CredEnumerateW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern BOOL CredEnumerate(string target, uint flags, out uint count, out IntPtr credentialsPtr);

        [DllImport("advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern BOOL CredRead(string target, uint type, int reservedFlag, out IntPtr CredentialPtr);

        [DllImport("advapi32.dll", EntryPoint = "CredWriteW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern BOOL CredWrite([In] ref CREDENTIAL userCredential, uint flags);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern BOOL CredFree([In] IntPtr cred);
        #endregion

        private const int _maxBlobSize = 512 * 5;
        private const string _credPrefix = "KeePassWinHello_";

        public int Count
        {
            get
            {
                int count = 0;
                ForEach(ncred => count += IsExpired(ncred) ? 0 : 1);
                return count;
            }
        }

        public void Clear()
        {
            var credsToRemove = new List<string>();
            ForEach(ncred => credsToRemove.Add(Marshal.PtrToStringUni(ncred.TargetName)));

            foreach (var target in credsToRemove)
                CredDelete(target, CRED_TYPE_GENERIC, 0);
        }

        //public KeyWindowsStorage() { }

        private string GetTarget(string path)
        {
            return _credPrefix + path;
        }

        public void AddOrUpdate(string dbPath, ProtectedKey protectedKey)
        {
            byte[] data = ProtectedKey.Serialize(protectedKey);
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

                    CredWrite(ref ncred, 0).ThrowOnError("CredWrite");
                }
                finally
                {
                    Marshal.FreeCoTaskMem(ncred.UserName);
                    Marshal.FreeCoTaskMem(ncred.TargetName);
                    Marshal.FreeCoTaskMem(ncred.CredentialBlob);
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
                bool hasKey = CredRead(GetTarget(dbPath), CRED_TYPE_GENERIC, 0, out ncredPtr).Result;
                return hasKey && !IsExpired((CREDENTIAL)Marshal.PtrToStructure(ncredPtr, typeof(CREDENTIAL)));
            }
            finally
            {
                if (ncredPtr != IntPtr.Zero)
                    CredFree(ncredPtr);
            }
        }

        public void Purge()
        {
            var credsToRemove = new List<string>();

            ForEach(ncred => {
                if (IsExpired(ncred))
                    credsToRemove.Add(Marshal.PtrToStringUni(ncred.TargetName));
            });

            foreach (var target in credsToRemove)
                CredDelete(target, CRED_TYPE_GENERIC, 0);
        }

        private bool IsExpired(CREDENTIAL ncred)
        {
            try
            {
                long highDateTime = (long)((uint)ncred.LastWritten.dwHighDateTime) << 32;
                long lowDateTime = (uint)ncred.LastWritten.dwLowDateTime;

                var createdDate = DateTime.FromFileTime(highDateTime | lowDateTime);
                if (DateTime.Now - createdDate >= Settings.Instance.InvalidatingTime)
                    return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                return true;
            }

            return false;
        }

        private void ForEach(Action<CREDENTIAL> action)
        {
            IntPtr ncredsPtr;
            uint count;

            if (!CredEnumerate(GetTarget("*"), 0, out count, out ncredsPtr)
                .ThrowOnError("CredEnumerate", ERROR_NOT_FOUND))
            {
                return;
            }

            try
            {
                for (int i = 0; i != count; ++i)
                {
                    IntPtr ncredPtr = Marshal.ReadIntPtr(ncredsPtr, i * IntPtr.Size);
                    var ncred = (CREDENTIAL)Marshal.PtrToStructure(ncredPtr, typeof(CREDENTIAL));

                    action(ncred);
                }
            }
            finally
            {
                CredFree(ncredsPtr);
            }
        }

        public void Remove(string dbPath)
        {
            CredDelete(GetTarget(dbPath), CRED_TYPE_GENERIC, 0).ThrowOnError("CredDelete");
        }

        public bool TryGetValue(string dbPath, out ProtectedKey protectedKey)
        {
            protectedKey = null;
            IntPtr ncredPtr;

            if (!CredRead(GetTarget(dbPath), CRED_TYPE_GENERIC, 0, out ncredPtr).Result)
            {
                Debug.Assert(Marshal.GetLastWin32Error() == ERROR_NOT_FOUND);
                return false;
            }

            byte[] data = null;
            try
            {
                var ncred = (CREDENTIAL)Marshal.PtrToStructure(ncredPtr, typeof(CREDENTIAL));
                if (IsExpired(ncred))
                    return false;

                data = new byte[ncred.CredentialBlobSize];
                Marshal.Copy(ncred.CredentialBlob, data, 0, data.Length);

                protectedKey = ProtectedKey.Deserialize(data);
            }
            catch
            {
                CredDelete(GetTarget(dbPath), CRED_TYPE_GENERIC, 0);
                return false;
            }
            finally
            {
                CredFree(ncredPtr);
                if (data != null)
                    MemUtil.ZeroByteArray(data);
            }
            return true;
        }
    }
}
