using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using Freedom.WebApi.Models;
using Freedom.Extensions;
using log4net;

namespace Freedom.WebApi.Infrastructure
{
    public class ApplicationMetadataCache : IApplicationMetadataCache
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private ApplicationMetadata _applicationMetadata;
        private DateTime? _lastWriteDateTimeUtc;

        private const string MetadataFileName = "ApplicationMetadata.xml";

        public ApplicationMetadataCache(string defaultBootstrapClientDirectory)
        {
            if (string.IsNullOrEmpty(defaultBootstrapClientDirectory))
                throw new ArgumentNullException(nameof(defaultBootstrapClientDirectory));

            // if there's a path specified in tha app.Config or web.Config use it over the default
            string rootDirectory = ConfigurationManager.AppSettings[nameof(BootstrapClientDirectory)];

            if (!string.IsNullOrEmpty(rootDirectory))
                BootstrapClientDirectory = rootDirectory;

            BootstrapClientDirectory = defaultBootstrapClientDirectory;

            if (File.Exists(MetadataFilePath))
            {
                Log.Info(
                    $"This server will use the ApplicationMetadata file found at '{MetadataFilePath}'" +
                    " when a Hedgehog client bootstrapper requests an updated client.");
            }
            else
            {
                Log.Info(
                    $"This server could not find an ApplicationMetadata file at it's expected location of '{MetadataFilePath}'. " +
                    "This server will not supply client updates.");
            }
        }

        private string BootstrapClientDirectory { get; }

        private string MetadataFilePath
            => !string.IsNullOrEmpty(BootstrapClientDirectory)
                ? Path.Combine(BootstrapClientDirectory, MetadataFileName)
                : null;

        public ApplicationMetadata GetApplicationMetadata()
        {
            if (_applicationMetadata != null && !HasWriteTimeChanged())
                return _applicationMetadata;

            try
            {
                _applicationMetadata = LoadApplicationMetadata();

                if (CheckMetadata(_applicationMetadata))
                {
                    FileInfo metadataFileInfo = new FileInfo(MetadataFilePath);

                    _lastWriteDateTimeUtc = metadataFileInfo.LastWriteTimeUtc;

                    return _applicationMetadata;
                }
            }
            catch (DirectoryNotFoundException)
            {
                // Fall through and return null.
            }
            catch (FileNotFoundException)
            {
                // Fall through and return null.
            }

            _applicationMetadata = null;
            _lastWriteDateTimeUtc = null;
            return null;
        }

        public string GetLocalFilePath(string fileName)
        {
           return Path.Combine(BootstrapClientDirectory, fileName);
        }

        private bool HasWriteTimeChanged()
        {
            FileInfo metadataFileInfo = new FileInfo(MetadataFilePath);

            if (!metadataFileInfo.Exists)
                return true;

            return _lastWriteDateTimeUtc != metadataFileInfo.LastWriteTimeUtc;
        }

        private ApplicationMetadata LoadApplicationMetadata()
        {
            string filePath = MetadataFilePath;

            DataContractSerializer serializer = new DataContractSerializer(typeof(ApplicationMetadata));

            using (Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                return (ApplicationMetadata)serializer.ReadObject(stream);
        }

        private bool CheckMetadata(ApplicationMetadata metadata)
        {
            bool result = true;

            foreach (FileMetadata file in metadata.Files)
                result &= CheckFile(file);

            return result;
        }

        private bool CheckFile(FileMetadata metadata)
        {
            string filePath = Path.Combine(BootstrapClientDirectory, metadata.Name);

            FileInfo fileInfo = new FileInfo(filePath);

            if (!fileInfo.Exists)
            {
                Log.Warn($"The file '{filePath}' specified in the client metadata could not be found.");
                return false;
            }

            if (metadata.Length > 0)
            {
                if (metadata.Length != fileInfo.Length)
                {
                    Log.Warn($"File length mismatch. The file '{filePath}' has a length of {fileInfo.Length} bytes, but the metadata states a length of {metadata.Length} bytes.");
                    return false;
                }
            }
            else
            {
                metadata.Length = fileInfo.Length;
            }

            metadata.ModifiedDateTime = fileInfo.LastWriteTimeUtc;

            string fileHash = FileUtility.CalculateFileHash(filePath).ToHexString();

            if (string.IsNullOrEmpty(metadata.Hash))
                metadata.Hash = fileHash;
            else if (!string.Equals(fileHash, metadata.Hash, StringComparison.OrdinalIgnoreCase))
            {
                Log.Warn($"File hash mismatch. The file '{filePath}' has a hash of '{fileHash}', but the metadata states it expectes a hash of '{fileHash}");
                return false;
            }

            return true;
        }
    }
}