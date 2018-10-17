using System;
using System.Reflection;
using System.ServiceProcess;
using Freedom.Domain.Services.BackgroundWorkQueue;
using log4net;
using Microsoft.Owin.Hosting;

namespace Freedom.Server
{
    public class FreedomServerService : ServiceBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IDisposable _owinServer;
        private PeriodicWorkerController _periodicWorkerController;

        public FreedomServerService(string serviceName)
        {
            ServiceName = serviceName;
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                _owinServer = WebApp.Start<Program>(Program.BaseAddress);
            }
            catch (Exception ex)
            {
                Log.Error("An exception occurred while starting the WebApp.", ex);
                _owinServer?.Dispose();
                _owinServer = null;
            }

            _periodicWorkerController = new PeriodicWorkerController();
        }

        protected override void OnStop()
        {
            (IoC.TryGet<IBackgroundWorkQueue>() as IDisposable)?.Dispose();

            _periodicWorkerController?.Dispose();
            _periodicWorkerController = null;

            try
            {
                _owinServer?.Dispose();
            }
            catch (Exception ex)
            {
                Log.Error("An exception occurred while stopping the WebApp.", ex);
            }
            finally
            {
                _owinServer = null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_periodicWorkerController != null)
                {
                    _periodicWorkerController.Dispose();
                    _periodicWorkerController = null;
                }

                if (_owinServer != null)
                {
                    _owinServer.Dispose();
                    _owinServer = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
