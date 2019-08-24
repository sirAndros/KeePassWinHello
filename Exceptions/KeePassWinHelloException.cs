using System;

namespace KeePassWinHello
{
    [Serializable]
    public class KeePassWinHelloException : Exception
    {
        public bool IsPresentable { get; protected set; }

        public KeePassWinHelloException() { }
        public KeePassWinHelloException(string message) : base(message) { }
        public KeePassWinHelloException(string message, Exception inner) : base(message, inner) { }
        protected KeePassWinHelloException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
