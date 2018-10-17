using System;
using System.Runtime.Serialization;

namespace Freedom.WebApi.Models
{
    [DataContract(Namespace = Namespace)]
    public class FileMetadata
    {
        public const string Namespace = "http://schemas.automatedstocktrader.com/bootstrapper";

        [DataMember]
        public string Name { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Version { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public long Length { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DateTime ModifiedDateTime { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Hash { get; set; }
    }
}
