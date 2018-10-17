// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.Utilities;
    using System.Globalization;
    using System.Reflection;

    /// <summary>
    /// Represents information about a column.
    ///
    /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
    /// (such as the end user of an application). If input is accepted from such sources it should be validated 
    /// before being passed to these APIs to protect against SQL injection attacks etc.
    /// </summary>
    public class ColumnModel : PropertyModel
    {
        private readonly Type _clrType;
        private readonly object _clrDefaultValue;
        private PropertyInfo _apiPropertyInfo;

        /// <summary>
        /// Initializes a new instance of the ColumnModel class.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="type"> The data type for this column. </param>
        public ColumnModel(PrimitiveTypeKind type)
            : this(type, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ColumnModel class.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="type"> The data type for this column. </param>
        /// <param name="typeUsage"> Additional details about the data type. This includes details such as maximum length, nullability etc. </param>
        public ColumnModel(PrimitiveTypeKind type, TypeUsage typeUsage)
            : base(type, typeUsage)
        {
            _clrType = PrimitiveType.GetEdmPrimitiveType(type).ClrEquivalentType;
            _clrDefaultValue = CreateDefaultValue();
        }

        private object CreateDefaultValue()
        {
            if (_clrType.IsValueType)
            {
                return Activator.CreateInstance(_clrType);
            }

            if (_clrType == typeof(string))
            {
                return string.Empty;
            }

            if (_clrType == typeof(DbGeography))
            {
                return DbGeography.FromText("POINT(0 0)");
            }

            if (_clrType == typeof(DbGeometry))
            {
                return DbGeometry.FromText("POINT(0 0)");
            }
            return new byte[0];
        }

        /// <summary>
        /// Gets the CLR type corresponding to the database type of this column.
        /// </summary>
        public virtual Type ClrType
        {
            get { return _clrType; }
        }

        /// <summary>
        /// Gets the default value for the CLR type corresponding to the database type of this column.
        /// </summary>
        public virtual object ClrDefaultValue
        {
            get { return _clrDefaultValue; }
        }

        /// <summary>
        /// Gets or sets a value indicating if this column can store null values.
        /// </summary>
        public virtual bool? IsNullable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if values for this column will be generated by the database using the identity pattern.
        /// </summary>
        public virtual bool IsIdentity { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if this property model should be configured as a timestamp.
        /// </summary>
        public virtual bool IsTimestamp { get; set; }

        internal PropertyInfo ApiPropertyInfo
        {
            get { return _apiPropertyInfo; }
            set
            {
                DebugCheck.NotNull(value);

                _apiPropertyInfo = value;
            }
        }

        private static readonly Dictionary<PrimitiveTypeKind, int> _typeSize // in bytes
            = new Dictionary<PrimitiveTypeKind, int>
                  {
                      { PrimitiveTypeKind.Binary, int.MaxValue },
                      { PrimitiveTypeKind.Boolean, 1 },
                      { PrimitiveTypeKind.Byte, 1 },
                      { PrimitiveTypeKind.DateTime, 8 },
                      { PrimitiveTypeKind.DateTimeOffset, 10 },
                      { PrimitiveTypeKind.Decimal, 17 },
                      { PrimitiveTypeKind.Double, 53 },
                      { PrimitiveTypeKind.Guid, 16 },
                      { PrimitiveTypeKind.Int16, 2 },
                      { PrimitiveTypeKind.Int32, 4 },
                      { PrimitiveTypeKind.Int64, 8 },
                      { PrimitiveTypeKind.SByte, 1 },
                      { PrimitiveTypeKind.Single, 4 },
                      { PrimitiveTypeKind.String, int.MaxValue },
                      { PrimitiveTypeKind.Time, 5 },
                      { PrimitiveTypeKind.Geometry, int.MaxValue },
                      { PrimitiveTypeKind.Geography, int.MaxValue },
                      { PrimitiveTypeKind.GeometryPoint, int.MaxValue },
                      { PrimitiveTypeKind.GeometryLineString, int.MaxValue },
                      { PrimitiveTypeKind.GeometryPolygon, int.MaxValue },
                      { PrimitiveTypeKind.GeometryMultiPoint, int.MaxValue },
                      { PrimitiveTypeKind.GeometryMultiLineString, int.MaxValue },
                      { PrimitiveTypeKind.GeometryMultiPolygon, int.MaxValue },
                      { PrimitiveTypeKind.GeometryCollection, int.MaxValue },
                      { PrimitiveTypeKind.GeographyPoint, int.MaxValue },
                      { PrimitiveTypeKind.GeographyLineString, int.MaxValue },
                      { PrimitiveTypeKind.GeographyPolygon, int.MaxValue },
                      { PrimitiveTypeKind.GeographyMultiPoint, int.MaxValue },
                      { PrimitiveTypeKind.GeographyMultiLineString, int.MaxValue },
                      { PrimitiveTypeKind.GeographyMultiPolygon, int.MaxValue },
                      { PrimitiveTypeKind.GeographyCollection, int.MaxValue },
                  };

        /// <summary>
        /// Determines if this column is a narrower data type than another column.
        /// Used to determine if altering the supplied column definition to this definition will result in data loss.
        /// </summary>
        /// <param name="column"> The column to compare to. </param>
        /// <param name="providerManifest"> Details of the database provider being used. </param>
        /// <returns> True if this column is of a narrower data type. </returns>
        public bool IsNarrowerThan(ColumnModel column, DbProviderManifest providerManifest)
        {
            Check.NotNull(column, "column");
            Check.NotNull(providerManifest, "providerManifest");

            var typeUsage = providerManifest.GetStoreType(TypeUsage);
            var otherTypeUsage = providerManifest.GetStoreType(column.TypeUsage);

            return (_typeSize[Type] < _typeSize[column.Type])
                   || !(IsUnicode ?? true) && (column.IsUnicode ?? true)
                   || !(IsNullable ?? true) && (column.IsNullable ?? true)
                   || IsNarrowerThan(typeUsage, otherTypeUsage);
        }

        private static bool IsNarrowerThan(TypeUsage typeUsage, TypeUsage other)
        {
            DebugCheck.NotNull(typeUsage);
            DebugCheck.NotNull(other);

            foreach (var facetName in
                new[]
                    {
                        DbProviderManifest.MaxLengthFacetName,
                        DbProviderManifest.PrecisionFacetName,
                        DbProviderManifest.ScaleFacetName
                    })
            {
                Facet facet, otherFacet;
                if (!typeUsage.Facets.TryGetValue(facetName, true, out facet)
                    || !other.Facets.TryGetValue(facet.Name, true, out otherFacet)
                    || (facet.Value == otherFacet.Value))
                {
                    continue;
                }

                var valueAsInt = Convert.ToInt32(facet.Value, CultureInfo.InvariantCulture);
                var otherValueAsInt = Convert.ToInt32(otherFacet.Value, CultureInfo.InvariantCulture);

                if (valueAsInt < otherValueAsInt)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
