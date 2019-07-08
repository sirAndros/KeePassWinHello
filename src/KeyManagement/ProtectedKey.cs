using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using KeePassLib.Keys;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePassWinHello
{
    class ProtectedKey
    {
        enum KcpType
        {
            kcpCustomKey,
            kcpPassword,
            kcpKeyFile,
            kcpUserAccount,
        }
        class KcpData
        {
            public KcpType KcpType { get; private set; }
            public ProtectedBinary EncryptedData { get; private set; }
            public string CustomName { get; private set; }

            public KcpData(KcpType kcpType, ProtectedBinary encryptedData = null, string name = null)
            {
                KcpType = kcpType;
                EncryptedData = encryptedData;
                CustomName = name;
            }
        }

        private readonly ProtectedBinary _protectedPassword;
        private readonly List<KcpData> _keys;

        public static ProtectedKey Create(CompositeKey compositeKey, KeyCipher keyCipher)
        {
            return new ProtectedKey(compositeKey, keyCipher);
        }

        public ProtectedKey(CompositeKey compositeKey, KeyCipher keyCipher)
        {
            var password = keyCipher.GeneratePassword();
            _protectedPassword = keyCipher.Protect(password);
            _keys = new List<KcpData>();

            foreach (var key in compositeKey.UserKeys)
            {
                KcpData data;

                var p = key as KcpPassword;
                var kf = key as KcpKeyFile;
                var ck = key as KcpCustomKey;

                if (p != null)
                {
                    if (p.Password != null)
                        data = new KcpData(KcpType.kcpPassword, keyCipher.Encrypt(p.Password, password));
                    else
                        data = new KcpData(KcpType.kcpCustomKey, keyCipher.Encrypt(p.KeyData, password), p.ToString());
                }
                else if (kf != null)
                {
                    data = new KcpData(KcpType.kcpKeyFile, keyCipher.Encrypt(new ProtectedString(false, kf.Path), password));
                }
                else if (key is KcpUserAccount)
                {
                    data = new KcpData(KcpType.kcpUserAccount);
                }
                else
                {
                    Debug.Assert(ck != null, "Unknown key type");
                    var name = ck != null ? ck.Name : key.ToString();
                    data = new KcpData(KcpType.kcpCustomKey, keyCipher.Encrypt(key.KeyData, password), name);
                }

                _keys.Add(data);
            }
        }

        public CompositeKey GetCompositeKey(KeyCipher keyCipher)
        {
            var password = keyCipher.UnProtect(_protectedPassword);
            var compositeKey = new CompositeKey();

            foreach (var data in _keys)
            {
                IUserKey key = null;
                byte[] rawData = null;
                if (data.KcpType != KcpType.kcpUserAccount)
                    rawData = keyCipher.Decrypt(data.EncryptedData, password);

                try
                {
                    switch (data.KcpType)
                    {
                        case KcpType.kcpCustomKey:
                            key = new KcpCustomKey(data.CustomName, rawData, false);
                            break;
                        case KcpType.kcpPassword:
                            key = new KcpPassword(rawData);
                            break;
                        case KcpType.kcpKeyFile:
                            key = new KcpKeyFile(Encoding.UTF8.GetString(rawData));
                            break;
                        case KcpType.kcpUserAccount:
                            key = new KcpUserAccount();
                            break;
                    }
                }
                finally
                {
                    if (rawData != null)
                        MemUtil.ZeroByteArray(rawData);
                }

                Debug.Assert(key != null);
                compositeKey.AddUserKey(key);
            }

            return compositeKey;
        }
    }
}
