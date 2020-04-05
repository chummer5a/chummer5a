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
using System.Xml;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;
using Chummer.Annotations;
using Chummer.Backend.Attributes;

namespace Chummer.Backend.Skills
{
    public class SkillsSection : INotifyMultiplePropertyChanged
    {
        private readonly Character _objCharacter;
        private readonly Dictionary<Guid, Skill> _dicSkillBackups = new Dictionary<Guid, Skill>();

        public SkillsSection(Character character)
        {
            _objCharacter = character;
            _objCharacter.LOG.PropertyChanged += UpdateKnowledgePointsFromAttributes;
            _objCharacter.INT.PropertyChanged += UpdateKnowledgePointsFromAttributes;

        }

        public void UnbindSkillsSection()
        {
            _objCharacter.LOG.PropertyChanged -= UpdateKnowledgePointsFromAttributes;
            _objCharacter.INT.PropertyChanged -= UpdateKnowledgePointsFromAttributes;
            _dicSkillBackups.Clear();
        }

        [NotifyPropertyChangedInvocator]
        public void OnPropertyChanged([CallerMemberName] string strPropertyName = null)
        {
            OnMultiplePropertyChanged(strPropertyName);
        }

        public void OnMultiplePropertyChanged(params string[] lstPropertyNames)
        {
            ICollection<string> lstNamesOfChangedProperties = null;
            foreach (string strPropertyName in lstPropertyNames)
            {
                if (lstNamesOfChangedProperties == null)
                    lstNamesOfChangedProperties = SkillSectionDependancyGraph.GetWithAllDependants(strPropertyName);
                else
                {
                    foreach (string strLoopChangedProperty in SkillSectionDependancyGraph.GetWithAllDependants(strPropertyName))
                        lstNamesOfChangedProperties.Add(strLoopChangedProperty);
                }
            }

            if ((lstNamesOfChangedProperties?.Count > 0) != true)
                return;

            foreach (string strPropertyToChange in lstNamesOfChangedProperties)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strPropertyToChange));
            }
        }

        internal void AddSkills(FilterOptions skills, string strName = "")
        {
            List<Skill> lstExistingSkills = GetSkillList(skills, strName, true).ToList();

            Skills.MergeInto(lstExistingSkills, CompareSkills, (objExistSkill, objNewSkill) =>
            {
                if (objNewSkill.Base > objExistSkill.Base)
                    objExistSkill.Base = objNewSkill.Base;
                if (objNewSkill.Karma > objExistSkill.Karma)
                    objExistSkill.Karma = objNewSkill.Karma;
                objExistSkill.Specializations.MergeInto(objNewSkill.Specializations, (x, y) => x.Free == y.Free ? string.Compare(x.DisplayName(GlobalOptions.Language), y.DisplayName(GlobalOptions.Language), StringComparison.Ordinal) : (x.Free ? 1 : -1));
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
                    _dicSkillBackups.Add(skill.SkillId, skill);
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
                        KnowledgeSkills.MergeInto(kno, (x, y) => string.Compare(x.Type, y.Type, StringComparison.Ordinal) == 0 ? CompareSkills(x, y) : (string.Compare(x.Type, y.Type, StringComparison.Ordinal) == -1 ? -1 : 1), (objExistSkill, objNewSkill) =>
                        {
                            if (objNewSkill.Base > objExistSkill.Base)
                                objExistSkill.Base = objNewSkill.Base;
                            if (objNewSkill.Karma > objExistSkill.Karma)
                                objExistSkill.Karma = objNewSkill.Karma;
                            objExistSkill.Specializations.MergeInto(objNewSkill.Specializations, (x, y) => x.Free == y.Free ? string.Compare(x.DisplayName(GlobalOptions.Language), y.DisplayName(GlobalOptions.Language), StringComparison.Ordinal) : (x.Free ? 1 : -1));
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

        internal async void Load(XmlNode xmlSkillNode, bool blnLegacy, CustomActivity parentActivity)
        {
            if (xmlSkillNode == null)
                return;
            using (var op_load_char_skills = Timekeeper.StartSyncron("load_char_skills_skillnode", parentActivity))
            {

                if (!blnLegacy)
                {
                    using (var op_load_char_skills_groups = Timekeeper.StartSyncron("load_char_skills_groups", op_load_char_skills))
                    {
                        List<SkillGroup> lstLoadingSkillGroups = new List<SkillGroup>();
                        using (XmlNodeList xmlGroupsList = xmlSkillNode.SelectNodes("groups/group"))
                            if (xmlGroupsList != null)
                                foreach (XmlNode xmlNode in xmlGroupsList)
                                {
                                    SkillGroup objGroup = new SkillGroup(_objCharacter);
                                    objGroup.Load(xmlNode);
                                    lstLoadingSkillGroups.Add(objGroup);
                                }

                        lstLoadingSkillGroups.Sort((i1, i2) =>
                            string.Compare(i2.DisplayName, i1.DisplayName, StringComparison.Ordinal));
                        foreach (SkillGroup skillgroup in lstLoadingSkillGroups)
                        {
                            SkillGroups.Add(skillgroup);
                        }

                        //Timekeeper.Finish("load_char_skills_groups");
                    }

                    using (var op_load_char_skills_groups = Timekeeper.StartSyncron("load_char_skills_normal", op_load_char_skills))
                    {
                        //Load skills. Because sorting a BindingList is complicated we use a temporery normal list
                        List<Skill> lstLoadingSkills = new List<Skill>();
                        using (XmlNodeList xmlSkillsList = xmlSkillNode.SelectNodes("skills/skill"))
                            if (xmlSkillsList != null)
                                foreach (XmlNode xmlNode in xmlSkillsList)
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
                                objExistSkill.Specializations.MergeInto(objNewSkill.Specializations,
                                    (x, y) => x.Free == y.Free
                                        ? string.Compare(x.DisplayName(GlobalOptions.Language),
                                            y.DisplayName(GlobalOptions.Language), StringComparison.Ordinal)
                                        : (x.Free ? 1 : -1));
                            });
                            if (blnDoAddToDictionary)
                                _dicSkills.Add(strName, objSkill);
                        }

                        // TODO: Skill groups don't refresh their CanIncrease property correctly when the last of their skills is being added, as the total basse rating will be zero. Call this here to force a refresh.
                        foreach (SkillGroup g in SkillGroups)
                        {
                            g.OnPropertyChanged(nameof(SkillGroup.SkillList));
                        }

                        //Timekeeper.Finish("load_char_skills_normal");
                    }

                    using (var op_load_char_skills_kno = Timekeeper.StartSyncron("load_char_skills_kno", op_load_char_skills))
                    {
                        using (XmlNodeList xmlSkillsList = xmlSkillNode.SelectNodes("knoskills/skill"))
                            if (xmlSkillsList != null)
                                foreach (XmlNode xmlNode in xmlSkillsList)
                                {
                                    if (Skill.Load(_objCharacter, xmlNode) is KnowledgeSkill objSkill)
                                        KnowledgeSkills.Add(objSkill);
                                }

                        //Timekeeper.Finish("load_char_skills_kno");
                    }

                    using (var op_load_char_skills_kno = Timekeeper.StartSyncron("load_char_knowsoft_buffer", op_load_char_skills))
                    {
                        // Knowsoft Buffer.
                        using (XmlNodeList xmlSkillsList = xmlSkillNode.SelectNodes("skilljackknowledgeskills/skill"))
                            if (xmlSkillsList != null)
                                foreach (XmlNode xmlNode in xmlSkillsList)
                                {
                                    string strName = string.Empty;
                                    if (xmlNode.TryGetStringFieldQuickly("name", ref strName))
                                        KnowsoftSkills.Add(new KnowledgeSkill(_objCharacter, strName, false));
                                }

                        //Timekeeper.Finish("load_char_knowsoft_buffer");
                    }
                }
                else
                {
                    List<Skill> lstTempSkillList = new List<Skill>();
                    using (XmlNodeList xmlSkillsList = xmlSkillNode.SelectNodes("skills/skill"))
                        if (xmlSkillsList != null)
                            foreach (XmlNode xmlNode in xmlSkillsList)
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
                            _dicSkills.Add(
                                objSkill.IsExoticSkill
                                    ? objSkill.Name + " (" +
                                      objSkill.DisplaySpecializationMethod(GlobalOptions.DefaultLanguage) + ')'
                                    : objSkill.Name, objSkill);
                        }

                        UpdateUndoList(xmlSkillNode);
                    }
                }

                HashSet<string> hashSkillGuids = new HashSet<string>();
                XmlDocument skillsDoc = XmlManager.Load("skills.xml", _objCharacter.Options.CustomDataDictionary);
                foreach (XmlNode node in skillsDoc.SelectNodes(
                    $"/chummer/skills/skill[not(exotic) and ({_objCharacter.Options.BookXPath()}) {SkillFilter(FilterOptions.NonSpecial)}]")
                )
                {
                    hashSkillGuids.Add(node["name"].InnerText);
                }

                foreach (string skillId in hashSkillGuids.Where(s => Skills.All(skill => skill.Name != s)))
                {
                    XmlNode objXmlSkillNode = skillsDoc.SelectSingleNode($"/chummer/skills/skill[name = \"{skillId}\"]");
                    if (objXmlSkillNode != null)
                    {
                        Skill objSkill = Skill.FromData(objXmlSkillNode, _objCharacter);
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

        internal async void LoadFromHeroLab(XmlNode xmlSkillNode, CustomActivity parentActivity)
        {
            using (var op_load_char_skills_groups = Timekeeper.StartSyncron("load_char_skills_groups", parentActivity))
            {
                List<SkillGroup> lstLoadingSkillGroups = new List<SkillGroup>();
                using (XmlNodeList xmlGroupsList = xmlSkillNode.SelectNodes("groups/skill"))
                    if (xmlGroupsList != null)
                        foreach (XmlNode xmlNode in xmlGroupsList)
                        {
                            SkillGroup objGroup = new SkillGroup(_objCharacter);
                            objGroup.LoadFromHeroLab(xmlNode);
                            lstLoadingSkillGroups.Add(objGroup);
                        }

                lstLoadingSkillGroups.Sort((i1, i2) =>
                    string.Compare(i2.DisplayName, i1.DisplayName, StringComparison.Ordinal));
                foreach (SkillGroup skillgroup in lstLoadingSkillGroups)
                {
                    SkillGroups.Add(skillgroup);
                }

                //Timekeeper.Finish("load_char_skills_groups");
            }

            using (var op_load_char_skills = Timekeeper.StartSyncron("load_char_skills", parentActivity))
            {

                List<Skill> lstTempSkillList = new List<Skill>();
                using (XmlNodeList xmlSkillsList = xmlSkillNode.SelectNodes("active/skill"))
                    if (xmlSkillsList?.Count > 0)
                        foreach (XmlNode xmlNode in xmlSkillsList)
                        {
                            Skill objSkill = Skill.LoadFromHeroLab(_objCharacter, xmlNode, false);
                            if (objSkill != null)
                                lstTempSkillList.Add(objSkill);
                        }

                using (XmlNodeList xmlSkillsList = xmlSkillNode.SelectNodes("knowledge/skill"))
                    if (xmlSkillsList?.Count > 0)
                        foreach (XmlNode xmlNode in xmlSkillsList)
                        {
                            Skill objSkill = Skill.LoadFromHeroLab(_objCharacter, xmlNode, true);
                            if (objSkill != null)
                                lstTempSkillList.Add(objSkill);
                        }

                using (XmlNodeList xmlSkillsList = xmlSkillNode.SelectNodes("language/skill"))
                    if (xmlSkillsList?.Count > 0)
                        foreach (XmlNode xmlNode in xmlSkillsList)
                        {
                            Skill objSkill = Skill.LoadFromHeroLab(_objCharacter, xmlNode, true, "Language");
                            if (objSkill != null)
                                lstTempSkillList.Add(objSkill);
                        }

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
                        objExistSkill.Specializations.MergeInto(objNewSkill.Specializations,
                            (x, y) => x.Free == y.Free
                                ? string.Compare(x.DisplayName(GlobalOptions.Language),
                                    y.DisplayName(GlobalOptions.Language), StringComparison.Ordinal)
                                : (x.Free ? 1 : -1));
                    });
                    if (blnDoAddToDictionary)
                        _dicSkills.Add(strName, objSkill);
                }

                UpdateUndoList(xmlSkillNode);

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

                    if (_objCharacter.BuildMethodHasSkillPoints)
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
                                if (objSkillToPutPointsInto == null || (objLoopSkill.Karma <= intSkillPointCount &&
                                                                        (intLoopTotalKarmaCost >
                                                                         intSkillToPutPointsIntoTotalKarmaCost ||
                                                                         (intLoopTotalKarmaCost ==
                                                                          intSkillToPutPointsIntoTotalKarmaCost &&
                                                                          objLoopSkill.Karma >
                                                                          objSkillToPutPointsInto.Karma))))
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
                                if (objSkillToPutPointsInto == null ||
                                    intLoopTotalKarmaCost > intHighestTotalKarmaCost ||
                                    (intLoopTotalKarmaCost == intHighestTotalKarmaCost &&
                                     objLoopSkill.Karma > objSkillToPutPointsInto.Karma))
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
                            if (objKnowledgeSkillToPutPointsInto == null ||
                                (objLoopKnowledgeSkill.Karma <= intKnowledgeSkillPointCount &&
                                 (intLoopTotalKarmaCost > intKnowledgeSkillToPutPointsIntoTotalKarmaCost ||
                                  (intLoopTotalKarmaCost == intKnowledgeSkillToPutPointsIntoTotalKarmaCost &&
                                   objLoopKnowledgeSkill.Karma > objKnowledgeSkillToPutPointsInto.Karma))))
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
                            if (objKnowledgeSkillToPutPointsInto == null ||
                                intLoopTotalKarmaCost > intHighestTotalKarmaCost ||
                                (intLoopTotalKarmaCost == intHighestTotalKarmaCost && objLoopKnowledgeSkill.Karma >
                                 objKnowledgeSkillToPutPointsInto.Karma))
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
                    foreach (KnowledgeSkill objLoopSkill in KnowledgeSkills)
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
                    lstNodesToChange[i].InnerText = map.TryGetValue(lstNodesToChange[i].InnerText, out Guid guidLoop) ? guidLoop.ToString("D") : StringExtensions.EmptyGuid;
                }
            }
        }

        internal void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("newskills");

            objWriter.WriteElementString("skillptsmax", SkillPointsMaximum.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("skillgrpsmax", SkillGroupPointsMaximum.ToString(GlobalOptions.InvariantCultureInfo));

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
                    foreach (Skill objLoopSkill in GetSkillList(FilterOptions.NonSpecial))
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
        public IDictionary<string, Skill> SkillsDictionary => _dicSkills;

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
                List<Skill> resultList = new List<Skill>();
                foreach (Skill objLoopSkill in _lstSkills)
                {
                    if (objLoopSkill.Rating > 0)
                        resultList.Add(objLoopSkill);
                }
                return resultList;
            }
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
                int fromAttributes = _objCharacter.Options.FreeKnowledgeMultiplier;
                // Calculate Free Knowledge Skill Points. Free points = (INT + LOG) * 2.
                if (_objCharacter.Options.UseTotalValueForFreeKnowledge)
                {
                    fromAttributes *= (_objCharacter.INT.TotalValue + _objCharacter.LOG.TotalValue);
                }
                else
                {
                    fromAttributes *= (_objCharacter.INT.Value + _objCharacter.LOG.Value) ;
                }

                int val = ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.FreeKnowledgeSkills);
                return fromAttributes + val;
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
                if (!_objCharacter.BuildMethodHasSkillPoints)
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
            ExoticSkill lhsExoticSkill = lhs as ExoticSkill;
            if (rhs is ExoticSkill rhsExoticSkill)
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

        public static int CompareSkillGroups(SkillGroup objXGroup, SkillGroup objYGroup)
        {
            if (objXGroup == null)
            {
                if (objYGroup == null)
                    return 0;
                return -1;
            }
            return objYGroup == null ? 1 : string.Compare(objXGroup.DisplayName, objYGroup.DisplayName, StringComparison.Ordinal);
        }

        public IEnumerable<Skill> GetSkillList(FilterOptions filter, string strName = "", bool blnFetchFromBackup = false)
        {
            //TODO less retarded way please
            // Load the Skills information.
            // Populate the Skills list.
            using (XmlNodeList xmlSkillList = XmlManager.Load("skills.xml", _objCharacter.Options.CustomDataDictionary).SelectNodes("/chummer/skills/skill[not(exotic) and (" + _objCharacter.Options.BookXPath() + ')' + SkillFilter(filter, strName) + "]"))
            {
                // First pass, build up a list of all of the Skills so we can sort them in alphabetical order for the current language.
                Dictionary<string, Skill> dicSkills = new Dictionary<string, Skill>(xmlSkillList?.Count ?? 0);
                List<ListItem> lstSkillOrder = new List<ListItem>();
                if (xmlSkillList != null)
                {
                    foreach (XmlNode xmlSkill in xmlSkillList)
                    {
                        string strSkillName = xmlSkill["name"]?.InnerText ?? string.Empty;
                        lstSkillOrder.Add(new ListItem(strSkillName, xmlSkill["translate"]?.InnerText ?? strSkillName));
                        //TODO: read from backup
                        if (blnFetchFromBackup && _dicSkillBackups.Count > 0 && xmlSkill.TryGetField("id", Guid.TryParse, out Guid guiSkillId))
                        {
                            if (_dicSkillBackups.TryGetValue(guiSkillId, out Skill objSkill) && objSkill != null)
                            {
                                dicSkills.Add(objSkill.Name, objSkill);
                                _dicSkillBackups.Remove(guiSkillId);
                            }
                            else
                            {
                                dicSkills.Add(strSkillName, Skill.FromData(xmlSkill, _objCharacter));
                            }
                        }
                        else
                        {
                            Skill objSkill = Skill.FromData(xmlSkill, _objCharacter);
                            dicSkills.Add(strSkillName, objSkill);
                        }
                    }
                }

                lstSkillOrder.Sort(CompareListItems.CompareNames);

                // Second pass, retrieve the Skills in the order they're presented in the list.
                foreach (ListItem objItem in lstSkillOrder)
                {
                    yield return dicSkills[objItem.Value.ToString()];
                }
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
                case FilterOptions.XPath:
                    return $" and ({strName})";
                default:
                    throw new ArgumentOutOfRangeException(nameof(eFilter), eFilter, null);
            }
        }

        private static readonly DependancyGraph<string> SkillSectionDependancyGraph =
            new DependancyGraph<string>(
                new DependancyGraphNode<string>(nameof(HasKnowledgePoints),
                    new DependancyGraphNode<string>(nameof(KnowledgeSkillPoints))
                ),
                new DependancyGraphNode<string>(nameof(KnowledgeSkillPointsRemain),
                    new DependancyGraphNode<string>(nameof(KnowledgeSkillPoints)),
                    new DependancyGraphNode<string>(nameof(KnowledgeSkillPointsUsed),
                        new DependancyGraphNode<string>(nameof(KnowledgeSkillRanksSum)),
                        new DependancyGraphNode<string>(nameof(SkillPointsSpentOnKnoskills),
                            new DependancyGraphNode<string>(nameof(KnowledgeSkillPoints)),
                            new DependancyGraphNode<string>(nameof(KnowledgeSkillRanksSum))
                        )
                    )
                )
            );

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

        private void UpdateKnowledgePointsFromAttributes(object sender, PropertyChangedEventArgs e)
        {
            if ((_objCharacter.Options.UseTotalValueForFreeKnowledge && e.PropertyName == nameof(CharacterAttrib.TotalValue)) ||
                 (!_objCharacter.Options.UseTotalValueForFreeKnowledge && e.PropertyName == nameof(CharacterAttrib.Value)))
            {
                OnPropertyChanged(nameof(KnowledgeSkillPoints));
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
