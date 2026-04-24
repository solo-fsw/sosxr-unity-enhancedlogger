using UnityEngine;

public class StackTraceInBuild
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.Full);
        Debug.Log("We set the StackTrace to Full since we're in Dev Build or Editor");
#else
        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        Debug.Log("We set the StackTrace to None since we're in Release Build");
#endif
    }
}
