using System;

namespace WinHelloQuickUnlock
{
    public interface IWinHello
    {
        byte[] Encrypt(byte[] data);
        byte[] PromptToDecrypt(byte[] data);
    }

    static class WinHelloCryptProvider
    {
        public static IWinHello GetInstance(string message, IntPtr windowHandle)
        {
#if DEBUG
            return new WinHelloStub
            {
                Message = message,
                Handle = windowHandle,
            };
#else
            return new WinHello
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
            return WinHello.IsAvailable();
#endif
        }
    }
}