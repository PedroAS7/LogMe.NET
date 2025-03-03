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

using System.Collections.Generic;
using System.Drawing;

namespace LogMe;

/// <summary>
/// An interface allowing LogMe.NET logger to retrieve rich text colors to use
/// </summary>
public interface IColorAtlas
{
    /// <summary>
    /// Returns a read-only dictionary containing pairs of log level -> colors to be used for log's text color in
    /// rich text. In case a log level not present in dictionary is used for a log entry, Color.Black is used
    /// </summary>
    /// <returns>The read-only dictionary</returns>
    public IReadOnlyDictionary<LogLevel, Color> GetForegroundColors();

    /// <summary>
    /// Returns a read-only dictionary containing pairs of log level -> colors to be used for log's background color in
    /// rich text. In case a log level not present in dictionary is used for a log entry, Color.Transparent is used
    /// </summary>
    /// <returns>The read-only dictionary</returns>
    public IReadOnlyDictionary<LogLevel, Color> GetBackgroundColors();
}