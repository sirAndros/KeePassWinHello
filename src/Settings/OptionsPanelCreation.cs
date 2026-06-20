using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using KeePass.Forms;
using KeePass.Plugins;
using KeePassLib;
using KeePassLib.Serialization;

namespace KeePassWinHello
{
    partial class OptionsPanel
    {
        internal static void OnOptionsLoad(OptionsForm optionsForm, IPluginHost host, IKeyManager keyManager, UIContextManager uiContextManager)
        {
            AddTab(GetTabControl(optionsForm), GetTabsImageList(optionsForm), keyManager, uiContextManager, GetDatabaseItems(host));
        }

        private static void AddTab(TabControl tabMain, ImageList imageList, IKeyManager keyManager, UIContextManager uiContextManager, IList<DatabaseItem> databaseItems)
        {
            Debug.Assert(tabMain != null);
            if (tabMain == null)
                return;

            if (imageList == null)
            {
                if (tabMain.ImageList == null)
                {
                    tabMain.ImageList = new ImageList();
                    tabMain.ImageList.ColorDepth = ColorDepth.Depth32Bit;
                    tabMain.ImageList.TransparentColor = Color.Transparent;
                }

                imageList = tabMain.ImageList;
            }   

            const string iconKey = "KPWH_icon";
            imageList.Images.Add(iconKey, Properties.Resources.KPWH);
            var optionsPanel = new OptionsPanel(keyManager, uiContextManager, databaseItems);

            var newTab = new TabPage(Settings.OptionsTabName)
            {
                UseVisualStyleBackColor = true,
                ImageIndex = imageList.Images.IndexOfKey(iconKey),
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

        private static IList<DatabaseItem> GetDatabaseItems(IPluginHost host)
        {
            var result = new List<DatabaseItem>();
            var paths = new HashSet<string>(StringComparer.Ordinal);

            if (host == null)
                return result;

            try
            {
                PropertyInfo documentManagerProperty = host.MainWindow.GetType().GetProperty("DocumentManager");
                object documentManager = documentManagerProperty != null
                    ? documentManagerProperty.GetValue(host.MainWindow, null)
                    : null;
                PropertyInfo documentsProperty = documentManager != null
                    ? documentManager.GetType().GetProperty("Documents")
                    : null;
                var documents = documentsProperty != null
                    ? documentsProperty.GetValue(documentManager, null) as IEnumerable
                    : null;

                if (documents != null)
                {
                    foreach (object document in documents)
                    {
                        try
                        {
                            AddDatabaseItem(result, paths, GetDocumentConnectionInfo(document));
                        }
                        catch (Exception)
                        {
                            // Ignore one incompatible document and continue discovering the others.
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Fall back to the active database below for KeePass versions with different internals.
            }

            if (host.Database != null)
                AddDatabaseItem(result, paths, host.Database.IOConnectionInfo);

            return result;
        }

        private static IOConnectionInfo GetDocumentConnectionInfo(object document)
        {
            if (document == null)
                return null;

            PropertyInfo databaseProperty = document.GetType().GetProperty("Database");
            var database = databaseProperty != null
                ? databaseProperty.GetValue(document, null) as PwDatabase
                : null;
            if (database != null && database.IOConnectionInfo != null &&
                !String.IsNullOrEmpty(database.IOConnectionInfo.Path))
                return database.IOConnectionInfo;

            PropertyInfo lockedIocProperty = document.GetType().GetProperty("LockedIoc");
            return lockedIocProperty != null
                ? lockedIocProperty.GetValue(document, null) as IOConnectionInfo
                : null;
        }

        private static void AddDatabaseItem(ICollection<DatabaseItem> result,
            ISet<string> paths, IOConnectionInfo connectionInfo)
        {
            if (connectionInfo == null || String.IsNullOrEmpty(connectionInfo.Path) ||
                !paths.Add(connectionInfo.Path))
                return;

            string displayName;
            try
            {
                MethodInfo getDisplayNameMethod = connectionInfo.GetType().GetMethod("GetDisplayName", Type.EmptyTypes);
                displayName = getDisplayNameMethod != null
                    ? getDisplayNameMethod.Invoke(connectionInfo, null) as string
                    : null;
            }
            catch (Exception)
            {
                displayName = null;
            }

            if (String.IsNullOrEmpty(displayName))
                displayName = connectionInfo.Path;

            result.Add(new DatabaseItem(connectionInfo.Path, displayName));
        }
    }
}
