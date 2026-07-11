using System;

namespace KeePassWinHello
{
    [Serializable]
    public class AuthProviderInvalidTicketException : AuthProviderException
    {
        public override bool IsPresentable { get { return true; } }

        public AuthProviderInvalidTicketException(string message) : base(message) { }
        public AuthProviderInvalidTicketException(string message, Exception inner) : base(message, inner) { }
        protected AuthProviderInvalidTicketException() { }
        protected AuthProviderInvalidTicketException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
