using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Freedom.MSBuild.Models;

namespace Freedom.MSBuild
{
    public class CreateApplicationMetadata : Task
    {
        private const int BufferSize = 1024 * 1204;

        [Required]
        public string Name { get; set; }

        public string Version { get; set; }

        [Required]
        public ITaskItem OutputFile { get; set; }

        [Required]
        public ITaskItem[] Files { get; set; }

        public override bool Execute()
        {
            ApplicationMetadata metadata = new ApplicationMetadata();

            metadata.Name = Name;
            metadata.Version = Version;

            foreach (ITaskItem taskItem in Files)
            {
                if (File.Exists(taskItem.ItemSpec)) continue;
                Log.LogError($"Unable to build ApplicationMetadata file. The source file '{taskItem.ItemSpec}' was not found.");
                return false;
            }

            foreach (ITaskItem item in Files)
            {
                metadata.Files.Add(GetFileMetadata(item));
            }

            if (metadata.Files.Count > 0)
                metadata.LaunchTarget = metadata.Files[0].Name;

            DataContractSerializer serializer = new DataContractSerializer(typeof(ApplicationMetadata));
            XmlWriterSettings settings = new XmlWriterSettings { Indent = true };

            using (FileStream fileStream = new FileStream(OutputFile.ItemSpec, FileMode.Create, FileAccess.ReadWrite, FileShare.None, BufferSize))
            using (StreamWriter streamWriter = new StreamWriter(fileStream))
            using (XmlWriter xmlWriter = XmlWriter.Create(streamWriter, settings))
                serializer.WriteObject(xmlWriter, metadata);

            return true;
        }

        private static FileMetadata GetFileMetadata(ITaskItem item)
        {
            FileInfo fileInfo = new FileInfo(item.ItemSpec);

            bool excludeHash = GetMetadataAsBoolean(item, "ExcludeHash", false);

            FileMetadata metadata = new FileMetadata();

            metadata.Name = fileInfo.Name;

            if (!excludeHash)
            {
                metadata.Hash = GetFileHash(fileInfo.FullName);
                metadata.Length = fileInfo.Length;
                metadata.Version = TryGetFileVersion(fileInfo.FullName);
            }

            return metadata;
        }

        private static bool GetMetadataAsBoolean(ITaskItem item, string metadataName, bool defaultValue)
        {
            try
            {
                bool result;

                return bool.TryParse(item.GetMetadata(metadataName), out result) ? result : defaultValue;
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        private static string TryGetFileVersion(string filePath)
        {
            try
            {
                FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(filePath);

                return versionInfo.FileVersion;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string GetFileHash(string filePath)
        {
            SHA256 sha256 = new SHA256Managed();

            using (Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, BufferSize))
            {
                byte[] hash = sha256.ComputeHash(stream);

                return BitConverter.ToString(hash).Replace("-", string.Empty);
            }
        }
    }
}
