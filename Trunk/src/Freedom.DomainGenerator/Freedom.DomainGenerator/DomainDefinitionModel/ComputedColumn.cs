using System;
using System.Xml.Serialization;
using Freedom.DomainGenerator.CommonDefinitionModel;

namespace Freedom.DomainGenerator.DomainDefinitionModel
{
    [Serializable]
    public class ComputedColumn : NamedItem
    {
        [XmlText]
        public string Definition { get; set; }

        [XmlAttribute]
        public bool IsPersisted { get; set; }
    }
}
