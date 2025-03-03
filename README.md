# LogMe.NET

A simple, yet effective logger for .NET. It allows you to get complete control over where your logs go to, and how they
are formatted, all from one object.

The library makes sure message logging is thread-safe, so even multithreaded projects can use it.


## What can I do with it?

On applications where you need to have logs, this library allows you to have a sort of "one-stop shop" class that
dispatches logs to all registered *log providers* (see below) you've registered.


## What's a log provider?

A log provider is any class type that inherits from the `LogMe.LogProvider` class. They're effectively the classes that
receive the logs, and forward them somewhere else - hence the name *provider*.

This library comes with three different log provider classes:

- `FileLog`: saves logs into a file. Useful when you need to make the logs available outside your application
- `StringBufferLog`: saves logs into a `StringWriter` object. Useful, for example, when you need to show the logs on
  your application's GUI
- `StdLog`: forwards logs into the `stdout` (`Console.Out`) or `stderr` (`Console.Error`), depending on the log's level.
  Useful for CLI applications


## Log levels

Different log levels can be used. Each log provider has its own log level, which cannot be changed. They are sorted
by importance:

- *Error*: used for logs that usually require your utmost attention. Also used to log exceptions
- *Warning*: mostly used for non-fatal but important logs
- *Info*: usually for informational logs about the application's execution
- *Debug*: used for logs that would help you to easily debug an application
- *Trace*: usually used for verbose logs that pinpoint the application's execution

Depending on the log provider's log level, it may or may not use the log it has received.


## Multithreaded environments

This library is able to handle logs coming from different threads, in order of arrival. The logging functions are
thread-safe.

The caller that creates the `LogMe.Logger`'s class instance is considered to be the main thread. All other threads
calling the logging functions will be considered to be secondary threads, and will have their name or ID, when a name
is unavailable, shown in the logs if the registered providers have the `LogMe.LoggerFlags.Thread` flag set.


## Timestamps

Logs may show the timestamp when the registered providers have the `LogMe.LoggerFlags.Timestamp` flag set. The timestamp
is formatted into a human-readable timestamp, splitting days, seconds, and milliseconds in the output.

The maximum timestamp is 9999 days, 86399.999 seconds (just 1 millisecond short of a 10000th day), which allows you to
leave your application executing for longer than you almost definitely need!


## Caller information

In some cases, it might be useful to show the current function's name and line on the logs. To allow this to happen,
providers may use the `LogMe.LoggerFlags.Caller` flag.


## Catching exceptions

This logging library allows you to easily log exceptions, and provides a human-readable stacktrace. If the log provider
has the `LoggerFlags.Caller` flag, and the log level is at least debug (so, `LogMe.LogLevel.Debug` or
`LogMe.LogLevel.Trace`), the library will attempt to retrieve the file name and path, as well as line number. This is
not always possible, especially when it comes to applications built in release mode without debug symbols. Example
stacktrace:

```
[                                   ThisSuperCoolApp.Main():38 ][E] An error has been raised at ThisSuperCoolApp.Main:38: Object reference not set to an instance of an object.
Call stack:
   at LogMe.LogProvider.Exception(Exception ex, TimeSpan diff, String threadName, Boolean isMainThread) in C:\...\LogMe.NET\LogProvider.cs:line 381
   at LogMe.Logger.Exception(Exception ex) in C:\...\LogMe.NET\Logger.cs:line 263
   at ThisSuperCoolApp.Main(String[] args) in C:\...\coolApp\Program.cs:line 38

```

*NOTE:* the padding behind the class and function names is intentionally large. It allows stack frames containing large
names to show up aligned to all other log entries. However, you may modify the amount of characters to display per
provider, using the `CallerInfoPadding` protected property. Values may range between `[5; 70]`


## Richtext support

If you want to have colorful that catch your attention at the glimpse of an eye, you might want to use the
`LogMe.LoggerFlags.RichText` flag for your providers. They will output text using the RichText format. The log entries
will also be formatted using a background and a text color. The class providing the colors may be changed using the
`colorAtlas` parameter when instantiating a provider.


## Usage examples

An example program can be found inside the `Example` folder in this project's public repository.

```csharp
using System;
using LogMe;


public class ThisSuperCoolApp
{
    static void Main(string[] args)
    {
        // Create the logger instance. This is the instance that will receive all logs and dispatch them
        var logger = new Logger();

        /*
         * Create the providers you need
         */

        // A log provider that forwards logs to the console, and shows the logs' timestamps. Max level is information
        var consoleLogProvider = new StdLog("Console logs",
                                            LogLevel.Info,
                                            LoggerFlags.Timestamp);

        // A log provider that forwards logs to a file, and shows the thread's information, as well as the caller's information
        // Logs all the logs it receives
        var fileLogProvider = new FileLog("File logs",
                                          "logFile.log",
                                          LogLevel.Trace,
                                          replace: true,
                                          extraFlags: LoggerFlags.Caller | LoggerFlags.Thread);

        // Register the providers
        logger.AddProvider(consoleLogProvider);
        logger.AddProvider(fileLogProvider);

        // Start logging!
        try
        {
            logger.Info("An informational message");
            logger.Debug("A debug message");
            var nullStr = null as string;
            if(nullStr.Contains("HA!"))
            {
                logger.Error("This shouldn't happen!");
            }
        }
        catch(NullReferenceException ex)
        {
            logger.Exception(ex);
        }

        // Close all log providers via the logger
        logger.Close();
    }
}
```

In the log file, you will find the following logs:

```
[                                   ThisSuperCoolApp.Main():36 ][I] An informational message
[                                   ThisSuperCoolApp.Main():37 ][D] A debug message
[                                   ThisSuperCoolApp.Main():38 ][E] An error has been raised at ThisSuperCoolApp.Main:38: Object reference not set to an instance of an object.
Call stack:
   at LogMe.LogProvider.Exception(Exception ex, TimeSpan diff, String threadName, Boolean isMainThread) in C:\...\LogMe.NET\LogProvider.cs:line 381
   at LogMe.Logger.Exception(Exception ex) in C:\...\LogMe.NET\Logger.cs:line 263
   at ThisSuperCoolApp.Main(String[] args) in C:\...\coolApp\Program.cs:line 38

```

In the console, the following logs are shown:

```
[           0s.031 ][I] An informational message
[           0s.071 ][E] An error has been raised at ThisSuperCoolApp.Main:0: Object reference not set to an instance of an object.
Call stack:
   at LogMe.LogProvider.Exception(Exception ex, TimeSpan diff, String threadName, Boolean isMainThread)
   at LogMe.Logger.Exception(Exception ex)
   at ThisSuperCoolApp.Main(String[] args)
```

## Time impact

Ideally, on the field, logging functionalities should so quick as to be close to imperceptible. Because of this,
we've setup stress tests that show how long it takes this library to log different amounts of messages with different
amounts of log providers. In these tests, logged messages' length is 32 bytes (31 bytes + 1 null char). Tests are run
using the `StringBufferLog` log provider class, with the buffer never being emptied.

As you'll notice, providers that log the caller information are by far the slowest ones. It is therefore advised to only
use log providers with these capabilities whenever needed, and not whenever possible.

Note that all shown times are the output of the tests when run with library being built on Release.

### Simple log

| Number of log providers | 1K messages | 10K messages | 100K messages |
|:-----------------------:|:-----------:|:------------:|:-------------:|
|            1            |   < 1 ms    |    < 1 ms    |     4 ms      |
|           10            |   < 1 ms    |     1 ms     |     18 ms     |
|           100           |    7 ms     |    40 ms     |    339 ms     |

### Log with timestamp

| Number of log providers | 1K messages | 10K messages | 100K messages |
|:-----------------------:|:-----------:|:------------:|:-------------:|
|            1            |   < 1 ms    |    < 1 ms    |     9 ms      |
|           10            |   < 1 ms    |     6 ms     |     67 ms     |
|           100           |    6 ms     |    73 ms     |    633 ms     |

### Log with caller information

| Number of log providers | 1K messages | 10K messages | 100K messages |
|:-----------------------:|:-----------:|:------------:|:-------------:|
|            1            |    3 ms     |    24 ms     |    244 ms     |
|           10            |    24 ms    |    241 ms    |   **2.4 s**   |
|           100           |   303 ms    |  **2.44 s**  |  **24.08 s**  |


## License

```
This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 3 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301
USA
```

You shall find the full license in the `COPYING.LESSER` file in this library's public repository.