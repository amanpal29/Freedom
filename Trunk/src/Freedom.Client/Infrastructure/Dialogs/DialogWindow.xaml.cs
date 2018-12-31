using Freedom.UI;
using Freedom.UI.ViewModels;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Freedom.Client.Infrastructure.Dialogs
{
    /// <summary>
    /// Interaction logic for DialogWindow.xaml
    /// </summary>
    public partial class DialogWindow
    {
        private int _initialContentRendered;

        public DialogWindow(DialogViewModel dialogViewModel)
        {
            DataContext = dialogViewModel;

            InitializeComponent();

            double screenHeight = Math.Min(SystemParameters.MaximizedPrimaryScreenHeight,
                                           SystemParameters.PrimaryScreenHeight);

            double screenWidth = Math.Min(SystemParameters.MaximizedPrimaryScreenWidth,
                                          SystemParameters.PrimaryScreenWidth);

            MaxHeight = Math.Floor(screenHeight * 0.9);

            MaxWidth = Math.Floor(screenWidth * 0.9);
        }

        private void HandleWindowLoaded(object sender, RoutedEventArgs e)
        {
            DialogViewModel dialogViewModel = DataContext as DialogViewModel;

            if (dialogViewModel != null && dialogViewModel.AllowResize)
            {
                DialogGrid.RowDefinitions[0].Height = new GridLength(1.0, GridUnitType.Star);
                DialogGrid.ColumnDefinitions[0].Width = new GridLength(1.0, GridUnitType.Star);
                ResizeMode = ResizeMode.CanResizeWithGrip;
                Activated += UnlockMaximumSize;
            }

            FocusHelper.FocusFirstFocusableChildElement(ContentControl);
        }

        private async void HandleContentRendered(object sender, EventArgs e)
        {
            if (Interlocked.CompareExchange(ref _initialContentRendered, 1, 0) != 0) return;

            IAsyncInitializable asyncInitializable = DataContext as IAsyncInitializable;

            if (asyncInitializable == null) return;

            using (new WindowWaitMonitor(this))
            {
                await asyncInitializable.InitializeAsync();
            }

            CommandManager.InvalidateRequerySuggested();

            FocusHelper.FocusFirstFocusableChildElement(ContentControl);
        }

        private void UnlockMaximumSize(object sender, EventArgs e)
        {
            SizeToContent = SizeToContent.Manual;
            MaxHeight = double.PositiveInfinity;
            MaxWidth = double.PositiveInfinity;
            Activated -= UnlockMaximumSize;
        }

        private void HandleWindowClosing(object sender, CancelEventArgs e)
        {
            DialogViewModel dialogViewModel = DataContext as DialogViewModel;

            dialogViewModel?.OnWindowClosing(sender, e);
        }

        private void HandleWindowClosed(object sender, EventArgs e)
        {
            DialogViewModel dialogViewModel = DataContext as DialogViewModel;

            dialogViewModel?.OnWindowClosed(sender, e);
        }
    }
}
