/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using RtfPipe;

namespace Chummer
{
    public static class StringExtensions
    {
        public static bool IsEmptyGuid(this string strInput)
        {
            return string.Equals(strInput, Utils.GuidEmptyString, StringComparison.OrdinalIgnoreCase);
        }

        public static char[] ToPooledCharArray(this string strInput, out int intLength)
        {
            intLength = strInput.Length;
            char[] achrReturn = ArrayPool<char>.Shared.Rent(intLength);
            try
            {
                if (intLength > 0)
                {
                    int intMemoryLength = intLength * sizeof(char);
                    unsafe
                    {
                        fixed (char* smem = strInput)
                        fixed (char* dmem = achrReturn)
                        {
                            Buffer.MemoryCopy((byte*)smem, (byte*)dmem, intMemoryLength, intMemoryLength);
                        }
                    }
                }

                return achrReturn;
            }
            catch
            {
                ArrayPool<char>.Shared.Return(achrReturn);
                throw;
            }
        }

        /// <summary>
        /// Version of string.Concat that is faster than the built-in version for shorter strings (including for string arrays) because it uses stackalloc, but needs to enumerate over the input strings twice and so needs a collection as an input.
        /// </summary>
        public static string ConcatFast(params string[] lstStrings)
        {
            return ConcatFast(Array.AsReadOnly(lstStrings));
        }

        /// <summary>
        /// Version of string.Concat that is faster than the built-in version for shorter strings (including for string arrays) because it uses stackalloc, but needs to enumerate over the input strings twice and so needs a collection as an input.
        /// </summary>
        public static string ConcatFast(IReadOnlyCollection<string> lstStrings)
        {
            int intStringsCount = lstStrings.Count;
            if (intStringsCount == 0)
                return string.Empty;
            if (intStringsCount == 1)
                return lstStrings.ElementAt(0);
            int intTotalLength = 0;
            using (IEnumerator<string> objEnumerator = lstStrings.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    string strLoop = objEnumerator.Current;
                    if (!string.IsNullOrEmpty(strLoop) && (intTotalLength += strLoop.Length) > GlobalSettings.MaxStackLimit16BitTypes)
                        return string.Concat(lstStrings);
                }
                objEnumerator.Reset();
                // Stackalloc is faster than a heap-allocated array, but string constructor requires use of unsafe context because there are no overloads for Span<char>
                unsafe
                {
                    char* achrNewChars = stackalloc char[intTotalLength];
                    // What we're doing here is copying the string-as-CharArray via memory blocks into a new CharArray
                    int intCurrent = 0;
                    while (objEnumerator.MoveNext())
                    {
                        string strLoop = objEnumerator.Current;
                        int intLoopLength = strLoop?.Length ?? 0;
                        if (intLoopLength > 0)
                        {
                            fixed (char* src = strLoop)
                            {
                                Buffer.MemoryCopy((byte*)src, (byte*)(achrNewChars + intCurrent), (intTotalLength - intCurrent) * sizeof(char), intLoopLength * sizeof(char));
                            }
                            intCurrent += intLoopLength;
                        }
                    }

                    // ... then we create a new string from the new CharArray (using intCurrent just in case)
                    return new string(achrNewChars, 0, intCurrent);
                }
            }
        }

        /// <summary>
        /// Version of string.Concat that is faster than the built-in version for shorter strings (including for string arrays) because it uses stackalloc.
        /// </summary>
        public static string ConcatFast(string strArg0, string strArg1)
        {
            int intTotalLength = (strArg0?.Length ?? 0) + (strArg1?.Length ?? 0);
            if (intTotalLength > GlobalSettings.MaxStackLimit16BitTypes)
                return string.Concat(strArg0, strArg1);
            // Stackalloc is faster than a heap-allocated array, but string constructor requires use of unsafe context because there are no overloads for Span<char>
            unsafe
            {
                char* achrNewChars = stackalloc char[intTotalLength];
                // What we're doing here is copying the string-as-CharArray via memory blocks into a new CharArray
                int intCurrent = 0;
                int intLoopLength = strArg0?.Length ?? 0;
                if (intLoopLength > 0)
                {
                    fixed (char* src = strArg0)
                    {
                        Buffer.MemoryCopy((byte*)src, (byte*)achrNewChars, intTotalLength * sizeof(char), intLoopLength * sizeof(char));
                    }
                    intCurrent = intLoopLength;
                }
                intLoopLength = strArg1?.Length ?? 0;
                if (intLoopLength > 0)
                {
                    fixed (char* src = strArg1)
                    {
                        Buffer.MemoryCopy((byte*)src, (byte*)(achrNewChars + intCurrent), (intTotalLength - intCurrent) * sizeof(char), intLoopLength * sizeof(char));
                    }
                    intCurrent += intLoopLength;
                }

                // ... then we create a new string from the new CharArray (using intCurrent just in case)
                return new string(achrNewChars, 0, intCurrent);
            }
        }

        /// <summary>
        /// Version of string.Concat that is faster than the built-in version for shorter strings (including for string arrays) because it uses stackalloc.
        /// </summary>
        public static string ConcatFast(string strArg0, string strArg1, string strArg2)
        {
            int intTotalLength = (strArg0?.Length ?? 0) + (strArg1?.Length ?? 0) + (strArg2?.Length ?? 0);
            if (intTotalLength > GlobalSettings.MaxStackLimit16BitTypes)
                return string.Concat(strArg0, strArg1, strArg2);
            // Stackalloc is faster than a heap-allocated array, but string constructor requires use of unsafe context because there are no overloads for Span<char>
            unsafe
            {
                char* achrNewChars = stackalloc char[intTotalLength];
                // What we're doing here is copying the string-as-CharArray via memory blocks into a new CharArray
                int intCurrent = 0;
                int intLoopLength = strArg0?.Length ?? 0;
                if (intLoopLength > 0)
                {
                    fixed (char* src = strArg0)
                    {
                        Buffer.MemoryCopy((byte*)src, (byte*)achrNewChars, intTotalLength * sizeof(char), intLoopLength * sizeof(char));
                    }
                    intCurrent = intLoopLength;
                }
                intLoopLength = strArg1?.Length ?? 0;
                if (intLoopLength > 0)
                {
                    fixed (char* src = strArg1)
                    {
                        Buffer.MemoryCopy((byte*)src, (byte*)(achrNewChars + intCurrent), (intTotalLength - intCurrent) * sizeof(char), intLoopLength * sizeof(char));
                    }
                    intCurrent += intLoopLength;
                }
                intLoopLength = strArg2?.Length ?? 0;
                if (intLoopLength > 0)
                {
                    fixed (char* src = strArg2)
                    {
                        Buffer.MemoryCopy((byte*)src, (byte*)(achrNewChars + intCurrent), (intTotalLength - intCurrent) * sizeof(char), intLoopLength * sizeof(char));
                    }
                    intCurrent += intLoopLength;
                }

                // ... then we create a new string from the new CharArray (using intCurrent just in case)
                return new string(achrNewChars, 0, intCurrent);
            }
        }

        /// <summary>
        /// Version of string.Concat that is faster than the built-in version for shorter strings (including for string arrays) because it uses stackalloc.
        /// </summary>
        public static string ConcatFast(string strArg0, string strArg1, string strArg2, string strArg3)
        {
            int intTotalLength = (strArg0?.Length ?? 0) + (strArg1?.Length ?? 0) + (strArg2?.Length ?? 0) + (strArg3?.Length ?? 0);
            if (intTotalLength > GlobalSettings.MaxStackLimit16BitTypes)
                return string.Concat(strArg0, strArg1, strArg2, strArg3);
            // Stackalloc is faster than a heap-allocated array, but string constructor requires use of unsafe context because there are no overloads for Span<char>
            unsafe
            {
                char* achrNewChars = stackalloc char[intTotalLength];
                // What we're doing here is copying the string-as-CharArray via memory blocks into a new CharArray
                int intCurrent = 0;
                int intLoopLength = strArg0?.Length ?? 0;
                if (intLoopLength > 0)
                {
                    fixed (char* src = strArg0)
                    {
                        Buffer.MemoryCopy((byte*)src, (byte*)achrNewChars, intTotalLength * sizeof(char), intLoopLength * sizeof(char));
                    }
                    intCurrent = intLoopLength;
                }
                intLoopLength = strArg1?.Length ?? 0;
                if (intLoopLength > 0)
                {
                    fixed (char* src = strArg1)
                    {
                        Buffer.MemoryCopy((byte*)src, (byte*)(achrNewChars + intCurrent), (intTotalLength - intCurrent) * sizeof(char), intLoopLength * sizeof(char));
                    }
                    intCurrent += intLoopLength;
                }
                intLoopLength = strArg2?.Length ?? 0;
                if (intLoopLength > 0)
                {
                    fixed (char* src = strArg2)
                    {
                        Buffer.MemoryCopy((byte*)src, (byte*)(achrNewChars + intCurrent), (intTotalLength - intCurrent) * sizeof(char), intLoopLength * sizeof(char));
                    }
                    intCurrent += intLoopLength;
                }
                intLoopLength = strArg3?.Length ?? 0;
                if (intLoopLength > 0)
                {
                    fixed (char* src = strArg3)
                    {
                        Buffer.MemoryCopy((byte*)src, (byte*)(achrNewChars + intCurrent), (intTotalLength - intCurrent) * sizeof(char), intLoopLength * sizeof(char));
                    }
                    intCurrent += intLoopLength;
                }

                // ... then we create a new string from the new CharArray (using intCurrent just in case)
                return new string(achrNewChars, 0, intCurrent);
            }
        }

        /// <summary>
        /// Version of string.Concat that is faster than the built-in version for shorter strings (including for string arrays) because it uses stackalloc.
        /// </summary>
        public static string ConcatFast(string strArg0, string strArg1, string strArg2, string strArg3, string strArg4)
        {
            int intTotalLength = (strArg0?.Length ?? 0) + (strArg1?.Length ?? 0) + (strArg2?.Length ?? 0) + (strArg3?.Length ?? 0);
            if (intTotalLength > GlobalSettings.MaxStackLimit16BitTypes)
                return string.Concat(string.Concat(strArg0, strArg1, strArg2, strArg3), strArg4);
            intTotalLength += (strArg4?.Length ?? 0);
            if (intTotalLength > GlobalSettings.MaxStackLimit16BitTypes)
                return string.Concat(ConcatFast(strArg0, strArg1, strArg2, strArg3), strArg4);
            // Stackalloc is faster than a heap-allocated array, but string constructor requires use of unsafe context because there are no overloads for Span<char>
            unsafe
            {
                char* achrNewChars = stackalloc char[intTotalLength];
                // What we're doing here is copying the string-as-CharArray via memory blocks into a new CharArray
                int intCurrent = 0;
                int intLoopLength = strArg0?.Length ?? 0;
                if (intLoopLength > 0)
                {
                    fixed (char* src = strArg0)
                    {
                        Buffer.MemoryCopy((byte*)src, (byte*)achrNewChars, intTotalLength * sizeof(char), intLoopLength * sizeof(char));
                    }
                    intCurrent = intLoopLength;
                }
                intLoopLength = strArg1?.Length ?? 0;
                if (intLoopLength > 0)
                {
                    fixed (char* src = strArg1)
                    {
                        Buffer.MemoryCopy((byte*)src, (byte*)(achrNewChars + intCurrent), (intTotalLength - intCurrent) * sizeof(char), intLoopLength * sizeof(char));
                    }
                    intCurrent += intLoopLength;
                }
                intLoopLength = strArg2?.Length ?? 0;
                if (intLoopLength > 0)
                {
                    fixed (char* src = strArg2)
                    {
                        Buffer.MemoryCopy((byte*)src, (byte*)(achrNewChars + intCurrent), (intTotalLength - intCurrent) * sizeof(char), intLoopLength * sizeof(char));
                    }
                    intCurrent += intLoopLength;
                }
                intLoopLength = strArg3?.Length ?? 0;
                if (intLoopLength > 0)
                {
                    fixed (char* src = strArg3)
                    {
                        Buffer.MemoryCopy((byte*)src, (byte*)(achrNewChars + intCurrent), (intTotalLength - intCurrent) * sizeof(char), intLoopLength * sizeof(char));
                    }
                    intCurrent += intLoopLength;
                }
                intLoopLength = strArg4?.Length ?? 0;
                if (intLoopLength > 0)
                {
                    fixed (char* src = strArg4)
                    {
                        Buffer.MemoryCopy((byte*)src, (byte*)(achrNewChars + intCurrent), (intTotalLength - intCurrent) * sizeof(char), intLoopLength * sizeof(char));
                    }
                    intCurrent += intLoopLength;
                }

                // ... then we create a new string from the new CharArray (using intCurrent just in case)
                return new string(achrNewChars, 0, intCurrent);
            }
        }

        /// <summary>
        /// Version of string.Concat that is faster than the built-in version for shorter strings (including for string arrays) because it uses stackalloc.
        /// </summary>
        public static string ConcatFast(string strArg0, string strArg1, string strArg2, string strArg3, string strArg4, string strArg5)
        {
            int intTotalLength = (strArg0?.Length ?? 0) + (strArg1?.Length ?? 0) + (strArg2?.Length ?? 0) + (strArg3?.Length ?? 0);
            if (intTotalLength > GlobalSettings.MaxStackLimit16BitTypes)
                return ConcatFast(string.Concat(strArg0, strArg1, strArg2, strArg3), strArg4, strArg5);
            intTotalLength += (strArg4?.Length ?? 0) + (strArg5?.Length ?? 0);
            if (intTotalLength > GlobalSettings.MaxStackLimit16BitTypes)
                return string.Concat(ConcatFast(strArg0, strArg1, strArg2, strArg3), strArg4, strArg5);
            // Stackalloc is faster than a heap-allocated array, but string constructor requires use of unsafe context because there are no overloads for Span<char>
            unsafe
            {
                char* achrNewChars = stackalloc char[intTotalLength];
                // What we're doing here is copying the string-as-CharArray via memory blocks into a new CharArray
                int intCurrent = 0;
                int intLoopLength = strArg0?.Length ?? 0;
                if (intLoopLength > 0)
                {
                    fixed (char* src = strArg0)
                    {
                        Buffer.MemoryCopy((byte*)src, (byte*)achrNewChars, intTotalLength * sizeof(char), intLoopLength * sizeof(char));
                    }
                    intCurrent = intLoopLength;
                }
                intLoopLength = strArg1?.Length ?? 0;
                if (intLoopLength > 0)
                {
                    fixed (char* src = strArg1)
                    {
                        Buffer.MemoryCopy((byte*)src, (byte*)(achrNewChars + intCurrent), (intTotalLength - intCurrent) * sizeof(char), intLoopLength * sizeof(char));
                    }
                    intCurrent += intLoopLength;
                }
                intLoopLength = strArg2?.Length ?? 0;
                if (intLoopLength > 0)
                {
                    fixed (char* src = strArg2)
                    {
                        Buffer.MemoryCopy((byte*)src, (byte*)(achrNewChars + intCurrent), (intTotalLength - intCurrent) * sizeof(char), intLoopLength * sizeof(char));
                    }
                    intCurrent += intLoopLength;
                }
                intLoopLength = strArg3?.Length ?? 0;
                if (intLoopLength > 0)
                {
                    fixed (char* src = strArg3)
                    {
                        Buffer.MemoryCopy((byte*)src, (byte*)(achrNewChars + intCurrent), (intTotalLength - intCurrent) * sizeof(char), intLoopLength * sizeof(char));
                    }
                    intCurrent += intLoopLength;
                }
                intLoopLength = strArg4?.Length ?? 0;
                if (intLoopLength > 0)
                {
                    fixed (char* src = strArg4)
                    {
                        Buffer.MemoryCopy((byte*)src, (byte*)(achrNewChars + intCurrent), (intTotalLength - intCurrent) * sizeof(char), intLoopLength * sizeof(char));
                    }
                    intCurrent += intLoopLength;
                }
                intLoopLength = strArg5?.Length ?? 0;
                if (intLoopLength > 0)
                {
                    fixed (char* src = strArg5)
                    {
                        Buffer.MemoryCopy((byte*)src, (byte*)(achrNewChars + intCurrent), (intTotalLength - intCurrent) * sizeof(char), intLoopLength * sizeof(char));
                    }
                    intCurrent += intLoopLength;
                }

                // ... then we create a new string from the new CharArray (using intCurrent just in case)
                return new string(achrNewChars, 0, intCurrent);
            }
        }

        /// <summary>
        /// Version of string.Concat that is faster than the built-in version for shorter strings (including for string arrays) because it uses stackalloc.
        /// </summary>
        public static string ConcatFast(string strArg0, string strArg1, string strArg2, string strArg3, string strArg4, string strArg5, string strArg6)
        {
            int intTotalLength = (strArg0?.Length ?? 0) + (strArg1?.Length ?? 0) + (strArg2?.Length ?? 0) + (strArg3?.Length ?? 0);
            if (intTotalLength > GlobalSettings.MaxStackLimit16BitTypes)
                return ConcatFast(string.Concat(strArg0, strArg1, strArg2, strArg3), strArg4, strArg5, strArg6);
            intTotalLength += (strArg4?.Length ?? 0) + (strArg5?.Length ?? 0) + (strArg6?.Length ?? 0);
            if (intTotalLength > GlobalSettings.MaxStackLimit16BitTypes)
                return string.Concat(ConcatFast(strArg0, strArg1, strArg2, strArg3), strArg4, strArg5, strArg6);
            // Stackalloc is faster than a heap-allocated array, but string constructor requires use of unsafe context because there are no overloads for Span<char>
            unsafe
            {
                char* achrNewChars = stackalloc char[intTotalLength];
                // What we're doing here is copying the string-as-CharArray via memory blocks into a new CharArray
                int intCurrent = 0;
                int intLoopLength = strArg0?.Length ?? 0;
                if (intLoopLength > 0)
                {
                    fixed (char* src = strArg0)
                    {
                        Buffer.MemoryCopy((byte*)src, (byte*)achrNewChars, intTotalLength * sizeof(char), intLoopLength * sizeof(char));
                    }
                    intCurrent = intLoopLength;
                }
                intLoopLength = strArg1?.Length ?? 0;
                if (intLoopLength > 0)
                {
                    fixed (char* src = strArg1)
                    {
                        Buffer.MemoryCopy((byte*)src, (byte*)(achrNewChars + intCurrent), (intTotalLength - intCurrent) * sizeof(char), intLoopLength * sizeof(char));
                    }
                    intCurrent += intLoopLength;
                }
                intLoopLength = strArg2?.Length ?? 0;
                if (intLoopLength > 0)
                {
                    fixed (char* src = strArg2)
                    {
                        Buffer.MemoryCopy((byte*)src, (byte*)(achrNewChars + intCurrent), (intTotalLength - intCurrent) * sizeof(char), intLoopLength * sizeof(char));
                    }
                    intCurrent += intLoopLength;
                }
                intLoopLength = strArg3?.Length ?? 0;
                if (intLoopLength > 0)
                {
                    fixed (char* src = strArg3)
                    {
                        Buffer.MemoryCopy((byte*)src, (byte*)(achrNewChars + intCurrent), (intTotalLength - intCurrent) * sizeof(char), intLoopLength * sizeof(char));
                    }
                    intCurrent += intLoopLength;
                }
                intLoopLength = strArg4?.Length ?? 0;
                if (intLoopLength > 0)
                {
                    fixed (char* src = strArg4)
                    {
                        Buffer.MemoryCopy((byte*)src, (byte*)(achrNewChars + intCurrent), (intTotalLength - intCurrent) * sizeof(char), intLoopLength * sizeof(char));
                    }
                    intCurrent += intLoopLength;
                }
                intLoopLength = strArg5?.Length ?? 0;
                if (intLoopLength > 0)
                {
                    fixed (char* src = strArg5)
                    {
                        Buffer.MemoryCopy((byte*)src, (byte*)(achrNewChars + intCurrent), (intTotalLength - intCurrent) * sizeof(char), intLoopLength * sizeof(char));
                    }
                    intCurrent += intLoopLength;
                }
                intLoopLength = strArg6?.Length ?? 0;
                if (intLoopLength > 0)
                {
                    fixed (char* src = strArg6)
                    {
                        Buffer.MemoryCopy((byte*)src, (byte*)(achrNewChars + intCurrent), (intTotalLength - intCurrent) * sizeof(char), intLoopLength * sizeof(char));
                    }
                    intCurrent += intLoopLength;
                }

                // ... then we create a new string from the new CharArray (using intCurrent just in case)
                return new string(achrNewChars, 0, intCurrent);
            }
        }

        /// <summary>
        /// Version of string.Join that is faster for shorter strings because it uses stackalloc instead of StringBuilder, but needs to enumerate over the input strings twice and so needs a collection as an input.
        /// </summary>
        public static string JoinFast(string strSeparator, IReadOnlyCollection<string> lstStrings)
        {
            int intSeparatorLength = strSeparator.Length;
            if (intSeparatorLength == 0)
                return ConcatFast(lstStrings);
            int intStringsCount = lstStrings.Count;
            if (intStringsCount == 0)
                return string.Empty;
            if (intStringsCount == 1)
                return lstStrings.ElementAt(0);
            int intTotalLength = (intStringsCount - 1) * intSeparatorLength;
            using (IEnumerator<string> objEnumerator = lstStrings.GetEnumerator())
            {
                while (objEnumerator.MoveNext())
                {
                    string strLoop = objEnumerator.Current;
                    if ((intTotalLength += strLoop.Length + intSeparatorLength) > GlobalSettings.MaxStackLimit16BitTypes)
                        return string.Join(strSeparator, lstStrings);
                }
                objEnumerator.Reset();
                if (objEnumerator.MoveNext())
                {
                    // Stackalloc is faster than a heap-allocated array, but string constructor requires use of unsafe context because there are no overloads for Span<char>
                    unsafe
                    {
                        char* achrNewChars = stackalloc char[intTotalLength];
                        // What we're doing here is copying the string-as-CharArray via memory blocks into a new CharArray
                        int intCurrent = 0;
                        int intSeparatorByteLength = intSeparatorLength * sizeof(char);

                        fixed (char* sep = strSeparator)
                        {
                            string strLoop = objEnumerator.Current;
                            int intLoopLength = strLoop?.Length ?? 0;
                            if (intLoopLength > 0)
                            {
                                fixed (char* src = strLoop)
                                {
                                    Buffer.MemoryCopy((byte*)src, (byte*)achrNewChars, intTotalLength * sizeof(char), intLoopLength * sizeof(char));
                                }
                                intCurrent = intLoopLength;
                            }
                            else
                            {
                                while (objEnumerator.MoveNext())
                                {
                                    strLoop = objEnumerator.Current;
                                    intLoopLength = strLoop?.Length ?? 0;
                                    if (intLoopLength > 0)
                                    {
                                        fixed (char* src = strLoop)
                                        {
                                            Buffer.MemoryCopy((byte*)src, (byte*)achrNewChars, intTotalLength * sizeof(char), intLoopLength * sizeof(char));
                                        }
                                        intCurrent = intLoopLength;
                                        break;
                                    }
                                }
                            }
                            while (objEnumerator.MoveNext())
                            {
                                strLoop = objEnumerator.Current;
                                intLoopLength = strLoop?.Length ?? 0;
                                if (intLoopLength > 0)
                                {
                                    Buffer.MemoryCopy((byte*)sep, (byte*)(achrNewChars + intCurrent), (intTotalLength - intCurrent) * sizeof(char), intSeparatorByteLength);
                                    intCurrent += intSeparatorLength;
                                    fixed (char* src = strLoop)
                                    {
                                        Buffer.MemoryCopy((byte*)src, (byte*)(achrNewChars + intCurrent), (intTotalLength - intCurrent) * sizeof(char), intLoopLength * sizeof(char));
                                    }
                                    intCurrent += intLoopLength;
                                }
                            }
                        }
                        // ... then we create a new string from the new CharArray (using intCurrent just in case)
                        return new string(achrNewChars, 0, intCurrent);
                    }
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Version of string.Join that is faster for shorter strings because it uses stackalloc instead of StringBuilder, but needs to enumerate over the input strings twice and so needs a collection as an input.
        /// </summary>
        public static async Task<string> JoinFastAsync(string strSeparator, IAsyncReadOnlyCollection<string> lstStrings, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intSeparatorLength = strSeparator.Length;
            if (intSeparatorLength == 0)
                return ConcatFast(lstStrings);
            int intStringsCount = lstStrings.Count;
            if (intStringsCount == 0)
                return string.Empty;
            if (intStringsCount == 1)
                return lstStrings.ElementAt(0);
            int intTotalLength = (intStringsCount - 1) * intSeparatorLength;
            IEnumerator<string> objEnumerator = await lstStrings.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                while (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    string strLoop = objEnumerator.Current;
                    if ((intTotalLength += strLoop.Length + intSeparatorLength) > GlobalSettings.MaxStackLimit16BitTypes)
                        return string.Join(strSeparator, lstStrings);
                }
                objEnumerator.Reset();
                token.ThrowIfCancellationRequested();
                if (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    // Stackalloc is faster than a heap-allocated array, but string constructor requires use of unsafe context because there are no overloads for Span<char>
                    unsafe
                    {
                        char* achrNewChars = stackalloc char[intTotalLength];
                        // What we're doing here is copying the string-as-CharArray via memory blocks into a new CharArray
                        int intCurrent = 0;
                        int intSeparatorByteLength = intSeparatorLength * sizeof(char);

                        fixed (char* sep = strSeparator)
                        {
                            string strLoop = objEnumerator.Current;
                            int intLoopLength = strLoop?.Length ?? 0;
                            if (intLoopLength > 0)
                            {
                                fixed (char* src = strLoop)
                                {
                                    Buffer.MemoryCopy((byte*)src, (byte*)achrNewChars, intTotalLength * sizeof(char), intLoopLength * sizeof(char));
                                }
                                intCurrent = intLoopLength;
                            }
                            else
                            {
                                while (objEnumerator.MoveNext())
                                {
                                    token.ThrowIfCancellationRequested();
                                    strLoop = objEnumerator.Current;
                                    intLoopLength = strLoop?.Length ?? 0;
                                    if (intLoopLength > 0)
                                    {
                                        fixed (char* src = strLoop)
                                        {
                                            Buffer.MemoryCopy((byte*)src, (byte*)achrNewChars, intTotalLength * sizeof(char), intLoopLength * sizeof(char));
                                        }
                                        intCurrent = intLoopLength;
                                        break;
                                    }
                                }
                            }
                            while (objEnumerator.MoveNext())
                            {
                                token.ThrowIfCancellationRequested();
                                strLoop = objEnumerator.Current;
                                intLoopLength = strLoop?.Length ?? 0;
                                if (intLoopLength > 0)
                                {
                                    Buffer.MemoryCopy((byte*)sep, (byte*)(achrNewChars + intCurrent), (intTotalLength - intCurrent) * sizeof(char), intSeparatorByteLength);
                                    intCurrent += intSeparatorLength;
                                    fixed (char* src = strLoop)
                                    {
                                        Buffer.MemoryCopy((byte*)src, (byte*)(achrNewChars + intCurrent), (intTotalLength - intCurrent) * sizeof(char), intLoopLength * sizeof(char));
                                    }
                                    intCurrent += intLoopLength;
                                }
                            }
                        }
                        // ... then we create a new string from the new CharArray (using intCurrent just in case)
                        return new string(achrNewChars, 0, intCurrent);
                    }
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objDisposeAsync)
                    await objDisposeAsync.DisposeAsync().ConfigureAwait(false);
                else if (objEnumerator is IDisposable objDispose)
                    objDispose.Dispose();
            }
            return string.Empty;
        }

        public static async Task<string> JoinAsync(string strSeparator, IEnumerable<Task<string>> lstStringTasks,
                                                   CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            bool blnAddSeparator = false;
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdReturn))
            {
                foreach (Task<string> tskString in lstStringTasks)
                {
                    token.ThrowIfCancellationRequested();
                    if (blnAddSeparator)
                        sbdReturn.Append(strSeparator);
                    else
                        blnAddSeparator = true;
                    token.ThrowIfCancellationRequested();
                    sbdReturn.Append(await tskString.ConfigureAwait(false));
                }

                return sbdReturn.ToString();
            }
        }

        /// <summary>
        /// Identical to string::Replace(), but the comparison for equality is custom-defined instead of always being case-sensitive Ordinal
        /// </summary>
        /// <param name="strInput">String on which to operate</param>
        /// <param name="strOldValue">Substring to replace</param>
        /// <param name="strNewValue">Substring with which <paramref name="strOldValue"/> gets replaced</param>
        /// <param name="eStringComparison">String Comparison to use when checking for identity</param>
        /// <returns>New string with all instances of <paramref name="strOldValue"/> replaced with <paramref name="strNewValue"/>, but where the equality check was custom-defined by <paramref name="eStringComparison"/></returns>
        public static string Replace(this string strInput, string strOldValue, string strNewValue,
                                     StringComparison eStringComparison)
        {
            if (string.IsNullOrEmpty(strInput) || string.IsNullOrEmpty(strOldValue))
                return strInput;
            if (strNewValue == null)
                throw new ArgumentNullException(nameof(strNewValue));
            // Built-in Replace method uses Ordinal comparison, so just defer to that if that is what we have defined
            if (eStringComparison == StringComparison.Ordinal)
                return strInput.Replace(strOldValue, strNewValue);
            // Do the check first before we do anything else so that we exit out quickly if nothing needs replacing
            int intHead = strInput.IndexOf(strOldValue, eStringComparison);
            if (intHead == -1)
                return strInput;
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdReturn))
            {
                // Buffer size is increased by 1 in addition to the length-dependent stuff in order to compensate for integer division rounding down
                int intNewCapacity = strInput.Length + 1 + Math.Max(0, strNewValue.Length - strOldValue.Length);
                if (sbdReturn.Capacity < intNewCapacity)
                    sbdReturn.Capacity = intNewCapacity;
                int intEndPositionOfLastReplace = 0;
                // intHead already set to the index of the first instance, for loop's initializer can be left empty
                for (;
                     intHead != -1;
                     intHead = strInput.IndexOf(strOldValue, intEndPositionOfLastReplace, eStringComparison))
                {
                    sbdReturn.Append(strInput, intEndPositionOfLastReplace, intHead - intEndPositionOfLastReplace)
                             .Append(strNewValue);
                    intEndPositionOfLastReplace = intHead + strOldValue.Length;
                }

                sbdReturn.Append(strInput, intEndPositionOfLastReplace, strInput.Length - intEndPositionOfLastReplace);
                return sbdReturn.ToString();
            }
        }

        /// <summary>
        /// Method to quickly remove all instances of a char from a string (much faster than using Replace() with an empty string)
        /// </summary>
        /// <param name="strInput">String on which to operate</param>
        /// <param name="chrToDelete">Character to remove</param>
        /// <returns>New string with characters removed</returns>
        public static string FastEscape(this string strInput, char chrToDelete)
        {
            if (strInput == null)
                return string.Empty;
            int intLength = strInput.Length;
            if (intLength == 0)
                return strInput;
            if (intLength > GlobalSettings.MaxStackLimit16BitTypes)
            {
                string strReturn;
                using (new FetchSafelyFromArrayPool<char>(ArrayPool<char>.Shared, intLength, out char[] achrNewChars))
                {
                    // What we're doing here is copying the string-as-CharArray char-by-char into a new CharArray, but skipping over any instance of chrToDelete...
                    int intCurrent = 0;
                    for (int i = 0; i < intLength; ++i)
                    {
                        char chrLoop = strInput[i];
                        if (chrLoop != chrToDelete)
                            achrNewChars[intCurrent++] = chrLoop;
                    }

                    // ... then we create a new string from the new CharArray, but only up to the number of characters that actually ended up getting copied
                    strReturn = new string(achrNewChars, 0, intCurrent);
                }

                return strReturn;
            }

            // Stackalloc is faster than a heap-allocated array, but string constructor requires use of unsafe context because there are no overloads for Span<char>
            unsafe
            {
                char* achrNewChars = stackalloc char[intLength];
                // What we're doing here is copying the string-as-CharArray char-by-char into a new CharArray, but skipping over any instance of chrToDelete...
                int intCurrent = 0;
                for (int i = 0; i < intLength; ++i)
                {
                    char chrLoop = strInput[i];
                    if (chrLoop != chrToDelete)
                        achrNewChars[intCurrent++] = chrLoop;
                }

                // ... then we create a new string from the new CharArray, but only up to the number of characters that actually ended up getting copied
                return new string(achrNewChars, 0, intCurrent);
            }
        }

        /// <summary>
        /// Method to quickly remove all instances of all chars in an array from a string (much faster than using a series of Replace() with an empty string)
        /// </summary>
        /// <param name="strInput">String on which to operate</param>
        /// <param name="achrToDelete">Array of characters to remove</param>
        /// <returns>New string with characters removed</returns>
        public static string FastEscape(this string strInput, params char[] achrToDelete)
        {
            if (strInput == null)
                return string.Empty;
            int intDeleteLength = achrToDelete.Length;
            if (intDeleteLength == 0)
                return strInput;
            int intLength = strInput.Length;
            if (intLength == 0)
                return strInput;
            if (intLength > GlobalSettings.MaxStackLimit16BitTypes)
            {
                string strReturn;
                using (new FetchSafelyFromArrayPool<char>(ArrayPool<char>.Shared, intLength, out char[] achrNewChars))
                {
                    // What we're doing here is copying the string-as-CharArray char-by-char into a new CharArray, but skipping over any instance of chars in achrToDelete...
                    int intCurrent = 0;
                    for (int i = 0; i < intLength; ++i)
                    {
                        bool blnDoChar = true;
                        char chrLoop = strInput[i];
                        for (int j = 0; j < intDeleteLength; ++j)
                        {
                            if (chrLoop == achrToDelete[j])
                            {
                                blnDoChar = false;
                                break;
                            }
                        }

                        if (blnDoChar)
                            achrNewChars[intCurrent++] = chrLoop;
                    }

                    // ... then we create a new string from the new CharArray, but only up to the number of characters that actually ended up getting copied
                    strReturn = new string(achrNewChars, 0, intCurrent);
                }

                return strReturn;
            }

            // Stackalloc is faster than a heap-allocated array, but string constructor requires use of unsafe context because there are no overloads for Span<char>
            unsafe
            {
                char* achrNewChars = stackalloc char[intLength];
                // What we're doing here is copying the string-as-CharArray char-by-char into a new CharArray, but skipping over any instance of chars in achrToDelete...
                int intCurrent = 0;
                for (int i = 0; i < intLength; ++i)
                {
                    bool blnDoChar = true;
                    char chrLoop = strInput[i];
                    for (int j = 0; j < intDeleteLength; ++j)
                    {
                        if (chrLoop == achrToDelete[j])
                        {
                            blnDoChar = false;
                            break;
                        }
                    }

                    if (blnDoChar)
                        achrNewChars[intCurrent++] = chrLoop;
                }

                // ... then we create a new string from the new CharArray, but only up to the number of characters that actually ended up getting copied
                return new string(achrNewChars, 0, intCurrent);
            }
        }

        /// <summary>
        /// Method to quickly remove all instances of a substring from a string (should be faster than using Replace() with an empty string)
        /// </summary>
        /// <param name="strInput">String on which to operate</param>
        /// <param name="strSubstringToDelete">Substring to remove</param>
        /// <param name="eComparison">Comparison rules by which to find instances of the substring to remove. Useful for when case-insensitive removal is required.</param>
        /// <returns>New string with <paramref name="strSubstringToDelete"/> removed</returns>
        public static string FastEscape(this string strInput, string strSubstringToDelete,
                                        StringComparison eComparison = StringComparison.Ordinal)
        {
            // It's actually faster to just run Replace(), albeit with our special comparison override, than to make our own fancy function
            return strInput.Replace(strSubstringToDelete, string.Empty, eComparison);
        }

        /// <summary>
        /// Method to quickly remove the first instance of a substring from a string.
        /// </summary>
        /// <param name="strInput">String on which to operate.</param>
        /// <param name="strSubstringToDelete">Substring to remove.</param>
        /// <param name="intStartIndex">Index from which to begin searching.</param>
        /// <param name="eComparison">Comparison rules by which to find the substring to remove. Useful for when case-insensitive removal is required.</param>
        /// <returns>New string with the first instance of <paramref name="strSubstringToDelete"/> removed starting from <paramref name="intStartIndex"/>.</returns>
        public static string FastEscapeOnceFromStart(this string strInput, string strSubstringToDelete,
                                                     int intStartIndex = 0,
                                                     StringComparison eComparison = StringComparison.Ordinal)
        {
            if (strSubstringToDelete == null)
                return strInput;
            int intToDeleteLength = strSubstringToDelete.Length;
            if (intToDeleteLength == 0)
                return strInput;
            if (strInput == null)
                return string.Empty;
            if (strInput.Length < intToDeleteLength)
                return strInput;

            int intIndexToBeginRemove = strInput.IndexOf(strSubstringToDelete, intStartIndex, eComparison);
            return intIndexToBeginRemove == -1 ? strInput : strInput.Remove(intIndexToBeginRemove, intToDeleteLength);
        }

        /// <summary>
        /// Method to quickly remove the last instance of a substring from a string.
        /// </summary>
        /// <param name="strInput">String on which to operate.</param>
        /// <param name="strSubstringToDelete">Substring to remove.</param>
        /// <param name="intStartIndex">Index from which to begin searching (proceeding towards the beginning of the string).</param>
        /// <param name="eComparison">Comparison rules by which to find the substring to remove. Useful for when case-insensitive removal is required.</param>
        /// <returns>New string with the last instance of <paramref name="strSubstringToDelete"/> removed starting from <paramref name="intStartIndex"/>.</returns>
        public static string FastEscapeOnceFromEnd(this string strInput, string strSubstringToDelete,
                                                   int intStartIndex = -1,
                                                   StringComparison eComparison = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(strInput) || strSubstringToDelete == null)
                return strInput;
            int intToDeleteLength = strSubstringToDelete.Length;
            if (intToDeleteLength == 0)
                return strInput;
            if (intStartIndex < 0)
                intStartIndex += strInput.Length;
            if (intStartIndex < intToDeleteLength - 1)
                return strInput;

            int intIndexToBeginRemove = strInput.LastIndexOf(strSubstringToDelete, intStartIndex, eComparison);
            return intIndexToBeginRemove == -1 ? strInput : strInput.Remove(intIndexToBeginRemove, intToDeleteLength);
        }

        /// <summary>
        /// Syntactic sugar for string::IndexOfAny that uses params in its argument for the char array.
        /// </summary>
        /// <param name="strHaystack">String to search.</param>
        /// <param name="anyOf">Array of characters to match with IndexOfAny</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfAny(this string strHaystack, params char[] anyOf)
        {
            if (string.IsNullOrEmpty(strHaystack))
                return -1;
            return strHaystack.IndexOfAny(anyOf);
        }

        /// <summary>
        /// Find the index of the first instance of a set of strings inside a haystack string.
        /// </summary>
        /// <param name="strHaystack">String to search.</param>
        /// <param name="astrNeedles">Array of strings to match.</param>
        /// <param name="eComparison">Comparison rules by which to find instances of the substring to remove. Useful for when case-insensitive removal is required.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfAny(this string strHaystack, IReadOnlyCollection<string> astrNeedles, StringComparison eComparison)
        {
            return IndexOfAny(strHaystack, astrNeedles, 0, eComparison);
        }

        /// <summary>
        /// Find the index of the first instance of a set of strings inside a haystack string.
        /// </summary>
        /// <param name="strHaystack">String to search.</param>
        /// <param name="astrNeedles">Array of strings to match.</param>
        /// <param name="intStartIndex">Index from which to start looking.</param>
        /// <param name="eComparison">Comparison rules by which to find instances of the substring to remove. Useful for when case-insensitive removal is required.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfAny(this string strHaystack, IReadOnlyCollection<string> astrNeedles, int intStartIndex, StringComparison eComparison = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(strHaystack))
                return -1;
            int intHaystackLength = strHaystack.Length;
            if (intHaystackLength == 0)
                return -1;
            if (astrNeedles == null)
                return -1;
            int intNumNeedles = astrNeedles.Count;
            if (intNumNeedles == 0)
                return -1;

            // While one might think this is the slowest, worst-scaling way of checking for multiple needles, it's actually faster
            // in C# than a more detailed approach where characters of the haystack are progressively checked against all needles.
            if (astrNeedles.All(x => x.Length + intStartIndex > intHaystackLength))
                return -1;

            int intEarliestNeedleIndex = intHaystackLength;
            foreach (string strNeedle in astrNeedles)
            {
                int intNeedleIndex = strHaystack.IndexOf(strNeedle, intStartIndex, Math.Min(intHaystackLength, intEarliestNeedleIndex + strNeedle.Length) - intStartIndex, eComparison);
                if (intNeedleIndex >= 0 && intNeedleIndex < intEarliestNeedleIndex)
                    intEarliestNeedleIndex = intNeedleIndex;
            }
            return intEarliestNeedleIndex != intHaystackLength ? intEarliestNeedleIndex : -1;
        }

        /// <summary>
        /// Find the index of the first instance of a set of strings inside a haystack string.
        /// </summary>
        /// <param name="strHaystack">String to search.</param>
        /// <param name="astrNeedles">Array of strings to match.</param>
        /// <param name="eComparison">Comparison rules by which to find instances of the substring to remove. Useful for when case-insensitive removal is required.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfAny(this string strHaystack, IEnumerable<string> astrNeedles, StringComparison eComparison = StringComparison.Ordinal)
        {
            return IndexOfAny(strHaystack, astrNeedles, 0, eComparison);
        }

        /// <summary>
        /// Find the index of the first instance of a set of strings inside a haystack string.
        /// </summary>
        /// <param name="strHaystack">String to search.</param>
        /// <param name="astrNeedles">Array of strings to match.</param>
        /// <param name="intStartIndex">Index from which to start looking.</param>
        /// <param name="eComparison">Comparison rules by which to find instances of the substring to remove. Useful for when case-insensitive removal is required.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfAny(this string strHaystack, IEnumerable<string> astrNeedles, int intStartIndex, StringComparison eComparison = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(strHaystack))
                return -1;
            int intHaystackLength = strHaystack.Length;
            if (intHaystackLength == 0)
                return -1;
            if (astrNeedles == null)
                return -1;

            // While one might think this is the slowest, worst-scaling way of checking for multiple needles, it's actually faster
            // in C# than a more detailed approach where characters of the haystack are progressively checked against all needles.

            int intEarliestNeedleIndex = intHaystackLength;
            foreach (string strNeedle in astrNeedles)
            {
                int intNeedleIndex = strHaystack.IndexOf(strNeedle, intStartIndex, Math.Min(intHaystackLength, intEarliestNeedleIndex + strNeedle.Length) - intStartIndex, eComparison);
                if (intNeedleIndex >= 0 && intNeedleIndex < intEarliestNeedleIndex)
                    intEarliestNeedleIndex = intNeedleIndex;
            }
            return intEarliestNeedleIndex != intHaystackLength ? intEarliestNeedleIndex : -1;
        }

        /// <summary>
        /// Find the index of the first instance of a set of strings inside a haystack string.
        /// </summary>
        /// <param name="strHaystack">String to search.</param>
        /// <param name="astrNeedles">Array of strings to match.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfAny(this string strHaystack, params string[] astrNeedles)
        {
            return strHaystack.IndexOfAny(astrNeedles, StringComparison.Ordinal);
        }

        /// <summary>
        /// Find the index of the first instance of a set of strings inside a haystack string.
        /// </summary>
        /// <param name="strHaystack">String to search.</param>
        /// <param name="astrNeedles">Array of strings to match.</param>
        /// <param name="intStartIndex">Index from which to start looking.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfAny(this string strHaystack, int intStartIndex, params string[] astrNeedles)
        {
            return strHaystack.IndexOfAny(astrNeedles, intStartIndex, StringComparison.Ordinal);
        }

        /// <summary>
        /// Syntactic sugar for string::LastIndexOfAny that uses params in its argument for the char array.
        /// </summary>
        /// <param name="strHaystack">String to search.</param>
        /// <param name="anyOf">Array of characters to match with LastIndexOfAny</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LastIndexOfAny(this string strHaystack, params char[] anyOf)
        {
            if (string.IsNullOrEmpty(strHaystack))
                return -1;
            return strHaystack.LastIndexOfAny(anyOf);
        }

        /// <summary>
        /// Find if of a haystack string contains any of a set of strings.
        /// </summary>
        /// <param name="strHaystack">String to search.</param>
        /// <param name="astrNeedles">Array of strings to match.</param>
        /// <param name="eComparison">Comparison rules by which to find instances of the substring to remove. Useful for when case-insensitive removal is required.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsAny(this string strHaystack, IReadOnlyCollection<string> astrNeedles, StringComparison eComparison)
        {
            if (string.IsNullOrEmpty(strHaystack))
                return false;
            int intHaystackLength = strHaystack.Length;
            if (intHaystackLength == 0)
                return false;
            if (astrNeedles == null)
                return false;
            int intNumNeedles = astrNeedles.Count;
            if (intNumNeedles == 0)
                return false;

            // While one might think this is the slowest, worst-scaling way of checking for multiple needles, it's actually faster
            // in C# than a more detailed approach where characters of the haystack are progressively checked against all needles.

            return astrNeedles.Any(x => x.Length <= intHaystackLength && strHaystack.Contains(x, eComparison));
        }

        /// <summary>
        /// Find if of a haystack string contains any of a set of strings.
        /// </summary>
        /// <param name="strHaystack">String to search.</param>
        /// <param name="astrNeedles">Array of strings to match.</param>
        /// <param name="eComparison">Comparison rules by which to find instances of the substring to remove. Useful for when case-insensitive removal is required.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsAny(this string strHaystack, IEnumerable<string> astrNeedles, StringComparison eComparison = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(strHaystack))
                return false;
            int intHaystackLength = strHaystack.Length;
            if (intHaystackLength == 0)
                return false;
            return astrNeedles != null &&
                   // While one might think this is the slowest, worst-scaling way of checking for multiple needles, it's actually faster
                   // in C# than a more detailed approach where characters of the haystack are progressively checked against all needles.
                   astrNeedles.Any(strNeedle => strHaystack.Contains(strNeedle, eComparison));
        }

        /// <summary>
        /// Find if of a haystack string contains any of a set of strings.
        /// </summary>
        /// <param name="strHaystack">String to search.</param>
        /// <param name="astrNeedles">Array of strings to match.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsAny(this string strHaystack, params string[] astrNeedles)
        {
            return strHaystack.ContainsAny(astrNeedles, StringComparison.Ordinal);
        }

        /// <summary>
        /// Find if of a haystack string contains any of a set of strings (parallelized version where each needle is checked in parallel).
        /// </summary>
        /// <param name="strHaystack">String to search.</param>
        /// <param name="astrNeedles">Array of strings to match.</param>
        /// <param name="eComparison">Comparison rules by which to find instances of the substring to remove. Useful for when case-insensitive removal is required.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsAnyParallel(this string strHaystack, IReadOnlyCollection<string> astrNeedles, StringComparison eComparison)
        {
            if (string.IsNullOrEmpty(strHaystack))
                return false;
            int intHaystackLength = strHaystack.Length;
            if (intHaystackLength == 0)
                return false;
            if (astrNeedles == null)
                return false;
            int intNumNeedles = astrNeedles.Count;
            if (intNumNeedles == 0)
                return false;

            // While one might think this is the slowest, worst-scaling way of checking for multiple needles, it's actually faster
            // in C# than a more detailed approach where characters of the haystack are progressively checked against all needles.

            return astrNeedles.AsParallel().Any(x => x.Length <= intHaystackLength && strHaystack.Contains(x, eComparison));
        }

        /// <summary>
        /// Find if of a haystack string contains any of a set of strings (parallelized version where each needle is checked in parallel).
        /// </summary>
        /// <param name="strHaystack">String to search.</param>
        /// <param name="astrNeedles">Array of strings to match.</param>
        /// <param name="eComparison">Comparison rules by which to find instances of the substring to remove. Useful for when case-insensitive removal is required.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsAnyParallel(this string strHaystack, IEnumerable<string> astrNeedles, StringComparison eComparison = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(strHaystack))
                return false;
            int intHaystackLength = strHaystack.Length;
            if (intHaystackLength == 0)
                return false;
            return astrNeedles != null &&
                   // While one might think this is the slowest, worst-scaling way of checking for multiple needles, it's actually faster
                   // in C# than a more detailed approach where characters of the haystack are progressively checked against all needles.
                   astrNeedles.AsParallel().Any(strNeedle => strHaystack.Contains(strNeedle, eComparison));
        }

        /// <summary>
        /// Find if of a haystack string contains any of a set of strings (parallelized version where each needle is checked in parallel).
        /// </summary>
        /// <param name="strHaystack">String to search.</param>
        /// <param name="astrNeedles">Array of strings to match.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsAnyParallel(this string strHaystack, params string[] astrNeedles)
        {
            return strHaystack.ContainsAnyParallel(astrNeedles, StringComparison.Ordinal);
        }

        /// <summary>
        /// Syntactic sugar for string::Split that uses one separator char in its argument in addition to StringSplitOptions.
        /// </summary>
        /// <param name="strInput">String to search.</param>
        /// <param name="chrSeparator">Separator to use.</param>
        /// <param name="eSplitOptions">String split options.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string[] Split(this string strInput, char chrSeparator, StringSplitOptions eSplitOptions)
        {
            if (strInput == null)
                throw new ArgumentNullException(nameof(strInput));
            return strInput.Split(new[] {chrSeparator}, eSplitOptions);
        }

        /// <summary>
        /// Syntactic sugar for string::Split that uses one separator string in its argument in addition to StringSplitOptions.
        /// </summary>
        /// <param name="strInput">String to search.</param>
        /// <param name="strSeparator">Separator to use.</param>
        /// <param name="eSplitOptions">String split options.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string[] Split(this string strInput, string strSeparator, StringSplitOptions eSplitOptions)
        {
            if (strInput == null)
                throw new ArgumentNullException(nameof(strInput));
            return strInput.Split(new[] {strSeparator}, eSplitOptions);
        }

        /// <summary>
        /// Syntactic sugar for a version of Contains(char) for strings that is faster than messing with Linq
        /// </summary>
        /// <param name="strHaystack">Input string to search.</param>
        /// <param name="chrNeedle">Character for which to look.</param>
        /// <param name="intStartIndex">Index from which to begin searching.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains(this string strHaystack, char chrNeedle, int intStartIndex = 0)
        {
            if (strHaystack == null)
                throw new ArgumentNullException(nameof(strHaystack));
            return strHaystack.IndexOf(chrNeedle, intStartIndex) != -1;
        }

        /// <summary>
        /// Syntactic sugar for a version of Contains(string) for strings based on a specified StringComparison
        /// </summary>
        /// <param name="strHaystack">Input string to search.</param>
        /// <param name="strNeedle">String for which to look.</param>
        /// <param name="eComparison">Comparison to use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains(this string strHaystack, string strNeedle, StringComparison eComparison)
        {
            if (strHaystack == null)
                throw new ArgumentNullException(nameof(strHaystack));
            return strHaystack.IndexOf(strNeedle, eComparison) != -1;
        }

        /// <summary>
        /// Syntactic sugar for a version of Contains(string) for strings from a specific starting index
        /// </summary>
        /// <param name="strHaystack">Input string to search.</param>
        /// <param name="strNeedle">String for which to look.</param>
        /// <param name="intStartIndex">Index from which to begin searching.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains(this string strHaystack, string strNeedle, int intStartIndex)
        {
            if (strHaystack == null)
                throw new ArgumentNullException(nameof(strHaystack));
            return strHaystack.IndexOf(strNeedle, intStartIndex) != -1;
        }

        /// <summary>
        /// Syntactic sugar for a version of Contains(string) for strings based on a specified StringComparison from a specific starting index
        /// </summary>
        /// <param name="strHaystack">Input string to search.</param>
        /// <param name="strNeedle">String for which to look.</param>
        /// <param name="intStartIndex">Index from which to begin searching.</param>
        /// <param name="eComparison">Comparison to use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains(this string strHaystack, string strNeedle, int intStartIndex, StringComparison eComparison)
        {
            if (strHaystack == null)
                throw new ArgumentNullException(nameof(strHaystack));
            return strHaystack.IndexOf(strNeedle, intStartIndex, eComparison) != -1;
        }

        /// <summary>
        /// Version of string::Split() that avoids allocations where possible, thus making it lighter on memory (and also on CPU because allocations take time) than all versions of string::Split()
        /// </summary>
        /// <param name="strInput">Input textblock.</param>
        /// <param name="chrSplit">Character to use for splitting.</param>
        /// <param name="eSplitOptions">Optional argument that can be used to skip over empty entries.</param>
        /// <returns>Enumerable containing substrings of <paramref name="strInput"/> split based on <paramref name="chrSplit"/></returns>
        public static IEnumerable<string> SplitNoAlloc(this string strInput, char chrSplit,
                                                       StringSplitOptions eSplitOptions = StringSplitOptions.None)
        {
            if (string.IsNullOrEmpty(strInput))
            {
                if (eSplitOptions == StringSplitOptions.None)
                    yield return string.Empty;
                yield break;
            }
            int intInputLength = strInput.Length;
            int intLoopLength;
            for (int intStart = 0; intStart < intInputLength; intStart += intLoopLength + 1)
            {
                intLoopLength = strInput.IndexOf(chrSplit, intStart);
                if (intLoopLength < 0)
                    intLoopLength = intInputLength;
                intLoopLength -= intStart;
                if (intLoopLength != 0)
                    yield return strInput.Substring(intStart, intLoopLength);
                else if (eSplitOptions == StringSplitOptions.None)
                    yield return string.Empty;
            }
        }

        /// <summary>
        /// Version of string::Split() that avoids allocations where possible, thus making it lighter on memory (and also on CPU because allocations take time) than all versions of string::Split()
        /// </summary>
        /// <param name="strInput">Input textblock.</param>
        /// <param name="chrSplit">Character to use for splitting.</param>
        /// <param name="intCount">The maximum number of substrings to return.</param>
        /// <param name="eSplitOptions">Optional argument that can be used to skip over empty entries.</param>
        /// <returns>Enumerable containing substrings of <paramref name="strInput"/> split based on <paramref name="chrSplit"/></returns>
        public static IEnumerable<string> SplitNoAlloc(this string strInput, char chrSplit, int intCount,
                                                       StringSplitOptions eSplitOptions = StringSplitOptions.None)
        {
            if (string.IsNullOrEmpty(strInput))
            {
                if (eSplitOptions == StringSplitOptions.None)
                    yield return string.Empty;
                yield break;
            }
            if (intCount <= 0)
                yield break;
            int intInputLength = strInput.Length;
            int intLoopLength;
            for (int intStart = 0; intStart < intInputLength; intStart += intLoopLength + 1)
            {
                if (--intCount < 0)
                    yield break;
                intLoopLength = strInput.IndexOf(chrSplit, intStart);
                if (intLoopLength < 0)
                    intLoopLength = intInputLength;
                intLoopLength -= intStart;
                if (intLoopLength != 0)
                    yield return strInput.Substring(intStart, intLoopLength);
                else if (eSplitOptions == StringSplitOptions.None)
                    yield return string.Empty;
            }
        }

        /// <summary>
        /// Version of string::Split() that avoids allocations where possible, thus making it lighter on memory (and also on CPU because allocations take time) than all versions of string::Split()
        /// </summary>
        /// <param name="strInput">Input textblock.</param>
        /// <param name="strSplit">String to use for splitting.</param>
        /// <param name="eSplitOptions">Optional argument that can be used to skip over empty entries.</param>
        /// <param name="eComparison">Comparison to use when searching for the next instance of <paramref name="strSplit"/>.</param>
        /// <returns>Enumerable containing substrings of <paramref name="strInput"/> split based on <paramref name="strSplit"/></returns>
        public static IEnumerable<string> SplitNoAlloc(this string strInput, string strSplit,
            StringSplitOptions eSplitOptions = StringSplitOptions.None, StringComparison eComparison = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(strInput))
            {
                if (eSplitOptions == StringSplitOptions.None)
                    yield return string.Empty;
                yield break;
            }
            if (string.IsNullOrEmpty(strSplit))
            {
                yield return strInput;
                yield break;
            }

            int intInputLength = strInput.Length;
            int intSplitLength = strSplit.Length;
            int intLoopLength;
            for (int intStart = 0; intStart < intInputLength; intStart += intLoopLength + intSplitLength)
            {
                intLoopLength = strInput.IndexOf(strSplit, intStart, eComparison);
                if (intLoopLength < 0)
                    intLoopLength = intInputLength;
                intLoopLength -= intStart;
                if (intLoopLength != 0)
                    yield return strInput.Substring(intStart, intLoopLength);
                else if (eSplitOptions == StringSplitOptions.None)
                    yield return string.Empty;
            }
        }

        /// <summary>
        /// Version of string::Split() that avoids allocations where possible, thus making it lighter on memory (and also on CPU because allocations take time) than all versions of string::Split()
        /// </summary>
        /// <param name="strInput">Input textblock.</param>
        /// <param name="strSplit">String to use for splitting.</param>
        /// <param name="intCount">The maximum number of substrings to return.</param>
        /// <param name="eSplitOptions">Optional argument that can be used to skip over empty entries.</param>
        /// <param name="eComparison">Comparison to use when searching for the next instance of <paramref name="strSplit"/>.</param>
        /// <returns>Enumerable containing substrings of <paramref name="strInput"/> split based on <paramref name="strSplit"/></returns>
        public static IEnumerable<string> SplitNoAlloc(this string strInput, string strSplit, int intCount,
            StringSplitOptions eSplitOptions = StringSplitOptions.None, StringComparison eComparison = StringComparison.Ordinal)
        {
            if (intCount <= 0)
                yield break;
            if (string.IsNullOrEmpty(strInput))
            {
                if (eSplitOptions == StringSplitOptions.None)
                    yield return string.Empty;
                yield break;
            }
            if (string.IsNullOrEmpty(strSplit))
            {
                yield return strInput;
                yield break;
            }

            int intInputLength = strInput.Length;
            int intSplitLength = strSplit.Length;
            int intLoopLength;
            for (int intStart = 0; intStart < intInputLength; intStart += intLoopLength + intSplitLength)
            {
                if (--intCount < 0)
                    yield break;
                intLoopLength = strInput.IndexOf(strSplit, intStart, eComparison);
                if (intLoopLength < 0)
                    intLoopLength = intInputLength;
                intLoopLength -= intStart;
                if (intLoopLength != 0)
                    yield return strInput.Substring(intStart, intLoopLength);
                else if (eSplitOptions == StringSplitOptions.None)
                    yield return string.Empty;
            }
        }

        /// <summary>
        /// Version of string::Split() that avoids allocations where possible, thus making it lighter on memory (and also on CPU because allocations take time) than all versions of string::Split()
        /// </summary>
        /// <param name="strInput">Input textblock.</param>
        /// <param name="achrSplit">Characters to use for splitting.</param>
        /// <returns>Enumerable containing substrings of <paramref name="strInput"/> split based on <paramref name="achrSplit"/></returns>
        public static IEnumerable<string> SplitNoAlloc(this string strInput, params char[] achrSplit)
        {
            return SplitNoAlloc(strInput, StringSplitOptions.None, achrSplit);
        }

        /// <summary>
        /// Version of string::Split() that avoids allocations where possible, thus making it lighter on memory (and also on CPU because allocations take time) than all versions of string::Split()
        /// </summary>
        /// <param name="strInput">Input textblock.</param>
        /// <param name="achrSplit">Characters to use for splitting.</param>
        /// <returns>Enumerable containing substrings of <paramref name="strInput"/> split based on <paramref name="achrSplit"/></returns>
        public static IEnumerable<string> SplitNoAlloc(this string strInput, StringSplitOptions eSplitOptions, params char[] achrSplit)
        {
            if (string.IsNullOrEmpty(strInput))
            {
                if (eSplitOptions != StringSplitOptions.RemoveEmptyEntries)
                    yield return string.Empty;
                yield break;
            }
            if (achrSplit.Length == 0)
            {
                if (eSplitOptions != StringSplitOptions.RemoveEmptyEntries)
                    yield return strInput;
                yield break;
            }
            int intLoopLength;
            for (int intStart = 0; intStart < strInput.Length; intStart += intLoopLength + 1)
            {
                intLoopLength = strInput.IndexOfAny(achrSplit, intStart);
                if (intLoopLength < 0)
                    intLoopLength = strInput.Length;
                intLoopLength -= intStart;
                if (intLoopLength == 0)
                {
                    if (eSplitOptions != StringSplitOptions.RemoveEmptyEntries)
                        yield return string.Empty;
                }
                else
                    yield return strInput.Substring(intStart, intLoopLength);
            }
        }

        /// <summary>
        /// Version of string::Split() that guarantees that the returned string will be of a specific size, padding with string.Empty when needed.
        /// Slightly faster than built-in versions of string:Split() because fewer allocations are needed and there is no need to search ahead for how many elements should be in the returned array.
        /// </summary>
        /// <param name="strInput">Input textblock.</param>
        /// <param name="intSize">Size of the array to return.</param>
        /// <param name="chrSplit">Character to use for splitting.</param>
        /// <param name="eSplitOptions">Optional argument that can be used to skip over empty entries.</param>
        /// <returns>Array of length <paramref name="intSize"/> containing substrings of <paramref name="strInput"/> split based on <paramref name="chrSplit"/></returns>
        public static string[] SplitFixedSize(this string strInput, char chrSplit, int intSize,
                                                       StringSplitOptions eSplitOptions = StringSplitOptions.None)
        {
            if (intSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(intSize));
            string[] astrReturn = new string[intSize];
            Array.Clear(astrReturn, 0, intSize);
            if (string.IsNullOrEmpty(strInput))
                return astrReturn;
            if (intSize == 1)
            {
                astrReturn[0] = strInput;
                return astrReturn;
            }
            int intInputLength = strInput.Length;
            int intLoopLength;
            int intIndex = 0;
            for (int intStart = 0; intStart < intInputLength; intStart += intLoopLength + 1)
            {
                intLoopLength = strInput.IndexOf(chrSplit, intStart);
                if (intLoopLength < 0)
                    intLoopLength = intInputLength;
                intLoopLength -= intStart;
                if (intLoopLength != 0)
                    astrReturn[intIndex++] = strInput.Substring(intStart, intLoopLength);
                else if (eSplitOptions == StringSplitOptions.None)
                    ++intIndex;
                if (intIndex >= intSize)
                    break;
            }
            return astrReturn;
        }

        /// <summary>
        /// Version of string::Split() that guarantees that the returned string will be of a specific size, padding with string.Empty when needed.
        /// Slightly faster than built-in versions of string:Split() because fewer allocations are needed and there is no need to search ahead for how many elements should be in the returned array.
        /// </summary>
        /// <param name="strInput">Input textblock.</param>
        /// <param name="intSize">Size of the array to return.</param>
        /// <param name="strSplit">String to use for splitting.</param>
        /// <param name="eSplitOptions">Optional argument that can be used to skip over empty entries.</param>
        /// <param name="eComparison">Comparison to use when searching for the next instance of <paramref name="strSplit"/>.</param>
        /// <returns>Array of length <paramref name="intSize"/> containing substrings of <paramref name="strInput"/> split based on <paramref name="strSplit"/></returns>
        public static string[] SplitFixedSize(this string strInput, string strSplit, int intSize,
            StringSplitOptions eSplitOptions = StringSplitOptions.None, StringComparison eComparison = StringComparison.Ordinal)
        {
            if (intSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(intSize));
            string[] astrReturn = new string[intSize];
            Array.Clear(astrReturn, 0, intSize);
            if (string.IsNullOrEmpty(strInput))
                return astrReturn;
            if (string.IsNullOrEmpty(strSplit) || intSize == 1)
            {
                astrReturn[0] = strInput;
                return astrReturn;
            }

            int intInputLength = strInput.Length;
            int intSplitLength = strSplit.Length;
            int intLoopLength;
            int intIndex = 0;
            for (int intStart = 0; intStart < intInputLength; intStart += intLoopLength + intSplitLength)
            {
                intLoopLength = strInput.IndexOf(strSplit, intStart, eComparison);
                if (intLoopLength < 0)
                    intLoopLength = intInputLength;
                intLoopLength -= intStart;
                if (intLoopLength != 0)
                    astrReturn[intIndex++] = strInput.Substring(intStart, intLoopLength);
                else if (eSplitOptions == StringSplitOptions.None)
                    ++intIndex;
                if (intIndex >= intSize)
                    break;
            }
            return astrReturn;
        }

        /// <summary>
        /// Version of string::Split() that guarantees that the returned string will be of a specific size, padding with string.Empty when needed.
        /// Slightly faster than built-in versions of string:Split() because fewer allocations are needed and there is no need to search ahead for how many elements should be in the returned array.
        /// </summary>
        /// <param name="strInput">Input textblock.</param>
        /// <param name="intSize">Size of the array to return.</param>
        /// <param name="achrSplit">Characters to use for splitting.</param>
        /// <returns>Array of length <paramref name="intSize"/> containing substrings of <paramref name="strInput"/> split based on <paramref name="achrSplit"/></returns>
        public static string[] SplitFixedSize(this string strInput, int intSize, params char[] achrSplit)
        {
            if (intSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(intSize));
            string[] astrReturn = new string[intSize];
            Array.Clear(astrReturn, 0, intSize);
            if (string.IsNullOrEmpty(strInput))
                return astrReturn;
            if (achrSplit.Length == 0 || intSize == 1)
            {
                astrReturn[0] = strInput;
                return astrReturn;
            }
            int intLoopLength;
            int intIndex = 0;
            for (int intStart = 0; intStart < strInput.Length; intStart += intLoopLength + 1)
            {
                intLoopLength = strInput.IndexOfAny(achrSplit, intStart);
                if (intLoopLength < 0)
                    intLoopLength = strInput.Length;
                intLoopLength -= intStart;
                if (intLoopLength != 0)
                    astrReturn[intIndex] = strInput.Substring(intStart, intLoopLength);
                ++intIndex;
                if (intIndex >= intSize)
                    break;
            }
            return astrReturn;
        }

        /// <summary>
        /// Version of string::Split() that returns an array from ArrayPool.Shared instead of allocating it, and only splits to a specific array size, padding with string.Empty when necessary.
        /// Slightly faster than built-in versions of string:Split() because no allocations are needed and there is no need to search ahead for how many elements should be in the returned array.
        /// Remember to return the result to ArrayPool.Shared when finished with it!
        /// </summary>
        /// <param name="strInput">Input textblock.</param>
        /// <param name="intSize">Size of the array to return.</param>
        /// <param name="chrSplit">Character to use for splitting.</param>
        /// <param name="eSplitOptions">Optional argument that can be used to skip over empty entries.</param>
        /// <returns>Array of length <paramref name="intSize"/> containing substrings of <paramref name="strInput"/> split based on <paramref name="chrSplit"/></returns>
        public static string[] SplitFixedSizePooledArray(this string strInput, char chrSplit, int intSize,
                                                       StringSplitOptions eSplitOptions = StringSplitOptions.None)
        {
            if (intSize < 0)
                throw new ArgumentOutOfRangeException(nameof(intSize));
            if (intSize == 0)
                return ArrayPool<string>.Shared.Rent(0);
            string[] astrReturn = ArrayPool<string>.Shared.Rent(intSize);
            try
            {
                Array.Clear(astrReturn, 0, intSize);
                if (string.IsNullOrEmpty(strInput))
                    return astrReturn;
                if (intSize == 1)
                {
                    astrReturn[0] = strInput;
                    return astrReturn;
                }
                int intInputLength = strInput.Length;
                int intLoopLength;
                int intIndex = 0;
                for (int intStart = 0; intStart < intInputLength; intStart += intLoopLength + 1)
                {
                    intLoopLength = strInput.IndexOf(chrSplit, intStart);
                    if (intLoopLength < 0)
                        intLoopLength = intInputLength;
                    intLoopLength -= intStart;
                    if (intLoopLength != 0)
                        astrReturn[intIndex++] = strInput.Substring(intStart, intLoopLength);
                    else if (eSplitOptions == StringSplitOptions.None)
                        ++intIndex;
                    if (intIndex >= intSize)
                        break;
                }
                return astrReturn;
            }
            catch
            {
                ArrayPool<string>.Shared.Return(astrReturn);
                throw;
            }
        }

        /// <summary>
        /// Version of string::Split() that returns an array from ArrayPool.Shared instead of allocating it, and only splits to a specific array size, padding with string.Empty when necessary.
        /// Slightly faster than built-in versions of string:Split() because no allocations are needed and there is no need to search ahead for how many elements should be in the returned array.
        /// Remember to return the result to ArrayPool.Shared when finished with it!
        /// </summary>
        /// <param name="strInput">Input textblock.</param>
        /// <param name="intSize">Size of the array to return.</param>
        /// <param name="strSplit">String to use for splitting.</param>
        /// <param name="eSplitOptions">Optional argument that can be used to skip over empty entries.</param>
        /// <param name="eComparison">Comparison to use when searching for the next instance of <paramref name="strSplit"/>.</param>
        /// <returns>Array of length <paramref name="intSize"/> containing substrings of <paramref name="strInput"/> split based on <paramref name="strSplit"/></returns>
        public static string[] SplitFixedSizePooledArray(this string strInput, string strSplit, int intSize,
            StringSplitOptions eSplitOptions = StringSplitOptions.None, StringComparison eComparison = StringComparison.Ordinal)
        {
            if (intSize < 0)
                throw new ArgumentOutOfRangeException(nameof(intSize));
            if (intSize == 0)
                return ArrayPool<string>.Shared.Rent(0);
            string[] astrReturn = ArrayPool<string>.Shared.Rent(intSize);
            try
            {
                Array.Clear(astrReturn, 0, intSize);
                if (string.IsNullOrEmpty(strInput))
                    return astrReturn;
                if (string.IsNullOrEmpty(strSplit) || intSize == 1)
                {
                    astrReturn[0] = strInput;
                    return astrReturn;
                }

                int intInputLength = strInput.Length;
                int intSplitLength = strSplit.Length;
                int intLoopLength;
                int intIndex = 0;
                for (int intStart = 0; intStart < intInputLength; intStart += intLoopLength + intSplitLength)
                {
                    intLoopLength = strInput.IndexOf(strSplit, intStart, eComparison);
                    if (intLoopLength < 0)
                        intLoopLength = intInputLength;
                    intLoopLength -= intStart;
                    if (intLoopLength != 0)
                        astrReturn[intIndex++] = strInput.Substring(intStart, intLoopLength);
                    else if (eSplitOptions == StringSplitOptions.None)
                        ++intIndex;
                    if (intIndex >= intSize)
                        break;
                }
                return astrReturn;
            }
            catch
            {
                ArrayPool<string>.Shared.Return(astrReturn);
                throw;
            }
        }

        /// <summary>
        /// Version of string::Split() that returns an array from ArrayPool.Shared instead of allocating it, and only splits to a specific array size, padding with string.Empty when necessary.
        /// Slightly faster than built-in versions of string:Split() because no allocations are needed and there is no need to search ahead for how many elements should be in the returned array.
        /// Remember to return the result to ArrayPool.Shared when finished with it!
        /// </summary>
        /// <param name="strInput">Input textblock.</param>
        /// <param name="intSize">Size of the array to return.</param>
        /// <param name="achrSplit">Characters to use for splitting.</param>
        /// <returns>Array of length <paramref name="intSize"/> containing substrings of <paramref name="strInput"/> split based on <paramref name="achrSplit"/></returns>
        public static string[] SplitFixedSizePooledArray(this string strInput, int intSize, params char[] achrSplit)
        {
            if (intSize < 0)
                throw new ArgumentOutOfRangeException(nameof(intSize));
            if (intSize == 0)
                return ArrayPool<string>.Shared.Rent(0);
            string[] astrReturn = ArrayPool<string>.Shared.Rent(intSize);
            try
            {
                Array.Clear(astrReturn, 0, intSize);
                if (string.IsNullOrEmpty(strInput))
                    return astrReturn;
                if (achrSplit.Length == 0 || intSize == 1)
                {
                    astrReturn[0] = strInput;
                    return astrReturn;
                }
                int intLoopLength;
                int intIndex = 0;
                for (int intStart = 0; intStart < strInput.Length; intStart += intLoopLength + 1)
                {
                    intLoopLength = strInput.IndexOfAny(achrSplit, intStart);
                    if (intLoopLength < 0)
                        intLoopLength = strInput.Length;
                    intLoopLength -= intStart;
                    if (intLoopLength != 0)
                        astrReturn[intIndex] = strInput.Substring(intStart, intLoopLength);
                    ++intIndex;
                    if (intIndex >= intSize)
                        break;
                }
                return astrReturn;
            }
            catch
            {
                ArrayPool<string>.Shared.Return(astrReturn);
                throw;
            }
        }

        /// <summary>
        /// Version of string::Split() that returns a pooled string array, reducing overall allocations at the cost of needing to pay attention to disposal
        /// </summary>
        /// <param name="strInput">Input textblock.</param>
        /// <param name="arrayLength">Length of the returned array. Needs to be stored and handled separately because we cannot guarantee that a pooled array will not be longer than necessary for the split.</param>
        /// <param name="chrSplit">Character to use for splitting.</param>
        /// <param name="eSplitOptions">Optional argument that can be used to skip over empty entries.</param>
        /// <returns>String array rented from ArrayPool{string}.Shared containing substrings of <paramref name="strInput"/> split based on <paramref name="chrSplit"/></returns>
        public static string[] SplitToPooledArray(this string strInput, out int arrayLength, char chrSplit,
                                                       StringSplitOptions eSplitOptions = StringSplitOptions.None)
        {
            arrayLength = 0;
            if (string.IsNullOrEmpty(strInput))
            {
                if (eSplitOptions == StringSplitOptions.None)
                {
                    arrayLength = 1;
                    string[] astrFastReturn = ArrayPool<string>.Shared.Rent(1);
                    astrFastReturn[0] = string.Empty;
                    return astrFastReturn;
                }
                else
                    return ArrayPool<string>.Shared.Rent(0);
            }
            int intInputLength = strInput.Length;
            int intLoopLength;
            for (int intStart = 0; intStart < intInputLength; intStart += intLoopLength + 1)
            {
                intLoopLength = strInput.IndexOf(chrSplit, intStart);
                if (intLoopLength < 0)
                    intLoopLength = intInputLength;
                intLoopLength -= intStart;
                if (intLoopLength != 0 || eSplitOptions == StringSplitOptions.None)
                    ++arrayLength;
            }
            if (arrayLength <= 0)
                return ArrayPool<string>.Shared.Rent(0);
            string[] astrReturn = ArrayPool<string>.Shared.Rent(arrayLength);
            try
            {
                int intIndex = 0;
                for (int intStart = 0; intStart < intInputLength; intStart += intLoopLength + 1)
                {
                    intLoopLength = strInput.IndexOf(chrSplit, intStart);
                    if (intLoopLength < 0)
                        intLoopLength = intInputLength;
                    intLoopLength -= intStart;
                    if (intLoopLength != 0)
                        astrReturn[intIndex++] = strInput.Substring(intStart, intLoopLength);
                    else if (eSplitOptions == StringSplitOptions.None)
                        astrReturn[intIndex++] = string.Empty;
                }
            }
            catch
            {
                ArrayPool<string>.Shared.Return(astrReturn);
                throw;
            }
            return astrReturn;
        }

        /// <summary>
        /// Version of string::Split() that returns a pooled string array, reducing overall allocations at the cost of needing to pay attention to disposal
        /// </summary>
        /// <param name="strInput">Input textblock.</param>
        /// <param name="arrayLength">Length of the returned array. Needs to be stored and handled separately because we cannot guarantee that a pooled array will not be longer than necessary for the split.</param>
        /// <param name="chrSplit">Character to use for splitting.</param>
        /// <param name="intCount">The maximum number of substrings to return.</param>
        /// <param name="eSplitOptions">Optional argument that can be used to skip over empty entries.</param>
        /// <returns>String array rented from ArrayPool{string}.Shared containing substrings of <paramref name="strInput"/> split based on <paramref name="chrSplit"/></returns>
        public static string[] SplitToPooledArray(this string strInput, out int arrayLength, char chrSplit, int intCount,
                                                       StringSplitOptions eSplitOptions = StringSplitOptions.None)
        {
            arrayLength = 0;
            if (intCount <= 0)
                return ArrayPool<string>.Shared.Rent(0);
            if (string.IsNullOrEmpty(strInput))
            {
                if (eSplitOptions == StringSplitOptions.None)
                {
                    arrayLength = 1;
                    string[] astrFastReturn = ArrayPool<string>.Shared.Rent(1);
                    astrFastReturn[0] = string.Empty;
                    return astrFastReturn;
                }
                else
                    return ArrayPool<string>.Shared.Rent(0);
            }
            int intInputLength = strInput.Length;
            int intLoopLength;
            for (int intStart = 0; intStart < intInputLength; intStart += intLoopLength + 1)
            {
                if (arrayLength >= intCount)
                    break;
                intLoopLength = strInput.IndexOf(chrSplit, intStart);
                if (intLoopLength < 0)
                    intLoopLength = intInputLength;
                intLoopLength -= intStart;
                if (intLoopLength != 0 || eSplitOptions == StringSplitOptions.None)
                    ++arrayLength;
            }
            if (arrayLength <= 0)
                return ArrayPool<string>.Shared.Rent(0);
            string[] astrReturn = ArrayPool<string>.Shared.Rent(arrayLength);
            try
            {
                int intIndex = 0;
                for (int intStart = 0; intStart < intInputLength; intStart += intLoopLength + 1)
                {
                    if (arrayLength >= intCount)
                        break;
                    intLoopLength = strInput.IndexOf(chrSplit, intStart);
                    if (intLoopLength < 0)
                        intLoopLength = intInputLength;
                    intLoopLength -= intStart;
                    if (intLoopLength != 0)
                        astrReturn[intIndex++] = strInput.Substring(intStart, intLoopLength);
                    else if (eSplitOptions == StringSplitOptions.None)
                        astrReturn[intIndex++] = string.Empty;
                }
            }
            catch
            {
                ArrayPool<string>.Shared.Return(astrReturn);
                throw;
            }
            return astrReturn;
        }

        /// <summary>
        /// Version of string::Split() that returns a pooled string array, reducing overall allocations at the cost of needing to pay attention to disposal
        /// </summary>
        /// <param name="strInput">Input textblock.</param>
        /// <param name="arrayLength">Length of the returned array. Needs to be stored and handled separately because we cannot guarantee that a pooled array will not be longer than necessary for the split.</param>
        /// <param name="strSplit">String to use for splitting.</param>
        /// <param name="eSplitOptions">Optional argument that can be used to skip over empty entries.</param>
        /// <param name="eComparison">Comparison to use when searching for the next instance of <paramref name="strSplit"/>.</param>
        /// <returns>String array rented from ArrayPool{string}.Shared containing substrings of <paramref name="strInput"/> split based on <paramref name="strSplit"/></returns>
        public static string[] SplitToPooledArray(this string strInput, out int arrayLength, string strSplit,
                                                       StringSplitOptions eSplitOptions = StringSplitOptions.None, StringComparison eComparison = StringComparison.Ordinal)
        {
            arrayLength = 0;
            if (string.IsNullOrEmpty(strInput))
            {
                if (eSplitOptions == StringSplitOptions.None)
                {
                    arrayLength = 1;
                    string[] astrFastReturn = ArrayPool<string>.Shared.Rent(1);
                    astrFastReturn[0] = string.Empty;
                    return astrFastReturn;
                }
                else
                    return ArrayPool<string>.Shared.Rent(0);
            }
            if (string.IsNullOrEmpty(strSplit))
            {
                string[] astrFastReturn = ArrayPool<string>.Shared.Rent(1);
                astrFastReturn[0] = strInput;
                return astrFastReturn;
            }
            int intInputLength = strInput.Length;
            int intSplitLength = strSplit.Length;
            int intLoopLength;
            for (int intStart = 0; intStart < intInputLength; intStart += intLoopLength + intSplitLength)
            {
                intLoopLength = strInput.IndexOf(strSplit, intStart, eComparison);
                if (intLoopLength < 0)
                    intLoopLength = intInputLength;
                intLoopLength -= intStart;
                if (intLoopLength != 0 || eSplitOptions == StringSplitOptions.None)
                    ++arrayLength;
            }
            if (arrayLength <= 0)
                return ArrayPool<string>.Shared.Rent(0);
            string[] astrReturn = ArrayPool<string>.Shared.Rent(arrayLength);
            try
            {
                int intIndex = 0;
                for (int intStart = 0; intStart < intInputLength; intStart += intLoopLength + intSplitLength)
                {
                    intLoopLength = strInput.IndexOf(strSplit, intStart, eComparison);
                    if (intLoopLength < 0)
                        intLoopLength = intInputLength;
                    intLoopLength -= intStart;
                    if (intLoopLength != 0)
                        astrReturn[intIndex++] = strInput.Substring(intStart, intLoopLength);
                    else if (eSplitOptions == StringSplitOptions.None)
                        astrReturn[intIndex++] = string.Empty;
                }
            }
            catch
            {
                ArrayPool<string>.Shared.Return(astrReturn);
                throw;
            }
            return astrReturn;
        }

        /// <summary>
        /// Version of string::Split() that returns a pooled string array, reducing overall allocations at the cost of needing to pay attention to disposal
        /// </summary>
        /// <param name="strInput">Input textblock.</param>
        /// <param name="arrayLength">Length of the returned array. Needs to be stored and handled separately because we cannot guarantee that a pooled array will not be longer than necessary for the split.</param>
        /// <param name="strSplit">String to use for splitting.</param>
        /// <param name="intCount">The maximum number of substrings to return.</param>
        /// <param name="eSplitOptions">Optional argument that can be used to skip over empty entries.</param>
        /// <param name="eComparison">Comparison to use when searching for the next instance of <paramref name="strSplit"/>.</param>
        /// <returns>String array rented from ArrayPool{string}.Shared containing substrings of <paramref name="strInput"/> split based on <paramref name="strSplit"/></returns>
        public static string[] SplitToPooledArray(this string strInput, out int arrayLength, string strSplit, int intCount,
                                                       StringSplitOptions eSplitOptions = StringSplitOptions.None, StringComparison eComparison = StringComparison.Ordinal)
        {
            arrayLength = 0;
            if (intCount <= 0)
                return ArrayPool<string>.Shared.Rent(0);
            if (string.IsNullOrEmpty(strInput))
            {
                if (eSplitOptions == StringSplitOptions.None)
                {
                    arrayLength = 1;
                    string[] astrFastReturn = ArrayPool<string>.Shared.Rent(1);
                    astrFastReturn[0] = string.Empty;
                    return astrFastReturn;
                }
                else
                    return ArrayPool<string>.Shared.Rent(0);
            }
            if (string.IsNullOrEmpty(strSplit))
            {
                arrayLength = 1;
                string[] astrFastReturn = ArrayPool<string>.Shared.Rent(1);
                astrFastReturn[0] = strInput;
                return astrFastReturn;
            }
            int intInputLength = strInput.Length;
            int intSplitLength = strSplit.Length;
            int intLoopLength;
            for (int intStart = 0; intStart < intInputLength; intStart += intLoopLength + intSplitLength)
            {
                if (arrayLength >= intCount)
                    break;
                intLoopLength = strInput.IndexOf(strSplit, intStart, eComparison);
                if (intLoopLength < 0)
                    intLoopLength = intInputLength;
                intLoopLength -= intStart;
                if (intLoopLength != 0 || eSplitOptions == StringSplitOptions.None)
                    ++arrayLength;
            }
            if (arrayLength <= 0)
                return ArrayPool<string>.Shared.Rent(0);
            string[] astrReturn = ArrayPool<string>.Shared.Rent(arrayLength);
            try
            {
                int intIndex = 0;
                for (int intStart = 0; intStart < intInputLength; intStart += intLoopLength + intSplitLength)
                {
                    if (arrayLength >= intCount)
                        break;
                    intLoopLength = strInput.IndexOf(strSplit, intStart, eComparison);
                    if (intLoopLength < 0)
                        intLoopLength = intInputLength;
                    intLoopLength -= intStart;
                    if (intLoopLength != 0)
                        astrReturn[intIndex++] = strInput.Substring(intStart, intLoopLength);
                    else if (eSplitOptions == StringSplitOptions.None)
                        astrReturn[intIndex++] = string.Empty;
                }
            }
            catch
            {
                ArrayPool<string>.Shared.Return(astrReturn);
                throw;
            }
            return astrReturn;
        }

        /// <summary>
        /// Version of string::Split() that avoids allocations where possible, thus making it lighter on memory (and also on CPU because allocations take time) than all versions of string::Split()
        /// </summary>
        /// <param name="strInput">Input textblock.</param>
        /// <param name="arrayLength">Length of the returned array. Needs to be stored and handled separately because we cannot guarantee that a pooled array will not be longer than necessary for the split.</param>
        /// <param name="achrSplit">Characters to use for splitting.</param>
        /// <returns>String array rented from ArrayPool{string}.Shared containing substrings of <paramref name="strInput"/> split based on <paramref name="achrSplit"/></returns>
        public static string[] SplitToPooledArray(this string strInput, out int arrayLength, params char[] achrSplit)
        {
            arrayLength = 0;
            if (string.IsNullOrEmpty(strInput))
            {
                arrayLength = 1;
                string[] astrFastReturn = ArrayPool<string>.Shared.Rent(1);
                astrFastReturn[0] = string.Empty;
                return astrFastReturn;
            }
            if (achrSplit.Length == 0)
            {
                arrayLength = 1;
                string[] astrFastReturn = ArrayPool<string>.Shared.Rent(1);
                astrFastReturn[0] = strInput;
                return astrFastReturn;
            }
            int intInputLength = strInput.Length;
            int intLoopLength;
            for (int intStart = 0; intStart < intInputLength; intStart += intLoopLength + 1)
            {
                intLoopLength = strInput.IndexOfAny(achrSplit, intStart);
                if (intLoopLength < 0)
                    intLoopLength = intInputLength;
                intLoopLength -= intStart;
                ++arrayLength;
            }
            if (arrayLength <= 0)
                return ArrayPool<string>.Shared.Rent(0);
            string[] astrReturn = ArrayPool<string>.Shared.Rent(arrayLength);
            try
            {
                int intIndex = 0;
                for (int intStart = 0; intStart < intInputLength; intStart += intLoopLength + 1)
                {
                    intLoopLength = strInput.IndexOfAny(achrSplit, intStart);
                    if (intLoopLength < 0)
                        intLoopLength = intInputLength;
                    intLoopLength -= intStart;
                    if (intLoopLength != 0)
                        astrReturn[intIndex++] = strInput.Substring(intStart, intLoopLength);
                    else
                        astrReturn[intIndex++] = string.Empty;
                }
            }
            catch
            {
                ArrayPool<string>.Shared.Return(astrReturn);
                throw;
            }
            return astrReturn;
        }

        /// <summary>
        /// Special version of SplitNoAlloc that is meant for processing command-line arguments (where we are supposed to ignore spaces inside of quotation mark blocks)
        /// </summary>
        /// <param name="strInput">String to process.</param>
        /// <returns>Enumerable containing substrings of <paramref name="strInput"/> split by whitespace</returns>
        public static IEnumerable<string> ProcessArgsString(this string strInput)
        {
            if (string.IsNullOrEmpty(strInput))
                yield break;
            int intInputLength = strInput.Length;
            int intLoopLength;
            for (int intStart = 0; intStart < intInputLength; intStart += intLoopLength + 1)
            {
                intLoopLength = strInput.IndexOfAny(s_achrWhiteSpaceAndQuotes, intStart);
                if (intLoopLength < 0)
                    intLoopLength = intInputLength;
                else
                {
                    while (!char.IsWhiteSpace(strInput, intLoopLength))
                    {
                        intLoopLength = strInput.IndexOf('"', intLoopLength + 1);
                        if (intLoopLength < 0)
                        {
                            intLoopLength = intInputLength;
                            break;
                        }
                        intLoopLength = strInput.IndexOfAny(s_achrWhiteSpaceAndQuotes, intLoopLength + 1);
                        if (intLoopLength < 0)
                        {
                            intLoopLength = intInputLength;
                            break;
                        }
                    }
                }
                intLoopLength -= intStart;
                if (intLoopLength != 0)
                    yield return strInput.Substring(intStart, intLoopLength);
            }
        }

        /// <summary>
        /// Normalizes whitespace for a given textblock, removing extra spaces and trimming the string.
        /// </summary>
        /// <param name="strInput">Input textblock</param>
        /// <param name="funcIsWhiteSpace">Custom function with which to check if a character should count as whitespace. If null, defaults to char::IsWhiteSpace && !char::IsControl.</param>
        /// <returns>New string with any chars that return true from <paramref name="funcIsWhiteSpace"/> replaced with the first whitespace in a sequence and any excess whitespace removed.</returns>
        public static string NormalizeWhiteSpace(this string strInput, Func<char, bool> funcIsWhiteSpace = null)
        {
            if (strInput == null)
                return string.Empty;
            int intLength = strInput.Length;
            if (intLength == 0)
                return strInput;
            if (funcIsWhiteSpace == null)
                funcIsWhiteSpace = x => char.IsWhiteSpace(x) && !char.IsControl(x);
            if (intLength > GlobalSettings.MaxStackLimit16BitTypes)
            {
                string strReturn;
                using (new FetchSafelyFromArrayPool<char>(ArrayPool<char>.Shared, intLength, out char[] achrNewChars))
                {
                    // What we're going here is copying the string-as-CharArray char-by-char into a new CharArray, but processing whitespace characters differently...
                    int intCurrent = 0;
                    int intLoopWhitespaceCount = 0;
                    bool blnTrimMode = true;
                    char chrLastAddedCharacter = ' ';
                    for (int i = 0; i < intLength; ++i)
                    {
                        char chrLoop = strInput[i];
                        // If we encounter a block of identical whitespace chars, we replace the first instance with chrWhiteSpace, then skip over the rest until we encounter a char that isn't whitespace
                        if (funcIsWhiteSpace(chrLoop))
                        {
                            ++intLoopWhitespaceCount;
                            if (chrLastAddedCharacter != chrLoop && !blnTrimMode)
                            {
                                achrNewChars[intCurrent++] = chrLoop;
                                chrLastAddedCharacter = chrLoop;
                            }
                        }
                        else
                        {
                            intLoopWhitespaceCount = 0;
                            blnTrimMode = false;
                            achrNewChars[intCurrent++] = chrLoop;
                            chrLastAddedCharacter = chrLoop;
                        }
                    }

                    // If all we had was whitespace, return a string with just a single space character
                    if (intLoopWhitespaceCount >= intCurrent)
                    {
                        return " ";
                    }

                    // ... then we create a new string from the new CharArray, but only up to the number of characters that actually ended up getting copied.
                    // If the last char is whitespace, we don't copy that, either.
                    strReturn = new string(achrNewChars, 0, intCurrent - intLoopWhitespaceCount);
                }

                return strReturn;
            }

            // Stackalloc is faster than a heap-allocated array, but string constructor requires use of unsafe context because there are no overloads for Span<char>
            unsafe
            {
                // Create CharArray in which we will store the new string
                char* achrNewChars = stackalloc char[intLength];
                int intCurrent = 0;
                int intLoopWhitespaceCount = 0;
                bool blnTrimMode = true;
                char chrLastAddedCharacter = ' ';
                for (int i = 0; i < intLength; ++i)
                {
                    char chrLoop = strInput[i];
                    // If we encounter a block of identical whitespace chars, we replace the first instance with chrWhiteSpace, then skip over the rest until we encounter a char that isn't whitespace
                    if (funcIsWhiteSpace(chrLoop))
                    {
                        ++intLoopWhitespaceCount;
                        if (chrLastAddedCharacter != chrLoop && !blnTrimMode)
                        {
                            achrNewChars[intCurrent++] = chrLoop;
                            chrLastAddedCharacter = chrLoop;
                        }
                    }
                    else
                    {
                        intLoopWhitespaceCount = 0;
                        blnTrimMode = false;
                        achrNewChars[intCurrent++] = chrLoop;
                        chrLastAddedCharacter = chrLoop;
                    }
                }

                return intLoopWhitespaceCount >= intCurrent
                    // If all we had was whitespace, return a string with just a single space character
                    ? " "
                    // ... then we create a new string from the new CharArray, but only up to the number of characters that actually ended up getting copied.
                    // If the last char is whitespace, we don't copy that, either.
                    : new string(achrNewChars, 0, intCurrent - intLoopWhitespaceCount);
            }
        }

        /// <summary>
        /// Returns whether a string contains only legal characters.
        /// </summary>
        /// <param name="strInput">String to check.</param>
        /// <param name="blnWhitelist">Whether the list of chars is a whitelist and the string can only contain characters in the list (true) or a blacklist and the string cannot contain any characts in the list (false).</param>
        /// <param name="achrChars">List of chars against which to check the string.</param>
        /// <returns>True if the string contains only legal characters, false if the string contains at least one illegal character.</returns>
        public static bool IsLegalCharsOnly(this string strInput, bool blnWhitelist, params char[] achrChars)
        {
            return IsLegalCharsOnly(strInput, blnWhitelist, Array.AsReadOnly(achrChars));
        }

        /// <summary>
        /// Returns whether a string contains only legal characters.
        /// </summary>
        /// <param name="strInput">String to check.</param>
        /// <param name="blnWhitelist">Whether the list of chars is a whitelist and the string can only contain characters in the list (true) or a blacklist and the string cannot contain any characts in the list (false).</param>
        /// <param name="achrChars">List of chars against which to check the string.</param>
        /// <returns>True if the string contains only legal characters, false if the string contains at least one illegal character.</returns>
        public static bool IsLegalCharsOnly(this string strInput, bool blnWhitelist, IReadOnlyCollection<char> achrChars)
        {
            if (strInput == null)
                return false;
            int intLength = strInput.Length;
            if (intLength == 0)
                return true;
            int intLegalCharsLength = achrChars.Count;
            if (intLegalCharsLength == 0)
                return true;
            for (int i = 0; i < intLength; ++i)
            {
                char chrLoop = strInput[i];
                bool blnCharIsInList = false;
                for (int j = 0; j < intLegalCharsLength; ++j)
                {
                    if (chrLoop == achrChars.ElementAtBetter(j))
                    {
                        blnCharIsInList = true;
                        break;
                    }
                }

                if (blnCharIsInList != blnWhitelist)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Trims a substring out of the beginning of a string. If the substring appears multiple times at the beginning, all instances of it will be trimmed.
        /// </summary>
        /// <param name="strInput">String on which to operate</param>
        /// <param name="strToTrim">Substring to trim</param>
        /// <param name="eComparison">Comparison rules by which to find the substring to remove. Useful for when case-insensitive removal is required.</param>
        /// <returns>Trimmed String</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string TrimStart(this string strInput, string strToTrim,
                                       StringComparison eComparison = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(strInput) || string.IsNullOrEmpty(strToTrim))
                return strInput;
            int intTrimLength = strToTrim.Length;
            if (intTrimLength == 1)
                return strInput.TrimStart(strToTrim[0]);

            int i = strInput.IndexOf(strToTrim, eComparison);
            if (i == -1)
                return strInput;

            int intAmountToTrim = 0;
            do
            {
                intAmountToTrim += intTrimLength;
                i = strInput.IndexOf(strToTrim, intAmountToTrim, eComparison);
            } while (i != -1);

            return strInput.Substring(intAmountToTrim);
        }

        /// <summary>
        /// Trims a substring out of the end of a string. If the substring appears multiple times at the end, all instances of it will be trimmed.
        /// </summary>
        /// <param name="strInput">String on which to operate</param>
        /// <param name="strToTrim">Substring to trim</param>
        /// <param name="eComparison">Comparison rules by which to find the substring to remove. Useful for when case-insensitive removal is required.</param>
        /// <returns>Trimmed String</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string TrimEnd(this string strInput, string strToTrim,
                                     StringComparison eComparison = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(strInput) || string.IsNullOrEmpty(strToTrim))
                return strInput;
            int intTrimLength = strToTrim.Length;
            if (intTrimLength == 1)
                return strInput.TrimEnd(strToTrim[0]);

            int i = strInput.LastIndexOf(strToTrim, eComparison);
            if (i == -1)
                return strInput;

            int intInputLastIndex = strInput.Length - 1;
            int intAmountToTrim = 0;
            do
            {
                intAmountToTrim += intTrimLength;
                i = strInput.LastIndexOf(strToTrim, intInputLastIndex - intAmountToTrim, eComparison);
            } while (i != -1);

            return strInput.Substring(0, intInputLastIndex - intTrimLength);
        }

        /// <summary>
        /// Escapes a substring once out of a string if the string begins with it.
        /// </summary>
        /// <param name="strInput">String on which to operate</param>
        /// <param name="strToTrim">Substring to escape</param>
        /// <param name="blnOmitCheck">If we already know that the string begins with the substring</param>
        /// <returns>String with <paramref name="strToTrim"/> escaped out once from the beginning of it.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string TrimStartOnce(this string strInput, string strToTrim, bool blnOmitCheck = false)
        {
            if (!string.IsNullOrEmpty(strInput) && !string.IsNullOrEmpty(strToTrim)
                                                // Need to make sure string actually starts with the substring, otherwise we don't want to be cutting out the beginning of the string
                                                && (blnOmitCheck
                                                    || strInput.StartsWith(strToTrim, StringComparison.Ordinal)))
            {
                return strInput.Substring(strToTrim.Length);
            }

            return strInput;
        }

        /// <summary>
        /// If a string begins with any substrings, the one with which it begins is trimmed out of the string once.
        /// </summary>
        /// <param name="strInput">String on which to operate</param>
        /// <param name="astrToTrim">Substrings to trim</param>
        /// <returns>Trimmed String</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string TrimStartOnce(this string strInput, params string[] astrToTrim)
        {
            if (!string.IsNullOrEmpty(strInput) && astrToTrim != null)
            {
                // Without this we could trim a smaller string just because it was found first, this makes sure we find the largest one
                int intHowMuchToTrim = 0;

                int intLength = astrToTrim.Length;
                for (int i = 0; i < intLength; ++i)
                {
                    string strStringToTrim = astrToTrim[i];
                    // Need to make sure string actually starts with the substring, otherwise we don't want to be cutting out the beginning of the string
                    if (strStringToTrim.Length > intHowMuchToTrim
                        && strInput.StartsWith(strStringToTrim, StringComparison.Ordinal))
                    {
                        intHowMuchToTrim = strStringToTrim.Length;
                    }
                }

                if (intHowMuchToTrim > 0)
                    return strInput.Substring(intHowMuchToTrim);
            }

            return strInput;
        }

        /// <summary>
        /// Escapes a char once out of a string if the string begins with it.
        /// </summary>
        /// <param name="strInput">String on which to operate</param>
        /// <param name="chrToTrim">Char to escape</param>
        /// <returns>String with <paramref name="chrToTrim"/> escaped out once from the beginning of it.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string TrimStartOnce(this string strInput, char chrToTrim)
        {
            return !string.IsNullOrEmpty(strInput) && strInput[0] == chrToTrim
                ? strInput.Substring(1)
                : strInput;
        }

        /// <summary>
        /// If a string begins with any chars, the one with which it begins is trimmed out of the string once.
        /// </summary>
        /// <param name="strInput">String on which to operate</param>
        /// <param name="achrToTrim">Chars to trim</param>
        /// <returns>Trimmed String</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string TrimStartOnce(this string strInput, params char[] achrToTrim)
        {
            return !string.IsNullOrEmpty(strInput) && strInput.StartsWith(achrToTrim)
                ? strInput.Substring(1)
                : strInput;
        }

        /// <summary>
        /// Trims a substring out of a string if the string ends with it.
        /// </summary>
        /// <param name="strInput">String on which to operate</param>
        /// <param name="strToTrim">Substring to trim</param>
        /// <param name="blnOmitCheck">If we already know that the string ends with the substring</param>
        /// <returns>Trimmed String</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string TrimEndOnce(this string strInput, string strToTrim, bool blnOmitCheck = false)
        {
            if (!string.IsNullOrEmpty(strInput) && !string.IsNullOrEmpty(strToTrim)
                                                // Need to make sure string actually ends with the substring, otherwise we don't want to be cutting out the end of the string
                                                && (blnOmitCheck
                                                    || strInput.EndsWith(strToTrim, StringComparison.Ordinal)))
            {
                return strInput.Substring(0, strInput.Length - strToTrim.Length);
            }

            return strInput;
        }

        /// <summary>
        /// If a string ends with any substrings, the one with which it begins is trimmed out of the string once.
        /// </summary>
        /// <param name="strInput">String on which to operate</param>
        /// <param name="astrToTrim">Substrings to trim</param>
        /// <returns>Trimmed String</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string TrimEndOnce(this string strInput, params string[] astrToTrim)
        {
            if (!string.IsNullOrEmpty(strInput) && astrToTrim != null)
            {
                // Without this we could trim a smaller string just because it was found first, this makes sure we find the largest one
                int intHowMuchToTrim = 0;

                int intLength = astrToTrim.Length;
                for (int i = 0; i < intLength; ++i)
                {
                    string strStringToTrim = astrToTrim[i];
                    // Need to make sure string actually ends with the substring, otherwise we don't want to be cutting out the end of the string
                    if (strStringToTrim.Length > intHowMuchToTrim
                        && strInput.EndsWith(strStringToTrim, StringComparison.Ordinal))
                    {
                        intHowMuchToTrim = strStringToTrim.Length;
                    }
                }

                if (intHowMuchToTrim > 0)
                    return strInput.Substring(0, strInput.Length - intHowMuchToTrim);
            }

            return strInput;
        }

        /// <summary>
        /// Trims a char out of a string if the string ends with it.
        /// </summary>
        /// <param name="strInput">String on which to operate</param>
        /// <param name="chrToTrim">Char to trim</param>
        /// <returns>Trimmed String</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string TrimEndOnce(this string strInput, char chrToTrim)
        {
            if (!string.IsNullOrEmpty(strInput))
            {
                int intLength = strInput.Length;
                if (strInput[intLength - 1] == chrToTrim)
                    return strInput.Substring(0, intLength - 1);
            }

            return strInput;
        }

        /// <summary>
        /// If a string ends with any chars, the one with which it begins is trimmed out of the string once.
        /// </summary>
        /// <param name="strInput">String on which to operate</param>
        /// <param name="achrToTrim">Chars to trim</param>
        /// <returns>Trimmed String</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string TrimEndOnce(this string strInput, params char[] achrToTrim)
        {
            if (!string.IsNullOrEmpty(strInput) && strInput.EndsWith(achrToTrim))
                return strInput.Substring(0, strInput.Length - 1);
            return strInput;
        }

        /// <summary>
        /// Determines whether the first char of this string instance matches the specified char.
        /// </summary>
        /// <param name="strInput">String to check.</param>
        /// <param name="chrToCheck">Char to check.</param>
        /// <returns>True if string has a non-zero length and begins with the char, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool StartsWith(this string strInput, char chrToCheck)
        {
            return strInput?.Length > 0 && strInput[0] == chrToCheck;
        }

        /// <summary>
        /// Determines whether the first char of this string instance matches any of the specified chars.
        /// </summary>
        /// <param name="strInput">String to check.</param>
        /// <param name="achrToCheck">Chars to check.</param>
        /// <returns>True if string has a non-zero length and begins with any of the specified chars, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool StartsWith(this string strInput, params char[] achrToCheck)
        {
            if (string.IsNullOrEmpty(strInput) || achrToCheck == null)
                return false;
            char chrCharToCheck = strInput[0];
            int intParamsLength = achrToCheck.Length;
            for (int i = 0; i < intParamsLength; ++i)
            {
                if (chrCharToCheck == achrToCheck[i])
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the beginning of this string instance matches any of the specified strings.
        /// </summary>
        /// <param name="strInput">String to check.</param>
        /// <param name="astrToCheck">Strings to check.</param>
        /// <returns>True if string has a non-zero length and begins with any of the specified chars, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool StartsWith(this string strInput, params string[] astrToCheck)
        {
            if (!string.IsNullOrEmpty(strInput) && astrToCheck != null)
            {
                int intLength = astrToCheck.Length;
                for (int i = 0; i < intLength; ++i)
                {
                    if (strInput.StartsWith(astrToCheck[i], StringComparison.Ordinal))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether the last char of this string instance matches the specified char.
        /// </summary>
        /// <param name="strInput">String to check.</param>
        /// <param name="chrToCheck">Char to check.</param>
        /// <returns>True if string has a non-zero length and ends with the char, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EndsWith(this string strInput, char chrToCheck)
        {
            if (strInput == null)
                return false;
            int intLength = strInput.Length;
            return intLength > 0 && strInput[intLength - 1] == chrToCheck;
        }

        /// <summary>
        /// Determines whether the last char of this string instance matches any of the specified chars.
        /// </summary>
        /// <param name="strInput">String to check.</param>
        /// <param name="achrToCheck">Chars to check.</param>
        /// <returns>True if string has a non-zero length and ends with any of the specified chars, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EndsWith(this string strInput, params char[] achrToCheck)
        {
            if (strInput == null || achrToCheck == null)
                return false;
            int intLength = strInput.Length;
            if (intLength == 0)
                return false;
            char chrCharToCheck = strInput[intLength - 1];
            int intParamsLength = achrToCheck.Length;
            for (int i = 0; i < intParamsLength; ++i)
            {
                if (chrCharToCheck == achrToCheck[i])
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the end of this string instance matches any of the specified strings.
        /// </summary>
        /// <param name="strInput">String to check.</param>
        /// <param name="astrToCheck">Strings to check.</param>
        /// <returns>True if string has a non-zero length and ends with any of the specified chars, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EndsWith(this string strInput, params string[] astrToCheck)
        {
            if (!string.IsNullOrEmpty(strInput) && astrToCheck != null)
            {
                int intLength = astrToCheck.Length;
                for (int i = 0; i < intLength; ++i)
                {
                    if (strInput.EndsWith(astrToCheck[i], StringComparison.Ordinal))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Like string::Replace(), but meant for if the new value would be expensive to calculate. Actually slower than string::Replace() if the new value is something simple.
        /// If the string does not contain any instances of the pattern to replace, then the expensive method to generate a replacement is not run.
        /// </summary>
        /// <param name="strInput">Base string in which the replacing takes place.</param>
        /// <param name="strOldValue">Pattern for which to check and which to replace.</param>
        /// <param name="funcNewValueFactory">Function to generate the string that replaces the pattern in the base string.</param>
        /// <param name="eStringComparison">The StringComparison to use for finding and replacing items.</param>
        /// <returns>The result of a string::Replace() method if a replacement is made, the original string otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string CheapReplace(this string strInput, string strOldValue, Func<string> funcNewValueFactory,
                                          StringComparison eStringComparison = StringComparison.Ordinal)
        {
            if (!string.IsNullOrEmpty(strInput) && funcNewValueFactory != null)
            {
                if (eStringComparison == StringComparison.Ordinal)
                {
                    if (strInput.Contains(strOldValue))
                        return strInput.Replace(strOldValue, funcNewValueFactory.Invoke());
                }
                else if (strInput.IndexOf(strOldValue, eStringComparison) != -1)
                    return strInput.Replace(strOldValue, funcNewValueFactory.Invoke(), eStringComparison);
            }

            return strInput;
        }

        /// <summary>
        /// Like string::Replace(), but meant for if the new value would be expensive to calculate. Actually slower than string::Replace() if the new value is something simple.
        /// This is the async version that can be run in case a value is really expensive to get.
        /// If the string does not contain any instances of the pattern to replace, then the expensive method to generate a replacement is not run.
        /// </summary>
        /// <param name="strInput">Base string in which the replacing takes place.</param>
        /// <param name="strOldValue">Pattern for which to check and which to replace.</param>
        /// <param name="funcNewValueFactory">Function to generate the string that replaces the pattern in the base string.</param>
        /// <param name="eStringComparison">The StringComparison to use for finding and replacing items.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>The result of a string::Replace() method if a replacement is made, the original string otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<string> CheapReplaceAsync(this string strInput, string strOldValue,
                                                           Func<string> funcNewValueFactory,
                                                           StringComparison eStringComparison
                                                               = StringComparison.Ordinal,
                                                           CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!string.IsNullOrEmpty(strInput) && funcNewValueFactory != null)
            {
                if (eStringComparison == StringComparison.Ordinal)
                {
                    if (strInput.Contains(strOldValue))
                    {
                        token.ThrowIfCancellationRequested();
                        string strFactoryResult = string.Empty;
                        using (CancellationTokenTaskSource<string> objCancelTaskSource
                               = new CancellationTokenTaskSource<string>(token))
                        {
                            await Task.WhenAny(Task.Factory.FromAsync(funcNewValueFactory.BeginInvoke,
                                                                      x => strFactoryResult
                                                                          = funcNewValueFactory.EndInvoke(x), null),
                                               objCancelTaskSource.Task).ConfigureAwait(false);
                        }

                        token.ThrowIfCancellationRequested();
                        return strInput.Replace(strOldValue, strFactoryResult);
                    }
                }
                else if (strInput.IndexOf(strOldValue, eStringComparison) != -1)
                {
                    token.ThrowIfCancellationRequested();
                    string strFactoryResult = string.Empty;
                    using (CancellationTokenTaskSource<string> objCancelTaskSource
                           = new CancellationTokenTaskSource<string>(token))
                    {
                        await Task.WhenAny(Task.Factory.FromAsync(funcNewValueFactory.BeginInvoke,
                                                                  x => strFactoryResult
                                                                      = funcNewValueFactory.EndInvoke(x), null),
                                           objCancelTaskSource.Task).ConfigureAwait(false);
                    }

                    token.ThrowIfCancellationRequested();
                    return strInput.Replace(strOldValue, strFactoryResult, eStringComparison);
                }
            }

            return strInput;
        }

        /// <summary>
        /// Like string::Replace(), but meant for if the new value would be expensive to calculate. Actually slower than string::Replace() if the new value is something simple.
        /// This is the async version that can be run in case a value is really expensive to get.
        /// If the string does not contain any instances of the pattern to replace, then the expensive method to generate a replacement is not run.
        /// </summary>
        /// <param name="strInputTask">Task returning the base string in which the replacing takes place.</param>
        /// <param name="strOldValue">Pattern for which to check and which to replace.</param>
        /// <param name="funcNewValueFactory">Function to generate the string that replaces the pattern in the base string.</param>
        /// <param name="eStringComparison">The StringComparison to use for finding and replacing items.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>The result of a string::Replace() method if a replacement is made, the original string otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<string> CheapReplaceAsync(this ValueTask<string> strInputTask, string strOldValue,
                                                           Func<string> funcNewValueFactory,
                                                           StringComparison eStringComparison
                                                               = StringComparison.Ordinal,
                                                           CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return await CheapReplaceAsync(await strInputTask.ConfigureAwait(false), strOldValue, funcNewValueFactory,
                                           eStringComparison, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Like string::Replace(), but meant for if the new value would be expensive to calculate. Actually slower than string::Replace() if the new value is something simple.
        /// This is the async version that can be run in case a value is really expensive to get.
        /// If the string does not contain any instances of the pattern to replace, then the expensive method to generate a replacement is not run.
        /// </summary>
        /// <param name="strInputTask">Task returning the base string in which the replacing takes place.</param>
        /// <param name="strOldValue">Pattern for which to check and which to replace.</param>
        /// <param name="funcNewValueFactory">Function to generate the string that replaces the pattern in the base string.</param>
        /// <param name="eStringComparison">The StringComparison to use for finding and replacing items.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>The result of a string::Replace() method if a replacement is made, the original string otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<string> CheapReplaceAsync(this Task<string> strInputTask, string strOldValue,
                                                           Func<string> funcNewValueFactory,
                                                           StringComparison eStringComparison
                                                               = StringComparison.Ordinal,
                                                           CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return await CheapReplaceAsync(await strInputTask.ConfigureAwait(false), strOldValue, funcNewValueFactory,
                                           eStringComparison, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Like string::Replace(), but meant for if the new value would be expensive to calculate. Actually slower than string::Replace() if the new value is something simple.
        /// This is the async version that can be run in case a value is really expensive to get.
        /// If the string does not contain any instances of the pattern to replace, then the expensive method to generate a replacement is not run.
        /// </summary>
        /// <param name="strInput">Base string in which the replacing takes place.</param>
        /// <param name="strOldValue">Pattern for which to check and which to replace.</param>
        /// <param name="funcNewValueFactory">Function to generate the string that replaces the pattern in the base string.</param>
        /// <param name="eStringComparison">The StringComparison to use for finding and replacing items.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>The result of a string::Replace() method if a replacement is made, the original string otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<string> CheapReplaceAsync(this string strInput, string strOldValue,
                                                           Func<Task<string>> funcNewValueFactory,
                                                           StringComparison eStringComparison
                                                               = StringComparison.Ordinal,
                                                           CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!string.IsNullOrEmpty(strInput) && funcNewValueFactory != null)
            {
                if (eStringComparison == StringComparison.Ordinal)
                {
                    if (strInput.Contains(strOldValue))
                    {
                        token.ThrowIfCancellationRequested();
                        string strNewValue = await funcNewValueFactory.Invoke().ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        return strInput.Replace(strOldValue, strNewValue);
                    }
                }
                else if (strInput.IndexOf(strOldValue, eStringComparison) != -1)
                {
                    token.ThrowIfCancellationRequested();
                    string strNewValue = await funcNewValueFactory.Invoke().ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    return strInput.Replace(strOldValue, strNewValue, eStringComparison);
                }
            }

            return strInput;
        }

        /// <summary>
        /// Like string::Replace(), but meant for if the new value would be expensive to calculate. Actually slower than string::Replace() if the new value is something simple.
        /// This is the async version that can be run in case a value is really expensive to get.
        /// If the string does not contain any instances of the pattern to replace, then the expensive method to generate a replacement is not run.
        /// </summary>
        /// <param name="strInputTask">Task returning the base string in which the replacing takes place.</param>
        /// <param name="strOldValue">Pattern for which to check and which to replace.</param>
        /// <param name="funcNewValueFactory">Function to generate the string that replaces the pattern in the base string.</param>
        /// <param name="eStringComparison">The StringComparison to use for finding and replacing items.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>The result of a string::Replace() method if a replacement is made, the original string otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<string> CheapReplaceAsync(this ValueTask<string> strInputTask, string strOldValue,
                                                           Func<Task<string>> funcNewValueFactory,
                                                           StringComparison eStringComparison
                                                               = StringComparison.Ordinal,
                                                           CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return await CheapReplaceAsync(await strInputTask.ConfigureAwait(false), strOldValue, funcNewValueFactory,
                                           eStringComparison, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Like string::Replace(), but meant for if the new value would be expensive to calculate. Actually slower than string::Replace() if the new value is something simple.
        /// This is the async version that can be run in case a value is really expensive to get.
        /// If the string does not contain any instances of the pattern to replace, then the expensive method to generate a replacement is not run.
        /// </summary>
        /// <param name="strInputTask">Task returning the base string in which the replacing takes place.</param>
        /// <param name="strOldValue">Pattern for which to check and which to replace.</param>
        /// <param name="funcNewValueFactory">Function to generate the string that replaces the pattern in the base string.</param>
        /// <param name="eStringComparison">The StringComparison to use for finding and replacing items.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>The result of a string::Replace() method if a replacement is made, the original string otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<string> CheapReplaceAsync(this Task<string> strInputTask, string strOldValue,
                                                           Func<Task<string>> funcNewValueFactory,
                                                           StringComparison eStringComparison
                                                               = StringComparison.Ordinal,
                                                           CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return await CheapReplaceAsync(await strInputTask.ConfigureAwait(false), strOldValue, funcNewValueFactory,
                                           eStringComparison, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Tests whether a given string is a Guid. Returns false if not.
        /// </summary>
        /// <param name="strGuid">String to test.</param>
        /// <returns>True if string is a Guid, false if not.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsGuid(this string strGuid)
        {
            return Guid.TryParse(strGuid, out Guid _);
        }

        private static readonly Dictionary<string, string> s_DicLigaturesMap = new Dictionary<string, string>
        {
            {"ﬀ", "ff"},
            {"ﬃ", "ffi"},
            {"ﬄ", "ffl"},
            {"ﬁ", "fi"},
            {"ﬂ", "fl"},
            // Some PDF fonts have this control character defined as the "fi" ligature for some reason.
            // It's dumb and will cause XML errors, so it definitely has to be replaced/cleaned.
            {'\u001f'.ToString(), "fi"}
        };

        /// <summary>
        /// Replace some of the bad ligatures that are present in Shadowrun sourcebooks with proper characters
        /// </summary>
        /// <param name="strInput">String to clean.</param>
        /// <returns>Cleaned string with bad ligatures replaced with full latin characters</returns>
        public static string CleanStylisticLigatures(this string strInput)
        {
            if (string.IsNullOrEmpty(strInput))
                return strInput;
            string strReturn = strInput;
            foreach (KeyValuePair<string, string> kvpLigature in s_DicLigaturesMap)
                strReturn = strReturn.Replace(kvpLigature.Key, kvpLigature.Value);
            return strReturn;
        }

        /// <summary>
        /// Replace some of the bad ligatures that are present in Shadowrun sourcebooks with proper characters
        /// </summary>
        /// <param name="sbdInput">StringBuilder to clean.</param>
        public static StringBuilder CleanStylisticLigatures(this StringBuilder sbdInput)
        {
            if (sbdInput == null)
                throw new ArgumentNullException(nameof(sbdInput));
            foreach (KeyValuePair<string, string> kvpLigature in s_DicLigaturesMap)
                sbdInput.Replace(kvpLigature.Key, kvpLigature.Value);
            return sbdInput;
        }

        /// <summary>
        /// Word wraps the given text to fit within the specified width.
        /// </summary>
        /// <param name="strText">Text to be word wrapped</param>
        /// <param name="intWidth">Width, in characters, to which the text should be word wrapped</param>
        /// <returns>The modified text</returns>
        public static string WordWrap(this string strText, int intWidth = 128)
        {
            // Lucidity checks
            if (string.IsNullOrEmpty(strText))
                return strText;
            if (intWidth >= strText.Length)
                return strText;

            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdReturn))
            {
                int intNewCapacity = strText.Length;
                if (sbdReturn.Capacity < intNewCapacity)
                    sbdReturn.Capacity = intNewCapacity;
                string strNewLine = Environment.NewLine;
                // Parse each line of text
                int intNextPosition;
                for (int intCurrentPosition = 0;
                     intCurrentPosition < strText.Length;
                     intCurrentPosition = intNextPosition)
                {
                    // Find end of line
                    int intEndOfLinePosition
                        = strText.IndexOf(strNewLine, intCurrentPosition, StringComparison.Ordinal);
                    if (intEndOfLinePosition == -1)
                        intNextPosition = intEndOfLinePosition = strText.Length;
                    else
                        intNextPosition = intEndOfLinePosition + strNewLine.Length;

                    // Copy this line of text, breaking into smaller lines as needed
                    if (intEndOfLinePosition > intCurrentPosition)
                    {
                        do
                        {
                            int intLengthToRead = intEndOfLinePosition - intCurrentPosition;
                            if (intLengthToRead > intWidth)
                                intLengthToRead = strText.BreakLine(intCurrentPosition, intWidth);
                            sbdReturn.Append(strText, intCurrentPosition, intLengthToRead).AppendLine();

                            // Trim whitespace following break
                            intCurrentPosition += intLengthToRead;
                            while (intCurrentPosition < intEndOfLinePosition
                                   && char.IsWhiteSpace(strText[intCurrentPosition])
                                   && !char.IsControl(strText[intCurrentPosition]))
                                ++intCurrentPosition;
                        } while (intEndOfLinePosition > intCurrentPosition);
                    }
                    else
                        sbdReturn.AppendLine(); // Empty line
                }

                return sbdReturn.ToString();
            }
        }

        /// <summary>
        /// Checks if every letter in a string is uppercase or not.
        /// </summary>
        public static bool IsAllLettersUpperCase(this string strText)
        {
            if (strText == null)
                throw new ArgumentNullException(nameof(strText));
            return string.IsNullOrEmpty(strText) || strText.All(x => !char.IsLetter(x) || char.IsUpper(x));
        }

        /// <summary>
        /// Locates position to break the given line so as to avoid
        /// breaking words.
        /// </summary>
        /// <param name="strText">String that contains line of text</param>
        /// <param name="intPosition">Index where line of text starts</param>
        /// <param name="intMax">Maximum line length</param>
        /// <returns>The modified line length</returns>
        private static int BreakLine(this string strText, int intPosition, int intMax)
        {
            if (strText == null)
                return intMax;
            if (intMax + intPosition >= strText.Length)
                return intMax;
            // Find last whitespace in line
            for (int i = intMax; i >= 0; --i)
            {
                char chrLoop = strText[intPosition + i];
                if (!char.IsControl(chrLoop)
                    && chrLoop != '\u00A0' // Non-breaking spaces should not break lines
                    && chrLoop != '\u202F' // Non-breaking spaces should not break lines
                    && chrLoop != '\uFEFF' // Non-breaking spaces should not break lines
                    && (char.IsWhiteSpace(chrLoop)
                        || chrLoop == '\u00AD')) // Soft hyphens allow breakage
                {
                    // Return length of text before whitespace
                    return i + 1;
                }
            }

            // If no whitespace found, break at maximum length
            return intMax;
        }

        /// <summary>
        /// Normalizes line endings to always be that of Environment.NewLine.
        /// </summary>
        /// <param name="strInput">String to normalize.</param>
        /// <param name="blnEscaped">If the line endings in the string are defined in an escaped fashion (e.g. as "\\n"), set to true.</param>
        /// <returns></returns>
        public static string NormalizeLineEndings(this string strInput, bool blnEscaped = false)
        {
            if (string.IsNullOrEmpty(strInput))
                return strInput;
            string[] astrLineEndingStrings = blnEscaped ? s_astrEscapedLineEndingStrings : s_astrLineEndingStrings;
            if (!strInput.ContainsAnyParallel(astrLineEndingStrings))
                return strInput;
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdReturn))
            {
                sbdReturn.Append(strInput);
                // Two-step process so that newlines normalized in earlier iterations of the loop are not re-detected by later iterations
                // Yes, this will replace null characters with newlines as well, too bad, we shouldn't have null characters in our strings anyway
                foreach (string strSequence in astrLineEndingStrings)
                    sbdReturn.Replace(strSequence, "\u0000");
                sbdReturn.Replace("\u0000", Environment.NewLine);
                return sbdReturn.ToString();
            }
        }

        /// <summary>
        /// Clean a string for usage inside an XPath filter, also surrounding it with quotation marks in an appropriate way.
        /// </summary>
        /// <param name="strSearch">String to clean.</param>
        public static string CleanXPath(this string strSearch)
        {
            if (string.IsNullOrEmpty(strSearch))
                return "\"\"";
            int intQuotePos = strSearch.IndexOf('"');
            if (intQuotePos == -1)
            {
                return '\"' + strSearch + '\"';
            }

            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdReturn))
            {
                int intNewCapacity = strSearch.Length + 10;
                if (sbdReturn.Capacity < intNewCapacity)
                    sbdReturn.Capacity = intNewCapacity;
                sbdReturn.Append("concat(\"");
                int intSubStringStart = 0;
                for (; intQuotePos != -1; intQuotePos = strSearch.IndexOf('"', intSubStringStart))
                {
                    sbdReturn.Append(strSearch, intSubStringStart, intQuotePos - intSubStringStart)
                             .Append("\", '\"', \"");
                    intSubStringStart = intQuotePos + 1;
                }

                return sbdReturn.Append(strSearch, intSubStringStart, strSearch.Length - intSubStringStart)
                                .Append("\")").ToString();
            }
        }

        // Order is important so that we replace composites before chars
        private static readonly string[] s_astrStringsToProcessForHtmlConversion = new[] { "&", "&amp;amp;", "<", ">", "\r\n", "\n\r", "\n", "\r" };

        /// <summary>
        /// Escapes characters in a string that would cause confusion if the string were placed as HTML content
        /// </summary>
        /// <param name="strToClean">String to clean.</param>
        /// <returns>Copy of <paramref name="strToClean"/> with the characters "&", the greater than sign, and the lesser than sign escaped for HTML.</returns>
        public static string CleanForHtml(this string strToClean)
        {
            if (string.IsNullOrEmpty(strToClean))
                return string.Empty;
            if (!strToClean.ContainsAnyParallel(s_astrStringsToProcessForHtmlConversion))
                return strToClean;
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdReturn))
            {
                sbdReturn.Append(strToClean);
                sbdReturn.Replace("&", "&amp;")
                    .Replace("&amp;amp;", "&amp;")
                    .Replace("<", "&lt;")
                    .Replace(">", "&gt;");
                foreach (string strSequence in s_astrLineEndingStrings)
                    sbdReturn.Replace(strSequence, "<br />");
                return sbdReturn.ToString();
            }
        }

        private static readonly ReadOnlyCollection<char> s_achrPathInvalidPathChars
            = Array.AsReadOnly(Path.GetInvalidPathChars());

        private static readonly ReadOnlyCollection<char> s_achrPathInvalidFileNameChars
            = Array.AsReadOnly(Path.GetInvalidFileNameChars());

        /// <summary>
        /// Replaces all the characters in a string that are invalid for file names with underscores.
        /// </summary>
        /// <param name="strToClean">String to clean.</param>
        /// <param name="blnEscapeOnlyPathInvalidChars">If true, only characters that are invalid in path names will be replaced with underscores.</param>
        /// <returns>Copy of <paramref name="strToClean"/> with all characters that are not valid for file names replaced with underscores.</returns>
        public static string CleanForFileName(this string strToClean, bool blnEscapeOnlyPathInvalidChars = false)
        {
            if (string.IsNullOrEmpty(strToClean))
                return string.Empty;
            foreach (char invalidChar in blnEscapeOnlyPathInvalidChars
                         ? s_achrPathInvalidPathChars
                         : s_achrPathInvalidFileNameChars)
                strToClean = strToClean.Replace(invalidChar, '_');
            return strToClean;
        }

        /// <summary>
        /// Surrounds a plaintext string with basic RTF formatting so that it can be processed as an RTF string
        /// </summary>
        /// <param name="strInput">String to process</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>Version of <paramref name="strInput"/> surrounded with RTF formatting codes</returns>
        public static string PlainTextToRtf(this string strInput, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(strInput))
                return string.Empty;
            if (strInput.IsRtf())
                return strInput;
            strInput = strInput.NormalizeWhiteSpace();
            s_RtbRtfManipulatorLock.SafeWait(token);
            try
            {
                if (!s_RtbRtfManipulator.Value.IsHandleCreated)
                {
                    Utils.RunOnMainThread(() => s_RtbRtfManipulator.Value.CreateControl(), token: token);
                }

                return s_RtbRtfManipulator.Value.DoThreadSafeFunc(x =>
                {
                    x.Text = strInput;
                    return x.Rtf;
                }, token);
            }
            finally
            {
                s_RtbRtfManipulatorLock.Release();
            }
        }

        /// <summary>
        /// Surrounds a plaintext string with basic RTF formatting so that it can be processed as an RTF string
        /// </summary>
        /// <param name="strInput">String to process</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>Version of <paramref name="strInput"/> surrounded with RTF formatting codes</returns>
        public static Task<string> PlainTextToRtfAsync(this string strInput, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return string.IsNullOrEmpty(strInput) ? Task.FromResult(string.Empty) : InnerDo();

            async Task<string> InnerDo()
            {
                if (strInput.IsRtf())
                    return strInput;
                strInput = strInput.NormalizeWhiteSpace();
                await s_RtbRtfManipulatorLock.WaitAsync(token).ConfigureAwait(false);
                try
                {
                    if (!s_RtbRtfManipulator.Value.IsHandleCreated)
                    {
                        await Utils.RunOnMainThreadAsync(() => s_RtbRtfManipulator.Value.CreateControl(), token)
                                   .ConfigureAwait(false);
                    }

                    return await s_RtbRtfManipulator.Value.DoThreadSafeFuncAsync(x =>
                    {
                        x.Text = strInput;
                        return x.Rtf;
                    }, token).ConfigureAwait(false);
                }
                finally
                {
                    s_RtbRtfManipulatorLock.Release();
                }
            }
        }

        /// <summary>
        /// Strips RTF formatting from a string
        /// </summary>
        /// <param name="strInput">String to process</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>Version of <paramref name="strInput"/> without RTF formatting codes</returns>
        public static string RtfToPlainText(this string strInput, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(strInput))
                return string.Empty;
            string strInputTrimmed = strInput.TrimStart();
            string strReturn = strInputTrimmed.StartsWith("{/rtf1", StringComparison.Ordinal)
                               || strInputTrimmed.StartsWith(@"{\rtf1", StringComparison.Ordinal)
                ? strInput.StripRichTextFormat(token)
                : strInput;

            return strReturn.NormalizeWhiteSpace();
        }

        /// <summary>
        /// Strips RTF formatting from a string
        /// </summary>
        /// <param name="strInput">String to process</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>Version of <paramref name="strInput"/> without RTF formatting codes</returns>
        public static Task<string> RtfToPlainTextAsync(this string strInput, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<string>(token);
            if (string.IsNullOrEmpty(strInput))
                return Task.FromResult(string.Empty);
            return Task.Run(() =>
            {
                string strInputTrimmed = strInput.TrimStart();
                string strReturn = strInputTrimmed.StartsWith("{/rtf1", StringComparison.Ordinal)
                                   || strInputTrimmed.StartsWith(@"{\rtf1", StringComparison.Ordinal)
                    ? strInput.StripRichTextFormat(token)
                    : strInput;

                return strReturn.NormalizeWhiteSpace();
            }, token);
        }

        public static string RtfToHtml(this string strInput, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(strInput))
                return string.Empty;
            string strReturn = strInput.IsRtf() ? Rtf.ToHtml(strInput) : strInput.CleanForHtml();
            return strReturn.CleanStylisticLigatures().NormalizeWhiteSpace().CleanOfXmlInvalidUnicodeChars();
        }

        public static Task<string> RtfToHtmlAsync(this string strInput, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<string>(token);
            if (string.IsNullOrEmpty(strInput))
                return Task.FromResult(string.Empty);
            return Task.Run(() =>
            {
                string strReturn = strInput.IsRtf()
                    ? Rtf.ToHtml(strInput)
                    : strInput.CleanForHtml();
                return strReturn.CleanStylisticLigatures().NormalizeWhiteSpace().CleanOfXmlInvalidUnicodeChars();
            }, token);
        }

        /// <summary>
        /// Takes a string with RTF formatting and transforms all of its colors to dark mode versions.
        /// </summary>
        /// <param name="strInput">String to process with light mode colors.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>Version of <paramref name="strInput"/> with RTF color codes replaced with dark mode versions.</returns>
        public static string RtfToDarkMode(this string strInput, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(strInput))
                return string.Empty;
            string strInputTrimmed = strInput.TrimStart();
            if (!strInputTrimmed.StartsWith("{/rtf1", StringComparison.Ordinal) && !strInputTrimmed.StartsWith(@"{\rtf1", StringComparison.Ordinal))
                return strInput; // Faster and dirtier version of the full IsRtf check for performance reasons
            int intColorTableStart = strInput.IndexOf(@"{\colortbl;", StringComparison.Ordinal);
            if (intColorTableStart < 0)
                return strInput;
            int intColorTableEnd = strInput.IndexOf('}', intColorTableStart);
            if (intColorTableEnd < 0 || intColorTableStart - intColorTableEnd <= 11)
                return strInput;
            string strInputColorTable = strInput.Substring(intColorTableStart, intColorTableEnd - intColorTableStart - 11);
            if (string.IsNullOrEmpty(strInputColorTable))
                return strInput;
            MatchCollection lstColorMatches = s_RtfColorsRegex.Value.Matches(strInputColorTable);
            if (lstColorMatches.Count == 0)
                return strInput;
            string strInputPreColorTable = strInput.Substring(0, intColorTableStart);
            string strInputPostColorTable = intColorTableEnd + 1 < strInput.Length ? strInput.Substring(intColorTableEnd + 1) : string.Empty;
            Dictionary<string, string> dicColorReplacements = new Dictionary<string, string>(lstColorMatches.Count);
            foreach (Match objColorEntry in lstColorMatches)
            {
                if (dicColorReplacements.ContainsKey(objColorEntry.Value))
                    continue;
                GroupCollection lstColorValues = objColorEntry.Groups;
                if (lstColorValues.Count < 3
                    || !int.TryParse(lstColorValues[0].Value, out int intRed)
                    || !int.TryParse(lstColorValues[1].Value, out int intGreen)
                    || !int.TryParse(lstColorValues[2].Value, out int intBlue))
                    continue;
                Color objExistingColor = Color.FromArgb(intRed, intGreen, intBlue);
                Color objDarkModeColor = ColorManager.GenerateDarkModeColor(objExistingColor);
                dicColorReplacements.Add(objColorEntry.Value, "\\red" + objDarkModeColor.R.ToString(GlobalSettings.InvariantCultureInfo)
                    + "\\green" + objDarkModeColor.G.ToString(GlobalSettings.InvariantCultureInfo)
                    + "\\blue" + objDarkModeColor.B.ToString(GlobalSettings.InvariantCultureInfo) + ';');
            }
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdInputColorTable))
            {
                sbdInputColorTable.Append(strInputColorTable);
                foreach (KeyValuePair<string, string> kvpReplace in dicColorReplacements)
                {
                    sbdInputColorTable.Replace(kvpReplace.Key, kvpReplace.Value);
                }
                return strInputPreColorTable + sbdInputColorTable.ToString() + strInputPostColorTable;
            }
        }

        /// <summary>
        /// Takes a string with RTF formatting that is assumed to have dark mode formatting and transforms all of its colors to light mode versions.
        /// </summary>
        /// <param name="strInput">String to process with dark mode colors.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>Version of <paramref name="strInput"/> with RTF color codes replaced with light mode versions.</returns>
        public static string RtfInverseToDarkMode(this string strInput, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(strInput))
                return string.Empty;
            string strInputTrimmed = strInput.TrimStart();
            if (!strInputTrimmed.StartsWith("{/rtf1", StringComparison.Ordinal) && !strInputTrimmed.StartsWith(@"{\rtf1", StringComparison.Ordinal))
                return strInput; // Faster and dirtier version of the full IsRtf check for performance reasons
            int intColorTableStart = strInput.IndexOf(@"{\colortbl;", StringComparison.Ordinal);
            if (intColorTableStart < 0)
                return strInput;
            int intColorTableEnd = strInput.IndexOf('}', intColorTableStart);
            if (intColorTableEnd < 0 || intColorTableEnd - intColorTableStart <= 11)
                return strInput;
            string strInputColorTable = strInput.Substring(intColorTableStart, intColorTableEnd - intColorTableStart - 11);
            if (string.IsNullOrEmpty(strInputColorTable))
                return strInput;
            MatchCollection lstColorMatches = s_RtfColorsRegex.Value.Matches(strInputColorTable);
            if (lstColorMatches.Count == 0)
                return strInput;
            string strInputPreColorTable = strInput.Substring(0, intColorTableStart);
            string strInputPostColorTable = intColorTableEnd + 1 < strInput.Length ? strInput.Substring(intColorTableEnd + 1) : string.Empty;
            Dictionary<string, string> dicColorReplacements = new Dictionary<string, string>(lstColorMatches.Count);
            foreach (Match objColorEntry in lstColorMatches)
            {
                if (dicColorReplacements.ContainsKey(objColorEntry.Value))
                    continue;
                GroupCollection lstColorValues = objColorEntry.Groups;
                if (lstColorValues.Count < 3
                    || !int.TryParse(lstColorValues[0].Value, out int intRed)
                    || !int.TryParse(lstColorValues[1].Value, out int intGreen)
                    || !int.TryParse(lstColorValues[2].Value, out int intBlue))
                    continue;
                Color objDarkModeColor = Color.FromArgb(intRed, intGreen, intBlue);
                Color objInvertedColor = ColorManager.GenerateInverseDarkModeColor(objDarkModeColor);
                dicColorReplacements.Add(objColorEntry.Value, "\\red" + objInvertedColor.R.ToString(GlobalSettings.InvariantCultureInfo)
                    + "\\green" + objInvertedColor.G.ToString(GlobalSettings.InvariantCultureInfo)
                    + "\\blue" + objInvertedColor.B.ToString(GlobalSettings.InvariantCultureInfo) + ';');
            }
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdInputColorTable))
            {
                sbdInputColorTable.Append(strInputColorTable);
                foreach (KeyValuePair<string, string> kvpReplace in dicColorReplacements)
                {
                    sbdInputColorTable.Replace(kvpReplace.Key, kvpReplace.Value);
                }
                return strInputPreColorTable + sbdInputColorTable.ToString() + strInputPostColorTable;
            }
        }

        /// <summary>
        /// Whether a string is an RTF document
        /// </summary>
        /// <param name="strInput">The string to check.</param>
        /// <returns>True if <paramref name="strInput"/> is an RTF document, False otherwise.</returns>
        public static bool IsRtf(this string strInput)
        {
            if (string.IsNullOrEmpty(strInput))
                return false;
            string strInputTrimmed = strInput.TrimStart();
            if (strInputTrimmed.StartsWith("{/rtf1", StringComparison.Ordinal)
                || strInputTrimmed.StartsWith(@"{\rtf1", StringComparison.Ordinal))
            {
                return s_RtfStripperRegex.Value.IsMatch(strInputTrimmed);
            }

            return false;
        }

        /// <summary>
        /// Checks if a string contains any HTML tags
        /// </summary>
        /// <param name="strInput">The string to check.</param>
        /// <returns>True if the string contains HTML tags, False otherwise.</returns>
        public static bool ContainsHtmlTags(this string strInput)
        {
            if (string.IsNullOrEmpty(strInput))
                return false;
            int intInputLength = strInput.Length;
            int intIndex = strInput.IndexOf('<');
            if (intIndex < 0 || intIndex + 1 >= intInputLength)
                return false;
            // First check for special tags that are easy to identify: comments and doctypes
            int intCommentOpener = strInput.IndexOf("<!--", intIndex);
            if (intCommentOpener > 0 && intCommentOpener + 7 < intInputLength && strInput.IndexOf("-->", intCommentOpener + 4) > intCommentOpener)
                return true;
            int intDoctypeOpener = strInput.IndexOf("<!DOCTYPE", intIndex);
            if (intDoctypeOpener > 0 && intDoctypeOpener + 10 < intInputLength && strInput.IndexOf('>', intDoctypeOpener + 9) > intDoctypeOpener)
                return true;
            int intClosingIndex = strInput.IndexOf('>', intIndex + 1);
            while (intClosingIndex - intIndex > 1)
            {
                bool blnHasSlash = false;
                bool blnValidTag = true;
                for (int i = intIndex + 1; i < intClosingIndex; ++i)
                {
                    char chrLoop = strInput[i];
                    if (char.IsLetterOrDigit(chrLoop))
                        continue; // Letters are generally allowed, digits have some restrictions but it's too expensive to check for those so let's not.
                    switch (chrLoop)
                    {
                        case ' ':
                            if (i > intIndex + 1)
                                continue;
                            blnValidTag = false;
                            break;
                        case '/':
                            // Slash only allowed as part of a closing tag
                            if (!blnHasSlash && (i == intIndex + 1 || i == intClosingIndex - 1))
                            {
                                blnHasSlash = true;
                                continue;
                            }
                            blnValidTag = false;
                            break;
                        case '=':
                            // Equals signs only valid as part of an attribute assignment
                            if (i > intIndex + 1 && i < intClosingIndex - 1 && strInput[i+1] == '\"' && char.IsLetterOrDigit(strInput[i-1]))
                                continue;
                            blnValidTag = false;
                            break;
                        case '\"':
                            // If we have a quote, skip immediately to the next instance of a quote
                            if (i < intClosingIndex - 1)
                            {
                                int intNextQuote = strInput.IndexOf('\"', i + 1, intClosingIndex - i - 1);
                                if (intNextQuote > i)
                                {
                                    i = intNextQuote;
                                    continue;
                                }
                            }
                            blnValidTag = false;
                            break;
                        default:
                            blnValidTag = false;
                            break;
                    }
                    if (!blnValidTag)
                        break;
                }
                if (blnValidTag)
                    return true;
                if (intClosingIndex + 1 >= intInputLength)
                    return false;
                intIndex = strInput.IndexOf('<', intClosingIndex + 1);
                if (intIndex < 0 || intIndex + 1 >= intInputLength)
                    return false;
                intClosingIndex = strInput.IndexOf('>', intIndex + 1);
            }
            return false;
        }

        /// <summary>
        /// Cleans a string of characters that could cause issues when saved in an xml file and then loaded back in
        /// </summary>
        public static string CleanOfXmlInvalidUnicodeChars(this string strInput)
        {
            return string.IsNullOrEmpty(strInput)
                ? string.Empty
                : strInput.FastEscape(s_achrXmlInvalidUnicodeChars);
        }

        /// <summary>
        /// Checks if a string has characters that could cause issues when saved in an xml file and then loaded back in
        /// </summary>
        public static bool HasAnyXmlInvalidUnicodeChars(this string strInput)
        {
            return !string.IsNullOrEmpty(strInput) && strInput.IndexOfAny(s_achrXmlInvalidUnicodeChars) >= 0;
        }

        /// <summary>
        /// Processes a string containing one or more FixedValues elements to return the appropriate value based on the input rating.
        /// Is also able to handle cases where there are functions with commas inside of the FixedValues string(s).
        /// </summary>
        /// <param name="strInput">String to process (should not have FixedValues trimmed).</param>
        /// <param name="intRating">Rating to use for FixedValues.</param>
        public static string ProcessFixedValuesString(this string strInput, int intRating)
        {
            if (string.IsNullOrEmpty(strInput))
                return string.Empty;
            int intFixedValuesIndex = strInput.IndexOf("FixedValues(", StringComparison.Ordinal);
            if (intFixedValuesIndex < 0)
                return strInput;
            if (intFixedValuesIndex == 0 && strInput[strInput.Length - 1] == ')' && strInput.LastIndexOf("FixedValues(", StringComparison.Ordinal) == 0)
            {
                // Simple case that is the most common, so handle separately: single FixedValues() entry that wraps around the entire string
                strInput = strInput.TrimStartOnce("FixedValues(", true).TrimEndOnce(')');
                int intIndexInner = strInput.IndexOfAny(s_achrOpenParenthesesComma);
                if (intIndexInner < 0)
                    return strInput;
                return ProcessFixedValuesStringCore(strInput, intRating, intIndexInner);
            }
            string strFirstPart = strInput.Substring(0, intFixedValuesIndex);
            string strSecondPart = strInput.Substring(intFixedValuesIndex + 13);
            int intIndex = strSecondPart.IndexOfAny(s_achrParentheses);
            if (intIndex < 0)
            {
                intIndex = strSecondPart.IndexOfAny(s_achrOpenParenthesesComma);
                if (intIndex < 0)
                    return strFirstPart + strSecondPart;
                return strFirstPart + ProcessFixedValuesString(ProcessFixedValuesStringCore(strSecondPart, intRating, intIndex), intRating);
            }
            if (strSecondPart[intIndex] != ')')
            {
                int intNumParentheses = 1;
                while (intNumParentheses > 0)
                {
                    intIndex = strSecondPart.IndexOfAny(s_achrParentheses, intIndex);
                    if (intIndex < 0)
                        break;
                    switch (strSecondPart[intIndex])
                    {
                        case '(':
                            ++intNumParentheses;
                            break;
                        case ')':
                            --intNumParentheses;
                            break;
                    }
                    ++intIndex;
                    if (intNumParentheses == 0)
                    {
                        intIndex = strSecondPart.IndexOfAny(s_achrParentheses, intIndex);
                        if (intIndex < 0 || strSecondPart[intIndex] == ')')
                            break;
                        ++intIndex;
                        ++intNumParentheses;
                    }
                }

                if (intIndex < 0)
                {
                    intIndex = strSecondPart.IndexOfAny(s_achrOpenParenthesesComma);
                    if (intIndex < 0)
                        return strFirstPart + strSecondPart;
                    return strFirstPart + ProcessFixedValuesString(ProcessFixedValuesStringCore(strSecondPart, intRating, intIndex), intRating);
                }
            }

            // Simple case: we just have to process the entire second half of the string as a single FixedValues
            if (intIndex + 1 >= strSecondPart.Length)
            {
                strSecondPart = strSecondPart.Substring(0, intIndex);
                intIndex = strSecondPart.IndexOfAny(s_achrOpenParenthesesComma);
                if (intIndex < 0)
                    return strFirstPart + strSecondPart;
                return strFirstPart + ProcessFixedValuesString(ProcessFixedValuesStringCore(strSecondPart, intRating, intIndex), intRating);
            }

            string strSecondPartA = strSecondPart.Substring(0, intIndex);
            string strSecondPartB = intIndex + 2 < strSecondPart.Length ? strSecondPart.Substring(intIndex + 2) : string.Empty;
            intIndex = strSecondPartA.IndexOfAny(s_achrOpenParenthesesComma);
            if (intIndex < 0)
                return strFirstPart + strSecondPartA + strSecondPartB;
            return strFirstPart + ProcessFixedValuesString(ProcessFixedValuesStringCore(strSecondPartA, intRating, intIndex), intRating) + ProcessFixedValuesString(strSecondPartB, intRating);
        }

        /// <summary>
        /// Processes a string containing one or more FixedValues elements to return the appropriate value based on the input rating.
        /// Is also able to handle cases where there are functions with commas inside of the FixedValues string.
        /// </summary>
        /// <param name="strInput">String to process (should not have FixedValues trimmed).</param>
        /// <param name="funcRating">Function to get the rating to use for FixedValues.</param>
        public static string ProcessFixedValuesString(this string strInput, Func<int> funcRating)
        {
            if (string.IsNullOrEmpty(strInput))
                return string.Empty;
            int intFixedValuesIndex = strInput.IndexOf("FixedValues(", StringComparison.Ordinal);
            if (intFixedValuesIndex < 0)
                return strInput;
            if (intFixedValuesIndex == 0 && strInput[strInput.Length - 1] == ')' && strInput.LastIndexOf("FixedValues(", StringComparison.Ordinal) == 0)
            {
                // Simple case that is the most common, so handle separately: single FixedValues() entry that wraps around the entire string
                strInput = strInput.TrimStartOnce("FixedValues(", true).TrimEndOnce(')');
                int intIndexInner = strInput.IndexOfAny(s_achrOpenParenthesesComma);
                if (intIndexInner < 0)
                    return strInput;
                return ProcessFixedValuesStringCore(strInput, funcRating(), intIndexInner);
            }
            string strFirstPart = strInput.Substring(0, intFixedValuesIndex);
            string strSecondPart = strInput.Substring(intFixedValuesIndex + 13);
            int intIndex = strSecondPart.IndexOfAny(s_achrParentheses);
            if (intIndex < 0)
            {
                intIndex = strSecondPart.IndexOfAny(s_achrOpenParenthesesComma);
                if (intIndex < 0)
                    return strFirstPart + strSecondPart;
                int intRatingInner = funcRating();
                return strFirstPart + ProcessFixedValuesString(ProcessFixedValuesStringCore(strSecondPart, intRatingInner, intIndex), intRatingInner);
            }
            if (strSecondPart[intIndex] != ')')
            {
                int intNumParentheses = 1;
                while (intNumParentheses > 0)
                {
                    intIndex = strSecondPart.IndexOfAny(s_achrParentheses, intIndex);
                    if (intIndex < 0)
                        break;
                    switch (strSecondPart[intIndex])
                    {
                        case '(':
                            ++intNumParentheses;
                            break;
                        case ')':
                            --intNumParentheses;
                            break;
                    }
                    ++intIndex;
                    if (intNumParentheses == 0)
                    {
                        intIndex = strSecondPart.IndexOfAny(s_achrParentheses, intIndex);
                        if (intIndex < 0 || strSecondPart[intIndex] == ')')
                            break;
                        ++intIndex;
                        ++intNumParentheses;
                    }
                }

                if (intIndex < 0)
                {
                    intIndex = strSecondPart.IndexOfAny(s_achrOpenParenthesesComma);
                    if (intIndex < 0)
                        return strFirstPart + strSecondPart;
                    int intRatingInner = funcRating();
                    return strFirstPart + ProcessFixedValuesString(ProcessFixedValuesStringCore(strSecondPart, intRatingInner, intIndex), intRatingInner);
                }
            }

            // Simple case: we just have to process the entire second half of the string as a single FixedValues
            if (intIndex + 1 >= strSecondPart.Length)
            {
                strSecondPart = strSecondPart.Substring(0, intIndex);
                intIndex = strSecondPart.IndexOfAny(s_achrOpenParenthesesComma);
                if (intIndex < 0)
                    return strFirstPart + strSecondPart;
                int intRatingInner = funcRating();
                return strFirstPart + ProcessFixedValuesString(ProcessFixedValuesStringCore(strSecondPart, intRatingInner, intIndex), intRatingInner);
            }

            string strSecondPartA = strSecondPart.Substring(0, intIndex);
            string strSecondPartB = intIndex + 2 < strSecondPart.Length ? strSecondPart.Substring(intIndex + 2) : string.Empty;
            intIndex = strSecondPartA.IndexOfAny(s_achrOpenParenthesesComma);
            if (intIndex < 0)
                return strFirstPart + strSecondPartA + strSecondPartB;
            int intRating = funcRating();
            return strFirstPart + ProcessFixedValuesString(ProcessFixedValuesStringCore(strSecondPartA, intRating, intIndex), intRating) + ProcessFixedValuesString(strSecondPartB, intRating);
        }

        /// <summary>
        /// Processes a string containing one or more FixedValues elements to return the appropriate value based on the input rating.
        /// Is also able to handle cases where there are functions with commas inside of the FixedValues string.
        /// </summary>
        /// <param name="strInput">String to process (should not have FixedValues trimmed).</param>
        /// <param name="funcRating">Function to get the rating to use for FixedValues.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async static Task<string> ProcessFixedValuesStringAsync(this string strInput, Func<Task<int>> funcRating, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(strInput))
                return string.Empty;
            int intFixedValuesIndex = strInput.IndexOf("FixedValues(", StringComparison.Ordinal);
            if (intFixedValuesIndex < 0)
                return strInput;
            if (intFixedValuesIndex == 0 && strInput[strInput.Length - 1] == ')' && strInput.LastIndexOf("FixedValues(", StringComparison.Ordinal) == 0)
            {
                // Simple case that is the most common, so handle separately: single FixedValues() entry that wraps around the entire string
                strInput = strInput.TrimStartOnce("FixedValues(", true).TrimEndOnce(')');
                int intIndexInner = strInput.IndexOfAny(s_achrOpenParenthesesComma);
                if (intIndexInner < 0)
                    return strInput;
                return ProcessFixedValuesStringCore(strInput, await funcRating().ConfigureAwait(false), intIndexInner);
            }
            string strFirstPart = strInput.Substring(0, intFixedValuesIndex);
            string strSecondPart = strInput.Substring(intFixedValuesIndex + 13);
            int intIndex = strSecondPart.IndexOfAny(s_achrParentheses);
            if (intIndex < 0)
            {
                intIndex = strSecondPart.IndexOfAny(s_achrOpenParenthesesComma);
                if (intIndex < 0)
                    return strFirstPart + strSecondPart;
                int intRatingInner = await funcRating().ConfigureAwait(false);
                return strFirstPart + ProcessFixedValuesString(ProcessFixedValuesStringCore(strSecondPart, intRatingInner, intIndex), intRatingInner);
            }
            if (strSecondPart[intIndex] != ')')
            {
                int intNumParentheses = 1;
                while (intNumParentheses > 0)
                {
                    intIndex = strSecondPart.IndexOfAny(s_achrParentheses, intIndex);
                    if (intIndex < 0)
                        break;
                    switch (strSecondPart[intIndex])
                    {
                        case '(':
                            ++intNumParentheses;
                            break;
                        case ')':
                            --intNumParentheses;
                            break;
                    }
                    ++intIndex;
                    if (intNumParentheses == 0)
                    {
                        intIndex = strSecondPart.IndexOfAny(s_achrParentheses, intIndex);
                        if (intIndex < 0 || strSecondPart[intIndex] == ')')
                            break;
                        ++intIndex;
                        ++intNumParentheses;
                    }
                }

                if (intIndex < 0)
                {
                    intIndex = strSecondPart.IndexOfAny(s_achrOpenParenthesesComma);
                    if (intIndex < 0)
                        return strFirstPart + strSecondPart;
                    int intRatingInner = await funcRating().ConfigureAwait(false);
                    return strFirstPart + ProcessFixedValuesString(ProcessFixedValuesStringCore(strSecondPart, intRatingInner, intIndex), intRatingInner);
                }
            }

            // Simple case: we just have to process the entire second half of the string as a single FixedValues
            if (intIndex + 1 >= strSecondPart.Length)
            {
                strSecondPart = strSecondPart.Substring(0, intIndex);
                intIndex = strSecondPart.IndexOfAny(s_achrOpenParenthesesComma);
                if (intIndex < 0)
                    return strFirstPart + strSecondPart;
                int intRatingInner = await funcRating().ConfigureAwait(false);
                return strFirstPart + ProcessFixedValuesString(ProcessFixedValuesStringCore(strSecondPart, intRatingInner, intIndex), intRatingInner);
            }

            string strSecondPartA = strSecondPart.Substring(0, intIndex);
            string strSecondPartB = intIndex + 2 < strSecondPart.Length ? strSecondPart.Substring(intIndex + 2) : string.Empty;
            intIndex = strSecondPartA.IndexOfAny(s_achrOpenParenthesesComma);
            if (intIndex < 0)
                return strFirstPart + strSecondPartA + strSecondPartB;
            int intRating = await funcRating().ConfigureAwait(false);
            return strFirstPart + ProcessFixedValuesString(ProcessFixedValuesStringCore(strSecondPartA, intRating, intIndex), intRating) + ProcessFixedValuesString(strSecondPartB, intRating);
        }

        private static string ProcessFixedValuesStringCore(this string strInput, int intRating, int intIndex)
        {
            if (string.IsNullOrEmpty(strInput))
                return string.Empty;
            if (intIndex < 0)
                return strInput;
            int intInputLength = strInput.Length;
            if (intIndex >= intInputLength)
                return strInput;
            // Function is more complicated than just splitting by commas because we need to be able to ignore commas that are inside of parentheses
            if (intRating == int.MaxValue)
            {
                // Do the same thing as with intRating == 1, but backwards and looking at closed parantheses instead
                intIndex = strInput.LastIndexOfAny(s_achrClosedParenthesesComma);
                if (strInput[intIndex] == ',')
                    return strInput.Substring(intIndex + 1);
                --intIndex;
                int intNumParentheses = 1;
                while (intNumParentheses > 0)
                {
                    intIndex = strInput.LastIndexOfAny(s_achrParentheses, 0, intIndex + 1);
                    // Unclosed parantheses before our last comma, so just seek to last comma and split from there
                    if (intIndex <= 0)
                    {
                        intIndex = strInput.LastIndexOf(',');
                        return intIndex < 0
                            ? strInput
                            : strInput.Substring(intIndex + 1);
                    }
                    switch (strInput[intIndex])
                    {
                        case '(':
                            --intNumParentheses;
                            break;
                        case ')':
                            ++intNumParentheses;
                            break;
                    }
                    --intIndex;
                    if (intNumParentheses == 0)
                    {
                        intIndex = strInput.LastIndexOfAny(s_achrClosedParenthesesComma, 0, intIndex + 1);
                        if (intIndex <= 0)
                            break;
                        if (strInput[intIndex] == ',')
                            return strInput.Substring(intIndex + 1);
                        --intIndex;
                        ++intNumParentheses;
                    }
                }
            }
            else
            {
                if (strInput[intIndex] != ',')
                {
                    ++intIndex;
                    int intNumParentheses = 1;
                    while (intNumParentheses > 0)
                    {
                        intIndex = strInput.IndexOfAny(s_achrParentheses, intIndex);
                        // Unclosed parantheses before our first comma, so return the entire string
                        if (intIndex < 0 || intIndex == intInputLength - 1)
                            break;
                        switch (strInput[intIndex])
                        {
                            case '(':
                                ++intNumParentheses;
                                break;
                            case ')':
                                --intNumParentheses;
                                break;
                        }
                        ++intIndex;
                        if (intNumParentheses == 0)
                        {
                            intIndex = strInput.IndexOfAny(s_achrOpenParenthesesComma, intIndex);
                            if (intIndex < 0 || strInput[intIndex] == ',')
                                break;
                            ++intIndex;
                            ++intNumParentheses;
                        }
                    }
                }
                if (intIndex < 0)
                    return strInput;
                if (intRating <= 1)
                {
                    return strInput.Substring(0, intIndex);
                }
                else
                {
                    int intLastCommaIndex = intIndex;
                    for (int intCurrentCount = 2; intCurrentCount <= intRating; intCurrentCount++)
                    {
                        intIndex = strInput.IndexOfAny(s_achrOpenParenthesesComma, intIndex + 1);
                        if (intIndex < 0 || intIndex == intInputLength - 1)
                            return strInput.Substring(intLastCommaIndex + 1);
                        if (strInput[intIndex] == ',')
                        {
                            if (intCurrentCount == intRating)
                                return strInput.Substring(intLastCommaIndex + 1, intIndex - intLastCommaIndex - 1);
                            intLastCommaIndex = intIndex;
                        }
                        else
                        {
                            ++intIndex;
                            int intNumParentheses = 1;
                            while (intNumParentheses > 0)
                            {
                                intIndex = strInput.IndexOfAny(s_achrParentheses, intIndex);
                                // Unclosed parantheses before our first comma, so skip directly to next comma
                                if (intIndex < 0 || intIndex == intInputLength - 1)
                                {
                                    intIndex = strInput.IndexOf(',', intLastCommaIndex + 1);
                                    if (intIndex < 0 || intIndex == intInputLength - 1)
                                        return strInput.Substring(intLastCommaIndex + 1);
                                    else if (intCurrentCount == intRating)
                                        return strInput.Substring(intLastCommaIndex + 1, intIndex - intLastCommaIndex - 1);
                                    intLastCommaIndex = intIndex;
                                    break;
                                }
                                switch (strInput[intIndex])
                                {
                                    case '(':
                                        ++intNumParentheses;
                                        break;
                                    case ')':
                                        --intNumParentheses;
                                        break;
                                }
                                ++intIndex;
                                if (intNumParentheses == 0)
                                {
                                    intIndex = strInput.IndexOfAny(s_achrOpenParenthesesComma, intIndex);
                                    if (intIndex < 0 || intIndex == intInputLength - 1)
                                        return strInput.Substring(intLastCommaIndex + 1);
                                    if (strInput[intIndex] == ',')
                                    {
                                        if (intCurrentCount == intRating)
                                            return strInput.Substring(intLastCommaIndex + 1, intIndex - intLastCommaIndex - 1);
                                        intLastCommaIndex = intIndex;
                                        break;
                                    }
                                    else
                                    {
                                        ++intIndex;
                                        ++intNumParentheses;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return strInput;
        }

        // Treat as ReadOnlyCollection please, they are only not that because key string methods cannot use a ReadOnlyCollection as their argument
        private static readonly char[] s_achrWhiteSpaceAndQuotes = new[] { ' ', '\u00a0', '\u0085', '\t', '\n', '\v', '\f', '\r', '\"' };
        private static readonly char[] s_achrParentheses = new[] { '(', ')' };
        private static readonly char[] s_achrOpenParenthesesComma = new[] { '(', ',' };
        private static readonly char[] s_achrClosedParenthesesComma = new[] { ')', ',' };
        private static readonly char[] s_achrXmlInvalidUnicodeChars = new[]
        {
            '\u0000',
            '\u0001',
            '\u0002',
            '\u0003',
            '\u0004',
            '\u0005',
            '\u0006',
            '\u0007',
            '\u0008',
            '\u000B',
            '\u000C',
            '\u000E',
            '\u000F',
            '\u0010',
            '\u0011',
            '\u0012',
            '\u0013',
            '\u0014',
            '\u0015',
            '\u0016',
            '\u0017',
            '\u0018',
            '\u0019',
            '\u001A',
            '\u001B',
            '\u001C',
            '\u001D',
            '\u001E',
            '\u001F'
        };

        // Order is important so that we replace composites before chars
        private static readonly string[] s_astrLineEndingStrings = new[] { "\r\n", "\n\r", "\n", "\r" };

        // Order is important so that we replace composites before chars
        private static readonly string[] s_astrEscapedLineEndingStrings = new[] { "\\r\\n", "\\n\\r", "\\n", "\\r" };

        private static readonly DebuggableSemaphoreSlim s_RtbRtfManipulatorLock = new DebuggableSemaphoreSlim();
        private static readonly Lazy<RichTextBox> s_RtbRtfManipulator = new Lazy<RichTextBox>(() => Utils.RunOnMainThread(() => new RichTextBox(), token: CancellationToken.None));

        /// <summary>
        /// Strip RTF Tags from RTF Text.
        /// Translated by Chris Benard (with some modifications from Delnar_Ersike) from Python located at:
        /// http://stackoverflow.com/a/188877/448
        /// </summary>
        /// <param name="inputRtf">RTF formatted text</param>
        /// <param name="token">Cancellation token to use (if any).</param>
        /// <returns>Plain text from RTF</returns>
        public static string StripRichTextFormat(this string inputRtf, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(inputRtf))
            {
                return string.Empty;
            }

            Match objMatch = s_RtfStripperRegex.Value.Match(inputRtf);

            if (!objMatch.Success)
            {
                // Didn't match the regex
                return inputRtf;
            }

            Stack<StackEntry> stkGroups = new Stack<StackEntry>();
            bool blnIgnorable = false; // Whether this group (and all inside it) are "ignorable".
            int intUCSkip = 1; // Number of ASCII characters to skip after a unicode character.
            int intCurSkip = 0; // Number of ASCII characters left to skip

            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdReturn))
            {
                for (; objMatch.Success; objMatch = objMatch.NextMatch())
                {
                    token.ThrowIfCancellationRequested();
                    string strBrace = objMatch.Groups[5].Value;

                    if (!string.IsNullOrEmpty(strBrace))
                    {
                        intCurSkip = 0;
                        switch (strBrace[0])
                        {
                            case '{':
                                // Push state
                                stkGroups.Push(new StackEntry(intUCSkip, blnIgnorable));
                                break;
                            case '}':
                            {
                                // Pop state
                                StackEntry entry = stkGroups.Pop();
                                intUCSkip = entry.NumberOfCharactersToSkip;
                                blnIgnorable = entry.Ignorable;
                                break;
                            }
                        }
                    }
                    else
                    {
                        string strCharacter = objMatch.Groups[4].Value;
                        if (!string.IsNullOrEmpty(strCharacter)) // \x (not a letter)
                        {
                            intCurSkip = 0;
                            char chrLoop = strCharacter[0];
                            if (chrLoop == '~')
                            {
                                if (!blnIgnorable)
                                {
                                    sbdReturn.Append('\xA0');
                                }
                            }
                            else if ("{}\\".Contains(chrLoop))
                            {
                                if (!blnIgnorable)
                                {
                                    sbdReturn.Append(strCharacter);
                                }
                            }
                            else if (chrLoop == '*')
                            {
                                blnIgnorable = true;
                            }
                        }
                        else
                        {
                            string strWord = objMatch.Groups[1].Value;
                            if (!string.IsNullOrEmpty(strWord)) // \foo
                            {
                                intCurSkip = 0;
                                if (s_SetRtfDestinations.Contains(strWord))
                                {
                                    blnIgnorable = true;
                                }
                                else if (!blnIgnorable)
                                {
                                    if (s_DicSpecialRtfCharacters.TryGetValue(strWord, out string strValue))
                                    {
                                        sbdReturn.Append(strValue);
                                    }
                                    else
                                    {
                                        string strArg = objMatch.Groups[2].Value;
                                        switch (strWord)
                                        {
                                            case "uc":
                                                intUCSkip = int.Parse(strArg);
                                                break;
                                            case "u":
                                            {
                                                int c = int.Parse(strArg);
                                                if (c < 0)
                                                {
                                                    c += 0x10000;
                                                }

                                                sbdReturn.Append(char.ConvertFromUtf32(c));
                                                intCurSkip = intUCSkip;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                string strHex = objMatch.Groups[3].Value;
                                if (!string.IsNullOrEmpty(strHex)) // \'xx
                                {
                                    if (intCurSkip > 0)
                                    {
                                        --intCurSkip;
                                    }
                                    else if (!blnIgnorable)
                                    {
                                        int c = int.Parse(strHex, System.Globalization.NumberStyles.HexNumber);
                                        sbdReturn.Append(char.ConvertFromUtf32(c));
                                    }
                                }
                                else
                                {
                                    string strTChar = objMatch.Groups[6].Value;
                                    if (!string.IsNullOrEmpty(strTChar))
                                    {
                                        if (intCurSkip > 0)
                                        {
                                            --intCurSkip;
                                        }
                                        else if (!blnIgnorable)
                                        {
                                            sbdReturn.Append(strTChar);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return sbdReturn.ToString();
            }
        }

        private readonly struct StackEntry
        {
            public int NumberOfCharactersToSkip { get; }
            public bool Ignorable { get; }

            public StackEntry(int numberOfCharactersToSkip, bool ignorable)
            {
                NumberOfCharactersToSkip = numberOfCharactersToSkip;
                Ignorable = ignorable;
            }
        }

        private static readonly Lazy<Regex> s_RtfStripperRegex = new Lazy<Regex>(() => new Regex(
            @"\\([a-z]{1,32})(-?\d{1,10})?[ ]?|\\'([0-9a-f]{2})|\\([^a-z])|([{}])|[\r\n]+|(.)",
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled));

        private static readonly Lazy<Regex> s_RtfColorsRegex = new Lazy<Regex>(() => new Regex(
            @"\\red(\d+)\\green(\d+)\\blue(\d+);",
            RegexOptions.CultureInvariant | RegexOptions.Compiled));

        private static readonly IReadOnlyCollection<string> s_SetRtfDestinations = new HashSet<string>
        {
            "aftncn", "aftnsep", "aftnsepc", "annotation", "atnauthor", "atndate", "atnicn", "atnid",
            "atnparent", "atnref", "atntime", "atrfend", "atrfstart", "author", "background",
            "bkmkend", "bkmkstart", "blipuid", "buptim", "category", "colorschememapping",
            "colortbl", "comment", "company", "creatim", "datafield", "datastore", "defchp", "defpap",
            "do", "doccomm", "docvar", "dptxbxtext", "ebcend", "ebcstart", "factoidname", "falt",
            "fchars", "ffdeftext", "ffentrymcr", "ffexitmcr", "ffformat", "ffhelptext", "ffl",
            "ffname", "ffstattext", "field", "file", "filetbl", "fldinst", "fldrslt", "fldtype",
            "fname", "fontemb", "fontfile", "fonttbl", "footer", "footerf", "footerl", "footerr",
            "footnote", "formfield", "ftncn", "ftnsep", "ftnsepc", "g", "generator", "gridtbl",
            "header", "headerf", "headerl", "headerr", "hl", "hlfr", "hlinkbase", "hlloc", "hlsrc",
            "hsv", "htmltag", "info", "keycode", "keywords", "latentstyles", "lchars", "levelnumbers",
            "leveltext", "lfolevel", "linkval", "list", "listlevel", "listname", "listoverride",
            "listoverridetable", "listpicture", "liststylename", "listtable", "listtext",
            "lsdlockedexcept", "macc", "maccPr", "mailmerge", "maln", "malnScr", "manager", "margPr",
            "mbar", "mbarPr", "mbaseJc", "mbegChr", "mborderBox", "mborderBoxPr", "mbox", "mboxPr",
            "mchr", "mcount", "mctrlPr", "md", "mdeg", "mdegHide", "mden", "mdiff", "mdPr", "me",
            "mendChr", "meqArr", "meqArrPr", "mf", "mfName", "mfPr", "mfunc", "mfuncPr", "mgroupChr",
            "mgroupChrPr", "mgrow", "mhideBot", "mhideLeft", "mhideRight", "mhideTop", "mhtmltag",
            "mlim", "mlimloc", "mlimlow", "mlimlowPr", "mlimupp", "mlimuppPr", "mm", "mmaddfieldname",
            "mmath", "mmathPict", "mmathPr", "mmaxdist", "mmc", "mmcJc", "mmconnectstr",
            "mmconnectstrdata", "mmcPr", "mmcs", "mmdatasource", "mmheadersource", "mmmailsubject",
            "mmodso", "mmodsofilter", "mmodsofldmpdata", "mmodsomappedname", "mmodsoname",
            "mmodsorecipdata", "mmodsosort", "mmodsosrc", "mmodsotable", "mmodsoudl",
            "mmodsoudldata", "mmodsouniquetag", "mmPr", "mmquery", "mmr", "mnary", "mnaryPr",
            "mnoBreak", "mnum", "mobjDist", "moMath", "moMathPara", "moMathParaPr", "mopEmu",
            "mphant", "mphantPr", "mplcHide", "mpos", "mr", "mrad", "mradPr", "mrPr", "msepChr",
            "mshow", "mshp", "msPre", "msPrePr", "msSub", "msSubPr", "msSubSup", "msSubSupPr", "msSup",
            "msSupPr", "mstrikeBLTR", "mstrikeH", "mstrikeTLBR", "mstrikeV", "msub", "msubHide",
            "msup", "msupHide", "mtransp", "mtype", "mvertJc", "mvfmf", "mvfml", "mvtof", "mvtol",
            "mzeroAsc", "mzeroDesc", "mzeroWid", "nesttableprops", "nextfile", "nonesttables",
            "objalias", "objclass", "objdata", "object", "objname", "objsect", "objtime", "oldcprops",
            "oldpprops", "oldsprops", "oldtprops", "oleclsid", "operator", "panose", "password",
            "passwordhash", "pgp", "pgptbl", "picprop", "pict", "pn", "pnseclvl", "pntext", "pntxta",
            "pntxtb", "printim", "private", "propname", "protend", "protstart", "protusertbl", "pxe",
            "result", "revtbl", "revtim", "rsidtbl", "rxe", "shp", "shpgrp", "shpinst",
            "shppict", "shprslt", "shptxt", "sn", "sp", "staticval", "stylesheet", "subject", "sv",
            "svb", "tc", "template", "themedata", "title", "txe", "ud", "upr", "userprops",
            "wgrffmtfilter", "windowcaption", "writereservation", "writereservhash", "xe", "xform",
            "xmlattrname", "xmlattrvalue", "xmlclose", "xmlname", "xmlnstbl",
            "xmlopen"
        };

        private static readonly IReadOnlyDictionary<string, string> s_DicSpecialRtfCharacters = new Dictionary<string, string>
        {
            {"par", "\n"},
            {"sect", "\n\n"},
            {"page", "\n\n"},
            {"line", "\n"},
            {"tab", "\t"},
            {"emdash", "\u2014"},
            {"endash", "\u2013"},
            {"emspace", "\u2003"},
            {"enspace", "\u2002"},
            {"qmspace", "\u2005"},
            {"bullet", "\u2022"},
            {"lquote", "\u2018"},
            {"rquote", "\u2019"},
            {"ldblquote", "\u201C"},
            {"rdblquote", "\u201D"},
        };

        /// <summary>
        /// Converts the specified string, which encodes binary data as base-64 digits, to an equivalent 8-bit unsigned integer array.
        /// Nearly identical to Convert.FromBase64String(), but the byte array that's returned is from a shared ArrayPool instead of newly allocated.
        /// </summary>
        /// <param name="s">The string to convert.</param>
        /// <param name="arrayLength">Actual length of the array used. Important because ArrayPool array can be larger than the lengths requested</param>
        /// <param name="token">Cancellation token to listen to, if any.</param>
        /// <returns>A rented array (from ArrayPool.Shared) of 8-bit unsigned integers that is equivalent to s.</returns>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="FormatException">The length of s, ignoring white-space characters, is not zero or a multiple of 4. -or-The format of s is invalid. s contains a non-base-64 character, more than two padding characters, or a non-white space-character among the padding characters.</exception>
        public static byte[] ToBase64PooledByteArray(this string s, out int arrayLength, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (s == null)
            {
                throw new ArgumentNullException(nameof(s));
            }

            arrayLength = 0;

            unchecked
            {
                unsafe
                {
                    fixed (char* inputPtr = s)
                    {
                        int inputLength = s.Length;
                        while (inputLength > 0)
                        {
                            token.ThrowIfCancellationRequested();
                            int num = inputPtr[inputLength - 1];
                            if (num != 32 && num != 10 && num != 13 && num != 9)
                            {
                                break;
                            }

                            inputLength--;
                        }

                        arrayLength = FromBase64_ComputeResultLength(inputPtr, inputLength);
                        byte[] array = ArrayPool<byte>.Shared.Rent(arrayLength);
                        try
                        {
                            Array.Clear(array, 0, arrayLength);
                            fixed (byte* startDestPtr = array)
                            {
                                _ = FromBase64_Decode(inputPtr, inputLength, startDestPtr, arrayLength);
                            }

                            return array;
                        }
                        catch
                        {
                            ArrayPool<byte>.Shared.Return(array);
                            throw;
                        }
                    }

                    int FromBase64_ComputeResultLength(char* inputPtr, int inputLength)
                    {
                        char* ptr = inputPtr + inputLength;
                        int num = inputLength;
                        int num2 = 0;
                        while (inputPtr < ptr)
                        {
                            token.ThrowIfCancellationRequested();
                            uint num3 = *inputPtr;
                            inputPtr++;
                            switch (num3)
                            {
                                case 0u:
                                case 1u:
                                case 2u:
                                case 3u:
                                case 4u:
                                case 5u:
                                case 6u:
                                case 7u:
                                case 8u:
                                case 9u:
                                case 10u:
                                case 11u:
                                case 12u:
                                case 13u:
                                case 14u:
                                case 15u:
                                case 16u:
                                case 17u:
                                case 18u:
                                case 19u:
                                case 20u:
                                case 21u:
                                case 22u:
                                case 23u:
                                case 24u:
                                case 25u:
                                case 26u:
                                case 27u:
                                case 28u:
                                case 29u:
                                case 30u:
                                case 31u:
                                case 32u:
                                    num--;
                                    break;

                                case 61u:
                                    num--;
                                    num2++;
                                    break;
                            }
                        }

                        switch (num2)
                        {
                            case 1:
                                num2 = 2;
                                break;

                            case 2:
                                num2 = 1;
                                break;

                            case 0:
                                break;

                            default:
                                throw new FormatException(
                                    "The input is not a valid Base-64 string as it contains a non-base 64 character, more than two padding characters, or an illegal character among the padding characters.");
                        }

                        return num / 4 * 3 + num2;
                    }

                    int FromBase64_Decode(char* startInputPtr, int inputLength, byte* startDestPtr, int destLength)
                    {
                        char* ptr = startInputPtr;
                        byte* ptr2 = startDestPtr;
                        char* ptr3 = ptr + inputLength;
                        byte* ptr4 = ptr2 + destLength;
                        uint num = 255u;
                        while (ptr < ptr3)
                        {
                            token.ThrowIfCancellationRequested();
                            uint num2 = *ptr;
                            ptr++;
                            if (num2 - 65 <= 25)
                            {
                                num2 -= 65;
                            }
                            else if (num2 - 97 <= 25)
                            {
                                num2 -= 71;
                            }
                            else if (num2 - 48 <= 9)
                            {
                                num2 -= 4294967292u;
                            }
                            else
                            {
                                if (num2 <= 32)
                                {
                                    if (num2 - 9 <= 1 || num2 == 13 || num2 == 32)
                                    {
                                        continue;
                                    }

                                    throw new FormatException("The input is not a valid Base-64 string as it contains a non-base 64 character, more than two padding characters, or an illegal character among the padding characters.");
                                }

                                if (num2 != 43)
                                {
                                    if (num2 != 47)
                                    {
                                        if (num2 != 61)
                                        {
                                            throw new FormatException("The input is not a valid Base-64 string as it contains a non-base 64 character, more than two padding characters, or an illegal character among the padding characters.");
                                        }

                                        if (ptr == ptr3)
                                        {
                                            num <<= 6;
                                            if ((num & 0x80000000u) == 0)
                                            {
                                                throw new FormatException(
                                                    "Invalid length for a Base-64 char array or string.");
                                            }

                                            if ((int)(ptr4 - ptr2) < 2)
                                            {
                                                return -1;
                                            }

                                            *(ptr2++) = (byte)(num >> 16);
                                            *(ptr2++) = (byte)(num >> 8);
                                            num = 255u;
                                            break;
                                        }

                                        for (; ptr < ptr3 - 1; ptr++)
                                        {
                                            int num3 = *ptr;
                                            if (num3 != 32 && num3 != 10 && num3 != 13 && num3 != 9)
                                            {
                                                break;
                                            }
                                        }

                                        if (ptr == ptr3 - 1 && *ptr == '=')
                                        {
                                            num <<= 12;
                                            if ((num & 0x80000000u) == 0)
                                            {
                                                throw new FormatException(
                                                    "Invalid length for a Base-64 char array or string.");
                                            }

                                            if ((int)(ptr4 - ptr2) < 1)
                                            {
                                                return -1;
                                            }

                                            *(ptr2++) = (byte)(num >> 16);
                                            num = 255u;
                                            break;
                                        }

                                        throw new FormatException(
                                            "The input is not a valid Base-64 string as it contains a non-base 64 character, more than two padding characters, or an illegal character among the padding characters.");
                                    }

                                    num2 = 63u;
                                }
                                else
                                {
                                    num2 = 62u;
                                }
                            }

                            num = (num << 6) | num2;
                            if ((num & 0x80000000u) != 0)
                            {
                                if ((int)(ptr4 - ptr2) < 3)
                                {
                                    return -1;
                                }

                                *ptr2 = (byte)(num >> 16);
                                ptr2[1] = (byte)(num >> 8);
                                ptr2[2] = (byte)num;
                                ptr2 += 3;
                                num = 255u;
                            }
                        }

                        if (num != 255)
                        {
                            throw new FormatException("Invalid length for a Base-64 char array or string.");
                        }

                        return (int)(ptr2 - startDestPtr);
                    }
                }
            }
        }

        /// <summary>
        /// Reads the specified string that encodes binary data as base-64 digits into a stream directly.
        /// Much more memory-efficient version of Convert.FromBase64String() if the byte array would just be immediately fed into a stream anyway.
        /// However, much slower than using Convert.FromBase64String() or ToBase64PooledByteArray() because of unusable optimizations around writing to streams in unsafe code.
        /// </summary>
        /// <param name="s">The string to convert and feed into <paramref name="stream"/>.</param>
        /// <param name="stream">Stream to hold the byte array of the base-64 decoded version of <paramref name="s"/>.</param>
        /// <param name="token">Cancellation token to listen to, if any.</param>
        /// <exception cref="ArgumentNullException">s is null.</exception>
        /// <exception cref="FormatException">The length of s, ignoring white-space characters, is not zero or a multiple of 4. -or-The format of s is invalid. s contains a non-base-64 character, more than two padding characters, or a non-white space-character among the padding characters.</exception>
        public static void ToBase64Stream(this string s, Stream stream, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (s == null)
            {
                throw new ArgumentNullException(nameof(s));
            }

            unchecked
            {
                unsafe
                {
                    fixed (char* inputPtr = s)
                    {
                        int inputLength = s.Length;
                        while (inputLength > 0)
                        {
                            token.ThrowIfCancellationRequested();
                            int num = inputPtr[inputLength - 1];
                            if (num != 32 && num != 10 && num != 13 && num != 9)
                            {
                                break;
                            }

                            inputLength--;
                        }

                        int num2 = FromBase64_ComputeResultLength(inputPtr, inputLength);
                        stream.Position = 0;
                        stream.SetLength(num2);
                        _ = FromBase64_Decode(inputPtr, inputLength, 0, num2);
                    }

                    int FromBase64_ComputeResultLength(char* inputPtr, int inputLength)
                    {
                        char* ptr = inputPtr + inputLength;
                        int num = inputLength;
                        int num2 = 0;
                        while (inputPtr < ptr)
                        {
                            token.ThrowIfCancellationRequested();
                            uint num3 = *inputPtr;
                            inputPtr++;
                            switch (num3)
                            {
                                case 0u:
                                case 1u:
                                case 2u:
                                case 3u:
                                case 4u:
                                case 5u:
                                case 6u:
                                case 7u:
                                case 8u:
                                case 9u:
                                case 10u:
                                case 11u:
                                case 12u:
                                case 13u:
                                case 14u:
                                case 15u:
                                case 16u:
                                case 17u:
                                case 18u:
                                case 19u:
                                case 20u:
                                case 21u:
                                case 22u:
                                case 23u:
                                case 24u:
                                case 25u:
                                case 26u:
                                case 27u:
                                case 28u:
                                case 29u:
                                case 30u:
                                case 31u:
                                case 32u:
                                    num--;
                                    break;

                                case 61u:
                                    num--;
                                    num2++;
                                    break;
                            }
                        }

                        switch (num2)
                        {
                            case 1:
                                num2 = 2;
                                break;

                            case 2:
                                num2 = 1;
                                break;

                            case 0:
                                break;

                            default:
                                throw new FormatException(
                                    "The input is not a valid Base-64 string as it contains a non-base 64 character, more than two padding characters, or an illegal character among the padding characters.");
                        }

                        return num / 4 * 3 + num2;
                    }

                    int FromBase64_Decode(char* startInputPtr, int inputLength, int startPosition, int destLength)
                    {
                        char* ptr = startInputPtr;
                        char* ptr3 = ptr + inputLength;
                        int endPosition = startPosition + destLength;
                        uint num = 255u;
                        while (ptr < ptr3)
                        {
                            token.ThrowIfCancellationRequested();
                            uint num2 = *ptr;
                            ptr++;
                            if (num2 - 65 <= 25)
                            {
                                num2 -= 65;
                            }
                            else if (num2 - 97 <= 25)
                            {
                                num2 -= 71;
                            }
                            else if (num2 - 48 <= 9)
                            {
                                num2 -= 4294967292u;
                            }
                            else
                            {
                                if (num2 <= 32)
                                {
                                    if (num2 - 9 <= 1 || num2 == 13 || num2 == 32)
                                    {
                                        continue;
                                    }

                                    throw new FormatException("The input is not a valid Base-64 string as it contains a non-base 64 character, more than two padding characters, or an illegal character among the padding characters.");
                                }

                                if (num2 != 43)
                                {
                                    if (num2 != 47)
                                    {
                                        if (num2 != 61)
                                        {
                                            throw new FormatException("The input is not a valid Base-64 string as it contains a non-base 64 character, more than two padding characters, or an illegal character among the padding characters.");
                                        }

                                        if (ptr == ptr3)
                                        {
                                            num <<= 6;
                                            if ((num & 0x80000000u) == 0)
                                            {
                                                throw new FormatException(
                                                    "Invalid length for a Base-64 char array or string.");
                                            }

                                            if ((int)(endPosition - stream.Position) < 2)
                                            {
                                                return -1;
                                            }

                                            using (TemporaryArray<byte> aParams = new TemporaryArray<byte>((byte)(num >> 16), (byte)(num >> 8)))
                                                stream.Write(aParams.RawArray, 0, 2);
                                            num = 255u;
                                            break;
                                        }

                                        for (; ptr < ptr3 - 1; ptr++)
                                        {
                                            int num3 = *ptr;
                                            if (num3 != 32 && num3 != 10 && num3 != 13 && num3 != 9)
                                            {
                                                break;
                                            }
                                        }

                                        if (ptr == ptr3 - 1 && *ptr == '=')
                                        {
                                            num <<= 12;
                                            if ((num & 0x80000000u) == 0)
                                            {
                                                throw new FormatException(
                                                    "Invalid length for a Base-64 char array or string.");
                                            }

                                            if ((int)(endPosition - stream.Position) < 1)
                                            {
                                                return -1;
                                            }

                                            stream.WriteByte((byte)(num >> 16));
                                            num = 255u;
                                            break;
                                        }

                                        throw new FormatException(
                                            "The input is not a valid Base-64 string as it contains a non-base 64 character, more than two padding characters, or an illegal character among the padding characters.");
                                    }

                                    num2 = 63u;
                                }
                                else
                                {
                                    num2 = 62u;
                                }
                            }

                            num = (num << 6) | num2;
                            if ((num & 0x80000000u) != 0)
                            {
                                if ((int)(endPosition - stream.Position) < 3)
                                {
                                    return -1;
                                }

                                using (TemporaryArray<byte> aParams = new TemporaryArray<byte>((byte)(num >> 16), (byte)(num >> 8), (byte)num))
                                    stream.Write(aParams.RawArray, 0, 3);
                                num = 255u;
                            }
                        }

                        if (num != 255)
                        {
                            throw new FormatException("Invalid length for a Base-64 char array or string.");
                        }

                        return (int)(stream.Position - startPosition);
                    }
                }
            }
        }
    }
}
