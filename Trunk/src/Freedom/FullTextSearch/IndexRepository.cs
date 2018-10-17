using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Freedom.Extensions;

namespace Freedom.FullTextSearch
{
    public class IndexRepository : IDisposable
    {
        private const string CreateTableSql =
            "CREATE TABLE _FullTextIndex (" +
            " Source int NOT NULL," +
            " Keyword nvarchar(32) NOT NULL," +
            " Id uniqueidentifier NOT NULL," +
            " Weight int NOT NULL," +
            " CONSTRAINT PK__FullTextIndex PRIMARY KEY ( source, keyword, id ))";

        public const string CreateIndexSql =
            "CREATE UNIQUE INDEX UNQ__FullTextIndex on _FullTextIndex( source, id, keyword ) include( [weight] )";

        private const string CreateKeyValueTableTypeSql =
            "CREATE TYPE tvp_KeywordWeight AS TABLE (" +
            " Keyword nvarchar(32) NOT NULL," +
            " Weight int NOT NULL)";

        private const string CreateUpdateStoredProcedureSql =
            "CREATE PROCEDURE UpdateFullTextIndex " +
            "	(@source INT, @id UNIQUEIDENTIFIER, @pairs dbo.tvp_KeywordWeight readonly) " +
            "AS " +
            "BEGIN " +
            "    SET NOCOUNT ON; " +
            " " +
            "    DELETE FROM _FullTextIndex " +
            "    WHERE Source = @source " +
            "      AND Id = @id " +
            "      AND Keyword NOT IN (SELECT Keyword FROM @pairs); " +
            " " +
            "    UPDATE t " +
            "    SET    t.weight = s.weight " +
            "    FROM   _FullTextIndex t " +
            "    JOIN   @pairs s ON t.Source = @source AND t.Id = @id AND t.Keyword = s.Keyword " +
            "    WHERE  t.Weight <> s.Weight; " +
            " " +
            "    INSERT INTO _FullTextIndex (Source, Keyword, Id, Weight) " +
            "    SELECT @source, p.keyword, @id ,p.weight " +
            "    FROM   @pairs p " +
            "    WHERE  p.keyword NOT IN (SELECT Keyword FROM _FullTextIndex WHERE source = @source AND id = @id); " +
            "END";

        private const string ClearIndexSql =
            "DELETE FROM _FullTextIndex WHERE Source = @source";

        private const string ClearEntrySql =
            "DELETE FROM _FullTextIndex WHERE Source = @source AND Id = @id";

        private const string GrantExecutePermission =
            "GRANT EXECUTE ON TYPE::dbo.tvp_KeywordWeight TO public;\n" +
            "GRANT EXECUTE ON OBJECT::dbo.UpdateFullTextIndex TO public;";

        private DbConnection _connection;

        public static async Task InitializeAsync(DbConnection dbConnection, CancellationToken cancellationToken)
        {
            DbCommand command = dbConnection.CreateCommand();
            command.CommandType = CommandType.Text;

            command.CommandText = CreateTableSql;
            await command.ExecuteNonQueryAsync(cancellationToken);

            command.CommandText = CreateIndexSql;
            await command.ExecuteNonQueryAsync(cancellationToken);

            command.CommandText = CreateKeyValueTableTypeSql;
            await command.ExecuteNonQueryAsync(cancellationToken);

            command.CommandText = CreateUpdateStoredProcedureSql;
            await command.ExecuteNonQueryAsync(cancellationToken);

            command.CommandText = GrantExecutePermission;
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        public IndexRepository(DbConnection connection)
        {
            _connection = connection;

            if (_connection.State != ConnectionState.Open)
                _connection.Open();
        }

        private static DataTable BuildKeyworkWeightTable(IEnumerable<KeyValuePair<string, int>> values)
        {
            DataTable table = new DataTable("tvp_KeywordWeight");

            table.Columns.Add("Keyword", typeof(string));
            table.Columns.Add("Weight", typeof(int));

            foreach (KeyValuePair<string, int> pair in values)
                table.Rows.Add(pair.Key, pair.Value);

            return table;
        }

        public Task<int> ClearAsync(int source, CancellationToken cancellationToken)
        {
            DbCommand command = _connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = ClearIndexSql;
            command.CreateParameter("source", source);
            return command.ExecuteNonQueryAsync(cancellationToken);
        }

        public Task<int> ClearAsync(int source, Guid id, CancellationToken cancellationToken)
        {
            DbCommand command = _connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = ClearEntrySql;
            command.CreateParameter("source", source);
            command.CreateParameter("id", id);
            return command.ExecuteNonQueryAsync(cancellationToken);
        }

        public Task<int> UpdateAsync(int source, Guid id, IEnumerable<KeyValuePair<string, int>> values, CancellationToken cancellationToken)
        {
            DbCommand command = _connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "UpdateFullTextIndex";
            command.CreateParameter("source", source);
            command.CreateParameter("id", id);
            command.CreateParameter("pairs", BuildKeyworkWeightTable(values));
            return command.ExecuteNonQueryAsync(cancellationToken);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            _connection?.Dispose();
            _connection = null;
        }
    }
}
