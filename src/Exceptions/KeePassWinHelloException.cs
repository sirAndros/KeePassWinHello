using System;
using System.Runtime.Serialization;

namespace KeePassWinHello
{
    [Serializable]
    public class KeePassWinHelloException : Exception
    {
        public bool IsPresentable { get; protected set; }

        public KeePassWinHelloException() { }
        public KeePassWinHelloException(string message) : base(message) { }
        public KeePassWinHelloException(string message, Exception inner) : base(message, inner) { }

        protected KeePassWinHelloException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            IsPresentable = info.GetBoolean("IsPresentable");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("IsPresentable", IsPresentable);
        }
    }
}
