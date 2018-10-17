using System;
using System.Data;
using Freedom.Extensions;
using Freedom.SystemData;

namespace Freedom.Domain.Infrastructure
{
    public class FreedomDatabaseDataProvider : ISystemDataProvider
    {
        private readonly string _sectionName;
        private readonly Func<IDbConnection> _dbConnectionFactory;

        public FreedomDatabaseDataProvider(string sectionName, Func<IDbConnection> dbConnectionFactory)
        {
            _sectionName = sectionName;
            _dbConnectionFactory = dbConnectionFactory;
        }

        private IDbConnection GetConnection()
        {
            try
            {
                IDbConnection connection = _dbConnectionFactory();

                if (connection.State != ConnectionState.Open)
                    connection.Open();

                return connection;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void LoadData(SystemDataCollection collection)
        {
            IDbConnection connection = GetConnection();

            if (connection == null) return;            

            using (connection)
            {
                SystemDataSection section = collection[_sectionName];

                section.TryAdd("Global Id", () => GetGlobalId(connection));
            }
        }

        private static string GetGlobalId(IDbConnection dbConnection)
        {
            IDbCommand command = dbConnection.CreateCommand();
            
            command.CommandType = CommandType.Text;
            command.CommandText = "select [Value] from [ApplicationSetting] where [Key] = @key";
            command.CreateParameter("key", "GlobalId");

            string globalId = command.ExecuteScalar() as string;

            return string.IsNullOrWhiteSpace(globalId) ? "(unknown)" : globalId;
        }
    }

}
