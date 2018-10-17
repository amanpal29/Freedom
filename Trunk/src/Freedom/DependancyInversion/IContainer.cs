using System;
using System.Collections;

namespace Freedom.DependancyInversion
{
    public interface IContainer
    {
        object GetInstance(Type type);
        object TryGetInstance(Type type);
        IEnumerable GetAllInstances(Type type);
    }
}