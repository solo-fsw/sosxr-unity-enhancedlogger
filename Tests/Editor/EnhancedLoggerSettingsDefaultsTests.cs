using NUnit.Framework;
using SOSXR.EnhancedLogger;
using UnityEngine;


namespace SOSXR.EnhancedLogger.Tests
{
    /// <summary>
    ///     The default colors are written as RGB values so the package compiles on Unity 6000.0 and 6000.1.
    ///     On 6000.2 and newer they must still equal the Color presets they replace.
    /// </summary>
    public class EnhancedLoggerSettingsDefaultsTests
    {
        private EnhancedLoggerSettings _settings;


        [SetUp]
        public void SetUp()
        {
            _settings = ScriptableObject.CreateInstance<EnhancedLoggerSettings>();
        }


        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_settings);
        }


#if UNITY_6000_2_OR_NEWER
        [Test]
        public void DefaultColors_MatchUnityPresets()
        {
            AssertColor(Color.softRed, _settings.ErrorColor);
            AssertColor(Color.orange, _settings.WarningColor);
            AssertColor(Color.cornflowerBlue, _settings.DebugColor);
            AssertColor(Color.khaki, _settings.InfoColor);
            AssertColor(Color.softGreen, _settings.SuccessColor);
            AssertColor(Color.hotPink, _settings.VerboseColor);
        }
#endif


        [Test]
        public void DefaultColors_AreOpaque()
        {
            Assert.AreEqual(1f, _settings.ErrorColor.a);
            Assert.AreEqual(1f, _settings.WarningColor.a);
            Assert.AreEqual(1f, _settings.DebugColor.a);
            Assert.AreEqual(1f, _settings.InfoColor.a);
            Assert.AreEqual(1f, _settings.SuccessColor.a);
            Assert.AreEqual(1f, _settings.VerboseColor.a);
        }


        private static void AssertColor(Color expected, Color actual)
        {
            Assert.AreEqual(expected.r, actual.r, 1e-4f, "r");
            Assert.AreEqual(expected.g, actual.g, 1e-4f, "g");
            Assert.AreEqual(expected.b, actual.b, 1e-4f, "b");
            Assert.AreEqual(expected.a, actual.a, 1e-4f, "a");
        }
    }
}
