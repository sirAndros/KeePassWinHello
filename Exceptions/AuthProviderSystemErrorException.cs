using System;

namespace KeePassWinHello
{
    // TODO: Implement ExternalException logic
    [Serializable]
    public class AuthProviderSystemErrorException : AuthProviderException
    {
        public AuthProviderSystemErrorException() { }
        public AuthProviderSystemErrorException(string message, int errorCode) : base(message) {
            // TODO:
        }
        public AuthProviderSystemErrorException(string message) : base(message) { }
        public AuthProviderSystemErrorException(string message, Exception inner) : base(message, inner) { }
        protected AuthProviderSystemErrorException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}