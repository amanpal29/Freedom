using Freedom.Client.Infrastructure;
using Freedom.ViewModels;

namespace Freedom.Client.ViewModel
{
    public class SystemStatusViewModel : ViewModelBase
    {        
        public SystemStatusViewModel()
        {   
        }

        public virtual string GlobalInstanceName => string.IsNullOrWhiteSpace(ApplicationSettings.Current.GlobalInstanceName)
            ? "Freedom Server"
            : ApplicationSettings.Current.GlobalInstanceName;        
    }
}
