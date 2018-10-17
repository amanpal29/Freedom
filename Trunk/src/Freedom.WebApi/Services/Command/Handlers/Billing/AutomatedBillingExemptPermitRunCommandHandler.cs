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
    public class AutomatedBillingExemptPermitRunCommandHandler :
        LocalContextCommandHandler<AutomatedBillingExemptPermitRunCommand>
    {
        public override IEnumerable<SystemPermission> RequiredPermissions
        {
            get { yield return SystemPermission.AddPermit; }
        }

        protected override async Task<bool> Handle(HedgehogLocalContext db,
            AutomatedBillingExemptPermitRunCommand command,
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

            int billingCycleStartMonth = command.BillingCycleStartDate.Month;
            int billingYear = command.BillingCycleStartDate.Year;

            IQueryable<AccountFacility> accountFacilities = db.AccountFacility
                .Include(af => af.Facility)
                .Where(af =>
                    af.Facility.IsActive &&
                    af.Facility.PermitTypeId != null &&
                    command.PermitTypeIds.Contains((Guid) af.Facility.PermitTypeId) &&
                    af.BillingCycleStartMonth == billingCycleStartMonth &&
                    af.IsExemptFromBilling &&
                    af.EndDateTime == null);

            IQueryable<Guid> facilityIds = accountFacilities.Select(af => af.FacilityId);

            await accountFacilities.LoadAsync();

            Dictionary<Guid, DateTime> facilityPermitExpriyDate = await db.Permit
                .Where(p => facilityIds.Contains(p.FacilityId) && p.RevocationDateTime != null)
                .GroupBy(p => p.FacilityId)
                .ToDictionaryAsync(grp => grp.Key, grp => grp.Max(p => p.ExpiryDate));

            await db.FacilityOperationsRestriction
                .Where(fr => facilityIds.Contains(fr.FacilityId))
                .LoadAsync();

            bool success = false;

            int? permitNumber = null;

            IntegerSequence sequence = new IntegerSequence(command.MinimumPermitNumber, command.MaximumPermitNumber);

            foreach (AccountFacility accountFacility in accountFacilities)
            {
                if (!sequence.HasValues) break;

                if (accountFacility.Facility.PermitTypeId == null) continue;  // It should be impossible for PermitTypeId to be null.

                DateTime? permitExpriyDate =
                    facilityPermitExpriyDate.ContainsKey(accountFacility.FacilityId)
                        ? facilityPermitExpriyDate[accountFacility.FacilityId]
                        : (DateTime?) null;
                
                if (permitExpriyDate > command.BillingCycleStartDate) continue;

                permitNumber = sequence.GetNextValue();

                Permit permit = new Permit();

                permit.AccountId = accountFacility.AccountId;
                permit.FacilityId = accountFacility.FacilityId;
                permit.BatchId = batch.Id;
                permit.TypeId = accountFacility.Facility.PermitTypeId.Value;
                permit.IssuedById = context.CurrentUserId;
                permit.Number = permitNumber.Value.ToString("d6");
                permit.Period = GetPermitPeriod(accountFacility.Facility,
                    billingYear, accountFacility.BillingCycleStartMonth);
                permit.Conditions = accountFacility.Facility.OperatingConditions;
                permit.OperationsRestrictionIds = accountFacility.Facility.OperationsRestrictionIds;

                db.Permit.Add(permit);

                success = true;
            }

            await UpdateLastIssuedPermitNumberAsync(db, permitNumber);

            return success;
        }

        private static async Task UpdateLastIssuedPermitNumberAsync(HedgehogLocalContext db, int? permitNumber)
        {
            const string lastIssuedPermitNumberKey = "LastIssuedPermitNumber";

            if (permitNumber == null) return;

            ApplicationSetting lastIssuedPermitNumber = await db.ApplicationSetting
                .FirstOrDefaultAsync(s => s.Key == lastIssuedPermitNumberKey);

            if (lastIssuedPermitNumber == null)
            {
                lastIssuedPermitNumber = new ApplicationSetting();
                lastIssuedPermitNumber.Key = lastIssuedPermitNumberKey;
                db.ApplicationSetting.Add(lastIssuedPermitNumber);
            }

            lastIssuedPermitNumber.Value = permitNumber.Value.ToString();
        }

        private static DateRange GetPermitPeriod(Facility facility, int billingYear, int billingCycleStartMonth)
        {
            DateTime startDate = new DateTime(billingYear, billingCycleStartMonth, 1);

            DateTime endDate = startDate.AddYears(1);

            if (facility.OperatingSchedule.IsOpenYearRound)
                return new DateRange(startDate, endDate);

            DateRange billingCycle = new DateRange(startDate, endDate);

            startDate = billingCycle.Contains(facility.OperatingSchedule.StartDate(billingYear))
                            ? facility.OperatingSchedule.StartDate(billingYear)
                            : facility.OperatingSchedule.StartDate(billingYear + 1);

            Debug.Assert(billingCycle.Contains(startDate));

            endDate = facility.OperatingSchedule.EndDate(startDate.Year);

            if (endDate < startDate)
                endDate = facility.OperatingSchedule.EndDate(startDate.Year + 1);

            return new DateRange(startDate, endDate.AddDays(1));
        }
    }
}