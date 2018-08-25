using KeePassLib.Keys;
using KeePassLib.Utility;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics.Contracts;
using KeePassLib.Security;
using System.Linq;
using System.Security.Cryptography;
using KeePassLib;
using KeePassLib.Cryptography;
using KeePassLib.Cryptography.Cipher;
using System.Threading.Tasks;
using System.Threading;

namespace KeePassWinHello
{
    public class WinHelloProvider : KeyProvider
    {
        /// <summary>Auto Prompt setting name</summary>
        public const string CfgAutoPrompt = KeePassWinHelloExt.ShortProductName + "_AutoPrompt";
        /// <summary>PIN setting name</summary>
        public const string CfgValidPeriod = KeePassWinHelloExt.ShortProductName + "_ValidPeriod";

        /// <summary>The valid periods in seconds.</summary>
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
            public byte[] Nonce;
            public ProtectedBinary ComposedKey;

            public bool IsValid()
            {
                return ValidUntil >= DateTime.Now;
            }
        }

        /// <summary>Maps database paths to cached keys</summary>
        private readonly Dictionary<string, WinHelloData> unlockCache;

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
            get { return true; }
        }

        public WinHelloProvider()
        {
            unlockCache = new Dictionary<string, WinHelloData>();
        }

        /// <summary>
        /// Creates a cipher instance with a random nonce and the pin as key.
        /// </summary>
        /// <param name="pin">The pin to use as key.</param>
        /// <returns>The cipher instance and the nonce.</returns>
        private static Tuple<CtrBlockCipher, byte[]> CreateCipher(ProtectedString pin)
        {
            Contract.Requires(pin != null);
            Contract.Ensures(Contract.Result<Tuple<ChaCha20Cipher, byte[]>>() != null);

            var nonceLength = PwDefs.FileVersion64 >= 0x0002002300000000UL /*2.35*/ ? 12u : 8u;
            var nonce = CryptoRandom.Instance.GetRandomBytes(nonceLength);

            var pinBytes = pin.ReadUtf8();

            var result = Tuple.Create(CreateCipher(pinBytes, nonce), nonce);

            MemUtil.ZeroByteArray(pinBytes);

            return result;
        }

        /// <summary>
        /// Creates a cipher instance (<see cref="ChaCha20Cipher"/> or <see cref="Salsa20Cipher"/> (KeePass older as 2.35)) with the nonce and the pin as key.
        /// </summary>
        /// <param name="pin">The pin to use as key.</param>
        /// <param name="nonce">The nonce to use as IV.</param>
        /// <returns>The cipher instance.</returns>
        private static CtrBlockCipher CreateCipher(byte[] pin, byte[] nonce)
        {
            Contract.Requires(pin != null);
            Contract.Requires(nonce != null);
            Contract.Ensures(Contract.Result<ChaCha20Cipher>() != null);

            var key = new byte[32];
            using (var h = new SHA512Managed())
            {
                var hashBytes = h.ComputeHash(pin);
                Array.Copy(hashBytes, key, 32);

                MemUtil.ZeroByteArray(hashBytes);
            }

            CtrBlockCipher cipher;

            if (nonce.Length == 12 /*>= KeePass 2.35*/)
            {
                cipher = new ChaCha20Cipher(key, nonce, false);
            }
            else
            {
                cipher = new Salsa20Cipher(key, nonce);
            }

            MemUtil.ZeroByteArray(key);

            return cipher;
        }

        /// <summary>Adds a database-key mapping.</summary>
        /// <param name="databasePath">Full path of the database file.</param>
        /// <param name="pin">The pin to unlock the database.</param>
        /// <param name="keys">The keys used to encrypt the database.</param>
        public void AddCachedKey(string databasePath, ProtectedString pin, CompositeKey keys)
        {
            Contract.Requires(!string.IsNullOrEmpty(databasePath));
            Contract.Requires(pin != null);
            Contract.Requires(keys != null);

            var validPeriod = KeePassWinHelloExt.Host.CustomConfig.GetULong(CfgValidPeriod, VALID_DEFAULT);

            lock (unlockCache)
            {
                var cn = CreateCipher(pin);

                unlockCache[databasePath] = new WinHelloData
                {
                    ValidUntil = validPeriod == VALID_UNLIMITED ? DateTime.MaxValue : DateTime.Now.AddSeconds(validPeriod),
                    Nonce = cn.Item2,
                    ComposedKey = keys.CombineKeys().Encrypt(cn.Item1)
                };

                cn.Item1.Dispose();
            }
        }

        /// <summary>Removes the cached key associated with the path of the database.</summary>
        /// <param name="databasePath">Full path of the database file.</param>
        public void RemoveCachedKey(string databasePath)
        {
            lock (unlockCache)
            {
                unlockCache.Remove(databasePath);
            }
        }

        /// <summary>Attempts to get cached key from the given key.</summary>
        /// <param name="databasePath">Full path of the database file.</param>
        /// <param name="data">[out] The data.</param>
        /// <returns>true if it succeeds, false if it fails.</returns>
        private bool TryGetCachedKey(string databasePath, out WinHelloData data)
        {
            lock (unlockCache)
            {
                return unlockCache.TryGetValue(databasePath, out data);
            }
        }

        /// <summary>Query if the path of the database has a cached key.</summary>
        /// <param name="databasePath">Full path of the database file.</param>
        /// <returns>true if a cached key exists, false if not.</returns>
        public bool IsCachedKey(string databasePath)
        {
            WinHelloData temp;
            return TryGetCachedKey(databasePath, out temp);
        }

        /// <summary>Clears the expiered keys.</summary>
        public void ClearExpieredKeys()
        {
            lock (unlockCache)
            {
                foreach (var key in unlockCache.Where(kv => kv.Value.IsValid() == false).Select(kv => kv.Key).ToList())
                {
                    unlockCache.Remove(key);
                }
            }
        }

        /// <summary>Clears the cache.</summary>
        public void ClearCache()
        {
            lock (unlockCache)
            {
                unlockCache.Clear();
            }
        }

        public override byte[] GetKey(KeyProviderQueryContext ctx)
        {
            if (ctx.CreatingNewKey)
            {
                MessageService.ShowWarning("Can't use WinHello to create new keys.");
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

            using (var form = new PromptHolder())
            {
                if (form.ShowDialog() != DialogResult.OK)
                    return null;
            }

            ProtectedBinary result;
            var pinBytes = new ProtectedString(true, KeePassWinHelloExt.Salt).ReadUtf8();
            using (var cipher = CreateCipher(pinBytes, data.Nonce))
            {
                RemoveCachedKey(ctx.DatabasePath);
                result = data.ComposedKey.Decrypt(cipher);
            }
            MemUtil.ZeroByteArray(pinBytes);

            return result.ReadData();
        }
    }
}
