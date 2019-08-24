﻿using System;
using System.Linq;
using System.Windows.Forms;

namespace KeePassWinHello
{
    class XorProvider : IAuthProvider
    {
        private const byte _entropy = 42;

        public XorProvider()
        {
            CurrentCacheType = AuthCacheType.Local;
        }

        public AuthCacheType CurrentCacheType { get; private set; } // TDB

        public void ClaimCurrentCacheType(AuthCacheType newType)
        {
            CurrentCacheType = newType;
        }

        public byte[] Encrypt(byte[] data)
        {
            return data.Select(x => (byte)(x ^ _entropy)).ToArray();
        }

        public byte[] PromptToDecrypt(byte[] data)
        {
            string message = "Default message";
            var uiContext = AuthProviderUIContext.Current;
            if (uiContext != null)
                message = uiContext.Message;

            var dlgRslt = MessageBox.Show(uiContext, message, "Test", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (dlgRslt == DialogResult.OK)
            {
                return Encrypt(data);
            }
            else
            {
                throw new UnauthorizedAccessException("Canceled");
            }
        }
    }
}
