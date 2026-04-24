# EnhancedLogger

- By: Maarten R. Struijk Wilbrink
- For: Leiden University SOSXR
- Fully open source: Feel free to add to, or modify, anything you see fit.

A utility class for logging messages with customisable log levels and colours in Unity.

## Table of Contents

- [Introduction](#introduction)
- [Features](#features)
- [Installation](#installation)
- [Usage](#usage)
  - [Log Levels](#log-levels)
  - [Log Messages](#log-messages)
    - [Instance Methods](#instance-methods)
    - [Static Methods](#static-methods)
    - [Multiple Message Overloads](#multiple-message-overloads)
  - [Scene View Buttons](#scene-view-buttons)
  - [File Logging](#file-logging)
  - [Colors](#colors)
  - [Settings](#settings)
- [Downsides](#downsides)
- [Inline Documentation](#inline-documentation)
- [Tests](#tests)
- [Contributing](#contributing)
- [License](#license)

## Introduction

This Unity package provides a flexible and customizable logging utility named `Log`. It allows you to log messages with different log levels (Error, Warning, Debug, Info, Success, and Verbose) and customize the appearance of log messages with colors.

The utility is designed to be easy to integrate into your Unity projects.

## Features

- Log messages with various log levels (None, Error, Warning, Debug, Info, Success, Verbose)
- Suppress logs you don't want to see via log level filtering
- Write logs to markdown files with automatic deduplication and occurrence tracking
- Scene view buttons for quick log level changes (bottom-right corner)
- Click to navigate to the source code and GameObject that called the log
- Customize log message appearance with colors per log level
- Support for both instance methods (extension methods) and static methods
- Support for 1-4 message overloads with automatic joining
- Runs logs only in Unity Editor and Development Build. Logs are completely stripped from Production builds via compiler directives, making it a zero-cost solution in production
- Automatic settings asset creation on first use

## Installation

### Unity Package Manager

1. Open your Unity project.
2. Open the Unity Package Manager from the `Window` menu.
3. Click the `+` button in the top-left corner of the Package Manager window.
4. Select `Add package from git URL…`.
5. Enter the following Git URL in the input field: `https://github.com/mrstruijk/EnhancedLogger.git` (making sure of the `.git` at the end).
6. Click the `Add` button.

### Manual Installation

If you prefer manual installation (also allowing you more customizability), clone or download [the repository](https://github.com/mrstruijk/EnhancedLogger) to your Unity project.

## Dependencies

Does not have any direct dependencies, but does work well with the [In-Game Debug Console](https://github.com/solo-fsw/sosxr-unity-ingamedebugconsole), originally created by [yasirkula](https://github.com/yasirkula/UnityIngameDebugConsole).

## Usage

## Log Levels

- **None** - Suppresses all logs
- **Error** - Catastrophic errors that break the game
- **Warning** - Severe issues that should not be ignored
- **Debug** - Temporary messages for development (should be cleaned up when done)
- **Info** - General information messages (default level)
- **Success** - Successful operations or state changes
- **Verbose** - Detailed information for debugging application flow

Each LogLevel shows the logs of the selected LogLevel and all logs with a higher LogLevel. For example, if you select `Warning`, you will see both `Warning` and `Error` logs. Selecting `Verbose` will show all logs. The default LogLevel is `Info` (also showing `Debug`, `Warning`, and `Error` logs).

### Opinionated Usage

`Debug` is used only for when you're working on a specific module. Put in as many logs as you want, to see how the thing you're building is progressing. Then, when things 'settle down' and it starts working as intended: either move the `Debug` logs to the appropriate higher or lower level, or delete them. By the end of each day / sprint / module, `Debug` should be empty again.

#### `Info` and `Success`

With `Verbose` there's too many other messages that are displaying, while a `Warning` is often also not what I was looking for. A lot of the messages were of things that were simply happening, without them needing to warn me of things. That's what the `Info` level is made for. Similarly, but only for the good stuff: `Success`.

## Log Messages

### Instance Methods

Instance methods are extension methods on Unity Objects (MonoBehaviour, GameObject, etc.). They provide the most information in logs:

- Logs include the GameObject name that called the log
- Clicking the log in the console navigates to the GameObject in the Hierarchy
- Clicking the log navigates to the source code line

Use instance methods when you have access to a MonoBehaviour or GameObject:

```csharp
this.Debug("Debug message");
otherGameObject.Warning("Warning message");
gameObject.Success("Success message");
```

### Static Methods

Static methods are useful when you don't have access to a MonoBehaviour instance or when the caller object may be destroyed:

- No GameObject reference in the log output
- Cannot click to navigate to GameObject in Hierarchy
- Can still click the location information (second line) to navigate to the source code
- Useful for logging from static classes or utility methods

Use static methods when needed:

```csharp
Log.Static("Info message", LogLevel.Info);
Log.Static("Debug message");
Log.Static("Error message", LogLevel.Error);
```

### Multiple Message Overloads

All logging methods support 1-4 messages, which are automatically combined with a " : " separator:

```csharp
// 1 message
this.Debug("Loading complete");

// 2 messages
this.Debug("Loading", "Player data");

// 3 messages
this.Debug("Loading", "Player data", "from file");

// 4 messages
this.Debug("Loading", "Player data", "from file", "success");
```

This is useful for organizing related information in a single log call. See the Samples folder for more examples.

## Scene View Buttons

The EnhancedLogger provides convenient buttons in the Scene view for quick log level changes:

- **Location:** Bottom-right corner of the Scene view
- **Buttons:** None, Error, Warning, Debug, Info, Success, Verbose
- **Current Level:** Highlighted in green

Alternatively, use the menu: `SOSXR > EnhancedLogger > [LogLevel]`

The selected log level persists between Play mode and Edit mode.

## File Logging

The EnhancedLogger can automatically write logs to markdown files in your persistent data path. This feature is controlled by the `WriteToFile` toggle in `EnhancedLoggerSettings`.

### How It Works

- Logs are cached in memory during runtime
- Identical messages are deduplicated (count tracked instead of duplicating)
- Each session creates a unique log file with a timestamp
- File is written when the application quits
- Location: `Application.persistentDataPath/EnhancedLogger/`

### Deduplication Strategy

Instead of writing duplicate messages, the logger tracks:

- **Message content** - The exact message text
- **Count** - Number of times this message appeared
- **First occurrence** - Timestamp of when the message was first logged
- **Last occurrence** - Timestamp of the most recent occurrence

This keeps log files clean and readable while preserving important information about message frequency.

### Output Format

Log files are written in Markdown format for easy reading and parsing. Each line shows:

```
Message text - from `HH:mm:ss` to `HH:mm:ss` shown **Nx**
```

### Lifecycle

File logging hooks into Unity's `Application.quitting` event via a delegate:

- Subscription happens automatically before the first scene loads (`[RuntimeInitializeOnLoadMethod]`)
- On quit, `WriteFinalFile` flushes the cache and unsubscribes the delegate to prevent duplicate writes in the Editor
- You can also call `WriteToFile.WriteFile()` manually for an early flush

## Colors

Each log level has a customizable color that appears in the console prefix.

### Default Colors

- **Error:** Red
- **Warning:** Orange
- **Debug:** Cornflower Blue
- **Info:** Khaki
- **Success:** Green
- **Verbose:** Hot Pink

### How Colors Work

Colors are applied to the log prefix using Unity's `<color>` tag syntax. The color is converted to hexadecimal RGB format (e.g., #FF0000 for red) and wrapped in tags that the Unity console interprets.

### Customizing Colors

Edit the `EnhancedLoggerSettings` asset to change colors for each log level. The changes take effect immediately in the Editor.

## Settings

Settings can be found in a ScriptableObject in the Resources folder called `EnhancedLoggerSettings`.

### LogLevel

In the Scene view, select the LogLevel you want to use. This will determine which log messages are displayed in the Console.

Alternatively, set the LogLevel via the Menu bar at `SOSXR > EnhancedLogger`, or directly in the `EnhancedLoggerSettings` object.

### Automatic Settings Creation

If the `EnhancedLoggerSettings` asset doesn't exist, it will be created automatically in `Assets/_SOSXR/Resources/` on first use. A warning will be logged to inform you of this automatic creation.

### Prefix

Prefixes, such as `[DEBUG]`, can be set in the `EnhancedLoggerSettings` object. Leave blank if you prefer no prefix.

### File Logging Toggle

Enable or disable file logging via the `WriteToFile` toggle in `EnhancedLoggerSettings`.

## Downsides

- When using Static methods, you cannot click on the log message itself to navigate to the GameObject. However, you can click the location information (second line showing filename, method, and line number) to navigate to the source code.
- Instance methods require a MonoBehaviour or GameObject context. Static methods have no GameObject context.
- File logging writes on application quit, not in real-time. It's intended for post-session analysis, not live streaming.
- You need to copy the Demo files into your normal Asset folder to use the Demo scene. See [this explanation here as to why](https://forum.unity.com/threads/it-is-not-allowed-to-open-a-scene-in-a-read-only-package-why.118036/).

## Inline Documentation

The project includes comprehensive inline documentation in the source code:

- All classes have detailed XML documentation explaining their purpose
- All public methods have XML documentation with parameter descriptions
- Complex logic includes inline comments explaining the implementation
- Sample files include class-level documentation explaining what each demo demonstrates

For detailed usage notes, advanced patterns, and implementation details, refer to the inline documentation in the source files. This is especially useful for understanding:

- How the message deduplication strategy works
- How the color injection system works
- How the button positioning is calculated
- When to use Static vs instance methods

## Tests

You can find tests in the `LogTests.cs` file. To run the tests, open the Unity Test Runner and execute the tests from there.

## Contributing

If you find any issues or have suggestions for improvements, feel free to open an issue or submit a pull request.

## License

Under the MIT License See the [LICENSE](LICENSE) file for more information. Feel free to modify the content based on your project's specifics, and add any additional sections or information as needed.
