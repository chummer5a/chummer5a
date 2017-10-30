using Chummer.Backend.Attributes;
using Chummer.Backend.Equipment;
using Chummer.Backend.Extensions;
using Chummer.Skills;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace Chummer.Backend.Shared_Methods
{
    internal class SelectionShared
    {
        //TODO: Might be a better location for this; Class names are screwy.
        /// <summary>
        ///     Evaluates requirements of a given node against a given Character object.
        /// </summary>
        /// <param name="objXmlNode">XmlNode of the object.</param>
        /// <param name="blnShowMessage">Should warning messages about whether the object has failed to validate be shown?</param>
        /// <param name="objCharacter">Character Object.</param>
        /// <param name="objQualityDocument"></param>
        /// <param name="strIgnoreQuality">
        ///     Name of a Quality that should be ignored. Typically used when swapping Qualities in
        ///     career mode.
        /// </param>
        /// <param name="strLocalName"></param>
        /// <param name="objMetatypeDocument"></param>
        /// <param name="objCritterDocument"></param>
        /// <param name="strSourceName">Name of the improvement that called this (if it was called by an improvement adding it)</param>
        /// <returns></returns>
        public static bool RequirementsMet(XmlNode objXmlNode, bool blnShowMessage, Character objCharacter, XmlDocument objMetatypeDocument = null, XmlDocument objCritterDocument = null, XmlDocument objQualityDocument = null, string strIgnoreQuality = "", string strLocalName = "", string strSourceName = "", string strLocation = "")
        {
            // Ignore the rules.
            if (objCharacter.IgnoreRules)
                return true;
            if (objXmlNode == null)
                return false;
            if (objMetatypeDocument == null)
                objMetatypeDocument = XmlManager.Load("metatypes.xml");
            if (objCritterDocument == null)
                objCritterDocument = XmlManager.Load("critters.xml");
            if (objQualityDocument == null)
                objQualityDocument = XmlManager.Load("qualities.xml");
            // See if the character is in career mode but would want to add a chargen-only Quality
            if (objCharacter.Created)
            {
                if (objXmlNode["chargenonly"] != null && objXmlNode["chargenonly"]?.InnerText != "no")
                {
                    if (blnShowMessage)
                    {
                        MessageBox.Show(LanguageManager.GetString("Message_SelectGeneric_ChargenRestriction").Replace("{0}", strLocalName),
                            LanguageManager.GetString("MessageTitle_SelectGeneric_Restriction").Replace("{0}", strLocalName),
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    return false;
                }
            }
            // See if the character is using priority-based gen and is trying to add a Quality that can only be added through priorities
            else if ((objCharacter.BuildMethod == CharacterBuildMethod.Priority || objCharacter.BuildMethod == CharacterBuildMethod.SumtoTen) && objXmlNode["onlyprioritygiven"] != null && objXmlNode["onlyprioritygiven"]?.InnerText != "no")
            {
                if (blnShowMessage)
                {
                    MessageBox.Show(LanguageManager.GetString("MessageTitle_SelectGeneric_PriorityRestriction").Replace("{0}", strLocalName),
                        LanguageManager.GetString("MessageTitle_SelectGeneric_Restriction").Replace("{0}", strLocalName),
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return false;
            }
            // See if the character already has this Quality and whether or not multiple copies are allowed.
            string strLimitString = string.Empty;
            // If the limit at chargen is different from the actual limit, we need to make sure we fetch the former if the character is in Create mode
            string strChargenLimitString = objXmlNode["chargenlimit"]?.InnerText;
            if (!string.IsNullOrWhiteSpace(strChargenLimitString) && !objCharacter.Created)
                strLimitString = strChargenLimitString;
            else
            {
                strLimitString = objXmlNode["limit"]?.InnerText;
                // Default case is each quality can only be taken once
                if (string.IsNullOrWhiteSpace(strLimitString))
                {
                    if (objXmlNode.Name == "quality" || objXmlNode.Name == "cyberware" || objXmlNode.Name == "bioware")
                        strLimitString = "1";
                    else
                        strLimitString = "no";
                }
            }
            if (strLimitString != "no")
            {
                XmlDocument objDummyDoc = new XmlDocument();
                XPathNavigator objNavigator = objDummyDoc.CreateNavigator();
                foreach (string strAttribute in AttributeSection.AttributeStrings)
                {
                    CharacterAttrib objLoopAttribute = objCharacter.GetAttribute(strAttribute);
                    strLimitString = strLimitString.Replace("{" + strAttribute + "}", objLoopAttribute.TotalValue.ToString());
                    strLimitString = strLimitString.Replace("{" + strAttribute + "Base}", objLoopAttribute.TotalBase.ToString());
                }
                foreach (string strLimb in Character.LimbStrings)
                {
                    int objLoopLimbCount = objCharacter.LimbCount(strLimb);
                    if (!string.IsNullOrEmpty(strLocation))
                        objLoopLimbCount /= 2;
                    strLimitString = strLimitString.Replace("{" + strLimb + "}", objLoopLimbCount.ToString());
                }
                try
                {
                    XPathExpression objExpression = objNavigator.Compile(strLimitString);
                    strLimitString = objNavigator.Evaluate(objExpression).ToString();
                }
                catch (XPathException)
                {
                    strLimitString = "1";
                }

                string limitTitle = LanguageManager.GetString("MessageTitle_SelectGeneric_Limit").Replace("{0}", strLocalName);
                string limitMessage = LanguageManager.GetString("Message_SelectGeneric_Limit").Replace("{0}", strLocalName);
                
                List<string> lstNamesIncludedInLimit = new List<string>();
                if (objXmlNode["name"] != null)
                {
                    lstNamesIncludedInLimit.Add(objXmlNode["name"].InnerText);
                }

                // We could set this to a list immediately, but I'd rather the pointer start at null so that no list ends up getting selected for the "default" case below
                IEnumerable<INamedItem> objListToCheck = null;
                bool blnCheckCyberwareChildren = false;
                switch (objXmlNode.Name)
                {
                    case "quality":
                        {
                            objListToCheck = objCharacter.Qualities.Where(objQuality => objQuality.SourceName == strSourceName);
                            break;
                        }
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
                if (objXmlNode["limitwithinclusions"] != null)
                {
                    intExtendedLimit = Convert.ToInt32(objXmlNode["limitwithinclusions"].InnerText);
                }
                int intCount = 0;
                int intExtendedCount = 0;
                if (objListToCheck != null || blnCheckCyberwareChildren)
                {
                    if (blnCheckCyberwareChildren)
                    {
                        if (string.IsNullOrEmpty(strLocation))
                        {
                            intCount = objCharacter.Cyberware.DeepCount(x => x.Children.Where(y => string.IsNullOrEmpty(y.PlugsIntoModularMount)), x => x.Name != strIgnoreQuality && lstNamesIncludedInLimit.Any(strName => strName == x.Name));
                        }
                        // We only want to check against 'ware that is on the same side as this one
                        else
                        {
                            intCount = objCharacter.Cyberware.DeepCount(x => x.Children.Where(y => string.IsNullOrEmpty(y.PlugsIntoModularMount)), x => x.Name != strIgnoreQuality && x.Location == strLocation && lstNamesIncludedInLimit.Any(strName => strName == x.Name));
                        }
                    }
                    else
                        intCount = objListToCheck.Count(objItem => objItem.Name != strIgnoreQuality && lstNamesIncludedInLimit.Any(objLimitName => objLimitName == objItem.Name));
                    intExtendedCount = intCount;
                    // In case one item is split up into multiple entries with different names, e.g. Indomitable quality, we need to be able to check all those entries against the limit
                    if (objXmlNode["includeinlimit"] != null)
                    {
                        foreach (XmlNode objChildXml in objXmlNode["includeinlimit"].ChildNodes)
                        {
                            lstNamesIncludedInLimit.Add(objChildXml.InnerText);
                        }

                        if (blnCheckCyberwareChildren)
                        {
                            if (string.IsNullOrEmpty(strLocation))
                            {
                                intExtendedCount = objCharacter.Cyberware.DeepCount(x => x.Children.Where(y => string.IsNullOrEmpty(y.PlugsIntoModularMount)), objItem => objItem.Name != strIgnoreQuality && lstNamesIncludedInLimit.Any(objLimitName => objLimitName == objItem.Name));
                            }
                            // We only want to check against 'ware that is on the same side as this one
                            else
                            {
                                intExtendedCount = objCharacter.Cyberware.DeepCount(x => x.Children.Where(y => string.IsNullOrEmpty(y.PlugsIntoModularMount)), x => x.Name != strIgnoreQuality && x.Location == strLocation && lstNamesIncludedInLimit.Any(strName => strName == x.Name));
                            }
                        }
                        else
                            intExtendedCount = objListToCheck.Count(objItem => objItem.Name != strIgnoreQuality && lstNamesIncludedInLimit.Any(objLimitName => objLimitName == objItem.Name));
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

            if (objXmlNode.InnerXml.Contains("forbidden"))
            {
                string forbiddenTitle = LanguageManager.GetString("MessageTitle_SelectGeneric_Restriction").Replace("{0}", strLocalName);
                string forbiddenMessage = LanguageManager.GetString("Message_SelectGeneric_Restriction").Replace("{0}", strLocalName);
                // Loop through the oneof requirements.
                XmlNodeList objXmlForbiddenList = objXmlNode.SelectNodes("forbidden/oneof");
                if (objXmlForbiddenList != null)
                    foreach (XmlNode objXmlOneOf in objXmlForbiddenList)
                    {
                        XmlNodeList objXmlOneOfList = objXmlOneOf.ChildNodes;

                        foreach (XmlNode objXmlForbidden in objXmlOneOfList)
                        {
                            // The character is not allowed to take the Quality, so display a message and uncheck the item.
                            if (TestNodeRequirements(objXmlForbidden, objCharacter, out string name, strIgnoreQuality, objMetatypeDocument,
                                objCritterDocument, objQualityDocument))
                            {
                                if (blnShowMessage)
                                    MessageBox.Show(
                                        forbiddenMessage + name, forbiddenTitle,
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return false;
                            }
                        }
                    }
            }

            if (objXmlNode.InnerXml.Contains("required"))
            {
                string requiredTitle = LanguageManager.GetString("MessageTitle_SelectGeneric_Requirement").Replace("{0}", strLocalName);
                string requiredMessage = LanguageManager.GetString("Message_SelectGeneric_Requirement").Replace("{0}", strLocalName);
                string strRequirement = string.Empty;
                bool blnRequirementMet = true;

                // Loop through the oneof requirements.
                XmlNodeList objXmlRequiredList = objXmlNode.SelectNodes("required/oneof");
                foreach (XmlNode objXmlOneOf in objXmlRequiredList)
                {
                    bool blnOneOfMet = false;
                    string strThisRequirement = "\n" +
                                                LanguageManager.GetString("Message_SelectQuality_OneOf");
                    XmlNodeList objXmlOneOfList = objXmlOneOf.ChildNodes;
                    foreach (XmlNode objXmlRequired in objXmlOneOfList)
                    {
                        blnOneOfMet = TestNodeRequirements(objXmlRequired, objCharacter, out string name, strIgnoreQuality,
                            objMetatypeDocument,
                            objCritterDocument, objQualityDocument);

                        if (blnOneOfMet)
                            break;
                        strThisRequirement += name;
                    }

                    // Update the flag for requirements met.
                    blnRequirementMet = blnRequirementMet && blnOneOfMet;
                    strRequirement += strThisRequirement;
                }

                if (blnRequirementMet || blnShowMessage)
                {
                    // Loop through the allof requirements.
                    objXmlRequiredList = objXmlNode.SelectNodes("required/allof");
                    foreach (XmlNode objXmlAllOf in objXmlRequiredList)
                    {
                        bool blnAllOfMet = true;
                        string strThisRequirement = "\n" +
                                                 LanguageManager.GetString("Message_SelectQuality_AllOf");
                        XmlNodeList objXmlAllOfList = objXmlAllOf.ChildNodes;
                        foreach (XmlNode objXmlRequired in objXmlAllOfList)
                        {
                            bool blnFound = TestNodeRequirements(objXmlRequired, objCharacter, out string name, strIgnoreQuality,
                                objMetatypeDocument,
                                objCritterDocument, objQualityDocument);

                            // If this item was not found, fail the AllOfMet condition.
                            if (!blnFound)
                            {
                                blnAllOfMet = false;
                                if (blnShowMessage)
                                    strThisRequirement += name;
                                else
                                    break;
                            }
                        }

                        // Update the flag for requirements met.
                        blnRequirementMet = blnRequirementMet && blnAllOfMet;
                        strRequirement += strThisRequirement;
                    }
                }

                // The character has not met the requirements, so display a message and uncheck the item.
                if (!blnRequirementMet)
                {
                    requiredMessage += strRequirement;

                    if (blnShowMessage)
                         MessageBox.Show(requiredMessage, requiredTitle,
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
            }
            return true;
        }

        public static bool TestNodeRequirements(XmlNode node, Character character, out string name,
            string strIgnoreQuality = "", XmlDocument objMetatypeDocument = null, XmlDocument objCritterDocument = null,
            XmlDocument objQualityDocument = null)
        {
            XmlNode nameNode;
            string strNodeInnerText = node.InnerText;
            string strNodeName = node["name"]?.InnerText ?? string.Empty;
            switch (node.Name)
            {
                case "attribute":
                    // Check to see if an Attribute meets a requirement.
                    CharacterAttrib objAttribute = character.GetAttribute(strNodeName);
                    int intTargetValue = Convert.ToInt32(node["total"].InnerText);
                    name = $"\n\t{objAttribute.DisplayAbbrev} {intTargetValue}";
                    // Special cases for when we want to check if a special attribute is enabled
                    if (intTargetValue == 1)
                    {
                        if (objAttribute.Abbrev == "MAG")
                            return character.MAGEnabled;
                        if (objAttribute.Abbrev == "RES")
                            return character.RESEnabled;
                        if (objAttribute.Abbrev == "DEP")
                            return character.DEPEnabled;
                    }
                    return objAttribute.TotalValue >= intTargetValue;

                case "attributetotal":
                    string strNodeAttributes = node["attributes"].InnerText;
                    int intNodeVal = Convert.ToInt32(node["val"].InnerText);
                    // Check if the character's Attributes add up to a particular total.
                    string strAttributes = AttributeSection.AttributeStrings.Aggregate(strNodeAttributes,
                        (current, strAttribute) => current.Replace(strAttribute, character.GetAttribute(strAttribute).DisplayAbbrev));
                    name = $"\n\t{strAttributes} {intNodeVal}";
                    strAttributes = AttributeSection.AttributeStrings.Aggregate(strNodeAttributes,
                        (current, strAttribute) => current.Replace(strAttribute, character.GetAttribute(strAttribute).Value.ToString()));
                    XmlDocument objXmlDocument = new XmlDocument();
                    XPathNavigator nav = objXmlDocument.CreateNavigator();
                    XPathExpression xprAttributes = nav.Compile(strAttributes);
                    return Convert.ToInt32(nav.Evaluate(xprAttributes)) >= intNodeVal;
                case "careerkarma":
                    // Check Career Karma requirement.
                    name = "\n\t" + LanguageManager.GetString("Message_SelectQuality_RequireKarma")
                               .Replace("{0}", strNodeInnerText);
                    return character.CareerKarma >= Convert.ToInt32(strNodeInnerText);
                case "critterpower":
                    // Run through all of the Powers the character has and see if the current required item exists.
                    if (character.CritterEnabled && character.CritterPowers != null)
                    {
                        CritterPower critterPower = character.CritterPowers.FirstOrDefault(p => p.Name == strNodeInnerText);
                        if (critterPower != null)
                        {
                            name = critterPower.DisplayNameShort;
                            return true;
                        }
                        XmlDocument critterPowers = XmlManager.Load("critterpowers.xml");
                        nameNode =
                            critterPowers.SelectSingleNode($"/chummer/powers/power[name = \"{strNodeInnerText}\"]");
                        name = nameNode["translate"] != null
                            ? "\n\t" + nameNode["translate"].InnerText
                            : "\n\t" + strNodeInnerText;
                        name += $" ({LanguageManager.GetString("Tab_Critter")})";
                        return false;
                    }
                    break;
                case "bioware":
                case "cyberware":
                    {
                        int count = Convert.ToInt32(node.Attributes?["count"]?.InnerText ?? "1");
                        name = null;
                        name += node.Name == "cyberware"
                            ? "\n\t" + LanguageManager.GetString("Label_Cyberware") + strNodeInnerText
                            : "\n\t" + LanguageManager.GetString("Label_Bioware") + strNodeInnerText;
                        string strWareNodeSelectAttribute = node.Attributes?["select"]?.InnerText ?? string.Empty;
                        return character.Cyberware.DeepCount(x => x.Children.Where(y => string.IsNullOrEmpty(y.PlugsIntoModularMount)), objCyberware =>
                               objCyberware.Name == strNodeInnerText && string.IsNullOrEmpty(strWareNodeSelectAttribute) || strWareNodeSelectAttribute == objCyberware.Extra) >= count;
                    }
                case "biowarecontains":
                case "cyberwarecontains":
                {
                        int count = Convert.ToInt32(node.Attributes?["count"]?.InnerText ?? "1");
                        name = null;
                        name += node.Name == "cyberware"
                            ? "\n\t" + LanguageManager.GetString("Label_Cyberware") + strNodeInnerText
                            : "\n\t" + LanguageManager.GetString("Label_Bioware") + strNodeInnerText;
                        Improvement.ImprovementSource source = Improvement.ImprovementSource.Cyberware;
                        if (node.Name == "biowarecontains")
                        {
                            source = Improvement.ImprovementSource.Bioware;
                        }
                        string strWareContainsNodeSelectAttribute = node.Attributes?["select"]?.InnerText ?? string.Empty;
                        return character.Cyberware.DeepCount(x => x.Children.Where(y => string.IsNullOrEmpty(y.PlugsIntoModularMount)), objCyberware =>
                            objCyberware.SourceType == source && objCyberware.Name.Contains(strNodeInnerText) &&
                            string.IsNullOrEmpty(strWareContainsNodeSelectAttribute) || strWareContainsNodeSelectAttribute == objCyberware.Extra) >= count;
                }
                case "damageresistance":
                    // Damage Resistance must be a particular value.
                    name = "\n\t" + LanguageManager.GetString("String_DamageResistance");
                    return character.BOD.TotalValue + ImprovementManager.ValueOf(character, Improvement.ImprovementType.DamageResistance) >=
                           Convert.ToInt32(strNodeInnerText);
                case "ess":
                    string objEssNodeGradeAttributeText = node.Attributes?["grade"]?.InnerText ?? string.Empty;
                    if (!string.IsNullOrEmpty(objEssNodeGradeAttributeText))
                    {
                        decimal decGrade =
                            character.Cyberware.Where(
                                    objCyberware =>
                                        objCyberware.Grade.Name.Contains(objEssNodeGradeAttributeText))
                                .Sum(objCyberware => objCyberware.CalculatedESS());
                        if (strNodeInnerText.StartsWith("-"))
                        {
                            // Essence must be less than the value.
                            name = "\n\t" +
                                   LanguageManager.GetString(
                                           "Message_SelectQuality_RequireESSGradeBelow")
                                       .Replace("{0}", strNodeInnerText)
                                       .Replace("{1}", objEssNodeGradeAttributeText)
                                       .Replace("{2}", decGrade.ToString(CultureInfo.InvariantCulture));
                            return decGrade <
                                   Convert.ToDecimal(strNodeInnerText.TrimStart('-'), GlobalOptions.InvariantCultureInfo);
                        }
                        // Essence must be equal to or greater than the value.
                        name = "\n\t" +
                               LanguageManager.GetString(
                                       "Message_SelectQuality_RequireESSGradeAbove")
                                   .Replace("{0}", strNodeInnerText)
                                   .Replace("{1}", objEssNodeGradeAttributeText)
                                   .Replace("{2}", decGrade.ToString(CultureInfo.InvariantCulture));
                        return decGrade >= Convert.ToDecimal(strNodeInnerText, GlobalOptions.InvariantCultureInfo);
                    }
                    // Check Essence requirement.
                    if (strNodeInnerText.StartsWith("-"))
                    {
                        // Essence must be less than the value.
                        name = "\n\t" +
                               LanguageManager.GetString(
                                       "Message_SelectQuality_RequireESSBelow")
                                   .Replace("{0}", strNodeInnerText)
                                   .Replace("{1}", character.Essence.ToString(CultureInfo.InvariantCulture));
                        return character.Essence <
                               Convert.ToDecimal(strNodeInnerText.TrimStart('-'), GlobalOptions.InvariantCultureInfo);
                    }
                    // Essence must be equal to or greater than the value.
                    name = "\n\t" +
                           LanguageManager.GetString(
                                   "Message_SelectQuality_RequireESSAbove")
                               .Replace("{0}", strNodeInnerText)
                               .Replace("{1}", character.Essence.ToString(CultureInfo.InvariantCulture));
                    return character.Essence >= Convert.ToDecimal(strNodeInnerText, GlobalOptions.InvariantCultureInfo);

                case "group":
                    // Check that clustered options are present (Magical Tradition + Skill 6, for example)
                    string result = string.Empty;
                    if (node.ChildNodes.Cast<XmlNode>().Any(childNode => !TestNodeRequirements(childNode, character, out result, strIgnoreQuality,
                        objMetatypeDocument,
                        objCritterDocument, objQualityDocument)))
                    {
                        name = result;
                        return false;
                    }
                    name = result;
                    return true;
                case "initiategrade":
                    // Character's initiate grade must be higher than or equal to the required value.
                    name = "\n\t" + LanguageManager.GetString("String_InitiateGrade") + " >= " + strNodeInnerText;
                    return character.InitiateGrade >= Convert.ToInt32(strNodeInnerText);
                case "martialtechnique":
                    // Character needs a specific Martial Arts technique.
                    XmlNode martialDoc = XmlManager.Load("martialarts.xml");
                    nameNode = martialDoc.SelectSingleNode($"/chummer/techniques/technique[name = \"{strNodeInnerText}\"]");
                    name = nameNode["translate"] != null
                        ? "\n\t" + nameNode["translate"].InnerText
                        : "\n\t" + strNodeInnerText;
                    name += $" ({LanguageManager.GetString("String_MartialArt")})";
                    return character.MartialArts.Any(martialart => martialart.Advantages.Any(technique => technique.Name == strNodeInnerText));
                case "metamagic":
                    XmlNode metamagicDoc = XmlManager.Load("metamagic.xml");
                    nameNode =
                        metamagicDoc.SelectSingleNode($"/chummer/metamagics/metamagic[name = \"{strNodeInnerText}\"]");
                    name = nameNode["translate"] != null
                        ? "\n\t" + nameNode["translate"].InnerText
                        : "\n\t" + strNodeInnerText;
                    name += $" ({LanguageManager.GetString("String_Metamagic")})";
                    return character.Metamagics.Any(objMetamagic => objMetamagic.Name == strNodeInnerText);
                case "metamagicart":
                case "art":
                    XmlNode metamagicArtDoc = XmlManager.Load("metamagic.xml");
                    nameNode =
                        metamagicArtDoc.SelectSingleNode($"/chummer/arts/art[name = \"{strNodeInnerText}\"]");
                    name = nameNode["translate"] != null
                        ? "\n\t" + nameNode["translate"].InnerText
                        : "\n\t" + strNodeInnerText;
                    name += $" ({LanguageManager.GetString("String_Art")})";
                    if (character.Options.IgnoreArt)
                    {
                        if (node.Name == "art")
                        {
                            return true;
                        }
                        foreach (Metamagic metamagic in character.Metamagics)
                        {
                            XmlNode metaNode =
                                metamagicArtDoc.SelectSingleNode($"/chummer/metamagics/metamagic[name = \"{metamagic.Name}\"]/required");
                            if (metaNode?.InnerXml.Contains($"<art>{strNodeInnerText}</art>") == true)
                            {
                                return metaNode.InnerXml.Contains($"<art>{strNodeInnerText}</art>");
                            }
                            metaNode =
                               metamagicArtDoc.SelectSingleNode($"/chummer/metamagics/metamagic[name = \"{metamagic.Name}\"]/forbidden");
                            if (metaNode?.InnerXml.Contains($"<art>{strNodeInnerText}</art>") == true)
                            {
                                return metaNode.InnerXml.Contains($"<art>{strNodeInnerText}</art>");
                            }
                        }
                        return false;
                    }
                    return character.Arts.Any(art => art.Name == strNodeInnerText);

                case "metatype":
                    // Check the Metatype restriction.
                    nameNode =
                        objMetatypeDocument.SelectSingleNode($"/chummer/metatypes/metatype[name = \"{strNodeInnerText}\"]") ??
                        objCritterDocument.SelectSingleNode($"/chummer/metatypes/metatype[name = \"{strNodeInnerText}\"]");
                    name = nameNode["translate"] != null
                        ? "\n\t" + nameNode["translate"].InnerText
                        : "\n\t" + strNodeInnerText;
                    name += $" ({LanguageManager.GetString("String_Metatype")})";
                    return strNodeInnerText == character.Metatype;

                case "metatypecategory":
                    // Check the Metatype Category restriction.
                    nameNode =
                        objMetatypeDocument.SelectSingleNode($"/chummer/categories/category[. = \"{strNodeInnerText}\"]") ??
                        objCritterDocument.SelectSingleNode($"/chummer/categories/category[. = \"{strNodeInnerText}\"]");
                    name = nameNode?.Attributes["translate"] != null
                        ? "\n\t" + nameNode.Attributes["translate"]?.InnerText
                        : "\n\t" + strNodeInnerText;
                    name += LanguageManager.GetString("String_MetatypeCategory");
                    return strNodeInnerText == character.MetatypeCategory;

                case "metavariant":
                    // Check the Metavariant restriction.
                    nameNode =
                        objMetatypeDocument.SelectSingleNode($"/chummer/metavariants/metavariant[name = \"{strNodeInnerText}\"]") ??
                        objCritterDocument.SelectSingleNode($"/chummer/metavariants/metavariant[name = \"{strNodeInnerText}\"]");
                    name = nameNode["translate"] != null
                        ? "\n\t" + nameNode["translate"].InnerText
                        : "\n\t" + strNodeInnerText;
                    name += $" ({LanguageManager.GetString("String_Metavariant")})";
                    return strNodeInnerText == character.Metavariant;

                case "power":
                    // Run through all of the Powers the character has and see if the current required item exists.
                    Power power = character.Powers.FirstOrDefault(p => p.Name == strNodeInnerText);
                    if (power != null)
                    {
                        name = power.DisplayNameShort;
                        return true;
                    }
                    XmlDocument xmlPowers = XmlManager.Load("powers.xml");
                    nameNode =
                        xmlPowers.SelectSingleNode($"/chummer/powers/power[name = \"{strNodeInnerText}\"]");
                    name = nameNode["translate"] != null
                        ? "\n\t" + nameNode["translate"].InnerText
                        : "\n\t" + strNodeInnerText;
                    name += $" ({LanguageManager.GetString("Tab_Adept")})";
                    return false;

                case "quality":
                    Quality quality =
                        character.Qualities.FirstOrDefault(q => q.Name == strNodeInnerText && q.Name != strIgnoreQuality);
                    if (quality != null)
                    {
                        name = quality.DisplayNameShort;
                        return true;
                    }
                    // ReSharper disable once RedundantIfElseBlock (Suppresses node warning)
                    else
                    {
                        nameNode =
                            objQualityDocument.SelectSingleNode($"/chummer/qualities/quality[name = \"{strNodeInnerText}\"]");
                        name = nameNode?["translate"] != null
                            ? "\n\t" + nameNode["translate"].InnerText
                            : "\n\t" + strNodeInnerText;
                        name += $" ({LanguageManager.GetString("String_Quality")})";
                        return false;
                    }

                case "skill":
                    // Check if the character has the required Skill.
                    if (node["type"] != null)
                    {
                        KnowledgeSkill s = character.SkillsSection.KnowledgeSkills
                            .Where(objSkill => objSkill.Name == strNodeName &&
                                               (node["spec"] == null ||
                                                objSkill.Specializations.Any(objSpec => objSpec.Name == node["spec"]?.InnerText)))
                            .FirstOrDefault(objSkill => objSkill.TotalBaseRating >= Convert.ToInt32(node["val"]?.InnerText));

                        if (s != null)
                        {
                            name = s.DisplayName;
                            if (node["spec"] != null && !character.Improvements.Any(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.DisableSpecializationEffects && objImprovement.UniqueName == s.Name && string.IsNullOrEmpty(objImprovement.Condition)))
                            {
                                name += $" ({node["spec"].InnerText})";
                            }
                            if (node["val"] != null)
                            {
                                name += $" {node["val"].InnerText}";
                            }
                            return true;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(strNodeName))
                        {
                            Skill s = character.SkillsSection.GetActiveSkill(strNodeName);
                            // Exotic Skill
                            if (s == null && node["spec"] != null)
                                s = character.SkillsSection.GetActiveSkill(strNodeName + " (" + node["spec"].InnerText + ")");
                            if (s != null && (node["spec"] == null || s.Specializations.Any(objSpec => objSpec.Name == node["spec"]?.InnerText)) && s.TotalBaseRating >= Convert.ToInt32(node["val"]?.InnerText))
                            {
                                name = s.DisplayName;
                                if (node["spec"] != null && !character.Improvements.Any(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.DisableSpecializationEffects && objImprovement.UniqueName == s.Name && string.IsNullOrEmpty(objImprovement.Condition)))
                                {
                                    name += $" ({node["spec"].InnerText})";
                                }
                                if (node["val"] != null)
                                {
                                    name += $" {node["val"].InnerText}";
                                }
                                return true;
                            }
                        }
                    }
                    XmlDocument xmlSkills = XmlManager.Load("skills.xml");
                    nameNode =
                        xmlSkills.SelectSingleNode($"/chummer/skills/skill[name = \"{strNodeName}\"]");
                    name = nameNode?["translate"] != null
                        ? "\n\t" + nameNode["translate"].InnerText
                        : "\n\t" + strNodeName;
                    if (node["spec"] != null)
                    {
                        name += $" ({node["spec"].InnerText})";
                    }
                    if (node["val"] != null)
                    {
                        name += $" {node["val"].InnerText}";
                    }
                    name += $" ({LanguageManager.GetString("Tab_Skills")})";
                    return false;

                case "skillgrouptotal":
                    {
                        // Check if the total combined Ratings of Skill Groups adds up to a particular total.
                        int intTotal = 0;
                        var strGroups = node["skillgroups"].InnerText.Split('+');
                        string outString = "\n\t";
                        for (int i = 0; i <= strGroups.Length - 1; i++)
                            foreach (SkillGroup objGroup in character.SkillsSection.SkillGroups)
                                if (objGroup.Name == strGroups[i])
                                {
                                    outString += objGroup.DisplayName + ", ";
                                    intTotal += objGroup.Rating;
                                    break;
                                }
                        name = outString;
                        name += $" ({LanguageManager.GetString("String_ExpenseSkillGroup")})";
                        return intTotal >= Convert.ToInt32(node["val"].InnerText);
                    }
                case "spell":
                    // Check for a specific Spell.
                    XmlDocument xmlSpell = XmlManager.Load("spells.xml");
                    nameNode =
                        xmlSpell.SelectSingleNode($"/chummer/spells/spell[name = \"{strNodeInnerText}\"]");
                    name = nameNode["translate"] != null
                        ? "\n\t" + nameNode["translate"].InnerText
                        : "\n\t" + strNodeInnerText;
                    name += $" ({LanguageManager.GetString("String_DescSpell")})";
                    return character.Spells.Any(spell => spell.Name == strNodeInnerText);
                case "spellcategory":
                    // Check for a specified amount of a particular Spell category.
                    XmlDocument xmlSpells = XmlManager.Load("spells.xml");
                    nameNode =
                        xmlSpells.SelectSingleNode($"/chummer/categories/category[. = \"{strNodeName}\"]");
                    name = nameNode["translate"] != null
                        ? "\n\t" + nameNode["translate"].InnerText
                        : "\n\t" + strNodeInnerText;
                    name += $" ({LanguageManager.GetString("String_SpellCategory")})";
                    return character.Spells.Count(objSpell => objSpell.Category == strNodeName) >= Convert.ToInt32(node["count"].InnerText);
                case "spelldescriptor":
                    // Check for a specified amount of a particular Spell Descriptor.
                    name = "\n\t" + LanguageManager.GetString("Label_Descriptors") + " >= " + node["count"].InnerText;
                    return character.Spells.Count(objSpell => objSpell.Descriptors.Contains(strNodeName)) >= Convert.ToInt32(node["count"].InnerText);
                case "streetcredvsnotoriety":
                    // Street Cred must be higher than Notoriety.
                    name = "\n\t" + LanguageManager.GetString("String_StreetCred") + " >= " +
                           LanguageManager.GetString("String_Notoriety");
                    return character.StreetCred >= character.Notoriety;
                case "tradition":
                    // Character needs a specific Tradition.
                    XmlDocument xmlTradition = XmlManager.Load("traditions.xml");
                    nameNode =
                        xmlTradition.SelectSingleNode($"/chummer/traditions/tradition[name = \"{strNodeInnerText}\"]");
                    name = nameNode["translate"] != null
                        ? "\n\t" + nameNode["translate"].InnerText
                        : "\n\t" + strNodeInnerText;
                    name += $" ({LanguageManager.GetString("String_Tradition")})";
                    return character.MagicTradition == strNodeInnerText;
                default:
                    Utils.BreakIfDebug();
                    break;
            }
            name = strNodeInnerText;
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
        /// <param name="blnAddToList"></param>
        /// <returns></returns>
        public static bool CheckAvailRestriction(XmlNode objXmlGear, Character objCharacter, bool blnHide, int intRating = 0,
            int intAvailModifier = 0, bool blnAddToList = true)
        {
            if (objXmlGear == null)
                return false;
            XmlDocument objXmlDocument = new XmlDocument();
            //TODO: Better handler for restricted gear
            if (!blnHide || objCharacter.Created || objCharacter.RestrictedGear ||
                objCharacter.IgnoreRules || !blnAddToList) return blnAddToList;
            // Avail.
            // If avail contains "F" or "R", remove it from the string so we can use the expression.
            string strAvailExpr = objXmlGear["avail"]?.InnerText ?? string.Empty;
            string strPrefix = string.Empty;
            if (strAvailExpr.StartsWith("FixedValues"))
            {
                var strValues = strAvailExpr.TrimStart("FixedValues", true).Trim("()".ToCharArray()).Split(',');
                strAvailExpr = strValues[Math.Max(intRating - 1, 0)];
            }
            if (strAvailExpr.Substring(strAvailExpr.Length - 1, 1) == "F" ||
                strAvailExpr.Substring(strAvailExpr.Length - 1, 1) == "R")
                strAvailExpr = strAvailExpr.Substring(0, strAvailExpr.Length - 1);
            if (strAvailExpr.Substring(0, 1) == "+")
            {
                strPrefix = "+";
                strAvailExpr = strAvailExpr.Substring(1, strAvailExpr.Length - 1);
            }
            if (strPrefix == "+") return blnAddToList;
            try
            {
                XPathNavigator nav = objXmlDocument.CreateNavigator();
                XPathExpression xprAvail = nav.Compile(strAvailExpr.Replace("Rating",
                    intRating.ToString(GlobalOptions.InvariantCultureInfo)));
                blnAddToList = Convert.ToInt32(nav.Evaluate(xprAvail)) + intAvailModifier <=
                               objCharacter.MaximumAvailability;
            }
            catch (XPathException)
            {
            }
            return blnAddToList;
        }

        public static bool CheckNuyenRestriction(XPathNavigator nav, XmlNode objXmlGear, Character objCharacter, decimal decMaxNuyen, decimal decCostMultiplier = 1.0m)
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
                        int intTmp;
                        if (int.TryParse(strLoopCostString, out intTmp))
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
                    XPathExpression xprCost = nav.Compile(objCostNode.InnerText.Replace("Rating", intRating.ToString(GlobalOptions.InvariantCultureInfo)));
                    decCost = Convert.ToDecimal(nav.Evaluate(xprCost), GlobalOptions.InvariantCultureInfo);
                }
                catch (XPathException)
                {
                    decimal decTemp;
                    if (decimal.TryParse(objCostNode.InnerText, out decTemp))
                    {
                        decCost = decTemp;
                    }
                }

                if (objCostNode.InnerText.StartsWith("FixedValues"))
                {
                    string[] strValues = objCostNode.InnerText.TrimStart("FixedValues", true).Trim("()".ToCharArray()).Split(',');
                    decCost = Convert.ToDecimal(strValues[0].Trim("[]".ToCharArray()), GlobalOptions.InvariantCultureInfo);
                }
                else if (objCostNode.InnerText.StartsWith("Variable"))
                {
                    decimal decMin = 0;
                    string strCost = objCostNode.InnerText.TrimStart("Variable", true).Trim("()".ToCharArray());
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
