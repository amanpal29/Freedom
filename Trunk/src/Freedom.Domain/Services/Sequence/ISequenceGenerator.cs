using System.Threading.Tasks;

namespace Freedom.Domain.Services.Sequence
{
    public interface ISequenceGenerator
    {
        Task<long> GetNextValueAsync(string key);
    }
}
