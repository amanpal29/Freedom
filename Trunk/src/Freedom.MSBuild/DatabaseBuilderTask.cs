using System.Data.SqlClient;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Globalization;

namespace Freedom.MSBuild
{
    public abstract class DatabaseBuilderTask : Task
    {
        private const string DefaultDatabaseName = @"Freedom";
        private const string AzureadInstance = "https://login.microsoftonline.com/{0}";
        private const string DefaultAzureTenant = "amanpal23hotmail.onmicrosoft.com";
        private const string DefaultAzureRedirectUri = "https://amanpal23hotmail.onmicrosoft.com/freedomdaemonconsole";
        private Guid DefaultAzureClientId = new Guid("{43360543-57a8-494c-aca5-420a5c8d2d81}");

        private string _databaseName;            
        private string _azureRedirectUri;
        private string _azureClientId;
        private string _azureTenant;
        
        [Required]
        public string ConnectionString { get; set; }

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
            }
        }

        public string AzureSubscriptionId { get; set; }

        public string ResourceGroupName { get; set; }

        public string ServerName { get; set; }

        public string AzureTenant
        {
            get
            {
                return string.IsNullOrEmpty(_azureTenant) ? DefaultAzureTenant : _azureTenant;
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
                return string.IsNullOrEmpty(_azureClientId) ? DefaultAzureClientId.ToString() : _azureClientId;
            }
            set
            {
                _azureClientId = value;
            }
        }

        public string AzureAuthority { get { return String.Format(CultureInfo.InvariantCulture, AzureadInstance, AzureTenant); }  }

        public string AzureRedirectUri {
            get
            {
                return string.IsNullOrEmpty(_azureRedirectUri) ? DefaultAzureRedirectUri : _azureRedirectUri;
            }
            set
            {
                _azureRedirectUri = value;
            }
        }

        public string ClientSecret { get; set; }
    }
}