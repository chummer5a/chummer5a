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
using System.Xml;

namespace Chummer.Backend.Skills
{
    public class KnowledgeSkill : Skill
    {
        private static readonly Dictionary<string, string> s_CategoriesSkillMap = new Dictionary<string, string>();  //Categories to their attribtue

        public static IEnumerable<ListItem> DefaultKnowledgeSkills(string strLanguage)
        {
            using (XmlNodeList xmlSkillList = XmlManager.Load("skills.xml", strLanguage).SelectNodes("/chummer/knowledgeskills/skill"))
                if (xmlSkillList != null)
                    foreach (XmlNode xmlSkill in xmlSkillList)
                    {
                        string strName = xmlSkill["name"]?.InnerText ?? string.Empty;
                        yield return new ListItem(strName, xmlSkill["translate"]?.InnerText ?? strName);
                    }
        }

        /// <summary>
        /// Load the (possible translated) types of kno skills (Academic, Street...)
        /// </summary>
        /// <param name="strLanguage"></param>
        /// <returns></returns>
        public static IEnumerable<ListItem> KnowledgeTypes(string strLanguage)
        {
            using (XmlNodeList xmlCategoryList = XmlManager.Load("skills.xml", strLanguage).SelectNodes("/chummer/categories/category[@type = \"knowledge\"]"))
                if (xmlCategoryList != null)
                    foreach (XmlNode objXmlCategory in xmlCategoryList)
                    {
                        string strInnerText = objXmlCategory.InnerText;
                        yield return new ListItem(strInnerText, objXmlCategory.Attributes?["translate"]?.InnerText ?? strInnerText);
                    }
        }

        static KnowledgeSkill()
        {
            using (XmlNodeList xmlSkillList = XmlManager.Load("skills.xml").SelectNodes("/chummer/knowledgeskills/skill"))
                if (xmlSkillList != null)
                    foreach (XmlNode objXmlSkill in xmlSkillList)
                    {
                        string strCategory = objXmlSkill["category"]?.InnerText;
                        if (!string.IsNullOrWhiteSpace(strCategory))
                        {
                            s_CategoriesSkillMap[strCategory] = objXmlSkill["attribute"]?.InnerText;
                        }
                    }
        }

        public override bool AllowDelete => true;

        private string _strType = string.Empty;
        public bool ForcedName { get; }

        public KnowledgeSkill(Character character) : base(character)
        {
            AttributeObject = character.LOG;
        }

        public KnowledgeSkill(Character character, string forcedName) : this(character)
        {
            WriteableName = forcedName;
            ForcedName = true;
        }

        public string WriteableName
        {
            get => DisplayName;
            set
            {
                if (ForcedName)
                {
                    return;
                }

                if (string.Equals(DisplayName, value))
                {
                    return;
                }
                
                LoadSkill(value);
                Name = value;
                OnPropertyChanged();
            }
        }
        
        private void LoadSkill(string inputSkillName)
        {
            var skillName = GetSkillName(inputSkillName);
            var xmlSkillDoc = XmlManager.Load("skills.xml", GlobalOptions.Language);
            var skillNode = xmlSkillDoc.SelectSingleNode($"/chummer/knowledgeskills/skill[name = \"{ skillName }\"]");

            if (skillNode == null)
            {
                SkillId = Guid.Empty;
                return;
            }

            SkillId = skillNode.TryGetField("id", Guid.TryParse, out Guid guidTemp)
                ? guidTemp
                : Guid.Empty;

            var strCategory = skillNode["category"]?.InnerText;

            if (!string.IsNullOrEmpty(strCategory))
            {
                Type = strCategory;
            }

            var strAttribute = skillNode["attribute"]?.InnerText;

            if (!string.IsNullOrEmpty(strAttribute))
            {
                AttributeObject = CharacterObject.GetAttribute(strAttribute) ?? CharacterObject.LOG;
            }
        }

        private static string GetSkillName(string inputSkillName)
        {
            if (GlobalOptions.Language == GlobalOptions.DefaultLanguage)
            {
                return inputSkillName;
            }

            var result = GetDefaultLanguageSkillName(inputSkillName);
            return result;
        }

        private static string GetDefaultLanguageSkillName(string translatedSkillName)
        {
            var xmlSkillDoc = XmlManager.Load("skills.xml", GlobalOptions.Language);
            var skillTranslationNode = xmlSkillDoc.SelectSingleNode($"/chummer/knowledgeskills/skill[translate = \"{ translatedSkillName }\"]");
            
            if (skillTranslationNode == null)
            {
                return LanguageManager.ReverseTranslateExtra(translatedSkillName, GlobalOptions.Language);
            }

            return skillTranslationNode["name"]?.InnerText ?? translatedSkillName;
        }

        public override string SkillCategory => Type;

        public override string DisplayPool
        {
            get
            {
                if (Rating == 0 && Type == "Language")
                {
                    return LanguageManager.GetString("Skill_NativeLanguageShort",GlobalOptions.Language);
                }
                else
                {
                    return base.DisplayPool;
                }
            }
        }

        /// <summary>
        /// The attributeValue this skill have from Skilljacks + Knowsoft
        /// </summary>
        /// <returns>Artificial skill attributeValue</returns>
        public override int CyberwareRating
        {
            get
            {
                if (_intCachedCyberwareRating != int.MinValue)
                    return _intCachedCyberwareRating;

                string strTranslatedName = DisplayNameMethod(GlobalOptions.Language);
                //this might do hardwires if i understand how they works correctly
                int intMaxHardwire = -1;
                foreach (Improvement objImprovement in CharacterObject.Improvements)
                {
                    if (objImprovement.ImproveType == Improvement.ImprovementType.Hardwire && (objImprovement.ImprovedName == Name || objImprovement.ImprovedName == strTranslatedName) && objImprovement.Enabled && objImprovement.Value > intMaxHardwire)
                    {
                        intMaxHardwire = objImprovement.Value;
                    }
                }
                if (intMaxHardwire >= 0)
                {
                    return _intCachedCyberwareRating = intMaxHardwire;
                }

                int intMaxSkillsoftRating = ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.SkillsoftAccess);
                if (intMaxSkillsoftRating > 0)
                {
                    int intMax = 0;
                    foreach (Improvement objSkillsoftImprovement in CharacterObject.Improvements)
                    {
                        if (objSkillsoftImprovement.ImproveType == Improvement.ImprovementType.Skillsoft && objSkillsoftImprovement.ImprovedName == InternalId && objSkillsoftImprovement.Enabled)
                        {
                            intMax = Math.Max(intMax, objSkillsoftImprovement.Value);
                        }
                    }

                    return _intCachedCyberwareRating = Math.Min(intMax, intMaxSkillsoftRating);
                }

                return _intCachedCyberwareRating = 0;
            }
        }

        public string Type
        {
            get => _strType;
            set
            {
                if (value == _strType) return;
                _strType = value;

                //2018-22-03: Causes any attempt to alter the Type for skills with names that match
                //default skills to reset to the default Type for that skill. If we want to disable
                //that behaviour, better to disable it via the control.
                /*if (!LoadSkill())
                    {
                        if (s_CategoriesSkillMap.TryGetValue(value, out string strNewAttributeValue))
                        {
                            AttributeObject = CharacterObject.GetAttribute(strNewAttributeValue);
                        }
                    }*/
                if (s_CategoriesSkillMap.TryGetValue(value, out string strNewAttributeValue))
                {
                    AttributeObject = CharacterObject.GetAttribute(strNewAttributeValue);
                }

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// How much karma this costs. Return value during career mode is undefined
        /// </summary>
        /// <returns></returns>
        public override int CurrentKarmaCost
        {
            get
            {
                int intTotalBaseRating = TotalBaseRating;
                int intCost = intTotalBaseRating * (intTotalBaseRating + 1);
                int intLower = Base + FreeKarma;
                intCost -= intLower * (intLower + 1);

                intCost /= 2;
                intCost *= CharacterObject.Options.KarmaImproveKnowledgeSkill;
                // We have bought the first level with karma, too
                if (intLower == 0 && intCost > 0)
                    intCost += CharacterObject.Options.KarmaNewKnowledgeSkill - CharacterObject.Options.KarmaImproveKnowledgeSkill;

                decimal decMultiplier = 1.0m;
                int intExtra = 0;
                int intSpecCount = 0;
                foreach (SkillSpecialization objSpec in Specializations)
                {
                    if (!objSpec.Free && BuyWithKarma)
                        intSpecCount += 1;
                }
                int intSpecCost = CharacterObject.Options.KarmaKnowledgeSpecialization * intSpecCount;
                int intExtraSpecCost = 0;
                decimal decSpecCostMultiplier = 1.0m;
                foreach (Improvement objLoopImprovement in CharacterObject.Improvements)
                {
                    if (objLoopImprovement.Minimum <= intTotalBaseRating &&
                        (string.IsNullOrEmpty(objLoopImprovement.Condition) || (objLoopImprovement.Condition == "career") == CharacterObject.Created || (objLoopImprovement.Condition == "create") != CharacterObject.Created) && objLoopImprovement.Enabled)
                    {
                        if (objLoopImprovement.ImprovedName == Name || string.IsNullOrEmpty(objLoopImprovement.ImprovedName))
                        {
                            if (objLoopImprovement.ImproveType == Improvement.ImprovementType.KnowledgeSkillKarmaCost)
                                intExtra += objLoopImprovement.Value * (Math.Min(intTotalBaseRating, objLoopImprovement.Maximum == 0 ? int.MaxValue : objLoopImprovement.Maximum) - Math.Max(intLower, objLoopImprovement.Minimum - 1));
                            else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.KnowledgeSkillKarmaCostMultiplier)
                                decMultiplier *= objLoopImprovement.Value / 100.0m;
                        }
                        else if (objLoopImprovement.ImprovedName == SkillCategory)
                        {
                            if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategoryKarmaCost)
                                intExtra += objLoopImprovement.Value * (Math.Min(intTotalBaseRating, objLoopImprovement.Maximum == 0 ? int.MaxValue : objLoopImprovement.Maximum) - Math.Max(intLower, objLoopImprovement.Minimum - 1));
                            else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategoryKarmaCostMultiplier)
                                decMultiplier *= objLoopImprovement.Value / 100.0m;
                            else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategorySpecializationKarmaCost)
                                intExtraSpecCost += objLoopImprovement.Value * intSpecCount;
                            else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategorySpecializationKarmaCostMultiplier)
                                decSpecCostMultiplier *= objLoopImprovement.Value / 100.0m;
                        }
                    }
                }
                if (decMultiplier != 1.0m)
                    intCost = decimal.ToInt32(decimal.Ceiling(intCost * decMultiplier));

                if (decSpecCostMultiplier != 1.0m)
                    intSpecCost = decimal.ToInt32(decimal.Ceiling(intSpecCost * decSpecCostMultiplier));
                intCost += intExtra;
                intCost += intSpecCost + intExtraSpecCost; //Spec

                return Math.Max(intCost, 0);
            }
        }

        /// <summary>
        /// Karma price to upgrade. Returns negative if impossible. Minimum value is always 1.
        /// </summary>
        /// <returns>Price in karma</returns>
        public override int UpgradeKarmaCost
        {
            get
            {
                int intTotalBaseRating = TotalBaseRating;
                if (intTotalBaseRating >= RatingMaximum)
                {
                    return -1;
                }
                int intOptionsCost;
                int intValue;
                if (intTotalBaseRating == 0)
                {
                    intOptionsCost = CharacterObject.Options.KarmaNewKnowledgeSkill;
                    intValue = intOptionsCost;
                }
                else
                {
                    intOptionsCost = CharacterObject.Options.KarmaNewKnowledgeSkill;
                    intValue = (intTotalBaseRating + 1) * intOptionsCost;
                }

                decimal decMultiplier = 1.0m;
                int intExtra = 0;
                foreach (Improvement objLoopImprovement in CharacterObject.Improvements)
                {
                    if ((objLoopImprovement.Maximum == 0 || intTotalBaseRating + 1 <= objLoopImprovement.Maximum) && objLoopImprovement.Minimum <= intTotalBaseRating + 1 &&
                        (string.IsNullOrEmpty(objLoopImprovement.Condition) || (objLoopImprovement.Condition == "career") == CharacterObject.Created || (objLoopImprovement.Condition == "create") != CharacterObject.Created) && objLoopImprovement.Enabled)
                    {
                        if (objLoopImprovement.ImprovedName == Name || string.IsNullOrEmpty(objLoopImprovement.ImprovedName))
                        {
                            if (objLoopImprovement.ImproveType == Improvement.ImprovementType.KnowledgeSkillKarmaCost)
                                intExtra += objLoopImprovement.Value;
                            else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.KnowledgeSkillKarmaCostMultiplier)
                                decMultiplier *= objLoopImprovement.Value / 100.0m;
                        }
                        else if (objLoopImprovement.ImprovedName == SkillCategory)
                        {
                            if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategoryKarmaCost)
                                intExtra += objLoopImprovement.Value;
                            else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategoryKarmaCostMultiplier)
                                decMultiplier *= objLoopImprovement.Value / 100.0m;
                        }
                    }
                }
                if (decMultiplier != 1.0m)
                    intValue = decimal.ToInt32(decimal.Ceiling(intValue * decMultiplier));
                intValue += intExtra;

                return Math.Max(intValue, Math.Min(1, intOptionsCost));
            }
        }

        /// <summary>
        /// How much Sp this costs. Price during career mode is undefined
        /// </summary>
        /// <returns></returns>
        public override int CurrentSpCost
        {
            get
            {
                int intPointCost = BasePoints + (string.IsNullOrWhiteSpace(Specialization) || BuyWithKarma ? 0 : 1);

                int intExtra = 0;
                decimal decMultiplier = 1.0m;
                foreach (Improvement objLoopImprovement in CharacterObject.Improvements)
                {
                    if (objLoopImprovement.Minimum <= BasePoints &&
                        (string.IsNullOrEmpty(objLoopImprovement.Condition) || (objLoopImprovement.Condition == "career") == CharacterObject.Created || (objLoopImprovement.Condition == "create") != CharacterObject.Created) && objLoopImprovement.Enabled)
                    {
                        if (objLoopImprovement.ImprovedName == Name || string.IsNullOrEmpty(objLoopImprovement.ImprovedName))
                        {
                            if (objLoopImprovement.ImproveType == Improvement.ImprovementType.KnowledgeSkillPointCost)
                                intExtra += objLoopImprovement.Value * (Math.Min(BasePoints, objLoopImprovement.Maximum == 0 ? int.MaxValue : objLoopImprovement.Maximum) - objLoopImprovement.Minimum);
                            else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.KnowledgeSkillPointCostMultiplier)
                                decMultiplier *= objLoopImprovement.Value / 100.0m;
                        }
                        else if (objLoopImprovement.ImprovedName == SkillCategory)
                        {
                            if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategoryPointCost)
                                intExtra += objLoopImprovement.Value * (Math.Min(BasePoints, objLoopImprovement.Maximum == 0 ? int.MaxValue : objLoopImprovement.Maximum) - objLoopImprovement.Minimum);
                            else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategoryPointCostMultiplier)
                                decMultiplier *= objLoopImprovement.Value / 100.0m;
                        }
                    }
                }
                if (decMultiplier != 1.0m)
                    intPointCost = decimal.ToInt32(decimal.Ceiling(intPointCost * decMultiplier));
                intPointCost += intExtra;

                return Math.Max(intPointCost, 0);
            }
        }

        public override void WriteTo(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("skill");
            objWriter.WriteElementString("guid", Id.ToString("D"));
            objWriter.WriteElementString("suid", SkillId.ToString("D"));
            objWriter.WriteElementString("isknowledge", bool.TrueString);
            objWriter.WriteElementString("skillcategory", SkillCategory);
            objWriter.WriteElementString("karma", KarmaPoints.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("base", BasePoints.ToString(GlobalOptions.InvariantCultureInfo)); //this could acctually be saved in karma too during career
            objWriter.WriteElementString("notes", Notes);
            if (!CharacterObject.Created)
            {
                objWriter.WriteElementString("buywithkarma", BuyWithKarma.ToString());
            }

            if (Specializations.Count != 0)
            {
                objWriter.WriteStartElement("specs");
                foreach (SkillSpecialization objSpecialization in Specializations)
                {
                    objSpecialization.Save(objWriter);
                }
                objWriter.WriteEndElement();
            }

            objWriter.WriteElementString("name", Name);
            objWriter.WriteElementString("type", _strType);
            if (ForcedName)
                objWriter.WriteElementString("forced", null);

            objWriter.WriteEndElement();

        }

        public void Load(XmlNode xmlNode)
        {
            if (xmlNode == null)
                return;
            string strTemp = Name;
            if (xmlNode.TryGetStringFieldQuickly("name", ref strTemp))
                Name = strTemp;
            if (xmlNode.TryGetField("id", Guid.TryParse, out Guid guiTemp))
                SkillId = guiTemp;
            else if (xmlNode.TryGetField("suid", Guid.TryParse, out Guid guiTemp2))
                SkillId = guiTemp2;

            // Legacy shim
            if (SkillId.Equals(Guid.Empty))
            {
                XmlNode objDataNode = XmlManager.Load("skills.xml", GlobalOptions.Language).SelectSingleNode("/chummer/knowledgeskills/skill[name = \"" + Name + "\"]");
                if (objDataNode.TryGetField("id", Guid.TryParse, out Guid guidTemp))
                    SkillId = guidTemp;
            }
            
            string strCategoryString = string.Empty;
            if ((xmlNode.TryGetStringFieldQuickly("type", ref strCategoryString) && !string.IsNullOrEmpty(strCategoryString))
                || (xmlNode.TryGetStringFieldQuickly("skillcategory", ref strCategoryString) && !string.IsNullOrEmpty(strCategoryString)))
            {
                Type = strCategoryString;
            }
        }
    }
}
