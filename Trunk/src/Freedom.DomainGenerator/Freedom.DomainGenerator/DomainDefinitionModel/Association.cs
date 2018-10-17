using Freedom.DomainGenerator.CommonDefinitionModel;
using System;


namespace Freedom.DomainGenerator.DomainDefinitionModel
{
    public class Association : NamedItem
    {
        public static string GenerateName(EntityType entityType, Relationship relationship)
        {
            switch (relationship.RelationshipType)
            {
                case RelationshipType.Optional:
                case RelationshipType.Required:
                case RelationshipType.Parent:
                    return string.IsNullOrEmpty(relationship.Purpose)
                        ? $"FK_{entityType.Name}_{relationship.RelatedType}"
                        : $"FK_{entityType.Name}_{relationship.RelatedType}_{relationship.Purpose}";

                case RelationshipType.Collection:
                case RelationshipType.Children:
                    return string.IsNullOrEmpty(relationship.Purpose)
                        ? $"FK_{relationship.RelatedType}_{entityType.Name}"
                        : $"FK_{relationship.RelatedType}_{entityType.Name}_{relationship.Purpose}";

                default:
                    throw new InvalidOperationException(
                        $"Unable to generate an association name for relationship of type {relationship.RelationshipType}.");
            }
        }

        public string PrincipalType { get; internal set; }

        public string DependentType { get; internal set; }

        public string ToPrincipalName { get; internal set; }

        public string ToDependentName { get; internal set; }

        public string Purpose { get; internal set; }

        public Multiplicity PrincipalMultiplicity { get; internal set; }

        public Multiplicity DependentMultiplicity { get; internal set; }

        public OnDeleteAction OnDelete { get; internal set; }

        public string PrincipalColumnName => "Id";

        public string DependentColumnName => string.IsNullOrEmpty(Purpose) ? PrincipalType + "Id" : Purpose + "Id";

        public string PrincipalRole => IsSelfJoin ? PrincipalType + "1" : PrincipalType;

        public string DependentRole => DependentType;

        public bool IsOneToOne => PrincipalMultiplicity != Multiplicity.Many && DependentMultiplicity != Multiplicity.Many;

        public bool IsManyToMany => PrincipalMultiplicity == Multiplicity.Many && DependentMultiplicity == Multiplicity.Many;

        public bool IsSelfJoin => PrincipalType == DependentType;
    }
}