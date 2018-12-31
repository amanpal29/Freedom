using Freedom.Constraints;
using Freedom.Domain.Exceptions;
using Freedom.Domain.Model;
using Freedom.Domain.Services.Repository;
using log4net;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Freedom.Client.Services.Repository
{
    public class EntityRepositoryAutoRetryDecorator : IEntityRepository
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEntityRepository _entityRepository;

        private int _maxErrorAttempts = 3;
        private int _maxRushAttempts = 2;
        private TimeSpan _rushDelay = new TimeSpan(0, 0, 10);

        public EntityRepositoryAutoRetryDecorator(IEntityRepository entityRepository)
        {
            if (entityRepository == null)
                throw new ArgumentNullException(nameof(entityRepository));

            _entityRepository = entityRepository;
        }

        public int MaxErrorAttempts
        {
            get { return _maxErrorAttempts; }
            set
            {
                if (value < 1)
                    throw new ArgumentException("Value can't be less than 1", nameof(value));

                _maxErrorAttempts = value;
            }
        }

        public int MaxRushAttempts
        {
            get { return _maxRushAttempts; }
            set
            {
                if (value < 1)
                    throw new ArgumentException("Value can't be less than 1", nameof(value));

                _maxRushAttempts = value;
            }
        }

        public TimeSpan RushDelay
        {
            get { return _rushDelay; }
            set
            {
                if (value < TimeSpan.Zero)
                    throw new ArgumentException("Value can't be negative", nameof(value));

                _rushDelay = value;
            }
        }

        private bool ShouldRetry(CommunicationException ex)
        {
            if (ex == null)
                throw new ArgumentNullException(nameof(ex));

            HttpStatusCommunicationException httpStatus = ex as HttpStatusCommunicationException;

            if (httpStatus == null)
                return true;

            switch (httpStatus.StatusCode)
            {
                case HttpStatusCode.RequestTimeout:         // 408
                case HttpStatusCode.Conflict:               // 409
                case HttpStatusCode.UpgradeRequired:        // 426
                case HttpStatusCode.InternalServerError:    // 500
                case HttpStatusCode.BadGateway:             // 502
                case HttpStatusCode.ServiceUnavailable:     // 503
                case HttpStatusCode.GatewayTimeout:         // 504
                    return true;

                default:
                    return false;
            }
        }

        // RUSH == RetryUntilSomethingHappens
        private async Task<T> RetryUntilSomethingHappens<T>(Func<Task<T>> action, CancellationToken cancellationToken)
        {
            List<Task<T>> attempts = new List<Task<T>>();

            while (attempts.Count < MaxRushAttempts)
            {
                cancellationToken.ThrowIfCancellationRequested();

                attempts.Add(action());

                Task<T> firstCompleted = await TaskHelper.WhenAny(attempts, RushDelay, cancellationToken);

                if (firstCompleted != null)
                    return await firstCompleted;
            }

            return await await Task.WhenAny(attempts);
        }

        private async Task<T> AutoRetry<T>(Func<Task<T>> action, CancellationToken cancellationToken)
        {
            int attempts = 0;

            while (true)
            {
                attempts++;

                try
                {
                    return await RetryUntilSomethingHappens(action, cancellationToken);
                }
                catch (CommunicationException ex)
                {
                    if (!ShouldRetry(ex) || attempts >= MaxErrorAttempts)
                        throw;

                    Log.Warn("A possibly recoverable error occurred while communicating with the server, retrying...", ex);
                }

                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        #region Implementation of IEntityRepository

        public async Task<Entity> GetEntityAsync(string entityTypeName, DateTime? pointInTime,
            ResolutionGraph resolutionGraph, Constraint constraint,
            CancellationToken cancellationToken)
        {
            return await AutoRetry(() =>
                _entityRepository.GetEntityAsync(entityTypeName, pointInTime, resolutionGraph, constraint,
                    cancellationToken),
                cancellationToken);
        }

        public async Task<IEnumerable<Entity>> GetEntitiesAsync(string entityTypeName, DateTime? pointInTime,
            ResolutionGraph resolutionGraph,
            Constraint constraint, CancellationToken cancellationToken)
        {
            return await AutoRetry(() =>
                _entityRepository.GetEntitiesAsync(entityTypeName, pointInTime, resolutionGraph, constraint,
                    cancellationToken),
                cancellationToken);
        }

        public async Task<IEnumerable<Entity>> SearchAsync(string entityTypeName, string searchTerm, Constraint constraint, CancellationToken cancellationToken)
        {
            return await AutoRetry(() =>
                _entityRepository.SearchAsync(entityTypeName, searchTerm, constraint, cancellationToken),
                cancellationToken);
        }

        public async Task<int> GetCountAsync(string entityTypeName, Constraint constraint, CancellationToken cancellationToken)
        {
            return await AutoRetry(() =>
                _entityRepository.GetCountAsync(entityTypeName, constraint, cancellationToken),
                cancellationToken);
        }

        public async Task<GroupCollection> GetGroupsAsync(string entityTypeName, string keyField, AggregateFunction function, string valueField,
            Constraint constraint, CancellationToken cancellationToken)
        {
            return await AutoRetry(() =>
                _entityRepository.GetGroupsAsync(entityTypeName, keyField, function, valueField, constraint, cancellationToken),
                cancellationToken);
        }

        #endregion
    }
}
