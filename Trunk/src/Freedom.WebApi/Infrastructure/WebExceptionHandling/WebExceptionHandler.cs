using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Results;
using log4net;

namespace Freedom.WebApi.Infrastructure.WebExceptionHandling
{
    /// <summary>
    /// Generic exception handler that will return a 500 - Internal Server Error
    /// </summary>
    public class WebExceptionHandler : ExceptionHandler, IWebExceptionHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public virtual HttpStatusCode HttpStatusCode => HttpStatusCode.InternalServerError;

        public virtual HttpError CreateHttpError(Exception exception, bool includeErrorDetail)
        {
            return new HttpError(exception, includeErrorDetail);
        }

        public override void Handle(ExceptionHandlerContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            ExceptionContext exceptionContext = context.ExceptionContext;

            if (exceptionContext == null)
                throw new ArgumentException("ExceptionContext must not be null.", nameof(context));

            HttpRequestMessage request = exceptionContext.Request;

            if (request == null)
                throw new ArgumentException("ExceptionContext.Request must not be null.", nameof(context));

            Guid uniqueId = Guid.NewGuid();

            Log.Info($"Creating error response for exception with the uniqueId {uniqueId}.", exceptionContext.Exception);

            HttpError httpError = CreateHttpError(exceptionContext.Exception, request.ShouldIncludeErrorDetail());

            httpError.Add(nameof(uniqueId), uniqueId.ToString());

            HttpResponseMessage response = request.CreateErrorResponse(HttpStatusCode, httpError);

            context.Result = new ResponseMessageResult(response);
        }
    }
}
