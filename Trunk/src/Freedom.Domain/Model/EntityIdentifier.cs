using System;
using System.Collections.Generic;
using System.Linq;

namespace Freedom.Domain.Model
{
    public struct EntityIdentifier
    {
        #region Fields

        public static readonly EntityIdentifier Empty = new EntityIdentifier();

        #endregion

        #region Constructors

        public EntityIdentifier(Type entityType, Guid id)
        {
            EntityType = entityType;
            Id = id;
        }

        public EntityIdentifier(EntityBase entity)
            : this()
        {
            if (entity == null) return;
            EntityType = entity.GetType();
            Id = entity.Id;
        }

        #endregion

        #region Properties

        public Type EntityType { get; }

        public Guid Id { get; }

        #endregion

        #region Public Methods

        public bool IsMatch(EntityBase entity)
        {
            if (entity == null)
                return false;

            return entity.GetType() == EntityType && entity.Id == Id;
        }

        public static bool IsEmpty(EntityIdentifier entityIdentifier)
        {
            return entityIdentifier.EntityType == null && entityIdentifier.Id == Guid.Empty;
        }

        #endregion

        #region Object and Operator Overrides 

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            return Equals((EntityIdentifier) obj);
        }

        public bool Equals(EntityIdentifier entityIdentifier)
        {
            return entityIdentifier.EntityType == EntityType && entityIdentifier.Id == Id;
        }

        public static bool operator ==(EntityIdentifier a, EntityIdentifier b)
        {
            return a.EntityType == b.EntityType && a.Id == b.Id;
        }

        public static bool operator !=(EntityIdentifier a, EntityIdentifier b)
        {
            return a.EntityType != b.EntityType || a.Id != b.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return IsEmpty(this) ? "(Empty)" : $"{EntityType.Name} - {Id}";
        }

        #endregion
    }

    public static class EntityIdentifierExtensions
    {
        public static void Add(this ICollection<EntityIdentifier> collection, EntityBase entity)
        {
            if (entity != null)
            {
                collection.Add(new EntityIdentifier(entity));
            }
        }

        public static void AddMany(this ICollection<EntityIdentifier> collection, IEnumerable<EntityBase> entities)
        {
            if (entities != null)
            {
                foreach (EntityBase entity in entities.Where(entity => entity != null))
                    collection.Add(new EntityIdentifier(entity));
            }
        }

        public static bool Contains(this ICollection<EntityIdentifier> collection, EntityBase entity)
        {
            return entity == null || collection.Contains(new EntityIdentifier(entity));
        }
    }
}