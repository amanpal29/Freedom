using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;

namespace Freedom.DependancyInversion
{
    public class Container : IContainer
    {
        #region Fields

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Dictionary<string, List<Func<object>>> _factories = new Dictionary<string, List<Func<object>>>();

        #endregion

        #region Registration Methods

        #region Add Methods

        private void Add(Type type, Func<object> factoryMethod)
        {
            string typeName = type.AssemblyQualifiedName;

            if (!string.IsNullOrEmpty(typeName))
            {
                if (!_factories.ContainsKey(typeName))
                    _factories.Add(typeName, new List<Func<object>>());

                _factories[typeName].Add(factoryMethod);
            }
        }

        public void Add<TObject>() where TObject : new()
        {
            Add(typeof (TObject), () => new TObject());
        }

        public void Add<TObject, TInstance>() where TInstance : TObject, new()
        {
            Add(typeof(TObject), () => new TInstance());
        }

        public void Add<TObject>(TObject instance)
        {
            Add(typeof(TObject), () => instance);
        }

        public void Add<TObject>(Func<TObject> factoryMethod)
        {
            if (factoryMethod == null) throw new ArgumentNullException(nameof(factoryMethod));

            Add(typeof (TObject), () => factoryMethod());
        }

        #endregion

        #region Use Methods

        private void Use(Type type, Func<object> factoryMethod)
        {
            string typeName = type.AssemblyQualifiedName;

            if (!string.IsNullOrEmpty(typeName))
            {
                List<Func<object>> factoryMethods;

                if (_factories.ContainsKey(typeName))
                {
                    factoryMethods = _factories[typeName];

                    factoryMethods.Clear();
                }
                else
                {
                    factoryMethods = new List<Func<object>>();

                    _factories.Add(typeName, factoryMethods);
                }

                factoryMethods.Add(factoryMethod);
            }
        }

        public void Use<TObject>() where TObject : new()
        {
            Use(typeof(TObject), () => new TObject());
        }

        public void Use<TObject, TInstance>() where TInstance : TObject, new()
        {
            Use(typeof(TObject), () => new TInstance());
        }

        public void Use<TObject>(TObject instance)
        {
            Use(typeof(TObject), () => instance);
        }

        public void Use<TObject>(Func<TObject> factoryMethod)
        {
            if (factoryMethod == null) throw new ArgumentNullException(nameof(factoryMethod));

            Use(typeof(TObject), () => factoryMethod());
        }

        #endregion

        #region Scan Methods

        public void ScanWithDefaultConventions(Assembly assembly)
        {
            IEnumerable<Type> interfaces = assembly.GetTypes().Where(type => type.IsInterface);

            foreach (Type type in interfaces)
            {
                if (string.IsNullOrEmpty(type.AssemblyQualifiedName)) continue;
                
                if (_factories.ContainsKey(type.AssemblyQualifiedName)) continue;

                if (type.Name.StartsWith("I") && type.Name.Length > 1)
                {
                    string concreteTypeName = $"{type.Namespace}.{type.Name.Substring(1)}";

                    Type concreteType = assembly.GetType(concreteTypeName, false);

                    if (concreteType?.GetConstructor(Type.EmptyTypes) != null)
                    {
                        Log.InfoFormat("Scan registered the default instance for type: {0}", type.FullName);
                        Use(type, () => Activator.CreateInstance(concreteType));
                    }
                }
            }
        }
        
        public void ScanForAllInstancesOfType(Assembly assembly, Type type, Func<Type, bool> predicate = null)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            if (type == null)
                throw new ArgumentNullException(nameof(type));

            IEnumerable<Type> implementations = assembly.GetTypes()
                .Where(type.IsAssignableFrom)
                .Where(ct => ct.IsClass && !ct.IsAbstract && !ct.IsGenericTypeDefinition);

            if (predicate != null)
            {
                implementations = implementations.Where(predicate);
            }

            foreach (Type concreteType in implementations)
            {
                if (concreteType.GetConstructor(Type.EmptyTypes) == null)
                    continue;
                try
                {
                    object instance = Activator.CreateInstance(concreteType);
                    Add(type, () => instance);
                    Log.InfoFormat("Scan registerd concrete type {0} for type {1}", concreteType.Name, type.Name);
                }
                catch (Exception exception)
                {
                    Log.Warn($"An exception occurred while trying to create an instance of {concreteType.Name}",
                        exception);
                }
            }
        }

        public void RegisterAssembly(Assembly assembly)
        {
            IEnumerable<Type> registrationTypes = assembly.GetTypes()
                .Where(type => typeof (IRegister).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract);

            foreach (Type registrationType in registrationTypes)
            {
                ConstructorInfo defaultConstructor = registrationType.GetConstructor(new Type[0]);

                if (defaultConstructor != null)
                {
                    try
                    {
                        IRegister instance = (IRegister) defaultConstructor.Invoke(new object[0]);

                        instance.Register(this);

                        Log.InfoFormat("Registered plug-in {0}", registrationType.AssemblyQualifiedName);
                    }
                    catch (Exception ex)
                    {
                        Log.Warn(
                            $"An error occurred while trying to register type {registrationType.AssemblyQualifiedName}.",
                            ex);
                    }
                }
                else
                {
                    Log.WarnFormat("Unable to register type {0}, there is no default constructor.",
                                   registrationType.AssemblyQualifiedName);
                }
            }
        }

        public void ScanAssembliesInPath(string path, string searchPattern)
        {
            IEnumerable<string> assemblyPaths =
                Directory.GetFiles(path, searchPattern, SearchOption.TopDirectoryOnly)
                    .Where(IsExecutable);

            foreach (string assemblyPath in assemblyPaths)
            {
                try
                {
                    Assembly assembly = Assembly.LoadFrom(assemblyPath);

                    if (assembly != null)
                    {
                        RegisterAssembly(assembly);
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Log.Warn($"An ReflectionTypeLoadException occurred when scanning assembly {assemblyPath}.", ex);

                    if (ex.LoaderExceptions == null) continue;

                    foreach (Exception exception in ex.LoaderExceptions)
                        Log.Warn("Loader Exception", exception);
                }
                catch (Exception ex)
                {
                    Log.Warn($"An error occurred when scanning assembly {assemblyPath}.", ex);
                }
            }
        }

        private static bool IsExecutable(string fileName)
        {
            string extension = Path.GetExtension(fileName);

            return
                string.Compare(extension, ".exe", StringComparison.OrdinalIgnoreCase) == 0 ||
                string.Compare(extension, ".dll", StringComparison.OrdinalIgnoreCase) == 0;
        }

        #endregion

        #region Clear Methods

        public bool Clear(Type type)
        {
            return !string.IsNullOrEmpty(type?.AssemblyQualifiedName) && _factories.Remove(type.AssemblyQualifiedName);
        }

        public bool Clear<TObject>()
        {
            string assemblyQualifiedName = typeof (TObject).AssemblyQualifiedName;

            return !string.IsNullOrEmpty(assemblyQualifiedName) && _factories.Remove(assemblyQualifiedName);
        }

        #endregion

        #endregion

        #region Get Method

        public T Get<T>()
        {
            return (T) GetInstance(typeof(T));
        }

        #endregion

        #region Implementation of IContainer

        public object GetInstance(Type type)
        {
            if (string.IsNullOrEmpty(type?.AssemblyQualifiedName))
                throw new ArgumentNullException(nameof(type));

            if (!_factories.ContainsKey(type.AssemblyQualifiedName))
            {
                string message = $"The type {type.AssemblyQualifiedName} was not registered in the container.";

                throw new TypeNotRegisteredException(message);
            }

            object instance;

            List<Func<object>> factoryMethods = _factories[type.AssemblyQualifiedName];

            if (factoryMethods.Count > 1)
            {
                string message =
                    $"There is more than one factory registered for type {type.AssemblyQualifiedName}, try GetAll() instead.";

                throw new ConstructionFailedException(message);
            }

            try
            {
                Func<object> factoryMethod = factoryMethods[0];

                instance = factoryMethod();
            }
            catch (TargetInvocationException exception)
            {
                string message = $"An error occurred while tring to create an instance of {type.AssemblyQualifiedName}.";

                throw new ConstructionFailedException(message, exception.InnerException);
            }
            catch (Exception exception)
            {
                string message = $"An error occurred while tring to create an instance of {type.AssemblyQualifiedName}.";

                throw new ConstructionFailedException(message, exception);
            }

            if (instance == null)
                throw new ConstructionFailedException($"The factory method failed to create an instance of {type.AssemblyQualifiedName}.");

            if (!type.IsInstanceOfType(instance))
            {
                string message =
                    $"The factory method created an instance of type {instance.GetType().AssemblyQualifiedName}, when an instance of type {type.AssemblyQualifiedName} was expected.";

                throw new ConstructionFailedException(message);
            }

            return instance;
        }

        public object TryGetInstance(Type type)
        {
            if (string.IsNullOrEmpty(type?.AssemblyQualifiedName))
                throw new ArgumentNullException(nameof(type));

            object instance = null;

            if (_factories.ContainsKey(type.AssemblyQualifiedName))
            {
                List<Func<object>> factoryMethods = _factories[type.AssemblyQualifiedName];

                if (factoryMethods.Count == 1)
                {
                    Func<object> factoryMethod = factoryMethods[0];

                    instance = factoryMethod();

                    if (!type.IsInstanceOfType(instance))
                        instance = null;
                }
            }

            return instance;
        }

        public IEnumerable GetAllInstances(Type type)
        {
            if (string.IsNullOrEmpty(type?.AssemblyQualifiedName))
                throw new ArgumentNullException(nameof(type));

            return _factories.ContainsKey(type.AssemblyQualifiedName)
                ? _factories[type.AssemblyQualifiedName].Select(factoryMethod => factoryMethod()) 
                : Enumerable.Empty<object>();
        }

        #endregion
    }
}
