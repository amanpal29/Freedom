using System;
using System.Linq;
using System.Management;

namespace Freedom.SystemData
{
    public class OperatingSystemDataProvider : ISystemDataProvider
    {
        private readonly string _sectionName;

        public OperatingSystemDataProvider(string sectionName)
        {
            _sectionName = sectionName;
        }

        public void LoadData(SystemDataCollection collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            SystemDataSection section = collection[_sectionName];

            section.TryAdd(@"Operating System", GetOperatingSystem);
            section.TryAdd(@"    Service Pack", () => Environment.OSVersion.ServicePack, "No Service Pack Installed");
            section.TryAdd(@"    Type", GetOperatingSystemType);
            section.TryAdd(@"    Version", () => Environment.OSVersion.Version.ToString());
        }

        private static string GetOperatingSystem()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"SELECT Caption FROM Win32_OperatingSystem");

            foreach (ManagementObject managementObject in searcher.Get().OfType<ManagementObject>())
            {
                string result = managementObject[@"Caption"].ToString();

                if (!string.IsNullOrWhiteSpace(result))
                    return result.Trim();
            }

            return null;
        }

        private static string GetOperatingSystemType()
        {
            return (Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit") + " Operating System";
        }
    }
}
