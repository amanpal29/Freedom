namespace Freedom.Domain.Infrastructure.ExceptionHandling
{
    /// <summary>
    /// The handled status of an exception
    /// </summary>
    public enum ExceptionHandledStatus
    {
        /// <summary>
        /// The exception was not handled
        /// </summary>
        Unhandled,

        /// <summary>
        /// The exception was handled and no further action is needed
        /// </summary>
        Handled,

        /// <summary>
        /// The user requested that the operation be retried
        /// </summary>
        Retry,

        /// <summary>
        /// The user wants to cancel the operation that caused the exception
        /// </summary>
        Cancel
    }
}
