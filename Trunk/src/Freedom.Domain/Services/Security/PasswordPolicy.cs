using System.Runtime.Serialization;

namespace Freedom.Domain.Services.Security
{
    [DataContract(Namespace = Constants.ContractNamespace)]
    public class PasswordPolicy
    {
        [DataMember]
        public int MinimumPasswordLength { get; set; }

        [DataMember]
        public int MinimumPasswordComplexity { get; set; }
    }
}
