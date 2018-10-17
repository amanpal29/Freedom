using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Freedom.Exceptions
{
    [Serializable]
    public class ServerException : Exception
    {
        public ServerException()
        {
        }

        public ServerException(string message)
            : base(message)
        {
        }

        public ServerException(string message, Guid uniqueId)
            : base(message)
        {
            UniqueId = uniqueId;
        }

        public ServerException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected ServerException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            UniqueId = new Guid(info.GetString(nameof(UniqueId)));
        }

        public Guid UniqueId { get; set; }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(UniqueId), UniqueId.ToString());
        }
    }
}
