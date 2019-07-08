using System;

namespace KeePassWinHello
{
    public interface IAuthProvider
    {
        byte[] Encrypt(byte[] data);
        byte[] PromptToDecrypt(byte[] data);
    }

    static class AuthProviderFactory
    {
        public static IAuthProvider GetInstance(string message, IntPtr windowHandle)
        {
#if DEBUG
            return new XorProvider
            {
                Message = message,
                Handle = windowHandle,
            };
#else
            var provider = new WinHelloProvider
            {
                Message = message,
                ParentHandle = windowHandle,
            };

            if (UAC.IsCurrentProcessElevated())
                return new WinHelloProviderForegroundDecorator(provider);
            else
                return provider;
#endif
        }

        public static bool IsAvailable()
        {
#if DEBUG
            return true;
#else
            return WinHelloProvider.IsAvailable();
#endif
        }
    }
}