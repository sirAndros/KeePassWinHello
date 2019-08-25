using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using KeePass.Forms;
using KeePassLib.Keys;
using KeePassLib.Serialization;
using KeePassWinHello.Utilities;

namespace KeePassWinHello
{
    interface IKeyManager
    {
        int KeysCount { get; }

        void RevokeAll();
        void ClaimCurrentCacheType(AuthCacheType authCacheType);
    }

    class KeyManager : IKeyManager
    {
        private readonly KeyCipher _keyCipher;
        private readonly IKeyStorage _keyStorage;
        private readonly IntPtr _keePassMainWindowHandle;

        public int  KeysCount   { get { return _keyStorage.Count; } }

        public KeyManager(IntPtr windowHandle)
        {
            _keePassMainWindowHandle = windowHandle;
            _keyCipher = new KeyCipher(windowHandle);
            _keyStorage = KeyStorageFactory.Create(_keyCipher.AuthProvider);
        }

        public void OnKeyPrompt(KeyPromptForm keyPromptForm, MainForm mainWindow)
        {
            if (!Settings.Instance.Enabled)
                return;

            string dbPath = GetDbPath(keyPromptForm);
            if (keyPromptForm.SecureDesktopMode)
            {
                if (IsKeyForDataBaseExist(dbPath))
                {
                    var dbFile = GetIoInfo(keyPromptForm);
                    CloseFormWithResult(keyPromptForm, DialogResult.Cancel);
                    Task.Factory.StartNew(() =>
                    {
                        KeePass.Program.Config.Security.MasterKeyOnSecureDesktop = false;
                        Thread.Yield();
                        ReOpenKeyPromptForm(mainWindow, dbFile);
                    })
                    .ContinueWith(_ =>
                    {
                        KeePass.Program.Config.Security.MasterKeyOnSecureDesktop = true;
                    });
                }
            }
            else
            {
                try
                {
                    CompositeKey compositeKey;
                    if (ExtractCompositeKey(dbPath, out compositeKey))
                    {
                        SetCompositeKey(keyPromptForm, compositeKey);
                        CloseFormWithResult(keyPromptForm, DialogResult.OK);
                    }
                }
                catch (AuthProviderUserCancelledException)
                {
                    CloseFormWithResult(keyPromptForm, DialogResult.Cancel);
                }
            }
        }

        public void OnDBClosing(object sender, FileClosingEventArgs e)
        {
            if (e == null)
            {
                Debug.Fail("Event is null");
                return;
            }

            if (e.Cancel || e.Database == null || e.Database.MasterKey == null || e.Database.IOConnectionInfo == null)
                return;

            string dbPath = e.Database.IOConnectionInfo.Path;
            if (!IsDBLocking(e) && Settings.Instance.GetAuthCacheType() == AuthCacheType.Local)
            {
                _keyStorage.Remove(dbPath);
            }
            else if (Settings.Instance.Enabled)
            {
                _keyStorage.AddOrUpdate(dbPath, ProtectedKey.Create(e.Database.MasterKey, _keyCipher));
            }
        }

        public void RevokeAll()
        {
            _keyStorage.Clear();
        }

        public void ClaimCurrentCacheType(AuthCacheType authCacheType)
        {
            try
            {
                _keyCipher.AuthProvider.ClaimCurrentCacheType(authCacheType);
            }
            catch (AuthProviderUserCancelledException)
            {
                if (authCacheType == AuthCacheType.Persistent)
                    Settings.Instance.WinStorageEnabled = false;

                MessageBox.Show(AuthProviderUIContext.Current, "[TBD] Canceled", Settings.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private static void CloseFormWithResult(KeyPromptForm keyPromptForm, DialogResult result)
        {
            // Remove flushing
            keyPromptForm.Visible = false;
            keyPromptForm.Opacity = 0;

            keyPromptForm.DialogResult = result;
            keyPromptForm.Close();
        }

        private static void ReOpenKeyPromptForm(MainForm mainWindow, IOConnectionInfo dbFile)
        {
            Action action = () => mainWindow.OpenDatabase(dbFile, null, false);
            mainWindow.Invoke(action);
        }

        private bool IsKeyForDataBaseExist(string dbPath)
        {
            return !String.IsNullOrEmpty(dbPath)
                && _keyStorage.ContainsKey(dbPath);
        }

        private bool ExtractCompositeKey(string dbPath, out CompositeKey compositeKey)
        {
            compositeKey = null;

            if (String.IsNullOrEmpty(dbPath))
                return false;

            ProtectedKey encryptedData;
            if (!_keyStorage.TryGetValue(dbPath, out encryptedData))
                return false;

            try
            {
                using (AuthProviderUIContext.With(Settings.DecryptConfirmationMessage, _keePassMainWindowHandle))
                {
                    compositeKey = encryptedData.GetCompositeKey(_keyCipher);
                    return true;
                }
            }
            catch (Exception)
            {
                _keyStorage.Remove(dbPath);
                throw;
            }
            return false;
        }

        private static void SetCompositeKey(KeyPromptForm keyPromptForm, CompositeKey compositeKey)
        {
            var fieldInfo = keyPromptForm.GetType().GetField("m_pKey", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fieldInfo != null)
                fieldInfo.SetValue(keyPromptForm, compositeKey);
        }

        private static bool IsDBLocking(FileClosingEventArgs e)
        {
            try
            {
                var FlagsProperty = typeof(FileClosingEventArgs).GetProperty("Flags");
                if (FlagsProperty == null)
                    return true;

                var FlagsType = FlagsProperty.PropertyType;
                int FlagsValue = Convert.ToInt32(FlagsProperty.GetValue(e, null));

                var names = Enum.GetNames(FlagsType);
                for (int i = 0; i != names.Length; ++i)
                {
                    if (names[i] == "Locking")
                    {
                        int Locking = Convert.ToInt32(Enum.GetValues(FlagsType).GetValue(i));
                        if ((FlagsValue & Locking) != Locking)
                        {
                            return false;
                        }
                        break;
                    }
                }
            }
            catch { }
            return true;
        }

        private static string GetDbPath(KeyPromptForm keyPromptForm)
        {
            var ioInfo = GetIoInfo(keyPromptForm);
            if (ioInfo == null)
                return null;
            return ioInfo.Path;
        }

        private static IOConnectionInfo GetIoInfo(KeyPromptForm keyPromptForm)
        {
            var fieldInfo = keyPromptForm.GetType().GetField("m_ioInfo", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fieldInfo == null)
                return null;
            return fieldInfo.GetValue(keyPromptForm) as IOConnectionInfo;
        }
    }
}
