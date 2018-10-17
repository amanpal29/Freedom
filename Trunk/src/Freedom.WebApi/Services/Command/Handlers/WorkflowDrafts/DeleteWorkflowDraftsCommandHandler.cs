using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hedgehog.CommandModel.WorkflowDrafts;
using Hedgehog.Model;

namespace Hedgehog.Services.Command.Handlers.WorkflowDrafts
{
    public class DeleteWorkflowDraftsCommandHandler : RepositoryCommandHandler<DeleteWorkflowDraftsCommand>
    {
        public override IEnumerable<SystemPermission> RequiredPermissions
        {
            get { yield break; }
        }
        protected override async Task<bool> Handle(HedgehogRepository repository, DeleteWorkflowDraftsCommand command, CommandExecutionContext context)
        {
            List<WorkflowDraft> drafts =
                await repository.Context.WorkflowDraft.Where(d => d.ServiceProviderId == command.ServiceProviderId).ToListAsync();

            foreach (Guid draftId in drafts.Select(d => d.Id))
            {
                await repository.DeleteWorkflowDraftAsync(draftId);
            }

            return true;
        }
    }
}
