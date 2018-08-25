using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using KeePass.UI;

namespace KeePassWinHello
{
    public partial class PromptHolder : Form
    {
        public PromptHolder()
        {
            InitializeComponent();
            GlobalWindowManager.AddWindow(this);
        }

        private void TestForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            GlobalWindowManager.RemoveWindow(this);
        }

        private void TestForm_Shown(object sender, EventArgs e)
        {
        }

        private void TestForm_Load(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(500);

                BeginInvoke(new Action(() =>
                {
                    bool res = WinHelloProxy.RequestHelloAuthAsync("Authentication to access KeePass database", delegate (AuthStatus status)
                    {
                        if (status == AuthStatus.whSuccess)
                        {
                            DialogResult = DialogResult.OK;
                            //MessageBox.Show("OK");
                        }
                        else
                        {
                            DialogResult = DialogResult.No;
                            //MessageBox.Show("Fail");
                            //KeePassWinHelloExt.Provider.RemoveCachedKey(_db);
                        }
                        Close();
                    });

                    if (!res)
                        Close();
                }));
            });
        }
    }
}
