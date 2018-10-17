using System;
using System.Xml.Serialization;
using Freedom.DomainGenerator.CommonDefinitionModel;

namespace Freedom.DomainGenerator.DigestDefinitionModel
{
    [Serializable]
    public class Entity : NamedItem
    {
        public Entity()
        {
            Properties = new NamedItemCollection<Property>();
            CommonTables = new NamedItemCollection<CommonTable>();
        }

        [XmlAttribute]
        public bool Reportable { get; set; }

        [XmlElement("Property")]
        public NamedItemCollection<Property> Properties { get; private set; }

        [XmlElement("CommonTable")]
        public NamedItemCollection<CommonTable> CommonTables { get; private set; }

        [XmlElement("Entity.From")]
        public string From { get; set; }
    }
}