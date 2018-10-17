using System.Collections.Generic;

namespace Freedom.FullTextSearch
{
    public class IndexKeySetCollection : Dictionary<string, IndexKeySet>
    {
        public bool Contains<T>()
        {
            return ContainsKey(typeof(T).FullName);
        }

        public IndexKeySet<T> Get<T>()
        {
            IndexKeySet result;

            if (!TryGetValue(typeof(T).FullName, out result))
                return null;

            return (IndexKeySet<T>) result;
        }

        public IndexKeySet<T> Ensure<T>()
        {
            IndexKeySet result;

            if (!TryGetValue(typeof(T).FullName, out result))
                Add(typeof(T).FullName, result = new IndexKeySet<T>());

            return (IndexKeySet<T>) result;
        }
    }
}
