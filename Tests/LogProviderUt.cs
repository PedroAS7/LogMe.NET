/*
 * LogMe
 * Copyright (C) 2023-2025 PeterAS17
 * https://peteras17.me/
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301
 * USA
 */

using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;
using LogMe;

namespace Test;


/// <summary>
/// Tests functionalities of the log provider class
/// </summary>
public class LogProviderUt
{
    private delegate void ManagerLogFunction(string message);
    private Logger _logger;

    
    [SetUp]
    public void Setup()
    {
        _logger = new Logger();
    }

    
    /// <summary>
    /// Log provider that allows log timestamp to be adjusted
    /// </summary>
    [SuppressMessage("Usage", "CA2245:Do not assign a property to itself")]
    private class AdjustableTimeLogProvider : StringBufferLog
    {
        public TimeSpan FixedTime { get; private set; }
        private bool _fixTime;
        
        public AdjustableTimeLogProvider(string name, StringWriter output, LogLevel level, LoggerFlags extraFlags = LoggerFlags.None)
            : base(name, output, level, extraFlags)
        {
            FixedTime = TimeSpan.Zero;
            _fixTime = false;
            
            // Tests fields are protected and not private
            DaysPadding = DaysPadding;
            SecsPadding = SecsPadding;
            MsPadding = MsPadding;
        }

        public override void Log(string message, LogLevel level, TimeSpan diff, string threadName, bool isMainThread)
        {
            base.Log(message, level, _fixTime ? FixedTime : diff, threadName, isMainThread);
        }

        public void SetTime(TimeSpan time)
        {
            FixedTime = time;
            _fixTime = true;
        }
    }

    /// <summary>
    /// Logger class used to test that an invalid log level on a call to .Log() raises an exception 
    /// </summary>
    private class BrokenLogLevelLogger : LogProvider
    {
        public BrokenLogLevelLogger(StringWriter outStream)
            : base("Broken log level logger", LogLevel.Debug, LoggerFlags.None, outStream)
        { }

        public override void Log(string message, LogLevel level, TimeSpan diff, string threadName, bool isMainThread)
        {
            base.Log(message, (LogLevel)int.MaxValue, diff, threadName, isMainThread);
        }
    }
    

    /// <summary>
    /// Tests the standard timestamp format and value
    /// </summary>
    [Test]
    public void TimestampTextTest()
    {
        const string testMessage = "Test";
        
        /*
         * No timestamp
         */
        {
            var sw = new StringWriter();
            var provider = new AdjustableTimeLogProvider("Log provider", sw, LogLevel.Trace);
            _logger.AddProvider(provider);

            // Fix time to 0ms and log message. Compare result
            TestContext.WriteLine("No timestamp");
            provider.SetTime(TimeSpan.Zero);
            Assert.That(sw.ToString(), Does.Not.Match(MatchingTimestampRegex(provider)));
            Console.WriteLine(sw.ToString());

            _logger.RemoveProvider(provider);
        }

        /*
         * With timestamp
         */
        {
            var sw = new StringWriter();
            var provider = new AdjustableTimeLogProvider("Log provider", sw, LogLevel.Trace, LoggerFlags.Timestamp);
            _logger.AddProvider(provider);

            TestWithTime(provider,
                         sw,
                         TimeSpan.FromMilliseconds(0));

            TestWithTime(provider,
                         sw,
                         TimeSpan.FromMilliseconds(1));

            TestWithTime(provider,
                         sw,
                         TimeSpan.FromMilliseconds(11));

            TestWithTime(provider,
                         sw,
                         TimeSpan.FromMilliseconds(111));

            TestWithTime(provider,
                         sw,
                         TimeSpan.FromSeconds(2));

            TestWithTime(provider,
                         sw,
                         TimeSpan.FromMilliseconds(2111));

            TestWithTime(provider,
                         sw,
                         TimeSpan.FromMilliseconds(22111));

            TestWithTime(provider,
                         sw,
                         TimeSpan.FromMilliseconds(222111));

            TestWithTime(provider,
                         sw,
                         TimeSpan.FromMilliseconds(2222111));

            TestWithTime(provider,
                         sw,
                         TimeSpan.FromDays(1) - TimeSpan.FromMilliseconds(1));

            TestWithTime(provider,
                         sw,
                         TimeSpan.FromDays(1));

            TestWithTime(provider,
                         sw,
                         TimeSpan.FromDays(10));

            TestWithTime(provider,
                         sw,
                         TimeSpan.FromDays(100));

            TestWithTime(provider,
                         sw,
                         TimeSpan.FromDays(1000));

            // Maximum amount of days the standard format supports
            TestWithTime(provider,
                         sw,
                         TimeSpan.FromDays(9999));
            
            // Maximum time the standard format supports
            TestWithTime(provider,
                         sw,
                         TimeSpan.FromDays(10000) - TimeSpan.FromMilliseconds(1));
            
            // Over maximum time
            TestContext.WriteLine($"Fix time to over maximum and log message. Compare result");
            sw.GetStringBuilder().Clear();
            provider.SetTime(TimeSpan.FromMilliseconds(TimeSpan.FromDays(10000).TotalMilliseconds));
            _logger.Info(testMessage);
            TestContext.WriteLine($"\tResult: {sw}");
            Assert.That(sw.ToString(), Does.Not.Match(MatchingTimestampRegex(provider)));

            _logger.RemoveProvider(provider);
        }
        return;

        // Function that generates a regex to match against the timestamp to check its format
        string MatchingTimestampRegex(AdjustableTimeLogProvider logger)
        {
            var loggerTime = logger.FixedTime;

            var daysLen = loggerTime.Days.ToString().Length;
            var regexStart = loggerTime >= TimeSpan.FromDays(1)
                ? $@"\[ \s{{{logger.DaysPadding - daysLen - 1}}}\d{{{daysLen}}}d "
                : $@"\[ \s{{{logger.DaysPadding}}} ";

            var secondsLen = ((uint)(loggerTime.Seconds + loggerTime.Minutes * 60 + loggerTime.Hours * 60 * 60)).ToString().Length;
            var regexStr = regexStart + $@"\s{{{logger.SecsPadding - secondsLen}}}\d{{{secondsLen}}}s\.\d{{{logger.MsPadding}}} \]";

            return regexStr;
        }

        // Function that generates a string to match against the timestamp to check the written value
        void TestWithTime(AdjustableTimeLogProvider provider, StringWriter buffer, TimeSpan time)
        {
            TestContext.WriteLine($"Fix time to {time} and log message. Compare result");
            buffer.GetStringBuilder().Clear();
            provider.SetTime(TimeSpan.FromMilliseconds(time.TotalMilliseconds));
            _logger.Info(testMessage);
            TestContext.WriteLine($"\tResult: {buffer}");

            var expectedEnd = time.Milliseconds.ToString();
            var expectedStart = string.Empty;
            
            var daySeconds = time.Seconds + time.Minutes * 60 + time.Hours * 60 * 60;
            if (daySeconds > 0)
            {
                expectedEnd = $"{daySeconds}s.{expectedEnd.Repeat((uint)(provider.MsPadding - expectedEnd.Length))}{expectedEnd}";
            }

            if (time.Days > 0)
            {
                expectedStart = $"{time.Days}d ";
            }
            
            // Test format with regex, then timestamp with values
            Assert.That(buffer.ToString(), Does.Match(MatchingTimestampRegex(provider)));
            Assert.That(buffer.ToString(), Does.Contain($"{expectedEnd} ]"));
            
            if (!string.IsNullOrEmpty(expectedStart))
            {
                Assert.That(buffer.ToString(), Does.Contain(expectedStart));
            }
        }
    }
    

    /// <summary>
    /// Tests that only the allowed message log levels are logged
    /// </summary>
    [Test]
    public void LogLevelsTest()
    {
        const string testMessage = "Test message";
        var managerFunc = new Dictionary<LogLevel, ManagerLogFunction>
        {
            { LogLevel.Error, _logger.Error },
            { LogLevel.Warning, _logger.Warning },
            { LogLevel.Info, _logger.Info },
            { LogLevel.Debug, _logger.Debug },
            { LogLevel.Trace, _logger.Trace }
        };

        foreach (LogLevel loggerLevel in Enum.GetValues(typeof(LogLevel)))
        {
            var writer = new StringWriter();
            var provider = new StringBufferLog("Str logger", writer, loggerLevel);
            _logger.AddProvider(provider);

            foreach (LogLevel messageLevel in Enum.GetValues(typeof(LogLevel)))
            {
                TestContext.Out.WriteLine($"Testing logger level {loggerLevel} with {messageLevel} message level");

                // Clear previous logs
                writer.GetStringBuilder().Clear();
                managerFunc[messageLevel](testMessage);

                // Expect output
                if (messageLevel <= loggerLevel)
                {
                    Assert.That(writer.GetStringBuilder(),
                        Has.Length.GreaterThanOrEqualTo(testMessage.Length)); // Compensate potential overhead
                }
                // Expect no output
                else
                {
                    Assert.That(writer.GetStringBuilder(), Has.Length.EqualTo(0));
                }
            }

            _logger.RemoveProvider(provider);
        }
    }

    
    /// <summary>
    /// Checks that manager throws an error if an unknown log level is passed in the parameters list
    /// </summary>
    [Test]
    public void InvalidLogLevelTest()
    {
        var writer = new StringWriter();
        var provider = new BrokenLogLevelLogger(writer);
        _logger.AddProvider(provider);
        Assert.Throws<ArgumentOutOfRangeException>(() => _logger.Debug("Test"));
        _logger.RemoveProvider(provider);
    }

    
    /// <summary>
    /// Checks that an exception is raised when logger tries to be used after it's closed
    /// </summary>
    [Test]
    public void OpsWhenStreamClosedTest()
    {
        var writer = new StringWriter();
        var provider = new StringBufferLog("Str logger", writer, LogLevel.Debug);
        _logger.AddProvider(provider);
        provider.Close();
        Assert.That(provider.Ready, Is.False);
        Assert.Throws<EndOfStreamException>(() => _logger.Debug("Test"));
        Assert.Throws<EndOfStreamException>(() => provider.Flush());
        _logger.RemoveProvider(provider);
    }


    /// <summary>
    /// Checks that a valid callstack is printed when .Exception() is called
    /// Logger is in non-debug log level
    /// </summary>
    [Test]
    public void CallstackInNonDebugExceptionTest()
    {
        const string exceptionMessage = "Test exception";
        var exception = new Exception(exceptionMessage);
        
        TestContext.Out.WriteLine("Callstack on non-debug log level");
        var writer = new StringWriter();
        var provider = new StringBufferLog("Str logger", writer, LogLevel.Error);
        _logger.AddProvider(provider);
        _logger.Exception(exception);
        _logger.RemoveProvider(provider);
        var logStr = writer.GetStringBuilder().ToString();
        TestContext.Out.WriteLine(logStr);
        
        // Contains error message
        Assert.That(logStr, Does.Contain(exceptionMessage));
        
        // Contains file and function
        Assert.That(logStr, Does.Match("at [a-zA-Z0-9.]+" + nameof(CallstackInNonDebugExceptionTest)));
        
        // Contains correct throwing function and no line
        Assert.That(logStr, Does.Contain(nameof(CallstackInNonDebugExceptionTest) + ":0"));
    }


    /// <summary>
    /// Checks that a valid callstack is printed when .Exception() is called
    /// Logger is in debug log level
    /// </summary>
    [Test]
    public void CallstackInDebugExceptionTest()
    {
        const string exceptionMessage = "Test exception";
        var exception = new Exception(exceptionMessage);
        
        TestContext.Out.WriteLine("Callstack on non-debug log level");
        var writer = new StringWriter();
        var provider = new StringBufferLog("Str logger", writer, LogLevel.Debug);
        _logger.AddProvider(provider);
        _logger.Exception(exception);
        var lineNumber = GetLine();
        _logger.RemoveProvider(provider);
        var logStr = writer.GetStringBuilder().ToString();
        TestContext.Out.WriteLine(logStr);
        
        // Contains error message
        Assert.That(logStr, Does.Contain(exceptionMessage));
        
        // Contains file and function
        Assert.That(logStr, Does.Match("at [a-zA-Z0-9.]+" + nameof(CallstackInDebugExceptionTest)));
        
        // Contains correct throwing function and correct line
        Assert.That(logStr, Does.Contain(nameof(CallstackInDebugExceptionTest) + ":" + (lineNumber - 1)));
        return;

        int GetLine([CallerLineNumber] int n = 0)
        {
            return n;
        }
    }
}