﻿using System;
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
        void SwitchCurrentCacheType(AuthCacheType authCacheType);
        bool IsAvailable();
    }

    class KeyManager : IKeyManager, IDisposable
    {
        private IKeyStorage _keyStorage;
        private readonly KeyCipher _keyCipher;
        private readonly IntPtr _keePassMainWindowHandle;

        private const int NoChanges = -777;
        private int _masterKeyTries = NoChanges;
        private CancellationTokenSource _cancellationTokenSource;

        private bool _notifiedAboutRdp = false;

        public int  KeysCount   { get { return _keyStorage.Count; } }

        public KeyManager(IntPtr windowHandle)
        {
            _keePassMainWindowHandle = windowHandle;
            _keyCipher = new KeyCipher(windowHandle);
            _keyStorage = KeyStorageFactory.Create();

            TryInit();
        }

        public bool IsAvailable()
        {
            try
            {
                var authCacheType = _keyCipher.AuthProvider.CurrentCacheType;
                Debug.Assert(authCacheType == Settings.Instance.GetAuthCacheType());
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void TryInit()
        {
            try
            {
                if (_keyCipher.AuthProvider.CurrentCacheType != Settings.Instance.GetAuthCacheType())
                {
                    // todo: something unexpected, messagebox
                    SwitchCurrentCacheType(AuthCacheType.Local);
                }
            }
            catch (AuthProviderException ex)
            {
                HandleAuthProviderException(ex);
            }
        }

        public void OnKeyPrompt(KeyPromptForm keyPromptForm, MainForm mainWindow)
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
                {
                    if (_cancellationTokenSource == null)
                        _cancellationTokenSource = new CancellationTokenSource();

                    Task.Factory.StartNew(() => MonitorWarning(_cancellationTokenSource.Token), _cancellationTokenSource.Token);

                    try
                    {
                        int masterKeyTries = KeePass.Program.Config.Security.MasterKeyTries;
                        Interlocked.Exchange(ref _masterKeyTries, masterKeyTries);
                        KeePass.Program.Config.Security.MasterKeyTries = ++masterKeyTries;
                        KeePass.Program.Config.Security.MasterKeyOnSecureDesktop = false;
                        SetCompositeKey(keyPromptForm, new CompositeKey());
                        CloseFormWithResult(keyPromptForm, DialogResult.OK);
                    }
                    catch
                    {
                        StopMonitorWarning();
                        throw;
                    }
                }
            }
            else
            {
                StopMonitorWarning();

                int oldTriesValue = Interlocked.Exchange(ref _masterKeyTries, NoChanges);
                if (oldTriesValue != NoChanges)
                {
                    KeePass.Program.Config.Security.MasterKeyTries = oldTriesValue;
                    KeePass.Program.Config.Security.MasterKeyOnSecureDesktop = true;
                }

                Unlock(keyPromptForm, dbPath);
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
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;
            }
        }

        private void MonitorWarning(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                const string MsgBoxClass = "#32770";
                var msgBox = Win32Window.Find(MsgBoxClass, "KeePass");
                // todo close only with "invalid key" message
                if (msgBox != null)
                {
                    msgBox.Close();
                    break;
                }
                Thread.Sleep(10);
            }
        }

        private void Unlock(KeyPromptForm keyPromptForm, string dbPath)
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
                if (Settings.Instance.RevokeOnCancel)
                    _keyStorage.Remove(dbPath);

                CloseFormWithResult(keyPromptForm, DialogResult.Cancel);
            }
            catch (AuthProviderException ex)
            {
                if (!HandleAuthProviderException(ex))
                    throw;
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
            catch (AuthProviderException ex)
            {
                if (!HandleAuthProviderException(ex))
                    throw;
            }
        }

        private bool HandleAuthProviderException(AuthProviderException ex)
        {
            if (ex is AuthProviderKeyNotFoundException)
            {
                // It's expected not to throw exceptions
                SwitchCurrentCacheType(AuthCacheType.Local);
                ErrorHandler.ShowError(ex, "Credential Manager storage has been turned off. Use Options dialog to turn it on.");
            }
            else if (ex is AuthProviderInvalidKeyException)
            {
                // The key might be compromised so we revoke all stored passwords
                // It's expected not to throw exceptions
                SwitchCurrentCacheType(AuthCacheType.Local);
                ErrorHandler.ShowError(ex,
                    "For security reasons Credential Manager storage has been turned off. Use Options dialog to turn it on.");
            }
            else if (ex is AuthProviderUserCancelledException)
            {
                // it's OK
            }
            else if (ex is AuthProviderIsUnavailableException)
            {
                // it's OK
            }
            else
            {
                return false;
            }

            return true;
        }

        public void RevokeAll()
        {
            _keyStorage.Clear();
        }

        public void SwitchCurrentCacheType(AuthCacheType authCacheType)
        {
            _keyCipher.AuthProvider.ClaimCurrentCacheType(authCacheType);
            _keyStorage.Clear();
            _keyStorage = KeyStorageFactory.Create(authCacheType);
            if (authCacheType == AuthCacheType.Local)
                Settings.Instance.WinStorageEnabled = false;
            // todo migrate
        }

        public void Dispose()
        {
            if (_cancellationTokenSource != null)
                _cancellationTokenSource.Dispose();
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

        private bool ExtractCompositeKey(string dbPath, out CompositeKey compositeKey)
        {
            compositeKey = null;

            if (String.IsNullOrEmpty(dbPath))
                return false;

            ProtectedKey encryptedData;
            if (!_keyStorage.TryGetValue(dbPath, out encryptedData))
                return false;

            using (AuthProviderUIContext.With(Settings.DecryptConfirmationMessage, _keePassMainWindowHandle))
                compositeKey = encryptedData.GetCompositeKey(_keyCipher);

            return true;
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
