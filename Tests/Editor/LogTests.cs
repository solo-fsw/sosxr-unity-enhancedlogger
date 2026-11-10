using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using SOSXR.EnhancedLogger;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
///     Base class for logging tests providing common setup, cleanup, and helper methods.
/// </summary>
public abstract class LogTestBase
{
    protected GameObject MockObject { get; private set; }
    protected List<LogEntry> CapturedLogs { get; private set; }
    private EnhancedLoggerSettings _originalSettings;
    private EnhancedLoggerSettings _testSettings;

    protected class LogEntry
    {
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public LogType Type { get; set; }
        public DateTime Timestamp { get; set; }
    }

    [SetUp]
    public void BaseSetup()
    {
        _ = Log.CurrentLogLevel;
        _originalSettings = Log.EnhancedLoggerSettings;
        _testSettings = ScriptableObject.CreateInstance<EnhancedLoggerSettings>();
        _testSettings.WriteToFile = false;
        Log.EnhancedLoggerSettings = _testSettings;
        MockObject = new GameObject("TestObject");
        CapturedLogs = new List<LogEntry>();
        Application.logMessageReceived += CaptureLog;
        Log.CurrentLogLevel = LogLevel.Info;
    }

    [TearDown]
    public void BaseTearDown()
    {
        Application.logMessageReceived -= CaptureLog;

        if (MockObject != null)
        {
            UnityEngine.Object.DestroyImmediate(MockObject);
        }

        CapturedLogs?.Clear();
        Log.EnhancedLoggerSettings = _originalSettings;
        UnityEngine.Object.DestroyImmediate(_testSettings);
    }

    private void CaptureLog(string logString, string stackTrace, LogType type)
    {
        CapturedLogs.Add(
            new LogEntry
            {
                Message = logString,
                StackTrace = stackTrace,
                Type = type,
                Timestamp = DateTime.Now,
            }
        );
    }

    protected void ExpectError(string message)
    {
        var prefix = Regex.Escape(Log.GetPrefix(LogLevel.Error));
        var payload = Regex.Escape(message ?? string.Empty);
        LogAssert.Expect(
            LogType.Error,
            new Regex(@"\A<color=#[0-9a-fA-F]{6,8}>" + prefix + @" \| LogTests \([^\r\n]* : \d+\)</color> : " + payload + @"\n\z")
        );
    }

    protected sealed class ContextCapturingLogHandler : ILogHandler
    {
        private readonly ILogHandler _inner;
        public readonly List<UnityEngine.Object> Contexts = new List<UnityEngine.Object>();

        public ContextCapturingLogHandler(ILogHandler inner) => _inner = inner;

        public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
        {
            Contexts.Add(context);
            _inner.LogFormat(logType, context, format, args);
        }

        public void LogException(Exception exception, UnityEngine.Object context)
        {
            _inner.LogException(exception, context);
        }
    }

    /// <summary>
    ///     Asserts that at least one log contains the expected text.
    /// </summary>
    protected void AssertLogContains(string expectedText, string message = null)
    {
        var found = CapturedLogs.Any(l => l.Message.Contains(expectedText));
        var actualMessages = string.Join("\n", CapturedLogs.Select(l => l.Message));
        var fullMessage =
            message ?? $"Expected log containing '{expectedText}' but found:\n{actualMessages}";
        Assert.IsTrue(found, fullMessage);
    }

    /// <summary>
    ///     Asserts that exactly N logs were captured.
    /// </summary>
    protected void AssertLogCount(int expectedCount, string message = null)
    {
        var fullMessage =
            message ?? $"Expected {expectedCount} logs but found {CapturedLogs.Count}";
        Assert.AreEqual(expectedCount, CapturedLogs.Count, fullMessage);
    }

    /// <summary>
    ///     Asserts that no logs contain the forbidden text.
    /// </summary>
    protected void AssertLogDoesNotContain(string forbiddenText, string message = null)
    {
        var found = CapturedLogs.Any(l => l.Message.Contains(forbiddenText));
        var fullMessage = message ?? $"Found unexpected log containing '{forbiddenText}'";
        Assert.IsFalse(found, fullMessage);
    }

    /// <summary>
    ///     Asserts that a log was captured with the specified level's prefix and color.
    /// </summary>
    protected void AssertLogLevelPresent(LogLevel level)
    {
        var prefix = Log.GetPrefix(level);
        AssertLogContains(prefix, $"Expected log with {level} prefix '{prefix}'");
    }
}

/// <summary>
///     Tests that each log level shows its own logs.
/// </summary>
[TestFixture]
public class ShowOwnLogs : LogTestBase
{
    [Test]
    public void Error_ShowsErrorLogs()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        ExpectError("test error");
        MockObject.Error("test error");

        AssertLogCount(1);
        AssertLogContains("test error");
        AssertLogLevelPresent(LogLevel.Error);
        Assert.AreEqual(LogType.Error, CapturedLogs[0].Type);
    }

    [Test]
    public void Warning_ShowsWarningLogs()
    {
        Log.CurrentLogLevel = LogLevel.Warning;
        MockObject.Warning("test warning");

        AssertLogCount(1);
        AssertLogContains("test warning");
        AssertLogLevelPresent(LogLevel.Warning);
        Assert.AreEqual(LogType.Warning, CapturedLogs[0].Type);
    }

    [Test]
    public void Debug_ShowsDebugLogs()
    {
        Log.CurrentLogLevel = LogLevel.Debug;
        MockObject.Debug("test debug");

        AssertLogCount(1);
        AssertLogContains("test debug");
        AssertLogLevelPresent(LogLevel.Debug);
    }

    [Test]
    public void Info_ShowsInfoLogs()
    {
        Log.CurrentLogLevel = LogLevel.Info;
        MockObject.Info("test info");

        AssertLogCount(1);
        AssertLogContains("test info");
        AssertLogLevelPresent(LogLevel.Info);
    }

    [Test]
    public void Success_ShowsSuccessLogs()
    {
        Log.CurrentLogLevel = LogLevel.Success;
        MockObject.Success("test success");

        AssertLogCount(1);
        AssertLogContains("test success");
        AssertLogLevelPresent(LogLevel.Success);
    }

    [Test]
    public void Verbose_ShowsVerboseLogs()
    {
        Log.CurrentLogLevel = LogLevel.Verbose;
        MockObject.Verbose("test verbose");

        AssertLogCount(1);
        AssertLogContains("test verbose");
        AssertLogLevelPresent(LogLevel.Verbose);
    }
}

/// <summary>
///     Tests that higher priority logs are shown when lower priority level is set.
/// </summary>
[TestFixture]
public class ShowHigherPriorityLogs : LogTestBase
{
    [Test]
    public void Warning_ShowsErrorLogs()
    {
        Log.CurrentLogLevel = LogLevel.Warning;
        ExpectError("error message");
        MockObject.Error("error message");

        AssertLogCount(1);
        AssertLogContains("error message");
        AssertLogContains(Log.GetPrefix(LogLevel.Error));
    }

    [Test]
    public void Debug_ShowsWarningAndErrorLogs()
    {
        Log.CurrentLogLevel = LogLevel.Debug;
        ExpectError("error");
        MockObject.Error("error");
        MockObject.Warning("warning");

        AssertLogCount(2, "Expected 2 logs (Error + Warning)");
        AssertLogContains("error");
        AssertLogContains("warning");
        AssertLogContains(Log.GetPrefix(LogLevel.Error));
        AssertLogContains(Log.GetPrefix(LogLevel.Warning));
    }

    [Test]
    public void Info_ShowsAllHigherPriorityLogs()
    {
        Log.CurrentLogLevel = LogLevel.Info;
        ExpectError("error");
        MockObject.Error("error");
        MockObject.Warning("warning");
        MockObject.Debug("debug");

        AssertLogCount(3, "Expected 3 logs (Error + Warning + Debug)");
        AssertLogContains("error");
        AssertLogContains("warning");
        AssertLogContains("debug");
    }
}

/// <summary>
///     Tests that lower priority logs are suppressed when higher priority level is set.
/// </summary>
[TestFixture]
public class SuppressLowerPriorityLogs : LogTestBase
{
    [Test]
    public void Error_SuppressesWarningAndLowerLogs()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        MockObject.Warning("suppressed");
        MockObject.Debug("suppressed");
        MockObject.Info("suppressed");
        MockObject.Success("suppressed");
        MockObject.Verbose("suppressed");

        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public void Warning_SuppressesDebugAndLowerLogs()
    {
        Log.CurrentLogLevel = LogLevel.Warning;
        MockObject.Debug("suppressed");
        MockObject.Info("suppressed");
        MockObject.Success("suppressed");
        MockObject.Verbose("suppressed");

        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public void None_SuppressesAllLogs()
    {
        Log.CurrentLogLevel = LogLevel.None;
        MockObject.Error("suppressed");
        MockObject.Warning("suppressed");
        MockObject.Debug("suppressed");
        MockObject.Info("suppressed");
        MockObject.Success("suppressed");
        MockObject.Verbose("suppressed");

        LogAssert.NoUnexpectedReceived();
    }
}

/// <summary>
///     Tests for static Log methods.
/// </summary>
[TestFixture]
public class StaticLogMethods : LogTestBase
{
    [Test]
    public void Static_Error_LogsMessage()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        ExpectError("static error");
        Log.Static("static error", LogLevel.Error);

        AssertLogCount(1);
        AssertLogContains("static error");
        AssertLogLevelPresent(LogLevel.Error);
    }

    [Test]
    public void Static_Warning_LogsMessage()
    {
        Log.CurrentLogLevel = LogLevel.Warning;
        Log.Static("static warning", LogLevel.Warning);

        AssertLogCount(1);
        AssertLogContains("static warning");
        AssertLogLevelPresent(LogLevel.Warning);
    }

    [Test]
    public void Static_DefaultsToDebugLevel()
    {
        Log.CurrentLogLevel = LogLevel.Debug;
        Log.Static("static debug");

        AssertLogCount(1);
        AssertLogContains("static debug");
        AssertLogLevelPresent(LogLevel.Debug);
    }

    [Test]
    public void Static_MultipleMessages_CombinesWithSeparator()
    {
        Log.CurrentLogLevel = LogLevel.Info;
        Log.Static("A", "B", "C", logLevel: LogLevel.Info);

        AssertLogCount(1);
        AssertLogContains("A : B : C");
    }
}

/// <summary>
///     Tests for multiple message overloads.
/// </summary>
[TestFixture]
public class MultipleMessageOverloads : LogTestBase
{
    [Test]
    public void Error_TwoMessages_CombinesWithSeparator()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        ExpectError("First : Second");
        MockObject.Error("First", "Second");

        AssertLogCount(1);
        AssertLogContains("First : Second");
    }

    [Test]
    public void Error_FourMessages_CombinesAll()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        ExpectError("A : B : C : D");
        MockObject.Error("A", "B", "C", "D");

        AssertLogCount(1);
        AssertLogContains("A : B : C : D");
    }

    [Test]
    public void Warning_ThreeMessages_CombinesWithSeparator()
    {
        Log.CurrentLogLevel = LogLevel.Warning;
        MockObject.Warning("One", "Two", "Three");

        AssertLogCount(1);
        AssertLogContains("One : Two : Three");
    }

    [Test]
    public void Debug_TwoMessages_CombinesWithSeparator()
    {
        Log.CurrentLogLevel = LogLevel.Debug;
        MockObject.Debug("Msg1", "Msg2");

        AssertLogCount(1);
        AssertLogContains("Msg1 : Msg2");
    }
}

/// <summary>
///     Tests for edge cases and error handling.
/// </summary>
[TestFixture]
public class EdgeCases : LogTestBase
{
    [Test]
    public void Error_NullMessage_DoesNotThrow()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        ExpectError(null);
        Assert.DoesNotThrow(() => MockObject.Error(null));
    }

    [Test]
    public void Error_EmptyMessage_LogsEmpty()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        ExpectError("");
        MockObject.Error("");

        AssertLogCount(1, "Empty message should still produce a log");
    }

    [Test]
    public void Error_LongMessage_HandledCorrectly()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        var longMessage = new string('x', 1000);
        ExpectError(longMessage);
        MockObject.Error(longMessage);

        AssertLogCount(1);
        AssertLogContains(longMessage.Substring(0, 100));
    }

    [Test]
    public void Error_MessageWithSpecialCharacters_HandledCorrectly()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        ExpectError("Special: {braces} and [brackets]");
        MockObject.Error("Special: {braces} and [brackets]");

        AssertLogCount(1);
        AssertLogContains("{braces}");
        AssertLogContains("[brackets]");
    }

    [Test]
    public void Error_UnicodeMessage_HandledCorrectly()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        ExpectError("Unicode: 你好世界 🎮 émojis");
        MockObject.Error("Unicode: 你好世界 🎮 émojis");

        AssertLogCount(1);
        AssertLogContains("你好世界");
    }

    [Test]
    public void LogLevel_CanBeChangedAtRuntime()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        MockObject.Warning("first - suppressed");
        Log.CurrentLogLevel = LogLevel.Warning;
        MockObject.Warning("second - shown");

        AssertLogCount(1, "Only second log should appear");
        AssertLogContains("second - shown");
        AssertLogDoesNotContain("first - suppressed");
    }

    [Test]
    public void GetPrefix_ReturnsDefault_WhenSettingsNull()
    {
        var prefix = Log.GetPrefix(LogLevel.Error);
        Assert.IsNotNull(prefix);
        Assert.IsNotEmpty(prefix);
    }

    [Test]
    public void GetColor_ReturnsDefault_WhenSettingsNull()
    {
        var color = Log.GetColor(LogLevel.Error);
        Assert.AreNotEqual(default(Color), color);
    }
}

/// <summary>
///     Tests for settings management.
/// </summary>
[TestFixture]
public class SettingsTests
{
    [Test]
    public void CurrentLogLevel_CanBeSetAndRetrieved()
    {
        var originalLevel = Log.CurrentLogLevel;

        try
        {
            Log.CurrentLogLevel = LogLevel.Warning;
            Assert.AreEqual(LogLevel.Warning, Log.CurrentLogLevel);
            Log.CurrentLogLevel = LogLevel.Debug;
            Assert.AreEqual(LogLevel.Debug, Log.CurrentLogLevel);
        }
        finally
        {
            Log.CurrentLogLevel = originalLevel;
        }
    }

    [Test]
    public void GetPrefix_ReturnsCorrectPrefixForEachLevel()
    {
        var errorPrefix = Log.GetPrefix(LogLevel.Error);
        var warningPrefix = Log.GetPrefix(LogLevel.Warning);
        var debugPrefix = Log.GetPrefix(LogLevel.Debug);
        var infoPrefix = Log.GetPrefix(LogLevel.Info);
        var successPrefix = Log.GetPrefix(LogLevel.Success);
        var verbosePrefix = Log.GetPrefix(LogLevel.Verbose);

        Assert.IsNotNull(errorPrefix);
        Assert.IsNotNull(warningPrefix);
        Assert.IsNotNull(debugPrefix);
        Assert.IsNotNull(infoPrefix);
        Assert.IsNotNull(successPrefix);
        Assert.IsNotNull(verbosePrefix);
        Assert.AreNotEqual(errorPrefix, warningPrefix);
    }

    [Test]
    public void GetColor_ReturnsValidColorForEachLevel()
    {
        var errorColor = Log.GetColor(LogLevel.Error);
        var warningColor = Log.GetColor(LogLevel.Warning);
        var debugColor = Log.GetColor(LogLevel.Debug);

        Assert.AreNotEqual(default(Color), errorColor);
        Assert.AreNotEqual(default(Color), warningColor);
        Assert.AreNotEqual(default(Color), debugColor);
        Assert.AreNotEqual(errorColor, warningColor);
    }

    [Test]
    public void Settings_NotNull()
    {
        var settings = Log.EnhancedLoggerSettings;
        Assert.IsNotNull(settings, "Settings should be available");
    }
}

[TestFixture]
public class LogLevelCombinationsTests : LogTestBase
{
    [Test]
    public void Verbose_ShowsAllLowerPriority()
    {
        Log.CurrentLogLevel = LogLevel.Verbose;
        ExpectError("error");
        MockObject.Error("error");
        MockObject.Warning("warning");
        MockObject.Debug("debug");
        MockObject.Info("info");
        MockObject.Success("success");
        MockObject.Verbose("verbose");
        AssertLogCount(6, "Verbose level should show all 6 log levels");
    }

    [Test]
    public void Success_SuppressesVerboseOnly()
    {
        Log.CurrentLogLevel = LogLevel.Success;
        ExpectError("error");
        MockObject.Error("error");
        MockObject.Warning("warning");
        MockObject.Debug("debug");
        MockObject.Info("info");
        MockObject.Success("success");
        MockObject.Verbose("suppressed");
        AssertLogCount(5, "Success level should show 5 logs (all except verbose)");
        AssertLogDoesNotContain("suppressed");
    }

    [Test]
    public void Info_SuppressesSuccessAndVerbose()
    {
        Log.CurrentLogLevel = LogLevel.Info;
        ExpectError("error");
        MockObject.Error("error");
        MockObject.Warning("warning");
        MockObject.Debug("debug");
        MockObject.Info("info");
        MockObject.Success("suppressed1");
        MockObject.Verbose("suppressed2");
        AssertLogCount(4, "Info level should show 4 logs");
        AssertLogDoesNotContain("suppressed1");
        AssertLogDoesNotContain("suppressed2");
    }

    [Test]
    public void Debug_SuppressesInfoSuccessVerbose()
    {
        Log.CurrentLogLevel = LogLevel.Debug;
        ExpectError("error");
        MockObject.Error("error");
        MockObject.Warning("warning");
        MockObject.Debug("debug");
        MockObject.Info("suppressed1");
        MockObject.Success("suppressed2");
        MockObject.Verbose("suppressed3");
        AssertLogCount(3, "Debug level should show 3 logs");
    }

    [Test]
    public void Warning_SuppressesDebugInfoSuccessVerbose()
    {
        Log.CurrentLogLevel = LogLevel.Warning;
        ExpectError("error");
        MockObject.Error("error");
        MockObject.Warning("warning");
        MockObject.Debug("suppressed1");
        MockObject.Info("suppressed2");
        MockObject.Success("suppressed3");
        MockObject.Verbose("suppressed4");
        AssertLogCount(2, "Warning level should show 2 logs");
    }

    [Test]
    public void Error_ShowsOnlyError()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        ExpectError("error");
        MockObject.Error("error");
        MockObject.Warning("suppressed1");
        MockObject.Debug("suppressed2");
        MockObject.Info("suppressed3");
        MockObject.Success("suppressed4");
        MockObject.Verbose("suppressed5");
        AssertLogCount(1, "Error level should show only 1 log");
        AssertLogContains("error");
    }
}

[TestFixture]
public class MessageContentTests : LogTestBase
{
    [Test]
    public void Log_UsesObjectAsContext()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        MockObject.name = "MyTestObject";
        var originalHandler = UnityEngine.Debug.unityLogger.logHandler;
        var handler = new ContextCapturingLogHandler(originalHandler);
        try
        {
            UnityEngine.Debug.unityLogger.logHandler = handler;
            ExpectError("test");
            MockObject.Error("test");
        }
        finally
        {
            UnityEngine.Debug.unityLogger.logHandler = originalHandler;
        }
        AssertLogCount(1);
        AssertLogContains("test");
        Assert.AreEqual(1, handler.Contexts.Count);
        Assert.AreSame(MockObject, handler.Contexts[0]);
    }

    [Test]
    public void Log_ContainsMethodName()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        ExpectError("test");
        MockObject.Error("test");
        AssertLogContains("Log_ContainsMethodName");
    }

    [Test]
    public void Log_ContainsPrefix()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        ExpectError("test");
        MockObject.Error("test");
        AssertLogContains(Log.GetPrefix(LogLevel.Error));
    }

    [Test]
    public void Log_ContainsColorTag()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        ExpectError("test");
        MockObject.Error("test");
        Assert.IsTrue(
            CapturedLogs.Any(l => l.Message.Contains("<color=")),
            "Log should contain color tag"
        );
    }

    [Test]
    public void StaticLog_DoesNotContainObjectName()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        ExpectError("static test");
        Log.Static("static test", LogLevel.Error);
        AssertLogCount(1);
        AssertLogContains("static test");
    }

    [Test]
    public void MultipleLogs_MaintainOrder()
    {
        Log.CurrentLogLevel = LogLevel.Info;
        MockObject.Info("first");
        MockObject.Info("second");
        MockObject.Info("third");
        AssertLogCount(3);
        var firstLog = CapturedLogs[0].Message;
        var secondLog = CapturedLogs[1].Message;
        var thirdLog = CapturedLogs[2].Message;
        Assert.IsTrue(
            firstLog.Contains("first")
                && secondLog.Contains("second")
                && thirdLog.Contains("third"),
            "Logs should maintain order"
        );
    }
}

[TestFixture]
public class GameObjectTests : LogTestBase
{
    [Test]
    public void Log_WithNullObject_DoesNotThrow()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        ExpectError("test");
        Assert.DoesNotThrow(() => Log.Static("test", LogLevel.Error));
        AssertLogCount(1);
    }

    [Test]
    public void Log_AfterObjectDestroyed_StillWorks()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        var tempObject = new GameObject("TempObject");
        ExpectError("before destroy");
        tempObject.Error("before destroy");
        UnityEngine.Object.DestroyImmediate(tempObject);
        AssertLogCount(1);
        AssertLogContains("before destroy");
    }

    [Test]
    public void Log_MultipleObjects_DistinguishesSources()
    {
        Log.CurrentLogLevel = LogLevel.Info;
        var obj1 = new GameObject("Object1");
        var obj2 = new GameObject("Object2");
        var originalHandler = UnityEngine.Debug.unityLogger.logHandler;
        var handler = new ContextCapturingLogHandler(originalHandler);
        try
        {
            UnityEngine.Debug.unityLogger.logHandler = handler;
            obj1.Info("from object1");
            obj2.Info("from object2");
            Assert.AreEqual(2, handler.Contexts.Count);
            Assert.AreSame(obj1, handler.Contexts[0]);
            Assert.AreSame(obj2, handler.Contexts[1]);
        }
        finally
        {
            UnityEngine.Debug.unityLogger.logHandler = originalHandler;
            UnityEngine.Object.DestroyImmediate(obj1);
            UnityEngine.Object.DestroyImmediate(obj2);
        }
        AssertLogCount(2);
        AssertLogContains("from object1");
        AssertLogContains("from object2");
    }
}

public abstract class IsolatedFileLogTestBase
{
    private string _directory;
    internal FileLogSession Session { get; private set; }
    protected string FilePath { get; private set; }

    [SetUp]
    public void SetUpFileSession()
    {
        _directory = Path.Combine(Path.GetTempPath(), "EnhancedLoggerTests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_directory);
        FilePath = Path.Combine(_directory, "session.md");
        Session = new FileLogSession(_directory, FilePath, "Test");
    }

    [TearDown]
    public void TearDownFileSession()
    {
        if (FilePath != null && File.Exists(FilePath)) File.Delete(FilePath);
        if (_directory != null && Directory.Exists(_directory)) Directory.Delete(_directory);
    }
}

[TestFixture]
public class WriteToFileTests : IsolatedFileLogTestBase
{
    [Test]
    public void WriteToFile_CacheSize_RemainsZero()
    {
        Assert.AreEqual(0, Session.CacheSize);
        Session.Log("test message");
        Assert.AreEqual(0, Session.CacheSize);
    }

    [Test]
    public void WriteToFile_Log_DoesNotThrowWithNullSettings()
    {
        Assert.DoesNotThrow(() => Session.Log("test message"));
    }

    [Test]
    public void WriteToFile_MultipleLogs_DoesNotThrow()
    {
        Assert.DoesNotThrow(() =>
        {
            for (int i = 0; i < 10; i++)
            {
                Session.Log($"message {i}");
            }
        });
    }
}

[TestFixture]
public class StressTests : LogTestBase
{
    [Test]
    public void RapidLogging_HandlesCorrectly()
    {
        Log.CurrentLogLevel = LogLevel.Debug;
        const int logCount = 100;
        for (int i = 0; i < logCount; i++)
        {
            MockObject.Debug($"rapid log {i}");
        }
        AssertLogCount(logCount, $"Should have {logCount} logs");
    }

    [Test]
    public void RapidLevelSwitching_HandlesCorrectly()
    {
        int expectedLogs = 0;
        for (int i = 0; i < 20; i++)
        {
            var level = (LogLevel)(i % 7);
            Log.CurrentLogLevel = level;
            if (level != LogLevel.None)
            {
                ExpectError("test");
            }
            MockObject.Error("test");
            if (level != LogLevel.None)
            {
                expectedLogs++;
            }
        }
        AssertLogCount(
            expectedLogs,
            $"Should have {expectedLogs} logs (None level suppresses all)"
        );
    }

    [Test]
    public void ManyUniqueMessages_HandlesCorrectly()
    {
        Log.CurrentLogLevel = LogLevel.Info;
        const int messageCount = 50;
        for (int i = 0; i < messageCount; i++)
        {
            MockObject.Info($"unique message {i} with GUID {System.Guid.NewGuid()}");
        }
        AssertLogCount(messageCount);
    }
}

[TestFixture]
public class LogLevelEnumTests
{
    [Test]
    public void LogLevel_Values_AreInCorrectOrder()
    {
        Assert.IsTrue((int)LogLevel.None < (int)LogLevel.Error);
        Assert.IsTrue((int)LogLevel.Error < (int)LogLevel.Warning);
        Assert.IsTrue((int)LogLevel.Warning < (int)LogLevel.Debug);
        Assert.IsTrue((int)LogLevel.Debug < (int)LogLevel.Info);
        Assert.IsTrue((int)LogLevel.Info < (int)LogLevel.Success);
        Assert.IsTrue((int)LogLevel.Success < (int)LogLevel.Verbose);
    }

    [Test]
    public void LogLevel_Count_IsSeven()
    {
        var values = (LogLevel[])Enum.GetValues(typeof(LogLevel));
        Assert.AreEqual(7, values.Length, "Should have 7 log levels");
    }

    [Test]
    public void LogLevel_HasAllExpectedValues()
    {
        Assert.IsTrue(Enum.IsDefined(typeof(LogLevel), LogLevel.None));
        Assert.IsTrue(Enum.IsDefined(typeof(LogLevel), LogLevel.Error));
        Assert.IsTrue(Enum.IsDefined(typeof(LogLevel), LogLevel.Warning));
        Assert.IsTrue(Enum.IsDefined(typeof(LogLevel), LogLevel.Debug));
        Assert.IsTrue(Enum.IsDefined(typeof(LogLevel), LogLevel.Info));
        Assert.IsTrue(Enum.IsDefined(typeof(LogLevel), LogLevel.Success));
        Assert.IsTrue(Enum.IsDefined(typeof(LogLevel), LogLevel.Verbose));
    }
}

[TestFixture]
public class AdditionalEdgeCaseTests : LogTestBase
{
    [Test]
    public void Info_WithNullMessage2_SkipsNull()
    {
        Log.CurrentLogLevel = LogLevel.Info;
        MockObject.Info("first", null, "third");
        AssertLogCount(1);
    }

    [Test]
    public void Error_WithWhitespaceMessage_LogsWhitespace()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        ExpectError("   ");
        MockObject.Error("   ");
        AssertLogCount(1);
    }

    [Test]
    public void Warning_WithTabsAndNewlines_HandledCorrectly()
    {
        Log.CurrentLogLevel = LogLevel.Warning;
        MockObject.Warning("line1\nline2\ttabbed");
        AssertLogCount(1);
        AssertLogContains("line1");
    }

    [Test]
    public void Debug_WithHtmlTags_Preserved()
    {
        Log.CurrentLogLevel = LogLevel.Debug;
        MockObject.Debug("<b>bold</b> <i>italic</i>");
        AssertLogCount(1);
        AssertLogContains("<b>");
    }

    [Test]
    public void Success_WithEmoji_HandledCorrectly()
    {
        Log.CurrentLogLevel = LogLevel.Success;
        MockObject.Success("✅ Success! 🎉");
        AssertLogCount(1);
    }

    [Test]
    public void Verbose_WithMixedContent_HandledCorrectly()
    {
        Log.CurrentLogLevel = LogLevel.Verbose;
        MockObject.Verbose("Numbers: 123, Symbols: @#$%, Text: hello");
        AssertLogCount(1);
    }

    [Test]
    public void Static_WithNullMessage_HandledCorrectly()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        ExpectError(null);
        Assert.DoesNotThrow(() => Log.Static(null, LogLevel.Error));
        AssertLogCount(1);
    }

    [Test]
    public void Static_WithEmptyMessages_AllEmpty()
    {
        Log.CurrentLogLevel = LogLevel.Info;
        Log.Static("", "", "", "", LogLevel.Info);
        AssertLogCount(1);
    }

    [Test]
    public void Log_AfterMultipleLevelChanges_WorksCorrectly()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        ExpectError("1");
        MockObject.Error("1");
        Log.CurrentLogLevel = LogLevel.Warning;
        MockObject.Warning("2");
        Log.CurrentLogLevel = LogLevel.Debug;
        MockObject.Debug("3");
        Log.CurrentLogLevel = LogLevel.Info;
        MockObject.Info("4");
        AssertLogCount(4);
    }

    [Test]
    public void AllLevels_NoneLevel_SuppressesEverything()
    {
        Log.CurrentLogLevel = LogLevel.None;
        MockObject.Error("e");
        MockObject.Warning("w");
        MockObject.Debug("d");
        MockObject.Info("i");
        MockObject.Success("s");
        MockObject.Verbose("v");
        AssertLogCount(0);
    }
}

[TestFixture]
public class IntegrationTests : LogTestBase
{
    [Test]
    public void FullWorkflow_MultipleObjectsAndLevels()
    {
        var obj1 = new GameObject("Player");
        var obj2 = new GameObject("Enemy");

        Log.CurrentLogLevel = LogLevel.Success;
        ExpectError("Player error");
        obj1.Error("Player error");
        obj1.Warning("Player warning");
        obj2.Debug("Enemy debug");
        obj2.Info("Enemy info");
        obj1.Success("Player success");

        AssertLogCount(5, "Should see Error, Warning, Debug, Info, Success (not Verbose)");
        Assert.IsTrue(
            CapturedLogs.Any(l => l.Message.Contains("Player") && l.Message.Contains("ERROR"))
        );
        Assert.IsTrue(
            CapturedLogs.Any(l => l.Message.Contains("Enemy") && l.Message.Contains("INFORM"))
        );

        UnityEngine.Object.DestroyImmediate(obj1);
        UnityEngine.Object.DestroyImmediate(obj2);
    }

    [Test]
    public void RealisticScenario_GameLoop()
    {
        Log.CurrentLogLevel = LogLevel.Verbose;

        MockObject.Debug("Game loop started");
        MockObject.Info("Player position updated", "x: 10", "y: 5");
        MockObject.Warning("Low health detected");
        ExpectError("Failed to load asset");
        MockObject.Error("Failed to load asset");
        MockObject.Success("Level completed");
        MockObject.Verbose("Frame rendered in 16ms");
        AssertLogCount(6, "All 6 log levels should appear at Verbose level");
    }

    [Test]
    public void BulkLogging_100Messages()
    {
        Log.CurrentLogLevel = LogLevel.Info;
        for (int i = 0; i < 100; i++)
        {
            MockObject.Info($"Bulk message {i}");
        }
        Assert.AreEqual(100, CapturedLogs.Count);
    }
}

[TestFixture]
public class FinalCoverageTests : LogTestBase
{
    [Test]
    public void Error_WithAllMessageParts_4Parts()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        ExpectError("A : B : C : D");
        MockObject.Error("A", "B", "C", "D");
        AssertLogContains("A : B : C : D");
    }

    [Test]
    public void Warning_With3Parts_Part4Ignored()
    {
        Log.CurrentLogLevel = LogLevel.Warning;
        MockObject.Warning("A", "B", "C");
        AssertLogContains("A : B : C");
    }

    [Test]
    public void Debug_With2Parts()
    {
        Log.CurrentLogLevel = LogLevel.Debug;
        MockObject.Debug("First", "Second");
        AssertLogContains("First : Second");
    }

    [Test]
    public void Info_SinglePart()
    {
        Log.CurrentLogLevel = LogLevel.Info;
        MockObject.Info("Only");
        AssertLogContains("Only");
    }

    [Test]
    public void Success_EmptyPart2_Ignored()
    {
        Log.CurrentLogLevel = LogLevel.Success;
        MockObject.Success("Main", "");
        AssertLogContains("Main");
    }

    [Test]
    public void Verbose_AllPartsNullExceptFirst()
    {
        Log.CurrentLogLevel = LogLevel.Verbose;
        MockObject.Verbose("Valid", null, null, null);
        AssertLogContains("Valid");
    }

    [Test]
    public void Static_SingleMessage()
    {
        Log.CurrentLogLevel = LogLevel.Debug;
        Log.Static("Single");
        AssertLogContains("Single");
    }

    [Test]
    public void Static_TwoMessages()
    {
        Log.CurrentLogLevel = LogLevel.Debug;
        Log.Static("One", "Two");
        AssertLogContains("One : Two");
    }

    [Test]
    public void Static_FourMessages()
    {
        Log.CurrentLogLevel = LogLevel.Info;
        Log.Static("1", "2", "3", "4", LogLevel.Info);
        AssertLogContains("1 : 2 : 3 : 4");
    }
}

[TestFixture]
public class WriteToFileEdgeCaseTests : IsolatedFileLogTestBase
{
    [Test]
    public void WriteToFile_Log_WithBackticks_EscapesCorrectly()
    {
        Session.Log("Message with `backticks`");
        StringAssert.Contains("Message with \\`backticks\\`", File.ReadAllText(FilePath));
    }

    [Test]
    public void WriteToFile_Log_WithNewlines_HandledCorrectly()
    {
        Session.Log("Line 1\r\nLine 2\nLine 3");
        var lines = File.ReadAllLines(FilePath);
        Assert.AreEqual(1, lines.Count(line => line.Contains("Line 1")));
        Assert.IsTrue(lines.Any(line => line.Contains("Line 1") && line.Contains("Line 2") && line.Contains("Line 3")));
    }

    [Test]
    public void WriteToFile_Log_EmptyString_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => Session.Log(""));
    }

    [Test]
    public void WriteToFile_Log_NullMessage_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => Session.Log(null));
    }
}

[TestFixture]
public class StringCombinerTests : LogTestBase
{
    [Test]
    public void StringCombiner_AllNullExceptFirst()
    {
        Log.CurrentLogLevel = LogLevel.Info;
        MockObject.Info("Only", null, null, null);
        AssertLogContains("Only");
    }

    [Test]
    public void StringCombiner_MixedNullAndEmpty()
    {
        Log.CurrentLogLevel = LogLevel.Info;
        MockObject.Info("First", "", null, "");
        AssertLogContains("First");
    }

    [Test]
    public void StringCombiner_AllEmptyStrings()
    {
        Log.CurrentLogLevel = LogLevel.Info;
        MockObject.Info("", "", "", "");
        AssertLogCount(1);
    }
}

[TestFixture]
public class SettingsCreationTests
{
    [Test]
    public void Settings_AssetExists_AfterAccess()
    {
        var settings = Log.EnhancedLoggerSettings;
        Assert.IsNotNull(settings, "Settings asset should exist after first access");
    }

    [Test]
    public void Settings_DefaultValues_AreValid()
    {
        var settings = Log.EnhancedLoggerSettings;
        Assert.IsNotNull(settings.ErrorPrefix);
        Assert.IsNotNull(settings.WarningPrefix);
        Assert.IsNotNull(settings.DebugPrefix);
        Assert.IsNotNull(settings.InfoPrefix);
        Assert.IsNotNull(settings.SuccessPrefix);
        Assert.IsNotNull(settings.VerbosePrefix);
    }

    [Test]
    public void Settings_CanToggleFileLogging()
    {
        var settings = Log.EnhancedLoggerSettings;
        bool originalValue = settings.WriteToFile;

        settings.WriteToFile = !originalValue;
        Assert.AreEqual(!originalValue, settings.WriteToFile);

        settings.WriteToFile = originalValue;
        Assert.AreEqual(originalValue, settings.WriteToFile);
    }
}

[TestFixture]
public class SecurityAndSafetyTests : LogTestBase
{
    [Test]
    public void Log_WithPotentialInjection_Safe()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        ExpectError("Test #hashtag @mention <tag> 'quote' \"double\" **bold** __underline__ ~~strike~~ ||spoiler||");
        MockObject.Error(
            "Test #hashtag @mention <tag> 'quote' \"double\" **bold** __underline__ ~~strike~~ ||spoiler||"
        );
        AssertLogCount(1);
    }

    [Test]
    public void Log_WithVeryLongMessage_Handled()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        string longMessage = new string('X', 5000);
        ExpectError(longMessage);
        MockObject.Error(longMessage);
        AssertLogCount(1);
    }

    [Test]
    public void Log_WithPathCharacters_Safe()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        ExpectError("Path: C:\\Users\\Test\\File.txt or /home/user/file");
        MockObject.Error("Path: C:\\Users\\Test\\File.txt or /home/user/file");
        AssertLogCount(1);
    }
}
