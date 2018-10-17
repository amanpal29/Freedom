using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Freedom.Domain.Exceptions;
using Freedom.Domain.Model;
using Freedom.Extensions;

namespace Freedom.Domain.Interfaces
{
    public interface ILookup<out TEntity> : IEnumerable<TEntity>, IRefreshable 
        where TEntity : class
    {
        IEnumerable<TEntity> GetActive(Guid? include);

        IEnumerable<TEntity> GetActive(IEnumerable<Guid> include);
            
        IEnumerable<TEntity> GetActiveForParent(Guid? parentId, Guid? include);

        IEnumerable<TEntity> GetActiveForParent(Guid? parentId, IEnumerable<Guid> include);
            
        Guid? GetDefaultId();

        Guid? GetDefaultIdForParent(Guid? parentId);

        bool Contains(Guid id);

        TEntity this[Guid? key] { get; }

        IEnumerable<Type> ResolvedTypes { get; }

        Task EnsureRefreshedAsync(DateTime? lastModifiedDateTime);
    }

    public static class Lookup<T>
        where T : class
    {
        public static ILookup<T> Instance => IoC.Get<ILookupRegistry>().GetLookup<T>();

        public static IEnumerable<T> GetActive()
        {
            return Instance.GetActive((Guid?)null);
        }

        public static IEnumerable<T> GetActive(Guid? include)
        {
            return Instance.GetActive(include);
        }

        public static IEnumerable<T> GetActive(IEnumerable<Guid> include)
        {
            return Instance.GetActive(include);
        }

        public static IEnumerable<T> GetActiveForParent(Guid? parentId)
        {
            return Instance.GetActiveForParent(parentId, (Guid?) null);
        }

        public static IEnumerable<T> GetActiveForParent(Guid? parentId, Guid? include)
        {
            return Instance.GetActiveForParent(parentId, include);
        }

        public static IEnumerable<T> GetActiveForParent(Guid? parentId, IEnumerable<Guid> include)
        {
            return Instance.GetActiveForParent(parentId, include);
        }

        public static IEnumerable<T> GetActiveForParents(IEnumerable<Guid> parentIds)
        {
            return parentIds.SelectMany(id => Instance.GetActiveForParent(id, (Guid?) null));
        }

        public static IEnumerable<T> GetActiveForParents(IEnumerable<Guid> parentIds, IEnumerable<Guid> include)
        {
            return GetActiveForParents(parentIds)
                .Union(include.Where(Instance.Contains).Select(x => Instance[x]))
                .Distinct();
        }

        public static T GetDefault()
        {
            ILookup<T> instance = Instance;

            return instance[instance.GetDefaultId()];
        }

        public static T GetDefaultForParent(Guid? parentId)
        {
            ILookup<T> instance = Instance;

            return instance[instance.GetDefaultIdForParent(parentId)];
        }

        public static Guid? GetDefaultId()
        {
            return Instance.GetDefaultId();
        }

        public static Guid? GetDefaultIdForParent(Guid? parentId)
        {
            return Instance.GetDefaultIdForParent(parentId);
        }

        public static Guid GetRequiredDefaultId()
        {
            Guid? defaultId = Instance.GetDefaultId();

            if (defaultId == null)
                throw new EmptyLookupException(typeof(T));

            return defaultId.Value;
        }

        public static Guid GetRequiredDefaultIdForParent(Guid? parentId)
        {
            Guid? defaultId = Instance.GetDefaultIdForParent(parentId);

            if (defaultId == null)
                throw new EmptyLookupException(typeof(T));

            return defaultId.Value;
        }

        public static bool Contains(Guid id)
        {
            return Instance.Contains(id);
        }

        public static T Get(Guid? key)
        {
            if (!key.HasValue)
                return null;

            T result = Instance[key];

            if (result == null)
                throw new KeyNotFoundException(
                    $"The key {key} was not found in the lookup for {typeof (T).Name.ToDisplayName()}");

            return result;
        }

        public static T TryGet(Guid? key)
        {
            return Instance[key];
        }

        public static IEnumerable<T> GetMany(ICollection<Guid> ids)
        {
            return ids.Select(x => Get(x));
        }

        public static string GetDescription(Guid? id)
        {
            if (id == null || !Contains(id))
                return string.Empty;

            LookupBase entity = Instance[id] as LookupBase;

            return entity != null ? entity.Description : string.Empty;
        }

        public static bool Contains(Guid? id)
        {
            return id.HasValue && Instance.Contains(id.Value);
        }

        public static bool Contains(T entity)
        {
            EntityBase entityBase = entity as EntityBase;

            return entityBase != null && Instance.Contains(entityBase.Id);
        }

        public static int Count(Func<T, bool> predicate)
        {
            return Instance.Count(predicate);
        }

        public static IEnumerable<T> Where(Func<T, bool> predicate)
        {
            return Instance.Where(predicate);
        }       

        public static Guid? FindDescription(string description)
        {
            if (!typeof(LookupBase).IsAssignableFrom(typeof(T)))
                throw new InvalidOperationException("FindDescription is only available on Lookup<LookupBase>.");

            if (string.IsNullOrEmpty(description))
                return null;

            foreach (LookupBase entity in Instance.GetActive((Guid?) null).OfType<LookupBase>())
                if (description == entity.Description)
                    return entity.Id;

            foreach (LookupBase entity in Instance.OfType<LookupBase>())
                if (string.Equals(description, entity.Description, StringComparison.InvariantCultureIgnoreCase))
                    return entity.Id;

            return null;
        }
    }
}
