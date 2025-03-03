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

namespace LogMe
{
    /// <summary>
    /// Logger ready to output messages into stdout or stderr, depending on log level
    /// </summary>
    public class StdLog : LogProvider
    {
        /// <summary>
        /// Creates a new console output logger
        /// </summary>
        /// <param name="name">Logger name</param>
        /// <param name="level">Maximum logged messages' level</param>
        /// <param name="extraFlags">Any extra flags used in logger</param>
        public StdLog(string name,
                      LogLevel level,
                      LoggerFlags extraFlags = LoggerFlags.None)
            : base(name, level, LoggerFlags.File | extraFlags)
        {
        }

        public override void Log(string message, LogLevel level, TimeSpan diff, string threadName, bool isMainThread)
        {
            // Set output stream to output log, and reset it at the end
            OutStream = level == LogLevel.Error ? Console.Error : Console.Out;
            base.Log(message, level, diff, threadName, isMainThread);
            OutStream = null;
        }
    }
}