using System.Data.Common;

namespace Freedom.Domain.Services.Sync
{
    public interface IOfflineConnectionFactory
    {
        bool DatabaseExists { get; set; }
        string ProviderName { get; }
        string ProviderConnectionString { get; }
        DbConnection CreateConnection();
    }
}
