using System;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Web.Http.Tracing;
using log4net;

namespace Freedom.WebApi.Infrastructure
{
    public sealed class Log4NetTraceWriter : ITraceWriter
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public bool IsEnabled(string category, TraceLevel level)
        {
            return true;
        }

        public void Trace(HttpRequestMessage request, string category, TraceLevel level, Action<TraceRecord> traceAction)
        {
            if (level == TraceLevel.Off)
                return;

            TraceRecord record = new TraceRecord(request, category, level);

            traceAction?.Invoke(record);

            switch (level)
            {
                case TraceLevel.Debug:
                    Log.Debug(BuildMessage(record));
                    break;

                case TraceLevel.Info:
                    Log.Info(BuildMessage(record));
                    break;

                case TraceLevel.Warn:
                    Log.Warn(BuildMessage(record));
                    break;

                case TraceLevel.Error:
                    Log.Error(BuildMessage(record));
                    break;

                case TraceLevel.Fatal:
                    Log.Fatal(BuildMessage(record));
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }

        private static string BuildMessage(TraceRecord record)
        {
            StringBuilder message = new StringBuilder();

            if (record.Request != null)
            {
                if (record.Request.Method != null)
                    message.Append(" ").Append(record.Request.Method.Method);

                if (record.Request.RequestUri != null)
                    message.Append(" ").Append(record.Request.RequestUri.AbsoluteUri);
            }

            if (!string.IsNullOrWhiteSpace(record.Category))
                message.Append(" ").Append(record.Category);

            if (!string.IsNullOrWhiteSpace(record.Operator))
                message.Append(" ").Append(record.Operator).Append(" ").Append(record.Operation);

            if (!string.IsNullOrWhiteSpace(record.Message))
                message.Append(" ").Append(record.Message);

            if (!string.IsNullOrEmpty(record.Exception?.GetBaseException().Message))
                message.Append(" ").AppendLine(record.Exception.GetBaseException().Message);

            return message.ToString();
        }
    }
}