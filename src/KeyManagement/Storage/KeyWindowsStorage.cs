using KeePassLib.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;

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
        private static extern bool CredDelete(string target, uint type, int reservedFlag);

        [DllImport("advapi32.dll", EntryPoint = "CredEnumerateW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CredEnumerate(string target, uint flags, out uint count, out IntPtr credentialsPtr);

        [DllImport("advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CredRead(string target, uint type, int reservedFlag, out IntPtr CredentialPtr);

        [DllImport("advapi32.dll", EntryPoint = "CredWriteW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CredWrite([In] ref CREDENTIAL userCredential, uint flags);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool CredFree([In] IntPtr cred);
        #endregion

        private const int _maxBlobSize = 512 * 5;
        private const string _credPrefix = "KeePassWinHello_";

        public int Count
        {
            get
            {
                int count = 0;
                ForEach(ncred => count += IsValid(ncred) ? 1 : 0);
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

                    if (!CredWrite(ref ncred, 0))
                        throw new Win32Exception(Marshal.GetLastWin32Error());
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
                bool hasKey = CredRead(GetTarget(dbPath), CRED_TYPE_GENERIC, 0, out ncredPtr);
                return hasKey && IsValid((CREDENTIAL)Marshal.PtrToStructure(ncredPtr, typeof(CREDENTIAL)));
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
                if (!IsValid(ncred))
                    credsToRemove.Add(Marshal.PtrToStringUni(ncred.TargetName));
            });

            foreach (var target in credsToRemove)
                CredDelete(target, CRED_TYPE_GENERIC, 0);
        }

        private bool IsValid(CREDENTIAL ncred)
        {
            try
            {
                long highDateTime = (long)((uint)ncred.LastWritten.dwHighDateTime) << 32;
                long lowDateTime = (uint)ncred.LastWritten.dwLowDateTime;

                var createdDate = DateTime.FromFileTime(highDateTime | lowDateTime);
                if (DateTime.Now - createdDate >= Settings.Instance.InvalidatingTime)
                    return false;
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }

            return true;
        }

        private void ForEach(Action<CREDENTIAL> action)
        {
            IntPtr ncredsPtr;
            uint count;

            if (!CredEnumerate(GetTarget("*"), 0, out count, out ncredsPtr))
            {
                var lastError = Marshal.GetLastWin32Error();
                if (lastError == ERROR_NOT_FOUND)
                    return;

                throw new Win32Exception(lastError);
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
            if (!CredDelete(GetTarget(dbPath), CRED_TYPE_GENERIC, 0))
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        public bool TryGetValue(string dbPath, out ProtectedKey protectedKey)
        {
            protectedKey = null;
            IntPtr ncredPtr = IntPtr.Zero;

            if (!CredRead(GetTarget(dbPath), CRED_TYPE_GENERIC, 0, out ncredPtr))
            {
                Debug.Assert(Marshal.GetLastWin32Error() == ERROR_NOT_FOUND);
                return false;
            }

            byte[] data = null;
            try
            {
                var ncred = (CREDENTIAL)Marshal.PtrToStructure(ncredPtr, typeof(CREDENTIAL));
                if (!IsValid(ncred))
                    return false;

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
                if (data != null)
                    MemUtil.ZeroByteArray(data);
            }
            return true;
        }
    }
}
