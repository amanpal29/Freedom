using System;
using System.Runtime.Serialization;

namespace Freedom.Domain.Services.Status
{
    [DataContract(Namespace = Constants.ContractNamespace)]
    public class VersionData
    {
        [DataMember(EmitDefaultValue = false)]
        public string MinimumClientVersion { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string MaximumClientVersion { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string ServerVersion { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int DatabaseRevision { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Guid DatabaseIdentifier { get; set; }
    }
}