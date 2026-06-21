using System;

namespace KeePassWinHello
{
    [Serializable]
    public class InvalidProtectedKeyException : KeePassWinHelloException
    {
        public override bool IsPresentable { get { return true; } }

        public InvalidProtectedKeyException()
            : this("The cached key for this database is invalid and has been removed.") { }
        public InvalidProtectedKeyException(string message) : base(message) { }
        public InvalidProtectedKeyException(string message, Exception inner) : base(message, inner) { }
        protected InvalidProtectedKeyException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
