using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Freedom.Extensions
{
    public static class TypeExtensions
    {
        public static TAttribute GetAttribute<TAttribute>(this Type type, bool inherit = false)
            where TAttribute : Attribute
        {
            return (TAttribute) type.GetCustomAttributes(typeof (TAttribute), inherit).FirstOrDefault()
                   ?? GetDefaultAttribute<TAttribute>();
        }

        /// <summary>
        ///     Returns the default value for an attribute.  This uses the following hurestic:
        ///         1.  It looks for a public static field named "Default".
        ///         2.  It creates an attribute with the default constructor and checks if it's IsDefaultAttribute() == true
        ///         3.  It returns null.
        /// </summary> 
        public static TAttribute GetDefaultAttribute<TAttribute>()
            where TAttribute : Attribute
        {
            Attribute attribute = null;

            Type reflect = TypeDescriptor.GetReflectionType(typeof (TAttribute));

            FieldInfo fieldInfo = reflect.GetField("Default",
                                                   BindingFlags.Public | BindingFlags.Static | BindingFlags.GetField);

            if (fieldInfo != null && fieldInfo.IsStatic)
            {
                attribute = (Attribute) fieldInfo.GetValue(null);
            }
            else
            {
                ConstructorInfo constructorInfo = reflect.UnderlyingSystemType.GetConstructor(new Type[0]);

                if (constructorInfo != null)
                {
                    attribute = (Attribute) constructorInfo.Invoke(new object[0]);

                    // If we successfully created, verify that it is the 
                    // default.  Attributes don't have to abide by this rule.
                    if (!attribute.IsDefaultAttribute())
                        attribute = null;
                }
            }

            return (TAttribute) attribute;
        }

        public static bool HasDefaultConstructor(this Type type)
        {
            return TypeDescriptor.GetReflectionType(type).UnderlyingSystemType.GetConstructor(new Type[0]) != null;
        }

        public static bool IsBrowsable(this Type type, bool defaultValue = true)
        {
            BrowsableAttribute browsableAttribute =
                (BrowsableAttribute) type.GetCustomAttributes(typeof (BrowsableAttribute), false).SingleOrDefault();

            return browsableAttribute?.Browsable ?? defaultValue;
        }

        public static bool IsNullable(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>
        /// If type is a nullable type, returns the generic argument, otherwise returns the type.
        /// 
        /// E.g.
        ///     typeof(int).Unnullable() = typeof(int)
        ///     typeof(int?).Unnullable() = typeof(int)
        ///     typeof(SomeClass).Unnullable() = typeof(SomeClass)
        /// </summary>
        public static Type Unnullable(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)
                ? type.GetGenericArguments()[0]
                : type;
        }
    }
}

