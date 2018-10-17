using System.Threading.Tasks;

namespace Freedom.WebApi.Infrastructure
{
    public interface IFreedomUserCache
    {
        FreedomUser GetUserDataFromCache(string name);

        Task<FreedomUser> GetFreedomUserFromDatabaseAsync(string name);
    }

    public static class FreedomUserCacheExtensions
    {
        public static async Task<FreedomUser> GetFreedomUserAsync(this IFreedomUserCache cache, string name)
        {
            return cache.GetUserDataFromCache(name) ??
                   await cache.GetFreedomUserFromDatabaseAsync(name);
        }
    }
}