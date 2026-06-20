using System;
using System.Runtime.Serialization;

namespace KeePassWinHello
{
    [Serializable]
    public class KeyStorageException : KeePassWinHelloException
    {
        public override bool IsPresentable { get { return true; } }

        public KeyStorageException(string message) : base(message) { }
        public KeyStorageException(string message, Exception inner) : base(message, inner) { }
        protected KeyStorageException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
