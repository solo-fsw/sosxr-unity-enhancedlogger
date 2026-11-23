using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SOSXR.EnhancedLogger.EditorScripts
{
    /// <summary>
    ///     Creates buttons in the Scene view to change the log level
    ///     The current log level is shown in Green in the Scene view
    ///     Also adds menu items to change the log level (see the Logging menu)
    ///     Log level is persistent from Play mode to Edit mode and vice versa
    /// </summary>
    public static class LogButtons
    {
        // A static class, not a MonoBehaviour: it lives in an Editor-only assembly and only holds static code.
        private const string ShowButtonsPrefKey = "SOSXR.EnhancedLogger.ShowSceneViewButtons";

        /// <summary>Whether the log level buttons are drawn in the Scene view. Stored per user in EditorPrefs. Default: true.</summary>
        public static bool ShowSceneViewButtons
        {
            get => EditorPrefs.GetBool(ShowButtonsPrefKey, true);
            set
            {
                EditorPrefs.SetBool(ShowButtonsPrefKey, value);
                SceneView.RepaintAll();
            }
        }

        // Calculate the total number of buttons needed by getting the highest enum value and adding 1.
        // This ensures we have a button for each log level, including the "None" level.
        private static readonly int _buttonCount =
            (int)Enum.GetValues(typeof(LogLevel)).Cast<LogLevel>().Last() + 1; // We want to get the Max count of the enum LogLevel.
        private const string _menuPath = "SOSXR/EnhancedLogger/";
        private const float _buttonWidth = 80f;
        private const float _buttonHeight = 20f;
        private const float _margin_hor = 25;
        private const float _margin_vert = 50;

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (!ShowSceneViewButtons)
            {
                return;
            }

            // Read the level once per GUI event. Don't use Log.CurrentLogLevel here: it creates the settings asset
            // when it is missing, and drawing the Scene view must not add files to the project.
            var currentLevel = GetDisplayedLogLevel();

            Handles.BeginGUI();

            // Get the size of the Scene view
            var sceneViewSize = new Vector2(sceneView.position.width, sceneView.position.height);

            // Position buttons in the bottom-right corner of the Scene view.
            // The coordinate system has (0,0) at the top-left, so we subtract from the scene view dimensions
            // to place buttons at the bottom-right. The rect dimensions include margins for proper spacing.
            var verticalButtonRect = new Rect(
                sceneViewSize.x - (_buttonWidth + _margin_hor),
                sceneViewSize.y - (_buttonHeight * _buttonCount + _margin_vert),
                _buttonWidth + _margin_hor,
                _buttonHeight * _buttonCount + _margin_vert
            );

            GUILayout.BeginArea(verticalButtonRect);

            GUILayout.BeginVertical();

            if (CreateButton(nameof(None), currentLevel))
            {
                None();
            }

            if (CreateButton(nameof(Error), currentLevel))
            {
                Error();
            }

            if (CreateButton(nameof(Warning), currentLevel))
            {
                Warning();
            }

            if (CreateButton(nameof(Debug), currentLevel))
            {
                Debug();
            }

            if (CreateButton(nameof(Info), currentLevel))
            {
                Info();
            }

            if (CreateButton(nameof(Success), currentLevel))
            {
                Success();
            }

            if (CreateButton(nameof(Verbose), currentLevel))
            {
                Verbose();
            }

            GUILayout.EndVertical();

            GUILayout.EndArea();

            Handles.EndGUI();
        }

        /// <summary>The level shown by the buttons: from the loaded or existing settings asset, without creating one.</summary>
        private static LogLevel GetDisplayedLogLevel()
        {
            var settings = Log.EnhancedLoggerSettings ? Log.EnhancedLoggerSettings : Log.FindSettingsInProject();

            return settings ? settings.CurrentLogLevel : LogLevel.Info;
        }

        private static bool CreateButton(string text, LogLevel currentLevel)
        {
            var isSelected = currentLevel.ToString() == text;
            var previousColor = GUI.backgroundColor;

            if (isSelected)
            {
                GUI.backgroundColor = Color.green; // Highlight the selected button with a different color
            }

            var result = GUILayout.Button(
                text,
                GUILayout.Width(_buttonWidth),
                GUILayout.Height(_buttonHeight)
            );

            GUI.backgroundColor = previousColor; // Restore the color the Scene view used

            return result;
        }

        [MenuItem(_menuPath + "Show Scene View Buttons", false, 50)]
        private static void ToggleSceneViewButtons()
        {
            ShowSceneViewButtons = !ShowSceneViewButtons;
        }

        [MenuItem(_menuPath + "Show Scene View Buttons", true)]
        private static bool ToggleSceneViewButtonsValidate()
        {
            Menu.SetChecked(_menuPath + "Show Scene View Buttons", ShowSceneViewButtons);

            return true;
        }

        [MenuItem("SOSXR/Folders/" + nameof(EnhancedLogger), false, 100)]
        [MenuItem(_menuPath + "Stored Logs Folder", false, 100)]
        private static void EnhancedLogger()
        {
            var fullPath = Path.Combine(Application.persistentDataPath, "EnhancedLogger");

            if (Directory.Exists(fullPath))
            {
                EditorUtility.RevealInFinder(fullPath);
            }
            else
            {
                UnityEngine.Debug.LogWarning($"Folder not found at path: {fullPath}");
            }
        }

        /// <summary>
        ///     Chose if you want NO logs shown.
        /// </summary>
        [MenuItem(_menuPath + nameof(None))]
        private static void None()
        {
            Log.CurrentLogLevel = LogLevel.None;
        }

        /// <summary>
        ///     Chose if you only want Error logs shown.
        /// </summary>
        [MenuItem(_menuPath + nameof(Error))]
        private static void Error()
        {
            Log.CurrentLogLevel = LogLevel.Error;
        }

        /// <summary>
        ///     Choose if you want both Warning and Error logs shown.
        /// </summary>
        [MenuItem(_menuPath + nameof(Warning))]
        private static void Warning()
        {
            Log.CurrentLogLevel = LogLevel.Warning;
        }

        /// <summary>
        ///     Choose if you want Debug, Warning, and Error logs shown.
        ///     Default.
        /// </summary>
        [MenuItem(_menuPath + nameof(Debug))]
        private static void Debug()
        {
            Log.CurrentLogLevel = LogLevel.Debug;
        }

        /// <summary>
        ///     Choose if you want Info, Debug, Warning, and Error logs shown.
        /// </summary>
        [MenuItem(_menuPath + nameof(Info))]
        private static void Info()
        {
            Log.CurrentLogLevel = LogLevel.Info;
        }

        /// <summary>
        ///     Choose if you want Success, Info, Debug, Warning, and Error logs shown.
        /// </summary>
        [MenuItem(_menuPath + nameof(Success))]
        private static void Success()
        {
            Log.CurrentLogLevel = LogLevel.Success;
        }

        /// <summary>
        ///     Choose if you want to see ALL logs. This is Verbose, Success, Info, Debug, Warning, and Error logs.
        /// </summary>
        [MenuItem(_menuPath + nameof(Verbose))]
        private static void Verbose()
        {
            Log.CurrentLogLevel = LogLevel.Verbose;
        }
    }
}
