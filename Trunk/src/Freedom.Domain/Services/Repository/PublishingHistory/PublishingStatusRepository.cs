using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Freedom.Domain.Model;
using Freedom.Domain.Services.Time;
using Freedom.Extensions;

namespace Freedom.Domain.Services.Repository.PublishingHistory
{
    public class PublishingStatusRepository
    {
        #region SQL Command String Constants

        private const string TableName = "_DisclosurePublishingStatus";

        private const string CreateTableSql =
            "CREATE TABLE [_DisclosurePublishingStatus] (" +
            "[Id] uniqueidentifier NOT NULL, " +
            "[EntityTypeName] nvarchar(255) NOT NULL, " +
            "[LastPublishDateTime] datetime2 NOT NULL, " +
            "[IsVisible] bit NOT NULL, " +
            "CONSTRAINT [PK_DisclosurePublishingStatus] PRIMARY KEY ([Id], [EntityTypeName]))";

        private const string DeleteTableSql =
            "DELETE [_DisclosurePublishingStatus]";

        #endregion

        #region Fields

        private readonly IDbConnection _dbConnection;
        private readonly ITimeService _timeService = IoC.TryGet<ITimeService>() ?? new LocalTimeService();

        public PublishingStatusRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;

            if (_dbConnection.State == ConnectionState.Closed)
                _dbConnection.Open();
        }

        #endregion

        #region Initialize Method

        public static async Task InitializeAsync(DbConnection dbConnection, CancellationToken cancellationToken)
        {
            await dbConnection.ExecuteNonQueryAsync(CreateTableSql, cancellationToken);
        }

        #endregion

        #region Methods
        
        public void Update(string entityTypeName, Guid id, bool visible)
        {
            if (string.IsNullOrWhiteSpace(entityTypeName))
                throw new ArgumentNullException(nameof(entityTypeName));

            if (id == Guid.Empty)
                throw new ArgumentException("id can not be an empty Guid.", nameof(id));

            Dictionary<string, object> keys = new Dictionary<string, object>();
            keys.Add("Id", id);
            keys.Add("EntityTypeName", entityTypeName);

            Dictionary<string, object> values = new Dictionary<string, object>();
            values.Add("LastPublishDateTime", _timeService.UtcNow);
            values.Add("IsVisible", visible);

            if (_dbConnection.UpdateRecords(TableName, values, keys) == 0)
                _dbConnection.InsertRecord(TableName, keys.Union(values));
        }

        public void Update(EntityBase entity, bool visible)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            Type entityType = entity.GetType().BaseType;
            
            if (entityType != null)
                Update(entityType.Name, entity.Id, visible);
        }

        public void Clear()
        {
            using (IDbCommand command = _dbConnection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = DeleteTableSql;
                command.ExecuteNonQuery();
            }
        }

        #endregion

    }
}
