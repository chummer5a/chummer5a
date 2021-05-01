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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace Chummer
{
    public static class SelectionShared
    {
        #region XmlNode overloads for selection methods.

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
        public static async Task<bool> RequirementsMet(this XmlNode xmlNode, Character objCharacter, object objParent = null, string strLocalName = "", string strIgnoreQuality = "", string strSourceName = "", string strLocation = "", bool blnIgnoreLimit = false)
        {
            if (xmlNode == null || objCharacter == null)
                return false;
            // Ignore the rules.
            return objCharacter.IgnoreRules || await xmlNode.CreateNavigator().RequirementsMet(objCharacter, objParent, strLocalName, strIgnoreQuality, strSourceName, strLocation, blnIgnoreLimit);
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
            return objXmlGear != null && objXmlGear.CreateNavigator().CheckAvailRestriction(objCharacter, intRating, intAvailModifier);
        }

        public static bool CheckNuyenRestriction(XmlNode objXmlGear, decimal decMaxNuyen, decimal decCostMultiplier = 1.0m, int intRating = 1)
        {
            return objXmlGear != null && objXmlGear.CreateNavigator().CheckNuyenRestriction(decMaxNuyen, decCostMultiplier, intRating);
        }
        #endregion

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
        public static async Task<bool> RequirementsMet(this XPathNavigator xmlNode, Character objCharacter, object objParent = null, string strLocalName = "", string strIgnoreQuality = "", string strSourceName = "", string strLocation = "", bool blnIgnoreLimit = false)
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
                if (xmlNode.SelectSingleNode("chargenonly") != null)
                {
                    if (blnShowMessage)
                    {
                        Program.MainForm.ShowMessageBox(
                            string.Format(
                                GlobalOptions.CultureInfo,
                                LanguageManager.GetString("Message_SelectGeneric_ChargenRestriction"),
                                strLocalName),
                            string.Format(
                                GlobalOptions.CultureInfo,
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
                if (xmlNode.SelectSingleNode("careeronly") != null)
                {
                    if (blnShowMessage)
                    {
                        Program.MainForm.ShowMessageBox(
                            string.Format(
                                GlobalOptions.CultureInfo,
                                LanguageManager.GetString("Message_SelectGeneric_CareerOnlyRestriction"),
                                strLocalName),
                            string.Format(
                                GlobalOptions.CultureInfo,
                                LanguageManager.GetString("MessageTitle_SelectGeneric_Restriction"),
                                strLocalName),
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    return false;
                }
                if (objCharacter.EffectiveBuildMethodUsesPriorityTables)
                {
                    if (xmlNode.SelectSingleNode("onlyprioritygiven") != null)
                    {
                        if (blnShowMessage)
                        {
                            Program.MainForm.ShowMessageBox(
                                string.Format(
                                    GlobalOptions.CultureInfo,
                                    LanguageManager.GetString("Message_SelectGeneric_PriorityRestriction"),
                                    strLocalName),
                                string.Format(
                                    GlobalOptions.CultureInfo,
                                    LanguageManager.GetString("MessageTitle_SelectGeneric_Restriction"),
                                    strLocalName),
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        return false;
                    }
                }
            }
            if (!blnIgnoreLimit)
            {
                // See if the character already has this Quality and whether or not multiple copies are allowed.
                // If the limit at chargen is different from the actual limit, we need to make sure we fetch the former if the character is in Create mode
                string strLimitString = xmlNode.SelectSingleNode("chargenlimit")?.Value;
                if (string.IsNullOrWhiteSpace(strLimitString) || objCharacter.Created)
                {
                    strLimitString = xmlNode.SelectSingleNode("limit")?.Value;
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
                    StringBuilder objLimitString = new StringBuilder(strLimitString);
                    objCharacter.AttributeSection.ProcessAttributesInXPath(objLimitString, strLimitString);
                    foreach (string strLimb in Character.LimbStrings)
                    {
                        objLimitString.CheapReplace(strLimitString, "{" + strLimb + "}", () => (string.IsNullOrEmpty(strLocation) ? objCharacter.LimbCount(strLimb) : objCharacter.LimbCount(strLimb) / 2).ToString(GlobalOptions.InvariantCultureInfo));
                    }

                    object objProcess = CommonFunctions.EvaluateInvariantXPath(objLimitString.ToString(), out bool blnIsSuccess);
                    strLimitString = blnIsSuccess ? objProcess.ToString() : "1";

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
                                List<MartialArtTechnique> objTempList;
                                if (objParent is MartialArt objArt)
                                {
                                    objTempList = new List<MartialArtTechnique>(objArt.Techniques);
                                }
                                else
                                {
                                    objTempList = new List<MartialArtTechnique>(objCharacter.MartialArts.Count);
                                    foreach (MartialArt objMartialArt in objCharacter.MartialArts)
                                    {
                                        objTempList.AddRange(objMartialArt.Techniques);
                                    }
                                }
                                objListToCheck = objTempList;
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

                    int intLimit = Convert.ToInt32(strLimitString, GlobalOptions.InvariantCultureInfo);
                    int intExtendedLimit = intLimit;
                    string strLimitWithInclusions = xmlNode.SelectSingleNode("limitwithinclusions")?.Value;
                    if (!string.IsNullOrEmpty(strLimitWithInclusions))
                    {
                        intExtendedLimit = Convert.ToInt32(strLimitWithInclusions, GlobalOptions.InvariantCultureInfo);
                    }
                    int intCount = 0;
                    int intExtendedCount = 0;
                    if (objListToCheck != null || blnCheckCyberwareChildren)
                    {
                        List<IHasName> lstToCheck = objListToCheck?.ToList();
                        string strNameNode = xmlNode.SelectSingleNode("name")?.Value ?? string.Empty;
                        if (blnCheckCyberwareChildren)
                        {
                            intCount = string.IsNullOrEmpty(strLocation)
                                ? objCharacter.Cyberware.DeepCount(x => x.Children, x => string.IsNullOrEmpty(x.PlugsIntoModularMount) && strNameNode == x.Name)
                                : objCharacter.Cyberware.DeepCount(x => x.Children, x => string.IsNullOrEmpty(x.PlugsIntoModularMount) && x.Location == strLocation && strNameNode == x.Name);
                        }
                        else
                            intCount = lstToCheck?.Count(objItem => strNameNode == objItem.Name) ?? 0;
                        intExtendedCount = intCount;
                        // In case one item is split up into multiple entries with different names, e.g. Indomitable quality, we need to be able to check all those entries against the limit
                        XPathNavigator xmlIncludeInLimit = xmlNode.SelectSingleNode("includeinlimit");
                        if (xmlIncludeInLimit != null)
                        {
                            List<string> lstNamesIncludedInLimit = new List<string>(1);
                            if (!string.IsNullOrEmpty(strNameNode))
                            {
                                lstNamesIncludedInLimit.Add(strNameNode);
                            }
                            foreach (XPathNavigator objChildXml in xmlIncludeInLimit.SelectChildren(XPathNodeType.Element))
                            {
                                lstNamesIncludedInLimit.Add(objChildXml.Value);
                            }

                            if (blnCheckCyberwareChildren)
                            {
                                intExtendedCount = string.IsNullOrEmpty(strLocation)
                                    ? objCharacter.Cyberware.DeepCount(x => x.Children, x => string.IsNullOrEmpty(x.PlugsIntoModularMount) && lstNamesIncludedInLimit.Any(objLimitName => objLimitName == x.Name))
                                    : objCharacter.Cyberware.DeepCount(x => x.Children, x => string.IsNullOrEmpty(x.PlugsIntoModularMount) && x.Location == strLocation && lstNamesIncludedInLimit.Any(strName => strName == x.Name));
                            }
                            else
                                intExtendedCount = lstToCheck?.Count(objItem => lstNamesIncludedInLimit.Any(objLimitName => objLimitName == objItem.Name)) ?? 0;
                        }
                    }
                    if (intCount >= intLimit || intExtendedCount >= intExtendedLimit)
                    {
                        if (blnShowMessage)
                        {
                            Program.MainForm.ShowMessageBox(
                                string.Format(
                                    GlobalOptions.CultureInfo,
                                    LanguageManager.GetString("Message_SelectGeneric_Limit"),
                                    strLocalName, intLimit == 0 ? 1 : intLimit),
                                string.Format(
                                    GlobalOptions.CultureInfo,
                                    LanguageManager.GetString("MessageTitle_SelectGeneric_Limit"),
                                    strLocalName),
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        return false;
                    }
                }
            }

            XPathNavigator xmlForbiddenNode = xmlNode.SelectSingleNode("forbidden");
            if (xmlForbiddenNode != null)
            {
                // Loop through the oneof requirements.
                foreach (XPathNavigator objXmlOneOf in xmlForbiddenNode.Select("oneof"))
                {
                    foreach (XPathNavigator xmlForbiddenItemNode in objXmlOneOf.SelectChildren(XPathNodeType.Element))
                    {
                        // The character is not allowed to take the Quality, so display a message and uncheck the item.
                        Tuple<bool, string> tupResult = await xmlForbiddenItemNode.TestNodeRequirements(objCharacter, objParent, strIgnoreQuality, blnShowMessage);
                        if (tupResult.Item1)
                        {
                            if (blnShowMessage)
                            {
                                Program.MainForm.ShowMessageBox(
                                    string.Format(
                                        GlobalOptions.CultureInfo,
                                        LanguageManager.GetString("Message_SelectGeneric_Restriction"),
                                        strLocalName) + tupResult.Item2,
                                    string.Format(
                                        GlobalOptions.CultureInfo,
                                        LanguageManager.GetString("MessageTitle_SelectGeneric_Restriction"),
                                        strLocalName),
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            return false;
                        }
                    }
                }
            }

            XPathNavigator xmlRequiredNode = xmlNode.SelectSingleNode("required");
            if (xmlRequiredNode != null)
            {
                StringBuilder objRequirement = new StringBuilder();
                bool blnRequirementMet = true;

                // Loop through the oneof requirements.
                foreach (XPathNavigator objXmlOneOf in xmlRequiredNode.Select("oneof"))
                {
                    bool blnOneOfMet = false;
                    StringBuilder objThisRequirement = new StringBuilder(Environment.NewLine + LanguageManager.GetString("Message_SelectQuality_OneOf"));
                    foreach (XPathNavigator xmlRequiredItemNode in objXmlOneOf.SelectChildren(XPathNodeType.Element))
                    {
                        Tuple<bool, string> tupResult = await xmlRequiredItemNode.TestNodeRequirements(objCharacter, objParent, strIgnoreQuality, blnShowMessage);
                        if (tupResult.Item1)
                        {
                            blnOneOfMet = true;
                            if (!blnShowMessage)
                                break;
                        }
                        if (blnShowMessage)
                            objThisRequirement.Append(tupResult.Item2);
                    }

                    // Update the flag for requirements met.
                    if (!blnOneOfMet)
                    {
                        blnRequirementMet = false;
                        if (blnShowMessage)
                            objRequirement.Append(objThisRequirement);
                    }
                    if (!blnRequirementMet && !blnShowMessage)
                        break;
                }

                if (blnRequirementMet || blnShowMessage)
                {
                    // Loop through the allof requirements.
                    foreach (XPathNavigator objXmlAllOf in xmlRequiredNode.Select("allof"))
                    {
                        bool blnAllOfMet = true;
                        StringBuilder objThisRequirement = new StringBuilder(Environment.NewLine + LanguageManager.GetString("Message_SelectQuality_AllOf"));
                        foreach (XPathNavigator xmlRequiredItemNode in objXmlAllOf.SelectChildren(XPathNodeType.Element))
                        {
                            // If this item was not found, fail the AllOfMet condition.
                            Tuple<bool, string> tupResult = await xmlRequiredItemNode.TestNodeRequirements(objCharacter, objParent, strIgnoreQuality, blnShowMessage);
                            if (tupResult.Item1)
                                continue;
                            blnAllOfMet = false;
                            if (blnShowMessage)
                                objThisRequirement.Append(tupResult.Item2);
                            else
                                break;
                        }

                        // Update the flag for requirements met.
                        if (!blnAllOfMet)
                        {
                            blnRequirementMet = false;
                            if (blnShowMessage)
                                objRequirement.Append(objThisRequirement);
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
                        Program.MainForm.ShowMessageBox(
                            string.Format(
                                GlobalOptions.CultureInfo,
                                LanguageManager.GetString("Message_SelectGeneric_Requirement"),
                                strLocalName) + objRequirement,
                            string.Format(
                                GlobalOptions.CultureInfo,
                                LanguageManager.GetString("MessageTitle_SelectGeneric_Requirement"),
                                strLocalName),
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    return false;
                }
            }
            return true;
        }

        public static async Task<Tuple<bool, string>> TestNodeRequirements(this XPathNavigator xmlNode, Character objCharacter, object objParent, string strIgnoreQuality = "", bool blnShowMessage = true)
        {
            string strName = string.Empty;
            if (xmlNode == null || objCharacter == null)
            {
                return new Tuple<bool, string>(false, strName);
            }

            string strSpace = LanguageManager.GetString("String_Space");
            string strNodeInnerText = xmlNode.Value;
            string strNodeName = xmlNode.SelectSingleNode("name")?.Value ?? string.Empty;
            switch (xmlNode.Name)
            {
                case "attribute":
                    {
                        // Check to see if an Attribute meets a requirement.
                        CharacterAttrib objAttribute = objCharacter.GetAttribute(strNodeName);
                        int intTargetValue = Convert.ToInt32(xmlNode.SelectSingleNode("total")?.Value, GlobalOptions.InvariantCultureInfo);
                        if (blnShowMessage)
                            strName = string.Format(GlobalOptions.CultureInfo, "{0}\t{1}{2}{3}", Environment.NewLine, objAttribute.DisplayAbbrev, strSpace, intTargetValue);
                        // Special cases for when we want to check if a special attribute is enabled
                        if (intTargetValue == 1)
                        {
                            switch (objAttribute.Abbrev)
                            {
                                case "MAG":
                                    return new Tuple<bool, string>(objCharacter.MAGEnabled, strName);
                                case "MAGAdept":
                                    return new Tuple<bool, string>(objCharacter.MAGEnabled && objCharacter.IsMysticAdept, strName);
                                case "RES":
                                    return new Tuple<bool, string>(objCharacter.RESEnabled, strName);
                                case "DEP":
                                    return new Tuple<bool, string>(objCharacter.DEPEnabled, strName);
                            }
                        }

                        if (xmlNode.SelectSingleNode("natural") != null)
                        {
                            return new Tuple<bool, string>(objAttribute.Value >= intTargetValue, strName);
                        }
                        return new Tuple<bool, string>(objAttribute.TotalValue >= intTargetValue, strName);
                    }
                case "attributetotal":
                    {
                        string strNodeAttributes = xmlNode.SelectSingleNode("attributes")?.Value ?? string.Empty;
                        int intNodeVal = Convert.ToInt32(xmlNode.SelectSingleNode("val")?.Value, GlobalOptions.InvariantCultureInfo);
                        // Check if the character's Attributes add up to a particular total.
                        string strAttributes = strNodeAttributes;
                        string strValue = strNodeAttributes;
                        foreach (string strAttribute in AttributeSection.AttributeStrings)
                        {
                            CharacterAttrib objLoopAttrib = objCharacter.GetAttribute(strAttribute);
                            if (strNodeAttributes.Contains(objLoopAttrib.Abbrev))
                            {
                                strAttributes = strAttributes.Replace(strAttribute, objLoopAttrib.DisplayAbbrev);
                                strValue = strValue.Replace(strAttribute, xmlNode.SelectSingleNode("natural") != null
                                    ? objLoopAttrib.Value.ToString(GlobalOptions.InvariantCultureInfo)
                                    : objLoopAttrib.TotalValue.ToString(GlobalOptions.InvariantCultureInfo));
                            }
                        }
                        if (blnShowMessage)
                            strName = string.Format(GlobalOptions.CultureInfo, "{0}\t{2}{1}{3}", Environment.NewLine, strSpace, strAttributes, intNodeVal);
                        object objProcess = CommonFunctions.EvaluateInvariantXPath(strValue, out bool blnIsSuccess);
                        return new Tuple<bool, string>((blnIsSuccess ? ((double)objProcess).StandardRound() : 0) >= intNodeVal, strName);
                    }
                case "careerkarma":
                    {
                        // Check Career Karma requirement.
                        if (blnShowMessage)
                            strName = Environment.NewLine + '\t' + string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("Message_SelectQuality_RequireKarma"), strNodeInnerText);
                        return new Tuple<bool, string>(objCharacter.CareerKarma >= Convert.ToInt32(strNodeInnerText, GlobalOptions.InvariantCultureInfo), strName);
                    }
                case "chargenonly":
                    {
                        if (blnShowMessage)
                            strName = Environment.NewLine + '\t' + LanguageManager.GetString("Message_SelectGeneric_ChargenRestriction");
                        return new Tuple<bool, string>(!objCharacter.Created, strName);
                    }
                case "careeronly":
                    {
                        if (blnShowMessage)
                            strName = Environment.NewLine + '\t' + LanguageManager.GetString("Message_SelectGeneric_CareerOnlyRestriction");
                        return new Tuple<bool, string>(objCharacter.Created, strName);
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
                                return new Tuple<bool, string>(true, strName);
                            }
                        }
                        if (blnShowMessage)
                        {
                            string strTranslate = (await objCharacter.LoadDataXPathAsync("critterpowers.xml")).SelectSingleNode(
                                "/chummer/powers/power[name = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                            strName = string.Format(GlobalOptions.CultureInfo, "{0}\t{2}{1}({3})",
                                Environment.NewLine, strSpace, !string.IsNullOrEmpty(strTranslate) ? strTranslate : strNodeInnerText, LanguageManager.GetString("Tab_Critter"));
                        }
                        return new Tuple<bool, string>(false, strName);
                    }
                case "bioware":
                    {
                        int count = Convert.ToInt32(xmlNode.SelectSingleNode("@count")?.Value ?? "1", GlobalOptions.InvariantCultureInfo);
                        if (blnShowMessage)
                        {
                            string strTranslate = (await objCharacter.LoadDataXPathAsync("bioware.xml")).SelectSingleNode(
                                "/chummer/biowares/bioware[name = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                            strName = string.Format(GlobalOptions.CultureInfo, "{0}\t{2}{1}{3}",
                                Environment.NewLine, strSpace, LanguageManager.GetString("Label_Bioware"), !string.IsNullOrEmpty(strTranslate) ? strTranslate : strNodeInnerText);
                        }
                        string strWareNodeSelectAttribute = xmlNode.SelectSingleNode("@select")?.Value ?? string.Empty;
                        return new Tuple<bool, string>(objCharacter.Cyberware.DeepCount(x => x.Children, objCyberware => objCyberware.Name == strNodeInnerText &&
                                objCyberware.SourceType == Improvement.ImprovementSource.Bioware && string.IsNullOrEmpty(objCyberware.PlugsIntoModularMount) &&
                               (string.IsNullOrEmpty(strWareNodeSelectAttribute) || strWareNodeSelectAttribute == objCyberware.Extra)) >= count, strName);
                    }
                case "cyberware":
                    {
                        int count = Convert.ToInt32(xmlNode.SelectSingleNode("@count")?.Value ?? "1", GlobalOptions.InvariantCultureInfo);
                        if (blnShowMessage)
                        {
                            string strTranslate = (await objCharacter.LoadDataXPathAsync("cyberware.xml")).SelectSingleNode(
                                "/chummer/cyberwares/cyberware[name = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                            strName = string.Format(GlobalOptions.CultureInfo, "{0}\t{2}{1}{3}",
                                Environment.NewLine, strSpace, LanguageManager.GetString("Label_Cyberware"), !string.IsNullOrEmpty(strTranslate) ? strTranslate : strNodeInnerText);
                        }
                        string strWareNodeSelectAttribute = xmlNode.SelectSingleNode("@select")?.Value ?? string.Empty;
                        return new Tuple<bool, string>(objCharacter.Cyberware.DeepCount(x => x.Children, objCyberware => objCyberware.Name == strNodeInnerText &&
                                objCyberware.SourceType == Improvement.ImprovementSource.Cyberware && string.IsNullOrEmpty(objCyberware.PlugsIntoModularMount) &&
                               (string.IsNullOrEmpty(strWareNodeSelectAttribute) || strWareNodeSelectAttribute == objCyberware.Extra)) >= count, strName);
                    }
                case "biowarecontains":
                    {
                        int count = Convert.ToInt32(xmlNode.SelectSingleNode("@count")?.Value ?? "1", GlobalOptions.InvariantCultureInfo);
                        if (blnShowMessage)
                        {
                            string strTranslate = (await objCharacter.LoadDataXPathAsync("bioware.xml")).SelectSingleNode(
                                "/chummer/biowares/bioware[name = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                            strName = string.Format(GlobalOptions.CultureInfo, "{0}\t{2}{1}{3}",
                                Environment.NewLine, strSpace, LanguageManager.GetString("Label_Bioware"), !string.IsNullOrEmpty(strTranslate) ? strTranslate : strNodeInnerText);
                        }
                        string strWareNodeSelectAttribute = xmlNode.SelectSingleNode("@select")?.Value ?? string.Empty;
                        return new Tuple<bool, string>(objCharacter.Cyberware.DeepCount(x => x.Children, objCyberware => objCyberware.Name.Contains(strNodeInnerText) &&
                                objCyberware.SourceType == Improvement.ImprovementSource.Bioware && string.IsNullOrEmpty(objCyberware.PlugsIntoModularMount) &&
                               (string.IsNullOrEmpty(strWareNodeSelectAttribute) || strWareNodeSelectAttribute == objCyberware.Extra)) >= count, strName);
                    }
                case "cyberwarecontains":
                    {
                        int count = Convert.ToInt32(xmlNode.SelectSingleNode("@count")?.Value ?? "1", GlobalOptions.InvariantCultureInfo);
                        if (blnShowMessage)
                        {
                            string strTranslate = (await objCharacter.LoadDataXPathAsync("cyberware.xml")).SelectSingleNode(
                                "/chummer/cyberwares/cyberware[name = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                            strName = string.Format(GlobalOptions.CultureInfo, "{0}\t{2}{1}{3}",
                                Environment.NewLine, strSpace, LanguageManager.GetString("Label_Cyberware"), !string.IsNullOrEmpty(strTranslate) ? strTranslate : strNodeInnerText);
                        }
                        string strWareNodeSelectAttribute = xmlNode.SelectSingleNode("@select")?.Value ?? string.Empty;
                        return new Tuple<bool, string>(objCharacter.Cyberware.DeepCount(x => x.Children, objCyberware => objCyberware.Name.Contains(strNodeInnerText) &&
                                objCyberware.SourceType == Improvement.ImprovementSource.Cyberware && string.IsNullOrEmpty(objCyberware.PlugsIntoModularMount) &&
                               (string.IsNullOrEmpty(strWareNodeSelectAttribute) || strWareNodeSelectAttribute == objCyberware.Extra)) >= count, strName);
                    }
                case "damageresistance":
                    {
                        // Damage Resistance must be a particular value.
                        if (blnShowMessage)
                            strName = Environment.NewLine + '\t' + LanguageManager.GetString("String_DamageResistance");
                        return new Tuple<bool, string>(
                            objCharacter.BOD.TotalValue + ImprovementManager.ValueOf(objCharacter,
                                Improvement.ImprovementType.DamageResistance) >= Convert.ToInt32(strNodeInnerText,
                                GlobalOptions.InvariantCultureInfo), strName);
                    }
                case "ess":
                    {
                        string strEssNodeGradeAttributeText = xmlNode.SelectSingleNode("@grade")?.Value ?? string.Empty;
                        if (!string.IsNullOrEmpty(strEssNodeGradeAttributeText))
                        {
                            HashSet<string> setEssNodeGradeAttributeText = new HashSet<string>(strEssNodeGradeAttributeText.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries));
                            decimal decGrade =
                                objCharacter.Cyberware.Where(
                                        objCyberware =>
                                            setEssNodeGradeAttributeText.Any(func => objCyberware.Grade.Name.Contains(func)))
                                    .AsParallel().Sum(objCyberware => objCyberware.CalculatedESS);
                            if (strNodeInnerText.StartsWith('-'))
                            {
                                // Essence must be less than the value.
                                if (blnShowMessage)
                                    strName = Environment.NewLine + '\t' +
                                              string.Format(GlobalOptions.CultureInfo
                                                  , LanguageManager.GetString("Message_SelectQuality_RequireESSGradeBelow")
                                                  , strNodeInnerText
                                                  , strEssNodeGradeAttributeText
                                                  , decGrade.ToString(GlobalOptions.CultureInfo));
                                return new Tuple<bool, string>(decGrade < Convert.ToDecimal(strNodeInnerText.TrimStart('-'), GlobalOptions.InvariantCultureInfo), strName);
                            }
                            // Essence must be equal to or greater than the value.
                            if (blnShowMessage)
                                strName = Environment.NewLine + '\t' +
                                          string.Format(GlobalOptions.CultureInfo
                                              , LanguageManager.GetString("Message_SelectQuality_RequireESSAbove")
                                              , strNodeInnerText
                                              , strEssNodeGradeAttributeText
                                              , decGrade.ToString(GlobalOptions.CultureInfo));
                            return new Tuple<bool, string>(decGrade >= Convert.ToDecimal(strNodeInnerText, GlobalOptions.InvariantCultureInfo), strName);
                        }
                        // Check Essence requirement.
                        if (strNodeInnerText.StartsWith('-'))
                        {
                            // Essence must be less than the value.
                            if (blnShowMessage)
                                strName = Environment.NewLine + '\t' +
                                          string.Format(GlobalOptions.CultureInfo
                                              , LanguageManager.GetString("Message_SelectQuality_RequireESSBelow")
                                              , strNodeInnerText
                                              , objCharacter.Essence().ToString(GlobalOptions.CultureInfo));
                            return new Tuple<bool, string>(objCharacter.Essence() < Convert.ToDecimal(strNodeInnerText.TrimStart('-'), GlobalOptions.InvariantCultureInfo), strName);
                        }
                        // Essence must be equal to or greater than the value.
                        if (blnShowMessage)
                            strName = Environment.NewLine + '\t' +
                                      string.Format(GlobalOptions.CultureInfo
                                          , LanguageManager.GetString("Message_SelectQuality_RequireESSAbove")
                                          , strNodeInnerText
                                          , objCharacter.Essence().ToString(GlobalOptions.CultureInfo));
                        return new Tuple<bool, string>(objCharacter.Essence() >= Convert.ToDecimal(strNodeInnerText, GlobalOptions.InvariantCultureInfo), strName);
                    }
                case "echo":
                    {
                        Metamagic objMetamagic = objCharacter.Metamagics.FirstOrDefault(x => x.Name == strNodeInnerText && x.SourceType == Improvement.ImprovementSource.Echo);
                        if (objMetamagic != null)
                        {
                            if (blnShowMessage)
                                strName = objMetamagic.CurrentDisplayName;
                            return new Tuple<bool, string>(true, strName);
                        }
                        if (blnShowMessage)
                        {
                            string strTranslate = (await objCharacter.LoadDataXPathAsync("echoes.xml")).SelectSingleNode(
                                "/chummer/echoes/echo[name = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                            strName = string.Format(GlobalOptions.CultureInfo, "{0}\t{2}{1}({3})",
                                Environment.NewLine, strSpace, !string.IsNullOrEmpty(strTranslate) ? strTranslate : strNodeInnerText, LanguageManager.GetString("String_Echo"));
                        }
                        return new Tuple<bool, string>(false, strName);
                    }
                case "gameplayoption":
                {
                    // A particular gameplay option is required.
                    if (blnShowMessage)
                        strName = string.Format("{0}\t{2}{1}={1}{3}", Environment.NewLine, strSpace, LanguageManager.GetString("String_GameplayOption"), strNodeInnerText);
                    return new Tuple<bool, string>(objCharacter.CharacterOptionsKey == strNodeInnerText, strName);
                }
                case "gear":
                    {
                        Gear objGear = objCharacter.Gear.FirstOrDefault(x => x.Name == strNodeInnerText);
                        //TODO: Probably a better way to handle minrating/rating/maxrating but eh, YAGNI.

                        if (xmlNode.SelectSingleNode("@minrating")?.Value != null)
                        {
                            int rating = Convert.ToInt32(xmlNode.SelectSingleNode("@minrating")?.Value, GlobalOptions.InvariantCultureInfo);
                            objGear = objCharacter.Gear.FirstOrDefault(x => x.Name == strNodeInnerText && x.Rating >= rating);
                        }
                        else if (xmlNode.SelectSingleNode("@rating")?.Value != null)
                        {
                            int rating = Convert.ToInt32(xmlNode.SelectSingleNode("@rating")?.Value, GlobalOptions.InvariantCultureInfo);
                            objGear = objCharacter.Gear.FirstOrDefault(x => x.Name == strNodeInnerText && x.Rating == rating);
                        }
                        else if (xmlNode.SelectSingleNode("@maxrating")?.Value != null)
                        {
                            int rating = Convert.ToInt32(xmlNode.SelectSingleNode("@maxrating")?.Value, GlobalOptions.InvariantCultureInfo);
                            objGear = objCharacter.Gear.FirstOrDefault(x => x.Name == strNodeInnerText && x.Rating <= rating);
                        }
                        if (objGear != null)
                        {
                            if (blnShowMessage)
                                strName = objGear.CurrentDisplayNameShort;
                            return new Tuple<bool, string>(true, strName);
                        }
                        if (blnShowMessage)
                        {
                            // Character needs a specific Martial Art.
                            string strTranslate = (await objCharacter.LoadDataXPathAsync("gear.xml")).SelectSingleNode(
                                "/chummer/gears/gear[name = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                            strName = string.Format(GlobalOptions.CultureInfo, "{0}\t{2}{1}({3})",
                                Environment.NewLine, strSpace, !string.IsNullOrEmpty(strTranslate) ? strTranslate : strNodeInnerText, LanguageManager.GetString("String_Gear"));
                        }
                        return new Tuple<bool, string>(false, strName);
                    }
                case "group":
                    {
                        // Check that clustered options are present (Magical Tradition + Skill 6, for example)
                        bool blnResult = true;
                        StringBuilder sbdResultName = new StringBuilder(Environment.NewLine + '\t' + LanguageManager.GetString("Message_SelectQuality_AllOf"));
                        foreach (XPathNavigator xmlChildNode in xmlNode.SelectChildren(XPathNodeType.Element))
                        {
                            Tuple<bool, string> tupLoopResult = await xmlChildNode.TestNodeRequirements(objCharacter, objParent, strIgnoreQuality, blnShowMessage);
                            blnResult = blnResult && tupLoopResult.Item1;
                            if (!blnResult && !blnShowMessage)
                                break;
                            if (!tupLoopResult.Item1)
                                sbdResultName.Append(tupLoopResult.Item2.Replace(Environment.NewLine + '\t', Environment.NewLine + '\t' + '\t'));
                        }
                        if (blnShowMessage)
                            strName = sbdResultName.ToString();
                        return new Tuple<bool, string>(blnResult, strName);
                    }
                case "grouponeof":
                {
                    // Check that one of the clustered options are present
                    bool blnResult = false;
                    StringBuilder sbdResultName = new StringBuilder(Environment.NewLine + '\t' + LanguageManager.GetString("Message_SelectQuality_OneOf"));
                    foreach (XPathNavigator xmlChildNode in xmlNode.SelectChildren(XPathNodeType.Element))
                    {
                        Tuple<bool, string> tupLoopResult = await xmlChildNode.TestNodeRequirements(objCharacter, objParent, strIgnoreQuality, blnShowMessage);
                        blnResult = tupLoopResult.Item1 || blnResult;
                        if (blnResult && !blnShowMessage)
                            break;
                        sbdResultName.Append(tupLoopResult.Item2.Replace(Environment.NewLine + '\t', Environment.NewLine + '\t' + '\t'));
                    }
                    if (blnShowMessage)
                        strName = sbdResultName.ToString();
                    return new Tuple<bool, string>(blnResult, strName);
                    }
                case "initiategrade":
                    {
                        // Character's initiate grade must be higher than or equal to the required value.
                        if (blnShowMessage)
                            strName = Environment.NewLine + '\t' + LanguageManager.GetString("String_InitiateGrade") + strSpace + '≥' + strSpace + strNodeInnerText;
                        return new Tuple<bool, string>(objCharacter.InitiateGrade >= Convert.ToInt32(strNodeInnerText, GlobalOptions.InvariantCultureInfo), strName);
                    }
                case "martialart":
                    {
                        MartialArt objMartialArt = objCharacter.MartialArts.FirstOrDefault(x => x.Name == strNodeInnerText);
                        if (objMartialArt != null)
                        {
                            if (blnShowMessage)
                                strName = objMartialArt.CurrentDisplayName;
                            return new Tuple<bool, string>(true, strName);
                        }
                        if (blnShowMessage)
                        {
                            // Character needs a specific Martial Art.
                            string strTranslate = (await objCharacter.LoadDataXPathAsync("martialarts.xml")).SelectSingleNode(
                                "/chummer/martialarts/martialart[name = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                            strName = string.Format(GlobalOptions.CultureInfo, "{0}\t{2}{1}({3})",
                                Environment.NewLine, strSpace, !string.IsNullOrEmpty(strTranslate) ? strTranslate : strNodeInnerText, LanguageManager.GetString("String_MartialArt"));
                        }
                        return new Tuple<bool, string>(false, strName);
                    }
                case "martialtechnique":
                    {
                        foreach (MartialArt objMartialArt in objCharacter.MartialArts)
                        {
                            MartialArtTechnique objMartialArtTechnique = objMartialArt.Techniques.FirstOrDefault(x => x.Name == strNodeInnerText);
                            if (objMartialArtTechnique != null)
                            {
                                if (blnShowMessage)
                                    strName = objMartialArtTechnique.CurrentDisplayName;
                                return new Tuple<bool, string>(true, strName);
                            }
                        }
                        if (blnShowMessage)
                        {
                            // Character needs a specific Martial Arts technique.
                            string strTranslate = (await objCharacter.LoadDataXPathAsync("martialarts.xml")).SelectSingleNode(
                                "/chummer/techniques/technique[name = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                            strName = string.Format(GlobalOptions.CultureInfo, "{0}\t{2}{1}({3})",
                                Environment.NewLine, strSpace, !string.IsNullOrEmpty(strTranslate) ? strTranslate : strNodeInnerText, LanguageManager.GetString("String_MartialArt"));
                        }
                        return new Tuple<bool, string>(false, strName);
                    }
                case "metamagic":
                    {
                        Metamagic objMetamagic = objCharacter.Metamagics.FirstOrDefault(x => x.Name == strNodeInnerText && x.SourceType == Improvement.ImprovementSource.Metamagic);
                        if (objMetamagic != null)
                        {
                            if (blnShowMessage)
                                strName = objMetamagic.CurrentDisplayName;
                            return new Tuple<bool, string>(true, strName);
                        }
                        if (blnShowMessage)
                        {
                            string strTranslate = (await objCharacter.LoadDataXPathAsync("metamagic.xml")).SelectSingleNode(
                                "/chummer/metamagics/metamagic[name = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                            strName = string.Format(GlobalOptions.CultureInfo, "{0}\t{2}{1}({3})",
                                Environment.NewLine, strSpace, !string.IsNullOrEmpty(strTranslate) ? strTranslate : strNodeInnerText, LanguageManager.GetString("String_Metamagic"));
                        }
                        return new Tuple<bool, string>(false, strName);
                    }
                case "metamagicart":
                case "art":
                {
                        // Street Grimoire adds High Arts, which group metamagics and such together. If we're ignoring this requirement
                        if (objCharacter.Options.IgnoreArt)
                        {
                            // If we're looking for an art, return true.
                            if (xmlNode.Name == "art")
                            {
                                return new Tuple<bool, string>(true, strName);
                            }

                            XPathNavigator xmlMetamagicDoc = (await objCharacter.LoadDataXPathAsync("metamagic.xml"))
                                .SelectSingleNode("/chummer");
                            if (blnShowMessage)
                            {
                                string strTranslateArt = xmlMetamagicDoc
                                    ?.SelectSingleNode("arts/art[name = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                                strName = string.Format(GlobalOptions.CultureInfo, "{0}\t{2}{1}({3})",
                                    Environment.NewLine, strSpace, !string.IsNullOrEmpty(strTranslateArt) ? strTranslateArt : strNodeInnerText, LanguageManager.GetString("String_Art"));
                            }

                            if (xmlMetamagicDoc == null)
                                return new Tuple<bool, string>(true, strName);
                            // Loop through the data file for each metamagic to find the Required and Forbidden nodes.
                            foreach (Metamagic metamagic in objCharacter.Metamagics)
                            {
                                XPathNavigator xmlMetamagicNode =
                                    xmlMetamagicDoc.SelectSingleNode(
                                        "metamagics/metamagic[name = " + metamagic.Name.CleanXPath() + ']');
                                if (xmlMetamagicNode != null)
                                {
                                    if (xmlMetamagicNode.SelectSingleNode(
                                        "required/art[. = " + strNodeInnerText.CleanXPath() + ']') != null)
                                    {
                                        return new Tuple<bool, string>(true, strName);
                                    }

                                    if (xmlMetamagicNode.SelectSingleNode(
                                        "forbidden/art[. = " + strNodeInnerText.CleanXPath() + ']') != null)
                                    {
                                        return new Tuple<bool, string>(false, strName);
                                    }
                                }
                                else
                                {
                                    // We couldn't find a metamagic with this name, so it's probably an art. Try and find the node.
                                    // If we can't, it's probably a data entry error.
                                    xmlMetamagicNode =
                                        xmlMetamagicDoc.SelectSingleNode("arts/art[name = " + metamagic.Name.CleanXPath() + ']');
                                    if (xmlMetamagicNode == null)
                                        Utils.BreakIfDebug();
                                    else
                                        return new Tuple<bool, string>(true, strName);
                                }
                            }

                            return new Tuple<bool, string>(true, strName);
                        }

                        Art objArt = objCharacter.Arts.FirstOrDefault(x => x.Name == strNodeInnerText);
                        if (objArt != null)
                        {
                            if (blnShowMessage)
                                strName = objArt.CurrentDisplayName;
                            return new Tuple<bool, string>(true, strName);
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
                                return new Tuple<bool, string>(true, strName);
                            }
                        }

                        if (!blnShowMessage)
                            return new Tuple<bool, string>(false, strName);
                        string strTranslate = (await objCharacter.LoadDataXPathAsync("metamagic.xml"))
                            .SelectSingleNode("/chummer/arts/art[name = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                        strName = string.Format(GlobalOptions.CultureInfo, "{0}\t{2}{1}({3})",
                            Environment.NewLine, strSpace, !string.IsNullOrEmpty(strTranslate) ? strTranslate : strNodeInnerText, LanguageManager.GetString("String_Art"));
                        return new Tuple<bool, string>(false, strName);
                }
                case "magenabled":
                    {
                        // Character must be Awakened.
                        if (blnShowMessage)
                            strName = Environment.NewLine + '\t' +
                                      LanguageManager.GetString("String_AttributeMAGLong") +
                                      strSpace + '≥' + strSpace + 1.ToString(GlobalOptions.CultureInfo);
                        return new Tuple<bool, string>(objCharacter.MAGEnabled, strName);
                    }
                case "metatype":
                    {
                        if (blnShowMessage)
                        {
                            string strXPathFilter = "/chummer/metatypes/metatype[name = " + strNodeInnerText.CleanXPath() + "]/translate";
                            // Check the Metatype restriction.
                            string strTranslate = (await objCharacter.LoadDataXPathAsync("metatypes.xml")).SelectSingleNode(strXPathFilter)?.Value
                                                  ?? (await objCharacter.LoadDataXPathAsync("critters.xml")).SelectSingleNode(strXPathFilter)?.Value;
                            strName = string.Format(GlobalOptions.CultureInfo, "{0}\t{2}{1}({3})",
                                Environment.NewLine, strSpace, !string.IsNullOrEmpty(strTranslate) ? strTranslate : strNodeInnerText, LanguageManager.GetString("String_Metatype"));
                        }
                        return new Tuple<bool, string>(strNodeInnerText == objCharacter.Metatype, strName);
                    }
                case "metatypecategory":
                    {
                        if (blnShowMessage)
                        {
                            string strXPathFilter = "/chummer/categories/category[. = " + strNodeInnerText.CleanXPath() + "]/@translate";
                            // Check the Metatype Category restriction.
                            string strTranslate = (await objCharacter.LoadDataXPathAsync("metatypes.xml")).SelectSingleNode(strXPathFilter)?.Value
                                                  ?? (await objCharacter.LoadDataXPathAsync("critters.xml")).SelectSingleNode(strXPathFilter)?.Value;
                            strName = string.Format(GlobalOptions.CultureInfo, "{0}\t{2}{1}({3})",
                                Environment.NewLine, strSpace,!string.IsNullOrEmpty(strTranslate) ? strTranslate : strNodeInnerText, LanguageManager.GetString("String_MetatypeCategory"));
                        }
                        return new Tuple<bool, string>(strNodeInnerText == objCharacter.MetatypeCategory, strName);
                    }
                case "metavariant":
                    {
                        if (blnShowMessage)
                        {
                            string strXPathFilter = "/chummer/metatypes/metatype/metavariants/metavariant[name = " + strNodeInnerText.CleanXPath() + "]/translate";
                            // Check the Metavariant restriction.
                            string strTranslate = (await objCharacter.LoadDataXPathAsync("metatypes.xml")).SelectSingleNode(strXPathFilter)?.Value
                                                  ?? (await objCharacter.LoadDataXPathAsync("critters.xml")).SelectSingleNode(strXPathFilter)?.Value;
                            strName = string.Format(GlobalOptions.CultureInfo, "{0}\t{2}{1}({3})",
                                Environment.NewLine, strSpace,!string.IsNullOrEmpty(strTranslate) ? strTranslate : strNodeInnerText, LanguageManager.GetString("String_Metavariant"));
                        }
                        return new Tuple<bool, string>(strNodeInnerText == objCharacter.Metavariant, strName);
                    }
                case "nuyen":
                    {
                        // Character's nuyen must be higher than or equal to the required value.
                        if (blnShowMessage)
                            strName = Environment.NewLine + '\t' + LanguageManager.GetString("String_Nuyen") + strSpace + '≥' + strSpace + strNodeInnerText;
                        return new Tuple<bool, string>(objCharacter.Nuyen >= Convert.ToInt32(strNodeInnerText, GlobalOptions.InvariantCultureInfo), strName);
                    }
                case "onlyprioritygiven":
                    {
                        if (blnShowMessage)
                            strName = Environment.NewLine + '\t' + LanguageManager.GetString("Message_SelectGeneric_PriorityRestriction");
                        return new Tuple<bool, string>(objCharacter.EffectiveBuildMethodUsesPriorityTables, strName);
                    }
                case "power":
                    {
                        // Run through all of the Powers the character has and see if the current required item exists.
                        Power power = objCharacter.Powers.FirstOrDefault(p => p.Name == strNodeInnerText);
                        if (power != null)
                        {
                            if (blnShowMessage)
                                strName = power.CurrentDisplayName;
                            return new Tuple<bool, string>(true, strName);
                        }
                        if (blnShowMessage)
                        {
                            string strTranslate = (await objCharacter.LoadDataXPathAsync("powers.xml")).SelectSingleNode(
                                "/chummer/powers/power[name = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                            strName = string.Format(GlobalOptions.CultureInfo, "{0}\t{2}{1}({3})",
                                Environment.NewLine, strSpace, !string.IsNullOrEmpty(strTranslate) ? strTranslate : strNodeInnerText, LanguageManager.GetString("Tab_Adept"));
                        }
                        return new Tuple<bool, string>(false, strName);
                    }
                case "program":
                    {
                        // Character needs a specific Program.
                        if (!blnShowMessage)
                            return new Tuple<bool, string>(objCharacter.AIPrograms.Any(p => p.Name == strNodeInnerText), strName);
                        string strTranslate = (await objCharacter.LoadDataXPathAsync("programs.xml")).SelectSingleNode(
                            "/chummer/programs/program[name = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                        strName = string.Format(GlobalOptions.CultureInfo, "{0}\t{2}{1}({3})",
                            Environment.NewLine, strSpace, !string.IsNullOrEmpty(strTranslate) ? strTranslate : strNodeInnerText, LanguageManager.GetString("String_Program"));
                        return new Tuple<bool, string>(objCharacter.AIPrograms.Any(p => p.Name == strNodeInnerText), strName);
                    }
                case "quality":
                    {
                        string strExtra = xmlNode.SelectSingleNode("@extra")?.Value;
                        Quality quality = !string.IsNullOrEmpty(strExtra)
                            ? objCharacter.Qualities.FirstOrDefault(q => q.Name == strNodeInnerText && q.Extra == strExtra && q.Name != strIgnoreQuality)
                            : objCharacter.Qualities.FirstOrDefault(q => q.Name == strNodeInnerText && q.Name != strIgnoreQuality);
                        if (quality != null)
                        {
                            if (blnShowMessage)
                                strName = quality.CurrentDisplayName;
                            return new Tuple<bool, string>(true, strName);
                        }
                        if (!blnShowMessage)
                            return new Tuple<bool, string>(false, strName);
                        string strTranslate = (await objCharacter.LoadDataXPathAsync("qualities.xml")).SelectSingleNode(
                            "/chummer/qualities/quality[name = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                        strName = string.Format(GlobalOptions.CultureInfo, "{0}\t{2}{1}({3})",
                            Environment.NewLine, strSpace, !string.IsNullOrEmpty(strTranslate) ? strTranslate : strNodeInnerText, LanguageManager.GetString("String_Quality"));
                        return new Tuple<bool, string>(false, strName);
                    }
                case "resenabled":
                    // Character must be Emerged.
                    if (blnShowMessage)
                        strName = Environment.NewLine + '\t' + LanguageManager.GetString("String_AttributeRESLong") + strSpace + '≥' + strSpace + 1.ToString(GlobalOptions.CultureInfo);
                    return new Tuple<bool, string>(objCharacter.RESEnabled, strName);
                case "skill":
                    {
                        string strSpec = xmlNode.SelectSingleNode("spec")?.Value;
                        string strValue = xmlNode.SelectSingleNode("val")?.Value;
                        int intValue = Convert.ToInt32(strValue, GlobalOptions.InvariantCultureInfo);
                        // Check if the character has the required Skill.
                        if (xmlNode.SelectSingleNode("type") != null)
                        {
                            KnowledgeSkill objKnowledgeSkill = objCharacter.SkillsSection.KnowledgeSkills
                                .FirstOrDefault(objSkill => objSkill.Name == strNodeName &&
                                                   (string.IsNullOrEmpty(strSpec) ||
                                                    objSkill.Specializations.Any(objSpec => objSpec.Name == strSpec) &&
                                                    objSkill.TotalBaseRating >= intValue));

                            if (objKnowledgeSkill != null)
                            {
                                if (blnShowMessage)
                                {
                                    strName = objKnowledgeSkill.CurrentDisplayName;
                                    if (!string.IsNullOrEmpty(strSpec) && !objCharacter.Improvements.Any(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.DisableSpecializationEffects && objImprovement.ImprovedName == objKnowledgeSkill.Name && string.IsNullOrEmpty(objImprovement.Condition) && objImprovement.Enabled))
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
                            if (!string.IsNullOrEmpty(strNodeName))
                            {
                                Skill objSkill = objCharacter.SkillsSection.GetActiveSkill(strNodeName);
                                // Exotic Skill
                                if (objSkill == null && !string.IsNullOrEmpty(strSpec))
                                    objSkill = objCharacter.SkillsSection.GetActiveSkill(strNodeName + strSpace + '(' + strSpec + ')');
                                if (objSkill != null && (xmlNode.SelectSingleNode("spec") == null || objSkill.Specializations.Any(objSpec => objSpec.Name == strSpec)) && objSkill.TotalBaseRating >= intValue)
                                {
                                    if (blnShowMessage)
                                    {
                                        strName = objSkill.CurrentDisplayName;
                                        if (!string.IsNullOrEmpty(strSpec) && !objCharacter.Improvements.Any(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.DisableSpecializationEffects && objImprovement.ImprovedName == objSkill.Name && string.IsNullOrEmpty(objImprovement.Condition) && objImprovement.Enabled))
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
                            XPathNavigator xmlSkillDoc = await objCharacter.LoadDataXPathAsync("skills.xml");
                            string strSkillName = xmlNode.SelectSingleNode("name")?.Value;
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
                        return new Tuple<bool, string>(false, strName);
                    }
                case "skillgrouptotal":
                    {
                        // Check if the total combined Ratings of Skill Groups adds up to a particular total.
                        int intTotal = 0;
                        string[] strGroups = xmlNode.SelectSingleNode("skillgroups")?.Value.Split('+', StringSplitOptions.RemoveEmptyEntries);
                        StringBuilder objOutputString = new StringBuilder(Environment.NewLine + '\t');
                        if (strGroups != null)
                        {
                            for (int i = 0; i <= strGroups.Length - 1; ++i)
                            {
                                foreach (SkillGroup objGroup in objCharacter.SkillsSection.SkillGroups)
                                {
                                    if (objGroup.Name == strGroups[i])
                                    {
                                        if (blnShowMessage)
                                            objOutputString.Append(objGroup.CurrentDisplayName + ',' + strSpace);
                                        intTotal += objGroup.Rating;
                                        break;
                                    }
                                }
                            }
                        }

                        if (blnShowMessage)
                        {
                            if (objOutputString.Length > 0)
                                objOutputString.Length -= 2;
                            strName = objOutputString + strSpace + '(' + LanguageManager.GetString("String_ExpenseSkillGroup") + ')';
                        }
                        return new Tuple<bool, string>(intTotal >= Convert.ToInt32(xmlNode.SelectSingleNode("val")?.Value, GlobalOptions.InvariantCultureInfo), strName);
                    }
                case "specialmodificationlimit":
                {
                    // Add in the cost of all child components.
                    int intMods = objCharacter.Weapons.GetAllDescendants(x => x.UnderbarrelWeapons).AsParallel().Sum(x => x.WeaponAccessories.Count(y => y.SpecialModification));
                    intMods += objCharacter.Vehicles.AsParallel().Sum(objVehicle =>
                    {
                        IEnumerable<Weapon> lstWeapons = objVehicle.Weapons
                            .Concat(objVehicle.WeaponMounts.SelectMany(objMount => objMount.Weapons))
                            .GetAllDescendants(x => x.UnderbarrelWeapons);
                        return lstWeapons.AsParallel().Sum(x => x.WeaponAccessories.Count(y => y.SpecialModification));
                    });
                    if (blnShowMessage)
                    {
                        strName = string.Format(GlobalOptions.CultureInfo, "{0}\t{2}{1}≥{1}{3}",
                            Environment.NewLine, strSpace, LanguageManager.GetString("String_SpecialModificationLimit"), strNodeInnerText);
                    }

                    return new Tuple<bool, string>((intMods + Convert.ToInt32(strNodeInnerText, GlobalOptions.InvariantCultureInfo)) <= objCharacter.SpecialModificationLimit, strName);
                }
                case "spell":
                    {
                        Spell objSpell = objCharacter.Spells.FirstOrDefault(x => x.Name == strNodeInnerText);
                        if (objSpell != null)
                        {
                            if (blnShowMessage)
                                strName = objSpell.CurrentDisplayName;
                            return new Tuple<bool, string>(true, strName);
                        }
                        if (blnShowMessage)
                        {
                            // Check for a specific Spell.
                            string strTranslate = (await objCharacter.LoadDataXPathAsync("spells.xml")).SelectSingleNode("/chummer/spells/spell[name = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                            strName = string.Format(GlobalOptions.CultureInfo, "{0}\t{2}{1}({3})",
                                Environment.NewLine, strSpace, !string.IsNullOrEmpty(strTranslate) ? strTranslate : strNodeInnerText, LanguageManager.GetString("String_DescSpell"));
                        }
                        return new Tuple<bool, string>(false, strName);
                    }
                case "spellcategory":
                    {
                        // Check for a specified amount of a particular Spell category.
                        if (blnShowMessage)
                        {
                            string strTranslate = (await objCharacter.LoadDataXPathAsync("spells.xml")).SelectSingleNode("/chummer/categories/category[. = " + strNodeName.CleanXPath() + "]/@translate")?.Value;
                            strName = string.Format(GlobalOptions.CultureInfo, "{0}\t{2}{1}({3})",
                                Environment.NewLine, strSpace, !string.IsNullOrEmpty(strTranslate) ? strTranslate : strNodeInnerText, LanguageManager.GetString("String_SpellCategory"));
                        }
                        return new Tuple<bool, string>(objCharacter.Spells.Count(objSpell => objSpell.Category == strNodeName) >= Convert.ToInt32(xmlNode.SelectSingleNode("count")?.Value, GlobalOptions.InvariantCultureInfo), strName);
                    }
                case "spelldescriptor":
                    {
                        string strCount = xmlNode.SelectSingleNode("count")?.Value ?? string.Empty;
                        // Check for a specified amount of a particular Spell Descriptor.
                        if (blnShowMessage)
                            strName = Environment.NewLine + '\t' + LanguageManager.GetString("Label_Descriptors") + strSpace + '≥' + strSpace + strCount;
                        return new Tuple<bool, string>(objCharacter.Spells.Count(objSpell => objSpell.Descriptors.Contains(strNodeName)) >= Convert.ToInt32(strCount, GlobalOptions.InvariantCultureInfo), strName);
                    }
                case "streetcredvsnotoriety":
                    {
                        // Street Cred must be higher than Notoriety.
                        if (blnShowMessage)
                            strName = Environment.NewLine + '\t' + LanguageManager.GetString("String_StreetCred") + strSpace + '≥' + strSpace + LanguageManager.GetString("String_Notoriety");
                        return new Tuple<bool, string>(objCharacter.StreetCred >= objCharacter.Notoriety, strName);
                    }
                case "submersiongrade":
                    {
                        // Character's initiate grade must be higher than or equal to the required value.
                        if (blnShowMessage)
                            strName = Environment.NewLine + '\t' + LanguageManager.GetString("String_SubmersionGrade") + strSpace + '≥' + strSpace + strNodeInnerText;
                        return new Tuple<bool, string>(objCharacter.SubmersionGrade >= Convert.ToInt32(strNodeInnerText, GlobalOptions.InvariantCultureInfo), strName);
                    }
                case "tradition":
                    {
                        // Character needs a specific Tradition.
                        if (blnShowMessage)
                        {
                            string strTranslate = (await objCharacter.LoadDataXPathAsync("traditions.xml")).SelectSingleNode(
                                "/chummer/traditions/tradition[name = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                            strName = string.Format(GlobalOptions.CultureInfo, "{0}\t{2}{1}({3})",
                                Environment.NewLine, strSpace, !string.IsNullOrEmpty(strTranslate) ? strTranslate : strNodeInnerText, LanguageManager.GetString("String_Tradition"));
                        }
                        return new Tuple<bool, string>(objCharacter.MagicTradition.Name == strNodeInnerText, strName);
                    }
                case "traditionspiritform":
                {
                    // Character needs a specific spirit form provided by their Tradition.
                    if (blnShowMessage)
                    {
                        string strTranslate = (await objCharacter.LoadDataXPathAsync("critterpowers.xml")).SelectSingleNode(
                            "/chummer/powers/power[name = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                        strName = string.Format(GlobalOptions.CultureInfo, "{0}\t{2}{1}({3})",
                            Environment.NewLine, strSpace, !string.IsNullOrEmpty(strTranslate) ? strTranslate : strNodeInnerText, LanguageManager.GetString("String_Tradition"));
                    }
                    return new Tuple<bool, string>(objCharacter.MagicTradition.SpiritForm == strNodeInnerText, strName);
                }
                case "weapon":
                {
                    // Character needs a specific Weapon.
                    if (!blnShowMessage)
                        return new Tuple<bool, string>(objCharacter.Weapons.Any(w => w.Name == strNodeInnerText), strName);
                    string strTranslate = (await objCharacter.LoadDataXPathAsync("weapons.xml")).SelectSingleNode(
                        "/chummer/weapons/weapon[name = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                    strName = string.Format(GlobalOptions.CultureInfo, "{0}\t{2}{1}({3})",
                        Environment.NewLine, strSpace, !string.IsNullOrEmpty(strTranslate) ? strTranslate : strNodeInnerText, LanguageManager.GetString("String_Weapon"));
                    return new Tuple<bool, string>(objCharacter.Weapons.Any(w => w.Name == strNodeInnerText), strName);
                }
                case "accessory" when objParent is Weapon objWeapon:
                {
                    if (!blnShowMessage)
                        return new Tuple<bool, string>(objWeapon.WeaponAccessories.Any(objAccessory => objAccessory.Name == strNodeInnerText), strName);
                    string strTranslate = (await objCharacter.LoadDataXPathAsync("weapons.xml"))
                        .SelectSingleNode("/chummer/accessories/accessory[name = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                    strName = string.Format(GlobalOptions.CultureInfo, "{0}\t{2}{1}({3})",
                        Environment.NewLine, strSpace, !string.IsNullOrEmpty(strTranslate) ? strTranslate : strNodeInnerText, LanguageManager.GetString("String_WeaponAccessory"));
                    return new Tuple<bool, string>(objWeapon.WeaponAccessories.Any(objAccessory => objAccessory.Name == strNodeInnerText), strName);
                }
                case "armormod":
                {
                    if (blnShowMessage)
                    {
                        string strTranslate = (await objCharacter.LoadDataXPathAsync("armor.xml"))
                            .SelectSingleNode("/chummer/armormods/armormod[name = " + strNodeInnerText.CleanXPath() + "]/translate")?.Value;
                        strName = string.Format(GlobalOptions.CultureInfo, "{0}\t{2}{1}({3})",
                            Environment.NewLine, strSpace, !string.IsNullOrEmpty(strTranslate) ? strTranslate : strNodeInnerText, LanguageManager.GetString("String_ArmorMod"));
                    }

                    if (xmlNode.GetAttribute("sameparent", string.Empty) == bool.TrueString)
                    {
                        if (objParent is Armor objArmor)
                            return new Tuple<bool, string>(objArmor.ArmorMods.Any(mod => mod.Name == strNodeInnerText), strName);
                        return new Tuple<bool, string>(false, strName);
                    }
                    return new Tuple<bool, string>(objCharacter.Armor.Any(armor => armor.ArmorMods.Any(mod => mod.Name == strNodeInnerText)), strName);
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
            if (objCharacter == null || objCharacter.Created || objCharacter.RestrictedGear > 0 || objCharacter.IgnoreRules)
                return true;
            // Avail.

            XPathNavigator objAvailNode = objXmlGear.SelectSingleNode("avail");
            if (objAvailNode == null)
            {
                int intHighestAvailNode = 0;
                foreach (XPathNavigator objLoopNode in objXmlGear.SelectChildren(XPathNodeType.Element))
                {
                    if (objLoopNode.Name.StartsWith("avail", StringComparison.Ordinal))
                    {
                        string strLoopCostString = objLoopNode.Name.Substring(5);
                        if (int.TryParse(strLoopCostString, out int intTmp))
                        {
                            intHighestAvailNode = Math.Max(intHighestAvailNode, intTmp);
                        }
                    }
                }
                objAvailNode = objXmlGear.SelectSingleNode("avail" + intHighestAvailNode);
                for (int i = intRating; i <= intHighestAvailNode; ++i)
                {
                    XPathNavigator objLoopNode = objXmlGear.SelectSingleNode("avail" + i.ToString(GlobalOptions.InvariantCultureInfo));
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
            object objProcess = CommonFunctions.EvaluateInvariantXPath(strAvailExpr.Replace("Rating", intRating.ToString(GlobalOptions.InvariantCultureInfo)), out bool blnIsSuccess);
            if (blnIsSuccess)
                intAvail += ((double)objProcess).StandardRound();
            return intAvail <= objCharacter.Options.MaximumAvailability;
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
            XPathNavigator objCostNode = objXmlGear.SelectSingleNode("cost");
            if (objCostNode == null)
            {
                int intCostRating = 1;
                foreach (XmlNode objLoopNode in objXmlGear.SelectChildren(XPathNodeType.Element))
                {
                    if (objLoopNode.Name.StartsWith("cost", StringComparison.Ordinal))
                    {
                        string strLoopCostString = objLoopNode.Name.Substring(4);
                        if (int.TryParse(strLoopCostString, out int intTmp) && intTmp <= intRating)
                        {
                            intCostRating = Math.Max(intCostRating, intTmp);
                        }
                    }
                }

                objCostNode = objXmlGear.SelectSingleNode("cost" + intCostRating.ToString(GlobalOptions.InvariantCultureInfo));
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

                object objProcess = CommonFunctions.EvaluateInvariantXPath(strCost.Replace("Rating", intRating.ToString(GlobalOptions.InvariantCultureInfo)), out bool blnIsSuccess);
                if (blnIsSuccess)
                    decCost = Convert.ToDecimal(objProcess, GlobalOptions.InvariantCultureInfo);
            }
            return decMaxNuyen >= decCost * decCostMultiplier;
        }
    }
}
