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
        private static readonly TranslatedField<string> s_Translator = new TranslatedField<string>();
        private static readonly Dictionary<string, string> s_CategoriesSkillMap = new Dictionary<string, string>();  //Categories to their attribtue
        private static readonly Dictionary<string, string> s_NameCategoryMap = new Dictionary<string, string>();  //names to their category

        public static IList<ListItem> DefaultKnowledgeSkillCatagories { get; }
        public static IList<ListItem> KnowledgeTypes { get; }  //Load the (possible translated) types of kno skills (Academic, Street...)

        static KnowledgeSkill()
        {
            XmlDocument objXmlDocument = XmlManager.Load("skills.xml");

            XmlNodeList objXmlCategoryList = objXmlDocument.SelectNodes("/chummer/categories/category[@type = \"knowledge\"]");

            KnowledgeTypes = (from XmlNode objXmlCategory in objXmlCategoryList
                let display = objXmlCategory.Attributes?["translate"]?.InnerText ?? objXmlCategory.InnerText
                orderby display
                select new ListItem(objXmlCategory.InnerText, display)).ToList();

            XmlNodeList objXmlSkillList = objXmlDocument.SelectNodes("/chummer/knowledgeskills/skill");
            DefaultKnowledgeSkillCatagories = new List<ListItem>();
            foreach (XmlNode objXmlSkill in objXmlSkillList)
            {
                string strSkillName = objXmlSkill["name"]?.InnerText;
                if (string.IsNullOrWhiteSpace(strSkillName))
                    continue;
                string display = objXmlSkill["translate"]?.InnerText ?? strSkillName;

                if (GlobalOptions.Language != GlobalOptions.DefaultLanguage)
                {
                    s_Translator.Add(strSkillName, display);
                }

                DefaultKnowledgeSkillCatagories.Add(new ListItem(strSkillName, display));

                string strCategory = objXmlSkill["category"]?.InnerText;
                if (!string.IsNullOrWhiteSpace(strCategory))
                {
                    s_NameCategoryMap[strSkillName] = strCategory;
                    s_CategoriesSkillMap[strCategory] = objXmlSkill["attribute"]?.InnerText;
                }
            }

            DefaultKnowledgeSkillCatagories = DefaultKnowledgeSkillCatagories.OrderBy(x => x.Name).ToList();
        }

        public static IList<ListItem> KnowledgeSkillsWithCategory(params string[] categories)
        {
            HashSet<string> set = new HashSet<string>(categories);

            return DefaultKnowledgeSkillCatagories.Where(x => set.Contains(s_NameCategoryMap[x.Value])).ToList();
        } 

        public override bool AllowDelete
        {
            get { return true; } //TODO LM
        }

        private string _strTranslated; //non english name, if present
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

        public IList<ListItem> KnowledgeSkillCatagories
        {
            get { return DefaultKnowledgeSkillCatagories; }
        }

        public string WriteableName
        {
            get { return s_Translator.Read(Name, ref _strTranslated); }
            set
            {
                if (ForcedName)
                    return;
                string strOriginal = Name;
                s_Translator.Write(value, ref strOriginal, ref _strTranslated);
                Name = strOriginal;
                LoadSuggestedSpecializations(Name);

                OnPropertyChanged();
                OnPropertyChanged(nameof(Type));
                OnPropertyChanged(nameof(Base));
            }
        }

        private void LoadSuggestedSpecializations(string name)
        {
            if (s_NameCategoryMap.TryGetValue(name, out string strNameValue))
            {
                SuggestedSpecializations.Clear();

                XmlNodeList list = XmlManager.Load("skills.xml").SelectNodes($"chummer/knowledgeskills/skill[name = \"{name}\"]/specs/spec");
                foreach (XmlNode node in list)
                {
                    string strInnerText = node.InnerText;
                    SuggestedSpecializations.Add(new ListItem(strInnerText, node.Attributes?["translate"]?.InnerText ?? strInnerText));
                }
                
                SuggestedSpecializations.Sort(CompareListItems.CompareNames);
                OnPropertyChanged(nameof(CGLSpecializations));
            }
        }

        public void LoadDefaultType(string name)
        {
            if (name == null)
                return;
            //TODO: Should this be targeted against guid for uniqueness? Creating a knowledge skill in career always generates a new SkillId instead of using the one from skills.
            XmlNode skillNode = XmlManager.Load("skills.xml").SelectSingleNode($"chummer/knowledgeskills/skill[name = \"{name}\"]");
            if (skillNode != null)
            {
                _strType = skillNode["category"]?.InnerText ?? string.Empty;
                AttributeObject = CharacterObject.GetAttribute(skillNode["attribute"]?.InnerText ?? "LOG");
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
                foreach (Gear objSkillsoft in CharacterObject.Gear.DeepWhere(x => x.Children, x => x.Equipped && x.Category == "Skillsofts" &&
                    (x.Extra == Name || x.Extra == Name + ", " + LanguageManager.GetString("Label_SelectGear_Hacked"))))
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
            if (_strTranslated != null)
                writer.WriteElementString(GlobalOptions.Language, _strTranslated);
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
            node.TryGetStringFieldQuickly(GlobalOptions.Language, ref _strTranslated);

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
