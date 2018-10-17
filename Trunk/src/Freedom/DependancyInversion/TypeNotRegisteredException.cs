using System;
using System.Runtime.Serialization;

namespace Freedom.DependancyInversion
{
    [Serializable]
    public class TypeNotRegisteredException : Exception
    {
        public TypeNotRegisteredException()
        {
        }

        public TypeNotRegisteredException(string message)
            : base(message)
        {
        }

        public TypeNotRegisteredException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected TypeNotRegisteredException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
