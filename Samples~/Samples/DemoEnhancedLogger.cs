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
            Log.Error("I TOLD YOU NOT TO DO THIS :)", "You're gonna get a real NullReferenceException now.");
            DestroyImmediate(gameObject);
        }

        this.Warning("This is a warning message", "It is shown when the log level is set to Warning, Debug, Success, or Info", "The current log level is", Log.CurrentLogLevel);

        Camera.main.Debug("This is a debug message on another object");

        this.Success("This is a success message", "We can do string interpolation", $"It is shown when the log level is set to {Log.CurrentLogLevel} or {LogLevel.Info}, but not when it's set to {LogLevel.None}, {LogLevel.Error}, or {LogLevel.Warning}.");

        if (otherGameObject != null)
        {
            otherGameObject.Info("Yet I am showing this Info log..?");
        }
        else
        {
            this.Error("The 'OtherGameObject' has not been set!");
        }

        this.Info("Did you know that none of these logs are shown in a Release build?", "They get blocked by the compiler, so they don't slow down your game. Hopefully.");

        this.Success("However, you can still see them in the Editor, and in a Development build. Hurray :)");
    }
}