using System;
using System.Xml.Serialization;
using Freedom.DomainGenerator.CommonDefinitionModel;

namespace Freedom.DomainGenerator.DomainDefinitionModel
{
    [Serializable]
    public class ComplexType : NamedItem
    {
        public ComplexType()
        {
            Properties = new NamedItemCollection<Property>();
        }

        [XmlElement("Property")]
        public NamedItemCollection<Property> Properties { get; private set; }
    }
}