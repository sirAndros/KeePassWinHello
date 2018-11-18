using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using KeePassLib.Keys;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace WinHelloQuickUnlock
{
    class KeyCipher
    {
        private readonly IWinHello _cryptProvider;

        public KeyCipher(string message, IntPtr windowHandle)
        {
            _cryptProvider = WinHelloCryptProvider.GetInstance(message, windowHandle);
        }


        public CompositeKey Protect(CompositeKey compositeKey)
        {
            return compositeKey; //todo
            var result = new CompositeKey();
            foreach (var key in compositeKey.UserKeys)
            {
                var protectedKeyData = Protect(key.KeyData);
                //result.AddUserKey();
            }
            return result;
        }

        public CompositeKey UnProtect(CompositeKey compositeKey)
        {
            try
            {
                var data = _cryptProvider.Encrypt(Encoding.UTF8.GetBytes("TEST_MOTHER_FUCKER"));
                _cryptProvider.PromptToDecrypt(data);
                return compositeKey; //todo
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.ToString());
                Debug.Fail(ex.ToString());
                return null;
            }
        }

        //private IUserKey ReCreateKey(IUserKey oldKey, ProtectedBinary newSecret)
        //{
        //    var type = oldKey.GetType();
        //    if (type == typeof(KcpPassword))
        //    {
        //        return new 
        //    }
        //    switch (type.FullName)
        //    {
        //        case "KeePassLib.Keys.KcpPassword":
        //            return new KcpPassword()
        //        default:
        //            break;
        //    }
        //}

        private ProtectedBinary Protect(ProtectedBinary key)
        {
            byte[] data = key.ReadData();
            byte[] encryptedData;
            try
            {
                encryptedData = _cryptProvider.Encrypt(data);
            }
            finally
            {
                MemUtil.ZeroByteArray(data);
            }

            var result = new ProtectedBinary(true, encryptedData);
            MemUtil.ZeroByteArray(encryptedData);
            return result;
        }

        private ProtectedBinary UnProtect(ProtectedBinary key)
        {
            byte[] encryptedData = key.ReadData();

            byte[] data;
            try
            {
                data = _cryptProvider.PromptToDecrypt(encryptedData);
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
