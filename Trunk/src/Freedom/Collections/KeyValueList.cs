using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Freedom.Collections
{
    [CollectionDataContract(Namespace = Namespace, ItemName = "Item")]
    public class KeyValueList<TKey, TValue> : List<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>
    {
        public const string Namespace = "http://schemas.Freedomsoftware.com";

        #region Implementation of IDictionary<TKey,TValue>

        public bool ContainsKey(TKey key)
        {
            foreach (KeyValuePair<TKey, TValue> keyValuePair in this)
                if (Equals(keyValuePair.Key, key))
                    return true;

            return false;
        }

        public void Add(TKey key, TValue value)
        {
            Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        public bool Remove(TKey key)
        {
            return RemoveAll(kvp => Equals(kvp.Key, key)) > 0;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            foreach (KeyValuePair<TKey, TValue> keyValuePair in this)
            {
                if (!Equals(keyValuePair.Key, key)) continue;
                value = keyValuePair.Value;
                return true;
            }

            value = default(TValue);
            return false;
        }

        public TValue this[TKey key]
        {
            get
            {
                foreach (KeyValuePair<TKey, TValue> keyValuePair in this)
                    if (Equals(keyValuePair.Key, key))
                        return keyValuePair.Value;

                throw new KeyNotFoundException();
            }
            set
            {
                for (int i = 0; i < Count; i++)
                {
                    if (!Equals(this[i].Key, key)) continue;
                    this[i] = new KeyValuePair<TKey, TValue>(key, value);
                }

                Add(new KeyValuePair<TKey, TValue>(key, value));
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                List<TKey> result = new List<TKey>(Count);

                foreach (KeyValuePair<TKey, TValue> keyValuePair in this)
                    result.Add(keyValuePair.Key);

                return new ReadOnlyCollection<TKey>(result);
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                List<TValue> result = new List<TValue>(Count);

                foreach (KeyValuePair<TKey, TValue> keyValuePair in this)
                    result.Add(keyValuePair.Value);

                return new ReadOnlyCollection<TValue>(result);
            }
        }

        #endregion
    }
}
