using System;
using System.Runtime.Serialization;

namespace Freedom.BackgroundWorker
{
    [Serializable]
    public class JobQueueException : Exception
    {
        public JobQueueException()
        {
        }

        public JobQueueException(string message)
            : base(message)
        {
        }

        public JobQueueException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected JobQueueException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
