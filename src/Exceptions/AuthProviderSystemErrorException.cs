using System;

namespace KeePassWinHello
{
    [Serializable]
    public class AuthProviderSystemErrorException : EnviromentErrorException
    {
        public AuthProviderSystemErrorException() { }
        public AuthProviderSystemErrorException(string message, int errorCode) : base(message, errorCode) { }
        public AuthProviderSystemErrorException(string message) : base(message) { }
        public AuthProviderSystemErrorException(string message, Exception inner) : base(message, inner) { }
        protected AuthProviderSystemErrorException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}