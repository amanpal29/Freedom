using Freedom.WebApi.Models;

namespace Freedom.WebApi.Infrastructure
{
    public interface IApplicationMetadataCache
    {
        ApplicationMetadata GetApplicationMetadata();

        string GetLocalFilePath(string fileName);
    }
}