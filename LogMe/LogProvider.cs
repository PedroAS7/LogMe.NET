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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace LogMe
{
    /// <summary>
    /// Base class for an object that receives logs, and provides them to an output stream
    /// </summary>
    public abstract class LogProvider
    {
        /// <summary>
        /// This class' name
        /// </summary>
        private static readonly string CurrentClassName = typeof(LogProvider).FullName ?? "";

        /// <summary>
        /// Logger class' name 
        /// </summary>
        private static readonly string LogMgrClassName = typeof(Logger).FullName ?? "";

        /// <summary>
        /// Number of call frames obtained to ask GC to collect previous ones
        /// </summary>
        private static ushort _obtainedCallFrames;

        /// <summary>
        /// Maximum number of call frames obtained before asking GC to collect garbage
        /// </summary>
        private const ushort MaximumObtainedCallFrames = 1000;

        /// <summary>
        /// Minimum padding size for caller information
        /// </summary>
        private const uint MinCallerInfoPadding = 5;

        /// <summary>
        /// Maximum padding size for caller information
        /// </summary>
        private const uint MaxCallerInfoPadding = 70;

        /// <summary>
        /// Padding size for caller information. Default is 60 characters
        /// </summary>
        private uint _callerInfoPadding = 60;

        /// <summary>
        /// Dictionary containing log level to log level prefixes 
        /// </summary>
        private static readonly IReadOnlyDictionary<LogLevel, string> LevelToPrefix = new Dictionary<LogLevel, string>
        {
            { LogLevel.Error,   "E" },
            { LogLevel.Warning, "W" },
            { LogLevel.Info,    "I" },
            { LogLevel.Debug,   "D" },
            { LogLevel.Trace,   "T" }
        };

        /// <summary>
        /// Interface instance containing functions returning rich text background or foreground colors
        /// </summary>
        protected readonly IColorAtlas ColorAtlas;

        /// <summary>
        /// Character used for new line
        /// </summary>
        protected const char NewLine = '\n';

        /// <summary>
        /// Output stream for this provider
        /// </summary>
        protected TextWriter OutStream;

        /// <summary>
        /// Provider's name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Provider's log level
        /// </summary>
        public LogLevel LogLevel { get; }

        /// <summary>
        /// Provider's flags
        /// </summary>
        public LoggerFlags Flags { get; }

        /// <summary>
        /// Padding size for days when showing log time
        /// </summary>
        public uint DaysPadding { get; protected set; } = 5;

        /// <summary>
        /// Padding size for seconds when showing log time
        /// </summary>
        public uint SecsPadding { get; protected set; } = 5;

        /// <summary>
        /// Padding size for milliseconds when showing log time
        /// </summary>
        public uint MsPadding { get; protected set; } = 3;

        /// <summary>
        /// Padding size for caller information. Checks that value is [5; 70]
        /// </summary>
        public uint CallerInfoPadding
        {
            get => _callerInfoPadding;
            protected set
            {
                if (value is < MinCallerInfoPadding or > MaxCallerInfoPadding)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "CallerInfoPadding");
                }

                _callerInfoPadding = value;
            }
        }

        /// <summary>
        /// Returns true when output stream is not null, meaning logs can be sent to this provider
        /// </summary>
        public bool Ready => OutStream != null;

        /// <summary>
        /// Logger constructor
        /// </summary>
        /// <param name="name">Logger name</param>
        /// <param name="level">Minimum acceptable message level</param>
        /// <param name="flags">Logger flags</param>
        /// <param name="outStream">Output stream</param>
        /// <param name="colorAtlas">Rich text color atlas</param>
        protected LogProvider(string name,
                              LogLevel level,
                              LoggerFlags flags,
                              TextWriter outStream = null,
                              IColorAtlas colorAtlas = null)
        {
            Name = name;
            Flags = flags;
            LogLevel = level;
            OutStream = outStream;
            ColorAtlas = colorAtlas ?? new DefaultRichTextAtlas();
        }


        ~LogProvider()
        {
            Close();
        }


        /// <summary>
        /// Returns the current time as a string, based on when the logger started
        /// </summary>
        /// <param name="diff">Time delta since logger was created</param>
        /// <returns>A formatted string containing the current time</returns>
        private string GetTimestamp(TimeSpan diff)
        {
            const string retVal = "[ {pD}{d} {pS}{s}s.{pM}{ms} ]";
            var dayCount = diff.Days;
            var totalSeconds = (int)diff.TotalSeconds;
            var totalMs = diff.Milliseconds;

            // Formatting variables
            var showDays = dayCount > 0;

            var calcDaysPadding = DaysPadding;
            var calcSecsPadding = SecsPadding;
            var calcMsPadding = MsPadding;

            if (showDays)
            {
                calcDaysPadding -= 1; // for the 'd' indicator

                // Underflow cases
                if (calcDaysPadding < (uint)dayCount.ToString().Length)
                {
                    calcDaysPadding = 0;
                }
                else
                {
                    calcDaysPadding -= (uint)dayCount.ToString().Length;
                }

                // Subtract seconds from fully-elapsed days
                totalSeconds -= dayCount * 60 * 60 * 24;
            }

            calcSecsPadding -= (uint)totalSeconds.ToString().Length;
            calcMsPadding -= (uint)totalMs.ToString().Length;

            /*
             * Format the output
             */

            return retVal.Replace("{pD}", " ".Repeat(calcDaysPadding))
                         .Replace("{d}", showDays ? $"{dayCount.ToString()}d" : string.Empty)
                         .Replace("{pS}", " ".Repeat(calcSecsPadding))
                         .Replace("{s}", totalSeconds.ToString())
                         .Replace("{ms}", totalMs.ToString())
                         .Replace("{pM}", "0".Repeat(calcMsPadding));
        }


        /// <summary>
        /// Retrieves the caller's stacktrace frame to then output the caller's information
        /// </summary>
        /// <returns>String containing caller's frame</returns>
        private StackFrame GetCallerFrame()
        {
            // Skip this method, as well as the logger's logging method (debug, info, exception, etc.)
            var callStack = new StackTrace(2, fNeedFileInfo: LogLevel >= LogLevel.Debug);

            // If we have information about where we are
            if (string.IsNullOrEmpty(CurrentClassName))
                return new StackFrame();

            // Stack traces are heavy, and logging very often with log providers that require stack traces to be
            // generated is memory intensive. Because we don't know when GC is going to run, we politely ask it to run
            ++_obtainedCallFrames;
            if (_obtainedCallFrames >= MaximumObtainedCallFrames)
            {
                // Objects are short-lived. They belong to generation 0
                GC.Collect(0, GCCollectionMode.Optimized);
                _obtainedCallFrames = 0;
            }

            // Search through all the frames until we find one that matches a potential caller frame
            foreach (var frame in callStack.GetFrames())
            {
                var methodBase = frame.GetMethod();
                if (null == methodBase || null == methodBase.DeclaringType)
                {
                    continue;
                }

                // Break at the first frame where the class is not this class
                if (CurrentClassName != methodBase.DeclaringType.FullName
                 && LogMgrClassName  != methodBase.DeclaringType.FullName)
                {
                    return frame;
                }
            }

            return new StackFrame();
        }


        /// <summary>
        /// Obtains the caller's information
        /// </summary>
        /// <returns>String containing formatted caller's information</returns>
        private string GetCallerInfo()
        {
            const string retVal = "[ {pF}{f} ]";

            var callerFrame = GetCallerFrame();
            var methodBase = callerFrame.GetMethod();

            // If we didn't find a suitable frame to show
            if (null == methodBase || null == methodBase.DeclaringType)
            {
                return retVal.Replace("{f}", "?.?:?")
                    .Replace("{pF}",
                        " ".Repeat((uint)(CallerInfoPadding - retVal.Length +
                                          4))); // + 4 because of the {pM} placeholder
            }

            // Build the string containing the caller's information
            var frameInfo = $"{methodBase.DeclaringType.Name}.{methodBase.Name}():{callerFrame.GetFileLineNumber()}";
            if (frameInfo.Length > CallerInfoPadding - 4)
            {
                frameInfo = $"...{frameInfo.Substring(frameInfo.Length - (int)CallerInfoPadding + 3)}";
            }

            // Format the returned string
            return retVal.Replace("{f}", frameInfo)
                .Replace("{pF}", " ".Repeat((uint)(CallerInfoPadding - frameInfo.Length)));
        }


        /// <summary>
        /// Prints the pre-message header. Depending on the flags enabled, outputs:
        /// - Rich text tags
        /// - Timestamp
        /// - Caller information
        /// </summary>
        /// <param name="messageLevel">Output message level</param>
        /// <param name="diff">Time delta since logger was created</param>
        /// <param name="threadName">Caller's thread name 8B hex ID if name is not available</param>
        /// <param name="isMainThread">Is the caller the thread that instantiated the logger?</param>
        private void PrintPreMessage(LogLevel messageLevel, TimeSpan diff, string threadName, bool isMainThread)
        {
            if (Flags.HasFlag(LoggerFlags.RichText))
            {
                var foreColor = ColorAtlas.GetForegroundColors()
                                                .GetValueOrDefault(messageLevel, Color.Black)
                                                .ToHexString();

                var backColor = ColorAtlas.GetBackgroundColors()
                                                .GetValueOrDefault(messageLevel, Color.Transparent)
                                                .ToHexString();

                OutStream.Write(
                    $"<p style=\"background-color:{backColor}; color:{foreColor}; width:100%; margin:0, padding:5\">");
            }

            if (Flags.HasFlag(LoggerFlags.Timestamp))
            {
                OutStream.Write(GetTimestamp(diff));
            }

            if (!isMainThread)
            {
                OutStream.Write($"[@{threadName}]");
            }

            if (Flags.HasFlag(LoggerFlags.Caller))
            {
                OutStream.Write(GetCallerInfo());
            }

            OutStream.Write($"[{LevelToPrefix[messageLevel]}] ");
        }


        /// <summary>
        /// Prints the post-message data. Depending on the enabled flags, outputs:
        /// - Rich text's end tag
        /// - New line
        /// </summary>
        /// <exception cref="EndOfStreamException"></exception>
        private void PrintPostMessage()
        {
            if (!Ready)
            {
                throw new EndOfStreamException("Logger is not ready!");
            }

            if (Flags.HasFlag(LoggerFlags.RichText))
            {
                OutStream.Write("</p>");
            }

            OutStream.Write(NewLine);
        }


        /// <summary>
        /// Outputs an exception's message and stacktrace
        /// </summary>
        /// <param name="ex">Raised exception</param>
        /// <param name="diff">Time delta since logger was created</param>
        /// <param name="threadName">Caller's thread name 8B hex ID if name is not available</param>
        /// <param name="isMainThread">Is the caller the thread that instantiated the logger?</param>
        public void Exception(Exception ex, TimeSpan diff, string threadName, bool isMainThread)
        {
            var callStack = new StackTrace(fNeedFileInfo: LogLevel >= LogLevel.Debug);
            var callerStack = GetCallerFrame();
            var callerMethod = callerStack.GetMethod();

            var exceptionLocation = "in an unknown file, unknown line";
            if (null != callerMethod && null != callerMethod.DeclaringType)
            {
                exceptionLocation =
                    $"at {callerMethod.DeclaringType.Name}.{callerMethod.Name}:{callerStack.GetFileLineNumber()}";
            }

            Log($"An error has been raised {exceptionLocation}: {ex.Message}\nCall stack:\n{callStack}", LogLevel.Error,
                diff, threadName, isMainThread);
        }


        /// <summary>
        /// Logs the provided message with the provided level
        /// </summary>
        /// <param name="message">Message to be output</param>
        /// <param name="level">Level to output message in</param>
        /// <param name="diff">Time delta since logger was created</param>
        /// <param name="threadName">Caller's thread name 8B hex ID if name is not available</param>
        /// <param name="isMainThread">Is the caller the thread that instantiated the logger?</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown in case level is unknown</exception>
        /// <exception cref="EndOfStreamException">Thrown in case the logger is not ready yet</exception>
        public virtual void Log(string message, LogLevel level, TimeSpan diff, string threadName, bool isMainThread)
        {
            // Invalid log level
            if (!Enum.IsDefined(typeof(LogLevel), level))
            {
                throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }

            // Logger not ready yet
            if (!Ready)
            {
                throw new EndOfStreamException("Logger is not ready!");
            }

            // Ignore messages we shouldn't be saving
            if (LogLevel < level)
            {
                return;
            }

            PrintPreMessage(level, diff, threadName, isMainThread);
            OutStream.Write(message);
            PrintPostMessage();
        }

        /// <summary>
        /// Flushes the output stream
        /// </summary>
        /// <exception cref="EndOfStreamException">Thrown in case the logger's not ready yet</exception>
        public void Flush()
        {
            if (!Ready)
            {
                throw new EndOfStreamException("Logger is not ready!");
            }

            OutStream.Flush();
        }


        /// <summary>
        /// Closes the logger, rendering it unusable until reinstantiated
        /// </summary>
        public virtual void Close()
        {
            OutStream?.Flush();
            OutStream?.Close();
            OutStream = null;
        }
    }
}