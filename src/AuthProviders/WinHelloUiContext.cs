using System;

namespace KeePassWinHello
{
    internal sealed class WinHelloUIContext : IDisposable
    {
        [ThreadStatic]
        public static WinHelloUIContext Current;

        public string Message { get; private set; }
        public IntPtr ParentWindowHandle { get; private set; }

        private WinHelloUIContext(string message, IntPtr windowHandle)
        {
            Message = message;
            ParentWindowHandle = windowHandle;
        }

        public static WinHelloUIContext With(string message, IntPtr windowHandle)
        {
            var result = new WinHelloUIContext(message, windowHandle);
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