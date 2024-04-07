using System;
using KeePassWinHello.Utilities;

namespace KeePassWinHello
{
    class KeyManagerProvider : IDisposable
    {
        private readonly object _initMutex = new Object();
        private readonly UIContextManager _uiContextManager;

        private KeyManager _keyManager;
        private bool? _wasUnavailable = null;

        public KeyManagerProvider(UIContextManager uiContextManager)
        {
            _uiContextManager = uiContextManager;
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
                    var authProvider = GetAuthProvider();
                    var keyCipher = new KeyCipher(authProvider);
                    _keyManager = new KeyManager(_uiContextManager, keyCipher);
                    _wasUnavailable = false;
                }
                catch (AuthProviderIsUnavailableException)
                {
                    _wasUnavailable = true;
                }
                catch (Exception ex)
                {
                    _wasUnavailable = false;
                    _uiContextManager.CurrentContext.ShowError(ex);
                }
            }
        }

        private IAuthProvider GetAuthProvider()
        {
            var authCacheType = Settings.Instance.GetAuthCacheType();
            try
            {
                return AuthProviderFactory.GetInstance(authCacheType, _uiContextManager);
            }
            catch (AuthProviderKeyNotFoundException ex)
            {
                if (authCacheType == AuthCacheType.Local)
                    throw;

                Settings.Instance.WinStorageEnabled = false;
                authCacheType = Settings.Instance.GetAuthCacheType();
                _uiContextManager.CurrentContext.ShowError(ex, "Credential Manager storage has been turned off. Use Options dialog to turn it on.");
                return AuthProviderFactory.GetInstance(authCacheType, _uiContextManager);
            }
            catch (AuthProviderInvalidKeyException ex)
            {
                if (authCacheType == AuthCacheType.Local)
                    throw;

                Settings.Instance.WinStorageEnabled = false;
                authCacheType = Settings.Instance.GetAuthCacheType();
                _uiContextManager.CurrentContext.ShowError(ex, "For security reasons Credential Manager storage has been turned off. Use Options dialog to turn it on.");
                return AuthProviderFactory.GetInstance(authCacheType, _uiContextManager);
            }
        }
    }
}
