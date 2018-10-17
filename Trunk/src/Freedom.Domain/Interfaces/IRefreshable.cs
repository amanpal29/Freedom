using System.Threading.Tasks;

namespace Freedom.Domain.Interfaces
{
    public interface IRefreshable
    {
        Task RefreshAsync();
    }
}
