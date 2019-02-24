using System.Threading;
using System.Threading.Tasks;

namespace Freedom.Domain.Services.DatabaseBuilder
{
    public interface IDatabaseBuilderService
    {
        string ProviderConnectionString { get; set; }

        FreedomDatabaseType FreedomDatabaseType { get; set; }
        
        bool DatabaseExists { get; }
        Task CreateDatabaseAsync(CancellationToken cancellationToken, string accessToken);
        Task DeleteDatabaseAsync(CancellationToken cancellationToken, string accessToken);
    }

    public static class DatabaseBuilderServiceExtensions
    {
        public static Task CreateDatabaseAsync(this IDatabaseBuilderService databaseBuilderService, string accessToken)
            => databaseBuilderService.CreateDatabaseAsync(CancellationToken.None, accessToken);

        public static Task DeleteDatabaseAsync(this IDatabaseBuilderService databaseBuilderService, string accessToken)
            => databaseBuilderService.DeleteDatabaseAsync(CancellationToken.None, accessToken);
    }
}