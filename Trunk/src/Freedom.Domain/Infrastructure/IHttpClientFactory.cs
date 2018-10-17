using System.Net.Http;

namespace Freedom.Domain.Infrastructure
{
    public interface IHttpClientFactory
    {
        HttpClient Create();
    }
}
