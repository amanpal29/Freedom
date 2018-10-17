using System.Threading;
using System.Threading.Tasks;

namespace Freedom.Domain.Services.DatabaseBuilder
{
    public interface IDatabaseBuilderService
    {
        string ProviderConnectionString { get; set; }

        FreedomDatabaseType FreedomDatabaseType { get; set; }
        
        bool DatabaseExists { get; }
        Task CreateDatabaseAsync(CancellationToken cancellationToken);
        Task DeleteDatabaseAsync(CancellationToken cancellationToken);
    }

    public static class DatabaseBuilderServiceExtensions
    {
        public static Task CreateDatabaseAsync(this IDatabaseBuilderService databaseBuilderService)
            => databaseBuilderService.CreateDatabaseAsync(CancellationToken.None);

        public static Task DeleteDatabaseAsync(this IDatabaseBuilderService databaseBuilderService)
            => databaseBuilderService.DeleteDatabaseAsync(CancellationToken.None);
    }
}