using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Freedom.Domain.Infrastructure.ExceptionHandling;
using Freedom.Domain.Model;
using Freedom.Constraints;

namespace Freedom.Domain.Services.Repository
{
    public class EntityRepositoryManualRetryDecorator : IEntityRepository
    {
        private readonly IEntityRepository _entityRepository;

        public EntityRepositoryManualRetryDecorator(IEntityRepository entityRepository)
        {
            _entityRepository = entityRepository;
        }

        public Task<Entity> GetEntityAsync(string entityTypeName, DateTime? pointInTime, ResolutionGraph resolutionGraph, Constraint constraint,
            CancellationToken cancellationToken)
        {
            return IoC.Get<IExceptionHandlerService>().RetryAsync(
                    ct => _entityRepository.GetEntityAsync(entityTypeName, pointInTime, resolutionGraph, constraint, ct),
                    cancellationToken);
        }

        public Task<IEnumerable<Entity>> GetEntitiesAsync(string entityTypeName, DateTime? pointInTime, ResolutionGraph resolutionGraph,
            Constraint constraint, CancellationToken cancellationToken)
        {
            return IoC.Get<IExceptionHandlerService>().RetryAsync(
                ct => _entityRepository.GetEntitiesAsync(entityTypeName, pointInTime, resolutionGraph, constraint, ct),
                cancellationToken);
        }

        public Task<IEnumerable<Entity>> SearchAsync(string entityTypeName, string searchTerm, Constraint constraint, CancellationToken cancellationToken)
        {
            return IoC.Get<IExceptionHandlerService>().RetryAsync(
                ct => _entityRepository.SearchAsync(entityTypeName, searchTerm, constraint, ct),
                cancellationToken);
        }

        public Task<int> GetCountAsync(string entityTypeName, Constraint constraint, CancellationToken cancellationToken)
        {
            return IoC.Get<IExceptionHandlerService>().RetryAsync(
                ct => _entityRepository.GetCountAsync(entityTypeName, constraint, ct), 
                cancellationToken);
        }

        public Task<GroupCollection> GetGroupsAsync(string entityTypeName, string keyField, AggregateFunction function, string valueField,
            Constraint constraint, CancellationToken cancellationToken)
        {
            return IoC.Get<IExceptionHandlerService>().RetryAsync(
                ct => _entityRepository.GetGroupsAsync(entityTypeName, keyField, function, valueField, constraint, ct), 
                cancellationToken);
        }
    }
}
