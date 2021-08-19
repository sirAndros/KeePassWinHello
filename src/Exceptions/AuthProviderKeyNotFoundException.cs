using System;

namespace KeePassWinHello
{
    [Serializable]
    public class AuthProviderKeyNotFoundException : AuthProviderException // TODO: consider inherit from AuthProviderInvalidKeyException
    {
        public override bool IsPresentable { get { return true; } }

        public AuthProviderKeyNotFoundException(string message = "The given key does not exist.") : base(message) { }
        public AuthProviderKeyNotFoundException(string message, Exception inner) : base(message, inner) { }
        protected AuthProviderKeyNotFoundException() { }
        protected AuthProviderKeyNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}