using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Freedom.Domain.Services.Sync
{
    [DataContract(Namespace = Constants.ContractNamespace)]
    public class LinkChangeList
    {
        public LinkChangeList()
        {
        }

        public LinkChangeList(IEnumerable<Guid[]> addedLinks, IEnumerable<Guid[]> removedLinks)
        {
            AddedLinks = new List<Guid[]>(addedLinks);
            RemovedLinks = new List<Guid[]>(removedLinks);
        }

        [DataMember]
        public IList<Guid[]>  AddedLinks { get; set; }

        [DataMember]
        public IList<Guid[]>  RemovedLinks { get; set; }
    }
}
