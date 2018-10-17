using System.Threading.Tasks;
using Freedom.SystemData;

namespace Freedom.Domain.Services.Status
{
    public interface IStatusService
    {
        Task<VersionData> GetVersionDataAsync();

        Task<SystemDataCollection> GetSystemDataAsync();
    }
}
