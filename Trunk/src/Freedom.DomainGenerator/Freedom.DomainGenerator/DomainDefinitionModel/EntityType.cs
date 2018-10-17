using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Freedom.DomainGenerator.CommonDefinitionModel;

namespace Freedom.DomainGenerator.DomainDefinitionModel
{
    [Serializable]
    public class EntityType : NamedItem
    {
        public EntityType()
        {
            Audit = true;
            Properties = new NamedItemCollection<Property>();
            Relationships = new NamedItemCollection<Relationship>();
            ComputedColumns = new NamedItemCollection<ComputedColumn>();
            Indexes = new NamedItemCollection<Index>();
            DerivedEntityTypes = new NamedItemCollection<EntityType>();
        }

        [XmlAttribute]
        public string BaseType { get; set; }

        [XmlAttribute]
        public bool Abstract { get; set; }

        [XmlAttribute]
        public bool Audit { get; set; }

        [XmlAttribute]
        public string Reportable
        {
            get { return _reportable.HasValue ? _reportable.ToString() : null; }
            set { _reportable = !string.IsNullOrEmpty(value) && bool.Parse(value); }
        }

        private bool? _reportable;

        [XmlElement("Property")]
        public NamedItemCollection<Property> Properties  { get; private set; }

        [XmlElement("Relationship")]
        public NamedItemCollection<Relationship> Relationships { get; private set; }

        [XmlElement("ComputedColumn")]
        public NamedItemCollection<ComputedColumn> ComputedColumns { get; private set; }

        [XmlElement("Index")]
        public NamedItemCollection<Index> Indexes { get; private set; }

        [XmlIgnore]
        public EntityType BaseEntityType { get; internal set; }

        [XmlIgnore]
        public bool IsManyToManyIntermediate { get; internal set; }

        [XmlIgnore]
        public NamedItemCollection<EntityType> DerivedEntityTypes { get; }

        public string GetBaseTypeAttribute(string alias = "Self")
        {
            if (string.IsNullOrEmpty(BaseType))
                return string.Empty;

            return $" BaseType=\"{alias}.{BaseType}\"";
        }

        public string GetAbstractAttribute()
        {
            return Abstract ? " Abstract=\"true\"" : string.Empty;
        }

        public bool IsType(string entityTypeName)
        {
            for (EntityType entityType = this; entityType != null; entityType = entityType.BaseEntityType )
                if (entityType.Name == entityTypeName)
                    return true;

            return false;
        }

        public bool IsAggregateRoot
        {
            get { return Relationships.All(r => r.RelationshipType != RelationshipType.Parent); }
        }

        public EntityType GetParentEntityType()
        {
            try
            {
                Relationship parent = Relationships.SingleOrDefault(r => r.RelationshipType == RelationshipType.Parent);

                return parent?.RelatedEntityType;
            }
            catch (InvalidOperationException e)
            {
                throw new InvalidOperationException($"There is more than one parent for entity '{Name}'.", e);
            }
        }

        public EntityType GetAggregateRoot()
        {
            HashSet<EntityType> ancestors = new HashSet<EntityType>();

            for (EntityType current = this;;)
            {
                if (!ancestors.Add(current))
                    throw new InvalidOperationException(
                        $"Circular reference detected in ancestors of entity '{Name}'");

                EntityType parent = current.GetParentEntityType();

                if (parent == null || parent == current)
                    return current;

                current = parent;
            }
        }

        public IOrderedEnumerable<string> GetKnownTypes()
        {
            IEnumerable<string> propertyTypes = Properties
                .Where(p => p.IsEnumType || p.IsComplexType)
                .Select(p => p.Type);

            IEnumerable<string> derivedTypes = DerivedEntityTypes
                .Select(p => p.Name);

            return propertyTypes.Union(derivedTypes).Distinct().OrderBy(x => x);
        }

        public IList<EntityType> GetEntityTypeHierarchy()
        {
            List<EntityType> result = new List<EntityType>();

            for (EntityType subType = this; subType != null; subType = subType.BaseEntityType)
                result.Insert(0, subType);

            return result;
        }

        public IEnumerable<Property> GetFlattenedProperties()
        {
            foreach (EntityType entityType in GetEntityTypeHierarchy())
            {
                foreach (Property property in entityType.Properties)
                {
                    if (!property.IsComplexType)
                    {
                        yield return property;
                    }
                    else
                    {
                        foreach (Property subProperty in property.ComplexType.Properties)
                        {
                            yield return new Property
                            {
                                Name = $"{property.Name}_{subProperty.Name}",
                                Type = subProperty.Type,
                                EnumType = subProperty.EnumType,
                                Nullable = subProperty.Nullable
                            };
                        }
                    }
                }
            }
        }

        public IEnumerable<string> GetChildPaths()
        {
            foreach (Relationship relationship in Relationships.Where(r => r.RelationshipType == RelationshipType.Children))
            {
                bool any = false;

                foreach (string path in relationship.RelatedEntityType.GetChildPaths())
                {
                    any = true;

                    yield return relationship.Name + '.' + path;
                }

                if (!any)
                {
                    yield return relationship.Name;
                }
            }
        }
    }
}