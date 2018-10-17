using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hedgehog.CommandModel;
using Hedgehog.Infrastructure;
using Hedgehog.Services.Command;
using Hedgerow;

namespace Hedgehog.Model.Behaviors
{
    public class ApplyAutoNumberingOnCommitBehavior : IDbContextCommittingBehavior
    {
        public Task OnCommittingAsync(DbContext dbContext, CommandBase command, CommandExecutionContext commandContext,
            CancellationToken cancellationToken)
        {
            if (dbContext == null)
                throw new ArgumentNullException(nameof(dbContext));

            if (!(dbContext is HedgehogLocalContext))
                throw new ArgumentException("dbContext must be a HedgehogLocalContext", nameof(dbContext));

            if (commandContext.IsOffline)
                return Task.FromResult(0);

            IAutoNumberGenerator autoNumberGenerator = IoC.TryGet<IAutoNumberGenerator>();

            if (autoNumberGenerator == null)
                return Task.FromResult(0);

            return ApplyAutoNumberingOnCommitAsync(autoNumberGenerator, (HedgehogLocalContext)dbContext,
                commandContext.CurrentUserId, cancellationToken);
        }

        protected async Task ApplyAutoNumberingOnCommitAsync(IAutoNumberGenerator autoNumberGenerator, HedgehogLocalContext localContext, Guid currentUserId, 
            CancellationToken cancellationToken)
        {
            ApplicationSettingsBase applicationSettings = await localContext.GetApplicationSettingsAsync();
            User currentUser = await localContext.User.FindAsync(cancellationToken, currentUserId);

            await EnsureFacilityInfoIsLoadedAsync(localContext, cancellationToken);
            await EnsureNexusIssueInfoIsLoadedAsync(localContext, cancellationToken);

            IEnumerable<DbEntityEntry<NumberedRoot>> addedNumberedRoots = localContext.ChangeTracker
                .Entries<NumberedRoot>().Where(e => e.State == EntityState.Added);

            foreach (DbEntityEntry<NumberedRoot> numberedRootEntry in addedNumberedRoots)
                await autoNumberGenerator.ApplyAutoNumberingOnCommitAsync(localContext, applicationSettings, numberedRootEntry.Entity, currentUser);
        }

        private static async Task EnsureFacilityInfoIsLoadedAsync(HedgehogLocalContext db,
            CancellationToken cancellationToken)
        {
            List<Guid> facilityIds =
                db.ChangeTracker.Entries<Inspection>().Where(e => e.State == EntityState.Added).Select(e => e.Entity.FacilityId).ToList();

            if (facilityIds.Any())
               await db.Facility.Include(f => f.FacilityType.FacilityCategory.ProgramArea).Where(f => facilityIds.Contains(f.Id)).LoadAsync(cancellationToken);

            List<Guid> facilityTypeIds =
                db.ChangeTracker.Entries<Facility>()
                    .Where(e => e.State == EntityState.Added)
                    .Select(e => e.Entity.FacilityTypeId)
                    .ToList();

            if (facilityTypeIds.Any())
                await db.FacilityType.Include(ft => ft.FacilityCategory.ProgramArea).Where(ft => facilityTypeIds.Contains(ft.Id)).LoadAsync(cancellationToken);
        }

        private static async Task EnsureNexusIssueInfoIsLoadedAsync(HedgehogLocalContext db,
            CancellationToken cancellationToken)
        {
            List<Guid> personInvolvedNexusIssueIds =
                db.ChangeTracker.Entries<PersonInvolved>()
                    .Where(e => e.State == EntityState.Added)
                    .Select(e => e.Entity.NexusIssueId)
                    .ToList();

            if (personInvolvedNexusIssueIds.Any())
                await db.NexusIssue.Include(ni => ni.IssueType).Include(ni => ni.ProgramArea).Where(n => personInvolvedNexusIssueIds.Contains(n.Id)).LoadAsync(cancellationToken);
            
            List<Guid> animalInvolvedNexusIssueIds =
                db.ChangeTracker.Entries<AnimalInvolved>()
                    .Where(e => e.State == EntityState.Added)
                    .Select(e => e.Entity.NexusIssueId)
                    .ToList();

            if (animalInvolvedNexusIssueIds.Any())
                await db.NexusIssue.Include(ni => ni.IssueType).Include(ni => ni.ProgramArea).Where(ni => animalInvolvedNexusIssueIds.Contains(ni.Id)).LoadAsync(cancellationToken);

            List<Guid> nexusIssueTypeIds =
                db.ChangeTracker.Entries<NexusIssue>()
                    .Where(e => e.State == EntityState.Added && e.Entity.IssueTypeId != null)
                    .Select(e => e.Entity.IssueTypeId.Value)
                    .ToList();

            if (nexusIssueTypeIds.Any())
                await db.NexusIssueType.Include(nit => nit.ProgramArea).Where(nit => nexusIssueTypeIds.Contains(nit.Id)).LoadAsync(cancellationToken);

        }
    }
}