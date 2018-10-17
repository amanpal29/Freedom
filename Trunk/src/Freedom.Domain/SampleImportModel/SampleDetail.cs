using System.Collections.Generic;
using System.Xml.Serialization;
using Hedgerow;

namespace Hedgehog.SampleImportModel
{
    [XmlType(AnonymousType = true, Namespace = "http://schemas.hedgerowsoftware.com/sampleimportdefinition/v1.0")]
    public class SampleDetail
    {
        private List<SampleParameterDetail> _sampleParameterDetails; 

        [XmlElement]
        public string SampleNumber { get; set; }

        [XmlElement]
        public string SampleTestTypeCode { get; set; }

        [XmlElement]
        public string SampleTestType { get; set; }

        [XmlElement]
        public string SampleReason { get; set; }

        [XmlElement]
        public string FacilityNumber { get; set; }

        [XmlElement]
        public string FacilityName { get; set; }

        [XmlElement]
        public string SampleSiteTypeCode { get; set; }

        [XmlElement]
        public string SampleSiteType { get; set; }

        [XmlElement]
        public string SampleSite { get; set; }

        [XmlElement]
        public double? SampleSiteLatitude { get; set; }

        [XmlElement]
        public double? SampleSiteLongitude { get; set; }

        [XmlElement]
        public double? SampleSiteElevation { get; set; }

        [XmlElement]
        public SerializableDateTimeOffset SampleCollectedDate { get; set; }

        [XmlElement]
        public string SampleCollectedBy { get; set; }

        [XmlElement]
        public SerializableDateTimeOffset? SampleSubmittedDate { get; set; }

        [XmlElement]
        public string SampleSubmittedBy { get; set; }

        [XmlElement]
        public SerializableDateTimeOffset SampleAnalyzedDate { get; set; }

        [XmlElement]
        public string Comments { get; set; }

        [XmlElement]
        public string AdvisoryTypeCode { get; set; }

        [XmlElement("SampleParameterDetail")]
        public List<SampleParameterDetail> SampleParameterDetails
        {
            get { return _sampleParameterDetails ?? (_sampleParameterDetails = new List<SampleParameterDetail>()); }
            set { _sampleParameterDetails = value; }
        }
    }
}
