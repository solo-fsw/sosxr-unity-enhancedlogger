using UnityEngine;

namespace SOSXR.EnhancedLogger
{
    /// <summary>
    ///     ScriptableObject that stores all configurable settings for the EnhancedLogger.
    ///     Includes the active log level, per-level prefix strings, per-level colors, and the file-logging toggle.
    ///     Edit it in <c>Preferences &gt; SOSXR &gt; EnhancedLogger</c>. Create it via <c>SOSXR &gt; EnhancedLogger &gt; EnhancedLoggerSettings</c>, or let it be
    ///     generated automatically at <c>Assets/_SOSXR/Settings/EnhancedLoggerSettings.asset</c> on first use.
    ///     Builds add the asset to the Preloaded Assets in Player Settings, so players load it without a Resources folder.
    /// </summary>
    [CreateAssetMenu(
        fileName = "EnhancedLoggerSettings",
        menuName = "SOSXR/EnhancedLogger/EnhancedLoggerSettings",
        order = 1
    )]
    public class EnhancedLoggerSettings : ScriptableObject
    {
        /// <summary>The minimum log level required for a message to appear in the console. Messages below this level are suppressed.</summary>
        [Header("Log Level")]
        public LogLevel CurrentLogLevel = LogLevel.Info;

        /// <summary>Prefix prepended to Error-level log messages (default: <c>ERROR  </c>).</summary>
        [Header("Prefixes")]
        public string ErrorPrefix = "ERROR  ";

        /// <summary>Prefix prepended to Warning-level log messages (default: <c>WARNING</c>).</summary>
        public string WarningPrefix = "WARNING";

        /// <summary>Prefix prepended to Debug-level log messages (default: <c>DEBUG  </c>).</summary>
        public string DebugPrefix = "DEBUG  ";

        /// <summary>Prefix prepended to Info-level log messages (default: <c>INFORM </c>).</summary>
        public string InfoPrefix = "INFORM ";

        /// <summary>Prefix prepended to Success-level log messages (default: <c>SUCCESS</c>).</summary>
        public string SuccessPrefix = "SUCCESS";

        /// <summary>Prefix prepended to Verbose-level log messages (default: <c>VERBOSE</c>).</summary>
        public string VerbosePrefix = "VERBOSE";

        // The defaults are written as RGB values, not as Color.softRed etc.: those presets exist only from Unity 6000.2,
        // and this package supports 6000.0. The values are the same as the presets.

        /// <summary>Console color applied to Error-level log prefixes (default: soft red).</summary>
        [Header("Colors")]
        public Color ErrorColor = new(0.8627452f, 0.1921569f, 0.1960784f, 1f);

        /// <summary>Console color applied to Warning-level log prefixes (default: orange).</summary>
        public Color WarningColor = new(1f, 0.6470588f, 0f, 1f);

        /// <summary>Console color applied to Debug-level log prefixes (default: cornflower blue).</summary>
        public Color DebugColor = new(0.3921569f, 0.5843138f, 0.9294118f, 1f);

        /// <summary>Console color applied to Info-level log prefixes (default: khaki).</summary>
        public Color InfoColor = new(0.9411765f, 0.9019608f, 0.5490196f, 1f);

        /// <summary>Console color applied to Success-level log prefixes (default: soft green).</summary>
        public Color SuccessColor = new(0.5490196f, 0.7882354f, 0.1411765f, 1f);

        /// <summary>Console color applied to Verbose-level log prefixes (default: hot pink).</summary>
        public Color VerboseColor = new(1f, 0.4117647f, 0.7058824f, 1f);

        /// <summary>When <c>true</c>, each log message is appended to a session Markdown file in <c>Application.persistentDataPath/EnhancedLogger/</c>.</summary>
        [Header("File Logging")]
        public bool WriteToFile = true;

        /// <summary>
        ///     The first settings instance that was enabled. In players this is the Preloaded Asset, which Unity loads at startup.
        ///     <see cref="Log" /> uses it in players only; the Editor loads the asset from the project.
        /// </summary>
        internal static EnhancedLoggerSettings Instance { get; private set; }

        private void OnEnable()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        private void OnDisable()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
