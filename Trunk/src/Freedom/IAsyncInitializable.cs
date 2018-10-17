using System.Threading.Tasks;

namespace Freedom
{
    public interface IAsyncInitializable
    {
        bool IsInitialized { get; set; }

        Task InitializeCoreAsync();
    }

    public static class AsyncInitializableExtensions
    {
        public static async Task InitializeAsync(this IAsyncInitializable asyncInitializable)
        {
            await asyncInitializable.InitializeCoreAsync();

            asyncInitializable.IsInitialized = true;
        }
    }
}
