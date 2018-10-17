using System;
using System.Net;
using System.Web.Http.ExceptionHandling;

namespace Freedom.WebApi.Infrastructure.WebExceptionHandling
{
    public class NotImplementedExceptionHandler : WebExceptionHandler
    {
        public override HttpStatusCode HttpStatusCode => HttpStatusCode.NotImplemented;

        public override bool ShouldHandle(ExceptionHandlerContext context)
        {
            return context.Exception is NotImplementedException || context.Exception is NotSupportedException;
        }
    }
}
