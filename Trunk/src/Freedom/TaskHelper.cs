using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Freedom
{
    public static class TaskHelper
    {
        /// <summary>
        /// Like Task.WhenAny() but returns null if none of the tasks have completed before the specified timeout.
        /// </summary>
        public static Task<Task<T>> WhenAny<T>(IEnumerable<Task<T>> tasks, TimeSpan timeout)
        {
            return WhenAny(tasks, timeout, CancellationToken.None);
        }

        /// <summary>
        /// Like Task.WhenAny() but returns null if none of the tasks have completed before the specified timeout.
        /// </summary>
        public static async Task<Task<T>> WhenAny<T>(IEnumerable<Task<T>> tasks, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (tasks == null)
                throw new ArgumentNullException(nameof(tasks));

            if (timeout <= TimeSpan.Zero)
                throw new ArgumentException("Timeout must be greater than zero.", nameof(timeout));

            cancellationToken.ThrowIfCancellationRequested();

            List<Task> taskList = new List<Task>();

            foreach (Task<T> task in tasks)
            {
                if (task.IsCompleted)
                    return task;

                taskList.Add(task);
            }

            if (taskList.Count == 0)
                return null;

            Task delayTask = Task.Delay(timeout, cancellationToken);

            taskList.Add(delayTask);

            Task firstCompleted = await Task.WhenAny(taskList);

            return firstCompleted.Id != delayTask.Id ? (Task<T>)firstCompleted : null;
        }
    }
}
