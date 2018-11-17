using System;

namespace KeePassWinHello
{
    public interface IWinHello
    {
        string Message { get; set; }
        IntPtr ParentHandle { get; set; }

        byte[] Encrypt(byte[] data);
        byte[] PromptToDecrypt(byte[] data);
    }

    static class WinHelloCryptProvider
    {
        public static IWinHello GetInstance()
        {
#if DEBUG
            return new WinHelloStub();
#else
            return new WinHello();
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