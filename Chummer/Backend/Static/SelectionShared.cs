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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Attributes;
using Chummer.Backend.Equipment;
using Chummer.Backend.Skills;

namespace Chummer
{
    public static class SelectionShared
    {
        /// <summary>Evaluates requirements of a given node against a given Character object.</summary>
        /// <param name="xmlNode">XmlNode of the object.</param>
        /// <param name="objCharacter">Character object against which to check.</param>
        /// <param name="objParent">Parent object to be compared to.</param>
        /// <param name="strLocalName">Name of the type of item being checked for displaying messages. If empty or null, no message is displayed.</param>
        /// <param name="strIgnoreQuality">Name of a Quality that should be ignored. Typically used when swapping Qualities in career mode.</param>
        /// <param name="strLocation">Limb side to use if we need a specific limb side (Left or Right)</param>
        /// <param name="blnIgnoreLimit">Whether to ignore checking for limits on the total amount of this item the character can have.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [Obsolete("This method is a wrapper that calls XPathNavigator instead. Where possible, refactor the calling object to an XPathNavigator instead.", false)]
        public static bool RequirementsMet(this XmlNode xmlNode, Character objCharacter, object objParent = null, string strLocalName = "", string strIgnoreQuality = "", string strLocation = "", bool blnIgnoreLimit = false, CancellationToken token = default)
        {
            if (xmlNode == null || objCharacter == null)
                return false;
            // Ignore the rules.
            if (objCharacter.IgnoreRules)
                return true;
            XPathNavigator objNavigator = xmlNode.CreateNavigator();
            return Utils.SafelyRunSynchronously(() => objNavigator.RequirementsMetCoreAsync(
                                                    true, objCharacter, objParent, strLocalName,
                                                    strIgnoreQuality, strLocation,
                                                    blnIgnoreLimit, token), token);
        }

        /// <summary>Evaluates requirements of a given node against a given Character object.</summary>
        /// <param name="xmlNode">XmlNode of the object.</param>
        /// <param name="objCharacter">Character object against which to check.</param>
        /// <param name="objParent">Parent object to be compared to.</param>
        /// <param name="strLocalName">Name of the type of item being checked for displaying messages. If empty or null, no message is displayed.</param>
        /// <param name="strIgnoreQuality">Name of a Quality that should be ignored. Typically used when swapping Qualities in career mode.</param>
        /// <param name="strLocation">Limb side to use if we need a specific limb side (Left or Right)</param>
        /// <param name="blnIgnoreLimit">Whether to ignore checking for limits on the total amount of this item the character can have.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        [Obsolete("This method is a wrapper that calls XPathNavigator instead. Where possible, refactor the calling object to an XPathNavigator instead.", false)]
        public static Task<bool> RequirementsMetAsync(this XmlNode xmlNode, Character objCharacter, object objParent = null, string strLocalName = "", string strIgnoreQuality = "", string strLocation = "", bool blnIgnoreLimit = false, CancellationToken token = default)
        {
            if (xmlNode == null || objCharacter == null)
                return Task.FromResult(false);
            // Ignore the rules.
            if (objCharacter.IgnoreRules)
                return Task.FromResult(true);
            XPathNavigator objNavigator = xmlNode.CreateNavigator();
            return objNavigator.RequirementsMetCoreAsync(false, objCharacter, objParent, strLocalName, strIgnoreQuality,
                                                         strLocation, blnIgnoreLimit, token);
        }

        //TODO: Might be a better location for this; Class names are screwy.
        /// <summary>Evaluates requirements of a given node against a given Character object.</summary>
        /// <param name="xmlNode">XmlNode of the object.</param>
        /// <param name="objCharacter">Character object against which to check.</param>
        /// <param name="objParent">Parent object against which to check.</param>
        /// <param name="strLocalName">Name of the type of item being checked for displaying messages. If empty or null, no message is displayed.</param>
        /// <param name="strIgnoreQuality">Name (or ID) of a Quality that should be ignored. Typically used when swapping Qualities in career mode.</param>
        /// <param name="strLocation">Limb side to use if we need a specific limb side (Left or Right)</param>
        /// <param name="blnIgnoreLimit">Whether to ignore checking for limits on the total amount of this item the character can have.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static bool RequirementsMet(this XPathNavigator xmlNode, Character objCharacter, object objParent = null,
                                           string strLocalName = "", string strIgnoreQuality = "",
                                           string strLocation = "",
                                           bool blnIgnoreLimit = false, CancellationToken token = default)
        {
            return Utils.SafelyRunSynchronously(
                () => xmlNode.RequirementsMetCoreAsync(true, objCharacter, objParent, strLocalName, strIgnoreQuality,
                                                       strLocation, blnIgnoreLimit, token), token);
        }

        //TODO: Might be a better location for this; Class names are screwy.
        /// <summary>Evaluates requirements of a given node against a given Character object.</summary>
        /// <param name="xmlNode">XmlNode of the object.</param>
        /// <param name="objCharacter">Character object against which to check.</param>
        /// <param name="objParent">Parent object against which to check.</param>
        /// <param name="strLocalName">Name of the type of item being checked for displaying messages. If empty or null, no message is displayed.</param>
        /// <param name="strIgnoreQuality">Name (or ID) of a Quality that should be ignored. Typically used when swapping Qualities in career mode.</param>
        /// <param name="strLocation">Limb side to use if we need a specific limb side (Left or Right)</param>
        /// <param name="blnIgnoreLimit">Whether to ignore checking for limits on the total amount of this item the character can have.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public static Task<bool> RequirementsMetAsync(this XPathNavigator xmlNode, Character objCharacter, object objParent = null,
                                           string strLocalName = "", string strIgnoreQuality = "",
                                           string strLocation = "",
                                           bool blnIgnoreLimit = false, CancellationToken token = default)
        {
            return xmlNode.RequirementsMetCoreAsync(false, objCharacter, objParent, strLocalName, strIgnoreQuality,
                                                    strLocation, blnIgnoreLimit, token);
        }

        private readonly struct SpellCountTotals
        {
            public SpellCountTotals(int intSpellCount, int intRitualCount, int intAlchemicalCount)
            {
                SpellCount = intSpellCount;
                RitualCount = intRitualCount;
                AlchemicalCount = intAlchemicalCount;
            }

            public int SpellCount { get; }
            public int RitualCount { get; }
            public int AlchemicalCount { get; }
            public int TotalCount => SpellCount + RitualCount + AlchemicalCount;
        }

        private static async Task<SpellCountTotals> GetSpellCountsAsync(Character objCharacter, CancellationToken token = default)
        {
            if (objCharacter == null)
                return default;

            int intSpellCount = 0;
            int intRitualCount = 0;
            int intAlchemicalCount = 0;

            await objCharacter.Spells.ForEachAsync(objSpell =>
            {
                if (objSpell.Alchemical)
                {
                    intAlchemicalCount++;
                }
                else if (objSpell.Category == "Rituals")
                {
                    intRitualCount++;
                }
                else
                {
                    intSpellCount++;
                }
            }, token).ConfigureAwait(false);

            return new SpellCountTotals(intSpellCount, intRitualCount, intAlchemicalCount);
        }

        private static async Task<int> GetSpellLimitAsync(Character objCharacter, CancellationToken token = default)
        {
            if (objCharacter == null)
                return 0;

            int intMag = await (await objCharacter.GetAttributeAsync("MAG", token: token).ConfigureAwait(false))
                               .GetTotalValueAsync(token).ConfigureAwait(false);
            int intLimitMod = (int)(await ImprovementManager.ValueOfAsync(objCharacter, Improvement.ImprovementType.SpellLimit, token: token)
                                                      .ConfigureAwait(false));
            return intMag * 2 + intLimitMod;
        }

        public static async Task<bool> IsSpellLimitReachedAsync(Character objCharacter, CancellationToken token = default)
        {
            if (objCharacter == null)
                return false;
            if (await objCharacter.GetIgnoreRulesAsync(token).ConfigureAwait(false))
                return false;
            if (await objCharacter.GetCreatedAsync(token).ConfigureAwait(false))
                return false;

            SpellCountTotals objTotals = await GetSpellCountsAsync(objCharacter, token).ConfigureAwait(false);
            int intSpellLimit = await GetSpellLimitAsync(objCharacter, token).ConfigureAwait(false);
            return objTotals.TotalCount >= intSpellLimit;
        }

        public static async Task<bool> IsSpellLimitReachedAsync(Character objCharacter, string strCategory, bool blnIsAlchemical, CancellationToken token = default)
        {
            if (objCharacter == null)
                return false;
            if (await objCharacter.GetIgnoreRulesAsync(token).ConfigureAwait(false))
                return false;
            if (await objCharacter.GetCreatedAsync(token).ConfigureAwait(false))
                return false;

            SpellCountTotals objTotals = await GetSpellCountsAsync(objCharacter, token).ConfigureAwait(false);
            int intSpellLimit = await GetSpellLimitAsync(objCharacter, token).ConfigureAwait(false);

            if (blnIsAlchemical)
                return objTotals.AlchemicalCount >= intSpellLimit;
            if (string.Equals(strCategory, "Rituals", StringComparison.Ordinal))
                return objTotals.RitualCount >= intSpellLimit;
            return objTotals.SpellCount >= intSpellLimit;
        }

        private static async Task<bool> RequirementsMetCoreAsync(this XPathNavigator xmlNode, bool blnSync,
                                                                 Character objCharacter, object objParent = null,
                                                                 string strLocalName = "", string strIgnoreQuality = "",
                                                                 string strLocation = "",
                                                                 bool blnIgnoreLimit = false,
                                                                 CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (xmlNode == null || objCharacter == null)
                return false;
            // Ignore the rules.
            if (blnSync ? objCharacter.IgnoreRules : await objCharacter.GetIgnoreRulesAsync(token).ConfigureAwait(false))
                return true;
            bool blnShowMessage = !string.IsNullOrEmpty(strLocalName);
            // See if the character is in career mode but would want to add a chargen-only Quality
            if (blnSync ? objCharacter.Created : await objCharacter.GetCreatedAsync(token).ConfigureAwait(false))
            {
                if (xmlNode.SelectSingleNodeAndCacheExpression("chargenonly", token) != null)
                {
                    if (blnShowMessage)
                    {
                        if (blnSync)
                        {
                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                            Program.ShowScrollableMessageBox(
                                string.Format(GlobalSettings.CultureInfo,
                                    // ReSharper disable once MethodHasAsyncOverload
                                    LanguageManager.GetString("Message_SelectGeneric_ChargenRestriction", token: token),
                                    strLocalName),
                                // ReSharper disable once MethodHasAsyncOverload
                                string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("MessageTitle_SelectGeneric_Restriction", token: token), strLocalName),
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            await Program.ShowScrollableMessageBoxAsync(
                                string.Format(GlobalSettings.CultureInfo,
                                    await LanguageManager.GetStringAsync("Message_SelectGeneric_ChargenRestriction", token: token)
                                        .ConfigureAwait(false),
                                    strLocalName),
                                string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("MessageTitle_SelectGeneric_Restriction", token: token)
                                    .ConfigureAwait(false), strLocalName),
                                MessageBoxButtons.OK, MessageBoxIcon.Information, token: token).ConfigureAwait(false);
                        }
                    }

                    return false;
                }
            }
            // See if the character is using priority-based gen and is trying to add a Quality that can only be added through priorities
            else
            {
                if (xmlNode.SelectSingleNodeAndCacheExpression("careeronly", token) != null)
                {
                    if (blnShowMessage)
                    {
                        if (blnSync)
                        {
                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                            Program.ShowScrollableMessageBox(
                                string.Format(GlobalSettings.CultureInfo,
                                    // ReSharper disable once MethodHasAsyncOverload
                                    LanguageManager.GetString("Message_SelectGeneric_CareerOnlyRestriction", token: token),
                                    strLocalName),
                                // ReSharper disable once MethodHasAsyncOverload
                                string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("MessageTitle_SelectGeneric_Restriction", token: token), strLocalName),
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            await Program.ShowScrollableMessageBoxAsync(
                                string.Format(GlobalSettings.CultureInfo,
                                    await LanguageManager.GetStringAsync("Message_SelectGeneric_CareerOnlyRestriction", token: token)
                                        .ConfigureAwait(false),
                                    strLocalName),
                                string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("MessageTitle_SelectGeneric_Restriction", token: token)
                                    .ConfigureAwait(false), strLocalName),
                                MessageBoxButtons.OK, MessageBoxIcon.Information, token: token).ConfigureAwait(false);
                        }
                    }

                    return false;
                }

                if (objCharacter.EffectiveBuildMethodUsesPriorityTables
                    && xmlNode.SelectSingleNodeAndCacheExpression("onlyprioritygiven", token) != null)
                {
                    if (blnShowMessage)
                    {
                        if (blnSync)
                        {
                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                            Program.ShowScrollableMessageBox(
                                string.Format(GlobalSettings.CultureInfo,
                                    // ReSharper disable once MethodHasAsyncOverload
                                    LanguageManager.GetString("Message_SelectGeneric_PriorityRestriction", token: token),
                                    strLocalName),
                                // ReSharper disable once MethodHasAsyncOverload
                                string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("MessageTitle_SelectGeneric_Restriction", token: token), strLocalName),
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            await Program.ShowScrollableMessageBoxAsync(
                                string.Format(GlobalSettings.CultureInfo,
                                    await LanguageManager.GetStringAsync("Message_SelectGeneric_PriorityRestriction", token: token)
                                        .ConfigureAwait(false),
                                    strLocalName),
                                string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("MessageTitle_SelectGeneric_Restriction", token: token)
                                    .ConfigureAwait(false), strLocalName),
                                MessageBoxButtons.OK, MessageBoxIcon.Information, token: token).ConfigureAwait(false);
                        }
                    }

                    return false;
                }
            }

            if (!blnIgnoreLimit)
            {
                // See if the character already has this Quality and whether multiple copies are allowed.
                // If the limit at chargen is different from the actual limit, we need to make sure we fetch the former if the character is in Create mode
                string strLimitString = xmlNode.SelectSingleNodeAndCacheExpression("chargenlimit", token)?.Value ?? string.Empty;
                if (string.IsNullOrWhiteSpace(strLimitString) || objCharacter.Created)
                {
                    strLimitString = xmlNode.SelectSingleNodeAndCacheExpression("limit", token)?.Value ?? string.Empty;
                    // Default case is each quality can only be taken once
                    if (string.IsNullOrWhiteSpace(strLimitString))
                    {
                        if (xmlNode.Name == "quality" ||
                            xmlNode.Name == "martialart" ||
                            xmlNode.Name == "technique" ||
                            xmlNode.Name == "cyberware" ||
                            xmlNode.Name == "bioware")
                            strLimitString = "1";
                        else if (xmlNode.Name == "lifestylequality")
                        {
                            strLimitString = xmlNode.SelectSingleNodeAndCacheExpression("allowmultiple", token) != null
                                ? bool.FalseString
                                : "1";
                        }
                        else
                            strLimitString = bool.FalseString;
                    }
                }

                if (strLimitString != bool.FalseString)
                {
                    int intLimit;
                    if (strLimitString.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
                    {
                        if (strLimitString.HasValuesNeedingReplacementForXPathProcessing())
                        {
                            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                                      out StringBuilder sbdLimitString))
                            {
                                sbdLimitString.Append(strLimitString);
                                if (blnSync)
                                {
                                    foreach (string strLimb in Character.LimbStrings)
                                    {
                                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                        sbdLimitString.CheapReplace(strLimitString, "{" + strLimb + "}",
                                                                    () => (string.IsNullOrEmpty(strLocation)
                                                                            ? objCharacter.LimbCount(strLimb)
                                                                            : objCharacter.LimbCount(strLimb) / 2)
                                                                        .ToString(GlobalSettings.InvariantCultureInfo));
                                    }
                                    // ReSharper disable once MethodHasAsyncOverload
                                    objCharacter.ProcessAttributesInXPath(
                                        sbdLimitString, strLimitString, token: token);
                                }
                                else
                                {
                                    foreach (string strLimb in Character.LimbStrings)
                                    {
                                        await sbdLimitString.CheapReplaceAsync(strLimitString, "{" + strLimb + "}",
                                                                               () => (string.IsNullOrEmpty(strLocation)
                                                                                       ? objCharacter.LimbCount(strLimb)
                                                                                       : objCharacter.LimbCount(strLimb) / 2)
                                                                                   .ToString(
                                                                                       GlobalSettings.InvariantCultureInfo),
                                                                               token: token).ConfigureAwait(false);
                                    }

                                    await objCharacter
                                        .ProcessAttributesInXPathAsync(
                                            sbdLimitString, strLimitString, token: token).ConfigureAwait(false);
                                }
                                strLimitString = sbdLimitString.ToString();
                            }
                        }
                        if (blnSync)
                        {
                            (bool blnIsSuccess, object objProcess)
                                // ReSharper disable once MethodHasAsyncOverload
                                = CommonFunctions.EvaluateInvariantXPath(strLimitString, token);
                            intLimit = blnIsSuccess ? ((double)objProcess).StandardRound() : 1;
                        }
                        else
                        {
                            (bool blnIsSuccess, object objProcess)
                                = await CommonFunctions.EvaluateInvariantXPathAsync(strLimitString, token)
                                                       .ConfigureAwait(false);
                            intLimit = blnIsSuccess ? ((double)objProcess).StandardRound() : 1;
                        }
                    }
                    else
                        intLimit = decValue.StandardRound();

                    // We could set this to a list immediately, but I'd rather the pointer start at null so that no list ends up getting selected for the "default" case below
                    IEnumerable<IHasName> objListToCheck = null;
                    bool blnCheckCyberwareChildren = false;
                    switch (xmlNode.Name.ToUpperInvariant())
                    {
                        case "CHARACTERQUALITY":
                        case "QUALITY":
                        {
                            objListToCheck = objCharacter.Qualities.Where(objQuality =>
                                objQuality.Name != strIgnoreQuality && objQuality.SourceIDString != strIgnoreQuality);
                            break;
                        }
                        case "LIFESTYLEQUALITY":
                        {
                            objListToCheck = objParent is Lifestyle objLifestyle
                                ? objLifestyle.LifestyleQualities.Where(
                                    objQuality =>
                                        objQuality.Name != strIgnoreQuality
                                        && objQuality.SourceIDString != strIgnoreQuality)
                                : objCharacter.Lifestyles.SelectMany(x => x.LifestyleQualities).Where(
                                    objQuality =>
                                        objQuality.Name != strIgnoreQuality
                                        && objQuality.SourceIDString != strIgnoreQuality);
                            break;
                        }
                        case "ECHO":
                        case "METAMAGIC":
                        {
                            objListToCheck = objCharacter.Metamagics;
                            break;
                        }
                        case "ART":
                        {
                            objListToCheck = objCharacter.Arts;
                            break;
                        }
                        case "ENHANCEMENT":
                        {
                            objListToCheck = objCharacter.Enhancements;
                            break;
                        }
                        case "POWER":
                        {
                            objListToCheck = objCharacter.Powers;
                            break;
                        }
                        case "CRITTERPOWER":
                        {
                            objListToCheck = objCharacter.CritterPowers;
                            break;
                        }
                        case "MARTIALART":
                        {
                            objListToCheck = objCharacter.MartialArts;
                            break;
                        }
                        case "TECHNIQUE":
                        {
                            objListToCheck = objParent is MartialArt objArt
                                ? objArt.Techniques
                                : objCharacter.MartialArts.SelectMany(x => x.Children);
                            break;
                        }
                        case "CYBERWARE":
                        case "BIOWARE":
                        {
                            blnCheckCyberwareChildren = true;
                            break;
                        }
                        default:
                        {
                            Utils.BreakIfDebug();
                            break;
                        }
                    }

                    int intExtendedLimit = intLimit;
                    string strLimitWithInclusions = xmlNode.SelectSingleNodeAndCacheExpression("limitwithinclusions", token)?.Value;
                    if (!string.IsNullOrEmpty(strLimitWithInclusions))
                        int.TryParse(strLimitWithInclusions, System.Globalization.NumberStyles.Integer, GlobalSettings.InvariantCultureInfo, out intExtendedLimit);
                    int intCount = 0;
                    int intExtendedCount = 0;
                    if (objListToCheck != null || blnCheckCyberwareChildren)
                    {
                        string strNodeId = xmlNode.SelectSingleNodeAndCacheExpression("id", token)?.Value ?? string.Empty;
                        string strNodeName = xmlNode.SelectSingleNodeAndCacheExpression("name", token)?.Value ?? string.Empty;
                        intExtendedCount = intCount;
                        // In case one item is split up into multiple entries with different names, e.g. Indomitable quality, we need to be able to check all those entries against the limit
                        XPathNavigator xmlIncludeInLimit = xmlNode.SelectSingleNodeAndCacheExpression("includeinlimit", token);
                        if (xmlIncludeInLimit != null)
                        {
                            using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                            out HashSet<string>
                                                                                setNamesIncludedInLimit))
                            {
                                if (!string.IsNullOrEmpty(strNodeId))
                                {
                                    setNamesIncludedInLimit.Add(strNodeId);
                                }

                                if (!string.IsNullOrEmpty(strNodeName))
                                {
                                    setNamesIncludedInLimit.Add(strNodeName);
                                }

                                foreach (XPathNavigator objChildXml in xmlIncludeInLimit.SelectChildren(
                                             XPathNodeType.Element))
                                {
                                    setNamesIncludedInLimit.Add(objChildXml.Value);
                                }

                                if (blnCheckCyberwareChildren)
                                {
                                    foreach (Cyberware objItem in blnSync
                                                 ? objCharacter.Cyberware.GetAllDescendants(x => x.Children, token)
                                                 : await (await objCharacter.GetCyberwareAsync(token).ConfigureAwait(false))
                                                         .GetAllDescendantsAsync(x => x.GetChildrenAsync(token), token).ConfigureAwait(false))
                                    {
                                        if (!setNamesIncludedInLimit.Contains(objItem.Name)
                                            && !setNamesIncludedInLimit.Contains(objItem.InternalId))
                                            continue;
                                        if (!string.IsNullOrEmpty(strLocation) && objItem.Location != strLocation)
                                            continue;
                                        if (blnSync)
                                        {
                                            if (!string.IsNullOrEmpty(objItem.PlugsIntoModularMount)
                                                || !objItem.IsModularCurrentlyEquipped)
                                                continue;
                                        }
                                        else if (!string.IsNullOrEmpty(await objItem.GetPlugsIntoModularMountAsync(token).ConfigureAwait(false))
                                                 || !await objItem.GetIsModularCurrentlyEquippedAsync(token).ConfigureAwait(false))
                                            continue;
                                        if (strNodeName == objItem.Name || strNodeId == objItem.SourceIDString)
                                            ++intCount;
                                        ++intExtendedCount;
                                        if (!blnShowMessage
                                            && (intCount >= intLimit || intExtendedCount >= intExtendedLimit))
                                            return false;
                                    }
                                }
                                else
                                {
                                    foreach (IHasName objItem in objListToCheck)
                                    {
                                        string strItemName = objItem.Name;
                                        string strItemSourceId = string.Empty;
                                        if (objItem is IHasSourceId objItemId)
                                            strItemSourceId = objItemId.SourceIDString;
                                        if (!setNamesIncludedInLimit.Contains(strItemName)
                                            && !setNamesIncludedInLimit.Contains(strItemSourceId))
                                            continue;
                                        if (strNodeName == strItemName && strNodeId == strItemSourceId)
                                            ++intCount;
                                        ++intExtendedCount;
                                        if (!blnShowMessage
                                            && (intCount >= intLimit || intExtendedCount >= intExtendedLimit))
                                            return false;
                                    }
                                }
                            }
                        }
                        else if (blnCheckCyberwareChildren)
                        {
                            foreach (Cyberware objItem in blnSync
                                         ? objCharacter.Cyberware.GetAllDescendants(x => x.Children, token)
                                         : await (await objCharacter.GetCyberwareAsync(token).ConfigureAwait(false))
                                             .GetAllDescendantsAsync(x => x.GetChildrenAsync(token), token).ConfigureAwait(false))
                            {
                                if (strNodeName != objItem.Name && strNodeId != objItem.SourceIDString)
                                    continue;
                                if (!string.IsNullOrEmpty(strLocation) && objItem.Location != strLocation)
                                    continue;
                                if (blnSync)
                                {
                                    if (!string.IsNullOrEmpty(objItem.PlugsIntoModularMount)
                                        || !objItem.IsModularCurrentlyEquipped)
                                        continue;
                                }
                                else if (!string.IsNullOrEmpty(await objItem.GetPlugsIntoModularMountAsync(token).ConfigureAwait(false))
                                         || !await objItem.GetIsModularCurrentlyEquippedAsync(token).ConfigureAwait(false))
                                    continue;

                                ++intCount;
                                if (!blnShowMessage && intCount >= intLimit)
                                    return false;
                            }
                        }
                        else
                        {
                            foreach (IHasName objItem in objListToCheck)
                            {
                                if (strNodeName != objItem.Name && (!(objItem is IHasSourceId objItemId)
                                                                    || strNodeId != objItemId.SourceIDString))
                                    continue;
                                ++intCount;
                                if (!blnShowMessage && intCount >= intLimit)
                                    return false;
                            }
                        }
                    }

                    if (intCount >= intLimit || intExtendedCount >= intExtendedLimit)
                    {
                        if (blnShowMessage)
                        {
                            if (blnSync)
                            {
                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                Program.ShowScrollableMessageBox(
                                    string.Format(GlobalSettings.CultureInfo,
                                        // ReSharper disable once MethodHasAsyncOverload
                                        LanguageManager.GetString("Message_SelectGeneric_Limit", token: token),
                                        strLocalName, intLimit == 0 ? 1 : intLimit),
                                    // ReSharper disable once MethodHasAsyncOverload
                                    string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("MessageTitle_SelectGeneric_Limit", token: token), strLocalName),
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                await Program.ShowScrollableMessageBoxAsync(
                                    string.Format(GlobalSettings.CultureInfo,
                                        await LanguageManager.GetStringAsync("Message_SelectGeneric_Limit", token: token)
                                            .ConfigureAwait(false),
                                        strLocalName, intLimit == 0 ? 1 : intLimit),
                                    string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("MessageTitle_SelectGeneric_Limit", token: token)
                                        .ConfigureAwait(false), strLocalName),
                                    MessageBoxButtons.OK, MessageBoxIcon.Information, token: token).ConfigureAwait(false);
                            }
                        }

                        return false;
                    }
                }
            }

            XPathNavigator xmlForbiddenNode = xmlNode.SelectSingleNodeAndCacheExpression("forbidden", token);
            if (xmlForbiddenNode != null)
            {
                // Loop through the oneof requirements.
                foreach (XPathNavigator objXmlOneOf in xmlForbiddenNode.SelectAndCacheExpression("oneof", token))
                {
                    foreach (XPathNavigator xmlForbiddenItemNode in objXmlOneOf.SelectChildren(XPathNodeType.Element))
                    {
                        // The character is not allowed to take the Quality, so display a message and uncheck the item.
                        (bool blnLoopSuccess, string strName) = blnSync
                            ? Utils.SafelyRunSynchronously(
                                () => xmlForbiddenItemNode.TestNodeRequirementsAsync(
                                    true, objCharacter, objParent, strIgnoreQuality, blnShowMessage, token), token)
                            : await xmlForbiddenItemNode
                                    .TestNodeRequirementsAsync(false, objCharacter, objParent, strIgnoreQuality,
                                                               blnShowMessage, token).ConfigureAwait(false);
                        if (blnLoopSuccess)
                        {
                            if (blnShowMessage)
                            {
                                if (blnSync)
                                {
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    Program.ShowScrollableMessageBox(
                                        string.Format(GlobalSettings.CultureInfo,
                                            // ReSharper disable once MethodHasAsyncOverload
                                            LanguageManager.GetString("Message_SelectGeneric_Restriction", token: token),
                                            strLocalName) + strName,
                                        // ReSharper disable once MethodHasAsyncOverload
                                        string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("MessageTitle_SelectGeneric_Restriction", token: token), strLocalName),
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                else
                                {
                                    await Program.ShowScrollableMessageBoxAsync(
                                        string.Format(GlobalSettings.CultureInfo,
                                            await LanguageManager.GetStringAsync("Message_SelectGeneric_Restriction", token: token)
                                                .ConfigureAwait(false),
                                            strLocalName) + strName,
                                        string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("MessageTitle_SelectGeneric_Restriction", token: token)
                                            .ConfigureAwait(false), strLocalName),
                                        MessageBoxButtons.OK, MessageBoxIcon.Information, token: token).ConfigureAwait(false);
                                }
                            }

                            return false;
                        }
                    }
                }
            }

            XPathNavigator xmlRequiredNode = xmlNode.SelectSingleNodeAndCacheExpression("required", token);
            if (xmlRequiredNode != null)
            {
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                              out StringBuilder sbdRequirement))
                {
                    bool blnRequirementMet = true;

                    // Loop through the oneof requirements.
                    foreach (XPathNavigator objXmlOneOf in xmlRequiredNode.SelectAndCacheExpression("oneof", token))
                    {
                        bool blnOneOfMet = false;
                        using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                                      out StringBuilder sbdThisRequirement))
                        {
                            sbdThisRequirement.AppendLine()
                                              .Append(blnSync
                                                          // ReSharper disable once MethodHasAsyncOverload
                                                          ? LanguageManager.GetString(
                                                              "Message_SelectQuality_OneOf", token: token)
                                                          : await LanguageManager.GetStringAsync(
                                                                  "Message_SelectQuality_OneOf", token: token)
                                                              .ConfigureAwait(false));
                            foreach (XPathNavigator xmlRequiredItemNode in
                                     objXmlOneOf.SelectChildren(XPathNodeType.Element))
                            {
                                (bool blnLoopSuccess, string strName) = blnSync
                                    ? Utils.SafelyRunSynchronously(
                                        () => xmlRequiredItemNode.TestNodeRequirementsAsync(
                                            true, objCharacter, objParent, strIgnoreQuality, blnShowMessage, token), token)
                                    : await xmlRequiredItemNode
                                            .TestNodeRequirementsAsync(false, objCharacter, objParent, strIgnoreQuality,
                                                                       blnShowMessage, token).ConfigureAwait(false);
                                if (blnLoopSuccess)
                                {
                                    blnOneOfMet = true;
                                    if (!blnShowMessage)
                                        break;
                                }

                                if (blnShowMessage)
                                    sbdThisRequirement.Append(strName);
                            }

                            // Update the flag for requirements met.
                            if (!blnOneOfMet)
                            {
                                blnRequirementMet = false;
                                if (blnShowMessage)
                                    sbdRequirement.Append(sbdThisRequirement);
                            }
                        }

                        if (!blnRequirementMet && !blnShowMessage)
                            break;
                    }

                    if (blnRequirementMet || blnShowMessage)
                    {
                        // Loop through the allof requirements.
                        foreach (XPathNavigator objXmlAllOf in xmlRequiredNode.SelectAndCacheExpression("allof", token))
                        {
                            bool blnAllOfMet = true;
                            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                                          out StringBuilder sbdThisRequirement))
                            {
                                sbdThisRequirement.AppendLine()
                                                  .Append(blnSync
                                                              // ReSharper disable once MethodHasAsyncOverload
                                                              ? LanguageManager.GetString(
                                                                  "Message_SelectQuality_AllOf", token: token)
                                                              : await LanguageManager.GetStringAsync(
                                                                      "Message_SelectQuality_AllOf", token: token)
                                                                  .ConfigureAwait(false));
                                foreach (XPathNavigator xmlRequiredItemNode in objXmlAllOf.SelectChildren(
                                             XPathNodeType.Element))
                                {
                                    // If this item was not found, fail the AllOfMet condition.
                                    (bool blnLoopSuccess, string strName) = blnSync
                                        ? Utils.SafelyRunSynchronously(
                                            () => xmlRequiredItemNode.TestNodeRequirementsAsync(
                                                true, objCharacter, objParent, strIgnoreQuality, blnShowMessage, token), token)
                                        : await xmlRequiredItemNode
                                                .TestNodeRequirementsAsync(false, objCharacter, objParent, strIgnoreQuality,
                                                                           blnShowMessage, token).ConfigureAwait(false);
                                    if (!blnLoopSuccess)
                                    {
                                        blnAllOfMet = false;
                                        if (blnShowMessage)
                                            sbdThisRequirement.Append(strName);
                                        else
                                            break;
                                    }
                                }

                                // Update the flag for requirements met.
                                if (!blnAllOfMet)
                                {
                                    blnRequirementMet = false;
                                    if (blnShowMessage)
                                        sbdRequirement.Append(sbdThisRequirement);
                                }
                            }

                            if (!blnRequirementMet && !blnShowMessage)
                                break;
                        }
                    }

                    // The character has not met the requirements, so display a message and uncheck the item.
                    if (!blnRequirementMet)
                    {
                        if (blnShowMessage)
                        {
                            if (blnSync)
                            {
                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                Program.ShowScrollableMessageBox(
                                    string.Format(GlobalSettings.CultureInfo,
                                        // ReSharper disable once MethodHasAsyncOverload
                                        LanguageManager.GetString("Message_SelectGeneric_Restriction", token: token),
                                        strLocalName) + sbdRequirement.ToString(),
                                    // ReSharper disable once MethodHasAsyncOverload
                                    string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("MessageTitle_SelectGeneric_Restriction", token: token), strLocalName),
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                await Program.ShowScrollableMessageBoxAsync(
                                    string.Format(GlobalSettings.CultureInfo,
                                        await LanguageManager.GetStringAsync("Message_SelectGeneric_Restriction", token: token)
                                            .ConfigureAwait(false),
                                        strLocalName) + sbdRequirement.ToString(),
                                    string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("MessageTitle_SelectGeneric_Restriction", token: token)
                                        .ConfigureAwait(false), strLocalName),
                                    MessageBoxButtons.OK, MessageBoxIcon.Information, token: token).ConfigureAwait(false);
                            }
                        }

                        return false;
                    }
                }
            }

            return true;
        }

        private static async Task<ValueTuple<bool, string>> TestNodeRequirementsAsync(this XPathNavigator xmlNode,
            bool blnSync, Character objCharacter,
            object objParent, string strIgnoreQuality = "",
            bool blnShowMessage = true, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (xmlNode == null || objCharacter == null)
            {
                return new ValueTuple<bool, string>(false, string.Empty);
            }

            string strName = string.Empty;
            string strSpace = blnSync
                // ReSharper disable once MethodHasAsyncOverload
                ? LanguageManager.GetString("String_Space", token: token)
                : await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);
            string strNodeInnerText = xmlNode.Value;
            string strNodeId
                = xmlNode.SelectSingleNodeAndCacheExpression("id", token)?.Value ?? string.Empty;
            string strNodeName
                = xmlNode.SelectSingleNodeAndCacheExpression("name", token)?.Value ?? string.Empty;
            switch (xmlNode.Name)
            {
                case "attribute":
                {
                    if (blnSync)
                    {
                        // ReSharper disable MethodHasAsyncOverload
                        // Check to see if an Attribute meets a requirement.
                        CharacterAttrib objAttribute = objCharacter.GetAttribute(strNodeName, token: token);
                        int intTargetValue =
                            xmlNode.SelectSingleNodeAndCacheExpression("total", token)?.ValueAsInt ?? 0;
                        if (blnShowMessage)
                            strName = string.Format(GlobalSettings.CultureInfo, "{0}\t{1}{2}{3}", Environment.NewLine,
                                objAttribute?.CurrentDisplayAbbrev ?? objCharacter.TranslateExtra(strNodeName, token: token),
                                strSpace, intTargetValue);

                        if (xmlNode.SelectSingleNodeAndCacheExpression("natural", token) != null)
                        {
                            return new ValueTuple<bool, string>((objAttribute?.Value ?? 0) >= intTargetValue, strName);
                        }

                        return new ValueTuple<bool, string>((objAttribute?.TotalValue ?? 0) >= intTargetValue, strName);
                        // ReSharper restore MethodHasAsyncOverload
                    }
                    else
                    {
                        // Check to see if an Attribute meets a requirement.
                        CharacterAttrib objAttribute = await objCharacter.GetAttributeAsync(strNodeName, token: token)
                            .ConfigureAwait(false);
                        int intTargetValue
                            = xmlNode.SelectSingleNodeAndCacheExpression("total", token)?.ValueAsInt ?? 0;
                        if (blnShowMessage)
                            strName = string.Format(GlobalSettings.CultureInfo, "{0}\t{1}{2}{3}", Environment.NewLine,
                                objAttribute != null
                                    ? await objAttribute.GetCurrentDisplayAbbrevAsync(token)
                                        .ConfigureAwait(false)
                                    : await objCharacter.TranslateExtraAsync(strNodeName, token: token)
                                        .ConfigureAwait(false), strSpace, intTargetValue);

                        if (xmlNode.SelectSingleNodeAndCacheExpression("natural", token) != null)
                        {
                            return new ValueTuple<bool, string>(
                                (objAttribute != null
                                    ? await objAttribute.GetValueAsync(token).ConfigureAwait(false)
                                    : 0) >= intTargetValue, strName);
                        }

                        return new ValueTuple<bool, string>(
                            (objAttribute != null
                                ? await objAttribute.GetTotalValueAsync(token).ConfigureAwait(false)
                                : 0) >= intTargetValue, strName);
                    }
                }
                case "attributetotal":
                {
                    if (blnSync)
                    {
                        // ReSharper disable MethodHasAsyncOverload
                        string strNodeAttributes
                            = xmlNode.SelectSingleNodeAndCacheExpression("attributes", token)?.Value ?? string.Empty;
                        int intNodeVal = xmlNode.SelectSingleNodeAndCacheExpression("val", token)?.ValueAsInt ?? 0;
                        if (strNodeAttributes.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
                        {
                            // Check if the character's Attributes add up to a particular total.
                            string strValue = strNodeAttributes;
                            if (strValue.HasValuesNeedingReplacementForXPathProcessing())
                            {
                                strValue = objCharacter.ProcessAttributesInXPath(strValue, token: token);
                                if (blnShowMessage)
                                    strName = string.Format(GlobalSettings.CultureInfo, "{0}\t{2}{1}{3}", Environment.NewLine,
                                        strSpace,
                                        objCharacter.ProcessAttributesInXPathForTooltip(
                                            strNodeAttributes,
                                            blnShowValues: false, token: token), intNodeVal);
                            }
                            else if(blnShowMessage)
                                strName = string.Format(GlobalSettings.CultureInfo, "{0}\t{2}{1}{3}", Environment.NewLine,
                                    strSpace, strValue, intNodeVal);
                            (bool blnIsSuccess, object objProcess)
                                = CommonFunctions.EvaluateInvariantXPath(strValue, token);
                            return new ValueTuple<bool, string>(
                                (blnIsSuccess ? ((double)objProcess).StandardRound() : 0) >= intNodeVal, strName);
                        }
                        else if (blnShowMessage)
                            strName = string.Format(GlobalSettings.CultureInfo, "{0}\t{2}{1}{3}", Environment.NewLine,
                                strSpace, decValue, intNodeVal);
                        return new ValueTuple<bool, string>(decValue >= intNodeVal, strName);
                        // ReSharper restore MethodHasAsyncOverload
                    }
                    else
                    {
                        string strNodeAttributes
                            = xmlNode.SelectSingleNodeAndCacheExpression("attributes", token)?.Value ?? string.Empty;
                        int intNodeVal = xmlNode.SelectSingleNodeAndCacheExpression("val", token)?.ValueAsInt ?? 0;
                        if (strNodeAttributes.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
                        {
                            string strValue = strNodeAttributes;
                            if (strValue.HasValuesNeedingReplacementForXPathProcessing())
                            {
                                // Check if the character's Attributes add up to a particular total.
                                strValue
                                    = await objCharacter.ProcessAttributesInXPathAsync(strNodeAttributes, token: token)
                                        .ConfigureAwait(false);
                                if (blnShowMessage)
                                    strName = string.Format(GlobalSettings.CultureInfo, "{0}\t{2}{1}{3}", Environment.NewLine,
                                        strSpace,
                                        await objCharacter.ProcessAttributesInXPathForTooltipAsync(
                                            strNodeAttributes,
                                            blnShowValues: false, token: token).ConfigureAwait(false), intNodeVal);
                            }
                            else if (blnShowMessage)
                            {
                                strName = string.Format(GlobalSettings.CultureInfo, "{0}\t{2}{1}{3}", Environment.NewLine,
                                    strSpace,
                                    strValue, intNodeVal);
                            }
                            (bool blnIsSuccess, object objProcess)
                                    = await CommonFunctions.EvaluateInvariantXPathAsync(strValue, token).ConfigureAwait(false);
                            return new ValueTuple<bool, string>(
                                (blnIsSuccess ? ((double)objProcess).StandardRound() : 0) >= intNodeVal, strName);
                        }
                        else if (blnShowMessage)
                            strName = string.Format(GlobalSettings.CultureInfo, "{0}\t{2}{1}{3}", Environment.NewLine,
                                strSpace, decValue, intNodeVal);
                        return new ValueTuple<bool, string>(decValue >= intNodeVal, strName);
                    }
                }
                case "careerkarma":
                {
                    // Check Career Karma requirement.
                    if (blnShowMessage)
                        strName = Environment.NewLine + "\t" + string.Format(
                            GlobalSettings.CultureInfo, blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? LanguageManager.GetString("Message_SelectQuality_RequireKarma", token: token)
                                : await LanguageManager.GetStringAsync("Message_SelectQuality_RequireKarma",
                                    token: token).ConfigureAwait(false),
                            strNodeInnerText);
                    return new ValueTuple<bool, string>((blnSync
                        ? objCharacter.CareerKarma
                        : await objCharacter.GetCareerKarmaAsync(token).ConfigureAwait(false)) >= xmlNode.ValueAsInt, strName);
                }
                case "chargenonly":
                {
                    if (blnShowMessage)
                        strName = Environment.NewLine + "\t"
                                                      + (blnSync
                                                          // ReSharper disable once MethodHasAsyncOverload
                                                          ? LanguageManager.GetString(
                                                              "Message_SelectGeneric_ChargenRestriction", token: token)
                                                          : await LanguageManager.GetStringAsync(
                                                              "Message_SelectGeneric_ChargenRestriction",
                                                              token: token).ConfigureAwait(false));
                    return new ValueTuple<bool, string>(
                        !(blnSync
                            ? objCharacter.Created
                            : await objCharacter.GetCreatedAsync(token).ConfigureAwait(false)), strName);
                }
                case "careeronly":
                {
                    if (blnShowMessage)
                        strName = Environment.NewLine + "\t"
                                                      + (blnSync
                                                          // ReSharper disable once MethodHasAsyncOverload
                                                          ? LanguageManager.GetString(
                                                              "Message_SelectGeneric_CareerOnlyRestriction",
                                                              token: token)
                                                          : await LanguageManager.GetStringAsync(
                                                              "Message_SelectGeneric_CareerOnlyRestriction",
                                                              token: token).ConfigureAwait(false));
                    return new ValueTuple<bool, string>(
                        blnSync
                            ? objCharacter.Created
                            : await objCharacter.GetCreatedAsync(token).ConfigureAwait(false), strName);
                }
                case "critterpower":
                {
                    // Run through all of the Powers the character has and see if the current required item exists.
                    if (blnSync
                            ? objCharacter.CritterEnabled
                            : await objCharacter.GetCritterEnabledAsync(token).ConfigureAwait(false))
                    {
                        CritterPower objCritterPower = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? objCharacter.CritterPowers.FirstOrDefault(
                                p => MatchesNameOrSourceId(p.Name, p.SourceIDString, strNodeInnerText))
                            : await (await objCharacter.GetCritterPowersAsync(token).ConfigureAwait(false))
                                .FirstOrDefaultAsync(
                                    p => MatchesNameOrSourceId(p.Name, p.SourceIDString, strNodeInnerText), token)
                                .ConfigureAwait(false);
                        if (objCritterPower != null)
                        {
                            if (blnShowMessage)
                                strName = blnSync
                                    ? objCritterPower.CurrentDisplayName
                                    : await objCritterPower.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                            return new ValueTuple<bool, string>(true, strName);
                        }
                    }

                    if (blnShowMessage)
                    {
                        strName = await BuildDataItemRequirementRestrictionMessageAsync(blnSync, objCharacter, strNodeInnerText,
                            strSpace, "critterpowers.xml", "powers/power", "Tab_Critter", token).ConfigureAwait(false);
                    }

                    return new ValueTuple<bool, string>(false, strName);
                }
                case "bioware":
                    return blnSync
                        ? Utils.SafelyRunSynchronously(
                            () => TestInstalledWareRequirementCoreAsync(true, xmlNode, objCharacter, objParent,
                                Improvement.ImprovementSource.Bioware, WareMatchMode.ExactNameOrId, "Label_Bioware",
                                "bioware.xml", "biowares/bioware", false, strNodeInnerText, strSpace, blnShowMessage,
                                token), token)
                        : await TestInstalledWareRequirementCoreAsync(false, xmlNode, objCharacter, objParent,
                            Improvement.ImprovementSource.Bioware, WareMatchMode.ExactNameOrId, "Label_Bioware",
                            "bioware.xml", "biowares/bioware", false, strNodeInnerText, strSpace, blnShowMessage,
                            token).ConfigureAwait(false);
                case "cyberware":
                    return blnSync
                        ? Utils.SafelyRunSynchronously(
                            () => TestInstalledWareRequirementCoreAsync(true, xmlNode, objCharacter, objParent,
                                Improvement.ImprovementSource.Cyberware, WareMatchMode.ExactNameOrId,
                                "Label_Cyberware", "cyberware.xml", "cyberwares/cyberware", true, strNodeInnerText,
                                strSpace, blnShowMessage, token), token)
                        : await TestInstalledWareRequirementCoreAsync(false, xmlNode, objCharacter, objParent,
                            Improvement.ImprovementSource.Cyberware, WareMatchMode.ExactNameOrId, "Label_Cyberware",
                            "cyberware.xml", "cyberwares/cyberware", true, strNodeInnerText, strSpace, blnShowMessage,
                            token).ConfigureAwait(false);
                case "biowarecategory":
                    return blnSync
                        ? Utils.SafelyRunSynchronously(
                            () => TestInstalledWareRequirementCoreAsync(true, xmlNode, objCharacter, objParent,
                                Improvement.ImprovementSource.Bioware, WareMatchMode.Category, "Label_Bioware",
                                "bioware.xml", "biowares/bioware", true, strNodeInnerText, strSpace, blnShowMessage,
                                token), token)
                        : await TestInstalledWareRequirementCoreAsync(false, xmlNode, objCharacter, objParent,
                            Improvement.ImprovementSource.Bioware, WareMatchMode.Category, "Label_Bioware",
                            "bioware.xml", "biowares/bioware", true, strNodeInnerText, strSpace, blnShowMessage,
                            token).ConfigureAwait(false);
                case "cyberwarecategory":
                    return blnSync
                        ? Utils.SafelyRunSynchronously(
                            () => TestInstalledWareRequirementCoreAsync(true, xmlNode, objCharacter, objParent,
                                Improvement.ImprovementSource.Cyberware, WareMatchMode.Category, "Label_Cyberware",
                                "cyberware.xml", "cyberwares/cyberware", true, strNodeInnerText, strSpace,
                                blnShowMessage, token), token)
                        : await TestInstalledWareRequirementCoreAsync(false, xmlNode, objCharacter, objParent,
                            Improvement.ImprovementSource.Cyberware, WareMatchMode.Category, "Label_Cyberware",
                            "cyberware.xml", "cyberwares/cyberware", true, strNodeInnerText, strSpace, blnShowMessage,
                            token).ConfigureAwait(false);
                case "biowarecontains":
                    return blnSync
                        ? Utils.SafelyRunSynchronously(
                            () => TestInstalledWareRequirementCoreAsync(true, xmlNode, objCharacter, objParent,
                                Improvement.ImprovementSource.Bioware, WareMatchMode.NameContains, "Label_Bioware",
                                "bioware.xml", "biowares/bioware", false, strNodeInnerText, strSpace, blnShowMessage,
                                token), token)
                        : await TestInstalledWareRequirementCoreAsync(false, xmlNode, objCharacter, objParent,
                            Improvement.ImprovementSource.Bioware, WareMatchMode.NameContains, "Label_Bioware",
                            "bioware.xml", "biowares/bioware", false, strNodeInnerText, strSpace, blnShowMessage,
                            token).ConfigureAwait(false);
                case "cyberwarecontains":
                    return blnSync
                        ? Utils.SafelyRunSynchronously(
                            () => TestInstalledWareRequirementCoreAsync(true, xmlNode, objCharacter, objParent,
                                Improvement.ImprovementSource.Cyberware, WareMatchMode.NameContains,
                                "Label_Cyberware", "cyberware.xml", "cyberwares/cyberware", false, strNodeInnerText,
                                strSpace, blnShowMessage, token), token)
                        : await TestInstalledWareRequirementCoreAsync(false, xmlNode, objCharacter, objParent,
                            Improvement.ImprovementSource.Cyberware, WareMatchMode.NameContains, "Label_Cyberware",
                            "cyberware.xml", "cyberwares/cyberware", false, strNodeInnerText, strSpace, blnShowMessage,
                            token).ConfigureAwait(false);
                case "damageresistance":
                {
                    // Damage Resistance must be a particular value.
                    if (blnShowMessage)
                        strName = Environment.NewLine + "\t" + (blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? LanguageManager.GetString(
                                "String_DamageResistance",
                                token: token)
                            : await LanguageManager.GetStringAsync(
                                "String_DamageResistance",
                                token: token).ConfigureAwait(false));
                    int intDR = blnSync
                        ? objCharacter.BOD.TotalValue
                          // ReSharper disable once MethodHasAsyncOverload
                          + ImprovementManager.ValueOf(objCharacter, Improvement.ImprovementType.DamageResistance,
                              token: token).StandardRound()
                        : await (await objCharacter.GetAttributeAsync("BOD", token: token).ConfigureAwait(false))
                              .GetTotalValueAsync(token).ConfigureAwait(false)
                          + (await ImprovementManager.ValueOfAsync(objCharacter,
                              Improvement.ImprovementType.DamageResistance,
                              token: token).ConfigureAwait(false)).StandardRound();
                    return new ValueTuple<bool, string>(intDR >= xmlNode.ValueAsInt, strName);
                }
                case "depenabled":
                    // Character must be an AI.
                    if (blnShowMessage)
                        strName = Environment.NewLine + "\t" + string.Format(GlobalSettings.CultureInfo, blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? LanguageManager.GetString(
                                    "Message_SelectGeneric_HaveAttributeEnabled",
                                    token: token)
                                : await LanguageManager.GetStringAsync(
                                    "Message_SelectGeneric_HaveAttributeEnabled",
                                    token: token).ConfigureAwait(false),
                            blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? LanguageManager.GetString(
                                    "String_AttributeDEPLong",
                                    token: token)
                                : await LanguageManager.GetStringAsync(
                                    "String_AttributeDEPLong",
                                    token: token).ConfigureAwait(false));
                    return new ValueTuple<bool, string>(
                        blnSync
                            ? objCharacter.DEPEnabled
                            : await objCharacter.GetDEPEnabledAsync(token).ConfigureAwait(false), strName);

                case "ess":
                {
                    string strEssNodeGradeAttributeText
                        = xmlNode.SelectSingleNodeAndCacheExpression("@grade", token)?.Value
                          ?? string.Empty;
                    if (!string.IsNullOrEmpty(strEssNodeGradeAttributeText))
                    {
                        decimal decGrade;
                        using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool,
                                   out HashSet<string>
                                       setEssNodeGradeAttributeText))
                        {
                            setEssNodeGradeAttributeText.AddRange(
                                strEssNodeGradeAttributeText.SplitNoAlloc(
                                    ',', StringSplitOptions.RemoveEmptyEntries));
                            decGrade = blnSync
                                ? objCharacter.Cyberware.Sum(
                                    x => x.Grade.Name.ContainsAny(setEssNodeGradeAttributeText), x => x.CalculatedESS,
                                    token)
                                : await (await objCharacter.GetCyberwareAsync(token).ConfigureAwait(false)).SumAsync(
                                    async x =>
                                        (await x.GetGradeAsync(token).ConfigureAwait(false)).Name.ContainsAny(
                                            setEssNodeGradeAttributeText),
                                    x => x.GetCalculatedESSAsync(token), token).ConfigureAwait(false);
                        }

                        if (strNodeInnerText.StartsWith('-'))
                        {
                            // Essence must be less than the value.
                            if (blnShowMessage)
                                strName = Environment.NewLine + "\t" +
                                          string.Format(GlobalSettings.CultureInfo, blnSync
                                                  // ReSharper disable once MethodHasAsyncOverload
                                                  ? LanguageManager.GetString(
                                                      "Message_SelectQuality_RequireESSGradeBelow",
                                                      token: token)
                                                  : await LanguageManager.GetStringAsync(
                                                      "Message_SelectQuality_RequireESSGradeBelow",
                                                      token: token).ConfigureAwait(false), strNodeInnerText,
                                              strEssNodeGradeAttributeText,
                                              decGrade.ToString(GlobalSettings.CultureInfo));
                            decimal.TryParse(strNodeInnerText.TrimStart('-'), System.Globalization.NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decimal decThreshold1);
                            return new ValueTuple<bool, string>(decGrade < decThreshold1, strName);
                        }

                        // Essence must be equal to or greater than the value.
                        if (blnShowMessage)
                            strName = Environment.NewLine + "\t" +
                                      string.Format(GlobalSettings.CultureInfo, blnSync
                                              // ReSharper disable once MethodHasAsyncOverload
                                              ? LanguageManager.GetString(
                                                  "Message_SelectQuality_RequireESSAbove",
                                                  token: token)
                                              : await LanguageManager.GetStringAsync(
                                                  "Message_SelectQuality_RequireESSAbove",
                                                  token: token).ConfigureAwait(false), strNodeInnerText,
                                          strEssNodeGradeAttributeText, decGrade.ToString(GlobalSettings.CultureInfo));
                        decimal.TryParse(strNodeInnerText, System.Globalization.NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decimal decThreshold2);
                        return new ValueTuple<bool, string>(decGrade >= decThreshold2, strName);
                    }

                    decimal decEssence = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? objCharacter.Essence(token: token)
                        : await objCharacter.EssenceAsync(token: token).ConfigureAwait(false);

                    // Check Essence requirement.
                    if (strNodeInnerText.StartsWith('-'))
                    {
                        // Essence must be less than the value.
                        if (blnShowMessage)
                            strName = Environment.NewLine + "\t" +
                                      string.Format(GlobalSettings.CultureInfo, blnSync
                                              // ReSharper disable once MethodHasAsyncOverload
                                              ? LanguageManager.GetString(
                                                  "Message_SelectQuality_RequireESSBelow",
                                                  token: token)
                                              : await LanguageManager.GetStringAsync(
                                                  "Message_SelectQuality_RequireESSBelow",
                                                  token: token).ConfigureAwait(false), strNodeInnerText,
                                          decEssence.ToString(GlobalSettings.CultureInfo));
                        decimal.TryParse(strNodeInnerText.TrimStart('-'), System.Globalization.NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decimal decThreshold3);
                        return new ValueTuple<bool, string>(decEssence < decThreshold3, strName);
                    }

                    // Essence must be equal to or greater than the value.
                    if (blnShowMessage)
                        strName = Environment.NewLine + "\t" +
                                  string.Format(GlobalSettings.CultureInfo, blnSync
                                          // ReSharper disable once MethodHasAsyncOverload
                                          ? LanguageManager.GetString(
                                              "Message_SelectQuality_RequireESSAbove",
                                              token: token)
                                          : await LanguageManager.GetStringAsync(
                                              "Message_SelectQuality_RequireESSAbove",
                                              token: token).ConfigureAwait(false), strNodeInnerText,
                                      decEssence.ToString(GlobalSettings.CultureInfo));
                    decimal.TryParse(strNodeInnerText, System.Globalization.NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decimal decThreshold4);
                    return new ValueTuple<bool, string>(decEssence >= decThreshold4, strName);
                }
                case "echo":
                {
                    Metamagic objMetamagic = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? objCharacter.Metamagics.FirstOrDefault(
                            x => MatchesNameOrSourceId(x.Name, x.SourceIDString, strNodeInnerText)
                                 && x.SourceType == Improvement.ImprovementSource.Echo)
                        : await (await objCharacter.GetMetamagicsAsync(token).ConfigureAwait(false))
                            .FirstOrDefaultAsync(
                                x => MatchesNameOrSourceId(x.Name, x.SourceIDString, strNodeInnerText)
                                     && x.SourceType == Improvement.ImprovementSource.Echo,
                                token).ConfigureAwait(false);
                    if (objMetamagic != null)
                    {
                        if (blnShowMessage)
                            strName = blnSync
                                ? objMetamagic.CurrentDisplayName
                                : await objMetamagic.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                        return new ValueTuple<bool, string>(true, strName);
                    }

                    if (blnShowMessage)
                    {
                        strName = await BuildDataItemRequirementRestrictionMessageAsync(blnSync, objCharacter, strNodeInnerText,
                            strSpace, "echoes.xml", "echoes/echo", "String_Echo", token).ConfigureAwait(false);
                    }

                    return new ValueTuple<bool, string>(false, strName);
                }
                case "setting":
                case "gameplayoption":
                {
                    // A particular gameplay option is required.
                    if (blnShowMessage)
                        strName = Environment.NewLine + "\t" + (blnSync
                                      // ReSharper disable once MethodHasAsyncOverload
                                      ? LanguageManager.GetString(
                                          "String_GameplayOption",
                                          token: token)
                                      : await LanguageManager.GetStringAsync(
                                          "String_GameplayOption",
                                          token: token).ConfigureAwait(false))
                                  + strSpace + "=" + strSpace + strNodeInnerText;
                    // Check gameplay option name
                    CharacterSettings objSettings = blnSync
                        ? objCharacter.Settings
                        : await objCharacter.GetSettingsAsync(token).ConfigureAwait(false);
                    string strGameplayOptionName = blnSync
                        ? objSettings.GameplayOptionName
                        : await objSettings.GetGameplayOptionNameAsync(token).ConfigureAwait(false);
                    bool blnResult = strGameplayOptionName == strNodeInnerText;
                    return new ValueTuple<bool, string>(blnResult, strName);
                }
                case "gear":
                    return blnSync
                        ? Utils.SafelyRunSynchronously(
                            () => TestInstalledDataItemRequirementCoreAsync(true, xmlNode, objCharacter, objParent,
                                InstalledDataItemRequirementKind.Gear, strNodeInnerText, strSpace, blnShowMessage,
                                "gear.xml", "gears/gear", "String_Gear", token), token)
                        : await TestInstalledDataItemRequirementCoreAsync(false, xmlNode, objCharacter, objParent,
                            InstalledDataItemRequirementKind.Gear, strNodeInnerText, strSpace, blnShowMessage,
                            "gear.xml", "gears/gear", "String_Gear", token).ConfigureAwait(false);
                case "group":
                    return blnSync
                        ? Utils.SafelyRunSynchronously(
                            () => TestGroupedRequirementsCoreAsync(true, xmlNode, objCharacter, objParent,
                                strIgnoreQuality, RequirementGroupMode.AllOf, blnShowMessage, token), token)
                        : await TestGroupedRequirementsCoreAsync(false, xmlNode, objCharacter, objParent,
                            strIgnoreQuality, RequirementGroupMode.AllOf, blnShowMessage, token).ConfigureAwait(false);
                case "grouponeof":
                    return blnSync
                        ? Utils.SafelyRunSynchronously(
                            () => TestGroupedRequirementsCoreAsync(true, xmlNode, objCharacter, objParent,
                                strIgnoreQuality, RequirementGroupMode.OneOf, blnShowMessage, token), token)
                        : await TestGroupedRequirementsCoreAsync(false, xmlNode, objCharacter, objParent,
                            strIgnoreQuality, RequirementGroupMode.OneOf, blnShowMessage, token).ConfigureAwait(false);
                case "initiategrade":
                {
                    // Character's initiate grade must be higher than or equal to the required value.
                    if (blnShowMessage)
                        strName = Environment.NewLine + "\t" + (blnSync
                                      // ReSharper disable once MethodHasAsyncOverload
                                      ? LanguageManager.GetString("String_InitiateGrade", token: token)
                                      : await LanguageManager.GetStringAsync("String_InitiateGrade", token: token).ConfigureAwait(false))
                                  + strSpace + "≥" + strSpace + strNodeInnerText;
                    int.TryParse(strNodeInnerText, System.Globalization.NumberStyles.Integer, GlobalSettings.InvariantCultureInfo, out int intGrade);
                    return new ValueTuple<bool, string>(
                        (blnSync
                            ? objCharacter.InitiateGrade
                            : await objCharacter.GetInitiateGradeAsync(token).ConfigureAwait(false))
                        >= intGrade, strName);
                }
                case "martialart":
                {
                    MartialArt objMartialArt = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? objCharacter.MartialArts.FirstOrDefault(
                            x => MatchesNameOrSourceId(x.Name, x.SourceIDString, strNodeInnerText))
                        : await (await objCharacter.GetMartialArtsAsync(token).ConfigureAwait(false))
                            .FirstOrDefaultAsync(
                                x => MatchesNameOrSourceId(x.Name, x.SourceIDString, strNodeInnerText), token)
                            .ConfigureAwait(false);
                    if (objMartialArt != null)
                    {
                        if (blnShowMessage)
                            strName = blnSync
                                ? objMartialArt.CurrentDisplayName
                                : await objMartialArt.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                        return new ValueTuple<bool, string>(true, strName);
                    }

                    if (blnShowMessage)
                    {
                        strName = await BuildDataItemRequirementRestrictionMessageAsync(blnSync, objCharacter, strNodeInnerText,
                            strSpace, "martialarts.xml", "martialarts/martialart", "String_MartialArt", token)
                            .ConfigureAwait(false);
                    }

                    return new ValueTuple<bool, string>(false, strName);
                }
                case "martialtechnique":
                {
                    MartialArtTechnique objMartialArtTechnique = null;
                    if (blnSync)
                    {
                        objMartialArtTechnique = objCharacter.MartialArts.SelectMany(x => x.Techniques)
                            .FirstOrDefault(
                                x => MatchesNameOrSourceId(x.Name, x.SourceIDString, strNodeInnerText));
                    }
                    else
                    {
                        await (await objCharacter.GetMartialArtsAsync(token).ConfigureAwait(false))
                            .ForEachWithBreakAsync(async x =>
                            {
                                MartialArtTechnique objLoopTechnique
                                    = await x.Techniques.FirstOrDefaultAsync(
                                            y => MatchesNameOrSourceId(y.Name, y.SourceIDString, strNodeInnerText),
                                            token)
                                        .ConfigureAwait(false);
                                if (objLoopTechnique != null)
                                {
                                    objMartialArtTechnique = objLoopTechnique;
                                    return false;
                                }

                                return true;
                            }, token).ConfigureAwait(false);
                    }

                    if (objMartialArtTechnique != null)
                    {
                        if (blnShowMessage)
                            strName = blnSync
                                ? objMartialArtTechnique.CurrentDisplayName
                                : await objMartialArtTechnique.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                        return new ValueTuple<bool, string>(true, strName);
                    }

                    if (blnShowMessage)
                    {
                        strName = await BuildDataItemRequirementRestrictionMessageAsync(blnSync, objCharacter, strNodeInnerText,
                            strSpace, "martialarts.xml", "techniques/technique",
                            "Label_Options_MartialArtTechnique", token).ConfigureAwait(false);
                    }

                    return new ValueTuple<bool, string>(false, strName);
                }
                case "metamagic":
                {
                    Metamagic objMetamagic = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? objCharacter.Metamagics.FirstOrDefault(
                            x => MatchesNameOrSourceId(x.Name, x.SourceIDString, strNodeInnerText))
                        : await (await objCharacter.GetMetamagicsAsync(token).ConfigureAwait(false))
                            .FirstOrDefaultAsync(
                                x => MatchesNameOrSourceId(x.Name, x.SourceIDString, strNodeInnerText), token)
                            .ConfigureAwait(false);
                    if (objMetamagic != null)
                    {
                        if (blnShowMessage)
                            strName = blnSync
                                ? objMetamagic.CurrentDisplayName
                                : await objMetamagic.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                        return new ValueTuple<bool, string>(true, strName);
                    }

                    if (blnShowMessage)
                    {
                        strName = await BuildDataItemRequirementRestrictionMessageAsync(blnSync, objCharacter, strNodeInnerText,
                            strSpace, "metamagic.xml", "metamagics/metamagic", "String_Metamagic", token)
                            .ConfigureAwait(false);
                    }

                    return new ValueTuple<bool, string>(false, strName);
                }
                case "metamagicart":
                case "art":
                {
                    // Street Grimoire adds High Arts, which group metamagics and such together. If we're ignoring this requirement
                    if (objCharacter.Settings.IgnoreArt)
                    {
                        // If we're looking for an art, return true.
                        if (xmlNode.Name == "art")
                        {
                            return new ValueTuple<bool, string>(true, strName);
                        }

                        XPathNavigator xmlMetamagicDoc
                            = (blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? objCharacter.LoadDataXPath("metamagic.xml", token: token)
                                : await objCharacter.LoadDataXPathAsync("metamagic.xml", token: token)
                                    .ConfigureAwait(false)).SelectSingleNodeAndCacheExpression("/chummer", token);
                        if (blnShowMessage)
                        {
                            strName = await BuildDataItemRequirementRestrictionMessageAsync(blnSync, objCharacter,
                                strNodeInnerText, strSpace, "metamagic.xml", "arts/art", "String_Art", token)
                                .ConfigureAwait(false);
                        }

                        if (xmlMetamagicDoc == null) return new ValueTuple<bool, string>(true, strName);
                        string strNodeInnerTextCleaned = strNodeInnerText.CleanXPath();
                        // Loop through the data file for each metamagic to find the Required and Forbidden nodes.
                        foreach (Metamagic objMetamagic in objCharacter.Metamagics)
                        {
                            XPathNavigator xmlMetamagicNode = blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? objMetamagic.GetNodeXPath(token)
                                : await objMetamagic.GetNodeXPathAsync(token).ConfigureAwait(false);
                            if (xmlMetamagicNode?.SelectSingleNode(
                                    "forbidden/art[. = " + strNodeInnerTextCleaned + "]") != null)
                            {
                                return new ValueTuple<bool, string>(false, strName);
                            }
                        }

                        return new ValueTuple<bool, string>(true, strName);
                    }

                    Art objArt = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? objCharacter.Arts.FirstOrDefault(
                            x => MatchesNameOrSourceId(x.Name, x.SourceIDString, strNodeInnerText))
                        : await (await objCharacter.GetArtsAsync(token).ConfigureAwait(false)).FirstOrDefaultAsync(
                            x => MatchesNameOrSourceId(x.Name, x.SourceIDString, strNodeInnerText), token)
                            .ConfigureAwait(false);
                    if (objArt != null)
                    {
                        if (blnShowMessage)
                            strName = blnSync
                                ? objArt.CurrentDisplayName
                                : await objArt.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                        return new ValueTuple<bool, string>(true, strName);
                    }

                    // In some cases, we want to proxy metamagics for arts. If we haven't found a match yet, check it here.
                    if (xmlNode.Name == "metamagicart")
                    {
                        Metamagic objMetamagic = blnSync
                            ? objCharacter.Metamagics.FirstOrDefault(
                                x => MatchesNameOrSourceId(x.Name, x.SourceIDString, strNodeInnerText))
                            : await (await objCharacter.GetMetamagicsAsync(token).ConfigureAwait(false))
                                .FirstOrDefaultAsync(
                                    x => MatchesNameOrSourceId(x.Name, x.SourceIDString, strNodeInnerText), token)
                                .ConfigureAwait(false);
                        if (objMetamagic != null)
                        {
                            if (blnShowMessage)
                                strName = blnSync
                                    ? objMetamagic.CurrentDisplayName
                                    : await objMetamagic.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                            return new ValueTuple<bool, string>(true, strName);
                        }
                    }

                    if (!blnShowMessage)
                        return new ValueTuple<bool, string>(false, strName);
                    strName = await BuildDataItemRequirementRestrictionMessageAsync(blnSync, objCharacter, strNodeInnerText,
                        strSpace, "metamagic.xml", "arts/art", "String_Art", token).ConfigureAwait(false);
                    return new ValueTuple<bool, string>(false, strName);
                }
                case "magenabled":
                {
                    // Character must be Awakened.
                    if (blnShowMessage)
                        strName = Environment.NewLine + "\t" + string.Format(GlobalSettings.CultureInfo, blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? LanguageManager.GetString(
                                    "Message_SelectGeneric_HaveAttributeEnabled",
                                    token: token)
                                : await LanguageManager.GetStringAsync(
                                    "Message_SelectGeneric_HaveAttributeEnabled",
                                    token: token).ConfigureAwait(false),
                            blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? LanguageManager.GetString(
                                    "String_AttributeMAGLong",
                                    token: token)
                                : await LanguageManager.GetStringAsync(
                                    "String_AttributeMAGLong",
                                    token: token).ConfigureAwait(false));
                    return new ValueTuple<bool, string>(
                        blnSync
                            ? objCharacter.MAGEnabled
                            : await objCharacter.GetMAGEnabledAsync(token).ConfigureAwait(false), strName);
                }
                case "metatype":
                {
                    if (blnShowMessage)
                    {
                        string strXPathFilter = "/chummer/metatypes/metatype[name = "
                                                + strNodeInnerText.CleanXPath() + " or id = "
                                                + strNodeInnerText.CleanXPath() + "]/translate";
                        // Check the Metatype restriction.
                        XPathNavigator objLoopDoc = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? objCharacter.LoadDataXPath("metatypes.xml", token: token)
                            : await objCharacter.LoadDataXPathAsync("metatypes.xml", token: token)
                                .ConfigureAwait(false);
                        string strTranslate = objLoopDoc.SelectSingleNode(strXPathFilter)?.Value;
                        if (string.IsNullOrEmpty(strTranslate))
                        {
                            objLoopDoc = blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? objCharacter.LoadDataXPath("critters.xml", token: token)
                                : await objCharacter.LoadDataXPathAsync("critters.xml", token: token)
                                    .ConfigureAwait(false);
                            strTranslate = objLoopDoc.SelectSingleNode(strXPathFilter)?.Value;
                        }

                        strName = Environment.NewLine + "\t"
                                                      + (!string.IsNullOrEmpty(strTranslate)
                                                          ? strTranslate
                                                          : strNodeInnerText.IsGuid()
                                                              ? objLoopDoc
                                                                  .SelectSingleNode(
                                                                      "/chummer/metatypes/metatype[id = "
                                                                      + strNodeInnerText.CleanXPath()
                                                                      + "]/name")?.Value ?? strNodeInnerText
                                                              : strNodeInnerText) + strSpace + "("
                                                      + (blnSync
                                                          // ReSharper disable once MethodHasAsyncOverload
                                                          ? LanguageManager.GetString(
                                                              "String_Metatype",
                                                              token: token)
                                                          : await LanguageManager.GetStringAsync(
                                                              "String_Metatype",
                                                              token: token).ConfigureAwait(false)) + ")";
                    }

                    return new ValueTuple<bool, string>(strNodeInnerText == objCharacter.Metatype
                                                   || strNodeInnerText == objCharacter.MetatypeGuid.ToString("D",
                                                       GlobalSettings.InvariantCultureInfo), strName);
                }
                case "metatypecategory":
                {
                    if (blnShowMessage)
                    {
                        string strXPathFilter = "/chummer/categories/category[. = " + strNodeInnerText.CleanXPath()
                            + "]/@translate";
                        // Check the Metatype Category restriction.
                        XPathNavigator objLoopDoc = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? objCharacter.LoadDataXPath("metatypes.xml", token: token)
                            : await objCharacter.LoadDataXPathAsync("metatypes.xml", token: token)
                                .ConfigureAwait(false);
                        string strTranslate = objLoopDoc.SelectSingleNode(strXPathFilter)?.Value;
                        if (string.IsNullOrEmpty(strTranslate))
                        {
                            objLoopDoc = blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? objCharacter.LoadDataXPath("critters.xml", token: token)
                                : await objCharacter.LoadDataXPathAsync("critters.xml", token: token)
                                    .ConfigureAwait(false);
                            strTranslate = objLoopDoc.SelectSingleNode(strXPathFilter)?.Value;
                        }

                        strName = Environment.NewLine + "\t"
                                                      + (!string.IsNullOrEmpty(strTranslate)
                                                          ? strTranslate
                                                          : strNodeInnerText) + strSpace + "("
                                                      + (blnSync
                                                          // ReSharper disable once MethodHasAsyncOverload
                                                          ? LanguageManager.GetString(
                                                              "String_MetatypeCategory",
                                                              token: token)
                                                          : await LanguageManager.GetStringAsync(
                                                              "String_MetatypeCategory",
                                                              token: token).ConfigureAwait(false)) + ")";
                    }

                    return new ValueTuple<bool, string>(strNodeInnerText == objCharacter.MetatypeCategory, strName);
                }
                case "metavariant":
                {
                    if (blnShowMessage)
                    {
                        string strXPathFilter = "/chummer/metatypes/metatype/metavariants/metavariant[name = "
                                                + strNodeInnerText.CleanXPath() + " or id = "
                                                + strNodeInnerText.CleanXPath() + "]/translate";
                        // Check the Metavariant restriction.
                        XPathNavigator objLoopDoc = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? objCharacter.LoadDataXPath("metatypes.xml", token: token)
                            : await objCharacter.LoadDataXPathAsync("metatypes.xml", token: token)
                                .ConfigureAwait(false);
                        string strTranslate = objLoopDoc.SelectSingleNode(strXPathFilter)?.Value;
                        if (string.IsNullOrEmpty(strTranslate))
                        {
                            objLoopDoc = blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? objCharacter.LoadDataXPath("critters.xml", token: token)
                                : await objCharacter.LoadDataXPathAsync("critters.xml", token: token)
                                    .ConfigureAwait(false);
                            strTranslate = objLoopDoc.SelectSingleNode(strXPathFilter)?.Value;
                        }

                        strName = Environment.NewLine + "\t"
                                                      + (!string.IsNullOrEmpty(strTranslate)
                                                          ? strTranslate
                                                          : strNodeInnerText.IsGuid()
                                                              ? objLoopDoc
                                                                  .SelectSingleNode(
                                                                      "/chummer/metatypes/metatype/metavariants/metavariant[id = "
                                                                      + strNodeInnerText.CleanXPath()
                                                                      + "]/name")?.Value ?? strNodeInnerText
                                                              : strNodeInnerText) + strSpace + "("
                                                      + (blnSync
                                                          // ReSharper disable once MethodHasAsyncOverload
                                                          ? LanguageManager.GetString(
                                                              "String_Metavariant",
                                                              token: token)
                                                          : await LanguageManager.GetStringAsync(
                                                              "String_Metavariant",
                                                              token: token).ConfigureAwait(false)) + ")";
                    }

                    return new ValueTuple<bool, string>(strNodeInnerText == objCharacter.Metavariant
                                                   || strNodeInnerText == objCharacter.MetavariantGuid.ToString("D",
                                                       GlobalSettings.InvariantCultureInfo), strName);
                }
                case "nuyen":
                {
                    // Character's nuyen must be higher than or equal to the required value.
                    if (blnShowMessage)
                        strName = Environment.NewLine + "\t" + (blnSync
                                      // ReSharper disable once MethodHasAsyncOverload
                                      ? LanguageManager.GetString(
                                          "String_Nuyen",
                                          token: token)
                                      : await LanguageManager.GetStringAsync(
                                          "String_Nuyen",
                                          token: token).ConfigureAwait(false)) + strSpace
                                  + "≥" + strSpace + strNodeInnerText;
                    return new ValueTuple<bool, string>(
                        (blnSync
                            ? objCharacter.Nuyen
                            : await objCharacter.GetNuyenAsync(token).ConfigureAwait(false)) >= xmlNode.ValueAsInt,
                        strName);
                }
                case "onlyprioritygiven":
                {
                    if (blnShowMessage)
                        strName = Environment.NewLine + "\t"
                                                      + (blnSync
                                                          // ReSharper disable once MethodHasAsyncOverload
                                                          ? LanguageManager.GetString(
                                                              "Message_SelectGeneric_PriorityRestriction",
                                                              token: token)
                                                          : await LanguageManager.GetStringAsync(
                                                              "Message_SelectGeneric_PriorityRestriction",
                                                              token: token).ConfigureAwait(false));
                    return new ValueTuple<bool, string>(
                        blnSync
                            ? objCharacter.EffectiveBuildMethodUsesPriorityTables
                            : await objCharacter.GetEffectiveBuildMethodUsesPriorityTablesAsync(token)
                                .ConfigureAwait(false), strName);
                }
                case "power":
                    return blnSync
                        ? Utils.SafelyRunSynchronously(
                            () => TestInstalledDataItemRequirementCoreAsync(true, xmlNode, objCharacter, objParent,
                                InstalledDataItemRequirementKind.Power, strNodeInnerText, strSpace, blnShowMessage,
                                "powers.xml", "powers/power", "Tab_Adept", token), token)
                        : await TestInstalledDataItemRequirementCoreAsync(false, xmlNode, objCharacter, objParent,
                            InstalledDataItemRequirementKind.Power, strNodeInnerText, strSpace, blnShowMessage,
                            "powers.xml", "powers/power", "Tab_Adept", token).ConfigureAwait(false);
                case "program":
                {
                    bool blnResult = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? objCharacter.AIPrograms.Any(
                            p => MatchesNameOrSourceId(p.Name, p.SourceIDString, strNodeInnerText), token)
                        : await (await objCharacter.GetAIProgramsAsync(token).ConfigureAwait(false)).AnyAsync(
                            p => MatchesNameOrSourceId(p.Name, p.SourceIDString, strNodeInnerText), token)
                            .ConfigureAwait(false);
                    // Character needs a specific Program.
                    if (!blnShowMessage)
                        return new ValueTuple<bool, string>(blnResult, strName);
                    strName = await BuildDataItemRequirementRestrictionMessageAsync(blnSync, objCharacter, strNodeInnerText,
                        strSpace, "programs.xml", "programs/program", "String_Program", token).ConfigureAwait(false);
                    return new ValueTuple<bool, string>(blnResult, strName);
                }
                case "characterquality":
                case "quality":
                    return blnSync
                        ? Utils.SafelyRunSynchronously(
                            () => TestInstalledCharacterQualityRequirementCoreAsync(true, xmlNode, objCharacter,
                                strNodeInnerText, strSpace, strIgnoreQuality, blnShowMessage, token), token)
                        : await TestInstalledCharacterQualityRequirementCoreAsync(false, xmlNode, objCharacter,
                            strNodeInnerText, strSpace, strIgnoreQuality, blnShowMessage, token).ConfigureAwait(false);
                case "lifestylequality":
                    return blnSync
                        ? Utils.SafelyRunSynchronously(
                            () => TestInstalledLifestyleQualityRequirementCoreAsync(true, xmlNode, objCharacter,
                                objParent, strNodeInnerText, strSpace, strIgnoreQuality, blnShowMessage, token), token)
                        : await TestInstalledLifestyleQualityRequirementCoreAsync(false, xmlNode, objCharacter,
                            objParent, strNodeInnerText, strSpace, strIgnoreQuality, blnShowMessage, token)
                            .ConfigureAwait(false);
                case "lifestyle":
                {
                    if (blnShowMessage)
                    {
                        strName = await BuildDataItemRequirementRestrictionMessageAsync(blnSync, objCharacter, strNodeInnerText,
                            strSpace, "lifestyles.xml", "lifestyles/lifestyle", "String_Lifestyle", token)
                            .ConfigureAwait(false);
                    }

                    return new ValueTuple<bool, string>(objParent is Lifestyle objLifestyle
                        ? (blnSync
                            ? objLifestyle.BaseLifestyle
                            : await objLifestyle.GetBaseLifestyleAsync(token).ConfigureAwait(false)) == strNodeInnerText
                        : blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? objCharacter.Lifestyles.Any(
                                x => x.BaseLifestyle == strNodeInnerText, token)
                            : await (await objCharacter.GetLifestylesAsync(token)
                                    .ConfigureAwait(false))
                                .AnyAsync(
                                    async x => await x.GetBaseLifestyleAsync(token).ConfigureAwait(false) ==
                                               strNodeInnerText,
                                    token)
                                .ConfigureAwait(false), strName);
                }
                case "resenabled":
                    // Character must be Emerged.
                    if (blnShowMessage)
                        strName = Environment.NewLine + "\t" + string.Format(GlobalSettings.CultureInfo, blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? LanguageManager.GetString(
                                    "Message_SelectGeneric_HaveAttributeEnabled",
                                    token: token)
                                : await LanguageManager.GetStringAsync(
                                    "Message_SelectGeneric_HaveAttributeEnabled",
                                    token: token).ConfigureAwait(false),
                            blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? LanguageManager.GetString(
                                    "String_AttributeRESLong",
                                    token: token)
                                : await LanguageManager.GetStringAsync(
                                    "String_AttributeRESLong",
                                    token: token).ConfigureAwait(false));
                    return new ValueTuple<bool, string>(
                        blnSync
                            ? objCharacter.RESEnabled
                            : await objCharacter.GetRESEnabledAsync(token).ConfigureAwait(false), strName);

                case "skill":
                {
                    string strSpec
                        = xmlNode.SelectSingleNodeAndCacheExpression("spec", token)?.Value;
                    string strValue = xmlNode.SelectSingleNodeAndCacheExpression("val", token)?.Value;
                    int intValue = xmlNode.SelectSingleNodeAndCacheExpression("val", token)?.ValueAsInt ?? 0;
                    Skill objSkill = null;
                    SkillsSection objSkillsSection = blnSync
                        ? objCharacter.SkillsSection
                        : await objCharacter.GetSkillsSectionAsync(token).ConfigureAwait(false);
                    // Check if the character has the required Skill.
                    // ReSharper disable once MethodHasAsyncOverload
                    if (xmlNode.SelectSingleNodeAndCacheExpression("type", token) != null)
                    {
                        if (blnSync)
                        {
                            objSkill = string.IsNullOrEmpty(strSpec)
                                ? objSkillsSection.KnowledgeSkills.FirstOrDefault(
                                    x => (string.Equals(x.SourceIDString, strNodeId,
                                             StringComparison.OrdinalIgnoreCase) || x.DictionaryKey == strNodeName)
                                         && x.TotalBaseRating >= intValue)
                                : objSkillsSection.KnowledgeSkills.FirstOrDefault(
                                    x => (string.Equals(x.SourceIDString, strNodeId,
                                             StringComparison.OrdinalIgnoreCase) || x.DictionaryKey == strNodeName)
                                         && x.HasSpecialization(strSpec, token)
                                         && x.TotalBaseRating >= intValue);
                        }
                        else
                        {
                            objSkill = string.IsNullOrEmpty(strSpec)
                                ? await objSkillsSection.KnowledgeSkills.FirstOrDefaultAsync(
                                    async x => (string.Equals(x.SourceIDString, strNodeId,
                                                    StringComparison.OrdinalIgnoreCase) ||
                                                x.DictionaryKey == strNodeName)
                                               && await x.GetTotalBaseRatingAsync(token).ConfigureAwait(false) >=
                                               intValue, token).ConfigureAwait(false)
                                : await objSkillsSection.KnowledgeSkills.FirstOrDefaultAsync(
                                    async x =>
                                        (string.Equals(x.SourceIDString, strNodeId,
                                             StringComparison.OrdinalIgnoreCase) ||
                                         await x.GetDictionaryKeyAsync(token).ConfigureAwait(false) == strNodeName)
                                        && await x.HasSpecializationAsync(strSpec, token).ConfigureAwait(false)
                                        && await x.GetTotalBaseRatingAsync(token).ConfigureAwait(false) >= intValue,
                                    token).ConfigureAwait(false);
                        }

                        if (objSkill != null)
                        {
                            if (blnShowMessage)
                            {
                                strName = blnSync
                                    ? objSkill.CurrentDisplayName
                                    : await objSkill.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                                if (!string.IsNullOrEmpty(strSpec) && (blnSync
                                        ? ImprovementManager
                                            // ReSharper disable once MethodHasAsyncOverload
                                            .GetCachedImprovementListForValueOf(
                                                objCharacter,
                                                Improvement.ImprovementType
                                                    .DisableSpecializationEffects,
                                                objSkill.DictionaryKey, token: token)
                                        : await ImprovementManager
                                            .GetCachedImprovementListForValueOfAsync(
                                                objCharacter,
                                                Improvement.ImprovementType
                                                    .DisableSpecializationEffects,
                                                await objSkill.GetDictionaryKeyAsync(token).ConfigureAwait(false),
                                                token: token).ConfigureAwait(false)).Count
                                    == 0)
                                {
                                    strName += strSpace + "(" + strSpec + ")";
                                }

                                if (!string.IsNullOrEmpty(strValue))
                                {
                                    strName += strSpace + strValue;
                                }
                            }

                            return new ValueTuple<bool, string>(true, strName);
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(strNodeId))
                        {
                            objSkill = blnSync
                                ? objSkillsSection.Skills.FirstOrDefault(
                                    x => string.Equals(x.SourceIDString, strNodeId, StringComparison.OrdinalIgnoreCase))
                                : await (await objSkillsSection.GetSkillsAsync(token).ConfigureAwait(false))
                                    .FirstOrDefaultAsync(
                                        x => string.Equals(x.SourceIDString, strNodeId,
                                            StringComparison.OrdinalIgnoreCase), token).ConfigureAwait(false);
                        }
                        else if (!string.IsNullOrEmpty(strNodeName))
                        {
                            objSkill = blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? objSkillsSection.GetActiveSkill(strNodeName, token)
                                : await objSkillsSection.GetActiveSkillAsync(strNodeName, token).ConfigureAwait(false);
                            // Exotic Skill
                            if (objSkill == null && !string.IsNullOrEmpty(strSpec))
                                objSkill = blnSync
                                    // ReSharper disable once MethodHasAsyncOverload
                                    ? objSkillsSection.GetActiveSkill(strNodeName + strSpace + "(" + strSpec + ")",
                                        token)
                                    : await objSkillsSection.GetActiveSkillAsync(
                                        strNodeName + strSpace + "(" + strSpec + ")", token).ConfigureAwait(false);
                        }

                        if (objSkill != null)
                        {
                            bool blnSpecMet = string.IsNullOrEmpty(strSpec) || (blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? objSkill.HasSpecialization(strSpec, token)
                                : await objSkill.HasSpecializationAsync(strSpec, token).ConfigureAwait(false));
                            if (blnSpecMet && (blnSync
                                    ? objSkill.TotalBaseRating
                                    : await objSkill.GetTotalBaseRatingAsync(token).ConfigureAwait(false)) >= intValue)
                            {
                                if (blnShowMessage)
                                {
                                    strName = blnSync
                                        ? objSkill.CurrentDisplayName
                                        : await objSkill.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                                    if (!string.IsNullOrEmpty(strSpec) && (blnSync
                                            ? ImprovementManager
                                                // ReSharper disable once MethodHasAsyncOverload
                                                .GetCachedImprovementListForValueOf(
                                                    objCharacter,
                                                    Improvement.ImprovementType
                                                        .DisableSpecializationEffects,
                                                    objSkill.DictionaryKey, token: token)
                                            : await ImprovementManager
                                                .GetCachedImprovementListForValueOfAsync(
                                                    objCharacter,
                                                    Improvement.ImprovementType
                                                        .DisableSpecializationEffects,
                                                    await objSkill.GetDictionaryKeyAsync(token).ConfigureAwait(false),
                                                    token: token).ConfigureAwait(false)).Count
                                        == 0)
                                    {
                                        strName += strSpace + "(" + strSpec + ")";
                                    }

                                    if (!string.IsNullOrEmpty(strValue))
                                    {
                                        strName += strSpace + strValue;
                                    }
                                }

                                return new ValueTuple<bool, string>(true, strName);
                            }
                        }
                    }

                    if (blnShowMessage)
                    {
                        XPathNavigator xmlSkillDoc = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? objCharacter.LoadDataXPath("skills.xml", token: token)
                            : await objCharacter.LoadDataXPathAsync("skills.xml", token: token).ConfigureAwait(false);
                        string strSkillId
                            = xmlNode.SelectSingleNodeAndCacheExpression("id", token)?.Value;
                        string strSkillName
                            = xmlNode.SelectSingleNodeAndCacheExpression("name", token)?.Value;
                        string strTranslate
                            = xmlSkillDoc.SelectSingleNode("/chummer/skills/skill[id = " + strSkillId.CleanXPath()
                                  + "]/translate")?.Value
                              ?? xmlSkillDoc
                                  .SelectSingleNode("/chummer/knowledgeskills/skill[id = " + strSkillId.CleanXPath()
                                      + "]/translate")?.Value
                              ?? xmlSkillDoc
                                  .SelectSingleNode("/chummer/skills/skill[name = " + strSkillName.CleanXPath()
                                      + "]/translate")?.Value
                              ?? xmlSkillDoc
                                  .SelectSingleNode("/chummer/knowledgeskills/skill[name = " + strSkillName.CleanXPath()
                                      + "]/translate")?.Value;
                        if (string.IsNullOrEmpty(strTranslate) && string.IsNullOrEmpty(strSkillName))
                            strSkillName
                                = xmlSkillDoc.SelectSingleNode(
                                      "/chummer/skills/skill[id = " + strSkillId.CleanXPath() + "]/name")?.Value
                                  ?? xmlSkillDoc
                                      .SelectSingleNode("/chummer/knowledgeskills/skill[id = " + strSkillId.CleanXPath()
                                          + "]/name")?.Value;
                        strName = Environment.NewLine + "\t"
                                                      + (!string.IsNullOrEmpty(strTranslate)
                                                          ? strTranslate
                                                          : strSkillName);
                        if (!string.IsNullOrEmpty(strSpec))
                        {
                            strName += strSpace + "(" + strSpec + ")";
                        }

                        if (!string.IsNullOrEmpty(strValue))
                        {
                            strName += strSpace + strValue;
                        }

                        strName += strSpace + "(" + (blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? LanguageManager.GetString(
                                "Tab_Skills",
                                token: token)
                            : await LanguageManager.GetStringAsync(
                                "Tab_Skills",
                                token: token).ConfigureAwait(false)) + ")";
                    }

                    return new ValueTuple<bool, string>(false, strName);
                }
                case "skilltotal":
                {
                    // Check if the total combined Ratings of Skills adds up to a particular total.
                    int intTotal = 0;
                    IEnumerable<string> lstSkills
                        = xmlNode.SelectSingleNodeAndCacheExpression("skills", token)?.Value
                            .SplitNoAlloc('+', StringSplitOptions.RemoveEmptyEntries);
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                               out StringBuilder sbdOutput))
                    {
                        sbdOutput.AppendLine().Append('\t');
                        if (lstSkills != null)
                        {
                            SkillsSection objSkillsSection = blnSync
                                ? objCharacter.SkillsSection
                                : await objCharacter.GetSkillsSectionAsync(token).ConfigureAwait(false);
                            if (blnSync)
                            {
                                // If the xmlnode contains Type element, assume that it is a Knowledge skill.
                                if (xmlNode.SelectSingleNodeAndCacheExpression("type", token) != null)
                                {
                                    foreach (string strLoop in lstSkills)
                                    {
                                        foreach (KnowledgeSkill objGroup in objSkillsSection.KnowledgeSkills)
                                        {
                                            if (objGroup.DictionaryKey != strLoop
                                                && objGroup.SourceIDString != strLoop)
                                                continue;
                                            if (blnShowMessage)
                                                sbdOutput.Append(objGroup.CurrentDisplayName, ',', strSpace);
                                            intTotal += objGroup.Rating;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (string strLoop in lstSkills)
                                    {
                                        foreach (Skill objGroup in objSkillsSection.Skills)
                                        {
                                            if (objGroup.DictionaryKey != strLoop
                                                && objGroup.SourceIDString != strLoop)
                                                continue;
                                            if (blnShowMessage)
                                                sbdOutput.Append(objGroup.CurrentDisplayName, ',', strSpace);
                                            intTotal += objGroup.Rating;
                                            break;
                                        }
                                    }
                                }
                            }
                            // If the xmlnode contains Type element, assume that it is a Knowledge skill.
                            else if (xmlNode.SelectSingleNodeAndCacheExpression("type", token) != null)
                            {
                                foreach (string strLoop in lstSkills)
                                {
                                    await (await objSkillsSection.GetKnowledgeSkillsAsync(token).ConfigureAwait(false))
                                        .ForEachWithBreakAsync(
                                            async objGroup =>
                                            {
                                                if (await objGroup.GetDictionaryKeyAsync(token).ConfigureAwait(false) !=
                                                    strLoop
                                                    && objGroup.SourceIDString != strLoop)
                                                    return true;
                                                if (blnShowMessage)
                                                    sbdOutput.Append(await objGroup.GetCurrentDisplayNameAsync(token)
                                                            .ConfigureAwait(false), ',', strSpace);
                                                intTotal += await objGroup.GetRatingAsync(token).ConfigureAwait(false);
                                                return false;
                                            }, token).ConfigureAwait(false);
                                }
                            }
                            else
                            {
                                foreach (string strLoop in lstSkills)
                                {
                                    await (await objSkillsSection.GetSkillsAsync(token).ConfigureAwait(false))
                                        .ForEachWithBreakAsync(
                                            async objGroup =>
                                            {
                                                if (await objGroup.GetDictionaryKeyAsync(token).ConfigureAwait(false) !=
                                                    strLoop
                                                    && objGroup.SourceIDString != strLoop)
                                                    return true;
                                                if (blnShowMessage)
                                                    sbdOutput.Append(await objGroup.GetCurrentDisplayNameAsync(token)
                                                            .ConfigureAwait(false), ',', strSpace);
                                                intTotal += await objGroup.GetRatingAsync(token).ConfigureAwait(false);
                                                return false;
                                            }, token).ConfigureAwait(false);
                                }
                            }
                        }

                        if (!blnShowMessage)
                            return new ValueTuple<bool, string>(
                                intTotal >= (xmlNode.SelectSingleNodeAndCacheExpression("val", token)?.ValueAsInt ?? 0),
                                strName);
                        if (sbdOutput.Length > 0)
                            sbdOutput.Length -= 2;
                        strName = sbdOutput.Append(strSpace, '(').Append(blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? LanguageManager.GetString(
                                "String_ExpenseSkill",
                                token: token)
                            : await LanguageManager.GetStringAsync(
                                "String_ExpenseSkill",
                                token: token).ConfigureAwait(false), ')').ToString();
                    }

                    int intTarget = xmlNode.SelectSingleNodeAndCacheExpression("val", token)
                        ?.ValueAsInt ?? 0;
                    return new ValueTuple<bool, string>(intTotal >= intTarget, strName);
                }
                case "skillgrouptotal":
                {
                    // Check if the total combined Ratings of Skill Groups adds up to a particular total.
                    int intTotal = 0;
                    IEnumerable<string> lstGroups
                        = xmlNode.SelectSingleNodeAndCacheExpression("skillgroups", token)?.Value
                            .SplitNoAlloc('+', StringSplitOptions.RemoveEmptyEntries);
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                               out StringBuilder sbdOutput))
                    {
                        sbdOutput.AppendLine().Append('\t');
                        if (lstGroups != null)
                        {
                            SkillsSection objSkillsSection = blnSync
                                ? objCharacter.SkillsSection
                                : await objCharacter.GetSkillsSectionAsync(token).ConfigureAwait(false);
                            if (blnSync)
                            {
                                foreach (string strLoop in lstGroups)
                                {
                                    foreach (SkillGroup objGroup in objSkillsSection.SkillGroups)
                                    {
                                        if (objGroup.Name == strLoop)
                                        {
                                            if (blnShowMessage)
                                                sbdOutput.Append(objGroup.CurrentDisplayName, ',', strSpace);
                                            intTotal += objGroup.Rating;
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                foreach (string strLoop in lstGroups)
                                {
                                    await (await objSkillsSection.GetSkillGroupsAsync(token).ConfigureAwait(false))
                                        .ForEachWithBreakAsync(
                                            async objGroup =>
                                            {
                                                if (objGroup.Name == strLoop)
                                                {
                                                    if (blnShowMessage)
                                                        sbdOutput.Append(await objGroup
                                                                .GetCurrentDisplayNameAsync(token)
                                                                .ConfigureAwait(false), ',', strSpace);
                                                    intTotal += await objGroup.GetRatingAsync(token)
                                                        .ConfigureAwait(false);
                                                    return false;
                                                }

                                                return true;
                                            }, token).ConfigureAwait(false);
                                }
                            }
                        }

                        if (blnShowMessage)
                        {
                            if (sbdOutput.Length > 0)
                                sbdOutput.Length -= 2;
                            strName = sbdOutput.Append(strSpace, '(').Append(blnSync
                                          // ReSharper disable once MethodHasAsyncOverload
                                          ? LanguageManager.GetString(
                                              "String_ExpenseSkillGroup",
                                              token: token)
                                          : await LanguageManager.GetStringAsync(
                                              "String_ExpenseSkillGroup",
                                              token: token).ConfigureAwait(false), ')').ToString();
                        }
                    }

                    int intTarget = xmlNode.SelectSingleNodeAndCacheExpression("val", token)?.ValueAsInt ?? 0;
                    return new ValueTuple<bool, string>(intTotal >= intTarget, strName);
                }
                case "specialmodificationlimit":
                {
                    // Add in the cost of all child components.
                    int intMods = 0;
                    if (blnSync)
                    {
                        intMods = objCharacter.Weapons.GetAllDescendants(x => x.UnderbarrelWeapons, token).Concat(
                                objCharacter.Vehicles.SelectMany(
                                    y => y.Weapons.Concat(y.WeaponMounts.SelectMany(x => x.Weapons))
                                        .GetAllDescendants(x => x.UnderbarrelWeapons)))
                            .Sum(x => x.WeaponAccessories.Count(y => y.SpecialModification, token));
                    }
                    else
                    {
                        List<Weapon> lstWeapons = new List<Weapon>(2 * await (await objCharacter.GetWeaponsAsync(token)).GetCountAsync(token).ConfigureAwait(false));
                        lstWeapons.AddRange(await (await objCharacter.GetWeaponsAsync(token)
                                     .ConfigureAwait(false)).GetAllDescendantsAsync(
                                     x => x.UnderbarrelWeapons, token).ConfigureAwait(false));

                        await (await objCharacter.GetVehiclesAsync(token).ConfigureAwait(false)).ForEachAsync(async objVehicle =>
                        {
                            lstWeapons.AddRange(await objVehicle.Weapons
                                            .GetAllDescendantsAsync(x => x.UnderbarrelWeapons, token)
                                            .ConfigureAwait(false));

                            await objVehicle.WeaponMounts.ForEachAsync(async objMount =>
                            {
                                lstWeapons.AddRange(await objMount.Weapons.GetAllDescendantsAsync(
                                                x => x.UnderbarrelWeapons, token).ConfigureAwait(false));
                            }, token).ConfigureAwait(false);
                        }, token).ConfigureAwait(false);
                        foreach (Weapon objWeapon in lstWeapons)
                            intMods += await objWeapon.WeaponAccessories.CountAsync(x => x.SpecialModification, token).ConfigureAwait(false);
                    }

                    if (blnShowMessage)
                    {
                        strName = Environment.NewLine + "\t"
                                                      + (blnSync
                                                          // ReSharper disable once MethodHasAsyncOverload
                                                          ? LanguageManager.GetString(
                                                              "String_SpecialModificationLimit",
                                                              token: token)
                                                          : await LanguageManager.GetStringAsync(
                                                              "String_SpecialModificationLimit",
                                                              token: token).ConfigureAwait(false))
                                                      + strSpace + "≥" + strSpace + strNodeInnerText;
                    }

                    return new ValueTuple<bool, string>(
                        intMods + xmlNode.ValueAsInt <= (blnSync ? objCharacter.SpecialModificationLimit : await objCharacter.GetSpecialModificationLimitAsync(token).ConfigureAwait(false)), strName);
                }
                case "spell":
                {
                    Spell objSpell = blnSync
                        ? objCharacter.Spells.FirstOrDefault(
                            x => MatchesNameOrSourceId(x.Name, x.SourceIDString, strNodeInnerText))
                        : await (await objCharacter.GetSpellsAsync(token).ConfigureAwait(false)).FirstOrDefaultAsync(
                            x => MatchesNameOrSourceId(x.Name, x.SourceIDString, strNodeInnerText), token)
                            .ConfigureAwait(false);
                    if (objSpell != null)
                    {
                        if (blnShowMessage)
                            strName = blnSync
                                ? objSpell.CurrentDisplayName
                                : await objSpell.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                        return new ValueTuple<bool, string>(true, strName);
                    }

                    if (blnShowMessage)
                    {
                        strName = await BuildDataItemRequirementRestrictionMessageAsync(blnSync, objCharacter, strNodeInnerText,
                            strSpace, "spells.xml", "spells/spell", "String_DescSpell", token).ConfigureAwait(false);
                    }

                    return new ValueTuple<bool, string>(false, strName);
                }
                case "spellcategory":
                {
                    // Check for a specified amount of a particular Spell category.
                    if (blnShowMessage)
                    {
                        string strTranslate
                            = (blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? objCharacter.LoadDataXPath("spells.xml", token: token)
                                : await objCharacter.LoadDataXPathAsync("spells.xml", token: token)
                                    .ConfigureAwait(false))
                            .SelectSingleNode(
                                "/chummer/categories/category[. = "
                                + strNodeName.CleanXPath() + "]/@translate")?.Value;
                        strName = Environment.NewLine + "\t"
                                                      + (!string.IsNullOrEmpty(strTranslate)
                                                          ? strTranslate
                                                          : strNodeInnerText) + strSpace + "("
                                                      + (blnSync
                                                          // ReSharper disable once MethodHasAsyncOverload
                                                          ? LanguageManager.GetString(
                                                              "String_SpellCategory",
                                                              token: token)
                                                          : await LanguageManager.GetStringAsync(
                                                              "String_SpellCategory",
                                                              token: token).ConfigureAwait(false)) + ")";
                    }

                    int intTarget = xmlNode.SelectSingleNodeAndCacheExpression("count", token)?.ValueAsInt ?? 0;
                    if (blnSync)
                        // ReSharper disable once MethodHasAsyncOverload
                        return new ValueTuple<bool, string>(
                            objCharacter.Spells.Count(objSpell => objSpell.Category == strNodeName, token) >= intTarget,
                            strName);
                    return new ValueTuple<bool, string>(
                        await (await objCharacter.GetSpellsAsync(token).ConfigureAwait(false))
                            .CountAsync(objSpell => objSpell.Category == strNodeName, token).ConfigureAwait(false) >=
                        intTarget, strName);
                }
                case "spelldescriptor":
                {
                    int intCount = xmlNode.SelectSingleNodeAndCacheExpression("count", token)?.ValueAsInt ?? 0;
                    // Check for a specified amount of a particular Spell Descriptor.
                    if (blnShowMessage)
                        strName = Environment.NewLine + "\t" + (blnSync
                                      // ReSharper disable once MethodHasAsyncOverload
                                      ? LanguageManager.GetString(
                                          "Label_Descriptors",
                                          token: token)
                                      : await LanguageManager.GetStringAsync(
                                          "Label_Descriptors",
                                          token: token).ConfigureAwait(false)) + strSpace
                                  + "≥" + strSpace + intCount.ToString(GlobalSettings.CultureInfo);
                    if (blnSync)
                        // ReSharper disable once MethodHasAsyncOverload
                        return new ValueTuple<bool, string>(objCharacter.Spells.Count(
                                                           objSpell => objSpell.Descriptors.Contains(strNodeName),
                                                           token)
                                                       // ReSharper disable once MethodHasAsyncOverload
                                                       >= intCount, strName);
                    return new ValueTuple<bool, string>(
                        await (await objCharacter.GetSpellsAsync(token).ConfigureAwait(false)).CountAsync(
                            objSpell => objSpell.Descriptors.Contains(strNodeName), token).ConfigureAwait(false)
                        >= intCount, strName);
                }
                case "streetcredvsnotoriety":
                {
                    // Street Cred must be higher than Notoriety.
                    if (blnShowMessage)
                        strName = Environment.NewLine + "\t" + (blnSync
                                      // ReSharper disable once MethodHasAsyncOverload
                                      ? LanguageManager.GetString(
                                          "String_StreetCred",
                                          token: token)
                                      : await LanguageManager.GetStringAsync(
                                          "String_StreetCred",
                                          token: token).ConfigureAwait(false)) + strSpace
                                  + "≥" + strSpace + (blnSync
                                      // ReSharper disable once MethodHasAsyncOverload
                                      ? LanguageManager.GetString(
                                          "String_Notoriety",
                                          token: token)
                                      : await LanguageManager.GetStringAsync(
                                          "String_Notoriety",
                                          token: token).ConfigureAwait(false));
                    return new ValueTuple<bool, string>(
                        blnSync
                            ? objCharacter.StreetCred >= objCharacter.Notoriety
                            : await objCharacter.GetStreetCredAsync(token).ConfigureAwait(false) >=
                              await objCharacter.GetNotorietyAsync(token).ConfigureAwait(false), strName);
                }
                case "submersiongrade":
                {
                    // Character's initiate grade must be higher than or equal to the required value.
                    if (blnShowMessage)
                        strName = Environment.NewLine + "\t" + (blnSync
                                      // ReSharper disable once MethodHasAsyncOverload
                                      ? LanguageManager.GetString(
                                          "String_SubmersionGrade",
                                          token: token)
                                      : await LanguageManager.GetStringAsync(
                                          "String_SubmersionGrade",
                                          token: token).ConfigureAwait(false))
                                  + strSpace + "≥" + strSpace + strNodeInnerText;
                    return new ValueTuple<bool, string>(
                        (blnSync
                            ? objCharacter.SubmersionGrade
                            : await objCharacter.GetSubmersionGradeAsync(token).ConfigureAwait(false))
                        >= xmlNode.ValueAsInt, strName);
                }
                case "tradition":
                {
                    // Character needs a specific Tradition.
                    if (blnShowMessage)
                    {
                        strName = await BuildDataItemRequirementRestrictionMessageAsync(blnSync, objCharacter, strNodeInnerText,
                            strSpace, "traditions.xml", "traditions/tradition", "String_Tradition", token)
                            .ConfigureAwait(false);
                    }

                    Backend.Uniques.Tradition objTradition = blnSync
                        ? objCharacter.MagicTradition
                        : await objCharacter.GetMagicTraditionAsync(token).ConfigureAwait(false);

                    return new ValueTuple<bool, string>(
                        MatchesNameOrSourceId(objTradition.Name, objTradition.SourceIDString, strNodeInnerText),
                        strName);
                }
                case "traditionspiritform":
                {
                    // Character needs a specific spirit form provided by their Tradition.
                    if (blnShowMessage)
                    {
                        strName = await BuildDataItemRequirementRestrictionMessageAsync(blnSync, objCharacter, strNodeInnerText,
                            strSpace, "critterpowers.xml", "powers/power", "String_Tradition", token)
                            .ConfigureAwait(false);
                    }

                    XPathNavigator objLoopDoc;

                    Backend.Uniques.Tradition objTradition = blnSync
                        ? objCharacter.MagicTradition
                        : await objCharacter.GetMagicTraditionAsync(token).ConfigureAwait(false);

                    if (objTradition.SpiritForm == strNodeInnerText)
                        return new ValueTuple<bool, string>(true, strName);
                    if (!strNodeInnerText.IsGuid())
                        return new ValueTuple<bool, string>(false, strName);
                    objLoopDoc = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? objCharacter.LoadDataXPath("critterpowers.xml", token: token)
                        : await objCharacter.LoadDataXPathAsync("critterpowers.xml", token: token)
                            .ConfigureAwait(false);
                    string strEnglishName = objLoopDoc
                        .SelectSingleNode(
                            "/chummer/powers/power[id = " + strNodeInnerText.CleanXPath()
                                                          + "]/name")?.Value;
                    return new ValueTuple<bool, string>(objTradition.SpiritForm == strEnglishName, strName);
                }
                case "weapon":
                {
                    bool blnReturn = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? objCharacter.Weapons.Any(
                            x => MatchesNameOrSourceId(x.Name, x.SourceIDString, strNodeInnerText), token)
                        : await objCharacter.Weapons.AnyAsync(
                            x => MatchesNameOrSourceId(x.Name, x.SourceIDString, strNodeInnerText), token)
                            .ConfigureAwait(false);
                    // Character needs a specific Weapon.
                    if (!blnShowMessage)
                        return new ValueTuple<bool, string>(blnReturn, strName);
                    strName = await BuildDataItemRequirementRestrictionMessageAsync(blnSync, objCharacter, strNodeInnerText,
                        strSpace, "weapons.xml", "weapons/weapon", "String_Weapon", token).ConfigureAwait(false);
                    return new ValueTuple<bool, string>(blnReturn, strName);
                }
                case "accessory" when objParent is Weapon objWeapon:
                {
                        bool blnReturn = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? objWeapon.WeaponAccessories.Any(
                            x => MatchesNameOrSourceId(x.Name, x.SourceIDString, strNodeInnerText), token)
                        : await objWeapon.WeaponAccessories.AnyAsync(
                            x => MatchesNameOrSourceId(x.Name, x.SourceIDString, strNodeInnerText), token)
                            .ConfigureAwait(false);
                    if (!blnShowMessage)
                        return new ValueTuple<bool, string>(blnReturn, strName);
                    strName = await BuildDataItemRequirementRestrictionMessageAsync(blnSync, objCharacter, strNodeInnerText,
                        strSpace, "weapons.xml", "accessories/accessory", "String_WeaponAccessory", token)
                        .ConfigureAwait(false);
                    return new ValueTuple<bool, string>(blnReturn, strName);
                }
                case "weapondetails" when objParent is Weapon objWeapon:
                {
                    return new ValueTuple<bool, string>(
                        blnSync
                            // ReSharper disable MethodHasAsyncOverload
                            ? objWeapon.GetNodeXPath(token).ProcessFilterOperationNode(xmlNode, false, token)
                            // ReSharper restore MethodHasAsyncOverload
                            : await (await objWeapon.GetNodeXPathAsync(token).ConfigureAwait(false))
                                .ProcessFilterOperationNodeAsync(
                                    xmlNode, false, token).ConfigureAwait(false), strName);
                }
                case "armormod":
                    return blnSync
                        ? Utils.SafelyRunSynchronously(
                            () => TestInstalledDataItemRequirementCoreAsync(true, xmlNode, objCharacter, objParent,
                                InstalledDataItemRequirementKind.ArmorMod, strNodeInnerText, strSpace, blnShowMessage,
                                "armor.xml", "armormods/armormod", "String_ArmorMod", token), token)
                        : await TestInstalledDataItemRequirementCoreAsync(false, xmlNode, objCharacter, objParent,
                            InstalledDataItemRequirementKind.ArmorMod, strNodeInnerText, strSpace, blnShowMessage,
                            "armor.xml", "armormods/armormod", "String_ArmorMod", token).ConfigureAwait(false);
                default:
                    Utils.BreakIfDebug();
                    break;
            }

            if (blnShowMessage)
                strName = strNodeInnerText;
            return new ValueTuple<bool, string>(false, strName);
        }

        /// <summary>
        /// Parsed <c>rating</c>, <c>minrating</c>, and <c>maxrating</c> attributes from a requirements condition node.
        /// </summary>
        private readonly struct RequirementRatingFilter
        {
            public RequirementRatingFilter(int? intExactRating, int intMinRating, int intMaxRating, bool blnHasFilter)
            {
                ExactRating = intExactRating;
                MinRating = intMinRating;
                MaxRating = intMaxRating;
                HasFilter = blnHasFilter;
            }

            public int? ExactRating { get; }

            public int MinRating { get; }

            public int MaxRating { get; }

            public bool HasFilter { get; }
        }

        /// <summary>
        /// Reads rating-related attributes from a requirements condition node into a <see cref="RequirementRatingFilter"/>.
        /// </summary>
        /// <param name="xmlNode">Condition node that may define <c>rating</c>, <c>minrating</c>, and/or <c>maxrating</c>.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>A filter describing which installed ratings satisfy the condition.</returns>
        private static RequirementRatingFilter GetRequirementRatingFilter(XPathNavigator xmlNode, CancellationToken token)
        {
            XPathNavigator objExactRatingNode = xmlNode.SelectSingleNodeAndCacheExpression("@rating", token);
            if (objExactRatingNode != null)
                return new RequirementRatingFilter(objExactRatingNode.ValueAsInt, 0, int.MaxValue, true);
            XPathNavigator objMinRatingNode = xmlNode.SelectSingleNodeAndCacheExpression("@minrating", token);
            XPathNavigator objMaxRatingNode = xmlNode.SelectSingleNodeAndCacheExpression("@maxrating", token);
            if (objMinRatingNode != null || objMaxRatingNode != null)
            {
                return new RequirementRatingFilter(null, objMinRatingNode?.ValueAsInt ?? 0,
                    objMaxRatingNode?.ValueAsInt ?? int.MaxValue, true);
            }

            return default;
        }

        /// <summary>
        /// Returns whether an installed item's rating satisfies a <see cref="RequirementRatingFilter"/>.
        /// </summary>
        /// <param name="intRating">Rating of the installed item being checked.</param>
        /// <param name="objFilter">Parsed rating requirements from the condition node.</param>
        /// <returns><c>true</c> if the rating satisfies the filter (or no rating filter is defined).</returns>
        private static bool MeetsRequirementRatingFilter(int intRating, RequirementRatingFilter objFilter)
        {
            if (!objFilter.HasFilter)
                return true;
            if (objFilter.ExactRating.HasValue)
                return intRating == objFilter.ExactRating.Value;
            return intRating >= objFilter.MinRating && intRating <= objFilter.MaxRating;
        }

        /// <summary>
        /// How an installed cyberware/bioware requirement matches against character items.
        /// </summary>
        private enum WareMatchMode
        {
            /// <summary>Match by exact name or source id.</summary>
            ExactNameOrId,
            /// <summary>Match when the item name contains the condition text.</summary>
            NameContains,
            /// <summary>Match by cyberware/bioware category.</summary>
            Category
        }

        /// <summary>
        /// Returns whether an item's name or source id equals the condition text.
        /// </summary>
        /// <param name="strItemName">Installed item name.</param>
        /// <param name="strItemSourceId">Installed item source id.</param>
        /// <param name="strNodeInnerText">Name or id from the condition node.</param>
        /// <returns><c>true</c> if the name or source id matches.</returns>
        private static bool MatchesNameOrSourceId(string strItemName, string strItemSourceId, string strNodeInnerText)
        {
            return strItemName == strNodeInnerText
                   || string.Equals(strItemSourceId, strNodeInnerText, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns whether an installed item matches a condition's name/id and rating requirements.
        /// </summary>
        /// <param name="strItemName">Installed item name.</param>
        /// <param name="strItemSourceId">Installed item source id.</param>
        /// <param name="intRating">Installed item rating.</param>
        /// <param name="strNodeInnerText">Name or id from the condition node.</param>
        /// <param name="objRatingFilter">Parsed rating requirements from the condition node.</param>
        /// <returns><c>true</c> if the item satisfies both the identity and rating checks.</returns>
        private static bool CharacterItemMatchesNameOrIdAndRating(string strItemName, string strItemSourceId,
            int intRating, string strNodeInnerText, RequirementRatingFilter objRatingFilter)
        {
            return MatchesNameOrSourceId(strItemName, strItemSourceId, strNodeInnerText)
                   && MeetsRequirementRatingFilter(intRating, objRatingFilter);
        }

        /// <summary>
        /// Returns whether installed cyberware satisfies a ware requirement condition.
        /// </summary>
        /// <param name="objCyberware">Cyberware on the character to test.</param>
        /// <param name="strNodeInnerText">Name, id, category, or partial name from the condition node.</param>
        /// <param name="strSelectExtra">Optional <c>select</c> attribute value the item's <see cref="Cyberware.Extra"/> must match.</param>
        /// <param name="eSourceType">Expected <see cref="Cyberware.SourceType"/> when not checking same-parent children.</param>
        /// <param name="eMatchMode">How to compare <paramref name="strNodeInnerText"/> against the item.</param>
        /// <param name="objRatingFilter">Parsed rating requirements from the condition node.</param>
        /// <param name="blnSameParentChild">If <c>true</c>, only match mode and rating are evaluated (same-parent child check).</param>
        /// <returns><c>true</c> if the cyberware satisfies the condition.</returns>
        private static bool InstalledCyberwareMatchesRequirement(Cyberware objCyberware, string strNodeInnerText,
            string strSelectExtra, Improvement.ImprovementSource eSourceType, WareMatchMode eMatchMode,
            RequirementRatingFilter objRatingFilter, bool blnSameParentChild)
        {
            if (!blnSameParentChild)
            {
                if (objCyberware.SourceType != eSourceType)
                    return false;
                if (!string.IsNullOrEmpty(objCyberware.PlugsIntoModularMount))
                    return false;
                if (!string.IsNullOrEmpty(strSelectExtra) && objCyberware.Extra != strSelectExtra)
                    return false;
            }

            if (!MeetsRequirementRatingFilter(objCyberware.Rating, objRatingFilter))
                return false;

            switch (eMatchMode)
            {
                case WareMatchMode.ExactNameOrId:
                    return MatchesNameOrSourceId(objCyberware.Name, objCyberware.SourceIDString, strNodeInnerText);
                case WareMatchMode.NameContains:
                    return objCyberware.Name.Contains(strNodeInnerText);
                case WareMatchMode.Category:
                    return objCyberware.Category == strNodeInnerText;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Async version of <see cref="InstalledCyberwareMatchesRequirement"/>.
        /// </summary>
        /// <param name="objCyberware">Cyberware on the character to test.</param>
        /// <param name="strNodeInnerText">Name, id, category, or partial name from the condition node.</param>
        /// <param name="strSelectExtra">Optional <c>select</c> attribute value the item's <see cref="Cyberware.Extra"/> must match.</param>
        /// <param name="eSourceType">Expected <see cref="Cyberware.SourceType"/> when not checking same-parent children.</param>
        /// <param name="eMatchMode">How to compare <paramref name="strNodeInnerText"/> against the item.</param>
        /// <param name="objRatingFilter">Parsed rating requirements from the condition node.</param>
        /// <param name="blnSameParentChild">If <c>true</c>, only match mode and rating are evaluated (same-parent child check).</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns><c>true</c> if the cyberware satisfies the condition.</returns>
        private static async Task<bool> InstalledCyberwareMatchesRequirementAsync(Cyberware objCyberware,
            string strNodeInnerText, string strSelectExtra, Improvement.ImprovementSource eSourceType,
            WareMatchMode eMatchMode, RequirementRatingFilter objRatingFilter, bool blnSameParentChild,
            CancellationToken token)
        {
            if (!blnSameParentChild)
            {
                if (await objCyberware.GetSourceTypeAsync(token).ConfigureAwait(false) != eSourceType)
                    return false;
                if (!string.IsNullOrEmpty(await objCyberware.GetPlugsIntoModularMountAsync(token).ConfigureAwait(false)))
                    return false;
                if (!string.IsNullOrEmpty(strSelectExtra) && objCyberware.Extra != strSelectExtra)
                    return false;
            }

            if (!MeetsRequirementRatingFilter(await objCyberware.GetRatingAsync(token).ConfigureAwait(false),
                    objRatingFilter))
                return false;

            switch (eMatchMode)
            {
                case WareMatchMode.ExactNameOrId:
                    return MatchesNameOrSourceId(objCyberware.Name,
                        await objCyberware.GetSourceIDStringAsync(token).ConfigureAwait(false), strNodeInnerText);
                case WareMatchMode.NameContains:
                    return objCyberware.Name.Contains(strNodeInnerText);
                case WareMatchMode.Category:
                    return objCyberware.Category == strNodeInnerText;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Builds the restriction message text for an installed cyberware/bioware requirement.
        /// </summary>
        /// <param name="blnSync">Flag for whether method should always use synchronous code or not.</param>
        /// <param name="objCharacter">Character whose data files are used for translation lookup.</param>
        /// <param name="strNodeInnerText">Name, id, category, or partial name from the condition node.</param>
        /// <param name="strSpace">Localized space character.</param>
        /// <param name="strLabelKey">LanguageManager key for the ware type label.</param>
        /// <param name="strDataFile">Data file to load (for example, <c>cyberware.xml</c>).</param>
        /// <param name="strItemXPathBase">XPath to the item node under <c>/chummer/</c> (for example, <c>cyberwares/cyberware</c>).</param>
        /// <param name="eMatchMode">How the condition text is resolved for display.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>Formatted restriction message text.</returns>
        private static async Task<string> BuildInstalledWareRequirementRestrictionMessageAsync(bool blnSync,
            Character objCharacter, string strNodeInnerText, string strSpace, string strLabelKey,
            string strDataFile, string strItemXPathBase, WareMatchMode eMatchMode, CancellationToken token)
        {
            if (blnSync)
            {
                return Utils.SafelyRunSynchronously(
                    () => BuildInstalledWareRequirementRestrictionMessageCoreAsync(true, objCharacter, strNodeInnerText,
                        strSpace, strLabelKey, strDataFile, strItemXPathBase, eMatchMode, token), token);
            }

            return await BuildInstalledWareRequirementRestrictionMessageCoreAsync(false, objCharacter, strNodeInnerText,
                strSpace, strLabelKey, strDataFile, strItemXPathBase, eMatchMode, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Core implementation for <see cref="BuildInstalledWareRequirementRestrictionMessageAsync"/>.
        /// </summary>
        private static async Task<string> BuildInstalledWareRequirementRestrictionMessageCoreAsync(bool blnSync,
            Character objCharacter, string strNodeInnerText, string strSpace, string strLabelKey,
            string strDataFile, string strItemXPathBase, WareMatchMode eMatchMode, CancellationToken token)
        {
            XPathNavigator objLoopDoc = blnSync
                // ReSharper disable once MethodHasAsyncOverload
                ? objCharacter.LoadDataXPath(strDataFile, token: token)
                : await objCharacter.LoadDataXPathAsync(strDataFile, token: token).ConfigureAwait(false);
            string strTranslate;
            if (eMatchMode == WareMatchMode.Category)
            {
                strTranslate = objLoopDoc
                    .SelectSingleNode("/chummer/categories/category[. = " + strNodeInnerText.CleanXPath()
                                      + "]/translate")?.Value;
            }
            else if (eMatchMode == WareMatchMode.ExactNameOrId)
            {
                strTranslate = objLoopDoc.SelectSingleNode("/chummer/" + strItemXPathBase + "[id = "
                                                           + strNodeInnerText.CleanXPath() + "]/translate")?.Value
                               ?? objLoopDoc.SelectSingleNode("/chummer/" + strItemXPathBase + "[name = "
                                                              + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
            }
            else
            {
                strTranslate = objLoopDoc.SelectSingleNode("/chummer/" + strItemXPathBase + "[name = "
                                                           + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
            }

            string strLabel = blnSync
                // ReSharper disable once MethodHasAsyncOverload
                ? LanguageManager.GetString(strLabelKey, token: token)
                : await LanguageManager.GetStringAsync(strLabelKey, token: token).ConfigureAwait(false);
            string strDisplay = !string.IsNullOrEmpty(strTranslate)
                ? strTranslate
                : eMatchMode == WareMatchMode.ExactNameOrId && strNodeInnerText.IsGuid()
                    ? objLoopDoc.SelectSingleNode("/chummer/" + strItemXPathBase + "[id = "
                                                  + strNodeInnerText.CleanXPath() + "]/name")?.Value
                      ?? strNodeInnerText
                    : strNodeInnerText;
            return Environment.NewLine + "\t" + strLabel + strSpace + strDisplay;
        }

        /// <summary>
        /// Builds the restriction message text for a gear, power, armor mod, or similar data-file item requirement.
        /// </summary>
        /// <param name="blnSync">Flag for whether method should always use synchronous code or not.</param>
        /// <param name="objCharacter">Character whose data files are used for translation lookup.</param>
        /// <param name="strNodeInnerText">Name or id from the condition node.</param>
        /// <param name="strSpace">Localized space character.</param>
        /// <param name="strDataFile">Data file to load (for example, <c>gear.xml</c>).</param>
        /// <param name="strItemXPathBase">XPath to the item node under <c>/chummer/</c> (for example, <c>gears/gear</c>).</param>
        /// <param name="strTypeLabelKey">LanguageManager key appended in parentheses (for example, <c>String_Gear</c>).</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>Formatted restriction message text.</returns>
        private static async Task<string> BuildDataItemRequirementRestrictionMessageAsync(bool blnSync,
            Character objCharacter, string strNodeInnerText, string strSpace, string strDataFile,
            string strItemXPathBase, string strTypeLabelKey, CancellationToken token)
        {
            if (blnSync)
            {
                return Utils.SafelyRunSynchronously(
                    () => BuildDataItemRequirementRestrictionMessageCoreAsync(true, objCharacter, strNodeInnerText,
                        strSpace, strDataFile, strItemXPathBase, strTypeLabelKey, token), token);
            }

            return await BuildDataItemRequirementRestrictionMessageCoreAsync(false, objCharacter, strNodeInnerText,
                strSpace, strDataFile, strItemXPathBase, strTypeLabelKey, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Core implementation for <see cref="BuildDataItemRequirementRestrictionMessageAsync"/>.
        /// </summary>
        private static async Task<string> BuildDataItemRequirementRestrictionMessageCoreAsync(bool blnSync,
            Character objCharacter, string strNodeInnerText, string strSpace, string strDataFile,
            string strItemXPathBase, string strTypeLabelKey, CancellationToken token)
        {
            XPathNavigator objLoopDoc = blnSync
                // ReSharper disable once MethodHasAsyncOverload
                ? objCharacter.LoadDataXPath(strDataFile, token: token)
                : await objCharacter.LoadDataXPathAsync(strDataFile, token: token).ConfigureAwait(false);
            string strTranslate
                = objLoopDoc.SelectSingleNode("/chummer/" + strItemXPathBase + "[id = "
                                              + strNodeInnerText.CleanXPath() + "]/translate")?.Value
                  ?? objLoopDoc.SelectSingleNode("/chummer/" + strItemXPathBase + "[name = "
                                                 + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
            string strTypeLabel = blnSync
                // ReSharper disable once MethodHasAsyncOverload
                ? LanguageManager.GetString(strTypeLabelKey, token: token)
                : await LanguageManager.GetStringAsync(strTypeLabelKey, token: token).ConfigureAwait(false);
            return Environment.NewLine + "\t"
                   + (!string.IsNullOrEmpty(strTranslate)
                       ? strTranslate
                       : strNodeInnerText.IsGuid()
                           ? objLoopDoc.SelectSingleNode("/chummer/" + strItemXPathBase + "[id = "
                                                         + strNodeInnerText.CleanXPath() + "]/name")?.Value
                             ?? strNodeInnerText
                           : strNodeInnerText) + strSpace + "(" + strTypeLabel + ")";
        }

        /// <summary>
        /// How grouped requirement nodes aggregate child requirement results.
        /// </summary>
        private enum RequirementGroupMode
        {
            /// <summary>All child requirements must be met (<c>group</c>).</summary>
            AllOf,
            /// <summary>At least one child requirement must be met (<c>grouponeof</c>).</summary>
            OneOf
        }

        /// <summary>
        /// Returns whether an installed quality entry matches a requirement node's name/id, optional <c>extra</c>, and ignore rules.
        /// </summary>
        /// <param name="strItemName">Installed quality name.</param>
        /// <param name="strItemSourceId">Installed quality source id.</param>
        /// <param name="strItemExtra">Installed quality <c>extra</c> value.</param>
        /// <param name="strNodeInnerText">Name or id from the condition node.</param>
        /// <param name="strRequiredExtra">Required <c>extra</c> attribute from the condition node, if any.</param>
        /// <param name="strIgnoreQuality">Quality name or id to ignore during the search.</param>
        /// <returns><c>true</c> if the entry satisfies the requirement.</returns>
        private static bool InstalledQualityEntryMatchesRequirement(string strItemName, string strItemSourceId,
            string strItemExtra, string strNodeInnerText, string strRequiredExtra, string strIgnoreQuality)
        {
            if (!MatchesNameOrSourceId(strItemName, strItemSourceId, strNodeInnerText))
                return false;
            if (strItemName == strIgnoreQuality || strItemSourceId == strIgnoreQuality)
                return false;
            if (!string.IsNullOrEmpty(strRequiredExtra) && strItemExtra != strRequiredExtra)
                return false;
            return true;
        }

        /// <summary>
        /// Evaluates <c>group</c> and <c>grouponeof</c> requirement nodes.
        /// </summary>
        /// <param name="blnSync">Flag for whether method should always use synchronous code or not.</param>
        /// <param name="xmlNode">Grouped requirement node whose element children are evaluated.</param>
        /// <param name="objCharacter">Character against which child requirements are checked.</param>
        /// <param name="objParent">Parent object passed to child requirement checks.</param>
        /// <param name="strIgnoreQuality">Quality name or id child checks should ignore.</param>
        /// <param name="eMode">Whether all or one child requirements must be met.</param>
        /// <param name="blnShowMessage">Whether to build aggregated restriction message text.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>Whether the grouped requirement is met and any restriction message text.</returns>
        private static async Task<ValueTuple<bool, string>> TestGroupedRequirementsCoreAsync(bool blnSync,
            XPathNavigator xmlNode, Character objCharacter, object objParent, string strIgnoreQuality,
            RequirementGroupMode eMode, bool blnShowMessage, CancellationToken token)
        {
            bool blnResult = eMode == RequirementGroupMode.AllOf;
            string strName = string.Empty;
            string strHeaderKey = eMode == RequirementGroupMode.AllOf
                ? "Message_SelectQuality_AllOf"
                : "Message_SelectQuality_OneOf";
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                       out StringBuilder sbdResultName))
            {
                sbdResultName.AppendLine().Append('\t')
                    .Append(blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? LanguageManager.GetString(strHeaderKey, token: token)
                        : await LanguageManager.GetStringAsync(strHeaderKey, token: token).ConfigureAwait(false));
                foreach (XPathNavigator xmlChildNode in xmlNode.SelectChildren(XPathNodeType.Element))
                {
                    (bool blnLoopResult, string strLoopResult) = blnSync
                        ? Utils.SafelyRunSynchronously(
                            () => xmlChildNode.TestNodeRequirementsAsync(true, objCharacter, objParent,
                                strIgnoreQuality, blnShowMessage, token), token)
                        : await xmlChildNode.TestNodeRequirementsAsync(false, objCharacter, objParent,
                            strIgnoreQuality, blnShowMessage, token).ConfigureAwait(false);
                    if (eMode == RequirementGroupMode.AllOf)
                    {
                        blnResult = blnResult && blnLoopResult;
                        if (!blnResult && !blnShowMessage)
                            break;
                        if (!blnLoopResult)
                        {
                            sbdResultName.Append(
                                strLoopResult.Replace(Environment.NewLine + "\t",
                                    Environment.NewLine + "\t\t"));
                        }
                    }
                    else
                    {
                        blnResult = blnLoopResult || blnResult;
                        if (blnResult && !blnShowMessage)
                            break;
                        sbdResultName.Append(
                            strLoopResult.Replace(Environment.NewLine + "\t",
                                Environment.NewLine + "\t\t"));
                    }
                }

                if (blnShowMessage)
                    strName = sbdResultName.ToString();
            }

            return new ValueTuple<bool, string>(blnResult, strName);
        }

        /// <summary>
        /// Evaluates <c>quality</c> and <c>characterquality</c> requirement nodes.
        /// </summary>
        /// <param name="blnSync">Flag for whether method should always use synchronous code or not.</param>
        /// <param name="xmlNode">Requirement condition node to evaluate.</param>
        /// <param name="objCharacter">Character whose qualities are searched.</param>
        /// <param name="strNodeInnerText">Inner text of the condition node.</param>
        /// <param name="strSpace">Localized space character.</param>
        /// <param name="strIgnoreQuality">Quality name or id to ignore during the search.</param>
        /// <param name="blnShowMessage">Whether to build restriction message text.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>Whether the requirement is met and any restriction message text.</returns>
        private static async Task<ValueTuple<bool, string>> TestInstalledCharacterQualityRequirementCoreAsync(
            bool blnSync, XPathNavigator xmlNode, Character objCharacter, string strNodeInnerText, string strSpace,
            string strIgnoreQuality, bool blnShowMessage, CancellationToken token)
        {
            string strExtra = xmlNode.SelectSingleNodeAndCacheExpression("@extra", token)?.Value ?? string.Empty;
            Quality objQuality = blnSync
                // ReSharper disable once MethodHasAsyncOverload
                ? objCharacter.Qualities.FirstOrDefault(
                    q => InstalledQualityEntryMatchesRequirement(q.Name, q.SourceIDString, q.Extra, strNodeInnerText,
                        strExtra, strIgnoreQuality))
                : await (await objCharacter.GetQualitiesAsync(token).ConfigureAwait(false)).FirstOrDefaultAsync(
                    q => InstalledQualityEntryMatchesRequirement(q.Name, q.SourceIDString, q.Extra, strNodeInnerText,
                        strExtra, strIgnoreQuality), token).ConfigureAwait(false);

            if (objQuality != null)
            {
                string strName = string.Empty;
                if (blnShowMessage)
                    strName = blnSync
                        ? objQuality.CurrentDisplayName
                        : await objQuality.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                return new ValueTuple<bool, string>(true, strName);
            }

            if (!blnShowMessage)
                return new ValueTuple<bool, string>(false, string.Empty);
            return new ValueTuple<bool, string>(false,
                await BuildDataItemRequirementRestrictionMessageAsync(blnSync, objCharacter, strNodeInnerText, strSpace,
                    "qualities.xml", "qualities/quality", "String_Quality", token).ConfigureAwait(false));
        }

        /// <summary>
        /// Evaluates <c>lifestylequality</c> requirement nodes.
        /// </summary>
        /// <param name="blnSync">Flag for whether method should always use synchronous code or not.</param>
        /// <param name="xmlNode">Requirement condition node to evaluate.</param>
        /// <param name="objCharacter">Character whose lifestyles are searched when no parent lifestyle is provided.</param>
        /// <param name="objParent">Parent lifestyle to search, if applicable.</param>
        /// <param name="strNodeInnerText">Inner text of the condition node.</param>
        /// <param name="strSpace">Localized space character.</param>
        /// <param name="strIgnoreQuality">Quality name or id to ignore during the search.</param>
        /// <param name="blnShowMessage">Whether to build restriction message text.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>Whether the requirement is met and any restriction message text.</returns>
        private static async Task<ValueTuple<bool, string>> TestInstalledLifestyleQualityRequirementCoreAsync(
            bool blnSync, XPathNavigator xmlNode, Character objCharacter, object objParent, string strNodeInnerText,
            string strSpace, string strIgnoreQuality, bool blnShowMessage, CancellationToken token)
        {
            string strExtra = xmlNode.SelectSingleNodeAndCacheExpression("@extra", token)?.Value ?? string.Empty;
            LifestyleQuality objQuality = null;
            if (blnSync)
            {
                IEnumerable<LifestyleQuality> lstLifestyleQualities = objParent is Lifestyle objLifestyle
                    ? objLifestyle.LifestyleQualities
                    : objCharacter.Lifestyles.SelectMany(x => x.LifestyleQualities);
                objQuality = lstLifestyleQualities.FirstOrDefault(
                    q => InstalledQualityEntryMatchesRequirement(q.Name, q.SourceIDString, q.Extra, strNodeInnerText,
                        strExtra, strIgnoreQuality));
            }
            else if (objParent is Lifestyle objLifestyle)
            {
                objQuality = await objLifestyle.LifestyleQualities.FirstOrDefaultAsync(
                    q => InstalledQualityEntryMatchesRequirement(q.Name, q.SourceIDString, q.Extra, strNodeInnerText,
                        strExtra, strIgnoreQuality), token).ConfigureAwait(false);
            }
            else
            {
                await (await objCharacter.GetLifestylesAsync(token).ConfigureAwait(false)).ForEachWithBreakAsync(
                    async x =>
                    {
                        LifestyleQuality objLoopQuality = await x.LifestyleQualities.FirstOrDefaultAsync(
                            q => InstalledQualityEntryMatchesRequirement(q.Name, q.SourceIDString, q.Extra,
                                strNodeInnerText, strExtra, strIgnoreQuality), token).ConfigureAwait(false);
                        if (objLoopQuality == null)
                            return true;
                        objQuality = objLoopQuality;
                        return false;
                    }, token).ConfigureAwait(false);
            }

            if (objQuality != null)
            {
                string strName = string.Empty;
                if (blnShowMessage)
                    strName = blnSync
                        ? objQuality.CurrentDisplayName
                        : await objQuality.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                return new ValueTuple<bool, string>(true, strName);
            }

            if (!blnShowMessage)
                return new ValueTuple<bool, string>(false, string.Empty);
            return new ValueTuple<bool, string>(false,
                await BuildDataItemRequirementRestrictionMessageAsync(blnSync, objCharacter, strNodeInnerText, strSpace,
                    "lifestyles.xml", "qualities/quality", "String_Quality", token).ConfigureAwait(false));
        }

        /// <summary>
        /// Installed character item types evaluated by <see cref="TestInstalledDataItemRequirementCoreAsync"/>.
        /// </summary>
        private enum InstalledDataItemRequirementKind
        {
            /// <summary>Gear on the character or under a gear parent.</summary>
            Gear,
            /// <summary>Adept powers on the character.</summary>
            Power,
            /// <summary>Armor mods on armor or under an armor parent.</summary>
            ArmorMod
        }

        /// <summary>
        /// Evaluates installed gear, power, or armor mod requirement nodes with optional rating and <c>sameparent</c> checks.
        /// </summary>
        /// <param name="blnSync">Flag for whether method should always use synchronous code or not.</param>
        /// <param name="xmlNode">Requirement condition node to evaluate.</param>
        /// <param name="objCharacter">Character whose equipment is searched.</param>
        /// <param name="objParent">Parent object for <c>sameparent</c> checks.</param>
        /// <param name="eKind">Which installed item type to evaluate.</param>
        /// <param name="strNodeInnerText">Inner text of the condition node.</param>
        /// <param name="strSpace">Localized space character.</param>
        /// <param name="blnShowMessage">Whether to build restriction message text.</param>
        /// <param name="strDataFile">Data file used when building restriction messages.</param>
        /// <param name="strItemXPathBase">XPath to the item node under <c>/chummer/</c> for message lookup.</param>
        /// <param name="strTypeLabelKey">LanguageManager key appended in parentheses on failure messages.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>Whether the requirement is met and any restriction message text.</returns>
        private static async Task<ValueTuple<bool, string>> TestInstalledDataItemRequirementCoreAsync(bool blnSync,
            XPathNavigator xmlNode, Character objCharacter, object objParent,
            InstalledDataItemRequirementKind eKind, string strNodeInnerText, string strSpace, bool blnShowMessage,
            string strDataFile, string strItemXPathBase, string strTypeLabelKey, CancellationToken token)
        {
            RequirementRatingFilter objRatingFilter = GetRequirementRatingFilter(xmlNode, token);
            string strName = string.Empty;

            if (eKind == InstalledDataItemRequirementKind.ArmorMod && blnShowMessage)
            {
                strName = await BuildDataItemRequirementRestrictionMessageAsync(blnSync, objCharacter, strNodeInnerText,
                    strSpace, strDataFile, strItemXPathBase, strTypeLabelKey, token).ConfigureAwait(false);
            }

            switch (eKind)
            {
                case InstalledDataItemRequirementKind.Gear:
                {
                    if (xmlNode.GetAttribute("sameparent", string.Empty) == bool.TrueString)
                    {
                        return new ValueTuple<bool, string>(objParent is IHasGear objGearParent && (blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? objGearParent.GearChildren.Any(
                                    x => CharacterItemMatchesNameOrIdAndRating(x.Name, x.SourceIDString, x.Rating,
                                        strNodeInnerText, objRatingFilter), token)
                                : await objGearParent.GearChildren.AnyAsync(
                                    async x => CharacterItemMatchesNameOrIdAndRating(x.Name, x.SourceIDString,
                                        await x.GetRatingAsync(token).ConfigureAwait(false), strNodeInnerText,
                                        objRatingFilter), token).ConfigureAwait(false)),
                            strName);
                    }

                    Gear objGear = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? objCharacter.Gear.FirstOrDefault(
                            x => CharacterItemMatchesNameOrIdAndRating(x.Name, x.SourceIDString, x.Rating,
                                strNodeInnerText, objRatingFilter))
                        : await (await objCharacter.GetGearAsync(token).ConfigureAwait(false)).FirstOrDefaultAsync(
                            async x => CharacterItemMatchesNameOrIdAndRating(x.Name, x.SourceIDString,
                                await x.GetRatingAsync(token).ConfigureAwait(false), strNodeInnerText,
                                objRatingFilter), token).ConfigureAwait(false);

                    if (objGear != null)
                    {
                        if (blnShowMessage)
                            strName = blnSync
                                ? objGear.CurrentDisplayNameShort
                                : await objGear.GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false);
                        return new ValueTuple<bool, string>(true, strName);
                    }

                    break;
                }
                case InstalledDataItemRequirementKind.Power:
                {
                    Power objPower = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? objCharacter.Powers.FirstOrDefault(
                            x => CharacterItemMatchesNameOrIdAndRating(x.Name, x.SourceIDString, x.Rating,
                                strNodeInnerText, objRatingFilter))
                        : await (await objCharacter.GetPowersAsync(token).ConfigureAwait(false)).FirstOrDefaultAsync(
                            async x => CharacterItemMatchesNameOrIdAndRating(x.Name, x.SourceIDString,
                                await x.GetRatingAsync(token).ConfigureAwait(false), strNodeInnerText,
                                objRatingFilter), token).ConfigureAwait(false);

                    if (objPower != null)
                    {
                        if (blnShowMessage)
                            strName = blnSync
                                ? objPower.CurrentDisplayName
                                : await objPower.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                        return new ValueTuple<bool, string>(true, strName);
                    }

                    break;
                }
                case InstalledDataItemRequirementKind.ArmorMod:
                {
                    if (xmlNode.GetAttribute("sameparent", string.Empty) == bool.TrueString)
                    {
                        return new ValueTuple<bool, string>(objParent is Armor objArmor && (blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? objArmor.ArmorMods.Any(
                                    x => CharacterItemMatchesNameOrIdAndRating(x.Name, x.SourceIDString, x.Rating,
                                        strNodeInnerText, objRatingFilter), token)
                                : await objArmor.ArmorMods.AnyAsync(
                                    async x => CharacterItemMatchesNameOrIdAndRating(x.Name, x.SourceIDString,
                                        await x.GetRatingAsync(token).ConfigureAwait(false), strNodeInnerText,
                                        objRatingFilter), token).ConfigureAwait(false)),
                            strName);
                    }

                    return new ValueTuple<bool, string>(
                        blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? objCharacter.Armor.Any(
                                x => x.ArmorMods.Any(y => CharacterItemMatchesNameOrIdAndRating(y.Name,
                                    y.SourceIDString, y.Rating, strNodeInnerText, objRatingFilter), token), token)
                            : await objCharacter.Armor.AnyAsync(
                                x => x.ArmorMods.AnyAsync(
                                    async y => CharacterItemMatchesNameOrIdAndRating(y.Name, y.SourceIDString,
                                        await y.GetRatingAsync(token).ConfigureAwait(false), strNodeInnerText,
                                        objRatingFilter), token),
                                token).ConfigureAwait(false), strName);
                }
                default:
                    Utils.BreakIfDebug();
                    break;
            }

            if (blnShowMessage)
            {
                strName = await BuildDataItemRequirementRestrictionMessageAsync(blnSync, objCharacter, strNodeInnerText,
                    strSpace, strDataFile, strItemXPathBase, strTypeLabelKey, token).ConfigureAwait(false);
            }

            return new ValueTuple<bool, string>(false, strName);
        }

        /// <summary>
        /// Evaluates installed cyberware/bioware requirement nodes (<c>cyberware</c>, <c>bioware</c>, and related contains/category nodes).
        /// </summary>
        /// <param name="blnSync">Flag for whether method should always use synchronous code or not.</param>
        /// <param name="xmlNode">Requirement condition node to evaluate.</param>
        /// <param name="objCharacter">Character whose cyberware is searched.</param>
        /// <param name="objParent">Parent object for <c>sameparent</c> checks (expected to be <see cref="Cyberware"/>).</param>
        /// <param name="eSourceType"><see cref="Improvement.ImprovementSource"/> to match when counting character items.</param>
        /// <param name="eMatchMode">How to compare the condition text against installed items.</param>
        /// <param name="strLabelKey">LanguageManager key for restriction message labels.</param>
        /// <param name="strDataFile">Data file used when building restriction messages.</param>
        /// <param name="strItemXPathBase">XPath to the item node under <c>/chummer/</c> for message lookup.</param>
        /// <param name="blnAllowSameParent">Whether <c>sameparent</c> may be evaluated on <paramref name="objParent"/>.</param>
        /// <param name="strNodeInnerText">Inner text of the condition node.</param>
        /// <param name="strSpace">Localized space character.</param>
        /// <param name="blnShowMessage">Whether to build a restriction message when the check fails.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>Whether the requirement is met and any restriction message text.</returns>
        private static async Task<ValueTuple<bool, string>> TestInstalledWareRequirementCoreAsync(bool blnSync,
            XPathNavigator xmlNode, Character objCharacter, object objParent,
            Improvement.ImprovementSource eSourceType, WareMatchMode eMatchMode, string strLabelKey,
            string strDataFile, string strItemXPathBase, bool blnAllowSameParent, string strNodeInnerText,
            string strSpace, bool blnShowMessage, CancellationToken token)
        {
            int intCount = xmlNode.SelectSingleNodeAndCacheExpression("@count", token)?.ValueAsInt ?? 1;
            RequirementRatingFilter objRatingFilter = GetRequirementRatingFilter(xmlNode, token);
            string strName = string.Empty;
            if (blnShowMessage)
            {
                strName = await BuildInstalledWareRequirementRestrictionMessageAsync(blnSync, objCharacter,
                    strNodeInnerText, strSpace, strLabelKey, strDataFile, strItemXPathBase, eMatchMode, token)
                    .ConfigureAwait(false);
            }

            string strWareNodeSelectAttribute
                = xmlNode.SelectSingleNodeAndCacheExpression("@select", token)?.Value ?? string.Empty;

            if (blnAllowSameParent && xmlNode.GetAttribute("sameparent", string.Empty) == bool.TrueString)
            {
                if (objParent is Cyberware objCyberware)
                {
                    bool blnResult = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? objCyberware.Children.Any(
                            mod => InstalledCyberwareMatchesRequirement(mod, strNodeInnerText,
                                strWareNodeSelectAttribute, eSourceType, eMatchMode, objRatingFilter, true), token)
                        : await (await objCyberware.GetChildrenAsync(token).ConfigureAwait(false))
                            .AnyAsync(
                                mod => InstalledCyberwareMatchesRequirementAsync(mod, strNodeInnerText,
                                    strWareNodeSelectAttribute, eSourceType, eMatchMode, objRatingFilter, true,
                                    token), token).ConfigureAwait(false);
                    return new ValueTuple<bool, string>(blnResult, strName);
                }

                return new ValueTuple<bool, string>(false, strName);
            }

            if (blnSync)
            {
                int intMatchCount = objCharacter.Cyberware.DeepCount(x => x.Children, objCyberware =>
                    InstalledCyberwareMatchesRequirement(objCyberware, strNodeInnerText, strWareNodeSelectAttribute,
                        eSourceType, eMatchMode, objRatingFilter, false));
                return new ValueTuple<bool, string>(intMatchCount >= intCount, strName);
            }

            int intAsyncMatchCount = await (await objCharacter.GetCyberwareAsync(token).ConfigureAwait(false))
                .DeepCountAsync(x => x.GetChildrenAsync(token), async objCyberware =>
                        await InstalledCyberwareMatchesRequirementAsync(objCyberware, strNodeInnerText,
                            strWareNodeSelectAttribute, eSourceType, eMatchMode, objRatingFilter, false, token)
                            .ConfigureAwait(false),
                    token).ConfigureAwait(false);
            return new ValueTuple<bool, string>(intAsyncMatchCount >= intCount, strName);
        }

        /// <summary>
        ///     Evaluates the availability of a given node against Availability Limits in Create Mode
        /// </summary>
        public static bool CheckAvailRestriction(XmlNode objXmlGear, Character objCharacter, int intRating = 1, int intAvailModifier = 0, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return objXmlGear?.CreateNavigator().CheckAvailRestriction(objCharacter, intRating, intAvailModifier, token) == true;
        }

        /// <summary>
        ///     Evaluates the availability of a given node against Availability Limits in Create Mode
        /// </summary>
        /// <param name="objXmlGear">XPathNavigator element to evaluate.</param>
        /// <param name="objCharacter">Character that we're comparing the Availability against.</param>
        /// <param name="intRating">Effective Rating of the object.</param>
        /// <param name="intAvailModifier">Availability Modifier from other sources.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>Returns False if not permitted with the current gameplay restrictions. Returns True if valid.</returns>
        public static bool CheckAvailRestriction(this XPathNavigator objXmlGear, Character objCharacter, int intRating = 1, int intAvailModifier = 0, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objXmlGear == null)
                return false;
            //TODO: Better handler for restricted gear
            if (objCharacter?.Created != false || objCharacter.RestrictedGear > 0 || objCharacter.IgnoreRules)
                return true;
            // Avail.

            XPathNavigator objAvailNode = objXmlGear.SelectSingleNodeAndCacheExpression("avail", token);
            if (objAvailNode == null)
            {
                int intHighestAvailNode = 0;
                foreach (XPathNavigator objLoopNode in objXmlGear.SelectChildren(XPathNodeType.Element))
                {
                    token.ThrowIfCancellationRequested();
                    if (!objLoopNode.Name.StartsWith("avail", StringComparison.Ordinal))
                        continue;
                    string strLoopCostString = objLoopNode.Name.Substring(5);
                    if (int.TryParse(strLoopCostString, out int intTmp))
                    {
                        intHighestAvailNode = Math.Max(intHighestAvailNode, intTmp);
                    }
                }
                objAvailNode = objXmlGear.SelectSingleNode("avail" + intHighestAvailNode.ToString(GlobalSettings.InvariantCultureInfo));
                for (int i = intRating; i <= intHighestAvailNode; ++i)
                {
                    token.ThrowIfCancellationRequested();
                    XPathNavigator objLoopNode = objXmlGear.SelectSingleNode("avail" + i.ToString(GlobalSettings.InvariantCultureInfo));
                    if (objLoopNode != null)
                    {
                        objAvailNode = objLoopNode;
                        break;
                    }
                }
            }

            // If avail contains "F" or "R", remove it from the string so we can use the expression.
            string strAvailExpr = objAvailNode?.Value ?? string.Empty;
            strAvailExpr = strAvailExpr.ProcessFixedValuesString(intRating)
                .Replace("{Rating}", intRating.ToString(GlobalSettings.InvariantCultureInfo))
                .Replace("Rating", intRating.ToString(GlobalSettings.InvariantCultureInfo));

            if (string.IsNullOrEmpty(strAvailExpr))
                return true;
            char chrFirstAvailChar = strAvailExpr[0];
            if (chrFirstAvailChar == '+' || chrFirstAvailChar == '-')
                return true;

            strAvailExpr = strAvailExpr.TrimEndOnce(" or Gear").TrimEndOnce('F', 'R');
            int intAvail = intAvailModifier;
            if (strAvailExpr.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
            {
                strAvailExpr = objCharacter.ProcessAttributesInXPath(strAvailExpr, token: token);
                (bool blnIsSuccess, object objProcess) = CommonFunctions.EvaluateInvariantXPath(strAvailExpr, token);
                if (blnIsSuccess)
                    intAvail += ((double)objProcess).StandardRound();
            }
            else
                intAvail += decValue.StandardRound();
            return intAvail <= objCharacter.Settings.MaximumAvailability;
        }

        /// <summary>
        ///     Evaluates the availability of a given node against Availability Limits in Create Mode
        /// </summary>
        public static async Task<bool> CheckAvailRestrictionAsync(XmlNode objXmlGear, Character objCharacter, int intRating = 1, int intAvailModifier = 0, CancellationToken token = default)
        {
            return objXmlGear != null && await objXmlGear.CreateNavigator().CheckAvailRestrictionAsync(objCharacter, intRating, intAvailModifier, token).ConfigureAwait(false);
        }

        /// <summary>
        ///     Evaluates the availability of a given node against Availability Limits in Create Mode
        /// </summary>
        /// <param name="objXmlGear">XPathNavigator element to evaluate.</param>
        /// <param name="objCharacter">Character that we're comparing the Availability against.</param>
        /// <param name="intRating">Effective Rating of the object.</param>
        /// <param name="intAvailModifier">Availability Modifier from other sources.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>Returns False if not permitted with the current gameplay restrictions. Returns True if valid.</returns>
        public static async Task<bool> CheckAvailRestrictionAsync(this XPathNavigator objXmlGear, Character objCharacter, int intRating = 1, int intAvailModifier = 0, CancellationToken token = default)
        {
            if (objXmlGear == null)
                return false;
            if (objCharacter == null)
                return true;
            //TODO: Better handler for restricted gear
            if (await objCharacter.GetCreatedAsync(token).ConfigureAwait(false) || await objCharacter.GetRestrictedGearAsync(token).ConfigureAwait(false) > 0 || await objCharacter.GetIgnoreRulesAsync(token).ConfigureAwait(false))
                return true;
            // Avail.

            XPathNavigator objAvailNode = objXmlGear.SelectSingleNodeAndCacheExpression("avail", token);
            if (objAvailNode == null)
            {
                int intHighestAvailNode = 0;
                foreach (XPathNavigator objLoopNode in objXmlGear.SelectChildren(XPathNodeType.Element))
                {
                    if (!objLoopNode.Name.StartsWith("avail", StringComparison.Ordinal))
                        continue;
                    string strLoopCostString = objLoopNode.Name.Substring(5);
                    if (int.TryParse(strLoopCostString, out int intTmp))
                    {
                        intHighestAvailNode = Math.Max(intHighestAvailNode, intTmp);
                    }
                }
                objAvailNode = objXmlGear.SelectSingleNode("avail" + intHighestAvailNode.ToString(GlobalSettings.InvariantCultureInfo));
                for (int i = intRating; i <= intHighestAvailNode; ++i)
                {
                    XPathNavigator objLoopNode = objXmlGear.SelectSingleNode("avail" + i.ToString(GlobalSettings.InvariantCultureInfo));
                    if (objLoopNode != null)
                    {
                        objAvailNode = objLoopNode;
                        break;
                    }
                }
            }

            // If avail contains "F" or "R", remove it from the string so we can use the expression.
            string strAvailExpr = objAvailNode?.Value ?? string.Empty;
            strAvailExpr = strAvailExpr.ProcessFixedValuesString(intRating)
                .Replace("{Rating}", intRating.ToString(GlobalSettings.InvariantCultureInfo))
                .Replace("Rating", intRating.ToString(GlobalSettings.InvariantCultureInfo));

            if (string.IsNullOrEmpty(strAvailExpr))
                return true;
            char chrFirstAvailChar = strAvailExpr[0];
            if (chrFirstAvailChar == '+' || chrFirstAvailChar == '-')
                return true;

            strAvailExpr = strAvailExpr.TrimEndOnce(" or Gear").TrimEndOnce('F', 'R');
            int intAvail = intAvailModifier;
            if (strAvailExpr.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
            {
                strAvailExpr = await objCharacter.ProcessAttributesInXPathAsync(strAvailExpr, token: token).ConfigureAwait(false);
                (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(strAvailExpr, token).ConfigureAwait(false);
                if (blnIsSuccess)
                    intAvail += ((double)objProcess).StandardRound();
            }
            else
                intAvail += decValue.StandardRound();
            return intAvail <= await (await objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetMaximumAvailabilityAsync(token).ConfigureAwait(false);
        }

        public static bool CheckNuyenRestriction(XmlNode objXmlGear, Character objCharacter, decimal decMaxNuyen, decimal decCostMultiplier = 1.0m, int intRating = 1, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return objXmlGear?.CreateNavigator().CheckNuyenRestriction(objCharacter, decMaxNuyen, decCostMultiplier, intRating, token) == true;
        }

        /// <summary>
        ///     Evaluates whether a given node can be purchased.
        /// </summary>
        /// <param name="objXmlGear">XPathNavigator element to evaluate.</param>
        /// <param name="objCharacter">Character to use for compound cost strings.</param>
        /// <param name="decMaxNuyen">Total nuyen amount that the character possesses.</param>
        /// <param name="decCostMultiplier">Multiplier of the object's cost value.</param>
        /// <param name="intRating">Effective Rating of the object.</param>
        /// <returns>Returns False if not permitted with the current restrictions. Returns True if valid.</returns>
        public static bool CheckNuyenRestriction(this XPathNavigator objXmlGear, Character objCharacter, decimal decMaxNuyen, decimal decCostMultiplier = 1.0m, int intRating = 1, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objXmlGear == null)
                return false;
            // Cost.
            decimal decCost = 0.0m;
            XPathNavigator objCostNode = objXmlGear.SelectSingleNodeAndCacheExpression("cost", token);
            if (objCostNode == null)
            {
                int intCostRating = 1;
                foreach (XmlNode objLoopNode in objXmlGear.SelectChildren(XPathNodeType.Element))
                {
                    token.ThrowIfCancellationRequested();
                    if (!objLoopNode.Name.StartsWith("cost", StringComparison.Ordinal))
                        continue;
                    string strLoopCostString = objLoopNode.Name.Substring(4);
                    if (int.TryParse(strLoopCostString, out int intTmp) && intTmp <= intRating)
                    {
                        intCostRating = Math.Max(intCostRating, intTmp);
                    }
                }

                objCostNode = objXmlGear.SelectSingleNode("cost" + intCostRating.ToString(GlobalSettings.InvariantCultureInfo));
            }
            string strCost = objCostNode?.Value;
            if (!string.IsNullOrEmpty(strCost))
            {
                strCost = strCost.ProcessFixedValuesString(intRating)
                    .Replace("{Rating}", intRating.ToString(GlobalSettings.InvariantCultureInfo))
                    .Replace("Rating", intRating.ToString(GlobalSettings.InvariantCultureInfo));
                if (strCost.StartsWith("Variable", StringComparison.Ordinal))
                {
                    strCost = strCost.TrimStartOnce("Variable(", true).TrimEndOnce(')');
                    int intHyphenIndex = strCost.IndexOf('-');
                    strCost = intHyphenIndex != -1 ? strCost.Substring(0, intHyphenIndex) : strCost.FastEscape('+');
                }
                if (strCost.DoesNeedXPathProcessingToBeConvertedToNumber(out decCost))
                {
                    strCost = objCharacter.ProcessAttributesInXPath(strCost, token: token);
                    (bool blnIsSuccess, object objProcess) = CommonFunctions.EvaluateInvariantXPath(strCost, token);
                    if (blnIsSuccess)
                        decCost = Convert.ToDecimal((double)objProcess);
                }
            }
            return decMaxNuyen >= decCost * decCostMultiplier;
        }

        public static async Task<bool> CheckNuyenRestrictionAsync(XmlNode objXmlGear, Character objCharacter, decimal decMaxNuyen, decimal decCostMultiplier = 1.0m, int intRating = 1, CancellationToken token = default)
        {
            return objXmlGear != null && await objXmlGear.CreateNavigator()
                                                         .CheckNuyenRestrictionAsync(
                                                             objCharacter, decMaxNuyen, decCostMultiplier, intRating, token).ConfigureAwait(false);
        }

        /// <summary>
        ///     Evaluates whether a given node can be purchased.
        /// </summary>
        /// <param name="objXmlGear">XPathNavigator element to evaluate.</param>
        /// <param name="objCharacter">Character to use for compound cost strings.</param>
        /// <param name="decMaxNuyen">Total nuyen amount that the character possesses.</param>
        /// <param name="decCostMultiplier">Multiplier of the object's cost value.</param>
        /// <param name="intRating">Effective Rating of the object.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>Returns False if not permitted with the current restrictions. Returns True if valid.</returns>
        public static async Task<bool> CheckNuyenRestrictionAsync(this XPathNavigator objXmlGear, Character objCharacter, decimal decMaxNuyen, decimal decCostMultiplier = 1.0m, int intRating = 1, CancellationToken token = default)
        {
            if (objXmlGear == null)
                return false;
            // Cost.
            decimal decCost = 0.0m;
            XPathNavigator objCostNode = objXmlGear.SelectSingleNodeAndCacheExpression("cost", token);
            if (objCostNode == null)
            {
                int intCostRating = 1;
                foreach (XmlNode objLoopNode in objXmlGear.SelectChildren(XPathNodeType.Element))
                {
                    if (!objLoopNode.Name.StartsWith("cost", StringComparison.Ordinal))
                        continue;
                    string strLoopCostString = objLoopNode.Name.Substring(4);
                    if (int.TryParse(strLoopCostString, out int intTmp) && intTmp <= intRating)
                    {
                        intCostRating = Math.Max(intCostRating, intTmp);
                    }
                }

                objCostNode = objXmlGear.SelectSingleNode("cost" + intCostRating.ToString(GlobalSettings.InvariantCultureInfo));
            }
            string strCost = objCostNode?.Value;
            if (!string.IsNullOrEmpty(strCost))
            {
                strCost = strCost.ProcessFixedValuesString(intRating)
                    .Replace("{Rating}", intRating.ToString(GlobalSettings.InvariantCultureInfo))
                    .Replace("Rating", intRating.ToString(GlobalSettings.InvariantCultureInfo));
                if (strCost.StartsWith("Variable", StringComparison.Ordinal))
                {
                    strCost = strCost.TrimStartOnce("Variable(", true).TrimEndOnce(')');
                    int intHyphenIndex = strCost.IndexOf('-');
                    strCost = intHyphenIndex != -1 ? strCost.Substring(0, intHyphenIndex) : strCost.FastEscape('+');
                }
                if (strCost.DoesNeedXPathProcessingToBeConvertedToNumber(out decCost))
                {
                    strCost = await objCharacter.ProcessAttributesInXPathAsync(strCost, token: token).ConfigureAwait(false);
                    (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(strCost, token).ConfigureAwait(false);
                    if (blnIsSuccess)
                        decCost = Convert.ToDecimal((double)objProcess);
                }
            }
            return decMaxNuyen >= decCost * decCostMultiplier;
        }
    }
}
