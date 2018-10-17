using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Freedom.Annotations;

namespace Freedom.FullTextSearch
{
    [Serializable]
    public class PropertyMetadataDictionary<T> : Dictionary<string, PropertyMetadata<T>>
    {
        public PropertyMetadataDictionary()
        {
        }

        public PropertyMetadataDictionary(int capacity) : base(capacity)
        {
        }

        public PropertyMetadataDictionary(IEqualityComparer<string> comparer) : base(comparer)
        {
        }

        public PropertyMetadataDictionary(int capacity, IEqualityComparer<string> comparer) : base(capacity, comparer)
        {
        }

        public PropertyMetadataDictionary([NotNull] IDictionary<string, PropertyMetadata<T>> dictionary) : base(dictionary)
        {
        }

        public PropertyMetadataDictionary([NotNull] IDictionary<string, PropertyMetadata<T>> dictionary, IEqualityComparer<string> comparer) : base(dictionary, comparer)
        {
        }

        protected PropertyMetadataDictionary(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}