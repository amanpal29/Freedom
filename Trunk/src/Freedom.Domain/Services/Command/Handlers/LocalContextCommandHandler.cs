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
    public abstract class LocalContextCommandHandler<TCommand> : CommandHandlerBase<TCommand>
        where TCommand : CommandBase
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override async Task<CommandResult> Handle(CommandBase commandBase, CommandExecutionContext context)
        {
            TCommand command = (TCommand) commandBase;

            try
            {
                using (FreedomLocalContext db = IoC.Get<FreedomLocalContext>())
                {
                    if (!await Handle(db, command, context))
                        return new CommandResult(false);

                    db.ChangeTracker.DetectChanges();
                        
                    db.UpdateAuditProperties(context.CurrentUserId, context.CurrentDateTime);

                    await db.OnCommittingAsync(command, context);

                    await db.SaveChangesAsync();

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
                    sb.Append($"{failure.Entry.Entity.GetType()} failed validation.\n");

                    foreach (DbValidationError error in failure.ValidationErrors)
                    {
                        sb.Append($"- {error.PropertyName} : {error.ErrorMessage}");
                        sb.AppendLine();
                    }
                }

                Log.Warn(sb);

                throw;
            }
        }

        protected abstract Task<bool> Handle(FreedomLocalContext db, TCommand command, CommandExecutionContext context);
    }
}