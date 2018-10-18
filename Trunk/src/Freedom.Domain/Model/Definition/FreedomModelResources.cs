using System;
using System.Data.Entity.Core.Metadata.Edm;
using System.IO;
using System.Reflection;

namespace Freedom.Domain.Model.Definition
{
    public static class FreedomModelResources
    {
        private static string BuildResourceUrl(string resourceName)
        {
            return $"res://{typeof(FreedomModelResources).Assembly.FullName}/" +
                   $"{typeof(FreedomModelResources).Namespace}.{resourceName}";
        }

        private static string GetResource(string resourceName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            string qualifiedName = string.Join(".", typeof(FreedomModelResources).Namespace, resourceName);
            
            Stream stream = assembly.GetManifestResourceStream(qualifiedName);

            if (stream == null) return null;

            using (StreamReader reader = new StreamReader(stream))
                return reader.ReadToEnd();
        }

        private static string GetStorageModelForProvider(string provider)
        {
            switch (provider)
            {
                case "System.Data.SqlClient":
                    provider = provider.Substring(provider.LastIndexOf('.') + 1);
                    break;

                default:
                    throw new InvalidOperationException($"The provider '{provider}' is not supported.");
            }

            return "FreedomStorageModel" + provider + ".ssdl";
        }

        public static string GetMetadataForProvider(string provider)
        {
            string csdl = "FreedomConceptualModel.csdl";
            string ssdl = GetStorageModelForProvider(provider);
            string msl = "FreedomModelMap.msl";

            return string.Join("|", BuildResourceUrl(csdl), BuildResourceUrl(ssdl), BuildResourceUrl(msl));
        }

        public static MetadataWorkspace GetMetadataWorkspaceForProvider(string provider)
        {
            string[] resourceUrls =
            {
                BuildResourceUrl("FreedomConceptualModel.csdl"),
                BuildResourceUrl(GetStorageModelForProvider(provider)),
                BuildResourceUrl("FreedomModelMap.msl")
            };

            Assembly[] assemblies = { Assembly.GetExecutingAssembly() };

            return new MetadataWorkspace(resourceUrls, assemblies);
        }

        public static string CreateDatabaseObjectsScript => GetResource("CreateDatabase.sql");

        public static string CreateServerDatabaseObjectsScript => GetResource("CreateServerDatabase.sql");
                
    }
}
