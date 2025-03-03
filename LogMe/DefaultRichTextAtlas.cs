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

namespace LogMe
{
    /// <summary>
    /// Contains dictionaries of log level to color pairs 
    /// </summary>
    public class DefaultRichTextAtlas : IColorAtlas
    {
        /// <summary>
        /// Dictionary containing log level to foreground color pairs 
        /// </summary>
        private static readonly IReadOnlyDictionary<LogLevel, Color> LevelToForeground = new Dictionary<LogLevel, Color>
        {
            { LogLevel.Error,   Color.White },
            { LogLevel.Warning, Color.Black },
            { LogLevel.Info,    Color.Black },
            { LogLevel.Debug,   Color.White },
            { LogLevel.Trace,   Color.White }
        };

        /// <summary>
        /// Dictionary containing log level to background color pairs 
        /// </summary>
        private static readonly IReadOnlyDictionary<LogLevel, Color> LevelToBackground = new Dictionary<LogLevel, Color>
        {
            { LogLevel.Error,   Color.FromArgb(230, 0, 0) },
            { LogLevel.Warning, Color.FromArgb(255, 187, 51) },
            { LogLevel.Info,    Color.White },
            { LogLevel.Debug,   Color.FromArgb(0, 153, 230) },
            { LogLevel.Trace,   Color.FromArgb(40, 60, 136) }
        };

        public IReadOnlyDictionary<LogLevel, Color> GetForegroundColors() => LevelToForeground;

        public IReadOnlyDictionary<LogLevel, Color> GetBackgroundColors() => LevelToBackground;
    }
}
