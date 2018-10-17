// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Utilities;

    /// <summary>
    /// Convention to set a default maximum length of 4000 for properties whose type supports length facets when SqlCe is the provider.
    /// </summary>
    public class SqlCePropertyMaxLengthConvention : IConceptualModelConvention<EntityType>, IConceptualModelConvention<ComplexType>
    {
        private const int DefaultLength = 4000;

        /// <inheritdoc />
        public virtual void Apply(EntityType item, DbModel model)
        {
            Check.NotNull(item, "item");
            Check.NotNull(model, "model");

            var providerInfo = model.ProviderInfo;

            if ((providerInfo != null)
                && providerInfo.IsSqlCe())
            {
                SetLength(item.DeclaredProperties);
            }
        }

        /// <inheritdoc />
        public virtual void Apply(ComplexType item, DbModel model)
        {
            Check.NotNull(item, "item");
            Check.NotNull(model, "model");

            var providerInfo = model.ProviderInfo;

            if ((providerInfo != null)
                && providerInfo.IsSqlCe())
            {
                SetLength(item.Properties);
            }
        }

        private static void SetLength(IEnumerable<EdmProperty> properties)
        {
            foreach (var property in properties)
            {
                if (!property.IsPrimitiveType)
                {
                    continue;
                }

                if ((property.PrimitiveType == PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String))
                    || (property.PrimitiveType == PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Binary)))
                {
                    SetDefaults(property);
                }
            }
        }

        private static void SetDefaults(EdmProperty property)
        {
            DebugCheck.NotNull(property);

            if ((property.MaxLength == null)
                && (!property.IsMaxLength))
            {
                property.MaxLength = DefaultLength;
            }
        }
    }
}
