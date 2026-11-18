using SOSXR.EnhancedLogger;
using UnityEngine;

/// <summary>
///     Demonstrates using ContextMenu attributes to trigger logs manually from the Inspector.
///     Right-click this component in the Inspector to see the context menu options for each log level.
///     This is useful for testing different log levels and verifying logger behavior without modifying code.
///     Compare with DemoEnhancedLoggerFour to see the difference between instance methods (this) and static methods (Log.Static()).
/// </summary>
public class DemoEnhancedLoggerInstanceMethods : MonoBehaviour
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
