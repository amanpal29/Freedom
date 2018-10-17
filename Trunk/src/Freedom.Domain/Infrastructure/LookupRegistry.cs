using System;
using System.Collections.Generic;
using Freedom.Domain.Interfaces;

namespace Freedom.Domain.Infrastructure
{
    public class LookupRegistry : ILookupRegistry
    {
        private readonly Dictionary<Type, ILookup<object>> _lookups = new Dictionary<Type, ILookup<object>>();

        public ILookup<TEntity> GetLookup<TEntity>() where TEntity : class
        {
            return (ILookup<TEntity>) _lookups[typeof (TEntity)];
        }

        public ILookup<object> GetLookup(Type type)
        {
            return _lookups[type];
        }

        public void Register<T>(ILookup<T> instance) where T : class
        {
            Type lookupType = typeof (T);

            if (_lookups.ContainsKey(typeof (T)))
            {
                _lookups[lookupType] = instance;
            }
            else
            {
                _lookups.Add(lookupType, instance);
            }
        }

        public IEnumerable<Type> RegisteredTypes => _lookups.Keys;

        public IEnumerable<Type> ResolvedTypes
        {
            get
            {
                HashSet<Type> result = new HashSet<Type>();

                foreach (ILookup<object> lookup in _lookups.Values)
                {
                    foreach (Type resolvedType in lookup.ResolvedTypes)
                    {
                        result.Add(resolvedType);
                    }
                }

                return result;
            }
        }
    }
}