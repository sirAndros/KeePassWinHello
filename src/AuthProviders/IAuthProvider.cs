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
        AuthCacheType CurrentCacheType { get; }
    }

    static class AuthProviderFactory
    {
        public static IAuthProvider GetInstance(AuthCacheType authCacheType)
        {
#if DUMMY_PROVIDER
            var provider = new XorProvider(authCacheType);
#else
            var provider = WinHelloProvider.CreateInstance(authCacheType);
#endif
            if (UAC.IsCurrentProcessElevated)
                return new WinHelloProviderForegroundDecorator(provider);
            else
                return provider;
        }
    }
}