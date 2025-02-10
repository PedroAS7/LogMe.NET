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

using LogMe;

namespace Test;

/// <summary>
/// Tests the log provider class that dumps logs into stdout and stderr 
/// </summary>
public class StdLogUt
{
    private delegate void LoggerLogFunction(string message);
    private const string TestMessage = "Test message";
    private Logger _logger;
    private StdLog _provider;
    private StringWriter _newStdOut;
    private StringWriter _newStdErr;
    private TextWriter _stdOut;
    private TextWriter _stdErr;
    
    
    [SetUp]
    public void Setup()
    {
        // Backup and replace output streams 
        _stdOut = Console.Out;
        _newStdOut = new StringWriter();
        Console.SetOut(_newStdOut);
        
        _stdErr = Console.Error;
        _newStdErr = new StringWriter();
        Console.SetError(_newStdErr);
        
        // Add logger to manager
        _logger = new Logger();
        _provider = new StdLog("Logger", LogLevel.Trace);
        _logger.AddProvider(_provider);
    }
    
    
    [TearDown]
    public void TearDown()
    {
        // Restore output streams
        Console.SetOut(_stdOut);
        Console.SetError(_stdErr);
        
        _logger.RemoveProvider(_provider);
    }
    
    
    /// <summary>
    /// Checks that writes to stdout are done successfully for the non-error log levels
    /// </summary>
    [Test]
    public void WriteOnStdoutTest()
    {
        var loggerFunc = new Dictionary<LogLevel, LoggerLogFunction>
        {
            { LogLevel.Warning, _logger.Warning },
            { LogLevel.Info, _logger.Info },
            { LogLevel.Debug, _logger.Debug },
            { LogLevel.Trace, _logger.Trace }
        };
        
        foreach (var pair in loggerFunc)
        {
            TestContext.Out.WriteLine($"Testing with log level {pair.Key}");
            _newStdOut.GetStringBuilder().Clear();
            _newStdErr.GetStringBuilder().Clear();
            
            Assert.Multiple(() =>
            {
                Assert.DoesNotThrow(() => pair.Value(TestMessage));
                Assert.That(_newStdOut.GetStringBuilder().ToString(), Has.Length.GreaterThanOrEqualTo(TestMessage.Length));
                Assert.That(_newStdErr.GetStringBuilder().ToString(), Has.Length.EqualTo(0));
            });
        }
    }
    
    
    /// <summary>
    /// Checks that writes to stderr are done successfully for the non-error log levels
    /// </summary>
    [Test]
    public void WriteOnStderrTest()
    {
        var loggerFunc = new Dictionary<LogLevel, LoggerLogFunction>
        {
            { LogLevel.Error, _logger.Error }
        };
        
        foreach (var pair in loggerFunc)
        {
            TestContext.Out.WriteLine($"Testing with log level {pair.Key}");
            _newStdOut.GetStringBuilder().Clear();
            _newStdErr.GetStringBuilder().Clear();
            
            Assert.Multiple(() =>
            {
                Assert.DoesNotThrow(() => pair.Value(TestMessage));
                Assert.That(_newStdOut.GetStringBuilder().ToString(), Has.Length.EqualTo(0));
                Assert.That(_newStdErr.GetStringBuilder().ToString(), Has.Length.GreaterThanOrEqualTo(TestMessage.Length));
            });
        }
    }
}