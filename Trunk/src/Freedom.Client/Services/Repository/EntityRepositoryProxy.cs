using Freedom.Client.Infrastructure.ExceptionHandling;
using Freedom.Constraints;
using Freedom.Domain.Model;
using Freedom.Domain.Services.Query;
using Freedom.Domain.Services.Repository;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Freedom.Client.Services.Repository
{
    public class EntityRepositoryProxy : IEntityRepository
    {
        private static readonly MediaTypeHeaderValue MediaType = new MediaTypeHeaderValue("application/xml")
        {
            CharSet = Encoding.UTF8.WebName
        };

        private static readonly MediaTypeFormatter Formatter = new XmlMediaTypeFormatter();

        private readonly IHttpClientErrorHandler _errorHandler = IoC.Get<IHttpClientErrorHandler>();

        #region Implementation of IEntityRepositoryAsync

        public Task<Entity> GetEntityAsync(string entityTypeName, DateTime? pointInTime, ResolutionGraph resolutionGraph, Constraint constraint, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Entity>> GetEntitiesAsync(string entityTypeName, DateTime? pointInTime, ResolutionGraph resolutionGraph, Constraint constraint, CancellationToken cancellationToken)
        {
            HttpClient httpClient = IoC.Get<HttpClient>();

            try
            {
                QueryRequest queryRequest = new QueryRequest(pointInTime, resolutionGraph, constraint);
                using (HttpContent content = new ObjectContent<QueryRequest>(queryRequest, Formatter, MediaType))
                using (HttpResponseMessage response = await httpClient.PostAsync($"query/{entityTypeName}", content, cancellationToken))
                {
                    await _errorHandler.HandleNonSuccessStatusCodeAsync(httpClient, response);

                    return await response.Content.ReadAsAsync<EntityCollection>(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _errorHandler.HandleException(httpClient, ex);

                throw;
            }
        }

        public async Task<IEnumerable<Entity>> SearchAsync(string entityTypeName, string searchTerm, Constraint constraint, CancellationToken cancellationToken)
        {
            HttpClient httpClient = IoC.Get<HttpClient>();

            try
            {
                string uri = $"query/search/{entityTypeName}?searchTerm={Uri.EscapeDataString(searchTerm)}";

                using (HttpContent content = new ObjectContent<Constraint>(constraint, Formatter, MediaType))
                using (HttpResponseMessage response = await httpClient.PostAsync(uri, content, cancellationToken))
                {
                    await _errorHandler.HandleNonSuccessStatusCodeAsync(httpClient, response);

                    return await response.Content.ReadAsAsync<EntityCollection>(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _errorHandler.HandleException(httpClient, ex);

                throw;
            }
        }

        public async Task<int> GetCountAsync(string entityTypeName, Constraint constraint, CancellationToken cancellationToken)
        {
            HttpClient httpClient = IoC.Get<HttpClient>();

            try
            {
                string uri = $"query/count/{entityTypeName}";

                using (HttpContent content = new ObjectContent<Constraint>(constraint, Formatter, MediaType))
                using (HttpResponseMessage response = await httpClient.PostAsync(uri, content, cancellationToken))
                {
                    await _errorHandler.HandleNonSuccessStatusCodeAsync(httpClient, response);

                    return await response.Content.ReadAsAsync<int>(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _errorHandler.HandleException(httpClient, ex);

                throw;
            }
        }

        public async Task<GroupCollection> GetGroupsAsync(string entityTypeName, string keyField, AggregateFunction function, string valueField, Constraint constraint, CancellationToken cancellationToken)
        {
            HttpClient httpClient = IoC.Get<HttpClient>();

            try
            {
                string uri = $"query/aggregate/{entityTypeName}?groupBy={keyField}";

                if (function != AggregateFunction.Invalid && function != AggregateFunction.Count)
                    uri += $"&function={function}&value={valueField}";

                using (HttpContent content = new ObjectContent<Constraint>(constraint, Formatter, MediaType))
                using (HttpResponseMessage response = await httpClient.PostAsync(uri, content, cancellationToken))
                {
                    await _errorHandler.HandleNonSuccessStatusCodeAsync(httpClient, response);

                    return await response.Content.ReadAsAsync<GroupCollection>(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _errorHandler.HandleException(httpClient, ex);

                throw;
            }
        }

        #endregion
    }
}
