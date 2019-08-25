using System;

namespace KeePassWinHello
{
    [Serializable]
    public class AuthProviderUserCancelledException : AuthProviderException
    {
        public AuthProviderUserCancelledException() : this("Operation was canceled by user.") { }
        public AuthProviderUserCancelledException(string message) : base(message) { }
        public AuthProviderUserCancelledException(string message, Exception inner) : base(message, inner) { }
        protected AuthProviderUserCancelledException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}