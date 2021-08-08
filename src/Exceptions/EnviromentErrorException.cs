using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeePassWinHello
{
    [Serializable]
    public class EnviromentErrorException : KeePassWinHelloException
    {
        public EnviromentErrorException() { }
        public EnviromentErrorException(string message) : base(message) { }
        public EnviromentErrorException(string message, Exception inner) : base(message, inner) { }

        public EnviromentErrorException(string debugInfo, int errorCode) : this(debugInfo + "\nError code: " + errorCode.ToString("X"))
        {
            ErrorCode = errorCode;
            // TODO: Implement ExternalException logic
        }

        public int ErrorCode { get; private set; }

        protected EnviromentErrorException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
            ErrorCode = info.GetInt32("ErrorCode");
        }

        public override void GetObjectData(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            info.AddValue("ErrorCode", ErrorCode);
            base.GetObjectData(info, context);
        }
    }
}
