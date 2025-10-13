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
using System.Threading;
using System.Threading.Tasks;
using Chummer.Annotations;

namespace Chummer
{
    internal static class StringBuilderExtensions
    {
        /// <summary>
        /// Syntactic sugar for enumerating through a StringBuilder's characters.
        /// </summary>
        /// <param name="sbdInput">StringBuilder whose contents should enumerated.</param>
        public static IEnumerable<char> Enumerate([NotNull] this StringBuilder sbdInput)
        {
            if (sbdInput.Length == 0)
                yield break;
            for (int i = 0; i < sbdInput.Length; ++i)
                yield return sbdInput[i];
        }

        /// <summary>
        /// Like <see cref="StringBuilder.Replace(string, string)"/>, but meant for if the new value would be expensive to calculate. Actually slower than <see cref="StringBuilder.Replace(string, string)"/> if the new value is something simple.
        /// If the string does not contain any instances of the pattern to replace, then the expensive method to generate a replacement is not run.
        /// </summary>
        /// <param name="sbdInput">Base StringBuilder in which the replacing takes place. Note that <see cref="StringBuilder.ToString()"/> will be applied to this as part of the method, so it may not be as cheap.</param>
        /// <param name="strOldValue">Pattern for which to check and which to replace.</param>
        /// <param name="funcNewValueFactory">Function to generate the string that replaces the pattern in the base string.</param>
        /// <param name="eStringComparison">The StringComparison to use for finding and replacing items.</param>
        /// <returns>The result of <see cref="StringBuilder.Replace(string, string)"/> if a replacement is made, the original string otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder CheapReplace([NotNull] this StringBuilder sbdInput, string strOldValue, Func<string> funcNewValueFactory, StringComparison eStringComparison = StringComparison.Ordinal)
        {
            return sbdInput.CheapReplace(sbdInput.ToString(), strOldValue, funcNewValueFactory, eStringComparison);
        }

        /// <summary>
        /// Like <see cref="StringBuilder.Replace(string, string)"/>, but meant for if the new value would be expensive to calculate. Actually slower than <see cref="StringBuilder.Replace(string, string)"/> if the new value is something simple.
        /// If the string does not contain any instances of the pattern to replace, then the expensive method to generate a replacement is not run.
        /// </summary>
        /// <param name="sbdInput">Base StringBuilder in which the replacing takes place.</param>
        /// <param name="strOriginal">Original string around which StringBuilder was created. Set this so that <see cref="StringBuilder.ToString()"/> does not need to be called.</param>
        /// <param name="strOldValue">Pattern for which to check and which to replace.</param>
        /// <param name="funcNewValueFactory">Function to generate the string that replaces the pattern in the base string.</param>
        /// <param name="eStringComparison">The StringComparison to use for finding and replacing items.</param>
        /// <returns>The result of <see cref="StringBuilder.Replace(string, string)"/> if a replacement is made, the original string otherwise.</returns>
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
        /// Like <see cref="StringBuilder.Replace(string, string)"/>, but meant for if the new value would be expensive to calculate. Actually slower than <see cref="StringBuilder.Replace(string, string)"/> if the new value is something simple.
        /// This is the async version that can be run in case a value is really expensive to get.
        /// If the string does not contain any instances of the pattern to replace, then the expensive method to generate a replacement is not run.
        /// </summary>
        /// <param name="sbdInput">Base StringBuilder in which the replacing takes place. Note that <see cref="StringBuilder.ToString()"/> will be applied to this as part of the method, so it may not be as cheap.</param>
        /// <param name="strOldValue">Pattern for which to check and which to replace.</param>
        /// <param name="funcNewValueFactory">Function to generate the string that replaces the pattern in the base string.</param>
        /// <param name="eStringComparison">The StringComparison to use for finding and replacing items.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>The result of <see cref="StringBuilder.Replace(string, string)"/> if a replacement is made, the original string otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<StringBuilder> CheapReplaceAsync([NotNull] this StringBuilder sbdInput, string strOldValue, Func<string> funcNewValueFactory, StringComparison eStringComparison = StringComparison.Ordinal, CancellationToken token = default)
        {
            return sbdInput.CheapReplaceAsync(sbdInput.ToString(), strOldValue, funcNewValueFactory, eStringComparison, token: token);
        }

        /// <summary>
        /// Like <see cref="StringBuilder.Replace(string, string)"/>, but meant for if the new value would be expensive to calculate. Actually slower than <see cref="StringBuilder.Replace(string, string)"/> if the new value is something simple.
        /// If the string does not contain any instances of the pattern to replace, then the expensive method to generate a replacement is not run.
        /// </summary>
        /// <param name="sbdInput">Base StringBuilder in which the replacing takes place.</param>
        /// <param name="strOriginal">Original string around which StringBuilder was created. Set this so that <see cref="StringBuilder.ToString()"/> does not need to be called.</param>
        /// <param name="strOldValue">Pattern for which to check and which to replace.</param>
        /// <param name="funcNewValueFactory">Function to generate the string that replaces the pattern in the base string.</param>
        /// <param name="eStringComparison">The StringComparison to use for finding and replacing items.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>The result of <see cref="StringBuilder.Replace(string, string)"/> if a replacement is made, the original string otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<StringBuilder> CheapReplaceAsync([NotNull] this StringBuilder sbdInput, string strOriginal, string strOldValue, Func<string> funcNewValueFactory, StringComparison eStringComparison = StringComparison.Ordinal, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (sbdInput.Length > 0 && !string.IsNullOrEmpty(strOriginal) && funcNewValueFactory != null)
            {
                token.ThrowIfCancellationRequested();
                if (eStringComparison == StringComparison.Ordinal)
                {
                    if (strOriginal.Contains(strOldValue))
                    {
                        token.ThrowIfCancellationRequested();
                        string strFactoryResult = string.Empty;
                        using (CancellationTokenTaskSource<string> objCancellationTokenTaskSource
                               = new CancellationTokenTaskSource<string>(token))
                        {
                            await Task.WhenAny(Task.Factory.FromAsync(funcNewValueFactory.BeginInvoke,
                                                                      x => strFactoryResult
                                                                          = funcNewValueFactory.EndInvoke(x),
                                                                      null), objCancellationTokenTaskSource.Task).ConfigureAwait(false);
                        }
                        token.ThrowIfCancellationRequested();
                        sbdInput.Replace(strOldValue, strFactoryResult);
                    }
                }
                else if (strOriginal.IndexOf(strOldValue, eStringComparison) != -1)
                {
                    token.ThrowIfCancellationRequested();
                    string strFactoryResult = string.Empty;
                    string strOldStringBuilderValue;
                    using (CancellationTokenTaskSource<string> objCancellationTokenTaskSource
                           = new CancellationTokenTaskSource<string>(token))
                    {
                        Task<string> tskGetValue = Task.Factory.FromAsync(funcNewValueFactory.BeginInvoke,
                                                                          x => strFactoryResult
                                                                              = funcNewValueFactory.EndInvoke(x), null);
                        strOldStringBuilderValue = sbdInput.ToString();
                        sbdInput.Clear();
                        await Task.WhenAny(tskGetValue, objCancellationTokenTaskSource.Task).ConfigureAwait(false);
                    }
                    token.ThrowIfCancellationRequested();
                    sbdInput.Append(
                            strOldStringBuilderValue.Replace(strOldValue, strFactoryResult, eStringComparison));
                }
            }

            return sbdInput;
        }

        /// <summary>
        /// Like <see cref="StringBuilder.Replace(string, string)"/>, but meant for if the new value would be expensive to calculate. Actually slower than <see cref="StringBuilder.Replace(string, string)"/> if the new value is something simple.
        /// This is the async version that can be run in case a value is really expensive to get.
        /// If the string does not contain any instances of the pattern to replace, then the expensive method to generate a replacement is not run.
        /// </summary>
        /// <param name="sbdInput">Base StringBuilder in which the replacing takes place. Note that <see cref="StringBuilder.ToString()"/> will be applied to this as part of the method, so it may not be as cheap.</param>
        /// <param name="strOldValue">Pattern for which to check and which to replace.</param>
        /// <param name="funcNewValueFactory">Function to generate the string that replaces the pattern in the base string.</param>
        /// <param name="eStringComparison">The StringComparison to use for finding and replacing items.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>The result of <see cref="StringBuilder.Replace(string, string)"/> if a replacement is made, the original string otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<StringBuilder> CheapReplaceAsync([NotNull] this StringBuilder sbdInput, string strOldValue, Func<Task<string>> funcNewValueFactory, StringComparison eStringComparison = StringComparison.Ordinal, CancellationToken token = default)
        {
            return sbdInput.CheapReplaceAsync(sbdInput.ToString(), strOldValue, funcNewValueFactory, eStringComparison, token: token);
        }

        /// <summary>
        /// Like <see cref="StringBuilder.Replace(string, string)"/>, but meant for if the new value would be expensive to calculate. Actually slower than <see cref="StringBuilder.Replace(string, string)"/> if the new value is something simple.
        /// If the string does not contain any instances of the pattern to replace, then the expensive method to generate a replacement is not run.
        /// </summary>
        /// <param name="sbdInput">Base StringBuilder in which the replacing takes place.</param>
        /// <param name="strOriginal">Original string around which StringBuilder was created. Set this so that <see cref="StringBuilder.ToString()"/> does not need to be called.</param>
        /// <param name="strOldValue">Pattern for which to check and which to replace.</param>
        /// <param name="funcNewValueFactory">Function to generate the string that replaces the pattern in the base string.</param>
        /// <param name="eStringComparison">The StringComparison to use for finding and replacing items.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>The result of <see cref="StringBuilder.Replace(string, string)"/> if a replacement is made, the original string otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<StringBuilder> CheapReplaceAsync([NotNull] this StringBuilder sbdInput, string strOriginal, string strOldValue, Func<Task<string>> funcNewValueFactory, StringComparison eStringComparison = StringComparison.Ordinal, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (sbdInput.Length > 0 && !string.IsNullOrEmpty(strOriginal) && funcNewValueFactory != null)
            {
                token.ThrowIfCancellationRequested();
                if (eStringComparison == StringComparison.Ordinal)
                {
                    if (strOriginal.Contains(strOldValue))
                    {
                        token.ThrowIfCancellationRequested();
                        Task<string> tskReplaceTask = funcNewValueFactory.Invoke();
                        using (CancellationTokenTaskSource<string> objCancellationTokenTaskSource
                               = new CancellationTokenTaskSource<string>(token))
                        {
                            await Task.WhenAny(tskReplaceTask, objCancellationTokenTaskSource.Task).ConfigureAwait(false);
                        }
                        token.ThrowIfCancellationRequested();
                        sbdInput.Replace(strOldValue, await tskReplaceTask.ConfigureAwait(false));
                    }
                }
                else if (strOriginal.IndexOf(strOldValue, eStringComparison) != -1)
                {
                    token.ThrowIfCancellationRequested();
                    Task<string> tskReplaceTask = funcNewValueFactory.Invoke();
                    string strOldStringBuilderValue = sbdInput.ToString();
                    sbdInput.Clear();
                    token.ThrowIfCancellationRequested();
                    using (CancellationTokenTaskSource<string> objCancellationTokenTaskSource
                           = new CancellationTokenTaskSource<string>(token))
                    {
                        await Task.WhenAny(tskReplaceTask, objCancellationTokenTaskSource.Task).ConfigureAwait(false);
                    }
                    token.ThrowIfCancellationRequested();
                    sbdInput.Append(strOldStringBuilderValue.Replace(strOldValue, await tskReplaceTask.ConfigureAwait(false), eStringComparison));
                }
            }

            return sbdInput;
        }

        /// <summary>
        /// Combination of <see cref="StringBuilder.Append(string)"/> and <see cref="string.Join(string, string[])"/>, appending a list of strings with a separator.
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
            using (IEnumerator<T> objEnumerator = lstValues.GetEnumerator())
            {
                if (objEnumerator.MoveNext())
                {
                    string strLoop = objEnumerator.Current?.ToString();
                    if (string.IsNullOrEmpty(strLoop))
                    {
                        while (objEnumerator.MoveNext())
                        {
                            strLoop = objEnumerator.Current?.ToString();
                            if (!string.IsNullOrEmpty(strLoop))
                            {
                                sbdInput.Append(strLoop);
                                break;
                            }
                        }
                    }
                    else
                        sbdInput.Append(strLoop);
                    while (objEnumerator.MoveNext())
                    {
                        strLoop = objEnumerator.Current?.ToString();
                        if (!string.IsNullOrEmpty(strLoop))
                            sbdInput.Append(strSeparator).Append(strLoop);
                    }
                }
                return sbdInput;
            }
        }

        /// <summary>
        /// Combination of <see cref="StringBuilder.Append(string)"/> and <see cref="string.Join(string, string[])"/>, appending a list of strings with a separator.
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
            using (IEnumerator<string> objEnumerator = lstValues.GetEnumerator())
            {
                if (objEnumerator.MoveNext())
                {
                    string strLoop = objEnumerator.Current;
                    if (string.IsNullOrEmpty(strLoop))
                    {
                        while (objEnumerator.MoveNext())
                        {
                            strLoop = objEnumerator.Current;
                            if (!string.IsNullOrEmpty(strLoop))
                            {
                                sbdInput.Append(strLoop);
                                break;
                            }
                        }
                    }
                    else
                        sbdInput.Append(strLoop);
                    while (objEnumerator.MoveNext())
                    {
                        strLoop = objEnumerator.Current;
                        if (!string.IsNullOrEmpty(strLoop))
                            sbdInput.Append(strSeparator).Append(strLoop);
                    }
                }
                return sbdInput;
            }
        }

        /// <summary>
        /// Combination of <see cref="StringBuilder.Append(string)"/> and <see cref="string.Join(string, string[], int, int)"/>, appending a list of strings with a separator.
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
            if (intCount > 0)
            {
                int intExtraLength = 0;
                for (int j = 0; j < intCount; ++j)
                {
                    string s = astrValues[j];
                    if (!string.IsNullOrEmpty(s))
                        intExtraLength += s.Length + strSeparator.Length;
                }
                sbdInput.EnsureCapacity(sbdInput.Length + intExtraLength);
                int i = 1;
                string strLoop = astrValues[intStartIndex];
                if (string.IsNullOrEmpty(strLoop))
                {
                    for (; i < intCount; ++i)
                    {
                        strLoop = astrValues[i + intStartIndex];
                        if (!string.IsNullOrEmpty(strLoop))
                        {
                            sbdInput.Append(strLoop);
                            break;
                        }
                    }
                }
                else
                    sbdInput.Append(strLoop);
                for (; i < intCount; ++i)
                {
                    strLoop = astrValues[i + intStartIndex];
                    if (!string.IsNullOrEmpty(strLoop))
                        sbdInput.Append(strSeparator).Append(strLoop);
                }
            }
            return sbdInput;
        }

        /// <summary>
        /// Combination of <see cref="StringBuilder.Append(string)"/> and <see cref="string.Join(string, string[])"/>, appending a list of strings with a separator.
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
            int intLength = astrValues.Length;
            if (intLength > 0)
            {
                int intExtraLength = 0;
                for (int j = 0; j < intLength; ++j)
                {
                    string s = astrValues[j];
                    if (!string.IsNullOrEmpty(s))
                        intExtraLength += s.Length + strSeparator.Length;
                }
                sbdInput.EnsureCapacity(sbdInput.Length + intExtraLength);
                int i = 1;
                string strLoop = astrValues[0];
                if (string.IsNullOrEmpty(strLoop))
                {
                    for (; i < intLength; ++i)
                    {
                        strLoop = astrValues[i];
                        if (!string.IsNullOrEmpty(strLoop))
                        {
                            sbdInput.Append(strLoop);
                            break;
                        }
                    }
                }
                else
                    sbdInput.Append(strLoop);
                for (; i < intLength; ++i)
                {
                    strLoop = astrValues[i];
                    if (!string.IsNullOrEmpty(strLoop))
                        sbdInput.Append(strSeparator).Append(strLoop);
                }
            }
            return sbdInput;
        }

        /// <summary>
        /// Combination of <see cref="StringBuilder.Append(object)"/> and <see cref="string.Join(string, object[])"/>, appending a list of strings with a separator.
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
            int intLength = aobjValues.Length;
            if (intLength > 0)
            {
                int i = 1;
                string strLoop = aobjValues[0]?.ToString();
                if (string.IsNullOrEmpty(strLoop))
                {
                    for (; i < intLength; ++i)
                    {
                        strLoop = aobjValues[i]?.ToString();
                        if (!string.IsNullOrEmpty(strLoop))
                        {
                            sbdInput.Append(strLoop);
                            break;
                        }
                    }
                }
                else
                    sbdInput.Append(strLoop);
                for (; i < intLength; ++i)
                {
                    strLoop = aobjValues[i]?.ToString();
                    if (!string.IsNullOrEmpty(strLoop))
                        sbdInput.Append(strSeparator).Append(strLoop);
                }
            }
            return sbdInput;
        }

        /// <summary>
        /// Combination of <see cref="StringBuilder.Append(object)"/> and <see cref="string.Join(string, object[])"/>, appending a list of strings with a separator.
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
            using (IEnumerator<T> objEnumerator = lstValues.GetEnumerator())
            {
                if (objEnumerator.MoveNext())
                {
                    string strLoop = objEnumerator.Current?.ToString();
                    if (string.IsNullOrEmpty(strLoop))
                    {
                        while (objEnumerator.MoveNext())
                        {
                            strLoop = objEnumerator.Current?.ToString();
                            if (!string.IsNullOrEmpty(strLoop))
                            {
                                sbdInput.Append(strLoop);
                                break;
                            }
                        }
                    }
                    else
                        sbdInput.Append(strLoop);
                    while (objEnumerator.MoveNext())
                    {
                        strLoop = objEnumerator.Current?.ToString();
                        if (!string.IsNullOrEmpty(strLoop))
                            sbdInput.Append(chrSeparator).Append(strLoop);
                    }
                }
                return sbdInput;
            }
        }

        /// <summary>
        /// Combination of <see cref="StringBuilder.Append(string)"/> and <see cref="string.Join(string, string[])"/>, appending a list of strings with a separator.
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
            using (IEnumerator<string> objEnumerator = lstValues.GetEnumerator())
            {
                if (objEnumerator.MoveNext())
                {
                    string strLoop = objEnumerator.Current;
                    if (string.IsNullOrEmpty(strLoop))
                    {
                        while (objEnumerator.MoveNext())
                        {
                            strLoop = objEnumerator.Current;
                            if (!string.IsNullOrEmpty(strLoop))
                            {
                                sbdInput.Append(strLoop);
                                break;
                            }
                        }
                    }
                    else
                        sbdInput.Append(strLoop);
                    while (objEnumerator.MoveNext())
                    {
                        strLoop = objEnumerator.Current;
                        if (!string.IsNullOrEmpty(strLoop))
                            sbdInput.Append(chrSeparator).Append(strLoop);
                    }
                }
                return sbdInput;
            }
        }

        /// <summary>
        /// Combination of <see cref="StringBuilder.Append(string)"/> and <see cref="string.Join(string, string[], int, int)"/>, appending a list of strings with a separator.
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
            if (intCount > 0)
            {
                int intExtraLength = 0;
                for (int j = 0; j < intCount; ++j)
                {
                    string s = astrValues[j];
                    if (!string.IsNullOrEmpty(s))
                        intExtraLength += s.Length + 1;
                }
                sbdInput.EnsureCapacity(sbdInput.Length + intExtraLength);
                int i = 1;
                string strLoop = astrValues[intStartIndex];
                if (string.IsNullOrEmpty(strLoop))
                {
                    for (; i < intCount; ++i)
                    {
                        strLoop = astrValues[i + intStartIndex];
                        if (!string.IsNullOrEmpty(strLoop))
                        {
                            sbdInput.Append(strLoop);
                            break;
                        }
                    }
                }
                else
                    sbdInput.Append(strLoop);
                for (; i < intCount; ++i)
                {
                    strLoop = astrValues[i + intStartIndex];
                    if (!string.IsNullOrEmpty(strLoop))
                        sbdInput.Append(chrSeparator).Append(strLoop);
                }
            }
            return sbdInput;
        }

        /// <summary>
        /// Combination of <see cref="StringBuilder.Append(string)"/> and <see cref="string.Join(string, string[])"/>, appending a list of strings with a separator.
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
            int intLength = astrValues.Length;
            if (intLength > 0)
            {
                int intExtraLength = 0;
                for (int j = 0; j < intLength; ++j)
                {
                    string s = astrValues[j];
                    if (!string.IsNullOrEmpty(s))
                        intExtraLength += s.Length + 1;
                }
                sbdInput.EnsureCapacity(sbdInput.Length + intExtraLength);
                int i = 1;
                string strLoop = astrValues[0];
                if (string.IsNullOrEmpty(strLoop))
                {
                    for (; i < intLength; ++i)
                    {
                        strLoop = astrValues[i];
                        if (!string.IsNullOrEmpty(strLoop))
                        {
                            sbdInput.Append(strLoop);
                            break;
                        }
                    }
                }
                else
                    sbdInput.Append(strLoop);
                for (; i < intLength; ++i)
                {
                    strLoop = astrValues[i];
                    if (!string.IsNullOrEmpty(strLoop))
                        sbdInput.Append(chrSeparator).Append(strLoop);
                }
            }
            return sbdInput;
        }

        /// <summary>
        /// Combination of <see cref="StringBuilder.Append(object)"/> and <see cref="string.Join(string, object[])"/>, appending a list of strings with a separator.
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
            int intLength = aobjValues.Length;
            if (intLength > 0)
            {
                int i = 1;
                string strLoop = aobjValues[0]?.ToString();
                if (string.IsNullOrEmpty(strLoop))
                {
                    for (; i < intLength; ++i)
                    {
                        strLoop = aobjValues[i]?.ToString();
                        if (!string.IsNullOrEmpty(strLoop))
                        {
                            sbdInput.Append(strLoop);
                            break;
                        }
                    }
                }
                else
                    sbdInput.Append(strLoop);
                for (; i < intLength; ++i)
                {
                    strLoop = aobjValues[i]?.ToString();
                    if (!string.IsNullOrEmpty(strLoop))
                        sbdInput.Append(chrSeparator).Append(strLoop);
                }
            }
            return sbdInput;
        }

        /// <summary>
        /// Async combination of <see cref="StringBuilder.Append(object)"/> and <see cref="string.Join(string, object[])"/>, appending a list of strings with a separator.
        /// </summary>
        /// <param name="sbdInput">Base StringBuilder onto which appending will take place.</param>
        /// <param name="strSeparator">The string to use as a separator. <paramref name="strSeparator" /> is included in the returned string only if value has more than one element.</param>
        /// <param name="lstValues">A collection that contains the strings to append.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns><paramref name="sbdInput" /> with values appended.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<StringBuilder> AppendJoinAsync<T>([NotNull] this StringBuilder sbdInput, string strSeparator, IEnumerable<Task<T>> lstValues, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (lstValues == null)
                throw new ArgumentNullException(nameof(lstValues));
            using (IEnumerator<Task<T>> objEnumerator = lstValues.GetEnumerator())
            {
                token.ThrowIfCancellationRequested();
                if (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    Task<T> tskCurrent = objEnumerator.Current;
                    string strLoop = tskCurrent != null ? (await tskCurrent.ConfigureAwait(false))?.ToString() : string.Empty;
                    token.ThrowIfCancellationRequested();
                    if (string.IsNullOrEmpty(strLoop))
                    {
                        while (objEnumerator.MoveNext())
                        {
                            token.ThrowIfCancellationRequested();
                            tskCurrent = objEnumerator.Current;
                            strLoop = tskCurrent != null ? (await tskCurrent.ConfigureAwait(false))?.ToString() : string.Empty;
                            token.ThrowIfCancellationRequested();
                            if (!string.IsNullOrEmpty(strLoop))
                            {
                                sbdInput.Append(strLoop);
                                break;
                            }
                        }
                    }
                    else
                        sbdInput.Append(strLoop);
                    while (objEnumerator.MoveNext())
                    {
                        token.ThrowIfCancellationRequested();
                        tskCurrent = objEnumerator.Current;
                        strLoop = tskCurrent != null ? (await tskCurrent.ConfigureAwait(false))?.ToString() : string.Empty;
                        token.ThrowIfCancellationRequested();
                        if (!string.IsNullOrEmpty(strLoop))
                            sbdInput.Append(strSeparator).Append(strLoop);
                    }
                }
                return sbdInput;
            }
        }

        /// <summary>
        /// Async combination of <see cref="StringBuilder.Append(string)"/> and <see cref="string.Join(string, string[])"/>, appending a list of strings with a separator.
        /// </summary>
        /// <param name="sbdInput">Base StringBuilder onto which appending will take place.</param>
        /// <param name="strSeparator">The string to use as a separator. <paramref name="strSeparator" /> is included in the returned string only if value has more than one element.</param>
        /// <param name="lstValues">A collection that contains the strings to append.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns><paramref name="sbdInput" /> with values appended.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<StringBuilder> AppendJoinAsync([NotNull] this StringBuilder sbdInput, string strSeparator, IEnumerable<Task<string>> lstValues, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (lstValues == null)
                throw new ArgumentNullException(nameof(lstValues));
            using (IEnumerator<Task<string>> objEnumerator = lstValues.GetEnumerator())
            {
                token.ThrowIfCancellationRequested();
                if (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    Task<string> tskCurrent = objEnumerator.Current;
                    string strLoop = tskCurrent != null ? await tskCurrent.ConfigureAwait(false) : string.Empty;
                    token.ThrowIfCancellationRequested();
                    if (string.IsNullOrEmpty(strLoop))
                    {
                        while (objEnumerator.MoveNext())
                        {
                            token.ThrowIfCancellationRequested();
                            tskCurrent = objEnumerator.Current;
                            strLoop = tskCurrent != null ? await tskCurrent.ConfigureAwait(false) : string.Empty;
                            token.ThrowIfCancellationRequested();
                            if (!string.IsNullOrEmpty(strLoop))
                            {
                                sbdInput.Append(strLoop);
                                break;
                            }
                        }
                    }
                    else
                        sbdInput.Append(strLoop);
                    while (objEnumerator.MoveNext())
                    {
                        token.ThrowIfCancellationRequested();
                        tskCurrent = objEnumerator.Current;
                        strLoop = tskCurrent != null ? await tskCurrent.ConfigureAwait(false) : string.Empty;
                        token.ThrowIfCancellationRequested();
                        if (!string.IsNullOrEmpty(strLoop))
                            sbdInput.Append(strSeparator).Append(strLoop);
                    }
                }
                return sbdInput;
            }
        }

        /// <summary>
        /// Async combination of <see cref="StringBuilder.Append(string)"/> and <see cref="string.Join(string, string[], int, int)"/>, appending a list of strings with a separator.
        /// </summary>
        /// <param name="sbdInput">Base StringBuilder onto which appending will take place.</param>
        /// <param name="strSeparator">The string to use as a separator. <paramref name="strSeparator" /> is included in the returned string only if value has more than one element.</param>
        /// <param name="astrValues">An array that contains the string to append.</param>
        /// <param name="intStartIndex">The first element in <paramref name="astrValues" /> to use.</param>
        /// <param name="intCount">The number of elements of <paramref name="astrValues" /> to use.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns><paramref name="sbdInput" /> with values appended.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<StringBuilder> AppendJoinAsync([NotNull] this StringBuilder sbdInput, string strSeparator, Task<string>[] astrValues, int intStartIndex, int intCount, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (astrValues == null)
                throw new ArgumentNullException(nameof(astrValues));
            if (intStartIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(intStartIndex));
            if (intCount < 0)
                throw new ArgumentOutOfRangeException(nameof(intCount));
            if (intStartIndex + intCount >= astrValues.Length)
                throw new ArgumentOutOfRangeException(nameof(intStartIndex));
            if (intCount > 0)
            {
                token.ThrowIfCancellationRequested();
                int i = 1;
                string strLoop = await astrValues[intStartIndex].ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                if (string.IsNullOrEmpty(strLoop))
                {
                    for (; i < intCount; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        strLoop = await astrValues[i + intStartIndex].ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        if (!string.IsNullOrEmpty(strLoop))
                        {
                            sbdInput.Append(strLoop);
                            break;
                        }
                    }
                }
                else
                    sbdInput.Append(strLoop);
                for (; i < intCount; ++i)
                {
                    token.ThrowIfCancellationRequested();
                    strLoop = await astrValues[i + intStartIndex].ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    if (!string.IsNullOrEmpty(strLoop))
                        sbdInput.Append(strSeparator).Append(strLoop);
                }
            }
            return sbdInput;
        }

        /// <summary>
        /// Async combination of <see cref="StringBuilder.Append(string)"/> and <see cref="string.Join(string, string[])"/>, appending a list of strings with a separator.
        /// </summary>
        /// <param name="sbdInput">Base StringBuilder onto which appending will take place.</param>
        /// <param name="strSeparator">The string to use as a separator. <paramref name="strSeparator" /> is included in the returned string only if value has more than one element.</param>
        /// <param name="astrValues">An array that contains the string to append.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns><paramref name="sbdInput" /> with values appended.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<StringBuilder> AppendJoinAsync([NotNull] this StringBuilder sbdInput, string strSeparator, CancellationToken token = default, params Task<string>[] astrValues)
        {
            token.ThrowIfCancellationRequested();
            if (astrValues == null)
                throw new ArgumentNullException(nameof(astrValues));
            int intLength = astrValues.Length;
            if (intLength > 0)
            {
                token.ThrowIfCancellationRequested();
                int i = 1;
                string strLoop = await astrValues[0].ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                if (string.IsNullOrEmpty(strLoop))
                {
                    for (; i < intLength; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        strLoop = await astrValues[i].ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        if (!string.IsNullOrEmpty(strLoop))
                        {
                            sbdInput.Append(strLoop);
                            break;
                        }
                    }
                }
                else
                    sbdInput.Append(strLoop);
                for (; i < intLength; ++i)
                {
                    token.ThrowIfCancellationRequested();
                    strLoop = await astrValues[i].ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    if (!string.IsNullOrEmpty(strLoop))
                        sbdInput.Append(strSeparator).Append(strLoop);
                }
            }
            return sbdInput;
        }

        /// <summary>
        /// Async combination of <see cref="StringBuilder.Append(object)"/> and <see cref="string.Join(string, object[])"/>, appending a list of strings with a separator.
        /// </summary>
        /// <param name="sbdInput">Base StringBuilder onto which appending will take place.</param>
        /// <param name="strSeparator">The string to use as a separator. <paramref name="strSeparator" /> is included in the returned string only if value has more than one element.</param>
        /// <param name="aobjValues">An array that contains the objects to append.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns><paramref name="sbdInput" /> with values appended.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<StringBuilder> AppendJoinAsync([NotNull] this StringBuilder sbdInput, string strSeparator, CancellationToken token = default, params Task<object>[] aobjValues)
        {
            token.ThrowIfCancellationRequested();
            if (aobjValues == null)
                throw new ArgumentNullException(nameof(aobjValues));
            int intLength = aobjValues.Length;
            if (intLength > 0)
            {
                token.ThrowIfCancellationRequested();
                int i = 1;
                string strLoop = (await aobjValues[0].ConfigureAwait(false))?.ToString();
                token.ThrowIfCancellationRequested();
                if (string.IsNullOrEmpty(strLoop))
                {
                    for (; i < intLength; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        strLoop = (await aobjValues[i].ConfigureAwait(false))?.ToString();
                        token.ThrowIfCancellationRequested();
                        if (!string.IsNullOrEmpty(strLoop))
                        {
                            sbdInput.Append(strLoop);
                            break;
                        }
                    }
                }
                else
                    sbdInput.Append(strLoop);
                for (; i < intLength; ++i)
                {
                    token.ThrowIfCancellationRequested();
                    strLoop = (await aobjValues[i].ConfigureAwait(false))?.ToString();
                    token.ThrowIfCancellationRequested();
                    if (!string.IsNullOrEmpty(strLoop))
                        sbdInput.Append(strSeparator).Append(strLoop);
                }
            }
            return sbdInput;
        }

        /// <summary>
        /// Async combination of <see cref="StringBuilder.Append(object)"/> and <see cref="string.Join(string, object[])"/>, appending a list of strings with a separator.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sbdInput">Base StringBuilder onto which appending will take place.</param>
        /// <param name="chrSeparator">The char to use as a separator. <paramref name="chrSeparator" /> is included in the returned string only if value has more than one element.</param>
        /// <param name="lstValues">A collection that contains the objects to append.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns><paramref name="sbdInput" /> with values appended.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<StringBuilder> AppendJoinAsync<T>([NotNull] this StringBuilder sbdInput, char chrSeparator, IEnumerable<Task<T>> lstValues, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (lstValues == null)
                throw new ArgumentNullException(nameof(lstValues));
            using (IEnumerator<Task<T>> objEnumerator = lstValues.GetEnumerator())
            {
                token.ThrowIfCancellationRequested();
                if (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (objEnumerator.MoveNext())
                    {
                        token.ThrowIfCancellationRequested();
                        Task<T> tskCurrent = objEnumerator.Current;
                        string strLoop = tskCurrent != null ? (await tskCurrent.ConfigureAwait(false))?.ToString() : string.Empty;
                        token.ThrowIfCancellationRequested();
                        if (string.IsNullOrEmpty(strLoop))
                        {
                            while (objEnumerator.MoveNext())
                            {
                                token.ThrowIfCancellationRequested();
                                tskCurrent = objEnumerator.Current;
                                strLoop = tskCurrent != null ? (await tskCurrent.ConfigureAwait(false))?.ToString() : string.Empty;
                                token.ThrowIfCancellationRequested();
                                if (!string.IsNullOrEmpty(strLoop))
                                {
                                    sbdInput.Append(strLoop);
                                    break;
                                }
                            }
                        }
                        else
                            sbdInput.Append(strLoop);
                        while (objEnumerator.MoveNext())
                        {
                            token.ThrowIfCancellationRequested();
                            tskCurrent = objEnumerator.Current;
                            strLoop = tskCurrent != null ? (await tskCurrent.ConfigureAwait(false))?.ToString() : string.Empty;
                            token.ThrowIfCancellationRequested();
                            if (!string.IsNullOrEmpty(strLoop))
                                sbdInput.Append(chrSeparator).Append(strLoop);
                        }
                    }
                }
                return sbdInput;
            }
        }

        /// <summary>
        /// Async combination of <see cref="StringBuilder.Append(string)"/> and <see cref="string.Join(string, string[])"/>, appending a list of strings with a separator.
        /// </summary>
        /// <param name="sbdInput">Base StringBuilder onto which appending will take place.</param>
        /// <param name="chrSeparator">The char to use as a separator. <paramref name="chrSeparator" /> is included in the returned string only if value has more than one element.</param>
        /// <param name="lstValues">A collection that contains the strings to append.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns><paramref name="sbdInput" /> with values appended.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<StringBuilder> AppendJoinAsync([NotNull] this StringBuilder sbdInput, char chrSeparator, IEnumerable<Task<string>> lstValues, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (lstValues == null)
                throw new ArgumentNullException(nameof(lstValues));
            token.ThrowIfCancellationRequested();
            using (IEnumerator<Task<string>> objEnumerator = lstValues.GetEnumerator())
            {
                token.ThrowIfCancellationRequested();
                if (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    if (objEnumerator.MoveNext())
                    {
                        token.ThrowIfCancellationRequested();
                        Task<string> tskCurrent = objEnumerator.Current;
                        string strLoop = tskCurrent != null ? await tskCurrent.ConfigureAwait(false) : string.Empty;
                        token.ThrowIfCancellationRequested();
                        if (string.IsNullOrEmpty(strLoop))
                        {
                            while (objEnumerator.MoveNext())
                            {
                                token.ThrowIfCancellationRequested();
                                tskCurrent = objEnumerator.Current;
                                strLoop = tskCurrent != null ? await tskCurrent.ConfigureAwait(false) : string.Empty;
                                token.ThrowIfCancellationRequested();
                                if (!string.IsNullOrEmpty(strLoop))
                                {
                                    sbdInput.Append(strLoop);
                                    break;
                                }
                            }
                        }
                        else
                            sbdInput.Append(strLoop);
                        while (objEnumerator.MoveNext())
                        {
                            token.ThrowIfCancellationRequested();
                            tskCurrent = objEnumerator.Current;
                            strLoop = tskCurrent != null ? await tskCurrent.ConfigureAwait(false) : string.Empty;
                            token.ThrowIfCancellationRequested();
                            if (!string.IsNullOrEmpty(strLoop))
                                sbdInput.Append(chrSeparator).Append(strLoop);
                        }
                    }
                }
                return sbdInput;
            }
        }

        /// <summary>
        /// Async combination of <see cref="StringBuilder.Append(string)"/> and <see cref="string.Join(string, string[], int, int)"/>, appending a list of strings with a separator.
        /// </summary>
        /// <param name="sbdInput">Base StringBuilder onto which appending will take place.</param>
        /// <param name="chrSeparator">The char to use as a separator. <paramref name="chrSeparator" /> is included in the returned string only if value has more than one element.</param>
        /// <param name="astrValues">An array that contains the string to append.</param>
        /// <param name="intStartIndex">The first element in <paramref name="astrValues" /> to use.</param>
        /// <param name="intCount">The number of elements of <paramref name="astrValues" /> to use.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns><paramref name="sbdInput" /> with values appended.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<StringBuilder> AppendJoinAsync([NotNull] this StringBuilder sbdInput, char chrSeparator, Task<string>[] astrValues, int intStartIndex, int intCount, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (astrValues == null)
                throw new ArgumentNullException(nameof(astrValues));
            if (intStartIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(intStartIndex));
            if (intCount < 0)
                throw new ArgumentOutOfRangeException(nameof(intCount));
            if (intStartIndex + intCount >= astrValues.Length)
                throw new ArgumentOutOfRangeException(nameof(intStartIndex));
            if (intCount > 0)
            {
                token.ThrowIfCancellationRequested();
                int i = 1;
                string strLoop = await astrValues[intStartIndex].ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                if (string.IsNullOrEmpty(strLoop))
                {
                    for (; i < intCount; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        strLoop = await astrValues[i + intStartIndex].ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        if (!string.IsNullOrEmpty(strLoop))
                        {
                            sbdInput.Append(strLoop);
                            break;
                        }
                    }
                }
                else
                    sbdInput.Append(strLoop);
                for (; i < intCount; ++i)
                {
                    token.ThrowIfCancellationRequested();
                    strLoop = await astrValues[i + intStartIndex].ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    if (!string.IsNullOrEmpty(strLoop))
                        sbdInput.Append(chrSeparator).Append(strLoop);
                }
            }
            return sbdInput;
        }

        /// <summary>
        /// Async combination of <see cref="StringBuilder.Append(string)"/> and <see cref="string.Join(string, string[])"/>, appending a list of strings with a separator.
        /// </summary>
        /// <param name="sbdInput">Base StringBuilder onto which appending will take place.</param>
        /// <param name="chrSeparator">The char to use as a separator. <paramref name="chrSeparator" /> is included in the returned string only if value has more than one element.</param>
        /// <param name="astrValues">An array that contains the string to append.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns><paramref name="sbdInput" /> with values appended.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<StringBuilder> AppendJoinAsync([NotNull] this StringBuilder sbdInput, char chrSeparator, CancellationToken token = default, params Task<string>[] astrValues)
        {
            token.ThrowIfCancellationRequested();
            if (astrValues == null)
                throw new ArgumentNullException(nameof(astrValues));
            int intLength = astrValues.Length;
            if (intLength > 0)
            {
                token.ThrowIfCancellationRequested();
                int i = 1;
                string strLoop = await astrValues[0].ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                if (string.IsNullOrEmpty(strLoop))
                {
                    for (; i < intLength; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        strLoop = await astrValues[i].ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        if (!string.IsNullOrEmpty(strLoop))
                        {
                            sbdInput.Append(strLoop);
                            break;
                        }
                    }
                }
                else
                    sbdInput.Append(strLoop);
                for (; i < intLength; ++i)
                {
                    token.ThrowIfCancellationRequested();
                    strLoop = await astrValues[i].ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    if (!string.IsNullOrEmpty(strLoop))
                        sbdInput.Append(chrSeparator).Append(strLoop);
                }
            }
            return sbdInput;
        }

        /// <summary>
        /// Async combination of <see cref="StringBuilder.Append(object)"/> and <see cref="string.Join(string, object[])"/>, appending a list of strings with a separator.
        /// </summary>
        /// <param name="sbdInput">Base StringBuilder onto which appending will take place.</param>
        /// <param name="chrSeparator">The char to use as a separator. <paramref name="chrSeparator" /> is included in the returned string only if value has more than one element.</param>
        /// <param name="aobjValues">An array that contains the objects to append.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns><paramref name="sbdInput" /> with values appended.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<StringBuilder> AppendJoinAsync([NotNull] this StringBuilder sbdInput, char chrSeparator, CancellationToken token = default, params Task<object>[] aobjValues)
        {
            token.ThrowIfCancellationRequested();
            if (aobjValues == null)
                throw new ArgumentNullException(nameof(aobjValues));
            int intLength = aobjValues.Length;
            if (intLength > 0)
            {
                token.ThrowIfCancellationRequested();
                int i = 1;
                string strLoop = (await aobjValues[0].ConfigureAwait(false))?.ToString();
                token.ThrowIfCancellationRequested();
                if (string.IsNullOrEmpty(strLoop))
                {
                    for (; i < intLength; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        strLoop = (await aobjValues[i].ConfigureAwait(false))?.ToString();
                        token.ThrowIfCancellationRequested();
                        if (!string.IsNullOrEmpty(strLoop))
                        {
                            sbdInput.Append(strLoop);
                            break;
                        }
                    }
                }
                else
                    sbdInput.Append(strLoop);
                for (; i < intLength; ++i)
                {
                    token.ThrowIfCancellationRequested();
                    strLoop = (await aobjValues[i].ConfigureAwait(false))?.ToString();
                    token.ThrowIfCancellationRequested();
                    if (!string.IsNullOrEmpty(strLoop))
                        sbdInput.Append(chrSeparator).Append(strLoop);
                }
            }
            return sbdInput;
        }

        /// <summary>
        /// Async combination of <see cref="StringBuilder.Append(object)"/> and <see cref="string.Join(string, object[])"/>, appending a list of strings with a separator.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sbdInput">Base StringBuilder onto which appending will take place.</param>
        /// <param name="strSeparator">The string to use as a separator. <paramref name="strSeparator" /> is included in the returned string only if value has more than one element.</param>
        /// <param name="lstValues">A collection that contains the objects to append.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns><paramref name="sbdInput" /> with values appended.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<StringBuilder> AppendJoinAsync<T>([NotNull] this StringBuilder sbdInput, string strSeparator, IAsyncEnumerable<T> lstValues, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (lstValues == null)
                throw new ArgumentNullException(nameof(lstValues));
            IEnumerator<T> objEnumerator = lstValues.GetEnumerator();
            try
            {
                token.ThrowIfCancellationRequested();
                if (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    string strLoop = objEnumerator.Current?.ToString();
                    if (string.IsNullOrEmpty(strLoop))
                    {
                        while (objEnumerator.MoveNext())
                        {
                            strLoop = objEnumerator.Current?.ToString();
                            if (!string.IsNullOrEmpty(strLoop))
                            {
                                sbdInput.Append(strLoop);
                                break;
                            }
                        }
                    }
                    else
                        sbdInput.Append(strLoop);
                    token.ThrowIfCancellationRequested();
                    while (objEnumerator.MoveNext())
                    {
                        token.ThrowIfCancellationRequested();
                        strLoop = objEnumerator.Current?.ToString();
                        if (!string.IsNullOrEmpty(strLoop))
                            sbdInput.Append(strSeparator).Append(strLoop);
                    }
                }
                return sbdInput;
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objDisposeAsync)
                    await objDisposeAsync.DisposeAsync().ConfigureAwait(false);
                else if (objEnumerator is IDisposable objDispose)
                    objDispose.Dispose();
            }
        }

        /// <summary>
        /// Async combination of <see cref="StringBuilder.Append(string)"/> and <see cref="string.Join(string, string[])"/>, appending a list of strings with a separator.
        /// </summary>
        /// <param name="sbdInput">Base StringBuilder onto which appending will take place.</param>
        /// <param name="strSeparator">The string to use as a separator. <paramref name="strSeparator" /> is included in the returned string only if value has more than one element.</param>
        /// <param name="lstValues">A collection that contains the strings to append.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns><paramref name="sbdInput" /> with values appended.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<StringBuilder> AppendJoinAsync([NotNull] this StringBuilder sbdInput, string strSeparator, IAsyncEnumerable<string> lstValues, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (lstValues == null)
                throw new ArgumentNullException(nameof(lstValues));
            IEnumerator<string> objEnumerator = lstValues.GetEnumerator();
            try
            {
                token.ThrowIfCancellationRequested();
                if (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    string strLoop = objEnumerator.Current;
                    if (string.IsNullOrEmpty(strLoop))
                    {
                        while (objEnumerator.MoveNext())
                        {
                            strLoop = objEnumerator.Current;
                            if (!string.IsNullOrEmpty(strLoop))
                            {
                                sbdInput.Append(strLoop);
                                break;
                            }
                        }
                    }
                    else
                        sbdInput.Append(strLoop);
                    token.ThrowIfCancellationRequested();
                    while (objEnumerator.MoveNext())
                    {
                        token.ThrowIfCancellationRequested();
                        strLoop = objEnumerator.Current;
                        if (!string.IsNullOrEmpty(strLoop))
                            sbdInput.Append(strSeparator).Append(strLoop);
                    }
                }
                return sbdInput;
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objDisposeAsync)
                    await objDisposeAsync.DisposeAsync().ConfigureAwait(false);
                else if (objEnumerator is IDisposable objDispose)
                    objDispose.Dispose();
            }
        }

        /// <summary>
        /// Async combination of <see cref="StringBuilder.Append(object)"/> and <see cref="string.Join(string, object[])"/>, appending a list of strings with a separator.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sbdInput">Base StringBuilder onto which appending will take place.</param>
        /// <param name="chrSeparator">The char to use as a separator. <paramref name="chrSeparator" /> is included in the returned string only if value has more than one element.</param>
        /// <param name="lstValues">A collection that contains the objects to append.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns><paramref name="sbdInput" /> with values appended.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<StringBuilder> AppendJoinAsync<T>([NotNull] this StringBuilder sbdInput, char chrSeparator, IAsyncEnumerable<T> lstValues, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (lstValues == null)
                throw new ArgumentNullException(nameof(lstValues));
            IEnumerator<T> objEnumerator = lstValues.GetEnumerator();
            try
            {
                token.ThrowIfCancellationRequested();
                if (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    string strLoop = objEnumerator.Current?.ToString();
                    if (string.IsNullOrEmpty(strLoop))
                    {
                        while (objEnumerator.MoveNext())
                        {
                            strLoop = objEnumerator.Current?.ToString();
                            if (!string.IsNullOrEmpty(strLoop))
                            {
                                sbdInput.Append(strLoop);
                                break;
                            }
                        }
                    }
                    else
                        sbdInput.Append(strLoop);
                    token.ThrowIfCancellationRequested();
                    while (objEnumerator.MoveNext())
                    {
                        token.ThrowIfCancellationRequested();
                        strLoop = objEnumerator.Current?.ToString();
                        if (!string.IsNullOrEmpty(strLoop))
                            sbdInput.Append(chrSeparator).Append(strLoop);
                    }
                }
                return sbdInput;
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objDisposeAsync)
                    await objDisposeAsync.DisposeAsync().ConfigureAwait(false);
                else if (objEnumerator is IDisposable objDispose)
                    objDispose.Dispose();
            }
        }

        /// <summary>
        /// Async combination of <see cref="StringBuilder.Append(string)"/> and <see cref="string.Join(string, string[])"/>, appending a list of strings with a separator.
        /// </summary>
        /// <param name="sbdInput">Base StringBuilder onto which appending will take place.</param>
        /// <param name="chrSeparator">The char to use as a separator. <paramref name="chrSeparator" /> is included in the returned string only if value has more than one element.</param>
        /// <param name="lstValues">A collection that contains the strings to append.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns><paramref name="sbdInput" /> with values appended.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<StringBuilder> AppendJoinAsync([NotNull] this StringBuilder sbdInput, char chrSeparator, IAsyncEnumerable<string> lstValues, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (lstValues == null)
                throw new ArgumentNullException(nameof(lstValues));
            IEnumerator<string> objEnumerator = lstValues.GetEnumerator();
            try
            {
                token.ThrowIfCancellationRequested();
                if (objEnumerator.MoveNext())
                {
                    token.ThrowIfCancellationRequested();
                    string strLoop = objEnumerator.Current;
                    if (string.IsNullOrEmpty(strLoop))
                    {
                        while (objEnumerator.MoveNext())
                        {
                            strLoop = objEnumerator.Current;
                            if (!string.IsNullOrEmpty(strLoop))
                            {
                                sbdInput.Append(strLoop);
                                break;
                            }
                        }
                    }
                    else
                        sbdInput.Append(strLoop);
                    token.ThrowIfCancellationRequested();
                    while (objEnumerator.MoveNext())
                    {
                        token.ThrowIfCancellationRequested();
                        strLoop = objEnumerator.Current;
                        if (!string.IsNullOrEmpty(strLoop))
                            sbdInput.Append(chrSeparator).Append(strLoop);
                    }
                }
                return sbdInput;
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objDisposeAsync)
                    await objDisposeAsync.DisposeAsync().ConfigureAwait(false);
                else if (objEnumerator is IDisposable objDispose)
                    objDispose.Dispose();
            }
        }

        /// <summary>
        /// Version of <see cref="StringBuilder.ToString()"/> that returns a trimmed version of the string.
        /// Faster than doing <see cref="StringBuilder.ToString()"/> and then <see cref="string.Trim()"/> because it takes advantage of StringBuilder internals that can modify string contents quickly without needing to allocate new strings.
        /// </summary>
        /// <param name="sbdInput">StringBuilder containing the string to be trimmed and returned.</param>
        /// <returns>The trimmed version of the string inside of <paramref name="sbdInput"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToTrimmedString([NotNull] this StringBuilder sbdInput)
        {
            int intLength = sbdInput.Length;
            if (intLength == 0)
                return sbdInput.ToString();

            int intIndex;
            for (intIndex = intLength - 1; intIndex >= 0; --intIndex)
            {
                if (!char.IsWhiteSpace(sbdInput[intIndex]))
                    break;
            }

            ++intIndex;
            if (intIndex < intLength)
                sbdInput.Length = intLength = intIndex;

            if (intLength == 0)
                return sbdInput.ToString();

            for (intIndex = 0; intIndex < intLength; ++intIndex)
            {
                if (!char.IsWhiteSpace(sbdInput[intIndex]))
                    return sbdInput.ToString(intIndex, intLength - intIndex);
            }

            return sbdInput.ToString();
        }

        /// <summary>
        /// Syntactic sugar for <see cref="StringBuilder.Append(string)"/> that over multiple strings.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder Append([NotNull] this StringBuilder sbdInput, params string[] astrValues)
        {
            int intExtraLength = 0;
            foreach (string strLoop in astrValues)
                intExtraLength += strLoop?.Length ?? 0;
            if (intExtraLength > 0)
            {
                sbdInput.EnsureCapacity(sbdInput.Length + intExtraLength);
                foreach (string strLoop in astrValues)
                    sbdInput.Append(strLoop);
            }
            return sbdInput;
        }

        /// <summary>
        /// Syntactic sugar for <see cref="StringBuilder.Append(string)"/> that over multiple strings.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder Append([NotNull] this StringBuilder sbdInput, string str1, string str2)
        {
            int intExtraLength = (str1?.Length ?? 0) + (str2?.Length ?? 0);
            if (intExtraLength > 0)
            {
                sbdInput.EnsureCapacity(sbdInput.Length + intExtraLength);
                sbdInput.Append(str1).Append(str2);
            }
            return sbdInput;
        }

        /// <summary>
        /// Syntactic sugar for <see cref="StringBuilder.Append(string)"/> that over multiple strings and/or chars.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder Append([NotNull] this StringBuilder sbdInput, string str1, char chr2)
        {
            int intExtraLength = (str1?.Length ?? 0) + 1;
            sbdInput.EnsureCapacity(sbdInput.Length + intExtraLength);
            return sbdInput.Append(str1).Append(chr2);
        }

        /// <summary>
        /// Syntactic sugar for <see cref="StringBuilder.Append(string)"/> that over multiple strings and/or chars.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder Append([NotNull] this StringBuilder sbdInput, char chr1, string str2)
        {
            int intExtraLength = 1 + (str2?.Length ?? 0);
            sbdInput.EnsureCapacity(sbdInput.Length + intExtraLength);
            return sbdInput.Append(chr1).Append(str2);
        }

        /// <summary>
        /// Syntactic sugar for <see cref="StringBuilder.Append(char)"/> that over multiple chars.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder Append([NotNull] this StringBuilder sbdInput, char chr1, char chr2)
        {
            sbdInput.EnsureCapacity(sbdInput.Length + 2);
            return sbdInput.Append(chr1).Append(chr2);
        }

        /// <summary>
        /// Syntactic sugar for <see cref="StringBuilder.Append(string)"/> that over multiple strings.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder Append([NotNull] this StringBuilder sbdInput, string str1, string str2, string str3)
        {
            int intExtraLength = (str1?.Length ?? 0) + (str2?.Length ?? 0) + (str3?.Length ?? 0);
            if (intExtraLength > 0)
            {
                sbdInput.EnsureCapacity(sbdInput.Length + intExtraLength);
                sbdInput.Append(str1).Append(str2).Append(str3);
            }
            return sbdInput;
        }

        /// <summary>
        /// Syntactic sugar for <see cref="StringBuilder.Append(string)"/> that over multiple strings and/or chars.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder Append([NotNull] this StringBuilder sbdInput, string str1, string str2, char chr3)
        {
            int intExtraLength = (str1?.Length ?? 0) + (str2?.Length ?? 0) + 1;
            sbdInput.EnsureCapacity(sbdInput.Length + intExtraLength);
            return sbdInput.Append(str1).Append(str2).Append(chr3);
        }

        /// <summary>
        /// Syntactic sugar for <see cref="StringBuilder.Append(string)"/> that over multiple strings and/or chars.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder Append([NotNull] this StringBuilder sbdInput, string str1, char chr2, string str3)
        {
            int intExtraLength = (str1?.Length ?? 0) + 1 + (str3?.Length ?? 0);
            sbdInput.EnsureCapacity(sbdInput.Length + intExtraLength);
            return sbdInput.Append(str1).Append(chr2).Append(str3);
        }

        /// <summary>
        /// Syntactic sugar for <see cref="StringBuilder.Append(string)"/> that over multiple strings and/or chars.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder Append([NotNull] this StringBuilder sbdInput, string str1, char chr2, char chr3)
        {
            int intExtraLength = (str1?.Length ?? 0) + 2;
            sbdInput.EnsureCapacity(sbdInput.Length + intExtraLength);
            return sbdInput.Append(str1).Append(chr2).Append(chr3);
        }

        /// <summary>
        /// Syntactic sugar for <see cref="StringBuilder.Append(string)"/> that over multiple strings and/or chars.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder Append([NotNull] this StringBuilder sbdInput, char chr1, string str2, string str3)
        {
            int intExtraLength = 1 + (str2?.Length ?? 0) + (str3?.Length ?? 0);
            sbdInput.EnsureCapacity(sbdInput.Length + intExtraLength);
            return sbdInput.Append(chr1).Append(str2).Append(str3);
        }

        /// <summary>
        /// Syntactic sugar for <see cref="StringBuilder.Append(string)"/> that over multiple strings and/or chars.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder Append([NotNull] this StringBuilder sbdInput, char chr1, string str2, char chr3)
        {
            int intExtraLength = 2 + (str2?.Length ?? 0);
            sbdInput.EnsureCapacity(sbdInput.Length + intExtraLength);
            return sbdInput.Append(chr1).Append(str2).Append(chr3);
        }

        /// <summary>
        /// Syntactic sugar for <see cref="StringBuilder.Append(string)"/> that over multiple strings and/or chars.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder Append([NotNull] this StringBuilder sbdInput, char chr1, char chr2, string str3)
        {
            int intExtraLength = 2 + (str3?.Length ?? 0);
            sbdInput.EnsureCapacity(sbdInput.Length + intExtraLength);
            return sbdInput.Append(chr1).Append(chr2).Append(str3);
        }

        /// <summary>
        /// Syntactic sugar for <see cref="StringBuilder.Append(char)"/> that over multiple chars.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder Append([NotNull] this StringBuilder sbdInput, char chr1, char chr2, char chr3)
        {
            sbdInput.EnsureCapacity(sbdInput.Length + 3);
            return sbdInput.Append(chr1).Append(chr2).Append(chr3);
        }

        /// <summary>
        /// Syntactic sugar for <see cref="StringBuilder.Append(string)"/> that over multiple strings.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder Append([NotNull] this StringBuilder sbdInput, string str1, string str2, string str3, string str4)
        {
            int intExtraLength = (str1?.Length ?? 0) + (str2?.Length ?? 0) + (str3?.Length ?? 0) + (str4?.Length ?? 0);
            if (intExtraLength > 0)
            {
                sbdInput.EnsureCapacity(sbdInput.Length + intExtraLength);
                sbdInput.Append(str1).Append(str2).Append(str3).Append(str4);
            }
            return sbdInput;
        }

        /// <summary>
        /// Syntactic sugar for <see cref="StringBuilder.Append(string)"/> that over multiple strings.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder Append([NotNull] this StringBuilder sbdInput, string str1, string str2, string str3, string str4, string str5)
        {
            int intExtraLength = (str1?.Length ?? 0) + (str2?.Length ?? 0) + (str3?.Length ?? 0) + (str4?.Length ?? 0) + (str5?.Length ?? 0);
            if (intExtraLength > 0)
            {
                sbdInput.EnsureCapacity(sbdInput.Length + intExtraLength);
                sbdInput.Append(str1).Append(str2).Append(str3).Append(str4).Append(str5);
            }
            return sbdInput;
        }

        /// <summary>
        /// Syntactic sugar for <see cref="StringBuilder.Append(string)"/> that over multiple strings.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder Append([NotNull] this StringBuilder sbdInput, string str1, string str2, string str3, string str4, string str5, string str6)
        {
            int intExtraLength = (str1?.Length ?? 0) + (str2?.Length ?? 0) + (str3?.Length ?? 0) + (str4?.Length ?? 0) + (str5?.Length ?? 0) + (str6?.Length ?? 0);
            if (intExtraLength > 0)
            {
                sbdInput.EnsureCapacity(sbdInput.Length + intExtraLength);
                sbdInput.Append(str1).Append(str2).Append(str3).Append(str4).Append(str5).Append(str6);
            }
            return sbdInput;
        }

        /// <summary>
        /// Syntactic sugar for <see cref="StringBuilder.Append(string)"/> that over multiple strings.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder Append([NotNull] this StringBuilder sbdInput, string str1, string str2, string str3, string str4, string str5, string str6, string str7)
        {
            int intExtraLength = (str1?.Length ?? 0) + (str2?.Length ?? 0) + (str3?.Length ?? 0) + (str4?.Length ?? 0) + (str5?.Length ?? 0) + (str6?.Length ?? 0) + (str7?.Length ?? 0);
            if (intExtraLength > 0)
            {
                sbdInput.EnsureCapacity(sbdInput.Length + intExtraLength);
                sbdInput.Append(str1).Append(str2).Append(str3).Append(str4).Append(str5).Append(str6).Append(str7);
            }
            return sbdInput;
        }

        /// <summary>
        /// Syntactic sugar for <see cref="StringBuilder.Append(char)"/> that over multiple chars.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder Append([NotNull] this StringBuilder sbdInput, params char[] achrValues)
        {
            int intExtraLength = achrValues.Length;
            if (intExtraLength > 0)
            {
                sbdInput.EnsureCapacity(sbdInput.Length + intExtraLength);
                foreach (char chrLoop in achrValues)
                    sbdInput.Append(chrLoop);
            }
            return sbdInput;
        }

        /// <summary>
        /// Syntactic sugar for <see cref="StringBuilder.AppendLine(string)"/> after calling <see cref="StringBuilder.Append(string)"/> over multiple strings.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder AppendLine([NotNull] this StringBuilder sbdInput, params string[] astrValues)
        {
            int intExtraLength = Environment.NewLine.Length;
            foreach (string strLoop in astrValues)
                intExtraLength += strLoop?.Length ?? 0;
            sbdInput.EnsureCapacity(sbdInput.Length + intExtraLength);
            foreach (string strLoop in astrValues)
                sbdInput.Append(strLoop);
            return sbdInput.AppendLine();
        }

        /// <summary>
        /// Syntactic sugar for <see cref="StringBuilder.AppendLine(string)"/> after calling <see cref="StringBuilder.Append(string)"/> over multiple strings.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder AppendLine([NotNull] this StringBuilder sbdInput, string str1, string str2)
        {
            int intExtraLength = (str1?.Length ?? 0) + (str2?.Length ?? 0) + Environment.NewLine.Length;
            sbdInput.EnsureCapacity(sbdInput.Length + intExtraLength);
            return sbdInput.Append(str1).AppendLine(str2);
        }

        /// <summary>
        /// Syntactic sugar for <see cref="StringBuilder.AppendLine(string)"/> after calling <see cref="StringBuilder.Append(string)"/> over multiple strings.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder AppendLine([NotNull] this StringBuilder sbdInput, string str1, string str2, string str3)
        {
            int intExtraLength = (str1?.Length ?? 0) + (str2?.Length ?? 0) + (str3?.Length ?? 0) + Environment.NewLine.Length;
            sbdInput.EnsureCapacity(sbdInput.Length + intExtraLength);
            return sbdInput.Append(str1).Append(str2).AppendLine(str3);
        }

        /// <summary>
        /// Syntactic sugar for <see cref="StringBuilder.AppendLine(string)"/> after calling <see cref="StringBuilder.Append(string)"/> over multiple strings.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder AppendLine([NotNull] this StringBuilder sbdInput, string str1, string str2, string str3, string str4)
        {
            int intExtraLength = (str1?.Length ?? 0) + (str2?.Length ?? 0) + (str3?.Length ?? 0) + (str4?.Length ?? 0) + Environment.NewLine.Length;
            sbdInput.EnsureCapacity(sbdInput.Length + intExtraLength);
            return sbdInput.Append(str1).Append(str2).Append(str3).AppendLine(str4);
        }

        /// <summary>
        /// Syntactic sugar for <see cref="StringBuilder.AppendLine(string)"/> after calling <see cref="StringBuilder.Append(string)"/> over multiple strings.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder AppendLine([NotNull] this StringBuilder sbdInput, string str1, string str2, string str3, string str4, string str5)
        {
            int intExtraLength = (str1?.Length ?? 0) + (str2?.Length ?? 0) + (str3?.Length ?? 0) + (str4?.Length ?? 0) + (str5?.Length ?? 0) + Environment.NewLine.Length;
            sbdInput.EnsureCapacity(sbdInput.Length + intExtraLength);
            return sbdInput.Append(str1).Append(str2).Append(str3).Append(str4).AppendLine(str5);
        }

        /// <summary>
        /// Syntactic sugar for <see cref="StringBuilder.AppendLine(string)"/> after calling <see cref="StringBuilder.Append(string)"/> over multiple strings.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder AppendLine([NotNull] this StringBuilder sbdInput, string str1, string str2, string str3, string str4, string str5, string str6)
        {
            int intExtraLength = (str1?.Length ?? 0) + (str2?.Length ?? 0) + (str3?.Length ?? 0) + (str4?.Length ?? 0) + (str5?.Length ?? 0) + (str6?.Length ?? 0) + Environment.NewLine.Length;
            sbdInput.EnsureCapacity(sbdInput.Length + intExtraLength);
            return sbdInput.Append(str1).Append(str2).Append(str3).Append(str4).Append(str5).AppendLine(str6);
        }

        /// <summary>
        /// Syntactic sugar for <see cref="StringBuilder.AppendLine(string)"/> after calling <see cref="StringBuilder.Append(string)"/> over multiple strings.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder AppendLine([NotNull] this StringBuilder sbdInput, string str1, string str2, string str3, string str4, string str5, string str6, string str7)
        {
            int intExtraLength = (str1?.Length ?? 0) + (str2?.Length ?? 0) + (str3?.Length ?? 0) + (str4?.Length ?? 0) + (str5?.Length ?? 0) + (str6?.Length ?? 0) + (str7?.Length ?? 0) + Environment.NewLine.Length;
            sbdInput.EnsureCapacity(sbdInput.Length + intExtraLength);
            return sbdInput.Append(str1).Append(str2).Append(str3).Append(str4).Append(str5).Append(str6).AppendLine(str7);
        }

        /// <summary>
        /// Syntactic sugar <see cref="StringBuilder.AppendLine()"/> after calling <see cref="StringBuilder.Append(char)"/>.
        /// </summary>
        /// <param name="sbdInput">Base StringBuilder in which appending is to take place.</param>
        /// <param name="chrValue">New character to append before the new line is appended.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder AppendLine([NotNull] this StringBuilder sbdInput, char chrValue)
        {
            return sbdInput.Append(chrValue).AppendLine();
        }

        /// <summary>
        /// Syntactic sugar for <see cref="StringBuilder.AppendLine()"/> after calling <see cref="StringBuilder.Append(char)"/> over multiple chars.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder AppendLine([NotNull] this StringBuilder sbdInput, params char[] achrValues)
        {
            int intExtraLength = achrValues.Length + Environment.NewLine.Length;
            sbdInput.EnsureCapacity(sbdInput.Length + intExtraLength);
            foreach (char chrLoop in achrValues)
                sbdInput.Append(chrLoop);
            return sbdInput.AppendLine();
        }

        /// <summary>
        /// Syntactic sugar for <see cref="StringBuilder.Insert(int, string)"/> over multiple strings.
        /// Elements are inserted such that the resulting string builder will have them in the same order as they are in the arguments.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder Insert([NotNull] this StringBuilder sbdInput, int index, params string[] astrValues)
        {
            int intExtraLength = 0;
            foreach (string strLoop in astrValues)
                intExtraLength += strLoop?.Length ?? 0;
            if (intExtraLength > 0)
            {
                sbdInput.EnsureCapacity(sbdInput.Length + intExtraLength);
                for (int i = astrValues.Length - 1; i > 0; --i)
                    sbdInput.Insert(index, astrValues[i]);
            }
            return sbdInput;
        }

        /// <summary>
        /// Syntactic sugar for <see cref="StringBuilder.Insert(int, string)"/> over multiple strings and/or chars.
        /// Elements are inserted such that the resulting string builder will have them in the same order as they are in the arguments.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder Insert([NotNull] this StringBuilder sbdInput, int index, string str1, string str2)
        {
            int intExtraLength = (str1?.Length ?? 0) + (str2?.Length ?? 0);
            if (intExtraLength > 0)
            {
                sbdInput.EnsureCapacity(sbdInput.Length + intExtraLength);
                sbdInput.Insert(index, str2).Insert(index, str1);
            }
            return sbdInput;
        }

        /// <summary>
        /// Syntactic sugar for <see cref="StringBuilder.Insert(int, string)"/> over multiple strings and/or chars.
        /// Elements are inserted such that the resulting string builder will have them in the same order as they are in the arguments.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder Insert([NotNull] this StringBuilder sbdInput, int index, string str1, char chr2)
        {
            int intExtraLength = (str1?.Length ?? 0) + 1;
            sbdInput.EnsureCapacity(sbdInput.Length + intExtraLength);
            return sbdInput.Insert(index, chr2).Insert(index, str1);
        }

        /// <summary>
        /// Syntactic sugar for <see cref="StringBuilder.Insert(int, string)"/> over multiple strings and/or chars.
        /// Elements are inserted such that the resulting string builder will have them in the same order as they are in the arguments.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder Insert([NotNull] this StringBuilder sbdInput, int index, char chr1, string str2)
        {
            int intExtraLength = 1 + (str2?.Length ?? 0);
            sbdInput.EnsureCapacity(sbdInput.Length + intExtraLength);
            return sbdInput.Insert(index, str2).Insert(index, chr1);
        }

        /// <summary>
        /// Syntactic sugar for <see cref="StringBuilder.Insert(int, char)"/> over multiple chars.
        /// Elements are inserted such that the resulting string builder will have them in the same order as they are in the arguments.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder Insert([NotNull] this StringBuilder sbdInput, int index, char chr1, char chr2)
        {
            sbdInput.EnsureCapacity(sbdInput.Length + 2);
            return sbdInput.Insert(index, chr2).Insert(index, chr1);
        }

        /// <summary>
        /// Syntactic sugar for <see cref="StringBuilder.Insert(int, string)"/> over multiple strings.
        /// Elements are inserted such that the resulting string builder will have them in the same order as they are in the arguments.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder Insert([NotNull] this StringBuilder sbdInput, int index, string str1, string str2, string str3)
        {
            int intExtraLength = (str1?.Length ?? 0) + (str2?.Length ?? 0) + (str3?.Length ?? 0);
            if (intExtraLength > 0)
            {
                sbdInput.EnsureCapacity(sbdInput.Length + intExtraLength);
                sbdInput.Insert(index, str3).Insert(index, str2).Insert(index, str1);
            }
            return sbdInput;
        }

        /// <summary>
        /// Syntactic sugar for <see cref="StringBuilder.Insert(int, string)"/> over multiple strings and/or chars.
        /// Elements are inserted such that the resulting string builder will have them in the same order as they are in the arguments.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder Insert([NotNull] this StringBuilder sbdInput, int index, string str1, string str2, char chr3)
        {
            int intExtraLength = (str1?.Length ?? 0) + (str2?.Length ?? 0) + 1;
            sbdInput.EnsureCapacity(sbdInput.Length + intExtraLength);
            return sbdInput.Insert(index, chr3).Insert(index, str2).Insert(index, str1);
        }

        /// <summary>
        /// Syntactic sugar for <see cref="StringBuilder.Insert(int, string)"/> over multiple strings and/or chars.
        /// Elements are inserted such that the resulting string builder will have them in the same order as they are in the arguments.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder Insert([NotNull] this StringBuilder sbdInput, int index, string str1, char chr2, string str3)
        {
            int intExtraLength = (str1?.Length ?? 0) + 1 + (str3?.Length ?? 0);
            sbdInput.EnsureCapacity(sbdInput.Length + intExtraLength);
            return sbdInput.Insert(index, str3).Insert(index, chr2).Insert(index, str1);
        }

        /// <summary>
        /// Syntactic sugar for <see cref="StringBuilder.Insert(int, string)"/> over multiple strings and/or chars.
        /// Elements are inserted such that the resulting string builder will have them in the same order as they are in the arguments.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder Insert([NotNull] this StringBuilder sbdInput, int index, string str1, char chr2, char chr3)
        {
            int intExtraLength = (str1?.Length ?? 0) + 2;
            sbdInput.EnsureCapacity(sbdInput.Length + intExtraLength);
            return sbdInput.Insert(index, chr3).Insert(index, chr2).Insert(index, str1);
        }

        /// <summary>
        /// Syntactic sugar for <see cref="StringBuilder.Insert(int, string)"/> over multiple strings and/or chars.
        /// Elements are inserted such that the resulting string builder will have them in the same order as they are in the arguments.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder Insert([NotNull] this StringBuilder sbdInput, int index, char chr1, string str2, string str3)
        {
            int intExtraLength = 1 + (str2?.Length ?? 0) + (str3?.Length ?? 0);
            sbdInput.EnsureCapacity(sbdInput.Length + intExtraLength);
            return sbdInput.Insert(index, str3).Insert(index, str2).Insert(index, chr1);
        }

        /// <summary>
        /// Syntactic sugar for <see cref="StringBuilder.Insert(int, string)"/> over multiple strings and/or chars.
        /// Elements are inserted such that the resulting string builder will have them in the same order as they are in the arguments.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder Insert([NotNull] this StringBuilder sbdInput, int index, char chr1, string str2, char chr3)
        {
            int intExtraLength = 2 + (str2?.Length ?? 0);
            sbdInput.EnsureCapacity(sbdInput.Length + intExtraLength);
            return sbdInput.Insert(index, chr3).Insert(index, str2).Insert(index, chr1);
        }

        /// <summary>
        /// Syntactic sugar for <see cref="StringBuilder.Insert(int, string)"/> over multiple strings and/or chars.
        /// Elements are inserted such that the resulting string builder will have them in the same order as they are in the arguments.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder Insert([NotNull] this StringBuilder sbdInput, int index, char chr1, char chr2, string str3)
        {
            int intExtraLength = 2 + (str3?.Length ?? 0);
            sbdInput.EnsureCapacity(sbdInput.Length + intExtraLength);
            return sbdInput.Insert(index, str3).Insert(index, chr2).Insert(index, chr1);
        }

        /// <summary>
        /// Syntactic sugar for <see cref="StringBuilder.Insert(int, char)"/> over multiple chars.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder Insert([NotNull] this StringBuilder sbdInput, int index, char chr1, char chr2, char chr3)
        {
            sbdInput.EnsureCapacity(sbdInput.Length + 3);
            return sbdInput.Insert(index, chr3).Insert(index, chr2).Insert(index, chr1);
        }

        /// <summary>
        /// Syntactic sugar for <see cref="StringBuilder.Insert(int, string)"/> over multiple strings.
        /// Elements are inserted such that the resulting string builder will have them in the same order as they are in the arguments.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder Insert([NotNull] this StringBuilder sbdInput, int index, string str1, string str2, string str3, string str4)
        {
            int intExtraLength = (str1?.Length ?? 0) + (str2?.Length ?? 0) + (str3?.Length ?? 0) + (str4?.Length ?? 0);
            if (intExtraLength > 0)
            {
                sbdInput.EnsureCapacity(sbdInput.Length + intExtraLength);
                sbdInput.Insert(index, str4).Insert(index, str3).Insert(index, str2).Insert(index, str1);
            }
            return sbdInput;
        }

        /// <summary>
        /// Syntactic sugar for <see cref="StringBuilder.Insert(int, string)"/> over multiple strings.
        /// Elements are inserted such that the resulting string builder will have them in the same order as they are in the arguments.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder Insert([NotNull] this StringBuilder sbdInput, int index, string str1, string str2, string str3, string str4, string str5)
        {
            int intExtraLength = (str1?.Length ?? 0) + (str2?.Length ?? 0) + (str3?.Length ?? 0) + (str4?.Length ?? 0) + (str5?.Length ?? 0);
            if (intExtraLength > 0)
            {
                sbdInput.EnsureCapacity(sbdInput.Length + intExtraLength);
                sbdInput.Insert(index, str5).Insert(index, str4).Insert(index, str3).Insert(index, str2).Insert(index, str1);
            }
            return sbdInput;
        }

        /// <summary>
        /// Syntactic sugar for <see cref="StringBuilder.Insert(int, string)"/> over multiple strings.
        /// Elements are inserted such that the resulting string builder will have them in the same order as they are in the arguments.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder Insert([NotNull] this StringBuilder sbdInput, int index, string str1, string str2, string str3, string str4, string str5, string str6)
        {
            int intExtraLength = (str1?.Length ?? 0) + (str2?.Length ?? 0) + (str3?.Length ?? 0) + (str4?.Length ?? 0) + (str5?.Length ?? 0) + (str6?.Length ?? 0);
            if (intExtraLength > 0)
            {
                sbdInput.EnsureCapacity(sbdInput.Length + intExtraLength);
                sbdInput.Insert(index, str6).Insert(index, str5).Insert(index, str4).Insert(index, str3).Insert(index, str2).Insert(index, str1);
            }
            return sbdInput;
        }

        /// <summary>
        /// Syntactic sugar for <see cref="StringBuilder.Insert(int, string)"/> over multiple strings.
        /// Elements are inserted such that the resulting string builder will have them in the same order as they are in the arguments.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder Insert([NotNull] this StringBuilder sbdInput, int index, string str1, string str2, string str3, string str4, string str5, string str6, string str7)
        {
            int intExtraLength = (str1?.Length ?? 0) + (str2?.Length ?? 0) + (str3?.Length ?? 0) + (str4?.Length ?? 0) + (str5?.Length ?? 0) + (str6?.Length ?? 0) + (str7?.Length ?? 0);
            if (intExtraLength > 0)
            {
                sbdInput.EnsureCapacity(sbdInput.Length + intExtraLength);
                sbdInput.Insert(index, str7).Insert(index, str6).Insert(index, str5).Insert(index, str4).Insert(index, str3).Insert(index, str2).Insert(index, str1);
            }
            return sbdInput;
        }
    }
}
