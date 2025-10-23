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

using Chummer.Backend.Equipment;
using Chummer.Backend.Skills;
using Chummer.Backend.Uniques;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace Chummer
{
    public static class ImprovementManager
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;

        // String that will be used to limit the selection in Pick forms.
        private static readonly ConcurrentDictionary<Character, string> s_dicLimitSelections = new ConcurrentDictionary<Character, string>();
        private static readonly ConcurrentDictionary<Character, string> s_dicSelectedValues = new ConcurrentDictionary<Character, string>();
        private static readonly ConcurrentDictionary<Character, string> s_dicForcedValues = new ConcurrentDictionary<Character, string>();
        private static string s_strInvariantLimitSelection;
        private static string s_strInvariantSelectedValue;
        private static string s_strInvariantForcedValue;

        private static readonly ConcurrentDictionary<Character, List<Improvement>> s_DictionaryTransactions
            = new ConcurrentDictionary<Character, List<Improvement>>();

        private static readonly ConcurrentHashSet<ValueTuple<ImprovementDictionaryKey, ConcurrentDictionary<ImprovementDictionaryKey, ValueTuple<decimal, List<Improvement>>>>>
            s_SetCurrentlyCalculatingValues = new ConcurrentHashSet<ValueTuple<ImprovementDictionaryKey, ConcurrentDictionary<ImprovementDictionaryKey, ValueTuple<decimal, List<Improvement>>>>>();

        private static readonly ConcurrentDictionary<ImprovementDictionaryKey, ValueTuple<decimal, List<Improvement>>>
            s_DictionaryCachedValues
                = new ConcurrentDictionary<ImprovementDictionaryKey, ValueTuple<decimal, List<Improvement>>>();

        private static readonly ConcurrentDictionary<ImprovementDictionaryKey, ValueTuple<decimal, List<Improvement>>>
            s_DictionaryCachedAugmentedValues
                = new ConcurrentDictionary<ImprovementDictionaryKey, ValueTuple<decimal, List<Improvement>>>();

        public readonly struct ImprovementDictionaryKey : IEquatable<ImprovementDictionaryKey>,
            IEquatable<ValueTuple<Character, Improvement.ImprovementType, string>>
        {
            private readonly ValueTuple<Character, Improvement.ImprovementType, string> _objTupleKey;

            public Character CharacterObject => _objTupleKey.Item1;
            public Improvement.ImprovementType ImprovementType => _objTupleKey.Item2;
            public string ImprovementName => _objTupleKey.Item3;

            public ImprovementDictionaryKey(Character objCharacter, Improvement.ImprovementType eImprovementType,
                                            string strImprovementName)
            {
                _objTupleKey
                    = new ValueTuple<Character, Improvement.ImprovementType, string>(
                        objCharacter, eImprovementType, strImprovementName);
            }

            public override int GetHashCode()
            {
                return (CharacterObject, ImprovementType, ImprovementName).GetHashCode();
            }

            public override bool Equals(object obj)
            {
                switch (obj)
                {
                    case ImprovementDictionaryKey objOtherImprovementDictionaryKey:
                        return Equals(objOtherImprovementDictionaryKey);

                    case Tuple<Character, Improvement.ImprovementType, string> objOtherTuple:
                        return Equals(objOtherTuple);

                    case ValueTuple<Character, Improvement.ImprovementType, string> objOtherValueTuple:
                        return Equals(objOtherValueTuple);

                    default:
                        return false;
                }
            }

            public bool Equals(ImprovementDictionaryKey other)
            {
                return CharacterObject == other.CharacterObject &&
                       ImprovementType == other.ImprovementType &&
                       ImprovementName == other.ImprovementName;
            }

            public bool Equals(Tuple<Character, Improvement.ImprovementType, string> other)
            {
                if (other == null)
                    return false;
                return CharacterObject == other.Item1 &&
                       ImprovementType == other.Item2 &&
                       ImprovementName == other.Item3;
            }

            public bool Equals(ValueTuple<Character, Improvement.ImprovementType, string> other)
            {
                return CharacterObject == other.Item1 &&
                       ImprovementType == other.Item2 &&
                       ImprovementName == other.Item3;
            }

            public override string ToString()
            {
                return _objTupleKey.ToString();
            }

            public static bool operator ==(ImprovementDictionaryKey x, ImprovementDictionaryKey y)
            {
                return x.Equals(y);
            }

            public static bool operator !=(ImprovementDictionaryKey x, ImprovementDictionaryKey y)
            {
                return !x.Equals(y);
            }

            public static bool operator ==(ImprovementDictionaryKey x, object y)
            {
                return x.Equals(y);
            }

            public static bool operator !=(ImprovementDictionaryKey x, object y)
            {
                return !x.Equals(y);
            }

            public static bool operator ==(object x, ImprovementDictionaryKey y)
            {
                return x?.Equals(y) ?? false;
            }

            public static bool operator !=(object x, ImprovementDictionaryKey y)
            {
                return !(x?.Equals(y) ?? false);
            }
        }

        #region Properties

        /// <summary>
        /// Limit what can be selected in Pick forms to a single value. This is typically used when selecting the Qualities for a Metavariant that has a specific
        /// CharacterAttribute selection for Qualities like Metagenic Improvement.
        /// </summary>
        public static string GetLimitSelection(Character objCharacter)
        {
            if (objCharacter == null)
                return s_strInvariantLimitSelection;
            s_dicLimitSelections.TryGetValue(objCharacter, out string strReturn);
            return strReturn;
        }
        /// <summary>
        /// Limit what can be selected in Pick forms to a single value. This is typically used when selecting the Qualities for a Metavariant that has a specific
        /// CharacterAttribute selection for Qualities like Metagenic Improvement.
        /// </summary>
        public static void SetLimitSelection(string value, Character objCharacter)
        {
            if (objCharacter == null)
                s_strInvariantLimitSelection = value;
            else if (!objCharacter.IsDisposed)
                s_dicLimitSelections.AddOrUpdate(objCharacter, value, (c, s) => value);
        }

        /// <summary>
        /// Clear the limit selection value
        /// </summary>
        public static void ClearLimitSelection(Character objCharacter)
        {
            if (objCharacter == null)
                s_strInvariantLimitSelection = string.Empty;
            else
                s_dicLimitSelections.TryRemove(objCharacter, out string _);
        }

        /// <summary>
        /// The string that was entered or selected from any of the dialogue windows that were presented because of this Improvement.
        /// </summary>
        public static string GetSelectedValue(Character objCharacter)
        {
            if (objCharacter == null)
                return s_strInvariantSelectedValue;
            s_dicSelectedValues.TryGetValue(objCharacter, out string strReturn);
            return strReturn;
        }

        /// <summary>
        /// The string that was entered or selected from any of the dialogue windows that were presented because of this Improvement.
        /// </summary>
        public static void SetSelectedValue(string value, Character objCharacter)
        {
            if (objCharacter == null)
                s_strInvariantSelectedValue = value;
            else if (!objCharacter.IsDisposed)
                s_dicSelectedValues.AddOrUpdate(objCharacter, value, (c, s) => value);
        }

        /// <summary>
        /// Clear the selected value (value that was entered or selected from any dialogue windows that were presented)
        /// </summary>
        public static void ClearSelectedValue(Character objCharacter)
        {
            if (objCharacter == null)
                s_strInvariantSelectedValue = string.Empty;
            else
                s_dicSelectedValues.TryRemove(objCharacter, out string _);
        }

        /// <summary>
        /// Force any dialogue windows that open to use this string as their selected value.
        /// </summary>
        public static string GetForcedValue(Character objCharacter)
        {
            if (objCharacter == null)
                return s_strInvariantForcedValue;
            s_dicForcedValues.TryGetValue(objCharacter, out string strReturn);
            return strReturn;
        }

        /// <summary>
        /// Force any dialogue windows that open to use this string as their selected value.
        /// </summary>
        public static void SetForcedValue(string value, Character objCharacter)
        {
            if (objCharacter == null)
                s_strInvariantForcedValue = value;
            else if (!objCharacter.IsDisposed)
                s_dicForcedValues.AddOrUpdate(objCharacter, value, (c, s) => value);
        }

        /// <summary>
        /// Clear the forced value (value that is forced by any dialogue windows that open to use a given string as their selected value)
        /// </summary>
        public static void ClearForcedValue(Character objCharacter)
        {
            if (objCharacter == null)
                s_strInvariantForcedValue = string.Empty;
            else
                s_dicForcedValues.TryRemove(objCharacter, out string _);
        }

        public static void ClearCachedValue(Character objCharacter, Improvement.ImprovementType eImprovementType,
                                            string strImprovementName = "", CancellationToken token = default)
        {
            if (!string.IsNullOrEmpty(strImprovementName))
            {
                ImprovementDictionaryKey objCheckKey
                    = new ImprovementDictionaryKey(objCharacter, eImprovementType, strImprovementName);
                token.ThrowIfCancellationRequested();
                s_DictionaryCachedValues.AddOrUpdate(objCheckKey,
                                                     x => new ValueTuple<decimal, List<Improvement>>(
                                                         decimal.MinValue, new List<Improvement>(8)),
                                                     (x, y) =>
                                                     {
                                                         y.Item2.Clear();
                                                         return new ValueTuple<decimal, List<Improvement>>(
                                                             decimal.MinValue, y.Item2);
                                                     });
                token.ThrowIfCancellationRequested();
                s_DictionaryCachedAugmentedValues.AddOrUpdate(objCheckKey,
                                                              x => new ValueTuple<decimal, List<Improvement>>(
                                                                  decimal.MinValue, new List<Improvement>(8)),
                                                              (x, y) =>
                                                              {
                                                                  y.Item2.Clear();
                                                                  return new ValueTuple<decimal, List<Improvement>>(
                                                                      decimal.MinValue, y.Item2);
                                                              });
            }
            else
            {
                List<ImprovementDictionaryKey> lstTempOuter = new List<ImprovementDictionaryKey>(Math.Max(s_DictionaryCachedValues.Count, s_DictionaryCachedAugmentedValues.Count));
                foreach (KeyValuePair<ImprovementDictionaryKey, ValueTuple<decimal, List<Improvement>>> kvpLoop in s_DictionaryCachedValues)
                {
                    token.ThrowIfCancellationRequested();
                    ImprovementDictionaryKey objCachedValueKey = kvpLoop.Key; // Set up this way to make sure main dictionary stays locked during enumeration
                    if (objCachedValueKey.CharacterObject == objCharacter && objCachedValueKey.ImprovementType == eImprovementType)
                        lstTempOuter.Add(objCachedValueKey);
                }
                foreach (ImprovementDictionaryKey objCheckKey in lstTempOuter)
                {
                    token.ThrowIfCancellationRequested();
                    if (s_DictionaryCachedValues.TryGetValue(objCheckKey,
                                                             out ValueTuple<decimal, List<Improvement>> tupTemp))
                    {
                        List<Improvement> lstTemp = tupTemp.Item2;
                        lstTemp.Clear();
                        s_DictionaryCachedValues
                            .AddOrUpdate(objCheckKey,
                                         x => new ValueTuple<decimal, List<Improvement>>(decimal.MinValue, lstTemp),
                                         (x, y) => new ValueTuple<decimal, List<Improvement>>(
                                             decimal.MinValue, lstTemp));
                    }
                }

                lstTempOuter.Clear();
                foreach (KeyValuePair<ImprovementDictionaryKey, ValueTuple<decimal, List<Improvement>>> kvpLoop in s_DictionaryCachedAugmentedValues)
                {
                    token.ThrowIfCancellationRequested();
                    ImprovementDictionaryKey objCachedValueKey = kvpLoop.Key; // Set up this way to make sure main dictionary stays locked during enumeration
                    if (objCachedValueKey.CharacterObject == objCharacter && objCachedValueKey.ImprovementType == eImprovementType)
                        lstTempOuter.Add(objCachedValueKey);
                }
                foreach (ImprovementDictionaryKey objCheckKey in lstTempOuter)
                {
                    token.ThrowIfCancellationRequested();
                    if (s_DictionaryCachedAugmentedValues.TryGetValue(objCheckKey,
                                                                      out ValueTuple<decimal, List<Improvement>> tupTemp))
                    {
                        List<Improvement> lstTemp = tupTemp.Item2;
                        lstTemp.Clear();
                        s_DictionaryCachedAugmentedValues
                            .AddOrUpdate(objCheckKey,
                                         x => new ValueTuple<decimal, List<Improvement>>(decimal.MinValue, lstTemp),
                                         (x, y) => new ValueTuple<decimal, List<Improvement>>(
                                             decimal.MinValue, lstTemp));
                    }
                }
            }
        }

        public static void ClearCachedValues(Character objCharacter, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<ImprovementDictionaryKey> lstToRemove = new List<ImprovementDictionaryKey>(Math.Max(s_DictionaryCachedValues.Count, s_DictionaryCachedAugmentedValues.Count));
            foreach (KeyValuePair<ImprovementDictionaryKey, ValueTuple<decimal, List<Improvement>>> kvpLoop in s_DictionaryCachedValues)
            {
                token.ThrowIfCancellationRequested();
                ImprovementDictionaryKey objKey = kvpLoop.Key; // Set up this way to make sure main dictionary stays locked during enumeration
                if (objKey.CharacterObject == objCharacter)
                    lstToRemove.Add(objKey);
            }
            foreach (ImprovementDictionaryKey objKey in lstToRemove)
            {
                token.ThrowIfCancellationRequested();
                if (s_DictionaryCachedValues.TryRemove(objKey, out ValueTuple<decimal, List<Improvement>> tupTemp))
                    tupTemp.Item2.Clear(); // Just in case this helps the GC
            }

            lstToRemove.Clear();
            foreach (KeyValuePair<ImprovementDictionaryKey, ValueTuple<decimal, List<Improvement>>> kvpLoop in s_DictionaryCachedAugmentedValues)
            {
                token.ThrowIfCancellationRequested();
                ImprovementDictionaryKey objKey = kvpLoop.Key; // Set up this way to make sure main dictionary stays locked during enumeration
                if (objKey.CharacterObject == objCharacter)
                    lstToRemove.Add(objKey);
            }
            foreach (ImprovementDictionaryKey objKey in lstToRemove)
            {
                token.ThrowIfCancellationRequested();
                if (s_DictionaryCachedAugmentedValues.TryRemove(objKey, out ValueTuple<decimal, List<Improvement>> tupTemp))
                    tupTemp.Item2.Clear(); // Just in case this helps the GC
            }

            s_DictionaryTransactions.TryRemove(objCharacter, out List<Improvement> _);
        }

        /// <summary>
        /// Clear all values tied to a specific character. Should be called when a character is reset or disposed.
        /// </summary>
        public static void ClearAllCharacterValues(Character objCharacter, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            ClearLimitSelection(objCharacter);
            token.ThrowIfCancellationRequested();
            ClearForcedValue(objCharacter);
            token.ThrowIfCancellationRequested();
            ClearSelectedValue(objCharacter);
            ClearCachedValues(objCharacter, token);
        }

        #endregion Properties

        #region Helper Methods

        /// <summary>
        /// Retrieve the total Improvement value for the specified ImprovementType.
        /// </summary>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="objImprovementType">ImprovementType to retrieve the value of.</param>
        /// <param name="blnAddToRating">Whether we should only retrieve values that have AddToRating enabled.</param>
        /// <param name="strImprovedName">Name to assign to the Improvement.</param>
        /// <param name="blnUnconditionalOnly">Whether to only fetch values for improvements that do not have a condition.</param>
        /// <param name="blnIncludeNonImproved">Whether to only fetch values for improvements that do not have an improvedname when specifying ImprovedNames.</param>
        /// <param name="lstUsedImprovements">List of the improvements actually used for the value</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static decimal ValueOf(Character objCharacter, Improvement.ImprovementType objImprovementType,
                                      out List<Improvement> lstUsedImprovements,
                                      bool blnAddToRating = false, string strImprovedName = "",
                                      bool blnUnconditionalOnly = true, bool blnIncludeNonImproved = false, CancellationToken token = default)
        {
            decimal decReturn;
            (decReturn, lstUsedImprovements) = MetaValueOf(objCharacter, objImprovementType, x => x.Value,
                                                           s_DictionaryCachedValues, blnAddToRating, strImprovedName,
                                                           blnUnconditionalOnly, blnIncludeNonImproved, token);
            if (decReturn != 0 && lstUsedImprovements.Count == 0)
            {
                Log.Warn("A cached value modifier somehow is not zero while having no used improvements in its list.");
                Utils.BreakIfDebug();
            }
            return decReturn;
        }

        /// <summary>
        /// Retrieve the total Improvement value for the specified ImprovementType.
        /// </summary>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="objImprovementType">ImprovementType to retrieve the value of.</param>
        /// <param name="blnAddToRating">Whether we should only retrieve values that have AddToRating enabled.</param>
        /// <param name="strImprovedName">Name to assign to the Improvement.</param>
        /// <param name="blnUnconditionalOnly">Whether to only fetch values for improvements that do not have a condition.</param>
        /// <param name="blnIncludeNonImproved">Whether to only fetch values for improvements that do not have an improvedname when specifying <paramref name="strImprovedName"/>.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static decimal ValueOf(Character objCharacter, Improvement.ImprovementType objImprovementType,
                                      bool blnAddToRating = false, string strImprovedName = "",
                                      bool blnUnconditionalOnly = true, bool blnIncludeNonImproved = false, CancellationToken token = default)
        {
            (decimal decReturn, List<Improvement> lstUsedImprovements) = MetaValueOf(
                objCharacter, objImprovementType, x => x.Value, s_DictionaryCachedValues, blnAddToRating,
                strImprovedName, blnUnconditionalOnly, blnIncludeNonImproved, token);
            if (decReturn != 0 && lstUsedImprovements.Count == 0)
            {
                Log.Warn("A cached value modifier somehow is not zero while having no used improvements in its list.");
                Utils.BreakIfDebug();
            }
            return decReturn;
        }

        /// <summary>
        /// Retrieve the total Improvement value for the specified ImprovementType.
        /// </summary>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="objImprovementType">ImprovementType to retrieve the value of.</param>
        /// <param name="blnAddToRating">Whether we should only retrieve values that have AddToRating enabled.</param>
        /// <param name="strImprovedName">Name to assign to the Improvement.</param>
        /// <param name="blnUnconditionalOnly">Whether to only fetch values for improvements that do not have a condition.</param>
        /// <param name="blnIncludeNonImproved">Whether to only fetch values for improvements that do not have an improvedname when specifying <paramref name="strImprovedName"/>.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task<decimal> ValueOfAsync(Character objCharacter, Improvement.ImprovementType objImprovementType,
                                                bool blnAddToRating = false, string strImprovedName = "",
                                                bool blnUnconditionalOnly = true, bool blnIncludeNonImproved = false, CancellationToken token = default)
        {
            (decimal decReturn, List<Improvement> lstUsedImprovements) = await MetaValueOfAsync(
                objCharacter, objImprovementType, x => x.Value, s_DictionaryCachedValues, blnAddToRating,
                strImprovedName, blnUnconditionalOnly, blnIncludeNonImproved, token).ConfigureAwait(false);
            if (decReturn != 0 && lstUsedImprovements.Count == 0)
            {
                Log.Warn("A cached value modifier somehow is not zero while having no used improvements in its list.");
                Utils.BreakIfDebug();
            }
            return decReturn;
        }

        /// <summary>
        /// Retrieve the total Improvement Augmented x Rating for the specified ImprovementType.
        /// </summary>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="objImprovementType">ImprovementType to retrieve the value of.</param>
        /// <param name="blnAddToRating">Whether we should only retrieve values that have AddToRating enabled.</param>
        /// <param name="strImprovedName">Name to assign to the Improvement.</param>
        /// <param name="blnUnconditionalOnly">Whether to only fetch values for improvements that do not have a condition.</param>
        /// <param name="blnIncludeNonImproved">Whether to only fetch values for improvements that do not have an improvedname when specifying ImprovedNames.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task<ValueTuple<decimal, List<Improvement>>> ValueOfTupleAsync(Character objCharacter, Improvement.ImprovementType objImprovementType,
                                                                bool blnAddToRating = false, string strImprovedName = "",
                                                                bool blnUnconditionalOnly = true, bool blnIncludeNonImproved = false, CancellationToken token = default)
        {
            (decimal decReturn, List<Improvement> lstUsedImprovements) = await MetaValueOfAsync(objCharacter, objImprovementType,
                x => x.Value,
                s_DictionaryCachedValues, blnAddToRating, strImprovedName,
                blnUnconditionalOnly,
                blnIncludeNonImproved, token).ConfigureAwait(false);
            if (decReturn != 0 && lstUsedImprovements.Count == 0)
            {
                Log.Warn("A cached value modifier somehow is not zero while having no used improvements in its list.");
                Utils.BreakIfDebug();
            }
            return new ValueTuple<decimal, List<Improvement>>(decReturn, lstUsedImprovements);
        }

        /// <summary>
        /// Gets the cached list of active improvements that contribute to values of a given improvement type.
        /// </summary>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="eImprovementType">ImprovementType for which active improvements should be fetched.</param>
        /// <param name="strImprovedName">Improvements are only fetched with the given improvedname. If empty, only those with an empty ImprovedName are fetched.</param>
        /// <param name="blnIncludeNonImproved">Whether to only fetch values for improvements that do not have an improvedname when specifying <paramref name="strImprovedName"/>.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>A cached list of all unconditional improvements that do not add to ratings and that match the conditions set by the arguments.</returns>
        public static List<Improvement> GetCachedImprovementListForValueOf(
            Character objCharacter, Improvement.ImprovementType eImprovementType, string strImprovedName = "",
            bool blnIncludeNonImproved = false, CancellationToken token = default)
        {
            ValueOf(objCharacter, eImprovementType, out List<Improvement> lstReturn, strImprovedName: strImprovedName,
                    blnIncludeNonImproved: blnIncludeNonImproved, token: token);
            return lstReturn;
        }

        /// <summary>
        /// Gets the cached list of active improvements that contribute to values of a given improvement type.
        /// </summary>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="eImprovementType">ImprovementType for which active improvements should be fetched.</param>
        /// <param name="strImprovedName">Improvements are only fetched with the given improvedname. If empty, only those with an empty ImprovedName are fetched.</param>
        /// <param name="blnIncludeNonImproved">Whether to only fetch values for improvements that do not have an improvedname when specifying <paramref name="strImprovedName"/>.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>A cached list of all unconditional improvements that do not add to ratings and that match the conditions set by the arguments.</returns>
        public static async Task<List<Improvement>> GetCachedImprovementListForValueOfAsync(
            Character objCharacter, Improvement.ImprovementType eImprovementType, string strImprovedName = "",
            bool blnIncludeNonImproved = false, CancellationToken token = default)
        {
            (decimal decReturn, List<Improvement> lstUsedImprovements) = await MetaValueOfAsync(
                objCharacter, eImprovementType, x => x.Value, s_DictionaryCachedValues, false,
                strImprovedName, true, blnIncludeNonImproved, token).ConfigureAwait(false);
            if (decReturn != 0 && lstUsedImprovements.Count == 0)
            {
                Log.Warn("A cached value modifier somehow is not zero while having no used improvements in its list.");
                Utils.BreakIfDebug();
            }
            return lstUsedImprovements;
        }

        /// <summary>
        /// Retrieve the total Improvement Augmented x Rating for the specified ImprovementType.
        /// </summary>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="objImprovementType">ImprovementType to retrieve the value of.</param>
        /// <param name="blnAddToRating">Whether we should only retrieve values that have AddToRating enabled.</param>
        /// <param name="strImprovedName">Name to assign to the Improvement.</param>
        /// <param name="blnUnconditionalOnly">Whether to only fetch values for improvements that do not have a condition.</param>
        /// <param name="blnIncludeNonImproved">Whether to only fetch values for improvements that do not have an improvedname when specifying ImprovedNames.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static decimal AugmentedValueOf(Character objCharacter, Improvement.ImprovementType objImprovementType,
                                               bool blnAddToRating = false, string strImprovedName = "",
                                               bool blnUnconditionalOnly = true, bool blnIncludeNonImproved = false, CancellationToken token = default)
        {
            (decimal decReturn, List<Improvement> lstUsedImprovements) = MetaValueOf(objCharacter, objImprovementType,
                x => x.Augmented * x.Rating,
                s_DictionaryCachedAugmentedValues, blnAddToRating, strImprovedName,
                blnUnconditionalOnly,
                blnIncludeNonImproved, token);
            if (decReturn != 0 && lstUsedImprovements.Count == 0)
            {
                Log.Warn("A cached augmented value modifier somehow is not zero while having no used improvements in its list.");
                Utils.BreakIfDebug();
            }
            return decReturn;
        }

        /// <summary>
        /// Retrieve the total Improvement Augmented x Rating for the specified ImprovementType.
        /// </summary>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="objImprovementType">ImprovementType to retrieve the value of.</param>
        /// <param name="blnAddToRating">Whether we should only retrieve values that have AddToRating enabled.</param>
        /// <param name="strImprovedName">Name to assign to the Improvement.</param>
        /// <param name="blnUnconditionalOnly">Whether to only fetch values for improvements that do not have a condition.</param>
        /// <param name="blnIncludeNonImproved">Whether to only fetch values for improvements that do not have an improvedname when specifying ImprovedNames.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task<decimal> AugmentedValueOfAsync(Character objCharacter, Improvement.ImprovementType objImprovementType,
                                                               bool blnAddToRating = false, string strImprovedName = "",
                                                               bool blnUnconditionalOnly = true, bool blnIncludeNonImproved = false, CancellationToken token = default)
        {
            (decimal decReturn, List<Improvement> lstUsedImprovements) = await MetaValueOfAsync(objCharacter, objImprovementType,
                x => x.Augmented * x.Rating,
                s_DictionaryCachedAugmentedValues, blnAddToRating, strImprovedName,
                blnUnconditionalOnly,
                blnIncludeNonImproved, token).ConfigureAwait(false);
            if (decReturn != 0 && lstUsedImprovements.Count == 0)
            {
                Log.Warn("A cached augmented value modifier somehow is not zero while having no used improvements in its list.");
                Utils.BreakIfDebug();
            }
            return decReturn;
        }

        /// <summary>
        /// Retrieve the total Improvement Augmented x Rating for the specified ImprovementType.
        /// </summary>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="objImprovementType">ImprovementType to retrieve the value of.</param>
        /// <param name="blnAddToRating">Whether we should only retrieve values that have AddToRating enabled.</param>
        /// <param name="strImprovedName">Name to assign to the Improvement.</param>
        /// <param name="blnUnconditionalOnly">Whether to only fetch values for improvements that do not have a condition.</param>
        /// <param name="blnIncludeNonImproved">Whether to only fetch values for improvements that do not have an improvedname when specifying ImprovedNames.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task<ValueTuple<decimal, List<Improvement>>> AugmentedValueOfTupleAsync(Character objCharacter, Improvement.ImprovementType objImprovementType,
                                                                bool blnAddToRating = false, string strImprovedName = "",
                                                                bool blnUnconditionalOnly = true, bool blnIncludeNonImproved = false, CancellationToken token = default)
        {
            (decimal decReturn, List<Improvement> lstUsedImprovements) = await MetaValueOfAsync(objCharacter, objImprovementType,
                x => x.Augmented * x.Rating,
                s_DictionaryCachedAugmentedValues, blnAddToRating, strImprovedName,
                blnUnconditionalOnly,
                blnIncludeNonImproved, token).ConfigureAwait(false);
            if (decReturn != 0 && lstUsedImprovements.Count == 0)
            {
                Log.Warn("A cached augmented value modifier somehow is not zero while having no used improvements in its list.");
                Utils.BreakIfDebug();
            }
            return new ValueTuple<decimal, List<Improvement>>(decReturn, lstUsedImprovements);
        }

        /// <summary>
        /// Retrieve the total Improvement Augmented x Rating for the specified ImprovementType.
        /// </summary>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="objImprovementType">ImprovementType to retrieve the value of.</param>
        /// <param name="lstUsedImprovements">List of the improvements actually used for the value.</param>
        /// <param name="blnAddToRating">Whether we should only retrieve values that have AddToRating enabled.</param>
        /// <param name="strImprovedName">Name to assign to the Improvement.</param>
        /// <param name="blnUnconditionalOnly">Whether to only fetch values for improvements that do not have a condition.</param>
        /// <param name="blnIncludeNonImproved">Whether to only fetch values for improvements that do not have an improvedname when specifying ImprovedNames.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static decimal AugmentedValueOf(Character objCharacter, Improvement.ImprovementType objImprovementType,
                                               out List<Improvement> lstUsedImprovements, bool blnAddToRating = false,
                                               string strImprovedName = "",
                                               bool blnUnconditionalOnly = true, bool blnIncludeNonImproved = false, CancellationToken token = default)
        {
            decimal decReturn;
            (decReturn, lstUsedImprovements) = MetaValueOf(objCharacter, objImprovementType,
                                                          x => x.Augmented * x.Rating,
                                                          s_DictionaryCachedAugmentedValues, blnAddToRating, strImprovedName,
                                                          blnUnconditionalOnly,
                                                          blnIncludeNonImproved, token);
            if (decReturn != 0 && lstUsedImprovements.Count == 0)
            {
                Log.Warn("A cached augmented value modifier somehow is not zero while having no used improvements in its list.");
                Utils.BreakIfDebug();
            }
            return decReturn;
        }

        /// <summary>
        /// Gets the cached list of active improvements that contribute to augmented values of a given improvement type.
        /// </summary>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="eImprovementType">ImprovementType for which active improvements should be fetched.</param>
        /// <param name="strImprovedName">Improvements are only fetched with the given improvedname. If empty, only those with an empty ImprovedName are fetched.</param>
        /// <param name="blnIncludeNonImproved">Whether to only fetch values for improvements that do not have an improvedname when specifying <paramref name="strImprovedName"/>.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>A cached list of all unconditional improvements that do not add to ratings and that match the conditions set by the arguments.</returns>
        public static List<Improvement> GetCachedImprovementListForAugmentedValueOf(
            Character objCharacter, Improvement.ImprovementType eImprovementType, string strImprovedName = "",
            bool blnIncludeNonImproved = false, CancellationToken token = default)
        {
            AugmentedValueOf(objCharacter, eImprovementType, out List<Improvement> lstReturn,
                             strImprovedName: strImprovedName, blnIncludeNonImproved: blnIncludeNonImproved, token: token);
            return lstReturn;
        }

        /// <summary>
        /// Gets the cached list of active improvements that contribute to augmented values of a given improvement type.
        /// </summary>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="eImprovementType">ImprovementType for which active improvements should be fetched.</param>
        /// <param name="strImprovedName">Improvements are only fetched with the given improvedname. If empty, only those with an empty ImprovedName are fetched.</param>
        /// <param name="blnIncludeNonImproved">Whether to only fetch values for improvements that do not have an improvedname when specifying <paramref name="strImprovedName"/>.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>A cached list of all unconditional improvements that do not add to ratings and that match the conditions set by the arguments.</returns>
        public static async Task<List<Improvement>> GetCachedImprovementListForAugmentedValueOfAsync(
            Character objCharacter, Improvement.ImprovementType eImprovementType, string strImprovedName = "",
            bool blnIncludeNonImproved = false, CancellationToken token = default)
        {
            (decimal decReturn, List<Improvement> lstUsedImprovements) = await MetaValueOfAsync(
                objCharacter, eImprovementType, x => x.Augmented * x.Rating, s_DictionaryCachedAugmentedValues, false,
                strImprovedName, true, blnIncludeNonImproved, token).ConfigureAwait(false);
            if (decReturn != 0 && lstUsedImprovements.Count == 0)
            {
                Log.Warn("A cached augmented value modifier somehow is not zero while having no used improvements in its list.");
                Utils.BreakIfDebug();
            }
            return lstUsedImprovements;
        }

        /// <summary>
        /// Internal function used for fetching some sort of collected value from a character's entire set of improvements
        /// </summary>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="eImprovementType">ImprovementType to retrieve the value of.</param>
        /// <param name="dicCachedValuesToUse">The caching dictionary to use. If null, values will not be cached.</param>
        /// <param name="blnAddToRating">Whether we should only retrieve values that have AddToRating enabled.</param>
        /// <param name="strImprovedName">Name to assign to the Improvement.</param>
        /// <param name="blnUnconditionalOnly">Whether to only fetch values for improvements that do not have a condition.</param>
        /// <param name="blnIncludeNonImproved">Whether to only fetch values for improvements that do not have an improvedname when specifying ImprovedNames.</param>
        /// <param name="funcValueGetter">Function for how to extract values for individual improvements.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        private static ValueTuple<decimal, List<Improvement>> MetaValueOf(Character objCharacter, Improvement.ImprovementType eImprovementType,
                                                                     Func<Improvement, decimal> funcValueGetter,
                                                                     ConcurrentDictionary<ImprovementDictionaryKey,
                                                                         ValueTuple<decimal, List<Improvement>>> dicCachedValuesToUse,
                                                                     bool blnAddToRating, string strImprovedName,
                                                                     bool blnUnconditionalOnly, bool blnIncludeNonImproved, CancellationToken token = default)
        {
            return Utils.SafelyRunSynchronously(() => MetaValueOfCoreAsync(true, objCharacter, eImprovementType, funcValueGetter, dicCachedValuesToUse,
                                                                           blnAddToRating, strImprovedName, blnUnconditionalOnly, blnIncludeNonImproved, token), token);
        }

        /// <summary>
        /// Internal function used for fetching some sort of collected value from a character's entire set of improvements
        /// </summary>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="eImprovementType">ImprovementType to retrieve the value of.</param>
        /// <param name="dicCachedValuesToUse">The caching dictionary to use. If null, values will not be cached.</param>
        /// <param name="blnAddToRating">Whether we should only retrieve values that have AddToRating enabled.</param>
        /// <param name="strImprovedName">Name to assign to the Improvement.</param>
        /// <param name="blnUnconditionalOnly">Whether to only fetch values for improvements that do not have a condition.</param>
        /// <param name="blnIncludeNonImproved">Whether to only fetch values for improvements that do not have an improvedname when specifying ImprovedNames.</param>
        /// <param name="funcValueGetter">Function for how to extract values for individual improvements.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        private static Task<ValueTuple<decimal, List<Improvement>>> MetaValueOfAsync(
            Character objCharacter, Improvement.ImprovementType eImprovementType,
            Func<Improvement, decimal> funcValueGetter,
            ConcurrentDictionary<ImprovementDictionaryKey,
                ValueTuple<decimal, List<Improvement>>> dicCachedValuesToUse,
            bool blnAddToRating, string strImprovedName,
            bool blnUnconditionalOnly, bool blnIncludeNonImproved, CancellationToken token = default)
        {
            return MetaValueOfCoreAsync(false, objCharacter, eImprovementType, funcValueGetter, dicCachedValuesToUse,
                                        blnAddToRating, strImprovedName, blnUnconditionalOnly, blnIncludeNonImproved, token);
        }

        /// <summary>
        /// Internal function used for fetching some sort of collected value from a character's entire set of improvements
        /// Uses flag hack method design outlined here to avoid locking:
        /// https://docs.microsoft.com/en-us/archive/msdn-magazine/2015/july/async-programming-brownfield-async-development
        /// </summary>
        /// <param name="blnSync">Flag for whether method should always use synchronous code or not.</param>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="eImprovementType">ImprovementType to retrieve the value of.</param>
        /// <param name="dicCachedValuesToUse">The caching dictionary to use. If null, values will not be cached.</param>
        /// <param name="blnAddToRating">Whether we should only retrieve values that have AddToRating enabled.</param>
        /// <param name="strImprovedName">Name to assign to the Improvement.</param>
        /// <param name="blnUnconditionalOnly">Whether to only fetch values for improvements that do not have a condition.</param>
        /// <param name="blnIncludeNonImproved">Whether to only fetch values for improvements that do not have an improvedname when specifying ImprovedNames.</param>
        /// <param name="funcValueGetter">Function for how to extract values for individual improvements.</param>
        /// <param name="token">CancellationToken to listen to.</param>
        private static async Task<ValueTuple<decimal, List<Improvement>>> MetaValueOfCoreAsync(bool blnSync, Character objCharacter, Improvement.ImprovementType eImprovementType,
                                                                                          Func<Improvement, decimal> funcValueGetter,
                                                                                          ConcurrentDictionary<ImprovementDictionaryKey,
                                                                                              ValueTuple<decimal, List<Improvement>>> dicCachedValuesToUse,
                                                                                          bool blnAddToRating, string strImprovedName,
                                                                                          bool blnUnconditionalOnly, bool blnIncludeNonImproved, CancellationToken token = default)
        {
            //Log.Info("objImprovementType = " + objImprovementType.ToString());
            //Log.Info("blnAddToRating = " + blnAddToRating.ToString());
            //Log.Info("strImprovedName = " + ("" + strImprovedName).ToString());
            token.ThrowIfCancellationRequested();

            if (funcValueGetter == null)
                throw new ArgumentNullException(nameof(funcValueGetter));

            if (objCharacter == null)
                return new ValueTuple<decimal, List<Improvement>>(0, new List<Improvement>(8));

            if (string.IsNullOrWhiteSpace(strImprovedName))
                strImprovedName = string.Empty;

            IDisposable objLocker = null;
            IAsyncDisposable objLockerAsync = null;
            if (blnSync)
                // ReSharper disable once MethodHasAsyncOverload
                objLocker = objCharacter.LockObject.EnterReadLock(token);
            else
                objLockerAsync = await objCharacter.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // These values are needed to prevent race conditions that could cause Chummer to crash
                ValueTuple<ImprovementDictionaryKey, ConcurrentDictionary<ImprovementDictionaryKey, ValueTuple<decimal, List<Improvement>>>> tupMyValueToCheck
                    = new ValueTuple<ImprovementDictionaryKey, ConcurrentDictionary<ImprovementDictionaryKey, ValueTuple<decimal, List<Improvement>>>>(
                        new ImprovementDictionaryKey(objCharacter, eImprovementType, strImprovedName),
                        dicCachedValuesToUse);
                ValueTuple<ImprovementDictionaryKey, ConcurrentDictionary<ImprovementDictionaryKey, ValueTuple<decimal, List<Improvement>>>> tupBlankValueToCheck
                    = new ValueTuple<ImprovementDictionaryKey, ConcurrentDictionary<ImprovementDictionaryKey, ValueTuple<decimal, List<Improvement>>>>(
                        new ImprovementDictionaryKey(objCharacter, eImprovementType, string.Empty),
                        dicCachedValuesToUse);

                // Only cache "default" ValueOf calls, otherwise there will be way too many values to cache
                bool blnFetchAndCacheResults = !blnAddToRating && blnUnconditionalOnly;

                // If we've got a value cached for the default ValueOf call for an improvementType, let's just return that
                List<Improvement> lstUsedImprovements = new List<Improvement>(blnSync
                    ? objCharacter.Improvements.Count
                    : await objCharacter.Improvements.GetCountAsync(token).ConfigureAwait(false));
                if (blnFetchAndCacheResults)
                {
                    if (dicCachedValuesToUse != null)
                    {
                        bool blnRepeatCheckCache;
                        do
                        {
                            blnRepeatCheckCache = false;
                            using (CancellationTokenSource objEmergencyRelease
                                   = new CancellationTokenSource(
                                       Utils.SleepEmergencyReleaseMaxTicks * Utils.DefaultSleepDuration))
                            {
                                CancellationToken objEmergencyReleaseToken = objEmergencyRelease.Token;
                                using (CancellationTokenSource objCombinedTokenSource
                                       = CancellationTokenSource.CreateLinkedTokenSource(
                                           token, objEmergencyReleaseToken))
                                {
                                    try
                                    {
                                        while (s_SetCurrentlyCalculatingValues.Contains(tupMyValueToCheck))
                                        {
                                            objEmergencyReleaseToken.ThrowIfCancellationRequested();
                                            if (blnSync)
                                                Utils.SafeSleep(objCombinedTokenSource.Token);
                                            else
                                                await Utils.SafeSleepAsync(objCombinedTokenSource.Token).ConfigureAwait(false);
                                        }
                                    }
                                    // Emergency exit, so break if we are debugging and return the default value (just in case)
                                    catch (OperationCanceledException)
                                    {
                                        if (objEmergencyReleaseToken.IsCancellationRequested)
                                        {
                                            Utils.BreakIfDebug();
                                            return new ValueTuple<decimal, List<Improvement>>(0, new List<Improvement>(8));
                                        }

                                        throw;
                                    }
                                }
                            }

                            // Also make sure we block off the conditionless check because we will be adding cached keys that will be used by the conditionless check
                            if (!string.IsNullOrWhiteSpace(strImprovedName) && !blnIncludeNonImproved)
                            {
                                using (CancellationTokenSource objEmergencyRelease
                                       = new CancellationTokenSource(
                                           Utils.SleepEmergencyReleaseMaxTicks * Utils.DefaultSleepDuration))
                                {
                                    CancellationToken objEmergencyReleaseToken = objEmergencyRelease.Token;
                                    using (CancellationTokenSource objCombinedTokenSource
                                           = CancellationTokenSource.CreateLinkedTokenSource(
                                               token, objEmergencyReleaseToken))
                                    {
                                        try
                                        {
                                            while (s_SetCurrentlyCalculatingValues.Contains(tupBlankValueToCheck))
                                            {
                                                objEmergencyReleaseToken.ThrowIfCancellationRequested();
                                                if (blnSync)
                                                    Utils.SafeSleep(objCombinedTokenSource.Token);
                                                else
                                                    await Utils.SafeSleepAsync(objCombinedTokenSource.Token)
                                                               .ConfigureAwait(false);
                                            }
                                        }
                                        // Emergency exit, so break if we are debugging and return the default value (just in case)
                                        catch (OperationCanceledException)
                                        {
                                            if (objEmergencyReleaseToken.IsCancellationRequested)
                                            {
                                                Utils.BreakIfDebug();
                                                return new ValueTuple<decimal, List<Improvement>>(
                                                    0, new List<Improvement>(8));
                                            }

                                            throw;
                                        }
                                    }
                                }

                                ImprovementDictionaryKey objCacheKey
                                    = new ImprovementDictionaryKey(objCharacter, eImprovementType, strImprovedName);
                                if (dicCachedValuesToUse.TryGetValue(
                                        objCacheKey, out ValueTuple<decimal, List<Improvement>> tupCachedValue) &&
                                    tupCachedValue.Item1 != decimal.MinValue)
                                {
                                    // To make sure we do not inadvertently alter the cached list
                                    return new ValueTuple<decimal, List<Improvement>>(
                                        tupCachedValue.Item1, tupCachedValue.Item2.ToList());
                                }

                                if (!s_SetCurrentlyCalculatingValues.TryAdd(tupMyValueToCheck))
                                    blnRepeatCheckCache = true;
                                else if (!s_SetCurrentlyCalculatingValues.TryAdd(tupBlankValueToCheck))
                                {
                                    s_SetCurrentlyCalculatingValues.Remove(tupMyValueToCheck);
                                    blnRepeatCheckCache = true;
                                }
                            }
                            else
                            {
                                bool blnDoRecalculate = true;
                                decimal decCachedValue = 0;
                                // Only fetch based on cached values if the dictionary contains at least one element with matching characters and types and none of those elements have a "reset" value of decimal.MinValue
                                foreach (KeyValuePair<ImprovementDictionaryKey, ValueTuple<decimal, List<Improvement>>>
                                             kvpLoopCachedEntry in dicCachedValuesToUse)
                                {
                                    token.ThrowIfCancellationRequested();
                                    ImprovementDictionaryKey objLoopKey = kvpLoopCachedEntry.Key;
                                    if (objLoopKey.CharacterObject != objCharacter ||
                                        objLoopKey.ImprovementType != eImprovementType)
                                        continue;
                                    token.ThrowIfCancellationRequested();
                                    if (!string.IsNullOrWhiteSpace(strImprovedName)
                                        && !string.IsNullOrWhiteSpace(objLoopKey.ImprovementName)
                                        && strImprovedName != objLoopKey.ImprovementName)
                                        continue;
                                    token.ThrowIfCancellationRequested();
                                    blnDoRecalculate = false;
                                    decimal decLoopCachedValue = kvpLoopCachedEntry.Value.Item1;
                                    if (decLoopCachedValue == decimal.MinValue)
                                    {
                                        blnDoRecalculate = true;
                                        break;
                                    }

                                    token.ThrowIfCancellationRequested();
                                    decCachedValue += decLoopCachedValue;
                                    lstUsedImprovements.AddRange(kvpLoopCachedEntry.Value.Item2);
                                }

                                if (blnDoRecalculate)
                                {
                                    lstUsedImprovements.Clear();
                                    if (!s_SetCurrentlyCalculatingValues.TryAdd(tupMyValueToCheck))
                                        blnRepeatCheckCache = true;
                                }
                                else
                                    return new ValueTuple<decimal, List<Improvement>>(decCachedValue, lstUsedImprovements);
                            }
                        } while (blnRepeatCheckCache);
                    }
                    else
                    {
                        // The code is breaking here to remind you (the programmer) to add in caching functionality for this type of value.
                        // The more often this sort of value is used, the more caching is necessary and the more often we will break here,
                        // and the annoyance of constantly having your debugger break here should push you to adding in caching functionality.
                        Utils.BreakIfDebug();
                        lstUsedImprovements.Clear();
                    }
                }
                else
                    lstUsedImprovements.Clear();

                try
                {
                    string strConditionToAcceptForUnconditional = (blnSync ? objCharacter.Created : await objCharacter.GetCreatedAsync(token).ConfigureAwait(false))
                        ? "career"
                        : "create";
                    List<Improvement> lstImprovementsToConsider;
                    if (blnSync)
                    {
                        lstImprovementsToConsider = new List<Improvement>(objCharacter.Improvements.Count);
                        // ReSharper disable once MethodHasAsyncOverload
                        objCharacter.Improvements.ForEach(ImprovementsLoopCommon, token);
                    }
                    else
                    {
                        lstImprovementsToConsider =
                            new List<Improvement>(await objCharacter.Improvements.GetCountAsync(token)
                                .ConfigureAwait(false));
                        await objCharacter.Improvements.ForEachAsync(ImprovementsLoopCommon, token)
                            .ConfigureAwait(false);
                    }

                    void ImprovementsLoopCommon(Improvement objImprovement)
                    {
                        if (objImprovement.ImproveType != eImprovementType || !objImprovement.Enabled)
                            return;
                        if (blnUnconditionalOnly && !string.IsNullOrEmpty(objImprovement.Condition) && objImprovement.Condition != strConditionToAcceptForUnconditional)
                            return;
                        // Matrix initiative boosting gear does not help Living Personas
                        if ((eImprovementType == Improvement.ImprovementType.MatrixInitiativeDice
                             || eImprovementType == Improvement.ImprovementType.MatrixInitiative
                             || eImprovementType == Improvement.ImprovementType.MatrixInitiativeDiceAdd)
                            && objImprovement.ImproveSource == Improvement.ImprovementSource.Gear
                            && objCharacter.ActiveCommlink is Gear objCommlink
                            && objCommlink.Name == "Living Persona")
                            return;
                        // Ignore items that apply to a Skill's Rating.
                        if (objImprovement.AddToRating != blnAddToRating)
                            return;
                        // If an Improved Name has been passed, only retrieve values that have this Improved Name.
                        if (!string.IsNullOrEmpty(strImprovedName))
                        {
                            string strLoopImprovedName = objImprovement.ImprovedName;
                            if (strImprovedName != strLoopImprovedName
                                && !(blnIncludeNonImproved && string.IsNullOrWhiteSpace(strLoopImprovedName)))
                                return;
                        }

                        lstImprovementsToConsider.Add(objImprovement);
                    }

                    List<Improvement> lstLoopImprovements;
                    Dictionary<string, List<ValueTuple<string, Improvement>>> dicUniquePairs
                        = new Dictionary<string, List<ValueTuple<string, Improvement>>>(lstImprovementsToConsider.Count);
                    Dictionary<string, decimal> dicValues
                        = new Dictionary<string, decimal>(lstImprovementsToConsider.Count);
                    Dictionary<string, List<Improvement>> dicImprovementsForValues
                        = new Dictionary<string, List<Improvement>>(lstImprovementsToConsider.Count);
                    Dictionary<string, HashSet<string>> dicUniqueNames
                        = new Dictionary<string, HashSet<string>>(lstImprovementsToConsider.Count);
                    try
                    {
                        foreach (Improvement objImprovement in lstImprovementsToConsider)
                        {
                            // Count custom improvements later
                            if (objImprovement.Custom)
                                continue;
                            string strLoopImprovedName = objImprovement.ImprovedName;
                            string strUniqueName = objImprovement.UniqueName;
                            if (!string.IsNullOrEmpty(strUniqueName))
                            {
                                // If this has a UniqueName, run through the current list of UniqueNames seen. If it is not already in the list, add it.
                                if (dicUniqueNames.TryGetValue(strLoopImprovedName, out HashSet<string> setUniqueNames))
                                {
                                    setUniqueNames.Add(strUniqueName);
                                }
                                else
                                {
                                    HashSet<string> setTemp = Utils.StringHashSetPool.Get();
                                    setTemp.Add(strUniqueName);
                                    dicUniqueNames.Add(strLoopImprovedName, setTemp);
                                }

                                // Add the values to the UniquePair List so we can check them later.
                                if (dicUniquePairs.TryGetValue(strLoopImprovedName,
                                                               out List<ValueTuple<string, Improvement>> lstUniquePairs))
                                {
                                    lstUniquePairs.Add(new ValueTuple<string, Improvement>(strUniqueName, objImprovement));
                                }
                                else
                                {
                                    dicUniquePairs.Add(strLoopImprovedName,
                                                       new List<ValueTuple<string, Improvement>>(lstImprovementsToConsider.Count)
                                                       {
                                                           new ValueTuple<string, Improvement>(strUniqueName, objImprovement)
                                                       });
                                }

                                if (!dicValues.ContainsKey(strLoopImprovedName))
                                {
                                    dicValues.Add(strLoopImprovedName, 0);
                                    dicImprovementsForValues.Add(strLoopImprovedName, new List<Improvement>(lstImprovementsToConsider.Count));
                                }
                            }
                            else if (dicValues.TryGetValue(strLoopImprovedName, out decimal decExistingValue))
                            {
                                dicValues[strLoopImprovedName] = decExistingValue + funcValueGetter(objImprovement);
                                dicImprovementsForValues[strLoopImprovedName].Add(objImprovement);
                            }
                            else
                            {
                                dicValues.Add(strLoopImprovedName, funcValueGetter(objImprovement));
                                dicImprovementsForValues.Add(strLoopImprovedName,
                                                             new List<Improvement>(lstImprovementsToConsider.Count) { objImprovement });
                            }
                        }

                        List<Improvement> lstInnerLoopImprovements = new List<Improvement>(lstImprovementsToConsider.Count);
                        foreach (KeyValuePair<string, HashSet<string>> objLoopValuePair in dicUniqueNames)
                        {
                            string strLoopImprovedName = objLoopValuePair.Key;
                            bool blnValuesDictionaryContains
                                = dicValues.TryGetValue(strLoopImprovedName, out decimal decLoopValue);
                            if (blnValuesDictionaryContains)
                                dicImprovementsForValues.TryGetValue(strLoopImprovedName, out lstLoopImprovements);
                            else
                                lstLoopImprovements = new List<Improvement>(dicUniqueNames.Count);
                            if (dicUniquePairs.TryGetValue(strLoopImprovedName,
                                                           out List<ValueTuple<string, Improvement>> lstUniquePairs))
                            {
                                HashSet<string> setUniqueNames = objLoopValuePair.Value;
                                lstInnerLoopImprovements.Clear();
                                if (setUniqueNames.Contains("precedence0"))
                                {
                                    // Retrieve only the highest precedence0 value.
                                    // Run through the list of UniqueNames and pick out the highest value for each one.
                                    Improvement objHighestImprovement = null;
                                    decimal decHighest = decimal.MinValue;
                                    foreach ((string strUnique, Improvement objLoopImprovement) in lstUniquePairs)
                                    {
                                        if (strUnique != "precedence0")
                                            continue;
                                        decimal decInnerLoopValue = funcValueGetter(objLoopImprovement);
                                        if (decHighest < decInnerLoopValue)
                                        {
                                            decHighest = decInnerLoopValue;
                                            objHighestImprovement = objLoopImprovement;
                                        }
                                    }

                                    if (objHighestImprovement != null)
                                        lstInnerLoopImprovements.Add(objHighestImprovement);

                                    if (setUniqueNames.Contains("precedence-1"))
                                    {
                                        foreach ((string strUnique, Improvement objLoopImprovement) in lstUniquePairs)
                                        {
                                            if (strUnique != "precedence-1")
                                                continue;
                                            decHighest += funcValueGetter(objLoopImprovement);
                                            lstInnerLoopImprovements.Add(objLoopImprovement);
                                        }
                                    }

                                    if (decLoopValue < decHighest)
                                    {
                                        decLoopValue = decHighest;
                                        lstLoopImprovements.Clear();
                                        lstLoopImprovements.AddRange(lstInnerLoopImprovements);
                                    }
                                }
                                else if (setUniqueNames.Contains("precedence1"))
                                {
                                    // Retrieve all the items that are precedence1 and nothing else.
                                    decimal decHighest = 0;
                                    foreach ((string strUnique, Improvement objLoopImprovement) in lstUniquePairs)
                                    {
                                        if (strUnique != "precedence1" && strUnique != "precedence-1")
                                            continue;
                                        decHighest += funcValueGetter(objLoopImprovement);
                                        lstInnerLoopImprovements.Add(objLoopImprovement);
                                    }

                                    if (decLoopValue < decHighest)
                                    {
                                        decLoopValue = decHighest;
                                        lstLoopImprovements.Clear();
                                        lstLoopImprovements.AddRange(lstInnerLoopImprovements);
                                    }
                                }
                                else
                                {
                                    // Run through the list of UniqueNames and pick out the highest value for each one.
                                    foreach (string strUniqueName in setUniqueNames)
                                    {
                                        Improvement objHighestImprovement = null;
                                        decimal decHighest = decimal.MinValue;
                                        foreach ((string strUnique, Improvement objLoopImprovement) in lstUniquePairs)
                                        {
                                            if (strUnique != strUniqueName)
                                                continue;
                                            decimal decInnerLoopValue = funcValueGetter(objLoopImprovement);
                                            if (decHighest < decInnerLoopValue)
                                            {
                                                decHighest = decInnerLoopValue;
                                                objHighestImprovement = objLoopImprovement;
                                            }
                                        }

                                        if (decHighest != decimal.MinValue)
                                        {
                                            decLoopValue += decHighest;
                                            lstLoopImprovements.Add(objHighestImprovement);
                                        }
                                    }
                                }

                                if (blnValuesDictionaryContains)
                                    dicValues[strLoopImprovedName] = decLoopValue;
                                else
                                {
                                    dicValues.Add(strLoopImprovedName, decLoopValue);
                                    dicImprovementsForValues.Add(strLoopImprovedName, lstLoopImprovements);
                                }
                            }
                        }
                    }
                    finally
                    {
                        List<HashSet<string>> lstToReturn = dicUniqueNames.Values.ToList();
                        for (int i = lstToReturn.Count - 1; i >= 0; --i)
                        {
                            HashSet<string> setLoop = lstToReturn[i];
                            Utils.StringHashSetPool.Return(ref setLoop);
                        }
                    }

                    // Factor in Custom Improvements.
                    dicUniqueNames.Clear();
                    dicUniquePairs.Clear();
                    Dictionary<string, decimal> dicCustomValues
                        = new Dictionary<string, decimal>(lstImprovementsToConsider.Count);
                    Dictionary<string, List<Improvement>> dicCustomImprovementsForValues
                        = new Dictionary<string, List<Improvement>>(lstImprovementsToConsider.Count);
                    try
                    {
                        foreach (Improvement objImprovement in lstImprovementsToConsider)
                        {
                            if (!objImprovement.Custom)
                                continue;
                            string strLoopImprovedName = objImprovement.ImprovedName;
                            string strUniqueName = objImprovement.UniqueName;
                            if (!string.IsNullOrEmpty(strUniqueName))
                            {
                                // If this has a UniqueName, run through the current list of UniqueNames seen. If it is not already in the list, add it.
                                if (dicUniqueNames.TryGetValue(strLoopImprovedName, out HashSet<string> setUniqueNames))
                                {
                                    setUniqueNames.Add(strUniqueName);
                                }
                                else
                                {
                                    HashSet<string> setTemp = Utils.StringHashSetPool.Get();
                                    setTemp.Add(strUniqueName);
                                    dicUniqueNames.Add(strLoopImprovedName, setTemp);
                                }

                                // Add the values to the UniquePair List so we can check them later.
                                if (dicUniquePairs.TryGetValue(strLoopImprovedName,
                                                               out List<ValueTuple<string, Improvement>> lstUniquePairs))
                                {
                                    lstUniquePairs.Add(new ValueTuple<string, Improvement>(strUniqueName, objImprovement));
                                }
                                else
                                {
                                    dicUniquePairs.Add(strLoopImprovedName,
                                                       new List<ValueTuple<string, Improvement>>(lstImprovementsToConsider.Count)
                                                       {
                                                           new ValueTuple<string, Improvement>(strUniqueName, objImprovement)
                                                       });
                                }

                                if (!dicCustomValues.ContainsKey(strLoopImprovedName))
                                {
                                    dicCustomValues.Add(strLoopImprovedName, 0);
                                    dicCustomImprovementsForValues.Add(strLoopImprovedName, new List<Improvement>(lstImprovementsToConsider.Count));
                                }
                            }
                            else if (dicCustomValues.TryGetValue(strLoopImprovedName, out decimal decExistingValue))
                            {
                                dicCustomValues[strLoopImprovedName]
                                    = decExistingValue + funcValueGetter(objImprovement);
                                dicCustomImprovementsForValues[strLoopImprovedName].Add(objImprovement);
                            }
                            else
                            {
                                dicCustomValues.Add(strLoopImprovedName, funcValueGetter(objImprovement));
                                dicCustomImprovementsForValues.Add(strLoopImprovedName,
                                                                   new List<Improvement>(lstImprovementsToConsider.Count) { objImprovement });
                            }
                        }

                        foreach (KeyValuePair<string, HashSet<string>> objLoopValuePair in dicUniqueNames)
                        {
                            string strLoopImprovedName = objLoopValuePair.Key;
                            bool blnValuesDictionaryContains
                                = dicCustomValues.TryGetValue(strLoopImprovedName, out decimal decLoopValue);
                            if (blnValuesDictionaryContains)
                                dicImprovementsForValues.TryGetValue(strLoopImprovedName, out lstLoopImprovements);
                            else
                                lstLoopImprovements = new List<Improvement>(dicUniqueNames.Count);
                            if (dicUniquePairs.TryGetValue(strLoopImprovedName,
                                                           out List<ValueTuple<string, Improvement>> lstUniquePairs))
                            {
                                // Run through the list of UniqueNames and pick out the highest value for each one.
                                foreach (string strUniqueName in objLoopValuePair.Value)
                                {
                                    Improvement objHighestImprovement = null;
                                    decimal decHighest = decimal.MinValue;
                                    foreach ((string strUnique, Improvement objLoopImprovement) in lstUniquePairs)
                                    {
                                        if (strUnique != strUniqueName)
                                            continue;
                                        decimal decInnerLoopValue = funcValueGetter(objLoopImprovement);
                                        if (decHighest < decInnerLoopValue)
                                        {
                                            decHighest = decInnerLoopValue;
                                            objHighestImprovement = objLoopImprovement;
                                        }
                                    }

                                    if (decHighest != decimal.MinValue)
                                    {
                                        decLoopValue += decHighest;
                                        (lstLoopImprovements ?? (lstLoopImprovements = new List<Improvement>(lstImprovementsToConsider.Count))).Add(
                                            objHighestImprovement);
                                    }
                                }

                                if (blnValuesDictionaryContains)
                                    dicCustomValues[strLoopImprovedName] = decLoopValue;
                                else
                                {
                                    dicCustomValues.Add(strLoopImprovedName, decLoopValue);
                                    dicCustomImprovementsForValues.Add(strLoopImprovedName, lstLoopImprovements);
                                }
                            }
                        }
                    }
                    finally
                    {
                        List<HashSet<string>> lstToReturn = dicUniqueNames.Values.ToList();
                        for (int i = lstToReturn.Count - 1; i >= 0; --i)
                        {
                            HashSet<string> setLoop = lstToReturn[i];
                            Utils.StringHashSetPool.Return(ref setLoop);
                        }
                    }

                    foreach (KeyValuePair<string, decimal> objLoopValuePair in dicCustomValues)
                    {
                        string strLoopImprovedName = objLoopValuePair.Key;
                        if (dicValues.TryGetValue(strLoopImprovedName, out decimal decExistingValue))
                        {
                            dicValues[strLoopImprovedName] = decExistingValue + objLoopValuePair.Value;
                            dicImprovementsForValues[strLoopImprovedName]
                                .AddRange(dicCustomImprovementsForValues[strLoopImprovedName]);
                        }
                        else
                        {
                            dicValues.Add(strLoopImprovedName, objLoopValuePair.Value);
                            dicImprovementsForValues.Add(strLoopImprovedName,
                                                         dicCustomImprovementsForValues[strLoopImprovedName]);
                        }
                    }

                    decimal decReturn = 0;

                    foreach (KeyValuePair<string, decimal> objLoopValuePair in dicValues)
                    {
                        string strLoopImprovedName = objLoopValuePair.Key;
                        decimal decLoopValue = objLoopValuePair.Value;
                        // If this is the default ValueOf() call, let's cache the value we've calculated so that we don't have to do this all over again unless something has changed
                        if (blnFetchAndCacheResults)
                        {
                            ValueTuple<decimal, List<Improvement>> tupNewValue =
                                new ValueTuple<decimal, List<Improvement>>(decLoopValue,
                                                                      dicImprovementsForValues[strLoopImprovedName]);
                            if (dicCachedValuesToUse != null)
                            {
                                ImprovementDictionaryKey objLoopCacheKey =
                                    new ImprovementDictionaryKey(objCharacter, eImprovementType, strLoopImprovedName);
                                token.ThrowIfCancellationRequested();
                                if (!dicCachedValuesToUse.TryAdd(objLoopCacheKey, tupNewValue))
                                {
                                    List<Improvement> lstTemp = dicCachedValuesToUse.TryGetValue(
                                        objLoopCacheKey, out ValueTuple<decimal, List<Improvement>> tupTemp)
                                        ? tupTemp.Item2
                                        : new List<Improvement>(tupNewValue.Item2.Count);

                                    if (!ReferenceEquals(lstTemp, tupNewValue.Item2))
                                    {
                                        lstTemp.Clear();
                                        lstTemp.AddRange(tupNewValue.Item2);
                                        tupNewValue = new ValueTuple<decimal, List<Improvement>>(decLoopValue, lstTemp);
                                    }

                                    dicCachedValuesToUse.AddOrUpdate(objLoopCacheKey, tupNewValue,
                                        (x, y) => tupNewValue);
                                }
                            }

                            lstUsedImprovements.AddRange(tupNewValue.Item2);
                        }
                        else
                            lstUsedImprovements.AddRange(dicImprovementsForValues[strLoopImprovedName]);

                        decReturn += decLoopValue;
                    }

                    return new ValueTuple<decimal, List<Improvement>>(decReturn, lstUsedImprovements);
                }
                finally
                {
                    // As a final step, remove the tuple used to flag an improvement value as currently being cached
                    s_SetCurrentlyCalculatingValues.Remove(tupMyValueToCheck);
                    s_SetCurrentlyCalculatingValues.Remove(tupBlankValueToCheck);
                }
            }
            finally
            {
                objLocker?.Dispose();
                if (objLockerAsync != null)
                    await objLockerAsync.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Convert a string to an integer, converting "Rating" to a number where appropriate.
        /// </summary>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="strValue">String value to parse.</param>
        /// <param name="intRating">Integer value to replace "Rating" with.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static int ValueToInt(Character objCharacter, string strValue, int intRating, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(strValue))
                return 0;
            //         Log.Enter("ValueToInt");
            //         Log.Info("strValue = " + strValue);
            //Log.Info("intRating = " + intRating.ToString());
            strValue = strValue.ProcessFixedValuesString(intRating)
                .Replace("{Rating}", intRating.ToString(GlobalSettings.InvariantCultureInfo))
                .Replace("Rating", intRating.ToString(GlobalSettings.InvariantCultureInfo));
            if (strValue.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
            {
                string strReturn = objCharacter.ProcessAttributesInXPath(strValue, token: token);

                //Log.Info("strValue = " + strValue);
                //Log.Info("strReturn = " + strReturn);

                // Treat this as a decimal value so any fractions can be rounded down. This is currently only used by the Boosted Reflexes Cyberware from SR2050.
                (bool blnIsSuccess, object objProcess) = CommonFunctions.EvaluateInvariantXPath(strReturn, token);

                //Log.Exit("ValueToInt");
                return blnIsSuccess ? ((double)objProcess).StandardRound() : 0;
            }

            //Log.Exit("ValueToInt");
            return decValue.StandardRound();
        }

        /// <summary>
        /// Convert a string to an integer, converting "Rating" to a number where appropriate.
        /// </summary>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="strValue">String value to parse.</param>
        /// <param name="intRating">Integer value to replace "Rating" with.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task<int> ValueToIntAsync(Character objCharacter, string strValue, int intRating, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(strValue))
                return 0;
            //         Log.Enter("ValueToInt");
            //         Log.Info("strValue = " + strValue);
            //Log.Info("intRating = " + intRating.ToString());
            strValue = strValue.ProcessFixedValuesString(intRating)
                .Replace("{Rating}", intRating.ToString(GlobalSettings.InvariantCultureInfo))
                .Replace("Rating", intRating.ToString(GlobalSettings.InvariantCultureInfo));
            if (strValue.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
            {
                string strReturn = await objCharacter.ProcessAttributesInXPathAsync(strValue, token: token).ConfigureAwait(false);

                //Log.Info("strValue = " + strValue);
                //Log.Info("strReturn = " + strReturn);

                // Treat this as a decimal value so any fractions can be rounded down. This is currently only used by the Boosted Reflexes Cyberware from SR2050.
                (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(strReturn, token).ConfigureAwait(false);

                //Log.Exit("ValueToInt");
                return blnIsSuccess ? ((double)objProcess).StandardRound() : 0;
            }

            //Log.Exit("ValueToInt");
            return decValue.StandardRound();
        }

        /// <summary>
        /// Convert a string to a decimal, converting "Rating" to a number where appropriate.
        /// </summary>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="strValue">String value to parse.</param>
        /// <param name="intRating">Integer value to replace "Rating" with.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static decimal ValueToDec(Character objCharacter, string strValue, int intRating, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(strValue))
                return 0;
            //         Log.Enter("ValueToInt");
            //         Log.Info("strValue = " + strValue);
            //Log.Info("intRating = " + intRating.ToString());
            strValue = strValue.ProcessFixedValuesString(intRating)
                .Replace("{Rating}", intRating.ToString(GlobalSettings.InvariantCultureInfo))
                .Replace("Rating", intRating.ToString(GlobalSettings.InvariantCultureInfo));
            if (strValue.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
            {
                string strReturn = objCharacter.ProcessAttributesInXPath(strValue, token: token);

                //Log.Info("strValue = " + strValue);
                //Log.Info("strReturn = " + strReturn);

                // Treat this as a decimal value so any fractions can be rounded down. This is currently only used by the Boosted Reflexes Cyberware from SR2050.
                (bool blnIsSuccess, object objProcess) = CommonFunctions.EvaluateInvariantXPath(strReturn, token);

                //Log.Exit("ValueToInt");
                return blnIsSuccess ? Convert.ToDecimal((double)objProcess) : 0;
            }

            //Log.Exit("ValueToInt");
            return decValue;
        }

        /// <summary>
        /// Convert a string to a decimal, converting "Rating" to a number where appropriate.
        /// </summary>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="strValue">String value to parse.</param>
        /// <param name="intRating">Integer value to replace "Rating" with.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task<decimal> ValueToDecAsync(Character objCharacter, string strValue, int intRating, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(strValue))
                return 0;
            //         Log.Enter("ValueToInt");
            //         Log.Info("strValue = " + strValue);
            //Log.Info("intRating = " + intRating.ToString());
            strValue = strValue.ProcessFixedValuesString(intRating)
                .Replace("{Rating}", intRating.ToString(GlobalSettings.InvariantCultureInfo))
                .Replace("Rating", intRating.ToString(GlobalSettings.InvariantCultureInfo));
            if (strValue.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
            {
                string strReturn = await objCharacter.ProcessAttributesInXPathAsync(strValue, token: token).ConfigureAwait(false);

                //Log.Info("strValue = " + strValue);
                //Log.Info("strReturn = " + strReturn);

                // Treat this as a decimal value so any fractions can be rounded down. This is currently only used by the Boosted Reflexes Cyberware from SR2050.
                (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(strReturn, token).ConfigureAwait(false);

                //Log.Exit("ValueToInt");
                return blnIsSuccess ? Convert.ToDecimal((double)objProcess) : 0;
            }

            //Log.Exit("ValueToInt");
            return decValue;
        }

        public static ValueTuple<string, bool> DoSelectSkill(XmlNode xmlBonusNode, Character objCharacter, int intRating,
            string strFriendlyName, bool blnIsKnowledgeSkill = false, CancellationToken token = default)
        {
            return Utils.SafelyRunSynchronously(() => DoSelectSkillCoreAsync(false, xmlBonusNode, objCharacter,
                intRating, strFriendlyName,
                blnIsKnowledgeSkill, token), token);
        }

        public static Task<ValueTuple<string, bool>> DoSelectSkillAsync(XmlNode xmlBonusNode, Character objCharacter, int intRating,
            string strFriendlyName, bool blnIsKnowledgeSkill = false, CancellationToken token = default)
        {
            return DoSelectSkillCoreAsync(false, xmlBonusNode, objCharacter, intRating, strFriendlyName,
                blnIsKnowledgeSkill, token);
        }

        private static async Task<ValueTuple<string, bool>> DoSelectSkillCoreAsync(bool blnSync, XmlNode xmlBonusNode, Character objCharacter, int intRating,
                                           string strFriendlyName, bool blnIsKnowledgeSkill, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (xmlBonusNode == null)
                throw new ArgumentNullException(nameof(xmlBonusNode));
            if (objCharacter == null)
                throw new ArgumentNullException(nameof(objCharacter));
            string strSelectedSkill;
            blnIsKnowledgeSkill = blnIsKnowledgeSkill
                                  || xmlBonusNode.Attributes?["knowledgeskills"]?.InnerTextIsTrueString() == true;
            if (blnIsKnowledgeSkill)
            {
                int intMinimumRating = 0;
                string strMinimumRating = xmlBonusNode.Attributes?["minimumrating"]?.InnerTextViaPool(token);
                if (!string.IsNullOrWhiteSpace(strMinimumRating))
                    intMinimumRating = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? ValueToInt(objCharacter, strMinimumRating, intRating, token)
                        : await ValueToIntAsync(objCharacter, strMinimumRating, intRating, token).ConfigureAwait(false);
                int intMaximumRating = int.MaxValue;
                string strMaximumRating = xmlBonusNode.Attributes?["maximumrating"]?.InnerTextViaPool(token);
                string strPrompt = xmlBonusNode.Attributes?["prompt"]?.InnerTextViaPool(token) ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(strMaximumRating))
                    intMaximumRating = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? ValueToInt(objCharacter, strMaximumRating, intRating, token)
                        : await ValueToIntAsync(objCharacter, strMaximumRating, intRating, token).ConfigureAwait(false);

                using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                out HashSet<string>
                                                                    setAllowedCategories))
                {
                    string strOnlyCategory = xmlBonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@skillcategory", token)?.Value;
                    if (!string.IsNullOrEmpty(strOnlyCategory))
                    {
                        setAllowedCategories.AddRange(strOnlyCategory
                                                      .SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries)
                                                      .Select(x => x.Trim()));
                    }
                    else
                    {
                        using (XmlNodeList xmlCategoryList = xmlBonusNode.SelectNodes("skillcategories/category"))
                        {
                            if (xmlCategoryList?.Count > 0)
                            {
                                foreach (XmlNode objNode in xmlCategoryList)
                                {
                                    setAllowedCategories.Add(objNode.InnerTextViaPool(token));
                                }
                            }
                        }
                    }

                    using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                    out HashSet<string>
                                                                        setForbiddenCategories))
                    {
                        string strExcludeCategory = xmlBonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@excludecategory", token)?.Value;
                        if (!string.IsNullOrEmpty(strExcludeCategory))
                        {
                            setForbiddenCategories.AddRange(
                                strExcludeCategory.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries)
                                                  .Select(x => x.Trim()));
                        }

                        using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                        out HashSet<string>
                                                                            setAllowedNames))
                        {
                            string strForcedValue = GetForcedValue(objCharacter);
                            if (!string.IsNullOrEmpty(strForcedValue))
                            {
                                setAllowedNames.Add(strForcedValue);
                            }
                            else if (!string.IsNullOrEmpty(strPrompt))
                            {
                                setAllowedNames.Add(strPrompt);
                            }
                            else
                            {
                                string strLimitToSkill = xmlBonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@limittoskill", token)?.Value;
                                if (!string.IsNullOrEmpty(strLimitToSkill))
                                {
                                    setAllowedNames.AddRange(
                                        strLimitToSkill.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries)
                                                       .Select(x => x.Trim()));
                                }
                            }

                            using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                            out HashSet<string>
                                                                                setAllowedLinkedAttributes))
                            {
                                string strLimitToAttribute
                                    = xmlBonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@limittoattribute", token)?.Value;
                                if (!string.IsNullOrEmpty(strLimitToAttribute))
                                {
                                    setAllowedLinkedAttributes.AddRange(
                                        strLimitToAttribute.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries)
                                                           .Select(x => x.Trim()));
                                }

                                using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
                                                                               out List<ListItem> lstDropdownItems))
                                {
                                    using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool,
                                               out HashSet<string>
                                                   setProcessedSkillNames))
                                    {
                                        IDisposable objLocker = null;
                                        IAsyncDisposable objLockerAsync = null;
                                        if (blnSync)
                                            objLocker = objCharacter.LockObject.EnterReadLock(token);
                                        else
                                            objLockerAsync = await objCharacter.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
                                        try
                                        {
                                            if (blnSync)
                                            {
                                                foreach (KnowledgeSkill objKnowledgeSkill in objCharacter.SkillsSection
                                                             .KnowledgeSkills)
                                                {
                                                    token.ThrowIfCancellationRequested();
                                                    string strName = objKnowledgeSkill.Name;
                                                    if (setAllowedCategories?.Contains(objKnowledgeSkill
                                                            .SkillCategory) !=
                                                        false
                                                        &&
                                                        setForbiddenCategories?.Contains(
                                                            objKnowledgeSkill.SkillCategory)
                                                        != true &&
                                                        setAllowedNames?.Contains(strName) != false &&
                                                        setAllowedLinkedAttributes?.Contains(blnSync
                                                            ? objKnowledgeSkill.Attribute
                                                            : await objKnowledgeSkill.GetAttributeAsync(token).ConfigureAwait(false))
                                                        != false)
                                                    {
                                                        int intSkillRating = objKnowledgeSkill.Rating;
                                                        if (intSkillRating >= intMinimumRating &&
                                                            intRating < intMaximumRating)
                                                        {
                                                            lstDropdownItems.Add(
                                                                new ListItem(strName,
                                                                    objKnowledgeSkill.CurrentDisplayName));
                                                        }
                                                    }

                                                    setProcessedSkillNames.Add(strName);
                                                }
                                            }
                                            else
                                            {
                                                await objCharacter.SkillsSection.KnowledgeSkills.ForEachAsync(
                                                    async objKnowledgeSkill =>
                                                    {
                                                        string strName = await objKnowledgeSkill.GetNameAsync(token).ConfigureAwait(false);
                                                        if (setAllowedCategories?.Contains(objKnowledgeSkill
                                                                .SkillCategory) !=
                                                            false
                                                            &&
                                                            setForbiddenCategories?.Contains(
                                                                objKnowledgeSkill.SkillCategory)
                                                            != true &&
                                                            setAllowedNames?.Contains(strName) !=
                                                            false &&
                                                            setAllowedLinkedAttributes?.Contains(
                                                                await objKnowledgeSkill.GetAttributeAsync(token).ConfigureAwait(false))
                                                            != false)
                                                        {
                                                            int intSkillRating =
                                                                await objKnowledgeSkill.GetRatingAsync(token).ConfigureAwait(false);
                                                            if (intSkillRating >= intMinimumRating &&
                                                                intRating < intMaximumRating)
                                                            {
                                                                lstDropdownItems.Add(
                                                                    new ListItem(strName,
                                                                        await objKnowledgeSkill
                                                                            .GetCurrentDisplayNameAsync(token).ConfigureAwait(false)));
                                                            }
                                                        }

                                                        setProcessedSkillNames.Add(strName);
                                                    }, token).ConfigureAwait(false);
                                            }

                                            if (!string.IsNullOrEmpty(strPrompt)
                                                && !setProcessedSkillNames.Contains(strPrompt))
                                            {
                                                lstDropdownItems.Add(
                                                    new ListItem(strPrompt,
                                                        blnSync
                                                            // ReSharper disable once MethodHasAsyncOverload
                                                            ? objCharacter.TranslateExtra(strPrompt, token: token)
                                                            : await objCharacter.TranslateExtraAsync(strPrompt,
                                                                token: token).ConfigureAwait(false)));
                                                setProcessedSkillNames.Add(strPrompt);
                                            }

                                            if (intMinimumRating <= 0)
                                            {
                                                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                           out StringBuilder sbdFilter))
                                                {
                                                    if (setAllowedCategories?.Count > 0)
                                                    {
                                                        sbdFilter.Append('(');
                                                        foreach (string strCategory in setAllowedCategories)
                                                        {
                                                            sbdFilter.Append("category = ", strCategory.CleanXPath(), " or ");
                                                        }

                                                        sbdFilter.Length -= 4;
                                                        sbdFilter.Append(')');
                                                    }

                                                    if (setForbiddenCategories?.Count > 0)
                                                    {
                                                        sbdFilter.Append(sbdFilter.Length > 0 ? " and not(" : "not(");
                                                        foreach (string strCategory in setForbiddenCategories)
                                                        {
                                                            sbdFilter.Append("category = ", strCategory.CleanXPath(), " or ");
                                                        }

                                                        sbdFilter.Length -= 4;
                                                        sbdFilter.Append(')');
                                                    }

                                                    if (setAllowedNames?.Count > 0)
                                                    {
                                                        sbdFilter.Append(sbdFilter.Length > 0 ? " and (" : "(");
                                                        foreach (string strName in setAllowedNames)
                                                        {
                                                            sbdFilter.Append("name = ", strName.CleanXPath(), " or ");
                                                        }

                                                        sbdFilter.Length -= 4;
                                                        sbdFilter.Append(')');
                                                    }

                                                    if (setProcessedSkillNames.Count > 0)
                                                    {
                                                        sbdFilter.Append(sbdFilter.Length > 0 ? " and not(" : "not(");
                                                        foreach (string strName in setProcessedSkillNames)
                                                        {
                                                            sbdFilter.Append("name = ", strName.CleanXPath(), " or ");
                                                        }

                                                        sbdFilter.Length -= 4;
                                                        sbdFilter.Append(')');
                                                    }

                                                    if (setAllowedLinkedAttributes?.Count > 0)
                                                    {
                                                        sbdFilter.Append(sbdFilter.Length > 0 ? " and (" : "(");
                                                        foreach (string strAttribute in setAllowedLinkedAttributes)
                                                        {
                                                            sbdFilter.Append("attribute = ", strAttribute.CleanXPath(), " or ");
                                                        }

                                                        sbdFilter.Length -= 4;
                                                        sbdFilter.Append(')');
                                                    }

                                                    string strFilter = sbdFilter.Length > 0
                                                        ? sbdFilter.Insert(0, ") and (").ToString()
                                                        : string.Empty;
                                                    foreach (XPathNavigator xmlSkill in (blnSync
                                                                 ? objCharacter
                                                                     // ReSharper disable once MethodHasAsyncOverload
                                                                     .LoadDataXPath("skills.xml", token: token)
                                                                 : await objCharacter
                                                                     .LoadDataXPathAsync("skills.xml", token: token).ConfigureAwait(false))
                                                             .Select(
                                                                 "/chummer/knowledgeskills/skill[(not(hide)"
                                                                 + strFilter + ")]"))
                                                    {
                                                        string strName = xmlSkill
                                                            .SelectSingleNodeAndCacheExpression("name", token)?.Value;
                                                        if (!string.IsNullOrEmpty(strName))
                                                            lstDropdownItems.Add(
                                                                new ListItem(
                                                                    strName,
                                                                    xmlSkill.SelectSingleNodeAndCacheExpression(
                                                                            "translate", token)
                                                                        ?.Value
                                                                    ?? strName));
                                                    }
                                                }
                                            }
                                        }
                                        finally
                                        {
                                            if (blnSync)
                                                objLocker.Dispose();
                                            else
                                                await objLockerAsync.DisposeAsync().ConfigureAwait(false);
                                        }
                                    }

                                    if (lstDropdownItems.Count == 1
                                        && (setAllowedNames == null || !string.IsNullOrWhiteSpace(strPrompt)))
                                    {
                                        strSelectedSkill = lstDropdownItems[0].Value.ToString();
                                    }
                                    else
                                    {
                                        lstDropdownItems.Sort(CompareListItems.CompareNames);

                                        string strDescription = blnSync
                                            // ReSharper disable once MethodHasAsyncOverload
                                            ? LanguageManager.GetString("Title_SelectSkill", token: token)
                                            : await LanguageManager.GetStringAsync("Title_SelectSkill", token: token).ConfigureAwait(false);
                                        bool blnAllowAutoSelect = string.IsNullOrWhiteSpace(strPrompt);
                                        using (ThreadSafeForm<SelectItem> frmPickSkill
                                               = blnSync
                                                   // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                   ? ThreadSafeForm<SelectItem>.Get(() => new SelectItem
                                                   {
                                                       Description = strDescription,
                                                       AllowAutoSelect = blnAllowAutoSelect
                                                   })
                                                   : await ThreadSafeForm<SelectItem>.GetAsync(() => new SelectItem
                                                   {
                                                       AllowAutoSelect = blnAllowAutoSelect
                                                   }, token).ConfigureAwait(false))
                                        {
                                            if (!blnSync)
                                                await frmPickSkill.MyForm.DoThreadSafeAsync(x => x.Description = strDescription, token).ConfigureAwait(false);
                                            if (setAllowedNames != null && string.IsNullOrWhiteSpace(strPrompt))
                                                frmPickSkill.MyForm.SetGeneralItemsMode(lstDropdownItems);
                                            else
                                                frmPickSkill.MyForm.SetDropdownItemsMode(lstDropdownItems);

                                            if ((blnSync
                                                    // ReSharper disable once MethodHasAsyncOverload
                                                    ? frmPickSkill.ShowDialogSafe(objCharacter, token)
                                                    : await frmPickSkill.ShowDialogSafeAsync(objCharacter, token).ConfigureAwait(false)) ==
                                                DialogResult.Cancel)
                                            {
                                                throw new AbortedException();
                                            }

                                            strSelectedSkill = blnSync
                                                ? frmPickSkill.MyForm.SelectedItem
                                                : await frmPickSkill.MyForm.DoThreadSafeFuncAsync(x => x.SelectedItem, token).ConfigureAwait(false);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                string strForcedValue = GetForcedValue(objCharacter);
                if (!string.IsNullOrEmpty(strForcedValue))
                {
                    (bool blnIsExotic, string strExoticName)
                        = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? ExoticSkill.IsExoticSkillNameTuple(objCharacter, strForcedValue, token)
                            : await ExoticSkill.IsExoticSkillNameTupleAsync(objCharacter, strForcedValue, token)
                                .ConfigureAwait(false);
                    string strFilter = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? objCharacter.Settings.BookXPath(token: token)
                        : await (await objCharacter
                                .GetSettingsAsync(token).ConfigureAwait(false)).BookXPathAsync(token: token)
                            .ConfigureAwait(false);
                    if (blnIsExotic)
                    {
                        strFilter = "/chummer/skills/skill[name = "
                                    + strExoticName.CleanXPath() + " and exotic = 'True' and ("
                                    + strFilter + ")]";
                    }
                    else
                    {
                        strFilter = "/chummer/skills/skill[name = "
                                    + strForcedValue.CleanXPath() + " and not(exotic = 'True') and ("
                                    + strFilter + ")]";
                    }

                    XPathNavigator xmlSkillNode =
                        (blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? objCharacter.LoadDataXPath("skills.xml", token: token)
                            : await objCharacter.LoadDataXPathAsync("skills.xml", token: token).ConfigureAwait(false))
                        .SelectSingleNode(strFilter) ??
                        throw new AbortedException();
                    int intMinimumRating = 0;
                    string strMinimumRating = xmlBonusNode.Attributes?["minimumrating"]?.InnerTextViaPool(token);
                    if (!string.IsNullOrWhiteSpace(strMinimumRating))
                        intMinimumRating = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? ValueToInt(objCharacter, strMinimumRating, intRating, token)
                            : await ValueToIntAsync(objCharacter, strMinimumRating, intRating, token)
                                .ConfigureAwait(false);
                    int intMaximumRating = int.MaxValue;
                    string strMaximumRating = xmlBonusNode.Attributes?["maximumrating"]?.InnerTextViaPool(token);
                    if (!string.IsNullOrWhiteSpace(strMaximumRating))
                        intMaximumRating = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? ValueToInt(objCharacter, strMaximumRating, intRating, token)
                            : await ValueToIntAsync(objCharacter, strMaximumRating, intRating, token)
                                .ConfigureAwait(false);
                    if (intMinimumRating != 0 || intMaximumRating != int.MaxValue)
                    {
                        Skill objExistingSkill = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? objCharacter.SkillsSection.GetActiveSkill(strForcedValue, token)
                            : await (await objCharacter.GetSkillsSectionAsync(token).ConfigureAwait(false))
                                .GetActiveSkillAsync(strForcedValue, token).ConfigureAwait(false);
                        if (objExistingSkill == null)
                        {
                            if (intMinimumRating > 0)
                                throw new AbortedException();
                        }
                        else
                        {
                            int intCurrentRating =
                                blnSync
                                    ? objExistingSkill.TotalBaseRating
                                    : await objExistingSkill.GetTotalBaseRatingAsync(token).ConfigureAwait(false);
                            if (intCurrentRating > intMaximumRating || intCurrentRating < intMinimumRating)
                                throw new AbortedException();
                        }
                    }

                    XPathNavigator xmlSkillCategories =
                        xmlBonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("skillcategories", token);
                    if (xmlSkillCategories != null)
                    {
                        string strCategory = xmlSkillNode.SelectSingleNodeAndCacheExpression("category", token)?.Value
                                             ?? string.Empty;
                        if (string.IsNullOrWhiteSpace(strCategory))
                            throw new AbortedException();
                        bool blnAbort = true;
                        foreach (XPathNavigator xmlCategory in xmlSkillCategories.Select("category"))
                        {
                            if (xmlCategory.Value == strCategory)
                            {
                                blnAbort = false;
                                break;
                            }
                        }

                        if (blnAbort)
                            throw new AbortedException();
                    }

                    string strTemp = xmlBonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@skillcategory", token)
                        ?.Value;
                    if (!string.IsNullOrEmpty(strTemp))
                    {
                        string strCategory = xmlSkillNode.SelectSingleNodeAndCacheExpression("category", token)?.Value
                                             ?? string.Empty;
                        if (string.IsNullOrWhiteSpace(strCategory) || !strTemp
                                .SplitNoAlloc(
                                    ',', StringSplitOptions.RemoveEmptyEntries)
                                .Contains(strCategory))
                            throw new AbortedException();
                    }

                    strTemp = xmlBonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@skillgroup", token)?.Value;
                    if (!string.IsNullOrEmpty(strTemp))
                    {
                        string strSkillGroup =
                            xmlSkillNode.SelectSingleNodeAndCacheExpression("skillgroup", token)?.Value
                            ?? string.Empty;
                        if (string.IsNullOrWhiteSpace(strSkillGroup) || !strTemp
                                .SplitNoAlloc(
                                    ',', StringSplitOptions.RemoveEmptyEntries)
                                .Contains(strSkillGroup))
                            throw new AbortedException();
                    }

                    strTemp = xmlBonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@excludecategory", token)
                        ?.Value;
                    if (!string.IsNullOrEmpty(strTemp))
                    {
                        string strCategory = xmlSkillNode.SelectSingleNodeAndCacheExpression("category", token)?.Value
                                             ?? string.Empty;
                        if (!string.IsNullOrWhiteSpace(strCategory) && strTemp
                                .SplitNoAlloc(
                                    ',', StringSplitOptions.RemoveEmptyEntries)
                                .Contains(strCategory))
                            throw new AbortedException();
                    }

                    strTemp = xmlBonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@excludeskillgroup", token)
                        ?.Value;
                    if (!string.IsNullOrEmpty(strTemp))
                    {
                        string strSkillGroup =
                            xmlSkillNode.SelectSingleNodeAndCacheExpression("skillgroup", token)?.Value
                            ?? string.Empty;
                        if (!string.IsNullOrWhiteSpace(strSkillGroup) && strTemp
                                .SplitNoAlloc(
                                    ',', StringSplitOptions.RemoveEmptyEntries)
                                .Contains(strSkillGroup))
                            throw new AbortedException();
                    }

                    strTemp = xmlBonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@limittoskill", token)?.Value;
                    if (!string.IsNullOrEmpty(strTemp) && !strTemp
                            .SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries)
                            .Contains(strForcedValue))
                        throw new AbortedException();
                    strTemp = xmlBonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@excludeskill", token)?.Value;
                    if (!string.IsNullOrEmpty(strTemp) && strTemp
                            .SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries)
                            .Contains(strForcedValue))
                        throw new AbortedException();
                    strTemp = xmlBonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@limittoattribute", token)
                        ?.Value;
                    if (!string.IsNullOrEmpty(strTemp))
                    {
                        string strAttribute = xmlSkillNode.SelectSingleNodeAndCacheExpression("attribute", token)?.Value
                                              ?? string.Empty;
                        if (string.IsNullOrWhiteSpace(strAttribute) || !strTemp
                                .SplitNoAlloc(
                                    ',', StringSplitOptions.RemoveEmptyEntries)
                                .Contains(strAttribute))
                            throw new AbortedException();
                    }

                    strSelectedSkill = strForcedValue;
                }
                else
                {
                    string strDescription;
                    if (blnSync)
                        strDescription = !string.IsNullOrEmpty(strFriendlyName)
                            ? string.Format(GlobalSettings.CultureInfo,
                                // ReSharper disable once MethodHasAsyncOverload
                                LanguageManager.GetString("String_Improvement_SelectSkillNamed", token: token),
                                strFriendlyName)
                            // ReSharper disable once MethodHasAsyncOverload
                            : LanguageManager.GetString("String_Improvement_SelectSkill", token: token);
                    else
                        strDescription = !string.IsNullOrEmpty(strFriendlyName)
                            ? string.Format(GlobalSettings.CultureInfo,
                                await LanguageManager
                                    .GetStringAsync("String_Improvement_SelectSkillNamed", token: token)
                                    .ConfigureAwait(false),
                                strFriendlyName)
                            : await LanguageManager.GetStringAsync("String_Improvement_SelectSkill", token: token)
                                .ConfigureAwait(false);
                    // Display the Select Skill window and record which Skill was selected.
                    using (ThreadSafeForm<SelectSkill> frmPickSkill
                           = blnSync
                               // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                               ? ThreadSafeForm<SelectSkill>.Get(() => new SelectSkill(objCharacter, strFriendlyName)
                               {
                                   Description = strDescription
                               })
                               : await ThreadSafeForm<SelectSkill>.GetAsync(() => new SelectSkill(objCharacter, strFriendlyName), token).ConfigureAwait(false))
                    {
                        if (!blnSync)
                            await frmPickSkill.MyForm.DoThreadSafeAsync(x => x.Description = strDescription, token).ConfigureAwait(false);
                        string strMinimumRating = xmlBonusNode
                                .SelectSingleNodeAndCacheExpressionAsNavigator("@minimumrating", token)?.Value;
                        if (!string.IsNullOrWhiteSpace(strMinimumRating))
                            frmPickSkill.MyForm.MinimumRating = blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? ValueToInt(objCharacter, strMinimumRating, intRating, token)
                                : await ValueToIntAsync(objCharacter, strMinimumRating, intRating, token)
                                    .ConfigureAwait(false);
                        string strMaximumRating = xmlBonusNode
                            .SelectSingleNodeAndCacheExpressionAsNavigator("@maximumrating", token)?.Value;
                        if (!string.IsNullOrWhiteSpace(strMaximumRating))
                            frmPickSkill.MyForm.MaximumRating = blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? ValueToInt(objCharacter, strMaximumRating, intRating, token)
                                : await ValueToIntAsync(objCharacter, strMaximumRating, intRating, token)
                                    .ConfigureAwait(false);

                        XmlElement xmlSkillCategories = xmlBonusNode["skillcategories"];
                        if (xmlSkillCategories != null)
                            frmPickSkill.MyForm.LimitToCategories = xmlSkillCategories;
                        string strTemp = xmlBonusNode
                            .SelectSingleNodeAndCacheExpressionAsNavigator("@skillcategory", token)
                            ?.Value;
                        if (!string.IsNullOrEmpty(strTemp))
                            frmPickSkill.MyForm.OnlyCategory = strTemp;
                        strTemp = xmlBonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@skillgroup", token)
                            ?.Value;
                        if (!string.IsNullOrEmpty(strTemp))
                            frmPickSkill.MyForm.OnlySkillGroup = strTemp;
                        strTemp = xmlBonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@excludecategory", token)
                            ?.Value;
                        if (!string.IsNullOrEmpty(strTemp))
                            frmPickSkill.MyForm.ExcludeCategory = strTemp;
                        strTemp = xmlBonusNode
                            .SelectSingleNodeAndCacheExpressionAsNavigator("@excludeskillgroup", token)
                            ?.Value;
                        if (!string.IsNullOrEmpty(strTemp))
                            frmPickSkill.MyForm.ExcludeSkillGroup = strTemp;
                        strTemp = xmlBonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@limittoskill", token)
                            ?.Value;
                        if (!string.IsNullOrEmpty(strTemp))
                            frmPickSkill.MyForm.LimitToSkill = strTemp;
                        strTemp = xmlBonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@excludeskill", token)
                            ?.Value;
                        if (!string.IsNullOrEmpty(strTemp))
                            frmPickSkill.MyForm.ExcludeSkill = strTemp;
                        strTemp = xmlBonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@limittoattribute", token)
                            ?.Value;
                        if (!string.IsNullOrEmpty(strTemp))
                            frmPickSkill.MyForm.LinkedAttribute = strTemp;

                        if ((blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? frmPickSkill.ShowDialogSafe(objCharacter, token)
                                : await frmPickSkill.ShowDialogSafeAsync(objCharacter, token).ConfigureAwait(false)) ==
                            DialogResult.Cancel)
                        {
                            throw new AbortedException();
                        }

                        strSelectedSkill = frmPickSkill.MyForm.SelectedSkill;
                    }
                }
            }

            return new ValueTuple<string, bool>(strSelectedSkill, blnIsKnowledgeSkill);
        }

        #endregion Helper Methods

        #region Improvement System

        /// <summary>
        /// Create all the Improvements for an XML Node.
        /// </summary>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="objImprovementSource">Type of object that grants these Improvements.</param>
        /// <param name="strSourceName">Name of the item that grants these Improvements.</param>
        /// <param name="nodBonus">bonus XML Node from the source data file.</param>
        /// <param name="intRating">Selected Rating value that is used to replace the Rating string in an Improvement.</param>
        /// <param name="strFriendlyName">Friendly name to show in any dialogue windows that ask for a value.</param>
        /// <param name="blnAddImprovementsToCharacter">If True, adds created improvements to the character. Set to false if all we need is a SelectedValue.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>True if successful</returns>
        public static bool CreateImprovements(Character objCharacter,
                                              Improvement.ImprovementSource objImprovementSource, string strSourceName,
                                              XmlNode nodBonus, int intRating = 1, string strFriendlyName = "",
                                              bool blnAddImprovementsToCharacter = true, CancellationToken token = default)
        {
            return Utils.SafelyRunSynchronously(() => CreateImprovementsCoreAsync(true, objCharacter, objImprovementSource, strSourceName, nodBonus,
                                                    intRating, strFriendlyName, blnAddImprovementsToCharacter, token), token);
        }

        /// <summary>
        /// Create all the Improvements for an XML Node.
        /// </summary>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="objImprovementSource">Type of object that grants these Improvements.</param>
        /// <param name="strSourceName">Name of the item that grants these Improvements.</param>
        /// <param name="nodBonus">bonus XML Node from the source data file.</param>
        /// <param name="intRating">Selected Rating value that is used to replace the Rating string in an Improvement.</param>
        /// <param name="strFriendlyName">Friendly name to show in any dialogue windows that ask for a value.</param>
        /// <param name="blnAddImprovementsToCharacter">If True, adds created improvements to the character. Set to false if all we need is a SelectedValue.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>True if successful</returns>
        public static Task<bool> CreateImprovementsAsync(Character objCharacter,
                                                         Improvement.ImprovementSource objImprovementSource,
                                                         string strSourceName,
                                                         XmlNode nodBonus, int intRating = 1,
                                                         string strFriendlyName = "",
                                                         bool blnAddImprovementsToCharacter = true, CancellationToken token = default)
        {
            return CreateImprovementsCoreAsync(false, objCharacter, objImprovementSource, strSourceName, nodBonus,
                                               intRating, strFriendlyName, blnAddImprovementsToCharacter, token);
        }

        /// <summary>
        /// Create all the Improvements for an XML Node.
        /// Uses flag hack method design outlined here to avoid locking:
        /// https://docs.microsoft.com/en-us/archive/msdn-magazine/2015/july/async-programming-brownfield-async-development
        /// </summary>
        /// <param name="blnSync">Flag for whether method should always use synchronous code or not.</param>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="objImprovementSource">Type of object that grants these Improvements.</param>
        /// <param name="strSourceName">Name of the item that grants these Improvements.</param>
        /// <param name="nodBonus">bonus XML Node from the source data file.</param>
        /// <param name="intRating">Selected Rating value that is used to replace the Rating string in an Improvement.</param>
        /// <param name="strFriendlyName">Friendly name to show in any dialogue windows that ask for a value.</param>
        /// <param name="blnAddImprovementsToCharacter">If True, adds created improvements to the character. Set to false if all we need is a SelectedValue.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>True if successful</returns>
        private static async Task<bool> CreateImprovementsCoreAsync(bool blnSync, Character objCharacter,
                                                                    Improvement.ImprovementSource objImprovementSource,
                                                                    string strSourceName,
                                                                    XmlNode nodBonus, int intRating,
                                                                    string strFriendlyName,
                                                                    bool blnAddImprovementsToCharacter, CancellationToken token = default)
        {
            Log.Debug("CreateImprovements enter");
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdTrace))
            {
                sbdTrace.Append("objImprovementSource = ").AppendLine(objImprovementSource.ToString());
                sbdTrace.Append("strSourceName = ").AppendLine(strSourceName);
                sbdTrace.Append("nodBonus = ").AppendLine(nodBonus?.OuterXmlViaPool(token));
                sbdTrace.Append("intRating = ").AppendLine(intRating.ToString(GlobalSettings.InvariantCultureInfo));
                sbdTrace.Append("strFriendlyName = ").AppendLine(strFriendlyName);

                IDisposable objSyncLocker = null;
                IAsyncDisposable objAsyncLocker = null;
                if (objCharacter != null)
                {
                    if (blnSync)
                        objSyncLocker = objCharacter.LockObject.EnterWriteLock(token);
                    else
                        objAsyncLocker = await objCharacter.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                }
                try
                {
                    token.ThrowIfCancellationRequested();
                    try
                    {
                        if (nodBonus == null)
                            return true;

                        SetSelectedValue(string.Empty, objCharacter);

                        string strForcedValue = GetForcedValue(objCharacter);
                        string strLimitSelection = GetLimitSelection(objCharacter);
                        sbdTrace.Append("strForcedValue = ").AppendLine(strForcedValue);
                        sbdTrace.Append("strLimitSelection = ").AppendLine(strLimitSelection);

                        // If no friendly name was provided, use the one from SourceName.
                        if (string.IsNullOrEmpty(strFriendlyName))
                            strFriendlyName = strSourceName;

                        if (nodBonus.HasChildNodes)
                        {
                            string strUnique = nodBonus.Attributes?["unique"]?.InnerTextViaPool(token) ?? string.Empty;
                            sbdTrace.AppendLine("Has Child Nodes");
                            if (nodBonus["selecttext"] != null)
                            {
                                sbdTrace.AppendLine("selecttext");

                                try
                                {
                                    if (!string.IsNullOrEmpty(strForcedValue))
                                    {
                                        strLimitSelection = strForcedValue;
                                    }
                                    else if (objCharacter != null)
                                    {
                                        ConcurrentStack<string> stkPushText = blnSync
                                            ? objCharacter.PushText
                                            : await objCharacter.GetPushTextAsync(token).ConfigureAwait(false);
                                        if (stkPushText.TryPop(out string strText))
                                        {
                                            strLimitSelection = strText;
                                        }
                                    }

                                    string strSelectedValue = GetSelectedValue(objCharacter);
                                    sbdTrace.Append("strSelectedValue = ").AppendLine(strSelectedValue);
                                    sbdTrace.Append("strLimitSelection = ").AppendLine(strLimitSelection);

                                    if (!string.IsNullOrEmpty(strLimitSelection))
                                    {
                                        SetSelectedValue(strLimitSelection, objCharacter);
                                    }
                                    else if (nodBonus["selecttext"].Attributes.Count == 0)
                                    {
                                        string strSelectText = blnSync
                                            // ReSharper disable once MethodHasAsyncOverload
                                            ? LanguageManager.GetString("String_Improvement_SelectText", token: token)
                                            : await LanguageManager
                                                .GetStringAsync("String_Improvement_SelectText", token: token)
                                                .ConfigureAwait(false);
                                        using (ThreadSafeForm<SelectText> frmPickText
                                               = blnSync
                                                   // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                   ? ThreadSafeForm<SelectText>.Get(() => new SelectText
                                                   {
                                                       Description = string.Format(GlobalSettings.CultureInfo,
                                                           strSelectText,
                                                           strFriendlyName)
                                                   })
                                                   : await ThreadSafeForm<SelectText>.GetAsync(() => new SelectText
                                                   {
                                                       Description = string.Format(GlobalSettings.CultureInfo,
                                                           strSelectText,
                                                           strFriendlyName)
                                                   }, token).ConfigureAwait(false))
                                        {
                                            if ((blnSync
                                                    // ReSharper disable once MethodHasAsyncOverload
                                                    ? frmPickText.ShowDialogSafe(objCharacter, token)
                                                    : await frmPickText.ShowDialogSafeAsync(objCharacter, token)
                                                        .ConfigureAwait(false))
                                                == DialogResult.Cancel)
                                            {
                                                if (blnSync)
                                                    // ReSharper disable once MethodHasAsyncOverload
                                                    Rollback(objCharacter, token);
                                                else
                                                    await RollbackAsync(objCharacter, token).ConfigureAwait(false);
                                                return false;
                                            }

                                            SetSelectedValue(frmPickText.MyForm.SelectedValue, objCharacter);
                                        }
                                    }
                                    else if (objCharacter != null)
                                    {
                                        string strXPath = nodBonus.SelectSingleNodeAndCacheExpressionAsNavigator(
                                                              "selecttext/@xpath", token)?.Value
                                                          ?? string.Empty;
                                        if (string.IsNullOrEmpty(strXPath))
                                        {
                                            if (blnSync)
                                                // ReSharper disable once MethodHasAsyncOverload
                                                Rollback(objCharacter, token);
                                            else
                                                await RollbackAsync(objCharacter, token).ConfigureAwait(false);
                                            return false;
                                        }

                                        string strXmlFile
                                            = nodBonus.SelectSingleNodeAndCacheExpressionAsNavigator(
                                                  "selecttext/@xml", token)?.Value
                                              ?? string.Empty;
                                        XPathNavigator xmlDoc
                                            = blnSync
                                                // ReSharper disable once MethodHasAsyncOverload
                                                ? objCharacter.LoadDataXPath(strXmlFile, token: token)
                                                : await objCharacter.LoadDataXPathAsync(strXmlFile, token: token)
                                                    .ConfigureAwait(false);
                                        using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(
                                                   Utils.ListItemListPool, out List<ListItem> lstItems))
                                        {
                                            //TODO: While this is a safeguard for uniques, preference should be that we're selecting distinct values in the xpath.
                                            //Use XPath2.0 distinct-values operators instead. REQUIRES > .Net 4.6
                                            using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool,
                                                       out HashSet<string> setUsedValues))
                                            {
                                                foreach (XPathNavigator objNode in xmlDoc.Select(strXPath))
                                                {
                                                    // First check if we are using a list of language keys
                                                    string strKey = objNode.Name == "key"
                                                        ? objNode.Value
                                                        : string.Empty;
                                                    if (string.IsNullOrEmpty(strKey))
                                                    {
                                                        string strName
                                                            = objNode.SelectSingleNodeAndCacheExpression(
                                                                  "name", token)?.Value
                                                              ?? string.Empty;
                                                        if (string.IsNullOrWhiteSpace(strName))
                                                        {
                                                            // Assume that if we're not looking at something that has an XML node,
                                                            // we're looking at a direct xpath filter or something that has proper names
                                                            // like the lifemodule storybuilder macros.
                                                            string strValue = objNode.Value;
                                                            if (setUsedValues.Add(strValue))
                                                                lstItems.Add(
                                                                    new ListItem(
                                                                        strValue,
                                                                        blnSync
                                                                            // ReSharper disable once MethodHasAsyncOverload
                                                                            ? objCharacter.TranslateExtra(
                                                                                strValue, strPreferFile: strXmlFile,
                                                                                token: token)
                                                                            : await objCharacter.TranslateExtraAsync(
                                                                                strValue, strPreferFile: strXmlFile,
                                                                                token: token).ConfigureAwait(false)));
                                                        }
                                                        else if (setUsedValues.Add(strName))
                                                        {
                                                            lstItems.Add(
                                                                new ListItem(
                                                                    strName,
                                                                    blnSync
                                                                        // ReSharper disable once MethodHasAsyncOverload
                                                                        ? objCharacter.TranslateExtra(
                                                                            strName, strPreferFile: strXmlFile,
                                                                            token: token)
                                                                        : await objCharacter.TranslateExtraAsync(
                                                                                strName, strPreferFile: strXmlFile,
                                                                                token: token)
                                                                            .ConfigureAwait(false)));
                                                        }
                                                    }
                                                    else
                                                    {
                                                        string strValue
                                                            = blnSync
                                                                // ReSharper disable once MethodHasAsyncOverload
                                                                ? LanguageManager.GetString(
                                                                    strKey, GlobalSettings.DefaultLanguage,
                                                                    token: token)
                                                                : await LanguageManager
                                                                    .GetStringAsync(
                                                                        strKey, GlobalSettings.DefaultLanguage,
                                                                        token: token).ConfigureAwait(false);
                                                        if (setUsedValues.Add(strValue))
                                                        {
                                                            lstItems.Add(
                                                                new ListItem(
                                                                    strValue,
                                                                    blnSync
                                                                        // ReSharper disable once MethodHasAsyncOverload
                                                                        ? LanguageManager.GetString(strKey,
                                                                            token: token)
                                                                        : await LanguageManager
                                                                            .GetStringAsync(strKey, token: token)
                                                                            .ConfigureAwait(false)));
                                                        }
                                                    }
                                                }
                                            }

                                            if (lstItems.Count == 0)
                                            {
                                                if (blnSync)
                                                    // ReSharper disable once MethodHasAsyncOverload
                                                    Rollback(objCharacter, token);
                                                else
                                                    await RollbackAsync(objCharacter, token).ConfigureAwait(false);
                                                return false;
                                            }

                                            string strSelectText = blnSync
                                                // ReSharper disable once MethodHasAsyncOverload
                                                ? LanguageManager.GetString("String_Improvement_SelectText",
                                                    token: token)
                                                : await LanguageManager
                                                    .GetStringAsync("String_Improvement_SelectText", token: token)
                                                    .ConfigureAwait(false);

                                            using (ThreadSafeForm<SelectItem> frmSelect
                                                   = blnSync
                                                       // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                       ? ThreadSafeForm<SelectItem>.Get(() => new SelectItem
                                                       {
                                                           Description = string.Format(GlobalSettings.CultureInfo,
                                                               strSelectText,
                                                               strFriendlyName)
                                                       })
                                                       : await ThreadSafeForm<SelectItem>.GetAsync(() => new SelectItem(), token).ConfigureAwait(false))
                                            {
                                                if (!blnSync)
                                                    await frmSelect.MyForm.DoThreadSafeAsync(x => x.Description = string.Format(GlobalSettings.CultureInfo,
                                                               strSelectText,
                                                               strFriendlyName), token).ConfigureAwait(false);
                                                if (Convert.ToBoolean(
                                                        nodBonus.SelectSingleNodeAndCacheExpressionAsNavigator(
                                                            "selecttext/@allowedit", token)?.Value,
                                                        GlobalSettings.InvariantCultureInfo))
                                                {
                                                    lstItems.Insert(0, ListItem.Blank);
                                                    frmSelect.MyForm.SetDropdownItemsMode(lstItems);
                                                }
                                                else
                                                {
                                                    frmSelect.MyForm.SetGeneralItemsMode(lstItems);
                                                }

                                                DialogResult eReturn = blnSync
                                                    // ReSharper disable once MethodHasAsyncOverload
                                                    ? frmSelect.ShowDialogSafe(objCharacter, token)
                                                    : await frmSelect.ShowDialogSafeAsync(objCharacter, token)
                                                        .ConfigureAwait(false);
                                                if (eReturn == DialogResult.Cancel)
                                                {
                                                    if (blnSync)
                                                        // ReSharper disable once MethodHasAsyncOverload
                                                        Rollback(objCharacter, token);
                                                    else
                                                        await RollbackAsync(objCharacter, token).ConfigureAwait(false);
                                                    return false;
                                                }

                                                SetSelectedValue(blnSync ? frmSelect.MyForm.SelectedItem : await frmSelect.MyForm.DoThreadSafeFuncAsync(x => x.SelectedItem, token).ConfigureAwait(false), objCharacter);
                                            }
                                        }
                                    }

                                    strSelectedValue = GetSelectedValue(objCharacter);
                                    sbdTrace.Append("SelectedValue = ").AppendLine(strSelectedValue);
                                    sbdTrace.Append("strSourceName = ").AppendLine(strSourceName);

                                    if (blnAddImprovementsToCharacter)
                                    {
                                        // Create the Improvement.
                                        sbdTrace.AppendLine("Calling CreateImprovement");
                                        if (blnSync)
                                            // ReSharper disable once MethodHasAsyncOverload
                                            CreateImprovement(objCharacter, strSelectedValue, objImprovementSource,
                                                strSourceName,
                                                Improvement.ImprovementType.Text,
                                                strUnique, token: token);
                                        else
                                            await CreateImprovementAsync(objCharacter, strSelectedValue,
                                                objImprovementSource,
                                                strSourceName,
                                                Improvement.ImprovementType.Text,
                                                strUnique, token: token).ConfigureAwait(false);
                                    }
                                }
                                catch
                                {
                                    if (blnSync)
                                        // ReSharper disable once MethodHasAsyncOverload
                                        Rollback(objCharacter, CancellationToken.None);
                                    else
                                        await RollbackAsync(objCharacter, CancellationToken.None).ConfigureAwait(false);
                                    throw;
                                }
                            }

                            // If there is no character object, don't attempt to add any Improvements.
                            if (objCharacter == null && blnAddImprovementsToCharacter)
                            {
                                sbdTrace.AppendLine("_objCharacter = Null");
                                return true;
                            }

                            // Check to see what bonuses the node grants.
                            if (blnSync)
                            {
                                try
                                {
                                    foreach (XmlNode bonusNode in nodBonus.ChildNodes)
                                    {
                                        if (ProcessBonus(objCharacter, objImprovementSource, ref strSourceName,
                                                intRating,
                                                strFriendlyName,
                                                bonusNode, strUnique, !blnAddImprovementsToCharacter, token))
                                            continue;
                                        // ReSharper disable once MethodHasAsyncOverload
                                        Rollback(objCharacter, token);
                                        sbdTrace.AppendLine("Bonus processing unsuccessful, returning.");
                                        return false;
                                    }
                                }
#if DEBUG
                                catch (Exception e)
#else
                                catch
#endif
                                {
                                    Utils.BreakIfDebug();
                                    // ReSharper disable once MethodHasAsyncOverload
                                    Rollback(objCharacter, token);
                                    throw;
                                }
                            }
                            else
                            {
                                try
                                {
                                    foreach (XmlNode bonusNode in nodBonus.ChildNodes)
                                    {
                                        bool blnSuccess;
                                        (blnSuccess, strSourceName) = await ProcessBonusAsync(
                                                objCharacter, objImprovementSource, strSourceName, intRating,
                                                strFriendlyName,
                                                bonusNode, strUnique, !blnAddImprovementsToCharacter, token)
                                            .ConfigureAwait(false);
                                        if (blnSuccess)
                                            continue;
                                        await RollbackAsync(objCharacter, token).ConfigureAwait(false);
                                        sbdTrace.AppendLine("Bonus processing unsuccessful, returning.");
                                        return false;
                                    }
                                }
#if DEBUG
                                catch (Exception e)
#else
                                catch
#endif
                                {
                                    Utils.BreakIfDebug();
                                    await RollbackAsync(objCharacter, token).ConfigureAwait(false);
                                    throw;
                                }
                            }
                        }
                        // If there is no character object, don't attempt to add any Improvements.
                        else if (objCharacter == null && blnAddImprovementsToCharacter)
                        {
                            sbdTrace.AppendLine("_objCharacter = Null");
                            return true;
                        }

                        // If we've made it this far, everything went OK, so commit the Improvements.

                        if (blnAddImprovementsToCharacter)
                        {
                            sbdTrace.AppendLine("Committing improvements.");
                            if (blnSync)
                                // ReSharper disable once MethodHasAsyncOverload
                                Commit(objCharacter, token);
                            else
                                await CommitAsync(objCharacter, token).ConfigureAwait(false);
                            sbdTrace.AppendLine("Finished committing improvements");
                        }
                        else
                        {
                            sbdTrace.AppendLine(
                                "Calling scheduled Rollback due to blnAddImprovementsToCharacter = false");
                            if (blnSync)
                                // ReSharper disable once MethodHasAsyncOverload
                                Rollback(objCharacter, token);
                            else
                                await RollbackAsync(objCharacter, token).ConfigureAwait(false);
                            sbdTrace.AppendLine("Returned from scheduled Rollback");
                        }

                        // If the bonus should not bubble up SelectedValues from its improvements, reset it to empty.
                        if (nodBonus.Attributes?["useselected"]?.InnerTextIsFalseString() == true)
                        {
                            SetSelectedValue(string.Empty, objCharacter);
                        }
                    }
                    finally
                    {
                        // Clear the Forced Value and Limit Selection strings once we're done to prevent these from forcing their values on other Improvements.
                        SetForcedValue(string.Empty, objCharacter);
                        SetLimitSelection(string.Empty, objCharacter);
                    }
                }
                //catch (Exception ex)
                //{
                //    objFunctions.LogWrite(CommonFunctions.LogType.Error, "Chummer.ImprovementManager", "ERROR Message = " + ex.Message);
                //    objFunctions.LogWrite(CommonFunctions.LogType.Error, "Chummer.ImprovementManager", "ERROR Source  = " + ex.Source);
                //    objFunctions.LogWrite(CommonFunctions.LogType.Error, "Chummer.ImprovementManager",
                //        "ERROR Trace   = " + ex.StackTrace.ToString());

                //    Rollback();
                //    throw;
                //}
                finally
                {
                    objSyncLocker?.Dispose();
                    if (objAsyncLocker != null)
                        await objAsyncLocker.DisposeAsync().ConfigureAwait(false);
                    Log.Trace(sbdTrace.ToString);
                    Log.Debug("CreateImprovements exit");
                }
            }

            return true;
        }

        private static bool ProcessBonus(Character objCharacter, Improvement.ImprovementSource objImprovementSource,
                                         ref string strSourceName,
                                         int intRating, string strFriendlyName, XmlNode bonusNode, string strUnique,
                                         bool blnIgnoreMethodNotFound = false, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (bonusNode == null)
                return false;
            //As this became a really big nest of **** that it searched past, several places having equal paths just adding a different improvement, a more flexible method was chosen.
            //So far it is just a slower Dictionary<string, Action> but should (in theory...) be able to leverage this in the future to do it smarter with methods that are the same but
            //getting a different parameter injected

            AddImprovementCollection container = new AddImprovementCollection(objCharacter, objImprovementSource,
                                                                              strSourceName, strUnique, GetForcedValue(objCharacter),
                                                                              GetLimitSelection(objCharacter), GetSelectedValue(objCharacter),
                                                                              strFriendlyName,
                                                                              intRating);

            Action<XmlNode> objImprovementMethod
                = ImprovementMethods.GetMethod(bonusNode.Name.ToUpperInvariant(), container);
            if (objImprovementMethod != null)
            {
                try
                {
                    using (objCharacter.LockObject.EnterWriteLock(token))
                        objImprovementMethod.Invoke(bonusNode);
                }
                catch (AbortedException)
                {
                    Rollback(objCharacter, token);
                    return false;
                }

                strSourceName = container.SourceName;
                SetForcedValue(container.ForcedValue, objCharacter);
                SetLimitSelection(container.LimitSelection, objCharacter);
                SetSelectedValue(container.SelectedValue, objCharacter);
            }
            else if (!blnIgnoreMethodNotFound && bonusNode.ChildNodes.Count > 0 && bonusNode.NodeType != XmlNodeType.Comment)
            {
                Utils.BreakIfDebug();
                Log.Warn(new object[] { "Tried to get unknown bonus", bonusNode.OuterXmlViaPool(token) });
                return false;
            }

            return true;
        }

        private static async Task<ValueTuple<bool, string>> ProcessBonusAsync(Character objCharacter, Improvement.ImprovementSource objImprovementSource,
                                         string strSourceName,
                                         int intRating, string strFriendlyName, XmlNode bonusNode, string strUnique,
                                         bool blnIgnoreMethodNotFound = false, CancellationToken token = default)
        {
            if (bonusNode == null)
                return new ValueTuple<bool, string>(false, strSourceName);
            //As this became a really big nest of **** that it searched past, several places having equal paths just adding a different improvement, a more flexible method was chosen.
            //So far it is just a slower Dictionary<string, Action> but should (in theory...) be able to leverage this in the future to do it smarter with methods that are the same but
            //getting a different parameter injected

            AddImprovementAsyncCollection container = new AddImprovementAsyncCollection(objCharacter,
                objImprovementSource,
                strSourceName, strUnique, GetForcedValue(objCharacter),
                GetLimitSelection(objCharacter), GetSelectedValue(objCharacter),
                strFriendlyName,
                intRating);

            Func<XmlNode, CancellationToken, Task> objImprovementMethod
                = ImprovementMethods.GetAsyncMethod(bonusNode.Name.ToUpperInvariant(), container);
            if (objImprovementMethod != null)
            {
                try
                {
                    IAsyncDisposable objLocker = await objCharacter.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        await objImprovementMethod.Invoke(bonusNode, token).ConfigureAwait(false);
                    }
                    finally
                    {
                        await objLocker.DisposeAsync().ConfigureAwait(false);
                    }
                }
                catch (AbortedException)
                {
                    await RollbackAsync(objCharacter, token).ConfigureAwait(false);
                    return new ValueTuple<bool, string>(false, strSourceName);
                }

                strSourceName = container.SourceName;
                SetForcedValue(container.ForcedValue, objCharacter);
                SetLimitSelection(container.LimitSelection, objCharacter);
                SetSelectedValue(container.SelectedValue, objCharacter);
            }
            else if (!blnIgnoreMethodNotFound && bonusNode.ChildNodes.Count > 0 && bonusNode.NodeType != XmlNodeType.Comment)
            {
                Utils.BreakIfDebug();
                Log.Warn(new object[] { "Tried to get unknown bonus", bonusNode.OuterXmlViaPool(token) });
                return new ValueTuple<bool, string>(false, strSourceName);
            }

            return new ValueTuple<bool, string>(true, strSourceName);
        }

        public static void EnableImprovements(Character objCharacter, IEnumerable<Improvement> objImprovementList, CancellationToken token = default)
        {
            EnableImprovements(objCharacter, objImprovementList.ToList(), token);
        }

        public static void EnableImprovements(Character objCharacter, Improvement objImprovement, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            EnableImprovements(objCharacter, objImprovement.Yield(), token);
        }

        public static void EnableImprovements(Character objCharacter, params Improvement[] objImprovementList)
        {
            EnableImprovements(objCharacter, Array.AsReadOnly(objImprovementList));
        }

        public static void EnableImprovements(Character objCharacter, IReadOnlyCollection<Improvement> objImprovementList, CancellationToken token = default)
        {
            Utils.SafelyRunSynchronously(() => EnableImprovementsCoreAsync(true, objCharacter, objImprovementList, token), token);
        }

        public static Task EnableImprovementsAsync(Character objCharacter, IEnumerable<Improvement> objImprovementList, CancellationToken token = default)
        {
            return EnableImprovementsAsync(objCharacter, objImprovementList.ToList(), token);
        }

        public static Task EnableImprovementsAsync(Character objCharacter, Improvement objImprovement, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return EnableImprovementsAsync(objCharacter, objImprovement.Yield(), token);
        }

        public static Task EnableImprovementsAsync(Character objCharacter, params Improvement[] objImprovementList)
        {
            return EnableImprovementsAsync(objCharacter, Array.AsReadOnly(objImprovementList));
        }

        public static Task EnableImprovementsAsync(Character objCharacter, IReadOnlyCollection<Improvement> objImprovementList, CancellationToken token = default)
        {
            return EnableImprovementsCoreAsync(false, objCharacter, objImprovementList, token);
        }

        public static async Task EnableImprovementsCoreAsync(bool blnSync, Character objCharacter,
                                                             IReadOnlyCollection<Improvement> objImprovementList,
                                                             CancellationToken token = default)
        {
            if (objCharacter == null)
                throw new ArgumentNullException(nameof(objCharacter));
            if (objImprovementList == null)
                throw new ArgumentNullException(nameof(objImprovementList));

            IAsyncDisposable objLocker = await objCharacter.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                foreach (Improvement objImprovement in objImprovementList)
                {
                    // Enable the Improvement.
                    if (blnSync)
                        objImprovement.Enabled = true;
                    else
                        await objImprovement.SetEnabledAsync(true, token).ConfigureAwait(false);
                }

                bool blnCharacterHasSkillsoftAccess
                    = (blnSync
                          // ReSharper disable once MethodHasAsyncOverload
                          ? ValueOf(objCharacter, Improvement.ImprovementType.SkillsoftAccess, token: token)
                          : await ValueOfAsync(objCharacter, Improvement.ImprovementType.SkillsoftAccess, token: token)
                              .ConfigureAwait(false))
                      > 0;
                // Now that the entire list is deleted from the character's improvements list, we do the checking of duplicates and extra effects
                foreach (Improvement objImprovement in objImprovementList)
                {
                    string strImprovedName = objImprovement.ImprovedName;
                    string strUniqueName = objImprovement.UniqueName;
                    switch (objImprovement.ImproveType)
                    {
                        case Improvement.ImprovementType.SkillLevel:
                            //TODO: Come back here and figure out wtf this did? Think it removed nested lifemodule skills? //Didn't this handle the collapsing knowledge skills thing?
                            //for (int i = _objCharacter.SkillsSection.Skills.Count - 1; i >= 0; i--)
                            //{
                            //    //wrote as foreach first, modify collection, not want rename
                            //    Skill skill = _objCharacter.SkillsSection.Skills[i];
                            //    for (int j = skill.Fold.Count - 1; j >= 0; j--)
                            //    {
                            //        Skill fold = skill.Fold[i];
                            //        if (fold.Id.ToString() == strImprovedName)
                            //        {
                            //            skill.Free(fold);
                            //            _objCharacter.SkillsSection.Skills.Remove(fold);
                            //        }
                            //    }

                            //    if (skill.Id.ToString() == strImprovedName)
                            //    {
                            //        while(skill.Fold.Count > 0) skill.Free(skill.Fold[0]);
                            //        //empty list, can't call clear as exposed list is RO

                            //        _objCharacter.SkillsSection.Skills.Remove(skill);
                            //    }
                            //}
                            break;

                        case Improvement.ImprovementType.SkillsoftAccess:
                            if (blnSync)
                            {
                                // ReSharper disable once MethodHasAsyncOverload
                                objCharacter.SkillsSection.KnowsoftSkills.ForEach(objKnowledgeSkill =>
                                {
                                    if (!objCharacter.SkillsSection.KnowledgeSkills.Contains(objKnowledgeSkill))
                                        objCharacter.SkillsSection.KnowledgeSkills.Add(objKnowledgeSkill);
                                }, token);
                            }
                            else
                            {
                                await objCharacter.SkillsSection.KnowsoftSkills.ForEachAsync(async objKnowledgeSkill =>
                                {
                                    if (!await objCharacter.SkillsSection.KnowledgeSkills
                                                           .ContainsAsync(objKnowledgeSkill, token)
                                                           .ConfigureAwait(false))
                                        await objCharacter.SkillsSection.KnowledgeSkills
                                                          .AddAsync(objKnowledgeSkill, token).ConfigureAwait(false);
                                }, token).ConfigureAwait(false);
                            }

                            break;

                        case Improvement.ImprovementType.Skillsoft:
                        {
                            if (blnCharacterHasSkillsoftAccess)
                            {
                                if (blnSync)
                                {
                                    // ReSharper disable once MethodHasAsyncOverload
                                    objCharacter.SkillsSection.KnowsoftSkills.ForEach(objKnowledgeSkill =>
                                    {
                                        if (objKnowledgeSkill.InternalId == strImprovedName
                                            && !objCharacter.SkillsSection.KnowledgeSkills.Contains(objKnowledgeSkill))
                                        {
                                            objCharacter.SkillsSection.KnowledgeSkills.Add(objKnowledgeSkill);
                                        }
                                    }, token);
                                }
                                else
                                {
                                    await objCharacter.SkillsSection.KnowsoftSkills.ForEachAsync(
                                        async objKnowledgeSkill =>
                                        {
                                            if (objKnowledgeSkill.InternalId == strImprovedName
                                                && !await objCharacter.SkillsSection.KnowledgeSkills
                                                                      .ContainsAsync(objKnowledgeSkill, token)
                                                                      .ConfigureAwait(false))
                                            {
                                                await objCharacter.SkillsSection.KnowledgeSkills
                                                                  .AddAsync(objKnowledgeSkill, token)
                                                                  .ConfigureAwait(false);
                                            }
                                        }, token).ConfigureAwait(false);
                                }
                            }
                        }
                            break;

                        case Improvement.ImprovementType.Attribute:
                            // Determine if access to any Special Attributes have been lost.
                            if (string.Equals(strUniqueName, "enableattribute", StringComparison.OrdinalIgnoreCase))
                            {
                                switch (strImprovedName.ToUpperInvariant())
                                {
                                    case "MAG":
                                        if (blnSync)
                                            objCharacter.MAGEnabled = true;
                                        else
                                            await objCharacter.SetMAGEnabledAsync(true, token).ConfigureAwait(false);
                                        break;

                                    case "RES":
                                        if (blnSync)
                                            objCharacter.RESEnabled = true;
                                        else
                                            await objCharacter.SetRESEnabledAsync(true, token).ConfigureAwait(false);
                                        break;

                                    case "DEP":
                                        if (blnSync)
                                            objCharacter.DEPEnabled = true;
                                        else
                                            await objCharacter.SetDEPEnabledAsync(true, token).ConfigureAwait(false);
                                        break;
                                }
                            }

                            break;

                        case Improvement.ImprovementType.SpecialTab:
                            // Determine if access to any special tabs have been lost.
                            switch (strUniqueName.ToUpperInvariant())
                            {
                                case "ENABLETAB":
                                    switch (strImprovedName.ToUpperInvariant())
                                    {
                                        case "MAGICIAN":
                                            if (blnSync)
                                                objCharacter.MagicianEnabled = true;
                                            else
                                                await objCharacter.SetMagicianEnabledAsync(true, token).ConfigureAwait(false);
                                            break;

                                        case "ADEPT":
                                            if (blnSync)
                                                objCharacter.AdeptEnabled = true;
                                            else
                                                await objCharacter.SetAdeptEnabledAsync(true, token).ConfigureAwait(false);
                                            break;

                                        case "TECHNOMANCER":
                                            if (blnSync)
                                                objCharacter.TechnomancerEnabled = true;
                                            else
                                                await objCharacter.SetTechnomancerEnabledAsync(true, token).ConfigureAwait(false);
                                            break;

                                        case "ADVANCED PROGRAMS":
                                            if (blnSync)
                                                objCharacter.AdvancedProgramsEnabled = true;
                                            else
                                                await objCharacter.SetAdvancedProgramsEnabledAsync(true, token).ConfigureAwait(false);
                                            break;

                                        case "CRITTER":
                                            if (blnSync)
                                                objCharacter.CritterEnabled = true;
                                            else
                                                await objCharacter.SetCritterEnabledAsync(true, token).ConfigureAwait(false);
                                            break;
                                    }

                                    break;
                                // Determine if access to any special tabs has been regained
                                case "DISABLETAB":
                                    switch (strImprovedName)
                                    {
                                        case "CYBERWARE":
                                            if (blnSync)
                                                objCharacter.CyberwareDisabled = true;
                                            else
                                                await objCharacter.SetCyberwareDisabledAsync(true, token).ConfigureAwait(false);
                                            break;

                                        case "INITIATION":
                                            if (blnSync)
                                                objCharacter.InitiationForceDisabled = true;
                                            else
                                                await objCharacter.SetInitiationForceDisabledAsync(true, token).ConfigureAwait(false);
                                            break;
                                    }

                                    break;
                            }

                            break;

                        case Improvement.ImprovementType.PrototypeTranshuman:
                            // Legacy compatibility
                            if (string.IsNullOrEmpty(strImprovedName))
                            {
                                if (blnSync)
                                    objCharacter.PrototypeTranshuman = 1;
                                else
                                    await objCharacter.SetPrototypeTranshumanAsync(1, token).ConfigureAwait(false);
                            }
                            else if (blnSync)
                            {
                                // ReSharper disable once MethodHasAsyncOverload
                                decimal decValue = ValueToDec(objCharacter, strImprovedName, objImprovement.Rating, token);
                                objCharacter.PrototypeTranshuman += decValue;
                            }
                            else
                            {
                                decimal decValue = await ValueToDecAsync(objCharacter, strImprovedName, objImprovement.Rating, token).ConfigureAwait(false);
                                await objCharacter.ModifyPrototypeTranshumanAsync(decValue, token).ConfigureAwait(false);
                            }
                            break;

                        case Improvement.ImprovementType.AddContact:
                            Contact objNewContact
                                = blnSync
                                    ? objCharacter.Contacts.FirstOrDefault(c => c.UniqueId == strImprovedName)
                                    : await objCharacter.Contacts.FirstOrDefaultAsync(
                                        c => c.UniqueId == strImprovedName, token).ConfigureAwait(false);
                            if (objNewContact != null)
                            {
                                // TODO: Add code to disable contact
                            }

                            break;

                        case Improvement.ImprovementType.Art:
                            Art objArt = blnSync
                                ? objCharacter.Arts.FirstOrDefault(x => x.InternalId == strImprovedName)
                                : await objCharacter.Arts.FirstOrDefaultAsync(
                                    x => x.InternalId == strImprovedName, token).ConfigureAwait(false);
                            if (objArt != null)
                            {
                                Improvement.ImprovementSource eSource = objArt.SourceType;
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverload
                                    EnableImprovements(objCharacter,
                                                       objCharacter.Improvements.Where(
                                                           x => x.ImproveSource == eSource
                                                                && x.SourceName == strImprovedName && x.Enabled),
                                                       token);
                                else
                                    await EnableImprovementsAsync(objCharacter,
                                                                  await objCharacter.Improvements.ToListAsync(
                                                                          x => x.ImproveSource == eSource
                                                                               && x.SourceName == strImprovedName
                                                                               && x.Enabled, token: token)
                                                                      .ConfigureAwait(false), token)
                                        .ConfigureAwait(false);
                            }

                            break;

                        case Improvement.ImprovementType.Metamagic:
                        case Improvement.ImprovementType.Echo:
                            Metamagic objMetamagic = blnSync
                                ? objCharacter.Metamagics.FirstOrDefault(o => o.InternalId == strImprovedName)
                                : await objCharacter.Metamagics.FirstOrDefaultAsync(
                                    o => o.InternalId == strImprovedName, token).ConfigureAwait(false);
                            if (objMetamagic != null)
                            {
                                Improvement.ImprovementSource eSource
                                    = objImprovement.ImproveType == Improvement.ImprovementType.Metamagic
                                        ? Improvement.ImprovementSource.Metamagic
                                        : Improvement.ImprovementSource.Echo;
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverload
                                    EnableImprovements(objCharacter,
                                                       objCharacter.Improvements.Where(
                                                           x => x.ImproveSource == eSource
                                                                && x.SourceName == strImprovedName && x.Enabled),
                                                       token);
                                else
                                    await EnableImprovementsAsync(objCharacter,
                                                                  await objCharacter.Improvements.ToListAsync(
                                                                          x => x.ImproveSource == eSource
                                                                               && x.SourceName == strImprovedName
                                                                               && x.Enabled, token: token)
                                                                      .ConfigureAwait(false), token)
                                        .ConfigureAwait(false);
                            }

                            break;

                        case Improvement.ImprovementType.CritterPower:
                            CritterPower objCritterPower = blnSync
                                ? objCharacter.CritterPowers.FirstOrDefault(
                                    x => x.InternalId == strImprovedName
                                         || (x.Name == strImprovedName && x.Extra == strUniqueName))
                                : await objCharacter.CritterPowers.FirstOrDefaultAsync(
                                                        x => x.InternalId == strImprovedName
                                                             || (x.Name == strImprovedName && x.Extra == strUniqueName),
                                                        token)
                                                    .ConfigureAwait(false);
                            if (objCritterPower != null)
                            {
                                string strPowerId = objCritterPower.InternalId;
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverload
                                    EnableImprovements(objCharacter,
                                                       objCharacter.Improvements.Where(
                                                           x => x.ImproveSource
                                                                == Improvement.ImprovementSource.CritterPower
                                                                && x.SourceName == strPowerId && x.Enabled), token);
                                else
                                    await EnableImprovementsAsync(objCharacter,
                                                                  await objCharacter.Improvements.ToListAsync(
                                                                          x => x.ImproveSource
                                                                               == Improvement.ImprovementSource
                                                                                   .CritterPower
                                                                               && x.SourceName == strPowerId
                                                                               && x.Enabled, token: token)
                                                                      .ConfigureAwait(false), token)
                                        .ConfigureAwait(false);
                            }

                            break;

                        case Improvement.ImprovementType.MentorSpirit:
                        case Improvement.ImprovementType.Paragon:
                            MentorSpirit objMentor = blnSync
                                ? objCharacter.MentorSpirits.FirstOrDefault(o => o.InternalId == strImprovedName)
                                : await objCharacter.MentorSpirits.FirstOrDefaultAsync(
                                    o => o.InternalId == strImprovedName, token).ConfigureAwait(false);
                            if (objMentor != null)
                            {
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverload
                                    EnableImprovements(objCharacter,
                                                       objCharacter.Improvements.Where(
                                                           x => x.ImproveSource
                                                                == Improvement.ImprovementSource.MentorSpirit
                                                                && x.SourceName == strImprovedName && x.Enabled),
                                                       token);
                                else
                                    await EnableImprovementsAsync(objCharacter,
                                                                  await objCharacter.Improvements.ToListAsync(
                                                                          x => x.ImproveSource
                                                                               == Improvement.ImprovementSource
                                                                                   .MentorSpirit
                                                                               && x.SourceName == strImprovedName
                                                                               && x.Enabled, token: token)
                                                                      .ConfigureAwait(false), token)
                                        .ConfigureAwait(false);
                            }

                            break;

                        case Improvement.ImprovementType.Gear:
                            Gear objGear = blnSync
                                ? objCharacter.Gear.FirstOrDefault(o => o.InternalId == strImprovedName)
                                : await objCharacter.Gear.FirstOrDefaultAsync(
                                    o => o.InternalId == strImprovedName, token).ConfigureAwait(false);
                            if (objGear != null)
                            {
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    objGear.ChangeEquippedStatus(true);
                                else
                                    await objGear.ChangeEquippedStatusAsync(true, token: token).ConfigureAwait(false);
                            }

                            break;

                        case Improvement.ImprovementType.Weapon:
                            Weapon objWeapon
                                = blnSync
                                    ? objCharacter.Weapons.DeepFirstOrDefault(x => x.Children,
                                          x => x.InternalId == strImprovedName, token)
                                      ??
                                      objCharacter.Vehicles.FindVehicleWeapon(strImprovedName, token)
                                    : await objCharacter.Weapons.DeepFirstOrDefaultAsync(x => x.Children,
                                          x => x.InternalId == strImprovedName, token: token).ConfigureAwait(false)
                                      ?? (await objCharacter.Vehicles.FindVehicleWeaponAsync(strImprovedName, token)
                                          .ConfigureAwait(false))
                                      .Item1;
                            if (objWeapon != null)
                            {
                                if (blnSync)
                                    objWeapon.Equipped = true;
                                else
                                    await objWeapon.SetEquippedAsync(true, token).ConfigureAwait(false);
                            }

                            break;

                        case Improvement.ImprovementType.Spell:
                            Spell objSpell = blnSync
                                ? objCharacter.Spells.FirstOrDefault(o => o.InternalId == strImprovedName)
                                : await objCharacter.Spells.FirstOrDefaultAsync(
                                    o => o.InternalId == strImprovedName, token).ConfigureAwait(false);
                            if (objSpell != null)
                            {
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverload
                                    EnableImprovements(objCharacter,
                                                       objCharacter.Improvements.Where(
                                                           x => x.ImproveSource == Improvement.ImprovementSource.Spell
                                                                && x.SourceName == strImprovedName && x.Enabled),
                                                       token);
                                else
                                    await EnableImprovementsAsync(objCharacter,
                                                                  await objCharacter.Improvements.ToListAsync(
                                                                          x => x.ImproveSource
                                                                               == Improvement.ImprovementSource.Spell
                                                                               && x.SourceName == strImprovedName
                                                                               && x.Enabled, token: token)
                                                                      .ConfigureAwait(false), token)
                                        .ConfigureAwait(false);
                            }

                            break;

                        case Improvement.ImprovementType.ComplexForm:
                            ComplexForm objComplexForm = blnSync
                                ? objCharacter.ComplexForms.FirstOrDefault(o => o.InternalId == strImprovedName)
                                : await objCharacter.ComplexForms.FirstOrDefaultAsync(
                                    o => o.InternalId == strImprovedName, token).ConfigureAwait(false);
                            if (objComplexForm != null)
                            {
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverload
                                    EnableImprovements(objCharacter,
                                                       objCharacter.Improvements.Where(
                                                           x => x.ImproveSource == Improvement.ImprovementSource
                                                                    .ComplexForm
                                                                && x.SourceName == strImprovedName && x.Enabled),
                                                       token);
                                else
                                    await EnableImprovementsAsync(objCharacter,
                                                                  await objCharacter.Improvements.ToListAsync(
                                                                          x => x.ImproveSource
                                                                               == Improvement.ImprovementSource
                                                                                   .ComplexForm
                                                                               && x.SourceName == strImprovedName
                                                                               && x.Enabled, token: token)
                                                                      .ConfigureAwait(false), token)
                                        .ConfigureAwait(false);
                            }

                            break;

                        case Improvement.ImprovementType.MartialArt:
                            MartialArt objMartialArt = blnSync
                                ? objCharacter.MartialArts.FirstOrDefault(o => o.InternalId == strImprovedName)
                                : await objCharacter.MartialArts.FirstOrDefaultAsync(
                                    o => o.InternalId == strImprovedName, token).ConfigureAwait(false);
                            if (objMartialArt != null)
                            {
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverload
                                    EnableImprovements(objCharacter,
                                                       objCharacter.Improvements.Where(
                                                           x => x.ImproveSource == Improvement.ImprovementSource
                                                                    .MartialArt
                                                                && x.SourceName == strImprovedName && x.Enabled),
                                                       token);
                                else
                                    await EnableImprovementsAsync(objCharacter,
                                                                  await objCharacter.Improvements.ToListAsync(
                                                                          x => x.ImproveSource
                                                                               == Improvement.ImprovementSource
                                                                                   .MartialArt
                                                                               && x.SourceName == strImprovedName
                                                                               && x.Enabled, token: token)
                                                                      .ConfigureAwait(false), token)
                                        .ConfigureAwait(false);
                                // Remove the Improvements for any Techniques for the Martial Art that is being removed.
                                if (blnSync)
                                {
                                    // ReSharper disable once MethodHasAsyncOverload
                                    objMartialArt.Techniques.ForEach(objTechnique =>
                                    {
                                        string strTechniqueId = objTechnique.InternalId;
                                        EnableImprovements(objCharacter,
                                                           objCharacter.Improvements.Where(
                                                               x => x.ImproveSource == Improvement.ImprovementSource
                                                                        .MartialArtTechnique
                                                                    && x.SourceName == strTechniqueId && x.Enabled),
                                                           token);
                                    }, token: token);
                                }
                                else
                                {
                                    await objMartialArt.Techniques.ForEachAsync(async objTechnique =>
                                    {
                                        string strTechniqueId = objTechnique.InternalId;
                                        await EnableImprovementsAsync(objCharacter,
                                                                      await objCharacter.Improvements.ToListAsync(
                                                                              x => x.ImproveSource
                                                                                  == Improvement.ImprovementSource
                                                                                      .MartialArtTechnique
                                                                                  && x.SourceName == strTechniqueId
                                                                                  && x.Enabled, token: token)
                                                                          .ConfigureAwait(false), token)
                                            .ConfigureAwait(false);
                                    }, token: token).ConfigureAwait(false);
                                }
                            }

                            break;

                        case Improvement.ImprovementType.SpecialSkills:
                        {
                            SkillsSection.FilterOption eFilterOption
                                = (SkillsSection.FilterOption) Enum.Parse(
                                    typeof(SkillsSection.FilterOption), strImprovedName);
                            if (blnSync)
                            {
                                foreach (Skill objSkill in objCharacter.SkillsSection.FetchExistingSkillsByFilter(eFilterOption, objImprovement.Target, true, token))
                                {
                                    objSkill.ForceDisabled = false;
                                }
                            }
                            else
                            {
                                foreach (Skill objSkill in await objCharacter.SkillsSection.FetchExistingSkillsByFilterAsync(eFilterOption, objImprovement.Target, token).ConfigureAwait(false))
                                {
                                    await objSkill.SetForceDisabledAsync(false, token).ConfigureAwait(false);
                                }
                            }
                        }
                            break;

                        case Improvement.ImprovementType.SpecificQuality:
                            Quality objQuality = blnSync
                                ? objCharacter.Qualities.FirstOrDefault(o => o.InternalId == strImprovedName)
                                : await objCharacter.Qualities.FirstOrDefaultAsync(
                                    o => o.InternalId == strImprovedName, token).ConfigureAwait(false);
                            if (objQuality != null)
                            {
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverload
                                    EnableImprovements(objCharacter,
                                                       objCharacter.Improvements.Where(
                                                           x => x.ImproveSource
                                                                == Improvement.ImprovementSource.Quality
                                                                && x.SourceName == strImprovedName && x.Enabled),
                                                       token);
                                else
                                    await EnableImprovementsAsync(objCharacter,
                                                                  await objCharacter.Improvements.ToListAsync(
                                                                          x => x.ImproveSource
                                                                               == Improvement.ImprovementSource.Quality
                                                                               && x.SourceName == strImprovedName
                                                                               && x.Enabled, token: token)
                                                                      .ConfigureAwait(false), token)
                                        .ConfigureAwait(false);
                            }

                            break;
                        /*
                        case Improvement.ImprovementType.SkillSpecialization:
                            {
                                Skill objSkill = objCharacter.SkillsSection.GetActiveSkill(strImprovedName);
                                SkillSpecialization objSkillSpec = objSkill?.Specializations.FirstOrDefault(x => x.Name == strUniqueName);
                                //if (objSkillSpec != null)
                                    // TODO: Temporarily remove skill specialization
                            }
                            break;
                            */
                        case Improvement.ImprovementType.AIProgram:
                            AIProgram objProgram = blnSync
                                ? objCharacter.AIPrograms.FirstOrDefault(o => o.InternalId == strImprovedName)
                                : await objCharacter.AIPrograms.FirstOrDefaultAsync(
                                    o => o.InternalId == strImprovedName, token).ConfigureAwait(false);
                            if (objProgram != null)
                            {
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverload
                                    EnableImprovements(objCharacter,
                                                       objCharacter.Improvements.Where(
                                                           x => x.ImproveSource
                                                                == Improvement.ImprovementSource.AIProgram
                                                                && x.SourceName == strImprovedName && x.Enabled),
                                                       token);
                                else
                                    await EnableImprovementsAsync(objCharacter,
                                                                  await objCharacter.Improvements.ToListAsync(
                                                                          x => x.ImproveSource
                                                                               == Improvement.ImprovementSource
                                                                                   .AIProgram
                                                                               && x.SourceName == strImprovedName
                                                                               && x.Enabled, token: token)
                                                                      .ConfigureAwait(false), token)
                                        .ConfigureAwait(false);
                            }

                            break;

                        case Improvement.ImprovementType.FreeWare:
                        {
                            Cyberware objCyberware = blnSync
                                ? objCharacter.Cyberware.FirstOrDefault(o => o.InternalId == strImprovedName)
                                : await objCharacter.Cyberware.FirstOrDefaultAsync(
                                    o => o.InternalId == strImprovedName, token).ConfigureAwait(false);
                            if (objCyberware != null && objCyberware.SourceID != Cyberware.EssenceHoleGUID &&
                                objCyberware.SourceID != Cyberware.EssenceAntiHoleGUID)
                            {
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    objCyberware.ChangeModularEquip(true);
                                else
                                    await objCyberware.ChangeModularEquipAsync(true, token: token)
                                        .ConfigureAwait(false);
                            }
                        }
                            break;
                    }
                }

                if (blnSync)
                    // ReSharper disable once MethodHasAsyncOverload
                    objImprovementList.ProcessRelevantEvents(token);
                else
                    await objImprovementList.ProcessRelevantEventsAsync(token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public static void DisableImprovements(Character objCharacter, IEnumerable<Improvement> objImprovementList, CancellationToken token = default)
        {
            DisableImprovements(objCharacter, objImprovementList.ToList(), token);
        }

        public static void DisableImprovements(Character objCharacter, Improvement objImprovement, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            DisableImprovements(objCharacter, objImprovement.Yield(), token);
        }

        public static void DisableImprovements(Character objCharacter, params Improvement[] objImprovementList)
        {
            DisableImprovements(objCharacter, Array.AsReadOnly(objImprovementList));
        }

        public static void DisableImprovements(Character objCharacter,
                                               IReadOnlyCollection<Improvement> objImprovementList, CancellationToken token = default)
        {
            Utils.SafelyRunSynchronously(() => DisableImprovementsCoreAsync(true, objCharacter, objImprovementList, token), token);
        }

        public static Task DisableImprovementsAsync(Character objCharacter, IEnumerable<Improvement> objImprovementList, CancellationToken token = default)
        {
            return DisableImprovementsAsync(objCharacter, objImprovementList.ToList(), token);
        }

        public static Task DisableImprovementsAsync(Character objCharacter, Improvement objImprovement, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return DisableImprovementsAsync(objCharacter, objImprovement.Yield(), token);
        }

        public static Task DisableImprovementsAsync(Character objCharacter, params Improvement[] objImprovementList)
        {
            return DisableImprovementsAsync(objCharacter, Array.AsReadOnly(objImprovementList));
        }

        public static Task DisableImprovementsAsync(Character objCharacter,
                                                    IReadOnlyCollection<Improvement> objImprovementList, CancellationToken token = default)
        {
            return DisableImprovementsCoreAsync(false, objCharacter, objImprovementList, token);
        }

        public static async Task DisableImprovementsCoreAsync(bool blnSync, Character objCharacter,
                                                              IReadOnlyCollection<Improvement> objImprovementList,
                                                              CancellationToken token = default)
        {
            if (objCharacter == null)
                throw new ArgumentNullException(nameof(objCharacter));
            if (objImprovementList == null)
                throw new ArgumentNullException(nameof(objImprovementList));

            IDisposable objLocker = null;
            IAsyncDisposable objAsyncLocker = null;
            if (blnSync)
                objLocker = objCharacter.LockObject.EnterWriteLock(token);
            else
                objAsyncLocker = await objCharacter.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                foreach (Improvement objImprovement in objImprovementList)
                {
                    // Disable the Improvement.
                    if (blnSync)
                        objImprovement.Enabled = false;
                    else
                        await objImprovement.SetEnabledAsync(false, token).ConfigureAwait(false);
                }

                // Now that the entire list is deleted from the character's improvements list, we do the checking of duplicates and extra effects
                foreach (Improvement objImprovement in objImprovementList)
                {
                    string strImprovedName = objImprovement.ImprovedName;
                    string strUniqueName = objImprovement.UniqueName;
                    Improvement.ImprovementType eImprovementType = objImprovement.ImproveType;
                    string strSourceName = objImprovement.SourceName;
                    bool blnHasDuplicate = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? objCharacter.Improvements.Any(
                            x => x.UniqueName == strUniqueName && x.ImprovedName == strImprovedName
                                                               && x.ImproveType == eImprovementType
                                                               && x.SourceName != strSourceName
                                                               && x.Enabled, token)
                        : await objCharacter.Improvements.AnyAsync(
                            x => x.UniqueName == strUniqueName && x.ImprovedName == strImprovedName
                                                               && x.ImproveType == eImprovementType
                                                               && x.SourceName != strSourceName
                                                               && x.Enabled, token: token).ConfigureAwait(false);

                    switch (eImprovementType)
                    {
                        case Improvement.ImprovementType.SkillLevel:
                            //TODO: Come back here and figure out wtf this did? Think it removed nested lifemodule skills? //Didn't this handle the collapsing knowledge skills thing?
                            //for (int i = _objCharacter.SkillsSection.Skills.Count - 1; i >= 0; i--)
                            //{
                            //    //wrote as foreach first, modify collection, not want rename
                            //    Skill skill = _objCharacter.SkillsSection.Skills[i];
                            //    for (int j = skill.Fold.Count - 1; j >= 0; j--)
                            //    {
                            //        Skill fold = skill.Fold[i];
                            //        if (fold.Id.ToString() == strImprovedName)
                            //        {
                            //            skill.Free(fold);
                            //            _objCharacter.SkillsSection.Skills.Remove(fold);
                            //        }
                            //    }

                            //    if (skill.Id.ToString() == strImprovedName)
                            //    {
                            //        while(skill.Fold.Count > 0) skill.Free(skill.Fold[0]);
                            //        //empty list, can't call clear as exposed list is RO

                            //        _objCharacter.SkillsSection.Skills.Remove(skill);
                            //    }
                            //}
                            break;

                        case Improvement.ImprovementType.SkillsoftAccess:
                            if (!blnHasDuplicate)
                            {
                                if (blnSync)
                                {
                                    // ReSharper disable once MethodHasAsyncOverload
                                    objCharacter.SkillsSection.KnowsoftSkills.ForEach(
                                        objKnowledgeSkill =>
                                            objCharacter.SkillsSection.KnowledgeSkills.Remove(objKnowledgeSkill),
                                        token: token);
                                }
                                else
                                {
                                    await objCharacter.SkillsSection.KnowsoftSkills.ForEachAsync(
                                        objKnowledgeSkill => objCharacter.SkillsSection.KnowledgeSkills
                                                                         .RemoveAsync(objKnowledgeSkill, token)
                                                                         , token: token).ConfigureAwait(false);
                                }
                            }

                            break;

                        case Improvement.ImprovementType.Skillsoft:
                            if (!blnHasDuplicate)
                            {
                                if (blnSync)
                                {
                                    objCharacter.SkillsSection.KnowsoftSkills.RemoveAll(
                                        x => x.InternalId == strImprovedName, token: token);
                                }
                                else
                                {
                                    await (await (await objCharacter.GetSkillsSectionAsync(token).ConfigureAwait(false))
                                        .GetKnowsoftSkillsAsync(token).ConfigureAwait(false))
                                        .RemoveAllAsync(x => x.InternalId == strImprovedName, token: token).ConfigureAwait(false);
                                }
                            }

                            break;

                        case Improvement.ImprovementType.Attribute:
                            // Determine if access to any Special Attributes have been lost.
                            if (string.Equals(strUniqueName, "enableattribute", StringComparison.OrdinalIgnoreCase) && !blnHasDuplicate)
                            {
                                switch (strImprovedName.ToUpperInvariant())
                                {
                                    case "MAG":
                                        if (blnSync)
                                            objCharacter.MAGEnabled = false;
                                        else
                                            await objCharacter.SetMAGEnabledAsync(false, token).ConfigureAwait(false);
                                        break;

                                    case "RES":
                                        if (blnSync)
                                            objCharacter.RESEnabled = false;
                                        else
                                            await objCharacter.SetRESEnabledAsync(false, token).ConfigureAwait(false);
                                        break;

                                    case "DEP":
                                        if (blnSync)
                                            objCharacter.DEPEnabled = false;
                                        else
                                            await objCharacter.SetDEPEnabledAsync(false, token).ConfigureAwait(false);
                                        break;
                                }
                            }

                            break;

                        case Improvement.ImprovementType.SpecialTab:
                            // Determine if access to any special tabs have been lost.
                            if (!blnHasDuplicate)
                            {
                                switch (strUniqueName.ToUpperInvariant())
                                {
                                    case "ENABLETAB":
                                        switch (strImprovedName.ToUpperInvariant())
                                        {
                                            case "MAGICIAN":
                                                if (blnSync)
                                                    objCharacter.MagicianEnabled = false;
                                                else
                                                    await objCharacter.SetMagicianEnabledAsync(false, token).ConfigureAwait(false);
                                                break;

                                            case "ADEPT":
                                                if (blnSync)
                                                    objCharacter.AdeptEnabled = false;
                                                else
                                                    await objCharacter.SetAdeptEnabledAsync(false, token).ConfigureAwait(false);
                                                break;

                                            case "TECHNOMANCER":
                                                if (blnSync)
                                                    objCharacter.TechnomancerEnabled = false;
                                                else
                                                    await objCharacter.SetTechnomancerEnabledAsync(false, token).ConfigureAwait(false);
                                                break;

                                            case "ADVANCED PROGRAMS":
                                                if (blnSync)
                                                    objCharacter.AdvancedProgramsEnabled = false;
                                                else
                                                    await objCharacter.SetAdvancedProgramsEnabledAsync(false, token).ConfigureAwait(false);
                                                break;

                                            case "CRITTER":
                                                if (blnSync)
                                                    objCharacter.CritterEnabled = false;
                                                else
                                                    await objCharacter.SetCritterEnabledAsync(false, token).ConfigureAwait(false);
                                                break;
                                        }

                                        break;
                                    // Determine if access to any special tabs has been regained
                                    case "DISABLETAB":
                                        switch (strImprovedName)
                                        {
                                            case "CYBERWARE":
                                                if (blnSync)
                                                    objCharacter.CyberwareDisabled = false;
                                                else
                                                    await objCharacter.SetCyberwareDisabledAsync(false, token).ConfigureAwait(false);
                                                break;

                                            case "INITIATION":
                                                if (blnSync)
                                                    objCharacter.InitiationForceDisabled = false;
                                                else
                                                    await objCharacter.SetInitiationForceDisabledAsync(false, token).ConfigureAwait(false);
                                                break;
                                        }

                                        break;
                                }
                            }

                            break;

                        case Improvement.ImprovementType.PrototypeTranshuman:
                            // Legacy compatibility
                            if (string.IsNullOrEmpty(strImprovedName))
                            {
                                if (!blnHasDuplicate)
                                {
                                    if (blnSync)
                                        objCharacter.PrototypeTranshuman = 0;
                                    else
                                        await objCharacter.SetPrototypeTranshumanAsync(0, token).ConfigureAwait(false);
                                }
                            }
                            else if (blnSync)
                            {
                                // ReSharper disable once MethodHasAsyncOverload
                                decimal decValue = ValueToDec(objCharacter, strImprovedName, objImprovement.Rating, token);
                                objCharacter.PrototypeTranshuman -= decValue;
                            }
                            else
                            {
                                decimal decValue = await ValueToDecAsync(objCharacter, strImprovedName, objImprovement.Rating, token).ConfigureAwait(false);
                                await objCharacter.ModifyPrototypeTranshumanAsync(-decValue, token).ConfigureAwait(false);
                            }

                            break;

                        case Improvement.ImprovementType.Adapsin:
                            break;

                        case Improvement.ImprovementType.AddContact:
                            Contact objNewContact
                                = blnSync
                                    ? objCharacter.Contacts.FirstOrDefault(c => c.UniqueId == strImprovedName)
                                    : await objCharacter.Contacts.FirstOrDefaultAsync(
                                        c => c.UniqueId == strImprovedName, token).ConfigureAwait(false);
                            if (objNewContact != null)
                            {
                                // TODO: Add code to disable contact
                            }

                            break;

                        case Improvement.ImprovementType.Art:
                            Art objArt = blnSync
                                ? objCharacter.Arts.FirstOrDefault(x => x.InternalId == strImprovedName)
                                : await objCharacter.Arts.FirstOrDefaultAsync(
                                    x => x.InternalId == strImprovedName, token).ConfigureAwait(false);
                            if (objArt != null)
                            {
                                Improvement.ImprovementSource eSource = objArt.SourceType;
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverload
                                    DisableImprovements(objCharacter,
                                                        objCharacter.Improvements.Where(
                                                            x => x.ImproveSource == eSource
                                                                 && x.SourceName == strImprovedName && x.Enabled),
                                                        token);
                                else
                                    await DisableImprovementsAsync(objCharacter,
                                                                   await objCharacter.Improvements.ToListAsync(
                                                                           x => x.ImproveSource == eSource
                                                                               && x.SourceName == strImprovedName
                                                                               && x.Enabled, token: token)
                                                                       .ConfigureAwait(false), token)
                                        .ConfigureAwait(false);
                            }

                            break;

                        case Improvement.ImprovementType.Metamagic:
                        case Improvement.ImprovementType.Echo:
                            Metamagic objMetamagic = blnSync
                                ? objCharacter.Metamagics.FirstOrDefault(o => o.InternalId == strImprovedName)
                                : await objCharacter.Metamagics.FirstOrDefaultAsync(
                                    o => o.InternalId == strImprovedName, token).ConfigureAwait(false);
                            if (objMetamagic != null)
                            {
                                Improvement.ImprovementSource eSource
                                    = objImprovement.ImproveType == Improvement.ImprovementType.Metamagic
                                        ? Improvement.ImprovementSource.Metamagic
                                        : Improvement.ImprovementSource.Echo;
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverload
                                    DisableImprovements(objCharacter,
                                                        objCharacter.Improvements.Where(
                                                            x => x.ImproveSource == eSource
                                                                 && x.SourceName == strImprovedName && x.Enabled),
                                                        token);
                                else
                                    await DisableImprovementsAsync(objCharacter,
                                                                   await objCharacter.Improvements.ToListAsync(
                                                                           x => x.ImproveSource == eSource
                                                                               && x.SourceName == strImprovedName
                                                                               && x.Enabled, token: token)
                                                                       .ConfigureAwait(false), token)
                                        .ConfigureAwait(false);
                            }

                            break;

                        case Improvement.ImprovementType.CritterPower:
                            CritterPower objCritterPower = blnSync
                                ? objCharacter.CritterPowers.FirstOrDefault(
                                    x => x.InternalId == strImprovedName
                                         || (x.Name == strImprovedName && x.Extra == strUniqueName))
                                : await objCharacter.CritterPowers.FirstOrDefaultAsync(
                                                        x => x.InternalId == strImprovedName
                                                             || (x.Name == strImprovedName && x.Extra == strUniqueName),
                                                        token)
                                                    .ConfigureAwait(false);
                            if (objCritterPower != null)
                            {
                                string strPowerId = objCritterPower.InternalId;
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverload
                                    DisableImprovements(objCharacter,
                                                        objCharacter.Improvements.Where(
                                                            x => x.ImproveSource
                                                                 == Improvement.ImprovementSource.CritterPower
                                                                 && x.SourceName == strPowerId && x.Enabled), token);
                                else
                                    await DisableImprovementsAsync(objCharacter,
                                                                   await objCharacter.Improvements.ToListAsync(
                                                                           x => x.ImproveSource
                                                                               == Improvement.ImprovementSource
                                                                                   .CritterPower
                                                                               && x.SourceName == strPowerId
                                                                               && x.Enabled, token: token)
                                                                       .ConfigureAwait(false), token)
                                        .ConfigureAwait(false);
                            }

                            break;

                        case Improvement.ImprovementType.MentorSpirit:
                        case Improvement.ImprovementType.Paragon:
                            MentorSpirit objMentor = blnSync
                                ? objCharacter.MentorSpirits.FirstOrDefault(o => o.InternalId == strImprovedName)
                                : await objCharacter.MentorSpirits.FirstOrDefaultAsync(
                                    o => o.InternalId == strImprovedName, token).ConfigureAwait(false);
                            if (objMentor != null)
                            {
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverload
                                    DisableImprovements(objCharacter,
                                                        objCharacter.Improvements.Where(
                                                            x => x.ImproveSource
                                                                 == Improvement.ImprovementSource.MentorSpirit
                                                                 && x.SourceName == strImprovedName && x.Enabled),
                                                        token);
                                else
                                    await DisableImprovementsAsync(objCharacter,
                                                                   await objCharacter.Improvements.ToListAsync(
                                                                           x => x.ImproveSource
                                                                               == Improvement.ImprovementSource
                                                                                   .MentorSpirit
                                                                               && x.SourceName == strImprovedName
                                                                               && x.Enabled, token: token)
                                                                       .ConfigureAwait(false), token)
                                        .ConfigureAwait(false);
                            }

                            break;

                        case Improvement.ImprovementType.Gear:
                            Gear objGear = blnSync
                                ? objCharacter.Gear.FirstOrDefault(o => o.InternalId == strImprovedName)
                                : await objCharacter.Gear.FirstOrDefaultAsync(
                                    o => o.InternalId == strImprovedName, token).ConfigureAwait(false);
                            if (objGear != null)
                            {
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    objGear.ChangeEquippedStatus(false);
                                else
                                    await objGear.ChangeEquippedStatusAsync(false, token: token).ConfigureAwait(false);
                            }

                            break;

                        case Improvement.ImprovementType.Weapon:
                            Weapon objWeapon
                                = blnSync
                                    ? objCharacter.Weapons.DeepFirstOrDefault(x => x.Children,
                                          x => x.InternalId == strImprovedName, token)
                                      ??
                                      objCharacter.Vehicles.FindVehicleWeapon(strImprovedName, token)
                                    : await objCharacter.Weapons.DeepFirstOrDefaultAsync(x => x.Children,
                                          x => x.InternalId == strImprovedName, token: token).ConfigureAwait(false)
                                      ?? (await objCharacter.Vehicles.FindVehicleWeaponAsync(strImprovedName, token)
                                          .ConfigureAwait(false))
                                      .Item1;
                            if (objWeapon != null)
                            {
                                if (blnSync)
                                    objWeapon.Equipped = false;
                                else
                                    await objWeapon.SetEquippedAsync(false, token).ConfigureAwait(false);
                            }
                            break;

                        case Improvement.ImprovementType.Spell:
                            Spell objSpell = blnSync
                                ? objCharacter.Spells.FirstOrDefault(o => o.InternalId == strImprovedName)
                                : await objCharacter.Spells.FirstOrDefaultAsync(
                                    o => o.InternalId == strImprovedName, token).ConfigureAwait(false);
                            if (objSpell != null)
                            {
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverload
                                    DisableImprovements(objCharacter,
                                                        objCharacter.Improvements.Where(
                                                            x => x.ImproveSource == Improvement.ImprovementSource.Spell
                                                                 && x.SourceName == strImprovedName && x.Enabled),
                                                        token);
                                else
                                    await DisableImprovementsAsync(objCharacter,
                                                                   await objCharacter.Improvements.ToListAsync(
                                                                           x => x.ImproveSource
                                                                               == Improvement.ImprovementSource.Spell
                                                                               && x.SourceName == strImprovedName
                                                                               && x.Enabled, token: token)
                                                                       .ConfigureAwait(false), token)
                                        .ConfigureAwait(false);
                            }

                            break;

                        case Improvement.ImprovementType.ComplexForm:
                            ComplexForm objComplexForm = blnSync
                                ? objCharacter.ComplexForms.FirstOrDefault(o => o.InternalId == strImprovedName)
                                : await objCharacter.ComplexForms.FirstOrDefaultAsync(
                                    o => o.InternalId == strImprovedName, token).ConfigureAwait(false);
                            if (objComplexForm != null)
                            {
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverload
                                    DisableImprovements(objCharacter,
                                                        objCharacter.Improvements.Where(
                                                            x => x.ImproveSource == Improvement.ImprovementSource
                                                                     .ComplexForm
                                                                 && x.SourceName == strImprovedName && x.Enabled),
                                                        token);
                                else
                                    await DisableImprovementsAsync(objCharacter,
                                                                   await objCharacter.Improvements.ToListAsync(
                                                                           x => x.ImproveSource
                                                                               == Improvement.ImprovementSource
                                                                                   .ComplexForm
                                                                               && x.SourceName == strImprovedName
                                                                               && x.Enabled, token: token)
                                                                       .ConfigureAwait(false), token)
                                        .ConfigureAwait(false);
                            }

                            break;

                        case Improvement.ImprovementType.MartialArt:
                            MartialArt objMartialArt = blnSync
                                ? objCharacter.MartialArts.FirstOrDefault(o => o.InternalId == strImprovedName)
                                : await objCharacter.MartialArts.FirstOrDefaultAsync(
                                    o => o.InternalId == strImprovedName, token).ConfigureAwait(false);
                            if (objMartialArt != null)
                            {
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverload
                                    DisableImprovements(objCharacter,
                                                        objCharacter.Improvements.Where(
                                                            x => x.ImproveSource == Improvement.ImprovementSource
                                                                     .MartialArt
                                                                 && x.SourceName == strImprovedName && x.Enabled),
                                                        token);
                                else
                                    await DisableImprovementsAsync(objCharacter,
                                                                   await objCharacter.Improvements.ToListAsync(
                                                                           x => x.ImproveSource
                                                                               == Improvement.ImprovementSource
                                                                                   .MartialArt
                                                                               && x.SourceName == strImprovedName
                                                                               && x.Enabled, token: token)
                                                                       .ConfigureAwait(false), token)
                                        .ConfigureAwait(false);
                                // Remove the Improvements for any Techniques for the Martial Art that is being removed.
                                if (blnSync)
                                {
                                    // ReSharper disable once MethodHasAsyncOverload
                                    objMartialArt.Techniques.ForEach(objTechnique =>
                                    {
                                        string strTechniqueId = objTechnique.InternalId;
                                        DisableImprovements(objCharacter,
                                                            objCharacter.Improvements.Where(
                                                                x => x.ImproveSource == Improvement.ImprovementSource
                                                                         .MartialArtTechnique
                                                                     && x.SourceName == strTechniqueId && x.Enabled),
                                                            token);
                                    }, token: token);
                                }
                                else
                                {
                                    await objMartialArt.Techniques.ForEachAsync(async objTechnique =>
                                    {
                                        string strTechniqueId = objTechnique.InternalId;
                                        await DisableImprovementsAsync(objCharacter,
                                                                       await objCharacter.Improvements.ToListAsync(
                                                                               x => x.ImproveSource
                                                                                   == Improvement.ImprovementSource
                                                                                       .MartialArtTechnique
                                                                                   && x.SourceName == strTechniqueId
                                                                                   && x.Enabled, token: token)
                                                                           .ConfigureAwait(false), token)
                                            .ConfigureAwait(false);
                                    }, token: token).ConfigureAwait(false);
                                }
                            }

                            break;

                        case Improvement.ImprovementType.SpecialSkills:
                            if (!blnHasDuplicate)
                            {
                                SkillsSection.FilterOption eFilterOption
                                    = (SkillsSection.FilterOption) Enum.Parse(
                                        typeof(SkillsSection.FilterOption), strImprovedName);
                                HashSet<Skill> setSkillsToKeepEnabled = new HashSet<Skill>();
                                if (blnSync)
                                {
                                    foreach (Improvement objLoopImprovement in
                                             // ReSharper disable once MethodHasAsyncOverload
                                             GetCachedImprovementListForValueOf(
                                                     objCharacter, Improvement.ImprovementType.SpecialSkills,
                                                     token: token))
                                    {
                                        if (objLoopImprovement == objImprovement)
                                            continue;
                                        eFilterOption
                                            = (SkillsSection.FilterOption)Enum.Parse(
                                                typeof(SkillsSection.FilterOption), objLoopImprovement.ImprovedName);
                                        setSkillsToKeepEnabled.AddRange(objCharacter.SkillsSection.FetchExistingSkillsByFilter(eFilterOption, objLoopImprovement.Target, false, token));
                                    }
                                    foreach (Skill objSkill in objCharacter.SkillsSection.FetchExistingSkillsByFilter(eFilterOption, objImprovement.Target, true, token))
                                    {
                                        objSkill.ForceDisabled = true;
                                    }
                                }
                                else
                                {
                                    foreach (Improvement objLoopImprovement in await
                                             GetCachedImprovementListForValueOfAsync(
                                                     objCharacter, Improvement.ImprovementType.SpecialSkills,
                                                     token: token)
                                                 .ConfigureAwait(false))
                                    {
                                        if (objLoopImprovement == objImprovement)
                                            continue;
                                        eFilterOption
                                            = (SkillsSection.FilterOption)Enum.Parse(
                                                typeof(SkillsSection.FilterOption), objLoopImprovement.ImprovedName);
                                        setSkillsToKeepEnabled.AddRange(await objCharacter.SkillsSection.FetchExistingSkillsByFilterAsync(eFilterOption, objLoopImprovement.Target, token).ConfigureAwait(false));
                                    }
                                    foreach (Skill objSkill in await objCharacter.SkillsSection.FetchExistingSkillsByFilterAsync(eFilterOption, objImprovement.Target, token).ConfigureAwait(false))
                                    {
                                        if (!setSkillsToKeepEnabled.Contains(objSkill))
                                            await objSkill.SetForceDisabledAsync(true, token).ConfigureAwait(false);
                                    }
                                }
                            }

                            break;

                        case Improvement.ImprovementType.SpecificQuality:
                            Quality objQuality = blnSync
                                ? objCharacter.Qualities.FirstOrDefault(o => o.InternalId == strImprovedName)
                                : await objCharacter.Qualities.FirstOrDefaultAsync(
                                    o => o.InternalId == strImprovedName, token).ConfigureAwait(false);
                            if (objQuality != null)
                            {
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverload
                                    DisableImprovements(objCharacter,
                                                        objCharacter.Improvements.Where(
                                                            x => x.ImproveSource
                                                                 == Improvement.ImprovementSource.Quality
                                                                 && x.SourceName == strImprovedName && x.Enabled),
                                                        token);
                                else
                                    await DisableImprovementsAsync(objCharacter,
                                                                   await objCharacter.Improvements.ToListAsync(
                                                                           x => x.ImproveSource
                                                                               == Improvement.ImprovementSource.Quality
                                                                               && x.SourceName == strImprovedName
                                                                               && x.Enabled, token: token)
                                                                       .ConfigureAwait(false), token)
                                        .ConfigureAwait(false);
                            }

                            break;
                        /*
                        case Improvement.ImprovementType.SkillSpecialization:
                            {
                                Skill objSkill = objCharacter.SkillsSection.GetActiveSkill(strImprovedName);
                                SkillSpecialization objSkillSpec = objSkill?.Specializations.FirstOrDefault(x => x.Name == strUniqueName);
                                //if (objSkillSpec != null)
                                    // TODO: Temporarily remove skill specialization
                            }
                            break;
                            */
                        case Improvement.ImprovementType.AIProgram:
                            AIProgram objProgram = blnSync
                                ? objCharacter.AIPrograms.FirstOrDefault(o => o.InternalId == strImprovedName)
                                : await objCharacter.AIPrograms.FirstOrDefaultAsync(
                                    o => o.InternalId == strImprovedName, token).ConfigureAwait(false);
                            if (objProgram != null)
                            {
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverload
                                    DisableImprovements(objCharacter,
                                                        objCharacter.Improvements.Where(
                                                            x => x.ImproveSource
                                                                 == Improvement.ImprovementSource.AIProgram
                                                                 && x.SourceName == strImprovedName && x.Enabled),
                                                        token);
                                else
                                    await DisableImprovementsAsync(objCharacter,
                                                                   await objCharacter.Improvements.ToListAsync(
                                                                           x => x.ImproveSource
                                                                               == Improvement.ImprovementSource
                                                                                   .AIProgram
                                                                               && x.SourceName == strImprovedName
                                                                               && x.Enabled, token: token)
                                                                       .ConfigureAwait(false), token)
                                        .ConfigureAwait(false);
                            }

                            break;

                        case Improvement.ImprovementType.FreeWare:
                        {
                            Cyberware objCyberware = blnSync
                                ? objCharacter.Cyberware.FirstOrDefault(o => o.InternalId == strImprovedName)
                                : await objCharacter.Cyberware.FirstOrDefaultAsync(
                                    o => o.InternalId == strImprovedName, token).ConfigureAwait(false);
                            if (objCyberware != null && objCyberware.SourceID != Cyberware.EssenceHoleGUID &&
                                objCyberware.SourceID != Cyberware.EssenceAntiHoleGUID)
                            {
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    objCyberware.ChangeModularEquip(false);
                                else
                                    await objCyberware.ChangeModularEquipAsync(false, token: token)
                                                      .ConfigureAwait(false);
                            }
                        }
                            break;
                    }
                }

                if (blnSync)
                    // ReSharper disable once MethodHasAsyncOverload
                    objImprovementList.ProcessRelevantEvents(token);
                else
                    await objImprovementList.ProcessRelevantEventsAsync(token).ConfigureAwait(false);
            }
            finally
            {
                objLocker?.Dispose();
                if (objAsyncLocker != null)
                    await objAsyncLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Remove all the Improvements for an XML Node.
        /// </summary>
        /// <param name="objCharacter">Character from which improvements should be deleted.</param>
        /// <param name="objImprovementSource">Type of object that granted these Improvements.</param>
        /// <param name="strSourceName">Name of the item that granted these Improvements.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static decimal RemoveImprovements(Character objCharacter,
                                                 Improvement.ImprovementSource objImprovementSource,
                                                 string strSourceName = "", CancellationToken token = default)
        {
            // If there is no character object, don't try to remove any Improvements.
            if (objCharacter == null)
            {
                return 0;
            }

            Log.Debug("RemoveImprovements called with:" + Environment.NewLine + "objImprovementSource = "
                     + objImprovementSource + Environment.NewLine + "strSourceName = " + strSourceName);
            List<Improvement> objImprovementList;
            using (objCharacter.LockObject.EnterReadLock(token))
            {
                // A List of Improvements to hold all the items that will eventually be deleted.
                objImprovementList = (string.IsNullOrEmpty(strSourceName)
                    ? objCharacter.Improvements.Where(objImprovement =>
                                                          objImprovement.ImproveSource == objImprovementSource)
                    : objCharacter.Improvements.Where(objImprovement =>
                                                          objImprovement.ImproveSource == objImprovementSource
                                                          && objImprovement.SourceName == strSourceName)).ToList();

                // Compatibility fix for when blnConcatSelectedValue was around
                if (strSourceName.IsGuid())
                {
                    string strSourceNameSpaced = strSourceName + LanguageManager.GetString("String_Space", token: token);
                    string strSourceNameSpacedInvariant = strSourceName + " ";
                    objImprovementList.AddRange(objCharacter.Improvements.Where(
                                                    objImprovement =>
                                                        objImprovement.ImproveSource == objImprovementSource &&
                                                        (objImprovement.SourceName.StartsWith(
                                                             strSourceNameSpaced, StringComparison.Ordinal)
                                                         || objImprovement.SourceName.StartsWith(
                                                             strSourceNameSpacedInvariant, StringComparison.Ordinal))));
                }
            }

            return RemoveImprovements(objCharacter, objImprovementList, token: token);
        }

        /// <summary>
        /// Remove all the Improvements for an XML Node.
        /// </summary>
        /// <param name="objCharacter">Character from which improvements should be deleted.</param>
        /// <param name="lstImprovementSources">Types of object that granted these Improvements.</param>
        /// <param name="strSourceName">Name of the item that granted these Improvements.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static decimal RemoveImprovements(Character objCharacter,
            IReadOnlyCollection<Improvement.ImprovementSource> lstImprovementSources,
            string strSourceName = "", CancellationToken token = default)
        {
            // If there is no character object, don't try to remove any Improvements.
            if (objCharacter == null)
            {
                return 0;
            }

            Log.Debug("RemoveImprovements called with:" + Environment.NewLine + "lstImprovementSources = "
                      + lstImprovementSources + Environment.NewLine + "strSourceName = " + strSourceName);
            List<Improvement> objImprovementList;
            using (objCharacter.LockObject.EnterReadLock(token))
            {
                // A List of Improvements to hold all the items that will eventually be deleted.
                objImprovementList = (string.IsNullOrEmpty(strSourceName)
                    ? objCharacter.Improvements.Where(objImprovement =>
                        lstImprovementSources.Contains(objImprovement.ImproveSource))
                    : objCharacter.Improvements.Where(objImprovement =>
                        lstImprovementSources.Contains(objImprovement.ImproveSource) &&
                        objImprovement.SourceName == strSourceName)).ToList();

                // Compatibility fix for when blnConcatSelectedValue was around
                if (strSourceName.IsGuid())
                {
                    string strSourceNameSpaced =
                        strSourceName + LanguageManager.GetString("String_Space", token: token);
                    string strSourceNameSpacedInvariant = strSourceName + " ";
                    objImprovementList.AddRange(objCharacter.Improvements.Where(
                        objImprovement =>
                            lstImprovementSources.Contains(objImprovement.ImproveSource) &&
                            (objImprovement.SourceName.StartsWith(
                                 strSourceNameSpaced, StringComparison.Ordinal)
                             || objImprovement.SourceName.StartsWith(
                                 strSourceNameSpacedInvariant, StringComparison.Ordinal))));
                }
            }

            return RemoveImprovements(objCharacter, objImprovementList, token: token);
        }

        /// <summary>
        /// Remove all the Improvements for an XML Node.
        /// </summary>
        /// <param name="objCharacter">Character from which improvements should be deleted.</param>
        /// <param name="objImprovementSource">Type of object that granted these Improvements.</param>
        /// <param name="lstSourceNames">Names of the items that granted these Improvements.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static decimal RemoveImprovements(Character objCharacter,
            Improvement.ImprovementSource objImprovementSource,
            IReadOnlyCollection<string> lstSourceNames, CancellationToken token = default)
        {
            // If there is no character object, don't try to remove any Improvements.
            if (objCharacter == null)
            {
                return 0;
            }
            // If there is nothing to remove, don't try to remove any Improvements
            if (lstSourceNames == null || lstSourceNames.Count == 0)
            {
                return 0;
            }

            Log.Debug("RemoveImprovements called with:" + Environment.NewLine + "objImprovementSource = "
                      + objImprovementSource + Environment.NewLine + "lstSourceNames = " + lstSourceNames);
            List<Improvement> objImprovementList;
            using (objCharacter.LockObject.EnterReadLock(token))
            {
                // A List of Improvements to hold all the items that will eventually be deleted.
                if (lstSourceNames.Any(x => x.IsGuid()))
                {
                    // Compatibility fix for when blnConcatSelectedValue was around
                    HashSet<string> setSpacedSourceNames = new HashSet<string>(lstSourceNames.Count);
                    foreach (string strSourceName in lstSourceNames)
                    {
                        if (!strSourceName.IsGuid())
                            continue;
                        setSpacedSourceNames.Add(
                            strSourceName + LanguageManager.GetString("String_Space", token: token));
                        setSpacedSourceNames.Add(
                            strSourceName + " ");
                    }

                    objImprovementList = new List<Improvement>(objCharacter.Improvements.Count);
                    foreach (Improvement objImprovement in objCharacter.Improvements)
                    {
                        if (objImprovement.ImproveSource != objImprovementSource)
                            continue;
                        if (lstSourceNames.Contains(objImprovement.SourceName) || setSpacedSourceNames.Any(x =>
                                objImprovement.SourceName.StartsWith(x, StringComparison.Ordinal)))
                        {
                            objImprovementList.Add(objImprovement);
                        }
                    }
                }
                else
                {
                    objImprovementList = objCharacter.Improvements.Where(objImprovement =>
                        objImprovement.ImproveSource == objImprovementSource &&
                        lstSourceNames.Contains(objImprovement.SourceName)).ToList();
                }
            }

            return RemoveImprovements(objCharacter, objImprovementList, token: token);
        }

        /// <summary>
        /// Remove all the Improvements for an XML Node.
        /// </summary>
        /// <param name="objCharacter">Character from which improvements should be deleted.</param>
        /// <param name="lstImprovementSources">Types of object that granted these Improvements.</param>
        /// <param name="lstSourceNames">Names of the items that granted these Improvements.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static decimal RemoveImprovements(Character objCharacter,
            IReadOnlyCollection<Improvement.ImprovementSource> lstImprovementSources,
            IReadOnlyCollection<string> lstSourceNames, CancellationToken token = default)
        {
            // If there is no character object, don't try to remove any Improvements.
            if (objCharacter == null)
            {
                return 0;
            }
            // If there is nothing to remove, don't try to remove any Improvements
            if (lstImprovementSources == null || lstImprovementSources.Count == 0)
            {
                return 0;
            }
            if (lstSourceNames == null || lstSourceNames.Count == 0)
            {
                return 0;
            }

            Log.Debug("RemoveImprovements called with:" + Environment.NewLine + "lstImprovementSources = "
                      + lstImprovementSources + Environment.NewLine + "lstSourceNames = " + lstSourceNames);
            List<Improvement> objImprovementList;
            using (objCharacter.LockObject.EnterReadLock(token))
            {
                // A List of Improvements to hold all the items that will eventually be deleted.
                if (lstSourceNames.Any(x => x.IsGuid()))
                {
                    // Compatibility fix for when blnConcatSelectedValue was around
                    HashSet<string> setSpacedSourceNames = new HashSet<string>(lstSourceNames.Count);
                    foreach (string strSourceName in lstSourceNames)
                    {
                        if (!strSourceName.IsGuid())
                            continue;
                        setSpacedSourceNames.Add(
                            strSourceName + LanguageManager.GetString("String_Space", token: token));
                        setSpacedSourceNames.Add(
                            strSourceName + " ");
                    }

                    objImprovementList = new List<Improvement>(objCharacter.Improvements.Count);
                    foreach (Improvement objImprovement in objCharacter.Improvements)
                    {
                        if (!lstImprovementSources.Contains(objImprovement.ImproveSource))
                            continue;
                        if (lstSourceNames.Contains(objImprovement.SourceName) || setSpacedSourceNames.Any(x =>
                                objImprovement.SourceName.StartsWith(x, StringComparison.Ordinal)))
                        {
                            objImprovementList.Add(objImprovement);
                        }
                    }
                }
                else
                {
                    objImprovementList = objCharacter.Improvements.Where(objImprovement =>
                        lstImprovementSources.Contains(objImprovement.ImproveSource) &&
                        lstSourceNames.Contains(objImprovement.SourceName)).ToList();
                }
            }

            return RemoveImprovements(objCharacter, objImprovementList, token: token);
        }

        /// <summary>
        /// Remove all the Improvements for an XML Node.
        /// </summary>
        /// <param name="objCharacter">Character from which improvements should be deleted.</param>
        /// <param name="objImprovementSource">Type of object that granted these Improvements.</param>
        /// <param name="strSourceName">Name of the item that granted these Improvements.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task<decimal> RemoveImprovementsAsync(Character objCharacter,
                                                                       Improvement.ImprovementSource objImprovementSource,
                                                                       string strSourceName = "", CancellationToken token = default)
        {
            // If there is no character object, don't try to remove any Improvements.
            if (objCharacter == null)
            {
                return 0;
            }

            Log.Debug("RemoveImprovements called with:" + Environment.NewLine + "objImprovementSource = "
                     + objImprovementSource + Environment.NewLine + "strSourceName = " + strSourceName);
            List<Improvement> objImprovementList;
            IAsyncDisposable objLocker = await objCharacter.LockObject.EnterReadLockAsync(token)
                .ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // A List of Improvements to hold all the items that will eventually be deleted.
                objImprovementList = (string.IsNullOrEmpty(strSourceName)
                    ? objCharacter.Improvements.Where(objImprovement =>
                        objImprovement.ImproveSource == objImprovementSource)
                    : objCharacter.Improvements.Where(objImprovement =>
                        objImprovement.ImproveSource == objImprovementSource
                        && objImprovement.SourceName == strSourceName)).ToList();

                // Compatibility fix for when blnConcatSelectedValue was around
                if (strSourceName.IsGuid())
                {
                    string strSourceNameSpaced = strSourceName +
                                                 await LanguageManager.GetStringAsync("String_Space", token: token)
                                                     .ConfigureAwait(false);
                    string strSourceNameSpacedInvariant = strSourceName + " ";
                    objImprovementList.AddRange(objCharacter.Improvements.Where(
                        objImprovement =>
                            objImprovement.ImproveSource == objImprovementSource &&
                            (objImprovement.SourceName.StartsWith(
                                 strSourceNameSpaced, StringComparison.Ordinal)
                             || objImprovement.SourceName.StartsWith(
                                 strSourceNameSpacedInvariant, StringComparison.Ordinal))));
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            return await RemoveImprovementsAsync(objCharacter, objImprovementList, token: token).ConfigureAwait(false);
        }

        /// <summary>
        /// Remove all the Improvements for an XML Node.
        /// </summary>
        /// <param name="objCharacter">Character from which improvements should be deleted.</param>
        /// <param name="lstImprovementSources">Types of object that granted these Improvements.</param>
        /// <param name="strSourceName">Name of the item that granted these Improvements.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task<decimal> RemoveImprovementsAsync(Character objCharacter,
            IReadOnlyCollection<Improvement.ImprovementSource> lstImprovementSources,
            string strSourceName = "", CancellationToken token = default)
        {
            // If there is no character object, don't try to remove any Improvements.
            if (objCharacter == null)
            {
                return 0;
            }

            Log.Debug("RemoveImprovements called with:" + Environment.NewLine + "lstImprovementSources = "
                      + lstImprovementSources + Environment.NewLine + "strSourceName = " + strSourceName);
            List<Improvement> objImprovementList;
            IAsyncDisposable objLocker = await objCharacter.LockObject.EnterReadLockAsync(token)
                .ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // A List of Improvements to hold all the items that will eventually be deleted.
                objImprovementList = await (string.IsNullOrEmpty(strSourceName)
                    ? objCharacter.Improvements.ToListAsync(objImprovement =>
                        lstImprovementSources.Contains(objImprovement.ImproveSource), token: token)
                    : objCharacter.Improvements.ToListAsync(objImprovement =>
                        lstImprovementSources.Contains(objImprovement.ImproveSource) &&
                        objImprovement.SourceName == strSourceName, token: token)).ConfigureAwait(false);

                // Compatibility fix for when blnConcatSelectedValue was around
                if (strSourceName.IsGuid())
                {
                    string strSourceNameSpaced = strSourceName +
                                                 await LanguageManager.GetStringAsync("String_Space", token: token)
                                                     .ConfigureAwait(false);
                    string strSourceNameSpacedInvariant = strSourceName + " ";
                    objImprovementList.AddRange(objCharacter.Improvements.Where(
                        objImprovement =>
                            lstImprovementSources.Contains(objImprovement.ImproveSource) &&
                            (objImprovement.SourceName.StartsWith(
                                 strSourceNameSpaced, StringComparison.Ordinal)
                             || objImprovement.SourceName.StartsWith(
                                 strSourceNameSpacedInvariant, StringComparison.Ordinal))));
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            return await RemoveImprovementsAsync(objCharacter, objImprovementList, token: token).ConfigureAwait(false);
        }

        /// <summary>
        /// Remove all the Improvements for an XML Node.
        /// </summary>
        /// <param name="objCharacter">Character from which improvements should be deleted.</param>
        /// <param name="objImprovementSource">Type of object that granted these Improvements.</param>
        /// <param name="lstSourceNames">Names of the items that granted these Improvements.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task<decimal> RemoveImprovementsAsync(Character objCharacter,
            Improvement.ImprovementSource objImprovementSource,
            IReadOnlyCollection<string> lstSourceNames, CancellationToken token = default)
        {
            // If there is no character object, don't try to remove any Improvements.
            if (objCharacter == null)
            {
                return 0;
            }
            // If there is nothing to remove, don't try to remove any Improvements
            if (lstSourceNames == null || lstSourceNames.Count == 0)
            {
                return 0;
            }

            Log.Debug("RemoveImprovements called with:" + Environment.NewLine + "objImprovementSource = "
                      + objImprovementSource + Environment.NewLine + "lstSourceNames = " + lstSourceNames);
            List<Improvement> objImprovementList;
            IAsyncDisposable objLocker = await objCharacter.LockObject.EnterReadLockAsync(token)
                .ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // A List of Improvements to hold all the items that will eventually be deleted.
                if (lstSourceNames.Any(x => x.IsGuid()))
                {
                    // Compatibility fix for when blnConcatSelectedValue was around
                    HashSet<string> setSpacedSourceNames = new HashSet<string>(lstSourceNames.Count);
                    foreach (string strSourceName in lstSourceNames)
                    {
                        if (!strSourceName.IsGuid())
                            continue;
                        setSpacedSourceNames.Add(
                            strSourceName + await LanguageManager.GetStringAsync("String_Space", token: token)
                                .ConfigureAwait(false));
                        setSpacedSourceNames.Add(
                            strSourceName + " ");
                    }

                    objImprovementList = new List<Improvement>(await objCharacter.Improvements.GetCountAsync(token).ConfigureAwait(false));
                    await objCharacter.Improvements.ForEachAsync(objImprovement =>
                    {
                        if (objImprovement.ImproveSource != objImprovementSource)
                            return;
                        if (lstSourceNames.Contains(objImprovement.SourceName) || setSpacedSourceNames.Any(x =>
                                objImprovement.SourceName.StartsWith(x, StringComparison.Ordinal)))
                        {
                            objImprovementList.Add(objImprovement);
                        }
                    }, token).ConfigureAwait(false);
                }
                else
                {
                    objImprovementList = await objCharacter.Improvements.ToListAsync(objImprovement =>
                        objImprovement.ImproveSource == objImprovementSource &&
                        lstSourceNames.Contains(objImprovement.SourceName), token: token).ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            return await RemoveImprovementsAsync(objCharacter, objImprovementList, token: token).ConfigureAwait(false);
        }

        /// <summary>
        /// Remove all the Improvements for an XML Node.
        /// </summary>
        /// <param name="objCharacter">Character from which improvements should be deleted.</param>
        /// <param name="lstImprovementSources">Types of object that granted these Improvements.</param>
        /// <param name="lstSourceNames">Names of the items that granted these Improvements.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task<decimal> RemoveImprovementsAsync(Character objCharacter,
            IReadOnlyCollection<Improvement.ImprovementSource> lstImprovementSources,
            IReadOnlyCollection<string> lstSourceNames, CancellationToken token = default)
        {
            // If there is no character object, don't try to remove any Improvements.
            if (objCharacter == null)
            {
                return 0;
            }
            // If there is nothing to remove, don't try to remove any Improvements
            if (lstImprovementSources == null || lstImprovementSources.Count == 0)
            {
                return 0;
            }
            if (lstSourceNames == null || lstSourceNames.Count == 0)
            {
                return 0;
            }

            Log.Debug("RemoveImprovements called with:" + Environment.NewLine + "lstImprovementSources = "
                      + lstImprovementSources + Environment.NewLine + "lstSourceNames = " + lstSourceNames);
            List<Improvement> objImprovementList;
            IAsyncDisposable objLocker = await objCharacter.LockObject.EnterReadLockAsync(token)
                .ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // A List of Improvements to hold all the items that will eventually be deleted.
                if (lstSourceNames.Any(x => x.IsGuid()))
                {
                    // Compatibility fix for when blnConcatSelectedValue was around
                    HashSet<string> setSpacedSourceNames = new HashSet<string>(lstSourceNames.Count);
                    foreach (string strSourceName in lstSourceNames)
                    {
                        if (!strSourceName.IsGuid())
                            continue;
                        setSpacedSourceNames.Add(
                            strSourceName + await LanguageManager.GetStringAsync("String_Space", token: token)
                                .ConfigureAwait(false));
                        setSpacedSourceNames.Add(
                            strSourceName + " ");
                    }

                    objImprovementList = new List<Improvement>(await objCharacter.Improvements.GetCountAsync(token).ConfigureAwait(false));
                    await objCharacter.Improvements.ForEachAsync(objImprovement =>
                    {
                        if (!lstImprovementSources.Contains(objImprovement.ImproveSource))
                            return;
                        if (lstSourceNames.Contains(objImprovement.SourceName) || setSpacedSourceNames.Any(x =>
                                objImprovement.SourceName.StartsWith(x, StringComparison.Ordinal)))
                        {
                            objImprovementList.Add(objImprovement);
                        }
                    }, token).ConfigureAwait(false);
                }
                else
                {
                    objImprovementList = await objCharacter.Improvements.ToListAsync(objImprovement =>
                        lstImprovementSources.Contains(objImprovement.ImproveSource) &&
                        lstSourceNames.Contains(objImprovement.SourceName), token: token).ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            return await RemoveImprovementsAsync(objCharacter, objImprovementList, token: token).ConfigureAwait(false);
        }

        /// <summary>
        /// Remove all the Improvements for an XML Node.
        /// </summary>
        /// <param name="objCharacter">Character from which improvements should be deleted.</param>
        /// <param name="objImprovementList">List of improvements to delete.</param>
        /// <param name="blnReapplyImprovements">Whether we're reapplying Improvements.</param>
        /// <param name="blnAllowDuplicatesFromSameSource">If we ignore checking whether a potential duplicate improvement has the same SourceName</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static decimal RemoveImprovements(Character objCharacter, IReadOnlyCollection<Improvement> objImprovementList,
                                                 bool blnReapplyImprovements = false,
                                                 bool blnAllowDuplicatesFromSameSource = false, CancellationToken token = default)
        {
            return Utils.SafelyRunSynchronously(() => RemoveImprovementsCoreAsync(false, objCharacter, objImprovementList, blnReapplyImprovements,
                                                    blnAllowDuplicatesFromSameSource, token), token);
        }

        /// <summary>
        /// Remove all the Improvements for an XML Node.
        /// </summary>
        /// <param name="objCharacter">Character from which improvements should be deleted.</param>
        /// <param name="objImprovementList">List of improvements to delete.</param>
        /// <param name="blnReapplyImprovements">Whether we're reapplying Improvements.</param>
        /// <param name="blnAllowDuplicatesFromSameSource">If we ignore checking whether a potential duplicate improvement has the same SourceName</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static Task<decimal> RemoveImprovementsAsync(Character objCharacter, IReadOnlyCollection<Improvement> objImprovementList,
                                                      bool blnReapplyImprovements = false,
                                                      bool blnAllowDuplicatesFromSameSource = false, CancellationToken token = default)
        {
            return RemoveImprovementsCoreAsync(false, objCharacter, objImprovementList, blnReapplyImprovements,
                                               blnAllowDuplicatesFromSameSource, token);
        }

        /// <summary>
        /// Remove all the Improvements for an XML Node.
        /// Uses flag hack method design outlined here to avoid locking:
        /// https://docs.microsoft.com/en-us/archive/msdn-magazine/2015/july/async-programming-brownfield-async-development
        /// </summary>
        /// <param name="blnSync">Flag for whether method should always use synchronous code or not.</param>
        /// <param name="objCharacter">Character from which improvements should be deleted.</param>
        /// <param name="objImprovementList">List of improvements to delete.</param>
        /// <param name="blnReapplyImprovements">Whether we're reapplying Improvements.</param>
        /// <param name="blnAllowDuplicatesFromSameSource">If we ignore checking whether a potential duplicate improvement has the same SourceName</param>
        /// <param name="token">Cancellation token to listen to.</param>
        private static async Task<decimal> RemoveImprovementsCoreAsync(bool blnSync, Character objCharacter,
                                                                       IReadOnlyCollection<Improvement> objImprovementList,
                                                                       bool blnReapplyImprovements,
                                                                       bool blnAllowDuplicatesFromSameSource,
                                                                       CancellationToken token = default)
        {
            Log.Debug("RemoveImprovements enter");
            //TODO: report a AI-operation (maybe with dependencies), so we get an idea how long this takes.
            // If there is no character object, don't try to remove any Improvements.
            if (objCharacter == null || objImprovementList == null)
            {
                Log.Debug("RemoveImprovements exit");
                return 0;
            }

            decimal decReturn = 0;
            IDisposable objLocker = null;
            IAsyncDisposable objLockerAsync = null;
            if (blnSync)
                objLocker = objCharacter.LockObject.EnterWriteLock(token);
            else
                objLockerAsync = await objCharacter.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // Note: As attractive as it may be to replace objImprovementList with an IEnumerable, we need to iterate through it twice for performance reasons

                // Now that we have all the applicable Improvements, remove them from the character.
                foreach (Improvement objImprovement in objImprovementList)
                {
                    // Remove the Improvement.
                    if (blnSync)
                    {
                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                        objCharacter.Improvements.Remove(objImprovement);
                    }
                    else
                    {
                        await objCharacter.Improvements.RemoveAsync(objImprovement, token).ConfigureAwait(false);
                    }
                    ClearCachedValue(objCharacter, objImprovement.ImproveType, objImprovement.ImprovedName, token: token);
                }

                // Now that the entire list is deleted from the character's improvements list, we do the checking of duplicates and extra effects
                foreach (Improvement objImprovement in objImprovementList)
                {
                    string strImprovedName = objImprovement.ImprovedName;
                    string strUniqueName = objImprovement.UniqueName;
                    Improvement.ImprovementType eImprovementType = objImprovement.ImproveType;
                    // See if the character has anything else that is granting them the same bonus as this improvement
                    bool blnHasDuplicate;
                    if (blnSync)
                    {
                        if (blnAllowDuplicatesFromSameSource)
                        {
                            // ReSharper disable once MethodHasAsyncOverload
                            blnHasDuplicate = objCharacter.Improvements.Any(
                                x => x.UniqueName == strUniqueName && x.ImprovedName == strImprovedName
                                                                   && x.ImproveType == eImprovementType
                                                                   && x.Enabled
                                                                   && !ReferenceEquals(x, objImprovement), token);
                            if (!blnHasDuplicate)
                            {
                                switch (eImprovementType)
                                {
                                    case Improvement.ImprovementType.Skillsoft:
                                    case Improvement.ImprovementType.Activesoft:
                                        // ReSharper disable once MethodHasAsyncOverload
                                        blnHasDuplicate = objCharacter.Improvements.Any(
                                            x => x.UniqueName == strUniqueName && x.ImprovedName == strImprovedName
                                                                               && x.ImproveType == Improvement.ImprovementType.Hardwire
                                                                               && x.Enabled
                                                                               && !ReferenceEquals(x, objImprovement), token);
                                        break;
                                    case Improvement.ImprovementType.Hardwire:
                                        // ReSharper disable once MethodHasAsyncOverload
                                        blnHasDuplicate = objCharacter.Improvements.Any(
                                            x => x.UniqueName == strUniqueName && x.ImprovedName == strImprovedName
                                                                               && (x.ImproveType
                                                                                   == Improvement.ImprovementType.Skillsoft
                                                                                   || x.ImproveType
                                                                                   == Improvement.ImprovementType
                                                                                       .Activesoft)
                                                                               && x.Enabled
                                                                               && !ReferenceEquals(x, objImprovement), token);
                                        break;
                                }
                            }
                        }
                        else
                        {
                            string strSourceName = objImprovement.SourceName;
                            // ReSharper disable once MethodHasAsyncOverload
                            blnHasDuplicate = await objCharacter.Improvements.AnyAsync(
                                x => x.UniqueName == strUniqueName && x.ImprovedName == strImprovedName
                                                                   && x.ImproveType == eImprovementType
                                                                   && x.SourceName != strSourceName
                                                                   && x.Enabled, token).ConfigureAwait(false);
                            if (!blnHasDuplicate)
                            {
                                switch (eImprovementType)
                                {
                                    case Improvement.ImprovementType.Skillsoft:
                                    case Improvement.ImprovementType.Activesoft:
                                        // ReSharper disable once MethodHasAsyncOverload
                                        blnHasDuplicate = await objCharacter.Improvements.AnyAsync(
                                            x => x.UniqueName == strUniqueName && x.ImprovedName == strImprovedName
                                                                               && x.ImproveType == Improvement.ImprovementType.Hardwire
                                                                               && x.SourceName != strSourceName
                                                                               && x.Enabled, token).ConfigureAwait(false);
                                        break;
                                    case Improvement.ImprovementType.Hardwire:
                                        blnHasDuplicate = await objCharacter.Improvements.AnyAsync(
                                            x => x.UniqueName == strUniqueName && x.ImprovedName == strImprovedName
                                                                               && (x.ImproveType
                                                                                   == Improvement.ImprovementType.Skillsoft
                                                                                   || x.ImproveType
                                                                                   == Improvement.ImprovementType
                                                                                       .Activesoft)
                                                                               && x.SourceName != strSourceName
                                                                               && x.Enabled, token).ConfigureAwait(false);
                                        break;
                                }
                            }
                        }
                    }
                    else if (blnAllowDuplicatesFromSameSource)
                    {
                        blnHasDuplicate = await objCharacter.Improvements.AnyAsync(
                                                                x => x.UniqueName == strUniqueName
                                                                     && x.ImprovedName == strImprovedName
                                                                     && x.ImproveType == eImprovementType
                                                                     && x.Enabled
                                                                     && !ReferenceEquals(x, objImprovement), token)
                                                            .ConfigureAwait(false);
                        if (!blnHasDuplicate)
                        {
                            switch (eImprovementType)
                            {
                                case Improvement.ImprovementType.Skillsoft:
                                case Improvement.ImprovementType.Activesoft:
                                    blnHasDuplicate = await objCharacter.Improvements.AnyAsync(
                                        x => x.UniqueName == strUniqueName && x.ImprovedName == strImprovedName
                                                                           && x.ImproveType == Improvement.ImprovementType.Hardwire
                                                                           && x.Enabled
                                                                           && !ReferenceEquals(x, objImprovement), token)
                                                                        .ConfigureAwait(false);
                                    break;
                                case Improvement.ImprovementType.Hardwire:
                                    blnHasDuplicate = await objCharacter.Improvements.AnyAsync(
                                        x => x.UniqueName == strUniqueName && x.ImprovedName == strImprovedName
                                                                           && (x.ImproveType
                                                                               == Improvement.ImprovementType.Skillsoft
                                                                               || x.ImproveType
                                                                               == Improvement.ImprovementType
                                                                                   .Activesoft)
                                                                           && x.Enabled
                                                                           && !ReferenceEquals(x, objImprovement), token)
                                                                        .ConfigureAwait(false);
                                    break;
                            }
                        }
                    }
                    else
                    {
                        string strSourceName = objImprovement.SourceName;
                        blnHasDuplicate = await objCharacter.Improvements.AnyAsync(
                            x => x.UniqueName == strUniqueName && x.ImprovedName == strImprovedName
                                                               && x.ImproveType == eImprovementType
                                                               && x.SourceName != strSourceName
                                                               && x.Enabled, token).ConfigureAwait(false);
                    }

                    switch (eImprovementType)
                    {
                        case Improvement.ImprovementType.SkillLevel:
                            //TODO: Come back here and figure out wtf this did? Think it removed nested lifemodule skills? //Didn't this handle the collapsing knowledge skills thing?
                            //for (int i = _objCharacter.SkillsSection.Skills.Count - 1; i >= 0; i--)
                            //{
                            //    //wrote as foreach first, modify collection, not want rename
                            //    Skill skill = _objCharacter.SkillsSection.Skills[i];
                            //    for (int j = skill.Fold.Count - 1; j >= 0; j--)
                            //    {
                            //        Skill fold = skill.Fold[i];
                            //        if (fold.Id.ToString() == strImprovedName)
                            //        {
                            //            skill.Free(fold);
                            //            _objCharacter.SkillsSection.Skills.Remove(fold);
                            //        }
                            //    }

                            //    if (skill.Id.ToString() == strImprovedName)
                            //    {
                            //        while(skill.Fold.Count > 0) skill.Free(skill.Fold[0]);
                            //        //empty list, can't call clear as exposed list is RO

                            //        _objCharacter.SkillsSection.Skills.Remove(skill);
                            //    }
                            //}
                            break;

                        case Improvement.ImprovementType.SkillsoftAccess:
                            if (!blnHasDuplicate)
                            {
                                if (blnSync)
                                {
                                    // ReSharper disable once MethodHasAsyncOverload
                                    objCharacter.SkillsSection.KnowsoftSkills.ForEach(objKnowledgeSkill =>
                                    {
                                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                        objCharacter.SkillsSection.KnowledgeSkills.Remove(objKnowledgeSkill);
                                    }, token);
                                }
                                else
                                {
                                    await objCharacter.SkillsSection.KnowsoftSkills.ForEachAsync(
                                        objKnowledgeSkill => objCharacter.SkillsSection.KnowledgeSkills
                                                                         .RemoveAsync(objKnowledgeSkill, token)
                                                                         , token).ConfigureAwait(false);
                                }
                            }

                            break;

                        case Improvement.ImprovementType.Activesoft:
                            if (!blnHasDuplicate && !blnReapplyImprovements)
                            {
                                if (blnSync)
                                {
                                    SkillsSection objSkillsSection = objCharacter.SkillsSection;
                                    // ReSharper disable once MethodHasAsyncOverload
                                    Skill objSkill = objSkillsSection.GetActiveSkill(strImprovedName, token: token);
                                    if (objSkill?.IsExoticSkill == true)
                                    {
                                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                        objSkillsSection.Skills.Remove(objSkill);
                                    }
                                }
                                else
                                {
                                    SkillsSection objSkillsSection = await objCharacter.GetSkillsSectionAsync(token).ConfigureAwait(false);
                                    Skill objSkill = await objSkillsSection.GetActiveSkillAsync(strImprovedName, token).ConfigureAwait(false);
                                    if (objSkill?.IsExoticSkill == true)
                                    {
                                        await (await objSkillsSection.GetSkillsAsync(token).ConfigureAwait(false)).RemoveAsync(objSkill, token).ConfigureAwait(false);
                                    }
                                }
                            }

                            break;

                        case Improvement.ImprovementType.Skillsoft:
                            if (!blnHasDuplicate && !blnReapplyImprovements)
                            {
                                if (blnSync)
                                {
                                    SkillsSection objSkillsSection = objCharacter.SkillsSection;
                                    ThreadSafeBindingList<KnowledgeSkill> lstKnowledgeSkills = objSkillsSection.KnowledgeSkills;
                                    lstKnowledgeSkills.RemoveAll(x => x.InternalId == strImprovedName, token: token);
                                    ThreadSafeBindingList<KnowledgeSkill> lstKnowsoftSkills = objSkillsSection.KnowsoftSkills;
                                    for (int i = lstKnowsoftSkills.Count - 1; i >= 0; --i)
                                    {
                                        KnowledgeSkill objSkill = lstKnowsoftSkills[i];
                                        if (objSkill.InternalId == strImprovedName)
                                        {
                                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                            lstKnowledgeSkills.Remove(objSkill);
                                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                            lstKnowsoftSkills.RemoveAt(i);
                                        }
                                    }
                                }
                                else
                                {
                                    SkillsSection objSkillsSection = await objCharacter.GetSkillsSectionAsync(token).ConfigureAwait(false);
                                    ThreadSafeBindingList<KnowledgeSkill> lstKnowledgeSkills = await objSkillsSection.GetKnowledgeSkillsAsync(token).ConfigureAwait(false);
                                    await lstKnowledgeSkills.RemoveAllAsync(x => x.InternalId == strImprovedName, token: token).ConfigureAwait(false);
                                    ThreadSafeBindingList<KnowledgeSkill> lstKnowsoftSkills = await objSkillsSection.GetKnowsoftSkillsAsync(token).ConfigureAwait(false);
                                    for (int i = await lstKnowsoftSkills.GetCountAsync(token).ConfigureAwait(false) - 1; i >= 0; --i)
                                    {
                                        KnowledgeSkill objSkill = await lstKnowsoftSkills.GetValueAtAsync(i, token).ConfigureAwait(false);
                                        if (objSkill.InternalId == strImprovedName)
                                        {
                                            await Task.WhenAll(
                                                lstKnowledgeSkills.RemoveAsync(objSkill, token),
                                                lstKnowsoftSkills.RemoveAtAsync(i, token)).ConfigureAwait(false);
                                        }
                                    }
                                }
                            }

                            break;

                        case Improvement.ImprovementType.Hardwire:
                            if (!blnHasDuplicate && !blnReapplyImprovements)
                            {
                                if (blnSync)
                                {
                                    SkillsSection objSkillsSection = objCharacter.SkillsSection;
                                    // ReSharper disable once MethodHasAsyncOverload
                                    Skill objSkill = objSkillsSection.GetActiveSkill(strImprovedName, token: token);
                                    if (objSkill == null)
                                    {
                                        ThreadSafeBindingList<KnowledgeSkill> lstKnowledgeSkills = objSkillsSection.KnowledgeSkills;
                                        lstKnowledgeSkills.RemoveAll(x => x.InternalId == strImprovedName, token: token);
                                        ThreadSafeBindingList<KnowledgeSkill> lstKnowsoftSkills = objSkillsSection.KnowsoftSkills;
                                        for (int i = lstKnowsoftSkills.Count - 1; i >= 0; --i)
                                        {
                                            KnowledgeSkill objKnoSkill = lstKnowsoftSkills[i];
                                            if (objKnoSkill.InternalId == strImprovedName)
                                            {
                                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                lstKnowledgeSkills.Remove(objKnoSkill);
                                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                lstKnowsoftSkills.RemoveAt(i);
                                            }
                                        }
                                    }
                                    else if (objSkill.IsExoticSkill)
                                    {
                                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                        objCharacter.SkillsSection.Skills.Remove(objSkill);
                                    }
                                }
                                else
                                {
                                    SkillsSection objSkillsSection = await objCharacter.GetSkillsSectionAsync(token).ConfigureAwait(false);
                                    Skill objSkill = await objSkillsSection.GetActiveSkillAsync(strImprovedName, token).ConfigureAwait(false);
                                    if (objSkill == null)
                                    {
                                        ThreadSafeBindingList<KnowledgeSkill> lstKnowledgeSkills = await objSkillsSection.GetKnowledgeSkillsAsync(token).ConfigureAwait(false);
                                        await lstKnowledgeSkills.RemoveAllAsync(x => x.InternalId == strImprovedName, token: token).ConfigureAwait(false);
                                        ThreadSafeBindingList<KnowledgeSkill> lstKnowsoftSkills = await objSkillsSection.GetKnowsoftSkillsAsync(token).ConfigureAwait(false);
                                        for (int i = await lstKnowsoftSkills.GetCountAsync(token).ConfigureAwait(false) - 1; i >= 0; --i)
                                        {
                                            KnowledgeSkill objKnoSkill = await lstKnowsoftSkills.GetValueAtAsync(i, token).ConfigureAwait(false);
                                            if (objKnoSkill.InternalId == strImprovedName)
                                            {
                                                await Task.WhenAll(
                                                    lstKnowledgeSkills.RemoveAsync(objKnoSkill, token),
                                                    lstKnowsoftSkills.RemoveAtAsync(i, token)).ConfigureAwait(false);
                                            }
                                        }
                                    }
                                    else if (objSkill.IsExoticSkill)
                                    {
                                        await objCharacter.SkillsSection.Skills.RemoveAsync(objSkill, token).ConfigureAwait(false);
                                    }
                                }
                            }

                            break;

                        case Improvement.ImprovementType.Attribute:
                            // Determine if access to any Special Attributes have been lost.
                            if (string.Equals(strUniqueName, "enableattribute", StringComparison.OrdinalIgnoreCase) && !blnHasDuplicate
                                                                   && !blnReapplyImprovements)
                            {
                                switch (strImprovedName.ToUpperInvariant())
                                {
                                    case "MAG":
                                        await objCharacter.SetMAGEnabledAsync(false, token).ConfigureAwait(false);
                                        break;

                                    case "RES":
                                        await objCharacter.SetRESEnabledAsync(false, token).ConfigureAwait(false);
                                        break;

                                    case "DEP":
                                        await objCharacter.SetDEPEnabledAsync(false, token).ConfigureAwait(false);
                                        break;
                                }
                            }

                            break;

                        case Improvement.ImprovementType.SpecialTab:
                            // Determine if access to any special tabs have been lost.
                            if (!blnHasDuplicate && !blnReapplyImprovements)
                            {
                                switch (strUniqueName.ToUpperInvariant())
                                {
                                    case "ENABLETAB":
                                        switch (strImprovedName.ToUpperInvariant())
                                        {
                                            case "MAGICIAN":
                                                if (blnSync)
                                                    objCharacter.MagicianEnabled = false;
                                                else
                                                    await objCharacter.SetMagicianEnabledAsync(false, token).ConfigureAwait(false);
                                                break;

                                            case "ADEPT":
                                                if (blnSync)
                                                    objCharacter.AdeptEnabled = false;
                                                else
                                                    await objCharacter.SetAdeptEnabledAsync(false, token).ConfigureAwait(false);
                                                break;

                                            case "TECHNOMANCER":
                                                if (blnSync)
                                                    objCharacter.TechnomancerEnabled = false;
                                                else
                                                    await objCharacter.SetTechnomancerEnabledAsync(false, token).ConfigureAwait(false);
                                                break;

                                            case "ADVANCED PROGRAMS":
                                                if (blnSync)
                                                    objCharacter.AdvancedProgramsEnabled = false;
                                                else
                                                    await objCharacter.SetAdvancedProgramsEnabledAsync(false, token).ConfigureAwait(false);
                                                break;

                                            case "CRITTER":
                                                if (blnSync)
                                                    objCharacter.CritterEnabled = false;
                                                else
                                                    await objCharacter.SetCritterEnabledAsync(false, token).ConfigureAwait(false);
                                                break;
                                        }

                                        break;
                                    // Determine if access to any special tabs has been regained
                                    case "DISABLETAB":
                                        switch (strImprovedName)
                                        {
                                            case "CYBERWARE":
                                                if (blnSync)
                                                    objCharacter.CyberwareDisabled = false;
                                                else
                                                    await objCharacter.SetCyberwareDisabledAsync(false, token).ConfigureAwait(false);
                                                break;

                                            case "INITIATION":
                                                if (blnSync)
                                                    objCharacter.InitiationForceDisabled = false;
                                                else
                                                    await objCharacter.SetInitiationForceDisabledAsync(false, token).ConfigureAwait(false);
                                                break;
                                        }

                                        break;
                                }
                            }

                            break;

                        case Improvement.ImprovementType.PrototypeTranshuman:
                            // Legacy compatibility
                            if (string.IsNullOrEmpty(strImprovedName))
                            {
                                if (!blnHasDuplicate)
                                {
                                    if (blnSync)
                                        objCharacter.PrototypeTranshuman = 0;
                                    else
                                        await objCharacter.SetPrototypeTranshumanAsync(0, token).ConfigureAwait(false);
                                }
                            }
                            else if (blnSync)
                            {
                                // ReSharper disable once MethodHasAsyncOverload
                                decimal decValue = ValueToDec(objCharacter, strImprovedName, objImprovement.Rating, token);
                                objCharacter.PrototypeTranshuman -= decValue;
                            }
                            else
                            {
                                decimal decValue = await ValueToDecAsync(objCharacter, strImprovedName, objImprovement.Rating, token).ConfigureAwait(false);
                                await objCharacter.ModifyPrototypeTranshumanAsync(-decValue, token).ConfigureAwait(false);
                            }

                            break;

                        case Improvement.ImprovementType.Adapsin:
                        {
                            if (!blnHasDuplicate && !blnReapplyImprovements)
                            {
                                foreach (Cyberware objCyberware in blnSync
                                             ? objCharacter.Cyberware.GetAllDescendants(
                                                 x => x.Children, token)
                                             : await (await objCharacter.GetCyberwareAsync(token).ConfigureAwait(false)).GetAllDescendantsAsync(
                                                 x => x.GetChildrenAsync(token), token).ConfigureAwait(false))
                                {
                                    Grade objOldGrade = blnSync ? objCyberware.Grade : await objCyberware.GetGradeAsync(token).ConfigureAwait(false);
                                    if (objOldGrade.Adapsin)
                                    {
                                        string strNewName = objOldGrade.Name.FastEscapeOnceFromEnd("(Adapsin)")
                                            .Trim();
                                        // Determine which GradeList to use for the Cyberware.
                                        if (blnSync)
                                        {
                                            // ReSharper disable once MethodHasAsyncOverload
                                            objCyberware.Grade = objCharacter.GetGradeByName(objCyberware.SourceType, strNewName, true, token);
                                        }
                                        else
                                            await objCyberware.SetGradeAsync(await objCharacter.GetGradeByNameAsync(objCyberware.SourceType, strNewName, true, token).ConfigureAwait(false), token).ConfigureAwait(false);
                                    }
                                }
                            }
                        }
                            break;

                        case Improvement.ImprovementType.AddContact:
                            Contact objNewContact = blnSync
                                ? objCharacter.Contacts.FirstOrDefault(c => c.UniqueId == strImprovedName)
                                : await objCharacter.Contacts
                                                    .FirstOrDefaultAsync(c => c.UniqueId == strImprovedName, token)
                                                    .ConfigureAwait(false);
                            if (objNewContact != null)
                            {
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    objCharacter.Contacts.Remove(objNewContact);
                                else
                                    await objCharacter.Contacts.RemoveAsync(objNewContact, token).ConfigureAwait(false);
                            }

                            break;

                        case Improvement.ImprovementType.Art:
                            Art objArt = blnSync
                                ? objCharacter.Arts.FirstOrDefault(x => x.InternalId == strImprovedName)
                                : await objCharacter.Arts
                                                    .FirstOrDefaultAsync(x => x.InternalId == strImprovedName, token)
                                                    .ConfigureAwait(false);
                            if (objArt != null)
                            {
                                decReturn += blnSync
                                    // ReSharper disable once MethodHasAsyncOverload
                                    ? RemoveImprovements(objCharacter, objArt.SourceType, objArt.InternalId, token: token)
                                    : await RemoveImprovementsAsync(objCharacter, objArt.SourceType, objArt.InternalId,
                                                                    token).ConfigureAwait(false);
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    objCharacter.Arts.Remove(objArt);
                                else
                                    await objCharacter.Arts.RemoveAsync(objArt, token).ConfigureAwait(false);
                            }

                            break;

                        case Improvement.ImprovementType.Metamagic:
                        case Improvement.ImprovementType.Echo:
                            Metamagic objMetamagic = blnSync
                                ? objCharacter.Metamagics.FirstOrDefault(x => x.InternalId == strImprovedName)
                                : await objCharacter.Metamagics
                                                    .FirstOrDefaultAsync(x => x.InternalId == strImprovedName, token)
                                                    .ConfigureAwait(false);
                            if (objMetamagic != null)
                            {
                                decReturn += blnSync
                                    // ReSharper disable once MethodHasAsyncOverload
                                    ? RemoveImprovements(objCharacter,
                                                         objImprovement.ImproveType
                                                         == Improvement.ImprovementType.Metamagic
                                                             ? Improvement.ImprovementSource.Metamagic
                                                             : Improvement.ImprovementSource.Echo,
                                                         objMetamagic.InternalId, token: token)
                                    : await RemoveImprovementsAsync(objCharacter,
                                                                    objImprovement.ImproveType
                                                                    == Improvement.ImprovementType.Metamagic
                                                                        ? Improvement.ImprovementSource.Metamagic
                                                                        : Improvement.ImprovementSource.Echo,
                                                                    objMetamagic.InternalId, token)
                                        .ConfigureAwait(false);
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    objCharacter.Metamagics.Remove(objMetamagic);
                                else
                                    await objCharacter.Metamagics.RemoveAsync(objMetamagic, token)
                                                      .ConfigureAwait(false);
                            }

                            break;

                        case Improvement.ImprovementType.LimitModifier:
                            LimitModifier limitMod = blnSync
                                ? objCharacter.LimitModifiers.FirstOrDefault(x => x.InternalId == strImprovedName)
                                : await objCharacter.LimitModifiers.FirstOrDefaultAsync(
                                    x => x.InternalId == strImprovedName, token).ConfigureAwait(false);
                            if (limitMod != null)
                            {
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    objCharacter.LimitModifiers.Remove(limitMod);
                                else
                                    await objCharacter.LimitModifiers.RemoveAsync(limitMod, token)
                                                      .ConfigureAwait(false);
                            }

                            break;

                        case Improvement.ImprovementType.CritterPower:
                            CritterPower objCritterPower = blnSync
                                ? objCharacter.CritterPowers.FirstOrDefault(
                                    x => x.InternalId == strImprovedName
                                         || (x.Name == strImprovedName && x.Extra == strUniqueName))
                                : await objCharacter.CritterPowers.FirstOrDefaultAsync(
                                                        x => x.InternalId == strImprovedName
                                                             || (x.Name == strImprovedName && x.Extra == strUniqueName),
                                                        token)
                                                    .ConfigureAwait(false);
                            if (objCritterPower != null)
                            {
                                decReturn += blnSync
                                    // ReSharper disable once MethodHasAsyncOverload
                                    ? RemoveImprovements(objCharacter, Improvement.ImprovementSource.CritterPower,
                                                         objCritterPower.InternalId, token: token)
                                    : await RemoveImprovementsAsync(objCharacter,
                                                                    Improvement.ImprovementSource.CritterPower,
                                                                    objCritterPower.InternalId, token)
                                        .ConfigureAwait(false);
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    objCharacter.CritterPowers.Remove(objCritterPower);
                                else
                                    await objCharacter.CritterPowers.RemoveAsync(objCritterPower, token)
                                                      .ConfigureAwait(false);
                            }

                            break;

                        case Improvement.ImprovementType.MentorSpirit:
                        case Improvement.ImprovementType.Paragon:
                            MentorSpirit objMentor = blnSync
                                ? objCharacter.MentorSpirits.FirstOrDefault(x => x.InternalId == strImprovedName)
                                : await objCharacter.MentorSpirits
                                                    .FirstOrDefaultAsync(x => x.InternalId == strImprovedName, token)
                                                    .ConfigureAwait(false);
                            if (objMentor != null)
                            {
                                decReturn += blnSync
                                    // ReSharper disable once MethodHasAsyncOverload
                                    ? RemoveImprovements(objCharacter, Improvement.ImprovementSource.MentorSpirit,
                                                         objMentor.InternalId, token: token)
                                    : await RemoveImprovementsAsync(objCharacter,
                                                                    Improvement.ImprovementSource.MentorSpirit,
                                                                    objMentor.InternalId, token).ConfigureAwait(false);
                                if (blnSync)
                                {
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    objCharacter.MentorSpirits.Remove(objMentor);
                                    // ReSharper disable once MethodHasAsyncOverload
                                    objMentor.Dispose();
                                }
                                else
                                {
                                    await objCharacter.MentorSpirits.RemoveAsync(objMentor, token)
                                                      .ConfigureAwait(false);
                                    await objMentor.DisposeAsync().ConfigureAwait(false);
                                }
                            }

                            break;

                        case Improvement.ImprovementType.Gear:
                            Gear objGear = blnSync
                                ? objCharacter.Gear.FirstOrDefault(x => x.InternalId == strImprovedName)
                                : await objCharacter.Gear
                                                    .FirstOrDefaultAsync(x => x.InternalId == strImprovedName, token)
                                                    .ConfigureAwait(false);
                            if (objGear != null)
                            {
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    decReturn += objGear.TotalCost + objGear.DeleteGear();
                                else
                                    decReturn += await objGear.GetTotalCostAsync(token).ConfigureAwait(false)
                                                 + await objGear.DeleteGearAsync(token: token).ConfigureAwait(false);
                            }

                            break;

                        case Improvement.ImprovementType.Weapon:
                        {
                            Weapon objWeapon
                                = blnSync
                                    ? objCharacter.Weapons.DeepFirstOrDefault(x => x.Children,
                                          x => x.InternalId == strImprovedName, token)
                                      ??
                                      objCharacter.Vehicles.FindVehicleWeapon(strImprovedName, token)
                                    : await objCharacter.Weapons.DeepFirstOrDefaultAsync(x => x.Children,
                                          x => x.InternalId == strImprovedName, token: token).ConfigureAwait(false)
                                      ?? (await objCharacter.Vehicles.FindVehicleWeaponAsync(strImprovedName, token)
                                          .ConfigureAwait(false))
                                      .Item1;
                            if (objWeapon != null)
                            {
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    decReturn += objWeapon.TotalCost + objWeapon.DeleteWeapon();
                                else
                                    decReturn += await objWeapon.GetTotalCostAsync(token).ConfigureAwait(false)
                                                 + await objWeapon.DeleteWeaponAsync(token: token).ConfigureAwait(false);
                            }
                        }
                            break;

                        case Improvement.ImprovementType.Spell:
                            Spell objSpell = blnSync
                                ? objCharacter.Spells.FirstOrDefault(x => x.InternalId == strImprovedName)
                                : await objCharacter.Spells
                                                    .FirstOrDefaultAsync(x => x.InternalId == strImprovedName, token)
                                                    .ConfigureAwait(false);
                            if (objSpell != null)
                            {
                                decReturn += blnSync
                                    // ReSharper disable once MethodHasAsyncOverload
                                    ? RemoveImprovements(objCharacter, Improvement.ImprovementSource.Spell,
                                                         objSpell.InternalId, token: token)
                                    : await RemoveImprovementsAsync(objCharacter, Improvement.ImprovementSource.Spell,
                                                                    objSpell.InternalId, token).ConfigureAwait(false);
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    objCharacter.Spells.Remove(objSpell);
                                else
                                    await objCharacter.Spells.RemoveAsync(objSpell, token).ConfigureAwait(false);
                            }

                            break;

                        case Improvement.ImprovementType.ComplexForm:
                            ComplexForm objComplexForm = blnSync
                                ? objCharacter.ComplexForms.FirstOrDefault(x => x.InternalId == strImprovedName)
                                : await objCharacter.ComplexForms
                                                    .FirstOrDefaultAsync(x => x.InternalId == strImprovedName, token)
                                                    .ConfigureAwait(false);
                            if (objComplexForm != null)
                            {
                                decReturn += blnSync
                                    // ReSharper disable once MethodHasAsyncOverload
                                    ? RemoveImprovements(objCharacter, Improvement.ImprovementSource.ComplexForm,
                                                         objComplexForm.InternalId, token: token)
                                    : await RemoveImprovementsAsync(objCharacter,
                                                                    Improvement.ImprovementSource.ComplexForm,
                                                                    objComplexForm.InternalId, token)
                                        .ConfigureAwait(false);
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    objCharacter.ComplexForms.Remove(objComplexForm);
                                else
                                    await objCharacter.ComplexForms.RemoveAsync(objComplexForm, token)
                                                      .ConfigureAwait(false);
                            }

                            break;

                        case Improvement.ImprovementType.MartialArt:
                            MartialArt objMartialArt
                                = blnSync
                                    ? objCharacter.MartialArts.FirstOrDefault(x => x.InternalId == strImprovedName)
                                    : await objCharacter.MartialArts.FirstOrDefaultAsync(
                                        x => x.InternalId == strImprovedName, token).ConfigureAwait(false);
                            if (objMartialArt != null)
                            {
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    decReturn += objMartialArt.DeleteMartialArt();
                                else
                                    decReturn += await objMartialArt.DeleteMartialArtAsync(token: token).ConfigureAwait(false);
                            }

                            break;

                        case Improvement.ImprovementType.SpecialSkills:
                            if (!blnHasDuplicate)
                            {
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverload
                                    objCharacter.SkillsSection.RemoveSkills(
                                        (SkillsSection.FilterOption)Enum.Parse(typeof(SkillsSection.FilterOption),
                                            strImprovedName), objImprovement.Target,
                                        !blnReapplyImprovements && objCharacter.Created, token: token);
                                else
                                    await objCharacter.SkillsSection.RemoveSkillsAsync(
                                            (SkillsSection.FilterOption)Enum.Parse(typeof(SkillsSection.FilterOption),
                                                strImprovedName), objImprovement.Target,
                                            !blnReapplyImprovements &&
                                            await objCharacter.GetCreatedAsync(token).ConfigureAwait(false), token)
                                        .ConfigureAwait(false);
                            }

                            break;

                        case Improvement.ImprovementType.SpecificQuality:
                            Quality objQuality = blnSync
                                ? objCharacter.Qualities.FirstOrDefault(
                                    objLoopQuality => objLoopQuality.InternalId == strImprovedName)
                                : await objCharacter.Qualities
                                                    .FirstOrDefaultAsync(
                                                        objLoopQuality => objLoopQuality.InternalId == strImprovedName,
                                                        token).ConfigureAwait(false);
                            if (objQuality != null)
                            {
                                // We need to add in the return cost of deleting the quality, so call this manually
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverload
                                    decReturn += objQuality.DeleteQuality(token: token);
                                else
                                    decReturn += await objQuality.DeleteQualityAsync(token: token)
                                                                 .ConfigureAwait(false);
                            }

                            break;

                        case Improvement.ImprovementType.SkillSpecialization:
                        case Improvement.ImprovementType.SkillExpertise:
                        {
                            Skill objSkill = blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? objCharacter.SkillsSection.GetActiveSkill(strImprovedName, token)
                                : await objCharacter.SkillsSection.GetActiveSkillAsync(strImprovedName, token)
                                                    .ConfigureAwait(false);
                            if (objSkill != null)
                            {
                                SkillSpecialization objSkillSpec = blnSync
                                    ? strUniqueName.IsGuid()
                                        ? objSkill.Specializations.FirstOrDefault(
                                            x => x.InternalId == strUniqueName)
                                        // Kept for legacy reasons
                                        : objSkill.Specializations.FirstOrDefault(x => x.Name == strUniqueName)
                                    : strUniqueName.IsGuid()
                                        ? await objSkill.Specializations.FirstOrDefaultAsync(
                                            x => x.InternalId == strUniqueName, token).ConfigureAwait(false)
                                        // Kept for legacy reasons
                                        : await objSkill.Specializations.FirstOrDefaultAsync(
                                            async x => await x.GetNameAsync(token).ConfigureAwait(false) ==
                                                       strUniqueName, token).ConfigureAwait(false);
                                if (objSkillSpec != null)
                                {
                                    if (blnSync)
                                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                        objSkill.Specializations.Remove(objSkillSpec);
                                    else
                                        await objSkill.Specializations.RemoveAsync(objSkillSpec, token)
                                                      .ConfigureAwait(false);
                                }
                            }
                        }
                            break;

                        case Improvement.ImprovementType.AIProgram:
                            AIProgram objProgram = blnSync
                                ? objCharacter.AIPrograms.FirstOrDefault(
                                    objLoopProgram => objLoopProgram.InternalId == strImprovedName)
                                : await objCharacter.AIPrograms
                                                    .FirstOrDefaultAsync(
                                                        objLoopProgram => objLoopProgram.InternalId == strImprovedName,
                                                        token).ConfigureAwait(false);
                            if (objProgram != null)
                            {
                                decReturn += blnSync
                                    // ReSharper disable once MethodHasAsyncOverload
                                    ? RemoveImprovements(objCharacter, Improvement.ImprovementSource.AIProgram,
                                                         objProgram.InternalId, token)
                                    : await RemoveImprovementsAsync(objCharacter,
                                                                    Improvement.ImprovementSource.AIProgram,
                                                                    objProgram.InternalId, token).ConfigureAwait(false);
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    objCharacter.AIPrograms.Remove(objProgram);
                                else
                                    await objCharacter.AIPrograms.RemoveAsync(objProgram, token).ConfigureAwait(false);
                            }

                            break;

                        case Improvement.ImprovementType.AdeptPowerFreeLevels:
                        case Improvement.ImprovementType.AdeptPowerFreePoints:
                            // Get the power improved by this improvement
                            Power objImprovedPower = blnSync
                                ? objCharacter.Powers.FirstOrDefault(
                                    objPower => objPower.Name == strImprovedName && objPower.Extra == strUniqueName)
                                : await objCharacter.Powers
                                                    .FirstOrDefaultAsync(
                                                        objPower => objPower.Name == strImprovedName
                                                                    && objPower.Extra == strUniqueName, token)
                                                    .ConfigureAwait(false);
                            if (objImprovedPower != null)
                            {
                                if (blnSync)
                                {
                                    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                                    if (objImprovement.ImproveType == Improvement.ImprovementType.AdeptPowerFreeLevels)
                                        objImprovedPower.OnMultiplePropertyChanged(
                                            nameof(Power.TotalRating), nameof(Power.FreeLevels));
                                    else
                                        objImprovedPower.OnMultiplePropertyChanged(
                                            nameof(Power.TotalRating), nameof(Power.FreePoints));

                                    if (objImprovedPower.TotalRating <= 0)
                                    {
                                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                        objImprovedPower.DeletePower();
                                    }
                                }
                                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                                else
                                {
                                    if (objImprovement.ImproveType == Improvement.ImprovementType.AdeptPowerFreeLevels)
                                        await objImprovedPower.OnMultiplePropertyChangedAsync(token,
                                                nameof(Power.TotalRating), nameof(Power.FreeLevels))
                                            .ConfigureAwait(false);
                                    else
                                        await objImprovedPower.OnMultiplePropertyChangedAsync(token,
                                                nameof(Power.TotalRating), nameof(Power.FreePoints))
                                            .ConfigureAwait(false);

                                    if (await objImprovedPower.GetTotalRatingAsync(token).ConfigureAwait(false) <= 0)
                                    {
                                        await objImprovedPower.DeletePowerAsync(token).ConfigureAwait(false);
                                    }
                                }
                            }

                            break;

                        case Improvement.ImprovementType.FreeWare:
                        {
                            // Specific to AddWare of an essence hole or antihole: because these can be created or destroyed after the improvement has been added, the name that is saved will be the source ID of the hole or antihole instead of the internal id
                            if (strImprovedName == Cyberware.EssenceHoleGuidString)
                            {
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    objCharacter.DecreaseEssenceHole(objImprovement.Rating);
                                else
                                    await objCharacter.DecreaseEssenceHoleAsync(objImprovement.Rating, token: token).ConfigureAwait(false);
                            }
                            else if (strImprovedName == Cyberware.EssenceAntiHoleGuidString)
                            {
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    objCharacter.IncreaseEssenceHole(objImprovement.Rating);
                                else
                                    await objCharacter.IncreaseEssenceHoleAsync(objImprovement.Rating, token: token).ConfigureAwait(false);
                            }
                            else
                            {
                                Cyberware objCyberware = blnSync
                                    ? objCharacter.Cyberware.FirstOrDefault(o => o.InternalId == strImprovedName)
                                    : await objCharacter.Cyberware
                                        .FirstOrDefaultAsync(o => o.InternalId == strImprovedName, token)
                                        .ConfigureAwait(false);
                                if (objCyberware != null)
                                {
                                    if (blnSync)
                                    {
                                        if (objCyberware.SourceID == Cyberware.EssenceHoleGUID)
                                        {
                                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                            objCharacter.DecreaseEssenceHole(objImprovement.Rating);
                                        }
                                        else if (objCyberware.SourceID == Cyberware.EssenceAntiHoleGUID)
                                        {
                                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                            objCharacter.IncreaseEssenceHole(objImprovement.Rating);
                                        }
                                        else
                                        {
                                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                            decReturn += objCyberware.TotalCost + objCyberware.DeleteCyberware();
                                        }
                                    }
                                    else if (await objCyberware.GetSourceIDAsync(token).ConfigureAwait(false) ==
                                             Cyberware.EssenceHoleGUID)
                                    {
                                        await objCharacter.DecreaseEssenceHoleAsync(objImprovement.Rating,
                                            token: token).ConfigureAwait(false);
                                    }
                                    else if (await objCyberware.GetSourceIDAsync(token).ConfigureAwait(false) ==
                                             Cyberware.EssenceAntiHoleGUID)
                                    {
                                        await objCharacter.IncreaseEssenceHoleAsync(objImprovement.Rating,
                                            token: token).ConfigureAwait(false);
                                    }
                                    else
                                    {
                                        decReturn += await objCyberware.GetTotalCostAsync(token).ConfigureAwait(false) +
                                                     await objCyberware
                                                         .DeleteCyberwareAsync(token: token).ConfigureAwait(false);
                                    }
                                }
                                // Check for the specific case where we added an essence hole or antihole and it perfectly canceled out an existing antihole or hole, leaving no cyberware to record in the improvement
                                else if (!string.IsNullOrEmpty(strImprovedName))
                                {
                                    XPathNavigator objDataNode =
                                        (blnSync
                                            // ReSharper disable once MethodHasAsyncOverload
                                            ? objCharacter.LoadDataXPath("bioware.xml", token: token)
                                            : await objCharacter.LoadDataXPathAsync("bioware.xml", token: token)
                                                .ConfigureAwait(false)).TryGetNodeByNameOrId(
                                            "/chummer/biowares/bioware",
                                            strImprovedName) ??
                                        (blnSync
                                            // ReSharper disable once MethodHasAsyncOverload
                                            ? objCharacter.LoadDataXPath("cyberware.xml", token: token)
                                            : await objCharacter.LoadDataXPathAsync("cyberware.xml", token: token)
                                                .ConfigureAwait(false)).TryGetNodeByNameOrId(
                                            "/chummer/cyberwares/cyberware",
                                            strImprovedName);
                                    if (objDataNode != null)
                                    {
                                        switch (objDataNode.SelectSingleNodeAndCacheExpression("id", token)?.Value)
                                        {
                                            case Cyberware.EssenceHoleGuidString when blnSync:
                                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                objCharacter.DecreaseEssenceHole(objImprovement.Rating);
                                                break;
                                            case Cyberware.EssenceHoleGuidString:
                                                await objCharacter.DecreaseEssenceHoleAsync(objImprovement.Rating,
                                                    token: token).ConfigureAwait(false);
                                                break;
                                            case Cyberware.EssenceAntiHoleGuidString when blnSync:
                                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                objCharacter.IncreaseEssenceHole(objImprovement.Rating);
                                                break;
                                            case Cyberware.EssenceAntiHoleGuidString:
                                                await objCharacter.IncreaseEssenceHoleAsync(objImprovement.Rating,
                                                    token: token).ConfigureAwait(false);
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                            break;
                    }
                }

                if (blnSync)
                    // ReSharper disable once MethodHasAsyncOverload
                    objImprovementList.ProcessRelevantEvents(token);
                else
                    await objImprovementList.ProcessRelevantEventsAsync(token).ConfigureAwait(false);
            }
            finally
            {
                if (blnSync)
                    objLocker.Dispose();
                else
                    await objLockerAsync.DisposeAsync().ConfigureAwait(false);
            }

            Log.Debug("RemoveImprovements exit");
            return decReturn;
        }

        /// <summary>
        /// Create a new Improvement and add it to the Character.
        /// </summary>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="strImprovedName">Specific name of the Improved object - typically the name of an CharacterAttribute being improved.</param>
        /// <param name="objImprovementSource">Type of object that grants this Improvement.</param>
        /// <param name="strSourceName">Name of the item that grants this Improvement.</param>
        /// <param name="objImprovementType">Type of object the Improvement applies to.</param>
        /// <param name="strUnique">Name of the pool this Improvement should be added to - only the single highest value in the pool will be applied to the character.</param>
        /// <param name="decValue">Set a Value for the Improvement.</param>
        /// <param name="intRating">Set a Rating for the Improvement - typically used for Adept Powers.</param>
        /// <param name="intMinimum">Improve the Minimum for an CharacterAttribute by the given amount.</param>
        /// <param name="intMaximum">Improve the Maximum for an CharacterAttribute by the given amount.</param>
        /// <param name="decAugmented">Improve the Augmented value for an CharacterAttribute by the given amount.</param>
        /// <param name="intAugmentedMaximum">Improve the Augmented Maximum value for an CharacterAttribute by the given amount.</param>
        /// <param name="strExclude">A list of child items that should not receive the Improvement's benefit (typically for Skill Groups).</param>
        /// <param name="blnAddToRating">Whether the bonus applies to a Skill's Rating instead of the dice pool in general.</param>
        /// <param name="strTarget">What target the Improvement has, if any (e.g. a target skill whose attribute to replace).</param>
        /// <param name="strCondition">Condition for when the bonus is applied.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static Improvement CreateImprovement(Character objCharacter, string strImprovedName,
                                             Improvement.ImprovementSource objImprovementSource,
                                             string strSourceName, Improvement.ImprovementType objImprovementType,
                                             string strUnique,
                                             decimal decValue = 0, int intRating = 1, int intMinimum = 0,
                                             int intMaximum = 0, decimal decAugmented = 0,
                                             int intAugmentedMaximum = 0, string strExclude = "",
                                             bool blnAddToRating = false, string strTarget = "",
                                             string strCondition = "", CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            Log.Debug("CreateImprovement");
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdTrace))
            {
                sbdTrace.Append("strImprovedName = ").AppendLine(strImprovedName);
                sbdTrace.Append("objImprovementSource = ").AppendLine(objImprovementSource.ToString());
                sbdTrace.Append("strSourceName = ").AppendLine(strSourceName);
                sbdTrace.Append("objImprovementType = ").AppendLine(objImprovementType.ToString());
                sbdTrace.Append("strUnique = ").AppendLine(strUnique);
                sbdTrace.Append("decValue = ").AppendLine(decValue.ToString(GlobalSettings.InvariantCultureInfo));
                sbdTrace.Append("intRating = ").AppendLine(intRating.ToString(GlobalSettings.InvariantCultureInfo));
                sbdTrace.Append("intMinimum = ").AppendLine(intMinimum.ToString(GlobalSettings.InvariantCultureInfo));
                sbdTrace.Append("intMaximum = ").AppendLine(intMaximum.ToString(GlobalSettings.InvariantCultureInfo));
                sbdTrace.Append("decAugmented = ").AppendLine(decAugmented.ToString(GlobalSettings.InvariantCultureInfo));
                sbdTrace.Append("intAugmentedMaximum = ").AppendLine(intAugmentedMaximum.ToString(GlobalSettings.InvariantCultureInfo));
                sbdTrace.Append("strExclude = ").AppendLine(strExclude);
                sbdTrace.Append("blnAddToRating = ").AppendLine(blnAddToRating.ToString(GlobalSettings.InvariantCultureInfo));
                sbdTrace.Append("strCondition = ").AppendLine(strCondition);
                Log.Trace(sbdTrace.ToString);
            }

            Improvement objImprovement = null;

            // Do not attempt to add the Improvements if the Character is null (as a result of Cyberware being added to a VehicleMod).
            if (objCharacter != null)
            {
                using (objCharacter.LockObject.EnterWriteLock(token))
                {
                    // Record the improvement.
                    // ReSharper disable once UseObjectOrCollectionInitializer
#pragma warning disable IDE0017
                    objImprovement = new Improvement(objCharacter)
#pragma warning restore IDE0017
                    {
                        ImprovedName = strImprovedName,
                        ImproveSource = objImprovementSource,
                        SourceName = strSourceName,
                        ImproveType = objImprovementType,
                        UniqueName = strUnique,
                        Value = decValue,
                        Rating = intRating,
                        Minimum = intMinimum,
                        Maximum = intMaximum,
                        Augmented = decAugmented,
                        AugmentedMaximum = intAugmentedMaximum,
                        Exclude = strExclude,
                        AddToRating = blnAddToRating,
                        Target = strTarget,
                        Condition = strCondition
                    };
                    // This is initially set to false make sure no property changers are triggered by the setters in the section above
                    objImprovement.SetupComplete = true;
                    // Add the Improvement to the list.
                    objCharacter.Improvements.Add(objImprovement);
                    ClearCachedValue(objCharacter, objImprovementType, strImprovedName, token);

                    // Add the Improvement to the Transaction List.
                    List<Improvement> lstTransactions
                        = s_DictionaryTransactions.AddOrUpdate(objCharacter, x => new List<Improvement>(1),
                                                               (x, y) => y);
                    lstTransactions.Add(objImprovement);
                }
            }

            Log.Debug("CreateImprovement exit");
            return objImprovement;
        }

        /// <summary>
        /// Create a new Improvement and add it to the Character.
        /// </summary>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="strImprovedName">Specific name of the Improved object - typically the name of an CharacterAttribute being improved.</param>
        /// <param name="objImprovementSource">Type of object that grants this Improvement.</param>
        /// <param name="strSourceName">Name of the item that grants this Improvement.</param>
        /// <param name="objImprovementType">Type of object the Improvement applies to.</param>
        /// <param name="strUnique">Name of the pool this Improvement should be added to - only the single highest value in the pool will be applied to the character.</param>
        /// <param name="decValue">Set a Value for the Improvement.</param>
        /// <param name="intRating">Set a Rating for the Improvement - typically used for Adept Powers.</param>
        /// <param name="intMinimum">Improve the Minimum for an CharacterAttribute by the given amount.</param>
        /// <param name="intMaximum">Improve the Maximum for an CharacterAttribute by the given amount.</param>
        /// <param name="decAugmented">Improve the Augmented value for an CharacterAttribute by the given amount.</param>
        /// <param name="intAugmentedMaximum">Improve the Augmented Maximum value for an CharacterAttribute by the given amount.</param>
        /// <param name="strExclude">A list of child items that should not receive the Improvement's benefit (typically for Skill Groups).</param>
        /// <param name="blnAddToRating">Whether the bonus applies to a Skill's Rating instead of the dice pool in general.</param>
        /// <param name="strTarget">What target the Improvement has, if any (e.g. a target skill whose attribute to replace).</param>
        /// <param name="strCondition">Condition for when the bonus is applied.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task<Improvement> CreateImprovementAsync(Character objCharacter, string strImprovedName,
                                                                          Improvement.ImprovementSource objImprovementSource,
                                                                          string strSourceName, Improvement.ImprovementType objImprovementType,
                                                                          string strUnique,
                                                                          decimal decValue = 0, int intRating = 1, int intMinimum = 0,
                                                                          int intMaximum = 0, decimal decAugmented = 0,
                                                                          int intAugmentedMaximum = 0, string strExclude = "",
                                                                          bool blnAddToRating = false, string strTarget = "",
                                                                          string strCondition = "", CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            Log.Debug("CreateImprovement");
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdTrace))
            {
                sbdTrace.Append("strImprovedName = ").AppendLine(strImprovedName);
                sbdTrace.Append("objImprovementSource = ").AppendLine(objImprovementSource.ToString());
                sbdTrace.Append("strSourceName = ").AppendLine(strSourceName);
                sbdTrace.Append("objImprovementType = ").AppendLine(objImprovementType.ToString());
                sbdTrace.Append("strUnique = ").AppendLine(strUnique);
                sbdTrace.Append("decValue = ").AppendLine(decValue.ToString(GlobalSettings.InvariantCultureInfo));
                sbdTrace.Append("intRating = ").AppendLine(intRating.ToString(GlobalSettings.InvariantCultureInfo));
                sbdTrace.Append("intMinimum = ").AppendLine(intMinimum.ToString(GlobalSettings.InvariantCultureInfo));
                sbdTrace.Append("intMaximum = ").AppendLine(intMaximum.ToString(GlobalSettings.InvariantCultureInfo));
                sbdTrace.Append("decAugmented = ").AppendLine(decAugmented.ToString(GlobalSettings.InvariantCultureInfo));
                sbdTrace.Append("intAugmentedMaximum = ").AppendLine(intAugmentedMaximum.ToString(GlobalSettings.InvariantCultureInfo));
                sbdTrace.Append("strExclude = ").AppendLine(strExclude);
                sbdTrace.Append("blnAddToRating = ").AppendLine(blnAddToRating.ToString(GlobalSettings.InvariantCultureInfo));
                sbdTrace.Append("strCondition = ").AppendLine(strCondition);
                Log.Trace(sbdTrace.ToString);
            }

            Improvement objImprovement = null;

            // Do not attempt to add the Improvements if the Character is null (as a result of Cyberware being added to a VehicleMod).
            if (objCharacter != null)
            {
                IAsyncDisposable objLocker = await objCharacter.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    // Record the improvement.
                    // ReSharper disable once UseObjectOrCollectionInitializer
#pragma warning disable IDE0017
                    objImprovement = new Improvement(objCharacter)
#pragma warning restore IDE0017
                    {
                        ImprovedName = strImprovedName,
                        ImproveSource = objImprovementSource,
                        SourceName = strSourceName,
                        ImproveType = objImprovementType,
                        UniqueName = strUnique,
                        Minimum = intMinimum,
                        Maximum = intMaximum,
                        Augmented = decAugmented,
                        AugmentedMaximum = intAugmentedMaximum,
                        Exclude = strExclude,
                        AddToRating = blnAddToRating,
                        Target = strTarget,
                        Condition = strCondition
                    };
                    await objImprovement.SetRatingAsync(intRating, token).ConfigureAwait(false);
                    await objImprovement.SetValueAsync(decValue, token).ConfigureAwait(false);
                    // This is initially set to false make sure no property changers are triggered by the setters in the section above
                    objImprovement.SetupComplete = true;
                    // Add the Improvement to the list.
                    await objCharacter.Improvements.AddAsync(objImprovement, token).ConfigureAwait(false);
                    ClearCachedValue(objCharacter, objImprovementType, strImprovedName, token);

                    // Add the Improvement to the Transaction List.
                    List<Improvement> lstTransactions
                        = s_DictionaryTransactions.AddOrUpdate(objCharacter, x => new List<Improvement>(1),
                                                               (x, y) => y);
                    lstTransactions.Add(objImprovement);
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
            }

            Log.Debug("CreateImprovement exit");
            return objImprovement;
        }

        /// <summary>
        /// Clear all the Improvements from the Transaction List.
        /// </summary>
        public static void Commit(Character objCharacter, CancellationToken token = default)
        {
            // No cancellation request here because we expect not to have to rollback once this method is called
            Log.Debug("Commit");
            // Clear all the Improvements from the Transaction List.
            if (s_DictionaryTransactions.TryRemove(objCharacter, out List<Improvement> lstTransactions))
            {
                lstTransactions.ProcessRelevantEvents(token);
            }

            Log.Debug("Commit exit");
        }

        /// <summary>
        /// Clear all the Improvements from the Transaction List.
        /// </summary>
        public static async Task CommitAsync(Character objCharacter, CancellationToken token = default)
        {
            // No cancellation request here because we expect not to have to rollback once this method is called
            Log.Debug("Commit");
            // Clear all the Improvements from the Transaction List.
            if (s_DictionaryTransactions.TryRemove(objCharacter, out List<Improvement> lstTransactions))
            {
                await lstTransactions.ProcessRelevantEventsAsync(token: token).ConfigureAwait(false);
            }

            Log.Debug("Commit exit");
        }

        /// <summary>
        /// Rollback all the Improvements from the Transaction List.
        /// </summary>
        public static void Rollback(Character objCharacter, CancellationToken token = default)
        {
            Log.Debug("Rollback enter");
            if (s_DictionaryTransactions.TryRemove(objCharacter, out List<Improvement> lstTransactions))
            {
                // Remove all the Improvements that were added.
                RemoveImprovements(objCharacter, lstTransactions, token: token);
                lstTransactions.Clear();
            }

            Log.Debug("Rollback exit");
        }

        /// <summary>
        /// Rollback all the Improvements from the Transaction List.
        /// </summary>
        public static async Task RollbackAsync(Character objCharacter, CancellationToken token = default)
        {
            Log.Debug("Rollback enter");
            if (s_DictionaryTransactions.TryRemove(objCharacter, out List<Improvement> lstTransactions))
            {
                // Remove all the Improvements that were added.
                await RemoveImprovementsAsync(objCharacter, lstTransactions, token: token).ConfigureAwait(false);
                lstTransactions.Clear();
            }

            Log.Debug("Rollback exit");
        }

        /// <summary>
        /// Fire off all events relevant to an improvement, making sure each event is only fired once.
        /// </summary>
        /// <param name="objImprovement">Improvement whose events to fire</param>
        /// <param name="lstExtraImprovedName">Additional ImprovedName versions to check, if any.</param>
        /// <param name="lstExtraImprovementTypes">Additional ImprovementType versions to check, if any.</param>
        /// <param name="lstExtraUniqueName">Additional UniqueName versions to check, if any.</param>
        /// <param name="lstExtraTarget">Additional Target versions to check, if any.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static void ProcessRelevantEvents(this Improvement objImprovement, IReadOnlyCollection<string> lstExtraImprovedName = null, IEnumerable<Improvement.ImprovementType> lstExtraImprovementTypes = null, IReadOnlyCollection<string> lstExtraUniqueName = null, IReadOnlyCollection<string> lstExtraTarget = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objImprovement?.SetupComplete != true)
                return;
            // Create a hashset of events to fire to make sure we only ever fire each event once
            using (new FetchSafelyFromSafeObjectPool<Dictionary<INotifyMultiplePropertiesChangedAsync, HashSet<string>>>(
                       Utils.DictionaryForMultiplePropertyChangedPool,
                       out Dictionary<INotifyMultiplePropertiesChangedAsync, HashSet<string>> dicChangedProperties))
            {
                try
                {
                    foreach ((INotifyMultiplePropertiesChangedAsync objToNotify, string strProperty) in objImprovement
                                 .GetRelevantPropertyChangers(lstExtraImprovedName: lstExtraImprovedName,
                                                              lstExtraUniqueName: lstExtraUniqueName,
                                                              lstExtraTarget: lstExtraTarget))
                    {
                        token.ThrowIfCancellationRequested();
                        if (!dicChangedProperties.TryGetValue(objToNotify,
                                                              out HashSet<string> setLoopPropertiesChanged))
                        {
                            setLoopPropertiesChanged = Utils.StringHashSetPool.Get();
                            dicChangedProperties.Add(objToNotify, setLoopPropertiesChanged);
                        }

                        setLoopPropertiesChanged.Add(strProperty);
                    }

                    if (lstExtraImprovementTypes != null)
                    {
                        foreach (Improvement.ImprovementType eOverrideType in lstExtraImprovementTypes)
                        {
                            token.ThrowIfCancellationRequested();
                            foreach ((INotifyMultiplePropertiesChangedAsync objToNotify, string strProperty) in objImprovement
                                         .GetRelevantPropertyChangers(lstExtraImprovedName: lstExtraImprovedName,
                                                                      eOverrideType: eOverrideType,
                                                                      lstExtraUniqueName: lstExtraUniqueName,
                                                                      lstExtraTarget: lstExtraTarget))
                            {
                                token.ThrowIfCancellationRequested();
                                if (!dicChangedProperties.TryGetValue(objToNotify,
                                                                      out HashSet<string> setLoopPropertiesChanged))
                                {
                                    setLoopPropertiesChanged = Utils.StringHashSetPool.Get();
                                    dicChangedProperties.Add(objToNotify, setLoopPropertiesChanged);
                                }

                                setLoopPropertiesChanged.Add(strProperty);
                            }
                        }
                    }

                    token.ThrowIfCancellationRequested();

                    // Fire each event once
                    foreach (KeyValuePair<INotifyMultiplePropertiesChangedAsync, HashSet<string>> kvpChangedProperties in
                             dicChangedProperties)
                    {
                        token.ThrowIfCancellationRequested();
                        kvpChangedProperties.Key.OnMultiplePropertiesChanged(kvpChangedProperties.Value);
                    }
                }
                finally
                {
                    List<HashSet<string>> lstToReturn = dicChangedProperties.Values.ToList();
                    for (int i = lstToReturn.Count - 1; i >= 0; --i)
                    {
                        HashSet<string> setLoop = lstToReturn[i];
                        Utils.StringHashSetPool.Return(ref setLoop);
                    }
                }
            }
        }

        /// <summary>
        /// Fire off all events relevant to an enumerable of improvements, making sure each event is only fired once.
        /// </summary>
        /// <param name="lstImprovements">Enumerable of improvements whose events to fire</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static void ProcessRelevantEvents(this IEnumerable<Improvement> lstImprovements, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (lstImprovements == null)
                return;
            // Create a hashset of events to fire to make sure we only ever fire each event once
            using (new FetchSafelyFromSafeObjectPool<Dictionary<INotifyMultiplePropertiesChangedAsync, HashSet<string>>>(
                       Utils.DictionaryForMultiplePropertyChangedPool,
                       out Dictionary<INotifyMultiplePropertiesChangedAsync, HashSet<string>> dicChangedProperties))
            {
                try
                {
                    token.ThrowIfCancellationRequested();
                    foreach (Improvement objImprovement in lstImprovements.Where(x => x.SetupComplete))
                    {
                        token.ThrowIfCancellationRequested();
                        foreach ((INotifyMultiplePropertiesChangedAsync objToNotify, string strProperty) in objImprovement
                                     .GetRelevantPropertyChangers())
                        {
                            token.ThrowIfCancellationRequested();
                            if (!dicChangedProperties.TryGetValue(objToNotify,
                                                                  out HashSet<string> setLoopPropertiesChanged))
                            {
                                setLoopPropertiesChanged = Utils.StringHashSetPool.Get();
                                dicChangedProperties.Add(objToNotify, setLoopPropertiesChanged);
                            }
                            setLoopPropertiesChanged.Add(strProperty);
                        }
                    }

                    token.ThrowIfCancellationRequested();
                    // Fire each event once
                    foreach (KeyValuePair<INotifyMultiplePropertiesChangedAsync, HashSet<string>> kvpChangedProperties in
                             dicChangedProperties)
                    {
                        token.ThrowIfCancellationRequested();
                        kvpChangedProperties.Key.OnMultiplePropertiesChanged(kvpChangedProperties.Value);
                    }
                }
                finally
                {
                    List<HashSet<string>> lstToReturn = dicChangedProperties.Values.ToList();
                    for (int i = lstToReturn.Count - 1; i >= 0; --i)
                    {
                        HashSet<string> setLoop = lstToReturn[i];
                        Utils.StringHashSetPool.Return(ref setLoop);
                    }
                }
            }
        }

        /// <summary>
        /// Fire off all events relevant to an improvement, making sure each event is only fired once.
        /// </summary>
        /// <param name="objImprovement">Improvement whose events to fire</param>
        /// <param name="lstExtraImprovedName">Additional ImprovedName versions to check, if any.</param>
        /// <param name="lstExtraImprovementTypes">Additional ImprovementType versions to check, if any.</param>
        /// <param name="lstExtraUniqueName">Additional UniqueName versions to check, if any.</param>
        /// <param name="lstExtraTarget">Additional Target versions to check, if any.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task ProcessRelevantEventsAsync(this Improvement objImprovement, IReadOnlyCollection<string> lstExtraImprovedName = null, IEnumerable<Improvement.ImprovementType> lstExtraImprovementTypes = null, IReadOnlyCollection<string> lstExtraUniqueName = null, IReadOnlyCollection<string> lstExtraTarget = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objImprovement?.SetupComplete != true)
                return;
            // Create a hashset of events to fire to make sure we only ever fire each event once
            using (new FetchSafelyFromSafeObjectPool<Dictionary<INotifyMultiplePropertiesChangedAsync, HashSet<string>>>(
                       Utils.DictionaryForMultiplePropertyChangedPool,
                       out Dictionary<INotifyMultiplePropertiesChangedAsync, HashSet<string>> dicChangedProperties))
            {
                try
                {
                    foreach ((INotifyMultiplePropertiesChangedAsync objToNotify, string strProperty) in await objImprovement
                                 .GetRelevantPropertyChangersAsync(lstExtraImprovedName: lstExtraImprovedName,
                                     lstExtraUniqueName: lstExtraUniqueName,
                                     lstExtraTarget: lstExtraTarget, token: token).ConfigureAwait(false))
                    {
                        token.ThrowIfCancellationRequested();
                        if (!dicChangedProperties.TryGetValue(objToNotify,
                                                              out HashSet<string> setLoopPropertiesChanged))
                        {
                            setLoopPropertiesChanged = Utils.StringHashSetPool.Get();
                            dicChangedProperties.Add(objToNotify, setLoopPropertiesChanged);
                        }

                        setLoopPropertiesChanged.Add(strProperty);
                    }

                    if (lstExtraImprovementTypes != null)
                    {
                        foreach (Improvement.ImprovementType eOverrideType in lstExtraImprovementTypes)
                        {
                            token.ThrowIfCancellationRequested();
                            foreach ((INotifyMultiplePropertiesChangedAsync objToNotify, string strProperty) in await objImprovement
                                         .GetRelevantPropertyChangersAsync(lstExtraImprovedName: lstExtraImprovedName,
                                             eOverrideType: eOverrideType,
                                             lstExtraUniqueName: lstExtraUniqueName,
                                             lstExtraTarget: lstExtraTarget, token: token).ConfigureAwait(false))
                            {
                                token.ThrowIfCancellationRequested();
                                if (!dicChangedProperties.TryGetValue(objToNotify,
                                                                      out HashSet<string> setLoopPropertiesChanged))
                                {
                                    setLoopPropertiesChanged = Utils.StringHashSetPool.Get();
                                    dicChangedProperties.Add(objToNotify, setLoopPropertiesChanged);
                                }

                                setLoopPropertiesChanged.Add(strProperty);
                            }
                        }
                    }

                    token.ThrowIfCancellationRequested();

                    // Fire each event once
                    foreach (KeyValuePair<INotifyMultiplePropertiesChangedAsync, HashSet<string>> kvpChangedProperties in
                             dicChangedProperties)
                    {
                        token.ThrowIfCancellationRequested();
                        await kvpChangedProperties.Key.OnMultiplePropertiesChangedAsync(kvpChangedProperties.Value, token).ConfigureAwait(false);
                    }
                }
                finally
                {
                    List<HashSet<string>> lstToReturn = dicChangedProperties.Values.ToList();
                    for (int i = lstToReturn.Count - 1; i >= 0; --i)
                    {
                        HashSet<string> setLoop = lstToReturn[i];
                        Utils.StringHashSetPool.Return(ref setLoop);
                    }
                }
            }
        }

        /// <summary>
        /// Fire off all events relevant to an enumerable of improvements, making sure each event is only fired once.
        /// </summary>
        /// <param name="lstImprovements">Enumerable of improvements whose events to fire</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task ProcessRelevantEventsAsync(this IEnumerable<Improvement> lstImprovements, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (lstImprovements == null)
                return;
            // Create a hashset of events to fire to make sure we only ever fire each event once
            using (new FetchSafelyFromSafeObjectPool<Dictionary<INotifyMultiplePropertiesChangedAsync, HashSet<string>>>(
                       Utils.DictionaryForMultiplePropertyChangedPool,
                       out Dictionary<INotifyMultiplePropertiesChangedAsync, HashSet<string>> dicChangedProperties))
            {
                try
                {
                    token.ThrowIfCancellationRequested();
                    foreach (Improvement objImprovement in lstImprovements.Where(x => x.SetupComplete))
                    {
                        token.ThrowIfCancellationRequested();
                        foreach ((INotifyMultiplePropertiesChangedAsync objToNotify, string strProperty) in await objImprovement
                                     .GetRelevantPropertyChangersAsync(token: token).ConfigureAwait(false))
                        {
                            token.ThrowIfCancellationRequested();
                            if (!dicChangedProperties.TryGetValue(objToNotify,
                                                                  out HashSet<string> setLoopPropertiesChanged))
                            {
                                setLoopPropertiesChanged = Utils.StringHashSetPool.Get();
                                dicChangedProperties.Add(objToNotify, setLoopPropertiesChanged);
                            }
                            setLoopPropertiesChanged.Add(strProperty);
                        }
                    }

                    token.ThrowIfCancellationRequested();
                    // Fire each event once
                    foreach (KeyValuePair<INotifyMultiplePropertiesChangedAsync, HashSet<string>> kvpChangedProperties in
                             dicChangedProperties)
                    {
                        token.ThrowIfCancellationRequested();
                        await kvpChangedProperties.Key.OnMultiplePropertiesChangedAsync(kvpChangedProperties.Value, token).ConfigureAwait(false);
                    }
                }
                finally
                {
                    List<HashSet<string>> lstToReturn = dicChangedProperties.Values.ToList();
                    for (int i = lstToReturn.Count - 1; i >= 0; --i)
                    {
                        HashSet<string> setLoop = lstToReturn[i];
                        Utils.StringHashSetPool.Return(ref setLoop);
                    }
                }
            }
        }

        #endregion Improvement System

        #region Condition Evaluation

        private static readonly string[] AndSeparators = { " and " };
        private static readonly string[] OrSeparators = { " or " };

        /// <summary>
        /// Evaluates a condition string against a target object using reflection.
        /// Supports various condition types:
        /// - XPath-like syntax: /character/created, /spell/alchemical, /skill/name
        /// - Property checks: @property = value
        /// - Negation: not(condition)
        /// - Multiple conditions: condition1 and condition2
        /// - Range checks: @property > value, @property < value
        /// - String contains: @property contains value
        /// - Legacy conditions: career, create, specialization names
        /// </summary>
        /// <param name="condition">The condition string to evaluate</param>
        /// <param name="targetObject">The object to evaluate the condition against</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>True if the condition is met, false otherwise</returns>
        public static async Task<bool> EvaluateConditionAsync(string condition, object targetObject, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(condition) || targetObject == null)
                return true;

            token.ThrowIfCancellationRequested();

            try
            {
                // Handle XPath-like syntax first (e.g., /character/created, /spell/alchemical)
                bool? xpathResult = await EvaluateXPathConditionAsync(condition, targetObject, token).ConfigureAwait(false);
                if (xpathResult.HasValue)
                {
                    return xpathResult.Value;
                }

                // Handle legacy conditions
                bool? legacyResult = await EvaluateLegacyConditionAsync(condition, targetObject, token).ConfigureAwait(false);
                if (legacyResult.HasValue)
                {
                    return legacyResult.Value;
                }

                // Handle simple negation
                if (condition.StartsWith("not(", StringComparison.OrdinalIgnoreCase) && condition.EndsWith(")"))
                {
                    string innerCondition = condition.Substring(4, condition.Length - 5);
                    return !await EvaluateConditionAsync(innerCondition, targetObject, token).ConfigureAwait(false);
                }

                // Handle AND conditions
                if (condition.Contains(" and ", StringComparison.OrdinalIgnoreCase))
                {
                    string[] parts = condition.Split(AndSeparators, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string part in parts)
                    {
                        if (!await EvaluateConditionAsync(part.Trim(), targetObject, token).ConfigureAwait(false))
                            return false;
                    }
                    return true;
                }

                // Handle OR conditions
                if (condition.Contains(" or ", StringComparison.OrdinalIgnoreCase))
                {
                    string[] parts = condition.Split(OrSeparators, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string part in parts)
                    {
                        if (await EvaluateConditionAsync(part.Trim(), targetObject, token).ConfigureAwait(false))
                            return true;
                    }
                    return false;
                }

                // Handle property comparisons
                return await EvaluatePropertyConditionAsync(condition, targetObject, token).ConfigureAwait(false);
            }
            catch (Exception)
            {
                Utils.BreakIfDebug();
                return true; // Default to allowing the improvement if condition evaluation fails
            }
        }

        /// <summary>
        /// Evaluates XPath-like syntax conditions (e.g., /character/created, /spell/alchemical).
        /// Returns null if the condition is not an XPath condition.
        /// </summary>
        private static async Task<bool?> EvaluateXPathConditionAsync(string condition, object targetObject, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            // Match XPath-like syntax: /objectType/property or /objectType/property operator value
            var match = Regex.Match(condition, @"^/(\w+)/([^=!<>]+)(?:\s*([=!<>]+)\s*(.+))?$");
            if (!match.Success)
                return null;

            string objectType = match.Groups[1].Value.ToLowerInvariant();
            string propertyName = match.Groups[2].Value.Trim();
            string operator = match.Groups[3].Success ? match.Groups[3].Value.Trim() : "=";
            string expectedValue = match.Groups[4].Success ? match.Groups[4].Value.Trim().Trim('"', '\'') : null;

            // Check if the target object matches the expected type
            if (!IsObjectTypeMatch(targetObject, objectType))
                return null;

            // Get the property value
            object propertyValue = await GetPropertyValueAsync(targetObject, propertyName, token).ConfigureAwait(false);
            if (propertyValue == null)
                return false;

            // If no expected value, return boolean conversion of property
            if (string.IsNullOrEmpty(expectedValue))
            {
                return Convert.ToBoolean(propertyValue);
            }

            // Compare with expected value using the specified operator
            return EvaluateComparison(propertyValue, expectedValue, operator);
        }

        /// <summary>
        /// Checks if the target object matches the expected object type.
        /// </summary>
        private static bool IsObjectTypeMatch(object targetObject, string objectType)
        {
            switch (objectType)
            {
                case "character":
                    return targetObject is Character;
                case "spell":
                    return targetObject is Spell;
                case "skill":
                    return targetObject is Skill;
                case "skillgroup":
                    return targetObject is SkillGroup;
                case "gear":
                    return targetObject is Gear;
                case "weapon":
                    return targetObject is Weapon;
                case "armor":
                    return targetObject is Armor;
                case "cyberware":
                case "bioware":
                    return targetObject is Cyberware;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Evaluates legacy conditions that don't use the new reflection syntax.
        /// Returns null if the condition is not a legacy condition.
        /// </summary>
        private static async Task<bool?> EvaluateLegacyConditionAsync(string condition, object targetObject, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            // Handle career/create conditions for Character objects
            if (targetObject is Character objCharacter)
            {
                if (condition == "career")
                    return objCharacter.Created;
                if (condition == "create")
                    return !objCharacter.Created;
            }

            // Handle skill specialization conditions
            if (targetObject is Skill objSkill)
            {
                if (objSkill.HasSpecialization(condition))
                    return true;
            }

            if (targetObject is SkillGroup objSkillGroup)
            {
                foreach (Skill objSkillInGroup in objSkillGroup.SkillList)
                {
                    if (objSkillInGroup.HasSpecialization(condition))
                        return true;
                }
            }

            return null; // Not a legacy condition
        }

        /// <summary>
        /// Evaluates property-based conditions using reflection.
        /// </summary>
        private static async Task<bool> EvaluatePropertyConditionAsync(string condition, object targetObject, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            // Handle @property syntax
            if (condition.StartsWith("@"))
            {
                string propertyName = condition.Substring(1);
                object propertyValue = await GetPropertyValueAsync(targetObject, propertyName, token).ConfigureAwait(false);
                return propertyValue != null && Convert.ToBoolean(propertyValue);
            }

            // Handle comparison operators: =, !=, >, <, >=, <=, contains
            if (condition.Contains("=") || condition.Contains("!=") || condition.Contains(">") || condition.Contains("<") || condition.Contains("contains"))
            {
                // Try different comparison operators
                string[] operators = { "!=", ">=", "<=", "=", ">", "<", "contains" };
                foreach (string op in operators)
                {
                    if (condition.Contains(op))
                    {
                        string[] parts = condition.Split(new[] { op }, 2, StringSplitOptions.None);
                        if (parts.Length == 2)
                        {
                            string propertyName = parts[0].Trim().TrimStart('@');
                            string expectedValue = parts[1].Trim().Trim('"', '\'');
                            
                            object propertyValue = await GetPropertyValueAsync(targetObject, propertyName, token).ConfigureAwait(false);
                            if (propertyValue != null)
                            {
                                return EvaluateComparison(propertyValue, expectedValue, op);
                            }
                        }
                        break;
                    }
                }
            }

            return true; // Default to allowing the improvement
        }

        /// <summary>
        /// Evaluates a comparison between a property value and an expected value.
        /// </summary>
        private static bool EvaluateComparison(object propertyValue, string expectedValue, string operator)
        {
            try
            {
                object convertedExpectedValue = ConvertValue(expectedValue, propertyValue.GetType());
                
                switch (operator)
                {
                    case "=":
                        return Equals(propertyValue, convertedExpectedValue);
                    case "!=":
                        return !Equals(propertyValue, convertedExpectedValue);
                    case ">":
                        return CompareValues(propertyValue, convertedExpectedValue) > 0;
                    case "<":
                        return CompareValues(propertyValue, convertedExpectedValue) < 0;
                    case ">=":
                        return CompareValues(propertyValue, convertedExpectedValue) >= 0;
                    case "<=":
                        return CompareValues(propertyValue, convertedExpectedValue) <= 0;
                    case "contains":
                        return propertyValue.ToString().Contains(expectedValue, StringComparison.OrdinalIgnoreCase);
                    default:
                        return Equals(propertyValue, convertedExpectedValue);
                }
            }
            catch
            {
                // If comparison fails, default to false
                return false;
            }
        }

        /// <summary>
        /// Compares two values for ordering operations.
        /// </summary>
        private static int CompareValues(object value1, object value2)
        {
            if (value1 is IComparable comparable1 && value2 is IComparable comparable2)
            {
                return comparable1.CompareTo(comparable2);
            }
            
            // Fallback to string comparison
            return string.Compare(value1.ToString(), value2.ToString(), StringComparison.Ordinal);
        }

        /// <summary>
        /// Gets a property value from an object using reflection.
        /// Handles both regular properties and async methods (GetXxxAsync).
        /// </summary>
        private static async Task<object> GetPropertyValueAsync(object targetObject, string propertyName, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            Type objectType = targetObject.GetType();
            
            // First try to find a regular property
            PropertyInfo property = objectType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property != null)
            {
                // Handle async properties (Task<T> types) - rare but possible
                if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    var getMethod = property.GetGetMethod();
                    if (getMethod != null)
                    {
                        var task = getMethod.Invoke(targetObject, null) as Task;
                        if (task != null)
                        {
                            await task.ConfigureAwait(false);
                            return GetTaskResult(task);
                        }
                    }
                }

                // Handle regular properties
                return property.GetValue(targetObject);
            }

            // Try to find an async method (GetXxxAsync pattern)
            string asyncMethodName = "Get" + propertyName + "Async";
            MethodInfo asyncMethod = objectType.GetMethod(asyncMethodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (asyncMethod != null)
            {
                // Check if the method returns a Task<T>
                if (asyncMethod.ReturnType.IsGenericType && asyncMethod.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    // Invoke the async method
                    var task = asyncMethod.Invoke(targetObject, new object[] { token }) as Task;
                    if (task != null)
                    {
                        await task.ConfigureAwait(false);
                        return GetTaskResult(task);
                    }
                }
            }

            // Try alternative async method patterns
            string[] alternativePatterns = {
                "Get" + propertyName + "Async",
                propertyName + "Async",
                "Is" + propertyName + "Async",
                "Has" + propertyName + "Async"
            };

            foreach (string methodName in alternativePatterns)
            {
                MethodInfo method = objectType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (method != null && method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    // Try to invoke with cancellation token if the method accepts it
                    object[] parameters = method.GetParameters().Length > 0 ? new object[] { token } : new object[0];
                    var task = method.Invoke(targetObject, parameters) as Task;
                    if (task != null)
                    {
                        await task.ConfigureAwait(false);
                        return GetTaskResult(task);
                    }
                }
            }

            return null; // Property/method not found
        }

        /// <summary>
        /// Gets the result from a completed Task.
        /// </summary>
        private static object GetTaskResult(Task task)
        {
            if (task is Task<string> stringTask)
                return stringTask.Result;
            if (task is Task<bool> boolTask)
                return boolTask.Result;
            if (task is Task<int> intTask)
                return intTask.Result;
            if (task is Task<decimal> decimalTask)
                return decimalTask.Result;
            if (task is Task<double> doubleTask)
                return doubleTask.Result;
            if (task is Task<float> floatTask)
                return floatTask.Result;
            
            // Use reflection to get the Result property for other Task<T> types
            var resultProperty = task.GetType().GetProperty("Result");
            return resultProperty?.GetValue(task);
        }

        /// <summary>
        /// Converts a string value to the target type.
        /// </summary>
        private static object ConvertValue(string value, Type targetType)
        {
            if (targetType == typeof(string))
                return value;
            if (targetType == typeof(bool))
                return bool.Parse(value);
            if (targetType == typeof(int))
                return int.Parse(value);
            if (targetType == typeof(decimal))
                return decimal.Parse(value);
            if (targetType == typeof(double))
                return double.Parse(value);
            if (targetType == typeof(float))
                return float.Parse(value);
            
            // Try to convert using the type's converter
            try
            {
                return Convert.ChangeType(value, targetType);
            }
            catch
            {
                return value; // Fallback to string
            }
        }

        /// <summary>
        /// Generic method to evaluate improvement conditions for any improvement type.
        /// This is the main entry point for all improvement condition evaluation.
        /// Supports XPath-like syntax: /character/created, /spell/alchemical, /skill/name
        /// </summary>
        /// <param name="improvement">The improvement to evaluate</param>
        /// <param name="targetObject">The target object to evaluate against</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>True if the improvement should be applied, false otherwise</returns>
        public static async Task<bool> EvaluateImprovementConditionAsync(Improvement improvement, object targetObject, CancellationToken token = default)
        {
            if (improvement == null || string.IsNullOrEmpty(improvement.Condition))
                return true;

            return await EvaluateConditionAsync(improvement.Condition, targetObject, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Synchronous version of the generic improvement condition evaluator.
        /// Handles legacy conditions and simple XPath conditions synchronously.
        /// </summary>
        /// <param name="improvement">The improvement to evaluate</param>
        /// <param name="targetObject">The target object to evaluate against</param>
        /// <returns>True if the improvement should be applied, false otherwise</returns>
        public static bool EvaluateImprovementCondition(Improvement improvement, object targetObject)
        {
            if (improvement == null || string.IsNullOrEmpty(improvement.Condition))
                return true;

            // Handle legacy conditions synchronously
            if (targetObject is Character objCharacter)
            {
                if (improvement.Condition == "career")
                    return objCharacter.Created;
                if (improvement.Condition == "create")
                    return !objCharacter.Created;
            }

            if (targetObject is Skill objSkill)
            {
                if (objSkill.HasSpecialization(improvement.Condition))
                    return true;
            }

            if (targetObject is SkillGroup objSkillGroup)
            {
                foreach (Skill objSkillInGroup in objSkillGroup.SkillList)
                {
                    if (objSkillInGroup.HasSpecialization(improvement.Condition))
                        return true;
                }
            }

            if (targetObject is Spell objSpell)
            {
                if (improvement.Condition == "not(@alchemical)" && objSpell.Alchemical)
                    return false;
                if (improvement.Condition == "not(/spell/alchemical)" && objSpell.Alchemical)
                    return false;
            }

            // Handle simple XPath conditions synchronously
            if (improvement.Condition.StartsWith("/"))
            {
                var match = Regex.Match(improvement.Condition, @"^/(\w+)/([^=]+)(?:\s*=\s*(.+))?$");
                if (match.Success)
                {
                    string objectType = match.Groups[1].Value.ToLowerInvariant();
                    string propertyName = match.Groups[2].Value.Trim();
                    string expectedValue = match.Groups[3].Success ? match.Groups[3].Value.Trim().Trim('"', '\'') : null;

                    if (IsObjectTypeMatch(targetObject, objectType))
                    {
                        try
                        {
                            Type objectTypeClass = targetObject.GetType();
                            PropertyInfo property = objectTypeClass.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                            
                            if (property != null)
                            {
                                object propertyValue = property.GetValue(targetObject);
                                if (propertyValue != null)
                                {
                                    if (string.IsNullOrEmpty(expectedValue))
                                    {
                                        return Convert.ToBoolean(propertyValue);
                                    }
                                    else
                                    {
                                        object convertedExpectedValue = ConvertValue(expectedValue, propertyValue.GetType());
                                        return Equals(propertyValue, convertedExpectedValue);
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // If reflection fails, default to true for backward compatibility
                        }
                    }
                }
            }

            // For complex conditions, default to true to maintain backward compatibility
            return true;
        }

        #endregion Condition Evaluation
    }
}
