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
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LogMe
{
    /// <summary>
    /// Class managing all loggers and dispatching messages onto them
    /// This class makes sure calls to loggers' functions are thread-safe
    /// </summary>
    public class Logger
    {
        /// <summary>
        /// Linked list containing all the registered provders
        /// </summary>
        private readonly LinkedList<LogProvider> _providersList = new();

        /// <summary>
        /// Logger's creation date. Used to show timestamps in logs
        /// </summary>
        private readonly DateTime _creationDateTime = DateTime.Now;

        /// <summary>
        /// Main thread's ID
        /// </summary>
        private readonly int _mainThreadId = Environment.CurrentManagedThreadId;
        
        /// <summary>
        /// Returns all the registered log providers' names
        /// </summary>
        public string[] ProvidersNames
        {
            get
            {
                lock (_providersList)
                {
                    var providersNames = new string[_providersList.Count];

                    var i = 0;
                    foreach (var provider in _providersList)
                    {
                        providersNames[i] = provider.Name;
                        ++i;
                    }
                    return providersNames;
                }
            }
        }

        /// <summary>
        /// Class destructor. Flushes all loggers and clears the loggers list
        /// </summary>
        ~Logger()
        {
            Close();
        }

        
        /// <summary>
        /// Checks whether a provider with provided name is already registered or not
        /// </summary>
        /// <param name="name">Log provider name</param>
        /// <returns>True if registered, false if not</returns>
        private bool HasProvider(string name)
        {
            lock (_providersList)
            {
                return _providersList.Any(l => l.Name == name);
            }
        }


        /// <summary>
        /// Logs the provided message with the provided log level
        /// Empty messages are simply ignored
        /// </summary>
        /// <remarks>Call is thread-safe</remarks>
        /// <param name="message">Message to output</param>
        /// <param name="level">Message log level</param>
        private void Log(string message, LogLevel level)
        {
            // Empty message? Ignore
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }
            
            var isMainThread = Environment.CurrentManagedThreadId == _mainThreadId;
            var threadName = Thread.CurrentThread.Name ?? Environment.CurrentManagedThreadId.ToString("x8");

            lock (_providersList)
            {
                // Calculate log timestamp. Must be coherent to log time, so we do it inside the locked section
                var diff = DateTime.Now - _creationDateTime;
                
                foreach (var provider in _providersList)
                {
                    provider.Log(message, level, diff, threadName, isMainThread);
                }
            }
        }


        /// <summary>
        /// Forcefully flushes and closes all the providers
        /// </summary>
        public void Close()
        {
            lock (_providersList)
            {
                // Create copy before iterating
                foreach (var provider in _providersList.ToArray())
                {
                    RemoveProvider(provider);
                }
            }
        }


        /// <summary>
        /// Registers a new log provider
        /// </summary>
        /// <param name="logProvider">Log provider to register. Name must be at least one non-whitespace character</param>
        /// <exception cref="ArgumentException">Thrown if another provider has the same name, or provider's name is invalid</exception>
        /// <exception cref="ArgumentNullException">Thrown if provider is null</exception>
        public void AddProvider(LogProvider logProvider)
        {
            if (null == logProvider)
            {
                throw new ArgumentNullException(nameof(logProvider));
            }
            
            lock (_providersList)
            {
                if (string.IsNullOrWhiteSpace(logProvider.Name))
                {
                    throw new ArgumentException("Non-null and non-whitespace log provider name is required.");
                }
                
                if (HasProvider(logProvider.Name))
                {
                    throw new ArgumentException($"A logger with the name {logProvider.Name} already exists!");   
                }
                
                _providersList.AddLast(logProvider);
            }
        }


        /// <summary>
        /// Removes a log provider from this manager. Searches by provider name
        /// </summary>
        /// <param name="providerName">Log provider name to search for</param>
        /// <exception cref="ArgumentException">Thrown if no provider with provided name exists, or argument is null</exception>
        /// <exception cref="ArgumentNullException">Thrown if provider is null</exception>
        public void RemoveProvider(string providerName)
        {
            if (null == providerName)
            {
                throw new ArgumentNullException(nameof(providerName));
            }
            
            lock (_providersList)
            {
                if (!HasProvider(providerName))
                {
                    throw new ArgumentException($"No logger with named {providerName} exists!");   
                }

                var provider = _providersList.First(l => l.Name == providerName);
                provider.Close();
                _providersList.Remove(provider);
            }
        }


        /// <summary>
        /// Removes a log provider from this manager. Searches by provider instance
        /// </summary>
        /// <param name="logProvider">Log provider to remove</param>
        /// <exception cref="ArgumentException">Thrown if no provider matches provided instance</exception>
        /// <exception cref="ArgumentNullException">Thrown if provider is null</exception>
        public void RemoveProvider(LogProvider logProvider)
        {
            if (null == logProvider)
            {
                throw new ArgumentNullException(nameof(logProvider));
            }
            
            RemoveProvider(logProvider.Name);
        }
        
        
        /// <summary>
        /// Outputs an error message to all registered loggers
        /// </summary>
        /// <remarks>Call is thread-safe</remarks>
        /// <param name="message">Message to output</param>
        public void Error(string message) => Log(message, LogLevel.Error);

        
        /// <summary>
        /// Outputs a warning message to all registered log providers
        /// </summary>
        /// <remarks>Call is thread-safe</remarks>
        /// <param name="message">Message to output</param>
        public void Warning(string message) => Log(message, LogLevel.Warning);

        
        /// <summary>
        /// Outputs an informational message to all registered log providers
        /// </summary>
        /// <remarks>Call is thread-safe</remarks>
        /// <param name="message">Message to output</param>
        public void Info(string message) => Log(message, LogLevel.Info);

        
        /// <summary>
        /// Outputs a debug message to all registered log providers
        /// </summary>
        /// <remarks>Call is thread-safe</remarks>
        /// <param name="message">Message to output</param>
        public void Debug(string message) => Log(message, LogLevel.Debug);

        
        /// <summary>
        /// Outputs a trace message to all registered log providers
        /// </summary>
        /// <remarks>Call is thread-safe</remarks>
        /// <param name="message">Message to output</param>
        public void Trace(string message) => Log(message, LogLevel.Trace);


        /// <summary>
        /// Logs an exception to all registered log providers
        /// </summary>
        /// <remarks>Call is thread-safe</remarks>
        /// <param name="ex">Exception to log. If null, a generic exception is created</param>
        public void Exception(Exception ex)
        {
            if (null == ex)
            {
                ex = new Exception("No exception message provided.");
            }
            
            lock (_providersList)
            {
                // Calculate log timestamp. Must be coherent to log time, so we do it inside the locked section
                var diff = DateTime.Now - _creationDateTime;
                
                foreach (var provider in _providersList)
                {
                    var isMainThread = Environment.CurrentManagedThreadId == _mainThreadId;
                    var threadName = Thread.CurrentThread.Name ?? Environment.CurrentManagedThreadId.ToString("x8");
                    provider.Exception(ex, diff, threadName, isMainThread);
                }   
            }
        }

        /// <summary>
        /// Flushes all log providers
        /// </summary>
        public void Flush()
        {
            lock (_providersList)
            {
                foreach (var provider in _providersList)
                {
                    provider.Flush();
                }   
            }
        }
    }
}
