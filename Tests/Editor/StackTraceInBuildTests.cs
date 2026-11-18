using NUnit.Framework;
using UnityEngine;

/// <summary>
///     The Editor must keep its own stack trace setting: Full stack traces break Console double-click to the calling code.
/// </summary>
[TestFixture]
public class StackTraceInBuildTests
{
    [TestCase(true)]
    [TestCase(false)]
    public void Editor_LeavesStackTraceUnchanged(bool isDebugBuild)
    {
        Assert.IsNull(StackTraceInBuild.StackTraceTypeFor(true, isDebugBuild));
    }

    [Test]
    public void DevelopmentBuild_UsesFull()
    {
        Assert.AreEqual(StackTraceLogType.Full, StackTraceInBuild.StackTraceTypeFor(false, true));
    }

    [Test]
    public void ReleaseBuild_UsesNone()
    {
        Assert.AreEqual(StackTraceLogType.None, StackTraceInBuild.StackTraceTypeFor(false, false));
    }
}
