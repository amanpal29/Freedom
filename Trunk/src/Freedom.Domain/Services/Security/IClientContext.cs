using System.Net.Http;

namespace Freedom.Domain.Services.Security
{
    public interface IClientContext
    {
        string GetClientAddress(HttpRequestMessage request);
    }
}
