using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hedgehog.CommandModel;
using Hedgehog.DigestModel;
using Hedgehog.Services.BackgroundWorkQueue;
using Hedgehog.Services.Command;
using Hedgerow.FullTextSearch;

namespace Hedgehog.Model.Behaviors
{
    public class ReindexFullTextSearchFieldsBehavior : IDbContextCommittingBehavior
    {
        public Task OnCommittingAsync(DbContext dbContext, CommandBase command, CommandExecutionContext commandContext,
            CancellationToken cancellationToken)
        {
            if (dbContext == null)
                throw new ArgumentNullException(nameof(dbContext));

            IndexKeySetCollection keySets = new IndexKeySetCollection();

            keySets.AddChanged<Account, AccountDigest>(dbContext);
            keySets.AddChanged<Transaction, AccountDigest>(dbContext, t => t.AccountId);

            keySets.AddChanged<AnimalInvolved, AnimalInvolvedDigest>(dbContext);

            keySets.AddChanged<Batch, BatchDigest>(dbContext);
            keySets.AddChanged<Permit, BatchDigest>(dbContext, t => t.BatchId);
            keySets.AddChanged<Transaction, BatchDigest>(dbContext, t => t.BatchId);
            keySets.AddChanged<Invoice, BatchDigest>(dbContext, t => t.BatchId);

            keySets.AddChanged<Contact, ContactDigest>(dbContext);

            keySets.AddChanged<CourseSession, CourseSessionDigest>(dbContext);

            keySets.AddChanged<Exemption, ExemptionDigest>(dbContext);

            keySets.AddChanged<Facility, FacilityDigest>(dbContext);
            keySets.AddChanged<Site, FacilityDigest>(dbContext, s => s.Facilities.Select(f =>  f.Id));

            keySets.AddChanged<FileReview, FileReviewDigest>(dbContext);

            keySets.AddChanged<Infraction, InfractionDigest>(dbContext);

            keySets.AddChanged<Inspection, InspectionDigest>(dbContext);

            keySets.AddChanged<Invoice, InvoiceDigest>(dbContext);

            keySets.AddChanged<NexusIssue, NexusDigest>(dbContext);
            keySets.AddChanged<Activity, NexusDigest>(dbContext, a => a.NexusIssueId);
            keySets.AddChanged<AnimalInvolved, NexusDigest>(dbContext, a => a.NexusIssueId);
            keySets.AddChanged<PersonInvolved, NexusDigest>(dbContext, a => a.NexusIssueId);

            keySets.AddChanged<Permit, PermitDigest>(dbContext);

            keySets.AddChanged<PersonInvolved, PersonInvolvedDigest>(dbContext);

            keySets.AddChanged<Report, ReportDigest>(dbContext);

            keySets.AddChanged<RiskAssessment, RiskAssessmentDigest>(dbContext);

            keySets.AddChanged<Sample, SampleDigest>(dbContext);

            keySets.AddChanged<User, ServiceProviderDigest>(dbContext);

            keySets.AddChanged<Timesheet, TimesheetDigest>(dbContext);
            keySets.AddChanged<TimeEntry, TimeEntryDigest>(dbContext);
            keySets.AddChanged<TimeEntry, TimesheetDigest>(dbContext, te => te.TimesheetId);

            keySets.AddChanged<Transaction, TransactionDigest>(dbContext);

            keySets.AddChanged<VaccineInventoryChange, VaccineDigest>(dbContext);

            if (keySets.Count == 0)
                return Task.FromResult(0);

            commandContext.OnCommitWorkItems.Add(new ReindexWorkItem(keySets));

            return Task.FromResult(0);
        }
    }

    internal static class IndexKeyListExtensions
    {
        public static void AddChanged<TEntity, TDigest>(this IndexKeySetCollection keySets, DbContext context)
            where TEntity : EntityBase
            where TDigest : Digest
        {
            keySets.AddChanged<TEntity, TDigest>(context, x => x.Id);
        }
    }
}
