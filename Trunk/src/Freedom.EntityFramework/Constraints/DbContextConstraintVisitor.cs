using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Freedom.Extensions;
using Freedom.Linq;

namespace Freedom.Constraints
{
    public class DbContextConstraintVisitor
    {
        #region Static Fields

        private static readonly MethodInfo ToStringMethod = typeof(object).GetMethod("ToString", new Type[0]);

        private static readonly MethodInfo StringStartsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });

        private static readonly MethodInfo StringContainsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });

        private static readonly MethodInfo ListOfGuidsContainsMethod =
            typeof (List<Guid>).GetMethod("Contains", new[] {typeof (Guid)});

        private static readonly MethodInfo QueryableWhereMethod =
            ExpressionHelper.GetMethod<IQueryable<object>>(x => x.Where((Expression<Func<object, bool>>)null));

        private static readonly MethodInfo QueryableSelectMethod =
            ExpressionHelper.GetMethod<IQueryable<object>>(x => x.Select((Expression<Func<object, object>>)null));

        private static readonly MethodInfo QueryableContainsMethod =
            ExpressionHelper.GetMethod<IQueryable<object>>(x => x.Contains(null));

        private static readonly MethodInfo QueryableOrderByMethod =
            ExpressionHelper.GetMethod<IQueryable<object>>(x => x.OrderBy((Expression<Func<object, object>>) null));

        private static readonly MethodInfo QueryableOrderByDescendingMethod =
            ExpressionHelper.GetMethod<IQueryable<object>>(
                x => x.OrderByDescending((Expression<Func<object, object>>) null));

        private static readonly MethodInfo QueryableThenByMethod =
            ExpressionHelper.GetMethod<IOrderedQueryable<object>>(x => x.ThenBy((Expression<Func<object, object>>) null));

        private static readonly MethodInfo QueryableThenByDescendingMethod =
            ExpressionHelper.GetMethod<IOrderedQueryable<object>>(
                x => x.ThenByDescending((Expression<Func<object, object>>) null));

        private static readonly MethodInfo QueryableSkipMethod =
            ExpressionHelper.GetMethod<IQueryable<object>>(x => x.Skip(1));

        private static readonly MethodInfo QueryableTakeMethod =
            ExpressionHelper.GetMethod<IQueryable<object>>(x => x.Take(1));

        private static readonly MethodInfo DbContextSetMethod =
            ExpressionHelper.GetMethod<DbContext>(x => x.Set<object>());

        #endregion

        #region Fields

        private readonly DbContext _context;
        private readonly Stack<ParameterExpression> _parameterStack = new Stack<ParameterExpression>();

        #endregion

        #region Constructor

        public DbContextConstraintVisitor(DbContext context)
        {
            _context = context;
        }

        #endregion

        #region Public Visitor Methods

        public LambdaExpression BuildLambdaPredicateExpression(Type setType, Constraint constraint)
        {
            ParameterExpression parameter = Expression.Parameter(setType);

            _parameterStack.Push(parameter);

            LambdaExpression result = Expression.Lambda(BuildPredicateExpression(constraint), parameter);

            _parameterStack.Pop();

            return result;
        }

        public Expression<Func<T, bool>> BuildLambdaPredicateExpression<T>(Constraint constraint)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(T));

            _parameterStack.Push(parameter);

            Expression<Func<T, bool>> result = Expression.Lambda<Func<T, bool>>(BuildPredicateExpression(constraint), parameter);

            _parameterStack.Pop();

            return result;
        }

        public IQueryable<T> BuildQuery<T>(string entityTypeName, Constraint constraint)
        {
            PageConstraint pageConstraint = constraint as PageConstraint;

            Constraint filterConstraint = pageConstraint != null ? pageConstraint.InnerConstraint : constraint;

            Type setType = _context.GetEntityTypeByName(entityTypeName);

            IQueryable dbSet = _context.DbSet(entityTypeName);

            if (filterConstraint == null && pageConstraint == null)
                return dbSet as IQueryable<T>;

            Expression finalExpression = Expression.Constant(dbSet);

            if (filterConstraint != null)
            {
                MethodInfo whereMethod = QueryableWhereMethod.MakeGenericMethod(setType);

                Expression lambda = BuildLambdaPredicateExpression(setType, filterConstraint);

                finalExpression = Expression.Call(null, whereMethod,
                    finalExpression, Expression.Quote(lambda));
            }

            if (pageConstraint != null)
            {
                if (pageConstraint.OrderBy != null)
                {
                    bool first = true;

                    foreach (OrderByExpression orderBy in pageConstraint.OrderBy)
                    {
                        LambdaExpression keySelectorExpression = BuildSelectLambdaExpression(setType, orderBy.FieldName);

                        MethodInfo orderMethod = GetOrderMethod(first, orderBy.Descending)
                            .MakeGenericMethod(setType, keySelectorExpression.ReturnType);

                        finalExpression = Expression.Call(null, orderMethod,
                            finalExpression, Expression.Quote(keySelectorExpression));

                        first = false;
                    }
                }

                if (pageConstraint.Skip > 0)
                {
                    finalExpression = Expression.Call(null,
                        QueryableSkipMethod.MakeGenericMethod(setType),
                        finalExpression,
                        Expression.Constant(pageConstraint.Skip, typeof(int)));
                }

                if (pageConstraint.Take < int.MaxValue)
                {
                    finalExpression = Expression.Call(null,
                        QueryableTakeMethod.MakeGenericMethod(setType),
                        finalExpression,
                        Expression.Constant(pageConstraint.Take, typeof (int)));
                }
            }

            return dbSet.Provider.CreateQuery<T>(finalExpression);
        }

        #endregion

        #region Private Helper Properties and Methods



        private static MethodInfo GetOrderMethod(bool isFirst, bool isDescending)
        {
            if (isFirst)
                return isDescending ? QueryableOrderByDescendingMethod : QueryableOrderByMethod;

            return isDescending ? QueryableThenByDescendingMethod : QueryableThenByMethod;
        }

        private IQueryable GetTypedDbSet(Type entityType)
        {
            MethodInfo genericMethod = DbContextSetMethod.MakeGenericMethod(entityType);

            return (IQueryable)genericMethod.Invoke(_context, null);
        }

        private ParameterExpression Parameter => _parameterStack.Peek();

        #endregion

        #region Expression Builder Methods

        // Builds an expression for the constant specified by value, with a type cast to the target type if needed
        private static Expression BuildConstantExpression(object value, Type target)
        {
            if (value == null || target.IsInstanceOfType(value))
                return Expression.Constant(value, target);

            return Expression.Convert(Expression.Constant(value), target);
        }


        // Builds an expression for  "x => x.propertyName"
        private static LambdaExpression BuildSelectLambdaExpression(Type elementType, string propertyName)
        {
            ParameterExpression parameter = Expression.Parameter(elementType);

            Expression memberAccess = ExpressionHelper.BuildMemberAccessForPath(parameter, propertyName);

            return Expression.Lambda(memberAccess, parameter);
        }

        #endregion

        #region Predicate Expression Builder methods (These build a predicate expression from a constraint)

        private Expression BuildPredicateExpression(Constraint constraint)
        {
            switch (constraint.ConstraintType)
            {
                case ConstraintType.Equal:
                case ConstraintType.NotEqual:
                case ConstraintType.GreaterThan:
                case ConstraintType.GreaterThanOrEqualTo:
                case ConstraintType.LessThan:
                case ConstraintType.LessThanOrEqualTo:
                    return BuildBinaryExpression((BinaryConstraint) constraint);

                case ConstraintType.And:
                case ConstraintType.Or:
                    return BuildCompositeExpression((CompositeConstraint) constraint);

                case ConstraintType.Subquery:
                    return BuildSubqueryExpression((SubqueryConstraint) constraint);

                case ConstraintType.InKeySet:
                    return BuildInKeySetExpression((InKeySetConstraint)constraint);

                case ConstraintType.StartsWith:
                    return BuildStartsWithExpression((StartsWithConstraint) constraint);

                case ConstraintType.StringContains:
                    return BuildStringContainsExpression((StringContainsConstraint) constraint);
            }

            throw new InvalidOperationException($"{constraint.GetType().Name} is not a supported constraint type.");
        }

        private Expression BuildBinaryExpression(BinaryConstraint constraint)
        {
            Expression memberExpression = ExpressionHelper.BuildMemberAccessForPath(Parameter, constraint.FieldName);

            Expression constantExpression = BuildConstantExpression(constraint.Value, memberExpression.Type);

            switch (constraint.ConstraintType)
            {
                case ConstraintType.Equal:
                    return Expression.Equal(memberExpression, constantExpression);

                case ConstraintType.NotEqual:
                    return Expression.NotEqual(memberExpression, constantExpression);

                case ConstraintType.LessThan:
                    return Expression.LessThan(memberExpression, constantExpression);

                case ConstraintType.LessThanOrEqualTo:
                    return Expression.LessThanOrEqual(memberExpression, constantExpression);

                case ConstraintType.GreaterThan:
                    return Expression.GreaterThan(memberExpression, constantExpression);

                case ConstraintType.GreaterThanOrEqualTo:
                    return Expression.GreaterThanOrEqual(memberExpression, constantExpression);

                default:
                    throw new InvalidOperationException("Internal Error: BinaryConstraint.ConstraintType was not a binary constraint type.");
            }
        }

        private Expression BuildCompositeExpression(CompositeConstraint constraint)
        {
            // An empty "And" constraint will match everything.
            // An empty "Or" constraint will match nothing.
            if (constraint.IsEmpty)
                return Expression.Constant(constraint.ConstraintType == ConstraintType.And);

            if (constraint.Count == 1)
                return BuildPredicateExpression(constraint[0]);

            int i = constraint.Count - 1;

            Expression expression = BuildPredicateExpression(constraint[i]);

            switch (constraint.ConstraintType)
            {
                case ConstraintType.And:
                    for (i--; i >= 0; i--)
                        expression = Expression.AndAlso(BuildPredicateExpression(constraint[i]), expression);
                    break;

                case ConstraintType.Or:
                    for (i--; i >= 0; i--)
                        expression = Expression.OrElse(BuildPredicateExpression(constraint[i]), expression);
                    break;

                default:
                    throw new InvalidOperationException("Internal Error: CompositeConstraint.ConstraintType was not a composite constraint type.");
            }

            return expression;
        }

        // Builds an expression for _context.Set<InnerType>().Where(innerConstraint).Select(innerPath).Contains(x.OuterPath)
        private MethodCallExpression BuildSubqueryExpression(SubqueryConstraint constraint)
        {
            // _context.Set<InnerType>()

            Type innerSetType = _context.GetEntityTypeByName(constraint.InnerEntityTypeName);

            ConstantExpression innerSetExpression = Expression.Constant(
                GetTypedDbSet(innerSetType), typeof (IQueryable<>).MakeGenericType(innerSetType));

            // .Where(innerConstraint)

            LambdaExpression innerPredicateExpression = BuildLambdaPredicateExpression(innerSetType,
                constraint.InnerConstraint);

            MethodInfo innerWhereMethod = QueryableWhereMethod.MakeGenericMethod(innerSetType);

            MethodCallExpression innerWhereExpression = Expression.Call(innerWhereMethod, innerSetExpression,
                innerPredicateExpression);

            // .Select(innerPath)

            LambdaExpression innerProjectionExpression = BuildSelectLambdaExpression(innerSetType, constraint.InnerPath);

            MethodInfo innerSelectMethod = QueryableSelectMethod.MakeGenericMethod(innerSetType,
                innerProjectionExpression.ReturnType);

            MethodCallExpression innerSelectExpression = Expression.Call(innerSelectMethod, innerWhereExpression,
                innerProjectionExpression);

            // .Contains(x.OuterPath)

            Expression outerProjectionExpression = ExpressionHelper.BuildMemberAccessForPath(Parameter, constraint.OuterPath);

            Type innerSetProjectionType = innerSelectExpression.Type.GenericTypeArguments[0];

            if (innerSetProjectionType != outerProjectionExpression.Type)
                outerProjectionExpression = Expression.Convert(outerProjectionExpression, innerSetProjectionType);

            MethodInfo containsMethod = QueryableContainsMethod.MakeGenericMethod(innerProjectionExpression.ReturnType);

            return Expression.Call(containsMethod, innerSelectExpression, outerProjectionExpression);
        }

        private Expression BuildInKeySetExpression(InKeySetConstraint constraint)
        {
            if (constraint.Values == null || constraint.Values.Count == 0)
                return Expression.Constant(false);

            ConstantExpression keySetExpression = Expression.Constant(new List<Guid>(constraint.Values));

            Expression keySelectorExpression = ExpressionHelper.BuildMemberAccessForPath(Parameter, constraint.FieldName);

            return Expression.Call(keySetExpression, ListOfGuidsContainsMethod, keySelectorExpression);
        }

        private Expression BuildStringContainsExpression(StringContainsConstraint constraint)
        {
            if (string.IsNullOrEmpty(constraint.Value))
                return Expression.Constant(true);

            Expression memberExpression = ExpressionHelper.BuildMemberAccessForPath(Parameter, constraint.FieldName);

            if (memberExpression.Type != typeof(string))
                memberExpression = Expression.Call(memberExpression, ToStringMethod);

            Expression valueExpression = BuildConstantExpression(constraint.Value, typeof(string));

            return Expression.Call(memberExpression, StringContainsMethod, valueExpression);
        }

        private Expression BuildStartsWithExpression(StartsWithConstraint constraint)
        {
            if (string.IsNullOrEmpty(constraint.Value))
                return Expression.Constant(true);

            Expression memberExpression = ExpressionHelper.BuildMemberAccessForPath(Parameter, constraint.FieldName);

            if (memberExpression.Type != typeof(string))
                memberExpression = Expression.Call(memberExpression, ToStringMethod);

            Expression valueExpression = BuildConstantExpression(constraint.Value, typeof(string));

            return Expression.Call(memberExpression, StringStartsWithMethod, valueExpression);
        }

        #endregion
    }
}
