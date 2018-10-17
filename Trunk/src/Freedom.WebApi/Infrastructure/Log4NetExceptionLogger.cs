using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Web.Http.Controllers;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Routing;
using log4net;

namespace Freedom.WebApi.Infrastructure
{
    public class Log4NetExceptionLogger : ExceptionLogger
    {
        private static readonly ILog GlobalLogger = LogManager.GetLogger(typeof(Log4NetExceptionLogger));

        public override void Log(ExceptionLoggerContext context)
        {
            Type controllerType = context.ExceptionContext?.ControllerContext?.Controller?.GetType();

            ILog logger = controllerType != null ? LogManager.GetLogger(controllerType) : GlobalLogger;

            logger.Error(BuildMessage(context), context.Exception);
        }

        private static string BuildMessage(ExceptionLoggerContext context)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("An unhandled exception occurred.");

            DocumentRequest(stringBuilder, context.Request);
            DocumentAction(stringBuilder, context.ExceptionContext?.ActionContext);
            DocumentController(stringBuilder, context.ExceptionContext?.ControllerContext);

            return stringBuilder.ToString();
        }

        private static void DocumentRequest(StringBuilder msg, HttpRequestMessage request)
        {
            if (request == null) return;

            msg.AppendLine("Request Information:");

            msg.AppendLine($"\t {request.Method} {request.RequestUri.AbsoluteUri} {request.Version}");

            foreach (KeyValuePair<string, IEnumerable<string>> header in request.Headers)
            {
                switch (header.Key)
                {
                    case "Authorization":
                        msg.AppendLine($"\t {header.Key} (redacted)");
                        break;
                    default:
                        msg.AppendLine($"\t {header.Key} {string.Join(",", header.Value)}");
                        break;
                }
            }

        }

        private static void DocumentController(StringBuilder msg, HttpControllerContext controller)
        {
            if (controller == null) return;

            DocumentRouteData(msg, controller.RouteData);
        }

        private static void DocumentRouteData(StringBuilder msg, IHttpRouteData routeData)
        {
            if (routeData == null) return;

            msg.AppendLine("Route Data:");

            foreach (KeyValuePair<string, object> keyValuePair in routeData.Values)
                msg.AppendLine($"\t {keyValuePair.Key} == {keyValuePair.Value}");
        }

        private static void DocumentAction(StringBuilder msg, HttpActionContext action)
        {
            if (action == null) return;

            msg.AppendLine("Action Information:");

            msg.AppendLine($"\t ActionName: {action.ActionDescriptor?.ActionName}");
            msg.AppendLine($"\t ReturnType: {action.ActionDescriptor?.ReturnType}");
        }
    }
}