using System.IO;
using NUnit.Framework;
using SOSXR.EnhancedLogger;
using SOSXR.EnhancedLogger.EditorScripts;
using UnityEditor;
using UnityEngine;

/// <summary>
///     The settings asset must live in the project (<c>Assets/_SOSXR/Settings/</c>), never in the package.
///     A copy inside an immutable package gets modified when the log level changes.
/// </summary>
[TestFixture]
public class EnhancedLoggerSettingsLocationTests
{
    private EnhancedLoggerSettings _settings;
    private EnhancedLoggerSettings _otherSettings;
    private Texture2D _otherAsset;

    [SetUp]
    public void SetUp()
    {
        _settings = ScriptableObject.CreateInstance<EnhancedLoggerSettings>();
        _otherSettings = ScriptableObject.CreateInstance<EnhancedLoggerSettings>();
        _otherAsset = new Texture2D(1, 1);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_settings);
        Object.DestroyImmediate(_otherSettings);
        Object.DestroyImmediate(_otherAsset);
    }

    [Test]
    public void SettingsAssetPath_IsInSosxrSettingsFolder()
    {
        Assert.AreEqual("Assets/_SOSXR/Settings/EnhancedLoggerSettings.asset", Log.SettingsAssetPath);
        StringAssert.DoesNotContain("/Resources/", Log.SettingsAssetPath);
    }

    [Test]
    public void Package_ContainsNoSettingsAsset()
    {
        var packageRoot = PackageRoot();
        var guids = AssetDatabase.FindAssets("t:" + nameof(EnhancedLoggerSettings), new[] {packageRoot});

        Assert.IsEmpty(guids, $"The package at {packageRoot} must not ship an EnhancedLoggerSettings asset.");
    }

    [Test]
    public void FindSettingsInProject_NeverReturnsPackageAsset()
    {
        var settings = Log.FindSettingsInProject();

        if (settings == null)
        {
            Assert.Pass("No settings asset in this project.");
        }

        StringAssert.StartsWith("Assets/", AssetDatabase.GetAssetPath(settings));
    }

    [Test]
    public void PreferencesProvider_IsUnderSosxrInUserPreferences()
    {
        var provider = EnhancedLoggerPreferences.CreateProvider();

        Assert.AreEqual("Preferences/SOSXR/EnhancedLogger", provider.settingsPath);
        Assert.AreEqual(SettingsScope.User, provider.scope);
    }

    [Test]
    public void WithSettings_AddsSettingsWhenMissing()
    {
        var result = EnhancedLoggerSettingsBuild.WithSettings(new Object[] {_otherAsset}, _settings);

        CollectionAssert.AreEqual(new Object[] {_otherAsset, _settings}, result);
    }

    [Test]
    public void WithSettings_KeepsListWhenSettingsPresent()
    {
        var result = EnhancedLoggerSettingsBuild.WithSettings(new Object[] {_settings, _otherAsset}, _settings);

        CollectionAssert.AreEqual(new Object[] {_settings, _otherAsset}, result);
    }

    [Test]
    public void WithSettings_ReplacesOtherSettings()
    {
        var result = EnhancedLoggerSettingsBuild.WithSettings(new Object[] {_otherSettings, _otherAsset}, _settings);

        CollectionAssert.AreEqual(new Object[] {_otherAsset, _settings}, result);
    }

    [Test]
    public void WithSettings_RemovesDuplicates()
    {
        var result = EnhancedLoggerSettingsBuild.WithSettings(new Object[] {_settings, _settings}, _settings);

        CollectionAssert.AreEqual(new Object[] {_settings}, result);
    }

    [Test]
    public void WithSettings_KeepsEmptyEntries()
    {
        var result = EnhancedLoggerSettingsBuild.WithSettings(new Object[] {null, _otherAsset}, _settings);

        CollectionAssert.AreEqual(new Object[] {null, _otherAsset, _settings}, result);
    }

    [Test]
    public void WithSettings_HandlesNullList()
    {
        var result = EnhancedLoggerSettingsBuild.WithSettings(null, _settings);

        CollectionAssert.AreEqual(new Object[] {_settings}, result);
    }

    /// <summary>Folder of the package, found from the location of Log.cs (Runtime/Log.cs).</summary>
    private static string PackageRoot()
    {
        foreach (var guid in AssetDatabase.FindAssets("Log t:MonoScript"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);

            if (script != null && script.GetClass() == typeof(Log))
            {
                return Path.GetDirectoryName(Path.GetDirectoryName(path))?.Replace('\\', '/');
            }
        }

        Assert.Fail("Log.cs not found.");

        return null;
    }
}
