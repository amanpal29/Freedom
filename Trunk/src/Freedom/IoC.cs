using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Freedom.DependancyInversion;

namespace Freedom
{
    public static class IoC
    {
        private const string NoContainerMessage = "There is no IoC container registered.";

        public static IContainer Container { get; set; }

        public static T Get<T>()
        {
            if (Container == null) throw new InvalidOperationException(NoContainerMessage);

            return (T) Container.GetInstance(typeof(T));
        }

        public static T TryGet<T>()
        {
            if (Container == null)
                return default(T);

            return (T)Container.TryGetInstance(typeof(T));
        }

        public static IEnumerable<T> GetAll<T>()
        {
            if (Container == null) throw new InvalidOperationException(NoContainerMessage);

            IEnumerable instances = Container.GetAllInstances(typeof(T));

            return instances?.Cast<T>() ?? Enumerable.Empty<T>();
        }
    }
}
