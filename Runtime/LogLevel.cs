namespace SOSXR.EnhancedLogger
{
    /// <summary>
    ///     Defines the severity tiers used by <see cref="Log"/> to filter console output.
    ///     The default active level is <see cref="Info"/> (set in <see cref="EnhancedLoggerSettings"/>).
    ///     Each level also shows all levels with a lower numeric value (i.e. higher urgency).
    /// </summary>
    public enum LogLevel
    {
        /// <summary>Suppresses all log output.</summary>
        None,

        /// <summary>Catastrophic errors that break the game. Lowest active priority — always shown unless <see cref="None"/> is set.</summary>
        Error,

        /// <summary>Severe issues that should not be ignored. Also shows <see cref="Error"/> logs.</summary>
        Warning,

        /// <summary>Temporary development messages. Use during active module development; clean up before merging. Also shows <see cref="Warning"/> and below.</summary>
        Debug,

        /// <summary>General informational messages about expected application behavior. Also shows <see cref="Debug"/> and below.</summary>
        Info,

        /// <summary>Confirmation of successful operations or state transitions. Also shows <see cref="Info"/> and below.</summary>
        Success,

        /// <summary>Highly detailed trace-level messages. Shows all log levels. Use sparingly to avoid console noise.</summary>
        Verbose
    }
}