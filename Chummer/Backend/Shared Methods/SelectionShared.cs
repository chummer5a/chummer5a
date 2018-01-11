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
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace Chummer.Backend
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
                string strChargenOnly = xmlNode["chargenonly"]?.InnerText;
                if (!string.IsNullOrEmpty(strChargenOnly) && (strChargenOnly != "no" || strChargenOnly == bool.FalseString))
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
            else if (objCharacter.BuildMethod == CharacterBuildMethod.Priority || objCharacter.BuildMethod == CharacterBuildMethod.SumtoTen)
            {
                string strPriorityOnly = xmlNode["onlyprioritygiven"]?.InnerText;
                if (!string.IsNullOrEmpty(strPriorityOnly) && (strPriorityOnly != "no" || strPriorityOnly == bool.FalseString))
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
            if (!blnIgnoreLimit)
            {
                // See if the character already has this Quality and whether or not multiple copies are allowed.
                string strLimitString = string.Empty;
                // If the limit at chargen is different from the actual limit, we need to make sure we fetch the former if the character is in Create mode
                string strChargenLimitString = xmlNode["chargenlimit"]?.InnerText;
                if (!string.IsNullOrWhiteSpace(strChargenLimitString) && !objCharacter.Created)
                    strLimitString = strChargenLimitString;
                else
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
                            strLimitString = "no";
                    }
                }
                if (strLimitString != "no")
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
                    try
                    {
                        strLimitString = CommonFunctions.EvaluateInvariantXPath(objLimitString.ToString()).ToString();
                    }
                    catch (XPathException)
                    {
                        strLimitString = "1";
                    }

                    string limitTitle = LanguageManager.GetString("MessageTitle_SelectGeneric_Limit", GlobalOptions.Language).Replace("{0}", strLocalName);
                    string limitMessage = LanguageManager.GetString("Message_SelectGeneric_Limit", GlobalOptions.Language).Replace("{0}", strLocalName);

                    List<string> lstNamesIncludedInLimit = new List<string>();
                    string strNameNode = xmlNode["name"]?.InnerText;
                    if (!string.IsNullOrEmpty(strNameNode))
                    {
                        lstNamesIncludedInLimit.Add(strNameNode);
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
                                List<MartialArtAdvantage> objTempList = new List<MartialArtAdvantage>(objCharacter.MartialArts.Count);
                                foreach (MartialArt objMartialArt in objCharacter.MartialArts)
                                {
                                    objTempList.AddRange(objMartialArt.Advantages);
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
                        if (blnCheckCyberwareChildren)
                        {
                            if (string.IsNullOrEmpty(strLocation))
                            {
                                intCount = objCharacter.Cyberware.DeepCount(x => x.Children, x => string.IsNullOrEmpty(x.PlugsIntoModularMount) && lstNamesIncludedInLimit.Any(strName => strName == x.Name));
                            }
                            // We only want to check against 'ware that is on the same side as this one
                            else
                            {
                                intCount = objCharacter.Cyberware.DeepCount(x => x.Children, x => string.IsNullOrEmpty(x.PlugsIntoModularMount) && x.Location == strLocation && lstNamesIncludedInLimit.Any(strName => strName == x.Name));
                            }
                        }
                        else
                            intCount = objListToCheck.Count(objItem => lstNamesIncludedInLimit.Any(objLimitName => objLimitName == objItem.Name));
                        intExtendedCount = intCount;
                        // In case one item is split up into multiple entries with different names, e.g. Indomitable quality, we need to be able to check all those entries against the limit
                        XmlNode xmlIncludeInLimit = xmlNode["includeinlimit"];
                        if (xmlIncludeInLimit != null)
                        {
                            foreach (XmlNode objChildXml in xmlIncludeInLimit.ChildNodes)
                            {
                                lstNamesIncludedInLimit.Add(objChildXml.InnerText);
                            }

                            if (blnCheckCyberwareChildren)
                            {
                                if (string.IsNullOrEmpty(strLocation))
                                {
                                    intExtendedCount = objCharacter.Cyberware.DeepCount(x => x.Children, objItem => string.IsNullOrEmpty(objItem.PlugsIntoModularMount) && lstNamesIncludedInLimit.Any(objLimitName => objLimitName == objItem.Name));
                                }
                                // We only want to check against 'ware that is on the same side as this one
                                else
                                {
                                    intExtendedCount = objCharacter.Cyberware.DeepCount(x => x.Children, x => string.IsNullOrEmpty(x.PlugsIntoModularMount) && x.Location == strLocation && lstNamesIncludedInLimit.Any(strName => strName == x.Name));
                                }
                            }
                            else
                                intExtendedCount = objListToCheck.Count(objItem => lstNamesIncludedInLimit.Any(objLimitName => objLimitName == objItem.Name));
                        }
                    }
                    if (intCount >= intLimit || intExtendedCount >= intExtendedLimit)
                    {
                        if (blnShowMessage)
                        {
                            limitMessage = limitMessage.Replace("{1}", intLimit == 0 ? "1" : intLimit.ToString());
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
                    XmlNodeList objXmlOneOfList = objXmlOneOf.ChildNodes;

                    foreach (XmlNode xmlForbiddenItemNode in objXmlOneOfList)
                    {
                        // The character is not allowed to take the Quality, so display a message and uncheck the item.
                        if (xmlForbiddenItemNode.TestNodeRequirements(objCharacter, out string name, strIgnoreQuality))
                        {
                            if (blnShowMessage)
                            {
                                string forbiddenTitle = LanguageManager.GetString("MessageTitle_SelectGeneric_Restriction", GlobalOptions.Language).Replace("{0}", strLocalName);
                                string forbiddenMessage = LanguageManager.GetString("Message_SelectGeneric_Restriction", GlobalOptions.Language).Replace("{0}", strLocalName) + name;
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
                object objRequirementLock = new object();
                bool blnRequirementMet = true;
                object blnRequirementMetLock = new object();

                // Loop through the oneof requirements.
                foreach (XmlNode objXmlOneOf in xmlRequiredNode.SelectNodes("oneof"))
                {
                    bool blnOneOfMet = false;
                    StringBuilder objThisRequirement = new StringBuilder("\n" + LanguageManager.GetString("Message_SelectQuality_OneOf", GlobalOptions.Language));
                    XmlNodeList objXmlOneOfList = objXmlOneOf.ChildNodes;
                    foreach (XmlNode xmlRequiredItemNode in objXmlOneOfList)
                    {
                        if (xmlRequiredItemNode.TestNodeRequirements(objCharacter, out string name, strIgnoreQuality))
                        {
                            blnOneOfMet = true;
                            break;
                        }
                        if (blnShowMessage)
                            objThisRequirement.Append(name);
                    }

                    // Update the flag for requirements met.
                    blnRequirementMet = blnRequirementMet && blnOneOfMet;
                    if (blnShowMessage)
                        objRequirement.Append(objThisRequirement.ToString());
                    else if (!blnOneOfMet)
                        break;
                }

                if (blnRequirementMet || blnShowMessage)
                {
                    // Loop through the allof requirements.
                    foreach (XmlNode objXmlAllOf in xmlRequiredNode.SelectNodes("allof"))
                    {
                        bool blnAllOfMet = true;
                        object blnOneOfMetLock = new object();
                        StringBuilder objThisRequirement = new StringBuilder("\n" + LanguageManager.GetString("Message_SelectQuality_AllOf", GlobalOptions.Language));
                        XmlNodeList objXmlAllOfList = objXmlAllOf.ChildNodes;
                        foreach (XmlNode xmlRequiredItemNode in objXmlAllOfList)
                        {
                            // If this item was not found, fail the AllOfMet condition.
                            if (!xmlRequiredItemNode.TestNodeRequirements(objCharacter, out string name, strIgnoreQuality))
                            {
                                blnAllOfMet = false;
                                if (blnShowMessage)
                                    objThisRequirement.Append(name);
                                else
                                    break;
                            }
                        }

                        // Update the flag for requirements met.
                        blnRequirementMet = blnRequirementMet && blnAllOfMet;
                        if (blnShowMessage)
                            objRequirement.Append(objThisRequirement.ToString());
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
                        string requiredMessage = LanguageManager.GetString("Message_SelectGeneric_Requirement", GlobalOptions.Language).Replace("{0}", strLocalName) + objRequirement.ToString();
                        MessageBox.Show(requiredMessage, requiredTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    return false;
                }
            }
            return true;
        }

        public static bool TestNodeRequirements(this XmlNode xmlNode, Character objCharacter, out string strName, string strIgnoreQuality = "")
        {
            if (xmlNode == null || objCharacter == null)
            {
                strName = string.Empty;
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
                        int intTargetValue = Convert.ToInt32(xmlNode["total"].InnerText);
                        strName = $"\n\t{objAttribute.DisplayAbbrev} {intTargetValue}";
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
                        string strNodeAttributes = xmlNode["attributes"].InnerText;
                        int intNodeVal = Convert.ToInt32(xmlNode["val"].InnerText);
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
                        strName = $"\n\t{strAttributes} {intNodeVal}";
                        return Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(strValue)) >= intNodeVal;
                    }
                case "careerkarma":
                    {
                        // Check Career Karma requirement.
                        strName = "\n\t" + LanguageManager.GetString("Message_SelectQuality_RequireKarma", GlobalOptions.Language).Replace("{0}", strNodeInnerText);
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
                                strName = critterPower.DisplayNameShort(GlobalOptions.Language);
                                return true;
                            }
                        }
                        string strTranslate = XmlManager.Load("critterpowers.xml").SelectSingleNode($"/chummer/powers/power[name = \"{strNodeInnerText}\"]")?["translate"]?.InnerText;
                        if (!string.IsNullOrEmpty(strTranslate))
                            strName = $"\n\t {strTranslate} ({LanguageManager.GetString("Tab_Critter", GlobalOptions.Language)})";
                        else
                            strName = $"\n\t {strNodeInnerText} ({LanguageManager.GetString("Tab_Critter", GlobalOptions.Language)})";
                        return false;
                    }
                case "bioware":
                    {
                        int count = Convert.ToInt32(xmlNode.Attributes?["count"]?.InnerText ?? "1");
                        string strTranslate = XmlManager.Load("bioware.xml").SelectSingleNode($"/chummer/biowares/bioware[name = \"{strNodeInnerText}\"]")?["translate"]?.InnerText;
                        if (!string.IsNullOrEmpty(strTranslate))
                            strName = $"\n\t{LanguageManager.GetString("Label_Bioware", GlobalOptions.Language)} {strTranslate}";
                        else
                            strName = $"\n\t{LanguageManager.GetString("Label_Bioware", GlobalOptions.Language)} {strNodeInnerText}";
                        string strWareNodeSelectAttribute = xmlNode.Attributes?["select"]?.InnerText ?? string.Empty;
                        return objCharacter.Cyberware.DeepCount(x => x.Children, objCyberware => objCyberware.Name == strNodeInnerText &&
                                objCyberware.SourceType == Improvement.ImprovementSource.Bioware && string.IsNullOrEmpty(objCyberware.PlugsIntoModularMount) &&
                               (string.IsNullOrEmpty(strWareNodeSelectAttribute) || strWareNodeSelectAttribute == objCyberware.Extra)) >= count;
                    }
                case "cyberware":
                    {
                        int count = Convert.ToInt32(xmlNode.Attributes?["count"]?.InnerText ?? "1");
                        string strTranslate = XmlManager.Load("cyberware.xml").SelectSingleNode($"/chummer/cyberwares/cyberware[name = \"{strNodeInnerText}\"]")?["translate"]?.InnerText;
                        if (!string.IsNullOrEmpty(strTranslate))
                            strName = $"\n\t{LanguageManager.GetString("Label_Cyberware", GlobalOptions.Language)} {strTranslate}";
                        else
                            strName = $"\n\t{LanguageManager.GetString("Label_Cyberware", GlobalOptions.Language)} {strNodeInnerText}";
                        string strWareNodeSelectAttribute = xmlNode.Attributes?["select"]?.InnerText ?? string.Empty;
                        return objCharacter.Cyberware.DeepCount(x => x.Children, objCyberware => objCyberware.Name == strNodeInnerText &&
                                objCyberware.SourceType == Improvement.ImprovementSource.Cyberware && string.IsNullOrEmpty(objCyberware.PlugsIntoModularMount) &&
                               (string.IsNullOrEmpty(strWareNodeSelectAttribute) || strWareNodeSelectAttribute == objCyberware.Extra)) >= count;
                    }
                case "biowarecontains":
                    {
                        int count = Convert.ToInt32(xmlNode.Attributes?["count"]?.InnerText ?? "1");
                        string strTranslate = XmlManager.Load("bioware.xml").SelectSingleNode($"/chummer/biowares/bioware[name = \"{strNodeInnerText}\"]")?["translate"]?.InnerText;
                        if (!string.IsNullOrEmpty(strTranslate))
                            strName = $"\n\t{LanguageManager.GetString("Label_Bioware", GlobalOptions.Language)} {strTranslate}";
                        else
                            strName = $"\n\t{LanguageManager.GetString("Label_Bioware", GlobalOptions.Language)} {strNodeInnerText}";
                        string strWareNodeSelectAttribute = xmlNode.Attributes?["select"]?.InnerText ?? string.Empty;
                        return objCharacter.Cyberware.DeepCount(x => x.Children, objCyberware => objCyberware.Name.Contains(strNodeInnerText) &&
                                objCyberware.SourceType == Improvement.ImprovementSource.Bioware && string.IsNullOrEmpty(objCyberware.PlugsIntoModularMount) &&
                               (string.IsNullOrEmpty(strWareNodeSelectAttribute) || strWareNodeSelectAttribute == objCyberware.Extra)) >= count;
                    }
                case "cyberwarecontains":
                    {
                        int count = Convert.ToInt32(xmlNode.Attributes?["count"]?.InnerText ?? "1");
                        string strTranslate = XmlManager.Load("cyberware.xml").SelectSingleNode($"/chummer/cyberwares/cyberware[name = \"{strNodeInnerText}\"]")?["translate"]?.InnerText;
                        if (!string.IsNullOrEmpty(strTranslate))
                            strName = $"\n\t{LanguageManager.GetString("Label_Cyberware", GlobalOptions.Language)} {strTranslate}";
                        else
                            strName = $"\n\t{LanguageManager.GetString("Label_Cyberware", GlobalOptions.Language)} {strNodeInnerText}";
                        string strWareNodeSelectAttribute = xmlNode.Attributes?["select"]?.InnerText ?? string.Empty;
                        return objCharacter.Cyberware.DeepCount(x => x.Children, objCyberware => objCyberware.Name.Contains(strNodeInnerText) &&
                                objCyberware.SourceType == Improvement.ImprovementSource.Cyberware && string.IsNullOrEmpty(objCyberware.PlugsIntoModularMount) &&
                               (string.IsNullOrEmpty(strWareNodeSelectAttribute) || strWareNodeSelectAttribute == objCyberware.Extra)) >= count;
                    }
                case "damageresistance":
                    {
                        // Damage Resistance must be a particular value.
                        strName = $"\n\t{LanguageManager.GetString("String_DamageResistance", GlobalOptions.Language)}";
                        return objCharacter.BOD.TotalValue + ImprovementManager.ValueOf(objCharacter, Improvement.ImprovementType.DamageResistance) >= Convert.ToInt32(strNodeInnerText);
                    }
                case "ess":
                    {
                        string objEssNodeGradeAttributeText = xmlNode.Attributes?["grade"]?.InnerText ?? string.Empty;
                        if (!string.IsNullOrEmpty(objEssNodeGradeAttributeText))
                        {
                            decimal decGrade =
                                objCharacter.Cyberware.Where(
                                        objCyberware =>
                                            objCyberware.Grade.Name.Contains(objEssNodeGradeAttributeText))
                                    .AsParallel().Sum(objCyberware => objCyberware.CalculatedESS());
                            if (strNodeInnerText.StartsWith('-'))
                            {
                                // Essence must be less than the value.
                                strName = "\n\t" +
                                       LanguageManager.GetString("Message_SelectQuality_RequireESSGradeBelow", GlobalOptions.Language)
                                           .Replace("{0}", strNodeInnerText)
                                           .Replace("{1}", objEssNodeGradeAttributeText)
                                           .Replace("{2}", decGrade.ToString(CultureInfo.InvariantCulture));
                                return decGrade < Convert.ToDecimal(strNodeInnerText.TrimStart('-'), GlobalOptions.InvariantCultureInfo);
                            }
                            // Essence must be equal to or greater than the value.
                            strName = "\n\t" +
                                   LanguageManager.GetString("Message_SelectQuality_RequireESSGradeAbove", GlobalOptions.Language)
                                       .Replace("{0}", strNodeInnerText)
                                       .Replace("{1}", objEssNodeGradeAttributeText)
                                       .Replace("{2}", decGrade.ToString(CultureInfo.InvariantCulture));
                            return decGrade >= Convert.ToDecimal(strNodeInnerText, GlobalOptions.InvariantCultureInfo);
                        }
                        // Check Essence requirement.
                        if (strNodeInnerText.StartsWith('-'))
                        {
                            // Essence must be less than the value.
                            strName = "\n\t" +
                                   LanguageManager.GetString("Message_SelectQuality_RequireESSBelow", GlobalOptions.Language)
                                       .Replace("{0}", strNodeInnerText)
                                       .Replace("{1}", objCharacter.Essence.ToString(CultureInfo.InvariantCulture));
                            return objCharacter.Essence < Convert.ToDecimal(strNodeInnerText.TrimStart('-'), GlobalOptions.InvariantCultureInfo);
                        }
                        // Essence must be equal to or greater than the value.
                        strName = "\n\t" +
                               LanguageManager.GetString("Message_SelectQuality_RequireESSAbove", GlobalOptions.Language)
                                   .Replace("{0}", strNodeInnerText)
                                   .Replace("{1}", objCharacter.Essence.ToString(CultureInfo.InvariantCulture));
                        return objCharacter.Essence >= Convert.ToDecimal(strNodeInnerText, GlobalOptions.InvariantCultureInfo);
                    }
                case "echo":
                    {
                        Metamagic objMetamagic = objCharacter.Metamagics.FirstOrDefault(x => x.Name == strNodeInnerText && x.SourceType == Improvement.ImprovementSource.Echo);
                        if (objMetamagic != null)
                        {
                            strName = objMetamagic.DisplayNameShort(GlobalOptions.Language);
                            return true;
                        }
                        string strTranslate = XmlManager.Load("echoes.xml").SelectSingleNode($"/chummer/echoes/echo[name = \"{strNodeInnerText}\"]")?["translate"]?.InnerText;
                        if (!string.IsNullOrEmpty(strTranslate))
                            strName = $"\n\t {strTranslate} ({LanguageManager.GetString("String_Echo", GlobalOptions.Language)})";
                        else
                            strName = $"\n\t {strNodeInnerText} ({LanguageManager.GetString("String_Echo", GlobalOptions.Language)})";
                        return false;
                    }
                case "group":
                    {
                        // Check that clustered options are present (Magical Tradition + Skill 6, for example)
                        string strResult = string.Empty;
                        foreach (XmlNode xmlChildNode in xmlNode.ChildNodes)
                        {
                            if (!xmlChildNode.TestNodeRequirements(objCharacter, out string result, strIgnoreQuality))
                            {
                                strResult = result;
                                break;
                            }
                        }
                        strName = strResult;
                        return string.IsNullOrEmpty(strResult);
                    }
                case "initiategrade":
                    {
                        // Character's initiate grade must be higher than or equal to the required value.
                        strName = "\n\t" + LanguageManager.GetString("String_InitiateGrade", GlobalOptions.Language) + " >= " + strNodeInnerText;
                        return objCharacter.InitiateGrade >= Convert.ToInt32(strNodeInnerText);
                    }
                case "martialart":
                    {
                        MartialArt objMartialArt = objCharacter.MartialArts.FirstOrDefault(x => x.Name == strNodeInnerText);
                        if (objMartialArt != null)
                        {
                            strName = objMartialArt.DisplayNameShort(GlobalOptions.Language);
                            return true;
                        }
                        // Character needs a specific Martial Art.
                        string strTranslate = XmlManager.Load("martialarts.xml").SelectSingleNode($"/chummer/martialarts/martialart[name = \"{strNodeInnerText}\"]")?["translate"]?.InnerText;
                        if (!string.IsNullOrEmpty(strTranslate))
                            strName = $"\n\t {strTranslate} ({LanguageManager.GetString("String_MartialArt", GlobalOptions.Language)})";
                        else
                            strName = $"\n\t {strNodeInnerText} ({LanguageManager.GetString("String_MartialArt", GlobalOptions.Language)})";
                        return false;
                    }
                case "martialtechnique":
                    {
                        foreach (MartialArt objMartialArt in objCharacter.MartialArts)
                        {
                            MartialArtAdvantage objMartialArtAdvantage = objMartialArt.Advantages.FirstOrDefault(x => x.Name == strNodeInnerText);
                            if (objMartialArtAdvantage != null)
                            {
                                strName = objMartialArtAdvantage.DisplayName(GlobalOptions.Language);
                                return true;
                            }
                        }
                        // Character needs a specific Martial Arts technique.
                        string strTranslate = XmlManager.Load("martialarts.xml").SelectSingleNode($"/chummer/techniques/technique[name = \"{strNodeInnerText}\"]")?["translate"]?.InnerText;
                        if (!string.IsNullOrEmpty(strTranslate))
                            strName = $"\n\t {strTranslate} ({LanguageManager.GetString("String_MartialArt", GlobalOptions.Language)})";
                        else
                            strName = $"\n\t {strNodeInnerText} ({LanguageManager.GetString("String_MartialArt", GlobalOptions.Language)})";
                        return false;
                    }
                case "metamagic":
                    {
                        Metamagic objMetamagic = objCharacter.Metamagics.FirstOrDefault(x => x.Name == strNodeInnerText && x.SourceType == Improvement.ImprovementSource.Metamagic);
                        if (objMetamagic != null)
                        {
                            strName = objMetamagic.DisplayNameShort(GlobalOptions.Language);
                            return true;
                        }
                        string strTranslate = XmlManager.Load("metamagic.xml").SelectSingleNode($"/chummer/metamagics/metamagic[name = \"{strNodeInnerText}\"]")?["translate"]?.InnerText;
                        if (!string.IsNullOrEmpty(strTranslate))
                            strName = $"\n\t {strTranslate} ({LanguageManager.GetString("String_Metamagic", GlobalOptions.Language)})";
                        else
                            strName = $"\n\t {strNodeInnerText} ({LanguageManager.GetString("String_Metamagic", GlobalOptions.Language)})";
                        return false;
                    }
                case "metamagicart":
                case "art":
                    {
                        if (objCharacter.Options.IgnoreArt)
                        {
                            XmlDocument xmlMetamagicDoc = XmlManager.Load("metamagic.xml");
                            string strTranslateArt = xmlMetamagicDoc.SelectSingleNode($"/chummer/arts/art[name = \"{strNodeInnerText}\"]")?["translate"]?.InnerText;
                            if (!string.IsNullOrEmpty(strTranslateArt))
                                strName = $"\n\t {strTranslateArt} ({LanguageManager.GetString("String_Art", GlobalOptions.Language)})";
                            else
                                strName = $"\n\t {strNodeInnerText} ({LanguageManager.GetString("String_Art", GlobalOptions.Language)})";
                            if (xmlNode.Name == "art")
                            {
                                return true;
                            }
                            foreach (Metamagic metamagic in objCharacter.Metamagics)
                            {
                                XmlNode xmlMetamagicNode = xmlMetamagicDoc.SelectSingleNode($"/chummer/metamagics/metamagic[name = \"{metamagic.Name}\"]");
                                if (xmlMetamagicNode != null)
                                {
                                    if (xmlMetamagicNode?.SelectSingleNode($"required/art[text() = \"{strNodeInnerText}\"]") != null)
                                    {
                                        return true;
                                    }
                                    if (xmlMetamagicNode?.SelectSingleNode($"forbidden/art[text() = \"{strNodeInnerText}\"]") != null)
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
                                strName = objArt.DisplayNameShort(GlobalOptions.Language);
                                return true;
                            }
                        }
                        string strTranslate = XmlManager.Load("metamagic.xml").SelectSingleNode($"/chummer/arts/art[name = \"{strNodeInnerText}\"]")?["translate"]?.InnerText;
                        if (!string.IsNullOrEmpty(strTranslate))
                            strName = $"\n\t {strTranslate} ({LanguageManager.GetString("String_Art", GlobalOptions.Language)})";
                        else
                            strName = $"\n\t {strNodeInnerText} ({LanguageManager.GetString("String_Art", GlobalOptions.Language)})";
                        return false;
                    }
                case "metatype":
                    {
                        string strXPathFilter = $"/chummer/metatypes/metatype[name = \"{strNodeInnerText}\"]";
                        // Check the Metatype restriction.
                        string strTranslate = XmlManager.Load("metatypes.xml").SelectSingleNode(strXPathFilter)?["translate"]?.InnerText ??
                                                XmlManager.Load("critters.xml").SelectSingleNode(strXPathFilter)?["translate"]?.InnerText;
                        if (!string.IsNullOrEmpty(strTranslate))
                            strName = $"\n\t {strTranslate} ({LanguageManager.GetString("String_Metatype", GlobalOptions.Language)})";
                        else
                            strName = $"\n\t {strNodeInnerText} ({LanguageManager.GetString("String_Metatype", GlobalOptions.Language)})";
                        return strNodeInnerText == objCharacter.Metatype;
                    }
                case "metatypecategory":
                    {
                        string strXPathFilter = $"/chummer/categories/category[. = \"{strNodeInnerText}\"]";
                        // Check the Metatype Category restriction.
                        string strTranslate = XmlManager.Load("metatypes.xml").SelectSingleNode(strXPathFilter)?.Attributes["translate"]?.InnerText ??
                                                XmlManager.Load("critters.xml").SelectSingleNode(strXPathFilter)?.Attributes["translate"]?.InnerText;
                        if (!string.IsNullOrEmpty(strTranslate))
                            strName = $"\n\t {strTranslate} ({LanguageManager.GetString("String_MetatypeCategory", GlobalOptions.Language)})";
                        else
                            strName = $"\n\t {strNodeInnerText} ({LanguageManager.GetString("String_MetatypeCategory", GlobalOptions.Language)})";
                        return strNodeInnerText == objCharacter.MetatypeCategory;
                    }
                case "metavariant":
                    {
                        string strXPathFilter = $"/chummer/metatypes/metatype/metavariants/metavariant[name = \"{strNodeInnerText}\"]";
                        // Check the Metavariant restriction.
                        string strTranslate = XmlManager.Load("metatypes.xml").SelectSingleNode(strXPathFilter)?["translate"]?.InnerText ??
                                                XmlManager.Load("critters.xml").SelectSingleNode(strXPathFilter)?["translate"]?.InnerText;
                        if (!string.IsNullOrEmpty(strTranslate))
                            strName = $"\n\t {strTranslate} ({LanguageManager.GetString("String_Metavariant", GlobalOptions.Language)})";
                        else
                            strName = $"\n\t {strNodeInnerText} ({LanguageManager.GetString("String_Metavariant", GlobalOptions.Language)})";
                        return strNodeInnerText == objCharacter.Metavariant;
                    }
                case "power":
                    {
                        // Run through all of the Powers the character has and see if the current required item exists.
                        Power power = objCharacter.Powers.FirstOrDefault(p => p.Name == strNodeInnerText);
                        if (power != null)
                        {
                            strName = power.DisplayNameShort(GlobalOptions.Language);
                            return true;
                        }
                        string strTranslate = XmlManager.Load("powers.xml").SelectSingleNode($"/chummer/powers/power[name = \"{strNodeInnerText}\"]")?["translate"]?.InnerText;
                        if (!string.IsNullOrEmpty(strTranslate))
                            strName = $"\n\t {strTranslate} ({LanguageManager.GetString("Tab_Adept", GlobalOptions.Language)})";
                        else
                            strName = $"\n\t {strNodeInnerText} ({LanguageManager.GetString("Tab_Adept", GlobalOptions.Language)})";
                        return false;
                    }
                case "quality":
                    {
                        Quality quality = objCharacter.Qualities.FirstOrDefault(q => q.Name == strNodeInnerText && q.Name != strIgnoreQuality);
                        if (quality != null)
                        {
                            strName = quality.DisplayNameShort(GlobalOptions.Language);
                            return true;
                        }
                        string strTranslate = XmlManager.Load("qualities.xml").SelectSingleNode($"/chummer/qualities/quality[name = \"{strNodeInnerText}\"]")?["translate"]?.InnerText;
                        if (!string.IsNullOrEmpty(strTranslate))
                            strName = $"\n\t {strTranslate} ({LanguageManager.GetString("String_Quality", GlobalOptions.Language)})";
                        else
                            strName = $"\n\t {strNodeInnerText} ({LanguageManager.GetString("String_Quality", GlobalOptions.Language)})";
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
                                strName = objKnowledgeSkill.DisplayNameMethod(GlobalOptions.Language);
                                if (!string.IsNullOrEmpty(strSpec) && !objCharacter.Improvements.Any(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.DisableSpecializationEffects && objImprovement.UniqueName == objKnowledgeSkill.Name && string.IsNullOrEmpty(objImprovement.Condition)))
                                {
                                    strName += $" ({strSpec})";
                                }
                                if (!string.IsNullOrEmpty(strValue))
                                {
                                    strName += $" {strValue}";
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
                                    strName = objSkill.DisplayNameMethod(GlobalOptions.Language);
                                    if (!string.IsNullOrEmpty(strSpec) && !objCharacter.Improvements.Any(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.DisableSpecializationEffects && objImprovement.UniqueName == objSkill.Name && string.IsNullOrEmpty(objImprovement.Condition)))
                                    {
                                        strName += $" ({strSpec})";
                                    }
                                    if (!string.IsNullOrEmpty(strValue))
                                    {
                                        strName += $" {strValue}";
                                    }
                                    return true;
                                }
                            }
                        }
                        XmlDocument xmlSkillDoc = XmlManager.Load("skills.xml");
                        string strTranslate = xmlSkillDoc?.SelectSingleNode($"/chummer/skills/skill[name = \"{strNodeInnerText}\"]")?["translate"]?.InnerText ??
                                                xmlSkillDoc?.SelectSingleNode($"/chummer/knowledgeskills/skill[name = \"{strNodeInnerText}\"]")?["translate"]?.InnerText;
                        if (!string.IsNullOrEmpty(strTranslate))
                            strName = $"\n\t {strTranslate}";
                        else
                            strName = $"\n\t {strNodeInnerText}";
                        if (!string.IsNullOrEmpty(strSpec))
                        {
                            strName += $" ({strSpec})";
                        }
                        if (!string.IsNullOrEmpty(strValue))
                        {
                            strName += $" {strValue}";
                        }
                        strName += $" ({LanguageManager.GetString("Tab_Skills", GlobalOptions.Language)})";
                        return false;
                    }
                case "skillgrouptotal":
                    {
                        // Check if the total combined Ratings of Skill Groups adds up to a particular total.
                        int intTotal = 0;
                        string[] strGroups = xmlNode["skillgroups"].InnerText.Split('+');
                        StringBuilder objOutputString = new StringBuilder("\n\t");
                        for (int i = 0; i <= strGroups.Length - 1; ++i)
                        {
                            foreach (SkillGroup objGroup in objCharacter.SkillsSection.SkillGroups)
                            {
                                if (objGroup.Name == strGroups[i])
                                {
                                    objOutputString.Append(objGroup.DisplayName + ", ");
                                    intTotal += objGroup.Rating;
                                    break;
                                }
                            }
                        }
                        if (objOutputString.Length > 0)
                            objOutputString.Length -= 2;
                        strName = objOutputString.ToString() + $" ({LanguageManager.GetString("String_ExpenseSkillGroup", GlobalOptions.Language)})";
                        return intTotal >= Convert.ToInt32(xmlNode["val"].InnerText);
                    }
                case "spell":
                    {
                        Spell objSpell = objCharacter.Spells.FirstOrDefault(x => x.Name == strNodeInnerText);
                        if (objSpell != null)
                        {
                            strName = objSpell.DisplayNameShort(GlobalOptions.Language);
                            return true;
                        }
                        // Check for a specific Spell.
                        string strTranslate = XmlManager.Load("spells.xml").SelectSingleNode($"/chummer/spells/spell[name = \"{strNodeInnerText}\"]")?["translate"]?.InnerText;
                        if (!string.IsNullOrEmpty(strTranslate))
                            strName = $"\n\t {strTranslate} ({LanguageManager.GetString("String_DescSpell", GlobalOptions.Language)})";
                        else
                            strName = $"\n\t {strNodeInnerText} ({LanguageManager.GetString("String_DescSpell", GlobalOptions.Language)})";
                        return false;
                    }
                case "spellcategory":
                    {
                        // Check for a specified amount of a particular Spell category.
                        string strTranslate = XmlManager.Load("spells.xml").SelectSingleNode($"/chummer/categories/category[. = \"{strNodeName}\"]")?.Attributes["translate"]?.InnerText;
                        if (!string.IsNullOrEmpty(strTranslate))
                            strName = $"\n\t {strTranslate} ({LanguageManager.GetString("String_SpellCategory", GlobalOptions.Language)})";
                        else
                            strName = $"\n\t {strNodeInnerText} ({LanguageManager.GetString("String_SpellCategory", GlobalOptions.Language)})";
                        return objCharacter.Spells.Count(objSpell => objSpell.Category == strNodeName) >= Convert.ToInt32(xmlNode["count"].InnerText);
                    }
                case "spelldescriptor":
                    {
                        string strCount = xmlNode["count"].InnerText;
                        // Check for a specified amount of a particular Spell Descriptor.
                        strName = "\n\t" + LanguageManager.GetString("Label_Descriptors", GlobalOptions.Language) + " >= " + strCount;
                        return objCharacter.Spells.Count(objSpell => objSpell.Descriptors.Contains(strNodeName)) >= Convert.ToInt32(strCount);
                    }
                case "streetcredvsnotoriety":
                    {
                        // Street Cred must be higher than Notoriety.
                        strName = "\n\t" + LanguageManager.GetString("String_StreetCred", GlobalOptions.Language) + " >= " + LanguageManager.GetString("String_Notoriety", GlobalOptions.Language);
                        return objCharacter.StreetCred >= objCharacter.Notoriety;
                    }
                case "submersiongrade":
                    {
                        // Character's initiate grade must be higher than or equal to the required value.
                        strName = "\n\t" + LanguageManager.GetString("String_SubmersionGrade", GlobalOptions.Language) + " >= " + strNodeInnerText;
                        return objCharacter.SubmersionGrade >= Convert.ToInt32(strNodeInnerText);
                    }
                case "tradition":
                    {
                        // Character needs a specific Tradition.
                        string strTranslate = XmlManager.Load("traditions.xml").SelectSingleNode($"/chummer/traditions/tradition[name = \"{strNodeInnerText}\"]")?["translate"]?.InnerText;
                        if (!string.IsNullOrEmpty(strTranslate))
                            strName = $"\n\t {strTranslate} ({LanguageManager.GetString("String_Tradition", GlobalOptions.Language)})";
                        else
                            strName = $"\n\t {strNodeInnerText} ({LanguageManager.GetString("String_Tradition", GlobalOptions.Language)})";
                        return objCharacter.MagicTradition == strNodeInnerText;
                    }
                default:
                    Utils.BreakIfDebug();
                    break;
            }
            strName = strNodeInnerText;
            return false;
        }
        
        /// <summary>
        ///     Evaluates the availability of a given node against Availability Limits in Create Mode
        /// </summary>
        /// <param name="objXmlGear"></param>
        /// <param name="objCharacter"></param>
        /// <param name="blnHide"></param>
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
            // If avail contains "F" or "R", remove it from the string so we can use the expression.
            string strAvailExpr = objXmlGear["avail"]?.InnerText ?? string.Empty;
            if (strAvailExpr.StartsWith("FixedValues("))
            {
                string[] strValues = strAvailExpr.TrimStart("FixedValues(", true).TrimEnd(')').Split(',');
                strAvailExpr = strValues[Math.Max(Math.Min(intRating - 1, strValues.Length - 1), 0)];
            }

            if (string.IsNullOrEmpty(strAvailExpr))
                return true;
            if (strAvailExpr[0] == '+')
                return true;

            strAvailExpr = strAvailExpr.TrimEnd('F', 'R');
            int intAvail = intAvailModifier;
            try
            {
                intAvail += Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(strAvailExpr.Replace("Rating", intRating.ToString(GlobalOptions.InvariantCultureInfo))));
            }
            catch (XPathException)
            {
            }
            return intAvail <= objCharacter.MaximumAvailability;
        }

        private static readonly char[] lstBracketChars = { '[', ']' };
        public static bool CheckNuyenRestriction(XmlNode objXmlGear, Character objCharacter, decimal decMaxNuyen, decimal decCostMultiplier = 1.0m)
        {
            // Cost.
            decimal decCost = 0.0m;
            XmlNode objCostNode = objXmlGear["cost"];
            int intRating = 1;
            if (objCostNode == null)
            {
                intRating = int.MaxValue;
                foreach (XmlNode objLoopNode in objXmlGear.ChildNodes)
                {
                    if (objLoopNode.NodeType == XmlNodeType.Element && objLoopNode.Name.StartsWith("cost"))
                    {
                        string strLoopCostString = objLoopNode.Name.Substring(4);
                        if (int.TryParse(strLoopCostString, out int intTmp))
                        {
                            intRating = Math.Min(intRating, intTmp);
                        }
                    }
                }
                objCostNode = objXmlGear["cost" + intRating.ToString(GlobalOptions.InvariantCultureInfo)];
            }
            if (objCostNode != null)
            {
                try
                {
                    decCost = Convert.ToDecimal(CommonFunctions.EvaluateInvariantXPath(objCostNode.InnerText.Replace("Rating", intRating.ToString(GlobalOptions.InvariantCultureInfo))), GlobalOptions.InvariantCultureInfo);
                }
                catch (XPathException)
                {
                    if (decimal.TryParse(objCostNode.InnerText, NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out decimal decTemp))
                    {
                        decCost = decTemp;
                    }
                }

                if (objCostNode.InnerText.StartsWith("FixedValues("))
                {
                    string[] strValues = objCostNode.InnerText.TrimStart("FixedValues(", true).TrimEnd(')').Split(',');
                    decCost = Convert.ToDecimal(strValues[0].Trim(lstBracketChars), GlobalOptions.InvariantCultureInfo);
                }
                else if (objCostNode.InnerText.StartsWith("Variable"))
                {
                    decimal decMin = 0;
                    string strCost = objCostNode.InnerText.TrimStart("Variable(", true).TrimEnd(')');
                    if (strCost.Contains('-'))
                    {
                        string[] strValues = strCost.Split('-');
                        decMin = Convert.ToDecimal(strValues[0], GlobalOptions.InvariantCultureInfo);
                    }
                    else
                        decMin = Convert.ToDecimal(strCost.FastEscape('+'), GlobalOptions.InvariantCultureInfo);

                    decCost = decMin;
                }
            }
            return decMaxNuyen >= decCost * decCostMultiplier;
        }
    }
}
