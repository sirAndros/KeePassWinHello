using System;

namespace KeePassWinHello
{
    public enum AuthCacheType
    {
        Persistent,
        Local,
    }

    public interface IAuthProvider
    {
        byte[] Encrypt(byte[] data);
        byte[] PromptToDecrypt(byte[] data);
        void ClaimCurrentCacheType(AuthCacheType authCacheType);
        AuthCacheType CurrentCacheType { get; set; }
    }

    static class AuthProviderFactory
    {
        public static IAuthProvider Create(IntPtr keePassWindowHandle)
        {
            var authCacheType = Settings.Instance.GetAuthCacheType();
#if DEBUG
            var provider = new XorProvider(authCacheType);
#else
            var provider = new WinHelloProvider(authCacheType);
#endif
            if (UAC.IsCurrentProcessElevated)
                return new WinHelloProviderForegroundDecorator(provider, keePassWindowHandle);
            else
                return provider;
        }
    }
}
