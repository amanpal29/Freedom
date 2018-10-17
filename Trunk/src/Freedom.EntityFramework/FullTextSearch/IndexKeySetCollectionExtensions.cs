using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Freedom.FullTextSearch
{
    public static class IndexKeySetCollectionExtensions
    {
        public static void AddChanged<TEntity, TDigest>(this IndexKeySetCollection indexKeySetCollection, DbContext context, Func<TEntity, Guid?> keySelector)
            where TEntity : class
        {
            IEnumerable<Guid> keysFromChangedEntities = context.ChangeTracker.Entries<TEntity>()
                .Where(entry => entry.State != EntityState.Unchanged)
                .Select(entry => entry.Entity)
                .Select(keySelector)
                .Where(key => key != null)
                .Cast<Guid>();

            using (IEnumerator<Guid> enumerator = keysFromChangedEntities.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                    return;

                ICollection<Guid> keySet = indexKeySetCollection.Ensure<TDigest>();

                do
                {
                    keySet.Add(enumerator.Current);
                } while (enumerator.MoveNext());
            }
        }

        public static void AddChanged<TEntity, TDigest>(this IndexKeySetCollection indexKeySetCollection, DbContext context, Func<TEntity, IEnumerable<Guid>> keysSelector)
            where TEntity : class
        {
            IEnumerable<Guid> keysFromChangedEntities = context.ChangeTracker.Entries<TEntity>()
                .Where(entry => entry.State != EntityState.Unchanged)
                .Select(entry => entry.Entity)
                .SelectMany(keysSelector);

            using (IEnumerator<Guid> enumerator = keysFromChangedEntities.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                    return;

                ICollection<Guid> keySet = indexKeySetCollection.Ensure<TDigest>();

                do
                {
                    keySet.Add(enumerator.Current);
                } while (enumerator.MoveNext());
            }
        }
    }
}
