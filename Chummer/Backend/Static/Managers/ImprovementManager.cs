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

using Chummer.Backend.Attributes;
using Chummer.Backend.Equipment;
using Chummer.Backend.Skills;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
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
        private static string _strLimitSelection = string.Empty;

        private static string _strSelectedValue = string.Empty;
        private static string _strForcedValue = string.Empty;

        private static readonly ConcurrentDictionary<Character, List<Improvement>> s_DictionaryTransactions
            = new ConcurrentDictionary<Character, List<Improvement>>();

        private static readonly ConcurrentHashSet<Tuple<ImprovementDictionaryKey, ConcurrentDictionary<ImprovementDictionaryKey, Tuple<decimal, List<Improvement>>>>>
            s_SetCurrentlyCalculatingValues = new ConcurrentHashSet<Tuple<ImprovementDictionaryKey, ConcurrentDictionary<ImprovementDictionaryKey, Tuple<decimal, List<Improvement>>>>>();

        private static readonly ConcurrentDictionary<ImprovementDictionaryKey, Tuple<decimal, List<Improvement>>>
            s_DictionaryCachedValues
                = new ConcurrentDictionary<ImprovementDictionaryKey, Tuple<decimal, List<Improvement>>>();

        private static readonly ConcurrentDictionary<ImprovementDictionaryKey, Tuple<decimal, List<Improvement>>>
            s_DictionaryCachedAugmentedValues
                = new ConcurrentDictionary<ImprovementDictionaryKey, Tuple<decimal, List<Improvement>>>();

        public readonly struct ImprovementDictionaryKey : IEquatable<ImprovementDictionaryKey>,
            IEquatable<Tuple<Character, Improvement.ImprovementType, string>>
        {
            private readonly Tuple<Character, Improvement.ImprovementType, string> _objTupleKey;

            public Character CharacterObject => _objTupleKey.Item1;
            public Improvement.ImprovementType ImprovementType => _objTupleKey.Item2;
            public string ImprovementName => _objTupleKey.Item3;

            public ImprovementDictionaryKey(Character objCharacter, Improvement.ImprovementType eImprovementType,
                                            string strImprovementName)
            {
                _objTupleKey
                    = new Tuple<Character, Improvement.ImprovementType, string>(
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
        public static string LimitSelection
        {
            get => _strLimitSelection;
            set => _strLimitSelection = value;
        }

        /// <summary>
        /// The string that was entered or selected from any of the dialogue windows that were presented because of this Improvement.
        /// </summary>
        public static string SelectedValue
        {
            get => _strSelectedValue;
            set => _strSelectedValue = value;
        }

        /// <summary>
        /// Force any dialogue windows that open to use this string as their selected value.
        /// </summary>
        public static string ForcedValue
        {
            get => _strForcedValue;
            set => _strForcedValue = value;
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
                                                     x => new Tuple<decimal, List<Improvement>>(
                                                         decimal.MinValue, new List<Improvement>()),
                                                     (x, y) =>
                                                     {
                                                         y.Item2.Clear();
                                                         return new Tuple<decimal, List<Improvement>>(
                                                             decimal.MinValue, y.Item2);
                                                     });
                token.ThrowIfCancellationRequested();
                s_DictionaryCachedAugmentedValues.AddOrUpdate(objCheckKey,
                                                              x => new Tuple<decimal, List<Improvement>>(
                                                                  decimal.MinValue, new List<Improvement>()),
                                                              (x, y) =>
                                                              {
                                                                  y.Item2.Clear();
                                                                  return new Tuple<decimal, List<Improvement>>(
                                                                      decimal.MinValue, y.Item2);
                                                              });
            }
            else
            {
                List<ImprovementDictionaryKey> lstTempOuter = new List<ImprovementDictionaryKey>();
                foreach (ImprovementDictionaryKey objCachedValueKey in s_DictionaryCachedValues.Keys)
                {
                    token.ThrowIfCancellationRequested();
                    if (objCachedValueKey.CharacterObject == objCharacter && objCachedValueKey.ImprovementType == eImprovementType)
                        lstTempOuter.Add(objCachedValueKey);
                }
                foreach (ImprovementDictionaryKey objCheckKey in lstTempOuter)
                {
                    token.ThrowIfCancellationRequested();
                    if (s_DictionaryCachedValues.TryGetValue(objCheckKey,
                                                             out Tuple<decimal, List<Improvement>> tupTemp))
                    {
                        List<Improvement> lstTemp = tupTemp.Item2;
                        lstTemp.Clear();
                        s_DictionaryCachedValues
                            .AddOrUpdate(objCheckKey,
                                         x => new Tuple<decimal, List<Improvement>>(decimal.MinValue, lstTemp),
                                         (x, y) => new Tuple<decimal, List<Improvement>>(
                                             decimal.MinValue, lstTemp));
                    }
                }

                lstTempOuter.Clear();
                foreach (ImprovementDictionaryKey objCachedValueKey in s_DictionaryCachedAugmentedValues.Keys)
                {
                    token.ThrowIfCancellationRequested();
                    if (objCachedValueKey.CharacterObject == objCharacter && objCachedValueKey.ImprovementType == eImprovementType)
                        lstTempOuter.Add(objCachedValueKey);
                }
                foreach (ImprovementDictionaryKey objCheckKey in lstTempOuter)
                {
                    token.ThrowIfCancellationRequested();
                    if (s_DictionaryCachedAugmentedValues.TryGetValue(objCheckKey,
                                                                      out Tuple<decimal, List<Improvement>> tupTemp))
                    {
                        List<Improvement> lstTemp = tupTemp.Item2;
                        lstTemp.Clear();
                        s_DictionaryCachedAugmentedValues
                            .AddOrUpdate(objCheckKey,
                                         x => new Tuple<decimal, List<Improvement>>(decimal.MinValue, lstTemp),
                                         (x, y) => new Tuple<decimal, List<Improvement>>(
                                             decimal.MinValue, lstTemp));
                    }
                }
            }
        }

        public static void ClearCachedValues(Character objCharacter, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<ImprovementDictionaryKey> lstToRemove = new List<ImprovementDictionaryKey>();
            foreach (ImprovementDictionaryKey objKey in s_DictionaryCachedValues.Keys)
            {
                token.ThrowIfCancellationRequested();
                if (objKey.CharacterObject == objCharacter)
                    lstToRemove.Add(objKey);
            }
            foreach (ImprovementDictionaryKey objKey in lstToRemove)
            {
                token.ThrowIfCancellationRequested();
                if (s_DictionaryCachedValues.TryRemove(objKey, out Tuple<decimal, List<Improvement>> tupTemp))
                    tupTemp.Item2.Clear(); // Just in case this helps the GC
            }

            lstToRemove.Clear();
            foreach (ImprovementDictionaryKey objKey in s_DictionaryCachedAugmentedValues.Keys)
            {
                token.ThrowIfCancellationRequested();
                if (objKey.CharacterObject == objCharacter)
                    lstToRemove.Add(objKey);
            }
            foreach (ImprovementDictionaryKey objKey in lstToRemove)
            {
                token.ThrowIfCancellationRequested();
                if (s_DictionaryCachedAugmentedValues.TryRemove(objKey, out Tuple<decimal, List<Improvement>> tupTemp))
                    tupTemp.Item2.Clear(); // Just in case this helps the GC
            }

            s_DictionaryTransactions.TryRemove(objCharacter, out List<Improvement> _);
        }

        #endregion Properties

        #region Helper Methods

        /// <summary>
        /// Retrieve the total Improvement value for the specified ImprovementType.
        /// </summary>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="objImprovementType">ImprovementType to retrieve the value of.</param>
        /// <param name="blnAddToRating">Whether or not we should only retrieve values that have AddToRating enabled.</param>
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
        /// <param name="blnAddToRating">Whether or not we should only retrieve values that have AddToRating enabled.</param>
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
        /// <param name="blnAddToRating">Whether or not we should only retrieve values that have AddToRating enabled.</param>
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
        /// <param name="blnAddToRating">Whether or not we should only retrieve values that have AddToRating enabled.</param>
        /// <param name="strImprovedName">Name to assign to the Improvement.</param>
        /// <param name="blnUnconditionalOnly">Whether to only fetch values for improvements that do not have a condition.</param>
        /// <param name="blnIncludeNonImproved">Whether to only fetch values for improvements that do not have an improvedname when specifying ImprovedNames.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task<Tuple<decimal, List<Improvement>>> ValueOfTupleAsync(Character objCharacter, Improvement.ImprovementType objImprovementType,
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
            return new Tuple<decimal, List<Improvement>>(decReturn, lstUsedImprovements);
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
        /// <param name="blnAddToRating">Whether or not we should only retrieve values that have AddToRating enabled.</param>
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
        /// <param name="blnAddToRating">Whether or not we should only retrieve values that have AddToRating enabled.</param>
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
        /// <param name="blnAddToRating">Whether or not we should only retrieve values that have AddToRating enabled.</param>
        /// <param name="strImprovedName">Name to assign to the Improvement.</param>
        /// <param name="blnUnconditionalOnly">Whether to only fetch values for improvements that do not have a condition.</param>
        /// <param name="blnIncludeNonImproved">Whether to only fetch values for improvements that do not have an improvedname when specifying ImprovedNames.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async Task<Tuple<decimal, List<Improvement>>> AugmentedValueOfTupleAsync(Character objCharacter, Improvement.ImprovementType objImprovementType,
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
            return new Tuple<decimal, List<Improvement>>(decReturn, lstUsedImprovements);
        }

        /// <summary>
        /// Retrieve the total Improvement Augmented x Rating for the specified ImprovementType.
        /// </summary>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="objImprovementType">ImprovementType to retrieve the value of.</param>
        /// <param name="lstUsedImprovements">List of the improvements actually used for the value.</param>
        /// <param name="blnAddToRating">Whether or not we should only retrieve values that have AddToRating enabled.</param>
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
        /// <param name="blnAddToRating">Whether or not we should only retrieve values that have AddToRating enabled.</param>
        /// <param name="strImprovedName">Name to assign to the Improvement.</param>
        /// <param name="blnUnconditionalOnly">Whether to only fetch values for improvements that do not have a condition.</param>
        /// <param name="blnIncludeNonImproved">Whether to only fetch values for improvements that do not have an improvedname when specifying ImprovedNames.</param>
        /// <param name="funcValueGetter">Function for how to extract values for individual improvements.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        private static Tuple<decimal, List<Improvement>> MetaValueOf(Character objCharacter, Improvement.ImprovementType eImprovementType,
                                                                     Func<Improvement, decimal> funcValueGetter,
                                                                     ConcurrentDictionary<ImprovementDictionaryKey,
                                                                         Tuple<decimal, List<Improvement>>> dicCachedValuesToUse,
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
        /// <param name="blnAddToRating">Whether or not we should only retrieve values that have AddToRating enabled.</param>
        /// <param name="strImprovedName">Name to assign to the Improvement.</param>
        /// <param name="blnUnconditionalOnly">Whether to only fetch values for improvements that do not have a condition.</param>
        /// <param name="blnIncludeNonImproved">Whether to only fetch values for improvements that do not have an improvedname when specifying ImprovedNames.</param>
        /// <param name="funcValueGetter">Function for how to extract values for individual improvements.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        private static Task<Tuple<decimal, List<Improvement>>> MetaValueOfAsync(
            Character objCharacter, Improvement.ImprovementType eImprovementType,
            Func<Improvement, decimal> funcValueGetter,
            ConcurrentDictionary<ImprovementDictionaryKey,
                Tuple<decimal, List<Improvement>>> dicCachedValuesToUse,
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
        /// <param name="blnAddToRating">Whether or not we should only retrieve values that have AddToRating enabled.</param>
        /// <param name="strImprovedName">Name to assign to the Improvement.</param>
        /// <param name="blnUnconditionalOnly">Whether to only fetch values for improvements that do not have a condition.</param>
        /// <param name="blnIncludeNonImproved">Whether to only fetch values for improvements that do not have an improvedname when specifying ImprovedNames.</param>
        /// <param name="funcValueGetter">Function for how to extract values for individual improvements.</param>
        /// <param name="token">CancellationToken to listen to.</param>
        private static async Task<Tuple<decimal, List<Improvement>>> MetaValueOfCoreAsync(bool blnSync, Character objCharacter, Improvement.ImprovementType eImprovementType,
                                                                                          Func<Improvement, decimal> funcValueGetter,
                                                                                          ConcurrentDictionary<ImprovementDictionaryKey,
                                                                                              Tuple<decimal, List<Improvement>>> dicCachedValuesToUse,
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
                return new Tuple<decimal, List<Improvement>>(0, new List<Improvement>());

            if (string.IsNullOrWhiteSpace(strImprovedName))
                strImprovedName = string.Empty;

            // ReSharper disable once MethodHasAsyncOverload
            using (blnSync ? objCharacter.LockObject.EnterReadLock(token) : await objCharacter.LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                // These values are needed to prevent race conditions that could cause Chummer to crash
                Tuple<ImprovementDictionaryKey, ConcurrentDictionary<ImprovementDictionaryKey, Tuple<decimal, List<Improvement>>>> tupMyValueToCheck
                    = new Tuple<ImprovementDictionaryKey, ConcurrentDictionary<ImprovementDictionaryKey, Tuple<decimal, List<Improvement>>>>(
                        new ImprovementDictionaryKey(objCharacter, eImprovementType, strImprovedName),
                        dicCachedValuesToUse);
                Tuple<ImprovementDictionaryKey, ConcurrentDictionary<ImprovementDictionaryKey, Tuple<decimal, List<Improvement>>>> tupBlankValueToCheck
                    = new Tuple<ImprovementDictionaryKey, ConcurrentDictionary<ImprovementDictionaryKey, Tuple<decimal, List<Improvement>>>>(
                        new ImprovementDictionaryKey(objCharacter, eImprovementType, string.Empty),
                        dicCachedValuesToUse);

                // Only cache "default" ValueOf calls, otherwise there will be way too many values to cache
                bool blnFetchAndCacheResults = !blnAddToRating && blnUnconditionalOnly;

                // If we've got a value cached for the default ValueOf call for an improvementType, let's just return that
                List<Improvement> lstUsedImprovements = new List<Improvement>();
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
                                            return new Tuple<decimal, List<Improvement>>(0, new List<Improvement>());
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
                                                return new Tuple<decimal, List<Improvement>>(
                                                    0, new List<Improvement>());
                                            }

                                            throw;
                                        }
                                    }
                                }

                                ImprovementDictionaryKey objCacheKey
                                    = new ImprovementDictionaryKey(objCharacter, eImprovementType, strImprovedName);
                                if (dicCachedValuesToUse.TryGetValue(
                                        objCacheKey, out Tuple<decimal, List<Improvement>> tupCachedValue) &&
                                    tupCachedValue.Item1 != decimal.MinValue)
                                {
                                    // To make sure we do not inadvertently alter the cached list
                                    return new Tuple<decimal, List<Improvement>>(
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
                                foreach (KeyValuePair<ImprovementDictionaryKey, Tuple<decimal, List<Improvement>>>
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
                                    return new Tuple<decimal, List<Improvement>>(decCachedValue, lstUsedImprovements);
                            }
                        } while (blnRepeatCheckCache);
                    }
                    else
                    {
                        // The code is breaking here to remind you (the programmer) to add in caching functionality for this type of value.
                        // The more often this sort of value is used, the more caching is necessary and the more often we will break here,
                        // and the annoyance of constantly having your debugger break here should push you to adding in caching functionality.
                        Utils.BreakIfDebug();
                        lstUsedImprovements = new List<Improvement>();
                    }
                }
                else
                    lstUsedImprovements = new List<Improvement>();

                try
                {
                    List<Improvement> lstImprovementsToConsider
                        = new List<Improvement>(objCharacter.Improvements.Count);
                    foreach (Improvement objImprovement in objCharacter.Improvements)
                    {
                        if (objImprovement.ImproveType != eImprovementType || !objImprovement.Enabled)
                            continue;
                        if (blnUnconditionalOnly && !string.IsNullOrEmpty(objImprovement.Condition))
                            continue;
                        // Matrix initiative boosting gear does not help Living Personas
                        if ((eImprovementType == Improvement.ImprovementType.MatrixInitiativeDice
                             || eImprovementType == Improvement.ImprovementType.MatrixInitiative
                             || eImprovementType == Improvement.ImprovementType.MatrixInitiativeDiceAdd)
                            && objImprovement.ImproveSource == Improvement.ImprovementSource.Gear
                            && objCharacter.ActiveCommlink is Gear objCommlink
                            && objCommlink.Name == "Living Persona")
                            continue;
                        // Ignore items that apply to a Skill's Rating.
                        if (objImprovement.AddToRating != blnAddToRating)
                            continue;
                        // If an Improved Name has been passed, only retrieve values that have this Improved Name.
                        if (!string.IsNullOrEmpty(strImprovedName))
                        {
                            string strLoopImprovedName = objImprovement.ImprovedName;
                            if (strImprovedName != strLoopImprovedName
                                && !(blnIncludeNonImproved && string.IsNullOrWhiteSpace(strLoopImprovedName)))
                                continue;
                        }

                        lstImprovementsToConsider.Add(objImprovement);
                    }

                    List<Improvement> lstLoopImprovements;
                    Dictionary<string, List<Tuple<string, Improvement>>> dicUniquePairs
                        = new Dictionary<string, List<Tuple<string, Improvement>>>(lstImprovementsToConsider.Count);
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
                                                               out List<Tuple<string, Improvement>> lstUniquePairs))
                                {
                                    lstUniquePairs.Add(new Tuple<string, Improvement>(strUniqueName, objImprovement));
                                }
                                else
                                {
                                    dicUniquePairs.Add(strLoopImprovedName,
                                                       new List<Tuple<string, Improvement>>(1)
                                                       {
                                                           new Tuple<string, Improvement>(strUniqueName, objImprovement)
                                                       });
                                }

                                if (!dicValues.ContainsKey(strLoopImprovedName))
                                {
                                    dicValues.Add(strLoopImprovedName, 0);
                                    dicImprovementsForValues.Add(strLoopImprovedName, new List<Improvement>());
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
                                                             new List<Improvement>(objImprovement.Yield()));
                            }
                        }

                        List<Improvement> lstInnerLoopImprovements = new List<Improvement>(1);
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
                                                           out List<Tuple<string, Improvement>> lstUniquePairs))
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
                                    // Retrieve all of the items that are precedence1 and nothing else.
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
                                                               out List<Tuple<string, Improvement>> lstUniquePairs))
                                {
                                    lstUniquePairs.Add(new Tuple<string, Improvement>(strUniqueName, objImprovement));
                                }
                                else
                                {
                                    dicUniquePairs.Add(strLoopImprovedName,
                                                       new List<Tuple<string, Improvement>>(1)
                                                       {
                                                           new Tuple<string, Improvement>(strUniqueName, objImprovement)
                                                       });
                                }

                                if (!dicCustomValues.ContainsKey(strLoopImprovedName))
                                {
                                    dicCustomValues.Add(strLoopImprovedName, 0);
                                    dicCustomImprovementsForValues.Add(strLoopImprovedName, new List<Improvement>());
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
                                                                   new List<Improvement>(objImprovement.Yield()));
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
                                                           out List<Tuple<string, Improvement>> lstUniquePairs))
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
                                        (lstLoopImprovements ?? (lstLoopImprovements = new List<Improvement>(1))).Add(
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
                            Tuple<decimal, List<Improvement>> tupNewValue =
                                new Tuple<decimal, List<Improvement>>(decLoopValue,
                                                                      dicImprovementsForValues[strLoopImprovedName]);
                            if (dicCachedValuesToUse != null)
                            {
                                ImprovementDictionaryKey objLoopCacheKey =
                                    new ImprovementDictionaryKey(objCharacter, eImprovementType, strLoopImprovedName);
                                token.ThrowIfCancellationRequested();
                                if (!dicCachedValuesToUse.TryAdd(objLoopCacheKey, tupNewValue))
                                {
                                    List<Improvement> lstTemp = dicCachedValuesToUse.TryGetValue(
                                        objLoopCacheKey, out Tuple<decimal, List<Improvement>> tupTemp)
                                        ? tupTemp.Item2
                                        : new List<Improvement>();

                                    if (!ReferenceEquals(lstTemp, tupNewValue.Item2))
                                    {
                                        lstTemp.Clear();
                                        lstTemp.AddRange(tupNewValue.Item2);
                                        tupNewValue = new Tuple<decimal, List<Improvement>>(decLoopValue, lstTemp);
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

                    return new Tuple<decimal, List<Improvement>>(decReturn, lstUsedImprovements);
                }
                finally
                {
                    // As a final step, remove the tuple used to flag an improvement value as currently being cached
                    s_SetCurrentlyCalculatingValues.Remove(tupMyValueToCheck);
                    s_SetCurrentlyCalculatingValues.Remove(tupBlankValueToCheck);
                }
            }
        }

        /// <summary>
        /// Convert a string to an integer, converting "Rating" to a number where appropriate.
        /// </summary>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="strValue">String value to parse.</param>
        /// <param name="intRating">Integer value to replace "Rating" with.</param>
        public static int ValueToInt(Character objCharacter, string strValue, int intRating)
        {
            if (string.IsNullOrEmpty(strValue))
                return 0;
            //         Log.Enter("ValueToInt");
            //         Log.Info("strValue = " + strValue);
            //Log.Info("intRating = " + intRating.ToString());
            if (strValue.StartsWith("FixedValues(", StringComparison.Ordinal))
            {
                string[] strValues = strValue.TrimStartOnce("FixedValues(", true).TrimEndOnce(')')
                                             .Split(',', StringSplitOptions.RemoveEmptyEntries);
                strValue = strValues[Math.Max(Math.Min(strValues.Length, intRating) - 1, 0)];
            }

            if (strValue.ContainsAny("Rating".Yield().Concat(AttributeSection.AttributeStrings)))
            {
                string strReturn = strValue.Replace("Rating", intRating.ToString(GlobalSettings.InvariantCultureInfo));
                // If the value contain an CharacterAttribute name, replace it with the character's CharacterAttribute.
                strReturn = objCharacter.AttributeSection.ProcessAttributesInXPath(strReturn);
                strReturn = strReturn.Replace("/", " div ");

                //Log.Info("strValue = " + strValue);
                //Log.Info("strReturn = " + strReturn);

                // Treat this as a decimal value so any fractions can be rounded down. This is currently only used by the Boosted Reflexes Cyberware from SR2050.
                (bool blnIsSuccess, object objProcess) = CommonFunctions.EvaluateInvariantXPath(strReturn);

                //Log.Exit("ValueToInt");
                return blnIsSuccess ? ((double)objProcess).StandardRound() : 0;
            }

            //Log.Exit("ValueToInt");
            int.TryParse(strValue, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out int intReturn);
            return intReturn;
        }

        /// <summary>
        /// Convert a string to an integer, converting "Rating" to a number where appropriate.
        /// </summary>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="strValue">String value to parse.</param>
        /// <param name="intRating">Integer value to replace "Rating" with.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async ValueTask<int> ValueToIntAsync(Character objCharacter, string strValue, int intRating, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(strValue))
                return 0;
            //         Log.Enter("ValueToInt");
            //         Log.Info("strValue = " + strValue);
            //Log.Info("intRating = " + intRating.ToString());
            if (strValue.StartsWith("FixedValues(", StringComparison.Ordinal))
            {
                string[] strValues = strValue.TrimStartOnce("FixedValues(", true).TrimEndOnce(')')
                                             .Split(',', StringSplitOptions.RemoveEmptyEntries);
                strValue = strValues[Math.Max(Math.Min(strValues.Length, intRating) - 1, 0)];
            }

            if (strValue.ContainsAny("Rating".Yield().Concat(AttributeSection.AttributeStrings)))
            {
                string strReturn = strValue.Replace("Rating", intRating.ToString(GlobalSettings.InvariantCultureInfo));
                // If the value contain an CharacterAttribute name, replace it with the character's CharacterAttribute.
                strReturn = await (await objCharacter.GetAttributeSectionAsync(token).ConfigureAwait(false)).ProcessAttributesInXPathAsync(strReturn, token: token).ConfigureAwait(false);
                strReturn = strReturn.Replace("/", " div ");

                //Log.Info("strValue = " + strValue);
                //Log.Info("strReturn = " + strReturn);

                // Treat this as a decimal value so any fractions can be rounded down. This is currently only used by the Boosted Reflexes Cyberware from SR2050.
                (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(strReturn, token).ConfigureAwait(false);

                //Log.Exit("ValueToInt");
                return blnIsSuccess ? ((double)objProcess).StandardRound() : 0;
            }

            //Log.Exit("ValueToInt");
            int.TryParse(strValue, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out int intReturn);
            return intReturn;
        }

        /// <summary>
        /// Convert a string to a decimal, converting "Rating" to a number where appropriate.
        /// </summary>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="strValue">String value to parse.</param>
        /// <param name="intRating">Integer value to replace "Rating" with.</param>
        public static decimal ValueToDec(Character objCharacter, string strValue, int intRating)
        {
            if (string.IsNullOrEmpty(strValue))
                return 0;
            //         Log.Enter("ValueToInt");
            //         Log.Info("strValue = " + strValue);
            //Log.Info("intRating = " + intRating.ToString());
            if (strValue.StartsWith("FixedValues(", StringComparison.Ordinal))
            {
                string[] strValues = strValue.TrimStartOnce("FixedValues(", true).TrimEndOnce(')')
                                             .Split(',', StringSplitOptions.RemoveEmptyEntries);
                strValue = strValues[Math.Max(Math.Min(strValues.Length, intRating) - 1, 0)];
            }

            if (strValue.ContainsAny("Rating".Yield().Concat(AttributeSection.AttributeStrings)))
            {
                string strReturn = strValue.Replace("Rating", intRating.ToString(GlobalSettings.InvariantCultureInfo));
                // If the value contain an CharacterAttribute name, replace it with the character's CharacterAttribute.
                strReturn = objCharacter.AttributeSection.ProcessAttributesInXPath(strReturn);
                strReturn = strReturn.Replace("/", " div ");

                //Log.Info("strValue = " + strValue);
                //Log.Info("strReturn = " + strReturn);

                // Treat this as a decimal value so any fractions can be rounded down. This is currently only used by the Boosted Reflexes Cyberware from SR2050.
                (bool blnIsSuccess, object objProcess) = CommonFunctions.EvaluateInvariantXPath(strReturn);

                //Log.Exit("ValueToInt");
                return blnIsSuccess ? Convert.ToDecimal((double)objProcess) : 0;
            }

            //Log.Exit("ValueToInt");
            decimal.TryParse(strValue, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decimal decReturn);
            return decReturn;
        }

        /// <summary>
        /// Convert a string to a decimal, converting "Rating" to a number where appropriate.
        /// </summary>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="strValue">String value to parse.</param>
        /// <param name="intRating">Integer value to replace "Rating" with.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async ValueTask<decimal> ValueToDecAsync(Character objCharacter, string strValue, int intRating, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(strValue))
                return 0;
            //         Log.Enter("ValueToInt");
            //         Log.Info("strValue = " + strValue);
            //Log.Info("intRating = " + intRating.ToString());
            if (strValue.StartsWith("FixedValues(", StringComparison.Ordinal))
            {
                string[] strValues = strValue.TrimStartOnce("FixedValues(", true).TrimEndOnce(')')
                                             .Split(',', StringSplitOptions.RemoveEmptyEntries);
                strValue = strValues[Math.Max(Math.Min(strValues.Length, intRating) - 1, 0)];
            }

            if (strValue.ContainsAny("Rating".Yield().Concat(AttributeSection.AttributeStrings)))
            {
                string strReturn = strValue.Replace("Rating", intRating.ToString(GlobalSettings.InvariantCultureInfo));
                // If the value contain an CharacterAttribute name, replace it with the character's CharacterAttribute.
                strReturn = await (await objCharacter.GetAttributeSectionAsync(token).ConfigureAwait(false)).ProcessAttributesInXPathAsync(strReturn, token: token).ConfigureAwait(false);
                strReturn = strReturn.Replace("/", " div ");

                //Log.Info("strValue = " + strValue);
                //Log.Info("strReturn = " + strReturn);

                // Treat this as a decimal value so any fractions can be rounded down. This is currently only used by the Boosted Reflexes Cyberware from SR2050.
                (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(strReturn, token).ConfigureAwait(false);

                //Log.Exit("ValueToInt");
                return blnIsSuccess ? Convert.ToDecimal((double)objProcess) : 0;
            }

            //Log.Exit("ValueToInt");
            decimal.TryParse(strValue, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decimal decReturn);
            return decReturn;
        }

        public static string DoSelectSkill(XmlNode xmlBonusNode, Character objCharacter, int intRating,
                                           string strFriendlyName, ref bool blnIsKnowledgeSkill)
        {
            if (xmlBonusNode == null)
                throw new ArgumentNullException(nameof(xmlBonusNode));
            if (objCharacter == null)
                throw new ArgumentNullException(nameof(objCharacter));
            string strSelectedSkill;
            blnIsKnowledgeSkill = blnIsKnowledgeSkill
                                  || xmlBonusNode.Attributes?["knowledgeskills"]?.InnerText == bool.TrueString;
            if (blnIsKnowledgeSkill)
            {
                int intMinimumRating = 0;
                string strMinimumRating = xmlBonusNode.Attributes?["minimumrating"]?.InnerText;
                if (!string.IsNullOrWhiteSpace(strMinimumRating))
                    intMinimumRating = ValueToInt(objCharacter, strMinimumRating, intRating);
                int intMaximumRating = int.MaxValue;
                string strMaximumRating = xmlBonusNode.Attributes?["maximumrating"]?.InnerText;
                string strPrompt = xmlBonusNode.Attributes?["prompt"]?.InnerText ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(strMaximumRating))
                    intMaximumRating = ValueToInt(objCharacter, strMaximumRating, intRating);

                using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                out HashSet<string>
                                                                    setAllowedCategories))
                {
                    string strOnlyCategory = xmlBonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@skillcategory")?.Value;
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
                                    setAllowedCategories.Add(objNode.InnerText);
                                }
                            }
                        }
                    }

                    using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                    out HashSet<string>
                                                                        setForbiddenCategories))
                    {
                        string strExcludeCategory = xmlBonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@excludecategory")?.Value;
                        if (!string.IsNullOrEmpty(strExcludeCategory))
                        {
                            setForbiddenCategories.AddRange(
                                strExcludeCategory.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries)
                                                  .Select(x => x.Trim()));
                        }

                        using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                        out HashSet<string>
                                                                            setAllowedNames))
                        {
                            if (!string.IsNullOrEmpty(ForcedValue))
                            {
                                setAllowedNames.Add(ForcedValue);
                            }
                            else if (!string.IsNullOrEmpty(strPrompt))
                            {
                                setAllowedNames.Add(strPrompt);
                            }
                            else
                            {
                                string strLimitToSkill = xmlBonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@limittoskill")?.Value;
                                if (!string.IsNullOrEmpty(strLimitToSkill))
                                {
                                    setAllowedNames.AddRange(
                                        strLimitToSkill.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries)
                                                       .Select(x => x.Trim()));
                                }
                            }

                            using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                            out HashSet<string>
                                                                                setAllowedLinkedAttributes))
                            {
                                string strLimitToAttribute
                                    = xmlBonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@limittoattribute")?.Value;
                                if (!string.IsNullOrEmpty(strLimitToAttribute))
                                {
                                    setAllowedLinkedAttributes.AddRange(
                                        strLimitToAttribute.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries)
                                                           .Select(x => x.Trim()));
                                }

                                using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                                               out List<ListItem> lstDropdownItems))
                                {
                                    using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                               out HashSet<string>
                                                   setProcessedSkillNames))
                                    {
                                        using (objCharacter.LockObject.EnterReadLock())
                                        {
                                            foreach (KnowledgeSkill objKnowledgeSkill in objCharacter.SkillsSection
                                                         .KnowledgeSkills)
                                            {
                                                if (setAllowedCategories?.Contains(objKnowledgeSkill.SkillCategory) != false
                                                    &&
                                                    setForbiddenCategories?.Contains(objKnowledgeSkill.SkillCategory)
                                                    != true &&
                                                    setAllowedNames?.Contains(objKnowledgeSkill.Name) != false &&
                                                    setAllowedLinkedAttributes?.Contains(objKnowledgeSkill.Attribute)
                                                    != false)
                                                {
                                                    int intSkillRating = objKnowledgeSkill.Rating;
                                                    if (intSkillRating >= intMinimumRating && intRating < intMaximumRating)
                                                    {
                                                        lstDropdownItems.Add(
                                                            new ListItem(objKnowledgeSkill.Name,
                                                                         objKnowledgeSkill.CurrentDisplayName));
                                                    }
                                                }

                                                setProcessedSkillNames.Add(objKnowledgeSkill.Name);
                                            }

                                            if (!string.IsNullOrEmpty(strPrompt)
                                                && !setProcessedSkillNames.Contains(strPrompt))
                                            {
                                                lstDropdownItems.Add(
                                                    new ListItem(strPrompt, objCharacter.TranslateExtra(strPrompt)));
                                                setProcessedSkillNames.Add(strPrompt);
                                            }

                                            if (intMinimumRating <= 0)
                                            {
                                                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                           out StringBuilder sbdFilter))
                                                {
                                                    if (setAllowedCategories?.Count > 0)
                                                    {
                                                        sbdFilter.Append('(');
                                                        foreach (string strCategory in setAllowedCategories)
                                                        {
                                                            sbdFilter.Append("category = ").Append(strCategory.CleanXPath())
                                                                     .Append(" or ");
                                                        }

                                                        sbdFilter.Length -= 4;
                                                        sbdFilter.Append(')');
                                                    }

                                                    if (setForbiddenCategories?.Count > 0)
                                                    {
                                                        sbdFilter.Append(sbdFilter.Length > 0 ? " and not(" : "not(");
                                                        foreach (string strCategory in setForbiddenCategories)
                                                        {
                                                            sbdFilter.Append("category = ").Append(strCategory.CleanXPath())
                                                                     .Append(" or ");
                                                        }

                                                        sbdFilter.Length -= 4;
                                                        sbdFilter.Append(')');
                                                    }

                                                    if (setAllowedNames?.Count > 0)
                                                    {
                                                        sbdFilter.Append(sbdFilter.Length > 0 ? " and (" : "(");
                                                        foreach (string strName in setAllowedNames)
                                                        {
                                                            sbdFilter.Append("name = ").Append(strName.CleanXPath())
                                                                     .Append(" or ");
                                                        }

                                                        sbdFilter.Length -= 4;
                                                        sbdFilter.Append(')');
                                                    }

                                                    if (setProcessedSkillNames.Count > 0)
                                                    {
                                                        sbdFilter.Append(sbdFilter.Length > 0 ? " and not(" : "not(");
                                                        foreach (string strName in setProcessedSkillNames)
                                                        {
                                                            sbdFilter.Append("name = ").Append(strName.CleanXPath())
                                                                     .Append(" or ");
                                                        }

                                                        sbdFilter.Length -= 4;
                                                        sbdFilter.Append(')');
                                                    }

                                                    if (setAllowedLinkedAttributes?.Count > 0)
                                                    {
                                                        sbdFilter.Append(sbdFilter.Length > 0 ? " and (" : "(");
                                                        foreach (string strAttribute in setAllowedLinkedAttributes)
                                                        {
                                                            sbdFilter.Append("attribute = ")
                                                                     .Append(strAttribute.CleanXPath())
                                                                     .Append(" or ");
                                                        }

                                                        sbdFilter.Length -= 4;
                                                        sbdFilter.Append(')');
                                                    }

                                                    string strFilter = sbdFilter.Length > 0
                                                        ? ") and (" + sbdFilter
                                                        : string.Empty;
                                                    foreach (XPathNavigator xmlSkill in objCharacter
                                                                 .LoadDataXPath("skills.xml")
                                                                 .Select(
                                                                     "/chummer/knowledgeskills/skill[(not(hide)"
                                                                     + strFilter + ")]"))
                                                    {
                                                        string strName = xmlSkill.SelectSingleNodeAndCacheExpression("name")?.Value;
                                                        if (!string.IsNullOrEmpty(strName))
                                                            lstDropdownItems.Add(
                                                                new ListItem(
                                                                    strName,
                                                                    xmlSkill.SelectSingleNodeAndCacheExpression("translate")
                                                                            ?.Value
                                                                    ?? strName));
                                                    }
                                                }
                                            }
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

                                        using (ThreadSafeForm<SelectItem> frmPickSkill
                                               = ThreadSafeForm<SelectItem>.Get(() => new SelectItem
                                               {
                                                   Description = LanguageManager.GetString("Title_SelectSkill"),
                                                   AllowAutoSelect = string.IsNullOrWhiteSpace(strPrompt)
                                               }))
                                        {
                                            if (setAllowedNames != null && string.IsNullOrWhiteSpace(strPrompt))
                                                frmPickSkill.MyForm.SetGeneralItemsMode(lstDropdownItems);
                                            else
                                                frmPickSkill.MyForm.SetDropdownItemsMode(lstDropdownItems);

                                            if (frmPickSkill.ShowDialogSafe(objCharacter) == DialogResult.Cancel)
                                            {
                                                throw new AbortedException();
                                            }

                                            strSelectedSkill = frmPickSkill.MyForm.SelectedItem;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (!string.IsNullOrEmpty(ForcedValue))
            {
                (bool blnIsExotic, string strExoticName)
                    = ExoticSkill.IsExoticSkillNameTuple(objCharacter, ForcedValue);
                string strFilter;
                if (blnIsExotic)
                {
                    strFilter = "/chummer/skills/skill[name = "
                                + strExoticName.CleanXPath() + " and exotic = 'True' and ("
                                + objCharacter.Settings.BookXPath() + ")]";
                }
                else
                {
                    strFilter = "/chummer/skills/skill[name = "
                                + ForcedValue.CleanXPath() + " and not(exotic = 'True') and ("
                                + objCharacter.Settings.BookXPath() + ")]";
                }

                XPathNavigator xmlSkillNode = objCharacter.LoadDataXPath("skills.xml").SelectSingleNode(strFilter) ?? throw new AbortedException();
                int intMinimumRating = 0;
                string strMinimumRating = xmlBonusNode.Attributes?["minimumrating"]?.InnerText;
                if (!string.IsNullOrWhiteSpace(strMinimumRating))
                    intMinimumRating = ValueToInt(objCharacter, strMinimumRating, intRating);
                int intMaximumRating = int.MaxValue;
                string strMaximumRating = xmlBonusNode.Attributes?["maximumrating"]?.InnerText;
                if (!string.IsNullOrWhiteSpace(strMaximumRating))
                    intMaximumRating = ValueToInt(objCharacter, strMaximumRating, intRating);
                if (intMinimumRating != 0 || intMaximumRating != int.MaxValue)
                {
                    Skill objExistingSkill = objCharacter.SkillsSection.GetActiveSkill(ForcedValue);
                    if (objExistingSkill == null)
                    {
                        if (intMinimumRating > 0)
                            throw new AbortedException();
                    }
                    else
                    {
                        int intCurrentRating = objExistingSkill.TotalBaseRating;
                        if (intCurrentRating > intMaximumRating || intCurrentRating < intMinimumRating)
                            throw new AbortedException();
                    }
                }

                XPathNavigator xmlSkillCategories = xmlBonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("skillcategories");
                if (xmlSkillCategories != null)
                {
                    string strCategory = xmlSkillNode.SelectSingleNodeAndCacheExpression("category")?.Value
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

                string strTemp = xmlBonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@skillcategory")?.Value;
                if (!string.IsNullOrEmpty(strTemp))
                {
                    string strCategory = xmlSkillNode.SelectSingleNodeAndCacheExpression("category")?.Value
                                         ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(strCategory) || !strTemp
                                                                   .SplitNoAlloc(
                                                                       ',', StringSplitOptions.RemoveEmptyEntries)
                                                                   .Contains(strCategory))
                        throw new AbortedException();
                }

                strTemp = xmlBonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@skillgroup")?.Value;
                if (!string.IsNullOrEmpty(strTemp))
                {
                    string strSkillGroup = xmlSkillNode.SelectSingleNodeAndCacheExpression("skillgroup")?.Value
                                           ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(strSkillGroup) || !strTemp
                                                                     .SplitNoAlloc(
                                                                         ',', StringSplitOptions.RemoveEmptyEntries)
                                                                     .Contains(strSkillGroup))
                        throw new AbortedException();
                }

                strTemp = xmlBonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@excludecategory")?.Value;
                if (!string.IsNullOrEmpty(strTemp))
                {
                    string strCategory = xmlSkillNode.SelectSingleNodeAndCacheExpression("category")?.Value
                                         ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(strCategory) && strTemp
                                                                   .SplitNoAlloc(
                                                                       ',', StringSplitOptions.RemoveEmptyEntries)
                                                                   .Contains(strCategory))
                        throw new AbortedException();
                }

                strTemp = xmlBonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@excludeskillgroup")?.Value;
                if (!string.IsNullOrEmpty(strTemp))
                {
                    string strSkillGroup = xmlSkillNode.SelectSingleNodeAndCacheExpression("skillgroup")?.Value
                                           ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(strSkillGroup) && strTemp
                                                                     .SplitNoAlloc(
                                                                         ',', StringSplitOptions.RemoveEmptyEntries)
                                                                     .Contains(strSkillGroup))
                        throw new AbortedException();
                }

                strTemp = xmlBonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@limittoskill")?.Value;
                if (!string.IsNullOrEmpty(strTemp) && !strTemp
                                                       .SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries)
                                                       .Contains(ForcedValue))
                    throw new AbortedException();
                strTemp = xmlBonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@excludeskill")?.Value;
                if (!string.IsNullOrEmpty(strTemp) && strTemp
                                                      .SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries)
                                                      .Contains(ForcedValue))
                    throw new AbortedException();
                strTemp = xmlBonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@limittoattribute")?.Value;
                if (!string.IsNullOrEmpty(strTemp))
                {
                    string strAttribute = xmlSkillNode.SelectSingleNodeAndCacheExpression("attribute")?.Value
                                          ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(strAttribute) || !strTemp
                                                                    .SplitNoAlloc(
                                                                        ',', StringSplitOptions.RemoveEmptyEntries)
                                                                    .Contains(strAttribute))
                        throw new AbortedException();
                }

                strSelectedSkill = ForcedValue;
            }
            else
            {
                // Display the Select Skill window and record which Skill was selected.
                using (ThreadSafeForm<SelectSkill> frmPickSkill
                       = ThreadSafeForm<SelectSkill>.Get(() => new SelectSkill(objCharacter, strFriendlyName)
                       {
                           Description = !string.IsNullOrEmpty(strFriendlyName)
                               ? string.Format(GlobalSettings.CultureInfo,
                                               LanguageManager.GetString("String_Improvement_SelectSkillNamed"),
                                               strFriendlyName)
                               : LanguageManager.GetString("String_Improvement_SelectSkill")
                       }))
                {
                    string strMinimumRating = xmlBonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@minimumrating")?.Value;
                    if (!string.IsNullOrWhiteSpace(strMinimumRating))
                        frmPickSkill.MyForm.MinimumRating = ValueToInt(objCharacter, strMinimumRating, intRating);
                    string strMaximumRating = xmlBonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@maximumrating")?.Value;
                    if (!string.IsNullOrWhiteSpace(strMaximumRating))
                        frmPickSkill.MyForm.MaximumRating = ValueToInt(objCharacter, strMaximumRating, intRating);

                    XmlNode xmlSkillCategories = xmlBonusNode["skillcategories"];
                    if (xmlSkillCategories != null)
                        frmPickSkill.MyForm.LimitToCategories = xmlSkillCategories;
                    string strTemp = xmlBonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@skillcategory")?.Value;
                    if (!string.IsNullOrEmpty(strTemp))
                        frmPickSkill.MyForm.OnlyCategory = strTemp;
                    strTemp = xmlBonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@skillgroup")?.Value;
                    if (!string.IsNullOrEmpty(strTemp))
                        frmPickSkill.MyForm.OnlySkillGroup = strTemp;
                    strTemp = xmlBonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@excludecategory")?.Value;
                    if (!string.IsNullOrEmpty(strTemp))
                        frmPickSkill.MyForm.ExcludeCategory = strTemp;
                    strTemp = xmlBonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@excludeskillgroup")?.Value;
                    if (!string.IsNullOrEmpty(strTemp))
                        frmPickSkill.MyForm.ExcludeSkillGroup = strTemp;
                    strTemp = xmlBonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@limittoskill")?.Value;
                    if (!string.IsNullOrEmpty(strTemp))
                        frmPickSkill.MyForm.LimitToSkill = strTemp;
                    strTemp = xmlBonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@excludeskill")?.Value;
                    if (!string.IsNullOrEmpty(strTemp))
                        frmPickSkill.MyForm.ExcludeSkill = strTemp;
                    strTemp = xmlBonusNode.SelectSingleNodeAndCacheExpressionAsNavigator("@limittoattribute")?.Value;
                    if (!string.IsNullOrEmpty(strTemp))
                        frmPickSkill.MyForm.LinkedAttribute = strTemp;

                    if (frmPickSkill.ShowDialogSafe(objCharacter) == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    strSelectedSkill = frmPickSkill.MyForm.SelectedSkill;
                }
            }

            return strSelectedSkill;
        }

        #endregion Helper Methods

        #region Improvement System

        /// <summary>
        /// Create all of the Improvements for an XML Node.
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
        /// Create all of the Improvements for an XML Node.
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
        /// Create all of the Improvements for an XML Node.
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
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdTrace))
            {
                sbdTrace.Append("objImprovementSource = ").AppendLine(objImprovementSource.ToString());
                sbdTrace.Append("strSourceName = ").AppendLine(strSourceName);
                sbdTrace.Append("nodBonus = ").AppendLine(nodBonus?.OuterXml);
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
                    if (nodBonus == null)
                    {
                        _strForcedValue = string.Empty;
                        _strLimitSelection = string.Empty;
                        return true;
                    }

                    _strSelectedValue = string.Empty;

                    sbdTrace.Append("_strForcedValue = ").AppendLine(_strForcedValue);
                    sbdTrace.Append("_strLimitSelection = ").AppendLine(_strLimitSelection);

                    // If no friendly name was provided, use the one from SourceName.
                    if (string.IsNullOrEmpty(strFriendlyName))
                        strFriendlyName = strSourceName;

                    if (nodBonus.HasChildNodes)
                    {
                        string strUnique = nodBonus.Attributes?["unique"]?.InnerText ?? string.Empty;
                        sbdTrace.AppendLine("Has Child Nodes");
                        if (nodBonus["selecttext"] != null)
                        {
                            sbdTrace.AppendLine("selecttext");

                            try
                            {
                                if (!string.IsNullOrEmpty(_strForcedValue))
                                {
                                    LimitSelection = _strForcedValue;
                                }
                                else if (objCharacter != null)
                                {
                                    if (objCharacter.PushText.TryPop(out string strText))
                                    {
                                        LimitSelection = strText;
                                    }
                                }

                                sbdTrace.Append("SelectedValue = ").AppendLine(SelectedValue);
                                sbdTrace.Append("LimitSelection = ").AppendLine(LimitSelection);

                                if (!string.IsNullOrEmpty(LimitSelection))
                                {
                                    _strSelectedValue = LimitSelection;
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
                                            ForcedValue = string.Empty;
                                            LimitSelection = string.Empty;
                                            return false;
                                        }

                                        _strSelectedValue = frmPickText.MyForm.SelectedValue;
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
                                        ForcedValue = string.Empty;
                                        LimitSelection = string.Empty;
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
                                    using (new FetchSafelyFromPool<List<ListItem>>(
                                               Utils.ListItemListPool, out List<ListItem> lstItems))
                                    {
                                        //TODO: While this is a safeguard for uniques, preference should be that we're selecting distinct values in the xpath.
                                        //Use XPath2.0 distinct-values operators instead. REQUIRES > .Net 4.6
                                        using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                   out HashSet<string> setUsedValues))
                                        {
                                            foreach (XPathNavigator objNode in xmlDoc.Select(strXPath))
                                            {
                                                // First check if we are using a list of language keys
                                                string strKey = objNode.Name == "key" ? objNode.Value : string.Empty;
                                                if (string.IsNullOrEmpty(strKey))
                                                {
                                                    string strName
                                                        = (blnSync
                                                              // ReSharper disable once MethodHasAsyncOverload
                                                              ? objNode.SelectSingleNodeAndCacheExpression(
                                                                  "name", token)
                                                              : await objNode
                                                                      .SelectSingleNodeAndCacheExpressionAsync(
                                                                          "name", token: token).ConfigureAwait(false))
                                                          ?.Value
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
                                                                strKey, GlobalSettings.DefaultLanguage, token: token)
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
                                                                    ? LanguageManager.GetString(strKey, token: token)
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
                                            ForcedValue = string.Empty;
                                            LimitSelection = string.Empty;
                                            return false;
                                        }

                                        string strSelectText = blnSync
                                            // ReSharper disable once MethodHasAsyncOverload
                                            ? LanguageManager.GetString("String_Improvement_SelectText", token: token)
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
                                                   : await ThreadSafeForm<SelectItem>.GetAsync(() => new SelectItem
                                                   {
                                                       Description = string.Format(GlobalSettings.CultureInfo,
                                                           strSelectText,
                                                           strFriendlyName)
                                                   }, token).ConfigureAwait(false))
                                        {
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
                                                ForcedValue = string.Empty;
                                                LimitSelection = string.Empty;
                                                return false;
                                            }

                                            _strSelectedValue = frmSelect.MyForm.SelectedItem;
                                        }
                                    }
                                }

                                sbdTrace.Append("SelectedValue = ").AppendLine(SelectedValue);
                                sbdTrace.Append("strSourceName = ").AppendLine(strSourceName);

                                if (blnAddImprovementsToCharacter)
                                {
                                    // Create the Improvement.
                                    sbdTrace.AppendLine("Calling CreateImprovement");
                                    if (blnSync)
                                        // ReSharper disable once MethodHasAsyncOverload
                                        CreateImprovement(objCharacter, _strSelectedValue, objImprovementSource,
                                                          strSourceName,
                                                          Improvement.ImprovementType.Text,
                                                          strUnique, token: token);
                                    else
                                        await CreateImprovementAsync(objCharacter, _strSelectedValue,
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
                                    if (ProcessBonus(objCharacter, objImprovementSource, ref strSourceName, intRating,
                                                     strFriendlyName,
                                                     bonusNode, strUnique, !blnAddImprovementsToCharacter))
                                        continue;
                                    // ReSharper disable once MethodHasAsyncOverload
                                    Rollback(objCharacter, token);
                                    sbdTrace.AppendLine("Bonus processing unsuccessful, returning.");
                                    return false;
                                }
                            }
                            catch
                            {
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
                            catch
                            {
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
                        Commit(objCharacter);
                        sbdTrace.AppendLine("Finished committing improvements");
                    }
                    else
                    {
                        sbdTrace.AppendLine("Calling scheduled Rollback due to blnAddImprovementsToCharacter = false");
                        if (blnSync)
                            // ReSharper disable once MethodHasAsyncOverload
                            Rollback(objCharacter, token);
                        else
                            await RollbackAsync(objCharacter, token).ConfigureAwait(false);
                        sbdTrace.AppendLine("Returned from scheduled Rollback");
                    }

                    // If the bonus should not bubble up SelectedValues from its improvements, reset it to empty.
                    if (nodBonus.Attributes?["useselected"]?.InnerText == bool.FalseString)
                    {
                        SelectedValue = string.Empty;
                    }

                    // Clear the Forced Value and Limit Selection strings once we're done to prevent these from forcing their values on other Improvements.
                    _strForcedValue = string.Empty;
                    _strLimitSelection = string.Empty;
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
                                         bool blnIgnoreMethodNotFound = false)
        {
            if (bonusNode == null)
                return false;
            //As this became a really big nest of **** that it searched past, several places having equal paths just adding a different improvement, a more flexible method was chosen.
            //So far it is just a slower Dictionary<string, Action> but should (in theory...) be able to leverage this in the future to do it smarter with methods that are the same but
            //getting a different parameter injected

            AddImprovementCollection container = new AddImprovementCollection(objCharacter, objImprovementSource,
                                                                              strSourceName, strUnique, _strForcedValue,
                                                                              _strLimitSelection, SelectedValue,
                                                                              strFriendlyName,
                                                                              intRating);

            Action<XmlNode> objImprovementMethod
                = ImprovementMethods.GetMethod(bonusNode.Name.ToUpperInvariant(), container);
            if (objImprovementMethod != null)
            {
                try
                {
                    using (objCharacter.LockObject.EnterWriteLock())
                        objImprovementMethod.Invoke(bonusNode);
                }
                catch (AbortedException)
                {
                    Rollback(objCharacter);
                    return false;
                }

                strSourceName = container.SourceName;
                _strForcedValue = container.ForcedValue;
                _strLimitSelection = container.LimitSelection;
                _strSelectedValue = container.SelectedValue;
            }
            else if (blnIgnoreMethodNotFound || bonusNode.ChildNodes.Count == 0)
            {
                return true;
            }
            else if (bonusNode.NodeType != XmlNodeType.Comment)
            {
                Utils.BreakIfDebug();
                Log.Warn(new object[] { "Tried to get unknown bonus", bonusNode.OuterXml });
                return false;
            }

            return true;
        }

        private static async ValueTask<Tuple<bool, string>> ProcessBonusAsync(Character objCharacter, Improvement.ImprovementSource objImprovementSource,
                                         string strSourceName,
                                         int intRating, string strFriendlyName, XmlNode bonusNode, string strUnique,
                                         bool blnIgnoreMethodNotFound = false, CancellationToken token = default)
        {
            if (bonusNode == null)
                return new Tuple<bool, string>(false, strSourceName);
            //As this became a really big nest of **** that it searched past, several places having equal paths just adding a different improvement, a more flexible method was chosen.
            //So far it is just a slower Dictionary<string, Action> but should (in theory...) be able to leverage this in the future to do it smarter with methods that are the same but
            //getting a different parameter injected

            AddImprovementCollection container = new AddImprovementCollection(objCharacter, objImprovementSource,
                                                                              strSourceName, strUnique, _strForcedValue,
                                                                              _strLimitSelection, SelectedValue,
                                                                              strFriendlyName,
                                                                              intRating);

            Action<XmlNode> objImprovementMethod
                = ImprovementMethods.GetMethod(bonusNode.Name.ToUpperInvariant(), container);
            if (objImprovementMethod != null)
            {
                try
                {
                    IAsyncDisposable objLocker = await objCharacter.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        objImprovementMethod.Invoke(bonusNode);
                    }
                    finally
                    {
                        await objLocker.DisposeAsync().ConfigureAwait(false);
                    }
                }
                catch (AbortedException)
                {
                    await RollbackAsync(objCharacter, token).ConfigureAwait(false);
                    return new Tuple<bool, string>(false, strSourceName);
                }

                strSourceName = container.SourceName;
                _strForcedValue = container.ForcedValue;
                _strLimitSelection = container.LimitSelection;
                _strSelectedValue = container.SelectedValue;
            }
            else if (blnIgnoreMethodNotFound || bonusNode.ChildNodes.Count == 0)
            {
                return new Tuple<bool, string>(true, strSourceName);
            }
            else if (bonusNode.NodeType != XmlNodeType.Comment)
            {
                Utils.BreakIfDebug();
                Log.Warn(new object[] { "Tried to get unknown bonus", bonusNode.OuterXml });
                return new Tuple<bool, string>(false, strSourceName);
            }

            return new Tuple<bool, string>(true, strSourceName);
        }

        public static void EnableImprovements(Character objCharacter, IEnumerable<Improvement> objImprovementList, CancellationToken token = default)
        {
            EnableImprovements(objCharacter, objImprovementList.ToList(), token);
        }

        public static void EnableImprovements(Character objCharacter, Improvement objImprovement, CancellationToken token = default)
        {
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
                    objImprovement.Enabled = true;
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
                            if (strUniqueName == "enableattribute")
                            {
                                switch (strImprovedName)
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
                            switch (strUniqueName)
                            {
                                case "enabletab":
                                    switch (strImprovedName)
                                    {
                                        case "Magician":
                                            objCharacter.MagicianEnabled = true;
                                            break;

                                        case "Adept":
                                            objCharacter.AdeptEnabled = true;
                                            break;

                                        case "Technomancer":
                                            objCharacter.TechnomancerEnabled = true;
                                            break;

                                        case "Advanced Programs":
                                            objCharacter.AdvancedProgramsEnabled = true;
                                            break;

                                        case "Critter":
                                            objCharacter.CritterEnabled = true;
                                            break;
                                    }

                                    break;
                                // Determine if access to any special tabs has been regained
                                case "disabletab":
                                    switch (strImprovedName)
                                    {
                                        case "Cyberware":
                                            objCharacter.CyberwareDisabled = true;
                                            break;

                                        case "Initiation":
                                            objCharacter.InitiationForceDisabled = true;
                                            break;
                                    }

                                    break;
                            }

                            break;

                        case Improvement.ImprovementType.PrototypeTranshuman:
                            // Legacy compatibility
                            if (string.IsNullOrEmpty(strImprovedName))
                                objCharacter.PrototypeTranshuman = 1;
                            else
                                objCharacter.PrototypeTranshuman
                                    += Convert.ToDecimal(strImprovedName, GlobalSettings.InvariantCultureInfo);
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
                                = objCharacter.Weapons.DeepFirstOrDefault(x => x.Children,
                                                                          x => x.InternalId == strImprovedName)
                                  ??
                                  objCharacter.Vehicles.FindVehicleWeapon(strImprovedName, out _, out _, out _);
                            if (objWeapon != null)
                                objWeapon.Equipped = true;
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
                            foreach (Skill objSkill in await objCharacter.SkillsSection.GetActiveSkillsFromDataAsync(
                                         eFilterOption, false, objImprovement.Target, token).ConfigureAwait(false))
                            {
                                objSkill.ForceDisabled = false;
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
                            if (objCyberware != null)
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

                objImprovementList.ProcessRelevantEvents(token);
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
                    objImprovement.Enabled = false;
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
                                                                         .AsTask(), token: token).ConfigureAwait(false);
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
                                    await objCharacter.SkillsSection.KnowsoftSkills.RemoveAllAsync(
                                        x => x.InternalId == strImprovedName, token: token).ConfigureAwait(false);
                                }
                            }

                            break;

                        case Improvement.ImprovementType.Attribute:
                            // Determine if access to any Special Attributes have been lost.
                            if (strUniqueName == "enableattribute" && !blnHasDuplicate)
                            {
                                switch (strImprovedName)
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
                                switch (strUniqueName)
                                {
                                    case "enabletab":
                                        switch (strImprovedName)
                                        {
                                            case "Magician":
                                                objCharacter.MagicianEnabled = false;
                                                break;

                                            case "Adept":
                                                objCharacter.AdeptEnabled = false;
                                                break;

                                            case "Technomancer":
                                                objCharacter.TechnomancerEnabled = false;
                                                break;

                                            case "Advanced Programs":
                                                objCharacter.AdvancedProgramsEnabled = false;
                                                break;

                                            case "Critter":
                                                objCharacter.CritterEnabled = false;
                                                break;
                                        }

                                        break;
                                    // Determine if access to any special tabs has been regained
                                    case "disabletab":
                                        switch (strImprovedName)
                                        {
                                            case "Cyberware":
                                                objCharacter.CyberwareDisabled = false;
                                                break;

                                            case "Initiation":
                                                objCharacter.InitiationForceDisabled = false;
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
                                    objCharacter.PrototypeTranshuman = 0;
                            }
                            else
                                objCharacter.PrototypeTranshuman
                                    -= Convert.ToDecimal(strImprovedName, GlobalSettings.InvariantCultureInfo);

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
                                = objCharacter.Weapons.DeepFirstOrDefault(x => x.Children,
                                                                          x => x.InternalId == strImprovedName)
                                  ??
                                  objCharacter.Vehicles.FindVehicleWeapon(strImprovedName, out _, out _, out _);
                            if (objWeapon != null)
                                objWeapon.Equipped = false;
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
                                HashSet<Skill> setSkillsToDisable
                                    = new HashSet<Skill>(await objCharacter.SkillsSection.GetActiveSkillsFromDataAsync(
                                                                               eFilterOption, false,
                                                                               objImprovement.Target, token)
                                                                           .ConfigureAwait(false));
                                foreach (Improvement objLoopImprovement in await
                                             GetCachedImprovementListForValueOfAsync(
                                                     objCharacter, Improvement.ImprovementType.SpecialSkills,
                                                     token: token)
                                                 .ConfigureAwait(false))
                                {
                                    if (objLoopImprovement == objImprovement)
                                        continue;
                                    eFilterOption
                                        = (SkillsSection.FilterOption) Enum.Parse(
                                            typeof(SkillsSection.FilterOption), objLoopImprovement.ImprovedName);
                                    setSkillsToDisable.ExceptWith(
                                        await objCharacter.SkillsSection.GetActiveSkillsFromDataAsync(
                                                              eFilterOption, false, objLoopImprovement.Target, token)
                                                          .ConfigureAwait(false));
                                    if (setSkillsToDisable.Count == 0)
                                        return;
                                }

                                foreach (Skill objSkill in setSkillsToDisable)
                                {
                                    objSkill.ForceDisabled = true;
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
                            if (objCyberware != null)
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

                objImprovementList.ProcessRelevantEvents(token);
            }
            finally
            {
                objLocker?.Dispose();
                if (objAsyncLocker != null)
                    await objAsyncLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Remove all of the Improvements for an XML Node.
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
                // A List of Improvements to hold all of the items that will eventually be deleted.
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
                    string strSourceNameSpacedInvariant = strSourceName + ' ';
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
        /// Remove all of the Improvements for an XML Node.
        /// </summary>
        /// <param name="objCharacter">Character from which improvements should be deleted.</param>
        /// <param name="objImprovementSource">Type of object that granted these Improvements.</param>
        /// <param name="strSourceName">Name of the item that granted these Improvements.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async ValueTask<decimal> RemoveImprovementsAsync(Character objCharacter,
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
            using (await objCharacter.LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                // A List of Improvements to hold all of the items that will eventually be deleted.
                objImprovementList = (string.IsNullOrEmpty(strSourceName)
                    ? objCharacter.Improvements.Where(objImprovement =>
                                                          objImprovement.ImproveSource == objImprovementSource)
                    : objCharacter.Improvements.Where(objImprovement =>
                                                          objImprovement.ImproveSource == objImprovementSource
                                                          && objImprovement.SourceName == strSourceName)).ToList();

                // Compatibility fix for when blnConcatSelectedValue was around
                if (strSourceName.IsGuid())
                {
                    string strSourceNameSpaced = strSourceName + await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);
                    string strSourceNameSpacedInvariant = strSourceName + ' ';
                    objImprovementList.AddRange(objCharacter.Improvements.Where(
                                                    objImprovement =>
                                                        objImprovement.ImproveSource == objImprovementSource &&
                                                        (objImprovement.SourceName.StartsWith(
                                                             strSourceNameSpaced, StringComparison.Ordinal)
                                                         || objImprovement.SourceName.StartsWith(
                                                             strSourceNameSpacedInvariant, StringComparison.Ordinal))));
                }
            }

            return await RemoveImprovementsAsync(objCharacter, objImprovementList, token: token).ConfigureAwait(false);
        }

        /// <summary>
        /// Remove all of the Improvements for an XML Node.
        /// </summary>
        /// <param name="objCharacter">Character from which improvements should be deleted.</param>
        /// <param name="objImprovementList">List of improvements to delete.</param>
        /// <param name="blnReapplyImprovements">Whether we're reapplying Improvements.</param>
        /// <param name="blnAllowDuplicatesFromSameSource">If we ignore checking whether a potential duplicate improvement has the same SourceName</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static decimal RemoveImprovements(Character objCharacter, ICollection<Improvement> objImprovementList,
                                                 bool blnReapplyImprovements = false,
                                                 bool blnAllowDuplicatesFromSameSource = false, CancellationToken token = default)
        {
            return Utils.SafelyRunSynchronously(() => RemoveImprovementsCoreAsync(false, objCharacter, objImprovementList, blnReapplyImprovements,
                                                    blnAllowDuplicatesFromSameSource, token), token);
        }

        /// <summary>
        /// Remove all of the Improvements for an XML Node.
        /// </summary>
        /// <param name="objCharacter">Character from which improvements should be deleted.</param>
        /// <param name="objImprovementList">List of improvements to delete.</param>
        /// <param name="blnReapplyImprovements">Whether we're reapplying Improvements.</param>
        /// <param name="blnAllowDuplicatesFromSameSource">If we ignore checking whether a potential duplicate improvement has the same SourceName</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static Task<decimal> RemoveImprovementsAsync(Character objCharacter, ICollection<Improvement> objImprovementList,
                                                      bool blnReapplyImprovements = false,
                                                      bool blnAllowDuplicatesFromSameSource = false, CancellationToken token = default)
        {
            return RemoveImprovementsCoreAsync(false, objCharacter, objImprovementList, blnReapplyImprovements,
                                               blnAllowDuplicatesFromSameSource, token);
        }

        /// <summary>
        /// Remove all of the Improvements for an XML Node.
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
                                                                       ICollection<Improvement> objImprovementList,
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

                // Now that we have all of the applicable Improvements, remove them from the character.
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
                                                                                   || eImprovementType
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
                                                                                   || eImprovementType
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
                                                                               || eImprovementType
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
                                                                         .AsTask(), token).ConfigureAwait(false);
                                }
                            }

                            break;

                        case Improvement.ImprovementType.Activesoft:
                            if (!blnHasDuplicate && !blnReapplyImprovements)
                            {
                                if (blnSync)
                                {
                                    // ReSharper disable once MethodHasAsyncOverload
                                    Skill objSkill = objCharacter.SkillsSection.GetActiveSkill(strImprovedName, token: token);
                                    if (objSkill?.IsExoticSkill == true)
                                    {
                                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                        objCharacter.SkillsSection.Skills.Remove(objSkill);
                                    }
                                }
                                else
                                {
                                    Skill objSkill = await objCharacter.SkillsSection.GetActiveSkillAsync(strImprovedName, token).ConfigureAwait(false);
                                    if (objSkill?.IsExoticSkill == true)
                                    {
                                        await objCharacter.SkillsSection.Skills.RemoveAsync(objSkill, token).ConfigureAwait(false);
                                    }
                                }
                            }

                            break;

                        case Improvement.ImprovementType.Skillsoft:
                            if (!blnHasDuplicate && !blnReapplyImprovements)
                            {
                                objCharacter.SkillsSection.KnowledgeSkills.RemoveAll(
                                    x => x.InternalId == strImprovedName, token: token);
                                if (blnSync)
                                {
                                    for (int i = objCharacter.SkillsSection.KnowsoftSkills.Count - 1; i >= 0; --i)
                                    {
                                        KnowledgeSkill objSkill = objCharacter.SkillsSection.KnowsoftSkills[i];
                                        if (objSkill.InternalId == strImprovedName)
                                        {
                                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                            objCharacter.SkillsSection.KnowledgeSkills.Remove(objSkill);
                                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                            objCharacter.SkillsSection.KnowsoftSkills.RemoveAt(i);
                                        }
                                    }
                                }
                                else
                                {
                                    for (int i = await objCharacter.SkillsSection.KnowsoftSkills.GetCountAsync(token)
                                                                   .ConfigureAwait(false) - 1;
                                         i >= 0;
                                         --i)
                                    {
                                        KnowledgeSkill objSkill = await objCharacter.SkillsSection.KnowsoftSkills
                                            .GetValueAtAsync(i, token).ConfigureAwait(false);
                                        if (objSkill.InternalId == strImprovedName)
                                        {
                                            await Task.WhenAll(
                                                objCharacter.SkillsSection.KnowledgeSkills.RemoveAsync(objSkill, token)
                                                            .AsTask(),
                                                objCharacter.SkillsSection.KnowsoftSkills.RemoveAtAsync(i, token)
                                                            .AsTask()).ConfigureAwait(false);
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
                                    // ReSharper disable once MethodHasAsyncOverload
                                    Skill objSkill = objCharacter.SkillsSection.GetActiveSkill(strImprovedName, token: token);
                                    if (objSkill == null)
                                    {
                                        objCharacter.SkillsSection.KnowledgeSkills.RemoveAll(
                                            x => x.InternalId == strImprovedName, token: token);
                                        for (int i = objCharacter.SkillsSection.KnowsoftSkills.Count - 1; i >= 0; --i)
                                        {
                                            KnowledgeSkill objKnoSkill = objCharacter.SkillsSection.KnowsoftSkills[i];
                                            if (objKnoSkill.InternalId == strImprovedName)
                                            {
                                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                objCharacter.SkillsSection.KnowledgeSkills.Remove(objKnoSkill);
                                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                objCharacter.SkillsSection.KnowsoftSkills.RemoveAt(i);
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
                                    Skill objSkill = await objCharacter.SkillsSection.GetActiveSkillAsync(strImprovedName, token).ConfigureAwait(false);
                                    if (objSkill == null)
                                    {
                                        objCharacter.SkillsSection.KnowledgeSkills.RemoveAll(
                                            x => x.InternalId == strImprovedName, token: token);
                                        for (int i = await objCharacter.SkillsSection.KnowsoftSkills.GetCountAsync(token)
                                                                       .ConfigureAwait(false) - 1;
                                             i >= 0;
                                             --i)
                                        {
                                            KnowledgeSkill objKnoSkill = await objCharacter.SkillsSection.KnowsoftSkills
                                                .GetValueAtAsync(i, token).ConfigureAwait(false);
                                            if (objKnoSkill.InternalId == strImprovedName)
                                            {
                                                await Task.WhenAll(
                                                    objCharacter.SkillsSection.KnowledgeSkills.RemoveAsync(objKnoSkill, token)
                                                                .AsTask(),
                                                    objCharacter.SkillsSection.KnowsoftSkills.RemoveAtAsync(i, token)
                                                                .AsTask()).ConfigureAwait(false);
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
                            if (strUniqueName == "enableattribute" && !blnHasDuplicate
                                                                   && !blnReapplyImprovements)
                            {
                                switch (strImprovedName)
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
                                switch (strUniqueName)
                                {
                                    case "enabletab":
                                        switch (strImprovedName)
                                        {
                                            case "Magician":
                                                objCharacter.MagicianEnabled = false;
                                                break;

                                            case "Adept":
                                                objCharacter.AdeptEnabled = false;
                                                break;

                                            case "Technomancer":
                                                objCharacter.TechnomancerEnabled = false;
                                                break;

                                            case "Advanced Programs":
                                                objCharacter.AdvancedProgramsEnabled = false;
                                                break;

                                            case "Critter":
                                                objCharacter.CritterEnabled = false;
                                                break;
                                        }

                                        break;
                                    // Determine if access to any special tabs has been regained
                                    case "disabletab":
                                        switch (strImprovedName)
                                        {
                                            case "Cyberware":
                                                objCharacter.CyberwareDisabled = false;
                                                break;

                                            case "Initiation":
                                                objCharacter.InitiationForceDisabled = false;
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
                                    objCharacter.PrototypeTranshuman = 0;
                            }
                            else
                            {
                                objCharacter.PrototypeTranshuman
                                    -= Convert.ToDecimal(strImprovedName, GlobalSettings.InvariantCultureInfo);
                            }

                            break;

                        case Improvement.ImprovementType.Adapsin:
                        {
                            if (!blnHasDuplicate && !blnReapplyImprovements)
                            {
                                foreach (Cyberware objCyberware in blnSync
                                             ? objCharacter.Cyberware.DeepWhere(
                                                 x => x.Children, x => x.Grade.Adapsin, token)
                                             : await objCharacter.Cyberware.DeepWhereAsync(
                                                 x => x.Children, x => x.Grade.Adapsin, token).ConfigureAwait(false))
                                {
                                    string strNewName = objCyberware.Grade.Name.FastEscapeOnceFromEnd("(Adapsin)")
                                                                    .Trim();
                                    // Determine which GradeList to use for the Cyberware.
                                    objCyberware.Grade = objCharacter.GetGrades(objCyberware.SourceType, true, token)
                                                                     .FirstOrDefault(x => x.Name == strNewName);
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
                                = objCharacter.Weapons.DeepFirstOrDefault(x => x.Children,
                                                                          x => x.InternalId == strImprovedName)
                                  ??
                                  objCharacter.Vehicles.FindVehicleWeapon(strImprovedName);
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
                                    decReturn += await objMartialArt.DeleteMartialArtAsync(token).ConfigureAwait(false);
                            }

                            break;

                        case Improvement.ImprovementType.SpecialSkills:
                            if (!blnHasDuplicate)
                            {
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverload
                                    objCharacter.SkillsSection.RemoveSkills(
                                        (SkillsSection.FilterOption) Enum.Parse(typeof(SkillsSection.FilterOption),
                                            strImprovedName), objImprovement.Target,
                                        !blnReapplyImprovements && objCharacter.Created, token: token);
                                else
                                    await objCharacter.SkillsSection.RemoveSkillsAsync(
                                        (SkillsSection.FilterOption) Enum.Parse(typeof(SkillsSection.FilterOption),
                                            strImprovedName), objImprovement.Target,
                                        !blnReapplyImprovements && objCharacter.Created, token).ConfigureAwait(false);
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
                                            x => x.Name == strUniqueName, token).ConfigureAwait(false);
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
                                    if (objImprovedPower.TotalRating <= 0)
                                    {
                                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                        objImprovedPower.DeletePower();
                                    }
                                }
                                else if (await objImprovedPower.GetTotalRatingAsync(token).ConfigureAwait(false) <= 0)
                                {
                                    await objImprovedPower.DeletePowerAsync(token).ConfigureAwait(false);
                                }

                                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                                if (objImprovement.ImproveType == Improvement.ImprovementType.AdeptPowerFreeLevels)
                                    objImprovedPower.OnMultiplePropertyChanged(
                                        nameof(objImprovedPower.TotalRating), nameof(objImprovedPower.FreeLevels));
                                else
                                    objImprovedPower.OnMultiplePropertyChanged(
                                        nameof(objImprovedPower.TotalRating), nameof(objImprovedPower.FreePoints));
                            }

                            break;

                        case Improvement.ImprovementType.FreeWare:
                        {
                            Cyberware objCyberware = blnSync
                                ? objCharacter.Cyberware.FirstOrDefault(o => o.InternalId == strImprovedName)
                                : await objCharacter.Cyberware
                                                    .FirstOrDefaultAsync(o => o.InternalId == strImprovedName, token)
                                                    .ConfigureAwait(false);
                            if (objCyberware != null)
                            {
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    decReturn += objCyberware.TotalCost + objCyberware.DeleteCyberware();
                                else
                                    decReturn += await objCyberware.GetTotalCostAsync(token).ConfigureAwait(false) + await objCyberware
                                        .DeleteCyberwareAsync(token: token).ConfigureAwait(false);
                            }
                        }
                            break;
                    }
                }

                objImprovementList.ProcessRelevantEvents(token);
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
        /// <param name="blnAddToRating">Whether or not the bonus applies to a Skill's Rating instead of the dice pool in general.</param>
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
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdTrace))
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
        /// <param name="blnAddToRating">Whether or not the bonus applies to a Skill's Rating instead of the dice pool in general.</param>
        /// <param name="strTarget">What target the Improvement has, if any (e.g. a target skill whose attribute to replace).</param>
        /// <param name="strCondition">Condition for when the bonus is applied.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static async ValueTask<Improvement> CreateImprovementAsync(Character objCharacter, string strImprovedName,
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
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdTrace))
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
        /// Clear all of the Improvements from the Transaction List.
        /// </summary>
        public static void Commit(Character objCharacter)
        {
            Log.Debug("Commit");
            // Clear all of the Improvements from the Transaction List.
            if (s_DictionaryTransactions.TryRemove(objCharacter, out List<Improvement> lstTransactions))
            {
                lstTransactions.ProcessRelevantEvents();
            }

            Log.Debug("Commit exit");
        }

        /// <summary>
        /// Rollback all of the Improvements from the Transaction List.
        /// </summary>
        public static void Rollback(Character objCharacter, CancellationToken token = default)
        {
            Log.Debug("Rollback enter");
            if (s_DictionaryTransactions.TryRemove(objCharacter, out List<Improvement> lstTransactions))
            {
                using (objCharacter.LockObject.EnterWriteLock(token))
                {
                    // Remove all of the Improvements that were added.
                    foreach (Improvement objTransactingImprovement in lstTransactions)
                    {
                        RemoveImprovements(objCharacter, objTransactingImprovement.ImproveSource,
                                           objTransactingImprovement.SourceName, token);
                        ClearCachedValue(objCharacter, objTransactingImprovement.ImproveType,
                                         objTransactingImprovement.ImprovedName, token);
                    }
                }

                lstTransactions.Clear();
            }

            Log.Debug("Rollback exit");
        }

        /// <summary>
        /// Rollback all of the Improvements from the Transaction List.
        /// </summary>
        public static async ValueTask RollbackAsync(Character objCharacter, CancellationToken token = default)
        {
            Log.Debug("Rollback enter");
            if (s_DictionaryTransactions.TryRemove(objCharacter, out List<Improvement> lstTransactions))
            {
                IAsyncDisposable objLocker = await objCharacter.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    // Remove all of the Improvements that were added.
                    foreach (Improvement objTransactingImprovement in lstTransactions)
                    {
                        await RemoveImprovementsAsync(objCharacter, objTransactingImprovement.ImproveSource,
                                                      objTransactingImprovement.SourceName, token).ConfigureAwait(false);
                        ClearCachedValue(objCharacter, objTransactingImprovement.ImproveType,
                            objTransactingImprovement.ImprovedName, token);
                    }
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }

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
        public static void ProcessRelevantEvents(this Improvement objImprovement, ICollection<string> lstExtraImprovedName = null, IEnumerable<Improvement.ImprovementType> lstExtraImprovementTypes = null, ICollection<string> lstExtraUniqueName = null, ICollection<string> lstExtraTarget = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objImprovement?.SetupComplete != true)
                return;
            // Create a hashset of events to fire to make sure we only ever fire each event once
            using (new FetchSafelyFromPool<Dictionary<INotifyMultiplePropertyChanged, HashSet<string>>>(
                       Utils.DictionaryForMultiplePropertyChangedPool,
                       out Dictionary<INotifyMultiplePropertyChanged, HashSet<string>> dicChangedProperties))
            {
                try
                {
                    foreach ((INotifyMultiplePropertyChanged objToNotify, string strProperty) in objImprovement
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
                            foreach ((INotifyMultiplePropertyChanged objToNotify, string strProperty) in objImprovement
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
                    foreach (KeyValuePair<INotifyMultiplePropertyChanged, HashSet<string>> kvpChangedProperties in
                             dicChangedProperties)
                    {
                        token.ThrowIfCancellationRequested();
                        kvpChangedProperties.Key.OnMultiplePropertyChanged(kvpChangedProperties.Value.ToList());
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
            using (new FetchSafelyFromPool<Dictionary<INotifyMultiplePropertyChanged, HashSet<string>>>(
                       Utils.DictionaryForMultiplePropertyChangedPool,
                       out Dictionary<INotifyMultiplePropertyChanged, HashSet<string>> dicChangedProperties))
            {
                try
                {
                    token.ThrowIfCancellationRequested();
                    foreach (Improvement objImprovement in lstImprovements.Where(x => x.SetupComplete))
                    {
                        token.ThrowIfCancellationRequested();
                        foreach ((INotifyMultiplePropertyChanged objToNotify, string strProperty) in objImprovement
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
                    foreach (KeyValuePair<INotifyMultiplePropertyChanged, HashSet<string>> kvpChangedProperties in
                             dicChangedProperties)
                    {
                        token.ThrowIfCancellationRequested();
                        kvpChangedProperties.Key.OnMultiplePropertyChanged(kvpChangedProperties.Value.ToList());
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
    }
}
