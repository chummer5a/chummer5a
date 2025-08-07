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
using iText.Kernel.Pdf.Canvas.Parser.ClipperLib;

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
                                string.Format(LanguageManager.GetString("MessageTitle_SelectGeneric_Restriction", token: token), strLocalName),
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            await Program.ShowScrollableMessageBoxAsync(
                                string.Format(GlobalSettings.CultureInfo,
                                    await LanguageManager.GetStringAsync("Message_SelectGeneric_ChargenRestriction", token: token)
                                        .ConfigureAwait(false),
                                    strLocalName),
                                string.Format(await LanguageManager.GetStringAsync("MessageTitle_SelectGeneric_Restriction", token: token)
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
                                string.Format(LanguageManager.GetString("MessageTitle_SelectGeneric_Restriction", token: token), strLocalName),
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            await Program.ShowScrollableMessageBoxAsync(
                                string.Format(GlobalSettings.CultureInfo,
                                    await LanguageManager.GetStringAsync("Message_SelectGeneric_CareerOnlyRestriction", token: token)
                                        .ConfigureAwait(false),
                                    strLocalName),
                                string.Format(await LanguageManager.GetStringAsync("MessageTitle_SelectGeneric_Restriction", token: token)
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
                                string.Format(LanguageManager.GetString("MessageTitle_SelectGeneric_Restriction", token: token), strLocalName),
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            await Program.ShowScrollableMessageBoxAsync(
                                string.Format(GlobalSettings.CultureInfo,
                                    await LanguageManager.GetStringAsync("Message_SelectGeneric_PriorityRestriction", token: token)
                                        .ConfigureAwait(false),
                                    strLocalName),
                                string.Format(await LanguageManager.GetStringAsync("MessageTitle_SelectGeneric_Restriction", token: token)
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
                    int intLimit = 1;
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
                                        sbdLimitString.CheapReplace(strLimitString, '{' + strLimb + '}',
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
                                        await sbdLimitString.CheapReplaceAsync(strLimitString, '{' + strLimb + '}',
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
                                = CommonFunctions.EvaluateInvariantXPath(strLimitString, token);
                            intLimit = blnIsSuccess ? ((double)objProcess).StandardRound() : 1;
                            // ReSharper restore MethodHasAsyncOverload
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
                    switch (xmlNode.Name)
                    {
                        case "characterquality":
                        case "quality":
                        {
                            objListToCheck = objCharacter.Qualities.Where(objQuality =>
                                objQuality.Name != strIgnoreQuality && objQuality.SourceIDString != strIgnoreQuality);
                            break;
                        }
                        case "lifestylequality":
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
                        case "echo":
                        case "metamagic":
                        {
                            objListToCheck = objCharacter.Metamagics;
                            break;
                        }
                        case "art":
                        {
                            objListToCheck = objCharacter.Arts;
                            break;
                        }
                        case "enhancement":
                        {
                            objListToCheck = objCharacter.Enhancements;
                            break;
                        }
                        case "power":
                        {
                            objListToCheck = objCharacter.Powers;
                            break;
                        }
                        case "critterpower":
                        {
                            objListToCheck = objCharacter.CritterPowers;
                            break;
                        }
                        case "martialart":
                        {
                            objListToCheck = objCharacter.MartialArts;
                            break;
                        }
                        case "technique":
                        {
                            objListToCheck = objParent is MartialArt objArt
                                ? objArt.Techniques
                                : objCharacter.MartialArts.SelectMany(x => x.Children);
                            break;
                        }
                        case "cyberware":
                        case "bioware":
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
                        intExtendedLimit = Convert.ToInt32(strLimitWithInclusions, GlobalSettings.InvariantCultureInfo);
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
                                    string.Format(LanguageManager.GetString("MessageTitle_SelectGeneric_Limit", token: token), strLocalName),
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                await Program.ShowScrollableMessageBoxAsync(
                                    string.Format(GlobalSettings.CultureInfo,
                                        await LanguageManager.GetStringAsync("Message_SelectGeneric_Limit", token: token)
                                            .ConfigureAwait(false),
                                        strLocalName, intLimit == 0 ? 1 : intLimit),
                                    string.Format(await LanguageManager.GetStringAsync("MessageTitle_SelectGeneric_Limit", token: token)
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
                                        string.Format(LanguageManager.GetString("MessageTitle_SelectGeneric_Restriction", token: token), strLocalName),
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                else
                                {
                                    await Program.ShowScrollableMessageBoxAsync(
                                        string.Format(GlobalSettings.CultureInfo,
                                            await LanguageManager.GetStringAsync("Message_SelectGeneric_Restriction", token: token)
                                                .ConfigureAwait(false),
                                            strLocalName) + strName,
                                        string.Format(await LanguageManager.GetStringAsync("MessageTitle_SelectGeneric_Restriction", token: token)
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
                                        strLocalName) + sbdRequirement,
                                    // ReSharper disable once MethodHasAsyncOverload
                                    string.Format(LanguageManager.GetString("MessageTitle_SelectGeneric_Restriction", token: token), strLocalName),
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                await Program.ShowScrollableMessageBoxAsync(
                                    string.Format(GlobalSettings.CultureInfo,
                                        await LanguageManager.GetStringAsync("Message_SelectGeneric_Restriction", token: token)
                                            .ConfigureAwait(false),
                                        strLocalName) + sbdRequirement,
                                    string.Format(await LanguageManager.GetStringAsync("MessageTitle_SelectGeneric_Restriction", token: token)
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

        private static async Task<Tuple<bool, string>> TestNodeRequirementsAsync(this XPathNavigator xmlNode,
            bool blnSync, Character objCharacter,
            object objParent, string strIgnoreQuality = "",
            bool blnShowMessage = true, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (xmlNode == null || objCharacter == null)
            {
                return new Tuple<bool, string>(false, string.Empty);
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
                            return new Tuple<bool, string>((objAttribute?.Value ?? 0) >= intTargetValue, strName);
                        }

                        return new Tuple<bool, string>((objAttribute?.TotalValue ?? 0) >= intTargetValue, strName);
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
                            return new Tuple<bool, string>(
                                (objAttribute != null
                                    ? await objAttribute.GetValueAsync(token).ConfigureAwait(false)
                                    : 0) >= intTargetValue, strName);
                        }

                        return new Tuple<bool, string>(
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
                            return new Tuple<bool, string>(
                                (blnIsSuccess ? ((double)objProcess).StandardRound() : 0) >= intNodeVal, strName);
                        }
                        else if (blnShowMessage)
                            strName = string.Format(GlobalSettings.CultureInfo, "{0}\t{2}{1}{3}", Environment.NewLine,
                                strSpace, decValue, intNodeVal);
                        return new Tuple<bool, string>(decValue >= intNodeVal, strName);
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
                            return new Tuple<bool, string>(
                                (blnIsSuccess ? ((double)objProcess).StandardRound() : 0) >= intNodeVal, strName);
                        }
                        else if (blnShowMessage)
                            strName = string.Format(GlobalSettings.CultureInfo, "{0}\t{2}{1}{3}", Environment.NewLine,
                                strSpace, decValue, intNodeVal);
                        return new Tuple<bool, string>(decValue >= intNodeVal, strName);
                    }
                }
                case "careerkarma":
                {
                    // Check Career Karma requirement.
                    if (blnShowMessage)
                        strName = Environment.NewLine + '\t' + string.Format(
                            GlobalSettings.CultureInfo, blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? LanguageManager.GetString("Message_SelectQuality_RequireKarma", token: token)
                                : await LanguageManager.GetStringAsync("Message_SelectQuality_RequireKarma",
                                    token: token).ConfigureAwait(false),
                            strNodeInnerText);
                    return new Tuple<bool, string>((blnSync
                        ? objCharacter.CareerKarma
                        : await objCharacter.GetCareerKarmaAsync(token).ConfigureAwait(false)) >= xmlNode.ValueAsInt, strName);
                }
                case "chargenonly":
                {
                    if (blnShowMessage)
                        strName = Environment.NewLine + '\t'
                                                      + (blnSync
                                                          // ReSharper disable once MethodHasAsyncOverload
                                                          ? LanguageManager.GetString(
                                                              "Message_SelectGeneric_ChargenRestriction", token: token)
                                                          : await LanguageManager.GetStringAsync(
                                                              "Message_SelectGeneric_ChargenRestriction",
                                                              token: token).ConfigureAwait(false));
                    return new Tuple<bool, string>(
                        !(blnSync
                            ? objCharacter.Created
                            : await objCharacter.GetCreatedAsync(token).ConfigureAwait(false)), strName);
                }
                case "careeronly":
                {
                    if (blnShowMessage)
                        strName = Environment.NewLine + '\t'
                                                      + (blnSync
                                                          // ReSharper disable once MethodHasAsyncOverload
                                                          ? LanguageManager.GetString(
                                                              "Message_SelectGeneric_CareerOnlyRestriction",
                                                              token: token)
                                                          : await LanguageManager.GetStringAsync(
                                                              "Message_SelectGeneric_CareerOnlyRestriction",
                                                              token: token).ConfigureAwait(false));
                    return new Tuple<bool, string>(
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
                                p => p.Name == strNodeInnerText || string.Equals(p.SourceIDString, strNodeInnerText,
                                    StringComparison.OrdinalIgnoreCase))
                            : await (await objCharacter.GetCritterPowersAsync(token).ConfigureAwait(false))
                                .FirstOrDefaultAsync(
                                    p => p.Name == strNodeInnerText || string.Equals(p.SourceIDString, strNodeInnerText,
                                        StringComparison.OrdinalIgnoreCase), token).ConfigureAwait(false);
                        if (objCritterPower != null)
                        {
                            if (blnShowMessage)
                                strName = blnSync
                                    ? objCritterPower.CurrentDisplayName
                                    : await objCritterPower.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                            return new Tuple<bool, string>(true, strName);
                        }
                    }

                    if (blnShowMessage)
                    {
                        XPathNavigator objLoopDoc = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? objCharacter.LoadDataXPath("critterpowers.xml", token: token)
                            : await objCharacter.LoadDataXPathAsync("critterpowers.xml", token: token)
                                .ConfigureAwait(false);
                        XPathNavigator objLoopNode
                            = objLoopDoc.TryGetNodeByNameOrId("/chummer/powers/power", strNodeInnerText);
                        string strTranslate =
                            objLoopNode?.SelectSingleNodeAndCacheExpression("translate", token)?.Value ?? string.Empty;
                        strName = Environment.NewLine + '\t'
                                                      + (!string.IsNullOrEmpty(strTranslate)
                                                          ? strTranslate
                                                          : strNodeInnerText.IsGuid()
                                                              ? objLoopNode?
                                                                    .SelectSingleNodeAndCacheExpression(
                                                                        "name", token)?.Value
                                                                ?? strNodeInnerText
                                                              : strNodeInnerText) + strSpace + '('
                                                      + (blnSync
                                                          // ReSharper disable once MethodHasAsyncOverload
                                                          ? LanguageManager.GetString(
                                                              "Tab_Critter",
                                                              token: token)
                                                          : await LanguageManager.GetStringAsync(
                                                              "Tab_Critter", token: token).ConfigureAwait(false)) + ')';
                    }

                    return new Tuple<bool, string>(false, strName);
                }
                case "bioware":
                {
                    int intCount
                        = xmlNode.SelectSingleNodeAndCacheExpression("@count", token)?.ValueAsInt ?? 1;
                    if (blnShowMessage)
                    {
                        XPathNavigator objLoopDoc = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? objCharacter.LoadDataXPath("bioware.xml", token: token)
                            : await objCharacter.LoadDataXPathAsync("bioware.xml", token: token).ConfigureAwait(false);
                        string strTranslate
                            = objLoopDoc.SelectSingleNode("/chummer/biowares/bioware[id = "
                                                          + strNodeInnerText.CleanXPath() + "]/translate")?.Value
                              ?? objLoopDoc
                                  .SelectSingleNode("/chummer/biowares/bioware[name = " + strNodeInnerText.CleanXPath()
                                      + "]/translate")?.Value;
                        strName = Environment.NewLine + '\t' + (blnSync
                                      // ReSharper disable once MethodHasAsyncOverload
                                      ? LanguageManager.GetString(
                                          "Label_Bioware",
                                          token: token)
                                      : await LanguageManager.GetStringAsync(
                                          "Label_Bioware",
                                          token: token).ConfigureAwait(false))
                                  + strSpace + (!string.IsNullOrEmpty(strTranslate)
                                      ? strTranslate
                                      : strNodeInnerText.IsGuid()
                                          ? objLoopDoc
                                              .SelectSingleNode(
                                                  "/chummer/biowares/bioware[id = " + strNodeInnerText.CleanXPath()
                                                  + "]/name")?.Value ?? strNodeInnerText
                                          : strNodeInnerText);
                    }

                    string strWareNodeSelectAttribute
                        = xmlNode.SelectSingleNodeAndCacheExpression("@select", token)?.Value
                          ?? string.Empty;
                    if (blnSync)
                    {
                        if (string.IsNullOrEmpty(strWareNodeSelectAttribute))
                            return new Tuple<bool, string>(objCharacter.Cyberware.DeepCount(
                                                               x => x.Children, objCyberware =>
                                                                   (objCyberware.Name == strNodeInnerText
                                                                    || objCyberware.SourceIDString
                                                                    == strNodeInnerText)
                                                                   && objCyberware.SourceType
                                                                   == Improvement.ImprovementSource.Bioware
                                                                   && string.IsNullOrEmpty(
                                                                       objCyberware.PlugsIntoModularMount))
                                                           >= intCount, strName);
                        return new Tuple<bool, string>(objCharacter.Cyberware.DeepCount(x => x.Children, objCyberware =>
                                                           (objCyberware.Name == strNodeInnerText
                                                            || objCyberware.SourceIDString
                                                            == strNodeInnerText)
                                                           && objCyberware.SourceType
                                                           == Improvement.ImprovementSource.Bioware
                                                           && string.IsNullOrEmpty(
                                                               objCyberware.PlugsIntoModularMount) &&
                                                           strWareNodeSelectAttribute == objCyberware.Extra)
                                                       >= intCount, strName);
                    }

                    if (string.IsNullOrEmpty(strWareNodeSelectAttribute))
                        return new Tuple<bool, string>(
                            await (await objCharacter.GetCyberwareAsync(token).ConfigureAwait(false)).DeepCountAsync(
                                x => x.GetChildrenAsync(token), async objCyberware =>
                                    (objCyberware.Name == strNodeInnerText
                                     || await objCyberware.GetSourceIDStringAsync(token).ConfigureAwait(false)
                                     == strNodeInnerText)
                                    && await objCyberware.GetSourceTypeAsync(token).ConfigureAwait(false)
                                    == Improvement.ImprovementSource.Bioware
                                    && string.IsNullOrEmpty(
                                        await objCyberware.GetPlugsIntoModularMountAsync(token).ConfigureAwait(false)),
                                token).ConfigureAwait(false)
                            >= intCount, strName);
                    return new Tuple<bool, string>(
                        await (await objCharacter.GetCyberwareAsync(token).ConfigureAwait(false)).DeepCountAsync(
                            x => x.GetChildrenAsync(token), async objCyberware =>
                                (objCyberware.Name == strNodeInnerText
                                 || await objCyberware.GetSourceIDStringAsync(token).ConfigureAwait(false)
                                 == strNodeInnerText)
                                && await objCyberware.GetSourceTypeAsync(token).ConfigureAwait(false)
                                == Improvement.ImprovementSource.Bioware
                                && string.IsNullOrEmpty(
                                    await objCyberware.GetPlugsIntoModularMountAsync(token).ConfigureAwait(false)) &&
                                strWareNodeSelectAttribute == objCyberware.Extra, token).ConfigureAwait(false)
                        >= intCount, strName);
                }
                case "cyberware":
                {
                    int intCount
                        = xmlNode.SelectSingleNodeAndCacheExpression("@count", token)?.ValueAsInt ?? 1;
                    if (blnShowMessage)
                    {
                        XPathNavigator objLoopDoc = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? objCharacter.LoadDataXPath("cyberware.xml", token: token)
                            : await objCharacter.LoadDataXPathAsync("cyberware.xml", token: token)
                                .ConfigureAwait(false);
                        string strTranslate
                            = objLoopDoc.SelectSingleNode("/chummer/cyberwares/cyberware[id = "
                                                          + strNodeInnerText.CleanXPath() + "]/translate")?.Value
                              ?? objLoopDoc
                                  .SelectSingleNode("/chummer/cyberwares/cyberware[name = "
                                                    + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                        strName = Environment.NewLine + '\t' + (blnSync
                                      // ReSharper disable once MethodHasAsyncOverload
                                      ? LanguageManager.GetString("Label_Cyberware", token: token)
                                      : await LanguageManager.GetStringAsync("Label_Cyberware", token: token)
                                          .ConfigureAwait(false))
                                  + strSpace + (!string.IsNullOrEmpty(strTranslate)
                                      ? strTranslate
                                      : strNodeInnerText.IsGuid()
                                          ? objLoopDoc
                                              .SelectSingleNode(
                                                  "/chummer/cyberwares/cyberware[id = " + strNodeInnerText.CleanXPath()
                                                  + "]/name")?.Value ?? strNodeInnerText
                                          : strNodeInnerText);
                    }

                    if (xmlNode.GetAttribute("sameparent", string.Empty) == bool.TrueString)
                    {
                        if (objParent is Cyberware objCyberware)
                            return new Tuple<bool, string>(blnSync
                                    // ReSharper disable once MethodHasAsyncOverload
                                    ? objCyberware.Children.Any(
                                        mod => mod.Name == strNodeInnerText
                                               || string.Equals(
                                                   mod.SourceIDString, strNodeInnerText,
                                                   StringComparison.OrdinalIgnoreCase),
                                        token)
                                    : await (await objCyberware.GetChildrenAsync(token).ConfigureAwait(false))
                                        .AnyAsync(async mod => mod.Name == strNodeInnerText
                                                         || string.Equals(
                                                             await mod.GetSourceIDStringAsync(token).ConfigureAwait(false),
                                                             strNodeInnerText,
                                                             StringComparison
                                                                 .OrdinalIgnoreCase), token)
                                        .ConfigureAwait(false)
                                , strName);
                        return new Tuple<bool, string>(false, strName);
                    }

                    string strWareNodeSelectAttribute
                        = xmlNode.SelectSingleNodeAndCacheExpression("@select", token)?.Value
                          ?? string.Empty;
                    if (blnSync)
                    {
                        if (string.IsNullOrEmpty(strWareNodeSelectAttribute))
                            return new Tuple<bool, string>(objCharacter.Cyberware.DeepCount(
                                                               x => x.Children, objCyberware =>
                                                                   (objCyberware.Name == strNodeInnerText
                                                                    || objCyberware.SourceIDString
                                                                    == strNodeInnerText)
                                                                   && objCyberware.SourceType
                                                                   == Improvement.ImprovementSource.Cyberware
                                                                   && string.IsNullOrEmpty(
                                                                       objCyberware.PlugsIntoModularMount))
                                                           >= intCount, strName);
                        return new Tuple<bool, string>(objCharacter.Cyberware.DeepCount(x => x.Children, objCyberware =>
                                                           (objCyberware.Name == strNodeInnerText
                                                            || objCyberware.SourceIDString
                                                            == strNodeInnerText)
                                                           && objCyberware.SourceType
                                                           == Improvement.ImprovementSource.Cyberware
                                                           && string.IsNullOrEmpty(
                                                               objCyberware.PlugsIntoModularMount) &&
                                                           strWareNodeSelectAttribute == objCyberware.Extra)
                                                       >= intCount, strName);
                    }

                    if (string.IsNullOrEmpty(strWareNodeSelectAttribute))
                        return new Tuple<bool, string>(
                            await (await objCharacter.GetCyberwareAsync(token).ConfigureAwait(false)).DeepCountAsync(
                                x => x.GetChildrenAsync(token), async objCyberware =>
                                    (objCyberware.Name == strNodeInnerText
                                     || await objCyberware.GetSourceIDStringAsync(token).ConfigureAwait(false)
                                     == strNodeInnerText)
                                    && await objCyberware.GetSourceTypeAsync(token).ConfigureAwait(false)
                                    == Improvement.ImprovementSource.Cyberware
                                    && string.IsNullOrEmpty(
                                        await objCyberware.GetPlugsIntoModularMountAsync(token).ConfigureAwait(false)),
                                token).ConfigureAwait(false)
                            >= intCount, strName);
                    return new Tuple<bool, string>(
                        await (await objCharacter.GetCyberwareAsync(token).ConfigureAwait(false)).DeepCountAsync(
                            x => x.GetChildrenAsync(token), async objCyberware =>
                                (objCyberware.Name == strNodeInnerText
                                 || await objCyberware.GetSourceIDStringAsync(token).ConfigureAwait(false)
                                 == strNodeInnerText)
                                && await objCyberware.GetSourceTypeAsync(token).ConfigureAwait(false)
                                == Improvement.ImprovementSource.Cyberware
                                && string.IsNullOrEmpty(
                                    await objCyberware.GetPlugsIntoModularMountAsync(token).ConfigureAwait(false)) &&
                                strWareNodeSelectAttribute == objCyberware.Extra, token).ConfigureAwait(false)
                        >= intCount, strName);
                }
                case "biowarecategory":
                {
                    int intCount
                        = xmlNode.SelectSingleNodeAndCacheExpression("@count", token)?.ValueAsInt ?? 1;
                    if (blnShowMessage)
                    {
                        string strTranslate = (blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? objCharacter.LoadDataXPath("bioware.xml", token: token)
                                : await objCharacter.LoadDataXPathAsync("bioware.xml", token: token)
                                    .ConfigureAwait(false))
                            .SelectSingleNode(
                                "/chummer/categories/category[. = " + strNodeInnerText.CleanXPath() + "]/translate")
                            ?.Value;
                        strName = Environment.NewLine + '\t' + (blnSync
                                      // ReSharper disable once MethodHasAsyncOverload
                                      ? LanguageManager.GetString(
                                          "Label_Bioware",
                                          token: token)
                                      : await LanguageManager.GetStringAsync(
                                          "Label_Bioware",
                                          token: token).ConfigureAwait(false))
                                  + strSpace + (!string.IsNullOrEmpty(strTranslate)
                                      ? strTranslate
                                      : strNodeInnerText);
                    }

                    if (xmlNode.GetAttribute("sameparent", string.Empty) == bool.TrueString)
                    {
                        if (objParent is Cyberware objCyberware)
                            return new Tuple<bool, string>(blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? objCyberware.Children.Any(
                                    mod => mod.Category == strNodeInnerText, token)
                                : await (await objCyberware.GetChildrenAsync(token).ConfigureAwait(false))
                                    .AnyAsync(mod => mod.Category == strNodeInnerText,
                                        token)
                                    .ConfigureAwait(false), strName);
                        return new Tuple<bool, string>(false, strName);
                    }

                    string strWareNodeSelectAttribute
                        = xmlNode.SelectSingleNodeAndCacheExpression("@select", token)?.Value
                          ?? string.Empty;
                    if (blnSync)
                    {
                        if (string.IsNullOrEmpty(strWareNodeSelectAttribute))
                            return new Tuple<bool, string>(objCharacter.Cyberware.DeepCount(
                                    x => x.Children, objCyberware =>
                                        objCyberware.Category == strNodeInnerText &&
                                        objCyberware.SourceType
                                        == Improvement.ImprovementSource.Bioware
                                        && string.IsNullOrEmpty(
                                            objCyberware.PlugsIntoModularMount)) >= intCount,
                                strName);
                        return new Tuple<bool, string>(objCharacter.Cyberware.DeepCount(x => x.Children, objCyberware =>
                                                           objCyberware.Category == strNodeInnerText &&
                                                           objCyberware.SourceType
                                                           == Improvement.ImprovementSource.Bioware
                                                           && string.IsNullOrEmpty(
                                                               objCyberware.PlugsIntoModularMount) &&
                                                           strWareNodeSelectAttribute == objCyberware.Extra)
                                                       >= intCount, strName);
                    }

                    if (string.IsNullOrEmpty(strWareNodeSelectAttribute))
                        return new Tuple<bool, string>(
                            await (await objCharacter.GetCyberwareAsync(token).ConfigureAwait(false)).DeepCountAsync(
                                x => x.GetChildrenAsync(token), async objCyberware =>
                                    objCyberware.Category == strNodeInnerText &&
                                    await objCyberware.GetSourceTypeAsync(token).ConfigureAwait(false)
                                    == Improvement.ImprovementSource.Bioware
                                    && string.IsNullOrEmpty(
                                        await objCyberware.GetPlugsIntoModularMountAsync(token).ConfigureAwait(false)),
                                token).ConfigureAwait(false) >= intCount,
                            strName);
                    return new Tuple<bool, string>(
                        await (await objCharacter.GetCyberwareAsync(token).ConfigureAwait(false)).DeepCountAsync(
                            x => x.GetChildrenAsync(token), async objCyberware =>
                                objCyberware.Category == strNodeInnerText &&
                                await objCyberware.GetSourceTypeAsync(token).ConfigureAwait(false)
                                == Improvement.ImprovementSource.Bioware
                                && string.IsNullOrEmpty(
                                    await objCyberware.GetPlugsIntoModularMountAsync(token).ConfigureAwait(false)) &&
                                strWareNodeSelectAttribute == objCyberware.Extra, token).ConfigureAwait(false)
                        >= intCount, strName);
                }
                case "cyberwarecategory":
                {
                    int intCount
                        = xmlNode.SelectSingleNodeAndCacheExpression("@count", token)?.ValueAsInt ?? 1;
                    if (blnShowMessage)
                    {
                        string strTranslate = (blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? objCharacter.LoadDataXPath("cyberware.xml", token: token)
                            : await objCharacter.LoadDataXPathAsync("cyberware.xml", token: token)
                                .ConfigureAwait(false)).SelectSingleNode(
                            "/chummer/categories/category[. = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                        strName = Environment.NewLine + '\t' + (blnSync
                                      // ReSharper disable once MethodHasAsyncOverload
                                      ? LanguageManager.GetString(
                                          "Label_Cyberware",
                                          token: token)
                                      : await LanguageManager.GetStringAsync(
                                          "Label_Cyberware",
                                          token: token).ConfigureAwait(false))
                                  + strSpace + (!string.IsNullOrEmpty(strTranslate)
                                      ? strTranslate
                                      : strNodeInnerText);
                    }

                    if (xmlNode.GetAttribute("sameparent", string.Empty) == bool.TrueString)
                    {
                        if (objParent is Cyberware objCyberware)
                            return new Tuple<bool, string>(
                                blnSync
                                    // ReSharper disable once MethodHasAsyncOverload
                                    ? objCyberware.Children.Any(mod => mod.Category == strNodeInnerText, token)
                                    : await (await objCyberware.GetChildrenAsync(token).ConfigureAwait(false))
                                        .AnyAsync(mod => mod.Category == strNodeInnerText, token)
                                        .ConfigureAwait(false), strName);
                        return new Tuple<bool, string>(false, strName);
                    }

                    string strWareNodeSelectAttribute
                        = xmlNode.SelectSingleNodeAndCacheExpression("@select", token)?.Value
                          ?? string.Empty;
                    if (blnSync)
                    {
                        if (string.IsNullOrEmpty(strWareNodeSelectAttribute))
                            return new Tuple<bool, string>(objCharacter.Cyberware.DeepCount(
                                    x => x.Children, objCyberware =>
                                        objCyberware.Category == strNodeInnerText &&
                                        objCyberware.SourceType
                                        == Improvement.ImprovementSource.Cyberware
                                        && string.IsNullOrEmpty(
                                            objCyberware.PlugsIntoModularMount)) >= intCount,
                                strName);
                        return new Tuple<bool, string>(objCharacter.Cyberware.DeepCount(x => x.Children, objCyberware =>
                                                           objCyberware.Category == strNodeInnerText &&
                                                           objCyberware.SourceType
                                                           == Improvement.ImprovementSource.Cyberware
                                                           && string.IsNullOrEmpty(
                                                               objCyberware.PlugsIntoModularMount) &&
                                                           strWareNodeSelectAttribute == objCyberware.Extra)
                                                       >= intCount, strName);
                    }

                    if (string.IsNullOrEmpty(strWareNodeSelectAttribute))
                        return new Tuple<bool, string>(
                            await (await objCharacter.GetCyberwareAsync(token).ConfigureAwait(false)).DeepCountAsync(
                                x => x.GetChildrenAsync(token), async objCyberware =>
                                    objCyberware.Category == strNodeInnerText &&
                                    await objCyberware.GetSourceTypeAsync(token).ConfigureAwait(false)
                                    == Improvement.ImprovementSource.Cyberware
                                    && string.IsNullOrEmpty(
                                        await objCyberware.GetPlugsIntoModularMountAsync(token).ConfigureAwait(false)),
                                token).ConfigureAwait(false) >= intCount,
                            strName);
                    return new Tuple<bool, string>(
                        await (await objCharacter.GetCyberwareAsync(token).ConfigureAwait(false)).DeepCountAsync(
                            x => x.GetChildrenAsync(token), async objCyberware =>
                                objCyberware.Category == strNodeInnerText &&
                                await objCyberware.GetSourceTypeAsync(token).ConfigureAwait(false)
                                == Improvement.ImprovementSource.Cyberware
                                && string.IsNullOrEmpty(
                                    await objCyberware.GetPlugsIntoModularMountAsync(token).ConfigureAwait(false)) &&
                                strWareNodeSelectAttribute == objCyberware.Extra, token).ConfigureAwait(false)
                        >= intCount, strName);
                }
                case "biowarecontains":
                {
                    int intCount
                        = xmlNode.SelectSingleNodeAndCacheExpression("@count", token)?.ValueAsInt ?? 1;
                    if (blnShowMessage)
                    {
                        string strTranslate
                            = (blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? objCharacter.LoadDataXPath("bioware.xml", token: token)
                                : await objCharacter.LoadDataXPathAsync("bioware.xml", token: token)
                                    .ConfigureAwait(false))
                            .SelectSingleNode(
                                "/chummer/biowares/bioware[name = "
                                + strNodeInnerText.CleanXPath() + "]/translate")
                            ?.Value;
                        strName = Environment.NewLine + '\t' + (blnSync
                                      // ReSharper disable once MethodHasAsyncOverload
                                      ? LanguageManager.GetString(
                                          "Label_Bioware",
                                          token: token)
                                      : await LanguageManager.GetStringAsync(
                                          "Label_Bioware",
                                          token: token).ConfigureAwait(false))
                                  + strSpace + (!string.IsNullOrEmpty(strTranslate)
                                      ? strTranslate
                                      : strNodeInnerText);
                    }

                    string strWareNodeSelectAttribute
                        = xmlNode.SelectSingleNodeAndCacheExpression("@select", token)?.Value
                          ?? string.Empty;
                    if (blnSync)
                    {
                        if (string.IsNullOrEmpty(strWareNodeSelectAttribute))
                            return new Tuple<bool, string>(objCharacter.Cyberware.DeepCount(
                                                               x => x.Children, objCyberware =>
                                                                   objCyberware.Name.Contains(strNodeInnerText) &&
                                                                   objCyberware.SourceType
                                                                   == Improvement.ImprovementSource.Bioware
                                                                   && string.IsNullOrEmpty(
                                                                       objCyberware.PlugsIntoModularMount))
                                                           >= intCount, strName);
                        return new Tuple<bool, string>(objCharacter.Cyberware.DeepCount(x => x.Children, objCyberware =>
                                                           objCyberware.Name.Contains(strNodeInnerText) &&
                                                           objCyberware.SourceType
                                                           == Improvement.ImprovementSource.Bioware
                                                           && string.IsNullOrEmpty(
                                                               objCyberware.PlugsIntoModularMount) &&
                                                           strWareNodeSelectAttribute == objCyberware.Extra)
                                                       >= intCount, strName);
                    }

                    if (string.IsNullOrEmpty(strWareNodeSelectAttribute))
                        return new Tuple<bool, string>(
                            await (await objCharacter.GetCyberwareAsync(token).ConfigureAwait(false)).DeepCountAsync(
                                x => x.GetChildrenAsync(token), async objCyberware =>
                                    objCyberware.Name.Contains(strNodeInnerText) &&
                                    await objCyberware.GetSourceTypeAsync(token).ConfigureAwait(false)
                                    == Improvement.ImprovementSource.Bioware
                                    && string.IsNullOrEmpty(
                                        await objCyberware.GetPlugsIntoModularMountAsync(token).ConfigureAwait(false)),
                                token).ConfigureAwait(false)
                            >= intCount, strName);
                    return new Tuple<bool, string>(
                        await (await objCharacter.GetCyberwareAsync(token).ConfigureAwait(false)).DeepCountAsync(
                            x => x.GetChildrenAsync(token), async objCyberware =>
                                objCyberware.Name.Contains(strNodeInnerText) &&
                                await objCyberware.GetSourceTypeAsync(token).ConfigureAwait(false)
                                == Improvement.ImprovementSource.Bioware
                                && string.IsNullOrEmpty(
                                    await objCyberware.GetPlugsIntoModularMountAsync(token).ConfigureAwait(false)) &&
                                strWareNodeSelectAttribute == objCyberware.Extra, token).ConfigureAwait(false)
                        >= intCount, strName);
                }
                case "cyberwarecontains":
                {
                    int intCount
                        = xmlNode.SelectSingleNodeAndCacheExpression("@count", token)?.ValueAsInt ?? 1;
                    if (blnShowMessage)
                    {
                        string strTranslate
                            = (blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? objCharacter.LoadDataXPath("cyberware.xml", token: token)
                                : await objCharacter.LoadDataXPathAsync("cyberware.xml", token: token)
                                    .ConfigureAwait(false))
                            .SelectSingleNode(
                                "/chummer/cyberwares/cyberware[name = "
                                + strNodeInnerText.CleanXPath() + "]/translate")
                            ?.Value;
                        strName = Environment.NewLine + '\t' + (blnSync
                                      // ReSharper disable once MethodHasAsyncOverload
                                      ? LanguageManager.GetString(
                                          "Label_Cyberware",
                                          token: token)
                                      : await LanguageManager.GetStringAsync(
                                          "Label_Cyberware",
                                          token: token).ConfigureAwait(false))
                                  + strSpace + (!string.IsNullOrEmpty(strTranslate)
                                      ? strTranslate
                                      : strNodeInnerText);
                    }

                    string strWareNodeSelectAttribute
                        = xmlNode.SelectSingleNodeAndCacheExpression("@select", token)?.Value
                          ?? string.Empty;
                    if (blnSync)
                    {
                        if (string.IsNullOrEmpty(strWareNodeSelectAttribute))
                            return new Tuple<bool, string>(objCharacter.Cyberware.DeepCount(
                                                               x => x.Children, objCyberware =>
                                                                   objCyberware.Name.Contains(strNodeInnerText) &&
                                                                   objCyberware.SourceType
                                                                   == Improvement.ImprovementSource.Cyberware
                                                                   && string.IsNullOrEmpty(
                                                                       objCyberware.PlugsIntoModularMount))
                                                           >= intCount, strName);
                        return new Tuple<bool, string>(objCharacter.Cyberware.DeepCount(x => x.Children, objCyberware =>
                                                           objCyberware.Name.Contains(strNodeInnerText) &&
                                                           objCyberware.SourceType
                                                           == Improvement.ImprovementSource.Cyberware
                                                           && string.IsNullOrEmpty(
                                                               objCyberware.PlugsIntoModularMount) &&
                                                           strWareNodeSelectAttribute == objCyberware.Extra)
                                                       >= intCount, strName);
                    }

                    if (string.IsNullOrEmpty(strWareNodeSelectAttribute))
                        return new Tuple<bool, string>(
                            await (await objCharacter.GetCyberwareAsync(token).ConfigureAwait(false)).DeepCountAsync(
                                x => x.GetChildrenAsync(token), async objCyberware =>
                                    objCyberware.Name.Contains(strNodeInnerText) &&
                                    await objCyberware.GetSourceTypeAsync(token).ConfigureAwait(false)
                                    == Improvement.ImprovementSource.Cyberware
                                    && string.IsNullOrEmpty(
                                        await objCyberware.GetPlugsIntoModularMountAsync(token).ConfigureAwait(false)),
                                token).ConfigureAwait(false)
                            >= intCount, strName);
                    return new Tuple<bool, string>(
                        await (await objCharacter.GetCyberwareAsync(token).ConfigureAwait(false)).DeepCountAsync(
                            x => x.GetChildrenAsync(token), async objCyberware =>
                                objCyberware.Name.Contains(strNodeInnerText) &&
                                await objCyberware.GetSourceTypeAsync(token).ConfigureAwait(false)
                                == Improvement.ImprovementSource.Cyberware
                                && string.IsNullOrEmpty(
                                    await objCyberware.GetPlugsIntoModularMountAsync(token).ConfigureAwait(false)) &&
                                strWareNodeSelectAttribute == objCyberware.Extra, token).ConfigureAwait(false)
                        >= intCount, strName);
                }
                case "damageresistance":
                {
                    // Damage Resistance must be a particular value.
                    if (blnShowMessage)
                        strName = Environment.NewLine + '\t' + (blnSync
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
                    return new Tuple<bool, string>(intDR >= xmlNode.ValueAsInt, strName);
                }
                case "depenabled":
                    // Character must be an AI.
                    if (blnShowMessage)
                        strName = Environment.NewLine + '\t' + string.Format(GlobalSettings.CultureInfo, blnSync
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
                    return new Tuple<bool, string>(
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
                                strName = Environment.NewLine + '\t' +
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
                            return new Tuple<bool, string>(decGrade
                                                           < Convert.ToDecimal(strNodeInnerText.TrimStart('-'),
                                                               GlobalSettings.InvariantCultureInfo), strName);
                        }

                        // Essence must be equal to or greater than the value.
                        if (blnShowMessage)
                            strName = Environment.NewLine + '\t' +
                                      string.Format(GlobalSettings.CultureInfo, blnSync
                                              // ReSharper disable once MethodHasAsyncOverload
                                              ? LanguageManager.GetString(
                                                  "Message_SelectQuality_RequireESSAbove",
                                                  token: token)
                                              : await LanguageManager.GetStringAsync(
                                                  "Message_SelectQuality_RequireESSAbove",
                                                  token: token).ConfigureAwait(false), strNodeInnerText,
                                          strEssNodeGradeAttributeText, decGrade.ToString(GlobalSettings.CultureInfo));
                        return new Tuple<bool, string>(
                            decGrade >= Convert.ToDecimal(strNodeInnerText, GlobalSettings.InvariantCultureInfo),
                            strName);
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
                            strName = Environment.NewLine + '\t' +
                                      string.Format(GlobalSettings.CultureInfo, blnSync
                                              // ReSharper disable once MethodHasAsyncOverload
                                              ? LanguageManager.GetString(
                                                  "Message_SelectQuality_RequireESSBelow",
                                                  token: token)
                                              : await LanguageManager.GetStringAsync(
                                                  "Message_SelectQuality_RequireESSBelow",
                                                  token: token).ConfigureAwait(false), strNodeInnerText,
                                          decEssence.ToString(GlobalSettings.CultureInfo));
                        return new Tuple<bool, string>(decEssence
                                                       < Convert.ToDecimal(strNodeInnerText.TrimStart('-'),
                                                           GlobalSettings.InvariantCultureInfo), strName);
                    }

                    // Essence must be equal to or greater than the value.
                    if (blnShowMessage)
                        strName = Environment.NewLine + '\t' +
                                  string.Format(GlobalSettings.CultureInfo, blnSync
                                          // ReSharper disable once MethodHasAsyncOverload
                                          ? LanguageManager.GetString(
                                              "Message_SelectQuality_RequireESSAbove",
                                              token: token)
                                          : await LanguageManager.GetStringAsync(
                                              "Message_SelectQuality_RequireESSAbove",
                                              token: token).ConfigureAwait(false), strNodeInnerText,
                                      decEssence.ToString(GlobalSettings.CultureInfo));
                    return new Tuple<bool, string>(decEssence
                                                   >= Convert.ToDecimal(strNodeInnerText,
                                                       GlobalSettings.InvariantCultureInfo), strName);
                }
                case "echo":
                {
                    Metamagic objMetamagic = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? objCharacter.Metamagics.FirstOrDefault(
                            x => (x.Name == strNodeInnerText || string.Equals(x.SourceIDString, strNodeInnerText,
                                     StringComparison.OrdinalIgnoreCase))
                                 && x.SourceType == Improvement.ImprovementSource.Echo)
                        : await (await objCharacter.GetMetamagicsAsync(token).ConfigureAwait(false))
                            .FirstOrDefaultAsync(
                                x => (x.Name == strNodeInnerText || string.Equals(x.SourceIDString, strNodeInnerText,
                                         StringComparison.OrdinalIgnoreCase))
                                     && x.SourceType == Improvement.ImprovementSource.Echo,
                                token).ConfigureAwait(false);
                    if (objMetamagic != null)
                    {
                        if (blnShowMessage)
                            strName = blnSync
                                ? objMetamagic.CurrentDisplayName
                                : await objMetamagic.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                        return new Tuple<bool, string>(true, strName);
                    }

                    if (blnShowMessage)
                    {
                        XPathNavigator objLoopDoc = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? objCharacter.LoadDataXPath("echoes.xml", token: token)
                            : await objCharacter.LoadDataXPathAsync("echoes.xml", token: token).ConfigureAwait(false);
                        string strTranslate
                            = objLoopDoc.SelectSingleNode("/chummer/echoes/echo[id = " + strNodeInnerText.CleanXPath()
                                  + "]/translate")?.Value
                              ?? objLoopDoc
                                  .SelectSingleNode("/chummer/echoes/echo[name = " + strNodeInnerText.CleanXPath()
                                      + "]/translate")?.Value;
                        strName = Environment.NewLine + '\t'
                                                      + (!string.IsNullOrEmpty(strTranslate)
                                                          ? strTranslate
                                                          : strNodeInnerText.IsGuid()
                                                              ? objLoopDoc
                                                                    .SelectSingleNode(
                                                                        "/chummer/echoes/echo[id = "
                                                                        + strNodeInnerText.CleanXPath() + "]/name")
                                                                    ?.Value
                                                                ?? strNodeInnerText
                                                              : strNodeInnerText) + strSpace + '('
                                                      + (blnSync
                                                          // ReSharper disable once MethodHasAsyncOverload
                                                          ? LanguageManager.GetString("String_Echo", token: token)
                                                          : await LanguageManager
                                                              .GetStringAsync("String_Echo", token: token)
                                                              .ConfigureAwait(false)) + ')';
                    }

                    return new Tuple<bool, string>(false, strName);
                }
                case "setting":
                case "gameplayoption":
                {
                    // A particular gameplay option is required.
                    if (blnShowMessage)
                        strName = Environment.NewLine + '\t' + (blnSync
                                      // ReSharper disable once MethodHasAsyncOverload
                                      ? LanguageManager.GetString(
                                          "String_GameplayOption",
                                          token: token)
                                      : await LanguageManager.GetStringAsync(
                                          "String_GameplayOption",
                                          token: token).ConfigureAwait(false))
                                  + strSpace + '=' + strSpace + strNodeInnerText;
                    return new Tuple<bool, string>(objCharacter.SettingsKey == strNodeInnerText, strName);
                }
                case "gear":
                {
                    Gear objGear;
                    //TODO: Probably a better way to handle minrating/rating/maxrating but eh, YAGNI.

                    XPathNavigator objExactRatingNode = xmlNode.SelectSingleNodeAndCacheExpression("@rating", token);
                        if (objExactRatingNode != null)
                        {
                            int intRating = objExactRatingNode.ValueAsInt;
                            objGear = blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? objCharacter.Gear.FirstOrDefault(
                                    x => (x.Name == strNodeInnerText || string.Equals(x.SourceIDString, strNodeInnerText,
                                             StringComparison.OrdinalIgnoreCase))
                                         && x.Rating == intRating)
                                : await (await objCharacter.GetGearAsync(token).ConfigureAwait(false)).FirstOrDefaultAsync(
                                        async x => (x.Name == strNodeInnerText || string.Equals(x.SourceIDString,
                                                       strNodeInnerText, StringComparison.OrdinalIgnoreCase))
                                                   && await x.GetRatingAsync(token).ConfigureAwait(false) == intRating,
                                        token)
                                    .ConfigureAwait(false);
                        }
                        else
                        {
                            XPathNavigator objMinRatingNode =
                                xmlNode.SelectSingleNodeAndCacheExpression("@minrating", token);
                            XPathNavigator objMaxRatingNode =
                                xmlNode.SelectSingleNodeAndCacheExpression("@maxrating", token);
                            if (objMinRatingNode != null || objMaxRatingNode != null)
                            {
                                int intMinRating = objMinRatingNode?.ValueAsInt ?? 0;
                                int intMaxRating = objMaxRatingNode?.ValueAsInt ?? int.MaxValue;
                                objGear = blnSync
                                    // ReSharper disable once MethodHasAsyncOverload
                                    ? objCharacter.Gear.FirstOrDefault(
                                        x =>
                                        {
                                            if (x.Name != strNodeInnerText
                                                && !string.Equals(x.SourceIDString, strNodeInnerText, StringComparison.OrdinalIgnoreCase))
                                                return false;
                                            int intMyRating = x.Rating;
                                            return intMyRating >= intMinRating && intMyRating <= intMaxRating;
                                        })
                                : await (await objCharacter.GetGearAsync(token).ConfigureAwait(false))
                                    .FirstOrDefaultAsync(
                                        async x =>
                                        {
                                            if (x.Name != strNodeInnerText
                                                && !string.Equals(x.SourceIDString, strNodeInnerText, StringComparison.OrdinalIgnoreCase))
                                                return false;
                                            int intMyRating = await x.GetRatingAsync(token).ConfigureAwait(false);
                                            return intMyRating >= intMinRating && intMyRating <= intMaxRating;
                                        },
                                        token).ConfigureAwait(false);
                        }
                        else
                        {
                            objGear = blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? objCharacter.Gear.FirstOrDefault(
                                    x => x.Name == strNodeInnerText || string.Equals(x.SourceIDString, strNodeInnerText,
                                        StringComparison.OrdinalIgnoreCase))
                                : await (await objCharacter.GetGearAsync(token).ConfigureAwait(false))
                                    .FirstOrDefaultAsync(
                                        x => x.Name == strNodeInnerText || string.Equals(x.SourceIDString,
                                            strNodeInnerText, StringComparison.OrdinalIgnoreCase), token)
                                    .ConfigureAwait(false);
                        }
                    }

                    if (objGear != null)
                    {
                        if (blnShowMessage)
                            strName = blnSync
                                ? objGear.CurrentDisplayNameShort
                                : await objGear.GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false);
                        return new Tuple<bool, string>(true, strName);
                    }

                    if (blnShowMessage)
                    {
                        // Character needs a specific Martial Art.
                        XPathNavigator objLoopDoc = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? objCharacter.LoadDataXPath("gear.xml", token: token)
                            : await objCharacter.LoadDataXPathAsync("gear.xml", token: token).ConfigureAwait(false);
                        string strTranslate
                            = objLoopDoc.SelectSingleNode("/chummer/gears/gear[id = " + strNodeInnerText.CleanXPath()
                                  + "]/translate")?.Value
                              ?? objLoopDoc
                                  .SelectSingleNode("/chummer/gears/gear[name = " + strNodeInnerText.CleanXPath()
                                      + "]/translate")?.Value;
                        strName = Environment.NewLine + '\t'
                                                      + (!string.IsNullOrEmpty(strTranslate)
                                                          ? strTranslate
                                                          : strNodeInnerText.IsGuid()
                                                              ? objLoopDoc
                                                                    .SelectSingleNode(
                                                                        "/chummer/gears/gear[id = "
                                                                        + strNodeInnerText.CleanXPath() + "]/name")
                                                                    ?.Value
                                                                ?? strNodeInnerText
                                                              : strNodeInnerText) + strSpace + '('
                                                      + (blnSync
                                                          // ReSharper disable once MethodHasAsyncOverload
                                                          ? LanguageManager.GetString(
                                                              "String_Gear",
                                                              token: token)
                                                          : await LanguageManager.GetStringAsync(
                                                              "String_Gear",
                                                              token: token).ConfigureAwait(false)) + ')';
                    }

                    return new Tuple<bool, string>(false, strName);
                }
                case "group":
                {
                    // Check that clustered options are present (Magical Tradition + Skill 6, for example)
                    bool blnResult = true;
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                               out StringBuilder sbdResultName))
                    {
                        sbdResultName.AppendLine().Append('\t')
                            .Append(blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? LanguageManager.GetString(
                                    "Message_SelectQuality_AllOf",
                                    token: token)
                                : await LanguageManager.GetStringAsync(
                                    "Message_SelectQuality_AllOf",
                                    token: token).ConfigureAwait(false));
                        foreach (XPathNavigator xmlChildNode in xmlNode.SelectChildren(XPathNodeType.Element))
                        {
                            (bool blnLoopResult, string strLoopResult) = blnSync
                                ? Utils.SafelyRunSynchronously(
                                    () => xmlChildNode.TestNodeRequirementsAsync(
                                        true, objCharacter, objParent, strIgnoreQuality, blnShowMessage, token), token)
                                : await xmlChildNode.TestNodeRequirementsAsync(
                                        false, objCharacter, objParent, strIgnoreQuality, blnShowMessage, token)
                                    .ConfigureAwait(false);
                            blnResult = blnResult && blnLoopResult;
                            if (!blnResult && !blnShowMessage)
                                break;
                            if (!blnLoopResult)
                                sbdResultName.Append(
                                    strLoopResult.Replace(Environment.NewLine + '\t',
                                        Environment.NewLine + '\t' + '\t'));
                        }

                        if (blnShowMessage)
                            strName = sbdResultName.ToString();
                    }

                    return new Tuple<bool, string>(blnResult, strName);
                }
                case "grouponeof":
                {
                    // Check that one of the clustered options are present
                    bool blnResult = false;
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                               out StringBuilder sbdResultName))
                    {
                        sbdResultName.AppendLine().Append('\t')
                            .Append(blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? LanguageManager.GetString(
                                    "Message_SelectQuality_OneOf",
                                    token: token)
                                : await LanguageManager.GetStringAsync(
                                    "Message_SelectQuality_OneOf",
                                    token: token).ConfigureAwait(false));
                        foreach (XPathNavigator xmlChildNode in xmlNode.SelectChildren(XPathNodeType.Element))
                        {
                            (bool blnLoopResult, string strLoopResult) = blnSync
                                ? Utils.SafelyRunSynchronously(
                                    () => xmlChildNode.TestNodeRequirementsAsync(
                                        true, objCharacter, objParent, strIgnoreQuality, blnShowMessage, token), token)
                                : await xmlChildNode.TestNodeRequirementsAsync(
                                        false, objCharacter, objParent, strIgnoreQuality, blnShowMessage, token)
                                    .ConfigureAwait(false);
                            blnResult = blnLoopResult || blnResult;
                            if (blnResult && !blnShowMessage)
                                break;
                            sbdResultName.Append(
                                strLoopResult.Replace(Environment.NewLine + '\t',
                                    Environment.NewLine + '\t' + '\t'));
                        }

                        if (blnShowMessage)
                            strName = sbdResultName.ToString();
                    }

                    return new Tuple<bool, string>(blnResult, strName);
                }
                case "initiategrade":
                {
                    // Character's initiate grade must be higher than or equal to the required value.
                    if (blnShowMessage)
                        strName = Environment.NewLine + '\t' + (blnSync
                                      // ReSharper disable once MethodHasAsyncOverload
                                      ? LanguageManager.GetString(
                                          "String_InitiateGrade",
                                          token: token)
                                      : await LanguageManager.GetStringAsync(
                                          "String_InitiateGrade",
                                          token: token).ConfigureAwait(false))
                                  + strSpace + '≥' + strSpace + strNodeInnerText;
                    return new Tuple<bool, string>(
                        (blnSync
                            ? objCharacter.InitiateGrade
                            : await objCharacter.GetInitiateGradeAsync(token).ConfigureAwait(false))
                        >= Convert.ToInt32(strNodeInnerText, GlobalSettings.InvariantCultureInfo), strName);
                }
                case "martialart":
                {
                    MartialArt objMartialArt = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? objCharacter.MartialArts.FirstOrDefault(
                            x => x.Name == strNodeInnerText || string.Equals(x.SourceIDString, strNodeInnerText,
                                StringComparison.OrdinalIgnoreCase))
                        : await (await objCharacter.GetMartialArtsAsync(token).ConfigureAwait(false))
                            .FirstOrDefaultAsync(
                                x => x.Name == strNodeInnerText || string.Equals(x.SourceIDString, strNodeInnerText,
                                    StringComparison.OrdinalIgnoreCase), token).ConfigureAwait(false);
                    if (objMartialArt != null)
                    {
                        if (blnShowMessage)
                            strName = blnSync
                                ? objMartialArt.CurrentDisplayName
                                : await objMartialArt.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                        return new Tuple<bool, string>(true, strName);
                    }

                    if (blnShowMessage)
                    {
                        // Character needs a specific Martial Art.
                        XPathNavigator objLoopDoc = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? objCharacter.LoadDataXPath("martialarts.xml", token: token)
                            : await objCharacter.LoadDataXPathAsync("martialarts.xml", token: token)
                                .ConfigureAwait(false);
                        string strTranslate
                            = objLoopDoc.SelectSingleNode("/chummer/martialarts/martialart[id = "
                                                          + strNodeInnerText.CleanXPath() + "]/translate")?.Value
                              ?? objLoopDoc
                                  .SelectSingleNode("/chummer/martialarts/martialart[name = "
                                                    + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                        strName = Environment.NewLine + '\t'
                                                      + (!string.IsNullOrEmpty(strTranslate)
                                                          ? strTranslate
                                                          : strNodeInnerText.IsGuid()
                                                              ? objLoopDoc
                                                                    .SelectSingleNode(
                                                                        "/chummer/martialarts/martialart[id = "
                                                                        + strNodeInnerText.CleanXPath() + "]/name")
                                                                    ?.Value
                                                                ?? strNodeInnerText
                                                              : strNodeInnerText) + strSpace + '('
                                                      + (blnSync
                                                          // ReSharper disable once MethodHasAsyncOverload
                                                          ? LanguageManager.GetString(
                                                              "String_MartialArt",
                                                              token: token)
                                                          : await LanguageManager.GetStringAsync(
                                                              "String_MartialArt",
                                                              token: token).ConfigureAwait(false)) + ')';
                    }

                    return new Tuple<bool, string>(false, strName);
                }
                case "martialtechnique":
                {
                    MartialArtTechnique objMartialArtTechnique = null;
                    if (blnSync)
                    {
                        objMartialArtTechnique = objCharacter.MartialArts.SelectMany(x => x.Techniques)
                            .FirstOrDefault(
                                x => x.Name == strNodeInnerText
                                     || string.Equals(x.SourceIDString, strNodeInnerText,
                                         StringComparison.OrdinalIgnoreCase));
                    }
                    else
                    {
                        await (await objCharacter.GetMartialArtsAsync(token).ConfigureAwait(false))
                            .ForEachWithBreakAsync(async x =>
                            {
                                MartialArtTechnique objLoopTechnique
                                    = await x.Techniques.FirstOrDefaultAsync(
                                            y => y.Name == strNodeInnerText || string.Equals(y.SourceIDString,
                                                strNodeInnerText, StringComparison.OrdinalIgnoreCase), token)
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
                        return new Tuple<bool, string>(true, strName);
                    }

                    if (blnShowMessage)
                    {
                        // Character needs a specific Martial Arts technique.
                        XPathNavigator objLoopDoc = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? objCharacter.LoadDataXPath("martialarts.xml", token: token)
                            : await objCharacter.LoadDataXPathAsync("martialarts.xml", token: token)
                                .ConfigureAwait(false);
                        string strTranslate
                            = objLoopDoc.SelectSingleNode("/chummer/techniques/technique[id = "
                                                          + strNodeInnerText.CleanXPath() + "]/translate")?.Value
                              ?? objLoopDoc
                                  .SelectSingleNode("/chummer/techniques/technique[name = "
                                                    + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                        strName = Environment.NewLine + '\t'
                                                      + (!string.IsNullOrEmpty(strTranslate)
                                                          ? strTranslate
                                                          : strNodeInnerText.IsGuid()
                                                              ? objLoopDoc
                                                                    .SelectSingleNode(
                                                                        "/chummer/techniques/technique[id = "
                                                                        + strNodeInnerText.CleanXPath() + "]/name")
                                                                    ?.Value
                                                                ?? strNodeInnerText
                                                              : strNodeInnerText) + strSpace + '('
                                                      + (blnSync
                                                          // ReSharper disable once MethodHasAsyncOverload
                                                          ? LanguageManager.GetString(
                                                              "Label_Options_MartialArtTechnique",
                                                              token: token)
                                                          : await LanguageManager.GetStringAsync(
                                                              "Label_Options_MartialArtTechnique",
                                                              token: token).ConfigureAwait(false))
                                                      + ')';
                    }

                    return new Tuple<bool, string>(false, strName);
                }
                case "metamagic":
                {
                    Metamagic objMetamagic = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? objCharacter.Metamagics.FirstOrDefault(
                            x => x.Name == strNodeInnerText || string.Equals(x.SourceIDString, strNodeInnerText,
                                StringComparison.OrdinalIgnoreCase))
                        : await (await objCharacter.GetMetamagicsAsync(token).ConfigureAwait(false))
                            .FirstOrDefaultAsync(
                                x => x.Name == strNodeInnerText || string.Equals(x.SourceIDString, strNodeInnerText,
                                    StringComparison.OrdinalIgnoreCase), token).ConfigureAwait(false);
                    if (objMetamagic != null)
                    {
                        if (blnShowMessage)
                            strName = blnSync
                                ? objMetamagic.CurrentDisplayName
                                : await objMetamagic.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                        return new Tuple<bool, string>(true, strName);
                    }

                    if (blnShowMessage)
                    {
                        XPathNavigator objLoopDoc = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? objCharacter.LoadDataXPath("metamagic.xml", token: token)
                            : await objCharacter.LoadDataXPathAsync("metamagic.xml", token: token)
                                .ConfigureAwait(false);
                        string strTranslate
                            = objLoopDoc.SelectSingleNode("/chummer/metamagics/metamagic[id = "
                                                          + strNodeInnerText.CleanXPath() + "]/translate")?.Value
                              ?? objLoopDoc
                                  .SelectSingleNode("/chummer/metamagics/metamagic[name = "
                                                    + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                        strName = Environment.NewLine + '\t'
                                                      + (!string.IsNullOrEmpty(strTranslate)
                                                          ? strTranslate
                                                          : strNodeInnerText.IsGuid()
                                                              ? objLoopDoc
                                                                    .SelectSingleNode(
                                                                        "/chummer/metamagics/metamagic[id = "
                                                                        + strNodeInnerText.CleanXPath() + "]/name")
                                                                    ?.Value
                                                                ?? strNodeInnerText
                                                              : strNodeInnerText) + strSpace + '('
                                                      + (blnSync
                                                          // ReSharper disable once MethodHasAsyncOverload
                                                          ? LanguageManager.GetString(
                                                              "String_Metamagic",
                                                              token: token)
                                                          : await LanguageManager.GetStringAsync(
                                                              "String_Metamagic",
                                                              token: token).ConfigureAwait(false)) + ')';
                    }

                    return new Tuple<bool, string>(false, strName);
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
                            return new Tuple<bool, string>(true, strName);
                        }

                        XPathNavigator xmlMetamagicDoc
                            = (blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? objCharacter.LoadDataXPath("metamagic.xml", token: token)
                                : await objCharacter.LoadDataXPathAsync("metamagic.xml", token: token)
                                    .ConfigureAwait(false)).SelectSingleNodeAndCacheExpression("/chummer", token);
                        if (blnShowMessage)
                        {
                            string strTranslateArt
                                = xmlMetamagicDoc
                                      ?.SelectSingleNode("arts/art[id = " + strNodeInnerText.CleanXPath() +
                                                         "]/translate")
                                      ?.Value
                                  ?? xmlMetamagicDoc
                                      ?.SelectSingleNode("arts/art[name = " + strNodeInnerText.CleanXPath()
                                                                            + "]/translate")?.Value;
                            strName = Environment.NewLine + '\t'
                                                          + (!string.IsNullOrEmpty(strTranslateArt)
                                                              ? strTranslateArt
                                                              : strNodeInnerText) + strSpace + '('
                                                          + (blnSync
                                                              // ReSharper disable once MethodHasAsyncOverload
                                                              ? LanguageManager.GetString(
                                                                  "String_Art",
                                                                  token: token)
                                                              : await LanguageManager.GetStringAsync(
                                                                  "String_Art",
                                                                  token: token).ConfigureAwait(false)) + ')';
                        }

                        if (xmlMetamagicDoc == null) return new Tuple<bool, string>(true, strName);
                        string strNodeInnerTextCleaned = strNodeInnerText.CleanXPath();
                        // Loop through the data file for each metamagic to find the Required and Forbidden nodes.
                        foreach (Metamagic objMetamagic in objCharacter.Metamagics)
                        {
                            XPathNavigator xmlMetamagicNode = blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? objMetamagic.GetNodeXPath(token)
                                : await objMetamagic.GetNodeXPathAsync(token).ConfigureAwait(false);
                            if (xmlMetamagicNode?.SelectSingleNode(
                                    "forbidden/art[. = " + strNodeInnerTextCleaned + ']') != null)
                            {
                                return new Tuple<bool, string>(false, strName);
                            }
                        }

                        return new Tuple<bool, string>(true, strName);
                    }

                    Art objArt = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? objCharacter.Arts.FirstOrDefault(
                            x => x.Name == strNodeInnerText || string.Equals(x.SourceIDString, strNodeInnerText,
                                StringComparison.OrdinalIgnoreCase))
                        : await (await objCharacter.GetArtsAsync(token).ConfigureAwait(false)).FirstOrDefaultAsync(
                            x => x.Name == strNodeInnerText || string.Equals(x.SourceIDString, strNodeInnerText,
                                StringComparison.OrdinalIgnoreCase), token).ConfigureAwait(false);
                    if (objArt != null)
                    {
                        if (blnShowMessage)
                            strName = blnSync
                                ? objArt.CurrentDisplayName
                                : await objArt.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                        return new Tuple<bool, string>(true, strName);
                    }

                    // In some cases, we want to proxy metamagics for arts. If we haven't found a match yet, check it here.
                    if (xmlNode.Name == "metamagicart")
                    {
                        Metamagic objMetamagic = blnSync
                            ? objCharacter.Metamagics.FirstOrDefault(
                                x => x.Name == strNodeInnerText || string.Equals(x.SourceIDString, strNodeInnerText,
                                    StringComparison.OrdinalIgnoreCase))
                            : await (await objCharacter.GetMetamagicsAsync(token).ConfigureAwait(false))
                                .FirstOrDefaultAsync(
                                    x => x.Name == strNodeInnerText || string.Equals(x.SourceIDString, strNodeInnerText,
                                        StringComparison.OrdinalIgnoreCase), token).ConfigureAwait(false);
                        if (objMetamagic != null)
                        {
                            if (blnShowMessage)
                                strName = blnSync
                                    ? objMetamagic.CurrentDisplayName
                                    : await objMetamagic.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                            return new Tuple<bool, string>(true, strName);
                        }
                    }

                    if (!blnShowMessage)
                        return new Tuple<bool, string>(false, strName);
                    XPathNavigator objLoopDoc = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? objCharacter.LoadDataXPath("metamagic.xml", token: token)
                        : await objCharacter.LoadDataXPathAsync("metamagic.xml", token: token).ConfigureAwait(false);
                    string strTranslate
                        = objLoopDoc.SelectSingleNode("/chummer/arts/art[id = " + strNodeInnerText.CleanXPath()
                              + "]/translate")?.Value
                          ?? objLoopDoc
                              .SelectSingleNode("/chummer/arts/art[name = " + strNodeInnerText.CleanXPath()
                                                                            + "]/translate")?.Value;
                    strName = Environment.NewLine + '\t'
                                                  + (!string.IsNullOrEmpty(strTranslate)
                                                      ? strTranslate
                                                      : strNodeInnerText.IsGuid()
                                                          ? objLoopDoc
                                                                .SelectSingleNode(
                                                                    "/chummer/arts/art[id = "
                                                                    + strNodeInnerText.CleanXPath() + "]/name")?.Value
                                                            ?? strNodeInnerText
                                                          : strNodeInnerText) + strSpace + '('
                                                  + (blnSync
                                                      // ReSharper disable once MethodHasAsyncOverload
                                                      ? LanguageManager.GetString(
                                                          "String_Art",
                                                          token: token)
                                                      : await LanguageManager.GetStringAsync(
                                                          "String_Art",
                                                          token: token).ConfigureAwait(false)) + ')';
                    return new Tuple<bool, string>(false, strName);
                }
                case "magenabled":
                {
                    // Character must be Awakened.
                    if (blnShowMessage)
                        strName = Environment.NewLine + '\t' + string.Format(GlobalSettings.CultureInfo, blnSync
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
                    return new Tuple<bool, string>(
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

                        strName = Environment.NewLine + '\t'
                                                      + (!string.IsNullOrEmpty(strTranslate)
                                                          ? strTranslate
                                                          : strNodeInnerText.IsGuid()
                                                              ? objLoopDoc
                                                                  .SelectSingleNode(
                                                                      "/chummer/metatypes/metatype[id = "
                                                                      + strNodeInnerText.CleanXPath()
                                                                      + "]/name")?.Value ?? strNodeInnerText
                                                              : strNodeInnerText) + strSpace + '('
                                                      + (blnSync
                                                          // ReSharper disable once MethodHasAsyncOverload
                                                          ? LanguageManager.GetString(
                                                              "String_Metatype",
                                                              token: token)
                                                          : await LanguageManager.GetStringAsync(
                                                              "String_Metatype",
                                                              token: token).ConfigureAwait(false)) + ')';
                    }

                    return new Tuple<bool, string>(strNodeInnerText == objCharacter.Metatype
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

                        strName = Environment.NewLine + '\t'
                                                      + (!string.IsNullOrEmpty(strTranslate)
                                                          ? strTranslate
                                                          : strNodeInnerText) + strSpace + '('
                                                      + (blnSync
                                                          // ReSharper disable once MethodHasAsyncOverload
                                                          ? LanguageManager.GetString(
                                                              "String_MetatypeCategory",
                                                              token: token)
                                                          : await LanguageManager.GetStringAsync(
                                                              "String_MetatypeCategory",
                                                              token: token).ConfigureAwait(false)) + ')';
                    }

                    return new Tuple<bool, string>(strNodeInnerText == objCharacter.MetatypeCategory, strName);
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

                        strName = Environment.NewLine + '\t'
                                                      + (!string.IsNullOrEmpty(strTranslate)
                                                          ? strTranslate
                                                          : strNodeInnerText.IsGuid()
                                                              ? objLoopDoc
                                                                  .SelectSingleNode(
                                                                      "/chummer/metatypes/metatype/metavariants/metavariant[id = "
                                                                      + strNodeInnerText.CleanXPath()
                                                                      + "]/name")?.Value ?? strNodeInnerText
                                                              : strNodeInnerText) + strSpace + '('
                                                      + (blnSync
                                                          // ReSharper disable once MethodHasAsyncOverload
                                                          ? LanguageManager.GetString(
                                                              "String_Metavariant",
                                                              token: token)
                                                          : await LanguageManager.GetStringAsync(
                                                              "String_Metavariant",
                                                              token: token).ConfigureAwait(false)) + ')';
                    }

                    return new Tuple<bool, string>(strNodeInnerText == objCharacter.Metavariant
                                                   || strNodeInnerText == objCharacter.MetavariantGuid.ToString("D",
                                                       GlobalSettings.InvariantCultureInfo), strName);
                }
                case "nuyen":
                {
                    // Character's nuyen must be higher than or equal to the required value.
                    if (blnShowMessage)
                        strName = Environment.NewLine + '\t' + (blnSync
                                      // ReSharper disable once MethodHasAsyncOverload
                                      ? LanguageManager.GetString(
                                          "String_Nuyen",
                                          token: token)
                                      : await LanguageManager.GetStringAsync(
                                          "String_Nuyen",
                                          token: token).ConfigureAwait(false)) + strSpace
                                  + '≥' + strSpace + strNodeInnerText;
                    return new Tuple<bool, string>(
                        (blnSync
                            ? objCharacter.Nuyen
                            : await objCharacter.GetNuyenAsync(token).ConfigureAwait(false)) >= xmlNode.ValueAsInt,
                        strName);
                }
                case "onlyprioritygiven":
                {
                    if (blnShowMessage)
                        strName = Environment.NewLine + '\t'
                                                      + (blnSync
                                                          // ReSharper disable once MethodHasAsyncOverload
                                                          ? LanguageManager.GetString(
                                                              "Message_SelectGeneric_PriorityRestriction",
                                                              token: token)
                                                          : await LanguageManager.GetStringAsync(
                                                              "Message_SelectGeneric_PriorityRestriction",
                                                              token: token).ConfigureAwait(false));
                    return new Tuple<bool, string>(
                        blnSync
                            ? objCharacter.EffectiveBuildMethodUsesPriorityTables
                            : await objCharacter.GetEffectiveBuildMethodUsesPriorityTablesAsync(token)
                                .ConfigureAwait(false), strName);
                }
                case "power":
                {
                        // Run through all of the Powers the character has and see if the current required item exists.
                        Power objPower;
                        XPathNavigator objExactRatingNode = xmlNode.SelectSingleNodeAndCacheExpression("@rating", token);
                        if (objExactRatingNode != null)
                        {
                            int intRating = objExactRatingNode.ValueAsInt;
                            objPower = blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? objCharacter.Powers.FirstOrDefault(
                                    x => (x.Name == strNodeInnerText || string.Equals(x.SourceIDString, strNodeInnerText,
                                             StringComparison.OrdinalIgnoreCase))
                                         && x.Rating == intRating)
                                : await (await objCharacter.GetPowersAsync(token).ConfigureAwait(false)).FirstOrDefaultAsync(
                                        async x => (x.Name == strNodeInnerText || string.Equals(x.SourceIDString,
                                                       strNodeInnerText, StringComparison.OrdinalIgnoreCase))
                                                   && await x.GetRatingAsync(token).ConfigureAwait(false) == intRating,
                                        token)
                                    .ConfigureAwait(false);
                        }
                        else
                        {
                            XPathNavigator objMinRatingNode =
                                xmlNode.SelectSingleNodeAndCacheExpression("@minrating", token);
                            XPathNavigator objMaxRatingNode =
                                xmlNode.SelectSingleNodeAndCacheExpression("@maxrating", token);
                            if (objMinRatingNode != null || objMaxRatingNode != null)
                            {
                                int intMinRating = objMinRatingNode?.ValueAsInt ?? 0;
                                int intMaxRating = objMaxRatingNode?.ValueAsInt ?? int.MaxValue;
                                objPower = blnSync
                                    // ReSharper disable once MethodHasAsyncOverload
                                    ? objCharacter.Powers.FirstOrDefault(
                                        x => (x.Name == strNodeInnerText || string.Equals(x.SourceIDString,
                                                 strNodeInnerText, StringComparison.OrdinalIgnoreCase))
                                             && x.Rating >= intMinRating && x.Rating <= intMaxRating)
                                    : await (await objCharacter.GetPowersAsync(token).ConfigureAwait(false))
                                        .FirstOrDefaultAsync(
                                            async x => (x.Name == strNodeInnerText || string.Equals(x.SourceIDString,
                                                           strNodeInnerText, StringComparison.OrdinalIgnoreCase))
                                                       && await x.GetRatingAsync(token).ConfigureAwait(false) >=
                                                       intMinRating &&
                                                       await x.GetRatingAsync(token).ConfigureAwait(false) <= intMaxRating,
                                            token).ConfigureAwait(false);
                            }
                            else
                            {
                                objPower = blnSync
                                    // ReSharper disable once MethodHasAsyncOverload
                                    ? objCharacter.Powers.FirstOrDefault(
                                        x => x.Name == strNodeInnerText || string.Equals(x.SourceIDString, strNodeInnerText,
                                            StringComparison.OrdinalIgnoreCase))
                                    : await (await objCharacter.GetPowersAsync(token).ConfigureAwait(false))
                                        .FirstOrDefaultAsync(
                                            x => x.Name == strNodeInnerText || string.Equals(x.SourceIDString,
                                                strNodeInnerText, StringComparison.OrdinalIgnoreCase), token)
                                        .ConfigureAwait(false);
                            }
                        }
                        if (objPower != null)
                    {
                        if (blnShowMessage)
                            strName = blnSync
                                ? objPower.CurrentDisplayName
                                : await objPower.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                        return new Tuple<bool, string>(true, strName);
                    }

                    if (blnShowMessage)
                    {
                        XPathNavigator objLoopDoc = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? objCharacter.LoadDataXPath("powers.xml", token: token)
                            : await objCharacter.LoadDataXPathAsync("powers.xml", token: token).ConfigureAwait(false);
                        string strTranslate
                            = objLoopDoc.SelectSingleNode("/chummer/powers/power[id = " + strNodeInnerText.CleanXPath()
                                  + "]/translate")?.Value
                              ?? objLoopDoc
                                  .SelectSingleNode("/chummer/powers/power[name = " + strNodeInnerText.CleanXPath()
                                      + "]/translate")?.Value;
                        strName = Environment.NewLine + '\t'
                                                      + (!string.IsNullOrEmpty(strTranslate)
                                                          ? strTranslate
                                                          : strNodeInnerText.IsGuid()
                                                              ? objLoopDoc
                                                                    .SelectSingleNode(
                                                                        "/chummer/powers/power[id = "
                                                                        + strNodeInnerText.CleanXPath() + "]/name")
                                                                    ?.Value
                                                                ?? strNodeInnerText
                                                              : strNodeInnerText) + strSpace + '('
                                                      + (blnSync
                                                          // ReSharper disable once MethodHasAsyncOverload
                                                          ? LanguageManager.GetString(
                                                              "Tab_Adept",
                                                              token: token)
                                                          : await LanguageManager.GetStringAsync(
                                                              "Tab_Adept",
                                                              token: token).ConfigureAwait(false)) + ')';
                    }

                    return new Tuple<bool, string>(false, strName);
                }
                case "program":
                {
                    bool blnResult = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? objCharacter.AIPrograms.Any(p => p.Name == strNodeInnerText
                                                           || string.Equals(p.SourceIDString, strNodeInnerText,
                                                               StringComparison.OrdinalIgnoreCase), token)
                        : await (await objCharacter.GetAIProgramsAsync(token).ConfigureAwait(false)).AnyAsync(p =>
                                p.Name == strNodeInnerText
                                || string.Equals(p.SourceIDString, strNodeInnerText,
                                    StringComparison.OrdinalIgnoreCase),
                            token).ConfigureAwait(false);
                    // Character needs a specific Program.
                    if (!blnShowMessage)
                        return new Tuple<bool, string>(blnResult, strName);
                    XPathNavigator objLoopDoc = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? objCharacter.LoadDataXPath("programs.xml", token: token)
                        : await objCharacter.LoadDataXPathAsync("programs.xml", token: token).ConfigureAwait(false);
                    string strTranslate
                        = objLoopDoc.SelectSingleNode("/chummer/programs/program[id = " + strNodeInnerText.CleanXPath()
                              + "]/translate")?.Value
                          ?? objLoopDoc
                              .SelectSingleNode("/chummer/programs/program[name = " + strNodeInnerText.CleanXPath()
                                  + "]/translate")?.Value;
                    strName = Environment.NewLine + '\t'
                                                  + (!string.IsNullOrEmpty(strTranslate)
                                                      ? strTranslate
                                                      : strNodeInnerText.IsGuid()
                                                          ? objLoopDoc
                                                                .SelectSingleNode(
                                                                    "/chummer/programs/program[id = "
                                                                    + strNodeInnerText.CleanXPath() + "]/name")?.Value
                                                            ?? strNodeInnerText
                                                          : strNodeInnerText) + strSpace + '('
                                                  + (blnSync
                                                      // ReSharper disable once MethodHasAsyncOverload
                                                      ? LanguageManager.GetString(
                                                          "String_Program",
                                                          token: token)
                                                      : await LanguageManager.GetStringAsync(
                                                          "String_Program",
                                                          token: token).ConfigureAwait(false)) + ')';
                    return new Tuple<bool, string>(blnResult, strName);
                }
                case "characterquality":
                case "quality":
                {
                    string strExtra
                        = xmlNode.SelectSingleNodeAndCacheExpression("@extra", token)?.Value;
                    Quality objQuality = blnSync
                        ? !string.IsNullOrEmpty(strExtra)
                            ? objCharacter.Qualities.FirstOrDefault(
                                q => (q.Name == strNodeInnerText || string.Equals(q.SourceIDString, strNodeInnerText,
                                         StringComparison.OrdinalIgnoreCase))
                                     && q.Extra == strExtra && q.Name != strIgnoreQuality
                                     && q.SourceIDString != strIgnoreQuality)
                            : objCharacter.Qualities.FirstOrDefault(
                                q => (q.Name == strNodeInnerText || string.Equals(q.SourceIDString, strNodeInnerText,
                                         StringComparison.OrdinalIgnoreCase))
                                     && q.Name != strIgnoreQuality && q.SourceIDString != strIgnoreQuality)
                        : !string.IsNullOrEmpty(strExtra)
                            ? await (await objCharacter.GetQualitiesAsync(token).ConfigureAwait(false))
                                .FirstOrDefaultAsync(
                                    q => (q.Name == strNodeInnerText || string.Equals(q.SourceIDString,
                                             strNodeInnerText, StringComparison.OrdinalIgnoreCase))
                                         && q.Extra == strExtra && q.Name != strIgnoreQuality
                                         && q.SourceIDString != strIgnoreQuality, token).ConfigureAwait(false)
                            : await (await objCharacter.GetQualitiesAsync(token).ConfigureAwait(false))
                                .FirstOrDefaultAsync(
                                    q => (q.Name == strNodeInnerText || string.Equals(q.SourceIDString,
                                             strNodeInnerText, StringComparison.OrdinalIgnoreCase))
                                         && q.Name != strIgnoreQuality && q.SourceIDString != strIgnoreQuality, token)
                                .ConfigureAwait(false);
                    if (objQuality != null)
                    {
                        if (blnShowMessage)
                            strName = blnSync
                                ? objQuality.CurrentDisplayName
                                : await objQuality.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                        return new Tuple<bool, string>(true, strName);
                    }

                    if (!blnShowMessage)
                        return new Tuple<bool, string>(false, strName);
                    XPathNavigator objLoopDoc = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? objCharacter.LoadDataXPath("qualities.xml", token: token)
                        : await objCharacter.LoadDataXPathAsync("qualities.xml", token: token).ConfigureAwait(false);
                    string strTranslate
                        = objLoopDoc.SelectSingleNode("/chummer/qualities/quality[id = " + strNodeInnerText.CleanXPath()
                              + "]/translate")?.Value
                          ?? objLoopDoc
                              .SelectSingleNode("/chummer/qualities/quality[name = " + strNodeInnerText.CleanXPath()
                                  + "]/translate")?.Value;
                    strName = Environment.NewLine + '\t'
                                                  + (!string.IsNullOrEmpty(strTranslate)
                                                      ? strTranslate
                                                      : strNodeInnerText.IsGuid()
                                                          ? objLoopDoc
                                                                .SelectSingleNode(
                                                                    "/chummer/qualities/quality[id = "
                                                                    + strNodeInnerText.CleanXPath() + "]/name")?.Value
                                                            ?? strNodeInnerText
                                                          : strNodeInnerText) + strSpace + '('
                                                  + (blnSync
                                                      // ReSharper disable once MethodHasAsyncOverload
                                                      ? LanguageManager.GetString(
                                                          "String_Quality",
                                                          token: token)
                                                      : await LanguageManager.GetStringAsync(
                                                          "String_Quality",
                                                          token: token).ConfigureAwait(false)) + ')';
                    return new Tuple<bool, string>(false, strName);
                }
                case "lifestylequality":
                {
                    string strExtra
                        = xmlNode.SelectSingleNodeAndCacheExpression("@extra", token)?.Value;
                    LifestyleQuality objQuality = null;
                    if (blnSync)
                    {
                        IEnumerable<LifestyleQuality> lstLifestyleQualities = objParent is Lifestyle objLifestyle
                            ? objLifestyle.LifestyleQualities
                            : objCharacter.Lifestyles.SelectMany(x => x.LifestyleQualities);
                        objQuality = !string.IsNullOrEmpty(strExtra)
                            ? lstLifestyleQualities.FirstOrDefault(
                                q => (q.Name == strNodeInnerText || string.Equals(q.SourceIDString, strNodeInnerText,
                                         StringComparison.OrdinalIgnoreCase))
                                     && q.Extra == strExtra && q.Name != strIgnoreQuality
                                     && q.SourceIDString != strIgnoreQuality)
                            : lstLifestyleQualities.FirstOrDefault(
                                q => (q.Name == strNodeInnerText || string.Equals(q.SourceIDString, strNodeInnerText,
                                         StringComparison.OrdinalIgnoreCase))
                                     && q.Name != strIgnoreQuality && q.SourceIDString != strIgnoreQuality);
                    }
                    else if (objParent is Lifestyle objLifestyle)
                    {
                        objQuality = !string.IsNullOrEmpty(strExtra)
                            ? await objLifestyle.LifestyleQualities.FirstOrDefaultAsync(
                                q => (q.Name == strNodeInnerText || string.Equals(q.SourceIDString, strNodeInnerText,
                                         StringComparison.OrdinalIgnoreCase))
                                     && q.Extra == strExtra && q.Name != strIgnoreQuality
                                     && q.SourceIDString != strIgnoreQuality, token).ConfigureAwait(false)
                            : await objLifestyle.LifestyleQualities.FirstOrDefaultAsync(
                                    q => (q.Name == strNodeInnerText || string.Equals(q.SourceIDString,
                                             strNodeInnerText,
                                             StringComparison.OrdinalIgnoreCase))
                                         && q.Name != strIgnoreQuality && q.SourceIDString != strIgnoreQuality, token)
                                .ConfigureAwait(false);
                    }
                    else if (!string.IsNullOrEmpty(strExtra))
                    {
                        await (await objCharacter.GetLifestylesAsync(token).ConfigureAwait(false))
                            .ForEachWithBreakAsync(async x =>
                            {
                                LifestyleQuality objLoopQuality
                                    = await x.LifestyleQualities.FirstOrDefaultAsync(
                                        q => (q.Name == strNodeInnerText || string.Equals(q.SourceIDString,
                                                 strNodeInnerText, StringComparison.OrdinalIgnoreCase))
                                             && q.Extra == strExtra && q.Name != strIgnoreQuality
                                             && q.SourceIDString != strIgnoreQuality, token).ConfigureAwait(false);
                                if (objLoopQuality != null)
                                {
                                    objQuality = objLoopQuality;
                                    return false;
                                }

                                return true;
                            }, token).ConfigureAwait(false);
                    }
                    else
                    {
                        await (await objCharacter.GetLifestylesAsync(token).ConfigureAwait(false))
                            .ForEachWithBreakAsync(async x =>
                            {
                                LifestyleQuality objLoopQuality
                                    = await x.LifestyleQualities.FirstOrDefaultAsync(
                                        q => (q.Name == strNodeInnerText || string.Equals(q.SourceIDString,
                                                 strNodeInnerText, StringComparison.OrdinalIgnoreCase))
                                             && q.Name != strIgnoreQuality && q.SourceIDString != strIgnoreQuality,
                                        token).ConfigureAwait(false);
                                if (objLoopQuality != null)
                                {
                                    objQuality = objLoopQuality;
                                    return false;
                                }

                                return true;
                            }, token).ConfigureAwait(false);
                    }

                    if (objQuality != null)
                    {
                        if (blnShowMessage)
                            strName = blnSync
                                ? objQuality.CurrentDisplayName
                                : await objQuality.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                        return new Tuple<bool, string>(true, strName);
                    }

                    if (!blnShowMessage)
                        return new Tuple<bool, string>(false, strName);
                    XPathNavigator objLoopDoc = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? objCharacter.LoadDataXPath("lifestyles.xml", token: token)
                        : await objCharacter.LoadDataXPathAsync("lifestyles.xml", token: token).ConfigureAwait(false);
                    string strTranslate
                        = objLoopDoc.SelectSingleNode("/chummer/qualities/quality[id = " + strNodeInnerText.CleanXPath()
                              + "]/translate")?.Value
                          ?? objLoopDoc
                              .SelectSingleNode("/chummer/qualities/quality[name = " + strNodeInnerText.CleanXPath()
                                  + "]/translate")?.Value;
                    strName = Environment.NewLine + '\t'
                                                  + (!string.IsNullOrEmpty(strTranslate)
                                                      ? strTranslate
                                                      : strNodeInnerText.IsGuid()
                                                          ? objLoopDoc
                                                                .SelectSingleNode(
                                                                    "/chummer/qualities/quality[id = "
                                                                    + strNodeInnerText.CleanXPath() + "]/name")?.Value
                                                            ?? strNodeInnerText
                                                          : strNodeInnerText) + strSpace + '('
                                                  + (blnSync
                                                      // ReSharper disable once MethodHasAsyncOverload
                                                      ? LanguageManager.GetString(
                                                          "String_Quality",
                                                          token: token)
                                                      : await LanguageManager.GetStringAsync(
                                                          "String_Quality",
                                                          token: token).ConfigureAwait(false)) + ')';
                    return new Tuple<bool, string>(false, strName);
                }
                case "lifestyle":
                {
                    if (blnShowMessage)
                    {
                        XPathNavigator objLoopDoc = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? objCharacter.LoadDataXPath("lifestyles.xml", token: token)
                            : await objCharacter.LoadDataXPathAsync("lifestyles.xml", token: token)
                                .ConfigureAwait(false);
                        string strTranslate
                            = objLoopDoc.SelectSingleNode("/chummer/lifestyles/lifestyle[id = "
                                                          + strNodeInnerText.CleanXPath()
                                                          + "]/translate")?.Value
                              ?? objLoopDoc
                                  .SelectSingleNode("/chummer/lifestyles/lifestyle[name = "
                                                    + strNodeInnerText.CleanXPath()
                                                    + "]/translate")?.Value;
                        strName = Environment.NewLine + '\t'
                                                      + (!string.IsNullOrEmpty(strTranslate)
                                                          ? strTranslate
                                                          : strNodeInnerText.IsGuid()
                                                              ? objLoopDoc
                                                                    .SelectSingleNode(
                                                                        "/chummer/lifestyles/lifestyle[id = "
                                                                        + strNodeInnerText.CleanXPath() + "]/name")
                                                                    ?.Value
                                                                ?? strNodeInnerText
                                                              : strNodeInnerText) + strSpace + '('
                                                      + (blnSync
                                                          // ReSharper disable once MethodHasAsyncOverload
                                                          ? LanguageManager.GetString(
                                                              "String_Lifestyle",
                                                              token: token)
                                                          : await LanguageManager.GetStringAsync(
                                                              "String_Lifestyle",
                                                              token: token).ConfigureAwait(false)) + ')';
                    }

                    return new Tuple<bool, string>(objParent is Lifestyle objLifestyle
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
                        strName = Environment.NewLine + '\t' + string.Format(GlobalSettings.CultureInfo, blnSync
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
                    return new Tuple<bool, string>(
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
                                    strName += strSpace + '(' + strSpec + ')';
                                }

                                if (!string.IsNullOrEmpty(strValue))
                                {
                                    strName += strSpace + strValue;
                                }
                            }

                            return new Tuple<bool, string>(true, strName);
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
                                    ? objSkillsSection.GetActiveSkill(strNodeName + strSpace + '(' + strSpec + ')',
                                        token)
                                    : await objSkillsSection.GetActiveSkillAsync(
                                        strNodeName + strSpace + '(' + strSpec + ')', token).ConfigureAwait(false);
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
                                        strName += strSpace + '(' + strSpec + ')';
                                    }

                                    if (!string.IsNullOrEmpty(strValue))
                                    {
                                        strName += strSpace + strValue;
                                    }
                                }

                                return new Tuple<bool, string>(true, strName);
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
                        strName = Environment.NewLine + '\t'
                                                      + (!string.IsNullOrEmpty(strTranslate)
                                                          ? strTranslate
                                                          : strSkillName);
                        if (!string.IsNullOrEmpty(strSpec))
                        {
                            strName += strSpace + '(' + strSpec + ')';
                        }

                        if (!string.IsNullOrEmpty(strValue))
                        {
                            strName += strSpace + strValue;
                        }

                        strName += strSpace + '(' + (blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? LanguageManager.GetString(
                                "Tab_Skills",
                                token: token)
                            : await LanguageManager.GetStringAsync(
                                "Tab_Skills",
                                token: token).ConfigureAwait(false)) + ')';
                    }

                    return new Tuple<bool, string>(false, strName);
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
                                                sbdOutput.Append(objGroup.CurrentDisplayName)
                                                    .Append(',')
                                                    .Append(strSpace);
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
                                                sbdOutput.Append(objGroup.CurrentDisplayName)
                                                    .Append(',')
                                                    .Append(strSpace);
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
                                                            .ConfigureAwait(false))
                                                        .Append(',')
                                                        .Append(strSpace);
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
                                                            .ConfigureAwait(false))
                                                        .Append(',')
                                                        .Append(strSpace);
                                                intTotal += await objGroup.GetRatingAsync(token).ConfigureAwait(false);
                                                return false;
                                            }, token).ConfigureAwait(false);
                                }
                            }
                        }

                        if (!blnShowMessage)
                            return new Tuple<bool, string>(
                                intTotal >= (xmlNode.SelectSingleNodeAndCacheExpression("val", token)?.ValueAsInt ?? 0),
                                strName);
                        if (sbdOutput.Length > 0)
                            sbdOutput.Length -= 2;
                        strName = sbdOutput + strSpace + '(' + (blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? LanguageManager.GetString(
                                "String_ExpenseSkill",
                                token: token)
                            : await LanguageManager.GetStringAsync(
                                "String_ExpenseSkill",
                                token: token).ConfigureAwait(false)) + ')';
                    }

                    int intTarget = xmlNode.SelectSingleNodeAndCacheExpression("val", token)
                        ?.ValueAsInt ?? 0;
                    return new Tuple<bool, string>(intTotal >= intTarget, strName);
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
                                                sbdOutput.Append(objGroup.CurrentDisplayName)
                                                    .Append(',')
                                                    .Append(strSpace);
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
                                                                .ConfigureAwait(false))
                                                            .Append(',')
                                                            .Append(strSpace);
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
                            strName = sbdOutput + strSpace + '('
                                      + (blnSync
                                          // ReSharper disable once MethodHasAsyncOverload
                                          ? LanguageManager.GetString(
                                              "String_ExpenseSkillGroup",
                                              token: token)
                                          : await LanguageManager.GetStringAsync(
                                              "String_ExpenseSkillGroup",
                                              token: token).ConfigureAwait(false)) + ')';
                        }
                    }

                    int intTarget = xmlNode.SelectSingleNodeAndCacheExpression("val", token)?.ValueAsInt ?? 0;
                    return new Tuple<bool, string>(intTotal >= intTarget, strName);
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
                        List<Weapon> lstWeapons = new List<Weapon>();
                        foreach (Weapon objWeapon in await (await objCharacter.GetWeaponsAsync(token)
                                     .ConfigureAwait(false)).GetAllDescendantsAsync(
                                     x => x.UnderbarrelWeapons, token).ConfigureAwait(false))
                        {
                            lstWeapons.Add(objWeapon);
                        }

                        await (await objCharacter.GetVehiclesAsync(token).ConfigureAwait(false)).ForEachAsync(async objVehicle =>
                        {
                            foreach (Weapon objWeapon in await objVehicle.Weapons
                                            .GetAllDescendantsAsync(x => x.UnderbarrelWeapons, token)
                                            .ConfigureAwait(false))
                            {
                                lstWeapons.Add(objWeapon);
                            }

                            await objVehicle.WeaponMounts.ForEachAsync(async objMount =>
                            {
                                foreach (Weapon objWeapon in await objMount.Weapons.GetAllDescendantsAsync(
                                                x => x.UnderbarrelWeapons, token).ConfigureAwait(false))
                                {
                                    lstWeapons.Add(objWeapon);
                                }
                            }, token).ConfigureAwait(false);
                        }, token).ConfigureAwait(false);
                        foreach (Weapon objWeapon in lstWeapons)
                            intMods += await objWeapon.WeaponAccessories.CountAsync(x => x.SpecialModification, token).ConfigureAwait(false);
                    }

                    if (blnShowMessage)
                    {
                        strName = Environment.NewLine + '\t'
                                                      + (blnSync
                                                          // ReSharper disable once MethodHasAsyncOverload
                                                          ? LanguageManager.GetString(
                                                              "String_SpecialModificationLimit",
                                                              token: token)
                                                          : await LanguageManager.GetStringAsync(
                                                              "String_SpecialModificationLimit",
                                                              token: token).ConfigureAwait(false))
                                                      + strSpace + '≥' + strSpace + strNodeInnerText;
                    }

                    return new Tuple<bool, string>(
                        intMods + xmlNode.ValueAsInt <= (blnSync ? objCharacter.SpecialModificationLimit : await objCharacter.GetSpecialModificationLimitAsync(token).ConfigureAwait(false)), strName);
                }
                case "spell":
                {
                    Spell objSpell = blnSync
                        ? objCharacter.Spells.FirstOrDefault(
                            x => x.Name == strNodeInnerText || string.Equals(x.SourceIDString, strNodeInnerText,
                                StringComparison.OrdinalIgnoreCase))
                        : await (await objCharacter.GetSpellsAsync(token).ConfigureAwait(false)).FirstOrDefaultAsync(
                            x => x.Name == strNodeInnerText || string.Equals(x.SourceIDString, strNodeInnerText,
                                StringComparison.OrdinalIgnoreCase), token).ConfigureAwait(false);
                    if (objSpell != null)
                    {
                        if (blnShowMessage)
                            strName = blnSync
                                ? objSpell.CurrentDisplayName
                                : await objSpell.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                        return new Tuple<bool, string>(true, strName);
                    }

                    if (blnShowMessage)
                    {
                        // Check for a specific Spell.
                        XPathNavigator objLoopDoc = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? objCharacter.LoadDataXPath("spells.xml", token: token)
                            : await objCharacter.LoadDataXPathAsync("spells.xml", token: token).ConfigureAwait(false);
                        string strTranslate
                            = objLoopDoc.SelectSingleNode("/chummer/spells/spell[id = " + strNodeInnerText.CleanXPath()
                                  + "]/translate")?.Value
                              ?? objLoopDoc
                                  .SelectSingleNode("/chummer/spells/spell[name = " + strNodeInnerText.CleanXPath()
                                      + "]/translate")?.Value;
                        strName = Environment.NewLine + '\t'
                                                      + (!string.IsNullOrEmpty(strTranslate)
                                                          ? strTranslate
                                                          : strNodeInnerText.IsGuid()
                                                              ? objLoopDoc
                                                                    .SelectSingleNode(
                                                                        "/chummer/spells/spell[id = "
                                                                        + strNodeInnerText.CleanXPath() + "]/name")
                                                                    ?.Value
                                                                ?? strNodeInnerText
                                                              : strNodeInnerText) + strSpace + '('
                                                      + (blnSync
                                                          // ReSharper disable once MethodHasAsyncOverload
                                                          ? LanguageManager.GetString(
                                                              "String_DescSpell",
                                                              token: token)
                                                          : await LanguageManager.GetStringAsync(
                                                              "String_DescSpell",
                                                              token: token).ConfigureAwait(false)) + ')';
                    }

                    return new Tuple<bool, string>(false, strName);
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
                        strName = Environment.NewLine + '\t'
                                                      + (!string.IsNullOrEmpty(strTranslate)
                                                          ? strTranslate
                                                          : strNodeInnerText) + strSpace + '('
                                                      + (blnSync
                                                          // ReSharper disable once MethodHasAsyncOverload
                                                          ? LanguageManager.GetString(
                                                              "String_SpellCategory",
                                                              token: token)
                                                          : await LanguageManager.GetStringAsync(
                                                              "String_SpellCategory",
                                                              token: token).ConfigureAwait(false)) + ')';
                    }

                    int intTarget = xmlNode.SelectSingleNodeAndCacheExpression("count", token)?.ValueAsInt ?? 0;
                    if (blnSync)
                        // ReSharper disable once MethodHasAsyncOverload
                        return new Tuple<bool, string>(
                            objCharacter.Spells.Count(objSpell => objSpell.Category == strNodeName, token) >= intTarget,
                            strName);
                    return new Tuple<bool, string>(
                        await (await objCharacter.GetSpellsAsync(token).ConfigureAwait(false))
                            .CountAsync(objSpell => objSpell.Category == strNodeName, token).ConfigureAwait(false) >=
                        intTarget, strName);
                }
                case "spelldescriptor":
                {
                    int intCount = xmlNode.SelectSingleNodeAndCacheExpression("count", token)?.ValueAsInt ?? 0;
                    // Check for a specified amount of a particular Spell Descriptor.
                    if (blnShowMessage)
                        strName = Environment.NewLine + '\t' + (blnSync
                                      // ReSharper disable once MethodHasAsyncOverload
                                      ? LanguageManager.GetString(
                                          "Label_Descriptors",
                                          token: token)
                                      : await LanguageManager.GetStringAsync(
                                          "Label_Descriptors",
                                          token: token).ConfigureAwait(false)) + strSpace
                                  + '≥' + strSpace + intCount.ToString(GlobalSettings.CultureInfo);
                    if (blnSync)
                        // ReSharper disable once MethodHasAsyncOverload
                        return new Tuple<bool, string>(objCharacter.Spells.Count(
                                                           objSpell => objSpell.Descriptors.Contains(strNodeName),
                                                           token)
                                                       // ReSharper disable once MethodHasAsyncOverload
                                                       >= intCount, strName);
                    return new Tuple<bool, string>(
                        await (await objCharacter.GetSpellsAsync(token).ConfigureAwait(false)).CountAsync(
                            objSpell => objSpell.Descriptors.Contains(strNodeName), token).ConfigureAwait(false)
                        >= intCount, strName);
                }
                case "streetcredvsnotoriety":
                {
                    // Street Cred must be higher than Notoriety.
                    if (blnShowMessage)
                        strName = Environment.NewLine + '\t' + (blnSync
                                      // ReSharper disable once MethodHasAsyncOverload
                                      ? LanguageManager.GetString(
                                          "String_StreetCred",
                                          token: token)
                                      : await LanguageManager.GetStringAsync(
                                          "String_StreetCred",
                                          token: token).ConfigureAwait(false)) + strSpace
                                  + '≥' + strSpace + (blnSync
                                      // ReSharper disable once MethodHasAsyncOverload
                                      ? LanguageManager.GetString(
                                          "String_Notoriety",
                                          token: token)
                                      : await LanguageManager.GetStringAsync(
                                          "String_Notoriety",
                                          token: token).ConfigureAwait(false));
                    return new Tuple<bool, string>(
                        blnSync
                            ? objCharacter.StreetCred >= objCharacter.Notoriety
                            : await objCharacter.GetStreetCredAsync(token).ConfigureAwait(false) >=
                              await objCharacter.GetNotorietyAsync(token).ConfigureAwait(false), strName);
                }
                case "submersiongrade":
                {
                    // Character's initiate grade must be higher than or equal to the required value.
                    if (blnShowMessage)
                        strName = Environment.NewLine + '\t' + (blnSync
                                      // ReSharper disable once MethodHasAsyncOverload
                                      ? LanguageManager.GetString(
                                          "String_SubmersionGrade",
                                          token: token)
                                      : await LanguageManager.GetStringAsync(
                                          "String_SubmersionGrade",
                                          token: token).ConfigureAwait(false))
                                  + strSpace + '≥' + strSpace + strNodeInnerText;
                    return new Tuple<bool, string>(
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
                        XPathNavigator objLoopDoc = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? objCharacter.LoadDataXPath("traditions.xml", token: token)
                            : await objCharacter.LoadDataXPathAsync("traditions.xml", token: token)
                                .ConfigureAwait(false);
                        string strTranslate
                            = objLoopDoc.SelectSingleNode("/chummer/traditions/tradition[id = "
                                                          + strNodeInnerText.CleanXPath() + "]/translate")?.Value
                              ?? objLoopDoc
                                  .SelectSingleNode("/chummer/traditions/tradition[name = "
                                                    + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                        strName = Environment.NewLine + '\t'
                                                      + (!string.IsNullOrEmpty(strTranslate)
                                                          ? strTranslate
                                                          : strNodeInnerText.IsGuid()
                                                              ? objLoopDoc
                                                                    .SelectSingleNode(
                                                                        "/chummer/traditions/tradition[id = "
                                                                        + strNodeInnerText.CleanXPath() + "]/name")
                                                                    ?.Value
                                                                ?? strNodeInnerText
                                                              : strNodeInnerText) + strSpace + '('
                                                      + (blnSync
                                                          // ReSharper disable once MethodHasAsyncOverload
                                                          ? LanguageManager.GetString(
                                                              "String_Tradition",
                                                              token: token)
                                                          : await LanguageManager.GetStringAsync(
                                                              "String_Tradition",
                                                              token: token).ConfigureAwait(false)) + ')';
                    }

                    Backend.Uniques.Tradition objTradition = blnSync
                        ? objCharacter.MagicTradition
                        : await objCharacter.GetMagicTraditionAsync(token).ConfigureAwait(false);

                    return new Tuple<bool, string>(objTradition.Name == strNodeInnerText
                                                   || string.Equals(objTradition.SourceIDString, strNodeInnerText,
                                                       StringComparison.OrdinalIgnoreCase), strName);
                }
                case "traditionspiritform":
                {
                    // Character needs a specific spirit form provided by their Tradition.
                    XPathNavigator objLoopDoc;
                    if (blnShowMessage)
                    {
                        objLoopDoc = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? objCharacter.LoadDataXPath("critterpowers.xml", token: token)
                            : await objCharacter.LoadDataXPathAsync("critterpowers.xml", token: token)
                                .ConfigureAwait(false);
                        string strTranslate
                            = objLoopDoc.SelectSingleNode("/chummer/powers/power[id = " + strNodeInnerText.CleanXPath()
                                  + "]/translate")?.Value
                              ?? objLoopDoc
                                  .SelectSingleNode("/chummer/powers/power[name = " + strNodeInnerText.CleanXPath()
                                      + "]/translate")?.Value;
                        strName = Environment.NewLine + '\t'
                                                      + (!string.IsNullOrEmpty(strTranslate)
                                                          ? strTranslate
                                                          : strNodeInnerText.IsGuid()
                                                              ? objLoopDoc
                                                                    .SelectSingleNode(
                                                                        "/chummer/powers/power[id = "
                                                                        + strNodeInnerText.CleanXPath() + "]/name")
                                                                    ?.Value
                                                                ?? strNodeInnerText
                                                              : strNodeInnerText) + strSpace + '('
                                                      + (blnSync
                                                          // ReSharper disable once MethodHasAsyncOverload
                                                          ? LanguageManager.GetString(
                                                              "String_Tradition",
                                                              token: token)
                                                          : await LanguageManager.GetStringAsync(
                                                              "String_Tradition",
                                                              token: token).ConfigureAwait(false)) + ')';
                    }

                    Backend.Uniques.Tradition objTradition = blnSync
                        ? objCharacter.MagicTradition
                        : await objCharacter.GetMagicTraditionAsync(token).ConfigureAwait(false);

                    if (objTradition.SpiritForm == strNodeInnerText)
                        return new Tuple<bool, string>(true, strName);
                    if (!strNodeInnerText.IsGuid())
                        return new Tuple<bool, string>(false, strName);
                    objLoopDoc = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? objCharacter.LoadDataXPath("critterpowers.xml", token: token)
                        : await objCharacter.LoadDataXPathAsync("critterpowers.xml", token: token)
                            .ConfigureAwait(false);
                    string strEnglishName = objLoopDoc
                        .SelectSingleNode(
                            "/chummer/powers/power[id = " + strNodeInnerText.CleanXPath()
                                                          + "]/name")?.Value;
                    return new Tuple<bool, string>(objTradition.SpiritForm == strEnglishName, strName);
                }
                case "weapon":
                {
                    bool blnReturn = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? objCharacter.Weapons.Any(x => x.Name == strNodeInnerText
                                                        || string.Equals(x.SourceIDString, strNodeInnerText,
                                                            StringComparison.OrdinalIgnoreCase), token)
                        : await objCharacter.Weapons.AnyAsync(
                            x => x.Name == strNodeInnerText
                                 || string.Equals(x.SourceIDString, strNodeInnerText,
                                     StringComparison.OrdinalIgnoreCase), token).ConfigureAwait(false);
                    // Character needs a specific Weapon.
                    if (!blnShowMessage)
                        return new Tuple<bool, string>(blnReturn, strName);
                    XPathNavigator objLoopDoc = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? objCharacter.LoadDataXPath("weapons.xml", token: token)
                        : await objCharacter.LoadDataXPathAsync("weapons.xml", token: token).ConfigureAwait(false);
                    string strTranslate
                        = objLoopDoc.SelectSingleNode("/chummer/weapons/weapon[id = " + strNodeInnerText.CleanXPath()
                              + "]/translate")?.Value
                          ?? objLoopDoc
                              .SelectSingleNode("/chummer/weapons/weapon[name = " + strNodeInnerText.CleanXPath()
                                  + "]/translate")?.Value;
                    strName = Environment.NewLine + '\t'
                                                  + (!string.IsNullOrEmpty(strTranslate)
                                                      ? strTranslate
                                                      : strNodeInnerText.IsGuid()
                                                          ? objLoopDoc
                                                                .SelectSingleNode(
                                                                    "/chummer/weapons/weapon[id = "
                                                                    + strNodeInnerText.CleanXPath() + "]/name")?.Value
                                                            ?? strNodeInnerText
                                                          : strNodeInnerText) + strSpace + '('
                                                  + (blnSync
                                                      // ReSharper disable once MethodHasAsyncOverload
                                                      ? LanguageManager.GetString(
                                                          "String_Weapon",
                                                          token: token)
                                                      : await LanguageManager.GetStringAsync(
                                                          "String_Weapon",
                                                          token: token).ConfigureAwait(false)) + ')';
                    return new Tuple<bool, string>(blnReturn, strName);
                }
                case "accessory" when objParent is Weapon objWeapon:
                {
                    bool blnReturn = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? objWeapon.WeaponAccessories.Any(x =>
                                x.Name == strNodeInnerText
                                || string.Equals(x.SourceIDString, strNodeInnerText,
                                    StringComparison.OrdinalIgnoreCase),
                            token)
                        : await objWeapon.WeaponAccessories.AnyAsync(x =>
                                x.Name == strNodeInnerText
                                || string.Equals(x.SourceIDString, strNodeInnerText,
                                    StringComparison.OrdinalIgnoreCase),
                            token).ConfigureAwait(false);
                    if (!blnShowMessage)
                        return new Tuple<bool, string>(blnReturn, strName);
                    XPathNavigator objLoopDoc = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? objCharacter.LoadDataXPath("weapons.xml", token: token)
                        : await objCharacter.LoadDataXPathAsync("weapons.xml", token: token).ConfigureAwait(false);
                    string strTranslate
                        = objLoopDoc.SelectSingleNode("/chummer/accessories/accessory[id = "
                                                      + strNodeInnerText.CleanXPath() + "]/translate")?.Value
                          ?? objLoopDoc
                              .SelectSingleNode("/chummer/accessories/accessory[name = " + strNodeInnerText.CleanXPath()
                                  + "]/translate")?.Value;
                    strName = Environment.NewLine + '\t'
                                                  + (!string.IsNullOrEmpty(strTranslate)
                                                      ? strTranslate
                                                      : strNodeInnerText.IsGuid()
                                                          ? objLoopDoc
                                                                .SelectSingleNode(
                                                                    "/chummer/accessories/accessory[id = "
                                                                    + strNodeInnerText.CleanXPath() + "]/name")?.Value
                                                            ?? strNodeInnerText
                                                          : strNodeInnerText) + strSpace + '('
                                                  + (blnSync
                                                      // ReSharper disable once MethodHasAsyncOverload
                                                      ? LanguageManager.GetString(
                                                          "String_WeaponAccessory",
                                                          token: token)
                                                      : await LanguageManager.GetStringAsync(
                                                          "String_WeaponAccessory",
                                                          token: token).ConfigureAwait(false)) + ')';
                    return new Tuple<bool, string>(blnReturn, strName);
                }
                case "weapondetails" when objParent is Weapon objWeapon:
                {
                    return new Tuple<bool, string>(
                        blnSync
                            // ReSharper disable MethodHasAsyncOverload
                            ? objWeapon.GetNodeXPath(token).ProcessFilterOperationNode(xmlNode, false, token)
                            // ReSharper restore MethodHasAsyncOverload
                            : await (await objWeapon.GetNodeXPathAsync(token).ConfigureAwait(false))
                                .ProcessFilterOperationNodeAsync(
                                    xmlNode, false, token).ConfigureAwait(false), strName);
                }
                case "armormod":
                {
                    if (blnShowMessage)
                    {
                        XPathNavigator objLoopDoc = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? objCharacter.LoadDataXPath("armor.xml", token: token)
                            : await objCharacter.LoadDataXPathAsync("armor.xml", token: token).ConfigureAwait(false);
                        string strTranslate
                            = objLoopDoc.SelectSingleNode("/chummer/armormods/armormod[id = "
                                                          + strNodeInnerText.CleanXPath() + "]/translate")?.Value
                              ?? objLoopDoc
                                  .SelectSingleNode("/chummer/armormods/armormod[name = " +
                                                    strNodeInnerText.CleanXPath()
                                                    + "]/translate")?.Value;
                        strName = Environment.NewLine + '\t'
                                                      + (!string.IsNullOrEmpty(strTranslate)
                                                          ? strTranslate
                                                          : strNodeInnerText.IsGuid()
                                                              ? objLoopDoc
                                                                    .SelectSingleNode(
                                                                        "/chummer/armormods/armormod[id = "
                                                                        + strNodeInnerText.CleanXPath() + "]/name")
                                                                    ?.Value
                                                                ?? strNodeInnerText
                                                              : strNodeInnerText) + strSpace + '('
                                                      + (blnSync
                                                          // ReSharper disable once MethodHasAsyncOverload
                                                          ? LanguageManager.GetString(
                                                              "String_ArmorMod",
                                                              token: token)
                                                          : await LanguageManager.GetStringAsync(
                                                              "String_ArmorMod",
                                                              token: token).ConfigureAwait(false)) + ')';
                    }

                    if (xmlNode.GetAttribute("sameparent", string.Empty) == bool.TrueString)
                    {
                        return new Tuple<bool, string>(objParent is Armor objArmor && (blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? objArmor.ArmorMods.Any(
                                    x => x.Name == strNodeInnerText
                                         || string.Equals(x.SourceIDString, strNodeInnerText,
                                             StringComparison.OrdinalIgnoreCase), token)
                                : await objArmor.ArmorMods.AnyAsync(
                                    x => x.Name == strNodeInnerText
                                         || string.Equals(x.SourceIDString, strNodeInnerText,
                                             StringComparison.OrdinalIgnoreCase), token).ConfigureAwait(false)),
                            strName);
                    }

                    return new Tuple<bool, string>(
                        blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? objCharacter.Armor.Any(
                                x => x.ArmorMods.Any(y => y.Name == strNodeInnerText
                                                          || string.Equals(y.SourceIDString, strNodeInnerText,
                                                              StringComparison.OrdinalIgnoreCase), token), token)
                            : await objCharacter.Armor.AnyAsync(
                                x => x.ArmorMods.AnyAsync(
                                    y => y.Name == strNodeInnerText || string.Equals(y.SourceIDString, strNodeInnerText,
                                        StringComparison.OrdinalIgnoreCase), token),
                                token).ConfigureAwait(false), strName);
                }
                default:
                    Utils.BreakIfDebug();
                    break;
            }

            if (blnShowMessage)
                strName = strNodeInnerText;
            return new Tuple<bool, string>(false, strName);
        }

        /// <summary>
        ///     Evaluates the availability of a given node against Availability Limits in Create Mode
        /// </summary>
        /// <param name="objXmlGear"></param>
        /// <param name="objCharacter"></param>
        /// <param name="intRating"></param>
        /// <param name="intAvailModifier"></param>
        /// <returns></returns>
        public static bool CheckAvailRestriction(XmlNode objXmlGear, Character objCharacter, int intRating = 1, int intAvailModifier = 0)
        {
            return objXmlGear?.CreateNavigator().CheckAvailRestriction(objCharacter, intRating, intAvailModifier) == true;
        }

        /// <summary>
        ///     Evaluates the availability of a given node against Availability Limits in Create Mode
        /// </summary>
        /// <param name="objXmlGear">XPathNavigator element to evaluate.</param>
        /// <param name="objCharacter">Character that we're comparing the Availability against.</param>
        /// <param name="intRating">Effective Rating of the object.</param>
        /// <param name="intAvailModifier">Availability Modifier from other sources.</param>
        /// <returns>Returns False if not permitted with the current gameplay restrictions. Returns True if valid.</returns>
        public static bool CheckAvailRestriction(this XPathNavigator objXmlGear, Character objCharacter, int intRating = 1, int intAvailModifier = 0)
        {
            if (objXmlGear == null)
                return false;
            //TODO: Better handler for restricted gear
            if (objCharacter?.Created != false || objCharacter.RestrictedGear > 0 || objCharacter.IgnoreRules)
                return true;
            // Avail.

            XPathNavigator objAvailNode = objXmlGear.SelectSingleNodeAndCacheExpression("avail");
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
                objAvailNode = objXmlGear.SelectSingleNode("avail" + intHighestAvailNode);
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
                strAvailExpr = objCharacter.ProcessAttributesInXPath(strAvailExpr);
                (bool blnIsSuccess, object objProcess) = CommonFunctions.EvaluateInvariantXPath(strAvailExpr);
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
                objAvailNode = objXmlGear.SelectSingleNode("avail" + intHighestAvailNode);
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
                strAvailExpr =await objCharacter.ProcessAttributesInXPathAsync(strAvailExpr, token: token).ConfigureAwait(false);
                (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(strAvailExpr, token).ConfigureAwait(false);
                if (blnIsSuccess)
                    intAvail += ((double)objProcess).StandardRound();
            }
            else
                intAvail += decValue.StandardRound();
            return intAvail <= await (await objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetMaximumAvailabilityAsync(token).ConfigureAwait(false);
        }

        public static bool CheckNuyenRestriction(XmlNode objXmlGear, Character objCharacter, decimal decMaxNuyen, decimal decCostMultiplier = 1.0m, int intRating = 1)
        {
            return objXmlGear?.CreateNavigator().CheckNuyenRestriction(objCharacter, decMaxNuyen, decCostMultiplier, intRating) == true;
        }

        /// <summary>
        ///     Evaluates whether a given node can be purchased.
        /// </summary>
        /// <param name="objXmlGear">XPathNavigator element to evaluate.</param>
        /// <param name="decMaxNuyen">Total nuyen amount that the character possesses.</param>
        /// <param name="decCostMultiplier">Multiplier of the object's cost value.</param>
        /// <param name="intRating">Effective Rating of the object.</param>
        /// <returns>Returns False if not permitted with the current restrictions. Returns True if valid.</returns>
        public static bool CheckNuyenRestriction(this XPathNavigator objXmlGear, Character objCharacter, decimal decMaxNuyen, decimal decCostMultiplier = 1.0m, int intRating = 1)
        {
            if (objXmlGear == null)
                return false;
            // Cost.
            decimal decCost = 0.0m;
            XPathNavigator objCostNode = objXmlGear.SelectSingleNodeAndCacheExpression("cost");
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
                    strCost = objCharacter.ProcessAttributesInXPath(strCost);
                    (bool blnIsSuccess, object objProcess) = CommonFunctions.EvaluateInvariantXPath(strCost);
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
