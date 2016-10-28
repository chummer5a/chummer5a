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
		private string _strAbbrev = "";
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
			_strAbbrev = objNode["name"].InnerText;
			_intMetatypeMin = Convert.ToInt32(objNode["metatypemin"].InnerText);
			_intMetatypeMax = Convert.ToInt32(objNode["metatypemax"].InnerText);
			_intMetatypeAugMax = Convert.ToInt32(objNode["metatypeaugmax"].InnerText);
		    
            objNode.TryGetField("base", out _intBase);
		    objNode.TryGetField("karma", out _intKarma);
            
            Value = Convert.ToInt32(objNode["value"].InnerText);
			_intAugModifier = Convert.ToInt32(objNode["augmodifier"].InnerText);

            if (_intBase == 0)
                _intBase = Value;
		}

		/// <summary>
		/// Print the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public void Print(XmlTextWriter objWriter)
		{
			objWriter.WriteStartElement("attribute");
			objWriter.WriteElementString("name", _strAbbrev);
			objWriter.WriteElementString("base", Value.ToString());
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
				if (Value < value)
					Value = value;
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
				if (Value > value)
					Value = value;
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
						if (objImprovement.UniqueName != "" && objImprovement.ImproveType == Improvement.ImprovementType.Attribute && objImprovement.ImprovedName == _strAbbrev)
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
						if (objImprovement.UniqueName != "" && objImprovement.ImproveType == Improvement.ImprovementType.Attribute && objImprovement.ImprovedName == _strAbbrev)
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
						if (objImprovement.UniqueName != "" && objImprovement.ImproveType == Improvement.ImprovementType.Attribute && objImprovement.ImprovedName == _strAbbrev + "Base")
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
                        if (objCyberware.Category == "Cyberlimb" && objCyberware.LimbSlot != "")
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
		public int TotalValue
		{
			get
			{
				int intMeat = Value + AttributeModifiers;
				int intReturn = intMeat;

                //// If this is AGI or STR, factor in any Cyberlimbs.
                if ((_strAbbrev == "AGI" || _strAbbrev == "STR") && !_objCharacter.Options.DontUseCyberlimbCalculation)
                {
                    int intLimbTotal = 0;
                    int intLimbCount = 0;
                    foreach (Cyberware objCyberware in _objCharacter.Cyberware
						.Where(objCyberware => objCyberware.Category == "Cyberlimb")
						.Where(objCyberware => !string.IsNullOrWhiteSpace(objCyberware.LimbSlot) && !_objCharacter.Options.ExcludeLimbSlot.Contains(objCyberware.LimbSlot)))
                    {
	                    intLimbCount++;
	                    switch (_strAbbrev)
	                    {
		                    case "STR":
			                    intLimbTotal += objCyberware.TotalStrength;
			                    break;
		                    default:
			                    intLimbTotal += objCyberware.TotalAgility;
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
                        int intTotal = Convert.ToInt32(Math.Floor(Convert.ToDecimal((intLimbTotal), GlobalOptions.Instance.CultureInfo) / Convert.ToDecimal(intLimbCount, GlobalOptions.Instance.CultureInfo)));
                        intReturn += intTotal;
                    }
                }

				// Do not let the CharacterAttribute go above the Metatype's Augmented Maximum.
				if (intReturn > TotalAugmentedMaximum)
					intReturn = TotalAugmentedMaximum;

				// An Attribute cannot go below 1 unless it is EDG, MAG, or RES, the character is a Critter, or the Metatype Maximum is 0.
				if (_objCharacter.CritterEnabled || _strAbbrev == "EDG" || _intMetatypeMax == 0 || (_objCharacter.EssencePenalty != 0 && (_strAbbrev == "MAG" || _strAbbrev == "RES")) || (_objCharacter.MetatypeCategory != "A.I." && _strAbbrev == "DEP"))
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
						else
							intReturn = 1;
					}
					else
						intReturn = Math.Max(_intMetatypeMin - _objCharacter.EssencePenalty, 0);
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
				if (_strAbbrev == "EDG" || _strAbbrev == "MAG" || _strAbbrev == "RES")
					intReturn = TotalMaximum + AugmentedMaximumModifiers;
				else
					intReturn = TotalMaximum + 4 + AugmentedMaximumModifiers;
                    // intReturn = TotalMaximum + Convert.ToInt32(Math.Floor((Convert.ToDecimal(TotalMaximum, GlobalOptions.Instance.CultureInfo) / 2))) + AugmentedMaximumModifiers;

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
			MetatypeMinimum = Convert.ToInt32(strMin);
			MetatypeMaximum = Convert.ToInt32(strMax);
			MetatypeAugmentedMaximum = Convert.ToInt32(strAug);
		}

		/// <summary>
		/// ToolTip that shows how the CharacterAttribute is calculating its Modified Rating.
		/// </summary>
		public string ToolTip()
		{
			string strReturn = "";
			strReturn += _strAbbrev + " (" + Value.ToString() + ")";
			string strModifier = "";

			List<string> lstUniqueName = new List<string>();
			List<string[,]> lstUniquePair = new List<string[,]>();
			foreach (Improvement objImprovement in _objCharacter.Improvements)
			{
				if (objImprovement.Enabled && !objImprovement.Custom)
				{
					if (objImprovement.UniqueName != "" && objImprovement.ImproveType == Improvement.ImprovementType.Attribute && objImprovement.ImprovedName == _strAbbrev)
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
				strModifier = "";
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
					if (objImprovement.UniqueName != "" && objImprovement.ImproveType == Improvement.ImprovementType.Attribute && objImprovement.ImprovedName == _strAbbrev)
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
        Entertainment = 3
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
	public enum QualityFailureReason : uint
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
	public class Quality
	{
		private Guid _guiID = new Guid();
		private string _strName = "";
        public string _strMetagenetic = "";
		private string _strExtra = "";
		private string _strSource = "";
		private string _strPage = "";
		private string _strMutant = "";
		private string _strNotes = "";
		private bool _blnImplemented = true;
		private bool _blnContributeToLimit = true;
		private bool _blnPrint = true;
		private int _intBP = 0;
        private int _intLP = 0;
		private QualityType _objQualityType = QualityType.Positive;
		private QualitySource _objQualitySource = QualitySource.Selected;
		private XmlNode _nodBonus;
		private readonly Character _objCharacter;
		private string _strAltName = "";
		private string _strAltPage = "";
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
			_strName = objXmlQuality["name"].InnerText;
            if (objXmlQuality["metagenetic"] != null)
            {
                _strMetagenetic = objXmlQuality["metagenetic"].InnerText;
            }
			// Check for a Variable Cost.
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
            if (objXmlQuality["lp"] != null)
            {
                _intLP = Convert.ToInt32(objXmlQuality["lp"].InnerText);
            }
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
			_strSource = objXmlQuality["source"].InnerText;
			_strPage = objXmlQuality["page"].InnerText;
			if (objXmlQuality["mutant"] != null)
				_strMutant = "yes";

			if (_objQualityType == QualityType.LifeModule)
			{
				objXmlQuality.TryGetField("stage", out _stage);
			}

            if(objXmlQuality["id"] != null)
                _qualiyGuid = Guid.Parse(objXmlQuality["id"].InnerText);

			if (GlobalOptions.Instance.Language != "en-us")
			{
				XmlDocument objXmlDocument = XmlManager.Instance.Load("qualities.xml");
				XmlNode objQualityNode = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + _strName + "\"]");
				if (objQualityNode != null)
				{
					if (objQualityNode["translate"] != null)
						_strAltName = objQualityNode["translate"].InnerText;
					if (objQualityNode["altpage"] != null)
						_strAltPage = objQualityNode["altpage"].InnerText;
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
					objWeapon.Name = objXmlNaturalWeapon["name"].InnerText;
					objWeapon.Category = LanguageManager.Instance.GetString("Tab_Critter");
					objWeapon.WeaponType = "Melee";
					objWeapon.Reach = Convert.ToInt32(objXmlNaturalWeapon["reach"].InnerText);
				    objWeapon.Accuracy = objXmlNaturalWeapon["accuracy"].InnerText;
					objWeapon.Damage = objXmlNaturalWeapon["damage"].InnerText; ;
					objWeapon.AP = objXmlNaturalWeapon["ap"].InnerText; ;
					objWeapon.Mode = "0";
					objWeapon.RC = "0";
					objWeapon.Concealability = 0;
					objWeapon.Avail = "0";
					objWeapon.Cost = 0;
					objWeapon.UseSkill = objXmlNaturalWeapon["useskill"].InnerText;
					objWeapon.Source = objXmlNaturalWeapon["source"].InnerText;
					objWeapon.Page = objXmlNaturalWeapon["page"].InnerText;
					objGearWeaponNode.ForeColor = SystemColors.GrayText;
					objGearWeaponNode.Text = objWeapon.Name;
					objGearWeaponNode.Tag = objWeapon.InternalId;
					objWeaponNodes.Add(objGearWeaponNode);

					_objCharacter.Weapons.Add(objWeapon);
				}
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
				if (objImprovementManager.SelectedValue != "")
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
				objWriter.WriteElementString("metagenetic", _strMetagenetic.ToString());
			}
			objWriter.WriteElementString("print", _blnPrint.ToString());
			objWriter.WriteElementString("qualitytype", _objQualityType.ToString());
			objWriter.WriteElementString("qualitysource", _objQualitySource.ToString());
			if (_strMutant != "")
				objWriter.WriteElementString("mutant", _strMutant);
			objWriter.WriteElementString("source", _strSource);
			objWriter.WriteElementString("page", _strPage);
			if (_nodBonus != null)
				objWriter.WriteRaw("<bonus>" + _nodBonus.InnerXml + "</bonus>");
			else
				objWriter.WriteElementString("bonus", "");
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
			_guiID = Guid.Parse(objNode["guid"].InnerText);
			_strName = objNode["name"].InnerText;
			_strExtra = objNode["extra"].InnerText;
			_intBP = Convert.ToInt32(objNode["bp"].InnerText);
			//_blnImplemented = Convert.ToBoolean(objNode["implemented"].InnerText);

			objNode.TryPreserveField("implemented", ref _blnImplemented);
			_blnContributeToLimit = Convert.ToBoolean(objNode["contributetolimit"].InnerText);
			_blnPrint = Convert.ToBoolean(objNode["print"].InnerText);
			_objQualityType = ConvertToQualityType(objNode["qualitytype"].InnerText);
			_objQualitySource = ConvertToQualitySource(objNode["qualitysource"].InnerText);
			objNode.TryGetField("metagenetic", out _strMetagenetic);
			objNode.TryGetField("mutant", out _strMutant);
			_strSource = objNode["source"].InnerText;
			_strPage = objNode["page"].InnerText;
			_nodBonus = objNode["bonus"];
			try
			{
				_guiWeaponID = Guid.Parse(objNode["weaponguid"].InnerText);
			}
			catch
			{
			}
			objNode.TryGetField("notes", out _strNotes);

			if (_objQualityType == QualityType.LifeModule)
			{
				objNode.TryGetField("stage", out _stage);
			}
			if (objNode["id"] != null)
			{
				Guid.TryParse(objNode["id"].InnerText, out _qualiyGuid);
			}

			if (GlobalOptions.Instance.Language != "en-us")
			{
				XmlDocument objXmlDocument = XmlManager.Instance.Load("qualities.xml");
				XmlNode objQualityNode = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + _strName + "\"]");
				if (objQualityNode != null)
				{
					if (objQualityNode["translate"] != null)
						_strAltName = objQualityNode["translate"].InnerText;
					if (objQualityNode["altpage"] != null)
						_strAltPage = objQualityNode["altpage"].InnerText;
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
				string strReturn = _strPage;
				if (_strAltPage != string.Empty)
					strReturn = _strAltPage;

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
			get
			{
				return _intBP;
			}
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
				string strReturn = _strName;
				if (_strAltName != string.Empty)
					strReturn = _strAltName;

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

				if (_strExtra != "")
				{
					LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
					// Attempt to retrieve the CharacterAttribute name.
					try
					{  //TODO Getstring dictionary check instead of clusterfuck used
						if (LanguageManager.Instance.GetString("String_Attribute" + _strExtra + "Short") != "")
							strReturn += " (" + LanguageManager.Instance.GetString("String_Attribute" + _strExtra + "Short") + ")";
						else
							strReturn += " (" + LanguageManager.Instance.TranslateExtra(_strExtra) + ")";
					}
					catch
					{
						strReturn += " (" + LanguageManager.Instance.TranslateExtra(_strExtra) + ")";
					}
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
		/// Number of points a Quality counts as for a Mutant Critter.
		/// </summary>
		public int MutantPoints
		{
			get
			{
				int intReturn = 0;

				if (_strMutant == "yes")
				{
					if (_strName.Contains("Rating 1"))
					{
						if (_objQualityType == QualityType.Positive)
							intReturn = 1;
						else
							intReturn = -1;
					}
					else if (_strName.Contains("Rating 2"))
					{
						if (_objQualityType == QualityType.Positive)
							intReturn = 2;
						else
							intReturn = -2;
					}
					else if (_strName.Contains("Rating 3"))
					{
						if (_objQualityType == QualityType.Positive)
							intReturn = 3;
						else
							intReturn = -3;
					}
					else if (_strName.Contains("Rating 4"))
					{
						if (_objQualityType == QualityType.Positive)
							intReturn = 4;
						else
							intReturn = -4;
					}
					else
					{
						if (_objQualityType == QualityType.Positive)
							intReturn = 1;
						else
							intReturn = -1;
					}
				}

				return intReturn;
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
			if (!objXmlQuality.TryCheckValue("limit", "no"))
			{
				foreach (Quality objQuality in objCharacter.Qualities)
				{
					if (objQuality.QualityId == objXmlQuality["id"].InnerText)
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
			XmlNode parrentNode = n.SelectSingleNode("../..");
			XmlNode sourceNode = null;
			if (parrentNode != null && parrentNode["id"] != null)
			{
				sourceNode = GetNodeOverrideable(parrentNode);
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
						else
						{ }
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
		private string _strName = "";
		private string _strCritterName = "";
		private int _intServicesOwed = 0;
		private SpiritType _objEntityType = SpiritType.Spirit;
		private bool _blnBound = true;
		private int _intForce = 1;
		private string _strFileName = "";
		private string _strRelativeName = "";
		private string _strNotes = "";
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
			_strName = objNode["name"].InnerText;
			objNode.TryGetField("crittername", out _strCritterName);
			_intServicesOwed = Convert.ToInt32(objNode["services"].InnerText);
			objNode.TryGetField("force", out _intForce);
			objNode.TryGetField("bound", out _blnBound);
			_objEntityType = ConvertToSpiritType(objNode["type"].InnerText);
			objNode.TryGetField("file", out _strFileName);
			objNode.TryGetField("relative", out _strRelativeName);
			objNode.TryGetField("notes", out _strNotes);
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
					String strInner;
					if (objXmlCritterNode.TryGetField(Attribute, out strInner))
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
						String attrName = objXmlSkillNode.Attributes["attr"].Value;
						int attr = attributes.ContainsKey(attrName) ? attributes[attrName] : _intForce;
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
				String source;
				String page;

				if (objXmlCritterNode.TryGetField("source", out source))
					objWriter.WriteElementString("source", source);
				if (objXmlCritterNode.TryGetField("page", out page))
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
			XmlNode objXmlPowerNode;
			string strSource = "";
			string strPage = "";
			objXmlPowerNode = objXmlDocument.SelectSingleNode("/chummer/powers/power[name=\"" + strPowerName + "\"]");
			if (objXmlPowerNode == null)
				objXmlPowerNode = objXmlDocument.SelectSingleNode("/chummer/powers/power[starts-with(\"" + strPowerName + "\", name)]");
			if (objXmlPowerNode != null)
			{
				objXmlPowerNode.TryGetField("source", out strSource);
				objXmlPowerNode.TryGetField("page", out strPage);
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
	public class Spell
	{
		private Guid _guiID = new Guid();
		private string _strName = "";
		private string _strDescriptors = "";
		private string _strCategory = "";
		private string _strType = "";
		private string _strRange = "";
		private string _strDamage = "";
		private string _strDuration = "";
		private string _strDV = "";
		private string _strSource = "";
		private string _strPage = "";
		private string _strExtra = "";
		private bool _blnLimited = false;
		private bool _blnExtended = false;
		private string _strNotes = "";
		private readonly Character _objCharacter;
		private string _strAltName = "";
		private string _strAltCategory = "";
		private string _strAltPage = "";
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
			_strName = objXmlSpellNode["name"].InnerText;
			_strDescriptors = objXmlSpellNode["descriptor"].InnerText;
			_strCategory = objXmlSpellNode["category"].InnerText;
			_strType = objXmlSpellNode["type"].InnerText;
			_strRange = objXmlSpellNode["range"].InnerText;
			_strDamage = objXmlSpellNode["damage"].InnerText;
			_strDuration = objXmlSpellNode["duration"].InnerText;
			_strDV = objXmlSpellNode["dv"].InnerText;
			_blnLimited = blnLimited;
			_blnExtended = blnExtended;
            _blnAlchemical = blnAlchemical;
            _strSource = objXmlSpellNode["source"].InnerText;
			_strPage = objXmlSpellNode["page"].InnerText;
            _objImprovementSource = objSource;

            string strDV = _strDV;
            if (_blnLimited && _strDV.StartsWith("F"))
            {
                int intPos = 0;
                if (strDV.Contains("-"))
                {
                    intPos = strDV.IndexOf("-") + 1;
                    string strAfter = strDV.Substring(intPos, strDV.Length - intPos);
                    strDV = strDV.Substring(0, intPos);
                    int intAfter = Convert.ToInt32(strAfter);
                    intAfter += 2;
                    strDV += intAfter.ToString();
                }
                else if (strDV.Contains("+"))
                {
                    intPos = strDV.IndexOf("+");
                    string strAfter = strDV.Substring(intPos, strDV.Length - intPos);
                    strDV = strDV.Substring(0, intPos);
                    int intAfter = Convert.ToInt32(strAfter);
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
            }
            _strDV = strDV;

			ImprovementManager objImprovementManager = new ImprovementManager(objCharacter);
			objImprovementManager.ForcedValue = strForcedValue;

			if (GlobalOptions.Instance.Language != "en-us")
			{
				XmlDocument objXmlDocument = XmlManager.Instance.Load("spells.xml");
				XmlNode objSpellNode = objXmlDocument.SelectSingleNode("/chummer/spells/spell[name = \"" + _strName + "\"]");
				if (objSpellNode != null)
				{
					if (objSpellNode["translate"] != null)
						_strAltName = objSpellNode["translate"].InnerText;
					if (objSpellNode["altpage"] != null)
						_strAltPage = objSpellNode["altpage"].InnerText;
				}

				objSpellNode = objXmlDocument.SelectSingleNode("/chummer/categories/category[. = \"" + _strCategory + "\"]");
				if (objSpellNode != null)
				{
					if (objSpellNode.Attributes["translate"] != null)
						_strAltCategory = objSpellNode.Attributes["translate"].InnerText;
				}
			}

			if (objXmlSpellNode["bonus"] != null)
			{
				if (!objImprovementManager.CreateImprovements(Improvement.ImprovementSource.Spell, _guiID.ToString(), objXmlSpellNode["bonus"], false, 1, DisplayNameShort))
				{
					_guiID = Guid.Empty;
					return;
				}
				if (objImprovementManager.SelectedValue != "")
				{
					_strExtra = objImprovementManager.SelectedValue;
				}
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
            _guiID = Guid.Parse(objNode["guid"].InnerText);
			_strName = objNode["name"].InnerText;
			_strDescriptors = objNode["descriptors"].InnerText;
			_strCategory = objNode["category"].InnerText;
			_strType = objNode["type"].InnerText;
			_strRange = objNode["range"].InnerText;
			_strDamage = objNode["damage"].InnerText;
			_strDuration = objNode["duration"].InnerText;
            try
            {
                _objImprovementSource = objImprovement.ConvertToImprovementSource(objNode["improvementsource"].InnerText);
            }
            catch { }
            try
            {
                _intGrade = Convert.ToInt32(objNode["grade"].InnerText);
            }
            catch { }
            _strDV = objNode["dv"].InnerText;
			try
			{
				_blnLimited = Convert.ToBoolean(objNode["limited"].InnerText);
			}
			catch
			{
			}
			try
			{
				_blnExtended = Convert.ToBoolean(objNode["extended"].InnerText);
			}
			catch
			{
			}
            try
            {
                _blnAlchemical = Convert.ToBoolean(objNode["alchemical"].InnerText);
            }
            catch
            {
            }
            _strSource = objNode["source"].InnerText;
			try
			{
				_strPage = objNode["page"].InnerText;
			}
			catch
			{
			}

			try
			{
				_strExtra = objNode["extra"].InnerText;
			}
			catch
			{
			}
			try
			{
				_strNotes = objNode["notes"].InnerText;
			}
			catch
			{
			}

			if (GlobalOptions.Instance.Language != "en-us")
			{
				XmlDocument objXmlDocument = XmlManager.Instance.Load("spells.xml");
				XmlNode objSpellNode = objXmlDocument.SelectSingleNode("/chummer/spells/spell[name = \"" + _strName + "\"]");
				if (objSpellNode != null)
				{
					if (objSpellNode["translate"] != null)
						_strAltName = objSpellNode["translate"].InnerText;
					if (objSpellNode["altpage"] != null)
						_strAltPage = objSpellNode["altpage"].InnerText;
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
				string strReturn = "";
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
				if (strReturn != string.Empty)
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
				string strReturn = _strCategory;
				if (_strAltCategory != string.Empty)
					strReturn = _strAltCategory;

				return strReturn;
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
				string strReturn = "";

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
				string strReturn = _strDV.Replace("/", "÷");
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
                //    if (strAfter == string.Empty)
                //        strAfter = "+2";
                //    else
                //    {
                //        int intValue = Convert.ToInt32(strAfter) + 2;
                //        if (intValue == 0)
                //            strAfter = "";
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
						intMAG = _objCharacter.MAGMagician;
				}

				XmlDocument objXmlDocument = new XmlDocument();
				XPathNavigator nav = objXmlDocument.CreateNavigator();
				XPathExpression xprDV;

				try
				{
					for (int i = 1; i <= intMAG * 2; i++)
					{
						// Calculate the Spell's Drain for the current Force.
						xprDV = nav.Compile(_strDV.Replace("F", i.ToString()).Replace("/", " div "));
						decimal decDV = Convert.ToDecimal(nav.Evaluate(xprDV).ToString());
						decDV = Math.Floor(decDV);
						int intDV = Convert.ToInt32(decDV);
						// Drain cannot be lower than 2.
						if (intDV < 2)
							intDV = 2;
						strTip += "\n   " + LanguageManager.Instance.GetString("String_Force") + " " + i.ToString() + ": " + intDV.ToString();
					}
				}
				catch
				{
					strTip = LanguageManager.Instance.GetString("Tip_SpellDrainSeeDescription");
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
				string strReturn = "";

				switch (_strDamage)
				{
					case "P":
						strReturn = LanguageManager.Instance.GetString("String_DamagePhysical");
						break;
					case "S":
						strReturn = LanguageManager.Instance.GetString("String_DamageStun");
						break;
					default:
						strReturn = "";
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
				string strReturn = "";

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
				string strReturn = _strPage;
				if (_strAltPage != string.Empty)
					strReturn = _strAltPage;

				return strReturn;
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
				if (_strAltName != string.Empty)
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
                if (_strExtra != "")
				{
					// Attempt to retrieve the CharacterAttribute name.
					try
					{
						if (LanguageManager.Instance.GetString("String_Attribute" + _strExtra + "Short") != "")
							strReturn += " (" + LanguageManager.Instance.GetString("String_Attribute" + _strExtra + "Short") + ")";
						else
							strReturn += " (" + LanguageManager.Instance.TranslateExtra(_strExtra) + ")";
					}
					catch
					{
						strReturn += " (" + LanguageManager.Instance.TranslateExtra(_strExtra) + ")";
					}
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
				string strReturn = "";

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
	public class Focus
	{
		private Guid _guiID = new Guid();
		private string _strName = "";
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
					int intPosition = strFocusName.IndexOf("(");
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
				string strReturn = "";
				foreach (Gear objGear in _lstGear)
					strReturn += objGear.DisplayName + ", ";

				// Remove the trailing comma.
				if (strReturn != string.Empty)
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
	public class Metamagic
	{
		private Guid _guiID = new Guid();
		private string _strName = "";
		private string _strSource = "";
		private string _strPage = "";
		private bool _blnPaidWithKarma = false;
        private int _intGrade = 0;
		private XmlNode _nodBonus;
		private Improvement.ImprovementSource _objImprovementSource = Improvement.ImprovementSource.Metamagic;
		private string _strNotes = "";

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
			_strName = objXmlMetamagicNode["name"].InnerText;
			_strSource = objXmlMetamagicNode["source"].InnerText;
			_strPage = objXmlMetamagicNode["page"].InnerText;
			_objImprovementSource = objSource;
            try
            {
                _intGrade = Convert.ToInt32(objXmlMetamagicNode["grade"].InnerText);
            }
            catch { }
			if (objXmlMetamagicNode.InnerXml.Contains("<bonus>"))
			{
				_nodBonus = objXmlMetamagicNode["bonus"];

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
				if (objImprovementManager.SelectedValue != "")
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
				objWriter.WriteElementString("bonus", "");
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
			_guiID = Guid.Parse(objNode["guid"].InnerText);
			_strName = objNode["name"].InnerText;
			_strSource = objNode["source"].InnerText;
			_strPage = objNode["page"].InnerText;
			try
			{
				_blnPaidWithKarma = Convert.ToBoolean(objNode["paidwithkarma"].InnerText);
			}
			catch
			{
			}
            try
            {
                _intGrade = Convert.ToInt32(objNode["grade"].InnerText);
            }
            catch
            {
            }

			_nodBonus = objNode["bonus"];
			_objImprovementSource = objImprovement.ConvertToImprovementSource(objNode["improvementsource"].InnerText);

			try
			{
				_strNotes = objNode["notes"].InnerText;
			}
			catch
			{
			}
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
					string strXmlFile = "";
					string strXPath = "";
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
					string strXmlFile = "";
					string strXPath = "";
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
    public class Art
    {
        private Guid _guiID = new Guid();
        private string _strName = "";
        private string _strSource = "";
        private string _strPage = "";
        private XmlNode _nodBonus;
        private int _intGrade = 0;
        private Improvement.ImprovementSource _objImprovementSource = Improvement.ImprovementSource.Art;
        private string _strNotes = "";

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
            _strName = objXmlArtNode["name"].InnerText;
            _strSource = objXmlArtNode["source"].InnerText;
            _strPage = objXmlArtNode["page"].InnerText;
            _objImprovementSource = objSource;
            try
            {
                _intGrade = Convert.ToInt32(objXmlArtNode["grade"].InnerText);
            }
            catch { }
            if (objXmlArtNode.InnerXml.Contains("<bonus>"))
            {
                _nodBonus = objXmlArtNode["bonus"];

                ImprovementManager objImprovementManager = new ImprovementManager(objCharacter);
                if (!objImprovementManager.CreateImprovements(objSource, _guiID.ToString(), _nodBonus, true, 1, DisplayNameShort))
                {
                    _guiID = Guid.Empty;
                    return;
                }
                if (objImprovementManager.SelectedValue != "")
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
                objWriter.WriteElementString("bonus", "");
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
            _guiID = Guid.Parse(objNode["guid"].InnerText);
            _strName = objNode["name"].InnerText;
            _strSource = objNode["source"].InnerText;
            _strPage = objNode["page"].InnerText;
            _nodBonus = objNode["bonus"];
            _objImprovementSource = objImprovement.ConvertToImprovementSource(objNode["improvementsource"].InnerText);

            try
            {
                _intGrade = Convert.ToInt32(objNode["grade"].InnerText);
            }
            catch
            {
            }
            try
            {
                _strNotes = objNode["notes"].InnerText;
            }
            catch
            {
            }
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
    public class Enhancement
    {
        private Guid _guiID = new Guid();
        private string _strName = "";
        private string _strSource = "";
        private string _strPage = "";
        private XmlNode _nodBonus;
        private int _intGrade = 0;
        private Improvement.ImprovementSource _objImprovementSource = Improvement.ImprovementSource.Enhancement;
        private string _strNotes = "";
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
            _strName = objXmlArtNode["name"].InnerText;
            _strSource = objXmlArtNode["source"].InnerText;
            _strPage = objXmlArtNode["page"].InnerText;
            _objImprovementSource = objSource;
            try
            {
                _intGrade = Convert.ToInt32(objXmlArtNode["grade"].InnerText);
            }
            catch { }
            if (objXmlArtNode.InnerXml.Contains("<bonus>"))
            {
                _nodBonus = objXmlArtNode["bonus"];

                ImprovementManager objImprovementManager = new ImprovementManager(objCharacter);
                if (!objImprovementManager.CreateImprovements(objSource, _guiID.ToString(), _nodBonus, true, 1, DisplayNameShort))
                {
                    _guiID = Guid.Empty;
                    return;
                }
                if (objImprovementManager.SelectedValue != "")
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
                objWriter.WriteElementString("bonus", "");
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
            _guiID = Guid.Parse(objNode["guid"].InnerText);
            _strName = objNode["name"].InnerText;
            _strSource = objNode["source"].InnerText;
            _strPage = objNode["page"].InnerText;
            _nodBonus = objNode["bonus"];
            _objImprovementSource = objImprovement.ConvertToImprovementSource(objNode["improvementsource"].InnerText);

            try
            {
                _intGrade = Convert.ToInt32(objNode["grade"].InnerText);
            }
            catch
            {
            }
            try
            {
                _strNotes = objNode["notes"].InnerText;
            }
            catch
            {
            }
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
	public class Power
	{
		private Guid _guiID = new Guid();
		private string _strName = "";
		private string _strExtra = "";
		private string _strSource = "";
		private string _strPage = "";
		private decimal _decPointsPerLevel = 0;
		private decimal _intRating = 1;
		private bool _blnLevelsEnabled = false;
		private int _intMaxLevel = 0;
		private bool _blnDiscountedAdeptWay = false;
		private bool _blnDiscountedGeas = false;
		private XmlNode _nodBonus;
		private string _strNotes = "";
		private bool _blnDoubleCost = true;
        private bool _blnFree = false;
        private int _intFreeLevels = 0;
        private decimal _decAdeptWayDiscount = 0;
        private string _strBonusSource = "";
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
			objWriter.WriteElementString("pointsperlevel", _decPointsPerLevel.ToString(GlobalOptions.Instance.CultureInfo));
            objWriter.WriteElementString("adeptway", _decAdeptWayDiscount.ToString(GlobalOptions.Instance.CultureInfo));
            objWriter.WriteElementString("rating", _intRating.ToString());
			objWriter.WriteElementString("levels", _blnLevelsEnabled.ToString());
			objWriter.WriteElementString("maxlevel", _intMaxLevel.ToString());
			objWriter.WriteElementString("discounted", _blnDiscountedAdeptWay.ToString());
			objWriter.WriteElementString("discountedgeas", _blnDiscountedGeas.ToString());
            objWriter.WriteElementString("bonussource", _strBonusSource);
            objWriter.WriteElementString("freepoints", _decFreePoints.ToString());
            objWriter.WriteElementString("source", _strSource);
			objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("free", _blnFree.ToString());
            objWriter.WriteElementString("freelevels", _intFreeLevels.ToString());
            objWriter.WriteElementString("doublecost", _blnDoubleCost.ToString());
			if (_nodBonus != null)
				objWriter.WriteRaw("<bonus>" + _nodBonus.InnerXml + "</bonus>");
			else
				objWriter.WriteElementString("bonus", "");
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
			_guiID = Guid.Parse(objNode["guid"].InnerText);
			_strName = objNode["name"].InnerText;
			_strExtra = objNode["extra"].InnerText;
			_decPointsPerLevel = Convert.ToDecimal(objNode["pointsperlevel"].InnerText, GlobalOptions.Instance.CultureInfo);
            if (objNode["adeptway"] != null)
                _decAdeptWayDiscount = Convert.ToDecimal(objNode["adeptway"].InnerText, GlobalOptions.Instance.CultureInfo);
            else
            {
                string strPowerName = _strName;
                if (strPowerName.Contains("("))
                    strPowerName = strPowerName.Substring(0, strPowerName.IndexOf("(") - 1);
                XmlDocument objXmlDocument = XmlManager.Instance.Load("powers.xml");
                XmlNode objXmlPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[starts-with(./name,\"" + strPowerName + "\")]");
                _decAdeptWayDiscount = Convert.ToDecimal(objXmlPower["adeptway"].InnerText, GlobalOptions.Instance.CultureInfo);
            }
            _intRating = Convert.ToInt32(objNode["rating"].InnerText);
			_blnLevelsEnabled = Convert.ToBoolean(objNode["levels"].InnerText);
            _blnFree = Convert.ToBoolean(objNode["free"].InnerText);
            _intFreeLevels = Convert.ToInt32(objNode["freelevels"].InnerText);
            _intMaxLevel = Convert.ToInt32(objNode["maxlevel"].InnerText);

			try
			{
				_blnDiscountedAdeptWay = Convert.ToBoolean(objNode["discounted"].InnerText);
			}
			catch
			{
			}
			try
			{
				_blnDiscountedGeas = Convert.ToBoolean(objNode["discountedgeas"].InnerText);
			}
			catch
			{
			}
            try
            {
                _strBonusSource = objNode["bonussource"].InnerText;
            }
            catch
            {
            }
            try
            {
                _decFreePoints = Convert.ToDecimal(objNode["freepoints"].InnerText);
            }
            catch
            {
            }
            try
			{
				_strSource = objNode["source"].InnerText;
			}
			catch
			{
			}
			try
			{
				_strPage = objNode["page"].InnerText;
			}
			catch
			{
			}
			try
			{
				_blnDoubleCost = Convert.ToBoolean(objNode["doublecost"].InnerText);
			}
			catch
			{
			}
			_nodBonus = objNode["bonus"];
			try
			{
				_strNotes = objNode["notes"].InnerText;
			}
			catch
			{
			}
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
			objWriter.WriteElementString("pointsperlevel", _decPointsPerLevel.ToString());
            objWriter.WriteElementString("adeptway", _decAdeptWayDiscount.ToString());
            if (_blnLevelsEnabled)
				objWriter.WriteElementString("rating", _intRating.ToString());
			else
				objWriter.WriteElementString("rating", "0");
			objWriter.WriteElementString("totalpoints", PowerPoints.ToString());
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
				
				if (_strExtra != "")
				{
					LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
					// Attempt to retrieve the CharacterAttribute name.
					try
					{
						if (LanguageManager.Instance.GetString("String_Attribute" + _strExtra + "Short") != "")
							strReturn += " (" + LanguageManager.Instance.GetString("String_Attribute" + _strExtra + "Short") + ")";
						else
							strReturn += " (" + LanguageManager.Instance.TranslateExtra(_strExtra) + ")";
					}
					catch
					{
						strReturn += " (" + LanguageManager.Instance.TranslateExtra(_strExtra) + ")";
					}
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
		public decimal Rating
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
	public class ComplexForm
	{
		private Guid _guiID = new Guid();
		private string _strName = "";
        private string _strTarget = "";
        private string _strDuration = "";
        private string _strFV = "";
        private string _strSource = "";
		private string _strPage = "";
        private string _strNotes = "";
        private string _strExtra = "";
		private string _strAltName = "";
		private string _strAltPage = "";
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
					if (objSpellNode["translate"] != null)
						_strAltName = objSpellNode["translate"].InnerText;
					if (objSpellNode["altpage"] != null)
						_strAltPage = objSpellNode["altpage"].InnerText;
				}
			}
			_strName = objXmlProgramNode["name"].InnerText;
            _strTarget = objXmlProgramNode["target"].InnerText;
			_strSource = objXmlProgramNode["source"].InnerText;
			_strPage = objXmlProgramNode["page"].InnerText;
            _strDuration = objXmlProgramNode["duration"].InnerText;
            _strExtra = strExtra;
            _strFV = objXmlProgramNode["fv"].InnerText;

            try
            {
                _strNotes = objXmlProgramNode["notes"].InnerText;
            }
            catch { }

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
			try
			{
				_guiID = Guid.Parse(objNode["guid"].InnerText);
			}
			catch
			{
			}
			_strName = objNode["name"].InnerText;
			try
			{
                _strTarget = objNode["target"].InnerText;
			}
			catch
			{
			}
			try
			{
                _strDuration = objNode["duration"].InnerText;
			}
			catch
			{
			}
            try
            {
                _strFV = objNode["fv"].InnerText;
            }
            catch
            {
            }
            try
            {
                _strExtra = objNode["extra"].InnerText;
            }
            catch
            {
            }
            try
			{
				_strSource = objNode["source"].InnerText;
			}
			catch
			{
			}
			try
			{
				_strPage = objNode["page"].InnerText;
			}
			catch
			{
			}
			try
			{
				_strNotes = objNode["notes"].InnerText;
			}
			catch
			{
			}
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
                if (_strExtra != "")
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
	/// A Martial Art.
	/// </summary>
	public class MartialArt
	{
		private string _strName = "";
		private string _strSource = "";
		private string _strPage = "";
		private int _intRating = 1;
		private List<MartialArtAdvantage> _lstAdvantages = new List<MartialArtAdvantage>();
		private string _strNotes = "";
		private Character _objCharacter;
        private bool _blnIsQuality;

		#region Create, Save, Load, and Print Methods
		public MartialArt(Character objCharacter)
		{
			_objCharacter = objCharacter;
		}

		/// Create a Martial Art from an XmlNode and return the TreeNodes for it.
		/// <param name="objXmlArtNode">XmlNode to create the object from.</param>
		/// <param name="objNode">TreeNode to populate a TreeView.</param>
		/// <param name="objCharacter">Character the Martial Art is being added to.</param>
		public void Create(XmlNode objXmlArtNode, TreeNode objNode, Character objCharacter)
		{
			_objCharacter = objCharacter;
			_strName = objXmlArtNode["name"].InnerText;
			_strSource = objXmlArtNode["source"].InnerText;
			_strPage = objXmlArtNode["page"].InnerText;
            if (objXmlArtNode["isquality"] != null)
                _blnIsQuality = Convert.ToBoolean(objXmlArtNode["isquality"].InnerText);
            else
                _blnIsQuality = false;

			objNode.Text = DisplayName;
			objNode.Tag = _strName;
		}

		/// <summary>
		/// Save the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public void Save(XmlTextWriter objWriter)
		{
			objWriter.WriteStartElement("martialart");
			objWriter.WriteElementString("name", _strName);
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
			_strName = objNode["name"].InnerText;
			_strSource = objNode["source"].InnerText;
			_strPage = objNode["page"].InnerText;
			_intRating = Convert.ToInt32(objNode["rating"].InnerText);
            _blnIsQuality = Convert.ToBoolean(objNode["isquality"].InnerText);

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

			try
			{
				_strNotes = objNode["notes"].InnerText;
			}
			catch
			{
			}
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
	public class MartialArtAdvantage
	{
		private Guid _guiID = new Guid();
		private string _strName = "";
		private string _strNotes = "";
        private string _strSource = "";
        private string _strPage = "";
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
			_strName = objXmlAdvantageNode["name"].InnerText;
            _strSource = objXmlAdvantageNode["source"].InnerText;
            _strPage = objXmlAdvantageNode["page"].InnerText;

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
			_guiID = Guid.Parse(objNode["guid"].InnerText);
			_strName = objNode["name"].InnerText;
			try
			{
				_strNotes = objNode["notes"].InnerText;
			}
			catch
			{
			}

            try
            {
                _strSource = objNode["source"].InnerText;
            }
            catch
            {
            }

            try
            {
                _strPage = objNode["page"].InnerText;
            }
            catch
            {
            }
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
    public class MartialArtManeuver
	{
		private Guid _guiID = new Guid();
		private string _strName = "";
		private string _strSource = "";
		private string _strPage = "";
		private string _strNotes = "";
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
			_strName = objXmlManeuverNode["name"].InnerText;
			_strSource = objXmlManeuverNode["source"].InnerText;
			_strPage = objXmlManeuverNode["page"].InnerText;

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
			_guiID = Guid.Parse(objNode["guid"].InnerText);
			_strName = objNode["name"].InnerText;
			_strSource = objNode["source"].InnerText;
			_strPage = objNode["page"].InnerText;
			try
			{
				_strNotes = objNode["notes"].InnerText;
			}
			catch
			{
			}
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
    public class LimitModifier
    {
        private Guid _guiID = new Guid();
        private string _strName = "";
        private string _strNotes = "";
        private string _strLimit = "";
        private string _strCondition = "";
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
            _guiID = Guid.Parse(objNode["guid"].InnerText);
            _strName = objNode["name"].InnerText;
            _strLimit = objNode["limit"].InnerText;
            _intBonus = Convert.ToInt32(objNode["bonus"].InnerText);
            try
            {
                _strCondition = objNode["condition"].InnerText;
            }
            catch
            {
            }
            try
            {
                _strNotes = objNode["notes"].InnerText;
            }
            catch
            {
            }
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
                string strBonus = "";
                if (_intBonus > 0)
                    strBonus = "+" + _intBonus.ToString();
                else
                    strBonus = _intBonus.ToString();

                string strReturn = DisplayNameShort + " [" + strBonus + "]";
                if (_strCondition != "")
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
		private string _strName = "";
        private string _strRole = "";
        private string _strLocation = "";
	    private string _strUnique;

        private int _intConnection = 1;
		private int _intLoyalty = 1;
		
		private string _strGroupName = "";
		private ContactType _objContactType = ContactType.Contact;
		private string _strFileName = "";
		private string _strRelativeName = "";
		private string _strNotes = "";
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

			if (ReadOnly) objWriter.WriteElementString("readonly", "");

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
			_strName = objNode["name"].InnerText;
			objNode.TryGetField("role", out _strRole);
			objNode.TryGetField("location", out _strLocation);
            _intConnection = Convert.ToInt32(objNode["connection"].InnerText);
			_intLoyalty = Convert.ToInt32(objNode["loyalty"].InnerText);
            _objContactType = ConvertToContactType(objNode["type"].InnerText);
			objNode.TryGetField("file", out _strFileName);
			objNode.TryGetField("notes", out _strNotes);
			objNode.TryGetField("groupname", out _strGroupName);
			objNode.TryGetField("free", out _blnFree);
			objNode.TryGetField("group", out _blnIsGroup);
			objNode.TryGetField("guid", out _strUnique);
			objNode.TryGetField("family", out _blnFamily);
			objNode.TryGetField("blackmail", out _blnBlackmail);
			try
			{
				_objColour = Color.FromArgb(Convert.ToInt32(objNode["colour"].InnerText));
			}
			catch
			{
			}

			if (objNode["readonly"] != null) _readonly = true;
		    objNode.TryGetField("mademan", out _blnMadeMan);
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
	public class CritterPower
	{
		private Guid _guiID = new Guid();
		private string _strName = "";
		private string _strCategory = "";
		private string _strType = "";
		private string _strAction = "";
		private string _strRange = "";
		private string _strDuration = "";
		private string _strExtra = "";
		private string _strSource = "";
		private string _strPage = "";
		private double _dblPowerPoints = 0.0;
		private XmlNode _nodBonus;
		private string _strNotes = "";
		private readonly Character _objCharacter;
		private bool _blnCountTowardsLimit = true;

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
			_strName = objXmlPowerNode["name"].InnerText;
			_strCategory = objXmlPowerNode["category"].InnerText;
			_strType = objXmlPowerNode["type"].InnerText;
			_strAction = objXmlPowerNode["action"].InnerText;
			_strRange = objXmlPowerNode["range"].InnerText;
			_strDuration = objXmlPowerNode["duration"].InnerText;
			_strSource = objXmlPowerNode["source"].InnerText;
			_strPage = objXmlPowerNode["page"].InnerText;
			_nodBonus = objXmlPowerNode["bonus"];

			// Create the TreeNode for the new item.
			objNode.Text = DisplayName;
			objNode.Tag = _guiID.ToString();

			if (intRating != 0)
				_strExtra = intRating.ToString();

			// If the piece grants a bonus, pass the information to the Improvement Manager.
			if (objXmlPowerNode.InnerXml.Contains("<bonus>"))
			{
				ImprovementManager objImprovementManager = new ImprovementManager(objCharacter);
				objImprovementManager.ForcedValue = strForcedValue;
				if (!objImprovementManager.CreateImprovements(Improvement.ImprovementSource.CritterPower, _guiID.ToString(), _nodBonus, true, intRating, DisplayNameShort))
				{
					_guiID = Guid.Empty;
					return;
				}
				if (objImprovementManager.SelectedValue != "")
				{
					_strExtra = objImprovementManager.SelectedValue;
					objNode.Text += " (" + objImprovementManager.SelectedValue + ")";
				}
			}
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
			objWriter.WriteElementString("category", _strCategory);
			objWriter.WriteElementString("type", _strType);
			objWriter.WriteElementString("action", _strAction);
			objWriter.WriteElementString("range", _strRange);
			objWriter.WriteElementString("duration", _strDuration);
			objWriter.WriteElementString("source", _strSource);
			objWriter.WriteElementString("page", _strPage);
			objWriter.WriteElementString("points", _dblPowerPoints.ToString(GlobalOptions.Instance.CultureInfo));
			objWriter.WriteElementString("counttowardslimit", _blnCountTowardsLimit.ToString());
			if (_nodBonus != null)
				objWriter.WriteRaw("<bonus>" + _nodBonus.InnerXml + "</bonus>");
			else
				objWriter.WriteElementString("bonus", "");
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
			_guiID = Guid.Parse(objNode["guid"].InnerText);
			_strName = objNode["name"].InnerText;
			_strExtra = objNode["extra"].InnerText;
			_strCategory = objNode["category"].InnerText;
			_strType = objNode["type"].InnerText;
			_strAction = objNode["action"].InnerText;
			_strRange = objNode["range"].InnerText;
			_strDuration = objNode["duration"].InnerText;
			_strSource = objNode["source"].InnerText;
			_strPage = objNode["page"].InnerText;
			try
			{
				_dblPowerPoints = Convert.ToDouble(objNode["points"].InnerText, GlobalOptions.Instance.CultureInfo);
			}
			catch
			{
			}
			try
			{
				_blnCountTowardsLimit = Convert.ToBoolean(objNode["counttowardslimit"].InnerText);
			}
			catch
			{
			}
			_nodBonus = objNode["bonus"];
			try
			{
				_strNotes = objNode["notes"].InnerText;
			}
			catch
			{
			}
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
				if (_strExtra != "")
				{
					LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
					// Attempt to retrieve the CharacterAttribute name.
					try
					{
						if (LanguageManager.Instance.GetString("String_Attribute" + _strExtra + "Short") != "")
							strReturn += " (" + LanguageManager.Instance.GetString("String_Attribute" + _strExtra + "Short") + ")";
						else
							strReturn += " (" + LanguageManager.Instance.TranslateExtra(_strExtra) + ")";
					}
					catch
					{
						strReturn += " (" + LanguageManager.Instance.TranslateExtra(_strExtra) + ")";
					}
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
				string strReturn = "";

				switch (_strType)
				{
					case "M":
						strReturn = LanguageManager.Instance.GetString("String_SpellTypeMana");
						break;
					case "P":
						strReturn = LanguageManager.Instance.GetString("String_SpellTypePhysical");
						break;
					default:
						strReturn = "";
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
				string strReturn = "";

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
				string strReturn = "";

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
		#endregion
	}

	/// <summary>
	/// An Initiation Grade.
	/// </summary>
	public class InitiationGrade
	{
		private Guid _guiID = new Guid();
		private bool _blnGroup = false;
		private bool _blnOrdeal = false;
        private bool _blnSchooling = false;
		private bool _blnTechnomancer = false;
		private int _intGrade = 0;
		private string _strNotes = "";

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
			_guiID = Guid.Parse(objNode["guid"].InnerText);
			_blnTechnomancer = Convert.ToBoolean(objNode["res"].InnerText);
			_intGrade = Convert.ToInt32(objNode["grade"].InnerText);
			_blnGroup = Convert.ToBoolean(objNode["group"].InnerText);
			_blnOrdeal = Convert.ToBoolean(objNode["ordeal"].InnerText);
            _blnSchooling = Convert.ToBoolean(objNode["schooling"].InnerText);
            try
			{
				_strNotes = objNode["notes"].InnerText;
			}
			catch
			{
			}
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
				int intCost = 0;
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

                intCost = Convert.ToInt32(Math.Ceiling(dblCost * dblMultiplier));

				return intCost;
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

	public class CalendarWeek
	{
		private Guid _guiID = new Guid();
		private int _intYear = 2072;
		private int _intWeek = 1;
		private string _strNotes = "";

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
		private string _strName = "";
		private string _strAdvantages = "";

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