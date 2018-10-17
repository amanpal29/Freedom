using System.Collections.Generic;
using System.Data.Entity.Core.Objects;

namespace Freedom.Domain.Model.Behaviors
{
    public interface IEntityCommitBehavior
    {
        void AfterCommit(IList<EntityBase> entities, ObjectContext context);
    }
}
