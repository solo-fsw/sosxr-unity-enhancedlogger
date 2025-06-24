using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace SOSXR.EnhancedLogger
{
    public static class WriteToFile
    {
        private static string _folderName => "EnhancedLogger";
        private static string _filePath;
        private static readonly Dictionary<string, LogEntry> _logCache = new();


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var folder = Path.Combine(Application.persistentDataPath, _folderName);
            Directory.CreateDirectory(folder);

            _filePath = Path.Combine(folder, $"{Application.productName}_{timestamp}.md");
            Application.quitting += WriteFile;
        }


        public static void Log(string message)
        {
            var now = DateTime.Now;

            if (_logCache.TryGetValue(message, out var entry))
            {
                entry.Count++;
                entry.LastTime = now;
            }
            else
            {
                _logCache[message] = new LogEntry
                {
                    Count = 1,
                    FirstTime = now,
                    LastTime = now
                };
            }
        }


        public static void WriteFile()
        {
            try
            {
                using var writer = new StreamWriter(_filePath, true);

                var title = $"{Application.productName} - {Application.version} - Unity {Application.unityVersion} - {(Application.isEditor ? "Editor" : "Build")}";
                writer.WriteLine($"# Log Summary of {title}\n");

                foreach (var kvp in _logCache)
                {
                    var msg = kvp.Key;
                    var entry = kvp.Value;

                    writer.WriteLine($"{EscapeMarkdown(msg)} - from `{entry.FirstTime:HH:mm:ss}` to `{entry.LastTime:HH:mm:ss}` shown **{entry.Count}x**");
                }

                _logCache.Clear();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WriteToFile] Failed to flush log: {ex.Message}");
            }
        }


        private static string EscapeMarkdown(string msg)
        {
            return msg.Replace("*", "\\*").Replace("_", "\\_").Replace("`", "\\`");
        }


        private class LogEntry
        {
            public int Count;
            public DateTime FirstTime;
            public DateTime LastTime;
        }
    }
}