namespace Freedom.Domain.Infrastructure.Reports
{
    public enum ErrorCode
    {
        Success = 0,

        HelpRequested = 1,

        InsufficientCommandLineParameters = 2,

        TooManyCommandLineParameters = 3,

        InvalidReportJobId =  4,

        UnexpectedException = 5,

        ReportJobNotFound = 6,

        ReportNotFound = 7,

        ReportTooLarge = 8
    }
}
