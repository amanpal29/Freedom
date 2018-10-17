using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Freedom.Domain.Model;
using Freedom.Domain.Services.Repository.Linq;
using Freedom.Constraints;

namespace Freedom.Domain.Services.Repository
{
    public static class EntityRepositoryLinqExtensions
    {
        public static IQueryable<TEntity> Get<TEntity>(this IEntityRepository entityRepository)
            where TEntity : Entity
        {
            return new EntityRepositoryQuery<TEntity>(entityRepository);
        }

        public static async Task<TEntity> FindAsync<TEntity>(this IEntityRepository entityRepository, Guid primaryKey)
            where TEntity : AggregateRoot
        {
            Constraint constraint = new EqualConstraint(nameof(AggregateRoot.Id), primaryKey);

            return await entityRepository.GetEntityAsync(
                typeof(TEntity).Name, null, null, constraint, CancellationToken.None) as TEntity;
        }

        public static async Task<TEntity> FindAsync<TEntity>(this IEntityRepository entityRepository, Guid primaryKey, CancellationToken cancellationToken)
            where TEntity : AggregateRoot
        {
            Constraint constraint = new EqualConstraint(nameof(AggregateRoot.Id), primaryKey);

            return await entityRepository.GetEntityAsync(
                typeof (TEntity).Name, null, null, constraint, cancellationToken) as TEntity;
        }
    }
}
