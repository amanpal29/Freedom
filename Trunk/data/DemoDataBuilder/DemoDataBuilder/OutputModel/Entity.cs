using System;
using System.Linq;
using System.Reflection;

namespace DemoDataBuilder.OutputModel
{
    public abstract class Entity
    {
        public static Type[] GetChildTypes()
        {
            return Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof (Entity).IsAssignableFrom(t)).ToArray();
        }

        public Guid Id = Guid.NewGuid();
    }
}
