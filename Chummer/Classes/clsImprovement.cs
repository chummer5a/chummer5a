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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend;
using Chummer.Backend.Equipment;
using Chummer.Classes;
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
			MovementMultiplier,
			DataStore
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
			foreach (Improvement objImprovement in _objCharacter.Improvements.Where(objImprovement => objImprovement.Enabled && !objImprovement.Custom))
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
					if (strValues[0, 0] == "precedence1" || strValues[0, 0] == "precedence-1")
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
				intValue = intHighest;
			}

			// Factor in Custom Improvements.
			lstUniqueName = new List<string>();
			lstUniquePair = new List<string[,]>();
			int intCustomValue = 0;
			foreach (Improvement objImprovement in _objCharacter.Improvements.Where(objImprovement => objImprovement.Enabled && objImprovement.Custom))
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
                }
				if (NodeExists(nodBonus, "selecttext"))
				{
					Log.Info("selecttext");

					if (_objCharacter != null)
					{
						if (_strForcedValue != "")
						{
							LimitSelection = _strForcedValue;
						}
						else if (_objCharacter.Pushtext.Count != 0)
						{
							LimitSelection = _objCharacter.Pushtext.Pop();
						}
					}

					Log.Info("_strForcedValue = " + SelectedValue);
					Log.Info("_strLimitSelection = " + LimitSelection);

					// Display the Select Text window and record the value that was entered.
					frmSelectText frmPickText = new frmSelectText();
					frmPickText.Description = LanguageManager.Instance.GetString("String_Improvement_SelectText")
						.Replace("{0}", strFriendlyName);

					if (LimitSelection != "")
					{
						frmPickText.SelectedValue = LimitSelection;
						frmPickText.Opacity = 0;
					}

					frmPickText.ShowDialog();

					// Make sure the dialogue window was not canceled.
					if (frmPickText.DialogResult == DialogResult.Cancel)
					{

						Rollback();
						ForcedValue = "";
						LimitSelection = "";
						Log.Exit("CreateImprovements");
						throw new AbortedException();
					}

					_strSelectedValue = frmPickText.SelectedValue;
					if (blnConcatSelectedValue)
						strSourceName += " (" + SelectedValue + ")";
					Log.Info("_strSelectedValue = " + SelectedValue);
					Log.Info("strSourceName = " + strSourceName);

					// Create the Improvement.
					Log.Info("Calling CreateImprovement");

					CreateImprovement(frmPickText.SelectedValue, objImprovementSource, strSourceName,
						Improvement.ImprovementType.Text,
						strUnique);
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
			try
			{
				//As this became a really big nest of **** that it searched past, several places having equal paths just adding a different improvement, a more flexible method was chosen.
				//So far it is just a slower Dictionar<string, Action> but should (in theory...) be able to leverage this in the future to do it smarter with methods that are the same but
				//getting a different parameter injected

				AddImprovementCollection container = new AddImprovementCollection(_objCharacter, this, objImprovementSource,
					strSourceName, strUnique, _strForcedValue, _strLimitSelection, SelectedValue, blnConcatSelectedValue,
					strFriendlyName, intRating, ValueToInt, Rollback);

				MethodInfo info;
				if (AddMethods.Value.TryGetValue(bonusNode.Name.ToUpperInvariant(), out info))
				{
					info.Invoke(container, new object[] {bonusNode});

					strSourceName = container.SourceName;
					_strForcedValue = container.ForcedValue;
					_strLimitSelection = container.LimitSelection;
					_strSelectedValue = container.SelectedValue;
				}
				else
				{
					Utils.BreakIfDebug();
					Log.Warning(new object[] {"Tried to get unknown bonus", bonusNode.OuterXml, string.Join(", ", AddMethods.Value.Keys)});
				}
			}
			catch (TargetInvocationException ex) when (ex.InnerException.GetType() == typeof(AbortedException))
			{
				Rollback();
				return false;
			
			}
			return true;
		}

		//this should probably be somewhere else...
		private static readonly Lazy<Dictionary<string, MethodInfo>> AddMethods = new Lazy<Dictionary<string, MethodInfo>>(() =>
		{
			MethodInfo[] allMethods = typeof(AddImprovementCollection).GetMethods();

			return allMethods.ToDictionary(x => x.Name.ToUpperInvariant());
		});
		

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
			List<Improvement> objImprovementList = _objCharacter.Improvements.Where(objImprovement => objImprovement.ImproveSource == objImprovementSource && objImprovement.SourceName == strSourceName).ToList();

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

				//Remove qualities that were granted by the Improvement.
				if (objImprovement.ImproveType == Improvement.ImprovementType.SpecificQuality)
				{
					foreach (Quality objQuality in _objCharacter.Qualities.Where(objQuality => objImprovement.ImprovedName == objQuality.InternalId))
					{
						_objCharacter.Qualities.Remove(objQuality);
						break;
					}
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
		/// <summary>
		/// Are Skill Points enabled for the character?
		/// </summary>
		/// <param name="method"></param>
		/// <returns></returns>
		public static bool HaveSkillPoints(this CharacterBuildMethod method)
		{
			return method == CharacterBuildMethod.Priority || method == CharacterBuildMethod.SumtoTen;
		}
	}
}