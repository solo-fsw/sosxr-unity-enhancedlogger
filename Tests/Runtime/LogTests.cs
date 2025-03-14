using NUnit.Framework;
using SOSXR.EnhancedLogger;
using UnityEngine;
using UnityEngine.TestTools;


[TestFixture]
public class ShowOwnLogs
{
    [Test]
    public void Error_ShowsErrorLogs()
    {
        Log.CurrentLogLevel = LogLevel.Error;

        var mockName = string.Concat("MockObject_", Random.Range(0, 100));
        var mockObject = new GameObject(mockName);

        var message = "This is an error message.";
        mockObject.Error(message);

        var prefix = Log.GetPrefix(LogLevel.Error);
        var color = Log.GetColor(LogLevel.Error);

        var objectName = $"<color={color}>[{mockObject.name}]{prefix}</color> : ";

        LogAssert.Expect(LogType.Log, $"{objectName}{message}\n");
    }


    [Test]
    public void Warning_ShowsWarningLogs()
    {
        Log.CurrentLogLevel = LogLevel.Warning;

        var mockName = string.Concat("MockObject_", Random.Range(0, 100));
        var mockObject = new GameObject(mockName);

        var message = "This is a warning message.";
        mockObject.Warning(message);

        var prefix = Log.GetPrefix(LogLevel.Warning);
        var color = Log.GetColor(LogLevel.Warning);

        var objectName = $"<color={color}>[{mockObject.name}]{prefix}</color> : ";

        LogAssert.Expect(LogType.Log, $"{objectName}{message}\n");
    }


    [Test]
    public void Debug_ShowsDebugLogs()
    {
        Log.CurrentLogLevel = LogLevel.Debug;

        var mockName = string.Concat("MockObject_", Random.Range(0, 100));
        var mockObject = new GameObject(mockName);

        var message = "This is a debug message.";
        mockObject.Debug(message);

        var prefix = Log.GetPrefix(LogLevel.Debug);
        var color = Log.GetColor(LogLevel.Debug);

        var objectName = $"<color={color}>[{mockObject.name}]{prefix}</color> : ";

        LogAssert.Expect(LogType.Log, $"{objectName}{message}\n");
    }


    [Test]
    public void Success_ShowsSuccessLogs()
    {
        Log.CurrentLogLevel = LogLevel.Success;

        var mockName = string.Concat("MockObject_", Random.Range(0, 100));
        var mockObject = new GameObject(mockName);

        var message = "This is a success message.";
        mockObject.Success(message);

        var prefix = Log.GetPrefix(LogLevel.Success);
        var color = Log.GetColor(LogLevel.Success);

        var objectName = $"<color={color}>[{mockObject.name}]{prefix}</color> : ";

        LogAssert.Expect(LogType.Log, $"{objectName}{message}\n");
    }


    [Test]
    public void Info_ShowsInfoLogs()
    {
        Log.CurrentLogLevel = LogLevel.Verbose;

        var mockName = string.Concat("MockObject_", Random.Range(0, 100));
        var mockObject = new GameObject(mockName);

        var message = "This is an info message.";
        mockObject.Verbose(message);

        var prefix = Log.GetPrefix(LogLevel.Verbose);
        var color = Log.GetColor(LogLevel.Verbose);

        var objectName = $"<color={color}>[{mockObject.name}]{prefix}</color> : ";

        LogAssert.Expect(LogType.Log, $"{objectName}{message}\n");
    }
}


[TestFixture]
public class ShowHigherLogs
{
    [Test]
    public void Warning_ShowsErrorLogs()
    {
        Log.CurrentLogLevel = LogLevel.Warning;

        var mockName = string.Concat("MockObject_", Random.Range(0, 100));
        var mockObject = new GameObject(mockName);

        var message = "This is an error message, which should be logged, because CurrentLogLevel is set to Warning.";
        mockObject.Error(message);

        var prefix = Log.GetPrefix(LogLevel.Error);
        var color = Log.GetColor(LogLevel.Error);

        var objectName = $"<color={color}>[{mockObject.name}]{prefix}</color> : ";

        LogAssert.Expect(LogType.Log, $"{objectName}{message}\n");
    }


    [Test]
    public void Debug_ShowsWarningLogs()
    {
        Log.CurrentLogLevel = LogLevel.Debug;

        var mockName = string.Concat("MockObject_", Random.Range(0, 100));
        var mockObject = new GameObject(mockName);

        var message = "This is a warning message, which should be logged, because CurrentLogLevel is set to Debug.";
        mockObject.Warning(message);

        var prefix = Log.GetPrefix(LogLevel.Warning);
        var color = Log.GetColor(LogLevel.Warning);

        var objectName = $"<color={color}>[{mockObject.name}]{prefix}</color> : ";

        LogAssert.Expect(LogType.Log, $"{objectName}{message}\n");
    }


    [Test]
    public void Success_ShowsErrorAndSuccessLogs()
    {
        Log.CurrentLogLevel = LogLevel.Success;

        var mockName = string.Concat("MockObject_", Random.Range(0, 100));
        var mockObject = new GameObject(mockName);

        var errorMessage = "This is an error message, which should be logged, because CurrentLogLevel is set to Success";
        mockObject.Error(errorMessage);

        var errorPrefix = Log.GetPrefix(LogLevel.Error);
        var errorColor = Log.GetColor(LogLevel.Error);
        var errorObjectName = $"<color={errorColor}>[{mockObject.name}]{errorPrefix}</color> : ";

        LogAssert.Expect(LogType.Log, $"{errorObjectName}{errorMessage}\n");

        var successMessage = "This is a success message, which should be logged, because CurrentLogLevel is set to Success.";
        mockObject.Success(successMessage);

        var successPrefix = Log.GetPrefix(LogLevel.Success);
        var successColor = Log.GetColor(LogLevel.Success);
        var successObjectName = $"<color={successColor}>[{mockObject.name}]{successPrefix}</color> : ";

        LogAssert.Expect(LogType.Log, $"{successObjectName}{successMessage}\n");
    }


    [Test]
    public void Info_ShowsAllLogs()
    {
        Log.CurrentLogLevel = LogLevel.Verbose;

        var mockName = string.Concat("MockObject_", Random.Range(0, 100));
        var mockObject = new GameObject(mockName);

        var warningMessage = "This is a warning message, which should be logged, because CurrentLogLevel is set to Info.";
        mockObject.Warning(warningMessage);

        var warningPrefix = Log.GetPrefix(LogLevel.Warning);
        var warningColor = Log.GetColor(LogLevel.Warning);
        var warningObjectName = $"<color={warningColor}>[{mockObject.name}]{warningPrefix}</color> : ";

        LogAssert.Expect(LogType.Log, $"{warningObjectName}{warningMessage}\n");

        var debugMessage = "This is a debug message, which should be logged, because CurrentLogLevel is set to Info.";
        mockObject.Debug(debugMessage);

        var debugPrefix = Log.GetPrefix(LogLevel.Debug);
        var debugColor = Log.GetColor(LogLevel.Debug);
        var debugObjectName = $"<color={debugColor}>[{mockObject.name}]{debugPrefix}</color> : ";

        LogAssert.Expect(LogType.Log, $"{debugObjectName}{debugMessage}\n");

        var successMessage = "This is a success message, which should be logged, because CurrentLogLevel is set to Info.";
        mockObject.Success(successMessage);

        var successPrefix = Log.GetPrefix(LogLevel.Success);
        var successColor = Log.GetColor(LogLevel.Success);
        var successObjectName = $"<color={successColor}>[{mockObject.name}]{successPrefix}</color> : ";

        LogAssert.Expect(LogType.Log, $"{successObjectName}{successMessage}\n");

        var infoMessage = "This is an info message, which should be logged, because CurrentLogLevel is set to Info.";
        mockObject.Verbose(infoMessage);

        var infoPrefix = Log.GetPrefix(LogLevel.Verbose);
        var infoColor = Log.GetColor(LogLevel.Verbose);
        var infoObjectName = $"<color={infoColor}>[{mockObject.name}]{infoPrefix}</color> : ";

        LogAssert.Expect(LogType.Log, $"{infoObjectName}{infoMessage}\n");
    }
}


[TestFixture]
public class SuppressLowerLogs
{
    [Test]
    public void Error_SuppressesWarningLogs()
    {
        Log.CurrentLogLevel = LogLevel.Error;

        var mockName = string.Concat("MockObject_", Random.Range(0, 100));
        var mockObject = new GameObject(mockName);

        var message = "This is a Warning message.";
        mockObject.Warning(message);

        LogAssert.NoUnexpectedReceived();
    }


    [Test]
    public void Debug_SuppressesSuccessAndInfoLogs()
    {
        Log.CurrentLogLevel = LogLevel.Debug;

        var mockName = string.Concat("MockObject_", Random.Range(0, 100));
        var mockObject = new GameObject(mockName);

        mockObject.Success("This is a success message.");
        mockObject.Verbose("This is an info message.");

        LogAssert.NoUnexpectedReceived();
    }


    [Test]
    public void None_SuppressesAllLogs()
    {
        Log.CurrentLogLevel = LogLevel.None;

        var mockName = string.Concat("MockObject_", Random.Range(0, 100));
        var mockObject = new GameObject(mockName);

        var errorMessage = "This is an error message, which should be suppressed. CurrentLogLevel is set to None.";
        mockObject.Error(errorMessage);

        var warningMessage = "This is a warning message, which should be suppressed. CurrentLogLevel is set to None.";
        mockObject.Warning(warningMessage);

        var debugMessage = "This is a debug message, which should be suppressed, because CurrentLogLevel is set to None.";
        mockObject.Debug(debugMessage);

        var successMessage = "This is a success message, which should be suppressed, because CurrentLogLevel is set to None.";
        mockObject.Success(successMessage);

        var infoMessage = "This is an info message, which should be suppressed, because CurrentLogLevel is set to None.";
        mockObject.Verbose(infoMessage);

        LogAssert.NoUnexpectedReceived();
    }
}