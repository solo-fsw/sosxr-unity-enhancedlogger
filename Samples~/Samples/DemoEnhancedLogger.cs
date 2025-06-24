using SOSXR.EnhancedLogger;
using UnityEngine;


/// <summary>
///     Add this to a GameObject in the Scene.
///     A short demo to show how the Enhanced Logger works
///     Check the Scene view to set the CurrentLogLevel to see the different log levels
/// </summary>
public class DemoEnhancedLogger : MonoBehaviour
{
    [Header("Don't flick this switch")]
    [SerializeField] private bool m_destroyThisGameObject = false;

    [Header("You can call logs unto other objects")]
    public GameObject otherGameObject;


    /// <summary>
    ///     I don't recommend using this many logs in the Update method, but it's just for demo purposes
    /// </summary>
    private void Update()
    {
        if (m_destroyThisGameObject)
        {
            Log.Static("I TOLD YOU NOT TO DO THIS :), You're gonna get a real NullReferenceException now.");
            DestroyImmediate(gameObject);
        }

        this.Warning($"This is a warning message, it is shown when the log level is set to Warning, Info, Debug, Success, or Verbose. The current log level i {Log.CurrentLogLevel.ToString()}");

        Camera.main.Debug("This is a debug message on another object");

        this.Success($"This is a success message. We can do string interpolation. It is shown when the log level is set to {Log.CurrentLogLevel.ToString()} or {nameof(LogLevel.Verbose)}, but not when it's set to {nameof(LogLevel.None)}, {nameof(LogLevel.Error)}, or {nameof(LogLevel.Warning)}.");

        if (otherGameObject)
        {
            otherGameObject.Verbose("Yet I am showing this Info log..?");
        }
        else
        {
            this.Error("The 'OtherGameObject' has not been set!");
        }

        this.Verbose("Did you know that none of these logs are shown in a Release build?, They get blocked by the compiler, so they don't slow down your game. Hopefully.");

        this.Success("However, you can still see them in the Editor, and in a Development build. Hurray :)");

        this.Error("This is an error message, it is shown when the log level is set to Error, Warning, Debug, Success, or Verbose. But not when it's set to None or Info.", "This is the error message that will be shown in the console. It can be used to provide more context about the error.");
    }
}