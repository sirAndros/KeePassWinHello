using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using KeePassLib.Keys;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePassWinHello
{
    public class WinHelloProvider : KeyProvider
    {
        public const string CfgAutoPrompt = KeePassWinHelloExt.ShortProductName + "_AutoPrompt";
        public const string CfgValidPeriod = KeePassWinHelloExt.ShortProductName + "_ValidPeriod";

        public const ulong VALID_UNLIMITED = 0;
        public const ulong VALID_1MINUTE = 60;
        public const ulong VALID_5MINUTES = VALID_1MINUTE * 5;
        public const ulong VALID_10MINUTES = VALID_5MINUTES * 2;
        public const ulong VALID_15MINUTES = VALID_5MINUTES * 3;
        public const ulong VALID_30MINUTES = VALID_15MINUTES * 2;
        public const ulong VALID_1HOUR = VALID_30MINUTES * 2;
        public const ulong VALID_2HOURS = VALID_1HOUR * 2;
        public const ulong VALID_6HOURS = VALID_2HOURS * 3;
        public const ulong VALID_12HOURS = VALID_6HOURS * 2;
        public const ulong VALID_1DAY = VALID_12HOURS * 2;
        public const ulong VALID_7DAYS = VALID_1DAY * 7;
        public const ulong VALID_MONTH = VALID_1DAY * 30;
        public const ulong VALID_DEFAULT = VALID_10MINUTES;

        private class WinHelloData
        {
            public DateTime ValidUntil;
            public ProtectedBinary ComposedKey;

            public bool IsValid()
            {
                return ValidUntil >= DateTime.Now;
            }
        }

        /// <summary>Maps database paths to cached keys</summary>
        private readonly Dictionary<string, WinHelloData> _unlockCache;

        public override string Name
        {
            get { return KeePassWinHelloExt.ShortProductName; }
        }

        public override bool DirectKey
        {
            get { return true; }
        }

        public override bool SecureDesktopCompatible
        {
            get { return false; }
        }

        public WinHelloProvider()
        {
            _unlockCache = new Dictionary<string, WinHelloData>();
        }


        public void CacheKeyForDB(string databasePath, CompositeKey keys)
        {
            Contract.Requires(!string.IsNullOrEmpty(databasePath));
            Contract.Requires(keys != null);
            Contract.Requires(WinHello.IsAvailable());

            var validPeriod = KeePassWinHelloExt.Host.CustomConfig.GetULong(CfgValidPeriod, VALID_DEFAULT);
            lock (_unlockCache)
            {
                _unlockCache[databasePath] = new WinHelloData
                {
                    ValidUntil = validPeriod == VALID_UNLIMITED ? DateTime.MaxValue : DateTime.Now.AddSeconds(validPeriod),
                    ComposedKey = Encrypt(keys),
                };
            }
        }

        public void RemoveCachedKey(string databasePath)
        {
            lock (_unlockCache)
                _unlockCache.Remove(databasePath);
        }

        private bool TryGetCachedKey(string databasePath, out WinHelloData data)
        {
            lock (_unlockCache)
                return _unlockCache.TryGetValue(databasePath, out data);
        }

        public bool IsCachedKey(string databasePath)
        {
            lock (_unlockCache)
                return _unlockCache.ContainsKey(databasePath);
        }

        public void ClearExpiredKeys()
        {
            lock (_unlockCache)
                foreach (var key in _unlockCache.Where(kv => !kv.Value.IsValid()).Select(kv => kv.Key).ToList())
                    _unlockCache.Remove(key);
        }

        public void ClearCache()
        {
            lock (_unlockCache)
                _unlockCache.Clear();
        }

        public override byte[] GetKey(KeyProviderQueryContext ctx)
        {
            if (ctx.CreatingNewKey)
            {
                MessageService.ShowWarning("Can't use this plugin to create new keys.");
                return null;
            }

            if (ctx.IsOnSecureDesktop)
            {
                MessageService.ShowWarning("Can't use WinHello on secure desktop.");
                return null;
            }

            WinHelloData data;
            if (!TryGetCachedKey(ctx.DatabasePath, out data) || !data.IsValid())
            {
                MessageService.ShowWarning("WinHello is not available for this database.");
                return null;
            }

            try
            {
                RemoveCachedKey(ctx.DatabasePath);
                var result = Decrypt(data.ComposedKey);
                return result.ReadData();
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.ToString());
                return null;
            }
        }


        private ProtectedBinary CombineKeys(CompositeKey key)
        {
            var dataList = new List<byte[]>();
            int dataLength = 0;
            foreach (var pKey in key.UserKeys)
            {
                var b = pKey.KeyData;
                if (b != null)
                {
                    var keyData = b.ReadData();
                    dataList.Add(keyData);
                    dataLength += keyData.Length;
                }
            }

            var allData = new byte[dataLength];
            int p = 0;
            foreach (var pbData in dataList)
            {
                Array.Copy(pbData, 0, allData, p, pbData.Length);
                p += pbData.Length;
                MemUtil.ZeroByteArray(pbData);
            }

            var pb = new ProtectedBinary(true, allData);
            MemUtil.ZeroByteArray(allData);
            return pb;
        }

        private ProtectedBinary Encrypt(CompositeKey composedKey)
        {
            return Encrypt(CombineKeys(composedKey));
        }
        private ProtectedBinary Encrypt(ProtectedBinary composedKey)
        {
            WinHello winHello = new WinHello();
            byte[] data = composedKey.ReadData();
            byte[] encryptedData;
            try
            {
                encryptedData = winHello.Encrypt(data);
            }
            finally
            {
                MemUtil.ZeroByteArray(data);
            }

            var result = new ProtectedBinary(true, encryptedData);
            MemUtil.ZeroByteArray(encryptedData);
            return result;
        }

        private ProtectedBinary Decrypt(ProtectedBinary encryptedKey)
        {
            WinHello winHello = new WinHello();
            winHello.Message = "Authentication to access KeePass database";
            winHello.ParentHandle = KeePassWinHelloExt.KeyPromptForm.Handle;

            byte[] encryptedData = encryptedKey.ReadData();

            byte[] data;
            try
            {
                data = winHello.PromptToDecrypt(encryptedData);
            }
            finally
            {
                MemUtil.ZeroByteArray(encryptedData);
            }

            var result = new ProtectedBinary(true, data);
            MemUtil.ZeroByteArray(data);
            return result;
        }
    }
}
