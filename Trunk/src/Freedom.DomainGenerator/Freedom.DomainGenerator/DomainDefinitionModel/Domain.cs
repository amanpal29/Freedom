using System;
using System.Xml.Serialization;
using Freedom.DomainGenerator.CommonDefinitionModel;

namespace Freedom.DomainGenerator.DomainDefinitionModel
{
    [Serializable]
    [XmlRoot(Namespace = "http://schemas.automatedstocktrader.com/domaindefinition")]
    public class Domain
    {
        private NamedItemCollection<EntityType> _entityTypes = new NamedItemCollection<EntityType>();
        private NamedItemCollection<ComplexType> _complexTypes = new NamedItemCollection<ComplexType>();
        private NamedItemCollection<EnumType> _enumTypes = new NamedItemCollection<EnumType>();
        private readonly NamedItemCollection<Association> _associations = new NamedItemCollection<Association>();

        [XmlAttribute]
        public string Namespace { get; set; }

        [XmlElement("EntityType")]
        public NamedItemCollection<EntityType> EntityTypes
        {
            get { return _entityTypes; }
            private set { _entityTypes = value; }
        }

        [XmlElement("ComplexType")]
        public NamedItemCollection<ComplexType> ComplexTypes
        {
            get { return _complexTypes; }
            private set { _complexTypes = value; }
        }

        [XmlElement("EnumType")]
        public NamedItemCollection<EnumType> EnumTypes
        {
            get { return _enumTypes; }
            private set { _enumTypes = value; }
        }

        [XmlIgnore]
        public NamedItemCollection<Association> Associations => _associations;

        public void Sort()
        {
            _enumTypes.Sort();

            _complexTypes.Sort();

            int index = _entityTypes.FindIndex(et => !et.Abstract);

            _entityTypes.Sort(index, _entityTypes.Count - index, null);

            _associations.Sort();
        }
    }
}