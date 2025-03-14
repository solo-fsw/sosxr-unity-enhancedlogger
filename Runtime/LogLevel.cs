namespace SOSXR.EnhancedLogger
{
    /// <summary>
    ///     Debug is by default the shown log level in the Editor and Development Build, thereby including also Error and
    ///     Warning logs.
    /// </summary>
    public enum LogLevel
    {
        None,
        Error,
        Warning,
        Info,
        Debug,
        Success,
        Verbose
    }
}