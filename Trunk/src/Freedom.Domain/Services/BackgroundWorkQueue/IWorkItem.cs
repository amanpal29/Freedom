using System.Threading;
using System.Threading.Tasks;

namespace Freedom.Domain.Services.BackgroundWorkQueue
{
    public interface IWorkItem
    {
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}