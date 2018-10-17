using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Freedom.Extensions
{
    public static class SystemDataExtensions
    {
        #region IDbCommand Extensions

        public static IDbDataParameter CreateParameter(this IDbCommand command, string name, object value)
        {
            return CreateParameter(command, ParameterDirection.Input, name, value);
        }

        public static IDbDataParameter CreateParameter(this IDbCommand command, ParameterDirection direction, string name)
        {
            IDbDataParameter parameter = command.CreateParameter();

            parameter.Direction = direction;
            parameter.ParameterName = name;

            command.Parameters.Add(parameter);

            return parameter;
        }

        public static IDbDataParameter CreateParameter(this IDbCommand command, ParameterDirection direction, string name, object value)
        {
            IDbDataParameter parameter = command.CreateParameter();

            parameter.Direction = direction;
            parameter.ParameterName = name;
            parameter.Value = value;

            if (value is DateTime)
            {
                parameter.DbType = DbType.DateTime2;
            }
            else if (value is byte[])
            {
                parameter.DbType = DbType.Binary;
            }
            else if (value is DataTable)
            {
                if (!(command is SqlCommand))
                    throw new ArgumentException(
                        "Table Valued Parameters are only implemented on SqlServer connections.", nameof(value));

                SqlParameter sqlParameter = (SqlParameter) parameter;
                sqlParameter.SqlDbType = SqlDbType.Structured;
                sqlParameter.TypeName = ((DataTable) value).TableName;
            }

            command.Parameters.Add(parameter);

            return parameter;
        }

        public static string DebugCommandSummary(this IDbCommand command)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(command.CommandText);

            if (command.Parameters.Count <= 0)
                return sb.ToString();

            sb.AppendLine("Parameters:");

            foreach (DbParameter parameter in command.Parameters)
            {
                sb.AppendLine(parameter.Value == null
                    ? $"\t{parameter.ParameterName} is null"
                    : $"\t{parameter.ParameterName} = {parameter.Value}");
            }

            return sb.ToString();
        }

        #endregion

        #region IDataReader Extensions

        public static string GetNullableString(this IDataReader dataReader, int i)
        {
            return !dataReader.IsDBNull(i) ? dataReader.GetString(i) : null;
        }

        public static string GetNullableString(this IDataReader dataReader, int i, int maxLength)
        {
            if (dataReader.IsDBNull(i))
                return null;

            string result = dataReader.GetString(i);

            if (result.Length == 0)
                return null;

            if (result.Length > maxLength)
                return result.Substring(0, maxLength);

            return result;
        }

        #endregion

        #region IDbConnection Extensions

        public static string EscapeIdentifier(this IDbConnection connection, string identifier)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            if (connection.GetType().Name == "SqlConnection" || connection.GetType().Name == "SqlCeConnection")
                 return "[" + (identifier ?? string.Empty).Replace("]", "]]") + "]";

            return identifier;
        }

        #region CreateCommand

        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static DbCommand CreateCommand(this DbConnection connection, string commandText)
        {
            DbCommand command = connection.CreateCommand();

            command.CommandType = CommandType.Text;
            command.CommandText = commandText;

            return command;
        }

        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static IDbCommand CreateCommand(this IDbConnection connection, string commandText)
        {
            IDbCommand command = connection.CreateCommand();

            command.CommandType = CommandType.Text;
            command.CommandText = commandText;

            return command;
        }

        #endregion

        #region ExecuteNonQuery

        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static int ExecuteNonQuery(this IDbConnection connection, string sql)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentNullException(nameof(sql));

            if (connection.State != ConnectionState.Open)
                connection.Open();

            using (IDbCommand command = connection.CreateCommand(sql))
                return command.ExecuteNonQuery();
        }

        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static Task<int> ExecuteNonQueryAsync(this DbConnection connection, string sql)
        {
            return connection.ExecuteNonQueryAsync(sql, CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static async Task<int> ExecuteNonQueryAsync(this DbConnection connection, string sql,
            CancellationToken cancellationToken)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentNullException(nameof(sql));

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);

            using (DbCommand command = connection.CreateCommand(sql))
                return await command.ExecuteNonQueryAsync(cancellationToken);
        }

        #endregion

        #region ExecuteScalar

        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static object ExecuteScalar(this IDbConnection connection, string sql)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentNullException(nameof(sql));

            if (connection.State != ConnectionState.Open)
                connection.Open();

            using (IDbCommand command = connection.CreateCommand(sql))
                return command.ExecuteScalar();
        }

        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static Task<object> ExecuteScalarAsync(this DbConnection connection, string sql)
        {
            return connection.ExecuteScalarAsync(sql, CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static async Task<object> ExecuteScalarAsync(this DbConnection connection, string sql, CancellationToken cancellationToken)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentNullException(nameof(sql));

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);

            DbCommand command = connection.CreateCommand(sql);

            return await command.ExecuteScalarAsync(cancellationToken);
        }

        #endregion

        #region LookupValue

        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static object LookupValue(this IDbConnection connection, string tableName, string fieldName, string primaryKeyFieldName, object primaryKey)
        {
            return connection.LookupValue(tableName, fieldName, new[] { new KeyValuePair<string, object>(primaryKeyFieldName, primaryKey) });
        }

        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static object LookupValue(this IDbConnection connection, string tableName, string fieldName, IEnumerable<KeyValuePair<string, object>> keys)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            if (fieldName == null)
                throw new ArgumentNullException(nameof(fieldName));

            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("tableName can't be empty.", nameof(tableName));

            if (string.IsNullOrWhiteSpace(fieldName))
                throw new ArgumentException("fieldName can't be empty.", nameof(fieldName));

            if (connection.State == ConnectionState.Closed)
                connection.Open();

            IDbCommand command = connection.CreateCommand();

            command.CommandType = CommandType.Text;

            StringBuilder sql = new StringBuilder();

            sql.AppendFormat("SELECT {0} FROM {1}",
                             connection.EscapeIdentifier(fieldName),
                             connection.EscapeIdentifier(tableName));

            if (keys != null)
            {
                int i = 0;

                foreach (KeyValuePair<string, object> keyValuePair in keys)
                {
                    sql.Append(i == 0 ? " WHERE " : " AND ");
                    sql.AppendFormat("{0} = @p{1}", connection.EscapeIdentifier(keyValuePair.Key), i);

                    IDbDataParameter parameter = command.CreateParameter();
                    parameter.Direction = ParameterDirection.Input;
                    parameter.ParameterName = $"p{i}";
                    parameter.Value = keyValuePair.Value;
                    command.Parameters.Add(parameter);

                    i++;
                }
            }

            command.CommandText = sql.ToString();

            return command.ExecuteScalar();
        }

        public static Task<object> LookupValueAsync(this DbConnection connection, string tableName, string fieldName, string primaryKeyFieldName, object primaryKey)
        {
            KeyValuePair<string, object>[] keys = { new KeyValuePair<string, object>(primaryKeyFieldName, primaryKey) };

            return connection.LookupValueAsync(tableName, fieldName, keys, CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static Task<object> LookupValueAsync(this DbConnection connection, string tableName, string fieldName, string primaryKeyFieldName, object primaryKey, CancellationToken cancellationToken)
        {
            KeyValuePair<string, object>[] keys = { new KeyValuePair<string, object>(primaryKeyFieldName, primaryKey) };

            return connection.LookupValueAsync(tableName, fieldName, keys, cancellationToken);
        }

        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static Task<object> LookupValueAsync(this DbConnection connection, string tableName, string fieldName,
            IEnumerable<KeyValuePair<string, object>> keys)
        {
            return connection.LookupValueAsync(tableName, fieldName, keys, CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static async Task<object> LookupValueAsync(this DbConnection connection, string tableName, string fieldName, IEnumerable<KeyValuePair<string, object>> keys, CancellationToken cancellationToken)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            if (fieldName == null)
                throw new ArgumentNullException(nameof(fieldName));

            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("tableName can't be empty.", nameof(tableName));

            if (string.IsNullOrWhiteSpace(fieldName))
                throw new ArgumentException("fieldName can't be empty.", nameof(fieldName));

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);

            DbCommand command = connection.CreateCommand();

            command.CommandType = CommandType.Text;

            StringBuilder sql = new StringBuilder();

            sql.AppendFormat("SELECT {0} FROM {1}",
                             connection.EscapeIdentifier(fieldName),
                             connection.EscapeIdentifier(tableName));

            if (keys != null)
            {
                int i = 0;

                foreach (KeyValuePair<string, object> keyValuePair in keys)
                {
                    sql.Append(i == 0 ? " WHERE " : " AND ");
                    sql.AppendFormat("{0} = @p{1}", connection.EscapeIdentifier(keyValuePair.Key), i);

                    DbParameter parameter = command.CreateParameter();
                    parameter.Direction = ParameterDirection.Input;
                    parameter.ParameterName = $"p{i}";
                    parameter.Value = keyValuePair.Value;
                    command.Parameters.Add(parameter);

                    i++;
                }
            }

            command.CommandText = sql.ToString();

            return await command.ExecuteScalarAsync(cancellationToken);
        }

        #endregion

        #region InsertRecord

        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static void InsertRecord(this IDbConnection connection, string tableName, IEnumerable<KeyValuePair<string, object>> values)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            if (values == null)
                throw new ArgumentNullException(nameof(values));

            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("tableName can't be empty.", nameof(tableName));

            if (connection.State == ConnectionState.Closed)
                connection.Open();

            using (IDbCommand command = connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;

                StringBuilder fields = new StringBuilder();
                StringBuilder paramNames = new StringBuilder();

                using (IEnumerator<KeyValuePair<string, object>> enumerator = values.GetEnumerator())
                {
                    int i = 0;

                    if (!enumerator.MoveNext())
                        throw new ArgumentException("values can't be empty.", nameof(values));

                    while (true)
                    {
                        fields.Append(connection.EscapeIdentifier(enumerator.Current.Key));

                        IDbDataParameter parameter = command.CreateParameter();
                        parameter.Direction = ParameterDirection.Input;
                        parameter.ParameterName = $"p{i++}";
                        parameter.Value = enumerator.Current.Value ?? DBNull.Value;
                        command.Parameters.Add(parameter);

                        paramNames.Append('@');
                        paramNames.Append(parameter.ParameterName);

                        if (!enumerator.MoveNext()) break;

                        fields.Append(", ");
                        paramNames.Append(", ");
                    }
                }

                command.CommandText =
                    $"insert into {connection.EscapeIdentifier(tableName)} ({fields}) values ({paramNames})";

                command.ExecuteNonQuery();
            }
        }

        public static Task<int> InsertRecordAsync(this DbConnection connection, string tableName,
            IEnumerable<KeyValuePair<string, object>> values)
        {
            return connection.InsertRecordAsync(tableName, values, CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static async Task<int> InsertRecordAsync(this DbConnection connection, string tableName,
            IEnumerable<KeyValuePair<string, object>> values, CancellationToken cancellationToken)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            if (values == null)
                throw new ArgumentNullException(nameof(values));

            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("tableName can't be empty.", nameof(tableName));

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);

            DbCommand command = connection.CreateCommand();
            command.CommandType = CommandType.Text;

            StringBuilder fields = new StringBuilder();
            StringBuilder paramNames = new StringBuilder();

            using (IEnumerator<KeyValuePair<string, object>> enumerator = values.GetEnumerator())
            {
                int i = 0;

                if (!enumerator.MoveNext())
                    throw new ArgumentException("values can't be empty.", nameof(values));

                while (true)
                {
                    fields.Append(connection.EscapeIdentifier(enumerator.Current.Key));

                    DbParameter parameter = command.CreateParameter();
                    parameter.Direction = ParameterDirection.Input;
                    parameter.ParameterName = $"p{i++}";
                    parameter.Value = enumerator.Current.Value ?? DBNull.Value;
                    command.Parameters.Add(parameter);

                    paramNames.Append('@');
                    paramNames.Append(parameter.ParameterName);

                    if (!enumerator.MoveNext()) break;

                    fields.Append(", ");
                    paramNames.Append(", ");
                }
            }

            command.CommandText =
                $"insert into {connection.EscapeIdentifier(tableName)} ({fields}) values ({paramNames})";

            return await command.ExecuteNonQueryAsync(cancellationToken);
        }

        #endregion

        #region UpdateRecords

        public static int UpdateRecords(this IDbConnection connection, string tableName, string fieldName, object value, string primaryKeyFieldName, object primaryKey)
        {
            return UpdateRecords(connection, tableName,
                                 new[] {new KeyValuePair<string, object>(fieldName, value)},
                                 new[] {new KeyValuePair<string, object>(primaryKeyFieldName, primaryKey)});
        }

        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static int UpdateRecords(this IDbConnection connection, string tableName, IEnumerable<KeyValuePair<string, object>> values, IEnumerable<KeyValuePair<string, object>> keys)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            if (values == null)
                throw new ArgumentNullException(nameof(values));

            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("tableName can't be empty.", nameof(tableName));

            if (connection.State == ConnectionState.Closed)
                connection.Open();
            
            IDbCommand command = connection.CreateCommand();

            command.CommandType = CommandType.Text;

            StringBuilder sql = new StringBuilder();

            int i = 0;

            sql.Append($"UPDATE {connection.EscapeIdentifier(tableName)}");

            foreach (KeyValuePair<string, object> keyValuePair in values)
            {
                sql.Append(i == 0 ? " SET " : ", ");
                sql.Append($"{connection.EscapeIdentifier(keyValuePair.Key)} = @v{i}");

                IDbDataParameter parameter = command.CreateParameter();
                parameter.Direction = ParameterDirection.Input;
                parameter.ParameterName = $"v{i}";
                parameter.Value = keyValuePair.Value ?? DBNull.Value;
                command.Parameters.Add(parameter);

                i++;
            }

            if (i == 0)
                throw new ArgumentException("values can't be empty.", nameof(values));

            if (keys != null)
            {
                i = 0;

                foreach (KeyValuePair<string, object> keyValuePair in keys)
                {
                    if (keyValuePair.Value == null)
                        throw new ArgumentNullException(nameof(keys), "Primary key values can't be null");

                    sql.Append(i == 0 ? " WHERE " : " AND ");
                    sql.Append($"{connection.EscapeIdentifier(keyValuePair.Key)} = @p{i}");

                    IDbDataParameter parameter = command.CreateParameter();
                    parameter.Direction = ParameterDirection.Input;
                    parameter.ParameterName = $"p{i}";
                    parameter.Value = keyValuePair.Value;
                    command.Parameters.Add(parameter);

                    i++;
                }
            }

            command.CommandText = sql.ToString();

            return command.ExecuteNonQuery();
        }

        public static Task<int> UpdateRecordsAsync(this DbConnection connection, string tableName,
            IEnumerable<KeyValuePair<string, object>> values, IEnumerable<KeyValuePair<string, object>> keys)
        {
            return connection.UpdateRecordsAsync(tableName, values, keys, CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static async Task<int> UpdateRecordsAsync(this DbConnection connection, string tableName, IEnumerable<KeyValuePair<string, object>> values, IEnumerable<KeyValuePair<string, object>> keys, CancellationToken cancellationToken)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            if (values == null)
                throw new ArgumentNullException(nameof(values));

            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("tableName can't be empty.", nameof(tableName));

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);

            DbCommand command = connection.CreateCommand();

            command.CommandType = CommandType.Text;

            StringBuilder sql = new StringBuilder();

            int i = 0;

            sql.Append($"UPDATE {connection.EscapeIdentifier(tableName)}");

            foreach (KeyValuePair<string, object> keyValuePair in values)
            {
                sql.Append(i == 0 ? " SET " : ", ");
                sql.Append($"{connection.EscapeIdentifier(keyValuePair.Key)} = @v{i}");

                DbParameter parameter = command.CreateParameter();
                parameter.Direction = ParameterDirection.Input;
                parameter.ParameterName = $"v{i}";
                parameter.Value = keyValuePair.Value ?? DBNull.Value;
                command.Parameters.Add(parameter);

                i++;
            }

            if (i == 0)
                throw new ArgumentException("values can't be empty.", nameof(values));

            if (keys != null)
            {
                i = 0;

                foreach (KeyValuePair<string, object> keyValuePair in keys)
                {
                    if (keyValuePair.Value == null)
                        throw new ArgumentNullException(nameof(keys), "Primary key values can't be null");

                    sql.Append(i == 0 ? " WHERE " : " AND ");
                    sql.Append($"{connection.EscapeIdentifier(keyValuePair.Key)} = @p{i}");

                    DbParameter parameter = command.CreateParameter();
                    parameter.Direction = ParameterDirection.Input;
                    parameter.ParameterName = $"p{i}";
                    parameter.Value = keyValuePair.Value;
                    command.Parameters.Add(parameter);

                    i++;
                }
            }

            command.CommandText = sql.ToString();

            return await command.ExecuteNonQueryAsync(cancellationToken);
        }

        #endregion

        #region DeleteRecord

        public static int DeleteRecord(this IDbConnection connection, string tableName, string primaryKeyFieldName,
            object primaryKey)
        {
            return DeleteRecords(connection, tableName,
                new[] {new KeyValuePair<string, object>(primaryKeyFieldName, primaryKey)});
        }

        public static async Task<int> DeleteRecordAsync(this DbConnection connection, string tableName,
            string primaryKeyFieldName, object primaryKey)
        {
            return await DeleteRecordsAsync(connection, tableName,
                new[] {new KeyValuePair<string, object>(primaryKeyFieldName, primaryKey)}, CancellationToken.None);
        }

        public static async Task<int> DeleteRecordAsync(this DbConnection connection, string tableName,
            string primaryKeyFieldName, object primaryKey, CancellationToken cancellationToken)
        {
            return await DeleteRecordsAsync(connection, tableName,
                new[] {new KeyValuePair<string, object>(primaryKeyFieldName, primaryKey)}, cancellationToken);
        }

        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static int DeleteRecords(this IDbConnection connection, string tableName, IEnumerable<KeyValuePair<string, object>> keys)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            if (keys == null)
                throw new ArgumentNullException(nameof(keys));

            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("tableName can't be empty.", nameof(tableName));

            if (connection.State == ConnectionState.Closed)
                connection.Open();

            IDbCommand command = connection.CreateCommand();

            command.CommandType = CommandType.Text;

            StringBuilder sql = new StringBuilder();

            sql.Append("DELETE ");
            sql.Append(connection.EscapeIdentifier(tableName));

            int i = 0;

            foreach (KeyValuePair<string, object> keyValuePair in keys)
            {
                sql.Append(i == 0 ? " WHERE " : " AND ");
                sql.AppendFormat("{0} = @p{1}", connection.EscapeIdentifier(keyValuePair.Key), i);

                IDbDataParameter parameter = command.CreateParameter();
                parameter.Direction = ParameterDirection.Input;
                parameter.ParameterName = $"p{i}";
                parameter.Value = keyValuePair.Value;
                command.Parameters.Add(parameter);

                i++;
            }

            if (i == 0)
            {
                throw new ArgumentException("keys cannot be an empty collection.", nameof(keys));
            }

            command.CommandText = sql.ToString();

            return command.ExecuteNonQuery();
        }

        public static Task<int> DeleteRecordsAsync(this DbConnection connection, string tableName, IEnumerable<KeyValuePair<string, object>> keys)
        {
            return connection.DeleteRecordsAsync(tableName, keys, CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public static async Task<int> DeleteRecordsAsync(this DbConnection connection, string tableName, IEnumerable<KeyValuePair<string, object>> keys, CancellationToken cancellationToken)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            if (keys == null)
                throw new ArgumentNullException(nameof(keys));

            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("tableName can't be empty.", nameof(tableName));

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);

            DbCommand command = connection.CreateCommand();

            command.CommandType = CommandType.Text;

            StringBuilder sql = new StringBuilder();

            sql.Append("DELETE ");
            sql.Append(connection.EscapeIdentifier(tableName));

            int i = 0;

            foreach (KeyValuePair<string, object> keyValuePair in keys)
            {
                sql.Append(i == 0 ? " WHERE " : " AND ");
                sql.AppendFormat("{0} = @p{1}", connection.EscapeIdentifier(keyValuePair.Key), i);

                DbParameter parameter = command.CreateParameter();
                parameter.Direction = ParameterDirection.Input;
                parameter.ParameterName = $"p{i}";
                parameter.Value = keyValuePair.Value;
                command.Parameters.Add(parameter);

                i++;
            }

            if (i == 0)
            {
                throw new ArgumentException("keys cannot be an empty collection.", nameof(keys));
            }

            command.CommandText = sql.ToString();

            return await command.ExecuteNonQueryAsync(cancellationToken);
        }
        
        #endregion

        #endregion
    }
}
