using System.Collections.Generic;
using System.Threading.Tasks;
using Hedgehog.CommandModel.WorkflowDrafts;
using Hedgehog.Model;

namespace Hedgehog.Services.Command.Handlers.WorkflowDrafts
{
    public class DeleteWorkflowDraftCommandHandler : RepositoryCommandHandler<DeleteWorkflowDraftCommand>
    {
        public override IEnumerable<SystemPermission> RequiredPermissions
        {
            get { yield break;}
        }
        protected override async Task<bool> Handle(HedgehogRepository repository, DeleteWorkflowDraftCommand command, CommandExecutionContext context)
        {
            await repository.DeleteWorkflowDraftAsync(command.WorkflowDraftId);
            return true;
        }
    }
}
