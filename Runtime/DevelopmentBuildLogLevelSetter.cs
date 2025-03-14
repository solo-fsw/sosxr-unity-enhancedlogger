using UnityEngine;


namespace SOSXR.EnhancedLogger
{
    /// <summary>
    ///     Add this to a GameObject if you want to set the log level in a Development Build to something else than Debug.
    /// </summary>
    public class DevelopmentBuildLogLevelSetter : MonoBehaviour
    {
        [SerializeField] private LogLevel m_developmentBuildLogLevel = LogLevel.Debug;


        private void Awake()
        {
            SetLogLevelInDevelopmentBuild();
        }


        private void SetLogLevelInDevelopmentBuild()
        {
            #if DEVELOPMENT_BUILD
            if (Log.CurrentLogLevel == m_developmentBuildLogLevel)
            {
                return;
            }
            Log.Error("Our current loglevel is", Log.CurrentLogLevel, "and we're changing that to", m_developmentBuildLogLevel);
            Log.CurrentLogLevel = m_developmentBuildLogLevel;
            #else
            Log.Verbose("Our current log level is", Log.CurrentLogLevel, "and we're not changing that to", m_developmentBuildLogLevel);
            #endif
        }
    }
}