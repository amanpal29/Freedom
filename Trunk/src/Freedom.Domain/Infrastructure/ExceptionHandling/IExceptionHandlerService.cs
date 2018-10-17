using System;

namespace Freedom.Domain.Infrastructure.ExceptionHandling
{

    /*
     * ExceptionHandlerService usage examples
     * 
     
    public void MimimalUsageExample()
    {
        try
        {
            // Some operation
        }
        catch (Exception ex)
        {
            if (IoC.Get<IExceptionHandlerService>().HandleException(ex) == ExceptionHandledStatus.Unhandled)
                throw;
        }
    }

    public void FullUsageExample()
    {
        try
        {
            // Some operation
        }
        catch (Exception ex)
        {
            ExceptionHandlerService exceptionHandlerService = IoC.Get<IExceptionHandlerService>();

            switch (exceptionHandlerService.HandleException(ex, canRetry: true, canCancel: true))
            {
                case ExceptionHandledStatus.Handled:
                    break;

                // This case branch should exist if canRetry = true
                case ExceptionHandledStatus.Retry:
                    // Try the operation again
                    break;

                // This case branch should exist if canCancel = true
                case ExceptionHandledStatus.Cancel:
                    // Rollback the operation
                    break;

                default:
                    throw;
            }
        }
    }
    */

    public interface IExceptionHandlerService
    {
        ExceptionHandledStatus HandleException(Exception exception, ExceptionContext context);
    }
}