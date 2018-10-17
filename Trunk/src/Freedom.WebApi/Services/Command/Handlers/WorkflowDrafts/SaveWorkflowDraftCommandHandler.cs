using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using Hedgehog.CommandModel.WorkflowDrafts;
using Hedgehog.Model;

namespace Hedgehog.Services.Command.Handlers.WorkflowDrafts
{
    public class SaveWorkflowDraftCommandHandler : RepositoryCommandHandler<SaveWorkflowDraftCommand>
    {
        public override IEnumerable<SystemPermission> RequiredPermissions
        {
            get { yield break; }
        }
        protected override async Task<bool> Handle(HedgehogRepository repository, SaveWorkflowDraftCommand command, CommandExecutionContext context)
        {
            if(command.WorkflowDraft.State == EntityState.Added)
                repository.Add(command.WorkflowDraft);
            else
            {
                await repository.UpdateAsync(command.WorkflowDraft);
            }

            return true;
        }
    }
}
