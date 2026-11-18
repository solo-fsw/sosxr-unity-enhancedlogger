using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

[assembly: InternalsVisibleTo("SOSXR.EnhancedLogger.Tests")]

namespace SOSXR.EnhancedLogger.EditorScripts
{
    /// <summary>
    ///     One frame of a console stack trace that points to a source file.
    /// </summary>
    internal readonly struct LogStackFrame
    {
        public LogStackFrame(string typeName, string filePath, int line)
        {
            TypeName = typeName;
            FilePath = filePath;
            Line = line;
        }

        /// <summary>Full name of the type that owns the method, e.g. <c>SOSXR.CharacterHandler</c>.</summary>
        public string TypeName { get; }

        /// <summary>File as written in the stack trace: a path (script-only stack traces) or only a file name (full stack traces).</summary>
        public string FilePath { get; }

        public int Line { get; }
    }

    /// <summary>
    ///     Finds the code that called <see cref="Log" /> in the text of a console entry.
    ///     Reads both stack trace formats of the Unity console:
    ///     script only: <c>SOSXR.CharacterHandler:SetCalmer () (at Assets/Scripts/CharacterHandler.cs:527)</c>
    ///     full: <c>#11  (Mono JIT Code) [CharacterHandler.cs:527] SOSXR.CharacterHandler:SetCalmer ()</c>
    /// </summary>
    internal static class LogCallerParser
    {
        internal static readonly string LoggerTypeName = typeof(Log).FullName;

        private static readonly Regex ScriptOnlyFrame = new(
            @"^\s*(?<method>\S[^(]*?)\s*\(.*\(at (?<file>.+):(?<line>\d+)\)\s*$",
            RegexOptions.Compiled
        );

        private static readonly Regex FullFrame = new(
            @"\[(?<file>[^\[\]]+?):(?<line>\d+)\]\s+(?<method>\S+)",
            RegexOptions.Compiled
        );

        /// <summary>
        ///     Reads every stack frame with a source file from the console text, in order.
        /// </summary>
        internal static List<LogStackFrame> ParseFrames(string consoleText)
        {
            var frames = new List<LogStackFrame>();

            if (string.IsNullOrEmpty(consoleText))
            {
                return frames;
            }

            foreach (var rawLine in consoleText.Split('\n'))
            {
                if (TryParseFrame(rawLine.TrimEnd('\r'), out var frame))
                {
                    frames.Add(frame);
                }
            }

            return frames;
        }

        /// <summary>
        ///     Finds the first frame after the last <see cref="Log" /> frame: the code that called the logger.
        /// </summary>
        /// <returns>False when the text holds no <see cref="Log" /> frame, or no frame after it.</returns>
        internal static bool TryFindCaller(string consoleText, out LogStackFrame caller)
        {
            return TryFindCaller(ParseFrames(consoleText), out caller);
        }

        internal static bool TryFindCaller(IReadOnlyList<LogStackFrame> frames, out LogStackFrame caller)
        {
            caller = default;
            var lastLoggerIndex = -1;

            for (var i = 0; i < frames.Count; i++)
            {
                if (IsLoggerFrame(frames[i]))
                {
                    lastLoggerIndex = i;
                }
            }

            if (lastLoggerIndex < 0 || lastLoggerIndex + 1 >= frames.Count)
            {
                return false;
            }

            caller = frames[lastLoggerIndex + 1];

            return true;
        }

        /// <summary>
        ///     True when <paramref name="line" /> is the line of a <see cref="Log" /> frame other than the first one.
        ///     The console opens the first frame on double-click, so such a line means the user clicked a stack trace link
        ///     into the logger on purpose.
        /// </summary>
        internal static bool IsDeeperLoggerLine(IReadOnlyList<LogStackFrame> frames, int line)
        {
            var seenFirst = false;

            foreach (var frame in frames)
            {
                if (!IsLoggerFrame(frame))
                {
                    continue;
                }

                if (seenFirst && frame.Line == line)
                {
                    return true;
                }

                seenFirst = true;
            }

            return false;
        }

        internal static bool IsLoggerFrame(LogStackFrame frame)
        {
            return string.Equals(frame.TypeName, LoggerTypeName, StringComparison.Ordinal);
        }

        internal static bool TryParseFrame(string line, out LogStackFrame frame)
        {
            frame = default;

            if (string.IsNullOrWhiteSpace(line))
            {
                return false;
            }

            var match = ScriptOnlyFrame.Match(line);

            if (!match.Success)
            {
                match = FullFrame.Match(line);
            }

            if (!match.Success || !int.TryParse(match.Groups["line"].Value, out var lineNumber))
            {
                return false;
            }

            frame = new LogStackFrame(
                TypeNameOf(match.Groups["method"].Value),
                match.Groups["file"].Value.Trim(),
                lineNumber
            );

            return true;
        }

        /// <summary>
        ///     <c>SOSXR.CharacterHandler:SetCalmer</c> becomes <c>SOSXR.CharacterHandler</c>.
        ///     Nested types use <c>/</c> in stack traces and <c>+</c> in <see cref="Type.FullName" />.
        /// </summary>
        private static string TypeNameOf(string method)
        {
            var colon = method.IndexOf(':');
            var typeName = colon >= 0 ? method.Substring(0, colon) : method;

            return typeName.Trim().Replace('/', '+');
        }
    }
}
