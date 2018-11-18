using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeePassLib.Keys;

namespace WinHelloQuickUnlock
{
    class KeyStorage
    {
        class Data
        {
            private readonly DateTime _createdDate;
            private readonly CompositeKey _compositeKey;

            public CompositeKey CompositeKey { get { return _compositeKey; } }

            public Data(CompositeKey compositeKey)
            {
                if (compositeKey == null)
                    throw new ArgumentNullException("compositeKey");

                _compositeKey = compositeKey;
                _createdDate = DateTime.UtcNow;
            }

            public bool IsValid()
            {
                return DateTime.UtcNow - _createdDate < Settings.Instance.InvalidatingTime;
            }
        }

        private readonly IDictionary<string, Data> _keys = new ConcurrentDictionary<string, Data>();

        public void AddOrUpdate(string dbPath, CompositeKey compositeKey)
        {
            _keys[dbPath] = new Data(compositeKey);
        }

        public void Remove(string dbPath)
        {
            _keys.Remove(dbPath);
        }

        public bool TryGetValue(string dbPath, out CompositeKey compositeKey)
        {
            Data data;
            if (_keys.TryGetValue(dbPath, out data))
            {
                if (data.IsValid())
                {
                    compositeKey = data.CompositeKey;
                    return true;
                }
                _keys.Remove(dbPath);
            }

            compositeKey = null;
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
