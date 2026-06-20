using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace KeePassWinHello
{
    internal sealed class UIContext : IWin32Window
    {
        public string Message { get; private set; }
        public HWND ParentWindowHandle { get; private set; }

        private readonly Control _uiThreadControl;
        private Form _promptParentWindow;

        IntPtr IWin32Window.Handle { get { return ParentWindowHandle.Value; } }

        public UIContext(string message, HWND windowHandle)
        {
            Message = message;
            ParentWindowHandle = windowHandle;
        }

        public UIContext(string message, IWin32Window parentWindow)
            : this(message, new HWND(parentWindow.Handle))
        {
            _uiThreadControl = parentWindow as Control;
        }

        public HWND PromptParentWindowHandle
        {
            get
            {
                if (Win32Window.IsVisibleAndNotMinimized(ParentWindowHandle))
                    return ParentWindowHandle;

                return EnsurePromptParentWindowHandle();
            }
        }

        public void DisposePromptParentWindow()
        {
            if (ShouldInvokeOnUIThread())
            {
                _uiThreadControl.Invoke(new MethodInvoker(DisposePromptParentWindowOnCurrentThread));
                return;
            }

            DisposePromptParentWindowOnCurrentThread();
        }

        private void DisposePromptParentWindowOnCurrentThread()
        {
            if (_promptParentWindow != null)
            {
                _promptParentWindow.Dispose();
                _promptParentWindow = null;
            }
        }

        private HWND EnsurePromptParentWindowHandle()
        {
            if (ShouldInvokeOnUIThread())
            {
                return (HWND)_uiThreadControl.Invoke(new Func<HWND>(EnsurePromptParentWindowHandleOnCurrentThread));
            }

            return EnsurePromptParentWindowHandleOnCurrentThread();
        }

        private HWND EnsurePromptParentWindowHandleOnCurrentThread()
        {
            if (_promptParentWindow == null || _promptParentWindow.IsDisposed)
                _promptParentWindow = PromptParentForm.CreateCentered();

            return new HWND(_promptParentWindow.Handle);
        }

        private bool ShouldInvokeOnUIThread()
        {
            return _uiThreadControl != null
                && !_uiThreadControl.IsDisposed
                && _uiThreadControl.InvokeRequired;
        }

        private sealed class PromptParentForm : Form
        {
            private PromptParentForm()
            {
                FormBorderStyle = FormBorderStyle.None;
                Opacity = 0.01;
                ShowInTaskbar = false;
                Size = new Size(1, 1);
                StartPosition = FormStartPosition.Manual;
                Text = Settings.ProductName;
            }

            protected override bool ShowWithoutActivation
            {
                get { return true; }
            }

            public static PromptParentForm CreateCentered()
            {
                PromptParentForm form = new PromptParentForm();
                Rectangle workingArea = Screen.FromPoint(Cursor.Position).WorkingArea;
                form.Location = new Point(
                    workingArea.Left + (workingArea.Width - form.Width) / 2,
                    workingArea.Top + (workingArea.Height - form.Height) / 2);
                form.Show();
                return form;
            }
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
            var context = new UIContext(message, parentWindow);
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
                _context.DisposePromptParentWindow();
            }
        }
    }
}
