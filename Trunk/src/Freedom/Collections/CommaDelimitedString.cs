using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Freedom.Collections
{
    public class CommaDelimitedString : IList<string>
    {
        private string _delimitedString;

        public CommaDelimitedString(string commaDelimitedString)
        {
            _delimitedString = commaDelimitedString;
        }

        private static List<string> ToList(string commaDelimitedString)
        {
            if (string.IsNullOrEmpty(commaDelimitedString))
                return new List<string>();

            return commaDelimitedString
                .Split(",;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => x.Length > 0)
                .ToList();
        }

        private static string Join(IEnumerable<string> values)
        {
            if (values == null)
                return null;

            using (IEnumerator<String> enumerator = values.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                    return null;

                StringBuilder result = new StringBuilder();

                if (!string.IsNullOrWhiteSpace(enumerator.Current))
                    result.Append(enumerator.Current.Trim());

                while (enumerator.MoveNext())
                {
                    result.Append(',');

                    if (!string.IsNullOrWhiteSpace(enumerator.Current))
                        result.Append(enumerator.Current.Trim());
                }

                return result.ToString();
            }
        }

        public IEnumerator<string> GetEnumerator()
        {
            return ToList(_delimitedString).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ToList(_delimitedString).GetEnumerator();
        }

        public void Add(string item)
        {
            if (!string.IsNullOrWhiteSpace(item))
                _delimitedString = $"{_delimitedString},{item}";
        }

        public void Clear()
        {
            _delimitedString = string.Empty;
        }

        public bool Contains(string item)
        {
            return ToList(_delimitedString).Any(x => x == item);
        }

        public void CopyTo(string[] array, int arrayIndex)
        {
            ToList(_delimitedString).CopyTo(array, arrayIndex);
        }

        public bool Remove(string item)
        {
            List<string> list = ToList(_delimitedString);

            bool result = list.Remove(item);

            _delimitedString = Join(list);

            return result;
        }

        public int Count => ToList(_delimitedString).Count;

        public bool IsReadOnly => false;

        public int IndexOf(string item)
        {
            return ToList(_delimitedString).IndexOf(item);
        }

        public void Insert(int index, string item)
        {
            List<string> list = ToList(_delimitedString);

            list.Insert(index, item);

            _delimitedString = Join(list);
        }

        public void RemoveAt(int index)
        {
            List<string> list = ToList(_delimitedString);

            list.RemoveAt(index);

            _delimitedString = Join(list);
        }

        public string this[int index]
        {
            get { return ToList(_delimitedString)[index]; }
            set
            {
                List<string> list = ToList(_delimitedString);

                list[index] = value;

                _delimitedString = Join(list);
            }
        }

        public override string ToString()
        {
            return string.Join(",", ToList(_delimitedString));
        }
    }
}
