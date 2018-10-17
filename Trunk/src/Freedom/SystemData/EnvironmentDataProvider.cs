using System;
using Microsoft.Win32;

namespace Freedom.SystemData
{
    public class EnvironmentDataProvider : ISystemDataProvider
    {
        private readonly string _sectionName;

        public EnvironmentDataProvider(string sectionName)
        {
            _sectionName = sectionName;
        }

        public void LoadData(SystemDataCollection collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            SystemDataSection section = collection[_sectionName];

            section.TryAdd(@"Computer Name", () => Environment.MachineName);
            section.TryAdd(@"Current User", GetCurrentUser);
            section.TryAdd(@"Processor", GetProcessor, "Unknown");
            section.TryAdd(@".NET Framework Version", () => Environment.Version.ToString());
        }

        private static string GetProcessor()
        {
            return Registry.GetValue(@"HKEY_LOCAL_MACHINE\Hardware\Description\System\CentralProcessor\0", @"ProcessorNameString", string.Empty).ToString();
        }

        private static string GetCurrentUser()
        {
            return !string.IsNullOrEmpty(Environment.UserDomainName)
                       ? Environment.UserDomainName + '\\' + Environment.UserName
                       : Environment.UserName;
        }
    }
}
