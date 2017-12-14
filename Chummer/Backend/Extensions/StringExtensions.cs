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
        /// <summary>
        /// Method to quickly remove all instances of a char from a string (much faster than using Replace() with an empty string)
        /// </summary>
        /// <param name="strBase">String on which to operate</param>
        /// <param name="chrToDelete">Character to remove</param>
        /// <returns>New string with characters removed</returns>
        public static string FastEscape(this string strBase, char chrToDelete)
        {
            int len = strBase.Length;
            char[] newChars = new char[len];
            // What we're going here is copying the string-as-CharArray char-by-char into a new CharArray, but skipping over any instance of chrToDelete...
            int i2 = 0;
            for (int i = 0; i < len; ++i)
            {
                char c = strBase[i];
                if (c != chrToDelete)
                    newChars[i2++] = c;
            }
            // ... then we create a new string from the new CharArray, but only up to the number of characters that actually ended up getting copied
            return new string(newChars, 0, i2);
        }

        /// <summary>
        /// Method to quickly remove all instances of all chars in an array from a string (much faster than using a series of Replace() with an empty string)
        /// </summary>
        /// <param name="strBase">String on which to operate</param>
        /// <param name="chrToDelete">Array of characters to remove</param>
        /// <returns>New string with characters removed</returns>
        public static string FastEscape(this string strBase, char[] chrToDelete)
        {
            int len = strBase.Length;
            int dellen = chrToDelete.Length;
            char[] newChars = new char[len];
            // What we're going here is copying the string-as-CharArray char-by-char into a new CharArray, but skipping over any instance of chrToDelete...
            int i2 = 0;
            for (int i = 0; i < len; ++i)
            {
                char c = strBase[i];
                for (int j = 0; j < dellen; ++j)
                {
                    if (c == chrToDelete[j])
                    {
                        goto SkipChar;
                    }
                }
                newChars[i2++] = c;
                SkipChar:;
            }
            // ... then we create a new string from the new CharArray, but only up to the number of characters that actually ended up getting copied
            return new string(newChars, 0, i2);
        }

        /// <summary>
        /// Trims a substring out of a string if the string begins with it.
        /// </summary>
        /// <param name="strBase">String on which to operate</param>
        /// <param name="strStringToTrim">Substring to trim</param>
        /// <param name="blnOmitCheck">If we already know that the string begins with the substring</param>
        /// <returns>Trimmed String</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string TrimStart(this string strBase, string strStringToTrim, bool blnOmitCheck = false)
        {
            // Need to make sure string actually starts with the substring, otherwise we don't want to be cutting out the beginning of the string
            if (blnOmitCheck || strBase.StartsWith(strStringToTrim))
            {
                int intTrimLength = strStringToTrim.Length;
                strBase = strBase.Substring(intTrimLength, strBase.Length - intTrimLength);
            }
            return strBase;
        }

        /// <summary>
        /// Trims a substring out of a string if the string ends with it.
        /// </summary>
        /// <param name="strBase">String on which to operate</param>
        /// <param name="strStringToTrim">Substring to trim</param>
        /// <param name="blnOmitCheck">If we already know that the string ends with the substring</param>
        /// <returns>Trimmed String</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string TrimEnd(this string strBase, string strStringToTrim, bool blnOmitCheck = false)
        {
            // Need to make sure string actually ends with the substring, otherwise we don't want to be cutting out the end of the string
            if (blnOmitCheck || strBase.EndsWith(strStringToTrim))
            {
                int intTrimLength = strStringToTrim.Length;
                strBase = strBase.Substring(0, strBase.Length - intTrimLength);
            }
            return strBase;
        }

        /// <summary>
        /// Determines whether the first char of this string instance matches the specified char.
        /// </summary>
        /// <param name="strBase">String to check.</param>
        /// <param name="chrCharToCheck">Char to check.</param>
        /// <returns>True if string has a non-zero length and begins with the char, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool StartsWith(this string strBase, char chrCharToCheck)
        {
            return (strBase.Length > 0 && strBase[0] == chrCharToCheck);
        }

        /// <summary>
        /// Determines whether the last char of this string instance matches the specified char.
        /// </summary>
        /// <param name="strBase">String to check.</param>
        /// <param name="chrCharToCheck">Char to check.</param>
        /// <returns>True if string has a non-zero length and ends with the char, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EndsWith(this string strBase, char chrCharToCheck)
        {
            int intLength = strBase.Length;
            return (intLength > 0 && strBase[intLength - 1] == chrCharToCheck);
        }

        /// <summary>
        /// Like string::Replace(), but if the string does not contain any instances of the pattern to replace, then the (potentially expensive) method to generate a replacement is not run.
        /// </summary>
        /// <param name="strBase">Base string in which the replacing takes place.</param>
        /// <param name="strOldValue">Pattern for which to check and which to replace.</param>
        /// <param name="funcNewValueFactory">Function to generate the string that replaces the pattern in the base string.</param>
        /// <returns>The result of a string::Replace() method if a replacement is made, the original string otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string CheapReplace(this string strBase, string strOldValue, Func<string> funcNewValueFactory)
        {
            if (strBase.Contains(strOldValue))
                return strBase.Replace(strOldValue, funcNewValueFactory.Invoke());
            return strBase;
        }

        /// <summary>
        /// Like StringBuilder::Replace(), but if the string does not contain any instances of the pattern to replace, then the (potentially expensive) method to generate a replacement is not run.
        /// </summary>
        /// <param name="strBase">Base StringBuilder in which the replacing takes place. Note that ToString() will be applied to this as part of the method, so it may not be as cheap.</param>
        /// <param name="strOldValue">Pattern for which to check and which to replace.</param>
        /// <param name="funcNewValueFactory">Function to generate the string that replaces the pattern in the base string.</param>
        /// <returns>The result of a StringBuilder::Replace() method if a replacement is made, the original string otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CheapReplace(this StringBuilder strBase, string strOldValue, Func<string> funcNewValueFactory)
        {
            strBase.CheapReplace(strBase.ToString(), strOldValue, funcNewValueFactory);
        }

        /// <summary>
        /// Like StringBuilder::Replace(), but if the string does not contain any instances of the pattern to replace, then the (potentially expensive) method to generate a replacement is not run.
        /// </summary>
        /// <param name="strBase">Base StringBuilder in which the replacing takes place.</param>
        /// <param name="strOriginal">Original string around which StringBuilder was created. Set this so that StringBuilder::ToString() doesn't need to be called.</param>
        /// <param name="strOldValue">Pattern for which to check and which to replace.</param>
        /// <param name="funcNewValueFactory">Function to generate the string that replaces the pattern in the base string.</param>
        /// <returns>The result of a StringBuilder::Replace() method if a replacement is made, the original string otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CheapReplace(this StringBuilder strBase, string strOriginal, string strOldValue, Func<string> funcNewValueFactory)
        {
            if (strOriginal.Contains(strOldValue))
                strBase.Replace(strOldValue, funcNewValueFactory.Invoke());
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
    }
}
