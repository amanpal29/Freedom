using System;
using System.Linq.Expressions;

namespace Freedom.ViewModels.Filters
{
    public interface IFilterViewModel<T>
    {
        Func<T, bool> Predicate { get; }

        Expression<Func<T, bool>> Expression { get; }
    }
}
