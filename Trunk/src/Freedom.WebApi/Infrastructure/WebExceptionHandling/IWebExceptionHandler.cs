using System.Web.Http.ExceptionHandling;

namespace Freedom.WebApi.Infrastructure.WebExceptionHandling
{
    public interface IWebExceptionHandler : IExceptionHandler
    {
        bool ShouldHandle(ExceptionHandlerContext context);
    }
}
