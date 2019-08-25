using System;
using System.Diagnostics;
using System.Drawing;
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

        public override Image SmallIcon
        {
            get
            {
                return UIUtil.GetSmallIconSize().Height > 16
                    ? Properties.Resources.KPWH_32x32
                    : Properties.Resources.KPWH_16x16;
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
            try
            {
                _keyManager = new KeyManager(_host.MainWindow.Handle);
                _host.MainWindow.FileClosingPre += _keyManager.OnDBClosing;
            }
            catch (AuthProviderIsUnavailableException)
            {
                // it's OK.
            }
            catch (Exception ex)
            {
                _keyManager = null;
                ErrorHandler.ShowError(ex);
            }
 
            GlobalWindowManager.WindowAdded += OnWindowAdded;

            return true;
        }

        public override void Terminate()
        {
            if (_host == null)
                return;

            GlobalWindowManager.WindowAdded -= OnWindowAdded;
            if (_keyManager != null)
                _host.MainWindow.FileClosingPre -= _keyManager.OnDBClosing;

            _host = null;
        }

        private void OnWindowAdded(object sender, GwmWindowEventArgs e)
        {
            try
            {
                var keyPromptForm = e.Form as KeyPromptForm;
                if (keyPromptForm != null && _keyManager != null)
                {
                    _keyManager.OnKeyPrompt(keyPromptForm, _host.MainWindow);
                    return;
                }

                var optionsForm = e.Form as OptionsForm;
                if (optionsForm != null)
                {
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
