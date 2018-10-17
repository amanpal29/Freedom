using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Routing;
using System.Web.Http.Validation;
using Freedom.WebApi.Filters;
using Freedom.WebApi.Formatters;
using Freedom.WebApi.Infrastructure;
using Freedom.WebApi.Infrastructure.WebExceptionHandling;
using Newtonsoft.Json;

namespace Freedom.WebApi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute("Root", string.Empty, new {controller = "status", action = "Get" });

            config.Routes.MapHttpRoute("DefaultApiWithAction", "{controller}/{action}/{id}",
                new { id = RouteParameter.Optional });

            config.Routes.MapHttpRoute("DefaultApiGet", "{controller}", new { action = "Get" },
                new { httpMethod = new HttpMethodConstraint(HttpMethod.Get) });

            config.Routes.MapHttpRoute("DefaultApiPost", "{controller}", new { action = "Post" },
                new { httpMethod = new HttpMethodConstraint(HttpMethod.Post) });

            // Authenticate Basic
            config.Filters.Add(new FreedomBasicAuthenticationAttribute());

            // Logs all requests
            config.Filters.Add(new ActionLoggingFilterAttribute());

            // Logs all exceptions
            config.Services.Add(typeof(IExceptionLogger), new Log4NetExceptionLogger());

            // Global exception handler
            CompositeExceptionHandler compositeExceptionHandler = new CompositeExceptionHandler(config.Services.GetExceptionHandler());
            compositeExceptionHandler.Add(new ArgumentExceptionHandler());
            compositeExceptionHandler.Add(new InsufficientPermissionExceptionHandler());
            compositeExceptionHandler.Add(new NotImplementedExceptionHandler());
            compositeExceptionHandler.Add(new SqlConstraintViolationExceptionHandler());
            compositeExceptionHandler.Add(new WebExceptionHandler());
            config.Services.Replace(typeof(IExceptionHandler), compositeExceptionHandler);

            // Disables validations of models
            config.Services.Clear(typeof(ModelValidatorProvider));

            // Configure the JsonSerializer
            config.Formatters.JsonFormatter.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind;
            config.Formatters.JsonFormatter.SerializerSettings.DateParseHandling = DateParseHandling.None;
            config.Formatters.JsonFormatter.SerializerSettings.TypeNameHandling = TypeNameHandling.Auto;

            // Add the CSV formatter
            config.Formatters.Add(new CsvFormatter());
        }
    }
}
