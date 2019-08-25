using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KeePassWinHello.Utilities
{
    internal static class ErrorHandler
    {
        public static DialogResult ShowError(Exception ex, string description = null)
        {
            string message = ex.Message;
            var messageType = MessageBoxIcon.Warning;

            var ourException = ex as KeePassWinHelloException;
            if (ourException == null || !ourException.IsPresentable)
            {
                var sb = new StringBuilder();
                sb.AppendLine("[TBD] Something went wrong. Write an issue.");
                sb.AppendLine("Technical info:");
                sb.Append(ex);

                message = sb.ToString();
                messageType = MessageBoxIcon.Error;
            }

            if (!String.IsNullOrEmpty(description))
                message = description + Environment.NewLine + message;

            return MessageBox.Show(AuthProviderUIContext.Current, message, Settings.ProductName, MessageBoxButtons.OK, messageType);
        }
    }
}
