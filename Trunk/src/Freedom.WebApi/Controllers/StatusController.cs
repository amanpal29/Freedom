using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Web.Http;
using Freedom.Domain.Model;
using Freedom.Domain.Services.Status;
using Freedom;
using Freedom.Extensions;
using Freedom.SystemData;
using log4net;

namespace Freedom.WebApi.Controllers
{
    [AllowAnonymous]
    public class StatusController : ApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const string TextHtmlMediaType = "text/html";

        private const string GlobalIdKey = "GlobalId";
        private const string MinimumClientVersionKey = "MinimumClientVersion";
        private const string MaximumClientVersionKey = "MaximumClientVersion";

        // This could've be done cleaner with Razor.
        //
        // This is the only place we're returning HTML at the moment. I chose a StringBuilder
        // here instead so I don't have to include the entire Razor engine in the deployment package.
        // If the Razor Engine is ever added to the project anyway, this should be refactored to use it.
        //
        // - DGG 2016-01-27    
        public HttpResponseMessage Get()
        {
            HttpResponseMessage response = new HttpResponseMessage();

            StringBuilder status = new StringBuilder();

            status.AppendLine("<html>");

            status.AppendLine("<head>");
            status.AppendLine("<title>Server Status</title>");
            status.AppendLine("</head>");

            status.AppendLine("<body>");

            status.AppendLine("<h2>The Server is Running.</h2>");

            status.AppendLine($"The current server time is {DateTime.UtcNow:dd-MMM-yyyy hh:mm:ss} UTC<br/>");
            status.AppendLine($"The current server time is {DateTime.Now:dd-MMM-yyyy hh:mm:ss zzz} Local<br/>");

            foreach (SystemDataSection systemDataSection in SystemDataCollection.GetCurrentData())
            {
                status.AppendLine($"<h3>{systemDataSection.Name}</h3>");

                status.AppendLine("<dl>");

                foreach (KeyValuePair<string, string> element in systemDataSection.Items)
                    status.AppendLine($"<dt>{element.Key}</dt><dd>{element.Value}</dd>");

                status.AppendLine("</dl>");
            }

            status.AppendLine("</body>");

            status.AppendLine("</html>");

            response.Content = new StringContent(status.ToString(), Encoding.UTF8, TextHtmlMediaType);

            return response;
        }

        [HttpGet]
        public VersionData Version()
        {
            VersionData versionData = new VersionData();

            try
            {
                using (IDbConnection dbConnection = IoC.Get<IDbConnection>())
                {
                    if (dbConnection.State != ConnectionState.Open)
                        dbConnection.Open();

                    versionData.DatabaseIdentifier = ToGuid(dbConnection.LookupValue(nameof(ApplicationSetting),
                        nameof(ApplicationSetting.Value), nameof(ApplicationSetting.Key), GlobalIdKey)) ?? Guid.Empty;
                }
            }
            catch (Exception ex)
            {
                Log.Warn("Exception while trying to read DatabaseRevision applicaion setting.", ex);
            }

            versionData.MinimumClientVersion = ConfigurationManager.AppSettings.Get(MinimumClientVersionKey);
            versionData.MaximumClientVersion = ConfigurationManager.AppSettings.Get(MaximumClientVersionKey);
            versionData.ServerVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            return versionData;
        }

        [HttpGet]
        public SystemDataCollection SystemData()
        {
            return SystemDataCollection.GetCurrentData();
        }

        private static Guid? ToGuid(object value)
        {
            if (value == null)
                return null;

            if (value is Guid)
                return (Guid) value;

            Guid result;

            if (Guid.TryParse(value.ToString(), out result))
                return result;

            return null;
        }
    }
}
