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

        public EnviromentErrorException(string debugInfo, int errorCode) : this(debugInfo + "\nError code: 0x" + errorCode.ToString("X"))
        {
            ErrorCode = errorCode;
            Context = debugInfo;
            // TODO: Implement ExternalException logic
        }

        public int ErrorCode { get; private set; }
        public string Context { get; private set; }

        protected EnviromentErrorException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
            ErrorCode = info.GetInt32("ErrorCode");
            Context = info.GetString("Context");
        }

        public override void GetObjectData(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            info.AddValue("ErrorCode", ErrorCode);
            info.AddValue("Context", Context);

            base.GetObjectData(info, context);
        }
    }
}
