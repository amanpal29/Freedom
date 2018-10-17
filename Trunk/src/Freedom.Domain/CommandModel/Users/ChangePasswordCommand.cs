using System.Runtime.Serialization;

namespace Freedom.Domain.CommandModel.Users
{
    [DataContract(Namespace = Namespace, IsReference = true)]
    public class ChangePasswordCommand : CommandBase
    {
        [DataMember(EmitDefaultValue = false)]
        public string CurrentPassword { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string NewPassword { get; set; }
    }
}
