using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using Microsoft.Build.Framework;

namespace Freedom.MSBuild
{
    public class ExecuteSql : DatabaseBuilderTask
    {
        /// <summary>
        /// Gets or sets sql files to execute.
        /// </summary>
        /// <value>The files.</value>
        public ITaskItem[] Files { get; set; }

        #region Overrides of Task

        public override bool Execute()
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                List<string> scripts = new List<string>();

                foreach (ITaskItem taskItem in Files)
                {
                    Log.LogMessage("Reading Script File '{0}'.", taskItem.ItemSpec);
                    scripts.Add(File.ReadAllText(taskItem.ItemSpec));
                }

                string[] batches = string.Join("\r\nGO\r\n", scripts).Split(new[] {"\r\nGO\r\n"}, StringSplitOptions.RemoveEmptyEntries);

                foreach (string batch in batches)
                {
                    Log.LogMessage(batch);

                    IDbCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = batch;
                    command.ExecuteNonQuery();
                }
            }

            return true;
        }

        #endregion
    }
}
