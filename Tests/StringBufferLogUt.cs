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
/// Tests functionalities specific to the string buffer log provider class, and its inheriting children
/// </summary>
public class StringBufferLogUt
{
    private StringWriter _sw;
    private Logger _logger;
    private StringBufferLog _provider;

    [SetUp]
    public void Setup()
    {
        // Add logger to manager
        _sw = new StringWriter();
        _logger = new Logger();
        _provider = new StringBufferLog("Logger", _sw, LogLevel.Trace);
        _logger.AddProvider(_provider);
    }


    [TearDown]
    public void TearDown()
    {
        _logger.RemoveProvider(_provider);
    }

    /// <summary>
    /// Tests that the log provider can be cleared on demand
    /// </summary>
    [Test]
    public void ClearLogTest()
    {
        _logger.Debug("Test");
        Assert.That(_sw.GetStringBuilder().ToString(), Is.Not.Empty);
        Assert.That(_sw.GetStringBuilder().ToString(), Contains.Substring("Test"));
        _provider.Clear();
        Assert.That(_sw.GetStringBuilder().ToString(), Is.Empty);
        _logger.Debug("Test");
        Assert.That(_sw.GetStringBuilder().ToString(), Is.Not.Empty);
        Assert.That(_sw.GetStringBuilder().ToString(), Contains.Substring("Test"));
    }

    /// <summary>
    /// Makes sure users can get the logged files from the log provider
    /// </summary>
    [Test]
    public void GetLinesTest()
    {
        Assert.That(_provider.GetLines(), Is.Empty);
        _logger.Debug("Test");
        Assert.That(_provider.GetLines(), Has.Length.EqualTo(1));
        _logger.Debug("Test");
        Assert.That(_provider.GetLines(), Has.Length.EqualTo(2));
        _provider.Clear();
        Assert.That(_provider.GetLines(), Is.Empty);
        _logger.Debug("Test");
        _provider.KeepLast(0);
        Assert.That(_provider.GetLines(), Is.Empty);
    }

    /// <summary>
    /// Only keeps the last few lines of logs
    /// </summary>
    [Test]
    public void LogTruncateTest()
    {
        const string message = "Test";
        const int linesToKeep = 10;
        const int linesToLog = 15;
        var yetToLog = linesToLog;

        Assert.That(_provider.GetLines(), Is.Empty);

        while (yetToLog > 0)
        {
            var loggedSoFar = linesToLog - yetToLog;
            _logger.Debug(message);

            Assert.That(_provider.GetLines(), Has.Length.EqualTo(Math.Min(loggedSoFar + 1, linesToKeep + 1)));
            _provider.KeepLast(linesToKeep);
            Assert.That(_provider.GetLines(), Has.Length.EqualTo(Math.Min(loggedSoFar + 1, linesToKeep)));

            --yetToLog;
        }
    }
}