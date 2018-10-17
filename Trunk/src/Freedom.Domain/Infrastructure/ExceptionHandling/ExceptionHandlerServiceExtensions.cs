using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Freedom.Domain.Infrastructure.ExceptionHandling
{
    public static class ExceptionHandlerServiceExtensions
    {
        public static ExceptionHandledStatus HandleException(this IExceptionHandlerService service, Exception exception, ExceptionContextFlags contextFlags = ExceptionContextFlags.None)
        {
            return service.HandleException(exception, new ExceptionContext(new StackTrace().GetFrame(1).GetMethod(), contextFlags));
        }

        public static void RetryAction(this IExceptionHandlerService service, Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            bool keepTrying = true;

            while (keepTrying)
            {
                keepTrying = false;

                try
                {
                    action();
                }
                catch (Exception exception)
                {
                    ExceptionHandledStatus result = service.HandleException(exception,
                        ExceptionContextFlags.CanRetry | ExceptionContextFlags.CanCancel);

                    switch (result)
                    {
                        case ExceptionHandledStatus.Handled:
                            break;

                        case ExceptionHandledStatus.Cancel:
                            throw new CanceledException("The operation was canceled by the current user.", exception);

                        case  ExceptionHandledStatus.Retry:
                            keepTrying = true;
                            break;

                        default:
                            throw;
                    }
                }
            }

        }

        public static async Task<TResult> RetryAsync<TResult>(this IExceptionHandlerService service, Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            bool keepTrying = true;

            while (keepTrying && !cancellationToken.IsCancellationRequested)
            {
                keepTrying = false;

                try
                {
                    return await action(cancellationToken);
                }
                catch (Exception exception)
                {
                    if (cancellationToken.IsCancellationRequested)
                        throw new CanceledException("The operation was canceled by the current user.", exception);

                    ExceptionHandledStatus result = service.HandleException(exception,
                        ExceptionContextFlags.CanRetry | ExceptionContextFlags.CanCancel);

                    switch (result)
                    {
                        case ExceptionHandledStatus.Handled:
                            break;

                        case ExceptionHandledStatus.Cancel:
                            throw new CanceledException("The operation was canceled by the current user.", exception);

                        case ExceptionHandledStatus.Retry:
                            keepTrying = true;
                            break;

                        default:
                            throw;
                    }
                }
            }

            return default(TResult);
        }
    }
}