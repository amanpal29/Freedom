using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Freedom.Domain.Model;
using Freedom.Constraints;

namespace Freedom.Domain.Services.Repository
{
    public interface IEntityRepository
    {
        Task<Entity> GetEntityAsync(string entityTypeName, DateTime? pointInTime, ResolutionGraph resolutionGraph, Constraint constraint, CancellationToken cancellationToken);

        Task<IEnumerable<Entity>> GetEntitiesAsync(string entityTypeName, DateTime? pointInTime, ResolutionGraph resolutionGraph, Constraint constraint, CancellationToken cancellationToken);

        Task<IEnumerable<Entity>> SearchAsync(string entityTypeName, string searchTerm, Constraint constraint, CancellationToken cancellationToken);

        Task<int> GetCountAsync(string entityTypeName, Constraint constraint, CancellationToken cancellationToken);

        Task<GroupCollection> GetGroupsAsync(string entityTypeName, string keyField, AggregateFunction function, string valueField, Constraint constraint, CancellationToken cancellationToken);
    }
}
