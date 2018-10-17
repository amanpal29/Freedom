using Hedgehog.Model;
using System;
using System.Linq;

namespace Hedgehog.Services.Command.Handlers.Billing
{
    internal class PermitCreationHelper
    {
        internal static void PopulatePermitWithBatchAndFacilityInfo(Permit permit, Batch batch, Facility facility)
        {
            permit.BatchId = batch.Id;
            permit.FacilityId = facility.Id;
            if (facility.PermitTypeId != null) permit.TypeId = facility.PermitTypeId.Value;
            permit.Conditions = facility.OperatingConditions;

            foreach (Guid operationsRestrictionId in facility.FacilityOperationsRestriction.Select(or => or.OperationsRestrictionId).Distinct())
                permit.PermitOperationsRestriction.Add(new PermitOperationsRestriction { OperationsRestrictionId = operationsRestrictionId });

            permit.MaximumCapacity = facility.MaximumCapacity;
        }
    }
}
