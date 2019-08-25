using System;
using System.Windows.Forms;

namespace KeePassWinHello
{
    internal sealed class AuthProviderUIContext : IDisposable, IWin32Window
    {
        [ThreadStatic]
        public static AuthProviderUIContext Current;

        public string Message { get; private set; }
        public IntPtr ParentWindowHandle { get; private set; }

        IntPtr IWin32Window.Handle { get { return ParentWindowHandle; } }

        private AuthProviderUIContext(string message, IntPtr windowHandle)
        {
            Message = message;
            ParentWindowHandle = windowHandle;
        }

        public static AuthProviderUIContext With(string message, IntPtr windowHandle)
        {
            var result = new AuthProviderUIContext(message, windowHandle);
            Current = result;
            return result;
        }

        public void Dispose()
        {
#pragma warning disable S2696 // Instance members should not write to "static" fields
            Current = null;
#pragma warning restore S2696 // Instance members should not write to "static" fields
        }
    }
}