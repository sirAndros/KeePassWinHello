using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeePassWinHello
{
    class RevocationProcessor : IObserver<OptionsPanel.ChangedOptions>, IDisposable
    {
        private readonly KeyManager _keyManager;
        private readonly IDisposable _cancellationToken;

        public RevocationProcessor(KeyManager keyManager)
        {
            _keyManager = keyManager;
            _cancellationToken = OptionsPanel.Subscribe(this);
        }

        public void OnNext(OptionsPanel.ChangedOptions value)
        {
            bool isApplicable = value.ChangeType == OptionsPanel.OptionsChangeType.RevokeAll
                             || value.ChangeType == OptionsPanel.OptionsChangeType.Settings_Enabled
                                && !Settings.Instance.Enabled;
            if (!isApplicable)
                return;

            _keyManager.RevokeAll();
        }

        public void Dispose()
        {
            _cancellationToken.Dispose();
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }
    }
}
