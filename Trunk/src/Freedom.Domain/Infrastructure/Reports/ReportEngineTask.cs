namespace Freedom.Domain.Infrastructure.Reports
{
    public enum ReportEngineTask
    {
        Unknown,

        Initializing,

        LoadingReportTemplate,

        LoadingData,

        WaitingForAttachments,

        GeneratingPages,

        Exporting
    }
}