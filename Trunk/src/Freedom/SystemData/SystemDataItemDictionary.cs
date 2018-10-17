using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Freedom.SystemData
{
    // This is implemented as a List<KeyValuePair<string, string>> instead of a Dictionary<string, string>
    // because the order of items needs to be preservered.  - DGG 2016-01-27
    [CollectionDataContract(Namespace = SystemDataConstants.Namespace, ItemName = "Item")]
    public class SystemDataItemDictionary : IDictionary<string, string>, IList<KeyValuePair<string, string>>
    {
        private readonly List<KeyValuePair<string, string>> _data = new List<KeyValuePair<string, string>>();

        private static bool KeyEquals(string left, string right)
            => string.Equals(left, right, StringComparison.OrdinalIgnoreCase);

        public void AddRange(IEnumerable<KeyValuePair<string, string>> items)
        {
            foreach (KeyValuePair<string, string> item in items)
            {
                _data.Add(item);
            }
        }

        public int IndexOf(string key)
        {
            for (int i = 0; i < Count; i++)
            {
                if (KeyEquals(_data[i].Key, key))
                    return i;
            }

            return -1;
        }

        public void Add(string key, string value, string defaultValue)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (!string.IsNullOrEmpty(value))
            {
                _data.Add(new KeyValuePair<string, string>(key, value));
            }
            else if (defaultValue != null)
            {
                _data.Add(new KeyValuePair<string, string>(key, defaultValue));
            }
        }

        public void TryAdd(string key, Func<string> value, string defaultValue = null)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            try
            {
                Add(key, value(), defaultValue);
            }
            catch (Exception ex)
            {
                Add(key, $"A {ex.GetType().Name} occured while trying to read {key}.");
            }
        }

        #region Implementation of IEnumerable

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        #endregion

        #region Implementation of ICollection<KeyValuePair<string,string>>

        public void Add(KeyValuePair<string, string> item)
        {
            _data.Add(item);
        }

        public void Clear()
        {
            _data.Clear();
        }

        public bool Contains(KeyValuePair<string, string> item)
        {
            return _data.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            _data.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, string> item)
        {
            return _data.Remove(item);
        }

        public int Count => _data.Count;

        public bool IsReadOnly => false;

        #endregion

        #region Implementation of IList<KeyValuePair<string,string>>

        public int IndexOf(KeyValuePair<string, string> item)
        {
            return _data.IndexOf(item);
        }

        public void Insert(int index, KeyValuePair<string, string> item)
        {
            _data.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _data.RemoveAt(index);
        }

        KeyValuePair<string, string> IList<KeyValuePair<string, string>>.this[int index]
        {
            get { return _data[index]; }
            set { _data[index] = value; }
        }

        #endregion

        #region Implementation of IDictionary<string,string>

        public bool ContainsKey(string key)
        {
            foreach (var i in _data)
            {
                if (KeyEquals(i.Key, key))
                    return true;
            }

            return false;
        }

        public void Add(string key, string value)
        {
            _data.Add(new KeyValuePair<string, string>(key, value));
        }

        public bool Remove(string key)
        {
            return _data.RemoveAll(i => KeyEquals(i.Key, key)) > 0;
        }

        public bool TryGetValue(string key, out string value)
        {
            foreach (KeyValuePair<string, string> item in _data)
            {
                if (item.Key != key) continue;

                value = item.Value;

                return true;
            }

            value = null;

            return false;
        }

        public string this[string key]
        {
            get
            {
                foreach (var item in _data)
                {
                    if (item.Key != key) continue;

                    return item.Value;
                }

                return null;
            }
            set
            {
                for (int i = 0; i < _data.Count; i++)
                {
                    if (_data[i].Key != key) continue;

                    _data[i] = new KeyValuePair<string, string>(key, value);

                    return;
                }

                _data.Add(new KeyValuePair<string, string>(key, value));
            }
        }

        public ICollection<string> Keys => _data.Select(i => i.Key).ToList();

        public ICollection<string> Values => _data.Select(i => i.Value).ToList();

        #endregion
    }
}