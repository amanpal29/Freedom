using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Freedom.Extensions
{
    public static class EnumerableExtensions
    {
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable)
        {
            return new HashSet<T>(enumerable);
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> enumerable)
        {
            return new ObservableCollection<T>(enumerable);
        }

        public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> enumerable)
        {
            return new ReadOnlyCollection<T>(enumerable as IList<T> ?? new List<T>(enumerable));
        }

        public static IEnumerable<T> Yield<T>(this T item)
        {
            if (item == null) yield break;

            yield return item;
        }
    }
}
