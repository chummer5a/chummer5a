using Chummer.Backend.Attributes;
using Chummer.Backend.Equipment;
using Chummer.Skills;
using System;
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
        /// <returns></returns>
        public static bool RequirementsMet(XmlNode objXmlNode, bool blnShowMessage, Character objCharacter, XmlDocument objMetatypeDocument = null, XmlDocument objCritterDocument = null, XmlDocument objQualityDocument = null, string strIgnoreQuality = "", string strLocalName = "")
        {
            // Ignore the rules.
            if (objCharacter.IgnoreRules)
                return true;
            if (objMetatypeDocument == null)
                objMetatypeDocument = XmlManager.Instance.Load("metatypes.xml");
            if (objCritterDocument == null)
                objCritterDocument = XmlManager.Instance.Load("critters.xml");
            if (objQualityDocument == null)
                objQualityDocument = XmlManager.Instance.Load("qualities.xml");
            if (objXmlNode == null) return false;
            // See if the character already has this Quality and whether or not multiple copies are allowed.
            if (objXmlNode["limit"] != null && objXmlNode["limit"]?.InnerText != "no")
            {
                string limitTitle = LanguageManager.Instance.GetString("MessageTitle_SelectGeneric_Limit").Replace("{0}", strLocalName);
                string limitMessage = LanguageManager.Instance.GetString("Message_SelectGeneric_Limit").Replace("{0}", strLocalName);
                int intLimit = Convert.ToInt32(objXmlNode["limit"]?.InnerText);
                int intCount;
                bool allow = true;

                switch (objXmlNode.Name)
                {
                    case "quality":
                        {
                            intCount =
                                objCharacter.Qualities.Count(
                                    objItem => objItem.Name == objXmlNode["name"]?.InnerText && objItem.Name != strIgnoreQuality);
                            allow = !(intCount > intLimit &&
                                    objCharacter.Qualities.Any(
                                        objItem =>
                                            objItem.Name == objXmlNode["name"]?.InnerText &&
                                            objItem.Name != strIgnoreQuality));
                            break;
                        }
                    case "metamagic":
                        {
                            intCount =
                                objCharacter.Metamagics.Count(
                                    objItem => objItem.Name == objXmlNode["name"]?.InnerText && objItem.Name != strIgnoreQuality);
                            allow = !(intCount > intLimit &&
                                    objCharacter.Metamagics.Any(
                                        objItem =>
                                            objItem.Name == objXmlNode["name"]?.InnerText &&
                                            objItem.Name != strIgnoreQuality));
                            break;
                        }
                    case "art":
                        {
                            intCount =
                                objCharacter.Arts.Count(
                                    objItem => objItem.Name == objXmlNode["name"]?.InnerText && objItem.Name != strIgnoreQuality);
                            allow = !(intCount > intLimit &&
                                    objCharacter.Arts.Any(
                                        objItem =>
                                            objItem.Name == objXmlNode["name"]?.InnerText &&
                                            objItem.Name != strIgnoreQuality));
                            break;
                        }
                    case "enhancement":
                        {
                            intCount =
                                objCharacter.Enhancements.Count(
                                    objItem => objItem.Name == objXmlNode["name"]?.InnerText && objItem.Name != strIgnoreQuality);
                            allow = !(intCount > intLimit &&
                                    objCharacter.Enhancements.Any(
                                        objItem =>
                                            objItem.Name == objXmlNode["name"]?.InnerText &&
                                            objItem.Name != strIgnoreQuality));
                            break;
                        }
                    default:
                        {
                            Utils.BreakIfDebug();
                            break;
                        }
                }
                if (!allow)
                {
                    if (blnShowMessage)
                    {
                        limitMessage = limitMessage.Replace("{1}", intLimit == 0 ? "1" : intLimit.ToString());
                        MessageBox.Show(limitMessage, limitTitle,
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return false;
                    }
                    return false;
                }
            }

            if (objXmlNode.InnerXml.Contains("forbidden"))
            {
                string forbiddenTitle = LanguageManager.Instance.GetString("MessageTitle_SelectGeneric_Restriction").Replace("{0}", strLocalName);
                string forbiddenMessage = LanguageManager.Instance.GetString("Message_SelectGeneric_Restriction").Replace("{0}", strLocalName);
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
                string requiredTitle = LanguageManager.Instance.GetString("MessageTitle_SelectGeneric_Requirement").Replace("{0}", strLocalName);
                string requiredMessage = LanguageManager.Instance.GetString("Message_SelectGeneric_Requirement").Replace("{0}", strLocalName);
                string strRequirement = string.Empty;
                bool blnRequirementMet = true;

                // Loop through the oneof requirements.
                XmlNodeList objXmlRequiredList = objXmlNode.SelectNodes("required/oneof");
                foreach (XmlNode objXmlOneOf in objXmlRequiredList)
                {
                    bool blnOneOfMet = false;
                    string strThisRequirement = "\n" +
                                                LanguageManager.Instance.GetString("Message_SelectQuality_OneOf");
                    XmlNodeList objXmlOneOfList = objXmlOneOf.ChildNodes;
                    foreach (XmlNode objXmlRequired in objXmlOneOfList)
                    {
                        blnOneOfMet = TestNodeRequirements(objXmlRequired, objCharacter, out string name, strIgnoreQuality,
                            objMetatypeDocument,
                            objCritterDocument, objQualityDocument);

                        if (blnOneOfMet) break;
                        strThisRequirement += name;
                    }

                    // Update the flag for requirements met.
                    blnRequirementMet = blnRequirementMet && blnOneOfMet;
                    strRequirement += strThisRequirement;
                }

                // Loop through the allof requirements.
                objXmlRequiredList = objXmlNode.SelectNodes("required/allof");
                foreach (XmlNode objXmlAllOf in objXmlRequiredList)
                {
                    bool blnAllOfMet = true;
                    string strThisRequirement = "\n" +
                                             LanguageManager.Instance.GetString("Message_SelectQuality_AllOf");
                    XmlNodeList objXmlAllOfList = objXmlAllOf.ChildNodes;
                    foreach (XmlNode objXmlRequired in objXmlAllOfList)
                    {
                        bool blnFound = TestNodeRequirements(objXmlRequired, objCharacter, out string name, strIgnoreQuality,
                            objMetatypeDocument,
                            objCritterDocument, objQualityDocument);

                        // If this item was not found, fail the AllOfMet condition.
                        if (blnFound) continue;
                        strThisRequirement += name;
                        blnAllOfMet = false;
                    }

                    // Update the flag for requirements met.
                    blnRequirementMet = blnRequirementMet && blnAllOfMet;
                    strRequirement += strThisRequirement;
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
            switch (node.Name)
            {
                case "attribute":
                    // Check to see if an Attribute meets a requirement.
                    CharacterAttrib objAttribute =
                        character.GetAttribute(node["name"].InnerText);
                    name = $"\n\t{objAttribute.DisplayAbbrev} {node["total"].InnerText}";
                    return objAttribute.TotalValue >=
                           Convert.ToInt32(node["total"].InnerText);

                case "attributetotal":
                    // Check if the character's Attributes add up to a particular total.
                    string strAttributes = Character.AttributeStrings.Aggregate(node["attributes"].InnerText,
                        (current, strAttribute) => current.Replace(strAttribute, character.GetAttribute(strAttribute).DisplayAbbrev));
                    name = $"\n\t{strAttributes} {node["val"].InnerText}";
                    strAttributes = Character.AttributeStrings.Aggregate(node["attributes"].InnerText,
                        (current, strAttribute) => current.Replace(strAttribute, character.GetAttribute(strAttribute).Value.ToString()));
                    XmlDocument objXmlDocument = new XmlDocument();
                    XPathNavigator nav = objXmlDocument.CreateNavigator();
                    XPathExpression xprAttributes = nav.Compile(strAttributes);
                    return Convert.ToInt32(nav.Evaluate(xprAttributes)) >= Convert.ToInt32(node["val"].InnerText);
                case "careerkarma":
                    // Check Career Karma requirement.
                    name = "\n\t" + LanguageManager.Instance.GetString("Message_SelectQuality_RequireKarma")
                               .Replace("{0}", node.InnerText);
                    return character.CareerKarma >= Convert.ToInt32(node.InnerText);

                case "critterpower":
                    // Run through all of the Powers the character has and see if the current required item exists.
                    if (character.CritterEnabled && character.CritterPowers != null)
                    {
                        CritterPower critterPower = character.CritterPowers.FirstOrDefault(p => p.Name == node.InnerText);
                        if (critterPower != null)
                        {
                            name = critterPower.DisplayNameShort;
                            return true;
                        }
                        XmlDocument critterPowers = XmlManager.Instance.Load("critterpowers.xml");
                        nameNode =
                            critterPowers.SelectSingleNode($"/chummer/powers/power[name = \"{node.InnerText}\"]");
                        name = nameNode["translate"] != null
                            ? "\n\t" + nameNode["translate"].InnerText
                            : "\n\t" + node.InnerText;
                        name += $" ({LanguageManager.Instance.GetString("Tab_Critter")})";
                        return false;
                    }
                    break;
                case "bioware":
                case "cyberware":
                {

                    int count = node.Attributes?["count"] != null ? Convert.ToInt32(node.Attributes?["count"].InnerText) : 1;
                    name = null;
                    name += node.Name == "cyberware"
                        ? "\n\t" + LanguageManager.Instance.GetString("Label_Cyberware") + node.InnerText
                        : "\n\t" + LanguageManager.Instance.GetString("Label_Bioware") + node.InnerText;
                    return
                        character.Cyberware.Count(
                            objCyberware =>
                                objCyberware.Name == node.InnerText && node.Attributes?["select"] == null ||
                                node.Attributes?["select"] != null && node.Attributes?["select"].InnerText == objCyberware.Location) >= count;
                    }
                case "biowarecontains":
                case "cyberwarecontains":
                {
                    int count = node.Attributes?["count"] != null ? Convert.ToInt32(node.Attributes?["count"].InnerText) : 1;
                        name = null;
                        name += node.Name == "cyberware"
                            ? "\n\t" + LanguageManager.Instance.GetString("Label_Cyberware") + node.InnerText
                            : "\n\t" + LanguageManager.Instance.GetString("Label_Bioware") + node.InnerText;
                    Improvement.ImprovementSource source = Improvement.ImprovementSource.Cyberware;
                    if (node.Name == "biowarecontains")
                    {
                        source = Improvement.ImprovementSource.Bioware;
                    }
                    return character.Cyberware.Count(objCyberware =>
                            objCyberware.SourceType == source && objCyberware.Name.Contains(node.InnerText) && node.Attributes?["select"] == null ||
                            node.Attributes?["select"] != null && node.Attributes?["select"].InnerText == objCyberware.Location) >= count;
                }
                case "damageresistance":
                    // Damage Resistance must be a particular value.
                    ImprovementManager objImprovementManager = new ImprovementManager(character);
                    name = "\n\t" + LanguageManager.Instance.GetString("String_DamageResistance");
                    return character.BOD.TotalValue + objImprovementManager.ValueOf(Improvement.ImprovementType.DamageResistance) >=
                           Convert.ToInt32(node.InnerText);
                case "ess":
                    if (node.Attributes["grade"] != null)
                    {
                        decimal decGrade =
                            character.Cyberware.Where(
                                    objCyberware =>
                                        objCyberware.Grade.Name ==
                                        node.Attributes?["grade"].InnerText)
                                .Sum(objCyberware => objCyberware.CalculatedESS());
                        if (node.InnerText.StartsWith("-"))
                        {
                            // Essence must be less than the value.
                            name = "\n\t" +
                                   LanguageManager.Instance.GetString(
                                           "Message_SelectQuality_RequireESSGradeBelow")
                                       .Replace("{0}", node.InnerText)
                                       .Replace("{1}", node.Attributes["grade"].InnerText)
                                       .Replace("{2}", decGrade.ToString(CultureInfo.InvariantCulture));
                            return decGrade <
                                   Convert.ToDecimal(node.InnerText.Replace("-", string.Empty), GlobalOptions.InvariantCultureInfo);
                        }
                        // Essence must be equal to or greater than the value.
                        name = "\n\t" +
                               LanguageManager.Instance.GetString(
                                       "Message_SelectQuality_RequireESSGradeAbove")
                                   .Replace("{0}", node.InnerText)
                                   .Replace("{1}", node.Attributes["grade"].InnerText)
                                   .Replace("{2}", decGrade.ToString(CultureInfo.InvariantCulture));
                        return decGrade >= Convert.ToDecimal(node.InnerText, GlobalOptions.InvariantCultureInfo);
                    }
                    // Check Essence requirement.
                    if (node.InnerText.StartsWith("-"))
                    {
                        // Essence must be less than the value.
                        name = "\n\t" +
                               LanguageManager.Instance.GetString(
                                       "Message_SelectQuality_RequireESSBelow")
                                   .Replace("{0}", node.InnerText)
                                   .Replace("{1}", character.Essence.ToString(CultureInfo.InvariantCulture));
                        return character.Essence <
                               Convert.ToDecimal(node.InnerText.Replace("-", string.Empty), GlobalOptions.InvariantCultureInfo);
                    }
                    // Essence must be equal to or greater than the value.
                    name = "\n\t" +
                           LanguageManager.Instance.GetString(
                                   "Message_SelectQuality_RequireESSAbove")
                               .Replace("{0}", node.InnerText)
                               .Replace("{1}", character.Essence.ToString(CultureInfo.InvariantCulture));
                    return character.Essence >= Convert.ToDecimal(node.InnerText, GlobalOptions.InvariantCultureInfo);

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
                    name = "\n\t" + LanguageManager.Instance.GetString("String_InitiateGrade") + " >= " + node.InnerText;
                    return character.InitiateGrade >= Convert.ToInt32(node.InnerText);
                case "martialtechnique":
                    // Character needs a specific Martial Arts technique.
                    XmlNode martialDoc = XmlManager.Instance.Load("martialarts.xml");
                    nameNode = martialDoc.SelectSingleNode($"/chummer/techniques/technique[name = \"{node.InnerText}\"]");
                    name = nameNode["translate"] != null
                        ? "\n\t" + nameNode["translate"].InnerText
                        : "\n\t" + node.InnerText;
                    name += $" ({LanguageManager.Instance.GetString("String_MartialArt")})";
                    return character.MartialArts.Any(martialart => martialart.Advantages.Any(technique => technique.Name == node.InnerText));
                case "metamagic":
                    XmlNode metamagicDoc = XmlManager.Instance.Load("metamagic.xml");
                    nameNode =
                        metamagicDoc.SelectSingleNode($"/chummer/metamagics/metamagic[name = \"{node.InnerText}\"]");
                    name = nameNode["translate"] != null
                        ? "\n\t" + nameNode["translate"].InnerText
                        : "\n\t" + node.InnerText;
                    name += $" ({LanguageManager.Instance.GetString("String_Metamagic")})";
                    return character.Metamagics.Any(objMetamagic => objMetamagic.Name == node.InnerText);
                case "metamagicart":
                case "art":
                    XmlNode metamagicArtDoc = XmlManager.Instance.Load("metamagic.xml");
                    nameNode =
                        metamagicArtDoc.SelectSingleNode($"/chummer/arts/art[name = \"{node.InnerText}\"]");
                    name = nameNode["translate"] != null
                        ? "\n\t" + nameNode["translate"].InnerText
                        : "\n\t" + node.InnerText;
                    name += $" ({LanguageManager.Instance.GetString("String_Art")})";
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
                            if (metaNode?.InnerXml.Contains($"<art>{node.InnerText}</art>") == true)
                            {
                                return metaNode.InnerXml.Contains($"<art>{node.InnerText}</art>");
                            }
                            metaNode =
                               metamagicArtDoc.SelectSingleNode($"/chummer/metamagics/metamagic[name = \"{metamagic.Name}\"]/forbidden");
                            if (metaNode?.InnerXml.Contains($"<art>{node.InnerText}</art>") == true)
                            {
                                return metaNode.InnerXml.Contains($"<art>{node.InnerText}</art>");
                            }
                        }
                        return false;
                    }
                    return character.Arts.Any(art => art.Name == node.InnerText);

                case "metatype":
                    // Check the Metatype restriction.
                    nameNode =
                        objMetatypeDocument.SelectSingleNode($"/chummer/metatypes/metatype[name = \"{node.InnerText}\"]") ??
                        objCritterDocument.SelectSingleNode($"/chummer/metatypes/metatype[name = \"{node.InnerText}\"]");
                    name = nameNode["translate"] != null
                        ? "\n\t" + nameNode["translate"].InnerText
                        : "\n\t" + node.InnerText;
                    name += $" ({LanguageManager.Instance.GetString("String_Metatype")})";
                    return node.InnerText == character.Metatype;

                case "metatypecategory":
                    // Check the Metatype Category restriction.
                    nameNode =
                        objMetatypeDocument.SelectSingleNode($"/chummer/categories/category[. = \"{node.InnerText}\"]") ??
                        objCritterDocument.SelectSingleNode($"/chummer/categories/category[. = \"{node.InnerText}\"]");
                    name = nameNode?.Attributes["translate"] != null
                        ? "\n\t" + nameNode.Attributes["translate"]?.InnerText
                        : "\n\t" + node.InnerText;
                    name += LanguageManager.Instance.GetString("String_MetatypeCategory");
                    return node.InnerText == character.MetatypeCategory;

                case "metavariant":
                    // Check the Metavariant restriction.
                    nameNode =
                        objMetatypeDocument.SelectSingleNode($"/chummer/metavariants/metavariant[name = \"{node.InnerText}\"]") ??
                        objCritterDocument.SelectSingleNode($"/chummer/metavariants/metavariant[name = \"{node.InnerText}\"]");
                    name = nameNode["translate"] != null
                        ? "\n\t" + nameNode["translate"].InnerText
                        : "\n\t" + node.InnerText;
                    name += $" ({LanguageManager.Instance.GetString("String_Metavariant")})";
                    return node.InnerText == character.Metavariant;

                case "power":
                    // Run through all of the Powers the character has and see if the current required item exists.
                    Power power = character.Powers.FirstOrDefault(p => p.Name == node.InnerText);
                    if (power != null)
                    {
                        name = power.DisplayNameShort;
                        return true;
                    }
                    XmlDocument xmlPowers = XmlManager.Instance.Load("powers.xml");
                    nameNode =
                        xmlPowers.SelectSingleNode($"/chummer/powers/power[name = \"{node.InnerText}\"]");
                    name = nameNode["translate"] != null
                        ? "\n\t" + nameNode["translate"].InnerText
                        : "\n\t" + node.InnerText;
                    name += $" ({LanguageManager.Instance.GetString("Tab_Adept")})";
                    return false;

                case "quality":
                    Quality quality =
                        character.Qualities.FirstOrDefault(q => q.Name == node.InnerText && q.Name != strIgnoreQuality);
                    if (quality != null)
                    {
                        name = quality.DisplayNameShort;
                        return true;
                    }
                    // ReSharper disable once RedundantIfElseBlock (Suppresses node warning)
                    else
                    {
                        nameNode =
                            objQualityDocument.SelectSingleNode($"/chummer/qualities/quality[name = \"{node.InnerText}\"]");
                        name = nameNode?["translate"] != null
                            ? "\n\t" + nameNode["translate"].InnerText
                            : "\n\t" + node.InnerText;
                        name += $" ({LanguageManager.Instance.GetString("String_Quality")})";
                        return false;
                    }

                case "skill":
                    // Check if the character has the required Skill.
                    if (node["type"] != null)
                    {
                        KnowledgeSkill s = character.SkillsSection.KnowledgeSkills
                            .Where(objSkill => objSkill.Name == node["name"]?.InnerText &&
                                               (node["spec"] == null ||
                                                objSkill.Specializations.Any(objSpec => objSpec.Name == node["spec"]?.InnerText)))
                            .FirstOrDefault(objSkill => objSkill.LearnedRating >= Convert.ToInt32(node["val"]?.InnerText));

                        if (s != null)
                        {
                            name = s.DisplayName;
                            if (node["spec"] != null)
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
                        Skill s = character.SkillsSection.Skills
                            .Where(objSkill => objSkill.Name == node["name"]?.InnerText &&
                                               (node["spec"] == null ||
                                                objSkill.Specializations.Any(objSpec => objSpec.Name == node["spec"]?.InnerText)))
                            .FirstOrDefault(objSkill => objSkill.LearnedRating >= Convert.ToInt32(node["val"]?.InnerText));

                        if (s != null)
                        {
                            name = s.DisplayName;
                            if (node["spec"] != null)
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
                    XmlDocument xmlSkills = XmlManager.Instance.Load("skills.xml");
                    nameNode =
                        xmlSkills.SelectSingleNode($"/chummer/skills/skill[name = \"{node["name"].InnerText}\"]");
                    name = nameNode?["translate"] != null
                        ? "\n\t" + nameNode["translate"].InnerText
                        : "\n\t" + node["name"].InnerText;
                    if (node["spec"] != null)
                    {
                        name += $" ({node["spec"].InnerText})";
                    }
                    if (node["val"] != null)
                    {
                        name += $" {node["val"].InnerText}";
                    }
                    name += $" ({LanguageManager.Instance.GetString("Tab_Skills")})";
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
                        name += $" ({LanguageManager.Instance.GetString("String_ExpenseSkillGroup")})";
                        return intTotal >= Convert.ToInt32(node["val"].InnerText);
                    }
                case "spell":
                    // Check for a specific Spell.
                    XmlDocument xmlSpell = XmlManager.Instance.Load("spells.xml");
                    nameNode =
                        xmlSpell.SelectSingleNode($"/chummer/spells/spell[name = \"{node.InnerText}\"]");
                    name = nameNode["translate"] != null
                        ? "\n\t" + nameNode["translate"].InnerText
                        : "\n\t" + node.InnerText;
                    name += $" ({LanguageManager.Instance.GetString("String_DescSpell")})";
                    return character.Spells.Any(spell => spell.Name == node.InnerText);
                case "spellcategory":
                    // Check for a specified amount of a particular Spell category.
                    XmlDocument xmlSpells = XmlManager.Instance.Load("spells.xml");
                    nameNode =
                        xmlSpells.SelectSingleNode($"/chummer/categories/category[. = \"{node["name"].InnerText}\"]");
                    name = nameNode["translate"] != null
                        ? "\n\t" + nameNode["translate"].InnerText
                        : "\n\t" + node.InnerText;
                    name += $" ({LanguageManager.Instance.GetString("String_SpellCategory")})";
                    return character.Spells.Count(objSpell => objSpell.Category == node["name"].InnerText) >= Convert.ToInt32(node["count"].InnerText);
                case "spelldescriptor":
                    // Check for a specified amount of a particular Spell Descriptor.
                    name = "\n\t" + LanguageManager.Instance.GetString("Label_Descriptors") + " >= " + node["count"].InnerText;
                    return character.Spells.Count(objSpell => objSpell.Descriptors.Contains(node["name"].InnerText)) >= Convert.ToInt32(node["count"].InnerText);
                case "streetcredvsnotoriety":
                    // Street Cred must be higher than Notoriety.
                    name = "\n\t" + LanguageManager.Instance.GetString("String_StreetCred") + " >= " +
                           LanguageManager.Instance.GetString("String_Notoriety");
                    return character.StreetCred >= character.Notoriety;
                case "tradition":
                    // Character needs a specific Tradition.
                    XmlDocument xmlTradition = XmlManager.Instance.Load("traditions.xml");
                    nameNode =
                        xmlTradition.SelectSingleNode($"/chummer/traditions/tradition[name = \"{node.InnerText}\"]");
                    name = nameNode["translate"] != null
                        ? "\n\t" + nameNode["translate"].InnerText
                        : "\n\t" + node.InnerText;
                    name += $" ({LanguageManager.Instance.GetString("String_Tradition")})";
                    return character.MagicTradition == node.InnerText;
                default:
                    Utils.BreakIfDebug();
                    break;
            }
            name = node.InnerText;
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
            XmlDocument objXmlDocument = new XmlDocument();
            //TODO: Better handler for restricted gear
            if (!blnHide || objCharacter.Created || objCharacter.RestrictedGear ||
                objCharacter.IgnoreRules || !blnAddToList) return blnAddToList;
            // Avail.
            // If avail contains "F" or "R", remove it from the string so we can use the expression.
            string strAvailExpr = string.Empty;
            string strPrefix = string.Empty;
            if (objXmlGear["avail"] != null)
                strAvailExpr = objXmlGear["avail"].InnerText;
            if (intRating <= 3 && objXmlGear["avail3"] != null)
                strAvailExpr = objXmlGear["avail3"].InnerText;
            else if (intRating <= 6 && objXmlGear["avail6"] != null)
                strAvailExpr = objXmlGear["avail6"].InnerText;
            else if (intRating >= 7 && objXmlGear["avail10"] != null)
                strAvailExpr = objXmlGear["avail10"].InnerText;
            if (strAvailExpr.StartsWith("FixedValues"))
            {
                var strValues = strAvailExpr.Replace("FixedValues(", string.Empty).Replace(")", string.Empty).Split(',');
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
            catch
            {
            }
            return blnAddToList;
        }
    }
}
