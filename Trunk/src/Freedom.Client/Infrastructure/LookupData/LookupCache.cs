using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Freedom.Domain.Interfaces;
using Freedom.Domain.Model;

namespace Freedom.Client.Infrastructure.LookupData
{
    public class LookupCache<TEntity> : ILookup<TEntity>
        where TEntity : EntityBase
    {
        private const string NoParent = "This lookup doesn't have a logical parent";

        private static readonly Func<TEntity, bool> ActiveStatePredicate =
            x => ((IActiveState) x).IsActive;

        private readonly Func<TEntity, Guid> _getParentId;
        private readonly Func<TEntity, bool> _isActivePredicate;

        public LookupCache()
            : this(null, null)
        {

        }

        public LookupCache(Func<TEntity, Guid> getParentId)
            : this(getParentId, null)
        {
            
        }

        public LookupCache(Func<TEntity, bool> isActivePredicate)
            : this(null, isActivePredicate)
        {
            
        }

        public LookupCache(Func<TEntity, Guid> getParentId, Func<TEntity, bool> isActivePredicate)
        {
            _getParentId = getParentId;
            _isActivePredicate = isActivePredicate;

            if (isActivePredicate == null && typeof (IActiveState).IsAssignableFrom(typeof (TEntity)))
                _isActivePredicate = ActiveStatePredicate;
        }

        #region Implementation of IEnumerable

        public IEnumerator<TEntity> GetEnumerator()
        {
            return LookupTable<TEntity>.Instance.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return LookupTable<TEntity>.Instance.GetEnumerator();
        }

        #endregion

        #region Implementation of IRefreshable

        public async Task RefreshAsync()
        {
            await LookupTable<TEntity>.RefreshAsync();
        }

        #endregion

        #region Implementation of ILookup<out TEntity>

        public IEnumerable<TEntity> GetActive(Guid? include)
        {
            if (_isActivePredicate == null)
                return LookupTable<TEntity>.Instance;

            if (include == null)
                return LookupTable<TEntity>.Instance.Where(_isActivePredicate);

            return LookupTable<TEntity>.Instance.Where<TEntity>(x => x.Id == include || _isActivePredicate(x));
        }

        public IEnumerable<TEntity> GetActive(IEnumerable<Guid> include)
        {
            if (_isActivePredicate == null)
                return LookupTable<TEntity>.Instance;

            if (include == null)
                return LookupTable<TEntity>.Instance.Where(_isActivePredicate);

            return LookupTable<TEntity>.Instance.Where<TEntity>(x => include.Contains(x.Id) || _isActivePredicate(x));
        }

        public IEnumerable<TEntity> GetActiveForParent(Guid? parentId, Guid? include)
        {
            if (_getParentId == null)
                throw new NotSupportedException(NoParent);

            IEnumerable<TEntity> items = LookupTable<TEntity>.Instance;

            if (parentId != null)
                items = items.Where(x => _getParentId(x) == parentId);

            if (_isActivePredicate == null)
                return items;

            if (include == null)
                return items.Where(_isActivePredicate);

            return items.Where(x => x.Id == include || _isActivePredicate(x));
        }

        public IEnumerable<TEntity> GetActiveForParent(Guid? parentId, IEnumerable<Guid> include)
        {
            if (_getParentId == null)
                throw new NotSupportedException(NoParent);

            IEnumerable<TEntity> items = LookupTable<TEntity>.Instance;

            if (parentId != null)
                items = items.Where(x => _getParentId(x) == parentId);

            if (_isActivePredicate == null)
                return items;

            if (include == null)
                return items.Where(_isActivePredicate);

            return items.Where(x => include.Contains(x.Id) || _isActivePredicate(x));
        }

        public Guid? GetDefaultId()
        {
            IEnumerable<TEntity> items = LookupTable<TEntity>.Instance;

            if (_isActivePredicate != null)
                items = items.Where(_isActivePredicate);

            TEntity defaultEntity = items.FirstOrDefault();

            return defaultEntity?.Id;
        }

        public Guid? GetDefaultIdForParent(Guid? parentId)
        {
            if (_getParentId == null)
                throw new NotSupportedException(NoParent);

            IEnumerable<TEntity> items = LookupTable<TEntity>.Instance;

            if (parentId != null)
                items = items.Where(x => _getParentId(x) == parentId);

            if (_isActivePredicate != null)
                items = items.Where(_isActivePredicate);

            TEntity defaultEntity = items.FirstOrDefault();

            return defaultEntity?.Id;
        }

        public bool Contains(Guid id)
        {
            return LookupTable<TEntity>.Instance.ContainsKey(id);
        }

        public TEntity this[Guid? key] => key == null ? null : LookupTable<TEntity>.Instance[key.Value];

        public IEnumerable<Type> ResolvedTypes => LookupTable<TEntity>.ResolvedTypes;

        public Task EnsureRefreshedAsync(DateTime? lastModifiedDateTime)
        {
            return LookupTable<TEntity>.EnsureRefreshedAsync(lastModifiedDateTime);
        }

        #endregion
    }
}
