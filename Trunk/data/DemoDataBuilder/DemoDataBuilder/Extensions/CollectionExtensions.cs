using System;
using System.Collections.Generic;
using System.Linq;

namespace DemoDataBuilder.Extensions
{
    public static class CollectionExtensions
    {
        private static readonly Random Rng = new Random();

        public static T Random<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
                throw new ArgumentNullException("enumerable");

            ICollection<T> collection = (enumerable as ICollection<T>) ?? new List<T>(enumerable);

            if (collection.Count == 0)
                throw new InvalidOperationException("The collection is empty.");

            int index = Rng.Next(0, collection.Count);

            IList<T> list = collection as IList<T>;

            return list != null ? list[index] : collection.Skip(index - 1).First();
        }

        public static T RandomOrDefault<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
                throw new ArgumentNullException("enumerable");

            ICollection<T> collection = (enumerable as ICollection<T>) ?? new List<T>(enumerable);

            if (collection.Count == 0)
                return default(T);

            int index = Rng.Next(0, collection.Count);

            IList<T> list = collection as IList<T>;

            return list != null ? list[index] : collection.Skip(index - 1).First();
        }

    }
}
