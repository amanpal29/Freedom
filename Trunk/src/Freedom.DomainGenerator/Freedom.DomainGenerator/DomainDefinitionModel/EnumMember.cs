using System;
using System.Xml.Serialization;
using Freedom.DomainGenerator.CommonDefinitionModel;

namespace Freedom.DomainGenerator.DomainDefinitionModel
{
    [Serializable]
    public class EnumMember : NamedItem
    {
        private bool? _browsable;
        private int _value;

        [XmlAttribute]
        public string Value
        {
            get { return _value.ToString(); }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _value = 0;
                }
                else if (value.StartsWith("0x"))
                {
                    _value = int.Parse(value.Substring(2), System.Globalization.NumberStyles.HexNumber);
                }
                else
                {
                    _value = int.Parse(value);
                }
            }
        }

        [XmlIgnore]
        public string ValueAsHex
        {
            get
            {
                if (_value == -1)
                    return "-1";

                return "0x" + _value.ToString("X");
            }
        }

        [XmlAttribute]
        public bool Browsable
        {
            get { return _browsable ?? true; }
            set { _browsable = value; }
        }

        [XmlAttribute]
        public string Description { get; set; }
    }
}