using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SOSXR.EnhancedLogger.EditorScripts
{
    /// <summary>
    ///     Before each build, adds the project's settings asset to the Preloaded Assets in Player Settings.
    ///     Players load Preloaded Assets at startup, so the logger has its settings without a Resources folder.
    ///     Other <see cref="EnhancedLoggerSettings" /> entries are removed from the list, so only one is preloaded.
    /// </summary>
    public class EnhancedLoggerSettingsBuild : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            _ = Log.CurrentLogLevel; // Loads the settings, or creates the asset when the project has none.

            var settings = Log.FindSettingsInProject();

            if (settings == null)
            {
                Debug.LogWarning("EnhancedLogger: no settings asset found. The build uses default settings.");

                return;
            }

            var current = PlayerSettings.GetPreloadedAssets();
            var updated = WithSettings(current, settings);

            if (!updated.SequenceEqual(current))
            {
                PlayerSettings.SetPreloadedAssets(updated);
            }
        }

        /// <summary>
        ///     Returns <paramref name="preloaded" /> with <paramref name="settings" /> as its only <see cref="EnhancedLoggerSettings" />.
        ///     Keeps all other entries, including empty ones, in their order.
        /// </summary>
        internal static Object[] WithSettings(IReadOnlyList<Object> preloaded, EnhancedLoggerSettings settings)
        {
            var result = new List<Object>();
            var added = false;

            foreach (var asset in preloaded ?? new Object[0])
            {
                if (asset is EnhancedLoggerSettings)
                {
                    if (!added && asset == settings)
                    {
                        result.Add(asset);
                        added = true;
                    }

                    continue;
                }

                result.Add(asset);
            }

            if (!added)
            {
                result.Add(settings);
            }

            return result.ToArray();
        }
    }
}
