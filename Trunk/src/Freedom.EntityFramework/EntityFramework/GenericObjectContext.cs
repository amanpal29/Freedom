using System;
using System.Collections.Generic;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Objects;

namespace Freedom.EntityFramework
{
    public class GenericObjectContext : ObjectContext
    {
        #region Fields

        private readonly Dictionary<string, object> _objectSets = new Dictionary<string, object>();

        #endregion

        #region Constructors

        public GenericObjectContext(EntityConnection connection)
            : base(connection)
        {
        }

        public GenericObjectContext(EntityConnection connection, bool contextOwnsConnection)
            : base(connection, contextOwnsConnection)
        {
        }

        public GenericObjectContext(string connectionString)
            : base(connectionString)
        {
        }

        protected GenericObjectContext(string connectionString, string defaultContainerName)
            : base(connectionString, defaultContainerName)
        {
        }

        protected GenericObjectContext(EntityConnection connection, string defaultContainerName)
            : base(connection, defaultContainerName)
        {
        }

        #endregion

        #region Private Methods

        protected string GetKey(Type entityType)
        {
            if (entityType == null)
                throw  new ArgumentNullException(nameof(entityType));

            if (!entityType.IsClass)
                throw new ArgumentException("entityType must be a reference type", nameof(entityType));

            return entityType.Name;
        }

        #endregion

        #region Public Methods

        public ObjectSet<TEntity> Set<TEntity>()
            where TEntity : class
        {
            string key = GetKey(typeof (TEntity));

            if (_objectSets.ContainsKey(key))
                return (ObjectSet<TEntity>) _objectSets[key];

            ObjectSet<TEntity> result = CreateObjectSet<TEntity>();

            _objectSets.Add(key, result);

            return result;
        }

        #endregion
    }
}
