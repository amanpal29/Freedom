using System.Collections.Generic;
using System.Runtime.Serialization;
using Freedom.Domain.Model;
using Freedom.Annotations;

namespace Freedom.Domain.Services.Query
{
    [CollectionDataContract(Namespace = Constants.ContractNamespace, ItemName = "Item", IsReference = true)]
    public class EntityCollection : List<Entity>
    {
        public EntityCollection()
        {
        }

        public EntityCollection([NotNull] IEnumerable<Entity> collection)
            : base(collection)
        {
        }
    }
}
