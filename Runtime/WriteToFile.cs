using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SOSXR.EnhancedLogger
{
    /// <summary>
    ///     Handles writing log messages to a markdown file in the persistent data path.
    ///     Uses a caching strategy to deduplicate identical messages and track their frequency and timing.
    ///     This reduces log file noise by consolidating repeated messages into a single entry with occurrence count.
    ///     The log file is written when the application quits, containing a summary of all logged messages
    ///     with timestamps (first and last occurrence) and the number of times each message appeared.
    /// </summary>
    public static class WriteToFile
    {
        private static string _folderName => "EnhancedLogger";
        private static string _filePath;
        private static readonly Dictionary<string, LogEntry> _logCache = new();

        /// <summary>
        ///     Maximum number of unique log entries to cache in memory.
        ///     When exceeded, older entries are flushed to disk early to prevent memory issues.
        /// </summary>
        private const int MaxCacheSize = 10000;

        /// <summary>
        ///     Current cache size for monitoring purposes.
        /// </summary>
        public static int CacheSize => _logCache.Count;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            // Initialize the log file path with a timestamp to create a unique file for each session.
            // This method is called before the first scene loads, ensuring the file path is ready
            // before any logs are written. The Application.quitting event is registered to flush
            // all cached logs to the file when the application exits.
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var folder = Path.Combine(Application.persistentDataPath, _folderName);
            Directory.CreateDirectory(folder);

            _filePath = Path.Combine(folder, $"{Application.productName}_{timestamp}.md");
            Application.quitting += WriteFinalFile;
        }

        /// <summary>
        ///     Queues a log message for deduplication and eventual file output.
        ///     Identical messages increment a shared counter rather than creating duplicate entries.
        ///     The file is not written until <see cref="WriteFile"/> is called (i.e. on application quit).
        /// </summary>
        /// <param name="message">The formatted log message to record (color tags should be stripped before passing).</param>
        public static void Log(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            var now = DateTime.Now;

            if (_logCache.TryGetValue(message, out var entry))
            {
                entry.Count++;
                entry.LastTime = now;
            }
            else
            {
                if (_logCache.Count >= MaxCacheSize)
                {
                    WriteFile();
                }

                _logCache[message] = new LogEntry
                {
                    Count = 1,
                    FirstTime = now,
                    LastTime = now,
                };
            }
        }

        /// <summary>
        ///     Flushes all cached log entries to the Markdown file at <see cref="_filePath"/>.
        ///     Each entry is written with its message text, first/last occurrence timestamps, and occurrence count.
        ///     Called automatically on <c>Application.quitting</c>; can also be invoked manually for early flush.
        /// </summary>
        public static void WriteFile()
        {
            try
            {
                using var writer = new StreamWriter(_filePath, true);

                var title =
                    $"{Application.productName} - {Application.version} - Unity {Application.unityVersion} - {(Application.isEditor ? "Editor" : "Build")}";

                writer.WriteLine($"# Log Summary of {title}\n");

                foreach (var kvp in _logCache)
                {
                    var msg = kvp.Key;
                    var entry = kvp.Value;

                    writer.WriteLine(
                        $"{EscapeMarkdown(msg)} - from `{entry.FirstTime:HH:mm:ss}` to `{entry.LastTime:HH:mm:ss}` shown **{entry.Count}x**"
                    );
                }

                _logCache.Clear();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WriteToFile] Failed to flush log: {ex.Message}");
            }
        }

        public static void WriteFinalFile()
        {
            WriteFile();
            Application.quitting -= WriteFinalFile;
        }

        private static string EscapeMarkdown(string msg)
        {
            if (string.IsNullOrEmpty(msg))
            {
                return "`";
            }

            msg = msg.Replace("`", "\\`");
            msg = msg.Replace("\n", " ");
            msg = msg.Replace("\r", "");

            return $"`{msg}`";
        }

        /// <summary>
        ///     Represents a cached log entry with deduplication information.
        ///     Count: Number of times this message has been logged.
        ///     FirstTime: Timestamp of the first occurrence of this message.
        ///     LastTime: Timestamp of the most recent occurrence of this message.
        /// </summary>
        private class LogEntry
        {
            public int Count;
            public DateTime FirstTime;
            public DateTime LastTime;
        }
    }
}
