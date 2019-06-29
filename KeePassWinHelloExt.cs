using System.Diagnostics;
using System.Drawing;
using KeePass.Forms;
using KeePass.Plugins;
using KeePass.UI;

namespace KeePassWinHello
{
    public class KeePassWinHelloExt : Plugin
    {
        private IPluginHost _host;
        private KeyManager _keyManager;

        public override Image SmallIcon
        {
            get { return Properties.Resources.windows_hello16x16; }
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
            _keyManager = new KeyManager(_host.MainWindow.Handle);

            _host.MainWindow.FileClosingPre += _keyManager.OnDBClosing;
            GlobalWindowManager.WindowAdded += OnWindowAdded;

            return true;
        }

        public override void Terminate()
        {
            if (_host == null)
                return;

            GlobalWindowManager.WindowAdded -= OnWindowAdded;
            _host.MainWindow.FileClosingPre -= _keyManager.OnDBClosing;

            _host = null;
        }

        private void OnWindowAdded(object sender, GwmWindowEventArgs e)
        {
            var keyPromptForm = e.Form as KeyPromptForm;
            if (keyPromptForm != null)
            {
                _keyManager.OnKeyPrompt(keyPromptForm, _host.MainWindow);
                return;
            }

            var optionsForm = e.Form as OptionsForm;
            if (optionsForm != null)
            {
                _keyManager.OnOptionsLoad(optionsForm);
                return;
            }
        }
    }
}
