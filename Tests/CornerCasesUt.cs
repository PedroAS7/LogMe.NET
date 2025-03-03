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
/// Tests corner cases
/// </summary>
public class CornerCasesUt
{
    private delegate void LoggingFunction(string message);


    /// <summary>
    /// Makes sure that trying to add an already-registered provider does not work, and same goes for removing one
    /// </summary>
    [Test]
    public void RepeatedLogProviderTest()
    {
        var logger = new Logger();
        var sw = new StringWriter();
        var provider = new StringBufferLog("Test provider", sw, LogLevel.Trace);

        Assert.DoesNotThrow(() => logger.AddProvider(provider));
        Assert.Throws<ArgumentException>(() => logger.AddProvider(provider));

        Assert.DoesNotThrow(() => logger.RemoveProvider(provider));
        Assert.Throws<ArgumentException>(() => logger.RemoveProvider(provider));
    }


    /// <summary>
    /// Tests functions with null or empty parameters
    /// </summary>
    [Test]
    public void EmptyOrNullParametersTest()
    {
        var logger = new Logger();
        var sw = new StringWriter();
        var provider = new StringBufferLog("Test provider", sw, LogLevel.Trace);
        logger.AddProvider(provider);

        Assert.Throws<ArgumentNullException>(() => logger.AddProvider(null));
        Assert.Throws<ArgumentException>(() => logger.AddProvider(new StringBufferLog("", new StringWriter(), LogLevel.Trace)));
        Assert.Throws<ArgumentException>(() => logger.AddProvider(new StringBufferLog("  ", new StringWriter(), LogLevel.Trace)));
        Assert.Throws<ArgumentNullException>(() => logger.RemoveProvider(null as LogProvider));
        Assert.Throws<ArgumentNullException>(() => logger.RemoveProvider(null as string));

        TestLogFunction(logger.Trace, sw);
        TestLogFunction(logger.Debug, sw);
        TestLogFunction(logger.Info, sw);
        TestLogFunction(logger.Warning, sw);
        TestLogFunction(logger.Error, sw);

        logger.Exception(null);
        Assert.That(sw.ToString(), Is.Not.Empty);

        logger.RemoveProvider(provider);
        return;

        void TestLogFunction(LoggingFunction func, StringWriter buffer)
        {
            buffer.GetStringBuilder().Clear();

            func(null);
            Assert.That(buffer.ToString(), Is.Empty);

            func("");
            Assert.That(buffer.ToString(), Is.Empty);

            func("         ");
            Assert.That(buffer.ToString(), Is.Empty);
        }
    }


    /// <summary>
    /// Makes sure that provider names cannot be deleted
    /// </summary>
    [Test]
    public void NoDeletingProvidersTest()
    {
        var logger = new Logger();
        var sw = new StringWriter();
        var provider = new StringBufferLog("Test provider", sw, LogLevel.Trace);
        logger.AddProvider(provider);

        Assert.That(logger.ProvidersNames, Is.Not.Empty);
        logger.ProvidersNames.SetValue(null, 0);
        Assert.That(logger.ProvidersNames, Is.Not.Empty);
        Assert.That(logger.ProvidersNames[0], Is.Not.Null);

        logger.RemoveProvider(provider);
    }
}