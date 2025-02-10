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

public class LoggerUt
{
    private Logger _logger;
        
    [SetUp]
    public void Setup()
    {
        _logger = new Logger();
    }

    /// <summary>
    /// Tests the manager's functionalities when no loggers are present
    /// </summary>
    [Test]
    public void NoLoggersTest()
    {
        Assert.That(_logger.ProvidersNames, Is.Empty);
            
        // Message logging process
        Assert.Multiple(() =>
        {
            Assert.DoesNotThrow(() => _logger.Exception(new Exception()));
            Assert.DoesNotThrow(() => _logger.Error(""));
            Assert.DoesNotThrow(() => _logger.Warning(""));
            Assert.DoesNotThrow(() => _logger.Info(""));
            Assert.DoesNotThrow(() => _logger.Debug(""));
            Assert.DoesNotThrow(() => _logger.Trace(""));
        });
    }

        
    /// <summary>
    /// Tests the manager when it contains one logger
    /// </summary>
    [Test]
    public void OneProvidertest()
    {
        Assert.That(_logger.ProvidersNames, Is.Empty);
        const string loggerName = "Str logger";
        var stringStream = new StringWriter();
        var provider = new StringBufferLog(loggerName, stringStream, LogLevel.Trace);
            
        // Logger register process
        Assert.Multiple(() =>
        {
            Assert.DoesNotThrow(() => _logger.AddProvider(provider));
            Assert.That(_logger.ProvidersNames, Does.Contain(loggerName));
            Assert.That(_logger.ProvidersNames, Has.Length.EqualTo(1));
            Assert.Throws<ArgumentException>(() => _logger.AddProvider(provider)); // Repeated logger
        });
            
        // Message logging process
        Assert.Multiple(() =>
        {
            Assert.DoesNotThrow(() => _logger.Error(""));
            Assert.DoesNotThrow(() => _logger.Warning(""));
            Assert.DoesNotThrow(() => _logger.Info(""));
            Assert.DoesNotThrow(() => _logger.Debug(""));
            Assert.DoesNotThrow(() => _logger.Trace(""));
        });
            
        // Logger unregister process
        Assert.Multiple(() =>
        {
            Assert.DoesNotThrow(() => _logger.RemoveProvider(provider));
            Assert.That(_logger.ProvidersNames, Is.Empty);
            Assert.DoesNotThrow(() => _logger.AddProvider(provider));
            Assert.That(_logger.ProvidersNames, Has.Length.EqualTo(1));
            Assert.DoesNotThrow(() => _logger.RemoveProvider(provider.Name));
            Assert.That(_logger.ProvidersNames, Is.Empty);
            Assert.Throws<ArgumentException>(() => _logger.RemoveProvider(provider));
            Assert.Throws<ArgumentException>(() => _logger.RemoveProvider(provider.Name));
            Assert.That(_logger.ProvidersNames, Is.Empty);
        });

        _logger.Close();
    }
}