using Microsoft.Build.Framework;
using System;
using System.Collections;
using log4net;
using System.Reflection;

namespace Freedom.MSBuild.Infrastructure
{
    public class FreedomBuildEngine : IBuildEngine
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public int ColumnNumberOfTaskNode
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool ContinueOnError
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int LineNumberOfTaskNode
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string ProjectFileOfTaskNode
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool BuildProjectFile(string projectFileName, string[] targetNames, IDictionary globalProperties, IDictionary targetOutputs)
        {
            throw new NotImplementedException();
        }

        public void LogCustomEvent(CustomBuildEventArgs e)
        {
            Log.Info(e.Message);            
        }

        public void LogErrorEvent(BuildErrorEventArgs e)
        {
            Log.Error(e.Message);
        }

        public void LogMessageEvent(BuildMessageEventArgs e)
        {
            Log.Info(e.Message);
        }

        public void LogWarningEvent(BuildWarningEventArgs e)
        {
            Log.Warn(e.Message);
        }
    }
}
