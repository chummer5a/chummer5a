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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
 using System.Text;
 using System.Threading;
 using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Annotations;
 using Chummer.Backend.Equipment;
using Chummer.Backend;
 using Chummer.Skills;

namespace Chummer
{
	/// <summary>
	/// Character CharacterAttribute.
	/// </summary>
	[DebuggerDisplay("{_strAbbrev}")]
	public class CharacterAttrib : INotifyPropertyChanged
	{
		private int _intMetatypeMin = 1;
		private int _intMetatypeMax = 6;
		private int _intMetatypeAugMax = 9;
		private int _intValue = 0;
		private int _intAugModifier = 0;
        private int _intBase = 0;
        private int _intKarma = 0;
		private string _strAbbrev;
		public Character _objCharacter;

		public event PropertyChangedEventHandler PropertyChanged;

		#region Constructor, Save, Load, and Print Methods
		/// <summary>
		/// Character CharacterAttribute.
		/// </summary>
		/// <param name="strAbbrev">CharacterAttribute abbreviation.</param>
		public CharacterAttrib(string strAbbrev)
		{
			_strAbbrev = strAbbrev;
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
			objWriter.WriteElementString("value", Value.ToString());
            objWriter.WriteElementString("base", _intBase.ToString());
            objWriter.WriteElementString("karma", _intKarma.ToString());
            objWriter.WriteElementString("augmodifier", _intAugModifier.ToString());
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
            if (objNode == null)
                return;
            objNode.TryGetStringFieldQuickly("name", ref _strAbbrev);
            objNode.TryGetInt32FieldQuickly("metatypemin", ref _intMetatypeMin);
            objNode.TryGetInt32FieldQuickly("metatypemax", ref _intMetatypeMax);
            objNode.TryGetInt32FieldQuickly("metatypeaugmax", ref _intMetatypeAugMax);

            objNode.TryGetInt32FieldQuickly("base", ref _intBase);
		    objNode.TryGetInt32FieldQuickly("karma", ref _intKarma);

            int intTemp = 0;
            if (objNode.TryGetInt32FieldQuickly("value", ref intTemp))
                Value = intTemp;
            objNode.TryGetInt32FieldQuickly("augmodifier", ref _intAugModifier);

			//TODO: This causes an issue if MAG/RES is burned down to 0 from Essence loss. Is this a legacy load item?
            //if (_intBase == 0)
            //    _intBase = Value;
		}

		/// <summary>
		/// Print the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public void Print(XmlTextWriter objWriter, string strOverwriteBase = "")
		{
			objWriter.WriteStartElement("attribute");
			objWriter.WriteElementString("name", _strAbbrev);
			objWriter.WriteElementString("base", !string.IsNullOrEmpty(strOverwriteBase) ? strOverwriteBase : Value.ToString());
			objWriter.WriteElementString("total", TotalValue.ToString());
			objWriter.WriteElementString("min", TotalMinimum.ToString());
			objWriter.WriteElementString("max", TotalMaximum.ToString());
			objWriter.WriteElementString("aug", TotalAugmentedMaximum.ToString());
			objWriter.WriteElementString("bp", CalculatedBP().ToString());
			objWriter.WriteEndElement();
		}
		#endregion

		#region Properties
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
				// If changing the Minimum would cause the current value to be outside of its bounds, bring it back within acceptable limits.
				if (Value < value + MinimumModifiers)
					Value = value + MinimumModifiers;
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
				// If changing the Maximum would cause the current value to be outside of its bounds, bring it back within acceptable limits.
				if (Value > value + MaximumModifiers)
					Value = value + MaximumModifiers;
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
				OnPropertyChanged();
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
				OnPropertyChanged();
            }
        }

        /// <summary>
		/// Current value of the CharacterAttribute.
		/// </summary>
		public int Value
		{
			get
			{
				//This amount of object allocation is uglier than the US national dept, but improvementmanager is due for a rewrite anyway
				ImprovementManager _objImprovement = new ImprovementManager(_objCharacter);
				return _intValue + MetatypeMinimum + Math.Min(_objImprovement.ValueOf(Improvement.ImprovementType.Attributelevel, false, Abbrev),MetatypeMaximum - MetatypeMinimum);
			}
			set
			{
				ImprovementManager _objImprovement = new ImprovementManager(_objCharacter);

				_intValue = value - (MetatypeMinimum + Math.Min(_objImprovement.ValueOf(Improvement.ImprovementType.Attributelevel, false, Abbrev), MetatypeMaximum - MetatypeMinimum));
				OnPropertyChanged();
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
		/// The total amount of the modifiers that affect the CharacterAttribute's value.
		/// </summary>
		public int AttributeModifiers
		{
			get
			{
				List<string> lstUniqueName = new List<string>();
				List<string[,]> lstUniquePair = new List<string[,]>();
				int intModifier = 0;
				foreach (Improvement objImprovement in _objCharacter.Improvements)
				{
					if (objImprovement.Enabled && !objImprovement.Custom)
					{
						if (!string.IsNullOrEmpty(objImprovement.UniqueName) && objImprovement.ImproveType == Improvement.ImprovementType.Attribute && objImprovement.ImprovedName == _strAbbrev)
						{
							// If this has a UniqueName, run through the current list of UniqueNames seen. If it is not already in the list, add it.
							bool blnFound = false;
							foreach (string strName in lstUniqueName)
							{
								if (strName == objImprovement.UniqueName)
									blnFound = true;
								break;
							}
							if (!blnFound)
								lstUniqueName.Add(objImprovement.UniqueName);

							// Add the values to the UniquePair List so we can check them later.
							string[,] strValues = new string[,] { { objImprovement.UniqueName, (objImprovement.Augmented * objImprovement.Rating).ToString() } };
							lstUniquePair.Add(strValues);
						}
						else
						{
							if (objImprovement.ImproveType == Improvement.ImprovementType.Attribute && objImprovement.ImprovedName == _strAbbrev)
								intModifier += objImprovement.Augmented * objImprovement.Rating;
						}
					}
				}

				if (lstUniqueName.Contains("precedence0"))
				{
					// Retrieve only the highest precedence0 value.
					// Run through the list of UniqueNames and pick out the highest value for each one.
					int intHighest = -999;
					foreach (string[,] strValues in lstUniquePair)
					{
						if (strValues[0, 0] == "precedence0")
						{
							if (Convert.ToInt32(strValues[0, 1]) > intHighest)
								intHighest = Convert.ToInt32(strValues[0, 1]);
						}
					}
					if (lstUniqueName.Contains("precedence-1"))
					{
						foreach (string[,] strValues in lstUniquePair)
						{
							if (strValues[0, 0] == "precedence-1")
							{
								intHighest += Convert.ToInt32(strValues[0, 1]);
							}
						}
					}
					intModifier = intHighest;
				}
				else if (lstUniqueName.Contains("precedence1"))
				{
					// Retrieve all of the items that are precedence1 and nothing else.
					intModifier = 0;
					foreach (string[,] strValues in lstUniquePair)
					{
						if (strValues[0, 0] == "precedence1" || strValues[0, 0] == "precedence-1")
							intModifier += Convert.ToInt32(strValues[0, 1]);
					}
				}
				else
				{
					// Run through the list of UniqueNames and pick out the highest value for each one.
					foreach (string strName in lstUniqueName)
					{
						int intHighest = -999;
						foreach (string[,] strValues in lstUniquePair)
						{
							if (strValues[0, 0] == strName)
							{
								if (Convert.ToInt32(strValues[0, 1]) > intHighest)
									intHighest = Convert.ToInt32(strValues[0, 1]);
							}
						}
						intModifier += intHighest;
					}
				}

				// Factor in Custom Improvements.
				lstUniqueName = new List<string>();
				lstUniquePair = new List<string[,]>();
				int intCustomModifier = 0;
				if (_strAbbrev == "REA")
				{
				}
				foreach (Improvement objImprovement in _objCharacter.Improvements)
				{
					if (objImprovement.Enabled && objImprovement.Custom)
					{
						if (!string.IsNullOrEmpty(objImprovement.UniqueName) && objImprovement.ImproveType == Improvement.ImprovementType.Attribute && objImprovement.ImprovedName == _strAbbrev)
						{
							// If this has a UniqueName, run through the current list of UniqueNames seen. If it is not already in the list, add it.
							bool blnFound = false;
							foreach (string strName in lstUniqueName)
							{
								if (strName == objImprovement.UniqueName)
									blnFound = true;
								break;
							}
							if (!blnFound)
								lstUniqueName.Add(objImprovement.UniqueName);

							// Add the values to the UniquePair List so we can check them later.
							string[,] strValues = new string[,] { { objImprovement.UniqueName, (objImprovement.Augmented * objImprovement.Rating).ToString() } };
							lstUniquePair.Add(strValues);
						}
						else
						{
							if (objImprovement.ImproveType == Improvement.ImprovementType.Attribute && objImprovement.ImprovedName == _strAbbrev)
								intCustomModifier += objImprovement.Augmented * objImprovement.Rating;
						}
					}
				}

				// Run through the list of UniqueNames and pick out the highest value for each one.
				foreach (string strName in lstUniqueName)
				{
					int intHighest = -999;
					foreach (string[,] strValues in lstUniquePair)
					{
						if (strValues[0, 0] == strName)
						{
							if (Convert.ToInt32(strValues[0, 1]) > intHighest)
								intHighest = Convert.ToInt32(strValues[0, 1]);
						}
					}
					intCustomModifier += intHighest;
				}

				intModifier += AttributeValueModifiers + intCustomModifier;
				return intModifier;
			}
		}

		/// <summary>
		/// The total amount of the modifiers that raise the actual value of the CharacterAttribute and increase its Karma cost.
		/// </summary>
		public int AttributeValueModifiers
		{
			get
			{
				List<string> lstUniqueName = new List<string>();
				List<string[,]> lstUniquePair = new List<string[,]>();
				int intModifier = 0;
				foreach (Improvement objImprovement in _objCharacter.Improvements)
				{
					if (objImprovement.Enabled)
					{
						if (!string.IsNullOrEmpty(objImprovement.UniqueName) && objImprovement.ImproveType == Improvement.ImprovementType.Attribute && objImprovement.ImprovedName == _strAbbrev + "Base")
						{
							// If this has a UniqueName, run through the current list of UniqueNames seen. If it is not already in the list, add it.
							bool blnFound = false;
							foreach (string strName in lstUniqueName)
							{
								if (strName == objImprovement.UniqueName)
									blnFound = true;
								break;
							}
							if (!blnFound)
								lstUniqueName.Add(objImprovement.UniqueName);

							// Add the values to the UniquePair List so we can check them later.
							string[,] strValues = new string[,] { { objImprovement.UniqueName, (objImprovement.Augmented * objImprovement.Rating).ToString() } };
							lstUniquePair.Add(strValues);
						}
						else
						{
							if (objImprovement.ImproveType == Improvement.ImprovementType.Attribute && objImprovement.ImprovedName == _strAbbrev + "Base")
								intModifier += objImprovement.Augmented * objImprovement.Rating;
						}
					}
				}

				if (lstUniqueName.Contains("precedence0"))
				{
					// Retrieve only the highest precedence0 value.
					// Run through the list of UniqueNames and pick out the highest value for each one.
					int intHighest = -999;
					foreach (string[,] strValues in lstUniquePair)
					{
						if (strValues[0, 0] == "precedence0")
						{
							if (Convert.ToInt32(strValues[0, 1]) > intHighest)
								intHighest = Convert.ToInt32(strValues[0, 1]);
						}
					}
					if (lstUniqueName.Contains("precedence-1"))
					{
						foreach (string[,] strValues in lstUniquePair)
						{
							if (strValues[0, 0] == "precedence-1")
							{
								intHighest += Convert.ToInt32(strValues[0, 1]);
							}
						}
					}
					intModifier = intHighest;
				}
				else if (lstUniqueName.Contains("precedence1"))
				{
					// Retrieve all of the items that are precedence1 and nothing else.
					intModifier = 0;
					foreach (string[,] strValues in lstUniquePair)
					{
						if (strValues[0, 0] == "precedence1" || strValues[0, 0] == "precedence-1")
							intModifier += Convert.ToInt32(strValues[0, 1]);
					}
				}
				else
				{
					// Run through the list of UniqueNames and pick out the highest value for each one.
					foreach (string strName in lstUniqueName)
					{
						int intHighest = -999;
						foreach (string[,] strValues in lstUniquePair)
						{
							if (strValues[0, 0] == strName)
							{
								if (Convert.ToInt32(strValues[0, 1]) > intHighest)
									intHighest = Convert.ToInt32(strValues[0, 1]);
							}
						}
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
					if (objImprovement.ImproveType == Improvement.ImprovementType.Attribute && objImprovement.ImprovedName == _strAbbrev && objImprovement.Enabled)
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
				int intModifier = 0;
				foreach (Improvement objImprovement in _objCharacter.Improvements)
				{
					if (objImprovement.ImproveType == Improvement.ImprovementType.Attribute && objImprovement.ImprovedName == _strAbbrev && objImprovement.Enabled)
					{
						intModifier += objImprovement.Maximum * objImprovement.Rating;
					}
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
		public int CalculatedTotalValue (bool blnIncludeCyberlimbs = true)
		{
				int intMeat = Value + AttributeModifiers;
				int intReturn = intMeat;

                //// If this is AGI or STR, factor in any Cyberlimbs.
                if ((_strAbbrev == "AGI" || _strAbbrev == "STR") && !_objCharacter.Options.DontUseCyberlimbCalculation && blnIncludeCyberlimbs)
                {
                    int intLimbTotal = 0;
                    int intLimbCount = 0;
                    foreach (Cyberware objCyberware in _objCharacter.Cyberware
						.Where(objCyberware => objCyberware.Category == "Cyberlimb")
						.Where(objCyberware => !string.IsNullOrWhiteSpace(objCyberware.LimbSlot) && !_objCharacter.Options.ExcludeLimbSlot.Contains(objCyberware.LimbSlot)))
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
                        intReturn = 0;
                        if (intLimbCount < _objCharacter.Options.LimbCount)
                        {
                            // Not all of the limbs have been replaced, so we need to place the Attribute in the other "limbs" to get the average value.
                            for (int i = intLimbCount + 1; i <= _objCharacter.Options.LimbCount; i++)
                                intLimbTotal += intMeat;
                            intLimbCount = _objCharacter.Options.LimbCount;
                        }
                        int intTotal = intLimbTotal / intLimbCount;
                        intReturn += intTotal;
                    }
                }

				// Do not let the CharacterAttribute go above the Metatype's Augmented Maximum.
				if (intReturn > TotalAugmentedMaximum)
					intReturn = TotalAugmentedMaximum;

				// An Attribute cannot go below 1 unless it is EDG, MAG, or RES, the character is a Critter, or the Metatype Maximum is 0.
				if (_objCharacter.CritterEnabled || _strAbbrev == "EDG" || _intMetatypeMax == 0 || (_objCharacter.EssencePenalty != 0 && (_strAbbrev == "MAG" || _strAbbrev == "RES")))
				{
					if (intReturn < 0)
						return 0;
				}
				else
				{
					if (intReturn < 1)
						return 1;
				}

				// If we're looking at MAG and the character is a Cyberzombie, MAG is always 1, regardless of ESS penalties and bonuses.
				if (_objCharacter.MetatypeCategory == "Cyberzombie" && _strAbbrev == "MAG")
					return 1;

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
				int intReturn = MetatypeMinimum + MinimumModifiers;
				if (_objCharacter.IsCritter || _intMetatypeMax == 0)
				{
					if (intReturn < 0)
						intReturn = 0;
				}
				else
				{
					if (intReturn < 1)
						intReturn = 1;
				}
				/*
				if	(
					(_strAbbrev == "MAG" && !(_objCharacter.AdeptEnabled || _objCharacter.MagicianEnabled)) || 
					(_strAbbrev == "RES" && !_objCharacter.TechnomancerEnabled) || 
					(_strAbbrev == "DEP" && !(_objCharacter.Metatype == "A.I."))
					)
				{
					intReturn = 0;
				}*/

				if (_objCharacter.EssencePenalty != 0 && (_strAbbrev == "MAG" || _strAbbrev == "RES"))
				{
					if (_objCharacter.Options.ESSLossReducesMaximumOnly || _objCharacter.OverrideSpecialAttributeEssenceLoss)
					{
						// If the House Rule for Essence Loss Only Affects Maximum MAG/RES is turned on, the minimum should always be 1 unless the total ESS penalty is greater than or equal to
						// the CharacterAttribute's total maximum, in which case the minimum becomes 0.
						if (_objCharacter.EssencePenalty >= _objCharacter.MAG.TotalMaximum)
							intReturn = 0;
					}
					else
						intReturn = Math.Max(intReturn - _objCharacter.EssencePenalty, 0);
				}

				// If we're looking at MAG and the character is a Cyberzombie, MAG is always 1, regardless of ESS penalties and bonuses.
				if (_objCharacter.MetatypeCategory == "Cyberzombie" && _strAbbrev == "MAG")
					intReturn = 1;

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
				if (_strAbbrev == "EDG" || _strAbbrev == "MAG" || _strAbbrev == "RES" || _strAbbrev == "DEP")
                    intReturn = TotalMaximum + AugmentedMaximumModifiers;
				else
					intReturn = TotalMaximum + 4 + AugmentedMaximumModifiers;
                    // intReturn = TotalMaximum + (TotalMaximum / 2) + AugmentedMaximumModifiers;

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
		    int intTmp;
		    int.TryParse(strMin, out intTmp);
			MetatypeMinimum = intTmp;
            int.TryParse(strMax, out intTmp);
            MetatypeMaximum = intTmp;
            int.TryParse(strAug, out intTmp);
            MetatypeAugmentedMaximum = intTmp;
		}

		/// <summary>
		/// ToolTip that shows how the CharacterAttribute is calculating its Modified Rating.
		/// </summary>
		public string ToolTip()
		{
			string strReturn = _strAbbrev + " (" + Value.ToString() + ")";
			string strModifier = string.Empty;

			List<string> lstUniqueName = new List<string>();
			List<string[,]> lstUniquePair = new List<string[,]>();
			foreach (Improvement objImprovement in _objCharacter.Improvements)
			{
				if (objImprovement.Enabled && !objImprovement.Custom)
				{
					if (!string.IsNullOrEmpty(objImprovement.UniqueName) && objImprovement.ImproveType == Improvement.ImprovementType.Attribute && objImprovement.ImprovedName == _strAbbrev)
					{
						// If this has a UniqueName, run through the current list of UniqueNames seen. If it is not already in the list, add it.
						bool blnFound = false;
						foreach (string strName in lstUniqueName)
						{
							if (strName == objImprovement.UniqueName)
								blnFound = true;
							break;
						}
						if (!blnFound)
							lstUniqueName.Add(objImprovement.UniqueName);

						// Add the values to the UniquePair List so we can check them later.
						string[,] strValues = new string[,] { { objImprovement.UniqueName, (objImprovement.Augmented * objImprovement.Rating).ToString(), _objCharacter.GetObjectName(objImprovement) } };
						lstUniquePair.Add(strValues);
					}
					else
					{
						if (objImprovement.ImproveType == Improvement.ImprovementType.Attribute && objImprovement.ImprovedName == _strAbbrev && !(objImprovement.Value == 0 && objImprovement.Augmented == 0))
							strModifier += " + " + _objCharacter.GetObjectName(objImprovement) + " (" + (objImprovement.Augmented * objImprovement.Rating).ToString() + ")";
					}
				}
			}

			if (lstUniqueName.Contains("precedence0"))
			{
				// Retrieve only the highest precedence0 value.
				// Run through the list of UniqueNames and pick out the highest value for each one.
				int intHighest = -999;

				foreach (string[,] strValues in lstUniquePair)
				{
					if (strValues[0, 0] == "precedence0")
					{
						if (Convert.ToInt32(strValues[0, 1]) > intHighest)
						{
							intHighest = Convert.ToInt32(strValues[0, 1]);
							strModifier = " + " + strValues[0, 2] + " (" + strValues[0, 1] + ")";
						}
					}
				}
				if (lstUniqueName.Contains("precedence-1"))
				{
					foreach (string[,] strValues in lstUniquePair)
					{
						if (strValues[0, 0] == "precedence-1")
						{
							intHighest += Convert.ToInt32(strValues[0, 1]);
							strModifier += " + " + strValues[0, 2] + " (" + strValues[0, 1] + ")";
						}
					}
				}
			}
			else if (lstUniqueName.Contains("precedence1"))
			{
				// Retrieve all of the items that are precedence1 and nothing else.
				strModifier = string.Empty;
				foreach (string[,] strValues in lstUniquePair)
				{
					if (strValues[0, 0] == "precedence1" || strValues[0, 0] == "precedence-1")
						strModifier += " + " + strValues[0, 2] + " (" + strValues[0, 1] + ")";
				}
			}
			else
			{
				// Run through the list of UniqueNames and pick out the highest value for each one.
				foreach (string strName in lstUniqueName)
				{
					int intHighest = -999;
					foreach (string[,] strValues in lstUniquePair)
					{
						if (strValues[0, 0] == strName)
						{
							if (Convert.ToInt32(strValues[0, 1]) > intHighest)
							{
								intHighest = Convert.ToInt32(strValues[0, 1]);
								strModifier += " + " + strValues[0, 2] + " (" + strValues[0, 1] + ")";
							}
						}
					}
				}
			}

			// Factor in Custom Improvements.
			lstUniqueName = new List<string>();
			lstUniquePair = new List<string[,]>();
			foreach (Improvement objImprovement in _objCharacter.Improvements)
			{
				if (objImprovement.Enabled && objImprovement.Custom)
				{
					if (!string.IsNullOrEmpty(objImprovement.UniqueName) && objImprovement.ImproveType == Improvement.ImprovementType.Attribute && objImprovement.ImprovedName == _strAbbrev)
					{
						// If this has a UniqueName, run through the current list of UniqueNames seen. If it is not already in the list, add it.
						bool blnFound = false;
						foreach (string strName in lstUniqueName)
						{
							if (strName == objImprovement.UniqueName)
								blnFound = true;
							break;
						}
						if (!blnFound)
							lstUniqueName.Add(objImprovement.UniqueName);

						// Add the values to the UniquePair List so we can check them later.
						string[,] strValues = new string[,] { { objImprovement.UniqueName, (objImprovement.Augmented * objImprovement.Rating).ToString(), _objCharacter.GetObjectName(objImprovement) } };
						lstUniquePair.Add(strValues);
					}
					else
					{
						if (objImprovement.ImproveType == Improvement.ImprovementType.Attribute && objImprovement.ImprovedName == _strAbbrev)
							strModifier += " + " + _objCharacter.GetObjectName(objImprovement) + " (" + (objImprovement.Augmented * objImprovement.Rating).ToString() + ")";
					}
				}
			}

			// Run through the list of UniqueNames and pick out the highest value for each one.
			foreach (string strName in lstUniqueName)
			{
				int intHighest = -999;
				foreach (string[,] strValues in lstUniquePair)
				{
					if (strValues[0, 0] == strName)
					{
						if (Convert.ToInt32(strValues[0, 1]) > intHighest)
						{
							intHighest = Convert.ToInt32(strValues[0, 1]);
							strModifier = " + " + strValues[0, 2] + " (" + strValues[0, 1] + ")";
						}
					}
				}
			}

			//// If this is AGI or STR, factor in any Cyberlimbs.
			StringBuilder strCyberlimb = new StringBuilder();
			if ((_strAbbrev == "AGI" || _strAbbrev == "STR") && !_objCharacter.Options.DontUseCyberlimbCalculation)
			{
				LanguageManager.Instance.Load(GlobalOptions.Instance.Language, null);
				foreach (Cyberware objCyberware in _objCharacter.Cyberware)
                {
                    if (objCyberware.Category == "Cyberlimb")
                    {
	                    if (_strAbbrev == "AGI")
						{
							strCyberlimb.Append("\n");
							strCyberlimb.Append(objCyberware.DisplayName + " (");
							strCyberlimb.Append(objCyberware.TotalAgility.ToString());
							strCyberlimb.Append(")");
						}
	                    else
	                    {
							strCyberlimb.Append("\n");
							strCyberlimb.Append(objCyberware.DisplayName + " (");
							strCyberlimb.Append(objCyberware.TotalStrength.ToString());
							strCyberlimb.Append(")");
						}
                    }
                }
                    strModifier += strCyberlimb;
            }

			return strReturn + strModifier;
		}

		/// <summary>
		/// Amount of BP/Karma spent on this CharacterAttribute.
		/// </summary>
		private int CalculatedBP()
		{
			int intBP = 0;

			if (_strAbbrev != "EDG" && _strAbbrev != "MAG" && _strAbbrev != "RES" && _strAbbrev != "DEP")
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
				if (!_objCharacter.Options.ESSLossReducesMaximumOnly && !_objCharacter.OverrideSpecialAttributeEssenceLoss)
					intEssenceLoss = _objCharacter.EssencePenalty;

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

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			
		}
		#endregion

		#region static

		private static readonly Lazy<HashSet<string>> _physicalAttributes =
			new Lazy<HashSet<string>>(() => new HashSet<string>() {"BOD", "AGI", "REA", "STR"},
				LazyThreadSafetyMode.PublicationOnly);
		public static HashSet<string> PhysicalAttributes
		{
			get { return _physicalAttributes.Value; }
		}


		#endregion
	}

	/// <summary>
	/// Type of Quality.
	/// </summary>
	public enum QualityType
	{
		Positive = 0,
		Negative = 1,          
        LifeModule = 2,
        Entertainment = 3,
        Contracts = 4
	}

	/// <summary>
	/// Source of the Quality.
	/// </summary>
    public enum QualitySource
    {
        Selected = 0,
        Metatype = 1,
        MetatypeRemovable = 2,
		BuiltIn = 3,
        LifeModule = 4
    }

	/// <summary>
	/// Reason a quality is not valid
	/// </summary>
	[Flags]
	public enum QualityFailureReason : int
	{
		Allowed = 0x0,
		LimitExceeded =  0x1,
		RequiredSingle = 0x2,
		RequiredMultiple = 0x4,
		ForbiddenSingle = 0x8, 
        MetatypeRequired = 0x10
	}

	/// <summary>
	/// A Quality.
	/// </summary>
	public class Quality : INamedItemWithGuid
	{
		private Guid _guiID = new Guid();
		private string _strName = string.Empty;
        public string _strMetagenetic = string.Empty;
		private string _strExtra = string.Empty;
		private string _strSource = string.Empty;
		private string _strPage = string.Empty;
		private string _strMutant = string.Empty;
		private string _strNotes = string.Empty;
		private bool _blnImplemented = true;
		private bool _blnContributeToLimit = true;
		private bool _blnPrint = true;
		private int _intBP = 0;
        private int _intLP = 0;
		private QualityType _objQualityType = QualityType.Positive;
		private QualitySource _objQualitySource = QualitySource.Selected;
		private XmlNode _nodBonus;
		private XmlNode _nodDiscounts;
		private readonly Character _objCharacter;
		private string _strAltName = string.Empty;
		private string _strAltPage = string.Empty;
		private Guid _guiWeaponID = new Guid();
	    private Guid _qualiyGuid = new Guid();
		private string _stage;

		public String Stage
		{
			get { return _stage; }
			private set { _stage = value; }
		}

		#region Helper Methods
		/// <summary>
		/// Convert a string to a QualityType.
		/// </summary>
		/// <param name="strValue">String value to convert.</param>
		public QualityType ConvertToQualityType(string strValue)
		{
			switch (strValue)
			{
				case "Negative":
					return QualityType.Negative;
                case "LifeModule":
			        return QualityType.LifeModule;
				default:
					return QualityType.Positive;
			}
		}

		/// <summary>
		/// Convert a string to a QualitySource.
		/// </summary>
		/// <param name="strValue">String value to convert.</param>
		public QualitySource ConvertToQualitySource(string strValue)
		{
			switch (strValue)
			{
				case "Metatype":
					return QualitySource.Metatype;
				case "MetatypeRemovable":
					return QualitySource.MetatypeRemovable;
				case "LifeModule":
					return QualitySource.LifeModule;
				case "Built-In":
					return QualitySource.BuiltIn;
				default:
					return QualitySource.Selected;
			}
		}
		#endregion

		#region Constructor, Create, Save, Load, and Print Methods
		public Quality(Character objCharacter)
		{
			// Create the GUID for the new Quality.
			_guiID = Guid.NewGuid();
			_objCharacter = objCharacter;
		}

		/// <summary>
		/// Create a Quality from an XmlNode and return the TreeNodes for it.
		/// </summary>
		/// <param name="objXmlQuality">XmlNode to create the object from.</param>
		/// <param name="objCharacter">Character object the Quality will be added to.</param>
		/// <param name="objQualitySource">Source of the Quality.</param>
		/// <param name="objNode">TreeNode to populate a TreeView.</param>
		/// <param name="objWeapons">List of Weapons that should be added to the Character.</param>
		/// <param name="objWeaponNodes">List of TreeNodes to represent the Weapons added.</param>
		/// <param name="strForceValue">Force a value to be selected for the Quality.</param>
		public virtual void Create(XmlNode objXmlQuality, Character objCharacter, QualitySource objQualitySource, TreeNode objNode, List<Weapon> objWeapons, List<TreeNode> objWeaponNodes, string strForceValue = "")
		{
            objXmlQuality.TryGetStringFieldQuickly("name", ref _strName);
            objXmlQuality.TryGetStringFieldQuickly("metagenetic", ref _strMetagenetic);
            // Check for a Variable Cost.
            if (objXmlQuality["karma"] != null)
            {
                if (objXmlQuality["karma"].InnerText.StartsWith("Variable"))
                {
                    int intMin = 0;
                    int intMax = 0;
                    string strCost = objXmlQuality["karma"].InnerText.Replace("Variable(", string.Empty).Replace(")", string.Empty);
                    if (strCost.Contains("-"))
                    {
                        string[] strValues = strCost.Split('-');
                        intMin = Convert.ToInt32(strValues[0]);
                        intMax = Convert.ToInt32(strValues[1]);
                    }
                    else
                        intMin = Convert.ToInt32(strCost.Replace("+", string.Empty));

                    if (intMin != 0 || intMax != 0)
                    {
                        frmSelectNumber frmPickNumber = new frmSelectNumber();
                        if (intMax == 0)
                            intMax = 1000000;
                        frmPickNumber.Minimum = intMin;
                        frmPickNumber.Maximum = intMax;
                        frmPickNumber.Description = LanguageManager.Instance.GetString("String_SelectVariableCost").Replace("{0}", DisplayNameShort);
                        frmPickNumber.AllowCancel = false;
                        frmPickNumber.ShowDialog();
                        _intBP = frmPickNumber.SelectedValue;
                    }
                }
                else
                {
                    _intBP = Convert.ToInt32(objXmlQuality["karma"].InnerText);
                }
            }
            objXmlQuality.TryGetInt32FieldQuickly("lp", ref _intLP);
            if (objXmlQuality["category"] != null)
			    _objQualityType = ConvertToQualityType(objXmlQuality["category"].InnerText);
			_objQualitySource = objQualitySource;
			if (objXmlQuality["print"] != null)
			{
				if (objXmlQuality["print"].InnerText == "no")
					_blnPrint = false;
			}
			if (objXmlQuality["implemented"] != null)
			{
				if (objXmlQuality["implemented"].InnerText == "False")
					_blnImplemented = false;
			}
			if (objXmlQuality["contributetolimit"] != null)
			{
				if (objXmlQuality["contributetolimit"].InnerText == "no")
					_blnContributeToLimit = false;
			}
            objXmlQuality.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlQuality.TryGetStringFieldQuickly("page", ref _strPage);
			if (objXmlQuality["mutant"] != null)
				_strMutant = "yes";

			if (_objQualityType == QualityType.LifeModule)
			{
				objXmlQuality.TryGetStringFieldQuickly("stage", ref _stage);
			}

            if(objXmlQuality["id"] != null)
                _qualiyGuid = Guid.Parse(objXmlQuality["id"].InnerText);

			if (GlobalOptions.Instance.Language != "en-us")
			{
				XmlDocument objXmlDocument = XmlManager.Instance.Load("qualities.xml");
				XmlNode objQualityNode = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + _strName + "\"]");
				if (objQualityNode != null)
				{
                    objQualityNode.TryGetStringFieldQuickly("translate", ref _strAltName);
                    objQualityNode.TryGetStringFieldQuickly("altpage", ref _strAltPage);
				}
			}

			// Add Weapons if applicable.
			if (objXmlQuality.InnerXml.Contains("<addweapon>"))
			{
				XmlDocument objXmlWeaponDocument = XmlManager.Instance.Load("weapons.xml");

				// More than one Weapon can be added, so loop through all occurrences.
				foreach (XmlNode objXmlAddWeapon in objXmlQuality.SelectNodes("addweapon"))
				{
					XmlNode objXmlWeapon = objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"" + objXmlAddWeapon.InnerText + "\" and starts-with(category, \"Quality\")]");

					TreeNode objGearWeaponNode = new TreeNode();
					Weapon objGearWeapon = new Weapon(objCharacter);
					objGearWeapon.Create(objXmlWeapon, objCharacter, objGearWeaponNode, null, null);
					objGearWeaponNode.ForeColor = SystemColors.GrayText;
					objWeaponNodes.Add(objGearWeaponNode);
					objWeapons.Add(objGearWeapon);

					_guiWeaponID = Guid.Parse(objGearWeapon.InternalId);
				}
			}

			if (objXmlQuality.InnerXml.Contains("<naturalweapons>"))
            {
				foreach (XmlNode objXmlNaturalWeapon in objXmlQuality["naturalweapons"].SelectNodes("naturalweapon"))
				{
					TreeNode objGearWeaponNode = new TreeNode();
					Weapon objWeapon = new Weapon(_objCharacter);
                    if (objXmlNaturalWeapon["name"] != null)
					    objWeapon.Name = objXmlNaturalWeapon["name"].InnerText;
                    objWeapon.Category = LanguageManager.Instance.GetString("Tab_Critter");
					objWeapon.WeaponType = "Melee";
                    if (objXmlNaturalWeapon["reach"] != null)
                        objWeapon.Reach = Convert.ToInt32(objXmlNaturalWeapon["reach"].InnerText);
                    if (objXmlNaturalWeapon["accuracy"] != null)
                        objWeapon.Accuracy = objXmlNaturalWeapon["accuracy"].InnerText;
                    if (objXmlNaturalWeapon["damage"] != null)
                        objWeapon.Damage = objXmlNaturalWeapon["damage"].InnerText;
                    if (objXmlNaturalWeapon["ap"] != null)
                        objWeapon.AP = objXmlNaturalWeapon["ap"].InnerText; ;
					objWeapon.Mode = "0";
					objWeapon.RC = "0";
					objWeapon.Concealability = 0;
					objWeapon.Avail = "0";
					objWeapon.Cost = 0;
                    if (objXmlNaturalWeapon["useskill"] != null)
                        objWeapon.UseSkill = objXmlNaturalWeapon["useskill"].InnerText;
                    if (objXmlNaturalWeapon["source"] != null)
                        objWeapon.Source = objXmlNaturalWeapon["source"].InnerText;
                    if (objXmlNaturalWeapon["page"] != null)
                        objWeapon.Page = objXmlNaturalWeapon["page"].InnerText;
					objGearWeaponNode.ForeColor = SystemColors.GrayText;
					objGearWeaponNode.Text = objWeapon.Name;
					objGearWeaponNode.Tag = objWeapon.InternalId;
					objWeaponNodes.Add(objGearWeaponNode);

					_objCharacter.Weapons.Add(objWeapon);
				}
			}
			if (objXmlQuality.InnerXml.Contains("<costdiscount>"))
			{
				_nodDiscounts = objXmlQuality["costdiscount"];
			}
			// If the item grants a bonus, pass the information to the Improvement Manager.
			if (objXmlQuality.InnerXml.Contains("<bonus>"))
			{
				ImprovementManager objImprovementManager = new ImprovementManager(objCharacter);
				objImprovementManager.ForcedValue = strForceValue;
				if (!objImprovementManager.CreateImprovements(Improvement.ImprovementSource.Quality, _guiID.ToString(), objXmlQuality["bonus"], false, 1, DisplayNameShort))
				{
					_guiID = Guid.Empty;
					return;
				}
				if (!string.IsNullOrEmpty(objImprovementManager.SelectedValue))
				{
					_strExtra = objImprovementManager.SelectedValue;
					objNode.Text += " (" + objImprovementManager.SelectedValue + ")";
				}
			}

			// Metatype Qualities appear as grey text to show that they cannot be removed.
			if (objQualitySource == QualitySource.Metatype || objQualitySource == QualitySource.MetatypeRemovable)
				objNode.ForeColor = SystemColors.GrayText;

			objNode.Text = DisplayName;
			objNode.Tag = InternalId;
		}

		/// <summary>
		/// Save the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public virtual void Save(XmlTextWriter objWriter)
		{
			objWriter.WriteStartElement("quality");
			objWriter.WriteElementString("guid", _guiID.ToString());
			objWriter.WriteElementString("name", _strName);
			objWriter.WriteElementString("extra", _strExtra);
			objWriter.WriteElementString("bp", _intBP.ToString());
			objWriter.WriteElementString("implemented", _blnImplemented.ToString());
			objWriter.WriteElementString("contributetolimit", _blnContributeToLimit.ToString());
			if (_strMetagenetic != null)
			{
				objWriter.WriteElementString("metagenetic", _strMetagenetic);
			}
			objWriter.WriteElementString("print", _blnPrint.ToString());
			objWriter.WriteElementString("qualitytype", _objQualityType.ToString());
			objWriter.WriteElementString("qualitysource", _objQualitySource.ToString());
			if (!string.IsNullOrEmpty(_strMutant))
				objWriter.WriteElementString("mutant", _strMutant);
			objWriter.WriteElementString("source", _strSource);
			objWriter.WriteElementString("page", _strPage);
			if (_nodBonus != null)
				objWriter.WriteRaw("<bonus>" + _nodBonus.InnerXml + "</bonus>");
			else
				objWriter.WriteElementString("bonus", string.Empty);
			if (_guiWeaponID != Guid.Empty)
				objWriter.WriteElementString("weaponguid", _guiWeaponID.ToString());
			objWriter.WriteElementString("notes", _strNotes);
			if (_objQualityType == QualityType.LifeModule)
			{
				objWriter.WriteElementString("stage", _stage);
			}

			if (!_qualiyGuid.Equals(Guid.Empty))
			{
				objWriter.WriteElementString("id", _qualiyGuid.ToString());
			}

			objWriter.WriteEndElement();
			_objCharacter.SourceProcess(_strSource);
		}

		/// <summary>
		/// Load the CharacterAttribute from the XmlNode.
		/// </summary>
		/// <param name="objNode">XmlNode to load.</param>
		public virtual void Load(XmlNode objNode)
		{
            objNode.TryGetField("guid", Guid.TryParse, out _guiID);
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
            objNode.TryGetInt32FieldQuickly("bp", ref _intBP);
            objNode.TryGetBoolFieldQuickly("implemented", ref _blnImplemented);
            objNode.TryGetBoolFieldQuickly("contributetolimit", ref _blnContributeToLimit);
            objNode.TryGetBoolFieldQuickly("print", ref _blnPrint);
            if (objNode["qualitytype"] != null)
			    _objQualityType = ConvertToQualityType(objNode["qualitytype"].InnerText);
            if (objNode["qualitysource"] != null)
                _objQualitySource = ConvertToQualitySource(objNode["qualitysource"].InnerText);
			objNode.TryGetStringFieldQuickly("metagenetic", ref _strMetagenetic);
			objNode.TryGetStringFieldQuickly("mutant", ref _strMutant);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
			_nodBonus = objNode["bonus"];
			_nodDiscounts = objNode["costdiscount"];
            objNode.TryGetField("weaponguid", Guid.TryParse, out _guiWeaponID);
			objNode.TryGetStringFieldQuickly("notes", ref _strNotes);

			if (_objQualityType == QualityType.LifeModule)
			{
				objNode.TryGetStringFieldQuickly("stage", ref _stage);
			}
            objNode.TryGetField("id", Guid.TryParse, out _qualiyGuid);

			if (GlobalOptions.Instance.Language != "en-us")
			{
				XmlDocument objXmlDocument = XmlManager.Instance.Load("qualities.xml");
				XmlNode objQualityNode = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + _strName + "\"]");
				if (objQualityNode != null)
				{
                    objQualityNode.TryGetStringFieldQuickly("translate", ref _strAltName);
                    objQualityNode.TryGetStringFieldQuickly("altpage", ref _strAltPage);
				}
			}
		}

		/// <summary>
		/// Print the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public void Print(XmlTextWriter objWriter)
		{
			if (_blnPrint)
			{
				objWriter.WriteStartElement("quality");
				objWriter.WriteElementString("name", DisplayNameShort);
				objWriter.WriteElementString("extra", LanguageManager.Instance.TranslateExtra(_strExtra));
				objWriter.WriteElementString("bp", _intBP.ToString());
				string strQualityType = _objQualityType.ToString();
				if (GlobalOptions.Instance.Language != "en-us")
				{
					XmlDocument objXmlDocument = XmlManager.Instance.Load("qualities.xml");

					XmlNode objNode = objXmlDocument.SelectSingleNode("/chummer/categories/category[. = \"" + strQualityType + "\"]");
					if (objNode != null)
					{
						if (objNode.Attributes["translate"] != null)
							strQualityType = objNode.Attributes["translate"].InnerText;
					}
				}
				objWriter.WriteElementString("qualitytype", strQualityType);
				objWriter.WriteElementString("qualitytype_english", _objQualityType.ToString());
				objWriter.WriteElementString("qualitysource", _objQualitySource.ToString());
				objWriter.WriteElementString("source", _objCharacter.Options.LanguageBookShort(_strSource));
				objWriter.WriteElementString("page", Page);
				if (_objCharacter.Options.PrintNotes)
					objWriter.WriteElementString("notes", _strNotes);
				objWriter.WriteEndElement();
			}
		}
		#endregion

		#region Properties
		/// <summary>
		/// Internal identifier which will be used to identify this Quality in the Improvement system.
		/// </summary>
		public string InternalId
		{
			get
			{
				return _guiID.ToString();
			}
		}

        /// <summary>
        /// Internal identifier for the quality type
        /// </summary>
	    public string QualityId
	    {
	        get { return _qualiyGuid.ToString(); }
	    }

		/// <summary>
		/// Guid of a Weapon.
		/// </summary>
		public string WeaponID
		{
			get
			{
				return _guiWeaponID.ToString();
			}
			set
			{
				_guiWeaponID = Guid.Parse(value);
			}
		}

		/// <summary>
		/// Quality's name.
		/// </summary>
		public string Name
		{
			get
			{
				return _strName;
			}
			set
			{
				_strName = value;
			}
		}

		/// <summary>
		/// Extra information that should be applied to the name, like a linked CharacterAttribute.
		/// </summary>
		public string Extra
		{
			get
			{
				return _strExtra;
			}
			set
			{
				_strExtra = value;
			}
		}

		/// <summary>
		/// Sourcebook.
		/// </summary>
		public string Source
		{
			get
			{
				return _strSource;
			}
			set
			{
				_strSource = value;
			}
		}

		/// <summary>
		/// Page Number.
		/// </summary>
		public string Page
		{
			get
			{
				if (!string.IsNullOrEmpty(_strAltPage))
					return _strAltPage;

				return _strPage;
			}
			set
			{
				_strPage = value;
			}
		}

		/// <summary>
		/// Bonus node from the XML file.
		/// </summary>
		public XmlNode Bonus
		{
			get
			{
				return _nodBonus;
			}
			set
			{
				_nodBonus = value;
			}
		}

		/// <summary>
		/// Quality Type.
		/// </summary>
		public QualityType Type
		{
			get
			{
				return _objQualityType;
			}
			set
			{
				_objQualityType = value;
			}
		}

		/// <summary>
		/// Source of the Quality.
		/// </summary>
		public QualitySource OriginSource
		{
			get
			{
				return _objQualitySource;
			}
			set
			{
				_objQualitySource = value;
			}
		}

		/// <summary>
		/// Number of Build Points the Quality costs.
		/// </summary>
		public int BP
		{
			get { return CalculatedBP(); }
			set
			{
				_intBP = value;
			}
		}

		/// <summary>
        /// Number of Build Points the Quality costs.
        /// </summary>
        public int LP
        {
            get
            {
                return _intLP;
            }
            set
            {
                _intLP = value;
            }
        }
		/// <summary>
		/// The name of the object as it should be displayed on printouts (translated name only).
		/// </summary>
		public string DisplayNameShort
		{
			get
			{
				if (!string.IsNullOrEmpty(_strAltName))
					return _strAltName;

				return _strName;
			}
		}

		/// <summary>
		/// The name of the object as it should be displayed in lists. Name (Extra).
		/// </summary>
		public string DisplayName
		{
			get
			{
				string strReturn = DisplayNameShort;

				if (!string.IsNullOrEmpty(_strExtra))
				{
					LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
					// Attempt to retrieve the CharacterAttribute name.
					strReturn += " (" + LanguageManager.Instance.TranslateExtra(_strExtra) + ")";
				}
				return strReturn;
			}
		}

		/// <summary>
		/// Whether or not the Quality appears on the printouts.
		/// </summary>
		public bool AllowPrint
		{
			get
			{
				return _blnPrint;
			}
			set
			{
				_blnPrint = value;
			}
		}

		/// <summary>
		/// Whether or not the Quality has been implemented completely, or needs additional code support.
		/// </summary>
		public bool Implemented
		{
			get
			{
				return _blnImplemented;
			}
			set
			{
				_blnImplemented = value;
			}
		}
		/// <summary>
		/// Whether or not the Quality contributes towards the character's Quality BP limits.
		/// </summary>
		public bool ContributeToLimit
		{
			get
			{
				bool blnReturn = _blnContributeToLimit;

				if (!_blnContributeToLimit)
				{
					return false;
				}
				if (_objQualitySource == QualitySource.Metatype || _objQualitySource == QualitySource.MetatypeRemovable)
					return false;

				//Positive Metagenetic Qualities are free if you're a Changeling. 
				if (_strMetagenetic == "yes" && _objCharacter.MetageneticLimit > 0)
				{
					return false;
				}

				//The Beast's Way and the Spiritual Way get the Mentor Spirit for free.
				switch (_strName)
				{
					case "Mentor Spirit":
						if (_objCharacter.Qualities.Any(objQuality => objQuality.Name == "The Beast's Way" || objQuality.Name == "The Spiritual Way"))
							return false;
						break;
					case "High Pain Tolerance (Rating 1)":
						if (_objCharacter.Qualities.Any(objQuality => objQuality.Name == "Pie Iesu Domine. Dona Eis Requiem."))
						{
							return false;
						}
						break;
					default:
						return true;
				}
				return blnReturn;
			}
			set
			{
				_blnContributeToLimit = value;
			}
		}

		/// <summary>
		/// Whether or not the Quality contributes towards the character's Total BP.
		/// </summary>
		public bool ContributeToBP
		{
			get
			{
				if (_objQualitySource == QualitySource.Metatype || _objQualitySource == QualitySource.MetatypeRemovable)
					return false;

                    //Positive Metagenetic Qualities are free if you're a Changeling. 
                    if (_strMetagenetic == "yes" && _objCharacter.MetageneticLimit > 0)
                    {
	                    return false;
                    }

					//The Beast's Way and the Spiritual Way get the Mentor Spirit for free.
					switch (_strName)
                    {
	                    case "Mentor Spirit":
							if (_objCharacter.Qualities.Any(objQuality => objQuality.Name == "The Beast's Way" || objQuality.Name == "The Spiritual Way"))
								return false;
		                    break;
						case "High Pain Tolerance (Rating 1)":
							if (_objCharacter.Qualities.Any(objQuality => objQuality.Name == "Pie Iesu Domine. Dona Eis Requiem."))
							{
								return false;
							}
							break;
	                    default:
		                    return true;
                    }

				return true;
			}
		}

		/// <summary>
		/// Notes.
		/// </summary>
		public string Notes
		{
			get
			{
				return _strNotes;
			}
			set
			{
				_strNotes = value;
			}
		}

		/// <summary>
		/// Evaluates whether the Quality qualifies for any discounts/increases to its cost and returns the total cost.
		/// </summary>
		/// <returns></returns>
		private int CalculatedBP()
		{
			if (_nodDiscounts == null || !_nodDiscounts.HasChildNodes)
			{
				return _intBP;
			}
			int intReturn = _intBP;
			bool blnFound = false;
			foreach (XmlNode objNode in _nodDiscounts)
			{
				if (objNode.Name == "required")
				{
					if (objNode["oneof"].Cast<XmlNode>().Any(objRequiredNode => objRequiredNode.Name == "quality" && _objCharacter.Qualities.Any(objQuality => objQuality.Name == objRequiredNode.InnerText)))
					{
						blnFound = true;
						break;
                    }
                    if (objNode["oneof"].Cast<XmlNode>().Any(objRequiredNode => objRequiredNode.Name == "power" && _objCharacter.Powers.Any(objPower => objPower.Name == objRequiredNode.InnerText)))
                    {
                        blnFound = true;
                        break;
                    }
                }
			}
			if (blnFound)
			{
			    if (Type == QualityType.Positive)
			    {
			        intReturn += Convert.ToInt32(_nodDiscounts["value"]?.InnerText);
			    }
                else if (Type == QualityType.Negative)
                {
                    intReturn -= Convert.ToInt32(_nodDiscounts["value"]?.InnerText);
                }
            }
			return intReturn;
		}
		#endregion

		#region Static Methods

		/// <summary>
		/// Retuns weither a quality is valid on said Character
		/// THIS IS A WIP AND ONLY CHECKS QUALITIES. REQUIRED POWERS, METATYPES AND OTHERS WON'T BE CHECKED
		/// </summary>
		/// <param name="objCharacter">The Character</param>
		/// <param name="XmlQuality">The XmlNode describing the quality</param>
		/// <returns>Is the Quality valid on said Character</returns>
		public static bool IsValid(Character objCharacter, XmlNode objXmlQuality)
		{
			QualityFailureReason q;
			List<Quality> q2;
			return IsValid(objCharacter, objXmlQuality, out q, out q2);
		}

		/// <summary>
		/// Retuns weither a quality is valid on said Character
		/// THIS IS A WIP AND ONLY CHECKS QUALITIES. REQUIRED POWERS, METATYPES AND OTHERS WON'T BE CHECKED
		/// ConflictingQualities will only contain existing Qualities and won't contain required but missing Qualities
		/// </summary>
		/// <param name="objCharacter">The Character</param>
		/// <param name="objXmlQuality">The XmlNode describing the quality</param>
		/// <param name="reason">The reason the quality is not valid</param>
		/// <param name="conflictingQualities">List of Qualities that conflicts with this Quality</param>
		/// <returns>Is the Quality valid on said Character</returns>
		public static bool IsValid(Character objCharacter, XmlNode objXmlQuality, out QualityFailureReason reason, out List<Quality> conflictingQualities)
		{
			conflictingQualities = new List<Quality>();
			reason = QualityFailureReason.Allowed;
			//If limit are not present or no, check if same quality exists
		    string strTemp = string.Empty;
			if (!(objXmlQuality.TryGetStringFieldQuickly("limit", ref strTemp) && strTemp == "no"))
			{
				foreach (Quality objQuality in objCharacter.Qualities)
				{
					if (objQuality.QualityId == objXmlQuality["id"]?.InnerText)
					{
						reason |= QualityFailureReason.LimitExceeded; //QualityFailureReason is a flag enum, meaning each bit represents a different thing
						//So instead of changing it, |= adds rhs to list of reasons on lhs, if it is not present
						conflictingQualities.Add(objQuality);
					}
				}
			}

			if (objXmlQuality["required"] != null)
			{
				if (objXmlQuality["required"]["oneof"] != null)
				{
					if (objXmlQuality["required"]["oneof"]["quality"] != null)
					{
						XmlNodeList objXmlRequiredList = objXmlQuality.SelectNodes("required/oneof/quality");
						//Add to set for O(N log M) runtime instead of O(N * M)
						//like it matters...

						HashSet<String> qualityRequired = new HashSet<String>();
						foreach (XmlNode node in objXmlRequiredList)
						{
							qualityRequired.Add(node.InnerText);
						}

						bool blnFound = false;
						foreach (Quality quality in objCharacter.Qualities)
						{
							if (qualityRequired.Contains(quality.Name))
							{
								blnFound = true;
								break;
							}
						}

						if (!blnFound)
						{
							reason |= QualityFailureReason.RequiredSingle;
						}
					}

					if (objXmlQuality["required"]["oneof"]["metatype"] != null)
					{
						XmlNodeList objXmlRequiredList = objXmlQuality.SelectNodes("required/oneof/metatype");

						bool blnFound = false;
						foreach (XmlNode node in objXmlRequiredList)
						{
							if (node.InnerText == objCharacter.Metatype)
							{
								blnFound = true;
								break;
							}
						}

						if (!blnFound)
						{
							reason |= QualityFailureReason.MetatypeRequired;
						}
					}
                }
				if (objXmlQuality["required"]["allof"] != null)
				{
					XmlNodeList objXmlRequiredList = objXmlQuality.SelectNodes("required/allof/quality");
					//Add to set for O(N log M) runtime instead of O(N * M)


					HashSet<String> qualityRequired = new HashSet<String>();
					foreach (XmlNode node in objXmlRequiredList)
					{
						qualityRequired.Add(node.InnerText);
					}

					foreach (Quality quality in objCharacter.Qualities)
					{
						if (qualityRequired.Contains(quality.Name))
						{
							qualityRequired.Remove(quality.Name);
						}
					}

					if (qualityRequired.Count > 0)
					{
						reason |= QualityFailureReason.RequiredMultiple;
					}
				}
			}

			if (objXmlQuality["forbidden"] != null)
			{
				if (objXmlQuality["forbidden"]["oneof"] != null)
				{
					XmlNodeList objXmlRequiredList = objXmlQuality.SelectNodes("forbidden/oneof/quality");
					//Add to set for O(N log M) runtime instead of O(N * M)

					HashSet<String> qualityForbidden = new HashSet<String>();
					foreach (XmlNode node in objXmlRequiredList)
					{
						qualityForbidden.Add(node.InnerText);
					}

					foreach (Quality quality in objCharacter.Qualities)
					{
						if (qualityForbidden.Contains(quality.Name))
						{
							reason |= QualityFailureReason.ForbiddenSingle;
							conflictingQualities.Add(quality);
						}
					}
				}
			}
			
			return conflictingQualities.Count <= 0 & reason == QualityFailureReason.Allowed;
		}

		/// <summary>
		/// This method builds a xmlNode upwards adding/overriding elements
		/// </summary>
		/// <param name="id">ID of the node</param>
		/// <returns>A XmlNode containing the id and all nodes of its parrents</returns>
		public static XmlNode GetNodeOverrideable(string id)
		{
			XmlDocument xmlDocument = XmlManager.Instance.Load("lifemodules.xml");
			XmlNode node = xmlDocument.SelectSingleNode("//*[id = \"" + id + "\"]");
			return GetNodeOverrideable(node);
		}

		private static XmlNode GetNodeOverrideable(XmlNode n)
		{
			XmlNode workNode = n.Clone();  //clone as to not mess up the acctual xml document
            if (workNode != null)
            {
                XmlNode parentNode = n.SelectSingleNode("../..");
                if (parentNode != null && parentNode["id"] != null)
                {
                    XmlNode sourceNode = GetNodeOverrideable(parentNode);
                    if (sourceNode != null)
                    {
                        foreach (XmlNode node in sourceNode.ChildNodes)
                        {
                            if (workNode[node.LocalName] == null && node.LocalName != "versions")
                            {
                                workNode.AppendChild(node.Clone());
                            }
                            else if (node.LocalName == "bonus" && workNode["bonus"] != null)
                            {
                                foreach (XmlNode childNode in node.ChildNodes)
                                {
                                    workNode["bonus"].AppendChild(childNode.Clone());
                                }
                            }
                        }
                    }
                }
            }

			return workNode;
		}

		#endregion

		}

		/// <summary>
	/// Type of Spirit.
	/// </summary>
	public enum SpiritType
	{
		Spirit = 0,
		Sprite = 1,
	}

	/// <summary>
	/// A Magician's Spirit or Technomancer's Sprite.
	/// </summary>
	public class Spirit
	{
		private string _strName = string.Empty;
		private string _strCritterName = string.Empty;
		private int _intServicesOwed = 0;
		private SpiritType _objEntityType = SpiritType.Spirit;
		private bool _blnBound = true;
		private int _intForce = 1;
		private string _strFileName = string.Empty;
		private string _strRelativeName = string.Empty;
		private string _strNotes = string.Empty;
		private readonly Character _objCharacter;

		#region Helper Methods
		/// <summary>
		/// Convert a string to a SpiritType.
		/// </summary>
		/// <param name="strValue">String value to convert.</param>
		public SpiritType ConvertToSpiritType(string strValue)
		{
			switch (strValue)
			{
				case "Spirit":
					return SpiritType.Spirit;
				default:
					return SpiritType.Sprite;
			}
		}
		#endregion

		#region Constructor, Save, Load, and Print Methods
		public Spirit(Character objCharacter)
		{
			// Create the GUID for the new Spirit.
			_objCharacter = objCharacter;
		}

		/// <summary>
		/// Save the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public void Save(XmlTextWriter objWriter)
		{
			objWriter.WriteStartElement("spirit");
			objWriter.WriteElementString("name", _strName);
			objWriter.WriteElementString("crittername", _strCritterName);
			objWriter.WriteElementString("services", _intServicesOwed.ToString());
			objWriter.WriteElementString("force", _intForce.ToString());
			objWriter.WriteElementString("bound", _blnBound.ToString());
			objWriter.WriteElementString("type", _objEntityType.ToString());
			objWriter.WriteElementString("file", _strFileName);
			objWriter.WriteElementString("relative", _strRelativeName);
			objWriter.WriteElementString("notes", _strNotes);
			objWriter.WriteEndElement();
		}

		/// <summary>
		/// Load the Spirit from the XmlNode.
		/// </summary>
		/// <param name="objNode">XmlNode to load.</param>
		public void Load(XmlNode objNode)
		{
            if (objNode == null)
                return;
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            objNode.TryGetStringFieldQuickly("crittername", ref _strCritterName);
            objNode.TryGetInt32FieldQuickly("services", ref _intServicesOwed);
            objNode.TryGetInt32FieldQuickly("force", ref _intForce);
			objNode.TryGetBoolFieldQuickly("bound", ref _blnBound);
            if (objNode["type"] != null)
			    _objEntityType = ConvertToSpiritType(objNode["type"].InnerText);
			objNode.TryGetStringFieldQuickly("file", ref _strFileName);
			objNode.TryGetStringFieldQuickly("relative", ref _strRelativeName);
			objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
		}

		/// <summary>
		/// Print the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public void Print(XmlTextWriter objWriter)
		{
			// Translate the Critter name if applicable.
			string strName = _strName;
			XmlDocument objXmlDocument;
			if (_objEntityType == SpiritType.Spirit)
				objXmlDocument = XmlManager.Instance.Load("traditions.xml");
			else
				objXmlDocument = XmlManager.Instance.Load("streams.xml");
			XmlNode objXmlCritterNode = objXmlDocument.SelectSingleNode("/chummer/spirits/spirit[name = \"" + strName + "\"]");
			if (GlobalOptions.Instance.Language != "en-us")
			{
				if (objXmlCritterNode != null)
				{
					if (objXmlCritterNode["translate"] != null)
						strName = objXmlCritterNode["translate"].InnerText;
				}
			}



			objWriter.WriteStartElement("spirit");
			objWriter.WriteElementString("name", strName);
			objWriter.WriteElementString("crittername", _strCritterName);
			objWriter.WriteElementString("services", _intServicesOwed.ToString());
			objWriter.WriteElementString("force", _intForce.ToString());

			if (objXmlCritterNode != null)
			{
				//Attributes for spirits, named differently as to not confuse <attribtue>

				Dictionary<String, int> attributes = new Dictionary<string, int>();
				objWriter.WriteStartElement("spiritattributes");
				foreach (string Attribute in new String[] {"bod", "agi", "rea", "str", "cha", "int", "wil", "log", "ini"})
				{
					String strInner = string.Empty;
					if (objXmlCritterNode.TryGetStringFieldQuickly(Attribute, ref strInner))
					{
						//Here is some black magic (used way too many places)
						//To calculate the int value of a string
						//TODO: implement a sane expression evaluator

						XPathNavigator navigator = objXmlCritterNode.CreateNavigator();
						XPathExpression exp = navigator.Compile(strInner.Replace("F", _intForce.ToString()));
						int value;
						if (!int.TryParse(navigator.Evaluate(exp).ToString(), out value))
						{
							value = _intForce; //if failed to parse, default to force
						}
						value = Math.Max(value, 1); //Min value is 1
						objWriter.WriteElementString(Attribute, value.ToString());

						attributes[Attribute] = value;
					}
				}

				objWriter.WriteEndElement();

				//Dump skills, (optional)powers if present to output

				XmlDocument objXmlPowersDocument = XmlManager.Instance.Load("spiritpowers.xml");
				if (objXmlCritterNode["powers"] != null)
				{
					//objWriter.WriteRaw(objXmlCritterNode["powers"].OuterXml);
					objWriter.WriteStartElement("powers");
					foreach (XmlNode objXmlPowerNode in objXmlCritterNode["powers"].ChildNodes)
					{
						PrintPowerInfo(objWriter, objXmlPowersDocument, objXmlPowerNode.InnerText);
					}
					objWriter.WriteEndElement();
				}

				if (objXmlCritterNode["optionalpowers"] != null)
				{
					//objWriter.WriteRaw(objXmlCritterNode["optionalpowers"].OuterXml);
					objWriter.WriteStartElement("optionalpowers");
					foreach (XmlNode objXmlPowerNode in objXmlCritterNode["optionalpowers"].ChildNodes)
					{
						PrintPowerInfo(objWriter, objXmlPowersDocument, objXmlPowerNode.InnerText);
					}
					objWriter.WriteEndElement();
				}

				if (objXmlCritterNode["skills"] != null)
				{
					//objWriter.WriteRaw(objXmlCritterNode["skills"].OuterXml);
					objWriter.WriteStartElement("skills");
					foreach (XmlNode objXmlSkillNode in objXmlCritterNode["skills"].ChildNodes)
					{
                        string attrName = objXmlSkillNode.Attributes?["attr"]?.Value;
                        int attr;
                        if (!attributes.TryGetValue(attrName, out attr))
                            attr = _intForce;
                        int dicepool = attr + _intForce;

                        objWriter.WriteStartElement("skill");
						objWriter.WriteElementString("name", objXmlSkillNode.InnerText);
						objWriter.WriteElementString("attr", attrName);
						objWriter.WriteElementString("pool", dicepool.ToString());
						objWriter.WriteEndElement();
					}
					objWriter.WriteEndElement();
				}

				if (objXmlCritterNode["weaknesses"] != null)
				{
					objWriter.WriteRaw(objXmlCritterNode["weaknesses"].OuterXml);
				}

				//Page in book for reference
				String source = string.Empty;
				String page = string.Empty;

				if (objXmlCritterNode.TryGetStringFieldQuickly("source", ref source))
					objWriter.WriteElementString("source", source);
				if (objXmlCritterNode.TryGetStringFieldQuickly("page", ref page))
					objWriter.WriteElementString("page", page);
			}

			objWriter.WriteElementString("bound", _blnBound.ToString());
			objWriter.WriteElementString("type", _objEntityType.ToString());




			if (_objCharacter.Options.PrintNotes)
				objWriter.WriteElementString("notes", _strNotes);
			objWriter.WriteEndElement();
		}

		private void PrintPowerInfo(XmlTextWriter objWriter, XmlDocument objXmlDocument, string strPowerName)
		{
			string strSource = string.Empty;
			string strPage = string.Empty;
			XmlNode objXmlPowerNode = objXmlDocument.SelectSingleNode("/chummer/powers/power[name=\"" + strPowerName + "\"]");
			if (objXmlPowerNode == null)
				objXmlPowerNode = objXmlDocument.SelectSingleNode("/chummer/powers/power[starts-with(\"" + strPowerName + "\", name)]");
			if (objXmlPowerNode != null)
			{
				objXmlPowerNode.TryGetStringFieldQuickly("source", ref strSource);
				objXmlPowerNode.TryGetStringFieldQuickly("page", ref strPage);
			}

			objWriter.WriteStartElement("power");
			objWriter.WriteElementString("name", strPowerName);
			objWriter.WriteElementString("source", strSource);
			objWriter.WriteElementString("page", strPage);
			objWriter.WriteEndElement();
		}
		#endregion

		#region Properties
		/// <summary>
		/// The Character object being used by the Spirit.
		/// </summary>
		public Character CharacterObject
		{
			get
			{
				return _objCharacter;
			}
		}

		/// <summary>
		/// Name of the Spirit's Metatype.
		/// </summary>
		public string Name
		{
			get
			{
				return _strName;
			}
			set
			{
				_strName = value;
			}
		}

		/// <summary>
		/// Name of the Spirit.
		/// </summary>
		public string CritterName
		{
			get
			{
				return _strCritterName;
			}
			set
			{
				_strCritterName = value;
			}
		}

		/// <summary>
		/// Number of Services the Spirit owes.
		/// </summary>
		public int ServicesOwed
		{
			get
			{
				return _intServicesOwed;
			}
			set
			{
				_intServicesOwed = value;
			}
		}

		/// <summary>
		/// The Spirit's Force.
		/// </summary>
		public int Force
		{
			get
			{
				return _intForce;
			}
			set
			{
				_intForce = value;
			}
		}

		/// <summary>
		/// Whether or not the Spirit is Bound.
		/// </summary>
		public bool Bound
		{
			get
			{
				return _blnBound;
			}
			set
			{
				_blnBound = value;
			}
		}

		/// <summary>
		/// The Spirit's type, either Spirit or Sprite.
		/// </summary>
		public SpiritType EntityType
		{
			get
			{
				return _objEntityType;
			}
			set
			{
				_objEntityType = value;
			}
		}

		/// <summary>
		/// Name of the save file for this Spirit/Sprite.
		/// </summary>
		public string FileName
		{
			get
			{
				return _strFileName;
			}
			set
			{
				_strFileName = value;
			}
		}

		/// <summary>
		/// Relative path to the save file.
		/// </summary>
		public string RelativeFileName
		{
			get
			{
				return _strRelativeName;
			}
			set
			{
				_strRelativeName = value;
			}
		}

		/// <summary>
		/// Notes.
		/// </summary>
		public string Notes
		{
			get
			{
				return _strNotes;
			}
			set
			{
				_strNotes = value;
			}
		}
		#endregion
	}

	/// <summary>
	/// A Magician Spell.
	/// </summary>
	public class Spell : INamedItemWithGuid
	{
		private Guid _guiID = new Guid();
		private string _strName = string.Empty;
		private string _strDescriptors = string.Empty;
		private string _strCategory = string.Empty;
		private string _strType = string.Empty;
		private string _strRange = string.Empty;
		private string _strDamage = string.Empty;
		private string _strDuration = string.Empty;
		private string _strDV = string.Empty;
		private string _strSource = string.Empty;
		private string _strPage = string.Empty;
		private string _strExtra = string.Empty;
		private bool _blnLimited = false;
		private bool _blnExtended = false;
		private string _strNotes = string.Empty;
		private readonly Character _objCharacter;
		private string _strAltName = string.Empty;
		private string _strAltCategory = string.Empty;
		private string _strAltPage = string.Empty;
        private bool _blnAlchemical = false;
        private int _intGrade = 0;
        private Improvement.ImprovementSource _objImprovementSource = Improvement.ImprovementSource.Spell;

		#region Constructor, Create, Save, Load, and Print Methods
		public Spell(Character objCharacter)
		{
			// Create the GUID for the new Spell.
			_guiID = Guid.NewGuid();
			_objCharacter = objCharacter;
		}

		/// Create a Spell from an XmlNode and return the TreeNodes for it.
		/// <param name="objXmlSpellNode">XmlNode to create the object from.</param>
		/// <param name="objCharacter">Character the Gear is being added to.</param>
		/// <param name="objNode">TreeNode to populate a TreeView.</param>
		/// <param name="strForcedValue">Value to forcefully select for any ImprovementManager prompts.</param>
		/// <param name="blnLimited">Whether or not the Spell should be marked as Limited.</param>
		/// <param name="blnExtended">Whether or not the Spell should be marked as Extended.</param>
        public void Create(XmlNode objXmlSpellNode, Character objCharacter, TreeNode objNode, string strForcedValue = "", bool blnLimited = false, bool blnExtended = false, bool blnAlchemical = false, Improvement.ImprovementSource objSource = Improvement.ImprovementSource.Spell)
		{
            objXmlSpellNode.TryGetStringFieldQuickly("name", ref _strName);
            _blnExtended = blnExtended;
            if (GlobalOptions.Instance.Language != "en-us")
            {
                XmlDocument objXmlDocument = XmlManager.Instance.Load("spells.xml");
                XmlNode objSpellNode = objXmlDocument.SelectSingleNode("/chummer/spells/spell[name = \"" + _strName + "\"]");
                if (objSpellNode != null)
                {
                    objSpellNode.TryGetStringFieldQuickly("translate", ref _strAltName);
                    objSpellNode.TryGetStringFieldQuickly("altpage", ref _strAltPage);
                }

                objSpellNode = objXmlDocument.SelectSingleNode("/chummer/categories/category[. = \"" + _strCategory + "\"]");
                if (objSpellNode != null)
                {
                    if (objSpellNode.Attributes["translate"] != null)
                        _strAltCategory = objSpellNode.Attributes["translate"].InnerText;
                }
            }

            ImprovementManager objImprovementManager = new ImprovementManager(objCharacter);
            objImprovementManager.ForcedValue = strForcedValue;
            if (objXmlSpellNode["bonus"] != null)
            {
                if (!objImprovementManager.CreateImprovements(Improvement.ImprovementSource.Spell, _guiID.ToString(), objXmlSpellNode["bonus"], false, 1, DisplayNameShort))
                {
                    _guiID = Guid.Empty;
                    return;
                }
                if (!string.IsNullOrEmpty(objImprovementManager.SelectedValue))
                {
                    _strExtra = objImprovementManager.SelectedValue;
                }
            }
		    
            objXmlSpellNode.TryGetStringFieldQuickly("descriptor", ref _strDescriptors);
            objXmlSpellNode.TryGetStringFieldQuickly("category", ref _strCategory);
            objXmlSpellNode.TryGetStringFieldQuickly("type", ref _strType);
            objXmlSpellNode.TryGetStringFieldQuickly("range", ref _strRange);
            objXmlSpellNode.TryGetStringFieldQuickly("damage", ref _strDamage);
            objXmlSpellNode.TryGetStringFieldQuickly("duration", ref _strDuration);
            objXmlSpellNode.TryGetStringFieldQuickly("dv", ref _strDV);
			_blnLimited = blnLimited;
            _blnAlchemical = blnAlchemical;
            objXmlSpellNode.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlSpellNode.TryGetStringFieldQuickly("page", ref _strPage);
            _objImprovementSource = objSource;

            if (_blnLimited && _strDV.StartsWith("F"))
            {
                string strDV = _strDV;
                int intPos = 0;
                if (strDV.Contains("-"))
                {
                    intPos = strDV.IndexOf('-') + 1;
                    string strAfter = strDV.Substring(intPos, strDV.Length - intPos);
                    strDV = strDV.Substring(0, intPos);
                    int intAfter;
                    int.TryParse(strAfter, out intAfter);
                    intAfter += 2;
                    strDV += intAfter.ToString();
                }
                else if (strDV.Contains("+"))
                {
                    intPos = strDV.IndexOf('+');
                    string strAfter = strDV.Substring(intPos, strDV.Length - intPos);
                    strDV = strDV.Substring(0, intPos);
                    int intAfter;
                    int.TryParse(strAfter, out intAfter);
                    intAfter -= 2;
                    if (intAfter > 0)
                        strDV += "+" + intAfter.ToString();
                    else if (intAfter < 0)
                        strDV += intAfter.ToString();
                }
                else
                {
                    strDV += "-2";
                }
                _strDV = strDV;
            }

			//TreeNode objNode = new TreeNode();
			objNode.Text = DisplayName;
			objNode.Tag = _guiID.ToString();

			//return objNode;
		}

		/// <summary>
		/// Save the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public void Save(XmlTextWriter objWriter)
		{
			objWriter.WriteStartElement("spell");
			objWriter.WriteElementString("guid", _guiID.ToString());
			objWriter.WriteElementString("name", _strName);
			objWriter.WriteElementString("descriptors", _strDescriptors);
			objWriter.WriteElementString("category", _strCategory);
			objWriter.WriteElementString("type", _strType);
			objWriter.WriteElementString("range", _strRange);
			objWriter.WriteElementString("damage", _strDamage);
			objWriter.WriteElementString("duration", _strDuration);
			objWriter.WriteElementString("dv", _strDV);
			objWriter.WriteElementString("limited", _blnLimited.ToString());
			objWriter.WriteElementString("extended", _blnExtended.ToString());
            objWriter.WriteElementString("alchemical", _blnAlchemical.ToString());
            objWriter.WriteElementString("source", _strSource);
			objWriter.WriteElementString("page", _strPage);
			objWriter.WriteElementString("extra", _strExtra);
			objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteElementString("improvementsource", _objImprovementSource.ToString());
            objWriter.WriteElementString("grade", _intGrade.ToString());
            objWriter.WriteEndElement();
			_objCharacter.SourceProcess(_strSource);
		}

		/// <summary>
		/// Load the Spell from the XmlNode.
		/// </summary>
		/// <param name="objNode">XmlNode to load.</param>
		public void Load(XmlNode objNode)
		{
            Improvement objImprovement = new Improvement();
            objNode.TryGetField("guid", Guid.TryParse, out _guiID);
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            objNode.TryGetStringFieldQuickly("descriptors", ref _strDescriptors);
            objNode.TryGetStringFieldQuickly("category", ref _strCategory);
            objNode.TryGetStringFieldQuickly("type", ref _strType);
            objNode.TryGetStringFieldQuickly("range", ref _strRange);
            objNode.TryGetStringFieldQuickly("damage", ref _strDamage);
            objNode.TryGetStringFieldQuickly("duration", ref _strDuration);
            if (objNode["improvementsource"] != null)
            {
                _objImprovementSource = objImprovement.ConvertToImprovementSource(objNode["improvementsource"].InnerText);
            }
            objNode.TryGetInt32FieldQuickly("grade", ref _intGrade);
            objNode.TryGetStringFieldQuickly("dv", ref _strDV);
            objNode.TryGetBoolFieldQuickly("limited", ref _blnLimited);
            objNode.TryGetBoolFieldQuickly("extended", ref _blnExtended);
            objNode.TryGetBoolFieldQuickly("alchemical", ref _blnAlchemical);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strSource);

            objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);

			if (GlobalOptions.Instance.Language != "en-us")
			{
				XmlDocument objXmlDocument = XmlManager.Instance.Load("spells.xml");
				XmlNode objSpellNode = objXmlDocument.SelectSingleNode("/chummer/spells/spell[name = \"" + _strName + "\"]");
				if (objSpellNode != null)
				{
                    objSpellNode.TryGetStringFieldQuickly("translate", ref _strAltName);
                    objSpellNode.TryGetStringFieldQuickly("altpage", ref _strAltPage);
				}

				objSpellNode = objXmlDocument.SelectSingleNode("/chummer/categories/category[. = \"" + _strCategory + "\"]");
				if (objSpellNode != null)
				{
					if (objSpellNode.Attributes["translate"] != null)
						_strAltCategory = objSpellNode.Attributes["translate"].InnerText;
				}
			}
		}

		/// <summary>
		/// Print the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public void Print(XmlTextWriter objWriter)
		{
			objWriter.WriteStartElement("spell");
			if (_blnLimited)
				objWriter.WriteElementString("name", DisplayNameShort + " (" + LanguageManager.Instance.GetString("String_SpellLimited") + ")");
			else if (_blnAlchemical)
                objWriter.WriteElementString("name", DisplayNameShort + " (" + LanguageManager.Instance.GetString("String_SpellAlchemical") + ")");
            else
				objWriter.WriteElementString("name", DisplayNameShort);
			objWriter.WriteElementString("descriptors", DisplayDescriptors);
			objWriter.WriteElementString("category", DisplayCategory);
			objWriter.WriteElementString("type", DisplayType);
			objWriter.WriteElementString("range", DisplayRange);
			objWriter.WriteElementString("damage", DisplayDamage);
			objWriter.WriteElementString("duration", DisplayDuration);
			objWriter.WriteElementString("dv", DisplayDV);
		    objWriter.WriteElementString("alchemy", Alchemical.ToString());
		    objWriter.WriteElementString("dicepool", DicePool.ToString());
			objWriter.WriteElementString("source", _objCharacter.Options.LanguageBookShort(_strSource));
			objWriter.WriteElementString("page", Page);
			objWriter.WriteElementString("extra", LanguageManager.Instance.TranslateExtra(_strExtra));
			if (_objCharacter.Options.PrintNotes)
				objWriter.WriteElementString("notes", _strNotes);
			objWriter.WriteEndElement();
		}
		#endregion

		#region Properties
		/// <summary>
		/// Internal identifier which will be used to identify this Spell in the Improvement system.
		/// </summary>
		public string InternalId
		{
			get
			{
				return _guiID.ToString();
			}
		}

		/// <summary>
		/// Spell's name.
		/// </summary>
		public string Name
		{
			get
			{
				return _strName;
			}
			set
			{
				_strName = value;
			}
		}

        /// <summary>
        /// Spell's grade.
        /// </summary>
        public int Grade
        {
            get
            {
                return _intGrade;
            }
            set
            {
                _intGrade = value;
            }
        }

        /// <summary>
		/// Spell's descriptors.
		/// </summary>
		public string Descriptors
		{
			get
			{
				return _strDescriptors;
			}
			set
			{
				_strDescriptors = value;
			}
		}

		/// <summary>
		/// Translated Descriptors.
		/// </summary>
		public string DisplayDescriptors
		{
			get
			{
				string strReturn = string.Empty;
				bool blnExtendedFound = false;

				string[] strDescriptorsIn = _strDescriptors.Split(',');
				foreach (string strDescriptor in strDescriptorsIn)
				{
					switch (strDescriptor.Trim())
					{
                        case "Active":
                            strReturn += LanguageManager.Instance.GetString("String_DescActive") + ", ";
                            break;
                        case "Adept":
                            strReturn += LanguageManager.Instance.GetString("String_DescAdept") + ", ";
                            break;
                        case "Alchemical Preparation":
                            strReturn += LanguageManager.Instance.GetString("String_DescAlchemicalPreparation") + ", ";
                            break;
                        case "Anchored":
                            strReturn += LanguageManager.Instance.GetString("String_DescAnchored") + ", ";
                            break;
                        case "Area":
                            strReturn += LanguageManager.Instance.GetString("String_DescArea") + ", ";
                            break;
                        case "Blood":
                            strReturn += LanguageManager.Instance.GetString("String_DescBlood") + ", ";
                            break;
                        case "Contractual":
                            strReturn += LanguageManager.Instance.GetString("String_DescContractual") + ", ";
                            break;
                        case "Direct":
                            strReturn += LanguageManager.Instance.GetString("String_DescDirect") + ", ";
                            break;
                        case "Directional":
                            strReturn += LanguageManager.Instance.GetString("String_DescDirectional") + ", ";
                            break;
                        case "Elemental":
                            strReturn += LanguageManager.Instance.GetString("String_DescElemental") + ", ";
                            break;
                        case "Environmental":
                            strReturn += LanguageManager.Instance.GetString("String_DescEnvironmental") + ", ";
                            break;
                        case "Geomancy":
                            strReturn += LanguageManager.Instance.GetString("String_DescGeomancy") + ", ";
                            break;
                        case "Indirect":
                            strReturn += LanguageManager.Instance.GetString("String_DescIndirect") + ", ";
                            break;
                        case "Mana":
                            strReturn += LanguageManager.Instance.GetString("String_DescMana") + ", ";
                            break;
                        case "Material Link":
                            strReturn += LanguageManager.Instance.GetString("String_DescMaterialLink") + ", ";
                            break;
                        case "Mental":
                            strReturn += LanguageManager.Instance.GetString("String_DescMental") + ", ";
                            break;
                        case "Minion":
                            strReturn += LanguageManager.Instance.GetString("String_DescMinion") + ", ";
                            break;
                        case "Multi-Sense":
                            strReturn += LanguageManager.Instance.GetString("String_DescMultiSense") + ", ";
                            break;
                        case "Negative":
                            strReturn += LanguageManager.Instance.GetString("String_DescNegative") + ", ";
                            break;
                        case "Obvious":
                            strReturn += LanguageManager.Instance.GetString("String_DescObvious") + ", ";
                            break;
                        case "Organic Link":
                            strReturn += LanguageManager.Instance.GetString("String_DescOrganicLink") + ", ";
                            break;
                        case "Passive":
                            strReturn += LanguageManager.Instance.GetString("String_DescPassive") + ", ";
                            break;
                        case "Physical":
                            strReturn += LanguageManager.Instance.GetString("String_DescPhysical") + ", ";
                            break;
                        case "Psychic":
                            strReturn += LanguageManager.Instance.GetString("String_DescPsychic") + ", ";
                            break;
                        case "Realistic":
                            strReturn += LanguageManager.Instance.GetString("String_DescRealistic") + ", ";
                            break;
                        case "Single-Sense":
                            strReturn += LanguageManager.Instance.GetString("String_DescSingleSense") + ", ";
                            break;
                        case "Touch":
                            strReturn += LanguageManager.Instance.GetString("String_DescTouch") + ", ";
                            break;
                        case "Spell":
                            strReturn += LanguageManager.Instance.GetString("String_DescSpell") + ", ";
                            break;
                        case "Spotter":
                            strReturn += LanguageManager.Instance.GetString("String_DescSpotter") + ", ";
                            break;
                    }
				}

				// If Extended Area was not found and the Extended flag is enabled, add Extended Area to the list of Descriptors.
				if (_blnExtended && !blnExtendedFound)
					strReturn += LanguageManager.Instance.GetString("String_DescExtendedArea") + ", ";

				// Remove the trailing comma.
				if (!string.IsNullOrEmpty(strReturn))
					strReturn = strReturn.Substring(0, strReturn.Length - 2);

				return strReturn;
			}
		}

		/// <summary>
		/// Translated Category.
		/// </summary>
		public string DisplayCategory
		{
			get
			{
				if (!string.IsNullOrEmpty(_strAltCategory))
					return _strAltCategory;

				return _strCategory;
			}
		}

		/// <summary>
		/// Spell's category.
		/// </summary>
		public string Category
		{
			get
			{
				return _strCategory;
			}
			set
			{
				_strCategory = value;
			}
		}

		/// <summary>
		/// Spell's type.
		/// </summary>
		public string Type
		{
			get
			{
				return _strType;
			}
			set
			{
				_strType = value;
			}
		}

		/// <summary>
		/// Translated Type.
		/// </summary>
		public string DisplayType
		{
			get
			{
				string strReturn = string.Empty;

				switch (_strType)
				{
					case "M":
						strReturn = LanguageManager.Instance.GetString("String_SpellTypeMana");
						break;
					default:
						strReturn = LanguageManager.Instance.GetString("String_SpellTypePhysical");
						break;
				}

				return strReturn;
			}
		}

		/// <summary>
		/// Translated Drain Value.
		/// </summary>
		public string DisplayDV
		{
			get
			{
				string strReturn = _strDV.Replace('/', '÷');
				strReturn = strReturn.Replace("F", LanguageManager.Instance.GetString("String_SpellForce"));
				strReturn = strReturn.Replace("Overflow damage", LanguageManager.Instance.GetString("String_SpellOverflowDamage"));
				strReturn = strReturn.Replace("Damage Value", LanguageManager.Instance.GetString("String_SpellDamageValue"));
				strReturn = strReturn.Replace("Toxin DV", LanguageManager.Instance.GetString("String_SpellToxinDV"));
				strReturn = strReturn.Replace("Disease DV", LanguageManager.Instance.GetString("String_SpellDiseaseDV"));
				strReturn = strReturn.Replace("Radiation Power", LanguageManager.Instance.GetString("String_SpellRadiationPower"));

                //if (_blnExtended)
                //{
                //    // Add +2 to the DV value if Extended is selected.
                //    int intPos = strReturn.IndexOf(')') + 1;
                //    string strAfter = strReturn.Substring(intPos, strReturn.Length - intPos);
                //    strReturn = strReturn.Remove(intPos, strReturn.Length - intPos);
                //    if (string.IsNullOrEmpty(strAfter))
                //        strAfter = "+2";
                //    else
                //    {
                //        int intValue = Convert.ToInt32(strAfter) + 2;
                //        if (intValue == 0)
                //            strAfter = string.Empty;
                //        else if (intValue > 0)
                //            strAfter = "+" + intValue.ToString();
                //        else
                //            strAfter = intValue.ToString();
                //    }
                //    strReturn += strAfter;
                //}

                return strReturn;
			}
		}

		/// <summary>
		/// Drain Tooltip.
		/// </summary>
		public string DVTooltip
		{
			get
			{
				string strTip = LanguageManager.Instance.GetString("Tip_SpellDrainBase");
				int intMAG = _objCharacter.MAG.TotalValue;

				if (_objCharacter.AdeptEnabled && _objCharacter.MagicianEnabled)
				{
					if (_objCharacter.Options.SpiritForceBasedOnTotalMAG)
						intMAG = _objCharacter.MAG.TotalValue;
					else
						intMAG = _objCharacter.MAG.Value;
				}

				XmlDocument objXmlDocument = new XmlDocument();
				XPathNavigator nav = objXmlDocument.CreateNavigator();
				XPathExpression xprDV;
                object xprResult = null;

                for (int i = 1; i <= intMAG * 2; i++)
                {
                    // Calculate the Spell's Drain for the current Force.
                    xprDV = nav.Compile(_strDV.Replace("F", i.ToString()).Replace("/", " div "));
                    
                    try
                    {
                        xprResult = nav.Evaluate(xprDV);
                    }
                    catch (XPathException)
                    {
                        xprResult = null;
                    }
                    if (xprResult != null)
                    {
                        int intDV = Convert.ToInt32(Math.Floor(Convert.ToDouble(xprResult.ToString())));
                        // Drain cannot be lower than 2.
                        if (intDV < 2)
                            intDV = 2;
                        strTip += "\n   " + LanguageManager.Instance.GetString("String_Force") + " " + i.ToString() + ": " + intDV.ToString();
                    }
                    else
                    {
                        strTip = LanguageManager.Instance.GetString("Tip_SpellDrainSeeDescription");
                        break;
                    }
                }

				return strTip;
			}
		}

		/// <summary>
		/// Spell's range.
		/// </summary>
		public string Range
		{
			get
			{
				return _strRange;
			}
			set
			{
				_strRange = value;
			}
		}

		/// <summary>
		/// Translated Range.
		/// </summary>
		public string DisplayRange
		{
			get
			{
				string strReturn = _strRange;
				strReturn = strReturn.Replace("Self", LanguageManager.Instance.GetString("String_SpellRangeSelf"));
				strReturn = strReturn.Replace("LOS", LanguageManager.Instance.GetString("String_SpellRangeLineOfSight"));
				strReturn = strReturn.Replace("LOI", LanguageManager.Instance.GetString("String_SpellRangeLineOfInfluence"));
				strReturn = strReturn.Replace("T", LanguageManager.Instance.GetString("String_SpellRangeTouch"));
				strReturn = strReturn.Replace("(A)", "(" + LanguageManager.Instance.GetString("String_SpellRangeArea") + ")");
				strReturn = strReturn.Replace("MAG", LanguageManager.Instance.GetString("String_AttributeMAGShort"));

				return strReturn;
			}
		}

		/// <summary>
		/// Spell's damage.
		/// </summary>
		public string Damage
		{
			get
			{
				return _strDamage;
			}
			set
			{
				_strDamage = value;
			}
		}

		/// <summary>
		/// Translated Damage.
		/// </summary>
		public string DisplayDamage
		{
			get
			{
				string strReturn = string.Empty;

				switch (_strDamage)
				{
					case "P":
						strReturn = LanguageManager.Instance.GetString("String_DamagePhysical");
						break;
					case "S":
						strReturn = LanguageManager.Instance.GetString("String_DamageStun");
						break;
					default:
						strReturn = string.Empty;
						break;
				}

				return strReturn;
			}
		}

		/// <summary>
		/// Spell's duration.
		/// </summary>
		public string Duration
		{
			get
			{
				return _strDuration;
			}
			set
			{
				_strDuration = value;
			}
		}

		/// <summary>
		/// Translated Duration.
		/// </summary>
		public string DisplayDuration
		{
			get
			{
				string strReturn = string.Empty;

				switch (_strDuration)
				{
					case "P":
						strReturn = LanguageManager.Instance.GetString("String_SpellDurationPermanent");
						break;
					case "S":
						strReturn = LanguageManager.Instance.GetString("String_SpellDurationSustained");
						break;
					default:
						strReturn = LanguageManager.Instance.GetString("String_SpellDurationInstant");
						break;
				}

				return strReturn;
			}
		}

		/// <summary>
		/// Spell's drain value.
		/// </summary>
		public string DV
		{
			get
			{
				return _strDV;
			}
			set
			{
				_strDV = value;
			}
		}

		/// <summary>
		/// Spell's Source.
		/// </summary>
		public string Source
		{
			get
			{
				return _strSource;
			}
			set
			{
				_strSource = value;
			}
		}

		/// <summary>
		/// Sourcebook Page Number.
		/// </summary>
		public string Page
		{
			get
			{
				if (!string.IsNullOrEmpty(_strAltPage))
					return _strAltPage;

				return _strPage;
			}
			set
			{
				_strPage = value;
			}
		}

		/// <summary>
		/// Extra information from Improvement dialogues.
		/// </summary>
		public string Extra
		{
			get
			{
				return _strExtra;
			}
			set
			{
				_strExtra = value;
			}
		}

		/// <summary>
		/// Whether or not the Spell is Limited.
		/// </summary>
		public bool Limited
		{
			get
			{
				return _blnLimited;
			}
			set
			{
				_blnLimited = value;
			}
		}

		/// <summary>
		/// Whether or not the Spell is Extended.
		/// </summary>
		public bool Extended
		{
			get
			{
				return _blnExtended;
			}
			set
			{
				_blnExtended = value;
			}
		}

        /// <summary>
        /// Whether or not the Spell is Alchemical.
        /// </summary>
        public bool Alchemical
        {
            get
            {
                return _blnAlchemical;
            }
            set
            {
                _blnAlchemical = value;
            }
        }

        /// <summary>
		/// Notes.
		/// </summary>
		public string Notes
		{
			get
			{
				return _strNotes;
			}
			set
			{
				_strNotes = value;
			}
		}

		/// <summary>
		/// The name of the object as it should be displayed on printouts (translated name only).
		/// </summary>
		public string DisplayNameShort
		{
			get
			{
				string strReturn = _strName;
				if (!string.IsNullOrEmpty(_strAltName))
					strReturn = _strAltName;

				if (_blnExtended)
					strReturn += ", " + LanguageManager.Instance.GetString("String_SpellExtended");

				return strReturn;
			}
		}

		/// <summary>
		/// The name of the object as it should be displayed in lists.
		/// </summary>
		public string DisplayName
		{
			get
			{
				LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);

				string strReturn = DisplayNameShort;

				if (_blnLimited)
					strReturn += " (" + LanguageManager.Instance.GetString("String_SpellLimited") + ")";
                if (_blnAlchemical)
                    strReturn += " (" + LanguageManager.Instance.GetString("String_SpellAlchemical") + ")";
                if (!string.IsNullOrEmpty(_strExtra))
				{
					// Attempt to retrieve the CharacterAttribute name.
					strReturn += " (" + LanguageManager.Instance.TranslateExtra(_strExtra) + ")";
				}
				return strReturn;
			}
		}
		#endregion

		#region ComplexProperties
		/// <summary>
		/// The Dice Pool size for the Active Skill required to cast the Spell.
		/// </summary>
		public int DicePool
		{
			get
			{
				int intReturn = 0;
				foreach (Skill objSkill in _objCharacter.SkillsSection.Skills)
				{
                    if (objSkill.Name == "Spellcasting" && !_blnAlchemical && _strCategory != "Rituals" && _strCategory != "Enchantments")
					{
						intReturn = objSkill.Pool;
						// Add any Specialization bonus if applicable.
						if (objSkill.HasSpecialization(_strCategory))
							intReturn += 2;
					}
                    if (objSkill.Name == "Ritual Spellcasting" && !_blnAlchemical && _strCategory == "Rituals")
                    {
                        intReturn = objSkill.Pool;
                        // Add any Specialization bonus if applicable.
                        if (objSkill.HasSpecialization(_strCategory))
                            intReturn += 2;
                    }
                    if (objSkill.Name == "Artificing" && !_blnAlchemical && _strCategory == "Enchantments")
                    {
                        intReturn = objSkill.Pool;
                        // Add any Specialization bonus if applicable.
                        if (objSkill.HasSpecialization(_strCategory))
                            intReturn += 2;
                    }
                    if (objSkill.Name == "Alchemy" && _blnAlchemical)
                    {
                        intReturn = objSkill.Pool;
                        // Add any Specialization bonus if applicable.
                        if (objSkill.HasSpecialization(_strCategory))
                            intReturn += 2;
                    }
                }

				// Include any Improvements to the Spell Category.
				ImprovementManager objImprovementManager = new ImprovementManager(_objCharacter);
				intReturn += objImprovementManager.ValueOf(Improvement.ImprovementType.SpellCategory, false, _strCategory);

				return intReturn;
			}
		}

		/// <summary>
		/// Tooltip information for the Dice Pool.
		/// </summary>
		public string DicePoolTooltip
		{
			get
			{
				string strReturn = string.Empty;

				foreach (Skill objSkill in _objCharacter.SkillsSection.Skills)
				{
                    if (objSkill.Name == "Spellcasting" && !_blnAlchemical && _strCategory != "Rituals" && _strCategory != "Enchantments")
                    {
                        strReturn = objSkill.GetDisplayName() + " (" + objSkill.Pool.ToString() + ")";
                        // Add any Specialization bonus if applicable.
                        if (objSkill.HasSpecialization(_strCategory))
                            strReturn += " + " + LanguageManager.Instance.GetString("String_ExpenseSpecialization") + ": " + DisplayCategory + " (2)";
                    }
                    if (objSkill.Name == "Ritual Spellcasting" && !_blnAlchemical && _strCategory == "Rituals")
                    {
                        strReturn = objSkill.GetDisplayName() + " (" + objSkill.Pool.ToString() + ")";
                        // Add any Specialization bonus if applicable.
                        if (objSkill.HasSpecialization(_strCategory))
                            strReturn += " + " + LanguageManager.Instance.GetString("String_ExpenseSpecialization") + ": " + DisplayCategory + " (2)";
                    }
                    if (objSkill.Name == "Artificing" && !_blnAlchemical && _strCategory == "Enchantments")
                    {
                        strReturn = objSkill.GetDisplayName() + " (" + objSkill.Pool.ToString() + ")";
                        // Add any Specialization bonus if applicable.
                        if (objSkill.HasSpecialization(_strCategory))
                            strReturn += " + " + LanguageManager.Instance.GetString("String_ExpenseSpecialization") + ": " + DisplayCategory + " (2)";
                    }
                    if (objSkill.Name == "Alchemy" && _blnAlchemical)
                    {
                        strReturn = objSkill.GetDisplayName() + " (" + objSkill.Pool.ToString() + ")";
                        // Add any Specialization bonus if applicable.
                        if (objSkill.HasSpecialization(_strCategory))
                            strReturn += " + " + LanguageManager.Instance.GetString("String_ExpenseSpecialization") + ": " + DisplayCategory + " (2)";
                    }
                }

				// Include any Improvements to the Spell Category.
				ImprovementManager objImprovementManager = new ImprovementManager(_objCharacter);
				if (objImprovementManager.ValueOf(Improvement.ImprovementType.SpellCategory, false, _strCategory) != 0)
					strReturn += " + " + DisplayCategory + " (" + objImprovementManager.ValueOf(Improvement.ImprovementType.SpellCategory, false, _strCategory).ToString() + ")";

				return strReturn;
			}
		}
		#endregion
	}

	/// <summary>
	/// A Focus.
	/// </summary>
	public class Focus : INamedItemWithGuid
	{
		private Guid _guiID = new Guid();
		private string _strName = string.Empty;
		private Guid _guiGearId = new Guid();
		private int _intRating = 0;

		#region Constructor, Create, Save, and Load Methods
		public Focus()
		{
			// Create the GUID for the new Focus.
			_guiID = Guid.NewGuid();
		}

		/// <summary>
		/// Save the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public void Save(XmlTextWriter objWriter)
		{
			objWriter.WriteStartElement("focus");
			objWriter.WriteElementString("guid", _guiID.ToString());
			objWriter.WriteElementString("name", _strName);
			objWriter.WriteElementString("gearid", _guiGearId.ToString());
			objWriter.WriteElementString("rating", _intRating.ToString());
			objWriter.WriteEndElement();
		}

		/// <summary>
		/// Load the Focus from the XmlNode.
		/// </summary>
		/// <param name="objNode">XmlNode to load.</param>
		public void Load(XmlNode objNode)
		{
			_guiID = Guid.Parse(objNode["guid"].InnerText);
			_strName = objNode["name"].InnerText;
			_intRating = Convert.ToInt32(objNode["rating"].InnerText);
			_guiGearId = Guid.Parse(objNode["gearid"].InnerText);
		}
		#endregion

		#region Properties
		/// <summary>
		/// Internal identifier which will be used to identify this Focus in the Improvement system.
		/// </summary>
		public string InternalId
		{
			get
			{
				return _guiID.ToString();
			}
		}

		/// <summary>
		/// Foci's name.
		/// </summary>
		public string Name
		{
			get
			{
				return _strName;
			}
			set
			{
				_strName = value;
			}
		}

		/// <summary>
		/// GUID of the linked Gear.
		/// </summary>
		public string GearId
		{
			get
			{
				return _guiGearId.ToString();
			}
			set
			{
				_guiGearId = Guid.Parse(value);
			}
		}

		/// <summary>
		/// Rating of the Foci.
		/// </summary>
		public int Rating
		{
			get
			{
				return _intRating;
			}
			set
			{
				_intRating = value;
			}
		}
		#endregion
	}

	/// <summary>
	/// A Stacked Focus.
	/// </summary>
	public class StackedFocus
	{
		private Guid _guiID = new Guid();
		private bool _blnBonded = false;
		private Guid _guiGearId = new Guid();
		private List<Gear> _lstGear = new List<Gear>();
		private readonly Character _objCharacter;

		#region Constructor, Create, Save, and Load Methods
		public StackedFocus(Character objCharacter)
		{
			// Create the GUID for the new Focus.
			_guiID = Guid.NewGuid();
			_objCharacter = objCharacter;
		}

		/// <summary>
		/// Save the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public void Save(XmlTextWriter objWriter)
		{
			objWriter.WriteStartElement("stackedfocus");
			objWriter.WriteElementString("guid", _guiID.ToString());
			objWriter.WriteElementString("gearid", _guiGearId.ToString());
			objWriter.WriteElementString("bonded", _blnBonded.ToString());
			objWriter.WriteStartElement("gears");
			foreach (Gear objGear in _lstGear)
				objGear.Save(objWriter);
			objWriter.WriteEndElement();
			objWriter.WriteEndElement();
		}

		/// <summary>
		/// Load the Stacked Focus from the XmlNode.
		/// </summary>
		/// <param name="objNode">XmlNode to load.</param>
		public void Load(XmlNode objNode)
		{
			_guiID = Guid.Parse(objNode["guid"].InnerText);
			_guiGearId = Guid.Parse(objNode["gearid"].InnerText);
			_blnBonded = Convert.ToBoolean(objNode["bonded"].InnerText);
			XmlNodeList nodGears = objNode.SelectNodes("gears/gear");
			foreach (XmlNode nodGear in nodGears)
			{
				Gear objGear = new Gear(_objCharacter);
				objGear.Load(nodGear);
				_lstGear.Add(objGear);
			}
		}
		#endregion

		#region Properties
		/// <summary>
		/// Internal identifier which will be used to identify this Stacked Focus in the Improvement system.
		/// </summary>
		public string InternalId
		{
			get
			{
				return _guiID.ToString();
			}
		}

		/// <summary>
		/// GUID of the linked Gear.
		/// </summary>
		public string GearId
		{
			get
			{
				return _guiGearId.ToString();
			}
			set
			{
				_guiGearId = Guid.Parse(value);
			}
		}

		/// <summary>
		/// Whether or not the Stacked Focus in Bonded.
		/// </summary>
		public bool Bonded
		{
			get
			{
				return _blnBonded;
			}
			set
			{
				_blnBonded = value;
			}
		}

		/// <summary>
		/// The Stacked Focus' total Force.
		/// </summary>
		public int TotalForce
		{
			get
			{
				int intReturn = 0;
				foreach (Gear objGear in _lstGear)
					intReturn += objGear.Rating;

				return intReturn;
			}
		}

		/// <summary>
		/// The cost in Karma to bind this Stacked Focus.
		/// </summary>
		public int BindingCost
		{
			get
			{
				int intCost = 0;
				foreach (Gear objFocus in _lstGear)
				{
					// Each Focus costs an amount of Karma equal to their Force x speicific Karma cost.
					string strFocusName = objFocus.Name;
					int intPosition = strFocusName.IndexOf('(');
					if (intPosition > -1)
						strFocusName = strFocusName.Substring(0, intPosition - 1);
					int intKarmaMultiplier = 0;
					switch (strFocusName)
					{
						case "Qi Focus":
							intKarmaMultiplier = _objCharacter.Options.KarmaQiFocus;
							break;
						case "Sustaining Focus":
							intKarmaMultiplier = _objCharacter.Options.KarmaSustainingFocus;
							break;
						case "Counterspelling Focus":
							intKarmaMultiplier = _objCharacter.Options.KarmaCounterspellingFocus;
							break;
						case "Banishing Focus":
							intKarmaMultiplier = _objCharacter.Options.KarmaBanishingFocus;
							break;
						case "Binding Focus":
							intKarmaMultiplier = _objCharacter.Options.KarmaBindingFocus;
							break;
						case "Weapon Focus":
							intKarmaMultiplier = _objCharacter.Options.KarmaWeaponFocus;
							break;
						case "Spellcasting Focus":
							intKarmaMultiplier = _objCharacter.Options.KarmaSpellcastingFocus;
							break;
						case "Summoning Focus":
							intKarmaMultiplier = _objCharacter.Options.KarmaSummoningFocus;
							break;
						case "Alchemical Focus":
							intKarmaMultiplier = _objCharacter.Options.KarmaAlchemicalFocus;
							break;
						case "Centering Focus":
							intKarmaMultiplier = _objCharacter.Options.KarmaCenteringFocus;
							break;
						case "Masking Focus":
							intKarmaMultiplier = _objCharacter.Options.KarmaMaskingFocus;
							break;
						case "Disenchanting Focus":
							intKarmaMultiplier = _objCharacter.Options.KarmaDisenchantingFocus;
							break;
						case "Power Focus":
							intKarmaMultiplier = _objCharacter.Options.KarmaPowerFocus;
							break;
						case "Flexible Signature Focus":
							intKarmaMultiplier = _objCharacter.Options.KarmaFlexibleSignatureFocus;
							break;
						case "Ritual Spellcasting Focus":
							intKarmaMultiplier = _objCharacter.Options.KarmaRitualSpellcastingFocus;
							break;
						case "Spell Shaping Focus":
							intKarmaMultiplier = _objCharacter.Options.KarmaSpellShapingFocus;
							break;
						default:
							intKarmaMultiplier = 1;
							break;
					}
					intCost += (objFocus.Rating * intKarmaMultiplier);
				}
				return intCost;
			}
		}

		/// <summary>
		/// Stacked Focus Name.
		/// </summary>
		public string Name
		{
			get
			{
				string strReturn = string.Empty;
				foreach (Gear objGear in _lstGear)
					strReturn += objGear.DisplayName + ", ";

				// Remove the trailing comma.
				if (!string.IsNullOrEmpty(strReturn))
					strReturn = strReturn.Substring(0, strReturn.Length - 2);

				return strReturn;
			}
		}

		/// <summary>
		/// List of Gear that make up the Stacked Focus.
		/// </summary>
		public List<Gear> Gear
		{
			get
			{
				return _lstGear;
			}
			set
			{
				_lstGear = value;
			}
		}
		#endregion
	}

	/// <summary>
	/// A Metamagic or Echo.
	/// </summary>
	public class Metamagic : INamedItemWithGuid
	{
		private Guid _guiID = new Guid();
		private string _strName = string.Empty;
		private string _strSource = string.Empty;
		private string _strPage = string.Empty;
		private bool _blnPaidWithKarma = false;
        private int _intGrade = 0;
		private XmlNode _nodBonus;
		private Improvement.ImprovementSource _objImprovementSource = Improvement.ImprovementSource.Metamagic;
		private string _strNotes = string.Empty;

		private readonly Character _objCharacter;

		#region Constructor, Create, Save, Load, and Print Methods
		public Metamagic(Character objCharacter)
		{
			// Create the GUID for the new piece of Cyberware.
			_guiID = Guid.NewGuid();
			_objCharacter = objCharacter;
		}

		/// Create a Metamagic from an XmlNode and return the TreeNodes for it.
		/// <param name="objXmlMetamagicNode">XmlNode to create the object from.</param>
		/// <param name="objCharacter">Character the Gear is being added to.</param>
		/// <param name="objNode">TreeNode to populate a TreeView.</param>
		/// <param name="objSource">Source of the Improvement.</param>
		public void Create(XmlNode objXmlMetamagicNode, Character objCharacter, TreeNode objNode, Improvement.ImprovementSource objSource)
		{
            objXmlMetamagicNode.TryGetStringFieldQuickly("name", ref _strName);
            objXmlMetamagicNode.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlMetamagicNode.TryGetStringFieldQuickly("page", ref _strPage);
			_objImprovementSource = objSource;
            objXmlMetamagicNode.TryGetInt32FieldQuickly("grade", ref _intGrade);
            _nodBonus = objXmlMetamagicNode["bonus"];
            if (_nodBonus != null)
			{
				int intRating = 1;
				if (_objCharacter.SubmersionGrade > 0)
					intRating = _objCharacter.SubmersionGrade;
				else
					intRating = _objCharacter.InitiateGrade;

				ImprovementManager objImprovementManager = new ImprovementManager(objCharacter);
				if (!objImprovementManager.CreateImprovements(objSource, _guiID.ToString(), _nodBonus, true, intRating, DisplayNameShort))
				{
					_guiID = Guid.Empty;
					return;
				}
				if (!string.IsNullOrEmpty(objImprovementManager.SelectedValue))
					_strName += " (" + objImprovementManager.SelectedValue + ")";
			}

            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, null);

            if (_objCharacter.SubmersionGrade > 0)
                objNode.Text = LanguageManager.Instance.GetString("Label_Echo") + " " + DisplayName;
            else
                objNode.Text = LanguageManager.Instance.GetString("Label_Metamagic") + " " + DisplayName;
            objNode.Tag = _guiID.ToString();
		}

		/// <summary>
		/// Save the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public void Save(XmlTextWriter objWriter)
		{
			objWriter.WriteStartElement("metamagic");
			objWriter.WriteElementString("guid", _guiID.ToString());
			objWriter.WriteElementString("name", _strName);
			objWriter.WriteElementString("source", _strSource);
			objWriter.WriteElementString("paidwithkarma", _blnPaidWithKarma.ToString());
			objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("grade", _intGrade.ToString());
			if (_nodBonus != null)
				objWriter.WriteRaw(_nodBonus.OuterXml);
			else
				objWriter.WriteElementString("bonus", string.Empty);
			objWriter.WriteElementString("improvementsource", _objImprovementSource.ToString());
			objWriter.WriteElementString("notes", _strNotes);
			objWriter.WriteEndElement();
			_objCharacter.SourceProcess(_strSource);
		}

		/// <summary>
		/// Load the Metamagic from the XmlNode.
		/// </summary>
		/// <param name="objNode">XmlNode to load.</param>
		public void Load(XmlNode objNode)
		{
			Improvement objImprovement = new Improvement();
            objNode.TryGetField("guid", Guid.TryParse, out _guiID);
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetBoolFieldQuickly("paidwithkarma", ref _blnPaidWithKarma);
            objNode.TryGetInt32FieldQuickly("grade", ref _intGrade);

			_nodBonus = objNode["bonus"];
            if (objNode["improvementsource"] != null)
    			_objImprovementSource = objImprovement.ConvertToImprovementSource(objNode["improvementsource"].InnerText);

            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
		}

		/// <summary>
		/// Print the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public void Print(XmlTextWriter objWriter)
		{
			objWriter.WriteStartElement("metamagic");
			objWriter.WriteElementString("name", DisplayNameShort);
			objWriter.WriteElementString("source", _objCharacter.Options.LanguageBookShort(_strSource));
			objWriter.WriteElementString("page", Page);
            objWriter.WriteElementString("grade", _intGrade.ToString());
            objWriter.WriteElementString("improvementsource", _objImprovementSource.ToString());
			if (_objCharacter.Options.PrintNotes)
				objWriter.WriteElementString("notes", _strNotes);
			objWriter.WriteEndElement();
		}
		#endregion

		#region Properties
		/// <summary>
		/// Internal identifier which will be used to identify this Metamagic in the Improvement system.
		/// </summary>
		public string InternalId
		{
			get
			{
				return _guiID.ToString();
			}
		}

		/// <summary>
		/// Bonus node from the XML file.
		/// </summary>
		public XmlNode Bonus
		{
			get
			{
				return _nodBonus;
			}
			set
			{
				_nodBonus = value;
			}
		}

		/// <summary>
		/// ImprovementSource Type.
		/// </summary>
		public Improvement.ImprovementSource SourceType
		{
			get
			{
				return _objImprovementSource;
			}
			set
			{
				_objImprovementSource = value;
			}
		}

		/// <summary>
		/// Metamagic name.
		/// </summary>
		public string Name
		{
			get
			{
				return _strName;
			}
			set
			{
				_strName = value;
			}
		}

		/// <summary>
		/// The name of the object as it should be displayed on printouts (translated name only).
		/// </summary>
		public string DisplayNameShort
		{
			get
			{
				string strReturn = _strName;
				// Get the translated name if applicable.
				if (GlobalOptions.Instance.Language != "en-us")
				{
					string strXmlFile = string.Empty;
					string strXPath = string.Empty;
					if (_objImprovementSource == Improvement.ImprovementSource.Metamagic)
					{
						strXmlFile = "metamagic.xml";
						strXPath = "/chummer/metamagics/metamagic";
					}
					else
					{
						strXmlFile = "echoes.xml";
						strXPath = "/chummer/echoes/echo";
					}
					XmlDocument objXmlDocument = XmlManager.Instance.Load(strXmlFile);
					XmlNode objNode = objXmlDocument.SelectSingleNode(strXPath + "[name = \"" + _strName + "\"]");
					if (objNode != null)
					{
						if (objNode["translate"] != null)
							strReturn = objNode["translate"].InnerText;
					}
				}

				return strReturn;
			}
		}

		/// <summary>
		/// The name of the object as it should be displayed in lists. Name (Extra).
		/// </summary>
		public string DisplayName
		{
			get
			{
				string strReturn = DisplayNameShort;

				return strReturn;
			}
		}

        /// <summary>
        /// Grade.
        /// </summary>
        public Int32 Grade
        {
            get
            {
                return _intGrade;
            }
            set
            {
                _intGrade = value;
            }
        }

        /// <summary>
		/// Sourcebook.
		/// </summary>
		public string Source
		{
			get
			{
				return _strSource;
			}
			set
			{
				_strSource = value;
			}
		}

		/// <summary>
		/// Sourcebook Page Number.
		/// </summary>
		public string Page
		{
			get
			{
				string strReturn = _strPage;
				// Get the translated name if applicable.
				if (GlobalOptions.Instance.Language != "en-us")
				{
					string strXmlFile = string.Empty;
					string strXPath = string.Empty;
					if (_objImprovementSource == Improvement.ImprovementSource.Metamagic)
					{
						strXmlFile = "metamagic.xml";
						strXPath = "/chummer/metamagics/metamagic";
					}
					else
					{
						strXmlFile = "echoes.xml";
						strXPath = "/chummer/echoes/echo";
					}
					XmlDocument objXmlDocument = XmlManager.Instance.Load(strXmlFile);
					XmlNode objNode = objXmlDocument.SelectSingleNode(strXPath + "[name = \"" + _strName + "\"]");
					if (objNode != null)
					{
						if (objNode["altpage"] != null)
							strReturn = objNode["altpage"].InnerText;
					}
				}

				return strReturn;
			}
			set
			{
				_strPage = value;
			}
		}

		/// <summary>
		/// Whether or not the Metamagic was paid for with Karma.
		/// </summary>
		public bool PaidWithKarma
		{
			get
			{
				return _blnPaidWithKarma;
			}
			set
			{
				_blnPaidWithKarma = value;
			}
		}

		/// <summary>
		/// Notes.
		/// </summary>
		public string Notes
		{
			get
			{
				return _strNotes;
			}
			set
			{
				_strNotes = value;
			}
		}
		#endregion
	}

    /// <summary>
    /// An Art.
    /// </summary>
    public class Art : INamedItemWithGuid
    {
        private Guid _guiID = new Guid();
        private string _strName = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private XmlNode _nodBonus;
        private int _intGrade = 0;
        private Improvement.ImprovementSource _objImprovementSource = Improvement.ImprovementSource.Art;
        private string _strNotes = string.Empty;

        private readonly Character _objCharacter;

        #region Constructor, Create, Save, Load, and Print Methods
        public Art(Character objCharacter)
        {
            // Create the GUID for the new art.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// Create an Art from an XmlNode and return the TreeNodes for it.
        /// <param name="objXmlArtNode">XmlNode to create the object from.</param>
        /// <param name="objCharacter">Character the Gear is being added to.</param>
        /// <param name="objNode">TreeNode to populate a TreeView.</param>
        /// <param name="objSource">Source of the Improvement.</param>
        public void Create(XmlNode objXmlArtNode, Character objCharacter, TreeNode objNode, Improvement.ImprovementSource objSource)
        {
            objXmlArtNode.TryGetStringFieldQuickly("name", ref _strName);
            objXmlArtNode.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlArtNode.TryGetStringFieldQuickly("page", ref _strPage);
            _objImprovementSource = objSource;
            objXmlArtNode.TryGetInt32FieldQuickly("grade", ref _intGrade);
            _nodBonus = objXmlArtNode["bonus"];
            if (_nodBonus != null)
            {
                ImprovementManager objImprovementManager = new ImprovementManager(objCharacter);
                if (!objImprovementManager.CreateImprovements(objSource, _guiID.ToString(), _nodBonus, true, 1, DisplayNameShort))
                {
                    _guiID = Guid.Empty;
                    return;
                }
                if (!string.IsNullOrEmpty(objImprovementManager.SelectedValue))
                    _strName += " (" + objImprovementManager.SelectedValue + ")";
            }

            objNode.Text = LanguageManager.Instance.GetString("Label_Art") + " " + DisplayName;
            objNode.Tag = _guiID.ToString();
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("art");
            objWriter.WriteElementString("guid", _guiID.ToString());
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("grade", _intGrade.ToString());
            if (_nodBonus != null)
                objWriter.WriteRaw(_nodBonus.OuterXml);
            else
                objWriter.WriteElementString("bonus", string.Empty);
            objWriter.WriteElementString("improvementsource", _objImprovementSource.ToString());
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteEndElement();
			_objCharacter.SourceProcess(_strSource);
		}

        /// <summary>
        /// Load the Metamagic from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            Improvement objImprovement = new Improvement();
            objNode.TryGetField("guid", Guid.TryParse, out _guiID);
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            _nodBonus = objNode["bonus"];
            if (objNode["improvementsource"] != null)
                _objImprovementSource = objImprovement.ConvertToImprovementSource(objNode["improvementsource"].InnerText);

            objNode.TryGetInt32FieldQuickly("grade", ref _intGrade);
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Print(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("art");
            objWriter.WriteElementString("name", DisplayNameShort);
            objWriter.WriteElementString("source", _objCharacter.Options.LanguageBookShort(_strSource));
            objWriter.WriteElementString("page", Page);
            objWriter.WriteElementString("improvementsource", _objImprovementSource.ToString());
            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteEndElement();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Internal identifier which will be used to identify this Metamagic in the Improvement system.
        /// </summary>
        public string InternalId
        {
            get
            {
                return _guiID.ToString();
            }
        }

        /// <summary>
        /// Bonus node from the XML file.
        /// </summary>
        public XmlNode Bonus
        {
            get
            {
                return _nodBonus;
            }
            set
            {
                _nodBonus = value;
            }
        }

        /// <summary>
        /// ImprovementSource Type.
        /// </summary>
        public Improvement.ImprovementSource SourceType
        {
            get
            {
                return _objImprovementSource;
            }
            set
            {
                _objImprovementSource = value;
            }
        }

        /// <summary>
        /// Metamagic name.
        /// </summary>
        public string Name
        {
            get
            {
                return _strName;
            }
            set
            {
                _strName = value;
            }
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort
        {
            get
            {
                string strReturn = _strName;
                // Get the translated name if applicable.
                if (GlobalOptions.Instance.Language != "en-us")
                {
                    string strXmlFile = "metamagic.xml";
                    string strXPath = "/chummer/arts/art";
                    XmlDocument objXmlDocument = XmlManager.Instance.Load(strXmlFile);
                    XmlNode objNode = objXmlDocument.SelectSingleNode(strXPath + "[name = \"" + _strName + "\"]");
                    if (objNode != null)
                    {
                        if (objNode["translate"] != null)
                            strReturn = objNode["translate"].InnerText;
                    }
                }

                return strReturn;
            }
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Name (Extra).
        /// </summary>
        public string DisplayName
        {
            get
            {
                string strReturn = DisplayNameShort;

                return strReturn;
            }
        }

        /// <summary>
        /// The initiate grade where the art was learned.
        /// </summary>
        public int Grade
        {
            get
            {
                return _intGrade;
            }
            set
            {
                _intGrade = value;
            }
        }

        /// <summary>
        /// Sourcebook.
        /// </summary>
        public string Source
        {
            get
            {
                return _strSource;
            }
            set
            {
                _strSource = value;
            }
        }

        /// <summary>
        /// Sourcebook Page Number.
        /// </summary>
        public string Page
        {
            get
            {
                string strReturn = _strPage;
                // Get the translated name if applicable.
                if (GlobalOptions.Instance.Language != "en-us")
                {
                    string strXmlFile = "metamagic.xml";
                    string strXPath = "/chummer/metamagics/metamagic";
                    XmlDocument objXmlDocument = XmlManager.Instance.Load(strXmlFile);
                    XmlNode objNode = objXmlDocument.SelectSingleNode(strXPath + "[name = \"" + _strName + "\"]");
                    if (objNode != null)
                    {
                        if (objNode["altpage"] != null)
                            strReturn = objNode["altpage"].InnerText;
                    }
                }

                return strReturn;
            }
            set
            {
                _strPage = value;
            }
        }

        /// <summary>
        /// Notes.
        /// </summary>
        public string Notes
        {
            get
            {
                return _strNotes;
            }
            set
            {
                _strNotes = value;
            }
        }
        #endregion
    }

    /// <summary>
    /// An Enhancement.
    /// </summary>
    public class Enhancement : INamedItemWithGuid
    {
        private Guid _guiID = new Guid();
        private string _strName = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private XmlNode _nodBonus;
        private int _intGrade = 0;
        private Improvement.ImprovementSource _objImprovementSource = Improvement.ImprovementSource.Enhancement;
        private string _strNotes = string.Empty;
        private Power _objParent;

        private readonly Character _objCharacter;

        #region Constructor, Create, Save, Load, and Print Methods
        public Enhancement(Character objCharacter)
        {
            // Create the GUID for the new art.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// Create an Enhancement from an XmlNode and return the TreeNodes for it.
        /// <param name="objXmlEnhancementNode">XmlNode to create the object from.</param>
        /// <param name="objCharacter">Character the Enhancement is being added to.</param>
        /// <param name="objNode">TreeNode to populate a TreeView.</param>
        /// <param name="objSource">Source of the Improvement.</param>
        public void Create(XmlNode objXmlArtNode, Character objCharacter, TreeNode objNode, Improvement.ImprovementSource objSource)
        {
            objXmlArtNode.TryGetStringFieldQuickly("name", ref _strName);
            objXmlArtNode.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlArtNode.TryGetStringFieldQuickly("page", ref _strPage);
            _objImprovementSource = objSource;
            objXmlArtNode.TryGetInt32FieldQuickly("grade", ref _intGrade);
            _nodBonus = objXmlArtNode["bonus"];
            if (_nodBonus != null)
            {
                ImprovementManager objImprovementManager = new ImprovementManager(objCharacter);
                if (!objImprovementManager.CreateImprovements(objSource, _guiID.ToString(), _nodBonus, true, 1, DisplayNameShort))
                {
                    _guiID = Guid.Empty;
                    return;
                }
                if (!string.IsNullOrEmpty(objImprovementManager.SelectedValue))
                    _strName += " (" + objImprovementManager.SelectedValue + ")";
            }

            objNode.Text = LanguageManager.Instance.GetString("Label_Enhancement") + " " + DisplayName;
            objNode.Tag = _guiID.ToString();
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("enhancement");
            objWriter.WriteElementString("guid", _guiID.ToString());
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("grade", _intGrade.ToString());
            if (_nodBonus != null)
                objWriter.WriteRaw(_nodBonus.OuterXml);
            else
                objWriter.WriteElementString("bonus", string.Empty);
            objWriter.WriteElementString("improvementsource", _objImprovementSource.ToString());
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteEndElement();
			_objCharacter.SourceProcess(_strSource);
		}

        /// <summary>
        /// Load the Enhancement from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            Improvement objImprovement = new Improvement();
            objNode.TryGetField("guid", Guid.TryParse, out _guiID);
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            _nodBonus = objNode["bonus"];
            if (objNode["improvementsource"] != null)
                _objImprovementSource = objImprovement.ConvertToImprovementSource(objNode["improvementsource"].InnerText);

            objNode.TryGetInt32FieldQuickly("grade", ref _intGrade);
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Print(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("enhancement");
            objWriter.WriteElementString("name", DisplayNameShort);
            objWriter.WriteElementString("source", _objCharacter.Options.LanguageBookShort(_strSource));
            objWriter.WriteElementString("page", Page);
            objWriter.WriteElementString("improvementsource", _objImprovementSource.ToString());
            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteEndElement();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Internal identifier which will be used to identify this Metamagic in the Improvement system.
        /// </summary>
        public string InternalId
        {
            get
            {
                return _guiID.ToString();
            }
        }

        /// <summary>
        /// Bonus node from the XML file.
        /// </summary>
        public XmlNode Bonus
        {
            get
            {
                return _nodBonus;
            }
            set
            {
                _nodBonus = value;
            }
        }

        /// <summary>
        /// ImprovementSource Type.
        /// </summary>
        public Improvement.ImprovementSource SourceType
        {
            get
            {
                return _objImprovementSource;
            }
            set
            {
                _objImprovementSource = value;
            }
        }

        /// <summary>
        /// Metamagic name.
        /// </summary>
        public string Name
        {
            get
            {
                return _strName;
            }
            set
            {
                _strName = value;
            }
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort
        {
            get
            {
                string strReturn = _strName;
                // Get the translated name if applicable.
                if (GlobalOptions.Instance.Language != "en-us")
                {
                    string strXmlFile = "powers.xml";
                    string strXPath = "/chummer/enhancements/enhancement";
                    XmlDocument objXmlDocument = XmlManager.Instance.Load(strXmlFile);
                    XmlNode objNode = objXmlDocument.SelectSingleNode(strXPath + "[name = \"" + _strName + "\"]");
                    if (objNode != null)
                    {
                        if (objNode["translate"] != null)
                            strReturn = objNode["translate"].InnerText;
                    }
                }

                return strReturn;
            }
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Name (Extra).
        /// </summary>
        public string DisplayName
        {
            get
            {
                string strReturn = DisplayNameShort;

                return strReturn;
            }
        }

        /// <summary>
        /// The initiate grade where the enhancement was learned.
        /// </summary>
        public int Grade
        {
            get
            {
                return _intGrade;
            }
            set
            {
                _intGrade = value;
            }
        }

        /// <summary>
        /// Sourcebook.
        /// </summary>
        public string Source
        {
            get
            {
                return _strSource;
            }
            set
            {
                _strSource = value;
            }
        }

        /// <summary>
        /// Sourcebook Page Number.
        /// </summary>
        public string Page
        {
            get
            {
                string strReturn = _strPage;
                // Get the translated name if applicable.
                if (GlobalOptions.Instance.Language != "en-us")
                {
                    string strXmlFile = "metamagic.xml";
                    string strXPath = "/chummer/metamagics/metamagic";
                    XmlDocument objXmlDocument = XmlManager.Instance.Load(strXmlFile);
                    XmlNode objNode = objXmlDocument.SelectSingleNode(strXPath + "[name = \"" + _strName + "\"]");
                    if (objNode != null)
                    {
                        if (objNode["altpage"] != null)
                            strReturn = objNode["altpage"].InnerText;
                    }
                }

                return strReturn;
            }
            set
            {
                _strPage = value;
            }
        }

        /// <summary>
        /// Notes.
        /// </summary>
        public string Notes
        {
            get
            {
                return _strNotes;
            }
            set
            {
                _strNotes = value;
            }
        }

        /// <summary>
        /// Parent Power.
        /// </summary>
        public Power Parent
        {
            get
            {
                return _objParent;
            }
            set
            {
                _objParent = value;
            }
        }
        #endregion
    }

    /// <summary>
	/// An Adept Power.
	/// </summary>
	public class Power : INamedItemWithGuid
	{
		private Guid _guiID = new Guid();
		private string _strName = string.Empty;
		private string _strExtra = string.Empty;
		private string _strSource = string.Empty;
		private string _strPage = string.Empty;
		private decimal _decPointsPerLevel = 0;
		private int _intRating = 1;
		private bool _blnLevelsEnabled = false;
		private int _intMaxLevel = 0;
		private bool _blnDiscountedAdeptWay = false;
		private bool _blnDiscountedGeas = false;
		private XmlNode _nodBonus;
		private string _strNotes = string.Empty;
		private bool _blnDoubleCost = true;
        private bool _blnFree = false;
        private int _intFreeLevels = 0;
        private decimal _decAdeptWayDiscount = 0;
        private string _strBonusSource = string.Empty;
        private decimal _decFreePoints = 0;
        private List<Enhancement> _lstEnhancements = new List<Enhancement>();

		private readonly Character _objCharacter;

		#region Constructor, Create, Save, Load, and Print Methods
		public Power(Character objCharacter)
		{
			// Create the GUID for the new Power.
			_guiID = Guid.NewGuid();
			_objCharacter = objCharacter;
		}

		/// <summary>
		/// Save the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public void Save(XmlTextWriter objWriter)
		{
			objWriter.WriteStartElement("power");
			objWriter.WriteElementString("guid", _guiID.ToString());
			objWriter.WriteElementString("name", _strName);
			objWriter.WriteElementString("extra", _strExtra);
			objWriter.WriteElementString("pointsperlevel", _decPointsPerLevel.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("adeptway", _decAdeptWayDiscount.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("rating", _intRating.ToString());
			objWriter.WriteElementString("levels", _blnLevelsEnabled.ToString());
			objWriter.WriteElementString("maxlevel", _intMaxLevel.ToString());
			objWriter.WriteElementString("discounted", _blnDiscountedAdeptWay.ToString());
			objWriter.WriteElementString("discountedgeas", _blnDiscountedGeas.ToString());
            objWriter.WriteElementString("bonussource", _strBonusSource);
            objWriter.WriteElementString("freepoints", _decFreePoints.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("source", _strSource);
			objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("free", _blnFree.ToString());
            objWriter.WriteElementString("freelevels", _intFreeLevels.ToString());
            objWriter.WriteElementString("doublecost", _blnDoubleCost.ToString());
			if (_nodBonus != null)
				objWriter.WriteRaw("<bonus>" + _nodBonus.InnerXml + "</bonus>");
			else
				objWriter.WriteElementString("bonus", string.Empty);
            objWriter.WriteStartElement("enhancements");
            foreach (Enhancement objEnhancement in _lstEnhancements)
            {
                objEnhancement.Save(objWriter);
            }
            objWriter.WriteEndElement();
            objWriter.WriteElementString("notes", _strNotes);
			objWriter.WriteEndElement();
			_objCharacter.SourceProcess(_strSource);
		}

		/// <summary>
		/// Load the Power from the XmlNode.
		/// </summary>
		/// <param name="objNode">XmlNode to load.</param>
		public void Load(XmlNode objNode)
		{
            objNode.TryGetField("guid", Guid.TryParse, out _guiID);
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
            objNode.TryGetDecFieldQuickly("pointsperlevel", ref _decPointsPerLevel);
            if (objNode["adeptway"] != null)
                _decAdeptWayDiscount = Convert.ToDecimal(objNode["adeptway"].InnerText, GlobalOptions.InvariantCultureInfo);
            else
            {
                string strPowerName = _strName;
                if (strPowerName.Contains('('))
                    strPowerName = strPowerName.Substring(0, strPowerName.IndexOf('(') - 1);
                XmlDocument objXmlDocument = XmlManager.Instance.Load("powers.xml");
                XmlNode objXmlPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[starts-with(./name,\"" + strPowerName + "\")]");
                if (objXmlPower?["adeptway"] != null)
                    _decAdeptWayDiscount = Convert.ToDecimal(objXmlPower["adeptway"].InnerText, GlobalOptions.InvariantCultureInfo);
            }
            objNode.TryGetInt32FieldQuickly("rating", ref _intRating);
            objNode.TryGetBoolFieldQuickly("levels", ref _blnLevelsEnabled);
            objNode.TryGetBoolFieldQuickly("free", ref _blnFree);
            objNode.TryGetInt32FieldQuickly("freelevels", ref _intFreeLevels);
            objNode.TryGetInt32FieldQuickly("maxlevel", ref _intMaxLevel);

            objNode.TryGetBoolFieldQuickly("discounted", ref _blnDiscountedAdeptWay);
            objNode.TryGetBoolFieldQuickly("discountedgeas", ref _blnDiscountedGeas);
            objNode.TryGetStringFieldQuickly("bonussource", ref _strBonusSource);
            objNode.TryGetDecFieldQuickly("freepoints", ref _decFreePoints);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetBoolFieldQuickly("doublecost", ref _blnDoubleCost);
			_nodBonus = objNode["bonus"];
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            if (objNode.InnerXml.Contains("enhancements"))
            {
                XmlNodeList nodEnhancements = objNode.SelectNodes("enhancements/enhancement");
                foreach (XmlNode nodEnhancement in nodEnhancements)
                {
                    Enhancement objEnhancement = new Enhancement(_objCharacter);
                    objEnhancement.Load(nodEnhancement);
                    objEnhancement.Parent = this;
                    _lstEnhancements.Add(objEnhancement);
                }
            }
        }

		/// <summary>
		/// Print the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public void Print(XmlTextWriter objWriter)
		{
			objWriter.WriteStartElement("power");
			objWriter.WriteElementString("name", DisplayNameShort);
			objWriter.WriteElementString("extra", LanguageManager.Instance.TranslateExtra(_strExtra));
			objWriter.WriteElementString("pointsperlevel", _decPointsPerLevel.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("adeptway", _decAdeptWayDiscount.ToString(GlobalOptions.InvariantCultureInfo));
            if (_blnLevelsEnabled)
				objWriter.WriteElementString("rating", _intRating.ToString());
			else
				objWriter.WriteElementString("rating", "0");
			objWriter.WriteElementString("totalpoints", PowerPoints.ToString(GlobalOptions.InvariantCultureInfo));
			objWriter.WriteElementString("source", _objCharacter.Options.LanguageBookShort(_strSource));
			objWriter.WriteElementString("page", Page);
			if (_objCharacter.Options.PrintNotes)
				objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteStartElement("enhancements");
            foreach (Enhancement objEnhancement in _lstEnhancements)
            {
                objEnhancement.Print(objWriter);
            }
            objWriter.WriteEndElement();
            objWriter.WriteEndElement();
		}
		#endregion

		#region Properties
		/// <summary>
		/// The Character object being used by the Power.
		/// </summary>
		public Character CharacterObject
		{
			get
			{
				return _objCharacter;
			}
		}

		/// <summary>
		/// Internal identifier which will be used to identify this Power in the Improvement system.
		/// </summary>
		public string InternalId
		{
			get
			{
				return _guiID.ToString();
			}
		}

		/// <summary>
		/// Power's name.
		/// </summary>
		public string Name
		{
			get
			{
				return _strName;
			}
			set
			{
				_strName = value;
			}
		}

		/// <summary>
		/// Extra information that should be applied to the name, like a linked CharacterAttribute.
		/// </summary>
		public string Extra
		{
			get
			{
				return _strExtra;
			}
			set
			{
				_strExtra = value;
			}
		}

        /// <summary>
        /// The Enhancements currently applied to the Power.
        /// </summary>
        public List<Enhancement> Enhancements
        {
            get
            {
                return _lstEnhancements;
            }
        }

        /// <summary>
		/// The name of the object as it should be displayed on printouts (translated name only).
		/// </summary>
		public string DisplayNameShort
		{
			get
			{
				string strReturn = _strName;

				// Get the translated name if applicable.
				if (GlobalOptions.Instance.Language != "en-us")
				{
					XmlDocument objXmlDocument = XmlManager.Instance.Load("powers.xml");
					XmlNode objNode = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"" + _strName + "\"]");
					if (objNode["translate"] != null)
						strReturn = objNode["translate"].InnerText;
				}

				return strReturn;
			}
		}

		/// <summary>
		/// The full name of the Power (Name + any Extra text).
		/// </summary>
		public string FullName
		{
			get
			{
				string strReturn = DisplayNameShort;
				
				if (!string.IsNullOrEmpty(_strExtra))
				{
					LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
					// Attempt to retrieve the CharacterAttribute name.
					strReturn += " (" + LanguageManager.Instance.TranslateExtra(_strExtra) + ")";
				}

				return strReturn;
			}
		}

		/// <summary>
		/// Power Point cost per level of the Power.
		/// </summary>
		public decimal PointsPerLevel
		{
			get
			{
				return _decPointsPerLevel;
			}
			set
			{
				_decPointsPerLevel = value;
			}
		}

        /// <summary>
        /// Power Point discount for an Adept Way.
        /// </summary>
        public decimal AdeptWayDiscount
        {
            get
            {
                return _decAdeptWayDiscount;
            }
            set
            {
                _decAdeptWayDiscount = value;
            }
        }

        /// <summary>
		/// Calculated Power Point cost per level of the Power (including discounts).
		/// </summary>
		public decimal CalculatedPointsPerLevel
		{
			get
			{
				return _decPointsPerLevel;
			}
		}

		/// <summary>
		/// Calculate the discount that is applied to the Power.
		/// </summary>
		private decimal Discount
		{
			get
			{
                if (_blnDiscountedAdeptWay)
                    return _decAdeptWayDiscount;
                else
                    return _decFreePoints;
			}
		}

		/// <summary>
		/// The current Rating of the Power.
		/// </summary>
		public int Rating
		{
			get
			{
				return _intRating;
			}
			set
			{
				_intRating = value;
			}
		}

        /// <summary>
        /// Free levels of the power.
        /// </summary>
        public int FreeLevels
        {
            get
            {
                return _intFreeLevels;
            }
            set
            {
                _intFreeLevels = value;
            }
        }

        /// <summary>
        /// Is the power free.
        /// </summary>
        public bool Free
        {
            get
            {
                return _blnFree;
            }
            set
            {
                _blnFree = value;
            }
        }

        /// <summary>
		/// Total number of Power Points the Power costs.
		/// </summary>
		public decimal PowerPoints
		{
			get
			{
                if (_blnFree)
                    return 0;
                else
                {
                    decimal decReturn = (_intRating - _intFreeLevels) * PointsPerLevel;
                    decReturn -= Discount;

                    // Look at the Improvements created by the Power and determine if it has taken an CharacterAttribute above its Metatype Maximum.
                    //if (_blnDoubleCost)
                    //{
                    //    foreach (Improvement objImprovement in _objCharacter.Improvements)
                    //    {
                    //        if (objImprovement.SourceName == InternalId && objImprovement.ImproveType == Improvement.ImprovementType.CharacterAttribute && objImprovement.Enabled)
                    //        {
                    //            CharacterAttribute objAttribute = _objCharacter.GetAttribute(objImprovement.ImprovedName);
                    //            if (objAttribute.Value + objAttribute.AttributeValueModifiers > objAttribute.MetatypeMaximum + objAttribute.MaximumModifiers)
                    //            {
                    //                // Use the lower of the difference between Augmented Maximum and the Power's Rating.
                    //                int intDiff = (objAttribute.Value + objAttribute.AttributeValueModifiers) - (objAttribute.MetatypeMaximum + objAttribute.MaximumModifiers);
                    //                intDiff = Math.Min(intDiff, Convert.ToInt32(_intRating));

                    //                // Double the number of Power Points used to make up this difference.
                    //                decReturn += CalculatedPointsPerLevel * intDiff;
                    //            }
                    //        }
                    //    }
                    //}

                    return decReturn;
                }
			}
		}

        /// <summary>
        /// Bonus source.
        /// </summary>
        public string BonusSource
        {
            get
            {
                return _strBonusSource;
            }
            set
            {
                _strBonusSource = value;
            }
        }

        /// <summary>
        /// Bonus source.
        /// </summary>
        public decimal FreePoints
        {
            get
            {
                return _decFreePoints;
            }
            set
            {
                _decFreePoints = value;
            }
        }

        /// <summary>
		/// Sourcebook.
		/// </summary>
		public string Source
		{
			get
			{
				return _strSource;
			}
			set
			{
				_strSource = value;
			}
		}

		/// <summary>
		/// Page Number.
		/// </summary>
		public string Page
		{
			get
			{
				string strReturn = _strPage;

				// Get the translated name if applicable.
				if (GlobalOptions.Instance.Language != "en-us")
				{
					XmlDocument objXmlDocument = XmlManager.Instance.Load("powers.xml");
					XmlNode objNode = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"" + _strName + "\"]");
					if (objNode["altpage"] != null)
						strReturn = objNode["altpage"].InnerText;
				}

				return strReturn;
			}
			set
			{
				_strPage = value;
			}
		}

		/// <summary>
		/// Bonus node from the XML file.
		/// </summary>
		public XmlNode Bonus
		{
			get
			{
				return _nodBonus;
			}
			set
			{
				_nodBonus = value;
			}
		}

		/// <summary>
		/// Whether or not Levels enabled for the Power.
		/// </summary>
		public bool LevelsEnabled
		{
			get
			{
				return _blnLevelsEnabled;
			}
			set
			{
				_blnLevelsEnabled = value;
			}
		}

		/// <summary>
		/// Maximum Level for the Power.
		/// </summary>
		public int MaxLevels
		{
			get
			{
				return _intMaxLevel;
			}
			set
			{
				_intMaxLevel = value;
			}
		}

		/// <summary>
		/// Whether or not the Power Cost is discounted by 25% from Adept Way.
		/// </summary>
		public bool DiscountedAdeptWay
		{
			get
			{
				return _blnDiscountedAdeptWay;
			}
			set
			{
				_blnDiscountedAdeptWay = value;
			}
		}

		/// <summary>
		/// Whether or not the Power Cost is discounted by 25% from Geas.
		/// </summary>
		public bool DiscountedGeas
		{
			get
			{
				return _blnDiscountedGeas;
			}
			set
			{
				_blnDiscountedGeas = value;
			}
		}

		/// <summary>
		/// Notes.
		/// </summary>
		public string Notes
		{
			get
			{
				return _strNotes;
			}
			set
			{
				_strNotes = value;
			}
		}

		/// <summary>
		/// Whether or not the Power Point cost is doubled when an CharacterAttribute exceeds its Metatype Maximum.
		/// </summary>
		public bool DoubleCost
		{
			get
			{
				return _blnDoubleCost;
			}
			set
			{
				_blnDoubleCost = value;
			}
		}

		/// <summary>
		/// The number of Power Points that have been doubled because of exceeding the Metatype's Maximum CharacterAttribute values.
		/// </summary>
		public int DoubledPoints
		{
			get
			{
                if (_blnFree)
                    return 0;
                else
                {
                    decimal decReturn = (_intRating - _intFreeLevels) * PointsPerLevel;
                    decReturn -= Discount;
                    int intDoubledPoints = 0;

                    // Look at the Improvements created by the Power and determine if it has taken an CharacterAttribute above its Metatype Maximum.
                    if (_blnDoubleCost)
                    {
                        foreach (Improvement objImprovement in _objCharacter.Improvements)
                        {
                            if (objImprovement.SourceName == InternalId && objImprovement.ImproveType == Improvement.ImprovementType.Attribute && objImprovement.Enabled)
                            {
                                CharacterAttrib objAttribute = _objCharacter.GetAttribute(objImprovement.ImprovedName);
                                if (objAttribute.Value + objAttribute.AttributeValueModifiers > objAttribute.MetatypeMaximum + objAttribute.MaximumModifiers)
                                {
                                    // Use the lower of the difference between Augmented Maximum and the Power's Rating.
                                    int intDiff = (objAttribute.Value + objAttribute.AttributeValueModifiers) - (objAttribute.MetatypeMaximum + objAttribute.MaximumModifiers);
                                    intDiff = Math.Min(intDiff, Convert.ToInt32(_intRating));

                                    // Double the number of Power Points used to make up this difference.
                                    decReturn += CalculatedPointsPerLevel * intDiff;
                                    intDoubledPoints = intDiff;
                                }
                            }
                        }
                    }

				return intDoubledPoints;
                }
            }
		}
		#endregion
	}

	/// <summary>
	/// A Technomancer Program or Complex Form.
	/// </summary>
	public class ComplexForm : INamedItemWithGuid
	{
		private Guid _guiID = new Guid();
		private string _strName = string.Empty;
        private string _strTarget = string.Empty;
        private string _strDuration = string.Empty;
        private string _strFV = string.Empty;
        private string _strSource = string.Empty;
		private string _strPage = string.Empty;
        private string _strNotes = string.Empty;
        private string _strExtra = string.Empty;
		private string _strAltName = string.Empty;
		private string _strAltPage = string.Empty;
		private readonly Character _objCharacter;

		#region Constructor, Create, Save, Load, and Print Methods
        public ComplexForm(Character objCharacter)
		{
			// Create the GUID for the new Complex Form.
			_guiID = Guid.NewGuid();
			_objCharacter = objCharacter;
		}

		/// Create a Complex Form from an XmlNode.
		/// <param name="objXmlProgramNode">XmlNode to create the object from.</param>
		/// <param name="objCharacter">Character the Gear is being added to.</param>
		/// <param name="objNode">TreeNode to populate a TreeView.</param>
		/// <param name="strForcedValue">Value to forcefully select for any ImprovementManager prompts.</param>
		public void Create(XmlNode objXmlProgramNode, Character objCharacter, TreeNode objNode, string strExtra = "")
		{
			if (GlobalOptions.Instance.Language != "en-us")
			{
				XmlDocument objXmlDocument = XmlManager.Instance.Load("complexforms.xml");
				XmlNode objSpellNode = objXmlDocument.SelectSingleNode("/chummer/complexforms/complexform[name = \"" + _strName + "\"]");
				if (objSpellNode != null)
				{
                    objSpellNode.TryGetStringFieldQuickly("translate", ref _strAltName);
                    objSpellNode.TryGetStringFieldQuickly("altpage", ref _strAltPage);
				}
			}
            objXmlProgramNode.TryGetStringFieldQuickly("name", ref _strName);
            objXmlProgramNode.TryGetStringFieldQuickly("target", ref _strTarget);
            objXmlProgramNode.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlProgramNode.TryGetStringFieldQuickly("page", ref _strPage);
            objXmlProgramNode.TryGetStringFieldQuickly("duration", ref _strDuration);
            objXmlProgramNode.TryGetStringFieldQuickly("fv", ref _strFV);
            _strExtra = strExtra;

            objXmlProgramNode.TryGetStringFieldQuickly("notes", ref _strNotes);

			objNode.Text = DisplayName;
			objNode.Tag = _guiID.ToString();
		}

		/// <summary>
		/// Save the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public void Save(XmlTextWriter objWriter)
		{
			objWriter.WriteStartElement("complexform");
			objWriter.WriteElementString("guid", _guiID.ToString());
			objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("target", _strTarget);
            objWriter.WriteElementString("duration", _strDuration);
            objWriter.WriteElementString("fv", _strFV);
            objWriter.WriteElementString("extra", _strExtra);
            objWriter.WriteElementString("source", _strSource);
			objWriter.WriteElementString("page", _strPage);
			objWriter.WriteElementString("notes", _strNotes);
			objWriter.WriteEndElement();
			_objCharacter.SourceProcess(_strSource);
		}

		/// <summary>
		/// Load the Complex Form from the XmlNode.
		/// </summary>
		/// <param name="objNode">XmlNode to load.</param>
		public void Load(XmlNode objNode)
		{
            objNode.TryGetField("guid", Guid.TryParse, out _guiID);
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            objNode.TryGetStringFieldQuickly("target", ref _strTarget);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetStringFieldQuickly("duration", ref _strDuration);
            objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
            objNode.TryGetStringFieldQuickly("fv", ref _strFV);
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
		}

		/// <summary>
		/// Print the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public void Print(XmlTextWriter objWriter)
		{
			objWriter.WriteStartElement("complexform");
			objWriter.WriteElementString("name", DisplayNameShort);
			objWriter.WriteElementString("duration", _strDuration);
			objWriter.WriteElementString("fv", _strFV);
			objWriter.WriteElementString("target", _strTarget);
			objWriter.WriteElementString("source", _objCharacter.Options.LanguageBookShort(_strSource));
			objWriter.WriteElementString("page", Page);
			if (_objCharacter.Options.PrintNotes)
				objWriter.WriteElementString("notes", _strNotes);
			objWriter.WriteEndElement();
		}
		#endregion

		#region Properties
		/// <summary>
		/// Internal identifier which will be used to identify this Complex Form in the Improvement system.
		/// </summary>
		public string InternalId
		{
			get
			{
				return _guiID.ToString();
			}
		}

		/// <summary>
		/// Complex Form's name.
		/// </summary>
		public string Name
		{
			get
			{
				return _strName;
			}
			set
			{
				_strName = value;
			}
		}

        /// <summary>
        /// Complex Form's extra info.
        /// </summary>
        public string Extra
        {
            get
            {
                return _strExtra;
            }
            set
            {
                _strExtra = value;
            }
        }

        /// <summary>
		/// The name of the object as it should be displayed on printouts (translated name only).
		/// </summary>
		public string DisplayNameShort
		{
			get
			{
				string strReturn = _strName;
                if (!string.IsNullOrEmpty(_strExtra))
                    strReturn += " (" + _strExtra + ")";
				// Get the translated name if applicable.
                if (GlobalOptions.Instance.Language != "en-us")
                {
                    XmlDocument objXmlDocument = XmlManager.Instance.Load("complexforms.xml");
                    XmlNode objNode = objXmlDocument.SelectSingleNode("/chummer/complexforms/complexform[name = \"" + _strName + "\"]");
                    if (objNode != null)
                    {
                        if (objNode["translate"] != null)
                            strReturn = objNode["translate"].InnerText;
                    }
                }

				return strReturn;
			}
		}

		/// <summary>
		/// The name of the object as it should be displayed in lists. Name (Extra).
		/// </summary>
		public string DisplayName
		{
			get
			{
				string strReturn = DisplayNameShort;
				return strReturn;
			}
		}

		/// <summary>
		/// Complex Form's Duration.
		/// </summary>
        public string Duration
		{
			get
			{
				return _strDuration;
			}
			set
			{
                _strDuration = value;
			}
		}

		/// <summary>
		/// The Complex Form's FV.
		/// </summary>
        public string FV
		{
			get
			{
                return _strFV;
			}
			set
			{
                _strFV = value;
			}
		}

        /// <summary>
        /// The Complex Form's Target.
        /// </summary>
        public string Target
        {
            get
            {
                return _strTarget;
            }
            set
            {
                _strTarget = value;
            }
        }

        /// <summary>
		/// Complex Form's Source.
		/// </summary>
		public string Source
		{
			get
			{
				return _strSource;
			}
			set
			{
				_strSource = value;
			}
		}

		/// <summary>
		/// Sourcebook Page Number.
		/// </summary>
		public string Page
		{
			get
			{
				string strReturn = _strPage;
				// Get the translated name if applicable.
                //if (GlobalOptions.Instance.Language != "en-us")
                //{
                //    XmlDocument objXmlDocument = XmlManager.Instance.Load("complexforms.xml");
                //    XmlNode objNode = objXmlDocument.SelectSingleNode("/chummer/complexforms/complexform[name = \"" + _strName + "\"]");
                //    if (objNode != null)
                //    {
                //        if (objNode["altpage"] != null)
                //            strReturn = objNode["altpage"].InnerText;
                //    }
                //}

				return strReturn;
			}
			set
			{
				_strPage = value;
			}
		}

		/// <summary>
		/// Notes.
		/// </summary>
		public string Notes
		{
			get
			{
				return _strNotes;
			}
			set
			{
				_strNotes = value;
			}
		}
		#endregion
	}

    /// <summary>
	/// An AI Program or Advanced Program.
	/// </summary>
	public class AIProgram : INamedItemWithGuid
    {
        private Guid _guiID = new Guid();
        private string _strName = string.Empty;
        private string _strRequiresProgram = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private string _strNotes = string.Empty;
        private string _strExtra = string.Empty;
        private string _strAltName = string.Empty;
        private string _strAltPage = string.Empty;
        private bool _boolIsAdvancedProgram = false;
        private bool _boolCanDelete = true;
        private readonly Character _objCharacter;

        #region Constructor, Create, Save, Load, and Print Methods
        public AIProgram(Character objCharacter)
        {
            // Create the GUID for the new Program.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// Create a Program from an XmlNode.
        /// <param name="objXmlProgramNode">XmlNode to create the object from.</param>
        /// <param name="objCharacter">Character the Gear is being added to.</param>
        /// <param name="objNode">TreeNode to populate a TreeView.</param>
        /// <param name="strForcedValue">Value to forcefully select for any ImprovementManager prompts.</param>
        public void Create(XmlNode objXmlProgramNode, Character objCharacter, TreeNode objNode, bool boolIsAdvancedProgram, string strExtra = "", bool boolCanDelete = true)
        {
            if (GlobalOptions.Instance.Language != "en-us")
            {
                XmlDocument objXmlDocument = XmlManager.Instance.Load("programs.xml");
                XmlNode objSpellNode = objXmlDocument.SelectSingleNode("/chummer/programs/program[name = \"" + _strName + "\"]");
                if (objSpellNode != null)
                {
                    objSpellNode.TryGetStringFieldQuickly("translate", ref _strAltName);
                    objSpellNode.TryGetStringFieldQuickly("altpage", ref _strAltPage);
                }
            }
            objXmlProgramNode.TryGetStringFieldQuickly("name", ref _strName);
            _strRequiresProgram = LanguageManager.Instance.GetString("String_None");
            _boolCanDelete = boolCanDelete;
            objXmlProgramNode.TryGetStringFieldQuickly("require", ref _strRequiresProgram);
            objXmlProgramNode.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlProgramNode.TryGetStringFieldQuickly("page", ref _strPage);
            _strExtra = strExtra;
            _boolIsAdvancedProgram = boolIsAdvancedProgram;

            objXmlProgramNode.TryGetStringFieldQuickly("notes", ref _strNotes);

            objNode.Text = DisplayName;
            objNode.Tag = _guiID.ToString();
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("aiprogram");
            objWriter.WriteElementString("guid", _guiID.ToString());
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("requiresprogram", _strRequiresProgram);
            objWriter.WriteElementString("extra", _strExtra);
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteElementString("isadvancedprogram", _boolIsAdvancedProgram ? "true" : "false");
            objWriter.WriteEndElement();
            _objCharacter.SourceProcess(_strSource);
        }

        /// <summary>
        /// Load the Program from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            objNode.TryGetField("guid", Guid.TryParse, out _guiID);
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            objNode.TryGetStringFieldQuickly("requiresprogram", ref _strRequiresProgram);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            if (objNode["isadvancedprogram"] != null)
            {
                _boolIsAdvancedProgram = objNode["isadvancedprogram"].InnerText == "true";
            }
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Print(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("aiprogram");
            objWriter.WriteElementString("name", DisplayNameShort);
            if (string.IsNullOrEmpty(_strRequiresProgram) || _strRequiresProgram == LanguageManager.Instance.GetString("String_None"))
                objWriter.WriteElementString("requiresprogram", LanguageManager.Instance.GetString("String_None")); 
            else
                objWriter.WriteElementString("requiresprogram", DisplayRequiresProgram);
            objWriter.WriteElementString("source", _objCharacter.Options.LanguageBookShort(_strSource));
            objWriter.WriteElementString("page", Page);
            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteEndElement();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Internal identifier which will be used to identify this AI Program in the Improvement system.
        /// </summary>
        public string InternalId
        {
            get
            {
                return _guiID.ToString();
            }
        }

        /// <summary>
        /// AI Program's name.
        /// </summary>
        public string Name
        {
            get
            {
                return _strName;
            }
            set
            {
                _strName = value;
            }
        }

        /// <summary>
        /// AI Program's extra info.
        /// </summary>
        public string Extra
        {
            get
            {
                return _strExtra;
            }
            set
            {
                _strExtra = value;
            }
        }

        /// <summary>
		/// The name of the object as it should be displayed on printouts (translated name only).
		/// </summary>
		public string DisplayNameShort
        {
            get
            {
                string strReturn = _strName;
                if (!string.IsNullOrEmpty(_strExtra))
                    strReturn += " (" + _strExtra + ")";
                // Get the translated name if applicable.
                if (GlobalOptions.Instance.Language != "en-us")
                {
                    XmlDocument objXmlDocument = XmlManager.Instance.Load("programs.xml");
                    XmlNode objNode = objXmlDocument.SelectSingleNode("/chummer/programs/program[name = \"" + _strName + "\"]");
                    if (objNode != null)
                    {
                        if (objNode["translate"] != null)
                            strReturn = objNode["translate"].InnerText;
                    }
                }

                return strReturn;
            }
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Name (Extra).
        /// </summary>
        public string DisplayName
        {
            get
            {
                string strReturn = DisplayNameShort;
                return strReturn;
            }
        }

        /// <summary>
		/// AI Advanced Program's requirement program.
		/// </summary>
		public string RequiresProgram
        {
            get
            {
                return _strRequiresProgram;
            }
            set
            {
                _strRequiresProgram = value;
            }
        }

        /// <summary>
		/// AI Advanced Program's requirement program.
		/// </summary>
		public string DisplayRequiresProgram
        {
            get
            {
                string strReturn = RequiresProgram;
                // Get the translated name if applicable.
                if (GlobalOptions.Instance.Language != "en-us")
                {
                    XmlDocument objXmlDocument = XmlManager.Instance.Load("programs.xml");
                    XmlNode objNode = objXmlDocument.SelectSingleNode("/chummer/programs/program[name = \"" + RequiresProgram + "\"]");
                    if (objNode != null)
                    {
                        if (objNode["translate"] != null)
                            strReturn = objNode["translate"].InnerText;
                    }
                }

                return strReturn;
            }
        }

        /// <summary>
		/// If the AI Advanced Program is added from a quality.
		/// </summary>
		public bool CanDelete
        {
            get
            {
                return _boolCanDelete;
            }
            set
            {
                _boolCanDelete = value;
            }
        }

        /// <summary>
		/// AI Program's Source.
		/// </summary>
		public string Source
        {
            get
            {
                return _strSource;
            }
            set
            {
                _strSource = value;
            }
        }

        /// <summary>
        /// Sourcebook Page Number.
        /// </summary>
        public string Page
        {
            get
            {
                string strReturn = _strPage;
                // Get the translated name if applicable.
                //if (GlobalOptions.Instance.Language != "en-us")
                //{
                //    XmlDocument objXmlDocument = XmlManager.Instance.Load("complexforms.xml");
                //    XmlNode objNode = objXmlDocument.SelectSingleNode("/chummer/programs/program[name = \"" + _strName + "\"]");
                //    if (objNode != null)
                //    {
                //        if (objNode["altpage"] != null)
                //            strReturn = objNode["altpage"].InnerText;
                //    }
                //}

                return strReturn;
            }
            set
            {
                _strPage = value;
            }
        }

        /// <summary>
        /// Notes.
        /// </summary>
        public string Notes
        {
            get
            {
                return _strNotes;
            }
            set
            {
                _strNotes = value;
            }
        }

        /// <summary>
		/// If the AI Program is an Advanced Program.
		/// </summary>
		public bool IsAdvancedProgram
        {
            get
            {
                return _boolIsAdvancedProgram;
            }
        }
        #endregion
    }

    /// <summary>
    /// A Martial Art.
    /// </summary>
    public class MartialArt : INamedItemWithGuid
	{
		private string _strName = string.Empty;
		private string _strSource = string.Empty;
		private string _strPage = string.Empty;
		private int _intRating = 1;
		private Guid _guiID = new Guid();
		private List<MartialArtAdvantage> _lstAdvantages = new List<MartialArtAdvantage>();
		private string _strNotes = string.Empty;
		private Character _objCharacter;
        private bool _blnIsQuality;

		#region Create, Save, Load, and Print Methods
		public MartialArt(Character objCharacter)
		{
			_objCharacter = objCharacter;
			_guiID = Guid.NewGuid();
		}

		/// Create a Martial Art from an XmlNode and return the TreeNodes for it.
		/// <param name="objXmlArtNode">XmlNode to create the object from.</param>
		/// <param name="objNode">TreeNode to populate a TreeView.</param>
		/// <param name="objCharacter">Character the Martial Art is being added to.</param>
		public void Create(XmlNode objXmlArtNode, TreeNode objNode, Character objCharacter)
		{
			_objCharacter = objCharacter;
            objXmlArtNode.TryGetStringFieldQuickly("name", ref _strName);
            objXmlArtNode.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlArtNode.TryGetStringFieldQuickly("page", ref _strPage);
            _blnIsQuality = objXmlArtNode["isquality"] != null && Convert.ToBoolean(objXmlArtNode["isquality"].InnerText);

			if (objXmlArtNode["bonus"] != null)
			{
				ImprovementManager objImprovementManager = new ImprovementManager(objCharacter);
				objImprovementManager.CreateImprovements(Improvement.ImprovementSource.MartialArt, InternalId,
					objXmlArtNode["bonus"], false, 1, DisplayNameShort);
			}

			objNode.Text = DisplayName;
			objNode.Tag = _guiID.ToString();
		}

		/// <summary>
		/// Save the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public void Save(XmlTextWriter objWriter)
		{
			objWriter.WriteStartElement("martialart");
			objWriter.WriteElementString("name", _strName);
			objWriter.WriteElementString("guid", InternalId);
			objWriter.WriteElementString("source", _strSource);
			objWriter.WriteElementString("page", _strPage);
			objWriter.WriteElementString("rating", _intRating.ToString());
            objWriter.WriteElementString("isquality", _blnIsQuality.ToString());
            objWriter.WriteStartElement("martialartadvantages");
            foreach (MartialArtAdvantage objAdvantage in _lstAdvantages)
			{
				objAdvantage.Save(objWriter);
			}
			objWriter.WriteEndElement();
			objWriter.WriteElementString("notes", _strNotes);
			objWriter.WriteEndElement();
			_objCharacter.SourceProcess(_strSource);
		}

		/// <summary>
		/// Load the Martial Art from the XmlNode.
		/// </summary>
		/// <param name="objNode">XmlNode to load.</param>
		public void Load(XmlNode objNode)
		{
            if (objNode.TryGetField("guid", Guid.TryParse, out _guiID))
                _guiID = Guid.NewGuid();
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetInt32FieldQuickly("rating", ref _intRating);
            objNode.TryGetBoolFieldQuickly("isquality", ref _blnIsQuality);

			if (objNode.InnerXml.Contains("martialartadvantages"))
			{
				XmlNodeList nodAdvantages = objNode.SelectNodes("martialartadvantages/martialartadvantage");
				foreach (XmlNode nodAdvantage in nodAdvantages)
				{
					MartialArtAdvantage objAdvantage = new MartialArtAdvantage(_objCharacter);
					objAdvantage.Load(nodAdvantage);
					_lstAdvantages.Add(objAdvantage);
				}
			}

            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
		}

		/// <summary>
		/// Print the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public void Print(XmlTextWriter objWriter)
		{
			objWriter.WriteStartElement("martialart");
			objWriter.WriteElementString("name", DisplayNameShort);
			objWriter.WriteElementString("source", _objCharacter.Options.LanguageBookShort(_strSource));
			objWriter.WriteElementString("page", Page);
			objWriter.WriteElementString("rating", _intRating.ToString());
			objWriter.WriteStartElement("martialartadvantages");
			foreach (MartialArtAdvantage objAdvantage in _lstAdvantages)
			{
				objAdvantage.Print(objWriter);
			}
			objWriter.WriteEndElement();
			if (_objCharacter.Options.PrintNotes)
				objWriter.WriteElementString("notes", _strNotes);
			objWriter.WriteEndElement();
		}
		#endregion

		#region Properties
		/// <summary>
		/// Name.
		/// </summary>
		public string Name
		{
			get
			{
				return _strName;
			}
			set
			{
				_strName = value;
			}
		}

		public string InternalId
		{
			get { return _guiID.ToString(); }
		}

		/// <summary>
		/// The name of the object as it should be displayed on printouts (translated name only).
		/// </summary>
		public string DisplayNameShort
		{
			get
			{
				string strReturn = _strName;
				// Get the translated name if applicable.
				if (GlobalOptions.Instance.Language != "en-us")
				{
					XmlDocument objXmlDocument = XmlManager.Instance.Load("martialarts.xml");
					XmlNode objNode = objXmlDocument.SelectSingleNode("/chummer/martialarts/martialart[name = \"" + _strName + "\"]");
					if (objNode != null)
					{
						if (objNode["translate"] != null)
							strReturn = objNode["translate"].InnerText;
					}
				}

				return strReturn;
			}
		}

		/// <summary>
		/// The name of the object as it should be displayed in lists. Name (Extra).
		/// </summary>
		public string DisplayName
		{
			get
			{
				string strReturn = DisplayNameShort;

				return strReturn;
			}
		}

		/// <summary>
		/// Sourcebook.
		/// </summary>
		public string Source
		{
			get
			{
				return _strSource;
			}
			set
			{
				_strSource = value;
			}
		}

		/// <summary>
		/// Page Number.
		/// </summary>
		public string Page
		{
			get
			{
				string strReturn = _strPage;
				// Get the translated name if applicable.
				if (GlobalOptions.Instance.Language != "en-us")
				{
					XmlDocument objXmlDocument = XmlManager.Instance.Load("martialarts.xml");
					XmlNode objNode = objXmlDocument.SelectSingleNode("/chummer/martialarts/martialart[name = \"" + _strName + "\"]");
					if (objNode != null)
					{
						if (objNode["altpage"] != null)
							strReturn = objNode["altpage"].InnerText;
					}
				}

				return strReturn;
			}
			set
			{
				_strPage = value;
			}
		}

		/// <summary>
		/// Rating.
		/// </summary>
		public int Rating
		{
			get
			{
				return _intRating;
			}
			set
			{
				_intRating = value;
			}
		}

        /// <summary>
        /// Is from a quality.
        /// </summary>
        public bool IsQuality
        {
            get
            {
                return _blnIsQuality;
            }
            set
            {
                _blnIsQuality = value;
            }
        }

        /// <summary>
		/// Selected Martial Arts Advantages.
		/// </summary>
		public List<MartialArtAdvantage> Advantages
		{
			get
			{
				return _lstAdvantages;
			}
		}

		/// <summary>
		/// Notes.
		/// </summary>
		public string Notes
		{
			get
			{
				return _strNotes;
			}
			set
			{
				_strNotes = value;
			}
		}
		#endregion
	}

	/// <summary>
	/// A Martial Arts Advantage.
	/// </summary>
	public class MartialArtAdvantage : INamedItemWithGuid
	{
		private Guid _guiID = new Guid();
		private string _strName = string.Empty;
		private string _strNotes = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
		private Character _objCharacter;

		#region Constructor, Create, Save, Load, and Print Methods
		public MartialArtAdvantage(Character objCharacter)
		{
			// Create the GUID for the new Martial Art Advantage.
			_guiID = Guid.NewGuid();
			_objCharacter = objCharacter;
		}

		/// Create a Martial Art Advantage from an XmlNode and return the TreeNodes for it.
		/// <param name="objXmlAdvantageNode">XmlNode to create the object from.</param>
		/// <param name="objCharacter">Character the Gear is being added to.</param>
		/// <param name="objNode">TreeNode to populate a TreeView.</param>
		public void Create(XmlNode objXmlAdvantageNode, Character objCharacter, TreeNode objNode)
		{
            objXmlAdvantageNode.TryGetStringFieldQuickly("name", ref _strName);
            objXmlAdvantageNode.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlAdvantageNode.TryGetStringFieldQuickly("page", ref _strPage);

            if (objXmlAdvantageNode["bonus"] != null)
			{
				ImprovementManager objImprovementManager = new ImprovementManager(objCharacter);
				if (!objImprovementManager.CreateImprovements(Improvement.ImprovementSource.MartialArtAdvantage, _guiID.ToString(), objXmlAdvantageNode["bonus"], false, 1, DisplayNameShort))
				{
					_guiID = Guid.Empty;
					return;
				}
			}

			objNode.Text = DisplayName;
			objNode.Tag = _guiID.ToString();
		}

		/// <summary>
		/// Save the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public void Save(XmlTextWriter objWriter)
		{
			objWriter.WriteStartElement("martialartadvantage");
			objWriter.WriteElementString("guid", _guiID.ToString());
			objWriter.WriteElementString("name", _strName);
			objWriter.WriteElementString("notes", _strNotes);
			objWriter.WriteElementString("source", _strSource);
			objWriter.WriteElementString("page", _strSource);
            objWriter.WriteEndElement();
			_objCharacter.SourceProcess(_strSource);
		}

		/// <summary>
		/// Load the Martial Art Advantage from the XmlNode.
		/// </summary>
		/// <param name="objNode">XmlNode to load.</param>
		public void Load(XmlNode objNode)
		{
            if (objNode.TryGetField("guid", Guid.TryParse, out _guiID))
                _guiID = Guid.NewGuid();
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
        }

		/// <summary>
		/// Print the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public void Print(XmlTextWriter objWriter)
		{
			objWriter.WriteStartElement("martialartadvantage");
			objWriter.WriteElementString("name", DisplayNameShort);
			if (_objCharacter.Options.PrintNotes)
				objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteEndElement();
		}
		#endregion

		#region Properties
		/// <summary>
		/// Internal identifier which will be used to identify this Martial Art Advantage in the Improvement system.
		/// </summary>
		public string InternalId
		{
			get
			{
				return _guiID.ToString();
			}
		}

		/// <summary>
		/// Name.
		/// </summary>
		public string Name
		{
			get
			{
				return _strName;
			}
			set
			{
				_strName = value;
			}
		}

		/// <summary>
		/// The name of the object as it should be displayed on printouts (translated name only).
		/// </summary>
		public string DisplayNameShort
		{
			get
			{
				string strReturn = _strName;
				// Get the translated name if applicable.
				if (GlobalOptions.Instance.Language != "en-us")
				{
					XmlDocument objXmlDocument = XmlManager.Instance.Load("martialarts.xml");
                    XmlNode objNode = objXmlDocument.SelectSingleNode("/chummer/martialarts/martialart/techniques/technique[. = \"" + _strName + "\"]");
					if (objNode != null)
					{
						if (objNode.Attributes["translate"] != null)
							strReturn = objNode.Attributes["translate"].InnerText;
					}
				}

				return strReturn;
			}
		}

		/// <summary>
		/// The name of the object as it should be displayed in lists. Name (Extra).
		/// </summary>
		public string DisplayName
		{
			get
			{
				string strReturn = DisplayNameShort;

				return strReturn;
			}
		}

        /// <summary>
        /// Notes attached to this technique.
        /// </summary>
        public string Notes
        {
            get
            {
                return _strNotes;
            }
            set
            {
                _strNotes = value;
            }
        }

        /// <summary>
		/// Sourcebook.
		/// </summary>
		public string Source
        {
            get
            {
                return _strSource;
            }
            set
            {
                _strSource = value;
            }
        }

        /// <summary>
        /// Page Number.
        /// </summary>
        public string Page
        {
            get
            {
                return _strPage;
            }
            set
            {
                _strPage = value;
            }
        }
        #endregion
    }

    /// <summary>
    /// A Martial Art Maneuver.
    /// </summary>
    public class MartialArtManeuver : INamedItemWithGuid
	{
		private Guid _guiID = new Guid();
		private string _strName = string.Empty;
		private string _strSource = string.Empty;
		private string _strPage = string.Empty;
		private string _strNotes = string.Empty;
		private readonly Character _objCharacter;

		#region Constructor, Create, Save, Load, and Print Methods
		public MartialArtManeuver(Character objCharacter)
		{
			// Create the GUID for the new Martial Art Maneuver.
			_guiID = Guid.NewGuid();
			_objCharacter = objCharacter;
		}

		/// Create a Martial Art Maneuver from an XmlNode and return the TreeNodes for it.
		/// <param name="objXmlManeuverNode">XmlNode to create the object from.</param>
		/// <param name="objNode">TreeNode to populate a TreeView.</param>
		public void Create(XmlNode objXmlManeuverNode, TreeNode objNode)
		{
            objXmlManeuverNode.TryGetStringFieldQuickly("name", ref _strName);
            objXmlManeuverNode.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlManeuverNode.TryGetStringFieldQuickly("page", ref _strPage);

            objNode.Text = DisplayName;
			objNode.Tag = _guiID.ToString();
		}

		/// <summary>
		/// Save the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public void Save(XmlTextWriter objWriter)
		{
			objWriter.WriteStartElement("martialartmaneuver");
			objWriter.WriteElementString("guid", _guiID.ToString());
			objWriter.WriteElementString("name", _strName);
			objWriter.WriteElementString("source", _strSource);
			objWriter.WriteElementString("page", _strPage);
			objWriter.WriteElementString("notes", _strNotes);
			objWriter.WriteEndElement();
			_objCharacter.SourceProcess(_strSource);
		}

		/// <summary>
		/// Load the Martial Art Maneuver from the XmlNode.
		/// </summary>
		/// <param name="objNode">XmlNode to load.</param>
		public void Load(XmlNode objNode)
		{
            if (objNode.TryGetField("guid", Guid.TryParse, out _guiID))
                _guiID = Guid.NewGuid();
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
        }

		/// <summary>
		/// Print the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public void Print(XmlTextWriter objWriter)
		{
			objWriter.WriteStartElement("martialartmaneuver");
			objWriter.WriteElementString("name", DisplayNameShort);
			objWriter.WriteElementString("source", _objCharacter.Options.LanguageBookShort(_strSource));
			objWriter.WriteElementString("page", Page);
			if (_objCharacter.Options.PrintNotes)
				objWriter.WriteElementString("notes", _strNotes);
			objWriter.WriteEndElement();
		}
		#endregion

		#region Properties
		/// <summary>
		/// Internal identifier which will be used to identify this Martial Art Maneuver in the Improvement system.
		/// </summary>
		public string InternalId
		{
			get
			{
				return _guiID.ToString();
			}
		}

		/// <summary>
		/// Name.
		/// </summary>
		public string Name
		{
			get
			{
				return _strName;
			}
			set
			{
				_strName = value;
			}
		}

		/// <summary>
		/// The name of the object as it should be displayed on printouts (translated name only).
		/// </summary>
		public string DisplayNameShort
		{
			get
			{
				string strReturn = _strName;
				// Get the translated name if applicable.
				if (GlobalOptions.Instance.Language != "en-us")
				{
					XmlDocument objXmlDocument = XmlManager.Instance.Load("martialarts.xml");
					XmlNode objNode = objXmlDocument.SelectSingleNode("/chummer/maneuvers/maneuver[name = \"" + _strName + "\"]");
					if (objNode != null)
					{
						if (objNode["translate"] != null)
							strReturn = objNode["translate"].InnerText;
					}
				}

				return strReturn;
			}
		}

		/// <summary>
		/// The name of the object as it should be displayed in lists. Name (Extra).
		/// </summary>
		public string DisplayName
		{
			get
			{
				string strReturn = DisplayNameShort;

				return strReturn;
			}
		}

		/// <summary>
		/// Sourcebook.
		/// </summary>
		public string Source
		{
			get
			{
				return _strSource;
			}
			set
			{
				_strSource = value;
			}
		}

		/// <summary>
		/// Page.
		/// </summary>
		public string Page
		{
			get
			{
				string strReturn = _strPage;
				// Get the translated name if applicable.
				if (GlobalOptions.Instance.Language != "en-us")
				{
					XmlDocument objXmlDocument = XmlManager.Instance.Load("martialarts.xml");
					XmlNode objNode = objXmlDocument.SelectSingleNode("/chummer/maneuvers/maneuver[name = \"" + _strName + "\"]");
					if (objNode != null)
					{
						if (objNode["altpage"] != null)
							strReturn = objNode["altpage"].InnerText;
					}
				}

				return strReturn;
			}
			set
			{
				_strPage = value;
			}
		}

		/// <summary>
		/// Notes.
		/// </summary>
		public string Notes
		{
			get
			{
				return _strNotes;
			}
			set
			{
				_strNotes = value;
			}
		}
		#endregion
	}

    /// <summary>
	/// Type of Contact.
	/// </summary>
	public enum LimitType
    {
        Physical = 0,
        Mental = 1,
        Social = 2
    }

    /// <summary>
    /// A Skill Limit Modifier.
    /// </summary>
    public class LimitModifier : INamedItemWithGuid
    {
        private Guid _guiID = new Guid();
        private string _strName = string.Empty;
        private string _strNotes = string.Empty;
        private string _strLimit = string.Empty;
        private string _strCondition = string.Empty;
        private int _intBonus = 0;
        private Character _objCharacter;

        #region Constructor, Create, Save, Load, and Print Methods
        public LimitModifier(Character objCharacter)
        {
            // Create the GUID for the new Skill Limit Modifier.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// Create a Skill Limit Modifier from an XmlNode and return the TreeNodes for it.
        /// <param name="objXmlAdvantageNode">XmlNode to create the object from.</param>
        /// <param name="objCharacter">Character the Gear is being added to.</param>
        /// <param name="objNode">TreeNode to populate a TreeView.</param>
        public void Create(XmlNode objXmlLimitModifierNode, Character objCharacter, TreeNode objNode)
        {
            _strName = objXmlLimitModifierNode["name"].InnerText;

            if (objXmlLimitModifierNode["bonus"] != null)
            {
                ImprovementManager objImprovementManager = new ImprovementManager(objCharacter);
                if (!objImprovementManager.CreateImprovements(Improvement.ImprovementSource.MartialArtAdvantage, _guiID.ToString(), objXmlLimitModifierNode["bonus"], false, 1, DisplayNameShort))
                {
                    _guiID = Guid.Empty;
                    return;
                }
            }

            objNode.Text = DisplayName;
            objNode.Tag = _guiID.ToString();
        }

        /// Create a Skill Limit Modifier from properties and return the TreeNodes for it.
        /// <param name="strName">The name of the modifier.</param>
        /// <param name="intBonus">The bonus amount.</param>
        /// <param name="strLimit">The limit this modifies.</param>
        /// <param name="objCharacter">Character the modifier is being added to.</param>
        /// <param name="objNode">TreeNode to populate a TreeView.</param>
        public void Create(string strName, int intBonus, string strLimit, string strCondition, Character objCharacter, TreeNode objNode)
        {
            _strName = strName;
            _strLimit = strLimit;
            _intBonus = intBonus;
            _strCondition = strCondition;

            objNode.Text = DisplayName;
            objNode.Tag = _guiID.ToString();
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("limitmodifier");
            objWriter.WriteElementString("guid", _guiID.ToString());
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("limit", _strLimit);
            objWriter.WriteElementString("condition", _strCondition);
            objWriter.WriteElementString("bonus", _intBonus.ToString());
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Load the Skill Limit Modifier from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            if (objNode.TryGetField("guid", Guid.TryParse, out _guiID))
                _guiID = Guid.NewGuid();
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            objNode.TryGetStringFieldQuickly("limit", ref _strLimit);
            objNode.TryGetInt32FieldQuickly("bonus", ref _intBonus);
            objNode.TryGetStringFieldQuickly("condition", ref _strCondition);
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Print(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("limitmodifier");
            objWriter.WriteElementString("name", DisplayName);
            objWriter.WriteElementString("condition", _strCondition);
            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteEndElement();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Internal identifier which will be used to identify this Skill Limit Modifier in the Improvement system.
        /// </summary>
        public string InternalId
        {
            get
            {
                return _guiID.ToString();
            }
        }

		/// <summary>
		/// Internal identifier which will be used to identify this Skill Limit Modifier in the Improvement system.
		/// </summary>
		public Guid Guid
		{
			get { return _guiID; }
			set
			{
				_guiID = value;
			}
		}

		/// <summary>
		/// Name.
		/// </summary>
		public string Name
        {
            get
            {
                return _strName;
            }
            set
            {
                _strName = value;
            }
        }

        /// <summary>
        /// Name.
        /// </summary>
        public string Notes
        {
            get
            {
                return _strNotes;
            }
            set
            {
                _strNotes = value;
            }
        }

        /// <summary>
        /// Limit.
        /// </summary>
        public string Limit
        {
            get
            {
                return _strLimit;
            }
            set
            {
                _strLimit = value;
            }
        }

        /// <summary>
        /// Condition.
        /// </summary>
        public string Condition
        {
            get
            {
                return _strCondition;
            }
            set
            {
                _strCondition = value;
            }
        }

        /// <summary>
        /// Bonus.
        /// </summary>
        public int Bonus
        {
            get
            {
                return _intBonus;
            }
            set
            {
                _intBonus = value;
            }
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort
        {
            get
            {
                string strReturn = _strName;
                return strReturn;
            }
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Name (Extra).
        /// </summary>
        public string DisplayName
        {
            get
            {
                string strBonus = string.Empty;
                if (_intBonus > 0)
                    strBonus = "+" + _intBonus.ToString();
                else
                    strBonus = _intBonus.ToString();

                string strReturn = DisplayNameShort + " [" + strBonus + "]";
                if (!string.IsNullOrEmpty(_strCondition))
                    strReturn += " (" + _strCondition + ")";
                return strReturn;
            }
        }
        #endregion
    }

    /// <summary>
	/// Type of Contact.
	/// </summary>
	public enum ContactType
	{
		Contact = 0,
		Enemy = 1,
		Pet = 2,
	}

	/// <summary>
	/// A Contact or Enemy.
	/// </summary>
	public class Contact
	{
		private string _strName = string.Empty;
        private string _strRole = string.Empty;
        private string _strLocation = string.Empty;
	    private string _strUnique;

        private int _intConnection = 1;
		private int _intLoyalty = 1;
		
		private string _strGroupName = string.Empty;
		private ContactType _objContactType = ContactType.Contact;
		private string _strFileName = string.Empty;
		private string _strRelativeName = string.Empty;
		private string _strNotes = string.Empty;
		private Color _objColour;
		private bool _blnFree = false;
        private bool _blnIsGroup = false;
		private Character _objCharacter;
	    private bool _blnMadeMan;
		private bool _blnBlackmail;
		private bool _blnFamily;
		private bool _readonly = false;

		#region Helper Methods
		/// <summary>
		/// Convert a string to a ContactType.
		/// </summary>
		/// <param name="strValue">String value to convert.</param>
		public ContactType ConvertToContactType(string strValue)
		{
			switch (strValue)
			{
				case "Contact":
					return ContactType.Contact;
				case "Pet":
					return ContactType.Pet;
				default:
					return ContactType.Enemy;
			}
		}
		#endregion

		#region Constructor, Save, Load, and Print Methods
		public Contact(Character objCharacter)
		{
			_objCharacter = objCharacter;
		}

		/// <summary>
		/// Save the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public void Save(XmlTextWriter objWriter)
		{
			objWriter.WriteStartElement("contact");
			objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("role", _strRole);
            objWriter.WriteElementString("location", _strLocation);
            objWriter.WriteElementString("connection", _intConnection.ToString());
			objWriter.WriteElementString("loyalty", _intLoyalty.ToString());
            objWriter.WriteElementString("type", _objContactType.ToString());
			objWriter.WriteElementString("file", _strFileName);
			objWriter.WriteElementString("relative", _strRelativeName);
			objWriter.WriteElementString("notes", _strNotes);
			objWriter.WriteElementString("groupname", _strGroupName);
			objWriter.WriteElementString("colour", _objColour.ToArgb().ToString());
			objWriter.WriteElementString("free", _blnFree.ToString());
            objWriter.WriteElementString("group", _blnIsGroup.ToString());
            objWriter.WriteElementString("mademan", _blnMadeMan.ToString());
			objWriter.WriteElementString("family",_blnFamily.ToString());
			objWriter.WriteElementString("blackmail", _blnBlackmail.ToString());

			if (ReadOnly) objWriter.WriteElementString("readonly", string.Empty);

			if (_strUnique != null)
		    {
		        objWriter.WriteElementString("guid", _strUnique);
		    }
            objWriter.WriteEndElement();
		}

		/// <summary>
		/// Load the Contact from the XmlNode.
		/// </summary>
		/// <param name="objNode">XmlNode to load.</param>
		public void Load(XmlNode objNode)
		{
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            objNode.TryGetStringFieldQuickly("role", ref _strRole);
			objNode.TryGetStringFieldQuickly("location", ref _strLocation);
            objNode.TryGetInt32FieldQuickly("connection", ref _intConnection);
            objNode.TryGetInt32FieldQuickly("loyalty", ref _intLoyalty);
            if (objNode["type"] != null)
                _objContactType = ConvertToContactType(objNode["type"].InnerText);
			objNode.TryGetStringFieldQuickly("file", ref _strFileName);
			objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
			objNode.TryGetStringFieldQuickly("groupname", ref _strGroupName);
			objNode.TryGetBoolFieldQuickly("free", ref _blnFree);
			objNode.TryGetBoolFieldQuickly("group", ref _blnIsGroup);
			objNode.TryGetStringFieldQuickly("guid", ref _strUnique);
			objNode.TryGetBoolFieldQuickly("family", ref _blnFamily);
			objNode.TryGetBoolFieldQuickly("blackmail", ref _blnBlackmail);
			if (objNode["colour"] != null)
			{
                int intTmp = _objColour.ToArgb();
                if (objNode.TryGetInt32FieldQuickly("colour", ref intTmp))
                    _objColour = Color.FromArgb(intTmp);
			}

			if (objNode["readonly"] != null) _readonly = true;
		    objNode.TryGetBoolFieldQuickly("mademan", ref _blnMadeMan);
		}

		/// <summary>
		/// Print the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public void Print(XmlTextWriter objWriter)
		{
			objWriter.WriteStartElement("contact");
			objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("role", _strRole);
            objWriter.WriteElementString("location", _strLocation);
            if (IsGroup == false)
				objWriter.WriteElementString("connection", _intConnection.ToString());
			else
                objWriter.WriteElementString("connection", "Group(" + _intConnection.ToString() + ")");
			objWriter.WriteElementString("loyalty", _intLoyalty.ToString());
			objWriter.WriteElementString("type", LanguageManager.Instance.GetString("String_" + _objContactType.ToString()));
			objWriter.WriteElementString("mademan", _blnMadeMan.ToString());
			objWriter.WriteElementString("blackmail", _blnBlackmail.ToString());
			objWriter.WriteElementString("family", _blnFamily.ToString());
			if (_objCharacter.Options.PrintNotes)
				objWriter.WriteElementString("notes", _strNotes);
			objWriter.WriteEndElement();
		}
		#endregion

		#region Properties

		public bool ReadOnly
		{
			get
			{
				return _readonly;
			}
			set
			{
				_readonly = value; 
			}
		}


        /// <summary>
        /// Total points used for this contact.
        /// </summary>
        public int ContactPoints
        {
            get
            {
                if (Free) return 0;
                int intReturn = 0;
                if (Family) intReturn += 1;
                if (Blackmail) intReturn += 2;
                intReturn += _intConnection + _intLoyalty;
                return intReturn;
            }
        }

		/// <summary>
		/// Name of the Contact.
		/// </summary>
		public string Name
		{
			get
			{
				return _strName;
			}
			set
			{
				_strName = value;
			}
		}

        /// <summary>
        /// Role of the Contact.
        /// </summary>
        public string Role
        {
            get
            {
                return _strRole;
            }
            set
            {
                _strRole = value;
            }
        }

        /// <summary>
        /// Location of the Contact.
        /// </summary>
        public string Location
        {
            get
            {
                return _strLocation;
            }
            set
            {
                _strLocation = value;
            }
        }

        /// <summary>
		/// Contact's Connection Rating.
		/// </summary>
		public int Connection
		{
			get
			{
				return _intConnection;
			}
			set
			{
				_intConnection = value;
			}
		}

		/// <summary>
		/// Contact's Loyalty Rating (or Enemy's Incidence Rating).
		/// </summary>
		public int Loyalty
		{
			get
			{
				return _intLoyalty;
			}
			set
			{
				_intLoyalty = value;
			}
		}

		

        /// <summary>
        /// Is this contract a group contract?
        /// </summary>
	    public bool IsGroup
	    {
	        get
	        {
	            return _blnIsGroup;
	        }
            set
            {
                _blnIsGroup = value;

	            if (value)
	            {
		            _intLoyalty = 1;
	            }
            }
	    }

		

		/// <summary>
		/// The Contact's type, either Contact or Enemy.
		/// </summary>
		public ContactType EntityType
		{
			get
			{
				return _objContactType;
			}
			set
			{
				_objContactType = value;
			}
		}

		/// <summary>
		/// Name of the save file for this Contact.
		/// </summary>
		public string FileName
		{
			get
			{
				return _strFileName;
			}
			set
			{
				_strFileName = value;
			}
		}

		/// <summary>
		/// Relative path to the save file.
		/// </summary>
		public string RelativeFileName
		{
			get
			{
				return _strRelativeName;
			}
			set
			{
				_strRelativeName = value;
			}
		}

		/// <summary>
		/// Notes.
		/// </summary>
		public string Notes
		{
			get
			{
				return _strNotes;
			}
			set
			{
				_strNotes = value;
			}
		}

		/// <summary>
		/// Group Name.
		/// </summary>
		public string GroupName
		{
			get
			{
				return _strGroupName;
			}
			set
			{
				_strGroupName = value;
			}
		}

		/// <summary>
		/// Contact Colour.
		/// </summary>
		public Color Colour
		{
			get
			{
				return _objColour;
			}
			set
			{
				_objColour = value;
			}
		}

		/// <summary>
		/// Whether or not this is a free contact.
		/// </summary>
		public bool Free
		{
			get
			{
				return _blnFree;
			}
			set
			{
				_blnFree = value;
			}
		}
        /// <summary>
        /// Unique ID for this contact
        /// </summary>
	    public String GUID
	    {
            get
            {
                if (_strUnique == null)
                {
                    _strUnique = Guid.NewGuid().ToString();
                }

                return _strUnique;
            }
	    }

	    /// <summary>
	    /// Is this contact a made man
	    /// </summary>
	    public bool MadeMan
	    {
	        get
            {
                return _blnMadeMan;
            }
	        set
	        {
                _blnMadeMan = value;
		        if (value)
		        {
			        _intLoyalty = 3;
		        }
	        }
	    }

		public bool Blackmail
		{
			get { return _blnBlackmail; }
			set { _blnBlackmail = value; }
		}

		public bool Family
		{
			get { return _blnFamily; }
			set { _blnFamily = value; }
		}

		#endregion
	}

	/// <summary>
	/// A Critter Power.
	/// </summary>
	public class CritterPower : INamedItemWithGuid
	{
		private Guid _guiID = new Guid();
		private string _strName = string.Empty;
		private string _strCategory = string.Empty;
		private string _strType = string.Empty;
		private string _strAction = string.Empty;
		private string _strRange = string.Empty;
		private string _strDuration = string.Empty;
		private string _strExtra = string.Empty;
		private string _strSource = string.Empty;
		private string _strPage = string.Empty;
	    private int _intKarma = 0;
		private double _dblPowerPoints = 0.0;
		private XmlNode _nodBonus;
		private string _strNotes = string.Empty;
		private readonly Character _objCharacter;
		private bool _blnCountTowardsLimit = true;
	    private int _intRating;

	    #region Constructor, Create, Save, Load, and Print Methods
		public CritterPower(Character objCharacter)
		{
			// Create the GUID for the new Power.
			_guiID = Guid.NewGuid();
			_objCharacter = objCharacter;
		}

		/// Create a Critter Power from an XmlNode and return the TreeNodes for it.
		/// <param name="objXmlPowerNode">XmlNode to create the object from.</param>
		/// <param name="objCharacter">Character the Gear is being added to.</param>
		/// <param name="objNode">TreeNode to populate a TreeView.</param>
		/// <param name="intRating">Selected Rating for the Gear.</param>
		/// <param name="strForcedValue">Value to forcefully select for any ImprovementManager prompts.</param>
		public void Create(XmlNode objXmlPowerNode, Character objCharacter, TreeNode objNode, int intRating = 0, string strForcedValue = "")
		{
            objXmlPowerNode.TryGetStringFieldQuickly("name", ref _strName);
            _intRating = intRating;
            _nodBonus = objXmlPowerNode["bonus"];
            // If the piece grants a bonus, pass the information to the Improvement Manager.
            if (_nodBonus != null)
            {
                ImprovementManager objImprovementManager = new ImprovementManager(objCharacter);
                objImprovementManager.ForcedValue = strForcedValue;
                if (!objImprovementManager.CreateImprovements(Improvement.ImprovementSource.CritterPower, _guiID.ToString(), _nodBonus, true, intRating, DisplayNameShort))
                {
                    _guiID = Guid.Empty;
                    return;
                }
                if (!string.IsNullOrEmpty(objImprovementManager.SelectedValue))
                {
                    _strExtra = objImprovementManager.SelectedValue;
                    objNode.Text += " (" + objImprovementManager.SelectedValue + ")";
                }
                else if (intRating != 0)
                    _strExtra = intRating.ToString();
            }
            else if (intRating != 0)
                _strExtra = intRating.ToString();
            objXmlPowerNode.TryGetStringFieldQuickly("category", ref _strCategory);
            objXmlPowerNode.TryGetStringFieldQuickly("type", ref _strType);
            objXmlPowerNode.TryGetStringFieldQuickly("action", ref _strAction);
            objXmlPowerNode.TryGetStringFieldQuickly("range", ref _strRange);
            objXmlPowerNode.TryGetStringFieldQuickly("duration", ref _strDuration);
            objXmlPowerNode.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlPowerNode.TryGetStringFieldQuickly("page", ref _strPage);
		    objXmlPowerNode.TryGetInt32FieldQuickly("karma", ref _intKarma);

			// Create the TreeNode for the new item.
			objNode.Text = DisplayName;
			objNode.Tag = _guiID.ToString();
		}

		/// <summary>
		/// Save the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public void Save(XmlTextWriter objWriter)
		{
			objWriter.WriteStartElement("critterpower");
			objWriter.WriteElementString("guid", _guiID.ToString());
			objWriter.WriteElementString("name", _strName);
			objWriter.WriteElementString("extra", _strExtra);
            objWriter.WriteElementString("rating", _intRating.ToString());
            objWriter.WriteElementString("category", _strCategory);
			objWriter.WriteElementString("type", _strType);
			objWriter.WriteElementString("action", _strAction);
			objWriter.WriteElementString("range", _strRange);
			objWriter.WriteElementString("duration", _strDuration);
			objWriter.WriteElementString("source", _strSource);
			objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("karma", _intKarma.ToString());
            objWriter.WriteElementString("points", _dblPowerPoints.ToString(GlobalOptions.InvariantCultureInfo));
			objWriter.WriteElementString("counttowardslimit", _blnCountTowardsLimit.ToString());
			if (_nodBonus != null)
				objWriter.WriteRaw("<bonus>" + _nodBonus.InnerXml + "</bonus>");
			else
				objWriter.WriteElementString("bonus", string.Empty);
			objWriter.WriteElementString("notes", _strNotes);
			objWriter.WriteEndElement();
			_objCharacter.SourceProcess(_strSource);
		}

		/// <summary>
		/// Load the Critter Power from the XmlNode.
		/// </summary>
		/// <param name="objNode">XmlNode to load.</param>
		public void Load(XmlNode objNode)
		{
		    objNode.TryGetField("guid", Guid.TryParse, out _guiID);
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
            objNode.TryGetStringFieldQuickly("category", ref _strCategory);
            objNode.TryGetStringFieldQuickly("type", ref _strType);
            objNode.TryGetStringFieldQuickly("action", ref _strAction);
            objNode.TryGetStringFieldQuickly("range", ref _strRange);
            objNode.TryGetStringFieldQuickly("duration", ref _strDuration);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
		    objNode.TryGetDoubleFieldQuickly("points", ref _dblPowerPoints);
		    objNode.TryGetBoolFieldQuickly("counttowardslimit", ref _blnCountTowardsLimit);
			_nodBonus = objNode["bonus"];
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
		}

		/// <summary>
		/// Print the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public void Print(XmlTextWriter objWriter)
		{
			objWriter.WriteStartElement("critterpower");
			objWriter.WriteElementString("name", DisplayNameShort);
			objWriter.WriteElementString("extra", LanguageManager.Instance.TranslateExtra(_strExtra));
			objWriter.WriteElementString("category", DisplayCategory);
			objWriter.WriteElementString("type", DisplayType);
			objWriter.WriteElementString("action", DisplayAction);
			objWriter.WriteElementString("range", DisplayRange);
			objWriter.WriteElementString("duration", DisplayDuration);
			objWriter.WriteElementString("source", _objCharacter.Options.LanguageBookShort(_strSource));
			objWriter.WriteElementString("page", Page);
			if (_objCharacter.Options.PrintNotes)
				objWriter.WriteElementString("notes", _strNotes);
			objWriter.WriteEndElement();
		}
		#endregion

		#region Properties
		/// <summary>
		/// Internal identifier which will be used to identify this Critter Power in the Improvement system.
		/// </summary>
		public string InternalId
		{
			get
			{
				return _guiID.ToString();
			}
		}

        /// <summary>
        /// Paid levels of the power. 
        /// </summary>
	    public int Rating
	    {
	        get { return _intRating; }
	        set
	        {
	            if (Extra == Rating.ToString())
	            {
	                Extra = value.ToString();
	            }
	            _intRating = value;
	        }
	    }

        /// <summary>
        /// Total rating of the power, including any bonus levels from Improvements.
        /// </summary>
	    public int TotalRating
	    {
	        get
	        {
	            return _intRating + _objCharacter.Improvements.Where(objImprovement => objImprovement.ImprovedName == Name && objImprovement.ImproveType == Improvement.ImprovementType.CritterPowerLevel && objImprovement.Enabled).Sum(objImprovement => objImprovement.Rating);
	        }
	    }

		/// <summary>
		/// Power's name.
		/// </summary>
		public string Name
		{
			get
			{
				return _strName;
			}
			set
			{
				_strName = value;
			}
		}

		/// <summary>
		/// The name of the object as it should be displayed on printouts (translated name only).
		/// </summary>
		public string DisplayNameShort
		{
			get
			{
				string strReturn = _strName;
				// Get the translated name if applicable.
				if (GlobalOptions.Instance.Language != "en-us")
				{
					XmlDocument objXmlDocument = XmlManager.Instance.Load("critterpowers.xml");
					XmlNode objNode = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"" + _strName + "\"]");
					if (objNode != null)
					{
						if (objNode["translate"] != null)
							strReturn = objNode["translate"].InnerText;
					}
				}

				return strReturn;
			}
		}

		/// <summary>
		/// The name of the object as it should be displayed in lists. Name (Extra).
		/// </summary>
		public string DisplayName
		{
			get
			{
				string strReturn = DisplayNameShort;
				if (!string.IsNullOrEmpty(_strExtra))
				{
					LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
					// Attempt to retrieve the CharacterAttribute name.
					strReturn += " (" + LanguageManager.Instance.TranslateExtra(_strExtra) + ")";
				}

				return strReturn;
			}
		}

		/// <summary>
		/// Extra information that should be applied to the name, like a linked CharacterAttribute.
		/// </summary>
		public string Extra
		{
			get
			{
				return _strExtra;
			}
			set
			{
				_strExtra = value;
			}
		}

		/// <summary>
		/// Sourcebook.
		/// </summary>
		public string Source
		{
			get
			{
				return _strSource;
			}
			set
			{
				_strSource = value;
			}
		}

		/// <summary>
		/// Page Number.
		/// </summary>
		public string Page
		{
			get
			{
				string strReturn = _strPage;
				// Get the translated name if applicable.
				if (GlobalOptions.Instance.Language != "en-us")
				{
					XmlDocument objXmlDocument = XmlManager.Instance.Load("critterpowers.xml");
					XmlNode objNode = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"" + _strName + "\"]");
					if (objNode != null)
					{
						if (objNode["altpage"] != null)
							strReturn = objNode["altpage"].InnerText;
					}
				}

				return strReturn;
			}
			set
			{
				_strPage = value;
			}
		}

		/// <summary>
		/// Bonus node from the XML file.
		/// </summary>
		public XmlNode Bonus
		{
			get
			{
				return _nodBonus;
			}
			set
			{
				_nodBonus = value;
			}
		}

		/// <summary>
		/// Translated Category.
		/// </summary>
		public string DisplayCategory
		{
			get
			{
				string strReturn = _strCategory;
				// Get the translated name if applicable.
				if (GlobalOptions.Instance.Language != "en-us")
				{
					XmlDocument objXmlDocument = XmlManager.Instance.Load("critterpowers.xml");
					XmlNode objNode = objXmlDocument.SelectSingleNode("/chummer/categories/category[. = \"" + _strCategory + "\"]");
					if (objNode != null)
					{
						if (objNode.Attributes["translate"] != null)
							strReturn = objNode.Attributes["translate"].InnerText;
					}
				}

				return strReturn;
			}
		}

		/// <summary>
		/// Category.
		/// </summary>
		public string Category
		{
			get
			{
				return _strCategory;
			}
			set
			{
				_strCategory = value;
			}
		}

		/// <summary>
		/// Type.
		/// </summary>
		public string Type
		{
			get
			{
				return _strType;
			}
			set
			{
				_strType = value;
			}
		}

		/// <summary>
		/// Translated Type.
		/// </summary>
		public string DisplayType
		{
			get
			{
				string strReturn = string.Empty;

				switch (_strType)
				{
					case "M":
						strReturn = LanguageManager.Instance.GetString("String_SpellTypeMana");
						break;
					case "P":
						strReturn = LanguageManager.Instance.GetString("String_SpellTypePhysical");
						break;
					default:
						strReturn = string.Empty;
						break;
				}

				return strReturn;
			}
		}

		/// <summary>
		/// Action.
		/// </summary>
		public string Action
		{
			get
			{
				return _strAction;
			}
			set
			{
				_strAction = value;
			}
		}

		/// <summary>
		/// Translated Action.
		/// </summary>
		public string DisplayAction
		{
			get
			{
				string strReturn = string.Empty;

				switch (_strAction)
				{
					case "Auto":
						strReturn = LanguageManager.Instance.GetString("String_ActionAutomatic");
						break;
					case "Free":
						strReturn = LanguageManager.Instance.GetString("String_ActionFree");
						break;
					case "Simple":
						strReturn = LanguageManager.Instance.GetString("String_ActionSimple");
						break;
					case "Complex":
						strReturn = LanguageManager.Instance.GetString("String_ActionComplex");
						break;
					case "Special":
						strReturn = LanguageManager.Instance.GetString("String_SpellDurationSpecial");
						break;
				}

				return strReturn;
			}
		}

		/// <summary>
		/// Range.
		/// </summary>
		public string Range
		{
			get
			{
				return _strRange;
			}
			set
			{
				_strRange = value;
			}
		}

		/// <summary>
		/// Translated Range.
		/// </summary>
		public string DisplayRange
		{
			get
			{
				string strReturn = _strRange;
				strReturn = strReturn.Replace("Self", LanguageManager.Instance.GetString("String_SpellRangeSelf"));
				strReturn = strReturn.Replace("Special", LanguageManager.Instance.GetString("String_SpellDurationSpecial"));
				strReturn = strReturn.Replace("LOS", LanguageManager.Instance.GetString("String_SpellRangeLineOfSight"));
				strReturn = strReturn.Replace("LOI", LanguageManager.Instance.GetString("String_SpellRangeLineOfInfluence"));
				strReturn = strReturn.Replace("T", LanguageManager.Instance.GetString("String_SpellRangeTouch"));
				strReturn = strReturn.Replace("(A)", "(" + LanguageManager.Instance.GetString("String_SpellRangeArea") + ")");
				strReturn = strReturn.Replace("MAG", LanguageManager.Instance.GetString("String_AttributeMAGShort"));

				return strReturn;
			}
		}

		/// <summary>
		/// Duration.
		/// </summary>
		public string Duration
		{
			get
			{
				return _strDuration;
			}
			set
			{
				_strDuration = value;
			}
		}

		/// <summary>
		/// Translated Duration.
		/// </summary>
		public string DisplayDuration
		{
			get
			{
				string strReturn = string.Empty;

				switch (_strDuration)
				{
					case "Instant":
						strReturn = LanguageManager.Instance.GetString("String_SpellDurationInstantLong");
						break;
					case "Sustained":
						strReturn = LanguageManager.Instance.GetString("String_SpellDurationSustained");
						break;
					case "Always":
						strReturn = LanguageManager.Instance.GetString("String_SpellDurationAlways");
						break;
					case "Special":
						strReturn = LanguageManager.Instance.GetString("String_SpellDurationSpecial");
						break;
					default:
						strReturn = _strDuration;
						break;
				}

				return strReturn;
			}
		}

		/// <summary>
		/// Power Points used by the Critter Power (Free Spirits only).
		/// </summary>
		public double PowerPoints
		{
			get
			{
				return _dblPowerPoints;
			}
			set
			{
				_dblPowerPoints = value;
			}
		}

		/// <summary>
		/// Notes.
		/// </summary>
		public string Notes
		{
			get
			{
				return _strNotes;
			}
			set
			{
				_strNotes = value;
			}
		}

		/// <summary>
		/// Whether or not the Critter Power counts towards their total number of Critter Powers.
		/// </summary>
		public bool CountTowardsLimit
		{
			get
			{
				return _blnCountTowardsLimit;
			}
			set
			{
				_blnCountTowardsLimit = value;
			}
		}

        /// <summary>
        /// Karma that the Critter must pay to take the power.
        /// </summary>
	    public int Karma
	    {
	        get { return _intKarma; }
	        set { _intKarma = value; }
	    }
		#endregion
	}

	/// <summary>
	/// An Initiation Grade.
	/// </summary>
	public class InitiationGrade : IItemWithGuid
	{
		private Guid _guiID = new Guid();
		private bool _blnGroup = false;
		private bool _blnOrdeal = false;
        private bool _blnSchooling = false;
		private bool _blnTechnomancer = false;
		private int _intGrade = 0;
		private string _strNotes = string.Empty;

		private readonly CharacterOptions _objOptions;

		#region Constructor, Create, Save, and Load Methods
		public InitiationGrade(Character objCharacter)
		{
			// Create the GUID for the new InitiationGrade.
			_guiID = Guid.NewGuid();
			_objOptions = objCharacter.Options;
		}

		/// Create an Intiation Grade from an XmlNode and return the TreeNodes for it.
		/// <param name="intGrade">Grade number.</param>
		/// <param name="blnTechnomancer">Whether or not the character is a Technomancer.</param>
		/// <param name="blnGroup">Whether or not a Group was used.</param>
		/// <param name="blnOrdeal">Whether or not an Ordeal was used.</param>
        /// <param name="blnSchooling">Whether or not Schooling was used.</param>
		public void Create(int intGrade, bool blnTechnomancer, bool blnGroup, bool blnOrdeal, bool blnSchooling)
		{
			_intGrade = intGrade;
			_blnTechnomancer = blnTechnomancer;
			_blnGroup = blnGroup;
			_blnOrdeal = blnOrdeal;
            _blnSchooling = blnSchooling;
		}

		/// <summary>
		/// Save the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public void Save(XmlTextWriter objWriter)
		{
			objWriter.WriteStartElement("initiationgrade");
			objWriter.WriteElementString("guid", _guiID.ToString());
			objWriter.WriteElementString("res", _blnTechnomancer.ToString());
			objWriter.WriteElementString("grade", _intGrade.ToString());
			objWriter.WriteElementString("group", _blnGroup.ToString());
			objWriter.WriteElementString("ordeal", _blnOrdeal.ToString());
            objWriter.WriteElementString("schooling", _blnSchooling.ToString());
            objWriter.WriteElementString("notes", _strNotes);
			objWriter.WriteEndElement();
		}

		/// <summary>
		/// Load the Initiation Grade from the XmlNode.
		/// </summary>
		/// <param name="objNode">XmlNode to load.</param>
		public void Load(XmlNode objNode)
		{
            if (objNode.TryGetField("guid", Guid.TryParse, out _guiID))
                _guiID = Guid.NewGuid();
            objNode.TryGetBoolFieldQuickly("res", ref _blnTechnomancer);
            objNode.TryGetInt32FieldQuickly("grade", ref _intGrade);
            objNode.TryGetBoolFieldQuickly("group", ref _blnGroup);
            objNode.TryGetBoolFieldQuickly("ordeal", ref _blnOrdeal);
            objNode.TryGetBoolFieldQuickly("schooling", ref _blnSchooling);
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
		}
		#endregion

		#region Properties
		/// <summary>
		/// Internal identifier which will be used to identify this Initiation Grade in the Improvement system.
		/// </summary>
		public string InternalId
		{
			get
			{
				return _guiID.ToString();
			}
		}

		/// <summary>
		/// Initiate Grade.
		/// </summary>
		public int Grade
		{
			get
			{
				return _intGrade;
			}
			set
			{
				_intGrade = value;
			}
		}

		/// <summary>
		/// Whether or not a Group was used.
		/// </summary>
		public bool Group
		{
			get
			{
				return _blnGroup;
			}
			set
			{
				_blnGroup = value;
			}
		}

		/// <summary>
		/// Whether or not an Ordeal was used.
		/// </summary>
		public bool Ordeal
		{
			get
			{
				return _blnOrdeal;
			}
			set
			{
				_blnOrdeal = value;
			}
		}

        /// <summary>
        /// Whether or not Schooling was used.
        /// </summary>
        public bool Schooling
        {
            get
            {
                return _blnSchooling;
            }
            set
            {
                _blnSchooling = value;
            }
        }

        /// <summary>
		/// Whether or not the Initiation Grade is for a Technomancer.
		/// </summary>
		public bool Technomancer
		{
			get
			{
				return _blnTechnomancer;
			}
			set
			{
				_blnTechnomancer = value;
			}
		}
		#endregion

		#region Complex Properties
		/// <summary>
		/// The Initiation Grade's Karma cost.
		/// </summary>
		public int KarmaCost
		{
			get
			{
				double dblCost = 10.0 + (_intGrade * _objOptions.KarmaInitiation);
				double dblMultiplier = 1.0;
				
				// Discount for Group.
				if (_blnGroup)
					dblMultiplier -= 0.1;

				// Discount for Ordeal.
				if (_blnOrdeal)
					dblMultiplier -= 0.1;

                // Discount for Schooling.
                if (_blnSchooling)
                    dblMultiplier -= 0.1;

                return Convert.ToInt32(Math.Ceiling(dblCost * dblMultiplier));
			}
		}

		/// <summary>
		/// Text to display in the Initiation Grade list.
		/// </summary>
		public string Text
		{
			get
			{
				LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);

				string strReturn = LanguageManager.Instance.GetString("String_Grade") + " " + _intGrade.ToString();
				if (_blnGroup || _blnOrdeal)
				{
					strReturn += " (";
					if (_blnGroup)
					{
						if (_blnTechnomancer)
							strReturn += LanguageManager.Instance.GetString("String_Network");
						else
							strReturn += LanguageManager.Instance.GetString("String_Group");
						if (_blnOrdeal || _blnSchooling)
							strReturn += ", ";
					}
					if (_blnOrdeal)
					{
						if (_blnTechnomancer)
							strReturn += LanguageManager.Instance.GetString("String_Task");
						else
							strReturn += LanguageManager.Instance.GetString("String_Ordeal");
                        if (_blnSchooling)
                            strReturn += ", ";
                    }
                    if (_blnSchooling)
                    {
                        strReturn += LanguageManager.Instance.GetString("String_Schooling");
                    }
                    strReturn += ")";
				}
				
				return strReturn;
			}
		}

		/// <summary>
		/// Notes.
		/// </summary>
		public string Notes
		{
			get
			{
				return _strNotes;
			}
			set
			{
				_strNotes = value;
			}
		}
		#endregion
	}

	public class CalendarWeek : IItemWithGuid
	{
		private Guid _guiID = new Guid();
		private int _intYear = 2072;
		private int _intWeek = 1;
		private string _strNotes = string.Empty;

		#region Constructor, Save, Load, and Print Methods
		public CalendarWeek()
		{
			// Create the GUID for the new CalendarWeek.
			_guiID = Guid.NewGuid();
		}

		public CalendarWeek(int intYear, int intWeek)
		{
			// Create the GUID for the new CalendarWeek.
			_guiID = Guid.NewGuid();
			_intYear = intYear;
			_intWeek = intWeek;
		}

		/// <summary>
		/// Save the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public void Save(XmlTextWriter objWriter)
		{
			objWriter.WriteStartElement("week");
			objWriter.WriteElementString("guid", _guiID.ToString());
			objWriter.WriteElementString("year", _intYear.ToString());
			objWriter.WriteElementString("week", _intWeek.ToString());
			objWriter.WriteElementString("notes", _strNotes);
			objWriter.WriteEndElement();
		}

		/// <summary>
		/// Load the Calendar Week from the XmlNode.
		/// </summary>
		/// <param name="objNode">XmlNode to load.</param>
		public void Load(XmlNode objNode)
		{
			_guiID = Guid.Parse(objNode["guid"].InnerText);
			_intYear = Convert.ToInt32(objNode["year"].InnerText);
			_intWeek = Convert.ToInt32(objNode["week"].InnerText);
			_strNotes = objNode["notes"].InnerText;
		}

		/// <summary>
		/// Print the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public void Print(XmlTextWriter objWriter)
		{
			objWriter.WriteStartElement("week");
			objWriter.WriteElementString("year", _intYear.ToString());
			objWriter.WriteElementString("month", Month.ToString());
			objWriter.WriteElementString("week", MonthWeek.ToString());
			objWriter.WriteElementString("notes", _strNotes);
			objWriter.WriteEndElement();
		}
		#endregion

		#region Properties
		/// <summary>
		/// Internal identifier which will be used to identify this Calendar Week in the Improvement system.
		/// </summary>
		public string InternalId
		{
			get
			{
				return _guiID.ToString();
			}
		}

		/// <summary>
		/// Year.
		/// </summary>
		public int Year
		{
			get
			{
				return _intYear;
			}
			set
			{
				_intYear = value;
			}
		}

		/// <summary>
		/// Month.
		/// </summary>
		public int Month
		{
			get
			{
				switch (_intWeek)
				{
					case 1:
					case 2:
					case 3:
					case 4:
						return 1;
					case 5:
					case 6:
					case 7:
					case 8:
						return 2;
					case 9:
					case 10:
					case 11:
					case 12:
					case 13:
						return 3;
					case 14:
					case 15:
					case 16:
					case 17:
						return 4;
					case 18:
					case 19:
					case 20:
					case 21:
						return 5;
					case 22:
					case 23:
					case 24:
					case 25:
					case 26:
						return 6;
					case 27:
					case 28:
					case 29:
					case 30:
						return 7;
					case 31:
					case 32:
					case 33:
					case 34:
						return 8;
					case 35:
					case 36:
					case 37:
					case 38:
					case 39:
						return 9;
					case 40:
					case 41:
					case 42:
					case 43:
						return 10;
					case 44:
					case 45:
					case 46:
					case 47:
						return 11;
					default:
						return 12;
				}
			}
		}

		/// <summary>
		/// Week of the month.
		/// </summary>
		public int MonthWeek
		{
			get
			{
				switch (_intWeek)
				{
					case 1:
					case 5:
					case 9:
					case 14:
					case 18:
					case 22:
					case 27:
					case 31:
					case 35:
					case 40:
					case 44:
					case 48:
						return 1;
					case 2:
					case 6:
					case 10:
					case 15:
					case 19:
					case 23:
					case 28:
					case 32:
					case 36:
					case 41:
					case 45:
					case 49:
						return 2;
					case 3:
					case 7:
					case 11:
					case 16:
					case 20:
					case 24:
					case 29:
					case 33:
					case 37:
					case 42:
					case 46:
					case 50:
						return 3;
					case 4:
					case 8:
					case 12:
					case 17:
					case 21:
					case 25:
					case 30:
					case 34:
					case 38:
					case 43:
					case 47:
					case 51:
						return 4;
					default:
						return 5;
				}
			}
		}

		/// <summary>
		/// Month and Week to display.
		/// </summary>
		public string DisplayName
		{
			get
			{
				string strReturn = LanguageManager.Instance.GetString("String_WeekDisplay").Replace("{0}", _intYear.ToString()).Replace("{1}", Month.ToString()).Replace("{2}", MonthWeek.ToString());
				return strReturn;
			}
		}

		/// <summary>
		/// Week.
		/// </summary>
		public int Week
		{
			get
			{
				return _intWeek;
			}
			set
			{
				_intWeek = value;
			}
		}

		/// <summary>
		/// Notes.
		/// </summary>
		public string Notes
		{
			get
			{
				return _strNotes;
			}
			set
			{
				_strNotes = value;
			}
		}
		#endregion
	}

	public class MentorSpirit
	{
		private string _strName = string.Empty;
		private string _strAdvantages = string.Empty;

		#region Properties
		/// <summary>
		/// Name of the Mentor Spirit or Paragon.
		/// </summary>
		public string Name
		{
			get
			{
				return _strName;
			}
			set
			{
				_strName = value;
			}
		}

		/// <summary>
		/// Advantages and Disadvantages that the Mentor Spirit or Paragon grants.
		/// </summary>
		public string Advantages
		{
			get
			{
				return _strAdvantages;
			}
			set
			{
				_strAdvantages = value;
			}
		}
		#endregion
	}
}