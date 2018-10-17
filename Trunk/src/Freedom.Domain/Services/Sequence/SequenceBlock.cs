using System.Runtime.Serialization;

namespace Freedom.Domain.Services.Sequence
{
    [DataContract(Namespace = Constants.ContractNamespace)]
    public class SequenceBlock
    {
        public SequenceBlock()
        {
        }

        public SequenceBlock(long start, long end)
        {
            Start = start;
            End = end;
        }

        [DataMember]
        public long Start { get; set; }

        [DataMember]
        public long End { get; set; }
    }
}
