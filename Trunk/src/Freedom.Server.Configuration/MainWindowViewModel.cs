using Freedom.Server.Tools.Features.DatabaseBuilder;
using Freedom.ViewModels;

namespace Freedom.Server.Tools
{
    public class MainWindowViewModel : ViewModelBase
    {
        private DatabaseServerConfigurationViewModel _databaseServerConfigurationViewModel;

        public MainWindowViewModel()
        {
            _databaseServerConfigurationViewModel = new DatabaseServerConfigurationViewModel();
        }

        public DatabaseServerConfigurationViewModel DatabaseServerConfigurationViewModel
        {
            get { return _databaseServerConfigurationViewModel; }
        }
    }
}
