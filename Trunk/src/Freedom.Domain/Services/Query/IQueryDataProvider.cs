using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Freedom.Domain.Model;
using Freedom.Domain.Services.Repository;
using Freedom.Constraints;

namespace Freedom.Domain.Services.Query
{
    public interface IQueryDataProvider
    {
        IEnumerable<Type> ProvidedTypes { get; }

        Task<IEnumerable<Entity>> GetEntitiesAsync(Type entityType, DateTime? pointInTime,
            ResolutionGraph resolutionGraph, Constraint constraint, CancellationToken cancellationToken);

        Task<int> GetCountAsync(Type entityType, Constraint constraint, CancellationToken cancellationToken);

        Task<GroupCollection> GetGroupsAsync(Type entityType, string keyField, AggregateFunction function,
            string valueField, Constraint constraint, CancellationToken cancellationToken);

        Task<IEnumerable<Entity>> SearchAsync(Type entityType, string searchTerm, Constraint constraint,
            CancellationToken cancellationToken);
    }
}
