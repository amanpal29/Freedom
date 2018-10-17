using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using Freedom.Server.Infrastructure;

namespace Freedom.Server
{
    [RunInstaller(true)]
    public class FreedomServerServiceInstaller : Installer
    {
        private const int ErrorCancelledCode = 1223;

        private const string DefaultServiceName = "FreedomServer";

        private const string Account = "account";
        private const string UserName = "username";
        private const string Password = "password";
        private const string InstanceName = "instancename";
        private const string ServiceName = "servicename";

        private readonly ServiceProcessInstaller _serviceProcessInstaller = new ServiceProcessInstaller();
        private readonly ServiceInstaller _serviceInstaller = new ServiceInstaller();

        public FreedomServerServiceInstaller()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            _serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
            _serviceProcessInstaller.Username = null;
            _serviceProcessInstaller.Password = null;

            _serviceInstaller.ServiceName = DefaultServiceName;
            _serviceInstaller.DisplayName = $"Freedom 1.0 Server {Assembly.GetExecutingAssembly().GetName().Version}";
            _serviceInstaller.Description = "Provides central data repository services to Hedgehog 5 clients.";
            _serviceInstaller.StartType = ServiceStartMode.Automatic;

            Installers.Add(_serviceProcessInstaller);
            Installers.Add(_serviceInstaller);
        }

        protected override void OnBeforeInstall(IDictionary savedState)
        {
            base.OnBeforeInstall(savedState);

            string account = Context.Parameters[Account];

            if (nameof(ServiceAccount.LocalService).Equals(account, StringComparison.OrdinalIgnoreCase))
            {
                _serviceProcessInstaller.Account = ServiceAccount.LocalService;
            }
            else if (nameof(ServiceAccount.LocalSystem).Equals(account, StringComparison.OrdinalIgnoreCase))
            {
                _serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
            }
            else if (nameof(ServiceAccount.NetworkService).Equals(account, StringComparison.OrdinalIgnoreCase))
            {
                _serviceProcessInstaller.Account = ServiceAccount.NetworkService;
            }
            else if (nameof(ServiceAccount.User).Equals(account, StringComparison.OrdinalIgnoreCase))
            {
                _serviceProcessInstaller.Account = ServiceAccount.User;

                if (!string.IsNullOrWhiteSpace(Context.Parameters[UserName]))
                {
                    _serviceProcessInstaller.Username = Context.Parameters[UserName];
                    _serviceProcessInstaller.Password = Context.Parameters[Password];
                }
            }
            if (string.IsNullOrWhiteSpace(account))
            {
                if (!string.IsNullOrWhiteSpace(Context.Parameters[UserName]))
                {
                    _serviceProcessInstaller.Account = ServiceAccount.User;
                    _serviceProcessInstaller.Username = Context.Parameters[UserName];
                    _serviceProcessInstaller.Password = Context.Parameters[Password];
                }
            }
            else
            {
                throw new InstallException($"Unsupported parameter for account {account}");
            }

            string instanceName = Context.Parameters[InstanceName];

            if (!string.IsNullOrWhiteSpace(instanceName))
            {
                _serviceInstaller.ServiceName = DefaultServiceName + instanceName;
                _serviceInstaller.DisplayName = $"Freedom 1.0 Server {Assembly.GetExecutingAssembly().GetName().Version} ({instanceName})";
            }

            if (!string.IsNullOrWhiteSpace(Context.Parameters[ServiceName]))
                _serviceInstaller.ServiceName = Context.Parameters[ServiceName];
        }

        protected override void OnBeforeUninstall(IDictionary savedState)
        {
            base.OnBeforeUninstall(savedState);

            if (!string.IsNullOrWhiteSpace(Context.Parameters[ServiceName]))
                _serviceInstaller.ServiceName = Context.Parameters[ServiceName];
        }

        /// <summary>
        /// Runs InstallUtil.exe as Administrator to install this service.
        /// </summary>
        internal static void InstallService(CommandLineArguments arguments)
        {
            string existingServiceName = WindowsServiceHelper.GetServiceName();

            if (!string.IsNullOrEmpty(existingServiceName))
            {
                ServiceController serviceController = new ServiceController(existingServiceName);

                Console.Error.WriteLine(
                    $"This service is already installed as {serviceController.ServiceName} [{serviceController.DisplayName}]");

                return;
            }

            Dictionary<string, string> args = new Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(arguments.InstanceName))
                args.Add("InstanceName", arguments.InstanceName);

            if (!string.IsNullOrWhiteSpace(arguments.Account))
                args.Add("Account", arguments.Account);

            if (!string.IsNullOrWhiteSpace(arguments.UserName))
                args.Add("UserName", arguments.UserName);

            if (!string.IsNullOrWhiteSpace(arguments.Password))
                args.Add("Password", arguments.Password);

            RunInstallUtil(args);
        }

        /// <summary>
        /// Runs InstallUtil.exe as Administrator to Uninstall this service.
        /// </summary>
        internal static void UninstallService()
        {
            string serviceName = WindowsServiceHelper.GetServiceName();

            if (string.IsNullOrEmpty(serviceName))
            {
                Console.Error.WriteLine("This service is not installed.");

                return;
            }

            Dictionary<string, string> args = new Dictionary<string, string>();

            args.Add("U", null);
            args.Add("ServiceName", serviceName);

            RunInstallUtil(args);
        }

        /// <summary>
        /// Runs InstallUtil.exe as Administrator to Uninstall this service.
        /// </summary>
        internal static string GetStatus()
        {
            try
            {
                string serviceName = WindowsServiceHelper.GetServiceName();

                if (string.IsNullOrEmpty(serviceName))
                    return "This service is not installed.";

                ServiceController serviceController = new ServiceController(serviceName);

                return
                    $"This service is installed as {serviceController.ServiceName} [{serviceController.DisplayName}]"
                    + $" and has a status of {serviceController.Status}.";
            }
            catch (Exception ex)
            {
                return $"An error occurred trying to read the status of this service. {ex}";
            }
        }

        /// <summary>
        /// Runs InstallUtil.exe as Administrator with the specified arguments on the currently executing assembly
        /// </summary>
        private static void RunInstallUtil(IEnumerable<KeyValuePair<string, string>> arguments)
        {
            if (arguments == null)
                throw new ArgumentNullException(nameof(arguments));

            string installUtilPath = Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "InstallUtil.exe");
            string filename = Path.GetFileName(Assembly.GetExecutingAssembly().Location);

            StringBuilder processArguments = new StringBuilder();

            foreach (KeyValuePair<string, string> keyValuePair in arguments)
            {
                processArguments.Append(string.IsNullOrEmpty(keyValuePair.Value)
                    ? $"/{keyValuePair.Key} "
                    : $"/{keyValuePair.Key}=\"{keyValuePair.Value}\" ");
            }

            processArguments.Append($"\"{filename}\"");

            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo(installUtilPath);
                processStartInfo.Arguments = processArguments.ToString();
                processStartInfo.Verb = "runas";
                Process.Start(processStartInfo);
            }
            catch (Win32Exception ex)
            {
                // Ignore the exception if the user cancelled at the UAC elevation prompt 
                if (ex.NativeErrorCode != ErrorCancelledCode)
                    throw;
            }
            catch (FileNotFoundException)
            {
                Console.Error.WriteLine("Unable to locate InstallUtil.exe.");
            }
        }
    }
}
