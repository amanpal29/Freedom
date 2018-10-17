using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Freedom.Extensions
{
    public static class MetadataHelper
    {
        public static MetadataWorkspace GetMetadataWorkspace(this IObjectContextAdapter context)
        {
            return context.ObjectContext.MetadataWorkspace;
        }

        public static Type GetEntityTypeByName(this IObjectContextAdapter context, string entityTypeName)
        {
            if (string.IsNullOrEmpty(entityTypeName))
                throw new ArgumentNullException(nameof(entityTypeName));

            MetadataWorkspace metadata = context.ObjectContext.MetadataWorkspace;

            EntityType entityType = metadata.GetItems<EntityType>(DataSpace.CSpace)
                .FirstOrDefault(x => x.Name == entityTypeName);

            if (entityType == null)
                return null;

            return context.GetType().Assembly.GetType(entityType.FullName, false, false);
        }

        private static ReferentialConstraint GetReferentialConstraint(IObjectContextAdapter context,
            Type type, string navigationPropertyName)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (string.IsNullOrEmpty(navigationPropertyName))
                throw new ArgumentNullException(nameof(navigationPropertyName));

            MetadataWorkspace metadata = context.GetMetadataWorkspace();

            EntityType entityType = metadata.GetItem<EntityType>(type.FullName, DataSpace.CSpace);

            NavigationProperty navigationProperty = entityType.NavigationProperties[navigationPropertyName];

            AssociationType associationType = (AssociationType) navigationProperty.RelationshipType;

            if (associationType.ReferentialConstraints.Count == 0)
                return null;

            if (associationType.ReferentialConstraints.Count > 1)
                throw new InvalidOperationException("More that one ReferentialConstraint was found.");

            return associationType.ReferentialConstraints[0];
        }

        public static ForeignKeyConstraint GetForeignKeyConstraint(
            this IObjectContextAdapter context, Type type, string navigationPropertyName)
        {
            ReferentialConstraint constraint = GetReferentialConstraint(context, type, navigationPropertyName);

            if (constraint.FromProperties.Count != 1 || constraint.ToProperties.Count != 1)
                throw new InvalidOperationException("The ReferentialConstraint does not have exactly one field pair.");

            return new ForeignKeyConstraint(
                new PropertyReference(constraint.FromRole.GetEntityType().FullName, constraint.FromProperties[0].Name),
                new PropertyReference(constraint.ToRole.GetEntityType().FullName, constraint.ToProperties[0].Name));
        }

        public static IEnumerable<ForeignKeyConstraint> GetForeignKeyConstraints(
            this IObjectContextAdapter context, Type type, string navigationPropertyName)
        {
            ReferentialConstraint constraint = GetReferentialConstraint(context, type, navigationPropertyName);

            if (constraint.FromProperties.Count != constraint.ToProperties.Count)
                throw new InvalidOperationException(
                    "The ReferentialConstraint had mismatching number of property references.");

            for (int i = 0; i < constraint.FromProperties.Count; i++)
            {
                EdmProperty from = constraint.FromProperties[i];
                EdmProperty to = constraint.ToProperties[i];

                yield return new ForeignKeyConstraint(
                    new PropertyReference(from.TypeName, from.Name),
                    new PropertyReference(to.TypeName, to.Name));
            }
        }

    }
}
