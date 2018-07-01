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

			XmlNodeList objXmlCategoryList = objXmlDocument.SelectNodes("/chummer/categories/category[@type = \"knowledge\"]");

            KnowledgeTypes = (from XmlNode objXmlCategory in objXmlCategoryList
		        let display = objXmlCategory.Attributes?["translate"]?.InnerText ?? objXmlCategory.InnerText
		        orderby display
		        select new ListItem(objXmlCategory.InnerText, display)).ToList();

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
                    return;
                string strNewName = value;
                if (GlobalOptions.Language != GlobalOptions.DefaultLanguage)
                {
                    XmlDocument xmlSkillDoc = XmlManager.Load("skills.xml", GlobalOptions.Language);
                    XmlNode xmlNewNode = xmlSkillDoc.SelectSingleNode("/chummer/knowledgeskills/skill[translate = \"" + value + "\" and category = \"" + Type + "\"]") ??
                                         xmlSkillDoc.SelectSingleNode("/chummer/knowledgeskills/skill[translate = \"" + value + "\"]");
                    if (xmlNewNode != null)
                        strNewName = xmlNewNode["name"]?.InnerText ?? value;
                    else
                        strNewName = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
                }
                if (Name != strNewName)
                {
                    Name = strNewName;
                    LoadDefaultType();
                    LoadSuggestedSpecializations();
                    OnPropertyChanged();
                }
            }
        }

        private void LoadSuggestedSpecializations()
        {
            SuggestedSpecializations.Clear();

            XmlNodeList list = GetNode()?.SelectNodes("specs/spec");

	    private string _translated; //non english name, if present
		private List<ListItem> _knowledgeSkillCatagories;
		private string _type;
		public bool ForcedName { get; }

		public KnowledgeSkill(Character character) : base(character, (string)null)
		{
			AttributeObject = character.LOG;
			AttributeObject.PropertyChanged += OnLinkedAttributeChanged;
			_type = string.Empty;
			SuggestedSpecializations = new List<ListItem>();
		}

		public KnowledgeSkill(Character character, string forcedName) : this(character)
		{
			WriteableName = forcedName;
			ForcedName = true;
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

		private void LoadSuggestedSpecializations(string name)
		{
		    string strNameValue;
			if (NameCategoryMap.TryGetValue(name, out strNameValue))
			{
				SuggestedSpecializations.Clear();

				XmlNodeList list =
					XmlManager.Instance.Load("skills.xml").SelectNodes($"chummer/knowledgeskills/skill[name = \"{name}\"]/specs/spec");
				foreach (XmlNode node in list)
				{
					SuggestedSpecializations.Add(ListItem.AutoXml(node.InnerText, node));
				}

                SortListItem objSort = new SortListItem();
                SuggestedSpecializations.Sort(objSort.Compare);
				OnPropertyChanged(nameof(CGLSpecializations));
			}
		}

		public override string SkillCategory
		{
			get { return Type; }
		}

		public override string DisplayPool
		{
			get
			{
				if (Rating == 0 && _type == "Language")
				{
					return "N";
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
        public override int CyberwareRating()
        {

            if (_cachedWareRating != int.MinValue) return _cachedWareRating;

            if (IsKnowledgeSkill && CharacterObject.SkillsoftAccess)
            {
                Func<Gear, int> recusivestuff = null;
                recusivestuff = (gear) =>
                {
                    //TODO this works with translate?
                    if (gear.Equipped && gear.Category == "Skillsofts" &&
                        (gear.Extra == Name ||
                         gear.Extra == Name + ", " + LanguageManager.Instance.GetString("Label_SelectGear_Hacked")))
                    {
                        return gear.Rating;
                    }
                    return gear.Children.Select(child => recusivestuff(child)).FirstOrDefault(returned => returned > 0);
                };

                return _cachedWareRating = CharacterObject.Gear.Select(child => recusivestuff(child)).FirstOrDefault(val => val > 0);

            }

            return _cachedWareRating = 0;
        }

        public string Type
		{
			get { return _type; }
			set
			{
			    string strNewAttributeValue;
                if (!CategoriesSkillMap.TryGetValue(value, out strNewAttributeValue)) return;
				AttributeObject.PropertyChanged -= OnLinkedAttributeChanged;
				AttributeObject = CharacterObject.GetAttribute(strNewAttributeValue);

				AttributeObject.PropertyChanged += OnLinkedAttributeChanged;
				_type = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(AttributeModifiers));
			}
		}

                //2018-22-03: Causes any attempt to alter the Type for skills with names that match
                //default skills to reset to the default Type for that skill. If we want to disable
                //that behaviour, better to disable it via the control.
                /*if (!LoadDefaultType())
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

			return cost;
		}

		public static int CompareKnowledgeSkills(KnowledgeSkill rhs, KnowledgeSkill lhs)
		{
			if (rhs.Name != null && lhs.Name != null)
			{
				return string.Compare(rhs.WriteableName, lhs.WriteableName, StringComparison.Ordinal);
			}
			return 0;
		}

		/// <summary>
		/// Karma price to upgrade. Returns negative if impossible. Minimum value is always 1.
		/// </summary>
		/// <returns>Price in karma</returns>
		public override int UpgradeKarmaCost()
		{
            if (LearnedRating >= RatingMaximum)
            {
                return -1;
            }
            int adjustment = 0;
			if (CharacterObject.SkillsSection.JackOfAllTrades && CharacterObject.Created)
			{
				adjustment = LearnedRating > 5 ? 2 : -1;
			}
			if (HasRelatedBoost() && CharacterObject.Created && LearnedRating >= 3)
			{
				adjustment -= 1;
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

		    value = Math.Max(value, 1);
			if (UneducatedEffect())
                value *= 2;
			return value;
		}

		/// <summary>
		/// How much Sp this costs. Price during career mode is undefined
		/// </summary>
		/// <returns></returns>
		public override int CurrentSpCost()
		{
		    int intPointCost = BasePoints + (string.IsNullOrWhiteSpace(Specialization) || BuyWithKarma ? 0 : 1);
			if (HasRelatedBoost())
			{
                intPointCost = (intPointCost + 1)/2;
			}
            return intPointCost;
        }

		/// <summary>
		/// This method checks if the character have the related knowledge skill
		/// quality.
		/// 
		/// Eg. If it is a language skill, it returns Character.Linguistic, if
		/// it is technical it returns Character.TechSchool
		/// </summary>
		/// <returns></returns>
		private bool HasRelatedBoost()
		{
			switch (_type)
			{
				case "Language":
					return CharacterObject.SkillsSection.Linguist;
				case "Professional":
					return CharacterObject.SkillsSection.TechSchool;
				case "Street":
					return CharacterObject.SkillsSection.SchoolOfHardKnocks;
				case "Academic":
					return CharacterObject.SkillsSection.CollegeEducation;
				case "Interest":
					return false;
			}
            return false;
        }

		private bool UneducatedEffect()
		{
			switch (_type)
			{
				case "Professional":
                case "Academic":
                    return CharacterObject.SkillsSection.Uneducated;
			}
            return false;
        }

		protected override void SaveExtendedData(XmlTextWriter writer)
		{
			writer.WriteElementString("name", _name);
			writer.WriteElementString("type", _type);
			if (_translated != null)
                writer.WriteElementString(GlobalOptions.Instance.Language, _translated);
			if (ForcedName)
                writer.WriteElementString("forced", null);
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

        public override bool IsKnowledgeSkill
        {
            get { return true; }
        }
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
            if (list != null)
            {
                foreach (XmlNode node in list)
                {
                    string strInnerText = node.InnerText;
                    SuggestedSpecializations.Add(new ListItem(strInnerText, node.Attributes?["translate"]?.InnerText ?? strInnerText));
                }
                SuggestedSpecializations.Sort(CompareListItems.CompareNames);
            }
            OnPropertyChanged(nameof(SuggestedSpecializations));
        }
        public bool LoadDefaultType()
        {
            XmlDocument xmlSkillDoc = XmlManager.Load("skills.xml", GlobalOptions.Language);
            XmlNode xmlNewNode = xmlSkillDoc.SelectSingleNode("/chummer/knowledgeskills/skill[name = \"" + Name + "\" and category = \"" + Type + "\"]") ??
                                 xmlSkillDoc.SelectSingleNode("/chummer/knowledgeskills/skill[name = \"" + Name + "\"]");
            if (xmlNewNode != null)
            {
                SkillId = xmlNewNode.TryGetField("id", Guid.TryParse, out Guid guidTemp) ? guidTemp : Guid.Empty;
                string strCategory = xmlNewNode["category"]?.InnerText;
                if (!string.IsNullOrEmpty(strCategory))
                    Type = strCategory;
                string strAttribute = xmlNewNode["attribute"]?.InnerText;
                if (!string.IsNullOrEmpty(strAttribute))
                    AttributeObject = CharacterObject.GetAttribute(strAttribute) ?? CharacterObject.LOG;
                return true;
            }
            SkillId = Guid.Empty;
            return false;
        }
                if (IsKnowledgeSkill)
                {
                    int intMaxSkillsoftRating = Math.Min(IsKnowledgeSkill ? int.MaxValue : ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.Skillwire), ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.SkillsoftAccess));
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
                }
                return _intCachedCyberwareRating = 0;
            }
        }

        {
            get => _strType;
            set
            {
                if (value == _strType) return;
                _strType = value;
                return Math.Max(intCost, 0);
            }
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
        protected override void SaveExtendedData(XmlTextWriter writer)
        {
            writer.WriteElementString("name", Name);
            writer.WriteElementString("type", _strType);
            if (ForcedName)
            LoadSuggestedSpecializations();
            string strCategoryString = string.Empty;
            if ((xmlNode.TryGetStringFieldQuickly("type", ref strCategoryString) && !string.IsNullOrEmpty(strCategoryString))
                || (xmlNode.TryGetStringFieldQuickly("skillcategory", ref strCategoryString) && !string.IsNullOrEmpty(strCategoryString)))
            {
                Type = strCategoryString;
            }
        }

        public override bool IsKnowledgeSkill => true;