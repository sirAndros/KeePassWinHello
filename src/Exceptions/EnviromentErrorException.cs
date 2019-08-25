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

        public EnviromentErrorException(string debugInfo, int errorCode) : this(debugInfo + "!!! " + errorCode.ToString("X"))
        {
            // TODO: Implement ExternalException logic
        }

        protected EnviromentErrorException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
