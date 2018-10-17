using System;
using System.Runtime.Serialization;

namespace Freedom.Exceptions
{
    [Serializable]
    public class UnsupportedQueryException : Exception
    {
        public UnsupportedQueryException()
        {
        }

        public UnsupportedQueryException(string message)
            : base(message)
        {
        }

        public UnsupportedQueryException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected UnsupportedQueryException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
