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

using System.Collections.Generic;

namespace LogMe
{
    /// <summary>
    /// Contains dictionaries of log level to color pairs 
    /// </summary>
    internal static class RichTextDb
    {
        /// <summary>
        /// Dictionary containing log level to foreground color pairs 
        /// </summary>
        internal static readonly Dictionary<LogLevel, string> LevelToForeground = new Dictionary<LogLevel, string>
        {
            { LogLevel.Error,   "#FFFFFF" },
            { LogLevel.Warning, "#000000" },
            { LogLevel.Info,    "#000000" },
            { LogLevel.Debug,   "#FFFFFF" },
            { LogLevel.Trace,   "#FFFFFF" }
        };

        /// <summary>
        /// Dictionary containing log level to background color pairs 
        /// </summary>
        internal static readonly Dictionary<LogLevel, string> LevelToBackground = new Dictionary<LogLevel, string>
        {
            { LogLevel.Error,   "#E60000" },
            { LogLevel.Warning, "#FFBB33" },
            { LogLevel.Info,    "#FFFFFF" },
            { LogLevel.Debug,   "#0099E6" },
            { LogLevel.Trace,   "#283C88" }
        };

        /// <summary>
        /// Dictionary containing log level to log level prefixes 
        /// </summary>
        internal static readonly Dictionary<LogLevel, string> LevelToPrefix = new Dictionary<LogLevel, string>
        {
            { LogLevel.Error,   "E" },
            { LogLevel.Warning, "W" },
            { LogLevel.Info,    "I" },
            { LogLevel.Debug,   "D" },
            { LogLevel.Trace,   "T" }
        };
    }
}
