using Freedom.SystemData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Freedom.CloudService.WorkerRole.Infrastructure
{
    public class CloudServiceDataProvider : ISystemDataProvider
    {
        private readonly string _sectionName;

        public CloudServiceDataProvider(string sectionName)
        {
            _sectionName = sectionName;
        }

        public void LoadData(SystemDataCollection collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            ServiceController controller = GetCurrentServiceController();

            if (controller == null) return;

            SystemDataSection section = collection[_sectionName];

            section.Add(@"Service", controller.ServiceName);
            section.Add(@"    Display Name", controller.DisplayName);
            section.Add(@"    Service Type", controller.ServiceType.ToString());
        }

        private static ServiceController GetCurrentServiceController()
        {
            try
            {
                int processId = Process.GetCurrentProcess().Id;

                string query = "SELECT * FROM Win32_Service where ProcessId = " + processId;

                ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);

                foreach (ManagementObject managementObject in searcher.Get().OfType<ManagementObject>())
                {
                    string serviceName = managementObject["Name"].ToString();

                    ServiceController controller = ServiceController.GetServices()
                        .FirstOrDefault(sc => sc.ServiceName == serviceName);

                    if (controller != null)
                        return controller;
                }
            }
            catch (Exception)
            {
                return null;
            }

            return null;
        }
    }
}
