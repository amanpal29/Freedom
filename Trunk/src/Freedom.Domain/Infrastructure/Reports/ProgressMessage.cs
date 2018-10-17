using System;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Freedom.Domain.Infrastructure.Reports
{
    [XmlRoot(Namespace = Namespace, IsNullable = false)]
    public class ProgressMessage
    {
        #region Xml Serialization

        public const string Namespace = "";

        public static readonly XmlSerializer Serializer = new XmlSerializer(typeof(ProgressMessage));

        public static readonly XmlWriterSettings XmlWriterSettings = new XmlWriterSettings
        {
            OmitXmlDeclaration = true,
            NewLineHandling = NewLineHandling.Entitize
        };

        [XmlNamespaceDeclarations] public static readonly XmlSerializerNamespaces Namespaces =
            new XmlSerializerNamespaces(new[] {new XmlQualifiedName(string.Empty, Namespace)});

        #endregion

        #region Constructors

        public ProgressMessage()
        {
        }

        public ProgressMessage(ReportEngineTask currentTask)
        {
            CurrentTask = currentTask;
        }

        public ProgressMessage(ReportEngineTask currentTask, double value, double maximum)
        {
            CurrentTask = currentTask;
            Value = value;
            Maximum = maximum;
        }

        public static ProgressMessage Create(XmlReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            return (ProgressMessage) Serializer.Deserialize(reader);
        }

        #endregion

        [XmlAttribute]
        public ReportEngineTask CurrentTask { get; set; }

        [XmlAttribute]
        public double Value { get; set; }

        [XmlAttribute]
        public double Maximum { get; set; }

        public double GetProgress(double minimum, double maximum)
        {
            if (minimum >= maximum)
                throw new ArgumentException("minimum must be less than maximum", nameof(minimum));

            if (double.IsNaN(Value) || double.IsNaN(Maximum))
                return double.NaN;

            if (Value > Maximum)
                return maximum;

            if (Maximum <= 0)
                return minimum;

            double range = maximum - minimum;

            return minimum + Value/Maximum*range;
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
            return Value >= 0d && Maximum > 0d
                ? $"{CurrentTask}: {Value:f1}/{Maximum:f1} complete"
                : $"{CurrentTask}";
        }
    }
}
