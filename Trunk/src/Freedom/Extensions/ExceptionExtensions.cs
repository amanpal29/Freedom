using System;
using System.Reflection;

namespace Freedom.Extensions
{
    public static class ExceptionExtensions
    {
        public static Exception Unwrap(this Exception exception)
        {
            Exception result = exception;

            while (true)
            {
                AggregateException aggregateException = result as AggregateException;

                if (aggregateException != null)
                {
                    result = aggregateException.InnerException;
                    continue;
                }

                TargetInvocationException targetInvocationException = result as TargetInvocationException;

                if (targetInvocationException != null)
                {
                    result = targetInvocationException.InnerException;
                    continue;
                }

                break;
            }

            return result ?? exception;
        }

        public static TException Find<TException>(this Exception exception)
            where TException : Exception
        {
            while (exception != null)
            {
                TException target = exception as TException;

                if (target != null)
                    return target;

                exception = exception.InnerException;
            }

            return null;
        }
    }
}
