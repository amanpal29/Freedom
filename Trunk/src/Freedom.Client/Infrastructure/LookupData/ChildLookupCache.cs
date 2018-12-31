using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Freedom.Domain.Interfaces;
using Freedom.Domain.Model;

namespace Freedom.Client.Infrastructure.LookupData
{
    public class ChildLookupCache<TParent, TChild> : ILookup<TChild>
        where TChild : EntityBase
        where TParent : EntityBase
    {
        private readonly Func<TParent, IEnumerable<TChild>> _projector;
        private readonly Func<TParent, TChild, bool> _isActivePredicate;

        public ChildLookupCache(Func<TParent, IEnumerable<TChild>> projector)
        {
            if (projector == null)
                throw new ArgumentNullException(nameof(projector));

            _projector = projector;

            bool parentHasIsActive = typeof (IActiveState).IsAssignableFrom(typeof (TParent));
            bool childHasIsActive = typeof (IActiveState).IsAssignableFrom(typeof (TChild));

            if (parentHasIsActive && childHasIsActive)
            {
                _isActivePredicate = (p, c) => ((IActiveState) p).IsActive && ((IActiveState) c).IsActive;
            }
            else if (parentHasIsActive)
            {
                _isActivePredicate = (p, c) => ((IActiveState) p).IsActive;
            }
            else if (childHasIsActive)
            {
                _isActivePredicate = (p, c) => ((IActiveState) c).IsActive;
            }
        }

        public ChildLookupCache(Func<TParent, IEnumerable<TChild>> projector, Func<TParent, TChild, bool> isActivePredicate)
        {
            if (projector == null)
                throw new ArgumentNullException(nameof(projector));

            _projector = projector;
            _isActivePredicate = isActivePredicate;
        }
        
        #region Implementation of IEnumerable

        public IEnumerator<TChild> GetEnumerator()
        {
            return LookupTable<TParent>.Instance.SelectMany(_projector).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return LookupTable<TParent>.Instance.SelectMany(_projector).GetEnumerator();
        }

        #endregion

        #region Implementation of IRefreshable

        public async Task RefreshAsync()
        {
            await LookupTable<TParent>.RefreshAsync();
        }

        #endregion

        #region Implementation of ILookup<out TChild>

        public IEnumerable<TChild> GetActive(Guid? include)
        {
            foreach (TParent parent in LookupTable<TParent>.Instance)
            {
                IEnumerable<TChild> children = _projector(parent);

                if (typeof (IComparable).IsAssignableFrom(typeof (TChild)))
                    children = children.OrderBy(x => x);

                foreach (TChild child in children)
                    if (_isActivePredicate == null || _isActivePredicate(parent, child) || child.Id == include)
                        yield return child;
            }
        }

        public IEnumerable<TChild> GetActive(IEnumerable<Guid> include)
        {
            // To avoid multiple enumeration
            IEnumerable<Guid> includeCollection = include.ToList();

            foreach (TParent parent in LookupTable<TParent>.Instance)
            {
                IEnumerable<TChild> children = _projector(parent);

                if (typeof(IComparable).IsAssignableFrom(typeof(TChild)))
                    children = children.OrderBy(x => x);

                foreach (TChild child in children)
                    if (_isActivePredicate == null || _isActivePredicate(parent, child) || includeCollection.Contains(child.Id))
                        yield return child;
            }
        }

        public IEnumerable<TChild> GetActiveForParent(Guid? parentId, Guid? include)
        {
            if (parentId == null)
                return Enumerable.Empty<TChild>();

            TParent parent = LookupTable<TParent>.Instance[parentId.Value];

            if (parent == null)
                return Enumerable.Empty<TChild>();

            IEnumerable<TChild> children = _projector(parent);
            
            if (_isActivePredicate != null)
                children = children.Where(child => _isActivePredicate(parent, child) || child.Id == include);

            if (typeof(IComparable).IsAssignableFrom(typeof(TChild)))
                children = children.OrderBy(x => x);

            return children;
        }

        public IEnumerable<TChild> GetActiveForParent(Guid? parentId, IEnumerable<Guid> include)
        {
            if (parentId == null)
                return Enumerable.Empty<TChild>();

            TParent parent = LookupTable<TParent>.Instance[parentId.Value];

            if (parent == null)
                return Enumerable.Empty<TChild>();

            IEnumerable<TChild> children = _projector(parent);

            if (_isActivePredicate != null)
                children = children.Where(child => _isActivePredicate(parent, child) || include.Contains(child.Id));

            if (typeof(IComparable).IsAssignableFrom(typeof(TChild)))
                children = children.OrderBy(x => x);

            return children;
        }

        public Guid? GetDefaultId()
        {
            foreach (TParent parent in LookupTable<TParent>.Instance)
            {
                IEnumerable<TChild> children = _projector(parent);

                if (typeof (IComparable).IsAssignableFrom(typeof (TChild)))
                    children = children.OrderBy(x => x);

                foreach (TChild child in children)
                    if (_isActivePredicate == null || _isActivePredicate(parent, child))
                        return child.Id;
            }

            return null;
        }

        public Guid? GetDefaultIdForParent(Guid? parentId)
        {
            if (parentId == null)
                return GetDefaultId();

            TParent parent = LookupTable<TParent>.Instance[parentId.Value];

            if (parent != null)
            {
                IEnumerable<TChild> children = _projector(parent);

                if (typeof(IComparable).IsAssignableFrom(typeof(TChild)))
                    children = children.OrderBy(x => x);

                foreach (TChild child in children)
                    if (_isActivePredicate == null || _isActivePredicate(parent, child))
                        return child.Id;
            }

            return null;
        }

        public bool Contains(Guid id)
        {
            return LookupTable<TParent>.Instance.SelectMany(_projector).Any(child => child.Id == id);
        }

        public TChild this[Guid? key]
        {
            get
            {
                if (!key.HasValue)
                    return null;

                TChild result = LookupTable<TParent>.Instance
                    .SelectMany(_projector)
                    .FirstOrDefault(child => child.Id == key.Value);

                if (result != null)
                    return result;

                return LookupTable<TParent>.Instance
                    .SelectMany(_projector)
                    .FirstOrDefault(child => child.Id == key.Value);
            }
        }

        public IEnumerable<Type> ResolvedTypes => LookupTable<TParent>.ResolvedTypes;

        public Task EnsureRefreshedAsync(DateTime? lastModifiedDateTime)
        {
            return LookupTable<TParent>.EnsureRefreshedAsync(lastModifiedDateTime);
        }

        #endregion
    }
}
