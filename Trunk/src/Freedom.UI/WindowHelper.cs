using Freedom.ViewModels;
using Freedom.Extensions;
using System.Linq;
using System.Windows;

namespace Freedom.UI
{
    public static class WindowHelper
    {
        public static Window FindWindow(object context)
        {
            if (context == null)
                return null;

            if (context is Window)
                return (Window)context;

            foreach (Window window in Application.Current.Windows.OfType<Window>())
                if (window.DataContext == context)
                    return window;

            ViewModelBase viewModel = context as ViewModelBase;

            if (viewModel == null)
                return null;

            foreach (Window window in Application.Current.Windows.OfType<Window>())
            {
                ViewModelBase dataContext = window.DataContext as ViewModelBase;

                if (dataContext != null && dataContext.HasDescendant(viewModel))
                    return window;
            }

            return null;
        }
    }
}
