using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Threading;
using Freedom.Collections;
using log4net;
using ThreadState = System.Threading.ThreadState;

namespace Freedom.BackgroundWorker
{
    public class JobQueue : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [ThreadStatic]
        private static Job _currentJob;  // current job of the current thread

        private readonly ConcurrentQueue<Job> _workItemsQueue = new ConcurrentQueue<Job>();
        private readonly ConcurrentHashSet<Thread> _workerThreads = new ConcurrentHashSet<Thread>();

        private readonly string _name;
        private readonly int _maxWorkerThreads = Environment.ProcessorCount;

        private readonly ManualResetEvent _idleEvent = new ManualResetEvent(true);

        private int _jobCount;
        private readonly object _jobCountLock = new object();

        private int _threadIdentity;

        public JobQueue()
            : this(null, Thread.CurrentThread.Priority)
        {
            
        }

        public JobQueue(string name)
            : this(name, Thread.CurrentThread.Priority)
        {
        }

        public JobQueue(string name, ThreadPriority threadPriority)
        {
            _name = name ?? string.Empty;
            ThreadPriority = threadPriority;
        }

        public void Enqueue(Job job)
        {
            if (job == null)
                throw new ArgumentNullException(nameof(job));

            if (job.State != JobState.Queued)
                throw new ArgumentException("The job has already been executed.", nameof(job));

            Log.DebugFormat("Enqueuing job {0} for execution in queue {1}", job, _name);

            job.JobQueue = this;

            lock (_jobCountLock)
            {
                if (++_jobCount == 1)
                    _idleEvent.Reset();
            }

            _workItemsQueue.Enqueue(job);

            if (_jobCount > _workerThreads.Count)
                StartNewThread();
        }

        private void StartNewThread()
        {
            lock (_workerThreads.SyncRoot)
            {
                if (IsShuttingDown)
                    return;

                if (_workerThreads.Count >= _maxWorkerThreads)
                    return;

                Thread workerThread = new Thread(ProcessQueuedItems)
                {
                    Name = _name + " Thread #" + ++_threadIdentity,
                    IsBackground = true,
                    Priority = ThreadPriority
                };

                _workerThreads.Add(workerThread);

                workerThread.Start();
            }
        }

        public ThreadPriority ThreadPriority { get; }

        public bool IsShuttingDown { get; private set; }

        // This is the main process of a worker thread...
        private void ProcessQueuedItems()
        {
            try
            {
                while (!IsShuttingDown)
                {
                    // This is the normal thread exit scenario, when there are no more jobs in the queue.
                    // Double-check locking is employed to ensure that if another job is added while we're
                    // shutting down another thread gets started.

                    if (!_workItemsQueue.TryDequeue(out _currentJob))
                    {
                        lock (_workerThreads.SyncRoot)
                        {
                            if (!_workItemsQueue.TryDequeue(out _currentJob))
                            {
                                _workerThreads.Remove(Thread.CurrentThread);

                                break;
                            }
                        }
                    }

                    Debug.Assert(_currentJob != null);

                    try
                    {
                        if (_currentJob.IsCancelling)
                        {
                            _currentJob.State = JobState.Canceled;
                        }
                        else
                        {
                            _currentJob.State = JobState.InProgress;

                            bool result = _currentJob.Execute();

                            _currentJob.State = result ? JobState.Completed : JobState.Canceled;
                        }
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (Exception exception)
                    {
                        if (Thread.CurrentThread.ThreadState.HasFlag(ThreadState.AbortRequested))
                        {
                            // If an abort was requested, this exception probably wraps a ThreadAbortException

                            // if so, find the ThreadAbortException and throw it instead
                            Exception threadAbortException = exception;

                            while (threadAbortException != null)
                            {
                                threadAbortException = threadAbortException.InnerException;

                                if (threadAbortException is ThreadAbortException)
                                    throw threadAbortException;
                            }

                            // if not, we need to at least break out of the while loop...
                            break;
                        }

                        // Otherwise, just log the error and move on to the next job....

                        _currentJob.Error = exception;
                        _currentJob.State = JobState.Failed;

                        Log.Warn($"Job {_currentJob} failed in queue {_name}", _currentJob.Error);
                    }
                    finally
                    {
                        (_currentJob as IDisposable)?.Dispose();

                        lock (_jobCountLock)
                        {
                            if (--_jobCount == 0)
                                _idleEvent.Set();
                        }
                    }
                }
            }
            catch (ThreadAbortException)
            {
                Thread.ResetAbort();
            }
            catch (Exception exception)
            {
                Log.Error("Unexpected exception on JobQueue thread", exception);
            }
            finally
            {
                _workerThreads.Remove(Thread.CurrentThread);
            }
        }

        public bool WaitForIdle(TimeSpan timeout)
        {
            return _idleEvent.WaitOne(timeout);
        }

        public void ShutDown(TimeSpan timeout)
        {
            IsShuttingDown = true;

            foreach (Job job in _workItemsQueue)
                job.Cancel();

            Thread[] threads;

            // signal the shutdown, cancel jobs that haven't started yet, and make a copy of the collection of threads

            lock (_workerThreads.SyncRoot)
            {
                threads = new Thread[_workerThreads.Count];

                _workerThreads.CopyTo(threads, 0);
            }

            // wait for the specified period of time for the threads to end on their own

            long ticksLeft = timeout.Ticks;
            
            foreach (Thread thread in threads)
            {
                long startTicks = DateTime.UtcNow.Ticks;

                thread.Join(new TimeSpan(ticksLeft));

                long elapsedTicks = DateTime.UtcNow.Ticks - startTicks;

                ticksLeft -= elapsedTicks;

                if (ticksLeft < 0) break;
            }

            // Forceably abort any thread that's still running...

            foreach (Thread thread in threads.Where(thread => thread.IsAlive))
            {
                try
                {
                    thread.Abort();
                }
                catch (SecurityException exception)
                {
                    Log.Warn("Unable to forceably abort a worker thread.", exception);
                }
                catch (ThreadStateException)
                {
                    // Happens if the thread dies between the thread.IsAlive check and the thread.Abort()
                    // Since killing the thread is what we wanted to happen anyway... just ignore it.
                }
            }
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            ShutDown(new TimeSpan(0, 0, 30));

            _idleEvent.Dispose();
        }

        #endregion
    }
}
