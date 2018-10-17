using System;
using System.Web.Http.Controllers;

namespace Freedom.WebApi.Filters
{
    public class NoJsonAttribute : Attribute, IControllerConfiguration
    {
        public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
        {
            controllerSettings.Formatters.Remove(controllerSettings.Formatters.JsonFormatter);
        }
    }
}