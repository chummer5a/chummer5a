using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Chummer.Backend.Equipment;
using Chummer.Datastructures;

namespace Chummer.Backend.Skills
{
    public class KnowledgeSkill : Skill
    {
        private static readonly Dictionary<string, string> s_CategoriesSkillMap = new Dictionary<string, string>();  //Categories to their attribtue

        public static IList<ListItem> DefaultKnowledgeSkills(string strLanguage)
        {
            List<ListItem> lstKnowledgeSkills = new List<ListItem>();
            XmlDocument objSkillsDocument = XmlManager.Load("skills.xml", strLanguage);
            if (objSkillsDocument != null)
            {
                foreach (XmlNode objXmlSkill in objSkillsDocument.SelectNodes("/chummer/knowledgeskills/skill"))
                {
                    string strName = objXmlSkill["name"]?.InnerText ?? string.Empty;
                    lstKnowledgeSkills.Add(new ListItem(strName, objXmlSkill["translate"]?.InnerText ?? strName));
                }
                lstKnowledgeSkills.Sort(CompareListItems.CompareNames);
            }
            return lstKnowledgeSkills;
        }

        /// <summary>
        /// Load the (possible translated) types of kno skills (Academic, Street...)
        /// </summary>
        /// <param name="strLanguage"></param>
        /// <returns></returns>
        public static IList<ListItem> KnowledgeTypes(string strLanguage)
        {
            List<ListItem> lstKnowledgeTypes = new List<ListItem>();
            XmlDocument objSkillsDocument = XmlManager.Load("skills.xml", strLanguage);
            if (objSkillsDocument != null)
            {
                foreach (XmlNode objXmlCategory in objSkillsDocument.SelectNodes("/chummer/categories/category[@type = \"knowledge\"]"))
                {
                    string strInnerText = objXmlCategory.InnerText;
                    lstKnowledgeTypes.Add(new ListItem(strInnerText, objXmlCategory.Attributes?["translate"]?.InnerText ?? strInnerText));
                }
                lstKnowledgeTypes.Sort(CompareListItems.CompareNames);
            }
            return lstKnowledgeTypes;
        }

        static KnowledgeSkill()
        {
            XmlNodeList objXmlSkillList = XmlManager.Load("skills.xml")?.SelectNodes("/chummer/knowledgeskills/skill");

            foreach (XmlNode objXmlSkill in objXmlSkillList)
            {
                string strCategory = objXmlSkill["category"]?.InnerText;
                if (!string.IsNullOrWhiteSpace(strCategory))
                {
                    s_CategoriesSkillMap[strCategory] = objXmlSkill["attribute"]?.InnerText;
                }
            }
        }

        public static IList<ListItem> KnowledgeSkillsWithCategory(string strLanguage, params string[] categories)
        {
            HashSet<string> lstCategories = new HashSet<string>(categories);
            List<ListItem> lstKnowledgeSkills = new List<ListItem>();
            XmlDocument objSkillsDocument = XmlManager.Load("skills.xml", strLanguage);
            if (objSkillsDocument != null)
            {
                foreach (XmlNode objXmlSkill in objSkillsDocument.SelectNodes("/chummer/knowledgeskills/skill"))
                {
                    if (lstCategories.Contains(objXmlSkill["category"]?.InnerText))
                    {
                        string strName = objXmlSkill["name"]?.InnerText ?? string.Empty;
                        lstKnowledgeSkills.Add(new ListItem(strName, objXmlSkill["translate"]?.InnerText ?? strName));
                    }
                }
                lstKnowledgeSkills.Sort(CompareListItems.CompareNames);
            }
            return lstKnowledgeSkills;
        } 

        public override bool AllowDelete
        {
            get { return true; } //TODO LM
        }
        
        private string _strType = string.Empty;
        public bool ForcedName { get; }

        public KnowledgeSkill(Character character) : base(character)
        {
            AttributeObject = character.LOG;
            AttributeObject.PropertyChanged += OnLinkedAttributeChanged;
        }

        public KnowledgeSkill(Character character, string forcedName) : this(character)
        {
            WriteableName = forcedName;
            LoadDefaultType(Name);
            ForcedName = true;
        }

        public string WriteableName
        {
            get
            {
                return Name;
            }
            set
            {
                if (ForcedName)
                    return;
                Name = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
                LoadSuggestedSpecializations(Name);

                OnPropertyChanged();
                OnPropertyChanged(nameof(Type));
                OnPropertyChanged(nameof(Base));
            }
        }

        private void LoadSuggestedSpecializations(string strName)
        {
            SuggestedSpecializations.Clear();

            XmlNodeList list = GetNode()?.SelectNodes("specs/spec");

            if (list != null)
            {
                foreach (XmlNode node in list)
                {
                    string strInnerText = node.InnerText;
                    SuggestedSpecializations.Add(new ListItem(strInnerText, node.Attributes?["translate"]?.InnerText ?? strInnerText));
                }

                SuggestedSpecializations.Sort(CompareListItems.CompareNames);
            }
            OnPropertyChanged(nameof(CGLSpecializations));
        }

        public void LoadDefaultType(string strName)
        {
            if (strName == null)
                return;
            //TODO: Should this be targeted against guid for uniqueness? Creating a knowledge skill in career always generates a new SkillId instead of using the one from skills.
            XmlNode xmlSkillNode = XmlManager.Load("skills.xml").SelectSingleNode($"chummer/knowledgeskills/skill[name = \"{strName}\"]");
            if (xmlSkillNode != null)
            {
                _strType = xmlSkillNode["category"]?.InnerText ?? string.Empty;
                AttributeObject = CharacterObject.GetAttribute(xmlSkillNode["attribute"]?.InnerText ?? "LOG");
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
                if (Rating == 0 && _strType == "Language")
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
            if (CachedWareRating != int.MinValue)
                return CachedWareRating;

            if (IsKnowledgeSkill && CharacterObject.SkillsoftAccess)
            {
                int intMax = 0;
                //TODO this works with translate?
                foreach (Gear objSkillsoft in CharacterObject.Gear.DeepWhere(x => x.Children, x => x.Equipped && x.Category == "Skillsofts" && (x.Extra == Name || x.Extra == DisplayNameMethod(GlobalOptions.Language))))
                {
                    if (objSkillsoft.Rating > intMax)
                        intMax = objSkillsoft.Rating;
                }
                return CachedWareRating = intMax;
            }

            return CachedWareRating = 0;
        }

        public string Type
        {
            get { return _strType; }
            set
            {
                if (!s_CategoriesSkillMap.TryGetValue(value, out string strNewAttributeValue)) return;
                AttributeObject.PropertyChanged -= OnLinkedAttributeChanged;
                AttributeObject = CharacterObject.GetAttribute(strNewAttributeValue);

                AttributeObject.PropertyChanged += OnLinkedAttributeChanged;
                _strType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AttributeModifiers));
            }
        }

        /// <summary>
        /// How much karma this costs. Return value during career mode is undefined
        /// </summary>
        /// <returns></returns>
        public override int CurrentKarmaCost()
        {
            int intTotalBaseRating = TotalBaseRating;
            int cost = intTotalBaseRating * (intTotalBaseRating + 1);
            int lower = Base + FreeKarma();
            cost -= lower * (lower + 1);

            cost /= 2;
            cost *= CharacterObject.Options.KarmaImproveKnowledgeSkill;
            // We have bought the first level with karma, too
            if (lower == 0 && cost > 0)
                cost += CharacterObject.Options.KarmaNewKnowledgeSkill - CharacterObject.Options.KarmaImproveKnowledgeSkill;

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
                            intExtra += objLoopImprovement.Value * (Math.Min(intTotalBaseRating, objLoopImprovement.Maximum == 0 ? int.MaxValue : objLoopImprovement.Maximum) - Math.Max(lower, objLoopImprovement.Minimum - 1));
                        else if (objLoopImprovement.ImproveType == Improvement.ImprovementType.KnowledgeSkillKarmaCostMultiplier)
                            decMultiplier *= objLoopImprovement.Value / 100.0m;
                    }
                    else if (objLoopImprovement.ImprovedName == SkillCategory)
                    {
                        if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SkillCategoryKarmaCost)
                            intExtra += objLoopImprovement.Value * (Math.Min(intTotalBaseRating, objLoopImprovement.Maximum == 0 ? int.MaxValue : objLoopImprovement.Maximum) - Math.Max(lower, objLoopImprovement.Minimum - 1));
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
                cost = decimal.ToInt32(decimal.Ceiling(cost * decMultiplier));

            if (decSpecCostMultiplier != 1.0m)
                intSpecCost = decimal.ToInt32(decimal.Ceiling(intSpecCost * decSpecCostMultiplier));
            cost += intExtra;
            cost += intSpecCost + intExtraSpecCost; //Spec

            return Math.Max(cost, 0);
        }

        /// <summary>
        /// Karma price to upgrade. Returns negative if impossible. Minimum value is always 1.
        /// </summary>
        /// <returns>Price in karma</returns>
        public override int UpgradeKarmaCost()
        {
            int intTotalBaseRating = TotalBaseRating;
            if (intTotalBaseRating >= RatingMaximum)
            {
                return -1;
            }
            int intOptionsCost = 1;
            int value = 0;
            if (intTotalBaseRating == 0)
            {
                intOptionsCost = CharacterObject.Options.KarmaNewKnowledgeSkill;
                value = intOptionsCost;
            }
            else
            {
                intOptionsCost = CharacterObject.Options.KarmaNewKnowledgeSkill;
                value = (intTotalBaseRating + 1) * intOptionsCost;
            }

            decimal decMultiplier = 1.0m;
            int intExtra = 0;
            foreach (Improvement objLoopImprovement in CharacterObject.Improvements)
            {
                if ((objLoopImprovement.Maximum == 0 || intTotalBaseRating <= objLoopImprovement.Maximum) && objLoopImprovement.Minimum <= intTotalBaseRating &&
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
                value = decimal.ToInt32(decimal.Ceiling(value * decMultiplier));
            value += intExtra;

            return Math.Max(value, Math.Min(1, intOptionsCost));
        }

        /// <summary>
        /// How much Sp this costs. Price during career mode is undefined
        /// </summary>
        /// <returns></returns>
        public override int CurrentSpCost()
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
                writer.WriteElementString("forced", null);
        }

        public void Load(XmlNode node)
        {
            if (node == null)
                return;
            string strTemp = Name;
            if (node.TryGetStringFieldQuickly("name", ref strTemp))
                Name = strTemp;
            if (node["id"] != null)
                SkillId = Guid.Parse(node["id"].InnerText);
            else if (node["suid"] != null)
                SkillId = Guid.Parse(node["suid"].InnerText);

            // Legacy shim
            if (SkillId.Equals(Guid.Empty))
            {
                XmlNode objDataNode = XmlManager.Load("skills.xml", GlobalOptions.Language)?.SelectSingleNode("/chummer/knowledgeskills/skill[name = \"" + Name + "\"]");
                if (objDataNode?["id"] != null)
                    SkillId = Guid.Parse(objDataNode["id"].InnerText);
            }

            LoadSuggestedSpecializations(Name);
            string strCategoryString = string.Empty;
            if ((node.TryGetStringFieldQuickly("type", ref strCategoryString) && !string.IsNullOrEmpty(strCategoryString))
                || (node.TryGetStringFieldQuickly("skillcategory", ref strCategoryString) && !string.IsNullOrEmpty(strCategoryString)))
            {
                Type = strCategoryString;
            }
        }

        public override bool IsKnowledgeSkill
        {
            get { return true; }
        }
    }
}
