using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
