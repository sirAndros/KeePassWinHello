using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KeePassWinHello.Utilities
{
    internal static class ErrorHandler
    {
        public static DialogResult ShowError(this UIContext uiContext, Exception ex, string description = null)
        {
            string message = ex.Message;
            var messageType = MessageBoxIcon.Warning;

            var bugReportUrl = "https://github.com/sirAndros/KeePassWinHello/issues/new?labels=bug&template=bug_report.md";

            var ourException = ex as KeePassWinHelloException;
            if (ourException == null || !ourException.IsPresentable)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Something went wrong. Please report the issue in our Github repository by pressing the \"Help\" button with the following technical info.");
                sb.Append(ex);

                message = sb.ToString();
                messageType = MessageBoxIcon.Error;

                var envException = ex as EnviromentErrorException;
                if (envException != null)
                {
                    var title = String.Format("[{0} — 0x{1:X}]", envException.Context, envException.ErrorCode);
                    bugReportUrl += "&title=" + Uri.EscapeDataString(title);
                }
            }

            if (!String.IsNullOrEmpty(description))
                message = description + Environment.NewLine + message;

            //todo TaskDialog
            return MessageBox.Show(uiContext, message, Settings.ProductName, MessageBoxButtons.OK, messageType,
                    MessageBoxDefaultButton.Button1, 0, bugReportUrl, "Report the issue");
        }
    }
}
