using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using SOSXR.EnhancedLogger;
using UnityEngine;
using UnityEngine.TestTools;

[TestFixture]
public class WriteToFileRegressionTests
{
    private string _directory;
    private string _file;
    private FileLogSession _session;

    [SetUp]
    public void SetUp()
    {
        _directory = Path.Combine(Path.GetTempPath(), "EnhancedLoggerTests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_directory);
        _file = Path.Combine(_directory, "session.md");
        _session = new FileLogSession(_directory, _file, "Regression test");
    }

    [TearDown]
    public void TearDown()
    {
        if (_directory == null || !Directory.Exists(_directory)) return;

        // Delete only paths created by these tests, never unknown files.
        foreach (var name in new[] { "session.md", "other.md", "old.md", "old.md.lock", "old.md.lock.v2", ".retention.lock", "saved.md" })
        {
            var path = Path.Combine(_directory, name);
            if (File.Exists(path)) File.Delete(path);
        }
        if (Directory.Exists(_file)) Directory.Delete(_file);
        var missing = Path.Combine(_directory, "missing");
        var missingFile = Path.Combine(missing, "session.md");
        if (File.Exists(missingFile)) File.Delete(missingFile);
        if (Directory.Exists(missing)) Directory.Delete(missing);
        Directory.Delete(_directory);
    }

    [Test]
    public void Messages_AreWrittenInOrderIncludingDuplicates()
    {
        _session.Log("order-first");
        _session.Log("order-repeat");
        _session.Log("order-middle");
        _session.Log("order-repeat");
        _session.Log("order-last");

        var lines = File.ReadAllLines(_file).Where(line => line.Contains("order-")).ToArray();
        Assert.AreEqual(5, lines.Length);
        StringAssert.Contains("order-first", lines[0]);
        StringAssert.Contains("order-repeat", lines[1]);
        StringAssert.Contains("order-middle", lines[2]);
        StringAssert.Contains("order-repeat", lines[3]);
        StringAssert.Contains("order-last", lines[4]);
        Assert.AreEqual(0, _session.CacheSize);
    }

    [Test]
    public void ConcurrentWriters_PreserveEveryMessageAndEachWorkersOrder()
    {
        const int workers = 8;
        const int messagesPerWorker = 40;
        Parallel.For(0, workers, worker =>
        {
            for (var message = 0; message < messagesPerWorker; message++)
                _session.Log($"entry-w{worker}-{message:D3}-end");
        });

        var lines = File.ReadAllLines(_file).Where(line => line.Contains("entry-w")).ToArray();
        Assert.AreEqual(workers * messagesPerWorker, lines.Length);
        for (var worker = 0; worker < workers; worker++)
        {
            var workerLines = lines.Where(line => line.Contains($"entry-w{worker}-")).ToArray();
            Assert.AreEqual(messagesPerWorker, workerLines.Length);
            for (var message = 0; message < messagesPerWorker; message++)
                StringAssert.Contains($"entry-w{worker}-{message:D3}-end", workerLines[message]);
        }
    }

    [Test]
    public void Log_WritesAndClosesBeforeReturning()
    {
        _session.Log("before-flush");
        var before = File.ReadAllText(_file);
        StringAssert.Contains("before-flush", before);
        using (new FileStream(_file, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) { }

        Assert.AreEqual(before, File.ReadAllText(_file));

        _session.Log("second-message");
        StringAssert.Contains("second-message", File.ReadAllText(_file));
        Assert.AreEqual(0, _session.CacheSize);
    }

    [Test]
    public void ExistingDestination_IsPreservedAndDisablesSessionWithOneWarning()
    {
        File.WriteAllText(_file, "pre-existing content");

        ExpectOneWarning(() =>
        {
            _session.Log("collision-first");
            _session.Log("collision-second");
        });

        Assert.AreEqual("pre-existing content", File.ReadAllText(_file));
        Assert.AreEqual(0, _session.CacheSize);
    }

    [Test]
    public void ConcurrentFailure_WarnsOnceAndPreservesExistingFile()
    {
        File.WriteAllText(_file, "pre-existing content");
        var warnings = 0;
        void Capture(string message, string stackTrace, LogType type)
        {
            if (type == LogType.Warning && message.StartsWith("[WriteToFile] File logging disabled for this session:"))
                System.Threading.Interlocked.Increment(ref warnings);
        }

        Application.logMessageReceivedThreaded += Capture;
        try
        {
            LogAssert.Expect(LogType.Warning, new Regex(@"^\[WriteToFile\] File logging disabled for this session:"));
            Parallel.For(0, 8, i => _session.Log($"collision-{i}"));
            Assert.AreEqual(1, warnings);
            Assert.AreEqual("pre-existing content", File.ReadAllText(_file));
            LogAssert.NoUnexpectedReceived();
        }
        finally
        {
            Application.logMessageReceivedThreaded -= Capture;
        }
    }

    [Test]
    public void MissingDirectory_FailureWarnsOnceAndDoesNotRetryAfterRecovery()
    {
        var missing = Path.Combine(_directory, "missing");
        var file = Path.Combine(missing, "session.md");
        var session = new FileLogSession(_directory, file, "Missing directory");

        ExpectOneWarning(() =>
        {
            session.Log("first-failure");
            Directory.CreateDirectory(missing);
            session.Log("must-not-retry");
        });

        Assert.IsFalse(File.Exists(file));
        Assert.AreEqual(0, session.CacheSize);
    }

    [Test]
    public void AppendFailure_WarnsOnceAndDoesNotRetryAfterRecovery()
    {
        _session.Log("committed-before-failure");
        var saved = Path.Combine(_directory, "saved.md");
        File.Move(_file, saved);
        Directory.CreateDirectory(_file);

        ExpectOneWarning(() =>
        {
            _session.Log("failed-append");
            Directory.Delete(_file);
            File.Move(saved, _file);
            _session.Log("must-not-retry");
        });

        var contents = File.ReadAllText(_file);
        StringAssert.Contains("committed-before-failure", contents);
        StringAssert.DoesNotContain("failed-append", contents);
        StringAssert.DoesNotContain("must-not-retry", contents);
    }

    [Test]
    public void IndependentSessions_WriteOnlyTheirOwnFiles()
    {
        var otherFile = Path.Combine(_directory, "other.md");
        var other = new FileLogSession(_directory, otherFile, "Other session");
        _session.Log("first-session-message");
        other.Log("other-session-message");

        var first = File.ReadAllText(_file);
        var second = File.ReadAllText(otherFile);
        StringAssert.Contains("first-session-message", first);
        StringAssert.DoesNotContain("other-session-message", first);
        StringAssert.Contains("other-session-message", second);
        StringAssert.DoesNotContain("first-session-message", second);
    }

    [Test]
    public void Initialize_GeneratesDistinctSafePathsWithoutCreatingFiles()
    {
        var first = new FileLogSession();
        var second = new FileLogSession();
        first.Initialize();
        second.Initialize();
        var pathField = typeof(FileLogSession).GetField("_filePath", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        var firstPath = (string)pathField.GetValue(first);
        var secondPath = (string)pathField.GetValue(second);

        if (string.IsNullOrEmpty(Application.persistentDataPath))
        {
            Assert.IsNull(firstPath);
            Assert.IsNull(secondPath);
            return;
        }
        Assert.IsNotNull(firstPath);
        Assert.AreNotEqual(firstPath, secondPath);
        StringAssert.StartsWith("session_", Path.GetFileName(firstPath));
        StringAssert.EndsWith(".md", firstPath);
        Assert.IsFalse(File.Exists(firstPath));
        Assert.IsFalse(File.Exists(secondPath));
    }

    [Test]
    public void UninitializedSession_WarnsOnceWithoutWriting()
    {
        var uninitialized = new FileLogSession();
        ExpectOneWarning(() =>
        {
            uninitialized.Log("first message");
            uninitialized.Log("second message");
        });
        Assert.AreEqual(0, uninitialized.CacheSize);

        uninitialized.Initialize();
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        typeof(FileLogSession).GetField("_folderPath", flags).SetValue(uninitialized, _directory);
        typeof(FileLogSession).GetField("_filePath", flags).SetValue(uninitialized, _file);
        uninitialized.Log("after initialization");
        var contents = File.ReadAllText(_file);
        StringAssert.Contains("after initialization", contents);
        StringAssert.DoesNotContain("first message", contents);
    }

    [Test]
    public void EndSession_PreventsWritingToPreviousSession()
    {
        _session.Log("before session end");
        var previous = File.ReadAllText(_file);
        _session.EndSession();

        ExpectOneWarning(() =>
        {
            _session.Log("after session end");
            _session.Log("another late message");
        });
        Assert.AreEqual(previous, File.ReadAllText(_file));
    }

    [Test]
    public void Writing_PreservesExistingLogsAndMarkerFiles()
    {
        var originalFiles = new Dictionary<string, string>
        {
            ["old.md"] = "old log content",
            ["old.md.lock"] = "legacy marker",
            ["old.md.lock.v2"] = "versioned marker",
            [".retention.lock"] = "coordinator marker",
        };
        foreach (var item in originalFiles)
            File.WriteAllText(Path.Combine(_directory, item.Key), item.Value);

        _session.Log("new-session-message");
        foreach (var item in originalFiles)
            Assert.AreEqual(item.Value, File.ReadAllText(Path.Combine(_directory, item.Key)), item.Key);
        StringAssert.Contains("new-session-message", File.ReadAllText(_file));
    }

    private static void ExpectOneWarning(Action action)
    {
        var warnings = new List<string>();
        void Capture(string message, string stackTrace, LogType type)
        {
            if (type == LogType.Warning) warnings.Add(message);
        }

        Application.logMessageReceived += Capture;
        try
        {
            LogAssert.Expect(LogType.Warning, new Regex(@"^\[WriteToFile\] File logging disabled for this session:"));
            action();
            Assert.AreEqual(1, warnings.Count, "A failed session must warn exactly once.");
            LogAssert.NoUnexpectedReceived();
        }
        finally
        {
            Application.logMessageReceived -= Capture;
        }
    }
}
