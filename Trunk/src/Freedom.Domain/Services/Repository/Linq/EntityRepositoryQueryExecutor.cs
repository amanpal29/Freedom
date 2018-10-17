using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Freedom.Domain.Model;
using Freedom.Constraints;
using Freedom.Exceptions;
using Freedom.FullTextSearch;
using Freedom.Linq;

namespace Freedom.Domain.Services.Repository.Linq
{
    internal class EntityRepositoryQueryExecutor
    {
        private static readonly MethodInfo EnumerableOfTypeMethod =
            ExpressionHelper.GetMethod<IEnumerable<object>>(x => x.OfType<IEnumerable<object>>());

        private static readonly MethodInfo EnumerableSelectMethod =
            ExpressionHelper.GetMethod<IEnumerable<object>>(x => x.Select(y => (object) null));

        private static readonly MethodInfo EnumerableReverseMethod =
            ExpressionHelper.GetMethod<IEnumerable<object>>(x => x.Reverse());

        private static readonly PropertyInfo KeyProperty = typeof (KeyValuePair<string, string>).GetProperty("Key");
        
        private static readonly PropertyInfo ValueProperty = typeof(KeyValuePair<string, string>).GetProperty("Value");

        private readonly IEntityRepository _entityRepository;

        private enum QueryType
        {
            Enumerable,
            SingleElement,
            FullTextSearch,
            GetGroups,
            Count,
            Any
        }

        private Type _elementType;
        private QueryType _queryType = QueryType.Enumerable;
        private ResolutionGraph _resolutionGraph;
        private readonly AndConstraint _filter = new AndConstraint();
        private string _searchText;
        private readonly List<OrderByExpression> _sort = new List<OrderByExpression>();
        private bool _sortComplete;
        private AggregateFunction _aggregateFunction = AggregateFunction.Invalid;
        private Delegate _groupProjection;
        private Type _outputElementType;
        private bool _reverseSortGroupKey;
        private string _keyField;
        private string _valueField;
        private bool _singleElementOnly;
        private bool _allowNull;
        private int? _skip;
        private int? _take;


        internal EntityRepositoryQueryExecutor(IEntityRepository entityRepository, Expression expression)
        {
            if (entityRepository == null)
                throw new ArgumentNullException(nameof(entityRepository));

            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            _entityRepository = entityRepository;

            while (expression != null)
            {
                switch (expression.NodeType)
                {
                    case ExpressionType.Call:
                        expression = HandleMethodCall((MethodCallExpression) expression);
                        break;

                    case ExpressionType.Constant:
                        expression = HandleConstant((ConstantExpression) expression);
                        break;

                    default:
                        throw new UnsupportedQueryException();
                }
            }

            if (_sort.Count > 0 && !_sortComplete)
                throw new UnsupportedQueryException("a ThenBy expression without OrderBy expression is not valid.");
        }

        public object Execute()
        {
            try
            {
                if (SynchronizationContext.Current == null)
                    return ExecuteAsync().Result;

                Debug.Print(
                    "Warning: Making a synchronous call to the entity repository on a thread with a SynchronizationContext.");

                return Task.Run(ExecuteAsync).Result;
            }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions.Count > 1)
                    throw;

                throw ex.InnerExceptions[0];
            }

        }

        public async Task<object> ExecuteAsync()
        {
            switch (_queryType)
            {
                case QueryType.Enumerable:
                    return await ExecuteEnumerableAsync();

                case QueryType.SingleElement:
                    return await ExecuteSingleElementAsync();

                case QueryType.FullTextSearch:
                    return await ExecuteFullTextSearchAsync();

                case QueryType.GetGroups:
                    return await ExecuteGetGroupsAsync();

                case QueryType.Count:
                    return await ExecuteCountAsync();

                case QueryType.Any:
                    return await ExecuteAnyAsync();

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task<object> ExecuteEnumerableAsync()
        {
            IEnumerable<Entity> result = await _entityRepository.GetEntitiesAsync(_elementType.Name, null, _resolutionGraph, BuildFinalConstraint(), CancellationToken.None);

            MethodInfo typeCastMethod = EnumerableOfTypeMethod.MakeGenericMethod(_elementType);

            return typeCastMethod.Invoke(null, new object[] {result});
        }

        private async Task<object> ExecuteSingleElementAsync()
        {
            IEnumerable<Entity> entities = await _entityRepository.GetEntitiesAsync(_elementType.Name, null, _resolutionGraph, BuildFinalConstraint(), CancellationToken.None);

            return GetSingleElementFromCollection(entities);
        }

        private object GetSingleElementFromCollection(IEnumerable<Entity> entities)
        {
            IList<Entity> list = entities as IList<Entity>;

            if (list != null)
            {
                switch (list.Count)
                {
                    case 0:
                        if (_allowNull)
                            return null;

                        throw new InvalidOperationException("Sequence contains no elements");

                    case 1:
                        return list[0];

                    default:
                        if (_singleElementOnly)
                            throw new InvalidOperationException("Sequence contains more than one element");

                        return list[0];
                }
            }

            using (IEnumerator<Entity> e = entities.GetEnumerator())
            {
                if (!e.MoveNext())
                {
                    if (_allowNull)
                        return null;

                    throw new InvalidOperationException("Sequence contains no elements");
                }

                Entity result = e.Current;

                if (_singleElementOnly && e.MoveNext())
                    throw new InvalidOperationException("Sequence contains more than one element");

                return result;
            }
        }

        private async Task<object> ExecuteFullTextSearchAsync()
        {
            IEnumerable<Entity> result = await _entityRepository.SearchAsync(_elementType.Name, _searchText, BuildFinalConstraint(), CancellationToken.None);

            MethodInfo typeCastMethod = EnumerableOfTypeMethod.MakeGenericMethod(_elementType);

            return typeCastMethod.Invoke(null, new object[] {result});
        }

        private async Task<object> ExecuteGetGroupsAsync()
        {
            GroupCollection groups = await _entityRepository.GetGroupsAsync(_elementType.Name, _keyField, _aggregateFunction, _valueField, GetFilterConstraint(), CancellationToken.None);

            MethodInfo selectMethod = EnumerableSelectMethod.MakeGenericMethod(typeof (KeyValuePair<string, string>), _outputElementType);

            object result = selectMethod.Invoke(null, new object[] {groups, _groupProjection});

            if (_reverseSortGroupKey)
            {
                // Groups will already come from the server sorted ascending.
                // If the expresssion was for sorted descending we need to reverse the array.

                MethodInfo reverseMethod = EnumerableReverseMethod.MakeGenericMethod(_outputElementType);

                object[] parameters = {result};

                result = reverseMethod.Invoke(null, parameters);
            }

            return result;
        }

        private Task<int> ExecuteCountAsync()
        {
            return _entityRepository.GetCountAsync(_elementType.Name, BuildFinalConstraint(), CancellationToken.None);
        }

        private async Task<bool> ExecuteAnyAsync()
        {
            int count = await _entityRepository.GetCountAsync(_elementType.Name, BuildFinalConstraint(), CancellationToken.None);

            return count > 0;
        }

        public Constraint GetFilterConstraint()
        {
            switch (_filter.Count)
            {
                case 0:
                    return null;

                case 1:
                    return _filter[0];

                default:
                    return _filter;
            }
        }

        private Constraint BuildFinalConstraint()
        {
            Constraint filterConstraint = GetFilterConstraint();

            switch (_queryType)
            {
                case QueryType.Enumerable:
                {
                    if (!_skip.HasValue && !_take.HasValue && _sort.Count == 0)
                        return filterConstraint;

                    PageConstraint pageConstraint = new PageConstraint();

                    pageConstraint.Skip = _skip ?? 0;
                    pageConstraint.Take = _take ?? int.MaxValue;
                    pageConstraint.OrderBy = _sort;
                    pageConstraint.InnerConstraint = filterConstraint;

                    return pageConstraint;
                }

                case QueryType.SingleElement:
                {
                    PageConstraint pageConstraint = new PageConstraint();

                    pageConstraint.Skip = _skip ?? 0;
                    pageConstraint.Take = _singleElementOnly ? 2 : 1;
                    pageConstraint.OrderBy = _sort;
                    pageConstraint.InnerConstraint = filterConstraint;

                    return pageConstraint;
                }

                case QueryType.Any:
                case QueryType.FullTextSearch:
                case QueryType.Count:
                    return filterConstraint;

                default:
                    throw new InvalidOperationException("INTERNAL ERROR: Unsupported query type in BuildFinalConstraint method.");
            }
        }

        private Expression HandleConstant(ConstantExpression expression)
        {
            object value = expression.Value;

            if (value == null)
                throw new NullReferenceException("The query surface constant was null.");

            Type type = value.GetType();

            if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof (EntityRepositoryQuery<>))
                throw new InvalidOperationException("Internal Error: Expected instance of EntityRepositoryQuery`1");

            IQueryable queryable = (IQueryable) value;

            _elementType = queryable.ElementType;

            return null;
        }

        private Expression HandleMethodCall(MethodCallExpression expression)
        {
            if (expression.Method.DeclaringType == typeof (Queryable))
                return HandleQueryableMethodCall(expression);

            if (expression.Method.DeclaringType == typeof (QueryableResolutionExtensions))
                return HandleResolutionExtensionMethodCall(expression);

            if (expression.Method.DeclaringType == typeof (QueryableFullTextSearchExtensions))
                return HandleFullTextSearchExtensionMethodCall(expression);

            throw new UnsupportedQueryException($"Method call {expression.Method.DeclaringType}::{expression.Method.Name} is not supported");
        }

        private Expression HandleQueryableMethodCall(MethodCallExpression expression)
        {
            switch (expression.Method.Name)
            {
                case "Any":
                    _queryType = QueryType.Any;
                    if (expression.Arguments.Count > 1)
                        HandleFilter(expression.Arguments[1]);
                    break;

                case "Distinct":
                    if (IsSupportedProjectionSelectDistinct(expression))
                        return HandleProjectionSelectDistinct(expression);

                    throw new UnsupportedQueryException("Distinct projection was not supported.");

                case "Select":
                    if (IsSupportedProjectionOfGroupCount(expression))
                        return HandleProjectionOfGroupBy(expression);

                    throw new UnsupportedQueryException("Select projection was not supported.");

                case "Single":
                case "SingleOrDefault":
                case "First":
                case "FirstOrDefault":
                    _queryType = QueryType.SingleElement;
                    _singleElementOnly = expression.Method.Name.StartsWith("Single");
                    _allowNull = expression.Method.Name.EndsWith("OrDefault");
                    if (expression.Arguments.Count > 1)
                        HandleFilter(expression.Arguments[1]);
                    break;

                case "Count":
                    _queryType = QueryType.Count;
                    if (expression.Arguments.Count > 1)
                        HandleFilter(expression.Arguments[1]);
                    break;

                case "Take":
                    HandleTake(expression.Arguments[1]);
                    break;

                case "Skip":
                    HandleSkip(expression.Arguments[1]);
                    break;

                case "ThenBy":
                case "ThenByDescending":
                case "OrderBy":
                case "OrderByDescending":
                    if (expression.Arguments.Count > 2)
                        throw new UnsupportedQueryException("An sort expression with custom comparer is not supported.");

                    HandleSort(expression.Arguments[1], expression.Method.Name.EndsWith("Descending"));

                    _sortComplete = expression.Method.Name.StartsWith("OrderBy");
                    break;

                case "Where":
                    HandleFilter(expression.Arguments[1]);
                    break;

                default:
                    throw new UnsupportedQueryException($"Method call {expression.Method.DeclaringType}::{expression.Method.Name} is not supported");
            }

            return expression.Arguments[0];
        }

        private bool IsSupportedProjectionSelectDistinct(MethodCallExpression distinctExpression)
        {
            if (distinctExpression == null || distinctExpression.Method.DeclaringType != typeof (Queryable) || distinctExpression.Method.Name != nameof(Queryable.Distinct))
                return false;

            MethodCallExpression selectExpression = distinctExpression.Arguments[0] as MethodCallExpression;

            if (selectExpression == null || selectExpression.Method.DeclaringType != typeof (Queryable) || selectExpression.Method.Name != nameof(Queryable.Select))
                return false;

            return true;
        }

        private Expression HandleProjectionSelectDistinct(MethodCallExpression expression)
        {
            _queryType = QueryType.GetGroups;

            MethodCallExpression selectExpression = (MethodCallExpression) expression.Arguments[0];

            LambdaExpression keyLambdaExpression = ExpressionHelper.UnwrapLambdaExpressions(selectExpression.Arguments[1]);

            _keyField = ExpressionHelper.GetPath(keyLambdaExpression);

            _outputElementType = keyLambdaExpression.Body.Type;

            _aggregateFunction = AggregateFunction.Count;

            _groupProjection = (Func<KeyValuePair<string, string>, string>) (kvp => kvp.Key);

            return selectExpression.Arguments[0];
        }

        private static bool IsSupportedProjectionOfGroupCount(MethodCallExpression selectExpression)
        {
            if (selectExpression == null || selectExpression.Method.DeclaringType != typeof (Queryable) || selectExpression.Method.Name != "Select")
                return false;

            Expression expression = selectExpression.Arguments[0];

            if (ExpressionHelper.IsSortExpression(expression))
            {
                MethodCallExpression orderExpression = (MethodCallExpression) expression;

                expression = orderExpression.Arguments[0];

                LambdaExpression sortParameter = ExpressionHelper.UnwrapLambdaExpressions(orderExpression.Arguments[1]);

                if (sortParameter == null || ExpressionHelper.GetPath(sortParameter) != "Key")
                    return false;
            }

            MethodCallExpression groupByExpression = expression as MethodCallExpression;

            if (groupByExpression == null || groupByExpression.Method.DeclaringType != typeof (Queryable) || groupByExpression.Method.Name != "GroupBy")
                return false;

            LambdaExpression lambdaExpression = ExpressionHelper.UnwrapLambdaExpressions(selectExpression.Arguments[1]);

            NewExpression newExpression = lambdaExpression?.Body as NewExpression;

            return newExpression != null && newExpression.Arguments.All(CanRewriteArgumentForProjectionOfGroupCount);
        }

        private static bool CanRewriteArgumentForProjectionOfGroupCount(Expression expression)
        {
            if (expression is MemberExpression)
            {
                MemberExpression memberExpression = (MemberExpression) expression;

                return memberExpression.Member.Name == "Key" && memberExpression.Expression is ParameterExpression;
            }

            if (expression is MethodCallExpression)
            {
                MethodCallExpression methodCallExpression = (MethodCallExpression) expression;

                if (methodCallExpression.Method.DeclaringType != typeof (Queryable) && methodCallExpression.Method.DeclaringType != typeof (Enumerable))
                    return false;

                AggregateFunction function;

                if (!Enum.TryParse(methodCallExpression.Method.Name, false, out function))
                    return false;

                if (!(methodCallExpression.Arguments[0] is ParameterExpression))
                    return false;

                switch (function)
                {
                    case AggregateFunction.Count:
                        return methodCallExpression.Arguments.Count == 1;

                    case AggregateFunction.Average:
                    case AggregateFunction.Min:
                    case AggregateFunction.Max:
                    case AggregateFunction.Sum:
                        return methodCallExpression.Arguments.Count == 2;
                }
            }

            return false;
        }

        private Expression HandleProjectionOfGroupBy(MethodCallExpression selectExpression)
        {
            _queryType = QueryType.GetGroups;

            MethodCallExpression expression = (MethodCallExpression) selectExpression.Arguments[0];

            if (ExpressionHelper.IsSortExpression(expression))
            {
                _reverseSortGroupKey = expression.Method.Name.EndsWith("Descending");

                expression = (MethodCallExpression) expression.Arguments[0];
            }

            MethodCallExpression groupByExpression = expression;

            LambdaExpression keyLambdaExpression = ExpressionHelper.UnwrapLambdaExpressions(groupByExpression.Arguments[1]);

            _keyField = ExpressionHelper.GetPath(keyLambdaExpression);

            LambdaExpression projectionLambdaExpression = ExpressionHelper.UnwrapLambdaExpressions(selectExpression.Arguments[1]);

            NewExpression newExpression = (NewExpression) projectionLambdaExpression.Body;

            _groupProjection = RewriteNewProjection(newExpression);

            _outputElementType = newExpression.Type;

            return groupByExpression.Arguments[0];
        }

        private Delegate RewriteNewProjection(NewExpression originalBody)
        {
            _outputElementType = originalBody.Constructor.DeclaringType;

            // Create a new body for the lambda expression

            ParameterExpression parameterExpression = Expression.Parameter(typeof (KeyValuePair<string, string>));

            Expression[] arguments = new Expression[originalBody.Arguments.Count];

            for (int i = 0; i < originalBody.Arguments.Count; i++)
                arguments[i] = RewriteArgumentExpression(originalBody.Arguments[i], parameterExpression);

            Expression body = Expression.New(originalBody.Constructor, arguments);

            // Compile the new predicate

            LambdaExpression lambdaExpression = Expression.Lambda(body, parameterExpression);

            return lambdaExpression.Compile();
        }

        private Expression RewriteArgumentExpression(Expression expression, ParameterExpression parameterExpression)
        {
            Expression result;

            if (expression is MemberExpression)
            {
                result = Expression.MakeMemberAccess(parameterExpression, KeyProperty);
            }
            else
            {
                MethodCallExpression methodCallExpression = expression as MethodCallExpression;

                if (methodCallExpression == null)
                    throw new InvalidOperationException("Internal Error: Unsupported argument rewrite.");

                AggregateFunction function;

                if (!Enum.TryParse(methodCallExpression.Method.Name, false, out function))
                    throw new InvalidOperationException("Internal Error: Unsupported aggregate method.");

                if (_aggregateFunction != AggregateFunction.Invalid && _aggregateFunction != function)
                    throw new UnsupportedQueryException("Mutiple group aggregate functions are not supported.");

                _aggregateFunction = function;

                if (_aggregateFunction != AggregateFunction.Count)
                {
                    LambdaExpression lambda = ExpressionHelper.UnwrapLambdaExpressions(methodCallExpression.Arguments[1]);

                    string valueField = ExpressionHelper.GetPath(lambda);

                    if (_valueField != null && _valueField != valueField)
                        throw new UnsupportedQueryException("Mutiple group aggregates of mutiple value fields are not supported.");

                    _valueField = valueField;
                }

                result = Expression.MakeMemberAccess(parameterExpression, ValueProperty);
            }

            return ExpressionHelper.ConvertFromString(result, expression.Type);
        }

        private Expression HandleResolutionExtensionMethodCall(MethodCallExpression expression)
        {
            switch (expression.Method.Name)
            {
                case nameof(QueryableResolutionExtensions.Include):
                    string path = (string) ExpressionHelper.Evaluate(expression.Arguments[1]);

                    if (string.IsNullOrWhiteSpace(path))
                        break;

                    if (_queryType == QueryType.FullTextSearch)
                        throw new UnsupportedQueryException("An Include expression can't be used with a BestMatch expression.");

                    if (_resolutionGraph == null)
                        _resolutionGraph = new ResolutionGraph();

                    _resolutionGraph.Add(path);

                    break;
            }

            return expression.Arguments[0];
        }

        private Expression HandleFullTextSearchExtensionMethodCall(MethodCallExpression expression)
        {
            switch (expression.Method.Name)
            {
                case nameof(QueryableFullTextSearchExtensions.BestMatch):
                    if (_filter.Count > 0)
                        throw new UnsupportedQueryException("A Where expression can't be used after a BestMatch expression");

                    if (_sort.Count > 0)
                        throw new UnsupportedQueryException("An OrderBy expression can't be used after a BestMatch expression");

                    if (_take.HasValue)
                        throw new UnsupportedQueryException("A Take expression can't be used after a BestMatch expression.");

                    if (_skip.HasValue)
                        throw new UnsupportedQueryException("A Skip expression can't be used after a BestMatch expression.");

                    if (_resolutionGraph?.Count > 0)
                        throw new UnsupportedQueryException("An Include expression can't be used with a BestMatch expression.");

                    _queryType = QueryType.FullTextSearch;
                    _searchText = (string) ExpressionHelper.Evaluate(expression.Arguments[1]) ?? string.Empty;

                    break;

                case nameof(QueryableFullTextSearchExtensions.ContainsText):
                    string searchText = (string) ExpressionHelper.Evaluate(expression.Arguments[1]);

                    if (!string.IsNullOrWhiteSpace(searchText))
                        _filter.Add(new FullTextSearchConstraint(searchText));

                    break;
            }

            return expression.Arguments[0];
        }

        private void HandleFilter(Expression expression)
        {
            Constraint constraint = ConstraintBuilder.Build(expression);

            _filter.Add(constraint);
        }

        private void HandleSort(Expression expression, bool isDescending)
        {
            if (_sortComplete)
                throw new UnsupportedQueryException("Mutiple OrderBy or OrderByDescending expressions");

            expression = ExpressionHelper.UnwrapConversionExpressions(expression);

            LambdaExpression lambdaExpression = expression as LambdaExpression;

            if (lambdaExpression == null)
                throw new ArgumentException("Ordering expression must be a lambda projection");

            MemberExpression memberExpression = lambdaExpression.Body as MemberExpression;

            if (memberExpression == null)
                throw new ArgumentException("Ordering lambda projections must be a member access");

            string fieldName = memberExpression.Member.Name;

            expression = ExpressionHelper.UnwrapConversionExpressions(memberExpression.Expression);

            if (!(expression is ParameterExpression))
                throw new ArgumentException("Ordering lamba projections must be a single member access on the parameter expression");

            _sort.Insert(0, new OrderByExpression(fieldName, isDescending));
        }

        private void HandleTake(Expression expression)
        {
            int value = (int) ExpressionHelper.Evaluate(expression);

            if (_take.HasValue)
                throw new UnsupportedQueryException("Multiple Take expressions in a query are not supported.");

            if (_skip.HasValue)
                throw new UnsupportedQueryException("A Skip expression after a Take expression in a query is not supported.");

            _take = value;
        }

        private void HandleSkip(Expression expression)
        {
            int value = (int) ExpressionHelper.Evaluate(expression);

            if (_skip.HasValue)
                throw new UnsupportedQueryException("Multiple Skip expressions in a query are not supported.");

            _skip = value;
        }
    }
}
