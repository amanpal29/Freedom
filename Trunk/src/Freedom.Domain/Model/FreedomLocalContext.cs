using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Threading.Tasks;
using Freedom.Domain.Infrastructure;
using Freedom.Domain.CommandModel;
using Freedom.Domain.Services.Command;
using Freedom.Domain.Model.Behaviors;

namespace Freedom.Domain.Model
{
    public partial class FreedomLocalContext
    {
        private ApplicationSettingsBase _applicationSettings;

        public async Task<ApplicationSettingsBase> GetApplicationSettingsAsync()
        {
            if (_applicationSettings != null)
                return _applicationSettings;

            FreedomContextApplicationSettings result = new FreedomContextApplicationSettings(this);

            await result.InitializeAsync();

            _applicationSettings = result;

            return _applicationSettings;
        }

        public Task OnCommittingAsync(CommandBase command, CommandExecutionContext commandContext)
        {
            return OnCommittingAsync(command, commandContext, CancellationToken.None);
        }

        public virtual async Task OnCommittingAsync(CommandBase command, CommandExecutionContext commandContext,
            CancellationToken cancellationToken)
        {
            foreach (IDbContextCommittingBehavior behavior in IoC.GetAll<IDbContextCommittingBehavior>())
            {
                cancellationToken.ThrowIfCancellationRequested();

                await behavior.OnCommittingAsync(this, command, commandContext, cancellationToken);
            }
        }

        public void UpdateAuditProperties(Guid currentUserId, DateTime modifyDateTime)
        {
            IEnumerable<DbEntityEntry> addedEntries = ChangeTracker.Entries()
                .Where(e => e.Entity is AggregateRoot && e.State.HasFlag(EntityState.Added));

            foreach (DbEntityEntry entry in addedEntries)
            {
                AggregateRoot aggregateRoot = (AggregateRoot)entry.Entity;
                aggregateRoot.CreatedBy = null;
                aggregateRoot.CreatedById = currentUserId;
                aggregateRoot.CreatedDateTime = modifyDateTime;
                aggregateRoot.ModifiedBy = null;
                aggregateRoot.ModifiedById = currentUserId;
                aggregateRoot.ModifiedDateTime = modifyDateTime;
            }

            IEnumerable<DbEntityEntry> changedEntries = ChangeTracker.Entries()
                .Where(e => e.Entity is AggregateRoot && e.State.HasFlag(EntityState.Modified));

            foreach (DbEntityEntry entry in changedEntries)
            {
                AggregateRoot aggregateRoot = (AggregateRoot)entry.Entity;

                aggregateRoot.ModifiedById = currentUserId;
                aggregateRoot.ModifiedDateTime = modifyDateTime;
            }
        }
    }
}
