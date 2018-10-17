using System;
using System.Collections.Generic;

namespace Freedom.Domain.Interfaces
{
    public interface ILookupRegistry
    {
        ILookup<T> GetLookup<T>() where T : class;

        ILookup<object> GetLookup(Type lookupType); 

        void Register<T>(ILookup<T> instance) where T : class;

        IEnumerable<Type> RegisteredTypes { get; }

        IEnumerable<Type> ResolvedTypes { get; } 
    }
}
