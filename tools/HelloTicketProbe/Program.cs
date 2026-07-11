using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;

namespace HelloTicketProbe
{
    // Standalone repro of KeePassWinHello's NGC key usage, to diagnose
    // WINBIO_E_INVALID_TICKET (0x80098044) on face auth without KeePass in the loop.
    internal static class Program
    {
        private const string MS_NGC_KEY_STORAGE_PROVIDER = "Microsoft Passport Key Storage Provider";
        private const string NCRYPT_WINDOW_HANDLE_PROPERTY = "HWND Handle";
        private const string NCRYPT_USE_CONTEXT_PROPERTY = "Use Context";
        private const string NCRYPT_LENGTH_PROPERTY = "Length";
        private const string NCRYPT_KEY_USAGE_PROPERTY = "Key Usage";
        private const string NCRYPT_NGC_CACHE_TYPE_PROPERTY = "NgcCacheType";
        private const string NCRYPT_PIN_CACHE_IS_GESTURE_REQUIRED_PROPERTY = "PinCacheIsGestureRequired";
        private const string BCRYPT_RSA_ALGORITHM = "RSA";
        private const int NCRYPT_NGC_CACHE_TYPE_PROPERTY_AUTH_MANDATORY_FLAG = 0x00000001;
        private const int NCRYPT_ALLOW_DECRYPT_FLAG = 0x00000001;
        private const int NCRYPT_ALLOW_SIGNING_FLAG = 0x00000002;
        private const int NCRYPT_PAD_PKCS1_FLAG = 0x00000002;
        private const int NCRYPT_OVERWRITE_KEY_FLAG = 0x00000080;
        private const int NCRYPT_SILENT_FLAG = 0x00000040;
        private const int NTE_NO_KEY = unchecked((int)0x8009000D);
        private const int NTE_USER_CANCELLED = unchecked((int)0x80090036);
        private const int WINBIO_E_INVALID_TICKET = unchecked((int)0x80098044);

        [DllImport("cryptngc.dll", CharSet = CharSet.Unicode)]
        private static extern int NgcGetDefaultDecryptionKeyName(string pszSid, int dwReserved1, int dwReserved2, out string ppszKeyName);

        [DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
        private static extern int NCryptOpenStorageProvider(out IntPtr phProvider, string pszProviderName, int dwFlags);

        [DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
        private static extern int NCryptOpenKey(IntPtr hProvider, out IntPtr phKey, string pszKeyName, int dwLegacyKeySpec, int dwFlags);

        [DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
        private static extern int NCryptCreatePersistedKey(IntPtr hProvider, out IntPtr phKey, string pszAlgId, string pszKeyName, int dwLegacyKeySpec, int dwFlags);

        [DllImport("ncrypt.dll")]
        private static extern int NCryptFinalizeKey(IntPtr hKey, int dwFlags);

        [DllImport("ncrypt.dll")]
        private static extern int NCryptDeleteKey(IntPtr hKey, int flags);

        [DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
        private static extern int NCryptSetProperty(IntPtr hObject, string pszProperty, byte[] pbInput, int cbInput, int dwFlags);

        [DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
        private static extern int NCryptSetProperty(IntPtr hObject, string pszProperty, string pbInput, int cbInput, int dwFlags);

        [DllImport("ncrypt.dll")]
        private static extern int NCryptEncrypt(IntPtr hKey, byte[] pbInput, int cbInput, IntPtr pvPaddingZero, byte[] pbOutput, int cbOutput, out int pcbResult, int dwFlags);

        [DllImport("ncrypt.dll")]
        private static extern int NCryptDecrypt(IntPtr hKey, byte[] pbInput, int cbInput, IntPtr pvPaddingZero, byte[] pbOutput, int cbOutput, out int pcbResult, int dwFlags);

        [DllImport("ncrypt.dll")]
        private static extern int NCryptFreeObject(IntPtr hObject);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        private static string _logPath;
        private static string _sid;

        private static void Log(string message)
        {
            var line = string.Format("{0:HH:mm:ss.fff} {1}", DateTime.Now, message);
            Console.WriteLine(line);
            try { File.AppendAllText(_logPath, line + Environment.NewLine); } catch { }
        }

        private static string Hr(int hr)
        {
            return hr == 0 ? "OK" : string.Format("0x{0:X8}", hr);
        }

        private static int Main()
        {
            _logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "probe-log.txt");
            _sid = WindowsIdentity.GetCurrent().User.ToString();

            Log("==================================================================");
            Log(string.Format("Probe start. OS build: {0}. Log: {1}", Environment.OSVersion.Version, _logPath));

            string defaultKey;
            int hr = NgcGetDefaultDecryptionKeyName(_sid, 0, 0, out defaultKey);
            Log(string.Format("NgcGetDefaultDecryptionKeyName: {0}, key present: {1}", Hr(hr), !string.IsNullOrEmpty(defaultKey)));
            if (string.IsNullOrEmpty(defaultKey))
            {
                Log("Windows Hello is not available for this account. Aborting.");
                return 1;
            }

            Console.WriteLine();
            Console.WriteLine("IMPORTANT: for a meaningful test, run this right after signing into");
            Console.WriteLine("Windows with FACE, before any PIN has been entered in this session.");
            Console.WriteLine();

            // Same name construction as WinHelloProvider: SID//Domain/SubDomain/Name
            string pluginKeyName = _sid + "//KeePassWinHello//KeePassWinHello";
            RunPhase("PHASE 1: plugin's existing persistent key", pluginKeyName, createFresh: false);

            string probeKeyName = _sid + "//KPWHProbe//KPWHProbe";
            RunPhase("PHASE 2: freshly created key", probeKeyName, createFresh: true);

            Console.WriteLine();
            Log("Probe finished. Send probe-log.txt back for analysis.");
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey(true);
            return 0;
        }

        private static void RunPhase(string title, string keyName, bool createFresh)
        {
            Console.WriteLine();
            Log("---- " + title + " ----");
            Log("Key name: " + keyName);

            Console.WriteLine("Press Y to run this phase, any other key to skip.");
            if (char.ToUpperInvariant(Console.ReadKey(true).KeyChar) != 'Y')
            {
                Log("Phase skipped by user.");
                return;
            }

            IntPtr provider;
            int hr = NCryptOpenStorageProvider(out provider, MS_NGC_KEY_STORAGE_PROVIDER, 0);
            Log("NCryptOpenStorageProvider: " + Hr(hr));
            if (hr != 0) return;

            try
            {
                if (createFresh)
                {
                    if (!CreateKey(provider, keyName))
                        return;
                }

                byte[] plain = Encoding.ASCII.GetBytes("HelloTicketProbe-test-payload-01");
                byte[] cipher = EncryptBlob(provider, keyName, plain);
                if (cipher == null) return;

                // Attempt 1: expect the user to authenticate with FACE.
                Console.WriteLine();
                Console.WriteLine(">>> A Windows Hello prompt will appear. Use FACE (let it scan).");
                Console.WriteLine(">>> Press any key to show the prompt...");
                Console.ReadKey(true);
                int hrFace = DecryptBlob(provider, keyName, cipher, plain, "face attempt");

                if (hrFace == WINBIO_E_INVALID_TICKET)
                {
                    // Attempt 2: same call, but the user picks PIN in the dialog.
                    Console.WriteLine();
                    Console.WriteLine(">>> Prompt again. This time click 'More choices' and select PIN.");
                    Console.WriteLine(">>> Press any key to show the prompt...");
                    Console.ReadKey(true);
                    int hrPin = DecryptBlob(provider, keyName, cipher, plain, "PIN attempt");

                    if (hrPin == 0)
                    {
                        // Did the PIN repair the session for biometrics?
                        Console.WriteLine();
                        Console.WriteLine(">>> Prompt once more. Use FACE again to see if it now works.");
                        Console.WriteLine(">>> Press any key to show the prompt...");
                        Console.ReadKey(true);
                        DecryptBlob(provider, keyName, cipher, plain, "face after PIN");
                    }
                }
            }
            finally
            {
                if (createFresh)
                    DeleteKey(provider, keyName);
                NCryptFreeObject(provider);
            }
        }

        private static bool CreateKey(IntPtr provider, string keyName)
        {
            IntPtr key;
            int hr = NCryptCreatePersistedKey(provider, out key, BCRYPT_RSA_ALGORITHM, keyName, 0, NCRYPT_OVERWRITE_KEY_FLAG);
            Log("NCryptCreatePersistedKey: " + Hr(hr));
            if (hr != 0) return false;

            byte[] length = BitConverter.GetBytes(2048);
            hr = NCryptSetProperty(key, NCRYPT_LENGTH_PROPERTY, length, length.Length, 0);
            Log("  set Length: " + Hr(hr));

            byte[] usage = BitConverter.GetBytes(NCRYPT_ALLOW_DECRYPT_FLAG | NCRYPT_ALLOW_SIGNING_FLAG);
            hr = NCryptSetProperty(key, NCRYPT_KEY_USAGE_PROPERTY, usage, usage.Length, 0);
            Log("  set Key Usage: " + Hr(hr));

            byte[] cacheType = BitConverter.GetBytes(NCRYPT_NGC_CACHE_TYPE_PROPERTY_AUTH_MANDATORY_FLAG);
            hr = NCryptSetProperty(key, NCRYPT_NGC_CACHE_TYPE_PROPERTY, cacheType, cacheType.Length, 0);
            Log("  set NgcCacheType: " + Hr(hr));

            ApplyUIContext(key, "HelloTicketProbe: creating test key");

            Console.WriteLine(">>> Finalizing the key may show a Windows Hello prompt. Authenticate however you like.");
            hr = NCryptFinalizeKey(key, 0);
            Log("NCryptFinalizeKey: " + Hr(hr));
            NCryptFreeObject(key);
            return hr == 0;
        }

        private static void DeleteKey(IntPtr provider, string keyName)
        {
            IntPtr key;
            int hr = NCryptOpenKey(provider, out key, keyName, 0, 0);
            if (hr != 0) { Log("cleanup NCryptOpenKey: " + Hr(hr)); return; }
            hr = NCryptDeleteKey(key, 0);
            Log("cleanup NCryptDeleteKey: " + Hr(hr));
            if (hr != 0) NCryptFreeObject(key);
        }

        private static byte[] EncryptBlob(IntPtr provider, string keyName, byte[] plain)
        {
            IntPtr key;
            int hr = NCryptOpenKey(provider, out key, keyName, 0, NCRYPT_SILENT_FLAG);
            Log("NCryptOpenKey (silent, for encrypt): " + Hr(hr) + (hr == NTE_NO_KEY ? " [key does not exist]" : ""));
            if (hr != 0) return null;

            try
            {
                int cb;
                hr = NCryptEncrypt(key, plain, plain.Length, IntPtr.Zero, null, 0, out cb, NCRYPT_PAD_PKCS1_FLAG);
                if (hr != 0) { Log("NCryptEncrypt (size): " + Hr(hr)); return null; }
                byte[] cipher = new byte[cb];
                hr = NCryptEncrypt(key, plain, plain.Length, IntPtr.Zero, cipher, cipher.Length, out cb, NCRYPT_PAD_PKCS1_FLAG);
                Log("NCryptEncrypt: " + Hr(hr));
                return hr == 0 ? cipher : null;
            }
            finally
            {
                NCryptFreeObject(key);
            }
        }

        private static int DecryptBlob(IntPtr provider, string keyName, byte[] cipher, byte[] expectedPlain, string label)
        {
            IntPtr key;
            int hr = NCryptOpenKey(provider, out key, keyName, 0, 0);
            Log(string.Format("NCryptOpenKey ({0}): {1}", label, Hr(hr)));
            if (hr != 0) return hr;

            try
            {
                ApplyUIContext(key, "HelloTicketProbe: " + label);

                byte[] gestureRequired = BitConverter.GetBytes(1);
                hr = NCryptSetProperty(key, NCRYPT_PIN_CACHE_IS_GESTURE_REQUIRED_PROPERTY, gestureRequired, gestureRequired.Length, 0);
                Log("  set PinCacheIsGestureRequired: " + Hr(hr));

                byte[] output = new byte[cipher.Length];
                int cb;
                hr = NCryptDecrypt(key, cipher, cipher.Length, IntPtr.Zero, output, output.Length, out cb, NCRYPT_PAD_PKCS1_FLAG);
                bool roundTrip = false;
                if (hr == 0 && cb == expectedPlain.Length)
                {
                    roundTrip = true;
                    for (int i = 0; i < cb; ++i)
                        if (output[i] != expectedPlain[i]) { roundTrip = false; break; }
                }
                Log(string.Format("NCryptDecrypt ({0}): {1}{2}", label, Hr(hr),
                    hr == 0 ? (roundTrip ? " [payload verified]" : " [PAYLOAD MISMATCH]")
                    : hr == WINBIO_E_INVALID_TICKET ? " [WINBIO_E_INVALID_TICKET]"
                    : hr == NTE_USER_CANCELLED ? " [user cancelled]" : ""));
                return hr;
            }
            finally
            {
                NCryptFreeObject(key);
            }
        }

        private static void ApplyUIContext(IntPtr key, string message)
        {
            IntPtr hwnd = GetConsoleWindow();
            if (hwnd != IntPtr.Zero)
            {
                byte[] handle = BitConverter.GetBytes(IntPtr.Size == 8 ? hwnd.ToInt64() : hwnd.ToInt32());
                NCryptSetProperty(key, NCRYPT_WINDOW_HANDLE_PROPERTY, handle, handle.Length, 0);
            }
            NCryptSetProperty(key, NCRYPT_USE_CONTEXT_PROPERTY, message, (message.Length + 1) * 2, 0);
        }
    }
}
