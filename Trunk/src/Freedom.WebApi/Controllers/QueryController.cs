using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Freedom.Domain.Exceptions;
using Freedom.WebApi.Filters;
using Freedom.Domain.Model;
using Freedom.WebApi.Models;
using Freedom.WebApi.Results;
using Freedom.Domain.Services.Query;
using Freedom.Domain.Services.Repository;
using Freedom.Domain.Services.Security;
using Freedom.Constraints;
using Constraint = Freedom.Constraints.Constraint;

namespace Freedom.WebApi.Controllers
{
    [FreedomAuthentication]
    public class QueryController : ApiController
    {
        private readonly IClientContext _clientContext = IoC.Get<IClientContext>();
        private readonly IQueryDataProviderCollection _dataProviders = IoC.Get<IQueryDataProviderCollection>();

        private string ClientIp => _clientContext?.GetClientAddress(Request);

        [HttpGet]
        [Route("query/{entityTypeName}/{id}")]
        public async Task<IHttpActionResult> GetAsync(string entityTypeName, Guid id,
            CancellationToken cancellationToken)
        {
            IQueryDataProvider dataProvider = _dataProviders.GetProviderForType(entityTypeName);

            if (dataProvider == null)
                return NotFound();

            Type entityType = _dataProviders.GetEntityTypeByName(entityTypeName);

            Constraint constraint = new EqualConstraint(nameof(EntityBase.Id), id);

            try
            {
                IEnumerable<Entity> results =
                    await dataProvider.GetEntitiesAsync(entityType, null, null, constraint, cancellationToken);

                Entity result = results.FirstOrDefault();

                if (result == null)
                    return NotFound();
                
                return new OkNegotiatedContentResult(entityType, result, this);
            }            
            catch (InsufficientPermissionException)
            {
                return new ForbiddenResult(Request);
            }
        }

        [HttpPost]
        [Route("query/{entityTypeName}")]
        public async Task<IHttpActionResult> QueryAsync(string entityTypeName, [FromBody] QueryRequest queryRequest, CancellationToken cancellationToken)
        {
            IQueryDataProvider dataProvider = _dataProviders.GetProviderForType(entityTypeName);

            if (dataProvider == null)
                return NotFound();

            Type entityType = _dataProviders.GetEntityTypeByName(entityTypeName);
            
            try
            {
                IEnumerable<Entity> result = await dataProvider.GetEntitiesAsync(entityType,
                    queryRequest.PointInTime, queryRequest.ResolutionGraph, queryRequest.Constraint, cancellationToken);

                EntityCollection entities = new EntityCollection(result);
                
                return Ok(entities);
            }            
            catch (InsufficientPermissionException)
            {
                return new ForbiddenResult(Request);
            }
        }

        [HttpPost]
        [Route("query/aggregate/{entityTypeName}")]
        public async Task<IHttpActionResult> QueryAsync(string entityTypeName, [FromUri] AggregateRequest aggregateRequest, [FromBody] Constraint constraint, CancellationToken cancellationToken)
        {
            IQueryDataProvider dataProvider = _dataProviders.GetProviderForType(entityTypeName);

            if (dataProvider == null)
                return NotFound();

            Type entityType = _dataProviders.GetEntityTypeByName(entityTypeName);

            if (string.IsNullOrEmpty(aggregateRequest?.GroupBy))
                return BadRequest("You must specify a groupBy field");

            AggregateFunction aggregateFunction;
            
            if (!Enum.TryParse(aggregateRequest.Function, true, out aggregateFunction))
                aggregateFunction = AggregateFunction.Count;

            try
            {
                GroupCollection result = await dataProvider.GetGroupsAsync(entityType,
                    aggregateRequest.GroupBy, aggregateFunction, aggregateRequest.Value, constraint, cancellationToken);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("query/count/{entityTypeName}")]
        public async Task<IHttpActionResult> CountAsync(string entityTypeName, [FromBody] Constraint constraint, CancellationToken cancellationToken)
        {
            IQueryDataProvider dataProvider = _dataProviders.GetProviderForType(entityTypeName);

            if (dataProvider == null)
                return NotFound();

            try
            {
                Type entityType = _dataProviders.GetEntityTypeByName(entityTypeName);

                return Ok(await dataProvider.GetCountAsync(entityType, constraint, cancellationToken));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("query/search/{entityTypeName}")]
        public async Task<IHttpActionResult> CountAsync(string entityTypeName, string searchTerm, [FromBody] Constraint constraint, CancellationToken cancellationToken)
        {
            IQueryDataProvider dataProvider = _dataProviders.GetProviderForType(entityTypeName);

            if (dataProvider == null)
                return NotFound();

            try
            {
                Type entityType = _dataProviders.GetEntityTypeByName(entityTypeName);

                IEnumerable<Entity> result = await dataProvider.SearchAsync(entityType, searchTerm, constraint, cancellationToken);

                return Ok(new EntityCollection(result));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpPost]
        [Route("query/lastmodifieddate")]
        public async Task<IHttpActionResult> GetLastModifiedDateAsync(IEnumerable<string> lookupTables)
        {
            const string lastModifiedSql = "SELECT '{0}' [Table], MAX([AuditStartDateTime]) [LastModified] FROM [{0}_V]";

            LastModifiedDateDictionary result = new LastModifiedDateDictionary();

            if (lookupTables == null)
                return BadRequest("Lookup table list cannot be empty.");

            using (DbConnection connection = IoC.Get<DbConnection>())
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                IEnumerable<string> queries = lookupTables
                    .Where(t => _dataProviders.GetProviderForType(t) is ModelEntityDataProvider)
                    .Select(t => string.Format(lastModifiedSql, t));

                DbCommand command = connection.CreateCommand();

                command.CommandType = CommandType.Text;

                command.CommandText = string.Join(" UNION ", queries);

                if (string.IsNullOrEmpty(command.CommandText))
                    return Ok(result);

                using (DbDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        string tableName = reader.GetString(0);

                        if (await reader.IsDBNullAsync(1)) continue;

                        result.Add(tableName, reader.GetDateTime(1));
                    }
                }
            }

            return Ok(result);
        }
    }
}
