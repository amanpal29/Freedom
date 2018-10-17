using System;
using System.Runtime.Serialization;

namespace Freedom.Domain.Exceptions
{
    [Serializable]
    public class NoConfigurationLoadedException : Exception
    {
        public NoConfigurationLoadedException ()
        {
        }

        public NoConfigurationLoadedException (string message)
            : base(message)
        {
        }

        public NoConfigurationLoadedException (string message, Exception inner)
            : base(message, inner)
        {
        }

        protected NoConfigurationLoadedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
