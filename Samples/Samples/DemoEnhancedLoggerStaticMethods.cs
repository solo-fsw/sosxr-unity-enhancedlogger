using SOSXR.EnhancedLogger;
using UnityEngine;

/// <summary>
///     Demonstrates using Log.Static() for logging from static contexts or when the caller object may be destroyed.
///     Right-click this component in the Inspector to see the context menu options for each log level.
///     Use Log.Static() instead of instance methods when: (1) logging from static methods, (2) the caller object will be destroyed,
///     or (3) you don't have access to a MonoBehaviour instance. Note that Log.Static() loses the ability to click in the console
///     to navigate to the source code line, but it prevents NullReferenceExceptions when the caller is destroyed.
///     Compare with DemoEnhancedLoggerThree to see the difference between instance methods (this) and static methods (Log.Static()).
/// </summary>
public class DemoEnhancedLoggerStaticMethods : MonoBehaviour
{
    [ContextMenu(nameof(MakeVerboseLog))]
    public void MakeVerboseLog()
    {
        Log.Static("Some verbose log, directly from the static Log class", LogLevel.Verbose);
    }

    [ContextMenu(nameof(MakeDebugLog))]
    public void MakeDebugLog()
    {
        Log.Static("Some debug log, directly from the static Log class");
    }

    [ContextMenu(nameof(MakeInfoLog))]
    public void MakeInfoLog()
    {
        Log.Static("Some info log, directly from the static Log class", LogLevel.Info);
    }

    [ContextMenu(nameof(MakeSuccessLog))]
    public void MakeSuccessLog()
    {
        Log.Static("Some success log, directly from the static Log class", LogLevel.Success);
    }

    [ContextMenu(nameof(MakeWarningLog))]
    public void MakeWarningLog()
    {
        Log.Static("Some warning log, directly from the static Log class", LogLevel.Warning);
    }

    [ContextMenu(nameof(MakeErrorLog))]
    public void MakeErrorLog()
    {
        Log.Static("Some error log, directly from the static Log class", LogLevel.Error);
    }
}
