using UnityEngine;


namespace SOSXR.EnhancedLogger
{
    /// <summary>
    ///     ScriptableObject that stores all configurable settings for the EnhancedLogger.
    ///     Includes the active log level, per-level prefix strings, per-level colors, and the file-logging toggle.
    ///     Create or locate this asset via <c>SOSXR &gt; EnhancedLogger &gt; EnhancedLoggerSettings</c>, or let it be
    ///     generated automatically in <c>Assets/_SOSXR/Resources/</c> on first use.
    /// </summary>
    [CreateAssetMenu(fileName = "EnhancedLoggerSettings", menuName = "SOSXR/EnhancedLogger/EnhancedLoggerSettings", order = 1)]
    public class EnhancedLoggerSettings : ScriptableObject
    {
        /// <summary>The minimum log level required for a message to appear in the console. Messages below this level are suppressed.</summary>
        [Header("Log Level")] public LogLevel CurrentLogLevel = LogLevel.Info;

        /// <summary>Prefix prepended to Error-level log messages (default: <c>[ERROR]</c>).</summary>
        [Header("Prefixes")] public string ErrorPrefix = "[ERROR]";

        /// <summary>Prefix prepended to Warning-level log messages (default: <c>WARNING</c>).</summary>
        public string WarningPrefix = "WARNING";

        /// <summary>Prefix prepended to Debug-level log messages (default: <c>=DEBUG=</c>).</summary>
        public string DebugPrefix = "=DEBUG=";

        /// <summary>Prefix prepended to Info-level log messages (default: <c>INFORM:</c>).</summary>
        public string InfoPrefix = "INFORM:";

        /// <summary>Prefix prepended to Success-level log messages (default: <c>SUCCESS</c>).</summary>
        public string SuccessPrefix = "SUCCESS";

        /// <summary>Prefix prepended to Verbose-level log messages (default: <c>VERBOSE</c>).</summary>
        public string VerbosePrefix = "VERBOSE";

        /// <summary>Console color applied to Error-level log prefixes (default: soft red).</summary>
        [Header("Colors")] public Color ErrorColor = Color.softRed;

        /// <summary>Console color applied to Warning-level log prefixes (default: orange).</summary>
        public Color WarningColor = Color.orange;

        /// <summary>Console color applied to Debug-level log prefixes (default: cornflower blue).</summary>
        public Color DebugColor = Color.cornflowerBlue;

        /// <summary>Console color applied to Info-level log prefixes (default: khaki).</summary>
        public Color InfoColor = Color.khaki;

        /// <summary>Console color applied to Success-level log prefixes (default: soft green).</summary>
        public Color SuccessColor = Color.softGreen;

        /// <summary>Console color applied to Verbose-level log prefixes (default: hot pink).</summary>
        public Color VerboseColor = Color.hotPink;

        /// <summary>When <c>true</c>, log messages are also written to a Markdown file in <c>Application.persistentDataPath/EnhancedLogger/</c> on application quit.</summary>
        [Header("File Logging")] public bool WriteToFile = true;

    }
}