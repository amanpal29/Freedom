using System;
using System.Xml.Serialization;
using Freedom.DomainGenerator.CommonDefinitionModel;

namespace Freedom.DomainGenerator.DomainDefinitionModel
{
    [Serializable]
    public class EnumType : NamedItem
    {
        public EnumType()
        {
            Members = new NamedItemCollection<EnumMember>();
        }

        [XmlAttribute]
        public bool IsFlags { get; set; }

        [XmlElement("Member")]
        public NamedItemCollection<EnumMember> Members { get; private set; }
    }
}