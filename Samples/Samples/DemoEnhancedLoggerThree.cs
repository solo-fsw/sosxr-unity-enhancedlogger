using SOSXR.EnhancedLogger;
using UnityEngine;


/// <summary>
///     Demonstrates using ContextMenu attributes to trigger logs manually from the Inspector.
///     Right-click this component in the Inspector to see the context menu options for each log level.
///     This is useful for testing different log levels and verifying logger behavior without modifying code.
///     Compare with DemoEnhancedLoggerFour to see the difference between instance methods (this) and static methods (Log.Static()).
/// </summary>
public class DemoEnhancedLoggerThree : MonoBehaviour
{
    [ContextMenu(nameof(MakeVerboseLog))]
    /// <summary>Emits a <see cref="LogLevel.Verbose"/> log via the instance extension method.</summary>
    public void MakeVerboseLog()
    {
        this.Verbose("Some verbose log");
    }


    [ContextMenu(nameof(MakeDebugLog))]
    /// <summary>Emits a <see cref="LogLevel.Debug"/> log via the instance extension method.</summary>
    public void MakeDebugLog()
    {
        this.Debug("Some debug log");
    }


    [ContextMenu(nameof(MakeInfoLog))]
    /// <summary>Emits an <see cref="LogLevel.Info"/> log via the instance extension method.</summary>
    public void MakeInfoLog()
    {
        this.Info("Some info log");
    }


    [ContextMenu(nameof(MakeSuccessLog))]
    /// <summary>Emits a <see cref="LogLevel.Success"/> log via the instance extension method.</summary>
    public void MakeSuccessLog()
    {
        this.Success("Some success log");
    }


    [ContextMenu(nameof(MakeWarningLog))]
    /// <summary>Emits a <see cref="LogLevel.Warning"/> log via the instance extension method.</summary>
    public void MakeWarningLog()
    {
        this.Warning("Some warning log");
    }


    [ContextMenu(nameof(MakeErrorLog))]
    /// <summary>Emits an <see cref="LogLevel.Error"/> log with two message parts via the instance extension method.</summary>
    public void MakeErrorLog()
    {
        this.Error("Some error log", "This is the error message");
    }
}


/*
public static class LogginTesting
{
    public static void LogTest(string message, Object caller, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
    {
        var logMessage = $"{Path.GetFileName(callerFilePath)} - {callerName}({callerLineNumber}): {message}";

        UnityEngine.Debug.LogFormat(LogType.Log, LogOption.None, caller, logMessage);
    }


    public static void LogTest(this Object caller, string message, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
    {
        var logMessage = $"{Path.GetFileName(callerFilePath)} - {callerName}({callerLineNumber}): {message}";

        UnityEngine.Debug.LogFormat(LogType.Log, LogOption.None, caller, logMessage);
    }
}*/