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

using System.Text;
using LogMe;

namespace Test;

/// <summary>
/// Tests string extensions
/// </summary>
public class ExtensionsUt
{
    /// <summary>
    /// Tests cases where returned string is empty
    /// </summary>
    [Test]
    public void EmptyStringRepetitionTest()
    {
        Assert.Multiple(() =>
        {
            Assert.That("".Repeat(0), Is.EqualTo(string.Empty));
            Assert.That("".Repeat(10), Is.EqualTo(string.Empty));
            Assert.That("T".Repeat(0), Is.EqualTo(string.Empty));
        });
    }

    
    /// <summary>
    /// Cases where returned string is not empty
    /// </summary>
    [Test]
    public void StringRepetitionTest()
    {
        const string str = "test";
        Assert.Multiple(() =>
        {
            Assert.That(str.Repeat(1), Is.EqualTo(str));
            Assert.That(str.Repeat(2), Is.EqualTo(str + str));
            Assert.That(str.Repeat(10), Has.Length.EqualTo(str.Length * 10));
            Assert.Throws<OverflowException>(() => str.Repeat((uint)(Extensions.MaxRepeatedLength / str.Length) - 1).Repeat(2));
        });
    }
}