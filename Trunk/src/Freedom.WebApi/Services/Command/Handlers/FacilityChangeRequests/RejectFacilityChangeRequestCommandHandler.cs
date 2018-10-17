using System.Collections.Generic;
using System.Threading.Tasks;
using Hedgehog.CommandModel.FacilityChangeRequests;
using Hedgehog.Model;
using Hedgehog.Services.Command.Handlers.NexusIssues;

namespace Hedgehog.Services.Command.Handlers.FacilityChangeRequests
{
    public class RejectFacilityChangeRequestCommandHandler : RepositoryCommandHandler<RejectFacilityChangeRequestCommand>
    {
        public override IEnumerable<SystemPermission> RequiredPermissions
        {
            get { yield return SystemPermission.CloseChangeRequest; }
        }

        protected override async Task<bool> Handle(HedgehogRepository repository,
            RejectFacilityChangeRequestCommand command, CommandExecutionContext context)
        {
            FacilityChangeRequest changeRequest =
                await repository.GetFacilityChangeRequestAsync(command.FacilityChangeRequestId);

            changeRequest.Status = RequestStatus.Rejected;
            repository.UpdateAuditProperties(changeRequest);

            Activity activity = new Activity();
            activity.NexusIssueId = changeRequest.NexusIssueId;
            activity.Source = ActivitySource.ChangeRequestRejected;
            activity.ActivityDateTime = command.CommandCreatedDateTime;
            activity.PerformedById = context.CurrentUserId;
            activity.Comments = "Change request rejected. Changes will not be applied to this Facility.";
            activity.SetDelta(nameof(NexusIssue.Status), NexusIssueStatus.Open, NexusIssueStatus.Closed);
            repository.Add(activity);

            await NexusCommandHandlerCommon.ReapplyAllDeltasAsync(repository.Context, changeRequest.NexusIssueId);

            return true;
        }
    }
}
