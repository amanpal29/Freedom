using Freedom.DomainGenerator.CommonDefinitionModel;
using Freedom.DomainGenerator.DomainDefinitionModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace Freedom.DomainGenerator
{
    public static class DomainBuilder
    {
        public static Domain Load(string fileName)
        {
            using (XmlReader reader = XmlReader.Create(fileName))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Domain));

                Domain domain = (Domain)serializer.Deserialize(reader);

                CheckForDuplicates(domain);

                IdentifyEnumAndComplexTypes(domain);

                LinkBaseEntityTypes(domain);

                CreatePropertiesForRelationships(domain);

                CreateIntermediateEntityTypes(domain);

                LinkRelatedTypes(domain);

                BuildAssociations(domain);

                CheckParentChildrenRelationships(domain);

                domain.Sort();

                return domain;
            }
        }

        private static void CheckForDuplicates(Domain domain)
        {
            NamedItemCollection<NamedItem> items = new NamedItemCollection<NamedItem>();

            items.AddRange(domain.EnumTypes);
            items.AddRange(domain.ComplexTypes);
            items.AddRange(domain.EntityTypes);

            ICollection<string> duplicateItems = items.GetDuplicateItems();

            if (duplicateItems.Count > 0)
            {
                string message = $"The following Types were defined more than once: {string.Join(", ", duplicateItems)}";

                throw new InvalidOperationException(message);
            }
        }

        private static void IdentifyEnumAndComplexTypes(Domain domain)
        {
            foreach (ComplexType complexType in domain.ComplexTypes)
            {
                foreach (Property property in complexType.Properties)
                {
                    property.EnumType = domain.EnumTypes[property.Type];
                    property.ComplexType = domain.ComplexTypes[property.Type];
                }
            }

            foreach (EntityType entityType in domain.EntityTypes)
            {
                foreach (Property property in entityType.Properties)
                {
                    property.EnumType = domain.EnumTypes[property.Type];
                    property.ComplexType = domain.ComplexTypes[property.Type];
                }
            }
        }

        private static void LinkBaseEntityTypes(Domain domain)
        {
            foreach (EntityType entityType in domain.EntityTypes)
            {
                if (string.IsNullOrEmpty(entityType.BaseType)) continue;

                entityType.BaseEntityType = domain.EntityTypes[entityType.BaseType];

                if (entityType.BaseEntityType == null)
                {
                    string message =
                        $"The base type '{entityType.BaseType}' referenced by entity type '{entityType.Name} was not found in the domain.";

                    throw new InvalidOperationException(message);
                }

                entityType.BaseEntityType.DerivedEntityTypes.Add(entityType);
            }
        }

        private static void LinkRelatedTypes(Domain domain)
        {
            foreach (EntityType entityType in domain.EntityTypes)
            {
                foreach (Relationship relationship in entityType.Relationships)
                {
                    relationship.RelatedEntityType = domain.EntityTypes[relationship.RelatedType];

                    if (relationship.RelatedEntityType == null)
                    {
                        string message =
                            $"The related type '{relationship.RelatedType}' referenced in relationship '{relationship.Name}' on entity type '{entityType.Name}' was not found in the domain.";

                        throw new InvalidOperationException(message);
                    }
                }
            }
        }

        private static void CreateIntermediateEntityTypes(Domain domain)
        {
            foreach (EntityType entityType in domain.EntityTypes.ToList())
            {
                foreach (Relationship relationship in entityType.Relationships.Where(r => r.RelationshipType == RelationshipType.ManyToMany).ToList())
                {
                    if (string.IsNullOrEmpty(relationship.Intermediate))
                    {
                        string message =
                            $"The releationship {entityType.Name}:{relationship.Name} is a many-to-many relationship, but no name was specified for the intermediate table.";

                        throw new InvalidOperationException(message);
                    }

                    if (domain.EntityTypes[relationship.Intermediate] != null)
                    {
                        string message = string.Format(
                            "Unable to create intermediate entity type '{0}' for relationship '{1}' defined on entity '{2}'.  An entity type named '{0}' already exists.",
                            relationship.Intermediate, relationship.Name, entityType.Name);

                        throw new InvalidOperationException(message);
                    }

                    // Create the intermediate entity

                    EntityType intermediate = new EntityType();

                    intermediate.Name = relationship.Intermediate;

                    intermediate.IsManyToManyIntermediate = true;

                    intermediate.Relationships.Add(new Relationship
                    {
                        Name = entityType.Name,
                        RelatedType = entityType.Name,
                        RelationshipType = RelationshipType.Parent
                    });

                    intermediate.Relationships.Add(new Relationship
                    {
                        Name = relationship.RelatedType,
                        RelatedType = relationship.RelatedType,
                        RelationshipType = RelationshipType.Required
                    });

                    intermediate.Properties.Add(new Property
                    {
                        Name = entityType.Name + "Id",
                        Type = "Guid",
                        Nullable = false,
                        IsPrimaryKey = true
                    });

                    intermediate.Properties.Add(new Property
                    {
                        Name = relationship.RelatedType + "Id",
                        Type = "Guid",
                        Nullable = false,
                        IsPrimaryKey = true
                    });

                    domain.EntityTypes.Add(intermediate);

                    // Add a relationship to the intermediate entity

                    entityType.Relationships.Add(new Relationship
                    {
                        Name = relationship.Intermediate,
                        RelatedType = relationship.Intermediate,
                        RelationshipType = RelationshipType.Children
                    });
                }
            }
        }

        private static void CreatePropertiesForRelationships(Domain domain)
        {
            foreach (EntityType entityType in domain.EntityTypes)
            {
                foreach (Relationship relationship in entityType.Relationships)
                {
                    if (relationship.IsCollection) continue;

                    string propertyName = relationship.Name + "Id";

                    if (entityType.Properties[propertyName] != null)
                    {
                        string message =
                            $"Unable to add property named {propertyName} to entity {entityType.Name} for relationship {relationship.Name} because it already exists.";

                        throw new InvalidOperationException(message);
                    }

                    Property property = new Property();

                    property.Name = propertyName;

                    property.DatabaseColumnName = string.IsNullOrEmpty(relationship.Purpose)
                        ? relationship.RelatedType + "Id"
                        : relationship.Purpose + "Id";

                    property.Type = "Guid";

                    property.Nullable = !relationship.IsRequired;

                    entityType.Properties.Add(property);
                }
            }
        }

        private static void BuildAssociations(Domain domain)
        {
            foreach (EntityType entityType in domain.EntityTypes.Where(et => !et.Abstract))
            {
                for (EntityType subType = entityType; subType != null; subType = subType.BaseEntityType)
                {
                    foreach (Relationship relationship in subType.Relationships.Where(r => !r.IsCollection))
                    {
                        domain.Associations.Add(BuildAssociation(entityType, relationship));
                    }
                }
            }

            foreach (EntityType entityType in domain.EntityTypes.Where(et => !et.Abstract))
            {
                for (EntityType subType = entityType; subType != null; subType = subType.BaseEntityType)
                {
                    foreach (Relationship relationship in subType.Relationships.Where(r => r.IsCollection))
                    {
                        if (relationship.RelationshipType == RelationshipType.ManyToMany) continue;

                        string associationName = Association.GenerateName(entityType, relationship);

                        Association association = domain.Associations[associationName];

                        if (association == null)
                        {
                            string message =
                                $"There is no recipocal relationship in {relationship.RelatedType} for {relationship.Name} on {entityType.Name}";

                            throw new InvalidOperationException(message);
                        }

                        if (relationship.RelationshipType == RelationshipType.Collection &&
                            association.OnDelete == OnDeleteAction.Cascade)
                        {
                            string message =
                                $"Warning: The relationship {entityType.Name}:{relationship.Name} is a children relationship, but it's reciprocal in {relationship.RelatedType} is not a parent relationship";

                            throw new InvalidOperationException(message);
                        }

                        if (relationship.RelationshipType != RelationshipType.Collection &&
                            association.OnDelete == OnDeleteAction.None)
                        {
                            string message =
                                $"Warning: The relationship {entityType.Name}:{relationship.Name} is not a children relationship, but it's reciprocal in {relationship.RelatedType} is marked a parent relationship";

                            throw new InvalidOperationException(message);
                        }
                    }
                }
            }
        }

        private static Association BuildAssociation(EntityType entityType, Relationship relationship)
        {
            Association association = new Association();

            association.Name = Association.GenerateName(entityType, relationship);

            association.PrincipalType = relationship.RelatedType;

            association.DependentType = entityType.Name;

            association.Purpose = relationship.Purpose;

            association.PrincipalMultiplicity = relationship.IsRequired
                ? Multiplicity.One
                : Multiplicity.ZeroOrOne;

            association.DependentMultiplicity = Multiplicity.Many;

            association.ToPrincipalName = relationship.Name;

            association.OnDelete = relationship.RelationshipType == RelationshipType.Parent
                ? OnDeleteAction.Cascade
                : OnDeleteAction.None;

            return association;
        }

        private static void CheckParentChildrenRelationships(Domain domain)
        {
            foreach (EntityType entityType in domain.EntityTypes.Where(et => et.IsType("AggregateRoot")))
            {
                foreach (Relationship relationship in entityType.Relationships.Where(r => r.RelationshipType == RelationshipType.Parent))
                {
                    Debug.Print("Entity type {0} is an aggregate root, but has a relationship ({1}) to parent entity {2}",
                        entityType.Name, relationship.Name, relationship.RelatedType);

                }
            }
        }
    }
}
