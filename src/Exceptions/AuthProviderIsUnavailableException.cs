using System;

namespace KeePassWinHello
{
    [Serializable]
    public class AuthProviderIsUnavailableException : AuthProviderException
    {
        public AuthProviderIsUnavailableException() : this("Authentication provider is not available.") { }
        public AuthProviderIsUnavailableException(string message) : base(message) { }
        public AuthProviderIsUnavailableException(string message, Exception inner) : base(message, inner) { }
        protected AuthProviderIsUnavailableException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}