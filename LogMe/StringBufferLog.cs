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

using System.IO;
using System;

namespace LogMe
{
    /// <summary>
    /// Logger ready to output messages into a string buffer
    /// </summary>
    public class StringBufferLog : LogProvider
    {
        /// <summary>
        /// Creates a new string buffer logger
        /// </summary>
        /// <param name="name">Logger name</param>
        /// <param name="output">Output string stream</param>
        /// <param name="level">Maximum logged messages' level</param>
        /// <param name="extraFlags">Any extra flags used in logger</param>
        /// <param name="colorAtlas">Rich text color atlas</param>
        public StringBufferLog(string name,
                               StringWriter output,
                               LogLevel level,
                               LoggerFlags extraFlags = LoggerFlags.None,
                               IColorAtlas colorAtlas = null)
            : base(name, level, LoggerFlags.File | extraFlags, output, colorAtlas: colorAtlas)
        {
        }


        /// <summary>
        /// Returns all the lines logged so far to this log provider
        /// </summary>
        /// <returns>An array of lines</returns>
        public string[] GetLines()
        {
            lock (OutStream)
            {
                var sw = (StringWriter)OutStream;

                // Make sure we have all the latest data
                sw.Flush();
                var logs = sw.GetStringBuilder().ToString();

                if (string.IsNullOrEmpty(logs))
                {
                    return Array.Empty<string>();
                }

                return logs
                    .TrimEnd(NewLine)
                    .Split('\n');
            }
        }

        /// <summary>
        /// Keeps the last n logged lines, from most recent to oldest
        /// </summary>
        /// <param name="n">Number of lines to keep</param>
        public void KeepLast(int n = 50)
        {
            lock (OutStream)
            {
                var loggedStr = ((StringWriter)OutStream).GetStringBuilder().ToString();
                Clear();

                // User requested a clear
                if (n == 0)
                {
                    return;
                }

                // Search for index of (lines - n)th line
                var index = loggedStr.Length - 2;
                var linesCount = 0;
                while (index >= 0 && linesCount < n - 1)
                {
                    if (loggedStr[index] == NewLine)
                    {
                        ++linesCount;
                    }

                    --index;
                }

                // Only write if index is valid or not enough lines are in string
                if (index >= 0 || linesCount < n)
                {
                    // Skip last new line found
                    OutStream.Write(loggedStr.AsSpan(index + 1));
                }
            }
        }

        /// <summary>
        /// Clears the output stream
        /// </summary>
        public void Clear()
        {
            lock (OutStream)
            {
                ((StringWriter)OutStream).GetStringBuilder().Clear();
            }
        }
    }
}