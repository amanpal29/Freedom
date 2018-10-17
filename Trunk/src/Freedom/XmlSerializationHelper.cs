using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Freedom
{
    public static class XmlSerializationHelper
    {
        public static string Serialize<T>(T value)
        {
            if (Equals(value, default(T)))
                return null;

            XmlSerializer serializer = new XmlSerializer(typeof(T));

            StringBuilder stringBuilder = new StringBuilder();

            using (XmlWriter xmlWriter = XmlWriter.Create(stringBuilder))
                serializer.Serialize(xmlWriter, value);

            return stringBuilder.ToString();
        }

        public static T Deserialize<T>(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                return default(T);

            XmlSerializer serializer = new XmlSerializer(typeof(T));

            using (XmlReader xmlReader = XmlReader.Create(new StringReader(xml)))
                return (T) serializer.Deserialize(xmlReader);
        } 
    }
}
