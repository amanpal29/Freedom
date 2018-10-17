using System;
using System.Runtime.Serialization;

namespace Freedom.Domain.Exceptions
{
    [Serializable]
    public class UnsupportedCommandException : InvalidOperationException
    {
        public UnsupportedCommandException()
        {
        }

        public UnsupportedCommandException(string message)
            : base(message)
        {
        }

        public UnsupportedCommandException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected UnsupportedCommandException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
