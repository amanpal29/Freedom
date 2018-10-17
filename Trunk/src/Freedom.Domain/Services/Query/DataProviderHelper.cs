using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Freedom.Domain.Model;
using Freedom.Domain.Services.Repository;
using Freedom;
using Freedom.Annotations;
using Freedom.Linq;

namespace Freedom.Domain.Services.Query
{
    public static class DataProviderHelper
    {
        private static readonly MethodInfo QueryableSelectMethod =
            ExpressionHelper.GetMethod<IQueryable<object>>(x => x.Select((Expression<Func<object, object>>)null));

        private static readonly MethodInfo QueryableSelectManyMethod =
            ExpressionHelper.GetMethod<IQueryable<object>>(
                x => x.SelectMany((Expression<Func<object, IEnumerable<object>>>)null));

        private static readonly MethodInfo DbContextSetMethod =
            ExpressionHelper.GetMethod<DbContext>(x => x.Set<object>());

        private static readonly PropertyInfo EntityBaseIdProperty =
            typeof(EntityBase).GetProperty(nameof(EntityBase.Id));

        private static readonly MethodInfo GuidCollectionContainsMethod =
            typeof(ICollection<Guid>).GetMethod(nameof(ICollection<Guid>.Contains), new[] { typeof(Guid) });

        private static readonly MethodInfo QueryableWhereMethod =
            ExpressionHelper.GetMethod<IQueryable<object>>(x => x.Where((Expression<Func<object, bool>>)null));

        public struct TypedQuery
        {
            public TypedQuery([NotNull] IQueryable query, [NotNull] Type elementType) : this()
            {
                Query = query;
                ElementType = elementType;
            }

            public IQueryable Query { get; }

            public Type ElementType { get; }
        }

        public static async Task ResolveAsync(TypedQuery source, ResolutionGraphNode nodes,
            CancellationToken cancellationToken)
        {
            ResolutionGraph defaultGraph = GetDefaultResolutionGraph(source.ElementType);

            if (defaultGraph != null)
            {
                if (nodes == null)
                    nodes = defaultGraph.Nodes;
                else
                    nodes.AddRange(defaultGraph);
            }

            if (nodes == null) return;

            foreach (ResolutionGraphNode node in nodes)
            {
                TypedQuery childQuery = BuildSelectQuery(source, node.Name);

                await childQuery.Query.LoadAsync(cancellationToken);

                await ResolveAsync(childQuery, node, cancellationToken);
            }
        }

        private static ResolutionGraph GetDefaultResolutionGraph(Type entityType)
        {
            FieldInfo field = entityType.GetField("DefaultResolutionGraph",
                BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

            if (field == null || field.FieldType != typeof(ResolutionGraph))
                return null;

            return (ResolutionGraph)field.GetValue(null);
        }

        private static TypedQuery BuildSelectQuery(TypedQuery source, [NotNull] string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException(nameof(propertyName));

            PropertyInfo property = source.ElementType.GetProperty(propertyName);

            if (property == null)
                throw new ArgumentException(
                    $"The navigation property {propertyName} was not found on type {source.ElementType.FullName}",
                    nameof(propertyName));

            Type enumerableType = TypeHelper.GetClosedEnumerableType(property.PropertyType);

            Type elementType = enumerableType != null ? enumerableType.GetGenericArguments()[0] : property.PropertyType;

            MethodInfo selectMethod = enumerableType != null
                ? QueryableSelectManyMethod.MakeGenericMethod(source.ElementType, elementType)
                : QueryableSelectMethod.MakeGenericMethod(source.ElementType, elementType);

            ParameterExpression parameter = Expression.Parameter(source.ElementType);

            Expression memberAccess = Expression.MakeMemberAccess(parameter, property);

            if (enumerableType != null)
                memberAccess = Expression.Convert(memberAccess, enumerableType);

            LambdaExpression lamba = Expression.Lambda(memberAccess, parameter);

            MethodCallExpression queryableSelect = Expression.Call(null, selectMethod, source.Query.Expression,
                Expression.Quote(lamba));

            return new TypedQuery(source.Query.Provider.CreateQuery(queryableSelect), elementType);
        }

        public static TypedQuery BuildMutipleEntityQuery(DbContext context, Type entityType, ICollection<Guid> ids)
        {
            ParameterExpression parameter = Expression.Parameter(entityType);

            MemberExpression memberAccess = Expression.MakeMemberAccess(parameter, EntityBaseIdProperty);

            ConstantExpression constant = Expression.Constant(ids, typeof(ICollection<Guid>));

            MethodCallExpression predicate = Expression.Call(constant, GuidCollectionContainsMethod, memberAccess);

            return BuildTypedQuery(context, entityType, predicate);
        }

        private static TypedQuery BuildTypedQuery(DbContext context, Type entityType, Expression predicate)
        {
            IQueryable dbSet = GetTypedDbSet(context, entityType);

            ConstantExpression dbSetExpression = Expression.Constant(dbSet,
                typeof(IQueryable<>).MakeGenericType(entityType));

            LambdaExpression lambda = Expression.Lambda(predicate, ExpressionHelper.GetParameters(predicate));

            MethodCallExpression whereCall = Expression.Call(null,
                QueryableWhereMethod.MakeGenericMethod(entityType),
                dbSetExpression, Expression.Quote(lambda));

            IQueryable query = dbSet.Provider.CreateQuery(whereCall);

            return new TypedQuery(query, entityType);
        }

        private static IQueryable GetTypedDbSet(DbContext context, Type entityType)
        {
            MethodInfo genericMethod = DbContextSetMethod.MakeGenericMethod(entityType);

            return (IQueryable)genericMethod.Invoke(context, null);
        }
    }
}
