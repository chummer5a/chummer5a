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
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using Chummer.Annotations;

namespace Chummer.Backend.Skills
{
    public sealed class SkillsSection : INotifyMultiplePropertyChanged, IDisposable
    {
        private readonly Character _objCharacter;
        private readonly Dictionary<Guid, Skill> _dicSkillBackups = new Dictionary<Guid, Skill>();

        public SkillsSection(Character character)
        {
            _objCharacter = character ?? throw new ArgumentNullException(nameof(character));
            _objCharacter.PropertyChanged += OnCharacterPropertyChanged;
            _objCharacter.Settings.PropertyChanged += OnCharacterSettingsPropertyChanged;
            SkillGroups.BeforeRemove += SkillGroupsOnBeforeRemove;
            KnowsoftSkills.BeforeRemove += KnowsoftSkillsOnBeforeRemove;
            KnowledgeSkills.BeforeRemove += KnowledgeSkillsOnBeforeRemove;
            KnowledgeSkills.ListChanged += KnowledgeSkillsOnListChanged;
            Skills.BeforeRemove += SkillsOnBeforeRemove;
            Skills.ListChanged += SkillsOnListChanged;
        }

        private void SkillGroupsOnBeforeRemove(object sender, RemovingOldEventArgs e)
        {
            using (new EnterWriteLock(_objCharacter.LockObject))
                SkillGroups[e.OldIndex].UnbindSkillGroup();
        }

        private void SkillsOnBeforeRemove(object sender, RemovingOldEventArgs e)
        {
            using (new EnterUpgradeableReadLock(_objCharacter.LockObject))
            {
                Skill objSkill = Skills[e.OldIndex];
                using (new EnterWriteLock(_objCharacter.LockObject))
                {
                    _dicSkills.Remove(objSkill.DictionaryKey);
                    if (!_dicSkillBackups.ContainsValue(objSkill))
                        objSkill.Dispose();
                }
            }
        }

        private void KnowledgeSkillsOnBeforeRemove(object sender, RemovingOldEventArgs e)
        {
            using (new EnterUpgradeableReadLock(_objCharacter.LockObject))
            {
                KnowledgeSkill objSkill = KnowledgeSkills[e.OldIndex];
                using (new EnterWriteLock(_objCharacter.LockObject))
                {
                    objSkill.PropertyChanged -= OnKnowledgeSkillPropertyChanged;
                    if (!_dicSkillBackups.ContainsValue(objSkill) && !KnowsoftSkills.Contains(objSkill))
                        objSkill.Dispose();
                }
            }
        }

        private void KnowsoftSkillsOnBeforeRemove(object sender, RemovingOldEventArgs e)
        {
            using (new EnterUpgradeableReadLock(_objCharacter.LockObject))
            {
                KnowledgeSkill objSkill = KnowledgeSkills[e.OldIndex];
                if (!_dicSkillBackups.ContainsValue(objSkill) && !KnowledgeSkills.Contains(objSkill))
                {
                    using (new EnterWriteLock(_objCharacter.LockObject))
                        objSkill.Dispose();
                }
            }
        }

        private void SkillsOnListChanged(object sender, ListChangedEventArgs e)
        {
            using (new EnterWriteLock(_objCharacter.LockObject))
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
        }

        private void KnowledgeSkillsOnListChanged(object sender, ListChangedEventArgs e)
        {
            switch (e.ListChangedType)
            {
                case ListChangedType.Reset:
                    using (new EnterWriteLock(_objCharacter.LockObject))
                    {
                        foreach (KnowledgeSkill objKnoSkill in KnowledgeSkills)
                            objKnoSkill.PropertyChanged += OnKnowledgeSkillPropertyChanged;
                    }
                    goto case ListChangedType.ItemDeleted;
                case ListChangedType.ItemAdded:
                    using (new EnterWriteLock(_objCharacter.LockObject))
                        KnowledgeSkills[e.NewIndex].PropertyChanged += OnKnowledgeSkillPropertyChanged;
                    goto case ListChangedType.ItemDeleted;
                case ListChangedType.ItemDeleted:
                    this.OnMultiplePropertyChanged(nameof(KnowledgeSkillRanksSum), nameof(HasAvailableNativeLanguageSlots));
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
            _objCharacter.PropertyChanged -= OnCharacterPropertyChanged;
            _objCharacter.Settings.PropertyChanged -= OnCharacterSettingsPropertyChanged;
            _dicSkillBackups.Clear();
        }

        private void OnCharacterPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e?.PropertyName == nameof(Character.EffectiveBuildMethodUsesPriorityTables))
                OnPropertyChanged(nameof(SkillPointsSpentOnKnoskills));
        }

        private void OnCharacterSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e?.PropertyName)
            {
                case nameof(CharacterSettings.KnowledgePointsExpression):
                    OnPropertyChanged(nameof(KnowledgeSkillPoints));
                    break;
                case nameof(CharacterSettings.MaxSkillRatingCreate):
                {
                    using (new EnterUpgradeableReadLock(_objCharacter.LockObject))
                    {
                        if (!_objCharacter.Created && !_objCharacter.IgnoreRules)
                        {
                            foreach (Skill objSkill in Skills)
                                objSkill.OnPropertyChanged(nameof(Skill.RatingMaximum));
                            foreach (Skill objSkill in _dicSkillBackups.Values.Where(x => !x.IsKnowledgeSkill))
                                objSkill.OnPropertyChanged(nameof(Skill.RatingMaximum));
                        }
                    }

                    break;
                }
                case nameof(CharacterSettings.MaxKnowledgeSkillRatingCreate):
                {
                    using (new EnterUpgradeableReadLock(_objCharacter.LockObject))
                    {
                        if (!_objCharacter.Created && !_objCharacter.IgnoreRules)
                        {
                            foreach (KnowledgeSkill objSkill in KnowledgeSkills)
                                objSkill.OnPropertyChanged(nameof(Skill.RatingMaximum));
                            foreach (Skill objSkill in _dicSkillBackups.Values.Where(x => x.IsKnowledgeSkill))
                                objSkill.OnPropertyChanged(nameof(Skill.RatingMaximum));
                        }
                    }

                    break;
                }
            }
        }

        [NotifyPropertyChangedInvocator]
        public void OnPropertyChanged([CallerMemberName] string strPropertyName = null)
        {
            this.OnMultiplePropertyChanged(strPropertyName);
        }

        public void OnMultiplePropertyChanged(IReadOnlyCollection<string> lstPropertyNames)
        {
            HashSet<string> setNamesOfChangedProperties = null;
            try
            {
                foreach (string strPropertyName in lstPropertyNames)
                {
                    if (setNamesOfChangedProperties == null)
                        setNamesOfChangedProperties
                            = s_SkillSectionDependencyGraph.GetWithAllDependents(this, strPropertyName, true);
                    else
                    {
                        foreach (string strLoopChangedProperty in s_SkillSectionDependencyGraph
                                     .GetWithAllDependentsEnumerable(this, strPropertyName))
                            setNamesOfChangedProperties.Add(strLoopChangedProperty);
                    }
                }

                if (setNamesOfChangedProperties == null || setNamesOfChangedProperties.Count == 0)
                    return;
                if (setNamesOfChangedProperties.Contains(nameof(KnowledgeSkillPoints)))
                    _intCachedKnowledgePoints = int.MinValue;

                foreach (string strPropertyToChange in setNamesOfChangedProperties)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strPropertyToChange));
                }
            }
            finally
            {
                if (setNamesOfChangedProperties != null)
                    Utils.StringHashSetPool.Return(setNamesOfChangedProperties);
            }
        }

        internal IEnumerable<Skill> GetActiveSkillsFromData(FilterOption eFilterOption, bool blnDeleteSkillsFromBackupIfFound = false, string strName = "")
        {
            using (new EnterUpgradeableReadLock(_objCharacter.LockObject))
            {
                XmlDocument xmlSkillsDocument = _objCharacter.LoadData("skills.xml");
                using (XmlNodeList xmlSkillList = xmlSkillsDocument
                           .SelectNodes("/chummer/skills/skill[not(exotic) and (" + _objCharacter.Settings.BookXPath()
                                        + ')'
                                        + SkillFilter(eFilterOption, strName) + ']'))
                {
                    if (xmlSkillList?.Count > 0)
                    {
                        foreach (XmlNode xmlSkill in xmlSkillList)
                        {
                            if (_dicSkillBackups.Count > 0
                                && xmlSkill.TryGetField("id", Guid.TryParse, out Guid guiSkillId)
                                && _dicSkillBackups.TryGetValue(guiSkillId, out Skill objSkill)
                                && objSkill != null)
                            {
                                if (blnDeleteSkillsFromBackupIfFound)
                                    _dicSkillBackups.Remove(guiSkillId);
                                yield return objSkill;
                            }
                            else
                            {
                                bool blnIsKnowledgeSkill
                                    = xmlSkillsDocument
                                      .SelectSingleNode("/chummer/categories/category[. = "
                                                        + xmlSkill["category"]?.InnerText.CleanXPath() + "]/@type")
                                      ?.Value
                                      != "active";
                                yield return Skill.FromData(xmlSkill, _objCharacter, blnIsKnowledgeSkill);
                            }
                        }
                    }
                }
            }
        }

        internal void AddSkills(FilterOption eFilterOption, string strName = "")
        {
            List<Skill> lstSkillsToAdd = GetActiveSkillsFromData(eFilterOption, true, strName).ToList();
            using (new EnterWriteLock(_objCharacter.LockObject))
            {
                foreach (Skill objSkill in lstSkillsToAdd)
                {
                    Guid guidLoop = objSkill.SkillId;
                    if (guidLoop != Guid.Empty && !objSkill.IsExoticSkill)
                    {
                        Skill objExistingSkill = Skills.FirstOrDefault(x => x.SkillId == guidLoop);
                        if (objExistingSkill != null)
                        {
                            MergeSkills(objExistingSkill, objSkill);
                            continue;
                        }
                    }

                    Skills.AddWithSort(objSkill, CompareSkills, MergeSkills);
                }
            }
        }

        internal ExoticSkill AddExoticSkill(string strName, string strSpecific)
        {
            using (new EnterUpgradeableReadLock(_objCharacter.LockObject))
            {
                XmlNode xmlSkillNode = _objCharacter.LoadData("skills.xml")
                                                    .SelectSingleNode(
                                                        "/chummer/skills/skill[name = " + strName.CleanXPath() + ']');
                using (new EnterWriteLock(_objCharacter.LockObject))
                {
                    ExoticSkill objExoticSkill = new ExoticSkill(_objCharacter, xmlSkillNode)
                    {
                        Specific = strSpecific
                    };
                    Skills.AddWithSort(objExoticSkill, CompareSkills, MergeSkills);
                    return objExoticSkill;
                }
            }
        }

        internal void RemoveSkills(FilterOption eSkillsToRemove, string strName = "", bool blnCreateKnowledge = true)
        {
            using (new EnterUpgradeableReadLock(_objCharacter.LockObject))
            {
                HashSet<Skill> setSkillsToRemove
                    = new HashSet<Skill>(GetActiveSkillsFromData(eSkillsToRemove, false, strName));
                // Check for duplicates (we'd normally want to make sure the improvement is enabled, but disabled SpecialSkills just force-disables a skill, so we need to keep those)
                foreach (Improvement objImprovement in _objCharacter.Improvements.Where(
                             x => x.ImproveType == Improvement.ImprovementType.SpecialSkills))
                {
                    FilterOption eFilterOption
                        = (FilterOption) Enum.Parse(typeof(FilterOption), objImprovement.ImprovedName);
                    setSkillsToRemove.ExceptWith(GetActiveSkillsFromData(eFilterOption, false, objImprovement.Target));
                }

                if (setSkillsToRemove.Count == 0)
                    return;

                Lazy<string> strKnowledgeSkillTypeToUse = null;
                if (blnCreateKnowledge)
                {
                    strKnowledgeSkillTypeToUse = new Lazy<string>(() =>
                    {
                        XPathNavigator xmlCategories = _objCharacter.LoadDataXPath("skills.xml")
                                                                    .SelectSingleNodeAndCacheExpression(
                                                                        "/chummer/categories");
                        if (xmlCategories.SelectSingleNode("category[@type = \"knowledge\" and . = \"Professional\"]")
                            != null)
                            return "Professional";
                        return xmlCategories.SelectSingleNode("category[@type = \"knowledge\"]")?.Value
                               ?? "Professional";
                    });
                }

                using (new EnterWriteLock(_objCharacter.LockObject))
                {
                    for (int i = Skills.Count - 1; i >= 0; --i)
                    {
                        Skill objSkill = Skills[i];
                        if (!setSkillsToRemove.Contains(objSkill))
                            continue;
                        if (!objSkill.IsExoticSkill)
                            _dicSkillBackups.Add(objSkill.SkillId, objSkill);
                        Skills.RemoveAt(i);

                        if (blnCreateKnowledge && objSkill.TotalBaseRating > 0)
                        {
                            KnowledgeSkill objNewKnowledgeSkill = new KnowledgeSkill(_objCharacter)
                            {
                                Type = strKnowledgeSkillTypeToUse.Value,
                                WritableName = objSkill.Name,
                                Base = objSkill.Base,
                                Karma = objSkill.Karma
                            };
                            objNewKnowledgeSkill.Specializations.AddRange(objSkill.Specializations);
                            KnowledgeSkills.AddWithSort(objNewKnowledgeSkill, (x, y) =>
                            {
                                switch (string.CompareOrdinal(x.Type, y.Type))
                                {
                                    case 0:
                                        return CompareSkills(x, y);
                                    case -1:
                                        return -1;
                                    default:
                                        return 1;
                                }
                            }, MergeSkills);
                        }
                    }

                    if (!_objCharacter.Created)
                    {
                        // zero out any skill groups whose skills did not make the final cut
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
            }
        }

        internal void Load(XmlNode xmlSkillNode, bool blnLegacy, CustomActivity parentActivity)
        {
            if (xmlSkillNode == null)
                return;
            using (new EnterWriteLock(_objCharacter.LockObject))
            {
                using (CustomActivity opLoadCharSkills
                       = Timekeeper.StartSyncron("load_char_skills_skillnode", parentActivity))
                {
                    if (!blnLegacy)
                    {
                        using (_ = Timekeeper.StartSyncron("load_char_skills_groups", opLoadCharSkills))
                        {
                            using (XmlNodeList xmlGroupsList = xmlSkillNode.SelectNodes("groups/group"))
                            {
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
                                            SkillGroups.AddWithSort(objGroup, CompareSkillGroups,
                                                                    (objExistingSkillGroup, objNewSkillGroup) =>
                                                                    {
                                                                        foreach (Skill x in objExistingSkillGroup
                                                                            .SkillList
                                                                            .Where(x => !objExistingSkillGroup
                                                                                .SkillList.Contains(x)))
                                                                            objExistingSkillGroup.Add(x);
                                                                        objNewSkillGroup.UnbindSkillGroup();
                                                                    });
                                        }
                                        else
                                            objGroup.Load(xmlNode);
                                    }
                                }
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
                                if (objSkill.SkillId != Guid.Empty && !objSkill.IsExoticSkill)
                                {
                                    Skill objExistingSkill = Skills.FirstOrDefault(x => x.SkillId == objSkill.SkillId);
                                    if (objExistingSkill != null)
                                    {
                                        MergeSkills(objExistingSkill, objSkill);
                                        continue;
                                    }
                                }

                                Skills.AddWithSort(objSkill, CompareSkills, MergeSkills);
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
                                        Skill objUncastSkill = Skill.Load(_objCharacter, xmlNode);
                                        if (objUncastSkill is KnowledgeSkill objSkill)
                                            KnowledgeSkills.Add(objSkill);
                                        else
                                        {
                                            Utils
                                                .BreakIfDebug(); // Somehow, a non-knowledge skill got into a knowledge skill block
                                            objUncastSkill.Dispose();
                                        }
                                    }
                                }
                            }

                            // Legacy sweep for native language skills
                            if (_objCharacter.LastSavedVersion <= new Version(5, 212, 72) && _objCharacter.Created
                                && !KnowledgeSkills.Any(x => x.IsNativeLanguage))
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
                            using (XmlNodeList xmlSkillsList
                                   = xmlSkillNode.SelectNodes("skilljackknowledgeskills/skill"))
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
                                if (objSkill.SkillId != Guid.Empty && !objSkill.IsExoticSkill)
                                {
                                    Skill objExistingSkill = Skills.FirstOrDefault(x => x.SkillId == objSkill.SkillId);
                                    if (objExistingSkill != null)
                                    {
                                        MergeSkills(objExistingSkill, objSkill);
                                        continue;
                                    }
                                }

                                Skills.Add(objSkill);
                            }

                            UpdateUndoList(xmlSkillNode.OwnerDocument);
                        }
                    }

                    XPathNavigator skillsDocXPath = _objCharacter.LoadDataXPath("skills.xml");
                    using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                    out HashSet<string>
                                                                        setSkillGuids))
                    {
                        foreach (XPathNavigator node in skillsDocXPath.Select(
                                     "/chummer/skills/skill[not(exotic) and (" + _objCharacter.Settings.BookXPath()
                                                                               + ')'
                                                                               + SkillFilter(FilterOption.NonSpecial)
                                                                               + ']'))
                        {
                            string strName = node.SelectSingleNodeAndCacheExpression("name")?.Value;
                            if (!string.IsNullOrEmpty(strName))
                                setSkillGuids.Add(strName);
                        }

                        XmlDocument skillsDoc = _objCharacter.LoadData("skills.xml");
                        foreach (string skillId in setSkillGuids.Where(s => Skills.All(skill => skill.Name != s)))
                        {
                            XmlNode objXmlSkillNode
                                = skillsDoc.SelectSingleNode(
                                    "/chummer/skills/skill[name = " + skillId.CleanXPath() + ']');
                            if (objXmlSkillNode != null)
                            {
                                Skill objSkill = Skill.FromData(objXmlSkillNode, _objCharacter, false);
                                Skills.Add(objSkill);
                            }
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
        }

        internal void LoadFromHeroLab(XPathNavigator xmlSkillNode, CustomActivity parentActivity)
        {
            using (new EnterWriteLock(_objCharacter.LockObject))
            {
                using (_ = Timekeeper.StartSyncron("load_char_skills_groups", parentActivity))
                {
                    foreach (XPathNavigator xmlNode in xmlSkillNode.SelectAndCacheExpression("groups/skill"))
                    {
                        SkillGroup objGroup = new SkillGroup(_objCharacter);
                        objGroup.LoadFromHeroLab(xmlNode);
                        SkillGroups.AddWithSort(objGroup, CompareSkillGroups,
                                                (objExistingSkillGroup, objNewSkillGroup) =>
                                                {
                                                    foreach (Skill x in objExistingSkillGroup.SkillList
                                                                 .Where(x => !objExistingSkillGroup
                                                                              .SkillList.Contains(x)))
                                                        objExistingSkillGroup.Add(x);
                                                    objNewSkillGroup.UnbindSkillGroup();
                                                });
                    }

                    //Timekeeper.Finish("load_char_skills_groups");
                }

                using (_ = Timekeeper.StartSyncron("load_char_skills", parentActivity))
                {
                    List<Skill> lstTempSkillList = new List<Skill>(Skills.Count);
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
                        if (objSkill.SkillId != Guid.Empty && !objSkill.IsExoticSkill)
                        {
                            Skill objExistingSkill = Skills.FirstOrDefault(x => x.SkillId == objSkill.SkillId);
                            if (objExistingSkill != null)
                            {
                                MergeSkills(objExistingSkill, objSkill);
                                continue;
                            }
                        }

                        Skills.AddWithSort(objSkill, CompareSkills, MergeSkills);
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
                                                && objLoopKnowledgeSkill.Karma
                                                > objKnowledgeSkillToPutPointsInto.Karma))))
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
        }

        private void UpdateUndoList(XmlDocument xmlSkillOwnerDocument)
        {
            using (new EnterWriteLock(_objCharacter.LockObject))
            {
                //Hacky way of converting Expense entries to guid based skill identification
                //specs already did?
                //First create dictionary mapping name=>guid
                using (LockingDictionary<string, Guid> dicGroups = new LockingDictionary<string, Guid>())
                using (LockingDictionary<string, Guid> dicSkills = new LockingDictionary<string, Guid>())
                {
                    // Potentially expensive checks that can (and therefore should) be parallelized. Normally, this would just be a Parallel.Invoke,
                    // but we want to allow UI messages to happen, just in case this is called on the Main Thread and another thread wants to show a message box.
                    // Not using async-await because this is trivial code and I do not want to infect everything that calls this with async as well.
                    Utils.RunWithoutThreadLock(
                        () =>
                        {
                            Parallel.ForEach(SkillGroups, x =>
                            {
                                // ReSharper disable once AccessToDisposedClosure
                                if (x.Rating > 0 && !dicGroups.ContainsKey(x.Name))
                                    // ReSharper disable once AccessToDisposedClosure
                                    dicGroups.TryAdd(x.Name, x.Id);
                            });
                        },
                        () =>
                        {
                            Parallel.ForEach(Skills, x =>
                            {
                                if (x.TotalBaseRating > 0)
                                    // ReSharper disable once AccessToDisposedClosure
                                    dicSkills.TryAdd(x.Name, x.Id);
                            });
                        },
                        // ReSharper disable once AccessToDisposedClosure
                        () => Parallel.ForEach(KnowledgeSkills, x => dicSkills.TryAdd(x.Name, x.Id)));
                    UpdateUndoSpecific(
                        dicSkills,
                        EnumerableExtensions.ToEnumerable(KarmaExpenseType.AddSkill, KarmaExpenseType.ImproveSkill));
                    UpdateUndoSpecific(dicGroups, KarmaExpenseType.ImproveSkillGroup.Yield());

                    void UpdateUndoSpecific(IDictionary<string, Guid> map,
                                            IEnumerable<KarmaExpenseType> typesRequiringConverting)
                    {
                        //Build a crazy xpath to get everything we want to convert

                        string strXPath = "/character/expenses/expense[type = \'Karma\']/undo[" +
                                          string.Join(
                                              " or ",
                                              typesRequiringConverting.Select(
                                                  x => "karmatype = " + x.ToString().CleanXPath())) +
                                          "]/objectid";

                        //Find everything
                        XmlNodeList lstNodesToChange = xmlSkillOwnerDocument.SelectNodes(strXPath);
                        if (lstNodesToChange != null)
                        {
                            for (int i = 0; i < lstNodesToChange.Count; i++)
                            {
                                lstNodesToChange[i].InnerText
                                    = map.TryGetValue(lstNodesToChange[i].InnerText, out Guid guidLoop)
                                        ? guidLoop.ToString("D", GlobalSettings.InvariantCultureInfo)
                                        : StringExtensions.EmptyGuid;
                            }
                        }
                    }
                }
            }
        }

        internal void Save(XmlTextWriter objWriter)
        {
            using (new EnterReadLock(_objCharacter.LockObject))
            {
                objWriter.WriteStartElement("newskills");

                objWriter.WriteElementString("skillptsmax",
                                             SkillPointsMaximum.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("skillgrpsmax",
                                             SkillGroupPointsMaximum.ToString(GlobalSettings.InvariantCultureInfo));

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
        }

        internal void Reset()
        {
            using (new EnterWriteLock(_objCharacter.LockObject))
            {
                _dicSkills.Clear();
                Skills.Clear();
                KnowledgeSkills.Clear();
                KnowsoftSkills.Clear();
                SkillGroups.Clear();
                SkillPointsMaximum = 0;
                SkillGroupPointsMaximum = 0;
                _blnSkillsInitialized = false;
            }
        }

        /// <summary>
        /// Maximum Skill Rating.
        /// </summary>
        public int MaxSkillRating { get; set; } = 0;
        
        private bool _blnSkillsInitialized;
        private readonly ThreadSafeBindingList<Skill> _lstSkills = new ThreadSafeBindingList<Skill>();
        private readonly LockingDictionary<string, Skill> _dicSkills = new LockingDictionary<string, Skill>();

        /// <summary>
        /// Active Skills
        /// </summary>
        public ThreadSafeBindingList<Skill> Skills
        {
            get
            {
                using (new EnterUpgradeableReadLock(_objCharacter.LockObject))
                {
                    if (!_blnSkillsInitialized && _objCharacter.SkillsSection == this)
                    {
                        using (new EnterWriteLock(_objCharacter.LockObject))
                        {
                            XmlDocument xmlSkillsDocument = _objCharacter.LoadData("skills.xml");
                            using (XmlNodeList xmlSkillList = xmlSkillsDocument
                                       .SelectNodes("/chummer/skills/skill[not(exotic) and ("
                                                    + _objCharacter.Settings.BookXPath() + ')'
                                                    + SkillFilter(FilterOption.NonSpecial) + ']'))
                            {
                                if (xmlSkillList?.Count > 0)
                                {
                                    foreach (XmlNode xmlSkill in xmlSkillList)
                                    {
                                        bool blnIsKnowledgeSkill
                                            = xmlSkillsDocument
                                              .SelectSingleNode("/chummer/categories/category[. = "
                                                                + xmlSkill["category"]?.InnerText.CleanXPath()
                                                                + "]/@type")
                                              ?.Value
                                              != "active";
                                        Skill objSkill = Skill.FromData(xmlSkill, _objCharacter, blnIsKnowledgeSkill);
                                        if (objSkill.SkillId != Guid.Empty && !objSkill.IsExoticSkill)
                                        {
                                            Skill objExistingSkill
                                                = _lstSkills.FirstOrDefault(x => x.SkillId == objSkill.SkillId);
                                            if (objExistingSkill != null)
                                            {
                                                MergeSkills(objExistingSkill, objSkill);
                                                continue;
                                            }
                                        }

                                        _lstSkills.AddWithSort(objSkill, CompareSkills, MergeSkills);
                                    }
                                }
                            }

                            _blnSkillsInitialized = true;
                        }
                    }

                    return _lstSkills;
                }
            }
        }

        /// <summary>
        /// Checks if the character has an active skill with a given name.
        /// </summary>
        /// <param name="strSkillKey">Name of the skill. For exotic skills, this is slightly different, refer to a Skill's DictionaryKey property for more info.</param>
        /// <returns></returns>
        public bool HasActiveSkill(string strSkillKey)
        {
            using (new EnterReadLock(_objCharacter.LockObject))
                return _dicSkills.ContainsKey(strSkillKey);
        }

        /// <summary>
        /// Gets an active skill by its Name. Returns null if none found.
        /// </summary>
        /// <param name="strSkillName">Name of the skill.</param>
        /// <returns></returns>
        public Skill GetActiveSkill(string strSkillName)
        {
            using (new EnterReadLock(_objCharacter.LockObject))
            {
                _dicSkills.TryGetValue(strSkillName, out Skill objReturn);
                return objReturn;
            }
        }

        /// <summary>
        /// This is only used for reflection, so that all zero ratings skills are not uploaded
        /// </summary>
        [HubTag]
        public List<Skill> NotZeroRatingSkills
        {
            get
            {
                using (new EnterReadLock(_objCharacter.LockObject))
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
        }

        public ThreadSafeBindingList<KnowledgeSkill> KnowledgeSkills { get; } = new ThreadSafeBindingList<KnowledgeSkill>();

        /// <summary>
        /// KnowsoftSkills.
        /// </summary>
        public ThreadSafeBindingList<KnowledgeSkill> KnowsoftSkills { get; } = new ThreadSafeBindingList<KnowledgeSkill>();

        /// <summary>
        /// Skill Groups.
        /// </summary>
        public ThreadSafeBindingList<SkillGroup> SkillGroups { get; } = new ThreadSafeBindingList<SkillGroup>();

        public bool HasKnowledgePoints => KnowledgeSkillPoints > 0;

        public bool HasAvailableNativeLanguageSlots
        {
            get
            {
                using (new EnterReadLock(_objCharacter.LockObject))
                    return KnowledgeSkills.Count(x => x.IsNativeLanguage) < 1
                        + ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.NativeLanguageLimit);
            }
        }

        private int _intCachedKnowledgePoints = int.MinValue;

        /// <summary>
        /// Number of free Knowledge Skill Points the character has.
        /// </summary>
        public int KnowledgeSkillPoints
        {
            get
            {
                using (new EnterReadLock(_objCharacter.LockObject))
                {
                    if (_intCachedKnowledgePoints == int.MinValue)
                    {
                        string strExpression = _objCharacter.Settings.KnowledgePointsExpression;
                        if (strExpression.IndexOfAny('{', '+', '-', '*', ',') != -1 || strExpression.Contains("div"))
                        {
                            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                          out StringBuilder sbdValue))
                            {
                                sbdValue.Append(strExpression);
                                _objCharacter.AttributeSection.ProcessAttributesInXPath(sbdValue, strExpression);

                                // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                                object objProcess
                                    = CommonFunctions.EvaluateInvariantXPath(
                                        sbdValue.ToString(), out bool blnIsSuccess);
                                _intCachedKnowledgePoints = blnIsSuccess ? ((double) objProcess).StandardRound() : 0;
                            }
                        }
                        else
                            int.TryParse(strExpression, NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                                         out _intCachedKnowledgePoints);

                        _intCachedKnowledgePoints += ImprovementManager
                                                     .ValueOf(_objCharacter,
                                                              Improvement.ImprovementType.FreeKnowledgeSkills)
                                                     .StandardRound();
                    }

                    return _intCachedKnowledgePoints;
                }
            }
        }

        /// <summary>
        /// Number of free Knowledge skill points the character have remaining
        /// </summary>
        public int KnowledgeSkillPointsRemain
        {
            get
            {
                using (new EnterReadLock(_objCharacter.LockObject))
                    return KnowledgeSkillPoints - KnowledgeSkillPointsUsed;
            }
        }

        /// <summary>
        /// Number of knowledge skill points the character have used.
        /// </summary>
        public int KnowledgeSkillPointsUsed
        {
            get
            {
                using (new EnterReadLock(_objCharacter.LockObject))
                    return KnowledgeSkillRanksSum - SkillPointsSpentOnKnoskills;
            }
        }

        /// <summary>
        /// Sum of knowledge skill ranks the character has allocated.
        /// </summary>
        public int KnowledgeSkillRanksSum
        {
            get
            {
                using (new EnterReadLock(_objCharacter.LockObject))
                    return KnowledgeSkills.Sum(x => x.CurrentSpCost);
            }
        }

        /// <summary>
        /// Number of Skill Points that have been spent on knowledge skills.
        /// </summary>
        public int SkillPointsSpentOnKnoskills
        {
            get
            {
                using (new EnterReadLock(_objCharacter.LockObject))
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
        }

        /// <summary>
        /// Number of free Skill Points the character has left.
        /// </summary>
        public int SkillPoints
        {
            get
            {
                using (new EnterReadLock(_objCharacter.LockObject))
                {
                    if (SkillPointsMaximum == 0)
                    {
                        return 0;
                    }

                    return SkillPointsMaximum - Skills.TotalCostSp() - SkillPointsSpentOnKnoskills;
                }
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
            get
            {
                using (new EnterReadLock(_objCharacter.LockObject))
                    return SkillGroupPointsMaximum - SkillGroups.Sum(x => x.Base - x.FreeBase);
            }
        }

        /// <summary>
        /// Number of maximum Skill Groups the character has.
        /// </summary>
        public int SkillGroupPointsMaximum { get; set; }

        public static int CompareSpecializations(SkillSpecialization lhs, SkillSpecialization rhs)
        {
            if (lhs == null)
                return rhs == null ? 0 : 1;
            if (rhs == null)
                return -1;
            if (lhs.Parent != rhs.Parent)
                return CompareSkills(rhs.Parent, lhs.Parent);
            if (lhs.Free != rhs.Free)
                return lhs.Free ? 1 : -1;
            return string.Compare(lhs.CurrentDisplayName, rhs.CurrentDisplayName, false,
                GlobalSettings.CultureInfo);
        }

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
                return objYGroup == null ? 0 : 1;
            if (objYGroup == null)
                return -1;
            return string.Compare(objXGroup.CurrentDisplayName, objYGroup.CurrentDisplayName, false,
                GlobalSettings.CultureInfo);
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
                    return " and name = " + strName.CleanXPath();

                case FilterOption.XPath:
                    return " and (" + strName + ')';

                default:
                    throw new ArgumentOutOfRangeException(nameof(eFilter), eFilter, null);
            }
        }

        private void MergeSkills(Skill objExistingSkill, Skill objNewSkill)
        {
            objExistingSkill.CopyInternalId(objNewSkill);
            if (objNewSkill.BasePoints > objExistingSkill.BasePoints)
                objExistingSkill.BasePoints = objNewSkill.BasePoints;
            if (objNewSkill.KarmaPoints > objExistingSkill.KarmaPoints)
                objExistingSkill.KarmaPoints = objNewSkill.KarmaPoints;
            objExistingSkill.BuyWithKarma = objNewSkill.BuyWithKarma;
            objExistingSkill.Notes += objNewSkill.Notes;
            objExistingSkill.NotesColor = objNewSkill.NotesColor;
            objExistingSkill.Specializations.AddRangeWithSort(objNewSkill.Specializations, CompareSpecializations);
            objNewSkill.Dispose();
        }

        private List<ListItem> _lstDefaultKnowledgeSkills;

        public IReadOnlyList<ListItem> MyDefaultKnowledgeSkills
        {
            get
            {
                using (new EnterReadLock(_objCharacter.LockObject))
                {
                    if (GlobalSettings.LiveCustomData || _lstDefaultKnowledgeSkills == null)
                    {
                        if (_lstDefaultKnowledgeSkills == null)
                            _lstDefaultKnowledgeSkills = Utils.ListItemListPool.Get();
                        else
                            _lstDefaultKnowledgeSkills.Clear();
                        XPathNavigator xmlSkillsDocument
                            = XmlManager.LoadXPath("skills.xml",
                                                   _objCharacter.Settings.EnabledCustomDataDirectoryPaths);
                        foreach (XPathNavigator xmlSkill in xmlSkillsDocument.SelectAndCacheExpression(
                                     "/chummer/knowledgeskills/skill"))
                        {
                            string strName = xmlSkill.SelectSingleNodeAndCacheExpression("name")?.Value ?? string.Empty;
                            _lstDefaultKnowledgeSkills.Add(
                                new ListItem(
                                    strName,
                                    xmlSkill.SelectSingleNodeAndCacheExpression("translate")?.Value ?? strName));
                        }

                        _lstDefaultKnowledgeSkills.Sort(CompareListItems.CompareNames);
                    }

                    return _lstDefaultKnowledgeSkills;
                }
            }
        }

        private List<ListItem> _lstKnowledgeTypes;

        public IReadOnlyList<ListItem> MyKnowledgeTypes
        {
            get
            {
                using (new EnterReadLock(_objCharacter.LockObject))
                {
                    if (GlobalSettings.LiveCustomData || _lstKnowledgeTypes == null)
                    {
                        if (_lstKnowledgeTypes == null)
                            _lstKnowledgeTypes = Utils.ListItemListPool.Get();
                        else
                            _lstKnowledgeTypes.Clear();
                        XPathNavigator xmlSkillsDocument
                            = XmlManager.LoadXPath("skills.xml",
                                                   _objCharacter.Settings.EnabledCustomDataDirectoryPaths);
                        foreach (XPathNavigator objXmlCategory in xmlSkillsDocument.SelectAndCacheExpression(
                                     "/chummer/categories/category[@type = \"knowledge\"]"))
                        {
                            string strInnerText = objXmlCategory.Value;
                            _lstKnowledgeTypes.Add(new ListItem(strInnerText,
                                                                objXmlCategory
                                                                    .SelectSingleNodeAndCacheExpression("@translate")
                                                                    ?.Value ?? strInnerText));
                        }

                        _lstKnowledgeTypes.Sort(CompareListItems.CompareNames);
                    }

                    return _lstKnowledgeTypes;
                }
            }
        }

        private static readonly PropertyDependencyGraph<SkillsSection> s_SkillSectionDependencyGraph =
            new PropertyDependencyGraph<SkillsSection>(
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
            XPath
        }

        internal void ForceProperyChangedNotificationAll(string strName)
        {
            using (new EnterUpgradeableReadLock(_objCharacter.LockObject))
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
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            using (new EnterReadLock(_objCharacter.LockObject))
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
            using (new EnterReadLock(_objCharacter.LockObject))
            {
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
                                                                                  intAttributeOverrideValue:
                                                                                  0)); // We explicitly want to override the attribute value with 0 because we're just fetching the pure skill pool
                                                           });
                    else
                        strReturn = strReturn.CheapReplace('{' + strSkillKey + '}',
                                                           () => GetActiveSkill(strSkillKey).DisplayName(strLanguage));
                }
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
            using (new EnterReadLock(_objCharacter.LockObject))
            {
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
                                                                     intAttributeOverrideValue:
                                                                     0)); // We explicitly want to override the attribute value with 0 because we're just fetching the pure skill pool
                                              });
                    else
                        sbdInput.CheapReplace(strOriginal, '{' + strSkillKey + '}',
                                              () => GetActiveSkill(strSkillKey).DisplayName(strLanguage));
                }
            }
        }
        #endregion

        /// <inheritdoc />
        public void Dispose()
        {
            UnbindSkillsSection();
            foreach (Skill objSkill in Skills)
                objSkill.Dispose();
            Skills.Dispose();
            foreach (KnowledgeSkill objSkill in KnowledgeSkills)
                objSkill.Dispose();
            KnowledgeSkills.Dispose();
            KnowsoftSkills.Clear();
            KnowsoftSkills.Dispose();
            SkillGroups.Clear();
            SkillGroups.Dispose();
            _dicSkills.Dispose();
            if (_lstDefaultKnowledgeSkills != null)
                Utils.ListItemListPool.Return(_lstDefaultKnowledgeSkills);
            if (_lstKnowledgeTypes != null)
                Utils.ListItemListPool.Return(_lstKnowledgeTypes);
        }
    }
}
