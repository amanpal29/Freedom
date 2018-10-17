using System;
using System.Runtime.Serialization;

namespace Freedom.Domain.Services.Sync
{
    [DataContract]
    public class UniqueNumberBlock
    {
        public UniqueNumberBlock(string key, Tuple<long, long> block)
        {
            Key = key;
            Start = block.Item1;
            End = block.Item2;
        }

        [DataMember]
        public string Key { get; set; }

        [DataMember]
        public long Start { get; set; }

        [DataMember]
        public long End { get; set; }

        public Tuple<long, long> AsTuple()
        {
            return new Tuple<long, long>(Start, End);
        }
    }
}
