using System;
using System.Threading;
using System.Threading.Tasks;

namespace KeePassWinHello
{
    class WinHelloProviderForegroundDecorator : IAuthProvider
    {
        private readonly IAuthProvider _winHelloProvider;
        private readonly IntPtr _keePassWindowHandle;

        public WinHelloProviderForegroundDecorator(IAuthProvider provider, IntPtr keePassWindowHandle)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

            _winHelloProvider = provider;
            _keePassWindowHandle = keePassWindowHandle;
        }

        public AuthCacheType CurrentCacheType
        {
            get
            {
                return _winHelloProvider.CurrentCacheType;
            }
        }

        public void ClaimCurrentCacheType(AuthCacheType newType)
        {
            _winHelloProvider.ClaimCurrentCacheType(newType);
        }

        public byte[] Encrypt(byte[] data)
        {
            return _winHelloProvider.Encrypt(data);
        }

        public byte[] PromptToDecrypt(byte[] data)
        {
            using (var tokenSource = new CancellationTokenSource())
            {
                Win32Window.AllowAllSetForeground();
                Task.Factory.StartNew(MakePromptWindowForegroundSafe, tokenSource.Token);

                try
                {
                    var result = _winHelloProvider.PromptToDecrypt(data);
                    BringKeePassMainWindowToFrontSafe();
                    return result;
                }
                catch
                {
                    tokenSource.Cancel();
                    throw;
                } 
            }
        }

        private void BringKeePassMainWindowToFrontSafe()
        {
            try
            {
                Win32Window.From(_keePassWindowHandle).EnsureForeground();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Fail(ex.Message);
            }
        }

        private void MakePromptWindowForegroundSafe()
        {
            try
            {
#if DEBUG
                const string targetWindowClass = null;
#else
                const string targetWindowClass = "Credential Dialog Xaml Host"; 
#endif
                Win32Window.Find(targetWindowClass, "Windows Security", 2000)
                    .EnsureForeground();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Fail(ex.Message);
            }
        }
    }
}
