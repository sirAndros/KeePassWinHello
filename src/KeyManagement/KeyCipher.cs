using System;
using System.IO;
using System.Windows.Forms;
using KeePassLib.Cryptography;
using KeePassLib.Cryptography.Cipher;
using KeePassLib.Security;
using KeePassLib.Utility;
using KeePassWinHello.Utilities;

namespace KeePassWinHello
{
    class KeyCipher
    {
        private readonly uint _randomSeedBits;
        private readonly byte[] _encryptionIV;
        private readonly ICipherEngine _cipherEngine;
        private readonly IAuthProvider _cryptProvider;

        public KeyCipher()
        {
            _randomSeedBits = 256;
            _encryptionIV = new byte[16];
            _cryptProvider = GetAuthProvider();
            _cipherEngine = CipherPool.GlobalPool.GetCipher(StandardAesEngine.AesUuid);
        }

        private static IAuthProvider GetAuthProvider()
        {
            var authCacheType = Settings.Instance.GetAuthCacheType();
            try
            {
                return AuthProviderFactory.GetInstance(authCacheType);
            }
            catch (AuthProviderKeyNotFoundException ex)
            {
                if (authCacheType == AuthCacheType.Local)
                    throw;

                Settings.Instance.WinStorageEnabled = false;
                authCacheType = Settings.Instance.GetAuthCacheType();
                ErrorHandler.ShowError(ex, "Credential Manager storage has been turned off. Use Options dialog to turn it on.");
                return AuthProviderFactory.GetInstance(authCacheType);
            }
            catch (AuthProviderInvalidKeyException ex)
            {
                if (authCacheType == AuthCacheType.Local)
                    throw;

                Settings.Instance.WinStorageEnabled = false;
                authCacheType = Settings.Instance.GetAuthCacheType();
                ErrorHandler.ShowError(ex, "For security reasons Credential Manager storage has been turned off. Use Options dialog to turn it on.");
                return AuthProviderFactory.GetInstance(authCacheType);
            }
        }

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
