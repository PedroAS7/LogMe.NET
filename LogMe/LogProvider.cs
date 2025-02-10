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
using System.Diagnostics;
using System.IO;

namespace LogMe
{
    public abstract class LogProvider
    {
        private static readonly string CurrentClassName = typeof(LogProvider).FullName ?? "";
        private static readonly string LogMgrClassName = typeof(Logger).FullName ?? "";
        
        protected readonly DateTime LoggerStart;
        protected TextWriter OutStream;

        public string Name { get; }
        public LogLevel LogLevel { get; }
        public LoggerFlags Flags { get; }
        public uint DaysPadding { get; protected set; } = 5;
        public uint SecsPadding { get; protected set; } = 5;
        public uint MsPadding { get; protected set; } = 3;
        
        public bool Ready => OutStream != null;

        /// <summary>
        /// Logger constructor
        /// </summary>
        /// <param name="name">Logger name</param>
        /// <param name="level">Minimum acceptable message level</param>
        /// <param name="flags">Logger flags</param>
        /// <param name="outStream">Output stream</param>
        protected LogProvider(string name,
                              LogLevel level,
                              LoggerFlags flags,
                              TextWriter outStream = null)
        {
            Name = name;
            Flags = flags;
            LogLevel = level;
            LoggerStart = DateTime.Now;
            OutStream = outStream;
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
            var totalMs = diff.Milliseconds ;

            // Formatting variables
            var showDays = dayCount > 0;

            var calcDaysPadding = DaysPadding;
            var calcSecsPadding = SecsPadding;
            var calcMsPadding = MsPadding;
            
            if (showDays)
            {
                calcDaysPadding -= 1; // for the 'd' indicator
                calcDaysPadding -= (uint)dayCount.ToString().Length;
                
                // Underflow
                if (calcDaysPadding > DaysPadding)
                {
                    calcDaysPadding = 0;
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
            var callStack = new StackTrace(fNeedFileInfo: LogLevel >= LogLevel.Debug);

            // If we have information about where we are
            if (string.IsNullOrEmpty(CurrentClassName))
                return new StackFrame();

            // Search for the first non-logger-related frame
            var frameArray = callStack.GetFrames();

            // Search through all the frames until we find one that matches a potential caller frame
            foreach (var frame in frameArray)
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
            const uint maxSize = 60;
            const string retVal = "[ {pF}{f} ]";

            var callerFrame = GetCallerFrame();
            var methodBase = callerFrame.GetMethod();

            // If we didn't find a suitable frame to show
            if (null == methodBase || null == methodBase.DeclaringType)
            {
                return retVal.Replace("{f}", "?.?:?")
                             .Replace("{pF}", " ".Repeat((uint)(maxSize - retVal.Length + 4))); // + 4 because of the {pM} placeholder
            }

            // Build the string containing the caller's information
            var frameInfo = $"{methodBase.DeclaringType.Name}.{methodBase.Name}():{callerFrame.GetFileLineNumber()}";
            if(frameInfo.Length > maxSize - 4)
            {
                frameInfo = $"...{frameInfo.Substring(frameInfo.Length - (int)maxSize + 3)}";
            }

            // Format the returned string
            return retVal.Replace("{f}", frameInfo)
                         .Replace("{pF}", " ".Repeat((uint)(maxSize - frameInfo.Length)));
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
                OutStream.Write($"<p style=\"background-color:{RichTextDb.LevelToBackground[messageLevel]}; color:{RichTextDb.LevelToForeground[messageLevel]}; width:100%; margin:0, padding:5\">");
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

            OutStream.Write($"[{RichTextDb.LevelToPrefix[messageLevel]}] ");
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

            OutStream.Write('\n');
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
                exceptionLocation = $"at {callerMethod.DeclaringType.Name}.{callerMethod.Name}:{callerStack.GetFileLineNumber()}";
            }      

            Log($"An error has been raised {exceptionLocation}: {ex.Message}\nCall stack:\n{callStack}", LogLevel.Error, diff, threadName, isMainThread);
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
