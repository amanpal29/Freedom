using System.Data.Entity.Validation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Freedom.Domain.CommandModel;
using Freedom.Domain.Model;
using Freedom.Domain.Services.BackgroundWorkQueue;
using log4net;

namespace Freedom.Domain.Services.Command.Handlers
{
    public abstract class RepositoryCommandHandler<TCommand> : CommandHandlerBase<TCommand>
        where TCommand : CommandBase
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override async Task<CommandResult> Handle(CommandBase commandBase, CommandExecutionContext context)
        {
            TCommand command = (TCommand) commandBase;

            try
            {
                FreedomLocalContext db = IoC.Get<FreedomLocalContext>();

                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;

                using (FreedomRepository repository = new FreedomRepository(db, context.CurrentUserId, context.CurrentDateTime))
                {
                    if (!await Handle(repository, command, context))
                        return new CommandResult(false);

                    db.ChangeTracker.DetectChanges();

                    await db.OnCommittingAsync(command, context);

                    await repository.SaveChangesAsync();

                    await OnCommitAsync(repository, command, context);

                    if (context.OnCommitWorkItems.Count > 0)
                    {
                        IBackgroundWorkQueue backgroundWorkQueue = IoC.TryGet<IBackgroundWorkQueue>();

                        if (backgroundWorkQueue != null)
                            backgroundWorkQueue.QueueItems(context.OnCommitWorkItems);
                        else
                            Log.Warn($"{context.OnCommitWorkItems} post-commit jobs will not be completed because there is no Background Work Queue available");
                    }

                    return new CommandResult(true);
                }
            }
            catch (DbEntityValidationException ex)
            {
                StringBuilder sb = new StringBuilder();

                foreach (DbEntityValidationResult failure in ex.EntityValidationErrors)
                {
                    sb.AppendLine($"{failure.Entry.Entity.GetType()} failed validation.");

                    foreach (DbValidationError error in failure.ValidationErrors)
                        sb.AppendLine($"- {error.PropertyName} : {error.ErrorMessage}");
                }

                Log.Warn(sb);

                throw;
            }
        }

        protected abstract Task<bool> Handle(FreedomRepository repository, TCommand command, CommandExecutionContext context);

        protected virtual Task OnCommitAsync(FreedomRepository repository, TCommand command, CommandExecutionContext context)
        {
            return Task.FromResult(0);
        }
    }
}
