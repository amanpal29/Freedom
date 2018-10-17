using System;
using System.Runtime.Serialization;

namespace Freedom.DependancyInversion
{
    [Serializable]
    public class ConstructionFailedException : Exception
    {
        public ConstructionFailedException()
        {
        }

        public ConstructionFailedException(string message)
            : base(message)
        {
        }

        public ConstructionFailedException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected ConstructionFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
