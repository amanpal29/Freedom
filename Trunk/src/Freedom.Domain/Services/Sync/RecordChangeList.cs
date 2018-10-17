using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Freedom.Domain.Services.Sync
{
    [DataContract(Namespace = Constants.ContractNamespace)]
    public class RecordChangeList
    {
        public RecordChangeList()
        {
            UpdatedRecords = new List<Guid>();
            DeletedRecords = new List<Guid>();
        }

        public RecordChangeList(IEnumerable<Guid> updatedRecords, IEnumerable<Guid> deletedRecords)
        {
            UpdatedRecords = new List<Guid>(updatedRecords);
            DeletedRecords = new List<Guid>(deletedRecords);
        }

        [DataMember]
        public IList<Guid> UpdatedRecords { get; set; }

        [DataMember]
        public IList<Guid> DeletedRecords { get; set; }
    }
}
