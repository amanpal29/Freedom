using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Hedgehog.CommandModel.Billing;
using Hedgehog.Infrastructure;
using Hedgehog.Model;
using Hedgerow;

namespace Hedgehog.Services.Command.Handlers.Billing
{
    public class AutomatedInvoiceRunCommandHandler : LocalContextCommandHandler<AutomatedInvoiceRunCommand>
    {
        public override IEnumerable<SystemPermission> RequiredPermissions
        {
            get { yield return SystemPermission.AddInvoice; }
        }

        protected override async Task<bool> Handle(HedgehogLocalContext db, AutomatedInvoiceRunCommand command, CommandExecutionContext context)
        {
            IAutoNumberGenerator autoNumberGenerator = IoC.TryGet<IAutoNumberGenerator>();

            Batch batch = new Batch();

            batch.Id = command.BatchId;
            batch.BatchType = BatchType.Invoices;
            batch.Number = $"BN{command.CommandCreatedDateTime:yyyyMMddhhmmss}";
            batch.BatchDateTime = command.CommandCreatedDateTime;
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

            List<Transaction> transactions = await db.Transaction.Include(t => t.Account)
                .Where(t => t.AccountId != null && t.InvoiceId == null && t.TransactionType.IsInvoiceable && t.Account.WorkAreaId != null && workAreaIds.Contains(t.Account.WorkAreaId.Value))
                .OrderBy(t => t.AccountId)
                .ThenBy(t => t.TransactionDateTime)
                .ToListAsync();

            Invoice currentInvoice = null;
            int invoiceLineNumber = 0;

            bool success = false;

            ApplicationSettingsBase applicationSettings = await db.GetApplicationSettingsAsync();

            foreach (Transaction transaction in transactions)
            {
                Debug.Assert(transaction.AccountId != null, "transaction.AccountId != null");

                if (currentInvoice?.AccountId != transaction.AccountId)
                {
                    success = true;

                    currentInvoice = new Invoice();
         
                    currentInvoice.AccountId = transaction.AccountId.Value;
                    currentInvoice.BatchId = command.BatchId;
                    currentInvoice.InvoiceDate = command.CommandCreatedDateTime.Date;
                    currentInvoice.Description = command.Description;

                    if (autoNumberGenerator != null)
                        await
                            autoNumberGenerator.ApplyAutoNumberingOnCommitAsync(db, applicationSettings, currentInvoice,
                                await db.User.FindAsync(context.CurrentUserId));

                    invoiceLineNumber = 1;
                    db.Invoice.Add(currentInvoice);
                }

                Debug.Assert(currentInvoice != null, "currentInvoice != null");

                transaction.InvoiceId = currentInvoice.Id;
                transaction.InvoiceLine = invoiceLineNumber++;
            }

            return success;
        }
    }
}
