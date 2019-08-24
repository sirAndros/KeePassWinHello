using System;
using System.IO;
using KeePassLib.Cryptography;
using KeePassLib.Cryptography.Cipher;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePassWinHello
{
    class KeyCipher
    {
        private readonly uint _randomSeedBits;
        private readonly byte[] _encryptionIV;
        private readonly ICipherEngine _cipherEngine;
        private readonly IAuthProvider _cryptProvider;

        public KeyCipher(string message, IntPtr windowHandle)
        {
            _randomSeedBits = 256;
            //_encryptionIV = CryptoRandom.Instance.GetRandomBytes(16);
            _encryptionIV = new byte[16];
            _cipherEngine = CipherPool.GlobalPool.GetCipher(StandardAesEngine.AesUuid);
            _cryptProvider = AuthProviderFactory.GetInstance(message, windowHandle);
        }

        public bool IsAvailable { get { return AuthProviderFactory.IsAvailable(); } }

        public IAuthProvider AuthProvider { get { return _cryptProvider; } }

        public ProtectedBinary Protect(ProtectedBinary key)
        {
            byte[] data = key.ReadData();
            byte[] encryptedData = null;
            try
            {
                encryptedData = _cryptProvider.Encrypt(data);

                var result = new ProtectedBinary(true, encryptedData);
                return result;
            }
            finally
            {
                MemUtil.ZeroByteArray(data);
                if (encryptedData != null)
                    MemUtil.ZeroByteArray(encryptedData);
            }
        }

        public ProtectedBinary UnProtect(ProtectedBinary key)
        {
            byte[] encryptedData = key.ReadData();
            byte[] data = null;
            try
            {
                data = _cryptProvider.PromptToDecrypt(encryptedData);

                var result = new ProtectedBinary(true, data);
                return result;
            }
            finally
            {
                MemUtil.ZeroByteArray(encryptedData);
                if (data != null)
                    MemUtil.ZeroByteArray(data);
            }
        }

        public ProtectedBinary GeneratePassword()
        {
            var data = CryptoRandom.Instance.GetRandomBytes(_randomSeedBits >> 3);
            try
            {
                return new ProtectedBinary(true, data);
            }
            finally
            {
                MemUtil.ZeroByteArray(data);
            }
        }

        public byte[] Decrypt(ProtectedBinary encodedData, ProtectedBinary password)
        {
            byte[] rawData = encodedData.ReadData();
            byte[] rawPassword = password.ReadData();
            try
            {
                using (var stream = new MemoryStream(rawData))
                using (var decryptingStream = _cipherEngine.DecryptStream(stream, rawPassword, _encryptionIV))
                using (var reader = new MemoryStream())
                {
                    decryptingStream.CopyTo(reader);
                    return reader.ToArray();
                }
            }
            finally
            {
                MemUtil.ZeroByteArray(rawData);
                MemUtil.ZeroByteArray(rawPassword);
            }
        }

        public ProtectedBinary Encrypt(ProtectedBinary data, ProtectedBinary password)
        {
            byte[] rawData = data.ReadData();
            byte[] rawPassword = password.ReadData();
            byte[] rawEncrypted = null;
            try
            {
                using (var stream = new MemoryStream())
                using (var encryptingStream = _cipherEngine.EncryptStream(stream, rawPassword, _encryptionIV))
                {
                    encryptingStream.Write(rawData, 0, rawData.Length);
                    encryptingStream.Close();

                    rawEncrypted = stream.ToArray();
                    return new ProtectedBinary(true, rawEncrypted);
                }
            }
            finally
            {
                MemUtil.ZeroByteArray(rawData);
                MemUtil.ZeroByteArray(rawPassword);
                if (rawEncrypted != null)
                    MemUtil.ZeroByteArray(rawEncrypted);
            }
        }

        public ProtectedBinary Encrypt(ProtectedString data, ProtectedBinary password)
        {
            var rawData = data.ReadUtf8();
            try
            {
                return Encrypt(new ProtectedBinary(true, rawData), password);
            }
            finally
            {
                MemUtil.ZeroByteArray(rawData);
            }
        }
    }
}
