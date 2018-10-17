using System;
using System.Runtime.Serialization;

namespace Freedom.Domain.Infrastructure.ExceptionHandling
{
    [Serializable]
    public class CanceledException : Exception
    {
        public CanceledException()
        {
        }

        public CanceledException(string message)
            : base(message)
        {
        }

        public CanceledException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected CanceledException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
