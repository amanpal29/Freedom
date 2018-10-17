using System;
using System.Runtime.Serialization;

namespace Freedom.SystemData
{
    [DataContract(Namespace = SystemDataConstants.Namespace)]
    public class SystemDataSection
    {
        private SystemDataItemDictionary _items;

        public SystemDataSection(string name)
        {
            Name = name;
        }

        [DataMember(Order = 1)]
        public string Name { get; set; }

        [DataMember(Order = 2)]
        public SystemDataItemDictionary Items => _items ?? (_items = new SystemDataItemDictionary());

        public void Add(string key, string value)
        {
            Items.Add(key, value);
        }

        public void Add(string key, string value, string defaultValue)
        {
            Items.Add(key, value, defaultValue);
        }

        public void TryAdd(string key, Func<string> valueFunction, string defaultValue = null)
        {
            Items.TryAdd(key, valueFunction, defaultValue);
        }
    }
}
