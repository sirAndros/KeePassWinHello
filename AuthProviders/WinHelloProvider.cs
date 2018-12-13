using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;
using Microsoft.Win32.SafeHandles;

namespace KeePassWinHello
{
    internal class WinHelloProvider : IAuthProvider
    {
        private const string MS_NGC_KEY_STORAGE_PROVIDER = "Microsoft Passport Key Storage Provider";
        private const string NCRYPT_WINDOW_HANDLE_PROPERTY = "HWND Handle";
        private const string NCRYPT_USE_CONTEXT_PROPERTY = "Use Context";
        private const string NCRYPT_PIN_CACHE_IS_GESTURE_REQUIRED_PROPERTY = "PinCacheIsGestureRequired";
        private const int NCRYPT_PAD_PKCS1_FLAG = 0x00000002;
        private const int NTE_USER_CANCELLED = unchecked((int)0x80090036);

        [DllImport("cryptngc.dll", CharSet = CharSet.Unicode)]
        private static extern int NgcGetDefaultDecryptionKeyName(string pszSid, int dwReserved1, int dwReserved2, [Out] out string ppszKeyName);

        [DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
        private static extern int NCryptOpenStorageProvider([Out] out SafeNCryptProviderHandle phProvider, string pszProviderName, int dwFlags);

        [DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
        private static extern int NCryptOpenKey(SafeNCryptProviderHandle hProvider, [Out] out SafeNCryptKeyHandle phKey, string pszKeyName, int dwLegacyKeySpec, CngKeyOpenOptions dwFlags);

        [DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
        private static extern int NCryptSetProperty(SafeNCryptHandle hObject, string pszProperty, string pbInput, int cbInput, CngPropertyOptions dwFlags);

        [DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
        private static extern int NCryptSetProperty(SafeNCryptHandle hObject, string pszProperty, [In, MarshalAs(UnmanagedType.LPArray)] byte[] pbInput, int cbInput, CngPropertyOptions dwFlags);

        [DllImport("ncrypt.dll")]
        private static extern int NCryptEncrypt(SafeNCryptKeyHandle hKey,
                                               [In, MarshalAs(UnmanagedType.LPArray)] byte[] pbInput,
                                               int cbInput,
                                               IntPtr pvPaddingZero,
                                               [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pbOutput,
                                               int cbOutput,
                                               [Out] out int pcbResult,
                                               int dwFlags);

        [DllImport("ncrypt.dll")]
        private static extern int NCryptDecrypt(SafeNCryptKeyHandle hKey,
                                               [In, MarshalAs(UnmanagedType.LPArray)] byte[] pbInput,
                                               int cbInput,
                                               IntPtr pvPaddingZero,
                                               [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pbOutput,
                                               int cbOutput,
                                               [Out] out int pcbResult,
                                               int dwFlags);


        private static Lazy<string> m_CurrentPassportKeyName = new Lazy<string>(retreivePassportKeyName);

        private static string retreivePassportKeyName()
        {
            string key;
            NgcGetDefaultDecryptionKeyName(WindowsIdentity.GetCurrent().User.Value, 0, 0, out key);
            return key;
        }

        private int chkResult
        {
            set
            {
                if (value < 0)
                {
                    /*
                     * NTE_BAD_FLAGS
                     * NTE_BAD_KEYSET
                     * NTE_BAD_KEY_STATE
                     * NTE_BUFFER_TOO_SMALL
                     * NTE_INVALID_HANDLE
                     * NTE_INVALID_PARAMETER
                     * NTE_PERM
                     * NTE_NO_MEMORY
                     * NTE_NOT_SUPPORTED
                     * NTE_USER_CANCELLED
                     */
                    switch (value)
                    {
                        case NTE_USER_CANCELLED:
                            throw new UnauthorizedAccessException("Canceled");
                        default:
                            throw new ExternalException("External error occurred", value);
                    }
                }
            }
        }

        /// public API
        public static bool IsAvailable()
        {
            return !string.IsNullOrEmpty(m_CurrentPassportKeyName.Value);
        }

        public WinHelloProvider() { }

        public string Message { get; set; }

        public IntPtr ParentHandle { get; set; }

        public byte[] Encrypt(byte[] data)
        {
            if (!IsAvailable())
                throw new NotSupportedException("Windows Hello is not available");

            byte[] cbResult;
            SafeNCryptProviderHandle ngcProviderHandle;
            chkResult = NCryptOpenStorageProvider(out ngcProviderHandle, MS_NGC_KEY_STORAGE_PROVIDER, 0);
            using (ngcProviderHandle)
            {
                SafeNCryptKeyHandle ngcKeyHandle;
                chkResult = NCryptOpenKey(ngcProviderHandle, out ngcKeyHandle, m_CurrentPassportKeyName.Value, 0, CngKeyOpenOptions.Silent);
                using (ngcKeyHandle)
                {
                    int pcbResult;
                    chkResult = NCryptEncrypt(ngcKeyHandle, data, data.Length, IntPtr.Zero, null, 0, out pcbResult, NCRYPT_PAD_PKCS1_FLAG);

                    cbResult = new byte[pcbResult];
                    chkResult = NCryptEncrypt(ngcKeyHandle, data, data.Length, IntPtr.Zero, cbResult, cbResult.Length, out pcbResult, NCRYPT_PAD_PKCS1_FLAG);
                    System.Diagnostics.Debug.Assert(cbResult.Length == pcbResult);
                }
            }

            return cbResult;
        }

        public byte[] PromptToDecrypt(byte[] data)
        {
            if (!IsAvailable())
                throw new NotSupportedException("Windows Hello is not available");

            byte[] cbResult;
            SafeNCryptProviderHandle ngcProviderHandle;
            chkResult = NCryptOpenStorageProvider(out ngcProviderHandle, MS_NGC_KEY_STORAGE_PROVIDER, 0);
            using (ngcProviderHandle)
            {
                SafeNCryptKeyHandle ngcKeyHandle;
                chkResult = NCryptOpenKey(ngcProviderHandle, out ngcKeyHandle, m_CurrentPassportKeyName.Value, 0, CngKeyOpenOptions.None);
                using (ngcKeyHandle)
                {
                    if (ParentHandle != IntPtr.Zero)
                    {
                        byte[] handle = BitConverter.GetBytes(IntPtr.Size == 8 ? ParentHandle.ToInt64() : ParentHandle.ToInt32());
                        chkResult = NCryptSetProperty(ngcKeyHandle, NCRYPT_WINDOW_HANDLE_PROPERTY, handle, handle.Length, CngPropertyOptions.None);
                    }

                    if (!string.IsNullOrEmpty(Message))
                        chkResult = NCryptSetProperty(ngcKeyHandle, NCRYPT_USE_CONTEXT_PROPERTY, Message, (Message.Length + 1) * 2, CngPropertyOptions.None);

                    byte[] pinRequired = BitConverter.GetBytes(1);
                    chkResult = NCryptSetProperty(ngcKeyHandle, NCRYPT_PIN_CACHE_IS_GESTURE_REQUIRED_PROPERTY, pinRequired, pinRequired.Length, CngPropertyOptions.None);

                    // The pbInput and pbOutput parameters can point to the same buffer. In this case, this function will perform the decryption in place.
                    cbResult = new byte[data.Length * 2];
                    int pcbResult;
                    chkResult = NCryptDecrypt(ngcKeyHandle, data, data.Length, IntPtr.Zero, cbResult, cbResult.Length, out pcbResult, NCRYPT_PAD_PKCS1_FLAG);
                    // TODO: secure resize
                    Array.Resize(ref cbResult, pcbResult);
                }
            }

            return cbResult;
        }

        public static string GetKeyName()
        {
            return m_CurrentPassportKeyName.Value;
        }
    }
}
