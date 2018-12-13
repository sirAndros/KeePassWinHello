using System;

namespace WinHelloQuickUnlock
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
            return new WinHelloProvider
            {
                Message = message,
                ParentHandle = windowHandle,
            };
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