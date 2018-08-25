using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Timers;
using System.Windows.Forms;
using KeePass.Forms;
using KeePass.Plugins;
using KeePass.UI;
using KeePassLib.Serialization;

namespace KeePassWinHello
{
    public class KeePassWinHelloExt : Plugin
	{
        public static KeyPromptForm _keyPromptForm { get; private set; }
        public static IPluginHost Host { get; private set; } = null;
        public static WinHelloProvider Provider { get; private set; } = null;

        private System.Timers.Timer timer;
        public const string ShortProductName = "WindowsHello";
        public const string Salt = "StrongSecureSaltWithSaltAndBitches!!!ААААААГОНЬ!!!";

        public override Image SmallIcon => Properties.Resources.windows_hello16x16;
        public override string UpdateUrl => "https://github.com/sirAndros/KeePassWinHello/raw/master/keepass.version"; 


        public override bool Initialize(IPluginHost host)
		{
			if (Host != null) { Debug.Assert(false); Terminate(); }
			if (host == null) { return false; }

			Provider = new WinHelloProvider();

            Host = host;
			Host.KeyProviderPool.Add(Provider);
			Host.MainWindow.FileClosingPre += FileClosingPreHandler;

			GlobalWindowManager.WindowAdded += WindowAddedHandler;

			timer = new System.Timers.Timer(1000);
			timer.Elapsed += ElapsedHandler;
			timer.Start();

			return true;
		}

		public override void Terminate()
		{
			if (Host == null)
                return;

            if (timer != null)
            {
                timer.Stop();
                timer.Elapsed -= ElapsedHandler;
                timer = null; 
            }

			GlobalWindowManager.WindowAdded -= WindowAddedHandler;
            Host.MainWindow.FileClosingPre -= FileClosingPreHandler;
			Host.KeyProviderPool.Remove(Provider);

			Host = null;
		}

		/// <summary>
		/// If the timer elapsed clear the expiered keys.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ElapsedHandler(object sender, ElapsedEventArgs e)
		{
			Provider.ClearExpieredKeys();
		}

		/// <summary>
		/// Gets the masterkey before the database is closed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void FileClosingPreHandler(object sender, FileClosingEventArgs e)
		{
			if (e == null) { Debug.Assert(false); return; }
			if (e.Cancel) { return; }

			if (e.Database != null && e.Database.MasterKey != null)
			{
                var pin = new KeePassLib.Security.ProtectedString(true, Salt);
                Provider.AddCachedKey(e.Database.IOConnectionInfo.Path, pin, e.Database.MasterKey);

				// If no key is set, remove possible cached key.
				//provider.RemoveCachedKey(e.Database.IOConnectionInfo.Path);
			}
		}

		/// <summary>
		/// Used to modify other form when they load.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void WindowAddedHandler(object sender, GwmWindowEventArgs e)
		{
			var keyPromptForm = e.Form as KeyPromptForm;
			if (keyPromptForm != null)
			{
                _keyPromptForm = keyPromptForm;

                keyPromptForm.Shown += delegate (object sender2, EventArgs e2)
				{
					// Warning: If one of the private fields get renamed this method will fail!
					var m_cmbKeyFile = keyPromptForm.Controls.Find("m_cmbKeyFile", false).FirstOrDefault() as ComboBox;
					if (m_cmbKeyFile != null)
					{
						var fieldInfo = keyPromptForm.GetType().GetField("m_ioInfo", BindingFlags.Instance | BindingFlags.NonPublic);
						if (fieldInfo != null)
						{
							var ioInfo = fieldInfo.GetValue(keyPromptForm) as IOConnectionInfo;
							if (ioInfo != null)
							{
								if (Provider.IsCachedKey(ioInfo.Path) && WinHelloProxy.IsHelloAuthAvailable())
								{
									var index = m_cmbKeyFile.Items.IndexOf(ShortProductName);
									if (index != -1)
									{
										m_cmbKeyFile.SelectedIndex = index;

										var m_cbPassword = keyPromptForm.Controls.Find("m_cbPassword", false).FirstOrDefault() as CheckBox;
										if (m_cbPassword != null)
										{
											UIUtil.SetChecked(m_cbPassword, false);
										}

                                        // If AutoPrompt is enabled click the Ok button.
                                        if (Host.CustomConfig.GetBool(WinHelloProvider.CfgAutoPrompt, true))
                                        {
                                            var m_btnOK = keyPromptForm.Controls.Find("m_btnOK", false).FirstOrDefault() as Button;
                                            if (m_btnOK != null)
                                            {
                                                m_btnOK.PerformClick();
                                            }
                                        }
                                        return;
									}
								}
							}
						}

						// If KeePass autoselected WinHello but there isn't a key available just unselect it.
						if (m_cmbKeyFile.Text == ShortProductName)
						{
							var m_cbKeyFile = keyPromptForm.Controls.Find("m_cbKeyFile", false).FirstOrDefault() as CheckBox;
							if (m_cbKeyFile != null)
							{
								UIUtil.SetChecked(m_cbKeyFile, false);
							}
						}
					}
				};
			}

			var optionsForm = e.Form as OptionsForm;
			if (optionsForm != null)
			{
				optionsForm.Shown += delegate (object sender2, EventArgs e2)
				{
					try
					{
						// Add the WinHello options tab.
						var m_tabMain = optionsForm.Controls.Find("m_tabMain", true).FirstOrDefault() as TabControl;
						if (m_tabMain != null)
						{
							if (m_tabMain.ImageList == null)
							{
								m_tabMain.ImageList = new ImageList();
							}
							var imageIndex = m_tabMain.ImageList.Images.Add(Properties.Resources.windows_hello16x16, Color.Transparent);

							var newTab = new TabPage(ShortProductName);
							newTab.UseVisualStyleBackColor = true;
							newTab.ImageIndex = imageIndex;

							var optionsPanel = new OptionsPanel(this);
							newTab.Controls.Add(optionsPanel);
							optionsPanel.Dock = DockStyle.Fill;

							m_tabMain.TabPages.Add(newTab);
						}
					}
					catch (Exception ex)
					{
						Debug.Fail(ex.ToString());
					}
				};
			}
		}
	}
}
