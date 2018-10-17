using System;

namespace Freedom.Domain.Infrastructure.ExceptionHandling
{
    public interface IExceptionHandler
    {
        /// <summary>
        /// Handles the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="context">The context in which the exception occurred.</param>
        /// <returns>
        /// An ExceptionHandledStatus
        /// </returns>
        ExceptionHandledStatus Handle(Exception exception, ExceptionContext context);
    }
}
