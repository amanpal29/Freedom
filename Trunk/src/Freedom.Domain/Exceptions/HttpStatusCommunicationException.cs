using System;
using System.Net;
using System.Runtime.Serialization;

namespace Freedom.Domain.Exceptions
{
    [Serializable]
    public class HttpStatusCommunicationException : CommunicationException
    {
        public HttpStatusCommunicationException()
        {
        }

        public HttpStatusCommunicationException(string message)
            : base(message)
        {
        }

        public HttpStatusCommunicationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public HttpStatusCommunicationException(HttpStatusCode statusCode, string message)
            : base(message)
        {
            StatusCode = statusCode;
        }

        public HttpStatusCommunicationException(HttpStatusCode statusCode, string message, Exception inner)
            : base(message, inner)
        {
            StatusCode = statusCode;
        }

        protected HttpStatusCommunicationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

            StatusCode = (HttpStatusCode) info.GetInt32(nameof(StatusCode));
            UniqueId = info.GetValue(nameof(UniqueId), typeof(Guid)) as Guid?;
        }

        public HttpStatusCode StatusCode { get;  }

        public Guid? UniqueId { get; set; }

        public object Content { get; set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(StatusCode), (int) StatusCode);
            info.AddValue(nameof(UniqueId), UniqueId);
        }
    }
}
