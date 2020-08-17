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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using RtfPipe;

namespace Chummer
{
    public static class StringExtensions
    {
        public static string EmptyGuid { get; } = Guid.Empty.ToString("D", GlobalOptions.InvariantCultureInfo);

        public static bool IsEmptyGuid(this string strInput)
        {
            return strInput == EmptyGuid;
        }

        /// <summary>
        /// Identical to string::Replace(), but the comparison for equality is custom-defined instead of always being case-sensitive Ordinal
        /// </summary>
        /// <param name="strInput">String on which to operate</param>
        /// <param name="strOldValue">Substring to replace</param>
        /// <param name="strNewValue">Substring with which <paramref name="strOldValue"/> gets replaced</param>
        /// <param name="eStringComparison">String Comparison to use when checking for identity</param>
        /// <returns>New string with all instances of <paramref name="strOldValue"/> replaced with <paramref name="strNewValue"/>, but where the equality check was custom-defined by <paramref name="eStringComparison"/></returns>
        public static string Replace(this string strInput, string strOldValue, string strNewValue, StringComparison eStringComparison)
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
            // Buffer size is increased by 1 in addition to the length-dependent stuff in order to compensate for integer division rounding down
            StringBuilder sbdReturn = new StringBuilder(strInput.Length + 1 + Math.Max(0, strInput.Length * (strNewValue.Length - strOldValue.Length) / strOldValue.Length));
            int intEndPositionOfLastReplace = 0;
            // intHead already set to the index of the first instance, for loop's initializer can be left empty
            for (; intHead != -1; intHead = strInput.IndexOf(strOldValue, intEndPositionOfLastReplace, eStringComparison))
            {
                sbdReturn.Append(strInput.Substring(intEndPositionOfLastReplace, intHead - intEndPositionOfLastReplace));
                sbdReturn.Append(strNewValue);
                intEndPositionOfLastReplace = intHead + strOldValue.Length;
            }
            sbdReturn.Append(strInput.Substring(intEndPositionOfLastReplace));
            return sbdReturn.ToString();
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
            if (intLength > GlobalOptions.MaxStackLimit)
            {
                char[] achrNewChars = new char[intLength];
                // What we're going here is copying the string-as-CharArray char-by-char into a new CharArray, but skipping over any instance of chrToDelete...
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
            // Stackalloc is faster than a heap-allocated array, but string constructor requires use of unsafe context because there are no overloads for Span<char>
            unsafe
            {
                char* achrNewChars = stackalloc char[intLength];
                // What we're going here is copying the string-as-CharArray char-by-char into a new CharArray, but skipping over any instance of chrToDelete...
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
            if (intLength > GlobalOptions.MaxStackLimit)
            {
                char[] achrNewChars = new char[intLength];
                // What we're going here is copying the string-as-CharArray char-by-char into a new CharArray, but skipping over any instance of chars in achrToDelete...
                int intCurrent = 0;
                for (int i = 0; i < intLength; ++i)
                {
                    char chrLoop = strInput[i];
                    for (int j = 0; j < intDeleteLength; ++j)
                    {
                        if (chrLoop == achrToDelete[j])
                        {
                            goto SkipChar;
                        }
                    }

                    achrNewChars[intCurrent++] = chrLoop;
                    SkipChar: ;
                }

                // ... then we create a new string from the new CharArray, but only up to the number of characters that actually ended up getting copied
                return new string(achrNewChars, 0, intCurrent);
            }
            // Stackalloc is faster than a heap-allocated array, but string constructor requires use of unsafe context because there are no overloads for Span<char>
            unsafe
            {
                char* achrNewChars = stackalloc char[intLength];
                // What we're going here is copying the string-as-CharArray char-by-char into a new CharArray, but skipping over any instance of chars in achrToDelete...
                int intCurrent = 0;
                for (int i = 0; i < intLength; ++i)
                {
                    char chrLoop = strInput[i];
                    for (int j = 0; j < intDeleteLength; ++j)
                    {
                        if (chrLoop == achrToDelete[j])
                        {
                            goto SkipChar;
                        }
                    }

                    achrNewChars[intCurrent++] = chrLoop;
                    SkipChar:;
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
        public static string FastEscape(this string strInput, string strSubstringToDelete, StringComparison eComparison = StringComparison.Ordinal)
        {
            if (strSubstringToDelete == null)
                return strInput;
            int intToDeleteLength = strSubstringToDelete.Length;
            if (intToDeleteLength == 0)
                return strInput;
            if (intToDeleteLength == 1)
                return strInput.FastEscape(strSubstringToDelete[0]);
            if (strInput == null)
                return string.Empty;
            int intLength = strInput.Length;
            if (intLength < intToDeleteLength)
                return strInput;

            // Quickly exit if no instance of the substring is found
            int intCurrentEnd = strInput.IndexOf(strSubstringToDelete, 0, eComparison);
            if (intCurrentEnd == -1)
                return strInput;

            if (intLength > GlobalOptions.MaxStackLimit)
            {
                // Create CharArray in which we will store the new string
                char[] achrNewChars = new char[intLength];
                // Logic is to read the input string into the CharArray up to the next instance of the substring, then jump over the substring's length and repeat until no more substrings are found
                int intCurrentLength = 0;
                int intCurrentReadPosition = 0;
                do
                {
                    for (; intCurrentReadPosition < intCurrentEnd; ++intCurrentReadPosition)
                    {
                        achrNewChars[intCurrentLength++] = strInput[intCurrentReadPosition];
                    }

                    intCurrentReadPosition += intToDeleteLength;
                    intCurrentEnd = strInput.IndexOf(strSubstringToDelete, intCurrentReadPosition, eComparison);
                } while (intCurrentEnd != -1);

                // Copy the remainder of the string once there are no more needles to remove
                for (; intCurrentReadPosition < intLength; ++intCurrentReadPosition)
                {
                    achrNewChars[intCurrentLength++] = strInput[intCurrentReadPosition];
                }

                // Create a new string from the new CharArray, but only up to the number of characters that actually ended up getting copied
                return new string(achrNewChars, 0, intCurrentLength);
            }
            // Stackalloc is faster than a heap-allocated array, but string constructor requires use of unsafe context because there are no overloads for Span<char>
            unsafe
            {
                // Create CharArray in which we will store the new string
                char* achrNewChars = stackalloc char[intLength];
                // Logic is to read the input string into the CharArray up to the next instance of the substring, then jump over the substring's length and repeat until no more substrings are found
                int intCurrentLength = 0;
                int intCurrentReadPosition = 0;
                do
                {
                    for (; intCurrentReadPosition < intCurrentEnd; ++intCurrentReadPosition)
                    {
                        achrNewChars[intCurrentLength++] = strInput[intCurrentReadPosition];
                    }

                    intCurrentReadPosition += intToDeleteLength;
                    intCurrentEnd = strInput.IndexOf(strSubstringToDelete, intCurrentReadPosition, eComparison);
                } while (intCurrentEnd != -1);

                // Copy the remainder of the string once there are no more needles to remove
                for (; intCurrentReadPosition < intLength; ++intCurrentReadPosition)
                {
                    achrNewChars[intCurrentLength++] = strInput[intCurrentReadPosition];
                }

                // Create a new string from the new CharArray, but only up to the number of characters that actually ended up getting copied
                return new string(achrNewChars, 0, intCurrentLength);
            }
        }

        /// <summary>
        /// Method to quickly remove the first instance of a substring from a string.
        /// </summary>
        /// <param name="strInput">String on which to operate.</param>
        /// <param name="intStartIndex">Index from which to begin searching.</param>
        /// <param name="strSubstringToDelete">Substring to remove.</param>
        /// <param name="eComparison">Comparison rules by which to find the substring to remove. Useful for when case-insensitive removal is required.</param>
        /// <returns>New string with the first instance of <paramref name="strSubstringToDelete"/> removed starting from <paramref name="intStartIndex"/>.</returns>
        public static string FastEscapeOnceFromStart(this string strInput, string strSubstringToDelete, int intStartIndex = 0, StringComparison eComparison = StringComparison.Ordinal)
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
        /// <param name="intStartIndex">Index from which to begin searching (proceeding towards the beginning of the string).</param>
        /// <param name="strSubstringToDelete">Substring to remove.</param>
        /// <param name="eComparison">Comparison rules by which to find the substring to remove. Useful for when case-insensitive removal is required.</param>
        /// <returns>New string with the last instance of <paramref name="strSubstringToDelete"/> removed starting from <paramref name="intStartIndex"/>.</returns>
        public static string FastEscapeOnceFromEnd(this string strInput, string strSubstringToDelete, int intStartIndex = -1, StringComparison eComparison = StringComparison.Ordinal)
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
        /// Syntactic sugar for string::Split that uses one separator char in its argument in addition to StringSplitOptions.
        /// </summary>
        /// <param name="strInput">String to search.</param>
        /// <param name="chrSeparator">Separator to use.</param>
        /// <param name="eSplitOptions">String split options.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string[] Split(this string strInput, char chrSeparator, StringSplitOptions eSplitOptions)
        {
            if (strInput == null)
                throw new ArgumentNullException(nameof(strInput));
            return strInput.SplitNoAlloc(chrSeparator, eSplitOptions).ToArray();
        }

        /// <summary>
        /// Syntactic sugar for string::Split that uses one separator char in its argument in addition to StringSplitOptions.
        /// </summary>
        /// <param name="strInput">String to search.</param>
        /// <param name="strSeparator">Separator to use.</param>
        /// <param name="eSplitOptions">String split options.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string[] Split(this string strInput, string strSeparator, StringSplitOptions eSplitOptions)
        {
            if (strInput == null)
                throw new ArgumentNullException(nameof(strInput));
            return strInput.SplitNoAlloc(strSeparator, eSplitOptions).ToArray();
        }

        /// <summary>
        /// Syntactic sugar for a version of Contains(char) for strings that is faster than messing with Linq
        /// </summary>
        /// <param name="strHaystack">Input string to search.</param>
        /// <param name="chrNeedle">Character for which to look.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains(this string strHaystack, char chrNeedle)
        {
            if (strHaystack == null)
                throw new ArgumentNullException(nameof(strHaystack));
            return strHaystack.IndexOf(chrNeedle) != -1;
        }

        /// <summary>
        /// Version of string::Split() that avoids allocations where possible, thus making it lighter on memory (and also on CPU because allocations take time) than all versions of string::Split()
        /// </summary>
        /// <param name="strInput">Input textblock.</param>
        /// <param name="chrSplit">Character to use for splitting.</param>
        /// <param name="eSplitOptions">Optional argument that can be used to skip over empty entries.</param>
        /// <returns>Enumerable containing substrings of <paramref name="strInput"/> split based on <paramref name="chrSplit"/></returns>
        public static IEnumerable<string> SplitNoAlloc(this string strInput, char chrSplit, StringSplitOptions eSplitOptions = StringSplitOptions.None)
        {
            if (string.IsNullOrEmpty(strInput))
                yield break;
            int intLoopLength;
            for (int intStart = 0; intStart < strInput.Length; intStart += intLoopLength + 1)
            {
                intLoopLength = strInput.IndexOf(chrSplit, intStart);
                if (intLoopLength < 0)
                    intLoopLength = strInput.Length;
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
        /// <returns>Enumerable containing substrings of <paramref name="strInput"/> split based on <paramref name="strSplit"/></returns>
        public static IEnumerable<string> SplitNoAlloc(this string strInput, string strSplit, StringSplitOptions eSplitOptions = StringSplitOptions.None)
        {
            if (string.IsNullOrEmpty(strInput))
                yield break;
            if (string.IsNullOrEmpty(strSplit))
            {
                yield return strInput;
                yield break;
            }
            int intLoopLength;
            for (int intStart = 0; intStart < strInput.Length; intStart += intLoopLength + strSplit.Length)
            {
                intLoopLength = strInput.IndexOf(strSplit, intStart, StringComparison.Ordinal);
                if (intLoopLength < 0)
                    intLoopLength = strInput.Length;
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
            if (string.IsNullOrEmpty(strInput))
                yield break;
            int intLoopLength;
            for (int intStart = 0; intStart < strInput.Length; intStart += intLoopLength + 1)
            {
                intLoopLength = strInput.IndexOfAny(achrSplit, intStart);
                if (intLoopLength < 0)
                    intLoopLength = strInput.Length;
                intLoopLength -= intStart;
                yield return intLoopLength != 0 ? strInput.Substring(intStart, intLoopLength) : string.Empty;
            }
        }

        /// <summary>
        /// Normalizes whitespace for a given textblock, removing extra spaces and trimming the string in the process.
        /// </summary>
        /// <param name="strInput">Input textblock</param>
        /// <param name="chrWhiteSpace">Whitespace character to use when replacing chars.</param>
        /// <param name="funcIsWhiteSpace">Custom function with which to check if a character should count as whitespace. If null, defaults to char::IsWhiteSpace.</param>
        /// <returns>New string with any chars that return true from <paramref name="funcIsWhiteSpace"/> replaced with <paramref name="chrWhiteSpace"/> and any excess whitespace removed.</returns>
        public static string NormalizeWhiteSpace(this string strInput, char chrWhiteSpace = ' ', Func<char, bool> funcIsWhiteSpace = null)
        {
            if (strInput == null)
                return string.Empty;
            int intLength = strInput.Length;
            if (intLength == 0)
                return strInput;
            if (funcIsWhiteSpace == null)
                funcIsWhiteSpace = char.IsWhiteSpace;
            if (intLength > GlobalOptions.MaxStackLimit)
            {
                char[] achrNewChars = new char[intLength];
                // What we're going here is copying the string-as-CharArray char-by-char into a new CharArray, but processing whitespace characters differently...
                int intCurrent = 0;
                // Start as true so that whitespace at the first character is trimmed as well
                bool blnLastCharWasWhiteSpace = true;
                for (int i = 0; i < intLength; ++i)
                {
                    char chrLoop = strInput[i];
                    // If we encounter a block of whitespace chars, we replace the first instance with chrWhiteSpace, then skip over the rest until we encounter a char that isn't whitespace
                    if (funcIsWhiteSpace(chrLoop))
                    {
                        if (!blnLastCharWasWhiteSpace)
                            achrNewChars[intCurrent++] = chrWhiteSpace;
                        blnLastCharWasWhiteSpace = true;
                    }
                    else
                    {
                        achrNewChars[intCurrent++] = chrLoop;
                        blnLastCharWasWhiteSpace = false;
                    }
                }

                // ... then we create a new string from the new CharArray, but only up to the number of characters that actually ended up getting copied.
                // If the last char is whitespace, we don't copy that, either.
                return new string(achrNewChars, 0, blnLastCharWasWhiteSpace ? intCurrent - 1 : intCurrent);
            }
            // Stackalloc is faster than a heap-allocated array, but string constructor requires use of unsafe context because there are no overloads for Span<char>
            unsafe
            {
                // Create CharArray in which we will store the new string
                char* achrNewChars = stackalloc char[intLength];
                // What we're going here is copying the string-as-CharArray char-by-char into a new CharArray, but processing whitespace characters differently...
                int intCurrent = 0;
                // Start as true so that whitespace at the first character is trimmed as well
                bool blnLastCharWasWhiteSpace = true;
                for (int i = 0; i < intLength; ++i)
                {
                    char chrLoop = strInput[i];
                    // If we encounter a block of whitespace chars, we replace the first instance with chrWhiteSpace, then skip over the rest until we encounter a char that isn't whitespace
                    if (funcIsWhiteSpace(chrLoop))
                    {
                        if (!blnLastCharWasWhiteSpace)
                            achrNewChars[intCurrent++] = chrWhiteSpace;
                        blnLastCharWasWhiteSpace = true;
                    }
                    else
                    {
                        achrNewChars[intCurrent++] = chrLoop;
                        blnLastCharWasWhiteSpace = false;
                    }
                }

                // ... then we create a new string from the new CharArray, but only up to the number of characters that actually ended up getting copied.
                // If the last char is whitespace, we don't copy that, either.
                return new string(achrNewChars, 0, blnLastCharWasWhiteSpace ? intCurrent - 1 : intCurrent);
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
            if (strInput == null)
                return false;
            int intLength = strInput.Length;
            if (intLength == 0)
                return true;
            int intLegalCharsLength = achrChars.Length;
            if (intLegalCharsLength == 0)
                return true;
            for (int i = 0; i < intLength; ++i)
            {
                char chrLoop = strInput[i];
                bool blnCharIsInList = false;
                for (int j = 0; j < intLegalCharsLength; ++j)
                {
                    if (chrLoop == achrChars[j])
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
        public static string TrimStart(this string strInput, string strToTrim, StringComparison eComparison = StringComparison.Ordinal)
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
            }
            while (i != -1);
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
        public static string TrimEnd(this string strInput, string strToTrim, StringComparison eComparison = StringComparison.Ordinal)
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
            }
            while (i != -1);
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
            if (!string.IsNullOrEmpty(strInput) && !string.IsNullOrEmpty(strToTrim))
            {
                // Need to make sure string actually starts with the substring, otherwise we don't want to be cutting out the beginning of the string
                if (blnOmitCheck || strInput.StartsWith(strToTrim, StringComparison.Ordinal))
                {
                    int intTrimLength = strToTrim.Length;
                    return strInput.Substring(intTrimLength, strInput.Length - intTrimLength);
                }
            }
            return strInput;
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
            if (!string.IsNullOrEmpty(strInput) && !string.IsNullOrEmpty(strToTrim))
            {
                // Need to make sure string actually ends with the substring, otherwise we don't want to be cutting out the end of the string
                if (blnOmitCheck || strInput.EndsWith(strToTrim, StringComparison.Ordinal))
                {
                    return strInput.Substring(0, strInput.Length - strToTrim.Length);
                }
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
                    if (strStringToTrim.Length > intHowMuchToTrim && strInput.StartsWith(strStringToTrim, StringComparison.Ordinal))
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
                    if (strStringToTrim.Length > intHowMuchToTrim && strInput.EndsWith(strStringToTrim, StringComparison.Ordinal))
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
        /// Escapes a char once out of a string if the string begins with it.
        /// </summary>
        /// <param name="strInput">String on which to operate</param>
        /// <param name="chrToTrim">Char to escape</param>
        /// <returns>String with <paramref name="chrToTrim"/> escaped out once from the beginning of it.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string TrimStartOnce(this string strInput, char chrToTrim)
        {
            if (!string.IsNullOrEmpty(strInput) && strInput[0] == chrToTrim)
            {
                return strInput.Substring(1, strInput.Length - 1);
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
        /// If a string begins with any chars, the one with which it begins is trimmed out of the string once.
        /// </summary>
        /// <param name="strInput">String on which to operate</param>
        /// <param name="achrToTrim">Chars to trim</param>
        /// <returns>Trimmed String</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string TrimStartOnce(this string strInput, params char[] achrToTrim)
        {
            if (!string.IsNullOrEmpty(strInput) && strInput.StartsWith(achrToTrim))
                return strInput.Substring(1, strInput.Length - 1);
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
            return (strInput?.Length > 0 && strInput[0] == chrToCheck);
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
            return (intLength > 0 && strInput[intLength - 1] == chrToCheck);
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
        public static string CheapReplace(this string strInput, string strOldValue, Func<string> funcNewValueFactory, StringComparison eStringComparison = StringComparison.Ordinal)
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
        /// Like StringBuilder::Replace(), but meant for if the new value would be expensive to calculate. Actually slower than string::Replace() if the new value is something simple.
        /// If the string does not contain any instances of the pattern to replace, then the expensive method to generate a replacement is not run.
        /// </summary>
        /// <param name="sbdInput">Base StringBuilder in which the replacing takes place. Note that ToString() will be applied to this as part of the method, so it may not be as cheap.</param>
        /// <param name="strOldValue">Pattern for which to check and which to replace.</param>
        /// <param name="funcNewValueFactory">Function to generate the string that replaces the pattern in the base string.</param>
        /// <param name="eStringComparison">The StringComparison to use for finding and replacing items.</param>
        /// <returns>The result of a StringBuilder::Replace() method if a replacement is made, the original string otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder CheapReplace(this StringBuilder sbdInput, string strOldValue, Func<string> funcNewValueFactory, StringComparison eStringComparison = StringComparison.Ordinal)
        {
            return sbdInput.CheapReplace(sbdInput?.ToString() ?? string.Empty, strOldValue, funcNewValueFactory, eStringComparison);
        }

        /// <summary>
        /// Like StringBuilder::Replace(), but meant for if the new value would be expensive to calculate. Actually slower than string::Replace() if the new value is something simple.
        /// If the string does not contain any instances of the pattern to replace, then the expensive method to generate a replacement is not run.
        /// </summary>
        /// <param name="sbdInput">Base StringBuilder in which the replacing takes place.</param>
        /// <param name="strOriginal">Original string around which StringBuilder was created. Set this so that StringBuilder::ToString() doesn't need to be called.</param>
        /// <param name="strOldValue">Pattern for which to check and which to replace.</param>
        /// <param name="funcNewValueFactory">Function to generate the string that replaces the pattern in the base string.</param>
        /// <param name="eStringComparison">The StringComparison to use for finding and replacing items.</param>
        /// <returns>The result of a StringBuilder::Replace() method if a replacement is made, the original string otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder CheapReplace(this StringBuilder sbdInput, string strOriginal, string strOldValue, Func<string> funcNewValueFactory, StringComparison eStringComparison = StringComparison.Ordinal)
        {
            if (sbdInput?.Length > 0 && !string.IsNullOrEmpty(strOriginal) && funcNewValueFactory != null)
            {
                if (eStringComparison == StringComparison.Ordinal)
                {
                    if (strOriginal.Contains(strOldValue))
                        sbdInput.Replace(strOldValue, funcNewValueFactory.Invoke());
                }
                else if (strOriginal.IndexOf(strOldValue, eStringComparison) != -1)
                {
                    string strOldStringBuilderValue = sbdInput.ToString();
                    sbdInput.Clear();
                    sbdInput.Append(strOldStringBuilderValue.Replace(strOldValue, funcNewValueFactory.Invoke(), eStringComparison));
                }
            }

            return sbdInput;
        }

        /// <summary>
        /// Combination of StringBuilder::Append() and static string::Join(), appending an list of strings with a separator.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sbdInput">Base StringBuilder onto which appending will take place.</param>
        /// <param name="strSeparator">The string to use as a separator. <paramref name="strSeparator" /> is included in the returned string only if value has more than one element.</param>
        /// <param name="lstValues">A collection that contains the objects to append.</param>
        /// <returns><paramref name="sbdInput" /> with values appended.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder AppendJoin<T>(this StringBuilder sbdInput, string strSeparator, IEnumerable<T> lstValues)
        {
            if (sbdInput == null)
                throw new ArgumentNullException(nameof(sbdInput));
            if (lstValues == null)
                throw new ArgumentNullException(nameof(lstValues));
            bool blnFirst = true;
            foreach (T objValue in lstValues)
            {
                if (blnFirst)
                    blnFirst = false;
                else
                    sbdInput.Append(strSeparator);
                sbdInput.Append(objValue);
            }
            return sbdInput;
        }

        /// <summary>
        /// Combination of StringBuilder::Append() and static string::Join(), appending an list of strings with a separator.
        /// </summary>
        /// <param name="sbdInput">Base StringBuilder onto which appending will take place.</param>
        /// <param name="strSeparator">The string to use as a separator. <paramref name="strSeparator" /> is included in the returned string only if value has more than one element.</param>
        /// <param name="lstValues">A collection that contains the strings to append.</param>
        /// <returns><paramref name="sbdInput" /> with values appended.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder AppendJoin(this StringBuilder sbdInput, string strSeparator, IEnumerable<string> lstValues)
        {
            if (sbdInput == null)
                throw new ArgumentNullException(nameof(sbdInput));
            if (lstValues == null)
                throw new ArgumentNullException(nameof(lstValues));
            bool blnFirst = true;
            foreach (string strValue in lstValues)
            {
                if (blnFirst)
                    blnFirst = false;
                else
                    sbdInput.Append(strSeparator);
                sbdInput.Append(strValue);
            }
            return sbdInput;
        }

        /// <summary>
        /// Combination of StringBuilder::Append() and static string::Join(), appending an list of strings with a separator.
        /// </summary>
        /// <param name="sbdInput">Base StringBuilder onto which appending will take place.</param>
        /// <param name="strSeparator">The string to use as a separator. <paramref name="strSeparator" /> is included in the returned string only if value has more than one element.</param>
        /// <param name="astrValues">An array that contains the string to append.</param>
        /// <param name="intStartIndex">The first element in <paramref name="astrValues" /> to use.</param>
        /// <param name="intCount">The number of elements of <paramref name="astrValues" /> to use.</param>
        /// <returns><paramref name="sbdInput" /> with values appended.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder AppendJoin(this StringBuilder sbdInput, string strSeparator, string[] astrValues, int intStartIndex, int intCount)
        {
            if (sbdInput == null)
                throw new ArgumentNullException(nameof(sbdInput));
            if (astrValues == null)
                throw new ArgumentNullException(nameof(astrValues));
            if (intStartIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(intStartIndex));
            if (intCount < 0)
                throw new ArgumentOutOfRangeException(nameof(intCount));
            if (intStartIndex + intCount >= astrValues.Length)
                throw new ArgumentOutOfRangeException(nameof(intStartIndex));
            for (int i = 0; i < intCount; ++i)
            {
                if (i > 0)
                    sbdInput.Append(strSeparator);
                sbdInput.Append(astrValues[i + intStartIndex]);
            }
            return sbdInput;
        }

        /// <summary>
        /// Combination of StringBuilder::Append() and static string::Join(), appending an list of strings with a separator.
        /// </summary>
        /// <param name="sbdInput">Base StringBuilder onto which appending will take place.</param>
        /// <param name="strSeparator">The string to use as a separator. <paramref name="strSeparator" /> is included in the returned string only if value has more than one element.</param>
        /// <param name="astrValues">An array that contains the string to append.</param>
        /// <returns><paramref name="sbdInput" /> with values appended.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder AppendJoin(this StringBuilder sbdInput, string strSeparator, params string[] astrValues)
        {
            if (sbdInput == null)
                throw new ArgumentNullException(nameof(sbdInput));
            if (astrValues == null)
                throw new ArgumentNullException(nameof(astrValues));
            for(int i = 0; i < astrValues.Length; ++i)
            {
                if (i > 0)
                    sbdInput.Append(strSeparator);
                sbdInput.Append(astrValues[i]);
            }
            return sbdInput;
        }

        /// <summary>
        /// Combination of StringBuilder::Append() and static string::Join(), appending an list of strings with a separator.
        /// </summary>
        /// <param name="sbdInput">Base StringBuilder onto which appending will take place.</param>
        /// <param name="strSeparator">The string to use as a separator. <paramref name="strSeparator" /> is included in the returned string only if value has more than one element.</param>
        /// <param name="aobjValues">An array that contains the objects to append.</param>
        /// <returns><paramref name="sbdInput" /> with values appended.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder AppendJoin(this StringBuilder sbdInput, string strSeparator, params object[] aobjValues)
        {
            if (sbdInput == null)
                throw new ArgumentNullException(nameof(sbdInput));
            if (aobjValues == null)
                throw new ArgumentNullException(nameof(aobjValues));
            for (int i = 0; i < aobjValues.Length; ++i)
            {
                if (i > 0)
                    sbdInput.Append(strSeparator);
                sbdInput.Append(aobjValues[i]);
            }
            return sbdInput;
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

            int intNextPosition;
            StringBuilder sbdReturn = new StringBuilder(strText.Length);
            string strNewLine = Environment.NewLine;
            // Parse each line of text
            for (int intCurrentPosition = 0; intCurrentPosition < strText.Length; intCurrentPosition = intNextPosition)
            {
                // Find end of line
                int intEndOfLinePosition = strText.IndexOf(strNewLine, intCurrentPosition, StringComparison.Ordinal);
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
                        while (intCurrentPosition < intEndOfLinePosition && char.IsWhiteSpace(strText[intCurrentPosition]))
                            intCurrentPosition += 1;
                    }
                    while (intEndOfLinePosition > intCurrentPosition);
                }
                else
                    sbdReturn.AppendLine(); // Empty line
            }
            return sbdReturn.ToString();
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
                if (char.IsWhiteSpace(chrLoop))
                {
                    // Return length of text before whitespace
                    return i + 1;
                }
            }

            // If no whitespace found, break at maximum length
            return intMax;
        }

        /// <summary>
        /// Clean an XPath string.
        /// </summary>
        /// <param name="strSearch">String to clean.</param>
        public static string CleanXPath(this string strSearch)
        {
            if(string.IsNullOrEmpty(strSearch))
                return null;
            int intQuotePos = strSearch.IndexOf('"');
            if (intQuotePos == -1)
            {
                return '\"' + strSearch + '\"';
            }

            StringBuilder sbdReturn = new StringBuilder("concat(\"", strSearch.Length + 10);
            for (int intSubStringStart = 0; intQuotePos != -1; intQuotePos = strSearch.IndexOf('"', intSubStringStart))
            {
                sbdReturn.Append(strSearch.Substring(intSubStringStart, intQuotePos - intSubStringStart)).Append("\", '\"', \"");
                intSubStringStart = intQuotePos + 1;
            }
            return sbdReturn.Append(strSearch).Append("\")").ToString();
        }

        /// <summary>
        /// Escapes characters in a string that would cause confusion if the string were placed as HTML content
        /// </summary>
        /// <param name="strToClean">String to clean.</param>
        /// <returns>Copy of input string with the characters "&", the greater than sign, and the lesser than sign escaped for HTML.</returns>
        public static string CleanForHTML(this string strToClean)
        {
            if (string.IsNullOrEmpty(strToClean))
                return string.Empty;
            return strToClean
                .Replace("&", "&amp;")
                .Replace("&amp;amp;", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\n\r", "<br />")
                .Replace("\r\n", "<br />")
                .Replace("\n", "<br />")
                .Replace("\r", "<br />");
        }

        /// <summary>
        /// Surrounds a plaintext string with basic RTF formatting so that it can be processed as an RTF string
        /// </summary>
        /// <param name="strInput">String to process</param>
        /// <returns>Version of <paramref name="strInput"/> surrounded with RTF formatting codes</returns>
        public static string PlainTextToRtf(this string strInput)
        {
            if (string.IsNullOrEmpty(strInput))
                return string.Empty;
            if (strInput.IsRtf())
                return strInput;
            lock (rtbRtfManipulatorLock)
            {
                if (!rtbRtfManipulator.IsHandleCreated)
                    rtbRtfManipulator.CreateControl();
                rtbRtfManipulator.DoThreadSafe(() => rtbRtfManipulator.Text = strInput);
                return rtbRtfManipulator.Rtf;
            }
        }

        /// <summary>
        /// Strips RTF formatting from a string
        /// </summary>
        /// <param name="strInput">String to process</param>
        /// <returns>Version of <paramref name="strInput"/> without RTF formatting codes</returns>
        public static string RtfToPlainText(this string strInput)
        {
            if (string.IsNullOrEmpty(strInput))
                return string.Empty;
            string strInputTrimmed = strInput.TrimStart();
            if (strInputTrimmed.StartsWith(@"{/rtf1", StringComparison.Ordinal)
                || strInputTrimmed.StartsWith(@"{\rtf1", StringComparison.Ordinal))
            {
                lock (rtbRtfManipulatorLock)
                {
                    if (!rtbRtfManipulator.IsHandleCreated)
                        rtbRtfManipulator.CreateControl();
                    try
                    {
                        rtbRtfManipulator.DoThreadSafe(() => rtbRtfManipulator.Rtf = strInput);
                    }
                    catch (ArgumentException)
                    {
                        return strInput;
                    }

                    return rtbRtfManipulator.Text;
                }
            }
            return strInput;
        }

        public static string RtfToHtml(this string strInput)
        {
            if (string.IsNullOrEmpty(strInput))
                return string.Empty;
            return strInput.IsRtf() ? Rtf.ToHtml(strInput) : strInput.CleanForHTML();
        }

        /// <summary>
        /// Whether or not a string is an RTF document
        /// </summary>
        /// <param name="strInput">The string to check.</param>
        /// <returns>True if <paramref name="strInput"/> is an RTF document, False otherwise.</returns>
        public static bool IsRtf(this string strInput)
        {
            if (!string.IsNullOrEmpty(strInput))
            {
                string strInputTrimmed = strInput.TrimStart();
                if (strInputTrimmed.StartsWith(@"{/rtf1", StringComparison.Ordinal)
                    || strInputTrimmed.StartsWith(@"{\rtf1", StringComparison.Ordinal))
                {
                    lock (rtbRtfManipulatorLock)
                    {
                        if (!rtbRtfManipulator.IsHandleCreated)
                            rtbRtfManipulator.CreateControl();
                        try
                        {
                            rtbRtfManipulator.DoThreadSafe(() => rtbRtfManipulator.Rtf = strInput);
                        }
                        catch (ArgumentException)
                        {
                            return false;
                        }
                    }

                    return true;
                }
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
            return !string.IsNullOrEmpty(strInput) && rgxHtmlTagExpression.IsMatch(strInput);
        }

        private static readonly Regex rgxHtmlTagExpression = new Regex(@"/<\/?[a-z][\s\S]*>/i",
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly object rtbRtfManipulatorLock = new object();
        private static readonly RichTextBox rtbRtfManipulator = new RichTextBox();
    }
}
