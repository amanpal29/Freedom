using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Freedom
{
    public static class DelegateFactory
    {
        public static Func<TIn, TOut> PropertyGetFunc<TIn, TOut>(Type input, string propertyName)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException(nameof(propertyName));

            PropertyInfo propertyInfo = input.GetProperty(propertyName);

            if (propertyInfo == null)
                throw new ArgumentException(
                    $"The property {propertyName} was not found on type {input.FullName}.",
                    nameof(propertyName));

            ParameterExpression parameterExpression = Expression.Parameter(typeof(TIn), "p");

            Expression inputExpression = typeof (TIn) != input
                ? (Expression) Expression.Convert(parameterExpression, input)
                : parameterExpression;

            Expression selector = Expression.MakeMemberAccess(inputExpression, propertyInfo);

            if (typeof (TOut) != propertyInfo.PropertyType)
                selector = Expression.Convert(selector, typeof (TOut));

            Expression<Func<TIn, TOut>> labmda = Expression.Lambda<Func<TIn, TOut>>(selector, parameterExpression);

            return labmda.Compile();
        }

        public static Action<TInstance, TValue> PropertySetAction<TInstance, TValue>(
            Expression<Func<TInstance, TValue>> propertyGetExpression)
        {
            if (propertyGetExpression == null)
                throw new ArgumentNullException(nameof(propertyGetExpression));

            MemberExpression memberExpression = propertyGetExpression.Body as MemberExpression;

            if (!(memberExpression?.Member is PropertyInfo))
                throw new ArgumentException("propertyGetExpression must be a property member access expression", nameof(propertyGetExpression));

            PropertyInfo property = (PropertyInfo) memberExpression.Member;

            return PropertySetAction<TInstance, TValue>(property);
        }

        public static Action<TInstance, TValue> PropertySetAction<TInstance, TValue>(PropertyInfo property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            if (!property.CanWrite)
                throw new ArgumentException($"The property {property.Name} is readonly.", nameof(property));

            ParameterExpression instanceParam = Expression.Parameter(typeof(TInstance));

            ParameterExpression valueParam = Expression.Parameter(typeof(TValue));

            MethodCallExpression call = Expression.Call(instanceParam, property.GetSetMethod(), valueParam);

            Expression<Action<TInstance, TValue>> lambda =
                Expression.Lambda<Action<TInstance, TValue>>(call, instanceParam, valueParam);

            return lambda.Compile();
        }
    }
}
