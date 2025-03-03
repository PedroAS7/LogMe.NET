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

using System;
using System.IO;

namespace LogMe
{
    /// <summary>
    /// Logger ready to output messages into a file
    /// </summary>
    public class FileLog : LogProvider
    {
        /// <summary>
        /// Creates a new log file output
        /// </summary>
        /// <param name="name">Logger name</param>
        /// <param name="path">Path to output file</param>
        /// <param name="level">Maximum logged messages' level</param>
        /// <param name="replace">If true, replaces file if exists. If not, appends to existing file. Default = false</param>
        /// <param name="extraFlags">Any extra flags used in logger. Default = none</param>
        /// <param name="bufferSize">Output buffer size. Must be strictly greater than 0. Default = 4KB</param>
        /// <param name="colorAtlas">Rich text color atlas</param>
        /// <exception cref="ArgumentException">Raised if buffer size is invalid</exception>
        public FileLog(string name,
                       string path,
                       LogLevel level,
                       bool replace = false,
                       LoggerFlags extraFlags = LoggerFlags.None,
                       int bufferSize = 4096,
                       IColorAtlas colorAtlas = null)
            : base(name, level, LoggerFlags.File | extraFlags, colorAtlas: colorAtlas)
        {
            if (bufferSize <= 0)
            {
                throw new ArgumentException("Invalid buffer size");
            }

            var fileStream = new FileStream(path,
                                            replace ? FileMode.Create : FileMode.Append,
                                            FileAccess.Write,
                                            FileShare.Write,
                                            bufferSize);

            OutStream = new StreamWriter(fileStream);
        }
    }
}