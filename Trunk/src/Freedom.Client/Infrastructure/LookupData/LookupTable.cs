using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Freedom.Domain.Model;
using Freedom.Domain.Services.Repository;

namespace Freedom.Client.Infrastructure.LookupData
{
    internal class LookupTable<TEntity> : IList<TEntity>, IDictionary<Guid, TEntity>
        where TEntity : EntityBase
    {
        private const string CollectionIsReadOnly = "Collection is read-only.";

        // ReSharper disable StaticFieldInGenericType
        // These are valid uses of a static field in a generic type.

        private static ResolutionGraph _resolutionGraph;
        private static Task _currentRefresh;

        // ReSharper enable StaticFieldInGenericType

        private readonly List<TEntity> _entities;
        private readonly Dictionary<Guid, TEntity> _dictionary;

        #region Constructors

        protected LookupTable(DateTime refreshDateTime, IEnumerable<TEntity> entities)
        {
            RefreshedDateTime = refreshDateTime;

            _entities = new List<TEntity>(entities);

            if (typeof(IComparable).IsAssignableFrom(typeof(TEntity)))
                _entities.Sort();

            _dictionary = new Dictionary<Guid, TEntity>();

            foreach (TEntity entity in _entities)
                _dictionary[entity.Id] = entity;
        }

        #endregion

        #region Singleton Instance

        public static LookupTable<TEntity> Instance { get; private set; }

        public static ResolutionGraph ResolutionGraph
        {
            get { return _resolutionGraph; }
            set
            {
                if (_resolutionGraph == value) return;
                _resolutionGraph = value;
                Instance = null;
            }
        }

        public static IEnumerable<Type> ResolvedTypes
            => _resolutionGraph?.GetResolvedTypes(typeof (TEntity))
               ?? new[] {typeof (TEntity)};

        public static Task RefreshAsync()
        {
            Task currentRefresh = _currentRefresh;

            if (currentRefresh != null)
                return currentRefresh;

            return _currentRefresh = InternalRefreshAsync().ContinueWith(x => _currentRefresh = null);
        }

        public static async Task EnsureRefreshedAsync(DateTime? lastModifiedDateTime)
        {
            if (lastModifiedDateTime == null)
            {
                Instance = new LookupTable<TEntity>(DateTime.MinValue, Enumerable.Empty<TEntity>());
            }
            else
            {
                if (Instance == null)
                    TryLoadFromRepository();

                if (Instance == null || Instance.RefreshedDateTime < lastModifiedDateTime)
                    await RefreshAsync();
            }
        }

        private static async Task InternalRefreshAsync()
        {
            DateTime refreshDateTime = DateTime.UtcNow;

            IEntityRepository entityRepository = IoC.Get<IEntityRepository>();

            IEnumerable<Entity> baseCollection = await entityRepository.GetEntitiesAsync(
                typeof (TEntity).Name, null, _resolutionGraph, null, CancellationToken.None);

            List<TEntity> entities = baseCollection.Cast<TEntity>().ToList();

            Instance = new LookupTable<TEntity>(refreshDateTime, entities);

            SaveToRepository();
        }

        public static bool IsLoaded => Instance != null;

        internal static bool SaveToRepository()
        {
            ILookupRepository lookupRepository = IoC.TryGet<ILookupRepository>();

            if (lookupRepository == null || Instance == null)
                return false;

            lookupRepository.SaveLookupData(Instance.RefreshedDateTime, Instance._entities);

            return true;
        }

        internal static DateTime? TryLoadFromRepository()
        {
            ILookupRepository lookupRepository = IoC.TryGet<ILookupRepository>();

            if (lookupRepository == null)
                return null;

            List<TEntity> entities = new List<TEntity>();

            DateTime? cachedDateTime = lookupRepository.LoadLookupData(entities);

            if (cachedDateTime != null)
                Instance = new LookupTable<TEntity>(cachedDateTime.Value, entities);

            return cachedDateTime;
        }

        #endregion

        #region Properties

        public DateTime RefreshedDateTime { get; }

        #endregion

        #region Implementation of IEnumerable

        public IEnumerator<TEntity> GetEnumerator()
        {
            return _entities.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _entities.GetEnumerator();
        }

        IEnumerator<KeyValuePair<Guid, TEntity>> IEnumerable<KeyValuePair<Guid, TEntity>>.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        #endregion

        #region Implementation of ICollection<TEntity>

        public void Add(TEntity item)
        {
            throw new NotSupportedException(CollectionIsReadOnly);
        }

        public void Clear()
        {
            throw new NotSupportedException(CollectionIsReadOnly);
        }

        public bool Contains(TEntity item)
        {
            return _entities.Contains(item);
        }

        public void CopyTo(TEntity[] array, int arrayIndex)
        {
            _entities.CopyTo(array, arrayIndex);
        }

        public int Count => _entities.Count;

        public bool IsReadOnly => true;

        public bool Remove(TEntity item)
        {
            throw new NotSupportedException(CollectionIsReadOnly);
        }

        #endregion

        #region Implementation of IList<TEntity>

        public int IndexOf(TEntity item)
        {
            return _entities.IndexOf(item);
        }

        public void Insert(int index, TEntity item)
        {
            throw new NotSupportedException(CollectionIsReadOnly);
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException(CollectionIsReadOnly);
        }

        public TEntity this[int index]
        {
            get { return _entities[index]; }
            set { throw new NotSupportedException(CollectionIsReadOnly); }
        }

        #endregion

        #region Implementation of ICollection<KeyValuePair<Guid,TEntity>>

        public void Add(KeyValuePair<Guid, TEntity> item)
        {
            throw new NotSupportedException(CollectionIsReadOnly);
        }

        public bool Contains(KeyValuePair<Guid, TEntity> item)
        {
            return ((ICollection<KeyValuePair<Guid, TEntity>>) _dictionary).Contains(item);
        }

        public void CopyTo(KeyValuePair<Guid, TEntity>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<Guid, TEntity>>) _dictionary).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<Guid, TEntity> item)
        {
            throw new NotSupportedException(CollectionIsReadOnly);
        }

        #endregion

        #region Implementation of IDictionary<Guid,TEntity>

        public bool ContainsKey(Guid key)
        {
            return _dictionary.ContainsKey(key);
        }

        public void Add(Guid key, TEntity value)
        {
            throw new NotSupportedException(CollectionIsReadOnly);
        }

        public bool Remove(Guid key)
        {
            throw new NotSupportedException(CollectionIsReadOnly);
        }

        public bool TryGetValue(Guid key, out TEntity value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public TEntity this[Guid key]
        {
            get { return _dictionary.ContainsKey(key) ? _dictionary[key] : null; }
            set { throw new NotSupportedException(CollectionIsReadOnly); }
        }

        public ICollection<Guid> Keys => _dictionary.Keys;

        public ICollection<TEntity> Values => _entities;

        #endregion
    }
}
