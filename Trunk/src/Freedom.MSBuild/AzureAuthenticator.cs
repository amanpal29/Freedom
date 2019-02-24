using log4net;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Freedom.MSBuild
{
    public class AzureAuthenticator
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public AzureAuthenticator(string clientId, string clientSecret, string authority, string redirectUri)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
            Authority = authority;
            RedirectUri = redirectUri;
        }

        internal string ClientId { get; private set; }

        private string ClientSecret { get; set; }

        internal string Authority { get; private set; }

        internal string RedirectUri { get; private set; }

        public AuthenticationResult GetToken(string[] scopes)
        {
            ClientCredential clientCredentials = new ClientCredential(ClientSecret);
            var app = new ConfidentialClientApplication(ClientId, Authority, RedirectUri, clientCredentials, null, new TokenCache());

            AuthenticationResult result = null;
            try
            {
                result = app.AcquireTokenForClientAsync(scopes).Result;
            }
            catch (MsalServiceException ex) when (ex.Message.Contains("AADSTS70011"))
            {
                Log.Error("Invalid scope. The scope has to be of the form 'https://resourceurl/.default'");
                Log.Info("Mitigation: change the scope to be as expected");
            }
            return result;
        }
    }
}
