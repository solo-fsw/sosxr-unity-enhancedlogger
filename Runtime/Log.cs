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
        private const string ErrorPrefix = "<b>[!ERROR!]</b>";
        private const string WarningPrefix = "<b>[WARNING]</b>";
        private const string InfoPrefix = "";
        private const string DebugPrefix = "<DEBUG>";
        private const string SuccessPrefix = "";
        private const string VerbosePrefix = "";

        private const string ErrorColor = "red";
        private const string WarningColor = "yellow";
        private const string InfoColor = "lightblue";
        private const string DebugColor = "orange";
        private const string SuccessColor = "green";
        private const string VerboseColor = "white";


        public static LogLevel CurrentLogLevel = LogLevel.Debug;


        private static string Color(this string myString, string color)
        {
            #if UNITY_EDITOR
            return $"<color={color}>{myString}</color>";
            #elif DEVELOPMENT_BUILD
            return myString;
            #else
            return null;
            #endif
        }


        public static string GetPrefix(LogLevel logLevel)
        {
            return logLevel switch
                   {
                       LogLevel.Error => ErrorPrefix,
                       LogLevel.Warning => WarningPrefix,
                       LogLevel.Info => InfoPrefix,
                       LogLevel.Debug => DebugPrefix,
                       LogLevel.Success => SuccessPrefix,
                       _ => VerbosePrefix
                   };
        }


        public static string GetColor(LogLevel logLevel)
        {
            return logLevel switch
                   {
                       LogLevel.Error => ErrorColor,
                       LogLevel.Warning => WarningColor,
                       LogLevel.Info => InfoColor,
                       LogLevel.Debug => DebugColor,
                       LogLevel.Success => SuccessColor,
                       _ => VerboseColor
                   };
        }


        /// <summary>
        ///     Logs are not shown in the release build
        ///     Theoretically, this should be less expensive than the Debug.Log, over which I have no control
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="caller"></param>
        /// <param name="message"></param>
        private static void DoLog(LogLevel logLevel, object caller, params object[] message)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (CurrentLogLevel < logLevel)
            {
                return;
            }

            var objectName = "";

            if (caller is Object unityObject)
            {
                if (unityObject == null || string.IsNullOrEmpty(unityObject.name))
                {
                    UnityEngine.Debug.LogWarningFormat("Cannot use the name of this object");
                    objectName = "[" + "NAME_LESS" + "]";
                }
                else
                {
                    objectName = "[" + unityObject.name + "]";
                }
            }
            else
            {
                objectName = "[" + caller + "]";
            }

            var prefix = GetPrefix(logLevel);
            var color = GetColor(logLevel);

            if (!string.IsNullOrEmpty(prefix))
            {
                objectName = string.Concat(objectName, prefix);
            }

            UnityEngine.Debug.Log($"{objectName.Color(color)} : {string.Join(" : ", message)}\n", caller as Object);
            #endif
        }


        /// <summary>
        ///     Designed for catastrophic level error-logging.
        ///     The logger defaults to show Error messages in the editor / development build (Debug and up).
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="message"></param>
        public static void Error(this Object caller, params object[] message)
        {
            DoLog(LogLevel.Error, caller, message);
        }


        /// <summary>
        ///     Designed for catastrophic level error-logging
        ///     The logger defaults to show Error messages in the editor / development build (Debug and up).
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="message"></param>
        public static void Error(string caller, params object[] message)
        {
            DoLog(LogLevel.Error, caller, message);
        }


        /// <summary>
        ///     Designed for warnings which need to be fixed, but do not break the game.
        ///     The logger defaults to show Warning messages in the editor / development build (Debug and up).
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="message"></param>
        public static void Warning(this Object caller, params object[] message)
        {
            DoLog(LogLevel.Warning, caller, message);
        }


        /// <summary>
        ///     Designed for warnings which need to be fixed, but do not break the game
        ///     The logger defaults to show Warning messages in the editor / development build (Debug and up).
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="message"></param>
        public static void Warning(string caller, params object[] message)
        {
            DoLog(LogLevel.Warning, caller, message);
        }


        /// <summary>
        ///     Designed for general information messages.
        ///     The logger defaults to show Info messages in the editor / development build (because it defaults to Debug and up).
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="message"></param>
        public static void Info(this Object caller, params object[] message)
        {
            DoLog(LogLevel.Info, caller, message);
        }


        /// <summary>
        ///     Designed for general information messages.
        ///     The logger defaults to show Info messages in the editor / development build (because it defaults to Debug and up).
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="message"></param>
        public static void Info(string caller, params object[] message)
        {
            DoLog(LogLevel.Info, caller, message);
        }


        /// <summary>
        ///     Designed for temporary debug messages.
        ///     The Logger defaults to show Debug messages in the editor / development build.
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="message"></param>
        public static void Debug(this Object caller, params object[] message)
        {
            DoLog(LogLevel.Debug, caller, message);
        }


        /// <summary>
        ///     Designed for temporary debug messages.
        ///     The Logger defaults to show Debug messages in the editor / development build.
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="message"></param>
        public static void Debug(string caller, params object[] message)
        {
            DoLog(LogLevel.Debug, caller, message);
        }


        /// <summary>
        ///     Designed to note when something does work. Hurray!
        ///     By default these messages are not shown in the editor / development build, because the Log defaults to Debug.
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="message"></param>
        public static void Success(this Object caller, params object[] message)
        {
            DoLog(LogLevel.Success, caller, message);
        }


        /// <summary>
        ///     Designed to note when something does work. Party.
        ///     By default these messages are not shown in the editor / development build, because the Log defaults to Debug.
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="message"></param>
        public static void Success(string caller, params object[] message)
        {
            DoLog(LogLevel.Success, caller, message);
        }


        /// <summary>
        ///     Designed for the lowest level of persistent Logging. Only shown in Full / Info logging mode
        ///     By default these messages are not shown in the editor / development build, because the Log defaults to Debug.
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="message"></param>
        public static void Verbose(this Object caller, params object[] message)
        {
            DoLog(LogLevel.Verbose, caller, message);
        }


        /// <summary>
        ///     Designed for the lowest level of persistent Logging. Only shown in Full / Info logging mode
        ///     By default these messages are not shown in the editor / development build, because the Log defaults to Debug.
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="message"></param>
        public static void Verbose(string caller, params object[] message)
        {
            DoLog(LogLevel.Verbose, caller, message);
        }
    }
}