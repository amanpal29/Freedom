using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Freedom.FullTextSearch
{
    public static class QueryableFullTextSearchExtensions
    {
        public static IQueryable<TSource> BestMatch<TSource>(this IQueryable<TSource> source, string searchText)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (string.IsNullOrEmpty(searchText))
                throw new ArgumentNullException(nameof(searchText));

            return source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    ((MethodInfo) MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof (TSource)),
                    source.Expression, Expression.Constant(searchText)));
        }

        public static IQueryable<TSource> ContainsText<TSource>(this IQueryable<TSource> source, string searchText)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.Provider.CreateQuery<TSource>(
                Expression.Call(
                    null,
                    ((MethodInfo) MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof (TSource)),
                    source.Expression, Expression.Constant(searchText, typeof(string))));
        }
    }
}
