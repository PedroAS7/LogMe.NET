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
using System.Text;

namespace LogMe
{
    /// <summary>
    /// Class containing string extensions
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// An arbitrarily-big length to impose a limit because, let's be honest, trying to reach the language's
        /// limit of 2GB for a string will most often than not result in an OutOfMemory exception being thrown
        /// </summary>
        public const int MaxRepeatedLength = 100 * 1024 * 1024;
        
        /// <summary>
        /// Repeats the provided string a specific number of times
        /// </summary>
        /// <param name="str">String to repeat</param>
        /// <param name="count">Number of times to repeat it</param>
        /// <returns>Repeated string</returns>
        /// <exception cref="OverflowException">Thrown if repeated string is over 100MiB</exception>
        public static string Repeat(this string str, uint count)
        {
            var finalLen = (uint)str.Length * count;
            var sb = new StringBuilder();

            // Sanity checks - empty repetition
            if (count == 0)
            {
                return string.Empty;
            }
            
            // Sanity checks - string to long
            if (finalLen > MaxRepeatedLength)
            {
                throw new OverflowException("Repeated string is too long");
            }
            
            // Reserve this many bytes
            sb.EnsureCapacity((int)finalLen);
            
            // Repeat string
            for (uint i = 0; i < count; ++i)
            {
                sb.Append(str);
            }

            return sb.ToString();
        }
    }
}
