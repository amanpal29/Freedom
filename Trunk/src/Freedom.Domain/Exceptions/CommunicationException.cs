using System;
using System.Runtime.Serialization;

namespace Freedom.Domain.Exceptions
{
    [Serializable]
    public class CommunicationException : Exception
    {
        public CommunicationException()
        {
        }

        public CommunicationException(string message) : base(message)
        {
        }

        public CommunicationException(string message, Exception inner) : base(message, inner)
        {
        }

        protected CommunicationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
