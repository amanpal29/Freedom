using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Freedom.Annotations;
using Freedom.Constraints;
using Freedom.FullTextSearch;

namespace Freedom.Linq
{
    public class ConstraintBuilder
    {
        private const string NodeTypeNotSupported = @"Expressions of type {0} are not supported";
        private const string MethodCallNotSupported = @"Method calls to {0}.{1} are not supported";
        private const string MustBePredicateExpression = @"Predicate lambda expression required";
        private const string BinaryExpressionsMustMatchConstant = @"Binary expressions are only supported when only one side of the expression is parametric.";
        private const string KeySetCantBeNull = @"The valueset argument of a key in set constraint can not be null.";
        private const string KeySetMustBeGuids = @"The valueset argument of a key in set constraint must be an IEnumerable of Guids.";
        private const string MethodCallNotSupportedInInnerQuery = @"Unsupported method call to Queryable.{0} in the inner query of a subquery constraint.";
        private const string InnerQueryCantBeNull = @"The inner query of a subquery cannot be null.";
        private const string InnerQueryMustBeQueryable = @"The inner query of a subquery must itself be an IQueryable";
        private const string SubqueryMustBeAProjection = @"Subquery constraints must be a select projection on an IQueryable";
        private const string InnerQueriesMustBeAProjection = @"Inner queries must be a select projection.";
        private const string DirectMemberAccessMustBeBoolean = @"Direct MemberAccess must be of boolean type.";
        private const string ConstraintIsNotInvertable = @"A constraint of type {0} cannot be inverted.";
        private const string OnlyQueryableMethodsAreSupported = @"Only method calls to Queryable are supported.";
        private const string MultipleSkipNotSupported = @"Multiple skip expressions are not supported";
        private const string MutipleTakeNotSupported = @"Multiple take expressions are not supported";
        private const string NoSkipAfterTake = @"Skip expressions after a take expression are not supported.";
        private const string NoResorting = @"Multiple OrderBy/OrderByDescending expressions are not supported";
        private const string NoCustomComparerSorting = @"An sort expression with custom comparer is not supported.";
        private const string OrderingExpressionMustBeALambdaProjection = @"Ordering expression must be a lambda projection";
        private const string FullTextSupportForContainsTextOnly = @"ContainsText is the only supported QueryableFullTextSearchExtensions method";

        public static Constraint Build(IQueryable query)
        {
            bool sortComplete = false;

            PageConstraint pageConstraint = new PageConstraint();
            AndConstraint filterConstraint = new AndConstraint();

            Expression expression = query.Expression;

            while (expression.NodeType == ExpressionType.Call)
            {
                MethodCallExpression methodCallExpression = (MethodCallExpression) expression;

                if (methodCallExpression.Method.DeclaringType == typeof(QueryableFullTextSearchExtensions))
                {
                    if (methodCallExpression.Method.Name == nameof(QueryableFullTextSearchExtensions.ContainsText))
                    {
                        string searchText = (string)ExpressionHelper.Evaluate(methodCallExpression.Arguments[1]);

                        if (!string.IsNullOrWhiteSpace(searchText))
                            filterConstraint.Add(new FullTextSearchConstraint(searchText));
                    }
                    else
                    {
                        throw new InvalidOperationException(FullTextSupportForContainsTextOnly);
                    }

                    expression = methodCallExpression.Arguments[0];

                    continue;
                }

                if (methodCallExpression.Method.DeclaringType != typeof (Queryable))
                    throw new InvalidOperationException(OnlyQueryableMethodsAreSupported);

                switch (methodCallExpression.Method.Name)
                {
                    case nameof(Queryable.Skip):
                        if (pageConstraint.Skip != 0)
                            throw new InvalidOperationException(MultipleSkipNotSupported);

                        pageConstraint.Skip = (int) ExpressionHelper.Evaluate(methodCallExpression.Arguments[1]);
                        break;

                    case nameof(Queryable.Take):
                        if (pageConstraint.Take != int.MaxValue)
                            throw new InvalidOperationException(MutipleTakeNotSupported);

                        if (pageConstraint.Skip != 0)
                            throw new InvalidOperationException(NoSkipAfterTake);

                        pageConstraint.Take = (int) ExpressionHelper.Evaluate(methodCallExpression.Arguments[1]);
                        break;

                    case nameof(Queryable.OrderBy):
                    case nameof(Queryable.OrderByDescending):
                    case nameof(Queryable.ThenBy):
                    case nameof(Queryable.ThenByDescending):
                        if (sortComplete)
                            throw new InvalidOperationException(NoResorting);

                        pageConstraint.AddOrderBy(BuildOrderBy(methodCallExpression));

                        sortComplete = methodCallExpression.Method.Name.StartsWith("OrderBy");
                        break;

                    case nameof(Queryable.Where):
                        filterConstraint.Add(Build(methodCallExpression.Arguments[1]));
                        break;
                }

                expression = methodCallExpression.Arguments[0];
            }

            if (expression.NodeType != ExpressionType.Constant)
                throw new InvalidOperationException(string.Format(NodeTypeNotSupported, expression.NodeType));

            switch (filterConstraint.Count)
            {
                case 0:
                    break;

                case 1:
                    pageConstraint.InnerConstraint = filterConstraint[0];
                    break;

                default:
                    pageConstraint.InnerConstraint = filterConstraint;
                    break;
            }

            if (pageConstraint.IsAllRecords && pageConstraint.OrderBy.Count == 0)
                return pageConstraint.InnerConstraint;

            return pageConstraint;
        }

        public static Constraint Build<T>(Expression<Func<T, bool>> predicateExpression)
        {
            return Convert(predicateExpression.Body);
        } 

        public static Constraint Build(Expression expression)
        {
            while (expression.NodeType == ExpressionType.Quote)
            {
                UnaryExpression unaryExpression = (UnaryExpression) expression;
                expression = unaryExpression.Operand;
            }

            LambdaExpression lambdaExpression = expression as LambdaExpression;

            if (lambdaExpression == null)
                throw new ArgumentException(MustBePredicateExpression, nameof(expression));

            if (lambdaExpression.ReturnType != typeof(bool) && lambdaExpression.ReturnType != typeof(bool?))
                throw new ArgumentException(MustBePredicateExpression, nameof(expression));

            return Convert(lambdaExpression.Body);
        }

        private static OrderByExpression BuildOrderBy(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression == null)
                throw new ArgumentNullException(nameof(methodCallExpression));

            if (methodCallExpression.Method.DeclaringType != typeof(Queryable))
                throw new ArgumentException("methodCallExpression must be a call to a Queryable method", nameof(methodCallExpression));

            if (methodCallExpression.Method.Name != nameof(Queryable.OrderBy) &&
                methodCallExpression.Method.Name != nameof(Queryable.OrderByDescending) &&
                methodCallExpression.Method.Name != nameof(Queryable.ThenBy) &&
                methodCallExpression.Method.Name != nameof(Queryable.ThenByDescending))
                throw new ArgumentException("methodCallExpression must be a call to a Queryable sort method", nameof(methodCallExpression));

            if (methodCallExpression.Arguments.Count > 2)
                throw new InvalidOperationException(NoCustomComparerSorting);

            Expression expression = ExpressionHelper.UnwrapConversionExpressions(methodCallExpression.Arguments[1]);

            LambdaExpression lambdaExpression = expression as LambdaExpression;

            if (lambdaExpression == null)
                throw new InvalidOperationException(OrderingExpressionMustBeALambdaProjection);

            string fieldName = ExpressionHelper.GetPath(lambdaExpression);

            return new OrderByExpression(fieldName, methodCallExpression.Method.Name.EndsWith("Descending"));
        }

        private static Constraint Convert([NotNull] Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            switch (expression.NodeType)
            {
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    return ConvertUnaryExpression((UnaryExpression) expression);

                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                    return ConvertBinaryExpression((BinaryExpression) expression);

                case ExpressionType.Call:
                    return ConvertMethodCall((MethodCallExpression) expression);

                case ExpressionType.MemberAccess:
                    return ConvertMemberAccess((MemberExpression) expression);

                default:
                    throw new InvalidOperationException(string.Format(NodeTypeNotSupported, expression.NodeType));
            }
        }

        private static Constraint ConvertUnaryExpression(UnaryExpression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Not:
                    return InvertConstraint(Convert(expression.Operand));

                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    return Convert(expression.Operand);

                default:
                    throw new InvalidOperationException(string.Format(NodeTypeNotSupported, expression.NodeType));
            }
        }

        private static Constraint InvertConstraint(Constraint input)
        {
            BinaryConstraint binaryConstraint = input as BinaryConstraint;

            if (binaryConstraint != null)
                return InvertBinaryConstraint(binaryConstraint);

            CompositeConstraint compositeConstraint = input as CompositeConstraint;

            if (compositeConstraint != null)
                return InvertCompositeConstraint(compositeConstraint);

            throw new InvalidOperationException(string.Format(ConstraintIsNotInvertable, input.ConstraintType));
        }

        private static BinaryConstraint InvertBinaryConstraint(BinaryConstraint input)
        {
            switch (input.ConstraintType)
            {
                case ConstraintType.Equal:
                    return BinaryConstraint.Build(ExpressionType.NotEqual, input.FieldName, input.Value);

                case ConstraintType.NotEqual:
                    return BinaryConstraint.Build(ExpressionType.Equal, input.FieldName, input.Value);

                case ConstraintType.GreaterThanOrEqualTo:
                    return BinaryConstraint.Build(ExpressionType.LessThan, input.FieldName, input.Value);

                case ConstraintType.LessThanOrEqualTo:
                    return BinaryConstraint.Build(ExpressionType.GreaterThan, input.FieldName, input.Value);

                case ConstraintType.GreaterThan:
                    return BinaryConstraint.Build(ExpressionType.LessThanOrEqual, input.FieldName, input.Value);

                case ConstraintType.LessThan:
                    return BinaryConstraint.Build(ExpressionType.GreaterThanOrEqual, input.FieldName, input.Value);

                default:
                    throw new InvalidOperationException("INTERNAL ERROR: BinaryConstraint.ConstraintType is not a binary constraint type.");
            }
        }

        private static CompositeConstraint InvertCompositeConstraint(CompositeConstraint input)
        {
            switch (input.ConstraintType)
            {
                case ConstraintType.And:
                    return new OrConstraint(input.Select(InvertConstraint));

                case ConstraintType.Or:
                    return new AndConstraint(input.Select(InvertConstraint));

                default:
                    throw new InvalidOperationException(
                        "INTERNAL ERROR: CompositeConstraint.ConstraintType is not a composite constraint type.");
            }
        }

        private static Constraint ConvertBinaryExpression(BinaryExpression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                    bool leftIsValue = ExpressionHelper.IsParameterless(expression.Left);
                    bool rightIsValue = ExpressionHelper.IsParameterless(expression.Right);

                    if (leftIsValue == rightIsValue)
                        throw new InvalidOperationException(BinaryExpressionsMustMatchConstant);

                    Expression fieldExpression = leftIsValue ? expression.Right : expression.Left;
                    Expression valueExpression = leftIsValue ? expression.Left : expression.Right;

                    string path = ExpressionHelper.GetPathInternal(fieldExpression);
                    object value = ExpressionHelper.Evaluate(valueExpression);

                    Type fieldType = ExpressionHelper.GetSourceFieldType(fieldExpression);

                    if (value != null && !fieldType.IsInstanceOfType(value))
                    {
                        TypeConverter typeConverter = TypeDescriptor.GetConverter(value.GetType());

                        if (typeConverter.CanConvertTo(fieldType))
                            value = typeConverter.ConvertTo(value, fieldType);
                    }

                    ExpressionType expressionType = leftIsValue ? BinaryConstraint.FlipBinaryExpressionType(expression.NodeType) : expression.NodeType;

                    return BinaryConstraint.Build(expressionType, path, value);

                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return CompositeConstraint.Build(expression.NodeType, Convert(expression.Left), Convert(expression.Right));

                default:
                    throw new InvalidOperationException(string.Format(NodeTypeNotSupported, expression.NodeType));
            }
        }

        private static Constraint ConvertMethodCall(MethodCallExpression expression)
        {
            // The Constraint.IsMatch() call
            if (typeof (Constraint).IsAssignableFrom(expression.Method.DeclaringType) && expression.Method.Name == nameof(Constraint.IsMatch))
                return (Constraint) ExpressionHelper.Evaluate(expression.Object);

            // The Queryable.Contains() call
            if (expression.Method.DeclaringType == typeof (Queryable) && expression.Method.Name == nameof(Queryable.Contains) && expression.Arguments.Count == 2)
                return ConvertSubqueryConstraint(expression);

            // The Enumerable.Contains() call
            if (expression.Method.DeclaringType == typeof (Enumerable) && expression.Method.Name == nameof(Enumerable.Contains) && expression.Arguments.Count == 2)
                return ConvertValueInKeySetConstraint(expression.Arguments[0], expression.Arguments[1]);

            // The ICollection<>.Contains() call (when make explicitally)
            if (expression.Method.DeclaringType == typeof (ICollection<Guid>) && expression.Method.Name == nameof(ICollection<Guid>.Contains))
                return ConvertValueInKeySetConstraint(expression.Object, expression.Arguments[0]);

            // The ICollection<>.Contains() call (when implemented on a concrete class)
            if (expression.Method.DeclaringType != null && !expression.Method.DeclaringType.IsInterface)
            {
                Type collectionElementType = TypeHelper.GetCollectionElementType(expression.Method.DeclaringType);
                if (collectionElementType != null) // If it implements ICollection<>
                {
                    Type collectionType = typeof (ICollection<>).MakeGenericType(collectionElementType);

                    MethodInfo interfaceMethod = collectionType.GetMethod(nameof(ICollection<object>.Contains), new[] {collectionElementType});

                    MethodInfo method = TypeHelper.GetImplementation(expression.Method.DeclaringType, interfaceMethod);

                    if (expression.Method == method)
                        return ConvertValueInKeySetConstraint(expression.Object, expression.Arguments[0]);
                }
            }

            string declaringType = expression.Method.DeclaringType != null ? expression.Method.DeclaringType.FullName : "dynamic";

            throw new InvalidOperationException(string.Format(MethodCallNotSupported, declaringType, expression.Method.Name));
        }

        private static Constraint ConvertValueInKeySetConstraint(Expression enumerableExpression, Expression fieldProjectionExpression)
        {
            object valueSet = ExpressionHelper.Evaluate(enumerableExpression);

            if (valueSet == null)
                throw new InvalidOperationException(KeySetCantBeNull);

            IEnumerable<Guid> keySet = valueSet as IEnumerable<Guid>;

            if (keySet == null)
                throw new InvalidOperationException(KeySetMustBeGuids);

            string fieldName = ExpressionHelper.GetPathInternal(fieldProjectionExpression);

            return new InKeySetConstraint(fieldName, keySet);
        }

        private static SubqueryConstraint ConvertSubqueryConstraint(MethodCallExpression expression)
        {
            MethodCallExpression selectCall = GetInnerProjection(expression.Arguments[0]);

            Expression innerQuery = selectCall.Arguments[0];

            Type innerEntityType = innerQuery.Type.GetGenericArguments()[0];

            Constraint innerConstraint = ExtractInnerConstraint(innerQuery);

            string innerPath = ExpressionHelper.GetPathInternal(selectCall.Arguments[1]);

            string outerPath = ExpressionHelper.GetPathInternal(expression.Arguments[1]);

            return new SubqueryConstraint(outerPath, innerPath, innerEntityType, innerConstraint);
        }

        private static Constraint ExtractInnerConstraint(Expression innerQuery)
        {
            AndConstraint innerConstraint = new AndConstraint();

            while (innerQuery is MethodCallExpression)
            {
                MethodCallExpression whereCallExpression = (MethodCallExpression) innerQuery;

                if (whereCallExpression.Method.DeclaringType != typeof (Queryable))
                    break;

                switch (whereCallExpression.Method.Name)
                {
                    case nameof(Queryable.Where):
                        innerConstraint.Add(Build(whereCallExpression.Arguments[1]));
                        innerQuery = whereCallExpression.Arguments[0];
                        break;

                    case nameof(Queryable.Cast):
                    case nameof(Queryable.Distinct):
                    case nameof(Queryable.OrderBy):
                    case nameof(Queryable.OrderByDescending):
                    case nameof(Queryable.ThenBy):
                    case nameof(Queryable.ThenByDescending):
                    case nameof(Queryable.Reverse):
                        innerQuery = whereCallExpression.Arguments[0];
                        break;

                    default:
                        throw new InvalidOperationException(string.Format(MethodCallNotSupportedInInnerQuery, whereCallExpression.Method.Name));
                }
            }

            switch (innerConstraint.Count)
            {
                case 0:
                    return null;

                case 1:
                    return innerConstraint[0];

                default:
                    return innerConstraint;
            }
        }

        private static MethodCallExpression GetInnerProjection(Expression expression)
        {
            if (ExpressionHelper.IsParameterless(expression))
            {
                object query = ExpressionHelper.Evaluate(expression);

                if (query == null)
                    throw new InvalidOperationException(InnerQueryCantBeNull);

                IQueryable queryable = query as IQueryable;

                if (queryable == null)
                    throw new InvalidOperationException(InnerQueryMustBeQueryable);

                expression = queryable.Expression;
            }

            if (!(expression is MethodCallExpression))
                throw new InvalidOperationException(SubqueryMustBeAProjection);

            MethodCallExpression methodCallExpression = (MethodCallExpression) expression;

            if (methodCallExpression.Method.DeclaringType != typeof (Queryable) || methodCallExpression.Method.Name != nameof(Queryable.Select))
                throw new InvalidOperationException(InnerQueriesMustBeAProjection);

            return methodCallExpression;
        }

        private static Constraint ConvertMemberAccess(MemberExpression expression)
        {
            PropertyInfo propertyInfo = expression.Member as PropertyInfo;

            FieldInfo fieldInfo = expression.Member as FieldInfo;

            Type memberType = null;

            if (propertyInfo != null)
                memberType = propertyInfo.PropertyType;

            if (fieldInfo != null)
                memberType = fieldInfo.FieldType;

            if (memberType != typeof (bool) && memberType != typeof (bool?))
                throw new InvalidOperationException(DirectMemberAccessMustBeBoolean);

            string path = ExpressionHelper.GetPathInternal(expression);

            return new EqualConstraint(path, true);
        }
    }
}
