using System;
using System.Reflection;
using log4net;

namespace Freedom.Domain.Infrastructure.ExceptionHandling
{
    public class LoggingExceptionHandler : IExceptionHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ExceptionHandledStatus Handle(Exception exception, ExceptionContext context)
        {
            Log.Warn("An exception has occurred", exception);

            return ExceptionHandledStatus.Unhandled;
        }
    }
}
