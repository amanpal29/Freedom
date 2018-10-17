using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Freedom.Domain.Services.Repository.Linq;
using Freedom.Linq;

namespace Freedom.Domain.Services.Repository
{
    public static class QueryableAsyncExtensions
    {
        // ReSharper disable RedundantCast

        // The casts are redundant in the current version of .NET (4.5 at the time of this comment)
        // because there is only one overload of these methods.  But if additional overloads are
        // added in the future, they might not be.  - DGG 2015-08-12

        private static readonly MethodInfo QueryableAny =
            ExpressionHelper.GetMethod<IQueryable<object>>(x => x.Any());

        private static readonly MethodInfo QueryableAnyPredicate =
            ExpressionHelper.GetMethod<IQueryable<object>>(x => x.Any((Expression<Func<object, bool>>)null));

        private static readonly MethodInfo QueryableCount =
            ExpressionHelper.GetMethod<IQueryable<object>>(x => x.Count());

        private static readonly MethodInfo QueryableCountPredicate =
            ExpressionHelper.GetMethod<IQueryable<object>>(x => x.Count((Expression<Func<object, bool>>)null));

        private static readonly MethodInfo QueryableFirst =
            ExpressionHelper.GetMethod<IQueryable<object>>(x => x.First());

        private static readonly MethodInfo QueryableFirstPredicate =
            ExpressionHelper.GetMethod<IQueryable<object>>(x => x.First((Expression<Func<object, bool>>)null));

        private static readonly MethodInfo QueryableFirstOrDefault =
            ExpressionHelper.GetMethod<IQueryable<object>>(x => x.FirstOrDefault());

        private static readonly MethodInfo QueryableFirstOrDefaultPredicate =
            ExpressionHelper.GetMethod<IQueryable<object>>(x => x.FirstOrDefault((Expression<Func<object, bool>>)null));

        private static readonly MethodInfo QueryableSingle =
            ExpressionHelper.GetMethod<IQueryable<object>>(x => x.Single());

        private static readonly MethodInfo QueryableSinglePredicate =
            ExpressionHelper.GetMethod<IQueryable<object>>(x => x.Single((Expression<Func<object, bool>>)null));

        private static readonly MethodInfo QueryableSingleOrDefault =
            ExpressionHelper.GetMethod<IQueryable<object>>(x => x.SingleOrDefault());

        private static readonly MethodInfo QueryableSingleOrDefaultPredicate =
            ExpressionHelper.GetMethod<IQueryable<object>>(x => x.SingleOrDefault((Expression<Func<object, bool>>)null));

        // ReSharper restore RedundantCast

        #region Any

        public static Task<bool> AnyAsync<TSource>(this IQueryable<TSource> source)
        {
            IAsyncQueryProvider asyncQueryProvider = source.Provider as IAsyncQueryProvider;

            if (asyncQueryProvider == null)
                throw new NotSupportedException("The query provider is not an IAsyncQueryProvider");

            Expression expression = Expression.Call(null, QueryableAny.MakeGenericMethod(typeof(TSource)), source.Expression);

            return asyncQueryProvider.ExecuteAsync<bool>(expression);
        }

        public static Task<bool> AnyAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            IAsyncQueryProvider asyncQueryProvider = source.Provider as IAsyncQueryProvider;

            if (asyncQueryProvider == null)
                throw new NotSupportedException("The query provider is not an IAsyncQueryProvider");

            Expression expression = Expression.Call(null, QueryableAnyPredicate.MakeGenericMethod(typeof(TSource)), source.Expression, predicate);

            return asyncQueryProvider.ExecuteAsync<bool>(expression);
        }

        #endregion

        #region Count

        public static Task<int> CountAsync<TSource>(this IQueryable<TSource> source)
        {
            IAsyncQueryProvider asyncQueryProvider = source.Provider as IAsyncQueryProvider;

            if (asyncQueryProvider == null)
                throw new NotSupportedException("The query provider is not an IAsyncQueryProvider");

            Expression expression = Expression.Call(null, QueryableCount.MakeGenericMethod(typeof(TSource)), source.Expression);

            return asyncQueryProvider.ExecuteAsync<int>(expression);
        }

        public static Task<int> CountAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            IAsyncQueryProvider asyncQueryProvider = source.Provider as IAsyncQueryProvider;

            if (asyncQueryProvider == null)
                throw new NotSupportedException("The query provider is not an IAsyncQueryProvider");

            Expression expression = Expression.Call(null, QueryableCountPredicate.MakeGenericMethod(typeof(TSource)), source.Expression, predicate);

            return asyncQueryProvider.ExecuteAsync<int>(expression);
        }

        #endregion

        #region First / FirstOrDefault

        public static Task<TSource> FirstAsync<TSource>(this IQueryable<TSource> source)
        {
            IAsyncQueryProvider asyncQueryProvider = source.Provider as IAsyncQueryProvider;

            if (asyncQueryProvider == null)
                throw new NotSupportedException("The query provider is not an IAsyncQueryProvider");

            Expression expression = Expression.Call(null, QueryableFirst.MakeGenericMethod(typeof(TSource)), source.Expression);

            return asyncQueryProvider.ExecuteAsync<TSource>(expression);
        }

        public static Task<TSource> FirstAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            IAsyncQueryProvider asyncQueryProvider = source.Provider as IAsyncQueryProvider;

            if (asyncQueryProvider == null)
                throw new NotSupportedException("The query provider is not an IAsyncQueryProvider");

            Expression expression = Expression.Call(null, QueryableFirstPredicate.MakeGenericMethod(typeof(TSource)), source.Expression, predicate);

            return asyncQueryProvider.ExecuteAsync<TSource>(expression);
        }

        public static Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source)
        {
            IAsyncQueryProvider asyncQueryProvider = source.Provider as IAsyncQueryProvider;

            if (asyncQueryProvider == null)
                throw new NotSupportedException("The query provider is not an IAsyncQueryProvider");

            Expression expression = Expression.Call(null, QueryableFirstOrDefault.MakeGenericMethod(typeof(TSource)), source.Expression);

            return asyncQueryProvider.ExecuteAsync<TSource>(expression);
        }

        public static Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            IAsyncQueryProvider asyncQueryProvider = source.Provider as IAsyncQueryProvider;

            if (asyncQueryProvider == null)
                throw new NotSupportedException("The query provider is not an IAsyncQueryProvider");

            Expression expression = Expression.Call(null, QueryableFirstOrDefaultPredicate.MakeGenericMethod(typeof(TSource)), source.Expression, predicate);

            return asyncQueryProvider.ExecuteAsync<TSource>(expression);
        }

        #endregion

        #region Single / SingleOrDefault

        public static Task<TSource> SingleAsync<TSource>(this IQueryable<TSource> source)
        {
            IAsyncQueryProvider asyncQueryProvider = source.Provider as IAsyncQueryProvider;

            if (asyncQueryProvider == null)
                throw new NotSupportedException("The query provider is not an IAsyncQueryProvider");

            Expression expression = Expression.Call(null, QueryableSingle.MakeGenericMethod(typeof(TSource)), source.Expression);

            return asyncQueryProvider.ExecuteAsync<TSource>(expression);
        }

        public static Task<TSource> SingleAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            IAsyncQueryProvider asyncQueryProvider = source.Provider as IAsyncQueryProvider;

            if (asyncQueryProvider == null)
                throw new NotSupportedException("The query provider is not an IAsyncQueryProvider");

            Expression expression = Expression.Call(null, QueryableSinglePredicate.MakeGenericMethod(typeof(TSource)), source.Expression, predicate);

            return asyncQueryProvider.ExecuteAsync<TSource>(expression);
        }

        public static Task<TSource> SingleOrDefaultAsync<TSource>(this IQueryable<TSource> source)
        {
            IAsyncQueryProvider asyncQueryProvider = source.Provider as IAsyncQueryProvider;

            if (asyncQueryProvider == null)
                throw new NotSupportedException("The query provider is not an IAsyncQueryProvider");

            Expression expression = Expression.Call(null, QueryableSingleOrDefault.MakeGenericMethod(typeof(TSource)), source.Expression);

            return asyncQueryProvider.ExecuteAsync<TSource>(expression);
        }

        public static Task<TSource> SingleOrDefaultAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {

            IAsyncQueryProvider asyncQueryProvider = source.Provider as IAsyncQueryProvider;

            if (asyncQueryProvider == null)
                throw new NotSupportedException("The query provider is not an IAsyncQueryProvider");

            Expression expression = Expression.Call(null, QueryableSingleOrDefaultPredicate.MakeGenericMethod(typeof(TSource)), source.Expression, predicate);

            return asyncQueryProvider.ExecuteAsync<TSource>(expression);
        }

        #endregion

        #region AsEnumerable / ToArray / ToDictionary / ToList

        public static async Task<IEnumerable<TSource>> AsEnumerableAsync<TSource>(this IQueryable<TSource> source)
        {
            IAsyncQueryProvider asyncQueryProvider = source.Provider as IAsyncQueryProvider;

            if (asyncQueryProvider == null)
                throw new NotSupportedException("The query provider is not an IAsyncQueryProvider");

            return await asyncQueryProvider.ExecuteAsync<IEnumerable<TSource>>(source.Expression);
        }

        public static async Task<TSource[]> ToArrayAsync<TSource>(this IQueryable<TSource> source)
        {
            return (await source.ToListAsync()).ToArray();
        }

        public static async Task<List<TSource>> ToListAsync<TSource>(this IQueryable<TSource> source)
        {
            return new List<TSource>(await source.AsEnumerableAsync());
        }

        public static async Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(this IQueryable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return (await source.AsEnumerableAsync()).ToDictionary(keySelector);
        }

        public static async Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(this IQueryable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            return (await source.AsEnumerableAsync()).ToDictionary(keySelector, comparer);
        }

        public static async Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(this IQueryable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            return (await source.AsEnumerableAsync()).ToDictionary(keySelector, elementSelector);
        }

        public static async Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(this IQueryable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        {
            return (await source.AsEnumerableAsync()).ToDictionary(keySelector, elementSelector, comparer);
        }
   
        #endregion
    }
}
