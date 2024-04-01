﻿using System;
using System.Diagnostics;
using System.Threading;

namespace KeePassWinHello
{
    internal sealed class KeePassWarningSuppresser : IDisposable
    {
        private readonly Thread _thread;
        private readonly HDESK _mainDesktop;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private KeePassWarningSuppresser(HDESK mainDesktop)
        {
            _mainDesktop = mainDesktop;
            _cancellationTokenSource = new CancellationTokenSource();
            _thread = new Thread(MonitorWarning)
            {
                Name = "WarningSuppressorThread",
                IsBackground = true,
            };
            _thread.Start();
        }

        public static IDisposable SuppressAllWarningWindows(HDESK mainDesktop)
        {
            return new KeePassWarningSuppresser(mainDesktop);
        }

        private void MonitorWarning()
        {
            WinAPI.SetThreadDesktop(_mainDesktop);

            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                const string MsgBoxClass = "#32770";
                try
                {
                    var msgBox = Win32Window.Find(MsgBoxClass, "KeePass");

                    // todo close only with "invalid key" message
                    if (msgBox != null)
                    {
                        msgBox.Close();
                        break;
                    }
                }
                catch (Exception e)
                {
                    Debug.Fail("Failed to find and close KeePass warning window", e.ToString());
                }
                Thread.Sleep(10);
            }
        }

        public void Dispose()
        {
            using (_cancellationTokenSource)
                _cancellationTokenSource.Cancel();

            _thread.Join();
        }
    }
}
