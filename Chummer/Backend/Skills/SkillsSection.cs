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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using Chummer.Annotations;

namespace Chummer.Backend.Skills
{
    public class SkillsSection : INotifyMultiplePropertyChanged
    {
        private readonly Character _objCharacter;
        private readonly Dictionary<Guid, Skill> _dicSkillBackups = new Dictionary<Guid, Skill>();

        public SkillsSection(Character character)
        {
            _objCharacter = character;
            if (_objCharacter != null)
            {
                _objCharacter.PropertyChanged += OnCharacterPropertyChanged;
                _objCharacter.Settings.PropertyChanged += OnCharacterSettingsPropertyChanged;
            }
            KnowledgeSkills.BeforeRemove += KnowledgeSkillsOnBeforeRemove;
            KnowledgeSkills.ListChanged += KnowledgeSkillsOnListChanged;
            _lstSkills.BeforeRemove += SkillsOnBeforeRemove;
            _lstSkills.ListChanged += SkillsOnListChanged;
        }

        private void SkillsOnListChanged(object sender, ListChangedEventArgs e)
        {
            switch (e.ListChangedType)
            {
                case ListChangedType.Reset:
                    _dicSkills.Clear();
                    foreach (Skill objSkill in _lstSkills)
                    {
                        if (!_dicSkills.ContainsKey(objSkill.DictionaryKey))
                            _dicSkills.Add(objSkill.DictionaryKey, objSkill);
                    }
                    break;

                case ListChangedType.ItemAdded:
                    Skill objNewSkill = _lstSkills[e.NewIndex];
                    if (!_dicSkills.ContainsKey(objNewSkill.DictionaryKey))
                        _dicSkills.Add(objNewSkill.DictionaryKey, objNewSkill);
                    break;
            }
        }

        private void SkillsOnBeforeRemove(object sender, RemovingOldEventArgs e)
        {
            _dicSkills.Remove(_lstSkills[e.OldIndex].DictionaryKey);
        }

        private void KnowledgeSkillsOnBeforeRemove(object sender, RemovingOldEventArgs e)
        {
            KnowledgeSkills[e.OldIndex].PropertyChanged -= OnKnowledgeSkillPropertyChanged;
        }

        private void KnowledgeSkillsOnListChanged(object sender, ListChangedEventArgs e)
        {
            switch (e.ListChangedType)
            {
                case ListChangedType.Reset:
                    foreach (KnowledgeSkill objKnoSkill in KnowledgeSkills)
                        objKnoSkill.PropertyChanged += OnKnowledgeSkillPropertyChanged;
                    goto case ListChangedType.ItemDeleted;
                case ListChangedType.ItemAdded:
                    KnowledgeSkills[e.NewIndex].PropertyChanged += OnKnowledgeSkillPropertyChanged;
                    goto case ListChangedType.ItemDeleted;
                case ListChangedType.ItemDeleted:
                    OnMultiplePropertyChanged(nameof(KnowledgeSkillRanksSum), nameof(HasAvailableNativeLanguageSlots));
                    break;
            }
        }

        private void OnKnowledgeSkillPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(KnowledgeSkill.CurrentSpCost):
                    OnPropertyChanged(nameof(KnowledgeSkillRanksSum));
                    break;

                case nameof(KnowledgeSkill.IsNativeLanguage):
                    OnPropertyChanged(nameof(HasAvailableNativeLanguageSlots));
                    break;
            }
        }

        public void UnbindSkillsSection()
        {
            if (_objCharacter != null)
            {
                _objCharacter.PropertyChanged -= OnCharacterPropertyChanged;
                _objCharacter.Settings.PropertyChanged -= OnCharacterSettingsPropertyChanged;
            }
            _dicSkillBackups.Clear();
        }

        private void OnCharacterPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e?.PropertyName == nameof(Character.EffectiveBuildMethodUsesPriorityTables))
                OnPropertyChanged(nameof(SkillPointsSpentOnKnoskills));
        }

        private void OnCharacterSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e?.PropertyName == nameof(CharacterSettings.KnowledgePointsExpression))
                OnPropertyChanged(nameof(KnowledgeSkillPoints));
        }

        [NotifyPropertyChangedInvocator]
        public void OnPropertyChanged([CallerMemberName] string strPropertyName = null)
        {
            OnMultiplePropertyChanged(strPropertyName);
        }

        public void OnMultiplePropertyChanged(params string[] lstPropertyNames)
        {
            HashSet<string> lstNamesOfChangedProperties = null;
            foreach (string strPropertyName in lstPropertyNames)
            {
                if (lstNamesOfChangedProperties == null)
                    lstNamesOfChangedProperties = s_SkillSectionDependencyGraph.GetWithAllDependents(this, strPropertyName);
                else
                {
                    foreach (string strLoopChangedProperty in s_SkillSectionDependencyGraph.GetWithAllDependents(this, strPropertyName))
                        lstNamesOfChangedProperties.Add(strLoopChangedProperty);
                }
            }

            if (lstNamesOfChangedProperties == null || lstNamesOfChangedProperties.Count == 0)
                return;
            if (lstNamesOfChangedProperties.Contains(nameof(KnowledgeSkillPoints)))
                _intCachedKnowledgePoints = int.MinValue;

            foreach (string strPropertyToChange in lstNamesOfChangedProperties)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strPropertyToChange));
            }
        }

        internal void AddSkills(FilterOption skills, string strName = "")
        {
            List<Skill> lstExistingSkills = GetSkillList(skills, strName, true).ToList();

            Skills.AddRangeWithSort(lstExistingSkills, CompareSkills, (objExistSkill, objNewSkill) =>
            {
                if (objNewSkill.Base > objExistSkill.Base)
                    objExistSkill.Base = objNewSkill.Base;
                if (objNewSkill.Karma > objExistSkill.Karma)
                    objExistSkill.Karma = objNewSkill.Karma;
                objExistSkill.Specializations.AddRangeWithSort(objNewSkill.Specializations,
                    (x, y) => x.Free == y.Free
                        ? string.Compare(x.CurrentDisplayName, y.CurrentDisplayName, false, GlobalSettings.CultureInfo)
                        : (x.Free ? 1 : -1));
            });
        }

        internal ExoticSkill AddExoticSkill(string strName, string strSpecific)
        {
            XmlNode xmlSkillNode = _objCharacter.LoadData("skills.xml").SelectSingleNode("/chummer/skills/skill[name = " + strName.CleanXPath() + "]");
            ExoticSkill objExoticSkill = new ExoticSkill(_objCharacter, xmlSkillNode)
            {
                Specific = strSpecific
            };
            Skills.AddWithSort(objExoticSkill, CompareSkills, (objExistSkill, objNewSkill) =>
            {
                if (objNewSkill.Base > objExistSkill.Base)
                    objExistSkill.Base = objNewSkill.Base;
                if (objNewSkill.Karma > objExistSkill.Karma)
                    objExistSkill.Karma = objNewSkill.Karma;
                objExistSkill.Specializations.AddRangeWithSort(objNewSkill.Specializations,
                    (x, y) => x.Free == y.Free
                        ? string.Compare(x.CurrentDisplayName, y.CurrentDisplayName, false, GlobalSettings.CultureInfo)
                        : (x.Free ? 1 : -1));
            });
            return objExoticSkill;
        }

        internal void RemoveSkills(FilterOption skills, bool createKnowledge = true)
        {
            string strCategory;
            switch (skills)
            {
                case FilterOption.Magician:
                case FilterOption.Sorcery:
                case FilterOption.Conjuring:
                case FilterOption.Enchanting:
                case FilterOption.Adept:
                    strCategory = "Magical Active";
                    break;

                case FilterOption.Technomancer:
                    strCategory = "Resonance Active";
                    break;

                default:
                    return;
            }
            // Check for duplicates (we'd normally want to make sure it's enabled, but SpecialSkills doesn't process the Enabled property properly)
            foreach (Improvement objImprovement in _objCharacter.Improvements.Where(x => x.ImproveType == Improvement.ImprovementType.SpecialSkills))
            {
                FilterOption eLoopFilter = (FilterOption)Enum.Parse(typeof(FilterOption), objImprovement.ImprovedName);
                string strLoopCategory = string.Empty;
                switch (eLoopFilter)
                {
                    case FilterOption.Magician:
                    case FilterOption.Sorcery:
                    case FilterOption.Conjuring:
                    case FilterOption.Enchanting:
                    case FilterOption.Adept:
                        strLoopCategory = "Magical Active";
                        break;

                    case FilterOption.Technomancer:
                        strLoopCategory = "Resonance Active";
                        break;
                }
                if (strLoopCategory == strCategory)
                    return;
            }

            for (int i = Skills.Count - 1; i >= 0; --i)
            {
                if (Skills[i].SkillCategory == strCategory)
                {
                    Skill skill = Skills[i];
                    _dicSkillBackups.Add(skill.SkillId, skill);
                    Skills.RemoveAt(i);

                    if (_objCharacter.Created && skill.TotalBaseRating > 0 && createKnowledge)
                    {
                        KnowledgeSkill kno = new KnowledgeSkill(_objCharacter)
                        {
                            Type = skill.Name == "Arcana" ? "Academic" : "Professional",
                            WritableName = skill.Name,
                            Base = skill.Base,
                            Karma = skill.Karma
                        };
                        kno.Specializations.AddRange(skill.Specializations);
                        KnowledgeSkills.AddWithSort(kno, (x, y) => string.Compare(x.Type, y.Type, StringComparison.Ordinal) == 0 ? CompareSkills(x, y) : (string.Compare(x.Type, y.Type, StringComparison.Ordinal) == -1 ? -1 : 1), (objExistSkill, objNewSkill) =>
                        {
                            if (objNewSkill.Base > objExistSkill.Base)
                                objExistSkill.Base = objNewSkill.Base;
                            if (objNewSkill.Karma > objExistSkill.Karma)
                                objExistSkill.Karma = objNewSkill.Karma;
                            objExistSkill.Specializations.AddRangeWithSort(objNewSkill.Specializations, (x, y) => x.Free == y.Free ? string.Compare(x.CurrentDisplayName, y.CurrentDisplayName, false, GlobalSettings.CultureInfo) : (x.Free ? 1 : -1));
                        });
                    }
                }
            }
            if (!_objCharacter.Created)
            {
                // zero out any skillgroups whose skills did not make the final cut
                foreach (SkillGroup objSkillGroup in SkillGroups)
                {
                    if (!objSkillGroup.SkillList.Any(x => _dicSkills.ContainsKey(x.DictionaryKey)))
                    {
                        objSkillGroup.Base = 0;
                        objSkillGroup.Karma = 0;
                    }
                }
            }
        }

        internal void Load(XmlNode xmlSkillNode, bool blnLegacy, CustomActivity parentActivity)
        {
            if (xmlSkillNode == null)
                return;
            using (var opLoadCharSkills = Timekeeper.StartSyncron("load_char_skills_skillnode", parentActivity))
            {
                if (!blnLegacy)
                {
                    using (_ = Timekeeper.StartSyncron("load_char_skills_groups", opLoadCharSkills))
                    {
                        List<SkillGroup> lstLoadingSkillGroups;
                        using (XmlNodeList xmlGroupsList = xmlSkillNode.SelectNodes("groups/group"))
                        {
                            lstLoadingSkillGroups = new List<SkillGroup>(xmlGroupsList?.Count ?? 0);
                            if (xmlGroupsList?.Count > 0)
                            {
                                foreach (XmlNode xmlNode in xmlGroupsList)
                                {
                                    string strName = xmlNode["name"]?.InnerText;
                                    SkillGroup objGroup = null;
                                    if (!string.IsNullOrEmpty(strName))
                                        objGroup = SkillGroups.FirstOrDefault(x => x.Name == strName);
                                    if (objGroup == null)
                                    {
                                        objGroup = new SkillGroup(_objCharacter, strName);
                                        objGroup.Load(xmlNode);
                                        lstLoadingSkillGroups.Add(objGroup);
                                    }
                                    else
                                        objGroup.Load(xmlNode);
                                }
                            }
                        }

                        lstLoadingSkillGroups.Sort((i1, i2) =>
                            string.Compare(i2.CurrentDisplayName, i1.CurrentDisplayName, false, GlobalSettings.CultureInfo));
                        foreach (SkillGroup skillgroup in lstLoadingSkillGroups)
                        {
                            SkillGroups.Add(skillgroup);
                        }

                        //Timekeeper.Finish("load_char_skills_groups");
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_skills_normal", opLoadCharSkills))
                    {
                        //Load skills. Because sorting a BindingList is complicated we use a temporery normal list
                        List<Skill> lstLoadingSkills;
                        using (XmlNodeList xmlSkillsList = xmlSkillNode.SelectNodes("skills/skill"))
                        {
                            lstLoadingSkills = new List<Skill>(xmlSkillsList?.Count ?? 0);
                            if (xmlSkillsList?.Count > 0)
                            {
                                foreach (XmlNode xmlNode in xmlSkillsList)
                                {
                                    Skill objSkill = Skill.Load(_objCharacter, xmlNode);
                                    if (objSkill != null)
                                        lstLoadingSkills.Add(objSkill);
                                }
                            }
                        }

                        lstLoadingSkills.Sort(CompareSkills);

                        foreach (Skill objSkill in lstLoadingSkills)
                        {
                            _lstSkills.AddWithSort(objSkill, CompareSkills, (objExistSkill, objNewSkill) =>
                            {
                                if (objNewSkill.Base > objExistSkill.Base)
                                    objExistSkill.Base = objNewSkill.Base;
                                if (objNewSkill.Karma > objExistSkill.Karma)
                                    objExistSkill.Karma = objNewSkill.Karma;
                                objExistSkill.Specializations.AddRangeWithSort(objNewSkill.Specializations,
                                    (x, y) => x.Free == y.Free
                                        ? string.Compare(x.CurrentDisplayName,
                                            y.CurrentDisplayName, false, GlobalSettings.CultureInfo)
                                        : (x.Free ? 1 : -1));
                            });
                        }

                        // TODO: Skill groups don't refresh their CanIncrease property correctly when the last of their skills is being added, as the total basse rating will be zero. Call this here to force a refresh.
                        foreach (SkillGroup g in SkillGroups)
                        {
                            g.OnPropertyChanged(nameof(SkillGroup.SkillList));
                        }

                        //Timekeeper.Finish("load_char_skills_normal");
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_skills_kno", opLoadCharSkills))
                    {
                        using (XmlNodeList xmlSkillsList = xmlSkillNode.SelectNodes("knoskills/skill"))
                        {
                            if (xmlSkillsList != null)
                            {
                                foreach (XmlNode xmlNode in xmlSkillsList)
                                {
                                    if (Skill.Load(_objCharacter, xmlNode) is KnowledgeSkill objSkill)
                                        KnowledgeSkills.Add(objSkill);
                                }
                            }
                        }

                        // Legacy sweep for native language skills
                        if (_objCharacter.LastSavedVersion <= new Version(5, 212, 72) && _objCharacter.Created && !KnowledgeSkills.Any(x => x.IsNativeLanguage))
                        {
                            KnowledgeSkill objEnglishSkill = new KnowledgeSkill(_objCharacter)
                            {
                                WritableName = "English",
                                IsNativeLanguage = true
                            };
                            KnowledgeSkills.Add(objEnglishSkill);
                        }
                        //Timekeeper.Finish("load_char_skills_kno");
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_knowsoft_buffer", opLoadCharSkills))
                    {
                        // Knowsoft Buffer.
                        using (XmlNodeList xmlSkillsList = xmlSkillNode.SelectNodes("skilljackknowledgeskills/skill"))
                        {
                            if (xmlSkillsList != null)
                            {
                                foreach (XmlNode xmlNode in xmlSkillsList)
                                {
                                    string strName = string.Empty;
                                    if (xmlNode.TryGetStringFieldQuickly("name", ref strName))
                                        KnowsoftSkills.Add(new KnowledgeSkill(_objCharacter, strName, false));
                                }
                            }
                        }

                        //Timekeeper.Finish("load_char_knowsoft_buffer");
                    }
                }
                else
                {
                    List<Skill> lstTempSkillList;
                    using (XmlNodeList xmlSkillsList = xmlSkillNode.SelectNodes("skills/skill"))
                    {
                        lstTempSkillList = new List<Skill>(xmlSkillsList?.Count ?? 0);
                        if (xmlSkillsList?.Count > 0)
                        {
                            foreach (XmlNode xmlNode in xmlSkillsList)
                            {
                                Skill objSkill = Skill.LegacyLoad(_objCharacter, xmlNode);
                                if (objSkill != null)
                                    lstTempSkillList.Add(objSkill);
                            }
                        }
                    }

                    if (lstTempSkillList.Count > 0)
                    {
                        List<Skill> lstUnsortedSkills = new List<Skill>(lstTempSkillList.Count);

                        //Variable/Anon method as to not clutter anywhere else. Not sure if clever or stupid
                        bool OldSkillFilter(Skill skill)
                        {
                            if (skill.Rating > 0)
                                return true;

                            if (skill.SkillCategory == "Resonance Active" && !_objCharacter.RESEnabled)
                                return false;

                            //This could be more fine grained, but frankly i don't care
                            return skill.SkillCategory != "Magical Active" || _objCharacter.MAGEnabled;
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
                        }

                        UpdateUndoList(xmlSkillNode.OwnerDocument);
                    }
                }

                XPathNavigator skillsDocXPath = _objCharacter.LoadDataXPath("skills.xml");
                HashSet<string> hashSkillGuids = new HashSet<string>();
                foreach (XPathNavigator node in skillsDocXPath.Select(
                    string.Format(GlobalSettings.InvariantCultureInfo, "/chummer/skills/skill[not(exotic) and ({0}){1}]",
                        _objCharacter.Settings.BookXPath(), SkillFilter(FilterOption.NonSpecial))))
                {
                    string strName = node.SelectSingleNode("name")?.Value;
                    if (!string.IsNullOrEmpty(strName))
                        hashSkillGuids.Add(strName);
                }
                XmlDocument skillsDoc = _objCharacter.LoadData("skills.xml");
                foreach (string skillId in hashSkillGuids.Where(s => Skills.All(skill => skill.Name != s)))
                {
                    XmlNode objXmlSkillNode = skillsDoc.SelectSingleNode("/chummer/skills/skill[name = " + skillId.CleanXPath() + ']');
                    if (objXmlSkillNode != null)
                    {
                        Skill objSkill = Skill.FromData(objXmlSkillNode, _objCharacter, false);
                        Skills.Add(objSkill);
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
                        if (!objSkillGroup.SkillList.Any(x => _dicSkills.ContainsKey(x.DictionaryKey)))
                        {
                            objSkillGroup.Base = 0;
                            objSkillGroup.Karma = 0;
                        }
                    }
                }

                //Workaround for probably breaking compability between earlier beta builds
                if (xmlSkillNode["skillptsmax"] == null)
                {
                    xmlSkillNode = xmlSkillNode.OwnerDocument?["character"];
                }

                int intTmp = 0;
                if (xmlSkillNode.TryGetInt32FieldQuickly("skillptsmax", ref intTmp))
                    SkillPointsMaximum = intTmp;
                if (xmlSkillNode.TryGetInt32FieldQuickly("skillgrpsmax", ref intTmp))
                    SkillGroupPointsMaximum = intTmp;

                //Timekeeper.Finish("load_char_skills");
            }
        }

        internal void LoadFromHeroLab(XPathNavigator xmlSkillNode, CustomActivity parentActivity)
        {
            using (_ = Timekeeper.StartSyncron("load_char_skills_groups", parentActivity))
            {
                List<SkillGroup> lstLoadingSkillGroups = new List<SkillGroup>();
                foreach (XPathNavigator xmlNode in xmlSkillNode.SelectAndCacheExpression("groups/skill"))
                {
                    SkillGroup objGroup = new SkillGroup(_objCharacter);
                    objGroup.LoadFromHeroLab(xmlNode);
                    lstLoadingSkillGroups.Add(objGroup);
                }

                lstLoadingSkillGroups.Sort((i1, i2) =>
                    string.Compare(i2.CurrentDisplayName, i1.CurrentDisplayName, false, GlobalSettings.CultureInfo));
                foreach (SkillGroup skillgroup in lstLoadingSkillGroups)
                {
                    SkillGroups.Add(skillgroup);
                }

                //Timekeeper.Finish("load_char_skills_groups");
            }

            using (_ = Timekeeper.StartSyncron("load_char_skills", parentActivity))
            {
                List<Skill> lstTempSkillList = new List<Skill>();
                foreach (XPathNavigator xmlNode in xmlSkillNode.SelectAndCacheExpression("active/skill"))
                {
                    Skill objSkill = Skill.LoadFromHeroLab(_objCharacter, xmlNode, false);
                    if (objSkill != null)
                        lstTempSkillList.Add(objSkill);
                }
                foreach (XPathNavigator xmlNode in xmlSkillNode.SelectAndCacheExpression("knowledge/skill"))
                {
                    Skill objSkill = Skill.LoadFromHeroLab(_objCharacter, xmlNode, true);
                    if (objSkill != null)
                        lstTempSkillList.Add(objSkill);
                }
                foreach (XPathNavigator xmlNode in xmlSkillNode.SelectAndCacheExpression("language/skill"))
                {
                    Skill objSkill = Skill.LoadFromHeroLab(_objCharacter, xmlNode, true, "Language");
                    if (objSkill != null)
                        lstTempSkillList.Add(objSkill);
                }

                List<Skill> lstUnsortedSkills = new List<Skill>(lstTempSkillList.Count);

                //Variable/Anon method as to not clutter anywhere else. Not sure if clever or stupid
                bool OldSkillFilter(Skill skill)
                {
                    if (skill.Rating > 0)
                        return true;

                    if (skill.SkillCategory == "Resonance Active" && !_objCharacter.RESEnabled)
                        return false;

                    //This could be more fine grained, but frankly i don't care
                    return skill.SkillCategory != "Magical Active" || _objCharacter.MAGEnabled;
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
                    _lstSkills.AddWithSort(objSkill, CompareSkills, (objExistSkill, objNewSkill) =>
                    {
                        if (objNewSkill.Base > objExistSkill.Base)
                            objExistSkill.Base = objNewSkill.Base;
                        if (objNewSkill.Karma > objExistSkill.Karma)
                            objExistSkill.Karma = objNewSkill.Karma;
                        objExistSkill.Specializations.AddRangeWithSort(objNewSkill.Specializations,
                            (x, y) => x.Free == y.Free
                                ? string.Compare(x.CurrentDisplayName,
                                    y.CurrentDisplayName, false, GlobalSettings.CultureInfo)
                                : (x.Free ? 1 : -1));
                    });
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
                        if (!objSkillGroup.SkillList.Any(x => _dicSkills.ContainsKey(x.DictionaryKey)))
                        {
                            objSkillGroup.Base = 0;
                            objSkillGroup.Karma = 0;
                        }
                    }

                    if (_objCharacter.EffectiveBuildMethodUsesPriorityTables)
                    {
                        // Allocate Skill Points
                        int intSkillPointCount = SkillPointsMaximum;
                        Skill objSkillToPutPointsInto;

                        // First loop through skills where costs can be 100% covered with points
                        do
                        {
                            objSkillToPutPointsInto = null;
                            int intSkillToPutPointsIntoTotalKarmaCost = 0;
                            foreach (Skill objLoopSkill in Skills)
                            {
                                if (objLoopSkill.Karma == 0)
                                    continue;
                                // Put points into the attribute with the highest total karma cost.
                                // In case of ties, pick the one that would need more points to cover it (the other one will hopefully get picked up at a later cycle)
                                int intLoopTotalKarmaCost = objLoopSkill.CurrentKarmaCost;
                                if (objSkillToPutPointsInto == null
                                    || (objLoopSkill.Karma <= intSkillPointCount
                                        && (intLoopTotalKarmaCost > intSkillToPutPointsIntoTotalKarmaCost
                                            || (intLoopTotalKarmaCost == intSkillToPutPointsIntoTotalKarmaCost
                                                && objLoopSkill.Karma > objSkillToPutPointsInto.Karma))))
                                {
                                    objSkillToPutPointsInto = objLoopSkill;
                                    intSkillToPutPointsIntoTotalKarmaCost = intLoopTotalKarmaCost;
                                }
                            }

                            if (objSkillToPutPointsInto != null)
                            {
                                objSkillToPutPointsInto.Base = objSkillToPutPointsInto.Karma;
                                intSkillPointCount -= objSkillToPutPointsInto.Karma;
                                objSkillToPutPointsInto.Karma = 0;
                            }
                        } while (objSkillToPutPointsInto != null && intSkillPointCount > 0);

                        // If any points left over, then put them all into the attribute with the highest karma cost
                        if (intSkillPointCount > 0 && Skills.Any(x => x.Karma != 0))
                        {
                            int intHighestTotalKarmaCost = 0;
                            foreach (Skill objLoopSkill in Skills)
                            {
                                if (objLoopSkill.Karma == 0)
                                    continue;
                                // Put points into the attribute with the highest total karma cost.
                                // In case of ties, pick the one that would need more points to cover it (the other one will hopefully get picked up at a later cycle)
                                int intLoopTotalKarmaCost = objLoopSkill.CurrentKarmaCost;
                                if (objSkillToPutPointsInto == null
                                    || intLoopTotalKarmaCost > intHighestTotalKarmaCost
                                    || (intLoopTotalKarmaCost == intHighestTotalKarmaCost
                                        && objLoopSkill.Karma > objSkillToPutPointsInto.Karma))
                                {
                                    objSkillToPutPointsInto = objLoopSkill;
                                    intHighestTotalKarmaCost = intLoopTotalKarmaCost;
                                }
                            }

                            if (objSkillToPutPointsInto != null)
                            {
                                objSkillToPutPointsInto.Base = intSkillPointCount;
                                objSkillToPutPointsInto.Karma -= intSkillPointCount;
                            }
                        }
                    }

                    // Allocate Knowledge Skill Points
                    int intKnowledgeSkillPointCount = KnowledgeSkillPoints;
                    Skill objKnowledgeSkillToPutPointsInto;

                    // First loop through skills where costs can be 100% covered with points
                    do
                    {
                        objKnowledgeSkillToPutPointsInto = null;
                        int intKnowledgeSkillToPutPointsIntoTotalKarmaCost = 0;
                        foreach (KnowledgeSkill objLoopKnowledgeSkill in KnowledgeSkills)
                        {
                            if (objLoopKnowledgeSkill.Karma == 0)
                                continue;
                            // Put points into the attribute with the highest total karma cost.
                            // In case of ties, pick the one that would need more points to cover it (the other one will hopefully get picked up at a later cycle)
                            int intLoopTotalKarmaCost = objLoopKnowledgeSkill.CurrentKarmaCost;
                            if (objKnowledgeSkillToPutPointsInto == null
                                || (objLoopKnowledgeSkill.Karma <= intKnowledgeSkillPointCount
                                    && (intLoopTotalKarmaCost > intKnowledgeSkillToPutPointsIntoTotalKarmaCost
                                        || (intLoopTotalKarmaCost == intKnowledgeSkillToPutPointsIntoTotalKarmaCost
                                            && objLoopKnowledgeSkill.Karma > objKnowledgeSkillToPutPointsInto.Karma))))
                            {
                                objKnowledgeSkillToPutPointsInto = objLoopKnowledgeSkill;
                                intKnowledgeSkillToPutPointsIntoTotalKarmaCost = intLoopTotalKarmaCost;
                            }
                        }

                        if (objKnowledgeSkillToPutPointsInto != null)
                        {
                            objKnowledgeSkillToPutPointsInto.Base = objKnowledgeSkillToPutPointsInto.Karma;
                            intKnowledgeSkillPointCount -= objKnowledgeSkillToPutPointsInto.Karma;
                            objKnowledgeSkillToPutPointsInto.Karma = 0;
                        }
                    } while (objKnowledgeSkillToPutPointsInto != null && intKnowledgeSkillPointCount > 0);

                    // If any points left over, then put them all into the attribute with the highest karma cost
                    if (intKnowledgeSkillPointCount > 0 && KnowledgeSkills.Any(x => x.Karma != 0))
                    {
                        int intHighestTotalKarmaCost = 0;
                        foreach (KnowledgeSkill objLoopKnowledgeSkill in KnowledgeSkills)
                        {
                            if (objLoopKnowledgeSkill.Karma == 0)
                                continue;
                            // Put points into the attribute with the highest total karma cost.
                            // In case of ties, pick the one that would need more points to cover it (the other one will hopefully get picked up at a later cycle)
                            int intLoopTotalKarmaCost = objLoopKnowledgeSkill.CurrentKarmaCost;
                            if (objKnowledgeSkillToPutPointsInto == null
                                || intLoopTotalKarmaCost > intHighestTotalKarmaCost
                                || (intLoopTotalKarmaCost == intHighestTotalKarmaCost
                                    && objLoopKnowledgeSkill.Karma > objKnowledgeSkillToPutPointsInto.Karma))
                            {
                                objKnowledgeSkillToPutPointsInto = objLoopKnowledgeSkill;
                                intHighestTotalKarmaCost = intLoopTotalKarmaCost;
                            }
                        }

                        if (objKnowledgeSkillToPutPointsInto != null)
                        {
                            objKnowledgeSkillToPutPointsInto.Base = intKnowledgeSkillPointCount;
                            objKnowledgeSkillToPutPointsInto.Karma -= intKnowledgeSkillPointCount;
                        }
                    }
                }

                //Timekeeper.Finish("load_char_skills");
            }
        }

        private void UpdateUndoList(XmlDocument xmlSkillOwnerDocument)
        {
            //Hacky way of converting Expense entries to guid based skill identification
            //specs already did?
            //First create dictionary mapping name=>guid
            ConcurrentDictionary<string, Guid> dicGroups = new ConcurrentDictionary<string, Guid>();
            ConcurrentDictionary<string, Guid> dicSkills = new ConcurrentDictionary<string, Guid>();
            // Potentially expensive checks that can (and therefore should) be parallelized. Normally, this would just be a Parallel.Invoke,
            // but we want to allow UI messages to happen, just in case this is called on the Main Thread and another thread wants to show a message box.
            // Not using async-await because this is trivial code and I do not want to infect everything that calls this with async as well.
            Utils.RunWithoutThreadLock(
                () =>
                {
                    Parallel.ForEach(SkillGroups, x =>
                    {
                        if (x.Rating > 0 && !dicGroups.ContainsKey(x.Name))
                            dicGroups.TryAdd(x.Name, x.Id);
                    });
                },
                () =>
                {
                    Parallel.ForEach(Skills, x =>
                    {
                        if (x.TotalBaseRating > 0)
                            dicSkills.TryAdd(x.Name, x.Id);
                    });
                },
                () =>
                {
                    Parallel.ForEach(KnowledgeSkills, x =>
                    {
                        dicSkills.TryAdd(x.Name, x.Id);
                    });
                });
            UpdateUndoSpecific(xmlSkillOwnerDocument, dicSkills, EnumerableExtensions.ToEnumerable(KarmaExpenseType.AddSkill, KarmaExpenseType.ImproveSkill));
            UpdateUndoSpecific(xmlSkillOwnerDocument, dicGroups, KarmaExpenseType.ImproveSkillGroup.Yield());
        }

        private static void UpdateUndoSpecific(XmlDocument doc, IDictionary<string, Guid> map, IEnumerable<KarmaExpenseType> typesRequreingConverting)
        {
            //Build a crazy xpath to get everything we want to convert

            string strXPath = "/character/expenses/expense[type = \'Karma\']/undo[" +
                              string.Join(" or ", typesRequreingConverting.Select(x => "karmatype = '" + x + "'")) +
                              "]/objectid";

            //Find everything
            XmlNodeList lstNodesToChange = doc.SelectNodes(strXPath);
            if (lstNodesToChange != null)
            {
                for (int i = 0; i < lstNodesToChange.Count; i++)
                {
                    lstNodesToChange[i].InnerText = map.TryGetValue(lstNodesToChange[i].InnerText, out Guid guidLoop)
                        ? guidLoop.ToString("D", GlobalSettings.InvariantCultureInfo)
                        : StringExtensions.EmptyGuid;
                }
            }
        }

        internal void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("newskills");

            objWriter.WriteElementString("skillptsmax", SkillPointsMaximum.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("skillgrpsmax", SkillGroupPointsMaximum.ToString(GlobalSettings.InvariantCultureInfo));

            objWriter.WriteStartElement("skills");
            List<Skill> lstSkillsOrdered = new List<Skill>(Skills);
            lstSkillsOrdered.Sort(CompareSkills);
            foreach (Skill objSkill in lstSkillsOrdered)
            {
                objSkill.WriteTo(objWriter);
            }
            objWriter.WriteEndElement();
            objWriter.WriteStartElement("knoskills");
            List<KnowledgeSkill> lstKnoSkillsOrdered = new List<KnowledgeSkill>(KnowledgeSkills);
            lstKnoSkillsOrdered.Sort(CompareSkills);
            foreach (KnowledgeSkill objKnowledgeSkill in lstKnoSkillsOrdered)
            {
                objKnowledgeSkill.WriteTo(objWriter);
            }
            objWriter.WriteEndElement();

            objWriter.WriteStartElement("skilljackknowledgeskills");
            lstKnoSkillsOrdered = new List<KnowledgeSkill>(KnowsoftSkills);
            lstKnoSkillsOrdered.Sort(CompareSkills);
            foreach (KnowledgeSkill objKnowledgeSkill in lstKnoSkillsOrdered)
            {
                objKnowledgeSkill.WriteTo(objWriter);
            }
            objWriter.WriteEndElement();

            objWriter.WriteStartElement("groups");
            List<SkillGroup> lstSkillGroups = new List<SkillGroup>(SkillGroups);
            lstSkillGroups.Sort(CompareSkillGroups);
            foreach (SkillGroup objSkillGroup in lstSkillGroups)
            {
                objSkillGroup.WriteTo(objWriter);
            }
            objWriter.WriteEndElement();
            objWriter.WriteEndElement();
        }

        internal void Reset()
        {
            foreach (Skill objSkill in _lstSkills)
                objSkill.UnbindSkill();
            _lstSkills.Clear();
            _dicSkills.Clear();
            foreach (KnowledgeSkill objKnowledgeSkill in KnowledgeSkills)
                objKnowledgeSkill.UnbindSkill();
            KnowledgeSkills.Clear();
            foreach (SkillGroup objGroup in SkillGroups)
                objGroup.UnbindSkillGroup();
            SkillGroups.Clear();
            SkillPointsMaximum = 0;
            SkillGroupPointsMaximum = 0;
            foreach (KnowledgeSkill objKnowledgeSkill in KnowsoftSkills)
                objKnowledgeSkill.UnbindSkill();
            KnowsoftSkills.Clear();
        }

        /// <summary>
        /// Maximum Skill Rating.
        /// </summary>
        public int MaxSkillRating { get; set; } = 0;

        private readonly CachedBindingList<Skill> _lstSkills = new CachedBindingList<Skill>();
        private readonly Dictionary<string, Skill> _dicSkills = new Dictionary<string, Skill>();

        /// <summary>
        /// Active Skills
        /// </summary>
        public CachedBindingList<Skill> Skills
        {
            get
            {
                if (_lstSkills.Count == 0)
                {
                    foreach (Skill objLoopSkill in GetSkillList(FilterOption.NonSpecial))
                    {
                        _lstSkills.Add(objLoopSkill);
                    }
                }
                return _lstSkills;
            }
        }

        /// <summary>
        /// Checks if the character has an active skill with a given name.
        /// </summary>
        /// <param name="strSkillKey">Name of the skill. For exotic skills, this is slightly different, refer to a Skill's DictionaryKey property for more info.</param>
        /// <returns></returns>
        public bool HasActiveSkill(string strSkillKey)
        {
            return _dicSkills.ContainsKey(strSkillKey);
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

        /// <summary>
        /// This is only used for reflection, so that all zero ratings skills are not uploaded
        /// </summary>
        [HubTag]
        public List<Skill> NotZeroRatingSkills
        {
            get
            {
                List<Skill> resultList = new List<Skill>(_lstSkills.Count);
                foreach (Skill objLoopSkill in _lstSkills)
                {
                    if (objLoopSkill.Rating > 0)
                        resultList.Add(objLoopSkill);
                }
                return resultList;
            }
        }

        public CachedBindingList<KnowledgeSkill> KnowledgeSkills { get; } = new CachedBindingList<KnowledgeSkill>();

        /// <summary>
        /// KnowsoftSkills.
        /// </summary>
        public List<KnowledgeSkill> KnowsoftSkills { get; } = new List<KnowledgeSkill>(1);

        /// <summary>
        /// Skill Groups.
        /// </summary>
        public BindingList<SkillGroup> SkillGroups { get; } = new BindingList<SkillGroup>();

        public bool HasKnowledgePoints => KnowledgeSkillPoints > 0;

        public bool HasAvailableNativeLanguageSlots => KnowledgeSkills.Count(x => x.IsNativeLanguage) < 1 + ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.NativeLanguageLimit);

        private int _intCachedKnowledgePoints = int.MinValue;

        /// <summary>
        /// Number of free Knowledge Skill Points the character has.
        /// </summary>
        public int KnowledgeSkillPoints
        {
            get
            {
                if (_intCachedKnowledgePoints == int.MinValue)
                {
                    string strExpression = _objCharacter.Settings.KnowledgePointsExpression;
                    if (strExpression.IndexOfAny('{', '+', '-', '*', ',') != -1 || strExpression.Contains("div"))
                    {
                        StringBuilder objValue = new StringBuilder(strExpression);
                        _objCharacter.AttributeSection.ProcessAttributesInXPath(objValue, strExpression);

                        // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                        object objProcess = CommonFunctions.EvaluateInvariantXPath(objValue.ToString(), out bool blnIsSuccess);
                        _intCachedKnowledgePoints = blnIsSuccess ? ((double)objProcess).StandardRound() : 0;
                    }
                    else
                        int.TryParse(strExpression, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out _intCachedKnowledgePoints);

                    _intCachedKnowledgePoints += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.FreeKnowledgeSkills).StandardRound();
                }

                return _intCachedKnowledgePoints;
            }
        }

        /// <summary>
        /// Number of free Knowledge skill points the character have remaining
        /// </summary>
        public int KnowledgeSkillPointsRemain => KnowledgeSkillPoints - KnowledgeSkillPointsUsed;

        /// <summary>
        /// Number of knowledge skill points the character have used.
        /// </summary>
        public int KnowledgeSkillPointsUsed => KnowledgeSkillRanksSum - SkillPointsSpentOnKnoskills;

        /// <summary>
        /// Sum of knowledge skill ranks the character has allocated.
        /// </summary>
        public int KnowledgeSkillRanksSum
        {
            get { return KnowledgeSkills.AsParallel().Sum(x => x.CurrentSpCost); }
        }

        /// <summary>
        /// Number of Skill Points that have been spent on knowledge skills.
        /// </summary>
        public int SkillPointsSpentOnKnoskills
        {
            get
            {
                //Even if it is stupid, you can spend real skill points on knoskills...
                if (!_objCharacter.EffectiveBuildMethodUsesPriorityTables)
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
            if (rhs == null && lhs == null)
                return 0;
            ExoticSkill lhsExoticSkill = lhs as ExoticSkill;
            if (rhs is ExoticSkill rhsExoticSkill)
            {
                return lhsExoticSkill != null
                    ? string.Compare(rhsExoticSkill.DisplaySpecific(GlobalSettings.Language),
                        lhsExoticSkill.DisplaySpecific(GlobalSettings.Language) ?? string.Empty, false,
                        GlobalSettings.CultureInfo)
                    : 1;
            }
            if (lhsExoticSkill != null)
                return -1;
            return string.Compare(rhs?.CurrentDisplayName ?? string.Empty, lhs?.CurrentDisplayName ?? string.Empty, false, GlobalSettings.CultureInfo);
        }

        public static int CompareSkillGroups(SkillGroup objXGroup, SkillGroup objYGroup)
        {
            if (objXGroup == null)
            {
                return objYGroup == null ? 0 : 1;
            }
            return objYGroup == null ? -1 : string.Compare(objXGroup.CurrentDisplayName, objYGroup.CurrentDisplayName, false, GlobalSettings.CultureInfo);
        }

        public IEnumerable<Skill> GetSkillList(FilterOption filter, string strName = "", bool blnFetchFromBackup = false)
        {
            //TODO less retarded way please
            // Load the Skills information.
            // Populate the Skills list.
            XmlDocument xmlSkillsDocument = _objCharacter.LoadData("skills.xml");
            using (XmlNodeList xmlSkillList = xmlSkillsDocument
                .SelectNodes(string.Format(GlobalSettings.InvariantCultureInfo, "/chummer/skills/skill[not(exotic) and ({0}){1}]",
                    _objCharacter.Settings.BookXPath(), SkillFilter(filter, strName))))
            {
                // First pass, build up a list of all of the Skills so we can sort them in alphabetical order for the current language.
                List<ListItem> lstSkillOrder = new List<ListItem>(xmlSkillList?.Count ?? 0);
                Dictionary<ListItem, Skill> dicSkills = new Dictionary<ListItem, Skill>(lstSkillOrder.Capacity);
                if (xmlSkillList?.Count > 0)
                {
                    foreach (XmlNode xmlSkill in xmlSkillList)
                    {
                        string strSkillName = xmlSkill["name"]?.InnerText ?? string.Empty;
                        ListItem objSkillItem = new ListItem(strSkillName, xmlSkill["translate"]?.InnerText ?? strSkillName);
                        lstSkillOrder.Add(objSkillItem);
                        //TODO: read from backup
                        if (blnFetchFromBackup && _dicSkillBackups.Count > 0 && xmlSkill.TryGetField("id", Guid.TryParse, out Guid guiSkillId))
                        {
                            if (_dicSkillBackups.TryGetValue(guiSkillId, out Skill objSkill) && objSkill != null)
                            {
                                dicSkills.Add(objSkillItem, objSkill);
                                _dicSkillBackups.Remove(guiSkillId);
                            }
                            else
                            {
                                bool blnIsKnowledgeSkill
                                    = xmlSkillsDocument
                                      .SelectSingleNode("/chummer/categories/category[. = "
                                                        + xmlSkill["category"]?.InnerText.CleanXPath() + "]/@type")
                                      ?.Value != "active";
                                dicSkills.Add(objSkillItem, Skill.FromData(xmlSkill, _objCharacter, blnIsKnowledgeSkill));
                            }
                        }
                        else
                        {
                            bool blnIsKnowledgeSkill
                                = xmlSkillsDocument
                                  .SelectSingleNode("/chummer/categories/category[. = "
                                                    + xmlSkill["category"]?.InnerText.CleanXPath() + "]/@type")?.Value
                                  != "active";
                            Skill objSkill = Skill.FromData(xmlSkill, _objCharacter, blnIsKnowledgeSkill);
                            dicSkills.Add(objSkillItem, objSkill);
                        }
                    }
                }

                lstSkillOrder.Sort(CompareListItems.CompareNames);

                // Second pass, retrieve the Skills in the order they're presented in the list.
                foreach (ListItem objItem in lstSkillOrder)
                {
                    yield return dicSkills[objItem];
                }
            }
        }

        private static string SkillFilter(FilterOption eFilter, string strName = "")
        {
            switch (eFilter)
            {
                case FilterOption.All:
                    return string.Empty;

                case FilterOption.NonSpecial:
                    return " and not(category = 'Magical Active') and not(category = 'Resonance Active')";

                case FilterOption.Magician:
                    return " and category = 'Magical Active'";

                case FilterOption.Sorcery:
                    return " and category = 'Magical Active' and (skillgroup = 'Sorcery' or skillgroup = '' or not(skillgroup))";

                case FilterOption.Conjuring:
                    return " and category = 'Magical Active' and (skillgroup = 'Conjuring' or skillgroup = '' or not(skillgroup))";

                case FilterOption.Enchanting:
                    return " and category = 'Magical Active' and (skillgroup = 'Enchanting' or skillgroup = '' or not(skillgroup))";

                case FilterOption.Adept:
                case FilterOption.Aware:
                case FilterOption.Explorer:
                    return " and category = 'Magical Active' and (skillgroup = '' or not(skillgroup))";

                case FilterOption.Spellcasting:
                    return " and category = 'Magical Active' and name = 'Spellcasting'";

                case FilterOption.Technomancer:
                    return " and category = 'Resonance Active'";

                case FilterOption.Name:
                    return " and name = '" + strName + "'";

                case FilterOption.XPath:
                    return " and (" + strName + ')';

                default:
                    throw new ArgumentOutOfRangeException(nameof(eFilter), eFilter, null);
            }
        }

        private IReadOnlyList<ListItem> _lstDefaultKnowledgeSkills;

        public IReadOnlyList<ListItem> MyDefaultKnowledgeSkills
        {
            get
            {
                if (GlobalSettings.LiveCustomData || _lstDefaultKnowledgeSkills == null)
                {
                    return _lstDefaultKnowledgeSkills = KnowledgeSkill.DefaultKnowledgeSkills(_objCharacter);
                }
                return _lstDefaultKnowledgeSkills;
            }
        }

        private IReadOnlyList<ListItem> _lstKnowledgeTypes;

        public IReadOnlyList<ListItem> MyKnowledgeTypes
        {
            get
            {
                if (GlobalSettings.LiveCustomData || _lstKnowledgeTypes == null)
                {
                    return _lstKnowledgeTypes = KnowledgeSkill.KnowledgeTypes(_objCharacter);
                }
                return _lstKnowledgeTypes;
            }
        }

        private static readonly DependencyGraph<string, SkillsSection> s_SkillSectionDependencyGraph =
            new DependencyGraph<string, SkillsSection>(
                new DependencyGraphNode<string, SkillsSection>(nameof(HasKnowledgePoints),
                    new DependencyGraphNode<string, SkillsSection>(nameof(KnowledgeSkillPoints))
                ),
                new DependencyGraphNode<string, SkillsSection>(nameof(SkillGroupPoints),
                    new DependencyGraphNode<string, SkillsSection>(nameof(SkillGroupPointsMaximum))
                ),
                new DependencyGraphNode<string, SkillsSection>(nameof(SkillPoints),
                    new DependencyGraphNode<string, SkillsSection>(nameof(SkillPointsMaximum)),
                    new DependencyGraphNode<string, SkillsSection>(nameof(SkillPointsSpentOnKnoskills))
                ),
                new DependencyGraphNode<string, SkillsSection>(nameof(KnowledgeSkillPointsRemain),
                    new DependencyGraphNode<string, SkillsSection>(nameof(KnowledgeSkillPoints)),
                    new DependencyGraphNode<string, SkillsSection>(nameof(KnowledgeSkillPointsUsed),
                        new DependencyGraphNode<string, SkillsSection>(nameof(KnowledgeSkillRanksSum)),
                        new DependencyGraphNode<string, SkillsSection>(nameof(SkillPointsSpentOnKnoskills),
                            new DependencyGraphNode<string, SkillsSection>(nameof(KnowledgeSkillPoints)),
                            new DependencyGraphNode<string, SkillsSection>(nameof(KnowledgeSkillRanksSum))
                        )
                    )
                )
            );

        public enum FilterOption
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
            XPath,
        }

        internal void ForceProperyChangedNotificationAll(string strName)
        {
            foreach (Skill objSkill in Skills)
            {
                objSkill.OnPropertyChanged(strName);
            }

            foreach (KnowledgeSkill objSkill in KnowledgeSkills)
            {
                objSkill.OnPropertyChanged(strName);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            foreach (Skill objSkill in Skills)
            {
                if ((GlobalSettings.PrintSkillsWithZeroRating || objSkill.Rating > 0) && objSkill.Enabled)
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
        #region XPath Processing
        /// <summary>
        /// Replaces substring in the form of {Skill} with the total dicepool of the skill.
        /// </summary>
        /// <param name="strInput">Stringbuilder object that contains the input.</param>
        /// <param name="dicValueOverrides">Alternative dictionary to use for value lookup instead of SkillsSection.GetActiveSkill.</param>
        public string ProcessSkillsInXPath(string strInput, IReadOnlyDictionary<string, int> dicValueOverrides = null)
        {
            return string.IsNullOrEmpty(strInput)
                ? strInput
                : ProcessSkillsInXPathForTooltip(strInput, blnShowValues: false, dicValueOverrides: dicValueOverrides);
        }

        /// <summary>
        /// Replaces stringbuilder content in the form of {Skill} with the total dicepool of the skill.
        /// </summary>
        /// <param name="sbdInput">Stringbuilder object that contains the input.</param>
        /// <param name="strOriginal">Original text that will be used in the final Stringbuilder. Replaces stringbuilder input without replacing the object.</param>
        /// <param name="dicValueOverrides">Alternative dictionary to use for value lookup instead of SkillsSection.GetActiveSkill.</param>
        public void ProcessSkillsInXPath(StringBuilder sbdInput, string strOriginal = "", IReadOnlyDictionary<string, int> dicValueOverrides = null)
        {
            if (sbdInput == null || sbdInput.Length <= 0)
                return;
            ProcessSkillsInXPathForTooltip(sbdInput, strOriginal, blnShowValues: false,
                dicValueOverrides: dicValueOverrides);
        }

        /// <summary>
        /// Replaces substring in the form of {Skill} with 'Skill (Pool)'. Intended to be used by tooltips and similar. 
        /// </summary>
        /// <param name="strInput">Stringbuilder object that contains the input.</param>
        /// <param name="objCultureInfo">Culture type used by the language. Defaults to null, which is then system defaults.</param>
        /// <param name="strLanguage">Language to use for displayname translation.</param>
        /// <param name="blnShowValues">Whether to include the dicepool value in the return string.</param>
        /// <param name="dicValueOverrides">Alternative dictionary to use for value lookup instead of SkillsSection.GetActiveSkill.</param>
        public string ProcessSkillsInXPathForTooltip(string strInput, CultureInfo objCultureInfo = null, string strLanguage = "", bool blnShowValues = true, IReadOnlyDictionary<string, int> dicValueOverrides = null)
        {
            if (string.IsNullOrEmpty(strInput))
                return strInput;
            if (objCultureInfo == null)
                objCultureInfo = GlobalSettings.CultureInfo;
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalSettings.Language;
            string strReturn = strInput;
            string strFormat = blnShowValues ? LanguageManager.GetString("String_Space", strLanguage) + "({0})" : string.Empty;
            foreach (string strSkillKey in Skills.Select(i => i.DictionaryKey))
            {
                if (blnShowValues)
                    strReturn = strReturn.CheapReplace('{' + strSkillKey + '}',
                                                       () =>
                                                       {
                                                           Skill objLoopSkill = GetActiveSkill(strSkillKey);
                                                           return objLoopSkill.DisplayName(strLanguage)
                                                                  + string.Format(
                                                                      objCultureInfo, strFormat,
                                                                      dicValueOverrides?.ContainsKey(strSkillKey)
                                                                      == true
                                                                          ? dicValueOverrides[strSkillKey]
                                                                          : objLoopSkill.PoolOtherAttribute(
                                                                              objLoopSkill.Attribute,
                                                                              intAttributeOverrideValue: 0)); // We explicitly want to override the attribute value with 0 because we're just fetching the pure skill pool
                                                       });
                else
                    strReturn = strReturn.CheapReplace('{' + strSkillKey + '}',
                                                       () => GetActiveSkill(strSkillKey).DisplayName(strLanguage));
            }
            return strReturn;
        }

        /// <summary>
        /// Replaces Stringbuilder content in the form of {Active Skill Name} with 'Active Skill Name (Pool)', ie {Athletics} becomes 'Athletics (1)'. Intended to be used by tooltips and similar. 
        /// </summary>
        /// <param name="sbdInput">Stringbuilder object that contains the input.</param>
        /// <param name="strOriginal">Original text that will be used in the final Stringbuilder. Replaces stringbuilder input without replacing the object.</param>
        /// <param name="objCultureInfo">Culture type used by the language. Defaults to null, which is then system defaults.</param>
        /// <param name="strLanguage">Language to use for displayname translation.</param>
        /// <param name="blnShowValues">Whether to include the dicepool value in the return string.</param>
        /// <param name="dicValueOverrides">Alternative dictionary to use for value lookup instead of SkillsSection.GetActiveSkill.</param>
        public void ProcessSkillsInXPathForTooltip(StringBuilder sbdInput, string strOriginal = "", CultureInfo objCultureInfo = null, string strLanguage = "", bool blnShowValues = true, IReadOnlyDictionary<string, int> dicValueOverrides = null)
        {
            if (sbdInput == null || sbdInput.Length <= 0)
                return;
            if (string.IsNullOrEmpty(strOriginal))
                strOriginal = sbdInput.ToString();
            if (objCultureInfo == null)
                objCultureInfo = GlobalSettings.CultureInfo;
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalSettings.Language;
            string strFormat = blnShowValues ? LanguageManager.GetString("String_Space", strLanguage) + "({0})" : string.Empty;
            foreach (string strSkillKey in Skills.Select(i => i.DictionaryKey))
            {
                if (blnShowValues)
                    sbdInput.CheapReplace(strOriginal, '{' + strSkillKey + '}',
                                          () =>
                                          {
                                              Skill objLoopSkill = GetActiveSkill(strSkillKey);
                                              return objLoopSkill.DisplayName(strLanguage)
                                                     + string.Format(
                                                         objCultureInfo, strFormat,
                                                         dicValueOverrides?.ContainsKey(strSkillKey)
                                                         == true
                                                             ? dicValueOverrides[strSkillKey]
                                                             : objLoopSkill.PoolOtherAttribute(
                                                                 objLoopSkill.Attribute,
                                                                 intAttributeOverrideValue: 0)); // We explicitly want to override the attribute value with 0 because we're just fetching the pure skill pool
                                          });
                else
                    sbdInput.CheapReplace(strOriginal, '{' + strSkillKey + '}',
                                          () => GetActiveSkill(strSkillKey).DisplayName(strLanguage));
            }
        }
        #endregion
    }
}
