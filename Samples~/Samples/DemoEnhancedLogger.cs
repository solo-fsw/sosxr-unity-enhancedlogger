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
    [SerializeField]
    private bool m_destroyThisGameObject;

    [Header("You can call logs unto other objects")]
    public GameObject OtherGameObject;

    /// <summary>
    ///     I don't recommend using this many logs in the Update method, but it's just for demo purposes.
    ///     Regardless, if you do want to use this in Update, you will be fine, since all are stripped in Release builds.
    /// </summary>
    private void Update()
    {
        if (m_destroyThisGameObject)
        {
            Log.Static("I TOLD YOU NOT TO DO THIS :)");
            DestroyImmediate(gameObject);
        }

        this.Warning(
            $"This is a warning message, it is shown when the log level is set to Warning, Info, Debug, Success, or Verbose. The current log level is {Log.CurrentLogLevel.ToString()}"
        );

        this.Success(
            $"This is a success message. This message is shown when the log level is set to {nameof(LogLevel.Success)}, but not when it's set to {nameof(LogLevel.None)}, {nameof(LogLevel.Error)}, {nameof(LogLevel.Warning)}, {nameof(LogLevel.Debug)} or {nameof(LogLevel.Info)}"
        );

        Camera.main.Verbose(
            "This is a message on another object. Usually you'd use the EnhancedLogger on the current object (by using it as an extension method to the current object: this.Verbose(...)). However, you can run it on another object as well. In this example it runs on the main camera by using the extension method on that object: Camera.main.Verbose(...)."
        );

        if (OtherGameObject == null)
        {
            this.Error("The 'OtherGameObject' has not been set!");
        }
        else
        {
            OtherGameObject.Verbose(
                "... yet we're still able to show a log on it, as long as it's set in the Inspector!"
            );
        }

        this.Verbose(
            "Did you know that none of these logs are shown in a Release build? They get blocked by the compiler, so they don't slow down your game."
        );

        this.Success(
            "However, you can still see them in the Editor, and in a Development build. Hurray :)"
        );

        this.Error(
            "This is an error message, it is shown when the log level is set to Error, Warning, Debug, Success, or Verbose, but not when it's set to None"
        );
    }
}
