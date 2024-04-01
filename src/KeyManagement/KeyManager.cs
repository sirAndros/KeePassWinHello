using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
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

    class KeyManager : IKeyManager, IDisposable
    {
        private IKeyStorage _keyStorage;
        private readonly KeyCipher _keyCipher;
        private const int NoChanges = -777;
        private int _masterKeyTries = NoChanges;
        private IDisposable _warningSuppresser;

        private bool _notifiedAboutRdp = false;

        public int KeysCount { get { return _keyStorage.Count; } }

        public KeyManager(IWin32Window parentWindow)
        {
            _keyCipher = new KeyCipher(parentWindow);
            _keyStorage = KeyStorageFactory.Create(_keyCipher.AuthProvider);
        }

        public void OnKeyPrompt(KeyPromptForm keyPromptForm, HDESK mainDesktop)
        {
            if (!Settings.Instance.Enabled)
                return;

            if (SystemInformation.TerminalServerSession) // RDP
            {
                if (!_notifiedAboutRdp)
                {
                    MessageBox.Show(AuthProviderUIContext.Current,
                        "Windows Hello is not available for a remote session. Unfortunately, you are forced to enter database using default authorization.\n" +
                        "The key will be kept in case you prompt to enter without using RDP. The usual key retention settings are being applied.",
                        Settings.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _notifiedAboutRdp = true;
                }
                return;
            }
            else
            {
                _notifiedAboutRdp = false;
            }

            string dbPath = GetDbPath(keyPromptForm);
            if (keyPromptForm.SecureDesktopMode)
            {
                if (IsKeyForDataBaseExist(dbPath))
                    RestartPromptOnMainDesktopAndSuppressWarning(keyPromptForm, mainDesktop);
            }
            else
            {
                StopMonitorWarning();
                RestoreSecureDesktopSettings();
                Unlock(keyPromptForm, dbPath);
            }
        }

        private void RestartPromptOnMainDesktopAndSuppressWarning(KeyPromptForm keyPromptForm, HDESK mainDesktop)
        {
            IDisposable warningSuppresser = null;
            if (Volatile.Read(ref _warningSuppresser) == null)
                warningSuppresser = KeePassWarningSuppresser.SuppressAllWarningWindows(mainDesktop);

            try
            {
                RestartPromptOnMainDesktop(keyPromptForm);

                //continue supression until the next prompt appears
                if (Interlocked.CompareExchange(ref _warningSuppresser, warningSuppresser, null) == null)
                    warningSuppresser = null;
            }
            finally
            {
                if (warningSuppresser != null)
                    warningSuppresser.Dispose();
            }
        }

        private void RestartPromptOnMainDesktop(KeyPromptForm keyPromptForm)
        {
            int masterKeyTries = KeePass.Program.Config.Security.MasterKeyTries;
            Interlocked.Exchange(ref _masterKeyTries, masterKeyTries);
            KeePass.Program.Config.Security.MasterKeyTries = ++masterKeyTries;
            KeePass.Program.Config.Security.MasterKeyOnSecureDesktop = false;

            SetCompositeKey(keyPromptForm, new CompositeKey());
            CloseFormWithResult(keyPromptForm, DialogResult.OK);
        }

        private void RestoreSecureDesktopSettings()
        {
            int oldTriesValue = Interlocked.Exchange(ref _masterKeyTries, NoChanges);
            if (oldTriesValue != NoChanges)
            {
                KeePass.Program.Config.Security.MasterKeyTries = oldTriesValue;
                KeePass.Program.Config.Security.MasterKeyOnSecureDesktop = true;
            }
        }

        public void OnDBClosing(object sender, FileClosingEventArgs e)
        {
            if (e == null)
            {
                Debug.Fail("Event is null");
                return;
            }

            if (SystemInformation.TerminalServerSession) // RDP
                return;

            if (e.Cancel || e.Database == null || e.Database.IOConnectionInfo == null)
                return;

            var databaseMasterKey = e.Database.MasterKey;
            if (databaseMasterKey == null)
                return;

            Lock(IsDBLocking(e), e.Database.IOConnectionInfo.Path, databaseMasterKey);
        }

        private void StopMonitorWarning()
        {
            using (_warningSuppresser)
                _warningSuppresser = null;
        }

        private void Unlock(KeyPromptForm keyPromptForm, string dbPath)
        {
            try
            {
                CompositeKey compositeKey;
                if (ExtractCompositeKey(dbPath, keyPromptForm.Handle, out compositeKey))
                {
                    SetCompositeKey(keyPromptForm, compositeKey);
                    CloseFormWithResult(keyPromptForm, DialogResult.OK);
                }
            }
            catch (AuthProviderKeyNotFoundException ex)
            {
                // It's expected not to throw exceptions
                ClaimCurrentCacheType(AuthCacheType.Local);
                ErrorHandler.ShowError(ex, "Credential Manager storage has been turned off. Use Options dialog to turn it on.");
                CloseFormWithResult(keyPromptForm, DialogResult.Cancel);
            }
            catch (AuthProviderUserCancelledException)
            {
                CloseFormWithResult(keyPromptForm, DialogResult.Cancel);
            }
        }

        private void Lock(bool isDbLocking, string dbPath, CompositeKey databaseMasterKey)
        {
            try
            {
                if (!isDbLocking && Settings.Instance.GetAuthCacheType() == AuthCacheType.Local)
                {
                    _keyStorage.Remove(dbPath);
                }
                else if (Settings.Instance.Enabled)
                {
                    _keyStorage.AddOrUpdate(dbPath, ProtectedKey.Create(databaseMasterKey, _keyCipher));
                }
            }
            catch (AuthProviderKeyNotFoundException ex)
            {
                // It's expected not to throw exceptions
                ClaimCurrentCacheType(AuthCacheType.Local);
                ErrorHandler.ShowError(ex, "Credential Manager storage has been turned off. Use Options dialog to turn it on.");
            }
            catch (AuthProviderInvalidKeyException ex)
            {
                // It's expected not to throw exceptions
                ClaimCurrentCacheType(AuthCacheType.Local);
                ErrorHandler.ShowError(ex,
                    "For security reasons Credential Manager storage has been turned off. Use Options dialog to turn it on.");
            }
            catch (AuthProviderUserCancelledException)
            {
                // it's OK
            }
        }

        public void RevokeAll()
        {
            _keyStorage.Clear();
        }

        public void ClaimCurrentCacheType(AuthCacheType authCacheType)
        {
            _keyCipher.AuthProvider.ClaimCurrentCacheType(authCacheType);
            _keyStorage.Clear();
            _keyStorage = KeyStorageFactory.Create(_keyCipher.AuthProvider);
            if (authCacheType == AuthCacheType.Local)
                Settings.Instance.WinStorageEnabled = false;
            // todo migrate
        }

        public void Dispose()
        {
            if (_warningSuppresser != null)
                _warningSuppresser.Dispose();
        }

        private static void CloseFormWithResult(KeyPromptForm keyPromptForm, DialogResult result)
        {
            // Remove flushing
            keyPromptForm.Visible = false;
            keyPromptForm.Opacity = 0;

            keyPromptForm.DialogResult = result;
            keyPromptForm.Close();
        }

        private bool IsKeyForDataBaseExist(string dbPath)
        {
            return !String.IsNullOrEmpty(dbPath)
                && _keyStorage.ContainsKey(dbPath);
        }

        private bool ExtractCompositeKey(string dbPath, IntPtr keePassWindowHandle, out CompositeKey compositeKey)
        {
            compositeKey = null;

            if (String.IsNullOrEmpty(dbPath))
                return false;

            ProtectedKey encryptedData;
            if (!_keyStorage.TryGetValue(dbPath, out encryptedData))
                return false;

            try
            {
                using (AuthProviderUIContext.With(Settings.DecryptConfirmationMessage, keePassWindowHandle))
                {
                    compositeKey = encryptedData.GetCompositeKey(_keyCipher);
                    return true;
                }
            }
            catch (AuthProviderInvalidKeyException)
            {
                // The key might be compromised so we revoke all stored passwords
                ClaimCurrentCacheType(AuthCacheType.Local);
                throw;
            }
            catch (AuthProviderUserCancelledException)
            {
                if (Settings.Instance.RevokeOnCancel)
                    _keyStorage.Remove(dbPath);
                throw;
            }
            catch (Exception)
            {
                _keyStorage.Remove(dbPath);
                throw;
            }
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
