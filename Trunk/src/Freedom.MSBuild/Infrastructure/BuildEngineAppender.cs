using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Repository.Hierarchy;
using Microsoft.Build.Framework;

namespace Freedom.MSBuild.Infrastructure
{
    public class BuildEngineAppender : AppenderSkeleton
    {
        public static void Register(IBuildEngine buildEngine)
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
            hierarchy.ResetConfiguration();
            hierarchy.Root.AddAppender(new BuildEngineAppender(buildEngine));
            hierarchy.Configured = true;
        }

        private readonly IBuildEngine _buildEngine;

        public BuildEngineAppender(IBuildEngine buildEngine)
        {
            _buildEngine = buildEngine;

            Name = GetType().Name;
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            int lineNumber;

            string message = loggingEvent.MessageObject?.ToString();

            if (!int.TryParse(loggingEvent.LocationInformation.LineNumber, out lineNumber))
                lineNumber = -1;

            if (loggingEvent.Level >= Level.Error)
            {
                BuildErrorEventArgs args = new BuildErrorEventArgs(
                    string.Empty, string.Empty,
                    loggingEvent.LocationInformation.FileName,
                    lineNumber, 0, 0, 0,
                    message, string.Empty,
                    loggingEvent.LoggerName,
                    loggingEvent.TimeStamp,
                    loggingEvent.MessageObject);
                    
                _buildEngine.LogErrorEvent(args);
            }
            else if (loggingEvent.Level >= Level.Warn)
            {
                BuildWarningEventArgs args = new BuildWarningEventArgs(
                    string.Empty, string.Empty,
                    loggingEvent.LocationInformation.FileName,
                    lineNumber, 0, 0, 0,
                    message, string.Empty,
                    loggingEvent.LoggerName,
                    loggingEvent.TimeStamp,
                    loggingEvent.MessageObject);

                _buildEngine.LogWarningEvent(args);
            }
            else
            {
                BuildMessageEventArgs args = new BuildMessageEventArgs(
                    message,
                    string.Empty,
                    loggingEvent.LoggerName,
                    loggingEvent.Level >= Level.Info
                        ? MessageImportance.Normal
                        : MessageImportance.Low,
                    loggingEvent.TimeStamp,
                    loggingEvent.MessageObject);

                _buildEngine.LogMessageEvent(args);
            }
        }
    }
}
