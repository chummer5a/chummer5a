using Chummer.Annotations;
using Chummer.Backend.Equipment;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Xml;
using Chummer.Datastructures;

namespace Chummer.Backend.Attributes
{
    /// <summary>
    /// Character CharacterAttribute. 
    /// If using databinding, you should generally be using AttributeSection.{ATT}Binding
    /// </summary>
    [DebuggerDisplay("{" + nameof(_strAbbrev) + "}")]
    public class CharacterAttrib : INotifyPropertyChanged
    {
        private int _intMetatypeMin = 1;
        private int _intMetatypeMax = 6;
        private int _intMetatypeAugMax = 9;
        private int _intAugModifier;
        private int _intBase;
        private int _intKarma;
        private string _strAbbrev = string.Empty;
        public Character _objCharacter;
		private string _strDisplayNameShort;
		private string _strDisplayNameLong;
		private string _strDisplayNameFormatted;
		private AttributeCategory _enumCategory;
		private AttributeCategory _enumMetatypeCategory;
		private string _strDisplayAbbrev;

		public event PropertyChangedEventHandler PropertyChanged;

		#region Constructor, Save, Load, and Print Methods

		/// <summary>
		/// Character CharacterAttribute.
		/// </summary>
		/// <param name="character"></param>
		/// <param name="abbrev"></param>
		/// <param name="enumCategory"></param>
		public CharacterAttrib(Character character, string abbrev, AttributeCategory enumCategory = AttributeCategory.Standard)
        {
	        _strAbbrev = abbrev;
	        MetatypeCategory = enumCategory;
	        _objCharacter = character;
			_objCharacter.AttributeImprovementEvent += OnImprovementEvent;
			_objCharacter.PropertyChanged += OnCharacterChanged;
		}

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("attribute");
            objWriter.WriteElementString("name", _strAbbrev);
            objWriter.WriteElementString("metatypemin", _intMetatypeMin.ToString());
            objWriter.WriteElementString("metatypemax", _intMetatypeMax.ToString());
            objWriter.WriteElementString("metatypeaugmax", _intMetatypeAugMax.ToString());
            objWriter.WriteElementString("base", _intBase.ToString());
            objWriter.WriteElementString("karma", _intKarma.ToString());
            objWriter.WriteElementString("augmodifier", _intAugModifier.ToString());
			objWriter.WriteElementString("metatypecategory", MetatypeCategory.ToString());
            // External reader friendly stuff.
            objWriter.WriteElementString("totalvalue", TotalValue.ToString());
            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Load the CharacterAttribute from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            _strAbbrev = objNode["name"].InnerText;
            _intMetatypeMin = Convert.ToInt32(objNode["metatypemin"].InnerText);
            _intMetatypeMax = Convert.ToInt32(objNode["metatypemax"].InnerText);
            _intMetatypeAugMax = Convert.ToInt32(objNode["metatypeaugmax"].InnerText);
            objNode.TryGetField("base", out _intBase);
            objNode.TryGetField("karma", out _intKarma);
			if (!BaseUnlocked)
			{
				_intBase = 0;
			}
			//Converts old attributes to split metatype minimum and base. Saves recalculating Base - TotalMinimum all the time. 
			if (objNode["value"] != null)
			{
				int i = Convert.ToInt32(objNode["value"].InnerText);
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
			_enumMetatypeCategory = ConvertToAttributeCategory(objNode["category"]?.InnerText);
			_enumCategory = ConvertToAttributeCategory(_strAbbrev);
	        _enumMetatypeCategory = ConvertToMetatypeAttributeCategory(objNode["metatypecategory"]?.InnerText ?? "Standard");
			_intAugModifier = Convert.ToInt32(objNode["augmodifier"].InnerText);
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Print(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("attribute");
            objWriter.WriteElementString("name_english", Abbrev);
            objWriter.WriteElementString("name", DisplayAbbrev);
            objWriter.WriteElementString("base", Value.ToString());
            objWriter.WriteElementString("total", TotalValue.ToString());
            objWriter.WriteElementString("min", TotalMinimum.ToString());
            objWriter.WriteElementString("max", TotalMaximum.ToString());
            objWriter.WriteElementString("aug", TotalAugmentedMaximum.ToString());
			objWriter.WriteElementString("bp", CalculatedBP().ToString());
			objWriter.WriteElementString("metatypecategory", MetatypeCategory.ToString());
			objWriter.WriteEndElement();
        }
        #endregion
        /// <summary>
        /// Type of Attribute.
        /// </summary>
        public enum AttributeCategory
        {
            Standard = 0,
            Special = 1,
            Shapeshifter = 2
        }

        #region Properties

	    public AttributeCategory Category
	    {
		    get { return _enumCategory; }
			set { _enumCategory = value; }
		}

		public AttributeCategory MetatypeCategory
		{
			get { return _enumMetatypeCategory; }
			set { _enumMetatypeCategory = value; }
		}

		/// <summary>
		/// Minimum value for the CharacterAttribute as set by the character's Metatype.
		/// </summary>
		public int MetatypeMinimum
        {
            get
            {
                int intReturn = _intMetatypeMin;
                foreach (Improvement objImprovement in _objCharacter.Improvements.Where(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.ReplaceAttribute).Where(objImprovement => objImprovement.ImprovedName == Abbrev))
                {
                    intReturn = objImprovement.Minimum;
                }
                return intReturn;
            }
            set
            {
                _intMetatypeMin = value;
                OnPropertyChanged(nameof(FreeBase));
                OnPropertyChanged(nameof(TotalBase));
                OnPropertyChanged(nameof(AugmentedMetatypeLimits));
            }
        }

        /// <summary>
        /// Maximum value for the CharacterAttribute as set by the character's Metatype.
        /// </summary>
        public int MetatypeMaximum
        {
            get
            {
                int intReturn = _intMetatypeMax;
                foreach (Improvement objImprovement in _objCharacter.Improvements.Where(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.ReplaceAttribute).Where(objImprovement => objImprovement.ImprovedName == Abbrev))
                {
                    intReturn = objImprovement.Maximum;
                }
                return intReturn;
            }
            set
            {
                _intMetatypeMax = value;
                OnPropertyChanged(nameof(AugmentedMetatypeLimits));
            }
        }

        /// <summary>
        /// Maximum augmented value for the CharacterAttribute as set by the character's Metatype.
        /// </summary>
        public int MetatypeAugmentedMaximum
        {
            get
            {
                return _intMetatypeAugMax;
            }
            set
            {
                _intMetatypeAugMax = value;
                OnPropertyChanged(nameof(AugmentedMetatypeLimits));
            }
        }

        /// <summary>
        /// Current base value of the CharacterAttribute.
        /// </summary>
        public int Base
        {
            get
            {
                return _intBase;
            }
            set
            {
                _intBase = value;
                OnPropertyChanged(nameof(Value));
            }
        }

        /// <summary>
        /// Value of Base as used for attribute controls. 
        /// </summary>
        public int TotalBase
        {
            //TODO: Ugly ugly ugly, may cause UI confusion.
            get { return Math.Max(Base + FreeBase + TotalMinimum, TotalMinimum); }
            set
            {
                Base = Math.Max(value - FreeBase - TotalMinimum, 0);
            }
        }

        public int FreeBase
        {
            get
            {
                return Math.Min(ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.Attributelevel, false, Abbrev), MetatypeMaximum - MetatypeMinimum);
            }
        }

        /// <summary>
        /// Current karma value of the CharacterAttribute.
        /// </summary>
        public int Karma
        {
            get
            {
                return _intKarma;
            }
            set
            {
                _intKarma = value;
                OnPropertyChanged(nameof(Value));
            }
        }

        /// <summary>
        /// Current value of the CharacterAttribute before modifiers are applied.
        /// </summary>
        public int Value
        {
            get
            {
                return Math.Min(Base + FreeBase + Karma + TotalMinimum + AttributeValueModifiers, TotalMaximum);
            }
        }

        /// <summary>
        /// Formatted Value of the attribute, including the sum of any modifiers in brackets.
        /// </summary>
        public string DisplayValue
        {
            get
            {
                return HasModifiers ? $"{Value} ({CalculatedTotalValue()})" : $"{Value}";
            }
        }

        /// <summary>
        /// Augmentation modifier value for the CharacterAttribute.
        /// </summary>
        /// <remarks>This value should not be saved with the character information. It should instead be re-calculated every time the character is loaded and augmentations are added/removed.</remarks>
        public int AugmentModifier
        {
            get
            {
                return _intAugModifier;
            }
            set
            {
                _intAugModifier = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The CharacterAttribute's total value including augmentations.
        /// </summary>
        /// <remarks>This value should not be saved with the character information. It should instead be re-calculated every time the character is loaded and augmentations are added/removed.</remarks>
        public int Augmented
        {
            get
            {
                return Value + _intAugModifier;
            }
        }

        /// <summary>
        /// The total amount of the modifiers that affect the CharacterAttribute's value without affecting Karma costs.
        /// </summary>
        public int AttributeModifiers
        {
            get
            {
                HashSet<string> lstUniqueName = new HashSet<string>();
                HashSet<Tuple<string, int>> lstUniquePair = new HashSet<Tuple<string, int>>();
                int intModifier = 0;
                foreach (Improvement objImprovement in _objCharacter.Improvements)
                {
                    if (objImprovement.Enabled && !objImprovement.Custom && objImprovement.ImproveType == Improvement.ImprovementType.Attribute && objImprovement.ImprovedName == _strAbbrev && string.IsNullOrEmpty(objImprovement.Condition))
                    {
                        string strUniqueName = objImprovement.UniqueName;
                        if (!string.IsNullOrEmpty(strUniqueName))
                        {
                            // If this has a UniqueName, run through the current list of UniqueNames seen. If it is not already in the list, add it.
                            if (!lstUniqueName.Contains(strUniqueName))
                                lstUniqueName.Add(strUniqueName);

                            // Add the values to the UniquePair List so we can check them later.
                            lstUniquePair.Add(new Tuple<string, int>(strUniqueName, objImprovement.Augmented * objImprovement.Rating));
                        }
                        else if (!((Abbrev == "MAG" || Abbrev == "DEP" || Abbrev == "RES") &&
                                     objImprovement.SourceName == "Essence Loss" &&
                                    _objCharacter.Options.ESSLossReducesMaximumOnly && _objCharacter.EssencePenalty > 0))
                        {
                            intModifier += objImprovement.Augmented * objImprovement.Rating;
                        }
                    }
                }

                if (lstUniqueName.Contains("precedence0"))
                {
                    // Retrieve only the highest precedence0 value.
                    // Run through the list of UniqueNames and pick out the highest value for each one.
                    int intHighest = int.MinValue;
                    foreach (Tuple<string, int> strValues in lstUniquePair)
                    {
                        if (strValues.Item1 == "precedence0")
                        {
                            if (strValues.Item2 > intHighest)
                                intHighest = strValues.Item2;
                        }
                    }
                    if (lstUniqueName.Contains("precedence-1"))
                    {
                        foreach (Tuple<string, int> strValues in lstUniquePair)
                        {
                            if (strValues.Item1 == "precedence-1")
                            {
                                intHighest += strValues.Item2;
                            }
                        }
                    }
                    intModifier = Math.Max(intHighest, intModifier);
                }
                else if (lstUniqueName.Contains("precedence1"))
                {
                    // Retrieve all of the items that are precedence1 and nothing else.
                    int intTmpModifier = 0;
                    foreach (Tuple<string, int> strValues in lstUniquePair)
                    {
                        if (strValues.Item1 == "precedence1" || strValues.Item1 == "precedence-1")
                            intTmpModifier += strValues.Item2;
                    }
                    intModifier = Math.Max(intTmpModifier, intModifier);
                }
                else
                {
                    // Run through the list of UniqueNames and pick out the highest value for each one.
                    foreach (string strName in lstUniqueName)
                    {
                        int intHighest = int.MinValue;
                        foreach (Tuple<string, int> strValues in lstUniquePair)
                        {
                            if (strValues.Item1 == strName)
                            {
                                if (strValues.Item2 > intHighest)
                                    intHighest = strValues.Item2;
                            }
                        }
                        if (intHighest != int.MinValue)
                            intModifier += intHighest;
                    }
                }

                // Factor in Custom Improvements.
                lstUniqueName.Clear();
                lstUniquePair.Clear();
                int intCustomModifier = 0;
                foreach (Improvement objImprovement in _objCharacter.Improvements)
                {
                    if (objImprovement.Enabled && objImprovement.Custom && objImprovement.ImproveType == Improvement.ImprovementType.Attribute && objImprovement.ImprovedName == _strAbbrev && string.IsNullOrEmpty(objImprovement.Condition))
                    {
                        string strUniqueName = objImprovement.UniqueName;
                        if (!string.IsNullOrEmpty(strUniqueName))
                        {
                            // If this has a UniqueName, run through the current list of UniqueNames seen. If it is not already in the list, add it.
                            if (!lstUniqueName.Contains(strUniqueName))
                                lstUniqueName.Add(strUniqueName);

                            // Add the values to the UniquePair List so we can check them later.
                            lstUniquePair.Add(new Tuple<string, int>(strUniqueName, objImprovement.Augmented * objImprovement.Rating));
                        }
                        else
                        {
                            intCustomModifier += objImprovement.Augmented * objImprovement.Rating;
                        }
                    }
                }

                // Run through the list of UniqueNames and pick out the highest value for each one.
                foreach (string strName in lstUniqueName)
                {
                    int intHighest = int.MinValue;
                    foreach (Tuple<string, int> strValues in lstUniquePair)
                    {
                        if (strValues.Item1 == strName)
                        {
                            if (strValues.Item2 > intHighest)
                                intHighest = strValues.Item2;
                        }
                    }
                    if (intHighest != int.MinValue)
                        intCustomModifier += intHighest;
                }

                return intModifier + intCustomModifier;
            }
        }

        /// <summary>
        /// The total amount of the modifiers that raise the actual value of the CharacterAttribute and increase its Karma cost.
        /// </summary>
        public int AttributeValueModifiers
        {
            get
            {
                HashSet<string> lstUniqueName = new HashSet<string>();
                HashSet<Tuple<string, int>> lstUniquePair = new HashSet<Tuple<string, int>>();
                int intModifier = 0;
                foreach (Improvement objImprovement in _objCharacter.Improvements)
                {
                    if (objImprovement.Enabled && objImprovement.ImproveType == Improvement.ImprovementType.Attribute && objImprovement.ImprovedName == _strAbbrev + "Base" && string.IsNullOrEmpty(objImprovement.Condition))
                    {
                        string strUniqueName = objImprovement.UniqueName;
                        if (!string.IsNullOrEmpty(strUniqueName))
                        {
                            // If this has a UniqueName, run through the current list of UniqueNames seen. If it is not already in the list, add it.
                            if (!lstUniqueName.Contains(strUniqueName))
                                lstUniqueName.Add(strUniqueName);

                            // Add the values to the UniquePair List so we can check them later.
                            lstUniquePair.Add(new Tuple<string, int>(strUniqueName, objImprovement.Augmented * objImprovement.Rating));
                        }
                        else
                        {
                            intModifier += objImprovement.Augmented * objImprovement.Rating;
                        }
                    }
                }

                if (lstUniqueName.Contains("precedence0"))
                {
                    // Retrieve only the highest precedence0 value.
                    // Run through the list of UniqueNames and pick out the highest value for each one.
                    int intHighest = int.MinValue;
                    foreach (Tuple<string, int> strValues in lstUniquePair)
                    {
                        if (strValues.Item1 == "precedence0")
                        {
                            if (strValues.Item2 > intHighest)
                                intHighest = strValues.Item2;
                        }
                    }
                    if (lstUniqueName.Contains("precedence-1"))
                    {
                        foreach (Tuple<string, int> strValues in lstUniquePair)
                        {
                            if (strValues.Item1 == "precedence-1")
                            {
                                intHighest += strValues.Item2;
                            }
                        }
                    }
                    intModifier = Math.Max(intHighest, intModifier);
                }
                else if (lstUniqueName.Contains("precedence1"))
                {
                    // Retrieve all of the items that are precedence1 and nothing else.
                    int intTmpModifier = 0;
                    foreach (Tuple<string, int> strValues in lstUniquePair)
                    {
                        if (strValues.Item1 == "precedence1" || strValues.Item1 == "precedence-1")
                            intTmpModifier += strValues.Item2;
                    }
                    intModifier = Math.Max(intTmpModifier, intModifier);
                }
                else
                {
                    // Run through the list of UniqueNames and pick out the highest value for each one.
                    foreach (string strName in lstUniqueName)
                    {
                        int intHighest = int.MinValue;
                        foreach (Tuple<string, int> strValues in lstUniquePair)
                        {
                            if (strValues.Item1 == strName)
                            {
                                if (strValues.Item2 > intHighest)
                                    intHighest = strValues.Item2;
                            }
                        }
                        if (intHighest != int.MinValue)
                            intModifier += intHighest;
                    }
                }

                return intModifier;
            }
        }

        /// <summary>
        /// Whether or not the CharacterAttribute has any modifiers from Improvements.
        /// </summary>
        public bool HasModifiers
        {
            get
            {
                foreach (Improvement objImprovement in _objCharacter.Improvements)
                {
                    if (objImprovement.ImproveType == Improvement.ImprovementType.Attribute && (objImprovement.ImprovedName == _strAbbrev || objImprovement.ImprovedName == _strAbbrev + "Base") && objImprovement.Enabled && objImprovement.Augmented != 0)
                        return true;
                }

                // If this is AGI or STR, factor in any Cyberlimbs.
                if (!_objCharacter.Options.DontUseCyberlimbCalculation && (_strAbbrev == "AGI" || _strAbbrev == "STR"))
                {
                    foreach (Cyberware objCyberware in _objCharacter.Cyberware)
                    {
                        if (objCyberware.Category == "Cyberlimb" && !string.IsNullOrEmpty(objCyberware.LimbSlot))
                            return true;
                    }
                }

                if ((_objCharacter.MAGEnabled && Abbrev == "MAG" || _objCharacter.RESEnabled && Abbrev == "RES" || _objCharacter.DEPEnabled && Abbrev == "DEP") && _objCharacter.EssencePenalty > 0)
                {
                    return true;
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
                foreach (Improvement objImprovement in _objCharacter.Improvements)
                {
                    if (objImprovement.ImproveType == Improvement.ImprovementType.Attribute && (objImprovement.ImprovedName == _strAbbrev || objImprovement.ImprovedName == _strAbbrev + "Base") && objImprovement.Enabled)
                    {
                        intModifier += objImprovement.Minimum * objImprovement.Rating;
                    }
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
                return _objCharacter.Improvements.Where(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.Attribute && (objImprovement.ImprovedName == _strAbbrev || objImprovement.ImprovedName == _strAbbrev + "Base") && objImprovement.Enabled).Sum(objImprovement => objImprovement.Maximum * objImprovement.Rating);
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
                foreach (Improvement objImprovement in _objCharacter.Improvements)
                {
                    if (objImprovement.ImproveType == Improvement.ImprovementType.Attribute && objImprovement.ImprovedName == _strAbbrev && objImprovement.Enabled)
                    {
                        intModifier += objImprovement.AugmentedMaximum * objImprovement.Rating;
                    }
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
            if (_objCharacter.MetatypeCategory == "Cyberzombie" && _strAbbrev == "MAG")
                return 1;

            int intMeat = Value + AttributeModifiers;
            int intReturn = intMeat;

            //// If this is AGI or STR, factor in any Cyberlimbs.
            if ((_strAbbrev == "AGI" || _strAbbrev == "STR") && !_objCharacter.Options.DontUseCyberlimbCalculation && blnIncludeCyberlimbs)
            {
                int intLimbTotal = 0;
                int intLimbCount = 0;
                foreach (Cyberware objCyberware in _objCharacter.Cyberware
                    .Where(objCyberware => objCyberware.Category == "Cyberlimb" && !string.IsNullOrWhiteSpace(objCyberware.LimbSlot) && !_objCharacter.Options.ExcludeLimbSlot.Contains(objCyberware.LimbSlot)))
                {
                    intLimbCount += objCyberware.LimbSlotCount;
                    switch (_strAbbrev)
                    {
                        case "STR":
                            intLimbTotal += objCyberware.TotalStrength * objCyberware.LimbSlotCount;
                            break;
                        default:
                            intLimbTotal += objCyberware.TotalAgility * objCyberware.LimbSlotCount;
                            break;
                    }
                }

                if (intLimbCount > 0)
                {
                    int intMaxLimbs = _objCharacter.LimbCount();
                    int intMissingLimbCount = Math.Max(intMaxLimbs - intLimbCount, 0);
                    // Not all of the limbs have been replaced, so we need to place the Attribute in the other "limbs" to get the average value.
                    intLimbTotal += intMeat * intMissingLimbCount;
                    intReturn = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(intLimbTotal, GlobalOptions.CultureInfo) / Convert.ToDecimal(intMaxLimbs, GlobalOptions.CultureInfo)));
                }
            }
            // Do not let the CharacterAttribute go above the Metatype's Augmented Maximum.
            if (intReturn > TotalAugmentedMaximum)
                intReturn = TotalAugmentedMaximum;

            // An Attribute cannot go below 1 unless it is EDG, MAG, or RES, the character is a Critter, or the Metatype Maximum is 0.
            if (intReturn < 1)
            {
                if ((_objCharacter.CritterEnabled || _strAbbrev == "EDG" || _intMetatypeMax == 0 || (_objCharacter.EssencePenalty != 0 && (_strAbbrev == "MAG" || _strAbbrev == "RES")) || (_objCharacter.MetatypeCategory != "A.I." && _strAbbrev == "DEP")))
                    return 0;
                else
                    return 1;
            }
            return intReturn;
        }

        /// <summary>
        /// The CharacterAttribute's total value (Value + Modifiers).
        /// </summary>
        public int TotalValue
        {
            get { return CalculatedTotalValue(); }
        }

        /// <summary>
        /// The CharacterAttribute's combined Minimum value (Metatype Minimum + Modifiers).
        /// </summary>
        public int TotalMinimum
        {
            get
            {
                // If we're looking at MAG and the character is a Cyberzombie, MAG is always 1, regardless of ESS penalties and bonuses.
                if (_objCharacter.MetatypeCategory == "Cyberzombie" && _strAbbrev == "MAG")
                    return 1;

                int intReturn = MetatypeMinimum + MinimumModifiers;
                if (intReturn < 1)
                {
                    if (_objCharacter.IsCritter || _intMetatypeMax == 0 || _strAbbrev == "EDG")
                        intReturn = 0;
                    else
                        intReturn = 1;
                }

                if (_strAbbrev != "MAG" && _strAbbrev != "RES" && _strAbbrev != "DEP")
                    return intReturn;

                int intEssencePenalty = _objCharacter.EssencePenalty;
                if (_strAbbrev == "MAG")
                    intEssencePenalty = _objCharacter.EssencePenaltyMAG;
                if (intEssencePenalty == 0)
                    return intReturn;

                if (!_objCharacter.Options.ESSLossReducesMaximumOnly)
                {
                    return Math.Max(intReturn - intEssencePenalty, 0);
                }
                if (intEssencePenalty >= TotalMaximum)
                {
                    intReturn = Math.Max(intReturn - intEssencePenalty, 0);
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
                int intReturn = MetatypeMaximum + MaximumModifiers;

                if (intReturn < 0)
                    intReturn = 0;

                // If we're looking at MAG and the character is a Cyberzombie, MAG is always 1, regardless of ESS penalties and bonuses.
                if (_objCharacter.MetatypeCategory == "Cyberzombie" && _strAbbrev == "MAG")
                    intReturn = 1;

                return intReturn;
            }
        }

        /// <summary>
        /// The CharacterAttribute's combined Augmented Maximum value (Metatype Augmented Maximum + Modifiers).
        /// </summary>
        public int TotalAugmentedMaximum
        {
            get
            {
                int intReturn = 0;
                if (_strAbbrev == "EDG" || _strAbbrev == "MAG" || _strAbbrev == "RES")
                    intReturn = TotalMaximum + AugmentedMaximumModifiers;
                else
                    intReturn = TotalMaximum + 4 + AugmentedMaximumModifiers;
                // intReturn = TotalMaximum + Convert.ToInt32(Math.Floor((Convert.ToDecimal(TotalMaximum, GlobalOptions.CultureInfo) / 2))) + AugmentedMaximumModifiers;

                if (intReturn < 0)
                    intReturn = 0;

                // If we're looking at MAG and the character is a Cyberzombie, MAG is always 1, regardless of ESS penalties and bonuses.
                if (_objCharacter.MetatypeCategory == "Cyberzombie" && _strAbbrev == "MAG")
                    intReturn = 1;

                return intReturn;
            }
        }

        /// <summary>
        /// CharacterAttribute abbreviation.
        /// </summary>
        public string Abbrev
        {
            get
            {
                return _strAbbrev;
            }
        }

        public string DisplayNameShort
        {
            get {
                if (string.IsNullOrWhiteSpace(_strDisplayNameShort))
                {
                    string strName = "String_Attribute{0}Short".Replace("{0}", _strAbbrev);
                    _strDisplayNameShort = LanguageManager.GetString(strName);
                    return _strDisplayNameShort;
                }
                return _strDisplayNameShort;
                }
            set { _strDisplayNameShort = value; }
        }

        public string DisplayNameLong
        {
            get
            {
                //TODO: Is this a terrible idea?
                if (string.IsNullOrWhiteSpace(_strDisplayNameLong))
                {
                    string strName = "String_Attribute{0}Long".Replace("{0}", _strAbbrev);
                    _strDisplayNameLong = LanguageManager.GetString(strName);
                    return _strDisplayNameLong;
                }
                return _strDisplayNameLong;
            }
            set { _strDisplayNameLong = value; }
        }

        public string DisplayNameFormatted
        {
            get
            {
                //TODO: Is this a terrible idea?
                if (string.IsNullOrWhiteSpace(_strDisplayNameFormatted))
                {
                    _strDisplayNameFormatted = DisplayNameLong + " (" + DisplayNameShort + ")";
                    return _strDisplayNameFormatted;
                }
                return _strDisplayNameFormatted;
            }
            set { _strDisplayNameFormatted = value; }
        }

        /// <summary>
        /// Is it possible to place points in Base or is it prevented by their build method?
        /// </summary>
        public bool BaseUnlocked
        {
            get
            {
                return _objCharacter.BuildMethod.HaveSkillPoints();
            }
        }

        /// <summary>
        /// CharacterAttribute Limits
        /// </summary>
        public string MetatypeLimits
        {
            get
            {
                return string.Format("{0} / {1} ({2})", MetatypeMinimum, MetatypeMaximum, MetatypeAugmentedMaximum);
            }
        }

        /// <summary>
        /// CharacterAttribute Limits
        /// </summary>
        public string AugmentedMetatypeLimits
        {
            get
            {
                return string.Format("{0} / {1} ({2})", TotalMinimum, TotalMaximum, TotalAugmentedMaximum);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Set the minimum, maximum, and augmented values for the CharacterAttribute based on string values from the Metatype XML file.
        /// </summary>
        /// <param name="strMin">Metatype's minimum value for the CharacterAttribute.</param>
        /// <param name="strMax">Metatype's maximum value for the CharacterAttribute.</param>
        /// <param name="strAug">Metatype's maximum augmented value for the CharacterAttribute.</param>
        public void AssignLimits(string strMin, string strMax, string strAug)
        {
            MetatypeMinimum = Convert.ToInt32(strMin);
            MetatypeMaximum = Convert.ToInt32(strMax);
            MetatypeAugmentedMaximum = Convert.ToInt32(strAug);
        }

        public string UpgradeToolTip
        {
            get
            {
                return string.Format(LanguageManager.GetString("Tip_ImproveItem"), (Value + 1), UpgradeKarmaCost());
            }
        }

        /// <summary>
        /// ToolTip that shows how the CharacterAttribute is calculating its Modified Rating.
        /// </summary>
        public string ToolTip
        {
            get
            {
                string strReturn = string.Empty;
                strReturn += _strAbbrev + " (" + Value.ToString() + ")";
                string strModifier = string.Empty;

                HashSet<string> lstUniqueName = new HashSet<string>();
                List<Tuple<string, int, string>> lstUniquePair = new List<Tuple<string, int, string>>();
                int intBaseValue = 0;
                foreach (Improvement objImprovement in _objCharacter.Improvements)
                {
                    if (objImprovement.Enabled && !objImprovement.Custom && objImprovement.ImproveType == Improvement.ImprovementType.Attribute && objImprovement.ImprovedName == _strAbbrev && string.IsNullOrEmpty(objImprovement.Condition))
                    {
                        string strUniqueName = objImprovement.UniqueName;
                        if (!string.IsNullOrEmpty(strUniqueName) && strUniqueName != "enableattribute" && objImprovement.ImproveType == Improvement.ImprovementType.Attribute &&
                            objImprovement.ImprovedName == _strAbbrev)
                        {
                            // If this has a UniqueName, run through the current list of UniqueNames seen. If it is not already in the list, add it.
                            if (!lstUniqueName.Contains(strUniqueName))
                                lstUniqueName.Add(strUniqueName);

                            // Add the values to the UniquePair List so we can check them later.
                            lstUniquePair.Add(new Tuple<string, int, string>(strUniqueName, objImprovement.Augmented * objImprovement.Rating, _objCharacter.GetObjectName(objImprovement)));
                        }
                        else if (!(objImprovement.Value == 0 && objImprovement.Augmented == 0))
                        {
                            strModifier += " + " + _objCharacter.GetObjectName(objImprovement) + " (" +
                                           (objImprovement.Augmented * objImprovement.Rating).ToString() + ")";
                            intBaseValue += objImprovement.Augmented * objImprovement.Rating;
                        }
                    }
                }

                if (lstUniqueName.Contains("precedence0"))
                {
                    // Retrieve only the highest precedence0 value.
                    // Run through the list of UniqueNames and pick out the highest value for each one.
                    int intHighest = int.MinValue;

                    string strNewModifier = string.Empty;
                    foreach (Tuple<string, int, string> strValues in lstUniquePair)
                    {
                        if (strValues.Item1 == "precedence0")
                        {
                            if (strValues.Item2 > intHighest)
                            {
                                intHighest = strValues.Item2;
                                strNewModifier = " + " + strValues.Item3 + " (" + strValues.Item2.ToString() + ")";
                            }
                        }
                    }
                    if (lstUniqueName.Contains("precedence-1"))
                    {
                        foreach (Tuple<string, int, string> strValues in lstUniquePair)
                        {
                            if (strValues.Item1 == "precedence-1")
                            {
                                intHighest += strValues.Item2;
                                strNewModifier += " + " + strValues.Item3 + " (" + strValues.Item2.ToString() + ")";
                            }
                        }
                    }

                    if (intHighest > intBaseValue)
                        strModifier = strNewModifier;
                }
                else if (lstUniqueName.Contains("precedence1"))
                {
                    // Retrieve all of the items that are precedence1 and nothing else.
                    int intHighest = int.MinValue;
                    string strNewModifier = string.Empty;
                    foreach (Tuple<string, int, string> strValues in lstUniquePair)
                    {
                        if (strValues.Item1 == "precedence1" || strValues.Item1 == "precedence-1")
                        {
                            strNewModifier += " + " + strValues.Item3 + " (" + strValues.Item2.ToString() + ")";
                            intHighest += strValues.Item2;
                        }
                    }
                    if (intHighest > intBaseValue)
                        strModifier = strNewModifier;
                }
                else
                {
                    // Run through the list of UniqueNames and pick out the highest value for each one.
                    foreach (string strName in lstUniqueName)
                    {
                        int intHighest = int.MinValue;
                        foreach (Tuple<string, int, string> strValues in lstUniquePair)
                        {
                            if (strValues.Item1 == strName)
                            {
                                if (strValues.Item2 > intHighest)
                                {
                                    intHighest = strValues.Item2;
                                    strModifier += " + " + strValues.Item3 + " (" + strValues.Item2.ToString() + ")";
                                }
                            }
                        }
                    }
                }

                // Factor in Custom Improvements.
                lstUniqueName.Clear();
                lstUniquePair.Clear();
                foreach (Improvement objImprovement in _objCharacter.Improvements)
                {
                    if (objImprovement.Enabled && objImprovement.Custom && objImprovement.ImproveType == Improvement.ImprovementType.Attribute && objImprovement.ImprovedName == _strAbbrev && string.IsNullOrEmpty(objImprovement.Condition))
                    {
                        string strUniqueName = objImprovement.UniqueName;
                        if (!string.IsNullOrEmpty(strUniqueName))
                        {
                            // If this has a UniqueName, run through the current list of UniqueNames seen. If it is not already in the list, add it.
                            if (!lstUniqueName.Contains(strUniqueName))
                                lstUniqueName.Add(strUniqueName);

                            // Add the values to the UniquePair List so we can check them later.
                            lstUniquePair.Add(new Tuple<string, int, string>(strUniqueName, objImprovement.Augmented * objImprovement.Rating, _objCharacter.GetObjectName(objImprovement)));
                        }
                        else
                        {
                            strModifier += " + " + _objCharacter.GetObjectName(objImprovement) + " (" +
                                               (objImprovement.Augmented * objImprovement.Rating).ToString() + ")";
                        }
                    }
                }

                // Run through the list of UniqueNames and pick out the highest value for each one.
                foreach (string strName in lstUniqueName)
                {
                    int intHighest = int.MinValue;
                    foreach (Tuple<string, int, string> strValues in lstUniquePair)
                    {
                        if (strValues.Item1 == strName)
                        {
                            if (strValues.Item2 > intHighest)
                            {
                                intHighest = strValues.Item2;
                                strModifier += " + " + strValues.Item3 + " (" + strValues.Item2.ToString() + ")";
                            }
                        }
                    }
                }

                //// If this is AGI or STR, factor in any Cyberlimbs.
                StringBuilder strCyberlimb = new StringBuilder();
                if ((_strAbbrev == "AGI" || _strAbbrev == "STR") && !_objCharacter.Options.DontUseCyberlimbCalculation)
                {
                    LanguageManager.Load(GlobalOptions.Language, null);
                    foreach (Cyberware objCyberware in _objCharacter.Cyberware)
                    {
                        if (objCyberware.Category == "Cyberlimb")
                        {
                            strCyberlimb.Append("\n");
                            strCyberlimb.Append(objCyberware.DisplayName + " (");
                            if (_strAbbrev == "AGI")
                            {
                                strCyberlimb.Append(objCyberware.TotalAgility.ToString());
                            }
                            else
                            {
                                strCyberlimb.Append(objCyberware.TotalStrength.ToString());
                            }
                            strCyberlimb.Append(")");
                        }
                    }
                    strModifier += strCyberlimb;
                }
                /*
                if ((_strAbbrev == "RES" || _strAbbrev == "MAG" || _strAbbrev == "DEP") && _objCharacter.EssencePenalty != 0)
                {
                    strModifier += $" + -{_objCharacter.EssencePenalty} ({LanguageManager.GetString("String_AttributeESSLong")})";
                }
                */

                return strReturn + strModifier;
            }
        }

        public Character CharacteObject => _objCharacter;

        /// <summary>
        /// Amount of BP/Karma spent on this CharacterAttribute.
        /// </summary>
        public int CalculatedBP()
        {
            int intBP = 0;

            if (_strAbbrev != "EDG" && _strAbbrev != "MAG" && _strAbbrev != "RES")
            {
                if (_objCharacter.Options.AlternateMetatypeAttributeKarma)
                {
                    // Weird house rule method that treats the Metatype's minimum as being 1 for the purpose of calculating Karma costs.
                    for (int i = 1; i <= _objCharacter.GetAttribute(_strAbbrev).Value - _objCharacter.GetAttribute(_strAbbrev).TotalMinimum; i++)
                        intBP += (i + 1) * _objCharacter.Options.KarmaAttribute;
                }
                else
                {
                    // Karma calculation starts from the minimum score + 1 and steps through each up to the current score. At each step, the current number is multplied by the Karma Cost to
                    // give us the cost of at each step.
                    for (int i = _objCharacter.GetAttribute(_strAbbrev).TotalMinimum + 1; i <= _objCharacter.GetAttribute(_strAbbrev).Value; i++)
                        intBP += i * _objCharacter.Options.KarmaAttribute;
                }
            }
            else
            {
                // Find the character's Essence Loss. This applies unless the house rule to have ESS Loss only affect the Maximum of the CharacterAttribute is turned on.
                int intEssenceLoss = 0;
                if (!_objCharacter.Options.ESSLossReducesMaximumOnly)
                {
                    intEssenceLoss = _objCharacter.EssencePenalty;
                    if (_strAbbrev == "MAG")
                        intEssenceLoss = _objCharacter.EssencePenaltyMAG;
                }

                // Don't apply the ESS loss penalty to EDG.
                int intUseEssenceLoss = intEssenceLoss;
                if (_strAbbrev == "EDG")
                    intUseEssenceLoss = 0;

                // If the character has an ESS penalty, the minimum needs to be bumped up by 1 so that the cost calculation is correct.
                int intMinModifier = 0;
                if (intUseEssenceLoss > 0)
                    intMinModifier = 1;

                if (_objCharacter.GetAttribute(_strAbbrev).TotalMinimum == 0 && _objCharacter.GetAttribute(_strAbbrev).TotalMaximum == 0)
                {
                    intBP += 0;
                }
                else
                {
                    // Karma calculation starts from the minimum score + 1 and steps through each up to the current score. At each step, the current number is multplied by the Karma Cost to
                    // give us the cost of at each step.
                    for (int i = _objCharacter.GetAttribute(_strAbbrev).TotalMinimum + 1 + intMinModifier; i <= _objCharacter.GetAttribute(_strAbbrev).Value + intUseEssenceLoss; i++)
                        intBP += i * _objCharacter.Options.KarmaAttribute;
                }
            }

            return intBP;
        }

        public int SpentPriorityPoints => Base;

        public bool AtMetatypeMaximum => Value == TotalMaximum && TotalMinimum > 0;

        public int KarmaMaximum => TotalMaximum - TotalBase;
        public int PriorityMaximum => TotalMaximum - Karma;
        /// <summary>
        /// Karma price to upgrade. Returns negative if impossible
        /// </summary>
        /// <returns>Price in karma</returns>
        public virtual int UpgradeKarmaCost()
        {
            int upgrade;
            if (Value >= TotalMaximum)
            {
                upgrade = -1;
            }
            else if (Value == 0)
            {
                upgrade = _objCharacter.Options.KarmaAttribute;
            }
            else
            {
                upgrade = (Value + 1) * _objCharacter.Options.KarmaAttribute;
            }
            if (_objCharacter.Options.AlternateMetatypeAttributeKarma)
                upgrade -= (_objCharacter.STR.MetatypeMinimum - 1) * _objCharacter.Options.KarmaAttribute;

            if (_strAbbrev == "STR" && _objCharacter.Cyberware.Find(x =>
                        x.Name == "Myostatin Inhibitor") != null)
            {
                upgrade -= 2;
            }

            return upgrade;

        }

        public virtual int TotalKarmaCost()
        {
            if (Karma == 0)
                return 0;

            int intTotalBase = TotalBase;
            if (_objCharacter.Options.AlternateMetatypeAttributeKarma)
                intTotalBase = 1;

            // The expression below is a shortened version of n*(n+1)/2 when applied to karma costs. n*(n+1)/2 is the sum of all numbers from 1 to n.
            // I'm taking n*(n+1)/2 where n = Base + Karma, then subtracting n*(n+1)/2 from it where n = Base. After removing all terms that cancel each other out, the expression below is what remains.
            int intCost = (2 * intTotalBase + Karma + 1) * Karma / 2 * _objCharacter.Options.KarmaAttribute;

            // Since Myostatin Inhibitor just gives a flat -2 per karma level, its effect can be calculated by simple multiplication
            if (Abbrev == "STR" && _objCharacter.Cyberware.Find(x => x.Name == "Myostatin Inhibitor") != null)
            {
                intCost -= 2 * Karma;
            }

            return intCost;
        }

        public bool CanUpgradeCareer
        {
            get { return _objCharacter.Karma >= UpgradeKarmaCost() && TotalMaximum > Value; }
        }

        // Caching the value prevents calling the event multiple times. 
        private bool _oldUpgrade;
        private void OnCharacterChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName != nameof(Character.Karma)) return;
            if (_oldUpgrade == CanUpgradeCareer) return;
            _oldUpgrade = CanUpgradeCareer;
            OnPropertyChanged(nameof(CanUpgradeCareer));
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            foreach (string s in DependencyTree.Find(propertyName))
            {
                var v = new PropertyChangedEventArgs(s);
                PropertyChanged?.Invoke(this, v);
            }

        }

		/// <summary>
		/// Convert a string to an Attribute Category.
		/// </summary>
		/// <param name="strValue">String value to convert.</param>
		/// <param name="strAbbrev">Linked attribute abbreviation.</param>
		public AttributeCategory ConvertToAttributeCategory(string strAbbrev)
		{
			switch (strAbbrev)
			{
				case "DEP":
				case "EDG":
				case "ESS":
				case "MAG":
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
		public AttributeCategory ConvertToMetatypeAttributeCategory(string strValue)
		{
			//If a value does exist, test whether it belongs to a shapeshifter form.
			switch (strValue)
			{
				case "Shapeshifter":
					return AttributeCategory.Shapeshifter;
				case "Metahuman":
				case "Standard":
				default:
					return AttributeCategory.Standard;
			}
		}
		#endregion

        #region static

        private static readonly Lazy<HashSet<string>> _physicalAttributes =
            new Lazy<HashSet<string>>(() => new HashSet<string>() { "BOD", "AGI", "REA", "STR" },
                LazyThreadSafetyMode.PublicationOnly);

        public static HashSet<string> PhysicalAttributes
        {
            get { return _physicalAttributes.Value; }
        }
        //A tree of dependencies. Once some of the properties are changed, 
        //anything they depend on, also needs to raise OnChanged
        //This tree keeps track of dependencies
        private static readonly ReverseTree<string> DependencyTree =
            new ReverseTree<string>(nameof(ToolTip),
                new ReverseTree<string>(nameof(DisplayValue),
                    new ReverseTree<string>(nameof(Augmented),
                    new ReverseTree<string>(nameof(TotalValue),
                        new ReverseTree<string>(nameof(AttributeModifiers)),
                                    new ReverseTree<string>(nameof(Karma)),
                                    new ReverseTree<string>(nameof(Base)),
                                        new ReverseTree<string>(nameof(AugmentedMetatypeLimits),
                                            new ReverseTree<string>(nameof(TotalMinimum)),
                                            new ReverseTree<string>(nameof(TotalMaximum)),
                                            new ReverseTree<string>(nameof(TotalAugmentedMaximum)))))));

        public string UpgradeKarmaCostString
        {
            get
            {
               return LanguageManager.GetString("Message_ConfirmKarmaExpense").Replace("{0}", _strAbbrev.Replace("{1}", (Value + 1).ToString()).Replace("{2}", UpgradeKarmaCost().ToString()));
            }
        }

        /// <summary>
        /// Translated abbreviation of the attribute.
        /// </summary>
        public string DisplayAbbrev
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_strDisplayAbbrev))
                {
                    _strDisplayAbbrev = LanguageManager.GetString($"String_Attribute{Abbrev}Short");
                }
                return _strDisplayAbbrev;
            }
        }

        public void Upgrade()
        {
            if (!CanUpgradeCareer) return;

            int price = UpgradeKarmaCost();
            string upgradetext = $"{LanguageManager.GetString("String_ExpenseAttribute")} {_strAbbrev} {Value} -> {Value + AttributeValueModifiers + 1}";

            ExpenseLogEntry entry = new ExpenseLogEntry();
            entry.Create(price * -1, upgradetext, ExpenseType.Karma, DateTime.Now);
            entry.Undo = new ExpenseUndo().CreateKarma(KarmaExpenseType.ImproveAttribute, _strAbbrev);

            _objCharacter.ExpenseEntries.Add(entry);

            Karma += 1;
            _objCharacter.Karma -= price;
        }

        public void Degrade(int intValue)
        {
            for (int i = intValue; i > 0; i--)
            {
                if (Karma > 0)
                {
                    Karma -= 1;
                }
                else if (Base > 0)
                {
                    Base -= 1;
                }
                else if (Abbrev == "EDG" && TotalMinimum > 0)
                {
                    //Edge can reduce the metatype minimum below zero. 
                    MetatypeMinimum -= 1;
                }
                else
                    return;
            }
        }
        [Obsolete("Refactor this method away once improvementmanager gets outbound events")]
        private void OnImprovementEvent(List<Improvement> improvements)
        {
            if (improvements.Any(imp => imp.ImproveType == Improvement.ImprovementType.Attribute && imp.ImprovedName == Abbrev && imp.Enabled && imp.Augmented != 0))
            {
                OnPropertyChanged(nameof(Augmented));
            }
            else if (improvements.Any(imp => imp.ImproveType == Improvement.ImprovementType.ReplaceAttribute && imp.ImprovedName == Abbrev))
            {
                OnPropertyChanged(nameof(AugmentedMetatypeLimits));
            }
            else if (improvements.Any(imp => imp.ImproveType == Improvement.ImprovementType.Attribute && imp.ImprovedName == Abbrev && imp.Enabled && imp.AugmentedMaximum != 0 || imp.Maximum != 0))
            {
                foreach (Improvement i in improvements.Where(imp => imp.ImproveType == Improvement.ImprovementType.Attribute && imp.ImprovedName == Abbrev && imp.Enabled))
                {
                    if (i.Maximum != 0 || i.AugmentedMaximum != 0)
                    {
                        OnPropertyChanged(nameof(TotalAugmentedMaximum));
                    }
                    if (i.Minimum != 0)
                    {
                        OnPropertyChanged(nameof(TotalMinimum));
                    }
                    if (i.Value != 0)
                    {
                        OnPropertyChanged(nameof(TotalValue));
                    }
                }
                OnPropertyChanged(nameof(AugmentedMetatypeLimits));
            }
            else if (improvements.Any(imp => imp.ImproveSource == Improvement.ImprovementSource.Cyberware))
            {
                OnPropertyChanged(nameof(AttributeModifiers));
            }
            else if (improvements.Any(imp => imp.ImproveType == Improvement.ImprovementType.Attributelevel))
            {
                OnPropertyChanged(nameof(Base));
            }
        }

        /// <summary>
        /// Forces a particular event to fire.
        /// </summary>
        /// <param name="property"></param>
        public void ForceEvent(string property)
        {
            foreach (string s in DependencyTree.Find(property))
            {
                var v = new PropertyChangedEventArgs(s);
                PropertyChanged?.Invoke(this, v);
            }
        }
        #endregion
    }
}
