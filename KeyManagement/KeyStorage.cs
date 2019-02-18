using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace KeePassWinHello
{
    class KeyStorage
    {
        class Data
        {
            private readonly DateTime _createdDate;
            private readonly ProtectedKey _protectedKey;

            public ProtectedKey ProtectedKey { get { return _protectedKey; } }

            public Data(ProtectedKey protectedKey)
            {
                if (protectedKey == null)
                    throw new ArgumentNullException("protectedKey");

                _protectedKey = protectedKey;
                _createdDate = DateTime.UtcNow;
            }

            public bool IsValid()
            {
                return DateTime.UtcNow - _createdDate < Settings.Instance.InvalidatingTime;
            }
        }

        private readonly IDictionary<string, Data> _keys = new ConcurrentDictionary<string, Data>();

        public void AddOrUpdate(string dbPath, ProtectedKey protectedKey)
        {
            _keys[dbPath] = new Data(protectedKey);
        }

        public void Remove(string dbPath)
        {
            _keys.Remove(dbPath);
        }

        public bool ContainsKey(string dbPath)
        {
            Data data;
            return _keys.TryGetValue(dbPath, out data)
                && data.IsValid();
        }

        public bool TryGetValue(string dbPath, out ProtectedKey protectedKey)
        {
            Data data;
            if (_keys.TryGetValue(dbPath, out data))
            {
                if (data.IsValid())
                {
                    protectedKey = data.ProtectedKey;
                    return true;
                }
                _keys.Remove(dbPath);
            }

            protectedKey = null;
            return false;
        }

        public void Purge()
        {
            foreach (var item in _keys)
            {
                if (!item.Value.IsValid())
                {
                    _keys.Remove(item.Key);
                }
            }
        }
    }
}
