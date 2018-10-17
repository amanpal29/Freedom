using System;

namespace Freedom.BackgroundWorker
{
    public abstract class Job
    {
        private bool _isCancelling;
        private JobQueue _jobQueue;

        protected Job()
        {
            State = JobState.Queued;
        }

        public JobState State { get; internal set; }

        public Exception Error { get; internal set; }

        public JobQueue JobQueue
        {
            get { return _jobQueue; }
            internal set
            {
                if (_jobQueue != null && _jobQueue != value)
                    throw new JobQueueException("This job is already in a job queue.");

                _jobQueue = value;    
            }
        }

        protected internal abstract bool Execute();

        internal void Cancel()
        {
            _isCancelling = true;
        }

        public bool IsCancelling => _isCancelling || (_jobQueue != null && _jobQueue.IsShuttingDown);
    }
}