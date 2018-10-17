using System.Runtime.InteropServices;
using System.Xml.Serialization;

namespace Hedgehog.SampleImportModel
{
    [XmlType(AnonymousType= true, Namespace= "http://schemas.hedgerowsoftware.com/sampleimportdefinition/v1.0")]
    public class SampleParameterDetail
    {
        [XmlElement]
        public string ParameterCode { get; set; }

        [XmlElement]
        public string Parameter { get; set; }

        [XmlElement(ElementName = "UOMCode")]
        public string UomCode { get; set; }

        [XmlElement]
        public string FinalResult { get; set; }

        [XmlElement]
        public string Technician { get; set; }
    }
}
