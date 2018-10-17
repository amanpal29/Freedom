using System;
using System.Runtime.Serialization;

namespace Freedom.Domain.Exceptions
{
    [Serializable]
    public class InsufficientPermissionException : Exception
    {
        public InsufficientPermissionException()
        {
        }

        public InsufficientPermissionException(string message)
            : base(message)
        {
        }

        public InsufficientPermissionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected InsufficientPermissionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
