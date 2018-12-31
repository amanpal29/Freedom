using System;
using System.Linq;
using System.Windows;
using Freedom;
using Freedom.ViewModels;
using Freedom.UI;

namespace Freedom.Client.Infrastructure.Dialogs
{
    public interface IDialogService
    {
        bool? Show(object owner, DialogViewModel dialogViewModel);
    }

    public class DialogService : IDialogService
    {
        public bool? Show(object owner, DialogViewModel dialogViewModel)
        {
            if (dialogViewModel == null)
                throw new ArgumentNullException(nameof(dialogViewModel));

            Window window;

            if (owner == null)
            {
                window = Application.Current.MainWindow;
            }
            else if (owner is Window)
            {
                window = (Window) owner;
            }
            else if (owner is DependencyObject)
            {
                window = Window.GetWindow((DependencyObject) owner);
            }
            else
            {
                window = Application.Current.Windows.OfType<Window>()
                    .FirstOrDefault(wnd => IsOwner(wnd, owner));
            }

            if (window == null)
            {
                throw new InvalidOperationException("Unable to locate window for owner viewmodel.");
            }

            return ShowCore(window, dialogViewModel);
        }

        /// <summary>
        /// Determines whether the specified parent (usually a window or a viewmodel) is owner of the specified child (usually a viewmodel)
        /// </summary>
        private static bool IsOwner(object parent, object child)
        {
            if (parent == null || child == null)
                return false;

            if (parent == child)
                return true;

            FrameworkElement frameworkElement = parent as FrameworkElement;
            
            if (frameworkElement != null)
            {
                if (frameworkElement.DataContext != parent && IsOwner(frameworkElement.DataContext, child))
                    return true;
            }

            ViewModelBase parentViewModel = parent as ViewModelBase;

            return parentViewModel?.Children?.Any(vm => IsOwner(vm, child)) ?? false;
        }

        protected virtual bool? ShowCore(Window owner, DialogViewModel dialogViewModel)
        {
            using (WaitCursor waitCursor = new WaitCursor())
            {
                if (dialogViewModel == null)
                    throw new ArgumentNullException(nameof(dialogViewModel));

                DialogWindow window = new DialogWindow(dialogViewModel);

                window.Owner = owner;

                // ReSharper disable once AccessToDisposedClosure
                window.Loaded += (s, a) => waitCursor.Dispose();

                return window.ShowDialog();
            }
        }
    }
}