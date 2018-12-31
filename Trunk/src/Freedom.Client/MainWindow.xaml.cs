using Freedom.Client.Infrastructure;
using Freedom.Client.ViewModel;
using Freedom.Domain.Infrastructure.ExceptionHandling;
using Freedom.Domain.Interfaces;
using Freedom.Extensions;
using Freedom.UI.Extensions;
using Freedom.UI.ViewModels;
using Freedom.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Freedom.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : IRefreshable
    {
        private readonly string _key;
        private bool _forceClose;
        private int _initialContentRendered;

        internal MainWindow(MainViewModel viewModel = null)
        {
            DataContext = viewModel ?? new MainViewModel();

            InitializeComponent();            

            _key = this == Application.Current.MainWindow
                ? $"{MainViewModel.Workspace.GetType().FullName}.{typeof(MainWindow).Name}"
                : MainViewModel.Workspace.GetType().FullName;

            if (MainViewModel?.Workspace != null)
                this.RestoreWindowPosition(_key);
        }

        internal MainViewModel MainViewModel => DataContext as MainViewModel;        

        #region Event Handlers

        private void HandleCloseCommand(object sender, ExecutedRoutedEventArgs executedRoutedEventArgs)
        {
            Close();
        }

        private void HandleWindowClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            this.SaveWindowPosition(_key);

            if (_forceClose) return;

            MainViewModel?.OnWindowClosing(sender, cancelEventArgs);
        }

        private void HandleWindowClosed(object sender, EventArgs eventArgs)
        {
            if (_forceClose) return;

            if (MainViewModel == null) return;

            MainViewModel.OnWindowClosed(sender, eventArgs);                    
            
        }

        private async void HandleContentRendered(object sender, EventArgs e)
        {
            if (Interlocked.CompareExchange(ref _initialContentRendered, 1, 0) != 0) return;

            ViewModelBase root = DataContext as ViewModelBase;

            if (root == null) return;

            List<IAsyncInitializable> asyncInitializables = root.GetDescendants<IAsyncInitializable>().ToList();

            if (asyncInitializables.Count <= 0) return;

            try
            {
                using (new WindowWaitMonitor(this))
                {
                    await Task.WhenAll(asyncInitializables.Where(x => !x.IsInitialized).Select(x => x.InitializeAsync()));
                }

                CommandManager.InvalidateRequerySuggested();
            }
            catch (CanceledException)
            {
                _forceClose = true;

                Close();
            }
        }       

        #endregion

        #region Implementation of IRefreshable

        public virtual async Task RefreshAsync()
        {
            IRefreshable refreshable = DataContext as IRefreshable;

            if (refreshable != null)
            {
                await refreshable.RefreshAsync();
            }
        }

        #endregion
    }
}
