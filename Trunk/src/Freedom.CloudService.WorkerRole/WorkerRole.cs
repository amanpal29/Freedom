using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.Owin.Hosting;
using Freedom.CloudService.WorkerRole.Config;

namespace Freedom.CloudService.WorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        private IDisposable _owinServer = null;
        public override void Run()
        {
            Trace.TraceInformation("Freedom.CloudService.WorkerRole is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at https://go.microsoft.com/fwlink/?LinkId=166357.

            var endpoint = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["Endpoint1"];
            string baseUri = $"{endpoint.Protocol}://+:{endpoint.IPEndpoint.Port}/Freedom/";

            Trace.TraceInformation($"Starting Freedom.CloudService.WorkerRole at {baseUri}",
                "Information");            
            try
            {
                CloudServiceBootstrapper.Bootstrap();

                _owinServer = WebApp.Start<FreedomService>(new StartOptions(url: baseUri));

                bool result = base.OnStart();

                Trace.TraceInformation("Freedom.CloudService.WorkerRole has been started");

                return result;
            }
            catch (Exception ex)
            {
                Trace.TraceError("An exception occurred while starting the WebApp.", ex);
                _owinServer?.Dispose();
                _owinServer = null;
                return false;
            }            
        }

        public override void OnStop()
        {
            Trace.TraceInformation("Freedom.CloudService.WorkerRole is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            try
            {
                _owinServer?.Dispose();
            }
            catch (Exception ex)
            {
                Trace.TraceError("An exception occurred while stopping the WebApp.", ex);
            }
            finally
            {
                _owinServer = null;
            }

            base.OnStop();

            Trace.TraceInformation("Freedom.CloudService.WorkerRole has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working");
                await Task.Delay(1000);
            }
        }
    }
}
