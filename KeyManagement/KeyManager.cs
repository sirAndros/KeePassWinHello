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

namespace KeePassWinHello
{
    class KeyManager
    {
        private readonly KeyCipher _keyCipher;
        private readonly KeyStorage _keyStorage;
        private volatile bool _isSecureDesktopSettingChanged = false;

        public KeyManager(IntPtr windowHandle)
        {
            _keyStorage = new KeyStorage();
            _keyCipher = new KeyCipher(Settings.ConfirmationMessage, windowHandle);
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
                        _isSecureDesktopSettingChanged = true;
                        Thread.Yield();
                        ReOpenKeyPromptForm(mainWindow, dbFile);
                    })
                    .ContinueWith(_ =>
                    {
                        KeePass.Program.Config.Security.MasterKeyOnSecureDesktop = true;
                        _isSecureDesktopSettingChanged = false;
                    });
                }
            }
            else
            {
                CompositeKey compositeKey;
                if (ExtractCompositeKey(dbPath, out compositeKey))
                {
                    SetCompositeKey(keyPromptForm, compositeKey);
                    CloseFormWithResult(keyPromptForm, DialogResult.OK);
                }
                else if (_isSecureDesktopSettingChanged)    // can be here only from recursive call. No extra sync needed.
                {
                    var dbFile = GetIoInfo(keyPromptForm);
                    CloseFormWithResult(keyPromptForm, DialogResult.Cancel);
                    Task.Factory.StartNew(() => ReOpenKeyPromptForm(mainWindow, dbFile));
                }
            }
        }

        public void OnOptionsLoad(OptionsForm optionsForm)
        {
            OptionsPanel.AddTab(GetTabControl(optionsForm), GetTabsImageList(optionsForm), _keyCipher.IsAvailable);
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
            if (!IsDBLocking(e))
            {
                _keyStorage.Remove(dbPath);
            }
            else if (AuthProviderFactory.IsAvailable() && Settings.Instance.Enabled)
            {
                _keyStorage.AddOrUpdate(dbPath, ProtectedKey.Create(e.Database.MasterKey, _keyCipher));
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
                compositeKey = encryptedData.GetCompositeKey(_keyCipher);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                _keyStorage.Remove(dbPath);
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.ToString()); // TODO: fix canceled exception
                _keyStorage.Remove(dbPath);
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

        private static TabControl GetTabControl(OptionsForm optionsForm)
        {
            return optionsForm.Controls.Find("m_tabMain", true).FirstOrDefault() as TabControl;
        }

        private static ImageList GetTabsImageList(OptionsForm optionsForm)
        {
            var m_ilIconsField = optionsForm.GetType().GetField("m_ilIcons", BindingFlags.Instance | BindingFlags.NonPublic);
            if (m_ilIconsField == null)
                return null;
            return m_ilIconsField.GetValue(optionsForm) as ImageList;
        }
    }
}
