using System.Runtime.Serialization;
using Freedom.Domain.Model;

namespace Freedom.Domain.CommandModel.Users
{
    [DataContract(Namespace = Namespace, IsReference = true)]
    public class UpdateUserCommand : CommandBase
    {
        public UpdateUserCommand()
        {
        }

        public UpdateUserCommand(User user)
        {
            User = user;
        }

        [DataMember(EmitDefaultValue = false)]
        public User User { get; set; }
    }
}