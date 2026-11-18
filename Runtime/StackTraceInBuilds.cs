using UnityEngine;

/// <summary>
///     Sets the stack trace type for <see cref="LogType.Log" /> messages in players.
///     Development builds: Full (managed and native frames). Release builds: None.
///     The Editor is left alone: it keeps the setting from the Console menu (Stack Trace Logging) or Player Settings.
///     Full stack traces in the Editor stop the Console from opening the calling code on double-click.
/// </summary>
public class StackTraceInBuild
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        var stackTraceType = StackTraceTypeFor(Application.isEditor, Debug.isDebugBuild);

        if (stackTraceType == null)
        {
            return;
        }

        Application.SetStackTraceLogType(LogType.Log, stackTraceType.Value);
        Debug.Log($"We set the StackTrace to {stackTraceType.Value} since we're in a {(Debug.isDebugBuild ? "Development" : "Release")} Build");
    }

    /// <summary>
    ///     Stack trace type to use for <see cref="LogType.Log" />, or null to leave the current setting unchanged (Editor).
    ///     Uses <see cref="Debug.isDebugBuild" /> at runtime: the DEVELOPMENT_BUILD symbol is deprecated in Unity 6.6 (UAC0009).
    /// </summary>
    internal static StackTraceLogType? StackTraceTypeFor(bool isEditor, bool isDebugBuild)
    {
        if (isEditor)
        {
            return null;
        }

        return isDebugBuild ? StackTraceLogType.Full : StackTraceLogType.None;
    }
}
