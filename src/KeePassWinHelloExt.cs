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
        private KeyManager _keyManager;
        private bool _wasUnavailable;
        private readonly object _initMutex = new Object();
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

            InitKeyManager();

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

            if (_keyManager != null)
            {
                lock (_initMutex)
                {
                    _keyManager.Dispose();
                    _keyManager = null;
                }
            }

            _host = null;
        }

        private void InitKeyManager()
        {
            lock (_initMutex)
            {
                try
                {
                    _keyManager = new KeyManager(_host.MainWindow.Handle);
                    _wasUnavailable = false;
                }
                catch (AuthProviderIsUnavailableException)
                {
                    _wasUnavailable = true;
                }
                catch (Exception ex)
                {
                    ErrorHandler.ShowError(ex);
                }
            }
        }

        private void OnPreFileClosing(object sender, FileClosingEventArgs e)
        {
            if (_wasUnavailable)
                InitKeyManager();

            if (_keyManager != null)
                _keyManager.OnDBClosing(sender, e);
        }

        private void OnWindowAdded(object sender, GwmWindowEventArgs e)
        {
            try
            {
                var keyPromptForm = e.Form as KeyPromptForm;
                if (keyPromptForm != null)
                {
                    if (_wasUnavailable)
                        InitKeyManager();

                    if (_keyManager != null)
                    {
                        lock (_unlockMutex)
                            _keyManager.OnKeyPrompt(keyPromptForm, _host.MainWindow);
                        return; 
                    }
                }

                var optionsForm = e.Form as OptionsForm;
                if (optionsForm != null)
                {
                    if (_wasUnavailable)
                        InitKeyManager();

                    OptionsPanel.OnOptionsLoad(optionsForm, _keyManager);
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
