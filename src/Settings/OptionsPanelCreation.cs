﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using KeePass.Forms;

namespace KeePassWinHello
{
    partial class OptionsPanel
    {
        internal static void OnOptionsLoad(OptionsForm optionsForm, IKeyManager keyManager)
        {
            AddTab(GetTabControl(optionsForm), GetTabsImageList(optionsForm), keyManager);
        }

        private static void AddTab(TabControl tabMain, ImageList imageList, IKeyManager keyManager)
        {
            Debug.Assert(tabMain != null);
            if (tabMain == null)
                return;

            if (imageList == null)
            {
                if (tabMain.ImageList == null)
                    tabMain.ImageList = new ImageList();
                imageList = tabMain.ImageList;
            }

            var imageIndex = imageList.Images.Add(Properties.Resources.windows_hello16x16, Color.Transparent);
            var optionsPanel = new OptionsPanel(keyManager);

            var newTab = new TabPage(Settings.OptionsTabName)
            {
                UseVisualStyleBackColor = true,
                ImageIndex = imageIndex
            };

            newTab.Controls.Add(optionsPanel);
            optionsPanel.Dock = DockStyle.Fill;

            tabMain.TabPages.Add(newTab);
            tabMain.Multiline = false;
        }

        private static TabControl GetTabControl(OptionsForm optionsForm)
        {
            return optionsForm.Controls.Find("m_tabMain", true).FirstOrDefault() as TabControl;
        }

        private static ImageList GetTabsImageList(OptionsForm optionsForm)
        {
            var m_ilIconsField = optionsForm.GetType().GetField("m_ilIcons", BindingFlags.Instance | BindingFlags.NonPublic);
            if (m_ilIconsField == null)
                return null;
            return m_ilIconsField.GetValue(optionsForm) as ImageList;
        }
    }
}
