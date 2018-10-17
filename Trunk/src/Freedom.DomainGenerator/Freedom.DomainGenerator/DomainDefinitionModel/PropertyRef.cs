using System;
using System.Xml.Serialization;
using Freedom.DomainGenerator.CommonDefinitionModel;

namespace Freedom.DomainGenerator.DomainDefinitionModel
{
    [Serializable]
    public class PropertyRef : NamedItem
    {
        [XmlAttribute]
        public bool Descending { get; set; }
    }
}