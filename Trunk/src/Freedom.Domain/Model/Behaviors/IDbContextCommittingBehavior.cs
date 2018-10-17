using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;
using Freedom.Domain.CommandModel;
using Freedom.Domain.Services.Command;

namespace Freedom.Domain.Model.Behaviors
{
    public interface IDbContextCommittingBehavior
    {
        Task OnCommittingAsync(DbContext dbContext, CommandBase command, CommandExecutionContext commandContext, CancellationToken cancellationToken);
    }
}
