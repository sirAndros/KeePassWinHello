using System;
using System.Runtime.InteropServices;
using System.Text;

namespace AdysTech.CredentialManager
{
    internal class Credential
    {
        public UInt32 Flags;
        public NativeCode.CredentialType Type;
        public string TargetName;
        public string Comment;
        public DateTime LastWritten;
        public UInt32 CredentialBlobSize;
        public string CredentialBlob;
        public NativeCode.Persistance Persist;
        public UInt32 AttributeCount;
        public IntPtr Attributes;
        public string TargetAlias;
        public string UserName;

        public Credential()
        {

        }

        internal Credential(NativeCode.NativeCredential ncred)
        {
            CredentialBlobSize = ncred.CredentialBlobSize;
            if (ncred.CredentialBlobSize > 2)
            {
                CredentialBlob = Marshal.PtrToStringUni(ncred.CredentialBlob,(int)ncred.CredentialBlobSize / 2);
            }
            
            UserName = Marshal.PtrToStringUni(ncred.UserName);
            TargetName = Marshal.PtrToStringUni(ncred.TargetName);
            TargetAlias = Marshal.PtrToStringUni(ncred.TargetAlias);
            Type = ncred.Type;
            Flags = ncred.Flags;
            Persist = (NativeCode.Persistance)ncred.Persist;
            try
            {
                LastWritten = DateTime.FromFileTime((long)((ulong)ncred.LastWritten.dwHighDateTime << 32 | (ulong)ncred.LastWritten.dwLowDateTime));
            }
            catch (ArgumentOutOfRangeException)
            { }
        }

        public Credential(System.Net.NetworkCredential credential)
        {
            CredentialBlob = credential.Password;
            UserName = String.IsNullOrWhiteSpace(credential.Domain) ? credential.UserName : credential.Domain + "\\" + credential.UserName;
            CredentialBlobSize = (UInt32)Encoding.Unicode.GetBytes(credential.Password).Length;
            AttributeCount = 0;
            Attributes = IntPtr.Zero;
            Comment = null;
            TargetAlias = null;
            Type = NativeCode.CredentialType.Generic;
            Persist = NativeCode.Persistance.Session;
        }

        /// <summary>
        /// This method derives a NativeCredential instance from a given Credential instance.
        /// </summary>
        /// <param name="cred">The managed Credential counterpart containing data to be stored.</param>
        /// <returns>A NativeCredential instance that is derived from the given Credential
        /// instance.</returns>
        internal NativeCode.NativeCredential GetNativeCredential()
        {
            NativeCode.NativeCredential ncred = new NativeCode.NativeCredential();
            ncred.AttributeCount = 0;
            ncred.Attributes = IntPtr.Zero;
            ncred.Comment = IntPtr.Zero;
            ncred.TargetAlias = IntPtr.Zero;
            ncred.Type = this.Type;
            ncred.Persist = (UInt32)this.Persist;
            ncred.UserName = Marshal.StringToCoTaskMemUni(this.UserName);
            ncred.TargetName = Marshal.StringToCoTaskMemUni(this.TargetName);
            ncred.CredentialBlob = Marshal.StringToCoTaskMemUni(this.CredentialBlob);
            ncred.CredentialBlobSize = (UInt32)this.CredentialBlobSize;
            if (this.LastWritten != DateTime.MinValue)
            {
                var fileTime = this.LastWritten.ToFileTimeUtc();
                ncred.LastWritten.dwLowDateTime = (int)(fileTime & 0xFFFFFFFFL);
                ncred.LastWritten.dwHighDateTime = (int)((fileTime >> 32) & 0xFFFFFFFFL);
            }
            return ncred;
        }
    }
}
