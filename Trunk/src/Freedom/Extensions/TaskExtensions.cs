using System;
using System.Threading;
using System.Threading.Tasks;

namespace Freedom.Extensions
{
    public static class TaskExtensions
    {
        #region Asynchronous Programming Model

        /// <summary>
        /// These extension methods allow you to implement the older Asynchronous Programming Model methods
        /// (i.e. the Begin/End pattern) for any Task-based Asynchronous Pattern method.
        /// 
        /// Adapted from the methods provided here:
        /// http://blogs.msdn.com/b/pfxteam/archive/2011/06/27/using-tasks-to-implement-the-apm-pattern.aspx
        /// </summary>

        // Usage example:
        // 
        // public async Task<int> ReadAsync(byte[] buffer, int offset, int count) { ... }
        // 
        // public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        // {
        //     return ReadAsync(buffer, offset, count).Begin(callback, state);
        // }
        //
        // public override int EndRead(IAsyncResult ar)
        // {
        //    return ar.End<int>();
        // }

        public static Task<TResult> Begin<TResult>(this Task<TResult> task, AsyncCallback callback, object state)
        {
            // Shortcuts for specific use cases:
            // If the object state provided by the caller is the same state that stored in the supplied task
            // (e.g. if the task was created with an overload of Task.Factory.StartNew that accepts object state),
            // then there's no reason to create a new task. And in that case, if there is no callback provided, 
            // there’s no reason to have a continuation at all.
            if (task.AsyncState == state)
            {
                if (callback != null)
                {
                    task.ContinueWith(delegate { callback(task); }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);
                }

                return task;
            }

            TaskCompletionSource<TResult> taskCompletionSource = new TaskCompletionSource<TResult>(state);

            task.ContinueWith(delegate
            {
                if (task.IsFaulted && task.Exception != null)
                    taskCompletionSource.TrySetException(task.Exception.InnerExceptions);
                else if (task.IsCanceled)
                    taskCompletionSource.TrySetCanceled();
                else
                    taskCompletionSource.TrySetResult(task.Result);

                callback?.Invoke(taskCompletionSource.Task);
            }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);

            return taskCompletionSource.Task;
        }

        public static TResult End<TResult>(this IAsyncResult task)
        {
            if (!(task is Task<TResult>))
                throw new ArgumentException($"Expected task to be a Task<{typeof(TResult).Name}>.", nameof(task));

            try
            {
                return ((Task<TResult>)task).Result;
            }
            catch (AggregateException ae)
            {
                throw ae.InnerException;
            }
        }

        #endregion
    }
}
