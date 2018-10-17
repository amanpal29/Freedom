using System.Runtime.Serialization;
using Freedom.Domain.Model;

namespace Freedom.Domain.CommandModel.Users
{
    [DataContract(Namespace = Namespace, IsReference = true)]
    public class AddUserCommand : CommandBase
    {
        [DataMember(EmitDefaultValue = false)]
        public User User { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Password { get; set; }
    }
}