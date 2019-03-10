using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public byte[] Encrypt(byte[] data)
        {
            return _winHelloProvider.Encrypt(data);
        }

        public byte[] PromptToDecrypt(byte[] data)
        {
            using (var tokenSource = new CancellationTokenSource())
            {
                Task.Factory.StartNew(MakePromptWindowForegroundSafe, tokenSource.Token);

                try
                {
                    return _winHelloProvider.PromptToDecrypt(data);
                }
                catch
                {
                    tokenSource.Cancel();
                    throw;
                } 
            }
        }

        private static void MakePromptWindowForegroundSafe()
        {
            try
            {
                bool success = Utilities.WindowsForegroundEnsurer.EnsureForeground(IntPtr.Zero, "Credential Dialog Xaml Host", "Windows Security", 2000);
                // todo: if (success) SetForeground(KeePassWin)
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Fail(ex.Message);
            }
        }
    }
}
