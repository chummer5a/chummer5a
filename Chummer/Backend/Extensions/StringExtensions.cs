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

namespace Chummer
{
    static class StringExtensions
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
            int intLength = strInput?.Length ?? 0;
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
            int intDeleteLength = achrToDelete.Length;
            if (intDeleteLength == 0)
                return strInput;
            int intLength = strInput?.Length ?? 0;
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
        /// Normalises whitespace for a given textblock, removing extra spaces and trimming the string in the process.
        /// </summary>
        /// <param name="strInput">Input textblock</param>
        /// <param name="chrWhiteSpace">Whitespace character to use</param>
        /// <returns>New string with any excess whitespace removed</returns>
        public static string NormalizeWhiteSpace(this string strInput, char chrWhiteSpace = ' ')
        {
            int intLength = strInput?.Length ?? 0;
            if (intLength == 0)
                return strInput;
            char[] achrNewChars = new char[intLength];
            // What we're going here is copying the string-as-CharArray char-by-char into a new CharArray, but processing whitespace characters differently...
            int intCurrent = 0;
            // Start as true so that whitespace at the first character is trimmed as well
            bool blnLastCharWasWhiteSpace = true;
            for (int i = 0; i < intLength; ++i)
            {
                char chrLoop = strInput[i];
                // If we encounter a block of whitespace chars, we replace the first instance with chrWhiteSpace, then skip over the rest until we encounter a char that isn't whitespace
                if (char.IsWhiteSpace(chrLoop))
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
        /// Trims a substring out of a string if the string begins with it.
        /// </summary>
        /// <param name="strInput">String on which to operate</param>
        /// <param name="strStringToTrim">Substring to trim</param>
        /// <param name="blnOmitCheck">If we already know that the string begins with the substring</param>
        /// <returns>Trimmed String</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string TrimStart(this string strInput, string strStringToTrim, bool blnOmitCheck = false)
        {
            if (!string.IsNullOrEmpty(strInput))
            {
                // Need to make sure string actually starts with the substring, otherwise we don't want to be cutting out the beginning of the string
                if (blnOmitCheck || strInput.StartsWith(strStringToTrim))
                {
                    int intTrimLength = strStringToTrim.Length;
                    return strInput.Substring(intTrimLength, strInput.Length - intTrimLength);
                }
            }
            return strInput;
        }

        /// <summary>
        /// Trims a substring out of a string if the string ends with it.
        /// </summary>
        /// <param name="strInput">String on which to operate</param>
        /// <param name="strStringToTrim">Substring to trim</param>
        /// <param name="blnOmitCheck">If we already know that the string ends with the substring</param>
        /// <returns>Trimmed String</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string TrimEnd(this string strInput, string strStringToTrim, bool blnOmitCheck = false)
        {
            if (!string.IsNullOrEmpty(strInput))
            {
                // Need to make sure string actually ends with the substring, otherwise we don't want to be cutting out the end of the string
                if (blnOmitCheck || strInput.EndsWith(strStringToTrim))
                {
                    return strInput.Substring(0, strInput.Length - strStringToTrim.Length);
                }
            }
            return strInput;
        }

        /// <summary>
        /// If a string begins with any substrings, the one with which it begins is trimmed out of the string.
        /// </summary>
        /// <param name="strInput">String on which to operate</param>
        /// <param name="astrStringToTrim">Substrings to trim</param>
        /// <returns>Trimmed String</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string TrimStart(this string strInput, params string[] astrStringToTrim)
        {
            if (!string.IsNullOrEmpty(strInput))
            {
                int intLength = astrStringToTrim.Length;
                for (int i = 0; i < intLength; ++i)
                {
                    string strStringToTrim = astrStringToTrim[i];
                    // Need to make sure string actually starts with the substring, otherwise we don't want to be cutting out the beginning of the string
                    if (strInput.StartsWith(strStringToTrim))
                    {
                        int intTrimLength = strStringToTrim.Length;
                        return strInput.Substring(intTrimLength, strInput.Length - intTrimLength);
                    }
                }
            }
            return strInput;
        }

        /// <summary>
        /// If a string ends with any substrings, the one with which it begins is trimmed out of the string.
        /// </summary>
        /// <param name="strInput">String on which to operate</param>
        /// <param name="astrStringToTrim">Substrings to trim</param>
        /// <returns>Trimmed String</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string TrimEnd(this string strInput, params string[] astrStringToTrim)
        {
            if (!string.IsNullOrEmpty(strInput))
            {
                int intLength = astrStringToTrim.Length;
                for (int i = 0; i < intLength; ++i)
                {
                    string strStringToTrim = astrStringToTrim[i];
                    // Need to make sure string actually ends with the substring, otherwise we don't want to be cutting out the end of the string
                    if (strInput.EndsWith(strStringToTrim))
                    {
                        return strInput.Substring(0, strInput.Length - strStringToTrim.Length);
                    }
                }
            }
            return strInput;
        }

        /// <summary>
        /// Determines whether the first char of this string instance matches the specified char.
        /// </summary>
        /// <param name="strInput">String to check.</param>
        /// <param name="chrCharToCheck">Char to check.</param>
        /// <returns>True if string has a non-zero length and begins with the char, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool StartsWith(this string strInput, char chrCharToCheck)
        {
            return (strInput?.Length > 0 && strInput[0] == chrCharToCheck);
        }

        /// <summary>
        /// Determines whether the last char of this string instance matches the specified char.
        /// </summary>
        /// <param name="strInput">String to check.</param>
        /// <param name="chrCharToCheck">Char to check.</param>
        /// <returns>True if string has a non-zero length and ends with the char, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EndsWith(this string strInput, char chrCharToCheck)
        {
            int intLength = strInput?.Length ?? 0;
            return (intLength > 0 && strInput[intLength - 1] == chrCharToCheck);
        }

        /// <summary>
        /// Determines whether the first char of this string instance matches any of the specified chars.
        /// </summary>
        /// <param name="strInput">String to check.</param>
        /// <param name="achrCharToCheck">Chars to check.</param>
        /// <returns>True if string has a non-zero length and begins with any of the specified chars, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool StartsWith(this string strInput, params char[] achrCharToCheck)
        {
            if (strInput?.Length == 0)
                return false;
            char chrCharToCheck = strInput[0];
            int intParamsLength = achrCharToCheck.Length;
            for (int i = 0; i < intParamsLength; ++i)
            {
                if (chrCharToCheck == achrCharToCheck[i])
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Determines whether the last char of this string instance matches any of the specified chars.
        /// </summary>
        /// <param name="strInput">String to check.</param>
        /// <param name="achrCharToCheck">Chars to check.</param>
        /// <returns>True if string has a non-zero length and ends with any of the specified chars, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EndsWith(this string strInput, params char[] achrCharToCheck)
        {
            int intLength = strInput?.Length ?? 0;
            if (intLength == 0)
                return false;
            char chrCharToCheck = strInput[intLength - 1];
            int intParamsLength = achrCharToCheck.Length;
            for (int i = 0; i < intParamsLength; ++i)
            {
                if (chrCharToCheck == achrCharToCheck[i])
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Determines whether the beginning of this string instance matches any of the specified strings.
        /// </summary>
        /// <param name="strInput">String to check.</param>
        /// <param name="astrStringsToCheck">Strings to check.</param>
        /// <returns>True if string has a non-zero length and begins with any of the specified chars, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool StartsWith(this string strInput, params string[] astrStringsToCheck)
        {
            if (!string.IsNullOrEmpty(strInput))
            {
                int intLength = astrStringsToCheck.Length;
                for (int i = 0; i < intLength; ++i)
                {
                    if (strInput.StartsWith(astrStringsToCheck[i]))
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
        /// <param name="astrStringsToCheck">Strings to check.</param>
        /// <returns>True if string has a non-zero length and ends with any of the specified chars, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EndsWith(this string strInput, params string[] astrStringsToCheck)
        {
            if (!string.IsNullOrEmpty(strInput))
            {
                int intLength = astrStringsToCheck.Length;
                for (int i = 0; i < intLength; ++i)
                {
                    if (strInput.EndsWith(astrStringsToCheck[i]))
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
        /// <param name="value">String to test</param>
        /// <returns>True if string is a Guid, false if not.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsGuid(this string strGuid)
        {
            return Guid.TryParse(strGuid, out Guid guidDummy);
        }

        /// <summary>
        /// Word wraps the given text to fit within the specified width.
        /// </summary>
        /// <param name="strText">Text to be word wrapped</param>
        /// <param name="intWidth">Width, in characters, to which the text
        /// should be word wrapped</param>
        /// <returns>The modified text</returns>
        public static string WordWrap(this string strText, int intWidth)
        {
            // Lucidity checks
            if (intWidth < 1)
                return strText;
            if (string.IsNullOrEmpty(strText))
                return strText;

            int next = 0;
            StringBuilder sb = new StringBuilder(strText.Length);

            // Parse each line of text
            for (int pos = 0; pos < strText.Length; pos = next)
            {
                // Find end of line
                int eol = strText.IndexOf(Environment.NewLine, pos, StringComparison.Ordinal);
                if (eol == -1)
                    next = eol = strText.Length;
                else
                    next = eol + Environment.NewLine.Length;

                // Copy this line of text, breaking into smaller lines as needed
                if (eol > pos)
                {
                    do
                    {
                        int len = eol - pos;
                        if (len > intWidth)
                            len = strText.BreakLine(pos, intWidth);
                        sb.Append(strText, pos, len);
                        sb.Append(Environment.NewLine);

                        // Trim whitespace following break
                        pos += len;
                        while (pos < eol && char.IsWhiteSpace(strText[pos]))
                            pos += 1;
                    }
                    while (eol > pos);
                }
                else sb.Append(Environment.NewLine); // Empty line
            }
            return sb.ToString();
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
            if (intMax + intPosition >= strText?.Length)
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
        private static string CleanXPath(this string strSearch)
        {
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
    }
}
