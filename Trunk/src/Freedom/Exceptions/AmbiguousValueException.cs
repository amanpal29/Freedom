using System;
using System.Runtime.Serialization;

namespace Freedom.Exceptions
{
    /// <summary>
    /// Thrown when parsing a composite value, such as an address or name
    /// into it's component parts, and the system is unsure if the parsing is correct
    /// </summary>
    [Serializable]
    public class AmbiguousValueException : Exception
    {
        public AmbiguousValueException()
        {
        }

        public AmbiguousValueException(string message)
            : base(message)
        {
        }

        public AmbiguousValueException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected AmbiguousValueException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
