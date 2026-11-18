using NUnit.Framework;
using SOSXR.EnhancedLogger.EditorScripts;

[TestFixture]
public class LogCallerParserTests
{
    private const string FullStackTrace =
        "VERBOSE | CharacterHandler (SetCalmer : 525) : We're always cool\n" +
        "\n" +
        " #0 PlatformStacktrace::GetStacktrace(int, StacktraceTypes::LRUCache<void*, StacktraceTypes::LRUValue>*)\n" +
        " #1 DebugStringToFileExtended(DebugStringToFileData const&)\n" +
        " #5  (Mono JIT Code) UnityEngine.DebugLogHandler:Internal_Log (UnityEngine.LogType,UnityEngine.LogOption,string,UnityEngine.Object)\n" +
        " #6  (Mono JIT Code) [DebugLogHandler.cs:10] UnityEngine.DebugLogHandler:LogFormat (UnityEngine.LogType,UnityEngine.Object,string,object[])\n" +
        " #7  (Mono JIT Code) [Logger.cs:67] UnityEngine.Logger:Log (UnityEngine.LogType,object,UnityEngine.Object)\n" +
        " #8  (Mono JIT Code) [Debug.bindings.cs:136] UnityEngine.Debug:Log (object,UnityEngine.Object)\n" +
        " #9  (Mono JIT Code) [Log.cs:315] SOSXR.EnhancedLogger.Log:Console (SOSXR.EnhancedLogger.LogLevel,string,UnityEngine.Object,string,string,int)\n" +
        " #10  (Mono JIT Code) [Log.cs:652] SOSXR.EnhancedLogger.Log:Verbose (UnityEngine.Object,string,int,string,string)\n" +
        " #11  (Mono JIT Code) [CharacterHandler.cs:527] SOSXR.CharacterHandler:SetCalmer ()\n" +
        " #12  (Mono JIT Code) [CharacterHandler.cs:274] SOSXR.CharacterHandler:InitAll ()\n" +
        " #15 mono_jit_runtime_invoke\n";

    private const string ScriptOnlyStackTrace =
        "VERBOSE | CharacterHandler (SetCalmer : 525) : We're always cool\n" +
        "UnityEngine.Debug:Log (object,UnityEngine.Object)\n" +
        "SOSXR.EnhancedLogger.Log:Console (SOSXR.EnhancedLogger.LogLevel,string,UnityEngine.Object,string,string,int) (at Assets/_SOSXR/sosxr-unity-enhancedlogger/Runtime/Log.cs:315)\n" +
        "SOSXR.EnhancedLogger.Log:Verbose (UnityEngine.Object,string,int,string,string) (at Assets/_SOSXR/sosxr-unity-enhancedlogger/Runtime/Log.cs:652)\n" +
        "SOSXR.CharacterHandler:SetCalmer () (at Assets/Scripts/CharacterHandler.cs:527)\n" +
        "SOSXR.CharacterHandler:Awake () (at Assets/Scripts/CharacterHandler.cs:211)\n";

    [Test]
    public void FullStackTrace_FindsFirstFrameAfterLogger()
    {
        Assert.IsTrue(LogCallerParser.TryFindCaller(FullStackTrace, out var caller));
        Assert.AreEqual("SOSXR.CharacterHandler", caller.TypeName);
        Assert.AreEqual("CharacterHandler.cs", caller.FilePath);
        Assert.AreEqual(527, caller.Line);
    }

    [Test]
    public void ScriptOnlyStackTrace_FindsFirstFrameAfterLogger()
    {
        Assert.IsTrue(LogCallerParser.TryFindCaller(ScriptOnlyStackTrace, out var caller));
        Assert.AreEqual("SOSXR.CharacterHandler", caller.TypeName);
        Assert.AreEqual("Assets/Scripts/CharacterHandler.cs", caller.FilePath);
        Assert.AreEqual(527, caller.Line);
    }

    [Test]
    public void WindowsLineEndings_AreHandled()
    {
        Assert.IsTrue(LogCallerParser.TryFindCaller(ScriptOnlyStackTrace.Replace("\n", "\r\n"), out var caller));
        Assert.AreEqual("Assets/Scripts/CharacterHandler.cs", caller.FilePath);
        Assert.AreEqual(527, caller.Line);
    }

    [Test]
    public void PathWithSpaces_IsKept()
    {
        const string text =
            "SOSXR.EnhancedLogger.Log:Info (UnityEngine.Object,string,int,string,string) (at Assets/Log.cs:500)\n" +
            "My.Game.Player:Start () (at Assets/My Scripts/Player.cs:12)\n";

        Assert.IsTrue(LogCallerParser.TryFindCaller(text, out var caller));
        Assert.AreEqual("Assets/My Scripts/Player.cs", caller.FilePath);
        Assert.AreEqual(12, caller.Line);
    }

    [Test]
    public void NestedTypeAndLambda_TypeNameUsesPlus()
    {
        const string text =
            "SOSXR.EnhancedLogger.Log:Info (UnityEngine.Object,string,int,string,string) (at Assets/Log.cs:500)\n" +
            "My.Outer/Inner:<Start>b__0_0 () (at Assets/Outer.cs:40)\n";

        Assert.IsTrue(LogCallerParser.TryFindCaller(text, out var caller));
        Assert.AreEqual("My.Outer+Inner", caller.TypeName);
        Assert.AreEqual(40, caller.Line);
    }

    [Test]
    public void NoLoggerFrame_ReturnsFalse()
    {
        const string text =
            "Hello\n" +
            "UnityEngine.Debug:Log (object) \n" +
            "My.Game.Player:Start () (at Assets/Player.cs:12)\n";

        Assert.IsFalse(LogCallerParser.TryFindCaller(text, out _));
    }

    [Test]
    public void LoggerFrameIsLast_ReturnsFalse()
    {
        const string text = "SOSXR.EnhancedLogger.Log:Info (UnityEngine.Object,string,int,string,string) (at Assets/Log.cs:500)\n";

        Assert.IsFalse(LogCallerParser.TryFindCaller(text, out _));
    }

    [Test]
    public void EmptyOrNullText_ReturnsFalse()
    {
        Assert.IsFalse(LogCallerParser.TryFindCaller((string) null, out _));
        Assert.IsFalse(LogCallerParser.TryFindCaller(string.Empty, out _));
    }

    [Test]
    public void IsDeeperLoggerLine_FirstLoggerLineIsNotDeeper()
    {
        var frames = LogCallerParser.ParseFrames(FullStackTrace);

        Assert.IsFalse(LogCallerParser.IsDeeperLoggerLine(frames, 315));
    }

    [Test]
    public void IsDeeperLoggerLine_SecondLoggerLineIsDeeper()
    {
        var frames = LogCallerParser.ParseFrames(FullStackTrace);

        Assert.IsTrue(LogCallerParser.IsDeeperLoggerLine(frames, 652));
    }

    [Test]
    public void ParseFrames_SkipsLinesWithoutFile()
    {
        var frames = LogCallerParser.ParseFrames(FullStackTrace);

        // DebugLogHandler, Logger, Debug.bindings, Log x2, CharacterHandler x2
        Assert.AreEqual(7, frames.Count);
    }
}
