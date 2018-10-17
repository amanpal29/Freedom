using System;
using System.Runtime.Serialization;

namespace Freedom.Domain.Exceptions
{
    [Serializable]
    public class PasswordComplexityException : Exception
    {
        public PasswordComplexityException()
        {
        }

        public PasswordComplexityException(string message)
            : base(message)
        {
        }

        public PasswordComplexityException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public PasswordComplexityException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
