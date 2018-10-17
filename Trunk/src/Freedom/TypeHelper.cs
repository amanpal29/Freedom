using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Freedom
{
    public static class TypeHelper
    {
        public static Type GetElementType(Type type)
        {
            Type closedEnumerableType = GetClosedEnumerableType(type);

            return closedEnumerableType == null
                       ? type
                       : closedEnumerableType.GetGenericArguments()[0];
        }

        public static MethodInfo GetImplementation(Type type, MethodInfo interfaceMethod)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (interfaceMethod == null)
                throw new ArgumentNullException(nameof(interfaceMethod));

            if (interfaceMethod.DeclaringType == null || !interfaceMethod.DeclaringType.IsInterface)
                throw new ArgumentException("interfaceMethod must be a member declared on an interface type", nameof(interfaceMethod));

            InterfaceMapping interfaceMap = type.GetInterfaceMap(interfaceMethod.DeclaringType);

            int index = Array.IndexOf(interfaceMap.InterfaceMethods, interfaceMethod);

            return interfaceMap.TargetMethods[index];
        }

        public static Type GetCollectionElementType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == typeof (ICollection<>))
                return type.GenericTypeArguments[0];

            foreach (Type interfaceType in type.GetInterfaces())
                if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof (ICollection<>))
                    return interfaceType.GenericTypeArguments[0];

            return null;
        }

        public static Type GetClosedEnumerableType(Type type)
        {
            // Don't try to get IEnumerable<> for null or strings.
            // Strings would be treated like IEnumerable<char>.  We don't want that.
            if (type == null || type == typeof(string))
                return null;

            // If it's an array, get IEnumerable of the array type
            if (type.IsArray)
                return typeof(IEnumerable<>).MakeGenericType(type.GetElementType());

            // If this is a generic type, see if any IEnumerable<> of any of the generic arguments works
            if (type.IsGenericType)
            {
                foreach (Type arg in type.GetGenericArguments())
                {
                    Type ienum = typeof(IEnumerable<>).MakeGenericType(arg);

                    if (ienum.IsAssignableFrom(type))
                        return ienum;
                }
            }

            // Check if we can get IEnumerable from any of the interfaces of this type
            foreach (Type interfaceType in type.GetInterfaces())
            {
                Type ienum = GetClosedEnumerableType(interfaceType);
                if (ienum != null) return ienum;
            }

            // Check the base type of this type
            if (type.BaseType != null && type.BaseType != typeof(object))
            {
                return GetClosedEnumerableType(type.BaseType);
            }

            return null;
        }

        public static bool IsInstanceOfGenericTypeDefinition(object instance, Type genericTypeDefinition)
        {
            if (genericTypeDefinition == null)
                throw new ArgumentNullException(nameof(genericTypeDefinition));

            if (!genericTypeDefinition.IsGenericTypeDefinition)
                throw new ArgumentException("genericTypeDefinition must be a generic type definition.", nameof(genericTypeDefinition));

            if (genericTypeDefinition.IsInterface)
                throw new ArgumentException("genericTypeDefinition must not be an interface type.", nameof(genericTypeDefinition));

            if (instance == null)
                return false;

            Type type = instance.GetType();

            while (type != null && type != typeof(object))
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == genericTypeDefinition)
                    return true;

                type = type.BaseType;
            }

            return false;
        }

        public static bool IsSubclassOfGenericType(Type type, Type genericTypeDefinition)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (genericTypeDefinition == null)
                throw new ArgumentNullException(nameof(genericTypeDefinition));

            if (!genericTypeDefinition.IsGenericTypeDefinition)
                throw new ArgumentException("genericTypeDefinition should be an open generic type",
                                            nameof(genericTypeDefinition));

            for (Type current = type; current != null; current = current.BaseType)
                if (current.IsGenericType && current.GetGenericTypeDefinition() == genericTypeDefinition)
                    return true;

            return false;
        }

        public static Type FindClosedGenericBaseType(Type type, Type openGenericTypeDefinition)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (openGenericTypeDefinition == null)
                throw new ArgumentNullException(nameof(openGenericTypeDefinition));

            if (!openGenericTypeDefinition.IsGenericTypeDefinition)
                throw new ArgumentException("openGenericTypeDefinition should be an open generic type",
                                            nameof(openGenericTypeDefinition));

            for (Type current = type; current != null; current = current.BaseType)
                if (current.IsGenericType && current.GetGenericTypeDefinition() == openGenericTypeDefinition)
                    return current;

            return null;
        }

        public static bool IsCovariantOfGenericType(Type parentType, Type childType)
        {
            if (parentType == null)
                throw new ArgumentNullException(nameof(parentType));

            if (!parentType.IsGenericType)
                throw new ArgumentException("parentType must be a closed generic type.", nameof(parentType));

            if (childType == null)
                return false;

            Type openGeneric = parentType.GetGenericTypeDefinition();
            Type[] parentArgs = parentType.GetGenericArguments();

            List<Type> candidateTypes = new List<Type>();

            if (openGeneric.IsInterface)
            {
                if (childType.IsInterface && childType.IsGenericType && childType.GetGenericTypeDefinition() == openGeneric)
                    candidateTypes.Add(childType);

                foreach (Type childInterface in childType.GetInterfaces())
                    if (childInterface.IsGenericType && childInterface.GetGenericTypeDefinition() == openGeneric)
                        candidateTypes.Add(childInterface);
            }
            else
            {
                for (Type hierarchyType = childType; hierarchyType != null; hierarchyType = hierarchyType.BaseType)
                    if (hierarchyType.IsGenericType && hierarchyType.GetGenericTypeDefinition() == openGeneric)
                        candidateTypes.Add(hierarchyType);
            }

            foreach (Type candidateType in candidateTypes)
            {
                Type[] candidateArgs = candidateType.GetGenericArguments();

                if (parentArgs.Length != candidateArgs.Length)
                    continue;

                if (!parentArgs.Where((t, i) => !t.IsAssignableFrom(candidateArgs[i])).Any())
                    return true;
            }

            return false;
        }

        public static object CreateInstanceOfGenericType(Type genericTypeDefinition, Type genericTypeArgument, params object[] constructorArguments)
        {
            Type closedGenericType = genericTypeDefinition.MakeGenericType(genericTypeArgument);

            return Activator.CreateInstance(closedGenericType, constructorArguments);
        }

        public static Type GetPropertyType(Type baseType, string path)
        {
            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic |
                                              BindingFlags.Instance | BindingFlags.Static;

            if (string.IsNullOrEmpty(path))
                return null;

            string[] pathParts = path.Split('.');

            Type currentType = baseType;

            foreach (string part in pathParts)
            {
                PropertyInfo property = currentType.GetProperty(part, bindingFlags) ??
                                        GetElementType(currentType).GetProperty(part, bindingFlags);
                
                if (property == null)
                    return null;

                currentType = property.PropertyType;
            }

            return currentType;
        }

        public static T GetStatic<T>(Type type, string name)
        {
            PropertyInfo propertyInfo = type.GetProperty(name,
                BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            if (propertyInfo != null && typeof (T).IsAssignableFrom(propertyInfo.PropertyType))
                return (T) propertyInfo.GetValue(null, null);

            FieldInfo fieldInfo = type.GetField(name,
                BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            if (fieldInfo != null && typeof (T).IsAssignableFrom(fieldInfo.FieldType))
                return (T) fieldInfo.GetValue(null);

            return default(T);
        }
    }
}
