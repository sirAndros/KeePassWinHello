using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace KeePassWinHello
{
    internal sealed class UIContext : IWin32Window
    {
        public string Message { get; private set; }
        public HWND ParentWindowHandle { get; private set; }

        IntPtr IWin32Window.Handle { get { return ParentWindowHandle.Value; } }

        public UIContext(string message, HWND windowHandle)
        {
            Message = message;
            ParentWindowHandle = windowHandle;
        }
    }

    internal sealed class UIContextManager
    {
        private readonly LinkedList<UIContext> _contexts = new LinkedList<UIContext>();
        private readonly object _lock = new object();
        private readonly HDESK _mainDesktop;

        public UIContextManager(HDESK mainDesktop)
        {
            _mainDesktop = mainDesktop;
        }

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

        public HDESK MainDesktop { get { return _mainDesktop; } }

        public IDisposable PushContext(string message, IWin32Window parentWindow)
        {
            var context = new UIContext(message, new HWND(parentWindow.Handle));
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
                bool removed = _contextManager._contexts.Remove(_context);
                Debug.Assert(removed);
            }
        }
    }
}