using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Freedom.Domain.Exceptions
{
    [Serializable]
    public class ConfidentialAccessDeniedException : Exception
    {
        public ConfidentialAccessDeniedException()
        {
        }

        public ConfidentialAccessDeniedException(IEnumerable<Guid> requestedIds, string message)
            : base(message)
        {
            RequestedIds = requestedIds;
        }

        public ConfidentialAccessDeniedException(IEnumerable<Guid> requestedIds, string message, Exception innerException)
            : base(message, innerException)
        {
            RequestedIds = requestedIds;
        }

        protected ConfidentialAccessDeniedException(IEnumerable<Guid> requestedIds, SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            RequestedIds = requestedIds;
        }

        public IEnumerable<Guid> RequestedIds { get; }
    }
}
