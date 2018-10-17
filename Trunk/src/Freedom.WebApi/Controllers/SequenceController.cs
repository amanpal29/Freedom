using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using System.Web.Http;
using Freedom.Domain.Services.Sequence;
using Freedom.Extensions;

namespace Freedom.WebApi.Controllers
{
    public class SequenceController : ApiController
    {
        private const long DefaultStartValue = 1;
        private const long DefaultBlockSize = 20;

        [HttpGet]
        [Route("sequence/nextvalue/{key}")]
        public async Task<IHttpActionResult> NextValue(string key)
        {
            if (string.IsNullOrWhiteSpace(key) || key.Length > 255)
                return BadRequest("Invalid sequence key");

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
                    if (await reader.ReadAsync() && !await reader.IsDBNullAsync(0))
                    {
                        long nextValue = reader.GetInt64(0);

                        return Ok(nextValue);
                    }
                }

                // We should only get here if the sequence didn't exist.  In this case we'll create it..

                command = connection.CreateCommand();

                command.CommandType = CommandType.Text;

                command.CommandText = "INSERT INTO _Sequence (SequenceName, NextValue, BlockSize) " +
                                      "VALUES (@sequenceName, @nextValue, @blockSize)";

                command.CreateParameter("sequenceName", key);
                command.CreateParameter("nextValue", DefaultStartValue + 1L);
                command.CreateParameter("blockSize", DefaultBlockSize);

                await command.ExecuteNonQueryAsync();

                return Ok(DefaultStartValue);
            }
        }

        [HttpGet]
        [Route("sequence/nextblock/{key}")]
        public async Task<IHttpActionResult> NextBlock(string key)
        {
            if (string.IsNullOrWhiteSpace(key) || key.Length > 255)
                return BadRequest("Invalid sequence key");

            using (DbConnection connection = IoC.Get<DbConnection>())
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                DbCommand command = connection.CreateCommand();

                command.CommandType = CommandType.Text;

                command.CommandText = "UPDATE _Sequence " +
                                      "SET NextValue = NextValue + BlockSize " +
                                      "OUTPUT DELETED.NextValue, INSERTED.BlockSize " +
                                      "WHERE SequenceName = @sequenceName ";

                command.CreateParameter("sequenceName", key);

                using (DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow))
                {
                    if (await reader.ReadAsync())
                    {
                        long startValue = reader.GetInt64(0);

                        long blockSize = reader.GetInt64(1);

                        return Ok(new SequenceBlock(startValue, startValue + blockSize - 1L));
                    }
                }

                // We should only get here if the sequence didn't exist.  In this case we'll create it..

                command = connection.CreateCommand();

                command.CommandType = CommandType.Text;

                command.CommandText = "INSERT INTO _Sequence (SequenceName, NextValue, BlockSize) " +
                                      "VALUES (@sequenceName, @nextValue, @blockSize)";

                command.CreateParameter("sequenceName", key);
                command.CreateParameter("nextValue", DefaultStartValue + DefaultBlockSize);
                command.CreateParameter("blockSize", DefaultBlockSize);

                await command.ExecuteNonQueryAsync();

                return Ok(new SequenceBlock(DefaultStartValue, DefaultStartValue + DefaultBlockSize - 1L));
            }
        }
    }
}
