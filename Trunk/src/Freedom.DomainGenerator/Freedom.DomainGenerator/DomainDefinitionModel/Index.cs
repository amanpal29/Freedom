using System;
using System.Linq;
using System.Xml.Serialization;
using Freedom.DomainGenerator.CommonDefinitionModel;

namespace Freedom.DomainGenerator.DomainDefinitionModel
{
    [Serializable]
    public class Index : NamedItem
    {
        public Index()
        {
            Properties = new NamedItemCollection<PropertyRef>();
            Includes = new NamedItemCollection<PropertyRef>();
        }

        [XmlAttribute]
        public bool IsUnique { get; set; }

        [XmlElement("PropertyRef")]
        public NamedItemCollection<PropertyRef> Properties { get; private set; }

        [XmlElement("IncludeRef")]
        public NamedItemCollection<PropertyRef> Includes { get; private set; }

        [XmlIgnore]
        public string DatabaseColumns
        {
            get { return string.Join(", ", Properties.Select(p => p.Descending ? $"[{p.Name}] desc" : $"[{p.Name}]")); }
        }

        [XmlIgnore]
        public string IncludeColumns
        {
            get { return string.Join(", ", Includes.Select(p => $"[{p.Name}]")); }
        }
    }
}