# EnhancedLogger

**Package:** com.sosxr.enhancedlogger v1.0.0  
**Unity:** 6000.0+  
**Generated:** 2025-03-30

## OVERVIEW

Unity package providing customizable logging with 6 log levels, colored output, file logging, and Scene View controls. Zero runtime cost in production builds via compiler stripping.

## STRUCTURE

```
./
├── Runtime/           # Core logging API (4 files)
│   ├── Log.cs         # Main static class with extension methods
│   ├── LogLevel.cs    # Enum: None, Error, Warning, Debug, Info, Success, Verbose
│   ├── EnhancedLoggerSettings.cs  # ScriptableObject configuration
│   └── WriteToFile.cs # Markdown file logging with deduplication
├── Editor/            # Editor-only tools
│   └── LogButtons.cs  # Scene View UI + menu items
├── Tests/Runtime/     # NUnit test assembly
│   └── LogTests.cs    # Log level filtering tests
├── Samples~/Samples/  # Demo scripts (4 files)
├── Resources/         # Auto-generated settings asset
└── Media/             # Package icons/assets
```

## WHERE TO LOOK

| Task | Location | Notes |
|------|----------|-------|
| Log API | `Runtime/Log.cs` | Static extension methods on Object |
| Settings | `Runtime/EnhancedLoggerSettings.cs` | Colors, prefixes, log level, file toggle |
| Log levels | `Runtime/LogLevel.cs` | Enum definition with XML docs |
| File logging | `Runtime/WriteToFile.cs` | Deduplication cache, markdown output |
| Scene UI | `Editor/LogButtons.cs` | Scene View buttons + menu items |
| Tests | `Tests/Runtime/LogTests.cs` | NUnit with LogAssert |
| Demo usage | `Samples~/Samples/*.cs` | 4 demo scripts showing patterns |

## CODE MAP

| Symbol | Type | File | Lines | Role |
|--------|------|------|-------|------|
| `Log` | static class | Log.cs | 515 | Main API with extension methods |
| `LogLevel` | enum | LogLevel.cs | 31 | Severity levels None→Verbose |
| `EnhancedLoggerSettings` | ScriptableObject | EnhancedLoggerSettings.cs | 58 | Configuration asset |
| `WriteToFile` | static class | WriteToFile.cs | 120 | File logging with dedupe |
| `LogButtons` | MonoBehaviour | LogButtons.cs | 206 | Editor Scene View UI |

## CONVENTIONS

### Log Level Philosophy (Opinionated)

- **Debug** = Temporary dev messages. Clean up before merging.
- **Info** = Expected behavior, useful to know.
- **Success** = Confirmations, state changes.
- **Verbose** = Detailed traces. Use sparingly.
- **Warning/Error** = Issues requiring attention.

Each level shows itself + all higher-priority levels (lower enum value).

### API Patterns

```csharp
// Instance (extension) methods - preferred
this.Debug("message");
gameObject.Error("error");

// Static methods - no GameObject context
Log.Static("message", LogLevel.Info);

// Multiple messages (auto-joined with " : ")
this.Info("Loading", "player data", "complete");
```

### Compiler Directives

```csharp
// Logs stripped from release builds
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
    return false;
#endif
```

### Assembly Structure

- `SOSXR.EnhancedLogger` (Runtime)
- `SOSXR.EnhancedLogger.Editor` (Editor-only)
- `SOSXR.EnhancedLogger.Tests` (Test assembly)
- `SOSXR.EnhancedLogger.Samples` (Demo code)

## ANTI-PATTERNS

### Logging Practices to Avoid

- **Logging sensitive data**: Never log passwords, API keys, PII, or secrets. Log files are written to persistent storage.
- **Excessive Verbose logging**: Verbose level can flood logs and impact performance. Use sparingly.
- **Debug logs in production**: Debug level is for temporary development only. Remove or upgrade before merging.
- **Logging in tight loops**: Avoid logging inside Update() or frequent loops without rate limiting.

### API Misuse

- **Static methods lose navigation**: No click-to-GameObject or click-to-source. Prefer instance methods.
- **Mixing log levels inconsistently**: Use appropriate levels. Don't use Error for expected behavior.
- **Ignoring null messages**: While handled safely, passing null indicates a code smell. Validate inputs.

### Performance Anti-Patterns

- **1000+ character messages**: Extremely long messages impact memory. Keep logs concise.
- **Rapid fire logging**: Thousands of logs per second can overwhelm the cache. Use deduplication wisely.
- **Creating logs that are never shown**: Check CurrentLogLevel before expensive log formatting.

### File Logging

- **File logging is not real-time**: Writes on application quit only. Don't rely on it for crash debugging.
- **Settings auto-creation**: If `Resources/EnhancedLoggerSettings.asset` missing, created at `Assets/_SOSXR/Resources/`.
- **Demo files require copy**: Cannot use scenes directly from package (Unity limitation).
- **Assuming file logging is enabled**: Always check WriteToFile setting. File logging may be disabled.

## UNIQUE STYLES

### Extension Method Design

All logging methods are extension methods on `UnityEngine.Object`:

```csharp
public static void Debug(this Object caller, string message, 
    [CallerLineNumber] int callerLineNumber = 0,
    [CallerMemberName] string callerName = "",
    [CallerFilePath] string callerFilePath = "")
```

Uses `Caller*Attributes` for automatic source location capture.

### Color Injection

Unity console color tags via hex conversion:

```csharp
private static string Color(this string message, Color color)
{
    Color32 c = color;
    var hex = $"#{c.r:X2}{c.g:X2}{c.b:X2}";
    return $"<color={hex}>{message}</color>";
}
```

### Deduplication Strategy

File logging caches messages, counts occurrences:

```
Message text - from `HH:mm:ss` to `HH:mm:ss` shown **Nx**
```

## WORKFLOWS

### Change Log Level

1. Scene View buttons (bottom-right)
2. Menu: `SOSXR > EnhancedLogger > [Level]`
3. Code: `Log.CurrentLogLevel = LogLevel.Debug`

### Run Tests

Open Unity Test Runner → PlayMode → Run all.

### File Logging Location

`Application.persistentDataPath/EnhancedLogger/{ProductName}_{timestamp}.md`

Menu shortcut: `SOSXR > Folders > EnhancedLogger`

## DEPENDENCIES

None. Optional integration with [In-Game Debug Console](https://github.com/solo-fsw/sosxr-unity-ingamedebugconsole).

## NOTES

- Zero-cost in production: All logs stripped by compiler
- Settings persist between Play/Edit mode
- Namespace: `SOSXR.EnhancedLogger`
- Editor namespace: `SOSXR.EnhancedLogger.EditorScripts`
- Samples directory: The `Samples~` folder name is temporary and will be renamed to `Samples` in a future update
