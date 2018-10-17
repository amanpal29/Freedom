using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.ServiceProcess;
using System.Web.Http;
using System.Web.Http.Tracing;
using Freedom.Server.Resources;
using Freedom.Server.Config;
using Freedom.WebApi;
using Freedom.WebApi.Infrastructure;
using Freedom.Domain.Services.BackgroundWorkQueue;
using Freedom.Parsers;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Repository.Hierarchy;
using Microsoft.Owin.Hosting;
using Owin;

namespace Freedom.Server
{
    public class Program
    {
        public const string Name = "Freedom";

        public const string ServiceName = "FreedomServer";

        public const string DefaultBaseAddress = "https://+:443/Freedom/";

        public static string HostName => IPGlobalProperties.GetIPGlobalProperties().HostName;

        public static string BaseAddress => ConfigurationManager.AppSettings["BaseAddress"] ?? DefaultBaseAddress;

        public static void Main(string[] args)
        {
            try
            {
                CommandLineArguments arguments;

                if (!CommandLineParser.TryParse(args, Console.Error, out arguments) || arguments.Help)
                {
                    string filename = Path.GetFileName(Assembly.GetExecutingAssembly().Location);

                    Console.WriteLine(FreedomServerResources.CommandLineHelp, filename);
                }
                else if (arguments.Install)
                {
                    FreedomServerServiceInstaller.InstallService(arguments);
                }
                else if (arguments.Uninstall)
                {
                    FreedomServerServiceInstaller.UninstallService();
                }
                else if (arguments.Status)
                {
                    Console.WriteLine(FreedomServerServiceInstaller.GetStatus());
                }
                else
                {
                    if (!arguments.Foreground)
                        DisableConsoleAppenders();

                    ServerBootstrapper.Bootstrap();

                    if (arguments.Foreground)
                    {
                        RunInForeground();
                    }
                    else
                    {
                        ServiceBase.Run(new FreedomServerService(ServiceName));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static bool IsRootLogLevelTraceOrLower
        {
            get
            {
                Hierarchy hierarchy = LogManager.GetRepository() as Hierarchy;

                return hierarchy?.Root?.Level != null && hierarchy.Root.Level <= Level.Trace;
            }
        }

        private static void DisableConsoleAppenders()
        {
            IAppender[] appenders = LogManager.GetRepository().GetAppenders();

            foreach (ConsoleAppender appender in appenders.OfType<ConsoleAppender>())
                appender.Threshold = Level.Off;

            foreach (ColoredConsoleAppender appender in appenders.OfType<ColoredConsoleAppender>())
                appender.Threshold = Level.Off;
        }

        private static void RunInForeground()
        {
            using (WebApp.Start<Program>(BaseAddress))
            using (new PeriodicWorkerController())
            using (IoC.TryGet<IBackgroundWorkQueue>() as IDisposable)
            {
                Console.WriteLine("The server is running. Press <ENTER> to stop the server.");
                Console.ReadLine();
            }
        }

        public static void Configuration(IAppBuilder appBuilder)
        {
            HttpConfiguration configuration = new HttpConfiguration();

            HttpListener listener = appBuilder.GetProperty<HttpListener>();

            listener.AuthenticationSchemes =
                AuthenticationSchemes.Anonymous |
                AuthenticationSchemes.Basic |
                AuthenticationSchemes.Ntlm |
                AuthenticationSchemes.Negotiate;

            listener.Realm = Name;

            WebApiConfig.Register(configuration);

            if (IsRootLogLevelTraceOrLower)
            {
                configuration.Services.Replace(typeof(ITraceWriter), new Log4NetTraceWriter());
            }

            appBuilder.UseWebApi(configuration);
        }
    }

    public static class AppBuilderExtensions
    {
        public static T GetProperty<T>(this IAppBuilder appBuilder)
        {
            return (T)appBuilder.Properties[typeof(T).FullName];
        }
    }
}
