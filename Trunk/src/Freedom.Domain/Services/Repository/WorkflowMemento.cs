using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Freedom.Domain.CommandModel;

namespace Freedom.Domain.Services.Repository
{
    [DataContract]
    public class WorkflowMemento
    {
        private List<CommandBase> _commands; 

        public WorkflowMemento(Type workflowType, Guid createdById, string createdByName)
        {
            WorkflowType = workflowType.FullName;
            CreatedById = createdById;
            CreatedByName = createdByName;
            CreationDateTime = DateTime.UtcNow;
        }
        
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public string WorkflowType { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public Guid CreatedById { get; set; }

        [DataMember]
        public string CreatedByName { get; set; }

        [DataMember]
        public DateTime CreationDateTime { get; set; }

        [DataMember]
        public List<CommandBase> Commands
        {
            get { return _commands ?? (_commands = new List<CommandBase>()); }
            set { _commands = value; }
        }

        public bool Any<TCommand>(Func<TCommand, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return _commands != null && _commands.OfType<TCommand>().Any(predicate);
        }
    }
}
