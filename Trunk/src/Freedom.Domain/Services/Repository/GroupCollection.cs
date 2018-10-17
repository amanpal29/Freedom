using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Freedom.Domain.Model;
using Freedom.Annotations;

namespace Freedom.Domain.Services.Repository
{
    [Serializable]
    [CollectionDataContract(Namespace = Entity.Namespace, ItemName = "Group")]
    public class GroupCollection : Dictionary<string, string>
    {
        public GroupCollection()
        {
        }

        public GroupCollection(int capacity) : base(capacity)
        {
        }

        public GroupCollection(IEqualityComparer<string> comparer) : base(comparer)
        {
        }

        public GroupCollection(int capacity, IEqualityComparer<string> comparer) : base(capacity, comparer)
        {
        }

        public GroupCollection([NotNull] IDictionary<string, string> dictionary) : base(dictionary)
        {
        }

        public GroupCollection([NotNull] IDictionary<string, string> dictionary, IEqualityComparer<string> comparer) : base(dictionary, comparer)
        {
        }

        protected GroupCollection(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
