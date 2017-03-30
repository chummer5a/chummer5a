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
    class SelectionShared
    {
        //TODO: Might be a better location for this; Class names are screwy.
        /// <summary>
        /// Evaluates requirements of a given node against a given Character object.
        /// </summary>
        /// <param name="objXmlNode">XmlNode of the object.</param>
        /// <param name="blnShowMessage">Should warning messages about whether the object has failed to validate be shown?</param>
        /// <param name="objCharacter">Character Object.</param>
        /// <param name="strIgnoreQuality">Name of a Quality that should be ignored. Typically used when swapping Qualities in career mode.</param>
        /// <returns></returns>
        public static bool RequirementsMet(XmlNode objXmlNode, bool blnShowMessage, Character objCharacter, string strIgnoreQuality = "", XmlDocument objMetatypeDocument = null, XmlDocument objCritterDocument = null, XmlDocument objQualityDocument = null)
        {
            // Ignore the rules.
            if (objCharacter.IgnoreRules)
                return true;
            //TODO: Find a better way to manage this, repeated calls to the method cause a stupid amount of CPU activity from loading the XMLs. Cache the XMLs in Main as part of the Load method, maybe?
            if (objMetatypeDocument == null)
            {
                objMetatypeDocument = XmlManager.Instance.Load("metatypes.xml");
            }
            if (objCritterDocument == null)
            {
                objCritterDocument = XmlManager.Instance.Load("critters.xml");
            }
            if (objQualityDocument == null)
            {
                objQualityDocument = XmlManager.Instance.Load("qualities.xml");
            }
            // See if the character already has this Quality and whether or not multiple copies are allowed.
            if (objXmlNode == null) return false;
            if (objXmlNode["limit"]?.InnerText != "no")
            {
                switch (objXmlNode.Name)
                {
                    case "quality":
                        {
                            int intLimit = Convert.ToInt32(objXmlNode["limit"]?.InnerText);
                            int intCount =
                                objCharacter.Qualities.Count(objItem => objItem.Name == objXmlNode["name"]?.InnerText && objItem.Name != strIgnoreQuality);
                            if (intCount > intLimit &&
                                objCharacter.Qualities.Any(
                                    objItem =>
                                        objItem.Name == objXmlNode["name"]?.InnerText &&
                                        objItem.Name != strIgnoreQuality))
                            {
                                if (blnShowMessage)
                                    MessageBox.Show(
                                        LanguageManager.Instance.GetString("Message_SelectQuality_QualityLimit"),
                                        LanguageManager.Instance.GetString("MessageTitle_SelectQuality_QualityLimit"),
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return false;
                            }
                            break;
                        }
                }
            }

            if (objXmlNode.InnerXml.Contains("forbidden"))
            {
                bool blnRequirementForbidden = false;
                string strForbidden = string.Empty;

                // Loop through the oneof requirements.
                XmlNodeList objXmlForbiddenList = objXmlNode.SelectNodes("forbidden/oneof");
                if (objXmlForbiddenList != null)
                    foreach (XmlNode objXmlOneOf in objXmlForbiddenList)
                    {
                        XmlNodeList objXmlOneOfList = objXmlOneOf.ChildNodes;

                        foreach (XmlNode objXmlForbidden in objXmlOneOfList)
                        {
                            switch (objXmlForbidden.Name)
                            {
                                case "quality":
                                    // Run through all of the Qualities the character has and see if the current forbidden item exists.
                                    // If so, turn on the RequirementForbidden flag so it cannot be selected.
                                    foreach (Quality objQuality in objCharacter.Qualities)
                                    {
                                        if (objQuality.Name == objXmlForbidden.InnerText && objQuality.Name != strIgnoreQuality)
                                        {
                                            blnRequirementForbidden = true;
                                            strForbidden += "\n\t" + objQuality.DisplayNameShort;
                                        }
                                    }
                                    break;

                                case "metatype":
                                    // Check the Metatype restriction.
                                    if (objXmlForbidden.InnerText == objCharacter.Metatype)
                                    {
                                        blnRequirementForbidden = true;
                                        XmlNode objNode =
                                            objMetatypeDocument.SelectSingleNode(
                                                "/chummer/metatypes/metatype[name = \"" + objXmlForbidden.InnerText +
                                                "\"]") ??
                                            objCritterDocument.SelectSingleNode(
                                                "/chummer/metatypes/metatype[name = \"" + objXmlForbidden.InnerText +
                                                "\"]");
                                        strForbidden += objNode["translate"] != null
                                            ? "\n\t" + objNode["translate"].InnerText
                                            : "\n\t" + objXmlForbidden.InnerText;
                                    }
                                    break;

                                case "metatypecategory":
                                    // Check the Metatype Category restriction.
                                    if (objXmlForbidden.InnerText == objCharacter.MetatypeCategory)
                                    {
                                        blnRequirementForbidden = true;
                                        XmlNode objNode =
                                            objMetatypeDocument.SelectSingleNode(
                                                "/chummer/categories/category[. = \"" + objXmlForbidden.InnerText +
                                                "\"]") ??
                                            objCritterDocument.SelectSingleNode(
                                                "/chummer/categories/category[. = \"" + objXmlForbidden.InnerText +
                                                "\"]");
                                        strForbidden += objNode.Attributes["translate"] != null
                                            ? "\n\t" + objNode.Attributes["translate"].InnerText
                                            : "\n\t" + objXmlForbidden.InnerText;
                                    }
                                    break;

                                case "metavariant":
                                    // Check the Metavariant restriction.
                                    if (objXmlForbidden.InnerText == objCharacter.Metavariant)
                                    {
                                        blnRequirementForbidden = true;
                                        XmlNode objNode =
                                            objMetatypeDocument.SelectSingleNode(
                                                "/chummer/metatypes/metatype/metavariants/metavariant[name = \"" +
                                                objXmlForbidden.InnerText + "\"]") ??
                                            objCritterDocument.SelectSingleNode(
                                                "/chummer/metatypes/metatype/metavariants/metavariant[name = \"" +
                                                objXmlForbidden.InnerText + "\"]");
                                        strForbidden += objNode["translate"] != null
                                            ? "\n\t" + objNode["translate"].InnerText
                                            : "\n\t" + objXmlForbidden.InnerText;
                                    }
                                    break;

                                case "metagenetic":
                                    // Check to see if the character has a Metagenetic Quality.
                                    foreach (Quality objQuality in objCharacter.Qualities)
                                    {
                                        XmlNode objXmlCheck =
                                            objQualityDocument.SelectSingleNode(
                                                "/chummer/qualities/quality[name = \"" + objQuality.Name + "\"]");
                                        if (objXmlCheck["metagenetic"] == null) continue;
                                        if (objXmlCheck["metagenetic"].InnerText.ToLower() != "yes") continue;
                                        blnRequirementForbidden = true;
                                        strForbidden += "\n\t" + objQuality.DisplayName;
                                        break;
                                    }
                                    break;
                            }
                        }
                    }

                // The character is not allowed to take the Quality, so display a message and uncheck the item.
                if (blnRequirementForbidden)
                {
                    if (blnShowMessage)
                        MessageBox.Show(
                            LanguageManager.Instance.GetString("Message_SelectQuality_QualityRestriction") +
                            strForbidden,
                            LanguageManager.Instance.GetString("MessageTitle_SelectQuality_QualityRestriction"),
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
            }

            if (objXmlNode.InnerXml.Contains("required"))
            {
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
                        bool blnFoundThis;
                        switch (objXmlRequired.Name)
                        {
                            case "quality":
                                // Run through all of the Qualities the character has and see if the current required item exists.
                                // If so, turn on the RequirementMet flag so it can be selected.
                                foreach (Quality objCharacterQuality in objCharacter.Qualities)
                                {
                                    if (objCharacterQuality.Name == objXmlRequired.InnerText)
                                        blnOneOfMet = true;
                                }

                                if (!blnOneOfMet)
                                {
                                    XmlNode objNode =
                                        objQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" +
                                                                            objXmlRequired.InnerText + "\"]");
                                    strThisRequirement += objNode["translate"] != null
                                        ? "\n\t" + objNode["translate"].InnerText
                                        : "\n\t" + objXmlRequired.InnerText;
                                }
                                break;

                            case "metatype":
                                // Check the Metatype requirement.
                                if (objXmlRequired.InnerText == objCharacter.Metatype)
                                    blnOneOfMet = true;
                                else
                                {
                                    XmlNode objNode =
                                        objMetatypeDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + objXmlRequired.InnerText + "\"]") ??
                                        objCritterDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + objXmlRequired.InnerText + "\"]");
                                    strThisRequirement += objNode["translate"] != null
                                        ? "\n\t" + objNode["translate"].InnerText
                                        : "\n\t" + objXmlRequired.InnerText;
                                }
                                break;

                            case "metatypecategory":
                                // Check the Metatype Category requirement.
                                if (objXmlRequired.InnerText == objCharacter.MetatypeCategory)
                                    blnOneOfMet = true;
                                else
                                {
                                    XmlNode objNode =
                                        objMetatypeDocument.SelectSingleNode("/chummer/categories/category[. = \"" +
                                                                             objXmlRequired.InnerText + "\"]") ??
                                        objCritterDocument.SelectSingleNode("/chummer/categories/category[. = \"" +
                                                                            objXmlRequired.InnerText + "\"]");
                                    strThisRequirement += objNode["translate"] != null
                                        ? "\n\t" + objNode["translate"].InnerText
                                        : "\n\t" + objXmlRequired.InnerText;
                                }
                                break;

                            case "metavariant":
                                // Check the Metavariant requirement.
                                if (objXmlRequired.InnerText == objCharacter.Metavariant)
                                    blnOneOfMet = true;
                                else
                                {
                                    XmlNode objNode =
                                        objMetatypeDocument.SelectSingleNode("/chummer/metatypes/metatype/metavariants/metavariant[name = \"" + objXmlRequired.InnerText + "\"]") ??
                                        objCritterDocument.SelectSingleNode("/chummer/metatypes/metatype/metavariants/metavariant[name = \"" + objXmlRequired.InnerText + "\"]");
                                    strThisRequirement += objNode["translate"] != null
                                        ? "\n\t" + objNode["translate"].InnerText
                                        : "\n\t" + objXmlRequired.InnerText;
                                }
                                break;

                            case "power":
                                // Run through all of the Powers the character has and see if the current required item exists.
                                // If so, turn on the RequirementMet flag so it can be selected.
                                if (objCharacter.AdeptEnabled && objCharacter.Powers != null)
                                {
                                    foreach (Power objCharacterPower in objCharacter.Powers)
                                    {
                                        //Check that the power matches the name and doesn't come from a bonus source like a focus. There's probably an edge case that this will break.
                                        if (objXmlRequired.InnerText == objCharacterPower.Name &&
                                            objCharacterPower.BonusSource.Length == 0)
                                        {
                                            blnOneOfMet = true;
                                        }
                                    }

                                    if (!blnOneOfMet)
                                    {
                                        strThisRequirement += "\n\t" + objXmlRequired.InnerText;
                                    }
                                }
                                break;

                            case "inherited":
                                strThisRequirement += "\n\t" +
                                                      LanguageManager.Instance.GetString(
                                                          "Message_SelectQuality_Inherit");
                                break;

                            case "careerkarma":
                                // Check Career Karma requirement.
                                if (objCharacter.CareerKarma >= Convert.ToInt32(objXmlRequired.InnerText))
                                    blnOneOfMet = true;
                                else
                                    strThisRequirement = "\n\t" +
                                                         LanguageManager.Instance.GetString(
                                                                 "Message_SelectQuality_RequireKarma")
                                                             .Replace("{0}", objXmlRequired.InnerText);
                                break;

                            case "ess":
                                if (objXmlRequired.Attributes["grade"] != null)
                                {
                                    decimal decGrade =
                                        objCharacter.Cyberware.Where(
                                                objCyberware =>
                                                    objCyberware.Grade.Name ==
                                                    objXmlRequired.Attributes?["grade"].InnerText)
                                            .Sum(objCyberware => objCyberware.CalculatedESS);
                                    if (objXmlRequired.InnerText.StartsWith("-"))
                                    {
                                        // Essence must be less than the value.
                                        if (decGrade <
                                            Convert.ToDecimal(objXmlRequired.InnerText.Replace("-", string.Empty),
                                                GlobalOptions.InvariantCultureInfo))
                                            blnOneOfMet = true;
                                        else
                                        {
                                            strThisRequirement = "\n\t" +
                                                                 LanguageManager.Instance.GetString(
                                                                         "Message_SelectQuality_RequireESSGradeBelow")
                                                                     .Replace("{0}", objXmlRequired.InnerText)
                                                                     .Replace("{1}", objXmlRequired.Attributes["grade"].InnerText)
                                                                     .Replace("{2}", decGrade.ToString(CultureInfo.InvariantCulture));
                                        }
                                    }
                                    else
                                    {
                                        // Essence must be equal to or greater than the value.
                                        if (decGrade >=
                                            Convert.ToDecimal(objXmlRequired.InnerText,
                                                GlobalOptions.InvariantCultureInfo))
                                            blnOneOfMet = true;
                                        else
                                        {
                                            strThisRequirement = "\n\t" +
                                                                 LanguageManager.Instance.GetString(
                                                                         "Message_SelectQuality_RequireESSGradeAbove")
                                                                     .Replace("{0}", objXmlRequired.InnerText)
                                                                     .Replace("{1}", objXmlRequired.Attributes["grade"].InnerText)
                                                                     .Replace("{2}", decGrade.ToString(CultureInfo.InvariantCulture));
                                        }
                                    }
                                }
                                // Check Essence requirement.
                                else if (objXmlRequired.InnerText.StartsWith("-"))
                                {
                                    // Essence must be less than the value.
                                    if (objCharacter.Essence <
                                        Convert.ToDecimal(objXmlRequired.InnerText.Replace("-", string.Empty),
                                            GlobalOptions.InvariantCultureInfo))
                                        blnOneOfMet = true;
                                    else
                                    {
                                        strThisRequirement = "\n\t" +
                                                             LanguageManager.Instance.GetString(
                                                                     "Message_SelectQuality_RequireESSBelow")
                                                                 .Replace("{0}", objXmlRequired.InnerText)
                                                                 .Replace("{1}", objCharacter.Essence.ToString(CultureInfo.InvariantCulture));
                                    }
                                }
                                else
                                {
                                    // Essence must be equal to or greater than the value.
                                    if (objCharacter.Essence >=
                                        Convert.ToDecimal(objXmlRequired.InnerText,
                                            GlobalOptions.InvariantCultureInfo))
                                        blnOneOfMet = true;
                                    else
                                    {
                                        strThisRequirement = "\n\t" +
                                                             LanguageManager.Instance.GetString(
                                                                     "Message_SelectQuality_RequireESSAbove")
                                                                 .Replace("{0}", objXmlRequired.InnerText)
                                                                 .Replace("{1}", objCharacter.Essence.ToString(CultureInfo.InvariantCulture));
                                    }
                                }
                                break;

                            case "skill":
                                // Check if the character has the required Skill.
                                blnFoundThis =
                                    objCharacter.SkillsSection.Skills.Where(
                                            objSkill => objSkill.Name == objXmlRequired["name"]?.InnerText)
                                        .Any(
                                            objSkill =>
                                                objSkill.Rating >=
                                                Convert.ToInt32(objXmlRequired["val"]?.InnerText));
                                if (blnFoundThis)
                                {
                                    blnOneOfMet = true;
                                }
                                else
                                {
                                    strThisRequirement +=
                                        $"\n\t{objXmlRequired["name"].InnerText} {objXmlRequired["val"].InnerText}";
                                }
                                break;

                            case "attribute":
                                // Check to see if an Attribute meets a requirement.
                                CharacterAttrib objAttribute =
                                    objCharacter.GetAttribute(objXmlRequired["name"].InnerText);
                                blnFoundThis = false;
                                if (objXmlRequired["total"] != null)
                                {
                                    // Make sure the Attribute's total value meets the requirement.
                                    if (objAttribute.TotalValue >=
                                        Convert.ToInt32(objXmlRequired["total"].InnerText))
                                        blnFoundThis = true;
                                }
                                if (blnFoundThis)
                                {
                                    blnOneOfMet = true;
                                }
                                else
                                {
                                    strThisRequirement +=
                                        $"\n\t{objAttribute.Abbrev} {objXmlRequired["total"].InnerText}";
                                }
                                break;

                            case "attributetotal":
                                // Check if the character's Attributes add up to a particular total.
                                string strAttributes = objXmlRequired["attributes"].InnerText;
                                foreach (string strAttribute in Character.AttributeStrings)
                                {
                                    strAttributes = strAttributes.Replace(strAttribute,
                                        objCharacter.GetAttribute(strAttribute).Value.ToString());
                                }

                                XmlDocument objXmlDocument = new XmlDocument();
                                XPathNavigator nav = objXmlDocument.CreateNavigator();
                                XPathExpression xprAttributes = nav.Compile(strAttributes);
                                if (Convert.ToInt32(nav.Evaluate(xprAttributes)) >=
                                    Convert.ToInt32(objXmlRequired["val"].InnerText))
                                    blnOneOfMet = true;
                                else
                                {
                                    strThisRequirement += $"\n\t{strAttributes} {objXmlRequired["val"].InnerText}";
                                }
                                break;

                            case "skillgrouptotal":
                            {
                                // Check if the total combined Ratings of Skill Groups adds up to a particular total.
                                int intTotal = 0;
                                string[] strGroups = objXmlRequired["skillgroups"].InnerText.Split('+');
                                for (int i = 0; i <= strGroups.Length - 1; i++)
                                {
                                    foreach (SkillGroup objGroup in objCharacter.SkillsSection.SkillGroups)
                                    {
                                        if (objGroup.Name == strGroups[i])
                                        {
                                            intTotal += objGroup.Rating;
                                            break;
                                        }
                                    }
                                }

                                if (intTotal >= Convert.ToInt32(objXmlRequired["val"].InnerText))
                                    blnOneOfMet = true;
                            }
                                break;

                            case "cyberwares":
                            {
                                // Check to see if the character has a number of the required Cyberware/Bioware items.
                                int intTotal = 0;

                                // Check Cyberware.
                                foreach (XmlNode objXmlCyberware in objXmlRequired.SelectNodes("cyberware"))
                                {
                                    blnFoundThis =
                                        objCharacter.Cyberware.Where(
                                                objCyberware => objCyberware.Name == objXmlCyberware.InnerText)
                                            .Any(
                                                objCyberware =>
                                                    objXmlCyberware.Attributes?["select"] == null ||
                                                    objXmlCyberware.Attributes["select"].InnerText ==
                                                    objCyberware.Location);
                                    if (blnFoundThis)
                                    {
                                        intTotal++;
                                    }
                                    else
                                    {
                                        strThisRequirement += "\n\t" +
                                                              LanguageManager.Instance.GetString("Label_Cyberware") +
                                                              objXmlRequired.InnerText;
                                    }
                                }

                                foreach (XmlNode objXmlCyberware in objXmlRequired.SelectNodes("bioware"))
                                {
                                    blnFoundThis =
                                        objCharacter.Cyberware.Where(
                                                objCyberware => objCyberware.Name == objXmlCyberware.InnerText)
                                            .Any(
                                                objCyberware =>
                                                    objXmlCyberware.Attributes?["select"] == null ||
                                                    objXmlCyberware.Attributes["select"].InnerText ==
                                                    objCyberware.Location);
                                    if (blnFoundThis)
                                    {
                                        intTotal++;
                                    }
                                    else
                                    {
                                        strThisRequirement += "\n\t" +
                                                              LanguageManager.Instance.GetString("Label_Bioware") +
                                                              objXmlRequired.InnerText;
                                    }
                                }

                                // Check Cyberware name that contain a straing.
                                foreach (XmlNode objXmlCyberware in objXmlRequired.SelectNodes("cyberwarecontains"))
                                {
                                    foreach (Cyberware objCyberware in objCharacter.Cyberware)
                                    {
                                        if (objCyberware.Name.Contains(objXmlCyberware.InnerText))
                                        {
                                            if (objXmlCyberware.Attributes["select"] == null)
                                            {
                                                intTotal++;
                                                break;
                                            }
                                            else if (objXmlCyberware.Attributes["select"].InnerText ==
                                                     objCyberware.Location)
                                            {
                                                intTotal++;
                                                break;
                                            }
                                        }
                                    }
                                }

                                // Check Bioware name that contain a straing.
                                foreach (XmlNode objXmlCyberware in objXmlRequired.SelectNodes("biowarecontains"))
                                {
                                    foreach (Cyberware objCyberware in objCharacter.Cyberware)
                                    {
                                        if (objCyberware.Name.Contains(objXmlCyberware.InnerText))
                                        {
                                            if (objXmlCyberware.Attributes["select"] == null)
                                            {
                                                intTotal++;
                                                break;
                                            }
                                            else if (objXmlCyberware.Attributes["select"].InnerText ==
                                                     objCyberware.Location)
                                            {
                                                intTotal++;
                                                break;
                                            }
                                        }
                                    }
                                }

                                // Check for Cyberware Plugins.
                                foreach (XmlNode objXmlCyberware in objXmlRequired.SelectNodes("cyberwareplugin"))
                                {
                                    intTotal += objCharacter.Cyberware.Count(objCyberware => objCyberware.Children.Any(objPlugin => objPlugin.Name == objXmlCyberware.InnerText));
                                }

                                // Check for Cyberware Categories.
                                foreach (XmlNode objXmlCyberware in objXmlRequired.SelectNodes("cyberwarecategory"))
                                {
                                    intTotal += objCharacter.Cyberware.Count(objCyberware => objCyberware.Category == objXmlCyberware.InnerText);
                                }

                                if (intTotal >= Convert.ToInt32(objXmlRequired["count"].InnerText))
                                    blnOneOfMet = true;
                            }
                                break;

                            case "streetcredvsnotoriety":
                                // Street Cred must be higher than Notoriety.
                                if (objCharacter.StreetCred >= objCharacter.Notoriety)
                                    blnOneOfMet = true;
                                break;

                            case "damageresistance":
                                // Damage Resistance must be a particular value.
                                ImprovementManager objImprovementManager = new ImprovementManager(objCharacter);
                                if (objCharacter.BOD.TotalValue +
                                    objImprovementManager.ValueOf(Improvement.ImprovementType.DamageResistance) >=
                                    Convert.ToInt32(objXmlRequired.InnerText))
                                    blnOneOfMet = true;
                                break;
                        }
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
                        bool blnFound = false;
                        switch (objXmlRequired.Name)
                        {
                            case "quality":
                                // Run through all of the Qualities the character has and see if the current required item exists.
                                // If so, turn on the RequirementMet flag so it can be selected.
                                foreach (Quality objCharacterQuality in objCharacter.Qualities)
                                {
                                    if (objCharacterQuality.Name == objXmlRequired.InnerText)
                                        blnFound = true;
                                }

                                if (!blnFound)
                                {
                                    XmlNode objNode =
                                        objQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" +
                                                                            objXmlRequired.InnerText + "\"]");
                                    if (objNode["translate"] != null)
                                        strThisRequirement += "\n\t" + objNode["translate"].InnerText;
                                    else
                                        strThisRequirement += "\n\t" + objXmlRequired.InnerText;
                                }
                                break;

                            case "metatype":
                                // Check the Metatype requirement.
                                if (objXmlRequired.InnerText == objCharacter.Metatype)
                                    blnFound = true;
                                else
                                {
                                    XmlNode objNode =
                                        objMetatypeDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + objXmlRequired.InnerText + "\"]") ??
                                        objCritterDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + objXmlRequired.InnerText + "\"]");
                                    strThisRequirement += objNode["translate"] != null
                                        ? "\n\t" + objNode["translate"].InnerText
                                        : "\n\t" + objXmlRequired.InnerText;
                                }
                                break;

                            case "metatypecategory":
                                // Check the Metatype Category requirement.
                                if (objXmlRequired.InnerText == objCharacter.MetatypeCategory)
                                    blnFound = true;
                                else
                                {
                                    XmlNode objNode =
                                        objMetatypeDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + objXmlRequired.InnerText + "\"]") ??
                                        objCritterDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + objXmlRequired.InnerText + "\"]");
                                    strThisRequirement += objNode["translate"] != null
                                        ? "\n\t" + objNode["translate"].InnerText
                                        : "\n\t" + objXmlRequired.InnerText;
                                }
                                break;

                            case "metavariant":
                                // Check the Metavariant requirement.
                                if (objXmlRequired.InnerText == objCharacter.Metavariant)
                                    blnFound = true;
                                else
                                {
                                    XmlNode objNode =
                                        objMetatypeDocument.SelectSingleNode("/chummer/metatypes/metatype/metavariants/metavariant[name = \"" + objXmlRequired.InnerText + "\"]") ??
                                        objCritterDocument.SelectSingleNode("/chummer/metatypes/metatype/metavariants/metavariant[name = \"" + objXmlRequired.InnerText + "\"]");
                                    strThisRequirement += objNode["translate"] != null
                                        ? "\n\t" + objNode["translate"].InnerText
                                        : "\n\t" + objXmlRequired.InnerText;
                                }
                                break;

                            case "inherited":
                                strThisRequirement += "\n\t" +
                                                      LanguageManager.Instance.GetString(
                                                          "Message_SelectQuality_Inherit");
                                break;

                            case "careerkarma":
                                // Check Career Karma requirement.
                                if (objCharacter.CareerKarma >= Convert.ToInt32(objXmlRequired.InnerText))
                                    blnFound = true;
                                else
                                    strThisRequirement = "\n\t" +
                                                         LanguageManager.Instance.GetString(
                                                                 "Message_SelectQuality_RequireKarma")
                                                             .Replace("{0}", objXmlRequired.InnerText);
                                break;

                            case "ess":
                                // Check Essence requirement.
                                if (objXmlRequired.InnerText.StartsWith("-"))
                                {
                                    // Essence must be less than the value.
                                    if (objCharacter.Essence <
                                        Convert.ToDecimal(objXmlRequired.InnerText.Replace("-", string.Empty),
                                            GlobalOptions.InvariantCultureInfo))
                                        blnFound = true;
                                }
                                else
                                {
                                    // Essence must be equal to or greater than the value.
                                    if (objCharacter.Essence >=
                                        Convert.ToDecimal(objXmlRequired.InnerText,
                                            GlobalOptions.InvariantCultureInfo))
                                        blnFound = true;
                                }
                                break;

                            case "skill":
                                // Check if the character has the required Skill.
                                if (objCharacter.SkillsSection.Skills.Where(objSkill => objSkill.Name == objXmlRequired["name"]?.InnerText).Any(objSkill => objSkill.Rating >= Convert.ToInt32(objXmlRequired["val"]?.InnerText)))
                                {
                                    blnFound = true;
                                }
                                if (!blnFound)
                                {
                                    strThisRequirement += "\n\t" + objXmlRequired.InnerText;
                                }
                                break;

                            case "attribute":
                                // Check to see if an Attribute meets a requirement.
                                CharacterAttrib objAttribute =
                                    objCharacter.GetAttribute(objXmlRequired["name"].InnerText);

                                if (objXmlRequired["total"] != null)
                                {
                                    // Make sure the Attribute's total value meets the requirement.
                                    if (objAttribute.TotalValue >=
                                        Convert.ToInt32(objXmlRequired["total"].InnerText))
                                        blnFound = true;
                                }
                                if (!blnFound)
                                {
                                    strThisRequirement += "\n\t" + objXmlRequired.InnerText;
                                }
                                break;

                            case "attributetotal":
                                // Check if the character's Attributes add up to a particular total.
                                string strAttributes = objXmlRequired["attributes"].InnerText;
                                foreach (string strAttribute in Character.AttributeStrings)
                                {
                                    strAttributes = strAttributes.Replace(strAttribute,
                                        objCharacter.GetAttribute(strAttribute).Value.ToString());
                                }

                                XmlDocument objXmlDocument = new XmlDocument();
                                XPathNavigator nav = objXmlDocument.CreateNavigator();
                                XPathExpression xprAttributes = nav.Compile(strAttributes);
                                if (Convert.ToInt32(nav.Evaluate(xprAttributes)) >=
                                    Convert.ToInt32(objXmlRequired["val"].InnerText))
                                    blnFound = true;
                                if (!blnFound)
                                {
                                    strThisRequirement += "\n\t" + objXmlRequired.InnerText;
                                }
                                break;

                            case "skillgrouptotal":
                            {
                                // Check if the total combined Ratings of Skill Groups adds up to a particular total.
                                int intTotal = 0;
                                string[] strGroups = objXmlRequired["skillgroups"].InnerText.Split('+');
                                for (int i = 0; i <= strGroups.Length - 1; i++)
                                {
                                    foreach (SkillGroup objGroup in objCharacter.SkillsSection.SkillGroups)
                                    {
                                        if (objGroup.Name == strGroups[i])
                                        {
                                            intTotal += objGroup.Rating;
                                            break;
                                        }
                                    }
                                }

                                if (intTotal >= Convert.ToInt32(objXmlRequired["val"].InnerText))
                                    blnFound = true;
                                if (!blnFound)
                                {
                                    strThisRequirement += "\n\t" + objXmlRequired.InnerText;
                                }
                            }
                                break;

                            case "cyberwares":
                            {
                                // Check to see if the character has a number of the required Cyberware/Bioware items.
                                int intTotal = 0;

                                // Check Cyberware.
                                foreach (XmlNode objXmlCyberware in objXmlRequired.SelectNodes("cyberware"))
                                {
                                    bool blnFoundThis =
                                        objCharacter.Cyberware.Where(
                                                objCyberware => objCyberware.Name == objXmlCyberware.InnerText)
                                            .Any(
                                                objCyberware =>
                                                    objXmlCyberware.Attributes?["select"] == null ||
                                                    objXmlCyberware.Attributes["select"].InnerText ==
                                                    objCyberware.Location);
                                    if (blnFoundThis)
                                    {
                                        intTotal++;
                                    }
                                    else
                                    {
                                        strThisRequirement += "\n\t" +
                                                              LanguageManager.Instance.GetString("Label_Cyberware") +
                                                              objXmlRequired.InnerText;
                                    }
                                }

                                // Check Bioware.
                                foreach (XmlNode objXmlBioware in objXmlRequired.SelectNodes("bioware"))
                                {
                                    bool blnFoundThis =
                                        objCharacter.Cyberware.Any(
                                            objCyberware => objCyberware.Name == objXmlBioware.InnerText);
                                    if (blnFoundThis)
                                    {
                                        intTotal++;
                                    }
                                    else
                                    {
                                        strThisRequirement += "\n\t" +
                                                              LanguageManager.Instance.GetString("Label_Bioware") +
                                                              objXmlRequired.InnerText;
                                    }
                                }

                                // Check Cyberware name that contain a straing.
                                foreach (XmlNode objXmlCyberware in objXmlRequired.SelectNodes("cyberwarecontains"))
                                {
                                    bool blnFoundThis = false;
                                    foreach (
                                        Cyberware objCyberware in
                                        objCharacter.Cyberware.Where(
                                            objCyberware => objCyberware.Name.Contains(objXmlCyberware.InnerText)))
                                    {
                                        blnFoundThis = objXmlCyberware.Attributes?["select"] == null ||
                                                       objXmlCyberware.Attributes["select"].InnerText ==
                                                       objCyberware.Location;
                                    }
                                    if (blnFoundThis)
                                    {
                                        intTotal++;
                                    }
                                    else
                                    {
                                        strThisRequirement += "\n\t" +
                                                              LanguageManager.Instance.GetString("Label_Cyberware") +
                                                              objXmlRequired.InnerText;
                                    }
                                }

                                // Check Bioware name that contain a straing.
                                foreach (XmlNode objXmlCyberware in objXmlRequired.SelectNodes("biowarecontains"))
                                {
                                    bool blnFoundThis = false;
                                    foreach (
                                        Cyberware objCyberware in
                                        objCharacter.Cyberware.Where(
                                            objCyberware => objCyberware.Name.Contains(objXmlCyberware.InnerText)))
                                    {
                                        blnFoundThis = objXmlCyberware.Attributes?["select"] == null ||
                                                       objXmlCyberware.Attributes["select"].InnerText ==
                                                       objCyberware.Location;
                                    }
                                    if (blnFoundThis)
                                    {
                                        intTotal++;
                                    }
                                    else
                                    {
                                        strThisRequirement += "\n\t" +
                                                              LanguageManager.Instance.GetString("Label_Bioware") +
                                                              objXmlRequired.InnerText;
                                    }
                                }

                                // Check for Cyberware Plugins.
                                foreach (XmlNode objXmlCyberware in objXmlRequired.SelectNodes("cyberwareplugin"))
                                {
                                    bool blnFoundThis = false;
                                    foreach (Cyberware objCyberware in objCharacter.Cyberware)
                                    {
                                        if (
                                            objCyberware.Children.Any(
                                                objPlugin => objPlugin.Name == objXmlCyberware.InnerText))
                                        {
                                            blnFoundThis = true;
                                        }
                                    }
                                    if (blnFoundThis)
                                    {
                                        intTotal++;
                                    }
                                    else
                                    {
                                        strThisRequirement += "\n\t" +
                                                              LanguageManager.Instance.GetString("Label_Cyberware") +
                                                              objXmlRequired.InnerText;
                                    }
                                }

                                // Check for Cyberware Categories.
                                foreach (XmlNode objXmlCyberware in objXmlRequired.SelectNodes("cyberwarecategory"))
                                {
                                    bool blnFoundThis =
                                        objCharacter.Cyberware.Any(
                                            objCyberware => objCyberware.Category == objXmlCyberware.InnerText);
                                    if (blnFoundThis)
                                    {
                                        intTotal++;
                                    }
                                    else
                                    {
                                        strThisRequirement += "\n\t" +
                                                              LanguageManager.Instance.GetString("Label_Cyberware") +
                                                              objXmlRequired.InnerText;
                                    }
                                }

                                if (intTotal >= Convert.ToInt32(objXmlRequired["count"].InnerText))
                                    blnFound = true;
                            }
                                break;

                            case "streetcredvsnotoriety":
                                // Street Cred must be higher than Notoriety.
                                if (objCharacter.StreetCred >= objCharacter.Notoriety)
                                    blnFound = true;
                                break;

                            case "damageresistance":
                                // Damage Resistance must be a particular value.
                                ImprovementManager objImprovementManager = new ImprovementManager(objCharacter);
                                if (objCharacter.BOD.TotalValue +
                                    objImprovementManager.ValueOf(Improvement.ImprovementType.DamageResistance) >=
                                    Convert.ToInt32(objXmlRequired.InnerText))
                                    blnFound = true;
                                break;
                        }

                        // If this item was not found, fail the AllOfMet condition.
                        if (!blnFound)
                            blnAllOfMet = false;
                    }

                    // Update the flag for requirements met.
                    blnRequirementMet = blnRequirementMet && blnAllOfMet;
                    strRequirement += strThisRequirement;
                }

                // The character has not met the requirements, so display a message and uncheck the item.
                if (!blnRequirementMet)
                {
                    string strMessage =
                        LanguageManager.Instance.GetString("Message_SelectQuality_QualityRequirement");
                    strMessage += strRequirement;

                    if (blnShowMessage)
                        MessageBox.Show(strMessage,
                            LanguageManager.Instance.GetString("MessageTitle_SelectQuality_QualityRequirement"),
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Evaluates the availability of a given node against Availability Limits in Create Mode
        /// </summary>
        /// <param name="objXmlGear"></param>
        /// <param name="_objCharacter"></param>
        /// <param name="intRating"></param>
        /// <param name="intAvailModifier"></param>
        /// <param name="blnAddToList"></param>
        /// <returns></returns>
        public static bool CheckAvailRestriction(XmlNode objXmlGear, Character _objCharacter, int intRating = 0, int intAvailModifier = 0, bool blnAddToList = true)
        {
            XmlDocument objXmlDocument = new XmlDocument();
            //TODO: Better handler for restricted gear
            if (_objCharacter.Options.HideItemsOverAvailLimit && !_objCharacter.Created && !_objCharacter.RestrictedGear && !_objCharacter.IgnoreRules && blnAddToList)
            {
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

                if (strAvailExpr.Substring(strAvailExpr.Length - 1, 1) == "F" || strAvailExpr.Substring(strAvailExpr.Length - 1, 1) == "R")
                {
                    strAvailExpr = strAvailExpr.Substring(0, strAvailExpr.Length - 1);
                }
                if (strAvailExpr.Substring(0, 1) == "+")
                {
                    strPrefix = "+";
                    strAvailExpr = strAvailExpr.Substring(1, strAvailExpr.Length - 1);
                }
                if (strPrefix != "+")
                {
                    try
                    {
                        XPathNavigator nav = objXmlDocument.CreateNavigator();
                        var xprAvail = nav.Compile(strAvailExpr.Replace("Rating",
                            intRating.ToString(GlobalOptions.InvariantCultureInfo)));
                        blnAddToList = Convert.ToInt32(nav.Evaluate(xprAvail)) + intAvailModifier <=
                                       _objCharacter.MaximumAvailability;
                    }
                    catch
                    {
                    }
                }
            }
            return blnAddToList;
        }
    }
}