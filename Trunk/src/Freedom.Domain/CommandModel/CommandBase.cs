using System;
using System.Runtime.Serialization;
using Freedom.Domain.CommandModel.Users;
using Freedom.Domain.Services.Time;
using Freedom.Extensions;

namespace Freedom.Domain.CommandModel
{
    [DataContract(Namespace = Namespace, IsReference = true)]
    [KnownType(typeof(AddUserCommand))]
    [KnownType(typeof(UpdateUserCommand))]
    public abstract class CommandBase
    {
        protected CommandBase()
        {
            CommandCreatedDateTime = IoC.TryGet<ITimeService>()?.Now ?? DateTimeOffset.Now;
        }

        public const string Namespace = "http://schemas.automatedstocktrader.com/freedom";

        public virtual string DisplayName => GetType().Name.ToDisplayName();

        [DataMember]
        public DateTimeOffset CommandCreatedDateTime { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool ExecuteNonInteractive { get; set; }
    }
}
