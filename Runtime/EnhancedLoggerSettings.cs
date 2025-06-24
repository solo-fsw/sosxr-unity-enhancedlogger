using System.IO;
using UnityEditor;
using UnityEngine;


namespace SOSXR.EnhancedLogger
{
    [CreateAssetMenu(fileName = "EnhancedLoggerSettings", menuName = "SOSXR/EnhancedLogger/EnhancedLoggerSettings", order = 1)]
    public class EnhancedLoggerSettings : ScriptableObject
    {
        [Header("Log Level")]
        public LogLevel CurrentLogLevel = LogLevel.Info;

        [Header("Prefixes")]
        public string ErrorPrefix = "[ERROR]";
        public string WarningPrefix = "WARNING";
        public string DebugPrefix = "=DEBUG=";
        public string InfoPrefix = "INFORM:";
        public string SuccessPrefix = "SUCCESS";
        public string VerbosePrefix = "VERBOSE";

        [Header("Colors")]
        public Color ErrorColor = Color.softRed;
        public Color WarningColor = Color.orange;
        public Color DebugColor = Color.cornflowerBlue;
        public Color InfoColor = Color.khaki;
        public Color SuccessColor = Color.softGreen;
        public Color VerboseColor = Color.hotPink;

        [Header("File Logging")]
        public bool WriteToFile = true;
    }
}