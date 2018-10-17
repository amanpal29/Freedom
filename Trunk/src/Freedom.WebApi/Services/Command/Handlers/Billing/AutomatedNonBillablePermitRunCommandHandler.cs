using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Hedgehog.CommandModel.Billing;
using Hedgehog.Model;
using Hedgerow;

namespace Hedgehog.Services.Command.Handlers.Billing
{
    public class AutomatedNonBillablePermitRunCommandHandler :
        LocalContextCommandHandler<AutomatedNonBillablePermitRunCommand>
    {
        public override IEnumerable<SystemPermission> RequiredPermissions
        {
            get { yield return SystemPermission.AddPermit; }
        }

        protected override async Task<bool> Handle(HedgehogLocalContext db, AutomatedNonBillablePermitRunCommand command,
            CommandExecutionContext context)
        {
            Batch batch = new Batch();
            batch.Id = command.BatchId;
            batch.BatchType = BatchType.Permits;
            batch.Number = $"BN{command.CommandCreatedDateTime:yyyyMMddhhmmss}";
            batch.BatchDateTime = command.CommandCreatedDateTime;
            batch.IsAutomated = true;
            batch.IsLocked = false;
            db.Batch.Add(batch);

            IQueryable<Facility> facilityQuery = db.Facility
                .Where(f => f.IsActive &&
                            command.NonBilledFacilityTypeIds.Contains(f.FacilityTypeId) &&
                            !f.FacilityType.IsBillableFacilityType && 
                            f.PermitTypeId != null && command.PermitTypeIds.Contains((Guid) f.PermitTypeId));

            Dictionary<Guid, DateTime> facilityPermitExpriyDate = await db.Permit
                .Where(p => facilityQuery.Select(f => f.Id).Contains(p.FacilityId) && p.RevocationDateTime != null)
                .GroupBy(p => p.FacilityId)
                .ToDictionaryAsync(grp => grp.Key, grp => grp.Max(p => p.ExpiryDate));


            List<Facility> facilities = await facilityQuery.ToListAsync();

            await facilityQuery.SelectMany(f => f.FacilityOperationsRestriction).LoadAsync();

            int permitCount = 0;

            ApplicationSetting lastIssuedPermitNumber = db.ApplicationSetting.FirstOrDefault(s => s.Key == "LastIssuedPermitNumber") ?? new ApplicationSetting { Key = "LastIssuedPermitNumber" };
            IntegerSequence sequence = command.MaximumPermitNumber.HasValue ? new IntegerSequence(command.MinimumPermitNumber, command.MaximumPermitNumber.Value) : new IntegerSequence(command.MinimumPermitNumber);

            foreach (Facility facility in facilities)
            {
                if (!sequence.HasValues) break;

                if (facilityPermitExpriyDate.ContainsKey(facility.Id) &&
                    facilityPermitExpriyDate[facility.Id] >= command.ExpiryDate)
                    continue;

                permitCount++;

                Permit permit = new Permit();

                permit.IssuedById = context.CurrentUserId;

                PermitCreationHelper.PopulatePermitWithBatchAndFacilityInfo(permit, batch, facility);

                permit.Period = GetPermitPeriod(command, facility);
                lastIssuedPermitNumber.Value = permit.Number = sequence.GetNextValue().ToString("d6");

                db.Permit.Add(permit);
            }

            if (lastIssuedPermitNumber.State == EntityState.Added)
                db.ApplicationSetting.Add(lastIssuedPermitNumber);

            return permitCount > 0;
        }

        private static DateRange GetPermitPeriod(AutomatedNonBillablePermitRunCommand command, Facility facility)
        {
            DateRange range = new DateRange(command.EffectiveDate, command.ExpiryDate);

            if (facility.OperatingSchedule.IsOpenYearRound)
                return range;

            DateTime startDate = range.Contains(facility.OperatingSchedule.StartDate(command.EffectiveDate.Year))
                            ? facility.OperatingSchedule.StartDate(command.EffectiveDate.Year)
                            : facility.OperatingSchedule.StartDate(command.EffectiveDate.Year + 1);

            Debug.Assert(range.Contains(startDate));

            DateTime endDate = facility.OperatingSchedule.EndDate(startDate.Year);

            if (endDate < startDate)
                endDate = facility.OperatingSchedule.EndDate(startDate.Year + 1);

            return new DateRange(startDate, endDate.AddDays(1));
        }
    
    }
}