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
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using Chummer.Annotations;

namespace Chummer.Backend.Skills
{
    [DebuggerDisplay("{_strGroupName} {_intSkillFromSp} {_intSkillFromKarma}")]
    public sealed class SkillGroup : INotifyMultiplePropertiesChangedAsync, IHasInternalId, IHasName, IEquatable<SkillGroup>, IHasLockObject, IHasCharacterObject
    {
        #region Core calculations

        private int _intSkillFromSp;
        private int _intSkillFromKarma;
        private bool _blnIsBroken;

        public AsyncFriendlyReaderWriterLock LockObject { get; }

        public Character CharacterObject => _objCharacter; //readonly member, no locking required

        public void Dispose()
        {
            using (LockObject.EnterWriteLock())
            {
                if (_objCharacter != null)
                {
                    try
                    {
                        _objCharacter.MultiplePropertiesChangedAsync -= OnCharacterPropertyChanged;
                    }
                    catch (ObjectDisposedException)
                    {
                        //swallow this
                    }

                    try
                    {
                        _objCharacter.Settings.MultiplePropertiesChangedAsync -= OnCharacterSettingsPropertyChanged;
                    }
                    catch (ObjectDisposedException)
                    {
                        //swallow this
                    }
                }

                foreach (Skill objSkill in _lstAffectedSkills)
                {
                    try
                    {
                        objSkill.MultiplePropertiesChangedAsync -= SkillOnMultiplePropertiesChanged;
                    }
                    catch (ObjectDisposedException)
                    {
                        // swallow this
                    }
                }

                _objCachedBaseUnbrokenLock.Dispose();
                _objCachedKarmaUnbrokenLock.Dispose();
                _objCachedIsDisabledLock.Dispose();
                _objCachedHasAnyBreakingSkillsLock.Dispose();
                _objCachedToolTipLock.Dispose();
            }
            LockObject.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                if (_objCharacter != null)
                {
                    try
                    {
                        _objCharacter.MultiplePropertiesChangedAsync -= OnCharacterPropertyChanged;
                    }
                    catch (ObjectDisposedException)
                    {
                        //swallow this
                    }

                    try
                    {
                        _objCharacter.Settings.MultiplePropertiesChangedAsync -= OnCharacterSettingsPropertyChanged;
                    }
                    catch (ObjectDisposedException)
                    {
                        //swallow this
                    }
                }

                foreach (Skill objSkill in _lstAffectedSkills)
                {
                    try
                    {
                        objSkill.MultiplePropertiesChangedAsync -= SkillOnMultiplePropertiesChanged;
                    }
                    catch (ObjectDisposedException)
                    {
                        // swallow this
                    }
                }

                await _objCachedBaseUnbrokenLock.DisposeAsync().ConfigureAwait(false);
                await _objCachedKarmaUnbrokenLock.DisposeAsync().ConfigureAwait(false);
                await _objCachedIsDisabledLock.DisposeAsync().ConfigureAwait(false);
                await _objCachedHasAnyBreakingSkillsLock.DisposeAsync().ConfigureAwait(false);
                await _objCachedToolTipLock.DisposeAsync().ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            await LockObject.DisposeAsync().ConfigureAwait(false);
        }

        public int Base
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return IsDisabled ? 0 : Math.Min(BasePoints + FreeBase, RatingMaximum);
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (!BaseUnbroken)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (!BaseUnbroken)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        //Calculate how far above maximum we are.
                        int intOverMax = value + KarmaPoints + FreeLevels - RatingMaximum;

                        //reduce value by max or 0
                        value -= Math.Max(0, intOverMax);

                        //and save back, cannot go under 0
                        BasePoints = Math.Max(0, value - FreeBase);

                        if (!CharacterObject.Created && ((CharacterObject.Settings.StrictSkillGroupsInCreateMode &&
                                                          !CharacterObject.IgnoreRules)
                                                         || !CharacterObject.Settings.UsePointsOnBrokenGroups))
                        {
                            foreach (Skill skill in SkillList)
                            {
                                skill.BasePoints = 0;
                            }
                        }
                    }
                }
            }
        }

        public async Task<int> GetBaseAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await GetIsDisabledAsync(token).ConfigureAwait(false)
                    ? 0
                    : Math.Min(await GetBasePointsAsync(token).ConfigureAwait(false) + await GetFreeBaseAsync(token).ConfigureAwait(false),
                        await GetRatingMaximumAsync(token).ConfigureAwait(false));
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task SetBaseAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (!await GetBaseUnbrokenAsync(token).ConfigureAwait(false))
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (!await GetBaseUnbrokenAsync(token).ConfigureAwait(false))
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    //Calculate how far above maximum we are.
                    int intOverMax = value + await GetKarmaPointsAsync(token).ConfigureAwait(false) +
                                     await GetFreeLevelsAsync(token).ConfigureAwait(false) -
                                     await GetRatingMaximumAsync(token).ConfigureAwait(false);

                    //reduce value by max or 0
                    value -= Math.Max(0, intOverMax);

                    //and save back, cannot go under 0
                    int intValue = Math.Max(0, value - await GetFreeBaseAsync(token).ConfigureAwait(false));
                    if (Interlocked.Exchange(ref _intSkillFromSp, intValue) != intValue)
                        await OnPropertyChangedAsync(nameof(BasePoints), token).ConfigureAwait(false);

                    CharacterSettings objSettings =
                        await CharacterObject.GetSettingsAsync(token).ConfigureAwait(false);
                    if (!await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false)
                        && ((await objSettings.GetStrictSkillGroupsInCreateModeAsync(token).ConfigureAwait(false)
                             && !await CharacterObject.GetIgnoreRulesAsync(token).ConfigureAwait(false))
                            || !await objSettings.GetUsePointsOnBrokenGroupsAsync(token).ConfigureAwait(false)))
                    {
                        foreach (Skill skill in SkillList)
                        {
                            await skill.SetBasePointsAsync(0, token).ConfigureAwait(false);
                        }
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

        public async Task ModifyBaseAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (!await GetBaseUnbrokenAsync(token).ConfigureAwait(false))
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (!await GetBaseUnbrokenAsync(token).ConfigureAwait(false))
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    //Calculate how far above maximum we are.
                    int intOverMax = value + await GetKarmaPointsAsync(token).ConfigureAwait(false) +
                                     await GetFreeLevelsAsync(token).ConfigureAwait(false) -
                                     await GetRatingMaximumAsync(token).ConfigureAwait(false);

                    //reduce value by max or 0
                    value -= Math.Max(0, intOverMax);

                    //and save back, cannot go under 0
                    int intValue = Math.Max(0, value - await GetFreeBaseAsync(token).ConfigureAwait(false));
                    if (Interlocked.Add(ref _intSkillFromSp, intValue) != intValue)
                        await OnPropertyChangedAsync(nameof(BasePoints), token).ConfigureAwait(false);

                    CharacterSettings objSettings =
                        await CharacterObject.GetSettingsAsync(token).ConfigureAwait(false);
                    if (!await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false)
                        && ((await objSettings.GetStrictSkillGroupsInCreateModeAsync(token).ConfigureAwait(false)
                             && !await CharacterObject.GetIgnoreRulesAsync(token).ConfigureAwait(false))
                            || !await objSettings.GetUsePointsOnBrokenGroupsAsync(token).ConfigureAwait(false)))
                    {
                        foreach (Skill skill in SkillList)
                        {
                            await skill.SetBasePointsAsync(0, token).ConfigureAwait(false);
                        }
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

        public int Karma
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    return IsDisabled
                        ? 0
                        : KarmaUnbroken
                            ? Math.Min(KarmaPoints + FreeLevels, RatingMaximum)
                            : Math.Min(FreeLevels, RatingMaximum);
                }
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (!KarmaUnbroken)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (!KarmaUnbroken)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        //Calculate how far above maximum we are.
                        int intOverMax = value + BasePoints + FreeBase - RatingMaximum;

                        //reduce value by max or 0
                        value -= Math.Max(0, intOverMax);

                        //and save back, cannot go under 0
                        KarmaPoints = Math.Max(0, value - FreeLevels);

                        if (CharacterObject.Settings.StrictSkillGroupsInCreateMode && !CharacterObject.Created &&
                            !CharacterObject.IgnoreRules && Karma > 0)
                        {
                            foreach (Skill skill in SkillList)
                            {
                                skill.KarmaPoints = 0;
                                skill.Specializations.RemoveAll(x => !x.Free);
                            }
                        }
                    }
                }
            }
        }

        public async Task<int> GetKarmaAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (!await GetKarmaUnbrokenAsync(token).ConfigureAwait(false) && KarmaPoints > 0)
                {
                    await SetKarmaPointsAsync(0, token).ConfigureAwait(false);
                }

                return await GetIsDisabledAsync(token).ConfigureAwait(false)
                    ? 0
                    : Math.Min(await GetKarmaPointsAsync(token).ConfigureAwait(false) + await GetFreeLevelsAsync(token).ConfigureAwait(false),
                        await GetRatingMaximumAsync(token).ConfigureAwait(false));
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Amount of skill points bought with karma and bonuses to the skills rating
        /// </summary>
        public async Task SetKarmaAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (!await GetKarmaUnbrokenAsync(token).ConfigureAwait(false))
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (!await GetKarmaUnbrokenAsync(token).ConfigureAwait(false))
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    //Calculate how far above maximum we are.
                    int intOverMax = value + await GetBasePointsAsync(token).ConfigureAwait(false) +
                                     await GetFreeBaseAsync(token).ConfigureAwait(false) -
                                     await GetRatingMaximumAsync(token).ConfigureAwait(false);

                    //reduce value by max or 0
                    value -= Math.Max(0, intOverMax);

                    //and save back, cannot go under 0
                    int intValue = Math.Max(0, value - await GetFreeLevelsAsync(token).ConfigureAwait(false));
                    if (Interlocked.Exchange(ref _intSkillFromKarma, intValue) != intValue)
                        await OnPropertyChangedAsync(nameof(KarmaPoints), token).ConfigureAwait(false);

                    int intGroupKarma = await GetKarmaAsync(token).ConfigureAwait(false);
                    if (intGroupKarma > 0
                        && await (await CharacterObject.GetSettingsAsync(token).ConfigureAwait(false))
                            .GetStrictSkillGroupsInCreateModeAsync(token).ConfigureAwait(false)
                        && !await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false)
                        && !await CharacterObject.GetIgnoreRulesAsync(token).ConfigureAwait(false))
                    {
                        foreach (Skill skill in SkillList)
                        {
                            await skill.SetKarmaPointsAsync(0, token).ConfigureAwait(false);
                            ThreadSafeObservableCollection<SkillSpecialization> lstSpecs
                                = await skill.GetSpecializationsAsync(token).ConfigureAwait(false);
                            foreach (SkillSpecialization objSpecialization in await lstSpecs.ToListAsync(
                                             async x => !await x.GetFreeAsync(token).ConfigureAwait(false), token)
                                         .ConfigureAwait(false))
                            {
                                await lstSpecs.RemoveAsync(objSpecialization, token).ConfigureAwait(false);
                            }
                        }
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

        /// <summary>
        /// Amount of skill points bought with karma and bonuses to the skills rating
        /// </summary>
        public async Task ModifyKarmaAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (!await GetKarmaUnbrokenAsync(token).ConfigureAwait(false))
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (!await GetKarmaUnbrokenAsync(token).ConfigureAwait(false))
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    //Calculate how far above maximum we are.
                    int intOverMax = value + await GetBasePointsAsync(token).ConfigureAwait(false) +
                                     await GetFreeBaseAsync(token).ConfigureAwait(false) -
                                     await GetRatingMaximumAsync(token).ConfigureAwait(false);

                    //reduce value by max or 0
                    value -= Math.Max(0, intOverMax);

                    //and save back, cannot go under 0
                    int intValue = Math.Max(0, value - await GetFreeLevelsAsync(token).ConfigureAwait(false));
                    if (Interlocked.Add(ref _intSkillFromKarma, intValue) != intValue)
                        await OnPropertyChangedAsync(nameof(KarmaPoints), token).ConfigureAwait(false);

                    int intGroupKarma = await GetKarmaAsync(token).ConfigureAwait(false);
                    if (intGroupKarma > 0
                        && await (await CharacterObject.GetSettingsAsync(token).ConfigureAwait(false))
                            .GetStrictSkillGroupsInCreateModeAsync(token).ConfigureAwait(false)
                        && !await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false)
                        && !await CharacterObject.GetIgnoreRulesAsync(token).ConfigureAwait(false))
                    {
                        foreach (Skill skill in SkillList)
                        {
                            await skill.SetKarmaPointsAsync(0, token).ConfigureAwait(false);
                            ThreadSafeObservableCollection<SkillSpecialization> lstSpecs
                                = await skill.GetSpecializationsAsync(token).ConfigureAwait(false);
                            foreach (SkillSpecialization objSpecialization in await lstSpecs.ToListAsync(
                                             async x => !await x.GetFreeAsync(token).ConfigureAwait(false), token)
                                         .ConfigureAwait(false))
                            {
                                await lstSpecs.RemoveAsync(objSpecialization, token).ConfigureAwait(false);
                            }
                        }
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

        /// <summary>
        /// Amount of Base that has been provided by non-Improvement sources.
        /// </summary>
        public int BasePoints
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intSkillFromSp;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    // No need to write lock because interlocked guarantees safety
                    if (Interlocked.Exchange(ref _intSkillFromSp, value) == value)
                        return;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// How many points REALLY are in _base. Better than subclasses calculating Base - FreeBase()
        /// </summary>
        public async Task<int> GetBasePointsAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intSkillFromSp;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// How many points REALLY are in _base. Better than subclasses calculating Base - FreeBase()
        /// </summary>
        public async Task SetBasePointsAsync(int value, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // No need to write lock because interlocked guarantees safety
                if (Interlocked.Exchange(ref _intSkillFromSp, value) == value)
                    return;
                await OnPropertyChangedAsync(nameof(BasePoints), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// How many points REALLY are in _base. Better than subclasses calculating Base - FreeBase()
        /// </summary>
        public async Task ModifyBasePointsAsync(int value, CancellationToken token = default)
        {
            if (value == 0)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                Interlocked.Add(ref _intSkillFromSp, value);
                await OnPropertyChangedAsync(nameof(BasePoints), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Amount of Karma Levels that has been provided by non-Improvement sources.
        /// </summary>
        public int KarmaPoints
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intSkillFromKarma;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    // No need to write lock because interlocked guarantees safety
                    if (Interlocked.Exchange(ref _intSkillFromKarma, value) == value)
                        return;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// How many points REALLY are in _karma Better than subclasses calculating Karma - FreeKarma()
        /// </summary>
        public async Task<int> GetKarmaPointsAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intSkillFromKarma;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// How many points REALLY are in _karma Better than subclasses calculating Karma - FreeKarma()
        /// </summary>
        public async Task SetKarmaPointsAsync(int value, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // No need to write lock because interlocked guarantees safety
                if (Interlocked.Exchange(ref _intSkillFromKarma, value) == value)
                    return;
                await OnPropertyChangedAsync(nameof(KarmaPoints), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// How many points REALLY are in _karma Better than subclasses calculating Karma - FreeKarma()
        /// </summary>
        public async Task ModifyKarmaPointsAsync(int value, CancellationToken token = default)
        {
            if (value == 0)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                Interlocked.Add(ref _intSkillFromKarma, value);
                await OnPropertyChangedAsync(nameof(KarmaPoints), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private int _intCachedBaseUnbroken = int.MinValue;
        private readonly AsyncFriendlyReaderWriterLock _objCachedBaseUnbrokenLock;

        /// <summary>
        /// Is it possible to increment this skill group from points
        /// Inverted to simplify databinding
        /// </summary>
        public bool BaseUnbroken
        {
            get
            {
                using (_objCachedBaseUnbrokenLock.EnterReadLock())
                {
                    if (_intCachedBaseUnbroken >= 0)
                        return _intCachedBaseUnbroken > 0;
                }

                using (_objCachedBaseUnbrokenLock.EnterUpgradeableReadLock())
                {
                    if (_intCachedBaseUnbroken >= 0)
                        return _intCachedBaseUnbroken > 0;

                    using (_objCachedBaseUnbrokenLock.EnterWriteLock())
                    {
                        if (IsDisabled || SkillList.Count == 0 ||
                            !_objCharacter.EffectiveBuildMethodUsesPriorityTables)
                            _intCachedBaseUnbroken = 0;
                        else if (_objCharacter.Settings.StrictSkillGroupsInCreateMode && !_objCharacter.Created)
                            _intCachedBaseUnbroken =
                                (SkillList.All(x => x.BasePoints + x.FreeBase <= 0)
                                 && SkillList.All(x => x.KarmaPoints + x.FreeKarma <= 0)).ToInt32();
                        else if (_objCharacter.Settings.UsePointsOnBrokenGroups)
                            _intCachedBaseUnbroken = KarmaUnbroken.ToInt32();
                        else
                            _intCachedBaseUnbroken = SkillList.All(x => x.BasePoints + x.FreeBase <= 0).ToInt32();
                    }

                    return _intCachedBaseUnbroken > 0;
                }
            }
        }

        /// <summary>
        /// Is it possible to increment this skill group from points
        /// Inverted to simplify databinding
        /// </summary>
        public async Task<bool> GetBaseUnbrokenAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await _objCachedBaseUnbrokenLock.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intCachedBaseUnbroken >= 0)
                    return _intCachedBaseUnbroken > 0;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            objLocker =
                await _objCachedBaseUnbrokenLock.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intCachedBaseUnbroken >= 0)
                    return _intCachedBaseUnbroken > 0;
                IAsyncDisposable objLocker2 = await _objCachedBaseUnbrokenLock.EnterWriteLockAsync(token)
                    .ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    if (await GetIsDisabledAsync(token).ConfigureAwait(false) || SkillList.Count == 0
                                                                              || !await _objCharacter
                                                                                  .GetEffectiveBuildMethodUsesPriorityTablesAsync(
                                                                                      token).ConfigureAwait(false))
                        _intCachedBaseUnbroken = 0;
                    else
                    {
                        CharacterSettings objSettings =
                            await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false);
                        if (await objSettings.GetStrictSkillGroupsInCreateModeAsync(token)
                                .ConfigureAwait(false)
                            && !await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false))
                            _intCachedBaseUnbroken =
                                (await SkillList.AllAsync(
                                     async x => await x.GetBasePointsAsync(token).ConfigureAwait(false) +
                                                await x.GetFreeBaseAsync(token).ConfigureAwait(false) <=
                                                0,
                                     token: token).ConfigureAwait(false)
                                 && await SkillList.AllAsync(
                                     async x => await x.GetKarmaPointsAsync(token).ConfigureAwait(false) +
                                                await x.GetFreeKarmaAsync(token).ConfigureAwait(false) <=
                                                0, token: token).ConfigureAwait(false)).ToInt32();
                        else if (await objSettings.GetUsePointsOnBrokenGroupsAsync(token)
                                     .ConfigureAwait(false))
                            _intCachedBaseUnbroken =
                                (await GetKarmaUnbrokenAsync(token).ConfigureAwait(false)).ToInt32();
                        else
                            _intCachedBaseUnbroken
                                = (await SkillList.AllAsync(
                                    async x => await x.GetBasePointsAsync(token).ConfigureAwait(false) +
                                               await x.GetFreeBaseAsync(token).ConfigureAwait(false) <=
                                               0,
                                    token: token).ConfigureAwait(false)).ToInt32();
                    }
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }

                return _intCachedBaseUnbroken > 0;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private int _intCachedKarmaUnbroken = int.MinValue;
        private readonly AsyncFriendlyReaderWriterLock _objCachedKarmaUnbrokenLock;

        /// <summary>
        /// Is it possible to increment this skill group from karma
        /// Inverted to simplify databinding
        /// </summary>
        public bool KarmaUnbroken
        {
            get
            {
                using (_objCachedKarmaUnbrokenLock.EnterReadLock())
                {
                    if (_intCachedKarmaUnbroken >= 0)
                        return _intCachedKarmaUnbroken > 0;
                }

                using (_objCachedKarmaUnbrokenLock.EnterUpgradeableReadLock())
                {
                    if (_intCachedKarmaUnbroken >= 0)
                        return _intCachedKarmaUnbroken > 0;
                    using (_objCachedKarmaUnbrokenLock.EnterWriteLock())
                    {
                        if (IsDisabled || SkillList.Count == 0)
                            _intCachedKarmaUnbroken = 0;
                        else if (_objCharacter.Settings.StrictSkillGroupsInCreateMode && !_objCharacter.Created)
                            _intCachedKarmaUnbroken = (SkillList.All(x => x.BasePoints + x.FreeBase <= 0)
                                                       && SkillList.All(x => x.KarmaPoints + x.FreeKarma <= 0))
                                .ToInt32();
                        else
                        {
                            int intHigh = SkillList.Max(x => x.BasePoints + x.FreeBase);

                            _intCachedKarmaUnbroken = SkillList.All(x =>
                                x.BasePoints + x.FreeBase
                                             + x.KarmaPoints + x.FreeKarma
                                >= intHigh).ToInt32();
                        }
                    }

                    return _intCachedKarmaUnbroken > 0;
                }
            }
        }

        /// <summary>
        /// Is it possible to increment this skill group from karma
        /// Inverted to simplify databinding
        /// </summary>
        public async Task<bool> GetKarmaUnbrokenAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await _objCachedKarmaUnbrokenLock.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intCachedKarmaUnbroken >= 0)
                    return _intCachedKarmaUnbroken > 0;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            objLocker = await _objCachedKarmaUnbrokenLock.EnterUpgradeableReadLockAsync(token)
                .ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intCachedKarmaUnbroken >= 0)
                    return _intCachedKarmaUnbroken > 0;
                IAsyncDisposable objLocker2 = await _objCachedKarmaUnbrokenLock.EnterWriteLockAsync(token)
                    .ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    if (await GetIsDisabledAsync(token).ConfigureAwait(false) || SkillList.Count == 0)
                        _intCachedKarmaUnbroken = 0;
                    else if (await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false))
                                 .GetStrictSkillGroupsInCreateModeAsync(token).ConfigureAwait(false)
                             && !await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false))
                        _intCachedBaseUnbroken =
                            (await SkillList.AllAsync(
                                 async x =>
                                     await x.GetBasePointsAsync(token).ConfigureAwait(false)
                                     + await x.GetFreeBaseAsync(token).ConfigureAwait(false) <= 0,
                                 token: token).ConfigureAwait(false)
                             && await SkillList.AllAsync(
                                 async x => await x.GetKarmaPointsAsync(token).ConfigureAwait(false) +
                                     await x.GetFreeKarmaAsync(token).ConfigureAwait(false) <= 0,
                                 token: token).ConfigureAwait(false)).ToInt32();
                    else
                    {
                        int intHigh = await SkillList.MaxAsync(
                            async x => await x.GetBasePointsAsync(token).ConfigureAwait(false) +
                                       await x.GetFreeBaseAsync(token).ConfigureAwait(false),
                            token: token).ConfigureAwait(false);

                        _intCachedKarmaUnbroken
                            = (await SkillList.AllAsync(
                                    async x => await x.GetBasePointsAsync(token)
                                                   .ConfigureAwait(false) +
                                               await x.GetFreeBaseAsync(token)
                                                   .ConfigureAwait(false) +
                                               await x.GetKarmaPointsAsync(token)
                                                   .ConfigureAwait(false)
                                               + await x.GetFreeKarmaAsync(token)
                                                   .ConfigureAwait(false)
                                               >= intHigh,
                                    token: token)
                                .ConfigureAwait(false)).ToInt32();
                    }
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }

                return _intCachedKarmaUnbroken > 0;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private int _intCachedIsDisabled = int.MinValue;
        private readonly AsyncFriendlyReaderWriterLock _objCachedIsDisabledLock;

        public bool IsDisabled
        {
            get
            {
                using (_objCachedIsDisabledLock.EnterReadLock())
                {
                    if (_intCachedIsDisabled >= 0)
                        return _intCachedIsDisabled > 0;
                }

                using (_objCachedIsDisabledLock.EnterUpgradeableReadLock())
                {
                    if (_intCachedIsDisabled >= 0)
                        return _intCachedIsDisabled > 0;
                    using (_objCachedIsDisabledLock.EnterWriteLock())
                    {
                        _intCachedIsDisabled = (ImprovementManager
                                                    .GetCachedImprovementListForValueOf(
                                                        _objCharacter,
                                                        Improvement.ImprovementType.SkillGroupDisable,
                                                        Name)
                                                    .Count > 0
                                                || ImprovementManager
                                                    .GetCachedImprovementListForValueOf(
                                                        _objCharacter,
                                                        Improvement.ImprovementType.SkillGroupCategoryDisable)
                                                    .Exists(
                                                        x => GetRelevantSkillCategories
                                                            .Contains(x.ImprovedName))).ToInt32();
                    }

                    return _intCachedIsDisabled > 0;
                }
            }
        }

        public async Task<bool> GetIsDisabledAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await _objCachedIsDisabledLock.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intCachedIsDisabled >= 0)
                    return _intCachedIsDisabled > 0;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            objLocker =
                await _objCachedIsDisabledLock.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intCachedIsDisabled >= 0)
                    return _intCachedIsDisabled > 0;
                IAsyncDisposable objLocker2 =
                    await _objCachedIsDisabledLock.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _intCachedIsDisabled = ((await ImprovementManager
                                                .GetCachedImprovementListForValueOfAsync(
                                                    _objCharacter,
                                                    Improvement.ImprovementType.SkillGroupDisable,
                                                    Name,
                                                    token: token).ConfigureAwait(false))
                                            .Count > 0
                                            || (await ImprovementManager
                                                .GetCachedImprovementListForValueOfAsync(
                                                    _objCharacter,
                                                    Improvement.ImprovementType.SkillGroupCategoryDisable,
                                                    token: token).ConfigureAwait(false))
                                            .Exists(
                                                x => GetRelevantSkillCategories.Contains(x.ImprovedName)))
                        .ToInt32();
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }

                return _intCachedIsDisabled > 0;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Can this skillgroup be increased in career mode?
        /// </summary>
        public bool IsBroken
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnIsBroken;
            }
            private set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnIsBroken == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnIsBroken = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Can this skillgroup be increased in career mode?
        /// </summary>
        public async Task<bool> GetIsBrokenAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnIsBroken;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Can this skillgroup be increased in career mode?
        /// </summary>
        private async Task SetIsBrokenAsync(bool value, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnIsBroken == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnIsBroken = value;
                    await OnPropertyChangedAsync(nameof(IsBroken), token).ConfigureAwait(false);
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

        private void UpdateIsBroken()
        {
            if (!_objCharacter.Created)
                return;
            if (!_objCharacter.Settings.AllowSkillRegrouping)
            {
                using (LockObject.EnterReadLock())
                {
                    if (IsBroken)
                        return;
                }
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (IsBroken)
                        return;
                    IsBroken = HasAnyBreakingSkills;
                }
            }
            using (LockObject.EnterUpgradeableReadLock())
            {
                IsBroken = HasAnyBreakingSkills;
            }
        }

        private async Task UpdateIsBrokenAsync(CancellationToken token = default)
        {
            if (!await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false))
                return;
            if (!(await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).AllowSkillRegrouping)
            {
                IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    if (await GetIsBrokenAsync(token).ConfigureAwait(false))
                        return;
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
                objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    if (await GetIsBrokenAsync(token).ConfigureAwait(false))
                        return;
                    await SetIsBrokenAsync(await GetHasAnyBreakingSkillsAsync(token).ConfigureAwait(false), token)
                        .ConfigureAwait(false);
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
            }
            IAsyncDisposable objLocker2 = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                await SetIsBrokenAsync(await GetHasAnyBreakingSkillsAsync(token).ConfigureAwait(false), token)
                    .ConfigureAwait(false);
            }
            finally
            {
                await objLocker2.DisposeAsync().ConfigureAwait(false);
            }
        }

        private int _intCachedHasAnyBreakingSkills = int.MinValue;
        private readonly AsyncFriendlyReaderWriterLock _objCachedHasAnyBreakingSkillsLock;

        public bool HasAnyBreakingSkills
        {
            get
            {
                using (_objCachedHasAnyBreakingSkillsLock.EnterReadLock())
                {
                    if (_intCachedHasAnyBreakingSkills >= 0)
                        return _intCachedHasAnyBreakingSkills > 0;
                }

                using (_objCachedHasAnyBreakingSkillsLock.EnterUpgradeableReadLock())
                {
                    if (_intCachedHasAnyBreakingSkills >= 0)
                        return _intCachedHasAnyBreakingSkills > 0;
                    using (_objCachedHasAnyBreakingSkillsLock.EnterWriteLock())
                    {
                        if (SkillList.Count <= 1)
                            _intCachedHasAnyBreakingSkills = 0;
                        else
                        {
                            Skill objFirstEnabledSkill = SkillList.Find(x => x.Enabled);
                            if (objFirstEnabledSkill == null ||
                                SkillList.All(x => x == objFirstEnabledSkill || !x.Enabled))
                                _intCachedHasAnyBreakingSkills = 0;
                            else if (_objCharacter.Settings.SpecializationsBreakSkillGroups && SkillList.Any(
                                         x =>
                                             x.Specializations.Count != 0
                                             && x.Enabled))
                            {
                                _intCachedHasAnyBreakingSkills = 1;
                            }
                            else
                            {
                                int intFirstSkillTotalBaseRating = objFirstEnabledSkill.TotalBaseRating;
                                _intCachedHasAnyBreakingSkills = SkillList.Any(x => x != objFirstEnabledSkill
                                    && x.TotalBaseRating
                                    != intFirstSkillTotalBaseRating
                                    && x.Enabled).ToInt32();
                            }
                        }
                    }

                    return _intCachedHasAnyBreakingSkills > 0;
                }
            }
        }

        public async Task<bool> GetHasAnyBreakingSkillsAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await _objCachedHasAnyBreakingSkillsLock.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intCachedHasAnyBreakingSkills >= 0)
                    return _intCachedHasAnyBreakingSkills > 0;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            objLocker = await _objCachedHasAnyBreakingSkillsLock.EnterUpgradeableReadLockAsync(token)
                .ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intCachedHasAnyBreakingSkills >= 0) return _intCachedHasAnyBreakingSkills > 0;
                IAsyncDisposable objLocker2 = await _objCachedHasAnyBreakingSkillsLock
                    .EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    if (SkillList.Count <= 1)
                        _intCachedHasAnyBreakingSkills = 0;
                    else
                    {
                        Skill objFirstEnabledSkill = await SkillList
                            .FirstOrDefaultAsync(
                                x => x.GetEnabledAsync(token), token)
                            .ConfigureAwait(false);
                        if (objFirstEnabledSkill == null ||
                            await SkillList
                                .AllAsync(
                                    async x => x == objFirstEnabledSkill
                                               || !await x.GetEnabledAsync(token).ConfigureAwait(false),
                                    token).ConfigureAwait(false))
                            _intCachedHasAnyBreakingSkills = 0;
                        else if (await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false))
                                     .GetSpecializationsBreakSkillGroupsAsync(token).ConfigureAwait(false)
                                 && await SkillList
                                     .AnyAsync(
                                         async x =>
                                             await (await x.GetSpecializationsAsync(token)
                                                     .ConfigureAwait(false)).GetCountAsync(token)
                                                 .ConfigureAwait(false) != 0
                                             && await x.GetEnabledAsync(token).ConfigureAwait(false),
                                         token).ConfigureAwait(false))
                        {
                            _intCachedHasAnyBreakingSkills = 1;
                        }
                        else
                        {
                            int intFirstSkillTotalBaseRating = await objFirstEnabledSkill
                                .GetTotalBaseRatingAsync(token)
                                .ConfigureAwait(false);
                            _intCachedHasAnyBreakingSkills = (await SkillList.AnyAsync(
                                    async x => x != objFirstEnabledSkill
                                               && await x.GetTotalBaseRatingAsync(token)
                                                   .ConfigureAwait(false)
                                               != intFirstSkillTotalBaseRating
                                               && await x.GetEnabledAsync(token).ConfigureAwait(false),
                                    token)
                                .ConfigureAwait(false)).ToInt32();
                        }
                    }
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }

                return _intCachedHasAnyBreakingSkills > 0;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public bool CareerCanIncrease
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return UpgradeKarmaCost <= CharacterObject.Karma && !IsDisabled && !IsBroken;
            }
        }

        public async Task<bool> GetCareerCanIncreaseAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await GetUpgradeKarmaCostAsync(token).ConfigureAwait(false)
                       <= await CharacterObject.GetKarmaAsync(token).ConfigureAwait(false)
                       && !await GetIsDisabledAsync(token).ConfigureAwait(false)
                       && !await GetIsBrokenAsync(token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public int Rating
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return Karma + Base;
            }
        }

        public async Task<int> GetRatingAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await GetKarmaAsync(token).ConfigureAwait(false) + await GetBaseAsync(token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public int FreeBase
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    return !string.IsNullOrEmpty(Name)
                        ? ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.SkillGroupBase, false,
                                Name)
                            .StandardRound()
                        : 0;
                }
            }
        }

        public async Task<int> GetFreeBaseAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return !string.IsNullOrEmpty(Name)
                    ? (await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.SkillGroupBase,
                        false, Name, token: token).ConfigureAwait(false)).StandardRound()
                    : 0;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public int FreeLevels
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    return !string.IsNullOrEmpty(Name)
                        ? ImprovementManager
                            .ValueOf(_objCharacter, Improvement.ImprovementType.SkillGroupLevel, false, Name)
                            .StandardRound()
                        : 0;
                }
            }
        }

        public async Task<int> GetFreeLevelsAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return !string.IsNullOrEmpty(Name)
                    ? (await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.SkillGroupLevel,
                        false, Name, token: token).ConfigureAwait(false)).StandardRound()
                    : 0;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Maximum possible rating
        /// </summary>
        public int RatingMaximum
        {
            get
            {
                using (_objCharacter.LockObject.EnterReadLock())
                {
                    return _objCharacter.Created || _objCharacter.IgnoreRules
                        ? _objCharacter.Settings.MaxSkillRating
                        : _objCharacter.Settings.MaxSkillRatingCreate;
                }
            }
        }

        /// <summary>
        /// Maximum possible rating
        /// </summary>
        public async Task<int> GetRatingMaximumAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await _objCharacter.LockObject.EnterReadLockAsync(token)
                .ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                CharacterSettings objSettings = await CharacterObject.GetSettingsAsync(token).ConfigureAwait(false);
                return await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false) ||
                       await _objCharacter.GetIgnoreRulesAsync(token).ConfigureAwait(false)
                    ? await objSettings.GetMaxSkillRatingAsync(token).ConfigureAwait(false)
                    : await objSettings.GetMaxSkillRatingCreateAsync(token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task Upgrade(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false))
                {
                    if (await GetIsBrokenAsync(token).ConfigureAwait(false))
                        return;

                    int intPrice = await GetUpgradeKarmaCostAsync(token).ConfigureAwait(false);

                    //If data file contains {4} this crashes but...
                    int intRating = await GetRatingAsync(token).ConfigureAwait(false);
                    string strUpgrade =
                        string.Format(GlobalSettings.CultureInfo, "{0}{4}{1}{4}{2}{4}->{4}{3}",
                                      await LanguageManager.GetStringAsync("String_ExpenseSkillGroup", token: token)
                                                           .ConfigureAwait(false),
                                      await GetCurrentDisplayNameAsync(token).ConfigureAwait(false),
                                      intRating, intRating + 1,
                                      await LanguageManager.GetStringAsync("String_Space", token: token)
                                                           .ConfigureAwait(false));

                    ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
                    objExpense.Create(intPrice * -1, strUpgrade, ExpenseType.Karma, DateTime.Now);
                    objExpense.Undo = new ExpenseUndo().CreateKarma(KarmaExpenseType.ImproveSkillGroup, InternalId);

                    await CharacterObject.ExpenseEntries.AddWithSortAsync(objExpense, token: token)
                                         .ConfigureAwait(false);

                    await CharacterObject.ModifyKarmaAsync(-intPrice, token).ConfigureAwait(false);
                }

                await SetKarmaAsync(await GetKarmaAsync(token).ConfigureAwait(false) + 1, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion Core calculations

        #region All the other stuff that is required

        public static SkillGroup Get(Skill objSkill)
        {
            if (objSkill == null)
                return null;
            using (objSkill.LockObject.EnterReadLock())
            {
                if (objSkill.SkillGroupObject != null)
                    return objSkill.SkillGroupObject;

                if (string.IsNullOrWhiteSpace(objSkill.SkillGroup))
                    return null;
            }

            objSkill.IsLoading = true;
            try
            {
                using (objSkill.LockObject.EnterUpgradeableReadLock())
                {
                    if (objSkill.SkillGroupObject != null)
                        return objSkill.SkillGroupObject;

                    if (string.IsNullOrWhiteSpace(objSkill.SkillGroup))
                        return null;

                    SkillGroup objSkillGroup =
                        objSkill.CharacterObject.SkillsSection.SkillGroups.Find(x => x.Name == objSkill.SkillGroup);
                    if (objSkillGroup != null)
                    {
                        if (!objSkillGroup.SkillList.Contains(objSkill))
                        {
                            objSkillGroup.LockObject.SetParent();
                            try
                            {
                                objSkillGroup.Add(objSkill);
                            }
                            finally
                            {
                                objSkillGroup.LockObject.SetParent(objSkillGroup.CharacterObject.LockObject);
                            }
                        }
                    }
                    else
                    {
                        objSkillGroup = new SkillGroup(objSkill.CharacterObject, objSkill.SkillGroup);
                        objSkillGroup.Add(objSkill);
                        objSkill.CharacterObject.SkillsSection.SkillGroups.AddWithSort(objSkillGroup,
                            SkillsSection.CompareSkillGroups,
                            (objExistingSkillGroup, objNewSkillGroup) =>
                            {
                                foreach (Skill x in objExistingSkillGroup.SkillList.Where(x =>
                                             !objExistingSkillGroup.SkillList.Contains(x)))
                                    objExistingSkillGroup.Add(x);
                                objNewSkillGroup.Dispose();
                            });
                        objSkillGroup.LockObject.SetParent(objSkillGroup.CharacterObject.LockObject);
                    }

                    return objSkillGroup;
                }
            }
            finally
            {
                objSkill.IsLoading = false;
            }
        }

        public static async Task<SkillGroup> GetAsync(Skill objSkill, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objSkill == null)
                return null;
            IAsyncDisposable objLocker = await objSkill.LockObject.EnterReadLockAsync(token)
                .ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (objSkill.SkillGroupObject != null)
                    return objSkill.SkillGroupObject;

                if (string.IsNullOrWhiteSpace(objSkill.SkillGroup))
                    return null;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            await objSkill.SetIsLoadingAsync(true, token).ConfigureAwait(false);
            try
            {
                objLocker = await objSkill.LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    if (objSkill.SkillGroupObject != null)
                        return objSkill.SkillGroupObject;

                    if (string.IsNullOrWhiteSpace(objSkill.SkillGroup))
                        return null;

                    SkillGroup objSkillGroup =
                        await (await objSkill.CharacterObject.SkillsSection.GetSkillGroupsAsync(token)
                                .ConfigureAwait(false))
                            .FindAsync(x => x.Name == objSkill.SkillGroup, token).ConfigureAwait(false);
                    if (objSkillGroup != null)
                    {
                        if (!objSkillGroup.SkillList.Contains(objSkill))
                        {
                            await objSkillGroup.LockObject.SetParentAsync(token: token).ConfigureAwait(false);
                            try
                            {
                                await objSkillGroup.AddAsync(objSkill, token).ConfigureAwait(false);
                            }
                            finally
                            {
                                await objSkillGroup.LockObject.SetParentAsync(objSkillGroup.CharacterObject.LockObject,
                                    token: token).ConfigureAwait(false);
                            }
                        }
                    }
                    else
                    {
                        objSkillGroup = new SkillGroup(objSkill.CharacterObject, objSkill.SkillGroup);
                        await objSkillGroup.AddAsync(objSkill, token).ConfigureAwait(false);
                        await objSkill.CharacterObject.SkillsSection.SkillGroups.AddWithSortAsync(objSkillGroup,
                            (x, y) => SkillsSection.CompareSkillGroupsAsync(x, y, token),
                            async (objExistingSkillGroup, objNewSkillGroup) =>
                            {
                                foreach (Skill x in objExistingSkillGroup.SkillList.Where(x =>
                                             !objExistingSkillGroup.SkillList.Contains(x)))
                                {
                                    await objExistingSkillGroup.AddAsync(x, token).ConfigureAwait(false);
                                }

                                await objNewSkillGroup.DisposeAsync().ConfigureAwait(false);
                            }, token: token).ConfigureAwait(false);
                        await objSkillGroup.LockObject.SetParentAsync(objSkillGroup.CharacterObject.LockObject,
                            token: token).ConfigureAwait(false);
                    }

                    return objSkillGroup;
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objSkill.SetIsLoadingAsync(false, token).ConfigureAwait(false);
            }
        }

        public void Add(Skill skill)
        {
            using (LockObject.EnterReadLock())
            {
                Guid guidAddedSkillId = skill.SkillId;
                // Do not add duplicate skills that we are still in the process of loading
                if (_lstAffectedSkills.Exists(x => x.SkillId == guidAddedSkillId))
                    return;
            }

            using (LockObject.EnterUpgradeableReadLock())
            {
                using (skill.LockObject.EnterUpgradeableReadLock())
                {
                    Guid guidAddedSkillId = skill.SkillId;
                    // Do not add duplicate skills that we are still in the process of loading
                    if (_lstAffectedSkills.Exists(x => x.SkillId == guidAddedSkillId))
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _lstAffectedSkills.Add(skill);
                        skill.MultiplePropertiesChangedAsync += SkillOnMultiplePropertiesChanged;
                        if (!skill.IsLoading && _objCharacter?.SkillsSection?.IsLoading != true)
                            OnPropertyChanged(nameof(SkillList));
                    }
                }
            }
        }

        public async Task AddAsync(Skill skill, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                Guid guidAddedSkillId = await skill.GetSkillIdAsync(token).ConfigureAwait(false);
                // Do not add duplicate skills that we are still in the process of loading
                if (await _lstAffectedSkills
                        .AnyAsync(
                            async x => await x.GetSkillIdAsync(token).ConfigureAwait(false)
                                       == guidAddedSkillId, token: token)
                        .ConfigureAwait(false))
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                IAsyncDisposable objLocker2 =
                    await skill.LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    Guid guidAddedSkillId = await skill.GetSkillIdAsync(token).ConfigureAwait(false);
                    // Do not add duplicate skills that we are still in the process of loading
                    if (await _lstAffectedSkills
                            .AnyAsync(
                                async x => await x.GetSkillIdAsync(token).ConfigureAwait(false)
                                           == guidAddedSkillId, token: token)
                            .ConfigureAwait(false))
                        return;
                    IAsyncDisposable objLocker3 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        _lstAffectedSkills.Add(skill);
                        skill.MultiplePropertiesChangedAsync += SkillOnMultiplePropertiesChanged;
                        if (!skill.IsLoading && _objCharacter?.SkillsSection?.IsLoading != true)
                            await OnPropertyChangedAsync(nameof(SkillList), token).ConfigureAwait(false);
                    }
                    finally
                    {
                        await objLocker3.DisposeAsync().ConfigureAwait(false);
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

        public void Remove(Skill skill)
        {
            using (LockObject.EnterUpgradeableReadLock())
            {
                using (LockObject.EnterWriteLock())
                {
                    if (!_lstAffectedSkills.Remove(skill))
                        return;
                    skill.MultiplePropertiesChangedAsync -= SkillOnMultiplePropertiesChanged;
                    if (!skill.IsLoading && _objCharacter?.SkillsSection?.IsLoading != true)
                        OnPropertyChanged(nameof(SkillList));
                }
            }
        }

        public async Task RemoveAsync(Skill skill, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker =
                await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    if (!_lstAffectedSkills.Remove(skill))
                        return;
                    skill.MultiplePropertiesChangedAsync -= SkillOnMultiplePropertiesChanged;
                    if (!skill.IsLoading && _objCharacter?.SkillsSection?.IsLoading != true)
                        await OnPropertyChangedAsync(nameof(SkillList), token).ConfigureAwait(false);
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

        internal void WriteTo(XmlWriter writer, CancellationToken token = default)
        {
            if (writer == null)
                return;
            using (LockObject.EnterReadLock(token))
            {
                writer.WriteStartElement("group");

                writer.WriteElementString("karma", _intSkillFromKarma.ToString(GlobalSettings.InvariantCultureInfo));
                writer.WriteElementString("base", _intSkillFromSp.ToString(GlobalSettings.InvariantCultureInfo));
                writer.WriteElementString("isbroken", _blnIsBroken.ToString(GlobalSettings.InvariantCultureInfo));
                writer.WriteElementString("id", _guidId.ToString("D", GlobalSettings.InvariantCultureInfo));
                writer.WriteElementString("name", _strGroupName);

                writer.WriteEndElement();
            }
        }

        internal async Task Print(XmlWriter objWriter, CultureInfo objCulture, string strLanguageToPrint, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objWriter == null)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // <skillgroup>
                XmlElementWriteHelper objBaseElement =
                    await objWriter.StartElementAsync("skillgroup", token: token).ConfigureAwait(false);
                try
                {
                    await objWriter.WriteElementStringAsync("guid", InternalId, token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("name",
                            await DisplayNameAsync(strLanguageToPrint, token).ConfigureAwait(false), token: token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("name_english", Name, token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("rating", Rating.ToString(objCulture), token: token)
                        .ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync("ratingmax", RatingMaximum.ToString(objCulture), token: token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("base", Base.ToString(objCulture), token: token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("karma", Karma.ToString(objCulture), token: token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("isbroken",
                        IsBroken.ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                }
                finally
                {
                    // </skillgroup>
                    await objBaseElement.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public void Load(XmlNode xmlNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (xmlNode == null)
                return;
            using (LockObject.EnterWriteLock(token))
            {
                if (xmlNode.TryGetField("id", Guid.TryParse, out Guid g) && g != Guid.Empty)
                    _guidId = g;
                xmlNode.TryGetStringFieldQuickly("name", ref _strGroupName);
                xmlNode.TryGetInt32FieldQuickly("karma", ref _intSkillFromKarma);
                xmlNode.TryGetInt32FieldQuickly("base", ref _intSkillFromSp);
                xmlNode.TryGetBoolFieldQuickly("isbroken", ref _blnIsBroken);
            }
        }

        public void LoadFromHeroLab(XPathNavigator xmlNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (xmlNode == null)
                return;
            using (LockObject.EnterWriteLock(token))
            {
                string strTemp = xmlNode.SelectSingleNodeAndCacheExpression("@name", token)?.Value;
                if (!string.IsNullOrEmpty(strTemp))
                    _strGroupName = strTemp.TrimEndOnce("Group").Trim();
                strTemp = xmlNode.SelectSingleNodeAndCacheExpression("@base", token)?.Value;
                if (!string.IsNullOrEmpty(strTemp) && int.TryParse(strTemp, out int intTemp))
                    _intSkillFromKarma = intTemp;
            }
        }

        private static readonly PropertyDependencyGraph<SkillGroup> s_SkillGroupDependencyGraph =
            new PropertyDependencyGraph<SkillGroup>(
                new DependencyGraphNode<string, SkillGroup>(nameof(DisplayRating),
                    new DependencyGraphNode<string, SkillGroup>(nameof(SkillList)),
                    new DependencyGraphNode<string, SkillGroup>(nameof(IsDisabled),
                        new DependencyGraphNode<string, SkillGroup>(nameof(Name))
                    ),
                    new DependencyGraphNode<string, SkillGroup>(nameof(IsBroken),
                        new DependencyGraphNode<string, SkillGroup>(nameof(HasAnyBreakingSkills), x => !x.IsBroken || x.CharacterObject.Settings.AllowSkillRegrouping, async (x, t) => !await x.GetIsBrokenAsync(t).ConfigureAwait(false) || await
                                (await x.CharacterObject.GetSettingsAsync(t).ConfigureAwait(false)).GetAllowSkillRegroupingAsync(t).ConfigureAwait(false),
                            new DependencyGraphNode<string, SkillGroup>(nameof(SkillList))
                        )
                    ),
                    new DependencyGraphNode<string, SkillGroup>(nameof(Rating),
                        new DependencyGraphNode<string, SkillGroup>(nameof(Karma),
                            new DependencyGraphNode<string, SkillGroup>(nameof(IsDisabled)),
                            new DependencyGraphNode<string, SkillGroup>(nameof(RatingMaximum)),
                            new DependencyGraphNode<string, SkillGroup>(nameof(FreeLevels),
                                new DependencyGraphNode<string, SkillGroup>(nameof(Name))
                            ),
                            new DependencyGraphNode<string, SkillGroup>(nameof(KarmaPoints)),
                            new DependencyGraphNode<string, SkillGroup>(nameof(KarmaUnbroken),
                                new DependencyGraphNode<string, SkillGroup>(nameof(IsDisabled)),
                                new DependencyGraphNode<string, SkillGroup>(nameof(SkillList))
                            )
                        ),
                        new DependencyGraphNode<string, SkillGroup>(nameof(Base),
                            new DependencyGraphNode<string, SkillGroup>(nameof(IsDisabled)),
                            new DependencyGraphNode<string, SkillGroup>(nameof(RatingMaximum)),
                            new DependencyGraphNode<string, SkillGroup>(nameof(FreeBase),
                                new DependencyGraphNode<string, SkillGroup>(nameof(Name))
                            ),
                            new DependencyGraphNode<string, SkillGroup>(nameof(BasePoints))
                        )
                    )
                ),
                new DependencyGraphNode<string, SkillGroup>(nameof(UpgradeToolTip),
                    new DependencyGraphNode<string, SkillGroup>(nameof(SkillList)),
                    new DependencyGraphNode<string, SkillGroup>(nameof(UpgradeKarmaCost),
                        new DependencyGraphNode<string, SkillGroup>(nameof(SkillList)),
                        new DependencyGraphNode<string, SkillGroup>(nameof(IsDisabled)),
                        new DependencyGraphNode<string, SkillGroup>(nameof(Rating)),
                        new DependencyGraphNode<string, SkillGroup>(nameof(Name))
                    )
                ),
                new DependencyGraphNode<string, SkillGroup>(nameof(CareerCanIncrease),
                    new DependencyGraphNode<string, SkillGroup>(nameof(UpgradeKarmaCost)),
                    new DependencyGraphNode<string, SkillGroup>(nameof(IsDisabled)),
                    new DependencyGraphNode<string, SkillGroup>(nameof(IsBroken))
                ),
                new DependencyGraphNode<string, SkillGroup>(nameof(BaseUnbroken),
                    new DependencyGraphNode<string, SkillGroup>(nameof(IsDisabled)),
                    new DependencyGraphNode<string, SkillGroup>(nameof(SkillList)),
                    new DependencyGraphNode<string, SkillGroup>(nameof(KarmaUnbroken), x => x._objCharacter.Settings.UsePointsOnBrokenGroups, async (x, t) => await (await x._objCharacter.GetSettingsAsync(t).ConfigureAwait(false)).GetUsePointsOnBrokenGroupsAsync(t).ConfigureAwait(false))
                ),
                new DependencyGraphNode<string, SkillGroup>(nameof(ToolTip),
                    new DependencyGraphNode<string, SkillGroup>(nameof(SkillList)),
                    new DependencyGraphNode<string, SkillGroup>(nameof(IsDisabled))
                ),
                new DependencyGraphNode<string, SkillGroup>(nameof(CurrentDisplayName),
                    new DependencyGraphNode<string, SkillGroup>(nameof(DisplayName),
                        new DependencyGraphNode<string, SkillGroup>(nameof(Name))
                    )
                ),
                new DependencyGraphNode<string, SkillGroup>(nameof(CurrentSpCost),
                    new DependencyGraphNode<string, SkillGroup>(nameof(BasePoints)),
                    new DependencyGraphNode<string, SkillGroup>(nameof(Name))
                ),
                new DependencyGraphNode<string, SkillGroup>(nameof(CurrentKarmaCost),
                    new DependencyGraphNode<string, SkillGroup>(nameof(KarmaPoints)),
                    new DependencyGraphNode<string, SkillGroup>(nameof(SkillList))
                )
            );

        private async Task SkillOnMultiplePropertiesChanged(object sender, MultiplePropertiesChangedEventArgs e,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<string> lstProperties = new List<string>();
            if (e.PropertyNames.Contains(nameof(Skill.BasePoints)) || e.PropertyNames.Contains(nameof(Skill.FreeBase)))
            {
                if (!await GetIsDisabledAsync(token).ConfigureAwait(false) && SkillList.Count != 0)
                {
                    if (await _objCharacter.GetEffectiveBuildMethodUsesPriorityTablesAsync(token).ConfigureAwait(false))
                    {
                        CharacterSettings objSettings = await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false);
                        if ((await objSettings.GetStrictSkillGroupsInCreateModeAsync(token).ConfigureAwait(false)
                             && !await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false))
                            || !await objSettings.GetUsePointsOnBrokenGroupsAsync(token).ConfigureAwait(false))
                            lstProperties.Add(nameof(BaseUnbroken));
                    }

                    lstProperties.Add(nameof(KarmaUnbroken));
                }
            }
            else if (e.PropertyNames.Contains(nameof(Skill.KarmaPoints)) || e.PropertyNames.Contains(nameof(Skill.FreeKarma)))
            {
                if (!await GetIsDisabledAsync(token).ConfigureAwait(false) && SkillList.Count != 0)
                    lstProperties.Add(nameof(KarmaUnbroken));
            }

            if (e.PropertyNames.Contains(nameof(Skill.TotalBaseRating)) || e.PropertyNames.Contains(nameof(Skill.Enabled)))
            {
                if (e.PropertyNames.Contains(nameof(Skill.Enabled)))
                {
                    lstProperties.Add(nameof(HasAnyBreakingSkills));
                }
                else if (SkillList.Count > 1)
                {
                    Skill objFirstEnabledSkill = await SkillList.FirstOrDefaultAsync(x => x.GetEnabledAsync(token), token: token).ConfigureAwait(false);
                    if (objFirstEnabledSkill != null && await SkillList.AllAsync(async x => x == objFirstEnabledSkill || !await x.GetEnabledAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false))
                    {
                        lstProperties.Add(nameof(HasAnyBreakingSkills));
                    }
                }
                lstProperties.Add(nameof(DisplayRating));
                lstProperties.Add(nameof(UpgradeToolTip));
                lstProperties.Add(nameof(CurrentKarmaCost));
                lstProperties.Add(nameof(UpgradeKarmaCost));
            }
            else if (e.PropertyNames.Contains(nameof(Skill.Specializations)) && await CharacterObject.Settings.GetSpecializationsBreakSkillGroupsAsync(token).ConfigureAwait(false))
            {
                if (SkillList.Count > 1)
                {
                    Skill objFirstEnabledSkill = await SkillList.FirstOrDefaultAsync(x => x.GetEnabledAsync(token), token: token).ConfigureAwait(false);
                    if (objFirstEnabledSkill != null && await SkillList.AllAsync(async x => x == objFirstEnabledSkill || !await x.GetEnabledAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false))
                        lstProperties.Add(nameof(HasAnyBreakingSkills));
                }
            }

            if (lstProperties.Count > 0)
                await OnMultiplePropertiesChangedAsync(lstProperties, token).ConfigureAwait(false);
        }

        private readonly List<Skill> _lstAffectedSkills = new List<Skill>(4);
        private string _strGroupName;
        private readonly Character _objCharacter;

        public SkillGroup(Character objCharacter, string strGroupName = "")
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            LockObject = new AsyncFriendlyReaderWriterLock(); // We need a separate lock so that we can properly disconnect ourselves from the character lock while we are loading data
            _objCachedBaseUnbrokenLock = new AsyncFriendlyReaderWriterLock(LockObject, true);
            _objCachedHasAnyBreakingSkillsLock = new AsyncFriendlyReaderWriterLock(LockObject, true);
            _objCachedIsDisabledLock = new AsyncFriendlyReaderWriterLock(LockObject, true);
            _objCachedKarmaUnbrokenLock = new AsyncFriendlyReaderWriterLock(LockObject, true);
            _objCachedToolTipLock = new AsyncFriendlyReaderWriterLock(LockObject, true);
            _strGroupName = strGroupName;
            objCharacter.MultiplePropertiesChangedAsync += OnCharacterPropertyChanged;
            objCharacter.Settings.MultiplePropertiesChangedAsync += OnCharacterSettingsPropertyChanged;
        }

        public string Name
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strGroupName;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    // No need to write lock because interlocked guarantees safety
                    if (Interlocked.Exchange(ref _strGroupName, value) == value)
                        return;
                    OnPropertyChanged();
                }
            }
        }

        public string CurrentDisplayName => DisplayName(GlobalSettings.Language);

        public Task<string> GetCurrentDisplayNameAsync(CancellationToken token = default) =>
            DisplayNameAsync(GlobalSettings.Language, token);

        public string DisplayName(string strLanguage)
        {
            using (LockObject.EnterReadLock())
            {
                if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                    return Name;
                return _objCharacter.LoadDataXPath("skills.xml", strLanguage)
                           .SelectSingleNodeAndCacheExpression("/chummer/skillgroups/name[. = " + Name.CleanXPath() + "]/@translate")
                           ?.Value ??
                       Name;
            }
        }

        public async Task<string> DisplayNameAsync(string strLanguage, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                    return Name;
                return (await _objCharacter.LoadDataXPathAsync("skills.xml", strLanguage, token: token)
                           .ConfigureAwait(false))
                       .SelectSingleNodeAndCacheExpression(
                           "/chummer/skillgroups/name[. = " + Name.CleanXPath() + "]/@translate", token: token)?.Value
                       ?? Name;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string DisplayRating
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    if (IsDisabled)
                        return LanguageManager.GetString("Label_SkillGroup_Disabled");
                    if (IsBroken)
                        return LanguageManager.GetString("Label_SkillGroup_Broken");
                    int intReturn = int.MaxValue;
                    foreach (Skill objSkill in SkillList)
                    {
                        if (objSkill.Enabled)
                            intReturn = Math.Min(intReturn, objSkill.TotalBaseRating);
                    }

                    return intReturn == int.MaxValue
                        ? 0.ToString(GlobalSettings.CultureInfo)
                        : intReturn.ToString(GlobalSettings.CultureInfo);
                }
            }
        }

        public async Task<string> GetDisplayRatingAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (await GetIsDisabledAsync(token).ConfigureAwait(false))
                    return await LanguageManager.GetStringAsync("Label_SkillGroup_Disabled", token: token)
                                                .ConfigureAwait(false);
                if (await GetIsBrokenAsync(token).ConfigureAwait(false))
                    return await LanguageManager.GetStringAsync("Label_SkillGroup_Broken", token: token)
                                                .ConfigureAwait(false);
                int intReturn = int.MaxValue;
                foreach (Skill objSkill in SkillList)
                {
                    if (await objSkill.GetEnabledAsync(token).ConfigureAwait(false))
                        intReturn = Math.Min(
                            intReturn, await objSkill.GetTotalBaseRatingAsync(token).ConfigureAwait(false));
                }

                return intReturn == int.MaxValue
                    ? 0.ToString(GlobalSettings.CultureInfo)
                    : intReturn.ToString(GlobalSettings.CultureInfo);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private string _strCachedToolTip = string.Empty;
        private readonly AsyncFriendlyReaderWriterLock _objCachedToolTipLock;

        public string ToolTip
        {
            get
            {
                using (_objCachedToolTipLock.EnterReadLock())
                {
                    if (!string.IsNullOrEmpty(_strCachedToolTip))
                        return _strCachedToolTip;
                }

                using (_objCachedToolTipLock.EnterUpgradeableReadLock())
                {
                    if (!string.IsNullOrEmpty(_strCachedToolTip))
                        return _strCachedToolTip;
                    using (_objCachedToolTipLock.EnterWriteLock())
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                               out StringBuilder sbdTooltip))
                    {
                        string strSpace = LanguageManager.GetString("String_Space");
                        sbdTooltip.Append(LanguageManager.GetString("Tip_SkillGroup_Skills")).Append(strSpace)
                            .AppendJoin(',' + strSpace, SkillList.Select(x => x.CurrentDisplayName)).AppendLine();

                        if (IsDisabled)
                        {
                            sbdTooltip.AppendLine(LanguageManager.GetString("Label_SkillGroup_DisabledBy"));
                            List<Improvement> lstImprovements
                                = ImprovementManager.GetCachedImprovementListForValueOf(
                                    _objCharacter, Improvement.ImprovementType.SkillGroupDisable, Name);
                            lstImprovements.AddRange(ImprovementManager.GetCachedImprovementListForValueOf(
                                    _objCharacter,
                                    Improvement.ImprovementType
                                        .SkillGroupCategoryDisable)
                                .Where(x => GetRelevantSkillCategories.Contains(
                                    x.ImprovedName)));
                            foreach (Improvement objImprovement in lstImprovements)
                            {
                                sbdTooltip.AppendLine(CharacterObject.GetObjectName(objImprovement));
                            }
                        }

                        return _strCachedToolTip = sbdTooltip.ToString();
                    }
                }
            }
        }

        public async Task<string> GetToolTipAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await _objCachedToolTipLock.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (!string.IsNullOrEmpty(_strCachedToolTip))
                    return _strCachedToolTip;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            objLocker = await _objCachedToolTipLock.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (!string.IsNullOrEmpty(_strCachedToolTip))
                    return _strCachedToolTip;
                IAsyncDisposable objLocker2 =
                    await _objCachedToolTipLock.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                               out StringBuilder sbdTooltip))
                    {
                        string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token)
                            .ConfigureAwait(false);
                        (await sbdTooltip
                            .Append(await LanguageManager.GetStringAsync("Tip_SkillGroup_Skills", token: token)
                                .ConfigureAwait(false)).Append(strSpace)
                            .AppendJoinAsync(',' + strSpace,
                                SkillList.Select(x => x.GetCurrentDisplayNameAsync(token)),
                                token).ConfigureAwait(false)).AppendLine();

                        if (await GetIsDisabledAsync(token).ConfigureAwait(false))
                        {
                            sbdTooltip.AppendLine(await LanguageManager
                                .GetStringAsync("Label_SkillGroup_DisabledBy", token: token)
                                .ConfigureAwait(false));
                            List<Improvement> lstImprovements
                                = await ImprovementManager.GetCachedImprovementListForValueOfAsync(
                                        _objCharacter,
                                        Improvement.ImprovementType.SkillGroupDisable, Name,
                                        token: token)
                                    .ConfigureAwait(false);
                            lstImprovements.AddRange((await ImprovementManager
                                    .GetCachedImprovementListForValueOfAsync(
                                        _objCharacter,
                                        Improvement.ImprovementType
                                            .SkillGroupCategoryDisable,
                                        token: token)
                                    .ConfigureAwait(false))
                                .Where(x => GetRelevantSkillCategories.Contains(
                                    x.ImprovedName)));
                            foreach (Improvement objImprovement in lstImprovements)
                            {
                                sbdTooltip.AppendLine(await CharacterObject
                                    .GetObjectNameAsync(objImprovement, token: token)
                                    .ConfigureAwait(false));
                            }
                        }

                        return _strCachedToolTip = sbdTooltip.ToString();
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

        public string UpgradeToolTip
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    int intRating = int.MaxValue;
                    foreach (Skill objSkill in SkillList)
                    {
                        if (objSkill.Enabled)
                            intRating = Math.Min(intRating, objSkill.TotalBaseRating);
                    }

                    if (intRating == int.MaxValue)
                        intRating = 0;
                    return string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Tip_ImproveItem"),
                        intRating + 1, UpgradeKarmaCost);
                }
            }
        }

        public async Task<string> GetUpgradeToolTipAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intRating = int.MaxValue;
                foreach (Skill objSkill in SkillList)
                {
                    if (await objSkill.GetEnabledAsync(token).ConfigureAwait(false))
                        intRating = Math.Min(
                            intRating, await objSkill.GetTotalBaseRatingAsync(token).ConfigureAwait(false));
                }

                if (intRating == int.MaxValue)
                    intRating = 0;
                return string.Format(GlobalSettings.CultureInfo,
                                     await LanguageManager.GetStringAsync("Tip_ImproveItem", token: token)
                                                          .ConfigureAwait(false), intRating + 1,
                                     await GetUpgradeKarmaCostAsync(token).ConfigureAwait(false));
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private Guid _guidId = Guid.NewGuid();

        public Guid Id
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _guidId;
            }
        }

        public string InternalId => Id.ToString("D", GlobalSettings.InvariantCultureInfo);

        #region HasWhateverSkills

        public bool HasCombatSkills
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return SkillList.Any(x => x.SkillCategory == "Combat Active");
            }
        }

        public bool HasPhysicalSkills
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return SkillList.Any(x => x.SkillCategory == "Physical Active");
            }
        }

        public bool HasSocialSkills
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return SkillList.Any(x => x.SkillCategory == "Social Active");
            }
        }

        public bool HasTechnicalSkills
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return SkillList.Any(x => x.SkillCategory == "Technical Active");
            }
        }

        public bool HasVehicleSkills
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return SkillList.Any(x => x.SkillCategory == "Vehicle Active");
            }
        }

        public bool HasMagicalSkills
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return SkillList.Any(x => x.SkillCategory == "Magical Active");
            }
        }

        public bool HasResonanceSkills
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return SkillList.Any(x => x.SkillCategory == "Resonance Active");
            }
        }

        public IEnumerable<string> GetRelevantSkillCategories
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return SkillList.Select(x => x.SkillCategory).Distinct();
            }
        }

        #endregion HasWhateverSkills

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
            using (LockObject.EnterUpgradeableReadLock())
            {
                HashSet<string> setNamesOfChangedProperties = null;
                try
                {
                    foreach (string strPropertyName in lstPropertyNames)
                    {
                        if (setNamesOfChangedProperties == null)
                            setNamesOfChangedProperties
                                = s_SkillGroupDependencyGraph.GetWithAllDependents(this, strPropertyName, true);
                        else
                        {
                            foreach (string strLoopChangedProperty in s_SkillGroupDependencyGraph
                                         .GetWithAllDependentsEnumerable(this, strPropertyName))
                                setNamesOfChangedProperties.Add(strLoopChangedProperty);
                        }
                    }

                    if (setNamesOfChangedProperties == null || setNamesOfChangedProperties.Count == 0)
                        return;

                    using (LockObject.EnterWriteLock())
                    {
                        if (setNamesOfChangedProperties.Contains(nameof(IsDisabled)))
                            _intCachedIsDisabled = int.MinValue;
                        if (setNamesOfChangedProperties.Contains(nameof(KarmaUnbroken)))
                            _intCachedKarmaUnbroken = int.MinValue;
                        if (setNamesOfChangedProperties.Contains(nameof(BaseUnbroken)))
                            _intCachedBaseUnbroken = int.MinValue;
                        if (setNamesOfChangedProperties.Contains(nameof(ToolTip)))
                            _strCachedToolTip = string.Empty;
                        if (setNamesOfChangedProperties.Contains(nameof(HasAnyBreakingSkills)))
                        {
                            _intCachedHasAnyBreakingSkills = int.MinValue;
                            UpdateIsBroken();
                        }

                        if (KarmaPoints > 0 && setNamesOfChangedProperties.Contains(nameof(KarmaUnbroken)) &&
                            _objCharacter?.Created == false && !KarmaUnbroken)
                        {
                            KarmaPoints = 0;
                        }
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
                                = await s_SkillGroupDependencyGraph.GetWithAllDependentsAsync(this, strPropertyName, true, token).ConfigureAwait(false);
                        else
                        {
                            foreach (string strLoopChangedProperty in await s_SkillGroupDependencyGraph
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
                        if (setNamesOfChangedProperties.Contains(nameof(IsDisabled)))
                            _intCachedIsDisabled = int.MinValue;
                        if (setNamesOfChangedProperties.Contains(nameof(KarmaUnbroken)))
                            _intCachedKarmaUnbroken = int.MinValue;
                        if (setNamesOfChangedProperties.Contains(nameof(BaseUnbroken)))
                            _intCachedBaseUnbroken = int.MinValue;
                        if (setNamesOfChangedProperties.Contains(nameof(ToolTip)))
                            _strCachedToolTip = string.Empty;
                        if (setNamesOfChangedProperties.Contains(nameof(HasAnyBreakingSkills)))
                        {
                            _intCachedHasAnyBreakingSkills = int.MinValue;
                            await UpdateIsBrokenAsync(token).ConfigureAwait(false);
                        }

                        if (await GetKarmaPointsAsync(token).ConfigureAwait(false) > 0
                            && setNamesOfChangedProperties.Contains(nameof(KarmaUnbroken))
                            && _objCharacter != null
                            && !await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false)
                            && !await GetKarmaUnbrokenAsync(token).ConfigureAwait(false))
                        {
                            await SetKarmaPointsAsync(0, token).ConfigureAwait(false);
                        }
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
                        List<Task> lstTasks =
                            new List<Task>(Math.Min(lstArgsList.Count * _setPropertyChangedAsync.Count,
                                Utils.MaxParallelBatchSize));
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
                                        token.ThrowIfCancellationRequested();
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
                                    token.ThrowIfCancellationRequested();
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

        private Task OnCharacterPropertyChanged(object sender, MultiplePropertiesChangedEventArgs e, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (CharacterObject?.IsLoading != false)
                return Task.CompletedTask;
            if (e.PropertyNames.Contains(nameof(Character.Karma)))
            {
                return e.PropertyNames.Contains(nameof(Character.EffectiveBuildMethodUsesPriorityTables))
                    ? OnMultiplePropertiesChangedAsync(
                        new[] { nameof(CareerCanIncrease), nameof(BaseUnbroken) }, token)
                    : OnPropertyChangedAsync(nameof(CareerCanIncrease), token);
            }

            return e.PropertyNames.Contains(nameof(Character.EffectiveBuildMethodUsesPriorityTables))
                ? OnPropertyChangedAsync(nameof(BaseUnbroken), token)
                : Task.CompletedTask;
        }

        private async Task OnCharacterSettingsPropertyChanged(object sender, MultiplePropertiesChangedEventArgs e,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (CharacterObject?.IsLoading != false)
            {
                if (e.PropertyNames.Contains(nameof(CharacterSettings.AllowSkillRegrouping)))
                {
                    await UpdateIsBrokenAsync(token).ConfigureAwait(false);
                }

                return;
            }

            List<string> lstProperties = new List<string>();
            if (CharacterObject != null)
            {
                CharacterSettings objSettings = await CharacterObject.GetSettingsAsync(token).ConfigureAwait(false);

                if (objSettings != null && !await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false))
                {
                    if (e.PropertyNames.Contains(nameof(CharacterSettings.StrictSkillGroupsInCreateMode)))
                    {
                        if (await objSettings.GetStrictSkillGroupsInCreateModeAsync(token).ConfigureAwait(false)
                            && !await CharacterObject.GetIgnoreRulesAsync(token).ConfigureAwait(false))
                        {
                            IAsyncDisposable objLocker =
                                await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
                            try
                            {
                                token.ThrowIfCancellationRequested();
                                if (await GetKarmaAsync(token).ConfigureAwait(false) > 0)
                                {
                                    await SkillList.ForEachAsync(async skill =>
                                    {
                                        IAsyncDisposable objLocker2 = await skill.LockObject.EnterWriteLockAsync(token)
                                            .ConfigureAwait(false);
                                        try
                                        {
                                            token.ThrowIfCancellationRequested();
                                            await skill.SetKarmaPointsAsync(0, token).ConfigureAwait(false);
                                            await skill.SetBasePointsAsync(0, token).ConfigureAwait(false);
                                            await skill.Specializations
                                                .RemoveAllAsync(
                                                    async x => !await x.GetFreeAsync(token).ConfigureAwait(false),
                                                    token: token).ConfigureAwait(false);
                                        }
                                        finally
                                        {
                                            await objLocker2.DisposeAsync().ConfigureAwait(false);
                                        }
                                    }, token).ConfigureAwait(false);
                                }
                                else
                                {
                                    await SkillList.ForEachAsync(async skill =>
                                    {
                                        IAsyncDisposable objLocker2 = await skill.LockObject.EnterWriteLockAsync(token)
                                            .ConfigureAwait(false);
                                        try
                                        {
                                            await skill.SetBasePointsAsync(0, token).ConfigureAwait(false);
                                        }
                                        finally
                                        {
                                            await objLocker2.DisposeAsync().ConfigureAwait(false);
                                        }
                                    }, token).ConfigureAwait(false);
                                }
                            }
                            finally
                            {
                                await objLocker.DisposeAsync().ConfigureAwait(false);
                            }
                        }
                        else if (e.PropertyNames.Contains(nameof(CharacterSettings.UsePointsOnBrokenGroups))
                                 && !await objSettings.GetUsePointsOnBrokenGroupsAsync(token).ConfigureAwait(false))
                        {
                            IAsyncDisposable objLocker =
                                await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
                            try
                            {
                                token.ThrowIfCancellationRequested();
                                await SkillList.ForEachAsync(async skill =>
                                {
                                    IAsyncDisposable objLocker2 =
                                        await skill.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                                    try
                                    {
                                        await skill.SetBasePointsAsync(0, token).ConfigureAwait(false);
                                    }
                                    finally
                                    {
                                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                                    }
                                }, token).ConfigureAwait(false);
                            }
                            finally
                            {
                                await objLocker.DisposeAsync().ConfigureAwait(false);
                            }
                        }

                        await OnPropertyChangedAsync(nameof(BaseUnbroken), token).ConfigureAwait(false);
                    }
                    else if (e.PropertyNames.Contains(nameof(CharacterSettings.UsePointsOnBrokenGroups)))
                    {
                        if (!await objSettings.GetUsePointsOnBrokenGroupsAsync(token).ConfigureAwait(false))
                        {
                            IAsyncDisposable objLocker =
                                await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
                            try
                            {
                                token.ThrowIfCancellationRequested();
                                await SkillList.ForEachAsync(async skill =>
                                {
                                    IAsyncDisposable objLocker2 =
                                        await skill.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                                    try
                                    {
                                        await skill.SetBasePointsAsync(0, token).ConfigureAwait(false);
                                    }
                                    finally
                                    {
                                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                                    }
                                }, token).ConfigureAwait(false);
                            }
                            finally
                            {
                                await objLocker.DisposeAsync().ConfigureAwait(false);
                            }
                        }

                        lstProperties.Add(nameof(BaseUnbroken));
                    }
                }
                else if (e.PropertyNames.Contains(nameof(CharacterSettings.StrictSkillGroupsInCreateMode))
                         || e.PropertyNames.Contains(nameof(CharacterSettings.UsePointsOnBrokenGroups)))
                    lstProperties.Add(nameof(BaseUnbroken));

                if (await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false)
                    || await CharacterObject.GetIgnoreRulesAsync(token).ConfigureAwait(false))
                {
                    if (e.PropertyNames.Contains(nameof(CharacterSettings.MaxSkillRating)))
                        lstProperties.Add(nameof(RatingMaximum));
                }
                else if (e.PropertyNames.Contains(nameof(CharacterSettings.MaxSkillRatingCreate)))
                {
                    lstProperties.Add(nameof(RatingMaximum));
                }
            }
            else if (e.PropertyNames.Contains(nameof(CharacterSettings.StrictSkillGroupsInCreateMode))
                     || e.PropertyNames.Contains(nameof(CharacterSettings.UsePointsOnBrokenGroups)))
                lstProperties.Add(nameof(BaseUnbroken));

            if (e.PropertyNames.Contains(nameof(CharacterSettings.KarmaNewSkillGroup))
                || e.PropertyNames.Contains(nameof(CharacterSettings.KarmaImproveSkillGroup)))
            {
                lstProperties.Add(nameof(CurrentKarmaCost));
            }

            if (e.PropertyNames.Contains(nameof(CharacterSettings.SpecializationsBreakSkillGroups)))
            {
                lstProperties.Add(nameof(HasAnyBreakingSkills));
            }

            if (lstProperties.Count > 0)
                await OnMultiplePropertiesChangedAsync(lstProperties, token).ConfigureAwait(false);
            if (e.PropertyNames.Contains(nameof(CharacterSettings.AllowSkillRegrouping)))
            {
                await UpdateIsBrokenAsync(token).ConfigureAwait(false);
            }
        }

        public int CurrentSpCost
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    int intReturn = BasePoints;
                    int intValue = intReturn;

                    decimal decMultiplier = 1.0m;
                    decimal decExtra = 0;
                    using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                               out HashSet<string>
                                   lstRelevantCategories))
                    {
                        lstRelevantCategories.AddRange(GetRelevantSkillCategories);
                        CharacterObject.Improvements.ForEach(objLoopImprovement =>
                        {
                            if ((objLoopImprovement.Maximum == 0 || intValue <= objLoopImprovement.Maximum)
                                && objLoopImprovement.Minimum <= intValue && objLoopImprovement.Enabled)
                            {
                                if (objLoopImprovement.ImprovedName == Name
                                    || string.IsNullOrEmpty(objLoopImprovement.ImprovedName))
                                {
                                    switch (objLoopImprovement.ImproveType)
                                    {
                                        case Improvement.ImprovementType.SkillGroupPointCost:
                                            decExtra += objLoopImprovement.Value
                                                        * (Math.Min(
                                                               intValue,
                                                               objLoopImprovement.Maximum == 0
                                                                   ? int.MaxValue
                                                                   : objLoopImprovement.Maximum) -
                                                           objLoopImprovement.Minimum);
                                            break;

                                        case Improvement.ImprovementType.SkillGroupPointCostMultiplier:
                                            decMultiplier *= objLoopImprovement.Value / 100.0m;
                                            break;
                                    }
                                }
                                else if (lstRelevantCategories.Contains(objLoopImprovement.ImprovedName))
                                {
                                    switch (objLoopImprovement.ImproveType)
                                    {
                                        case Improvement.ImprovementType.SkillGroupCategoryPointCost:
                                            decExtra += objLoopImprovement.Value
                                                        * (Math.Min(
                                                               intValue,
                                                               objLoopImprovement.Maximum == 0
                                                                   ? int.MaxValue
                                                                   : objLoopImprovement.Maximum) -
                                                           objLoopImprovement.Minimum);
                                            break;

                                        case Improvement.ImprovementType.SkillGroupCategoryPointCostMultiplier:
                                            decMultiplier *= objLoopImprovement.Value / 100.0m;
                                            break;
                                    }
                                }
                            }
                        });
                    }

                    if (decMultiplier != 1.0m)
                        intReturn = (intReturn * decMultiplier + decExtra).StandardRound();
                    else
                        intReturn += decExtra.StandardRound();

                    return Math.Max(intReturn, 0);
                }
            }
        }

        public async Task<int> GetCurrentSpCostAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intReturn = BasePoints;
                int intValue = intReturn;

                decimal decMultiplier = 1.0m;
                decimal decExtra = 0;
                using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                           out HashSet<string>
                               lstRelevantCategories))
                {
                    lstRelevantCategories.AddRange(GetRelevantSkillCategories);
                    await (await _objCharacter.GetImprovementsAsync(token).ConfigureAwait(false)).ForEachAsync(
                        objLoopImprovement =>
                        {
                            if ((objLoopImprovement.Maximum != 0 && intValue > objLoopImprovement.Maximum)
                                || objLoopImprovement.Minimum > intValue || !objLoopImprovement.Enabled)
                                return;
                            if (objLoopImprovement.ImprovedName == Name
                                || string.IsNullOrEmpty(objLoopImprovement.ImprovedName))
                            {
                                switch (objLoopImprovement.ImproveType)
                                {
                                    case Improvement.ImprovementType.SkillGroupPointCost:
                                        decExtra += objLoopImprovement.Value
                                                    * (Math.Min(
                                                        intValue,
                                                        objLoopImprovement.Maximum == 0
                                                            ? int.MaxValue
                                                            : objLoopImprovement.Maximum) - objLoopImprovement.Minimum);
                                        break;

                                    case Improvement.ImprovementType.SkillGroupPointCostMultiplier:
                                        decMultiplier *= objLoopImprovement.Value / 100.0m;
                                        break;
                                }
                            }
                            else if (lstRelevantCategories.Contains(objLoopImprovement.ImprovedName))
                            {
                                switch (objLoopImprovement.ImproveType)
                                {
                                    case Improvement.ImprovementType.SkillGroupCategoryPointCost:
                                        decExtra += objLoopImprovement.Value
                                                    * (Math.Min(
                                                        intValue,
                                                        objLoopImprovement.Maximum == 0
                                                            ? int.MaxValue
                                                            : objLoopImprovement.Maximum) - objLoopImprovement.Minimum);
                                        break;

                                    case Improvement.ImprovementType.SkillGroupCategoryPointCostMultiplier:
                                        decMultiplier *= objLoopImprovement.Value / 100.0m;
                                        break;
                                }
                            }
                        }, token: token).ConfigureAwait(false);
                }

                if (decMultiplier != 1.0m)
                    intReturn = (intReturn * decMultiplier + decExtra).StandardRound();
                else
                    intReturn += decExtra.StandardRound();

                return Math.Max(intReturn, 0);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public int CurrentKarmaCost
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    if (KarmaPoints == 0)
                        return 0;
                    int intUpper = int.MaxValue;
                    foreach (Skill objSkill in SkillList)
                    {
                        if (objSkill.Enabled)
                            intUpper = Math.Min(intUpper, objSkill.TotalBaseRating);
                    }

                    if (intUpper == int.MaxValue)
                        intUpper = 0;
                    int intLower = intUpper - KarmaPoints;

                    int intCost = intUpper * (intUpper + 1);
                    intCost -= intLower * (intLower + 1);
                    intCost /= 2; //We get square, need triangle

                    if (intCost == 1)
                        intCost *= _objCharacter.Settings.KarmaNewSkillGroup;
                    else
                        intCost *= _objCharacter.Settings.KarmaImproveSkillGroup;

                    decimal decMultiplier = 1.0m;
                    decimal decExtra = 0;
                    using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                               out HashSet<string>
                                   lstRelevantCategories))
                    {
                        lstRelevantCategories.AddRange(GetRelevantSkillCategories);
                        CharacterObject.Improvements.ForEach(objLoopImprovement =>
                        {
                            if (objLoopImprovement.Minimum <= intLower &&
                                (string.IsNullOrEmpty(objLoopImprovement.Condition)
                                 || (objLoopImprovement.Condition == "career") == _objCharacter.Created
                                 || (objLoopImprovement.Condition == "create") != _objCharacter.Created)
                                && objLoopImprovement.Enabled)
                            {
                                if (objLoopImprovement.ImprovedName == Name
                                    || string.IsNullOrEmpty(objLoopImprovement.ImprovedName))
                                {
                                    switch (objLoopImprovement.ImproveType)
                                    {
                                        case Improvement.ImprovementType.SkillGroupKarmaCost:
                                            decExtra += objLoopImprovement.Value
                                                        * (Math.Min(
                                                               intUpper,
                                                               objLoopImprovement.Maximum == 0
                                                                   ? int.MaxValue
                                                                   : objLoopImprovement.Maximum)
                                                           - Math.Max(intLower, objLoopImprovement.Minimum - 1));
                                            break;

                                        case Improvement.ImprovementType.SkillGroupKarmaCostMultiplier:
                                            decMultiplier *= objLoopImprovement.Value / 100.0m;
                                            break;
                                    }
                                }
                                else if (lstRelevantCategories.Contains(objLoopImprovement.ImprovedName))
                                {
                                    switch (objLoopImprovement.ImproveType)
                                    {
                                        case Improvement.ImprovementType.SkillGroupCategoryKarmaCost:
                                            decExtra += objLoopImprovement.Value
                                                        * (Math.Min(
                                                               intUpper,
                                                               objLoopImprovement.Maximum == 0
                                                                   ? int.MaxValue
                                                                   : objLoopImprovement.Maximum)
                                                           - Math.Max(intLower, objLoopImprovement.Minimum - 1));
                                            break;

                                        case Improvement.ImprovementType.SkillGroupCategoryKarmaCostMultiplier:
                                            decMultiplier *= objLoopImprovement.Value / 100.0m;
                                            break;
                                    }
                                }
                            }
                        });
                    }

                    if (decMultiplier != 1.0m)
                        intCost = (intCost * decMultiplier + decExtra).StandardRound();
                    else
                        intCost += decExtra.StandardRound();

                    return Math.Max(intCost, 0);
                }
            }
        }

        public async Task<int> GetCurrentKarmaCostAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intKarmaPoints = await GetKarmaPointsAsync(token).ConfigureAwait(false);
                if (intKarmaPoints == 0)
                    return 0;

                int intUpper = int.MaxValue;
                foreach (Skill objSkill in SkillList)
                {
                    if (await objSkill.GetEnabledAsync(token).ConfigureAwait(false))
                        intUpper = Math.Min(intUpper, await objSkill.GetTotalBaseRatingAsync(token).ConfigureAwait(false));
                }

                if (intUpper == int.MaxValue)
                    intUpper = 0;
                int intLower = intUpper - intKarmaPoints;

                int intCost = intUpper * (intUpper + 1);
                intCost -= intLower * (intLower + 1);
                intCost /= 2; //We get square, need triangle

                CharacterSettings objSettings = await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false);
                if (intCost == 1)
                    intCost *= await objSettings.GetKarmaNewSkillGroupAsync(token).ConfigureAwait(false);
                else
                    intCost *= await objSettings.GetKarmaImproveSkillGroupAsync(token).ConfigureAwait(false);

                decimal decMultiplier = 1.0m;
                decimal decExtra = 0;
                using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                           out HashSet<string>
                               lstRelevantCategories))
                {
                    lstRelevantCategories.AddRange(GetRelevantSkillCategories);
                    ThreadSafeObservableCollection<Improvement> lstImprovements =
                        await _objCharacter.GetImprovementsAsync(token).ConfigureAwait(false);
                    bool blnCreated = await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false);
                    await lstImprovements.ForEachAsync(objLoopImprovement =>
                    {
                        if (objLoopImprovement.Minimum <= intLower &&
                            (string.IsNullOrEmpty(objLoopImprovement.Condition)
                             || (objLoopImprovement.Condition == "career") == blnCreated
                             || (objLoopImprovement.Condition == "create") != blnCreated)
                            && objLoopImprovement.Enabled)
                        {
                            if (objLoopImprovement.ImprovedName == Name
                                || string.IsNullOrEmpty(objLoopImprovement.ImprovedName))
                            {
                                switch (objLoopImprovement.ImproveType)
                                {
                                    case Improvement.ImprovementType.SkillGroupKarmaCost:
                                        decExtra += objLoopImprovement.Value
                                                    * (Math.Min(
                                                           intUpper,
                                                           objLoopImprovement.Maximum == 0
                                                               ? int.MaxValue
                                                               : objLoopImprovement.Maximum)
                                                       - Math.Max(intLower, objLoopImprovement.Minimum - 1));
                                        break;

                                    case Improvement.ImprovementType.SkillGroupKarmaCostMultiplier:
                                        decMultiplier *= objLoopImprovement.Value / 100.0m;
                                        break;
                                }
                            }
                            else if (lstRelevantCategories.Contains(objLoopImprovement.ImprovedName))
                            {
                                switch (objLoopImprovement.ImproveType)
                                {
                                    case Improvement.ImprovementType.SkillGroupCategoryKarmaCost:
                                        decExtra += objLoopImprovement.Value
                                                    * (Math.Min(
                                                           intUpper,
                                                           objLoopImprovement.Maximum == 0
                                                               ? int.MaxValue
                                                               : objLoopImprovement.Maximum)
                                                       - Math.Max(intLower, objLoopImprovement.Minimum - 1));
                                        break;

                                    case Improvement.ImprovementType.SkillGroupCategoryKarmaCostMultiplier:
                                        decMultiplier *= objLoopImprovement.Value / 100.0m;
                                        break;
                                }
                            }
                        }
                    }, token: token).ConfigureAwait(false);
                }

                if (decMultiplier != 1.0m)
                    intCost = (intCost * decMultiplier + decExtra).StandardRound();
                else
                    intCost += decExtra.StandardRound();

                return Math.Max(intCost, 0);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public int UpgradeKarmaCost
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    if (IsDisabled)
                        return -1;
                    int intRating = int.MaxValue;
                    foreach (Skill objSkill in SkillList)
                    {
                        if (objSkill.Enabled)
                            intRating = Math.Min(intRating, objSkill.TotalBaseRating);
                    }

                    if (intRating == int.MaxValue)
                        intRating = 0;
                    int intReturn;
                    int intOptionsCost;
                    if (intRating == 0)
                    {
                        intOptionsCost = CharacterObject.Settings.KarmaNewSkillGroup;
                        intReturn = intOptionsCost;
                    }
                    else if (RatingMaximum > intRating)
                    {
                        intOptionsCost = CharacterObject.Settings.KarmaImproveSkillGroup;
                        intReturn = (intRating + 1) * intOptionsCost;
                    }
                    else
                    {
                        return -1;
                    }

                    decimal decMultiplier = 1.0m;
                    decimal decExtra = 0;
                    using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                               out HashSet<string>
                                   lstRelevantCategories))
                    {
                        lstRelevantCategories.AddRange(GetRelevantSkillCategories);
                        CharacterObject.Improvements.ForEach(objLoopImprovement =>
                        {
                            if ((objLoopImprovement.Maximum == 0 || intRating + 1 <= objLoopImprovement.Maximum)
                                && objLoopImprovement.Minimum <= intRating + 1 &&
                                (string.IsNullOrEmpty(objLoopImprovement.Condition)
                                 || (objLoopImprovement.Condition == "career") == _objCharacter.Created
                                 || (objLoopImprovement.Condition == "create") != _objCharacter.Created) &&
                                objLoopImprovement.Enabled)
                            {
                                if (objLoopImprovement.ImprovedName == Name
                                    || string.IsNullOrEmpty(objLoopImprovement.ImprovedName))
                                {
                                    switch (objLoopImprovement.ImproveType)
                                    {
                                        case Improvement.ImprovementType.SkillGroupKarmaCost:
                                            decExtra += objLoopImprovement.Value;
                                            break;

                                        case Improvement.ImprovementType.SkillGroupKarmaCostMultiplier:
                                            decMultiplier *= objLoopImprovement.Value / 100.0m;
                                            break;
                                    }
                                }
                                else if (lstRelevantCategories.Contains(objLoopImprovement.ImprovedName))
                                {
                                    switch (objLoopImprovement.ImproveType)
                                    {
                                        case Improvement.ImprovementType.SkillGroupCategoryKarmaCost:
                                            decExtra += objLoopImprovement.Value;
                                            break;

                                        case Improvement.ImprovementType.SkillGroupCategoryKarmaCostMultiplier:
                                            decMultiplier *= objLoopImprovement.Value / 100.0m;
                                            break;
                                    }
                                }
                            }
                        });
                    }

                    if (decMultiplier != 1.0m)
                        intReturn = (intReturn * decMultiplier + decExtra).StandardRound();
                    else
                        intReturn += decExtra.StandardRound();

                    return Math.Max(intReturn, Math.Min(1, intOptionsCost));
                }
            }
        }

        public async Task<int> GetUpgradeKarmaCostAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (await GetIsDisabledAsync(token).ConfigureAwait(false))
                    return -1;
                int intRating = int.MaxValue;
                foreach (Skill objSkill in SkillList)
                {
                    if (await objSkill.GetEnabledAsync(token).ConfigureAwait(false))
                        intRating = Math.Min(
                            intRating, await objSkill.GetTotalBaseRatingAsync(token).ConfigureAwait(false));
                }

                if (intRating == int.MaxValue)
                    intRating = 0;
                int intReturn;
                int intOptionsCost;
                if (intRating == 0)
                {
                    intOptionsCost = (await CharacterObject.GetSettingsAsync(token).ConfigureAwait(false)).KarmaNewSkillGroup;
                    intReturn = intOptionsCost;
                }
                else if (await GetRatingMaximumAsync(token).ConfigureAwait(false) > intRating)
                {
                    intOptionsCost = (await CharacterObject.GetSettingsAsync(token).ConfigureAwait(false)).KarmaImproveSkillGroup;
                    intReturn = (intRating + 1) * intOptionsCost;
                }
                else
                {
                    return -1;
                }

                decimal decMultiplier = 1.0m;
                decimal decExtra = 0;
                using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                out HashSet<string>
                                                                    lstRelevantCategories))
                {
                    lstRelevantCategories.AddRange(GetRelevantSkillCategories);
                    bool blnCreated = await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false);
                    await (await _objCharacter.GetImprovementsAsync(token).ConfigureAwait(false)).ForEachAsync(objLoopImprovement =>
                    {
                        if ((objLoopImprovement.Maximum == 0 || intRating + 1 <= objLoopImprovement.Maximum)
                            && objLoopImprovement.Minimum <= intRating + 1 &&
                            (string.IsNullOrEmpty(objLoopImprovement.Condition)
                             || (objLoopImprovement.Condition == "career") == blnCreated
                             || (objLoopImprovement.Condition == "create") != blnCreated) &&
                            objLoopImprovement.Enabled)
                        {
                            if (objLoopImprovement.ImprovedName == Name
                                || string.IsNullOrEmpty(objLoopImprovement.ImprovedName))
                            {
                                switch (objLoopImprovement.ImproveType)
                                {
                                    case Improvement.ImprovementType.SkillGroupKarmaCost:
                                        decExtra += objLoopImprovement.Value;
                                        break;

                                    case Improvement.ImprovementType.SkillGroupKarmaCostMultiplier:
                                        decMultiplier *= objLoopImprovement.Value / 100.0m;
                                        break;
                                }
                            }
                            else if (lstRelevantCategories.Contains(objLoopImprovement.ImprovedName))
                            {
                                switch (objLoopImprovement.ImproveType)
                                {
                                    case Improvement.ImprovementType.SkillGroupCategoryKarmaCost:
                                        decExtra += objLoopImprovement.Value;
                                        break;

                                    case Improvement.ImprovementType.SkillGroupCategoryKarmaCostMultiplier:
                                        decMultiplier *= objLoopImprovement.Value / 100.0m;
                                        break;
                                }
                            }
                        }
                    }, token: token).ConfigureAwait(false);
                }

                if (decMultiplier != 1.0m)
                    intReturn = (intReturn * decMultiplier + decExtra).StandardRound();
                else
                    intReturn += decExtra.StandardRound();

                return Math.Max(intReturn, Math.Min(1, intOptionsCost));
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current object.
        /// </returns>
        public override int GetHashCode()
        {
            return InternalId.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
                return true;
            return obj is SkillGroup objOther && Equals(objOther);
        }

        public bool Equals(SkillGroup other)
        {
            if (other == null)
                return false;
            if (ReferenceEquals(other, this))
                return true;
            using (LockObject.EnterReadLock())
            using (other.LockObject.EnterReadLock())
                return InternalId == other.InternalId;
        }

        /// <summary>
        /// List of skills that belong to this skill group.
        /// </summary>
        public IReadOnlyList<Skill> SkillList
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _lstAffectedSkills;
            }
        }

        #endregion All the other stuff that is required
    }
}
