using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;

namespace KeePassWinHello
{
    [StructLayout(LayoutKind.Sequential)]
    struct BOOL
    {
        public int Value;
        public bool Result { get { return Value != 0; } }

        public bool ThrowOnError(string debugInfo = "", params int[] ignoredErrors)
        {
            if (!Result)
            {
                int errorCode = Marshal.GetLastWin32Error();
                if (errorCode != 0 && (ignoredErrors == null || !ignoredErrors.Contains(errorCode)))
                    throw new EnviromentErrorException(debugInfo, errorCode);
            }

            return Result;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct HRESULT
    {
        public int Value;

        public int ThrowOnError(string debugInfo = "", params int[] ignoredErrors)
        {
            if (Value < 0 && (ignoredErrors == null || !ignoredErrors.Contains(Value)))
                throw new EnviromentErrorException(debugInfo, Value);

            return Value;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct HWND
    {
        public IntPtr Value;

        public HWND(IntPtr handle)
        {
            Value = handle;
        }

        public bool IsValid
        {
            get
            {
                return Value != IntPtr.Zero;
            }
        }

        public IntPtr ThrowOnError(string debugInfo = "", params int[] ignoredErrors)
        {
            if (!IsValid)
            {
                int errorCode = Marshal.GetLastWin32Error();
                if (ignoredErrors == null || !ignoredErrors.Contains(errorCode))
                    throw new EnviromentErrorException(debugInfo, errorCode);
            }

            return Value;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct HDESK
    {
        public IntPtr Value;

        public HDESK(IntPtr handle)
        {
            Value = handle;
        }

        public bool IsValid
        {
            get
            {
                return Value != IntPtr.Zero;
            }
        }

        public IntPtr ThrowOnError(string debugInfo = "", params int[] ignoredErrors)
        {
            if (!IsValid)
            {
                int errorCode = Marshal.GetLastWin32Error();
                if (ignoredErrors == null || !ignoredErrors.Contains(errorCode))
                    throw new EnviromentErrorException(debugInfo, errorCode);
            }

            return Value;
        }
    }
}
