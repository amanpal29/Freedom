using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Freedom.Domain.Model;
using Freedom.Domain.Services.Query;
using Freedom.Constraints;

namespace Freedom.Domain.Services.Repository
{
    public class QueryDataProviderEntityRepository : IEntityRepository
    {
        private readonly IQueryDataProviderCollection _dataProviders = IoC.Get<IQueryDataProviderCollection>();

        #region Implementation of IEntityRepository

        public async Task<Entity> GetEntityAsync(string entityTypeName, DateTime? pointInTime,
            ResolutionGraph resolutionGraph, Constraint constraint, CancellationToken cancellationToken)
        {
            IQueryDataProvider dataProvider = _dataProviders.GetProviderForType(entityTypeName);

            if (dataProvider == null)
                throw new NotSupportedException($"Querying entities of type {entityTypeName} is not supported.");

            Type entityType = _dataProviders.GetEntityTypeByName(entityTypeName);

            IEnumerable<Entity> results =
                await dataProvider.GetEntitiesAsync(entityType, null, null, constraint, cancellationToken);

            return results.FirstOrDefault();
        }

        public async Task<IEnumerable<Entity>> GetEntitiesAsync(string entityTypeName, DateTime? pointInTime,
            ResolutionGraph resolutionGraph, Constraint constraint, CancellationToken cancellationToken)
        {
            IQueryDataProvider dataProvider = _dataProviders.GetProviderForType(entityTypeName);

            if (dataProvider == null)
                throw new NotSupportedException($"Querying entities of type {entityTypeName} is not supported.");

            Type entityType = _dataProviders.GetEntityTypeByName(entityTypeName);

            return await dataProvider.GetEntitiesAsync(entityType, pointInTime,
                resolutionGraph, constraint, cancellationToken);
        }

        public async Task<IEnumerable<Entity>> SearchAsync(string entityTypeName, string searchTerm,
            Constraint constraint, CancellationToken cancellationToken)
        {
            IQueryDataProvider dataProvider = _dataProviders.GetProviderForType(entityTypeName);

            if (dataProvider == null)
                throw new NotSupportedException($"Querying entities of type {entityTypeName} is not supported.");

            Type entityType = _dataProviders.GetEntityTypeByName(entityTypeName);

            return await dataProvider.SearchAsync(entityType, searchTerm, constraint, cancellationToken);
        }

        public async Task<int> GetCountAsync(string entityTypeName, Constraint constraint,
            CancellationToken cancellationToken)
        {
            IQueryDataProvider dataProvider = _dataProviders.GetProviderForType(entityTypeName);

            if (dataProvider == null)
                throw new NotSupportedException($"Querying entities of type {entityTypeName} is not supported.");

            Type entityType = _dataProviders.GetEntityTypeByName(entityTypeName);

            return await dataProvider.GetCountAsync(entityType, constraint, cancellationToken);
        }

        public async Task<GroupCollection> GetGroupsAsync(string entityTypeName, string keyField,
            AggregateFunction function, string valueField, Constraint constraint, CancellationToken cancellationToken)
        {
            IQueryDataProvider dataProvider = _dataProviders.GetProviderForType(entityTypeName);

            if (dataProvider == null)
                throw new NotSupportedException($"Querying entities of type {entityTypeName} is not supported.");

            Type entityType = _dataProviders.GetEntityTypeByName(entityTypeName);

            return await dataProvider.GetGroupsAsync(entityType, keyField,
                function, valueField, constraint, cancellationToken);

        }

        #endregion
    }
}
