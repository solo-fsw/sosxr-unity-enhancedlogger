using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("SOSXR.EnhancedLogger.Tests")]

namespace SOSXR.EnhancedLogger
{
    /// <summary>Writes each message to a session-specific Markdown file.</summary>
    public static class WriteToFile
    {
        private static readonly FileLogSession _session = new();

        /// <summary>No messages are cached; writes complete before returning.</summary>
        public static int CacheSize => 0;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            _session.Initialize();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
        }

#if UNITY_EDITOR
        private static void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state != UnityEditor.PlayModeStateChange.EnteredEditMode) return;
            _session.EndSession();
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }
#endif

        /// <summary>Appends a message to the current session file.</summary>
        public static void Log(string message) => _session.Log(message);

        /// <summary>Retained for compatibility; each message is already written.</summary>
        public static void WriteFile() { }

        /// <summary>Retained for compatibility; there is no pending file output.</summary>
        public static void WriteFinalFile() { }
    }

    internal sealed class FileLogSession
    {
        private readonly object _sync = new();
        private string _folderPath;
        private string _filePath;
        private string _title;
        private bool _created;
        private bool _failed;

        internal FileLogSession(string folderPath = null, string filePath = null, string title = null)
        {
            _folderPath = folderPath;
            _filePath = filePath;
            _title = title;
        }

        public int CacheSize => 0;

        internal void Initialize()
        {
            var root = Application.persistentDataPath;
            var folder = string.IsNullOrEmpty(root) ? null : Path.Combine(root, "EnhancedLogger");
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture);
            var path = folder == null ? null : Path.Combine(folder, $"session_{timestamp}_{Guid.NewGuid():N}.md");
            var title = $"{Application.productName} - {Application.version} - Unity {Application.unityVersion}";
            lock (_sync)
            {
                _folderPath = folder;
                _filePath = path;
                _title = title;
                _created = false;
                _failed = false;
            }
        }

        internal void EndSession()
        {
            lock (_sync)
            {
                _folderPath = null;
                _filePath = null;
            }
        }

        public void Log(string message)
        {
            if (string.IsNullOrEmpty(message)) return;

            string warning = null;
            lock (_sync)
            {
                if (_failed) return;
                try
                {
                    if (string.IsNullOrEmpty(_folderPath) || string.IsNullOrEmpty(_filePath))
                        throw new InvalidOperationException("Persistent file logging is not initialized or unavailable.");

                    Directory.CreateDirectory(_folderPath);
                    var first = !_created;
                    using (var stream = new FileStream(_filePath, first ? FileMode.CreateNew : FileMode.Append, FileAccess.Write, FileShare.Read))
                    using (var writer = new StreamWriter(stream))
                    {
                        _created = true;
                        if (first && !string.IsNullOrEmpty(_title))
                            writer.WriteLine($"# {_title.Replace('\r', ' ').Replace('\n', ' ')}\n");
                        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                        var escaped = message.Replace("`", "\\`").Replace('\r', ' ').Replace('\n', ' ');
                        writer.WriteLine($"- {timestamp} UTC: `{escaped}`");
                    }
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException || ex is NotSupportedException || ex is System.Security.SecurityException || ex is InvalidOperationException)
                {
                    _failed = true;
                    warning = $"[WriteToFile] File logging disabled for this session: {ex.Message}";
                }
            }

            if (warning != null) Debug.LogWarning(warning);
        }
    }
}
