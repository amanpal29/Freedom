using System;
using System.Reflection;
using log4net;

namespace Freedom.Domain.Infrastructure.ExceptionHandling
{
    public class ExceptionSliencer : IExceptionHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Type _exceptionType;
        private readonly bool _firstChance;

        public ExceptionSliencer(Type exceptionType, bool firstChance = false)
        {
            _exceptionType = exceptionType;
            _firstChance = firstChance;
        }

        private bool CanSlienceException(Exception exception)
        {
            while (exception != null)
            {
                if (_exceptionType.IsInstanceOfType(exception))
                    return true;

                exception = exception.InnerException;
            }

            return false;
        }

        public ExceptionHandledStatus Handle(Exception exception, ExceptionContext context)
        {
            ExceptionHandledStatus status = ExceptionHandledStatus.Unhandled;

            if ((_firstChance || context.IsLastChance) && CanSlienceException(exception))
            {
                Log.Info("An exception was slienced.", exception);
                status = ExceptionHandledStatus.Handled;
            }
            
            return status;
        }

    }
}
