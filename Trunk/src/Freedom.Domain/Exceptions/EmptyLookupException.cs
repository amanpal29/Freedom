using System;
using System.Runtime.Serialization;
using Freedom.Extensions;

namespace Freedom.Domain.Exceptions
{
    [Serializable]
    public class EmptyLookupException : InvalidOperationException
    {
        public EmptyLookupException()
        {
        }

        public EmptyLookupException(string message)
            : base(message)
        {
        }

        public EmptyLookupException(Type entityType)
            : base($"There are no active {entityType.GetDisplayName()} entities created.")
        {
            EntityType = entityType;
        }

        public EmptyLookupException(Type entityType, string message)
            : base(message)
        {
            EntityType = entityType;
        }

        public EmptyLookupException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected EmptyLookupException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            string entityTypeName = info.GetString(nameof(EntityType));

            if (!string.IsNullOrEmpty(entityTypeName))
                EntityType = Type.GetType(entityTypeName, throwOnError: true, ignoreCase: false);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            if (EntityType != null)
                info.AddValue(nameof(EntityType), EntityType.FullName);
        }

        public Type EntityType { get; }
    }
}
