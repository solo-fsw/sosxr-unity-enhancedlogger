using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    /// <summary>
    ///     Waits for logs to be processed.
    /// </summary>
    protected IEnumerator WaitForLogs()
    {
        yield return null;
    }
}

/// <summary>
///     Tests that each log level shows its own logs.
/// </summary>
[TestFixture]
public class ShowOwnLogs : LogTestBase
{
    [UnityTest]
    public IEnumerator Error_ShowsErrorLogs()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        MockObject.Error("test error");
        yield return WaitForLogs();

        AssertLogCount(1);
        AssertLogContains("test error");
        AssertLogLevelPresent(LogLevel.Error);
    }

    [UnityTest]
    public IEnumerator Warning_ShowsWarningLogs()
    {
        Log.CurrentLogLevel = LogLevel.Warning;
        MockObject.Warning("test warning");
        yield return WaitForLogs();

        AssertLogCount(1);
        AssertLogContains("test warning");
        AssertLogLevelPresent(LogLevel.Warning);
    }

    [UnityTest]
    public IEnumerator Debug_ShowsDebugLogs()
    {
        Log.CurrentLogLevel = LogLevel.Debug;
        MockObject.Debug("test debug");
        yield return WaitForLogs();

        AssertLogCount(1);
        AssertLogContains("test debug");
        AssertLogLevelPresent(LogLevel.Debug);
    }

    [UnityTest]
    public IEnumerator Info_ShowsInfoLogs()
    {
        Log.CurrentLogLevel = LogLevel.Info;
        MockObject.Info("test info");
        yield return WaitForLogs();

        AssertLogCount(1);
        AssertLogContains("test info");
        AssertLogLevelPresent(LogLevel.Info);
    }

    [UnityTest]
    public IEnumerator Success_ShowsSuccessLogs()
    {
        Log.CurrentLogLevel = LogLevel.Success;
        MockObject.Success("test success");
        yield return WaitForLogs();

        AssertLogCount(1);
        AssertLogContains("test success");
        AssertLogLevelPresent(LogLevel.Success);
    }

    [UnityTest]
    public IEnumerator Verbose_ShowsVerboseLogs()
    {
        Log.CurrentLogLevel = LogLevel.Verbose;
        MockObject.Verbose("test verbose");
        yield return WaitForLogs();

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
    [UnityTest]
    public IEnumerator Warning_ShowsErrorLogs()
    {
        Log.CurrentLogLevel = LogLevel.Warning;
        MockObject.Error("error message");
        yield return WaitForLogs();

        AssertLogCount(1);
        AssertLogContains("error message");
        AssertLogContains(Log.GetPrefix(LogLevel.Error));
    }

    [UnityTest]
    public IEnumerator Debug_ShowsWarningAndErrorLogs()
    {
        Log.CurrentLogLevel = LogLevel.Debug;
        MockObject.Error("error");
        MockObject.Warning("warning");
        yield return WaitForLogs();

        AssertLogCount(2, "Expected 2 logs (Error + Warning)");
        AssertLogContains("error");
        AssertLogContains("warning");
        AssertLogContains(Log.GetPrefix(LogLevel.Error));
        AssertLogContains(Log.GetPrefix(LogLevel.Warning));
    }

    [UnityTest]
    public IEnumerator Info_ShowsAllHigherPriorityLogs()
    {
        Log.CurrentLogLevel = LogLevel.Info;
        MockObject.Error("error");
        MockObject.Warning("warning");
        MockObject.Debug("debug");
        yield return WaitForLogs();

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
    [UnityTest]
    public IEnumerator Static_Error_LogsMessage()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        Log.Static("static error", LogLevel.Error);
        yield return WaitForLogs();

        AssertLogCount(1);
        AssertLogContains("static error");
        AssertLogLevelPresent(LogLevel.Error);
    }

    [UnityTest]
    public IEnumerator Static_Warning_LogsMessage()
    {
        Log.CurrentLogLevel = LogLevel.Warning;
        Log.Static("static warning", LogLevel.Warning);
        yield return WaitForLogs();

        AssertLogCount(1);
        AssertLogContains("static warning");
        AssertLogLevelPresent(LogLevel.Warning);
    }

    [UnityTest]
    public IEnumerator Static_DefaultsToDebugLevel()
    {
        Log.CurrentLogLevel = LogLevel.Debug;
        Log.Static("static debug");
        yield return WaitForLogs();

        AssertLogCount(1);
        AssertLogContains("static debug");
        AssertLogLevelPresent(LogLevel.Debug);
    }

    [UnityTest]
    public IEnumerator Static_MultipleMessages_CombinesWithSeparator()
    {
        Log.CurrentLogLevel = LogLevel.Info;
        Log.Static("A", "B", "C", logLevel: LogLevel.Info);
        yield return WaitForLogs();

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
    [UnityTest]
    public IEnumerator Error_TwoMessages_CombinesWithSeparator()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        MockObject.Error("First", "Second");
        yield return WaitForLogs();

        AssertLogCount(1);
        AssertLogContains("First : Second");
    }

    [UnityTest]
    public IEnumerator Error_FourMessages_CombinesAll()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        MockObject.Error("A", "B", "C", "D");
        yield return WaitForLogs();

        AssertLogCount(1);
        AssertLogContains("A : B : C : D");
    }

    [UnityTest]
    public IEnumerator Warning_ThreeMessages_CombinesWithSeparator()
    {
        Log.CurrentLogLevel = LogLevel.Warning;
        MockObject.Warning("One", "Two", "Three");
        yield return WaitForLogs();

        AssertLogCount(1);
        AssertLogContains("One : Two : Three");
    }

    [UnityTest]
    public IEnumerator Debug_TwoMessages_CombinesWithSeparator()
    {
        Log.CurrentLogLevel = LogLevel.Debug;
        MockObject.Debug("Msg1", "Msg2");
        yield return WaitForLogs();

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
        Assert.DoesNotThrow(() => MockObject.Error(null));
    }

    [UnityTest]
    public IEnumerator Error_EmptyMessage_LogsEmpty()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        MockObject.Error("");
        yield return WaitForLogs();

        AssertLogCount(1, "Empty message should still produce a log");
    }

    [UnityTest]
    public IEnumerator Error_LongMessage_HandledCorrectly()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        var longMessage = new string('x', 1000);
        MockObject.Error(longMessage);
        yield return WaitForLogs();

        AssertLogCount(1);
        AssertLogContains(longMessage.Substring(0, 100));
    }

    [UnityTest]
    public IEnumerator Error_MessageWithSpecialCharacters_HandledCorrectly()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        MockObject.Error("Special: {braces} and [brackets]");
        yield return WaitForLogs();

        AssertLogCount(1);
        AssertLogContains("{braces}");
        AssertLogContains("[brackets]");
    }

    [UnityTest]
    public IEnumerator Error_UnicodeMessage_HandledCorrectly()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        MockObject.Error("Unicode: 你好世界 🎮 émojis");
        yield return WaitForLogs();

        AssertLogCount(1);
        AssertLogContains("你好世界");
    }

    [UnityTest]
    public IEnumerator LogLevel_CanBeChangedAtRuntime()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        MockObject.Warning("first - suppressed");
        Log.CurrentLogLevel = LogLevel.Warning;
        MockObject.Warning("second - shown");

        yield return WaitForLogs();

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
///     Tests for file logging functionality.
/// </summary>
[TestFixture]
public class FileLoggingTests
{
    private GameObject _mockObject;

    [SetUp]
    public void Setup()
    {
        _mockObject = new GameObject("TestObject");
        Log.CurrentLogLevel = LogLevel.Info;
    }

    [TearDown]
    public void TearDown()
    {
        if (_mockObject != null)
        {
            UnityEngine.Object.DestroyImmediate(_mockObject);
        }
        if (Log.EnhancedLoggerSettings != null)
        {
            Log.EnhancedLoggerSettings.WriteToFile = false;
        }
    }

    [UnityTest]
    public IEnumerator WriteToFile_Enabled_DoesNotThrow()
    {
        if (Log.EnhancedLoggerSettings == null)
        {
            Assert.Inconclusive("Settings not available - cannot test file logging");
            yield break;
        }

        Log.EnhancedLoggerSettings.WriteToFile = true;
        Assert.DoesNotThrow(() => _mockObject.Error("file test message"));
        yield return null;
    }

    [UnityTest]
    public IEnumerator WriteToFile_MultipleMessages_DoesNotThrow()
    {
        if (Log.EnhancedLoggerSettings == null)
        {
            Assert.Inconclusive("Settings not available");
            yield break;
        }

        Log.EnhancedLoggerSettings.WriteToFile = true;
        Assert.DoesNotThrow(() =>
        {
            _mockObject.Error("message 1");
            _mockObject.Error("message 2");
            _mockObject.Warning("warning");
        });
        yield return null;
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
    [UnityTest]
    public IEnumerator Verbose_ShowsAllLowerPriority()
    {
        Log.CurrentLogLevel = LogLevel.Verbose;
        MockObject.Error("error");
        MockObject.Warning("warning");
        MockObject.Debug("debug");
        MockObject.Info("info");
        MockObject.Success("success");
        MockObject.Verbose("verbose");
        yield return WaitForLogs();
        AssertLogCount(6, "Verbose level should show all 6 log levels");
    }

    [UnityTest]
    public IEnumerator Success_SuppressesVerboseOnly()
    {
        Log.CurrentLogLevel = LogLevel.Success;
        MockObject.Error("error");
        MockObject.Warning("warning");
        MockObject.Debug("debug");
        MockObject.Info("info");
        MockObject.Success("success");
        MockObject.Verbose("suppressed");
        yield return WaitForLogs();
        AssertLogCount(5, "Success level should show 5 logs (all except verbose)");
        AssertLogDoesNotContain("suppressed");
    }

    [UnityTest]
    public IEnumerator Info_SuppressesSuccessAndVerbose()
    {
        Log.CurrentLogLevel = LogLevel.Info;
        MockObject.Error("error");
        MockObject.Warning("warning");
        MockObject.Debug("debug");
        MockObject.Info("info");
        MockObject.Success("suppressed1");
        MockObject.Verbose("suppressed2");
        yield return WaitForLogs();
        AssertLogCount(4, "Info level should show 4 logs");
        AssertLogDoesNotContain("suppressed1");
        AssertLogDoesNotContain("suppressed2");
    }

    [UnityTest]
    public IEnumerator Debug_SuppressesInfoSuccessVerbose()
    {
        Log.CurrentLogLevel = LogLevel.Debug;
        MockObject.Error("error");
        MockObject.Warning("warning");
        MockObject.Debug("debug");
        MockObject.Info("suppressed1");
        MockObject.Success("suppressed2");
        MockObject.Verbose("suppressed3");
        yield return WaitForLogs();
        AssertLogCount(3, "Debug level should show 3 logs");
    }

    [UnityTest]
    public IEnumerator Warning_SuppressesDebugInfoSuccessVerbose()
    {
        Log.CurrentLogLevel = LogLevel.Warning;
        MockObject.Error("error");
        MockObject.Warning("warning");
        MockObject.Debug("suppressed1");
        MockObject.Info("suppressed2");
        MockObject.Success("suppressed3");
        MockObject.Verbose("suppressed4");
        yield return WaitForLogs();
        AssertLogCount(2, "Warning level should show 2 logs");
    }

    [UnityTest]
    public IEnumerator Error_ShowsOnlyError()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        MockObject.Error("error");
        MockObject.Warning("suppressed1");
        MockObject.Debug("suppressed2");
        MockObject.Info("suppressed3");
        MockObject.Success("suppressed4");
        MockObject.Verbose("suppressed5");
        yield return WaitForLogs();
        AssertLogCount(1, "Error level should show only 1 log");
        AssertLogContains("error");
    }
}

[TestFixture]
public class MessageContentTests : LogTestBase
{
    [UnityTest]
    public IEnumerator Log_ContainsObjectName()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        MockObject.name = "MyTestObject";
        MockObject.Error("test");
        yield return WaitForLogs();
        AssertLogContains("MyTestObject");
    }

    [UnityTest]
    public IEnumerator Log_ContainsMethodName()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        MockObject.Error("test");
        yield return WaitForLogs();
        AssertLogContains("Log_ContainsMethodName");
    }

    [UnityTest]
    public IEnumerator Log_ContainsPrefix()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        MockObject.Error("test");
        yield return WaitForLogs();
        AssertLogContains(Log.GetPrefix(LogLevel.Error));
    }

    [UnityTest]
    public IEnumerator Log_ContainsColorTag()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        MockObject.Error("test");
        yield return WaitForLogs();
        Assert.IsTrue(
            CapturedLogs.Any(l => l.Message.Contains("<color=")),
            "Log should contain color tag"
        );
    }

    [UnityTest]
    public IEnumerator StaticLog_DoesNotContainObjectName()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        Log.Static("static test", LogLevel.Error);
        yield return WaitForLogs();
        AssertLogCount(1);
        AssertLogContains("static test");
    }

    [UnityTest]
    public IEnumerator MultipleLogs_MaintainOrder()
    {
        Log.CurrentLogLevel = LogLevel.Info;
        MockObject.Info("first");
        MockObject.Info("second");
        MockObject.Info("third");
        yield return WaitForLogs();
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
    [UnityTest]
    public IEnumerator Log_WithNullObject_DoesNotThrow()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        Assert.DoesNotThrow(() => Log.Static("test", LogLevel.Error));
        yield return WaitForLogs();
        AssertLogCount(1);
    }

    [UnityTest]
    public IEnumerator Log_AfterObjectDestroyed_StillWorks()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        var tempObject = new GameObject("TempObject");
        tempObject.Error("before destroy");
        UnityEngine.Object.DestroyImmediate(tempObject);
        yield return WaitForLogs();
        AssertLogCount(1);
        AssertLogContains("before destroy");
    }

    [UnityTest]
    public IEnumerator Log_MultipleObjects_DistinguishesSources()
    {
        Log.CurrentLogLevel = LogLevel.Info;
        var obj1 = new GameObject("Object1");
        var obj2 = new GameObject("Object2");
        obj1.Info("from object1");
        obj2.Info("from object2");
        yield return WaitForLogs();
        AssertLogCount(2);
        Assert.IsTrue(CapturedLogs.Any(l => l.Message.Contains("Object1")));
        Assert.IsTrue(CapturedLogs.Any(l => l.Message.Contains("Object2")));
        UnityEngine.Object.DestroyImmediate(obj1);
        UnityEngine.Object.DestroyImmediate(obj2);
    }
}

[TestFixture]
public class WriteToFileTests
{
    [Test]
    public void WriteToFile_CacheSize_PropertyWorks()
    {
        var size = WriteToFile.CacheSize;
        Assert.IsTrue(size >= 0, "Cache size should be non-negative");
    }

    [Test]
    public void WriteToFile_Log_DoesNotThrowWithNullSettings()
    {
        Assert.DoesNotThrow(() => WriteToFile.Log("test message"));
    }

    [Test]
    public void WriteToFile_MultipleLogs_DoesNotThrow()
    {
        Assert.DoesNotThrow(() =>
        {
            for (int i = 0; i < 10; i++)
            {
                WriteToFile.Log($"message {i}");
            }
        });
    }
}

[TestFixture]
public class StressTests : LogTestBase
{
    [UnityTest]
    public IEnumerator RapidLogging_HandlesCorrectly()
    {
        Log.CurrentLogLevel = LogLevel.Debug;
        const int logCount = 100;
        for (int i = 0; i < logCount; i++)
        {
            MockObject.Debug($"rapid log {i}");
        }
        yield return WaitForLogs();
        AssertLogCount(logCount, $"Should have {logCount} logs");
    }

    [UnityTest]
    public IEnumerator RapidLevelSwitching_HandlesCorrectly()
    {
        int expectedLogs = 0;
        for (int i = 0; i < 20; i++)
        {
            var level = (LogLevel)(i % 7);
            Log.CurrentLogLevel = level;
            MockObject.Error("test");
            if (level != LogLevel.None)
            {
                expectedLogs++;
            }
        }
        yield return WaitForLogs();
        AssertLogCount(
            expectedLogs,
            $"Should have {expectedLogs} logs (None level suppresses all)"
        );
    }

    [UnityTest]
    public IEnumerator ManyUniqueMessages_HandlesCorrectly()
    {
        Log.CurrentLogLevel = LogLevel.Info;
        const int messageCount = 50;
        for (int i = 0; i < messageCount; i++)
        {
            MockObject.Info($"unique message {i} with GUID {System.Guid.NewGuid()}");
        }
        yield return WaitForLogs();
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
    [UnityTest]
    public IEnumerator Info_WithNullMessage2_SkipsNull()
    {
        Log.CurrentLogLevel = LogLevel.Info;
        MockObject.Info("first", null, "third");
        yield return WaitForLogs();
        AssertLogCount(1);
    }

    [UnityTest]
    public IEnumerator Error_WithWhitespaceMessage_LogsWhitespace()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        MockObject.Error("   ");
        yield return WaitForLogs();
        AssertLogCount(1);
    }

    [UnityTest]
    public IEnumerator Warning_WithTabsAndNewlines_HandledCorrectly()
    {
        Log.CurrentLogLevel = LogLevel.Warning;
        MockObject.Warning("line1\nline2\ttabbed");
        yield return WaitForLogs();
        AssertLogCount(1);
        AssertLogContains("line1");
    }

    [UnityTest]
    public IEnumerator Debug_WithHtmlTags_Preserved()
    {
        Log.CurrentLogLevel = LogLevel.Debug;
        MockObject.Debug("<b>bold</b> <i>italic</i>");
        yield return WaitForLogs();
        AssertLogCount(1);
        AssertLogContains("<b>");
    }

    [UnityTest]
    public IEnumerator Success_WithEmoji_HandledCorrectly()
    {
        Log.CurrentLogLevel = LogLevel.Success;
        MockObject.Success("✅ Success! 🎉");
        yield return WaitForLogs();
        AssertLogCount(1);
    }

    [UnityTest]
    public IEnumerator Verbose_WithMixedContent_HandledCorrectly()
    {
        Log.CurrentLogLevel = LogLevel.Verbose;
        MockObject.Verbose("Numbers: 123, Symbols: @#$%, Text: hello");
        yield return WaitForLogs();
        AssertLogCount(1);
    }

    [UnityTest]
    public IEnumerator Static_WithNullMessage_HandledCorrectly()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        Assert.DoesNotThrow(() => Log.Static(null, LogLevel.Error));
        yield return WaitForLogs();
        AssertLogCount(1);
    }

    [UnityTest]
    public IEnumerator Static_WithEmptyMessages_AllEmpty()
    {
        Log.CurrentLogLevel = LogLevel.Info;
        Log.Static("", "", "", "", LogLevel.Info);
        yield return WaitForLogs();
        AssertLogCount(1);
    }

    [UnityTest]
    public IEnumerator Log_AfterMultipleLevelChanges_WorksCorrectly()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        MockObject.Error("1");
        Log.CurrentLogLevel = LogLevel.Warning;
        MockObject.Warning("2");
        Log.CurrentLogLevel = LogLevel.Debug;
        MockObject.Debug("3");
        Log.CurrentLogLevel = LogLevel.Info;
        MockObject.Info("4");
        yield return WaitForLogs();
        AssertLogCount(4);
    }

    [UnityTest]
    public IEnumerator AllLevels_NoneLevel_SuppressesEverything()
    {
        Log.CurrentLogLevel = LogLevel.None;
        MockObject.Error("e");
        MockObject.Warning("w");
        MockObject.Debug("d");
        MockObject.Info("i");
        MockObject.Success("s");
        MockObject.Verbose("v");
        yield return WaitForLogs();
        AssertLogCount(0);
    }
}

[TestFixture]
public class IntegrationTests : LogTestBase
{
    [UnityTest]
    public IEnumerator FullWorkflow_MultipleObjectsAndLevels()
    {
        var obj1 = new GameObject("Player");
        var obj2 = new GameObject("Enemy");

        Log.CurrentLogLevel = LogLevel.Success;
        obj1.Error("Player error");
        obj1.Warning("Player warning");
        obj2.Debug("Enemy debug");
        obj2.Info("Enemy info");
        obj1.Success("Player success");

        yield return WaitForLogs();

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

    [UnityTest]
    public IEnumerator RealisticScenario_GameLoop()
    {
        Log.CurrentLogLevel = LogLevel.Verbose;

        MockObject.Debug("Game loop started");
        MockObject.Info("Player position updated", "x: 10", "y: 5");
        MockObject.Warning("Low health detected");
        MockObject.Error("Failed to load asset");
        MockObject.Success("Level completed");
        MockObject.Verbose("Frame rendered in 16ms");

        yield return WaitForLogs();
        AssertLogCount(6, "All 6 log levels should appear at Verbose level");
    }

    [UnityTest]
    public IEnumerator BulkLogging_100Messages()
    {
        Log.CurrentLogLevel = LogLevel.Info;
        for (int i = 0; i < 100; i++)
        {
            MockObject.Info($"Bulk message {i}");
        }
        yield return WaitForLogs();
        Assert.AreEqual(100, CapturedLogs.Count);
    }
}

[TestFixture]
public class FinalCoverageTests : LogTestBase
{
    [UnityTest]
    public IEnumerator Error_WithAllMessageParts_4Parts()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        MockObject.Error("A", "B", "C", "D");
        yield return WaitForLogs();
        AssertLogContains("A : B : C : D");
    }

    [UnityTest]
    public IEnumerator Warning_With3Parts_Part4Ignored()
    {
        Log.CurrentLogLevel = LogLevel.Warning;
        MockObject.Warning("A", "B", "C");
        yield return WaitForLogs();
        AssertLogContains("A : B : C");
    }

    [UnityTest]
    public IEnumerator Debug_With2Parts()
    {
        Log.CurrentLogLevel = LogLevel.Debug;
        MockObject.Debug("First", "Second");
        yield return WaitForLogs();
        AssertLogContains("First : Second");
    }

    [UnityTest]
    public IEnumerator Info_SinglePart()
    {
        Log.CurrentLogLevel = LogLevel.Info;
        MockObject.Info("Only");
        yield return WaitForLogs();
        AssertLogContains("Only");
    }

    [UnityTest]
    public IEnumerator Success_EmptyPart2_Ignored()
    {
        Log.CurrentLogLevel = LogLevel.Success;
        MockObject.Success("Main", "");
        yield return WaitForLogs();
        AssertLogContains("Main");
    }

    [UnityTest]
    public IEnumerator Verbose_AllPartsNullExceptFirst()
    {
        Log.CurrentLogLevel = LogLevel.Verbose;
        MockObject.Verbose("Valid", null, null, null);
        yield return WaitForLogs();
        AssertLogContains("Valid");
    }

    [UnityTest]
    public IEnumerator Static_SingleMessage()
    {
        Log.CurrentLogLevel = LogLevel.Debug;
        Log.Static("Single");
        yield return WaitForLogs();
        AssertLogContains("Single");
    }

    [UnityTest]
    public IEnumerator Static_TwoMessages()
    {
        Log.CurrentLogLevel = LogLevel.Debug;
        Log.Static("One", "Two");
        yield return WaitForLogs();
        AssertLogContains("One : Two");
    }

    [UnityTest]
    public IEnumerator Static_FourMessages()
    {
        Log.CurrentLogLevel = LogLevel.Info;
        Log.Static("1", "2", "3", "4", LogLevel.Info);
        yield return WaitForLogs();
        AssertLogContains("1 : 2 : 3 : 4");
    }
}

[TestFixture]
public class WriteToFileEdgeCaseTests
{
    [Test]
    public void WriteToFile_Log_WithBackticks_EscapesCorrectly()
    {
        Assert.DoesNotThrow(() => WriteToFile.Log("Message with `backticks`"));
    }

    [Test]
    public void WriteToFile_Log_WithNewlines_HandledCorrectly()
    {
        Assert.DoesNotThrow(() => WriteToFile.Log("Line 1\nLine 2\nLine 3"));
    }

    [Test]
    public void WriteToFile_Log_EmptyString_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => WriteToFile.Log(""));
    }

    [Test]
    public void WriteToFile_Log_NullMessage_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => WriteToFile.Log(null));
    }

    [Test]
    public void WriteToFile_ManyUniqueMessages_CacheGrows()
    {
        int initialCacheSize = WriteToFile.CacheSize;
        for (int i = 0; i < 100; i++)
        {
            WriteToFile.Log($"Unique message {i}");
        }
        Assert.IsTrue(
            WriteToFile.CacheSize > initialCacheSize,
            "Cache should grow with unique messages"
        );
    }

    [Test]
    public void WriteToFile_DuplicateMessages_CacheStaysSmall()
    {
        int initialCacheSize = WriteToFile.CacheSize;
        for (int i = 0; i < 100; i++)
        {
            WriteToFile.Log("Same message");
        }
        Assert.AreEqual(
            initialCacheSize + 1,
            WriteToFile.CacheSize,
            "Cache should have only 1 entry for duplicates"
        );
    }
}

[TestFixture]
public class StringCombinerTests : LogTestBase
{
    [UnityTest]
    public IEnumerator StringCombiner_AllNullExceptFirst()
    {
        Log.CurrentLogLevel = LogLevel.Info;
        MockObject.Info("Only", null, null, null);
        yield return WaitForLogs();
        AssertLogContains("Only");
    }

    [UnityTest]
    public IEnumerator StringCombiner_MixedNullAndEmpty()
    {
        Log.CurrentLogLevel = LogLevel.Info;
        MockObject.Info("First", "", null, "");
        yield return WaitForLogs();
        AssertLogContains("First");
    }

    [UnityTest]
    public IEnumerator StringCombiner_AllEmptyStrings()
    {
        Log.CurrentLogLevel = LogLevel.Info;
        MockObject.Info("", "", "", "");
        yield return WaitForLogs();
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
    [UnityTest]
    public IEnumerator Log_WithPotentialInjection_Safe()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        MockObject.Error(
            "Test #hashtag @mention <tag> 'quote' \"double\" **bold** __underline__ ~~strike~~ ||spoiler||"
        );
        yield return WaitForLogs();
        AssertLogCount(1);
    }

    [UnityTest]
    public IEnumerator Log_WithVeryLongMessage_Handled()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        string longMessage = new string('X', 5000);
        MockObject.Error(longMessage);
        yield return WaitForLogs();
        AssertLogCount(1);
    }

    [UnityTest]
    public IEnumerator Log_WithPathCharacters_Safe()
    {
        Log.CurrentLogLevel = LogLevel.Error;
        MockObject.Error("Path: C:\\Users\\Test\\File.txt or /home/user/file");
        yield return WaitForLogs();
        AssertLogCount(1);
    }
}
