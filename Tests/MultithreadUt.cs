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

using System.Text.RegularExpressions;
using LogMe;

namespace Test;

/// <summary>
/// Test logger's functionalities in a multithreaded environment
/// </summary>
public class MultithreadUt
{
    private StringWriter _writer;
    private Logger _logger;
        
    [SetUp]
    public void Setup()
    {
        _logger = new Logger();
        _writer = new StringWriter();
    }


    /// <summary>
    /// Test logger in a multithreaded environment. 10 threads logging 10000 messages each
    /// </summary>
    [Test]
    public void MultithreadWriteTest()
    {
        const int threadCount = 10;
        const int iterations = 10000;
        const string message = "Hello World! Thread ID: ";
        var threadList = new Thread[threadCount];
        var threadSynchSem = new Semaphore(0, threadCount);
        
        // Create logger with extra options just so that logging takes longer than logging a simple message
        var provider = new StringBufferLog("String logger",
                                           _writer,
                                           LogLevel.Trace,
                                           LoggerFlags.Caller | LoggerFlags.File | LoggerFlags.Timestamp);
        
        // Register logger
        _logger.AddProvider(provider);

        TestContext.Out.WriteLine("Starting all threads");
        for (var i = 0; i < threadCount; i++)
        {
            threadList[i] = new Thread((runInfo) =>
            {
                // Extract run information
                var threadId = ((int[])runInfo!)[0];
                var it = ((int[])runInfo)[1];
                TestContext.Out.WriteLine($"\tThread {threadId} is running");
                
                // Wait for synchronization before starting logging
                threadSynchSem.WaitOne();
                TestContext.Out.WriteLine($"\tThread {threadId} got synch signal");
                
                for (var j = 0; j < it; j++)
                {
                    _logger.Info(message + threadId!);   
                }
                
                TestContext.Out.WriteLine($"\tThread {threadId} is stopping");
            });
            
            threadList[i].Start(new []{ i, iterations });
        }
        
        threadSynchSem.Release(threadCount);
        
        TestContext.Out.WriteLine("Joining all threads");
        for (var i = 0; i < threadCount; i++)
        {
            threadList[i].Join();
        }
        
        TestContext.Out.WriteLine("Checking output");
        var expectedMinOutBytes = threadCount * iterations * message.Length;
        var fullLog = _writer.ToString();
        
        Assert.That(fullLog, Has.Length.GreaterThanOrEqualTo(expectedMinOutBytes));
        
        for (var i = 0; i < threadCount; i++)
        {
            var stringMatch = message + i;
            var matches = Regex.Matches(fullLog, stringMatch);
            Assert.That(matches, Has.Count.EqualTo(iterations));
        }
    }


    /// <summary>
    /// Checks that, in a multithreaded environment, the thread's name or ID show up in the logs
    /// </summary>
    [Test]
    public void ThreadNamesTest()
    {
        // Create provider that logs the thread's information
        var provider = new StringBufferLog("String logger",
                                           _writer,
                                           LogLevel.Trace,
                                           LoggerFlags.Thread);
        
        // Register provder
        _logger.AddProvider(provider);

        TestContext.Out.WriteLine("Starting all threads");
        
        // Thread 0 - Is named
        const string threadZeroName = "Thread0";
        
        var threadZero = new Thread(() =>
        {
            lock (_logger)
            {
                _logger.Info("Test");
                Assert.That(_writer.ToString(), Contains.Substring(threadZeroName));
            }
        });
        threadZero.Name = threadZeroName;
        threadZero.Start();
        threadZero.Join();
        
        // Thread 1 - Unnamed
        var threadOne = new Thread(() =>
        {
            lock (_logger)
            {
                _logger.Info("Test");
                Assert.That(_writer.ToString(), Contains.Substring(Environment.CurrentManagedThreadId.ToString("x8")));
            }
        });
        threadOne.Start();
        threadOne.Join();
    }
}