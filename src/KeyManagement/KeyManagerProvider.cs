using System;
using KeePassWinHello.Utilities;

namespace KeePassWinHello
{
    class KeyManagerProvider : IDisposable
    {
        private readonly object _initMutex = new Object();

        private KeyManager _keyManager;
        private bool? _wasUnavailable = null;
        private HDESK _mainDesktop;

        public KeyManagerProvider(HDESK mainDesktop)
        {
            _mainDesktop = mainDesktop;
        }

        public KeyManager ObtainKeyManager()
        {
            TryInitKeyManager();
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

        private void TryInitKeyManager()
        {
            lock (_initMutex)
            {
                if (_wasUnavailable == false)
                    return;

                Dispose(); //always init with actual parent window

                try
                {
                    _keyManager = new KeyManager(_mainDesktop);
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
