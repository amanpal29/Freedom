using System;
using System.Linq;
using System.Reflection;

namespace Freedom.DependancyInversion
{
    public static class DependancyInjectionExtensions
    {
        /// <summary>
        /// Creates an instance of the object of the specified type.  Passing in matching instances
        /// from the objects collection as arguments to the constructor.
        /// 
        /// The order of objects in the objects collection does not need to match the order of the parameters
        /// in the constructor.
        /// 
        /// For each parameter of the constructor, the first object in the objects collection that can
        /// be cast to that parameter's type will be used.
        /// 
        /// If there is no matching object in the objects collection for the parameter of the constructor, the default
        /// for that parameter's type will be used.
        /// 
        /// If the constructor throws an ArgumentNullException, and there is more than one public constructor for that object,
        /// another contrructor will be tried.
        /// 
        /// Only public Constructors will be tried, starting with the constructor with the most parameters,
        /// moving on to the constructors with less parameters, and finally the default constructor will be
        /// called if it exists.
        /// </summary>
        /// <returns>An instance of the specified type</returns>
        public static object CreateWith(this Type type, params object[] objects)
        {
            ConstructorInfo[] constructors = type.GetConstructors()
                .OrderByDescending(t => t.GetParameters().Length).ToArray();

            for (int i = 0; i < constructors.Length; i++)
            {
                ConstructorInfo constructor = constructors[i];

                try
                {
                    return constructor.Invoke(FindParameters(constructor.GetParameters(), objects));
                }
                catch (TargetInvocationException exception)
                {
                    // If the construction fails because one of the arguments was null,
                    // and there are more constructors to try, try the next constructor.
                    if (exception.InnerException is ArgumentNullException && i < constructors.Length - 1)
                        continue;

                    // Otherwise throw the inner exception
                    throw exception.InnerException;
                }
            }

            // This should be unreachable...

            return null;
        }

        public static object[] FindParameters(ParameterInfo[] parameters, params object[] objects)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            if (parameters.Length == 0)
                return new object[0];

            if (objects == null)
                throw new ArgumentNullException(nameof(objects));

            if (objects.Length == 0)
                throw new ArgumentException("objects cannot be an empty array when parameters is non-empty array .", nameof(objects));

            object[] result = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                result[i] = objects.FirstOrDefault(o => parameters[i].ParameterType.IsInstanceOfType(o));
            }

            return result;
        }
        
    }
}
