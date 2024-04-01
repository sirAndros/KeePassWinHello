using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace KeePassWinHello
{
    internal sealed class UIContext : IWin32Window
    {
        public string Message { get; private set; }
        public IntPtr ParentWindowHandle { get; private set; }

        IntPtr IWin32Window.Handle { get { return ParentWindowHandle; } }

        public UIContext(string message, IntPtr windowHandle)
        {
            Message = message;
            ParentWindowHandle = windowHandle;
        }
    }

    internal sealed class UIContextManager
    {
        private readonly LinkedList<UIContext> _contexts = new LinkedList<UIContext>();
        private readonly object _lock = new object();

        public UIContext CurrentContext
        {
            get
            {
                lock (_lock)
                {
                    var node = _contexts.First;
                    return node != null ? node.Value : null;
                }
            }
        }

        public IDisposable PushContext(string message, IWin32Window parentWindow)
        {
            var context = new UIContext(message, parentWindow.Handle);
            _contexts.AddFirst(context);
            return new Disposer(this, context);
        }

        private sealed class Disposer : IDisposable
        {
            private readonly UIContextManager _contextManager;
            private readonly UIContext _context;

            public Disposer(UIContextManager contextManager, UIContext context)
            {
                _contextManager = contextManager;
                _context = context;
            }

            public void Dispose()
            {
                Debug.Assert(_contextManager._contexts.Remove(_context));
            }
        }
    }
}