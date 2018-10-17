using System.Configuration;

namespace Freedom.FullTextSearch.Configuration
{
    [ConfigurationCollection(typeof(SubstitutionElement))]
    public class SubstitutionElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new SubstitutionElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            SubstitutionElement substitutionElement = (SubstitutionElement) element;

            return substitutionElement.Find;
        }

        public void Add(SubstitutionElement substitutionElement)
        {
            BaseAdd(substitutionElement);
        }

        public void Clear()
        {
            BaseClear();
        }

        public void Remove(SubstitutionElement substitutionElement)
        {
            BaseRemove(substitutionElement.Find);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public void Remove(string find)
        {
            BaseRemove(find);
        }

        public SubstitutionElement this[int idx] => (SubstitutionElement) BaseGet(idx);
    }
}