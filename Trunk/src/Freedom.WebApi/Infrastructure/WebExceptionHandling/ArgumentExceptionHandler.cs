using System;
using System.Net;
using System.Web.Http.ExceptionHandling;

namespace Freedom.WebApi.Infrastructure.WebExceptionHandling
{
    public class ArgumentExceptionHandler : WebExceptionHandler
    {
        public override HttpStatusCode HttpStatusCode => HttpStatusCode.BadRequest;

        public override bool ShouldHandle(ExceptionHandlerContext context)
        {
            return context.Exception is ArgumentException;
        }
    }
}
