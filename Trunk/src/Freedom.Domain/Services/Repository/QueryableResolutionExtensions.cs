using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Freedom.Linq;

namespace Freedom.Domain.Services.Repository
{
    public static class QueryableResolutionExtensions
    {
        public static IQueryable<TSource> Include<TSource>(this IQueryable<TSource> source, string path)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            return source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    ((MethodInfo) MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof (TSource)),
                    source.Expression, Expression.Constant(path)));
        }

        public static IQueryable<TSource> Include<TSource>(this IQueryable<TSource> source,
            Expression<Func<TSource, object>> path)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (path == null)
                throw new ArgumentNullException(nameof(path));

            return source.Include(ExpressionHelper.GetPath(path));
        }
    }
}