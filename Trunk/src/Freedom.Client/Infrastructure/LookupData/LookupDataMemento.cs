using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Freedom.Client.Infrastructure.LookupData
{
    [DataContract(Namespace = Domain.Constants.ContractNamespace, Name = "LookupDataMemento")]
    internal class LookupDataMemento<TEntity> : IEnumerable<TEntity>
    {
        private IEnumerable<TEntity> _entities;

        protected internal LookupDataMemento(DateTime refreshedDateTime, IEnumerable<TEntity> entities)
        {
            EntityType = typeof (TEntity).FullName;
            RefreshedDateTime = refreshedDateTime;
            _entities = entities;
        }

        [DataMember(Order = 0)]
        public string EntityType { get; set; }

        [DataMember(Order = 1)]
        public DateTime RefreshedDateTime { get; set; }

        [DataMember(Order = 2)]
        public IEnumerable<TEntity> Entities
        {
            get { return _entities ?? (_entities = Enumerable.Empty<TEntity>()); }
            set { _entities = value; }
        }

        public IEnumerator<TEntity> GetEnumerator()
        {
            return Entities.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
