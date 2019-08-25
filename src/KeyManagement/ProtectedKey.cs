using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;
using KeePassLib.Keys;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePassWinHello
{
    [Serializable]
    class ProtectedKey : SerializationBinder, ISerializable
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

        private const byte VERSION = 1;
        private readonly ProtectedBinary _protectedPassword;
        private readonly List<KcpData> _keys;

        protected ProtectedKey(SerializationInfo info, StreamingContext context)
        {
            var version = info.GetByte("v");
            if (version != VERSION)
                throw new FormatException("Incompatible version of ProtectedKey.");

            var p = (byte[])info.GetValue("p", typeof(byte[]));
            _protectedPassword = new ProtectedBinary(false, p);

            var kl = info.GetInt32("kl");
            _keys = new List<KcpData>(kl);
            for (int i = 0; i != kl; ++i)
            {
                var pfx = string.Format("k{0}", i);
                var n = info.GetString(pfx + "n");
                var t = (KcpType)info.GetValue(pfx + "t", typeof(KcpType));
                var d = (byte[])info.GetValue(pfx + "d", typeof(byte[]));
                _keys.Add(new KcpData(t, d != null ? new ProtectedBinary(false, d) : null, n));
            }
        }

        public ProtectedKey() { }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("v", VERSION);
            info.AddValue("p", _protectedPassword.ReadData(), typeof(byte[]));

            info.AddValue("kl", _keys.Count);
            for (int i = 0; i != _keys.Count; ++i)
            {
                var key = _keys[i];
                var pfx = string.Format("k{0}", i);
                info.AddValue(pfx + "t", key.KcpType, typeof(KcpType));
                info.AddValue(pfx + "n", key.CustomName, typeof(string));
                info.AddValue(pfx + "d", key.EncryptedData != null ? key.EncryptedData.ReadData() : null, typeof(byte[]));
            }
        }

        public static byte[] Serialize(ProtectedKey protectedKey)
        {
            var stream = new MemoryStream();
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, protectedKey);

            return stream.ToArray();
        }

        public static ProtectedKey Deserialize(byte[] data)
        {
            var stream = new MemoryStream();
            stream.Write(data, 0, data.Length);
            stream.Position = 0;

            var formatter = new BinaryFormatter();
            formatter.Binder = new ProtectedKey();
            return (ProtectedKey)formatter.Deserialize(stream);
        }

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

        public override Type BindToType(string assemblyName, string typeName)
        {
            return Assembly.GetExecutingAssembly().GetType(typeName);
        }
    }
}
