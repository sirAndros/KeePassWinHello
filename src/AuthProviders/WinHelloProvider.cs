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
            public void CheckStatus(int ignoreStatus = 0)
            {
                if (secStatus >= 0 || secStatus == ignoreStatus)
                    return;

                switch (secStatus)
                {
                    case NTE_USER_CANCELLED:
                        throw new AuthProviderUserCancelledException();
                    default:
                        throw new AuthProviderSystemErrorException("External error occurred", secStatus);
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
                _currentKeyName = RetreiveLocalKeyName();
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

                _currentKeyName = RetreivePersistentKeyName();
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
                return _currentKeyName == RetreiveLocalKeyName() ? AuthCacheType.Local : AuthCacheType.Persistent;
            }
            private set
            {
                if (value == AuthCacheType.Local)
                    _currentKeyName = RetreiveLocalKeyName();
                else
                {
                    System.Diagnostics.Debug.Assert(value == AuthCacheType.Persistent);
                    _currentKeyName = RetreivePersistentKeyName();
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
                    NCryptDeleteKey(ngcKeyHandle, 0).CheckStatus();
                    ngcKeyHandle.SetHandleAsInvalid();
                }
            }
        }

        private static bool TryOpenPersistentKey(out SafeNCryptKeyHandle ngcKeyHandle)
        {
            SafeNCryptProviderHandle ngcProviderHandle;
            NCryptOpenStorageProvider(out ngcProviderHandle, MS_NGC_KEY_STORAGE_PROVIDER, 0).CheckStatus();

            using (ngcProviderHandle)
            {
                NCryptOpenKey(ngcProviderHandle,
                    out ngcKeyHandle,
                    RetreivePersistentKeyName(),
                    0, CngKeyOpenOptions.None
                    ).CheckStatus(NTE_NO_KEY);
            }

            return ngcKeyHandle != null && !ngcKeyHandle.IsInvalid;
        }

        private static bool VerifyPersistentKeyIntegrity(SafeNCryptKeyHandle ngcKeyHandle)
        {
            int pcbResult;
            int keyUsage = 0;
            NCryptGetProperty(ngcKeyHandle, NCRYPT_KEY_USAGE_PROPERTY, ref keyUsage, sizeof(int), out pcbResult, CngPropertyOptions.None).CheckStatus();
            if ((keyUsage & NCRYPT_ALLOW_KEY_IMPORT_FLAG) == NCRYPT_ALLOW_KEY_IMPORT_FLAG)
                return false;

            int cacheType = 0;
            NCryptGetProperty(ngcKeyHandle, NCRYPT_NGC_CACHE_TYPE_PROPERTY, ref cacheType, sizeof(int), out pcbResult, CngPropertyOptions.None).CheckStatus();
            if (cacheType == 0)
                return false;

            return true;
        }

        private static SafeNCryptKeyHandle CreatePersistentKey(bool overwriteExisting)
        {
            SafeNCryptProviderHandle ngcProviderHandle;
            NCryptOpenStorageProvider(out ngcProviderHandle, MS_NGC_KEY_STORAGE_PROVIDER, 0).CheckStatus();

            SafeNCryptKeyHandle ngcKeyHandle;
            using (ngcProviderHandle)
            {
                NCryptCreatePersistedKey(ngcProviderHandle,
                    out ngcKeyHandle,
                    BCRYPT_RSA_ALGORITHM,
                    RetreivePersistentKeyName(),
                    0, overwriteExisting ? CngKeyCreationOptions.OverwriteExistingKey : CngKeyCreationOptions.None
                    ).CheckStatus();

                byte[] lengthProp = BitConverter.GetBytes(2048);
                NCryptSetProperty(ngcKeyHandle, NCRYPT_LENGTH_PROPERTY, lengthProp, lengthProp.Length, CngPropertyOptions.None).CheckStatus();

                byte[] keyUsage = BitConverter.GetBytes(NCRYPT_ALLOW_DECRYPT_FLAG | NCRYPT_ALLOW_SIGNING_FLAG);
                NCryptSetProperty(ngcKeyHandle, NCRYPT_KEY_USAGE_PROPERTY, keyUsage, keyUsage.Length, CngPropertyOptions.None).CheckStatus();

                byte[] cacheType = BitConverter.GetBytes(1);
                NCryptSetProperty(ngcKeyHandle, NCRYPT_NGC_CACHE_TYPE_PROPERTY, cacheType, cacheType.Length, CngPropertyOptions.None).CheckStatus();

                ApplyUIContext(ngcKeyHandle);

                NCryptFinalizeKey(ngcKeyHandle, 0).CheckStatus();
            }

            return ngcKeyHandle;
        }

        public byte[] Encrypt(byte[] data)
        {
            byte[] cbResult;
            SafeNCryptProviderHandle ngcProviderHandle;
            NCryptOpenStorageProvider(out ngcProviderHandle, MS_NGC_KEY_STORAGE_PROVIDER, 0).CheckStatus();
            using (ngcProviderHandle)
            {
                SafeNCryptKeyHandle ngcKeyHandle;
                NCryptOpenKey(ngcProviderHandle, out ngcKeyHandle, _currentKeyName, 0, CngKeyOpenOptions.Silent).CheckStatus();
                using (ngcKeyHandle)
                {
                    if (CurrentCacheType == AuthCacheType.Persistent && !VerifyPersistentKeyIntegrity(ngcKeyHandle))
                        throw new AuthProviderInvalidKeyException(InvalidatedKeyMessage);

                    int pcbResult;
                    NCryptEncrypt(ngcKeyHandle, data, data.Length, IntPtr.Zero, null, 0, out pcbResult, NCRYPT_PAD_PKCS1_FLAG).CheckStatus();

                    cbResult = new byte[pcbResult];
                    NCryptEncrypt(ngcKeyHandle, data, data.Length, IntPtr.Zero, cbResult, cbResult.Length, out pcbResult, NCRYPT_PAD_PKCS1_FLAG).CheckStatus();
                    System.Diagnostics.Debug.Assert(cbResult.Length == pcbResult);
                }
            }

            return cbResult;
        }

        public byte[] PromptToDecrypt(byte[] data)
        {
            byte[] cbResult;
            SafeNCryptProviderHandle ngcProviderHandle;
            NCryptOpenStorageProvider(out ngcProviderHandle, MS_NGC_KEY_STORAGE_PROVIDER, 0).CheckStatus();
            using (ngcProviderHandle)
            {
                SafeNCryptKeyHandle ngcKeyHandle;
                NCryptOpenKey(ngcProviderHandle, out ngcKeyHandle, _currentKeyName, 0, CngKeyOpenOptions.None).CheckStatus();
                using (ngcKeyHandle)
                {
                    if (CurrentCacheType == AuthCacheType.Persistent && !VerifyPersistentKeyIntegrity(ngcKeyHandle))
                        throw new AuthProviderInvalidKeyException(InvalidatedKeyMessage);

                    ApplyUIContext(ngcKeyHandle);

                    byte[] pinRequired = BitConverter.GetBytes(1);
                    NCryptSetProperty(ngcKeyHandle, NCRYPT_PIN_CACHE_IS_GESTURE_REQUIRED_PROPERTY, pinRequired, pinRequired.Length, CngPropertyOptions.None).CheckStatus();

                    // The pbInput and pbOutput parameters can point to the same buffer. In this case, this function will perform the decryption in place.
                    cbResult = new byte[data.Length * 2];
                    int pcbResult;
                    NCryptDecrypt(ngcKeyHandle, data, data.Length, IntPtr.Zero, cbResult, cbResult.Length, out pcbResult, NCRYPT_PAD_PKCS1_FLAG).CheckStatus();
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
                    NCryptSetProperty(ngcKeyHandle, NCRYPT_WINDOW_HANDLE_PROPERTY, handle, handle.Length, CngPropertyOptions.None).CheckStatus();
                }

                string message = uiContext.Message;
                if (!string.IsNullOrEmpty(message))
                    NCryptSetProperty(ngcKeyHandle, NCRYPT_USE_CONTEXT_PROPERTY, message, (message.Length + 1) * 2, CngPropertyOptions.None).CheckStatus();
            }
        }
    }



#if MONO

    /// <summary>
    ///     Flags to control how often and in which format a key is allowed to be exported
    /// </summary>
    [Flags]
    public enum CngExportPolicies {
        None = 0x00000000,
        AllowExport = 0x00000001,                       // NCRYPT_ALLOW_EXPORT_FLAG
        AllowPlaintextExport = 0x00000002,              // NCRYPT_ALLOW_PLAINTEXT_EXPORT_FLAG
        AllowArchiving = 0x00000004,                    // NCRYPT_ALLOW_ARCHIVING_FLAG
        AllowPlaintextArchiving = 0x00000008            // NCRYPT_ALLOW_PLAINTEXT_ARCHIVING_FLAG
    }

    /// <summary>
    ///     Flags controlling how the key is created
    /// </summary>
    [Flags]
    public enum CngKeyCreationOptions {
        None = 0x00000000,
        MachineKey = 0x00000020,                        // NCRYPT_MACHINE_KEY_FLAG
        OverwriteExistingKey = 0x00000080               // NCRYPT_OVERWRITE_KEY_FLAG               
    }

    /// <summary>
    ///     Flags controlling how a key is opened
    /// </summary>
    [Flags]
    public enum CngKeyOpenOptions {
        None = 0x00000000,
        UserKey = 0x00000000,
        MachineKey = 0x00000020,                        // NCRYPT_MACHINE_KEY_FLAG
        Silent = 0x00000040                             // NCRYPT_SILENT_FLAG                      
    }

    /// <summary>
    ///     Flags indicating the type of key
    /// </summary>
    [Flags]
    internal enum CngKeyTypes {
        None = 0x00000000,
        MachineKey = 0x00000020                         // NCRYPT_MACHINE_KEY_FLAG
    }

    /// <summary>
    ///     Bits defining what operations are valid to use a key with
    /// </summary>
    [Flags]
    public enum CngKeyUsages {
        None = 0x00000000,
        Decryption = 0x00000001,                        // NCRYPT_ALLOW_DECRYPT_FLAG
        Signing = 0x00000002,                           // NCRYPT_ALLOW_SIGNING_FLAG
        KeyAgreement = 0x00000004,                      // NCRYPT_ALLOW_KEY_AGREEMENT_FLAG
        AllUsages = 0x00ffffff                          // NCRYPT_ALLOW_ALL_USAGES
    }

    /// <summary>
    ///     Options affecting how a property is interpreted by CNG
    /// </summary>
    [Flags]
    public enum CngPropertyOptions {
        None = 0x00000000,
        CustomProperty = 0x40000000,                    // NCRYPT_PERSIST_ONLY_FLAG
        Persist = unchecked((int)0x80000000)            // NCRYPT_PERSIST_FLAG
    }

    public abstract class SafeNCryptHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		protected SafeNCryptHandle ()
			: base (true)
		{
		}

		protected SafeNCryptHandle (IntPtr handle, System.Runtime.InteropServices.SafeHandle parentHandle)
			: base (false)
		{
			throw new NotImplementedException ();
		}

		public override bool IsInvalid { get { throw new NotImplementedException (); } }

		protected override bool ReleaseHandle ()
		{
			return false;
		}

		protected abstract bool ReleaseNativeHandle();
	}
    
    public sealed class SafeNCryptKeyHandle : SafeNCryptHandle
	{
		public SafeNCryptKeyHandle ()
		{
		}

		public SafeNCryptKeyHandle (IntPtr handle, System.Runtime.InteropServices.SafeHandle parentHandle)
			: base (handle, parentHandle)
		{

		}

		protected override bool ReleaseNativeHandle ()
		{
			return false;
		}
	}
    
    public sealed class SafeNCryptProviderHandle : SafeNCryptHandle
	{
		public SafeNCryptProviderHandle ()
		{
		}

		protected override bool ReleaseNativeHandle ()
		{
			return false;
		}
	}
    
    public sealed class SafeNCryptSecretHandle : SafeNCryptHandle
	{
		public SafeNCryptSecretHandle ()
		{
		}

		protected override bool ReleaseNativeHandle ()
		{
			return false;
		}
	}

#endif
}
