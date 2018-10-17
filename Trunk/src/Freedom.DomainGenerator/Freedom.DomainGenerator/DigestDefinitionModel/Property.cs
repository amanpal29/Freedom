using System;
using System.Xml.Serialization;
using Freedom.DomainGenerator.CommonDefinitionModel;

namespace Freedom.DomainGenerator.DigestDefinitionModel
{
    [Serializable]
    public class Property : NamedItem
    {
        [XmlAttribute]
        public string Type { get; set; }

        [XmlAttribute]
        public string Select { get; set; }

        [XmlAttribute]
        public bool AutoProperty
        {
            get { return _autoProperty; }
            set { _autoProperty = value; }
        }

        private bool _autoProperty = true;

        [XmlAttribute]
        public string DisplayName
        {
            get { return _displayName ?? Name.ToDisplayName(); }
            set { _displayName = value; }
        }

        private string _displayName;

        [XmlAttribute]
        public string OrderBy
        {
            get { return _orderBy; }
            set { _orderBy = value; }
        }

        private string _orderBy;

        [XmlAttribute]
        public bool Nullable
        {
            get { return _nullable; }
            set { _nullable = value; }
        }

        private bool _nullable = true;

        [XmlAttribute]
        public bool Browsable
        {
            get { return _browsable ?? Type != "Guid"; }
            set { _browsable = value; }
        }

        private bool? _browsable;

        [XmlAttribute]
        public bool Groupable
        {
            get { return _groupable; }
            set { _groupable = value; }
        }

        private bool _groupable = true;

        [XmlAttribute]
        public string FilterStyle
        {
            get { return _filterStyle; }
            set { _filterStyle = value; }
        }

        private string _filterStyle;

        [XmlAttribute]
        public int DefaultColumnIndex
        {
            get { return _defaultColumnIndex; }
            set { _defaultColumnIndex = value; }
        }

        private int _defaultColumnIndex = -1;

        [XmlAttribute]
        public double SearchWeight { get; set; }

        [XmlAttribute]
        public string IndexHints { get; set; }

        public bool IsEnumType => !string.IsNullOrEmpty(Type) && Type.StartsWith("enum(") && Type.EndsWith(")");

        public string EnumType => IsEnumType ? Type.Substring(5, Type.Length - 6) : null;

        public string FieldName => Name.ToFieldName();

        public string FieldType
        {
            get
            {
                if (IsEnumType)
                    return Nullable ? EnumType + "?" : EnumType;

                switch (Type)
                {
                    case "byte[]":
                    case "string":
                        return Type;

                    case "DateTimeStamp":
                        return "DateTime";

                    default:
                        return Nullable ? Type + "?" : Type;
                }
            }
        }

        public string PropertyType
        {
            get
            {
                if (IsEnumType)
                    return Nullable ? EnumType + "?" : EnumType;

                switch (Type)
                {
                    case "byte[]":
                    case "string":
                        return Type;

                    case "DateTimeStamp":
                        return "DateTimeOffset";

                    default:
                        return Nullable ? Type + "?" : Type;
                }
            }
        }
    }
}