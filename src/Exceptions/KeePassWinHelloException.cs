using System;
using System.Runtime.Serialization;

namespace KeePassWinHello
{
    [Serializable]
    public class KeePassWinHelloException : Exception
    {
        public virtual bool IsPresentable { get { return false; } }

        public KeePassWinHelloException() { }
        public KeePassWinHelloException(string message) : base(message) { }
        public KeePassWinHelloException(string message, Exception inner) : base(message, inner) { }
        protected KeePassWinHelloException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
