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
        private static WinHelloKeyProvider _provider;

        public const string ShortProductName = "WindowsHello";
        internal static KeyPromptForm KeyPromptForm { get; private set; }
        internal static IPluginHost Host { get; private set; }

        private System.Timers.Timer _timer;

        public override Image SmallIcon
        {
            get { return Properties.Resources.windows_hello16x16; }
        }

        public override string UpdateUrl
        {
            get { return "https://github.com/sirAndros/KeePassWinHello/raw/master/keepass.version"; }
        }

        public override bool Initialize(IPluginHost host)
		{
			if (Host != null) { Debug.Assert(false); Terminate(); }
			if (host == null) { return false; }

			_provider = new WinHelloKeyProvider();

            Host = host;
			Host.KeyProviderPool.Add(_provider);
			Host.MainWindow.FileClosingPre += FileClosingPreHandler;

			GlobalWindowManager.WindowAdded += WindowAddedHandler;

			_timer = new System.Timers.Timer(1000);
			_timer.Elapsed += ElapsedHandler;
			_timer.Start();

			return true;
		}

		public override void Terminate()
		{
			if (Host == null)
                return;

            if (_timer != null)
            {
                _timer.Stop();
                _timer.Elapsed -= ElapsedHandler;
                _timer = null; 
            }

			GlobalWindowManager.WindowAdded -= WindowAddedHandler;
            Host.MainWindow.FileClosingPre -= FileClosingPreHandler;
			Host.KeyProviderPool.Remove(_provider);

			Host = null;
		}

		private void ElapsedHandler(object sender, ElapsedEventArgs e)
		{
			_provider.ClearExpiredKeys();
		}

		/// <summary>
		/// Gets the masterkey before the database is closed.
		/// </summary>
		private void FileClosingPreHandler(object sender, FileClosingEventArgs e)
		{
			if (e == null) { Debug.Assert(false); return; }
			if (e.Cancel) { return; }

			if (e.Database != null && e.Database.MasterKey != null && WinHelloCryptProvider.IsAvailable())
			{
                _provider.CacheKeyForDB(e.Database.IOConnectionInfo.Path, e.Database.MasterKey);
			}
		}

		/// <summary>
		/// Used to modify other form when they load.
		/// </summary>
		private void WindowAddedHandler(object sender, GwmWindowEventArgs e)
		{
			var keyPromptForm = e.Form as KeyPromptForm;
			if (keyPromptForm != null)
			{
                KeyPromptForm = keyPromptForm;

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
								if (_provider.IsCachedKey(ioInfo.Path) && WinHelloCryptProvider.IsAvailable())
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
                                        if (Host.CustomConfig.GetBool(WinHelloKeyProvider.CfgAutoPrompt, true))
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
