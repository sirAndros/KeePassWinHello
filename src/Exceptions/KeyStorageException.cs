using System;

namespace KeePassWinHello
{
    [Serializable]
    public class KeyStorageException : KeePassWinHelloException
    {
        public override bool IsPresentable { get { return true; } }

        public KeyStorageException(string message) : base(message) { }
        public KeyStorageException(string message, Exception inner) : base(message, inner) { }
        protected KeyStorageException() { }
        protected KeyStorageException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
