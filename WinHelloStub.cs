using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KeePassWinHello
{
    class WinHelloStub : IWinHello
    {
        public string Message { get; set; }
        public IntPtr ParentHandle { get; set; }

        public byte[] Encrypt(byte[] data)
        {
            return data.ToArray();
        }

        public byte[] PromptToDecrypt(byte[] data)
        {
            var dlgRslt = MessageBox.Show("Decrypt?", "Test", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (dlgRslt == DialogResult.OK)
            {
                return data.ToArray();
            }
            else
            {
                throw new Exception("Canceled");
            }
        }
    }
}
