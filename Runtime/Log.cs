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
        public static LogLevel CurrentLogLevel
        {
            get
            {
                LoadSettingsAsset();

                return EnhancedLoggerSettings.CurrentLogLevel;
            }
            set
            {
                LoadSettingsAsset();

                EnhancedLoggerSettings.CurrentLogLevel = value;
            }
        }

        public static EnhancedLoggerSettings EnhancedLoggerSettings;


        private static void LoadSettingsAsset()
        {
            if (EnhancedLoggerSettings)
            {
                return;
            }

            EnhancedLoggerSettings = Resources.Load<EnhancedLoggerSettings>(nameof(EnhancedLoggerSettings));

            if (!EnhancedLoggerSettings)
            {
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

                UnityEngine.Debug.LogWarningFormat($"No Settings asset found, creating a new one in the Resources folder at {assetPath}.");
            }
        }


        private static string Color(this string message, Color color)
        {
            Color32 c = color;
            var hex = $"#{c.r:X2}{c.g:X2}{c.b:X2}";

            return $"<color={hex}>{message}</color>";
        }


        public static string GetPrefix(LogLevel logLevel)
        {
            return logLevel switch
                   {
                       LogLevel.Error => EnhancedLoggerSettings.ErrorPrefix,
                       LogLevel.Warning => EnhancedLoggerSettings.WarningPrefix,
                       LogLevel.Debug => EnhancedLoggerSettings.DebugPrefix,
                       LogLevel.Info => EnhancedLoggerSettings.InfoPrefix,
                       LogLevel.Success => EnhancedLoggerSettings.SuccessPrefix,
                       _ => EnhancedLoggerSettings.VerbosePrefix
                   };
        }


        public static Color GetColor(LogLevel logLevel)
        {
            return logLevel switch
                   {
                       LogLevel.Error => EnhancedLoggerSettings.ErrorColor,
                       LogLevel.Warning => EnhancedLoggerSettings.WarningColor,
                       LogLevel.Debug => EnhancedLoggerSettings.DebugColor,
                       LogLevel.Info => EnhancedLoggerSettings.InfoColor,
                       LogLevel.Success => EnhancedLoggerSettings.SuccessColor,
                       _ => EnhancedLoggerSettings.VerboseColor
                   };
        }


        [RuntimeInitializeOnLoadMethod]
        public static void ReportLogLevel()
        {
            #if DEVELOPMENT_BUILD
                UnityEngine.Debug.Log($"Current Log Level: {CurrentLogLevel}");
            #endif
        }


        /// <summary>
        ///     Logs are not shown in the Release build
        ///     Theoretically, this should be less expensive than the Debug.Log(), over which I have no control.
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="callerObject"></param>
        /// <param name="message"></param>
        private static void Console(LogLevel logLevel, string message, Object callerObject = null, string callerName = "", string callerFilePath = "", int callerLineNumber = 0)
        {
            if (!ShouldShowLogs(logLevel))
            {
                return;
            }

            var location = $"{Path.GetFileNameWithoutExtension(callerFilePath)} ({callerName} : {callerLineNumber})";

            var postFix = string.Empty;

            if (callerObject)
            {
                postFix = " on " + callerObject?.name;
            }

            var messageStart = $"{GetPrefix(logLevel)} | {location}{postFix}";

            UnityEngine.Debug.LogFormat(LogType.Log, LogOption.None, callerObject, $"{messageStart.Color(GetColor(logLevel))} : {message}\n");

            if (EnhancedLoggerSettings.WriteToFile)
            {
                WriteToFile.Log($"{messageStart} : {message}");
            }
        }


        private static bool ShouldShowLogs(LogLevel logLevel)
        {
            #if !UNITY_EDITOR && !DEVELOPMENT_BUILD
                return false; // Logs are not shown in the Release build
            #endif

            return CurrentLogLevel >= logLevel; // Only show logs that are equal to or above the current log level
        }


        /// <summary>
        ///     Designed for catastrophic errors that break the game.
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="message"></param>
        /// <param name="callerLineNumber"></param>
        /// <param name="callerName"></param>
        /// <param name="callerFilePath"></param>
        public static void Error(this Object caller, string message, [CallerLineNumber] int callerLineNumber = 0, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "")
        {
            Console(LogLevel.Error, message, caller, callerName, callerFilePath, callerLineNumber);
        }


        /// <summary>
        ///     Designed for catastrophic errors that break the game.
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
        public static void Error(this Object caller, string message, string message2, string message3 = "", string message4 = "", [CallerLineNumber] int callerLineNumber = 0, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "")
        {
            var combinedMessage = StringCombiner(message, message2, message3, message4);
            Console(LogLevel.Error, combinedMessage, caller, callerName, callerFilePath, callerLineNumber);
        }


        /// <summary>
        ///     Designed for severe issues that should not be ignored.
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="message"></param>
        /// <param name="callerLineNumber"></param>
        /// <param name="callerName"></param>
        /// <param name="callerFilePath"></param>
        public static void Warning(this Object caller, string message, [CallerLineNumber] int callerLineNumber = 0, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "")
        {
            Console(LogLevel.Warning, message, caller, callerName, callerFilePath, callerLineNumber);
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
        public static void Warning(this Object caller, string message, string message2, string message3 = "", string message4 = "", [CallerLineNumber] int callerLineNumber = 0, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "")
        {
            var combinedMessage = StringCombiner(message, message2, message3, message4);
            Console(LogLevel.Warning, combinedMessage, caller, callerName, callerFilePath, callerLineNumber);
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
        public static void Debug(this Object caller, string message, [CallerLineNumber] int callerLineNumber = 0, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "")
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
        public static void Debug(this Object caller, string message, string message2, string message3 = "", string message4 = "", [CallerLineNumber] int callerLineNumber = 0, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "")
        {
            var combinedMessage = StringCombiner(message, message2, message3, message4);
            Console(LogLevel.Debug, combinedMessage, caller, callerName, callerFilePath, callerLineNumber);
        }


        /// <summary>
        ///     Designed for general information messages that are not errors or warnings, but are generally useful to know.
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="message"></param>
        /// <param name="callerLineNumber"></param>
        /// <param name="callerName"></param>
        /// <param name="callerFilePath"></param>
        public static void Info(this Object caller, string message, [CallerLineNumber] int callerLineNumber = 0, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "")
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
        public static void Info(this Object caller, string message, string message2, string message3 = "", string message4 = "", [CallerLineNumber] int callerLineNumber = 0, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "")
        {
            var combinedMessage = StringCombiner(message, message2, message3, message4);
            Console(LogLevel.Info, combinedMessage, caller, callerName, callerFilePath, callerLineNumber);
        }


        /// <summary>
        ///     Designed to show successful operations or states.
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="message"></param>
        /// <param name="callerLineNumber"></param>
        /// <param name="callerName"></param>
        /// <param name="callerFilePath"></param>
        public static void Success(this Object caller, string message, [CallerLineNumber] int callerLineNumber = 0, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "")
        {
            Console(LogLevel.Success, message, caller, callerName, callerFilePath, callerLineNumber);
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
        public static void Success(this Object caller, string message, string message2, string message3 = "", string message4 = "", [CallerLineNumber] int callerLineNumber = 0, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "")
        {
            var combinedMessage = StringCombiner(message, message2, message3, message4);
            Console(LogLevel.Success, combinedMessage, caller, callerName, callerFilePath, callerLineNumber);
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
        public static void Verbose(this Object caller, string message, [CallerLineNumber] int callerLineNumber = 0, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "")
        {
            Console(LogLevel.Verbose, message, caller, callerName, callerFilePath, callerLineNumber);
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
        public static void Verbose(this Object caller, string message, string message2, string message3 = "", string message4 = "", [CallerLineNumber] int callerLineNumber = 0, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "")
        {
            var combinedMessage = StringCombiner(message, message2, message3, message4);
            Console(LogLevel.Verbose, combinedMessage, caller, callerName, callerFilePath, callerLineNumber);
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
        public static void Console(this Object caller, string message, LogLevel logLevel = LogLevel.Debug, [CallerLineNumber] int callerLineNumber = 0, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "")
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
        public static void Console(this Object caller, string message, string message2, string message3 = "", string message4 = "", LogLevel logLevel = LogLevel.Debug, [CallerLineNumber] int callerLineNumber = 0, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "")
        {
            var combinedMessage = StringCombiner(message, message2, message3, message4);
            Console(logLevel, combinedMessage, caller, callerName, callerFilePath, callerLineNumber);
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
        public static void Static(string message, LogLevel logLevel = LogLevel.Debug, [CallerLineNumber] int callerLineNumber = 0, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "")
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
        public static void Static(string message, string message2, string message3 = "", string message4 = "", LogLevel logLevel = LogLevel.Debug, [CallerLineNumber] int callerLineNumber = 0, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "")
        {
            var combinedMessage = StringCombiner(message, message2, message3, message4);
            Console(logLevel, combinedMessage, null, callerName, callerFilePath, callerLineNumber);
        }


        private static string StringCombiner(string message, string message2 = "", string message3 = "", string message4 = "")
        {
            if (!string.IsNullOrEmpty(message4))
            {
                return string.Join(" : ", message, message2, message3, message4);
            }

            if (!string.IsNullOrEmpty(message3))
            {
                return string.Join(" : ", message, message2, message3);
            }

            if (!string.IsNullOrEmpty(message2))
            {
                return string.Join(" : ", message, message2);
            }

            return message;
        }
    }
}