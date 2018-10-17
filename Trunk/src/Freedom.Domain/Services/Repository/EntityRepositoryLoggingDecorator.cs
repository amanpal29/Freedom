using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Freedom.Domain.Model;
using Freedom.Constraints;
using log4net;

namespace Freedom.Domain.Services.Repository
{
    public class EntityRepositoryLoggingDecorator : IEntityRepository
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEntityRepository _innerRepository;

        public EntityRepositoryLoggingDecorator(IEntityRepository innerRepository)
        {
            if (innerRepository == null)
                throw new ArgumentNullException(nameof(innerRepository));

            _innerRepository = innerRepository;
        }
        
        #region Implementation of IEntityRepository

        public async Task<Entity> GetEntityAsync(string entityTypeName, DateTime? pointInTime, ResolutionGraph resolutionGraph, Constraint constraint, CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Entity result = await _innerRepository.GetEntityAsync(entityTypeName, pointInTime, resolutionGraph, constraint, cancellationToken);

            Log.Info($"GetEntityAsync of type {entityTypeName} as of {pointInTime?.ToString("r") ?? "now"} returned {result?.ToString() ?? "(null)"} in {stopwatch.ElapsedMilliseconds} ms.");

            return result;
        }

        public async Task<IEnumerable<Entity>> GetEntitiesAsync(string entityTypeName, DateTime? pointInTime, ResolutionGraph resolutionGraph, Constraint constraint, CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            IEnumerable<Entity> results =
                await _innerRepository.GetEntitiesAsync(entityTypeName, pointInTime, resolutionGraph, constraint, cancellationToken);

            ICollection<Entity> result = results as ICollection<Entity> ?? new List<Entity>(results);

            Log.Info($"GetEntitiesAsync of type {entityTypeName} as of {pointInTime?.ToString("r") ?? "now"} returned {result.Count} in {stopwatch.ElapsedMilliseconds} ms.");

            return result;
        }

        public async Task<IEnumerable<Entity>> SearchAsync(string entityTypeName, string searchTerm, Constraint constraint, CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            IEnumerable<Entity> results = await _innerRepository.SearchAsync(entityTypeName, searchTerm, constraint, cancellationToken);

            ICollection<Entity> result = results as ICollection<Entity> ?? new List<Entity>(results);

            Log.Info($"SearchAsync of type {entityTypeName} for '{searchTerm}' returned {result.Count} in {stopwatch.ElapsedMilliseconds} ms.");

            return result;
        }

        public async Task<int> GetCountAsync(string entityTypeName, Constraint constraint, CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            int result = await _innerRepository.GetCountAsync(entityTypeName, constraint, cancellationToken);

            Log.Info($"GetCountAsync found {result} entities of type {entityTypeName} in {stopwatch.ElapsedMilliseconds} ms.");

            return result;
        }

        public async Task<GroupCollection> GetGroupsAsync(string entityTypeName, string keyField, AggregateFunction function, string valueField, Constraint constraint, CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            GroupCollection result = await _innerRepository.GetGroupsAsync(entityTypeName, keyField, function, valueField, constraint, cancellationToken);

            Log.Info($"GetGroups of {entityTypeName} grouped by {keyField} with {function}({valueField}) returned {result.Count} groups in {stopwatch.ElapsedMilliseconds} ms");

            return result;
        }

        #endregion
    }
}
