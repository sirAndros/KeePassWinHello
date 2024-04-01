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
        private UIContextManager _uiContextManager;
        private IDisposable _uiContext;
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

            _uiContextManager = new UIContextManager();
            _uiContext = _uiContextManager.PushContext("TBD", host.MainWindow);

            Settings.Instance.Initialize(host.CustomConfig, _uiContextManager);

            var mainDesktop = WinAPI.GetThreadDesktop(WinAPI.GetCurrentThreadId());
            _keyManagerProvider = new KeyManagerProvider(mainDesktop, _uiContextManager);

            _host = host;
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

            _uiContext.Dispose();
            _uiContext = null;
            _uiContextManager = null;

            _host = null;
        }

        private void OnPreFileClosing(object sender, FileClosingEventArgs e)
        {
            try
            {
                var keyManager = _keyManagerProvider.ObtainKeyManager();
                if (keyManager != null)
                    keyManager.OnDBClosing(sender, e);
            }
            catch (Exception ex)
            {
                _uiContextManager.CurrentContext.ShowError(ex);
            }
        }

        private void OnWindowAdded(object sender, GwmWindowEventArgs e)
        {
            try
            {
                var keyPromptForm = e.Form as KeyPromptForm;
                if (keyPromptForm != null)
                {
                    var keyManager = _keyManagerProvider.ObtainKeyManager();
                    if (keyManager != null)
                    {
                        using (_uiContextManager.PushContext("Unlocking a database", keyPromptForm))
                        {
                            lock (_unlockMutex)
                                keyManager.OnKeyPrompt(keyPromptForm);
                            return;
                        }
                    }
                }

                var optionsForm = e.Form as OptionsForm;
                if (optionsForm != null)
                {
                    var keyManager = _keyManagerProvider.ObtainKeyManager();
                    using (_uiContextManager.PushContext("Modifying KeePass settings", optionsForm))
                    {
                        OptionsPanel.OnOptionsLoad(optionsForm, keyManager, _uiContextManager);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                _uiContextManager.CurrentContext.ShowError(ex);
            }
        }
    }
}
