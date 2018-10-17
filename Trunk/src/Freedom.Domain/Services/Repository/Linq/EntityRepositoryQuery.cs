using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Freedom.Domain.Services.Repository.Linq
{
    public class EntityRepositoryQuery<TEntity> : IOrderedQueryable<TEntity>
    {
        #region Fields

        private readonly Expression _expression;
        private readonly EntityRepositoryQueryProvider _queryProvider;

        #endregion

        #region Constructors

        public EntityRepositoryQuery(IEntityRepository entityRepository)
        {
            _expression = Expression.Constant(this);
            _queryProvider = new EntityRepositoryQueryProvider(entityRepository);
        }

        public EntityRepositoryQuery(IEntityRepository entityRepository, Expression expression)
        {
            _expression = expression;
            _queryProvider = new EntityRepositoryQueryProvider(entityRepository);
        }

        #endregion

        #region Implementation of IEnumerable

        public IEnumerator<TEntity> GetEnumerator()
        {
            return _queryProvider.Execute<IEnumerable<TEntity>>(_expression).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {

            return GetEnumerator();
        }

        #endregion

        #region  Implementation of IQueryable

        public Expression Expression => _expression;

        public Type ElementType => typeof (TEntity);

        public IQueryProvider Provider => _queryProvider;

        #endregion
    }
}
