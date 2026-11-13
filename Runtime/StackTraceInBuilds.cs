using UnityEngine;

public class StackTraceInBuild
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        // Debug.isDebugBuild is true in the Editor and in Development builds.
        // Runtime check replaces the DEVELOPMENT_BUILD symbol, which is deprecated in Unity 6.6 (UAC0009).
        if (Debug.isDebugBuild)
        {
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.Full);
            Debug.Log("We set the StackTrace to Full since we're in Dev Build or Editor");
        }
        else
        {
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Debug.Log("We set the StackTrace to None since we're in Release Build");
        }
    }
}
