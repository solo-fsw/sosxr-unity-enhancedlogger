using SOSXR.EnhancedLogger;
using UnityEngine;


/// <summary>
///     Add this to a GameObject in the Scene.
///     Sometimes the object you're calling the Log from will be deleted by the time the log fires.
///     To make sure you don't get a NullReferenceException, you can use the static Log class.
///     The first parameter functions as the object name.
/// </summary>
public class DemoEnhancedLoggerTwo : MonoBehaviour
{
    [Header("Hit it")]
    [SerializeField] private bool m_destroyThisGameObject = false;


    private void Update()
    {
        if (m_destroyThisGameObject)
        {
            DestroyImmediate(gameObject);
        }

        Log.Error("DisabledObject", "This is still shown even though the gameobject will be destroyed.");

        Log.Debug("This is a debug message", "I forgot to set the name as the first parameter. It looks a little weird now", "You can see this Log because the current log level is", Log.CurrentLogLevel);

        Log.Success(nameof(gameObject), "This is a success message. It is shown when the log level is set to Success, or Info. But not when it's set to None, Error, or Warning.");

        Log.Info(nameof(DemoEnhancedLoggerTwo), "This is an info message.", "It is only shown when the log level is set to Info");
    }
}