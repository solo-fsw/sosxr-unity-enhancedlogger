# EnhancedLogger

- By: Maarten R. Struijk Wilbrink
- For: Leiden University SOSXR
- Fully open source: Feel free to add to, or modify, anything you see fit.

A utility class for logging messages with customisable log levels and colours in Unity.

## Table of Contents

- [Introduction](#introduction)
- [Features](#features)
- [Downsides](#downsides)
- [Installation](#installation)
- [Usage](#usage)
    - [Log Levels](#log-levels)
    - [Log Messages](#log-messages)
- [Tests](#tests)
- [Contributing](#contributing)
- [License](#license)

## Introduction

This Unity package provides a flexible and customizable logging utility named `Log`. It allows you to log messages with
different log levels (Error, Warning, Debug, Success, Info) and customize the appearance of log messages with colors.
The utility is designed to be easy to integrate into your Unity projects.

## Features

- Log messages with various log levels, thereby suppressing ones you don't want to see.
- Customize log message appearance with colors (if you've cloned the project instead of using the package manager, you
  can change the colors in the `Log.cs` file).
- Unity Editor and Development Build aware. It will suppress your logs in a production build, making it a cheap solution
  for logging, and the best of both worlds: all the logging you could want in development, and none of the overhead in
  production.
- Click to go to the source of the log message, both the GameObject where the call came from, and the code + line number
  it is referring to (although not perfect, see Downsides)

## Downsides

- Unfortunately you can't click on the log message itself, but you can click on the second line of the log to go to the
  line number in the class (Otherwise you'll go to the `Log.cs` class instead). The first lines in the Console are
  reserved for the Log utility. Not so handy when you want to click to the code that generated the log message. If you
  know how to fix this, please let me know.
- It can only find the GameObject that called the Log message if it is a MonoBehaviour. If you call the `Log` directly (
  as you would in a static class for instance), it will not be able to click to find the GameObject.
- You need to copy the Demo files into your normal Asset folder to use the Demo scene.
  See [this explanation here as to why](https://forum.unity.com/threads/it-is-not-allowed-to-open-a-scene-in-a-read-only-package-why.1148036/).

## Installation

### Unity Package Manager

1. Open your Unity project.
2. Open the Unity Package Manager from the `Window` menu.
3. Click the `+` button in the top-left corner of the Package Manager window.
4. Select `Add package from git URL…`.
5. Enter the following Git URL in the input field: `https://github.com/mrstruijk/EnhancedLogger.git` (making sure of the
   `.git` at the end).
6. Click the `Add` button.

## Manual Installation

If you prefer manual installation (also allowing you more customizability), clone or
download [the repository](https://github.com/mrstruijk/EnhancedLogger) to your Unity project.

# Usage

## Log Levels

- Error
- Warning
- Info
- Debug
- Success
- Verbose

Each LogLevel shows the logs of the selected LogLevel and all logs with a higher LogLevel. For example, if you select
`Warning`, you will see both `Warning`, and `Error` logs. Selecting `Verbose` will show all logs. The default LogLevel
is `Debug` (showing `Debug`, `Info`, `Warning`, and `Error` logs).

### Opinionated Usage

`Debug` is used only for when you're working on a specific module. Put in as many logs as you want, to see how the thing
you're building is progressing. Then, when things 'settle down' and things start working as intended: move (almost) all
those `Debug` logs to the appropriate higher or lower level. By the end of each day / sprint / module, `Debug` should be
empty again.

`Info` is placed between `Debug` and `Warning` in the hierarchy. With `Verbose` there's too many other messages that are
displaying, while a `Warning` is often also not what I was looking for. A lot of the messages were of things that were
simply happening, without them needing to warn me of things.

## Setting Log Levels

In the Scene view, select the LogLevel you want to use. This will determine which log messages are displayed in the
Console. It shows the logs of the selected LogLevel and all logs with a higher LogLevel.

Alternatively you can set the LogLevel in the Menu bar at SOSXR > EnhancedLogger.

## Log Messages

To add logs to you scripts, see the example below, and the Samples folder for more examples.

```csharp
using UnityEngine;
using SOSXR.EnhancedLogger;

// Log messages with different levels. Note that depending on the CurrentLogLevel, maybe not all messages will be logged.
public class Example : MonoBehaviour
{
    public GameObject OtherGameObject;
    
    private void Start()
    {
        // You can use them as an extension method:
        // Standard way of logging is using 'this'. It will log the message from the current gameobject and print it's name.
        this.Debug("This is a debug message.");
        this.Warning("This is a warning message.");
     
        // Another object could serve as well.   
        OtherGameObject.Success("This is a success message. It is called from another gameobject"); // It will display the name of that other GameObject
        
        // Or from the static class:
        // These are useful when the object you're calling the Log from can be destroyed. Be careful with the second one, thay may cause a NullReferenceException anyway.
        Log.Info("Provide a name here", "This is an info message.");
        Log.Error(nameof(SomeObscureClass), "This is an error message.");
    }
}
```

It also works on static classes, or classes that are not derived from `MonoBehaviour`. Just use the `Log` class
directly (e.g. `Log.Debug("This is a debug message.");`).

## Tests

You can find tests in the LogTests.cs file. To run the tests, open the Unity Test Runner and execute the tests from
there.

## Contributing

If you find any issues or have suggestions for improvements, feel free to open an issue or submit a pull request.

## License

Under the MIT License See the [LICENSE](LICENSE) file for more information.
Feel free to modify the content based on your project's specifics, and add any additional sections or information as
needed.