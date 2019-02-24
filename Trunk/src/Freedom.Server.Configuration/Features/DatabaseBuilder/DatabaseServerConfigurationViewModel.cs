using Freedom.Domain.Services.DatabaseBuilder;
using Freedom.UI;
using Freedom.ViewModels;
using Microsoft.Identity.Client;
using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Reflection;
using log4net;
using System.Threading;
using System.Windows;
using System.Configuration;

namespace Freedom.Server.Tools.Features.DatabaseBuilder
{
    public class DatabaseServerConfigurationViewModel : ViewModelBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const string DefaultDatabaseName = @"Freedom";
        private const string AzureadInstance = "https://login.microsoftonline.com/{0}";
        private const string DefaultTenant = "amanpal23hotmail.onmicrosoft.com";
        private const string DefaultRedirectUri = "https://amanpal23hotmail.onmicrosoft.com/freedomdaemonconsole";
        private Guid DefaultClientId = new Guid("{43360543-57a8-494c-aca5-420a5c8d2d81}");

        private FreedomDatabaseType _freedomDatabaseType = FreedomDatabaseType.Cloud;
        private string _databaseName;
        private string _serverName;
        private string _azureRedirectUri;
        private string _azureClientId;
        private string _azureTenant;
        private string _accessToken;
        private static PublicClientApplication _clientApp;
                
        public string ConnectionString
        {
            get
            {
                ConnectionStringSettings connectionStringSettings = null;

                switch(DatabaseType)
                {
                    case FreedomDatabaseType.Cloud:
                        connectionStringSettings = ConfigurationManager.ConnectionStrings["cloud"];
                        break;
                    case FreedomDatabaseType.Server:
                        connectionStringSettings = ConfigurationManager.ConnectionStrings["local"];
                        break;
                    default:
                        throw new ArgumentException($"Database of type {DatabaseType} is not supported.", nameof(DatabaseType)); 
                }
                
                return connectionStringSettings != null ? connectionStringSettings.ConnectionString : string.Empty;
            }
        }

        public string MasterConnectionString
        {
            get
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(ConnectionString);
                builder.InitialCatalog = "master";
                builder.AttachDBFilename = string.Empty;
                return builder.ConnectionString;
            }
        }

        public string DatabaseName
        {
            get
            {
                if (!string.IsNullOrEmpty(_databaseName))
                    return _databaseName;

                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(ConnectionString);

                return string.IsNullOrEmpty(builder.InitialCatalog) ? DefaultDatabaseName : builder.InitialCatalog;
            }
            set
            {
                _databaseName = value;
                OnPropertyChanged(nameof(ConnectionString));
            }
        }

        public string AzureSubscriptionId { get; set; }

        public string ResourceGroupName { get; set; }

        public string ServerName
        {
            get
            {
                if (!string.IsNullOrEmpty(_serverName))
                    return _serverName;

                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(ConnectionString);

                return string.IsNullOrEmpty(builder.DataSource) ? string.Empty : GetServerName(builder.DataSource);
            }
            set
            {
                _serverName = value;
                OnPropertyChanged(nameof(ConnectionString));
            }
        }

        public string AzureTenant
        {
            get
            {
                return string.IsNullOrEmpty(_azureTenant) ? DefaultTenant : _azureTenant;
            }
            set
            {
                _azureTenant = value;
            }
        }

        public string AzureClientId
        {
            get
            {
                return string.IsNullOrEmpty(_azureClientId) ? DefaultClientId.ToString() : _azureClientId;
            }
            set
            {
                _azureClientId = value;
            }
        }

        public string AzureAuthority { get { return String.Format(CultureInfo.InvariantCulture, AzureadInstance, AzureTenant); } }

        public string AzureRedirectUri
        {
            get
            {
                return string.IsNullOrEmpty(_azureRedirectUri) ? DefaultRedirectUri : _azureRedirectUri;
            }
            set
            {
                _azureRedirectUri = value;
            }
        }

        public Boolean IsCloudDatabase
        {
            get { return _freedomDatabaseType == FreedomDatabaseType.Cloud; }
            set
            {
                _freedomDatabaseType = value ? FreedomDatabaseType.Cloud : FreedomDatabaseType.Server;
                OnPropertyChanged(nameof(IsCloudDatabase));
                OnPropertyChanged(nameof(ConnectionString));
                OnPropertyChanged(nameof(ServerName));
                OnPropertyChanged(nameof(DatabaseName));
            }
        }       

        public FreedomDatabaseType DatabaseType
        {
            get { return _freedomDatabaseType; }
            set { _freedomDatabaseType = value; }
        }
        
        internal string AccessToken { get { return _accessToken; } }    

        public ICommand AzureLoginCommand => new AsyncCommand(AzureLoginAsync, CanLogin);

        public ICommand RebuildDatabaseCommand => new RelayCommand(RebuildDatabase, CanRebuildDatabase);

        private bool CanLogin()
        {
            return IsCloudDatabase;
        }

        private async Task AzureLoginAsync()
        {
            string[] scopes = new string[] { "https://management.azure.com/user_impersonation" };
            _clientApp = new PublicClientApplication(AzureClientId, AzureAuthority, TokenCacheHelper.GetUserCache());

            AuthenticationResult authResult = null;
            var accounts = await _clientApp.GetAccountsAsync();

            try
            {
                authResult = await _clientApp.AcquireTokenSilentAsync(scopes, accounts.FirstOrDefault());
            }
            catch (MsalUiRequiredException ex)
            {
                // A MsalUiRequiredException happened on AcquireTokenSilentAsync. 
                // This indicates you need to call AcquireTokenAsync to acquire a token
                Log.Error($"MsalUiRequiredException: {ex.Message}");

                try
                {
                    authResult = await _clientApp.AcquireTokenAsync(scopes);
                }
                catch (MsalException msalex)
                {
                    Log.Error($"Error Acquiring Token:{Environment.NewLine}{msalex}");
                }
            }
            catch (Exception ex)
            {
                
                Log.Error($"Error Acquiring Token Silently:{Environment.NewLine}{ex}");
                return;
            }

            if (authResult != null)
            {
                _accessToken = authResult.AccessToken;
            }
        }

        private bool CanRebuildDatabase()
        {
            return !string.IsNullOrEmpty(ConnectionString)
                && !string.IsNullOrEmpty(ServerName)
                && !string.IsNullOrEmpty(DatabaseName)
                && (!IsCloudDatabase || (IsCloudDatabase && HasRequiredAzureInformation));
        }

        private bool HasRequiredAzureInformation {
            get {
                    return !string.IsNullOrEmpty(_accessToken) 
                        && !string.IsNullOrEmpty(AzureSubscriptionId) 
                        && !string.IsNullOrEmpty(ResourceGroupName);
            }
        }

        private void RebuildDatabase()
        {
            DatabaseBuildEngine engine = new DatabaseBuildEngine(this);

            LogWindow logWindow = new LogWindow(engine);

            logWindow.Owner = Application.Current.MainWindow;

            if (logWindow.ShowDialog() == true)
                Application.Current.Shutdown();           
            
        }

        private string GetServerName(string dataSource)
        {
            string serverName;
            int startIndex = dataSource.IndexOf(":");
            int startPosition = startIndex + 1;
            int endIndex = dataSource.IndexOf(",");

            if (startIndex == -1 || endIndex == -1 || startPosition >= endIndex)
            {
                 serverName = dataSource;
            }
            else
            {
                serverName = dataSource.Substring(startPosition, endIndex - startPosition);
            }           
            return IsCloudDatabase && serverName.Split('.').Any() ? serverName.Split('.').First() : serverName;
        }
    }
}
