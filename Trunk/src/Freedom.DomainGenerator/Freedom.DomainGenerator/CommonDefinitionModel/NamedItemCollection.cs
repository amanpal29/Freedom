using System;
using System.Collections.Generic;
using System.Linq;

namespace Freedom.DomainGenerator.CommonDefinitionModel
{
    [Serializable]
    public class NamedItemCollection<T> : List<T>
        where T : NamedItem
    {
        public T this[string name]
        {
            get { return this.SingleOrDefault(x => x.Name == name); }
        }

        public ICollection<string> GetDuplicateItems()
        {
            HashSet<string> itemSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            HashSet<string> resultSet = new HashSet<string>();

            // ReSharper disable once LoopCanBePartlyConvertedToQuery

            // This loop is easier to understand when it's not obfuscated in a LINQ query. 

            foreach (T item in this)
                if (!itemSet.Add(item.Name))
                    resultSet.Add(item.Name);

            return resultSet;
        }
    }
}