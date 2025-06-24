using SOSXR.EnhancedLogger;
using UnityEngine;


public class DemoEnhancedLoggerThree : MonoBehaviour
{
    [ContextMenu(nameof(MakeVerboseLog))]
    public void MakeVerboseLog()
    {
        this.Verbose("Some verbose log");
    }


    [ContextMenu(nameof(MakeDebugLog))]
    public void MakeDebugLog()
    {
        this.Debug("Some debug log");
    }


    [ContextMenu(nameof(MakeInfoLog))]
    public void MakeInfoLog()
    {
        this.Info("Some info log");
    }


    [ContextMenu(nameof(MakeSuccessLog))]
    public void MakeSuccessLog()
    {
        this.Success("Some success log");
    }


    [ContextMenu(nameof(MakeWarningLog))]
    public void MakeWarningLog()
    {
        this.Warning("Some warning log");
    }


    [ContextMenu(nameof(MakeErrorLog))]
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