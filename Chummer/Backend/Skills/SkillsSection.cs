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
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using Chummer.Annotations;
using Microsoft.VisualStudio.Threading;
using IAsyncDisposable = System.IAsyncDisposable;

namespace Chummer.Backend.Skills
{
    public sealed class SkillsSection : INotifyMultiplePropertyChanged, IHasLockObject
    {
        private int _intLoading = 1;
        private readonly Character _objCharacter;
        private readonly LockingDictionary<Guid, Skill> _dicSkillBackups = new LockingDictionary<Guid, Skill>();

        public SkillsSection(Character character)
        {
            _objCharacter = character ?? throw new ArgumentNullException(nameof(character));
            using (character.LockObject.EnterWriteLock())
                character.PropertyChanged += OnCharacterPropertyChanged;
            using (character.Settings.LockObject.EnterWriteLock())
                character.Settings.PropertyChanged += OnCharacterSettingsPropertyChanged;
            SkillGroups.BeforeRemove += SkillGroupsOnBeforeRemove;
            KnowsoftSkills.BeforeRemove += KnowsoftSkillsOnBeforeRemove;
            KnowledgeSkills.BeforeRemove += KnowledgeSkillsOnBeforeRemove;
            KnowledgeSkills.ListChanged += KnowledgeSkillsOnListChanged;
            Skills.BeforeRemove += SkillsOnBeforeRemove;
            Skills.ListChanged += SkillsOnListChanged;
        }

        private async void SkillGroupsOnBeforeRemove(object sender, RemovingOldEventArgs e)
        {
            using (await SkillGroups.LockObject.EnterReadLockAsync().ConfigureAwait(false))
            using (await LockObject.EnterReadLockAsync().ConfigureAwait(false))
            {
                if (_intLoading > 0)
                    return;
                IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
                try
                {
                    await (await SkillGroups.GetValueAtAsync(e.OldIndex).ConfigureAwait(false)).DisposeAsync().ConfigureAwait(false);
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        private async void SkillsOnBeforeRemove(object sender, RemovingOldEventArgs e)
        {
            using (await Skills.LockObject.EnterReadLockAsync().ConfigureAwait(false))
            using (await _dicSkillBackups.LockObject.EnterReadLockAsync().ConfigureAwait(false))
            using (await LockObject.EnterReadLockAsync().ConfigureAwait(false))
            {
                if (_intLoading > 0)
                    return;
                Skill objSkill = await Skills.GetValueAtAsync(e.OldIndex).ConfigureAwait(false);
                IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
                try
                {
                    await _dicSkills.RemoveAsync(await objSkill.GetDictionaryKeyAsync().ConfigureAwait(false)).ConfigureAwait(false);
                    if (!await _dicSkillBackups.ContainsValueAsync(objSkill).ConfigureAwait(false))
                        await objSkill.RemoveAsync().ConfigureAwait(false);
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        private async void KnowledgeSkillsOnBeforeRemove(object sender, RemovingOldEventArgs e)
        {
            using (await KnowledgeSkills.LockObject.EnterReadLockAsync().ConfigureAwait(false))
            using (await _dicSkillBackups.LockObject.EnterReadLockAsync().ConfigureAwait(false))
            using (await KnowsoftSkills.LockObject.EnterReadLockAsync().ConfigureAwait(false))
            using (await LockObject.EnterReadLockAsync().ConfigureAwait(false))
            {
                if (_intLoading > 0)
                    return;
                KnowledgeSkill objSkill = await KnowledgeSkills.GetValueAtAsync(e.OldIndex).ConfigureAwait(false);
                IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
                try
                {
                    IAsyncDisposable objLocker2 = await objSkill.LockObject.EnterWriteLockAsync().ConfigureAwait(false);
                    try
                    {
                        objSkill.PropertyChanged -= OnKnowledgeSkillPropertyChanged;
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }
                    if (!await _dicSkillBackups.ContainsValueAsync(objSkill).ConfigureAwait(false)
                        && !await KnowsoftSkills.ContainsAsync(objSkill).ConfigureAwait(false))
                        await objSkill.RemoveAsync().ConfigureAwait(false);
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        private async void KnowsoftSkillsOnBeforeRemove(object sender, RemovingOldEventArgs e)
        {
            using (await KnowsoftSkills.LockObject.EnterReadLockAsync().ConfigureAwait(false))
            using (await _dicSkillBackups.LockObject.EnterReadLockAsync().ConfigureAwait(false))
            using (await KnowledgeSkills.LockObject.EnterReadLockAsync().ConfigureAwait(false))
            using (await LockObject.EnterReadLockAsync().ConfigureAwait(false))
            {
                if (_intLoading > 0)
                    return;
                KnowledgeSkill objSkill = await KnowsoftSkills.GetValueAtAsync(e.OldIndex).ConfigureAwait(false);
                IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
                try
                {
                    if (!await _dicSkillBackups.ContainsValueAsync(objSkill).ConfigureAwait(false)
                        && !await KnowledgeSkills.ContainsAsync(objSkill).ConfigureAwait(false))
                    {
                        await objSkill.RemoveAsync().ConfigureAwait(false);
                    }
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        private async void SkillsOnListChanged(object sender, ListChangedEventArgs e)
        {
            using (await LockObject.EnterReadLockAsync().ConfigureAwait(false))
            {
                if (_intLoading > 0)
                    return;
                switch (e.ListChangedType)
                {
                    case ListChangedType.Reset:
                    {
                        using (await _lstSkills.LockObject.EnterReadLockAsync().ConfigureAwait(false))
                        using (await _dicSkills.LockObject.EnterReadLockAsync().ConfigureAwait(false))
                        {
                            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
                            try
                            {
                                await _dicSkills.ClearAsync().ConfigureAwait(false);
                                await _lstSkills.ForEachAsync(async objSkill =>
                                {
                                    string strLoop = await objSkill.GetDictionaryKeyAsync().ConfigureAwait(false);
                                    await _dicSkills.TryAddAsync(strLoop, objSkill).ConfigureAwait(false);
                                }).ConfigureAwait(false);
                            }
                            finally
                            {
                                await objLocker.DisposeAsync().ConfigureAwait(false);
                            }
                        }

                        break;
                    }
                    case ListChangedType.ItemAdded:
                    {
                        using (await _lstSkills.LockObject.EnterReadLockAsync().ConfigureAwait(false))
                        using (await _dicSkills.LockObject.EnterReadLockAsync().ConfigureAwait(false))
                        {
                            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
                            try
                            {
                                Skill objNewSkill = await _lstSkills.GetValueAtAsync(e.NewIndex).ConfigureAwait(false);
                                string strLoop = await objNewSkill.GetDictionaryKeyAsync().ConfigureAwait(false);
                                await _dicSkills.TryAddAsync(strLoop, objNewSkill).ConfigureAwait(false);
                            }
                            finally
                            {
                                await objLocker.DisposeAsync().ConfigureAwait(false);
                            }
                        }

                        break;
                    }
                }
            }
        }

        private async void KnowledgeSkillsOnListChanged(object sender, ListChangedEventArgs e)
        {
            using (await LockObject.EnterReadLockAsync().ConfigureAwait(false))
            {
                if (_intLoading > 0)
                    return;
                switch (e.ListChangedType)
                {
                    case ListChangedType.Reset:
                    {
                        using (await KnowledgeSkills.LockObject.EnterReadLockAsync().ConfigureAwait(false))
                        {
                            await KnowledgeSkills.ForEachAsync(async objKnoSkill =>
                            {
                                IAsyncDisposable objLocker = await objKnoSkill.LockObject.EnterWriteLockAsync().ConfigureAwait(false);
                                try
                                {
                                    objKnoSkill.PropertyChanged += OnKnowledgeSkillPropertyChanged;
                                }
                                finally
                                {
                                    await objLocker.DisposeAsync().ConfigureAwait(false);
                                }
                            }).ConfigureAwait(false);
                        }

                        goto case ListChangedType.ItemDeleted;
                    }
                    case ListChangedType.ItemAdded:
                    {
                        using (await KnowledgeSkills.LockObject.EnterReadLockAsync().ConfigureAwait(false))
                        {
                            KnowledgeSkill objKnoSkill = await KnowledgeSkills.GetValueAtAsync(e.NewIndex).ConfigureAwait(false);
                            if (objKnoSkill != null)
                            {
                                IAsyncDisposable objLocker = await objKnoSkill.LockObject.EnterWriteLockAsync()
                                                                              .ConfigureAwait(false);
                                try
                                {
                                    objKnoSkill.PropertyChanged += OnKnowledgeSkillPropertyChanged;
                                }
                                finally
                                {
                                    await objLocker.DisposeAsync().ConfigureAwait(false);
                                }
                            }
                        }

                        goto case ListChangedType.ItemDeleted;
                    }
                    case ListChangedType.ItemDeleted:
                        this.OnMultiplePropertyChanged(nameof(KnowledgeSkillRanksSum),
                                                       nameof(HasAvailableNativeLanguageSlots));
                        break;
                }
            }
        }

        private async void OnKnowledgeSkillPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            using (await LockObject.EnterReadLockAsync().ConfigureAwait(false))
            {
                if (_intLoading > 0)
                    return;
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
        }

        private async void OnCharacterPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            using (await LockObject.EnterReadLockAsync().ConfigureAwait(false))
            {
                if (_intLoading > 0)
                    return;
                if (e?.PropertyName == nameof(Character.EffectiveBuildMethodUsesPriorityTables))
                    OnPropertyChanged(nameof(SkillPointsSpentOnKnoskills));
            }
        }

        private async void OnCharacterSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            using (await LockObject.EnterReadLockAsync().ConfigureAwait(false))
            {
                if (_intLoading > 0)
                    return;
                switch (e?.PropertyName)
                {
                    case nameof(CharacterSettings.KnowledgePointsExpression):
                        OnPropertyChanged(nameof(KnowledgeSkillPoints));
                        break;

                    case nameof(CharacterSettings.MaxSkillRatingCreate):
                    {
                        if (!await _objCharacter.GetCreatedAsync().ConfigureAwait(false) && !await _objCharacter.GetIgnoreRulesAsync().ConfigureAwait(false))
                        {
                            await Skills.ForEachAsync(
                                objSkill => objSkill.OnPropertyChanged(nameof(Skill.RatingMaximum))).ConfigureAwait(false);
                            await _dicSkillBackups.ForEachAsync(
                                x =>
                                {
                                    if (!x.Value.IsKnowledgeSkill)
                                        x.Value.OnPropertyChanged(nameof(Skill.RatingMaximum));
                                }).ConfigureAwait(false);
                        }

                        break;
                    }
                    case nameof(CharacterSettings.MaxKnowledgeSkillRatingCreate):
                    {
                        if (!await _objCharacter.GetCreatedAsync().ConfigureAwait(false) && !await _objCharacter.GetIgnoreRulesAsync().ConfigureAwait(false))
                        {
                            await KnowledgeSkills.ForEachAsync(objSkill => objSkill.OnPropertyChanged(nameof(Skill.RatingMaximum))).ConfigureAwait(false);
                            await _dicSkillBackups.ForEachAsync(
                                x =>
                                {
                                    if (x.Value.IsKnowledgeSkill)
                                        x.Value.OnPropertyChanged(nameof(Skill.RatingMaximum));
                                }).ConfigureAwait(false);
                        }

                        break;
                    }
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
            using (LockObject.EnterReadLock())
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

                    using (LockObject.EnterWriteLock())
                    {
                        if (setNamesOfChangedProperties.Contains(nameof(KnowledgeSkillPoints)))
                            _intCachedKnowledgePoints = int.MinValue;
                    }

                    if (PropertyChanged != null)
                    {
                        Utils.RunOnMainThread(() =>
                        {
                            if (PropertyChanged != null)
                            {
                                // ReSharper disable once AccessToModifiedClosure
                                foreach (string strPropertyToChange in setNamesOfChangedProperties)
                                {
                                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(strPropertyToChange));
                                }
                            }
                        });
                    }
                }
                finally
                {
                    if (setNamesOfChangedProperties != null)
                        Utils.StringHashSetPool.Return(ref setNamesOfChangedProperties);
                }
            }
        }

        internal IEnumerable<Skill> GetActiveSkillsFromData(FilterOption eFilterOption, bool blnDeleteSkillsFromBackupIfFound = false, string strName = "", CancellationToken token = default)
        {
            XmlDocument xmlSkillsDocument = _objCharacter.LoadData("skills.xml", token: token);
            using (XmlNodeList xmlSkillList = xmlSkillsDocument
                       .SelectNodes("/chummer/skills/skill[not(exotic = 'True') and (" +
                                    _objCharacter.Settings.BookXPath(token: token)
                                    + ')'
                                    + SkillFilter(eFilterOption, strName) + ']'))
            {
                if (xmlSkillList?.Count > 0)
                {
                    using (LockObject.EnterReadLock(token))
                    {
                        foreach (XmlNode xmlSkill in xmlSkillList)
                        {
                            token.ThrowIfCancellationRequested();
                            if (_dicSkillBackups.Count > 0
                                && xmlSkill.TryGetField("id", Guid.TryParse, out Guid guiSkillId)
                                && _dicSkillBackups.TryGetValue(guiSkillId, out Skill objSkill, token: token)
                                && objSkill != null)
                            {
                                if (blnDeleteSkillsFromBackupIfFound)
                                    _dicSkillBackups.Remove(guiSkillId);
                                yield return objSkill;
                            }
                            else
                            {
                                string strCategoryCleaned = xmlSkill["category"]?.InnerText.CleanXPath();
                                bool blnIsKnowledgeSkill
                                    = string.IsNullOrEmpty(strCategoryCleaned) || xmlSkillsDocument
                                        .SelectSingleNodeAndCacheExpressionAsNavigator(
                                            "/chummer/categories/category[. = "
                                            + strCategoryCleaned + "]/@type", token)
                                        ?.Value
                                    != "active";
                                yield return Skill.FromData(xmlSkill, _objCharacter, blnIsKnowledgeSkill);
                            }
                        }
                    }
                }
            }
        }

        internal async Task<List<Skill>> GetActiveSkillsFromDataAsync(FilterOption eFilterOption, bool blnDeleteSkillsFromBackupIfFound = false, string strName = "", CancellationToken token = default)
        {
            List<Skill> lstReturn;
            XmlDocument xmlSkillsDocument =
                await _objCharacter.LoadDataAsync("skills.xml", token: token).ConfigureAwait(false);
            using (XmlNodeList xmlSkillList = xmlSkillsDocument
                       .SelectNodes("/chummer/skills/skill[not(exotic = 'True') and (" +
                                    await _objCharacter.Settings.BookXPathAsync(token: token).ConfigureAwait(false)
                                    + ')'
                                    + SkillFilter(eFilterOption, strName) + ']'))
            {
                lstReturn = new List<Skill>(xmlSkillList?.Count ?? 0);
                if (xmlSkillList?.Count > 0)
                {
                    using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
                    {
                        foreach (XmlNode xmlSkill in xmlSkillList)
                        {
                            if (await _dicSkillBackups.GetCountAsync(token).ConfigureAwait(false) > 0
                                && xmlSkill.TryGetField("id", Guid.TryParse, out Guid guiSkillId))
                            {
                                (bool blnSuccess, Skill objSkill) = await _dicSkillBackups
                                    .TryGetValueAsync(guiSkillId, token).ConfigureAwait(false);
                                if (blnSuccess && objSkill != null)
                                {
                                    if (blnDeleteSkillsFromBackupIfFound)
                                        await _dicSkillBackups.RemoveAsync(guiSkillId, token).ConfigureAwait(false);
                                    lstReturn.Add(objSkill);
                                }
                                else
                                {
                                    bool blnIsKnowledgeSkill
                                        = (await xmlSkillsDocument
                                              .SelectSingleNodeAndCacheExpressionAsNavigatorAsync(
                                                  "/chummer/categories/category[. = "
                                                  + xmlSkill["category"]?.InnerText.CleanXPath() + "]/@type", token)
                                              .ConfigureAwait(false))
                                          ?.Value
                                          != "active";
                                    lstReturn.Add(Skill.FromData(xmlSkill, _objCharacter, blnIsKnowledgeSkill));
                                }
                            }
                            else
                            {
                                bool blnIsKnowledgeSkill
                                    = (await xmlSkillsDocument
                                          .SelectSingleNodeAndCacheExpressionAsNavigatorAsync(
                                              "/chummer/categories/category[. = "
                                              + xmlSkill["category"]?.InnerText.CleanXPath() + "]/@type", token)
                                          .ConfigureAwait(false))
                                      ?.Value
                                      != "active";
                                lstReturn.Add(Skill.FromData(xmlSkill, _objCharacter, blnIsKnowledgeSkill));
                            }
                        }
                    }
                }
            }

            return lstReturn;
        }

        internal void AddSkills(FilterOption eFilterOption, string strName = "", CancellationToken token = default)
        {
            List<Skill> lstSkillsToAdd = GetActiveSkillsFromData(eFilterOption, true, strName, token).ToList();
            using (LockObject.EnterWriteLock(token))
            {
                foreach (Skill objSkill in lstSkillsToAdd)
                {
                    Guid guidLoop = objSkill.SkillId;
                    if (guidLoop != Guid.Empty && !objSkill.IsExoticSkill)
                    {
                        Skill objExistingSkill = Skills.Find(x => x.SkillId == guidLoop);
                        if (objExistingSkill != null)
                        {
                            MergeSkills(objExistingSkill, objSkill, token);
                            continue;
                        }
                    }

                    Skills.AddWithSort(objSkill, CompareSkills, (x, y) => MergeSkills(x, y, token), token);
                }
            }
        }

        internal async Task AddSkillsAsync(FilterOption eFilterOption, string strName = "", CancellationToken token = default)
        {
            List<Skill> lstSkillsToAdd = await GetActiveSkillsFromDataAsync(eFilterOption, true, strName, token).ConfigureAwait(false);
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                foreach (Skill objSkill in lstSkillsToAdd)
                {
                    Guid guidLoop = await objSkill.GetSkillIdAsync(token).ConfigureAwait(false);
                    if (guidLoop != Guid.Empty && !objSkill.IsExoticSkill)
                    {
                        Skill objExistingSkill = await Skills.FirstOrDefaultAsync(async x => await x.GetSkillIdAsync(token).ConfigureAwait(false) == guidLoop, token)
                                                             .ConfigureAwait(false);
                        if (objExistingSkill != null)
                        {
                            await MergeSkillsAsync(objExistingSkill, objSkill, token).ConfigureAwait(false);
                            continue;
                        }
                    }

                    await Skills
                          .AddWithSortAsync(objSkill, (x, y) => CompareSkillsAsync(x, y, token).AsTask(),
                                            (x, y) => MergeSkillsAsync(x, y, token), token)
                          .ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        internal ExoticSkill AddExoticSkill(string strName, string strSpecific, CancellationToken token = default)
        {
            XmlNode xmlSkillNode = _objCharacter.LoadData("skills.xml", token: token)
                .TryGetNodeByNameOrId("/chummer/skills/skill", strName);
            using (LockObject.EnterWriteLock(token))
            {
                ExoticSkill objExoticSkill = new ExoticSkill(_objCharacter, xmlSkillNode)
                {
                    Specific = strSpecific
                };
                Skills.AddWithSort(objExoticSkill, CompareSkills, (x, y) => MergeSkills(x, y, token), token);
                return objExoticSkill;
            }
        }

        internal async Task<ExoticSkill> AddExoticSkillAsync(string strName, string strSpecific, CancellationToken token = default)
        {
            XmlNode xmlSkillNode
                = (await _objCharacter.LoadDataAsync("skills.xml", token: token).ConfigureAwait(false))
                .TryGetNodeByNameOrId(
                    "/chummer/skills/skill", strName);
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                ExoticSkill objExoticSkill = new ExoticSkill(_objCharacter, xmlSkillNode)
                {
                    Specific = strSpecific
                };
                await Skills.AddWithSortAsync(objExoticSkill, (x, y) => CompareSkillsAsync(x, y, token).AsTask(),
                    (x, y) => MergeSkillsAsync(x, y, token),
                    token: token).ConfigureAwait(false);
                return objExoticSkill;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        internal void RemoveSkills(FilterOption eSkillsToRemove, string strName = "", bool blnCreateKnowledge = true, CancellationToken token = default)
        {
            using (LockObject.EnterReadLock(token))
            {
                HashSet<Skill> setSkillsToRemove
                    = new HashSet<Skill>(GetActiveSkillsFromData(eSkillsToRemove, false, strName, token));
                // Check for duplicates (we'd normally want to make sure the improvement is enabled, but disabled SpecialSkills just force-disables a skill, so we need to keep those)
                _objCharacter.Improvements.ForEach(objImprovement =>
                {
                    if (objImprovement.ImproveType != Improvement.ImprovementType.SpecialSkills)
                        return;
                    FilterOption eFilterOption
                        = (FilterOption) Enum.Parse(typeof(FilterOption), objImprovement.ImprovedName);
                    setSkillsToRemove.ExceptWith(
                        GetActiveSkillsFromData(eFilterOption, false, objImprovement.Target, token));
                }, token);

                if (setSkillsToRemove.Count == 0)
                    return;

                Lazy<string> strKnowledgeSkillTypeToUse = null;
                if (blnCreateKnowledge)
                {
                    strKnowledgeSkillTypeToUse = new Lazy<string>(() =>
                    {
                        XPathNavigator xmlCategories = _objCharacter.LoadDataXPath("skills.xml", token: token)
                                                                    .SelectSingleNodeAndCacheExpression(
                                                                        "/chummer/categories", token);
                        if (xmlCategories.SelectSingleNodeAndCacheExpression("category[@type = \"knowledge\" and . = \"Professional\"]", token)
                            != null)
                            return "Professional";
                        return xmlCategories.SelectSingleNodeAndCacheExpression("category[@type = \"knowledge\"]", token)?.Value
                               ?? "Professional";
                    });
                }

                using (LockObject.EnterWriteLock(token))
                {
                    for (int i = Skills.Count - 1; i >= 0; --i)
                    {
                        token.ThrowIfCancellationRequested();
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
                            }, (x, y) => MergeSkills(x, y, token), token: token);
                        }
                    }

                    if (!_objCharacter.Created)
                    {
                        // zero out any skill groups whose skills did not make the final cut
                        foreach (SkillGroup objSkillGroup in SkillGroups)
                        {
                            token.ThrowIfCancellationRequested();
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

        internal async Task RemoveSkillsAsync(FilterOption eSkillsToRemove, string strName = "", bool blnCreateKnowledge = true, CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                HashSet<Skill> setSkillsToRemove
                    = new HashSet<Skill>(await GetActiveSkillsFromDataAsync(eSkillsToRemove, false, strName, token).ConfigureAwait(false));
                // Check for duplicates (we'd normally want to make sure the improvement is enabled, but disabled SpecialSkills just force-disables a skill, so we need to keep those)
                await (await _objCharacter.GetImprovementsAsync(token).ConfigureAwait(false)).ForEachAsync(async objImprovement =>
                {
                    if (objImprovement.ImproveType == Improvement.ImprovementType.SpecialSkills)
                    {
                        FilterOption eFilterOption
                            = (FilterOption)Enum.Parse(typeof(FilterOption), objImprovement.ImprovedName);
                        setSkillsToRemove.ExceptWith(
                            await GetActiveSkillsFromDataAsync(eFilterOption, false, objImprovement.Target, token).ConfigureAwait(false));
                    }
                }, token: token).ConfigureAwait(false);

                if (setSkillsToRemove.Count == 0)
                    return;

                AsyncLazy<string> strKnowledgeSkillTypeToUse = null;
                if (blnCreateKnowledge)
                {
                    strKnowledgeSkillTypeToUse = new AsyncLazy<string>(async () =>
                    {
                        XPathNavigator xmlCategories = await (await _objCharacter.LoadDataXPathAsync("skills.xml", token: token).ConfigureAwait(false))
                                                             .SelectSingleNodeAndCacheExpressionAsync(
                                                                 "/chummer/categories", token).ConfigureAwait(false);
                        if (await xmlCategories.SelectSingleNodeAndCacheExpressionAsync("category[@type = \"knowledge\" and . = \"Professional\"]", token).ConfigureAwait(false)
                            != null)
                            return "Professional";
                        return (await xmlCategories.SelectSingleNodeAndCacheExpressionAsync("category[@type = \"knowledge\"]", token).ConfigureAwait(false))?.Value
                               ?? "Professional";
                    }, Utils.JoinableTaskFactory);
                }

                IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    ThreadSafeBindingList<Skill> lstSkills = await GetSkillsAsync(token).ConfigureAwait(false);
                    for (int i = await lstSkills.GetCountAsync(token).ConfigureAwait(false) - 1; i >= 0; --i)
                    {
                        Skill objSkill = await lstSkills.GetValueAtAsync(i, token).ConfigureAwait(false);
                        if (!setSkillsToRemove.Contains(objSkill))
                            continue;
                        if (!objSkill.IsExoticSkill)
                            await _dicSkillBackups.AddAsync(objSkill.SkillId, objSkill, token).ConfigureAwait(false);
                        await lstSkills.RemoveAtAsync(i, token).ConfigureAwait(false);

                        if (blnCreateKnowledge && await objSkill.GetTotalBaseRatingAsync(token).ConfigureAwait(false) > 0)
                        {
                            KnowledgeSkill objNewKnowledgeSkill = new KnowledgeSkill(_objCharacter);
                            await objNewKnowledgeSkill.SetBaseAsync(await objSkill.GetBaseAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
                            await objNewKnowledgeSkill.SetKarmaAsync(await objSkill.GetKarmaAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
                            await objNewKnowledgeSkill.SetTypeAsync(
                                await strKnowledgeSkillTypeToUse.GetValueAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
                            await objNewKnowledgeSkill.SetWritableNameAsync(await objSkill.GetNameAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
                            await objNewKnowledgeSkill.Specializations.AddRangeAsync(objSkill.Specializations, token: token).ConfigureAwait(false);
                            await (await GetKnowledgeSkillsAsync(token).ConfigureAwait(false)).AddWithSortAsync(
                                objNewKnowledgeSkill, async (x, y) =>
                                {
                                    switch (string.CompareOrdinal(await x.GetTypeAsync(token).ConfigureAwait(false),
                                                                  await y.GetTypeAsync(token).ConfigureAwait(false)))
                                    {
                                        case 0:
                                            return await CompareSkillsAsync(x, y, token).ConfigureAwait(false);

                                        case -1:
                                            return -1;

                                        default:
                                            return 1;
                                    }
                                }, (x, y) => MergeSkillsAsync(x, y, token), token: token).ConfigureAwait(false);
                        }
                    }

                    if (!await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false))
                    {
                        // zero out any skill groups whose skills did not make the final cut
                        await (await GetSkillGroupsAsync(token).ConfigureAwait(false)).ForEachAsync(
                            async objSkillGroup =>
                            {
                                if (!await objSkillGroup.SkillList
                                                        .AnyAsync(
                                                            async x => await _dicSkills.ContainsKeyAsync(
                                                                await x.GetDictionaryKeyAsync(token)
                                                                       .ConfigureAwait(false), token).ConfigureAwait(false), token: token)
                                                        .ConfigureAwait(false))
                                {
                                    await objSkillGroup.SetBaseAsync(0, token).ConfigureAwait(false);
                                    await objSkillGroup.SetKarmaAsync(0, token).ConfigureAwait(false);
                                }
                            }, token: token).ConfigureAwait(false);
                    }
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        internal void Load(XmlNode xmlSkillNode, bool blnLegacy, CustomActivity parentActivity, CancellationToken token = default)
        {
            Utils.SafelyRunSynchronously(() => LoadCoreAsync(true, xmlSkillNode, blnLegacy, parentActivity, token), token);
        }

        internal Task LoadAsync(XmlNode xmlSkillNode, bool blnLegacy, CustomActivity parentActivity, CancellationToken token = default)
        {
            return LoadCoreAsync(false, xmlSkillNode, blnLegacy, parentActivity, token);
        }

        private async Task LoadCoreAsync(bool blnSync, XmlNode xmlSkillNode, bool blnLegacy,
                                         CustomActivity parentActivity, CancellationToken token = default)
        {
            if (xmlSkillNode == null)
                return;
            IDisposable objLocker = null;
            IAsyncDisposable objLockerAsync = null;
            if (blnSync)
                // ReSharper disable once MethodHasAsyncOverload
                objLocker = LockObject.EnterWriteLock(token);
            else
                objLockerAsync = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                Interlocked.Increment(ref _intLoading);
                try
                {
                    bool blnDidInitializeInLoad = false;
                    using (CustomActivity opLoadCharSkills =
                           Timekeeper.StartSyncron("load_char_skills_skillnode", parentActivity))
                    {
                        _lstSkills.RaiseListChangedEvents = false;
                        _lstKnowledgeSkills.RaiseListChangedEvents = false;
                        _lstKnowsoftSkills.RaiseListChangedEvents = false;
                        _lstSkillGroups.RaiseListChangedEvents = false;
                        try
                        {

                            if (blnSync)
                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                _dicSkills.Clear();
                            else
                                await _dicSkills.ClearAsync(token).ConfigureAwait(false);
                            if (!blnLegacy)
                            {
                                using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                           out HashSet<string>
                                               setSkillIdsToSkip))
                                {
                                    // Special loading procedure where we initialize our skills list at the same time as loading it
                                    // This is faster than doing the initialize first and then loading of all our skills afterwards
                                    using (_ = Timekeeper.StartSyncron("load_char_skills_initialize", opLoadCharSkills))
                                    using (blnSync
                                               // ReSharper disable once MethodHasAsyncOverload
                                               ? _objSkillsInitializerLock.EnterReadLock(token)
                                               : await _objSkillsInitializerLock.EnterReadLockAsync(token)
                                                   .ConfigureAwait(false))
                                    {
                                        if (!_blnSkillsInitialized && _objCharacter.SkillsSection == this)
                                        {
                                            IDisposable objLocker2 = null;
                                            IAsyncDisposable objLocker2Async = null;
                                            if (blnSync)
                                                // ReSharper disable once MethodHasAsyncOverload
                                                objLocker2 = _objSkillsInitializerLock.EnterWriteLock(token);
                                            else
                                                objLocker2Async = await _objSkillsInitializerLock
                                                    .EnterWriteLockAsync(token)
                                                    .ConfigureAwait(false);
                                            try
                                            {
                                                token.ThrowIfCancellationRequested();
                                                if (!_blnSkillsInitialized)
                                                {
                                                    blnDidInitializeInLoad = true;
                                                    XmlDocument xmlSkillsDataDoc = blnSync
                                                        // ReSharper disable once MethodHasAsyncOverload
                                                        ? _objCharacter.LoadData("skills.xml", token: token)
                                                        : await _objCharacter.LoadDataAsync("skills.xml", token: token)
                                                            .ConfigureAwait(false);
                                                    using (XmlNodeList lstSkillDataNodes = xmlSkillsDataDoc.SelectNodes(
                                                               "/chummer/skills/skill[not(exotic = 'True') and ("
                                                               + (blnSync
                                                                   // ReSharper disable once MethodHasAsyncOverload
                                                                   ? _objCharacter.Settings.BookXPath(token: token)
                                                                   : await _objCharacter.Settings
                                                                       .BookXPathAsync(token: token)
                                                                       .ConfigureAwait(false)) + ')'
                                                               + SkillFilter(FilterOption.NonSpecial) + ']'))
                                                    {
                                                        if (lstSkillDataNodes?.Count > 0)
                                                        {
                                                            foreach (XmlNode xmlSkillDataNode in lstSkillDataNodes)
                                                            {
                                                                bool blnIsKnowledgeSkill
                                                                    = (blnSync
                                                                          ? xmlSkillsDataDoc
                                                                              // ReSharper disable once MethodHasAsyncOverload
                                                                              .SelectSingleNodeAndCacheExpressionAsNavigator(
                                                                                  "/chummer/categories/category[. = "
                                                                                  + xmlSkillDataNode["category"]
                                                                                      ?.InnerText
                                                                                      .CleanXPath()
                                                                                  + "]/@type", token)
                                                                          : await xmlSkillsDataDoc
                                                                              .SelectSingleNodeAndCacheExpressionAsNavigatorAsync(
                                                                                  "/chummer/categories/category[. = "
                                                                                  + xmlSkillDataNode["category"]
                                                                                      ?.InnerText
                                                                                      .CleanXPath()
                                                                                  + "]/@type", token)
                                                                              .ConfigureAwait(false))
                                                                      ?.Value
                                                                      != "active";
                                                                Skill objSkill = Skill.FromData(xmlSkillDataNode,
                                                                    _objCharacter,
                                                                    blnIsKnowledgeSkill);
                                                                if (objSkill.SkillId != Guid.Empty)
                                                                {
                                                                    string strSkillId = objSkill.SkillId.ToString("D");
                                                                    XmlNode xmlLoadingSkillNode =
                                                                        xmlSkillNode.SelectSingleNode(
                                                                            "skills/skill[suid = " +
                                                                            strSkillId.CleanXPath() + ']') ??
                                                                        xmlSkillNode.SelectSingleNode(
                                                                            "knoskills/skill[suid = " +
                                                                            strSkillId.CleanXPath() + ']');
                                                                    if (xmlLoadingSkillNode != null)
                                                                    {
                                                                        setSkillIdsToSkip.Add(strSkillId);
                                                                        Skill.Load(_objCharacter, xmlLoadingSkillNode,
                                                                            objSkill);
                                                                    }
                                                                }

                                                                KnowledgeSkill objKnoSkill = blnIsKnowledgeSkill
                                                                    ? objSkill as KnowledgeSkill
                                                                    : null;

                                                                if (blnSync)
                                                                {
                                                                    if (objKnoSkill != null)
                                                                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                                        _lstKnowledgeSkills.Add(objKnoSkill);
                                                                    else
                                                                    {
                                                                        string strKey = objSkill.DictionaryKey;
                                                                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                                        if (_dicSkills.TryAdd(strKey, objSkill))
                                                                        {
                                                                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                                            _lstSkills.Add(objSkill);
                                                                        }
                                                                        else if (_dicSkills.TryGetValue(strKey,
                                                                                 out Skill objExistingSkill, token))
                                                                        {
                                                                            // ReSharper disable once MethodHasAsyncOverload
                                                                            MergeSkills(objExistingSkill, objSkill,
                                                                                token);
                                                                        }
                                                                        else
                                                                            Utils.BreakIfDebug();
                                                                    }
                                                                }
                                                                else if (objKnoSkill != null)
                                                                    await _lstKnowledgeSkills
                                                                        .AddAsync(objKnoSkill, token)
                                                                        .ConfigureAwait(false);
                                                                else
                                                                {
                                                                    string strKey = await objSkill
                                                                        .GetDictionaryKeyAsync(token)
                                                                        .ConfigureAwait(false);
                                                                    if (await _dicSkills
                                                                            .TryAddAsync(strKey, objSkill, token)
                                                                            .ConfigureAwait(false))
                                                                    {
                                                                        await _lstSkills.AddAsync(objSkill, token)
                                                                            .ConfigureAwait(false);
                                                                    }
                                                                    else
                                                                    {
                                                                        (bool blnSuccess, Skill objExistingSkill) =
                                                                            await _dicSkills.TryGetValueAsync(strKey,
                                                                                token).ConfigureAwait(false);
                                                                        if (blnSuccess)
                                                                            await MergeSkillsAsync(objExistingSkill,
                                                                                objSkill, token).ConfigureAwait(false);
                                                                        else
                                                                            Utils.BreakIfDebug();
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }

                                                    _blnSkillsInitialized = true;
                                                }
                                            }
                                            finally
                                            {
                                                if (blnSync)
                                                    // ReSharper disable once MethodHasAsyncOverload
                                                    objLocker2.Dispose();
                                                else
                                                    await objLocker2Async.DisposeAsync().ConfigureAwait(false);
                                            }
                                        }
                                    }

                                    using (_ = Timekeeper.StartSyncron("load_char_skills_groups", opLoadCharSkills))
                                    {
                                        using (XmlNodeList xmlGroupsList = xmlSkillNode.SelectNodes("groups/group"))
                                        {
                                            if (xmlGroupsList?.Count > 0)
                                            {
                                                foreach (XmlNode xmlNode in xmlGroupsList)
                                                {
                                                    string strName = xmlNode["name"]?.InnerText ?? string.Empty;
                                                    SkillGroup objGroup = null;
                                                    if (!string.IsNullOrEmpty(strName))
                                                        objGroup = blnSync
                                                            ? SkillGroups.FirstOrDefault(x => x.Name == strName)
                                                            : await SkillGroups.FirstOrDefaultAsync(
                                                                    x => x.Name == strName, token: token)
                                                                .ConfigureAwait(false);
                                                    if (objGroup != null)
                                                        objGroup.Load(xmlNode, token: token);
                                                    else
                                                    {
                                                        objGroup = new SkillGroup(_objCharacter, strName);
                                                        objGroup.Load(xmlNode, token: token);
                                                        if (blnSync)
                                                        {
                                                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                            SkillGroups.Add(objGroup);
                                                        }
                                                        else
                                                        {
                                                            await SkillGroups.AddAsync(objGroup, token)
                                                                .ConfigureAwait(false);
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        //Timekeeper.Finish("load_char_skills_groups");
                                    }

                                    using (_ = Timekeeper.StartSyncron("load_char_skills_normal", opLoadCharSkills))
                                    {
                                        using (XmlNodeList xmlSkillsList = xmlSkillNode.SelectNodes("skills/skill"))
                                        {
                                            if (xmlSkillsList?.Count > 0)
                                            {
                                                foreach (XmlNode xmlNode in xmlSkillsList)
                                                {
                                                    string strLoopId = xmlNode["suid"]?.InnerText;
                                                    if (!string.IsNullOrEmpty(strLoopId) &&
                                                        setSkillIdsToSkip.Contains(strLoopId))
                                                        continue;
                                                    Skill objSkill = Skill.Load(_objCharacter, xmlNode);
                                                    if (objSkill == null)
                                                        continue;
                                                    if (blnSync)
                                                    {
                                                        string strKey = objSkill.DictionaryKey;
                                                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                        if (_dicSkills.TryAdd(strKey, objSkill))
                                                        {
                                                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                            _lstSkills.Add(objSkill);
                                                        }
                                                        else if (_dicSkills.TryGetValue(strKey,
                                                                     out Skill objExistingSkill, token))
                                                        {
                                                            // ReSharper disable once MethodHasAsyncOverload
                                                            MergeSkills(objExistingSkill, objSkill, token);
                                                        }
                                                        else
                                                            Utils.BreakIfDebug();
                                                    }
                                                    else
                                                    {
                                                        string strKey = await objSkill.GetDictionaryKeyAsync(token)
                                                            .ConfigureAwait(false);
                                                        if (await _dicSkills.TryAddAsync(strKey, objSkill, token)
                                                                .ConfigureAwait(false))
                                                        {
                                                            await _lstSkills.AddAsync(objSkill, token)
                                                                .ConfigureAwait(false);
                                                        }
                                                        else
                                                        {
                                                            (bool blnSuccess, Skill objExistingSkill) =
                                                                await _dicSkills.TryGetValueAsync(strKey,
                                                                    token).ConfigureAwait(false);
                                                            if (blnSuccess)
                                                                await MergeSkillsAsync(objExistingSkill,
                                                                    objSkill, token).ConfigureAwait(false);
                                                            else
                                                                Utils.BreakIfDebug();
                                                        }
                                                    }
                                                }
                                            }
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
                                                    string strLoopId = xmlNode["suid"]?.InnerText;
                                                    if (!string.IsNullOrEmpty(strLoopId) &&
                                                        setSkillIdsToSkip.Contains(strLoopId))
                                                        continue;
                                                    Skill objUncastSkill = Skill.Load(_objCharacter, xmlNode);
                                                    if (objUncastSkill is KnowledgeSkill objSkill)
                                                    {
                                                        if (blnSync)
                                                        {
                                                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                            if (!KnowledgeSkills.Contains(objSkill))
                                                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                                KnowledgeSkills.Add(objSkill);
                                                        }
                                                        else if (!await KnowledgeSkills.ContainsAsync(objSkill, token)
                                                                     .ConfigureAwait(false))
                                                            await KnowledgeSkills.AddAsync(objSkill, token)
                                                                .ConfigureAwait(false);
                                                    }
                                                    else
                                                    {
                                                        Utils
                                                            .BreakIfDebug(); // Somehow, a non-knowledge skill got into a knowledge skill block
                                                        if (blnSync)
                                                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                            objUncastSkill.Remove();
                                                        else
                                                            await objUncastSkill.RemoveAsync(token)
                                                                .ConfigureAwait(false);
                                                    }
                                                }
                                            }
                                        }

                                        // Legacy sweep for native language skills
                                        if (_objCharacter.LastSavedVersion <= new Version(5, 212, 72))
                                        {
                                            if (blnSync)
                                            {
                                                // ReSharper disable once MethodHasAsyncOverload
                                                if (_objCharacter.Created &&
                                                    !KnowledgeSkills.Any(x => x.IsNativeLanguage, token))
                                                {
                                                    KnowledgeSkill objEnglishSkill = new KnowledgeSkill(_objCharacter)
                                                    {
                                                        WritableName = "English",
                                                        IsNativeLanguage = true
                                                    };
                                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                    KnowledgeSkills.Add(objEnglishSkill);
                                                }
                                            }
                                            else if (await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false)
                                                     && !await KnowledgeSkills
                                                         .AnyAsync(x => x.GetIsNativeLanguageAsync(token).AsTask(),
                                                             token)
                                                         .ConfigureAwait(false))
                                            {
                                                KnowledgeSkill objEnglishSkill = new KnowledgeSkill(_objCharacter);
                                                await objEnglishSkill.SetWritableNameAsync("English", token)
                                                    .ConfigureAwait(false);
                                                await objEnglishSkill.SetIsNativeLanguageAsync(true, token)
                                                    .ConfigureAwait(false);
                                                await KnowledgeSkills.AddAsync(objEnglishSkill, token)
                                                    .ConfigureAwait(false);
                                            }
                                        }
                                        //Timekeeper.Finish("load_char_skills_kno");
                                    }
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
                                                {
                                                    KnowledgeSkill objSkill
                                                        = new KnowledgeSkill(_objCharacter, strName, false);
                                                    if (blnSync)
                                                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                        KnowsoftSkills.Add(objSkill);
                                                    else
                                                        await KnowsoftSkills.AddAsync(objSkill, token)
                                                            .ConfigureAwait(false);
                                                }
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

                                    async ValueTask<bool> OldSkillFilterAsync(Skill skill)
                                    {
                                        if (await skill.GetRatingAsync(token).ConfigureAwait(false) > 0)
                                            return true;

                                        if (skill.SkillCategory == "Resonance Active"
                                            && !await _objCharacter.GetRESEnabledAsync(token).ConfigureAwait(false))
                                            return false;

                                        //This could be more fine grained, but frankly i don't care
                                        return skill.SkillCategory != "Magical Active"
                                               || await _objCharacter.GetMAGEnabledAsync(token).ConfigureAwait(false);
                                    }

                                    foreach (Skill objSkill in lstTempSkillList)
                                    {
                                        if (objSkill is KnowledgeSkill objKnoSkill)
                                        {
                                            if (blnSync)
                                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                KnowledgeSkills.Add(objKnoSkill);
                                            else
                                                await KnowledgeSkills.AddAsync(objKnoSkill, token)
                                                    .ConfigureAwait(false);
                                        }
                                        else if (blnSync)
                                        {
                                            if (OldSkillFilter(objSkill))
                                            {
                                                lstUnsortedSkills.Add(objSkill);
                                            }
                                        }
                                        else if (await OldSkillFilterAsync(objSkill).ConfigureAwait(false))
                                        {
                                            lstUnsortedSkills.Add(objSkill);
                                        }
                                    }

                                    lstUnsortedSkills.Sort(CompareSkills);

                                    foreach (Skill objSkill in lstUnsortedSkills)
                                    {
                                        if (objSkill.SkillId != Guid.Empty && !objSkill.IsExoticSkill)
                                        {
                                            Skill objExistingSkill
                                                = Skills.FirstOrDefault(x => x.SkillId == objSkill.SkillId);
                                            if (objExistingSkill != null)
                                            {
                                                if (blnSync)
                                                    // ReSharper disable once MethodHasAsyncOverload
                                                    MergeSkills(objExistingSkill, objSkill, token);
                                                else
                                                    await MergeSkillsAsync(objExistingSkill, objSkill, token)
                                                        .ConfigureAwait(false);
                                                continue;
                                            }
                                        }

                                        if (blnSync)
                                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                            Skills.Add(objSkill);
                                        else
                                            await Skills.AddAsync(objSkill, token).ConfigureAwait(false);
                                    }

                                    if (blnSync)
                                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                        UpdateUndoList(xmlSkillNode.OwnerDocument);
                                    else
                                        await UpdateUndoListAsync(xmlSkillNode.OwnerDocument, token)
                                            .ConfigureAwait(false);
                                }
                            }

                            if (!blnDidInitializeInLoad)
                            {
                                using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                           out HashSet<string>
                                               setSkillNames))
                                {
                                    if (blnSync)
                                    {
                                        // ReSharper disable once MethodHasAsyncOverload
                                        Skills.ForEach(objSkill => setSkillNames.Add(objSkill.Name), token: token);
                                    }
                                    else
                                    {
                                        await Skills
                                            .ForEachAsync(
                                                async objSkill =>
                                                    setSkillNames.Add(await objSkill.GetNameAsync(token)
                                                        .ConfigureAwait(false)),
                                                token: token).ConfigureAwait(false);
                                    }

                                    XmlDocument xmlSkillsDataDoc = blnSync
                                        // ReSharper disable once MethodHasAsyncOverload
                                        ? _objCharacter.LoadData("skills.xml", token: token)
                                        : await _objCharacter.LoadDataAsync("skills.xml", token: token)
                                            .ConfigureAwait(false);
                                    using (XmlNodeList lstSkillDataNodes = xmlSkillsDataDoc.SelectNodes(
                                               "/chummer/skills/skill[not(exotic = 'True') and ("
                                               + (blnSync
                                                   // ReSharper disable once MethodHasAsyncOverload
                                                   ? _objCharacter.Settings.BookXPath(token: token)
                                                   : await _objCharacter.Settings.BookXPathAsync(token: token)
                                                       .ConfigureAwait(false)) + ')'
                                               + SkillFilter(FilterOption.NonSpecial) + ']'))
                                    {
                                        if (lstSkillDataNodes?.Count > 0)
                                        {
                                            foreach (XmlNode xmlSkillDataNode in lstSkillDataNodes)
                                            {
                                                string strName = xmlSkillDataNode["name"]?.InnerText;
                                                if (!string.IsNullOrEmpty(strName) && setSkillNames.Add(strName))
                                                {
                                                    Skill objSkill = Skill.FromData(xmlSkillDataNode, _objCharacter,
                                                        false);
                                                    if (objSkill == null)
                                                        continue;
                                                    if (blnSync)
                                                    {
                                                        string strKey = objSkill.DictionaryKey;
                                                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                        if (_dicSkills.TryAdd(strKey, objSkill))
                                                        {
                                                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                            _lstSkills.Add(objSkill);
                                                        }
                                                        else if (_dicSkills.TryGetValue(strKey,
                                                                     out Skill objExistingSkill, token))
                                                        {
                                                            // ReSharper disable once MethodHasAsyncOverload
                                                            MergeSkills(objExistingSkill, objSkill, token);
                                                        }
                                                        else
                                                            Utils.BreakIfDebug();
                                                    }
                                                    else
                                                    {
                                                        string strKey = await objSkill.GetDictionaryKeyAsync(token)
                                                            .ConfigureAwait(false);
                                                        if (await _dicSkills.TryAddAsync(strKey, objSkill, token)
                                                                .ConfigureAwait(false))
                                                        {
                                                            await _lstSkills.AddAsync(objSkill, token)
                                                                .ConfigureAwait(false);
                                                        }
                                                        else
                                                        {
                                                            (bool blnSuccess, Skill objExistingSkill) =
                                                                await _dicSkills.TryGetValueAsync(strKey,
                                                                    token).ConfigureAwait(false);
                                                            if (blnSuccess)
                                                                await MergeSkillsAsync(objExistingSkill,
                                                                    objSkill, token).ConfigureAwait(false);
                                                            else
                                                                Utils.BreakIfDebug();
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (blnSync)
                            {
                                foreach (KnowledgeSkill objKnoSkill in KnowledgeSkills)
                                {
                                    using (objKnoSkill.LockObject.EnterWriteLock(token))
                                        objKnoSkill.PropertyChanged += OnKnowledgeSkillPropertyChanged;
                                }
                            }
                            else
                            {
                                await KnowledgeSkills.ForEachAsync(
                                    async objKnoSkill =>
                                    {
                                        IAsyncDisposable objLocker2
                                            = await objKnoSkill.LockObject.EnterWriteLockAsync(token)
                                                .ConfigureAwait(false);
                                        try
                                        {
                                            token.ThrowIfCancellationRequested();
                                            objKnoSkill.PropertyChanged
                                                += OnKnowledgeSkillPropertyChanged;
                                        }
                                        finally
                                        {
                                            await objLocker2.DisposeAsync().ConfigureAwait(false);
                                        }
                                    },
                                    token).ConfigureAwait(false);
                            }

                            //This might give subtle bugs in the future,
                            //but right now it needs to be run once when upgrading or it might crash.
                            //As some didn't they crashed on loading skills.
                            //After this have run, it won't (for the crash i'm aware)
                            //TODO: Move it to the other side of the if someday?

                            if (blnSync)
                            {
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
                                        else
                                        {
                                            // TODO: Skill groups don't refresh their CanIncrease property correctly when the last of their skills is being added, as the total base rating will be zero. Call this here to force a refresh.
                                            objSkillGroup.OnPropertyChanged(nameof(SkillGroup.SkillList));
                                        }
                                    }
                                }
                                else
                                {
                                    // TODO: Skill groups don't refresh their CanIncrease property correctly when the last of their skills is being added, as the total base rating will be zero. Call this here to force a refresh.
                                    foreach (SkillGroup g in blnSync
                                                 ? SkillGroups.ToList()
                                                 : await SkillGroups.ToListAsync(token).ConfigureAwait(false))
                                    {
                                        g.OnPropertyChanged(nameof(SkillGroup.SkillList));
                                    }
                                }
                            }
                            else if (!await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false))
                            {
                                // zero out any skillgroups whose skills did not make the final cut
                                await SkillGroups.ForEachAsync(async objSkillGroup =>
                                {
                                    if (!await objSkillGroup.SkillList.AnyAsync(
                                                async x => await _dicSkills.ContainsKeyAsync(
                                                        await x.GetDictionaryKeyAsync(token)
                                                            .ConfigureAwait(false), token)
                                                    .ConfigureAwait(false), token: token)
                                            .ConfigureAwait(false))
                                    {
                                        await objSkillGroup.SetBaseAsync(0, token).ConfigureAwait(false);
                                        await objSkillGroup.SetKarmaAsync(0, token).ConfigureAwait(false);
                                    }
                                    else
                                    {
                                        // TODO: Skill groups don't refresh their CanIncrease property correctly when the last of their skills is being added, as the total base rating will be zero. Call this here to force a refresh.
                                        objSkillGroup.OnPropertyChanged(nameof(SkillGroup.SkillList));
                                    }
                                }, token).ConfigureAwait(false);
                            }
                            else
                            {
                                // TODO: Skill groups don't refresh their CanIncrease property correctly when the last of their skills is being added, as the total base rating will be zero. Call this here to force a refresh.
                                foreach (SkillGroup objSkillGroup in await SkillGroups.ToListAsync(token)
                                             .ConfigureAwait(false))
                                {
                                    objSkillGroup.OnPropertyChanged(nameof(SkillGroup.SkillList));
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
                            if (blnSync)
                            {
                                // ReSharper disable MethodHasAsyncOverloadWithCancellation
                                Utils.RunWithoutThreadLock(new Action[]
                                {
                                    () => _lstSkills.Sort(CompareSkills),
                                    () => _lstKnowledgeSkills.Sort(CompareSkills),
                                    () => _lstKnowsoftSkills.Sort(CompareSkills),
                                    () => _lstSkillGroups.Sort(CompareSkillGroups)
                                }, token: token);
                                // ReSharper restore MethodHasAsyncOverloadWithCancellation
                            }
                            else
                            {
                                await Task.WhenAll(_lstSkills.SortAsync(CompareSkills, token),
                                    _lstKnowledgeSkills.SortAsync(CompareSkills, token),
                                    _lstKnowsoftSkills.SortAsync(CompareSkills, token),
                                    _lstSkillGroups.SortAsync(CompareSkillGroups, token)).ConfigureAwait(false);
                            }
                        }
                        finally
                        {
                            _lstSkills.RaiseListChangedEvents = true;
                            _lstKnowledgeSkills.RaiseListChangedEvents = true;
                            _lstKnowsoftSkills.RaiseListChangedEvents = true;
                            _lstSkillGroups.RaiseListChangedEvents = true;
                        }

                        if (blnSync)
                        {
                            // ReSharper disable MethodHasAsyncOverloadWithCancellation
                            _lstSkills.ResetBindings();
                            _lstKnowledgeSkills.ResetBindings();
                            _lstKnowsoftSkills.ResetBindings();
                            _lstSkillGroups.ResetBindings();
                            // ReSharper restore MethodHasAsyncOverloadWithCancellation
                        }
                        else
                        {
                            await _lstSkills.ResetBindingsAsync(token).ConfigureAwait(false);
                            await _lstKnowledgeSkills.ResetBindingsAsync(token).ConfigureAwait(false);
                            await _lstKnowsoftSkills.ResetBindingsAsync(token).ConfigureAwait(false);
                            await _lstSkillGroups.ResetBindingsAsync(token).ConfigureAwait(false);
                        }
                    }
                }
                finally
                {
                    Interlocked.Decrement(ref _intLoading);
                }
            }
            finally
            {
                if (blnSync)
                    // ReSharper disable once MethodHasAsyncOverload
                    objLocker.Dispose();
                else
                    await objLockerAsync.DisposeAsync().ConfigureAwait(false);
            }
        }

        internal void LoadFromHeroLab(XPathNavigator xmlSkillNode, CustomActivity parentActivity, CancellationToken token = default)
        {
            using (LockObject.EnterWriteLock(token))
            {
                Interlocked.Increment(ref _intLoading);
                try
                {
                    using (_ = Timekeeper.StartSyncron("load_char_skills_groups", parentActivity))
                    {
                        foreach (XPathNavigator xmlNode in xmlSkillNode.SelectAndCacheExpression("groups/skill", token))
                        {
                            SkillGroup objGroup = new SkillGroup(_objCharacter);
                            objGroup.LoadFromHeroLab(xmlNode, token: token);
                            SkillGroups.AddWithSort(objGroup, CompareSkillGroups,
                                                    (objExistingSkillGroup, objNewSkillGroup) =>
                                                    {
                                                        foreach (Skill x in objExistingSkillGroup.SkillList
                                                                     .Where(x => !objExistingSkillGroup
                                                                                .SkillList.Contains(x)))
                                                            objExistingSkillGroup.Add(x);
                                                        objNewSkillGroup.Dispose();
                                                    }, token);
                        }

                        //Timekeeper.Finish("load_char_skills_groups");
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_skills", parentActivity))
                    {
                        List<Skill> lstTempSkillList = new List<Skill>(Skills.Count);
                        foreach (XPathNavigator xmlNode in xmlSkillNode.SelectAndCacheExpression("active/skill", token))
                        {
                            Skill objSkill = Skill.LoadFromHeroLab(_objCharacter, xmlNode, false, token: token);
                            if (objSkill != null)
                                lstTempSkillList.Add(objSkill);
                        }

                        foreach (XPathNavigator xmlNode in xmlSkillNode.SelectAndCacheExpression("knowledge/skill", token))
                        {
                            Skill objSkill = Skill.LoadFromHeroLab(_objCharacter, xmlNode, true, token: token);
                            if (objSkill != null)
                                lstTempSkillList.Add(objSkill);
                        }

                        foreach (XPathNavigator xmlNode in xmlSkillNode.SelectAndCacheExpression("language/skill", token))
                        {
                            Skill objSkill = Skill.LoadFromHeroLab(_objCharacter, xmlNode, true, "Language", token: token);
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
                            token.ThrowIfCancellationRequested();
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
                            token.ThrowIfCancellationRequested();
                            if (objSkill.SkillId != Guid.Empty && !objSkill.IsExoticSkill)
                            {
                                Skill objExistingSkill = Skills.FirstOrDefault(x => x.SkillId == objSkill.SkillId);
                                if (objExistingSkill != null)
                                {
                                    MergeSkills(objExistingSkill, objSkill, token);
                                    continue;
                                }
                            }

                            Skills.AddWithSort(objSkill, CompareSkills, (x, y) => MergeSkills(x, y, token), token);
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
                                token.ThrowIfCancellationRequested();
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
                                if (intSkillPointCount > 0 && Skills.Any(x => x.Karma != 0, token))
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
                            KnowledgeSkill objKnowledgeSkillToPutPointsInto;

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
                                                || (intLoopTotalKarmaCost
                                                    == intKnowledgeSkillToPutPointsIntoTotalKarmaCost
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
                            if (intKnowledgeSkillPointCount > 0 && KnowledgeSkills.Any(x => x.Karma != 0, token))
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
                finally
                {
                    Interlocked.Decrement(ref _intLoading);
                }
            }
        }

        private void UpdateUndoList(XmlDocument xmlSkillOwnerDocument)
        {
            using (LockObject.EnterWriteLock())
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
                                if (x.Rating > 0)
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

                    void UpdateUndoSpecific(IReadOnlyDictionary<string, Guid> map,
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

        private async ValueTask UpdateUndoListAsync(XmlDocument xmlSkillOwnerDocument, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                //Hacky way of converting Expense entries to guid based skill identification
                //specs already did?
                //First create dictionary mapping name=>guid
                LockingDictionary<string, Guid> dicGroups = new LockingDictionary<string, Guid>();
                try
                {
                    LockingDictionary<string, Guid> dicSkills = new LockingDictionary<string, Guid>();
                    try
                    {
                        // Potentially expensive checks that can (and therefore should) be parallelized. Normally, this would just be a Parallel.Invoke,
                        // but we want to allow UI messages to happen, just in case this is called on the Main Thread and another thread wants to show a message box.
                        // Not using async-await because this is trivial code and I do not want to infect everything that calls this with async as well.
                        await Task.WhenAll(
                                Task.Run(() => SkillGroups.ForEachParallelAsync(async x =>
                                {
                                    // ReSharper disable once AccessToDisposedClosure
                                    if (await x.GetRatingAsync(token).ConfigureAwait(false) > 0)
                                        // ReSharper disable once AccessToDisposedClosure
                                        await dicGroups.TryAddAsync(x.Name, x.Id, token).ConfigureAwait(false);
                                }, token: token), token),
                                Task.Run(() => Skills.ForEachParallelAsync(async x =>
                                {
                                    if (await x.GetTotalBaseRatingAsync(token).ConfigureAwait(false) > 0)
                                        // ReSharper disable once AccessToDisposedClosure
                                        await dicSkills.TryAddAsync(x.Name, x.Id, token).ConfigureAwait(false);
                                }, token: token), token),
                                // ReSharper disable once AccessToDisposedClosure
                                Task.Run(
                                    () => KnowledgeSkills.ForEachParallelAsync(
                                        x => dicSkills.TryAddAsync(x.Name, x.Id, token).AsTask(), token: token), token))
                            .ConfigureAwait(false);
                        UpdateUndoSpecific(
                            dicSkills,
                            EnumerableExtensions.ToEnumerable(KarmaExpenseType.AddSkill,
                                KarmaExpenseType.ImproveSkill));
                        UpdateUndoSpecific(dicGroups, KarmaExpenseType.ImproveSkillGroup.Yield());

                        void UpdateUndoSpecific(IReadOnlyDictionary<string, Guid> map,
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
                    finally
                    {
                        await dicSkills.DisposeAsync().ConfigureAwait(false);
                    }
                }
                finally
                {
                    await dicGroups.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        internal void Save(XmlWriter objWriter)
        {
            using (LockObject.EnterReadLock())
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

        internal void Reset(bool blnFirstTime = false, CancellationToken token = default)
        {
            using (LockObject.EnterWriteLock(token))
            {
                Interlocked.Increment(ref _intLoading);
                try
                {
                    _dicSkillBackups.ForEach(x => x.Value.Remove(), token);
                    _lstSkills.ForEach(x => x.Remove(), token);
                    KnowledgeSkills.ForEach(x =>
                    {
                        using (x.LockObject.EnterWriteLock(token))
                            x.PropertyChanged -= OnKnowledgeSkillPropertyChanged;
                        x.Remove();
                    }, token);
                    SkillGroups.ForEach(x => x.Dispose(), token);
                    _dicSkillBackups.Clear();
                    _dicSkills.Clear();
                    _lstSkills.Clear();
                    KnowledgeSkills.Clear();
                    KnowsoftSkills.Clear();
                    SkillGroups.Clear();
                    SkillPointsMaximum = 0;
                    SkillGroupPointsMaximum = 0;
                    _blnSkillsInitialized = false;
                }
                finally
                {
                    if (blnFirstTime)
                        Interlocked.Add(ref _intLoading, -2);
                    else
                        Interlocked.Decrement(ref _intLoading);
                }
            }
        }

        internal async ValueTask ResetAsync(bool blnFirstTime = false, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                Interlocked.Increment(ref _intLoading);
                try
                {
                    await _dicSkillBackups.ForEachAsync(x => x.Value.DisposeAsync().AsTask(), token).ConfigureAwait(false);
                    await _lstSkills.ForEachAsync(x => x.RemoveAsync(token).AsTask(), token).ConfigureAwait(false);
                    await KnowledgeSkills.ForEachAsync(async objSkill =>
                    {
                        IAsyncDisposable objLocker2
                            = await objSkill.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                        try
                        {
                            token.ThrowIfCancellationRequested();
                            objSkill.PropertyChanged -= OnKnowledgeSkillPropertyChanged;
                        }
                        finally
                        {
                            await objLocker2.DisposeAsync().ConfigureAwait(false);
                        }

                        await objSkill.RemoveAsync(token).ConfigureAwait(false);
                    }, token).ConfigureAwait(false);
                    await SkillGroups.ForEachAsync(x => x.DisposeAsync().AsTask(), token).ConfigureAwait(false);
                    await _dicSkillBackups.ClearAsync(token).ConfigureAwait(false);
                    await _dicSkills.ClearAsync(token).ConfigureAwait(false);
                    await _lstSkills.ClearAsync(token).ConfigureAwait(false);
                    await KnowledgeSkills.ClearAsync(token).ConfigureAwait(false);
                    await KnowsoftSkills.ClearAsync(token).ConfigureAwait(false);
                    await SkillGroups.ClearAsync(token).ConfigureAwait(false);
                    SkillPointsMaximum = 0;
                    SkillGroupPointsMaximum = 0;
                    _blnSkillsInitialized = false;
                }
                finally
                {
                    if (blnFirstTime)
                        Interlocked.Add(ref _intLoading, -2);
                    else
                        Interlocked.Decrement(ref _intLoading);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private bool _blnSkillsInitialized;
        private readonly AsyncFriendlyReaderWriterLock _objSkillsInitializerLock = new AsyncFriendlyReaderWriterLock();
        private readonly ThreadSafeBindingList<Skill> _lstSkills = new ThreadSafeBindingList<Skill>();
        private readonly LockingDictionary<string, Skill> _dicSkills = new LockingDictionary<string, Skill>();

        /// <summary>
        /// Active Skills
        /// </summary>
        public ThreadSafeBindingList<Skill> Skills
        {
            get
            {
                using (LockObject.EnterReadLock())
                using (_objSkillsInitializerLock.EnterReadLock())
                {
                    if (!_blnSkillsInitialized && _objCharacter.SkillsSection == this)
                    {
                        using (_objSkillsInitializerLock.EnterWriteLock())
                        {
                            if (!_blnSkillsInitialized
                                && _objCharacter.SkillsSection
                                == this) // repeat check to avoid redoing calculations if another thread read Skills before first one acquired write lock
                            {
                                XmlDocument xmlSkillsDocument = _objCharacter.LoadData("skills.xml");
                                using (XmlNodeList xmlSkillList = xmlSkillsDocument
                                           .SelectNodes("/chummer/skills/skill[not(exotic = 'True') and ("
                                                        + _objCharacter.Settings.BookXPath() + ')'
                                                        + SkillFilter(FilterOption.NonSpecial) + ']'))
                                {
                                    if (xmlSkillList?.Count > 0)
                                    {
                                        foreach (XmlNode xmlSkill in xmlSkillList)
                                        {
                                            bool blnIsKnowledgeSkill
                                                = xmlSkillsDocument
                                                  .SelectSingleNodeAndCacheExpressionAsNavigator("/chummer/categories/category[. = "
                                                                    + xmlSkill["category"]?.InnerText.CleanXPath()
                                                                    + "]/@type")
                                                  ?.Value
                                                  != "active";
                                            Skill objSkill = Skill.FromData(xmlSkill, _objCharacter,
                                                                            blnIsKnowledgeSkill);
                                            string strKey = objSkill.DictionaryKey;
                                            if (_dicSkills.TryAdd(strKey, objSkill))
                                                _lstSkills.Add(objSkill);
                                            else if (_dicSkills.TryGetValue(strKey, out Skill objExistingSkill))
                                                MergeSkills(objExistingSkill, objSkill);
                                            else
                                                Utils.BreakIfDebug();
                                        }
                                    }
                                }
                                _lstSkills.Sort(CompareSkills);

                                _blnSkillsInitialized = true;
                            }
                        }
                    }

                    return _lstSkills;
                }
            }
        }

        /// <summary>
        /// Active Skills
        /// </summary>
        public async ValueTask<ThreadSafeBindingList<Skill>> GetSkillsAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            using (await _objSkillsInitializerLock.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                if (!_blnSkillsInitialized
                    && await _objCharacter.GetSkillsSectionAsync(token).ConfigureAwait(false) == this)
                {
                    IAsyncDisposable objLocker
                        = await _objSkillsInitializerLock.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        if (!_blnSkillsInitialized
                            && await _objCharacter.GetSkillsSectionAsync(token).ConfigureAwait(false)
                            == this) // repeat check to avoid redoing calculations if another thread read Skills before first one acquired write lock
                        {
                            XmlDocument xmlSkillsDocument = await _objCharacter
                                                                  .LoadDataAsync("skills.xml", token: token)
                                                                  .ConfigureAwait(false);
                            using (XmlNodeList xmlSkillList = xmlSkillsDocument
                                       .SelectNodes("/chummer/skills/skill[not(exotic = 'True') and ("
                                                    + await (await _objCharacter.GetSettingsAsync(token)
                                                            .ConfigureAwait(false)).BookXPathAsync(token: token)
                                                        .ConfigureAwait(false) + ')'
                                                    + SkillFilter(FilterOption.NonSpecial) + ']'))
                            {
                                if (xmlSkillList?.Count > 0)
                                {
                                    foreach (XmlNode xmlSkill in xmlSkillList)
                                    {
                                        bool blnIsKnowledgeSkill
                                            = (await xmlSkillsDocument
                                                  .SelectSingleNodeAndCacheExpressionAsNavigatorAsync("/chummer/categories/category[. = "
                                                      + xmlSkill["category"]?.InnerText.CleanXPath()
                                                      + "]/@type", token).ConfigureAwait(false))
                                              ?.Value
                                              != "active";
                                        Skill objSkill = Skill.FromData(xmlSkill, _objCharacter,
                                                                        blnIsKnowledgeSkill);
                                        string strKey = await objSkill.GetDictionaryKeyAsync(token).ConfigureAwait(false);
                                        if (await _dicSkills.TryAddAsync(strKey, objSkill, token).ConfigureAwait(false))
                                        {
                                            await _lstSkills.AddAsync(objSkill, token).ConfigureAwait(false);
                                        }
                                        else
                                        {
                                            (bool blnSuccess, Skill objExistingSkill) =
                                                await _dicSkills.TryGetValueAsync(strKey,
                                                    token).ConfigureAwait(false);
                                            if (blnSuccess)
                                                await MergeSkillsAsync(objExistingSkill,
                                                    objSkill, token).ConfigureAwait(false);
                                            else
                                                Utils.BreakIfDebug();
                                        }
                                    }
                                }
                            }

                            await _lstSkills.SortAsync(CompareSkills, token).ConfigureAwait(false);

                            _blnSkillsInitialized = true;
                        }
                    }
                    finally
                    {
                        await objLocker.DisposeAsync().ConfigureAwait(false);
                    }
                }

                return _lstSkills;
            }
        }

        /// <summary>
        /// Checks if the character has an active skill with a given name.
        /// </summary>
        /// <param name="strSkillKey">Name of the skill. For exotic skills, this is slightly different, refer to a Skill's DictionaryKey property for more info.</param>
        public bool HasActiveSkill(string strSkillKey)
        {
            using (LockObject.EnterReadLock())
                return _dicSkills.ContainsKey(strSkillKey);
        }

        /// <summary>
        /// Checks if the character has an active skill with a given name.
        /// </summary>
        /// <param name="strSkillKey">Name of the skill. For exotic skills, this is slightly different, refer to a Skill's DictionaryKey property for more info.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async ValueTask<bool> HasActiveSkillAsync(string strSkillKey, CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
                return await _dicSkills.ContainsKeyAsync(strSkillKey, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets an active skill by its Name. Returns null if none found.
        /// </summary>
        /// <param name="strSkillName">Name of the skill.</param>
        /// <param name="token">CancellationToken to listen to.</param>
        public Skill GetActiveSkill(string strSkillName, CancellationToken token = default)
        {
            using (LockObject.EnterReadLock(token))
            {
                _dicSkills.TryGetValue(strSkillName, out Skill objReturn, token);
                return objReturn;
            }
        }

        /// <summary>
        /// Gets an active skill by its Name. Returns null if none found.
        /// </summary>
        /// <param name="strSkillName">Name of the skill.</param>
        /// <param name="token">CancellationToken to listen to.</param>
        public async ValueTask<Skill> GetActiveSkillAsync(string strSkillName, CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                return (await _dicSkills.TryGetValueAsync(strSkillName, token).ConfigureAwait(false)).Item2;
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
                using (LockObject.EnterReadLock())
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

        private readonly ThreadSafeBindingList<KnowledgeSkill> _lstKnowledgeSkills = new ThreadSafeBindingList<KnowledgeSkill>();
        private readonly ThreadSafeBindingList<KnowledgeSkill> _lstKnowsoftSkills = new ThreadSafeBindingList<KnowledgeSkill>();
        private readonly ThreadSafeBindingList<SkillGroup> _lstSkillGroups = new ThreadSafeBindingList<SkillGroup>();

        public ThreadSafeBindingList<KnowledgeSkill> KnowledgeSkills
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _lstKnowledgeSkills;
            }
        }

        public async ValueTask<ThreadSafeBindingList<KnowledgeSkill>> GetKnowledgeSkillsAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
                return _lstKnowledgeSkills;
        }

        /// <summary>
        /// KnowsoftSkills.
        /// </summary>
        public ThreadSafeBindingList<KnowledgeSkill> KnowsoftSkills
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _lstKnowsoftSkills;
            }
        }

        /// <summary>
        /// KnowsoftSkills.
        /// </summary>
        public async ValueTask<ThreadSafeBindingList<KnowledgeSkill>> GetKnowsoftSkillsAsync(
            CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
                return _lstKnowsoftSkills;
        }

        /// <summary>
        /// Skill Groups.
        /// </summary>
        public ThreadSafeBindingList<SkillGroup> SkillGroups
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _lstSkillGroups;
            }
        }

        /// <summary>
        /// Skill Groups.
        /// </summary>
        public async ValueTask<ThreadSafeBindingList<SkillGroup>> GetSkillGroupsAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
                return _lstSkillGroups;
        }

        public bool HasKnowledgePoints => KnowledgeSkillPoints > 0;

        public async ValueTask<bool> GetHasKnowledgePointsAsync(CancellationToken token = default) =>
            await GetKnowledgeSkillPointsAsync(token).ConfigureAwait(false) > 0;

        public bool HasAvailableNativeLanguageSlots
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return KnowledgeSkills.Count(x => x.IsNativeLanguage) < 1
                        + ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.NativeLanguageLimit);
            }
        }

        public async ValueTask<bool> GetHasAvailableNativeLanguageSlotsAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
                return await (await GetKnowledgeSkillsAsync(token).ConfigureAwait(false))
                             .CountAsync(x => x.GetIsNativeLanguageAsync(token).AsTask(), token).ConfigureAwait(false)
                       < 1 + await ImprovementManager
                               .ValueOfAsync(_objCharacter, Improvement.ImprovementType.NativeLanguageLimit,
                                             token: token)
                               .ConfigureAwait(false);
        }

        private int _intCachedKnowledgePoints = int.MinValue;

        private readonly AsyncFriendlyReaderWriterLock _objCachedKnowledgePointsLock
            = new AsyncFriendlyReaderWriterLock();

        /// <summary>
        /// Number of free Knowledge Skill Points the character has.
        /// </summary>
        public int KnowledgeSkillPoints
        {
            get
            {
                using (LockObject.EnterReadLock())
                using (_objCachedKnowledgePointsLock.EnterReadLock())
                {
                    if (_intCachedKnowledgePoints == int.MinValue)
                    {
                        using (_objCachedKnowledgePointsLock.EnterWriteLock())
                        {
                            if (_intCachedKnowledgePoints == int.MinValue) // Just in case
                            {
                                string strExpression = _objCharacter.Settings.KnowledgePointsExpression;
                                if (strExpression.IndexOfAny('{', '+', '-', '*', ',') != -1
                                    || strExpression.Contains("div"))
                                {
                                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                               out StringBuilder sbdValue))
                                    {
                                        sbdValue.Append(strExpression);
                                        _objCharacter.AttributeSection
                                                     .ProcessAttributesInXPath(sbdValue, strExpression);

                                        // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                                        (bool blnIsSuccess, object objProcess)
                                            = CommonFunctions.EvaluateInvariantXPath(
                                                sbdValue.ToString());
                                        _intCachedKnowledgePoints
                                            = blnIsSuccess ? ((double) objProcess).StandardRound() : 0;
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
                        }
                    }

                    return _intCachedKnowledgePoints;
                }
            }
        }

        /// <summary>
        /// Number of free Knowledge Skill Points the character has.
        /// </summary>
        public async ValueTask<int> GetKnowledgeSkillPointsAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            using (await _objCachedKnowledgePointsLock.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                if (_intCachedKnowledgePoints == int.MinValue)
                {
                    IAsyncDisposable objLocker = await _objCachedKnowledgePointsLock.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        if (_intCachedKnowledgePoints == int.MinValue) // Just in case
                        {
                            string strExpression = _objCharacter.Settings.KnowledgePointsExpression;
                            if (strExpression.IndexOfAny('{', '+', '-', '*', ',') != -1
                                || strExpression.Contains("div"))
                            {
                                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                              out StringBuilder sbdValue))
                                {
                                    sbdValue.Append(strExpression);
                                    await _objCharacter.AttributeSection.ProcessAttributesInXPathAsync(
                                        sbdValue, strExpression, token: token).ConfigureAwait(false);

                                    // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                                    (bool blnIsSuccess, object objProcess)
                                        = await CommonFunctions.EvaluateInvariantXPathAsync(
                                            sbdValue.ToString(), token).ConfigureAwait(false);
                                    _intCachedKnowledgePoints
                                        = blnIsSuccess ? ((double) objProcess).StandardRound() : 0;
                                }
                            }
                            else
                                int.TryParse(strExpression, NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                                             out _intCachedKnowledgePoints);

                            _intCachedKnowledgePoints += (await ImprovementManager
                                                                .ValueOfAsync(_objCharacter,
                                                                              Improvement.ImprovementType.FreeKnowledgeSkills, token: token).ConfigureAwait(false))
                                .StandardRound();
                        }
                    }
                    finally
                    {
                        await objLocker.DisposeAsync().ConfigureAwait(false);
                    }
                }

                return _intCachedKnowledgePoints;
            }
        }

        /// <summary>
        /// Number of free Knowledge skill points the character have remaining
        /// </summary>
        public int KnowledgeSkillPointsRemain
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return KnowledgeSkillPoints - KnowledgeSkillPointsUsed;
            }
        }

        /// <summary>
        /// Number of free Knowledge skill points the character have remaining
        /// </summary>
        public async ValueTask<int> GetKnowledgeSkillPointsRemainAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
                return await GetKnowledgeSkillPointsAsync(token).ConfigureAwait(false) - await GetKnowledgeSkillPointsUsedAsync(token).ConfigureAwait(false);
        }

        /// <summary>
        /// Number of knowledge skill points the character have used.
        /// </summary>
        public int KnowledgeSkillPointsUsed
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return KnowledgeSkillRanksSum - SkillPointsSpentOnKnoskills;
            }
        }

        /// <summary>
        /// Number of knowledge skill points the character have used.
        /// </summary>
        public async ValueTask<int> GetKnowledgeSkillPointsUsedAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
                return await GetKnowledgeSkillRanksSumAsync(token).ConfigureAwait(false) - await GetSkillPointsSpentOnKnoskillsAsync(token).ConfigureAwait(false);
        }

        /// <summary>
        /// Sum of knowledge skill ranks the character has allocated.
        /// </summary>
        public int KnowledgeSkillRanksSum
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return KnowledgeSkills.Sum(x => x.CurrentSpCost);
            }
        }

        /// <summary>
        /// Sum of knowledge skill ranks the character has allocated.
        /// </summary>
        public async ValueTask<int> GetKnowledgeSkillRanksSumAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
                return await (await GetKnowledgeSkillsAsync(token).ConfigureAwait(false)).SumAsync(x => x.GetCurrentSpCostAsync(token).AsTask(), token).ConfigureAwait(false);
        }

        /// <summary>
        /// Number of Skill Points that have been spent on knowledge skills.
        /// </summary>
        public int SkillPointsSpentOnKnoskills
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    //Even if it is stupid, you can spend real skill points on knoskills...
                    if (!_objCharacter.EffectiveBuildMethodUsesPriorityTables)
                    {
                        return 0;
                    }

                    int intReturn = KnowledgeSkillRanksSum - KnowledgeSkillPoints;
                    return Math.Max(intReturn, 0);
                }
            }
        }

        /// <summary>
        /// Number of Skill Points that have been spent on knowledge skills.
        /// </summary>
        public async ValueTask<int> GetSkillPointsSpentOnKnoskillsAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                //Even if it is stupid, you can spend real skill points on knoskills...
                if (!await _objCharacter.GetEffectiveBuildMethodUsesPriorityTablesAsync(token).ConfigureAwait(false))
                {
                    return 0;
                }

                int intReturn = await GetKnowledgeSkillRanksSumAsync(token).ConfigureAwait(false) - await GetKnowledgeSkillPointsAsync(token).ConfigureAwait(false);
                return Math.Max(intReturn, 0);
            }
        }

        /// <summary>
        /// Number of free Skill Points the character has left.
        /// </summary>
        public int SkillPoints
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    if (SkillPointsMaximum == 0)
                    {
                        return 0;
                    }

                    return SkillPointsMaximum - Skills.Sum(x => x.CurrentSpCost) - SkillPointsSpentOnKnoskills;
                }
            }
        }

        /// <summary>
        /// Number of free Skill Points the character has left.
        /// </summary>
        public async ValueTask<int> GetSkillPointsAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                if (SkillPointsMaximum == 0)
                {
                    return 0;
                }

                return SkillPointsMaximum
                       - await (await GetSkillsAsync(token).ConfigureAwait(false)).SumAsync(x => x.GetCurrentSpCostAsync(token).AsTask(),
                                                                      token).ConfigureAwait(false)
                       - await GetSkillPointsSpentOnKnoskillsAsync(token).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Number of maximum Skill Points the character has.
        /// </summary>
        public int SkillPointsMaximum
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intSkillPointsMaximum;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    // No need to write lock because interlocked guarantees safety
                    if (Interlocked.Exchange(ref _intSkillPointsMaximum, value) == value)
                        return;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Number of free Skill Points the character has.
        /// </summary>
        public int SkillGroupPoints
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return SkillGroupPointsMaximum - SkillGroups.Sum(x => x.Base - x.FreeBase);
            }
        }

        /// <summary>
        /// Number of free Skill Points the character has.
        /// </summary>
        public async ValueTask<int> GetSkillGroupPointsAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
                return SkillGroupPointsMaximum - await SkillGroups.SumAsync(x => x.Base - x.FreeBase, token: token).ConfigureAwait(false);
        }

        /// <summary>
        /// Number of maximum Skill Groups the character has.
        /// </summary>
        public int SkillGroupPointsMaximum
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intSkillGroupPointsMaximum;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    // No need to write lock because interlocked guarantees safety
                    if (Interlocked.Exchange(ref _intSkillGroupPointsMaximum, value) == value)
                        return;
                    OnPropertyChanged();
                }
            }
        }

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
                    ? string.Compare(rhsExoticSkill.CurrentDisplaySpecific,
                        lhsExoticSkill.CurrentDisplaySpecific ?? string.Empty, false,
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

        public static async ValueTask<int> CompareSpecializationsAsync(SkillSpecialization lhs, SkillSpecialization rhs, CancellationToken token = default)
        {
            if (lhs == null)
                return rhs == null ? 0 : 1;
            if (rhs == null)
                return -1;
            Skill objLhsParent = await lhs.GetParentAsync(token).ConfigureAwait(false);
            Skill objRhsParent = await lhs.GetParentAsync(token).ConfigureAwait(false);
            if (objLhsParent != objRhsParent)
                return await CompareSkillsAsync(objRhsParent, objLhsParent, token).ConfigureAwait(false);
            if (lhs.Free != rhs.Free)
                return lhs.Free ? 1 : -1;
            return string.Compare(await lhs.GetCurrentDisplayNameAsync(token).ConfigureAwait(false),
                                  await rhs.GetCurrentDisplayNameAsync(token).ConfigureAwait(false), false,
                                  GlobalSettings.CultureInfo);
        }

        public static async ValueTask<int> CompareSkillsAsync(Skill rhs, Skill lhs, CancellationToken token = default)
        {
            if (rhs == null && lhs == null)
                return 0;
            ExoticSkill lhsExoticSkill = lhs as ExoticSkill;
            if (rhs is ExoticSkill rhsExoticSkill)
            {
                return lhsExoticSkill != null
                    ? string.Compare(await rhsExoticSkill.GetCurrentDisplaySpecificAsync(token).ConfigureAwait(false),
                                     await lhsExoticSkill.GetCurrentDisplaySpecificAsync(token).ConfigureAwait(false) ?? string.Empty, false,
                                     GlobalSettings.CultureInfo)
                    : 1;
            }
            if (lhsExoticSkill != null)
                return -1;
            return string.Compare(rhs != null ? await rhs.GetCurrentDisplayNameAsync(token).ConfigureAwait(false) : string.Empty,
                                  lhs != null ? await lhs.GetCurrentDisplayNameAsync(token).ConfigureAwait(false) : string.Empty, false,
                                  GlobalSettings.CultureInfo);
        }

        public static async ValueTask<int> CompareSkillGroupsAsync(SkillGroup objXGroup, SkillGroup objYGroup, CancellationToken token = default)
        {
            if (objXGroup == null)
                return objYGroup == null ? 0 : 1;
            if (objYGroup == null)
                return -1;
            return string.Compare(await objXGroup.GetCurrentDisplayNameAsync(token).ConfigureAwait(false),
                                  await objYGroup.GetCurrentDisplayNameAsync(token).ConfigureAwait(false), false,
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

        private static void MergeSkills(Skill objExistingSkill, Skill objNewSkill, CancellationToken token = default)
        {
            using (objNewSkill.LockObject.EnterReadLock(token))
            using (objExistingSkill.LockObject.EnterReadLock(token))
            {
                objExistingSkill.CopyInternalId(objNewSkill);
                if (objNewSkill.BasePoints > objExistingSkill.BasePoints)
                    objExistingSkill.BasePoints = objNewSkill.BasePoints;
                if (objNewSkill.KarmaPoints > objExistingSkill.KarmaPoints)
                    objExistingSkill.KarmaPoints = objNewSkill.KarmaPoints;
                objExistingSkill.BuyWithKarma = objNewSkill.BuyWithKarma;
                objExistingSkill.Notes += objNewSkill.Notes;
                objExistingSkill.NotesColor = objNewSkill.NotesColor;
                objExistingSkill.Specializations.AddRangeWithSort(objNewSkill.Specializations, CompareSpecializations, token: token);
            }
            objNewSkill.Remove();
        }

        private static async Task MergeSkillsAsync(Skill objExistingSkill, Skill objNewSkill, CancellationToken token = default)
        {
            using (await objNewSkill.LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            using (await objExistingSkill.LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                objExistingSkill.CopyInternalId(objNewSkill);
                int intExistingBasePoints = await objExistingSkill.GetBasePointsAsync(token).ConfigureAwait(false);
                int intNewBasePoints = await objNewSkill.GetBasePointsAsync(token).ConfigureAwait(false);
                if (intExistingBasePoints < intNewBasePoints)
                    await objExistingSkill.SetBasePointsAsync(intNewBasePoints, token).ConfigureAwait(false);
                int intExistingKarmaPoints = await objExistingSkill.GetKarmaPointsAsync(token).ConfigureAwait(false);
                int intNewKarmaPoints = await objNewSkill.GetKarmaPointsAsync(token).ConfigureAwait(false);
                if (intExistingKarmaPoints < intNewKarmaPoints)
                    await objExistingSkill.SetKarmaPointsAsync(intNewKarmaPoints, token).ConfigureAwait(false);
                await objExistingSkill
                      .SetBuyWithKarmaAsync(await objNewSkill.GetBuyWithKarmaAsync(token).ConfigureAwait(false), token)
                      .ConfigureAwait(false);
                objExistingSkill.Notes += objNewSkill.Notes;
                objExistingSkill.NotesColor = objNewSkill.NotesColor;
                await objExistingSkill.Specializations
                                      .AddAsyncRangeWithSortAsync(objNewSkill.Specializations,
                                                                  (x, y) => CompareSpecializationsAsync(x, y, token)
                                                                      .AsTask(),
                                                                  token: token).ConfigureAwait(false);
            }
            await objNewSkill.RemoveAsync(token).ConfigureAwait(false);
        }

        private List<ListItem> _lstDefaultKnowledgeSkills;

        public IReadOnlyList<ListItem> MyDefaultKnowledgeSkills
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    if (GlobalSettings.LiveCustomData || _lstDefaultKnowledgeSkills == null)
                    {
                        if (_lstDefaultKnowledgeSkills == null)
                            _lstDefaultKnowledgeSkills = Utils.ListItemListPool.Get();
                        else
                            _lstDefaultKnowledgeSkills.Clear();
                        XPathNavigator xmlSkillsDocument = _objCharacter.LoadDataXPath("skills.xml");
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
        private int _intSkillGroupPointsMaximum;
        private int _intSkillPointsMaximum;

        public IReadOnlyList<ListItem> MyKnowledgeTypes
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    if (GlobalSettings.LiveCustomData || _lstKnowledgeTypes == null)
                    {
                        if (_lstKnowledgeTypes == null)
                            _lstKnowledgeTypes = Utils.ListItemListPool.Get();
                        else
                            _lstKnowledgeTypes.Clear();
                        XPathNavigator xmlSkillsDocument = _objCharacter.LoadDataXPath("skills.xml");
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

        internal bool IsLoading => _intLoading > 0;

        internal void ForcePropertyChangedNotificationAll(string strName)
        {
            using (LockObject.EnterReadLock())
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

        public async ValueTask Print(XmlWriter objWriter, CultureInfo objCulture, string strLanguageToPrint, CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                foreach (Skill objSkill in Skills)
                {
                    if ((GlobalSettings.PrintSkillsWithZeroRating || objSkill.Rating > 0) && objSkill.Enabled)
                    {
                        await objSkill.Print(objWriter, objCulture, strLanguageToPrint, token).ConfigureAwait(false);
                    }
                }

                foreach (SkillGroup objSkillGroup in SkillGroups)
                {
                    if (objSkillGroup.Rating > 0)
                    {
                        await objSkillGroup.Print(objWriter, objCulture, strLanguageToPrint, token).ConfigureAwait(false);
                    }
                }

                foreach (KnowledgeSkill objSkill in KnowledgeSkills)
                {
                    await objSkill.Print(objWriter, objCulture, strLanguageToPrint, token).ConfigureAwait(false);
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
            using (LockObject.EnterReadLock())
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
            using (LockObject.EnterReadLock())
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

        #endregion XPath Processing

        /// <inheritdoc />
        public void Dispose()
        {
            using (LockObject.EnterWriteLock())
            {
                try
                {
                    using (_objCharacter.LockObject.EnterWriteLock())
                        _objCharacter.PropertyChanged -= OnCharacterPropertyChanged;
                }
                catch (ObjectDisposedException)
                {
                    //swallow this
                }

                try
                {
                    using (_objCharacter.Settings.LockObject.EnterWriteLock())
                        _objCharacter.Settings.PropertyChanged -= OnCharacterSettingsPropertyChanged;
                }
                catch (ObjectDisposedException)
                {
                    //swallow this
                }
                _lstSkillGroups.ForEach(x => x.Dispose());
                _dicSkillBackups.ForEach(x => x.Value.Dispose());
                _dicSkillBackups.Clear();
                _lstSkills.ForEach(x => x.Dispose());
                _lstSkills.Dispose();
                _lstKnowledgeSkills.ForEach(x => x.Dispose());
                _lstKnowledgeSkills.Dispose();
                _lstKnowsoftSkills.Clear();
                _lstKnowsoftSkills.Dispose();
                _lstSkillGroups.Dispose();
                _dicSkills.Dispose();
                _dicSkillBackups.Dispose();
                _objSkillsInitializerLock.Dispose();
                _objCachedKnowledgePointsLock.Dispose();
                if (_lstDefaultKnowledgeSkills != null)
                    Utils.ListItemListPool.Return(ref _lstDefaultKnowledgeSkills);
                if (_lstKnowledgeTypes != null)
                    Utils.ListItemListPool.Return(ref _lstKnowledgeTypes);
            }
            LockObject.Dispose();
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                try
                {
                    IAsyncDisposable objLocker2
                        = await _objCharacter.LockObject.EnterWriteLockAsync().ConfigureAwait(false);
                    try
                    {
                        _objCharacter.PropertyChanged -= OnCharacterPropertyChanged;
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }
                }
                catch (ObjectDisposedException)
                {
                    //swallow this
                }

                try
                {
                    IAsyncDisposable objLocker2 = await _objCharacter.Settings.LockObject.EnterWriteLockAsync()
                                                                     .ConfigureAwait(false);
                    try
                    {
                        _objCharacter.Settings.PropertyChanged -= OnCharacterSettingsPropertyChanged;
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }
                }
                catch (ObjectDisposedException)
                {
                    //swallow this
                }
                await _lstSkillGroups.ForEachAsync(x => x.DisposeAsync().AsTask()).ConfigureAwait(false);
                await _dicSkillBackups.ForEachAsync(x => x.Value.DisposeAsync().AsTask()).ConfigureAwait(false);
                await _dicSkillBackups.ClearAsync().ConfigureAwait(false);
                await _lstSkills.ForEachAsync(x => x.DisposeAsync().AsTask()).ConfigureAwait(false);
                await _lstSkills.DisposeAsync().ConfigureAwait(false);
                await _lstKnowledgeSkills.ForEachAsync(x => x.DisposeAsync().AsTask()).ConfigureAwait(false);
                await _lstKnowledgeSkills.DisposeAsync().ConfigureAwait(false);
                await _lstKnowsoftSkills.ClearAsync().ConfigureAwait(false);
                await _lstKnowsoftSkills.DisposeAsync().ConfigureAwait(false);
                await _lstSkillGroups.DisposeAsync().ConfigureAwait(false);
                await _dicSkills.DisposeAsync().ConfigureAwait(false);
                await _dicSkillBackups.DisposeAsync().ConfigureAwait(false);
                await _objSkillsInitializerLock.DisposeAsync().ConfigureAwait(false);
                await _objCachedKnowledgePointsLock.DisposeAsync().ConfigureAwait(false);
                if (_lstDefaultKnowledgeSkills != null)
                    Utils.ListItemListPool.Return(ref _lstDefaultKnowledgeSkills);
                if (_lstKnowledgeTypes != null)
                    Utils.ListItemListPool.Return(ref _lstKnowledgeTypes);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            await LockObject.DisposeAsync().ConfigureAwait(false);
        }

        public AsyncFriendlyReaderWriterLock LockObject { get; } = new AsyncFriendlyReaderWriterLock();
    }
}
