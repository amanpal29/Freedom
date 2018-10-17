using System.Net;
using System.Web.Http.ExceptionHandling;
using Freedom.Domain.Exceptions;

namespace Freedom.WebApi.Infrastructure.WebExceptionHandling
{
    public class InsufficientPermissionExceptionHandler : WebExceptionHandler
    {
        public override HttpStatusCode HttpStatusCode => HttpStatusCode.Forbidden;

        public override bool ShouldHandle(ExceptionHandlerContext context)
        {
            return context.Exception is InsufficientPermissionException;
        }
    }
}