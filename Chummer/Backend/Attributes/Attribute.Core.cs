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
using Chummer.Backend.Equipment;

namespace Chummer.Backend.Attributes
{
    /// <summary>
    /// Character CharacterAttribute.
    /// If using databinding, you should generally be using AttributeSection.{ATT}Binding
    /// </summary>
    [HubClassTag("Abbrev", true, "TotalValue", "TotalValue")]
    [DebuggerDisplay("{" + nameof(_strAbbrev) + "}")]
    public sealed class CharacterAttrib : INotifyMultiplePropertyChanged, IHasLockObject
    {
        private int _intMetatypeMin = 1;
        private int _intMetatypeMax = 6;
        private int _intMetatypeAugMax = 10;
        private int _intBase;
        private int _intKarma;
        private string _strAbbrev;
        private readonly Character _objCharacter;
        private AttributeCategory _eMetatypeCategory;

        public event PropertyChangedEventHandler PropertyChanged;

        public AsyncFriendlyReaderWriterLock LockObject { get; } = new AsyncFriendlyReaderWriterLock();

        #region Constructor, Save, Load, and Print Methods

        /// <summary>
        /// Character CharacterAttribute.
        /// </summary>
        /// <param name="character"></param>
        /// <param name="abbrev"></param>
        /// <param name="enumCategory"></param>
        public CharacterAttrib(Character character, string abbrev, AttributeCategory enumCategory)
        {
            _strAbbrev = abbrev;
            _eMetatypeCategory = enumCategory;
            _objCharacter = character;
            if (character != null)
            {
                using (character.LockObject.EnterWriteLock())
                    character.PropertyChanged += OnCharacterChanged;
                using (character.Settings.LockObject.EnterWriteLock())
                    character.Settings.PropertyChanged += OnCharacterSettingsPropertyChanged;
            }
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlWriter objWriter)
        {
            if (objWriter == null)
                return;
            using (EnterReadLock.Enter(LockObject))
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
        /// Load the CharacterAttribute from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            using (LockObject.EnterWriteLock())
            {
                objNode.TryGetStringFieldQuickly("name", ref _strAbbrev);
                objNode.TryGetInt32FieldQuickly("metatypemin", ref _intMetatypeMin);
                objNode.TryGetInt32FieldQuickly("metatypemax", ref _intMetatypeMax);
                objNode.TryGetInt32FieldQuickly("metatypeaugmax", ref _intMetatypeAugMax);
                objNode.TryGetInt32FieldQuickly("base", ref _intBase);
                objNode.TryGetInt32FieldQuickly("karma", ref _intKarma);
                if (!BaseUnlocked && !_objCharacter.Created)
                {
                    _intBase = 0;
                }

                //Converts old attributes to split metatype minimum and base. Saves recalculating Base - TotalMinimum all the time.
                int i = 0;
                if (objNode.TryGetInt32FieldQuickly("value", ref i))
                {
                    i -= _intMetatypeMin;
                    if (BaseUnlocked)
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
                if (Abbrev == "MAG" || Abbrev == "MAGAdept" || Abbrev == "RES" || Abbrev == "DEP" || Abbrev == "EDG")
                {
                    _eMetatypeCategory = AttributeCategory.Special;
                }
                else
                {
                    _eMetatypeCategory =
                        ConvertToMetatypeAttributeCategory(objNode["metatypecategory"]?.InnerText ?? "Standard");
                }
            }
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="objCulture">Culture in which to print.</param>
        /// <param name="strLanguageToPrint">Language in which to print.</param>
        /// <param name="token">CancellationToken to listen to.</param>
        internal async ValueTask Print(XmlWriter objWriter, CultureInfo objCulture, string strLanguageToPrint, CancellationToken token = default)
        {
            if (objWriter == null)
                return;
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                switch (Abbrev)
                {
                    case "MAGAdept":
                        if (!await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetMysAdeptSecondMAGAttributeAsync(token).ConfigureAwait(false)
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
        }

        #endregion Constructor, Save, Load, and Print Methods

        /// <summary>
        /// Type of Attribute.
        /// </summary>
        public enum AttributeCategory
        {
            Standard = 0,
            Special,
            Shapeshifter
        }

        #region Properties

        public Character CharacterObject => _objCharacter;

        public AttributeCategory MetatypeCategory
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
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
                using (EnterReadLock.Enter(LockObject))
                    return _intMetatypeMin;
            }
            // DO NOT MAKE THIS NOT PRIVATE! Instead, create improvements to adjust this because that will play nice with ReplaceAttribute improvements
            private set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    // No need to write lock because interlocked guarantees safety
                    if (Interlocked.Exchange(ref _intMetatypeMin, value) == value)
                        return;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Minimum value for the CharacterAttribute as set by the character's Metatype or overwritten attributes nodes.
        /// </summary>
        public int MetatypeMinimum
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
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
        public async ValueTask<int> GetMetatypeMinimumAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (MetatypeCategory == AttributeCategory.Shapeshifter)
                    return RawMetatypeMinimum;
                int intReturn = RawMetatypeMinimum;
                Improvement objImprovement = await _objCharacter.Improvements.LastOrDefaultAsync(
                    x => x.ImproveType == Improvement.ImprovementType.ReplaceAttribute && x.ImprovedName == Abbrev
                        && x.Enabled && x.Minimum != 0, token: token).ConfigureAwait(false);
                if (objImprovement != null)
                {
                    intReturn = objImprovement.Minimum;
                }

                return intReturn;
            }
        }

        /// <summary>
        /// Maximum value for the CharacterAttribute as set by the character's Metatype.
        /// </summary>
        public int RawMetatypeMaximum
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _intMetatypeMax;
            }
            // DO NOT MAKE THIS NOT PRIVATE! Instead, create improvements to adjust this because that will play nice with ReplaceAttribute improvements
            private set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    // No need to write lock because interlocked guarantees safety
                    if (Interlocked.Exchange(ref _intMetatypeMax, value) == value)
                        return;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Maximum value for the CharacterAttribute as set by the character's Metatype or overwritten attributes nodes.
        /// </summary>
        public int MetatypeMaximum
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (Abbrev == "EDG" && _objCharacter.IsAI)
                        return _objCharacter.DEP.TotalValue;
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
        public async ValueTask<int> GetMetatypeMaximumAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (Abbrev == "EDG" && await _objCharacter.GetIsAIAsync(token).ConfigureAwait(false))
                    return await (await _objCharacter.GetAttributeAsync("DEP", token: token).ConfigureAwait(false)).GetTotalValueAsync(token).ConfigureAwait(false);
                if (MetatypeCategory == AttributeCategory.Shapeshifter)
                    return RawMetatypeMaximum;
                int intReturn = RawMetatypeMaximum;
                Improvement objImprovement = await _objCharacter.Improvements
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
        }

        /// <summary>
        /// Maximum augmented value for the CharacterAttribute as set by the character's Metatype.
        /// </summary>
        public int RawMetatypeAugmentedMaximum
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _intMetatypeAugMax;
            }
            // DO NOT MAKE THIS NOT PRIVATE! Instead, create improvements to adjust this because that will play nice with ReplaceAttribute improvements
            private set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    // No need to write lock because interlocked guarantees safety
                    if (Interlocked.Exchange(ref _intMetatypeAugMax, value) == value)
                        return;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Maximum augmented value for the CharacterAttribute as set by the character's Metatype or overwritten attributes nodes.
        /// </summary>
        public int MetatypeAugmentedMaximum
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
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
        private async ValueTask<int> GetMetatypeAugmentedMaximumAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (MetatypeCategory == AttributeCategory.Shapeshifter)
                    return RawMetatypeAugmentedMaximum;
                int intReturn = RawMetatypeAugmentedMaximum;
                Improvement objImprovement = await _objCharacter.Improvements
                    .LastOrDefaultAsync(
                        x => x.ImproveType == Improvement.ImprovementType.ReplaceAttribute &&
                             x.ImprovedName == Abbrev && x.Enabled && x.AugmentedMaximum != 0, token: token).ConfigureAwait(false);
                if (objImprovement != null)
                {
                    intReturn = objImprovement.AugmentedMaximum;
                }

                return intReturn;
            }
        }

        /// <summary>
        /// Current base value (priority points spent) of the CharacterAttribute.
        /// </summary>
        public int Base
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _intBase;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    // No need to write lock because interlocked guarantees safety
                    if (Interlocked.Exchange(ref _intBase, value) == value)
                        return;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Current base value (priority points spent) of the CharacterAttribute.
        /// </summary>
        public async ValueTask<int> GetBaseAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
                return _intBase;
        }

        /// <summary>
        /// Current base value (priority points spent) of the CharacterAttribute.
        /// </summary>
        public async ValueTask SetBaseAsync(int value, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                // No need to write lock because interlocked guarantees safety
                if (Interlocked.Exchange(ref _intBase, value) == value)
                    return;
                OnPropertyChanged(nameof(Base));
            }
        }

        /// <summary>
        /// Current base value (priority points spent) of the CharacterAttribute.
        /// </summary>
        public async ValueTask ModifyBaseAsync(int value, CancellationToken token = default)
        {
            if (value == 0)
                return;
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                // No need to write lock because interlocked guarantees safety
                Interlocked.Add(ref _intBase, value);
                OnPropertyChanged(nameof(Base));
            }
        }

        /// <summary>
        /// Total Value of Base Points as used by internal methods
        /// </summary>
        public int TotalBase
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return Math.Max(Base + FreeBase + RawMinimum, TotalMinimum);
            }
        }

        /// <summary>
        /// Total Value of Base Points as used by internal methods
        /// </summary>
        public async ValueTask<int> GetTotalBaseAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                return Math.Max(
                    Base + await GetFreeBaseAsync(token).ConfigureAwait(false) +
                    await GetRawMinimumAsync(token).ConfigureAwait(false),
                    await GetTotalMinimumAsync(token).ConfigureAwait(false));
            }
        }

        public int FreeBase
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    return Math.Min(
                        ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.Attributelevel, false,
                            Abbrev),
                        MetatypeMaximum - MetatypeMinimum).StandardRound();
                }
            }
        }

        public async ValueTask<int> GetFreeBaseAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                return Math.Min(
                    await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.Attributelevel,
                        false,
                        Abbrev, token: token).ConfigureAwait(false),
                    await GetMetatypeMaximumAsync(token).ConfigureAwait(false) -
                    await GetMetatypeMinimumAsync(token).ConfigureAwait(false)).StandardRound();
            }
        }

        /// <summary>
        /// Current karma value of the CharacterAttribute.
        /// </summary>
        public int Karma
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _intKarma;
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
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
        public async ValueTask<int> GetKarmaAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
                return _intKarma;
        }

        /// <summary>
        /// Current karma value of the CharacterAttribute.
        /// </summary>
        public async ValueTask SetKarmaAsync(int value, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                // No need to write lock because interlocked guarantees safety
                if (Interlocked.Exchange(ref _intKarma, value) == value)
                    return;
                OnPropertyChanged(nameof(Karma));
            }
        }

        /// <summary>
        /// Current karma value of the CharacterAttribute.
        /// </summary>
        public async ValueTask ModifyKarmaAsync(int value, CancellationToken token = default)
        {
            if (value == 0)
                return;
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                // No need to write lock because interlocked guarantees safety
                Interlocked.Add(ref _intKarma, value);
                OnPropertyChanged(nameof(Karma));
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
                using (EnterReadLock.Enter(LockObject))
                {
                    if (_intCachedValue == int.MinValue)
                    {
                        using (LockObject.EnterWriteLock())
                        {
                            _intCachedValue
                                = Math.Min(
                                    Math.Max(Base + FreeBase + RawMinimum + AttributeValueModifiers, TotalMinimum)
                                    + Karma,
                                    TotalMaximum);
                        }
                    }
                    return _intCachedValue;
                }
            }
        }

        /// <summary>
        /// Current value of the CharacterAttribute before modifiers are applied.
        /// </summary>
        public async ValueTask<int> GetValueAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (_intCachedValue == int.MinValue)
                {
                    IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        _intCachedValue
                            = await Task.Run(async () => Math.Min(
                                                 Math.Max(
                                                     Base + await GetFreeBaseAsync(token).ConfigureAwait(false) + await GetRawMinimumAsync(token).ConfigureAwait(false)
                                                     + await GetAttributeValueModifiersAsync(token).ConfigureAwait(false), await GetTotalMinimumAsync(token).ConfigureAwait(false))
                                                 + Karma,
                                                 await GetTotalMaximumAsync(token).ConfigureAwait(false)), token).ConfigureAwait(false);
                    }
                    finally
                    {
                        await objLocker.DisposeAsync().ConfigureAwait(false);
                    }
                }
            }
            return _intCachedValue;
        }

        /// <summary>
        /// Total Maximum value of the CharacterAttribute before essence modifiers are applied but .
        /// </summary>
        public int MaximumNoEssenceLoss(bool blnUseEssenceAtSpecialStart = false)
        {
            using (EnterReadLock.Enter(LockObject))
            {
                // If we're looking at MAG and the character is a Cyberzombie, MAG is always 1, regardless of ESS penalties and bonuses.
                if (_objCharacter.MetatypeCategory == "Cyberzombie" && (Abbrev == "MAG" || Abbrev == "MAGAdept"))
                {
                    return 1;
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

                return intTotalMaximum;
            }
        }

        /// <summary>
        /// Total Maximum value of the CharacterAttribute before essence modifiers are applied but .
        /// </summary>
        public async ValueTask<int> MaximumNoEssenceLossAsync(bool blnUseEssenceAtSpecialStart = false, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                // If we're looking at MAG and the character is a Cyberzombie, MAG is always 1, regardless of ESS penalties and bonuses.
                if (_objCharacter.MetatypeCategory == "Cyberzombie" && (Abbrev == "MAG" || Abbrev == "MAGAdept"))
                {
                    return 1;
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
                    ? CharacterObject.EssenceAtSpecialStart.StandardRound() -
                      await CharacterObject.ESS.GetMetatypeMaximumAsync(token).ConfigureAwait(false)
                    : 0;
                int intTotalMinimum = intRawMinimum + Math.Max(intMinimumLossFromEssence, intMaxLossFromEssence);
                int intTotalMaximum = intRawMaximum + Math.Max(intMaximumLossFromEssence, intMaxLossFromEssence);

                if (intTotalMinimum < 1)
                {
                    if (_objCharacter.IsCritter || intRawMaximumBase == 0 || Abbrev == "EDG" || Abbrev == "MAG" ||
                        Abbrev == "MAGAdept" || Abbrev == "RES" || Abbrev == "DEP")
                        intTotalMinimum = 0;
                    else
                        intTotalMinimum = 1;
                }

                if (intTotalMaximum < intTotalMinimum)
                    intTotalMaximum = intTotalMinimum;

                return intTotalMaximum;
            }
        }

        /// <summary>
        /// Formatted Value of the attribute, including the sum of any modifiers in brackets.
        /// </summary>
        public string DisplayValue
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
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
        public async ValueTask<string> GetDisplayValueAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                int intValue = await GetValueAsync(token).ConfigureAwait(false);
                return await HasModifiersAsync(token).ConfigureAwait(false)
                    ? string.Format(GlobalSettings.CultureInfo, "{0}{1}({2})", intValue,
                        await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false),
                        await GetTotalValueAsync(token).ConfigureAwait(false))
                    : intValue.ToString(GlobalSettings.CultureInfo);
            }
        }

        /// <summary>
        /// The total amount of the modifiers that affect the CharacterAttribute's value without affecting Karma costs.
        /// </summary>
        public int AttributeModifiers
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
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
        public async ValueTask<int> GetAttributeModifiersAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
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
        }

        /// <summary>
        /// The total amount of the modifiers that raise the actual value of the CharacterAttribute and increase its Karma cost.
        /// </summary>
        public int AttributeValueModifiers
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return ImprovementManager
                        .AugmentedValueOf(_objCharacter, Improvement.ImprovementType.Attribute, false, Abbrev + "Base")
                        .StandardRound();
            }
        }

        /// <summary>
        /// The total amount of the modifiers that raise the actual value of the CharacterAttribute and increase its Karma cost.
        /// </summary>
        public async ValueTask<int> GetAttributeValueModifiersAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                return (await ImprovementManager.AugmentedValueOfAsync(_objCharacter,
                        Improvement.ImprovementType.Attribute, false, Abbrev + "Base", token: token)
                    .ConfigureAwait(false))
                    .StandardRound();
            }
        }

        /// <summary>
        /// Whether or not the CharacterAttribute has any modifiers from Improvements.
        /// </summary>
        public bool HasModifiers(CancellationToken token = default)
        {
            using (EnterReadLock.Enter(LockObject, token))
            {
                foreach (Improvement objImprovement in ImprovementManager
                             .GetCachedImprovementListForAugmentedValueOf(
                                 _objCharacter, Improvement.ImprovementType.Attribute, Abbrev, token: token))
                {
                    if (objImprovement.Augmented * objImprovement.Rating != 0)
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
                    if (objImprovement.Augmented * objImprovement.Rating != 0)
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
                if (!_objCharacter.Settings.DontUseCyberlimbCalculation &&
                    Cyberware.CyberlimbAttributeAbbrevs.Contains(Abbrev))
                {
                    return _objCharacter.Cyberware.Any(objCyberware =>
                        objCyberware.Category == "Cyberlimb" && !string.IsNullOrEmpty(objCyberware.LimbSlot));
                }

                return false;
            }
        }

        /// <summary>
        /// Whether or not the CharacterAttribute has any modifiers from Improvements.
        /// </summary>
        public async Task<bool> HasModifiersAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                foreach (Improvement objImprovement in await ImprovementManager
                             .GetCachedImprovementListForAugmentedValueOfAsync(
                                 _objCharacter, Improvement.ImprovementType.Attribute, Abbrev, token: token)
                             .ConfigureAwait(false))
                {
                    if (objImprovement.Augmented * objImprovement.Rating != 0)
                        return true;
                    if ((objImprovement.ImproveSource == Improvement.ImprovementSource.EssenceLoss ||
                         objImprovement.ImproveSource == Improvement.ImprovementSource.EssenceLossChargen ||
                         objImprovement.ImproveSource == Improvement.ImprovementSource.CyberadeptDaemon) &&
                        (_objCharacter.MAGEnabled && (Abbrev == "MAG" || Abbrev == "MAGAdept") ||
                         _objCharacter.RESEnabled && Abbrev == "RES" ||
                         _objCharacter.DEPEnabled && Abbrev == "DEP"))
                        return true;
                }

                foreach (Improvement objImprovement in await ImprovementManager
                             .GetCachedImprovementListForAugmentedValueOfAsync(
                                 _objCharacter, Improvement.ImprovementType.Attribute, Abbrev + "Base", token: token)
                             .ConfigureAwait(false))
                {
                    if (objImprovement.Augmented * objImprovement.Rating != 0)
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
                if (!_objCharacter.Settings.DontUseCyberlimbCalculation &&
                    Cyberware.CyberlimbAttributeAbbrevs.Contains(Abbrev))
                {
                    return await _objCharacter.Cyberware
                        .AnyAsync(
                            objCyberware => objCyberware.Category == "Cyberlimb" &&
                                            !string.IsNullOrEmpty(objCyberware.LimbSlot), token: token)
                        .ConfigureAwait(false);
                }

                return false;
            }
        }

        /// <summary>
        /// The total amount of the modifiers that affect the CharacterAttribute's Minimum value.
        /// </summary>
        public int MinimumModifiers
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
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
        public async ValueTask<int> GetMinimumModifiersAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
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
        }

        /// <summary>
        /// The total amount of the modifiers that affect the CharacterAttribute's Maximum value.
        /// </summary>
        public int MaximumModifiers
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
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
        public async ValueTask<int> GetMaximumModifiersAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
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
        }

        /// <summary>
        /// The total amount of the modifiers that affect the CharacterAttribute's Augmented Maximum value.
        /// </summary>
        public int AugmentedMaximumModifiers
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
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
        public async ValueTask<int> GetAugmentedMaximumModifiersAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
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
        private async Task<int> CalculatedTotalValueCore(bool blnSync, bool blnIncludeCyberlimbs = true, CancellationToken token = default)
        {
            // ReSharper disable once MethodHasAsyncOverload
            using (blnSync ? EnterReadLock.Enter(LockObject, token) : await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                // If we're looking at MAG and the character is a Cyberzombie, MAG is always 1, regardless of ESS penalties and bonuses.
                if (_objCharacter.MetatypeCategory == "Cyberzombie" && (Abbrev == "MAG" || Abbrev == "MAGAdept"))
                    return 1;

                int intMeat = blnSync
                    ? Value + AttributeModifiers
                    : await GetValueAsync(token).ConfigureAwait(false) +
                      await GetAttributeModifiersAsync(token).ConfigureAwait(false);
                int intReturn = intMeat;

                int intPureCyberValue = 0;
                int intLimbCount = 0;
                // If this is AGI or STR, factor in any Cyberlimbs.
                if (blnIncludeCyberlimbs && !_objCharacter.Settings.DontUseCyberlimbCalculation &&
                    Cyberware.CyberlimbAttributeAbbrevs.Contains(Abbrev))
                {
                    int intLimbTotal;
                    if (blnSync)
                        (intLimbCount, intLimbTotal) = ProcessCyberlimbs(_objCharacter.Cyberware);
                    else
                        (intLimbCount, intLimbTotal) =
                            await ProcessCyberlimbsAsync(_objCharacter.Cyberware).ConfigureAwait(false);

                    Tuple<int, int> ProcessCyberlimbs(IEnumerable<Cyberware> lstToCheck)
                    {
                        int intLimbCountReturn = 0;
                        int intLimbTotalReturn = 0;
                        foreach (Cyberware objCyberware in lstToCheck)
                        {
                            if (objCyberware.Category == "Cyberlimb")
                            {
                                if (!string.IsNullOrWhiteSpace(objCyberware.LimbSlot)
                                    && !_objCharacter.Settings.ExcludeLimbSlot
                                        .Contains(objCyberware.LimbSlot))
                                {
                                    intLimbCountReturn += objCyberware.LimbSlotCount;
                                    intLimbTotalReturn += objCyberware.GetAttributeTotalValue(Abbrev) *
                                                          objCyberware.LimbSlotCount;
                                }
                            }
                            else
                            {
                                (int intLoop1, int intLoop2) = ProcessCyberlimbs(objCyberware.Children);
                                intLimbCountReturn += intLoop1;
                                intLimbTotalReturn += intLoop2;
                            }
                        }

                        return new Tuple<int, int>(intLimbCountReturn, intLimbTotalReturn);
                    }

                    async ValueTask<Tuple<int, int>> ProcessCyberlimbsAsync(IEnumerable<Cyberware> lstToCheck)
                    {
                        int intLimbCountReturn = 0;
                        int intLimbTotalReturn = 0;
                        foreach (Cyberware objCyberware in lstToCheck)
                        {
                            if (objCyberware.Category == "Cyberlimb")
                            {
                                if (!string.IsNullOrWhiteSpace(objCyberware.LimbSlot)
                                    && !_objCharacter.Settings.ExcludeLimbSlot
                                        .Contains(objCyberware.LimbSlot))
                                {
                                    intLimbCountReturn += objCyberware.LimbSlotCount;
                                    intLimbTotalReturn +=
                                        await objCyberware.GetAttributeTotalValueAsync(Abbrev, token)
                                            .ConfigureAwait(false) * objCyberware.LimbSlotCount;
                                }
                            }
                            else
                            {
                                (int intLoop1, int intLoop2) = await ProcessCyberlimbsAsync(objCyberware.Children)
                                    .ConfigureAwait(false);
                                intLimbCountReturn += intLoop1;
                                intLimbTotalReturn += intLoop2;
                            }
                        }

                        return new Tuple<int, int>(intLimbCountReturn, intLimbTotalReturn);
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
                        // Not all of the limbs have been replaced, so we need to place the Attribute in the other "limbs" to get the average value.
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
                    if (_objCharacter.CritterEnabled ||
                        (blnSync ? MetatypeMaximum : await GetMetatypeMaximumAsync(token).ConfigureAwait(false)) == 0 ||
                        Abbrev == "EDG" || Abbrev == "RES" || Abbrev == "MAG" || Abbrev == "MAGAdept" ||
                        (_objCharacter.MetatypeCategory != "A.I." && Abbrev == "DEP"))
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
        }

        private int _intCachedTotalValue = int.MinValue;

        /// <summary>
        /// The CharacterAttribute's total value (Value + Modifiers).
        /// </summary>
        [HubTag("TotalValue")]
        public int TotalValue
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (_intCachedTotalValue == int.MinValue)
                    {
                        using (LockObject.EnterWriteLock())
                            _intCachedTotalValue = CalculatedTotalValue();
                    }
                    return _intCachedTotalValue;
                }
            }
        }

        /// <summary>
        /// The CharacterAttribute's total value (Value + Modifiers).
        /// </summary>
        public async ValueTask<int> GetTotalValueAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (_intCachedTotalValue == int.MinValue)
                {
                    IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        _intCachedTotalValue = await Task.Run(() => CalculatedTotalValueAsync(token: token), token).ConfigureAwait(false);
                    }
                    finally
                    {
                        await objLocker.DisposeAsync().ConfigureAwait(false);
                    }
                }
                return _intCachedTotalValue;
            }
        }

        /// <summary>
        /// The CharacterAttribute's combined Minimum value (Metatype Minimum + Modifiers), uncapped by its zero.
        /// </summary>
        public int RawMinimum
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return CharacterObject.Settings.UnclampAttributeMinimum
                        ? MetatypeMinimum + MinimumModifiers
                        : Math.Max(MetatypeMinimum + MinimumModifiers, 0);
            }
        }

        /// <summary>
        /// The CharacterAttribute's combined Minimum value (Metatype Minimum + Modifiers), uncapped by its zero.
        /// </summary>
        public async ValueTask<int> GetRawMinimumAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                int intReturn = await GetMetatypeMinimumAsync(token).ConfigureAwait(false) +
                                await GetMinimumModifiersAsync(token).ConfigureAwait(false);
                if (!CharacterObject.Settings.UnclampAttributeMinimum && intReturn < 0)
                    intReturn = 0;
                return intReturn;
            }
        }

        /// <summary>
        /// The CharacterAttribute's combined Minimum value (Metatype Minimum + Modifiers).
        /// </summary>
        public int TotalMinimum
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
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
        public async ValueTask<int> GetTotalMinimumAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                // If we're looking at MAG and the character is a Cyberzombie, MAG is always 1, regardless of ESS penalties and bonuses.
                if (_objCharacter.MetatypeCategory == "Cyberzombie" && (Abbrev == "MAG" || Abbrev == "MAGAdept"))
                    return 1;

                int intReturn = await GetRawMinimumAsync(token).ConfigureAwait(false);
                if (intReturn < 1)
                {
                    if (_objCharacter.IsCritter || await GetTotalMaximumAsync(token).ConfigureAwait(false) == 0 ||
                        Abbrev == "EDG" || Abbrev == "MAG" || Abbrev == "MAGAdept" || Abbrev == "RES" ||
                        Abbrev == "DEP")
                        intReturn = 0;
                    else
                        intReturn = 1;
                }

                return intReturn;
            }
        }

        /// <summary>
        /// The CharacterAttribute's combined Maximum value (Metatype Maximum + Modifiers).
        /// </summary>
        public int TotalMaximum
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
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
        public async ValueTask<int> GetTotalMaximumAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                // If we're looking at MAG and the character is a Cyberzombie, MAG is always 1, regardless of ESS penalties and bonuses.
                if (_objCharacter.MetatypeCategory == "Cyberzombie" && (Abbrev == "MAG" || Abbrev == "MAGAdept"))
                    return 1;

                return Math.Max(0,
                    await GetMetatypeMaximumAsync(token).ConfigureAwait(false) +
                    await GetMaximumModifiersAsync(token).ConfigureAwait(false));
            }
        }

        /// <summary>
        /// The CharacterAttribute's combined Augmented Maximum value (Metatype Augmented Maximum + Modifiers).
        /// </summary>
        public int TotalAugmentedMaximum
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
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
        public async ValueTask<int> GetTotalAugmentedMaximumAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                // If we're looking at MAG and the character is a Cyberzombie, MAG is always 1, regardless of ESS penalties and bonuses.
                if (_objCharacter.MetatypeCategory == "Cyberzombie" && (Abbrev == "MAG" || Abbrev == "MAGAdept"))
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
            using (EnterReadLock.Enter(LockObject))
            {
                return Abbrev == "MAGAdept"
                    ? LanguageManager.MAGAdeptString(strLanguage, true)
                    : LanguageManager.GetString("String_Attribute" + Abbrev + "Long", strLanguage);
            }
        }

        public async Task<string> DisplayNameLongAsync(string strLanguage, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                return Abbrev == "MAGAdept"
                    ? await LanguageManager.MAGAdeptStringAsync(strLanguage, true, token).ConfigureAwait(false)
                    : await LanguageManager.GetStringAsync("String_Attribute" + Abbrev + "Long", strLanguage, token: token).ConfigureAwait(false);
            }
        }

        public string DisplayNameFormatted => GetDisplayNameFormatted(GlobalSettings.Language);

        public Task<string> GetDisplayNameFormattedAsync(CancellationToken token = default) => GetDisplayNameFormattedAsync(GlobalSettings.Language, token);

        public string GetDisplayNameFormatted(string strLanguage)
        {
            using (EnterReadLock.Enter(LockObject))
            {
                string strSpace = LanguageManager.GetString("String_Space", strLanguage);
                if (Abbrev == "MAGAdept")
                    return LanguageManager.GetString("String_AttributeMAGLong", strLanguage) + strSpace + '(' +
                           LanguageManager.GetString("String_AttributeMAGShort", strLanguage) + ')'
                           + strSpace + '(' + LanguageManager.GetString("String_DescAdept", strLanguage) + ')';

                return DisplayNameLong(strLanguage) + strSpace + '(' + DisplayNameShort(strLanguage) + ')';
            }
        }

        public async Task<string> GetDisplayNameFormattedAsync(string strLanguage, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                string strSpace = await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token)
                    .ConfigureAwait(false);
                if (Abbrev == "MAGAdept")
                    return await LanguageManager.GetStringAsync("String_AttributeMAGLong", strLanguage, token: token)
                               .ConfigureAwait(false) + strSpace + '(' + await LanguageManager
                               .GetStringAsync("String_AttributeMAGShort", strLanguage, token: token)
                               .ConfigureAwait(false) + ')'
                           + strSpace + '(' + await LanguageManager
                               .GetStringAsync("String_DescAdept", strLanguage, token: token).ConfigureAwait(false) +
                           ')';

                return await DisplayNameLongAsync(strLanguage, token).ConfigureAwait(false) + strSpace + '(' +
                       await DisplayNameShortAsync(strLanguage, token).ConfigureAwait(false) + ')';
            }
        }

        /// <summary>
        /// Is it possible to place points in Base or is it prevented by their build method?
        /// </summary>
        public bool BaseUnlocked => _objCharacter.EffectiveBuildMethodUsesPriorityTables;

        /// <summary>
        /// CharacterAttribute Limits
        /// </summary>
        public string AugmentedMetatypeLimits
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return string.Format(GlobalSettings.CultureInfo, "{1}{0}/{0}{2}{0}({3})",
                        LanguageManager.GetString("String_Space"), TotalMinimum, TotalMaximum, TotalAugmentedMaximum);
            }
        }

        /// <summary>
        /// CharacterAttribute Limits
        /// </summary>
        public async ValueTask<string> GetAugmentedMetatypeLimitsAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                return string.Format(GlobalSettings.CultureInfo, "{1}{0}/{0}{2}{0}({3})",
                    await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false),
                    await GetTotalMinimumAsync(token).ConfigureAwait(false),
                    await GetTotalMaximumAsync(token).ConfigureAwait(false),
                    await GetTotalAugmentedMaximumAsync(token).ConfigureAwait(false));
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
            using (EnterReadLock.Enter(LockObject))
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
                    OnMultiplePropertyChanged(lstProperties);
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
        public async ValueTask AssignLimitsAsync(int intMin, int intMax, int intAug, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                bool blnMinChanged = _intMetatypeMin != intMin;
                bool blnMaxChanged = _intMetatypeMax != intMax;
                bool blnAugMaxChanged = _intMetatypeAugMax != intAug;
                if (!blnMinChanged && !blnMaxChanged && !blnAugMaxChanged)
                    return;
                IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
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
                    OnMultiplePropertyChanged(lstProperties);
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
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
            using (EnterReadLock.Enter(LockObject))
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
                    OnMultiplePropertyChanged(lstProperties);
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
        public async ValueTask AssignBaseKarmaLimitsAsync(int intBase, int intKarma, int intMin, int intMax, int intAug, CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                bool blnBaseChanged = _intBase != intBase;
                bool blnKarmaChanged = _intKarma != intKarma;
                bool blnMinChanged = _intMetatypeMin != intMin;
                bool blnMaxChanged = _intMetatypeMax != intMax;
                bool blnAugMaxChanged = _intMetatypeAugMax != intAug;
                if (!blnBaseChanged && !blnKarmaChanged && !blnMinChanged && !blnMaxChanged && !blnAugMaxChanged)
                    return;
                IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
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
                    OnMultiplePropertyChanged(lstProperties);
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        public string UpgradeToolTip
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return UpgradeKarmaCost < 0
                        ? LanguageManager.GetString("Tip_ImproveItemAtMaximum")
                        : string.Format(
                            GlobalSettings.CultureInfo,
                            LanguageManager.GetString("Tip_ImproveItem"),
                            Value + 1,
                            UpgradeKarmaCost);
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
                using (EnterReadLock.Enter(LockObject))
                {
                    if (!string.IsNullOrEmpty(_strCachedToolTip))
                        return _strCachedToolTip;

                    using (LockObject.EnterWriteLock())
                    {
                        if (!string.IsNullOrEmpty(_strCachedToolTip)) // Second check just in case
                            return _strCachedToolTip;
                        string strSpace = LanguageManager.GetString("String_Space");

                        using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                   out HashSet<string> setUniqueNames))
                        {
                            decimal decBaseValue = 0;

                            List<Improvement> lstUsedImprovements
                                = ImprovementManager.GetCachedImprovementListForAugmentedValueOf(
                                    _objCharacter, Improvement.ImprovementType.Attribute, Abbrev);

                            List<Tuple<string, decimal, string>> lstUniquePair =
                                new List<Tuple<string, decimal, string>>(lstUsedImprovements.Count);

                            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                       out StringBuilder sbdModifier))
                            {
                                foreach (Improvement objImprovement in lstUsedImprovements.Where(
                                             objImprovement => !objImprovement.Custom))
                                {
                                    string strUniqueName = objImprovement.UniqueName;
                                    if (!string.IsNullOrEmpty(strUniqueName) && strUniqueName != "enableattribute"
                                                                             && objImprovement.ImproveType
                                                                             == Improvement.ImprovementType.Attribute
                                                                             && objImprovement.ImprovedName == Abbrev)
                                    {
                                        // If this has a UniqueName, run through the current list of UniqueNames seen. If it is not already in the list, add it.
                                        if (!setUniqueNames.Contains(strUniqueName))
                                            setUniqueNames.Add(strUniqueName);

                                        // Add the values to the UniquePair List so we can check them later.
                                        lstUniquePair.Add(new Tuple<string, decimal, string>(
                                            strUniqueName, objImprovement.Augmented * objImprovement.Rating,
                                            _objCharacter.GetObjectName(
                                                objImprovement, GlobalSettings.Language)));
                                    }
                                    else if (!(objImprovement.Value == 0 && objImprovement.Augmented == 0))
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

                                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
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
                                    // Retrieve all of the items that are precedence1 and nothing else.
                                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
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
                                                    .Append(decValue.ToString(GlobalSettings.CultureInfo)).Append(')');
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
                                    string strUniqueName = objImprovement.UniqueName;
                                    if (!string.IsNullOrEmpty(strUniqueName))
                                    {
                                        // If this has a UniqueName, run through the current list of UniqueNames seen. If it is not already in the list, add it.
                                        if (!setUniqueNames.Contains(strUniqueName))
                                            setUniqueNames.Add(strUniqueName);

                                        // Add the values to the UniquePair List so we can check them later.
                                        lstUniquePair.Add(new Tuple<string, decimal, string>(
                                            strUniqueName, objImprovement.Augmented * objImprovement.Rating,
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
                                if (!_objCharacter.Settings.DontUseCyberlimbCalculation &&
                                    Cyberware.CyberlimbAttributeAbbrevs.Contains(Abbrev))
                                {
                                    foreach (Cyberware objCyberware in _objCharacter.Cyberware)
                                    {
                                        if (objCyberware.Category == "Cyberlimb")
                                        {
                                            sbdModifier.AppendLine().Append(objCyberware.CurrentDisplayName)
                                                .Append(strSpace)
                                                .Append('(')
                                                .Append(objCyberware.GetAttributeTotalValue(Abbrev)
                                                    .ToString(GlobalSettings.CultureInfo)).Append(')');
                                        }
                                    }
                                }

                                return _strCachedToolTip = DisplayAbbrev + strSpace + '('
                                                           + Value.ToString(GlobalSettings.CultureInfo) + ')' +
                                                           sbdModifier;
                            }
                        }
                    }
                }
            }
        }

        public int SpentPriorityPoints
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    int intBase = Base;
                    int intReturn = intBase;

                    decimal decExtra = 0;
                    decimal decMultiplier = 1.0m;
                    foreach (Improvement objLoopImprovement in _objCharacter.Improvements)
                    {
                        if ((objLoopImprovement.ImprovedName == Abbrev
                             || string.IsNullOrEmpty(objLoopImprovement.ImprovedName)) &&
                            (string.IsNullOrEmpty(objLoopImprovement.Condition)
                             || (objLoopImprovement.Condition == "career") == _objCharacter.Created
                             || (objLoopImprovement.Condition == "create") != _objCharacter.Created) &&
                            objLoopImprovement.Minimum <= intBase && objLoopImprovement.Enabled)
                        {
                            switch (objLoopImprovement.ImproveType)
                            {
                                case Improvement.ImprovementType.AttributePointCost:
                                    decExtra += objLoopImprovement.Value
                                                * (Math.Min(
                                                    intBase,
                                                    objLoopImprovement.Maximum == 0
                                                        ? int.MaxValue
                                                        : objLoopImprovement.Maximum) - objLoopImprovement.Minimum);
                                    break;

                                case Improvement.ImprovementType.AttributePointCostMultiplier:
                                    decMultiplier *= objLoopImprovement.Value / 100.0m;
                                    break;
                            }
                        }
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
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                int intBase = Base;
                int intReturn = intBase;

                decimal decExtra = 0;
                decimal decMultiplier = 1.0m;
                bool blnCreated = await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false);
                await _objCharacter.Improvements.ForEachAsync(objLoopImprovement =>
                {
                    if ((objLoopImprovement.ImprovedName == Abbrev
                         || string.IsNullOrEmpty(objLoopImprovement.ImprovedName)) &&
                        (string.IsNullOrEmpty(objLoopImprovement.Condition)
                         || (objLoopImprovement.Condition == "career") == blnCreated
                         || (objLoopImprovement.Condition == "create") != blnCreated) &&
                        objLoopImprovement.Minimum <= intBase && objLoopImprovement.Enabled)
                    {
                        switch (objLoopImprovement.ImproveType)
                        {
                            case Improvement.ImprovementType.AttributePointCost:
                                decExtra += objLoopImprovement.Value
                                            * (Math.Min(
                                                intBase,
                                                objLoopImprovement.Maximum == 0
                                                    ? int.MaxValue
                                                    : objLoopImprovement.Maximum) - objLoopImprovement.Minimum);
                                break;

                            case Improvement.ImprovementType.AttributePointCostMultiplier:
                                decMultiplier *= objLoopImprovement.Value / 100.0m;
                                break;
                        }
                    }
                }, token: token).ConfigureAwait(false);

                if (decMultiplier != 1.0m)
                    intReturn = (intReturn * decMultiplier + decExtra).StandardRound();
                else
                    intReturn += decExtra.StandardRound();

                return Math.Max(intReturn, 0);
            }
        }

        public bool AtMetatypeMaximum
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return Value == TotalMaximum && TotalMaximum > 0;
            }
        }

        public async ValueTask<bool> GetAtMetatypeMaximumAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                int intTotalMaximum = await GetTotalMaximumAsync(token).ConfigureAwait(false);
                return intTotalMaximum > 0 && await GetValueAsync(token).ConfigureAwait(false) == intTotalMaximum;
            }
        }

        public int KarmaMaximum
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return Math.Max(TotalMaximum - TotalBase, 0);
            }
        }

        public async ValueTask<int> GetKarmaMaximumAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                return Math.Max(
                    await GetTotalMaximumAsync(token).ConfigureAwait(false) -
                    await GetTotalBaseAsync(token).ConfigureAwait(false), 0);
            }
        }

        public int PriorityMaximum
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return Math.Max(TotalMaximum - Karma - FreeBase - RawMinimum, 0);
            }
        }

        public async ValueTask<int> GetPriorityMaximumAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                return Math.Max(
                    await GetTotalMaximumAsync(token).ConfigureAwait(false) - Karma -
                    await GetFreeBaseAsync(token).ConfigureAwait(false) -
                    await GetRawMinimumAsync(token).ConfigureAwait(false), 0);
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
                using (EnterReadLock.Enter(LockObject))
                {
                    if (_intCachedUpgradeKarmaCost != int.MinValue)
                        return _intCachedUpgradeKarmaCost;

                    using (LockObject.EnterWriteLock())
                    {
                        if (_intCachedUpgradeKarmaCost != int.MinValue)
                            return _intCachedUpgradeKarmaCost;
                        int intValue = Value;
                        if (intValue >= TotalMaximum)
                        {
                            return -1;
                        }

                        int intUpgradeCost;
                        int intOptionsCost = _objCharacter.Settings.KarmaAttribute;
                        if (intValue == 0)
                        {
                            intUpgradeCost = intOptionsCost;
                        }
                        else
                        {
                            intUpgradeCost = (intValue + 1) * intOptionsCost;
                        }

                        if (_objCharacter.Settings.AlternateMetatypeAttributeKarma &&
                            !s_SetAlternateMetatypeAttributeKarmaExceptions.Contains(Abbrev))
                            intUpgradeCost -= (MetatypeMinimum - 1) * intOptionsCost;

                        decimal decExtra = 0;
                        decimal decMultiplier = 1.0m;
                        foreach (Improvement objLoopImprovement in _objCharacter.Improvements)
                        {
                            if ((objLoopImprovement.ImprovedName == Abbrev ||
                                 string.IsNullOrEmpty(objLoopImprovement.ImprovedName)) &&
                                (string.IsNullOrEmpty(objLoopImprovement.Condition) ||
                                 (objLoopImprovement.Condition == "career") == _objCharacter.Created ||
                                 (objLoopImprovement.Condition == "create") != _objCharacter.Created) &&
                                (objLoopImprovement.Maximum == 0 || intValue + 1 <= objLoopImprovement.Maximum) &&
                                objLoopImprovement.Minimum <= intValue + 1 && objLoopImprovement.Enabled)
                            {
                                switch (objLoopImprovement.ImproveType)
                                {
                                    case Improvement.ImprovementType.AttributeKarmaCost:
                                        decExtra += objLoopImprovement.Value;
                                        break;

                                    case Improvement.ImprovementType.AttributeKarmaCostMultiplier:
                                        decMultiplier *= objLoopImprovement.Value / 100.0m;
                                        break;
                                }
                            }
                        }

                        if (decMultiplier != 1.0m)
                            intUpgradeCost = (intUpgradeCost * decMultiplier + decExtra).StandardRound();
                        else
                            intUpgradeCost += decExtra.StandardRound();

                        return _intCachedUpgradeKarmaCost = Math.Max(intUpgradeCost, Math.Min(1, intOptionsCost));
                    }
                }
            }
        }

        /// <summary>
        /// Karma price to upgrade. Returns negative if impossible
        /// </summary>
        /// <returns>Price in karma</returns>
        public async ValueTask<int> GetUpgradeKarmaCostAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (_intCachedUpgradeKarmaCost != int.MinValue)
                    return _intCachedUpgradeKarmaCost;

                IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    if (_intCachedUpgradeKarmaCost != int.MinValue)
                        return _intCachedUpgradeKarmaCost;

                    int intValue = await GetValueAsync(token).ConfigureAwait(false);
                    if (intValue >= await GetTotalMaximumAsync(token).ConfigureAwait(false))
                    {
                        return -1;
                    }

                    int intUpgradeCost;
                    int intOptionsCost = _objCharacter.Settings.KarmaAttribute;
                    if (intValue == 0)
                    {
                        intUpgradeCost = intOptionsCost;
                    }
                    else
                    {
                        intUpgradeCost = (intValue + 1) * intOptionsCost;
                    }

                    if (_objCharacter.Settings.AlternateMetatypeAttributeKarma
                        && !s_SetAlternateMetatypeAttributeKarmaExceptions.Contains(Abbrev))
                        intUpgradeCost -= (await GetMetatypeMinimumAsync(token).ConfigureAwait(false) - 1) *
                                          intOptionsCost;

                    decimal decExtra = 0;
                    decimal decMultiplier = 1.0m;
                    bool blnCreated = await CharacterObject.GetCreatedAsync(token).ConfigureAwait(false);
                    await (await _objCharacter.GetImprovementsAsync(token).ConfigureAwait(false)).ForEachAsync(
                        objLoopImprovement =>
                        {
                            if ((objLoopImprovement.ImprovedName == Abbrev ||
                                 string.IsNullOrEmpty(objLoopImprovement.ImprovedName))
                                &&
                                (string.IsNullOrEmpty(objLoopImprovement.Condition)
                                 || (objLoopImprovement.Condition == "career") == blnCreated
                                 || (objLoopImprovement.Condition == "create") != blnCreated) &&
                                (objLoopImprovement.Maximum == 0 || intValue + 1 <= objLoopImprovement.Maximum)
                                && objLoopImprovement.Minimum <= intValue + 1 && objLoopImprovement.Enabled)
                            {
                                switch (objLoopImprovement.ImproveType)
                                {
                                    case Improvement.ImprovementType.AttributeKarmaCost:
                                        decExtra += objLoopImprovement.Value;
                                        break;

                                    case Improvement.ImprovementType.AttributeKarmaCostMultiplier:
                                        decMultiplier *= objLoopImprovement.Value / 100.0m;
                                        break;
                                }
                            }
                        }, token: token).ConfigureAwait(false);

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
        }

        public int TotalKarmaCost
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (Karma == 0)
                        return 0;

                    int intValue = Value;
                    int intRawTotalBase = _objCharacter.Settings.ReverseAttributePriorityOrder
                        ? Math.Max(FreeBase + RawMinimum, TotalMinimum)
                        : TotalBase;
                    int intTotalBase = intRawTotalBase;
                    if (_objCharacter.Settings.AlternateMetatypeAttributeKarma)
                    {
                        int intHumanMinimum = _objCharacter.Settings.ReverseAttributePriorityOrder
                            ? FreeBase + 1 + MinimumModifiers
                            : Base + FreeBase + 1 + MinimumModifiers;
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
                    int intCost = (2 * intTotalBase + Karma + 1) * Karma / 2 * _objCharacter.Settings.KarmaAttribute;

                    decimal decExtra = 0;
                    decimal decMultiplier = 1.0m;
                    foreach (Improvement objLoopImprovement in _objCharacter.Improvements)
                    {
                        if ((objLoopImprovement.ImprovedName == Abbrev ||
                             string.IsNullOrEmpty(objLoopImprovement.ImprovedName)) &&
                            (string.IsNullOrEmpty(objLoopImprovement.Condition) ||
                             (objLoopImprovement.Condition == "career") == _objCharacter.Created ||
                             (objLoopImprovement.Condition == "create") != _objCharacter.Created) &&
                            objLoopImprovement.Minimum <= intValue && objLoopImprovement.Enabled)
                        {
                            switch (objLoopImprovement.ImproveType)
                            {
                                case Improvement.ImprovementType.AttributeKarmaCost:
                                    decExtra += objLoopImprovement.Value *
                                                (Math.Min(intValue,
                                                    objLoopImprovement.Maximum == 0
                                                        ? int.MaxValue
                                                        : objLoopImprovement.Maximum) - Math.Max(intRawTotalBase,
                                                    objLoopImprovement.Minimum - 1));
                                    break;

                                case Improvement.ImprovementType.AttributeKarmaCostMultiplier:
                                    decMultiplier *= objLoopImprovement.Value / 100.0m;
                                    break;
                            }
                        }
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
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (Karma == 0)
                    return 0;

                int intValue = await GetValueAsync(token).ConfigureAwait(false);
                int intFreeBase = await GetFreeBaseAsync(token).ConfigureAwait(false);
                int intRawTotalBase = _objCharacter.Settings.ReverseAttributePriorityOrder
                    ? Math.Max(intFreeBase + await GetRawMinimumAsync(token).ConfigureAwait(false),
                        await GetTotalMinimumAsync(token).ConfigureAwait(false))
                    : await GetTotalBaseAsync(token).ConfigureAwait(false);
                int intTotalBase = intRawTotalBase;
                if (_objCharacter.Settings.AlternateMetatypeAttributeKarma)
                {
                    int intHumanMinimum = intFreeBase + 1 + await GetMinimumModifiersAsync(token).ConfigureAwait(false);
                    if (!_objCharacter.Settings.ReverseAttributePriorityOrder)
                        intHumanMinimum += Base;
                    if (intHumanMinimum < 1)
                    {
                        if (_objCharacter.IsCritter ||
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
                int intCost = (2 * intTotalBase + Karma + 1) * Karma / 2 * _objCharacter.Settings.KarmaAttribute;

                decimal decExtra = 0;
                decimal decMultiplier = 1.0m;
                foreach (Improvement objLoopImprovement in _objCharacter.Improvements)
                {
                    if ((objLoopImprovement.ImprovedName == Abbrev ||
                         string.IsNullOrEmpty(objLoopImprovement.ImprovedName)) &&
                        (string.IsNullOrEmpty(objLoopImprovement.Condition) ||
                         (objLoopImprovement.Condition == "career") == _objCharacter.Created ||
                         (objLoopImprovement.Condition == "create") != _objCharacter.Created) &&
                        objLoopImprovement.Minimum <= intValue && objLoopImprovement.Enabled)
                    {
                        switch (objLoopImprovement.ImproveType)
                        {
                            case Improvement.ImprovementType.AttributeKarmaCost:
                                decExtra += objLoopImprovement.Value *
                                            (Math.Min(intValue,
                                                objLoopImprovement.Maximum == 0
                                                    ? int.MaxValue
                                                    : objLoopImprovement.Maximum) - Math.Max(intRawTotalBase,
                                                objLoopImprovement.Minimum - 1));
                                break;

                            case Improvement.ImprovementType.AttributeKarmaCostMultiplier:
                                decMultiplier *= objLoopImprovement.Value / 100.0m;
                                break;
                        }
                    }
                }

                if (decMultiplier != 1.0m)
                    intCost = (intCost * decMultiplier + decExtra).StandardRound();
                else
                    intCost += decExtra.StandardRound();

                return Math.Max(intCost, 0);
            }
        }

        // Caching the value prevents calling the event multiple times.
        private int _intCachedCanUpgradeCareer = int.MinValue;

        public bool CanUpgradeCareer
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (_intCachedCanUpgradeCareer < 0)
                    {
                        using (LockObject.EnterWriteLock())
                        {
                            if (_intCachedCanUpgradeCareer < 0) // Second check in case another task already set this
                            {
                                _intCachedCanUpgradeCareer =
                                    (_objCharacter.Karma >= UpgradeKarmaCost && TotalMaximum > Value).ToInt32();
                            }
                        }
                    }

                    return _intCachedCanUpgradeCareer > 0;
                }
            }
        }

        public async ValueTask<bool> GetCanUpgradeCareerAsync(CancellationToken token = default)
        {
            using (await EnterReadLock.EnterAsync(LockObject, token).ConfigureAwait(false))
            {
                if (_intCachedCanUpgradeCareer < 0)
                {
                    IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        if (_intCachedCanUpgradeCareer < 0) // Second check in case another task already set this
                        {
                            _intCachedCanUpgradeCareer =
                                (await _objCharacter.GetKarmaAsync(token).ConfigureAwait(false)
                                 >= await GetUpgradeKarmaCostAsync(token).ConfigureAwait(false)
                                 && await GetTotalMaximumAsync(token).ConfigureAwait(false) >
                                 await GetValueAsync(token).ConfigureAwait(false)).ToInt32();
                        }
                    }
                    finally
                    {
                        await objLocker.DisposeAsync().ConfigureAwait(false);
                    }
                }

                return _intCachedCanUpgradeCareer > 0;
            }
        }

        private void OnCharacterChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Character.Karma):
                    OnPropertyChanged(nameof(CanUpgradeCareer));
                    break;

                case nameof(Character.EffectiveBuildMethodUsesPriorityTables):
                    OnPropertyChanged(nameof(BaseUnlocked));
                    break;

                case nameof(Character.LimbCount):
                    {
                        if (!CharacterObject.Settings.DontUseCyberlimbCalculation &&
                            Cyberware.CyberlimbAttributeAbbrevs.Contains(Abbrev) &&
                            CharacterObject.Cyberware.Any(objCyberware => objCyberware.Category == "Cyberlimb"
                                                                          && !string.IsNullOrWhiteSpace(objCyberware.LimbSlot)
                                                                          && !CharacterObject.Settings.ExcludeLimbSlot.Contains(objCyberware.LimbSlot)))
                        {
                            OnPropertyChanged(nameof(TotalValue));
                        }

                        break;
                    }
            }
        }

        private void OnCharacterSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(CharacterSettings.DontUseCyberlimbCalculation):
                    {
                        if (Cyberware.CyberlimbAttributeAbbrevs.Contains(Abbrev) &&
                            CharacterObject.Cyberware.Any(objCyberware => objCyberware.Category == "Cyberlimb"
                                                                          && !string.IsNullOrWhiteSpace(objCyberware.LimbSlot)
                                                                          && !CharacterObject.Settings.ExcludeLimbSlot.Contains(objCyberware.LimbSlot)))
                        {
                            this.OnMultiplePropertyChanged(nameof(TotalValue), nameof(HasModifiers));
                        }
                        break;
                    }
                case nameof(CharacterSettings.CyberlimbAttributeBonusCap):
                case nameof(CharacterSettings.ExcludeLimbSlot):
                    {
                        if ((Abbrev == "AGI" || Abbrev == "STR") &&
                            CharacterObject.Cyberware.Any(objCyberware => objCyberware.Category == "Cyberlimb"
                                                                          && !string.IsNullOrWhiteSpace(objCyberware.LimbSlot)
                                                                          && !CharacterObject.Settings.ExcludeLimbSlot.Contains(objCyberware.LimbSlot)))
                        {
                            this.OnMultiplePropertyChanged(nameof(TotalValue));
                        }
                        break;
                    }
                case nameof(CharacterSettings.UnclampAttributeMinimum):
                    {
                        OnPropertyChanged(nameof(RawMinimum));
                        break;
                    }
                case nameof(CharacterSettings.KarmaAttribute):
                case nameof(CharacterSettings.AlternateMetatypeAttributeKarma):
                    {
                        this.OnMultiplePropertyChanged(nameof(UpgradeKarmaCost), nameof(TotalKarmaCost));
                        break;
                    }
                case nameof(CharacterSettings.ReverseAttributePriorityOrder):
                    {
                        OnPropertyChanged(nameof(TotalKarmaCost));
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
            using (EnterReadLock.Enter(LockObject))
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
                        new DependencyGraphNode<string, CharacterAttrib>(nameof(TotalValue), x => x.HasModifiers(),
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
        public string DisplayAbbrev => GetDisplayAbbrev(GlobalSettings.Language);

        public string GetDisplayAbbrev(string strLanguage)
        {
            return Abbrev == "MAGAdept"
                ? LanguageManager.MAGAdeptString(strLanguage)
                : LanguageManager.GetString("String_Attribute" + Abbrev + "Short", strLanguage);
        }

        public Task<string> GetDisplayAbbrevAsync(string strLanguage, CancellationToken token = default)
        {
            return Abbrev == "MAGAdept"
                ? LanguageManager.MAGAdeptStringAsync(strLanguage, token: token)
                : LanguageManager.GetStringAsync("String_Attribute" + Abbrev + "Short", strLanguage, token: token);
        }

        public async ValueTask Upgrade(int intAmount = 1, CancellationToken token = default)
        {
            if (intAmount <= 0)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                bool blnCreated = await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false);
                for (int i = 0; i < intAmount; ++i)
                {
                    if (blnCreated)
                    {
                        if (!await GetCanUpgradeCareerAsync(token).ConfigureAwait(false))
                            return;

                        int intPrice = await GetUpgradeKarmaCostAsync(token).ConfigureAwait(false);
                        int intValue = await GetValueAsync(token).ConfigureAwait(false);

                        string strUpgradetext = string.Format(GlobalSettings.CultureInfo, "{1}{0}{2}{0}{3}{0}->{0}{4}",
                            await LanguageManager.GetStringAsync(
                                "String_Space", token: token).ConfigureAwait(false),
                            await LanguageManager.GetStringAsync(
                                "String_ExpenseAttribute", token: token).ConfigureAwait(false), Abbrev,
                            intValue, intValue + 1);

                        ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
                        objExpense.Create(intPrice * -1, strUpgradetext, ExpenseType.Karma, DateTime.Now);
                        objExpense.Undo = new ExpenseUndo().CreateKarma(KarmaExpenseType.ImproveAttribute, Abbrev);

                        await _objCharacter.ExpenseEntries.AddWithSortAsync(objExpense, token: token)
                            .ConfigureAwait(false);

                        await _objCharacter.ModifyKarmaAsync(-intPrice, token).ConfigureAwait(false);

                        // Undo burned Edge if possible first
                        if (Abbrev == "EDG")
                        {
                            int intBurnedEdge = -(await ImprovementManager
                                    .GetCachedImprovementListForValueOfAsync(
                                        _objCharacter, Improvement.ImprovementType.Attribute, "EDG", token: token)
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
                                    await ImprovementManager.CreateImprovementAsync(_objCharacter, "EDG",
                                        Improvement.ImprovementSource.BurnedEdge,
                                        string.Empty,
                                        Improvement.ImprovementType.Attribute,
                                        string.Empty, 0, 1, -intBurnedEdge, token: token).ConfigureAwait(false);
                                    await ImprovementManager.CommitAsync(_objCharacter, token).ConfigureAwait(false);
                                }

                                continue; // Skip increasing Karma
                            }
                        }
                    }

                    ++Karma;
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async ValueTask Degrade(int intAmount, CancellationToken token = default)
        {
            if (intAmount <= 0)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                bool blnCreated = await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false);
                for (int i = intAmount; i > 0; --i)
                {
                    if (Karma > 0)
                    {
                        --Karma;
                    }
                    else if (Base > 0)
                    {
                        --Base;
                    }
                    else if (Abbrev == "EDG" && blnCreated && await GetTotalMinimumAsync(token).ConfigureAwait(false) > 0)
                    {
                        //Edge can reduce the metatype minimum below zero.
                        int intBurnedEdge = -(await ImprovementManager
                                .GetCachedImprovementListForValueOfAsync(
                                    _objCharacter, Improvement.ImprovementType.Attribute, "EDG", token: token).ConfigureAwait(false))
                            .Sum(x => x.ImproveSource == Improvement.ImprovementSource.BurnedEdge,
                                x => x.Minimum * x.Rating, token: token) + 1;
                        await ImprovementManager.RemoveImprovementsAsync(_objCharacter, Improvement.ImprovementSource.BurnedEdge, token: token).ConfigureAwait(false);
                        await ImprovementManager.CreateImprovementAsync(_objCharacter, "EDG",
                            Improvement.ImprovementSource.BurnedEdge,
                            string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1, -intBurnedEdge, token: token).ConfigureAwait(false);
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

        /// <inheritdoc />
        public void Dispose()
        {
            using (LockObject.EnterWriteLock())
            {
                if (_objCharacter != null)
                {
                    using (_objCharacter.LockObject.EnterWriteLock())
                        _objCharacter.PropertyChanged -= OnCharacterChanged;
                    using (_objCharacter.Settings.LockObject.EnterWriteLock())
                        _objCharacter.Settings.PropertyChanged -= OnCharacterSettingsPropertyChanged;
                }
            }
            LockObject.Dispose();
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                if (_objCharacter != null)
                {
                    IAsyncDisposable objLocker2 = await _objCharacter.LockObject.EnterWriteLockAsync().ConfigureAwait(false);
                    try
                    {
                        _objCharacter.PropertyChanged -= OnCharacterChanged;
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }
                    objLocker2 = await _objCharacter.Settings.LockObject.EnterWriteLockAsync().ConfigureAwait(false);
                    try
                    {
                        _objCharacter.Settings.PropertyChanged -= OnCharacterSettingsPropertyChanged;
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}
