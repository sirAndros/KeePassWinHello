using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;
using Microsoft.Win32.SafeHandles;

namespace KeePassWinHello
{
    internal class WinHelloProvider : IAuthProvider
    {
        #region CNG key storage provider API
        private const string MS_NGC_KEY_STORAGE_PROVIDER = "Microsoft Passport Key Storage Provider";
        private const string NCRYPT_WINDOW_HANDLE_PROPERTY = "HWND Handle";
        private const string NCRYPT_USE_CONTEXT_PROPERTY = "Use Context";
        private const string NCRYPT_LENGTH_PROPERTY = "Length";
        private const string NCRYPT_KEY_USAGE_PROPERTY = "Key Usage";
        private const string NCRYPT_NGC_CACHE_TYPE_PROPERTY = "NgcCacheType";
        private const string NCRYPT_NGC_CACHE_TYPE_PROPERTY_DEPRECATED = "NgcCacheTypeProperty";
        private const string NCRYPT_PIN_CACHE_IS_GESTURE_REQUIRED_PROPERTY = "PinCacheIsGestureRequired";
        private const string BCRYPT_RSA_ALGORITHM = "RSA";
        private const int NCRYPT_ALLOW_DECRYPT_FLAG = 0x00000001;
        private const int NCRYPT_ALLOW_SIGNING_FLAG = 0x00000002;
        private const int NCRYPT_ALLOW_KEY_IMPORT_FLAG = 0x00000008;
        private const int NCRYPT_PAD_PKCS1_FLAG = 0x00000002;
        private const int NTE_USER_CANCELLED = unchecked((int)0x80090036);
        private const int NTE_NO_KEY = unchecked((int)0x8009000D);

        [StructLayout(LayoutKind.Sequential)]
        struct SECURITY_STATUS
        {
            public int secStatus;

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
            public void CheckStatus(string name = "", int ignoreStatus = 0)
            {
                if (secStatus >= 0 || secStatus == ignoreStatus)
                    return;

                switch (secStatus)
                {
                    case NTE_USER_CANCELLED:
                        throw new AuthProviderUserCancelledException();
                    default:
                        throw new AuthProviderSystemErrorException(name, secStatus);
                }
            }
        }

        [DllImport("cryptngc.dll", CharSet = CharSet.Unicode)]
        private static extern SECURITY_STATUS NgcGetDefaultDecryptionKeyName(string pszSid, int dwReserved1, int dwReserved2, [Out] out string ppszKeyName);

        [DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
        private static extern SECURITY_STATUS NCryptOpenStorageProvider([Out] out SafeNCryptProviderHandle phProvider, string pszProviderName, int dwFlags);

        [DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
        private static extern SECURITY_STATUS NCryptOpenKey(SafeNCryptProviderHandle hProvider, [Out] out SafeNCryptKeyHandle phKey, string pszKeyName, int dwLegacyKeySpec, CngKeyOpenOptions dwFlags);

        [DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
        private static extern SECURITY_STATUS NCryptCreatePersistedKey(SafeNCryptProviderHandle hProvider,
                                                          [Out] out SafeNCryptKeyHandle phKey,
                                                          string pszAlgId,
                                                          string pszKeyName,
                                                          int dwLegacyKeySpec,
                                                          CngKeyCreationOptions dwFlags);

        [DllImport("ncrypt.dll")]
        private static extern SECURITY_STATUS NCryptFinalizeKey(SafeNCryptKeyHandle hKey, int dwFlags);

        [DllImport("ncrypt.dll")]
        private static extern SECURITY_STATUS NCryptDeleteKey(SafeNCryptKeyHandle hKey, int flags);

        [DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
        private static extern SECURITY_STATUS NCryptGetProperty(SafeNCryptHandle hObject, string pszProperty, ref int pbOutput, int cbOutput, [Out] out int pcbResult, CngPropertyOptions dwFlags);

        [DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
        private static extern SECURITY_STATUS NCryptSetProperty(SafeNCryptHandle hObject, string pszProperty, string pbInput, int cbInput, CngPropertyOptions dwFlags);

        [DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
        private static extern SECURITY_STATUS NCryptSetProperty(SafeNCryptHandle hObject, string pszProperty, [In, MarshalAs(UnmanagedType.LPArray)] byte[] pbInput, int cbInput, CngPropertyOptions dwFlags);

        [DllImport("ncrypt.dll")]
        private static extern SECURITY_STATUS NCryptEncrypt(SafeNCryptKeyHandle hKey,
                                               [In, MarshalAs(UnmanagedType.LPArray)] byte[] pbInput,
                                               int cbInput,
                                               IntPtr pvPaddingZero,
                                               [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pbOutput,
                                               int cbOutput,
                                               [Out] out int pcbResult,
                                               int dwFlags);

        [DllImport("ncrypt.dll")]
        private static extern SECURITY_STATUS NCryptDecrypt(SafeNCryptKeyHandle hKey,
                                               [In, MarshalAs(UnmanagedType.LPArray)] byte[] pbInput,
                                               int cbInput,
                                               IntPtr pvPaddingZero,
                                               [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pbOutput,
                                               int cbOutput,
                                               [Out] out int pcbResult,
                                               int dwFlags);
        #endregion

        private static readonly Lazy<string> _localKeyName = new Lazy<string>(RetreiveLocalKeyName);
        private static readonly Lazy<string> _persistentKeyName = new Lazy<string>(RetreivePersistentKeyName);

        private static readonly object _mutex = new object();
        private static WeakReference _instance;

        private const string Domain = Settings.ProductName;
        private const string SubDomain = "";
        private const string PersistentName = Settings.ProductName;
        private const string InvalidatedKeyMessage = "Persistent key has not met integrity requirements. It might be caused by a spoofing attack. Try to recreate the key.";
        private string _currentKeyName;

        private static string RetreiveLocalKeyName()
        {
            string key;
            NgcGetDefaultDecryptionKeyName(WindowsIdentity.GetCurrent().User.Value, 0, 0, out key);
            return key;
        }
        private static string RetreivePersistentKeyName()
        {
            var sid = WindowsIdentity.GetCurrent().User.Value;
            return sid + "//" + Domain + "/" + SubDomain + "/" + PersistentName;
        }

        private static bool IsAvailable()
        {
            return !string.IsNullOrEmpty(_localKeyName.Value);
        }

        private WinHelloProvider(AuthCacheType authCacheType)
        {
            if (authCacheType == AuthCacheType.Local)
            {
                DeletePersistentKey();
                _currentKeyName = _localKeyName.Value;
            }
            else
            {
                System.Diagnostics.Debug.Assert(authCacheType == AuthCacheType.Persistent);

                SafeNCryptKeyHandle ngcKeyHandle;
                if (!TryOpenPersistentKey(out ngcKeyHandle))
                    throw new AuthProviderInvalidKeyException("Persistent key does not exist.");

                using (ngcKeyHandle)
                {
                    if (!VerifyPersistentKeyIntegrity(ngcKeyHandle))
                    {
                        ngcKeyHandle.Close();
                        DeletePersistentKey();
                        throw new AuthProviderInvalidKeyException(InvalidatedKeyMessage);
                    }
                }

                _currentKeyName = _persistentKeyName.Value;
            }
        }

        public static WinHelloProvider CreateInstance(AuthCacheType authCacheType)
        {
            if (!IsAvailable())
                throw new AuthProviderIsUnavailableException("Windows Hello is not available.");

            lock (_mutex)
            {
                WinHelloProvider winHelloProvider = null;
                if (_instance != null && (winHelloProvider = _instance.Target as WinHelloProvider) != null)
                {
                    if (winHelloProvider.CurrentCacheType == authCacheType)
                        return winHelloProvider;
                    else
                        throw new AuthProviderException("Incompatible cache type with existing instance.");
                }

                winHelloProvider = new WinHelloProvider(authCacheType);
                _instance = new WeakReference(winHelloProvider);

                return winHelloProvider;
            }
        }

        public void ClaimCurrentCacheType(AuthCacheType authCacheType)
        {
            if (CurrentCacheType == authCacheType)
                return;

            lock (_mutex)
            {
                if (authCacheType == AuthCacheType.Local)
                {
                    DeletePersistentKey();
                }
                else
                {
                    System.Diagnostics.Debug.Assert(authCacheType == AuthCacheType.Persistent);

                    SafeNCryptKeyHandle ngcKeyHandle;
                    if (TryOpenPersistentKey(out ngcKeyHandle))
                    {
                        using (ngcKeyHandle)
                        {
                            if (!VerifyPersistentKeyIntegrity(ngcKeyHandle))
                                throw new AuthProviderInvalidKeyException(InvalidatedKeyMessage);
                        }
                    }
                    else
                    {
                        using (ngcKeyHandle = CreatePersistentKey(false)) { }
                    }
                }
                CurrentCacheType = authCacheType;
            }
        }

        public AuthCacheType CurrentCacheType
        {
            get
            {
                return _currentKeyName == _localKeyName.Value ? AuthCacheType.Local : AuthCacheType.Persistent;
            }
            private set
            {
                if (value == AuthCacheType.Local)
                    _currentKeyName = _localKeyName.Value;
                else
                {
                    System.Diagnostics.Debug.Assert(value == AuthCacheType.Persistent);
                    _currentKeyName = _persistentKeyName.Value;
                }
            }
        }

        private static void DeletePersistentKey()
        {
            SafeNCryptKeyHandle ngcKeyHandle;
            if (TryOpenPersistentKey(out ngcKeyHandle))
            {
                using (ngcKeyHandle)
                {
                    NCryptDeleteKey(ngcKeyHandle, 0).CheckStatus("NCryptDeleteKey");
                    ngcKeyHandle.SetHandleAsInvalid();
                }
            }
        }

        private static bool TryOpenPersistentKey(out SafeNCryptKeyHandle ngcKeyHandle)
        {
            SafeNCryptProviderHandle ngcProviderHandle;
            NCryptOpenStorageProvider(out ngcProviderHandle, MS_NGC_KEY_STORAGE_PROVIDER, 0).CheckStatus("NCryptOpenStorageProvider");

            using (ngcProviderHandle)
            {
                NCryptOpenKey(ngcProviderHandle,
                    out ngcKeyHandle,
                    _persistentKeyName.Value,
                    0, CngKeyOpenOptions.None
                    ).CheckStatus("NCryptOpenKey", NTE_NO_KEY);
            }

            return ngcKeyHandle != null && !ngcKeyHandle.IsInvalid;
        }

        private static bool VerifyPersistentKeyIntegrity(SafeNCryptKeyHandle ngcKeyHandle)
        {
            int pcbResult;
            int keyUsage = 0;
            NCryptGetProperty(ngcKeyHandle, NCRYPT_KEY_USAGE_PROPERTY, ref keyUsage, sizeof(int), out pcbResult, CngPropertyOptions.None).CheckStatus("NCRYPT_KEY_USAGE_PROPERTY");
            if ((keyUsage & NCRYPT_ALLOW_KEY_IMPORT_FLAG) == NCRYPT_ALLOW_KEY_IMPORT_FLAG)
                return false;

            int cacheType = 0;
            try
            {
                NCryptGetProperty(ngcKeyHandle, NCRYPT_NGC_CACHE_TYPE_PROPERTY, ref cacheType, sizeof(int), out pcbResult, CngPropertyOptions.None).CheckStatus("NCRYPT_NGC_CACHE_TYPE_PROPERTY");
            }
            catch
            {
                NCryptGetProperty(ngcKeyHandle, NCRYPT_NGC_CACHE_TYPE_PROPERTY_DEPRECATED, ref cacheType, sizeof(int), out pcbResult, CngPropertyOptions.None).CheckStatus("NCRYPT_NGC_CACHE_TYPE_PROPERTY_DEPRECATED");
            }
            if (cacheType == 0)
                return false;

            return true;
        }

        private static SafeNCryptKeyHandle CreatePersistentKey(bool overwriteExisting)
        {
            SafeNCryptProviderHandle ngcProviderHandle;

            NCryptOpenStorageProvider(out ngcProviderHandle, MS_NGC_KEY_STORAGE_PROVIDER, 0).CheckStatus("NCryptOpenStorageProvider");

            SafeNCryptKeyHandle ngcKeyHandle;
            using (ngcProviderHandle)
            {
                NCryptCreatePersistedKey(ngcProviderHandle,
                            out ngcKeyHandle,
                            BCRYPT_RSA_ALGORITHM,
                            _persistentKeyName.Value,
                            0, overwriteExisting ? CngKeyCreationOptions.OverwriteExistingKey : CngKeyCreationOptions.None
                            ).CheckStatus("NCryptCreatePersistedKey");

                byte[] lengthProp = BitConverter.GetBytes(2048);
                NCryptSetProperty(ngcKeyHandle, NCRYPT_LENGTH_PROPERTY, lengthProp, lengthProp.Length, CngPropertyOptions.None).CheckStatus("NCRYPT_LENGTH_PROPERTY");

                byte[] keyUsage = BitConverter.GetBytes(NCRYPT_ALLOW_DECRYPT_FLAG | NCRYPT_ALLOW_SIGNING_FLAG);
                NCryptSetProperty(ngcKeyHandle, NCRYPT_KEY_USAGE_PROPERTY, keyUsage, keyUsage.Length, CngPropertyOptions.None).CheckStatus("NCRYPT_KEY_USAGE_PROPERTY");

                byte[] cacheType = BitConverter.GetBytes(1);
                try
                {
                    NCryptSetProperty(ngcKeyHandle, NCRYPT_NGC_CACHE_TYPE_PROPERTY, cacheType, cacheType.Length, CngPropertyOptions.None).CheckStatus("NCRYPT_NGC_CACHE_TYPE_PROPERTY");
                }
                catch
                {
                    NCryptSetProperty(ngcKeyHandle, NCRYPT_NGC_CACHE_TYPE_PROPERTY_DEPRECATED, cacheType, cacheType.Length, CngPropertyOptions.None).CheckStatus("NCRYPT_NGC_CACHE_TYPE_PROPERTY_DEPRECATED");
                }

                ApplyUIContext(ngcKeyHandle);

                NCryptFinalizeKey(ngcKeyHandle, 0).CheckStatus("NCryptFinalizeKey");
            }

            return ngcKeyHandle;
        }

        public byte[] Encrypt(byte[] data)
        {
            byte[] cbResult;
            SafeNCryptProviderHandle ngcProviderHandle;
            NCryptOpenStorageProvider(out ngcProviderHandle, MS_NGC_KEY_STORAGE_PROVIDER, 0).CheckStatus("NCryptOpenStorageProvider");
            using (ngcProviderHandle)
            {
                SafeNCryptKeyHandle ngcKeyHandle;
                NCryptOpenKey(ngcProviderHandle, out ngcKeyHandle, _currentKeyName, 0, CngKeyOpenOptions.Silent).CheckStatus("NCryptOpenKey");
                using (ngcKeyHandle)
                {
                    if (CurrentCacheType == AuthCacheType.Persistent && !VerifyPersistentKeyIntegrity(ngcKeyHandle))
                        throw new AuthProviderInvalidKeyException(InvalidatedKeyMessage);

                    int pcbResult;
                    NCryptEncrypt(ngcKeyHandle, data, data.Length, IntPtr.Zero, null, 0, out pcbResult, NCRYPT_PAD_PKCS1_FLAG).CheckStatus("NCryptEncrypt");

                    cbResult = new byte[pcbResult];
                    NCryptEncrypt(ngcKeyHandle, data, data.Length, IntPtr.Zero, cbResult, cbResult.Length, out pcbResult, NCRYPT_PAD_PKCS1_FLAG).CheckStatus("NCryptEncrypt");
                    System.Diagnostics.Debug.Assert(cbResult.Length == pcbResult);
                }
            }

            return cbResult;
        }

        public byte[] PromptToDecrypt(byte[] data)
        {
            byte[] cbResult;
            SafeNCryptProviderHandle ngcProviderHandle;
            NCryptOpenStorageProvider(out ngcProviderHandle, MS_NGC_KEY_STORAGE_PROVIDER, 0).CheckStatus("NCryptOpenStorageProvider");
            using (ngcProviderHandle)
            {
                SafeNCryptKeyHandle ngcKeyHandle;
                NCryptOpenKey(ngcProviderHandle, out ngcKeyHandle, _currentKeyName, 0, CngKeyOpenOptions.None).CheckStatus("NCryptOpenKey");
                using (ngcKeyHandle)
                {
                    if (CurrentCacheType == AuthCacheType.Persistent && !VerifyPersistentKeyIntegrity(ngcKeyHandle))
                        throw new AuthProviderInvalidKeyException(InvalidatedKeyMessage);

                    ApplyUIContext(ngcKeyHandle);

                    byte[] pinRequired = BitConverter.GetBytes(1);
                    NCryptSetProperty(ngcKeyHandle, NCRYPT_PIN_CACHE_IS_GESTURE_REQUIRED_PROPERTY, pinRequired, pinRequired.Length, CngPropertyOptions.None).CheckStatus("NCRYPT_PIN_CACHE_IS_GESTURE_REQUIRED_PROPERTY");

                    // The pbInput and pbOutput parameters can point to the same buffer. In this case, this function will perform the decryption in place.
                    cbResult = new byte[data.Length * 2];
                    int pcbResult;
                    NCryptDecrypt(ngcKeyHandle, data, data.Length, IntPtr.Zero, cbResult, cbResult.Length, out pcbResult, NCRYPT_PAD_PKCS1_FLAG).CheckStatus("NCryptDecrypt");
                    // TODO: secure resize
                    Array.Resize(ref cbResult, pcbResult);
                }
            }

            return cbResult;
        }

        private static void ApplyUIContext(SafeNCryptKeyHandle ngcKeyHandle)
        {
            var uiContext = AuthProviderUIContext.Current;
            if (uiContext != null)
            {
                IntPtr parentWindowHandle = uiContext.ParentWindowHandle;
                if (parentWindowHandle != IntPtr.Zero)
                {
                    byte[] handle = BitConverter.GetBytes(IntPtr.Size == 8 ? parentWindowHandle.ToInt64() : parentWindowHandle.ToInt32());
                    NCryptSetProperty(ngcKeyHandle, NCRYPT_WINDOW_HANDLE_PROPERTY, handle, handle.Length, CngPropertyOptions.None).CheckStatus("NCRYPT_WINDOW_HANDLE_PROPERTY");
                }

                string message = uiContext.Message;
                if (!string.IsNullOrEmpty(message))
                    NCryptSetProperty(ngcKeyHandle, NCRYPT_USE_CONTEXT_PROPERTY, message, (message.Length + 1) * 2, CngPropertyOptions.None).CheckStatus("NCRYPT_USE_CONTEXT_PROPERTY");
            }
        }
    }
}
