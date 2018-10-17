using System;
using System.Runtime.Serialization;

namespace Freedom.Domain.Exceptions
{
    [Serializable]
    public class SequenceEmptyException : Exception
    {

        public SequenceEmptyException()
        {
        }

        public SequenceEmptyException(string message)
            : base(message)
        {
        }

        public SequenceEmptyException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected SequenceEmptyException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}
