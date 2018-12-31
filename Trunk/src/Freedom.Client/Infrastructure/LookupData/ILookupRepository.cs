using System;
using System.Collections.Generic;
using Freedom.Domain.Model;

namespace Freedom.Client.Infrastructure.LookupData
{
    internal interface ILookupRepository
    {
        DateTime? LoadLookupData<TEntity>(ICollection<TEntity> lookupTable) where TEntity : EntityBase;

        bool SaveLookupData<TEntity>(DateTime refreshedDateTime, IEnumerable<TEntity> entities) where TEntity : EntityBase;
    }
}
