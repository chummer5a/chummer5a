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
    public sealed class SkillGroup : INotifyMultiplePropertyChangedAsync, IHasInternalId, IHasName, IEquatable<SkillGroup>, IHasLockObject
    {
        #region Core calculations

        private int _intSkillFromSp;
        private int _intSkillFromKarma;
        private bool _blnIsBroken;

        public AsyncFriendlyReaderWriterLock LockObject { get; } = new AsyncFriendlyReaderWriterLock();

        public void Dispose()
        {
            using (LockObject.EnterWriteLock())
            {
                if (_objCharacter != null)
                {
                    try
                    {
                        _objCharacter.PropertyChangedAsync -= OnCharacterPropertyChanged;
                    }
                    catch (ObjectDisposedException)
                    {
                        //swallow this
                    }

                    try
                    {
                        _objCharacter.Settings.PropertyChangedAsync -= OnCharacterSettingsPropertyChanged;
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
                        using (objSkill.LockObject.EnterWriteLock())
                            objSkill.PropertyChanged -= SkillOnPropertyChanged;
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
                        IAsyncDisposable objLocker2
                            = await _objCharacter.LockObject.EnterWriteLockAsync().ConfigureAwait(false);
                        try
                        {
                            _objCharacter.PropertyChangedAsync -= OnCharacterPropertyChanged;
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
                            _objCharacter.Settings.PropertyChangedAsync -= OnCharacterSettingsPropertyChanged;
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
                }

                foreach (Skill objSkill in _lstAffectedSkills)
                {
                    try
                    {
                        IAsyncDisposable objLocker2
                            = await objSkill.LockObject.EnterWriteLockAsync().ConfigureAwait(false);
                        try
                        {
                            objSkill.PropertyChanged -= SkillOnPropertyChanged;
                        }
                        finally
                        {
                            await objLocker2.DisposeAsync().ConfigureAwait(false);
                        }
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

        public async ValueTask<int> GetBaseAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                return await GetIsDisabledAsync(token).ConfigureAwait(false)
                    ? 0
                    : Math.Min(await GetBasePointsAsync(token).ConfigureAwait(false) + await GetFreeBaseAsync(token).ConfigureAwait(false),
                        await GetRatingMaximumAsync(token).ConfigureAwait(false));
            }
        }
        
        public async ValueTask SetBaseAsync(int value, CancellationToken token = default)
        {
            using (await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                if (!await GetBaseUnbrokenAsync(token).ConfigureAwait(false))
                    return;
                IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
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
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
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

        public async ValueTask<int> GetKarmaAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
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
        }

        /// <summary>
        /// Amount of skill points bought with karma and bonuses to the skills rating
        /// </summary>
        public async ValueTask SetKarmaAsync(int value, CancellationToken token = default)
        {
            using (await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                if (!await GetKarmaUnbrokenAsync(token).ConfigureAwait(false))
                    return;
                IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
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
                                         x => !x.Free, token).ConfigureAwait(false))
                            {
                                await lstSpecs.RemoveAsync(objSpecialization, token).ConfigureAwait(false);
                            }
                        }
                    }
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
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
        public async ValueTask<int> GetBasePointsAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                return _intSkillFromSp;
            }
        }

        /// <summary>
        /// How many points REALLY are in _base. Better than subclasses calculating Base - FreeBase()
        /// </summary>
        public async ValueTask SetBasePointsAsync(int value, CancellationToken token = default)
        {
            using (await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                // No need to write lock because interlocked guarantees safety
                if (Interlocked.Exchange(ref _intSkillFromSp, value) == value)
                    return;
                await OnPropertyChangedAsync(nameof(BasePoints), token).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// How many points REALLY are in _base. Better than subclasses calculating Base - FreeBase()
        /// </summary>
        public async ValueTask ModifyBasePointsAsync(int value, CancellationToken token = default)
        {
            if (value == 0)
                return;
            using (await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                // No need to write lock because interlocked guarantees safety
                Interlocked.Add(ref _intSkillFromSp, value);
                await OnPropertyChangedAsync(nameof(BasePoints), token).ConfigureAwait(false);
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
        public async ValueTask<int> GetKarmaPointsAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                return _intSkillFromKarma;
            }
        }

        /// <summary>
        /// How many points REALLY are in _karma Better than subclasses calculating Karma - FreeKarma()
        /// </summary>
        public async ValueTask SetKarmaPointsAsync(int value, CancellationToken token = default)
        {
            using (await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                // No need to write lock because interlocked guarantees safety
                if (Interlocked.Exchange(ref _intSkillFromKarma, value) == value)
                    return;
                await OnPropertyChangedAsync(nameof(KarmaPoints), token).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// How many points REALLY are in _karma Better than subclasses calculating Karma - FreeKarma()
        /// </summary>
        public async ValueTask ModifyKarmaPointsAsync(int value, CancellationToken token = default)
        {
            if (value == 0)
                return;
            using (await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                // No need to write lock because interlocked guarantees safety
                Interlocked.Add(ref _intSkillFromKarma, value);
                await OnPropertyChangedAsync(nameof(KarmaPoints), token).ConfigureAwait(false);
            }
        }

        private int _intCachedBaseUnbroken = int.MinValue;
        private readonly AsyncFriendlyReaderWriterLock _objCachedBaseUnbrokenLock = new AsyncFriendlyReaderWriterLock();

        /// <summary>
        /// Is it possible to increment this skill group from points
        /// Inverted to simplify databinding
        /// </summary>
        public bool BaseUnbroken
        {
            get
            {
                using (LockObject.EnterReadLock())
                using (_objCachedBaseUnbrokenLock.EnterUpgradeableReadLock())
                {
                    if (_intCachedBaseUnbroken < 0)
                    {
                        using (_objCachedBaseUnbrokenLock.EnterWriteLock())
                        {
                            if (_intCachedBaseUnbroken < 0) // Just in case
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
                        }
                    }

                    return _intCachedBaseUnbroken > 0;
                }
            }
        }

        /// <summary>
        /// Is it possible to increment this skill group from points
        /// Inverted to simplify databinding
        /// </summary>
        public async ValueTask<bool> GetBaseUnbrokenAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                using (await _objCachedBaseUnbrokenLock.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false))
                {
                    token.ThrowIfCancellationRequested();
                    if (_intCachedBaseUnbroken < 0)
                    {
                        IAsyncDisposable objLocker = await _objCachedBaseUnbrokenLock.EnterWriteLockAsync(token).ConfigureAwait(false);
                        try
                        {
                            token.ThrowIfCancellationRequested();
                            if (_intCachedBaseUnbroken < 0)
                            {
                                if (await GetIsDisabledAsync(token).ConfigureAwait(false) || SkillList.Count == 0
                                    || !await _objCharacter
                                        .GetEffectiveBuildMethodUsesPriorityTablesAsync(
                                            token).ConfigureAwait(false))
                                    _intCachedBaseUnbroken = 0;
                                else
                                {
                                    CharacterSettings objSettings =
                                        await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false);
                                    if (await objSettings.GetStrictSkillGroupsInCreateModeAsync(token).ConfigureAwait(false)
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
                                    else if (await objSettings.GetUsePointsOnBrokenGroupsAsync(token).ConfigureAwait(false))
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
                        }
                        finally
                        {
                            await objLocker.DisposeAsync().ConfigureAwait(false);
                        }
                    }

                    return _intCachedBaseUnbroken > 0;
                }
            }
        }

        private int _intCachedKarmaUnbroken = int.MinValue;
        private readonly AsyncFriendlyReaderWriterLock _objCachedKarmaUnbrokenLock = new AsyncFriendlyReaderWriterLock();

        /// <summary>
        /// Is it possible to increment this skill group from karma
        /// Inverted to simplify databinding
        /// </summary>
        public bool KarmaUnbroken
        {
            get
            {
                using (LockObject.EnterReadLock())
                using (_objCachedKarmaUnbrokenLock.EnterUpgradeableReadLock())
                {
                    if (_intCachedKarmaUnbroken < 0)
                    {
                        using (_objCachedKarmaUnbrokenLock.EnterWriteLock())
                        {
                            if (_intCachedKarmaUnbroken < 0) // Just in case
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
        public async ValueTask<bool> GetKarmaUnbrokenAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                using (await _objCachedKarmaUnbrokenLock.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false))
                {
                    token.ThrowIfCancellationRequested();
                    if (_intCachedKarmaUnbroken < 0)
                    {
                        IAsyncDisposable objLocker = await _objCachedKarmaUnbrokenLock.EnterWriteLockAsync(token).ConfigureAwait(false);
                        try
                        {
                            token.ThrowIfCancellationRequested();
                            if (_intCachedKarmaUnbroken < 0) // Just in case
                            {
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
                                        async x => await x.GetBasePointsAsync(token).ConfigureAwait(false) + await x.GetFreeBaseAsync(token).ConfigureAwait(false),
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
                        }
                        finally
                        {
                            await objLocker.DisposeAsync().ConfigureAwait(false);
                        }
                    }

                    return _intCachedKarmaUnbroken > 0;
                }
            }
        }

        private int _intCachedIsDisabled = int.MinValue;
        private readonly AsyncFriendlyReaderWriterLock _objCachedIsDisabledLock = new AsyncFriendlyReaderWriterLock();

        public bool IsDisabled
        {
            get
            {
                using (LockObject.EnterReadLock())
                using (_objCachedIsDisabledLock.EnterUpgradeableReadLock())
                {
                    if (_intCachedIsDisabled < 0)
                    {
                        using (_objCachedIsDisabledLock.EnterWriteLock())
                        {
                            if (_intCachedIsDisabled < 0) // Just in case
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
                                                           .Any(
                                                               x => GetRelevantSkillCategories
                                                                   .Contains(x.ImprovedName))).ToInt32();
                            }
                        }
                    }

                    return _intCachedIsDisabled > 0;
                }
            }
        }

        public async ValueTask<bool> GetIsDisabledAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                using (await _objCachedIsDisabledLock.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false))
                {
                    token.ThrowIfCancellationRequested();
                    if (_intCachedIsDisabled < 0)
                    {
                        IAsyncDisposable objLocker = await _objCachedIsDisabledLock.EnterWriteLockAsync(token).ConfigureAwait(false);
                        try
                        {
                            token.ThrowIfCancellationRequested();
                            if (_intCachedIsDisabled < 0)
                            {
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
                                                        .Any(
                                                            x => GetRelevantSkillCategories.Contains(x.ImprovedName)))
                                    .ToInt32();
                            }
                        }
                        finally
                        {
                            await objLocker.DisposeAsync().ConfigureAwait(false);
                        }
                    }

                    return _intCachedIsDisabled > 0;
                }
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
                    }
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Can this skillgroup be increased in career mode?
        /// </summary>
        public async ValueTask<bool> GetIsBrokenAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                return _blnIsBroken;
            }
        }

        /// <summary>
        /// Can this skillgroup be increased in career mode?
        /// </summary>
        private async ValueTask SetIsBrokenAsync(bool value, CancellationToken token = default)
        {
            using (await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                if (_blnIsBroken == value)
                    return;
                IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnIsBroken = value;
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
                await OnPropertyChangedAsync(nameof(IsBroken), token).ConfigureAwait(false);
            }
        }

        private void UpdateIsBroken()
        {
            using (LockObject.EnterUpgradeableReadLock())
            {
                if (!_objCharacter.Created)
                    return;
                if (!_objCharacter.Settings.AllowSkillRegrouping && IsBroken)
                    return;
                IsBroken = HasAnyBreakingSkills;
            }
        }

        private async ValueTask UpdateIsBrokenAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                if (!await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false))
                    return;
                if (!(await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).AllowSkillRegrouping
                    && await GetIsBrokenAsync(token).ConfigureAwait(false))
                    return;
                await SetIsBrokenAsync(await GetHasAnyBreakingSkillsAsync(token).ConfigureAwait(false), token)
                    .ConfigureAwait(false);
            }
        }

        private int _intCachedHasAnyBreakingSkills = int.MinValue;
        private readonly AsyncFriendlyReaderWriterLock _objCachedHasAnyBreakingSkillsLock = new AsyncFriendlyReaderWriterLock();

        public bool HasAnyBreakingSkills
        {
            get
            {
                using (LockObject.EnterReadLock())
                using (_objCachedHasAnyBreakingSkillsLock.EnterUpgradeableReadLock())
                {
                    if (_intCachedHasAnyBreakingSkills < 0)
                    {
                        using (_objCachedHasAnyBreakingSkillsLock.EnterWriteLock())
                        {
                            if (_intCachedHasAnyBreakingSkills < 0) // Just in case
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
                        }
                    }

                    return _intCachedHasAnyBreakingSkills > 0;
                }
            }
        }

        public async ValueTask<bool> GetHasAnyBreakingSkillsAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                using (await _objCachedHasAnyBreakingSkillsLock.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false))
                {
                    token.ThrowIfCancellationRequested();
                    if (_intCachedHasAnyBreakingSkills < 0)
                    {
                        IAsyncDisposable objLocker = await _objCachedHasAnyBreakingSkillsLock.EnterWriteLockAsync(token).ConfigureAwait(false);
                        try
                        {
                            token.ThrowIfCancellationRequested();
                            if (_intCachedHasAnyBreakingSkills < 0) // Just in case
                            {
                                if (SkillList.Count <= 1)
                                    _intCachedHasAnyBreakingSkills = 0;
                                else
                                {
                                    Skill objFirstEnabledSkill = await SkillList
                                        .FirstOrDefaultAsync(
                                            x => x.GetEnabledAsync(token).AsTask(), token)
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
                                                         await x.Specializations.GetCountAsync(token)
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
                                                           && await x.GetTotalBaseRatingAsync(token).ConfigureAwait(false)
                                                           != intFirstSkillTotalBaseRating
                                                           && await x.GetEnabledAsync(token).ConfigureAwait(false), token)
                                            .ConfigureAwait(false)).ToInt32();
                                    }
                                }
                            }
                        }
                        finally
                        {
                            await objLocker.DisposeAsync().ConfigureAwait(false);
                        }
                    }

                    return _intCachedHasAnyBreakingSkills > 0;
                }
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

        public async ValueTask<bool> GetCareerCanIncreaseAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                return await GetUpgradeKarmaCostAsync(token).ConfigureAwait(false)
                       <= await CharacterObject.GetKarmaAsync(token).ConfigureAwait(false)
                       && !await GetIsDisabledAsync(token).ConfigureAwait(false)
                       && !await GetIsBrokenAsync(token).ConfigureAwait(false);
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

        public async ValueTask<int> GetRatingAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                return await GetKarmaAsync(token).ConfigureAwait(false) + await GetBaseAsync(token).ConfigureAwait(false);
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

        public async ValueTask<int> GetFreeBaseAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                return !string.IsNullOrEmpty(Name)
                    ? (await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.SkillGroupBase,
                        false, Name, token: token).ConfigureAwait(false)).StandardRound()
                    : 0;
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

        public async ValueTask<int> GetFreeLevelsAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                return !string.IsNullOrEmpty(Name)
                    ? (await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.SkillGroupLevel,
                        false, Name, token: token).ConfigureAwait(false)).StandardRound()
                    : 0;
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
        public async ValueTask<int> GetRatingMaximumAsync(CancellationToken token = default)
        {
            using (await _objCharacter.LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                CharacterSettings objSettings = await CharacterObject.GetSettingsAsync(token).ConfigureAwait(false);
                return await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false) ||
                       await _objCharacter.GetIgnoreRulesAsync(token).ConfigureAwait(false)
                    ? await objSettings.GetMaxSkillRatingAsync(token).ConfigureAwait(false)
                    : await objSettings.GetMaxSkillRatingCreateAsync(token).ConfigureAwait(false);
            }
        }

        public async ValueTask Upgrade(CancellationToken token = default)
        {
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
                    string strUpgradetext =
                        string.Format(GlobalSettings.CultureInfo, "{0}{4}{1}{4}{2}{4}->{4}{3}",
                                      await LanguageManager.GetStringAsync("String_ExpenseSkillGroup", token: token)
                                                           .ConfigureAwait(false),
                                      await GetCurrentDisplayNameAsync(token).ConfigureAwait(false),
                                      intRating, intRating + 1,
                                      await LanguageManager.GetStringAsync("String_Space", token: token)
                                                           .ConfigureAwait(false));

                    ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
                    objExpense.Create(intPrice * -1, strUpgradetext, ExpenseType.Karma, DateTime.Now);
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
                        objSkillGroup.Add(objSkill);
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
                }

                return objSkillGroup;
            }
        }

        public static async Task<SkillGroup> GetAsync(Skill objSkill, CancellationToken token = default)
        {
            if (objSkill == null)
                return null;
            using (await objSkill.LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                if (objSkill.SkillGroupObject != null)
                    return objSkill.SkillGroupObject;

                if (string.IsNullOrWhiteSpace(objSkill.SkillGroup))
                    return null;

                SkillGroup objSkillGroup =
                    await objSkill.CharacterObject.SkillsSection.SkillGroups
                                  .FindAsync(x => x.Name == objSkill.SkillGroup, token).ConfigureAwait(false);
                if (objSkillGroup != null)
                {
                    if (!objSkillGroup.SkillList.Contains(objSkill))
                        await objSkillGroup.AddAsync(objSkill, token).ConfigureAwait(false);
                }
                else
                {
                    objSkillGroup = new SkillGroup(objSkill.CharacterObject, objSkill.SkillGroup);
                    await objSkillGroup.AddAsync(objSkill, token).ConfigureAwait(false);
                    await objSkill.CharacterObject.SkillsSection.SkillGroups.AddWithSortAsync(objSkillGroup,
                        (x , y) => SkillsSection.CompareSkillGroupsAsync(x, y, token).AsTask(),
                        async (objExistingSkillGroup, objNewSkillGroup) =>
                        {
                            foreach (Skill x in objExistingSkillGroup.SkillList.Where(x =>
                                         !objExistingSkillGroup.SkillList.Contains(x)))
                                await objExistingSkillGroup.AddAsync(x, token).ConfigureAwait(false);
                            await objNewSkillGroup.DisposeAsync().ConfigureAwait(false);
                        }, token: token).ConfigureAwait(false);
                }

                return objSkillGroup;
            }
        }

        public void Add(Skill skill)
        {
            using (LockObject.EnterUpgradeableReadLock())
            {
                using (skill.LockObject.EnterUpgradeableReadLock())
                {
                    Guid guidAddedSkillId = skill.SkillId;
                    // Do not add duplicate skills that we are still in the process of loading
                    if (_lstAffectedSkills.Any(x => x.SkillId == guidAddedSkillId))
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _lstAffectedSkills.Add(skill);
                        using (skill.LockObject.EnterWriteLock())
                            skill.PropertyChanged += SkillOnPropertyChanged;
                        if (_objCharacter?.SkillsSection?.IsLoading != true)
                            OnPropertyChanged(nameof(SkillList));
                    }
                }
            }
        }

        public async ValueTask AddAsync(Skill skill, CancellationToken token = default)
        {
            using (await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                using (await skill.LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false))
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
                    IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        _lstAffectedSkills.Add(skill);
                        IAsyncDisposable objLocker2 =
                            await skill.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                        try
                        {
                            token.ThrowIfCancellationRequested();
                            skill.PropertyChanged += SkillOnPropertyChanged;
                        }
                        finally
                        {
                            await objLocker2.DisposeAsync().ConfigureAwait(false);
                        }
                        if (_objCharacter?.SkillsSection?.IsLoading != true)
                            await OnPropertyChangedAsync(nameof(SkillList), token).ConfigureAwait(false);
                    }
                    finally
                    {
                        await objLocker.DisposeAsync().ConfigureAwait(false);
                    }
                }
            }
        }

        public void Remove(Skill skill)
        {
            using (LockObject.EnterUpgradeableReadLock())
            {
                using (skill.LockObject.EnterUpgradeableReadLock())
                using (LockObject.EnterWriteLock())
                {
                    if (!_lstAffectedSkills.Remove(skill))
                        return;
                    using (skill.LockObject.EnterWriteLock())
                        skill.PropertyChanged -= SkillOnPropertyChanged;
                    if (_objCharacter?.SkillsSection?.IsLoading != true)
                        OnPropertyChanged(nameof(SkillList));
                }
            }
        }

        public async ValueTask RemoveAsync(Skill skill, CancellationToken token = default)
        {
            using (await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                using (await skill.LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false))
                {
                    token.ThrowIfCancellationRequested();
                    IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        if (!_lstAffectedSkills.Remove(skill))
                            return;
                        IAsyncDisposable objLocker2
                            = await skill.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                        try
                        {
                            token.ThrowIfCancellationRequested();
                            skill.PropertyChanged -= SkillOnPropertyChanged;
                        }
                        finally
                        {
                            await objLocker2.DisposeAsync().ConfigureAwait(false);
                        }
                        if (_objCharacter?.SkillsSection?.IsLoading != true)
                            await OnPropertyChangedAsync(nameof(SkillList), token).ConfigureAwait(false);
                    }
                    finally
                    {
                        await objLocker.DisposeAsync().ConfigureAwait(false);
                    }
                }
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

        internal async ValueTask Print(XmlWriter objWriter, CultureInfo objCulture, string strLanguageToPrint, CancellationToken token = default)
        {
            if (objWriter == null)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterHiPrioReadLockAsync(token).ConfigureAwait(false);
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
                        new DependencyGraphNode<string, SkillGroup>(nameof(HasAnyBreakingSkills), x => !x.IsBroken || x.CharacterObject.Settings.AllowSkillRegrouping,
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
                    new DependencyGraphNode<string, SkillGroup>(nameof(KarmaUnbroken), x => x._objCharacter.Settings.UsePointsOnBrokenGroups)
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

        private void SkillOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Skill.BasePoints):
                case nameof(Skill.FreeBase):
                    this.OnMultiplePropertyChanged(nameof(BaseUnbroken), nameof(KarmaUnbroken));
                    break;

                case nameof(Skill.KarmaPoints):
                case nameof(Skill.FreeKarma):
                    OnPropertyChanged(nameof(KarmaUnbroken));
                    break;

                case nameof(Skill.Specializations):
                    if (CharacterObject.Settings.SpecializationsBreakSkillGroups)
                        OnPropertyChanged(nameof(HasAnyBreakingSkills));
                    break;

                case nameof(Skill.TotalBaseRating):
                case nameof(Skill.Enabled):
                    this.OnMultiplePropertyChanged(nameof(HasAnyBreakingSkills),
                                                   nameof(DisplayRating),
                                                   nameof(UpgradeToolTip),
                                                   nameof(CurrentKarmaCost),
                                                   nameof(UpgradeKarmaCost));
                    break;
            }
        }

        private readonly List<Skill> _lstAffectedSkills = new List<Skill>(4);
        private string _strGroupName;
        private readonly Character _objCharacter;

        public SkillGroup(Character objCharacter, string strGroupName = "")
        {
            _objCharacter = objCharacter;
            _strGroupName = strGroupName;
            if (objCharacter == null)
                return;
            objCharacter.PropertyChangedAsync += OnCharacterPropertyChanged;
            objCharacter.Settings.PropertyChangedAsync += OnCharacterSettingsPropertyChanged;
        }

        public Character CharacterObject
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _objCharacter;
            }
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

        public ValueTask<string> GetCurrentDisplayNameAsync(CancellationToken token = default) =>
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

        public async ValueTask<string> DisplayNameAsync(string strLanguage, CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                    return Name;
                return (await (await _objCharacter.LoadDataXPathAsync("skills.xml", strLanguage, token: token)
                                                  .ConfigureAwait(false))
                              .SelectSingleNodeAndCacheExpressionAsync(
                                  "/chummer/skillgroups/name[. = " + Name.CleanXPath() + "]/@translate", token: token)
                              .ConfigureAwait(false))?.Value
                       ?? Name;
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

        public async ValueTask<string> GetDisplayRatingAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
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
        }

        private string _strCachedToolTip = string.Empty;
        private readonly AsyncFriendlyReaderWriterLock _objCachedToolTipLock = new AsyncFriendlyReaderWriterLock();

        public string ToolTip
        {
            get
            {
                using (LockObject.EnterReadLock())
                using (_objCachedToolTipLock.EnterUpgradeableReadLock())
                {
                    if (!string.IsNullOrEmpty(_strCachedToolTip))
                        return _strCachedToolTip;
                    using (_objCachedToolTipLock.EnterWriteLock())
                    {
                        if (!string.IsNullOrEmpty(_strCachedToolTip)) // Just in case
                            return _strCachedToolTip;
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
        }

        public async ValueTask<string> GetToolTipAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                using (await _objCachedToolTipLock.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false))
                {
                    token.ThrowIfCancellationRequested();
                    if (!string.IsNullOrEmpty(_strCachedToolTip))
                        return _strCachedToolTip;
                    IAsyncDisposable objLocker = await _objCachedToolTipLock.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        if (!string.IsNullOrEmpty(_strCachedToolTip)) // Just in case
                            return _strCachedToolTip;
                        using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                   out StringBuilder sbdTooltip))
                        {
                            string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token)
                                .ConfigureAwait(false);
                            (await sbdTooltip
                                .Append(await LanguageManager.GetStringAsync("Tip_SkillGroup_Skills", token: token)
                                    .ConfigureAwait(false)).Append(strSpace)
                                .AppendJoinAsync(',' + strSpace,
                                    SkillList.Select(x => x.GetCurrentDisplayNameAsync(token).AsTask()),
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
                                lstImprovements.AddRange((await ImprovementManager.GetCachedImprovementListForValueOfAsync(
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
                        await objLocker.DisposeAsync().ConfigureAwait(false);
                    }
                }
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

        public async ValueTask<string> GetUpgradeToolTipAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
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

        private readonly List<PropertyChangedAsyncEventHandler> _lstPropertyChangedAsync =
            new List<PropertyChangedAsyncEventHandler>();

        public event PropertyChangedAsyncEventHandler PropertyChangedAsync
        {
            add
            {
                using (LockObject.EnterWriteLock())
                    _lstPropertyChangedAsync.Add(value);
            }
            remove
            {
                using (LockObject.EnterWriteLock())
                    _lstPropertyChangedAsync.Remove(value);
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

        public void OnMultiplePropertyChanged(IReadOnlyCollection<string> lstPropertyNames)
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

                    if (_lstPropertyChangedAsync.Count > 0)
                    {
                        List<PropertyChangedEventArgs> lstArgsList = setNamesOfChangedProperties.Select(x => new PropertyChangedEventArgs(x)).ToList();
                        Func<Task>[] aFuncs = new Func<Task>[lstArgsList.Count * _lstPropertyChangedAsync.Count];
                        int i = 0;
                        foreach (PropertyChangedAsyncEventHandler objEvent in _lstPropertyChangedAsync)
                        {
                            foreach (PropertyChangedEventArgs objArg in lstArgsList)
                                aFuncs[i++] = () => objEvent.Invoke(this, objArg);
                        }

                        Utils.RunWithoutThreadLock(aFuncs, CancellationToken.None);
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

        public async Task OnMultiplePropertyChangedAsync(IReadOnlyCollection<string> lstPropertyNames, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
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

                    IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
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
                        await objLocker.DisposeAsync().ConfigureAwait(false);
                    }

                    if (_lstPropertyChangedAsync.Count > 0)
                    {
                        List<PropertyChangedEventArgs> lstArgsList = setNamesOfChangedProperties.Select(x => new PropertyChangedEventArgs(x)).ToList();
                        List<Task> lstTasks = new List<Task>(Math.Min(lstArgsList.Count * _lstPropertyChangedAsync.Count, Utils.MaxParallelBatchSize));
                        int i = 0;
                        foreach (PropertyChangedAsyncEventHandler objEvent in _lstPropertyChangedAsync)
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
        }

        private Task OnCharacterPropertyChanged(object sender, PropertyChangedEventArgs e, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (CharacterObject?.IsLoading != false)
                return Task.CompletedTask;
            switch (e.PropertyName)
            {
                case nameof(Character.Karma):
                    return OnPropertyChangedAsync(nameof(CareerCanIncrease), token);

                case nameof(Character.EffectiveBuildMethodUsesPriorityTables):
                    return OnPropertyChangedAsync(nameof(BaseUnbroken), token);
            }

            return Task.CompletedTask;
        }

        private async Task OnCharacterSettingsPropertyChanged(object sender, PropertyChangedEventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (CharacterObject?.IsLoading != false && e.PropertyName != nameof(CharacterSettings.AllowSkillRegrouping))
                return;
            switch (e.PropertyName)
            {
                case nameof(CharacterSettings.StrictSkillGroupsInCreateMode):
                    if (CharacterObject?.Created == false && CharacterObject.Settings.StrictSkillGroupsInCreateMode &&
                        !CharacterObject.IgnoreRules)
                    {
                        using (await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false))
                        {
                            token.ThrowIfCancellationRequested();
                            if (await GetKarmaAsync(token).ConfigureAwait(false) > 0)
                            {
                                await SkillList.ForEachAsync(async skill =>
                                {
                                    IAsyncDisposable objLocker = await skill.LockObject.EnterWriteLockAsync(token)
                                        .ConfigureAwait(false);
                                    try
                                    {
                                        token.ThrowIfCancellationRequested();
                                        await skill.SetKarmaPointsAsync(0, token).ConfigureAwait(false);
                                        await skill.SetBasePointsAsync(0, token).ConfigureAwait(false);
                                        await skill.Specializations.RemoveAllAsync(x => !x.Free, token: token).ConfigureAwait(false);
                                    }
                                    finally
                                    {
                                        await objLocker.DisposeAsync().ConfigureAwait(false);
                                    }
                                }, token).ConfigureAwait(false);
                            }
                            else
                            {
                                await SkillList.ForEachAsync(async skill =>
                                {
                                    IAsyncDisposable objLocker = await skill.LockObject.EnterWriteLockAsync(token)
                                        .ConfigureAwait(false);
                                    try
                                    {
                                        await skill.SetBasePointsAsync(0, token).ConfigureAwait(false);
                                    }
                                    finally
                                    {
                                        await objLocker.DisposeAsync().ConfigureAwait(false);
                                    }
                                }, token).ConfigureAwait(false);
                            }
                        }
                    }

                    await OnPropertyChangedAsync(nameof(BaseUnbroken), token).ConfigureAwait(false);
                    break;
                case nameof(CharacterSettings.UsePointsOnBrokenGroups):
                    if (CharacterObject?.Created == false && !CharacterObject.Settings.UsePointsOnBrokenGroups)
                    {
                        using (await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false))
                        {
                            token.ThrowIfCancellationRequested();
                            await SkillList.ForEachAsync(async skill =>
                            {
                                IAsyncDisposable objLocker =
                                    await skill.LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                                try
                                {
                                    await skill.SetBasePointsAsync(0, token).ConfigureAwait(false);
                                }
                                finally
                                {
                                    await objLocker.DisposeAsync().ConfigureAwait(false);
                                }
                            }, token).ConfigureAwait(false);
                        }
                    }

                    await OnPropertyChangedAsync(nameof(BaseUnbroken), token).ConfigureAwait(false);
                    break;

                case nameof(CharacterSettings.KarmaNewSkillGroup):
                case nameof(CharacterSettings.KarmaImproveSkillGroup):
                    await OnPropertyChangedAsync(nameof(CurrentKarmaCost), token).ConfigureAwait(false);
                    break;

                case nameof(CharacterSettings.AllowSkillRegrouping):
                    await UpdateIsBrokenAsync(token).ConfigureAwait(false);
                    break;

                case nameof(CharacterSettings.SpecializationsBreakSkillGroups):
                    await OnPropertyChangedAsync(nameof(HasAnyBreakingSkills), token).ConfigureAwait(false);
                    break;

                case nameof(CharacterSettings.MaxSkillRating):
                    if (await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false)
                        || await _objCharacter.GetIgnoreRulesAsync(token).ConfigureAwait(false))
                        await OnPropertyChangedAsync(nameof(RatingMaximum), token).ConfigureAwait(false);
                    break;

                case nameof(CharacterSettings.MaxSkillRatingCreate):
                    if (!await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false)
                        && !await _objCharacter.GetIgnoreRulesAsync(token).ConfigureAwait(false))
                        await OnPropertyChangedAsync(nameof(RatingMaximum), token).ConfigureAwait(false);
                    break;
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

        public async ValueTask<int> GetCurrentSpCostAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
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

        public async ValueTask<int> GetCurrentKarmaCostAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
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

        public async ValueTask<int> GetUpgradeKarmaCostAsync(CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
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
