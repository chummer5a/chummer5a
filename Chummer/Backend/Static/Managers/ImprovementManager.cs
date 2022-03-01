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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace Chummer
{
    public static class ImprovementManager
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();

        // String that will be used to limit the selection in Pick forms.
        private static string _strLimitSelection = string.Empty;

        private static string _strSelectedValue = string.Empty;
        private static string _strForcedValue = string.Empty;

        private static readonly LockingDictionary<Character, List<Improvement>> s_DictionaryTransactions
            = new LockingDictionary<Character, List<Improvement>>(10);

        private static readonly LockingHashSet<Tuple<ImprovementDictionaryKey, IDictionary>>
            s_SetCurrentlyCalculatingValues = new LockingHashSet<Tuple<ImprovementDictionaryKey, IDictionary>>();

        private static readonly LockingDictionary<ImprovementDictionaryKey, Tuple<decimal, List<Improvement>>>
            s_DictionaryCachedValues
                = new LockingDictionary<ImprovementDictionaryKey, Tuple<decimal, List<Improvement>>>(
                    (int) Improvement.ImprovementType.NumImprovementTypes);

        private static readonly LockingDictionary<ImprovementDictionaryKey, Tuple<decimal, List<Improvement>>>
            s_DictionaryCachedAugmentedValues
                = new LockingDictionary<ImprovementDictionaryKey, Tuple<decimal, List<Improvement>>>(
                    (int) Improvement.ImprovementType.NumImprovementTypes);

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
                                            string strImprovementName = "")
        {
            if (!string.IsNullOrEmpty(strImprovementName))
            {
                ImprovementDictionaryKey objCheckKey
                    = new ImprovementDictionaryKey(objCharacter, eImprovementType, strImprovementName);
                if (!s_DictionaryCachedValues.TryAdd(objCheckKey,
                                                     new Tuple<decimal, List<Improvement>>(
                                                         decimal.MinValue, new List<Improvement>())))
                {
                    List<Improvement> lstTemp = s_DictionaryCachedValues[objCheckKey].Item2;
                    lstTemp.Clear();
                    s_DictionaryCachedValues[objCheckKey]
                        = new Tuple<decimal, List<Improvement>>(decimal.MinValue, lstTemp);
                }

                if (!s_DictionaryCachedAugmentedValues.TryAdd(objCheckKey,
                                                              new Tuple<decimal, List<Improvement>>(
                                                                  decimal.MinValue, new List<Improvement>())))
                {
                    List<Improvement> lstTemp = s_DictionaryCachedAugmentedValues[objCheckKey].Item2;
                    lstTemp.Clear();
                    s_DictionaryCachedAugmentedValues[objCheckKey]
                        = new Tuple<decimal, List<Improvement>>(decimal.MinValue, lstTemp);
                }
            }
            else
            {
                foreach (ImprovementDictionaryKey objCheckKey in s_DictionaryCachedValues.Keys
                             .Where(x => x.CharacterObject == objCharacter && x.ImprovementType == eImprovementType)
                             .ToList())
                {
                    List<Improvement> lstTemp = s_DictionaryCachedValues[objCheckKey].Item2;
                    lstTemp.Clear();
                    s_DictionaryCachedValues[objCheckKey]
                        = new Tuple<decimal, List<Improvement>>(decimal.MinValue, lstTemp);
                }

                foreach (ImprovementDictionaryKey objCheckKey in s_DictionaryCachedAugmentedValues.Keys
                             .Where(x => x.CharacterObject == objCharacter && x.ImprovementType == eImprovementType)
                             .ToList())
                {
                    List<Improvement> lstTemp = s_DictionaryCachedAugmentedValues[objCheckKey].Item2;
                    lstTemp.Clear();
                    s_DictionaryCachedAugmentedValues[objCheckKey]
                        = new Tuple<decimal, List<Improvement>>(decimal.MinValue, lstTemp);
                }
            }
        }

        public static void ClearCachedValues(Character objCharacter)
        {
            foreach (ImprovementDictionaryKey objKey in s_DictionaryCachedValues.Keys.Where(x => x.CharacterObject == objCharacter).ToList())
            {
                if (s_DictionaryCachedValues.TryRemove(objKey, out Tuple<decimal, List<Improvement>> tupTemp))
                    tupTemp.Item2.Clear(); // Just in case this helps the GC
            }

            foreach (ImprovementDictionaryKey objKey in s_DictionaryCachedAugmentedValues.Keys.Where(x => x.CharacterObject == objCharacter).ToList())
            {
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
        public static decimal ValueOf(Character objCharacter, Improvement.ImprovementType objImprovementType,
                                      bool blnAddToRating = false, string strImprovedName = "",
                                      bool blnUnconditionalOnly = true, bool blnIncludeNonImproved = false)
        {
            return ValueOf(objCharacter, objImprovementType, out List<Improvement> _, blnAddToRating,
                           strImprovedName, blnUnconditionalOnly, blnIncludeNonImproved);
        }

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
        public static decimal ValueOf(Character objCharacter, Improvement.ImprovementType objImprovementType,
                                      out List<Improvement> lstUsedImprovements,
                                      bool blnAddToRating = false, string strImprovedName = "",
                                      bool blnUnconditionalOnly = true, bool blnIncludeNonImproved = false)
        {
            decimal decReturn = MetaValueOf(objCharacter, objImprovementType, out lstUsedImprovements,
                                            x => x.Value,
                                            s_DictionaryCachedValues, blnAddToRating, strImprovedName,
                                            blnUnconditionalOnly,
                                            blnIncludeNonImproved);
            if (decReturn != 0 && lstUsedImprovements.Count == 0)
            {
                Log.Warn("A cached value modifier somehow is not zero while having no used improvements in its list.");
                Utils.BreakIfDebug();
            }
            return decReturn;
        }

        /// <summary>
        /// Gets the cached list of active improvements that contribute to values of a given improvement type.
        /// </summary>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="eImprovementType">ImprovementType for which active improvements should be fetched.</param>
        /// <param name="strImprovedName">Improvements are only fetched with the given improvedname. If empty, only those with an empty ImprovedName are fetched.</param>
        /// <param name="blnIncludeNonImproved">Whether to only fetch values for improvements that do not have an improvedname when specifying <paramref name="strImprovedName"/>.</param>
        /// <returns>A cached list of all unconditional improvements that do not add to ratings and that match the conditions set by the arguments.</returns>
        public static List<Improvement> GetCachedImprovementListForValueOf(
            Character objCharacter, Improvement.ImprovementType eImprovementType, string strImprovedName = "",
            bool blnIncludeNonImproved = false)
        {
            ValueOf(objCharacter, eImprovementType, out List<Improvement> lstReturn, strImprovedName: strImprovedName,
                    blnIncludeNonImproved: blnIncludeNonImproved);
            return lstReturn;
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
        public static decimal AugmentedValueOf(Character objCharacter, Improvement.ImprovementType objImprovementType,
                                               bool blnAddToRating = false, string strImprovedName = "",
                                               bool blnUnconditionalOnly = true, bool blnIncludeNonImproved = false)
        {
            return AugmentedValueOf(objCharacter, objImprovementType, out List<Improvement> _, blnAddToRating,
                                    strImprovedName, blnUnconditionalOnly, blnIncludeNonImproved);
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
        public static decimal AugmentedValueOf(Character objCharacter, Improvement.ImprovementType objImprovementType,
                                               out List<Improvement> lstUsedImprovements, bool blnAddToRating = false,
                                               string strImprovedName = "",
                                               bool blnUnconditionalOnly = true, bool blnIncludeNonImproved = false)
        {
            decimal decReturn = MetaValueOf(objCharacter, objImprovementType, out lstUsedImprovements,
                                           x => x.Augmented * x.Rating,
                                           s_DictionaryCachedAugmentedValues, blnAddToRating, strImprovedName,
                                           blnUnconditionalOnly,
                                           blnIncludeNonImproved);
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
        /// <returns>A cached list of all unconditional improvements that do not add to ratings and that match the conditions set by the arguments.</returns>
        public static List<Improvement> GetCachedImprovementListForAugmentedValueOf(
            Character objCharacter, Improvement.ImprovementType eImprovementType, string strImprovedName = "",
            bool blnIncludeNonImproved = false)
        {
            AugmentedValueOf(objCharacter, eImprovementType, out List<Improvement> lstReturn,
                             strImprovedName: strImprovedName, blnIncludeNonImproved: blnIncludeNonImproved);
            return lstReturn;
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
        /// <param name="lstUsedImprovements">List of the improvements actually used for the value</param>
        /// <param name="funcValueGetter">Function for how to extract values for individual improvements.</param>
        private static decimal MetaValueOf(Character objCharacter, Improvement.ImprovementType eImprovementType,
                                           out List<Improvement> lstUsedImprovements,
                                           Func<Improvement, decimal> funcValueGetter,
                                           LockingDictionary<ImprovementDictionaryKey,
                                               Tuple<decimal, List<Improvement>>> dicCachedValuesToUse,
                                           bool blnAddToRating, string strImprovedName,
                                           bool blnUnconditionalOnly, bool blnIncludeNonImproved)
        {
            //Log.Info("objImprovementType = " + objImprovementType.ToString());
            //Log.Info("blnAddToRating = " + blnAddToRating.ToString());
            //Log.Info("strImprovedName = " + ("" + strImprovedName).ToString());

            if (funcValueGetter == null)
                throw new ArgumentNullException(nameof(funcValueGetter));

            if (objCharacter == null)
            {
                lstUsedImprovements = new List<Improvement>();
                return 0;
            }

            if (string.IsNullOrWhiteSpace(strImprovedName))
                strImprovedName = string.Empty;

            using (EnterReadLock.Enter(objCharacter.LockObject))
            {
                // These values are needed to prevent race conditions that could cause Chummer to crash
                Tuple<ImprovementDictionaryKey, IDictionary> tupMyValueToCheck
                    = new Tuple<ImprovementDictionaryKey, IDictionary>(
                        new ImprovementDictionaryKey(objCharacter, eImprovementType, strImprovedName),
                        dicCachedValuesToUse);
                Tuple<ImprovementDictionaryKey, IDictionary> tupBlankValueToCheck
                    = new Tuple<ImprovementDictionaryKey, IDictionary>(
                        new ImprovementDictionaryKey(objCharacter, eImprovementType, string.Empty),
                        dicCachedValuesToUse);

                // Only cache "default" ValueOf calls, otherwise there will be way too many values to cache
                bool blnFetchAndCacheResults = !blnAddToRating && blnUnconditionalOnly;

                // If we've got a value cached for the default ValueOf call for an improvementType, let's just return that
                if (blnFetchAndCacheResults)
                {
                    if (dicCachedValuesToUse != null)
                    {
                        // First check to make sure an existing caching for this particular value is not already running. If one is, wait for it to finish before continuing
                        int intEmergencyRelease = 0;
                        for (;
                             !s_SetCurrentlyCalculatingValues.TryAdd(tupMyValueToCheck)
                             && intEmergencyRelease <= Utils.SleepEmergencyReleaseMaxTicks;
                             ++intEmergencyRelease)
                        {
                            Utils.SafeSleep();
                        }

                        // Emergency exit, so break if we are debugging and return the default value (just in case)
                        if (intEmergencyRelease > Utils.SleepEmergencyReleaseMaxTicks)
                        {
                            Utils.BreakIfDebug();
                            lstUsedImprovements = new List<Improvement>();
                            return 0;
                        }

                        // Also make sure we block off the conditionless check because we will be adding cached keys that will be used by the conditionless check
                        if (!string.IsNullOrWhiteSpace(strImprovedName) && !blnIncludeNonImproved)
                        {
                            intEmergencyRelease = 0;
                            for (;
                                 !s_SetCurrentlyCalculatingValues.TryAdd(tupBlankValueToCheck)
                                 && intEmergencyRelease <= Utils.SleepEmergencyReleaseMaxTicks;
                                 ++intEmergencyRelease)
                            {
                                Utils.SafeSleep();
                            }

                            // Emergency exit, so break if we are debugging and return the default value (just in case)
                            if (intEmergencyRelease > Utils.SleepEmergencyReleaseMaxTicks)
                            {
                                Utils.BreakIfDebug();
                                lstUsedImprovements = new List<Improvement>();
                                s_SetCurrentlyCalculatingValues.Remove(tupMyValueToCheck);
                                return 0;
                            }

                            ImprovementDictionaryKey objCacheKey
                                = new ImprovementDictionaryKey(objCharacter, eImprovementType, strImprovedName);
                            if (dicCachedValuesToUse.TryGetValue(objCacheKey,
                                                                 out Tuple<decimal, List<Improvement>> tupCachedValue)
                                && tupCachedValue.Item1 != decimal.MinValue)
                            {
                                s_SetCurrentlyCalculatingValues.Remove(tupMyValueToCheck);
                                s_SetCurrentlyCalculatingValues.Remove(tupBlankValueToCheck);
                                lstUsedImprovements
                                    = tupCachedValue.Item2
                                                    .ToList(); // To make sure we do not inadvertently alter the cached list
                                return tupCachedValue.Item1;
                            }

                            lstUsedImprovements = new List<Improvement>();
                        }
                        else
                        {
                            lstUsedImprovements = new List<Improvement>();
                            bool blnDoRecalculate = true;
                            decimal decCachedValue = 0;
                            // Only fetch based on cached values if the dictionary contains at least one element with matching characters and types and none of those elements have a "reset" value of decimal.MinValue
                            foreach (KeyValuePair<ImprovementDictionaryKey, Tuple<decimal, List<Improvement>>>
                                         objLoopCachedEntry in dicCachedValuesToUse)
                            {
                                ImprovementDictionaryKey objLoopKey = objLoopCachedEntry.Key;
                                if (objLoopKey.CharacterObject != objCharacter ||
                                    objLoopKey.ImprovementType != eImprovementType)
                                    continue;
                                if (!string.IsNullOrWhiteSpace(strImprovedName)
                                    && !string.IsNullOrWhiteSpace(objLoopKey.ImprovementName)
                                    && strImprovedName != objLoopKey.ImprovementName)
                                    continue;
                                blnDoRecalculate = false;
                                decimal decLoopCachedValue = objLoopCachedEntry.Value.Item1;
                                if (decLoopCachedValue == decimal.MinValue)
                                {
                                    blnDoRecalculate = true;
                                    break;
                                }

                                decCachedValue += decLoopCachedValue;
                                lstUsedImprovements.AddRange(objLoopCachedEntry.Value.Item2);
                            }

                            if (!blnDoRecalculate)
                            {
                                s_SetCurrentlyCalculatingValues.Remove(tupMyValueToCheck);
                                return decCachedValue;
                            }

                            lstUsedImprovements.Clear();
                        }
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
                                    if (!setUniqueNames.Contains(strUniqueName))
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
                        foreach (HashSet<string> setToReturn in dicUniqueNames.Values)
                            Utils.StringHashSetPool.Return(setToReturn);
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
                                    if (!setUniqueNames.Contains(strUniqueName))
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
                                        lstLoopImprovements.Add(objHighestImprovement);
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
                        foreach (HashSet<string> setToReturn in dicUniqueNames.Values)
                            Utils.StringHashSetPool.Return(setToReturn);
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
                                if (!dicCachedValuesToUse.TryAdd(objLoopCacheKey, tupNewValue))
                                {
                                    List<Improvement> lstTemp = dicCachedValuesToUse[objLoopCacheKey].Item2;
                                    if (!ReferenceEquals(lstTemp, tupNewValue.Item2))
                                    {
                                        lstTemp.Clear();
                                        lstTemp.AddRange(tupNewValue.Item2);
                                        tupNewValue = new Tuple<decimal, List<Improvement>>(decLoopValue, lstTemp);
                                    }

                                    dicCachedValuesToUse[objLoopCacheKey] = tupNewValue;
                                }
                            }

                            lstUsedImprovements.AddRange(tupNewValue.Item2);
                        }
                        else
                            lstUsedImprovements.AddRange(dicImprovementsForValues[strLoopImprovedName]);

                        decReturn += decLoopValue;
                    }

                    return decReturn;
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

            if (strValue.Contains("Rating") || AttributeSection.AttributeStrings.Any(strValue.Contains))
            {
                string strReturn = strValue.Replace("Rating", intRating.ToString(GlobalSettings.InvariantCultureInfo));
                // If the value contain an CharacterAttribute name, replace it with the character's CharacterAttribute.
                strReturn = objCharacter.AttributeSection.ProcessAttributesInXPath(strReturn);
                strReturn = strReturn.Replace("/", " div ");

                //Log.Info("strValue = " + strValue);
                //Log.Info("strReturn = " + strReturn);

                // Treat this as a decimal value so any fractions can be rounded down. This is currently only used by the Boosted Reflexes Cyberware from SR2050.
                object objProcess = CommonFunctions.EvaluateInvariantXPath(strReturn, out bool blnIsSuccess);
                int intValue = blnIsSuccess ? ((double) objProcess).StandardRound() : 0;

                //Log.Exit("ValueToInt");
                return intValue;
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

            if (strValue.Contains("Rating") || AttributeSection.AttributeStrings.Any(strValue.Contains))
            {
                string strReturn = strValue.Replace("Rating", intRating.ToString(GlobalSettings.InvariantCultureInfo));
                // If the value contain an CharacterAttribute name, replace it with the character's CharacterAttribute.
                strReturn = objCharacter.AttributeSection.ProcessAttributesInXPath(strReturn);
                strReturn = strReturn.Replace("/", " div ");

                //Log.Info("strValue = " + strValue);
                //Log.Info("strReturn = " + strReturn);

                // Treat this as a decimal value so any fractions can be rounded down. This is currently only used by the Boosted Reflexes Cyberware from SR2050.
                object objProcess = CommonFunctions.EvaluateInvariantXPath(strReturn, out bool blnIsSuccess);
                decimal decValue = blnIsSuccess ? Convert.ToDecimal((double) objProcess) : 0;

                //Log.Exit("ValueToInt");
                return decValue;
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
            string strSelectedSkill = string.Empty;
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
                    string strOnlyCategory = xmlBonusNode.SelectSingleNode("@skillcategory")?.InnerText;
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
                        string strExcludeCategory = xmlBonusNode.SelectSingleNode("@excludecategory")?.InnerText;
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
                                string strLimitToSkill = xmlBonusNode.SelectSingleNode("@limittoskill")?.InnerText;
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
                                    = xmlBonusNode.SelectSingleNode("@limittoattribute")?.InnerText;
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
                                        using (EnterReadLock.Enter(objCharacter.LockObject))
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
                                                        string strName = xmlSkill.SelectSingleNode("name")?.Value;
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

                                    lstDropdownItems.Sort(CompareListItems.CompareNames);
                                    
                                    DialogResult eResult = Program.GetFormForDialog(objCharacter).DoThreadSafeFunc(x =>
                                    {
                                        using (SelectItem frmPickSkill = new SelectItem
                                               {
                                                   Description = LanguageManager.GetString("Title_SelectSkill"),
                                                   AllowAutoSelect = string.IsNullOrWhiteSpace(strPrompt)
                                               })
                                        {
                                            if (setAllowedNames != null && string.IsNullOrWhiteSpace(strPrompt))
                                                frmPickSkill.SetGeneralItemsMode(lstDropdownItems);
                                            else
                                                frmPickSkill.SetDropdownItemsMode(lstDropdownItems);

                                            frmPickSkill.ShowDialogSafe(x);

                                            if (frmPickSkill.DialogResult != DialogResult.Cancel)
                                            {
                                                strSelectedSkill = frmPickSkill.SelectedItem;
                                            }

                                            return frmPickSkill.DialogResult;
                                        }
                                    });
                                    if (eResult == DialogResult.Cancel)
                                        throw new AbortedException();
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                DialogResult eResult = Program.GetFormForDialog(objCharacter).DoThreadSafeFunc(x =>
                {
                    // Display the Select Skill window and record which Skill was selected.
                    using (SelectSkill frmPickSkill = new SelectSkill(objCharacter, strFriendlyName)
                           {
                               Description = !string.IsNullOrEmpty(strFriendlyName)
                                   ? string.Format(GlobalSettings.CultureInfo,
                                                   LanguageManager.GetString("String_Improvement_SelectSkillNamed"),
                                                   strFriendlyName)
                                   : LanguageManager.GetString("String_Improvement_SelectSkill")
                           })
                    {
                        string strMinimumRating = xmlBonusNode.Attributes?["minimumrating"]?.InnerText;
                        if (!string.IsNullOrWhiteSpace(strMinimumRating))
                            frmPickSkill.MinimumRating = ValueToInt(objCharacter, strMinimumRating, intRating);
                        string strMaximumRating = xmlBonusNode.Attributes?["maximumrating"]?.InnerText;
                        if (!string.IsNullOrWhiteSpace(strMaximumRating))
                            frmPickSkill.MaximumRating = ValueToInt(objCharacter, strMaximumRating, intRating);

                        XmlNode xmlSkillCategories = xmlBonusNode.SelectSingleNode("skillcategories");
                        if (xmlSkillCategories != null)
                            frmPickSkill.LimitToCategories = xmlSkillCategories;
                        string strTemp = xmlBonusNode.SelectSingleNode("@skillcategory")?.InnerText;
                        if (!string.IsNullOrEmpty(strTemp))
                            frmPickSkill.OnlyCategory = strTemp;
                        strTemp = xmlBonusNode.SelectSingleNode("@skillgroup")?.InnerText;
                        if (!string.IsNullOrEmpty(strTemp))
                            frmPickSkill.OnlySkillGroup = strTemp;
                        strTemp = xmlBonusNode.SelectSingleNode("@excludecategory")?.InnerText;
                        if (!string.IsNullOrEmpty(strTemp))
                            frmPickSkill.ExcludeCategory = strTemp;
                        strTemp = xmlBonusNode.SelectSingleNode("@excludeskillgroup")?.InnerText;
                        if (!string.IsNullOrEmpty(strTemp))
                            frmPickSkill.ExcludeSkillGroup = strTemp;
                        strTemp = xmlBonusNode.SelectSingleNode("@limittoskill")?.InnerText;
                        if (!string.IsNullOrEmpty(strTemp))
                            frmPickSkill.LimitToSkill = strTemp;
                        strTemp = xmlBonusNode.SelectSingleNode("@excludeskill")?.InnerText;
                        if (!string.IsNullOrEmpty(strTemp))
                            frmPickSkill.ExcludeSkill = strTemp;
                        strTemp = xmlBonusNode.SelectSingleNode("@limittoattribute")?.InnerText;
                        if (!string.IsNullOrEmpty(strTemp))
                            frmPickSkill.LinkedAttribute = strTemp;

                        if (!string.IsNullOrEmpty(ForcedValue))
                        {
                            frmPickSkill.OnlySkill = ForcedValue;
                            frmPickSkill.Opacity = 0;
                        }

                        frmPickSkill.ShowDialogSafe(x);

                        // Make sure the dialogue window was not canceled.
                        if (frmPickSkill.DialogResult != DialogResult.Cancel)
                        {
                            strSelectedSkill = frmPickSkill.SelectedSkill;
                        }

                        return frmPickSkill.DialogResult;
                    }
                });
                if (eResult == DialogResult.Cancel)
                    throw new AbortedException();
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
        ///
        /// <returns>True if successful</returns>
        public static bool CreateImprovements(Character objCharacter,
                                              Improvement.ImprovementSource objImprovementSource, string strSourceName,
                                              XmlNode nodBonus, int intRating = 1, string strFriendlyName = "",
                                              bool blnAddImprovementsToCharacter = true)
        {
            Log.Debug("CreateImprovements enter");
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdTrace))
            {
                sbdTrace.Append("objImprovementSource = ").AppendLine(objImprovementSource.ToString());
                sbdTrace.Append("strSourceName = ").AppendLine(strSourceName);
                sbdTrace.Append("nodBonus = ").AppendLine(nodBonus?.OuterXml);
                sbdTrace.Append("intRating = ").AppendLine(intRating.ToString(GlobalSettings.InvariantCultureInfo));
                sbdTrace.Append("strFriendlyName = ").AppendLine(strFriendlyName);

                try
                {
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

                            if (!string.IsNullOrEmpty(_strForcedValue))
                            {
                                LimitSelection = _strForcedValue;
                            }
                            else if (objCharacter != null)
                            {
                                using (EnterWriteLock.Enter(objCharacter.LockObject))
                                {
                                    if (objCharacter.PushText.TryTake(out string strText))
                                    {
                                        LimitSelection = strText;
                                    }
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
                                DialogResult eResult = Program.GetFormForDialog(objCharacter).DoThreadSafeFunc(x =>
                                {
                                    // Display the Select Text window and record the value that was entered.
                                    using (SelectText frmPickText = new SelectText
                                           {
                                               Description =
                                                   string.Format(GlobalSettings.CultureInfo,
                                                                 LanguageManager.GetString(
                                                                     "String_Improvement_SelectText"),
                                                                 strFriendlyName)
                                           })
                                    {
                                        frmPickText.ShowDialogSafe(x);

                                        // Make sure the dialogue window was not canceled.
                                        if (frmPickText.DialogResult != DialogResult.Cancel)
                                        {
                                            _strSelectedValue = frmPickText.SelectedValue;
                                        }

                                        return frmPickText.DialogResult;
                                    }
                                });
                                if (eResult == DialogResult.Cancel)
                                {
                                    Rollback(objCharacter);
                                    ForcedValue = string.Empty;
                                    LimitSelection = string.Empty;
                                    return false;
                                }
                            }
                            else if (objCharacter != null)
                            {
                                DialogResult eResult = Program.GetFormForDialog(objCharacter).DoThreadSafeFunc(x =>
                                {
                                    using (SelectItem frmSelect = new SelectItem
                                           {
                                               Description = string.Format(GlobalSettings.CultureInfo,
                                                                           LanguageManager.GetString(
                                                                               "String_Improvement_SelectText"),
                                                                           strFriendlyName)
                                           })
                                    {
                                        string strXPath = nodBonus.SelectSingleNode("selecttext/@xpath")?.Value;
                                        if (string.IsNullOrEmpty(strXPath))
                                        {
                                            return DialogResult.Cancel;
                                        }

                                        string strXmlFile = nodBonus.SelectSingleNode("selecttext/@xml")?.Value
                                                            ?? string.Empty;
                                        XPathNavigator xmlDoc
                                            = objCharacter.LoadDataXPath(strXmlFile);
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
                                                    string strName
                                                        = objNode.SelectSingleNodeAndCacheExpression("name")?.Value
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
                                                                    objCharacter.TranslateExtra(
                                                                        strValue, strPreferFile: strXmlFile)));
                                                    }
                                                    else if (setUsedValues.Add(strName))
                                                    {
                                                        lstItems.Add(
                                                            new ListItem(
                                                                strName,
                                                                objCharacter.TranslateExtra(
                                                                    strName, strPreferFile: strXmlFile)));
                                                    }
                                                }
                                            }

                                            if (lstItems.Count == 0)
                                            {
                                                return DialogResult.Cancel;
                                            }

                                            if (Convert.ToBoolean(
                                                    nodBonus.SelectSingleNode("selecttext/@allowedit")?.Value,
                                                    GlobalSettings.InvariantCultureInfo))
                                            {
                                                lstItems.Insert(0, ListItem.Blank);
                                                frmSelect.SetDropdownItemsMode(lstItems);
                                            }
                                            else
                                            {
                                                frmSelect.SetGeneralItemsMode(lstItems);
                                            }

                                            frmSelect.ShowDialogSafe(x);

                                            if (frmSelect.DialogResult != DialogResult.Cancel)
                                            {
                                                _strSelectedValue = frmSelect.SelectedItem;
                                            }

                                            return frmSelect.DialogResult;
                                        }
                                    }
                                });
                                if (eResult == DialogResult.Cancel)
                                {
                                    Rollback(objCharacter);
                                    ForcedValue = string.Empty;
                                    LimitSelection = string.Empty;
                                    return false;
                                }
                            }

                            sbdTrace.Append("SelectedValue = ").AppendLine(SelectedValue);
                            sbdTrace.Append("strSourceName = ").AppendLine(strSourceName);

                            // Create the Improvement.
                            sbdTrace.AppendLine("Calling CreateImprovement");

                            CreateImprovement(objCharacter, _strSelectedValue, objImprovementSource, strSourceName,
                                              Improvement.ImprovementType.Text,
                                              strUnique);
                        }

                        // If there is no character object, don't attempt to add any Improvements.
                        if (objCharacter == null && blnAddImprovementsToCharacter)
                        {
                            sbdTrace.AppendLine("_objCharacter = Null");
                            return true;
                        }

                        // Check to see what bonuses the node grants.
                        foreach (XmlNode bonusNode in nodBonus.ChildNodes)
                        {
                            if (!ProcessBonus(objCharacter, objImprovementSource, ref strSourceName, intRating,
                                              strFriendlyName,
                                              bonusNode, strUnique, !blnAddImprovementsToCharacter))
                            {
                                Rollback(objCharacter);
                                sbdTrace.AppendLine("Bonus processing unsuccessful, returning.");
                                return false;
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
                        Rollback(objCharacter);
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
                    using (EnterWriteLock.Enter(objCharacter.LockObject))
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
                Log.Warn(new object[] {"Tried to get unknown bonus", bonusNode.OuterXml});
                return false;
            }

            return true;
        }

        public static void EnableImprovements(Character objCharacter, IEnumerable<Improvement> objImprovementList)
        {
            EnableImprovements(objCharacter, objImprovementList.ToList());
        }

        public static void EnableImprovements(Character objCharacter, params Improvement[] objImprovementList)
        {
            EnableImprovements(objCharacter, Array.AsReadOnly(objImprovementList));
        }

        public static void EnableImprovements(Character objCharacter,
                                              IReadOnlyCollection<Improvement> objImprovementList)
        {
            if (objCharacter == null)
                throw new ArgumentNullException(nameof(objCharacter));
            if (objImprovementList == null)
                throw new ArgumentNullException(nameof(objImprovementList));

            using (EnterWriteLock.Enter(objCharacter.LockObject))
            {
                foreach (Improvement objImprovement in objImprovementList)
                {
                    // Enable the Improvement.
                    objImprovement.Enabled = true;
                }

                bool blnCharacterHasSkillsoftAccess
                    = ValueOf(objCharacter, Improvement.ImprovementType.SkillsoftAccess) > 0;
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
                            foreach (KnowledgeSkill objKnowledgeSkill in objCharacter.SkillsSection.KnowsoftSkills)
                            {
                                if (!objCharacter.SkillsSection.KnowledgeSkills.Contains(objKnowledgeSkill))
                                    objCharacter.SkillsSection.KnowledgeSkills.Add(objKnowledgeSkill);
                            }

                            break;

                        case Improvement.ImprovementType.Skillsoft:
                        {
                            foreach (KnowledgeSkill objKnowledgeSkill in
                                     objCharacter.SkillsSection.KnowsoftSkills.Where(
                                         x => x.InternalId == strImprovedName))
                            {
                                if (blnCharacterHasSkillsoftAccess
                                    && !objCharacter.SkillsSection.KnowledgeSkills.Contains(objKnowledgeSkill))
                                    objCharacter.SkillsSection.KnowledgeSkills.Add(objKnowledgeSkill);
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
                                        objCharacter.MAGEnabled = true;
                                        break;

                                    case "RES":
                                        objCharacter.RESEnabled = true;
                                        break;

                                    case "DEP":
                                        objCharacter.DEPEnabled = true;
                                        break;
                                }
                            }

                            break;

                        case Improvement.ImprovementType.SpecialTab:
                            switch (strUniqueName)
                            {
                                // Determine if access to any special tabs have been lost.
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

                        case Improvement.ImprovementType.Adapsin:
                            break;

                        case Improvement.ImprovementType.AddContact:
                            Contact objNewContact
                                = objCharacter.Contacts.FirstOrDefault(c => c.UniqueId == strImprovedName);
                            if (objNewContact != null)
                            {
                                // TODO: Add code to enable disabled contact
                            }

                            break;

                        case Improvement.ImprovementType.Art:
                            Art objArt = objCharacter.Arts.FirstOrDefault(x => x.InternalId == strImprovedName);
                            if (objArt != null)
                            {
                                Improvement.ImprovementSource eSource
                                    = objArt.SourceType;
                                EnableImprovements(objCharacter,
                                                   objCharacter.Improvements.Where(
                                                       x => x.ImproveSource == eSource
                                                            && x.SourceName == strImprovedName && x.Enabled));
                            }

                            break;

                        case Improvement.ImprovementType.Metamagic:
                        case Improvement.ImprovementType.Echo:
                            Metamagic objMetamagic
                                = objCharacter.Metamagics.FirstOrDefault(x => x.InternalId == strImprovedName);
                            if (objMetamagic != null)
                            {
                                Improvement.ImprovementSource eSource
                                    = objImprovement.ImproveType == Improvement.ImprovementType.Metamagic
                                        ? Improvement.ImprovementSource.Metamagic
                                        : Improvement.ImprovementSource.Echo;
                                EnableImprovements(objCharacter,
                                                   objCharacter.Improvements.Where(
                                                       x => x.ImproveSource == eSource
                                                            && x.SourceName == strImprovedName && x.Enabled));
                            }

                            break;

                        case Improvement.ImprovementType.CritterPower:
                            CritterPower objCritterPower = objCharacter.CritterPowers.FirstOrDefault(
                                x => x.InternalId == strImprovedName || (x.Name == strImprovedName
                                                                         && x.Extra == strUniqueName));
                            if (objCritterPower != null)
                            {
                                string strPowerId = objCritterPower.InternalId;
                                EnableImprovements(objCharacter,
                                                   objCharacter.Improvements.Where(
                                                       x => x.ImproveSource
                                                            == Improvement.ImprovementSource.CritterPower
                                                            && x.SourceName == strPowerId && x.Enabled));
                            }

                            break;

                        case Improvement.ImprovementType.MentorSpirit:
                        case Improvement.ImprovementType.Paragon:
                            MentorSpirit objMentor
                                = objCharacter.MentorSpirits.FirstOrDefault(
                                    x => x.InternalId == strImprovedName);
                            if (objMentor != null)
                            {
                                EnableImprovements(objCharacter,
                                                   objCharacter.Improvements.Where(
                                                       x => x.ImproveSource
                                                            == Improvement.ImprovementSource.MentorSpirit
                                                            && x.SourceName == strImprovedName && x.Enabled));
                            }

                            break;

                        case Improvement.ImprovementType.Gear:
                            Gear objGear
                                = objCharacter.Gear.FirstOrDefault(x => x.InternalId == strImprovedName);
                            objGear?.ChangeEquippedStatus(true);
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
                            Spell objSpell
                                = objCharacter.Spells.FirstOrDefault(x => x.InternalId == strImprovedName);
                            if (objSpell != null)
                            {
                                EnableImprovements(objCharacter,
                                                   objCharacter.Improvements.Where(
                                                       x => x.ImproveSource == Improvement.ImprovementSource.Spell
                                                            && x.SourceName == strImprovedName && x.Enabled));
                            }

                            break;

                        case Improvement.ImprovementType.ComplexForm:
                            ComplexForm objComplexForm
                                = objCharacter.ComplexForms.FirstOrDefault(
                                    x => x.InternalId == strImprovedName);
                            if (objComplexForm != null)
                            {
                                EnableImprovements(objCharacter,
                                                   objCharacter.Improvements.Where(
                                                       x => x.ImproveSource == Improvement.ImprovementSource.ComplexForm
                                                            && x.SourceName == strImprovedName && x.Enabled));
                            }

                            break;

                        case Improvement.ImprovementType.MartialArt:
                            MartialArt objMartialArt
                                = objCharacter.MartialArts.FirstOrDefault(x => x.InternalId == strImprovedName);
                            if (objMartialArt != null)
                            {
                                EnableImprovements(objCharacter,
                                                   objCharacter.Improvements.Where(
                                                       x => x.ImproveSource == Improvement.ImprovementSource.MartialArt
                                                            && x.SourceName == strImprovedName && x.Enabled));
                                // Remove the Improvements for any Techniques for the Martial Art that is being removed.
                                foreach (MartialArtTechnique objTechnique in objMartialArt.Techniques)
                                {
                                    string strTechniqueId = objTechnique.InternalId;
                                    EnableImprovements(objCharacter,
                                                       objCharacter.Improvements.Where(
                                                           x => x.ImproveSource == Improvement.ImprovementSource
                                                                    .MartialArtTechnique
                                                                && x.SourceName == strTechniqueId && x.Enabled));
                                }
                            }

                            break;

                        case Improvement.ImprovementType.SpecialSkills:
                        {
                            SkillsSection.FilterOption eFilterOption
                                = (SkillsSection.FilterOption) Enum.Parse(
                                    typeof(SkillsSection.FilterOption), strImprovedName);
                            foreach (Skill objSkill in objCharacter.SkillsSection.GetActiveSkillsFromData(
                                         eFilterOption, false, objImprovement.Target))
                            {
                                objSkill.ForceDisabled = false;
                            }
                        }
                            break;

                        case Improvement.ImprovementType.SpecificQuality:
                            Quality objQuality = objCharacter.Qualities.FirstOrDefault(
                                objLoopQuality => objLoopQuality.InternalId == strImprovedName);
                            if (objQuality != null)
                            {
                                EnableImprovements(objCharacter,
                                                   objCharacter.Improvements.Where(
                                                       x => x.ImproveSource == Improvement.ImprovementSource.Quality
                                                            && x.SourceName == strImprovedName && x.Enabled));
                            }

                            break;
                        /*
                        case Improvement.ImprovementType.SkillSpecialization:
                            {
                                Skill objSkill = objCharacter.SkillsSection.GetActiveSkill(strImprovedName);
                                SkillSpecialization objSkillSpec = objSkill?.Specializations.FirstOrDefault(x => x.Name == strUniqueName);
                                //if (objSkillSpec != null)
                                // TODO: Add temporarily remove skill specialization
                            }
                            break;
                            */
                        case Improvement.ImprovementType.AIProgram:
                            AIProgram objProgram = objCharacter.AIPrograms.FirstOrDefault(
                                objLoopProgram => objLoopProgram.InternalId == strImprovedName);
                            if (objProgram != null)
                            {
                                DisableImprovements(objCharacter,
                                                    objCharacter.Improvements.Where(
                                                        x => x.ImproveSource == Improvement.ImprovementSource.AIProgram
                                                             && x.SourceName == strImprovedName && x.Enabled));
                            }

                            break;

                        case Improvement.ImprovementType.FreeWare:
                        {
                            Cyberware objCyberware
                                = objCharacter.Cyberware.FirstOrDefault(o => o.InternalId == strImprovedName);
                            objCyberware?.ChangeModularEquip(true);
                        }
                            break;
                    }
                }

                objImprovementList.ProcessRelevantEvents();
            }
        }

        public static void DisableImprovements(Character objCharacter, IEnumerable<Improvement> objImprovementList)
        {
            DisableImprovements(objCharacter, objImprovementList.ToList());
        }

        public static void DisableImprovements(Character objCharacter, params Improvement[] objImprovementList)
        {
            DisableImprovements(objCharacter, Array.AsReadOnly(objImprovementList));
        }

        public static void DisableImprovements(Character objCharacter,
                                               IReadOnlyCollection<Improvement> objImprovementList)
        {
            if (objCharacter == null)
                throw new ArgumentNullException(nameof(objCharacter));
            if (objImprovementList == null)
                throw new ArgumentNullException(nameof(objImprovementList));

            using (EnterWriteLock.Enter(objCharacter.LockObject))
            {
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
                    bool blnHasDuplicate = objCharacter.Improvements.Any(
                        x => x.UniqueName == strUniqueName && x.ImprovedName == strImprovedName
                                                           && x.ImproveType == eImprovementType
                                                           && x.SourceName != strSourceName
                                                           && x.Enabled);

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
                                foreach (KnowledgeSkill objKnowledgeSkill in objCharacter.SkillsSection.KnowsoftSkills)
                                {
                                    objCharacter.SkillsSection.KnowledgeSkills.Remove(objKnowledgeSkill);
                                }
                            }

                            break;

                        case Improvement.ImprovementType.Skillsoft:
                            if (!blnHasDuplicate)
                            {
                                objCharacter.SkillsSection.KnowsoftSkills.RemoveAll(
                                    x => x.InternalId == strImprovedName);
                            }

                            break;

                        case Improvement.ImprovementType.Attribute:
                            // Determine if access to any Special Attributes have been lost.
                            if (strUniqueName == "enableattribute" && !blnHasDuplicate)
                            {
                                switch (strImprovedName)
                                {
                                    case "MAG":
                                        objCharacter.MAGEnabled = false;
                                        break;

                                    case "RES":
                                        objCharacter.RESEnabled = false;
                                        break;

                                    case "DEP":
                                        objCharacter.DEPEnabled = false;
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
                                = objCharacter.Contacts.FirstOrDefault(c => c.UniqueId == strImprovedName);
                            if (objNewContact != null)
                            {
                                // TODO: Add code to disable contact
                            }

                            break;

                        case Improvement.ImprovementType.Art:
                            Art objArt = objCharacter.Arts.FirstOrDefault(x => x.InternalId == strImprovedName);
                            if (objArt != null)
                            {
                                Improvement.ImprovementSource eSource = objArt.SourceType;
                                DisableImprovements(objCharacter,
                                                    objCharacter.Improvements.Where(
                                                        x => x.ImproveSource == eSource
                                                             && x.SourceName == strImprovedName && x.Enabled));
                            }

                            break;

                        case Improvement.ImprovementType.Metamagic:
                        case Improvement.ImprovementType.Echo:
                            Metamagic objMetamagic
                                = objCharacter.Metamagics.FirstOrDefault(x => x.InternalId == strImprovedName);
                            if (objMetamagic != null)
                            {
                                Improvement.ImprovementSource eSource
                                    = objImprovement.ImproveType == Improvement.ImprovementType.Metamagic
                                        ? Improvement.ImprovementSource.Metamagic
                                        : Improvement.ImprovementSource.Echo;
                                DisableImprovements(objCharacter,
                                                    objCharacter.Improvements.Where(
                                                        x => x.ImproveSource == eSource
                                                             && x.SourceName == strImprovedName && x.Enabled));
                            }

                            break;

                        case Improvement.ImprovementType.CritterPower:
                            CritterPower objCritterPower = objCharacter.CritterPowers.FirstOrDefault(
                                x => x.InternalId == strImprovedName || (x.Name == strImprovedName
                                                                         && x.Extra == strUniqueName));
                            if (objCritterPower != null)
                            {
                                string strPowerId = objCritterPower.InternalId;
                                DisableImprovements(objCharacter,
                                                    objCharacter.Improvements.Where(
                                                        x => x.ImproveSource
                                                             == Improvement.ImprovementSource.CritterPower
                                                             && x.SourceName == strPowerId && x.Enabled));
                            }

                            break;

                        case Improvement.ImprovementType.MentorSpirit:
                        case Improvement.ImprovementType.Paragon:
                            MentorSpirit objMentor
                                = objCharacter.MentorSpirits.FirstOrDefault(
                                    x => x.InternalId == strImprovedName);
                            if (objMentor != null)
                            {
                                DisableImprovements(objCharacter,
                                                    objCharacter.Improvements.Where(
                                                        x => x.ImproveSource
                                                             == Improvement.ImprovementSource.MentorSpirit
                                                             && x.SourceName == strImprovedName && x.Enabled));
                            }

                            break;

                        case Improvement.ImprovementType.Gear:
                            Gear objGear
                                = objCharacter.Gear.FirstOrDefault(x => x.InternalId == strImprovedName);
                            objGear?.ChangeEquippedStatus(false);
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
                            Spell objSpell
                                = objCharacter.Spells.FirstOrDefault(x => x.InternalId == strImprovedName);
                            if (objSpell != null)
                            {
                                DisableImprovements(objCharacter,
                                                    objCharacter.Improvements.Where(
                                                        x => x.ImproveSource == Improvement.ImprovementSource.Spell
                                                             && x.SourceName == strImprovedName && x.Enabled));
                            }

                            break;

                        case Improvement.ImprovementType.ComplexForm:
                            ComplexForm objComplexForm
                                = objCharacter.ComplexForms.FirstOrDefault(
                                    x => x.InternalId == strImprovedName);
                            if (objComplexForm != null)
                            {
                                DisableImprovements(objCharacter,
                                                    objCharacter.Improvements.Where(
                                                        x => x.ImproveSource
                                                             == Improvement.ImprovementSource.ComplexForm
                                                             && x.SourceName == strImprovedName && x.Enabled));
                            }

                            break;

                        case Improvement.ImprovementType.MartialArt:
                            MartialArt objMartialArt
                                = objCharacter.MartialArts.FirstOrDefault(x => x.InternalId == strImprovedName);
                            if (objMartialArt != null)
                            {
                                DisableImprovements(objCharacter,
                                                    objCharacter.Improvements.Where(
                                                        x => x.ImproveSource == Improvement.ImprovementSource.MartialArt
                                                             && x.SourceName == strImprovedName && x.Enabled));
                                // Remove the Improvements for any Techniques for the Martial Art that is being removed.
                                foreach (MartialArtTechnique objTechnique in objMartialArt.Techniques)
                                {
                                    string strTechniqueId = objTechnique.InternalId;
                                    DisableImprovements(objCharacter,
                                                        objCharacter.Improvements.Where(
                                                            x => x.ImproveSource == Improvement.ImprovementSource
                                                                     .MartialArtTechnique
                                                                 && x.SourceName == strTechniqueId && x.Enabled));
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
                                    = new HashSet<Skill>(objCharacter.SkillsSection.GetActiveSkillsFromData(
                                                             eFilterOption, false, objImprovement.Target));
                                foreach (Improvement objLoopImprovement in GetCachedImprovementListForValueOf(
                                             objCharacter, Improvement.ImprovementType.SpecialSkills))
                                {
                                    if (objLoopImprovement == objImprovement)
                                        continue;
                                    eFilterOption
                                        = (SkillsSection.FilterOption) Enum.Parse(
                                            typeof(SkillsSection.FilterOption), objLoopImprovement.ImprovedName);
                                    setSkillsToDisable.ExceptWith(
                                        objCharacter.SkillsSection.GetActiveSkillsFromData(
                                            eFilterOption, false, objLoopImprovement.Target));
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
                            Quality objQuality = objCharacter.Qualities.FirstOrDefault(
                                objLoopQuality => objLoopQuality.InternalId == strImprovedName);
                            if (objQuality != null)
                            {
                                DisableImprovements(objCharacter,
                                                    objCharacter.Improvements.Where(
                                                        x => x.ImproveSource == Improvement.ImprovementSource.Quality
                                                             && x.SourceName == strImprovedName && x.Enabled));
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
                            AIProgram objProgram = objCharacter.AIPrograms.FirstOrDefault(
                                objLoopProgram => objLoopProgram.InternalId == strImprovedName);
                            if (objProgram != null)
                            {
                                DisableImprovements(objCharacter,
                                                    objCharacter.Improvements.Where(
                                                        x => x.ImproveSource == Improvement.ImprovementSource.AIProgram
                                                             && x.SourceName == strImprovedName && x.Enabled));
                            }

                            break;

                        case Improvement.ImprovementType.FreeWare:
                        {
                            Cyberware objCyberware
                                = objCharacter.Cyberware.FirstOrDefault(o => o.InternalId == strImprovedName);
                            objCyberware?.ChangeModularEquip(false);
                        }
                            break;
                    }
                }

                objImprovementList.ProcessRelevantEvents();
            }
        }

        /// <summary>
        /// Remove all of the Improvements for an XML Node.
        /// </summary>
        /// <param name="objCharacter">Character from which improvements should be deleted.</param>
        /// <param name="objImprovementSource">Type of object that granted these Improvements.</param>
        /// <param name="strSourceName">Name of the item that granted these Improvements.</param>
        public static decimal RemoveImprovements(Character objCharacter,
                                                 Improvement.ImprovementSource objImprovementSource,
                                                 string strSourceName = "")
        {
            // If there is no character object, don't try to remove any Improvements.
            if (objCharacter == null)
            {
                return 0;
            }

            Log.Debug("RemoveImprovements called with:" + Environment.NewLine + "objImprovementSource = "
                     + objImprovementSource + Environment.NewLine + "strSourceName = " + strSourceName);
            List<Improvement> objImprovementList;
            using (EnterReadLock.Enter(objCharacter.LockObject))
            {
                // A List of Improvements to hold all of the items that will eventually be deleted.
                objImprovementList = (string.IsNullOrEmpty(strSourceName)
                    ? objCharacter.Improvements.Where(objImprovement =>
                                                          objImprovement.ImproveSource == objImprovementSource)
                    : objCharacter.Improvements.Where(objImprovement =>
                                                          objImprovement.ImproveSource == objImprovementSource
                                                          && objImprovement.SourceName == strSourceName)).ToList();
            }

            // Compatibility fix for when blnConcatSelectedValue was around
            if (strSourceName.IsGuid())
            {
                string strSourceNameSpaced = strSourceName + LanguageManager.GetString("String_Space");
                string strSourceNameSpacedInvariant = strSourceName + ' ';
                using (EnterReadLock.Enter(objCharacter.LockObject))
                {
                    objImprovementList.AddRange(objCharacter.Improvements.Where(
                                                    objImprovement =>
                                                        objImprovement.ImproveSource == objImprovementSource &&
                                                        (objImprovement.SourceName.StartsWith(
                                                             strSourceNameSpaced, StringComparison.Ordinal)
                                                         || objImprovement.SourceName.StartsWith(
                                                             strSourceNameSpacedInvariant, StringComparison.Ordinal))));
                }
            }

            return RemoveImprovements(objCharacter, objImprovementList);
        }

        /// <summary>
        /// Remove all of the Improvements for an XML Node.
        /// </summary>
        /// <param name="objCharacter">Character from which improvements should be deleted.</param>
        /// <param name="objImprovementList">List of improvements to delete.</param>
        /// <param name="blnReapplyImprovements">Whether we're reapplying Improvements.</param>
        /// <param name="blnAllowDuplicatesFromSameSource">If we ignore checking whether a potential duplicate improvement has the same SourceName</param>
        public static decimal RemoveImprovements(Character objCharacter, ICollection<Improvement> objImprovementList,
                                                 bool blnReapplyImprovements = false,
                                                 bool blnAllowDuplicatesFromSameSource = false)
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
            using (EnterWriteLock.Enter(objCharacter.LockObject))
            {
                // Note: As attractive as it may be to replace objImprovementList with an IEnumerable, we need to iterate through it twice for performance reasons

                // Now that we have all of the applicable Improvements, remove them from the character.
                foreach (Improvement objImprovement in objImprovementList)
                {
                    // Remove the Improvement.
                    objCharacter.Improvements.Remove(objImprovement);
                    ClearCachedValue(objCharacter, objImprovement.ImproveType, objImprovement.ImprovedName);
                }

                // Now that the entire list is deleted from the character's improvements list, we do the checking of duplicates and extra effects
                foreach (Improvement objImprovement in objImprovementList)
                {
                    string strImprovedName = objImprovement.ImprovedName;
                    string strUniqueName = objImprovement.UniqueName;
                    Improvement.ImprovementType eImprovementType = objImprovement.ImproveType;
                    // See if the character has anything else that is granting them the same bonus as this improvement
                    bool blnHasDuplicate;
                    if (blnAllowDuplicatesFromSameSource)
                        blnHasDuplicate = objCharacter.Improvements.Any(
                            x => x.UniqueName == strUniqueName && x.ImprovedName == strImprovedName
                                                               && x.ImproveType == eImprovementType);
                    else
                    {
                        string strSourceName = objImprovement.SourceName;
                        blnHasDuplicate = objCharacter.Improvements.Any(
                            x => x.UniqueName == strUniqueName && x.ImprovedName == strImprovedName
                                                               && x.ImproveType == eImprovementType
                                                               && x.SourceName != strSourceName);
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
                                foreach (KnowledgeSkill objKnowledgeSkill in objCharacter.SkillsSection.KnowsoftSkills)
                                {
                                    objCharacter.SkillsSection.KnowledgeSkills.Remove(objKnowledgeSkill);
                                }
                            }

                            break;

                        case Improvement.ImprovementType.Skillsoft:
                            if (!blnHasDuplicate)
                            {
                                objCharacter.SkillsSection.KnowledgeSkills.RemoveAll(
                                    x => x.InternalId == strImprovedName);
                                for (int i = objCharacter.SkillsSection.KnowsoftSkills.Count - 1; i >= 0; --i)
                                {
                                    KnowledgeSkill objSkill = objCharacter.SkillsSection.KnowsoftSkills[i];
                                    if (objSkill.InternalId == strImprovedName)
                                    {
                                        objCharacter.SkillsSection.KnowledgeSkills.Remove(objSkill);
                                        objCharacter.SkillsSection.KnowsoftSkills.RemoveAt(i);
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
                                        objCharacter.MAGEnabled = false;
                                        break;

                                    case "RES":
                                        objCharacter.RESEnabled = false;
                                        break;

                                    case "DEP":
                                        objCharacter.DEPEnabled = false;
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
                                foreach (Cyberware objCyberware in objCharacter.Cyberware.DeepWhere(
                                             x => x.Children, x => x.Grade.Adapsin))
                                {
                                    string strNewName = objCyberware.Grade.Name.FastEscapeOnceFromEnd("(Adapsin)")
                                                                    .Trim();
                                    // Determine which GradeList to use for the Cyberware.
                                    objCyberware.Grade = objCharacter.GetGradeList(objCyberware.SourceType, true)
                                                                     .FirstOrDefault(x => x.Name == strNewName);
                                }
                            }
                        }
                            break;

                        case Improvement.ImprovementType.AddContact:
                            Contact objNewContact
                                = objCharacter.Contacts.FirstOrDefault(c => c.UniqueId == strImprovedName);
                            if (objNewContact != null)
                                objCharacter.Contacts.Remove(objNewContact);
                            break;

                        case Improvement.ImprovementType.Art:
                            Art objArt = objCharacter.Arts.FirstOrDefault(x => x.InternalId == strImprovedName);
                            if (objArt != null)
                            {
                                decReturn += RemoveImprovements(objCharacter, objArt.SourceType, objArt.InternalId);
                                objCharacter.Arts.Remove(objArt);
                            }

                            break;

                        case Improvement.ImprovementType.Metamagic:
                        case Improvement.ImprovementType.Echo:
                            Metamagic objMetamagic
                                = objCharacter.Metamagics.FirstOrDefault(x => x.InternalId == strImprovedName);
                            if (objMetamagic != null)
                            {
                                decReturn += RemoveImprovements(objCharacter,
                                                                objImprovement.ImproveType
                                                                == Improvement.ImprovementType.Metamagic
                                                                    ? Improvement.ImprovementSource.Metamagic
                                                                    : Improvement.ImprovementSource.Echo,
                                                                objMetamagic.InternalId);
                                objCharacter.Metamagics.Remove(objMetamagic);
                            }

                            break;

                        case Improvement.ImprovementType.LimitModifier:
                            LimitModifier limitMod
                                = objCharacter.LimitModifiers.FirstOrDefault(
                                    x => x.InternalId == strImprovedName);
                            if (limitMod != null)
                            {
                                objCharacter.LimitModifiers.Remove(limitMod);
                            }

                            break;

                        case Improvement.ImprovementType.CritterPower:
                            CritterPower objCritterPower = objCharacter.CritterPowers.FirstOrDefault(
                                x => x.InternalId == strImprovedName || (x.Name == strImprovedName
                                                                         && x.Extra == strUniqueName));
                            if (objCritterPower != null)
                            {
                                decReturn += RemoveImprovements(objCharacter,
                                                                Improvement.ImprovementSource.CritterPower,
                                                                objCritterPower.InternalId);
                                objCharacter.CritterPowers.Remove(objCritterPower);
                            }

                            break;

                        case Improvement.ImprovementType.MentorSpirit:
                        case Improvement.ImprovementType.Paragon:
                            MentorSpirit objMentor
                                = objCharacter.MentorSpirits.FirstOrDefault(
                                    x => x.InternalId == strImprovedName);
                            if (objMentor != null)
                            {
                                decReturn += RemoveImprovements(objCharacter,
                                                                Improvement.ImprovementSource.MentorSpirit,
                                                                objMentor.InternalId);
                                objCharacter.MentorSpirits.Remove(objMentor);
                            }

                            break;

                        case Improvement.ImprovementType.Gear:
                            Gear objGear
                                = objCharacter.Gear.FirstOrDefault(x => x.InternalId == strImprovedName);
                            if (objGear != null)
                            {
                                decReturn += objGear.TotalCost;
                                decReturn += objGear.DeleteGear();
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
                                decReturn += objWeapon.TotalCost;
                                decReturn += objWeapon.DeleteWeapon();
                            }
                        }
                            break;

                        case Improvement.ImprovementType.Spell:
                            Spell objSpell
                                = objCharacter.Spells.FirstOrDefault(x => x.InternalId == strImprovedName);
                            if (objSpell != null)
                            {
                                decReturn += RemoveImprovements(objCharacter, Improvement.ImprovementSource.Spell,
                                                                objSpell.InternalId);
                                objCharacter.Spells.Remove(objSpell);
                            }

                            break;

                        case Improvement.ImprovementType.ComplexForm:
                            ComplexForm objComplexForm
                                = objCharacter.ComplexForms.FirstOrDefault(
                                    x => x.InternalId == strImprovedName);
                            if (objComplexForm != null)
                            {
                                decReturn += RemoveImprovements(objCharacter, Improvement.ImprovementSource.ComplexForm,
                                                                objComplexForm.InternalId);
                                objCharacter.ComplexForms.Remove(objComplexForm);
                            }

                            break;

                        case Improvement.ImprovementType.MartialArt:
                            MartialArt objMartialArt
                                = objCharacter.MartialArts.FirstOrDefault(x => x.InternalId == strImprovedName);
                            if (objMartialArt != null)
                            {
                                decReturn += RemoveImprovements(objCharacter, Improvement.ImprovementSource.MartialArt,
                                                                objMartialArt.InternalId);
                                // Remove the Improvements for any Techniques for the Martial Art that is being removed.
                                foreach (MartialArtTechnique objTechnique in objMartialArt.Techniques)
                                {
                                    decReturn += RemoveImprovements(objCharacter,
                                                                    Improvement.ImprovementSource.MartialArtTechnique,
                                                                    objTechnique.InternalId);
                                }

                                objCharacter.MartialArts.Remove(objMartialArt);
                            }

                            break;

                        case Improvement.ImprovementType.SpecialSkills:
                            if (!blnHasDuplicate)
                            {
                                objCharacter.SkillsSection.RemoveSkills(
                                    (SkillsSection.FilterOption) Enum.Parse(typeof(SkillsSection.FilterOption),
                                                                            strImprovedName), objImprovement.Target,
                                    !blnReapplyImprovements && objCharacter.Created);
                            }

                            break;

                        case Improvement.ImprovementType.SpecificQuality:
                            Quality objQuality = objCharacter.Qualities.FirstOrDefault(
                                objLoopQuality => objLoopQuality.InternalId == strImprovedName);
                            if (objQuality != null)
                            {
                                // We need to add in the return cost of deleting the quality, so call this manually
                                decReturn += objQuality.DeleteQuality();
                            }

                            break;

                        case Improvement.ImprovementType.SkillSpecialization:
                        case Improvement.ImprovementType.SkillExpertise:
                        {
                            Skill objSkill = objCharacter.SkillsSection.GetActiveSkill(strImprovedName);
                            SkillSpecialization objSkillSpec = strUniqueName.IsGuid()
                                ? objSkill?.Specializations.FirstOrDefault(x => x.InternalId == strUniqueName)
                                // Kept for legacy reasons
                                : objSkill?.Specializations.FirstOrDefault(x => x.Name == strUniqueName);
                            if (objSkillSpec != null)
                                objSkill.Specializations.Remove(objSkillSpec);
                        }
                            break;

                        case Improvement.ImprovementType.AIProgram:
                            AIProgram objProgram = objCharacter.AIPrograms.FirstOrDefault(
                                objLoopProgram => objLoopProgram.InternalId == strImprovedName);
                            if (objProgram != null)
                            {
                                decReturn += RemoveImprovements(objCharacter, Improvement.ImprovementSource.AIProgram,
                                                                objProgram.InternalId);
                                objCharacter.AIPrograms.Remove(objProgram);
                            }

                            break;

                        case Improvement.ImprovementType.AdeptPowerFreeLevels:
                        case Improvement.ImprovementType.AdeptPowerFreePoints:
                            // Get the power improved by this improvement
                            Power objImprovedPower = objCharacter.Powers.FirstOrDefault(
                                objPower => objPower.Name == strImprovedName &&
                                            objPower.Extra == strUniqueName);
                            if (objImprovedPower != null)
                            {
                                if (objImprovedPower.TotalRating <= 0)
                                {
                                    objImprovedPower.DeletePower();
                                    objImprovedPower.UnbindPower();
                                }

                                objImprovedPower.OnPropertyChanged(nameof(objImprovedPower.TotalRating));
                                objImprovedPower.OnPropertyChanged(
                                    objImprovement.ImproveType == Improvement.ImprovementType.AdeptPowerFreeLevels
                                        ? nameof(Power.FreeLevels)
                                        : nameof(Power.FreePoints));
                            }

                            break;

                        case Improvement.ImprovementType.FreeWare:
                        {
                            Cyberware objCyberware
                                = objCharacter.Cyberware.FirstOrDefault(o => o.InternalId == strImprovedName);
                            if (objCyberware != null)
                            {
                                decReturn += objCyberware.TotalCost;
                                decReturn += objCyberware.DeleteCyberware();
                            }
                        }
                            break;
                    }
                }

                objImprovementList.ProcessRelevantEvents();
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
        public static Improvement CreateImprovement(Character objCharacter, string strImprovedName,
                                             Improvement.ImprovementSource objImprovementSource,
                                             string strSourceName, Improvement.ImprovementType objImprovementType,
                                             string strUnique,
                                             decimal decValue = 0, int intRating = 1, int intMinimum = 0,
                                             int intMaximum = 0, decimal decAugmented = 0,
                                             int intAugmentedMaximum = 0, string strExclude = "",
                                             bool blnAddToRating = false, string strTarget = "",
                                             string strCondition = "")
        {
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
                using (EnterWriteLock.Enter(objCharacter.LockObject))
                {
                    // Record the improvement.
                    objImprovement = new Improvement(objCharacter)
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
                    ClearCachedValue(objCharacter, objImprovementType, strImprovedName);

                    // Add the Improvement to the Transaction List.
                    List<Improvement> lstTransactions;
                    while (!s_DictionaryTransactions.TryGetValue(objCharacter, out lstTransactions))
                    {
                        lstTransactions = new List<Improvement>(1);
                        if (s_DictionaryTransactions.TryAdd(objCharacter, lstTransactions))
                            break;
                    }

                    lstTransactions.Add(objImprovement);
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
        private static void Rollback(Character objCharacter)
        {
            Log.Debug("Rollback enter");
            if (s_DictionaryTransactions.TryRemove(objCharacter, out List<Improvement> lstTransactions))
            {
                using (EnterWriteLock.Enter(objCharacter.LockObject))
                {
                    // Remove all of the Improvements that were added.
                    foreach (Improvement objTransactingImprovement in lstTransactions)
                    {
                        RemoveImprovements(objCharacter, objTransactingImprovement.ImproveSource,
                                           objTransactingImprovement.SourceName);
                        ClearCachedValue(objCharacter, objTransactingImprovement.ImproveType,
                                         objTransactingImprovement.ImprovedName);
                    }
                }

                lstTransactions.Clear();
            }

            Log.Debug("Rollback exit");
        }

        /// <summary>
        /// Fire off all events relevant to an enumerable of improvements, making sure each event is only fired once.
        /// </summary>
        /// <param name="lstImprovements">Enumerable of improvements whose events to fire</param>
        public static void ProcessRelevantEvents(this IEnumerable<Improvement> lstImprovements)
        {
            if (lstImprovements == null)
                return;
            // Create a hashset of events to fire to make sure we only ever fire each event once
            using (new FetchSafelyFromPool<Dictionary<INotifyMultiplePropertyChanged, HashSet<string>>>(
                       Utils.DictionaryForMultiplePropertyChangedPool,
                       out Dictionary<INotifyMultiplePropertyChanged, HashSet<string>> dicChangedProperties))
            {
                try
                {
                    foreach (Improvement objImprovement in lstImprovements.Where(x => x.SetupComplete))
                    {
                        foreach ((INotifyMultiplePropertyChanged objToNotify, string strProperty) in objImprovement
                                     .GetRelevantPropertyChangers())
                        {
                            if (!dicChangedProperties.TryGetValue(objToNotify,
                                                                  out HashSet<string> setLoopPropertiesChanged))
                            {
                                setLoopPropertiesChanged = Utils.StringHashSetPool.Get();
                                dicChangedProperties.Add(objToNotify, setLoopPropertiesChanged);
                            }
                            setLoopPropertiesChanged.Add(strProperty);
                        }
                    }

                    // Fire each event once
                    foreach (KeyValuePair<INotifyMultiplePropertyChanged, HashSet<string>> kvpChangedProperties in
                             dicChangedProperties)
                        kvpChangedProperties.Key.OnMultiplePropertyChanged(kvpChangedProperties.Value.ToList());
                }
                finally
                {
                    foreach (HashSet<string> setToReturn in dicChangedProperties.Values)
                        Utils.StringHashSetPool.Return(setToReturn);
                }
            }
        }

        #endregion Improvement System
    }
}
