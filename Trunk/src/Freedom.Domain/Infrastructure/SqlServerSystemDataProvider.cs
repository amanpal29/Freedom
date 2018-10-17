using System;
using System.Data;
using System.Data.SqlClient;
using Freedom.SystemData;

namespace Freedom.Domain.Infrastructure
{
    public class SqlServerSystemDataProvider : ISystemDataProvider
    {
        private readonly string _sectionName;
        private readonly Func<SqlConnection> _sqlConnectionFactory;

        public SqlServerSystemDataProvider(string sectionName, Func<SqlConnection> sqlConnectionFactory)
        {
            _sectionName = sectionName;
            _sqlConnectionFactory = sqlConnectionFactory;
        }

        private SqlConnection GetSqlConnection()
        {
            try
            {
                SqlConnection connection =  _sqlConnectionFactory();
                
                if (connection != null && connection.State != ConnectionState.Open)
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
            SqlConnection connection = GetSqlConnection();

            if (connection == null) return;

            using (connection)
            {
                SystemDataSection section = collection[_sectionName];

                section.TryAdd("Database Server", () => GetFriendlySqlServerVersion(connection.ServerVersion));
                section.TryAdd("Database Name", () => connection.Database);
                section.TryAdd("Datasource", () => connection.DataSource);
            }
        }

        private static string GetFriendlySqlServerVersion(string serverVersion)
        {
            string result = "SQL Server";

            string[] versionParts = serverVersion.Split('.');

            int major = int.Parse(versionParts[0]);
            int minor = int.Parse(versionParts[1]);

            switch (major)
            {
                case 7:
                    result = "SQL Server 7.0";
                    break;

                case 8:
                    result = "SQL Server 2000";
                    break;

                case 9:
                    result = "SQL Server 2005";
                    break;

                case 10:
                    switch (minor)
                    {
                        case 0:
                            result = "SQL Server 2008";
                            break;

                        case 50:
                            result = "SQL Server 2008 R2";
                            break;
                    }
                    break;

                case 11:
                    result = "SQL Server 2012";
                    break;

                case 12:
                    result = "SQL Server 2014";
                    break;
            }

            return $"{result} ({serverVersion})";
        }
    }
}
