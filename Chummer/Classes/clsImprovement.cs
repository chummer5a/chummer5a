using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace Chummer
{
    public class Improvement
    {
        public enum ImprovementType
        {
            Skill = 0,
            Attribute = 1,
            Text = 2,
			Armor = 3,
			Reach = 5,
			Nuyen = 6,
			Essence = 7,
			Reaction = 8,
			PhysicalCM = 9,
			StunCM = 10,
			UnarmedDV = 11,
			SkillGroup = 12,
			SkillCategory = 13,
			SkillAttribute = 14,
			InitiativePass = 15,
			MatrixInitiative = 16,
			MatrixInitiativePass = 17,
			LifestyleCost = 18,
			CMThreshold = 19,
			EnhancedArticulation = 20,
			WeaponCategoryDV = 21,
			CyberwareEssCost = 22,
			SpecialTab = 23,
			Initiative = 24,
			Uneducated = 25,
			LivingPersonaResponse = 26,
			LivingPersonaSignal = 27,
			LivingPersonaFirewall = 28,
			LivingPersonaSystem = 29,
			LivingPersonaBiofeedback = 30,
			Smartlink = 31,
			BiowareEssCost = 32,
			GenetechCostMultiplier = 33,
			BasicBiowareEssCost = 34,
			TransgenicsBiowareCost = 35,
			SoftWeave = 36,
			SensitiveSystem = 37,
			ConditionMonitor = 38,
			UnarmedDVPhysical = 39,
			MovementPercent = 40,
			Adapsin = 41,
			FreePositiveQualities = 42,
			FreeNegativeQualities = 43,
			NuyenMaxBP = 44,
			CMOverflow = 45,
			FreeSpiritPowerPoints = 46,
			AdeptPowerPoints = 47,
			ArmorEncumbrancePenalty = 48,
			Uncouth = 49,
			Initiation = 50,
			Submersion = 51,
			Infirm = 52,
			Skillwire = 53,
			DamageResistance = 54,
			RestrictedItemCount = 55,
			AdeptLinguistics = 56,
			SwimPercent = 57,
			FlyPercent = 58,
			FlySpeed = 59,
			JudgeIntentions = 60,
			LiftAndCarry = 61,
			Memory = 62,
			Concealability = 63,
			SwapSkillAttribute = 64,
			DrainResistance = 65,
			FadingResistance = 66,
			MatrixInitiativePassAdd = 67,
			InitiativePassAdd = 68,
			Composure = 69,
			UnarmedAP = 70,
			CMThresholdOffset = 71,
			Restricted = 72,
			Notoriety = 73,
			SpellCategory = 74,
			ThrowRange = 75,
			SkillsoftAccess = 76,
			AddSprite = 77,
			BlackMarketDiscount = 78,
			SelectWeapon = 79,
			ComplexFormLimit = 80,
			SpellLimit = 81,
			QuickeningMetamagic = 82,
			BasicLifestyleCost = 83,
			ThrowSTR = 84,
			IgnoreCMPenaltyStun = 85,
			IgnoreCMPenaltyPhysical = 86,
			CyborgEssence = 87,
			EssenceMax = 88,
            AdeptPower = 89,
            SpecificQuality = 90,
            MartialArt = 91,
            LimitModifier = 92,
            PhysicalLimit = 93,
            MentalLimit = 94,
            SocialLimit = 95,
            SchoolOfHardKnocks = 96,
            FriendsInHighPlaces = 97,
            JackOfAllTrades = 98,
            CollegeEducation = 99,
            Erased = 100,
            BornRich = 101,
            Fame = 102,
            LightningReflexes = 103,
            Linguist = 104,
            MadeMan = 105,
            Overclocker = 106,
            RestrictedGear = 107,
            TechSchool = 108,
            TrustFund = 109,
            ExCon = 110,
            BlackMarket = 111,
            ContactMadeMan = 112,
			SelectArmor = 113,
			Attributelevel = 114,
			SkillLevel = 115,
			SkillGroupLevel = 116,
			AddContact,
			Seeker,
		}

        public enum ImprovementSource
        {
            Quality = 0,
            Power = 1,
			Metatype = 2,
			Cyberware = 3,
			Metavariant = 4,
			Bioware = 5,
			Nanotech = 6,
			Genetech = 7,
			ArmorEncumbrance = 8,
			Gear = 9,
			Spell = 10,
			MartialArtAdvantage = 11,
			Initiation = 12,
			Submersion = 13,
			Metamagic = 14,
			Echo = 15,
			Armor = 16, 
			ArmorMod = 17,
			EssenceLoss = 18,
			ConditionMonitor = 19,
			CritterPower = 20,
			ComplexForm = 21,
			EdgeUse = 22,
			MutantCritter = 23,
			Cyberzombie = 24,
			StackedFocus = 25,
			AttributeLoss = 26,
            Art = 27,
            Enhancement = 28,
			Custom = 999,
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
            objFunctions.LogWrite(CommonFunctions.LogType.Entering, "Chummer.Improvement", "Save");

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

            objFunctions.LogWrite(CommonFunctions.LogType.Exiting, "Chummer.Improvement", "Save");
        }

		/// <summary>
		/// Load the Attribute from the XmlNode.
		/// </summary>
		/// <param name="objNode">XmlNode to load.</param>
		public void Load(XmlNode objNode)
		{
            objFunctions.LogWrite(CommonFunctions.LogType.Entering, "Chummer.Improvement", "Load");
            
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

            objFunctions.LogWrite(CommonFunctions.LogType.Exiting, "Chummer.Improvement", "Load");
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
        /// Name of the Skill or Attribute that the Improvement is improving.
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
		/// Attribute selection for Qualities like Metagenetic Improvement.
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
            objFunctions.LogWrite(CommonFunctions.LogType.Entering, "Chummer.ImprovementManager", "ValueOf");
			objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
				"objImprovementType = " + objImprovementType.ToString());
			objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
				"blnAddToRating = " + blnAddToRating.ToString());
			objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
				"strImprovedName = " + ("" + strImprovedName).ToString());

            if (_objCharacter == null)
            {
                objFunctions.LogWrite(CommonFunctions.LogType.Exiting, "Chummer.ImprovementManager", "ValueOf");
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

            objFunctions.LogWrite(CommonFunctions.LogType.Exiting, "Chummer.ImprovementManager", "ValueOf");

			return intValue + intCustomValue;
		}

		/// <summary>
		/// Convert a string to an integer, converting "Rating" to a number where appropriate.
		/// </summary>
		/// <param name="strValue">String value to parse.</param>
		/// <param name="intRating">Integer value to replace "Rating" with.</param>
		private int ValueToInt(string strValue, int intRating = 1)
		{
            objFunctions.LogWrite(CommonFunctions.LogType.Entering, "Chummer.ImprovementManager", "ValueToInt");
            objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "strValue = " + strValue);
			objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
				"intRating = " + intRating.ToString());
            
			if (strValue.Contains("Rating") || strValue.Contains("BOD") || strValue.Contains("AGI") || strValue.Contains("REA") ||
			    strValue.Contains("STR") || strValue.Contains("CHA") || strValue.Contains("INT") || strValue.Contains("LOG") ||
			    strValue.Contains("WIL") || strValue.Contains("EDG") || strValue.Contains("MAG") || strValue.Contains("RES"))
			{
				// If the value contain an Attribute name, replace it with the character's Attribute.
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
                objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "strValue = " + strValue);
                objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "strReturn = " + strReturn);
                XPathExpression xprValue = nav.Compile(strReturn);

				// Treat this as a decimal value so any fractions can be rounded down. This is currently only used by the Boosted Reflexes Cyberware from SR2050.
				decimal decValue = Convert.ToDecimal(nav.Evaluate(xprValue).ToString(), GlobalOptions.Instance.CultureInfo);
				decValue = Math.Floor(decValue);
				int intValue = Convert.ToInt32(decValue);

                objFunctions.LogWrite(CommonFunctions.LogType.Exiting, "Chummer.ImprovementManager", "ValueToInt");
				return Convert.ToInt32(intValue);
			}
			else
			{
                objFunctions.LogWrite(CommonFunctions.LogType.Exiting, "Chummer.ImprovementManager", "ValueToInt");
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
            objFunctions.LogWrite(CommonFunctions.LogType.Entering, "Chummer.ImprovementManager", "NodeExists");
			objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
				"objXmlNode = " + objXmlNode.OuterXml.ToString());
            objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "strName = " + strName);

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
            objFunctions.LogWrite(CommonFunctions.LogType.Entering, "Chummer.ImprovementManager", "CreateImprovements");
			objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
				"objImprovementSource = " + objImprovementSource.ToString());
			objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
				"strSourceName = " + strSourceName);
			objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
				"nodBonus = " + nodBonus.OuterXml.ToString());
			objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
				"blnConcatSelectedValue = " + blnConcatSelectedValue.ToString());
			objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
				"intRating = " + intRating.ToString());
			objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
				"strFriendlyName = " + strFriendlyName);
			objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
				"intRating = " + intRating.ToString());

            bool blnSuccess = true;

            /*try
            {*/
                if (nodBonus == null)
                {
                    _strForcedValue = "";
                    _strLimitSelection = "";
                    objFunctions.LogWrite(CommonFunctions.LogType.Exiting, "Chummer.ImprovementManager", "CreateImprovements");
                    return true;
                }

                string strUnique = "";
                if (nodBonus.Attributes["unique"] != null)
                    strUnique = nodBonus.Attributes["unique"].InnerText;

                _strSelectedValue = "";

				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"_strForcedValue = " + _strForcedValue);
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"_strLimitSelection = " + _strLimitSelection);

                // If no friendly name was provided, use the one from SourceName.
                if (strFriendlyName == "")
                    strFriendlyName = strSourceName;

                if (nodBonus.HasChildNodes)
                {
                    objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Has Child Nodes");

                    // Select Text (custom entry for things like Allergy).
                    if (NodeExists(nodBonus, "selecttext"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "selecttext");

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

						objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
							"_strForcedValue = " + _strSelectedValue);
						objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
							"_strLimitSelection = " + _strLimitSelection);

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
                            objFunctions.LogWrite(CommonFunctions.LogType.Exiting, "Chummer.ImprovementManager", "CreateImprovements");
                            return false;
                        }

                        _strSelectedValue = frmPickText.SelectedValue;
                        if (blnConcatSelectedValue)
                            strSourceName += " (" + _strSelectedValue + ")";
						objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
							"_strSelectedValue = " + _strSelectedValue);
						objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
							"strSourceName = " + strSourceName);

                        // Create the Improvement.
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
						CreateImprovement(frmPickText.SelectedValue, objImprovementSource, strSourceName, Improvement.ImprovementType.Text,
							strUnique);
                    }
                }

                // If there is no character object, don't attempt to add any Improvements.
                if (_objCharacter == null)
                {
                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_objCharacter = Null");
                    objFunctions.LogWrite(CommonFunctions.LogType.Exiting, "Chummer.ImprovementManager", "CreateImprovements");
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
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling Commit");
				Commit();
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Returned from Commit");
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
			objFunctions.LogWrite(CommonFunctions.LogType.Exiting, "Chummer.ImprovementManager", "CreateImprovements");
			return blnSuccess;

		}

		private bool ProcessBonus(Improvement.ImprovementSource objImprovementSource, ref string strSourceName,
			bool blnConcatSelectedValue,
			int intRating, string strFriendlyName, XmlNode bonusNode, string strUnique)
		{
			objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Has Child Nodes");
			// Add an Attribute.
			if (bonusNode.LocalName == ("addattribute"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "addattribute");
				if (bonusNode["name"].InnerText == "MAG")
				{
					_objCharacter.MAGEnabled = true;
					objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager",
						"Calling CreateImprovement for MAG");
					CreateImprovement("MAG", objImprovementSource, strSourceName, Improvement.ImprovementType.Attribute,
						"enableattribute", 0, 0);
				}
				else if (bonusNode["name"].InnerText == "RES")
				{
					_objCharacter.RESEnabled = true;
					objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager",
						"Calling CreateImprovement for RES");
					CreateImprovement("RES", objImprovementSource, strSourceName, Improvement.ImprovementType.Attribute,
						"enableattribute", 0, 0);
				}
			}

			// Enable a special tab.
			if (bonusNode.LocalName == ("enabletab"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "enabletab");
				foreach (XmlNode objXmlEnable in bonusNode.ChildNodes)
				{
					switch (objXmlEnable.InnerText)
					{
						case "magician":
							_objCharacter.MagicianEnabled = true;
							objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "magician");
							CreateImprovement("Magician", objImprovementSource, strSourceName, Improvement.ImprovementType.SpecialTab,
								"enabletab", 0, 0);
							break;
						case "adept":
							_objCharacter.AdeptEnabled = true;
							objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "adept");
							CreateImprovement("Adept", objImprovementSource, strSourceName, Improvement.ImprovementType.SpecialTab,
								"enabletab",
								0, 0);
							break;
						case "technomancer":
							_objCharacter.TechnomancerEnabled = true;
							objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "technomancer");
							CreateImprovement("Technomancer", objImprovementSource, strSourceName, Improvement.ImprovementType.SpecialTab,
								"enabletab", 0, 0);
							break;
						case "critter":
							_objCharacter.CritterEnabled = true;
							objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "critter");
							CreateImprovement("Critter", objImprovementSource, strSourceName, Improvement.ImprovementType.SpecialTab,
								"enabletab", 0, 0);
							break;
						case "initiation":
							_objCharacter.InitiationEnabled = true;
							objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "initiation");
							CreateImprovement("Initiation", objImprovementSource, strSourceName, Improvement.ImprovementType.SpecialTab,
								"enabletab", 0, 0);
							break;
					}
				}
			}

			// Select Restricted (select Restricted items for Fake Licenses).
			if (bonusNode.LocalName == ("selectrestricted"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "selectrestricted");
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

				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"_strSelectedValue = " + _strSelectedValue);
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"strSourceName = " + strSourceName);

				// Create the Improvement.
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
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
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "selectrestricted");
				if (_strForcedValue == "+2 to a Combat Skill")
					_strForcedValue = "";

				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"_strSelectedValue = " + _strSelectedValue);
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"_strForcedValue = " + _strForcedValue);

				// Display the Select Skill window and record which Skill was selected.
				frmSelectSkill frmPickSkill = new frmSelectSkill(_objCharacter);
				if (strFriendlyName != "")
					frmPickSkill.Description = LanguageManager.Instance.GetString("String_Improvement_SelectSkillNamed")
						.Replace("{0}", strFriendlyName);
				else
					frmPickSkill.Description = LanguageManager.Instance.GetString("String_Improvement_SelectSkill");

				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"selectskill = " + bonusNode.OuterXml.ToString());
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

				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"_strSelectedValue = " + _strSelectedValue);
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"strSourceName = " + strSourceName);

				// Find the selected Skill.
				foreach (Skill objSkill in _objCharacter.Skills)
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
								objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
								CreateImprovement(objSkill.Name + " (" + objSkill.Specialization + ")", objImprovementSource, strSourceName,
									Improvement.ImprovementType.Skill, strUnique, ValueToInt(bonusNode["val"].InnerText, intRating), 1,
									0, 0, 0, 0, "", blnAddToRating);
							}

							if (bonusNode.InnerXml.Contains("max"))
							{
								objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
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
								objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
								CreateImprovement(objSkill.Name, objImprovementSource, strSourceName, Improvement.ImprovementType.Skill,
									strUnique,
									ValueToInt(bonusNode["val"].InnerText, intRating), 1, 0, 0, 0, 0, "", blnAddToRating);
							}

							if (bonusNode.InnerXml.Contains("max"))
							{
								objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
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
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "selectskillgroup");
				string strExclude = "";
				if (bonusNode.Attributes["excludecategory"] != null)
					strExclude = bonusNode.Attributes["excludecategory"].InnerText;

				frmSelectSkillGroup frmPickSkillGroup = new frmSelectSkillGroup();
				if (strFriendlyName != "")
					frmPickSkillGroup.Description =
						LanguageManager.Instance.GetString("String_Improvement_SelectSkillGroupName").Replace("{0}", strFriendlyName);
				else
					frmPickSkillGroup.Description = LanguageManager.Instance.GetString("String_Improvement_SelectSkillGroup");

				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"_strForcedValue = " + _strForcedValue);
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"_strLimitSelection = " + _strLimitSelection);

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

				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"_strSelectedValue = " + _strSelectedValue);
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"strSourceName = " + strSourceName);

				if (bonusNode.SelectSingleNode("bonus") != null)
				{
					objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
					CreateImprovement(_strSelectedValue, objImprovementSource, strSourceName, Improvement.ImprovementType.SkillGroup,
						strUnique, ValueToInt(bonusNode["bonus"].InnerText, intRating), 1, 0, 0, 0, 0, strExclude,
						blnAddToRating);
				}
				else
				{
					objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
					CreateImprovement(_strSelectedValue, objImprovementSource, strSourceName, Improvement.ImprovementType.SkillGroup,
						strUnique, ValueToInt(bonusNode["max"].InnerText, intRating), 0, 0, 1, 0, 0, strExclude,
						blnAddToRating);
				}
			}

			if (bonusNode.LocalName == ("selectattributes"))
			{
				foreach (XmlNode objXmlAttribute in bonusNode.SelectNodes("selectattribute"))
				{
					objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "selectattribute");
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

					objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
						"selectattribute = " + bonusNode.OuterXml.ToString());

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

					objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
						"_strForcedValue = " + _strForcedValue);
					objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
						"_strLimitSelection = " + _strLimitSelection);

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

					objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
						"_strSelectedValue = " + _strSelectedValue);
					objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
						"strSourceName = " + strSourceName);

					// Record the improvement.
					int intMin = 0;
					int intAug = 0;
					int intMax = 0;
					int intAugMax = 0;

					// Extract the modifiers.
					if (objXmlAttribute.InnerXml.Contains("min"))
						intMin = ValueToInt(objXmlAttribute["min"].InnerXml, intRating);
					if (objXmlAttribute.InnerXml.Contains("val"))
						intAug = ValueToInt(objXmlAttribute["val"].InnerXml, intRating);
					if (objXmlAttribute.InnerXml.Contains("max"))
						intMax = ValueToInt(objXmlAttribute["max"].InnerXml, intRating);
					if (objXmlAttribute.InnerXml.Contains("aug"))
						intAugMax = ValueToInt(objXmlAttribute["aug"].InnerXml, intRating);

					string strAttribute = frmPickAttribute.SelectedAttribute;

					if (objXmlAttribute["affectbase"] != null)
						strAttribute += "Base";

					objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
					CreateImprovement(strAttribute, objImprovementSource, strSourceName, Improvement.ImprovementType.Attribute,
						strUnique,
						0, 1, intMin, intMax, intAug, intAugMax);
				}
			}

			// Select an Attribute.
			if (bonusNode.LocalName == ("selectattribute"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "selectattribute");
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

				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"selectattribute = " + bonusNode.OuterXml.ToString());

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

				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"_strForcedValue = " + _strForcedValue);
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"_strLimitSelection = " + _strLimitSelection);

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

				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"_strSelectedValue = " + _strSelectedValue);
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"strSourceName = " + strSourceName);

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

				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement(strAttribute, objImprovementSource, strSourceName, Improvement.ImprovementType.Attribute,
					strUnique,
					0, 1, intMin, intMax, intAug, intAugMax);
			}

			// Select a Limit.
			if (bonusNode.LocalName == ("selectlimit"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "selectlimit");
				// Display the Select Limit window and record which Limit was selected.
				frmSelectLimit frmPickLimit = new frmSelectLimit();
				if (strFriendlyName != "")
					frmPickLimit.Description = LanguageManager.Instance.GetString("String_Improvement_SelectLimitNamed")
						.Replace("{0}", strFriendlyName);
				else
					frmPickLimit.Description = LanguageManager.Instance.GetString("String_Improvement_SelectLimit");

				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"selectlimit = " + bonusNode.OuterXml.ToString());

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

				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"_strForcedValue = " + _strForcedValue);
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"_strLimitSelection = " + _strLimitSelection);

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

				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"_strSelectedValue = " + _strSelectedValue);
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"strSourceName = " + strSourceName);

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

				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement(strLimit, objImprovementSource, strSourceName, objType, strFriendlyName, intBonus, 0, intMin,
					intMax,
					intAug, intAugMax);
			}

			// Select an Attribute to use instead of the default on a skill.
			if (bonusNode.LocalName == ("swapskillattribute"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "swapskillattribute");
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

				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"swapskillattribute = " + bonusNode.OuterXml.ToString());

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

				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"_strForcedValue = " + _strForcedValue);
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"_strLimitSelection = " + _strLimitSelection);

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

				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"_strSelectedValue = " + _strSelectedValue);
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"strSourceName = " + strSourceName);

				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement(frmPickAttribute.SelectedAttribute, objImprovementSource, strSourceName,
					Improvement.ImprovementType.SwapSkillAttribute, strUnique);
			}

			// Select a Spell.
			if (bonusNode.LocalName == ("selectspell"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "selectspell");
				// Display the Select Spell window.
				frmSelectSpell frmPickSpell = new frmSelectSpell(_objCharacter);

				if (bonusNode.Attributes["category"] != null)
					frmPickSpell.LimitCategory = bonusNode.Attributes["category"].InnerText;

				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"selectspell = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"_strForcedValue = " + _strForcedValue);
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"_strLimitSelection = " + _strLimitSelection);

				if (_strForcedValue != "")
				{
					frmPickSpell.ForceSpellName = _strForcedValue;
					frmPickSpell.Opacity = 0;
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

				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"_strSelectedValue = " + _strSelectedValue);
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"strSourceName = " + strSourceName);

				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement(frmPickSpell.SelectedSpell, objImprovementSource, strSourceName, Improvement.ImprovementType.Text,
					strUnique);
			}

			// Select a Contact
			if (bonusNode.LocalName == ("selectcontact"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "selectcontact");
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

				int loyality, connection;
				
				bonusNode.TryGetField("loyality", out loyality, 1);
				bonusNode.TryGetField("connection", out connection, 1);
				bool group = bonusNode["group"] != null;
				bool free = bonusNode["free"] != null;

				Contact contact = new Contact(_objCharacter);
				contact.Free = free;
				contact.IsGroup = group;
				contact.Loyalty = loyality;
				contact.Connection = connection;
				contact.ReadOnly = true;
				_objCharacter.Contacts.Add(contact);

				CreateImprovement(contact.GUID, Improvement.ImprovementSource.Quality, strSourceName,
							Improvement.ImprovementType.AddContact, contact.GUID);
			}

			// Affect a Specific Attribute.
			if (bonusNode.LocalName == ("specificattribute"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "specificattribute");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"specificattribute = " + bonusNode.OuterXml.ToString());

				if (bonusNode["name"].InnerText != "ESS")
				{
					// Display the Select Attribute window and record which Attribute was selected.
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

					objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
					CreateImprovement(strAttribute, objImprovementSource, strSourceName, Improvement.ImprovementType.Attribute,
						strUseUnique, 0, 1, intMin, intMax, intAug, intAugMax);
				}
				else
				{
					objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
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

			if (bonusNode.LocalName == "knowledgeskilllevel")
			{
				Log.Info(new object[] {"knowledgeskilllevel", bonusNode.OuterXml});
				int value;
				String group;
				if (bonusNode.TryGetField("group", out group))
				{
					if (!bonusNode.TryGetField("val", out value))
					{
						value = 1;
					}



					Guid id;

					bool blnFound = false;

					if (bonusNode.TryGetField("id", Guid.TryParse, out id, Guid.NewGuid()))
					{
						foreach (Improvement improvement in _objCharacter.Improvements)
						{
							if (improvement.ImprovedName == id.ToString())
							{
								blnFound = true;
							}
						}
					}

					if (!blnFound)
					{
						Skill objNSkill = new Skill(_objCharacter);
						objNSkill.Id = id;
						objNSkill.IdImprovement = true;
						objNSkill.AllowDelete = false;
						objNSkill.KnowledgeSkill = true;
						objNSkill.LockKnowledge = true;
						objNSkill.SkillCategory = group;
						int max;
						bonusNode.TryGetField("max", out max, 9);
						objNSkill.RatingMaximum = max;

						String name;
						if (bonusNode.TryGetField("name", out name))
						{
							objNSkill.Name = name;
						}
						if (bonusNode["options"] != null)
						{
							List<String> Options = new List<String>();
							foreach (XmlNode node in bonusNode["options"].ChildNodes)
							{
								Options.Add(node.InnerText);
							}
							objNSkill.KnowledgeSkillCatagories = Options;
						}

						_objCharacter.Skills.Add(objNSkill);
					}

					CreateImprovement(id.ToString(), objImprovementSource, strSourceName,
						Improvement.ImprovementType.SkillLevel, "", value);


				}
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
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "nuyenmaxbp");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"nuyenmaxbp = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.NuyenMaxBP, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Apply a bonus/penalty to physical limit.
			if (bonusNode.LocalName == ("physicallimit"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "physicallimit");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"physicallimit = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.PhysicalLimit, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Apply a bonus/penalty to mental limit.
			if (bonusNode.LocalName == ("mentallimit"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "mentallimit");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"mentallimit = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.MentalLimit, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Apply a bonus/penalty to social limit.
			if (bonusNode.LocalName == ("sociallimit"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "sociallimit");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"sociallimit = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.SocialLimit, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Change the amount of Nuyen the character has at creation time (this can put the character over the amount they're normally allowed).
			if (bonusNode.LocalName == ("nuyenamt"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "nuyenamt");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"nuyenamt = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Nuyen, strUnique,
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Improve Condition Monitors.
			if (bonusNode.LocalName == ("conditionmonitor"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "conditionmonitor");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"conditionmonitor = " + bonusNode.OuterXml.ToString());
				// Physical Condition.
				if (bonusNode.InnerXml.Contains("physical"))
				{
					objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager",
						"Calling CreateImprovement for Physical");
					CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.PhysicalCM, strUnique,
						ValueToInt(bonusNode["physical"].InnerText, intRating));
				}

				// Stun Condition.
				if (bonusNode.InnerXml.Contains("stun"))
				{
					objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager",
						"Calling CreateImprovement for Stun");
					CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.StunCM, strUnique,
						ValueToInt(bonusNode["stun"].InnerText, intRating));
				}

				// Condition Monitor Threshold.
				if (NodeExists(bonusNode, "threshold"))
				{
					string strUseUnique = strUnique;
					if (bonusNode["threshold"].Attributes["precedence"] != null)
						strUseUnique = "precedence" + bonusNode["threshold"].Attributes["precedence"].InnerText;

					objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager",
						"Calling CreateImprovement for Threshold");
					CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.CMThreshold, strUseUnique,
						ValueToInt(bonusNode["threshold"].InnerText, intRating));
				}

				// Condition Monitor Threshold Offset. (Additioal boxes appear before the FIRST Condition Monitor penalty)
				if (NodeExists(bonusNode, "thresholdoffset"))
				{
					string strUseUnique = strUnique;
					if (bonusNode["thresholdoffset"].Attributes["precedence"] != null)
						strUseUnique = "precedence" + bonusNode["thresholdoffset"].Attributes["precedence"].InnerText;

					objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager",
						"Calling CreateImprovement for Threshold Offset");
					CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.CMThresholdOffset,
						strUseUnique, ValueToInt(bonusNode["thresholdoffset"].InnerText, intRating));
				}

				// Condition Monitor Overflow.
				if (bonusNode.InnerXml.Contains("overflow"))
				{
					objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager",
						"Calling CreateImprovement for Overflow");
					CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.CMOverflow, strUnique,
						ValueToInt(bonusNode["overflow"].InnerText, intRating));
				}
			}

			// Improve Living Personal Attributes.
			if (bonusNode.LocalName == ("livingpersona"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "livingpersona");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"livingpersona = " + bonusNode.OuterXml.ToString());
				// Response.
				if (bonusNode.InnerXml.Contains("response"))
				{
					objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager",
						"Calling CreateImprovement for response");
					CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.LivingPersonaResponse,
						strUnique, ValueToInt(bonusNode["response"].InnerText, intRating));
				}

				// Signal.
				if (bonusNode.InnerXml.Contains("signal"))
				{
					objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager",
						"Calling CreateImprovement for signal");
					CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.LivingPersonaSignal,
						strUnique,
						ValueToInt(bonusNode["signal"].InnerText, intRating));
				}

				// Firewall.
				if (bonusNode.InnerXml.Contains("firewall"))
				{
					objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager",
						"Calling CreateImprovement for firewall");
					CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.LivingPersonaFirewall,
						strUnique, ValueToInt(bonusNode["firewall"].InnerText, intRating));
				}

				// System.
				if (bonusNode.InnerXml.Contains("system"))
				{
					objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager",
						"Calling CreateImprovement for system");
					CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.LivingPersonaSystem,
						strUnique,
						ValueToInt(bonusNode["system"].InnerText, intRating));
				}

				// Biofeedback Filter.
				if (bonusNode.InnerXml.Contains("biofeedback"))
				{
					objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager",
						"Calling CreateImprovement for biofeedback");
					CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.LivingPersonaBiofeedback,
						strUnique, ValueToInt(bonusNode["biofeedback"].InnerText, intRating));
				}
			}

			// The Improvement adjusts a specific Skill.
			if (bonusNode.LocalName == ("specificskill"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "specificskill");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"specificskill = " + bonusNode.OuterXml.ToString());
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
					objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager",
						"Calling CreateImprovement for bonus");
					CreateImprovement(bonusNode["name"].InnerText, objImprovementSource, strSourceName,
						Improvement.ImprovementType.Skill, strUseUnique, ValueToInt(bonusNode["bonus"].InnerXml, intRating), 1, 0, 0, 0,
						0, "", blnAddToRating);
				}
				if (bonusNode["max"] != null)
				{
					objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager",
						"Calling CreateImprovement for max");
					CreateImprovement(bonusNode["name"].InnerText, objImprovementSource, strSourceName,
						Improvement.ImprovementType.Skill, strUseUnique, 0, 1, 0, ValueToInt(bonusNode["max"].InnerText, intRating), 0,
						0,
						"", blnAddToRating);
				}
			}

			// The Improvement adds a martial art
			if (bonusNode.LocalName == ("martialart"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "martialart");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"martialart = " + bonusNode.OuterXml.ToString());
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
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "limitmodifier");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"limitmodifier = " + bonusNode.OuterXml.ToString());
				LimitModifier objLimitMod = new LimitModifier(_objCharacter);
				string strLimit = bonusNode["limit"].InnerText;
				string strBonus = bonusNode["value"].InnerText;
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
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement(strLimit, objImprovementSource, strSourceName, Improvement.ImprovementType.LimitModifier,
					strFriendlyName, intBonus, 0, 0, 0, 0, 0, strCondition);
			}

			// The Improvement adjusts a Skill Category.
			if (bonusNode.LocalName == ("skillcategory"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "skillcategory");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"skillcategory = " + bonusNode.OuterXml.ToString());

				bool blnAddToRating = false;
				if (bonusNode["applytorating"] != null)
				{
					if (bonusNode["applytorating"].InnerText == "yes")
						blnAddToRating = true;
				}
				if (bonusNode.InnerXml.Contains("exclude"))
				{
					objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager",
						"Calling CreateImprovement - exclude");
					CreateImprovement(bonusNode["name"].InnerText, objImprovementSource, strSourceName,
						Improvement.ImprovementType.SkillCategory, strUnique, ValueToInt(bonusNode["bonus"].InnerXml, intRating), 1, 0,
						0,
						0, 0, bonusNode["exclude"].InnerText, blnAddToRating);
				}
				else
				{
					objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
					CreateImprovement(bonusNode["name"].InnerText, objImprovementSource, strSourceName,
						Improvement.ImprovementType.SkillCategory, strUnique, ValueToInt(bonusNode["bonus"].InnerXml, intRating), 1, 0,
						0,
						0, 0, "", blnAddToRating);
				}
			}

			// The Improvement adjusts a Skill Group.
			if (bonusNode.LocalName == ("skillgroup"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "skillgroup");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"skillgroup = " + bonusNode.OuterXml.ToString());

				bool blnAddToRating = false;
				if (bonusNode["applytorating"] != null)
				{
					if (bonusNode["applytorating"].InnerText == "yes")
						blnAddToRating = true;
				}
				if (bonusNode.InnerXml.Contains("exclude"))
				{
					objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager",
						"Calling CreateImprovement - exclude");
					CreateImprovement(bonusNode["name"].InnerText, objImprovementSource, strSourceName,
						Improvement.ImprovementType.SkillGroup, strUnique, ValueToInt(bonusNode["bonus"].InnerXml, intRating), 1, 0, 0, 0,
						0, bonusNode["exclude"].InnerText, blnAddToRating);
				}
				else
				{
					objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
					CreateImprovement(bonusNode["name"].InnerText, objImprovementSource, strSourceName,
						Improvement.ImprovementType.SkillGroup, strUnique, ValueToInt(bonusNode["bonus"].InnerXml, intRating), 1, 0, 0, 0,
						0, "", blnAddToRating);
				}
			}

			// The Improvement adjust Skills with the given Attribute.
			if (bonusNode.LocalName == ("skillattribute"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "skillattribute");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"skillattribute = " + bonusNode.OuterXml.ToString());

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
					objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager",
						"Calling CreateImprovement - exclude");
					CreateImprovement(bonusNode["name"].InnerText, objImprovementSource, strSourceName,
						Improvement.ImprovementType.SkillAttribute, strUseUnique, ValueToInt(bonusNode["bonus"].InnerXml, intRating), 1,
						0, 0, 0, 0, bonusNode["exclude"].InnerText, blnAddToRating);
				}
				else
				{
					objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
					CreateImprovement(bonusNode["name"].InnerText, objImprovementSource, strSourceName,
						Improvement.ImprovementType.SkillAttribute, strUseUnique, ValueToInt(bonusNode["bonus"].InnerXml, intRating), 1,
						0, 0, 0, 0, "", blnAddToRating);
				}
			}

			// The Improvement comes from Enhanced Articulation (improves Physical Active Skills linked to a Physical Attribute).
			if (bonusNode.LocalName == ("skillarticulation"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "skillarticulation");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"skillarticulation = " + bonusNode.OuterXml.ToString());

				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.EnhancedArticulation,
					strUnique,
					ValueToInt(bonusNode["bonus"].InnerText, intRating));
			}

			// Check for Armor modifiers.
			if (bonusNode.LocalName == ("armor"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "armor");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"armor = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Armor, strUnique,
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Reach modifiers.
			if (bonusNode.LocalName == ("reach"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "reach");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"reach = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Reach, strUnique,
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Unarmed Damage Value modifiers.
			if (bonusNode.LocalName == ("unarmeddv"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "unarmeddv");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"unarmeddv = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.UnarmedDV, strUnique,
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Unarmed Damage Value Physical.
			if (bonusNode.LocalName == ("unarmeddvphysical"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "unarmeddvphysical");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"unarmeddvphysical = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.UnarmedDVPhysical, "");
			}

			// Check for Unarmed Armor Penetration.
			if (bonusNode.LocalName == ("unarmedap"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "unarmedap");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"unarmedap = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.UnarmedAP, strUnique,
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Initiative modifiers.
			if (bonusNode.LocalName == ("initiative"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "initiative");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"initiative = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Initiative, strUnique,
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Initiative Pass modifiers. Only the highest one ever applies.
			if (bonusNode.LocalName == ("initiativepass"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "initiativepass");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"initiativepass = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.InitiativePass,
					"initiativepass", ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Initiative Pass modifiers. Only the highest one ever applies.
			if (bonusNode.LocalName == ("initiativepassadd"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "initiativepassadd");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"initiativepassadd = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.InitiativePassAdd, strUnique,
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Matrix Initiative modifiers.
			if (bonusNode.LocalName == ("matrixinitiative"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "matrixinitiative");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"matrixinitiative = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.MatrixInitiative, strUnique,
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Matrix Initiative Pass modifiers.
			if (bonusNode.LocalName == ("matrixinitiativepass"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "matrixinitiativepass");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"matrixinitiativepass = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.MatrixInitiativePass,
					"matrixinitiativepass", ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Matrix Initiative Pass modifiers.
			if (bonusNode.LocalName == ("matrixinitiativepassadd"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "matrixinitiativepassadd");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"matrixinitiativepassadd = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.MatrixInitiativePass,
					strUnique,
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Lifestyle cost modifiers.
			if (bonusNode.LocalName == ("lifestylecost"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "lifestylecost");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"lifestylecost = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.LifestyleCost, strUnique,
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for basic Lifestyle cost modifiers.
			if (bonusNode.LocalName == ("basiclifestylecost"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "basiclifestylecost");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"basiclifestylecost = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.BasicLifestyleCost, strUnique,
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Genetech Cost modifiers.
			if (bonusNode.LocalName == ("genetechcostmultiplier"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "genetechcostmultiplier");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"genetechcostmultiplier = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.GenetechCostMultiplier,
					strUnique, ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Genetech: Transgenics Cost modifiers.
			if (bonusNode.LocalName == ("transgenicsgenetechcost"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "transgenicsgenetechcost");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"transgenicsgenetechcost = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.TransgenicsBiowareCost,
					strUnique, ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Basic Bioware Essence Cost modifiers.
			if (bonusNode.LocalName == ("basicbiowareessmultiplier"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "basicbiowareessmultiplier");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"basicbiowareessmultiplier = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.BasicBiowareEssCost,
					strUnique,
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Bioware Essence Cost modifiers.
			if (bonusNode.LocalName == ("biowareessmultiplier"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "biowareessmultiplier");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"biowareessmultiplier = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.BiowareEssCost, strUnique,
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Cybeware Essence Cost modifiers.
			if (bonusNode.LocalName == ("cyberwareessmultiplier"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "cyberwareessmultiplier");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"cyberwareessmultiplier = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.CyberwareEssCost, strUnique,
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Uneducated modifiers.
			if (bonusNode.LocalName == ("uneducated"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "uneducated");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"uneducated = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Uneducated, strUnique);
				_objCharacter.Uneducated = true;
			}

			// Check for College Education modifiers.
			if (bonusNode.LocalName == ("collegeeducation"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "collegeeducation");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"collegeeducation = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.CollegeEducation, strUnique);
				_objCharacter.CollegeEducation = true;
			}

			// Check for Jack Of All Trades modifiers.
			if (bonusNode.LocalName == ("jackofalltrades"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "jackofalltrades");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"jackofalltrades = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.JackOfAllTrades, strUnique);
				_objCharacter.JackOfAllTrades = true;
			}

			// Check for Uncouth modifiers.
			if (bonusNode.LocalName == ("uncouth"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "uncouth");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"uncouth = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Uncouth, strUnique);
				_objCharacter.Uncouth = true;
			}

			// Check for Friends In High Places modifiers.
			if (bonusNode.LocalName == ("friendsinhighplaces"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "friendsinhighplaces");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"friendsinhighplaces = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.FriendsInHighPlaces,
					strUnique);
				_objCharacter.FriendsInHighPlaces = true;
			}
			// Check for School of Hard Knocks modifiers.
			if (bonusNode.LocalName == ("schoolofhardknocks"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "schoolofhardknocks");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"schoolofhardknocks = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.SchoolOfHardKnocks, strUnique);
				_objCharacter.SchoolOfHardKnocks = true;
			}
			// Check for ExCon modifiers.
			if (bonusNode.LocalName == ("excon"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "ExCon");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"ExCon = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.ExCon, strUnique);
				_objCharacter.ExCon = true;
			}

			// Check for TrustFund modifiers.
			if (bonusNode.LocalName == ("trustfund"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "TrustFund");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"TrustFund = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.TrustFund,
					strUnique,
					ValueToInt(bonusNode.InnerText, intRating));
				_objCharacter.TrustFund = ValueToInt(bonusNode.InnerText, intRating);
			}

			// Check for BlackMarket modifiers.
			if (bonusNode.LocalName == ("blackmarket"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "BlackMarket");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"BlackMarket = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.BlackMarket, strUnique);
				_objCharacter.BlackMarket = true;
			}


			// Check for Tech School modifiers.
			if (bonusNode.LocalName == ("techschool"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "techschool");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"techschool = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.TechSchool, strUnique);
				_objCharacter.TechSchool = true;
			}
			// Check for MadeMan modifiers.
			if (bonusNode.LocalName == ("mademan"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "MadeMan");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"MadeMan = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.MadeMan, strUnique);
				_objCharacter.MadeMan = true;
			}

			// Check for Linguist modifiers.
			if (bonusNode.LocalName == ("linguist"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Linguist");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"Linguist = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Linguist, strUnique);
				_objCharacter.Linguist = true;
			}

			// Check for LightningReflexes modifiers.
			if (bonusNode.LocalName == ("lightningreflexes"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "LightningReflexes");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"LightningReflexes = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.LightningReflexes, strUnique);
				_objCharacter.LightningReflexes = true;
			}

			// Check for Fame modifiers.
			if (bonusNode.LocalName == ("fame"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Fame");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"Fame = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Fame, strUnique);
				_objCharacter.Fame = true;
			}
			// Check for BornRich modifiers.
			if (bonusNode.LocalName == ("bornrich"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "BornRich");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"BornRich = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.BornRich, strUnique);
				_objCharacter.BornRich = true;
			}
			// Check for Erased modifiers.
			if (bonusNode.LocalName == ("erased"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Erased");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"Erased = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Erased, strUnique);
				_objCharacter.Erased = true;
			}
			// Check for Erased modifiers.
			if (bonusNode.LocalName == ("overclocker"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "OverClocker");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"Overclocker = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Overclocker, strUnique);
				_objCharacter.Overclocker = true;
			}

			// Check for Restricted Gear modifiers.
			if (bonusNode.LocalName == ("restrictedgear"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "restrictedgear");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"restrictedgear = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.RestrictedGear, strUnique);
				_objCharacter.RestrictedGear = true;
			}

			// Check for Adept Linguistics.
			if (bonusNode.LocalName == ("adeptlinguistics"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "adeptlinguistics");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"adeptlinguistics = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
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


				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "weaponcategorydv");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"weaponcategorydv = " + bonusNode.OuterXml.ToString());
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

					objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
						"_strForcedValue = " + _strForcedValue);

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

					objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
						"strSelected = " + _strSelectedValue);

					foreach (Power objPower in _objCharacter.Powers)
					{
						if (objPower.InternalId == strSourceName)
						{
							objPower.Extra = _strSelectedValue;
						}
					}

					objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
					CreateImprovement(_strSelectedValue, objImprovementSource, strSourceName,
						Improvement.ImprovementType.WeaponCategoryDV, strUnique, ValueToInt(nodWeapon["bonus"].InnerXml, intRating));
				}
				else
				{
					// Run through each of the Skill Groups since there may be more than one affected.
					foreach (XmlNode objXmlCategory in objXmlCategoryList)
					{
						objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
						CreateImprovement(objXmlCategory["name"].InnerText, objImprovementSource, strSourceName,
							Improvement.ImprovementType.WeaponCategoryDV, strUnique, ValueToInt(objXmlCategory["bonus"].InnerXml, intRating));
					}
				}
			}

			// Check for Mentor Spirit bonuses.
			if (bonusNode.LocalName == ("selectmentorspirit"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "selectmentorspirit");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"selectmentorspirit = " + bonusNode.OuterXml.ToString());
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

				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"_strSelectedValue = " + _strSelectedValue);
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"strSourceName = " + strSourceName);

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
					objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
						"frmPickMentorSpirit.Choice1BonusNode = " + frmPickMentorSpirit.Choice1BonusNode.OuterXml.ToString());
					string strForce = _strForcedValue;
					if (!frmPickMentorSpirit.Choice1.StartsWith("Adept:") && !frmPickMentorSpirit.Choice1.StartsWith("Magician:"))
						_strForcedValue = frmPickMentorSpirit.Choice1;
					else
						_strForcedValue = "";
					objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
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
					objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
						"frmPickMentorSpirit.Choice2BonusNode = " + frmPickMentorSpirit.Choice2BonusNode.OuterXml.ToString());
					string strForce = _strForcedValue;
					if (!frmPickMentorSpirit.Choice2.StartsWith("Adept:") && !frmPickMentorSpirit.Choice2.StartsWith("Magician:"))
						_strForcedValue = frmPickMentorSpirit.Choice2;
					else
						_strForcedValue = "";
					objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
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
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"_strSelectedValue = " + _strSelectedValue);
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"_strForcedValue = " + _strForcedValue);
			}

			// Check for Paragon bonuses.
			if (bonusNode.LocalName == ("selectparagon"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "selectparagon");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"selectparagon = " + bonusNode.OuterXml.ToString());
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
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "smartlink");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"smartlink = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Smartlink, "smartlink");
			}

			// Check for Adapsin bonus.
			if (bonusNode.LocalName == ("adapsin"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "adapsin");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"adapsin = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Adapsin, "adapsin");
			}

			// Check for SoftWeave bonus.
			if (bonusNode.LocalName == ("softweave"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "softweave");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"softweave = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.SoftWeave, "softweave");
			}

			// Check for Sensitive System.
			if (bonusNode.LocalName == ("sensitivesystem"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "sensitivesystem");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"sensitivesystem = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.SensitiveSystem,
					"sensitivesystem");
			}

			// Check for Movement Percent.
			if (bonusNode.LocalName == ("movementpercent"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "movementpercent");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"movementpercent = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.MovementPercent, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Swim Percent.
			if (bonusNode.LocalName == ("swimpercent"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "swimpercent");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"swimpercent = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.SwimPercent, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Fly Percent.
			if (bonusNode.LocalName == ("flypercent"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "flypercent");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"flypercent = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.FlyPercent, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Fly Speed.
			if (bonusNode.LocalName == ("flyspeed"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "flyspeed");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"flyspeed = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.FlySpeed, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for free Positive Qualities.
			if (bonusNode.LocalName == ("freepositivequalities"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "freepositivequalities");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"freepositivequalities = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.FreePositiveQualities, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for free Negative Qualities.
			if (bonusNode.LocalName == ("freenegativequalities"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "freenegativequalities");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"freenegativequalities = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.FreeNegativeQualities, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Select Side.
			if (bonusNode.LocalName == ("selectside"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "selectside");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"selectside = " + bonusNode.OuterXml.ToString());
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
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"_strSelectedValue = " + _strSelectedValue);
			}

			// Check for Free Spirit Power Points.
			if (bonusNode.LocalName == ("freespiritpowerpoints"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "freespiritpowerpoints");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"freespiritpowerpoints = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.FreeSpiritPowerPoints, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Adept Power Points.
			if (bonusNode.LocalName == ("adeptpowerpoints"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "adeptpowerpoints");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"adeptpowerpoints = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.AdeptPowerPoints, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Adept Powers
			if (bonusNode.LocalName == ("specificpower"))
			{
				//TODO: Probably broken
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "specificpower");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"specificpower = " + bonusNode.OuterXml.ToString());
				// If the character isn't an adept or mystic adept, skip the rest of this.
				if (_objCharacter.AdeptEnabled)
				{
					string strSelection = "";
					_strForcedValue = "";


					objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
						"objXmlSpecificPower = " + bonusNode.OuterXml.ToString());

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
						objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
							"selectlimit = " + bonusNode["selectlimit"].OuterXml.ToString());
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

						objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
							"_strForcedValue = " + _strForcedValue);
						objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
							"_strLimitSelection = " + _strLimitSelection);

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

						objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
							"_strSelectedValue = " + _strSelectedValue);
						objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
							"strSelection = " + strSelection);
					}

					if (bonusNode["selectskill"] != null)
					{
						objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
							"selectskill = " + bonusNode["selectskill"].OuterXml.ToString());
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

						objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
							"_strForcedValue = " + _strForcedValue);
						objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
							"_strLimitSelection = " + _strLimitSelection);

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

						objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
							"_strForcedValue = " + _strForcedValue);
						objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
							"_strSelectedValue = " + _strSelectedValue);
						objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
							"strSelection = " + strSelection);
					}

					if (bonusNode["selecttext"] != null)
					{
						objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
							"selecttext = " + bonusNode["selecttext"].OuterXml.ToString());
						frmSelectText frmPickText = new frmSelectText();


						if (_objCharacter.Pushtext.Count > 0)
						{
							strSelection = _objCharacter.Pushtext.Pop();
						}
						else
						{
							frmPickText.Description = LanguageManager.Instance.GetString("String_Improvement_SelectText")
								.Replace("{0}", strFriendlyName);

							objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
								"_strForcedValue = " + _strForcedValue);
							objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
								"_strLimitSelection = " + _strLimitSelection);

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
						objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
							"_strLimitSelection = " + _strLimitSelection);
						objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
							"strSelection = " + strSelection);
					}

					if (bonusNode["specificattribute"] != null)
					{
						objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
							"specificattribute = " + bonusNode["specificattribute"].OuterXml.ToString());
						strSelection = bonusNode["specificattribute"]["name"].InnerText.ToString();
						objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
							"strSelection = " + strSelection);
					}

					if (bonusNode["selectattribute"] != null)
					{
						objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
							"selectattribute = " + bonusNode["selectattribute"].OuterXml.ToString());
						XmlNode nodSkill = bonusNode;
						if (_strForcedValue.StartsWith("Adept"))
							_strForcedValue = "";

						// Display the Select Attribute window and record which Attribute was selected.
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

						objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
							"_strForcedValue = " + _strForcedValue);
						objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
							"_strLimitSelection = " + _strLimitSelection);

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

						objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
							"_strSelectedValue = " + _strSelectedValue);
						objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
							"strSourceName = " + strSourceName);
						objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
							"_strForcedValue = " + _strForcedValue);
					}

					// Check if the character already has this power
					objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
						"strSelection = " + strSelection);
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

					objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "blnHasPower = " + blnHasPower);

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
						objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager",
							"Adding Power " + strPowerName);
						// If no, add the power and mark it free or give it free levels
						objPower = new Power(_objCharacter);
						_objCharacter.Powers.Add(objPower);

						// Get the Power information
						XmlDocument objXmlDocument = new XmlDocument();
						objXmlDocument = XmlManager.Instance.Load("powers.xml");
						XmlNode objXmlPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"" + strPowerName + "\"]");
						objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
							"objXmlPower = " + objXmlPower.OuterXml.ToString());

						bool blnLevels = false;
						if (objXmlPower["levels"] != null)
							blnLevels = (objXmlPower["levels"].InnerText == "yes");
						objPower.LevelsEnabled = blnLevels;
						objPower.Name = strPowerNameLimit;
						objPower.PointsPerLevel = Convert.ToDecimal(objXmlPower["points"].InnerText, GlobalOptions.Instance.CultureInfo);
						objPower.Source = objXmlPower["source"].InnerText;
						objPower.Page = objXmlPower["page"].InnerText;
						if (strSelection != string.Empty)
							objPower.Extra = strSelection;
						if (objXmlPower["doublecost"] != null)
							objPower.DoubleCost = false;

						if (blnFree)
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
							objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovements");
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
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.AdeptPower, "");
			}

			// Select a Power.
			if (bonusNode.LocalName == ("selectpower"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "selectpower");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"_strSelectedValue = " + _strSelectedValue);
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"_strForcedValue = " + _strForcedValue);

				bool blnExistingPower = false;
				foreach (Power objExistingPower in _objCharacter.Powers)
				{
					if (objExistingPower.BonusSource == strSourceName)
					{
						blnExistingPower = true;
						if (!objExistingPower.Free)
						{
							if (objExistingPower.Name.StartsWith("Improved Reflexes"))
							{
								if (objExistingPower.Name.EndsWith("1"))
								{
									if (intRating >= 6)
										objExistingPower.FreePoints = 1.5M;
									else
										objExistingPower.FreePoints = 0;
								}
								else if (objExistingPower.Name.EndsWith("2"))
								{
									if (intRating >= 10)
										objExistingPower.FreePoints = 2.5M;
									else if (intRating >= 4)
										objExistingPower.FreePoints = 1.0M;
									else
										objExistingPower.FreePoints = 0;
								}
								else
								{
									if (intRating >= 14)
										objExistingPower.FreePoints = 3.5M;
									else if (intRating >= 8)
										objExistingPower.FreePoints = 2.0M;
									else if (intRating >= 4)
										objExistingPower.FreePoints = 1.0M;
									else
										objExistingPower.FreePoints = 0;
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
						}
					}
				}

				if (!blnExistingPower)
				{
					// Display the Select Skill window and record which Skill was selected.
					frmSelectPower frmPickPower = new frmSelectPower(_objCharacter);
					objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
						"selectpower = " + bonusNode.OuterXml.ToString());
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
					XmlNode objXmlPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"" + _strSelectedValue + "\"]");
					string strSelection = "";

					objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
						"_strSelectedValue = " + _strSelectedValue);
					objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
						"strSourceName = " + strSourceName);

					XmlNode objBonus = objXmlPower["bonus"];

					string strPowerNameLimit = _strSelectedValue;
					if (objBonus != null)
					{
						if (objBonus["selectlimit"] != null)
						{
							objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
								"selectlimit = " + objBonus["selectlimit"].OuterXml.ToString());
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

							objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
								"_strForcedValue = " + _strForcedValue);
							objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
								"_strLimitSelection = " + _strLimitSelection);

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

							objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
								"_strSelectedValue = " + _strSelectedValue);
							objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
								"strSelection = " + strSelection);
						}

						if (objBonus["selectskill"] != null)
						{
							objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
								"selectskill = " + objBonus["selectskill"].OuterXml.ToString());
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

							objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
								"_strForcedValue = " + _strForcedValue);
							objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
								"_strLimitSelection = " + _strLimitSelection);

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

							objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
								"_strForcedValue = " + _strForcedValue);
							objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
								"_strSelectedValue = " + _strSelectedValue);
							objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
								"strSelection = " + strSelection);
						}

						if (objBonus["selecttext"] != null)
						{
							objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
								"selecttext = " + objBonus["selecttext"].OuterXml.ToString());
							frmSelectText frmPickText = new frmSelectText();
							frmPickText.Description = LanguageManager.Instance.GetString("String_Improvement_SelectText")
								.Replace("{0}", strFriendlyName);

							objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
								"_strForcedValue = " + _strForcedValue);
							objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
								"_strLimitSelection = " + _strLimitSelection);

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

							objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
								"_strLimitSelection = " + _strLimitSelection);
							objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
								"strSelection = " + strSelection);
						}

						if (objBonus["specificattribute"] != null)
						{
							objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
								"specificattribute = " + objBonus["specificattribute"].OuterXml.ToString());
							strSelection = objBonus["specificattribute"]["name"].InnerText.ToString();
							objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
								"strSelection = " + strSelection);
						}

						if (objBonus["selectattribute"] != null)
						{
							objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
								"selectattribute = " + objBonus["selectattribute"].OuterXml.ToString());
							XmlNode nodSkill = objBonus;
							if (_strForcedValue.StartsWith("Adept"))
								_strForcedValue = "";

							// Display the Select Attribute window and record which Attribute was selected.
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

							objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
								"_strForcedValue = " + _strForcedValue);
							objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
								"_strLimitSelection = " + _strLimitSelection);

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

							objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
								"_strSelectedValue = " + _strSelectedValue);
							objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
								"strSourceName = " + strSourceName);
							objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
								"_strForcedValue = " + _strForcedValue);
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

					objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "blnHasPower = " + blnHasPower);

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
						objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager",
							"Adding Power " + _strSelectedValue);
						// Get the Power information
						_objCharacter.Powers.Add(objPower);
						objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
							"objXmlPower = " + objXmlPower.OuterXml.ToString());

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
							objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovements");
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

			// Check for Armor Encumbrance Penalty.
			if (bonusNode.LocalName == ("armorencumbrancepenalty"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "armorencumbrancepenalty");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"armorencumbrancepenalty = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.ArmorEncumbrancePenalty, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Initiation.
			if (bonusNode.LocalName == ("initiation"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "initiation");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"initiation = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Initiation, "",
					ValueToInt(bonusNode.InnerText, intRating));
				_objCharacter.InitiateGrade += ValueToInt(bonusNode.InnerText, intRating);
			}

			// Check for Submersion.
			if (bonusNode.LocalName == ("submersion"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "submersion");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"submersion = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Submersion, "",
					ValueToInt(bonusNode.InnerText, intRating));
				_objCharacter.SubmersionGrade += ValueToInt(bonusNode.InnerText, intRating);
			}

			// Check for Skillwires.
			if (bonusNode.LocalName == ("skillwire"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "skillwire");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"skillwire = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Skillwire, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Damage Resistance.
			if (bonusNode.LocalName == ("damageresistance"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "damageresistance");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"damageresistance = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.DamageResistance,
					"damageresistance", ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Restricted Item Count.
			if (bonusNode.LocalName == ("restricteditemcount"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "restricteditemcount");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"restricteditemcount = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.RestrictedItemCount, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Judge Intentions.
			if (bonusNode.LocalName == ("judgeintentions"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "judgeintentions");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"judgeintentions = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.JudgeIntentions, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Composure.
			if (bonusNode.LocalName == ("composure"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "composure");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"composure = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Composure, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Lift and Carry.
			if (bonusNode.LocalName == ("liftandcarry"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "liftandcarry");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"liftandcarry = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.LiftAndCarry, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Memory.
			if (bonusNode.LocalName == ("memory"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "memory");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"memory = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Memory, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Concealability.
			if (bonusNode.LocalName == ("concealability"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "concealability");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"concealability = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Concealability, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Drain Resistance.
			if (bonusNode.LocalName == ("drainresist"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "drainresist");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"drainresist = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.DrainResistance, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Fading Resistance.
			if (bonusNode.LocalName == ("fadingresist"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "fadingresist");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"fadingresist = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.FadingResistance, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Notoriety.
			if (bonusNode.LocalName == ("notoriety"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "notoriety");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"notoriety = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Notoriety, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Complex Form Limit.
			if (bonusNode.LocalName == ("complexformlimit"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "complexformlimit");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"complexformlimit = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.ComplexFormLimit, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Spell Limit.
			if (bonusNode.LocalName == ("spelllimit"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "spelllimit");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"spelllimit = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.SpellLimit, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Spell Category bonuses.
			if (bonusNode.LocalName == ("spellcategory"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "spellcategory");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"spellcategory = " + bonusNode.OuterXml.ToString());

				string strUseUnique = strUnique;
				if (bonusNode["name"].Attributes["precedence"] != null)
					strUseUnique = "precedence" + bonusNode["name"].Attributes["precedence"].InnerText;

				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement(bonusNode["name"].InnerText, objImprovementSource, strSourceName,
					Improvement.ImprovementType.SpellCategory, strUseUnique, ValueToInt(bonusNode["val"].InnerText, intRating));
			}

			// Check for Throwing Range bonuses.
			if (bonusNode.LocalName == ("throwrange"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "throwrange");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"throwrange = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.ThrowRange, strUnique,
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Throwing STR bonuses.
			if (bonusNode.LocalName == ("throwstr"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "throwstr");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"throwstr = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.ThrowSTR, strUnique,
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Skillsoft access.
			if (bonusNode.LocalName == ("skillsoftaccess"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "skillsoftaccess");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"skillsoftaccess = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.SkillsoftAccess, "");
			}

			// Check for Quickening Metamagic.
			if (bonusNode.LocalName == ("quickeningmetamagic"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "quickeningmetamagic");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"quickeningmetamagic = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.QuickeningMetamagic, "");
			}

			// Check for ignore Stun CM Penalty.
			if (bonusNode.LocalName == ("ignorecmpenaltystun"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "ignorecmpenaltystun");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"ignorecmpenaltystun = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.IgnoreCMPenaltyStun, "");
			}

			// Check for ignore Physical CM Penalty.
			if (bonusNode.LocalName == ("ignorecmpenaltyphysical"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "ignorecmpenaltyphysical");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"ignorecmpenaltyphysical = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.IgnoreCMPenaltyPhysical, "");
			}

			// Check for a Cyborg Essence which will permanently set the character's ESS to 0.1.
			if (bonusNode.LocalName == ("cyborgessence"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "cyborgessence");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"cyborgessence = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.CyborgEssence, "");
			}

			// Check for Maximum Essence which will permanently modify the character's Maximum Essence value.
			if (bonusNode.LocalName == ("essencemax"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "essencemax");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"essencemax = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.EssenceMax, "",
					ValueToInt(bonusNode.InnerText, intRating));
			}

			// Check for Select Sprite.
			if (bonusNode.LocalName == ("selectsprite"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "selectsprite");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"selectsprite = " + bonusNode.OuterXml.ToString());
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

				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement(frmPickItem.SelectedItem, objImprovementSource, strSourceName,
					Improvement.ImprovementType.AddSprite,
					"");
			}

			// Check for Black Market Discount.
			if (bonusNode.LocalName == ("blackmarketdiscount"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "blackmarketdiscount");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"blackmarketdiscount = " + bonusNode.OuterXml.ToString());
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.BlackMarketDiscount,
					strUnique);
				_objCharacter.BlackMarket = true;
			}
			// Select Armor (Mostly used for Custom Fit (Stack)).
			if (bonusNode.LocalName == ("selectarmor"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "selectarmor");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"selectarmor = " + bonusNode.OuterXml.ToString());
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

					objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strLimitSelection = " + _strLimitSelection);
					objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strForcedValue = " + _strForcedValue);

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
					objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strSelectedValue = " + _strSelectedValue);
					objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "strSelectedValue = " + strSelectedValue);
				}

			}

			// Select Weapon (custom entry for things like Spare Clip).
			if (bonusNode.LocalName == ("selectweapon"))
			{
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "selectweapon");
				objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
					"selectweapon = " + bonusNode.OuterXml.ToString());
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

					objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
						"_strLimitSelection = " + _strLimitSelection);
					objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
						"_strForcedValue = " + _strForcedValue);

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
					objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
						"_strSelectedValue = " + _strSelectedValue);
					objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
						"strSelectedValue = " + strSelectedValue);
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

					objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
						"_strLimitSelection = " + _strLimitSelection);
					objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
						"_strForcedValue = " + _strForcedValue);

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
					objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
						"_strSelectedValue = " + _strSelectedValue);
					objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
						"strSelectedValue = " + strSelectedValue);
				}

				// Create the Improvement.
				objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
				CreateImprovement(strSelectedValue, objImprovementSource, strSourceName, Improvement.ImprovementType.Text, strUnique);
			}
			// Select an Optional Power.
			if (bonusNode.LocalName == ("optionalpowers"))
			{
				XmlNodeList objXmlPowerList = bonusNode.SelectNodes("optionalpower");
				//objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager","selectoptionalpower");
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
					//values.Add(new KeyValuePair<string, Stream>(title, contents));
					lstValue.Add(new KeyValuePair<string, string>(strQuality,strForcedValue));
				}
				frmPickPower.LimitToList(lstValue);


				// Check to see if there is only one possible selection because of _strLimitSelection.
				if (_strForcedValue != "")
					_strLimitSelection = _strForcedValue;

				//objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strForcedValue = " + _strForcedValue);
				//objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strLimitSelection = " + _strLimitSelection);

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
				
                objPower.Create(objXmlPowerNode, _objCharacter, objPowerNode, intRating, strForcedValue);
				_objCharacter.CritterPowers.Add(objPower);
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
            objFunctions.LogWrite(CommonFunctions.LogType.Entering, "Chummer.ImprovementManager", "RemoveImprovements");
			objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
				"objImprovementSource = " + objImprovementSource.ToString());
			objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
				"strSourceName = " + strSourceName);

            // If there is no character object, don't try to remove any Improvements.
            if (_objCharacter == null)
            {
                objFunctions.LogWrite(CommonFunctions.LogType.Exiting, "Chummer.ImprovementManager", "RemoveImprovements");
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
					for (int i = _objCharacter.Skills.Count - 1; i >= 0; i--)
					{
						//wrote as foreach first, modify collection, not want rename
						Skill skill = _objCharacter.Skills[i];
						for (int j = skill.Fold.Count - 1; j >= 0; j--)
						{
							Skill fold = skill.Fold[i];
							if (fold.Id.ToString() == objImprovement.ImprovedName)
							{
								skill.Free(fold);
								_objCharacter.Skills.Remove(fold);
							}
						}

						if (skill.Id.ToString() == objImprovement.ImprovedName)
						{
							while(skill.Fold.Count > 0) skill.Free(skill.Fold[0]);
							//empty list, can't call clear as exposed list is RO

							_objCharacter.Skills.Remove(skill);
						}
					}
				}

                // Remove "free" adept powers if any.
                if (objImprovement.ImproveType == Improvement.ImprovementType.AdeptPower)
                {
                    // Load the power from XML.
                    // objImprovement.Notes = name of the mentor spirit choice. Find the power name from here.
                    XmlDocument objXmlMentorDocument = new XmlDocument();
                    objXmlMentorDocument = XmlManager.Instance.Load("mentors.xml");
					XmlNode objXmlMentorBonus =
						objXmlMentorDocument.SelectSingleNode("/chummer/mentors/mentor/choices/choice[name = \"" + objImprovement.Notes +
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
						_objCharacter.BlackMarket = false;
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
						_objCharacter.Uneducated = false;
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
						_objCharacter.Uncouth = false;
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
                        _objCharacter.SchoolOfHardKnocks = false;
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
                        _objCharacter.JackOfAllTrades = false;
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
                        _objCharacter.CollegeEducation = false;
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
                        _objCharacter.Linguist = false;
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
                        _objCharacter.TechSchool = false;
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
                // Turn off the BlackMarket flag if it is being removed.
                if (objImprovement.ImproveType == Improvement.ImprovementType.BlackMarket)
                {
                    bool blnFound = false;
                    // See if the character has anything else that is granting them access to BlackMarket.
                    foreach (Improvement objCharacterImprovement in _objCharacter.Improvements)
                    {
                        // Skip items from the current Improvement source.
                        if (objCharacterImprovement.SourceName != objImprovement.SourceName)
                        {
                            if (objCharacterImprovement.ImproveType == Improvement.ImprovementType.BlackMarket)
                            {
                                blnFound = true;
                                break;
                            }
                        }
                    }

                    if (!blnFound)
                        _objCharacter.BlackMarket = false;
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
			}

            objFunctions.LogWrite(CommonFunctions.LogType.Exiting, "Chummer.ImprovementManager", "RemoveImprovements");
        }

		/// <summary>
		/// Create a new Improvement and add it to the Character.
		/// </summary>
		/// <param name="strImprovedName">Speicific name of the Improved object - typically the name of an Attribute being improved.</param>
		/// <param name="objImprovementSource">Type of object that grants this Improvement.</param>
		/// <param name="strSourceName">Name of the item that grants this Improvement.</param>
		/// <param name="objImprovementType">Type of object the Improvement applies to.</param>
		/// <param name="strUnique">Name of the pool this Improvement should be added to - only the single higest value in the pool will be applied to the character.</param>
		/// <param name="intValue">Set a Value for the Improvement.</param>
		/// <param name="intRating">Set a Rating for the Improvement - typically used for Adept Powers.</param>
		/// <param name="intMinimum">Improve the Minimum for an Attribute by the given amount.</param>
		/// <param name="intMaximum">Improve the Maximum for an Attribute by the given amount.</param>
		/// <param name="intAugmented">Improve the Augmented value for an Attribute by the given amount.</param>
		/// <param name="intAugmentedMaximum">Improve the Augmented Maximum value for an Attribute by the given amount.</param>
		/// <param name="strExclude">A list of child items that should not receive the Improvement's benefit (typically for Skill Groups).</param>
		/// <param name="blnAddToRating">Whether or not the bonus applies to a Skill's Rating instead of the dice pool in general.</param>
		public void CreateImprovement(string strImprovedName, Improvement.ImprovementSource objImprovementSource,
			string strSourceName, Improvement.ImprovementType objImprovementType, string strUnique,
			int intValue = 0, int intRating = 1, int intMinimum = 0, int intMaximum = 0, int intAugmented = 0,
			int intAugmentedMaximum = 0, string strExclude = "", bool blnAddToRating = false)
		{
            objFunctions.LogWrite(CommonFunctions.LogType.Entering, "Chummer.ImprovementManager", "CreateImprovement");
			objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
				"strImprovedName = " + strImprovedName);
			objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
				"objImprovementSource = " + objImprovementSource.ToString());
			objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
				"strSourceName = " + strSourceName);
			objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
				"objImprovementType = " + objImprovementType.ToString());
            objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "strUnique = " + strUnique);
			objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
				"intValue = " + intValue.ToString());
			objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
				"intRating = " + intRating.ToString());
			objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
				"intMinimum = " + intMinimum.ToString());
			objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
				"intMaximum = " + intMaximum.ToString());
			objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
				"intAugmented = " + intAugmented.ToString());
			objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
				"intAugmentedMaximum = " + intAugmentedMaximum.ToString());
            objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "strExclude = " + strExclude);
			objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager",
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

            objFunctions.LogWrite(CommonFunctions.LogType.Exiting, "Chummer.ImprovementManager", "CreateImprovement");
        }

		/// <summary>
		/// Clear all of the Improvements from the Transaction List.
		/// </summary>
		public void Commit()
		{
            objFunctions.LogWrite(CommonFunctions.LogType.Entering, "Chummer.ImprovementManager", "Commit");
            // Clear all of the Improvements from the Transaction List.
			_lstTransaction.Clear();
            objFunctions.LogWrite(CommonFunctions.LogType.Exiting, "Chummer.ImprovementManager", "Commit");
        }

		/// <summary>
		/// Rollback all of the Improvements from the Transaction List.
		/// </summary>
		private void Rollback()
		{
            objFunctions.LogWrite(CommonFunctions.LogType.Entering, "Chummer.ImprovementManager", "Rollback");
            // Remove all of the Improvements that were added.
			foreach (Improvement objImprovement in _lstTransaction)
				RemoveImprovements(objImprovement.ImproveSource, objImprovement.SourceName);

			_lstTransaction.Clear();
            objFunctions.LogWrite(CommonFunctions.LogType.Exiting, "Chummer.ImprovementManager", "Rollback");
        }

		#endregion
	}
}