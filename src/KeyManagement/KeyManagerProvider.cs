﻿using System;
using System.Windows.Forms;
using KeePass.Plugins;
using KeePassWinHello.Utilities;

namespace KeePassWinHello
{
    class KeyManagerProvider : IDisposable
    {
        private readonly object _initMutex = new Object();
        private readonly IPluginHost _host;

        private KeyManager _keyManager;
        private bool? _wasUnavailable = null;

        public KeyManagerProvider(IPluginHost host)
        {
            _host = host;
        }

        public KeyManager ObtainKeyManager(IWin32Window parentWindow)
        {
            TryInitKeyManager(parentWindow);
            return _keyManager; // Can be null
        }

        public void Dispose()
        {
            lock (_initMutex)
            {
                if (_keyManager != null)
                {
                    _keyManager.Dispose();
                    _keyManager = null;
                }
                _wasUnavailable = null;
            }
        }

        private void TryInitKeyManager(IWin32Window parentWindow)
        {
            lock (_initMutex)
            {
                if (_wasUnavailable == false)
                    return;

                try
                {
                    _keyManager = new KeyManager(parentWindow);
                    _wasUnavailable = false;
                }
                catch (AuthProviderIsUnavailableException)
                {
                    _wasUnavailable = true;
                }
                catch (Exception ex)
                {
                    _wasUnavailable = false;
                    ErrorHandler.ShowError(ex);
                }
            }
        }
    }
}
