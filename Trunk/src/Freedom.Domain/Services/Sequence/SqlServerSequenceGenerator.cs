using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Freedom.Extensions;

namespace Freedom.Domain.Services.Sequence
{
    public class SqlServerSequenceGenerator : ISequenceGenerator
    {
        private const long DefaultBlockSize = 20;

        public async Task<long> GetNextValueAsync(string key)
        {
            using (DbConnection connection = IoC.Get<DbConnection>())
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                DbCommand command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = "UPDATE _Sequence " +
                                      "SET NextValue = NextValue + 1" +
                                      "OUTPUT DELETED.NextValue " +
                                      "WHERE SequenceName = @sequenceName ";
                command.CreateParameter("sequenceName", key);

                using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleResult))
                {
                    if (await reader.ReadAsync())
                        return reader.GetInt64(0);
                }

                // We should only get here if the sequence didn't exist.  In this case we'll create it..

                command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = "INSERT INTO _Sequence (SequenceName, NextValue, BlockSize) " +
                                      "VALUES (@sequenceName, @nextValue, @blockSize)";
                command.CreateParameter("sequenceName", key);
                command.CreateParameter("nextValue", 2);
                command.CreateParameter("blockSize", DefaultBlockSize);
                command.ExecuteNonQuery();

                return 1L;
            }
        }
    }
}