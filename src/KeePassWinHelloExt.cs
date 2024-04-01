using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using KeePass.Forms;
using KeePass.Plugins;
using KeePass.UI;
using KeePassWinHello.Utilities;

namespace KeePassWinHello
{
    public class KeePassWinHelloExt : Plugin
    {
        private IPluginHost _host;
        private KeyManagerProvider _keyManagerProvider;
        private readonly object _unlockMutex = new Object();

        public override Image SmallIcon
        {
            get
            {
                try
                {
                    var GetSmallIconSize = typeof(UIUtil).GetMethod("GetSmallIconSize", BindingFlags.Public | BindingFlags.Static);
                    var size = GetSmallIconSize != null ? (Size)GetSmallIconSize.Invoke(null, null) : new Size(16, 16);

                    return size.Height > 16
                        ? Properties.Resources.KPWH_32x32
                        : Properties.Resources.KPWH_16x16;
                }
                catch (Exception)
                {
                    return Properties.Resources.KPWH_16x16;
                }
            }
        }

        public override string UpdateUrl
        {
            get { return "https://github.com/sirAndros/KeePassWinHello/raw/master/keepass.version"; }
        }

        public override bool Initialize(IPluginHost host)
        {
            if (_host != null) { Debug.Assert(false); Terminate(); }
            if (host == null) { return false; }

            Settings.Instance.Initialize(host.CustomConfig);

            _host = host;
            _keyManagerProvider = new KeyManagerProvider(host);

            _host.MainWindow.FileClosingPre += OnPreFileClosing;
            GlobalWindowManager.WindowAdded += OnWindowAdded;

            return true;
        }

        public override void Terminate()
        {
            if (_host == null)
                return;

            GlobalWindowManager.WindowAdded -= OnWindowAdded;
            _host.MainWindow.FileClosingPre -= OnPreFileClosing;

            _keyManagerProvider.Dispose();
            _keyManagerProvider = null;

            _host = null;
        }

        private void OnPreFileClosing(object sender, FileClosingEventArgs e)
        {
            try
            {
                var keyManager = _keyManagerProvider.ObtainKeyManager(_host.MainWindow);
                if (keyManager != null)
                    keyManager.OnDBClosing(sender, e);
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(ex);
            }
        }

        private void OnWindowAdded(object sender, GwmWindowEventArgs e)
        {
            try
            {
                var keyPromptForm = e.Form as KeyPromptForm;
                if (keyPromptForm != null)
                {
                    var keyManager = _keyManagerProvider.ObtainKeyManager(keyPromptForm);
                    if (keyManager != null)
                    {
                        lock (_unlockMutex)
                            keyManager.OnKeyPrompt(keyPromptForm);
                        return; 
                    }
                }

                var optionsForm = e.Form as OptionsForm;
                if (optionsForm != null)
                {
                    var keyManager = _keyManagerProvider.ObtainKeyManager(optionsForm);
                    OptionsPanel.OnOptionsLoad(optionsForm, keyManager);
                    return;
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(ex);
            }
        }
    }
}
