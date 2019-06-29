using System;

namespace KeePassWinHello
{
    interface IKeyStorage
    {
        void AddOrUpdate(string dbPath, ProtectedKey protectedKey);
        bool TryGetValue(string dbPath, out ProtectedKey protectedKey);
        bool ContainsKey(string dbPath);
        void Remove(string dbPath);
        void Purge();
    }

    static class KeyStorageFactory
    {
        public static IKeyStorage Create()
        {
            if (UAC.IsCurrentProcessElevated && Settings.Instance.WinStorageEnabled)
                throw new NotImplementedException();

            return new KeyMemoryStorage();
        }
    }
}