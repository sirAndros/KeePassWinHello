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
            System.Windows.Forms.MessageBox.Show("Open!");
            if (UAC.IsCurrentProcessElevated() && Settings.Instance.WinStorageEnabled)
                return new KeyWindowsStorage();

            return new KeyMemoryStorage();
        }
    }
}