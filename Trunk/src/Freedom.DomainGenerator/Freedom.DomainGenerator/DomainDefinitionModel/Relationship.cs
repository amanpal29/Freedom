using System;
using System.Xml.Serialization;
using Freedom.DomainGenerator.CommonDefinitionModel;

namespace Freedom.DomainGenerator.DomainDefinitionModel
{
    [Serializable]
    public class Relationship : NamedItem
    {
        [XmlAttribute]
        public string RelatedType { get; set; }

        [XmlAttribute]
        public RelationshipType RelationshipType { get; set; }

        [XmlAttribute]
        public string Purpose { get; set; }

        [XmlAttribute]
        public string Intermediate { get; set; }

        [XmlIgnore]
        public EntityType RelatedEntityType { get; internal set; }

        [XmlIgnore]
        public bool IsRequired => RelationshipType == RelationshipType.Required ||
                                  RelationshipType == RelationshipType.Parent;

        [XmlIgnore]
        public bool IsCollection => RelationshipType == RelationshipType.Collection ||
                                    RelationshipType == RelationshipType.Children ||
                                    RelationshipType == RelationshipType.ManyToMany;

        [XmlIgnore]
        public string FieldName => Name.ToFieldName();
    }
}