using System;
using System.Threading;
using System.Threading.Tasks;

namespace KeePassWinHello
{
    class WinHelloProviderForegroundDecorator : IAuthProvider
    {
        private readonly WinHelloProvider _winHelloProvider;

        public WinHelloProviderForegroundDecorator(WinHelloProvider winHelloProvider)
        {
            if (winHelloProvider == null)
                throw new ArgumentNullException("winHelloProvider");

            _winHelloProvider = winHelloProvider;
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
                Utilities.WindowsForegroundEnsurer.AllowAllSetForeground();
                Task.Factory.StartNew(MakePromptWindowForegroundSafe, tokenSource.Token);

                try
                {
                    var result = _winHelloProvider.PromptToDecrypt(data);
                    Utilities.WindowsForegroundEnsurer.EnsureForeground(_winHelloProvider.ParentHandle);
                    return result;
                }
                catch
                {
                    tokenSource.Cancel();
                    throw;
                } 
            }
        }

        private void MakePromptWindowForegroundSafe()
        {
            try
            {
                Utilities.WindowsForegroundEnsurer.EnsureForeground(IntPtr.Zero, "Credential Dialog Xaml Host", "Windows Security", 2000); 
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Fail(ex.Message);
            }
        }
    }
}
