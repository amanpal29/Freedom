using System;
using System.Globalization;
using System.Xml.Serialization;
using Freedom.DomainGenerator.CommonDefinitionModel;

namespace Freedom.DomainGenerator.DomainDefinitionModel
{
    [Serializable]
    public class Property : NamedItem
    {
        public Property()
        {
            Nullable = true;
        }

        [XmlAttribute]
        public bool IsPrimaryKey { get; set; }

        [XmlAttribute]
        public string Type { get; set; }

        [XmlAttribute]
        public bool Nullable { get; set; }

        [XmlAttribute]
        public string DefaultValue { get; set; }

        [XmlAttribute]
        public PropertyFlags Flags { get; set; }

        [XmlIgnore]
        public string DatabaseColumnName
        {
            get { return _databaseColumnName ?? Name; }
            set { _databaseColumnName = value; }
        }
        private string _databaseColumnName;

        [XmlIgnore]
        public string FieldName
        {
            get
            {
                if (string.IsNullOrEmpty(Name))
                    return Name;

                if (Name.Length == 1)
                    return "_" + Name.ToLower(CultureInfo.CurrentCulture);

                return "_" + char.ToLower(Name[0], CultureInfo.CurrentCulture) + Name.Substring(1);
            }
        }

        [XmlIgnore]
        public bool IsEnumType => EnumType != null;

        [XmlIgnore]
        public EnumType EnumType { get; internal set; }

        [XmlIgnore]
        public bool IsComplexType => ComplexType != null;

        [XmlIgnore]
        public ComplexType ComplexType { get; internal set; }

        [XmlIgnore]
        public PrimitiveType PrimitiveType => IsEnumType ? PrimitiveTypes.Int32 : PrimitiveTypes.Get(Type);

        [XmlIgnore]
        public string ConceptualType => PrimitiveType.ConceptualType;

        [XmlIgnore]
        public string StorageType => PrimitiveType.StorageType;

        [XmlIgnore]
        public string SqlDataType => PrimitiveType.SqlDataType;

        [XmlIgnore]
        public string ClrType
        {
            get
            {
                if (IsEnumType || IsComplexType)
                    return Type;

                switch (Type)
                {
                    case "Binary":
                    case "Text":
                    case "String":
                        return PrimitiveType.ClrType;

                    default:
                        return Nullable ? PrimitiveType.ClrType + "?" : PrimitiveType.ClrType;
                }
            }
        }

        [XmlIgnore]
        public string XmlSchemaDataType => IsEnumType ? $"{Type}Enum" : PrimitiveType.XsdType;

        public string GetDatabaseDefault()
        {
            if (string.IsNullOrEmpty(DefaultValue))
                return string.Empty;

            switch (Type)
            {
                case "Boolean":
                    return " default " + (bool.Parse(DefaultValue) ? "1" : "0");

                case "Guid":
                    if (DefaultValue != "Guid.NewGuid()")
                    {
                        string message = $"Invalid Guid default value {DefaultValue} for property {Name}.";

                        throw new InvalidOperationException(message);
                    }
                    return " default newId()";

                case "String":
                case "Text":
                    return $" default '{DefaultValue}'";

                default:
                    return " default " + DefaultValue;
            }
        }

        public string GetClrFieldInitializer()
        {
            if (string.IsNullOrEmpty(DefaultValue))
                return string.Empty;

            switch (Type)
            {
                case "Boolean":
                    return bool.Parse(DefaultValue) ? " = true" : " = false";

                case "String":
                case "Text":
                    return $" = \"{DefaultValue.Replace("\"", "\\\"")}\"";

                default:
                    return $" = {DefaultValue}";
            }
        }
    }
}