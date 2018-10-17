using System;

namespace Freedom.Domain.Model
{
    public abstract partial class EntityBase : IEquatable<EntityBase>
    {
        #region Equality Operators

        public bool Equals(EntityBase entityBase)
        {
            if (ReferenceEquals(entityBase, null)) return false;
            if (ReferenceEquals(this, entityBase)) return true;
            if (GetType() != entityBase.GetType()) return false;
            if (entityBase.Id == Guid.Empty) return false;
            return entityBase.Id == Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;

            EntityBase entity = obj as EntityBase;

            return entity != null && Equals(entity);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        #endregion
    }
}
