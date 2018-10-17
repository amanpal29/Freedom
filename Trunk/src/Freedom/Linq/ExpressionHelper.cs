using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Freedom.Extensions;

namespace Freedom.Linq
{
    public static class ExpressionHelper
    {
        private static readonly MethodInfo ObjectToStringMethod = GetMethod<object>(x => x.ToString());

        private static readonly string[] SortMethodNames =
        {
            nameof(Queryable.OrderBy), nameof(Queryable.OrderByDescending),
            nameof(Queryable.ThenBy), nameof(Queryable.ThenByDescending)
        };

        public static PropertyInfo FindMember(Type type, string memberName)
        {
            PropertyInfo property = type.GetProperty(memberName,
                BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public);

            if (property != null)
                return property;

            throw new InvalidOperationException($"The property '{memberName}' was not found on type '{type.FullName}'");
        }

        // Builds an expression for to access mutiple members in a path
        public static Expression BuildMemberAccessForPath(Expression parameter, string path)
        {
            Expression result = parameter;

            string[] memberNames = path.Split('.');

            foreach (string memberName in memberNames)
            {
                PropertyInfo property = FindMember(result.Type, memberName);

                result = Expression.MakeMemberAccess(result, property);
            }

            return result;
        }

        // returns input?.ToString();
        public static Expression ConvertToString(Expression input)
        {
            if (input.Type == typeof (string))
                return input;

            if (input.Type.IsValueType && !input.Type.IsNullable())
                return Expression.Call(input, ObjectToStringMethod);

            ConstantExpression nullConstant = Expression.Constant(null, input.Type);

            ConstantExpression nullString = Expression.Constant(null, typeof (string));

            Expression isNull = Expression.Equal(input, nullConstant);

            MethodCallExpression toStringExpression = Expression.Call(input, ObjectToStringMethod);

            return Expression.Condition(isNull, nullString, toStringExpression);
        }

        public static MethodInfo GetMethod<T>(Expression<Action<T>> methodExpression)
        {
            MethodCallExpression methodCall = (MethodCallExpression)methodExpression.Body;

            MethodInfo result = methodCall.Method;

            if (result.IsGenericMethod)
                result = result.GetGenericMethodDefinition();

            return result;
        }

        public static MethodInfo GetMethod<T>(Expression<Func<T, object>> methodExpression)
        {
            Expression body = methodExpression.Body;

            while (body is UnaryExpression)
            {
                UnaryExpression unaryExpression = (UnaryExpression) body;

                body = unaryExpression.Operand;
            }

            MethodCallExpression methodCall = (MethodCallExpression) body;

            MethodInfo result = methodCall.Method;

            if (result.IsGenericMethod)
                result = result.GetGenericMethodDefinition();

            return result;
        }

        public static string GetPath<T>(Expression<Func<T, object>> pathExpression)
        {
            if (pathExpression == null)
                throw new ArgumentNullException(nameof(pathExpression));

            return GetPathInternal(pathExpression.Body);
        }

        public static string GetPath(LambdaExpression pathExpression)
        {
            if (pathExpression == null)
                throw new ArgumentNullException(nameof(pathExpression));

            return GetPathInternal(pathExpression.Body);
        }
        
        internal static string GetPathInternal(Expression expression)
        {
            bool inLambda = false;

            StringBuilder result = new StringBuilder();

            for (;;)
            {
                switch (expression.NodeType)
                {
                    case ExpressionType.Convert:
                    case ExpressionType.ConvertChecked:
                    case ExpressionType.Quote:
                    case ExpressionType.TypeAs:
                    {
                        UnaryExpression unaryExpression = (UnaryExpression) expression;

                        expression = unaryExpression.Operand;

                        continue;
                    }

                    case ExpressionType.Lambda:
                    {
                        if (inLambda)
                            throw new InvalidOperationException("Unable to derive path from nested lambda expression");

                        inLambda = true;

                        LambdaExpression lambdaExpression = (LambdaExpression) expression;

                        expression = lambdaExpression.Body;

                        continue;
                    }

                    case ExpressionType.Call:
                    {
                        MethodCallExpression methodCallExpression = (MethodCallExpression) expression;

                        if (methodCallExpression.Method.DeclaringType == typeof (Enumerable) &&
                            methodCallExpression.Method.Name == "Select" || methodCallExpression.Method.Name == "SelectMany")
                        {
                            if (result.Length > 0)
                                result.Insert(0, '.');

                            LambdaExpression lambda = (LambdaExpression) methodCallExpression.Arguments[1];

                            result.Insert(0, GetPathInternal(lambda.Body));

                            expression = methodCallExpression.Arguments[0];

                            continue;
                        }

                        goto default;
                    }

                    case ExpressionType.MemberAccess:
                    {
                        MemberExpression memberExpression = (MemberExpression) expression;

                        if (result.Length > 0)
                            result.Insert(0, '.');

                        SourceFieldAttribute sourceFieldAttribute = (SourceFieldAttribute)
                                Attribute.GetCustomAttribute(memberExpression.Member, typeof(SourceFieldAttribute), true);
                        
                        if (sourceFieldAttribute != null && !sourceFieldAttribute.IsDefaultAttribute())
                            result.Insert(0, sourceFieldAttribute.FieldName);
                        else
                            result.Insert(0, memberExpression.Member.Name);
                            
                        expression = memberExpression.Expression;

                        continue;
                    }

                    case ExpressionType.Parameter:
                        return result.ToString();

                    default:
                        throw new InvalidOperationException("Unable to convert expression to path.");
                }
            }
        }


        public static Type GetSourceFieldType(Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            switch (expression.NodeType)
            {
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.Quote:
                    {
                        UnaryExpression unaryExpression = (UnaryExpression)expression;

                        return GetSourceFieldType(unaryExpression.Operand);
                    }

                case ExpressionType.Lambda:
                    {
                        LambdaExpression lambdaExpression = (LambdaExpression)expression;

                        return GetSourceFieldType(lambdaExpression.Body);
                    }

                case ExpressionType.MemberAccess:
                    {
                        MemberExpression memberExpression = (MemberExpression)expression;

                        SourceFieldAttribute sourceFieldAttribute =
                            (SourceFieldAttribute)Attribute.GetCustomAttribute(memberExpression.Member, typeof(SourceFieldAttribute), true) ??
                            SourceFieldAttribute.Default;

                        return sourceFieldAttribute.FieldType;
                    }

                default:
                    return typeof(object);
            }
        }

        public static bool IsSortExpression(Expression expression)
        {
            MethodCallExpression methodCallExpression = expression as MethodCallExpression;

            return methodCallExpression != null &&
                   methodCallExpression.Method.DeclaringType == typeof (Queryable) &&
                   SortMethodNames.Contains(methodCallExpression.Method.Name);
        }

        public static bool IsQueryableMethodCall(this Expression expression, string queryableMethodName)
        {
            MethodCallExpression methodCallExpression = expression as MethodCallExpression;

            return methodCallExpression != null &&
                   methodCallExpression.Method.DeclaringType == typeof (Queryable) &&
                   methodCallExpression.Method.Name == queryableMethodName;
        }

        public static Expression ConvertFromString(Expression expression, Type targetType)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            if (targetType == null)
                throw new ArgumentNullException(nameof(targetType));

            if (expression.Type != typeof (string))
                throw new ArgumentException("expression.Type must be a string", nameof(expression));

            switch (Type.GetTypeCode(targetType))
            {
                case TypeCode.Object:
                case TypeCode.Boolean:
                {
                    TypeConverter typeConverter = TypeDescriptor.GetConverter(targetType);

                    if (typeConverter.CanConvertFrom(typeof (string)))
                    {
                        ConstantExpression typeConverterExpression = Expression.Constant(typeConverter);

                        MethodInfo convertMethod = typeConverter.GetType()
                            .GetMethod("ConvertFrom", new[]
                            {
                                typeof (ITypeDescriptorContext),
                                typeof (CultureInfo),
                                typeof (object)
                            });

                        ConstantExpression currentCultureExpression = Expression.Constant(CultureInfo.CurrentCulture);

                        ConstantExpression nullTypeDescriptorContext = Expression.Constant(null,
                            typeof (ITypeDescriptorContext));

                        MethodCallExpression convertFromCallExpression = Expression.Call(typeConverterExpression,
                            convertMethod, nullTypeDescriptorContext, currentCultureExpression, expression);

                        return Expression.Convert(convertFromCallExpression, targetType);
                    }

                    break;
                }

                case TypeCode.Int32:
                {
                    if (targetType.IsEnum)
                    {
                        TypeConverter typeConverter = TypeDescriptor.GetConverter(targetType);

                        if (typeConverter.CanConvertFrom(typeof(string)))
                        {
                            ConstantExpression typeConverterExpression = Expression.Constant(typeConverter);

                            MethodInfo convertMethodInfo = typeConverter.GetType()
                                .GetMethod("ConvertFrom", new[]
                                {
                                    typeof (ITypeDescriptorContext),
                                    typeof (CultureInfo),
                                    typeof (Enum)
                                });

                            ConstantExpression currentCultureExpression = Expression.Constant(CultureInfo.CurrentCulture);

                            ConstantExpression nullTypeDescriptorContext = Expression.Constant(null,
                                typeof(ITypeDescriptorContext));

                            MethodCallExpression convertFromCallExpression = Expression.Call(typeConverterExpression,
                                convertMethodInfo, nullTypeDescriptorContext, currentCultureExpression, expression);

                            return Expression.Convert(convertFromCallExpression, targetType);
                        }
                    }

                    MethodInfo convertMethod = GetMethod<string>(x => int.Parse(x));

                    return Expression.Call(convertMethod, expression);
                }

                case TypeCode.DateTime:
                {
                    MethodInfo convertMethod = GetMethod<string>(x => DateTime.Parse(x));

                    return Expression.Call(convertMethod, expression);
                }

                case TypeCode.String:
                    return expression;
            }

            throw new InvalidOperationException(
                $"There is no method registered to convert a string to a {targetType.FullName}");
        }

        public static Expression UnwrapConversionExpressions(Expression expression)
        {
            for (;;)
            {
                switch (expression.NodeType)
                {
                    case ExpressionType.Convert:
                    case ExpressionType.ConvertChecked:
                    case ExpressionType.Quote:
                    case ExpressionType.TypeAs:
                        UnaryExpression unaryExpression = (UnaryExpression) expression;
                        expression = unaryExpression.Operand;
                        break;

                    default:
                        return expression;
                }
            }
        }

        public static LambdaExpression UnwrapLambdaExpressions(Expression expression)
        {
            while (expression.NodeType == ExpressionType.Quote)
            {
                UnaryExpression unaryExpression = (UnaryExpression) expression;
                expression = unaryExpression.Operand;
            }

            return expression as LambdaExpression;
        }

        public static object Evaluate(Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            if (!IsParameterless(expression))
                throw new ArgumentException("The expression tree contains a ParameterExpression.", nameof(expression));

            if (IsTypeConversionsOfNull(expression))
                return null;

            expression = Expression.Convert(expression, typeof(object));

            Func<object> lambda = Expression.Lambda<Func<object>>(expression).Compile();

            return lambda();
        }

        private static bool IsTypeConversionsOfNull(Expression expression)
        {
            while (expression is UnaryExpression)
            {
                UnaryExpression unaryExpression = (UnaryExpression) expression;

                if (unaryExpression.NodeType != ExpressionType.Convert &&
                    unaryExpression.NodeType != ExpressionType.ConvertChecked)
                    break;

                expression = unaryExpression.Operand;
            }

            ConstantExpression constant = expression as ConstantExpression;

            return constant != null && string.IsNullOrEmpty(constant.Value?.ToString());
        }

        public static ParameterExpression[] GetParameters(Expression expression)
        {
            ParameterExpressionVisitor visitor = new ParameterExpressionVisitor();

            visitor.Visit(expression);

            return visitor.Parameters.ToArray();
        }

        public static bool IsParameterless(Expression expression)
        {
            ParameterExpressionVisitor visitor = new ParameterExpressionVisitor();

            visitor.Visit(expression);

            return visitor.Parameters.Count == 0;
        }

        private sealed class ParameterExpressionVisitor : ExpressionVisitor
        {
            readonly HashSet<ParameterExpression> _parameters = new HashSet<ParameterExpression>();

            public HashSet<ParameterExpression> Parameters => _parameters;

            protected override Expression VisitParameter(ParameterExpression node)
            {
                _parameters.Add(node);

                return base.VisitParameter(node);
            }
        }

        public static Expression Replace(this Expression expression, Expression find, Expression replaceWith)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            if (find == null)
                throw new ArgumentNullException(nameof(find));

            if (replaceWith == null)
                throw new ArgumentNullException(nameof(replaceWith));

            ExpressionVisitor visitor = new FindAndReplaceExpressionVisitor(find, replaceWith);

            return visitor.Visit(expression);
        }

        private sealed class FindAndReplaceExpressionVisitor : ExpressionVisitor
        {
            private readonly Expression _find;
            private readonly Expression _replaceWith;

            public FindAndReplaceExpressionVisitor(Expression find, Expression replaceWith)
            {
                _find = find;
                _replaceWith = replaceWith;
            }

            public override Expression Visit(Expression node)
            {
                return node == _find ? _replaceWith : base.Visit(node);
            }
        }

        public static Expression<Func<T, bool>> Any<T>(
            params Expression<Func<T, bool>>[] predicates)
        {
            return Any((IEnumerable<Expression<Func<T, bool>>>) predicates);
        }

        public static Expression<Func<T, bool>> Any<T>(
            IEnumerable<Expression<Func<T, bool>>> predicates)
        {
            if (predicates == null)
                throw new ArgumentNullException(nameof(predicates));

            predicates = predicates.Where(p => p != null);

            using (IEnumerator<Expression<Func<T, bool>>> enumerator = predicates.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                    return x => false; // Shortcut exist when there are no predicates in the set.

                Expression<Func<T, bool>> first = enumerator.Current;

                if (!enumerator.MoveNext())
                    return first; // Shortcut when there is only one predicate in the set.

                // Combine the body of the first two predicates...

                ParameterExpression parameter = Expression.Parameter(typeof (T), "x");

                Expression body = Expression.OrElse(
                    first.Body.Replace(first.Parameters[0], parameter),
                    enumerator.Current.Body.Replace(enumerator.Current.Parameters[0], parameter));

                // Combine the body of any remaining predicates

                while (enumerator.MoveNext())
                {
                    body = Expression.OrElse(body,
                        enumerator.Current.Body.Replace(enumerator.Current.Parameters[0], parameter));
                }

                // return the final lambda

                return Expression.Lambda<Func<T, bool>>(body, parameter);
            }
        }

        public static Expression<Func<T, bool>> All<T>(
            params Expression<Func<T, bool>>[] predicates)
        {
            return All((IEnumerable<Expression<Func<T, bool>>>) predicates);
        }

        public static Expression<Func<T, bool>> All<T>(
            IEnumerable<Expression<Func<T, bool>>> predicates)
        {
            if (predicates == null)
                throw new ArgumentNullException(nameof(predicates));

            predicates = predicates.Where(p => p != null);

            using (IEnumerator<Expression<Func<T, bool>>> enumerator = predicates.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                    return x => true; // Shortcut exist when there are no predicates in the set.

                Expression<Func<T, bool>> first = enumerator.Current;

                if (!enumerator.MoveNext())
                    return first; // Shortcut when there is only one predicate in the set.

                // Combine the body of the first two predicates...

                ParameterExpression parameter = Expression.Parameter(typeof(T), "x");

                Expression body = Expression.AndAlso(
                    first.Body.Replace(first.Parameters[0], parameter),
                    enumerator.Current.Body.Replace(enumerator.Current.Parameters[0], parameter));

                // Combine the body of any remaining predicates

                while (enumerator.MoveNext())
                {
                    body = Expression.AndAlso(body,
                        enumerator.Current.Body.Replace(enumerator.Current.Parameters[0], parameter));
                }

                // return the final lambda

                return Expression.Lambda<Func<T, bool>>(body, parameter);
            }
        }
    }
}
