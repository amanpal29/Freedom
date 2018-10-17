using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Freedom.Domain.Model;

namespace Freedom.Domain.Infrastructure
{
    [Serializable]
    public sealed class AutoNumberSettings
    {
        public AutoNumberSettings()
        {
            AllowManualOverride = true;
        }

        [XmlElement]
        public AutoNumberMode Mode { get; set; }

        [XmlElement]
        public string Format { get; set; }

        [XmlElement, DefaultValue(true)]
        public bool AllowManualOverride { get; set; }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();

            XmlSerializer serializer = new XmlSerializer(typeof(AutoNumberSettings));

            using (XmlWriter writer = XmlWriter.Create(result))
                serializer.Serialize(writer, this);

            return result.ToString();
        }

        public static AutoNumberSettings FromString(string s)
        {
            if (string.IsNullOrEmpty(s))
                throw new ArgumentNullException(nameof(s));

            XmlSerializer serializer = new XmlSerializer(typeof(AutoNumberSettings));

            using (TextReader reader = new StringReader(s))
                return (AutoNumberSettings)serializer.Deserialize(reader);
        }
    }
}
