using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Freedom.MSBuild
{
    public class ReadXmlFile : Task
    {
        [Required]
        public ITaskItem File { get; set; }

        [Required]
        public string XPath { get; set; }

        public string Directory { get; set; }

        public string Namespaces { get; set; }

        [Output]
        public ITaskItem[] Elements { get; set; }

        public override bool Execute()
        {
            if (!System.IO.File.Exists(File.ItemSpec))
            {
                Log.LogError($"File not found: {File.ItemSpec}");
                return false;
            }

            XmlDocument xmlDocument = new XmlDocument();

            xmlDocument.Load(File.ItemSpec);

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDocument.NameTable);

            if (!string.IsNullOrEmpty(Namespaces))
            {
                string[] pairs = Namespaces.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);

                foreach (string pair in pairs)
                {
                    int equalSign = pair.IndexOf('=');

                    if (equalSign < 0)
                    {
                        Log.LogWarning("The namespace declaration {0} is not valid.", pair);
                        continue;
                    }

                    nsmgr.AddNamespace(pair.Substring(0, equalSign), pair.Substring(equalSign + 1));
                }
            }

            XmlNodeList nodes = xmlDocument.SelectNodes(XPath, nsmgr);
            
            if (nodes == null)
                return false;

            List<ITaskItem> elements = new List<ITaskItem>();

            foreach (object node in nodes)
            {
                string value = null;

                XmlElement xmlElement = node as XmlElement;
                XmlAttribute xmlAttribute = node as XmlAttribute;

                if (xmlElement != null)
                {
                    value = xmlElement.InnerText;
                }
                else if (xmlAttribute != null)
                {
                     value = xmlAttribute.Value;
                }

                if (string.IsNullOrWhiteSpace(value)) continue;

                value = value.Trim();

                if (!string.IsNullOrEmpty(Directory))
                    value = Path.Combine(Directory, Path.GetFileName(value));

                elements.Add(new TaskItem(value));
            }

            Elements = elements.ToArray();

            return true;
        }
    }
}
