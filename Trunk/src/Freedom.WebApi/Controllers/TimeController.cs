using System;
using System.Web.Http;

namespace Freedom.WebApi.Controllers
{
    [AllowAnonymous]
    public class TimeController : ApiController
    {
        public DateTime Get()
        {
            return DateTime.UtcNow;
        }
    }
}
