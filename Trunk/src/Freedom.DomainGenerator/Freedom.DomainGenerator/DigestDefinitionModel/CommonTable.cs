using System;
using System.Xml.Serialization;
using Freedom.DomainGenerator.CommonDefinitionModel;

namespace Freedom.DomainGenerator.DigestDefinitionModel
{
    [Serializable]
    public class CommonTable : NamedItem
    {
        [XmlText]
        public string Definition { get; set; }
    }
}
