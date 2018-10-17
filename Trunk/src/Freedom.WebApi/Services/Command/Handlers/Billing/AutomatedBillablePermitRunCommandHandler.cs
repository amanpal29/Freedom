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
    public class AutomatedBillablePermitRunCommandHandler : LocalContextCommandHandler<AutomatedBillablePermitRunCommand>
    {
        public override IEnumerable<SystemPermission> RequiredPermissions
        {
            get { yield return SystemPermission.AddPermit; }
        }

        protected override async Task<bool> Handle(HedgehogLocalContext db, AutomatedBillablePermitRunCommand command, CommandExecutionContext context)
        {
            IntegerSequence sequence = new IntegerSequence(command.MinimumPermitNumber, command.MaximumPermitNumber);

            DateTimeOffset today = new DateTimeOffset(command.CommandCreatedDateTime.Date, command.CommandCreatedDateTime.Offset);

            DateTime startOfTomorrow = today.AddDays(1d).UtcDateTime;

            DateTime cutoffDate = today.AddYears(-1).UtcDateTime;

            Batch batch = new Batch();

            batch.Id = command.BatchId;
            batch.BatchType = BatchType.Permits;
            batch.BatchDateTime = command.CommandCreatedDateTime;
            batch.Number = $"BN{command.CommandCreatedDateTime:yyyyMMddhhmmss}";
            batch.IsAutomated = true;
            batch.IsLocked = false;

            db.Batch.Add(batch);

            HashSet<Guid> workAreaIds = new HashSet<Guid>();

            if (command.CanFilterByJustMyWorkAreas)
            {
                IQueryable<Guid> query =
                    db.WorkAreaServiceProvider.Where(x => x.ServiceProviderId == context.CurrentUserId)
                        .Select(x => x.WorkAreaId);

                foreach (Guid workAreaId in query.ToList())
                    workAreaIds.Add(workAreaId);
            }
            else
            {
                foreach (Guid workAreaId in command.WorkAreaIds)
                    workAreaIds.Add(workAreaId);
            }

            IQueryable<Guid?> accountIds = db.Transaction
                .Where(t => t.Class == TransactionClass.PermitFee &&
                            t.PermitId == null &&
                            t.TransactionDateTime > cutoffDate &&
                            t.AccountId != null)
                .Select(t => t.AccountId);

            await db.AccountFacility
                .Include(af => af.Facility)
                .Where(af => accountIds.Contains(af.AccountId) && af.EndDateTime == null)
                .LoadAsync();

            await db.Transaction
                .Where(t => accountIds.Contains(t.AccountId))
                .LoadAsync();

            await db.Invoice
                .Where(i => accountIds.Contains(i.AccountId))
                .LoadAsync();

            int permitCount = 0;

            string lastIssuedPermitNumber = null;

            List<Account> accounts = await db.Account
                .Where(a => accountIds.Contains(a.Id) && a.WorkAreaId != null && workAreaIds.Contains(a.WorkAreaId.Value))
                .ToListAsync();

            foreach (Account account in accounts)
            {
                List<Transaction> transactions = db.Transaction.Local
                    .Where(t => t.AccountId == account.Id)
                    .ToList();

                RemoveReversedPayments(transactions);

                RemovePaymentsThatHaveNotClearedBy(transactions, startOfTomorrow);

                Dictionary<Guid, decimal> netAmounts = CalculateNetTransactionAmounts(transactions);

                Debug.Assert(netAmounts.Values.Sum() == transactions.Sum(t => t.Amount));

                // Calculate the amount of payments that are not applied to permits yet...

                decimal unallocatedPayments = -transactions
                    .Where(t => t.Class != TransactionClass.PermitFee || t.PermitId != null)
                    .Sum(t => netAmounts[t.Id]);

                IEnumerable<Transaction> permitFeesToConsider = transactions
                    .Where(t => t.Class == TransactionClass.PermitFee && t.PermitId == null && !t.IsOnHold)
                    .ToList();

                IEnumerable<Transaction> sortedPermitFeesToConsider = permitFeesToConsider
                    .OrderBy(t => t.Invoice?.InvoiceDate ?? t.TransactionDateTime)
                    .ThenBy(t => t.InvoiceLine);

                foreach (Transaction permitFee in sortedPermitFeesToConsider)
                {
                    if (!sequence.HasValues) break;

                    if (!permitFee.FacilityId.HasValue || !permitFee.BillingYear.HasValue || permitFee.PermitFeeCategoryId == null) continue;  // Should never happen...

                    if (!command.PermitFeeCategoryIds.Contains(permitFee.PermitFeeCategoryId.Value)) continue;  // We're not issueing permits for this fee category

                    if (permitFee.Facility.PermitTypeId == null ||
                        !command.PermitTypeIds.Contains(permitFee.Facility.PermitTypeId.Value)) continue; // We're not issueing permits for this permit type...

                    AccountFacility accountFacility = db.AccountFacility.Local
                        .FirstOrDefault(af => af.AccountId == permitFee.AccountId &&
                                              af.FacilityId == permitFee.FacilityId &&
                                              af.EndDateTime == null);

                    if (accountFacility == null) continue;  // Facility is not associated with this account (anymore)...

                    if (netAmounts[permitFee.Id] <= 0m) continue;  // Permit fee was fully reversed, don't issue permit

                    if (netAmounts[permitFee.Id] > unallocatedPayments) break;  // Ran out of money to cover permits...

                    unallocatedPayments -= netAmounts[permitFee.Id];

                    permitCount++;

                    Permit permit = BuildPermit(batch, accountFacility, context.CurrentUserId, permitFee.BillingYear.Value);

                    lastIssuedPermitNumber = permit.Number = sequence.GetNextValue().ToString("d6");

                    db.Permit.Add(permit);

                    permitFee.PermitId = permit.Id;
                }

            }

            if (permitCount > 0 && !string.IsNullOrEmpty(lastIssuedPermitNumber))
                await SaveLastIssuedPermitNumber(db, lastIssuedPermitNumber);

            return permitCount > 0;
        }

        private static async Task SaveLastIssuedPermitNumber(HedgehogLocalContext db, string lastIssuedPermitNumber)
        {
            ApplicationSetting applicationSetting = await db.ApplicationSetting
                .FirstOrDefaultAsync(x => x.Key == "LastIssuedPermitNumber");

            if (applicationSetting == null)
            {
                applicationSetting = new ApplicationSetting();
                applicationSetting.Key = "LastIssuedPermitNumber";
                db.ApplicationSetting.Add(applicationSetting);
            }

            applicationSetting.Value = lastIssuedPermitNumber;
        }

        private static Permit BuildPermit(Batch batch, AccountFacility accountFacility, Guid currentUserId, int billingYear)
        {
            // Due to the a Where clause when getting the list of facilities,
            // accountFacility.Facility.PermitTypeId == command.PermitTypeId
            Debug.Assert(accountFacility.Facility.PermitTypeId != null,
                "accountFacility.Facility.PermitTypeId != null");

            Permit permit = new Permit();

            permit.AccountId = accountFacility.AccountId;
            permit.IssuedById = currentUserId;

            PermitCreationHelper.PopulatePermitWithBatchAndFacilityInfo(permit, batch, accountFacility.Facility);

            permit.Period = GetPermitPeriod(accountFacility.Facility,
                billingYear, accountFacility.BillingCycleStartMonth);

            return permit;
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

        private static void RemoveReversedPayments(ICollection<Transaction> transactions)
        {
            List<Transaction> reversals = transactions
                .Where(t => t.Class == TransactionClass.PaymentReversal)
                .ToList();

            foreach (Transaction paymentReversal in reversals)
            {
                Transaction payment = transactions.FirstOrDefault(t => t.Id == paymentReversal.RelatedTransactionId);

                // This *shouldn't* happen, either a FK is fucked, or the reversal is in a different account than the payment.
                if (payment == null) continue; 

                transactions.Remove(payment);
                transactions.Remove(paymentReversal);
            }
        }

        private static void RemovePaymentsThatHaveNotClearedBy(List<Transaction> transactions, DateTime minimumClearedDateTime)
        {
            transactions.RemoveAll(t => t.Class == TransactionClass.Payment && t.ClearedDateTime >= minimumClearedDateTime);
        }

        /// <summary>
        /// Calculates the "Net Amount" of all transactions
        /// 
        /// For PermitFees, the "net amount" is the amount after all reversals have been applyed
        /// For PermitFeeReversals, the "nee amount" is 0 if it has been applyed to a Permit fee.
        /// </summary>
        private static Dictionary<Guid, decimal> CalculateNetTransactionAmounts(ICollection<Transaction> transactions)
        {
            // Apply permit fee reversals to the netAmount for all permit fee transactions
            // and set the netAmount of the reversals to 0m

            Dictionary<Guid, decimal> netAmounts = transactions
                .ToDictionary(t => t.Id, t => t.Amount);

            IEnumerable<Transaction> permitFeeReversals = transactions
                .Where(t => t.Class == TransactionClass.PermitFeeReversal &&
                            t.RelatedTransactionId != null &&
                            netAmounts.ContainsKey(t.RelatedTransactionId.Value))
                .ToList();

            foreach (Transaction permitFeeReversal in permitFeeReversals)
            {
                Debug.Assert(permitFeeReversal.RelatedTransactionId != null,
                    "permitFeeReversal.RelatedTransactionId != null");

                Guid permitFeeId = permitFeeReversal.RelatedTransactionId.Value;

                netAmounts[permitFeeId] += permitFeeReversal.Amount;

                netAmounts[permitFeeReversal.Id] = 0m;
            }

            return netAmounts;
        }
    }
}
