using System.Runtime.Serialization;
using Freedom.Domain.Model;

namespace Freedom.Domain.Services.Repository
{
    [DataContract(Namespace = Entity.Namespace)]
    public enum AggregateFunction
    {
        [EnumMember] Invalid,
        [EnumMember] Count,
        [EnumMember] Average,
        [EnumMember] Min,
        [EnumMember] Max,
        [EnumMember] Sum
    }
}
