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
using System.Runtime.CompilerServices;
using System.Text;

namespace Chummer
{
    public static class StringExtensions
    {
        public static string EmptyGuid { get; } = Guid.Empty.ToString("D");

        public static bool IsEmptyGuid(this string strInput)
        {
            return strInput == EmptyGuid;
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
            SkipChar:;
            }
            // ... then we create a new string from the new CharArray, but only up to the number of characters that actually ended up getting copied
            return new string(achrNewChars, 0, intCurrent);
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
            }
            while (intCurrentEnd != -1);

            // Copy the remainder of the string once there are no more needles to remove
            for (; intCurrentReadPosition < intLength; ++intCurrentReadPosition)
            {
                achrNewChars[intCurrentLength++] = strInput[intCurrentReadPosition];
            }

            // Create a new string from the new CharArray, but only up to the number of characters that actually ended up getting copied
            return new string(achrNewChars, 0, intCurrentLength);
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
            if (strSubstringToDelete == null)
                return strInput;
            int intToDeleteLength = strSubstringToDelete.Length;
            if (intToDeleteLength == 0)
                return strInput;
            if (strInput == null)
                return string.Empty;
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
            return strInput.Split(new []{chrSeparator}, eSplitOptions);
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
            return strHaystack.IndexOf(chrNeedle) != -1;
        }

        /// <summary>
        /// Normalises whitespace for a given textblock, removing extra spaces and trimming the string in the process.
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
            if (!string.IsNullOrEmpty(strInput))
            {
                // Need to make sure string actually starts with the substring, otherwise we don't want to be cutting out the beginning of the string
                if (blnOmitCheck || strInput.StartsWith(strToTrim))
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
            if (!string.IsNullOrEmpty(strInput))
            {
                // Need to make sure string actually ends with the substring, otherwise we don't want to be cutting out the end of the string
                if (blnOmitCheck || strInput.EndsWith(strToTrim))
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
            if (strInput == null)
                return string.Empty;
            if (!string.IsNullOrEmpty(strInput) && astrToTrim != null)
            {
                // Without this we could trim a smaller string just because it was found first, this makes sure we find the largest one
                int intHowMuchToTrim = 0;

                int intLength = astrToTrim.Length;
                for (int i = 0; i < intLength; ++i)
                {
                    string strStringToTrim = astrToTrim[i];
                    // Need to make sure string actually starts with the substring, otherwise we don't want to be cutting out the beginning of the string
                    if (strStringToTrim.Length > intHowMuchToTrim && strInput.StartsWith(strStringToTrim))
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
            if (strInput == null)
                return string.Empty;
            if (!string.IsNullOrEmpty(strInput) && astrToTrim != null)
            {
                // Without this we could trim a smaller string just because it was found first, this makes sure we find the largest one
                int intHowMuchToTrim = 0;

                int intLength = astrToTrim.Length;
                for (int i = 0; i < intLength; ++i)
                {
                    string strStringToTrim = astrToTrim[i];
                    // Need to make sure string actually ends with the substring, otherwise we don't want to be cutting out the end of the string
                    if (strStringToTrim.Length > intHowMuchToTrim && strInput.EndsWith(strStringToTrim))
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
            if (strInput.StartsWith(achrToTrim))
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
            if (strInput.EndsWith(achrToTrim))
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
            if (strInput == null || achrToCheck == null)
                return false;
            if (strInput.Length == 0)
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
                    if (strInput.StartsWith(astrToCheck[i]))
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
                    if (strInput.EndsWith(astrToCheck[i]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Like string::Replace(), but if the string does not contain any instances of the pattern to replace, then the (potentially expensive) method to generate a replacement is not run.
        /// </summary>
        /// <param name="strInput">Base string in which the replacing takes place.</param>
        /// <param name="strOldValue">Pattern for which to check and which to replace.</param>
        /// <param name="funcNewValueFactory">Function to generate the string that replaces the pattern in the base string.</param>
        /// <returns>The result of a string::Replace() method if a replacement is made, the original string otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string CheapReplace(this string strInput, string strOldValue, Func<string> funcNewValueFactory)
        {
            if (strInput?.Contains(strOldValue) == true)
                return strInput.Replace(strOldValue, funcNewValueFactory.Invoke());
            return strInput;
        }

        /// <summary>
        /// Like StringBuilder::Replace(), but if the string does not contain any instances of the pattern to replace, then the (potentially expensive) method to generate a replacement is not run.
        /// </summary>
        /// <param name="strbldInput">Base StringBuilder in which the replacing takes place. Note that ToString() will be applied to this as part of the method, so it may not be as cheap.</param>
        /// <param name="strOldValue">Pattern for which to check and which to replace.</param>
        /// <param name="funcNewValueFactory">Function to generate the string that replaces the pattern in the base string.</param>
        /// <returns>The result of a StringBuilder::Replace() method if a replacement is made, the original string otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CheapReplace(this StringBuilder strbldInput, string strOldValue, Func<string> funcNewValueFactory)
        {
            strbldInput.CheapReplace(strbldInput.ToString(), strOldValue, funcNewValueFactory);
        }

        /// <summary>
        /// Like StringBuilder::Replace(), but if the string does not contain any instances of the pattern to replace, then the (potentially expensive) method to generate a replacement is not run.
        /// </summary>
        /// <param name="strbldInput">Base StringBuilder in which the replacing takes place.</param>
        /// <param name="strOriginal">Original string around which StringBuilder was created. Set this so that StringBuilder::ToString() doesn't need to be called.</param>
        /// <param name="strOldValue">Pattern for which to check and which to replace.</param>
        /// <param name="funcNewValueFactory">Function to generate the string that replaces the pattern in the base string.</param>
        /// <returns>The result of a StringBuilder::Replace() method if a replacement is made, the original string otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CheapReplace(this StringBuilder strbldInput, string strOriginal, string strOldValue, Func<string> funcNewValueFactory)
        {
            if (strOriginal?.Contains(strOldValue) == true)
                strbldInput.Replace(strOldValue, funcNewValueFactory.Invoke());
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
        public static string WordWrap(this string strText, int intWidth)
        {
            // Lucidity checks
            if (string.IsNullOrEmpty(strText))
                return strText;
            if (intWidth >= strText.Length)
                return strText;

            int intNextPosition;
            StringBuilder objReturn = new StringBuilder(strText.Length);
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
                        objReturn.Append(strText, intCurrentPosition, intLengthToRead);
                        objReturn.Append(strNewLine);

                        // Trim whitespace following break
                        intCurrentPosition += intLengthToRead;
                        while (intCurrentPosition < intEndOfLinePosition && char.IsWhiteSpace(strText[intCurrentPosition]))
                            intCurrentPosition += 1;
                    }
                    while (intEndOfLinePosition > intCurrentPosition);
                }
                else objReturn.Append(strNewLine); // Empty line
            }
            return objReturn.ToString();
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
            if(String.IsNullOrEmpty(strSearch))
                return null;
            int intQuotePos = strSearch.IndexOf('"');
            if (intQuotePos == -1)
            {
                return '\"' + strSearch + '\"';
            }

            StringBuilder objReturn = new StringBuilder("concat(\"");
            for (; intQuotePos != -1; intQuotePos = strSearch.IndexOf('"'))
            {
                string strSubstring = strSearch.Substring(0, intQuotePos);
                objReturn.Append(strSubstring);
                objReturn.Append("\", '\"', \"");
                strSearch = strSearch.Substring(intQuotePos + 1);
            }
            objReturn.Append(strSearch);
            objReturn.Append("\")");
            return objReturn.ToString();
        }

        /// <summary>
        /// Escapes characters in a string that would cause confusion if the string were placed as HTML content
        /// </summary>
        /// <param name="strToClean">String to clean.</param>
        /// <returns>Copy of input string with the characters "&", the greater than sign, and the lesser than sign escaped for HTML.</returns>
        public static string CleanForHTML(this string strToClean)
        {
            return strToClean
                .CheapReplace("<br />", () => "\n")
                .CheapReplace("&", () => "&amp;")
                .CheapReplace("&amp;amp;", () => "&amp;")
                .CheapReplace("<", () => "&lt;")
                .CheapReplace(">", () => "&gt;")
                .CheapReplace("\n\r", () => "<br />")
                .CheapReplace("\n", () => "<br />")
                .CheapReplace("\r", () => "<br />");
        }
    }
}
