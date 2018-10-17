using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Freedom.Domain.Services.Query
{
    public class QueryDataProviderCollection : Collection<IQueryDataProvider>, IQueryDataProviderCollection
    {
        private readonly Dictionary<string, IQueryDataProvider> _providerForType = new Dictionary<string, IQueryDataProvider>(StringComparer.OrdinalIgnoreCase);

        public QueryDataProviderCollection()
        {
            ProvidedTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        }

        public Type GetEntityTypeByName(string entityTypeName)
        {
            return ProvidedTypes.ContainsKey(entityTypeName) ? ProvidedTypes[entityTypeName] : null;
        }

        public IQueryDataProvider GetProviderForType(Type type)
        {
            if (!ProvidedTypes.ContainsKey(type.Name) || ProvidedTypes[type.Name] != type)
                return null;

            return _providerForType[type.Name];
        }
        
        public IQueryDataProvider GetProviderForType(string entityTypeName)
        {
            return _providerForType.ContainsKey(entityTypeName) ? _providerForType[entityTypeName] : null;
        }

        public IDictionary<string, Type> ProvidedTypes { get; }

        protected override void InsertItem(int index, IQueryDataProvider item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            List<Type> providedTypes = new List<Type>(item.ProvidedTypes);

            if (providedTypes.Any(t => ProvidedTypes.ContainsKey(t.Name)))
            {
                IEnumerable<string> types = providedTypes
                    .Where(t => ProvidedTypes.ContainsKey(t.Name))
                    .Select(t => t.Name);

                throw new ArgumentException(
                    $"A QueryDataProvider has already been registered for the following types: {string.Join(", ", types)}",
                    nameof(item));
            }

            base.InsertItem(index, item);

            foreach (Type providedType in providedTypes)
            {
                ProvidedTypes[providedType.Name] = providedType;
                _providerForType[providedType.Name] = item;
            }
        }

        protected override void RemoveItem(int index)
        {
            throw new NotSupportedException("Removing QueryDataProviders is not supported");
        }

        protected override void SetItem(int index, IQueryDataProvider item)
        {
            if (index == Count)
                InsertItem(index, item);
            else
                throw new NotSupportedException("Replacing QueryDataProviders is not supported");
        }
    }
}
