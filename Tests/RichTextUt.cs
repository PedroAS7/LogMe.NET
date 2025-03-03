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

using System.Drawing;
using LogMe;
using System.Xml;

namespace Test;

/// <summary>
/// Tests rich text output from log providers
/// </summary>
public class RichTextUt
{
    private delegate void ManagerLogFunction(string message);

    private StringWriter _sw;
    private Logger _logger;
    private StringBufferLog _provider;

    [SetUp]
    public void Setup()
    {
        // Add logger to manager
        _sw = new StringWriter();
        _logger = new Logger();
        _provider = new StringBufferLog("Logger", _sw, LogLevel.Trace, LoggerFlags.RichText);
        _logger.AddProvider(_provider);
    }


    [TearDown]
    public void TearDown()
    {
        _logger.RemoveProvider(_provider);
    }

    /// <summary>
    /// Makes sure rich text output actually includes the default colors
    /// </summary>
    [Test]
    public void DefaultRichTextColorTest()
    {
        const string message = "Test";
        var levelToFuncDict = new Dictionary<LogLevel, ManagerLogFunction>
        {
            { LogLevel.Error, _logger.Error },
            { LogLevel.Warning, _logger.Warning },
            { LogLevel.Info, _logger.Info },
            { LogLevel.Debug, _logger.Debug },
            { LogLevel.Trace, _logger.Trace }
        };

        foreach (var levelToFuncPair in levelToFuncDict)
        {
            var colorAtlas = new DefaultRichTextAtlas();
            var expectedStyle = @"background-color:\s*" + colorAtlas.GetBackgroundColors()[levelToFuncPair.Key].ToHexString() + @";\s*color:\s*" + colorAtlas.GetForegroundColors()[levelToFuncPair.Key].ToHexString();
            
            _provider.Clear();
            levelToFuncPair.Value(message);
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(_sw.GetStringBuilder().ToString());
            Console.Write(xmlDoc.Name);

            Assert.Multiple(() =>
            {
                Assert.That(xmlDoc.ChildNodes, Has.Count.EqualTo(1));
                Assert.That(xmlDoc.GetElementsByTagName("p"), Has.Count.EqualTo(1));
                Assert.That(xmlDoc.ChildNodes[0]!.Attributes, Is.Not.Null);
                Assert.That(xmlDoc.ChildNodes[0]!.Attributes!, Has.Count.EqualTo(1));
                Assert.That(xmlDoc.ChildNodes[0]!.Attributes!.GetNamedItem("style"), Is.Not.Null);
                Assert.That(xmlDoc.ChildNodes[0]!.Attributes!.GetNamedItem("style")!.Value, Does.Match(expectedStyle));
            });
        }
    }
}