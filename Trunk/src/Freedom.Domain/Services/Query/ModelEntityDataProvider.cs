using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Threading.Tasks;
using Freedom.Domain.Model;
using Freedom.Domain.Services.Repository;
using Freedom.ComponentModel;
using Freedom.Constraints;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using Freedom.Domain.Infrastructure.ExceptionHandling;
using Freedom.Linq;

namespace Freedom.Domain.Services.Query
{
    public class ModelEntityDataProvider : IQueryDataProvider
    {
        public IEnumerable<Type> ProvidedTypes
        {
            get
            {
                MetadataWorkspace metadata = IoC.Get<MetadataWorkspace>();

                Assembly assembly = typeof (EntityBase).Assembly;

                return metadata.GetItems<EntityType>(DataSpace.CSpace)
                    .Select(et => assembly.GetType(et.FullName))
                    .Where(t => !t.IsAbstract && !ConfidentialAttribute.IsDefined(t));
            }
        }

        private static Expression<Func<Entity, string>> BuildGroupKeySelectorExpression(Type entityType, string keyField)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(Entity), "x");

            Expression convertedParameter = Expression.Convert(parameter, entityType);

            Expression memberExpression = ExpressionHelper.BuildMemberAccessForPath(convertedParameter, keyField);

            memberExpression = ExpressionHelper.ConvertToString(memberExpression);

            Expression<Func<Entity, string>> keySelectorExpression =
                Expression.Lambda<Func<Entity, string>>(memberExpression, parameter);

            return keySelectorExpression;
        }

        public async Task<IEnumerable<Entity>> GetEntitiesAsync(Type entityType, DateTime? pointInTime,
            ResolutionGraph resolutionGraph, Constraint constraint, CancellationToken cancellationToken)
        {
            try
            {
                using (FreedomLocalContext context = IoC.Get<FreedomLocalContext>())
                {
                    context.Configuration.ProxyCreationEnabled = false;
                    context.Configuration.LazyLoadingEnabled = false;

                    DbContextConstraintVisitor visitor = new DbContextConstraintVisitor(context);

                    IQueryable<EntityBase> query = visitor.BuildQuery<EntityBase>(entityType.Name, constraint);

                    List<EntityBase> result = await query.ToListAsync(cancellationToken);

                    HashSet<Guid> ids = new HashSet<Guid>(result.Select(x => x.Id));

                    DataProviderHelper.TypedQuery baseQuery = DataProviderHelper.BuildMutipleEntityQuery(context, entityType, ids);

                    await DataProviderHelper.ResolveAsync(baseQuery, resolutionGraph?.Nodes, cancellationToken);

                    return result;
                }
            }
            catch (Exception exception)
            {
                IoC.Get<IExceptionHandlerService>().HandleException(exception);

                throw;
            }
        }

        public async Task<int> GetCountAsync(Type entityType, Constraint constraint, CancellationToken cancellationToken)
        {
            try
            {
                PageConstraint pageConstraint = constraint as PageConstraint;

                if (pageConstraint != null)
                    constraint = pageConstraint.InnerConstraint;

                using (FreedomLocalContext context = IoC.Get<FreedomLocalContext>())
                {
                    context.Configuration.LazyLoadingEnabled = false;
                    context.Configuration.ProxyCreationEnabled = false;

                    DbContextConstraintVisitor visitor = new DbContextConstraintVisitor(context);

                    IQueryable<Entity> query = visitor.BuildQuery<Entity>(entityType.Name, constraint);

                    return await query.CountAsync(cancellationToken);
                }
            }
            catch (Exception exception)
            {
                IoC.Get<IExceptionHandlerService>().HandleException(exception);

                throw;
            }
        }

        public async Task<GroupCollection> GetGroupsAsync(Type entityType, string keyField, AggregateFunction function, string valueField,
         Constraint constraint, CancellationToken cancellationToken)
        {
            try
            {
                if (function != AggregateFunction.Count)
                    throw new ArgumentException(
                        $"Only AggregateFunction.Count is supported when getting groups on entities of type '{entityType.Name}'",
                        nameof(function));

                PageConstraint pageConstraint = constraint as PageConstraint;

                if (pageConstraint != null)
                    constraint = pageConstraint.InnerConstraint;

                using (FreedomLocalContext context = IoC.Get<FreedomLocalContext>())
                {
                    DbContextConstraintVisitor visitor = new DbContextConstraintVisitor(context);

                    IQueryable<Entity> baseQuery = visitor.BuildQuery<Entity>(entityType.Name, constraint);

                    Expression<Func<Entity, string>> keySelectorExpression =
                        BuildGroupKeySelectorExpression(entityType, keyField);

                    Dictionary<string, string> dictionary = await baseQuery.GroupBy(keySelectorExpression)
                        .ToDictionaryAsync(grp => grp.Key, grp => grp.Count().ToString(), cancellationToken);

                    return new GroupCollection(dictionary);
                }
            }
            catch (Exception exception)
            {
                IoC.Get<IExceptionHandlerService>().HandleException(exception);

                throw;
            }
        }

        public Task<IEnumerable<Entity>> SearchAsync(Type entityType, string searchTerm, Constraint constraint, CancellationToken cancellationToken)
        {
            throw new NotSupportedException($"Searching on entities of type '{entityType.Name}' is not suppored.");
        }
    }
}
