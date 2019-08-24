using System;

namespace KeePassWinHello
{
    [Serializable]
    public class AuthProviderException : KeePassWinHelloException
    {
        public AuthProviderException() { }
        public AuthProviderException(string message) : base(message) { }
        public AuthProviderException(string message, Exception inner) : base(message, inner) { }
        protected AuthProviderException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}