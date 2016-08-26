﻿/*  This file is part of Chummer5a.
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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Linq;
 using Chummer.Backend;
 using Chummer.Backend.Equipment;
 using Chummer.Skills;

namespace Chummer
{
	[DebuggerDisplay("{DisplayDebug()}")]
    public class Improvement
    {
		private string DisplayDebug()
		{
			return $"{_objImprovementType} ({_intVal}, {_intRating}) <- {_objImprovementSource}, {_strSourceName}, {_strImprovedName}";
		}

        public enum ImprovementType
        {
            Attribute,
            Text,
			Armor,
			Reach,
			Nuyen,
			Essence,
			Reaction,
			PhysicalCM,
			StunCM,
			UnarmedDV,
			InitiativePass,
			MatrixInitiative,
			MatrixInitiativePass,
			LifestyleCost,
			CMThreshold,
			EnhancedArticulation,
			WeaponCategoryDV,
			CyberwareEssCost,
			SpecialTab,
			Initiative,
			Uneducated,
			LivingPersonaResponse,
			LivingPersonaSignal,
			LivingPersonaFirewall,
			LivingPersonaSystem,
			LivingPersonaBiofeedback,
			Smartlink,
			BiowareEssCost,
			GenetechCostMultiplier,
			BasicBiowareEssCost,
			TransgenicsBiowareCost,
			SoftWeave,
			SensitiveSystem,
			ConditionMonitor,
			UnarmedDVPhysical,
			MovementPercent,
			Adapsin,
			FreePositiveQualities,
			FreeNegativeQualities,
			FreeKnowledgeSkills, 
			NuyenMaxBP,
			CMOverflow,
			FreeSpiritPowerPoints,
			AdeptPowerPoints,
			ArmorEncumbrancePenalty,
			Uncouth,
			Initiation,
			Submersion,
			Infirm,
			Skillwire,
			DamageResistance,
			RestrictedItemCount,
			AdeptLinguistics,
			SwimPercent,
			FlyPercent,
			FlySpeed,
			JudgeIntentions,
			LiftAndCarry,
			Memory,
			Concealability,
			SwapSkillAttribute,
			DrainResistance,
			FadingResistance,
			MatrixInitiativePassAdd,
			InitiativePassAdd,
			Composure,
			UnarmedAP,
			CMThresholdOffset,
			Restricted,
			Notoriety,
			SpellCategory,
			ThrowRange,
			SkillsoftAccess, 
			AddSprite,
			BlackMarketDiscount,
			SelectWeapon,
			ComplexFormLimit,
			SpellLimit,
			QuickeningMetamagic,
			BasicLifestyleCost,
			ThrowSTR,
			IgnoreCMPenaltyStun,
			IgnoreCMPenaltyPhysical,
			CyborgEssence,
			EssenceMax,
            AdeptPower,
            SpecificQuality,
            MartialArt,
            LimitModifier,
            PhysicalLimit,
            MentalLimit,
            SocialLimit,
            SchoolOfHardKnocks,
            FriendsInHighPlaces,
            JackOfAllTrades,
            CollegeEducation,
            Erased,
            BornRich,
            Fame,
            LightningReflexes,
            Linguist,
            MadeMan,
            Overclocker,
            RestrictedGear,
            TechSchool,
            TrustFund,
            ExCon,
            BlackMarket,
            ContactMadeMan,
			SelectArmor,
			Attributelevel,
			AddContact,
			Seeker,
			PublicAwareness,
			PrototypeTranshuman,
			Hardwire,
            DealerConnection,
            Skill,  //Improve pool of skill based on name
			SkillGroup,  //Group
			SkillCategory, //category
			SkillAttribute, //attribute
			SkillLevel,  //Karma points in skill
			SkillGroupLevel, //group
			SkillBase,  //base points in skill
			SkillGroupBase, //group
			SkillKnowledgeForced, //A skill gained from a knowsoft 
			ReplaceAttribute, //Alter the base metatype or metavariant of a character. Used for infected.
			SpecialSkills,
			ReflexRecorderOptimization,
	        
        }

        public enum ImprovementSource
        {
            Quality,
            Power,
			Metatype,
			Cyberware,
			Metavariant,
			Bioware,
			Nanotech,
			Genetech,
			ArmorEncumbrance,
			Gear,
			Spell,
			MartialArtAdvantage,
			Initiation,
			Submersion,
			Metamagic,
			Echo,
			Armor, 
			ArmorMod,
			EssenceLoss,
			ConditionMonitor,
			CritterPower,
			ComplexForm,
			EdgeUse,
			MutantCritter,
			Cyberzombie,
			StackedFocus,
			AttributeLoss,
            Art,
            Enhancement,
			Custom,
	        Heritage
        }

		private string _strImprovedName = "";
        private string _strSourceName = "";
		private int _intMin = 0;
		private int _intMax = 0;
        private int _intAug = 0;
        private int _intAugMax = 0;
        private int _intVal = 0;
        private int _intRating = 1;
		private string _strExclude = "";
		private string _strUniqueName = "";
        private ImprovementType _objImprovementType;
        private ImprovementSource _objImprovementSource;
		private bool _blnCustom = false;
		private string _strCustomName = "";
		private string _strCustomId = "";
		private string _strCustomGroup = "";
		private string _strNotes = "";
		private bool _blnAddToRating = false;
		private bool _blnEnabled = true;
		private int _intOrder = 0;

        private CommonFunctions objFunctions = new CommonFunctions();

		#region Helper Methods

		/// <summary>
		/// Convert a string to an ImprovementType.
		/// </summary>
		/// <param name="strValue">String value to convert.</param>
		private ImprovementType ConvertToImprovementType(string strValue)
		{
			return (ImprovementType) Enum.Parse(typeof (ImprovementType), strValue);
		}

		/// <summary>
		/// Convert a string to an ImprovementSource.
		/// </summary>
		/// <param name="strValue">String value to convert.</param>
		public ImprovementSource ConvertToImprovementSource(string strValue)
		{
			return (ImprovementSource) Enum.Parse(typeof (ImprovementSource), strValue);
			}

		#endregion

		#region Save and Load Methods

		/// <summary>
		/// Save the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public void Save(XmlTextWriter objWriter)
		{
			Log.Enter("Save");

			objWriter.WriteStartElement("improvement");
			if (_strUniqueName != "")
				objWriter.WriteElementString("unique", _strUniqueName);
			objWriter.WriteElementString("improvedname", _strImprovedName);
			objWriter.WriteElementString("sourcename", _strSourceName);
			objWriter.WriteElementString("min", _intMin.ToString());
			objWriter.WriteElementString("max", _intMax.ToString());
			objWriter.WriteElementString("aug", _intAug.ToString());
			objWriter.WriteElementString("augmax", _intAugMax.ToString());
			objWriter.WriteElementString("val", _intVal.ToString());
			objWriter.WriteElementString("rating", _intRating.ToString());
			objWriter.WriteElementString("exclude", _strExclude);
			objWriter.WriteElementString("improvementttype", _objImprovementType.ToString());
			objWriter.WriteElementString("improvementsource", _objImprovementSource.ToString());
			objWriter.WriteElementString("custom", _blnCustom.ToString());
			objWriter.WriteElementString("customname", _strCustomName);
			objWriter.WriteElementString("customid", _strCustomId);
			objWriter.WriteElementString("customgroup", _strCustomGroup);
			objWriter.WriteElementString("addtorating", _blnAddToRating.ToString());
			objWriter.WriteElementString("enabled", _blnEnabled.ToString());
			objWriter.WriteElementString("order", _intOrder.ToString());
			objWriter.WriteElementString("notes", _strNotes);
			objWriter.WriteEndElement();

            Log.Exit("Save");
        }

		/// <summary>
		/// Load the CharacterAttribute from the XmlNode.
		/// </summary>
		/// <param name="objNode">XmlNode to load.</param>
		public void Load(XmlNode objNode)
		{
            Log.Enter("Load");
            
            try
			{
				_strUniqueName = objNode["unique"].InnerText;
			}
			catch
			{
			}
			_strImprovedName = objNode["improvedname"].InnerText;
			_strSourceName = objNode["sourcename"].InnerText;
			try
			{
				_intMin = Convert.ToInt32(objNode["min"].InnerText);
			}
			catch
			{
			}
			_intMax = Convert.ToInt32(objNode["max"].InnerText);
			_intAug = Convert.ToInt32(objNode["aug"].InnerText);
			_intAugMax = Convert.ToInt32(objNode["augmax"].InnerText);
			_intVal = Convert.ToInt32(objNode["val"].InnerText);
			_intRating = Convert.ToInt32(objNode["rating"].InnerText);
			_strExclude = objNode["exclude"].InnerText;
			_objImprovementType = ConvertToImprovementType(objNode["improvementttype"].InnerText);
			_objImprovementSource = ConvertToImprovementSource(objNode["improvementsource"].InnerText);
			_blnCustom = Convert.ToBoolean(objNode["custom"].InnerText);
			_strCustomName = objNode["customname"].InnerText;
			try
			{
				_strCustomId = objNode["customid"].InnerText;
			}
			catch
			{
			}
			try
			{
				_strCustomGroup = objNode["customgroup"].InnerText;
			}
			catch
			{
			}
			try
			{
				_blnAddToRating = Convert.ToBoolean(objNode["addtorating"].InnerText);
			}
			catch
			{
			}
			try
			{
				_blnEnabled = Convert.ToBoolean(objNode["enabled"].InnerText);
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
			try
			{
				_intOrder = Convert.ToInt32(objNode["order"].InnerText);
			}
			catch
			{
			}

            Log.Exit("Load");
        }

		#endregion

		#region Properties

		/// <summary>
		/// Whether or not this is a custom-made (manually created) Improvement.
		/// </summary>
		public bool Custom
		{
			get { return _blnCustom; }
			set { _blnCustom = value; }
			}

		/// <summary>
		/// User-entered name for the custom Improvement.
		/// </summary>
		public string CustomName
		{
			get { return _strCustomName; }
			set { _strCustomName = value; }
			}

		/// <summary>
		/// ID from the Improvements file. Only used for custom-made (manually created) Improvements.
		/// </summary>
		public string CustomId
		{
			get { return _strCustomId; }
			set { _strCustomId = value; }
			}

		/// <summary>
		/// Group name for the Custom Improvement.
		/// </summary>
		public string CustomGroup
		{
			get { return _strCustomGroup; }
			set { _strCustomGroup = value; }
			}

		/// <summary>
		/// User-entered notes for the custom Improvement.
		/// </summary>
		public string Notes
		{
			get { return _strNotes; }
			set { _strNotes = value; }
			}

        /// <summary>
        /// Name of the Skill or CharacterAttribute that the Improvement is improving.
        /// </summary>
        public string ImprovedName
        {
			get { return _strImprovedName; }
			set { _strImprovedName = value; }
            }

        /// <summary>
        /// Name of the source that granted this Improvement.
        /// </summary>
        public string SourceName
        {
			get { return _strSourceName; }
			set { _strSourceName = value; }
            }

        /// <summary>
        /// The type of Object that the Improvement is improving.
        /// </summary>
        public ImprovementType ImproveType
        {
			get { return _objImprovementType; }
			set { _objImprovementType = value; }
            }

        /// <summary>
        /// The type of Object that granted this Improvement.
        /// </summary>
        public ImprovementSource ImproveSource
        {
			get { return _objImprovementSource; }
			set { _objImprovementSource = value; }
            }

		/// <summary>
		/// Minimum value modifier.
		/// </summary>
		public int Minimum
		{
			get { return _intMin; }
			set { _intMin = value; }
			}

        /// <summary>
        /// Maximum value modifier.
        /// </summary>
        public int Maximum
        {
			get { return _intMax; }
			set { _intMax = value; }
            }

        /// <summary>
        /// Augmented Maximum value modifier.
        /// </summary>
        public int AugmentedMaximum
        {
			get { return _intAugMax; }
			set { _intAugMax = value; }
            }

        /// <summary>
        /// Augmented score modifier.
        /// </summary>
        public int Augmented
        {
			get { return _intAug; }
			set { _intAug = value; }
            }

        /// <summary>
        /// Value modifier.
        /// </summary>
        public int Value
        {
			get { return _intVal; }
			set { _intVal = value; }
            }

        /// <summary>
        /// The Rating value for the Improvement. This is 1 by default.
        /// </summary>
        public int Rating
        {
			get { return _intRating; }
			set { _intRating = value; }
            }

		/// <summary>
		/// A list of child items that should not receive the Improvement's benefit (typically for excluding a Skill from a Skill Group bonus).
		/// </summary>
		public string Exclude
		{
			get { return _strExclude; }
			set { _strExclude = value; }
			}

		/// <summary>
		/// A Unique name for the Improvement. Only the highest value of any one Improvement that is part of this Unique Name group will be applied.
		/// </summary>
		public string UniqueName
		{
			get { return _strUniqueName; }
			set { _strUniqueName = value; }
			}

		/// <summary>
		/// Whether or not the bonus applies directly to a Skill's Rating
		/// </summary>
		public bool AddToRating
		{
			get { return _blnAddToRating; }
			set { _blnAddToRating = value; }
			}

		/// <summary>
		/// Whether or not the Improvement is enabled and provided its bonus.
		/// </summary>
		public bool Enabled
		{
			get { return _blnEnabled; }
			set { _blnEnabled = value; }
			}

		/// <summary>
		/// Sort order for Custom Improvements.
		/// </summary>
		public int SortOrder
		{
			get { return _intOrder; }
			set { _intOrder = value; }
			}

		#endregion
	}

	public class ImprovementManager
	{
		private readonly Character _objCharacter;
		
		// String that will be used to limit the selection in Pick forms.
		private string _strLimitSelection = "";

		private string _strSelectedValue = "";
		private string _strForcedValue = "";
		private readonly List<Improvement> _lstTransaction = new List<Improvement>();

        private CommonFunctions objFunctions = new CommonFunctions();

		public ImprovementManager(Character objCharacter)
		{
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, null);
			_objCharacter = objCharacter;
		}

		#region Properties

		/// <summary>
		/// Limit what can be selected in Pick forms to a single value. This is typically used when selecting the Qualities for a Metavariant that has a specifiec
		/// CharacterAttribute selection for Qualities like Metagenetic Improvement.
		/// </summary>
		public string LimitSelection
		{
			get { return _strLimitSelection; }
			set { _strLimitSelection = value; }
			}

		/// <summary>
		/// The string that was entered or selected from any of the dialogue windows that were presented because of this Improvement.
		/// </summary>
		public string SelectedValue
		{
			get { return _strSelectedValue; }
			}

		/// <summary>
		/// Force any dialogue windows that open to use this string as their selected value.
		/// </summary>
		public string ForcedValue
		{
			set { _strForcedValue = value; }
			}

		#endregion

		#region Helper Methods

		/// <summary>
		/// Retrieve the total Improvement value for the specified ImprovementType.
		/// </summary>
		/// <param name="objImprovementType">ImprovementType to retrieve the value of.</param>
		/// <param name="blnAddToRating">Whether or not we should only retrieve values that have AddToRating enabled.</param>
		/// <param name="strImprovedName">Name to assign to the Improvement.</param>
		public int ValueOf(Improvement.ImprovementType objImprovementType, bool blnAddToRating = false,
			string strImprovedName = null)
		{
			//Log.Enter("ValueOf");
			//Log.Info("objImprovementType = " + objImprovementType.ToString());
			//Log.Info("blnAddToRating = " + blnAddToRating.ToString());
			//Log.Info("strImprovedName = " + ("" + strImprovedName).ToString());

            if (_objCharacter == null)
            {
                //Log.Exit("ValueOf");
                return 0;
            }

			List<string> lstUniqueName = new List<string>();
			List<string[,]> lstUniquePair = new List<string[,]>();
			int intValue = 0;
			foreach (Improvement objImprovement in _objCharacter.Improvements)
			{
				if (objImprovement.Enabled && !objImprovement.Custom)
				{
					bool blnAllowed = true;
					// Technomancers cannot benefit from Gear-based Matrix Initiative Pass modifiers (Gear - Sim Modules).
					if (_objCharacter.RESEnabled && objImprovement.ImproveSource == Improvement.ImprovementSource.Gear &&
					    objImprovementType == Improvement.ImprovementType.MatrixInitiativePass)
						blnAllowed = false;
					// Ignore items that apply to a Skill's Rating.
					if (objImprovement.AddToRating != blnAddToRating)
						blnAllowed = false;
					// If an Improved Name has been passed, only retrieve values that have this Improved Name.
					if (strImprovedName != null)
					{
						if (strImprovedName != objImprovement.ImprovedName)
							blnAllowed = false;
					}

					if (blnAllowed)
					{
						if (objImprovement.UniqueName != "" && objImprovement.ImproveType == objImprovementType)
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
							string[,] strValues = new string[,] {{objImprovement.UniqueName, objImprovement.Value.ToString()}};
							lstUniquePair.Add(strValues);
						}
						else
						{
							if (objImprovement.ImproveType == objImprovementType)
								intValue += objImprovement.Value;
						}
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
				intValue += intHighest;
			}

			if (lstUniqueName.Contains("precedence1"))
			{
				intValue = 0;
				// Retrieve all of the items that are precedence1 and nothing else.
				foreach (string[,] strValues in lstUniquePair)
				{
					if (strValues[0, 0] == "precedence1")
						intValue += Convert.ToInt32(strValues[0, 1]);
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
				if (lstUniqueName.Contains("ignoreprecedence"))
				{
					foreach (string[,] strValues in lstUniquePair)
					{
						if (strValues[0, 0] == "ignoreprecedence")
						{
							intHighest += Convert.ToInt32(strValues[0, 1]);
						}
					}
				}
				intValue = intHighest;
			}

			// Factor in Custom Improvements.
			lstUniqueName = new List<string>();
			lstUniquePair = new List<string[,]>();
			int intCustomValue = 0;
			foreach (Improvement objImprovement in _objCharacter.Improvements)
			{
				if (objImprovement.Enabled && objImprovement.Custom)
				{
					bool blnAllowed = true;
					// Technomancers cannot benefit from Gear-based Matrix Initiative Pass modifiers (Gear - Sim Modules).
					if (_objCharacter.RESEnabled && objImprovement.ImproveSource == Improvement.ImprovementSource.Gear &&
					    objImprovementType == Improvement.ImprovementType.MatrixInitiativePass)
						blnAllowed = false;
					// Ignore items that apply to a Skill's Rating.
					if (objImprovement.AddToRating != blnAddToRating)
						blnAllowed = false;
					// If an Improved Name has been passed, only retrieve values that have this Improved Name.
					if (strImprovedName != null)
					{
						if (strImprovedName != objImprovement.ImprovedName)
							blnAllowed = false;
					}

					if (blnAllowed)
					{
						if (objImprovement.UniqueName != "" && objImprovement.ImproveType == objImprovementType)
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
							string[,] strValues = new string[,] {{objImprovement.UniqueName, objImprovement.Value.ToString()}};
							lstUniquePair.Add(strValues);
						}
						else
						{
							if (objImprovement.ImproveType == objImprovementType)
								intCustomValue += objImprovement.Value;
						}
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
				intCustomValue += intHighest;
			}

            //Log.Exit("ValueOf");

			return intValue + intCustomValue;
		}

		/// <summary>
		/// Convert a string to an integer, converting "Rating" to a number where appropriate.
		/// </summary>
		/// <param name="strValue">String value to parse.</param>
		/// <param name="intRating">Integer value to replace "Rating" with.</param>
		private int ValueToInt(string strValue, int intRating)
		{
   //         Log.Enter("ValueToInt");
   //         Log.Info("strValue = " + strValue);
			//Log.Info("intRating = " + intRating.ToString());
            
			if (strValue.Contains("Rating") || strValue.Contains("BOD") || strValue.Contains("AGI") || strValue.Contains("REA") ||
			    strValue.Contains("STR") || strValue.Contains("CHA") || strValue.Contains("INT") || strValue.Contains("LOG") ||
			    strValue.Contains("WIL") || strValue.Contains("EDG") || strValue.Contains("MAG") || strValue.Contains("RES"))
			{
				// If the value contain an CharacterAttribute name, replace it with the character's CharacterAttribute.
				strValue = strValue.Replace("BOD", _objCharacter.BOD.TotalValue.ToString());
				strValue = strValue.Replace("AGI", _objCharacter.AGI.TotalValue.ToString());
				strValue = strValue.Replace("REA", _objCharacter.REA.TotalValue.ToString());
				strValue = strValue.Replace("STR", _objCharacter.STR.TotalValue.ToString());
				strValue = strValue.Replace("CHA", _objCharacter.CHA.TotalValue.ToString());
				strValue = strValue.Replace("INT", _objCharacter.INT.TotalValue.ToString());
				strValue = strValue.Replace("LOG", _objCharacter.LOG.TotalValue.ToString());
				strValue = strValue.Replace("WIL", _objCharacter.WIL.TotalValue.ToString());
				strValue = strValue.Replace("EDG", _objCharacter.EDG.TotalValue.ToString());
				strValue = strValue.Replace("MAG", _objCharacter.MAG.TotalValue.ToString());
				strValue = strValue.Replace("RES", _objCharacter.RES.TotalValue.ToString());

				XmlDocument objXmlDocument = new XmlDocument();
				XPathNavigator nav = objXmlDocument.CreateNavigator();
				string strReturn = strValue.Replace("Rating", intRating.ToString());
                //Log.Info("strValue = " + strValue);
                //Log.Info("strReturn = " + strReturn);
                XPathExpression xprValue = nav.Compile(strReturn);

				// Treat this as a decimal value so any fractions can be rounded down. This is currently only used by the Boosted Reflexes Cyberware from SR2050.
				decimal decValue = Convert.ToDecimal(nav.Evaluate(xprValue).ToString(), GlobalOptions.Instance.CultureInfo);
				decValue = Math.Floor(decValue);
				int intValue = Convert.ToInt32(decValue);

                //Log.Exit("ValueToInt");
				return Convert.ToInt32(intValue);
			}
			else
			{
                //Log.Exit("ValueToInt");
                if (strValue.Contains("FixedValues"))
				{
					string[] strValues = strValue.Replace("FixedValues(", string.Empty).Replace(")", string.Empty).Split(',');
					return Convert.ToInt32(strValues[intRating - 1]);
				}
				else
					return Convert.ToInt32(strValue);
			}
		}

		/// <summary>
		/// Determine whether or not an XmlNode with the specified name exists within an XmlNode.
		/// </summary>
		/// <param name="objXmlNode">XmlNode to examine.</param>
		/// <param name="strName">Name of the XmlNode to look for.</param>
		private bool NodeExists(XmlNode objXmlNode, string strName)
		{
   //         Log.Enter("NodeExists");
			//Log.Info("objXmlNode = " + objXmlNode.OuterXml.ToString());
   //         Log.Info("strName = " + strName);

            bool blnReturn = false;
			try
			{
				XmlNode objXmlTest = objXmlNode.SelectSingleNode(strName);
				if (objXmlTest != null)
					blnReturn = true;
			}
			catch
			{
			}

			return blnReturn;
		}

		#endregion

		#region Improvement System

		/// <summary>
		/// Create all of the Improvements for an XML Node.
		/// </summary>
		/// <param name="objImprovementSource">Type of object that grants these Improvements.</param>
		/// <param name="strSourceName">Name of the item that grants these Improvements.</param>
		/// <param name="nodBonus">bonus XMLXode from the source data file.</param>
		/// <param name="blnConcatSelectedValue">Whether or not any selected values should be concatinated with the SourceName string when storing.</param>
		/// <param name="intRating">Selected Rating value that is used to replace the Rating string in an Improvement.</param>
		/// <param name="strFriendlyName">Friendly name to show in any dialogue windows that ask for a value.</param>
		/// <returns>True if successfull</returns>
		public bool CreateImprovements(Improvement.ImprovementSource objImprovementSource, string strSourceName,
			XmlNode nodBonus, bool blnConcatSelectedValue = false, int intRating = 1, string strFriendlyName = "",
			object fCreate = null)
		{
            Log.Enter("CreateImprovements");
			Log.Info("objImprovementSource = " + objImprovementSource.ToString());
			Log.Info("strSourceName = " + strSourceName);
			Log.Info("nodBonus = " + nodBonus.OuterXml.ToString());
			Log.Info("blnConcatSelectedValue = " + blnConcatSelectedValue.ToString());
			Log.Info("intRating = " + intRating.ToString());
			Log.Info("strFriendlyName = " + strFriendlyName);
			Log.Info("intRating = " + intRating.ToString());

            bool blnSuccess = true;

            /*try
            {*/
                if (nodBonus == null)
                {
                    _strForcedValue = "";
                    _strLimitSelection = "";
                    Log.Exit("CreateImprovements");
                    return true;
                }

                string strUnique = "";
                if (nodBonus.Attributes["unique"] != null)
                    strUnique = nodBonus.Attributes["unique"].InnerText;

                _strSelectedValue = "";

				Log.Info(
					"_strForcedValue = " + _strForcedValue);
				Log.Info(
					"_strLimitSelection = " + _strLimitSelection);

                // If no friendly name was provided, use the one from SourceName.
                if (strFriendlyName == "")
                    strFriendlyName = strSourceName;

                if (nodBonus.HasChildNodes)
                {
                    Log.Info("Has Child Nodes");

                    // Select Text (custom entry for things like Allergy).
                    if (NodeExists(nodBonus, "selecttext"))
                    {
                        Log.Info("selecttext");

					if (_objCharacter != null)
					{
						if (_strForcedValue != "")
						{
							_strLimitSelection = _strForcedValue;
						}
						else if (_objCharacter.Pushtext.Count != 0)
						{
							_strLimitSelection = _objCharacter.Pushtext.Pop();
						}
					}

						Log.Info("_strForcedValue = " + _strSelectedValue);
						Log.Info("_strLimitSelection = " + _strLimitSelection);

                        // Display the Select Text window and record the value that was entered.
                        frmSelectText frmPickText = new frmSelectText();
						frmPickText.Description = LanguageManager.Instance.GetString("String_Improvement_SelectText")
							.Replace("{0}", strFriendlyName);

                        if (_strLimitSelection != "")
                        {
                            frmPickText.SelectedValue = _strLimitSelection;
                            frmPickText.Opacity = 0;
                        }

                        frmPickText.ShowDialog();

                        // Make sure the dialogue window was not canceled.
                        if (frmPickText.DialogResult == DialogResult.Cancel)
                        {
                            Rollback();
                            blnSuccess = false;
                            _strForcedValue = "";
                            _strLimitSelection = "";
                            Log.Exit("CreateImprovements");
                            return false;
                        }

                        _strSelectedValue = frmPickText.SelectedValue;
                        if (blnConcatSelectedValue)
                            strSourceName += " (" + _strSelectedValue + ")";
						Log.Info("_strSelectedValue = " + _strSelectedValue);
						Log.Info("strSourceName = " + strSourceName);

                        // Create the Improvement.
                        Log.Info("Calling CreateImprovement");
						CreateImprovement(frmPickText.SelectedValue, objImprovementSource, strSourceName, Improvement.ImprovementType.Text,
							strUnique);
                    }
                }

                // If there is no character object, don't attempt to add any Improvements.
                if (_objCharacter == null)
                {
                    Log.Info( "_objCharacter = Null");
                    Log.Exit("CreateImprovements");
                    return true;
                }

                // Check to see what bonuses the node grants.
				foreach (XmlNode bonusNode in nodBonus.ChildNodes)
                {
					blnSuccess = ProcessBonus(objImprovementSource, ref strSourceName, blnConcatSelectedValue, intRating,
						strFriendlyName, bonusNode, strUnique);
					if (blnSuccess == false)
					{
						Rollback();
						return false;
					}
				}


				// If we've made it this far, everything went OK, so commit the Improvements.
				Log.Info("Calling Commit");
				Commit();
				Log.Info("Returned from Commit");
				// Clear the Forced Value and Limit Selection strings once we're done to prevent these from forcing their values on other Improvements.
				_strForcedValue = "";
				_strLimitSelection = "";
			/*}
			catch (Exception ex)
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Error, "Chummer.ImprovementManager", "ERROR Message = " + ex.Message);
				objFunctions.LogWrite(CommonFunctions.LogType.Error, "Chummer.ImprovementManager", "ERROR Source  = " + ex.Source);
				objFunctions.LogWrite(CommonFunctions.LogType.Error, "Chummer.ImprovementManager",
					"ERROR Trace   = " + ex.StackTrace.ToString());
				
				Rollback();
				throw;
			}*/
			Log.Exit("CreateImprovements");
			return blnSuccess;

		}

		private bool ProcessBonus(Improvement.ImprovementSource objImprovementSource, ref string strSourceName,
			bool blnConcatSelectedValue,
			int intRating, string strFriendlyName, XmlNode bonusNode, string strUnique)
		{
			Log.Info("Has Child Nodes");
			// Add an Attribute.
			if (bonusNode.LocalName == ("addattribute"))
			{
				Log.Info("addattribute");
				if (bonusNode["name"].InnerText == "MAG")
				{
					_objCharacter.MAGEnabled = true;
					Log.Info("Calling CreateImprovement for MAG");
					CreateImprovement("MAG", objImprovementSource, strSourceName, Improvement.ImprovementType.Attribute,
						"enableattribute", 0, 0);
				}
				else if (bonusNode["name"].InnerText == "RES")
				{
					_objCharacter.RESEnabled = true;
					Log.Info("Calling CreateImprovement for RES");
					CreateImprovement("RES", objImprovementSource, strSourceName, Improvement.ImprovementType.Attribute,
						"enableattribute", 0, 0);
				}
			}
			// Add an Attribute Replacement.
			if (bonusNode.LocalName == ("replaceattributes"))
			{
				XmlNodeList objXmlAttributes = bonusNode.SelectNodes("replaceattribute");
				if (objXmlAttributes != null)
					foreach (XmlNode objXmlAttribute in objXmlAttributes)
					{
						Log.Info("replaceattribute");
						Log.Info("replaceattribute = " + bonusNode.OuterXml.ToString());
						// Record the improvement.
						int intMin = 0;
						int intMax = 0;

						// Extract the modifiers.
						if (objXmlAttribute.InnerXml.Contains("min"))
							intMin = Convert.ToInt32(objXmlAttribute["min"].InnerText);
						if (objXmlAttribute.InnerXml.Contains("max"))
							intMax = Convert.ToInt32(objXmlAttribute["max"].InnerText);
						string strAttribute = objXmlAttribute["name"].InnerText;

						Log.Info("Calling CreateImprovement");
						CreateImprovement(strAttribute, objImprovementSource, strSourceName, Improvement.ImprovementType.ReplaceAttribute,
							strUnique,
							0, 1, intMin, intMax, 0, 0);
					}
			}

			// Enable a special tab.
			if (bonusNode.LocalName == ("enabletab"))
			{
				Log.Info("enabletab");
				foreach (XmlNode objXmlEnable in bonusNode.ChildNodes)
				{
					switch (objXmlEnable.InnerText)
					{
						case "magician":
							_objCharacter.MagicianEnabled = true;
							Log.Info("magician");
							CreateImprovement("Magician", objImprovementSource, strSourceName, Improvement.ImprovementType.SpecialTab,
								"enabletab", 0, 0);
							break;
						case "adept":
							_objCharacter.AdeptEnabled = true;
							Log.Info("adept");
							CreateImprovement("Adept", objImprovementSource, strSourceName, Improvement.ImprovementType.SpecialTab,
								"enabletab",
								0, 0);
							break;
						case "technomancer":
							_objCharacter.TechnomancerEnabled = true;
							Log.Info("technomancer");
							CreateImprovement("Technomancer", objImprovementSource, strSourceName, Improvement.ImprovementType.SpecialTab,
								"enabletab", 0, 0);
							break;
						case "critter":
							_objCharacter.CritterEnabled = true;
							Log.Info("critter");
							CreateImprovement("Critter", objImprovementSource, strSourceName, Improvement.ImprovementType.SpecialTab,
								"enabletab", 0, 0);
							break;
						case "initiation":
							_objCharacter.InitiationEnabled = true;
							Log.Info("initiation");
							CreateImprovement("Initiation", objImprovementSource, strSourceName, Improvement.ImprovementType.SpecialTab,
								"enabletab", 0, 0);
							break;
					}
				}
			}

			// Select Restricted (select Restricted items for Fake Licenses).
			if (bonusNode.LocalName == ("selectrestricted"))
			{
				Log.Info("selectrestricted");
				frmSelectItem frmPickItem = new frmSelectItem();
				frmPickItem.Character = _objCharacter;
				if (_strForcedValue != string.Empty)
					frmPickItem.ForceItem = _strForcedValue;
				frmPickItem.AllowAutoSelect = false;
				frmPickItem.ShowDialog();

				// Make sure the dialogue window was not canceled.
				if (frmPickItem.DialogResult == DialogResult.Cancel)
				{
					Rollback();
					_strForcedValue = "";
					_strLimitSelection = "";
					return false;
				}

				_strSelectedValue = frmPickItem.SelectedItem;
				if (blnConcatSelectedValue)
					strSourceName += " (" + _strSelectedValue + ")";

				Log.Info("_strSelectedValue = " + _strSelectedValue);
				Log.Info("strSourceName = " + strSourceName);

				// Create the Improvement.
				Log.Info("Calling CreateImprovement");
				CreateImprovement(frmPickItem.SelectedItem, objImprovementSource, strSourceName,
					Improvement.ImprovementType.Restricted, strUnique);
			}

			if (bonusNode.LocalName == "cyberseeker")
			{
				//Check if valid attrib
				if (new string[] {"BOD", "AGI", "STR", "REA", "LOG", "CHA", "INT", "WIL", "BOX"}.Any(x => x == bonusNode.InnerText))
				{
					CreateImprovement(bonusNode.InnerText, objImprovementSource, strSourceName, Improvement.ImprovementType.Seeker, strUnique,0,0,0,0,0,0);

				}
				else
				{
					Utils.BreakIfDebug();
				}

			}

			// Select a Skill.
			if (bonusNode.LocalName == ("selectskill"))
			{
				//TODO this don't work
				Log.Info("selectskill");
				if (_strForcedValue == "+2 to a Combat Skill")
					_strForcedValue = "";

				Log.Info("_strSelectedValue = " + _strSelectedValue);
				Log.Info("_strForcedValue = " + _strForcedValue);

				// Display the Select Skill window and record which Skill was selected.
				frmSelectSkill frmPickSkill = new frmSelectSkill(_objCharacter);
				if (strFriendlyName != "")
					frmPickSkill.Description = LanguageManager.Instance.GetString("String_Improvement_SelectSkillNamed")
						.Replace("{0}", strFriendlyName);
				else
					frmPickSkill.Description = LanguageManager.Instance.GetString("String_Improvement_SelectSkill");

				Log.Info("selectskill = " + bonusNode.OuterXml.ToString());
				if (bonusNode.OuterXml.Contains("skillgroup"))
					frmPickSkill.OnlySkillGroup = bonusNode.Attributes["skillgroup"].InnerText;
				else if (bonusNode.OuterXml.Contains("skillcategory"))
					frmPickSkill.OnlyCategory = bonusNode.Attributes["skillcategory"].InnerText;
				else if (bonusNode.OuterXml.Contains("excludecategory"))
					frmPickSkill.ExcludeCategory = bonusNode.Attributes["excludecategory"].InnerText;
				else if (bonusNode.OuterXml.Contains("limittoskill"))
					frmPickSkill.LimitToSkill = bonusNode.Attributes["limittoskill"].InnerText;
				else if (bonusNode.OuterXml.Contains("limittoattribute"))
					frmPickSkill.LinkedAttribute = bonusNode.Attributes["limittoattribute"].InnerText;

				if (_strForcedValue != "")
				{
					frmPickSkill.OnlySkill = _strForcedValue;
					frmPickSkill.Opacity = 0;
				}
				frmPickSkill.ShowDialog();

				// Make sure the dialogue window was not canceled.
				if (frmPickSkill.DialogResult == DialogResult.Cancel)
				{
					Rollback();
					_strForcedValue = "";
					_strLimitSelection = "";
					return false;
				}

				bool blnAddToRating = false;
				if (bonusNode["applytorating"] != null)
				{
					if (bonusNode["applytorating"].InnerText == "yes")
						blnAddToRating = true;
				}

				_strSelectedValue = frmPickSkill.SelectedSkill;
				if (blnConcatSelectedValue)
					strSourceName += " (" + _strSelectedValue + ")";

				Log.Info("_strSelectedValue = " + _strSelectedValue);
				Log.Info("strSourceName = " + strSourceName);

				// Find the selected Skill.
				foreach (Skill objSkill in _objCharacter.SkillsSection.Skills)
				{
					if (frmPickSkill.SelectedSkill.Contains("Exotic Melee Weapon") ||
					    frmPickSkill.SelectedSkill.Contains("Exotic Ranged Weapon") ||
					    frmPickSkill.SelectedSkill.Contains("Pilot Exotic Vehicle"))
					{
						if (objSkill.Name + " (" + objSkill.Specialization + ")" == frmPickSkill.SelectedSkill)
						{
							// We've found the selected Skill.
							if (bonusNode.InnerXml.Contains("val"))
							{
								Log.Info("Calling CreateImprovement");
								CreateImprovement(objSkill.Name + " (" + objSkill.Specialization + ")", objImprovementSource, strSourceName,
									Improvement.ImprovementType.Skill, strUnique, ValueToInt(bonusNode["val"].InnerText, intRating), 1,
									0, 0, 0, 0, "", blnAddToRating);
							}

							if (bonusNode.InnerXml.Contains("max"))
							{
								Log.Info("Calling CreateImprovement");
								CreateImprovement(objSkill.Name + " (" + objSkill.Specialization + ")", objImprovementSource, strSourceName,
									Improvement.ImprovementType.Skill, strUnique, 0, 1, 0,
									ValueToInt(bonusNode["max"].InnerText, intRating), 0, 0, "", blnAddToRating);
							}
						}
					}
					else
					{
						if (objSkill.Name == frmPickSkill.SelectedSkill)
						{
							// We've found the selected Skill.
							if (bonusNode.InnerXml.Contains("val"))
							{
								Log.Info("Calling CreateImprovement");
								CreateImprovement(objSkill.Name, objImprovementSource, strSourceName, Improvement.ImprovementType.Skill,
									strUnique,
									ValueToInt(bonusNode["val"].InnerText, intRating), 1, 0, 0, 0, 0, "", blnAddToRating);
							}

							if (bonusNode.InnerXml.Contains("max"))
							{
								Log.Info("Calling CreateImprovement");
								CreateImprovement(objSkill.Name, objImprovementSource, strSourceName, Improvement.ImprovementType.Skill,
									strUnique,
									0, 1, 0, ValueToInt(bonusNode["max"].InnerText, intRating), 0, 0, "", blnAddToRating);
							}
						}
					}
				}
			}

			// Select a Skill Group.
			if (bonusNode.LocalName == ("selectskillgroup"))
			{
				Log.Info("selectskillgroup");
				string strExclude = "";
				if (bonusNode.Attributes["excludecategory"] != null)
					strExclude = bonusNode.Attributes["excludecategory"].InnerText;

				frmSelectSkillGroup frmPickSkillGroup = new frmSelectSkillGroup();
				if (strFriendlyName != "")
					frmPickSkillGroup.Description =
						LanguageManager.Instance.GetString("String_Improvement_SelectSkillGroupName").Replace("{0}", strFriendlyName);
				else
					frmPickSkillGroup.Description = LanguageManager.Instance.GetString("String_Improvement_SelectSkillGroup");

				Log.Info("_strForcedValue = " + _strForcedValue);
				Log.Info("_strLimitSelection = " + _strLimitSelection);

				if (_strForcedValue != "")
				{
					frmPickSkillGroup.OnlyGroup = _strForcedValue;
					frmPickSkillGroup.Opacity = 0;
				}

				if (strExclude != string.Empty)
					frmPickSkillGroup.ExcludeCategory = strExclude;

				frmPickSkillGroup.ShowDialog();

				// Make sure the dialogue window was not canceled.
				if (frmPickSkillGroup.DialogResult == DialogResult.Cancel)
				{
					Rollback();

					_strForcedValue = "";
					_strLimitSelection = "";
					return false;
				}

				bool blnAddToRating = false;
				if (bonusNode["applytorating"] != null)
				{
					if (bonusNode["applytorating"].InnerText == "yes")
						blnAddToRating = true;
				}

				_strSelectedValue = frmPickSkillGroup.SelectedSkillGroup;

				Log.Info("_strSelectedValue = " + _strSelectedValue);
				Log.Info("strSourceName = " + strSourceName);

				if (bonusNode.SelectSingleNode("bonus") != null)
				{
					Log.Info("Calling CreateImprovement");
					CreateImprovement(_strSelectedValue, objImprovementSource, strSourceName, Improvement.ImprovementType.SkillGroup,
						strUnique, ValueToInt(bonusNode["bonus"].InnerText, intRating), 1, 0, 0, 0, 0, strExclude,
						blnAddToRating);
				}
				else
				{
					Log.Info("Calling CreateImprovement");
					CreateImprovement(_strSelectedValue, objImprovementSource, strSourceName, Improvement.ImprovementType.SkillGroup,
						strUnique, 0, 0, 0, 1, 0, 0, strExclude,
						blnAddToRating);
				}
			}

			if (bonusNode.LocalName == ("selectattributes"))
			{
				foreach (XmlNode objXmlAttribute in bonusNode.SelectNodes("selectattribute"))
				{
					Log.Info("selectattribute");
					// Display the Select Attribute window and record which Skill was selected.
					frmSelectAttribute frmPickAttribute = new frmSelectAttribute();
					if (strFriendlyName != "")
						frmPickAttribute.Description =
							LanguageManager.Instance.GetString("String_Improvement_SelectAttributeNamed").Replace("{0}", strFriendlyName);
					else
						frmPickAttribute.Description = LanguageManager.Instance.GetString("String_Improvement_SelectAttribute");

					// Add MAG and/or RES to the list of Attributes if they are enabled on the form.
					if (_objCharacter.MAGEnabled)
						frmPickAttribute.AddMAG();
					if (_objCharacter.RESEnabled)
						frmPickAttribute.AddRES();

					Log.Info("selectattribute = " + bonusNode.OuterXml.ToString());

					if (objXmlAttribute.InnerXml.Contains("<attribute>"))
					{
						List<string> strValue = new List<string>();
						foreach (XmlNode objSubNode in objXmlAttribute.SelectNodes("attribute"))
							strValue.Add(objSubNode.InnerText);
						frmPickAttribute.LimitToList(strValue);
					}

					if (bonusNode.InnerXml.Contains("<excludeattribute>"))
					{
						List<string> strValue = new List<string>();
						foreach (XmlNode objSubNode in objXmlAttribute.SelectNodes("excludeattribute"))
							strValue.Add(objSubNode.InnerText);
						frmPickAttribute.RemoveFromList(strValue);
					}

					// Check to see if there is only one possible selection because of _strLimitSelection.
					if (_strForcedValue != "")
						_strLimitSelection = _strForcedValue;

					Log.Info("_strForcedValue = " + _strForcedValue);
					Log.Info("_strLimitSelection = " + _strLimitSelection);

					if (_strLimitSelection != "")
					{
						frmPickAttribute.SingleAttribute(_strLimitSelection);
						frmPickAttribute.Opacity = 0;
					}

					frmPickAttribute.ShowDialog();

					// Make sure the dialogue window was not canceled.
					if (frmPickAttribute.DialogResult == DialogResult.Cancel)
					{
						Rollback();
						_strForcedValue = "";
						return false;
					}

					_strSelectedValue = frmPickAttribute.SelectedAttribute;
					if (blnConcatSelectedValue)
						strSourceName += " (" + _strSelectedValue + ")";

					Log.Info("_strSelectedValue = " + _strSelectedValue);
					Log.Info("strSourceName = " + strSourceName);

					// Record the improvement.
					int intMin = 0;
					int intAug = 0;
					int intMax = 0;
					int intAugMax = 0;

					// Extract the modifiers.
					if (objXmlAttribute.InnerXml.Contains("min"))
						intMin = Convert.ToInt32(objXmlAttribute["min"].InnerText);
					if (objXmlAttribute.InnerXml.Contains("val"))
						intAug = Convert.ToInt32(objXmlAttribute["val"].InnerText);
					if (objXmlAttribute.InnerXml.Contains("max"))
						intMax = Convert.ToInt32(objXmlAttribute["max"].InnerText);
					if (objXmlAttribute.InnerXml.Contains("aug"))
						intAugMax = Convert.ToInt32(objXmlAttribute["aug"].InnerText);

					string strAttribute = frmPickAttribute.SelectedAttribute;

if (objXmlAttribute["affectbase"] != null)
						strAttribute += "Base";

					Log.Info("Calling CreateImprovement");
					CreateImprovement(strAttribute, objImprovementSource, strSourceName, Improvement.ImprovementType.Attribute,
						strUnique,
						0, 1, intMin, intMax, intAug, intAugMax);
				}
			}

			// Select an CharacterAttribute.
			if (bonusNode.LocalName == ("selectattribute"))
			{
				Log.Info("selectattribute");
				// Display the Select Attribute window and record which Skill was selected.
				frmSelectAttribute frmPickAttribute = new frmSelectAttribute();
				if (strFriendlyName != "")
					frmPickAttribute.Description =
						LanguageManager.Instance.GetString("String_Improvement_SelectAttributeNamed").Replace("{0}", strFriendlyName);
				else
					frmPickAttribute.Description = LanguageManager.Instance.GetString("String_Improvement_SelectAttribute");

				// Add MAG and/or RES to the list of Attributes if they are enabled on the form.
				if (_objCharacter.MAGEnabled)
					frmPickAttribute.AddMAG();
				if (_objCharacter.RESEnabled)
					frmPickAttribute.AddRES();

				Log.Info("selectattribute = " + bonusNode.OuterXml.ToString());

				if (bonusNode.InnerXml.Contains("<attribute>"))
				{
					List<string> strValue = new List<string>();
					foreach (XmlNode objXmlAttribute in bonusNode.SelectNodes("attribute"))
						strValue.Add(objXmlAttribute.InnerText);
					frmPickAttribute.LimitToList(strValue);
				}

				if (bonusNode.InnerXml.Contains("<excludeattribute>"))
				{
					List<string> strValue = new List<string>();
					foreach (XmlNode objXmlAttribute in bonusNode.SelectNodes("excludeattribute"))
						strValue.Add(objXmlAttribute.InnerText);
					frmPickAttribute.RemoveFromList(strValue);
				}

				// Check to see if there is only one possible selection because of _strLimitSelection.
				if (_strForcedValue != "")
					_strLimitSelection = _strForcedValue;

				Log.Info("_strForcedValue = " + _strForcedValue);
				Log.Info("_strLimitSelection = " + _strLimitSelection);

				if (_strLimitSelection != "")
				{
					frmPickAttribute.SingleAttribute(_strLimitSelection);
					frmPickAttribute.Opacity = 0;
				}

				frmPickAttribute.ShowDialog();

				// Make sure the dialogue window was not canceled.
				if (frmPickAttribute.DialogResult == DialogResult.Cancel)
				{
					Rollback();
					_strForcedValue = "";
					return false;
				}

				_strSelectedValue = frmPickAttribute.SelectedAttribute;
				if (blnConcatSelectedValue)
					strSourceName += " (" + _strSelectedValue + ")";

				Log.Info("_strSelectedValue = " + _strSelectedValue);
				Log.Info("strSourceName = " + strSourceName);

				// Record the improvement.
				int intMin = 0;
				int intAug = 0;
				int intMax = 0;
				int intAugMax = 0;

				// Extract the modifiers.
				if (bonusNode.InnerXml.Contains("min"))
					intMin = ValueToInt(bonusNode["min"].InnerXml, intRating);
				if (bonusNode.InnerXml.Contains("val"))
					intAug = ValueToInt(bonusNode["val"].InnerXml, intRating);
				if (bonusNode.InnerXml.Contains("max"))
					intMax = ValueToInt(bonusNode["max"].InnerXml, intRating);
				if (bonusNode.InnerXml.Contains("aug"))
					intAugMax = ValueToInt(bonusNode["aug"].InnerXml, intRating);

				string strAttribute = frmPickAttribute.SelectedAttribute;

				if (bonusNode["affectbase"] != null)
					strAttribute += "Base";

				Log.Info("Calling CreateImprovement");
				CreateImprovement(strAttribute, objImprovementSource, strSourceName, Improvement.ImprovementType.Attribute,
					strUnique,
					0, 1, intMin, intMax, intAug, intAugMax);
			}

			// Select a Limit.
			if (bonusNode.LocalName == ("selectlimit"))
			{
				Log.Info("selectlimit");
				// Display the Select Limit window and record which Limit was selected.
				frmSelectLimit frmPickLimit = new frmSelectLimit();
				if (strFriendlyName != "")
					frmPickLimit.Description = LanguageManager.Instance.GetString("String_Improvement_SelectLimitNamed")
						.Replace("{0}", strFriendlyName);
				else
					frmPickLimit.Description = LanguageManager.Instance.GetString("String_Improvement_SelectLimit");

				Log.Info("selectlimit = " + bonusNode.OuterXml.ToString());

				if (bonusNode.InnerXml.Contains("<limit>"))
				{
					List<string> strValue = new List<string>();
					foreach (XmlNode objXmlAttribute in bonusNode.SelectNodes("limit"))
						strValue.Add(objXmlAttribute.InnerText);
					frmPickLimit.LimitToList(strValue);
				}

				if (bonusNode.InnerXml.Contains("<excludelimit>"))
				{
					List<string> strValue = new List<string>();
					foreach (XmlNode objXmlAttribute in bonusNode.SelectNodes("excludelimit"))
						strValue.Add(objXmlAttribute.InnerText);
					frmPickLimit.RemoveFromList(strValue);
				}

				// Check to see if there is only one possible selection because of _strLimitSelection.
				if (_strForcedValue != "")
					_strLimitSelection = _strForcedValue;

				Log.Info("_strForcedValue = " + _strForcedValue);
				Log.Info("_strLimitSelection = " + _strLimitSelection);

				if (_strLimitSelection != "")
				{
					frmPickLimit.SingleLimit(_strLimitSelection);
					frmPickLimit.Opacity = 0;
				}

				frmPickLimit.ShowDialog();

				// Make sure the dialogue window was not canceled.
				if (frmPickLimit.DialogResult == DialogResult.Cancel)
				{
					Rollback();
					_strForcedValue = "";
					return false;
				}

				_strSelectedValue = frmPickLimit.SelectedLimit;
				if (blnConcatSelectedValue)
					strSourceName += " (" + _strSelectedValue + ")";

				// Record the improvement.
				int intMin = 0;
				int intAug = 0;
				int intMax = 0;
				int intAugMax = 0;

				// Extract the modifiers.
				if (bonusNode.InnerXml.Contains("min"))
					intMin = ValueToInt(bonusNode["min"].InnerXml, intRating);
				if (bonusNode.InnerXml.Contains("val"))
					intAug = ValueToInt(bonusNode["val"].InnerXml, intRating);
				if (bonusNode.InnerXml.Contains("max"))
					intMax = ValueToInt(bonusNode["max"].InnerXml, intRating);
				if (bonusNode.InnerXml.Contains("aug"))
					intAugMax = ValueToInt(bonusNode["aug"].InnerXml, intRating);

				string strLimit = frmPickLimit.SelectedLimit;

				if (bonusNode["affectbase"] != null)
					strLimit += "Base";

				Log.Info("_strSelectedValue = " + _strSelectedValue);
				Log.Info("strSourceName = " + strSourceName);

				LimitModifier objLimitMod = new LimitModifier(_objCharacter);
				// string strBonus = bonusNode["value"].InnerText;
				int intBonus = intAug;
				string strName = strFriendlyName;
				TreeNode nodTemp = new TreeNode();
				Improvement.ImprovementType objType = Improvement.ImprovementType.PhysicalLimit;

				switch (strLimit)
				{
					case "Mental":
						{
							objType = Improvement.ImprovementType.MentalLimit;
							break;
						}
					case "Social":
						{
							objType = Improvement.ImprovementType.SocialLimit;
							break;
						}
					default:
						{
							objType = Improvement.ImprovementType.PhysicalLimit;
							break;
						}
				}

				Log.Info("Calling CreateImprovement");
				CreateImprovement(strLimit, objImprovementSource, strSourceName, objType, strFriendlyName, intBonus, 0, intMin,
					intMax,
					intAug, intAugMax);
			}

			// Select an CharacterAttribute to use instead of the default on a skill.
			if (bonusNode.LocalName == ("swapskillattribute"))
			{
				Log.Info("swapskillattribute");
				// Display the Select Attribute window and record which Skill was selected.
				frmSelectAttribute frmPickAttribute = new frmSelectAttribute();
				if (strFriendlyName != "")
					frmPickAttribute.Description =
						LanguageManager.Instance.GetString("String_Improvement_SelectAttributeNamed").Replace("{0}", strFriendlyName);
				else
					frmPickAttribute.Description = LanguageManager.Instance.GetString("String_Improvement_SelectAttribute");

				List<string> strValue = new List<string>();
				strValue.Add("LOG");
				strValue.Add("WIL");
				strValue.Add("INT");
				strValue.Add("CHA");
				strValue.Add("EDG");
				strValue.Add("MAG");
				strValue.Add("RES");
				frmPickAttribute.RemoveFromList(strValue);

				Log.Info("swapskillattribute = " + bonusNode.OuterXml.ToString());

				if (bonusNode.InnerXml.Contains("<attribute>"))
				{
					List<string> strLimitValue = new List<string>();
					foreach (XmlNode objXmlAttribute in bonusNode.SelectNodes("attribute"))
						strLimitValue.Add(objXmlAttribute.InnerText);
					frmPickAttribute.LimitToList(strLimitValue);
				}

				// Check to see if there is only one possible selection because of _strLimitSelection.
				if (_strForcedValue != "")
					_strLimitSelection = _strForcedValue;

				Log.Info("_strForcedValue = " + _strForcedValue);
				Log.Info("_strLimitSelection = " + _strLimitSelection);

				if (_strLimitSelection != "")
				{
					frmPickAttribute.SingleAttribute(_strLimitSelection);
					frmPickAttribute.Opacity = 0;
				}

				frmPickAttribute.ShowDialog();

				// Make sure the dialogue window was not canceled.
				if (frmPickAttribute.DialogResult == DialogResult.Cancel)
				{
					Rollback();
					_strForcedValue = "";
					_strLimitSelection = "";
					return false;
				}

				_strSelectedValue = frmPickAttribute.SelectedAttribute;
				if (blnConcatSelectedValue)
					strSourceName += " (" + _strSelectedValue + ")";

				Log.Info("_strSelectedValue = " + _strSelectedValue);
				Log.Info("strSourceName = " + strSourceName);

				Log.Info("Calling CreateImprovement");
				CreateImprovement(frmPickAttribute.SelectedAttribute, objImprovementSource, strSourceName,
					Improvement.ImprovementType.SwapSkillAttribute, strUnique);
			}

			// Select a Spell.
			if (bonusNode.LocalName == ("selectspell"))
			{
				Log.Info("selectspell");
				// Display the Select Spell window.
				frmSelectSpell frmPickSpell = new frmSelectSpell(_objCharacter);

				if (bonusNode.Attributes["category"] != null)
					frmPickSpell.LimitCategory = bonusNode.Attributes["category"].InnerText;

				Log.Info("selectspell = " + bonusNode.OuterXml.ToString());
				Log.Info("_strForcedValue = " + _strForcedValue);
				Log.Info("_strLimitSelection = " + _strLimitSelection);

				if (_strForcedValue != "")
				{
					frmPickSpell.ForceSpellName = _strForcedValue;
					frmPickSpell.Opacity = 0;
				}

				if (bonusNode.Attributes["ignorerequirements"] != null)
				{
					frmPickSpell.IgnoreRequirements = Convert.ToBoolean(bonusNode.Attributes["ignorerequirements"].InnerText);
				}

				frmPickSpell.ShowDialog();

				// Make sure the dialogue window was not canceled.
				if (frmPickSpell.DialogResult == DialogResult.Cancel)
				{
					Rollback();
					_strForcedValue = "";
					_strLimitSelection = "";
					return false;
				}

				_strSelectedValue = frmPickSpell.SelectedSpell;
				if (blnConcatSelectedValue)
					strSourceName += " (" + _strSelectedValue + ")";

				Log.Info("_strSelectedValue = " + _strSelectedValue);
				Log.Info("strSourceName = " + strSourceName);

				Log.Info("Calling CreateImprovement");
				CreateImprovement(frmPickSpell.SelectedSpell, objImprovementSource, strSourceName, Improvement.ImprovementType.Text,
					strUnique);
			}

			// Select a Contact
			if (bonusNode.LocalName == ("selectcontact"))
			{
				Log.Info("selectcontact");
				XmlNode nodSelect = bonusNode;

				frmSelectItem frmSelect = new frmSelectItem();

				String strMode = NodeExists(nodSelect, "type")
					? nodSelect["type"].InnerText
					: "all";

				List<Contact> selectedContactsList;
				if (strMode == "all")
				{
					selectedContactsList = new List<Contact>(_objCharacter.Contacts);
				}
				else if (strMode == "group" || strMode == "nongroup")
				{
					bool blnGroup = strMode == "group";


					//Select any contact where IsGroup equals blnGroup
					//and add to a list
					selectedContactsList =
						new List<Contact>(from contact in _objCharacter.Contacts
							where contact.IsGroup == blnGroup
							select contact);
				}
				else
				{
					Rollback();
					return false;
				}

				if (selectedContactsList.Count == 0)
				{
					MessageBox.Show(LanguageManager.Instance.GetString("Message_NoContactFound"),
						LanguageManager.Instance.GetString("MessageTitle_NoContactFound"), MessageBoxButtons.OK, MessageBoxIcon.Error);
					Rollback();
					return false;
				}

				int count = 0;
				//Black magic LINQ to cast content of list to another type
				List<ListItem> contacts = new List<ListItem>(from x in selectedContactsList
					select new ListItem() {Name = x.Name, Value = (count++).ToString()});

				String strPrice = NodeExists(nodSelect, "cost")
					? nodSelect["cost"].InnerText
					: "";

				frmSelect.GeneralItems = contacts;
				frmSelect.ShowDialog();

				int index = int.Parse(frmSelect.SelectedItem);
				if (frmSelect.DialogResult != DialogResult.Cancel)
				{
					Contact selectedContact = selectedContactsList[index];

					if (nodSelect["mademan"] != null)
					{
						selectedContact.MadeMan = true;
						CreateImprovement(selectedContact.GUID, Improvement.ImprovementSource.Quality, strSourceName,
							Improvement.ImprovementType.ContactMadeMan, selectedContact.GUID);
					}

					if (String.IsNullOrWhiteSpace(_strSelectedValue))
					{
						_strSelectedValue = selectedContact.Name;
					}
					else
					{
						_strSelectedValue += (", " + selectedContact.Name);
					}
                }
				else
				{
					Rollback();
					return false;
				}
			}

			if (bonusNode.LocalName == "addcontact")
			{
				Log.Info("addcontact");

				int loyalty, connection;
				
				bonusNode.TryGetField("loyalty", out loyalty, 1);
				bonusNode.TryGetField("connection", out connection, 1);
				bool group = bonusNode["group"] != null;
				bool free = bonusNode["free"] != null;

				Contact contact = new Contact(_objCharacter);
				contact.Free = free;
				contact.IsGroup = group;
				contact.Loyalty = loyalty;
				contact.Connection = connection;
				contact.ReadOnly = true;
				_objCharacter.Contacts.Add(contact);

				CreateImprovement(contact.GUID, Improvement.ImprovementSource.Quality, strSourceName,
							Improvement.ImprovementType.AddContact, contact.GUID);
			}

			// Affect a Specific CharacterAttribute.
			if (bonusNode.LocalName == ("specificattribute"))
			{
				Log.Info("specificattribute");

				if (bonusNode["name"].InnerText != "ESS")
				{
					// Display the Select CharacterAttribute window and record which CharacterAttribute was selected.
					// Record the improvement.
					int intMin = 0;
					int intAug = 0;
					int intMax = 0;
					int intAugMax = 0;

					// Extract the modifiers.
					if (bonusNode.InnerXml.Contains("min"))
						intMin = ValueToInt(bonusNode["min"].InnerXml, intRating);
					if (bonusNode.InnerXml.Contains("val"))
						intAug = ValueToInt(bonusNode["val"].InnerXml, intRating);
					if (bonusNode.InnerXml.Contains("max"))
					{
						if (bonusNode["max"].InnerText.Contains("-natural"))
						{
							intMax = Convert.ToInt32(bonusNode["max"].InnerText.Replace("-natural", string.Empty)) -
							         _objCharacter.GetAttribute(bonusNode["name"].InnerText).MetatypeMaximum;
						}
						else
							intMax = ValueToInt(bonusNode["max"].InnerXml, intRating);
					}
					if (bonusNode.InnerXml.Contains("aug"))
						intAugMax = ValueToInt(bonusNode["aug"].InnerXml, intRating);

					string strUseUnique = strUnique;
					if (bonusNode["name"].Attributes["precedence"] != null)
						strUseUnique = "precedence" + bonusNode["name"].Attributes["precedence"].InnerText;

					string strAttribute = bonusNode["name"].InnerText;

					if (bonusNode["affectbase"] != null)
						strAttribute += "Base";
					
					CreateImprovement(strAttribute, objImprovementSource, strSourceName, Improvement.ImprovementType.Attribute,
						strUseUnique, 0, 1, intMin, intMax, intAug, intAugMax);
				}
				else
				{
					CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Essence, "",
						Convert.ToInt32(bonusNode["val"].InnerText));
				}
			}

			// Add a paid increase to an attribute
			if (bonusNode.LocalName == ("attributelevel"))
			{
				Log.Info(new object[] {"attributelevel", bonusNode.OuterXml});
				String strAttrib;
				int value;
				bonusNode.TryGetField("val", out value, 1);

				if (bonusNode.TryGetField("name", out strAttrib))
				{
					CreateImprovement(strAttrib, objImprovementSource, strSourceName,
						Improvement.ImprovementType.Attributelevel, "", value);
				}
				else
				{
					Log.Error(new object[] {"attributelevel", bonusNode.OuterXml});
				}
			}

			if (bonusNode.LocalName == ("skilllevel"))
			{
				Log.Info(new object[] {"skilllevel", bonusNode.OuterXml});
				String strSkill;
				int value;
				bonusNode.TryGetField("val", out value, 1);
                if (bonusNode.TryGetField("name", out strSkill))
				{
					CreateImprovement(strSkill, objImprovementSource, strSourceName,
						Improvement.ImprovementType.SkillLevel, "", value);


				}
				else
				{
					Log.Error(new object[] {"skilllevel", bonusNode.OuterXml});
				}
			}

			if (bonusNode.LocalName == "pushtext")
			{

				String push = bonusNode.InnerText;
				if (!String.IsNullOrWhiteSpace(push))
				{
					_objCharacter.Pushtext.Push(push);
				}
			}

			

			if (bonusNode.LocalName == "knowsoft")
			{
				int val = bonusNode["val"] != null ? ValueToInt(bonusNode["val"].InnerText, intRating) : 1;

				string name;
				if (!string.IsNullOrWhiteSpace(_strForcedValue))
				{
					name = _strForcedValue;
				}
				else if (bonusNode["pick"] != null)
				{
					List<ListItem> types;
					if (bonusNode["group"] != null)
					{
						var v = bonusNode.SelectNodes($"./group");
						types =
							KnowledgeSkill.KnowledgeTypes.Where(x => bonusNode.SelectNodes($"group[. = '{x.Value}']").Count > 0).ToList();

					}
					else if (bonusNode["notgroup"] != null)
					{
						types =
							KnowledgeSkill.KnowledgeTypes.Where(x => bonusNode.SelectNodes($"notgroup[. = '{x.Value}']").Count == 0).ToList();
					}
					else
					{
						types = KnowledgeSkill.KnowledgeTypes;
					}

					frmSelectItem select = new frmSelectItem();
					select.DropdownItems = KnowledgeSkill.KnowledgeSkillsWithCategory(types.Select(x => x.Value).ToArray());

					select.ShowDialog();
					if (select.DialogResult == DialogResult.Cancel)
					{
						return false;
					}

					name = select.SelectedItem;
				}
				else if (bonusNode["name"] != null)
				{
					name = bonusNode["name"].InnerText;
				}
				else
				{
					//TODO some kind of error handling
					Log.Error(new[] {bonusNode.OuterXml, "Missing pick or name"});
					return false;
				}
				_strSelectedValue = name;


				KnowledgeSkill skill = new KnowledgeSkill(_objCharacter, name);

				bool knowsoft = bonusNode.TryCheckValue("require", "skilljack");

				if (knowsoft)
				{
					_objCharacter.SkillsSection.KnowsoftSkills.Add(skill);
					if (_objCharacter.SkillsoftAccess)
					{
						_objCharacter.SkillsSection.KnowledgeSkills.Add(skill);
					}
				}
				else
				{
					_objCharacter.SkillsSection.KnowledgeSkills.Add(skill);
				}

				CreateImprovement(name, objImprovementSource, strSourceName, Improvement.ImprovementType.SkillBase, strUnique, val);
				CreateImprovement(skill.Id.ToString(), objImprovementSource, strSourceName,
					Improvement.ImprovementType.SkillKnowledgeForced, strUnique);

			}

			if (bonusNode.LocalName == "knowledgeskilllevel")
			{
				//Theoretically life modules, right now we just give out free points and let people sort it out themselves.
				//Going to be fun to do the real way, from a computer science perspective, but i don't feel like using 2 weeks on that now

				int val = bonusNode["val"] != null ? ValueToInt(bonusNode["val"].InnerText, intRating) : 1;
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.FreeKnowledgeSkills, "", val);
			}

			if (bonusNode.LocalName == "knowledgeskillpoints")
			{
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.FreeKnowledgeSkills, "", ValueToInt(bonusNode.InnerText,Convert.ToInt32(bonusNode.Value)));
			}

			if (bonusNode.LocalName == ("skillgrouplevel"))
			{
				Log.Info(new object[] {"skillgrouplevel", bonusNode.OuterXml});
				String strSkillGroup;
				int value;
				if (bonusNode.TryGetField("name", out strSkillGroup) &&
				    bonusNode.TryGetField("val", out value))
				{
					CreateImprovement(strSkillGroup, objImprovementSource, strSourceName,
						Improvement.ImprovementType.SkillGroupLevel, "", value);
				}
				else
				{
					Log.Error(new object[] {"skillgrouplevel", bonusNode.OuterXml});
				}
			}

			// Change the maximum number of BP that can be spent on Nuyen.
			if (bonusNode.LocalName == ("nuyenmaxbp"))
			{
				Log.Info("nuyenmaxbp");
				Log.Info("nuyenmaxbp = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.NuyenMaxBP, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Apply a bonus/penalty to physical limit.
			if (bonusNode.LocalName == ("physicallimit"))
			{
				Log.Info("physicallimit");
				Log.Info("physicallimit = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
			    CreateImprovement("Physical", objImprovementSource, strSourceName, Improvement.ImprovementType.PhysicalLimit, strFriendlyName,
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Apply a bonus/penalty to mental limit.
			if (bonusNode.LocalName == ("mentallimit"))
			{
				Log.Info("mentallimit");
				Log.Info("mentallimit = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("Mental", objImprovementSource, strSourceName, Improvement.ImprovementType.MentalLimit, strFriendlyName,
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Apply a bonus/penalty to social limit.
			if (bonusNode.LocalName == ("sociallimit"))
			{
				Log.Info("sociallimit");
				Log.Info("sociallimit = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("Social", objImprovementSource, strSourceName, Improvement.ImprovementType.SocialLimit, strFriendlyName,
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Change the amount of Nuyen the character has at creation time (this can put the character over the amount they're normally allowed).
			if (bonusNode.LocalName == ("nuyenamt"))
			{
				Log.Info("nuyenamt");
				Log.Info("nuyenamt = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Nuyen, strUnique,
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Improve Condition Monitors.
			if (bonusNode.LocalName == ("conditionmonitor"))
			{
				Log.Info("conditionmonitor");
				Log.Info("conditionmonitor = " + bonusNode.OuterXml.ToString());
				// Physical Condition.
				if (bonusNode.InnerXml.Contains("physical"))
				{
					Log.Info("Calling CreateImprovement for Physical");
					CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.PhysicalCM, strUnique,
						ValueToInt(bonusNode["physical"].InnerText, intRating));
				}

				// Stun Condition.
				if (bonusNode.InnerXml.Contains("stun"))
				{
					Log.Info("Calling CreateImprovement for Stun");
					CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.StunCM, strUnique,
						ValueToInt(bonusNode["stun"].InnerText, intRating));
				}

				// Condition Monitor Threshold.
				if (NodeExists(bonusNode, "threshold"))
				{
					string strUseUnique = strUnique;
					if (bonusNode["threshold"].Attributes["precedence"] != null)
						strUseUnique = "precedence" + bonusNode["threshold"].Attributes["precedence"].InnerText;

					Log.Info("Calling CreateImprovement for Threshold");
					CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.CMThreshold, strUseUnique,
						ValueToInt(bonusNode["threshold"].InnerText, intRating));
				}

				// Condition Monitor Threshold Offset. (Additioal boxes appear before the FIRST Condition Monitor penalty)
				if (NodeExists(bonusNode, "thresholdoffset"))
				{
					string strUseUnique = strUnique;
					if (bonusNode["thresholdoffset"].Attributes["precedence"] != null)
						strUseUnique = "precedence" + bonusNode["thresholdoffset"].Attributes["precedence"].InnerText;

					Log.Info("Calling CreateImprovement for Threshold Offset");
					CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.CMThresholdOffset,
						strUseUnique, ValueToInt(bonusNode["thresholdoffset"].InnerText, intRating));
				}

				// Condition Monitor Overflow.
				if (bonusNode.InnerXml.Contains("overflow"))
				{
					Log.Info("Calling CreateImprovement for Overflow");
					CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.CMOverflow, strUnique,
						ValueToInt(bonusNode["overflow"].InnerText, intRating));
				}
			}

			// Improve Living Personal Attributes.
			if (bonusNode.LocalName == ("livingpersona"))
			{
				Log.Info("livingpersona");
				Log.Info("livingpersona = " + bonusNode.OuterXml.ToString());
				// Response.
				if (bonusNode.InnerXml.Contains("response"))
				{
					Log.Info("Calling CreateImprovement for response");
					CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.LivingPersonaResponse,
						strUnique, ValueToInt(bonusNode["response"].InnerText, intRating));
				}

				// Signal.
				if (bonusNode.InnerXml.Contains("signal"))
				{
					Log.Info("Calling CreateImprovement for signal");
					CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.LivingPersonaSignal,
						strUnique,
						ValueToInt(bonusNode["signal"].InnerText, intRating));
				}

				// Firewall.
				if (bonusNode.InnerXml.Contains("firewall"))
				{
					Log.Info("Calling CreateImprovement for firewall");
					CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.LivingPersonaFirewall,
						strUnique, ValueToInt(bonusNode["firewall"].InnerText, intRating));
				}

				// System.
				if (bonusNode.InnerXml.Contains("system"))
				{
					Log.Info("Calling CreateImprovement for system");
					CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.LivingPersonaSystem,
						strUnique,
						ValueToInt(bonusNode["system"].InnerText, intRating));
				}

				// Biofeedback Filter.
				if (bonusNode.InnerXml.Contains("biofeedback"))
				{
					Log.Info("Calling CreateImprovement for biofeedback");
					CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.LivingPersonaBiofeedback,
						strUnique, ValueToInt(bonusNode["biofeedback"].InnerText, intRating));
				}
			}

			// The Improvement adjusts a specific Skill.
			if (bonusNode.LocalName == ("specificskill"))
			{
				Log.Info("specificskill");
				Log.Info("specificskill = " + bonusNode.OuterXml.ToString());
				bool blnAddToRating = false;
				if (bonusNode["applytorating"] != null)
				{
					if (bonusNode["applytorating"].InnerText == "yes")
						blnAddToRating = true;
				}

				string strUseUnique = strUnique;
				if (bonusNode.Attributes["precedence"] != null)
					strUseUnique = "precedence" + bonusNode.Attributes["precedence"].InnerText;

				// Record the improvement.
				if (bonusNode["bonus"] != null)
				{
					Log.Info("Calling CreateImprovement for bonus");
					CreateImprovement(bonusNode["name"].InnerText, objImprovementSource, strSourceName,
						Improvement.ImprovementType.Skill, strUseUnique, ValueToInt(bonusNode["bonus"].InnerXml, intRating), 1, 0, 0, 0,
						0, "", blnAddToRating);
				}
				if (bonusNode["max"] != null)
				{
					Log.Info("Calling CreateImprovement for max");
					CreateImprovement(bonusNode["name"].InnerText, objImprovementSource, strSourceName,
						Improvement.ImprovementType.Skill, strUseUnique, 0, 1, 0, ValueToInt(bonusNode["max"].InnerText, intRating), 0,
						0,
						"", blnAddToRating);
				}
			}

			if (bonusNode.LocalName == "reflexrecorderoptimization")
			{
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.ReflexRecorderOptimization, strUnique);
			}

			// The Improvement adds a martial art
			if (bonusNode.LocalName == ("martialart"))
			{
				Log.Info("martialart");
				Log.Info("martialart = " + bonusNode.OuterXml.ToString());
				XmlDocument _objXmlDocument = XmlManager.Instance.Load("martialarts.xml");
				XmlNode objXmlArt =
					_objXmlDocument.SelectSingleNode("/chummer/martialarts/martialart[name = \"" + bonusNode.InnerText +
					                                 "\"]");

				TreeNode objNode = new TreeNode();
				MartialArt objMartialArt = new MartialArt(_objCharacter);
				objMartialArt.Create(objXmlArt, objNode, _objCharacter);
				objMartialArt.IsQuality = true;
				_objCharacter.MartialArts.Add(objMartialArt);
			}

			// The Improvement adds a limit modifier
			if (bonusNode.LocalName == ("limitmodifier"))
			{
				Log.Info("limitmodifier");
				Log.Info("limitmodifier = " + bonusNode.OuterXml.ToString());
				LimitModifier objLimitMod = new LimitModifier(_objCharacter);
				string strLimit = bonusNode["limit"].InnerText;
				string strBonus = bonusNode["value"].InnerText;
			    if (strBonus == "Rating")
			    {
			        strBonus = intRating.ToString();
			    }
				string strCondition = "";
				try
				{
					strCondition = bonusNode["condition"].InnerText;
				}
				catch
				{
				}
				int intBonus = 0;
				if (strBonus == "Rating")
					intBonus = intRating;
				else
					intBonus = Convert.ToInt32(strBonus);
				string strName = strFriendlyName;
				TreeNode nodTemp = new TreeNode();
				Log.Info("Calling CreateImprovement");
				CreateImprovement(strLimit, objImprovementSource, strSourceName, Improvement.ImprovementType.LimitModifier,
					strFriendlyName, intBonus, 0, 0, 0, 0, 0, strCondition);
			}

			// The Improvement adjusts a Skill Category.
			if (bonusNode.LocalName == ("skillcategory"))
			{
				Log.Info("skillcategory");
				Log.Info("skillcategory = " + bonusNode.OuterXml.ToString());

				bool blnAddToRating = false;
				if (bonusNode["applytorating"] != null)
				{
					if (bonusNode["applytorating"].InnerText == "yes")
						blnAddToRating = true;
				}
				if (bonusNode.InnerXml.Contains("exclude"))
				{
					Log.Info("Calling CreateImprovement - exclude");
					CreateImprovement(bonusNode["name"].InnerText, objImprovementSource, strSourceName,
						Improvement.ImprovementType.SkillCategory, strUnique, ValueToInt(bonusNode["bonus"].InnerXml, intRating), 1, 0,
						0,
						0, 0, bonusNode["exclude"].InnerText, blnAddToRating);
				}
				else
				{
					Log.Info("Calling CreateImprovement");
					CreateImprovement(bonusNode["name"].InnerText, objImprovementSource, strSourceName,
						Improvement.ImprovementType.SkillCategory, strUnique, ValueToInt(bonusNode["bonus"].InnerXml, intRating), 1, 0,
						0,
						0, 0, "", blnAddToRating);
				}
			}

			// The Improvement adjusts a Skill Group.
			if (bonusNode.LocalName == ("skillgroup"))
			{
				Log.Info("skillgroup");
				Log.Info("skillgroup = " + bonusNode.OuterXml.ToString());

				bool blnAddToRating = false;
				if (bonusNode["applytorating"] != null)
				{
					if (bonusNode["applytorating"].InnerText == "yes")
						blnAddToRating = true;
				}
				if (bonusNode.InnerXml.Contains("exclude"))
				{
					Log.Info("Calling CreateImprovement - exclude");
					CreateImprovement(bonusNode["name"].InnerText, objImprovementSource, strSourceName,
						Improvement.ImprovementType.SkillGroup, strUnique, ValueToInt(bonusNode["bonus"].InnerXml, intRating), 1, 0, 0, 0,
						0, bonusNode["exclude"].InnerText, blnAddToRating);
				}
				else
				{
					Log.Info("Calling CreateImprovement");
					CreateImprovement(bonusNode["name"].InnerText, objImprovementSource, strSourceName,
						Improvement.ImprovementType.SkillGroup, strUnique, ValueToInt(bonusNode["bonus"].InnerXml, intRating), 1, 0, 0, 0,
						0, "", blnAddToRating);
				}
			}

			// The Improvement adjust Skills with the given CharacterAttribute.
			if (bonusNode.LocalName == ("skillattribute"))
			{
				Log.Info("skillattribute");
				Log.Info("skillattribute = " + bonusNode.OuterXml.ToString());

				string strUseUnique = strUnique;
				if (bonusNode["name"].Attributes["precedence"] != null)
					strUseUnique = "precedence" + bonusNode["name"].Attributes["precedence"].InnerText;

				bool blnAddToRating = false;
				if (bonusNode["applytorating"] != null)
				{
					if (bonusNode["applytorating"].InnerText == "yes")
						blnAddToRating = true;
				}
				if (bonusNode.InnerXml.Contains("exclude"))
				{
					Log.Info("Calling CreateImprovement - exclude");
					CreateImprovement(bonusNode["name"].InnerText, objImprovementSource, strSourceName,
						Improvement.ImprovementType.SkillAttribute, strUseUnique, ValueToInt(bonusNode["bonus"].InnerXml, intRating), 1,
						0, 0, 0, 0, bonusNode["exclude"].InnerText, blnAddToRating);
				}
				else
				{
					Log.Info("Calling CreateImprovement");
					CreateImprovement(bonusNode["name"].InnerText, objImprovementSource, strSourceName,
						Improvement.ImprovementType.SkillAttribute, strUseUnique, ValueToInt(bonusNode["bonus"].InnerXml, intRating), 1,
						0, 0, 0, 0, "", blnAddToRating);
				}
			}

			// The Improvement comes from Enhanced Articulation (improves Physical Active Skills linked to a Physical CharacterAttribute).
			if (bonusNode.LocalName == ("skillarticulation"))
			{
				Log.Info("skillarticulation");
				Log.Info("skillarticulation = " + bonusNode.OuterXml.ToString());

				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.EnhancedArticulation,
					strUnique,
					ValueToInt(bonusNode["bonus"].InnerText, intRating));
			}

			// Check for Armor modifiers.
			if (bonusNode.LocalName == ("armor"))
			{
				Log.Info("armor");
				Log.Info("armor = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				string strUseUnique = strUnique;
				if (bonusNode.Attributes["precedence"] != null)
				{
					strUseUnique = "precedence" + bonusNode.Attributes["precedence"].InnerText;
				}
				else if (bonusNode.Attributes["group"] != null)
				{
					strUseUnique = "group" + bonusNode.Attributes["group"].InnerText;
				}
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Armor, strUseUnique,
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Reach modifiers.
			if (bonusNode.LocalName == ("reach"))
			{
				Log.Info("reach");
				Log.Info("reach = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Reach, strUnique,
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Unarmed Damage Value modifiers.
			if (bonusNode.LocalName == ("unarmeddv"))
			{
				Log.Info("unarmeddv");
				Log.Info("unarmeddv = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.UnarmedDV, strUnique,
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Unarmed Damage Value Physical.
			if (bonusNode.LocalName == ("unarmeddvphysical"))
			{
				Log.Info("unarmeddvphysical");
				Log.Info("unarmeddvphysical = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.UnarmedDVPhysical, "");
			}

			// Check for Unarmed Armor Penetration.
			if (bonusNode.LocalName == ("unarmedap"))
			{
				Log.Info("unarmedap");
				Log.Info("unarmedap = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.UnarmedAP, strUnique,
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Initiative modifiers.
			if (bonusNode.LocalName == ("initiative"))
			{
				Log.Info("initiative");
				Log.Info("initiative = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Initiative, strUnique,
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Initiative Pass modifiers. Only the highest one ever applies.
			if (bonusNode.LocalName == ("initiativepass"))
			{
				Log.Info("initiativepass");
				Log.Info("initiativepass = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.InitiativePass,
					"initiativepass", ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Initiative Pass modifiers. Only the highest one ever applies.
			if (bonusNode.LocalName == ("initiativepassadd"))
			{
				Log.Info("initiativepassadd");
				Log.Info("initiativepassadd = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.InitiativePassAdd, strUnique,
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Matrix Initiative modifiers.
			if (bonusNode.LocalName == ("matrixinitiative"))
			{
				Log.Info("matrixinitiative");
				Log.Info("matrixinitiative = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.MatrixInitiative, strUnique,
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Matrix Initiative Pass modifiers.
			if (bonusNode.LocalName == ("matrixinitiativepass"))
			{
				Log.Info("matrixinitiativepass");
				Log.Info("matrixinitiativepass = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.MatrixInitiativePass,
					"matrixinitiativepass", ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Matrix Initiative Pass modifiers.
			if (bonusNode.LocalName == ("matrixinitiativepassadd"))
			{
				Log.Info("matrixinitiativepassadd");
				Log.Info("matrixinitiativepassadd = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.MatrixInitiativePass,
					strUnique,
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Lifestyle cost modifiers.
			if (bonusNode.LocalName == ("lifestylecost"))
			{
				Log.Info("lifestylecost");
				Log.Info("lifestylecost = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.LifestyleCost, strUnique,
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for basic Lifestyle cost modifiers.
			if (bonusNode.LocalName == ("basiclifestylecost"))
			{
				Log.Info("basiclifestylecost");
				Log.Info("basiclifestylecost = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.BasicLifestyleCost, strUnique,
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Genetech Cost modifiers.
			if (bonusNode.LocalName == ("genetechcostmultiplier"))
			{
				Log.Info("genetechcostmultiplier");
				Log.Info("genetechcostmultiplier = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.GenetechCostMultiplier,
					strUnique, ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Genetech: Transgenics Cost modifiers.
			if (bonusNode.LocalName == ("transgenicsgenetechcost"))
			{
				Log.Info("transgenicsgenetechcost");
				Log.Info("transgenicsgenetechcost = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.TransgenicsBiowareCost,
					strUnique, ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Basic Bioware Essence Cost modifiers.
			if (bonusNode.LocalName == ("basicbiowareessmultiplier"))
			{
				Log.Info("basicbiowareessmultiplier");
				Log.Info("basicbiowareessmultiplier = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.BasicBiowareEssCost,
					strUnique,
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Bioware Essence Cost modifiers.
			if (bonusNode.LocalName == ("biowareessmultiplier"))
			{
				Log.Info("biowareessmultiplier");
				Log.Info("biowareessmultiplier = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.BiowareEssCost, strUnique,
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Cybeware Essence Cost modifiers.
			if (bonusNode.LocalName == ("cyberwareessmultiplier"))
			{
				Log.Info("cyberwareessmultiplier");
				Log.Info("cyberwareessmultiplier = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.CyberwareEssCost, strUnique,
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Uneducated modifiers.
			if (bonusNode.LocalName == ("uneducated"))
			{
				Log.Info("uneducated");
				Log.Info("uneducated = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Uneducated, strUnique);
				_objCharacter.SkillsSection.Uneducated = true;
			}

			// Check for College Education modifiers.
			if (bonusNode.LocalName == ("collegeeducation"))
			{
				Log.Info("collegeeducation");
				Log.Info("collegeeducation = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.CollegeEducation, strUnique);
				_objCharacter.SkillsSection.CollegeEducation = true;
			}

			// Check for Jack Of All Trades modifiers.
			if (bonusNode.LocalName == ("jackofalltrades"))
			{
				Log.Info("jackofalltrades");
				Log.Info("jackofalltrades = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.JackOfAllTrades, strUnique);
				_objCharacter.SkillsSection.JackOfAllTrades = true;
			}

			// Check for Prototype Transhuman modifiers.
			if (bonusNode.LocalName == ("prototypetranshuman"))
			{
				Log.Info("prototypetranshuman");
				Log.Info("prototypetranshuman = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");

				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.PrototypeTranshuman, strUnique);
				_objCharacter.PrototypeTranshuman = Convert.ToDecimal(bonusNode.InnerText);

			}
			// Check for Uncouth modifiers.
			if (bonusNode.LocalName == ("uncouth"))
			{
				Log.Info("uncouth");
				Log.Info("uncouth = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Uncouth, strUnique);
				_objCharacter.SkillsSection.Uncouth = true;
			}

			// Check for Friends In High Places modifiers.
			if (bonusNode.LocalName == ("friendsinhighplaces"))
			{
				Log.Info("friendsinhighplaces");
				Log.Info("friendsinhighplaces = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.FriendsInHighPlaces,
					strUnique);
				_objCharacter.FriendsInHighPlaces = true;
			}
			// Check for School of Hard Knocks modifiers.
			if (bonusNode.LocalName == ("schoolofhardknocks"))
			{
				Log.Info("schoolofhardknocks");
				Log.Info("schoolofhardknocks = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.SchoolOfHardKnocks, strUnique);
				_objCharacter.SkillsSection.SchoolOfHardKnocks = true;
			}
			// Check for ExCon modifiers.
			if (bonusNode.LocalName == ("excon"))
			{
				Log.Info("ExCon");
				Log.Info("ExCon = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.ExCon, strUnique);
				_objCharacter.ExCon = true;
			}

			// Check for TrustFund modifiers.
			if (bonusNode.LocalName == ("trustfund"))
			{
				Log.Info("TrustFund");
				Log.Info("TrustFund = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.TrustFund,
					strUnique,
					ValueToInt(bonusNode.InnerText, intRating));
				_objCharacter.TrustFund = ValueToInt(bonusNode.InnerText, intRating);
			}

			// Check for Tech School modifiers.
			if (bonusNode.LocalName == ("techschool"))
			{
				Log.Info("techschool");
				Log.Info("techschool = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.TechSchool, strUnique);
				_objCharacter.SkillsSection.TechSchool = true;
			}
			// Check for MadeMan modifiers.
			if (bonusNode.LocalName == ("mademan"))
			{
				Log.Info("MadeMan");
				Log.Info("MadeMan = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.MadeMan, strUnique);
				_objCharacter.MadeMan = true;
			}

			// Check for Linguist modifiers.
			if (bonusNode.LocalName == ("linguist"))
			{
				Log.Info("Linguist");
				Log.Info("Linguist = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Linguist, strUnique);
				_objCharacter.SkillsSection.Linguist = true;
			}

			// Check for LightningReflexes modifiers.
			if (bonusNode.LocalName == ("lightningreflexes"))
			{
				Log.Info("LightningReflexes");
				Log.Info("LightningReflexes = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.LightningReflexes, strUnique);
				_objCharacter.LightningReflexes = true;
			}

			// Check for Fame modifiers.
			if (bonusNode.LocalName == ("fame"))
			{
				Log.Info("Fame");
				Log.Info("Fame = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Fame, strUnique);
				_objCharacter.Fame = true;
			}
			// Check for BornRich modifiers.
			if (bonusNode.LocalName == ("bornrich"))
			{
				Log.Info("BornRich");
				Log.Info("BornRich = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.BornRich, strUnique);
				_objCharacter.BornRich = true;
			}
			// Check for Erased modifiers.
			if (bonusNode.LocalName == ("erased"))
			{
				Log.Info("Erased");
				Log.Info("Erased = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Erased, strUnique);
				_objCharacter.Erased = true;
			}
			// Check for Erased modifiers.
			if (bonusNode.LocalName == ("overclocker"))
			{
				Log.Info("OverClocker");
				Log.Info("Overclocker = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Overclocker, strUnique);
				_objCharacter.Overclocker = true;
			}

			// Check for Restricted Gear modifiers.
			if (bonusNode.LocalName == ("restrictedgear"))
			{
				Log.Info("restrictedgear");
				Log.Info("restrictedgear = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.RestrictedGear, strUnique);
				_objCharacter.RestrictedGear = true;
			}

			// Check for Adept Linguistics.
			if (bonusNode.LocalName == ("adeptlinguistics"))
			{
				Log.Info("adeptlinguistics");
				Log.Info("adeptlinguistics = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.AdeptLinguistics, strUnique,
					1);
			}

			// Check for Weapon Category DV modifiers.
			if (bonusNode.LocalName == ("weaponcategorydv"))
			{
				//TODO: FIX THIS
				/*
				 * I feel like talking a little bit about improvementmanager at
				 * this point. It is an intresting class. First of all, it 
				 * manages to throw out everything we ever learned about OOP
				 * and create a class based on functional programming.
				 * 
				 * That is true, it is a class, based on manipulating a single
				 * list on another class.
				 * 
				 * But atleast there is a reference to it somewhere right?
				 * 
				 * No, you create one wherever you need it, meaning there are
				 * tens of instances of this class, all operating on the same 
				 * list
				 * 
				 * After that, it is just plain stupid.
				 * If you have an list of xmlNodes and some might be the same
				 * it checks if a specific node exists (sometimes even by text
				 * comparison on .OuterXml) and then runs specific code for 
				 * each. If it is there multiple times either of those 2 things
				 * happen.
				 * 
				 * 1. Sad, nothing we can do, guess you have to survive
				 * 2. Lets create a foreach in that specific part of the code
				 * 
				 * Fuck ImprovementManager, kill it with fire, burn the ashes
				 * and feed what remains to a dragon that eats unholy 
				 * abominations
				 */


				Log.Info("weaponcategorydv");
				Log.Info("weaponcategorydv = " + bonusNode.OuterXml.ToString());
				XmlNodeList objXmlCategoryList = bonusNode.SelectNodes("weaponcategorydv");
				XmlNode nodWeapon = bonusNode;

				if (NodeExists(nodWeapon, "selectskill"))
				{
					// Display the Select Skill window and record which Skill was selected.
					frmSelectItem frmPickCategory = new frmSelectItem();
					List<ListItem> lstGeneralItems = new List<ListItem>();

					ListItem liBlades = new ListItem();
					liBlades.Name = "Blades";
					liBlades.Value = "Blades";

					ListItem liClubs = new ListItem();
					liClubs.Name = "Clubs";
					liClubs.Value = "Clubs";

					ListItem liUnarmed = new ListItem();
					liUnarmed.Name = "Unarmed";
					liUnarmed.Value = "Unarmed";

					ListItem liAstral = new ListItem();
					liAstral.Name = "Astral Combat";
					liAstral.Value = "Astral Combat";

					ListItem liExotic = new ListItem();
					liExotic.Name = "Exotic Melee Weapons";
					liExotic.Value = "Exotic Melee Weapons";

					lstGeneralItems.Add(liAstral);
					lstGeneralItems.Add(liBlades);
					lstGeneralItems.Add(liClubs);
					lstGeneralItems.Add(liExotic);
					lstGeneralItems.Add(liUnarmed);
					frmPickCategory.GeneralItems = lstGeneralItems;

					if (strFriendlyName != "")
						frmPickCategory.Description =
							LanguageManager.Instance.GetString("String_Improvement_SelectSkillNamed").Replace("{0}", strFriendlyName);
					else
						frmPickCategory.Description = LanguageManager.Instance.GetString("Title_SelectWeaponCategory");

					Log.Info("_strForcedValue = " + _strForcedValue);

					if (_strForcedValue.StartsWith("Adept:") || _strForcedValue.StartsWith("Magician:"))
						_strForcedValue = "";

					if (_strForcedValue != "")
					{
						frmPickCategory.Opacity = 0;
					}
					frmPickCategory.ShowDialog();

					// Make sure the dialogue window was not canceled.
					if (frmPickCategory.DialogResult == DialogResult.Cancel)
					{
						Rollback();
						_strForcedValue = "";
						_strLimitSelection = "";
						return false;
					}

					_strSelectedValue = frmPickCategory.SelectedItem;

					Log.Info("strSelected = " + _strSelectedValue);

					foreach (Power objPower in _objCharacter.Powers)
					{
						if (objPower.InternalId == strSourceName)
						{
							objPower.Extra = _strSelectedValue;
						}
					}

					Log.Info("Calling CreateImprovement");
					CreateImprovement(_strSelectedValue, objImprovementSource, strSourceName,
						Improvement.ImprovementType.WeaponCategoryDV, strUnique, ValueToInt(nodWeapon["bonus"].InnerXml, intRating));
				}
				else
				{
					// Run through each of the Skill Groups since there may be more than one affected.
					foreach (XmlNode objXmlCategory in objXmlCategoryList)
					{
						Log.Info("Calling CreateImprovement");
						CreateImprovement(objXmlCategory["name"].InnerText, objImprovementSource, strSourceName,
							Improvement.ImprovementType.WeaponCategoryDV, strUnique, ValueToInt(objXmlCategory["bonus"].InnerXml, intRating));
					}
				}
			}

			// Check for Mentor Spirit bonuses.
			if (bonusNode.LocalName == ("selectmentorspirit"))
			{
				Log.Info("selectmentorspirit");
				Log.Info("selectmentorspirit = " + bonusNode.OuterXml.ToString());
				frmSelectMentorSpirit frmPickMentorSpirit = new frmSelectMentorSpirit(_objCharacter);
				frmPickMentorSpirit.ShowDialog();

				// Make sure the dialogue window was not canceled.
				if (frmPickMentorSpirit.DialogResult == DialogResult.Cancel)
				{
					Rollback();
					_strForcedValue = "";
					_strLimitSelection = "";
					return false;
				}

				_strSelectedValue = frmPickMentorSpirit.SelectedMentor;

				string strHoldValue = _strSelectedValue;
				if (blnConcatSelectedValue)
					strSourceName += " (" + _strSelectedValue + ")";

				Log.Info("_strSelectedValue = " + _strSelectedValue);
				Log.Info("strSourceName = " + strSourceName);

				if (frmPickMentorSpirit.BonusNode != null)
				{
					if (!CreateImprovements(objImprovementSource, strSourceName, frmPickMentorSpirit.BonusNode,
						blnConcatSelectedValue, intRating, strFriendlyName))
					{
						Rollback();
						_strForcedValue = "";
						_strLimitSelection = "";
						return false;
					}
				}

				if (frmPickMentorSpirit.Choice1BonusNode != null)
				{
					Log.Info("frmPickMentorSpirit.Choice1BonusNode = " + frmPickMentorSpirit.Choice1BonusNode.OuterXml.ToString());
					string strForce = _strForcedValue;
					if (!frmPickMentorSpirit.Choice1.StartsWith("Adept:") && !frmPickMentorSpirit.Choice1.StartsWith("Magician:"))
						_strForcedValue = frmPickMentorSpirit.Choice1;
					else
						_strForcedValue = "";
					Log.Info("Calling CreateImprovement");
					bool blnSuccess = CreateImprovements(objImprovementSource, strSourceName, frmPickMentorSpirit.Choice1BonusNode,
						blnConcatSelectedValue, intRating, strFriendlyName);
					if (!blnSuccess)
					{
						Rollback();
						_strForcedValue = "";
						_strLimitSelection = "";
						return false;
					}
					_strForcedValue = strForce;
					_objCharacter.Improvements.Last().Notes = frmPickMentorSpirit.Choice1;
				}

				if (frmPickMentorSpirit.Choice2BonusNode != null)
				{
					Log.Info("frmPickMentorSpirit.Choice2BonusNode = " + frmPickMentorSpirit.Choice2BonusNode.OuterXml.ToString());
					string strForce = _strForcedValue;
					if (!frmPickMentorSpirit.Choice2.StartsWith("Adept:") && !frmPickMentorSpirit.Choice2.StartsWith("Magician:"))
						_strForcedValue = frmPickMentorSpirit.Choice2;
					else
						_strForcedValue = "";
					Log.Info("Calling CreateImprovement");
					bool blnSuccess = CreateImprovements(objImprovementSource, strSourceName, frmPickMentorSpirit.Choice2BonusNode,
						blnConcatSelectedValue, intRating, strFriendlyName);
					if (!blnSuccess)
					{
						Rollback();
						_strForcedValue = "";
						_strLimitSelection = "";
						return false;
					}
					_strForcedValue = strForce;
					_objCharacter.Improvements.Last().Notes = frmPickMentorSpirit.Choice2;
				}

				_strSelectedValue = strHoldValue;
				Log.Info("_strSelectedValue = " + _strSelectedValue);
				Log.Info("_strForcedValue = " + _strForcedValue);
			}

			// Check for Paragon bonuses.
			if (bonusNode.LocalName == ("selectparagon"))
			{
				Log.Info("selectparagon");
				Log.Info("selectparagon = " + bonusNode.OuterXml.ToString());
				frmSelectMentorSpirit frmPickMentorSpirit = new frmSelectMentorSpirit(_objCharacter);
				frmPickMentorSpirit.XmlFile = "paragons.xml";
				frmPickMentorSpirit.ShowDialog();

				// Make sure the dialogue window was not canceled.
				if (frmPickMentorSpirit.DialogResult == DialogResult.Cancel)
				{
					Rollback();
					_strForcedValue = "";
					_strLimitSelection = "";
					return false;
				}

				_strSelectedValue = frmPickMentorSpirit.SelectedMentor;
				string strHoldValue = _strSelectedValue;
				if (blnConcatSelectedValue)
					strSourceName += " (" + _strSelectedValue + ")";

				if (frmPickMentorSpirit.BonusNode != null)
				{
					bool blnSuccess = CreateImprovements(objImprovementSource, strSourceName, frmPickMentorSpirit.BonusNode,
						blnConcatSelectedValue, intRating, strFriendlyName);
					if (!blnSuccess)
					{
						Rollback();
						_strForcedValue = "";
						_strLimitSelection = "";
						return false;
					}
				}

				if (frmPickMentorSpirit.Choice1BonusNode != null)
				{
					string strForce = _strForcedValue;
					_strForcedValue = frmPickMentorSpirit.Choice1;
					bool blnSuccess = CreateImprovements(objImprovementSource, strSourceName, frmPickMentorSpirit.Choice1BonusNode,
						blnConcatSelectedValue, intRating, strFriendlyName);
					if (!blnSuccess)
					{
						Rollback();
						_strForcedValue = "";
						_strLimitSelection = "";
						return false;
					}
					_strForcedValue = strForce;
					_objCharacter.Improvements.Last().Notes = frmPickMentorSpirit.Choice1;
				}

				if (frmPickMentorSpirit.Choice2BonusNode != null)
				{
					string strForce = _strForcedValue;
					_strForcedValue = frmPickMentorSpirit.Choice2;
					bool blnSuccess = CreateImprovements(objImprovementSource, strSourceName, frmPickMentorSpirit.Choice2BonusNode,
						blnConcatSelectedValue, intRating, strFriendlyName);
					if (!blnSuccess)
					{
						Rollback();
						_strForcedValue = "";
						_strLimitSelection = "";
						return false;
					}
					_strForcedValue = strForce;
					_objCharacter.Improvements.Last().Notes = frmPickMentorSpirit.Choice2;
				}

				_strSelectedValue = strHoldValue;
			}

			// Check for Smartlink bonus.
			if (bonusNode.LocalName == ("smartlink"))
			{
				Log.Info("smartlink");
				Log.Info("smartlink = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Smartlink, "smartlink");
			}

			// Check for Adapsin bonus.
			if (bonusNode.LocalName == ("adapsin"))
			{
				Log.Info("adapsin");
				Log.Info("adapsin = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Adapsin, "adapsin");
			}

			// Check for SoftWeave bonus.
			if (bonusNode.LocalName == ("softweave"))
			{
				Log.Info("softweave");
				Log.Info("softweave = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.SoftWeave, "softweave");
			}

			// Check for Sensitive System.
			if (bonusNode.LocalName == ("sensitivesystem"))
			{
				Log.Info("sensitivesystem");
				Log.Info("sensitivesystem = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.SensitiveSystem,
					"sensitivesystem");
			}

			// Check for Movement Percent.
			if (bonusNode.LocalName == ("movementpercent"))
			{
				Log.Info("movementpercent");
				Log.Info("movementpercent = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.MovementPercent, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Swim Percent.
			if (bonusNode.LocalName == ("swimpercent"))
			{
				Log.Info("swimpercent");
				Log.Info("swimpercent = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.SwimPercent, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Fly Percent.
			if (bonusNode.LocalName == ("flypercent"))
			{
				Log.Info("flypercent");
				Log.Info("flypercent = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.FlyPercent, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Fly Speed.
			if (bonusNode.LocalName == ("flyspeed"))
			{
				Log.Info("flyspeed");
				Log.Info("flyspeed = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.FlySpeed, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for free Positive Qualities.
			if (bonusNode.LocalName == ("freepositivequalities"))
			{
				Log.Info("freepositivequalities");
				Log.Info("freepositivequalities = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.FreePositiveQualities, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for free Negative Qualities.
			if (bonusNode.LocalName == ("freenegativequalities"))
			{
				Log.Info("freenegativequalities");
				Log.Info("freenegativequalities = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.FreeNegativeQualities, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Select Side.
			if (bonusNode.LocalName == ("selectside"))
			{
				Log.Info("selectside");
				Log.Info("selectside = " + bonusNode.OuterXml.ToString());
				frmSelectSide frmPickSide = new frmSelectSide();
				frmPickSide.Description = LanguageManager.Instance.GetString("Label_SelectSide").Replace("{0}", strFriendlyName);
				if (_strForcedValue != "")
					frmPickSide.ForceValue(_strForcedValue);
				else
					frmPickSide.ShowDialog();

				// Make sure the dialogue window was not canceled.
				if (frmPickSide.DialogResult == DialogResult.Cancel)
				{
					Rollback();
					_strForcedValue = "";
					_strLimitSelection = "";
					return false;
				}

				_strSelectedValue = frmPickSide.SelectedSide;
				Log.Info("_strSelectedValue = " + _strSelectedValue);
			}

			// Check for Free Spirit Power Points.
			if (bonusNode.LocalName == ("freespiritpowerpoints"))
			{
				Log.Info("freespiritpowerpoints");
				Log.Info("freespiritpowerpoints = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.FreeSpiritPowerPoints, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Adept Power Points.
			if (bonusNode.LocalName == ("adeptpowerpoints"))
			{
				Log.Info("adeptpowerpoints");
				Log.Info("adeptpowerpoints = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.AdeptPowerPoints, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Adept Powers
			if (bonusNode.LocalName == ("specificpower"))
			{
				//TODO: Probably broken
				Log.Info("specificpower");
				Log.Info("specificpower = " + bonusNode.OuterXml.ToString());
				// If the character isn't an adept or mystic adept, skip the rest of this.
				if (_objCharacter.AdeptEnabled)
				{
					string strSelection = "";
					_strForcedValue = "";


					Log.Info("objXmlSpecificPower = " + bonusNode.OuterXml.ToString());

					string strPowerName = bonusNode["name"].InnerText;
					int intLevels = 0;
					if (bonusNode["val"] != null)
						intLevels = Convert.ToInt32(bonusNode["val"].InnerText);
					bool blnFree = false;
					if (bonusNode["free"] != null)
						blnFree = (bonusNode["free"].InnerText == "yes");

					string strPowerNameLimit = strPowerName;
					if (bonusNode["selectlimit"] != null)
					{
						Log.Info("selectlimit = " + bonusNode["selectlimit"].OuterXml.ToString());
						_strForcedValue = "";
						// Display the Select Limit window and record which Limit was selected.
						frmSelectLimit frmPickLimit = new frmSelectLimit();
						if (strFriendlyName != "")
							frmPickLimit.Description = LanguageManager.Instance.GetString("String_Improvement_SelectLimitNamed")
								.Replace("{0}", strFriendlyName);
						else
							frmPickLimit.Description = LanguageManager.Instance.GetString("String_Improvement_SelectLimit");

						if (bonusNode["selectlimit"].InnerXml.Contains("<limit>"))
						{
							List<string> strValue = new List<string>();
							foreach (XmlNode objXmlAttribute in bonusNode["selectlimit"].SelectNodes("limit"))
								strValue.Add(objXmlAttribute.InnerText);
							frmPickLimit.LimitToList(strValue);
						}

						if (bonusNode["selectlimit"].InnerXml.Contains("<excludelimit>"))
						{
							List<string> strValue = new List<string>();
							foreach (XmlNode objXmlAttribute in bonusNode["selectlimit"].SelectNodes("excludelimit"))
								strValue.Add(objXmlAttribute.InnerText);
							frmPickLimit.RemoveFromList(strValue);
						}

						// Check to see if there is only one possible selection because of _strLimitSelection.
						if (_strForcedValue != "")
							_strLimitSelection = _strForcedValue;

						Log.Info("_strForcedValue = " + _strForcedValue);
						Log.Info("_strLimitSelection = " + _strLimitSelection);

						if (_strLimitSelection != "")
						{
							frmPickLimit.SingleLimit(_strLimitSelection);
							frmPickLimit.Opacity = 0;
						}

						frmPickLimit.ShowDialog();

						// Make sure the dialogue window was not canceled.
						if (frmPickLimit.DialogResult == DialogResult.Cancel)
						{
							Rollback();
							_strForcedValue = "";
							_strLimitSelection = "";
							return false;
						}

						_strSelectedValue = frmPickLimit.SelectedLimit;
						strSelection = _strSelectedValue;
						_strForcedValue = _strSelectedValue;

						Log.Info("_strForcedValue = " + _strForcedValue);
						Log.Info("_strLimitSelection = " + _strLimitSelection);
					}

					if (bonusNode["selectskill"] != null)
					{
						Log.Info("selectskill = " + bonusNode["selectskill"].OuterXml.ToString());
						XmlNode nodSkill = bonusNode;
						// Display the Select Skill window and record which Skill was selected.
						frmSelectSkill frmPickSkill = new frmSelectSkill(_objCharacter);
						if (strFriendlyName != "")
							frmPickSkill.Description = LanguageManager.Instance.GetString("String_Improvement_SelectSkillNamed")
								.Replace("{0}", strFriendlyName);
						else
							frmPickSkill.Description = LanguageManager.Instance.GetString("String_Improvement_SelectSkill");

						if (nodSkill.SelectSingleNode("selectskill").OuterXml.Contains("skillgroup"))
							frmPickSkill.OnlySkillGroup = nodSkill.SelectSingleNode("selectskill").Attributes["skillgroup"].InnerText;
						else if (nodSkill.SelectSingleNode("selectskill").OuterXml.Contains("skillcategory"))
							frmPickSkill.OnlyCategory = nodSkill.SelectSingleNode("selectskill").Attributes["skillcategory"].InnerText;
						else if (nodSkill.SelectSingleNode("selectskill").OuterXml.Contains("excludecategory"))
							frmPickSkill.ExcludeCategory = nodSkill.SelectSingleNode("selectskill").Attributes["excludecategory"].InnerText;
						else if (nodSkill.SelectSingleNode("selectskill").OuterXml.Contains("limittoskill"))
							frmPickSkill.LimitToSkill = nodSkill.SelectSingleNode("selectskill").Attributes["limittoskill"].InnerText;

						if (_strForcedValue.StartsWith("Adept:") || _strForcedValue.StartsWith("Magician:"))
							_strForcedValue = "";

						Log.Info("_strForcedValue = " + _strForcedValue);
						Log.Info("_strLimitSelection = " + _strLimitSelection);

						if (_strForcedValue != "")
						{
							frmPickSkill.OnlySkill = _strForcedValue;
							frmPickSkill.Opacity = 0;
						}
						frmPickSkill.ShowDialog();

						// Make sure the dialogue window was not canceled.
						if (frmPickSkill.DialogResult == DialogResult.Cancel)
						{
							Rollback();
							_strForcedValue = "";
							_strLimitSelection = "";
							return false;
						}

						_strSelectedValue = frmPickSkill.SelectedSkill;
						_strForcedValue = _strSelectedValue;
						strSelection = _strSelectedValue;

						Log.Info("_strForcedValue = " + _strForcedValue);
						Log.Info("_strSelectedValue = " + _strSelectedValue);
						Log.Info("strSelection = " + strSelection);
					}

					if (bonusNode["selecttext"] != null)
					{
						Log.Info("selecttext = " + bonusNode["selecttext"].OuterXml.ToString());
						frmSelectText frmPickText = new frmSelectText();


						if (_objCharacter.Pushtext.Count > 0)
						{
							strSelection = _objCharacter.Pushtext.Pop();
						}
						else
						{
							frmPickText.Description = LanguageManager.Instance.GetString("String_Improvement_SelectText")
								.Replace("{0}", strFriendlyName);

							Log.Info("_strForcedValue = " + _strForcedValue);
							Log.Info("_strLimitSelection = " + _strLimitSelection);

							if (_strLimitSelection != "")
							{
								frmPickText.SelectedValue = _strLimitSelection;
								frmPickText.Opacity = 0;
							}

							frmPickText.ShowDialog();

							// Make sure the dialogue window was not canceled.
							if (frmPickText.DialogResult == DialogResult.Cancel)
							{
								Rollback();
								_strForcedValue = "";
								_strLimitSelection = "";
								return false;
							}

							strSelection = frmPickText.SelectedValue;
							_strLimitSelection = strSelection;
						}
						Log.Info("_strLimitSelection = " + _strLimitSelection);
						Log.Info("strSelection = " + strSelection);
					}

					if (bonusNode["specificattribute"] != null)
					{
						Log.Info("specificattribute = " + bonusNode["specificattribute"].OuterXml.ToString());
						strSelection = bonusNode["specificattribute"]["name"].InnerText.ToString();
						Log.Info(
							"strSelection = " + strSelection);
					}

					if (bonusNode["selectattribute"] != null)
					{
						Log.Info("selectattribute = " + bonusNode["selectattribute"].OuterXml.ToString());
						XmlNode nodSkill = bonusNode;
						if (_strForcedValue.StartsWith("Adept"))
							_strForcedValue = "";

						// Display the Select CharacterAttribute window and record which CharacterAttribute was selected.
						frmSelectAttribute frmPickAttribute = new frmSelectAttribute();
						if (strFriendlyName != "")
							frmPickAttribute.Description =
								LanguageManager.Instance.GetString("String_Improvement_SelectAttributeNamed").Replace("{0}", strFriendlyName);
						else
							frmPickAttribute.Description = LanguageManager.Instance.GetString("String_Improvement_SelectAttribute");

						// Add MAG and/or RES to the list of Attributes if they are enabled on the form.
						if (_objCharacter.MAGEnabled)
							frmPickAttribute.AddMAG();
						if (_objCharacter.RESEnabled)
							frmPickAttribute.AddRES();

						if (nodSkill["selectattribute"].InnerXml.Contains("<attribute>"))
						{
							List<string> strValue = new List<string>();
							foreach (XmlNode objXmlAttribute in nodSkill["selectattribute"].SelectNodes("attribute"))
								strValue.Add(objXmlAttribute.InnerText);
							frmPickAttribute.LimitToList(strValue);
						}

						if (nodSkill["selectattribute"].InnerXml.Contains("<excludeattribute>"))
						{
							List<string> strValue = new List<string>();
							foreach (XmlNode objXmlAttribute in nodSkill["selectattribute"].SelectNodes("excludeattribute"))
								strValue.Add(objXmlAttribute.InnerText);
							frmPickAttribute.RemoveFromList(strValue);
						}

						// Check to see if there is only one possible selection because of _strLimitSelection.
						if (_strForcedValue != "")
							_strLimitSelection = _strForcedValue;

						Log.Info("_strForcedValue = " + _strForcedValue);
						Log.Info("_strLimitSelection = " + _strLimitSelection);

						if (_strLimitSelection != "")
						{
							frmPickAttribute.SingleAttribute(_strLimitSelection);
							frmPickAttribute.Opacity = 0;
						}

						frmPickAttribute.ShowDialog();

						// Make sure the dialogue window was not canceled.
						if (frmPickAttribute.DialogResult == DialogResult.Cancel)
						{
							Rollback();
							_strForcedValue = "";
							_strLimitSelection = "";
							return false;
						}

						_strSelectedValue = frmPickAttribute.SelectedAttribute;
						if (blnConcatSelectedValue)
							strSourceName += " (" + _strSelectedValue + ")";
						strSelection = _strSelectedValue;
						_strForcedValue = _strSelectedValue;

						Log.Info("_strSelectedValue = " + _strSelectedValue);
						Log.Info("strSourceName = " + strSourceName);
						Log.Info("_strForcedValue = " + _strForcedValue);
					}

					// Check if the character already has this power
					Log.Info("strSelection = " + strSelection);
					bool blnHasPower = false;
					Power objPower = new Power(_objCharacter);
					foreach (Power power in _objCharacter.Powers)
					{
						if (power.Name == strPowerNameLimit)
						{
							if (power.Extra != "" && power.Extra == strSelection)
							{
								blnHasPower = true;
								objPower = power;
							}
							else if (power.Extra == "")
							{
								blnHasPower = true;
								objPower = power;
							}
						}
					}

					Log.Info("blnHasPower = " + blnHasPower);

					if (blnHasPower)
					{
						// If yes, mark it free or give it free levels
						if (blnFree)
						{
							objPower.Free = true;
						}
						else
						{
							objPower.FreeLevels += intLevels;
							if (objPower.Rating < objPower.FreeLevels)
								objPower.Rating = objPower.FreeLevels;
						}
					}
					else
					{
						Log.Info("Adding Power " + strPowerName);
						// If no, add the power and mark it free or give it free levels
						objPower = new Power(_objCharacter);
						_objCharacter.Powers.Add(objPower);

						// Get the Power information
						XmlDocument objXmlDocument = new XmlDocument();
						objXmlDocument = XmlManager.Instance.Load("powers.xml");
						XmlNode objXmlPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"" + strPowerName + "\"]");
						Log.Info("objXmlPower = " + objXmlPower.OuterXml.ToString());

						bool blnLevels = false;
						if (objXmlPower["levels"] != null)
							blnLevels = (objXmlPower["levels"].InnerText != "no");
						objPower.LevelsEnabled = blnLevels;
						objPower.Name = strPowerNameLimit;
						if (strSelection != string.Empty)
							objPower.Extra = strSelection;
						if (objXmlPower["doublecost"] != null)
							objPower.DoubleCost = false;
						objPower.PointsPerLevel = Convert.ToDecimal(objXmlPower["points"].InnerText, GlobalOptions.Instance.CultureInfo);
						objPower.Source = objXmlPower["source"].InnerText;
						objPower.Page = objXmlPower["page"].InnerText;

						if (objPower.LevelsEnabled)
						{
							if (objPower.Name == "Improved Ability (skill)")
							{
									foreach (Skill objSkill in _objCharacter.SkillsSection.Skills)
									{
										if (objPower.Extra == objSkill.Name ||
										    (objSkill.IsExoticSkill &&
										     objPower.Extra == (objSkill.DisplayName + " (" + (objSkill as ExoticSkill).Specific + ")")))
										{
											int intImprovedAbilityMaximum = objSkill.Rating + (objSkill.Rating/2);
											if (intImprovedAbilityMaximum == 0)
											{
												intImprovedAbilityMaximum = 1;
											}
											objPower.MaxLevels = intImprovedAbilityMaximum;
										}
									}
							}
							else if(objXmlPower["levels"].InnerText == "yes")
							{
								objPower.MaxLevels = Convert.ToInt32(objXmlPower["levels"].InnerText);
							}
						}

						if (blnFree && objPower.MaxLevels == 0)
						{
							objPower.Free = true;
						}
						else
						{
							objPower.FreeLevels += intLevels;
							if (objPower.Rating < intLevels)
								objPower.Rating = objPower.FreeLevels;
						}

						if (objXmlPower.InnerXml.Contains("bonus"))
						{
							objPower.Bonus = objXmlPower["bonus"];
							Log.Info("Calling CreateImprovements");
							if (
								!CreateImprovements(Improvement.ImprovementSource.Power, objPower.InternalId, objPower.Bonus, false,
									Convert.ToInt32(objPower.Rating), objPower.DisplayNameShort))
							{
								_objCharacter.Powers.Remove(objPower);
							}
						}
					}
					_strSelectedValue = "";
					_strForcedValue = "";
					strSelection = "";
				}
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.AdeptPower, "");
			}

			// Select a Power.
			if (bonusNode.LocalName == ("selectpowers"))
			{
				XmlNodeList objXmlPowerList = bonusNode.SelectNodes("selectpower");
				foreach (XmlNode objNode in objXmlPowerList)
				{
					Log.Info("selectpower");
					Log.Info("_strSelectedValue = " + _strSelectedValue);
					Log.Info("_strForcedValue = " + _strForcedValue);

					//Gerry: These unfortunately did not work in any case of multiple bonuses
					// Switched the setting of powerpoints and levels to ADDING them
					// Remove resetting powerpoints.
					bool blnExistingPower = false;
					foreach (Power objExistingPower in _objCharacter.Powers)
					{
						if (objExistingPower.Name.StartsWith("Improved Reflexes"))
						{
							if (objExistingPower.Name.EndsWith("1"))
							{
								if (objExistingPower.Name.EndsWith("1"))
								{
									if (intRating >= 6)
										objExistingPower.FreePoints += 1.5M;
									//else
									//	objExistingPower.FreePoints = 0;
								}
								else if (objExistingPower.Name.EndsWith("2"))
								{
									if (intRating >= 10)
										objExistingPower.FreePoints += 2.5M;
									else if (intRating >= 4)
										objExistingPower.FreePoints += 1.0M;
									//else
									//	objExistingPower.FreePoints = 0;
								}
								else
								{
									if (intRating >= 14)
										objExistingPower.FreePoints += 3.5M;
									else if (intRating >= 8)
										objExistingPower.FreePoints += 2.0M;
									else if (intRating >= 4)
										objExistingPower.FreePoints += 1.0M;
									//else
									//	objExistingPower.FreePoints = 0;
								}
							}
							else
							{
								// we have to adjust the number of free levels.
								decimal decLevels = Convert.ToDecimal(intRating)/4;
								decLevels = Math.Floor(decLevels/objExistingPower.PointsPerLevel);
								objExistingPower.FreeLevels += Convert.ToInt32(decLevels);
								if (objExistingPower.Rating < intRating)
									objExistingPower.Rating = objExistingPower.FreeLevels;
								break;
							}
						}
						else
						{
							// we have to adjust the number of free levels.
							decimal decLevels = Convert.ToDecimal(intRating)/4;
							decLevels = Math.Floor(decLevels/objExistingPower.PointsPerLevel);
							objExistingPower.FreeLevels = Convert.ToInt32(decLevels);
							if (objExistingPower.Rating < intRating)
								objExistingPower.Rating = objExistingPower.FreeLevels;
							break;
						}
						//}
					}

					if (!blnExistingPower)
					{
						// Display the Select Skill window and record which Skill was selected.
						frmSelectPower frmPickPower = new frmSelectPower(_objCharacter);
						Log.Info("selectpower = " + objNode.OuterXml.ToString());

						if (objNode.OuterXml.Contains("limittopowers"))
							frmPickPower.LimitToPowers = objNode.Attributes["limittopowers"].InnerText;
						frmPickPower.ShowDialog();

						// Make sure the dialogue window was not canceled.
						if (frmPickPower.DialogResult == DialogResult.Cancel)
						{
							Rollback();
							_strForcedValue = "";
							_strLimitSelection = "";
							return false;
						}

						_strSelectedValue = frmPickPower.SelectedPower;
						if (blnConcatSelectedValue)
							strSourceName += " (" + _strSelectedValue + ")";

						XmlDocument objXmlDocument = XmlManager.Instance.Load("powers.xml");
						XmlNode objXmlPower =
							objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"" + _strSelectedValue + "\"]");
						string strSelection = "";

						Log.Info("_strSelectedValue = " + _strSelectedValue);
						Log.Info("strSourceName = " + strSourceName);

						XmlNode objBonus = objXmlPower["bonus"];

						string strPowerNameLimit = _strSelectedValue;
						if (objBonus != null)
						{
							if (objBonus["selectlimit"] != null)
							{
								Log.Info("selectlimit = " + objBonus["selectlimit"].OuterXml.ToString());
								_strForcedValue = "";
								// Display the Select Limit window and record which Limit was selected.
								frmSelectLimit frmPickLimit = new frmSelectLimit();
								if (strFriendlyName != "")
									frmPickLimit.Description = LanguageManager.Instance.GetString("String_Improvement_SelectLimitNamed")
										.Replace("{0}", strFriendlyName);
								else
									frmPickLimit.Description = LanguageManager.Instance.GetString("String_Improvement_SelectLimit");

								if (objBonus["selectlimit"].InnerXml.Contains("<limit>"))
								{
									List<string> strValue = new List<string>();
									foreach (XmlNode objXmlAttribute in objBonus["selectlimit"].SelectNodes("limit"))
										strValue.Add(objXmlAttribute.InnerText);
									frmPickLimit.LimitToList(strValue);
								}

								if (objBonus["selectlimit"].InnerXml.Contains("<excludelimit>"))
								{
									List<string> strValue = new List<string>();
									foreach (XmlNode objXmlAttribute in objBonus["selectlimit"].SelectNodes("excludelimit"))
										strValue.Add(objXmlAttribute.InnerText);
									frmPickLimit.RemoveFromList(strValue);
								}

								// Check to see if there is only one possible selection because of _strLimitSelection.
								if (_strForcedValue != "")
									_strLimitSelection = _strForcedValue;

								Log.Info("_strForcedValue = " + _strForcedValue);
								Log.Info("_strLimitSelection = " + _strLimitSelection);

								if (_strLimitSelection != "")
								{
									frmPickLimit.SingleLimit(_strLimitSelection);
									frmPickLimit.Opacity = 0;
								}

								frmPickLimit.ShowDialog();

								// Make sure the dialogue window was not canceled.
								if (frmPickLimit.DialogResult == DialogResult.Cancel)
								{
									Rollback();
									_strForcedValue = "";
									_strLimitSelection = "";
									return false;
								}

								_strSelectedValue = frmPickLimit.SelectedLimit;
								strSelection = _strSelectedValue;
								_strForcedValue = _strSelectedValue;

								Log.Info("_strForcedValue = " + _strForcedValue);
								Log.Info("_strLimitSelection = " + _strLimitSelection);
							}

							if (objBonus["selectskill"] != null)
							{
								Log.Info("selectskill = " + objBonus["selectskill"].OuterXml.ToString());
								XmlNode nodSkill = objBonus;
								// Display the Select Skill window and record which Skill was selected.
								frmSelectSkill frmPickSkill = new frmSelectSkill(_objCharacter);
								if (strFriendlyName != "")
									frmPickSkill.Description = LanguageManager.Instance.GetString("String_Improvement_SelectSkillNamed")
										.Replace("{0}", strFriendlyName);
								else
									frmPickSkill.Description = LanguageManager.Instance.GetString("String_Improvement_SelectSkill");

								if (nodSkill.SelectSingleNode("selectskill").OuterXml.Contains("skillgroup"))
									frmPickSkill.OnlySkillGroup = nodSkill.SelectSingleNode("selectskill").Attributes["skillgroup"].InnerText;
								else if (nodSkill.SelectSingleNode("selectskill").OuterXml.Contains("skillcategory"))
									frmPickSkill.OnlyCategory = nodSkill.SelectSingleNode("selectskill").Attributes["skillcategory"].InnerText;
								else if (nodSkill.SelectSingleNode("selectskill").OuterXml.Contains("excludecategory"))
									frmPickSkill.ExcludeCategory = nodSkill.SelectSingleNode("selectskill").Attributes["excludecategory"].InnerText;
								else if (nodSkill.SelectSingleNode("selectskill").OuterXml.Contains("limittoskill"))
									frmPickSkill.LimitToSkill = nodSkill.SelectSingleNode("selectskill").Attributes["limittoskill"].InnerText;

								if (_strForcedValue.StartsWith("Adept:") || _strForcedValue.StartsWith("Magician:"))
									_strForcedValue = "";

								Log.Info("_strForcedValue = " + _strForcedValue);
								Log.Info("_strLimitSelection = " + _strLimitSelection);

								if (_strForcedValue != "")
								{
									frmPickSkill.OnlySkill = _strForcedValue;
									frmPickSkill.Opacity = 0;
								}
								frmPickSkill.ShowDialog();

								// Make sure the dialogue window was not canceled.
								if (frmPickSkill.DialogResult == DialogResult.Cancel)
								{
									Rollback();
									_strForcedValue = "";
									_strLimitSelection = "";
									return false;
								}

								_strSelectedValue = frmPickSkill.SelectedSkill;
								_strForcedValue = _strSelectedValue;
								strSelection = _strSelectedValue;

								Log.Info("_strForcedValue = " + _strForcedValue);
								Log.Info("_strSelectedValue = " + _strSelectedValue);
								Log.Info("strSelection = " + strSelection);
							}

							if (objBonus["selecttext"] != null)
							{
								Log.Info("selecttext = " + objBonus["selecttext"].OuterXml.ToString());
								frmSelectText frmPickText = new frmSelectText();
								frmPickText.Description = LanguageManager.Instance.GetString("String_Improvement_SelectText")
									.Replace("{0}", strFriendlyName);

								Log.Info("_strForcedValue = " + _strForcedValue);
								Log.Info("_strLimitSelection = " + _strLimitSelection);

								if (_strLimitSelection != "")
								{
									frmPickText.SelectedValue = _strLimitSelection;
									frmPickText.Opacity = 0;
								}

								frmPickText.ShowDialog();

								// Make sure the dialogue window was not canceled.
								if (frmPickText.DialogResult == DialogResult.Cancel)
								{
									Rollback();
									_strForcedValue = "";
									_strLimitSelection = "";
									return false;
								}

								strSelection = frmPickText.SelectedValue;
								_strLimitSelection = strSelection;

								Log.Info("_strLimitSelection = " + _strLimitSelection);
								Log.Info("strSelection = " + strSelection);
							}

							if (objBonus["specificattribute"] != null)
							{
								Log.Info("specificattribute = " + objBonus["specificattribute"].OuterXml.ToString());
								strSelection = objBonus["specificattribute"]["name"].InnerText.ToString();
								Log.Info("strSelection = " + strSelection);
							}

							if (objBonus["selectattribute"] != null)
							{
								Log.Info("selectattribute = " + objBonus["selectattribute"].OuterXml.ToString());
								XmlNode nodSkill = objBonus;
								if (_strForcedValue.StartsWith("Adept"))
									_strForcedValue = "";

								// Display the Select CharacterAttribute window and record which CharacterAttribute was selected.
								frmSelectAttribute frmPickAttribute = new frmSelectAttribute();
								if (strFriendlyName != "")
									frmPickAttribute.Description =
										LanguageManager.Instance.GetString("String_Improvement_SelectAttributeNamed").Replace("{0}", strFriendlyName);
								else
									frmPickAttribute.Description = LanguageManager.Instance.GetString("String_Improvement_SelectAttribute");

								// Add MAG and/or RES to the list of Attributes if they are enabled on the form.
								if (_objCharacter.MAGEnabled)
									frmPickAttribute.AddMAG();
								if (_objCharacter.RESEnabled)
									frmPickAttribute.AddRES();

								if (nodSkill["selectattribute"].InnerXml.Contains("<attribute>"))
								{
									List<string> strValue = new List<string>();
									foreach (XmlNode objXmlAttribute in nodSkill["selectattribute"].SelectNodes("attribute"))
										strValue.Add(objXmlAttribute.InnerText);
									frmPickAttribute.LimitToList(strValue);
								}

								if (nodSkill["selectattribute"].InnerXml.Contains("<excludeattribute>"))
								{
									List<string> strValue = new List<string>();
									foreach (XmlNode objXmlAttribute in nodSkill["selectattribute"].SelectNodes("excludeattribute"))
										strValue.Add(objXmlAttribute.InnerText);
									frmPickAttribute.RemoveFromList(strValue);
								}

								// Check to see if there is only one possible selection because of _strLimitSelection.
								if (_strForcedValue != "")
									_strLimitSelection = _strForcedValue;

								Log.Info("_strForcedValue = " + _strForcedValue);
								Log.Info("_strLimitSelection = " + _strLimitSelection);

								if (_strLimitSelection != "")
								{
									frmPickAttribute.SingleAttribute(_strLimitSelection);
									frmPickAttribute.Opacity = 0;
								}

								frmPickAttribute.ShowDialog();

								// Make sure the dialogue window was not canceled.
								if (frmPickAttribute.DialogResult == DialogResult.Cancel)
								{
									Rollback();
									_strForcedValue = "";
									_strLimitSelection = "";
									return false;
								}

								_strSelectedValue = frmPickAttribute.SelectedAttribute;
								if (blnConcatSelectedValue)
									strSourceName += " (" + _strSelectedValue + ")";
								strSelection = _strSelectedValue;
								_strForcedValue = _strSelectedValue;

								Log.Info("_strSelectedValue = " + _strSelectedValue);
								Log.Info("strSourceName = " + strSourceName);
								Log.Info("_strForcedValue = " + _strForcedValue);
							}
						}

						// If no, add the power and mark it free or give it free levels
						Power objPower = new Power(_objCharacter);
						bool blnHasPower = false;

						foreach (Power power in _objCharacter.Powers)
						{
							if (power.Name == objXmlPower["name"].InnerText)
							{
								if (power.Extra != "" && power.Extra == strSelection)
								{
									blnHasPower = true;
									objPower = power;
								}
								else if (power.Extra == "")
								{
									blnHasPower = true;
									objPower = power;
								}
							}
						}

						Log.Info("blnHasPower = " + blnHasPower);

						if (blnHasPower)
						{
							// If yes, mark it free or give it free levels
							if (objXmlPower["levels"].InnerText == "no")
							{
								if (objPower.Name.StartsWith("Improved Reflexes"))
								{
									if (objPower.Name.EndsWith("1"))
									{
										if (intRating >= 6)
											objPower.FreePoints = 1.5M;
										else
											objPower.FreePoints = 0;
									}
									else if (objPower.Name.EndsWith("2"))
									{
										if (intRating >= 10)
											objPower.FreePoints = 2.5M;
										else if (intRating >= 4)
											objPower.FreePoints = 1.0M;
										else
											objPower.FreePoints = 0;
									}
									else
									{
										if (intRating >= 14)
											objPower.FreePoints = 3.5M;
										else if (intRating >= 8)
											objPower.FreePoints = 2.0M;
										else if (intRating >= 4)
											objPower.FreePoints = 1.0M;
										else
											objPower.FreePoints = 0;
									}
								}
								else
								{
									objPower.Free = true;
								}
							}
							else
							{
								decimal decLevels = Convert.ToDecimal(intRating)/4;
								decLevels = Math.Floor(decLevels/objPower.PointsPerLevel);
								objPower.FreeLevels += Convert.ToInt32(decLevels);
								objPower.Rating += Convert.ToInt32(decLevels);
							}
							objPower.BonusSource = strSourceName;
						}
						else
						{
							Log.Info("Adding Power " + _strSelectedValue);
							// Get the Power information
							_objCharacter.Powers.Add(objPower);
							Log.Info("objXmlPower = " + objXmlPower.OuterXml.ToString());

							bool blnLevels = false;
							if (objXmlPower["levels"] != null)
								blnLevels = (objXmlPower["levels"].InnerText == "yes");
							objPower.LevelsEnabled = blnLevels;
							objPower.Name = objXmlPower["name"].InnerText;
							objPower.PointsPerLevel = Convert.ToDecimal(objXmlPower["points"].InnerText, GlobalOptions.Instance.CultureInfo);
							objPower.Source = objXmlPower["source"].InnerText;
							objPower.Page = objXmlPower["page"].InnerText;
							objPower.BonusSource = strSourceName;
							if (strSelection != string.Empty)
								objPower.Extra = strSelection;
							if (objXmlPower["doublecost"] != null)
								objPower.DoubleCost = false;

							if (objXmlPower["levels"].InnerText == "no")
							{
								if (objPower.Name.StartsWith("Improved Reflexes"))
								{
									if (objPower.Name.EndsWith("1"))
									{
										if (intRating >= 6)
											objPower.FreePoints = 1.5M;
										else
											objPower.FreePoints = 0;
									}
									else if (objPower.Name.EndsWith("2"))
									{
										if (intRating >= 10)
											objPower.FreePoints = 2.5M;
										else if (intRating >= 4)
											objPower.FreePoints = 1.0M;
										else
											objPower.FreePoints = 0;
									}
									else
									{
										if (intRating >= 14)
											objPower.FreePoints = 3.5M;
										else if (intRating >= 8)
											objPower.FreePoints = 2.0M;
										else if (intRating >= 4)
											objPower.FreePoints = 1.0M;
										else
											objPower.FreePoints = 0;
									}
								}
								else
								{
									objPower.Free = true;
								}
							}
							else
							{
								decimal decLevels = Convert.ToDecimal(intRating)/4;
								decLevels = Math.Floor(decLevels/objPower.PointsPerLevel);
								objPower.FreeLevels += Convert.ToInt32(decLevels);
								if (objPower.Rating < intRating)
									objPower.Rating = objPower.FreeLevels;
							}

							if (objXmlPower.InnerXml.Contains("bonus"))
							{
								objPower.Bonus = objXmlPower["bonus"];
								Log.Info("Calling CreateImprovements");
								if (
									!CreateImprovements(Improvement.ImprovementSource.Power, objPower.InternalId, objPower.Bonus, false,
										Convert.ToInt32(objPower.Rating), objPower.DisplayNameShort))
								{
									_objCharacter.Powers.Remove(objPower);
								}
							}
						}
					}
				}
			}

			// Check for Armor Encumbrance Penalty.
			if (bonusNode.LocalName == ("armorencumbrancepenalty"))
			{
				Log.Info("armorencumbrancepenalty");
				Log.Info("armorencumbrancepenalty = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.ArmorEncumbrancePenalty, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Initiation.
			if (bonusNode.LocalName == ("initiation"))
			{
				Log.Info("initiation");
				Log.Info("initiation = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Initiation, "",
					ValueToInt(bonusNode.InnerText, intRating));
				_objCharacter.InitiateGrade += ValueToInt(bonusNode.InnerText, intRating);
			}

			// Check for Submersion.
			if (bonusNode.LocalName == ("submersion"))
			{
				Log.Info("submersion");
				Log.Info("submersion = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Submersion, "",
					ValueToInt(bonusNode.InnerText, intRating));
				_objCharacter.SubmersionGrade += ValueToInt(bonusNode.InnerText, intRating);
			}

			// Check for Skillwires.
			if (bonusNode.LocalName == ("skillwire"))
			{
				Log.Info("skillwire");
				Log.Info("skillwire = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Skillwire, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Hardwires.
			if (bonusNode.LocalName == ("hardwires"))
			{
				Log.Info("hardwire");
				Log.Info("hardwire = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				Cyberware objCyberware = new Cyberware(_objCharacter);
				CommonFunctions _objFunctions = new CommonFunctions();
				objCyberware = _objFunctions.FindCyberware(strSourceName, _objCharacter.Cyberware);
				if (objCyberware == null)
				{
					Log.Info("_strSelectedValue = " + _strSelectedValue);
					Log.Info("_strForcedValue = " + _strForcedValue);

					// Display the Select Skill window and record which Skill was selected.
					frmSelectSkill frmPickSkill = new frmSelectSkill(_objCharacter);
					if (strFriendlyName != "")
						frmPickSkill.Description = LanguageManager.Instance.GetString("String_Improvement_SelectSkillNamed")
							.Replace("{0}", strFriendlyName);
					else
						frmPickSkill.Description = LanguageManager.Instance.GetString("String_Improvement_SelectSkill");

					Log.Info("selectskill = " + bonusNode.OuterXml.ToString());
					if (bonusNode.OuterXml.Contains("skillgroup"))
						frmPickSkill.OnlySkillGroup = bonusNode.Attributes["skillgroup"].InnerText;
					else if (bonusNode.OuterXml.Contains("skillcategory"))
						frmPickSkill.OnlyCategory = bonusNode.Attributes["skillcategory"].InnerText;
					else if (bonusNode.OuterXml.Contains("excludecategory"))
						frmPickSkill.ExcludeCategory = bonusNode.Attributes["excludecategory"].InnerText;
					else if (bonusNode.OuterXml.Contains("limittoskill"))
						frmPickSkill.LimitToSkill = bonusNode.Attributes["limittoskill"].InnerText;
					else if (bonusNode.OuterXml.Contains("limittoattribute"))
						frmPickSkill.LinkedAttribute = bonusNode.Attributes["limittoattribute"].InnerText;

					if (_strForcedValue != "")
					{
						frmPickSkill.OnlySkill = _strForcedValue;
						frmPickSkill.Opacity = 0;
					}
					frmPickSkill.ShowDialog();

					// Make sure the dialogue window was not canceled.
					if (frmPickSkill.DialogResult == DialogResult.Cancel)
					{
						Rollback();
						_strForcedValue = "";
						_strLimitSelection = "";
						return false;
					}

					_strSelectedValue = frmPickSkill.SelectedSkill;
				}
				else
				{
					_strSelectedValue = objCyberware.Location;
				}
				if (blnConcatSelectedValue)
					strSourceName += " (" + _strSelectedValue + ")";

				Log.Info("_strSelectedValue = " + _strSelectedValue);
				Log.Info("strSourceName = " + strSourceName);
				CreateImprovement(_strSelectedValue, objImprovementSource, strSourceName, Improvement.ImprovementType.Hardwire, _strSelectedValue,
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Damage Resistance.
			if (bonusNode.LocalName == ("damageresistance"))
			{
				Log.Info("damageresistance");
				Log.Info("damageresistance = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.DamageResistance,"", 
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Restricted Item Count.
			if (bonusNode.LocalName == ("restricteditemcount"))
			{
				Log.Info("restricteditemcount");
				Log.Info("restricteditemcount = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.RestrictedItemCount, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Judge Intentions.
			if (bonusNode.LocalName == ("judgeintentions"))
			{
				Log.Info("judgeintentions");
				Log.Info("judgeintentions = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.JudgeIntentions, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Composure.
			if (bonusNode.LocalName == ("composure"))
			{
				Log.Info("composure");
				Log.Info("composure = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Composure, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Lift and Carry.
			if (bonusNode.LocalName == ("liftandcarry"))
			{
				Log.Info("liftandcarry");
				Log.Info("liftandcarry = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.LiftAndCarry, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Memory.
			if (bonusNode.LocalName == ("memory"))
			{
				Log.Info("memory");
				Log.Info("memory = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Memory, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Concealability.
			if (bonusNode.LocalName == ("concealability"))
			{
				Log.Info("concealability");
				Log.Info("concealability = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Concealability, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Drain Resistance.
			if (bonusNode.LocalName == ("drainresist"))
			{
				Log.Info("drainresist");
				Log.Info("drainresist = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.DrainResistance, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Fading Resistance.
			if (bonusNode.LocalName == ("fadingresist"))
			{
				Log.Info("fadingresist");
				Log.Info("fadingresist = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.FadingResistance, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Notoriety.
			if (bonusNode.LocalName == ("notoriety"))
			{
				Log.Info("notoriety");
				Log.Info("notoriety = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Notoriety, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Complex Form Limit.
			if (bonusNode.LocalName == ("complexformlimit"))
			{
				Log.Info("complexformlimit");
				Log.Info("complexformlimit = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.ComplexFormLimit, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Spell Limit.
			if (bonusNode.LocalName == ("spelllimit"))
			{
				Log.Info("spelllimit");
				Log.Info("spelllimit = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.SpellLimit, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Spell Category bonuses.
			if (bonusNode.LocalName == ("spellcategory"))
			{
				Log.Info("spellcategory");
				Log.Info("spellcategory = " + bonusNode.OuterXml.ToString());

				string strUseUnique = strUnique;
				if (bonusNode["name"].Attributes["precedence"] != null)
					strUseUnique = "precedence" + bonusNode["name"].Attributes["precedence"].InnerText;

				Log.Info("Calling CreateImprovement");
				CreateImprovement(bonusNode["name"].InnerText, objImprovementSource, strSourceName,
					Improvement.ImprovementType.SpellCategory, strUseUnique, ValueToInt(bonusNode["val"].InnerText, intRating));
			}

			// Check for Throwing Range bonuses.
			if (bonusNode.LocalName == ("throwrange"))
			{
				Log.Info("throwrange");
				Log.Info("throwrange = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.ThrowRange, strUnique,
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Throwing STR bonuses.
			if (bonusNode.LocalName == ("throwstr"))
			{
				Log.Info("throwstr");
				Log.Info("throwstr = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.ThrowSTR, strUnique,
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Skillsoft access.
			if (bonusNode.LocalName == ("skillsoftaccess"))
			{
				Log.Info("skillsoftaccess");
				Log.Info("skillsoftaccess = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.SkillsoftAccess, "");
				_objCharacter.SkillsSection.KnowledgeSkills.AddRange(_objCharacter.SkillsSection.KnowsoftSkills);
			}

			// Check for Quickening Metamagic.
			if (bonusNode.LocalName == ("quickeningmetamagic"))
			{
				Log.Info("quickeningmetamagic");
				Log.Info("quickeningmetamagic = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.QuickeningMetamagic, "");
			}

			// Check for ignore Stun CM Penalty.
			if (bonusNode.LocalName == ("ignorecmpenaltystun"))
			{
				Log.Info("ignorecmpenaltystun");
				Log.Info("ignorecmpenaltystun = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.IgnoreCMPenaltyStun, "");
			}

			// Check for ignore Physical CM Penalty.
			if (bonusNode.LocalName == ("ignorecmpenaltyphysical"))
			{
				Log.Info("ignorecmpenaltyphysical");
				Log.Info("ignorecmpenaltyphysical = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.IgnoreCMPenaltyPhysical, "");
			}

			// Check for a Cyborg Essence which will permanently set the character's ESS to 0.1.
			if (bonusNode.LocalName == ("cyborgessence"))
			{
				Log.Info("cyborgessence");
				Log.Info("cyborgessence = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.CyborgEssence, "");
			}

			// Check for Maximum Essence which will permanently modify the character's Maximum Essence value.
			if (bonusNode.LocalName == ("essencemax"))
			{
				Log.Info("essencemax");
				Log.Info("essencemax = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.EssenceMax, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Select Sprite.
			if (bonusNode.LocalName == ("selectsprite"))
			{
				Log.Info("selectsprite");
				Log.Info("selectsprite = " + bonusNode.OuterXml.ToString());
				XmlDocument objXmlDocument = XmlManager.Instance.Load("critters.xml");
				XmlNodeList objXmlNodeList =
					objXmlDocument.SelectNodes("/chummer/metatypes/metatype[contains(category, \"Sprites\")]");
				List<ListItem> lstCritters = new List<ListItem>();
				foreach (XmlNode objXmlNode in objXmlNodeList)
				{
					ListItem objItem = new ListItem();
					if (objXmlNode["translate"] != null)
						objItem.Name = objXmlNode["translate"].InnerText;
					else
						objItem.Name = objXmlNode["name"].InnerText;
					objItem.Value = objItem.Name;
					lstCritters.Add(objItem);
				}

				frmSelectItem frmPickItem = new frmSelectItem();
				frmPickItem.GeneralItems = lstCritters;
				frmPickItem.ShowDialog();

				if (frmPickItem.DialogResult == DialogResult.Cancel)
				{
					Rollback();
					_strForcedValue = "";
					_strLimitSelection = "";
					return false;
				}

				_strSelectedValue = frmPickItem.SelectedItem;

				Log.Info("Calling CreateImprovement");
				CreateImprovement(frmPickItem.SelectedItem, objImprovementSource, strSourceName,
					Improvement.ImprovementType.AddSprite,
					"");
			}

			// Check for Black Market Discount.
			if (bonusNode.LocalName == ("blackmarketdiscount"))
			{
				Log.Info("blackmarketdiscount");
				Log.Info("blackmarketdiscount = " + bonusNode.OuterXml.ToString());
				Log.Info("Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.BlackMarketDiscount,
					strUnique);
				_objCharacter.BlackMarketDiscount = true;
			}
			// Select Armor (Mostly used for Custom Fit (Stack)).
			if (bonusNode.LocalName == ("selectarmor"))
			{
				Log.Info("selectarmor");
				Log.Info("selectarmor = " + bonusNode.OuterXml.ToString());
				string strSelectedValue = "";
				if (_strForcedValue != "")
					_strLimitSelection = _strForcedValue;

				// Display the Select Item window and record the value that was entered.

				List<ListItem> lstArmors = new List<ListItem>();
				foreach (Armor objArmor in _objCharacter.Armor)
				{
					foreach (ArmorMod objMod in objArmor.ArmorMods)
					{
						if (objMod.Name.StartsWith("Custom Fit"))
						{
							ListItem objItem = new ListItem();
							objItem.Value = objArmor.Name;
							objItem.Name = objArmor.DisplayName;
							lstArmors.Add(objItem);
						}
					}
				}

				if (lstArmors.Count > 0)
				{

					frmSelectItem frmPickItem = new frmSelectItem();
					frmPickItem.Description = LanguageManager.Instance.GetString("String_Improvement_SelectText").Replace("{0}", strFriendlyName);
					frmPickItem.GeneralItems = lstArmors;

					Log.Info( "_strLimitSelection = " + _strLimitSelection);
					Log.Info( "_strForcedValue = " + _strForcedValue);

					if (_strLimitSelection != "")
					{
						frmPickItem.ForceItem = _strLimitSelection;
						frmPickItem.Opacity = 0;
					}

					frmPickItem.ShowDialog();

					// Make sure the dialogue window was not canceled.
					if (frmPickItem.DialogResult == DialogResult.Cancel)
					{
						Rollback();
						_strForcedValue = "";
						_strLimitSelection = "";
						return false;
					}

					_strSelectedValue = frmPickItem.SelectedItem;
					if (blnConcatSelectedValue)
						strSourceName += " (" + _strSelectedValue + ")";

					strSelectedValue = frmPickItem.SelectedItem;
					Log.Info( "_strSelectedValue = " + _strSelectedValue);
					Log.Info( "strSelectedValue = " + strSelectedValue);
				}

			}

			// Select Weapon (custom entry for things like Spare Clip).
			if (bonusNode.LocalName == ("selectweapon"))
			{
				Log.Info("selectweapon");
				Log.Info("selectweapon = " + bonusNode.OuterXml.ToString());
				string strSelectedValue = "";
				if (_strForcedValue != "")
					_strLimitSelection = _strForcedValue;

				if (_objCharacter == null)
				{
					// If the character is null (this is a Vehicle), the user must enter their own string.
					// Display the Select Item window and record the value that was entered.
					frmSelectText frmPickText = new frmSelectText();
					frmPickText.Description = LanguageManager.Instance.GetString("String_Improvement_SelectText")
						.Replace("{0}", strFriendlyName);

					Log.Info("_strLimitSelection = " + _strLimitSelection);
					Log.Info("_strForcedValue = " + _strForcedValue);

					if (_strLimitSelection != "")
					{
						frmPickText.SelectedValue = _strLimitSelection;
						frmPickText.Opacity = 0;
					}

					frmPickText.ShowDialog();

					// Make sure the dialogue window was not canceled.
					if (frmPickText.DialogResult == DialogResult.Cancel)
					{
						Rollback();
						_strForcedValue = "";
						_strLimitSelection = "";
						return false;
					}

					_strSelectedValue = frmPickText.SelectedValue;
					if (blnConcatSelectedValue)
						strSourceName += " (" + _strSelectedValue + ")";

					strSelectedValue = frmPickText.SelectedValue;
					Log.Info("_strSelectedValue = " + _strSelectedValue);
					Log.Info("strSelectedValue = " + strSelectedValue);
				}
				else
				{
					List<ListItem> lstWeapons = new List<ListItem>();
					foreach (Weapon objWeapon in _objCharacter.Weapons)
					{
						ListItem objItem = new ListItem();
						objItem.Value = objWeapon.Name;
						objItem.Name = objWeapon.DisplayName;
						lstWeapons.Add(objItem);
					}

					frmSelectItem frmPickItem = new frmSelectItem();
					frmPickItem.Description = LanguageManager.Instance.GetString("String_Improvement_SelectText")
						.Replace("{0}", strFriendlyName);
					frmPickItem.GeneralItems = lstWeapons;

					Log.Info("_strLimitSelection = " + _strLimitSelection);
					Log.Info("_strForcedValue = " + _strForcedValue);

					if (_strLimitSelection != "")
					{
						frmPickItem.ForceItem = _strLimitSelection;
						frmPickItem.Opacity = 0;
					}

					frmPickItem.ShowDialog();

					// Make sure the dialogue window was not canceled.
					if (frmPickItem.DialogResult == DialogResult.Cancel)
					{
						Rollback();
						_strForcedValue = "";
						_strLimitSelection = "";
						return false;
					}

					_strSelectedValue = frmPickItem.SelectedItem;
					if (blnConcatSelectedValue)
						strSourceName += " (" + _strSelectedValue + ")";

					strSelectedValue = frmPickItem.SelectedItem;
					Log.Info("_strSelectedValue = " + _strSelectedValue);
					Log.Info("strSelectedValue = " + strSelectedValue);
				}

				// Create the Improvement.
				Log.Info("Calling CreateImprovement");
				CreateImprovement(strSelectedValue, objImprovementSource, strSourceName, Improvement.ImprovementType.Text, strUnique);
			}

			// Select an Optional Power.
			if (bonusNode.LocalName == ("optionalpowers"))
			{
				XmlNodeList objXmlPowerList = bonusNode.SelectNodes("optionalpower");
				//Log.Info("selectoptionalpower");
				// Display the Select Attribute window and record which Skill was selected.
				frmSelectOptionalPower frmPickPower = new frmSelectOptionalPower();
				frmPickPower.Description = LanguageManager.Instance.GetString("String_Improvement_SelectOptionalPower");
				string strForcedValue = "";

				List<KeyValuePair<string, string>> lstValue = new List<KeyValuePair<string,string>>();
				foreach (XmlNode objXmlOptionalPower in objXmlPowerList)
				{
					string strQuality = objXmlOptionalPower.InnerText;
					if (objXmlOptionalPower.Attributes["select"] != null)
					{
						strForcedValue = objXmlOptionalPower.Attributes["select"].InnerText;
					}
					lstValue.Add(new KeyValuePair<string, string>(strQuality,strForcedValue));
				}
				frmPickPower.LimitToList(lstValue);


				// Check to see if there is only one possible selection because of _strLimitSelection.
				if (_strForcedValue != "")
					_strLimitSelection = _strForcedValue;

				Log.Info( "_strForcedValue = " + _strForcedValue);
				Log.Info( "_strLimitSelection = " + _strLimitSelection);

				if (_strLimitSelection != "")
				{
					frmPickPower.SinglePower(_strLimitSelection);
					frmPickPower.Opacity = 0;
				}

				frmPickPower.ShowDialog();

				// Make sure the dialogue window was not canceled.
				if (frmPickPower.DialogResult == DialogResult.Cancel)
				{
					Rollback();
					_strForcedValue = "";
					_strLimitSelection = "";
					return false;
				}

				_strSelectedValue = frmPickPower.SelectedPower;
				// Record the improvement.
				XmlDocument objXmlDocument = XmlManager.Instance.Load("critterpowers.xml");
				XmlNode objXmlPowerNode = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"" + _strSelectedValue + "\"]");
				TreeNode objPowerNode = new TreeNode();
				CritterPower objPower = new CritterPower(_objCharacter);
				
                objPower.Create(objXmlPowerNode, _objCharacter, objPowerNode, 0, strForcedValue);
				_objCharacter.CritterPowers.Add(objPower);
			}

			if (bonusNode.LocalName == "publicawareness")
			{
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.PublicAwareness, strUnique, ValueToInt(bonusNode.InnerText,1));
			}

			if (bonusNode.LocalName == "dealerconnection")
			{
				Log.Info("dealerconnection");
				frmSelectItem frmPickItem = new frmSelectItem();
				List<ListItem> lstItems = new List<ListItem>();
				XmlNodeList objXmlList = bonusNode.SelectNodes("category");
				foreach (XmlNode objNode in objXmlList)
				{
					ListItem objItem = new ListItem();
					objItem.Value = objNode.InnerText;
					objItem.Name = objNode.InnerText;
					lstItems.Add(objItem);
				}
				frmPickItem.GeneralItems = lstItems;
				frmPickItem.AllowAutoSelect = false;
				frmPickItem.ShowDialog();
				// Make sure the dialogue window was not canceled.
				if (frmPickItem.DialogResult == DialogResult.Cancel)
				{
					Rollback();
					_strForcedValue = "";
					_strLimitSelection = "";
					return false;
				}

				_strSelectedValue = frmPickItem.SelectedItem;
				if (blnConcatSelectedValue)
					strSourceName += " (" + _strSelectedValue + ")";

				Log.Info("_strSelectedValue = " + _strSelectedValue);
				Log.Info("strSourceName = " + strSourceName);

				// Create the Improvement.
				Log.Info("Calling CreateImprovement");
				CreateImprovement(frmPickItem.SelectedItem, objImprovementSource, strSourceName,
					Improvement.ImprovementType.DealerConnection, strUnique);
			}

			if (bonusNode.LocalName == "unlockskills")
			{
				List<string> options = bonusNode.InnerText.Split(',').Select(x => x.Trim()).ToList();
				string final;
				if (options.Count == 0)
				{
					Utils.BreakIfDebug();
					return false;
				}
				else if (options.Count == 1)
				{
					final = options[0];
				}
				else
				{
					frmSelectItem frmSelect = new frmSelectItem
					{
						AllowAutoSelect = true,
						GeneralItems = options.Select(x => new ListItem(x, x)).ToList()
					};
					
					if (_objCharacter.Pushtext.Count > 0)
					{
						frmSelect.ForceItem = _objCharacter.Pushtext.Pop();
					}

					if (frmSelect.ShowDialog() == DialogResult.Cancel)
					{
						return false;
					}

					final = frmSelect.SelectedItem;
				}
				
				SkillsSection.FilterOptions skills;
				if (Enum.TryParse(final, out skills))
				{
					_objCharacter.SkillsSection.AddSkills(skills);
					CreateImprovement(skills.ToString(), Improvement.ImprovementSource.Quality, strSourceName,
						Improvement.ImprovementType.SpecialSkills, strUnique);
				}
				else
				{
					Utils.BreakIfDebug();
					Log.Info(new[] {"Failed to parse", "specialskills", bonusNode.OuterXml});
				}
			}

			//nothing went wrong, so return true
			return true;
		}

		/// <summary>
		/// Remove all of the Improvements for an XML Node.
		/// </summary>
		/// <param name="objImprovementSource">Type of object that granted these Improvements.</param>
		/// <param name="strSourceName">Name of the item that granted these Improvements.</param>
		public void RemoveImprovements(Improvement.ImprovementSource objImprovementSource, string strSourceName)
		{
            Log.Enter("RemoveImprovements");
			Log.Info("objImprovementSource = " + objImprovementSource.ToString());
			Log.Info("strSourceName = " + strSourceName);

            // If there is no character object, don't try to remove any Improvements.
            if (_objCharacter == null)
            {
                Log.Exit("RemoveImprovements");
                return;
            }

			// A List of Improvements to hold all of the items that will eventually be deleted.
			List<Improvement> objImprovementList = new List<Improvement>();
			foreach (Improvement objImprovement in _objCharacter.Improvements)
			{
				if (objImprovement.ImproveSource == objImprovementSource && objImprovement.SourceName == strSourceName)
					objImprovementList.Add(objImprovement);
			}

			// Now that we have all of the applicable Improvements, remove them from the character.
			foreach (Improvement objImprovement in objImprovementList)
			{
				// Remove the Improvement.
				_objCharacter.Improvements.Remove(objImprovement);

				if (objImprovement.ImproveType == Improvement.ImprovementType.SkillLevel)
				{
					//TODO: Come back here and figure out wtf this did? Think it removed nested lifemodule skills? //Didn't this handle the collapsing knowledge skills thing?
					//for (int i = _objCharacter.SkillsSection.Skills.Count - 1; i >= 0; i--)
					//{
					//	//wrote as foreach first, modify collection, not want rename
					//	Skill skill = _objCharacter.SkillsSection.Skills[i];
					//	for (int j = skill.Fold.Count - 1; j >= 0; j--)
					//	{
					//		Skill fold = skill.Fold[i];
					//		if (fold.Id.ToString() == objImprovement.ImprovedName)
					//		{
					//			skill.Free(fold);
					//			_objCharacter.SkillsSection.Skills.Remove(fold);
					//		}
					//	}

					//	if (skill.Id.ToString() == objImprovement.ImprovedName)
					//	{
					//		while(skill.Fold.Count > 0) skill.Free(skill.Fold[0]);
					//		//empty list, can't call clear as exposed list is RO

					//		_objCharacter.SkillsSection.Skills.Remove(skill);
					//	}
					//}
				}

				if (objImprovement.ImproveType == Improvement.ImprovementType.SkillsoftAccess)
			    {
					_objCharacter.SkillsSection.KnowledgeSkills.RemoveAll(_objCharacter.SkillsSection.KnowsoftSkills.Contains);
			    }

                if (objImprovement.ImproveType == Improvement.ImprovementType.SkillKnowledgeForced)
                {
	                Guid guid = Guid.Parse(objImprovement.ImprovedName);
	                _objCharacter.SkillsSection.KnowledgeSkills.RemoveAll(skill => skill.Id == guid);
	                _objCharacter.SkillsSection.KnowsoftSkills.RemoveAll(skill => skill.Id == guid);
                }

                // Remove "free" adept powers if any.
                if (objImprovement.ImproveType == Improvement.ImprovementType.AdeptPower)
                {
					// Load the power from XML.
					// objImprovement.Notes = name of the mentor spirit choice. Find the power name from here.
					// TODO: Fix this properly. Generates a null exception if multiple adept powers are added by the improvement, as with the Dragonslayer Mentor Spirit. 
	                try
	                {
		                XmlDocument objXmlMentorDocument = new XmlDocument();
		                objXmlMentorDocument = XmlManager.Instance.Load("mentors.xml");
		                XmlNode objXmlMentorBonus =
			                objXmlMentorDocument.SelectSingleNode("/chummer/mentors/mentor/choices/choice[name = \"" +
			                                                      objImprovement.Notes +
			                                                      "\"]");
		                XmlNodeList objXmlPowerList = objXmlMentorBonus["bonus"].SelectNodes("specificpower");
		                foreach (XmlNode objXmlSpecificPower in objXmlPowerList)
		                {
			                // Get the Power information
			                XmlDocument objXmlDocument = new XmlDocument();
			                objXmlDocument = XmlManager.Instance.Load("powers.xml");

			                string strPowerName = objXmlSpecificPower["name"].InnerText;

			                // Find the power (if it still exists)
			                foreach (Power objPower in _objCharacter.Powers)
			                {
				                if (objPower.Name == strPowerName)
				                {
					                // Disable the free property and remove any free levels.
					                objPower.Free = false;
					                objPower.FreeLevels = 0;
				                }
			                }
		                }
	                }
	                catch
	                {

	                }
                }
                if (objImprovement.ImproveType == Improvement.ImprovementType.Attribute)
                {
                    CharacterAttrib objChangedAttribute = null;
                    switch (objImprovement.ImprovedName)
                    {
                        case "AGI":
                            objChangedAttribute = _objCharacter.AGI;
                            break;
                        case "REA":
                            objChangedAttribute = _objCharacter.REA;
                            break;
                        case "STR":
                            objChangedAttribute = _objCharacter.STR;
                            break;
                        case "CHA":
                            objChangedAttribute = _objCharacter.CHA;
                            break;
                        case "INT":
                            objChangedAttribute = _objCharacter.INT;
                            break;
                        case "LOG":
                            objChangedAttribute = _objCharacter.LOG;
                            break;
                        case "WIL":
                            objChangedAttribute = _objCharacter.WIL;
                            break;
                        case "EDG":
                            objChangedAttribute = _objCharacter.EDG;
                            break;
                        case "MAG":
                            objChangedAttribute = _objCharacter.MAG;
                            break;
                        case "RES":
                            objChangedAttribute = _objCharacter.RES;
                            break;
                        case "DEP":
                            objChangedAttribute = _objCharacter.DEP;
                            break;
                        case "BOD":
                        default:
                            objChangedAttribute = _objCharacter.BOD;
                            break;
                    }
                    if (objImprovement.Minimum > 0)
                    {
                        objChangedAttribute.Value -= objImprovement.Minimum;
                    }
                }
                // Determine if access to any Special Attributes have been lost.
                if (objImprovement.ImproveType == Improvement.ImprovementType.Attribute &&
				    objImprovement.UniqueName == "enableattribute")
				{
					if (objImprovement.ImprovedName == "MAG")
					{
						// See if the character has anything else that is granting them access to MAG.
						bool blnFound = false;
						foreach (Improvement objCharacterImprovement in _objCharacter.Improvements)
						{
							// Skip items from the current Improvement source.
							if (objCharacterImprovement.SourceName != objImprovement.SourceName)
							{
								if (objCharacterImprovement.ImproveType == Improvement.ImprovementType.Attribute &&
								    objCharacterImprovement.UniqueName == "enableattribute" && objCharacterImprovement.ImprovedName == "MAG")
								{
									blnFound = true;
									break;
								}
							}
						}

						if (!blnFound)
							_objCharacter.MAGEnabled = false;
					}
					else if (objImprovement.ImprovedName == "RES")
					{
						// See if the character has anything else that is granting them access to RES.
						bool blnFound = false;
						foreach (Improvement objCharacterImprovement in _objCharacter.Improvements)
						{
							// Skip items from the current Improvement source.
							if (objCharacterImprovement.SourceName != objImprovement.SourceName)
							{
								if (objCharacterImprovement.ImproveType == Improvement.ImprovementType.Attribute &&
								    objCharacterImprovement.UniqueName == "enableattribute" && objCharacterImprovement.ImprovedName == "RES")
								{
									blnFound = true;
									break;
								}
							}
						}

						if (!blnFound)
							_objCharacter.RESEnabled = false;
					}
				}

				// Determine if access to any special tabs have been lost.
				if (objImprovement.ImproveType == Improvement.ImprovementType.SpecialTab && objImprovement.UniqueName == "enabletab")
				{
					bool blnFound = false;
					switch (objImprovement.ImprovedName)
					{
						case "Magician":
							// See if the character has anything else that is granting them access to the Magician tab.
							foreach (Improvement objCharacterImprovement in _objCharacter.Improvements)
							{
								// Skip items from the current Improvement source.
								if (objCharacterImprovement.SourceName != objImprovement.SourceName)
								{
									if (objCharacterImprovement.ImproveType == Improvement.ImprovementType.SpecialTab &&
									    objCharacterImprovement.UniqueName == "enabletab" && objCharacterImprovement.ImprovedName == "Magician")
									{
										blnFound = true;
										break;
									}
								}
							}
							
							if (!blnFound)
								_objCharacter.MagicianEnabled = false;
							break;
						case "Adept":
							// See if the character has anything else that is granting them access to the Adept tab.
							foreach (Improvement objCharacterImprovement in _objCharacter.Improvements)
							{
								// Skip items from the current Improvement source.
								if (objCharacterImprovement.SourceName != objImprovement.SourceName)
								{
									if (objCharacterImprovement.ImproveType == Improvement.ImprovementType.SpecialTab &&
									    objCharacterImprovement.UniqueName == "enabletab" && objCharacterImprovement.ImprovedName == "Adept")
									{
										blnFound = true;
										break;
									}
								}
							}

							if (!blnFound)
								_objCharacter.AdeptEnabled = false;
							break;
						case "Technomancer":
							// See if the character has anything else that is granting them access to the Technomancer tab.
							foreach (Improvement objCharacterImprovement in _objCharacter.Improvements)
							{
								// Skip items from the current Improvement source.
								if (objCharacterImprovement.SourceName != objImprovement.SourceName)
								{
									if (objCharacterImprovement.ImproveType == Improvement.ImprovementType.SpecialTab &&
									    objCharacterImprovement.UniqueName == "enabletab" && objCharacterImprovement.ImprovedName == "Technomancer")
									{
										blnFound = true;
										break;
									}
								}
							}

							if (!blnFound)
								_objCharacter.TechnomancerEnabled = false;
							break;
						case "Critter":
							// See if the character has anything else that is granting them access to the Critter tab.
							foreach (Improvement objCharacterImprovement in _objCharacter.Improvements)
							{
								// Skip items from the current Improvement source.
								if (objCharacterImprovement.SourceName != objImprovement.SourceName)
								{
									if (objCharacterImprovement.ImproveType == Improvement.ImprovementType.SpecialTab &&
									    objCharacterImprovement.UniqueName == "enabletab" && objCharacterImprovement.ImprovedName == "Critter")
									{
										blnFound = true;
										break;
									}
								}
							}

							if (!blnFound)
								_objCharacter.CritterEnabled = false;
							break;
						case "Initiation":
							// See if the character has anything else that is granting them access to the Initiation tab.
							foreach (Improvement objCharacterImprovement in _objCharacter.Improvements)
							{
								// Skip items from the current Improvement source.
								if (objCharacterImprovement.SourceName != objImprovement.SourceName)
								{
									if (objCharacterImprovement.ImproveType == Improvement.ImprovementType.SpecialTab &&
									    objCharacterImprovement.UniqueName == "enabletab" && objCharacterImprovement.ImprovedName == "Initiation")
									{
										blnFound = true;
										break;
									}
								}
							}

							if (!blnFound)
								_objCharacter.InitiationEnabled = false;
							break;
					}
				}

				// Turn of the Black Market flag if it is being removed.
				if (objImprovement.ImproveType == Improvement.ImprovementType.BlackMarketDiscount)
				{
					bool blnFound = false;
					// See if the character has anything else that is granting them access to Black Market.
					foreach (Improvement objCharacterImprovement in _objCharacter.Improvements)
					{
						// Skip items from the current Improvement source.
						if (objCharacterImprovement.SourceName != objImprovement.SourceName)
						{
							if (objCharacterImprovement.ImproveType == Improvement.ImprovementType.BlackMarketDiscount)
							{
								blnFound = true;
								break;
							}
						}
					}

					if (!blnFound)
					{
						_objCharacter.BlackMarketDiscount = false;
						if (!_objCharacter.Created)
						{
							foreach (Vehicle objVehicle in _objCharacter.Vehicles)
							{
								objVehicle.BlackMarketDiscount = false;
								foreach (Weapon objWeapon in objVehicle.Weapons)
								{
									objWeapon.DiscountCost = false;
									foreach (WeaponAccessory objWeaponAccessory in objWeapon.WeaponAccessories)
									{
										objWeaponAccessory.DiscountCost = false;
									}
								}
								foreach (Gear objGear in objVehicle.Gear)
								{
									objGear.DiscountCost = false;
								}
								foreach (VehicleMod objMod in objVehicle.Mods)
								{
									objMod.DiscountCost = false;
								}
							}
							foreach (Weapon objWeapon in _objCharacter.Weapons)
							{
								objWeapon.DiscountCost = false;
								foreach (WeaponAccessory objWeaponAccessory in objWeapon.WeaponAccessories)
								{
									objWeaponAccessory.DiscountCost = false;
								}
							}
							foreach (Gear objGear in _objCharacter.Gear)
							{
								objGear.DiscountCost = false;

								foreach (Gear objChild in objGear.Children)
								{
									objGear.DiscountCost = false;
								}
							}
						}
					}
				}

				// Turn of the Uneducated flag if it is being removed.
				if (objImprovement.ImproveType == Improvement.ImprovementType.Uneducated)
				{
					bool blnFound = false;
					// See if the character has anything else that is granting them access to Uneducated.
					foreach (Improvement objCharacterImprovement in _objCharacter.Improvements)
					{
						// Skip items from the current Improvement source.
						if (objCharacterImprovement.SourceName != objImprovement.SourceName)
						{
							if (objCharacterImprovement.ImproveType == Improvement.ImprovementType.Uneducated)
							{
								blnFound = true;
								break;
							}
						}
					}

					if (!blnFound)
						_objCharacter.SkillsSection.Uneducated = false;
				}

				// Turn off the Uncouth flag if it is being removed.
				if (objImprovement.ImproveType == Improvement.ImprovementType.Uncouth)
				{
					bool blnFound = false;
					// See if the character has anything else that is granting them access to Uncouth.
					foreach (Improvement objCharacterImprovement in _objCharacter.Improvements)
					{
						// Skip items from the current Improvement source.
						if (objCharacterImprovement.SourceName != objImprovement.SourceName)
						{
							if (objCharacterImprovement.ImproveType == Improvement.ImprovementType.Uncouth)
							{
								blnFound = true;
								break;
							}
						}
					}

					if (!blnFound)
						_objCharacter.SkillsSection.Uncouth = false;
                }

                // Turn off the FriendsInHighPlaces flag if it is being removed.
                if (objImprovement.ImproveType == Improvement.ImprovementType.FriendsInHighPlaces)
                {
                    bool blnFound = false;
                    // See if the character has anything else that is granting them access to FriendsInHighPlaces.
                    foreach (Improvement objCharacterImprovement in _objCharacter.Improvements)
                    {
                        // Skip items from the current Improvement source.
                        if (objCharacterImprovement.SourceName != objImprovement.SourceName)
                        {
                            if (objCharacterImprovement.ImproveType == Improvement.ImprovementType.FriendsInHighPlaces)
                            {
                                blnFound = true;
                                break;
                            }
                        }
                    }

                    if (!blnFound)
                        _objCharacter.FriendsInHighPlaces = false;
                }

                // Turn off the SchoolOfHardKnocks flag if it is being removed.
                if (objImprovement.ImproveType == Improvement.ImprovementType.SchoolOfHardKnocks)
                {
                    bool blnFound = false;
                    // See if the character has anything else that is granting them access to SchoolOfHardKnocks.
                    foreach (Improvement objCharacterImprovement in _objCharacter.Improvements)
                    {
                        // Skip items from the current Improvement source.
                        if (objCharacterImprovement.SourceName != objImprovement.SourceName)
                        {
                            if (objCharacterImprovement.ImproveType == Improvement.ImprovementType.SchoolOfHardKnocks)
                            {
                                blnFound = true;
                                break;
                            }
                        }
                    }

                    if (!blnFound)
                        _objCharacter.SkillsSection.SchoolOfHardKnocks = false;
                }

                //Turn off the Ex-Con flag if it is being removed
			    if (objImprovement.ImproveType == Improvement.ImprovementType.ExCon)
			    {
                     bool blnFound = false;
                     // See if the character has anything else that is granting them access to SchoolOfHardKnocks.
                     foreach (Improvement objCharacterImprovement in _objCharacter.Improvements)
                     {
                         // Skip items from the current Improvement source.
                         if (objCharacterImprovement.SourceName != objImprovement.SourceName)
                         {
                             if (objCharacterImprovement.ImproveType == Improvement.ImprovementType.ExCon)
                             {
                                 blnFound = true;
                                 break;
                             }
                         }
                     }
 
                     if (!blnFound)
                         _objCharacter.ExCon = false;
 			    }

                // Turn off the FriendsInHighPlaces flag if it is being removed.
                if (objImprovement.ImproveType == Improvement.ImprovementType.FriendsInHighPlaces)
                {
                    bool blnFound = false;
                    // See if the character has anything else that is granting them access to FriendsInHighPlaces.
                    foreach (Improvement objCharacterImprovement in _objCharacter.Improvements)
                    {
                        // Skip items from the current Improvement source.
                        if (objCharacterImprovement.SourceName != objImprovement.SourceName)
                        {
                            if (objCharacterImprovement.ImproveType == Improvement.ImprovementType.FriendsInHighPlaces)
                            {
                                blnFound = true;
                                break;
                            }
                        }
                    }

                    if (!blnFound)
                        _objCharacter.FriendsInHighPlaces = false;
                }
                // Turn off the JackOfAllTrades flag if it is being removed.
                if (objImprovement.ImproveType == Improvement.ImprovementType.JackOfAllTrades)
                {
                    bool blnFound = false;
                    // See if the character has anything else that is granting them access to JackOfAllTrades.
                    foreach (Improvement objCharacterImprovement in _objCharacter.Improvements)
                    {
                        // Skip items from the current Improvement source.
                        if (objCharacterImprovement.SourceName != objImprovement.SourceName)
                        {
                            if (objCharacterImprovement.ImproveType == Improvement.ImprovementType.JackOfAllTrades)
                            {
                                blnFound = true;
                                break;
                            }
                        }
                    }

                    if (!blnFound)
                        _objCharacter.SkillsSection.JackOfAllTrades = false;
				}
				// Turn off the prototypetranshuman flag if it is being removed.
				if (objImprovement.ImproveType == Improvement.ImprovementType.PrototypeTranshuman)
				{
					bool blnFound = false;
					// See if the character has anything else that is granting them access to prototypetranshuman.
					foreach (Improvement objCharacterImprovement in _objCharacter.Improvements)
					{
						// Skip items from the current Improvement source.
						if (objCharacterImprovement.SourceName != objImprovement.SourceName)
						{
							if (objCharacterImprovement.ImproveType == Improvement.ImprovementType.PrototypeTranshuman)
							{
								blnFound = true;
								break;
							}
						}
					}

					if (!blnFound)
						_objCharacter.PrototypeTranshuman = 0;
				}
				// Turn off the CollegeEducation flag if it is being removed.
				if (objImprovement.ImproveType == Improvement.ImprovementType.CollegeEducation)
                {
                    bool blnFound = false;
                    // See if the character has anything else that is granting them access to CollegeEducation.
                    foreach (Improvement objCharacterImprovement in _objCharacter.Improvements)
                    {
                        // Skip items from the current Improvement source.
                        if (objCharacterImprovement.SourceName != objImprovement.SourceName)
                        {
                            if (objCharacterImprovement.ImproveType == Improvement.ImprovementType.CollegeEducation)
                            {
                                blnFound = true;
                                break;
                            }
                        }
                    }

                    if (!blnFound)
                        _objCharacter.SkillsSection.CollegeEducation = false;
                }
                // Turn off the Erased flag if it is being removed.
                if (objImprovement.ImproveType == Improvement.ImprovementType.Erased)
                {
                    bool blnFound = false;
                    // See if the character has anything else that is granting them access to Erased.
                    foreach (Improvement objCharacterImprovement in _objCharacter.Improvements)
                    {
                        // Skip items from the current Improvement source.
                        if (objCharacterImprovement.SourceName != objImprovement.SourceName)
                        {
                            if (objCharacterImprovement.ImproveType == Improvement.ImprovementType.Erased)
                            {
                                blnFound = true;
                                break;
                            }
                        }
                    }

                    if (!blnFound)
                        _objCharacter.Erased = false;
                }
                // Turn off the BornRich flag if it is being removed.
                if (objImprovement.ImproveType == Improvement.ImprovementType.BornRich)
                {
                    bool blnFound = false;
                    // See if the character has anything else that is granting them access to BornRich.
                    foreach (Improvement objCharacterImprovement in _objCharacter.Improvements)
                    {
                        // Skip items from the current Improvement source.
                        if (objCharacterImprovement.SourceName != objImprovement.SourceName)
                        {
                            if (objCharacterImprovement.ImproveType == Improvement.ImprovementType.BornRich)
                            {
                                blnFound = true;
                                break;
                            }
                        }
                    }

                    if (!blnFound)
                        _objCharacter.BornRich = false;
                }
                // Turn off the Fame flag if it is being removed.
                if (objImprovement.ImproveType == Improvement.ImprovementType.Fame)
                {
                    bool blnFound = false;
                    // See if the character has anything else that is granting them access to Fame.
                    foreach (Improvement objCharacterImprovement in _objCharacter.Improvements)
                    {
                        // Skip items from the current Improvement source.
                        if (objCharacterImprovement.SourceName != objImprovement.SourceName)
                        {
                            if (objCharacterImprovement.ImproveType == Improvement.ImprovementType.Fame)
                            {
                                blnFound = true;
                                break;
                            }
                        }
                    }

                    if (!blnFound)
                        _objCharacter.Fame = false;
                }
                // Turn off the LightningReflexes flag if it is being removed.
                if (objImprovement.ImproveType == Improvement.ImprovementType.LightningReflexes)
                {
                    bool blnFound = false;
                    // See if the character has anything else that is granting them access to LightningReflexes.
                    foreach (Improvement objCharacterImprovement in _objCharacter.Improvements)
                    {
                        // Skip items from the current Improvement source.
                        if (objCharacterImprovement.SourceName != objImprovement.SourceName)
                        {
                            if (objCharacterImprovement.ImproveType == Improvement.ImprovementType.LightningReflexes)
                            {
                                blnFound = true;
                                break;
                            }
                        }
                    }

                    if (!blnFound)
                        _objCharacter.LightningReflexes = false;
                }
                // Turn off the Linguist flag if it is being removed.
                if (objImprovement.ImproveType == Improvement.ImprovementType.Linguist)
                {
                    bool blnFound = false;
                    // See if the character has anything else that is granting them access to Linguist.
                    foreach (Improvement objCharacterImprovement in _objCharacter.Improvements)
                    {
                        // Skip items from the current Improvement source.
                        if (objCharacterImprovement.SourceName != objImprovement.SourceName)
                        {
                            if (objCharacterImprovement.ImproveType == Improvement.ImprovementType.Linguist)
                            {
                                blnFound = true;
                                break;
                            }
                        }
                    }

                    if (!blnFound)
                        _objCharacter.SkillsSection.Linguist = false;
                }
                // Turn off the MadeMan flag if it is being removed.
                if (objImprovement.ImproveType == Improvement.ImprovementType.MadeMan)
                {
                    bool blnFound = false;
                    // See if the character has anything else that is granting them access to MadeMan.
                    foreach (Improvement objCharacterImprovement in _objCharacter.Improvements)
                    {
                        // Skip items from the current Improvement source.
                        if (objCharacterImprovement.SourceName != objImprovement.SourceName)
                        {
                            if (objCharacterImprovement.ImproveType == Improvement.ImprovementType.MadeMan)
                            {
                                blnFound = true;
                                break;
                            }
                        }
                    }

                    if (!blnFound)
                        _objCharacter.MadeMan = false;
                }
                // Turn off the Overclocker flag if it is being removed.
                if (objImprovement.ImproveType == Improvement.ImprovementType.Overclocker)
                {
                    bool blnFound = false;
                    // See if the character has anything else that is granting them access to Overclocker.
                    foreach (Improvement objCharacterImprovement in _objCharacter.Improvements)
                    {
                        // Skip items from the current Improvement source.
                        if (objCharacterImprovement.SourceName != objImprovement.SourceName)
                        {
                            if (objCharacterImprovement.ImproveType == Improvement.ImprovementType.Overclocker)
                            {
                                blnFound = true;
                                break;
                            }
                        }
                    }

                    if (!blnFound)
                        _objCharacter.Overclocker = false;
                }
                // Turn off the RestrictedGear flag if it is being removed.
                if (objImprovement.ImproveType == Improvement.ImprovementType.RestrictedGear)
                {
                    bool blnFound = false;
                    // See if the character has anything else that is granting them access to RestrictedGear.
                    foreach (Improvement objCharacterImprovement in _objCharacter.Improvements)
                    {
                        // Skip items from the current Improvement source.
                        if (objCharacterImprovement.SourceName != objImprovement.SourceName)
                        {
                            if (objCharacterImprovement.ImproveType == Improvement.ImprovementType.RestrictedGear)
                            {
                                blnFound = true;
                                break;
                            }
                        }
                    }

                    if (!blnFound)
                        _objCharacter.RestrictedGear = false;
                }
                // Turn off the TechSchool flag if it is being removed.
                if (objImprovement.ImproveType == Improvement.ImprovementType.TechSchool)
                {
                    bool blnFound = false;
                    // See if the character has anything else that is granting them access to TechSchool.
                    foreach (Improvement objCharacterImprovement in _objCharacter.Improvements)
                    {
                        // Skip items from the current Improvement source.
                        if (objCharacterImprovement.SourceName != objImprovement.SourceName)
                        {
                            if (objCharacterImprovement.ImproveType == Improvement.ImprovementType.TechSchool)
                            {
                                blnFound = true;
                                break;
                            }
                        }
                    }

                    if (!blnFound)
                        _objCharacter.SkillsSection.TechSchool = false;
                }
                // Turn off the TrustFund flag if it is being removed.
                if (objImprovement.ImproveType == Improvement.ImprovementType.TrustFund)
                {
                    bool blnFound = false;
                    // See if the character has anything else that is granting them access to TrustFund.
                    foreach (Improvement objCharacterImprovement in _objCharacter.Improvements)
                    {
                        // Skip items from the current Improvement source.
                        if (objCharacterImprovement.SourceName != objImprovement.SourceName)
                        {
                            if (objCharacterImprovement.ImproveType == Improvement.ImprovementType.TrustFund)
                            {
                                blnFound = true;
                                break;
                            }
                        }
                    }

                    if (!blnFound)
                        _objCharacter.TrustFund = 0;
                }
                // Turn off the ExCon flag if it is being removed.
                if (objImprovement.ImproveType == Improvement.ImprovementType.ExCon)
                {
                    bool blnFound = false;
                    // See if the character has anything else that is granting them access to ExCon.
                    foreach (Improvement objCharacterImprovement in _objCharacter.Improvements)
                    {
                        // Skip items from the current Improvement source.
                        if (objCharacterImprovement.SourceName != objImprovement.SourceName)
                        {
                            if (objCharacterImprovement.ImproveType == Improvement.ImprovementType.ExCon)
                            {
                                blnFound = true;
                                break;
                            }
                        }
                    }

                    if (!blnFound)
                        _objCharacter.ExCon = false;
                }
				// Turn off the BlackMarketDiscount flag if it is being removed.
				if (objImprovement.ImproveType == Improvement.ImprovementType.BlackMarketDiscount)
                {
                    bool blnFound = false;
                    // See if the character has anything else that is granting them access to BlackMarket.
                    foreach (Improvement objCharacterImprovement in _objCharacter.Improvements)
                    {
                        // Skip items from the current Improvement source.
                        if (objCharacterImprovement.SourceName != objImprovement.SourceName)
                        {
                            if (objCharacterImprovement.ImproveType == Improvement.ImprovementType.BlackMarketDiscount)
                            {
                                blnFound = true;
                                break;
                            }
                        }
                    }

                    if (!blnFound)
	                {
		                _objCharacter.BlackMarketDiscount = false;
	                }
                }
                // If the last instance of Adapsin is being removed, convert all Adapsin Cyberware Grades to their non-Adapsin version.
                if (objImprovement.ImproveType == Improvement.ImprovementType.Adapsin)
				{
					if (!_objCharacter.AdapsinEnabled)
					{
						foreach (Cyberware objCyberware in _objCharacter.Cyberware)
						{
							if (objCyberware.Grade.Adapsin)
							{
								// Determine which GradeList to use for the Cyberware.
								GradeList objGradeList;
								if (objCyberware.SourceType == Improvement.ImprovementSource.Bioware)
									objGradeList = GlobalOptions.BiowareGrades;
								else
									objGradeList = GlobalOptions.CyberwareGrades;

								objCyberware.Grade = objGradeList.GetGrade(objCyberware.Grade.Name.Replace("(Adapsin)", string.Empty).Trim());
							}
						}
					}
				}

                // Remove MadeMan tag from a contact
				if (objImprovement.ImproveType == Improvement.ImprovementType.ContactMadeMan)
			    {
			        Contact contact = (from c in _objCharacter.Contacts
			            where c.GUID == objImprovement.ImprovedName
			            select c).First();

			        contact.MadeMan = false;
				}

				if (objImprovement.ImproveType == Improvement.ImprovementType.AddContact)
				{
					Contact contact = (from c in _objCharacter.Contacts
									   where c.GUID == objImprovement.ImprovedName
									   select c).First();

					_objCharacter.Contacts.Remove(contact);
				}

				// Decrease the character's Initiation Grade.
				if (objImprovement.ImproveType == Improvement.ImprovementType.Initiation)
					_objCharacter.InitiateGrade -= objImprovement.Value;

				// Decrease the character's Submersion Grade.
				if (objImprovement.ImproveType == Improvement.ImprovementType.Submersion)
					_objCharacter.SubmersionGrade -= objImprovement.Value;
				
				//Remove special (magical/resonance) skills
				if (objImprovement.ImproveType == Improvement.ImprovementType.SpecialSkills)
				{
					_objCharacter.SkillsSection.RemoveSkills((SkillsSection.FilterOptions)Enum.Parse(typeof(SkillsSection.FilterOptions), objImprovement.ImprovedName));
				}
			}


			_objCharacter.ImprovementHook(objImprovementList, this);

			Log.Exit("RemoveImprovements");
        }

		/// <summary>
		/// Create a new Improvement and add it to the Character.
		/// </summary>
		/// <param name="strImprovedName">Speicific name of the Improved object - typically the name of an CharacterAttribute being improved.</param>
		/// <param name="objImprovementSource">Type of object that grants this Improvement.</param>
		/// <param name="strSourceName">Name of the item that grants this Improvement.</param>
		/// <param name="objImprovementType">Type of object the Improvement applies to.</param>
		/// <param name="strUnique">Name of the pool this Improvement should be added to - only the single higest value in the pool will be applied to the character.</param>
		/// <param name="intValue">Set a Value for the Improvement.</param>
		/// <param name="intRating">Set a Rating for the Improvement - typically used for Adept Powers.</param>
		/// <param name="intMinimum">Improve the Minimum for an CharacterAttribute by the given amount.</param>
		/// <param name="intMaximum">Improve the Maximum for an CharacterAttribute by the given amount.</param>
		/// <param name="intAugmented">Improve the Augmented value for an CharacterAttribute by the given amount.</param>
		/// <param name="intAugmentedMaximum">Improve the Augmented Maximum value for an CharacterAttribute by the given amount.</param>
		/// <param name="strExclude">A list of child items that should not receive the Improvement's benefit (typically for Skill Groups).</param>
		/// <param name="blnAddToRating">Whether or not the bonus applies to a Skill's Rating instead of the dice pool in general.</param>
		public void CreateImprovement(string strImprovedName, Improvement.ImprovementSource objImprovementSource,
			string strSourceName, Improvement.ImprovementType objImprovementType, string strUnique,
			int intValue = 0, int intRating = 1, int intMinimum = 0, int intMaximum = 0, int intAugmented = 0,
			int intAugmentedMaximum = 0, string strExclude = "", bool blnAddToRating = false)
		{
            Log.Enter("CreateImprovement");
			Log.Info(
				"strImprovedName = " + strImprovedName);
			Log.Info(
				"objImprovementSource = " + objImprovementSource.ToString());
			Log.Info(
				"strSourceName = " + strSourceName);
			Log.Info(
				"objImprovementType = " + objImprovementType.ToString());
            Log.Info( "strUnique = " + strUnique);
			Log.Info(
				"intValue = " + intValue.ToString());
			Log.Info(
				"intRating = " + intRating.ToString());
			Log.Info(
				"intMinimum = " + intMinimum.ToString());
			Log.Info(
				"intMaximum = " + intMaximum.ToString());
			Log.Info(
				"intAugmented = " + intAugmented.ToString());
			Log.Info(
				"intAugmentedMaximum = " + intAugmentedMaximum.ToString());
            Log.Info( "strExclude = " + strExclude);
			Log.Info(
				"blnAddToRating = " + blnAddToRating.ToString());
            
            // Record the improvement.
			Improvement objImprovement = new Improvement();
			objImprovement.ImprovedName = strImprovedName;
			objImprovement.ImproveSource = objImprovementSource;
			objImprovement.SourceName = strSourceName;
			objImprovement.ImproveType = objImprovementType;
			objImprovement.UniqueName = strUnique;
			objImprovement.Value = intValue;
			objImprovement.Rating = intRating;
			objImprovement.Minimum = intMinimum;
			objImprovement.Maximum = intMaximum;
			objImprovement.Augmented = intAugmented;
			objImprovement.AugmentedMaximum = intAugmentedMaximum;
			objImprovement.Exclude = strExclude;
			objImprovement.AddToRating = blnAddToRating;

			// Do not attempt to add the Improvements if the Character is null (as a result of Cyberware being added to a VehicleMod).
			if (_objCharacter != null)
			{
				// Add the Improvement to the list.
				_objCharacter.Improvements.Add(objImprovement);

				// Add the Improvement to the Transaction List.
				_lstTransaction.Add(objImprovement);
			}

            Log.Exit("CreateImprovement");
        }

		/// <summary>
		/// Clear all of the Improvements from the Transaction List.
		/// </summary>
		public void Commit()
		{
            Log.Enter("Commit");
            // Clear all of the Improvements from the Transaction List.

			_objCharacter.ImprovementHook(_lstTransaction, this);
			_lstTransaction.Clear();
            Log.Exit("Commit");
        }

		/// <summary>
		/// Rollback all of the Improvements from the Transaction List.
		/// </summary>
		private void Rollback()
		{
            Log.Enter("Rollback");
            // Remove all of the Improvements that were added.
			foreach (Improvement objImprovement in _lstTransaction)
				RemoveImprovements(objImprovement.ImproveSource, objImprovement.SourceName);

			_lstTransaction.Clear();
            Log.Exit("Rollback");
        }

		#endregion
	}

	public static class ImprovementExtensions
	{
		public static bool HaveSkillPoints(this CharacterBuildMethod method)
		{
			return method == CharacterBuildMethod.Priority || method == CharacterBuildMethod.SumtoTen;
		}
	}
}