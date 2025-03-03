/*
 * LogMe.NET
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
/// Tests the log class that dumps logs into a file
/// </summary>
public class FileLogUt
{
    private const string LogFilePath = "log_test.txt";
    private const string TestMessage = "This is a test";
    private Logger _logger = new();


    [SetUp]
    public void Setup()
    {
        _logger = new Logger();

        // Delete file if exists, because it'll mess with the test
        if (!File.Exists(LogFilePath))
        {
            return;
        }

        File.Delete(LogFilePath);
    }


    /// <summary>
    /// Checks that files are created, replaced, or not deleted when supposed
    /// </summary>
    /// <exception cref="Exception"></exception>
    [Test]
    public void FileAccessTest()
    {
        FileLog provider;

        TestContext.Out.WriteLine("File creation and one message logging works");
        Assert.DoesNotThrow(() =>
        {
            provider = new FileLog("Logger", LogFilePath, LogLevel.Trace, replace: false);
            _logger.AddProvider(provider);
            _logger.Debug(TestMessage);
            _logger.Flush();
            _logger.RemoveProvider(provider);
            if (new FileInfo(LogFilePath).Length < TestMessage.Length)
            {
                throw new Exception();
            }
        });

        TestContext.Out.WriteLine("New logger with same file and \"replace: false\" does not replace file");
        Assert.That(() =>
        {
            provider = new FileLog("Logger", LogFilePath, LogLevel.Trace, replace: false);
            provider.Close();
            return File.Exists(LogFilePath) && new FileInfo(LogFilePath).Length >= TestMessage.Length;
        }, Is.True);

        TestContext.Out.WriteLine("New logger with same file and \"replace: true\" replaces file");
        Assert.That(() =>
        {
            provider = new FileLog("Logger", LogFilePath, LogLevel.Trace, replace: true);
            provider.Close();
            return File.Exists(LogFilePath) && new FileInfo(LogFilePath).Length == 0;
        }, Is.True);
    }


    /// <summary>
    /// Checks that the log file instance gets flushed when managed removes it from the list
    /// </summary>
    [Test]
    public void AutoFlushOnRemoveTest()
    {
        var provider = new FileLog("Logger", LogFilePath, LogLevel.Trace, replace: false);
        _logger.AddProvider(provider);
        _logger.Debug(TestMessage);
        _logger.RemoveProvider(provider);

        Assert.That(new FileInfo(LogFilePath), Has.Length.GreaterThanOrEqualTo(TestMessage.Length));
    }


    /// <summary>
    /// Tests that valid buffer size is verified
    /// </summary>
    [Test]
    public void InvalidBufferSizeTest()
    {
        Assert.Throws<ArgumentException>(() => new FileLog("Logger", LogFilePath, LogLevel.Trace, bufferSize: -1));
        Assert.Throws<ArgumentException>(() => new FileLog("Logger", LogFilePath, LogLevel.Trace, bufferSize: 0));
    }
}