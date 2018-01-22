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
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Chummer.Backend.Skills
{
    public class SkillsSection : INotifyPropertyChanged
    {
        private readonly Character _objCharacter;
        private readonly Dictionary<Guid, Skill> _skillValueBackup = new Dictionary<Guid, Skill>();
        private readonly static List<Skill> s_LstSkillBackups = new List<Skill>();

        public SkillsSection(Character character)
        {
            _objCharacter = character;
            _objCharacter.LOG.PropertyChanged += (sender, args) => KnoChanged();
            _objCharacter.INT.PropertyChanged += (sender, args) => KnoChanged();

            _objCharacter.SkillImprovementEvent += CharacterOnImprovementEvent;

        }

        private void CharacterOnImprovementEvent(ICollection<Improvement> improvements)
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
            List<Skill> lstExistingSkills = GetSkillList(_objCharacter, skills, strName).ToList();

            Skills.MergeInto(lstExistingSkills, CompareSkills, (objExistSkill, objNewSkill) =>
            {
                if (objNewSkill.Base > objExistSkill.Base)
                    objExistSkill.Base = objNewSkill.Base;
                if (objNewSkill.Karma > objExistSkill.Karma)
                    objExistSkill.Karma = objNewSkill.Karma;
                objExistSkill.Specializations.MergeInto(objNewSkill.Specializations, (x, y) => x.Free == y.Free ? String.Compare(x.DisplayName(GlobalOptions.Language), y.DisplayName(GlobalOptions.Language), StringComparison.Ordinal) : (x.Free ? 1 : -1));
            });
            foreach (Skill objSkill in lstExistingSkills)
            {
                string strKey = objSkill.IsExoticSkill ? objSkill.Name + " (" + objSkill.DisplaySpecializationMethod(GlobalOptions.DefaultLanguage) + ')' : objSkill.Name;
                if (!_dicSkills.ContainsKey(strKey))
                    _dicSkills.Add(strKey, objSkill);
            }
        }

        internal void RemoveSkills(FilterOptions skills, bool createKnowledge = true)
        {
            string strCategory;
            switch (skills)
            {
                case FilterOptions.Magician:
                case FilterOptions.Sorcery:
                case FilterOptions.Conjuring:
                case FilterOptions.Enchanting:
                case FilterOptions.Adept:
                    strCategory = "Magical Active";
                    break;
                case FilterOptions.Technomancer:
                    strCategory = "Resonance Active";
                    break;
                default:
                    return;
            }
            // Check for duplicates (we'd normally want to make sure it's enabled, but SpecialSkills doesn't process the Enabled property properly)
            foreach (Improvement objImprovement in _objCharacter.Improvements.Where(x => x.ImproveType == Improvement.ImprovementType.SpecialSkills))
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
                if (strLoopCategory == strCategory)
                    return;
            }

            for (int i = Skills.Count - 1; i >= 0; i--)
            {
                if (Skills[i].SkillCategory == strCategory)
                {
                    Skill skill = Skills[i];
                    _skillValueBackup[skill.SkillId] = skill;
                    s_LstSkillBackups.Add(skill);
                    Skills.RemoveAt(i);
                    SkillsDictionary.Remove(skill.IsExoticSkill ? skill.Name + " (" + skill.DisplaySpecializationMethod(GlobalOptions.DefaultLanguage) + ')' : skill.Name);
                    
                    if (_objCharacter.Created && skill.TotalBaseRating > 0 && createKnowledge)
                    {
                        KnowledgeSkill kno = new KnowledgeSkill(_objCharacter)
                        {
                            Type = skill.Name == "Arcana" ? "Academic" : "Professional",
                            WriteableName = skill.Name,
                            Base = skill.Base,
                            Karma = skill.Karma
                        };
                        kno.Specializations.AddRange(skill.Specializations);
                        KnowledgeSkills.MergeInto(kno, (x, y) => String.Compare(x.Type, y.Type, StringComparison.Ordinal) == 0 ? CompareSkills(x, y) : (String.Compare(x.Type, y.Type, StringComparison.Ordinal) == -1 ? -1 : 1), (objExistSkill, objNewSkill) =>
                        {
                            if (objNewSkill.Base > objExistSkill.Base)
                                objExistSkill.Base = objNewSkill.Base;
                            if (objNewSkill.Karma > objExistSkill.Karma)
                                objExistSkill.Karma = objNewSkill.Karma;
                            objExistSkill.Specializations.MergeInto(objNewSkill.Specializations, (x, y) => x.Free == y.Free ? String.Compare(x.DisplayName(GlobalOptions.Language), y.DisplayName(GlobalOptions.Language), StringComparison.Ordinal) : (x.Free ? 1 : -1));
                        });
                    }
                }
            }
            if (!_objCharacter.Created)
            {
                // zero out any skillgroups whose skills did not make the final cut
                foreach (SkillGroup objSkillGroup in SkillGroups)
                {
                    if (!objSkillGroup.SkillList.Any(x => SkillsDictionary.ContainsKey(x.Name)))
                    {
                        objSkillGroup.Base = 0;
                        objSkillGroup.Karma = 0;
                    }
                }
            }
        }

        internal void Load(XmlNode xmlSkillNode, bool blnLegacy = false)
        {
            if (xmlSkillNode == null)
                return;
            Timekeeper.Start("load_char_skills");

            if (!blnLegacy)
            {
                Timekeeper.Start("load_char_skills_groups");
                List<SkillGroup> lstLoadingSkillGroups = new List<SkillGroup>();
                foreach (XmlNode xmlNode in xmlSkillNode.SelectNodes("groups/group"))
                {
                    SkillGroup objGroup = new SkillGroup(_objCharacter);
                    objGroup.Load(xmlNode);
                        lstLoadingSkillGroups.Add(objGroup);
                }
                lstLoadingSkillGroups.Sort((i1, i2) => String.Compare(i2.DisplayName, i1.DisplayName, StringComparison.Ordinal));
                foreach (SkillGroup skillgroup in lstLoadingSkillGroups)
                {
                    SkillGroups.Add(skillgroup);
                }
                Timekeeper.Finish("load_char_skills_groups");

                Timekeeper.Start("load_char_skills_normal");
                //Load skills. Because sorting a BindingList is complicated we use a temporery normal list
                List<Skill> lstLoadingSkills = new List<Skill>();
                foreach (XmlNode xmlNode in xmlSkillNode.SelectNodes("skills/skill"))
                {
                    Skill objSkill = Skill.Load(_objCharacter, xmlNode);
                    if (objSkill != null)
                        lstLoadingSkills.Add(objSkill);
                }
                lstLoadingSkills.Sort(CompareSkills);


                foreach (Skill objSkill in lstLoadingSkills)
                {
                    string strName = objSkill.IsExoticSkill
                        ? $"{objSkill.Name} ({objSkill.DisplaySpecializationMethod(GlobalOptions.DefaultLanguage)})"
                        : objSkill.Name;
                    bool blnDoAddToDictionary = true;
                    _lstSkills.MergeInto(objSkill, CompareSkills, (objExistSkill, objNewSkill) =>
                    {
                        blnDoAddToDictionary = false;
                        if (objNewSkill.Base > objExistSkill.Base)
                            objExistSkill.Base = objNewSkill.Base;
                        if (objNewSkill.Karma > objExistSkill.Karma)
                            objExistSkill.Karma = objNewSkill.Karma;
                        objExistSkill.Specializations.MergeInto(objNewSkill.Specializations, (x, y) => x.Free == y.Free ? String.Compare(x.DisplayName(GlobalOptions.Language), y.DisplayName(GlobalOptions.Language), StringComparison.Ordinal) : (x.Free ? 1 : -1));
                    });
                    if (blnDoAddToDictionary)
                        _dicSkills.Add(strName, objSkill);
                }
                Timekeeper.Finish("load_char_skills_normal");

                Timekeeper.Start("load_char_skills_kno");
                foreach (XmlNode xmlNode in xmlSkillNode.SelectNodes("knoskills/skill"))
                {
                    if (Skill.Load(_objCharacter, xmlNode) is KnowledgeSkill objSkill)
                        KnowledgeSkills.Add(objSkill);
                }
                Timekeeper.Finish("load_char_skills_kno");

                Timekeeper.Start("load_char_knowsoft_buffer");
                // Knowsoft Buffer.
                foreach (XmlNode objXmlSkill in xmlSkillNode.SelectNodes("skilljackknowledgeskills/skill"))
                {
                    string strName = string.Empty;
                    if (objXmlSkill.TryGetStringFieldQuickly("name", ref strName))
                        KnowsoftSkills.Add(new KnowledgeSkill(_objCharacter, strName));
                }
                Timekeeper.Finish("load_char_knowsoft_buffer");
            }
            else
            {
                List<Skill> lstTempSkillList = new List<Skill>();
                foreach (XmlNode xmlNode in xmlSkillNode.SelectNodes("skills/skill"))
                {
                    Skill objSkill = Skill.LegacyLoad(_objCharacter, xmlNode);
                    if (objSkill != null)
                        lstTempSkillList.Add(objSkill);
                }

                if (lstTempSkillList.Count > 0)
                {
                    List<Skill> lstUnsortedSkills = new List<Skill>();

                    //Variable/Anon method as to not clutter anywhere else. Not sure if clever or stupid
                    bool OldSkillFilter(Skill skill)
                    {
                        if (skill.Rating > 0)
                            return true;

                        if (skill.SkillCategory == "Resonance Active" && !_objCharacter.RESEnabled)
                            return false;

                        //This could be more fine grained, but frankly i don't care
                        if (skill.SkillCategory == "Magical Active" && !_objCharacter.MAGEnabled)
                            return false;

                        return true;
                    }

                    foreach (Skill objSkill in lstTempSkillList)
                    {
                        if (objSkill is KnowledgeSkill objKnoSkill)
                        {
                            KnowledgeSkills.Add(objKnoSkill);
                        }
                        else if (OldSkillFilter(objSkill))
                        {
                            lstUnsortedSkills.Add(objSkill);
                        }
                    }

                    lstUnsortedSkills.Sort(CompareSkills);

                    foreach (Skill objSkill in lstUnsortedSkills)
                    {
                        _lstSkills.Add(objSkill);
                        _dicSkills.Add(objSkill.IsExoticSkill ? objSkill.Name + " (" + objSkill.DisplaySpecializationMethod(GlobalOptions.DefaultLanguage) + ')' : objSkill.Name, objSkill);
                    }

                    UpdateUndoList(xmlSkillNode);
                }
            }

            //This might give subtle bugs in the future, 
            //but right now it needs to be run once when upgrading or it might crash. 
            //As some didn't they crashed on loading skills. 
            //After this have run, it won't (for the crash i'm aware)
            //TODO: Move it to the other side of the if someday?
            
            if (!_objCharacter.Created)
            {
                // zero out any skillgroups whose skills did not make the final cut
                foreach (SkillGroup objSkillGroup in SkillGroups)
                {
                    if (!objSkillGroup.SkillList.Any(x => SkillsDictionary.ContainsKey(x.Name)))
                    {
                        objSkillGroup.Base = 0;
                        objSkillGroup.Karma = 0;
                    }
                }
            }

            //Workaround for probably breaking compability between earlier beta builds
            if (xmlSkillNode["skillptsmax"] == null)
            {
                xmlSkillNode = xmlSkillNode.OwnerDocument["character"];
            }

            int intTmp = 0;
            if (xmlSkillNode.TryGetInt32FieldQuickly("skillptsmax", ref intTmp))
                SkillPointsMaximum = intTmp;
            if (xmlSkillNode.TryGetInt32FieldQuickly("skillgrpsmax", ref intTmp))
                SkillGroupPointsMaximum = intTmp;

            Timekeeper.Finish("load_char_skills");
        }

        private void UpdateUndoList(XmlNode skillNode)
        {
            //Hacky way of converting Expense entries to guid based skill identification
            //specs allready did?
            //First create dictionary mapping name=>guid
            Dictionary<string, Guid> dicGroups = new Dictionary<string, Guid>();
            ConcurrentDictionary<string, Guid> dicSkills = new ConcurrentDictionary<string, Guid>();
            Parallel.Invoke(
                () =>
                {
                    foreach (SkillGroup objLoopSkillGroup in SkillGroups)
                    {
                        if (objLoopSkillGroup.Rating > 0 && !dicGroups.ContainsKey(objLoopSkillGroup.Name))
                        {
                            dicGroups.Add(objLoopSkillGroup.Name, objLoopSkillGroup.Id);
                        }
                    }
                },
                () =>
                {
                    foreach (Skill objLoopSkill in Skills)
                    {
                        if (objLoopSkill.TotalBaseRating > 0)
                        {
                            dicSkills.TryAdd(objLoopSkill.Name, objLoopSkill.Id);
                        }
                    }
                },
                () =>
                {
                    foreach (Skill objLoopSkill in KnowledgeSkills)
                    {
                        dicSkills.TryAdd(objLoopSkill.Name, objLoopSkill.Id);
                    }
                }
            );

            UpdateUndoSpecific(skillNode.OwnerDocument, dicSkills, new[] { KarmaExpenseType.AddSkill, KarmaExpenseType.ImproveSkill });
            UpdateUndoSpecific(skillNode.OwnerDocument, dicGroups, new[] { KarmaExpenseType.ImproveSkillGroup });
        }

        private static void UpdateUndoSpecific(XmlDocument doc, IDictionary<string, Guid> map, KarmaExpenseType[] typesRequreingConverting)
        {
            //Build a crazy xpath to get everything we want to convert

            string strXPath = $"/character/expenses/expense[type = \'Karma\']/undo[{string.Join(" or ", typesRequreingConverting.Select(x => $"karmatype = '{x}'"))}]/objectid";

            //Find everything
            XmlNodeList lstNodesToChange = doc.SelectNodes(strXPath);
            if (lstNodesToChange != null)
            {
                for (int i = 0; i < lstNodesToChange.Count; i++)
                {
                    if (map.TryGetValue(lstNodesToChange[i].InnerText, out Guid guidLoop))
                    {
                        lstNodesToChange[i].InnerText = guidLoop.ToString("D");
                    }
                    else
                    {
                        lstNodesToChange[i].InnerText = StringExtensions.EmptyGuid; //This creates 00.. guid in default formatting
                    }
                }
            }
        }

        internal void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("newskills");

            objWriter.WriteElementString("skillptsmax", SkillPointsMaximum.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("skillgrpsmax", SkillGroupPointsMaximum.ToString(CultureInfo.InvariantCulture));

            objWriter.WriteStartElement("skills");
            foreach (Skill objSkill in Skills)
            {
                objSkill.WriteTo(objWriter);
            }
            objWriter.WriteEndElement();
            objWriter.WriteStartElement("knoskills");
            foreach (KnowledgeSkill objKnowledgeSkill in KnowledgeSkills)
            {
                objKnowledgeSkill.WriteTo(objWriter);
            }
            objWriter.WriteEndElement();

            objWriter.WriteStartElement("skilljackknowledgeskills");
            foreach (KnowledgeSkill objSkill in KnowsoftSkills)
            {
                objSkill.WriteTo(objWriter);
            }
            objWriter.WriteEndElement();

            objWriter.WriteStartElement("groups");
            foreach (SkillGroup objSkillGroup in SkillGroups)
            {
                objSkillGroup.WriteTo(objWriter);
            }
            objWriter.WriteEndElement();
            objWriter.WriteEndElement();
        }

        internal void Reset()
        {
            _lstSkills.Clear();
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

        private readonly BindingList<Skill> _lstSkills = new BindingList<Skill>();
        private readonly Dictionary<string, Skill> _dicSkills = new Dictionary<string, Skill>();

        /// <summary>
        /// Active Skills
        /// </summary>
        public BindingList<Skill> Skills
        {
            get
            {
                if (_lstSkills.Count == 0)
                {
                    foreach (Skill objLoopSkill in GetSkillList(_objCharacter, FilterOptions.NonSpecial))
                    {
                        _lstSkills.Add(objLoopSkill);
                        _dicSkills.Add(objLoopSkill.IsExoticSkill ? objLoopSkill.Name + " (" + objLoopSkill.DisplaySpecializationMethod(GlobalOptions.DefaultLanguage) + ')' : objLoopSkill.Name, objLoopSkill);
                    }
                }
                return _lstSkills;
            }
        }

        /// <summary>
        /// Active Skills Dictionary
        /// </summary>
        public IDictionary<string, Skill> SkillsDictionary
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
            _dicSkills.TryGetValue(strSkillName, out Skill objReturn);
            return objReturn;
        }

        public BindingList<KnowledgeSkill> KnowledgeSkills { get; } = new BindingList<KnowledgeSkill>();


        /// <summary>
        /// KnowsoftSkills.
        /// </summary>
        public IList<KnowledgeSkill> KnowsoftSkills { get; } = new List<KnowledgeSkill>();

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
                if (_objCharacter.Options.UseTotalValueForFreeKnowledge)
                {
                    fromAttributes = (_objCharacter.INT.TotalValue + _objCharacter.LOG.TotalValue);
                }
                else
                {
                    fromAttributes = (_objCharacter.INT.Value + _objCharacter.LOG.Value) ;
                }

                fromAttributes *= _objCharacter.Options.FreeKnowledgeMultiplier;

                int val = ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.FreeKnowledgeSkills);
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
                if (_objCharacter.BuildMethod == CharacterBuildMethod.Karma ||
                    _objCharacter.BuildMethod == CharacterBuildMethod.LifeModule)
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
            get { return SkillGroupPointsMaximum - SkillGroups.Sum(x => x.Base - x.FreeBase); }
        }

        /// <summary>
        /// Number of maximum Skill Groups the character has.
        /// </summary>
        public int SkillGroupPointsMaximum { get; set; }

        public static int CompareSkills(Skill rhs, Skill lhs)
        {
            ExoticSkill lhsExoticSkill = (lhs.IsExoticSkill ? lhs : null) as ExoticSkill;
            if ((rhs.IsExoticSkill ? rhs : null) is ExoticSkill rhsExoticSkill)
            {
                if (lhsExoticSkill != null)
                {
                    return string.Compare(rhsExoticSkill.DisplaySpecific(GlobalOptions.Language), lhsExoticSkill.DisplaySpecific(GlobalOptions.Language) ?? string.Empty, StringComparison.Ordinal);
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

            return string.Compare(rhs.DisplayNameMethod(GlobalOptions.Language), lhs.DisplayNameMethod(GlobalOptions.Language), StringComparison.Ordinal);
        }

        public static IEnumerable<Skill> GetSkillList(Character c, FilterOptions filter, string strName = "")
        {
            //TODO less retarded way please
            // Load the Skills information.
            XmlDocument objXmlDocument = XmlManager.Load("skills.xml");

            // Populate the Skills list.
            XmlNodeList objXmlSkillList = objXmlDocument.SelectNodes("/chummer/skills/skill[not(exotic) and (" + c.Options.BookXPath() + ')' + SkillFilter(filter,strName) + "]");

            // First pass, build up a list of all of the Skills so we can sort them in alphabetical order for the current language.
            Dictionary<string, Skill> dicSkills = new Dictionary<string, Skill>(objXmlSkillList.Count);
            List<ListItem> lstSkillOrder = new List<ListItem>();
            foreach (XmlNode objXmlSkill in objXmlSkillList)
            {
                string strSkillName = objXmlSkill["name"]?.InnerText ?? string.Empty;
                lstSkillOrder.Add(new ListItem(strSkillName, objXmlSkill["translate"]?.InnerText ?? strSkillName));
                //TODO: read from backup
                if (s_LstSkillBackups.Count > 0)
                {
                    Skill objSkill = s_LstSkillBackups.FirstOrDefault(s => s.SkillId == Guid.Parse(objXmlSkill["id"].InnerText));
                    if (objSkill != null)
                    {
                        dicSkills.Add(objSkill.Name,objSkill);
                        s_LstSkillBackups.Remove(objSkill);
                    }
                }
                else
                {
                    Skill objSkill = Skill.FromData(objXmlSkill, c);
                    dicSkills.Add(strSkillName, objSkill);
                }
            }
            lstSkillOrder.Sort(CompareListItems.CompareNames);

            // Second pass, retrieve the Skills in the order they're presented in the list.
            foreach (ListItem objItem in lstSkillOrder)
            {
                yield return dicSkills[objItem.Value.ToString()];
            }
        }

        private static string SkillFilter(FilterOptions eFilter, string strName = "")
        {
            switch (eFilter)
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
                    return $" and name = '{strName}'";
                default:
                    throw new ArgumentOutOfRangeException(nameof(eFilter), eFilter, null);
            }
        }

        public enum FilterOptions
        {
            All = 0,
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
            Name,
        }

        internal void ForceProperyChangedNotificationAll(string strName)
        {
            foreach (Skill objSkill in Skills)
            {
                objSkill.ForceEvent(strName);
            }

            foreach (KnowledgeSkill objSkill in KnowledgeSkills)
            {
                objSkill.ForceEvent(strName);
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


        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            foreach (Skill objSkill in Skills)
            {
                if ((_objCharacter.Options.PrintSkillsWithZeroRating || objSkill.Rating > 0) && objSkill.Enabled)
                {
                    objSkill.Print(objWriter, objCulture, strLanguageToPrint);
                }
            }

            foreach (SkillGroup objSkillGroup in SkillGroups)
            {
                if (objSkillGroup.Rating > 0)
                {
                    objSkillGroup.Print(objWriter, objCulture, strLanguageToPrint);
                }
            }

            foreach (KnowledgeSkill objSkill in KnowledgeSkills)
            {
                objSkill.Print(objWriter, objCulture, strLanguageToPrint);
            }
        }
    }
}
