using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Reflection;
using System.Text;
using Freedom.Extensions;
using log4net;

namespace Freedom.EntityFramework
{
    public class LoggingInterceptor : IDbCommandInterceptor 
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static void LogCommand<TResult>(DbCommand command, DbCommandInterceptionContext<TResult> interceptionContext)
        {
            if (!Log.IsDebugEnabled) return;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(interceptionContext.IsAsync
                ? "Executing the following command asynchronously:"
                : "Executing the following command synchronously:");

            sb.Append(command.DebugCommandSummary());

            Log.Debug(sb);
        }

        private static void LogIfError<TResult>(DbCommand command, DbCommandInterceptionContext<TResult> interceptionContext)
        {
            if (interceptionContext.Exception != null)
                Log.Error($"The following SQL command failed:\n{command.CommandText}\n", interceptionContext.Exception);
        } 

        #region Implementation of IDbCommandInterceptor

        public void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            LogCommand(command, interceptionContext);
        }

        public void NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            LogIfError(command, interceptionContext);
        }

        public void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            LogCommand(command, interceptionContext);
        }

        public void ReaderExecuted(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            LogIfError(command, interceptionContext);
        }

        public void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            LogCommand(command, interceptionContext);
        }

        public void ScalarExecuted(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            LogIfError(command, interceptionContext);
        }

        #endregion
    }
}
