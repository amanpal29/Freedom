using System;
using System.Collections.Generic;

namespace Freedom.Domain.Services.Query
{
    public interface IQueryDataProviderCollection : ICollection<IQueryDataProvider>
    {
        IDictionary<string, Type> ProvidedTypes { get; }
        Type GetEntityTypeByName(string entityTypeName);
        IQueryDataProvider GetProviderForType(Type type);
        IQueryDataProvider GetProviderForType(string entityTypeName);
    }
}