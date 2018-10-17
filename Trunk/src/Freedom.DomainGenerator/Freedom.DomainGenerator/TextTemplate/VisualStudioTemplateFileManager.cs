using EnvDTE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Freedom.DomainGenerator.TextTemplate
{
    public class VisualStudioTemplateFileManager : TemplateFileManager
    {
        private readonly ProjectItem _templateProjectItem;
        private readonly DTE _dte;
        private readonly Action<string> _checkOutAction;
        private readonly Action<ICollection<string>> _projectSyncAction;

        /// <summary>
        /// Creates an instance of the VisualStudioTemplateFileManager class with the IDynamicHost instance
        /// </summary>
        public VisualStudioTemplateFileManager(object textTemplating)
            : base(textTemplating)
        {
            IServiceProvider hostServiceProvider = TextTransformation.Host.AsIServiceProvider();

            if (hostServiceProvider == null)
                throw new InvalidOperationException("Could not obtain hostServiceProvider");

            _dte = (DTE)hostServiceProvider.GetService(typeof(DTE));

            if (_dte == null)
                throw new InvalidOperationException("Could not obtain DTE from host");

            _templateProjectItem = _dte.Solution.FindProjectItem(TextTransformation.Host.TemplateFile);

            _checkOutAction = fileName => _dte.SourceControl.CheckOutItem(fileName);
            _projectSyncAction = keepFileNames => ProjectSync(_templateProjectItem, keepFileNames);
        }

        public override ICollection<string> Process(bool split = true)
        {
            if (_templateProjectItem.ProjectItems == null)
            {
                return new List<string>();
            }

            ICollection<string> generatedFileNames = base.Process(split);

            _projectSyncAction.EndInvoke(_projectSyncAction.BeginInvoke(generatedFileNames, null, null));

            return generatedFileNames;
        }

        protected override void CreateFile(string fileName, string content)
        {
            if (IsFileContentDifferent(fileName, content))
            {
                CheckoutFileIfRequired(fileName);
                File.WriteAllText(fileName, content);
            }
        }

        private static void ProjectSync(ProjectItem templateProjectItem, ICollection<string> keepFileNames)
        {
            HashSet<string> keepFileNameSet = new HashSet<string>(keepFileNames);

            string originalOutput = Path.GetFileNameWithoutExtension(templateProjectItem.FileNames[0]);

            Dictionary<string, ProjectItem> projectFiles =
                templateProjectItem.ProjectItems.Cast<ProjectItem>()
                    .ToDictionary(projectItem => projectItem.FileNames[0]);

            // Remove unused items from the project
            foreach (KeyValuePair<string, ProjectItem> pair in projectFiles)
            {
                if (!keepFileNames.Contains(pair.Key)
                    && !(Path.GetFileNameWithoutExtension(pair.Key) + ".").StartsWith(originalOutput + "."))
                {
                    pair.Value.Delete();
                }
            }

            // Add missing files to the project
            foreach (string fileName in keepFileNameSet)
            {
                if (!projectFiles.ContainsKey(fileName))
                {
                    templateProjectItem.ProjectItems.AddFromFile(fileName);
                }
            }
        }

        private void CheckoutFileIfRequired(string fileName)
        {
            if (_dte.SourceControl == null
                || !_dte.SourceControl.IsItemUnderSCC(fileName)
                || _dte.SourceControl.IsItemCheckedOut(fileName))
            {
                return;
            }

            // run on worker thread to prevent T4 calling back into VS
            _checkOutAction.EndInvoke(_checkOutAction.BeginInvoke(fileName, null, null));
        }
    }
}