using Freedom.Domain.Services.Security;
using Freedom.ViewModels;

namespace Freedom.Client.ViewModel
{
    internal class CurrentUserViewModel : ViewModelBase
    {
        
        public string CurrentUser
        {
            get
            {
                FreedomPrincipal freedomPrincipal = App.User as FreedomPrincipal;

                return freedomPrincipal?.DisplayName ?? App.User?.Identity?.Name ?? "Not logged in";
            }
        }
    }
}
