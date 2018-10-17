namespace Freedom.BackgroundWorker
{
    public enum JobState
    {
        Queued,
        InProgress,
        Completed,
        Canceled,
        Failed
    }
}