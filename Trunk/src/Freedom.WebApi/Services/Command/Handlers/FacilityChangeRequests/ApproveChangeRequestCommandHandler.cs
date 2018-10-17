using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Hedgehog.CommandModel.FacilityChangeRequests;
using Hedgehog.Infrastructure;
using Hedgehog.Model;
using Hedgehog.Services.Command.Handlers.NexusIssues;

namespace Hedgehog.Services.Command.Handlers.FacilityChangeRequests
{
    public class ApproveChangeRequestCommandHandler : RepositoryCommandHandler<ApproveChangeRequestCommand>
    {
        public override IEnumerable<SystemPermission> RequiredPermissions
        {
            get { yield return SystemPermission.CloseChangeRequest; }
        }

        protected override async Task<bool> Handle(HedgehogRepository repository, ApproveChangeRequestCommand command,
            CommandExecutionContext context)
        {
            ApplicationSettingsBase settings = await repository.Context.GetApplicationSettingsAsync();

            FacilityChangeRequest changeRequest =
                await repository.GetFacilityChangeRequestAsync(command.FacilityChangeRequestId);

            if (changeRequest == null)
                throw new ArgumentException($"A Facility Change Request with the id {command.FacilityChangeRequestId} was not found.");

            FacilitySections changedSectionsThatRequiredApproval =
                changeRequest.ChangedSections & settings.ChangeRequestSectionsForSupervisorApproval;

            NexusIssue nexusIssue =
                await repository.GetNexusIssueAsync(changeRequest.NexusIssueId);

            Facility facility = await repository.GetFacilityAsync(changeRequest.FacilityId);

            if (facility == null || nexusIssue == null)
                throw new InvalidOperationException("INTERNAL ERROR: The facility or Nexus Issue related to the change request could not be loaded.");

            facility.Site = await repository.GetSiteAsync(facility.SiteId);

            Guid? customFormId = await repository.Context.CustomForm
                .Where(cf => cf.FacilityId == facility.Id)
                .Select(cf => cf.Id)
                .FirstOrDefaultAsync();

            changeRequest.Status = RequestStatus.Approved;
            repository.UpdateAuditProperties(changeRequest);

            Activity activity = new Activity();
            activity.NexusIssueId = nexusIssue.Id;
            activity.Source = ActivitySource.ChangeRequestAccepted;
            activity.ActivityDateTime = command.CommandCreatedDateTime;
            activity.PerformedById = context.CurrentUserId;
            activity.SetDelta(nameof(NexusIssue.Status), NexusIssueStatus.Open, NexusIssueStatus.Closed);

            if (command.CreateNewFacility)
            {
                Facility newFacility = new Facility();

                newFacility.Copy(facility);
                newFacility.Id = Guid.NewGuid();
                newFacility.FacilityTypeAttributeIds = facility.FacilityTypeAttributeIds;
                newFacility.OperationsRestrictionIds = facility.OperationsRestrictionIds;

                if (changeRequest.ChangedSections.HasFlag(FacilitySections.FacilityType))
                    newFacility.RiskRating = Rating.Invalid;

                newFacility.Site = facility.Site;
                changeRequest.ApplyChanges(newFacility);

                repository.Add(newFacility);

                facility.IsActive = false;
                repository.UpdateAuditProperties(facility);

                if (customFormId != null && customFormId.Value != Guid.Empty)
                {
                    CustomForm originalCustomForm = await repository.GetCustomFormAsync(customFormId.Value);

                    CustomForm newCustomForm = new CustomForm();

                    newCustomForm.Copy(originalCustomForm);
                    newCustomForm.Id = Guid.NewGuid();
                    newCustomForm.FacilityId = newFacility.Id;

                    foreach (CustomFormAnswer answer in originalCustomForm.Answers)
                    {
                        CustomFormAnswer newAnswer = new CustomFormAnswer();
                        newAnswer.Copy(answer);
                        newAnswer.Id = Guid.NewGuid();
                        newAnswer.CustomFormId = newCustomForm.Id;
                        newCustomForm.Answers.Add(newAnswer);
                    }

                    repository.Add(newCustomForm);
                }

                activity.Comments =
                    "Change request accepted. Closed original Facility and created a new (duplicate of the original) Facility with the changes applied.";
            }
            else
            {
                changeRequest.ApplyChanges(facility);
                repository.UpdateAuditProperties(facility);

                if (changedSectionsThatRequiredApproval == FacilitySections.None)
                {
                    activity.Comments = "This change request does not contain any changes requiring " +
                                        "supervisor approval. The change request has been accepted and the " +
                                        "changes applied to the Facility.";
                }
                else
                {
                    activity.Comments = "Change request accepted. Changes applied to existing Facility.";

                }
            }

            repository.Add(activity);

            await NexusCommandHandlerCommon.ReapplyAllDeltasAsync(repository.Context, nexusIssue.Id);

            return true;
        }
    }
}
