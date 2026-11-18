using UnityEditor;
using UnityEngine;

namespace SOSXR.EnhancedLogger.EditorScripts
{
    /// <summary>
    ///     Shows the <see cref="EnhancedLoggerSettings" /> in <c>Preferences &gt; SOSXR &gt; EnhancedLogger</c>.
    ///     The values are stored in the project asset <c>Assets/_SOSXR/Settings/EnhancedLoggerSettings.asset</c>,
    ///     so they are shared with everyone who uses the project and are included in builds.
    ///     Also opened via the menu <c>SOSXR &gt; EnhancedLogger &gt; Preferences</c>.
    /// </summary>
    public static class EnhancedLoggerPreferences
    {
        internal const string PreferencesPath = "Preferences/SOSXR/EnhancedLogger";

        private static readonly string[] Keywords =
        {
            "Log Level", "Prefix", "Color", "Write To File", "Logger", "Logging", "Console"
        };

        private static SerializedObject s_serializedSettings;

        [SettingsProvider]
        internal static SettingsProvider CreateProvider()
        {
            return new SettingsProvider(PreferencesPath, SettingsScope.User)
            {
                label = "EnhancedLogger",
                guiHandler = _ => DrawSettings(),
                keywords = Keywords
            };
        }

        [MenuItem("SOSXR/EnhancedLogger/Preferences", false, 0)]
        private static void OpenPreferences()
        {
            SettingsService.OpenUserPreferences(PreferencesPath);
        }

        private static void DrawSettings()
        {
            _ = Log.CurrentLogLevel; // Loads the settings, or creates the asset when the project has none.
            var settings = Log.EnhancedLoggerSettings;

            if (settings == null)
            {
                EditorGUILayout.HelpBox("No EnhancedLoggerSettings found.", MessageType.Warning);

                return;
            }

            if (s_serializedSettings == null || s_serializedSettings.targetObject != settings)
            {
                s_serializedSettings = new SerializedObject(settings);
            }

            s_serializedSettings.Update();

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField("Settings Asset", settings, typeof(EnhancedLoggerSettings), false);
            }

            EditorGUILayout.Space();

            var property = s_serializedSettings.GetIterator();
            var enterChildren = true;

            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;

                if (property.propertyPath == "m_Script")
                {
                    continue;
                }

                EditorGUILayout.PropertyField(property, true);
            }

            if (s_serializedSettings.ApplyModifiedProperties() && EditorUtility.IsPersistent(settings))
            {
                AssetDatabase.SaveAssetIfDirty(settings);
            }
        }
    }
}
