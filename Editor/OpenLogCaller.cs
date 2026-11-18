using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SOSXR.EnhancedLogger.EditorScripts
{
    /// <summary>
    ///     Double-clicking an EnhancedLogger message in the Console opens the code that called the logger,
    ///     instead of the <c>Debug.Log</c> call inside Log.cs.
    ///     The console asks to open Log.cs; this callback reads the selected entry's stack trace,
    ///     finds the first frame after the logger, and opens that file and line instead.
    ///     Works with the "Script Only" and "Full" stack trace logging options.
    /// </summary>
    public static class OpenLogCaller
    {
        private const string ConsoleWindowTypeName = "UnityEditor.ConsoleWindow";

        internal static readonly Type ConsoleWindowType = typeof(EditorWindow).Assembly.GetType(ConsoleWindowTypeName);

        internal static readonly FieldInfo ConsoleInstanceField = ConsoleWindowType?.GetField(
            "ms_ConsoleWindow",
            BindingFlags.Static | BindingFlags.NonPublic
        );

        internal static readonly FieldInfo ActiveTextField = ConsoleWindowType?.GetField(
            "m_ActiveText",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        // Runs before other OnOpenAsset callbacks, so the redirect wins.
        [OnOpenAsset(0)]
#if UNITY_6000_3_OR_NEWER
        public static bool OnOpenAsset(EntityId entityId, int line)
        {
            var obj = EditorUtility.EntityIdToObject(entityId);
#else
        public static bool OnOpenAsset(int entityId, int line)
        {
            var obj = EditorUtility.InstanceIDToObject(entityId);
#endif
            if (!IsLoggerScript(obj) || !IsConsoleFocused())
            {
                return false; // Unity has control of this asset
            }

            var consoleText = GetSelectedConsoleText();
            var frames = LogCallerParser.ParseFrames(consoleText);

            // A click on a stack trace link deeper inside Log.cs: open Log.cs as asked.
            if (LogCallerParser.IsDeeperLoggerLine(frames, line))
            {
                return false;
            }

            if (!LogCallerParser.TryFindCaller(frames, out var caller))
            {
                return false;
            }

            return OpenFrame(caller); // True: I have control of this asset
        }

        /// <summary>
        ///     True when this Unity version still has the private Console members that the redirect reads by reflection.
        ///     When Unity renames them, this is false and double-click opens Log.cs, as without this callback.
        /// </summary>
        internal static bool CanReadConsoleText =>
            ConsoleWindowType != null
            && ConsoleInstanceField != null
            && ConsoleInstanceField.FieldType == ConsoleWindowType
            && ActiveTextField != null
            && ActiveTextField.FieldType == typeof(string);

        internal static bool IsLoggerScript(Object obj)
        {
            return obj is MonoScript script && script.GetClass() == typeof(Log);
        }

        private static bool IsConsoleFocused()
        {
            var focused = EditorWindow.focusedWindow;

            return focused != null && focused.GetType() == ConsoleWindowType;
        }

        /// <summary>
        ///     Text of the selected console entry: message plus stack trace.
        ///     The console has no public API for this, so it is read by reflection. Returns null when that fails.
        /// </summary>
        private static string GetSelectedConsoleText()
        {
            if (!CanReadConsoleText)
            {
                return null;
            }

            var console = ConsoleInstanceField.GetValue(null);

            return console == null ? null : ActiveTextField.GetValue(console) as string;
        }

        private static bool OpenFrame(LogStackFrame frame)
        {
            var script = FindScript(frame);

            if (script != null)
            {
                return AssetDatabase.OpenAsset(script, frame.Line);
            }

            var fullPath = Path.GetFullPath(frame.FilePath);

            if (!File.Exists(fullPath))
            {
                return false;
            }

            return InternalEditorUtility.OpenFileAtLineExternal(fullPath, frame.Line);
        }

        /// <summary>
        ///     Script-only stack traces hold a project path. Full stack traces hold only the file name:
        ///     then the script is searched by name, and the class name decides between scripts with the same name.
        /// </summary>
        private static MonoScript FindScript(LogStackFrame frame)
        {
            var byPath = AssetDatabase.LoadAssetAtPath<MonoScript>(frame.FilePath.Replace('\\', '/'));

            if (byPath != null)
            {
                return byPath;
            }

            var fileName = Path.GetFileName(frame.FilePath);
            var searchName = Path.GetFileNameWithoutExtension(fileName);

            var candidates = AssetDatabase.FindAssets(searchName + " t:MonoScript")
                                          .Select(AssetDatabase.GUIDToAssetPath)
                                          .Where(path => string.Equals(Path.GetFileName(path), fileName, StringComparison.OrdinalIgnoreCase))
                                          .Select(AssetDatabase.LoadAssetAtPath<MonoScript>)
                                          .Where(script => script != null)
                                          .ToList();

            if (candidates.Count == 0)
            {
                return null;
            }

            return candidates.FirstOrDefault(script => script.GetClass()?.FullName == frame.TypeName)
                   ?? candidates[0];
        }
    }
}
