using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Freedom.Annotations;

namespace Freedom.Domain.Services.Query
{
    [Serializable]
    [CollectionDataContract(Namespace = Constants.ContractNamespace)]
    public class LastModifiedDateDictionary : Dictionary<string, DateTime>
    {
        public LastModifiedDateDictionary()
        {
        }

        public LastModifiedDateDictionary([NotNull] IDictionary<string, DateTime> dictionary)
            : base(dictionary)
        {
        }

        protected LastModifiedDateDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public DateTime? GetLastModifiedDateTime(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                throw new ArgumentNullException(nameof(typeName));

            return ContainsKey(typeName) ? this[typeName] : (DateTime?) null;
        }

        public DateTime? GetLastModifiedDateTime(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return GetLastModifiedDateTime(type.Name);
        }

        public DateTime? GetLastModifiedDateTime(IEnumerable<Type> types)
        {
            if (types == null)
                throw new ArgumentNullException(nameof(types));

            return types.Max(t => GetLastModifiedDateTime(t));
        }
    }
}
