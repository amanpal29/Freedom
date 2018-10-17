using EnvDTE;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Freedom.DomainGenerator.TextTemplate
{
    /// <summary>
    /// Responsible for marking the various sections of the generation,
    /// so they can be split up into separate files
    /// </summary>
    public class TemplateFileManager
    {
        /// <summary>
        /// Creates the VisualStudioTemplateFileManager if VS is detected, otherwise
        /// creates the file system version.
        /// </summary>
        public static TemplateFileManager Create(object textTransformation)
        {
            DynamicTextTransformation transformation = DynamicTextTransformation.Create(textTransformation);

            IServiceProvider hostServiceProvider = transformation.Host.AsIServiceProvider();

            return hostServiceProvider?.GetService(typeof(DTE)) is DTE
                ? new VisualStudioTemplateFileManager(transformation)
                : new TemplateFileManager(transformation);
        }

        private sealed class Block
        {
            public string Name;
            public int Start, Length;
        }

        private readonly List<Block> _files = new List<Block>();
        private readonly Block _footer = new Block();
        private readonly Block _header = new Block();

        protected readonly DynamicTextTransformation TextTransformation;

        // reference to the GenerationEnvironment StringBuilder on the
        // TextTransformation object
        private readonly StringBuilder _generationEnvironment;

        private Block _currentBlock;

        /// <summary>
        /// Initializes an TemplateFileManager Instance  with the
        /// TextTransformation (T4 generated class) that is currently running
        /// </summary>
        protected TemplateFileManager(object textTransformation)
        {
            if (textTransformation == null)
            {
                throw new ArgumentNullException(nameof(textTransformation));
            }

            TextTransformation = DynamicTextTransformation.Create(textTransformation);
            _generationEnvironment = TextTransformation.GenerationEnvironment;
        }

        /// <summary>
        /// Marks the end of the last file if there was one, and starts a new
        /// and marks this point in generation as a new file.
        /// </summary>
        public void StartNewFile(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            CurrentBlock = new Block { Name = name };
        }

        public void StartFooter()
        {
            CurrentBlock = _footer;
        }

        public void StartHeader()
        {
            CurrentBlock = _header;
        }

        public void EndBlock()
        {
            if (CurrentBlock == null)
            {
                return;
            }

            CurrentBlock.Length = _generationEnvironment.Length - CurrentBlock.Start;

            if (CurrentBlock != _header && CurrentBlock != _footer)
            {
                _files.Add(CurrentBlock);
            }

            _currentBlock = null;
        }

        /// <summary>
        /// Produce the template output files.
        /// </summary>
        public virtual ICollection<string> Process(bool split = true)
        {
            List<string> generatedFileNames = new List<string>();

            if (split)
            {
                EndBlock();

                string headerText = _generationEnvironment.ToString(_header.Start, _header.Length);
                string footerText = _generationEnvironment.ToString(_footer.Start, _footer.Length);

                string outputPath = Path.GetDirectoryName(TextTransformation.Host.TemplateFile);

                Debug.Assert(outputPath != null, "outputPath != null");

                _files.Reverse();

                foreach (Block block in _files)
                {

                    string fileName = Path.Combine(outputPath, block.Name);
                    string content = headerText + _generationEnvironment.ToString(block.Start, block.Length) + footerText;

                    generatedFileNames.Add(fileName);
                    CreateFile(fileName, content);
                    _generationEnvironment.Remove(block.Start, block.Length);
                }
            }

            return generatedFileNames;
        }

        protected virtual void CreateFile(string fileName, string content)
        {
            if (IsFileContentDifferent(fileName, content))
                File.WriteAllText(fileName, content);
        }

        protected bool IsFileContentDifferent(string fileName, string newContent)
        {
            return !File.Exists(fileName) || File.ReadAllText(fileName) != newContent;
        }

        private Block CurrentBlock
        {
            get { return _currentBlock; }
            set
            {
                if (CurrentBlock != null)
                    EndBlock();

                if (value != null)
                    value.Start = _generationEnvironment.Length;

                _currentBlock = value;
            }
        }
    }
}
