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
using Chummer.Backend.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace Chummer
{
    public static class SelectionShared
    {
        //TODO: Might be a better location for this; Class names are screwy.
        /// <summary>Evaluates requirements of a given node against a given Character object.</summary>
        /// <param name="xmlNode">XmlNode of the object.</param>
        /// <param name="objCharacter">Character object against which to check.</param>
        /// <param name="strLocalName">Name of the type of item being checked for displaying messages. If empty or null, no message is displayed.</param>
        /// <param name="strIgnoreQuality">Name of a Quality that should be ignored. Typically used when swapping Qualities in career mode.</param>
        /// <param name="strSourceName">Name of the improvement that called this (if it was called by an improvement adding it)</param>
        /// <param name="strLocation">Limb side to use if we need a specific limb side (Left or Right)</param>
        /// <param name="blnIgnoreLimit">Whether to ignore checking for limits on the total amount of this item the character can have.</param>
        /// <returns></returns>
        public static bool RequirementsMet(this XmlNode xmlNode, Character objCharacter, string strLocalName = "", string strIgnoreQuality = "", string strSourceName = "", string strLocation = "", bool blnIgnoreLimit = false)
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
                if (xmlNode["chargenonly"] != null)
                {
                    if (blnShowMessage)
                    {
                        MessageBox.Show(LanguageManager.GetString("Message_SelectGeneric_ChargenRestriction", GlobalOptions.Language).Replace("{0}", strLocalName),
                            LanguageManager.GetString("MessageTitle_SelectGeneric_Restriction", GlobalOptions.Language).Replace("{0}", strLocalName),
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    return false;
                }
            }
            // See if the character is using priority-based gen and is trying to add a Quality that can only be added through priorities
            else
            {
                if (xmlNode["careeronly"] != null)
                {
                    if (blnShowMessage)
                    {
                        MessageBox.Show(LanguageManager.GetString("Message_SelectGeneric_CareerOnlyRestriction", GlobalOptions.Language).Replace("{0}", strLocalName),
                            LanguageManager.GetString("MessageTitle_SelectGeneric_Restriction", GlobalOptions.Language).Replace("{0}", strLocalName),
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    return false;
                }
                if (objCharacter.BuildMethod == CharacterBuildMethod.Priority || objCharacter.BuildMethod == CharacterBuildMethod.SumtoTen)
                {
                    if (xmlNode["onlyprioritygiven"] != null)
                    {
                        if (blnShowMessage)
                        {
                            MessageBox.Show(LanguageManager.GetString("MessageTitle_SelectGeneric_PriorityRestriction", GlobalOptions.Language).Replace("{0}", strLocalName),
                                LanguageManager.GetString("MessageTitle_SelectGeneric_Restriction", GlobalOptions.Language).Replace("{0}", strLocalName),
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
                string strLimitString = xmlNode["chargenlimit"]?.InnerText;
                if (string.IsNullOrWhiteSpace(strLimitString) || objCharacter.Created)
                {
                    strLimitString = xmlNode["limit"]?.InnerText;
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
                    foreach (string strAttribute in AttributeSection.AttributeStrings)
                    {
                        CharacterAttrib objLoopAttribute = objCharacter.GetAttribute(strAttribute);
                        objLimitString.CheapReplace(strLimitString, "{" + strAttribute + "}", () => objLoopAttribute.TotalValue.ToString());
                        objLimitString.CheapReplace(strLimitString, "{" + strAttribute + "Base}", () => objLoopAttribute.TotalBase.ToString());
                    }
                    foreach (string strLimb in Character.LimbStrings)
                    {
                        objLimitString.CheapReplace(strLimitString, "{" + strLimb + "}", () => (string.IsNullOrEmpty(strLocation) ? objCharacter.LimbCount(strLimb) : objCharacter.LimbCount(strLimb) / 2).ToString());
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
                                List<MartialArtTechnique> objTempList = new List<MartialArtTechnique>(objCharacter.MartialArts.Count);
                                foreach (MartialArt objMartialArt in objCharacter.MartialArts)
                                {
                                    objTempList.AddRange(objMartialArt.Techniques);
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

                    int intLimit = Convert.ToInt32(strLimitString);
                    int intExtendedLimit = intLimit;
                    string strLimitWithInclusions = xmlNode["limitwithinclusions"]?.InnerText;
                    if (!string.IsNullOrEmpty(strLimitWithInclusions))
                    {
                        intExtendedLimit = Convert.ToInt32(strLimitWithInclusions);
                    }
                    int intCount = 0;
                    int intExtendedCount = 0;
                    if (objListToCheck != null || blnCheckCyberwareChildren)
                    {
                        var lstToCheck = objListToCheck?.ToList() ?? new List<IHasName>();
                        string strNameNode = xmlNode["name"]?.InnerText;
                        if (blnCheckCyberwareChildren)
                        {
                            intCount = string.IsNullOrEmpty(strLocation)
                                ? objCharacter.Cyberware.DeepCount(x => x.Children, x => string.IsNullOrEmpty(x.PlugsIntoModularMount) && strNameNode == x.Name)
                                : objCharacter.Cyberware.DeepCount(x => x.Children, x => string.IsNullOrEmpty(x.PlugsIntoModularMount) && x.Location == strLocation && strNameNode == x.Name);
                        }
                        else
                            intCount = lstToCheck.Count(objItem => strNameNode == objItem.Name);
                        intExtendedCount = intCount;
                        // In case one item is split up into multiple entries with different names, e.g. Indomitable quality, we need to be able to check all those entries against the limit
                        XmlNode xmlIncludeInLimit = xmlNode["includeinlimit"];
                        if (xmlIncludeInLimit != null)
                        {
                            List<string> lstNamesIncludedInLimit = new List<string>();
                            if (!string.IsNullOrEmpty(strNameNode))
                            {
                                lstNamesIncludedInLimit.Add(strNameNode);
                            }
                            foreach (XmlNode objChildXml in xmlIncludeInLimit.ChildNodes)
                            {
                                lstNamesIncludedInLimit.Add(objChildXml.InnerText);
                            }

                            if (blnCheckCyberwareChildren)
                            {
                                intExtendedCount = string.IsNullOrEmpty(strLocation)
                                    ? objCharacter.Cyberware.DeepCount(x => x.Children, x => string.IsNullOrEmpty(x.PlugsIntoModularMount) && lstNamesIncludedInLimit.Any(objLimitName => objLimitName == x.Name))
                                    : objCharacter.Cyberware.DeepCount(x => x.Children, x => string.IsNullOrEmpty(x.PlugsIntoModularMount) && x.Location == strLocation && lstNamesIncludedInLimit.Any(strName => strName == x.Name));
                            }
                            else
                                intExtendedCount = lstToCheck.Count(objItem => lstNamesIncludedInLimit.Any(objLimitName => objLimitName == objItem.Name));
                        }
                    }
                    if (intCount >= intLimit || intExtendedCount >= intExtendedLimit)
                    {
                        if (blnShowMessage)
                        {
                            string limitTitle = LanguageManager.GetString("MessageTitle_SelectGeneric_Limit", GlobalOptions.Language).Replace("{0}", strLocalName);
                            string limitMessage = LanguageManager.GetString("Message_SelectGeneric_Limit", GlobalOptions.Language).Replace("{0}", strLocalName).Replace("{1}", intLimit == 0 ? "1" : intLimit.ToString());
                            MessageBox.Show(limitMessage, limitTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        return false;
                    }
                }
            }

            XmlNode xmlForbiddenNode = xmlNode["forbidden"];
            if (xmlForbiddenNode != null)
            {
                // Loop through the oneof requirements.
                foreach (XmlNode objXmlOneOf in xmlForbiddenNode.SelectNodes("oneof"))
                {
                    foreach (XmlNode xmlForbiddenItemNode in objXmlOneOf.ChildNodes)
                    {
                        // The character is not allowed to take the Quality, so display a message and uncheck the item.
                        if (xmlForbiddenItemNode.TestNodeRequirements(objCharacter, out string strName, strIgnoreQuality, blnShowMessage))
                        {
                            if (blnShowMessage)
                            {
                                string forbiddenTitle = LanguageManager.GetString("MessageTitle_SelectGeneric_Restriction", GlobalOptions.Language).Replace("{0}", strLocalName);
                                string forbiddenMessage = LanguageManager.GetString("Message_SelectGeneric_Restriction", GlobalOptions.Language).Replace("{0}", strLocalName) + strName;
                                MessageBox.Show(forbiddenMessage, forbiddenTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            return false;
                        }
                    }
                }
            }

            XmlNode xmlRequiredNode = xmlNode["required"];
            if (xmlRequiredNode != null)
            {
                StringBuilder objRequirement = new StringBuilder();
                bool blnRequirementMet = true;

                // Loop through the oneof requirements.
                foreach (XmlNode objXmlOneOf in xmlRequiredNode.SelectNodes("oneof"))
                {
                    bool blnOneOfMet = false;
                    StringBuilder objThisRequirement = new StringBuilder(Environment.NewLine + LanguageManager.GetString("Message_SelectQuality_OneOf", GlobalOptions.Language));
                    XmlNodeList objXmlOneOfList = objXmlOneOf.ChildNodes;
                    foreach (XmlNode xmlRequiredItemNode in objXmlOneOfList)
                    {
                        if (xmlRequiredItemNode.TestNodeRequirements(objCharacter, out string strName, strIgnoreQuality, blnShowMessage))
                        {
                            blnOneOfMet = true;
                            break;
                        }
                        if (blnShowMessage)
                            objThisRequirement.Append(strName);
                    }

                    // Update the flag for requirements met.
                    if (!blnOneOfMet)
                        blnRequirementMet = false;
                    if (blnShowMessage)
                        objRequirement.Append(objThisRequirement);
                    else if (!blnRequirementMet)
                        break;
                }

                if (blnRequirementMet || blnShowMessage)
                {
                    // Loop through the allof requirements.
                    foreach (XmlNode objXmlAllOf in xmlRequiredNode.SelectNodes("allof"))
                    {
                        bool blnAllOfMet = true;
                        StringBuilder objThisRequirement = new StringBuilder(Environment.NewLine + LanguageManager.GetString("Message_SelectQuality_AllOf", GlobalOptions.Language));
                        XmlNodeList objXmlAllOfList = objXmlAllOf.ChildNodes;
                        foreach (XmlNode xmlRequiredItemNode in objXmlAllOfList)
                        {
                            // If this item was not found, fail the AllOfMet condition.
                            if (!xmlRequiredItemNode.TestNodeRequirements(objCharacter, out string strName, strIgnoreQuality, blnShowMessage))
                            {
                                blnAllOfMet = false;
                                if (blnShowMessage)
                                    objThisRequirement.Append(strName);
                                else
                                    break;
                            }
                        }

                        // Update the flag for requirements met.
                        if (!blnAllOfMet)
                            blnRequirementMet = false;
                        if (blnShowMessage)
                            objRequirement.Append(objThisRequirement);
                        else if (!blnRequirementMet)
                            break;
                    }
                }

                // The character has not met the requirements, so display a message and uncheck the item.
                if (!blnRequirementMet)
                {
                    if (blnShowMessage)
                    {
                        string requiredTitle = LanguageManager.GetString("MessageTitle_SelectGeneric_Requirement", GlobalOptions.Language).Replace("{0}", strLocalName);
                        string requiredMessage = LanguageManager.GetString("Message_SelectGeneric_Requirement", GlobalOptions.Language).Replace("{0}", strLocalName) + objRequirement;
                        MessageBox.Show(requiredMessage, requiredTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    return false;
                }
            }
            return true;
        }

        public static bool TestNodeRequirements(this XmlNode xmlNode, Character objCharacter, out string strName, string strIgnoreQuality = "", bool blnShowMessage = true)
        {
            strName = string.Empty;
            if (xmlNode == null || objCharacter == null)
            {
                return false;
            }
            string strNodeInnerText = xmlNode.InnerText;
            string strNodeName = xmlNode["name"]?.InnerText ?? string.Empty;
            switch (xmlNode.Name)
            {
                case "attribute":
                    {
                        // Check to see if an Attribute meets a requirement.
                        CharacterAttrib objAttribute = objCharacter.GetAttribute(strNodeName);
                        int intTargetValue = Convert.ToInt32(xmlNode["total"]?.InnerText);
                        if (blnShowMessage)
                            strName = $"{Environment.NewLine}\t{objAttribute.DisplayAbbrev} {intTargetValue}";
                        // Special cases for when we want to check if a special attribute is enabled
                        if (intTargetValue == 1)
                        {
                            if (objAttribute.Abbrev == "MAG")
                                return objCharacter.MAGEnabled;
                            if (objAttribute.Abbrev == "MAGAdept")
                                return objCharacter.MAGEnabled && objCharacter.IsMysticAdept;
                            if (objAttribute.Abbrev == "RES")
                                return objCharacter.RESEnabled;
                            if (objAttribute.Abbrev == "DEP")
                                return objCharacter.DEPEnabled;
                        }
                        return objAttribute.TotalValue >= intTargetValue;
                    }
                case "attributetotal":
                    {
                        string strNodeAttributes = xmlNode["attributes"]?.InnerText ?? string.Empty;
                        int intNodeVal = Convert.ToInt32(xmlNode["val"]?.InnerText);
                        // Check if the character's Attributes add up to a particular total.
                        string strAttributes = strNodeAttributes;
                        string strValue = strNodeAttributes;
                        foreach (string strAttribute in AttributeSection.AttributeStrings)
                        {
                            CharacterAttrib objLoopAttrib = objCharacter.GetAttribute(strAttribute);
                            if (strNodeAttributes.Contains(objLoopAttrib.Abbrev))
                            {
                                strAttributes = strAttributes.Replace(strAttribute, objLoopAttrib.DisplayAbbrev);
                                strValue = strValue.Replace(strAttribute, objLoopAttrib.Value.ToString());
                            }
                        }
                        if (blnShowMessage)
                            strName = $"{Environment.NewLine}\t{strAttributes} {intNodeVal}";
                        object objProcess = CommonFunctions.EvaluateInvariantXPath(strValue, out bool blnIsSuccess);
                        return (blnIsSuccess ? Convert.ToInt32(objProcess) : 0) >= intNodeVal;
                    }
                case "careerkarma":
                    {
                        // Check Career Karma requirement.
                        if (blnShowMessage)
                            strName = Environment.NewLine + '\t' + LanguageManager.GetString("Message_SelectQuality_RequireKarma", GlobalOptions.Language).Replace("{0}", strNodeInnerText);
                        return objCharacter.CareerKarma >= Convert.ToInt32(strNodeInnerText);
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
                                    strName = critterPower.DisplayNameShort(GlobalOptions.Language);
                                return true;
                            }
                        }
                        if (blnShowMessage)
                        {
                            string strTranslate = XmlManager.Load("critterpowers.xml").SelectSingleNode($"/chummer/powers/power[name = {strNodeInnerText.CleanXPath()}]/translate")?.InnerText;
                            strName = !string.IsNullOrEmpty(strTranslate)
                                ? $"{Environment.NewLine}\t{strTranslate} ({LanguageManager.GetString("Tab_Critter", GlobalOptions.Language)})"
                                : $"{Environment.NewLine}\t{strNodeInnerText} ({LanguageManager.GetString("Tab_Critter", GlobalOptions.Language)})";
                        }
                        return false;
                    }
                case "bioware":
                    {
                        int count = Convert.ToInt32(xmlNode.Attributes?["count"]?.InnerText ?? "1");
                        if (blnShowMessage)
                        {
                            string strTranslate = XmlManager.Load("bioware.xml").SelectSingleNode($"/chummer/biowares/bioware[name = {strNodeInnerText.CleanXPath()}]/translate")?.InnerText;
                            strName = !string.IsNullOrEmpty(strTranslate)
                                ? $"{Environment.NewLine}\t{LanguageManager.GetString("Label_Bioware", GlobalOptions.Language)} {strTranslate}"
                                : $"{Environment.NewLine}\t{LanguageManager.GetString("Label_Bioware", GlobalOptions.Language)} {strNodeInnerText}";
                        }
                        string strWareNodeSelectAttribute = xmlNode.Attributes?["select"]?.InnerText ?? string.Empty;
                        return objCharacter.Cyberware.DeepCount(x => x.Children, objCyberware => objCyberware.Name == strNodeInnerText &&
                                objCyberware.SourceType == Improvement.ImprovementSource.Bioware && string.IsNullOrEmpty(objCyberware.PlugsIntoModularMount) &&
                               (string.IsNullOrEmpty(strWareNodeSelectAttribute) || strWareNodeSelectAttribute == objCyberware.Extra)) >= count;
                    }
                case "cyberware":
                    {
                        int count = Convert.ToInt32(xmlNode.Attributes?["count"]?.InnerText ?? "1");
                        if (blnShowMessage)
                        {
                            string strTranslate = XmlManager.Load("cyberware.xml").SelectSingleNode($"/chummer/cyberwares/cyberware[name = {strNodeInnerText.CleanXPath()}]/translate")?.InnerText;
                            strName = !string.IsNullOrEmpty(strTranslate)
                                ? $"{Environment.NewLine}\t{LanguageManager.GetString("Label_Cyberware", GlobalOptions.Language)} {strTranslate}"
                                : $"{Environment.NewLine}\t{LanguageManager.GetString("Label_Cyberware", GlobalOptions.Language)} {strNodeInnerText}";
                        }
                        string strWareNodeSelectAttribute = xmlNode.Attributes?["select"]?.InnerText ?? string.Empty;
                        return objCharacter.Cyberware.DeepCount(x => x.Children, objCyberware => objCyberware.Name == strNodeInnerText &&
                                objCyberware.SourceType == Improvement.ImprovementSource.Cyberware && string.IsNullOrEmpty(objCyberware.PlugsIntoModularMount) &&
                               (string.IsNullOrEmpty(strWareNodeSelectAttribute) || strWareNodeSelectAttribute == objCyberware.Extra)) >= count;
                    }
                case "biowarecontains":
                    {
                        int count = Convert.ToInt32(xmlNode.Attributes?["count"]?.InnerText ?? "1");
                        if (blnShowMessage)
                        {
                            string strTranslate = XmlManager.Load("bioware.xml").SelectSingleNode($"/chummer/biowares/bioware[name = {strNodeInnerText.CleanXPath()}]/translate")?.InnerText;
                            strName = !string.IsNullOrEmpty(strTranslate)
                                ? $"{Environment.NewLine}\t{LanguageManager.GetString("Label_Bioware", GlobalOptions.Language)} {strTranslate}"
                                : $"{Environment.NewLine}\t{LanguageManager.GetString("Label_Bioware", GlobalOptions.Language)} {strNodeInnerText}";
                        }
                        string strWareNodeSelectAttribute = xmlNode.Attributes?["select"]?.InnerText ?? string.Empty;
                        return objCharacter.Cyberware.DeepCount(x => x.Children, objCyberware => objCyberware.Name.Contains(strNodeInnerText) &&
                                objCyberware.SourceType == Improvement.ImprovementSource.Bioware && string.IsNullOrEmpty(objCyberware.PlugsIntoModularMount) &&
                               (string.IsNullOrEmpty(strWareNodeSelectAttribute) || strWareNodeSelectAttribute == objCyberware.Extra)) >= count;
                    }
                case "cyberwarecontains":
                    {
                        int count = Convert.ToInt32(xmlNode.Attributes?["count"]?.InnerText ?? "1");
                        if (blnShowMessage)
                        {
                            string strTranslate = XmlManager.Load("cyberware.xml").SelectSingleNode($"/chummer/cyberwares/cyberware[name = {strNodeInnerText.CleanXPath()}]/translate")?.InnerText;
                            strName = !string.IsNullOrEmpty(strTranslate)
                                ? $"{Environment.NewLine}\t{LanguageManager.GetString("Label_Cyberware", GlobalOptions.Language)} {strTranslate}"
                                : $"{Environment.NewLine}\t{LanguageManager.GetString("Label_Cyberware", GlobalOptions.Language)} {strNodeInnerText}";
                        }
                        string strWareNodeSelectAttribute = xmlNode.Attributes?["select"]?.InnerText ?? string.Empty;
                        return objCharacter.Cyberware.DeepCount(x => x.Children, objCyberware => objCyberware.Name.Contains(strNodeInnerText) &&
                                objCyberware.SourceType == Improvement.ImprovementSource.Cyberware && string.IsNullOrEmpty(objCyberware.PlugsIntoModularMount) &&
                               (string.IsNullOrEmpty(strWareNodeSelectAttribute) || strWareNodeSelectAttribute == objCyberware.Extra)) >= count;
                    }
                case "damageresistance":
                    {
                        // Damage Resistance must be a particular value.
                        if (blnShowMessage)
                            strName = $"{Environment.NewLine}\t{LanguageManager.GetString("String_DamageResistance", GlobalOptions.Language)}";
                        return objCharacter.BOD.TotalValue + ImprovementManager.ValueOf(objCharacter, Improvement.ImprovementType.DamageResistance) >= Convert.ToInt32(strNodeInnerText);
                    }
                case "ess":
                    {
                        string strEssNodeGradeAttributeText = xmlNode.Attributes?["grade"]?.InnerText ?? string.Empty;
                        if (!string.IsNullOrEmpty(strEssNodeGradeAttributeText))
                        {
                            HashSet<string> setEssNodeGradeAttributeText = new HashSet<string>(strEssNodeGradeAttributeText.Split(','));
                            decimal decGrade =
                                objCharacter.Cyberware.Where(
                                        objCyberware =>
                                            setEssNodeGradeAttributeText.Any(func => objCyberware.Grade.Name.Contains(func)))
                                    .AsParallel().Sum(objCyberware => objCyberware.CalculatedESS());
                            if (strNodeInnerText.StartsWith('-'))
                            {
                                // Essence must be less than the value.
                                if (blnShowMessage)
                                    strName = Environment.NewLine + '\t' +
                                       LanguageManager.GetString("Message_SelectQuality_RequireESSGradeBelow", GlobalOptions.Language)
                                           .Replace("{0}", strNodeInnerText)
                                           .Replace("{1}", strEssNodeGradeAttributeText)
                                           .Replace("{2}", decGrade.ToString(GlobalOptions.InvariantCultureInfo));
                                return decGrade < Convert.ToDecimal(strNodeInnerText.TrimStart('-'), GlobalOptions.InvariantCultureInfo);
                            }
                            // Essence must be equal to or greater than the value.
                            if (blnShowMessage)
                                strName = Environment.NewLine + '\t' +
                                   LanguageManager.GetString("Message_SelectQuality_RequireESSGradeAbove", GlobalOptions.Language)
                                       .Replace("{0}", strNodeInnerText)
                                       .Replace("{1}", strEssNodeGradeAttributeText)
                                       .Replace("{2}", decGrade.ToString(GlobalOptions.InvariantCultureInfo));
                            return decGrade >= Convert.ToDecimal(strNodeInnerText, GlobalOptions.InvariantCultureInfo);
                        }
                        // Check Essence requirement.
                        if (strNodeInnerText.StartsWith('-'))
                        {
                            // Essence must be less than the value.
                            if (blnShowMessage)
                                strName = Environment.NewLine + '\t' +
                                   LanguageManager.GetString("Message_SelectQuality_RequireESSBelow", GlobalOptions.Language)
                                       .Replace("{0}", strNodeInnerText)
                                       .Replace("{1}", objCharacter.Essence().ToString(GlobalOptions.InvariantCultureInfo));
                            return objCharacter.Essence() < Convert.ToDecimal(strNodeInnerText.TrimStart('-'), GlobalOptions.InvariantCultureInfo);
                        }
                        // Essence must be equal to or greater than the value.
                        if (blnShowMessage)
                            strName = Environment.NewLine + '\t' +
                               LanguageManager.GetString("Message_SelectQuality_RequireESSAbove", GlobalOptions.Language)
                                   .Replace("{0}", strNodeInnerText)
                                   .Replace("{1}", objCharacter.Essence().ToString(GlobalOptions.InvariantCultureInfo));
                        return objCharacter.Essence() >= Convert.ToDecimal(strNodeInnerText, GlobalOptions.InvariantCultureInfo);
                    }
                case "echo":
                    {
                        Metamagic objMetamagic = objCharacter.Metamagics.FirstOrDefault(x => x.Name == strNodeInnerText && x.SourceType == Improvement.ImprovementSource.Echo);
                        if (objMetamagic != null)
                        {
                            if (blnShowMessage)
                                strName = objMetamagic.DisplayNameShort(GlobalOptions.Language);
                            return true;
                        }
                        if (blnShowMessage)
                        {
                            string strTranslate = XmlManager.Load("echoes.xml").SelectSingleNode($"/chummer/echoes/echo[name = {strNodeInnerText.CleanXPath()}]/translate")?.InnerText;
                            strName = !string.IsNullOrEmpty(strTranslate)
                                ? $"{Environment.NewLine}\t{strTranslate} ({LanguageManager.GetString("String_Echo", GlobalOptions.Language)})"
                                : $"{Environment.NewLine}\t{strNodeInnerText} ({LanguageManager.GetString("String_Echo", GlobalOptions.Language)})";
                        }
                        return false;
                    }
                case "group":
                    {
                        // Check that clustered options are present (Magical Tradition + Skill 6, for example)
                        bool blnResult = true;
                        string strResultName = string.Empty;
                        foreach (XmlNode xmlChildNode in xmlNode.ChildNodes)
                        {
                            blnResult = xmlChildNode.TestNodeRequirements(objCharacter, out strResultName, strIgnoreQuality, blnShowMessage);
                            if (!blnResult)
                            {
                                break;
                            }
                        }
                        if (blnShowMessage)
                            strName = strResultName;
                        return blnResult;
                    }
                case "grouponeof":
                {
                    // Check that one of the clustered options are present
                    bool blnResult = false;
                    string strResultName = LanguageManager.GetString("Message_SelectQuality_OneOf", GlobalOptions.Language);
                    foreach (XmlNode xmlChildNode in xmlNode.ChildNodes)
                    {
                        blnResult = xmlChildNode.TestNodeRequirements(objCharacter, out string strLoopResult, strIgnoreQuality, blnShowMessage);
                        if (blnResult)
                        {
                            break;
                        }

                        strResultName += strLoopResult;
                    }
                    if (blnShowMessage)
                        strName = strResultName;
                    return blnResult;
                }
                case "initiategrade":
                    {
                        // Character's initiate grade must be higher than or equal to the required value.
                        if (blnShowMessage)
                            strName = Environment.NewLine + '\t' + LanguageManager.GetString("String_InitiateGrade", GlobalOptions.Language) + " >= " + strNodeInnerText;
                        return objCharacter.InitiateGrade >= Convert.ToInt32(strNodeInnerText);
                    }
                case "martialart":
                    {
                        MartialArt objMartialArt = objCharacter.MartialArts.FirstOrDefault(x => x.Name == strNodeInnerText);
                        if (objMartialArt != null)
                        {
                            if (blnShowMessage)
                                strName = objMartialArt.DisplayNameShort(GlobalOptions.Language);
                            return true;
                        }
                        if (blnShowMessage)
                        {
                            // Character needs a specific Martial Art.
                            string strTranslate = XmlManager.Load("martialarts.xml").SelectSingleNode($"/chummer/martialarts/martialart[name = {strNodeInnerText.CleanXPath()}]/translate")?.InnerText;
                            strName = !string.IsNullOrEmpty(strTranslate)
                                ? $"{Environment.NewLine}\t{strTranslate} ({LanguageManager.GetString("String_MartialArt", GlobalOptions.Language)})"
                                : $"{Environment.NewLine}\t{strNodeInnerText} ({LanguageManager.GetString("String_MartialArt", GlobalOptions.Language)})";
                        }
                        return false;
                    }
                case "martialtechnique":
                    {
                        foreach (MartialArt objMartialArt in objCharacter.MartialArts)
                        {
                            MartialArtTechnique objMartialArtTechnique = objMartialArt.Techniques.FirstOrDefault(x => x.Name == strNodeInnerText);
                            if (objMartialArtTechnique != null)
                            {
                                if (blnShowMessage)
                                    strName = objMartialArtTechnique.DisplayName(GlobalOptions.Language);
                                return true;
                            }
                        }
                        if (blnShowMessage)
                        {
                            // Character needs a specific Martial Arts technique.
                            string strTranslate = XmlManager.Load("martialarts.xml").SelectSingleNode($"/chummer/techniques/technique[name = {strNodeInnerText.CleanXPath()}]/translate")?.InnerText;
                            strName = !string.IsNullOrEmpty(strTranslate)
                                ? $"{Environment.NewLine}\t{strTranslate} ({LanguageManager.GetString("String_MartialArt", GlobalOptions.Language)})"
                                : $"{Environment.NewLine}\t{strNodeInnerText} ({LanguageManager.GetString("String_MartialArt", GlobalOptions.Language)})";
                        }
                        return false;
                    }
                case "metamagic":
                    {
                        Metamagic objMetamagic = objCharacter.Metamagics.FirstOrDefault(x => x.Name == strNodeInnerText && x.SourceType == Improvement.ImprovementSource.Metamagic);
                        if (objMetamagic != null)
                        {
                            if (blnShowMessage)
                                strName = objMetamagic.DisplayNameShort(GlobalOptions.Language);
                            return true;
                        }
                        if (blnShowMessage)
                        {
                            string strTranslate = XmlManager.Load("metamagic.xml").SelectSingleNode($"/chummer/metamagics/metamagic[name = {strNodeInnerText.CleanXPath()}]/translate")?.InnerText;
                            strName = !string.IsNullOrEmpty(strTranslate)
                                ? $"{Environment.NewLine}\t{strTranslate} ({LanguageManager.GetString("String_Metamagic", GlobalOptions.Language)})"
                                : $"{Environment.NewLine}\t{strNodeInnerText} ({LanguageManager.GetString("String_Metamagic", GlobalOptions.Language)})";
                        }
                        return false;
                    }
                case "metamagicart":
                case "art":
                    {
                        //If we're either ignoring Art Requirements or Street Grimoire isn't enabled, perform the normal checks.
                        if (objCharacter.Options.IgnoreArt || !objCharacter.Options.BookEnabled("SG"))
                        {
                            XmlDocument xmlMetamagicDoc = XmlManager.Load("metamagic.xml");
                            if (blnShowMessage)
                            {
                                string strTranslateArt = xmlMetamagicDoc.SelectSingleNode($"/chummer/arts/art[name = {strNodeInnerText.CleanXPath()}]/translate")?.InnerText;
                                strName = !string.IsNullOrEmpty(strTranslateArt)
                                    ? $"{Environment.NewLine}\t{strTranslateArt} ({LanguageManager.GetString("String_Art", GlobalOptions.Language)})"
                                    : $"{Environment.NewLine}\t{strNodeInnerText} ({LanguageManager.GetString("String_Art", GlobalOptions.Language)})";
                            }
                            if (xmlNode.Name == "art")
                            {
                                return true;
                            }
                            foreach (Metamagic metamagic in objCharacter.Metamagics)
                            {
                                XmlNode xmlMetamagicNode = xmlMetamagicDoc.SelectSingleNode($"/chummer/metamagics/metamagic[name = {metamagic.Name.CleanXPath()}]");
                                if (xmlMetamagicNode != null)
                                {
                                    if (xmlMetamagicNode.SelectSingleNode($"required/art[text() = {strNodeInnerText.CleanXPath()}]") != null)
                                    {
                                        return true;
                                    }
                                    if (xmlMetamagicNode.SelectSingleNode($"forbidden/art[text() = {strNodeInnerText.CleanXPath()}]") != null)
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            Art objArt = objCharacter.Arts.FirstOrDefault(x => x.Name == strNodeInnerText);
                            if (objArt != null)
                            {
                                if (blnShowMessage)
                                    strName = objArt.DisplayNameShort(GlobalOptions.Language);
                                return true;
                            }
                        }
                        if (blnShowMessage)
                        {
                            string strTranslate = XmlManager.Load("metamagic.xml").SelectSingleNode($"/chummer/arts/art[name = {strNodeInnerText.CleanXPath()}]/translate")?.InnerText;
                            strName = !string.IsNullOrEmpty(strTranslate)
                                ? $"{Environment.NewLine}\t{strTranslate} ({LanguageManager.GetString("String_Art", GlobalOptions.Language)})"
                                : $"{Environment.NewLine}\t{strNodeInnerText} ({LanguageManager.GetString("String_Art", GlobalOptions.Language)})";
                        }
                        return false;
                    }
                case "metatype":
                    {
                        if (blnShowMessage)
                        {
                            string strXPathFilter = $"/chummer/metatypes/metatype[name = {strNodeInnerText.CleanXPath()}]/translate";
                            // Check the Metatype restriction.
                            string strTranslate = XmlManager.Load("metatypes.xml").SelectSingleNode(strXPathFilter)?.InnerText ??
                                                    XmlManager.Load("critters.xml").SelectSingleNode(strXPathFilter)?.InnerText;
                            strName = !string.IsNullOrEmpty(strTranslate)
                                ? $"{Environment.NewLine}\t{strTranslate} ({LanguageManager.GetString("String_Metatype", GlobalOptions.Language)})"
                                : $"{Environment.NewLine}\t{strNodeInnerText} ({LanguageManager.GetString("String_Metatype", GlobalOptions.Language)})";
                        }
                        return strNodeInnerText == objCharacter.Metatype;
                    }
                case "metatypecategory":
                    {
                        if (blnShowMessage)
                        {
                            string strXPathFilter = $"/chummer/categories/category[text() = {strNodeInnerText.CleanXPath()}]/@translate";
                            // Check the Metatype Category restriction.
                            string strTranslate = XmlManager.Load("metatypes.xml").SelectSingleNode(strXPathFilter)?.InnerText ??
                                                    XmlManager.Load("critters.xml").SelectSingleNode(strXPathFilter)?.InnerText;
                            strName = !string.IsNullOrEmpty(strTranslate)
                                ? $"{Environment.NewLine}\t{strTranslate} ({LanguageManager.GetString("String_MetatypeCategory", GlobalOptions.Language)})"
                                : $"{Environment.NewLine}\t{strNodeInnerText} ({LanguageManager.GetString("String_MetatypeCategory", GlobalOptions.Language)})";
                        }
                        return strNodeInnerText == objCharacter.MetatypeCategory;
                    }
                case "metavariant":
                    {
                        if (blnShowMessage)
                        {
                            string strXPathFilter = $"/chummer/metatypes/metatype/metavariants/metavariant[name = {strNodeInnerText.CleanXPath()}]/translate";
                            // Check the Metavariant restriction.
                            string strTranslate = XmlManager.Load("metatypes.xml").SelectSingleNode(strXPathFilter)?.InnerText ??
                                                    XmlManager.Load("critters.xml").SelectSingleNode(strXPathFilter)?.InnerText;
                            strName = !string.IsNullOrEmpty(strTranslate)
                                ? $"{Environment.NewLine}\t{strTranslate} ({LanguageManager.GetString("String_Metavariant", GlobalOptions.Language)})"
                                : $"{Environment.NewLine}\t{strNodeInnerText} ({LanguageManager.GetString("String_Metavariant", GlobalOptions.Language)})";
                        }
                        return strNodeInnerText == objCharacter.Metavariant;
                    }
                case "power":
                    {
                        // Run through all of the Powers the character has and see if the current required item exists.
                        Power power = objCharacter.Powers.FirstOrDefault(p => p.Name == strNodeInnerText);
                        if (power != null)
                        {
                            if (blnShowMessage)
                                strName = power.DisplayNameShort(GlobalOptions.Language);
                            return true;
                        }
                        if (blnShowMessage)
                        {
                            string strTranslate = XmlManager.Load("powers.xml").SelectSingleNode($"/chummer/powers/power[name = {strNodeInnerText.CleanXPath()}]/translate")?.InnerText;
                            strName = !string.IsNullOrEmpty(strTranslate)
                                ? $"{Environment.NewLine}\t{strTranslate} ({LanguageManager.GetString("Tab_Adept", GlobalOptions.Language)})"
                                : $"{Environment.NewLine}\t{strNodeInnerText} ({LanguageManager.GetString("Tab_Adept", GlobalOptions.Language)})";
                        }
                        return false;
                    }
                    case "quality":
                    {
                        Quality quality = xmlNode.Attributes?["extra"] != null
                            ? objCharacter.Qualities.FirstOrDefault(q => q.Name == strNodeInnerText && q.Extra == xmlNode.Attributes?["extra"].InnerText && q.Name != strIgnoreQuality)
                            : objCharacter.Qualities.FirstOrDefault(q => q.Name == strNodeInnerText && q.Name != strIgnoreQuality);
                        if (quality != null)
                        {
                            if (blnShowMessage)
                                strName = quality.DisplayNameShort(GlobalOptions.Language);
                            return true;
                        }
                        if (!blnShowMessage) return false;
                        string strTranslate = XmlManager.Load("qualities.xml").SelectSingleNode($"/chummer/qualities/quality[name = {strNodeInnerText.CleanXPath()}]/translate")?.InnerText;
                        strName = !string.IsNullOrEmpty(strTranslate)
                            ? $"{Environment.NewLine}\t{strTranslate} ({LanguageManager.GetString("String_Quality", GlobalOptions.Language)})"
                            : $"{Environment.NewLine}\t{strNodeInnerText} ({LanguageManager.GetString("String_Quality", GlobalOptions.Language)})";
                        return false;
                    }
                case "skill":
                    {
                        string strSpec = xmlNode["spec"]?.InnerText;
                        string strValue = xmlNode["val"]?.InnerText;
                        int intValue = Convert.ToInt32(strValue);
                        // Check if the character has the required Skill.
                        if (xmlNode["type"] != null)
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
                                    strName = objKnowledgeSkill.DisplayNameMethod(GlobalOptions.Language);
                                    if (!string.IsNullOrEmpty(strSpec) && !objCharacter.Improvements.Any(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.DisableSpecializationEffects && objImprovement.UniqueName == objKnowledgeSkill.Name && string.IsNullOrEmpty(objImprovement.Condition) && objImprovement.Enabled))
                                    {
                                        strName += $" ({strSpec})";
                                    }
                                    if (!string.IsNullOrEmpty(strValue))
                                    {
                                        strName += $" {strValue}";
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
                                    objSkill = objCharacter.SkillsSection.GetActiveSkill(strNodeName + " (" + strSpec + ")");
                                if (objSkill != null && (xmlNode["spec"] == null || objSkill.Specializations.Any(objSpec => objSpec.Name == strSpec)) && objSkill.TotalBaseRating >= intValue)
                                {
                                    if (blnShowMessage)
                                    {
                                        strName = objSkill.DisplayNameMethod(GlobalOptions.Language);
                                        if (!string.IsNullOrEmpty(strSpec) && !objCharacter.Improvements.Any(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.DisableSpecializationEffects && objImprovement.UniqueName == objSkill.Name && string.IsNullOrEmpty(objImprovement.Condition) && objImprovement.Enabled))
                                        {
                                            strName += $" ({strSpec})";
                                        }
                                        if (!string.IsNullOrEmpty(strValue))
                                        {
                                            strName += $" {strValue}";
                                        }
                                    }
                                    return true;
                                }
                            }
                        }
                        if (blnShowMessage)
                        {
                            XmlDocument xmlSkillDoc = XmlManager.Load("skills.xml");
                            string strTranslate = xmlSkillDoc.SelectSingleNode($"/chummer/skills/skill[name = {strNodeInnerText.CleanXPath()}]/translate")?.InnerText ??
                                                    xmlSkillDoc.SelectSingleNode($"/chummer/knowledgeskills/skill[name = {strNodeInnerText.CleanXPath()}]/translate")?.InnerText;
                            strName = !string.IsNullOrEmpty(strTranslate) ? $"{Environment.NewLine}\t{strTranslate}" : $"{Environment.NewLine}\t{strNodeInnerText}";
                            if (!string.IsNullOrEmpty(strSpec))
                            {
                                strName += $" ({strSpec})";
                            }
                            if (!string.IsNullOrEmpty(strValue))
                            {
                                strName += $" {strValue}";
                            }
                            strName += $" ({LanguageManager.GetString("Tab_Skills", GlobalOptions.Language)})";
                        }
                        return false;
                    }
                case "skillgrouptotal":
                    {
                        // Check if the total combined Ratings of Skill Groups adds up to a particular total.
                        int intTotal = 0;
                        string[] strGroups = xmlNode["skillgroups"]?.InnerText.Split('+');
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
                                            objOutputString.Append(objGroup.DisplayName + ", ");
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
                            strName = objOutputString + $" ({LanguageManager.GetString("String_ExpenseSkillGroup", GlobalOptions.Language)})";
                        }
                        return intTotal >= Convert.ToInt32(xmlNode["val"]?.InnerText);
                    }
                case "spell":
                    {
                        Spell objSpell = objCharacter.Spells.FirstOrDefault(x => x.Name == strNodeInnerText);
                        if (objSpell != null)
                        {
                            if (blnShowMessage)
                                strName = objSpell.DisplayNameShort(GlobalOptions.Language);
                            return true;
                        }
                        if (blnShowMessage)
                        {
                            // Check for a specific Spell.
                            string strTranslate = XmlManager.Load("spells.xml").SelectSingleNode($"/chummer/spells/spell[name = {strNodeInnerText.CleanXPath()}]/translate")?.InnerText;
                            strName = !string.IsNullOrEmpty(strTranslate)
                                ? $"{Environment.NewLine}\t{strTranslate} ({LanguageManager.GetString("String_DescSpell", GlobalOptions.Language)})"
                                : $"{Environment.NewLine}\t{strNodeInnerText} ({LanguageManager.GetString("String_DescSpell", GlobalOptions.Language)})";
                        }
                        return false;
                    }
                case "spellcategory":
                    {
                        // Check for a specified amount of a particular Spell category.
                        if (blnShowMessage)
                        {
                            string strTranslate = XmlManager.Load("spells.xml").SelectSingleNode($"/chummer/categories/category[text() = {strNodeName.CleanXPath()}]/@translate")?.InnerText;
                            strName = !string.IsNullOrEmpty(strTranslate)
                                ? $"{Environment.NewLine}\t{strTranslate} ({LanguageManager.GetString("String_SpellCategory", GlobalOptions.Language)})"
                                : $"{Environment.NewLine}\t{strNodeInnerText} ({LanguageManager.GetString("String_SpellCategory", GlobalOptions.Language)})";
                        }
                        return objCharacter.Spells.Count(objSpell => objSpell.Category == strNodeName) >= Convert.ToInt32(xmlNode["count"]?.InnerText);
                    }
                case "spelldescriptor":
                    {
                        string strCount = xmlNode["count"]?.InnerText ?? string.Empty;
                        // Check for a specified amount of a particular Spell Descriptor.
                        if (blnShowMessage)
                            strName = Environment.NewLine + '\t' + LanguageManager.GetString("Label_Descriptors", GlobalOptions.Language) + " >= " + strCount;
                        return objCharacter.Spells.Count(objSpell => objSpell.Descriptors.Contains(strNodeName)) >= Convert.ToInt32(strCount);
                    }
                case "streetcredvsnotoriety":
                    {
                        // Street Cred must be higher than Notoriety.
                        if (blnShowMessage)
                            strName = Environment.NewLine + '\t' + LanguageManager.GetString("String_StreetCred", GlobalOptions.Language) + " >= " + LanguageManager.GetString("String_Notoriety", GlobalOptions.Language);
                        return objCharacter.StreetCred >= objCharacter.Notoriety;
                    }
                case "submersiongrade":
                    {
                        // Character's initiate grade must be higher than or equal to the required value.
                        if (blnShowMessage)
                            strName = Environment.NewLine + '\t' + LanguageManager.GetString("String_SubmersionGrade", GlobalOptions.Language) + " >= " + strNodeInnerText;
                        return objCharacter.SubmersionGrade >= Convert.ToInt32(strNodeInnerText);
                    }
                case "tradition":
                    {
                        // Character needs a specific Tradition.
                        if (blnShowMessage)
                        {
                            string strTranslate = XmlManager.Load("traditions.xml").SelectSingleNode($"/chummer/traditions/tradition[name = {strNodeInnerText.CleanXPath()}]/translate")?.InnerText;
                            strName = !string.IsNullOrEmpty(strTranslate)
                                ? $"{Environment.NewLine}\t{strTranslate} ({LanguageManager.GetString("String_Tradition", GlobalOptions.Language)})"
                                : $"{Environment.NewLine}\t{strNodeInnerText} ({LanguageManager.GetString("String_Tradition", GlobalOptions.Language)})";
                        }
                        return objCharacter.MagicTradition.Name == strNodeInnerText;
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
            if (objXmlGear == null)
                return false;
            //TODO: Better handler for restricted gear
            if (objCharacter.Created || objCharacter.RestrictedGear || objCharacter.IgnoreRules)
                return true;
            // Avail.

            XmlNode objAvailNode = objXmlGear["avail"];
            if (objAvailNode == null)
            {
                int intHighestAvailNode = 0;
                foreach (XmlNode objLoopNode in objXmlGear.ChildNodes)
                {
                    if (objLoopNode.NodeType == XmlNodeType.Element && objLoopNode.Name.StartsWith("avail"))
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
                    XmlNode objLoopNode = objXmlGear["avail" + i.ToString(GlobalOptions.InvariantCultureInfo)];
                    if (objLoopNode != null)
                    {
                        objAvailNode = objLoopNode;
                        break;
                    }
                }
            }
            // If avail contains "F" or "R", remove it from the string so we can use the expression.
            string strAvailExpr = objAvailNode?.InnerText ?? string.Empty;
            if (strAvailExpr.StartsWith("FixedValues("))
            {
                string[] strValues = strAvailExpr.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
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
                intAvail += Convert.ToInt32(objProcess);

            return intAvail <= objCharacter.MaximumAvailability;
        }

        public static bool CheckNuyenRestriction(XmlNode objXmlGear, decimal decMaxNuyen, decimal decCostMultiplier = 1.0m, int intRating = 1)
        {
            // Cost.
            decimal decCost = 0.0m;
            XmlNode objCostNode = objXmlGear["cost"];
            if (objCostNode == null)
            {
                int intCostRating = 1;
                XmlNodeList xmlChildNodesList = objXmlGear.SelectNodes("*");
                if (xmlChildNodesList?.Count > 0)
                {
                    foreach (XmlNode objLoopNode in xmlChildNodesList)
                    {
                        if (objLoopNode.Name.StartsWith("cost"))
                        {
                            string strLoopCostString = objLoopNode.Name.Substring(4);
                            if (int.TryParse(strLoopCostString, out int intTmp) && intTmp <= intRating)
                            {
                                intCostRating = Math.Max(intCostRating, intTmp);
                            }
                        }
                    }
                }

                objCostNode = objXmlGear.SelectSingleNode("cost" + intCostRating.ToString(GlobalOptions.InvariantCultureInfo));
            }
            string strCost = objCostNode?.InnerText;
            if (!string.IsNullOrEmpty(strCost))
            {
                if (strCost.StartsWith("FixedValues("))
                {
                    string[] strValues = strCost.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                    strCost = strValues[Math.Max(Math.Min(intRating, strValues.Length) - 1, 0)];
                }
                else if (strCost.StartsWith("Variable"))
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
                if (xmlNode.SelectSingleNode("chargenonly") != null)
                {
                    if (blnShowMessage)
                    {
                        MessageBox.Show(LanguageManager.GetString("Message_SelectGeneric_ChargenRestriction", GlobalOptions.Language).Replace("{0}", strLocalName),
                            LanguageManager.GetString("MessageTitle_SelectGeneric_Restriction", GlobalOptions.Language).Replace("{0}", strLocalName),
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
                        MessageBox.Show(LanguageManager.GetString("Message_SelectGeneric_CareerOnlyRestriction", GlobalOptions.Language).Replace("{0}", strLocalName),
                            LanguageManager.GetString("MessageTitle_SelectGeneric_Restriction", GlobalOptions.Language).Replace("{0}", strLocalName),
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    return false;
                }
                if (objCharacter.BuildMethod == CharacterBuildMethod.Priority || objCharacter.BuildMethod == CharacterBuildMethod.SumtoTen)
                {
                    if (xmlNode.SelectSingleNode("onlyprioritygiven") != null)
                    {
                        if (blnShowMessage)
                        {
                            MessageBox.Show(LanguageManager.GetString("Message_SelectGeneric_PriorityRestriction", GlobalOptions.Language).Replace("{0}", strLocalName),
                                LanguageManager.GetString("MessageTitle_SelectGeneric_Restriction", GlobalOptions.Language).Replace("{0}", strLocalName),
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
                    foreach (string strAttribute in AttributeSection.AttributeStrings)
                    {
                        CharacterAttrib objLoopAttribute = objCharacter.GetAttribute(strAttribute);
                        objLimitString.CheapReplace(strLimitString, "{" + strAttribute + "}", () => objLoopAttribute.TotalValue.ToString());
                        objLimitString.CheapReplace(strLimitString, "{" + strAttribute + "Base}", () => objLoopAttribute.TotalBase.ToString());
                    }
                    foreach (string strLimb in Character.LimbStrings)
                    {
                        objLimitString.CheapReplace(strLimitString, "{" + strLimb + "}", () => (string.IsNullOrEmpty(strLocation) ? objCharacter.LimbCount(strLimb) : objCharacter.LimbCount(strLimb) / 2).ToString());
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
                                    objTempList = new List<MartialArtTechnique>(objArt.Techniques.Count);
                                    objTempList.AddRange(objArt.Techniques);
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

                    int intLimit = Convert.ToInt32(strLimitString);
                    int intExtendedLimit = intLimit;
                    string strLimitWithInclusions = xmlNode.SelectSingleNode("limitwithinclusions")?.Value;
                    if (!string.IsNullOrEmpty(strLimitWithInclusions))
                    {
                        intExtendedLimit = Convert.ToInt32(strLimitWithInclusions);
                    }
                    int intCount = 0;
                    int intExtendedCount = 0;
                    if (objListToCheck != null || blnCheckCyberwareChildren)
                    {
                        var lstToCheck = objListToCheck?.ToList() ?? new List<IHasName>();
                        string strNameNode = xmlNode.SelectSingleNode("name")?.Value;
                        if (blnCheckCyberwareChildren)
                        {
                            intCount = string.IsNullOrEmpty(strLocation)
                                ? objCharacter.Cyberware.DeepCount(x => x.Children, x => string.IsNullOrEmpty(x.PlugsIntoModularMount) && strNameNode == x.Name)
                                : objCharacter.Cyberware.DeepCount(x => x.Children, x => string.IsNullOrEmpty(x.PlugsIntoModularMount) && x.Location == strLocation && strNameNode == x.Name);
                        }
                        else
                            intCount = lstToCheck.Count(objItem => strNameNode == objItem.Name);
                        intExtendedCount = intCount;
                        // In case one item is split up into multiple entries with different names, e.g. Indomitable quality, we need to be able to check all those entries against the limit
                        XPathNavigator xmlIncludeInLimit = xmlNode.SelectSingleNode("includeinlimit");
                        if (xmlIncludeInLimit != null)
                        {
                            List<string> lstNamesIncludedInLimit = new List<string>();
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
                                intExtendedCount = lstToCheck.Count(objItem => lstNamesIncludedInLimit.Any(objLimitName => objLimitName == objItem.Name));
                        }
                    }
                    if (intCount >= intLimit || intExtendedCount >= intExtendedLimit)
                    {
                        if (blnShowMessage)
                        {
                            string limitTitle = LanguageManager.GetString("MessageTitle_SelectGeneric_Limit", GlobalOptions.Language).Replace("{0}", strLocalName);
                            string limitMessage = LanguageManager.GetString("Message_SelectGeneric_Limit", GlobalOptions.Language).Replace("{0}", strLocalName).Replace("{1}", intLimit == 0 ? "1" : intLimit.ToString());
                            MessageBox.Show(limitMessage, limitTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                        if (xmlForbiddenItemNode.TestNodeRequirements(objCharacter, out string strName, strIgnoreQuality, blnShowMessage))
                        {
                            if (blnShowMessage)
                            {
                                string forbiddenTitle = LanguageManager.GetString("MessageTitle_SelectGeneric_Restriction", GlobalOptions.Language).Replace("{0}", strLocalName);
                                string forbiddenMessage = LanguageManager.GetString("Message_SelectGeneric_Restriction", GlobalOptions.Language).Replace("{0}", strLocalName) + strName;
                                MessageBox.Show(forbiddenMessage, forbiddenTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    StringBuilder objThisRequirement = new StringBuilder(Environment.NewLine + LanguageManager.GetString("Message_SelectQuality_OneOf", GlobalOptions.Language));
                    foreach (XPathNavigator xmlRequiredItemNode in objXmlOneOf.SelectChildren(XPathNodeType.Element))
                    {
                        if (xmlRequiredItemNode.TestNodeRequirements(objCharacter, out string strName, strIgnoreQuality, blnShowMessage))
                        {
                            blnOneOfMet = true;
                            break;
                        }
                        if (blnShowMessage)
                            objThisRequirement.Append(strName);
                    }

                    // Update the flag for requirements met.
                    if (!blnOneOfMet)
                        blnRequirementMet = false;
                    if (blnShowMessage && !blnOneOfMet)
                        objRequirement.Append(objThisRequirement);
                    else if (!blnRequirementMet)
                        break;
                }

                if (blnRequirementMet || blnShowMessage)
                {
                    // Loop through the allof requirements.
                    foreach (XPathNavigator objXmlAllOf in xmlRequiredNode.Select("allof"))
                    {
                        bool blnAllOfMet = true;
                        StringBuilder objThisRequirement = new StringBuilder(Environment.NewLine + LanguageManager.GetString("Message_SelectQuality_AllOf", GlobalOptions.Language));
                        foreach (XPathNavigator xmlRequiredItemNode in objXmlAllOf.SelectChildren(XPathNodeType.Element))
                        {
                            // If this item was not found, fail the AllOfMet condition.
                            if (!xmlRequiredItemNode.TestNodeRequirements(objCharacter, out string strName, strIgnoreQuality, blnShowMessage))
                            {
                                blnAllOfMet = false;
                                if (blnShowMessage)
                                    objThisRequirement.Append(strName);
                                else
                                    break;
                            }
                        }

                        // Update the flag for requirements met.
                        if (!blnAllOfMet)
                            blnRequirementMet = false;
                        if (blnShowMessage)
                            objRequirement.Append(objThisRequirement);
                        else if (!blnRequirementMet)
                            break;
                    }
                }

                // The character has not met the requirements, so display a message and uncheck the item.
                if (!blnRequirementMet)
                {
                    if (blnShowMessage)
                    {
                        string requiredTitle = LanguageManager.GetString("MessageTitle_SelectGeneric_Requirement", GlobalOptions.Language).Replace("{0}", strLocalName);
                        string requiredMessage = LanguageManager.GetString("Message_SelectGeneric_Requirement", GlobalOptions.Language).Replace("{0}", strLocalName) + objRequirement;
                        MessageBox.Show(requiredMessage, requiredTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    return false;
                }
            }
            return true;
        }

        public static bool TestNodeRequirements(this XPathNavigator xmlNode, Character objCharacter, out string strName, string strIgnoreQuality = "", bool blnShowMessage = true)
        {
            strName = string.Empty;
            if (xmlNode == null || objCharacter == null)
            {
                return false;
            }
            string strNodeInnerText = xmlNode.Value;
            string strNodeName = xmlNode.SelectSingleNode("name")?.Value ?? string.Empty;
            switch (xmlNode.Name)
            {
                case "attribute":
                    {
                        // Check to see if an Attribute meets a requirement.
                        CharacterAttrib objAttribute = objCharacter.GetAttribute(strNodeName);
                        int intTargetValue = Convert.ToInt32(xmlNode.SelectSingleNode("total")?.Value);
                        if (blnShowMessage)
                            strName = $"{Environment.NewLine}\t{objAttribute.DisplayAbbrev} {intTargetValue}";
                        // Special cases for when we want to check if a special attribute is enabled
                        if (intTargetValue == 1)
                        {
                            if (objAttribute.Abbrev == "MAG")
                                return objCharacter.MAGEnabled;
                            if (objAttribute.Abbrev == "MAGAdept")
                                return objCharacter.MAGEnabled && objCharacter.IsMysticAdept;
                            if (objAttribute.Abbrev == "RES")
                                return objCharacter.RESEnabled;
                            if (objAttribute.Abbrev == "DEP")
                                return objCharacter.DEPEnabled;
                        }
                        return objAttribute.TotalValue >= intTargetValue;
                    }
                case "attributetotal":
                    {
                        string strNodeAttributes = xmlNode.SelectSingleNode("attributes")?.Value ?? string.Empty;
                        int intNodeVal = Convert.ToInt32(xmlNode.SelectSingleNode("val")?.Value);
                        // Check if the character's Attributes add up to a particular total.
                        string strAttributes = strNodeAttributes;
                        string strValue = strNodeAttributes;
                        foreach (string strAttribute in AttributeSection.AttributeStrings)
                        {
                            CharacterAttrib objLoopAttrib = objCharacter.GetAttribute(strAttribute);
                            if (strNodeAttributes.Contains(objLoopAttrib.Abbrev))
                            {
                                strAttributes = strAttributes.Replace(strAttribute, objLoopAttrib.DisplayAbbrev);
                                strValue = strValue.Replace(strAttribute, objLoopAttrib.Value.ToString());
                            }
                        }
                        if (blnShowMessage)
                            strName = $"{Environment.NewLine}\t{strAttributes} {intNodeVal}";
                        object objProcess = CommonFunctions.EvaluateInvariantXPath(strValue, out bool blnIsSuccess);
                        return (blnIsSuccess ? Convert.ToInt32(objProcess) : 0) >= intNodeVal;
                    }
                case "careerkarma":
                    {
                        // Check Career Karma requirement.
                        if (blnShowMessage)
                            strName = Environment.NewLine + '\t' + LanguageManager.GetString("Message_SelectQuality_RequireKarma", GlobalOptions.Language).Replace("{0}", strNodeInnerText);
                        return objCharacter.CareerKarma >= Convert.ToInt32(strNodeInnerText);
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
                                    strName = critterPower.DisplayNameShort(GlobalOptions.Language);
                                return true;
                            }
                        }
                        if (blnShowMessage)
                        {
                            string strTranslate = XmlManager.Load("critterpowers.xml").SelectSingleNode($"/chummer/powers/power[name = {strNodeInnerText.CleanXPath()}]/translate")?.InnerText;
                            strName = !string.IsNullOrEmpty(strTranslate)
                                ? $"{Environment.NewLine}\t{strTranslate} ({LanguageManager.GetString("Tab_Critter", GlobalOptions.Language)})"
                                : $"{Environment.NewLine}\t{strNodeInnerText} ({LanguageManager.GetString("Tab_Critter", GlobalOptions.Language)})";
                        }
                        return false;
                    }
                case "bioware":
                    {
                        int count = Convert.ToInt32(xmlNode.SelectSingleNode("@count")?.Value ?? "1");
                        if (blnShowMessage)
                        {
                            string strTranslate = XmlManager.Load("bioware.xml").SelectSingleNode($"/chummer/biowares/bioware[name = {strNodeInnerText.CleanXPath()}]/translate")?.InnerText;
                            strName = !string.IsNullOrEmpty(strTranslate)
                                ? $"{Environment.NewLine}\t{LanguageManager.GetString("Label_Bioware", GlobalOptions.Language)} {strTranslate}"
                                : $"{Environment.NewLine}\t{LanguageManager.GetString("Label_Bioware", GlobalOptions.Language)} {strNodeInnerText}";
                        }
                        string strWareNodeSelectAttribute = xmlNode.SelectSingleNode("@select")?.Value ?? string.Empty;
                        return objCharacter.Cyberware.DeepCount(x => x.Children, objCyberware => objCyberware.Name == strNodeInnerText &&
                                objCyberware.SourceType == Improvement.ImprovementSource.Bioware && string.IsNullOrEmpty(objCyberware.PlugsIntoModularMount) &&
                               (string.IsNullOrEmpty(strWareNodeSelectAttribute) || strWareNodeSelectAttribute == objCyberware.Extra)) >= count;
                    }
                case "cyberware":
                    {
                        int count = Convert.ToInt32(xmlNode.SelectSingleNode("@count")?.Value ?? "1");
                        if (blnShowMessage)
                        {
                            string strTranslate = XmlManager.Load("cyberware.xml").SelectSingleNode($"/chummer/cyberwares/cyberware[name = {strNodeInnerText.CleanXPath()}]/translate")?.InnerText;
                            strName = !string.IsNullOrEmpty(strTranslate)
                                ? $"{Environment.NewLine}\t{LanguageManager.GetString("Label_Cyberware", GlobalOptions.Language)} {strTranslate}"
                                : $"{Environment.NewLine}\t{LanguageManager.GetString("Label_Cyberware", GlobalOptions.Language)} {strNodeInnerText}";
                        }
                        string strWareNodeSelectAttribute = xmlNode.SelectSingleNode("@select")?.Value ?? string.Empty;
                        return objCharacter.Cyberware.DeepCount(x => x.Children, objCyberware => objCyberware.Name == strNodeInnerText &&
                                objCyberware.SourceType == Improvement.ImprovementSource.Cyberware && string.IsNullOrEmpty(objCyberware.PlugsIntoModularMount) &&
                               (string.IsNullOrEmpty(strWareNodeSelectAttribute) || strWareNodeSelectAttribute == objCyberware.Extra)) >= count;
                    }
                case "biowarecontains":
                    {
                        int count = Convert.ToInt32(xmlNode.SelectSingleNode("@count")?.Value ?? "1");
                        if (blnShowMessage)
                        {
                            string strTranslate = XmlManager.Load("bioware.xml").SelectSingleNode($"/chummer/biowares/bioware[name = {strNodeInnerText.CleanXPath()}]/translate")?.InnerText;
                            strName = !string.IsNullOrEmpty(strTranslate)
                                ? $"{Environment.NewLine}\t{LanguageManager.GetString("Label_Bioware", GlobalOptions.Language)} {strTranslate}"
                                : $"{Environment.NewLine}\t{LanguageManager.GetString("Label_Bioware", GlobalOptions.Language)} {strNodeInnerText}";
                        }
                        string strWareNodeSelectAttribute = xmlNode.SelectSingleNode("@select")?.Value ?? string.Empty;
                        return objCharacter.Cyberware.DeepCount(x => x.Children, objCyberware => objCyberware.Name.Contains(strNodeInnerText) &&
                                objCyberware.SourceType == Improvement.ImprovementSource.Bioware && string.IsNullOrEmpty(objCyberware.PlugsIntoModularMount) &&
                               (string.IsNullOrEmpty(strWareNodeSelectAttribute) || strWareNodeSelectAttribute == objCyberware.Extra)) >= count;
                    }
                case "cyberwarecontains":
                    {
                        int count = Convert.ToInt32(xmlNode.SelectSingleNode("@count")?.Value ?? "1");
                        if (blnShowMessage)
                        {
                            string strTranslate = XmlManager.Load("cyberware.xml").SelectSingleNode($"/chummer/cyberwares/cyberware[name = {strNodeInnerText.CleanXPath()}]/translate")?.InnerText;
                            strName = !string.IsNullOrEmpty(strTranslate)
                                ? $"{Environment.NewLine}\t{LanguageManager.GetString("Label_Cyberware", GlobalOptions.Language)} {strTranslate}"
                                : $"{Environment.NewLine}\t{LanguageManager.GetString("Label_Cyberware", GlobalOptions.Language)} {strNodeInnerText}";
                        }
                        string strWareNodeSelectAttribute = xmlNode.SelectSingleNode("@select")?.Value ?? string.Empty;
                        return objCharacter.Cyberware.DeepCount(x => x.Children, objCyberware => objCyberware.Name.Contains(strNodeInnerText) &&
                                objCyberware.SourceType == Improvement.ImprovementSource.Cyberware && string.IsNullOrEmpty(objCyberware.PlugsIntoModularMount) &&
                               (string.IsNullOrEmpty(strWareNodeSelectAttribute) || strWareNodeSelectAttribute == objCyberware.Extra)) >= count;
                    }
                case "damageresistance":
                    {
                        // Damage Resistance must be a particular value.
                        if (blnShowMessage)
                            strName = $"{Environment.NewLine}\t{LanguageManager.GetString("String_DamageResistance", GlobalOptions.Language)}";
                        return objCharacter.BOD.TotalValue + ImprovementManager.ValueOf(objCharacter, Improvement.ImprovementType.DamageResistance) >= Convert.ToInt32(strNodeInnerText);
                    }
                case "ess":
                    {
                        string strEssNodeGradeAttributeText = xmlNode.SelectSingleNode("@grade")?.Value ?? string.Empty;
                        if (!string.IsNullOrEmpty(strEssNodeGradeAttributeText))
                        {
                            HashSet<string> setEssNodeGradeAttributeText = new HashSet<string>(strEssNodeGradeAttributeText.Split(','));
                            decimal decGrade =
                                objCharacter.Cyberware.Where(
                                        objCyberware =>
                                            setEssNodeGradeAttributeText.Any(func => objCyberware.Grade.Name.Contains(func)))
                                    .AsParallel().Sum(objCyberware => objCyberware.CalculatedESS());
                            if (strNodeInnerText.StartsWith('-'))
                            {
                                // Essence must be less than the value.
                                if (blnShowMessage)
                                    strName = Environment.NewLine + '\t' +
                                       LanguageManager.GetString("Message_SelectQuality_RequireESSGradeBelow", GlobalOptions.Language)
                                           .Replace("{0}", strNodeInnerText)
                                           .Replace("{1}", strEssNodeGradeAttributeText)
                                           .Replace("{2}", decGrade.ToString(GlobalOptions.InvariantCultureInfo));
                                return decGrade < Convert.ToDecimal(strNodeInnerText.TrimStart('-'), GlobalOptions.InvariantCultureInfo);
                            }
                            // Essence must be equal to or greater than the value.
                            if (blnShowMessage)
                                strName = Environment.NewLine + '\t' +
                                   LanguageManager.GetString("Message_SelectQuality_RequireESSGradeAbove", GlobalOptions.Language)
                                       .Replace("{0}", strNodeInnerText)
                                       .Replace("{1}", strEssNodeGradeAttributeText)
                                       .Replace("{2}", decGrade.ToString(GlobalOptions.InvariantCultureInfo));
                            return decGrade >= Convert.ToDecimal(strNodeInnerText, GlobalOptions.InvariantCultureInfo);
                        }
                        // Check Essence requirement.
                        if (strNodeInnerText.StartsWith('-'))
                        {
                            // Essence must be less than the value.
                            if (blnShowMessage)
                                strName = Environment.NewLine + '\t' +
                                   LanguageManager.GetString("Message_SelectQuality_RequireESSBelow", GlobalOptions.Language)
                                       .Replace("{0}", strNodeInnerText)
                                       .Replace("{1}", objCharacter.Essence().ToString(GlobalOptions.InvariantCultureInfo));
                            return objCharacter.Essence() < Convert.ToDecimal(strNodeInnerText.TrimStart('-'), GlobalOptions.InvariantCultureInfo);
                        }
                        // Essence must be equal to or greater than the value.
                        if (blnShowMessage)
                            strName = Environment.NewLine + '\t' +
                               LanguageManager.GetString("Message_SelectQuality_RequireESSAbove", GlobalOptions.Language)
                                   .Replace("{0}", strNodeInnerText)
                                   .Replace("{1}", objCharacter.Essence().ToString(GlobalOptions.InvariantCultureInfo));
                        return objCharacter.Essence() >= Convert.ToDecimal(strNodeInnerText, GlobalOptions.InvariantCultureInfo);
                    }
                case "echo":
                    {
                        Metamagic objMetamagic = objCharacter.Metamagics.FirstOrDefault(x => x.Name == strNodeInnerText && x.SourceType == Improvement.ImprovementSource.Echo);
                        if (objMetamagic != null)
                        {
                            if (blnShowMessage)
                                strName = objMetamagic.DisplayNameShort(GlobalOptions.Language);
                            return true;
                        }
                        if (blnShowMessage)
                        {
                            string strTranslate = XmlManager.Load("echoes.xml").SelectSingleNode($"/chummer/echoes/echo[name = {strNodeInnerText.CleanXPath()}]/translate")?.InnerText;
                            strName = !string.IsNullOrEmpty(strTranslate)
                                ? $"{Environment.NewLine}\t{strTranslate} ({LanguageManager.GetString("String_Echo", GlobalOptions.Language)})"
                                : $"{Environment.NewLine}\t{strNodeInnerText} ({LanguageManager.GetString("String_Echo", GlobalOptions.Language)})";
                        }
                        return false;
                    }
                case "group":
                    {
                        // Check that clustered options are present (Magical Tradition + Skill 6, for example)
                        bool blnResult = true;
                        string strResultName = string.Empty;
                        foreach (XPathNavigator xmlChildNode in xmlNode.SelectChildren(XPathNodeType.Element))
                        {
                            blnResult = xmlChildNode.TestNodeRequirements(objCharacter, out strResultName, strIgnoreQuality, blnShowMessage);
                            if (!blnResult)
                            {
                                break;
                            }
                        }
                        if (blnShowMessage)
                            strName = strResultName;
                        return blnResult;
                    }
                case "grouponeof":
                {
                    // Check that one of the clustered options are present
                    bool blnResult = false;
                    string strResultName = LanguageManager.GetString("Message_SelectQuality_OneOf", GlobalOptions.Language);
                    foreach (XPathNavigator xmlChildNode in xmlNode.SelectChildren(XPathNodeType.Element))
                    {
                        blnResult = xmlChildNode.TestNodeRequirements(objCharacter, out string strLoopResult, strIgnoreQuality, blnShowMessage);
                        if (blnResult)
                        {
                            break;
                        }

                        strResultName += strLoopResult;
                    }
                    if (blnShowMessage)
                        strName = strResultName;
                    return blnResult;
                }
                case "initiategrade":
                    {
                        // Character's initiate grade must be higher than or equal to the required value.
                        if (blnShowMessage)
                            strName = Environment.NewLine + '\t' + LanguageManager.GetString("String_InitiateGrade", GlobalOptions.Language) + " >= " + strNodeInnerText;
                        return objCharacter.InitiateGrade >= Convert.ToInt32(strNodeInnerText);
                    }
                case "martialart":
                    {
                        MartialArt objMartialArt = objCharacter.MartialArts.FirstOrDefault(x => x.Name == strNodeInnerText);
                        if (objMartialArt != null)
                        {
                            if (blnShowMessage)
                                strName = objMartialArt.DisplayNameShort(GlobalOptions.Language);
                            return true;
                        }
                        if (blnShowMessage)
                        {
                            // Character needs a specific Martial Art.
                            string strTranslate = XmlManager.Load("martialarts.xml").SelectSingleNode($"/chummer/martialarts/martialart[name = {strNodeInnerText.CleanXPath()}]/translate")?.InnerText;
                            strName = !string.IsNullOrEmpty(strTranslate)
                                ? $"{Environment.NewLine}\t{strTranslate} ({LanguageManager.GetString("String_MartialArt", GlobalOptions.Language)})"
                                : $"{Environment.NewLine}\t{strNodeInnerText} ({LanguageManager.GetString("String_MartialArt", GlobalOptions.Language)})";
                        }
                        return false;
                    }
                case "martialtechnique":
                    {
                        foreach (MartialArt objMartialArt in objCharacter.MartialArts)
                        {
                            MartialArtTechnique objMartialArtTechnique = objMartialArt.Techniques.FirstOrDefault(x => x.Name == strNodeInnerText);
                            if (objMartialArtTechnique != null)
                            {
                                if (blnShowMessage)
                                    strName = objMartialArtTechnique.DisplayName(GlobalOptions.Language);
                                return true;
                            }
                        }
                        if (blnShowMessage)
                        {
                            // Character needs a specific Martial Arts technique.
                            string strTranslate = XmlManager.Load("martialarts.xml").SelectSingleNode($"/chummer/techniques/technique[name = {strNodeInnerText.CleanXPath()}]/translate")?.InnerText;
                            strName = !string.IsNullOrEmpty(strTranslate)
                                ? $"{Environment.NewLine}\t{strTranslate} ({LanguageManager.GetString("String_MartialArt", GlobalOptions.Language)})"
                                : $"{Environment.NewLine}\t{strNodeInnerText} ({LanguageManager.GetString("String_MartialArt", GlobalOptions.Language)})";
                        }
                        return false;
                    }
                case "metamagic":
                    {
                        Metamagic objMetamagic = objCharacter.Metamagics.FirstOrDefault(x => x.Name == strNodeInnerText && x.SourceType == Improvement.ImprovementSource.Metamagic);
                        if (objMetamagic != null)
                        {
                            if (blnShowMessage)
                                strName = objMetamagic.DisplayNameShort(GlobalOptions.Language);
                            return true;
                        }
                        if (blnShowMessage)
                        {
                            string strTranslate = XmlManager.Load("metamagic.xml").SelectSingleNode($"/chummer/metamagics/metamagic[name = {strNodeInnerText.CleanXPath()}]/translate")?.InnerText;
                            strName = !string.IsNullOrEmpty(strTranslate)
                                ? $"{Environment.NewLine}\t{strTranslate} ({LanguageManager.GetString("String_Metamagic", GlobalOptions.Language)})"
                                : $"{Environment.NewLine}\t{strNodeInnerText} ({LanguageManager.GetString("String_Metamagic", GlobalOptions.Language)})";
                        }
                        return false;
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
                            return true;
                        }

                        XPathNavigator xmlMetamagicDoc = XmlManager.Load("metamagic.xml").GetFastNavigator()
                            .SelectSingleNode("/chummer");
                        if (blnShowMessage)
                        {
                            string strTranslateArt = xmlMetamagicDoc
                                ?.SelectSingleNode($"arts/art[name = {strNodeInnerText.CleanXPath()}]/translate")?.Value;
                            strName = !string.IsNullOrEmpty(strTranslateArt)
                                ? $"{Environment.NewLine}\t{strTranslateArt} ({LanguageManager.GetString("String_Art", GlobalOptions.Language)})"
                                : $"{Environment.NewLine}\t{strNodeInnerText} ({LanguageManager.GetString("String_Art", GlobalOptions.Language)})";
                        }

                        if (xmlMetamagicDoc != null)
                        {
                            // Loop through the data file for each metamagic to find the Required and Forbidden nodes. 
                            foreach (Metamagic metamagic in objCharacter.Metamagics)
                            {
                                XPathNavigator xmlMetamagicNode =
                                    xmlMetamagicDoc.SelectSingleNode(
                                        $"metamagics/metamagic[name = {metamagic.Name.CleanXPath()}]");
                                if (xmlMetamagicNode != null)
                                {
                                    if (xmlMetamagicNode.SelectSingleNode(
                                            $"required/art[text() = {strNodeInnerText.CleanXPath()}]") != null)
                                    {
                                        return true;
                                    }

                                    if (xmlMetamagicNode.SelectSingleNode(
                                            $"forbidden/art[text() = {strNodeInnerText.CleanXPath()}]") != null)
                                    {
                                        return false;
                                    }
                                }
                                else
                                {
                                    // We couldn't find a metamagic with this name, so it's probably an art. Try and find the node.
                                    // If we can't, it's probably a data entry error. 
                                    xmlMetamagicNode =
                                        xmlMetamagicDoc.SelectSingleNode($"arts/art[name = {metamagic.Name.CleanXPath()}]");
                                    if (xmlMetamagicNode == null)
                                        Utils.BreakIfDebug();
                                    else
                                        return true;
                                }
                            }
                        }

                        return true;
                    }
                    else
                    {
                        Art objArt = objCharacter.Arts.FirstOrDefault(x => x.Name == strNodeInnerText);
                        if (objArt != null)
                        {
                            if (blnShowMessage)
                                strName = objArt.DisplayNameShort(GlobalOptions.Language);
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
                                    strName = objMetamagic.DisplayNameShort(GlobalOptions.Language);
                                return true;
                            }
                        }

                        if (!blnShowMessage) return false;
                        string strTranslate = XmlManager.Load("metamagic.xml")
                            .SelectSingleNode($"/chummer/arts/art[name = {strNodeInnerText.CleanXPath()}]/translate")?.InnerText;
                        strName = !string.IsNullOrEmpty(strTranslate)
                            ? $"{Environment.NewLine}\t{strTranslate} ({LanguageManager.GetString("String_Art", GlobalOptions.Language)})"
                            : $"{Environment.NewLine}\t{strNodeInnerText} ({LanguageManager.GetString("String_Art", GlobalOptions.Language)})";
                        return false;
                    }
                }
                case "metatype":
                    {
                        if (blnShowMessage)
                        {
                            string strXPathFilter = $"/chummer/metatypes/metatype[name = {strNodeInnerText.CleanXPath()}]/translate";
                            // Check the Metatype restriction.
                            string strTranslate = XmlManager.Load("metatypes.xml").SelectSingleNode(strXPathFilter)?.InnerText ??
                                                    XmlManager.Load("critters.xml").SelectSingleNode(strXPathFilter)?.InnerText;
                            strName = !string.IsNullOrEmpty(strTranslate)
                                ? $"{Environment.NewLine}\t{strTranslate} ({LanguageManager.GetString("String_Metatype", GlobalOptions.Language)})"
                                : $"{Environment.NewLine}\t{strNodeInnerText} ({LanguageManager.GetString("String_Metatype", GlobalOptions.Language)})";
                        }
                        return strNodeInnerText == objCharacter.Metatype;
                    }
                case "metatypecategory":
                    {
                        if (blnShowMessage)
                        {
                            string strXPathFilter = $"/chummer/categories/category[text() = {strNodeInnerText.CleanXPath()}]/@translate";
                            // Check the Metatype Category restriction.
                            string strTranslate = XmlManager.Load("metatypes.xml").SelectSingleNode(strXPathFilter)?.InnerText ??
                                                    XmlManager.Load("critters.xml").SelectSingleNode(strXPathFilter)?.InnerText;
                            strName = !string.IsNullOrEmpty(strTranslate)
                                ? $"{Environment.NewLine}\t{strTranslate} ({LanguageManager.GetString("String_MetatypeCategory", GlobalOptions.Language)})"
                                : $"{Environment.NewLine}\t{strNodeInnerText} ({LanguageManager.GetString("String_MetatypeCategory", GlobalOptions.Language)})";
                        }
                        return strNodeInnerText == objCharacter.MetatypeCategory;
                    }
                case "metavariant":
                    {
                        if (blnShowMessage)
                        {
                            string strXPathFilter = $"/chummer/metatypes/metatype/metavariants/metavariant[name = {strNodeInnerText.CleanXPath()}]/translate";
                            // Check the Metavariant restriction.
                            string strTranslate = XmlManager.Load("metatypes.xml").SelectSingleNode(strXPathFilter)?.InnerText ??
                                                    XmlManager.Load("critters.xml").SelectSingleNode(strXPathFilter)?.InnerText;
                            strName = !string.IsNullOrEmpty(strTranslate)
                                ? $"{Environment.NewLine}\t{strTranslate} ({LanguageManager.GetString("String_Metavariant", GlobalOptions.Language)})"
                                : $"{Environment.NewLine}\t{strNodeInnerText} ({LanguageManager.GetString("String_Metavariant", GlobalOptions.Language)})";
                        }
                        return strNodeInnerText == objCharacter.Metavariant;
                    }
                case "power":
                    {
                        // Run through all of the Powers the character has and see if the current required item exists.
                        Power power = objCharacter.Powers.FirstOrDefault(p => p.Name == strNodeInnerText);
                        if (power != null)
                        {
                            if (blnShowMessage)
                                strName = power.DisplayNameShort(GlobalOptions.Language);
                            return true;
                        }
                        if (blnShowMessage)
                        {
                            string strTranslate = XmlManager.Load("powers.xml").SelectSingleNode($"/chummer/powers/power[name = {strNodeInnerText.CleanXPath()}]/translate")?.InnerText;
                            strName = !string.IsNullOrEmpty(strTranslate)
                                ? $"{Environment.NewLine}\t{strTranslate} ({LanguageManager.GetString("Tab_Adept", GlobalOptions.Language)})"
                                : $"{Environment.NewLine}\t{strNodeInnerText} ({LanguageManager.GetString("Tab_Adept", GlobalOptions.Language)})";
                        }
                        return false;
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
                                strName = quality.DisplayNameShort(GlobalOptions.Language);
                            return true;
                        }
                        if (!blnShowMessage) return false;
                        string strTranslate = XmlManager.Load("qualities.xml").SelectSingleNode($"/chummer/qualities/quality[name = {strNodeInnerText.CleanXPath()}]/translate")?.InnerText;
                        strName = !string.IsNullOrEmpty(strTranslate)
                            ? $"{Environment.NewLine}\t{strTranslate} ({LanguageManager.GetString("String_Quality", GlobalOptions.Language)})"
                            : $"{Environment.NewLine}\t{strNodeInnerText} ({LanguageManager.GetString("String_Quality", GlobalOptions.Language)})";
                        return false;
                    }
                case "skill":
                    {
                        string strSpec = xmlNode.SelectSingleNode("spec")?.Value;
                        string strValue = xmlNode.SelectSingleNode("val")?.Value;
                        int intValue = Convert.ToInt32(strValue);
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
                                    strName = objKnowledgeSkill.DisplayNameMethod(GlobalOptions.Language);
                                    if (!string.IsNullOrEmpty(strSpec) && !objCharacter.Improvements.Any(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.DisableSpecializationEffects && objImprovement.UniqueName == objKnowledgeSkill.Name && string.IsNullOrEmpty(objImprovement.Condition) && objImprovement.Enabled))
                                    {
                                        strName += $" ({strSpec})";
                                    }
                                    if (!string.IsNullOrEmpty(strValue))
                                    {
                                        strName += $" {strValue}";
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
                                    objSkill = objCharacter.SkillsSection.GetActiveSkill(strNodeName + " (" + strSpec + ")");
                                if (objSkill != null && (xmlNode.SelectSingleNode("spec") == null || objSkill.Specializations.Any(objSpec => objSpec.Name == strSpec)) && objSkill.TotalBaseRating >= intValue)
                                {
                                    if (blnShowMessage)
                                    {
                                        strName = objSkill.DisplayNameMethod(GlobalOptions.Language);
                                        if (!string.IsNullOrEmpty(strSpec) && !objCharacter.Improvements.Any(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.DisableSpecializationEffects && objImprovement.UniqueName == objSkill.Name && string.IsNullOrEmpty(objImprovement.Condition) && objImprovement.Enabled))
                                        {
                                            strName += $" ({strSpec})";
                                        }
                                        if (!string.IsNullOrEmpty(strValue))
                                        {
                                            strName += $" {strValue}";
                                        }
                                    }
                                    return true;
                                }
                            }
                        }
                        if (blnShowMessage)
                        {
                            XmlDocument xmlSkillDoc = XmlManager.Load("skills.xml");
                            string strSkillName = xmlNode.SelectSingleNode("name")?.Value;
                            string strTranslate = xmlSkillDoc.SelectSingleNode($"/chummer/skills/skill[name = {strSkillName.CleanXPath()}]/translate")?.InnerText ??
                                                    xmlSkillDoc.SelectSingleNode($"/chummer/knowledgeskills/skill[name = {strSkillName.CleanXPath()}]/translate")?.InnerText;
                            strName = !string.IsNullOrEmpty(strTranslate) ? $"{Environment.NewLine}\t{strTranslate}" : $"{Environment.NewLine}\t{xmlNode.SelectSingleNode("name")?.Value}";
                            if (!string.IsNullOrEmpty(strSpec))
                            {
                                strName += $" ({strSpec})";
                            }
                            if (!string.IsNullOrEmpty(strValue))
                            {
                                strName += $" {strValue}";
                            }
                            strName += $" ({LanguageManager.GetString("Tab_Skills", GlobalOptions.Language)})";
                        }
                        return false;
                    }
                case "skillgrouptotal":
                    {
                        // Check if the total combined Ratings of Skill Groups adds up to a particular total.
                        int intTotal = 0;
                        string[] strGroups = xmlNode.SelectSingleNode("skillgroups")?.Value.Split('+');
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
                                            objOutputString.Append(objGroup.DisplayName + ", ");
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
                            strName = objOutputString + $" ({LanguageManager.GetString("String_ExpenseSkillGroup", GlobalOptions.Language)})";
                        }
                        return intTotal >= Convert.ToInt32(xmlNode.SelectSingleNode("val")?.Value);
                    }
                case "spell":
                    {
                        Spell objSpell = objCharacter.Spells.FirstOrDefault(x => x.Name == strNodeInnerText);
                        if (objSpell != null)
                        {
                            if (blnShowMessage)
                                strName = objSpell.DisplayNameShort(GlobalOptions.Language);
                            return true;
                        }
                        if (blnShowMessage)
                        {
                            // Check for a specific Spell.
                            string strTranslate = XmlManager.Load("spells.xml").SelectSingleNode($"/chummer/spells/spell[name = {strNodeInnerText.CleanXPath()}]/translate")?.InnerText;
                            strName = !string.IsNullOrEmpty(strTranslate)
                                ? $"{Environment.NewLine}\t{strTranslate} ({LanguageManager.GetString("String_DescSpell", GlobalOptions.Language)})"
                                : $"{Environment.NewLine}\t{strNodeInnerText} ({LanguageManager.GetString("String_DescSpell", GlobalOptions.Language)})";
                        }
                        return false;
                    }
                case "spellcategory":
                    {
                        // Check for a specified amount of a particular Spell category.
                        if (blnShowMessage)
                        {
                            string strTranslate = XmlManager.Load("spells.xml").SelectSingleNode($"/chummer/categories/category[. = \"{strNodeName}\"]/@translate")?.InnerText;
                            strName = !string.IsNullOrEmpty(strTranslate)
                                ? $"{Environment.NewLine}\t{strTranslate} ({LanguageManager.GetString("String_SpellCategory", GlobalOptions.Language)})"
                                : $"{Environment.NewLine}\t{strNodeInnerText} ({LanguageManager.GetString("String_SpellCategory", GlobalOptions.Language)})";
                        }
                        return objCharacter.Spells.Count(objSpell => objSpell.Category == strNodeName) >= Convert.ToInt32(xmlNode.SelectSingleNode("count")?.Value);
                    }
                case "spelldescriptor":
                    {
                        string strCount = xmlNode.SelectSingleNode("count")?.Value ?? string.Empty;
                        // Check for a specified amount of a particular Spell Descriptor.
                        if (blnShowMessage)
                            strName = Environment.NewLine + '\t' + LanguageManager.GetString("Label_Descriptors", GlobalOptions.Language) + " >= " + strCount;
                        return objCharacter.Spells.Count(objSpell => objSpell.Descriptors.Contains(strNodeName)) >= Convert.ToInt32(strCount);
                    }
                case "streetcredvsnotoriety":
                    {
                        // Street Cred must be higher than Notoriety.
                        if (blnShowMessage)
                            strName = Environment.NewLine + '\t' + LanguageManager.GetString("String_StreetCred", GlobalOptions.Language) + " >= " + LanguageManager.GetString("String_Notoriety", GlobalOptions.Language);
                        return objCharacter.StreetCred >= objCharacter.Notoriety;
                    }
                case "submersiongrade":
                    {
                        // Character's initiate grade must be higher than or equal to the required value.
                        if (blnShowMessage)
                            strName = Environment.NewLine + '\t' + LanguageManager.GetString("String_SubmersionGrade", GlobalOptions.Language) + " >= " + strNodeInnerText;
                        return objCharacter.SubmersionGrade >= Convert.ToInt32(strNodeInnerText);
                    }
                case "tradition":
                    {
                        // Character needs a specific Tradition.
                        if (blnShowMessage)
                        {
                            string strTranslate = XmlManager.Load("traditions.xml").SelectSingleNode($"/chummer/traditions/tradition[name = {strNodeInnerText.CleanXPath()}]/translate")?.InnerText;
                            strName = !string.IsNullOrEmpty(strTranslate)
                                ? $"{Environment.NewLine}\t{strTranslate} ({LanguageManager.GetString("String_Tradition", GlobalOptions.Language)})"
                                : $"{Environment.NewLine}\t{strNodeInnerText} ({LanguageManager.GetString("String_Tradition", GlobalOptions.Language)})";
                        }
                        return objCharacter.MagicTradition.Name == strNodeInnerText;
                    }
                default:
                    Utils.BreakIfDebug();
                    break;
            }
            if (blnShowMessage)
                strName = strNodeInnerText;
            return false;
        }

        public static bool CheckAvailRestriction(XPathNavigator objXmlGear, Character objCharacter, int intRating = 1, int intAvailModifier = 0)
        {
            if (objXmlGear == null)
                return false;
            //TODO: Better handler for restricted gear
            if (objCharacter.Created || objCharacter.RestrictedGear || objCharacter.IgnoreRules)
                return true;
            // Avail.

            XPathNavigator objAvailNode = objXmlGear.SelectSingleNode("avail");
            if (objAvailNode == null)
            {
                int intHighestAvailNode = 0;
                foreach (XPathNavigator objLoopNode in objXmlGear.SelectChildren(XPathNodeType.Element))
                {
                    if (objLoopNode.Name.StartsWith("avail"))
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
            if (strAvailExpr.StartsWith("FixedValues("))
            {
                string[] strValues = strAvailExpr.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
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
                intAvail += Convert.ToInt32(objProcess);
            return intAvail <= objCharacter.MaximumAvailability;
        }

        public static bool CheckNuyenRestriction(XPathNavigator objXmlGear, decimal decMaxNuyen, decimal decCostMultiplier = 1.0m, int intRating = 1)
        {
            // Cost.
            decimal decCost = 0.0m;
            XPathNavigator objCostNode = objXmlGear.SelectSingleNode("cost");
            if (objCostNode == null)
            {
                int intCostRating = 1;
                foreach (XmlNode objLoopNode in objXmlGear.SelectChildren(XPathNodeType.Element))
                {
                    if (objLoopNode.Name.StartsWith("cost"))
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
                if (strCost.StartsWith("FixedValues("))
                {
                    string[] strValues = strCost.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                    strCost = strValues[Math.Max(Math.Min(intRating, strValues.Length) - 1, 0)];
                }
                else if (strCost.StartsWith("Variable"))
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
