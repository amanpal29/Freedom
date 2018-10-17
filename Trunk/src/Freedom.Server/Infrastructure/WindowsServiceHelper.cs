using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace Freedom.Server.Infrastructure
{
    public static class WindowsServiceHelper
    {
        private static FileInformation GetFileInformation(SafeFileHandle fileHandle)
        {
            if (fileHandle == null)
                throw new ArgumentNullException(nameof(fileHandle));

            if (fileHandle.IsInvalid)
                throw new ArgumentException("fileHandle is Invalid", nameof(fileHandle));

            FileInformation result;

            if (!NativeMethods.GetFileInformationByHandle(fileHandle.DangerousGetHandle(), out result))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            return result;
        }

        private static FileInformation GetFileInformation(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                return GetFileInformation(stream.SafeFileHandle);
        }

        private static bool IsSameFile(string path1, string path2)
        {
            if (string.IsNullOrEmpty(path1))
                throw new ArgumentNullException(nameof(path1));

            if (string.IsNullOrEmpty(path2))
                throw new ArgumentNullException(nameof(path2));

            if (!File.Exists(path1))
                return false;

            if (string.Compare(path1, path2, StringComparison.OrdinalIgnoreCase) == 0)
                return true;

            if (!File.Exists(path2))
                return false;

            return GetFileInformation(path1) == GetFileInformation(path2);
        }

        private static string GetPathFromCommandLine(string commandLine)
        {
            if (string.IsNullOrEmpty(commandLine))
                throw new ArgumentNullException(nameof(commandLine));

            commandLine = commandLine.Trim();

            if (commandLine.StartsWith("\""))
            {
                int split = commandLine.IndexOf('\"', 1);

                if (split >= 0)
                {
                    return commandLine.Substring(1, split - 1);
                }
            }
            else
            {
                int split = commandLine.IndexOf(' ');

                if (split >= 0)
                {
                    return commandLine.Substring(0, split);
                }
            }

            return commandLine;
        }

        public static string GetServiceName()
        {
            try
            {
                using (RegistryKey services = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services"))
                {
                    if (services == null)
                        return null;

                    foreach (string subKeyName in services.GetSubKeyNames())
                    {
                        using (RegistryKey service = services.OpenSubKey(subKeyName))
                        {
                            string path;

                            if (service == null) continue;

                            try
                            {
                                string commandLine = service.GetValue("ImagePath") as string;

                                if (string.IsNullOrEmpty(commandLine)) continue;

                                path = GetPathFromCommandLine(commandLine);
                            }
                            catch (Exception ex)
                            {
                                Console.Error.WriteLine(
                                    $"Unable to read ImagePath of service {subKeyName}. {ex}");

                                continue;
                            }

                            try
                            {
                                if (IsSameFile(path, Assembly.GetExecutingAssembly().Location))
                                    return subKeyName;
                            }
                            catch (Exception ex)
                            {
                                Console.Error.WriteLine(
                                    $"Unable to validate file information for service {subKeyName}. {ex}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Unable to identify the name of the current service. {ex}");
            }

            return null;
        }
    }
}
