using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Freedom.ComponentModel;

namespace Freedom.FullTextSearch
{
    public class PropertyMetadata<T>
    {
        private readonly PropertyDescriptor _propertyDescriptor;
        private readonly Func<T, string> _getter;

        public PropertyMetadata(PropertyDescriptor propertyDescriptor)
        {
            _propertyDescriptor = propertyDescriptor;
            _getter = BuildGetMethod(propertyDescriptor);

            SearchWeightAttribute searchWeightAttribute =
                _propertyDescriptor.Attributes.OfType<SearchWeightAttribute>().FirstOrDefault();

            SearchWeight = searchWeightAttribute?.SearchWeight ?? 0;

            IndexHintAttribute indexHintAttribute =
                _propertyDescriptor.Attributes.OfType<IndexHintAttribute>().FirstOrDefault();

            IndexHint = indexHintAttribute?.IndexHints ?? IndexHints.None;
        }

        private static Func<T, string> BuildGetMethod(PropertyDescriptor propertyDescriptor)
        {
            ParameterExpression parameter = Expression.Parameter(typeof (T), "x");

            const BindingFlags flags = BindingFlags.FlattenHierarchy | BindingFlags.Instance |
                                       BindingFlags.Public | BindingFlags.NonPublic;

            PropertyInfo property = typeof (T).GetProperty(propertyDescriptor.Name, flags);

            Expression expression = Expression.MakeMemberAccess(parameter, property);

            if (property.PropertyType != typeof (string))
            {
                MethodInfo toStringMethod = property.PropertyType.GetMethod("ToString", new Type[0]);

                MethodCallExpression asString = Expression.Call(expression, toStringMethod);

                if (property.PropertyType.IsValueType)
                {
                    expression = asString;

                }
                else
                {
                    ConstantExpression nullConstant = Expression.Constant(null, property.PropertyType);

                    BinaryExpression test = Expression.MakeBinary(ExpressionType.NotEqual, expression, nullConstant);

                    ConstantExpression nullString = Expression.Constant(null, typeof (string));

                    expression = Expression.Condition(test, asString, nullString, typeof (string));
                }
            }

            Expression<Func<T, string>> lambda = Expression.Lambda<Func<T, string>>(expression, parameter);

            return lambda.Compile();
        }

        public string Name => _propertyDescriptor.Name;

        public Type PropertyType => _propertyDescriptor.PropertyType;

        public double SearchWeight { get; }

        public IndexHints IndexHint { get; }

        public string GetValue(T item)
        {
            return item != null ? _getter(item) : null;
        }
    }
}
