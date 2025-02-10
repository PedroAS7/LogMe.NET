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

using System;

namespace LogMe
{
    /// <summary>
    /// Possible log levels. Messages will only be shown if logger's log level is equal to or above message's level
    /// </summary>
    public enum LogLevel
    {
        Error,
        Warning,
        Info,
        Debug,
        Trace
    }


    /// <summary>
    /// Logger flags that will modify its output 
    /// </summary>
    [Flags]
    public enum LoggerFlags
    {
        None        = 0,    // No flags (stream only supports poor text, outputs minimal information, and is shown on a GUI)
        RichText    = 1,    // Does the stream support rich text?
        File        = 2,    // Is the stream a file in the disk?
        Timestamp   = 4,    // Should a timestamp be included?
        Caller      = 8,    // Should the caller class + name be included?
        Thread      = 16    // Should the log show the thread's name, if it's not the main thread?
    }
}
