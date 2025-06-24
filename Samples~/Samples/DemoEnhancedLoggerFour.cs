using SOSXR.EnhancedLogger;
using UnityEngine;


public class DemoEnhancedLoggerFour : MonoBehaviour
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