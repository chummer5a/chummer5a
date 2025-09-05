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
using System.Collections.ObjectModel;
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
    public sealed class SkillsSection : INotifyMultiplePropertiesChangedAsync, IHasLockObject, IHasCharacterObject
    {
        private int _intLoading = 1;
        private readonly Character _objCharacter;
        private readonly ConcurrentDictionary<Guid, Skill> _dicSkillBackups = new ConcurrentDictionary<Guid, Skill>();

        public Character CharacterObject => _objCharacter; // readonly member, no locking needed

        public SkillsSection(Character objCharacter)
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            LockObject = objCharacter.LockObject;
            _objKnowledgeTypesLock = new AsyncFriendlyReaderWriterLock(LockObject, true);
            _objCachedKnowledgePointsLock = new AsyncFriendlyReaderWriterLock(LockObject, true);
            _objSkillsInitializerLock = new AsyncFriendlyReaderWriterLock(LockObject, true);
            _objKnowledgeSkillCategoriesMapLock = new AsyncFriendlyReaderWriterLock(LockObject, true);
            _objDefaultKnowledgeSkillsLock = new AsyncFriendlyReaderWriterLock(LockObject, true);
            _lstSkills = new ThreadSafeBindingList<Skill>(LockObject);
            _lstKnowledgeSkills = new ThreadSafeBindingList<KnowledgeSkill>(LockObject);
            _lstKnowsoftSkills = new ThreadSafeBindingList<KnowledgeSkill>(LockObject);
            _lstSkillGroups = new ThreadSafeBindingList<SkillGroup>(LockObject);
            objCharacter.PropertyChangedAsync += OnCharacterPropertyChanged;
            CharacterSettings objSettings = objCharacter.Settings;
            if (objSettings?.IsDisposed == false)
                objSettings.MultiplePropertiesChangedAsync += OnCharacterSettingsPropertyChanged;
            SkillGroups.BeforeRemoveAsync += SkillGroupsOnBeforeRemove;
            KnowsoftSkills.BeforeRemoveAsync += KnowsoftSkillsOnBeforeRemove;
            KnowledgeSkills.BeforeRemoveAsync += KnowledgeSkillsOnBeforeRemove;
            KnowledgeSkills.ListChangedAsync += KnowledgeSkillsOnListChanged;
            Skills.BeforeRemoveAsync += SkillsOnBeforeRemove;
            Skills.ListChangedAsync += SkillsOnListChanged;
        }

        private async Task SkillGroupsOnBeforeRemove(object sender, RemovingOldEventArgs e,
            CancellationToken token = default)
        {
            if (_intLoading > 0)
                return;
            token.ThrowIfCancellationRequested();
            SkillGroup objSkill = await SkillGroups.GetValueAtAsync(e.OldIndex, token).ConfigureAwait(false);
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                await objSkill.DisposeAsync().ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async Task SkillsOnBeforeRemove(object sender, RemovingOldEventArgs e,
            CancellationToken token = default)
        {
            if (_intLoading > 0)
                return;
            token.ThrowIfCancellationRequested();
            Skill objSkill = await Skills.GetValueAtAsync(e.OldIndex, token).ConfigureAwait(false);
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _dicSkills.TryRemove(await objSkill.GetDictionaryKeyAsync(token).ConfigureAwait(false), out _);
                if (_dicSkillBackups.All(x => x.Value != objSkill)) // Do not use Values collection to avoid race conditions
                    await objSkill.RemoveAsync(token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async Task KnowledgeSkillsOnBeforeRemove(object sender, RemovingOldEventArgs e,
            CancellationToken token = default)
        {
            if (_intLoading > 0)
                return;
            token.ThrowIfCancellationRequested();
            KnowledgeSkill objSkill =
                    await KnowledgeSkills.GetValueAtAsync(e.OldIndex, token).ConfigureAwait(false);
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                objSkill.MultiplePropertiesChangedAsync -= OnKnowledgeSkillPropertyChanged;

                if (_dicSkillBackups.All(x => x.Value != objSkill) // Do not use Values collection to avoid race conditions
                    && !await KnowsoftSkills.ContainsAsync(objSkill, token).ConfigureAwait(false))
                    await objSkill.RemoveAsync(token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async Task KnowsoftSkillsOnBeforeRemove(object sender, RemovingOldEventArgs e,
            CancellationToken token = default)
        {
            if (_intLoading > 0)
                return;
            token.ThrowIfCancellationRequested();
            KnowledgeSkill objSkill =
                    await KnowsoftSkills.GetValueAtAsync(e.OldIndex, token).ConfigureAwait(false);
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_dicSkillBackups.All(x => x.Value != objSkill) // Do not use Values collection to avoid race conditions
                    && !await KnowledgeSkills.ContainsAsync(objSkill, token).ConfigureAwait(false))
                    await objSkill.RemoveAsync(token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async Task SkillsOnListChanged(object sender, ListChangedEventArgs e, CancellationToken token = default)
        {
            if (_intLoading > 0)
                return;
            token.ThrowIfCancellationRequested();
            switch (e.ListChangedType)
            {
                case ListChangedType.Reset:
                {
                    token.ThrowIfCancellationRequested();
                    IAsyncDisposable objLocker =
                        await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        _dicSkills.Clear();
                        await _lstSkills.ForEachAsync(async objSkill =>
                        {
                            string strLoop = await objSkill.GetDictionaryKeyAsync(token).ConfigureAwait(false);
                            _dicSkills.TryAdd(strLoop, objSkill);
                        }, token: token).ConfigureAwait(false);
                    }
                    finally
                    {
                        await objLocker.DisposeAsync().ConfigureAwait(false);
                    }

                    break;
                }
                case ListChangedType.ItemAdded:
                {
                    token.ThrowIfCancellationRequested();
                    IAsyncDisposable objLocker =
                        await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        Skill objNewSkill = await _lstSkills.GetValueAtAsync(e.NewIndex, token)
                            .ConfigureAwait(false);
                        string strLoop = await objNewSkill.GetDictionaryKeyAsync(token).ConfigureAwait(false);
                        _dicSkills.TryAdd(strLoop, objNewSkill);
                    }
                    finally
                    {
                        await objLocker.DisposeAsync().ConfigureAwait(false);
                    }

                    break;
                }
            }
        }

        private async Task KnowledgeSkillsOnListChanged(object sender, ListChangedEventArgs e,
            CancellationToken token = default)
        {
            if (_intLoading > 0)
                return;
            token.ThrowIfCancellationRequested();
            switch (e.ListChangedType)
            {
                case ListChangedType.Reset:
                    await KnowledgeSkills
                        .ForEachAsync(x => x.MultiplePropertiesChangedAsync += OnKnowledgeSkillPropertyChanged, token: token)
                        .ConfigureAwait(false);

                    await this.OnMultiplePropertyChangedAsync(token, nameof(KnowledgeSkillRanksSum),
                        nameof(HasAvailableNativeLanguageSlots)).ConfigureAwait(false);
                    break;
                case ListChangedType.ItemAdded:
                    KnowledgeSkill objKnoSkill =
                        await KnowledgeSkills.GetValueAtAsync(e.NewIndex, token).ConfigureAwait(false);
                    if (objKnoSkill != null)
                        objKnoSkill.MultiplePropertiesChangedAsync += OnKnowledgeSkillPropertyChanged;

                    await this.OnMultiplePropertyChangedAsync(token, nameof(KnowledgeSkillRanksSum),
                        nameof(HasAvailableNativeLanguageSlots)).ConfigureAwait(false);
                    break;
                case ListChangedType.ItemDeleted:
                    await this.OnMultiplePropertyChangedAsync(token, nameof(KnowledgeSkillRanksSum),
                        nameof(HasAvailableNativeLanguageSlots)).ConfigureAwait(false);
                    break;
            }
        }

        private Task OnKnowledgeSkillPropertyChanged(object sender, MultiplePropertiesChangedEventArgs e, CancellationToken token = default)
        {
            if (_intLoading > 0)
                return Task.CompletedTask;
            if (e.PropertyNames.Contains(nameof(KnowledgeSkill.CurrentSpCost)))
            {
                return e.PropertyNames.Contains(nameof(KnowledgeSkill.IsNativeLanguage))
                    ? OnMultiplePropertiesChangedAsync(
                        new[] { nameof(KnowledgeSkillRanksSum), nameof(HasAvailableNativeLanguageSlots) }, token)
                    : OnPropertyChangedAsync(nameof(KnowledgeSkillRanksSum), token);
            }

            return e.PropertyNames.Contains(nameof(KnowledgeSkill.IsNativeLanguage))
                ? OnPropertyChangedAsync(nameof(HasAvailableNativeLanguageSlots), token)
                : Task.CompletedTask;
        }

        private Task OnCharacterPropertyChanged(object sender, PropertyChangedEventArgs e, CancellationToken token = default)
        {
            if (_intLoading > 0)
                return Task.CompletedTask;
            return e?.PropertyName == nameof(Character.EffectiveBuildMethodUsesPriorityTables)
                ? OnPropertyChangedAsync(nameof(SkillPointsSpentOnKnoskills), token)
                : Task.CompletedTask;
        }

        private async Task OnCharacterSettingsPropertyChanged(object sender, MultiplePropertiesChangedEventArgs e,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (_intLoading > 0)
                return;
            if (e.PropertyNames.Contains(nameof(CharacterSettings.KnowledgePointsExpression)))
            {
                await OnPropertyChangedAsync(nameof(KnowledgeSkillPoints), token).ConfigureAwait(false);
            }

            if (!await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false) &&
                !await _objCharacter.GetIgnoreRulesAsync(token).ConfigureAwait(false))
            {
                if (e.PropertyNames.Contains(nameof(CharacterSettings.MaxSkillRatingCreate)))
                {
                    await Skills.ForEachAsync(
                        objSkill => objSkill.OnPropertyChangedAsync(nameof(Skill.RatingMaximum), token),
                        token: token).ConfigureAwait(false);
                    if (e.PropertyNames.Contains(nameof(CharacterSettings.MaxKnowledgeSkillRatingCreate)))
                    {
                        await KnowledgeSkills
                            .ForEachAsync(
                                objSkill => objSkill.OnPropertyChangedAsync(nameof(Skill.RatingMaximum), token),
                                token: token).ConfigureAwait(false);
                        foreach (KeyValuePair<Guid, Skill> kvpSkill in _dicSkillBackups) // Do not use Values collection to avoid race conditions
                        {
                            await kvpSkill.Value.OnPropertyChangedAsync(nameof(Skill.RatingMaximum), token)
                                .ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        foreach (KeyValuePair<Guid, Skill> kvpSkill in _dicSkillBackups) // Do not use Values collection to avoid race conditions
                        {
                            token.ThrowIfCancellationRequested();
                            Skill objSkill = kvpSkill.Value;
                            if (!objSkill.IsKnowledgeSkill)
                                await objSkill.OnPropertyChangedAsync(nameof(Skill.RatingMaximum), token)
                                    .ConfigureAwait(false);
                        }
                    }
                }
                else if (e.PropertyNames.Contains(nameof(CharacterSettings.MaxKnowledgeSkillRatingCreate)))
                {
                    await KnowledgeSkills
                        .ForEachAsync(
                            objSkill => objSkill.OnPropertyChangedAsync(nameof(Skill.RatingMaximum), token),
                            token: token).ConfigureAwait(false);
                    foreach (KeyValuePair<Guid, Skill> kvpSkill in _dicSkillBackups) // Do not use Values collection to avoid race conditions
                    {
                        token.ThrowIfCancellationRequested();
                        Skill objSkill = kvpSkill.Value;
                        if (objSkill.IsKnowledgeSkill)
                            await objSkill.OnPropertyChangedAsync(nameof(Skill.RatingMaximum), token)
                                .ConfigureAwait(false);
                    }
                }
            }
        }

        [NotifyPropertyChangedInvocator]
        public void OnPropertyChanged([CallerMemberName] string strPropertyName = null)
        {
            this.OnMultiplePropertyChanged(strPropertyName);
        }

        public Task OnPropertyChangedAsync(string strPropertyName, CancellationToken token = default)
        {
            return this.OnMultiplePropertyChangedAsync(token, strPropertyName);
        }

        public void OnMultiplePropertiesChanged(IReadOnlyCollection<string> lstPropertyNames)
        {
            if (IsLoading)
                return;
            using (LockObject.EnterUpgradeableReadLock())
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

                    if (_setMultiplePropertiesChangedAsync.Count > 0)
                    {
                        MultiplePropertiesChangedEventArgs objArgs =
                            new MultiplePropertiesChangedEventArgs(setNamesOfChangedProperties.ToArray());
                        List<Func<Task>> lstFuncs = new List<Func<Task>>(_setMultiplePropertiesChangedAsync.Count);
                        foreach (MultiplePropertiesChangedAsyncEventHandler objEvent in _setMultiplePropertiesChangedAsync)
                        {
                            lstFuncs.Add(() => objEvent.Invoke(this, objArgs));
                        }

                        Utils.RunWithoutThreadLock(lstFuncs);
                        if (MultiplePropertiesChanged != null)
                        {
                            Utils.RunOnMainThread(() =>
                            {
                                // ReSharper disable once AccessToModifiedClosure
                                MultiplePropertiesChanged?.Invoke(this, objArgs);
                            });
                        }
                    }
                    else if (MultiplePropertiesChanged != null)
                    {
                        MultiplePropertiesChangedEventArgs objArgs =
                            new MultiplePropertiesChangedEventArgs(setNamesOfChangedProperties.ToArray());
                        Utils.RunOnMainThread(() =>
                        {
                            // ReSharper disable once AccessToModifiedClosure
                            MultiplePropertiesChanged?.Invoke(this, objArgs);
                        });
                    }

                    if (_setPropertyChangedAsync.Count > 0)
                    {
                        List<PropertyChangedEventArgs> lstArgsList = setNamesOfChangedProperties.Select(x => new PropertyChangedEventArgs(x)).ToList();
                        List<Func<Task>> lstFuncs = new List<Func<Task>>(lstArgsList.Count * _setPropertyChangedAsync.Count);
                        foreach (PropertyChangedAsyncEventHandler objEvent in _setPropertyChangedAsync)
                        {
                            foreach (PropertyChangedEventArgs objArg in lstArgsList)
                                lstFuncs.Add(() => objEvent.Invoke(this, objArg));
                        }

                        Utils.RunWithoutThreadLock(lstFuncs);
                        if (PropertyChanged != null)
                        {
                            Utils.RunOnMainThread(() =>
                            {
                                if (PropertyChanged != null)
                                {
                                    // ReSharper disable once AccessToModifiedClosure
                                    foreach (PropertyChangedEventArgs objArgs in lstArgsList)
                                    {
                                        PropertyChanged.Invoke(this, objArgs);
                                    }
                                }
                            });
                        }
                    }
                    else if (PropertyChanged != null)
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

        public async Task OnMultiplePropertiesChangedAsync(IReadOnlyCollection<string> lstPropertyNames, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (IsLoading)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                HashSet<string> setNamesOfChangedProperties = null;
                try
                {
                    foreach (string strPropertyName in lstPropertyNames)
                    {
                        if (setNamesOfChangedProperties == null)
                            setNamesOfChangedProperties
                                = await s_SkillSectionDependencyGraph.GetWithAllDependentsAsync(this, strPropertyName, true, token).ConfigureAwait(false);
                        else
                        {
                            foreach (string strLoopChangedProperty in await s_SkillSectionDependencyGraph
                                         .GetWithAllDependentsEnumerableAsync(this, strPropertyName, token).ConfigureAwait(false))
                                setNamesOfChangedProperties.Add(strLoopChangedProperty);
                        }
                    }

                    if (setNamesOfChangedProperties == null || setNamesOfChangedProperties.Count == 0)
                        return;

                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        if (setNamesOfChangedProperties.Contains(nameof(KnowledgeSkillPoints)))
                            _intCachedKnowledgePoints = int.MinValue;
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }

                    if (_setMultiplePropertiesChangedAsync.Count > 0)
                    {
                        MultiplePropertiesChangedEventArgs objArgs =
                            new MultiplePropertiesChangedEventArgs(setNamesOfChangedProperties.ToArray());
                        List<Task> lstTasks = new List<Task>(Utils.MaxParallelBatchSize);
                        int i = 0;
                        foreach (MultiplePropertiesChangedAsyncEventHandler objEvent in _setMultiplePropertiesChangedAsync)
                        {
                            lstTasks.Add(objEvent.Invoke(this, objArgs, token));
                            if (++i < Utils.MaxParallelBatchSize)
                                continue;
                            await Task.WhenAll(lstTasks).ConfigureAwait(false);
                            lstTasks.Clear();
                            i = 0;
                        }

                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                        if (MultiplePropertiesChanged != null)
                        {
                            await Utils.RunOnMainThreadAsync(() =>
                            {
                                // ReSharper disable once AccessToModifiedClosure
                                MultiplePropertiesChanged?.Invoke(this, objArgs);
                            }, token: token).ConfigureAwait(false);
                        }
                    }
                    else if (MultiplePropertiesChanged != null)
                    {
                        MultiplePropertiesChangedEventArgs objArgs =
                            new MultiplePropertiesChangedEventArgs(setNamesOfChangedProperties.ToArray());
                        await Utils.RunOnMainThreadAsync(() =>
                        {
                            // ReSharper disable once AccessToModifiedClosure
                            MultiplePropertiesChanged?.Invoke(this, objArgs);
                        }, token: token).ConfigureAwait(false);
                    }

                    if (_setPropertyChangedAsync.Count > 0)
                    {
                        List<PropertyChangedEventArgs> lstArgsList = setNamesOfChangedProperties
                            .Select(x => new PropertyChangedEventArgs(x)).ToList();
                        List<Task> lstTasks = new List<Task>(Utils.MaxParallelBatchSize);
                        int i = 0;
                        foreach (PropertyChangedAsyncEventHandler objEvent in _setPropertyChangedAsync)
                        {
                            foreach (PropertyChangedEventArgs objArg in lstArgsList)
                            {
                                lstTasks.Add(objEvent.Invoke(this, objArg, token));
                                if (++i < Utils.MaxParallelBatchSize)
                                    continue;
                                await Task.WhenAll(lstTasks).ConfigureAwait(false);
                                lstTasks.Clear();
                                i = 0;
                            }
                        }

                        await Task.WhenAll(lstTasks).ConfigureAwait(false);

                        if (PropertyChanged != null)
                        {
                            await Utils.RunOnMainThreadAsync(() =>
                            {
                                if (PropertyChanged != null)
                                {
                                    // ReSharper disable once AccessToModifiedClosure
                                    foreach (PropertyChangedEventArgs objArgs in lstArgsList)
                                    {
                                        PropertyChanged.Invoke(this, objArgs);
                                    }
                                }
                            }, token).ConfigureAwait(false);
                        }
                    }
                    else if (PropertyChanged != null)
                    {
                        await Utils.RunOnMainThreadAsync(() =>
                        {
                            if (PropertyChanged != null)
                            {
                                // ReSharper disable once AccessToModifiedClosure
                                foreach (string strPropertyToChange in setNamesOfChangedProperties)
                                {
                                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(strPropertyToChange));
                                }
                            }
                        }, token).ConfigureAwait(false);
                    }
                }
                finally
                {
                    if (setNamesOfChangedProperties != null)
                        Utils.StringHashSetPool.Return(ref setNamesOfChangedProperties);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private IEnumerable<Tuple<Skill, bool>> GetActiveSkillsFromData(FilterOption eFilterOption,
            bool blnDeleteSkillsFromBackupIfFound = false, string strName = "", CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            // Lock is upgradeable because retrieving a new skill requires creating it, which requires adding it to the character
            // Yes, this is counterintuitive. TODO: Fix this by allowing skills to be loaded in without necessarily adding them to their character first
            using (LockObject.EnterUpgradeableReadLock(token))
            {
                token.ThrowIfCancellationRequested();
                XmlDocument xmlSkillsDocument = _objCharacter.LoadData("skills.xml", token: token);
                using (XmlNodeList xmlSkillList = xmlSkillsDocument
                           .SelectNodes("/chummer/skills/skill[not(exotic = 'True') and (" +
                                        _objCharacter.Settings.BookXPath(token: token)
                                        + ')'
                                        + SkillFilter(eFilterOption, strName) + ']'))
                {
                    if (xmlSkillList?.Count > 0)
                    {
                        foreach (XmlNode xmlSkill in xmlSkillList)
                        {
                            token.ThrowIfCancellationRequested();
                            if (blnDeleteSkillsFromBackupIfFound)
                            {
                                if (!_dicSkillBackups.IsEmpty
                                    && xmlSkill.TryGetField("id", Guid.TryParse, out Guid guiSkillId)
                                    && _dicSkillBackups.TryRemove(guiSkillId, out Skill objSkill)
                                    && objSkill != null)
                                {
                                    yield return new Tuple<Skill, bool>(objSkill, true);
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
                                    yield return new Tuple<Skill, bool>(Skill.FromData(xmlSkill, _objCharacter, blnIsKnowledgeSkill), true);
                                }
                            }
                            else if (!_dicSkillBackups.IsEmpty
                                     && xmlSkill.TryGetField("id", Guid.TryParse, out Guid guiSkillId)
                                     && _dicSkillBackups.TryGetValue(guiSkillId, out Skill objSkill)
                                     && objSkill != null)
                            {
                                yield return new Tuple<Skill, bool>(objSkill, false);
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
                                yield return new Tuple<Skill, bool>(Skill.FromData(xmlSkill, _objCharacter, blnIsKnowledgeSkill), true);
                            }
                        }
                    }
                }
            }
        }

        private async Task<List<Tuple<Skill, bool>>> GetActiveSkillsFromDataAsync(FilterOption eFilterOption, bool blnDeleteSkillsFromBackupIfFound = false, string strName = "", CancellationToken token = default)
        {
            List<Tuple<Skill, bool>> lstReturn;
            XmlDocument xmlSkillsDocument =
                await _objCharacter.LoadDataAsync("skills.xml", token: token).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            // Lock is upgradeable because retrieving a new skill requires creating it, which requires adding it to the character
            // Yes, this is counterintuitive. TODO: Fix this by allowing skills to be loaded in without necessarily adding them to their character first
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                using (XmlNodeList xmlSkillList = xmlSkillsDocument
                           .SelectNodes("/chummer/skills/skill[not(exotic = 'True') and (" +
                                        await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).BookXPathAsync(token: token).ConfigureAwait(false)
                                        + ')'
                                        + SkillFilter(eFilterOption, strName) + ']'))
                {
                    lstReturn = new List<Tuple<Skill, bool>>(xmlSkillList?.Count ?? 0);
                    if (xmlSkillList?.Count > 0)
                    {
                        try
                        {
                            foreach (XmlNode xmlSkill in xmlSkillList)
                            {
                                if (blnDeleteSkillsFromBackupIfFound)
                                {
                                    if (!_dicSkillBackups.IsEmpty
                                        && xmlSkill.TryGetField("id", Guid.TryParse, out Guid guiSkillId) &&
                                        _dicSkillBackups.TryRemove(guiSkillId, out Skill objSkill) && objSkill != null)
                                    {
                                        lstReturn.Add(new Tuple<Skill, bool>(objSkill, true));
                                    }
                                    else
                                    {
                                        bool blnIsKnowledgeSkill
                                            = xmlSkillsDocument
                                                  .SelectSingleNodeAndCacheExpressionAsNavigator(
                                                      "/chummer/categories/category[. = "
                                                      + xmlSkill["category"]?.InnerText.CleanXPath() + "]/@type", token)
                                                  ?.Value
                                              != "active";
                                        lstReturn.Add(new Tuple<Skill, bool>(await Skill
                                            .FromDataAsync(xmlSkill, _objCharacter, blnIsKnowledgeSkill, token)
                                            .ConfigureAwait(false), true));
                                    }
                                }
                                else if (!_dicSkillBackups.IsEmpty
                                         && xmlSkill.TryGetField("id", Guid.TryParse, out Guid guiSkillId) &&
                                         _dicSkillBackups.TryGetValue(guiSkillId, out Skill objSkill) && objSkill != null)
                                {
                                    lstReturn.Add(new Tuple<Skill, bool>(objSkill, false));
                                }
                                else
                                {
                                    bool blnIsKnowledgeSkill
                                        = xmlSkillsDocument
                                              .SelectSingleNodeAndCacheExpressionAsNavigator(
                                                  "/chummer/categories/category[. = "
                                                  + xmlSkill["category"]?.InnerText.CleanXPath() + "]/@type", token)
                                              ?.Value
                                          != "active";
                                    lstReturn.Add(new Tuple<Skill, bool>(await Skill
                                        .FromDataAsync(xmlSkill, _objCharacter, blnIsKnowledgeSkill, token)
                                        .ConfigureAwait(false), true));
                                }
                            }
                        }
                        catch
                        {
                            foreach (Tuple<Skill, bool> tupSkill in lstReturn)
                            {
                                if (tupSkill.Item2)
                                    await tupSkill.Item1.RemoveAsync(CancellationToken.None).ConfigureAwait(false);
                            }
                            throw;
                        }
                    }
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            return lstReturn;
        }

        internal void AddSkills(FilterOption eFilterOption, string strName = "", CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (LockObject.EnterUpgradeableReadLock(token))
            {
                token.ThrowIfCancellationRequested();
                List<Tuple<Skill, bool>> lstSkillsToAdd = new List<Tuple<Skill, bool>>();
                try
                {
                    lstSkillsToAdd.AddRange(GetActiveSkillsFromData(eFilterOption, true, strName, token));
                    using (LockObject.EnterWriteLock(token))
                    {
                        token.ThrowIfCancellationRequested();
                        foreach (Tuple<Skill, bool> tupSkill in lstSkillsToAdd)
                        {
                            Skill objSkill = tupSkill.Item1;
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
                catch
                {
                    foreach (Tuple<Skill, bool> tupSkill in lstSkillsToAdd)
                    {
                        if (tupSkill.Item2)
                            tupSkill.Item1.Remove();
                    }
                    throw;
                }
            }
        }

        internal async Task AddSkillsAsync(FilterOption eFilterOption, string strName = "", CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                List<Tuple<Skill, bool>> lstSkillsToAdd = await GetActiveSkillsFromDataAsync(eFilterOption, true, strName, token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        foreach (Tuple<Skill, bool> tupSkill in lstSkillsToAdd)
                        {
                            Skill objSkill = tupSkill.Item1;
                            Guid guidLoop = await objSkill.GetSkillIdAsync(token).ConfigureAwait(false);
                            if (guidLoop != Guid.Empty && !objSkill.IsExoticSkill)
                            {
                                Skill objExistingSkill = await Skills
                                    .FirstOrDefaultAsync(
                                        async x => await x.GetSkillIdAsync(token).ConfigureAwait(false) == guidLoop, token)
                                    .ConfigureAwait(false);
                                if (objExistingSkill != null)
                                {
                                    await MergeSkillsAsync(objExistingSkill, objSkill, token).ConfigureAwait(false);
                                    continue;
                                }
                            }

                            await Skills
                                .AddWithSortAsync(objSkill, (x, y) => CompareSkillsAsync(x, y, token),
                                    (x, y) => MergeSkillsAsync(x, y, token), token)
                                .ConfigureAwait(false);
                        }
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }
                }
                catch
                {
                    foreach (Tuple<Skill, bool> tupSkill in lstSkillsToAdd)
                    {
                        if (tupSkill.Item2)
                            await tupSkill.Item1.RemoveAsync(token).ConfigureAwait(false);
                    }
                    throw;
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        internal ExoticSkill AddExoticSkill(string strName, string strSpecific, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (LockObject.EnterUpgradeableReadLock(token))
            {
                token.ThrowIfCancellationRequested();
                XmlNode xmlSkillNode = _objCharacter.LoadData("skills.xml", token: token)
                    .TryGetNodeByNameOrId("/chummer/skills/skill", strName);
                using (LockObject.EnterWriteLock(token))
                {
                    token.ThrowIfCancellationRequested();
                    ExoticSkill objExoticSkill = Skill.FromData(xmlSkillNode, _objCharacter, false) as ExoticSkill
                                                 ?? throw new ArgumentException(
                                                     "Attempted to add non-exotic skill as exotic skill");
                    try
                    {
                        objExoticSkill.Specific = strSpecific;
                        Skills.AddWithSort(objExoticSkill, CompareSkills, (x, y) => MergeSkills(x, y, token), token);
                        return objExoticSkill;
                    }
                    catch
                    {
                        objExoticSkill.Remove();
                        throw;
                    }
                }
            }
        }

        internal async Task<ExoticSkill> AddExoticSkillAsync(string strName, string strSpecific, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                XmlNode xmlSkillNode
                    = (await _objCharacter.LoadDataAsync("skills.xml", token: token).ConfigureAwait(false))
                    .TryGetNodeByNameOrId(
                        "/chummer/skills/skill", strName);
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    ExoticSkill objExoticSkill =
                        await Skill.FromDataAsync(xmlSkillNode, _objCharacter, false, token)
                            .ConfigureAwait(false) as ExoticSkill
                        ?? throw new ArgumentException("Attempted to add non-exotic skill as exotic skill");
                    try
                    {
                        await objExoticSkill.SetSpecificAsync(strSpecific, token).ConfigureAwait(false);
                        await Skills.AddWithSortAsync(objExoticSkill, (x, y) => CompareSkillsAsync(x, y, token),
                            (x, y) => MergeSkillsAsync(x, y, token),
                            token: token).ConfigureAwait(false);
                        return objExoticSkill;
                    }
                    catch
                    {
                        await objExoticSkill.RemoveAsync(CancellationToken.None).ConfigureAwait(false);
                        throw;
                    }
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        internal void RemoveSkills(FilterOption eSkillsToRemove, string strName = "", bool blnCreateKnowledge = true, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (LockObject.EnterUpgradeableReadLock(token))
            {
                token.ThrowIfCancellationRequested();
                HashSet<Skill> setSkillsToRemove = new HashSet<Skill>(FetchExistingSkillsByFilter(eSkillsToRemove, strName, token: token));
                // Check for duplicates (we'd normally want to make sure the improvement is enabled, but disabled SpecialSkills just force-disables a skill, so we need to keep those)
                foreach (Improvement objImprovement in _objCharacter.Improvements)
                {
                    token.ThrowIfCancellationRequested();
                    if (objImprovement.ImproveType != Improvement.ImprovementType.SpecialSkills)
                        continue;
                    FilterOption eFilterOption
                        = (FilterOption)Enum.Parse(typeof(FilterOption), objImprovement.ImprovedName);
                    foreach (Skill objSkill in FetchExistingSkillsByFilter(eFilterOption, objImprovement.Target, token: token))
                    {
                        setSkillsToRemove.Remove(objSkill);
                    }
                }

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
                    token.ThrowIfCancellationRequested();
                    for (int i = Skills.Count - 1; i >= 0; --i)
                    {
                        token.ThrowIfCancellationRequested();
                        Skill objSkill = Skills[i];
                        if (!setSkillsToRemove.Contains(objSkill))
                            continue;
                        if (!objSkill.IsExoticSkill)
                            _dicSkillBackups.TryAdd(objSkill.SkillId, objSkill);
                        Skills.RemoveAt(i);

                        if (blnCreateKnowledge && objSkill.TotalBaseRating > 0)
                        {
                            KnowledgeSkill objNewKnowledgeSkill = new KnowledgeSkill(_objCharacter);
                            try
                            {
                                objNewKnowledgeSkill.Type = strKnowledgeSkillTypeToUse.Value;
                                objNewKnowledgeSkill.WritableName = objSkill.Name;
                                objNewKnowledgeSkill.Base = objSkill.Base;
                                objNewKnowledgeSkill.Karma = objSkill.Karma;
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
                            catch
                            {
                                objNewKnowledgeSkill.Remove();
                                throw;
                            }
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
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                HashSet<Skill> setSkillsToRemove = new HashSet<Skill>(await FetchExistingSkillsByFilterAsync(eSkillsToRemove, strName, token).ConfigureAwait(false));
                // Check for duplicates (we'd normally want to make sure the improvement is enabled, but disabled SpecialSkills just force-disables a skill, so we need to keep those)
                await (await _objCharacter.GetImprovementsAsync(token).ConfigureAwait(false)).ForEachAsync(
                    async objImprovement =>
                    {
                        if (objImprovement.ImproveType != Improvement.ImprovementType.SpecialSkills)
                            return;
                        FilterOption eFilterOption
                            = (FilterOption)Enum.Parse(typeof(FilterOption), objImprovement.ImprovedName);
                        foreach (Skill objSkill in await FetchExistingSkillsByFilterAsync(eFilterOption, objImprovement.Target, token).ConfigureAwait(false))
                        {
                            setSkillsToRemove.Remove(objSkill);
                        }
                    }, token: token).ConfigureAwait(false);

                if (setSkillsToRemove.Count == 0)
                    return;

                AsyncLazy<string> strKnowledgeSkillTypeToUse = null;
                if (blnCreateKnowledge)
                {
                    strKnowledgeSkillTypeToUse = new AsyncLazy<string>(async () =>
                    {
                        XPathNavigator xmlCategories =
                            (await _objCharacter.LoadDataXPathAsync("skills.xml", token: token).ConfigureAwait(false))
                            .SelectSingleNodeAndCacheExpression("/chummer/categories", token);
                        if (xmlCategories.SelectSingleNodeAndCacheExpression(
                                "category[@type = \"knowledge\" and . = \"Professional\"]", token) != null)
                            return "Professional";
                        return xmlCategories
                                    .SelectSingleNodeAndCacheExpression("category[@type = \"knowledge\"]", token)?.Value
                                ?? "Professional";
                    }, Utils.JoinableTaskFactory);
                }

                token.ThrowIfCancellationRequested();
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
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
                            _dicSkillBackups.TryAdd(objSkill.SkillId, objSkill);
                        await lstSkills.RemoveAtAsync(i, token).ConfigureAwait(false);

                        if (blnCreateKnowledge &&
                            await objSkill.GetTotalBaseRatingAsync(token).ConfigureAwait(false) > 0)
                        {
                            KnowledgeSkill objNewKnowledgeSkill = new KnowledgeSkill(_objCharacter, false);
                            try
                            {
                                await objNewKnowledgeSkill.SetDefaultAttributeAsync("LOG", token).ConfigureAwait(false);
                                await objNewKnowledgeSkill
                                    .SetBaseAsync(await objSkill.GetBaseAsync(token).ConfigureAwait(false), token)
                                    .ConfigureAwait(false);
                                await objNewKnowledgeSkill
                                    .SetKarmaAsync(await objSkill.GetKarmaAsync(token).ConfigureAwait(false), token)
                                    .ConfigureAwait(false);
                                await objNewKnowledgeSkill.SetTypeAsync(
                                        await strKnowledgeSkillTypeToUse.GetValueAsync(token).ConfigureAwait(false), token)
                                    .ConfigureAwait(false);
                                await objNewKnowledgeSkill
                                    .SetWritableNameAsync(await objSkill.GetNameAsync(token).ConfigureAwait(false), token)
                                    .ConfigureAwait(false);
                                await objNewKnowledgeSkill.Specializations
                                    .AddRangeAsync(objSkill.Specializations, token: token).ConfigureAwait(false);
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
                            catch
                            {
                                await objNewKnowledgeSkill.RemoveAsync(CancellationToken.None).ConfigureAwait(false);
                                throw;
                            }
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
                                            async x => _dicSkills.ContainsKey(
                                                await x.GetDictionaryKeyAsync(token)
                                                    .ConfigureAwait(false)), token: token)
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
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        internal IEnumerable<Skill> FetchExistingSkillsByFilter(FilterOption eFilterOption, string strName = "", bool blnWithSideEffects = false, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IDisposable objLocker = LockObject.EnterReadLock(token);
            try
            {
                if (_dicSkillBackups.IsEmpty && Skills.Count == 0 && KnowledgeSkills.Count == 0)
                    yield break;
                if (blnWithSideEffects)
                {
                    objLocker.Dispose();
                    objLocker = LockObject.EnterUpgradeableReadLock(token);
                    if (_dicSkillBackups.IsEmpty && Skills.Count == 0 && KnowledgeSkills.Count == 0)
                        yield break;
                }
                XmlDocument xmlSkillsDocument = _objCharacter.LoadData("skills.xml", token: token);
                using (XmlNodeList xmlSkillList = xmlSkillsDocument
                            .SelectNodes("/chummer/skills/skill[not(exotic = 'True') and (" +
                                        _objCharacter.Settings.BookXPath(token: token)
                                        + ')'
                                        + SkillFilter(eFilterOption, strName) + ']'))
                {
                    if (xmlSkillList?.Count > 0)
                    {
                        foreach (XmlNode xmlSkill in xmlSkillList)
                        {
                            token.ThrowIfCancellationRequested();
                            if (xmlSkill.TryGetField("id", Guid.TryParse, out Guid guiSkillId))
                            {
                                if (_dicSkillBackups.TryGetValue(guiSkillId, out Skill objSkill) && objSkill != null)
                                    yield return objSkill;
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
                                    if (blnIsKnowledgeSkill)
                                    {
                                        objSkill = KnowledgeSkills.Find(x => x.SkillId == guiSkillId);
                                        if (objSkill != null)
                                            yield return objSkill;
                                        else
                                        {
                                            string strBackupName = string.Empty;
                                            if (xmlSkill.TryGetStringFieldQuickly("name", ref strBackupName))
                                            {
                                                objSkill = KnowledgeSkills.Find(x => x.Name == strName);
                                                if (objSkill != null)
                                                    yield return objSkill;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        objSkill = Skills.Find(x => x.SkillId == guiSkillId);
                                        if (objSkill != null)
                                            yield return objSkill;
                                        else
                                        {
                                            string strBackupName = string.Empty;
                                            if (xmlSkill.TryGetStringFieldQuickly("name", ref strBackupName))
                                            {
                                                objSkill = Skills.Find(x => x.Name == strName);
                                                if (objSkill != null)
                                                    yield return objSkill;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                objLocker.Dispose();
            }
        }

        internal async Task<List<Skill>> FetchExistingSkillsByFilterAsync(FilterOption eFilterOption, string strName = "", CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<Skill> lstReturn = new List<Skill>();
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (!_dicSkillBackups.IsEmpty)
                {
                    XmlDocument xmlSkillsDocument = await _objCharacter.LoadDataAsync("skills.xml", token: token).ConfigureAwait(false);
                    using (XmlNodeList xmlSkillList = xmlSkillsDocument
                               .SelectNodes("/chummer/skills/skill[not(exotic = 'True') and (" +
                                            await _objCharacter.Settings.BookXPathAsync(token: token).ConfigureAwait(false)
                                            + ')'
                                            + SkillFilter(eFilterOption, strName) + ']'))
                    {
                        if (xmlSkillList?.Count > 0)
                        {
                            foreach (XmlNode xmlSkill in xmlSkillList)
                            {
                                token.ThrowIfCancellationRequested();
                                if (xmlSkill.TryGetField("id", Guid.TryParse, out Guid guiSkillId))
                                {
                                    if (_dicSkillBackups.TryGetValue(guiSkillId, out Skill objSkill) && objSkill != null)
                                        lstReturn.Add(objSkill);
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
                                        if (blnIsKnowledgeSkill)
                                        {
                                            ThreadSafeBindingList<KnowledgeSkill> lstKnowledgeSkills = await GetKnowledgeSkillsAsync(token).ConfigureAwait(false);
                                            objSkill = await lstKnowledgeSkills.FirstOrDefaultAsync(async x => await x.GetSkillIdAsync(token).ConfigureAwait(false) == guiSkillId, token).ConfigureAwait(false);
                                            if (objSkill != null)
                                                lstReturn.Add(objSkill);
                                            else
                                            {
                                                string strBackupName = string.Empty;
                                                if (xmlSkill.TryGetStringFieldQuickly("name", ref strBackupName))
                                                {
                                                    objSkill = await lstKnowledgeSkills.FirstOrDefaultAsync(async x => await x.GetNameAsync(token).ConfigureAwait(false) == strName, token).ConfigureAwait(false);
                                                    if (objSkill != null)
                                                        lstReturn.Add(objSkill);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            ThreadSafeBindingList<Skill> lstSkills = await GetSkillsAsync(token).ConfigureAwait(false);
                                            objSkill = await lstSkills.FirstOrDefaultAsync(async x => await x.GetSkillIdAsync(token).ConfigureAwait(false) == guiSkillId, token).ConfigureAwait(false);
                                            if (objSkill != null)
                                                lstReturn.Add(objSkill);
                                            else
                                            {
                                                string strBackupName = string.Empty;
                                                if (xmlSkill.TryGetStringFieldQuickly("name", ref strBackupName))
                                                {
                                                    objSkill = await lstSkills.FirstOrDefaultAsync(async x => await x.GetNameAsync(token).ConfigureAwait(false) == strName, token).ConfigureAwait(false);
                                                    if (objSkill != null)
                                                        lstReturn.Add(objSkill);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            return lstReturn;
        }

        private ReadOnlyDictionary<string, string> _dicKnowledgeSkillCategoriesMap;  //Categories to their attribute
        private readonly AsyncFriendlyReaderWriterLock _objKnowledgeSkillCategoriesMapLock;

        public IReadOnlyDictionary<string, string> KnowledgeSkillCategoriesMap
        {
            get
            {
                if (GlobalSettings.LiveCustomData)
                {
                    XPathNodeIterator lstXmlSkills = _objCharacter.LoadDataXPath("skills.xml")
                        .SelectAndCacheExpression(
                            "/chummer/knowledgeskills/skill");
                    Dictionary<string, string> dicReturn = new Dictionary<string, string>(lstXmlSkills.Count);
                    foreach (XPathNavigator objXmlSkill in lstXmlSkills)
                    {
                        string strCategory = objXmlSkill.SelectSingleNodeAndCacheExpression("category")?.Value;
                        if (!string.IsNullOrWhiteSpace(strCategory))
                        {
                            dicReturn[strCategory] =
                                objXmlSkill.SelectSingleNodeAndCacheExpression("attribute")?.Value;
                        }
                    }

                    return new ReadOnlyDictionary<string, string>(dicReturn);
                }

                using (_objKnowledgeSkillCategoriesMapLock.EnterReadLock())
                {
                    if (_dicKnowledgeSkillCategoriesMap != null)
                        return _dicKnowledgeSkillCategoriesMap;
                }

                using (_objKnowledgeSkillCategoriesMapLock.EnterUpgradeableReadLock())
                {
                    if (_dicKnowledgeSkillCategoriesMap != null)
                        return _dicKnowledgeSkillCategoriesMap;

                    using (_objKnowledgeSkillCategoriesMapLock.EnterWriteLock())
                    {
                        XPathNodeIterator lstXmlSkills = _objCharacter.LoadDataXPath("skills.xml")
                            .SelectAndCacheExpression(
                                "/chummer/knowledgeskills/skill");
                        Dictionary<string, string> dicReturn =
                            new Dictionary<string, string>(lstXmlSkills.Count);
                        foreach (XPathNavigator objXmlSkill in lstXmlSkills)
                        {
                            string strCategory = objXmlSkill.SelectSingleNodeAndCacheExpression("category")
                                ?.Value;
                            if (!string.IsNullOrWhiteSpace(strCategory))
                            {
                                dicReturn[strCategory] =
                                    objXmlSkill.SelectSingleNodeAndCacheExpression("attribute")?.Value;
                            }
                        }

                        return _dicKnowledgeSkillCategoriesMap = new ReadOnlyDictionary<string, string>(dicReturn);
                    }
                }
            }
        }

        public async Task<IReadOnlyDictionary<string, string>> GetKnowledgeSkillCategoriesMapAsync(
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (GlobalSettings.LiveCustomData)
            {
                XPathNodeIterator lstXmlSkills =
                    (await _objCharacter.LoadDataXPathAsync("skills.xml", token: token).ConfigureAwait(false))
                    .SelectAndCacheExpression(
                        "/chummer/knowledgeskills/skill", token);
                Dictionary<string, string> dicReturn = new Dictionary<string, string>(lstXmlSkills.Count);
                foreach (XPathNavigator objXmlSkill in lstXmlSkills)
                {
                    string strCategory =
                        objXmlSkill.SelectSingleNodeAndCacheExpression("category", token)?.Value;
                    if (!string.IsNullOrWhiteSpace(strCategory))
                    {
                        dicReturn[strCategory] =
                            objXmlSkill.SelectSingleNodeAndCacheExpression("attribute", token)?.Value;
                    }
                }

                return new ReadOnlyDictionary<string, string>(dicReturn);
            }

            IAsyncDisposable objLocker = await _objKnowledgeSkillCategoriesMapLock.EnterReadLockAsync(token)
                .ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_dicKnowledgeSkillCategoriesMap != null)
                    return _dicKnowledgeSkillCategoriesMap;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            objLocker = await _objKnowledgeSkillCategoriesMapLock.EnterUpgradeableReadLockAsync(token)
                .ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();

                if (_dicKnowledgeSkillCategoriesMap != null)
                    return _dicKnowledgeSkillCategoriesMap;

                IAsyncDisposable objLocker2 = await _objKnowledgeSkillCategoriesMapLock.EnterWriteLockAsync(token)
                    .ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    XPathNodeIterator lstXmlSkills =
                        (await _objCharacter.LoadDataXPathAsync("skills.xml", token: token)
                            .ConfigureAwait(false))
                        .SelectAndCacheExpression(
                            "/chummer/knowledgeskills/skill", token);
                    Dictionary<string, string> dicReturn =
                        new Dictionary<string, string>(lstXmlSkills.Count);
                    foreach (XPathNavigator objXmlSkill in lstXmlSkills)
                    {
                        string strCategory =
                            objXmlSkill.SelectSingleNodeAndCacheExpression("category", token)?.Value;
                        if (!string.IsNullOrWhiteSpace(strCategory))
                        {
                            dicReturn[strCategory] =
                                objXmlSkill.SelectSingleNodeAndCacheExpression("attribute", token)?.Value;
                        }
                    }

                    return _dicKnowledgeSkillCategoriesMap = new ReadOnlyDictionary<string, string>(dicReturn);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
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
                            _dicSkills.Clear();
                            if (!blnLegacy)
                            {
                                using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool,
                                           out HashSet<string>
                                               setSkillIdsToSkip))
                                {
                                    // Special loading procedure where we initialize our skills list at the same time as loading it
                                    // This is faster than doing the initialize first and then loading of all our skills afterwards
                                    using (_ = Timekeeper.StartSyncron("load_char_skills_initialize", opLoadCharSkills))
                                    {
                                        token.ThrowIfCancellationRequested();
                                        if (blnSync)
                                            // ReSharper disable once MethodHasAsyncOverload
                                            _objSkillsInitializerLock.SetParent(token: token);
                                        else
                                            await _objSkillsInitializerLock.SetParentAsync(token: token).ConfigureAwait(false);
                                        try
                                        {
                                            IDisposable objLocker2 = null;
                                            IAsyncDisposable objLockerAsync2 = null;
                                            if (blnSync)
                                                // ReSharper disable once MethodHasAsyncOverload
                                                objLocker2 = _objSkillsInitializerLock.EnterReadLock(token);
                                            else
                                                objLockerAsync2 = await _objSkillsInitializerLock
                                                    .EnterReadLockAsync(token).ConfigureAwait(false);
                                            bool blnDoInitialize;
                                            try
                                            {
                                                token.ThrowIfCancellationRequested();
                                                blnDoInitialize = !_blnSkillsInitialized &&
                                                                  _objCharacter.SkillsSection == this;
                                            }
                                            finally
                                            {
                                                if (blnSync)
                                                    // ReSharper disable once MethodHasAsyncOverload
                                                    objLocker2.Dispose();
                                                else
                                                    await objLockerAsync2.DisposeAsync().ConfigureAwait(false);
                                            }

                                            if (blnDoInitialize)
                                            {
                                                objLocker2 = null;
                                                objLockerAsync2 = null;
                                                if (blnSync)
                                                    // ReSharper disable once MethodHasAsyncOverload
                                                    objLocker2 =
                                                        _objSkillsInitializerLock.EnterUpgradeableReadLock(token);
                                                else
                                                {
                                                    objLockerAsync2 = await _objSkillsInitializerLock
                                                        .EnterUpgradeableReadLockAsync(token)
                                                        .ConfigureAwait(false);
                                                }

                                                try
                                                {
                                                    token.ThrowIfCancellationRequested();
                                                    if (!_blnSkillsInitialized && _objCharacter.SkillsSection == this)
                                                    {
                                                        IDisposable objLocker3 = null;
                                                        IAsyncDisposable objLockerAsync3 = null;
                                                        if (blnSync)
                                                            // ReSharper disable once MethodHasAsyncOverload
                                                            objLocker3 =
                                                                _objSkillsInitializerLock.EnterWriteLock(token);
                                                        else
                                                            objLockerAsync3 = await _objSkillsInitializerLock
                                                                .EnterWriteLockAsync(token)
                                                                .ConfigureAwait(false);
                                                        try
                                                        {
                                                            token.ThrowIfCancellationRequested();
                                                            blnDidInitializeInLoad = true;
                                                            XmlDocument xmlSkillsDataDoc = blnSync
                                                                // ReSharper disable once MethodHasAsyncOverload
                                                                ? _objCharacter.LoadData("skills.xml",
                                                                    token: token)
                                                                : await _objCharacter
                                                                    .LoadDataAsync("skills.xml", token: token)
                                                                    .ConfigureAwait(false);
                                                            using (XmlNodeList lstSkillDataNodes =
                                                                   xmlSkillsDataDoc.SelectNodes(
                                                                       "/chummer/skills/skill[not(exotic = 'True') and ("
                                                                       + (blnSync
                                                                           // ReSharper disable once MethodHasAsyncOverload
                                                                           ? _objCharacter.Settings.BookXPath(
                                                                               token: token)
                                                                           : await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false))
                                                                               .BookXPathAsync(token: token)
                                                                               .ConfigureAwait(false)) + ')'
                                                                       + SkillFilter(FilterOption.NonSpecial) +
                                                                       ']'))
                                                            {
                                                                if (lstSkillDataNodes?.Count > 0)
                                                                {
                                                                    foreach (XmlNode xmlSkillDataNode in
                                                                             lstSkillDataNodes)
                                                                    {
                                                                        bool blnIsKnowledgeSkill
                                                                            = xmlSkillsDataDoc
                                                                                  // ReSharper disable once MethodHasAsyncOverload
                                                                                  .SelectSingleNodeAndCacheExpressionAsNavigator(
                                                                                      "/chummer/categories/category[. = "
                                                                                      + xmlSkillDataNode[
                                                                                              "category"]
                                                                                          ?.InnerText
                                                                                          .CleanXPath()
                                                                                      + "]/@type", token)
                                                                                  ?.Value
                                                                              != "active";
                                                                        Skill objSkill = blnSync
                                                                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                                            ? Skill.FromData(
                                                                                xmlSkillDataNode,
                                                                                _objCharacter,
                                                                                blnIsKnowledgeSkill)
                                                                            : await Skill.FromDataAsync(
                                                                                    xmlSkillDataNode,
                                                                                    _objCharacter,
                                                                                    blnIsKnowledgeSkill, token)
                                                                                .ConfigureAwait(false);
                                                                        try
                                                                        {
                                                                            if (objSkill.SkillId != Guid.Empty)
                                                                            {
                                                                                string strSkillId =
                                                                                    objSkill.SkillId.ToString("D", GlobalSettings.InvariantCultureInfo);
                                                                                XmlNode xmlLoadingSkillNode =
                                                                                    xmlSkillNode.SelectSingleNode(
                                                                                        "skills/skill[suid = " +
                                                                                        strSkillId.CleanXPath() +
                                                                                        ']') ??
                                                                                    xmlSkillNode.SelectSingleNode(
                                                                                        "knoskills/skill[suid = " +
                                                                                        strSkillId.CleanXPath() +
                                                                                        ']');
                                                                                if (xmlLoadingSkillNode != null)
                                                                                {
                                                                                    setSkillIdsToSkip.Add(
                                                                                        strSkillId);
                                                                                    if (blnSync)
                                                                                    {
                                                                                        Skill.Load(_objCharacter,
                                                                                            xmlLoadingSkillNode,
                                                                                            objSkill);
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        await Skill.LoadAsync(_objCharacter,
                                                                                            xmlLoadingSkillNode,
                                                                                            objSkill, token).ConfigureAwait(false);
                                                                                    }
                                                                                }
                                                                            }

                                                                            KnowledgeSkill objKnoSkill =
                                                                                blnIsKnowledgeSkill
                                                                                    ? objSkill as KnowledgeSkill
                                                                                    : null;

                                                                            if (blnSync)
                                                                            {
                                                                                if (objKnoSkill != null)
                                                                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                                                    _lstKnowledgeSkills
                                                                                        .Add(objKnoSkill);
                                                                                else
                                                                                {
                                                                                    string strKey =
                                                                                        objSkill.DictionaryKey;
                                                                                    if (_dicSkills.TryAdd(strKey,
                                                                                             objSkill))
                                                                                    {
                                                                                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                                                        _lstSkills.Add(objSkill);
                                                                                    }
                                                                                    else if (_dicSkills.TryGetValue(
                                                                                             strKey,
                                                                                             out Skill
                                                                                                 objExistingSkill))
                                                                                    {
                                                                                        // ReSharper disable once MethodHasAsyncOverload
                                                                                        MergeSkills(
                                                                                            objExistingSkill,
                                                                                            objSkill,
                                                                                            token);
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        Utils.BreakIfDebug();
                                                                                        objSkill?.Remove();
                                                                                    }
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
                                                                                if (_dicSkills.TryAdd(strKey,
                                                                                        objSkill))
                                                                                {
                                                                                    await _lstSkills.AddAsync(
                                                                                            objSkill,
                                                                                            token)
                                                                                        .ConfigureAwait(false);
                                                                                }
                                                                                else if (_dicSkills.TryGetValue(
                                                                                         strKey,
                                                                                         out Skill
                                                                                             objExistingSkill))
                                                                                    await MergeSkillsAsync(
                                                                                            objExistingSkill,
                                                                                            objSkill, token)
                                                                                        .ConfigureAwait(false);
                                                                                else
                                                                                {
                                                                                    Utils.BreakIfDebug();
                                                                                    if (objSkill != null)
                                                                                        await objSkill.RemoveAsync(CancellationToken.None).ConfigureAwait(false);
                                                                                }
                                                                            }
                                                                        }
                                                                        catch
                                                                        {
                                                                            if (blnSync)
                                                                                objSkill?.Remove();
                                                                            else if (objSkill != null)
                                                                                await objSkill.RemoveAsync(CancellationToken.None).ConfigureAwait(false);
                                                                            throw;
                                                                        }
                                                                    }
                                                                }
                                                            }

                                                            _blnSkillsInitialized = true;
                                                        }
                                                        finally
                                                        {
                                                            if (blnSync)
                                                                // ReSharper disable once MethodHasAsyncOverload
                                                                objLocker3.Dispose();
                                                            else
                                                                await objLockerAsync3.DisposeAsync()
                                                                    .ConfigureAwait(false);
                                                        }
                                                    }
                                                }
                                                finally
                                                {
                                                    if (blnSync)
                                                        // ReSharper disable once MethodHasAsyncOverload
                                                        objLocker2.Dispose();
                                                    else
                                                        await objLockerAsync2.DisposeAsync().ConfigureAwait(false);
                                                }
                                            }
                                        }
                                        finally
                                        {
                                            if (blnSync)
                                                // ReSharper disable once MethodHasAsyncOverload
                                                _objSkillsInitializerLock.SetParent(LockObject, token: token);
                                            else
                                                await _objSkillsInitializerLock.SetParentAsync(LockObject, token: token).ConfigureAwait(false);
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
                                                            ? _lstSkillGroups.FirstOrDefault(x => x.Name == strName)
                                                            : await _lstSkillGroups.FirstOrDefaultAsync(
                                                                    x => x.Name == strName, token: token)
                                                                .ConfigureAwait(false);
                                                    if (objGroup != null)
                                                    {
                                                        if (blnSync)
                                                            objGroup.Load(xmlNode, token);
                                                        else
                                                            await objGroup.LoadAsync(xmlNode, token).ConfigureAwait(false);
                                                    }
                                                    else
                                                    {
                                                        objGroup = new SkillGroup(_objCharacter, strName);
                                                        if (blnSync)
                                                        {
                                                            try
                                                            {
                                                                // ReSharper disable once MethodHasAsyncOverload
                                                                objGroup.Load(xmlNode, token);
                                                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                                _lstSkillGroups.Add(objGroup);
                                                                // ReSharper disable once MethodHasAsyncOverload
                                                                objGroup.LockObject.SetParent(_objCharacter.LockObject, token: token);
                                                            }
                                                            catch
                                                            {
                                                                try
                                                                {
                                                                    _lstSkillGroups.Remove(objGroup);
                                                                }
                                                                catch
                                                                {
                                                                    // swallow this
                                                                }
                                                                objGroup.Dispose();
                                                                throw;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            try
                                                            {
                                                                await objGroup.LoadAsync(xmlNode, token).ConfigureAwait(false);
                                                                await _lstSkillGroups.AddAsync(objGroup, token)
                                                                    .ConfigureAwait(false);
                                                                await objGroup.LockObject.SetParentAsync(_objCharacter.LockObject, token: token).ConfigureAwait(false);
                                                            }
                                                            catch
                                                            {
                                                                try
                                                                {
                                                                    await _lstSkillGroups.RemoveAsync(objGroup, CancellationToken.None).ConfigureAwait(false);
                                                                }
                                                                catch
                                                                {
                                                                    // swallow this
                                                                }
                                                                await objGroup.DisposeAsync().ConfigureAwait(false);
                                                                throw;
                                                            }
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
                                                    Skill objSkill;
                                                    bool blnNewSkill;
                                                    if (blnSync)
                                                        objSkill = Skill.Load(_objCharacter, xmlNode, out blnNewSkill);
                                                    else
                                                        (objSkill, blnNewSkill) = await Skill.LoadAsync(_objCharacter, xmlNode, token: token).ConfigureAwait(false);
                                                    if (objSkill == null)
                                                        continue;
                                                    try
                                                    {
                                                        if (blnSync)
                                                        {
                                                            string strKey = objSkill.DictionaryKey;
                                                            if (_dicSkills.TryAdd(strKey, objSkill))
                                                            {
                                                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                                _lstSkills.Add(objSkill);
                                                            }
                                                            else if (_dicSkills.TryGetValue(strKey,
                                                                         out Skill objExistingSkill))
                                                            {
                                                                // ReSharper disable once MethodHasAsyncOverload
                                                                MergeSkills(objExistingSkill, objSkill, token);
                                                            }
                                                            else
                                                            {
                                                                Utils.BreakIfDebug();
                                                                if (blnNewSkill)
                                                                    objSkill?.Remove();
                                                            }
                                                        }
                                                        else
                                                        {
                                                            string strKey = await objSkill.GetDictionaryKeyAsync(token)
                                                                .ConfigureAwait(false);
                                                            if (_dicSkills.TryAdd(strKey, objSkill))
                                                            {
                                                                await _lstSkills.AddAsync(objSkill, token)
                                                                    .ConfigureAwait(false);
                                                            }
                                                            else if (_dicSkills.TryGetValue(strKey,
                                                                         out Skill objExistingSkill))
                                                                await MergeSkillsAsync(objExistingSkill,
                                                                    objSkill, token).ConfigureAwait(false);
                                                            else
                                                            {
                                                                Utils.BreakIfDebug();
                                                                if (blnNewSkill && objSkill != null)
                                                                    await objSkill.RemoveAsync(token).ConfigureAwait(false);
                                                            }
                                                        }
                                                    }
                                                    catch
                                                    {
                                                        if (blnNewSkill)
                                                        {
                                                            if (blnSync)
                                                                objSkill?.Remove();
                                                            else if (objSkill != null)
                                                                await objSkill.RemoveAsync(CancellationToken.None).ConfigureAwait(false);
                                                        }
                                                        throw;
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
                                                    Skill objUncastSkill;
                                                    bool blnNewSkill;
                                                    if (blnSync)
                                                        objUncastSkill = Skill.Load(_objCharacter, xmlNode, out blnNewSkill);
                                                    else
                                                        (objUncastSkill, blnNewSkill) = await Skill.LoadAsync(_objCharacter, xmlNode, token: token).ConfigureAwait(false);
                                                    try
                                                    {
                                                        if (objUncastSkill is KnowledgeSkill objSkill)
                                                        {
                                                            if (blnSync)
                                                            {
                                                                ThreadSafeBindingList<KnowledgeSkill> lstKnowledgeSkills = KnowledgeSkills;
                                                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                                if (!lstKnowledgeSkills.Contains(objSkill))
                                                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                                    lstKnowledgeSkills.Add(objSkill);
                                                            }
                                                            else
                                                            {
                                                                ThreadSafeBindingList<KnowledgeSkill> lstKnowledgeSkills = await GetKnowledgeSkillsAsync(token).ConfigureAwait(false);
                                                                if (!await lstKnowledgeSkills.ContainsAsync(objSkill, token)
                                                                         .ConfigureAwait(false))
                                                                    await lstKnowledgeSkills.AddAsync(objSkill, token)
                                                                        .ConfigureAwait(false);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            Utils
                                                                .BreakIfDebug(); // Somehow, a non-knowledge skill got into a knowledge skill block
                                                            if (blnNewSkill && objUncastSkill != null)
                                                            {
                                                                if (blnSync)
                                                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                                    objUncastSkill.Remove();
                                                                else
                                                                    await objUncastSkill.RemoveAsync(token)
                                                                        .ConfigureAwait(false);
                                                            }
                                                        }
                                                    }
                                                    catch
                                                    {
                                                        if (blnNewSkill && objUncastSkill != null)
                                                        {
                                                            if (blnSync)
                                                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                                objUncastSkill.Remove();
                                                            else
                                                                await objUncastSkill.RemoveAsync(token)
                                                                    .ConfigureAwait(false);
                                                        }
                                                        throw;
                                                    }
                                                }
                                            }
                                        }

                                        // Legacy sweep for native language skills
                                        if (_objCharacter.LastSavedVersion <= new ValueVersion(5, 212, 72))
                                        {
                                            if (blnSync)
                                            {
                                                if (_objCharacter.Created)
                                                {
                                                    ThreadSafeBindingList<KnowledgeSkill> lstKnowledgeSkills = KnowledgeSkills;
                                                    // ReSharper disable once MethodHasAsyncOverload
                                                    if (!lstKnowledgeSkills.Any(x => x.IsNativeLanguage, token))
                                                    {
                                                        KnowledgeSkill objEnglishSkill = new KnowledgeSkill(_objCharacter);
                                                        try
                                                        {
                                                            objEnglishSkill.WritableName = "English";
                                                            objEnglishSkill.IsNativeLanguage = true;
                                                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                            lstKnowledgeSkills.Add(objEnglishSkill);
                                                        }
                                                        catch
                                                        {
                                                            objEnglishSkill.Remove();
                                                            throw;
                                                        }
                                                    }
                                                }
                                            }
                                            else if (await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false))
                                            {
                                                ThreadSafeBindingList<KnowledgeSkill> lstKnowledgeSkills = await GetKnowledgeSkillsAsync(token).ConfigureAwait(false);
                                                if (!await lstKnowledgeSkills
                                                         .AnyAsync(x => x.GetIsNativeLanguageAsync(token),
                                                             token)
                                                         .ConfigureAwait(false))
                                                {
                                                    KnowledgeSkill objEnglishSkill = new KnowledgeSkill(_objCharacter, false);
                                                    try
                                                    {
                                                        await objEnglishSkill.SetDefaultAttributeAsync("LOG", token).ConfigureAwait(false);
                                                        await objEnglishSkill.SetWritableNameAsync("English", token)
                                                            .ConfigureAwait(false);
                                                        await objEnglishSkill.SetIsNativeLanguageAsync(true, token)
                                                            .ConfigureAwait(false);
                                                        await lstKnowledgeSkills.AddAsync(objEnglishSkill, token)
                                                            .ConfigureAwait(false);
                                                    }
                                                    catch
                                                    {
                                                        await objEnglishSkill.RemoveAsync(CancellationToken.None).ConfigureAwait(false);
                                                        throw;
                                                    }
                                                }
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
                                                    if (blnSync)
                                                    {
                                                        KnowledgeSkill objSkill
                                                            = new KnowledgeSkill(_objCharacter, strName, false);
                                                        try
                                                        {
                                                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                            KnowsoftSkills.Add(objSkill);
                                                        }
                                                        catch
                                                        {
                                                            objSkill.Remove();
                                                            throw;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        KnowledgeSkill objSkill
                                                            = await KnowledgeSkill
                                                                .NewAsync(_objCharacter, strName, false, token)
                                                                .ConfigureAwait(false);
                                                        try
                                                        {
                                                            await KnowsoftSkills.AddAsync(objSkill, token)
                                                                .ConfigureAwait(false);
                                                        }
                                                        catch
                                                        {
                                                            await objSkill.RemoveAsync(CancellationToken.None).ConfigureAwait(false);
                                                            throw;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    //Timekeeper.Finish("load_char_knowsoft_buffer");
                                }
                            }
                            else
                            {
                                List<Skill> lstTempSkillList = null;
                                try
                                {
                                    using (XmlNodeList xmlSkillsList = xmlSkillNode.SelectNodes("skills/skill"))
                                    {
                                        lstTempSkillList = new List<Skill>(xmlSkillsList?.Count ?? 0);
                                        if (xmlSkillsList?.Count > 0)
                                        {
                                            foreach (XmlNode xmlNode in xmlSkillsList)
                                            {
                                                Skill objSkill = blnSync
                                                    ? Skill.LegacyLoad(_objCharacter, xmlNode)
                                                    : await Skill.LegacyLoadAsync(_objCharacter, xmlNode, token).ConfigureAwait(false);
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

                                        async Task<bool> OldSkillFilterAsync(Skill skill)
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
                                                    await (await GetKnowledgeSkillsAsync(token).ConfigureAwait(false)).AddAsync(objKnoSkill, token)
                                                        .ConfigureAwait(false);
                                            }
                                            else if (blnSync)
                                            {
                                                if (OldSkillFilter(objSkill))
                                                    lstUnsortedSkills.Add(objSkill);
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
                                                await (await GetSkillsAsync(token).ConfigureAwait(false)).AddAsync(objSkill, token).ConfigureAwait(false);
                                        }

                                        if (blnSync)
                                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                            UpdateUndoList(xmlSkillNode.OwnerDocument);
                                        else
                                            await UpdateUndoListAsync(xmlSkillNode.OwnerDocument, token)
                                                .ConfigureAwait(false);
                                    }
                                }
                                catch
                                {
                                    if (lstTempSkillList != null)
                                    {
                                        foreach (Skill objSkill in lstTempSkillList)
                                        {
                                            if (blnSync)
                                                objSkill.Remove();
                                            else
                                                await objSkill.RemoveAsync(CancellationToken.None).ConfigureAwait(false);
                                        }
                                    }
                                    throw;
                                }
                            }

                            if (!blnDidInitializeInLoad)
                            {
                                using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool,
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
                                                   : await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).BookXPathAsync(token: token)
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
                                                    Skill objSkill = blnSync
                                                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                        ? Skill.FromData(xmlSkillDataNode, _objCharacter, false)
                                                        : await Skill.FromDataAsync(xmlSkillDataNode, _objCharacter, false, token).ConfigureAwait(false);
                                                    if (objSkill == null)
                                                        continue;
                                                    if (blnSync)
                                                    {
                                                        try
                                                        {
                                                            string strKey = objSkill.DictionaryKey;
                                                            if (_dicSkills.TryAdd(strKey, objSkill))
                                                            {
                                                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                                _lstSkills.Add(objSkill);
                                                            }
                                                            else if (_dicSkills.TryGetValue(strKey,
                                                                         out Skill objExistingSkill))
                                                            {
                                                                // ReSharper disable once MethodHasAsyncOverload
                                                                MergeSkills(objExistingSkill, objSkill, token);
                                                            }
                                                            else
                                                            {
                                                                Utils.BreakIfDebug();
                                                                objSkill.Remove();
                                                            }
                                                        }
                                                        catch
                                                        {
                                                            objSkill.Remove();
                                                            throw;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        try
                                                        {
                                                            string strKey = await objSkill.GetDictionaryKeyAsync(token)
                                                                .ConfigureAwait(false);
                                                            if (_dicSkills.TryAdd(strKey, objSkill))
                                                            {
                                                                await _lstSkills.AddAsync(objSkill, token)
                                                                    .ConfigureAwait(false);
                                                            }
                                                            else if (_dicSkills.TryGetValue(strKey,
                                                                         out Skill objExistingSkill))
                                                                await MergeSkillsAsync(objExistingSkill,
                                                                    objSkill, token).ConfigureAwait(false);
                                                            else
                                                            {
                                                                Utils.BreakIfDebug();
                                                                await objSkill.RemoveAsync(token).ConfigureAwait(false);
                                                            }
                                                        }
                                                        catch
                                                        {
                                                            await objSkill.RemoveAsync(CancellationToken.None).ConfigureAwait(false);
                                                            throw;
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
                                    objKnoSkill.MultiplePropertiesChangedAsync += OnKnowledgeSkillPropertyChanged;
                                }
                            }
                            else
                            {
                                await KnowledgeSkills
                                    .ForEachAsync(
                                        objKnoSkill =>
                                            objKnoSkill.MultiplePropertiesChangedAsync += OnKnowledgeSkillPropertyChanged, token)
                                    .ConfigureAwait(false);
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
                                    foreach (SkillGroup objSkillGroup in SkillGroups.ToList())
                                    {
                                        token.ThrowIfCancellationRequested();
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
                                else if (_objCharacter.Settings.AllowSkillRegrouping)
                                {
                                    // TODO: Skill groups don't refresh their CanIncrease property correctly when the last of their skills is being added, as the total base rating will be zero. Call this here to force a refresh.
                                    foreach (SkillGroup g in SkillGroups.ToList())
                                    {
                                        token.ThrowIfCancellationRequested();
                                        g.OnMultiplePropertyChanged(nameof(SkillGroup.SkillList), nameof(SkillGroup.HasAnyBreakingSkills));
                                    }
                                }
                                else
                                {
                                    // TODO: Skill groups don't refresh their CanIncrease property correctly when the last of their skills is being added, as the total base rating will be zero. Call this here to force a refresh.
                                    foreach (SkillGroup g in SkillGroups.ToList())
                                    {
                                        token.ThrowIfCancellationRequested();
                                        g.OnPropertyChanged(nameof(SkillGroup.SkillList));
                                    }
                                }
                            }
                            else if (!await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false))
                            {
                                // zero out any skillgroups whose skills did not make the final cut
                                foreach (SkillGroup objSkillGroup in await (await GetSkillGroupsAsync(token).ConfigureAwait(false)).ToListAsync(token)
                                             .ConfigureAwait(false))
                                {
                                    token.ThrowIfCancellationRequested();
                                    if (!await objSkillGroup.SkillList.AnyAsync(
                                                async x => _dicSkills.ContainsKey(
                                                        await x.GetDictionaryKeyAsync(token)
                                                            .ConfigureAwait(false)), token: token)
                                            .ConfigureAwait(false))
                                    {
                                        await objSkillGroup.SetBaseAsync(0, token).ConfigureAwait(false);
                                        await objSkillGroup.SetKarmaAsync(0, token).ConfigureAwait(false);
                                    }
                                    else
                                    {
                                        // TODO: Skill groups don't refresh their CanIncrease property correctly when the last of their skills is being added, as the total base rating will be zero. Call this here to force a refresh.
                                        await objSkillGroup.OnPropertyChangedAsync(nameof(SkillGroup.SkillList), token)
                                            .ConfigureAwait(false);
                                    }
                                }
                            }
                            else if (_objCharacter.Settings.AllowSkillRegrouping)
                            {
                                // TODO: Skill groups don't refresh their CanIncrease property correctly when the last of their skills is being added, as the total base rating will be zero. Call this here to force a refresh.
                                foreach (SkillGroup objSkillGroup in await (await GetSkillGroupsAsync(token).ConfigureAwait(false)).ToListAsync(token)
                                             .ConfigureAwait(false))
                                {
                                    token.ThrowIfCancellationRequested();
                                    await objSkillGroup.OnMultiplePropertyChangedAsync(token, nameof(SkillGroup.SkillList), nameof(SkillGroup.HasAnyBreakingSkills)).ConfigureAwait(false);
                                }
                            }
                            else
                            {
                                // TODO: Skill groups don't refresh their CanIncrease property correctly when the last of their skills is being added, as the total base rating will be zero. Call this here to force a refresh.
                                foreach (SkillGroup objSkillGroup in await (await GetSkillGroupsAsync(token).ConfigureAwait(false)).ToListAsync(token)
                                             .ConfigureAwait(false))
                                {
                                    token.ThrowIfCancellationRequested();
                                    await objSkillGroup.OnPropertyChangedAsync(nameof(SkillGroup.SkillList), token)
                                        .ConfigureAwait(false);
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
                            try
                            {
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
                                objGroup.LockObject.SetParent(_objCharacter.LockObject, token: token);
                            }
                            catch
                            {
                                try
                                {
                                    SkillGroups.Remove(objGroup);
                                }
                                catch
                                {
                                    // swallow this
                                }
                                objGroup.Dispose();
                                throw;
                            }
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
                            XmlNode xmlLoop = lstNodesToChange[i];
                            if (xmlLoop == null)
                                continue;
                            xmlLoop.InnerText
                                = map.TryGetValue(xmlLoop.InnerText, out Guid guidLoop)
                                    ? guidLoop.ToString("D", GlobalSettings.InvariantCultureInfo)
                                    : Utils.GuidEmptyString;
                        }
                    }
                }
            }
        }

        private async Task UpdateUndoListAsync(XmlDocument xmlSkillOwnerDocument, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                //Hacky way of converting Expense entries to guid based skill identification
                //specs already did?
                //First create dictionary mapping name=>guid
                ConcurrentDictionary<string, Guid> dicGroups = new ConcurrentDictionary<string, Guid>();
                ConcurrentDictionary<string, Guid> dicSkills = new ConcurrentDictionary<string, Guid>();
                // Potentially expensive checks that can (and therefore should) be parallelized. Normally, this would just be a Parallel.Invoke,
                // but we want to allow UI messages to happen, just in case this is called on the Main Thread and another thread wants to show a message box.
                // Not using async-await because this is trivial code and I do not want to infect everything that calls this with async as well.
                await Task.WhenAll(
                        Task.Run(() => SkillGroups.ForEachParallelAsync(async x =>
                        {
                            // ReSharper disable once AccessToDisposedClosure
                            if (await x.GetRatingAsync(token).ConfigureAwait(false) > 0)
                                dicGroups.TryAdd(x.Name, x.Id);
                        }, token: token), token),
                        Task.Run(() => Skills.ForEachParallelAsync(async x =>
                        {
                            if (await x.GetTotalBaseRatingAsync(token).ConfigureAwait(false) > 0)
                                dicSkills.TryAdd(x.Name, x.Id);
                        }, token: token), token),
                        // ReSharper disable once AccessToDisposedClosure
                        Task.Run(
                            () => KnowledgeSkills.ForEachParallelAsync(x => dicSkills.TryAdd(x.Name, x.Id),
                                token: token), token))
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
                            XmlNode xmlLoop = lstNodesToChange[i];
                            if (xmlLoop == null)
                                continue;
                            xmlLoop.InnerText
                                = map.TryGetValue(xmlLoop.InnerText, out Guid guidLoop)
                                    ? guidLoop.ToString("D", GlobalSettings.InvariantCultureInfo)
                                    : Utils.GuidEmptyString;
                        }
                    }
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
                lstKnoSkillsOrdered.Clear();
                lstKnoSkillsOrdered.AddRange(KnowsoftSkills);
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
                    List<Skill> lstSkillBackups = _dicSkillBackups.GetValuesToListSafe();
                    _dicSkillBackups.Clear();
                    foreach (Skill objSkill in lstSkillBackups)
                        objSkill.Remove();
                    _lstSkills.ForEach(x => x.Remove(), token);
                    KnowledgeSkills.ForEach(x =>
                    {
                        x.MultiplePropertiesChangedAsync -= OnKnowledgeSkillPropertyChanged;
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

        internal async Task ResetAsync(bool blnFirstTime = false, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                Interlocked.Increment(ref _intLoading);
                try
                {
                    List<Skill> lstSkillBackups = _dicSkillBackups.GetValuesToListSafe();
                    _dicSkillBackups.Clear();
                    foreach (Skill objSkill in lstSkillBackups)
                        await objSkill.RemoveAsync(token).ConfigureAwait(false);
                    await _lstSkills.ForEachAsync(x => x.RemoveAsync(token), token).ConfigureAwait(false);
                    await KnowledgeSkills.ForEachAsync(objSkill =>
                    {
                        objSkill.MultiplePropertiesChangedAsync -= OnKnowledgeSkillPropertyChanged;
                        return objSkill.RemoveAsync(token);
                    }, token).ConfigureAwait(false);
                    await SkillGroups.ForEachWithSideEffectsAsync(async x => await x.DisposeAsync().ConfigureAwait(false), token).ConfigureAwait(false);
                    _dicSkillBackups.Clear();
                    _dicSkills.Clear();
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
        private readonly AsyncFriendlyReaderWriterLock _objSkillsInitializerLock;
        private readonly ThreadSafeBindingList<Skill> _lstSkills;
        private readonly ConcurrentDictionary<string, Skill> _dicSkills = new ConcurrentDictionary<string, Skill>();

        /// <summary>
        /// Active Skills
        /// </summary>
        public ThreadSafeBindingList<Skill> Skills
        {
            get
            {
                using (_objSkillsInitializerLock.EnterReadLock())
                {
                    if (_blnSkillsInitialized || _objCharacter.SkillsSection != this)
                        return _lstSkills;
                }

                using (_objSkillsInitializerLock.EnterUpgradeableReadLock())
                {
                    if (_blnSkillsInitialized || _objCharacter.SkillsSection != this)
                        return _lstSkills;
                    using (_objSkillsInitializerLock.EnterWriteLock())
                    {
                        _lstSkills.LockObject.SetParent();
                        _lstSkillGroups.LockObject.SetParent();
                        _lstSkills.RaiseListChangedEvents = false;
                        _lstSkillGroups.RaiseListChangedEvents = false;
                        try
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
                                                  .SelectSingleNodeAndCacheExpressionAsNavigator(
                                                      "/chummer/categories/category[. = "
                                                      + xmlSkill["category"]?.InnerText.CleanXPath()
                                                      + "]/@type")
                                                  ?.Value
                                              != "active";
                                        Skill objSkill = Skill.FromData(xmlSkill, _objCharacter,
                                            blnIsKnowledgeSkill);
                                        try
                                        {
                                            string strKey = objSkill.DictionaryKey;
                                            if (_dicSkills.TryAdd(strKey, objSkill))
                                                _lstSkills.Add(objSkill);
                                            else if (_dicSkills.TryGetValue(strKey, out Skill objExistingSkill))
                                                MergeSkills(objExistingSkill, objSkill);
                                            else
                                            {
                                                Utils.BreakIfDebug();
                                                objSkill.Remove();
                                            }
                                        }
                                        catch
                                        {
                                            objSkill?.Remove();
                                            throw;
                                        }
                                    }
                                }
                            }

                            _lstSkills.Sort(CompareSkills);
                        }
                        finally
                        {
                            _lstSkillGroups.RaiseListChangedEvents = true;
                            _lstSkills.RaiseListChangedEvents = true;
                            _lstSkillGroups.LockObject.SetParent(LockObject);
                            _lstSkills.LockObject.SetParent(LockObject);
                        }

                        _blnSkillsInitialized = true;
                    }

                    return _lstSkills;
                }
            }
        }

        /// <summary>
        /// Active Skills
        /// </summary>
        public async Task<ThreadSafeBindingList<Skill>> GetSkillsAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await _objSkillsInitializerLock.EnterReadLockAsync(token)
                .ConfigureAwait(false);
            try
            {
                if (_blnSkillsInitialized ||
                    await _objCharacter.GetSkillsSectionAsync(token).ConfigureAwait(false) != this)
                    return _lstSkills;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            objLocker = await _objSkillsInitializerLock.EnterUpgradeableReadLockAsync(token)
                .ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnSkillsInitialized ||
                    await _objCharacter.GetSkillsSectionAsync(token).ConfigureAwait(false) != this)
                    return _lstSkills;
                IAsyncDisposable objLocker2
                    = await _objSkillsInitializerLock.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    XmlDocument xmlSkillsDocument = await _objCharacter
                        .LoadDataAsync("skills.xml", token: token)
                        .ConfigureAwait(false);
                    await _lstSkills.LockObject.SetParentAsync(token: token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        await _lstSkillGroups.LockObject.SetParentAsync(token: token).ConfigureAwait(false);
                        try
                        {
                            token.ThrowIfCancellationRequested();
                            _lstSkills.RaiseListChangedEvents = false;
                            _lstSkillGroups.RaiseListChangedEvents = false;
                            try
                            {
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
                                                = xmlSkillsDocument
                                                      .SelectSingleNodeAndCacheExpressionAsNavigator(
                                                          "/chummer/categories/category[. = "
                                                          + xmlSkill["category"]?.InnerText.CleanXPath()
                                                          + "]/@type", token)
                                                      ?.Value
                                                  != "active";
                                            Skill objSkill = await Skill.FromDataAsync(xmlSkill, _objCharacter,
                                                blnIsKnowledgeSkill, token).ConfigureAwait(false);
                                            try
                                            {
                                                string strKey = await objSkill.GetDictionaryKeyAsync(token)
                                                    .ConfigureAwait(false);
                                                if (_dicSkills.TryAdd(strKey, objSkill))
                                                {
                                                    await _lstSkills.AddAsync(objSkill, token).ConfigureAwait(false);
                                                }
                                                else if (_dicSkills.TryGetValue(strKey,
                                                             out Skill objExistingSkill))
                                                    await MergeSkillsAsync(objExistingSkill,
                                                        objSkill, token).ConfigureAwait(false);
                                                else
                                                {
                                                    Utils.BreakIfDebug();
                                                    await objSkill.RemoveAsync(CancellationToken.None).ConfigureAwait(false);
                                                }
                                            }
                                            catch
                                            {
                                                await objSkill.RemoveAsync(CancellationToken.None).ConfigureAwait(false);
                                                throw;
                                            }
                                        }
                                    }
                                }

                                await _lstSkills.SortAsync(CompareSkills, token).ConfigureAwait(false);
                            }
                            finally
                            {
                                _lstSkillGroups.RaiseListChangedEvents = true;
                                _lstSkills.RaiseListChangedEvents = true;
                            }
                        }
                        finally
                        {
                            await _lstSkillGroups.LockObject.SetParentAsync(LockObject, token: token)
                                .ConfigureAwait(false);
                        }
                    }
                    finally
                    {
                        await _lstSkills.LockObject.SetParentAsync(LockObject, token: token).ConfigureAwait(false);
                    }

                    _blnSkillsInitialized = true;
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }

                return _lstSkills;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
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
        public async Task<bool> HasActiveSkillAsync(string strSkillKey, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _dicSkills.ContainsKey(strSkillKey);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
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
                _dicSkills.TryGetValue(strSkillName, out Skill objReturn);
                return objReturn;
            }
        }

        /// <summary>
        /// Gets an active skill by its Name. Returns null if none found.
        /// </summary>
        /// <param name="strSkillName">Name of the skill.</param>
        /// <param name="token">CancellationToken to listen to.</param>
        public async Task<Skill> GetActiveSkillAsync(string strSkillName, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _dicSkills.TryGetValue(strSkillName, out Skill objReturn);
                return objReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
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

        private readonly ThreadSafeBindingList<KnowledgeSkill> _lstKnowledgeSkills;
        private readonly ThreadSafeBindingList<KnowledgeSkill> _lstKnowsoftSkills;
        private readonly ThreadSafeBindingList<SkillGroup> _lstSkillGroups;

        public ThreadSafeBindingList<KnowledgeSkill> KnowledgeSkills
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _lstKnowledgeSkills;
            }
        }

        public async Task<ThreadSafeBindingList<KnowledgeSkill>> GetKnowledgeSkillsAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstKnowledgeSkills;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
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
        public async Task<ThreadSafeBindingList<KnowledgeSkill>> GetKnowsoftSkillsAsync(
            CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstKnowsoftSkills;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
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
        public async Task<ThreadSafeBindingList<SkillGroup>> GetSkillGroupsAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstSkillGroups;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public bool HasKnowledgePoints => KnowledgeSkillPoints > 0;

        public async Task<bool> GetHasKnowledgePointsAsync(CancellationToken token = default) =>
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

        public async Task<bool> GetHasAvailableNativeLanguageSlotsAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await (await GetKnowledgeSkillsAsync(token).ConfigureAwait(false))
                           .CountAsync(x => x.GetIsNativeLanguageAsync(token), token).ConfigureAwait(false)
                       < 1 + await ImprovementManager
                           .ValueOfAsync(_objCharacter, Improvement.ImprovementType.NativeLanguageLimit,
                               token: token)
                           .ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private int _intCachedKnowledgePoints = int.MinValue;

        private readonly AsyncFriendlyReaderWriterLock _objCachedKnowledgePointsLock;

        /// <summary>
        /// Number of free Knowledge Skill Points the character has.
        /// </summary>
        public int KnowledgeSkillPoints
        {
            get
            {
                using (_objCachedKnowledgePointsLock.EnterReadLock())
                {
                    if (_intCachedKnowledgePoints != int.MinValue)
                        return _intCachedKnowledgePoints;
                }

                using (_objCachedKnowledgePointsLock.EnterUpgradeableReadLock())
                {
                    if (_intCachedKnowledgePoints != int.MinValue)
                        return _intCachedKnowledgePoints;
                    using (_objCachedKnowledgePointsLock.EnterWriteLock())
                    {
                        string strExpression = _objCharacter.Settings.KnowledgePointsExpression;
                        if (strExpression.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
                        {
                            strExpression = _objCharacter.ProcessAttributesInXPath(strExpression);
                            // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                            (bool blnIsSuccess, object objProcess)
                                = CommonFunctions.EvaluateInvariantXPath(
                                    strExpression);
                            _intCachedKnowledgePoints
                                = blnIsSuccess ? ((double)objProcess).StandardRound() : 0;
                        }
                        else
                            _intCachedKnowledgePoints = decValue.StandardRound();

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
        /// Number of free Knowledge Skill Points the character has.
        /// </summary>
        public async Task<int> GetKnowledgeSkillPointsAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await _objCachedKnowledgePointsLock.EnterReadLockAsync(token)
                .ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intCachedKnowledgePoints != int.MinValue)
                    return _intCachedKnowledgePoints;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            objLocker = await _objCachedKnowledgePointsLock.EnterUpgradeableReadLockAsync(token)
                .ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intCachedKnowledgePoints != int.MinValue)
                    return _intCachedKnowledgePoints;
                IAsyncDisposable objLocker2 = await _objCachedKnowledgePointsLock.EnterWriteLockAsync(token)
                    .ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    string strExpression = await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetKnowledgePointsExpressionAsync(token).ConfigureAwait(false);
                    if (strExpression.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
                    {
                        strExpression = await _objCharacter
                                .ProcessAttributesInXPathAsync(strExpression, token: token).ConfigureAwait(false);
                        // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                        (bool blnIsSuccess, object objProcess)
                            = await CommonFunctions.EvaluateInvariantXPathAsync(
                                strExpression, token).ConfigureAwait(false);
                        _intCachedKnowledgePoints
                            = blnIsSuccess ? ((double)objProcess).StandardRound() : 0;
                    }
                    else
                        _intCachedKnowledgePoints = decValue.StandardRound();

                    _intCachedKnowledgePoints += (await ImprovementManager
                            .ValueOfAsync(_objCharacter,
                                Improvement.ImprovementType.FreeKnowledgeSkills, token: token)
                            .ConfigureAwait(false))
                        .StandardRound();
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }

                return _intCachedKnowledgePoints;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
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
        public async Task<int> GetKnowledgeSkillPointsRemainAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await GetKnowledgeSkillPointsAsync(token).ConfigureAwait(false) - await GetKnowledgeSkillPointsUsedAsync(token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
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
        public async Task<int> GetKnowledgeSkillPointsUsedAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await GetKnowledgeSkillRanksSumAsync(token).ConfigureAwait(false) - await GetSkillPointsSpentOnKnoskillsAsync(token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
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
        public async Task<int> GetKnowledgeSkillRanksSumAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await (await GetKnowledgeSkillsAsync(token).ConfigureAwait(false)).SumAsync(x => x.GetCurrentSpCostAsync(token), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
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
        public async Task<int> GetSkillPointsSpentOnKnoskillsAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                //Even if it is stupid, you can spend real skill points on knoskills...
                if (!await _objCharacter.GetEffectiveBuildMethodUsesPriorityTablesAsync(token).ConfigureAwait(false))
                {
                    return 0;
                }

                int intReturn = await GetKnowledgeSkillRanksSumAsync(token).ConfigureAwait(false) - await GetKnowledgeSkillPointsAsync(token).ConfigureAwait(false);
                return Math.Max(intReturn, 0);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
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
        public async Task<int> GetSkillPointsAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (SkillPointsMaximum == 0)
                {
                    return 0;
                }

                return SkillPointsMaximum
                       - await (await GetSkillsAsync(token).ConfigureAwait(false)).SumAsync(x => x.GetCurrentSpCostAsync(token),
                                                                      token).ConfigureAwait(false)
                       - await GetSkillPointsSpentOnKnoskillsAsync(token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
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
                using (LockObject.EnterUpgradeableReadLock())
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
        public async Task<int> GetSkillGroupPointsAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return SkillGroupPointsMaximum - await SkillGroups.SumAsync(x => x.Base - x.FreeBase, token: token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
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
                using (LockObject.EnterUpgradeableReadLock())
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
            Skill objLhsParent = lhs.Parent;
            Skill objRhsParent = rhs.Parent;
            if (objLhsParent != objRhsParent)
                return CompareSkills(objRhsParent, objLhsParent);
            bool blnLhsFree = lhs.Free;
            if (blnLhsFree != rhs.Free)
                return blnLhsFree ? 1 : -1;
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

        public static async Task<int> CompareSpecializationsAsync(SkillSpecialization lhs, SkillSpecialization rhs, CancellationToken token = default)
        {
            if (lhs == null)
                return rhs == null ? 0 : 1;
            if (rhs == null)
                return -1;
            Skill objLhsParent = await lhs.GetParentAsync(token).ConfigureAwait(false);
            Skill objRhsParent = await lhs.GetParentAsync(token).ConfigureAwait(false);
            if (objLhsParent != objRhsParent)
                return await CompareSkillsAsync(objRhsParent, objLhsParent, token).ConfigureAwait(false);
            bool blnLhsFree = await lhs.GetFreeAsync(token).ConfigureAwait(false);
            if (blnLhsFree != await rhs.GetFreeAsync(token).ConfigureAwait(false))
                return blnLhsFree ? 1 : -1;
            return string.Compare(await lhs.GetCurrentDisplayNameAsync(token).ConfigureAwait(false),
                                  await rhs.GetCurrentDisplayNameAsync(token).ConfigureAwait(false), false,
                                  GlobalSettings.CultureInfo);
        }

        public static async Task<int> CompareSkillsAsync(Skill rhs, Skill lhs, CancellationToken token = default)
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

        public static async Task<int> CompareSkillGroupsAsync(SkillGroup objXGroup, SkillGroup objYGroup, CancellationToken token = default)
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
            using (objExistingSkill.LockObject.EnterUpgradeableReadLock(token))
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

        private static async Task MergeSkillsAsync(Skill objExistingSkill, Skill objNewSkill,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await objExistingSkill.LockObject.EnterUpgradeableReadLockAsync(token)
                .ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                objExistingSkill.CopyInternalId(objNewSkill);
                int intExistingBasePoints = await objExistingSkill.GetBasePointsAsync(token).ConfigureAwait(false);
                int intNewBasePoints = await objNewSkill.GetBasePointsAsync(token).ConfigureAwait(false);
                if (intExistingBasePoints < intNewBasePoints)
                    await objExistingSkill.SetBasePointsAsync(intNewBasePoints, token).ConfigureAwait(false);
                int intExistingKarmaPoints =
                    await objExistingSkill.GetKarmaPointsAsync(token).ConfigureAwait(false);
                int intNewKarmaPoints = await objNewSkill.GetKarmaPointsAsync(token).ConfigureAwait(false);
                if (intExistingKarmaPoints < intNewKarmaPoints)
                    await objExistingSkill.SetKarmaPointsAsync(intNewKarmaPoints, token).ConfigureAwait(false);
                await objExistingSkill
                    .SetBuyWithKarmaAsync(await objNewSkill.GetBuyWithKarmaAsync(token).ConfigureAwait(false),
                        token)
                    .ConfigureAwait(false);
                await objExistingSkill.SetNotesAsync(
                    await objExistingSkill.GetNotesAsync(token).ConfigureAwait(false)
                    + await objNewSkill.GetNotesAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objExistingSkill.SetNotesColorAsync(await objNewSkill.GetNotesColorAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
                await (await objExistingSkill.GetSpecializationsAsync(token).ConfigureAwait(false))
                    .AddAsyncRangeWithSortAsync(await objNewSkill.GetSpecializationsAsync(token).ConfigureAwait(false),
                        (x, y) => CompareSpecializationsAsync(x, y, token)
                        ,
                        token: token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            await objNewSkill.RemoveAsync(token).ConfigureAwait(false);
        }

        private List<ListItem> _lstDefaultKnowledgeSkills;
        private readonly AsyncFriendlyReaderWriterLock _objDefaultKnowledgeSkillsLock;

        public IReadOnlyList<ListItem> MyDefaultKnowledgeSkills
        {
            get
            {
                if (GlobalSettings.LiveCustomData)
                {
                    List<ListItem> lstReturn = new List<ListItem>();
                    XPathNavigator xmlSkillsDocument = _objCharacter.LoadDataXPath("skills.xml");
                    foreach (XPathNavigator xmlSkill in xmlSkillsDocument.SelectAndCacheExpression(
                                 "/chummer/knowledgeskills/skill"))
                    {
                        string strName = xmlSkill.SelectSingleNodeAndCacheExpression("name")?.Value ?? string.Empty;
                        lstReturn.Add(
                            new ListItem(
                                strName,
                                xmlSkill.SelectSingleNodeAndCacheExpression("translate")?.Value ?? strName));
                    }

                    lstReturn.Sort(CompareListItems.CompareNames);
                    return lstReturn;
                }

                using (_objDefaultKnowledgeSkillsLock.EnterReadLock())
                {
                    if (_lstDefaultKnowledgeSkills != null)
                        return _lstDefaultKnowledgeSkills;
                }

                using (_objDefaultKnowledgeSkillsLock.EnterUpgradeableReadLock())
                {
                    if (_lstDefaultKnowledgeSkills != null)
                        return _lstDefaultKnowledgeSkills;

                    using (_objDefaultKnowledgeSkillsLock.EnterWriteLock())
                    {
                        _lstDefaultKnowledgeSkills = new List<ListItem>();
                        XPathNavigator xmlSkillsDocument = _objCharacter.LoadDataXPath("skills.xml");
                        foreach (XPathNavigator xmlSkill in xmlSkillsDocument.SelectAndCacheExpression(
                                     "/chummer/knowledgeskills/skill"))
                        {
                            string strName = xmlSkill.SelectSingleNodeAndCacheExpression("name")?.Value ??
                                             string.Empty;
                            _lstDefaultKnowledgeSkills.Add(
                                new ListItem(
                                    strName,
                                    xmlSkill.SelectSingleNodeAndCacheExpression("translate")?.Value ?? strName));
                        }

                        _lstDefaultKnowledgeSkills.Sort(CompareListItems.CompareNames);
                        return _lstDefaultKnowledgeSkills;
                    }
                }
            }
        }

        public async Task<IReadOnlyList<ListItem>> GetMyDefaultKnowledgeSkillsAsync(CancellationToken token = default)
        {
            if (GlobalSettings.LiveCustomData)
            {
                List<ListItem> lstReturn = new List<ListItem>();
                XPathNavigator xmlSkillsDocument =
                    await _objCharacter.LoadDataXPathAsync("skills.xml", token: token).ConfigureAwait(false);
                foreach (XPathNavigator xmlSkill in xmlSkillsDocument.SelectAndCacheExpression(
                             "/chummer/knowledgeskills/skill", token))
                {
                    string strName = xmlSkill.SelectSingleNodeAndCacheExpression("name", token)?.Value ?? string.Empty;
                    lstReturn.Add(
                        new ListItem(
                            strName,
                            xmlSkill.SelectSingleNodeAndCacheExpression("translate", token)?.Value ?? strName));
                }

                lstReturn.Sort(CompareListItems.CompareNames);
                return lstReturn;
            }

            IAsyncDisposable objLocker = await _objDefaultKnowledgeSkillsLock.EnterReadLockAsync(token)
                .ConfigureAwait(false);
            try
            {
                if (_lstDefaultKnowledgeSkills != null)
                    return _lstDefaultKnowledgeSkills;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            objLocker = await _objDefaultKnowledgeSkillsLock.EnterUpgradeableReadLockAsync(token)
                .ConfigureAwait(false);
            try
            {
                if (_lstDefaultKnowledgeSkills != null)
                    return _lstDefaultKnowledgeSkills;

                IAsyncDisposable objLocker2 =
                    await _objDefaultKnowledgeSkillsLock.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    _lstDefaultKnowledgeSkills = new List<ListItem>();
                    XPathNavigator xmlSkillsDocument =
                        await _objCharacter.LoadDataXPathAsync("skills.xml", token: token).ConfigureAwait(false);
                    foreach (XPathNavigator xmlSkill in xmlSkillsDocument.SelectAndCacheExpression(
                                 "/chummer/knowledgeskills/skill", token))
                    {
                        string strName = xmlSkill.SelectSingleNodeAndCacheExpression("name", token)?.Value ??
                                         string.Empty;
                        _lstDefaultKnowledgeSkills.Add(
                            new ListItem(
                                strName,
                                xmlSkill.SelectSingleNodeAndCacheExpression("translate", token)?.Value ?? strName));
                    }

                    _lstDefaultKnowledgeSkills.Sort(CompareListItems.CompareNames);
                    return _lstDefaultKnowledgeSkills;
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private List<ListItem> _lstKnowledgeTypes;
        private readonly AsyncFriendlyReaderWriterLock _objKnowledgeTypesLock;
        private int _intSkillGroupPointsMaximum;
        private int _intSkillPointsMaximum;

        public IReadOnlyList<ListItem> MyKnowledgeTypes
        {
            get
            {
                if (GlobalSettings.LiveCustomData)
                {
                    List<ListItem> lstReturn = new List<ListItem>();
                    XPathNavigator xmlSkillsDocument = _objCharacter.LoadDataXPath("skills.xml");
                    foreach (XPathNavigator objXmlCategory in xmlSkillsDocument.SelectAndCacheExpression(
                                 "/chummer/categories/category[@type = \"knowledge\"]"))
                    {
                        string strInnerText = objXmlCategory.Value;
                        lstReturn.Add(new ListItem(strInnerText,
                            objXmlCategory
                                .SelectSingleNodeAndCacheExpression("@translate")
                                ?.Value ?? strInnerText));
                    }

                    lstReturn.Sort(CompareListItems.CompareNames);
                    return lstReturn;
                }

                using (_objKnowledgeTypesLock.EnterReadLock())
                {
                    if (_lstKnowledgeTypes != null)
                        return _lstKnowledgeTypes;
                }

                using (_objKnowledgeTypesLock.EnterUpgradeableReadLock())
                {
                    if (_lstKnowledgeTypes != null)
                        return _lstKnowledgeTypes;

                    using (_objKnowledgeTypesLock.EnterWriteLock())
                    {
                        _lstKnowledgeTypes = new List<ListItem>();
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
                        return _lstKnowledgeTypes;
                    }
                }
            }
        }

        public async Task<IReadOnlyList<ListItem>> GetMyKnowledgeTypesAsync(CancellationToken token = default)
        {
            if (GlobalSettings.LiveCustomData)
            {
                List<ListItem> lstReturn = new List<ListItem>();
                XPathNavigator xmlSkillsDocument =
                    await _objCharacter.LoadDataXPathAsync("skills.xml", token: token).ConfigureAwait(false);
                foreach (XPathNavigator objXmlCategory in xmlSkillsDocument.SelectAndCacheExpression(
                             "/chummer/categories/category[@type = \"knowledge\"]", token))
                {
                    string strInnerText = objXmlCategory.Value;
                    lstReturn.Add(new ListItem(strInnerText,
                        objXmlCategory
                            .SelectSingleNodeAndCacheExpression("@translate", token)
                            ?.Value ?? strInnerText));
                }

                lstReturn.Sort(CompareListItems.CompareNames);
                return lstReturn;
            }

            IAsyncDisposable objLocker = await _objKnowledgeTypesLock.EnterReadLockAsync(token)
                .ConfigureAwait(false);
            try
            {
                if (_lstKnowledgeTypes != null)
                    return _lstKnowledgeTypes;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            objLocker = await _objDefaultKnowledgeSkillsLock.EnterUpgradeableReadLockAsync(token)
                .ConfigureAwait(false);
            try
            {
                if (_lstKnowledgeTypes != null)
                    return _lstKnowledgeTypes;

                IAsyncDisposable objLocker2 =
                    await _objDefaultKnowledgeSkillsLock.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    _lstKnowledgeTypes = new List<ListItem>();
                    XPathNavigator xmlSkillsDocument =
                        await _objCharacter.LoadDataXPathAsync("skills.xml", token: token).ConfigureAwait(false);
                    foreach (XPathNavigator objXmlCategory in xmlSkillsDocument.SelectAndCacheExpression(
                                 "/chummer/categories/category[@type = \"knowledge\"]", token))
                    {
                        string strInnerText = objXmlCategory.Value;
                        _lstKnowledgeTypes.Add(new ListItem(strInnerText,
                            objXmlCategory
                                .SelectSingleNodeAndCacheExpression("@translate", token)
                                ?.Value ?? strInnerText));
                    }

                    _lstKnowledgeTypes.Sort(CompareListItems.CompareNames);
                    return _lstKnowledgeTypes;
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
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

        public event PropertyChangedEventHandler PropertyChanged;

        private readonly ConcurrentHashSet<PropertyChangedAsyncEventHandler> _setPropertyChangedAsync =
            new ConcurrentHashSet<PropertyChangedAsyncEventHandler>();

        public event PropertyChangedAsyncEventHandler PropertyChangedAsync
        {
            add => _setPropertyChangedAsync.TryAdd(value);
            remove => _setPropertyChangedAsync.Remove(value);
        }

        public event MultiplePropertiesChangedEventHandler MultiplePropertiesChanged;

        private readonly ConcurrentHashSet<MultiplePropertiesChangedAsyncEventHandler> _setMultiplePropertiesChangedAsync =
            new ConcurrentHashSet<MultiplePropertiesChangedAsyncEventHandler>();

        public event MultiplePropertiesChangedAsyncEventHandler MultiplePropertiesChangedAsync
        {
            add => _setMultiplePropertiesChangedAsync.TryAdd(value);
            remove => _setMultiplePropertiesChangedAsync.Remove(value);
        }

        public async Task Print(XmlWriter objWriter, CultureInfo objCulture, string strLanguageToPrint, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
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
                        await objSkillGroup.Print(objWriter, objCulture, strLanguageToPrint, token)
                            .ConfigureAwait(false);
                    }
                }

                foreach (KnowledgeSkill objSkill in KnowledgeSkills)
                {
                    await objSkill.Print(objWriter, objCulture, strLanguageToPrint, token).ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
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
                                                                          dicValueOverrides != null && dicValueOverrides.TryGetValue(strSkillKey, out int intOverride)
                                                                              ? intOverride
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
                                                             dicValueOverrides != null && dicValueOverrides.TryGetValue(strSkillKey, out int intOverride)
                                                                 ? intOverride
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
                if (_objCharacter != null)
                {
                    if (!_objCharacter.IsDisposed)
                    {
                        try
                        {
                            _objCharacter.PropertyChangedAsync -= OnCharacterPropertyChanged;
                        }
                        catch (ObjectDisposedException)
                        {
                            //swallow this
                        }
                    }

                    CharacterSettings objSettings = _objCharacter.Settings;
                    if (objSettings?.IsDisposed == false)
                    {
                        try
                        {
                            objSettings.MultiplePropertiesChangedAsync -= OnCharacterSettingsPropertyChanged;
                        }
                        catch (ObjectDisposedException)
                        {
                            //swallow this
                        }
                    }
                }
                _lstSkillGroups.ForEach(x => x.Dispose());
                List<Skill> lstSkillBackups = _dicSkillBackups.GetValuesToListSafe();
                _dicSkillBackups.Clear();
                foreach (Skill objSkill in lstSkillBackups)
                    objSkill.Dispose();
                _dicSkillBackups.Clear();
                _lstSkills.ForEach(x => x.Dispose());
                _lstSkills.Dispose();
                _lstKnowledgeSkills.ForEach(x => x.Dispose());
                _lstKnowledgeSkills.Dispose();
                _lstKnowsoftSkills.Clear();
                _lstKnowsoftSkills.Dispose();
                _lstSkillGroups.Dispose();
                _objSkillsInitializerLock.Dispose();
                _objCachedKnowledgePointsLock.Dispose();
                using (_objDefaultKnowledgeSkillsLock.EnterWriteLock())
                {
                    if (_lstDefaultKnowledgeSkills != null)
                        Utils.ListItemListPool.Return(ref _lstDefaultKnowledgeSkills);
                }
                _objDefaultKnowledgeSkillsLock.Dispose();
                using (_objKnowledgeTypesLock.EnterWriteLock())
                {
                    if (_lstKnowledgeTypes != null)
                        Utils.ListItemListPool.Return(ref _lstKnowledgeTypes);
                }
                _objKnowledgeTypesLock.Dispose();
                using (_objKnowledgeSkillCategoriesMapLock.EnterWriteLock())
                {
                    _dicKnowledgeSkillCategoriesMap = null;
                }
                _objKnowledgeSkillCategoriesMapLock.Dispose();
            }
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                if (_objCharacter != null)
                {
                    if (!_objCharacter.IsDisposed)
                    {
                        try
                        {
                            _objCharacter.PropertyChangedAsync -= OnCharacterPropertyChanged;
                        }
                        catch (ObjectDisposedException)
                        {
                            //swallow this
                        }
                    }

                    CharacterSettings objSettings = await _objCharacter.GetSettingsAsync().ConfigureAwait(false);
                    if (objSettings?.IsDisposed == false)
                    {
                        try
                        {
                            objSettings.MultiplePropertiesChangedAsync -= OnCharacterSettingsPropertyChanged;
                        }
                        catch (ObjectDisposedException)
                        {
                            //swallow this
                        }
                    }
                }
                await _lstSkillGroups.ForEachWithSideEffectsAsync(async x => await x.DisposeAsync().ConfigureAwait(false)).ConfigureAwait(false);
                List<Skill> lstSkillBackups = _dicSkillBackups.GetValuesToListSafe();
                _dicSkillBackups.Clear();
                foreach (Skill objSkill in lstSkillBackups)
                    await objSkill.DisposeAsync().ConfigureAwait(false);
                await _lstSkills.ForEachWithSideEffectsAsync(async x => await x.DisposeAsync().ConfigureAwait(false)).ConfigureAwait(false);
                await _lstSkills.DisposeAsync().ConfigureAwait(false);
                await _lstKnowledgeSkills.ForEachWithSideEffectsAsync(async x => await x.DisposeAsync().ConfigureAwait(false)).ConfigureAwait(false);
                await _lstKnowledgeSkills.DisposeAsync().ConfigureAwait(false);
                await _lstKnowsoftSkills.ClearAsync().ConfigureAwait(false);
                await _lstKnowsoftSkills.DisposeAsync().ConfigureAwait(false);
                await _lstSkillGroups.DisposeAsync().ConfigureAwait(false);
                await _objSkillsInitializerLock.DisposeAsync().ConfigureAwait(false);
                await _objCachedKnowledgePointsLock.DisposeAsync().ConfigureAwait(false);
                IAsyncDisposable objLocker2 = await _objDefaultKnowledgeSkillsLock.EnterWriteLockAsync().ConfigureAwait(false);
                try
                {
                    if (_lstDefaultKnowledgeSkills != null)
                        Utils.ListItemListPool.Return(ref _lstDefaultKnowledgeSkills);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
                await _objDefaultKnowledgeSkillsLock.DisposeAsync().ConfigureAwait(false);
                objLocker2 = await _objKnowledgeTypesLock.EnterWriteLockAsync().ConfigureAwait(false);
                try
                {
                    if (_lstKnowledgeTypes != null)
                        Utils.ListItemListPool.Return(ref _lstKnowledgeTypes);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
                await _objKnowledgeTypesLock.DisposeAsync().ConfigureAwait(false);
                objLocker2 = await _objKnowledgeSkillCategoriesMapLock.EnterWriteLockAsync().ConfigureAwait(false);
                try
                {
                    _dicKnowledgeSkillCategoriesMap = null;
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
                await _objKnowledgeSkillCategoriesMapLock.DisposeAsync().ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public AsyncFriendlyReaderWriterLock LockObject { get; }
    }
}
