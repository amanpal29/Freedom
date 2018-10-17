using System;
using System.Xml.Serialization;
using Freedom.DomainGenerator.CommonDefinitionModel;

namespace Freedom.DomainGenerator.DigestDefinitionModel
{
    [Serializable]
    [XmlRoot(Namespace = "http://schemas.hedgerowsoftware.com/digestdefinition")]
    public class Domain
    {
        public Domain()
        {
            Entities = new NamedItemCollection<Entity>();
        }

        [XmlElement("Entity")]
        public NamedItemCollection<Entity> Entities { get; private set; }
    }
}
