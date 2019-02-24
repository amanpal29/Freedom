using Freedom.Domain.Services.DatabaseBuilder;
using Freedom.ViewModels;
using log4net;
using System;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Freedom.Server.Tools.Features.DatabaseBuilder
{
    public class DatabaseBuildEngine : ViewModelBase, IEngine
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private DatabaseServerConfigurationViewModel _databaseServerConfigurationViewModel;
        private bool _hasFinished = false;
        private bool? _succeeded = false;
        private bool _hasStarted;

        internal DatabaseBuildEngine(DatabaseServerConfigurationViewModel databaseServerConfigurationViewModel)
        {
            _databaseServerConfigurationViewModel = databaseServerConfigurationViewModel;
        }

        public bool HasFinished
        {
            get { return _hasFinished; }
            private set
            {
                if (value == _hasFinished) return;
                _hasFinished = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool HasStarted
        {
            get { return _hasStarted; }
            private set
            {
                if (value == _hasStarted) return;
                _hasStarted = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool? Succeeded
        {
            get { return _succeeded; }
            private set
            {
                if (value == _succeeded) return;
                _succeeded = value;
                OnPropertyChanged();
            }
        }

        public async void Start()
        {
            if (HasStarted) return;

            try
            {
                HasStarted = true;
                await RebuildDatabaseAsync();
            }
            finally
            {
                HasFinished = true;
            }           
            
            OnPropertyChanged(nameof(HasStarted));            
        }

        private async Task RebuildDatabaseAsync()
        {   
            DatabaseBuilderService databaseBuilderService = new DatabaseBuilderService();
            databaseBuilderService.FreedomDatabaseType = _databaseServerConfigurationViewModel.DatabaseType;

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(_databaseServerConfigurationViewModel.ConnectionString);
            builder.InitialCatalog = _databaseServerConfigurationViewModel.DatabaseName;
            databaseBuilderService.ProviderConnectionString = builder.ToString();
            databaseBuilderService.ServerName = _databaseServerConfigurationViewModel.ServerName;
            databaseBuilderService.SubscriptionId = _databaseServerConfigurationViewModel.AzureSubscriptionId;
            databaseBuilderService.ResourceGroupName = _databaseServerConfigurationViewModel.ResourceGroupName;

            String accessToken = _databaseServerConfigurationViewModel.AccessToken;

            Log.Info($"Starting to rebuild {builder.InitialCatalog}.");

            if (databaseBuilderService.DatabaseExists)
            {
               await databaseBuilderService.DeleteDatabaseAsync(accessToken != null ? accessToken : string.Empty);               
            }
            else
            {
                Log.Info($"Skipping DeleteDatabase task; database '{builder.InitialCatalog}' has already been deleted.");
            }

           await databaseBuilderService.CreateDatabaseAsync(CancellationToken.None, accessToken != null ? accessToken : string.Empty);            
        }        
    }
}
