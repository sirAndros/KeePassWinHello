using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace AdysTech.CredentialManager
{
    internal static class CredentialExtensions
    {
        internal static NetworkCredential ToNetworkCredential(this Credential cred)
        {
            if (cred == null)
            {
                return null;
            }

            string username = string.Empty;
            string domain = string.Empty;

            var passwd = cred.CredentialBlob;

            if (!string.IsNullOrEmpty(cred.UserName))
            {
                var user = cred.UserName;
                var userBuilder = new StringBuilder(cred.UserName.Length + 2);
                var domainBuilder = new StringBuilder(cred.UserName.Length + 2);

                var returnCode = NativeCode.CredUIParseUserName(user, userBuilder, userBuilder.Capacity, domainBuilder, domainBuilder.Capacity);
                var lastError = Marshal.GetLastWin32Error();

                //assuming invalid account name to be not meeting condition for CredUIParseUserName
                //"The name must be in UPN or down-level format, or a certificate"
                if (returnCode == NativeCode.CredentialUIReturnCodes.InvalidAccountName)
                {
                    userBuilder.Append(user);
                }
                else if (returnCode != 0)
                {
                    throw new Win32Exception(lastError, String.Format("CredUIParseUserName throw an error (Error code: {0})", lastError));
                }

                username = userBuilder.ToString();
                domain = domainBuilder.ToString();
            }
            return new NetworkCredential(username, passwd, domain);
        }
    }
}
