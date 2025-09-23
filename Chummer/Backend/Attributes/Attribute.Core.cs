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
using Chummer.Annotations;
using Chummer.Backend.Enums;
using Chummer.Backend.Equipment;

namespace Chummer.Backend.Attributes
{
    /// <summary>
    /// Character CharacterAttribute.
    /// If using databinding, you should generally be using AttributeSection.{ATT}Binding
    /// </summary>
    [HubClassTag("Abbrev", true, "TotalValue", "TotalValue")]
    [DebuggerDisplay("{" + nameof(_strAbbrev) + "}")]
    public sealed class CharacterAttrib : INotifyMultiplePropertiesChangedAsync, IHasLockObject, IHasCharacterObject
    {
        private int _intMetatypeMin = 1;
        private int _intMetatypeMax = 6;
        private int _intMetatypeAugMax = 10;
        private int _intBase;
        private int _intKarma;
        private string _strAbbrev;
        private readonly Character _objCharacter;
        private CharacterSettings _objCharacterSettings;
        private AttributeCategory _eMetatypeCategory;
        private int _intIsDisposed;

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

        public AsyncFriendlyReaderWriterLock LockObject { get; }

        public Character CharacterObject => _objCharacter; // readonly member, no locking required

        #region Constructor, Save, Load, and Print Methods

        /// <summary>
        /// Character CharacterAttribute.
        /// </summary>
        public CharacterAttrib(Character objCharacter, string abbrev, AttributeCategory enumCategory)
        {
            _strAbbrev = abbrev;
            _eMetatypeCategory = enumCategory;
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            _objCharacterSettings = objCharacter.Settings;
            LockObject = objCharacter.LockObject;
            _objCachedTotalValueLock = new AsyncFriendlyReaderWriterLock(LockObject, true);
            objCharacter.MultiplePropertiesChangedAsync += OnCharacterChanged;
            _objCharacterSettings.MultiplePropertiesChangedAsync += OnCharacterSettingsPropertyChanged;
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlWriter objWriter)
        {
            if (objWriter == null)
                return;
            using (LockObject.EnterReadLock())
            {
                objWriter.WriteStartElement("attribute");
                objWriter.WriteElementString("name", _strAbbrev);
                objWriter.WriteElementString("metatypemin",
                    _intMetatypeMin.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("metatypemax",
                    _intMetatypeMax.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("metatypeaugmax",
                    _intMetatypeAugMax.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("base", _intBase.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("karma", _intKarma.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("metatypecategory", _eMetatypeCategory.ToString());
                // External reader friendly stuff.
                objWriter.WriteElementString("totalvalue", TotalValue.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteEndElement();
            }
        }

        /// <summary>
        /// Load the Character Attribute from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            Utils.SafelyRunSynchronously(() => LoadCoreAsync(true, objNode));
        }

        /// <summary>
        /// Load the Character Attribute from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public Task LoadAsync(XmlNode objNode, CancellationToken token = default)
        {
            return LoadCoreAsync(false, objNode, token);
        }

        private async Task LoadCoreAsync(bool blnSync, XmlNode objNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objNode == null)
                return;
            IDisposable objLocker = null;
            IAsyncDisposable objLockerAsync = null;
            if (blnSync)
                objLocker = LockObject.EnterWriteLock(token);
            else
                objLockerAsync = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                objNode.TryGetStringFieldQuickly("name", ref _strAbbrev);
                objNode.TryGetInt32FieldQuickly("metatypemin", ref _intMetatypeMin);
                objNode.TryGetInt32FieldQuickly("metatypemax", ref _intMetatypeMax);
                objNode.TryGetInt32FieldQuickly("metatypeaugmax", ref _intMetatypeAugMax);
                objNode.TryGetInt32FieldQuickly("base", ref _intBase);
                objNode.TryGetInt32FieldQuickly("karma", ref _intKarma);
                if (blnSync)
                {
                    if (!BaseUnlocked && !_objCharacter.Created)
                        _intBase = 0;
                }
                else if (!await GetBaseUnlockedAsync(token).ConfigureAwait(false) && !await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false))
                    _intBase = 0;

                //Converts old attributes to split metatype minimum and base. Saves recalculating Base - TotalMinimum all the time.
                int i = 0;
                if (objNode.TryGetInt32FieldQuickly("value", ref i))
                {
                    i -= _intMetatypeMin;
                    if (blnSync ? BaseUnlocked : await GetBaseUnlockedAsync(token).ConfigureAwait(false))
                    {
                        _intBase = Math.Max(_intBase - _intMetatypeMin, 0);
                        i -= _intBase;
                    }

                    if (i > 0)
                    {
                        _intKarma = i;
                    }
                }

                int intCreateKarma = 0;
                // Shim for that one time karma was split into career and create values
                if (objNode.TryGetInt32FieldQuickly("createkarma", ref intCreateKarma))
                {
                    _intKarma += intCreateKarma;
                }

                if (_intBase < 0)
                    _intBase = 0;
                if (_intKarma < 0)
                    _intKarma = 0;
                if (!AttributeSection.PhysicalAttributes.Contains(Abbrev) && !AttributeSection.MentalAttributes.Contains(Abbrev))
                {
                    _eMetatypeCategory = AttributeCategory.Special;
                }
                else
                {
                    _eMetatypeCategory =
                        ConvertToMetatypeAttributeCategory(objNode["metatypecategory"]?.InnerTextViaPool(token) ?? "Standard");
                }
            }
            finally
            {
                if (blnSync)
                    objLocker.Dispose();
                else
                    await objLockerAsync.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="objCulture">Culture in which to print.</param>
        /// <param name="strLanguageToPrint">Language in which to print.</param>
        /// <param name="token">CancellationToken to listen to.</param>
        internal async Task Print(XmlWriter objWriter, CultureInfo objCulture, string strLanguageToPrint, CancellationToken token = default)
        {
            if (objWriter == null)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                switch (Abbrev)
                {
                    case "MAGAdept":
                        if (!await _objCharacterSettings
                                .GetMysAdeptSecondMAGAttributeAsync(token).ConfigureAwait(false)
                            || !await _objCharacter.GetIsMysticAdeptAsync(token).ConfigureAwait(false)
                            || !await _objCharacter.GetMAGEnabledAsync(token).ConfigureAwait(false))
                            return;
                        break;

                    case "MAG":
                        if (!await _objCharacter.GetMAGEnabledAsync(token).ConfigureAwait(false))
                            return;
                        break;

                    case "RES":
                        if (!await _objCharacter.GetRESEnabledAsync(token).ConfigureAwait(false))
                            return;
                        break;

                    case "DEP":
                        if (!await _objCharacter.GetDEPEnabledAsync(token).ConfigureAwait(false))
                            return;
                        break;
                }

                // <attribute>
                XmlElementWriteHelper objBaseElement =
                    await objWriter.StartElementAsync("attribute", token: token).ConfigureAwait(false);
                try
                {
                    await objWriter.WriteElementStringAsync("name_english", Abbrev, token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("name",
                            await GetDisplayAbbrevAsync(strLanguageToPrint, token).ConfigureAwait(false), token: token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("base",
                            (await GetValueAsync(token).ConfigureAwait(false)).ToString(objCulture), token: token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("total",
                            (await GetTotalValueAsync(token).ConfigureAwait(false)).ToString(objCulture), token: token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("min",
                            (await GetTotalMinimumAsync(token).ConfigureAwait(false)).ToString(objCulture),
                            token: token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("max",
                            (await GetTotalMaximumAsync(token).ConfigureAwait(false)).ToString(objCulture),
                            token: token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("aug",
                        (await GetTotalAugmentedMaximumAsync(token).ConfigureAwait(false)).ToString(objCulture),
                        token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("bp",
                            (await GetTotalKarmaCostAsync(token).ConfigureAwait(false)).ToString(objCulture),
                            token: token)
                        .ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync("metatypecategory", MetatypeCategory.ToString(), token: token)
                        .ConfigureAwait(false);
                }
                finally
                {
                    // </attribute>
                    await objBaseElement.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion Constructor, Save, Load, and Print Methods

        #region Properties

        public AttributeCategory MetatypeCategory
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _eMetatypeCategory;
            }
        }

        /// <summary>
        /// Minimum value for the CharacterAttribute as set by the character's Metatype.
        /// </summary>
        public int RawMetatypeMinimum
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intMetatypeMin;
            }
            // DO NOT MAKE THIS NOT PRIVATE! Instead, create improvements to adjust this because that will play nice with ReplaceAttribute improvements
            private set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    // No need to write lock because interlocked guarantees safety
                    if (Interlocked.Exchange(ref _intMetatypeMin, value) == value)
                        return;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Minimum value for the CharacterAttribute as set by the character's Metatype.
        /// </summary>
        public async Task<int> GetRawMetatypeMinimumAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intMetatypeMin;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Minimum value for the CharacterAttribute as set by the character's Metatype or overwritten attributes nodes.
        /// </summary>
        public int MetatypeMinimum
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    if (MetatypeCategory == AttributeCategory.Shapeshifter)
                        return RawMetatypeMinimum;
                    int intReturn = RawMetatypeMinimum;
                    Improvement objImprovement = _objCharacter.Improvements.LastOrDefault(
                        x => x.ImproveType == Improvement.ImprovementType.ReplaceAttribute && x.ImprovedName == Abbrev
                            && x.Enabled && x.Minimum != 0);
                    if (objImprovement != null)
                    {
                        intReturn = objImprovement.Minimum;
                    }

                    return intReturn;
                }
            }
        }

        /// <summary>
        /// Minimum value for the CharacterAttribute as set by the character's Metatype or overwritten attributes nodes.
        /// </summary>
        public async Task<int> GetMetatypeMinimumAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (MetatypeCategory == AttributeCategory.Shapeshifter)
                    return await GetRawMetatypeMinimumAsync(token).ConfigureAwait(false);
                int intReturn = await GetRawMetatypeMinimumAsync(token).ConfigureAwait(false);
                Improvement objImprovement = await (await _objCharacter.GetImprovementsAsync(token).ConfigureAwait(false)).LastOrDefaultAsync(
                    x => x.ImproveType == Improvement.ImprovementType.ReplaceAttribute && x.ImprovedName == Abbrev
                        && x.Enabled && x.Minimum != 0, token: token).ConfigureAwait(false);
                if (objImprovement != null)
                {
                    intReturn = objImprovement.Minimum;
                }

                return intReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Maximum value for the CharacterAttribute as set by the character's Metatype.
        /// </summary>
        public int RawMetatypeMaximum
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intMetatypeMax;
            }
            // DO NOT MAKE THIS NOT PRIVATE! Instead, create improvements to adjust this because that will play nice with ReplaceAttribute improvements
            private set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    // No need to write lock because interlocked guarantees safety
                    if (Interlocked.Exchange(ref _intMetatypeMax, value) == value)
                        return;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Maximum value for the CharacterAttribute as set by the character's Metatype.
        /// </summary>
        public async Task<int> GetRawMetatypeMaximumAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intMetatypeMax;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Maximum value for the CharacterAttribute as set by the character's Metatype or overwritten attributes nodes.
        /// </summary>
        public int MetatypeMaximum
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    if (Abbrev == "EDG" && _objCharacter.IsAI)
                        return _objCharacter.DEP?.TotalValue ?? (_objCharacter.IsLoading
                            ? RawMetatypeMaximum
                            : throw new NullReferenceException(nameof(Character.DEP)));
                    if (MetatypeCategory == AttributeCategory.Shapeshifter)
                        return RawMetatypeMaximum;
                    int intReturn = RawMetatypeMaximum;
                    Improvement objImprovement = _objCharacter.Improvements.LastOrDefault(x =>
                        x.ImproveType == Improvement.ImprovementType.ReplaceAttribute && x.ImprovedName == Abbrev &&
                        x.Enabled && x.Maximum != 0);
                    if (objImprovement != null)
                    {
                        intReturn = objImprovement.Maximum;
                    }

                    if (Abbrev == "ESS")
                    {
                        intReturn += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.EssenceMax)
                            .StandardRound();
                    }

                    return intReturn;
                }
            }
        }

        /// <summary>
        /// Maximum value for the CharacterAttribute as set by the character's Metatype or overwritten attributes nodes.
        /// </summary>
        public async Task<int> GetMetatypeMaximumAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Abbrev == "EDG" && await _objCharacter.GetIsAIAsync(token).ConfigureAwait(false))
                {
                    CharacterAttrib objDepth = await _objCharacter.GetAttributeAsync("DEP", token: token).ConfigureAwait(false);
                    if (objDepth != null)
                        return await objDepth.GetTotalValueAsync(token).ConfigureAwait(false);
                    return _objCharacter.IsLoading
                        ? await GetRawMetatypeMaximumAsync(token).ConfigureAwait(false)
                        : throw new NullReferenceException(nameof(Character.DEP));
                }

                if (MetatypeCategory == AttributeCategory.Shapeshifter)
                    return await GetRawMetatypeMaximumAsync(token).ConfigureAwait(false);
                int intReturn = await GetRawMetatypeMaximumAsync(token).ConfigureAwait(false);
                Improvement objImprovement = await (await _objCharacter.GetImprovementsAsync(token).ConfigureAwait(false))
                    .LastOrDefaultAsync(
                        x => x.ImproveType == Improvement.ImprovementType.ReplaceAttribute &&
                             x.ImprovedName == Abbrev && x.Enabled && x.Maximum != 0, token: token).ConfigureAwait(false);
                if (objImprovement != null)
                {
                    intReturn = objImprovement.Maximum;
                }

                if (Abbrev == "ESS")
                {
                    intReturn +=
                        (await ImprovementManager
                            .ValueOfAsync(_objCharacter, Improvement.ImprovementType.EssenceMax, token: token)
                            .ConfigureAwait(false)).StandardRound();
                }

                return intReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Maximum augmented value for the CharacterAttribute as set by the character's Metatype.
        /// </summary>
        public int RawMetatypeAugmentedMaximum
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intMetatypeAugMax;
            }
            // DO NOT MAKE THIS NOT PRIVATE! Instead, create improvements to adjust this because that will play nice with ReplaceAttribute improvements
            private set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    // No need to write lock because interlocked guarantees safety
                    if (Interlocked.Exchange(ref _intMetatypeAugMax, value) == value)
                        return;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Maximum augmented value for the CharacterAttribute as set by the character's Metatype.
        /// </summary>
        public async Task<int> GetRawMetatypeAugmentedMaximumAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intMetatypeAugMax;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Maximum augmented value for the CharacterAttribute as set by the character's Metatype or overwritten attributes nodes.
        /// </summary>
        public int MetatypeAugmentedMaximum
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    if (MetatypeCategory == AttributeCategory.Shapeshifter)
                        return RawMetatypeAugmentedMaximum;
                    int intReturn = RawMetatypeAugmentedMaximum;
                    Improvement objImprovement = _objCharacter.Improvements.LastOrDefault(x =>
                        x.ImproveType == Improvement.ImprovementType.ReplaceAttribute && x.ImprovedName == Abbrev &&
                        x.Enabled && x.AugmentedMaximum != 0);
                    if (objImprovement != null)
                    {
                        intReturn = objImprovement.AugmentedMaximum;
                    }

                    return intReturn;
                }
            }
        }

        /// <summary>
        /// Maximum augmented value for the CharacterAttribute as set by the character's Metatype or overwritten attributes nodes.
        /// </summary>
        public async Task<int> GetMetatypeAugmentedMaximumAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (MetatypeCategory == AttributeCategory.Shapeshifter)
                    return await GetRawMetatypeAugmentedMaximumAsync(token).ConfigureAwait(false);
                int intReturn = await GetRawMetatypeAugmentedMaximumAsync(token).ConfigureAwait(false);
                Improvement objImprovement = await (await _objCharacter.GetImprovementsAsync(token).ConfigureAwait(false))
                    .LastOrDefaultAsync(
                        x => x.ImproveType == Improvement.ImprovementType.ReplaceAttribute &&
                             x.ImprovedName == Abbrev && x.Enabled && x.AugmentedMaximum != 0, token: token).ConfigureAwait(false);
                if (objImprovement != null)
                {
                    intReturn = objImprovement.AugmentedMaximum;
                }

                return intReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public void DoBaseFix(bool blnDoBaseOnPropertyChanged = false)
        {
            using (LockObject.EnterWriteLock())
            {
                int intKarmaMaximum = KarmaMaximum;
                while (intKarmaMaximum < 0 && _intBase > 0)
                {
                    blnDoBaseOnPropertyChanged = true;
                    --_intBase;
                    intKarmaMaximum = KarmaMaximum;
                }

                // Very rough fix for when values somehow exceed maxima after loading in. This shouldn't happen in the first place, but this ad-hoc patch will help fix crashes.
                int intPriorityMaximum = PriorityMaximum;
                if (Base > intPriorityMaximum)
                    Base = intPriorityMaximum;
                else if (blnDoBaseOnPropertyChanged)
                    OnPropertyChanged(nameof(Base));

                if (Karma > intKarmaMaximum)
                    Karma = intKarmaMaximum;
            }
        }

        public async Task DoBaseFixAsync(bool blnDoBaseOnPropertyChanged = false, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intKarmaMaximum = await GetKarmaMaximumAsync(token).ConfigureAwait(false);
                while (intKarmaMaximum < 0 && _intBase > 0)
                {
                    blnDoBaseOnPropertyChanged = true;
                    --_intBase;
                    intKarmaMaximum = await GetKarmaMaximumAsync(token).ConfigureAwait(false);
                }

                // Very rough fix for when values somehow exceed maxima after loading in. This shouldn't happen in the first place, but this ad-hoc patch will help fix crashes.
                int intPriorityMaximum = await GetPriorityMaximumAsync(token).ConfigureAwait(false);
                if (await GetBaseAsync(token).ConfigureAwait(false) > intPriorityMaximum)
                    await SetBaseAsync(intPriorityMaximum, token).ConfigureAwait(false);
                else if (blnDoBaseOnPropertyChanged)
                    await OnPropertyChangedAsync(nameof(Base), token).ConfigureAwait(false);

                if (await GetKarmaAsync(token).ConfigureAwait(false) > intKarmaMaximum)
                    await SetKarmaAsync(intKarmaMaximum, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Current base value (priority points spent) of the CharacterAttribute.
        /// </summary>
        public int Base
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intBase;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_intBase == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _intBase = value;
                        DoBaseFix(true);
                    }
                }
            }
        }

        /// <summary>
        /// Current base value (priority points spent) of the CharacterAttribute.
        /// </summary>
        public async Task<int> GetBaseAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intBase;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Current base value (priority points spent) of the CharacterAttribute.
        /// </summary>
        public async Task SetBaseAsync(int value, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intBase == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _intBase = value;
                    await DoBaseFixAsync(true, token).ConfigureAwait(false);
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
        /// Current base value (priority points spent) of the CharacterAttribute.
        /// </summary>
        public async Task ModifyBaseAsync(int value, CancellationToken token = default)
        {
            if (value == 0)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                Interlocked.Add(ref _intBase, value);
                await DoBaseFixAsync(true, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Total Value of Base Points as used by internal methods
        /// </summary>
        public int TotalBase
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return Math.Max(Base + FreeBase + RawMinimum, TotalMinimum);
            }
        }

        /// <summary>
        /// Total Value of Base Points as used by internal methods
        /// </summary>
        public async Task<int> GetTotalBaseAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return Math.Max(
                    await GetBaseAsync(token).ConfigureAwait(false) +
                    await GetFreeBaseAsync(token).ConfigureAwait(false) +
                    await GetRawMinimumAsync(token).ConfigureAwait(false),
                    await GetTotalMinimumAsync(token).ConfigureAwait(false));
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
                    return Math.Min(
                        ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.Attributelevel, false,
                            Abbrev),
                        MetatypeMaximum - MetatypeMinimum).StandardRound();
                }
            }
        }

        public async Task<int> GetFreeBaseAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return Math.Min(
                    await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.Attributelevel,
                        false,
                        Abbrev, token: token).ConfigureAwait(false),
                    await GetMetatypeMaximumAsync(token).ConfigureAwait(false) -
                    await GetMetatypeMinimumAsync(token).ConfigureAwait(false)).StandardRound();
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Current karma value of the CharacterAttribute.
        /// </summary>
        public int Karma
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarma;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    // No need to write lock because interlocked guarantees safety
                    if (Interlocked.Exchange(ref _intKarma, value) == value)
                        return;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Current karma value of the CharacterAttribute.
        /// </summary>
        public async Task<int> GetKarmaAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarma;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Current karma value of the CharacterAttribute.
        /// </summary>
        public async Task SetKarmaAsync(int value, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // No need to write lock because interlocked guarantees safety
                if (Interlocked.Exchange(ref _intKarma, value) == value)
                    return;
                await OnPropertyChangedAsync(nameof(Karma), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Current karma value of the CharacterAttribute.
        /// </summary>
        public async Task ModifyKarmaAsync(int value, CancellationToken token = default)
        {
            if (value == 0)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                Interlocked.Add(ref _intKarma, value);
                await OnPropertyChangedAsync(nameof(Karma), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private int _intCachedValue = int.MinValue;

        /// <summary>
        /// Current value of the CharacterAttribute before modifiers are applied.
        /// </summary>
        public int Value
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    // intReturn for thread safety
                    int intReturn = _intCachedValue;
                    if (intReturn != int.MinValue)
                        return intReturn;
                    return _intCachedValue
                        = Math.Min(
                            Math.Max(Base + FreeBase + RawMinimum + AttributeValueModifiers, TotalMinimum)
                            + Karma,
                            TotalMaximum);
                }
            }
        }

        /// <summary>
        /// Current value of the CharacterAttribute before modifiers are applied.
        /// </summary>
        public async Task<int> GetValueAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // intReturn for thread safety
                int intReturn = _intCachedValue;
                if (intReturn != int.MinValue)
                    return intReturn;
                return _intCachedValue = Math.Min(Math.Max(
                                                    await GetBaseAsync(token).ConfigureAwait(false)
                                                        + await GetFreeBaseAsync(token).ConfigureAwait(false)
                                                        + await GetRawMinimumAsync(token).ConfigureAwait(false)
                                                        + await GetAttributeValueModifiersAsync(token)
                                                                .ConfigureAwait(false),
                                                    await GetTotalMinimumAsync(token).ConfigureAwait(false))
                                                + await GetKarmaAsync(token).ConfigureAwait(false),
                                            await GetTotalMaximumAsync(token).ConfigureAwait(false));
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The CharacterAttribute's combined Minimum and Maximum values (Metatype Min/Max + Modifiers) before essence modifiers are applied.
        /// </summary>
        public ValueTuple<int, int> MinimumMaximumNoEssenceLoss(bool blnUseEssenceAtSpecialStart = false)
        {
            using (LockObject.EnterReadLock())
            {
                // If we're looking at MAG and the character is a Cyberzombie, MAG is always 1, regardless of ESS penalties and bonuses.
                if (_objCharacter.MetatypeCategory == "Cyberzombie" && (Abbrev == "MAG" || Abbrev == "MAGAdept"))
                {
                    return new ValueTuple<int, int>(1, 1);
                }

                int intRawMinimum = MetatypeMinimum;
                int intRawMaximum = MetatypeMaximum;
                int intMinimumLossFromEssence = 0;
                int intMaximumLossFromEssence = 0;
                List<Improvement> lstModifiers =
                    ImprovementManager.GetCachedImprovementListForValueOf(_objCharacter,
                        Improvement.ImprovementType.Attribute, Abbrev);
                foreach (Improvement objImprovement in lstModifiers)
                {
                    if (objImprovement.ImproveSource != Improvement.ImprovementSource.EssenceLoss
                        && objImprovement.ImproveSource != Improvement.ImprovementSource.EssenceLossChargen
                        && objImprovement.ImproveSource != Improvement.ImprovementSource.CyberadeptDaemon)
                    {
                        intRawMinimum += objImprovement.Minimum * objImprovement.Rating;
                        intRawMaximum += objImprovement.Maximum * objImprovement.Rating;
                    }
                    else
                    {
                        intMinimumLossFromEssence += objImprovement.Minimum * objImprovement.Rating;
                        intMaximumLossFromEssence += objImprovement.Maximum * objImprovement.Rating;
                    }
                }

                lstModifiers = ImprovementManager.GetCachedImprovementListForValueOf(_objCharacter,
                    Improvement.ImprovementType.Attribute, Abbrev + "Base");
                foreach (Improvement objImprovement in lstModifiers)
                {
                    if (objImprovement.ImproveSource != Improvement.ImprovementSource.EssenceLoss
                        && objImprovement.ImproveSource != Improvement.ImprovementSource.EssenceLossChargen
                        && objImprovement.ImproveSource != Improvement.ImprovementSource.CyberadeptDaemon)
                    {
                        intRawMinimum += objImprovement.Minimum * objImprovement.Rating;
                        intRawMaximum += objImprovement.Maximum * objImprovement.Rating;
                    }
                    else
                    {
                        intMinimumLossFromEssence += objImprovement.Minimum * objImprovement.Rating;
                        intMaximumLossFromEssence += objImprovement.Maximum * objImprovement.Rating;
                    }
                }

                int intMaxLossFromEssence = blnUseEssenceAtSpecialStart
                    ? CharacterObject.EssenceAtSpecialStart.StandardRound() - CharacterObject.ESS.MetatypeMaximum
                    : 0;
                int intTotalMinimum = intRawMinimum + Math.Max(intMinimumLossFromEssence, intMaxLossFromEssence);
                int intTotalMaximum = intRawMaximum + Math.Max(intMaximumLossFromEssence, intMaxLossFromEssence);

                if (intTotalMinimum < 1)
                {
                    if (_objCharacter.IsCritter || MetatypeMaximum == 0 || Abbrev == "EDG" || Abbrev == "MAG" ||
                        Abbrev == "MAGAdept" || Abbrev == "RES" || Abbrev == "DEP")
                        intTotalMinimum = 0;
                    else
                        intTotalMinimum = 1;
                }

                if (intTotalMaximum < intTotalMinimum)
                    intTotalMaximum = intTotalMinimum;

                return new ValueTuple<int, int>(intTotalMinimum, intTotalMaximum);
            }
        }

        /// <summary>
        /// The CharacterAttribute's combined Minimum and Maximum values (Metatype Min/Max + Modifiers) before essence modifiers are applied.
        /// </summary>
        public async Task<ValueTuple<int, int>> MinimumMaximumNoEssenceLossAsync(bool blnUseEssenceAtSpecialStart = false, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // If we're looking at MAG and the character is a Cyberzombie, MAG is always 1, regardless of ESS penalties and bonuses.
                if (await _objCharacter.GetMetatypeCategoryAsync(token).ConfigureAwait(false) == "Cyberzombie"
                    && (Abbrev == "MAG" || Abbrev == "MAGAdept"))
                {
                    return new ValueTuple<int, int>(1, 1);
                }

                int intRawMaximumBase = await GetMetatypeMaximumAsync(token).ConfigureAwait(false);
                int intRawMinimum = await GetMetatypeMinimumAsync(token).ConfigureAwait(false);
                int intRawMaximum = intRawMaximumBase;
                int intMinimumLossFromEssence = 0;
                int intMaximumLossFromEssence = 0;
                List<Improvement> lstModifiers = await ImprovementManager
                    .GetCachedImprovementListForValueOfAsync(_objCharacter, Improvement.ImprovementType.Attribute,
                        Abbrev, token: token).ConfigureAwait(false);
                foreach (Improvement objImprovement in lstModifiers)
                {
                    if (objImprovement.ImproveSource != Improvement.ImprovementSource.EssenceLoss
                        && objImprovement.ImproveSource != Improvement.ImprovementSource.EssenceLossChargen
                        && objImprovement.ImproveSource != Improvement.ImprovementSource.CyberadeptDaemon)
                    {
                        intRawMinimum += objImprovement.Minimum * objImprovement.Rating;
                        intRawMaximum += objImprovement.Maximum * objImprovement.Rating;
                    }
                    else
                    {
                        intMinimumLossFromEssence += objImprovement.Minimum * objImprovement.Rating;
                        intMaximumLossFromEssence += objImprovement.Maximum * objImprovement.Rating;
                    }
                }

                lstModifiers = await ImprovementManager.GetCachedImprovementListForValueOfAsync(_objCharacter,
                    Improvement.ImprovementType.Attribute, Abbrev + "Base", token: token).ConfigureAwait(false);
                foreach (Improvement objImprovement in lstModifiers)
                {
                    if (objImprovement.ImproveSource != Improvement.ImprovementSource.EssenceLoss
                        && objImprovement.ImproveSource != Improvement.ImprovementSource.EssenceLossChargen
                        && objImprovement.ImproveSource != Improvement.ImprovementSource.CyberadeptDaemon)
                    {
                        intRawMinimum += objImprovement.Minimum * objImprovement.Rating;
                        intRawMaximum += objImprovement.Maximum * objImprovement.Rating;
                    }
                    else
                    {
                        intMinimumLossFromEssence += objImprovement.Minimum * objImprovement.Rating;
                        intMaximumLossFromEssence += objImprovement.Maximum * objImprovement.Rating;
                    }
                }

                int intMaxLossFromEssence = blnUseEssenceAtSpecialStart
                    ? (await CharacterObject.GetEssenceAtSpecialStartAsync(token).ConfigureAwait(false)).StandardRound() -
                      await (await CharacterObject.GetAttributeAsync("ESS", token: token).ConfigureAwait(false)).GetMetatypeMaximumAsync(token).ConfigureAwait(false)
                    : 0;
                int intTotalMinimum = intRawMinimum + Math.Max(intMinimumLossFromEssence, intMaxLossFromEssence);
                int intTotalMaximum = intRawMaximum + Math.Max(intMaximumLossFromEssence, intMaxLossFromEssence);

                if (intTotalMinimum < 1)
                {
                    if (await _objCharacter.GetIsCritterAsync(token).ConfigureAwait(false)
                        || intRawMaximumBase == 0 || Abbrev == "EDG" || Abbrev == "MAG" ||
                        Abbrev == "MAGAdept" || Abbrev == "RES" || Abbrev == "DEP")
                        intTotalMinimum = 0;
                    else
                        intTotalMinimum = 1;
                }

                if (intTotalMaximum < intTotalMinimum)
                    intTotalMaximum = intTotalMinimum;

                return new ValueTuple<int, int>(intTotalMinimum, intTotalMaximum);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The CharacterAttribute's combined Maximum value (Metatype Maximum + Modifiers) before essence modifiers are applied.
        /// </summary>
        public int MaximumNoEssenceLoss(bool blnUseEssenceAtSpecialStart = false)
        {
            return MinimumMaximumNoEssenceLoss(blnUseEssenceAtSpecialStart).Item2;
        }

        /// <summary>
        /// The CharacterAttribute's combined Maximum value (Metatype Maximum + Modifiers) before essence modifiers are applied.
        /// </summary>
        public async Task<int> MaximumNoEssenceLossAsync(bool blnUseEssenceAtSpecialStart = false, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return (await MinimumMaximumNoEssenceLossAsync(blnUseEssenceAtSpecialStart, token).ConfigureAwait(false)).Item2;
        }

        /// <summary>
        /// The CharacterAttribute's combined Minimum value (Metatype Minimum + Modifiers) before essence modifiers are applied.
        /// </summary>
        public int MinimumNoEssenceLoss(bool blnUseEssenceAtSpecialStart = false)
        {
            return MinimumMaximumNoEssenceLoss(blnUseEssenceAtSpecialStart).Item1;
        }

        /// <summary>
        /// The CharacterAttribute's combined Minimum value (Metatype Minimum + Modifiers) before essence modifiers are applied.
        /// </summary>
        public async Task<int> MinimumNoEssenceLossAsync(bool blnUseEssenceAtSpecialStart = false, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return (await MinimumMaximumNoEssenceLossAsync(blnUseEssenceAtSpecialStart, token).ConfigureAwait(false)).Item1;
        }

        /// <summary>
        /// Formatted Value of the attribute, including the sum of any modifiers in brackets.
        /// </summary>
        public string DisplayValue
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    return HasModifiers()
                        ? string.Format(GlobalSettings.CultureInfo, "{0}{1}({2})", Value,
                            LanguageManager.GetString("String_Space"), TotalValue)
                        : Value.ToString(GlobalSettings.CultureInfo);
                }
            }
        }

        /// <summary>
        /// Formatted Value of the attribute, including the sum of any modifiers in brackets.
        /// </summary>
        public async Task<string> GetDisplayValueAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intValue = await GetValueAsync(token).ConfigureAwait(false);
                return await HasModifiersAsync(token).ConfigureAwait(false)
                    ? string.Format(GlobalSettings.CultureInfo, "{0}{1}({2})", intValue,
                        await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false),
                        await GetTotalValueAsync(token).ConfigureAwait(false))
                    : intValue.ToString(GlobalSettings.CultureInfo);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The total amount of the modifiers that affect the CharacterAttribute's value without affecting Karma costs.
        /// </summary>
        public int AttributeModifiers
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    int intReturn = ImprovementManager
                        .AugmentedValueOf(_objCharacter, Improvement.ImprovementType.Attribute, false, Abbrev)
                        .StandardRound();
                    //The most that any attribute can be increased by is 4, plus/minus any improvements that affect the augmented max.
                    int intModifiersClamp = MetatypeAugmentedMaximum - MetatypeMaximum + AugmentedMaximumModifiers;
                    if (ImprovementManager
                            .GetCachedImprovementListForValueOf(_objCharacter,
                                Improvement.ImprovementType.AttributeMaxClamp,
                                Abbrev).Count > 0)
                        intModifiersClamp = Math.Min(intModifiersClamp, TotalMaximum - Value);
                    return Math.Min(intReturn, intModifiersClamp);
                }
            }
        }

        /// <summary>
        /// The total amount of the modifiers that affect the CharacterAttribute's value without affecting Karma costs.
        /// </summary>
        public async Task<int> GetAttributeModifiersAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intReturn = (await ImprovementManager
                        .AugmentedValueOfAsync(_objCharacter, Improvement.ImprovementType.Attribute, false, Abbrev,
                            token: token).ConfigureAwait(false))
                    .StandardRound();
                //The most that any attribute can be increased by is 4, plus/minus any improvements that affect the augmented max.
                int intModifiersClamp = await GetMetatypeAugmentedMaximumAsync(token).ConfigureAwait(false) -
                                        await GetMetatypeMaximumAsync(token).ConfigureAwait(false) +
                                        await GetAugmentedMaximumModifiersAsync(token).ConfigureAwait(false);
                if ((await ImprovementManager
                        .GetCachedImprovementListForValueOfAsync(_objCharacter,
                            Improvement.ImprovementType.AttributeMaxClamp,
                            Abbrev, token: token).ConfigureAwait(false)).Count > 0)
                    intModifiersClamp = Math.Min(intModifiersClamp, await GetTotalMaximumAsync(token).ConfigureAwait(false) - await GetValueAsync(token).ConfigureAwait(false));
                return Math.Min(intReturn, intModifiersClamp);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The total amount of the modifiers that raise the actual value of the CharacterAttribute and increase its Karma cost.
        /// </summary>
        public int AttributeValueModifiers
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return ImprovementManager
                        .AugmentedValueOf(_objCharacter, Improvement.ImprovementType.Attribute, false, Abbrev + "Base")
                        .StandardRound();
            }
        }

        /// <summary>
        /// The total amount of the modifiers that raise the actual value of the CharacterAttribute and increase its Karma cost.
        /// </summary>
        public async Task<int> GetAttributeValueModifiersAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return (await ImprovementManager.AugmentedValueOfAsync(_objCharacter,
                        Improvement.ImprovementType.Attribute, false, Abbrev + "Base", token: token)
                    .ConfigureAwait(false))
                    .StandardRound();
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether the CharacterAttribute has any modifiers from Improvements.
        /// </summary>
        public bool HasModifiers(CancellationToken token = default)
        {
            using (LockObject.EnterReadLock(token))
            {
                foreach (Improvement objImprovement in ImprovementManager
                             .GetCachedImprovementListForAugmentedValueOf(
                                 _objCharacter, Improvement.ImprovementType.Attribute, Abbrev, token: token))
                {
                    if (objImprovement.Rating != 0 && objImprovement.Augmented != 0)
                        return true;
                    if ((objImprovement.ImproveSource == Improvement.ImprovementSource.EssenceLoss ||
                         objImprovement.ImproveSource == Improvement.ImprovementSource.EssenceLossChargen ||
                         objImprovement.ImproveSource == Improvement.ImprovementSource.CyberadeptDaemon) &&
                        (_objCharacter.MAGEnabled && (Abbrev == "MAG" || Abbrev == "MAGAdept") ||
                         _objCharacter.RESEnabled && Abbrev == "RES" ||
                         _objCharacter.DEPEnabled && Abbrev == "DEP"))
                        return true;
                }

                foreach (Improvement objImprovement in ImprovementManager
                             .GetCachedImprovementListForAugmentedValueOf(
                                 _objCharacter, Improvement.ImprovementType.Attribute, Abbrev + "Base", token: token))
                {
                    if (objImprovement.Rating != 0 && objImprovement.Augmented != 0)
                        return true;
                    if ((objImprovement.ImproveSource == Improvement.ImprovementSource.EssenceLoss ||
                         objImprovement.ImproveSource == Improvement.ImprovementSource.EssenceLossChargen ||
                         objImprovement.ImproveSource == Improvement.ImprovementSource.CyberadeptDaemon) &&
                        (_objCharacter.MAGEnabled && (Abbrev == "MAG" || Abbrev == "MAGAdept") ||
                         _objCharacter.RESEnabled && Abbrev == "RES" ||
                         _objCharacter.DEPEnabled && Abbrev == "DEP"))
                        return true;
                }

                // If this is AGI or STR, factor in any Cyberlimbs.
                if (!_objCharacterSettings.DontUseCyberlimbCalculation &&
                    Cyberware.CyberlimbAttributeAbbrevs.Contains(Abbrev))
                {
                    return _objCharacter.Cyberware.Any(objCyberware => objCyberware.IsLimb && objCyberware.IsModularCurrentlyEquipped, token);
                }

                return false;
            }
        }

        /// <summary>
        /// Whether the CharacterAttribute has any modifiers from Improvements.
        /// </summary>
        public async Task<bool> HasModifiersAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                foreach (Improvement objImprovement in await ImprovementManager
                             .GetCachedImprovementListForAugmentedValueOfAsync(
                                 _objCharacter, Improvement.ImprovementType.Attribute, Abbrev, token: token)
                             .ConfigureAwait(false))
                {
                    if (objImprovement.Rating != 0 && objImprovement.Augmented != 0)
                        return true;
                    if ((objImprovement.ImproveSource == Improvement.ImprovementSource.EssenceLoss ||
                         objImprovement.ImproveSource == Improvement.ImprovementSource.EssenceLossChargen ||
                         objImprovement.ImproveSource == Improvement.ImprovementSource.CyberadeptDaemon) &&
                        (await _objCharacter.GetMAGEnabledAsync(token).ConfigureAwait(false) && (Abbrev == "MAG" || Abbrev == "MAGAdept") ||
                         await _objCharacter.GetRESEnabledAsync(token).ConfigureAwait(false) && Abbrev == "RES" ||
                         await _objCharacter.GetDEPEnabledAsync(token).ConfigureAwait(false) && Abbrev == "DEP"))
                        return true;
                }

                foreach (Improvement objImprovement in await ImprovementManager
                             .GetCachedImprovementListForAugmentedValueOfAsync(
                                 _objCharacter, Improvement.ImprovementType.Attribute, Abbrev + "Base", token: token)
                             .ConfigureAwait(false))
                {
                    if (objImprovement.Rating != 0 && objImprovement.Augmented != 0)
                        return true;
                    if ((objImprovement.ImproveSource == Improvement.ImprovementSource.EssenceLoss ||
                         objImprovement.ImproveSource == Improvement.ImprovementSource.EssenceLossChargen ||
                         objImprovement.ImproveSource == Improvement.ImprovementSource.CyberadeptDaemon) &&
                        (await _objCharacter.GetMAGEnabledAsync(token).ConfigureAwait(false) && (Abbrev == "MAG" || Abbrev == "MAGAdept") ||
                         await _objCharacter.GetRESEnabledAsync(token).ConfigureAwait(false) && Abbrev == "RES" ||
                         await _objCharacter.GetDEPEnabledAsync(token).ConfigureAwait(false) && Abbrev == "DEP"))
                        return true;
                }

                // If this is AGI or STR, factor in any Cyberlimbs.
                if (!await _objCharacterSettings.GetDontUseCyberlimbCalculationAsync(token).ConfigureAwait(false) &&
                    Cyberware.CyberlimbAttributeAbbrevs.Contains(Abbrev))
                {
                    return await (await _objCharacter.GetCyberwareAsync(token).ConfigureAwait(false))
                        .AnyAsync(
                            async objCyberware => await objCyberware.GetIsLimbAsync(token).ConfigureAwait(false) &&
                                                  await objCyberware.GetIsModularCurrentlyEquippedAsync(token)
                                                      .ConfigureAwait(false), token: token).ConfigureAwait(false);
                }

                return false;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The total amount of the modifiers that affect the CharacterAttribute's Minimum value.
        /// </summary>
        public int MinimumModifiers
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    int intModifier = 0;
                    List<Improvement> lstModifiers =
                        ImprovementManager.GetCachedImprovementListForValueOf(_objCharacter,
                            Improvement.ImprovementType.Attribute, Abbrev);
                    foreach (Improvement objImprovement in lstModifiers)
                    {
                        intModifier += objImprovement.Minimum * objImprovement.Rating;
                    }

                    lstModifiers = ImprovementManager.GetCachedImprovementListForValueOf(_objCharacter,
                        Improvement.ImprovementType.Attribute, Abbrev + "Base");
                    foreach (Improvement objImprovement in lstModifiers)
                    {
                        intModifier += objImprovement.Minimum * objImprovement.Rating;
                    }

                    return intModifier;
                }
            }
        }

        /// <summary>
        /// The total amount of the modifiers that affect the CharacterAttribute's Minimum value.
        /// </summary>
        public async Task<int> GetMinimumModifiersAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intModifier = 0;
                List<Improvement> lstModifiers = await ImprovementManager
                    .GetCachedImprovementListForValueOfAsync(_objCharacter, Improvement.ImprovementType.Attribute,
                        Abbrev, token: token).ConfigureAwait(false);
                foreach (Improvement objImprovement in lstModifiers)
                {
                    intModifier += objImprovement.Minimum * objImprovement.Rating;
                }

                lstModifiers = await ImprovementManager.GetCachedImprovementListForValueOfAsync(_objCharacter,
                    Improvement.ImprovementType.Attribute, Abbrev + "Base", token: token).ConfigureAwait(false);
                foreach (Improvement objImprovement in lstModifiers)
                {
                    intModifier += objImprovement.Minimum * objImprovement.Rating;
                }

                return intModifier;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The total amount of the modifiers that affect the CharacterAttribute's Maximum value.
        /// </summary>
        public int MaximumModifiers
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    int intModifier = 0;
                    List<Improvement> lstModifiers =
                        ImprovementManager.GetCachedImprovementListForValueOf(_objCharacter,
                            Improvement.ImprovementType.Attribute, Abbrev);
                    foreach (Improvement objImprovement in lstModifiers)
                    {
                        intModifier += objImprovement.Maximum * objImprovement.Rating;
                    }

                    lstModifiers = ImprovementManager.GetCachedImprovementListForValueOf(_objCharacter,
                        Improvement.ImprovementType.Attribute, Abbrev + "Base");
                    foreach (Improvement objImprovement in lstModifiers)
                    {
                        intModifier += objImprovement.Maximum * objImprovement.Rating;
                    }

                    return intModifier;
                }
            }
        }

        /// <summary>
        /// The total amount of the modifiers that affect the CharacterAttribute's Maximum value.
        /// </summary>
        public async Task<int> GetMaximumModifiersAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intModifier = 0;
                List<Improvement> lstModifiers = await ImprovementManager
                    .GetCachedImprovementListForValueOfAsync(_objCharacter, Improvement.ImprovementType.Attribute,
                        Abbrev, token: token).ConfigureAwait(false);
                foreach (Improvement objImprovement in lstModifiers)
                {
                    intModifier += objImprovement.Maximum * objImprovement.Rating;
                }

                lstModifiers = await ImprovementManager.GetCachedImprovementListForValueOfAsync(_objCharacter,
                    Improvement.ImprovementType.Attribute, Abbrev + "Base", token: token).ConfigureAwait(false);
                foreach (Improvement objImprovement in lstModifiers)
                {
                    intModifier += objImprovement.Maximum * objImprovement.Rating;
                }

                return intModifier;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The total amount of the modifiers that affect the CharacterAttribute's Augmented Maximum value.
        /// </summary>
        public int AugmentedMaximumModifiers
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    int intModifier = 0;
                    foreach (Improvement objImprovement in ImprovementManager.GetCachedImprovementListForValueOf(
                                 _objCharacter, Improvement.ImprovementType.Attribute, Abbrev))
                    {
                        intModifier += objImprovement.AugmentedMaximum * objImprovement.Rating;
                    }

                    return intModifier;
                }
            }
        }

        /// <summary>
        /// The total amount of the modifiers that affect the CharacterAttribute's Augmented Maximum value.
        /// </summary>
        public async Task<int> GetAugmentedMaximumModifiersAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intModifier = 0;
                List<Improvement> lstModifiers = await ImprovementManager
                    .GetCachedImprovementListForValueOfAsync(_objCharacter, Improvement.ImprovementType.Attribute,
                        Abbrev, token: token).ConfigureAwait(false);
                foreach (Improvement objImprovement in lstModifiers)
                {
                    intModifier += objImprovement.AugmentedMaximum * objImprovement.Rating;
                }

                return intModifier;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The CharacterAttribute's total value (Value + Modifiers).
        /// </summary>
        public int CalculatedTotalValue(bool blnIncludeCyberlimbs = true, CancellationToken token = default)
        {
            return Utils.SafelyRunSynchronously(() => CalculatedTotalValueCore(true, blnIncludeCyberlimbs, token), token);
        }

        /// <summary>
        /// The CharacterAttribute's total value (Value + Modifiers).
        /// </summary>
        public Task<int> CalculatedTotalValueAsync(bool blnIncludeCyberlimbs = true, CancellationToken token = default)
        {
            return CalculatedTotalValueCore(false, blnIncludeCyberlimbs, token);
        }

        /// <summary>
        /// The CharacterAttribute's total value (Value + Modifiers).
        /// </summary>
        private async Task<int> CalculatedTotalValueCore(bool blnSync, bool blnIncludeCyberlimbs = true,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IDisposable objLocker = null;
            IAsyncDisposable objLockerAsync = null;
            if (blnSync)
                // ReSharper disable once MethodHasAsyncOverload
                objLocker = LockObject.EnterReadLock(token);
            else
                objLockerAsync = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // If we're looking at MAG and the character is a Cyberzombie, MAG is always 1, regardless of ESS penalties and bonuses.
                string strMetatypeCategory = blnSync
                    ? _objCharacter.MetatypeCategory
                    : await _objCharacter.GetMetatypeCategoryAsync(token).ConfigureAwait(false);
                if (strMetatypeCategory == "Cyberzombie" && (Abbrev == "MAG" || Abbrev == "MAGAdept"))
                    return 1;

                int intMeat = blnSync
                    ? Value + AttributeModifiers
                    : await GetValueAsync(token).ConfigureAwait(false) +
                      await GetAttributeModifiersAsync(token).ConfigureAwait(false);
                int intReturn = intMeat;

                int intPureCyberValue = 0;
                int intLimbCount = 0;
                // If this is AGI or STR, factor in any Cyberlimbs.
                if (blnIncludeCyberlimbs
                    && !(blnSync
                            ? _objCharacterSettings.DontUseCyberlimbCalculation
                            : await _objCharacterSettings.GetDontUseCyberlimbCalculationAsync(token).ConfigureAwait(false))
                    && Cyberware.CyberlimbAttributeAbbrevs.Contains(Abbrev))
                {
                    int intLimbTotal;
                    if (blnSync)
                        (intLimbCount, intLimbTotal) = ProcessCyberlimbs(_objCharacter.Cyberware);
                    else
                        (intLimbCount, intLimbTotal) =
                            await ProcessCyberlimbsAsync(await _objCharacter.GetCyberwareAsync(token)
                                .ConfigureAwait(false)).ConfigureAwait(false);

                    ValueTuple<int, int> ProcessCyberlimbs(IEnumerable<Cyberware> lstToCheck)
                    {
                        int intLimbCountReturn = 0;
                        int intLimbTotalReturn = 0;
                        foreach (Cyberware objCyberware in lstToCheck)
                        {
                            if (!objCyberware.IsModularCurrentlyEquipped)
                                continue;
                            if (objCyberware.IsLimb)
                            {
                                if (_objCharacterSettings.ExcludeLimbSlot.Contains(objCyberware.LimbSlot))
                                    continue;

                                int intLoop = objCyberware.LimbSlotCount;
                                intLimbCountReturn += intLoop;
                                intLimbTotalReturn += objCyberware.GetAttributeTotalValue(Abbrev) *
                                                      intLoop;
                            }
                            else
                            {
                                (int intLoop1, int intLoop2) = ProcessCyberlimbs(objCyberware.Children);
                                intLimbCountReturn += intLoop1;
                                intLimbTotalReturn += intLoop2;
                            }
                        }

                        return new ValueTuple<int, int>(intLimbCountReturn, intLimbTotalReturn);
                    }

                    async Task<ValueTuple<int, int>> ProcessCyberlimbsAsync(IAsyncEnumerable<Cyberware> lstToCheck)
                    {
                        int intLimbCountReturn = 0;
                        int intLimbTotalReturn = 0;
                        await lstToCheck.ForEachAsync(async objCyberware =>
                        {
                            if (!await objCyberware.GetIsModularCurrentlyEquippedAsync(token).ConfigureAwait(false))
                                return;
                            if (await objCyberware.GetIsLimbAsync(token).ConfigureAwait(false))
                            {
                                if ((await _objCharacterSettings
                                        .GetExcludeLimbSlotAsync(token).ConfigureAwait(false))
                                    .Contains(objCyberware.LimbSlot))
                                    return;

                                int intLoop = await objCyberware.GetLimbSlotCountAsync(token).ConfigureAwait(false);
                                intLimbCountReturn += intLoop;
                                intLimbTotalReturn += await objCyberware.GetAttributeTotalValueAsync(Abbrev, token)
                                    .ConfigureAwait(false) * intLoop;
                            }
                            else
                            {
                                (int intLoop1, int intLoop2) = await ProcessCyberlimbsAsync(await objCyberware.GetChildrenAsync(token).ConfigureAwait(false))
                                    .ConfigureAwait(false);
                                intLimbCountReturn += intLoop1;
                                intLimbTotalReturn += intLoop2;
                            }
                        }, token).ConfigureAwait(false);

                        return new ValueTuple<int, int>(intLimbCountReturn, intLimbTotalReturn);
                    }

                    if (intLimbCount > 0)
                    {
                        // ReSharper disable once MethodHasAsyncOverload
                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                        int intMaxLimbs = blnSync
                            ? _objCharacter.LimbCount()
                            : await _objCharacter.LimbCountAsync(token: token).ConfigureAwait(false);
                        int intMissingLimbCount = Math.Max(intMaxLimbs - intLimbCount, 0);
                        intPureCyberValue = intLimbTotal;
                        // Not all limbs have been replaced, so we need to place the Attribute in the other "limbs" to get the average value.
                        intLimbTotal += Math.Max(intMeat, 0) * intMissingLimbCount;
                        intReturn = (intLimbTotal + intMaxLimbs - 1) / intMaxLimbs;
                    }
                }

                // Do not let the CharacterAttribute go above the Metatype's Augmented Maximum.
                intReturn = Math.Min(intReturn,
                    blnSync ? TotalAugmentedMaximum : await GetTotalAugmentedMaximumAsync(token).ConfigureAwait(false));

                // An Attribute cannot go below 1 unless it is EDG, MAG, or RES, the character is a Critter, the Metatype Maximum is 0, or it is caused by encumbrance (or a custom improvement).
                if (intReturn < 1)
                {
                    if ((blnSync
                            ? _objCharacter.CritterEnabled
                            : await _objCharacter.GetCritterEnabledAsync(token).ConfigureAwait(false)) ||
                        (blnSync ? MetatypeMaximum : await GetMetatypeMaximumAsync(token).ConfigureAwait(false)) == 0 ||
                        Abbrev == "EDG" || Abbrev == "RES" || Abbrev == "MAG" || Abbrev == "MAGAdept" ||
                        (Abbrev == "DEP" && strMetatypeCategory != "A.I."))
                        return 0;
                    List<Improvement> lstUsedImprovements;
                    decimal decImprovementValue;
                    if (blnSync)
                        decImprovementValue = ImprovementManager.AugmentedValueOf(
                            _objCharacter, Improvement.ImprovementType.Attribute,
                            out lstUsedImprovements,
                            strImprovedName: Abbrev, token: token);
                    else
                        (decImprovementValue, lstUsedImprovements)
                            = await ImprovementManager.AugmentedValueOfTupleAsync(
                                _objCharacter, Improvement.ImprovementType.Attribute, strImprovedName: Abbrev,
                                token: token).ConfigureAwait(false);
                    if (decImprovementValue < 0)
                    {
                        decimal decTotalCustomImprovements =
                            lstUsedImprovements.Sum(x => x.Custom, x => x.Augmented * x.Rating, token: token);
                        if (decTotalCustomImprovements < 0)
                            return 0;
                    }

                    switch (Abbrev)
                    {
                        case "STR":
                        {
                            // Special case for cyberlimbs: if every limb has been replaced with a modular connector with an attribute of 0, we allow the augmented attribute to be 0
                            if (intLimbCount > 0 && intPureCyberValue == 0)
                                return 0;
                            break;
                        }
                        case "REA":
                        {
                            decimal decTotalEncumbrance = lstUsedImprovements.Sum(
                                x => x.ImproveSource == Improvement.ImprovementSource
                                    .ArmorEncumbrance, x => x.Augmented * x.Rating, token: token);
                            if (decTotalEncumbrance < 0)
                                return 0;
                            break;
                        }
                        case "AGI":
                        {
                            // Special case for cyberlimbs: if every limb has been replaced with a modular connector with an attribute of 0, we allow the augmented attribute to be 0
                            if (intLimbCount > 0 && intPureCyberValue == 0)
                                return 0;
                            decimal decTotalEncumbrance = lstUsedImprovements.Sum(
                                x => x.ImproveSource == Improvement.ImprovementSource
                                    .ArmorEncumbrance, x => x.Augmented * x.Rating, token: token);
                            if (decTotalEncumbrance < 0)
                                return 0;
                            break;
                        }
                    }

                    return 1;
                }

                return intReturn;
            }
            finally
            {
                objLocker?.Dispose();
                if (objLockerAsync != null)
                    await objLockerAsync.DisposeAsync().ConfigureAwait(false);
            }
        }

        private int _intCachedTotalValue = int.MinValue;

        private readonly AsyncFriendlyReaderWriterLock _objCachedTotalValueLock;

        /// <summary>
        /// The CharacterAttribute's total value (Value + Modifiers).
        /// </summary>
        [HubTag("TotalValue")]
        public int TotalValue
        {
            get
            {
                using (_objCachedTotalValueLock.EnterReadLock())
                {
                    // intReturn for thread safety
                    if (_intCachedTotalValue != int.MinValue)
                        return _intCachedTotalValue;
                }

                using (_objCachedTotalValueLock.EnterUpgradeableReadLock())
                {
                    // intReturn for thread safety
                    if (_intCachedTotalValue != int.MinValue)
                        return _intCachedTotalValue;
                    using (_objCachedTotalValueLock.EnterWriteLock())
                    {
                        return _intCachedTotalValue = CalculatedTotalValue();
                    }
                }
            }
        }

        /// <summary>
        /// The CharacterAttribute's total value (Value + Modifiers).
        /// </summary>
        public async Task<int> GetTotalValueAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await _objCachedTotalValueLock.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intCachedTotalValue != int.MinValue)
                    return _intCachedTotalValue;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            objLocker =
                await _objCachedTotalValueLock.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intCachedTotalValue != int.MinValue)
                    return _intCachedTotalValue;
                IAsyncDisposable objLocker2 =
                    await _objCachedTotalValueLock.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    return _intCachedTotalValue = await CalculatedTotalValueAsync(token: token).ConfigureAwait(false);
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
        /// The CharacterAttribute's combined Minimum value (Metatype Minimum + Modifiers), uncapped by its zero.
        /// </summary>
        public int RawMinimum
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _objCharacterSettings.UnclampAttributeMinimum
                        ? MetatypeMinimum + MinimumModifiers
                        : Math.Max(MetatypeMinimum + MinimumModifiers, 0);
            }
        }

        /// <summary>
        /// The CharacterAttribute's combined Minimum value (Metatype Minimum + Modifiers), uncapped by its zero.
        /// </summary>
        public async Task<int> GetRawMinimumAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intReturn = await GetMetatypeMinimumAsync(token).ConfigureAwait(false) +
                                await GetMinimumModifiersAsync(token).ConfigureAwait(false);
                if (!_objCharacterSettings.UnclampAttributeMinimum && intReturn < 0)
                    intReturn = 0;
                return intReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The CharacterAttribute's combined Minimum value (Metatype Minimum + Modifiers).
        /// </summary>
        public int TotalMinimum
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    // If we're looking at MAG and the character is a Cyberzombie, MAG is always 1, regardless of ESS penalties and bonuses.
                    if (_objCharacter.MetatypeCategory == "Cyberzombie" && (Abbrev == "MAG" || Abbrev == "MAGAdept"))
                        return 1;

                    int intReturn = RawMinimum;
                    if (intReturn < 1)
                    {
                        if (_objCharacter.IsCritter || TotalMaximum == 0 || Abbrev == "EDG" || Abbrev == "MAG" ||
                            Abbrev == "MAGAdept" || Abbrev == "RES" || Abbrev == "DEP")
                            intReturn = 0;
                        else
                            intReturn = 1;
                    }

                    return intReturn;
                }
            }
        }

        /// <summary>
        /// The CharacterAttribute's combined Minimum value (Metatype Minimum + Modifiers).
        /// </summary>
        public async Task<int> GetTotalMinimumAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // If we're looking at MAG and the character is a Cyberzombie, MAG is always 1, regardless of ESS penalties and bonuses.
                if (await _objCharacter.GetMetatypeCategoryAsync(token).ConfigureAwait(false) == "Cyberzombie"
                    && (Abbrev == "MAG" || Abbrev == "MAGAdept"))
                    return 1;

                int intReturn = await GetRawMinimumAsync(token).ConfigureAwait(false);
                if (intReturn < 1)
                {
                    if (await _objCharacter.GetIsCritterAsync(token).ConfigureAwait(false) || await GetTotalMaximumAsync(token).ConfigureAwait(false) == 0 ||
                        Abbrev == "EDG" || Abbrev == "MAG" || Abbrev == "MAGAdept" || Abbrev == "RES" ||
                        Abbrev == "DEP")
                        intReturn = 0;
                    else
                        intReturn = 1;
                }

                return intReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The CharacterAttribute's combined Maximum value (Metatype Maximum + Modifiers).
        /// </summary>
        public int TotalMaximum
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    // If we're looking at MAG and the character is a Cyberzombie, MAG is always 1, regardless of ESS penalties and bonuses.
                    if (_objCharacter.MetatypeCategory == "Cyberzombie" && (Abbrev == "MAG" || Abbrev == "MAGAdept"))
                        return 1;

                    return Math.Max(0, MetatypeMaximum + MaximumModifiers);
                }
            }
        }

        /// <summary>
        /// The CharacterAttribute's combined Maximum value (Metatype Maximum + Modifiers).
        /// </summary>
        public async Task<int> GetTotalMaximumAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // If we're looking at MAG and the character is a Cyberzombie, MAG is always 1, regardless of ESS penalties and bonuses.
                if (await _objCharacter.GetMetatypeCategoryAsync(token).ConfigureAwait(false) == "Cyberzombie" && (Abbrev == "MAG" || Abbrev == "MAGAdept"))
                    return 1;

                return Math.Max(0,
                    await GetMetatypeMaximumAsync(token).ConfigureAwait(false) +
                    await GetMaximumModifiersAsync(token).ConfigureAwait(false));
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The CharacterAttribute's combined Augmented Maximum value (Metatype Augmented Maximum + Modifiers).
        /// </summary>
        public int TotalAugmentedMaximum
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    // If we're looking at MAG and the character is a Cyberzombie, MAG is always 1, regardless of ESS penalties and bonuses.
                    if (_objCharacter.MetatypeCategory == "Cyberzombie" && (Abbrev == "MAG" || Abbrev == "MAGAdept"))
                        return 1;

                    return ImprovementManager.GetCachedImprovementListForValueOf(_objCharacter,
                        Improvement.ImprovementType.AttributeMaxClamp, Abbrev).Count > 0
                        ? TotalMaximum
                        : Math.Max(0, MetatypeAugmentedMaximum + MaximumModifiers + AugmentedMaximumModifiers);
                }
            }
        }

        /// <summary>
        /// The CharacterAttribute's combined Augmented Maximum value (Metatype Augmented Maximum + Modifiers).
        /// </summary>
        public async Task<int> GetTotalAugmentedMaximumAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // If we're looking at MAG and the character is a Cyberzombie, MAG is always 1, regardless of ESS penalties and bonuses.
                if (await _objCharacter.GetMetatypeCategoryAsync(token).ConfigureAwait(false) == "Cyberzombie"
                    && (Abbrev == "MAG" || Abbrev == "MAGAdept"))
                    return 1;

                return (await ImprovementManager.GetCachedImprovementListForValueOfAsync(_objCharacter,
                           Improvement.ImprovementType.AttributeMaxClamp, Abbrev, token: token).ConfigureAwait(false))
                       .Count >
                       0
                    ? await GetTotalMaximumAsync(token).ConfigureAwait(false)
                    : Math.Max(0,
                        await GetMetatypeAugmentedMaximumAsync(token).ConfigureAwait(false) +
                        await GetMaximumModifiersAsync(token).ConfigureAwait(false) +
                        await GetAugmentedMaximumModifiersAsync(token).ConfigureAwait(false));
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// CharacterAttribute abbreviation.
        /// </summary>
        public string Abbrev => _strAbbrev;

        public string DisplayNameShort(string strLanguage)
        {
            return GetDisplayAbbrev(strLanguage);
        }

        public Task<string> DisplayNameShortAsync(string strLanguage, CancellationToken token = default)
        {
            return GetDisplayAbbrevAsync(strLanguage, token);
        }

        public string DisplayNameLong(string strLanguage)
        {
            using (LockObject.EnterReadLock())
            {
                return Abbrev == "MAGAdept"
                    ? LanguageManager.MAGAdeptString(strLanguage, true)
                    : LanguageManager.GetString("String_Attribute" + Abbrev + "Long", strLanguage);
            }
        }

        public async Task<string> DisplayNameLongAsync(string strLanguage, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return Abbrev == "MAGAdept"
                    ? await LanguageManager.MAGAdeptStringAsync(strLanguage, true, token).ConfigureAwait(false)
                    : await LanguageManager.GetStringAsync("String_Attribute" + Abbrev + "Long", strLanguage, token: token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string DisplayNameFormatted => GetDisplayNameFormatted(GlobalSettings.Language);

        public Task<string> GetDisplayNameFormattedAsync(CancellationToken token = default) => GetDisplayNameFormattedAsync(GlobalSettings.Language, token);

        public string GetDisplayNameFormatted(string strLanguage)
        {
            using (LockObject.EnterReadLock())
            {
                string strSpacePlusParen = LanguageManager.GetString("String_Space", strLanguage) + "(";
                if (Abbrev == "MAGAdept")
                    return LanguageManager.GetString("String_AttributeMAGLong", strLanguage) + strSpacePlusParen +
                           LanguageManager.GetString("String_AttributeMAGShort", strLanguage) + ")"
                           + strSpacePlusParen + LanguageManager.GetString("String_DescAdept", strLanguage) + ")";

                return DisplayNameLong(strLanguage) + strSpacePlusParen + DisplayNameShort(strLanguage) + ")";
            }
        }

        public async Task<string> GetDisplayNameFormattedAsync(string strLanguage, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                string strSpacePlusParen = await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token)
                    .ConfigureAwait(false) + "(";
                if (Abbrev == "MAGAdept")
                    return await LanguageManager.GetStringAsync("String_AttributeMAGLong", strLanguage, token: token)
                               .ConfigureAwait(false) + strSpacePlusParen + await LanguageManager
                               .GetStringAsync("String_AttributeMAGShort", strLanguage, token: token)
                               .ConfigureAwait(false) + ")"
                           + strSpacePlusParen + await LanguageManager
                               .GetStringAsync("String_DescAdept", strLanguage, token: token).ConfigureAwait(false) + ")";

                return await DisplayNameLongAsync(strLanguage, token).ConfigureAwait(false) + strSpacePlusParen +
                       await DisplayNameShortAsync(strLanguage, token).ConfigureAwait(false) + ")";
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Is it possible to place points in Base or is it prevented by their build method?
        /// </summary>
        public bool BaseUnlocked => _objCharacter.EffectiveBuildMethodUsesPriorityTables;

        /// <summary>
        /// Is it possible to place points in Base or is it prevented by their build method?
        /// </summary>
        public Task<bool> GetBaseUnlockedAsync(CancellationToken token = default) =>
            _objCharacter.GetEffectiveBuildMethodUsesPriorityTablesAsync(token);

        /// <summary>
        /// CharacterAttribute Limits
        /// </summary>
        public string AugmentedMetatypeLimits
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return string.Format(GlobalSettings.CultureInfo, "{1}{0}/{0}{2}{0}({3})",
                        LanguageManager.GetString("String_Space"), TotalMinimum, TotalMaximum, TotalAugmentedMaximum);
            }
        }

        /// <summary>
        /// CharacterAttribute Limits
        /// </summary>
        public async Task<string> GetAugmentedMetatypeLimitsAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return string.Format(GlobalSettings.CultureInfo, "{1}{0}/{0}{2}{0}({3})",
                    await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false),
                    await GetTotalMinimumAsync(token).ConfigureAwait(false),
                    await GetTotalMaximumAsync(token).ConfigureAwait(false),
                    await GetTotalAugmentedMaximumAsync(token).ConfigureAwait(false));
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Set the minimum, maximum, and augmented values for the CharacterAttribute based on string values from the Metatype XML file.
        /// </summary>
        /// <param name="intMin">Metatype's minimum value for the CharacterAttribute.</param>
        /// <param name="intMax">Metatype's maximum value for the CharacterAttribute.</param>
        /// <param name="intAug">Metatype's maximum augmented value for the CharacterAttribute.</param>
        public void AssignLimits(int intMin, int intMax, int intAug)
        {
            using (LockObject.EnterUpgradeableReadLock())
            {
                bool blnMinChanged = _intMetatypeMin != intMin;
                bool blnMaxChanged = _intMetatypeMax != intMax;
                bool blnAugMaxChanged = _intMetatypeAugMax != intAug;
                if (!blnMinChanged && !blnMaxChanged && !blnAugMaxChanged)
                    return;
                using (LockObject.EnterWriteLock())
                {
                    _intMetatypeMin = intMin;
                    _intMetatypeMax = intMax;
                    _intMetatypeAugMax = intAug;

                    List<string> lstProperties = new List<string>(3);
                    if (blnMinChanged)
                        lstProperties.Add(nameof(RawMetatypeMinimum));
                    if (blnMaxChanged)
                        lstProperties.Add(nameof(RawMetatypeMaximum));
                    if (blnAugMaxChanged)
                        lstProperties.Add(nameof(RawMetatypeAugmentedMaximum));

                    OnMultiplePropertiesChanged(lstProperties);
                }
            }
        }

        /// <summary>
        /// Set the minimum, maximum, and augmented values for the CharacterAttribute based on string values from the Metatype XML file.
        /// </summary>
        /// <param name="intMin">Metatype's minimum value for the CharacterAttribute.</param>
        /// <param name="intMax">Metatype's maximum value for the CharacterAttribute.</param>
        /// <param name="intAug">Metatype's maximum augmented value for the CharacterAttribute.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task AssignLimitsAsync(int intMin, int intMax, int intAug, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                bool blnMinChanged = _intMetatypeMin != intMin;
                bool blnMaxChanged = _intMetatypeMax != intMax;
                bool blnAugMaxChanged = _intMetatypeAugMax != intAug;
                if (!blnMinChanged && !blnMaxChanged && !blnAugMaxChanged)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _intMetatypeMin = intMin;
                    _intMetatypeMax = intMax;
                    _intMetatypeAugMax = intAug;

                    List<string> lstProperties = new List<string>(3);
                    if (blnMinChanged)
                        lstProperties.Add(nameof(RawMetatypeMinimum));
                    if (blnMaxChanged)
                        lstProperties.Add(nameof(RawMetatypeMaximum));
                    if (blnAugMaxChanged)
                        lstProperties.Add(nameof(RawMetatypeAugmentedMaximum));

                    await OnMultiplePropertiesChangedAsync(lstProperties, token).ConfigureAwait(false);
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
        /// Set the base, karma, minimum, maximum, and augmented values for the CharacterAttribute all at once, the last three based on string values from the Metatype XML file.
        /// </summary>
        /// <param name="intBase">Base value to use.</param>
        /// <param name="intKarma">Karma value to use.</param>
        /// <param name="intMin">Metatype's minimum value for the CharacterAttribute.</param>
        /// <param name="intMax">Metatype's maximum value for the CharacterAttribute.</param>
        /// <param name="intAug">Metatype's maximum augmented value for the CharacterAttribute.</param>
        public void AssignBaseKarmaLimits(int intBase, int intKarma, int intMin, int intMax, int intAug)
        {
            using (LockObject.EnterUpgradeableReadLock())
            {
                bool blnBaseChanged = _intBase != intBase;
                bool blnKarmaChanged = _intKarma != intKarma;
                bool blnMinChanged = _intMetatypeMin != intMin;
                bool blnMaxChanged = _intMetatypeMax != intMax;
                bool blnAugMaxChanged = _intMetatypeAugMax != intAug;
                if (!blnBaseChanged && !blnKarmaChanged && !blnMinChanged && !blnMaxChanged && !blnAugMaxChanged)
                    return;
                using (LockObject.EnterWriteLock())
                {
                    _intBase = intBase;
                    _intKarma = intKarma;
                    _intMetatypeMin = intMin;
                    _intMetatypeMax = intMax;
                    _intMetatypeAugMax = intAug;

                    List<string> lstProperties = new List<string>(5);
                    if (blnBaseChanged)
                        lstProperties.Add(nameof(Base));
                    if (blnKarmaChanged)
                        lstProperties.Add(nameof(Karma));
                    if (blnMinChanged)
                        lstProperties.Add(nameof(RawMetatypeMinimum));
                    if (blnMaxChanged)
                        lstProperties.Add(nameof(RawMetatypeMaximum));
                    if (blnAugMaxChanged)
                        lstProperties.Add(nameof(RawMetatypeAugmentedMaximum));

                    OnMultiplePropertiesChanged(lstProperties);
                }
            }
        }

        /// <summary>
        /// Set the minimum, maximum, and augmented values for the CharacterAttribute all at once, the last three based on string values from the Metatype XML file.
        /// </summary>
        /// <param name="intBase">Base value to use.</param>
        /// <param name="intKarma">Karma value to use.</param>
        /// <param name="intMin">Metatype's minimum value for the CharacterAttribute.</param>
        /// <param name="intMax">Metatype's maximum value for the CharacterAttribute.</param>
        /// <param name="intAug">Metatype's maximum augmented value for the CharacterAttribute.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task AssignBaseKarmaLimitsAsync(int intBase, int intKarma, int intMin, int intMax, int intAug, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                bool blnBaseChanged = _intBase != intBase;
                bool blnKarmaChanged = _intKarma != intKarma;
                bool blnMinChanged = _intMetatypeMin != intMin;
                bool blnMaxChanged = _intMetatypeMax != intMax;
                bool blnAugMaxChanged = _intMetatypeAugMax != intAug;
                if (!blnBaseChanged && !blnKarmaChanged && !blnMinChanged && !blnMaxChanged && !blnAugMaxChanged)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _intBase = intBase;
                    _intKarma = intKarma;
                    _intMetatypeMin = intMin;
                    _intMetatypeMax = intMax;
                    _intMetatypeAugMax = intAug;

                    List<string> lstProperties = new List<string>(5);
                    if (blnBaseChanged)
                        lstProperties.Add(nameof(Base));
                    if (blnKarmaChanged)
                        lstProperties.Add(nameof(Karma));
                    if (blnMinChanged)
                        lstProperties.Add(nameof(RawMetatypeMinimum));
                    if (blnMaxChanged)
                        lstProperties.Add(nameof(RawMetatypeMaximum));
                    if (blnAugMaxChanged)
                        lstProperties.Add(nameof(RawMetatypeAugmentedMaximum));

                    await OnMultiplePropertiesChangedAsync(lstProperties, token).ConfigureAwait(false);
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
                    return UpgradeKarmaCost < 0
                        ? LanguageManager.GetString("Tip_ImproveItemAtMaximum")
                        : string.Format(
                            GlobalSettings.CultureInfo,
                            LanguageManager.GetString("Tip_ImproveItem"),
                            Value + 1,
                            UpgradeKarmaCost);
            }
        }

        public async Task<string> GetUpgradeToolTipAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intUpgradeCost = await GetUpgradeKarmaCostAsync(token).ConfigureAwait(false);
                return intUpgradeCost < 0
                    ? await LanguageManager.GetStringAsync("Tip_ImproveItemAtMaximum", token: token)
                        .ConfigureAwait(false)
                    : string.Format(
                        GlobalSettings.CultureInfo,
                        await LanguageManager.GetStringAsync("Tip_ImproveItem", token: token).ConfigureAwait(false),
                        await GetValueAsync(token).ConfigureAwait(false) + 1,
                        intUpgradeCost);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private string _strCachedToolTip = string.Empty;

        /// <summary>
        /// ToolTip that shows how the CharacterAttribute is calculating its Modified Rating.
        /// </summary>
        public string ToolTip
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    // strReturn for thread safety
                    string strReturn = _strCachedToolTip;
                    if (!string.IsNullOrEmpty(strReturn))
                        return strReturn;

                    string strSpace = LanguageManager.GetString("String_Space");

                    using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                    out HashSet<string> setUniqueNames))
                    {
                        decimal decBaseValue = 0;

                        List<Improvement> lstUsedImprovements
                            = ImprovementManager.GetCachedImprovementListForAugmentedValueOf(
                                _objCharacter, Improvement.ImprovementType.Attribute, Abbrev);

                        List<ValueTuple<string, decimal, string>> lstUniquePair =
                            new List<ValueTuple<string, decimal, string>>(lstUsedImprovements.Count);

                        using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                                      out StringBuilder sbdModifier))
                        {
                            foreach (Improvement objImprovement in lstUsedImprovements.Where(
                                         objImprovement => !objImprovement.Custom))
                            {
                                if (objImprovement.Rating == 0 || objImprovement.Augmented == 0)
                                    continue;
                                string strUniqueName = objImprovement.UniqueName;
                                if (!string.IsNullOrEmpty(strUniqueName) && strUniqueName != "enableattribute"
                                                                         && objImprovement.ImproveType
                                                                         == Improvement.ImprovementType.Attribute
                                                                         && objImprovement.ImprovedName == Abbrev)
                                {
                                    // If this has a UniqueName, run through the current list of UniqueNames seen. If it is not already in the list, add it.
                                    setUniqueNames.Add(strUniqueName);

                                    // Add the values to the UniquePair List so we can check them later.
                                    lstUniquePair.Add(new ValueTuple<string, decimal, string>(
                                                          strUniqueName,
                                                          objImprovement.Augmented * objImprovement.Rating,
                                                          _objCharacter.GetObjectName(
                                                              objImprovement, GlobalSettings.Language)));
                                }
                                else
                                {
                                    decimal decValue = objImprovement.Augmented * objImprovement.Rating;
                                    sbdModifier.Append(strSpace).Append('+').Append(strSpace)
                                               .Append(
                                                   _objCharacter.GetObjectName(objImprovement, GlobalSettings.Language))
                                               .Append(strSpace).Append('(')
                                               .Append(decValue.ToString(GlobalSettings.CultureInfo))
                                               .Append(')');
                                    decBaseValue += decValue;
                                }
                            }

                            if (setUniqueNames.Contains("precedence0"))
                            {
                                // Retrieve only the highest precedence0 value.
                                // Run through the list of UniqueNames and pick out the highest value for each one.
                                decimal decHighest = decimal.MinValue;

                                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                                              out StringBuilder sbdNewModifier))
                                {
                                    foreach ((string strGroupName, decimal decValue, string strSourceName) in
                                             lstUniquePair)
                                    {
                                        if (strGroupName != "precedence0" || decValue <= decHighest)
                                            continue;
                                        decHighest = decValue;
                                        sbdNewModifier.Clear();
                                        sbdNewModifier
                                            .Append(strSpace).Append('+').Append(strSpace).Append(strSourceName)
                                            .Append(strSpace).Append('(')
                                            .Append(decValue.ToString(GlobalSettings.CultureInfo)).Append(')');
                                    }

                                    if (setUniqueNames.Contains("precedence-1"))
                                    {
                                        foreach ((string strGroupName, decimal decValue, string strSourceName) in
                                                 lstUniquePair)
                                        {
                                            if (strGroupName != "precedence-1")
                                                continue;
                                            decHighest += decValue;
                                            sbdNewModifier
                                                .Append(strSpace).Append('+').Append(strSpace).Append(strSourceName)
                                                .Append(strSpace).Append('(')
                                                .Append(decValue.ToString(GlobalSettings.CultureInfo)).Append(')');
                                        }
                                    }

                                    if (decHighest >= decBaseValue)
                                    {
                                        sbdModifier.Clear();
                                        sbdModifier.Append(sbdNewModifier);
                                    }
                                }
                            }
                            else if (setUniqueNames.Contains("precedence1"))
                            {
                                // Retrieve all the items that are precedence1 and nothing else.
                                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                                              out StringBuilder sbdNewModifier))
                                {
                                    foreach ((string _, decimal decValue, string strSourceName) in lstUniquePair
                                                 .Where(
                                                     s => s.Item1 == "precedence1" || s.Item1 == "precedence-1"))
                                    {
                                        sbdNewModifier.AppendFormat(GlobalSettings.CultureInfo,
                                                                    "{0}+{0}{1}{0}({2})",
                                                                    strSpace,
                                                                    strSourceName, decValue);
                                    }

                                    sbdModifier.Clear();
                                    sbdModifier.Append(sbdNewModifier);
                                }
                            }
                            else
                            {
                                // Run through the list of UniqueNames and pick out the highest value for each one.
                                foreach (string strName in setUniqueNames)
                                {
                                    decimal decHighest = decimal.MinValue;
                                    foreach ((string strGroupName, decimal decValue, string strSourceName) in
                                             lstUniquePair)
                                    {
                                        if (strGroupName == strName && decValue > decHighest)
                                        {
                                            decHighest = decValue;
                                            sbdModifier.Append(strSpace).Append('+').Append(strSpace)
                                                       .Append(strSourceName)
                                                       .Append(strSpace).Append('(')
                                                       .Append(decValue.ToString(GlobalSettings.CultureInfo))
                                                       .Append(')');
                                        }
                                    }
                                }
                            }

                            // Factor in Custom Improvements.
                            setUniqueNames.Clear();
                            lstUniquePair.Clear();
                            foreach (Improvement objImprovement in lstUsedImprovements.Where(
                                         objImprovement => objImprovement.Custom))
                            {
                                if (objImprovement.Rating == 0 || objImprovement.Augmented == 0)
                                    continue;
                                string strUniqueName = objImprovement.UniqueName;
                                if (!string.IsNullOrEmpty(strUniqueName))
                                {
                                    // If this has a UniqueName, run through the current list of UniqueNames seen. If it is not already in the list, add it.
                                    setUniqueNames.Add(strUniqueName);

                                    // Add the values to the UniquePair List so we can check them later.
                                    lstUniquePair.Add(new ValueTuple<string, decimal, string>(
                                                          strUniqueName,
                                                          objImprovement.Augmented * objImprovement.Rating,
                                                          _objCharacter.GetObjectName(
                                                              objImprovement, GlobalSettings.Language)));
                                }
                                else
                                {
                                    sbdModifier.Append(strSpace).Append('+').Append(strSpace)
                                               .Append(
                                                   _objCharacter.GetObjectName(objImprovement, GlobalSettings.Language))
                                               .Append(strSpace).Append('(')
                                               .Append((objImprovement.Augmented * objImprovement.Rating).ToString(
                                                           GlobalSettings.CultureInfo)).Append(')');
                                }
                            }

                            // Run through the list of UniqueNames and pick out the highest value for each one.
                            foreach (string strName in setUniqueNames)
                            {
                                decimal decHighest = decimal.MinValue;
                                foreach ((string strGroupName, decimal decValue, string strSourceName) in
                                         lstUniquePair)
                                {
                                    if (strGroupName != strName || decValue <= decHighest)
                                        continue;
                                    decHighest = decValue;
                                    sbdModifier.Append(strSpace).Append('+').Append(strSpace).Append(strSourceName)
                                               .Append(strSpace).Append('(')
                                               .Append(decValue.ToString(GlobalSettings.CultureInfo)).Append(')');
                                }
                            }

                            //// If this is AGI or STR, factor in any Cyberlimbs.
                            if (!_objCharacterSettings.DontUseCyberlimbCalculation &&
                                Cyberware.CyberlimbAttributeAbbrevs.Contains(Abbrev))
                            {
                                _objCharacter.Cyberware.ForEach(objCyberware => BuildTooltip(sbdModifier, objCyberware, strSpace));
                            }

                            return _strCachedToolTip = CurrentDisplayAbbrev + strSpace + "("
                                                       + Value.ToString(GlobalSettings.CultureInfo) + ")" +
                                                       sbdModifier.ToString();
                        }
                    }
                }

                void BuildTooltip(StringBuilder sbdModifier, Cyberware objCyberware, string strSpace)
                {
                    if (!objCyberware.IsLimb || !objCyberware.IsModularCurrentlyEquipped)
                    {
                        return;
                    }

                    if (objCyberware.InheritAttributes)
                    {
                        objCyberware.Children.ForEach(objChild => BuildTooltip(sbdModifier, objChild, strSpace));

                        return;
                    }

                    sbdModifier.AppendLine()
                        .Append(objCyberware.CurrentDisplayName)
                        .Append(strSpace)
                        .Append('(')
                        .Append(objCyberware.GetAttributeTotalValue(Abbrev).ToString(GlobalSettings.CultureInfo))
                        .Append(')');
                }
            }
        }

        /// <summary>
        /// ToolTip that shows how the CharacterAttribute is calculating its Modified Rating.
        /// </summary>
        public async Task<string> GetToolTipAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // strReturn for thread safety
                string strReturn = _strCachedToolTip;
                if (!string.IsNullOrEmpty(strReturn))
                    return strReturn;

                string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);

                using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                out HashSet<string> setUniqueNames))
                {
                    decimal decBaseValue = 0;

                    List<Improvement> lstUsedImprovements
                        = await ImprovementManager.GetCachedImprovementListForAugmentedValueOfAsync(
                            _objCharacter, Improvement.ImprovementType.Attribute, Abbrev, token: token).ConfigureAwait(false);

                    List<ValueTuple<string, decimal, string>> lstUniquePair =
                        new List<ValueTuple<string, decimal, string>>(lstUsedImprovements.Count);

                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder sbdModifier))
                    {
                        foreach (Improvement objImprovement in lstUsedImprovements.Where(
                                     objImprovement => !objImprovement.Custom))
                        {
                            token.ThrowIfCancellationRequested();
                            if (objImprovement.Rating == 0 || objImprovement.Augmented == 0)
                                continue;
                            string strUniqueName = objImprovement.UniqueName;
                            if (!string.IsNullOrEmpty(strUniqueName) && strUniqueName != "enableattribute"
                                                                     && objImprovement.ImproveType
                                                                     == Improvement.ImprovementType.Attribute
                                                                     && objImprovement.ImprovedName == Abbrev)
                            {
                                // If this has a UniqueName, run through the current list of UniqueNames seen. If it is not already in the list, add it.
                                setUniqueNames.Add(strUniqueName);

                                // Add the values to the UniquePair List so we can check them later.
                                lstUniquePair.Add(new ValueTuple<string, decimal, string>(
                                                      strUniqueName,
                                                      objImprovement.Augmented * objImprovement.Rating,
                                                      await _objCharacter.GetObjectNameAsync(
                                                          objImprovement, GlobalSettings.Language, token).ConfigureAwait(false)));
                            }
                            else
                            {
                                decimal decValue = objImprovement.Augmented * objImprovement.Rating;
                                sbdModifier.Append(strSpace).Append('+').Append(strSpace)
                                           .Append(
                                               await _objCharacter.GetObjectNameAsync(objImprovement, GlobalSettings.Language, token).ConfigureAwait(false))
                                           .Append(strSpace).Append('(')
                                           .Append(decValue.ToString(GlobalSettings.CultureInfo))
                                           .Append(')');
                                decBaseValue += decValue;
                            }
                        }

                        if (setUniqueNames.Contains("precedence0"))
                        {
                            // Retrieve only the highest precedence0 value.
                            // Run through the list of UniqueNames and pick out the highest value for each one.
                            decimal decHighest = decimal.MinValue;

                            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                                          out StringBuilder sbdNewModifier))
                            {
                                foreach ((string strGroupName, decimal decValue, string strSourceName) in
                                         lstUniquePair)
                                {
                                    token.ThrowIfCancellationRequested();
                                    if (strGroupName != "precedence0" || decValue <= decHighest)
                                        continue;
                                    decHighest = decValue;
                                    sbdNewModifier.Clear();
                                    sbdNewModifier
                                        .Append(strSpace).Append('+').Append(strSpace).Append(strSourceName)
                                        .Append(strSpace).Append('(')
                                        .Append(decValue.ToString(GlobalSettings.CultureInfo)).Append(')');
                                }

                                if (setUniqueNames.Contains("precedence-1"))
                                {
                                    foreach ((string strGroupName, decimal decValue, string strSourceName) in
                                             lstUniquePair)
                                    {
                                        token.ThrowIfCancellationRequested();
                                        if (strGroupName != "precedence-1")
                                            continue;
                                        decHighest += decValue;
                                        sbdNewModifier
                                            .Append(strSpace).Append('+').Append(strSpace).Append(strSourceName)
                                            .Append(strSpace).Append('(')
                                            .Append(decValue.ToString(GlobalSettings.CultureInfo)).Append(')');
                                    }
                                }

                                if (decHighest >= decBaseValue)
                                {
                                    sbdModifier.Clear();
                                    sbdModifier.Append(sbdNewModifier);
                                }
                            }
                        }
                        else if (setUniqueNames.Contains("precedence1"))
                        {
                            // Retrieve all the items that are precedence1 and nothing else.
                            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                                          out StringBuilder sbdNewModifier))
                            {
                                foreach ((string _, decimal decValue, string strSourceName) in lstUniquePair
                                             .Where(
                                                 s => s.Item1 == "precedence1" || s.Item1 == "precedence-1"))
                                {
                                    token.ThrowIfCancellationRequested();
                                    sbdNewModifier.AppendFormat(GlobalSettings.CultureInfo,
                                                                "{0}+{0}{1}{0}({2})",
                                                                strSpace,
                                                                strSourceName, decValue);
                                }

                                sbdModifier.Clear();
                                sbdModifier.Append(sbdNewModifier);
                            }
                        }
                        else
                        {
                            // Run through the list of UniqueNames and pick out the highest value for each one.
                            foreach (string strName in setUniqueNames)
                            {
                                token.ThrowIfCancellationRequested();
                                decimal decHighest = decimal.MinValue;
                                foreach ((string strGroupName, decimal decValue, string strSourceName) in
                                         lstUniquePair)
                                {
                                    if (strGroupName == strName && decValue > decHighest)
                                    {
                                        decHighest = decValue;
                                        sbdModifier.Append(strSpace).Append('+').Append(strSpace)
                                                   .Append(strSourceName)
                                                   .Append(strSpace).Append('(')
                                                   .Append(decValue.ToString(GlobalSettings.CultureInfo))
                                                   .Append(')');
                                    }
                                }
                            }
                        }

                        // Factor in Custom Improvements.
                        setUniqueNames.Clear();
                        lstUniquePair.Clear();
                        foreach (Improvement objImprovement in lstUsedImprovements.Where(
                                     objImprovement => objImprovement.Custom))
                        {
                            token.ThrowIfCancellationRequested();
                            if (objImprovement.Rating == 0 || objImprovement.Augmented == 0)
                                continue;
                            string strUniqueName = objImprovement.UniqueName;
                            if (!string.IsNullOrEmpty(strUniqueName))
                            {
                                // If this has a UniqueName, run through the current list of UniqueNames seen. If it is not already in the list, add it.
                                setUniqueNames.Add(strUniqueName);

                                // Add the values to the UniquePair List so we can check them later.
                                lstUniquePair.Add(new ValueTuple<string, decimal, string>(
                                                      strUniqueName,
                                                      objImprovement.Augmented * objImprovement.Rating,
                                                      await _objCharacter.GetObjectNameAsync(
                                                          objImprovement, GlobalSettings.Language, token).ConfigureAwait(false)));
                            }
                            else
                            {
                                sbdModifier.Append(strSpace).Append('+').Append(strSpace)
                                           .Append(
                                               await _objCharacter.GetObjectNameAsync(objImprovement, GlobalSettings.Language, token).ConfigureAwait(false))
                                           .Append(strSpace).Append('(')
                                           .Append((objImprovement.Augmented * objImprovement.Rating).ToString(
                                                       GlobalSettings.CultureInfo)).Append(')');
                            }
                        }

                        // Run through the list of UniqueNames and pick out the highest value for each one.
                        foreach (string strName in setUniqueNames)
                        {
                            token.ThrowIfCancellationRequested();
                            decimal decHighest = decimal.MinValue;
                            foreach ((string strGroupName, decimal decValue, string strSourceName) in
                                     lstUniquePair)
                            {
                                token.ThrowIfCancellationRequested();
                                if (strGroupName != strName || decValue <= decHighest)
                                    continue;
                                decHighest = decValue;
                                sbdModifier.Append(strSpace).Append('+').Append(strSpace).Append(strSourceName)
                                           .Append(strSpace).Append('(')
                                           .Append(decValue.ToString(GlobalSettings.CultureInfo)).Append(')');
                            }
                        }

                        //// If this is AGI or STR, factor in any Cyberlimbs.
                        if (!await _objCharacterSettings.GetDontUseCyberlimbCalculationAsync(token).ConfigureAwait(false) &&
                            Cyberware.CyberlimbAttributeAbbrevs.Contains(Abbrev))
                        {
                            await _objCharacter.Cyberware.ForEachAsync(objCyberware => BuildTooltip(sbdModifier, objCyberware, strSpace), token: token).ConfigureAwait(false);
                        }

                        return _strCachedToolTip = await GetCurrentDisplayAbbrevAsync(token).ConfigureAwait(false) + strSpace + "("
                                                   + (await GetValueAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.CultureInfo) + ")" +
                                                   sbdModifier.ToString();
                    }
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            async Task BuildTooltip(StringBuilder sbdModifier, Cyberware objCyberware, string strSpace)
            {
                if (!await objCyberware.GetIsLimbAsync(token).ConfigureAwait(false) || !await objCyberware.GetIsModularCurrentlyEquippedAsync(token).ConfigureAwait(false))
                {
                    return;
                }

                if (await objCyberware.GetInheritAttributesAsync(token).ConfigureAwait(false))
                {
                    await (await objCyberware.GetChildrenAsync(token).ConfigureAwait(false)).ForEachAsync(objChild => BuildTooltip(sbdModifier, objChild, strSpace), token: token).ConfigureAwait(false);

                    return;
                }

                sbdModifier.AppendLine()
                    .Append(await objCyberware.GetCurrentDisplayNameAsync(token).ConfigureAwait(false))
                    .Append(strSpace)
                    .Append('(')
                    .Append((await objCyberware.GetAttributeTotalValueAsync(Abbrev, token).ConfigureAwait(false)).ToString(GlobalSettings.CultureInfo))
                    .Append(')');
            }
        }

        public int SpentPriorityPoints
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    int intBase = Base;
                    int intReturn = intBase;

                    decimal decExtra = 0;
                    decimal decMultiplier = 1.0m;
                    foreach (Improvement objImprovement in ImprovementManager.GetCachedImprovementListForValueOf(_objCharacter, Improvement.ImprovementType.AttributePointCost, Abbrev, true))
                    {
                        if (objImprovement.Minimum <= intBase)
                            decExtra += objImprovement.Value
                                                        * (Math.Min(
                                                               intBase,
                                                               objImprovement.Maximum == 0
                                                                   ? int.MaxValue
                                                                   : objImprovement.Maximum)
                                                           - objImprovement.Minimum - 1);
                    }
                    foreach (Improvement objImprovement in ImprovementManager.GetCachedImprovementListForValueOf(_objCharacter, Improvement.ImprovementType.AttributePointCostMultiplier, Abbrev, true))
                    {
                        if (objImprovement.Minimum <= intBase)
                            decMultiplier *= objImprovement.Value / 100.0m;
                    }

                    if (decMultiplier != 1.0m)
                        intReturn = (intReturn * decMultiplier + decExtra).StandardRound();
                    else
                        intReturn += decExtra.StandardRound();

                    return Math.Max(intReturn, 0);
                }
            }
        }

        public async Task<int> GetSpentPriorityPointsAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intBase = Base;
                int intReturn = intBase;

                decimal decExtra = 0;
                decimal decMultiplier = 1.0m;
                foreach (Improvement objImprovement in await ImprovementManager.GetCachedImprovementListForValueOfAsync(_objCharacter, Improvement.ImprovementType.AttributePointCost, Abbrev, true, token).ConfigureAwait(false))
                {
                    if (objImprovement.Minimum <= intBase)
                        decExtra += objImprovement.Value
                                                    * (Math.Min(
                                                           intBase,
                                                           objImprovement.Maximum == 0
                                                               ? int.MaxValue
                                                               : objImprovement.Maximum)
                                                       - objImprovement.Minimum - 1);
                }
                foreach (Improvement objImprovement in await ImprovementManager.GetCachedImprovementListForValueOfAsync(_objCharacter, Improvement.ImprovementType.AttributePointCostMultiplier, Abbrev, true, token).ConfigureAwait(false))
                {
                    if (objImprovement.Minimum <= intBase)
                        decMultiplier *= objImprovement.Value / 100.0m;
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

        public bool AtMetatypeMaximum
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return Value == TotalMaximum && TotalMaximum > 0;
            }
        }

        public async Task<bool> GetAtMetatypeMaximumAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intTotalMaximum = await GetTotalMaximumAsync(token).ConfigureAwait(false);
                return intTotalMaximum > 0 && await GetValueAsync(token).ConfigureAwait(false) == intTotalMaximum;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public int KarmaMaximum
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return Math.Max(TotalMaximum - TotalBase, 0);
            }
        }

        public async Task<int> GetKarmaMaximumAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return Math.Max(
                    await GetTotalMaximumAsync(token).ConfigureAwait(false) -
                    await GetTotalBaseAsync(token).ConfigureAwait(false), 0);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public int PriorityMaximum
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return Math.Max(TotalMaximum - Karma - FreeBase - RawMinimum, 0);
            }
        }

        public async Task<int> GetPriorityMaximumAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return Math.Max(
                    await GetTotalMaximumAsync(token).ConfigureAwait(false) -
                    await GetKarmaAsync(token).ConfigureAwait(false) -
                    await GetFreeBaseAsync(token).ConfigureAwait(false) -
                    await GetRawMinimumAsync(token).ConfigureAwait(false), 0);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private int _intCachedUpgradeKarmaCost = int.MinValue;

        // This houserule does not work with how MAG / RES loss due to ESS loss in Chargen is handled
        // As long as the true Metatype Min is always 1, this will work. That should be the case for RAW, but can be problematic in CustomData
        // TODO: The handling of MAG / RES loss in chargen should be changed to use maybe use ImprovementType.AttributeLevel
        private static readonly IReadOnlyCollection<string> s_SetAlternateMetatypeAttributeKarmaExceptions
            = new HashSet<string>
            {
                "MAG",
                "RES",
                "DEP",
                "MAGAdept"
            };

        /// <summary>
        /// Karma price to upgrade. Returns negative if impossible
        /// </summary>
        /// <returns>Price in karma</returns>
        public int UpgradeKarmaCost
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    // intReturn for thread safety
                    int intReturn = _intCachedUpgradeKarmaCost;
                    if (intReturn != int.MinValue)
                        return intReturn;

                    int intValue = Value;
                    if (intValue >= TotalMaximum)
                    {
                        return -1;
                    }

                    int intUpgradeCost;
                    int intOptionsCost = _objCharacterSettings.KarmaAttribute;
                    if (intValue == 0)
                    {
                        intUpgradeCost = intOptionsCost;
                    }
                    else
                    {
                        intUpgradeCost = (intValue + 1) * intOptionsCost;
                    }

                    if (_objCharacterSettings.AlternateMetatypeAttributeKarma &&
                        !s_SetAlternateMetatypeAttributeKarmaExceptions.Contains(Abbrev))
                        intUpgradeCost -= (MetatypeMinimum - 1) * intOptionsCost;

                    decimal decExtra = 0;
                    decimal decMultiplier = 1.0m;
                    foreach (Improvement objImprovement in ImprovementManager.GetCachedImprovementListForValueOf(_objCharacter, Improvement.ImprovementType.AttributeKarmaCost, Abbrev, true))
                    {
                        if ((objImprovement.Maximum == 0 || intValue + 1 <= objImprovement.Maximum) &&
                            objImprovement.Minimum <= intValue + 1)
                            decExtra += objImprovement.Value;
                    }
                    foreach (Improvement objImprovement in ImprovementManager.GetCachedImprovementListForValueOf(_objCharacter, Improvement.ImprovementType.AttributeKarmaCostMultiplier, Abbrev, true))
                    {
                        if ((objImprovement.Maximum == 0 || intValue + 1 <= objImprovement.Maximum) &&
                            objImprovement.Minimum <= intValue + 1)
                            decMultiplier *= objImprovement.Value / 100.0m;
                    }

                    if (decMultiplier != 1.0m)
                        intUpgradeCost = (intUpgradeCost * decMultiplier + decExtra).StandardRound();
                    else
                        intUpgradeCost += decExtra.StandardRound();

                    return _intCachedUpgradeKarmaCost = Math.Max(intUpgradeCost, Math.Min(1, intOptionsCost));
                }
            }
        }

        /// <summary>
        /// Karma price to upgrade. Returns negative if impossible
        /// </summary>
        /// <returns>Price in karma</returns>
        public async Task<int> GetUpgradeKarmaCostAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // intReturn for thread safety
                int intReturn = _intCachedUpgradeKarmaCost;
                if (intReturn != int.MinValue)
                    return intReturn;

                int intValue = await GetValueAsync(token).ConfigureAwait(false);
                if (intValue >= await GetTotalMaximumAsync(token).ConfigureAwait(false))
                {
                    return -1;
                }

                CharacterSettings objSettings = await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false);
                int intUpgradeCost;
                int intOptionsCost = await objSettings.GetKarmaAttributeAsync(token).ConfigureAwait(false);
                if (intValue == 0)
                {
                    intUpgradeCost = intOptionsCost;
                }
                else
                {
                    intUpgradeCost = (intValue + 1) * intOptionsCost;
                }

                if (await objSettings.GetAlternateMetatypeAttributeKarmaAsync(token).ConfigureAwait(false)
                    && !s_SetAlternateMetatypeAttributeKarmaExceptions.Contains(Abbrev))
                    intUpgradeCost -= (await GetMetatypeMinimumAsync(token).ConfigureAwait(false) - 1) *
                                      intOptionsCost;

                decimal decExtra = 0;
                decimal decMultiplier = 1.0m;
                foreach (Improvement objImprovement in await ImprovementManager.GetCachedImprovementListForValueOfAsync(_objCharacter, Improvement.ImprovementType.AttributeKarmaCost, Abbrev, true, token).ConfigureAwait(false))
                {
                    if ((objImprovement.Maximum == 0 || intValue + 1 <= objImprovement.Maximum) &&
                        objImprovement.Minimum <= intValue + 1)
                        decExtra += objImprovement.Value;
                }
                foreach (Improvement objImprovement in await ImprovementManager.GetCachedImprovementListForValueOfAsync(_objCharacter, Improvement.ImprovementType.AttributeKarmaCostMultiplier, Abbrev, true, token).ConfigureAwait(false))
                {
                    if ((objImprovement.Maximum == 0 || intValue + 1 <= objImprovement.Maximum) &&
                        objImprovement.Minimum <= intValue + 1)
                        decMultiplier *= objImprovement.Value / 100.0m;
                }
                
                if (decMultiplier != 1.0m)
                    intUpgradeCost = (intUpgradeCost * decMultiplier + decExtra).StandardRound();
                else
                    intUpgradeCost += decExtra.StandardRound();

                return _intCachedUpgradeKarmaCost = Math.Max(intUpgradeCost, Math.Min(1, intOptionsCost));
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public int TotalKarmaCost
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    if (Karma == 0)
                        return 0;

                    int intValue = Value;
                    int intRawTotalBase = _objCharacterSettings.ReverseAttributePriorityOrder
                        ? Math.Max(FreeBase + RawMinimum, TotalMinimum)
                        : TotalBase;
                    int intTotalBase = intRawTotalBase;
                    if (_objCharacterSettings.AlternateMetatypeAttributeKarma)
                    {
                        int intHumanMinimum = FreeBase + 1 + MinimumModifiers;
                        if (!_objCharacterSettings.ReverseAttributePriorityOrder)
                            intHumanMinimum += Base;
                        if (intHumanMinimum < 1)
                        {
                            if (_objCharacter.IsCritter || MetatypeMaximum == 0 || Abbrev == "EDG" || Abbrev == "MAG" ||
                                Abbrev == "MAGAdept" || Abbrev == "RES" || Abbrev == "DEP")
                                intHumanMinimum = 0;
                            else
                                intHumanMinimum = 1;
                        }

                        intTotalBase = intHumanMinimum;
                    }

                    // The expression below is a shortened version of n*(n+1)/2 when applied to karma costs. n*(n+1)/2 is the sum of all numbers from 1 to n.
                    // I'm taking n*(n+1)/2 where n = Base + Karma, then subtracting n*(n+1)/2 from it where n = Base. After removing all terms that cancel each other out, the expression below is what remains.
                    int intCost = (2 * intTotalBase + Karma + 1) * Karma / 2 * _objCharacterSettings.KarmaAttribute;

                    decimal decExtra = 0;
                    decimal decMultiplier = 1.0m;
                    foreach (Improvement objImprovement in ImprovementManager.GetCachedImprovementListForValueOf(_objCharacter, Improvement.ImprovementType.AttributeKarmaCost, Abbrev, true))
                    {
                        if (objImprovement.Minimum <= intValue)
                            decExtra += objImprovement.Value *
                                                (Math.Min(intValue,
                                                          objImprovement.Maximum == 0
                                                              ? int.MaxValue
                                                              : objImprovement.Maximum) - Math.Max(intRawTotalBase,
                                                    objImprovement.Minimum - 1));
                    }
                    foreach (Improvement objImprovement in ImprovementManager.GetCachedImprovementListForValueOf(_objCharacter, Improvement.ImprovementType.AttributeKarmaCostMultiplier, Abbrev, true))
                    {
                        if (objImprovement.Minimum <= intValue)
                            decMultiplier *= objImprovement.Value / 100.0m;
                    }

                    if (decMultiplier != 1.0m)
                        intCost = (intCost * decMultiplier + decExtra).StandardRound();
                    else
                        intCost += decExtra.StandardRound();

                    return Math.Max(intCost, 0);
                }
            }
        }

        public async Task<int> GetTotalKarmaCostAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intKarma = await GetKarmaAsync(token).ConfigureAwait(false);
                if (intKarma == 0)
                    return 0;

                int intValue = await GetValueAsync(token).ConfigureAwait(false);
                int intFreeBase = await GetFreeBaseAsync(token).ConfigureAwait(false);
                CharacterSettings objSettings = await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false);
                int intRawTotalBase = await objSettings.GetReverseAttributePriorityOrderAsync(token).ConfigureAwait(false)
                    ? Math.Max(intFreeBase + await GetRawMinimumAsync(token).ConfigureAwait(false),
                        await GetTotalMinimumAsync(token).ConfigureAwait(false))
                    : await GetTotalBaseAsync(token).ConfigureAwait(false);
                int intTotalBase = intRawTotalBase;
                if (await objSettings.GetAlternateMetatypeAttributeKarmaAsync(token).ConfigureAwait(false))
                {
                    int intHumanMinimum = intFreeBase + 1 + await GetMinimumModifiersAsync(token).ConfigureAwait(false);
                    if (!await objSettings.GetReverseAttributePriorityOrderAsync(token).ConfigureAwait(false))
                        intHumanMinimum += await GetBaseAsync(token).ConfigureAwait(false);
                    if (intHumanMinimum < 1)
                    {
                        if (await _objCharacter.GetIsCritterAsync(token).ConfigureAwait(false) ||
                            await GetMetatypeMaximumAsync(token).ConfigureAwait(false) == 0 || Abbrev == "EDG" ||
                            Abbrev == "MAG" || Abbrev == "MAGAdept" || Abbrev == "RES" || Abbrev == "DEP")
                            intHumanMinimum = 0;
                        else
                            intHumanMinimum = 1;
                    }

                    intTotalBase = intHumanMinimum;
                }

                // The expression below is a shortened version of n*(n+1)/2 when applied to karma costs. n*(n+1)/2 is the sum of all numbers from 1 to n.
                // I'm taking n*(n+1)/2 where n = Base + Karma, then subtracting n*(n+1)/2 from it where n = Base. After removing all terms that cancel each other out, the expression below is what remains.
                int intCost = (2 * intTotalBase + intKarma + 1) * intKarma / 2 * await objSettings.GetKarmaAttributeAsync(token).ConfigureAwait(false);

                decimal decExtra = 0;
                decimal decMultiplier = 1.0m;
                foreach (Improvement objImprovement in await ImprovementManager.GetCachedImprovementListForValueOfAsync(_objCharacter, Improvement.ImprovementType.AttributePointCost, Abbrev, true, token).ConfigureAwait(false))
                {
                    if (objImprovement.Minimum <= intValue)
                        decExtra += objImprovement.Value *
                                            (Math.Min(intValue,
                                                      objImprovement.Maximum == 0
                                                          ? int.MaxValue
                                                          : objImprovement.Maximum) - Math.Max(intRawTotalBase,
                                                objImprovement.Minimum - 1));
                }
                foreach (Improvement objImprovement in await ImprovementManager.GetCachedImprovementListForValueOfAsync(_objCharacter, Improvement.ImprovementType.AttributePointCostMultiplier, Abbrev, true, token).ConfigureAwait(false))
                {
                    if (objImprovement.Minimum <= intValue)
                        decMultiplier *= objImprovement.Value / 100.0m;
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

        // Caching the value prevents calling the event multiple times.
        private int _intCachedCanUpgradeCareer = int.MinValue;

        public bool CanUpgradeCareer
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    // intReturn for thread safety
                    int intReturn = _intCachedCanUpgradeCareer;
                    if (intReturn < 0)
                    {
                        intReturn =
                            (_objCharacter.Karma >= UpgradeKarmaCost && TotalMaximum > Value).ToInt32();
                        _intCachedCanUpgradeCareer = intReturn;
                    }

                    return intReturn > 0;
                }
            }
        }

        public async Task<bool> GetCanUpgradeCareerAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // intReturn for thread safety
                int intReturn = _intCachedCanUpgradeCareer;
                if (intReturn < 0)
                {
                    intReturn =
                        (await _objCharacter.GetKarmaAsync(token).ConfigureAwait(false)
                         >= await GetUpgradeKarmaCostAsync(token).ConfigureAwait(false)
                         && await GetTotalMaximumAsync(token).ConfigureAwait(false) >
                         await GetValueAsync(token).ConfigureAwait(false)).ToInt32();
                    _intCachedCanUpgradeCareer = intReturn;
                }

                return intReturn > 0;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async Task OnCharacterChanged(object sender, MultiplePropertiesChangedEventArgs e,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool, out HashSet<string> setProperties))
            {
                if (e.PropertyNames.Contains(nameof(Character.Karma)))
                    setProperties.Add(nameof(CanUpgradeCareer));
                if (e.PropertyNames.Contains(nameof(Character.EffectiveBuildMethodUsesPriorityTables)))
                    setProperties.Add(nameof(BaseUnlocked));
                if (e.PropertyNames.Contains(nameof(Character.LimbCount)))
                {
                    CharacterSettings objSettings = await CharacterObject.GetSettingsAsync(token).ConfigureAwait(false);
                    if (!await objSettings.GetDontUseCyberlimbCalculationAsync(token).ConfigureAwait(false)
                        && Cyberware.CyberlimbAttributeAbbrevs.Contains(Abbrev)
                        && await (await CharacterObject.GetCyberwareAsync(token).ConfigureAwait(false)).AnyAsync(
                                async objCyberware => await objCyberware.GetIsLimbAsync(token).ConfigureAwait(false) &&
                                                      await objCyberware.GetIsModularCurrentlyEquippedAsync(token)
                                                          .ConfigureAwait(false) &&
                                                      !(await objSettings.GetExcludeLimbSlotAsync(token).ConfigureAwait(false)).Contains(
                                                          await objCyberware
                                                              .GetLimbSlotAsync(token).ConfigureAwait(false)), token: token)
                            .ConfigureAwait(false))
                    {
                        setProperties.Add(nameof(TotalValue));
                    }
                }
                if (e.PropertyNames.Contains(nameof(Character.Settings)))
                {
                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        CharacterSettings objNewSettings = await CharacterObject.GetSettingsAsync(token).ConfigureAwait(false);
                        CharacterSettings objOldSettings = Interlocked.Exchange(ref _objCharacterSettings, objNewSettings);
                        if (!ReferenceEquals(objNewSettings, objOldSettings))
                        {
                            if (objOldSettings?.IsDisposed == false)
                                objOldSettings.MultiplePropertiesChangedAsync -= OnCharacterSettingsPropertyChanged;
                            if (objNewSettings?.IsDisposed == false)
                            {
                                objNewSettings.MultiplePropertiesChangedAsync += OnCharacterSettingsPropertyChanged;
                                if (!await objNewSettings.HasIdenticalSettingsAsync(objOldSettings, token).ConfigureAwait(false))
                                {
                                    MultiplePropertiesChangedEventArgs e2 = new MultiplePropertiesChangedEventArgs(await objNewSettings.GetDifferingPropertyNamesAsync(objOldSettings, token).ConfigureAwait(false));
                                    await OnCharacterSettingsPropertyChanged(this, e2, token).ConfigureAwait(false);
                                }
                            }
                            else
                            {
                                MultiplePropertiesChangedEventArgs e2 = new MultiplePropertiesChangedEventArgs(await objOldSettings.GetDifferingPropertyNamesAsync(objNewSettings, token).ConfigureAwait(false));
                                await OnCharacterSettingsPropertyChanged(this, e2, token).ConfigureAwait(false);
                            }
                        }
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }
                }

                if (setProperties.Count > 0)
                    await OnMultiplePropertiesChangedAsync(setProperties, token).ConfigureAwait(false);
            }
        }

        private async Task OnCharacterSettingsPropertyChanged(object sender, MultiplePropertiesChangedEventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool, out HashSet<string> setProperties))
            {
                if (e.PropertyNames.Contains(nameof(CharacterSettings.DontUseCyberlimbCalculation)) && Cyberware.CyberlimbAttributeAbbrevs.Contains(Abbrev))
                {
                    CharacterSettings objSettings = await CharacterObject.GetSettingsAsync(token).ConfigureAwait(false);
                    if (await (await CharacterObject.GetCyberwareAsync(token).ConfigureAwait(false)).AnyAsync(
                            async objCyberware => await objCyberware.GetIsLimbAsync(token).ConfigureAwait(false) &&
                                                  await objCyberware.GetIsModularCurrentlyEquippedAsync(token)
                                                      .ConfigureAwait(false) &&
                                                  !(await objSettings.GetExcludeLimbSlotAsync(token).ConfigureAwait(false)).Contains(
                                                      await objCyberware
                                                          .GetLimbSlotAsync(token).ConfigureAwait(false)), token: token)
                        .ConfigureAwait(false))
                    {
                        setProperties.Add(nameof(TotalValue));
                        setProperties.Add(nameof(HasModifiers));
                    }
                }

                if ((e.PropertyNames.Contains(nameof(CharacterSettings.CyberlimbAttributeBonusCap))
                     || e.PropertyNames.Contains(nameof(CharacterSettings.ExcludeLimbSlot))) &&
                    !setProperties.Contains(nameof(TotalValue)) && Cyberware.CyberlimbAttributeAbbrevs.Contains(Abbrev))
                {
                    CharacterSettings objSettings = await CharacterObject.GetSettingsAsync(token).ConfigureAwait(false);
                    if (await (await CharacterObject.GetCyberwareAsync(token).ConfigureAwait(false)).AnyAsync(
                            async objCyberware => await objCyberware.GetIsLimbAsync(token).ConfigureAwait(false) &&
                                                  await objCyberware.GetIsModularCurrentlyEquippedAsync(token)
                                                      .ConfigureAwait(false) &&
                                                  !(await objSettings.GetExcludeLimbSlotAsync(token).ConfigureAwait(false)).Contains(
                                                      await objCyberware
                                                          .GetLimbSlotAsync(token).ConfigureAwait(false)), token: token)
                        .ConfigureAwait(false))
                    {
                        setProperties.Add(nameof(TotalValue));
                    }
                }

                if (e.PropertyNames.Contains(nameof(CharacterSettings.UnclampAttributeMinimum)))
                {
                    setProperties.Add(nameof(RawMinimum));
                }

                if (e.PropertyNames.Contains(nameof(CharacterSettings.KarmaAttribute))
                    || e.PropertyNames.Contains(nameof(CharacterSettings.AlternateMetatypeAttributeKarma)))
                {
                    setProperties.Add(nameof(UpgradeKarmaCost));
                    setProperties.Add(nameof(TotalKarmaCost));
                }
                else if (e.PropertyNames.Contains(nameof(CharacterSettings.ReverseAttributePriorityOrder)))
                {
                    setProperties.Add(nameof(TotalKarmaCost));
                }

                if (setProperties.Count > 0)
                    await OnMultiplePropertiesChangedAsync(setProperties, token).ConfigureAwait(false);
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

        private static readonly HashSet<string> s_SetPropertyNamesWithCachedValues = new HashSet<string>
        {
            nameof(CanUpgradeCareer),
            nameof(Value),
            nameof(TotalValue),
            nameof(UpgradeKarmaCost),
            nameof(ToolTip)
        };

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
                                = s_AttributeDependencyGraph.GetWithAllDependents(this, strPropertyName, true);
                        else
                        {
                            foreach (string strLoopChangedProperty in s_AttributeDependencyGraph
                                         .GetWithAllDependentsEnumerable(this, strPropertyName))
                                setNamesOfChangedProperties.Add(strLoopChangedProperty);
                        }
                    }

                    if (setNamesOfChangedProperties == null || setNamesOfChangedProperties.Count == 0)
                        return;

                    if (setNamesOfChangedProperties.Overlaps(s_SetPropertyNamesWithCachedValues))
                    {
                        using (LockObject.EnterWriteLock())
                        {
                            if (setNamesOfChangedProperties.Contains(nameof(CanUpgradeCareer)))
                                _intCachedCanUpgradeCareer = int.MinValue;
                            if (setNamesOfChangedProperties.Contains(nameof(Value)))
                                _intCachedValue = int.MinValue;
                            if (setNamesOfChangedProperties.Contains(nameof(TotalValue)))
                                _intCachedTotalValue = int.MinValue;
                            if (setNamesOfChangedProperties.Contains(nameof(UpgradeKarmaCost)))
                                _intCachedUpgradeKarmaCost = int.MinValue;
                            if (setNamesOfChangedProperties.Contains(nameof(ToolTip)))
                                _strCachedToolTip = string.Empty;
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
                                = await s_AttributeDependencyGraph.GetWithAllDependentsAsync(this, strPropertyName, true, token).ConfigureAwait(false);
                        else
                        {
                            foreach (string strLoopChangedProperty in await s_AttributeDependencyGraph
                                         .GetWithAllDependentsEnumerableAsync(this, strPropertyName, token).ConfigureAwait(false))
                                setNamesOfChangedProperties.Add(strLoopChangedProperty);
                        }
                    }

                    if (setNamesOfChangedProperties == null || setNamesOfChangedProperties.Count == 0)
                        return;

                    if (setNamesOfChangedProperties.Overlaps(s_SetPropertyNamesWithCachedValues))
                    {
                        IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                        try
                        {
                            token.ThrowIfCancellationRequested();
                            if (setNamesOfChangedProperties.Contains(nameof(CanUpgradeCareer)))
                                _intCachedCanUpgradeCareer = int.MinValue;
                            if (setNamesOfChangedProperties.Contains(nameof(Value)))
                                _intCachedValue = int.MinValue;
                            if (setNamesOfChangedProperties.Contains(nameof(TotalValue)))
                                _intCachedTotalValue = int.MinValue;
                            if (setNamesOfChangedProperties.Contains(nameof(UpgradeKarmaCost)))
                                _intCachedUpgradeKarmaCost = int.MinValue;
                            if (setNamesOfChangedProperties.Contains(nameof(ToolTip)))
                                _strCachedToolTip = string.Empty;
                        }
                        finally
                        {
                            await objLocker2.DisposeAsync().ConfigureAwait(false);
                        }
                    }

                    if (_setMultiplePropertiesChangedAsync.Count > 0)
                    {
                        MultiplePropertiesChangedEventArgs objArgs =
                            new MultiplePropertiesChangedEventArgs(setNamesOfChangedProperties.ToArray());
                        await ParallelExtensions.ForEachAsync(_setMultiplePropertiesChangedAsync, objEvent => objEvent.Invoke(this, objArgs, token), token).ConfigureAwait(false);
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
                        List<ValueTuple<PropertyChangedAsyncEventHandler, PropertyChangedEventArgs>> lstAsyncEventsList
                            = new List<ValueTuple<PropertyChangedAsyncEventHandler, PropertyChangedEventArgs>>(lstArgsList.Count * _setPropertyChangedAsync.Count);
                        foreach (PropertyChangedAsyncEventHandler objEvent in _setPropertyChangedAsync)
                        {
                            foreach (PropertyChangedEventArgs objArg in lstArgsList)
                            {
                                lstAsyncEventsList.Add(new ValueTuple<PropertyChangedAsyncEventHandler, PropertyChangedEventArgs>(objEvent, objArg));
                            }
                        }
                        await ParallelExtensions.ForEachAsync(lstAsyncEventsList, tupEvent => tupEvent.Item1.Invoke(this, tupEvent.Item2, token), token).ConfigureAwait(false);

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

        /// <summary>
        /// Convert a string to an Attribute Category.
        /// </summary>
        /// <param name="strAbbrev">Linked attribute abbreviation.</param>
        public static AttributeCategory ConvertToAttributeCategory(string strAbbrev)
        {
            switch (strAbbrev)
            {
                case "DEP":
                case "EDG":
                case "ESS":
                case "MAG":
                case "MAGAdept":
                case "RES":
                    return AttributeCategory.Special;

                default:
                    return AttributeCategory.Standard;
            }
        }

        /// <summary>
        /// Convert a string to an Attribute Category.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        public static AttributeCategory ConvertToMetatypeAttributeCategory(string strValue)
        {
            //If a value does exist, test whether it belongs to a shapeshifter form.
            switch (strValue)
            {
                case "Shapeshifter":
                    return AttributeCategory.Shapeshifter;

                default:
                    return AttributeCategory.Standard;
            }
        }

        #endregion Methods

        #region static

        //A tree of dependencies. Once some of the properties are changed,
        //anything they depend on, also needs to raise OnChanged
        //This tree keeps track of dependencies
        private static readonly PropertyDependencyGraph<CharacterAttrib> s_AttributeDependencyGraph =
            new PropertyDependencyGraph<CharacterAttrib>(
                new DependencyGraphNode<string, CharacterAttrib>(nameof(ToolTip),
                    new DependencyGraphNode<string, CharacterAttrib>(nameof(DisplayValue),
                        new DependencyGraphNode<string, CharacterAttrib>(nameof(Value),
                            new DependencyGraphNode<string, CharacterAttrib>(nameof(Karma)),
                            new DependencyGraphNode<string, CharacterAttrib>(nameof(Base)),
                            new DependencyGraphNode<string, CharacterAttrib>(nameof(FreeBase)),
                            new DependencyGraphNode<string, CharacterAttrib>(nameof(AttributeValueModifiers)),
                            new DependencyGraphNode<string, CharacterAttrib>(nameof(TotalMinimum),
                                new DependencyGraphNode<string, CharacterAttrib>(nameof(RawMinimum),
                                    new DependencyGraphNode<string, CharacterAttrib>(nameof(MetatypeMinimum),
                                        new DependencyGraphNode<string, CharacterAttrib>(nameof(RawMetatypeMinimum))
                                    ),
                                    new DependencyGraphNode<string, CharacterAttrib>(nameof(MinimumModifiers))
                                ),
                                new DependencyGraphNode<string, CharacterAttrib>(nameof(TotalMaximum))
                            ),
                            new DependencyGraphNode<string, CharacterAttrib>(nameof(TotalMaximum),
                                new DependencyGraphNode<string, CharacterAttrib>(nameof(MetatypeMaximum),
                                    new DependencyGraphNode<string, CharacterAttrib>(nameof(RawMetatypeMaximum))
                                ),
                                new DependencyGraphNode<string, CharacterAttrib>(nameof(MaximumModifiers))
                            )
                        ),
                        new DependencyGraphNode<string, CharacterAttrib>(nameof(TotalValue), x => x.HasModifiers(), (x, t) => x.HasModifiersAsync(t),
                            new DependencyGraphNode<string, CharacterAttrib>(nameof(HasModifiers))
                        ),
                        new DependencyGraphNode<string, CharacterAttrib>(nameof(HasModifiers))
                    )
                ),
                new DependencyGraphNode<string, CharacterAttrib>(nameof(TotalValue),
                    new DependencyGraphNode<string, CharacterAttrib>(nameof(CalculatedTotalValue),
                        new DependencyGraphNode<string, CharacterAttrib>(nameof(AttributeModifiers),
                            new DependencyGraphNode<string, CharacterAttrib>(nameof(MetatypeAugmentedMaximum)),
                            new DependencyGraphNode<string, CharacterAttrib>(nameof(MetatypeMaximum)),
                            new DependencyGraphNode<string, CharacterAttrib>(nameof(AugmentedMaximumModifiers)),
                            new DependencyGraphNode<string, CharacterAttrib>(nameof(TotalMaximum)),
                            new DependencyGraphNode<string, CharacterAttrib>(nameof(Value))
                        ),
                        new DependencyGraphNode<string, CharacterAttrib>(nameof(TotalAugmentedMaximum),
                            new DependencyGraphNode<string, CharacterAttrib>(nameof(AugmentedMaximumModifiers)),
                            new DependencyGraphNode<string, CharacterAttrib>(nameof(MetatypeAugmentedMaximum),
                                new DependencyGraphNode<string, CharacterAttrib>(nameof(RawMetatypeAugmentedMaximum))
                            ),
                            new DependencyGraphNode<string, CharacterAttrib>(nameof(MaximumModifiers))
                        ),
                        new DependencyGraphNode<string, CharacterAttrib>(nameof(Value),
                            new DependencyGraphNode<string, CharacterAttrib>(nameof(Karma)),
                            new DependencyGraphNode<string, CharacterAttrib>(nameof(Base)),
                            new DependencyGraphNode<string, CharacterAttrib>(nameof(FreeBase)),
                            new DependencyGraphNode<string, CharacterAttrib>(nameof(AttributeValueModifiers)),
                            new DependencyGraphNode<string, CharacterAttrib>(nameof(TotalMinimum),
                                new DependencyGraphNode<string, CharacterAttrib>(nameof(RawMinimum),
                                    new DependencyGraphNode<string, CharacterAttrib>(nameof(MetatypeMinimum)),
                                    new DependencyGraphNode<string, CharacterAttrib>(nameof(MinimumModifiers))
                                ),
                                new DependencyGraphNode<string, CharacterAttrib>(nameof(TotalMaximum))
                            ),
                            new DependencyGraphNode<string, CharacterAttrib>(nameof(TotalMaximum),
                                new DependencyGraphNode<string, CharacterAttrib>(nameof(MetatypeMaximum)),
                                new DependencyGraphNode<string, CharacterAttrib>(nameof(MaximumModifiers))
                            )
                        )
                    )
                ),
                new DependencyGraphNode<string, CharacterAttrib>(nameof(AugmentedMetatypeLimits),
                    new DependencyGraphNode<string, CharacterAttrib>(nameof(TotalMinimum)),
                    new DependencyGraphNode<string, CharacterAttrib>(nameof(TotalMaximum)),
                    new DependencyGraphNode<string, CharacterAttrib>(nameof(TotalAugmentedMaximum))
                ),
                new DependencyGraphNode<string, CharacterAttrib>(nameof(UpgradeToolTip),
                    new DependencyGraphNode<string, CharacterAttrib>(nameof(UpgradeKarmaCost),
                        new DependencyGraphNode<string, CharacterAttrib>(nameof(Value))
                    )
                ),
                new DependencyGraphNode<string, CharacterAttrib>(nameof(CanUpgradeCareer),
                    new DependencyGraphNode<string, CharacterAttrib>(nameof(UpgradeKarmaCost)),
                    new DependencyGraphNode<string, CharacterAttrib>(nameof(Value)),
                    new DependencyGraphNode<string, CharacterAttrib>(nameof(TotalMaximum))
                ),
                new DependencyGraphNode<string, CharacterAttrib>(nameof(KarmaMaximum),
                    new DependencyGraphNode<string, CharacterAttrib>(nameof(TotalMaximum)),
                    new DependencyGraphNode<string, CharacterAttrib>(nameof(TotalBase))
                ),
                new DependencyGraphNode<string, CharacterAttrib>(nameof(PriorityMaximum),
                    new DependencyGraphNode<string, CharacterAttrib>(nameof(TotalMaximum)),
                    new DependencyGraphNode<string, CharacterAttrib>(nameof(Karma)),
                    new DependencyGraphNode<string, CharacterAttrib>(nameof(FreeBase)),
                    new DependencyGraphNode<string, CharacterAttrib>(nameof(RawMinimum))
                )
            );

        /// <summary>
        /// Translated abbreviation of the attribute.
        /// </summary>
        public string CurrentDisplayAbbrev => GetDisplayAbbrev(GlobalSettings.Language);

        /// <summary>
        /// Translated abbreviation of the attribute.
        /// </summary>
        public Task<string> GetCurrentDisplayAbbrevAsync(CancellationToken token = default) => GetDisplayAbbrevAsync(GlobalSettings.Language, token);

        public string GetDisplayAbbrev(string strLanguage)
        {
            return Abbrev == "MAGAdept"
                ? LanguageManager.MAGAdeptString(strLanguage)
                : LanguageManager.GetString("String_Attribute" + Abbrev + "Short", strLanguage);
        }

        public Task<string> GetDisplayAbbrevAsync(string strLanguage, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<string>(token);
            return Abbrev == "MAGAdept"
                ? LanguageManager.MAGAdeptStringAsync(strLanguage, token: token)
                : LanguageManager.GetStringAsync("String_Attribute" + Abbrev + "Short", strLanguage, token: token);
        }

        public async Task Upgrade(int intAmount = 1, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (intAmount <= 0)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                bool blnCreated = await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false);
                for (int i = 0; i < intAmount; ++i)
                {
                    if (blnCreated)
                    {
                        if (!await GetCanUpgradeCareerAsync(token).ConfigureAwait(false))
                            return;

                        int intPrice = await GetUpgradeKarmaCostAsync(token).ConfigureAwait(false);
                        int intValue = await GetValueAsync(token).ConfigureAwait(false);

                        string strUpgradeText = string.Format(GlobalSettings.CultureInfo,
                            "{1}{0}{2}{0}{3}{0}->{0}{4}",
                            await LanguageManager.GetStringAsync(
                                "String_Space", token: token).ConfigureAwait(false),
                            await LanguageManager.GetStringAsync(
                                "String_ExpenseAttribute", token: token).ConfigureAwait(false), Abbrev,
                            intValue, intValue + 1);

                        ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
                        objExpense.Create(intPrice * -1, strUpgradeText, ExpenseType.Karma, DateTime.Now);
                        objExpense.Undo = new ExpenseUndo().CreateKarma(KarmaExpenseType.ImproveAttribute, Abbrev);

                        await _objCharacter.ExpenseEntries.AddWithSortAsync(objExpense, token: token)
                            .ConfigureAwait(false);

                        token.ThrowIfCancellationRequested();
                        await _objCharacter.ModifyKarmaAsync(-intPrice, token).ConfigureAwait(false);

                        // Undo burned Edge if possible first
                        if (Abbrev == "EDG")
                        {
                            int intBurnedEdge = -(await ImprovementManager
                                    .GetCachedImprovementListForValueOfAsync(
                                        _objCharacter, Improvement.ImprovementType.Attribute, "EDG",
                                        token: token)
                                    .ConfigureAwait(false))
                                .Sum(x => x.ImproveSource == Improvement.ImprovementSource.BurnedEdge,
                                    x => x.Minimum * x.Rating, token: token);
                            if (intBurnedEdge > 0)
                            {
                                await ImprovementManager.RemoveImprovementsAsync(
                                        _objCharacter, Improvement.ImprovementSource.BurnedEdge, token: token)
                                    .ConfigureAwait(false);
                                --intBurnedEdge;
                                if (intBurnedEdge > 0)
                                {
                                    try
                                    {
                                        await ImprovementManager.CreateImprovementAsync(_objCharacter, "EDG",
                                                Improvement.ImprovementSource.BurnedEdge,
                                                string.Empty,
                                                Improvement.ImprovementType.Attribute,
                                                string.Empty, 0, 1, -intBurnedEdge, token: token)
                                            .ConfigureAwait(false);
                                    }
                                    catch
                                    {
                                        await ImprovementManager
                                            .RollbackAsync(_objCharacter, CancellationToken.None)
                                            .ConfigureAwait(false);
                                        throw;
                                    }

                                    await ImprovementManager.CommitAsync(_objCharacter, token)
                                        .ConfigureAwait(false);
                                }

                                continue; // Skip increasing Karma
                            }
                        }
                    }

                    await ModifyKarmaAsync(1, token).ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task Degrade(int intAmount = 1, CancellationToken token = default)
        {
            if (intAmount <= 0)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                bool blnCreated = await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false);
                for (int i = intAmount; i > 0; --i)
                {
                    if (await GetKarmaAsync(token).ConfigureAwait(false) > 0)
                    {
                        await ModifyKarmaAsync(-1, token).ConfigureAwait(false);
                    }
                    else if (await GetBaseAsync(token).ConfigureAwait(false) > 0)
                    {
                        await ModifyBaseAsync(-1, token).ConfigureAwait(false);
                    }
                    else if (Abbrev == "EDG" && blnCreated &&
                             await GetTotalMinimumAsync(token).ConfigureAwait(false) > 0)
                    {
                        //Edge can reduce the metatype minimum below zero.
                        int intBurnedEdge = 1 - (await ImprovementManager
                                .GetCachedImprovementListForValueOfAsync(
                                    _objCharacter, Improvement.ImprovementType.Attribute, "EDG", token: token)
                                .ConfigureAwait(false))
                            .Sum(x => x.ImproveSource == Improvement.ImprovementSource.BurnedEdge,
                                x => x.Minimum * x.Rating, token: token);
                        token.ThrowIfCancellationRequested();
                        await ImprovementManager.RemoveImprovementsAsync(_objCharacter,
                            Improvement.ImprovementSource.BurnedEdge, token: token).ConfigureAwait(false);
                        try
                        {
                            await ImprovementManager.CreateImprovementAsync(_objCharacter, "EDG",
                                Improvement.ImprovementSource.BurnedEdge,
                                string.Empty,
                                Improvement.ImprovementType.Attribute,
                                string.Empty, 0, 1, -intBurnedEdge,
                                token: token).ConfigureAwait(false);
                        }
                        catch
                        {
                            await ImprovementManager.RollbackAsync(_objCharacter, CancellationToken.None)
                                .ConfigureAwait(false);
                            throw;
                        }

                        await ImprovementManager.CommitAsync(_objCharacter, token).ConfigureAwait(false);
                    }
                    else
                        return;
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion static

        public bool IsDisposed => _intIsDisposed > 0;

        /// <inheritdoc />
        public void Dispose()
        {
            if (IsDisposed)
                return;
            using (LockObject.EnterWriteLock())
            {
                if (Interlocked.CompareExchange(ref _intIsDisposed, 1, 0) != 0)
                    return;
                if (_objCharacter?.IsDisposed == false)
                {
                    try
                    {
                        _objCharacter.MultiplePropertiesChangedAsync -= OnCharacterChanged;
                    }
                    catch (ObjectDisposedException)
                    {
                        //swallow this
                    }
                }
                if (_objCharacterSettings?.IsDisposed == false)
                {
                    try
                    {
                        _objCharacterSettings.MultiplePropertiesChangedAsync -= OnCharacterSettingsPropertyChanged;
                    }
                    catch (ObjectDisposedException)
                    {
                        // swallow this
                    }
                }
                _objCachedTotalValueLock.Dispose();
                // to help the GC
                PropertyChanged = null;
                MultiplePropertiesChanged = null;
                _setPropertyChangedAsync.Clear();
                _setMultiplePropertiesChangedAsync.Clear();
            }
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if (IsDisposed)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                if (Interlocked.CompareExchange(ref _intIsDisposed, 1, 0) != 0)
                    return;
                if (_objCharacter?.IsDisposed == false)
                {
                    try
                    {
                        _objCharacter.MultiplePropertiesChangedAsync -= OnCharacterChanged;
                    }
                    catch (ObjectDisposedException)
                    {
                        //swallow this
                    }
                }
                if (_objCharacterSettings?.IsDisposed == false)
                {
                    try
                    {
                        _objCharacterSettings.MultiplePropertiesChangedAsync -= OnCharacterSettingsPropertyChanged;
                    }
                    catch (ObjectDisposedException)
                    {
                        // swallow this
                    }
                }

                await _objCachedTotalValueLock.DisposeAsync().ConfigureAwait(false);
                // to help the GC
                PropertyChanged = null;
                MultiplePropertiesChanged = null;
                _setPropertyChangedAsync.Clear();
                _setMultiplePropertiesChangedAsync.Clear();
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}
