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
    public class CharacterAttrib : INotifyMultiplePropertyChanged
    {
        private int _intMetatypeMin = 1;
        private int _intMetatypeMax = 6;
        private int _intMetatypeAugMax = 10;
        private int _intBase;
        private int _intKarma;
        private string _strAbbrev;
        private readonly Character _objCharacter;
        private AttributeCategory _enumMetatypeCategory;

        public event PropertyChangedEventHandler PropertyChanged;

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
            _enumMetatypeCategory = enumCategory;
            _objCharacter = character;
            if (_objCharacter != null)
            {
                _objCharacter.PropertyChanged += OnCharacterChanged;
                _objCharacter.Settings.PropertyChanged += OnCharacterSettingsPropertyChanged;
            }
        }

        public void UnbindAttribute()
        {
            if (_objCharacter != null)
            {
                _objCharacter.PropertyChanged -= OnCharacterChanged;
                _objCharacter.Settings.PropertyChanged -= OnCharacterSettingsPropertyChanged;
            }
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("attribute");
            objWriter.WriteElementString("name", _strAbbrev);
            objWriter.WriteElementString("metatypemin", _intMetatypeMin.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("metatypemax", _intMetatypeMax.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("metatypeaugmax", _intMetatypeAugMax.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("base", _intBase.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("karma", _intKarma.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("metatypecategory", _enumMetatypeCategory.ToString());
            // External reader friendly stuff.
            objWriter.WriteElementString("totalvalue", TotalValue.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Load the CharacterAttribute from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
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
                _enumMetatypeCategory = AttributeCategory.Special;
            }
            else
            {
                _enumMetatypeCategory =
                    ConvertToMetatypeAttributeCategory(objNode["metatypecategory"]?.InnerText ?? "Standard");
            }
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="objCulture">Culture in which to print.</param>
        /// <param name="strLanguageToPrint">Language in which to print.</param>
        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            if (objWriter == null)
                return;
            switch (Abbrev)
            {
                case "MAGAdept":
                    if (!_objCharacter.Settings.MysAdeptSecondMAGAttribute || !_objCharacter.IsMysticAdept || !_objCharacter.MAGEnabled)
                        return;
                    break;

                case "MAG":
                    if (!_objCharacter.MAGEnabled)
                        return;
                    break;

                case "RES":
                    if (!_objCharacter.RESEnabled)
                        return;
                    break;

                case "DEP":
                    if (!_objCharacter.DEPEnabled)
                        return;
                    break;
            }
            objWriter.WriteStartElement("attribute");
            objWriter.WriteElementString("name_english", Abbrev);
            objWriter.WriteElementString("name", GetDisplayAbbrev(strLanguageToPrint));
            objWriter.WriteElementString("base", Value.ToString(objCulture));
            objWriter.WriteElementString("total", TotalValue.ToString(objCulture));
            objWriter.WriteElementString("min", TotalMinimum.ToString(objCulture));
            objWriter.WriteElementString("max", TotalMaximum.ToString(objCulture));
            objWriter.WriteElementString("aug", TotalAugmentedMaximum.ToString(objCulture));
            objWriter.WriteElementString("bp", TotalKarmaCost.ToString(objCulture));
            objWriter.WriteElementString("metatypecategory", MetatypeCategory.ToString());
            objWriter.WriteEndElement();
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

        public AttributeCategory MetatypeCategory => _enumMetatypeCategory;

        /// <summary>
        /// Minimum value for the CharacterAttribute as set by the character's Metatype.
        /// </summary>
        public int MetatypeMinimum
        {
            get
            {
                int intReturn = _intMetatypeMin;
                foreach (Improvement objImprovement in ImprovementManager.GetCachedImprovementListForValueOf(
                             _objCharacter, Improvement.ImprovementType.ReplaceAttribute, Abbrev))
                    intReturn = Math.Max(intReturn, objImprovement.Minimum);
                return intReturn;
            }
            set
            {
                if (value != _intMetatypeMin)
                {
                    _intMetatypeMin = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Maximum value for the CharacterAttribute as set by the character's Metatype.
        /// </summary>
        public int MetatypeMaximum
        {
            get
            {
                if (Abbrev == "EDG" && _objCharacter.IsAI)
                    return _objCharacter.DEP.TotalValue;

                int intReturn = _intMetatypeMax;
                foreach (Improvement objImprovement in ImprovementManager.GetCachedImprovementListForValueOf(
                             _objCharacter, Improvement.ImprovementType.ReplaceAttribute, Abbrev))
                    intReturn = Math.Min(intReturn, objImprovement.Maximum);

                if (Abbrev == "ESS")
                {
                    intReturn += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.EssenceMax).StandardRound();
                }
                return intReturn;
            }
            set
            {
                if (value != _intMetatypeMax)
                {
                    _intMetatypeMax = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Maximum augmented value for the CharacterAttribute as set by the character's Metatype.
        /// </summary>
        public int MetatypeAugmentedMaximum
        {
            get
            {
                int intReturn = _intMetatypeAugMax;
                foreach (Improvement objImprovement in ImprovementManager.GetCachedImprovementListForValueOf(
                             _objCharacter, Improvement.ImprovementType.ReplaceAttribute, Abbrev))
                    intReturn = Math.Min(intReturn, objImprovement.AugmentedMaximum);
                return intReturn;
            }
            set
            {
                if (value != _intMetatypeAugMax)
                {
                    _intMetatypeAugMax = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Current base value (priority points spent) of the CharacterAttribute.
        /// </summary>
        public int Base
        {
            get => _intBase;
            set
            {
                if (value != _intBase)
                {
                    _intBase = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Total Value of Base Points as used by internal methods
        /// </summary>
        public int TotalBase => Math.Max(Base + FreeBase + RawMinimum, TotalMinimum);

        public int FreeBase => Math.Min(ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.Attributelevel, false, Abbrev), MetatypeMaximum - MetatypeMinimum).StandardRound();

        /// <summary>
        /// Current karma value of the CharacterAttribute.
        /// </summary>
        public int Karma
        {
            get => _intKarma;
            set
            {
                if (value != _intKarma)
                {
                    _intKarma = value;
                    OnPropertyChanged();
                }
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
                if (_intCachedValue == int.MinValue)
                    _intCachedValue = Math.Min(Math.Max(Base + FreeBase + RawMinimum + AttributeValueModifiers, TotalMinimum) + Karma, TotalMaximum);
                return _intCachedValue;
            }
        }

        /// <summary>
        /// Total Maximum value of the CharacterAttribute before essence modifiers are applied but .
        /// </summary>
        public int MaximumNoEssenceLoss(bool blnUseEssenceAtSpecialStart = false)
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
            List<Improvement> lstModifiers = ImprovementManager.GetCachedImprovementListForValueOf(_objCharacter, Improvement.ImprovementType.Attribute, Abbrev);
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
            lstModifiers = ImprovementManager.GetCachedImprovementListForValueOf(_objCharacter, Improvement.ImprovementType.Attribute, Abbrev + "Base");
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

            int intMaxLossFromEssence = blnUseEssenceAtSpecialStart ? CharacterObject.EssenceAtSpecialStart.StandardRound() - CharacterObject.ESS.MetatypeMaximum : 0;
            int intTotalMinimum = intRawMinimum + Math.Max(intMinimumLossFromEssence, intMaxLossFromEssence);
            int intTotalMaximum = intRawMaximum + Math.Max(intMaximumLossFromEssence, intMaxLossFromEssence);

            if (intTotalMinimum < 1)
            {
                if (_objCharacter.IsCritter || _intMetatypeMax == 0 || Abbrev == "EDG" || Abbrev == "MAG" || Abbrev == "MAGAdept" || Abbrev == "RES" || Abbrev == "DEP")
                    intTotalMinimum = 0;
                else
                    intTotalMinimum = 1;
            }
            if (intTotalMaximum < intTotalMinimum)
                intTotalMaximum = intTotalMinimum;

            return intTotalMaximum;
        }

        /// <summary>
        /// Formatted Value of the attribute, including the sum of any modifiers in brackets.
        /// </summary>
        public string DisplayValue => HasModifiers
            ? string.Format(GlobalSettings.CultureInfo, "{0}{1}({2})", Value, LanguageManager.GetString("String_Space"), TotalValue)
            : Value.ToString(GlobalSettings.CultureInfo);

        /// <summary>
        /// The total amount of the modifiers that affect the CharacterAttribute's value without affecting Karma costs.
        /// </summary>
        public int AttributeModifiers => ImprovementManager.AugmentedValueOf(_objCharacter, Improvement.ImprovementType.Attribute, false, Abbrev).StandardRound();

        /// <summary>
        /// The total amount of the modifiers that raise the actual value of the CharacterAttribute and increase its Karma cost.
        /// </summary>
        public int AttributeValueModifiers => ImprovementManager.AugmentedValueOf(_objCharacter, Improvement.ImprovementType.Attribute, false, Abbrev + "Base").StandardRound();

        /// <summary>
        /// Whether or not the CharacterAttribute has any modifiers from Improvements.
        /// </summary>
        public bool HasModifiers
        {
            get
            {
                foreach (Improvement objImprovement in ImprovementManager
                                                       .GetCachedImprovementListForValueOf(
                                                           _objCharacter, Improvement.ImprovementType.Attribute, Abbrev))
                {
                    if (objImprovement.Augmented != 0)
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
                                                       .GetCachedImprovementListForValueOf(
                                                           _objCharacter, Improvement.ImprovementType.Attribute, Abbrev + "Base"))
                {
                    if (objImprovement.Augmented != 0)
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
                if (!_objCharacter.Settings.DontUseCyberlimbCalculation && (Abbrev == "AGI" || Abbrev == "STR"))
                {
                    foreach (Cyberware objCyberware in _objCharacter.Cyberware)
                    {
                        if (objCyberware.Category == "Cyberlimb" && !string.IsNullOrEmpty(objCyberware.LimbSlot))
                            return true;
                    }
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
                int intModifier = 0;
                List<Improvement> lstModifiers = ImprovementManager.GetCachedImprovementListForValueOf(_objCharacter, Improvement.ImprovementType.Attribute, Abbrev);
                foreach (Improvement objImprovement in lstModifiers)
                {
                    intModifier += objImprovement.Minimum * objImprovement.Rating;
                }
                lstModifiers = ImprovementManager.GetCachedImprovementListForValueOf(_objCharacter, Improvement.ImprovementType.Attribute, Abbrev + "Base");
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
                int intModifier = 0;
                List<Improvement> lstModifiers = ImprovementManager.GetCachedImprovementListForValueOf(_objCharacter, Improvement.ImprovementType.Attribute, Abbrev);
                foreach (Improvement objImprovement in lstModifiers)
                {
                    intModifier += objImprovement.Maximum * objImprovement.Rating;
                }
                lstModifiers = ImprovementManager.GetCachedImprovementListForValueOf(_objCharacter, Improvement.ImprovementType.Attribute, Abbrev + "Base");
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
                int intModifier = 0;
                foreach (Improvement objImprovement in ImprovementManager.GetCachedImprovementListForValueOf(
                             _objCharacter, Improvement.ImprovementType.Attribute, Abbrev))
                {
                    intModifier += objImprovement.AugmentedMaximum * objImprovement.Rating;
                }
                return intModifier;
            }
        }

        /// <summary>
        /// The CharacterAttribute's total value (Value + Modifiers).
        /// </summary>
        public int CalculatedTotalValue(bool blnIncludeCyberlimbs = true)
        {
            // If we're looking at MAG and the character is a Cyberzombie, MAG is always 1, regardless of ESS penalties and bonuses.
            if (_objCharacter.MetatypeCategory == "Cyberzombie" && (Abbrev == "MAG" || Abbrev == "MAGAdept"))
                return 1;

            int intMeat = Value;
            //The most that any attribute can be increased by is 4, plus/minus any improvements that affect the augmented max.
            //TODO: Should probably be in AttributeModifiers property directly?
            if (ImprovementManager.GetCachedImprovementListForValueOf(_objCharacter, Improvement.ImprovementType.AttributeMaxClamp, Abbrev).Count > 0)
            {
                intMeat += Math.Min(AttributeModifiers, Math.Min(MetatypeAugmentedMaximum - MetatypeMaximum + AugmentedMaximumModifiers, TotalMaximum - intMeat));
            }
            else
            {
                intMeat += Math.Min(AttributeModifiers, MetatypeAugmentedMaximum - MetatypeMaximum + AugmentedMaximumModifiers);
            }
            int intReturn = intMeat;

            int intPureCyberValue = 0;
            int intLimbCount = 0;
            // If this is AGI or STR, factor in any Cyberlimbs.
            if ((Abbrev == "AGI" || Abbrev == "STR") && !_objCharacter.Settings.DontUseCyberlimbCalculation && blnIncludeCyberlimbs)
            {
                int intLimbTotal = 0;
                foreach (Cyberware objCyberware in _objCharacter.Cyberware.Where(objCyberware => objCyberware.Category == "Cyberlimb" && !string.IsNullOrWhiteSpace(objCyberware.LimbSlot) && !_objCharacter.Settings.ExcludeLimbSlot.Contains(objCyberware.LimbSlot)))
                {
                    intLimbCount += objCyberware.LimbSlotCount;
                    switch (Abbrev)
                    {
                        case "STR":
                            intLimbTotal += objCyberware.TotalStrength * objCyberware.LimbSlotCount;
                            break;

                        case "AGI":
                            intLimbTotal += objCyberware.TotalAgility * objCyberware.LimbSlotCount;
                            break;
                    }
                }
                //TODO: TEST THIS. There's probably some stupid combination of cybersuites that will cause a weird conflict with this and regular limbs. Something something extra limbs, idk.
                foreach (Cyberware objCyberSuite in _objCharacter.Cyberware.Where(objCyberware =>
                    objCyberware.Category == "Cybersuite"))
                {
                    foreach (Cyberware objCyberware in objCyberSuite.Children.Where(objCyberware => objCyberware.Category == "Cyberlimb" && !string.IsNullOrWhiteSpace(objCyberware.LimbSlot) && !_objCharacter.Settings.ExcludeLimbSlot.Contains(objCyberware.LimbSlot)))
                    {
                        intLimbCount += objCyberware.LimbSlotCount;
                        switch (Abbrev)
                        {
                            case "STR":
                                intLimbTotal += objCyberware.TotalStrength * objCyberware.LimbSlotCount;
                                break;

                            case "AGI":
                                intLimbTotal += objCyberware.TotalAgility * objCyberware.LimbSlotCount;
                                break;
                        }
                    }
                }

                if (intLimbCount > 0)
                {
                    int intMaxLimbs = _objCharacter.LimbCount();
                    int intMissingLimbCount = Math.Max(intMaxLimbs - intLimbCount, 0);
                    intPureCyberValue = intLimbTotal;
                    // Not all of the limbs have been replaced, so we need to place the Attribute in the other "limbs" to get the average value.
                    intLimbTotal += Math.Max(intMeat, 0) * intMissingLimbCount;
                    intReturn = (intLimbTotal + intMaxLimbs - 1) / intMaxLimbs;
                }
            }
            // Do not let the CharacterAttribute go above the Metatype's Augmented Maximum.
            if (intReturn > TotalAugmentedMaximum)
                intReturn = TotalAugmentedMaximum;

            // An Attribute cannot go below 1 unless it is EDG, MAG, or RES, the character is a Critter, the Metatype Maximum is 0, or it is caused by encumbrance (or a custom improvement).
            if (intReturn < 1)
            {
                if (_objCharacter.CritterEnabled || _intMetatypeMax == 0 || Abbrev == "EDG" || Abbrev == "RES" || Abbrev == "MAG" || Abbrev == "MAGAdept" || (_objCharacter.MetatypeCategory != "A.I." && Abbrev == "DEP"))
                    return 0;
                if (ImprovementManager.AugmentedValueOf(_objCharacter, Improvement.ImprovementType.Attribute,
                                                        out List<Improvement> lstUsedImprovements,
                                                        strImprovedName: Abbrev) < 0)
                {
                    decimal decTotalCustomImprovements = lstUsedImprovements
                                                         .Where(x => x.Custom).Sum(x => x.Augmented * x.Rating);
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
                        decimal decTotalEncumbrance = lstUsedImprovements
                                                      .Where(x => x.ImproveSource == Improvement.ImprovementSource
                                                                 .ArmorEncumbrance).Sum(x => x.Augmented * x.Rating);
                        if (decTotalEncumbrance < 0)
                            return 0;
                        break;
                    }
                    case "AGI":
                    {
                        // Special case for cyberlimbs: if every limb has been replaced with a modular connector with an attribute of 0, we allow the augmented attribute to be 0
                        if (intLimbCount > 0 && intPureCyberValue == 0)
                            return 0;
                        decimal decTotalEncumbrance = lstUsedImprovements
                                                      .Where(x => x.ImproveSource == Improvement.ImprovementSource
                                                                 .ArmorEncumbrance).Sum(x => x.Augmented * x.Rating);
                        if (decTotalEncumbrance < 0)
                            return 0;
                        break;
                    }
                }
                return 1;
            }
            return intReturn;
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
                if (_intCachedTotalValue == int.MinValue)
                    _intCachedTotalValue = CalculatedTotalValue();
                return _intCachedTotalValue;
            }
        }

        /// <summary>
        /// The CharacterAttribute's combined Minimum value (Metatype Minimum + Modifiers), uncapped by its zero.
        /// </summary>
        public int RawMinimum =>
            CharacterObject.Settings.UnclampAttributeMinimum
                ? MetatypeMinimum + MinimumModifiers
                : Math.Max(MetatypeMinimum + MinimumModifiers, 0);

        /// <summary>
        /// The CharacterAttribute's combined Minimum value (Metatype Minimum + Modifiers).
        /// </summary>
        public int TotalMinimum
        {
            get
            {
                // If we're looking at MAG and the character is a Cyberzombie, MAG is always 1, regardless of ESS penalties and bonuses.
                if (_objCharacter.MetatypeCategory == "Cyberzombie" && (Abbrev == "MAG" || Abbrev == "MAGAdept"))
                    return 1;

                int intReturn = RawMinimum;
                if (intReturn < 1)
                {
                    if (_objCharacter.IsCritter || TotalMaximum == 0 || Abbrev == "EDG" || Abbrev == "MAG" || Abbrev == "MAGAdept" || Abbrev == "RES" || Abbrev == "DEP")
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
                // If we're looking at MAG and the character is a Cyberzombie, MAG is always 1, regardless of ESS penalties and bonuses.
                if (_objCharacter.MetatypeCategory == "Cyberzombie" && (Abbrev == "MAG" || Abbrev == "MAGAdept"))
                    return 1;

                return Math.Max(0, MetatypeMaximum + MaximumModifiers);
            }
        }

        /// <summary>
        /// The CharacterAttribute's combined Augmented Maximum value (Metatype Augmented Maximum + Modifiers).
        /// </summary>
        public int TotalAugmentedMaximum
        {
            get
            {
                // If we're looking at MAG and the character is a Cyberzombie, MAG is always 1, regardless of ESS penalties and bonuses.
                if (_objCharacter.MetatypeCategory == "Cyberzombie" && (Abbrev == "MAG" || Abbrev == "MAGAdept"))
                    return 1;

                return ImprovementManager.GetCachedImprovementListForValueOf(_objCharacter, Improvement.ImprovementType.AttributeMaxClamp, Abbrev).Count > 0
                    ? TotalMaximum
                    : Math.Max(0, MetatypeAugmentedMaximum + MaximumModifiers + AugmentedMaximumModifiers);
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

        public string DisplayNameLong(string strLanguage)
        {
            return Abbrev == "MAGAdept"
                ? LanguageManager.MAGAdeptString(strLanguage, true)
                : LanguageManager.GetString("String_Attribute" + Abbrev + "Long", strLanguage);
        }

        public string DisplayNameFormatted => GetDisplayNameFormatted(GlobalSettings.Language);

        public string GetDisplayNameFormatted(string strLanguage)
        {
            string strSpace = LanguageManager.GetString("String_Space", strLanguage);
            if (Abbrev == "MAGAdept")
                return LanguageManager.GetString("String_AttributeMAGLong", strLanguage) + strSpace + '(' + LanguageManager.GetString("String_AttributeMAGShort", strLanguage) + ')'
                       + strSpace + '(' + LanguageManager.GetString("String_DescAdept", strLanguage) + ')';

            return DisplayNameLong(strLanguage) + strSpace + '(' + DisplayNameShort(strLanguage) + ')';
        }

        /// <summary>
        /// Is it possible to place points in Base or is it prevented by their build method?
        /// </summary>
        public bool BaseUnlocked => _objCharacter.EffectiveBuildMethodUsesPriorityTables;

        /// <summary>
        /// CharacterAttribute Limits
        /// </summary>
        public string AugmentedMetatypeLimits => string.Format(GlobalSettings.CultureInfo, "{1}{0}/{0}{2}{0}({3})",
            LanguageManager.GetString("String_Space"), TotalMinimum, TotalMaximum, TotalAugmentedMaximum);

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
            MetatypeMinimum = intMin;
            MetatypeMaximum = intMax;
            MetatypeAugmentedMaximum = intAug;
        }

        public string UpgradeToolTip => UpgradeKarmaCost < 0
            ? LanguageManager.GetString("Tip_ImproveItemAtMaximum")
            : string.Format(
                GlobalSettings.CultureInfo,
                LanguageManager.GetString("Tip_ImproveItem"),
                Value + 1,
                UpgradeKarmaCost);

        private string _strCachedToolTip = string.Empty;

        /// <summary>
        /// ToolTip that shows how the CharacterAttribute is calculating its Modified Rating.
        /// </summary>
        public string ToolTip
        {
            get
            {
                if (!string.IsNullOrEmpty(_strCachedToolTip))
                    return _strCachedToolTip;

                string strSpace = LanguageManager.GetString("String_Space");

                HashSet<string> lstUniqueName = new HashSet<string>();
                List<Tuple<string, decimal, string>> lstUniquePair = new List<Tuple<string, decimal, string>>();
                decimal decBaseValue = 0;

                List<Improvement> lstUsedImprovements
                    = ImprovementManager.GetCachedImprovementListForAugmentedValueOf(
                        _objCharacter, Improvement.ImprovementType.Attribute, Abbrev);

                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdModifier))
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
                            if (!lstUniqueName.Contains(strUniqueName))
                                lstUniqueName.Add(strUniqueName);

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
                                       .Append(_objCharacter.GetObjectName(objImprovement, GlobalSettings.Language))
                                       .Append(strSpace).Append('(')
                                       .Append(decValue.ToString(GlobalSettings.CultureInfo))
                                       .Append(')');
                            decBaseValue += decValue;
                        }
                    }

                    if (lstUniqueName.Contains("precedence0"))
                    {
                        // Retrieve only the highest precedence0 value.
                        // Run through the list of UniqueNames and pick out the highest value for each one.
                        decimal decHighest = decimal.MinValue;

                        using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                      out StringBuilder sbdNewModifier))
                        {
                            foreach ((string strGroupName, decimal decValue, string strSourceName) in lstUniquePair)
                            {
                                if (strGroupName == "precedence0" && decValue > decHighest)
                                {
                                    decHighest = decValue;
                                    sbdNewModifier.Clear();
                                    sbdNewModifier.Append(strSpace).Append('+').Append(strSpace).Append(strSourceName)
                                                  .Append(strSpace).Append('(')
                                                  .Append(decValue.ToString(GlobalSettings.CultureInfo)).Append(')');
                                }
                            }

                            if (lstUniqueName.Contains("precedence-1"))
                            {
                                foreach ((string strGroupName, decimal decValue, string strSourceName) in lstUniquePair)
                                {
                                    if (strGroupName == "precedence-1")
                                    {
                                        decHighest += decValue;
                                        sbdNewModifier
                                            .Append(strSpace).Append('+').Append(strSpace).Append(strSourceName)
                                            .Append(strSpace).Append('(')
                                            .Append(decValue.ToString(GlobalSettings.CultureInfo)).Append(')');
                                    }
                                }
                            }

                            if (decHighest >= decBaseValue)
                            {
                                sbdModifier.Clear();
                                sbdModifier.Append(sbdNewModifier);
                            }
                        }
                    }
                    else if (lstUniqueName.Contains("precedence1"))
                    {
                        // Retrieve all of the items that are precedence1 and nothing else.
                        using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                      out StringBuilder sbdNewModifier))
                        {
                            foreach ((string _, decimal decValue, string strSourceName) in lstUniquePair.Where(
                                         s => s.Item1 == "precedence1" || s.Item1 == "precedence-1"))
                            {
                                sbdModifier.AppendFormat(GlobalSettings.CultureInfo, "{0}+{0}{1}{0}({2})", strSpace,
                                                         strSourceName, decValue);
                            }
                            sbdModifier.Clear();
                            sbdModifier.Append(sbdNewModifier);
                        }
                    }
                    else
                    {
                        // Run through the list of UniqueNames and pick out the highest value for each one.
                        foreach (string strName in lstUniqueName)
                        {
                            decimal decHighest = decimal.MinValue;
                            foreach ((string strGroupName, decimal decValue, string strSourceName) in lstUniquePair)
                            {
                                if (strGroupName == strName && decValue > decHighest)
                                {
                                    decHighest = decValue;
                                    sbdModifier.Append(strSpace).Append('+').Append(strSpace).Append(strSourceName)
                                               .Append(strSpace).Append('(')
                                               .Append(decValue.ToString(GlobalSettings.CultureInfo)).Append(')');
                                }
                            }
                        }
                    }

                    // Factor in Custom Improvements.
                    lstUniqueName.Clear();
                    lstUniquePair.Clear();
                    foreach (Improvement objImprovement in lstUsedImprovements.Where(
                                 objImprovement => objImprovement.Custom))
                    {
                        string strUniqueName = objImprovement.UniqueName;
                        if (!string.IsNullOrEmpty(strUniqueName))
                        {
                            // If this has a UniqueName, run through the current list of UniqueNames seen. If it is not already in the list, add it.
                            if (!lstUniqueName.Contains(strUniqueName))
                                lstUniqueName.Add(strUniqueName);

                            // Add the values to the UniquePair List so we can check them later.
                            lstUniquePair.Add(new Tuple<string, decimal, string>(
                                                  strUniqueName, objImprovement.Augmented * objImprovement.Rating,
                                                  _objCharacter.GetObjectName(
                                                      objImprovement, GlobalSettings.Language)));
                        }
                        else
                        {
                            sbdModifier.Append(strSpace).Append('+').Append(strSpace)
                                       .Append(_objCharacter.GetObjectName(objImprovement, GlobalSettings.Language))
                                       .Append(strSpace).Append('(')
                                       .Append((objImprovement.Augmented * objImprovement.Rating).ToString(
                                                   GlobalSettings.CultureInfo)).Append(')');
                        }
                    }

                    // Run through the list of UniqueNames and pick out the highest value for each one.
                    foreach (string strName in lstUniqueName)
                    {
                        decimal decHighest = decimal.MinValue;
                        foreach ((string strGroupName, decimal decValue, string strSourceName) in lstUniquePair)
                        {
                            if (strGroupName == strName && decValue > decHighest)
                            {
                                decHighest = decValue;
                                sbdModifier.Append(strSpace).Append('+').Append(strSpace).Append(strSourceName)
                                           .Append(strSpace).Append('(')
                                           .Append(decValue.ToString(GlobalSettings.CultureInfo)).Append(')');
                            }
                        }
                    }

                    //// If this is AGI or STR, factor in any Cyberlimbs.
                    if ((Abbrev == "AGI" || Abbrev == "STR") && !_objCharacter.Settings.DontUseCyberlimbCalculation)
                    {
                        foreach (Cyberware objCyberware in _objCharacter.Cyberware)
                        {
                            if (objCyberware.Category == "Cyberlimb")
                            {
                                sbdModifier.AppendLine().Append(objCyberware.CurrentDisplayName).Append(strSpace)
                                           .Append('(').Append(Abbrev == "AGI"
                                                                   ? objCyberware.TotalAgility.ToString(
                                                                       GlobalSettings.CultureInfo)
                                                                   : objCyberware.TotalStrength.ToString(
                                                                       GlobalSettings.CultureInfo)).Append(')');
                            }
                        }
                    }

                    sbdModifier.Insert(0, '(' + Value.ToString(GlobalSettings.CultureInfo) + ')')
                               .Insert(0, DisplayAbbrev + strSpace);
                    return _strCachedToolTip = sbdModifier.ToString();
                }
            }
        }

        public int SpentPriorityPoints
        {
            get
            {
                int intBase = Base;
                int intReturn = intBase;

                decimal decExtra = 0;
                decimal decMultiplier = 1.0m;
                foreach (Improvement objLoopImprovement in _objCharacter.Improvements)
                {
                    if ((objLoopImprovement.ImprovedName == Abbrev || string.IsNullOrEmpty(objLoopImprovement.ImprovedName)) &&
                        (string.IsNullOrEmpty(objLoopImprovement.Condition) || (objLoopImprovement.Condition == "career") == _objCharacter.Created || (objLoopImprovement.Condition == "create") != _objCharacter.Created) &&
                        objLoopImprovement.Minimum <= intBase && objLoopImprovement.Enabled)
                    {
                        switch (objLoopImprovement.ImproveType)
                        {
                            case Improvement.ImprovementType.AttributePointCost:
                                decExtra += objLoopImprovement.Value * (Math.Min(intBase, objLoopImprovement.Maximum == 0 ? int.MaxValue : objLoopImprovement.Maximum) - objLoopImprovement.Minimum);
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

        public bool AtMetatypeMaximum => Value == TotalMaximum && TotalMinimum > 0;

        public int KarmaMaximum => Math.Max(TotalMaximum - TotalBase, 0);
        public int PriorityMaximum => Math.Max(TotalMaximum - Karma - FreeBase - RawMinimum, 0);

        private int _intCachedUpgradeKarmaCost = int.MinValue;

        /// <summary>
        /// Karma price to upgrade. Returns negative if impossible
        /// </summary>
        /// <returns>Price in karma</returns>
        public int UpgradeKarmaCost
        {
            get
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
                if (_objCharacter.Settings.AlternateMetatypeAttributeKarma)
                    intUpgradeCost -= (MetatypeMinimum - 1) * intOptionsCost;

                decimal decExtra = 0;
                decimal decMultiplier = 1.0m;
                foreach (Improvement objLoopImprovement in _objCharacter.Improvements)
                {
                    if ((objLoopImprovement.ImprovedName == Abbrev || string.IsNullOrEmpty(objLoopImprovement.ImprovedName)) &&
                        (string.IsNullOrEmpty(objLoopImprovement.Condition) || (objLoopImprovement.Condition == "career") == _objCharacter.Created || (objLoopImprovement.Condition == "create") != _objCharacter.Created) &&
                            (objLoopImprovement.Maximum == 0 || intValue + 1 <= objLoopImprovement.Maximum) && objLoopImprovement.Minimum <= intValue + 1 && objLoopImprovement.Enabled)
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

        public int TotalKarmaCost
        {
            get
            {
                if (Karma == 0)
                    return 0;

                int intValue = Value;
                int intRawTotalBase = _objCharacter.Settings.ReverseAttributePriorityOrder ? Math.Max(FreeBase + RawMinimum, TotalMinimum) : TotalBase;
                int intTotalBase = intRawTotalBase;
                if (_objCharacter.Settings.AlternateMetatypeAttributeKarma)
                {
                    int intHumanMinimum = _objCharacter.Settings.ReverseAttributePriorityOrder ? FreeBase + 1 + MinimumModifiers : Base + FreeBase + 1 + MinimumModifiers;
                    if (intHumanMinimum < 1)
                    {
                        if (_objCharacter.IsCritter || _intMetatypeMax == 0 || Abbrev == "EDG" || Abbrev == "MAG" || Abbrev == "MAGAdept" || Abbrev == "RES" || Abbrev == "DEP")
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
                    if ((objLoopImprovement.ImprovedName == Abbrev || string.IsNullOrEmpty(objLoopImprovement.ImprovedName)) &&
                        (string.IsNullOrEmpty(objLoopImprovement.Condition) || (objLoopImprovement.Condition == "career") == _objCharacter.Created || (objLoopImprovement.Condition == "create") != _objCharacter.Created) &&
                            objLoopImprovement.Minimum <= intValue && objLoopImprovement.Enabled)
                    {
                        switch (objLoopImprovement.ImproveType)
                        {
                            case Improvement.ImprovementType.AttributeKarmaCost:
                                decExtra += objLoopImprovement.Value * (Math.Min(intValue, objLoopImprovement.Maximum == 0 ? int.MaxValue : objLoopImprovement.Maximum) - Math.Max(intRawTotalBase, objLoopImprovement.Minimum - 1));
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
        private int _intCachedCanUpgradeCareer = -1;

        public bool CanUpgradeCareer
        {
            get
            {
                if (_intCachedCanUpgradeCareer < 0)
                    _intCachedCanUpgradeCareer = _objCharacter.Karma >= UpgradeKarmaCost && TotalMaximum > Value ? 1 : 0;

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
                            (Abbrev == "AGI" || Abbrev == "STR") &&
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
                        if ((Abbrev == "AGI" || Abbrev == "STR") &&
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
            HashSet<string> lstNamesOfChangedProperties = null;
            foreach (string strPropertyName in lstPropertyNames)
            {
                if (lstNamesOfChangedProperties == null)
                    lstNamesOfChangedProperties = s_AttributeDependencyGraph.GetWithAllDependents(this, strPropertyName);
                else
                {
                    foreach (string strLoopChangedProperty in s_AttributeDependencyGraph.GetWithAllDependents(this, strPropertyName))
                        lstNamesOfChangedProperties.Add(strLoopChangedProperty);
                }
            }

            if (lstNamesOfChangedProperties == null || lstNamesOfChangedProperties.Count == 0)
                return;

            if (lstNamesOfChangedProperties.Contains(nameof(CanUpgradeCareer)))
                _intCachedCanUpgradeCareer = -1;
            if (lstNamesOfChangedProperties.Contains(nameof(Value)))
                _intCachedValue = int.MinValue;
            if (lstNamesOfChangedProperties.Contains(nameof(TotalValue)))
                _intCachedTotalValue = int.MinValue;
            if (lstNamesOfChangedProperties.Contains(nameof(UpgradeKarmaCost)))
                _intCachedUpgradeKarmaCost = int.MinValue;
            if (lstNamesOfChangedProperties.Contains(nameof(ToolTip)))
                _strCachedToolTip = string.Empty;
            foreach (string strPropertyToChange in lstNamesOfChangedProperties)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strPropertyToChange));
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
        private static readonly DependencyGraph<string, CharacterAttrib> s_AttributeDependencyGraph =
            new DependencyGraph<string, CharacterAttrib>(
                new DependencyGraphNode<string, CharacterAttrib>(nameof(ToolTip),
                    new DependencyGraphNode<string, CharacterAttrib>(nameof(DisplayValue),
                        new DependencyGraphNode<string, CharacterAttrib>(nameof(HasModifiers)),
                        new DependencyGraphNode<string, CharacterAttrib>(nameof(TotalValue),
                            new DependencyGraphNode<string, CharacterAttrib>(nameof(AttributeModifiers)),
                            new DependencyGraphNode<string, CharacterAttrib>(nameof(MetatypeAugmentedMaximum)),
                            new DependencyGraphNode<string, CharacterAttrib>(nameof(MetatypeMaximum)),
                            new DependencyGraphNode<string, CharacterAttrib>(nameof(TotalAugmentedMaximum),
                                new DependencyGraphNode<string, CharacterAttrib>(nameof(AugmentedMaximumModifiers)),
                                new DependencyGraphNode<string, CharacterAttrib>(nameof(MetatypeAugmentedMaximum)),
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

        public void Upgrade(int intAmount = 1)
        {
            for (int i = 0; i < intAmount; ++i)
            {
                if (_objCharacter.Created)
                {
                    if (!CanUpgradeCareer)
                        return;

                    int intPrice = UpgradeKarmaCost;
                    int intValue = Value;

                    string strUpgradetext = string.Format(GlobalSettings.CultureInfo, "{1}{0}{2}{0}{3}{0}->{0}{4}",
                        LanguageManager.GetString("String_Space"), LanguageManager.GetString("String_ExpenseAttribute"), Abbrev, intValue, intValue + 1);

                    ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
                    objExpense.Create(intPrice * -1, strUpgradetext, ExpenseType.Karma, DateTime.Now);
                    objExpense.Undo = new ExpenseUndo().CreateKarma(KarmaExpenseType.ImproveAttribute, Abbrev);

                    _objCharacter.ExpenseEntries.AddWithSort(objExpense);

                    _objCharacter.Karma -= intPrice;
                }

                ++Karma;
            }
        }

        public void Degrade(int intAmount)
        {
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
                else if (Abbrev == "EDG" && _objCharacter.Created && TotalMinimum > 0)
                {
                    //Edge can reduce the metatype minimum below zero.
                    --MetatypeMinimum;
                }
                else
                    return;
            }
        }

        #endregion static
    }
}
