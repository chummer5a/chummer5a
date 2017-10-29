using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using Chummer.Backend;

namespace Chummer.Skills
{
    public class SkillsSection : INotifyPropertyChanged
    {
        public Action<object> CollegeEducationChanged;
        public Action<object> JackOfAllTradesChanged;
        public Action<object> LinguistChanged;
        public Action<object> SchoolOfHardKnocksChanged;
        public Action<object> TechSchoolChanged;
        public Action<object> UncouthChanged;
        public Action<object> UneducatedChanged;
        
        private readonly Character _character;
        private bool _blnUneducated;
        private bool _blnJackOfAllTrades;
        private bool _blnCollegeEducation;
        private bool _blnUncouth;
        private bool _blnSchoolOfHardKnocks;
        private bool _blnTechSchool;
        private bool _blnLinguist;
        private Dictionary<Guid, Skill> _skillValueBackup = new Dictionary<Guid, Skill>(); 

        public SkillsSection(Character character)
        {
            _character = character;
            _character.LOG.PropertyChanged += (sender, args) => KnoChanged();
            _character.INT.PropertyChanged += (sender, args) => KnoChanged();

            _character.SkillImprovementEvent += CharacterOnImprovementEvent;

        }

        private void CharacterOnImprovementEvent(List<Improvement> improvements)
        {
            if (PropertyChanged != null && improvements.Any(x => x.ImproveType == Improvement.ImprovementType.FreeKnowledgeSkills))
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(HasKnowledgePoints)));
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(KnowledgeSkillPoints)));
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(KnowledgeSkillPointsRemain)));
            }
        }

        internal void AddSkills(FilterOptions skills, string strName = "")
        {
            var list = GetSkillList(_character, skills, strName);

            Skills.MergeInto(list, CompareSkills);
            foreach (Skill objSkill in list)
            {
                string strKey = objSkill.IsExoticSkill ? objSkill.Name + " (" + objSkill.DisplaySpecialization + ")" : objSkill.Name;
                if (!_dicSkills.ContainsKey(strKey))
                    _dicSkills.Add(strKey, objSkill);
            }
        }

        internal void RemoveSkills(FilterOptions skills)
        {
            string category;
            switch (skills)
            {
                case FilterOptions.Magician:
                case FilterOptions.Sorcery:
                case FilterOptions.Conjuring:
                case FilterOptions.Enchanting:
                case FilterOptions.Adept:
                    category = "Magical Active";
                    break;
                case FilterOptions.Technomancer:
                    category = "Resonance Active";
                    break;
                default:
                    return;
            }
            // Check for duplicates (we'd normally want to make sure it's enabled, but SpecialSkills doesn't process the Enabled property properly)
            foreach (Improvement objImprovement in _character.Improvements.Where(x => x.ImproveType == Improvement.ImprovementType.SpecialSkills))
            {
                FilterOptions eLoopFilter = (FilterOptions)Enum.Parse(typeof(FilterOptions), objImprovement.ImprovedName);
                string strLoopCategory = string.Empty;
                switch (eLoopFilter)
                {
                    case FilterOptions.Magician:
                    case FilterOptions.Sorcery:
                    case FilterOptions.Conjuring:
                    case FilterOptions.Enchanting:
                    case FilterOptions.Adept:
                        strLoopCategory = "Magical Active";
                        break;
                    case FilterOptions.Technomancer:
                        strLoopCategory = "Resonance Active";
                        break;
                }
                if (strLoopCategory == category)
                    return;
            }

            for (int i = Skills.Count - 1; i >= 0; i--)
            {
                if (Skills[i].SkillCategory == category)
                {
                    Skill skill = Skills[i];
                    _skillValueBackup[skill.SkillId] = skill;
                    Skills.RemoveAt(i);
                    SkillsDictionary.Remove(skill.IsExoticSkill ? skill.Name + " (" + skill.DisplaySpecialization + ")" : skill.Name);

                    if (_character.Created && skill.TotalBaseRating > 0)
                    {
                        KnowledgeSkill kno = new KnowledgeSkill(_character)
                        {
                            Type = skill.Name == "Arcana" ? "Academic" : "Professional",
                            WriteableName = skill.Name,
                            Base = skill.Base,
                            Karma = skill.Karma
                        };
                        kno.Specializations.AddRange(skill.Specializations);
                        KnowledgeSkills.Add(kno);
                    }
                }
            }
        }

        internal void Load(XmlNode skillNode, bool legacy = false)
        {
            if (skillNode == null)
                return;
            Timekeeper.Start("load_char_skills");

            if (!legacy)
            {
                Timekeeper.Start("load_char_skills_groups");
                List<SkillGroup> loadingSkillGroups = new List<SkillGroup>();
                foreach (XmlNode node in skillNode.SelectNodes("groups/group"))
                {
                    SkillGroup skillgroup = SkillGroup.Load(_character, node);
                    if (skillgroup != null)
                        loadingSkillGroups.Add(skillgroup);
                }
                loadingSkillGroups.Sort((i1, i2) => String.Compare(i2.DisplayName, i1.DisplayName, StringComparison.Ordinal));
                foreach (SkillGroup skillgroup in loadingSkillGroups)
                {
                    SkillGroups.Add(skillgroup);
                }
                Timekeeper.Finish("load_char_skills_groups");

                Timekeeper.Start("load_char_skills_normal");
                //Load skills. Because sorting a BindingList is complicated we use a temporery normal list
                List<Skill> loadingSkills = new List<Skill>();
                foreach (XmlNode node in skillNode.SelectNodes("skills/skill"))
                {
                    Skill skill = Skill.Load(_character, node);
                    if (skill != null)
                        loadingSkills.Add(skill);
                }
                loadingSkills.Sort(CompareSkills);


                foreach (Skill skill in loadingSkills)
                {
                    _skills.Add(skill);
                    _dicSkills.Add(skill.IsExoticSkill ? skill.Name + " (" + skill.DisplaySpecialization + ")" : skill.Name, skill);
                }
                Timekeeper.Finish("load_char_skills_normal");

                Timekeeper.Start("load_char_skills_kno");
                foreach (XmlNode node in skillNode.SelectNodes("knoskills/skill"))
                {
                    KnowledgeSkill skill = Skill.Load(_character, node) as KnowledgeSkill;
                    if (skill != null)
                        KnowledgeSkills.Add(skill);
                }
                Timekeeper.Finish("load_char_skills_kno");

                Timekeeper.Start("load_char_knowsoft_buffer");
                // Knowsoft Buffer.
                foreach (XmlNode objXmlSkill in skillNode.SelectNodes("skilljackknowledgeskills/skill"))
                {
                    string strName = string.Empty;
                    if (objXmlSkill.TryGetStringFieldQuickly("name", ref strName))
                        KnowsoftSkills.Add(new KnowledgeSkill(_character, strName));
                }
                Timekeeper.Finish("load_char_knowsoft_buffer");
            }
            else
            {
                List<Skill> tempSkillList = new List<Skill>();
                foreach (XmlNode node in skillNode.SelectNodes("skills/skill"))
                {
                    Skill skill = Skill.LegacyLoad(_character, node);
                    if (skill != null)
                        tempSkillList.Add(skill);
                }

                List<Skill> unsoredSkills = new List<Skill>();

                //Variable/Anon method as to not clutter anywhere else. Not sure if clever or stupid
                Predicate<Skill> oldSkillFilter = skill =>
                {
                    if (skill.Rating > 0) return true;

                    if (skill.SkillCategory == "Resonance Active" && !_character.RESEnabled)
                    {
                        return false;
                    }

                    //This could be more fine grained, but frankly i don't care
                    if (skill.SkillCategory == "Magical Active" && !_character.MAGEnabled)
                    {
                        return false;
                    }

                    return true;
                };

                foreach (Skill skill in tempSkillList)
                {
                    KnowledgeSkill knoSkill = skill as KnowledgeSkill;
                    if (knoSkill != null)
                    {
                        KnowledgeSkills.Add(knoSkill);
                    }
                    else if(oldSkillFilter(skill))
                    {
                        unsoredSkills.Add(skill);
                    }
                }

                unsoredSkills.Sort(CompareSkills);

                unsoredSkills.ForEach(x => { _skills.Add(x); _dicSkills.Add(x.IsExoticSkill ? x.Name + " (" + x.DisplaySpecialization + ")" : x.Name, x); });

                UpdateUndoList(skillNode);
            }

            //This might give subtle bugs in the future, 
            //but right now it needs to be run once when upgrading or it might crash. 
            //As some didn't they crashed on loading skills. 
            //After this have run, it won't (for the crash i'm aware)
            //TODO: Move it to the other side of the if someday?

            //remove skillgroups whose skills did not make the final cut
            for (var i = SkillGroups.Count - 1; i >= 0; i--)
            {
                if (!SkillGroups[i].GetEnumerable().Any(x => SkillsDictionary.ContainsKey(x.Name)))
                {
                    SkillGroups.RemoveAt(i);
                    i--;
                }
            }

            //Workaround for probably breaking compability between earlier beta builds
            if (skillNode["skillptsmax"] == null)
            {
                skillNode = skillNode.OwnerDocument["character"];
            }

            int intTmp = 0;
            if (skillNode.TryGetInt32FieldQuickly("skillptsmax", ref intTmp))
                SkillPointsMaximum = intTmp;
            if (skillNode.TryGetInt32FieldQuickly("skillgrpsmax", ref intTmp))
                SkillGroupPointsMaximum = intTmp;
            skillNode.TryGetBoolFieldQuickly("uneducated", ref _blnUneducated);
            skillNode.TryGetBoolFieldQuickly("uncouth", ref _blnUncouth);
            skillNode.TryGetBoolFieldQuickly("schoolofhardknocks", ref _blnSchoolOfHardKnocks);
            skillNode.TryGetBoolFieldQuickly("collegeeducation", ref _blnCollegeEducation);
            skillNode.TryGetBoolFieldQuickly("jackofalltrades", ref _blnJackOfAllTrades);
            skillNode.TryGetBoolFieldQuickly("techschool", ref _blnTechSchool);
            skillNode.TryGetBoolFieldQuickly("linguist", ref _blnLinguist);

            Timekeeper.Finish("load_char_skills");
        }

        private void UpdateUndoList(XmlNode skillNode)
        {
            //Hacky way of converting Expense entries to guid based skill identification
            //specs allready did?
            //First create dictionary mapping name=>guid
            Dictionary<string, Guid> dicGroups = new Dictionary<string, Guid>();
            foreach (SkillGroup objLoopSkillGroup in SkillGroups)
            {
                if (objLoopSkillGroup.Rating > 0 && !dicGroups.ContainsKey(objLoopSkillGroup.Name))
                {
                    dicGroups.Add(objLoopSkillGroup.Name, objLoopSkillGroup.Id);
                }
            }

            Dictionary<string, Guid> dicSkills = new Dictionary<string, Guid>();
            foreach (Skill objLoopSkill in Skills)
            {
                if (objLoopSkill.TotalBaseRating > 0 && !dicSkills.ContainsKey(objLoopSkill.Name))
                {
                    dicSkills.Add(objLoopSkill.Name, objLoopSkill.Id);
                }
            }
            foreach (KnowledgeSkill objLoopSkill in KnowledgeSkills)
            {
                if (!dicGroups.ContainsKey(objLoopSkill.Name))
                {
                    dicSkills.Add(objLoopSkill.Name, objLoopSkill.Id);
                }
            }

            UpdateUndoSpecific(skillNode.OwnerDocument, dicSkills, new[] { KarmaExpenseType.AddSkill, KarmaExpenseType.ImproveSkill });
            UpdateUndoSpecific(skillNode.OwnerDocument, dicGroups, new[] { KarmaExpenseType.ImproveSkillGroup });
        }

        private static void UpdateUndoSpecific(XmlDocument doc, Dictionary<string, Guid> map, KarmaExpenseType[] typesRequreingConverting)
        {
            //Build a crazy xpath to get everything we want to convert

            string xpath =
                $"/character/expenses/expense[type = \'Karma\']/undo[{string.Join(" or ", typesRequreingConverting.Select(x => $"karmatype = '{x}'"))}]/objectid";

            //Find everything
            XmlNodeList nodesToChange = doc.SelectNodes(xpath);
            if (nodesToChange != null)
            {
                Guid guidLoop;
                for (var i = 0; i < nodesToChange.Count; i++)
                {
                    if (map.TryGetValue(nodesToChange[i].InnerText, out guidLoop))
                    {
                        nodesToChange[i].InnerText = guidLoop.ToString();
                    }
                    else
                    {
                        nodesToChange[i].InnerText = new Guid().ToString(); //This creates 00.. guid in default formatting
                    }
                }
            }
        }

        internal void Save(XmlTextWriter writer)
        {
            writer.WriteStartElement("newskills");

            writer.WriteElementString("skillptsmax", SkillPointsMaximum.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("skillgrpsmax", SkillGroupPointsMaximum.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("uneducated", Uneducated.ToString());
            writer.WriteElementString("uncouth", Uncouth.ToString());
            writer.WriteElementString("schoolofhardknocks", SchoolOfHardKnocks.ToString());
            writer.WriteElementString("collegeeducation", CollegeEducation.ToString());
            writer.WriteElementString("jackofalltrades", JackOfAllTrades.ToString());
            writer.WriteElementString("techschool", TechSchool.ToString());
            writer.WriteElementString("linguist", Linguist.ToString());

            writer.WriteStartElement("skills");
            foreach (Skill skill in Skills)
            {
                skill.WriteTo(writer);
            }
            writer.WriteEndElement();
            writer.WriteStartElement("knoskills");
            foreach (KnowledgeSkill knowledgeSkill in KnowledgeSkills)
            {
                knowledgeSkill.WriteTo(writer);
            }
            writer.WriteEndElement();

            writer.WriteStartElement("skilljackknowledgeskills");
            foreach (KnowledgeSkill objSkill in KnowsoftSkills)
            {
                objSkill.WriteTo(writer);
            }
            writer.WriteEndElement();

            writer.WriteStartElement("groups");
            foreach (SkillGroup skillGroup in SkillGroups)
            {
                skillGroup.WriteTo(writer);
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        internal void Reset()
        {
            _skills.Clear();
            _dicSkills.Clear();
            KnowledgeSkills.Clear();
            SkillGroups.Clear();
            SkillPointsMaximum = 0;
            SkillGroupPointsMaximum = 0;
            KnowsoftSkills.Clear();
        }

        /// <summary>
        /// Maximum Skill Rating.
        /// </summary>
        public int MaxSkillRating { get; set; } = 0;

        private readonly BindingList<Skill> _skills = new BindingList<Skill>();
        private readonly Dictionary<string, Skill> _dicSkills = new Dictionary<string, Skill>();

        /// <summary>
        /// Active Skills
        /// </summary>
        public BindingList<Skill> Skills
        {
            get
            {
                if (_skills.Count == 0)
                {
                    GetSkillList(_character, FilterOptions.NonSpecial).ForEach(x => { _skills.Add(x); _dicSkills.Add(x.IsExoticSkill ? x.Name + " (" + x.DisplaySpecialization + ")" : x.Name, x); });
                }
                return _skills;
            }
        }

        /// <summary>
        /// Active Skills Dictionary
        /// </summary>
        public Dictionary<string, Skill> SkillsDictionary
        {
            get
            {
                return _dicSkills;
            }
        }

        /// <summary>
        /// Gets an active skill by its Name. Returns null if none found.
        /// </summary>
        /// <param name="strSkillName">Name of the skill.</param>
        /// <returns></returns>
        public Skill GetActiveSkill(string strSkillName)
        {
            Skill objReturn = null;
            _dicSkills.TryGetValue(strSkillName, out objReturn);
            return objReturn;
        }

        public BindingList<KnowledgeSkill> KnowledgeSkills { get; } = new BindingList<KnowledgeSkill>();


        /// <summary>
        /// KnowsoftSkills.
        /// </summary>
        public List<KnowledgeSkill> KnowsoftSkills { get; } = new List<KnowledgeSkill>();

        /// <summary>
        /// Skill Groups.
        /// </summary>
        public BindingList<SkillGroup> SkillGroups { get; } = new BindingList<SkillGroup>();

        public bool HasKnowledgePoints => KnowledgeSkillPoints > 0;

        /// <summary>
        /// Number of free Knowledge Skill Points the character has.
        /// </summary>
        public int KnowledgeSkillPoints
        {
            get
            {
                int fromAttributes;
                // Calculate Free Knowledge Skill Points. Free points = (INT + LOG) * 2.
                if (_character.Options.UseTotalValueForFreeKnowledge)
                {
                    fromAttributes = (_character.INT.TotalValue + _character.LOG.TotalValue);
                }
                else
                {
                    fromAttributes = (_character.INT.Value + _character.LOG.Value) ;
                }

                fromAttributes *= _character.Options.FreeKnowledgeMultiplier;

                int val = ImprovementManager.ValueOf(_character, Improvement.ImprovementType.FreeKnowledgeSkills);
                return fromAttributes + val;
            }
        }

        /// <summary>
        /// Number of free Knowledge skill points the character have remaining
        /// </summary>
        public int KnowledgeSkillPointsRemain
        {
            get { return KnowledgeSkillPoints - KnowledgeSkillPointsUsed; }
        }

        /// <summary>
        /// Number of knowledge skill points the character have used.
        /// </summary>
        public int KnowledgeSkillPointsUsed
        {
            get { return KnowledgeSkillRanksSum - SkillPointsSpentOnKnoskills; }
        }

        /// <summary>
        /// Sum of knowledge skill ranks the character has allocated.
        /// </summary>
        public int KnowledgeSkillRanksSum
        {
            get { return KnowledgeSkills.Sum(x => x.CurrentSpCost()); }
        }

        /// <summary>
        /// Number of Skill Points that have been spent on knowledge skills.
        /// </summary>
       public int SkillPointsSpentOnKnoskills
        {
            get
            {
                //Even if it is stupid, you can spend real skill points on knoskills...
                if (_character.BuildMethod == CharacterBuildMethod.Karma ||
                    _character.BuildMethod == CharacterBuildMethod.LifeModule)
                {
                    return 0;
                }
                int work = 0;
                if (KnowledgeSkillRanksSum > KnowledgeSkillPoints)
                    work -= KnowledgeSkillPoints - KnowledgeSkillRanksSum;
                return work;
            }
        }

        /// <summary>
        /// Number of free Skill Points the character has left.
        /// </summary>
        public int SkillPoints
        {
            get
            {
                if (SkillPointsMaximum == 0)
                {
                    return 0;
                }
                return SkillPointsMaximum - Skills.TotalCostSp() - SkillPointsSpentOnKnoskills;
            }
        }

        /// <summary>
        /// Number of maximum Skill Points the character has.
        /// </summary>
        public int SkillPointsMaximum { get; set; }

        /// <summary>
        /// Number of free Skill Points the character has.
        /// </summary>
        public int SkillGroupPoints
        {
            get { return SkillGroupPointsMaximum - SkillGroups.Sum(x => x.Base - x.FreeBase()); }
        }

        /// <summary>
        /// Number of maximum Skill Groups the character has.
        /// </summary>
        public int SkillGroupPointsMaximum { get; set; }

        /// <summary>
        /// Whether or not Uneducated is enabled.
        /// </summary>
        public bool Uneducated
        {
            get { return _blnUneducated; }
            set
            {
                bool blnOldValue = _blnUneducated;
                _blnUneducated = value;

                if (blnOldValue != value)
                    UneducatedChanged?.Invoke(_character);
            }
        }

        /// <summary>
        /// Whether or not Jack of All Trades is enabled.
        /// </summary>
        public bool JackOfAllTrades
        {
            get { return _blnJackOfAllTrades; }
            set
            {
                bool blnOldValue = _blnJackOfAllTrades;
                _blnJackOfAllTrades = value;

                if (blnOldValue != value)
                    JackOfAllTradesChanged?.Invoke(_character);
            }
        }

        /// <summary>
        /// Whether or not College Education is enabled.
        /// </summary>
        public bool CollegeEducation
        {
            get { return _blnCollegeEducation; }
            set
            {
                bool blnOldValue = _blnCollegeEducation;
                _blnCollegeEducation = value;

                if (blnOldValue != value)
                    CollegeEducationChanged?.Invoke(_character);
            }
        }

        /// <summary>
        /// Whether or not Uncouth is enabled.
        /// </summary>
        public bool Uncouth
        {
            get { return _blnUncouth; }
            set
            {
                bool blnOldValue = _blnUncouth;
                _blnUncouth = value;

                if (blnOldValue != value)
                    UncouthChanged?.Invoke(_character);
            }
        }

        /// <summary>
        /// Whether or not School of Hard Knocks is enabled.
        /// </summary>
        public bool SchoolOfHardKnocks
        {
            get { return _blnSchoolOfHardKnocks; }
            set
            {
                bool blnOldValue = _blnSchoolOfHardKnocks;
                _blnSchoolOfHardKnocks = value;

                if (blnOldValue != value)
                    SchoolOfHardKnocksChanged?.Invoke(_character);
            }
        }

        /// <summary>
        /// Whether or not TechSchool is enabled.
        /// </summary>
        public bool TechSchool
        {
            get { return _blnTechSchool; }
            set
            {
                bool blnOldValue = _blnTechSchool;
                _blnTechSchool = value;

                if (blnOldValue != value)
                    TechSchoolChanged?.Invoke(_character);
            }
        }

        /// <summary>
        /// Whether or not Linguist is enabled.
        /// </summary>
        public bool Linguist
        {
            get { return _blnLinguist; }
            set
            {
                bool blnOldValue = _blnLinguist;
                _blnLinguist = value;

                if (blnOldValue != value)
                    LinguistChanged?.Invoke(_character);
            }
        }
        public static int CompareSkills(Skill rhs, Skill lhs)
        {
            ExoticSkill rhsExoticSkill = (rhs.IsExoticSkill ? rhs : null) as ExoticSkill;
            ExoticSkill lhsExoticSkill = (lhs.IsExoticSkill ? lhs : null) as ExoticSkill;
            if (rhsExoticSkill != null)
            {
                if (lhsExoticSkill != null)
                {
                    return string.Compare(rhsExoticSkill.Specific ?? string.Empty, lhsExoticSkill.Specific ?? string.Empty, StringComparison.Ordinal);
                }
                else
                {
                    return 1;
                }
            }
            else if (lhsExoticSkill != null)
            {
                return -1;
            }

            return string.Compare(rhs.DisplayName, lhs.DisplayName, StringComparison.Ordinal);
        }

        public static IEnumerable<Skill> GetSkillList(Character c, FilterOptions filter, string strName = "")
        {
            //TODO less retarded way please
            List<Skill> b = new List<Skill>();
            // Load the Skills information.
            XmlDocument objXmlDocument = XmlManager.Load("skills.xml");

            // Populate the Skills list.
            XmlNodeList objXmlSkillList = objXmlDocument.SelectNodes("/chummer/skills/skill[not(exotic) and (" + c.Options.BookXPath() + ")" + SkillFilter(filter,strName) + "]");

            // First pass, build up a list of all of the Skills so we can sort them in alphabetical order for the current language.
            Dictionary<string, Skill> dicSkills = new Dictionary<string, Skill>(objXmlSkillList.Count);
            List<ListItem> lstSkillOrder = new List<ListItem>();
            foreach (XmlNode objXmlSkill in objXmlSkillList)
            {
                ListItem objSkillItem = new ListItem();
                objSkillItem.Value = objXmlSkill["name"]?.InnerText;
                objSkillItem.Name = objXmlSkill["translate"]?.InnerText ?? objSkillItem.Value;
                lstSkillOrder.Add(objSkillItem);
                //TODO: read from backup
                Skill objSkill = Skill.FromData(objXmlSkill, c);
                dicSkills.Add(objSkillItem.Value, objSkill);
            }
            SortListItem objSort = new SortListItem();
            lstSkillOrder.Sort(objSort.Compare);

            // Second pass, retrieve the Skills in the order they're presented in the list.
            foreach (ListItem objItem in lstSkillOrder)
            {
                b.Add(dicSkills[objItem.Value]);
            }
            return b;
        }

        private static string SkillFilter(FilterOptions filter, string name = "")
        {
            switch (filter)
            {
                case FilterOptions.All:
                    return string.Empty;
                case FilterOptions.NonSpecial:
                    return " and not(category = 'Magical Active') and not(category = 'Resonance Active')";
                case FilterOptions.Magician:
                    return " and category = 'Magical Active'";
                case FilterOptions.Sorcery:
                    return " and category = 'Magical Active' and (skillgroup = 'Sorcery' or skillgroup = '' or not(skillgroup))";
                case FilterOptions.Conjuring:
                    return " and category = 'Magical Active' and (skillgroup = 'Conjuring' or skillgroup = '' or not(skillgroup))";
                case FilterOptions.Enchanting:
                    return " and category = 'Magical Active' and (skillgroup = 'Enchanting' or skillgroup = '' or not(skillgroup))";
                case FilterOptions.Adept:
                case FilterOptions.Aware:
                case FilterOptions.Explorer:
                    return " and category = 'Magical Active' and (skillgroup = '' or not(skillgroup))";
                case FilterOptions.Spellcasting:
                    return " and category = 'Magical Active' and name = 'Spellcasting'";
                case FilterOptions.Technomancer:
                    return " and category = 'Resonance Active'";
                case FilterOptions.Name:
                    return $" and name = '{name}'";
                default:
                    throw new ArgumentOutOfRangeException(nameof(filter), filter, null);
            }
        }

        public enum FilterOptions
        {
            All,
            NonSpecial,
            Magician,
            Sorcery,
            Conjuring,
            Enchanting,
            Adept,
            Aware,
            Explorer,
            Technomancer,
            Spellcasting,
            Name
        }

        internal void ForceProperyChangedNotificationAll(string name)
        {
            foreach (Skill skill in Skills)
            {
                skill.ForceEvent(name);
            }

            foreach (KnowledgeSkill skill in KnowledgeSkills)
            {
                skill.ForceEvent(name);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [Obsolete("Should be private and stuff. Play a little once improvementManager gets events")]
        private void KnoChanged()
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(KnowledgeSkillPoints)));
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(HasKnowledgePoints)));
            }
        }


        public void Print(XmlTextWriter objWriter)
        {
            foreach (Skill skill in Skills)
            {
                if ((_character.Options.PrintSkillsWithZeroRating || skill.Rating > 0) && skill.Enabled)
                {
                    skill.Print(objWriter);
                }
            }

            foreach (SkillGroup skillgroup in SkillGroups)
            {
                if (skillgroup.Rating > 0)
                {
                    skillgroup.Print(objWriter);
                }
            }

            foreach (KnowledgeSkill skill in KnowledgeSkills)
            {
                skill.Print(objWriter);
            }
        }
    }
}
