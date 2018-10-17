using System;
using System.Runtime.Serialization;

namespace Freedom.Domain.CommandModel.Users
{
    [DataContract(Namespace = Namespace, IsReference = true)]
    public class ResetPasswordCommand : CommandBase
    {
        [DataMember(EmitDefaultValue = false)]
        public Guid UserId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string NewPassword { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool ForcePasswordChange { get; set; }
    }
}
