using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;

namespace Freedom.Domain.Infrastructure.ExceptionHandling
{
    public class ExceptionHandlerService : IExceptionHandlerService
    {
        #region Fields

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Properties

        public IEnumerable<IExceptionHandler> Handlers => IoC.GetAll<IExceptionHandler>();

        #endregion

        #region Methods

        public ExceptionHandledStatus HandleException(Exception exception, ExceptionContext context)
        {
            Exception currentException = exception;
            ExceptionHandledStatus status = ExceptionHandledStatus.Unhandled;

            foreach (IExceptionHandler exceptionHandler in Handlers)
            {
                try
                {
                    status = exceptionHandler.Handle(currentException, context);
                }
                catch (Exception newException)
                {
                    currentException = newException;
                }

                if (status != ExceptionHandledStatus.Unhandled)
                    break;
            }

            if (status == ExceptionHandledStatus.Unhandled && context.IsLastChance)
            {
                // Not really much we can do, just log the error.
                Log.Error("An exception was not handled by any exception handler.", exception);

                status = ExceptionHandledStatus.Handled;
            }

            if (status == ExceptionHandledStatus.Unhandled && exception != currentException)
                throw currentException;

            return status;
        }

        #endregion
    }
}
