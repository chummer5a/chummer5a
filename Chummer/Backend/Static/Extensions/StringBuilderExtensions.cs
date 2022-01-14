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
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Chummer.Annotations;

namespace Chummer
{
    internal static class StringBuilderExtensions
    {
        /// <summary>
        /// Syntactic sugar for appending a character followed by a new line.
        /// </summary>
        /// <param name="sbdInput">Base StringBuilder in which appending is to take place.</param>
        /// <param name="chrValue">New character to append before the new line is appended.</param>
        /// <returns></returns>
        public static StringBuilder AppendLine([NotNull] this StringBuilder sbdInput, char chrValue)
        {
            return sbdInput.Append(chrValue).AppendLine();
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
        public static StringBuilder CheapReplace([NotNull] this StringBuilder sbdInput, string strOldValue, Func<string> funcNewValueFactory, StringComparison eStringComparison = StringComparison.Ordinal)
        {
            return sbdInput.CheapReplace(sbdInput.ToString(), strOldValue, funcNewValueFactory, eStringComparison);
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
        public static StringBuilder CheapReplace([NotNull] this StringBuilder sbdInput, string strOriginal, string strOldValue, Func<string> funcNewValueFactory, StringComparison eStringComparison = StringComparison.Ordinal)
        {
            if (sbdInput.Length > 0 && !string.IsNullOrEmpty(strOriginal) && funcNewValueFactory != null)
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
        /// Like StringBuilder::Replace(), but meant for if the new value would be expensive to calculate. Actually slower than string::Replace() if the new value is something simple.
        /// This is the async version that can be run in case a value is really expensive to get.
        /// If the string does not contain any instances of the pattern to replace, then the expensive method to generate a replacement is not run.
        /// </summary>
        /// <param name="sbdInput">Base StringBuilder in which the replacing takes place. Note that ToString() will be applied to this as part of the method, so it may not be as cheap.</param>
        /// <param name="strOldValue">Pattern for which to check and which to replace.</param>
        /// <param name="funcNewValueFactory">Function to generate the string that replaces the pattern in the base string.</param>
        /// <param name="eStringComparison">The StringComparison to use for finding and replacing items.</param>
        /// <returns>The result of a StringBuilder::Replace() method if a replacement is made, the original string otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<StringBuilder> CheapReplaceAsync([NotNull] this StringBuilder sbdInput, string strOldValue, Func<string> funcNewValueFactory, StringComparison eStringComparison = StringComparison.Ordinal)
        {
            return sbdInput.CheapReplaceAsync(sbdInput.ToString(), strOldValue, funcNewValueFactory, eStringComparison);
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
        public static async Task<StringBuilder> CheapReplaceAsync([NotNull] this StringBuilder sbdInput, string strOriginal, string strOldValue, Func<string> funcNewValueFactory, StringComparison eStringComparison = StringComparison.Ordinal)
        {
            if (sbdInput.Length > 0 && !string.IsNullOrEmpty(strOriginal) && funcNewValueFactory != null)
            {
                if (eStringComparison == StringComparison.Ordinal)
                {
                    if (strOriginal.Contains(strOldValue))
                    {
                        string strFactoryResult = string.Empty;
                        await Task.Factory.FromAsync(funcNewValueFactory.BeginInvoke, x => strFactoryResult = funcNewValueFactory.EndInvoke(x), null);
                        sbdInput.Replace(strOldValue, strFactoryResult);
                    }
                }
                else if (strOriginal.IndexOf(strOldValue, eStringComparison) != -1)
                {
                    string strFactoryResult = string.Empty;
                    Task<string> tskGetValue = Task.Factory.FromAsync(funcNewValueFactory.BeginInvoke,
                                                                      x => strFactoryResult = funcNewValueFactory.EndInvoke(x), null);
                    string strOldStringBuilderValue = sbdInput.ToString();
                    sbdInput.Clear();
                    await tskGetValue;
                    sbdInput.Append(strOldStringBuilderValue.Replace(strOldValue, strFactoryResult, eStringComparison));
                }
            }

            return sbdInput;
        }

        /// <summary>
        /// Combination of StringBuilder::Append() and static string::Join(), appending a list of strings with a separator.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sbdInput">Base StringBuilder onto which appending will take place.</param>
        /// <param name="strSeparator">The string to use as a separator. <paramref name="strSeparator" /> is included in the returned string only if value has more than one element.</param>
        /// <param name="lstValues">A collection that contains the objects to append.</param>
        /// <returns><paramref name="sbdInput" /> with values appended.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder AppendJoin<T>([NotNull] this StringBuilder sbdInput, string strSeparator, IEnumerable<T> lstValues)
        {
            if (lstValues == null)
                throw new ArgumentNullException(nameof(lstValues));
            bool blnFirst = true;
            foreach (T objValue in lstValues)
            {
                if (!blnFirst)
                    sbdInput.Append(strSeparator);
                sbdInput.Append(objValue);
                blnFirst = false;
            }
            return sbdInput;
        }

        /// <summary>
        /// Combination of StringBuilder::Append() and static string::Join(), appending a list of strings with a separator.
        /// </summary>
        /// <param name="sbdInput">Base StringBuilder onto which appending will take place.</param>
        /// <param name="strSeparator">The string to use as a separator. <paramref name="strSeparator" /> is included in the returned string only if value has more than one element.</param>
        /// <param name="lstValues">A collection that contains the strings to append.</param>
        /// <returns><paramref name="sbdInput" /> with values appended.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder AppendJoin([NotNull] this StringBuilder sbdInput, string strSeparator, IEnumerable<string> lstValues)
        {
            if (lstValues == null)
                throw new ArgumentNullException(nameof(lstValues));
            bool blnFirst = true;
            foreach (string strValue in lstValues)
            {
                if (!blnFirst)
                    sbdInput.Append(strSeparator);
                sbdInput.Append(strValue);
                blnFirst = false;
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
        public static StringBuilder AppendJoin([NotNull] this StringBuilder sbdInput, string strSeparator, string[] astrValues, int intStartIndex, int intCount)
        {
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
                string strLoop = astrValues[i + intStartIndex];
                if (i > 0)
                    sbdInput.Append(strSeparator);
                sbdInput.Append(strLoop);
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
        public static StringBuilder AppendJoin([NotNull] this StringBuilder sbdInput, string strSeparator, params string[] astrValues)
        {
            if (astrValues == null)
                throw new ArgumentNullException(nameof(astrValues));
            for (int i = 0; i < astrValues.Length; ++i)
            {
                string strLoop = astrValues[i];
                if (i > 0)
                    sbdInput.Append(strSeparator);
                sbdInput.Append(strLoop);
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
        public static StringBuilder AppendJoin([NotNull] this StringBuilder sbdInput, string strSeparator, params object[] aobjValues)
        {
            if (aobjValues == null)
                throw new ArgumentNullException(nameof(aobjValues));
            for (int i = 0; i < aobjValues.Length; ++i)
            {
                string strLoop = aobjValues[i].ToString();
                if (i > 0)
                    sbdInput.Append(strSeparator);
                sbdInput.Append(strLoop);
            }
            return sbdInput;
        }

        /// <summary>
        /// Combination of StringBuilder::Append() and static string::Join(), appending a list of strings with a separator.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sbdInput">Base StringBuilder onto which appending will take place.</param>
        /// <param name="chrSeparator">The char to use as a separator. <paramref name="chrSeparator" /> is included in the returned string only if value has more than one element.</param>
        /// <param name="lstValues">A collection that contains the objects to append.</param>
        /// <returns><paramref name="sbdInput" /> with values appended.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder AppendJoin<T>([NotNull] this StringBuilder sbdInput, char chrSeparator, IEnumerable<T> lstValues)
        {
            if (lstValues == null)
                throw new ArgumentNullException(nameof(lstValues));
            bool blnFirst = true;
            foreach (T objValue in lstValues)
            {
                if (!blnFirst)
                    sbdInput.Append(chrSeparator);
                sbdInput.Append(objValue);
                blnFirst = false;
            }
            return sbdInput;
        }

        /// <summary>
        /// Combination of StringBuilder::Append() and static string::Join(), appending a list of strings with a separator.
        /// </summary>
        /// <param name="sbdInput">Base StringBuilder onto which appending will take place.</param>
        /// <param name="chrSeparator">The char to use as a separator. <paramref name="chrSeparator" /> is included in the returned string only if value has more than one element.</param>
        /// <param name="lstValues">A collection that contains the strings to append.</param>
        /// <returns><paramref name="sbdInput" /> with values appended.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder AppendJoin([NotNull] this StringBuilder sbdInput, char chrSeparator, IEnumerable<string> lstValues)
        {
            if (lstValues == null)
                throw new ArgumentNullException(nameof(lstValues));
            bool blnFirst = true;
            foreach (string strValue in lstValues)
            {
                if (!blnFirst)
                    sbdInput.Append(chrSeparator);
                sbdInput.Append(strValue);
                blnFirst = false;
            }
            return sbdInput;
        }

        /// <summary>
        /// Combination of StringBuilder::Append() and static string::Join(), appending an list of strings with a separator.
        /// </summary>
        /// <param name="sbdInput">Base StringBuilder onto which appending will take place.</param>
        /// <param name="chrSeparator">The char to use as a separator. <paramref name="chrSeparator" /> is included in the returned string only if value has more than one element.</param>
        /// <param name="astrValues">An array that contains the string to append.</param>
        /// <param name="intStartIndex">The first element in <paramref name="astrValues" /> to use.</param>
        /// <param name="intCount">The number of elements of <paramref name="astrValues" /> to use.</param>
        /// <returns><paramref name="sbdInput" /> with values appended.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder AppendJoin([NotNull] this StringBuilder sbdInput, char chrSeparator, string[] astrValues, int intStartIndex, int intCount)
        {
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
                string strLoop = astrValues[i + intStartIndex];
                if (i > 0)
                    sbdInput.Append(chrSeparator);
                sbdInput.Append(strLoop);
            }
            return sbdInput;
        }

        /// <summary>
        /// Combination of StringBuilder::Append() and static string::Join(), appending an list of strings with a separator.
        /// </summary>
        /// <param name="sbdInput">Base StringBuilder onto which appending will take place.</param>
        /// <param name="chrSeparator">The char to use as a separator. <paramref name="chrSeparator" /> is included in the returned string only if value has more than one element.</param>
        /// <param name="astrValues">An array that contains the string to append.</param>
        /// <returns><paramref name="sbdInput" /> with values appended.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder AppendJoin([NotNull] this StringBuilder sbdInput, char chrSeparator, params string[] astrValues)
        {
            if (astrValues == null)
                throw new ArgumentNullException(nameof(astrValues));
            for (int i = 0; i < astrValues.Length; ++i)
            {
                string strLoop = astrValues[i];
                if (i > 0)
                    sbdInput.Append(chrSeparator);
                sbdInput.Append(strLoop);
            }
            return sbdInput;
        }

        /// <summary>
        /// Combination of StringBuilder::Append() and static string::Join(), appending an list of strings with a separator.
        /// </summary>
        /// <param name="sbdInput">Base StringBuilder onto which appending will take place.</param>
        /// <param name="chrSeparator">The char to use as a separator. <paramref name="chrSeparator" /> is included in the returned string only if value has more than one element.</param>
        /// <param name="aobjValues">An array that contains the objects to append.</param>
        /// <returns><paramref name="sbdInput" /> with values appended.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder AppendJoin([NotNull] this StringBuilder sbdInput, char chrSeparator, params object[] aobjValues)
        {
            if (aobjValues == null)
                throw new ArgumentNullException(nameof(aobjValues));
            for (int i = 0; i < aobjValues.Length; ++i)
            {
                string strLoop = aobjValues[i].ToString();
                if (i > 0)
                    sbdInput.Append(chrSeparator);
                sbdInput.Append(strLoop);
            }
            return sbdInput;
        }
    }
}
