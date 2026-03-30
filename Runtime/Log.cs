using System.IO;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace SOSXR.EnhancedLogger
{
    /// <summary>
    ///     This is a simple logger that can be used to log different types of messages
    ///     It allows you to control the output of the logs in the editor and in the development build,
    ///     while also allowing me to persist the log level between editor sessions.
    ///     Change the current log level in the Scene view, the Logging menu, with the DevelopmentBuildLogLevelSetter.cs, or
    ///     even by calling Log.CurrentLogLevel = LogLevel.XXXX from anywhere in your code.
    ///     Logs are not shown in the release build, thereby hopefully reducing the cost of logging.
    ///     Adapted from DrowsyFoxDev: https://www.youtube.com/watch?v=lRqR4YF8iQs
    /// </summary>
    public static class Log
    {
        /// <summary>
        ///     Gets or sets the active log level, which controls which messages appear in the console.
        ///     Changes persist between Play mode and Edit mode via the <see cref="EnhancedLoggerSettings"/> asset.
        ///     Logs at or above this level are shown; lower-priority levels are suppressed.
        ///     Defaults to <see cref="LogLevel.Info"/> if settings asset is not available.
        /// </summary>
        public static LogLevel CurrentLogLevel
        {
            get
            {
                LoadSettingsAsset();

                return EnhancedLoggerSettings != null
                    ? EnhancedLoggerSettings.CurrentLogLevel
                    : LogLevel.Info;
            }
            set
            {
                LoadSettingsAsset();

                if (EnhancedLoggerSettings != null)
                {
                    EnhancedLoggerSettings.CurrentLogLevel = value;
                }
            }
        }

        /// <summary>
        ///     Reference to the settings asset that stores log level, prefixes, colors, and file-logging options.
        ///     Loaded automatically from the Resources folder on first use. If the asset does not exist,
        ///     it is created at <c>Assets/_SOSXR/Resources/EnhancedLoggerSettings.asset</c>.
        /// </summary>
        public static EnhancedLoggerSettings EnhancedLoggerSettings;

        private static void LoadSettingsAsset()
        {
            if (EnhancedLoggerSettings)
            {
                return;
            }

            // Try to load from Resources folder first
            EnhancedLoggerSettings = Resources.Load<EnhancedLoggerSettings>(
                nameof(EnhancedLoggerSettings)
            );

            // Fallback: if the settings asset doesn't exist, create it automatically.
            // This ensures the logger always has a valid configuration, even on first use.
            // The asset is created in Assets/_SOSXR/Resources/ so it can be loaded at runtime via Resources.Load().
            if (!EnhancedLoggerSettings)
            {
#if UNITY_EDITOR
                var settingsFolder = "Assets/_SOSXR/Resources";
                var assetPath = $"{settingsFolder}/{nameof(EnhancedLoggerSettings)}.asset";

                // Create Resources folder if it doesn't exist
                if (!Directory.Exists(settingsFolder))
                {
                    Directory.CreateDirectory(settingsFolder);
                }

                // Create the settings asset if it doesn't exist, in the settingsPath
                EnhancedLoggerSettings = ScriptableObject.CreateInstance<EnhancedLoggerSettings>();
                AssetDatabase.CreateAsset(EnhancedLoggerSettings, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                UnityEngine.Debug.LogWarningFormat(
                    $"No Settings asset found, creating a new one in the Resources folder at {assetPath}."
                );
#else
                // In runtime/test mode, create a temporary instance if no asset exists
                EnhancedLoggerSettings = ScriptableObject.CreateInstance<EnhancedLoggerSettings>();
#endif
            }
        }

        /// <summary>
        ///     Wraps a message in Unity's color tag syntax to display it in a specific color in the console.
        ///     Converts the Color to a hexadecimal RGB format (e.g., #FF0000 for red) and wraps the message
        ///     in <color> tags that Unity's console interprets to apply the color.
        /// </summary>
        /// <param name="message">The text to colorize.</param>
        /// <param name="color">The color to apply to the message.</param>
        /// <returns>The message wrapped in Unity color tags with the hex color code.</returns>
        private static string Color(this string message, Color color)
        {
            Color32 c = color;
            var hex = $"#{c.r:X2}{c.g:X2}{c.b:X2}";

            return $"<color={hex}>{message}</color>";
        }

        /// <summary>
        ///     Returns the prefix string configured for the given <paramref name="logLevel"/> in <see cref="EnhancedLoggerSettings"/>
        ///     (e.g. <c>[ERROR]</c>, <c>WARNING</c>). Used to label log output in the console.
        ///     Returns default prefixes if settings asset is not available.
        /// </summary>
        /// <param name="logLevel">The log level whose prefix to retrieve.</param>
        /// <returns>The configured prefix string for the log level, or default if settings unavailable.</returns>
        public static string GetPrefix(LogLevel logLevel)
        {
            if (EnhancedLoggerSettings == null)
            {
                return logLevel switch
                {
                    LogLevel.Error => "[ERROR]",
                    LogLevel.Warning => "WARNING",
                    LogLevel.Debug => "=DEBUG=",
                    LogLevel.Info => "INFORM:",
                    LogLevel.Success => "SUCCESS",
                    _ => "VERBOSE",
                };
            }

            return logLevel switch
            {
                LogLevel.Error => EnhancedLoggerSettings.ErrorPrefix,
                LogLevel.Warning => EnhancedLoggerSettings.WarningPrefix,
                LogLevel.Debug => EnhancedLoggerSettings.DebugPrefix,
                LogLevel.Info => EnhancedLoggerSettings.InfoPrefix,
                LogLevel.Success => EnhancedLoggerSettings.SuccessPrefix,
                _ => EnhancedLoggerSettings.VerbosePrefix,
            };
        }

        /// <summary>
        ///     Returns the <see cref="Color"/> configured for the given <paramref name="logLevel"/> in <see cref="EnhancedLoggerSettings"/>.
        ///     Used to colorize the log prefix in the Unity console.
        ///     Returns default colors if settings asset is not available.
        /// </summary>
        /// <param name="logLevel">The log level whose color to retrieve.</param>
        /// <returns>The configured color for the log level, or default if settings unavailable.</returns>
        public static Color GetColor(LogLevel logLevel)
        {
            if (EnhancedLoggerSettings == null)
            {
                return logLevel switch
                {
                    LogLevel.Error => UnityEngine.Color.red,
                    LogLevel.Warning => UnityEngine.Color.yellow,
                    LogLevel.Debug => UnityEngine.Color.cyan,
                    LogLevel.Info => UnityEngine.Color.white,
                    LogLevel.Success => UnityEngine.Color.green,
                    _ => UnityEngine.Color.magenta,
                };
            }

            return logLevel switch
            {
                LogLevel.Error => EnhancedLoggerSettings.ErrorColor,
                LogLevel.Warning => EnhancedLoggerSettings.WarningColor,
                LogLevel.Debug => EnhancedLoggerSettings.DebugColor,
                LogLevel.Info => EnhancedLoggerSettings.InfoColor,
                LogLevel.Success => EnhancedLoggerSettings.SuccessColor,
                _ => EnhancedLoggerSettings.VerboseColor,
            };
        }

        /// <summary>
        ///     Logs the current log level to the console on startup in Development builds.
        ///     Called automatically via <see cref="RuntimeInitializeOnLoadMethodAttribute"/>; no manual invocation needed.
        /// </summary>
        [RuntimeInitializeOnLoadMethod]
        public static void ReportLogLevel()
        {
#if DEVELOPMENT_BUILD
            UnityEngine.Debug.Log($"Current Log Level: {CurrentLogLevel}");
#endif
        }

        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        ///     Logs are not shown in the Release build
        ///     Theoretically, this should be less expensive than the Debug.Log(), over which I have no control.
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="callerObject"></param>
        /// <param name="message"></param>
        private static void Console(
            LogLevel logLevel,
            string message,
            Object callerObject = null,
            string callerName = "",
            string callerFilePath = "",
            int callerLineNumber = 0
        )
        {
            if (!ShouldShowLogs(logLevel))
            {
                return;
            }

            message ??= string.Empty;

            // Build the location string: extract only the filename (without path) and combine with method name and line number.
            // This provides a concise reference to where the log was called from.
            var location =
                $"{Path.GetFileNameWithoutExtension(callerFilePath)} ({callerName} : {callerLineNumber})";

            var postFix = string.Empty;

            // If a caller object is provided, append it to the message prefix for easy identification
            // of which GameObject or component generated the log.
            if (callerObject)
            {
                postFix = " on " + callerObject.name;
            }

            var messageStart = $"{GetPrefix(logLevel)} | {location}{postFix}";

            // Escape curly braces in the message to prevent them from being interpreted as format specifiers
            // by Unity's LogFormat method, which uses string formatting internally.
            message = message.Replace("{", "{{").Replace("}", "}}");

            // Apply color to the message prefix using Unity's color tag syntax.
            // The Color() extension method converts the Color to a hex string and wraps it in <color> tags.
            // Unity's console interprets these tags to display the text in the specified color.
            UnityEngine.Debug.LogFormat(
                LogType.Log,
                LogOption.None,
                callerObject,
                $"{messageStart.Color(GetColor(logLevel))} : {message}\n"
            );

            // If file logging is enabled, write the message to the log file (without color tags).
            if (EnhancedLoggerSettings != null && EnhancedLoggerSettings.WriteToFile)
            {
                WriteToFile.Log($"{messageStart} : {message}");
            }
        }

        private static bool ShouldShowLogs(LogLevel logLevel)
        {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            return false; // Logs are not shown in the Release build
#endif

            // Show logs that are equal to or lower numeric value (higher priority) than the current log level
            // Example: If CurrentLogLevel = Info (4), show Error (1), Warning (2), Debug (3), Info (4), but NOT Success (5) or Verbose (6)
            // Fixed: was >=, should be <=
            return logLevel <= CurrentLogLevel;
        }

        /// <summary>
        ///     Designed for catastrophic errors that break the game.
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="message"></param>
        /// <param name="callerLineNumber"></param>
        /// <param name="callerName"></param>
        /// <param name="callerFilePath"></param>
        public static void Error(
            this Object caller,
            string message,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFilePath = ""
        )
        {
            Console(LogLevel.Error, message, caller, callerName, callerFilePath, callerLineNumber);
        }

        /// <summary>
        ///     Designed for catastrophic errors that break the game.
        ///     Overload for multiple messages.
        /// </summary>
        /// <param name="caller">The Unity Object that is calling this log method.</param>
        /// <param name="message">The primary message to log.</param>
        /// <param name="message2">Additional message part to append with " : " separator.</param>
        /// <param name="message3">Optional third message part to append with " : " separator.</param>
        /// <param name="message4"></param>
        /// <param name="callerLineNumber"></param>
        /// <param name="callerName"></param>
        /// <param name="callerFilePath"></param>
        public static void Error(
            this Object caller,
            string message,
            string message2,
            string message3 = "",
            string message4 = "",
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFilePath = ""
        )
        {
            var combinedMessage = StringCombiner(message, message2, message3, message4);
            Console(
                LogLevel.Error,
                combinedMessage,
                caller,
                callerName,
                callerFilePath,
                callerLineNumber
            );
        }

        /// <summary>
        ///     Designed for severe issues that should not be ignored.
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="message"></param>
        /// <param name="callerLineNumber"></param>
        /// <param name="callerName"></param>
        /// <param name="callerFilePath"></param>
        public static void Warning(
            this Object caller,
            string message,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFilePath = ""
        )
        {
            Console(
                LogLevel.Warning,
                message,
                caller,
                callerName,
                callerFilePath,
                callerLineNumber
            );
        }

        /// <summary>
        ///     Designed for severe issues that should not be ignored.
        ///     Overload for multiple messages.
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="message"></param>
        /// <param name="message2"></param>
        /// <param name="message3"></param>
        /// <param name="message4"></param>
        /// <param name="callerLineNumber"></param>
        /// <param name="callerName"></param>
        /// <param name="callerFilePath"></param>
        public static void Warning(
            this Object caller,
            string message,
            string message2,
            string message3 = "",
            string message4 = "",
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFilePath = ""
        )
        {
            var combinedMessage = StringCombiner(message, message2, message3, message4);
            Console(
                LogLevel.Warning,
                combinedMessage,
                caller,
                callerName,
                callerFilePath,
                callerLineNumber
            );
        }

        /// <summary>
        ///     Designed for temporary messages (for instance during development of a module).
        ///     Once development is completed, these logs should be moved to the correct LogLevel.
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="message"></param>
        /// <param name="callerLineNumber"></param>
        /// <param name="callerName"></param>
        /// <param name="callerFilePath"></param>
        public static void Debug(
            this Object caller,
            string message,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFilePath = ""
        )
        {
            Console(LogLevel.Debug, message, caller, callerName, callerFilePath, callerLineNumber);
        }

        /// <summary>
        ///     Designed for temporary messages (for instance during development of a module).
        ///     Once development is completed, these logs should be moved to the correct LogLevel.
        ///     Overload for multiple messages.
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="message"></param>
        /// <param name="message2"></param>
        /// <param name="message3"></param>
        /// <param name="message4"></param>
        /// <param name="callerLineNumber"></param>
        /// <param name="callerName"></param>
        /// <param name="callerFilePath"></param>
        public static void Debug(
            this Object caller,
            string message,
            string message2,
            string message3 = "",
            string message4 = "",
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFilePath = ""
        )
        {
            var combinedMessage = StringCombiner(message, message2, message3, message4);
            Console(
                LogLevel.Debug,
                combinedMessage,
                caller,
                callerName,
                callerFilePath,
                callerLineNumber
            );
        }

        /// <summary>
        ///     Designed for general information messages that are not errors or warnings, but are generally useful to know.
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="message"></param>
        /// <param name="callerLineNumber"></param>
        /// <param name="callerName"></param>
        /// <param name="callerFilePath"></param>
        public static void Info(
            this Object caller,
            string message,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFilePath = ""
        )
        {
            Console(LogLevel.Info, message, caller, callerName, callerFilePath, callerLineNumber);
        }

        /// <summary>
        ///     Designed for general information messages that are not errors or warnings, but are generally useful to know.
        ///     Overload for multiple messages.
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="message"></param>
        /// <param name="message2"></param>
        /// <param name="message3"></param>
        /// <param name="message4"></param>
        /// <param name="callerLineNumber"></param>
        /// <param name="callerName"></param>
        /// <param name="callerFilePath"></param>
        public static void Info(
            this Object caller,
            string message,
            string message2,
            string message3 = "",
            string message4 = "",
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFilePath = ""
        )
        {
            var combinedMessage = StringCombiner(message, message2, message3, message4);
            Console(
                LogLevel.Info,
                combinedMessage,
                caller,
                callerName,
                callerFilePath,
                callerLineNumber
            );
        }

        /// <summary>
        ///     Designed to show successful operations or states.
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="message"></param>
        /// <param name="callerLineNumber"></param>
        /// <param name="callerName"></param>
        /// <param name="callerFilePath"></param>
        public static void Success(
            this Object caller,
            string message,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFilePath = ""
        )
        {
            Console(
                LogLevel.Success,
                message,
                caller,
                callerName,
                callerFilePath,
                callerLineNumber
            );
        }

        /// <summary>
        ///     Designed to show successful operations or states.
        ///     Overload for multiple messages.
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="message"></param>
        /// <param name="message2"></param>
        /// <param name="message3"></param>
        /// <param name="message4"></param>
        /// <param name="callerLineNumber"></param>
        /// <param name="callerName"></param>
        /// <param name="callerFilePath"></param>
        public static void Success(
            this Object caller,
            string message,
            string message2,
            string message3 = "",
            string message4 = "",
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFilePath = ""
        )
        {
            var combinedMessage = StringCombiner(message, message2, message3, message4);
            Console(
                LogLevel.Success,
                combinedMessage,
                caller,
                callerName,
                callerFilePath,
                callerLineNumber
            );
        }

        /// <summary>
        ///     Designed for detailed information that is useful for debugging or understanding the flow of the application.
        ///     Often this shows information that is too much for the Info log level.
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="message"></param>
        /// <param name="callerLineNumber"></param>
        /// <param name="callerName"></param>
        /// <param name="callerFilePath"></param>
        public static void Verbose(
            this Object caller,
            string message,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFilePath = ""
        )
        {
            Console(
                LogLevel.Verbose,
                message,
                caller,
                callerName,
                callerFilePath,
                callerLineNumber
            );
        }

        /// <summary>
        ///     Designed for detailed information that is useful for debugging or understanding the flow of the application.
        ///     Often this shows information that is too much for the Info log level.
        ///     Overload for multiple messages.
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="message"></param>
        /// <param name="message2"></param>
        /// <param name="message3"></param>
        /// <param name="message4"></param>
        /// <param name="callerLineNumber"></param>
        /// <param name="callerName"></param>
        /// <param name="callerFilePath"></param>
        public static void Verbose(
            this Object caller,
            string message,
            string message2,
            string message3 = "",
            string message4 = "",
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFilePath = ""
        )
        {
            var combinedMessage = StringCombiner(message, message2, message3, message4);
            Console(
                LogLevel.Verbose,
                combinedMessage,
                caller,
                callerName,
                callerFilePath,
                callerLineNumber
            );
        }

        /// <summary>
        ///     For when you want to set your own log level. Defaults to Debug.
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="message"></param>
        /// <param name="logLevel"></param>
        /// <param name="callerLineNumber"></param>
        /// <param name="callerName"></param>
        /// <param name="callerFilePath"></param>
        public static void Console(
            this Object caller,
            string message,
            LogLevel logLevel = LogLevel.Debug,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFilePath = ""
        )
        {
            Console(logLevel, message, caller, callerName, callerFilePath, callerLineNumber);
        }

        /// <summary>
        ///     For when you want to set your own log level. Defaults to Debug.
        ///     Overload for multiple messages.
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="message"></param>
        /// <param name="message2"></param>
        /// <param name="message3"></param>
        /// <param name="message4"></param>
        /// <param name="logLevel"></param>
        /// <param name="callerLineNumber"></param>
        /// <param name="callerName"></param>
        /// <param name="callerFilePath"></param>
        public static void Console(
            this Object caller,
            string message,
            string message2,
            string message3 = "",
            string message4 = "",
            LogLevel logLevel = LogLevel.Debug,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFilePath = ""
        )
        {
            var combinedMessage = StringCombiner(message, message2, message3, message4);
            Console(
                logLevel,
                combinedMessage,
                caller,
                callerName,
                callerFilePath,
                callerLineNumber
            );
        }

        /// <summary>
        ///     Use when calling logs from static methods or when the caller is not an Object.
        ///     However, it loses the ability to click in the console to go to the line of code that called the log, and the GameObject in the Hierarchy that called it.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logLevel"></param>
        /// <param name="callerLineNumber"></param>
        /// <param name="callerName"></param>
        /// <param name="callerFilePath"></param>
        public static void Static(
            string message,
            LogLevel logLevel = LogLevel.Debug,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFilePath = ""
        )
        {
            Console(logLevel, message, null, callerName, callerFilePath, callerLineNumber);
        }

        /// <summary>
        ///     Use when calling logs from static methods or when the caller is not an Object.
        ///     However, it loses the ability to click in the console to go to the line of code that called the log, and the GameObject in the Hierarchy that called it.
        ///     Overload for multiple messages.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="message2"></param>
        /// <param name="message3"></param>
        /// <param name="message4"></param>
        /// <param name="logLevel"></param>
        /// <param name="callerLineNumber"></param>
        /// <param name="callerName"></param>
        /// <param name="callerFilePath"></param>
        public static void Static(
            string message,
            string message2,
            string message3 = "",
            string message4 = "",
            LogLevel logLevel = LogLevel.Debug,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string callerFilePath = ""
        )
        {
            var combinedMessage = StringCombiner(message, message2, message3, message4);
            Console(logLevel, combinedMessage, null, callerName, callerFilePath, callerLineNumber);
        }

        private static string StringCombiner(
            string message,
            string message2 = "",
            string message3 = "",
            string message4 = ""
        )
        {
            if (string.IsNullOrEmpty(message2))
            {
                return message;
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder(message);
            sb.Append(" : ").Append(message2);

            if (!string.IsNullOrEmpty(message3))
            {
                sb.Append(" : ").Append(message3);
            }

            if (!string.IsNullOrEmpty(message4))
            {
                sb.Append(" : ").Append(message4);
            }

            return sb.ToString();
        }
    }
}
