using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Hedgerow;


namespace Hedgehog.SampleImportModel
{
    [XmlType(AnonymousType = true, Namespace = "http://schemas.hedgerowsoftware.com/sampleimportdefinition/v1.0")]
    [XmlRoot(Namespace = "http://schemas.hedgerowsoftware.com/sampleimportdefinition/v1.0", IsNullable = false)]
    public class SampleBatch 
    {
        private List<SampleDetail> _sampleDetails;

        public SampleBatch()
        { }

        [XmlElement]
        public string LaboratoryCode { get; set; }

        [XmlElement]
        public string LaboratoryName { get; set; }

        [XmlElement("FileCreationDate")]
        public SerializableDateTimeOffset FileCreationDateTimeOffset { get; set; }

        [XmlElement]
        public int TotalSampleRecords { get; set; }

        [XmlElement("SampleDetail")]
        public List<SampleDetail> SampleDetails
        {
            get { return _sampleDetails ?? (_sampleDetails = new List<SampleDetail>()); }
            set { _sampleDetails = value; }
        }

        public static SampleBatch Deserialize(string data)
        {
            if(string.IsNullOrWhiteSpace(data))
                return null;

            XmlSerializer serializer = new XmlSerializer(typeof(SampleBatch));
            using (XmlReader reader = new XmlTextReader(new StringReader(data)))
            {
               return (SampleBatch) serializer.Deserialize(reader);
            }
        }
      
    }
}
