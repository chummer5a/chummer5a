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
using System.Globalization;
using System.Xml;
using System.Xml.XPath;
using Chummer;
using Chummer.Backend.Enums;

namespace ChummerAlmanac
{
    /// <summary>
    /// A label and its value, e.g. an attribute.
    /// </summary>
    internal sealed class SpiritStatLine
    {
        public SpiritStatLine(string strLabel, string strValue)
        {
            Label = strLabel;
            Value = strValue;
        }

        public string Label { get; }
        public string Value { get; }
    }

    /// <summary>
    /// A skill of a spirit with its dice pool.
    /// </summary>
    internal sealed class SpiritSkillLine
    {
        public SpiritSkillLine(string strName, string strAttribute, string strPool)
        {
            Name = strName;
            Attribute = strAttribute;
            Pool = strPool;
        }

        public string Name { get; }
        public string Attribute { get; }
        public string Pool { get; }
    }

    /// <summary>
    /// An optional power a spirit can have. The key is the English name and is what gets saved.
    /// </summary>
    internal sealed class SpiritPowerEntry
    {
        public SpiritPowerEntry(string strKey, string strDisplayName, string strSource = "")
        {
            Key = strKey;
            DisplayName = strDisplayName;
            Source = strSource ?? string.Empty;
        }

        public string Key { get; }
        public string DisplayName { get; }

        /// <summary>
        /// Book code and page as Chummer's PDF function expects them, e.g. "SR5 397". Empty if the data has none.
        /// </summary>
        public string Source { get; }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Source) ? DisplayName : DisplayName + "  [" + Source + "]";
        }
    }

    /// <summary>
    /// Everything shown for one spirit, calculated from Chummer's data files (including custom data)
    /// in the same way Chummer does it for its character sheets.
    /// </summary>
    internal sealed class SpiritStats
    {
        // Attribute entries of a spirit in traditions.xml / streams.xml, in display order.
        private static readonly string[] s_astrAttributes =
            { "bod", "agi", "rea", "str", "cha", "int", "log", "wil", "edg", "mag", "ess", "res", "dep" };

        // Attributes that traditions.xml usually leaves out, and where critters.xml has them instead.
        private static readonly Dictionary<string, string> s_dicCritterFallbackAttributes
            = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "edg", "edgmin" },
                { "mag", "magmin" },
                { "ess", "essmax" }
            };

        private const string InitiativeAttribute = "ini";

        public string TypeName { get; private set; } = string.Empty;
        public string Source { get; private set; } = string.Empty;
        public List<SpiritStatLine> Attributes { get; } = new List<SpiritStatLine>();
        public List<SpiritPowerEntry> Powers { get; } = new List<SpiritPowerEntry>();
        public List<SpiritPowerEntry> OptionalPowers { get; } = new List<SpiritPowerEntry>();
        public List<SpiritSkillLine> Skills { get; } = new List<SpiritSkillLine>();

        /// <summary>
        /// How many optional powers may be chosen. Negative if there is no limit to show.
        /// </summary>
        public int OptionalPowerLimit { get; private set; } = -1;

        /// <summary>
        /// Calculate the values of a spirit. Returns null if Chummer has no data for this type of spirit.
        /// </summary>
        public static SpiritStats Build(Spirit objSpirit, Character objCharacter)
        {
            if (objSpirit == null || objCharacter == null)
                return null;
            string strLanguage = GlobalSettings.Language;
            XmlNode xmlSpirit = objSpirit.GetNode(strLanguage);
            if (xmlSpirit == null)
                return null;

            int intForce = objSpirit.Force;
            bool blnIsSpirit = objSpirit.EntityType == SpiritType.Spirit;
            CultureInfo objCulture = GlobalSettings.CultureInfo ?? CultureInfo.CurrentCulture;
            string strD6 = LanguageManager.GetString("String_D6", false);
            if (string.IsNullOrEmpty(strD6))
                strD6 = "D6";

            SpiritStats objStats = new SpiritStats
            {
                TypeName = GetChildText(xmlSpirit, "translate") ?? objSpirit.Name
            };

            string strSource = GetChildText(xmlSpirit, "source");
            string strPage = GetChildText(xmlSpirit, "altpage") ?? GetChildText(xmlSpirit, "page");
            if (!string.IsNullOrEmpty(strSource))
                objStats.Source = string.IsNullOrEmpty(strPage) ? strSource : strSource + " " + strPage;

            // Attributes. Values are formulas like "F+3" where F is the Force.
            Dictionary<string, int> dicAttributeValues = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            // Edge, Magic and Essence of most spirits are only part of their critter entry, so look there as well.
            XPathNavigator xmlCritter = blnIsSpirit ? FindCritter(objCharacter, objSpirit.Name, strLanguage) : null;
            foreach (string strAttribute in s_astrAttributes)
            {
                string strFormula = GetChildText(xmlSpirit, strAttribute);
                if (strFormula == null && xmlCritter != null
                    && s_dicCritterFallbackAttributes.TryGetValue(strAttribute, out string strCritterElement))
                    strFormula = xmlCritter.SelectSingleNode(strCritterElement)?.Value;
                // A plain 0 means the spirit does not have this attribute at all (e.g. physical attributes of sprites).
                if (string.IsNullOrWhiteSpace(strFormula) || strFormula.Trim() == "0")
                    continue;
                int intValue = EvaluateFormula(strFormula, intForce);
                dicAttributeValues[strAttribute] = intValue;
                objStats.Attributes.Add(new SpiritStatLine(GetAttributeLabel(strAttribute),
                    intValue.ToString(objCulture)));
            }

            // Initiative
            string strInitiativeFormula = GetChildText(xmlSpirit, InitiativeAttribute);
            if (!string.IsNullOrWhiteSpace(strInitiativeFormula))
            {
                int intInitiative = EvaluateFormula(strInitiativeFormula, intForce);
                // Materialized spirits roll 2D6 for Initiative and 3D6 on the astral plane with (Force x 2) (SR5 303).
                string strInitiative = intInitiative.ToString(objCulture);
                if (blnIsSpirit)
                    strInitiative += " + 2" + strD6;
                objStats.Attributes.Add(new SpiritStatLine(GetAttributeLabel(InitiativeAttribute), strInitiative));
                if (blnIsSpirit)
                {
                    objStats.Attributes.Add(new SpiritStatLine(AlmanacStrings.Get("Label_AstralInitiative"),
                        (intForce * 2).ToString(objCulture) + " + 3" + strD6));
                }
            }

            // Powers and weaknesses
            XPathNavigator xmlSpiritPowers = LoadDataRoot(objCharacter, "spiritpowers.xml", strLanguage);
            XPathNavigator xmlCritterPowers = LoadDataRoot(objCharacter, "critterpowers.xml", strLanguage);
            XmlNode xmlPowers = xmlSpirit["powers"];
            if (xmlPowers != null)
            {
                foreach (XmlNode xmlPower in xmlPowers.ChildNodes)
                {
                    if (xmlPower.NodeType != XmlNodeType.Element)
                        continue;
                    string strDisplayName = GetPowerDisplayName(xmlPower, xmlSpiritPowers, xmlCritterPowers);
                    objStats.Powers.Add(new SpiritPowerEntry(GetPowerKey(xmlPower), strDisplayName,
                        GetPowerSource(xmlPower, xmlSpiritPowers, xmlCritterPowers, strLanguage)));
                }
            }
            XmlNode xmlWeaknesses = xmlSpirit["weaknesses"];
            if (xmlWeaknesses != null)
            {
                foreach (XmlNode xmlWeakness in xmlWeaknesses.ChildNodes)
                {
                    if (xmlWeakness.NodeType != XmlNodeType.Element)
                        continue;
                    objStats.Powers.Add(new SpiritPowerEntry(GetPowerKey(xmlWeakness),
                        string.Format(objCulture, AlmanacStrings.Get("Label_Weakness"),
                            GetPowerDisplayName(xmlWeakness, xmlSpiritPowers, xmlCritterPowers)),
                        GetPowerSource(xmlWeakness, xmlSpiritPowers, xmlCritterPowers, strLanguage)));
                }
            }

            // Optional powers
            XmlNode xmlOptionalPowers = xmlSpirit["optionalpowers"];
            if (xmlOptionalPowers != null)
            {
                foreach (XmlNode xmlPower in xmlOptionalPowers.ChildNodes)
                {
                    if (xmlPower.NodeType != XmlNodeType.Element)
                        continue;
                    string strKey = GetPowerKey(xmlPower);
                    if (string.IsNullOrEmpty(strKey))
                        continue;
                    objStats.OptionalPowers.Add(new SpiritPowerEntry(strKey,
                        GetPowerDisplayName(xmlPower, xmlSpiritPowers, xmlCritterPowers),
                        GetPowerSource(xmlPower, xmlSpiritPowers, xmlCritterPowers, strLanguage)));
                }
                // A spirit gets one optional power for every 3 full points of Force (SR5 303).
                if (blnIsSpirit && objStats.OptionalPowers.Count > 0)
                    objStats.OptionalPowerLimit = Math.Max(intForce / 3, 0);
            }

            // Skills. As in Chummer's character sheets, the dice pool is Force + linked attribute.
            XmlNode xmlSkills = xmlSpirit["skills"];
            if (xmlSkills != null)
            {
                XPathNavigator xmlSkillsData = LoadDataRoot(objCharacter, "skills.xml", strLanguage);
                foreach (XmlNode xmlSkill in xmlSkills.ChildNodes)
                {
                    if (xmlSkill.NodeType != XmlNodeType.Element)
                        continue;
                    string strEnglishName = xmlSkill.InnerText;
                    if (string.IsNullOrEmpty(strEnglishName))
                        continue;
                    string strName = TranslateFromData(xmlSkillsData, "/chummer/skills/skill", strEnglishName)
                                     ?? TranslateFromData(xmlSkillsData, "/chummer/knowledgeskills/skill", strEnglishName)
                                     ?? strEnglishName;
                    string strAttribute = xmlSkill.Attributes?["attr"]?.Value ?? string.Empty;
                    string strAttributeLabel = string.Empty;
                    int intPool = intForce;
                    if (!string.IsNullOrEmpty(strAttribute))
                    {
                        if (!dicAttributeValues.TryGetValue(strAttribute, out int intAttributeValue))
                            intAttributeValue = intForce;
                        intPool += intAttributeValue;
                        strAttributeLabel = GetAttributeLabel(strAttribute);
                    }
                    objStats.Skills.Add(new SpiritSkillLine(strName, strAttributeLabel, intPool.ToString(objCulture)));
                }
            }

            return objStats;
        }

        /// <summary>
        /// Evaluate a formula from the data files the way Chummer does: F is the Force,
        /// results are rounded up and never lower than 1.
        /// </summary>
        internal static int EvaluateFormula(string strFormula, int intForce)
        {
            string strExpression = strFormula.Replace("F", intForce.ToString(CultureInfo.InvariantCulture));
            (bool blnIsSuccess, object objResult) = CommonFunctions.EvaluateInvariantXPath(strExpression);
            int intValue = intForce;
            if (blnIsSuccess && objResult is double dblResult && !double.IsNaN(dblResult) && !double.IsInfinity(dblResult))
                intValue = Convert.ToInt32(dblResult >= 0 ? Math.Ceiling(dblResult) : Math.Floor(dblResult));
            return Math.Max(intValue, 1);
        }

        private static string GetChildText(XmlNode xmlParent, string strName)
        {
            XmlNode xmlChild = xmlParent?[strName];
            return xmlChild?.InnerText;
        }

        private static string GetAttributeLabel(string strAttribute)
        {
            string strUpper = strAttribute.ToUpperInvariant();
            string strLabel = LanguageManager.GetString("String_Attribute" + strUpper + "Long", false);
            return string.IsNullOrEmpty(strLabel) ? strUpper : strLabel;
        }

        private static XPathNavigator LoadDataRoot(Character objCharacter, string strFileName, string strLanguage)
        {
            try
            {
                return objCharacter.LoadDataXPath(strFileName, strLanguage);
            }
            catch (Exception e) when (e is XmlException || e is XPathException || e is System.IO.IOException)
            {
                return null;
            }
        }

        /// <summary>
        /// Find the critter entry (critters.xml) that belongs to a type of spirit. Returns null if there is none.
        /// </summary>
        private static XPathNavigator FindCritter(Character objCharacter, string strSpiritName, string strLanguage)
        {
            if (string.IsNullOrEmpty(strSpiritName))
                return null;
            XPathNavigator xmlCritters = LoadDataRoot(objCharacter, "critters.xml", strLanguage);
            if (xmlCritters == null)
                return null;
            try
            {
                return xmlCritters.SelectSingleNode("/chummer/metatypes/metatype[name = "
                                                    + strSpiritName.CleanXPath() + "]");
            }
            catch (XPathException)
            {
                return null;
            }
        }

        /// <summary>
        /// The English name of a power entry including its selection, e.g. "Elemental Attack (Air)".
        /// </summary>
        private static string GetPowerKey(XmlNode xmlPower)
        {
            string strName = xmlPower.InnerText ?? string.Empty;
            string strSelect = xmlPower.Attributes?["select"]?.Value;
            return string.IsNullOrEmpty(strSelect) ? strName : strName + " (" + strSelect + ")";
        }

        private static string GetPowerDisplayName(XmlNode xmlPower, XPathNavigator xmlSpiritPowers,
            XPathNavigator xmlCritterPowers)
        {
            string strName = xmlPower.InnerText ?? string.Empty;
            string strSelect = xmlPower.Attributes?["select"]?.Value;
            string strDisplayName = TranslatePower(strName, xmlSpiritPowers, xmlCritterPowers);
            return string.IsNullOrEmpty(strSelect) ? strDisplayName : strDisplayName + " (" + strSelect + ")";
        }

        /// <summary>
        /// Book code and page of a power from the data files, e.g. "SR5 397". Like Chummer's own source display this
        /// uses the page of the translated book when the data has one. Empty if the data has no source for the power.
        /// </summary>
        private static string GetPowerSource(XmlNode xmlPower, XPathNavigator xmlSpiritPowers,
            XPathNavigator xmlCritterPowers, string strLanguage)
        {
            string strName = xmlPower.InnerText ?? string.Empty;
            return GetSourceFromData(xmlSpiritPowers, strName, strLanguage)
                   ?? GetSourceFromData(xmlCritterPowers, strName, strLanguage)
                   ?? string.Empty;
        }

        private static string GetSourceFromData(XPathNavigator xmlData, string strEnglishName, string strLanguage)
        {
            if (xmlData == null || string.IsNullOrEmpty(strEnglishName))
                return null;
            try
            {
                XPathNavigator xmlPower = xmlData.SelectSingleNode("/chummer/powers/power[name = "
                                                                   + strEnglishName.CleanXPath() + "]");
                string strSource = xmlPower?.SelectSingleNode("source")?.Value;
                if (string.IsNullOrEmpty(strSource))
                    return null;
                string strPage = xmlPower.SelectSingleNode("page")?.Value;
                if (!string.Equals(strLanguage, GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                {
                    string strAltPage = xmlPower.SelectSingleNode("altpage")?.Value;
                    if (!string.IsNullOrEmpty(strAltPage))
                        strPage = strAltPage;
                }
                return string.IsNullOrEmpty(strPage) ? strSource : strSource + " " + strPage;
            }
            catch (XPathException)
            {
                return null;
            }
        }

        private static string TranslatePower(string strEnglishName, XPathNavigator xmlSpiritPowers,
            XPathNavigator xmlCritterPowers)
        {
            const string strPath = "/chummer/powers/power";
            string strTranslated = TranslateFromData(xmlSpiritPowers, strPath, strEnglishName)
                                   ?? TranslateFromData(xmlCritterPowers, strPath, strEnglishName);
            if (strTranslated != null)
                return strTranslated;
            // Entries like "Engulf (Air)" have no translation of their own: translate the part in front of the bracket.
            int intBracket = strEnglishName.IndexOf(" (", StringComparison.Ordinal);
            if (intBracket > 0)
            {
                string strBaseName = strEnglishName.Substring(0, intBracket);
                string strTranslatedBase = TranslateFromData(xmlSpiritPowers, strPath, strBaseName)
                                           ?? TranslateFromData(xmlCritterPowers, strPath, strBaseName);
                if (strTranslatedBase != null)
                    return strTranslatedBase + strEnglishName.Substring(intBracket);
            }
            return strEnglishName;
        }

        /// <summary>
        /// Look up the translated name of an entry in a data file. Returns null if there is none.
        /// </summary>
        private static string TranslateFromData(XPathNavigator xmlData, string strPath, string strEnglishName)
        {
            if (xmlData == null || string.IsNullOrEmpty(strEnglishName))
                return null;
            try
            {
                string strTranslated = xmlData
                    .SelectSingleNode(strPath + "[name = " + strEnglishName.CleanXPath() + "]/translate")?.Value;
                return string.IsNullOrEmpty(strTranslated) ? null : strTranslated;
            }
            catch (XPathException)
            {
                return null;
            }
        }
    }
}
