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
        /// <param name="strSourceName">Name of the improvement that called this (if it was called by an improvement adding it)</param>
        /// <param name="strLocation">Limb side to use if we need a specific limb side (Left or Right)</param>
        /// <param name="blnIgnoreLimit">Whether to ignore checking for limits on the total amount of this item the character can have.</param>
        /// <returns></returns>
        [Obsolete("This method is a wrapper that calls XPathNavigator instead. Where possible, refactor the calling object to an XPathNavigator instead.", false)]
        public static bool RequirementsMet(this XmlNode xmlNode, Character objCharacter, object objParent = null, string strLocalName = "", string strIgnoreQuality = "", string strSourceName = "", string strLocation = "", bool blnIgnoreLimit = false)
        {
            if (xmlNode == null || objCharacter == null)
                return false;
            // Ignore the rules.
            return objCharacter.IgnoreRules || xmlNode.CreateNavigator().RequirementsMet(objCharacter, objParent, strLocalName, strIgnoreQuality, strSourceName, strLocation, blnIgnoreLimit);
        }

        //TODO: Might be a better location for this; Class names are screwy.
        /// <summary>Evaluates requirements of a given node against a given Character object.</summary>
        /// <param name="xmlNode">XmlNode of the object.</param>
        /// <param name="objCharacter">Character object against which to check.</param>
        /// <param name="objParent">Parent object against which to check.</param>
        /// <param name="strLocalName">Name of the type of item being checked for displaying messages. If empty or null, no message is displayed.</param>
        /// <param name="strIgnoreQuality">Name of a Quality that should be ignored. Typically used when swapping Qualities in career mode.</param>
        /// <param name="strSourceName">Name of the improvement that called this (if it was called by an improvement adding it)</param>
        /// <param name="strLocation">Limb side to use if we need a specific limb side (Left or Right)</param>
        /// <param name="blnIgnoreLimit">Whether to ignore checking for limits on the total amount of this item the character can have.</param>
        /// <returns></returns>
        public static bool RequirementsMet(this XPathNavigator xmlNode, Character objCharacter, object objParent = null, string strLocalName = "", string strIgnoreQuality = "", string strSourceName = "", string strLocation = "", bool blnIgnoreLimit = false)
        {
            if (xmlNode == null || objCharacter == null)
                return false;
            // Ignore the rules.
            if (objCharacter.IgnoreRules)
                return true;
            bool blnShowMessage = !string.IsNullOrEmpty(strLocalName);
            // See if the character is in career mode but would want to add a chargen-only Quality
            if (objCharacter.Created)
            {
                if (xmlNode.SelectSingleNodeAndCacheExpression("chargenonly") != null)
                {
                    if (blnShowMessage)
                    {
                        Program.ShowMessageBox(
                            string.Format(
                                GlobalSettings.CultureInfo,
                                LanguageManager.GetString("Message_SelectGeneric_ChargenRestriction"),
                                strLocalName),
                            string.Format(
                                GlobalSettings.CultureInfo,
                                LanguageManager.GetString("MessageTitle_SelectGeneric_Restriction"),
                                strLocalName),
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    return false;
                }
            }
            // See if the character is using priority-based gen and is trying to add a Quality that can only be added through priorities
            else
            {
                if (xmlNode.SelectSingleNodeAndCacheExpression("careeronly") != null)
                {
                    if (blnShowMessage)
                    {
                        Program.ShowMessageBox(
                            string.Format(
                                GlobalSettings.CultureInfo,
                                LanguageManager.GetString("Message_SelectGeneric_CareerOnlyRestriction"),
                                strLocalName),
                            string.Format(
                                GlobalSettings.CultureInfo,
                                LanguageManager.GetString("MessageTitle_SelectGeneric_Restriction"),
                                strLocalName),
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    return false;
                }
                if (objCharacter.EffectiveBuildMethodUsesPriorityTables && xmlNode.SelectSingleNodeAndCacheExpression("onlyprioritygiven") != null)
                {
                    if (blnShowMessage)
                    {
                        Program.ShowMessageBox(
                            string.Format(
                                GlobalSettings.CultureInfo,
                                LanguageManager.GetString("Message_SelectGeneric_PriorityRestriction"),
                                strLocalName),
                            string.Format(
                                GlobalSettings.CultureInfo,
                                LanguageManager.GetString("MessageTitle_SelectGeneric_Restriction"),
                                strLocalName),
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    return false;
                }
            }
            if (!blnIgnoreLimit)
            {
                // See if the character already has this Quality and whether or not multiple copies are allowed.
                // If the limit at chargen is different from the actual limit, we need to make sure we fetch the former if the character is in Create mode
                string strLimitString = xmlNode.SelectSingleNodeAndCacheExpression("chargenlimit")?.Value;
                if (string.IsNullOrWhiteSpace(strLimitString) || objCharacter.Created)
                {
                    strLimitString = xmlNode.SelectSingleNodeAndCacheExpression("limit")?.Value;
                    // Default case is each quality can only be taken once
                    if (string.IsNullOrWhiteSpace(strLimitString))
                    {
                        if (xmlNode.Name == "quality" ||
                            xmlNode.Name == "martialart" ||
                            xmlNode.Name == "technique" ||
                            xmlNode.Name == "cyberware" ||
                            xmlNode.Name == "bioware")
                            strLimitString = "1";
                        else
                            strLimitString = bool.FalseString;
                    }
                }
                if (strLimitString != bool.FalseString)
                {
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder sbdLimitString))
                    {
                        sbdLimitString.Append(strLimitString);
                        objCharacter.AttributeSection.ProcessAttributesInXPath(sbdLimitString, strLimitString);
                        foreach (string strLimb in Character.LimbStrings)
                        {
                            sbdLimitString.CheapReplace(strLimitString, '{' + strLimb + '}',
                                                        () => (string.IsNullOrEmpty(strLocation)
                                                                ? objCharacter.LimbCount(strLimb)
                                                                : objCharacter.LimbCount(strLimb) / 2)
                                                            .ToString(GlobalSettings.InvariantCultureInfo));
                        }

                        object objProcess
                            = CommonFunctions.EvaluateInvariantXPath(sbdLimitString.ToString(), out bool blnIsSuccess);
                        strLimitString = blnIsSuccess ? objProcess.ToString() : "1";
                    }

                    // We could set this to a list immediately, but I'd rather the pointer start at null so that no list ends up getting selected for the "default" case below
                    IEnumerable<IHasName> objListToCheck = null;
                    bool blnCheckCyberwareChildren = false;
                    switch (xmlNode.Name)
                    {
                        case "quality":
                            {
                                objListToCheck = objCharacter.Qualities.Where(objQuality => objQuality.SourceName == strSourceName && objQuality.Name != strIgnoreQuality);
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

                    int intLimit = Convert.ToInt32(strLimitString, GlobalSettings.InvariantCultureInfo);
                    int intExtendedLimit = intLimit;
                    string strLimitWithInclusions = xmlNode.SelectSingleNodeAndCacheExpression("limitwithinclusions")?.Value;
                    if (!string.IsNullOrEmpty(strLimitWithInclusions))
                        intExtendedLimit = Convert.ToInt32(strLimitWithInclusions, GlobalSettings.InvariantCultureInfo);
                    int intCount = 0;
                    int intExtendedCount = 0;
                    if (objListToCheck != null || blnCheckCyberwareChildren)
                    {
                        string strNameNode = xmlNode.SelectSingleNodeAndCacheExpression("name")?.Value ?? string.Empty;
                        intExtendedCount = intCount;
                        // In case one item is split up into multiple entries with different names, e.g. Indomitable quality, we need to be able to check all those entries against the limit
                        XPathNavigator xmlIncludeInLimit = xmlNode.SelectSingleNodeAndCacheExpression("includeinlimit");
                        if (xmlIncludeInLimit != null)
                        {
                            using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                            out HashSet<string> setNamesIncludedInLimit))
                            {
                                if (!string.IsNullOrEmpty(strNameNode))
                                {
                                    setNamesIncludedInLimit.Add(strNameNode);
                                }

                                foreach (XPathNavigator objChildXml in xmlIncludeInLimit.SelectChildren(
                                             XPathNodeType.Element))
                                {
                                    setNamesIncludedInLimit.Add(objChildXml.Value);
                                }

                                if (blnCheckCyberwareChildren)
                                {
                                    foreach (Cyberware objItem in objCharacter.Cyberware.GetAllDescendants(x =>
                                                 x.Children))
                                    {
                                        if (!setNamesIncludedInLimit.Contains(objItem.Name))
                                            continue;
                                        if (!string.IsNullOrEmpty(strLocation) && objItem.Location != strLocation)
                                            continue;
                                        if (!string.IsNullOrEmpty(objItem.PlugsIntoModularMount)
                                            || !objItem.IsModularCurrentlyEquipped)
                                            continue;
                                        if (strNameNode == objItem.Name)
                                            ++intCount;
                                        ++intExtendedCount;
                                        if (!blnShowMessage
                                            && (intCount >= intLimit || intExtendedCount >= intExtendedLimit))
                                            return false;
                                    }
                                }
                                else
                                {
                                    foreach (string strItemName in objListToCheck.Select(x => x.Name))
                                    {
                                        if (!setNamesIncludedInLimit.Contains(strItemName))
                                            continue;
                                        if (strNameNode == strItemName)
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
                            foreach (Cyberware objItem in objCharacter.Cyberware.GetAllDescendants(x =>
                                x.Children))
                            {
                                if (strNameNode != objItem.Name)
                                    continue;
                                if (!string.IsNullOrEmpty(strLocation) && objItem.Location != strLocation)
                                    continue;
                                if (!string.IsNullOrEmpty(objItem.PlugsIntoModularMount) || !objItem.IsModularCurrentlyEquipped)
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
                                if (strNameNode != objItem.Name)
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
                            Program.ShowMessageBox(
                                string.Format(
                                    GlobalSettings.CultureInfo,
                                    LanguageManager.GetString("Message_SelectGeneric_Limit"),
                                    strLocalName, intLimit == 0 ? 1 : intLimit),
                                string.Format(
                                    GlobalSettings.CultureInfo,
                                    LanguageManager.GetString("MessageTitle_SelectGeneric_Limit"),
                                    strLocalName),
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        return false;
                    }
                }
            }

            XPathNavigator xmlForbiddenNode = xmlNode.SelectSingleNodeAndCacheExpression("forbidden");
            if (xmlForbiddenNode != null)
            {
                // Loop through the oneof requirements.
                foreach (XPathNavigator objXmlOneOf in xmlForbiddenNode.SelectAndCacheExpression("oneof"))
                {
                    foreach (XPathNavigator xmlForbiddenItemNode in objXmlOneOf.SelectChildren(XPathNodeType.Element))
                    {
                        // The character is not allowed to take the Quality, so display a message and uncheck the item.
                        if (xmlForbiddenItemNode.TestNodeRequirements(objCharacter, objParent, out string strName, strIgnoreQuality, blnShowMessage))
                        {
                            if (blnShowMessage)
                            {
                                Program.ShowMessageBox(
                                    string.Format(
                                        GlobalSettings.CultureInfo,
                                        LanguageManager.GetString("Message_SelectGeneric_Restriction"),
                                        strLocalName) + strName,
                                    string.Format(
                                        GlobalSettings.CultureInfo,
                                        LanguageManager.GetString("MessageTitle_SelectGeneric_Restriction"),
                                        strLocalName),
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            return false;
                        }
                    }
                }
            }

            XPathNavigator xmlRequiredNode = xmlNode.SelectSingleNodeAndCacheExpression("required");
            if (xmlRequiredNode != null)
            {
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                              out StringBuilder sbdRequirement))
                {
                    bool blnRequirementMet = true;

                    // Loop through the oneof requirements.
                    foreach (XPathNavigator objXmlOneOf in xmlRequiredNode.SelectAndCacheExpression("oneof"))
                    {
                        bool blnOneOfMet = false;
                        using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                      out StringBuilder sbdThisRequirement))
                        {
                            sbdThisRequirement.AppendLine()
                                              .Append(LanguageManager.GetString("Message_SelectQuality_OneOf"));
                            foreach (XPathNavigator xmlRequiredItemNode in
                                     objXmlOneOf.SelectChildren(XPathNodeType.Element))
                            {
                                if (xmlRequiredItemNode.TestNodeRequirements(
                                        objCharacter, objParent, out string strName, strIgnoreQuality, blnShowMessage))
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
                        foreach (XPathNavigator objXmlAllOf in xmlRequiredNode.SelectAndCacheExpression("allof"))
                        {
                            bool blnAllOfMet = true;
                            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                          out StringBuilder sbdThisRequirement))
                            {
                                sbdThisRequirement.AppendLine()
                                                  .Append(LanguageManager.GetString("Message_SelectQuality_AllOf"));
                                foreach (XPathNavigator xmlRequiredItemNode in objXmlAllOf.SelectChildren(
                                             XPathNodeType.Element))
                                {
                                    // If this item was not found, fail the AllOfMet condition.
                                    if (!xmlRequiredItemNode.TestNodeRequirements(
                                            objCharacter, objParent, out string strName, strIgnoreQuality,
                                            blnShowMessage))
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
                            Program.ShowMessageBox(
                                string.Format(
                                    GlobalSettings.CultureInfo,
                                    LanguageManager.GetString("Message_SelectGeneric_Requirement"),
                                    strLocalName) + sbdRequirement,
                                string.Format(
                                    GlobalSettings.CultureInfo,
                                    LanguageManager.GetString("MessageTitle_SelectGeneric_Requirement"),
                                    strLocalName),
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }

                        return false;
                    }
                }
            }
            return true;
        }

        public static bool TestNodeRequirements(this XPathNavigator xmlNode, Character objCharacter, object objParent, out string strName, string strIgnoreQuality = "", bool blnShowMessage = true)
        {
            strName = string.Empty;
            if (xmlNode == null || objCharacter == null)
            {
                return false;
            }

            string strSpace = LanguageManager.GetString("String_Space");
            string strNodeInnerText = xmlNode.Value;
            string strNodeName = xmlNode.SelectSingleNodeAndCacheExpression("name")?.Value ?? string.Empty;
            switch (xmlNode.Name)
            {
                case "attribute":
                    {
                        // Check to see if an Attribute meets a requirement.
                        CharacterAttrib objAttribute = objCharacter.GetAttribute(strNodeName);
                        int intTargetValue = xmlNode.SelectSingleNodeAndCacheExpression("total")?.ValueAsInt ?? 0;
                        if (blnShowMessage)
                            strName = string.Format(GlobalSettings.CultureInfo, "{0}\t{1}{2}{3}", Environment.NewLine, objAttribute.DisplayAbbrev, strSpace, intTargetValue);
                        // Special cases for when we want to check if a special attribute is enabled
                        if (intTargetValue == 1)
                        {
                            switch (objAttribute.Abbrev)
                            {
                                case "MAG":
                                    return objCharacter.MAGEnabled;

                                case "MAGAdept":
                                    return objCharacter.MAGEnabled && objCharacter.IsMysticAdept;

                                case "RES":
                                    return objCharacter.RESEnabled;

                                case "DEP":
                                    return objCharacter.DEPEnabled;
                            }
                        }

                        if (xmlNode.SelectSingleNodeAndCacheExpression("natural") != null)
                        {
                            return objAttribute.Value >= intTargetValue;
                        }
                        return objAttribute.TotalValue >= intTargetValue;
                    }
                case "attributetotal":
                    {
                        string strNodeAttributes = xmlNode.SelectSingleNodeAndCacheExpression("attributes")?.Value ?? string.Empty;
                        int intNodeVal = xmlNode.SelectSingleNodeAndCacheExpression("val")?.ValueAsInt ?? 0;
                        // Check if the character's Attributes add up to a particular total.
                        string strValue = strNodeAttributes;
                        strValue = objCharacter.AttributeSection.ProcessAttributesInXPath(strValue);
                        if (blnShowMessage)
                            strName = string.Format(GlobalSettings.CultureInfo, "{0}\t{2}{1}{3}", Environment.NewLine,
                                strSpace,
                                objCharacter.AttributeSection.ProcessAttributesInXPathForTooltip(strNodeAttributes,
                                    blnShowValues: false), intNodeVal);
                        object objProcess = CommonFunctions.EvaluateInvariantXPath(strValue, out bool blnIsSuccess);
                        return (blnIsSuccess ? ((double)objProcess).StandardRound() : 0) >= intNodeVal;
                    }
                case "careerkarma":
                    {
                        // Check Career Karma requirement.
                        if (blnShowMessage)
                            strName = Environment.NewLine + '\t' + string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Message_SelectQuality_RequireKarma"), strNodeInnerText);
                        return objCharacter.CareerKarma >= xmlNode.ValueAsInt;
                    }
                case "chargenonly":
                    {
                        if (blnShowMessage)
                            strName = Environment.NewLine + '\t' + LanguageManager.GetString("Message_SelectGeneric_ChargenRestriction");
                        return !objCharacter.Created;
                    }
                case "careeronly":
                    {
                        if (blnShowMessage)
                            strName = Environment.NewLine + '\t' + LanguageManager.GetString("Message_SelectGeneric_CareerOnlyRestriction");
                        return objCharacter.Created;
                    }
                case "critterpower":
                    {
                        // Run through all of the Powers the character has and see if the current required item exists.
                        if (objCharacter.CritterEnabled)
                        {
                            CritterPower critterPower = objCharacter.CritterPowers.FirstOrDefault(p => p.Name == strNodeInnerText);
                            if (critterPower != null)
                            {
                                if (blnShowMessage)
                                    strName = critterPower.CurrentDisplayName;
                                return true;
                            }
                        }
                        if (blnShowMessage)
                        {
                            string strTranslate = objCharacter.LoadDataXPath("critterpowers.xml").SelectSingleNode(
                                "/chummer/powers/power[name = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                            strName = Environment.NewLine + '\t'
                                                          + (!string.IsNullOrEmpty(strTranslate)
                                                              ? strTranslate
                                                              : strNodeInnerText) + strSpace + '('
                                                          + LanguageManager.GetString("Tab_Critter") + ')';
                        }
                        return false;
                    }
                case "bioware":
                    {
                        int count = xmlNode.SelectSingleNodeAndCacheExpression("@count")?.ValueAsInt ?? 1;
                        if (blnShowMessage)
                        {
                            string strTranslate = objCharacter.LoadDataXPath("bioware.xml").SelectSingleNode(
                                "/chummer/biowares/bioware[name = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                            strName = Environment.NewLine + '\t' + LanguageManager.GetString("Label_Bioware")
                                      + strSpace + (!string.IsNullOrEmpty(strTranslate)
                                          ? strTranslate
                                          : strNodeInnerText);
                        }
                        string strWareNodeSelectAttribute = xmlNode.SelectSingleNodeAndCacheExpression("@select")?.Value ?? string.Empty;
                        if (string.IsNullOrEmpty(strWareNodeSelectAttribute))
                            return objCharacter.Cyberware.DeepCount(x => x.Children, objCyberware =>
                                                                        objCyberware.Name == strNodeInnerText &&
                                                                        objCyberware.SourceType
                                                                        == Improvement.ImprovementSource.Bioware
                                                                        && string.IsNullOrEmpty(
                                                                            objCyberware.PlugsIntoModularMount))
                                   >= count;
                        return objCharacter.Cyberware.DeepCount(x => x.Children, objCyberware =>
                                                                    objCyberware.Name == strNodeInnerText &&
                                                                    objCyberware.SourceType
                                                                    == Improvement.ImprovementSource.Bioware
                                                                    && string.IsNullOrEmpty(
                                                                        objCyberware.PlugsIntoModularMount) &&
                                                                    strWareNodeSelectAttribute == objCyberware.Extra)
                               >= count;
                    }
                case "cyberware":
                    {
                        int count = xmlNode.SelectSingleNodeAndCacheExpression("@count")?.ValueAsInt ?? 1;
                        if (blnShowMessage)
                        {
                            string strTranslate = objCharacter.LoadDataXPath("cyberware.xml").SelectSingleNode(
                                "/chummer/cyberwares/cyberware[name = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                            strName = Environment.NewLine + '\t' + LanguageManager.GetString("Label_Cyberware")
                                      + strSpace + (!string.IsNullOrEmpty(strTranslate)
                                          ? strTranslate
                                          : strNodeInnerText);
                        }
                        if (xmlNode.GetAttribute("sameparent", string.Empty) == bool.TrueString)
                        {
                            if (objParent is Cyberware objCyberware)
                                return objCyberware.Children.Any(mod => mod.Name == strNodeInnerText);
                            return false;
                        }
                        string strWareNodeSelectAttribute = xmlNode.SelectSingleNodeAndCacheExpression("@select")?.Value ?? string.Empty;
                        if (string.IsNullOrEmpty(strWareNodeSelectAttribute))
                            return objCharacter.Cyberware.DeepCount(x => x.Children, objCyberware =>
                                                                        objCyberware.Name == strNodeInnerText &&
                                                                        objCyberware.SourceType
                                                                        == Improvement.ImprovementSource.Cyberware
                                                                        && string.IsNullOrEmpty(
                                                                            objCyberware.PlugsIntoModularMount))
                                   >= count;
                        return objCharacter.Cyberware.DeepCount(x => x.Children, objCyberware =>
                                                                    objCyberware.Name == strNodeInnerText &&
                                                                    objCyberware.SourceType
                                                                    == Improvement.ImprovementSource.Cyberware
                                                                    && string.IsNullOrEmpty(
                                                                        objCyberware.PlugsIntoModularMount) &&
                                                                    strWareNodeSelectAttribute == objCyberware.Extra)
                               >= count;
                    }
                case "biowarecategory":
                {
                    int count = xmlNode.SelectSingleNodeAndCacheExpression("@count")?.ValueAsInt ?? 1;
                    if (blnShowMessage)
                    {
                        string strTranslate = objCharacter.LoadDataXPath("bioware.xml").SelectSingleNode(
                            "/chummer/categories/category[. = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                        strName = Environment.NewLine + '\t' + LanguageManager.GetString("Label_Bioware")
                                  + strSpace + (!string.IsNullOrEmpty(strTranslate)
                                      ? strTranslate
                                      : strNodeInnerText);
                    }
                    if (xmlNode.GetAttribute("sameparent", string.Empty) == bool.TrueString)
                    {
                        if (objParent is Cyberware objCyberware)
                            return objCyberware.Children.Any(mod => mod.Category == strNodeInnerText);
                        return false;
                    }
                    string strWareNodeSelectAttribute = xmlNode.SelectSingleNodeAndCacheExpression("@select")?.Value ?? string.Empty;
                    if (string.IsNullOrEmpty(strWareNodeSelectAttribute))
                        return objCharacter.Cyberware.DeepCount(x => x.Children, objCyberware =>
                                                                    objCyberware.Category == strNodeInnerText &&
                                                                    objCyberware.SourceType
                                                                    == Improvement.ImprovementSource.Bioware
                                                                    && string.IsNullOrEmpty(
                                                                        objCyberware.PlugsIntoModularMount)) >= count;
                    return objCharacter.Cyberware.DeepCount(x => x.Children, objCyberware =>
                                                                objCyberware.Category == strNodeInnerText &&
                                                                objCyberware.SourceType
                                                                == Improvement.ImprovementSource.Bioware
                                                                && string.IsNullOrEmpty(
                                                                    objCyberware.PlugsIntoModularMount) &&
                                                                strWareNodeSelectAttribute == objCyberware.Extra)
                           >= count;
                }
                case "cyberwarecategory":
                {
                    int count = xmlNode.SelectSingleNodeAndCacheExpression("@count")?.ValueAsInt ?? 1;
                    if (blnShowMessage)
                    {
                        string strTranslate = objCharacter.LoadDataXPath("cyberware.xml").SelectSingleNode(
                            "/chummer/categories/category[. = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                        strName = Environment.NewLine + '\t' + LanguageManager.GetString("Label_Cyberware")
                                  + strSpace + (!string.IsNullOrEmpty(strTranslate)
                                      ? strTranslate
                                      : strNodeInnerText);
                    }
                    if (xmlNode.GetAttribute("sameparent", string.Empty) == bool.TrueString)
                    {
                        if (objParent is Cyberware objCyberware)
                            return objCyberware.Children.Any(mod => mod.Category == strNodeInnerText);
                        return false;
                    }
                    string strWareNodeSelectAttribute = xmlNode.SelectSingleNodeAndCacheExpression("@select")?.Value ?? string.Empty;
                    if (string.IsNullOrEmpty(strWareNodeSelectAttribute))
                        return objCharacter.Cyberware.DeepCount(x => x.Children, objCyberware =>
                                                                    objCyberware.Category == strNodeInnerText &&
                                                                    objCyberware.SourceType
                                                                    == Improvement.ImprovementSource.Cyberware
                                                                    && string.IsNullOrEmpty(
                                                                        objCyberware.PlugsIntoModularMount)) >= count;
                    return objCharacter.Cyberware.DeepCount(x => x.Children, objCyberware =>
                                                                objCyberware.Category == strNodeInnerText &&
                                                                objCyberware.SourceType
                                                                == Improvement.ImprovementSource.Cyberware
                                                                && string.IsNullOrEmpty(
                                                                    objCyberware.PlugsIntoModularMount) &&
                                                                strWareNodeSelectAttribute == objCyberware.Extra)
                           >= count;
                    }
                case "biowarecontains":
                    {
                        int count = xmlNode.SelectSingleNodeAndCacheExpression("@count")?.ValueAsInt ?? 1;
                        if (blnShowMessage)
                        {
                            string strTranslate = objCharacter.LoadDataXPath("bioware.xml").SelectSingleNode(
                                "/chummer/biowares/bioware[name = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                            strName = Environment.NewLine + '\t' + LanguageManager.GetString("Label_Bioware")
                                      + strSpace + (!string.IsNullOrEmpty(strTranslate)
                                          ? strTranslate
                                          : strNodeInnerText);
                        }
                        string strWareNodeSelectAttribute = xmlNode.SelectSingleNodeAndCacheExpression("@select")?.Value ?? string.Empty;
                        if (string.IsNullOrEmpty(strWareNodeSelectAttribute))
                            return objCharacter.Cyberware.DeepCount(x => x.Children, objCyberware =>
                                                                        objCyberware.Name.Contains(strNodeInnerText) &&
                                                                        objCyberware.SourceType
                                                                        == Improvement.ImprovementSource.Bioware
                                                                        && string.IsNullOrEmpty(
                                                                            objCyberware.PlugsIntoModularMount))
                                   >= count;
                        return objCharacter.Cyberware.DeepCount(x => x.Children, objCyberware =>
                                                                    objCyberware.Name.Contains(strNodeInnerText) &&
                                                                    objCyberware.SourceType
                                                                    == Improvement.ImprovementSource.Bioware
                                                                    && string.IsNullOrEmpty(
                                                                        objCyberware.PlugsIntoModularMount) &&
                                                                    strWareNodeSelectAttribute == objCyberware.Extra)
                               >= count;
                    }
                case "cyberwarecontains":
                    {
                        int count = xmlNode.SelectSingleNodeAndCacheExpression("@count")?.ValueAsInt ?? 1;
                        if (blnShowMessage)
                        {
                            string strTranslate = objCharacter.LoadDataXPath("cyberware.xml").SelectSingleNode(
                                "/chummer/cyberwares/cyberware[name = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                            strName = Environment.NewLine + '\t' + LanguageManager.GetString("Label_Cyberware")
                                      + strSpace + (!string.IsNullOrEmpty(strTranslate)
                                          ? strTranslate
                                          : strNodeInnerText);
                        }
                        string strWareNodeSelectAttribute = xmlNode.SelectSingleNodeAndCacheExpression("@select")?.Value ?? string.Empty;
                        if (string.IsNullOrEmpty(strWareNodeSelectAttribute))
                            return objCharacter.Cyberware.DeepCount(x => x.Children, objCyberware =>
                                                                        objCyberware.Name.Contains(strNodeInnerText) &&
                                                                        objCyberware.SourceType
                                                                        == Improvement.ImprovementSource.Cyberware
                                                                        && string.IsNullOrEmpty(
                                                                            objCyberware.PlugsIntoModularMount))
                                   >= count;
                        return objCharacter.Cyberware.DeepCount(x => x.Children, objCyberware =>
                                                                    objCyberware.Name.Contains(strNodeInnerText) &&
                                                                    objCyberware.SourceType
                                                                    == Improvement.ImprovementSource.Cyberware
                                                                    && string.IsNullOrEmpty(
                                                                        objCyberware.PlugsIntoModularMount) &&
                                                                    strWareNodeSelectAttribute == objCyberware.Extra)
                               >= count;
                    }
                case "damageresistance":
                    {
                        // Damage Resistance must be a particular value.
                        if (blnShowMessage)
                            strName = Environment.NewLine + '\t' + LanguageManager.GetString("String_DamageResistance");
                        return objCharacter.BOD.TotalValue + ImprovementManager.ValueOf(objCharacter, Improvement.ImprovementType.DamageResistance) >= xmlNode.ValueAsInt;
                    }
                case "depenabled":
                    // Character must be an AI.
                    if (blnShowMessage)
                        strName = Environment.NewLine + '\t' + LanguageManager.GetString("String_AttributeDEPLong") + strSpace + '≥' + strSpace + 1.ToString(GlobalSettings.CultureInfo);
                    return objCharacter.DEPEnabled;

                case "ess":
                    {
                        string strEssNodeGradeAttributeText = xmlNode.SelectSingleNodeAndCacheExpression("@grade")?.Value ?? string.Empty;
                        if (!string.IsNullOrEmpty(strEssNodeGradeAttributeText))
                        {
                            decimal decGrade;
                            using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                            out HashSet<string>
                                                                                setEssNodeGradeAttributeText))
                            {
                                setEssNodeGradeAttributeText.AddRange(
                                    strEssNodeGradeAttributeText.SplitNoAlloc(
                                        ',', StringSplitOptions.RemoveEmptyEntries));
                                decGrade =
                                    objCharacter.Cyberware
                                                .Sum(objCyberware =>
                                                         setEssNodeGradeAttributeText.Any(
                                                             func => objCyberware.Grade.Name.Contains(func)),
                                                     objCyberware => objCyberware.CalculatedESS);
                            }

                            if (strNodeInnerText.StartsWith('-'))
                            {
                                // Essence must be less than the value.
                                if (blnShowMessage)
                                    strName = Environment.NewLine + '\t' +
                                              string.Format(GlobalSettings.CultureInfo
                                                  , LanguageManager.GetString("Message_SelectQuality_RequireESSGradeBelow")
                                                  , strNodeInnerText
                                                  , strEssNodeGradeAttributeText
                                                  , decGrade.ToString(GlobalSettings.CultureInfo));
                                return decGrade < Convert.ToDecimal(strNodeInnerText.TrimStart('-'), GlobalSettings.InvariantCultureInfo);
                            }
                            // Essence must be equal to or greater than the value.
                            if (blnShowMessage)
                                strName = Environment.NewLine + '\t' +
                                          string.Format(GlobalSettings.CultureInfo
                                              , LanguageManager.GetString("Message_SelectQuality_RequireESSAbove")
                                              , strNodeInnerText
                                              , strEssNodeGradeAttributeText
                                              , decGrade.ToString(GlobalSettings.CultureInfo));
                            return decGrade >= Convert.ToDecimal(strNodeInnerText, GlobalSettings.InvariantCultureInfo);
                        }
                        // Check Essence requirement.
                        if (strNodeInnerText.StartsWith('-'))
                        {
                            // Essence must be less than the value.
                            if (blnShowMessage)
                                strName = Environment.NewLine + '\t' +
                                          string.Format(GlobalSettings.CultureInfo
                                              , LanguageManager.GetString("Message_SelectQuality_RequireESSBelow")
                                              , strNodeInnerText
                                              , objCharacter.Essence().ToString(GlobalSettings.CultureInfo));
                            return objCharacter.Essence() < Convert.ToDecimal(strNodeInnerText.TrimStart('-'), GlobalSettings.InvariantCultureInfo);
                        }
                        // Essence must be equal to or greater than the value.
                        if (blnShowMessage)
                            strName = Environment.NewLine + '\t' +
                                      string.Format(GlobalSettings.CultureInfo
                                          , LanguageManager.GetString("Message_SelectQuality_RequireESSAbove")
                                          , strNodeInnerText
                                          , objCharacter.Essence().ToString(GlobalSettings.CultureInfo));
                        return objCharacter.Essence() >= Convert.ToDecimal(strNodeInnerText, GlobalSettings.InvariantCultureInfo);
                    }
                case "echo":
                    {
                        Metamagic objMetamagic = objCharacter.Metamagics.FirstOrDefault(x => x.Name == strNodeInnerText && x.SourceType == Improvement.ImprovementSource.Echo);
                        if (objMetamagic != null)
                        {
                            if (blnShowMessage)
                                strName = objMetamagic.CurrentDisplayName;
                            return true;
                        }
                        if (blnShowMessage)
                        {
                            string strTranslate = objCharacter.LoadDataXPath("echoes.xml").SelectSingleNode(
                                "/chummer/echoes/echo[name = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                            strName = Environment.NewLine + '\t'
                                                          + (!string.IsNullOrEmpty(strTranslate)
                                                              ? strTranslate
                                                              : strNodeInnerText) + strSpace + '('
                                                          + LanguageManager.GetString("String_Echo") + ')';
                        }
                        return false;
                    }
                case "setting":
                case "gameplayoption":
                    {
                        // A particular gameplay option is required.
                        if (blnShowMessage)
                            strName = Environment.NewLine + '\t' + LanguageManager.GetString("String_GameplayOption")
                                      + strSpace + '=' + strSpace + strNodeInnerText;
                        return objCharacter.SettingsKey == strNodeInnerText;
                    }
                case "gear":
                    {
                        Gear objGear = objCharacter.Gear.FirstOrDefault(x => x.Name == strNodeInnerText);
                        //TODO: Probably a better way to handle minrating/rating/maxrating but eh, YAGNI.

                        if (xmlNode.SelectSingleNodeAndCacheExpression("@minrating") != null)
                        {
                            int rating = xmlNode.SelectSingleNodeAndCacheExpression("@minrating")?.ValueAsInt ?? 0;
                            objGear = objCharacter.Gear.FirstOrDefault(x => x.Name == strNodeInnerText && x.Rating >= rating);
                        }
                        else if (xmlNode.SelectSingleNodeAndCacheExpression("@rating") != null)
                        {
                            int rating = xmlNode.SelectSingleNodeAndCacheExpression("@rating")?.ValueAsInt ?? 0;
                            objGear = objCharacter.Gear.FirstOrDefault(x => x.Name == strNodeInnerText && x.Rating == rating);
                        }
                        else if (xmlNode.SelectSingleNodeAndCacheExpression("@maxrating") != null)
                        {
                            int rating = xmlNode.SelectSingleNodeAndCacheExpression("@maxrating")?.ValueAsInt ?? 0;
                            objGear = objCharacter.Gear.FirstOrDefault(x => x.Name == strNodeInnerText && x.Rating <= rating);
                        }
                        if (objGear != null)
                        {
                            if (blnShowMessage)
                                strName = objGear.CurrentDisplayNameShort;
                            return true;
                        }
                        if (blnShowMessage)
                        {
                            // Character needs a specific Martial Art.
                            string strTranslate = objCharacter.LoadDataXPath("gear.xml").SelectSingleNode(
                                "/chummer/gears/gear[name = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                            strName = Environment.NewLine + '\t'
                                                          + (!string.IsNullOrEmpty(strTranslate)
                                                              ? strTranslate
                                                              : strNodeInnerText) + strSpace + '('
                                                          + LanguageManager.GetString("String_Gear") + ')';
                        }
                        return false;
                    }
                case "group":
                    {
                        // Check that clustered options are present (Magical Tradition + Skill 6, for example)
                        bool blnResult = true;
                        using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                      out StringBuilder sbdResultName))
                        {
                            sbdResultName.AppendLine().Append('\t')
                                         .Append(LanguageManager.GetString("Message_SelectQuality_AllOf"));
                            foreach (XPathNavigator xmlChildNode in xmlNode.SelectChildren(XPathNodeType.Element))
                            {
                                bool blnLoopResult = xmlChildNode.TestNodeRequirements(
                                    objCharacter, objParent, out string strLoopResult, strIgnoreQuality,
                                    blnShowMessage);
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

                        return blnResult;
                    }
                case "grouponeof":
                    {
                        // Check that one of the clustered options are present
                        bool blnResult = false;
                        using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                      out StringBuilder sbdResultName))
                        {
                            sbdResultName.AppendLine().Append('\t')
                                         .Append(LanguageManager.GetString("Message_SelectQuality_OneOf"));
                            foreach (XPathNavigator xmlChildNode in xmlNode.SelectChildren(XPathNodeType.Element))
                            {
                                blnResult = xmlChildNode.TestNodeRequirements(
                                    objCharacter, objParent, out string strLoopResult, strIgnoreQuality,
                                    blnShowMessage) || blnResult;
                                if (blnResult && !blnShowMessage)
                                    break;
                                sbdResultName.Append(
                                    strLoopResult.Replace(Environment.NewLine + '\t',
                                                          Environment.NewLine + '\t' + '\t'));
                            }

                            if (blnShowMessage)
                                strName = sbdResultName.ToString();
                        }

                        return blnResult;
                    }
                case "initiategrade":
                    {
                        // Character's initiate grade must be higher than or equal to the required value.
                        if (blnShowMessage)
                            strName = Environment.NewLine + '\t' + LanguageManager.GetString("String_InitiateGrade") + strSpace + '≥' + strSpace + strNodeInnerText;
                        return objCharacter.InitiateGrade >= Convert.ToInt32(strNodeInnerText, GlobalSettings.InvariantCultureInfo);
                    }
                case "martialart":
                    {
                        MartialArt objMartialArt = objCharacter.MartialArts.FirstOrDefault(x => x.Name == strNodeInnerText);
                        if (objMartialArt != null)
                        {
                            if (blnShowMessage)
                                strName = objMartialArt.CurrentDisplayName;
                            return true;
                        }
                        if (blnShowMessage)
                        {
                            // Character needs a specific Martial Art.
                            string strTranslate = objCharacter.LoadDataXPath("martialarts.xml").SelectSingleNode(
                                "/chummer/martialarts/martialart[name = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                            strName = Environment.NewLine + '\t'
                                                          + (!string.IsNullOrEmpty(strTranslate)
                                                              ? strTranslate
                                                              : strNodeInnerText) + strSpace + '('
                                                          + LanguageManager.GetString("String_MartialArt") + ')';
                        }
                        return false;
                    }
                case "martialtechnique":
                {
                    MartialArtTechnique objMartialArtTechnique = objCharacter.MartialArts.SelectMany(x => x.Techniques)
                                                                             .FirstOrDefault(
                                                                                 x => x.Name == strNodeInnerText);
                        if (objMartialArtTechnique != null)
                        {
                            if (blnShowMessage)
                                strName = objMartialArtTechnique.CurrentDisplayName;
                            return true;
                        }
                        if (blnShowMessage)
                        {
                            // Character needs a specific Martial Arts technique.
                            string strTranslate = objCharacter.LoadDataXPath("martialarts.xml").SelectSingleNode(
                                "/chummer/techniques/technique[name = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                            strName = Environment.NewLine + '\t'
                                                          + (!string.IsNullOrEmpty(strTranslate)
                                                              ? strTranslate
                                                              : strNodeInnerText) + strSpace + '('
                                                          + LanguageManager.GetString("Label_Options_MartialArtTechnique") + ')';
                        }
                        return false;
                    }
                case "metamagic":
                    {
                        Metamagic objMetamagic = objCharacter.Metamagics.FirstOrDefault(x => x.Name == strNodeInnerText && x.SourceType == Improvement.ImprovementSource.Metamagic);
                        if (objMetamagic != null)
                        {
                            if (blnShowMessage)
                                strName = objMetamagic.CurrentDisplayName;
                            return true;
                        }
                        if (blnShowMessage)
                        {
                            string strTranslate = objCharacter.LoadDataXPath("metamagic.xml").SelectSingleNode(
                                "/chummer/metamagics/metamagic[name = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                            strName = Environment.NewLine + '\t'
                                                          + (!string.IsNullOrEmpty(strTranslate)
                                                              ? strTranslate
                                                              : strNodeInnerText) + strSpace + '('
                                                          + LanguageManager.GetString("String_Metamagic") + ')';
                        }
                        return false;
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
                                return true;
                            }

                            XPathNavigator xmlMetamagicDoc = objCharacter.LoadDataXPath("metamagic.xml")
                                .SelectSingleNodeAndCacheExpression("/chummer");
                            if (blnShowMessage)
                            {
                                string strTranslateArt = xmlMetamagicDoc
                                    ?.SelectSingleNode("arts/art[name = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                                strName = Environment.NewLine + '\t'
                                                              + (!string.IsNullOrEmpty(strTranslateArt)
                                                                  ? strTranslateArt
                                                                  : strNodeInnerText) + strSpace + '('
                                                              + LanguageManager.GetString("String_Art") + ')';
                            }

                            if (xmlMetamagicDoc == null) return true;
                            // Loop through the data file for each metamagic to find the Required and Forbidden nodes.
                            foreach (string strMetamagicNameForXPath in objCharacter.Metamagics.Select(x => x.Name.CleanXPath()))
                            {
                                XPathNavigator xmlMetamagicNode =
                                    xmlMetamagicDoc.SelectSingleNode(
                                        "metamagics/metamagic[name = " + strMetamagicNameForXPath + ']');
                                if (xmlMetamagicNode != null)
                                {
                                    if (xmlMetamagicNode.SelectSingleNode(
                                        "required/art[. = " + strNodeInnerText.CleanXPath() + ']') != null)
                                    {
                                        return true;
                                    }

                                    if (xmlMetamagicNode.SelectSingleNode(
                                        "forbidden/art[. = " + strNodeInnerText.CleanXPath() + ']') != null)
                                    {
                                        return false;
                                    }
                                }
                                else
                                {
                                    // We couldn't find a metamagic with this name, so it's probably an art. Try and find the node.
                                    // If we can't, it's probably a data entry error.
                                    xmlMetamagicNode =
                                        xmlMetamagicDoc.SelectSingleNode("arts/art[name = " + strMetamagicNameForXPath + ']');
                                    if (xmlMetamagicNode == null)
                                        Utils.BreakIfDebug();
                                    else
                                        return true;
                                }
                            }

                            return true;
                        }

                        Art objArt = objCharacter.Arts.FirstOrDefault(x => x.Name == strNodeInnerText);
                        if (objArt != null)
                        {
                            if (blnShowMessage)
                                strName = objArt.CurrentDisplayName;
                            return true;
                        }

                        // In some cases, we want to proxy metamagics for arts. If we haven't found a match yet, check it here.
                        if (xmlNode.Name == "metamagicart")
                        {
                            Metamagic objMetamagic =
                                objCharacter.Metamagics.FirstOrDefault(x => x.Name == strNodeInnerText);
                            if (objMetamagic != null)
                            {
                                if (blnShowMessage)
                                    strName = objMetamagic.CurrentDisplayName;
                                return true;
                            }
                        }

                        if (!blnShowMessage)
                            return false;
                        string strTranslate = objCharacter.LoadDataXPath("metamagic.xml").SelectSingleNode("/chummer/arts/art[name = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                        strName = Environment.NewLine + '\t'
                                                      + (!string.IsNullOrEmpty(strTranslate)
                                                          ? strTranslate
                                                          : strNodeInnerText) + strSpace + '('
                                                      + LanguageManager.GetString("String_Art") + ')';
                        return false;
                    }
                case "magenabled":
                    {
                        // Character must be Awakened.
                        if (blnShowMessage)
                            strName = Environment.NewLine + '\t' +
                                      LanguageManager.GetString("String_AttributeMAGLong") +
                                      strSpace + '≥' + strSpace + 1.ToString(GlobalSettings.CultureInfo);
                        return objCharacter.MAGEnabled;
                    }
                case "metatype":
                    {
                        if (blnShowMessage)
                        {
                            string strXPathFilter = "/chummer/metatypes/metatype[name = " + strNodeInnerText.CleanXPath() + "]/translate";
                            // Check the Metatype restriction.
                            string strTranslate = objCharacter.LoadDataXPath("metatypes.xml").SelectSingleNode(strXPathFilter)?.Value ??
                                                    objCharacter.LoadDataXPath("critters.xml").SelectSingleNode(strXPathFilter)?.Value;
                            strName = Environment.NewLine + '\t'
                                                          + (!string.IsNullOrEmpty(strTranslate)
                                                              ? strTranslate
                                                              : strNodeInnerText) + strSpace + '('
                                                          + LanguageManager.GetString("String_Metatype") + ')';
                        }
                        return strNodeInnerText == objCharacter.Metatype;
                    }
                case "metatypecategory":
                    {
                        if (blnShowMessage)
                        {
                            string strXPathFilter = "/chummer/categories/category[. = " + strNodeInnerText.CleanXPath() + "]/@translate";
                            // Check the Metatype Category restriction.
                            string strTranslate = objCharacter.LoadDataXPath("metatypes.xml").SelectSingleNode(strXPathFilter)?.Value ??
                                                    objCharacter.LoadDataXPath("critters.xml").SelectSingleNode(strXPathFilter)?.Value;
                            strName = Environment.NewLine + '\t'
                                                          + (!string.IsNullOrEmpty(strTranslate)
                                                              ? strTranslate
                                                              : strNodeInnerText) + strSpace + '('
                                                          + LanguageManager.GetString("String_MetatypeCategory") + ')';
                        }
                        return strNodeInnerText == objCharacter.MetatypeCategory;
                    }
                case "metavariant":
                    {
                        if (blnShowMessage)
                        {
                            string strXPathFilter = "/chummer/metatypes/metatype/metavariants/metavariant[name = " + strNodeInnerText.CleanXPath() + "]/translate";
                            // Check the Metavariant restriction.
                            string strTranslate = objCharacter.LoadDataXPath("metatypes.xml").SelectSingleNode(strXPathFilter)?.Value ??
                                                    objCharacter.LoadDataXPath("critters.xml").SelectSingleNode(strXPathFilter)?.Value;
                            strName = Environment.NewLine + '\t'
                                                          + (!string.IsNullOrEmpty(strTranslate)
                                                              ? strTranslate
                                                              : strNodeInnerText) + strSpace + '('
                                                          + LanguageManager.GetString("String_Metavariant") + ')';
                        }
                        return strNodeInnerText == objCharacter.Metavariant;
                    }
                case "nuyen":
                    {
                        // Character's nuyen must be higher than or equal to the required value.
                        if (blnShowMessage)
                            strName = Environment.NewLine + '\t' + LanguageManager.GetString("String_Nuyen") + strSpace + '≥' + strSpace + strNodeInnerText;
                        return objCharacter.Nuyen >= xmlNode.ValueAsInt;
                    }
                case "onlyprioritygiven":
                    {
                        if (blnShowMessage)
                            strName = Environment.NewLine + '\t' + LanguageManager.GetString("Message_SelectGeneric_PriorityRestriction");
                        return objCharacter.EffectiveBuildMethodUsesPriorityTables;
                    }
                case "power":
                    {
                        // Run through all of the Powers the character has and see if the current required item exists.
                        Power power = objCharacter.Powers.FirstOrDefault(p => p.Name == strNodeInnerText);
                        if (power != null)
                        {
                            if (blnShowMessage)
                                strName = power.CurrentDisplayName;
                            return true;
                        }
                        if (blnShowMessage)
                        {
                            string strTranslate = objCharacter.LoadDataXPath("powers.xml").SelectSingleNode(
                                "/chummer/powers/power[name = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                            strName = Environment.NewLine + '\t'
                                                          + (!string.IsNullOrEmpty(strTranslate)
                                                              ? strTranslate
                                                              : strNodeInnerText) + strSpace + '('
                                                          + LanguageManager.GetString("Tab_Adept") + ')';
                        }
                        return false;
                    }
                case "program":
                    {
                        // Character needs a specific Program.
                        if (!blnShowMessage) return objCharacter.AIPrograms.Any(p => p.Name == strNodeInnerText);
                        string strTranslate = objCharacter.LoadDataXPath("programs.xml").SelectSingleNode(
                            "/chummer/programs/program[name = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                        strName = Environment.NewLine + '\t'
                                                      + (!string.IsNullOrEmpty(strTranslate)
                                                          ? strTranslate
                                                          : strNodeInnerText) + strSpace + '('
                                                      + LanguageManager.GetString("String_Program") + ')';
                        return objCharacter.AIPrograms.Any(p => p.Name == strNodeInnerText);
                    }
                case "quality":
                    {
                        string strExtra = xmlNode.SelectSingleNodeAndCacheExpression("@extra")?.Value;
                        Quality quality = !string.IsNullOrEmpty(strExtra)
                            ? objCharacter.Qualities.FirstOrDefault(q => q.Name == strNodeInnerText && q.Extra == strExtra && q.Name != strIgnoreQuality)
                            : objCharacter.Qualities.FirstOrDefault(q => q.Name == strNodeInnerText && q.Name != strIgnoreQuality);
                        if (quality != null)
                        {
                            if (blnShowMessage)
                                strName = quality.CurrentDisplayName;
                            return true;
                        }
                        if (!blnShowMessage) return false;
                        string strTranslate = objCharacter.LoadDataXPath("qualities.xml").SelectSingleNode(
                            "/chummer/qualities/quality[name = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                        strName = Environment.NewLine + '\t'
                                                      + (!string.IsNullOrEmpty(strTranslate)
                                                          ? strTranslate
                                                          : strNodeInnerText) + strSpace + '('
                                                      + LanguageManager.GetString("String_Quality") + ')';
                        return false;
                    }
                case "resenabled":
                    // Character must be Emerged.
                    if (blnShowMessage)
                        strName = Environment.NewLine + '\t' + LanguageManager.GetString("String_AttributeRESLong") + strSpace + '≥' + strSpace + 1.ToString(GlobalSettings.CultureInfo);
                    return objCharacter.RESEnabled;

                case "skill":
                    {
                        string strSpec = xmlNode.SelectSingleNodeAndCacheExpression("spec")?.Value;
                        string strValue = xmlNode.SelectSingleNodeAndCacheExpression("val")?.Value;
                        int intValue = xmlNode.SelectSingleNodeAndCacheExpression("val")?.ValueAsInt ?? 0;
                        // Check if the character has the required Skill.
                        if (xmlNode.SelectSingleNodeAndCacheExpression("type") != null)
                        {
                            KnowledgeSkill objKnowledgeSkill = string.IsNullOrEmpty(strSpec)
                                ? objCharacter.SkillsSection.KnowledgeSkills.FirstOrDefault(
                                    objSkill => objSkill.Name == strNodeName && objSkill.TotalBaseRating >= intValue)
                                : objCharacter.SkillsSection.KnowledgeSkills.FirstOrDefault(
                                    objSkill => objSkill.Name == strNodeName
                                                && objSkill.Specializations.Any(objSpec => objSpec.Name == strSpec)
                                                && objSkill.TotalBaseRating >= intValue);

                            if (objKnowledgeSkill != null)
                            {
                                if (blnShowMessage)
                                {
                                    strName = objKnowledgeSkill.CurrentDisplayName;
                                    if (!string.IsNullOrEmpty(strSpec) && ImprovementManager
                                                                          .GetCachedImprovementListForValueOf(
                                                                              objCharacter,
                                                                              Improvement.ImprovementType
                                                                                  .DisableSpecializationEffects,
                                                                              objKnowledgeSkill.Name).Count == 0)
                                    {
                                        strName += strSpace + '(' + strSpec + ')';
                                    }
                                    if (!string.IsNullOrEmpty(strValue))
                                    {
                                        strName += strSpace + strValue;
                                    }
                                }
                                return true;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(strNodeName))
                            {
                                Skill objSkill = objCharacter.SkillsSection.GetActiveSkill(strNodeName);
                                // Exotic Skill
                                if (objSkill == null && !string.IsNullOrEmpty(strSpec))
                                    objSkill = objCharacter.SkillsSection.GetActiveSkill(strNodeName + strSpace + '(' + strSpec + ')');
                                if (objSkill != null && (xmlNode.SelectSingleNodeAndCacheExpression("spec") == null || objSkill.Specializations.Any(objSpec => objSpec.Name == strSpec)) && objSkill.TotalBaseRating >= intValue)
                                {
                                    if (blnShowMessage)
                                    {
                                        strName = objSkill.CurrentDisplayName;
                                        if (!string.IsNullOrEmpty(strSpec) && ImprovementManager.GetCachedImprovementListForValueOf(objCharacter, Improvement.ImprovementType.DisableSpecializationEffects, objSkill.Name).Count == 0)
                                        {
                                            strName += strSpace + '(' + strSpec + ')';
                                        }
                                        if (!string.IsNullOrEmpty(strValue))
                                        {
                                            strName += strSpace + strValue;
                                        }
                                    }
                                    return true;
                                }
                            }
                        }
                        if (blnShowMessage)
                        {
                            XPathNavigator xmlSkillDoc = objCharacter.LoadDataXPath("skills.xml");
                            string strSkillName = xmlNode.SelectSingleNodeAndCacheExpression("name")?.Value;
                            string strTranslate = xmlSkillDoc.SelectSingleNode("/chummer/skills/skill[name = " + strSkillName.CleanXPath() + "]/translate")?.Value
                                                  ?? xmlSkillDoc.SelectSingleNode("/chummer/knowledgeskills/skill[name = " + strSkillName.CleanXPath() + "]/translate")?.Value;
                            strName = Environment.NewLine + '\t' + (!string.IsNullOrEmpty(strTranslate) ? strTranslate : strSkillName);
                            if (!string.IsNullOrEmpty(strSpec))
                            {
                                strName += strSpace + '(' + strSpec + ')';
                            }
                            if (!string.IsNullOrEmpty(strValue))
                            {
                                strName += strSpace + strValue;
                            }
                            strName += strSpace + '(' + LanguageManager.GetString("Tab_Skills") + ')';
                        }
                        return false;
                    }
                case "skilltotal":
                    {
                    // Check if the total combined Ratings of Skills adds up to a particular total.
                    int intTotal = 0;
                    string[] strGroups = xmlNode.SelectSingleNodeAndCacheExpression("skills")?.Value.Split('+', StringSplitOptions.RemoveEmptyEntries);
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder sbdOutput))
                    {
                        sbdOutput.AppendLine().Append('\t');
                        if (strGroups != null)
                        {
                            // If the xmlnode contains Type element, assume that it is a Knowledge skill. 
                            if (xmlNode.SelectSingleNodeAndCacheExpression("type") != null)
                            {
                                for (int i = 0; i <= strGroups.Length - 1; ++i)
                                {
                                    foreach (KnowledgeSkill objGroup in objCharacter.SkillsSection.KnowledgeSkills)
                                    {
                                        if (objGroup.Name != strGroups[i]) continue;
                                        if (blnShowMessage)
                                            sbdOutput.Append(objGroup.CurrentDisplayName).Append(',')
                                                     .Append(strSpace);
                                        intTotal += objGroup.Rating;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 0; i <= strGroups.Length - 1; ++i)
                                {
                                    foreach (Skill objGroup in objCharacter.SkillsSection.Skills)
                                    {
                                        if (objGroup.Name != strGroups[i]) continue;
                                        if (blnShowMessage)
                                            sbdOutput.Append(objGroup.CurrentDisplayName).Append(',')
                                                     .Append(strSpace);
                                        intTotal += objGroup.Rating;
                                        break;
                                    }
                                }
                            }
                        }

                        if (!blnShowMessage)
                            return intTotal >= (xmlNode.SelectSingleNodeAndCacheExpression("val")?.ValueAsInt ?? 0);
                        if (sbdOutput.Length > 0)
                            sbdOutput.Length -= 2;
                        strName = sbdOutput + strSpace + '(' + LanguageManager.GetString("String_ExpenseSkill") + ')';
                    }

                    return intTotal >= (xmlNode.SelectSingleNodeAndCacheExpression("val")?.ValueAsInt ?? 0);
                    }
                case "skillgrouptotal":
                    {
                        // Check if the total combined Ratings of Skill Groups adds up to a particular total.
                        int intTotal = 0;
                        string[] strGroups = xmlNode.SelectSingleNodeAndCacheExpression("skillgroups")?.Value.Split('+', StringSplitOptions.RemoveEmptyEntries);
                        using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                      out StringBuilder sbdOutput))
                        {
                            sbdOutput.AppendLine().Append('\t');
                            if (strGroups != null)
                            {
                                for (int i = 0; i <= strGroups.Length - 1; ++i)
                                {
                                    foreach (SkillGroup objGroup in objCharacter.SkillsSection.SkillGroups)
                                    {
                                        if (objGroup.Name == strGroups[i])
                                        {
                                            if (blnShowMessage)
                                                sbdOutput.Append(objGroup.CurrentDisplayName).Append(',')
                                                         .Append(strSpace);
                                            intTotal += objGroup.Rating;
                                            break;
                                        }
                                    }
                                }
                            }

                            if (blnShowMessage)
                            {
                                if (sbdOutput.Length > 0)
                                    sbdOutput.Length -= 2;
                                strName = sbdOutput + strSpace + '('
                                          + LanguageManager.GetString("String_ExpenseSkillGroup") + ')';
                            }
                        }

                        return intTotal >= (xmlNode.SelectSingleNodeAndCacheExpression("val")?.ValueAsInt ?? 0);
                    }
                case "specialmodificationlimit":
                    {
                        // Add in the cost of all child components.
                        int intMods = objCharacter.Weapons.GetAllDescendants(x => x.UnderbarrelWeapons).Concat(
                                                   objCharacter.Vehicles.SelectMany(
                                                       y => y.Weapons.Concat(y.WeaponMounts.SelectMany(x => x.Weapons))
                                                             .GetAllDescendants(x => x.UnderbarrelWeapons)))
                                               .Sum(x => x.WeaponAccessories.Count(y => y.SpecialModification));
                        if (blnShowMessage)
                        {
                            strName = Environment.NewLine + '\t' + LanguageManager.GetString("String_SpecialModificationLimit")
                                      + strSpace + '≥' + strSpace + strNodeInnerText;
                        }

                        return (intMods + xmlNode.ValueAsInt) <= objCharacter.SpecialModificationLimit;
                    }
                case "spell":
                    {
                        Spell objSpell = objCharacter.Spells.FirstOrDefault(x => x.Name == strNodeInnerText);
                        if (objSpell != null)
                        {
                            if (blnShowMessage)
                                strName = objSpell.CurrentDisplayName;
                            return true;
                        }
                        if (blnShowMessage)
                        {
                            // Check for a specific Spell.
                            string strTranslate = objCharacter.LoadDataXPath("spells.xml").SelectSingleNode("/chummer/spells/spell[name = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                            strName = Environment.NewLine + '\t'
                                                          + (!string.IsNullOrEmpty(strTranslate)
                                                              ? strTranslate
                                                              : strNodeInnerText) + strSpace + '('
                                                          + LanguageManager.GetString("String_DescSpell") + ')';
                        }
                        return false;
                    }
                case "spellcategory":
                    {
                        // Check for a specified amount of a particular Spell category.
                        if (blnShowMessage)
                        {
                            string strTranslate = objCharacter.LoadDataXPath("spells.xml").SelectSingleNode("/chummer/categories/category[. = " + strNodeName.CleanXPath() + "]/@translate")?.Value;
                            strName = Environment.NewLine + '\t'
                                                          + (!string.IsNullOrEmpty(strTranslate)
                                                              ? strTranslate
                                                              : strNodeInnerText) + strSpace + '('
                                                          + LanguageManager.GetString("String_SpellCategory") + ')';
                        }
                        return objCharacter.Spells.Count(objSpell => objSpell.Category == strNodeName) >= (xmlNode.SelectSingleNodeAndCacheExpression("count")?.ValueAsInt ?? 0);
                    }
                case "spelldescriptor":
                    {
                        int intCount = xmlNode.SelectSingleNodeAndCacheExpression("count")?.ValueAsInt ?? 0;
                        // Check for a specified amount of a particular Spell Descriptor.
                        if (blnShowMessage)
                            strName = Environment.NewLine + '\t' + LanguageManager.GetString("Label_Descriptors") + strSpace + '≥' + strSpace + intCount.ToString(GlobalSettings.CultureInfo);
                        return objCharacter.Spells.Count(objSpell => objSpell.Descriptors.Contains(strNodeName)) >= intCount;
                    }
                case "streetcredvsnotoriety":
                    {
                        // Street Cred must be higher than Notoriety.
                        if (blnShowMessage)
                            strName = Environment.NewLine + '\t' + LanguageManager.GetString("String_StreetCred") + strSpace + '≥' + strSpace + LanguageManager.GetString("String_Notoriety");
                        return objCharacter.StreetCred >= objCharacter.Notoriety;
                    }
                case "submersiongrade":
                    {
                        // Character's initiate grade must be higher than or equal to the required value.
                        if (blnShowMessage)
                            strName = Environment.NewLine + '\t' + LanguageManager.GetString("String_SubmersionGrade") + strSpace + '≥' + strSpace + strNodeInnerText;
                        return objCharacter.SubmersionGrade >= xmlNode.ValueAsInt;
                    }
                case "tradition":
                    {
                        // Character needs a specific Tradition.
                        if (blnShowMessage)
                        {
                            string strTranslate = objCharacter.LoadDataXPath("traditions.xml").SelectSingleNode(
                                "/chummer/traditions/tradition[name = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                            strName = Environment.NewLine + '\t'
                                                          + (!string.IsNullOrEmpty(strTranslate)
                                                              ? strTranslate
                                                              : strNodeInnerText) + strSpace + '('
                                                          + LanguageManager.GetString("String_Tradition") + ')';
                        }
                        return objCharacter.MagicTradition.Name == strNodeInnerText;
                    }
                case "traditionspiritform":
                    {
                        // Character needs a specific spirit form provided by their Tradition.
                        if (blnShowMessage)
                        {
                            string strTranslate = objCharacter.LoadDataXPath("critterpowers.xml").SelectSingleNode(
                                "/chummer/powers/power[name = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                            strName = Environment.NewLine + '\t'
                                                          + (!string.IsNullOrEmpty(strTranslate)
                                                              ? strTranslate
                                                              : strNodeInnerText) + strSpace + '('
                                                          + LanguageManager.GetString("String_Tradition") + ')';
                        }
                        return objCharacter.MagicTradition.SpiritForm == strNodeInnerText;
                    }
                case "weapon":
                    {
                        // Character needs a specific Weapon.
                        if (!blnShowMessage) return objCharacter.Weapons.Any(w => w.Name == strNodeInnerText);
                        string strTranslate = objCharacter.LoadDataXPath("weapons.xml").SelectSingleNode(
                            "/chummer/weapons/weapon[name = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                        strName = Environment.NewLine + '\t'
                                                      + (!string.IsNullOrEmpty(strTranslate)
                                                          ? strTranslate
                                                          : strNodeInnerText) + strSpace + '('
                                                      + LanguageManager.GetString("String_Weapon") + ')';
                        return objCharacter.Weapons.Any(w => w.Name == strNodeInnerText);
                    }
                case "accessory" when objParent is Weapon objWeapon:
                    {
                        if (!blnShowMessage)
                            return objWeapon.WeaponAccessories.Any(objAccessory => objAccessory.Name == strNodeInnerText);
                        string strTranslate = objCharacter.LoadDataXPath("weapons.xml").SelectSingleNode("/chummer/accessories/accessory[name = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                        strName = Environment.NewLine + '\t'
                                                      + (!string.IsNullOrEmpty(strTranslate)
                                                          ? strTranslate
                                                          : strNodeInnerText) + strSpace + '('
                                                      + LanguageManager.GetString("String_WeaponAccessory") + ')';
                        return objWeapon.WeaponAccessories.Any(objAccessory => objAccessory.Name == strNodeInnerText);
                    }
                case "weapondetails" when objParent is Weapon objWeapon:
                    {
                        return objWeapon.GetNodeXPath().ProcessFilterOperationNode(xmlNode, false);
                    }
                case "armormod":
                    {
                        if (blnShowMessage)
                        {
                            string strTranslate = objCharacter.LoadDataXPath("armor.xml").SelectSingleNode("/chummer/armormods/armormod[name = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                            strName = Environment.NewLine + '\t'
                                                          + (!string.IsNullOrEmpty(strTranslate)
                                                              ? strTranslate
                                                              : strNodeInnerText) + strSpace + '('
                                                          + LanguageManager.GetString("String_ArmorMod") + ')';
                        }

                        if (xmlNode.GetAttribute("sameparent", string.Empty) == bool.TrueString)
                        {
                            if (objParent is Armor objArmor)
                                return objArmor.ArmorMods.Any(mod => mod.Name == strNodeInnerText);
                            return false;
                        }
                        return objCharacter.Armor.Any(armor => armor.ArmorMods.Any(mod => mod.Name == strNodeInnerText));
                    }
                default:
                    Utils.BreakIfDebug();
                    break;
            }
            if (blnShowMessage)
                strName = strNodeInnerText;
            return false;
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
            if (strAvailExpr.StartsWith("FixedValues(", StringComparison.Ordinal))
            {
                string[] strValues = strAvailExpr.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',', StringSplitOptions.RemoveEmptyEntries);
                strAvailExpr = strValues[Math.Max(Math.Min(intRating - 1, strValues.Length - 1), 0)];
            }

            if (string.IsNullOrEmpty(strAvailExpr))
                return true;
            char chrFirstAvailChar = strAvailExpr[0];
            if (chrFirstAvailChar == '+' || chrFirstAvailChar == '-')
                return true;

            strAvailExpr = strAvailExpr.TrimEndOnce(" or Gear").TrimEndOnce('F', 'R');
            int intAvail = intAvailModifier;
            object objProcess = CommonFunctions.EvaluateInvariantXPath(strAvailExpr.Replace("Rating", intRating.ToString(GlobalSettings.InvariantCultureInfo)), out bool blnIsSuccess);
            if (blnIsSuccess)
                intAvail += ((double)objProcess).StandardRound();
            return intAvail <= objCharacter.Settings.MaximumAvailability;
        }

        public static bool CheckNuyenRestriction(XmlNode objXmlGear, decimal decMaxNuyen, decimal decCostMultiplier = 1.0m, int intRating = 1)
        {
            return objXmlGear?.CreateNavigator().CheckNuyenRestriction(decMaxNuyen, decCostMultiplier, intRating) == true;
        }

        /// <summary>
        ///     Evaluates whether a given node can be purchased.
        /// </summary>
        /// <param name="objXmlGear">XPathNavigator element to evaluate.</param>
        /// <param name="decMaxNuyen">Total nuyen amount that the character possesses.</param>
        /// <param name="decCostMultiplier">Multiplier of the object's cost value.</param>
        /// <param name="intRating">Effective Rating of the object.</param>
        /// <returns>Returns False if not permitted with the current restrictions. Returns True if valid.</returns>
        public static bool CheckNuyenRestriction(this XPathNavigator objXmlGear, decimal decMaxNuyen, decimal decCostMultiplier = 1.0m, int intRating = 1)
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
                if (strCost.StartsWith("FixedValues(", StringComparison.Ordinal))
                {
                    string[] strValues = strCost.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',', StringSplitOptions.RemoveEmptyEntries);
                    strCost = strValues[Math.Max(Math.Min(intRating, strValues.Length) - 1, 0)];
                }
                else if (strCost.StartsWith("Variable", StringComparison.Ordinal))
                {
                    strCost = strCost.TrimStartOnce("Variable(", true).TrimEndOnce(')');
                    int intHyphenIndex = strCost.IndexOf('-');
                    strCost = intHyphenIndex != -1 ? strCost.Substring(0, intHyphenIndex) : strCost.FastEscape('+');
                }

                object objProcess = CommonFunctions.EvaluateInvariantXPath(strCost.Replace("Rating", intRating.ToString(GlobalSettings.InvariantCultureInfo)), out bool blnIsSuccess);
                if (blnIsSuccess)
                    decCost = Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo);
            }
            return decMaxNuyen >= decCost * decCostMultiplier;
        }
    }
}
