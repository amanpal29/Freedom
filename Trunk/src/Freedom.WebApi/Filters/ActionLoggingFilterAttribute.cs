using System;
using System.Diagnostics;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using log4net;

namespace Freedom.WebApi.Filters
{
    public class ActionLoggingFilterAttribute : ActionFilterAttribute, IActionFilter
    {
        #region Implementation of IFilter

        public async Task<HttpResponseMessage> ExecuteActionFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            IIdentity identity = actionContext?.RequestContext?.Principal?.Identity;

            string authorization = identity != null ? $"{identity.AuthenticationType}: {identity.Name}" : "Anonymous";

            Type controllerType = actionContext?.ControllerContext?.Controller?.GetType();

            if (controllerType == null)
                return await continuation();

            ILog logger = LogManager.GetLogger(controllerType);

            Stopwatch stopwatch = Stopwatch.StartNew();

            HttpResponseMessage result = await continuation();

            logger.Info($"{actionContext.Request.Method} {actionContext.Request.RequestUri} [{authorization}] ({stopwatch.ElapsedMilliseconds} ms)");

            return result;
        }

        #endregion
    }
}