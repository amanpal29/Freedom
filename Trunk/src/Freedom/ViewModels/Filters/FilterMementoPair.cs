using System.Xml.Serialization;

namespace Freedom.ViewModels.Filters
{
    [XmlRoot("Filter")]
    public class FilterMementoPair
    {
        public FilterMementoPair()
        {
        }

        public FilterMementoPair(string key, string value)
        {
            Key = key;
            Value = value;
        }

        [XmlAttribute]
        public string Key { get; set; }

        [XmlAttribute]
        public string Value { get; set; }
    }
}