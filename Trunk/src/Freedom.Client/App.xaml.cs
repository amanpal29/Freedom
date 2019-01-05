using Freedom.Client.Config;
using Freedom.Client.Features.AuthenticateUser;
using Freedom.Client.Features.Dashboard;
using Freedom.Client.Infrastructure;
using Freedom.Client.Infrastructure.Dialogs;
using Freedom.Client.Infrastructure.Dialogs.ViewModels;
using Freedom.Client.Properties;
using Freedom.Client.ViewModel;
using Freedom.Domain.Exceptions;
using Freedom.Domain.Infrastructure.ExceptionHandling;
using Freedom.Domain.Interfaces;
using Freedom.Domain.Services.Security;
using Freedom.Domain.Services.Time;
using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Freedom.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : IRefreshable
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public const string CompanyName = "Automated Trading Ltd.";
        public const string Name = "Freedom";

        private static readonly TimeSpan MinimumNetworkTimeout = new TimeSpan(0, 0, 5);
        private static readonly TimeSpan DefaultNetworkTimeout = new TimeSpan(0, 0, 30);

        private const int LoginFailureExitCode = 1;
        private const int VersionMismatchExitCode = 2;
        private const int ServerNotFoundExitCode = 3;

        #region Global Properties and Methods

        public static IPrincipal User { get; set; }

        public static Guid CurrentUserId
        {
            get
            {
                FreedomPrincipal freedomPrincipal = User as FreedomPrincipal;

                if (freedomPrincipal == null || freedomPrincipal.UserId == Guid.Empty)
                    throw new InvalidOperationException("The current user is not authenticated.");

                return freedomPrincipal.UserId;
            }
        }

        public static string BaseAddress => ConfigurationManager.AppSettings[nameof(BaseAddress)];

        public static TimeSpan NetworkTimeout { get; private set; } = DefaultNetworkTimeout;
                
        public static string DataFolder
        {
            get
            {
                string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string dataFolder = Path.Combine(appDataFolder, CompanyName, Name);

                FileUtility.TryEnsureDirectoryExists(dataFolder);

                return dataFolder;
            }
        }

        #region Application Startup and Shutdown

        private async void ApplicationStartup(object sender, StartupEventArgs e)
        {
            ClientBootstrapper.Bootstrap();
                        
            NetworkTimeout = GetNetworkTimeout();

            try
            {
                await Task.WhenAll(
                    InitializeTimeServiceAsync());
            }
            catch (CommunicationException ex)
            {
                
                    CancelMessageViewModel cancelMessageViewModel = new CancelMessageViewModel();
                    cancelMessageViewModel.MainInstructionText = $"Can't start {Name}.";
                    cancelMessageViewModel.SecondaryInstructionText =
                        $"{Name} can't connect to the server.\n\n{ex.Message}\n";

                    MainWindow = new DialogWindow(cancelMessageViewModel);
                    MainWindow.ShowDialog();

                    Shutdown(ServerNotFoundExitCode);
                    return;
                
            }

            SetApplicationToolTipDuration();           

            LoginDialog loginDialog = null;

            LoginViewModel loginViewModel = new BasicAuthenticationLoginViewModel();

            loginDialog = new LoginDialog(loginViewModel);

            if (!await loginDialog.ShowAsync())
            {
                Shutdown(LoginFailureExitCode);
                return;
            }

            await ApplicationSettings.InitializeAsync();            

            ITimeService timeService = IoC.TryGet<ITimeService>() ?? new LocalTimeService();

            Settings.Default.LastLoggedInDateTime = timeService.UtcNow;
            Settings.Default.Save();

            SetApplicationDateFormat();

            MainWindow = CreateMainWindow();

            MainWindow.Closing += HandleMainWindowClosing;

            MainWindow.Show();

            loginDialog?.Close();

            ShutdownMode = ShutdownMode.OnMainWindowClose;            
        }
                
        private static async Task InitializeTimeServiceAsync()
        {
            IAsyncInitializable timeService = IoC.TryGet<ITimeService>() as IAsyncInitializable;

            if (timeService != null)
            {
                await timeService.InitializeAsync();
            }
        }                
        
        private static void SetApplicationDateFormat()
        {
            string dateFormat = ApplicationSettings.Current.DateFormat;

            if (!string.IsNullOrEmpty(dateFormat) && dateFormat != "d" &&
                Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern != dateFormat)
            {
                CultureInfo cultureInfo = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
                cultureInfo.DateTimeFormat.ShortDatePattern = dateFormat;
                Thread.CurrentThread.CurrentCulture = cultureInfo;
            }
        }

        private static void SetApplicationToolTipDuration()
        {
            // Override the ToolTipService.ShowDuration through the entire app (set to 60 seconds)
            ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(60000));
        }        

        private static TimeSpan GetNetworkTimeout()
        {
            string timeoutString = ConfigurationManager.AppSettings[nameof(NetworkTimeout)];

            if (string.IsNullOrEmpty(timeoutString))
                return DefaultNetworkTimeout;

            TimeSpan timeSpan;

            if (!TimeSpan.TryParse(timeoutString, out timeSpan))
            {
                Log.Warn($"The value for NetworkTimeout in the configuration file ({timeoutString}) is not valid. "
                         + $"Using the default NetworkTimeout of {DefaultNetworkTimeout} instead.");

                return DefaultNetworkTimeout;
            }

            if (timeSpan < MinimumNetworkTimeout)
            {
                Log.Warn($"The NetworkTimeout setting {timeSpan} is not valid. "
                         + $"using the default NetworkTimeout of {DefaultNetworkTimeout} instead.");

                return DefaultNetworkTimeout;
            }

            return timeSpan;
        }

        private void ApplicationExit(object sender, ExitEventArgs e)
        {
            
        }

        #endregion

        #region Main Window

        private Window CreateMainWindow()
        {
            MainViewModel mainViewModel = new MainViewModel(new DashboardViewModel());
                       
            mainViewModel.StatusBarItems.Add(new SystemStatusViewModel());
            
            Window window = new MainWindow(mainViewModel);

            window.Closing += HandleMainWindowClosing;

            return window;
        }

        private void HandleMainWindowClosing(object sender, CancelEventArgs e)
        {
            // Save the local settings
            Settings.Default.Save();

            if (!e.Cancel)
            {
                // ... tell all child windows to close.
                foreach (Window window in Windows.Cast<Window>().ToArray())
                    if (IsChildWindow(window))
                        window.Close();

                // If they didn't all close, cancel the closing of the application.
                if (Windows.Cast<Window>().Any(IsChildWindow))
                    e.Cancel = true;
            }
        }

        private bool IsChildWindow(Window window)
        {
            return (window != null) && (window != MainWindow);
        }

        #endregion


        #region Implementation of IRefreshable

        public static async Task Refresh()
        {
            IRefreshable refreshable = (IRefreshable)Current;

            await refreshable.RefreshAsync();
        }

        async Task IRefreshable.RefreshAsync()
        {
            IEnumerable<Task> refreshTasks = Current.Windows.OfType<IRefreshable>().Select(x => x.RefreshAsync());

            await Task.WhenAll(refreshTasks);
        }

        #endregion

        #endregion

        #region Last Chance Exception Handling

        private void UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            IExceptionHandlerService exceptionHandlerService = IoC.Get<IExceptionHandlerService>();
            exceptionHandlerService.HandleException(e.Exception, ExceptionContextFlags.LastChance);
            e.Handled = true;
        }

        #endregion
    }
}
