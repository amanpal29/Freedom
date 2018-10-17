using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Freedom.Domain.Infrastructure.Reports
{
    [XmlRoot(Namespace = Namespace, IsNullable = false)]
    public class ExceptionMessage
    {
        #region XmlSerialization Constants

        public const string Namespace = "";

        public static readonly XmlSerializer Serializer = new XmlSerializer(typeof (ExceptionMessage));

        public static readonly XmlWriterSettings XmlWriterSettings = new XmlWriterSettings
        {
            OmitXmlDeclaration = true,
            NewLineHandling = NewLineHandling.Entitize
        };

        [XmlNamespaceDeclarations] public static readonly XmlSerializerNamespaces Namespaces =
            new XmlSerializerNamespaces(new[] {new XmlQualifiedName(string.Empty, Namespace)});

        #endregion

        #region Constructors

        public ExceptionMessage()
        {
        }

        public ExceptionMessage(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            FullName = exception.GetType().FullName;
            Message = exception.Message;
            StackTrace = exception.StackTrace;

            if (exception.InnerException != null)
                InnerException = new ExceptionMessage(exception.InnerException);
        }

        public static ExceptionMessage Create(string serialized)
        {
            if (string.IsNullOrEmpty(serialized))
                throw new ArgumentNullException(nameof(serialized));

            using (StringReader reader = new StringReader(serialized))
                return (ExceptionMessage) Serializer.Deserialize(reader);
        }

        public static ExceptionMessage Create(XmlReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            return (ExceptionMessage) Serializer.Deserialize(reader);
        }

        public static string ToXml(Exception ex)
        {
            return new ExceptionMessage(ex).ToXml();
        }
        
        #endregion

        [XmlAttribute]
        public string FullName { get; set; }

        [XmlAttribute]
        public string Message { get; set; }

        [XmlAttribute]
        public string StackTrace { get; set; }

        [XmlElement]
        public ExceptionMessage InnerException { get; set; }

        [XmlIgnore]
        public string DisplayName
            => !string.IsNullOrEmpty(Message) ? $"{FullName}: {Message}" : FullName;

        [XmlIgnore]
        public string FullStackTrace
        {
            get
            {
                StringBuilder result = new StringBuilder(StackTrace);

                for (ExceptionMessage current = InnerException; current != null; current = current.InnerException)
                {
                    result.Insert(0, Environment.NewLine);
                    result.Insert(0, "   --- End of inner exception stack trace ---");
                    result.Insert(0, Environment.NewLine);
                    result.Insert(0, current.StackTrace);
                }

                return result.ToString();
            }
        }

        public string ToXml()
        {
            StringBuilder stringBuilder = new StringBuilder();

            using (XmlWriter writer = XmlWriter.Create(stringBuilder, XmlWriterSettings))
                Serializer.Serialize(writer, this, Namespaces);

            return stringBuilder.ToString();
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();

            result.AppendLine(DisplayName);

            for (ExceptionMessage current = InnerException; current != null; current = current.InnerException)
            {
                result.Append(" ---> ");
                result.AppendLine(current.DisplayName);
            }

            result.Append(FullStackTrace);

            return result.ToString();
        }
    }
}
