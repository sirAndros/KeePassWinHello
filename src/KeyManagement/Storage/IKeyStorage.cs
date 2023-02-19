using System;

namespace KeePassWinHello
{
    interface IKeyStorage
    {
        int Count { get; }

        void AddOrUpdate(string dbPath, ProtectedKey protectedKey);
        bool TryGetValue(string dbPath, out ProtectedKey protectedKey);
        bool ContainsKey(string dbPath);
        void Remove(string dbPath);
        void Purge();
        void Clear();
    }

    static class KeyStorageFactory
    {
        public static IKeyStorage Create(AuthCacheType authCacheType)
        {
            if (authCacheType == AuthCacheType.Persistent)
                return new KeyWindowsStorage();

            return new KeyMemoryStorage();
        }

        public static IKeyStorage Create()
        {
            return Create(Settings.Instance.GetAuthCacheType());
        }
    }
}