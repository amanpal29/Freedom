using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Freedom.Domain.Services.Repository.Linq
{
    internal class EntityRepositoryQueryProvider : IAsyncQueryProvider
    {
        private readonly IEntityRepository _entityRepository;

        public EntityRepositoryQueryProvider(IEntityRepository entityRepository)
        {
            _entityRepository = entityRepository;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            Type elementType = TypeHelper.GetElementType(expression.Type);

            Type queryType = typeof (EntityRepositoryQuery<>).MakeGenericType(elementType);

            try
            {
                return (IQueryable) Activator.CreateInstance(queryType, _entityRepository, expression);
            }
            catch (TargetInvocationException targetInvocationException)
            {
                throw targetInvocationException.InnerException;
            }
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new EntityRepositoryQuery<TElement>(_entityRepository, expression);
        }

        public object Execute(Expression expression)
        {
            EntityRepositoryQueryExecutor executor = new EntityRepositoryQueryExecutor(_entityRepository, expression);

            return executor.Execute();
        }

        public Task<object> ExecuteAsync(Expression expression)
        {
            EntityRepositoryQueryExecutor executor = new EntityRepositoryQueryExecutor(_entityRepository, expression);

            return executor.ExecuteAsync();
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return (TResult) Execute(expression);
        }

        public async Task<TResult> ExecuteAsync<TResult>(Expression expression)
        {
            return (TResult) await ExecuteAsync(expression);
        }
    }
}
