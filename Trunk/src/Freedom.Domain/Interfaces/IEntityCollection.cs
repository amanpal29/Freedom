using System;
using System.Collections.Generic;

namespace Freedom.Domain.Interfaces
{
    public interface IEntityCollection<TEntity> : ICollection<TEntity>
    {
        void Add(Guid id);
        bool Contains(Guid id);
        bool Remove(Guid id);
        IEnumerable<Guid> Keys { get; }
    }
}
