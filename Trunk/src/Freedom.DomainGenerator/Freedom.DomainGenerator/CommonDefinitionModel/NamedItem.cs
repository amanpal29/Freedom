using System;
using System.Xml.Serialization;

namespace Freedom.DomainGenerator.CommonDefinitionModel
{
    [Serializable]
    public class NamedItem : IComparable, IComparable<NamedItem>
    {
        [XmlAttribute]
        public string Name { get; set; }

        public int CompareTo(object obj)
        {
            return CompareTo(obj as NamedItem);
        }

        public int CompareTo(NamedItem other)
        {
            if (other == null)
                return 1;

            return string.CompareOrdinal(Name, other.Name);
        }
    }
}
