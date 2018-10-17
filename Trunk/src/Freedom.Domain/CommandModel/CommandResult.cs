using System.Runtime.Serialization;

namespace Freedom.Domain.CommandModel
{
    [DataContract(Namespace = CommandBase.Namespace, IsReference = true)]
    public class CommandResult
    {
        public CommandResult(bool success)
        {
            Success = success;
        }

        [DataMember]
        public bool Success { get; set; }
    }
}