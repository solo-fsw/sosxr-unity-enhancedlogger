using NUnit.Framework;
using SOSXR.EnhancedLogger;
using SOSXR.EnhancedLogger.EditorScripts;
using UnityEditor;
using UnityEngine;

/// <summary>
///     Guards the reflection that <see cref="OpenLogCaller" /> uses to read the selected Console entry.
///     If Unity renames <c>ConsoleWindow.ms_ConsoleWindow</c> or <c>ConsoleWindow.m_ActiveText</c>, these tests fail,
///     so the broken double-click redirect is noticed after a Unity upgrade.
/// </summary>
[TestFixture]
public class OpenLogCallerTests
{
    [Test]
    public void ConsoleWindowType_Exists()
    {
        Assert.IsNotNull(OpenLogCaller.ConsoleWindowType, "UnityEditor.ConsoleWindow not found.");
        Assert.IsTrue(typeof(EditorWindow).IsAssignableFrom(OpenLogCaller.ConsoleWindowType));
    }

    [Test]
    public void ConsoleInstanceField_Exists()
    {
        Assert.IsNotNull(OpenLogCaller.ConsoleInstanceField, "ConsoleWindow.ms_ConsoleWindow not found.");
        Assert.AreEqual(OpenLogCaller.ConsoleWindowType, OpenLogCaller.ConsoleInstanceField.FieldType);
    }

    [Test]
    public void ActiveTextField_ExistsAndIsString()
    {
        Assert.IsNotNull(OpenLogCaller.ActiveTextField, "ConsoleWindow.m_ActiveText not found.");
        Assert.AreEqual(typeof(string), OpenLogCaller.ActiveTextField.FieldType);
    }

    [Test]
    public void CanReadConsoleText_IsTrue()
    {
        Assert.IsTrue(OpenLogCaller.CanReadConsoleText);
    }

    [Test]
    public void IsLoggerScript_TrueForLogScript()
    {
        var guids = AssetDatabase.FindAssets("Log t:MonoScript");
        MonoScript logScript = null;

        foreach (var guid in guids)
        {
            var script = AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(guid));

            if (script != null && script.GetClass() == typeof(Log))
            {
                logScript = script;
                break;
            }
        }

        Assert.IsNotNull(logScript, "Log.cs script asset not found.");
        Assert.IsTrue(OpenLogCaller.IsLoggerScript(logScript));
    }

    [Test]
    public void IsLoggerScript_FalseForOtherObjects()
    {
        var gameObject = new GameObject("NotTheLogger");

        try
        {
            Assert.IsFalse(OpenLogCaller.IsLoggerScript(gameObject));
            Assert.IsFalse(OpenLogCaller.IsLoggerScript(null));
        }
        finally
        {
            Object.DestroyImmediate(gameObject);
        }
    }
}
