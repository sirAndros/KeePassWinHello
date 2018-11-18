using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using KeePass.Forms;
using KeePassLib.Keys;
using KeePassLib.Security;
using KeePassLib.Serialization;
using KeePassLib.Utility;

namespace WinHelloQuickUnlock
{
    class KeyManager
    {
        private readonly KeyCipher _keyCipher;
        private readonly KeyStorage _keyStorage;

        public KeyManager(IntPtr windowHandle)
        {
            _keyStorage = new KeyStorage();
            _keyCipher = new KeyCipher(Settings.ConfirmationMessage, windowHandle);
        }

        public void OnKeyPrompt(KeyPromptForm keyPromptForm)
        {
            if (keyPromptForm.SecureDesktopMode)
                return;

            CompositeKey encryptedData;
            var dbPath = GetDbPath(keyPromptForm);
            if (!FindQuickUnlockData(dbPath, out encryptedData))
                return;

            var compositeKey = GetCompositeKey(encryptedData);
            if (compositeKey != null)
            {
                SetCompositeKey(keyPromptForm, compositeKey);
                keyPromptForm.DialogResult = DialogResult.OK;
                keyPromptForm.Close(); 
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
            if (!e.Flags.HasFlag(FileEventFlags.Locking))
            {
                _keyStorage.Remove(dbPath);
            }
            else if (WinHelloCryptProvider.IsAvailable())
            {
                _keyStorage.AddOrUpdate(dbPath, _keyCipher.Protect(e.Database.MasterKey));
            }
        }

        private void SetCompositeKey(KeyPromptForm keyPromptForm, CompositeKey compositeKey)
        {
            var fieldInfo = keyPromptForm.GetType().GetField("m_pKey", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fieldInfo != null)
                fieldInfo.SetValue(keyPromptForm, compositeKey);
        }

        private CompositeKey GetCompositeKey(CompositeKey encryptedData)
        {
            return _keyCipher.UnProtect(encryptedData);
        }

        private bool FindQuickUnlockData(string dbPath, out CompositeKey encryptedData)
        {
            if (String.IsNullOrEmpty(dbPath))
            {
                encryptedData = null;
                return false;
            }

            return _keyStorage.TryGetValue(dbPath, out encryptedData);
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
