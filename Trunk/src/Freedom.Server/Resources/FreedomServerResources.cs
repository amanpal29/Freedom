using System.IO;
using System.Reflection;

namespace Freedom.Server.Resources
{
    public static class FreedomServerResources
    {
        private static Stream GetResourceStream(string resourceName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            string qualifiedName = string.Join(".", typeof (FreedomServerResources).Namespace, resourceName);

            return assembly.GetManifestResourceStream(qualifiedName);
        }

        private static string GetResourceString(string resourceName)
        {
            using (StreamReader reader = new StreamReader(GetResourceStream(resourceName)))
                return reader.ReadToEnd();
        }

        public static string CommandLineHelp => GetResourceString("CommandLineHelp.txt");
    }
}
