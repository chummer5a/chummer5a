using System;
using System.Collections.Generic;
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
			switch (strValue)
			{
				case "Attribute":
					return ImprovementType.Attribute;
				case "Text":
					return ImprovementType.Text;
				case "Armor":
					return ImprovementType.Armor;
				case "Reach":
					return ImprovementType.Reach;
				case "Nuyen":
					return ImprovementType.Nuyen;
				case "Essence":
					return ImprovementType.Essence;
				case "Reaction":
					return ImprovementType.Reaction;
				case "PhysicalCM":
					return ImprovementType.PhysicalCM;
				case "StunCM":
					return ImprovementType.StunCM;
				case "UnarmedDV":
					return ImprovementType.UnarmedDV;
				case "SkillGroup":
					return ImprovementType.SkillGroup;
				case "SkillCategory":
					return ImprovementType.SkillCategory;
				case "SkillAttribute":
					return ImprovementType.SkillAttribute;
				case "InitiativePass":
					return ImprovementType.InitiativePass;
				case "MatrixInitiative":
					return ImprovementType.MatrixInitiative;
				case "MatrixInitiativePass":
					return ImprovementType.MatrixInitiativePass;
				case "LifestyleCost":
					return ImprovementType.LifestyleCost;
				case "CMThreshold":
					return ImprovementType.CMThreshold;
				case "EnhancedArticulation":
					return ImprovementType.EnhancedArticulation;
				case "WeaponCategoryDV":
					return ImprovementType.WeaponCategoryDV;
				case "CyberwareEssCost":
					return ImprovementType.CyberwareEssCost;
				case "SpecialTab":
					return ImprovementType.SpecialTab;
				case "Initiative":
					return ImprovementType.Initiative;
				case "Uneducated":
					return ImprovementType.Uneducated;
				case "LivingPersonaResponse":
					return ImprovementType.LivingPersonaResponse;
				case "LivingPersonaSignal":
					return ImprovementType.LivingPersonaSignal;
				case "LivingPersonaFirewall":
					return ImprovementType.LivingPersonaFirewall;
				case "LivingPersonaSystem":
					return ImprovementType.LivingPersonaSystem;
				case "LivingPersonaBiofeedback":
					return ImprovementType.LivingPersonaBiofeedback;
				case "Smartlink":
					return ImprovementType.Smartlink;
				case "BiowareEssCost":
					return ImprovementType.BiowareEssCost;
				case "GenetechCostMultiplier":
					return ImprovementType.GenetechCostMultiplier;
				case "BasicBiowareEssCost":
					return ImprovementType.BasicBiowareEssCost;
				case "TransgenicsBiowareCost":
					return ImprovementType.TransgenicsBiowareCost;
				case "SoftWeave":
					return ImprovementType.SoftWeave;
				case "SensitiveSystem":
					return ImprovementType.SensitiveSystem;
				case "ConditionMonitor":
					return ImprovementType.ConditionMonitor;
				case "UnarmedDVPhysical":
					return ImprovementType.UnarmedDVPhysical;
				case "MovementPercent":
					return ImprovementType.MovementPercent;
				case "Adapsin":
					return ImprovementType.Adapsin;
				case "FreePositiveQualities":
					return ImprovementType.FreePositiveQualities;
				case "FreeNegativeQualities":
					return ImprovementType.FreeNegativeQualities;
				case "NuyenMaxBP":
					return ImprovementType.NuyenMaxBP;
				case "CMOverflow":
					return ImprovementType.CMOverflow;
				case "FreeSpiritPowerPoints":
					return ImprovementType.FreeSpiritPowerPoints;
				case "AdeptPowerPoints":
					return ImprovementType.AdeptPowerPoints;
				case "ArmorEncumbrancePenalty":
					return ImprovementType.ArmorEncumbrancePenalty;
				case "Uncouth":
					return ImprovementType.Uncouth;
				case "Initiation":
					return ImprovementType.Initiation;
				case "Submersion":
					return ImprovementType.Submersion;
				case "Infirm":
					return ImprovementType.Infirm;
				case "Skillwire":
					return ImprovementType.Skillwire;
				case "DamageResistance":
					return ImprovementType.DamageResistance;
				case "RestrictedItemCount":
					return ImprovementType.RestrictedItemCount;
				case "AdeptLinguistics":
					return ImprovementType.AdeptLinguistics;
				case "SwimPercent":
					return ImprovementType.SwimPercent;
				case "FlyPercent":
					return ImprovementType.FlyPercent;
				case "FlySpeed":
					return ImprovementType.FlySpeed;
				case "JudgeIntentions":
					return ImprovementType.JudgeIntentions;
				case "LiftAndCarry":
					return ImprovementType.LiftAndCarry;
				case "Memory":
					return ImprovementType.Memory;
				case "SwapSkillAttribute":
					return ImprovementType.SwapSkillAttribute;
				case "DrainResistance":
					return ImprovementType.DrainResistance;
				case "FadingResistance":
					return ImprovementType.FadingResistance;
				case "MatrixInitiativePassAdd":
					return ImprovementType.MatrixInitiativePassAdd;
				case "InitiativePassAdd":
					return ImprovementType.InitiativePassAdd;
				case "Composure":
					return ImprovementType.Composure;
				case "UnarmedAP":
					return ImprovementType.UnarmedAP;
				case "CMThresholdOffset":
					return ImprovementType.CMThresholdOffset;
				case "Restricted":
					return ImprovementType.Restricted;
				case "Notoriety":
					return ImprovementType.Notoriety;
				case "SpellCategory":
					return ImprovementType.SpellCategory;
				case "ThrowRange":
					return ImprovementType.ThrowRange;
				case "SkillsoftAccess":
					return ImprovementType.SkillsoftAccess;
				case "AddSprite":
					return ImprovementType.AddSprite;
				case "BlackMarketDiscount":
					return ImprovementType.BlackMarketDiscount;
				case "SelectWeapon":
					return ImprovementType.SelectWeapon;
				case "ComplexFormLimit":
					return ImprovementType.ComplexFormLimit;
				case "SpellLimit":
					return ImprovementType.SpellLimit;
				case "QuickeningMetamagic":
					return ImprovementType.QuickeningMetamagic;
				case "BasicLifestyleCost":
					return ImprovementType.BasicLifestyleCost;
				case "ThrowSTR":
					return ImprovementType.ThrowSTR;
				case "IgnoreCMPenaltyStun":
					return ImprovementType.IgnoreCMPenaltyStun;
				case "IgnoreCMPenaltyPhysical":
					return ImprovementType.IgnoreCMPenaltyPhysical;
				case "CyborgEssence":
					return ImprovementType.CyborgEssence;
				case "EssenceMax":
					return ImprovementType.EssenceMax;
                case "SpecificPower":
                    return ImprovementType.AdeptPower;
                case "SpecificQuality":
                    return ImprovementType.SpecificQuality;
                case "MartialArt":
                    return ImprovementType.MartialArt;
                case "LimitModifier":
                    return ImprovementType.LimitModifier;
				default:
					return ImprovementType.Skill;
			}
		}

		/// <summary>
		/// Convert a string to an ImprovementSource.
		/// </summary>
		/// <param name="strValue">String value to convert.</param>
		public ImprovementSource ConvertToImprovementSource(string strValue)
		{
			switch (strValue)
			{
				case "Power":
					return ImprovementSource.Power;
				case "Metatype":
					return ImprovementSource.Metatype;
				case "Cyberware":
					return ImprovementSource.Cyberware;
				case "Metavariant":
					return ImprovementSource.Metavariant;
				case "Bioware":
					return ImprovementSource.Bioware;
				case "Nanotech":
					return ImprovementSource.Nanotech;
				case "Genetech":
					return ImprovementSource.Genetech;
				case "ArmorEncumbrance":
					return ImprovementSource.ArmorEncumbrance;
				case "Gear":
					return ImprovementSource.Gear;
				case "Spell":
					return ImprovementSource.Spell;
				case "MartialArtAdvantage":
					return ImprovementSource.MartialArtAdvantage;
				case "Initiation":
					return ImprovementSource.Initiation;
				case "Submersion":
					return ImprovementSource.Submersion;
				case "Metamagic":
					return ImprovementSource.Metamagic;
				case "Echo":
					return ImprovementSource.Echo;
				case "Armor":
					return ImprovementSource.Armor;
				case "ArmorMod":
					return ImprovementSource.ArmorMod;
				case "EssenceLoss":
					return ImprovementSource.EssenceLoss;
				case "ConditionMonitor":
					return ImprovementSource.ConditionMonitor;
				case "CritterPower":
					return ImprovementSource.CritterPower;
				case "ComplexForm":
					return ImprovementSource.ComplexForm;
				case "EdgeUse":
					return ImprovementSource.EdgeUse;
				case "MutantCritter":
					return ImprovementSource.MutantCritter;
				case "Cyberzombie":
					return ImprovementSource.Cyberzombie;
				case "StackedFocus":
					return ImprovementSource.StackedFocus;
				case "AttributeLoss":
					return ImprovementSource.AttributeLoss;
				case "Custom":
					return ImprovementSource.Custom;
				default:
			return ImprovementSource.Quality;
			}
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
			get
			{
				return _blnCustom;
			}
			set
			{
				_blnCustom = value;
			}
		}

		/// <summary>
		/// User-entered name for the custom Improvement.
		/// </summary>
		public string CustomName
		{
			get
			{
				return _strCustomName;
			}
			set
			{
				_strCustomName = value;
			}
		}

		/// <summary>
		/// ID from the Improvements file. Only used for custom-made (manually created) Improvements.
		/// </summary>
		public string CustomId
		{
			get
			{
				return _strCustomId;
			}
			set
			{
				_strCustomId = value;
			}
		}

		/// <summary>
		/// Group name for the Custom Improvement.
		/// </summary>
		public string CustomGroup
		{
			get
			{
				return _strCustomGroup;
			}
			set
			{
				_strCustomGroup = value;
			}
		}

		/// <summary>
		/// User-entered notes for the custom Improvement.
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
        /// Name of the Skill or Attribute that the Improvement is improving.
        /// </summary>
        public string ImprovedName
        {
            get
            {
                return _strImprovedName;
            }
            set
            {
                _strImprovedName = value;
            }
        }

        /// <summary>
        /// Name of the source that granted this Improvement.
        /// </summary>
        public string SourceName
        {
            get
            {
                return _strSourceName;
            }
            set
            {
                _strSourceName = value;
            }
        }

        /// <summary>
        /// The type of Object that the Improvement is improving.
        /// </summary>
        public ImprovementType ImproveType
        {
            get
            {
                return _objImprovementType;
            }
            set
            {
                _objImprovementType = value;
            }
        }

        /// <summary>
        /// The type of Object that granted this Improvement.
        /// </summary>
        public ImprovementSource ImproveSource
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
		/// Minimum value modifier.
		/// </summary>
		public int Minimum
		{
			get
			{
				return _intMin;
			}
			set
			{
				_intMin = value;
			}
		}

        /// <summary>
        /// Maximum value modifier.
        /// </summary>
        public int Maximum
        {
            get
            {
                return _intMax;
            }
            set
            {
                _intMax = value;
            }
        }

        /// <summary>
        /// Augmented Maximum value modifier.
        /// </summary>
        public int AugmentedMaximum
        {
            get
            {
                return _intAugMax;
            }
            set
            {
                _intAugMax = value;
            }
        }

        /// <summary>
        /// Augmented score modifier.
        /// </summary>
        public int Augmented
        {
            get
            {
                return _intAug;
            }
            set
            {
                _intAug = value;
            }
        }

        /// <summary>
        /// Value modifier.
        /// </summary>
        public int Value
        {
            get
            {
                return _intVal;
            }
            set
            {
                _intVal = value;
            }
        }

        /// <summary>
        /// The Rating value for the Improvement. This is 1 by default.
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
		/// A list of child items that should not receive the Improvement's benefit (typically for excluding a Skill from a Skill Group bonus).
		/// </summary>
		public string Exclude
		{
			get
			{
				return _strExclude;
			}
			set
			{
				_strExclude = value;
			}
		}

		/// <summary>
		/// A Unique name for the Improvement. Only the highest value of any one Improvement that is part of this Unique Name group will be applied.
		/// </summary>
		public string UniqueName
		{
			get
			{
				return _strUniqueName;
			}
			set
			{
				_strUniqueName = value;
			}
		}

		/// <summary>
		/// Whether or not the bonus applies directly to a Skill's Rating
		/// </summary>
		public bool AddToRating
		{
			get
			{
				return _blnAddToRating;
			}
			set
			{
				_blnAddToRating = value;
			}
		}

		/// <summary>
		/// Whether or not the Improvement is enabled and provided its bonus.
		/// </summary>
		public bool Enabled
		{
			get
			{
				return _blnEnabled;
			}
			set
			{
				_blnEnabled = value;
			}
		}

		/// <summary>
		/// Sort order for Custom Improvements.
		/// </summary>
		public int SortOrder
		{
			get
			{
				return _intOrder;
			}
			set
			{
				_intOrder = value;
			}
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
			get
			{
				return _strLimitSelection;
			}
			set
			{
				_strLimitSelection = value;
			}
		}

		/// <summary>
		/// The string that was entered or selected from any of the dialogue windows that were presented because of this Improvement.
		/// </summary>
		public string SelectedValue
		{
			get
			{
				return _strSelectedValue;
			}
		}

		/// <summary>
		/// Force any dialogue windows that open to use this string as their selected value.
		/// </summary>
		public string ForcedValue
		{
			set
			{
				_strForcedValue = value;
			}
		}
		#endregion

		#region Helper Methods

		/// <summary>
		/// Retrieve the total Improvement value for the specified ImprovementType.
		/// </summary>
		/// <param name="objImprovementType">ImprovementType to retrieve the value of.</param>
		/// <param name="blnAddToRating">Whether or not we should only retrieve values that have AddToRating enabled.</param>
		/// <param name="strImprovedName">Name to assign to the Improvement.</param>
		public int ValueOf(Improvement.ImprovementType objImprovementType, bool blnAddToRating = false, string strImprovedName = null)
		{
            objFunctions.LogWrite(CommonFunctions.LogType.Entering, "Chummer.ImprovementManager", "ValueOf");
            objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "objImprovementType = " + objImprovementType.ToString());
            objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "blnAddToRating = " + blnAddToRating.ToString());
            objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "strImprovedName = " + ("" + strImprovedName).ToString());

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
					if (_objCharacter.RESEnabled && objImprovement.ImproveSource == Improvement.ImprovementSource.Gear && objImprovementType == Improvement.ImprovementType.MatrixInitiativePass)
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
							string[,] strValues = new string[,] { { objImprovement.UniqueName, objImprovement.Value.ToString() } };
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
					if (_objCharacter.RESEnabled && objImprovement.ImproveSource == Improvement.ImprovementSource.Gear && objImprovementType == Improvement.ImprovementType.MatrixInitiativePass)
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
							string[,] strValues = new string[,] { { objImprovement.UniqueName, objImprovement.Value.ToString() } };
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
            objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "intRating = " + intRating.ToString());
            
            if (strValue.Contains("Rating") || strValue.Contains("BOD") || strValue.Contains("AGI") || strValue.Contains("REA") || strValue.Contains("STR") || strValue.Contains("CHA") || strValue.Contains("INT") || strValue.Contains("LOG") || strValue.Contains("WIL") || strValue.Contains("EDG") || strValue.Contains("MAG") || strValue.Contains("RES"))
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
            objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "objXmlNode = " + objXmlNode.OuterXml.ToString());
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
		public bool CreateImprovements(Improvement.ImprovementSource objImprovementSource, string strSourceName, XmlNode nodBonus, bool blnConcatSelectedValue = false, int intRating = 1, string strFriendlyName = "", object fCreate = null)
		{
            objFunctions.LogWrite(CommonFunctions.LogType.Entering, "Chummer.ImprovementManager", "CreateImprovements");
            objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "objImprovementSource = " + objImprovementSource.ToString());
            objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "strSourceName = " + strSourceName);
            objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "nodBonus = " + nodBonus.OuterXml.ToString());
            objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "blnConcatSelectedValue = " + blnConcatSelectedValue.ToString());
            objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "intRating = " + intRating.ToString());
            objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "strFriendlyName = " + strFriendlyName);
            objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "intRating = " + intRating.ToString());

            bool blnSuccess = true;

            try
            {
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

                objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strForcedValue = " + _strForcedValue);
                objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strLimitSelection = " + _strLimitSelection);

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
                        if (_strForcedValue != "")
                            _strLimitSelection = _strForcedValue;

                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strForcedValue = " + _strSelectedValue);
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strLimitSelection = " + _strLimitSelection);

                        // Display the Select Text window and record the value that was entered.
                        frmSelectText frmPickText = new frmSelectText();
                        frmPickText.Description = LanguageManager.Instance.GetString("String_Improvement_SelectText").Replace("{0}", strFriendlyName);

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
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strSelectedValue = " + _strSelectedValue);
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "strSourceName = " + strSourceName);

                        // Create the Improvement.
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement(frmPickText.SelectedValue, objImprovementSource, strSourceName, Improvement.ImprovementType.Text, strUnique);
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
                if (nodBonus.HasChildNodes)
                {
                    objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Has Child Nodes");
                    // Add an Attribute.
                    if (NodeExists(nodBonus, "addattribute"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "addattribute");
                        if (nodBonus["addattribute"]["name"].InnerText == "MAG")
                        {
                            _objCharacter.MAGEnabled = true;
                            objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement for MAG");
                            CreateImprovement("MAG", objImprovementSource, strSourceName, Improvement.ImprovementType.Attribute, "enableattribute", 0, 0);
                        }
                        else if (nodBonus["addattribute"]["name"].InnerText == "RES")
                        {
                            _objCharacter.RESEnabled = true;
                            objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement for RES");
                            CreateImprovement("RES", objImprovementSource, strSourceName, Improvement.ImprovementType.Attribute, "enableattribute", 0, 0);
                        }
                    }

                    // Enable a special tab.
                    if (NodeExists(nodBonus, "enabletab"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "enabletab");
                        foreach (XmlNode objXmlEnable in nodBonus["enabletab"].ChildNodes)
                        {
                            switch (objXmlEnable.InnerText)
                            {
                                case "magician":
                                    _objCharacter.MagicianEnabled = true;
                                    objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "magician");
                                    CreateImprovement("Magician", objImprovementSource, strSourceName, Improvement.ImprovementType.SpecialTab, "enabletab", 0, 0);
                                    break;
                                case "adept":
                                    _objCharacter.AdeptEnabled = true;
                                    objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "adept");
                                    CreateImprovement("Adept", objImprovementSource, strSourceName, Improvement.ImprovementType.SpecialTab, "enabletab", 0, 0);
                                    break;
                                case "technomancer":
                                    _objCharacter.TechnomancerEnabled = true;
                                    objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "technomancer");
                                    CreateImprovement("Technomancer", objImprovementSource, strSourceName, Improvement.ImprovementType.SpecialTab, "enabletab", 0, 0);
                                    break;
                                case "critter":
                                    _objCharacter.CritterEnabled = true;
                                    objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "critter");
                                    CreateImprovement("Critter", objImprovementSource, strSourceName, Improvement.ImprovementType.SpecialTab, "enabletab", 0, 0);
                                    break;
                                case "initiation":
                                    _objCharacter.InitiationEnabled = true;
                                    objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "initiation");
                                    CreateImprovement("Initiation", objImprovementSource, strSourceName, Improvement.ImprovementType.SpecialTab, "enabletab", 0, 0);
                                    break;
                            }
                        }
                    }

                    // Select Restricted (select Restricted items for Fake Licenses).
                    if (NodeExists(nodBonus, "selectrestricted"))
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
                            blnSuccess = false;
                            _strForcedValue = "";
                            _strLimitSelection = "";
                            return false;
                        }

                        _strSelectedValue = frmPickItem.SelectedItem;
                        if (blnConcatSelectedValue)
                            strSourceName += " (" + _strSelectedValue + ")";

                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strSelectedValue = " + _strSelectedValue);
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "strSourceName = " + strSourceName);

                        // Create the Improvement.
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement(frmPickItem.SelectedItem, objImprovementSource, strSourceName, Improvement.ImprovementType.Restricted, strUnique);
                    }

                    // Select a Skill.
                    if (NodeExists(nodBonus, "selectskill"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "selectrestricted");
                        if (_strForcedValue == "+2 to a Combat Skill")
                            _strForcedValue = "";

                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strSelectedValue = " + _strSelectedValue);
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strForcedValue = " + _strForcedValue);

                        // Display the Select Skill window and record which Skill was selected.
                        frmSelectSkill frmPickSkill = new frmSelectSkill(_objCharacter);
                        if (strFriendlyName != "")
                            frmPickSkill.Description = LanguageManager.Instance.GetString("String_Improvement_SelectSkillNamed").Replace("{0}", strFriendlyName);
                        else
                            frmPickSkill.Description = LanguageManager.Instance.GetString("String_Improvement_SelectSkill");

                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "selectskill = " + nodBonus.SelectSingleNode("selectskill").OuterXml.ToString());
                        if (nodBonus.SelectSingleNode("selectskill").OuterXml.Contains("skillgroup"))
                            frmPickSkill.OnlySkillGroup = nodBonus.SelectSingleNode("selectskill").Attributes["skillgroup"].InnerText;
                        else if (nodBonus.SelectSingleNode("selectskill").OuterXml.Contains("skillcategory"))
                            frmPickSkill.OnlyCategory = nodBonus.SelectSingleNode("selectskill").Attributes["skillcategory"].InnerText;
                        else if (nodBonus.SelectSingleNode("selectskill").OuterXml.Contains("excludecategory"))
                            frmPickSkill.ExcludeCategory = nodBonus.SelectSingleNode("selectskill").Attributes["excludecategory"].InnerText;
                        else if (nodBonus.SelectSingleNode("selectskill").OuterXml.Contains("limittoskill"))
                            frmPickSkill.LimitToSkill = nodBonus.SelectSingleNode("selectskill").Attributes["limittoskill"].InnerText;
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
                            blnSuccess = false;
                            _strForcedValue = "";
                            _strLimitSelection = "";
                            return false;
                        }

                        bool blnAddToRating = false;
                        if (nodBonus["selectskill"]["applytorating"] != null)
                        {
                            if (nodBonus["selectskill"]["applytorating"].InnerText == "yes")
                                blnAddToRating = true;
                        }

                        _strSelectedValue = frmPickSkill.SelectedSkill;
                        if (blnConcatSelectedValue)
                            strSourceName += " (" + _strSelectedValue + ")";

                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strSelectedValue = " + _strSelectedValue);
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "strSourceName = " + strSourceName);

                        // Find the selected Skill.
                        foreach (Skill objSkill in _objCharacter.Skills)
                        {
                            if (frmPickSkill.SelectedSkill.Contains("Exotic Melee Weapon") || frmPickSkill.SelectedSkill.Contains("Exotic Ranged Weapon") || frmPickSkill.SelectedSkill.Contains("Pilot Exotic Vehicle"))
                            {
                                if (objSkill.Name + " (" + objSkill.Specialization + ")" == frmPickSkill.SelectedSkill)
                                {
                                    // We've found the selected Skill.
                                    if (nodBonus.SelectSingleNode("selectskill").InnerXml.Contains("val"))
                                    {
                                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                                        CreateImprovement(objSkill.Name + " (" + objSkill.Specialization + ")", objImprovementSource, strSourceName, Improvement.ImprovementType.Skill, strUnique, ValueToInt(nodBonus["selectskill"]["val"].InnerText, intRating), 1, 0, 0, 0, 0, "", blnAddToRating);
                                    }

                                    if (nodBonus.SelectSingleNode("selectskill").InnerXml.Contains("max"))
                                    {
                                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                                        CreateImprovement(objSkill.Name + " (" + objSkill.Specialization + ")", objImprovementSource, strSourceName, Improvement.ImprovementType.Skill, strUnique, 0, 1, 0, ValueToInt(nodBonus["selectskill"]["max"].InnerText, intRating), 0, 0, "", blnAddToRating);
                                    }
                                }
                            }
                            else
                            {
                                if (objSkill.Name == frmPickSkill.SelectedSkill)
                                {
                                    // We've found the selected Skill.
                                    if (nodBonus.SelectSingleNode("selectskill").InnerXml.Contains("val"))
                                    {
                                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                                        CreateImprovement(objSkill.Name, objImprovementSource, strSourceName, Improvement.ImprovementType.Skill, strUnique, ValueToInt(nodBonus["selectskill"]["val"].InnerText, intRating), 1, 0, 0, 0, 0, "", blnAddToRating);
                                    }

                                    if (nodBonus.SelectSingleNode("selectskill").InnerXml.Contains("max"))
                                    {
                                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                                        CreateImprovement(objSkill.Name, objImprovementSource, strSourceName, Improvement.ImprovementType.Skill, strUnique, 0, 1, 0, ValueToInt(nodBonus["selectskill"]["max"].InnerText, intRating), 0, 0, "", blnAddToRating);
                                    }
                                }
                            }
                        }
                    }

                    // Select a Skill Group.
                    if (NodeExists(nodBonus, "selectskillgroup"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "selectskillgroup");
                        string strExclude = "";
                        if (nodBonus["selectskillgroup"].Attributes["excludecategory"] != null)
                            strExclude = nodBonus["selectskillgroup"].Attributes["excludecategory"].InnerText;

                        frmSelectSkillGroup frmPickSkillGroup = new frmSelectSkillGroup();
                        if (strFriendlyName != "")
                            frmPickSkillGroup.Description = LanguageManager.Instance.GetString("String_Improvement_SelectSkillGroupName").Replace("{0}", strFriendlyName);
                        else
                            frmPickSkillGroup.Description = LanguageManager.Instance.GetString("String_Improvement_SelectSkillGroup");

                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strForcedValue = " + _strForcedValue);
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strLimitSelection = " + _strLimitSelection);

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
                            blnSuccess = false;
                            _strForcedValue = "";
                            _strLimitSelection = "";
                            return false;
                        }

                        bool blnAddToRating = false;
                        if (nodBonus["selectskillgroup"]["applytorating"] != null)
                        {
                            if (nodBonus["selectskillgroup"]["applytorating"].InnerText == "yes")
                                blnAddToRating = true;
                        }

                        _strSelectedValue = frmPickSkillGroup.SelectedSkillGroup;

                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strSelectedValue = " + _strSelectedValue);
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "strSourceName = " + strSourceName);

                        if (nodBonus["selectskillgroup"].SelectSingleNode("bonus") != null)
                        {
                            objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                            CreateImprovement(_strSelectedValue, objImprovementSource, strSourceName, Improvement.ImprovementType.SkillGroup, strUnique, ValueToInt(nodBonus["selectskillgroup"]["bonus"].InnerText, intRating), 1, 0, 0, 0, 0, strExclude, blnAddToRating);
                        }
                        else
                        {
                            objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                            CreateImprovement(_strSelectedValue, objImprovementSource, strSourceName, Improvement.ImprovementType.SkillGroup, strUnique, ValueToInt(nodBonus["selectskillgroup"]["max"].InnerText, intRating), 0, 0, 1, 0, 0, strExclude, blnAddToRating);
                        }
                    }

                    // Select an Attribute.
                    if (NodeExists(nodBonus, "selectattribute"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "selectattribute");
                        // Display the Select Attribute window and record which Skill was selected.
                        frmSelectAttribute frmPickAttribute = new frmSelectAttribute();
                        if (strFriendlyName != "")
                            frmPickAttribute.Description = LanguageManager.Instance.GetString("String_Improvement_SelectAttributeNamed").Replace("{0}", strFriendlyName);
                        else
                            frmPickAttribute.Description = LanguageManager.Instance.GetString("String_Improvement_SelectAttribute");

                        // Add MAG and/or RES to the list of Attributes if they are enabled on the form.
                        if (_objCharacter.MAGEnabled)
                            frmPickAttribute.AddMAG();
                        if (_objCharacter.RESEnabled)
                            frmPickAttribute.AddRES();

                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "selectattribute = " + nodBonus["selectattribute"].OuterXml.ToString());

                        if (nodBonus["selectattribute"].InnerXml.Contains("<attribute>"))
                        {
                            List<string> strValue = new List<string>();
                            foreach (XmlNode objXmlAttribute in nodBonus["selectattribute"].SelectNodes("attribute"))
                                strValue.Add(objXmlAttribute.InnerText);
                            frmPickAttribute.LimitToList(strValue);
                        }

                        if (nodBonus["selectattribute"].InnerXml.Contains("<excludeattribute>"))
                        {
                            List<string> strValue = new List<string>();
                            foreach (XmlNode objXmlAttribute in nodBonus["selectattribute"].SelectNodes("excludeattribute"))
                                strValue.Add(objXmlAttribute.InnerText);
                            frmPickAttribute.RemoveFromList(strValue);
                        }

                        // Check to see if there is only one possible selection because of _strLimitSelection.
                        if (_strForcedValue != "")
                            _strLimitSelection = _strForcedValue;

                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strForcedValue = " + _strForcedValue);
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strLimitSelection = " + _strLimitSelection);

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
                            blnSuccess = false;
                            _strForcedValue = "";
                            _strLimitSelection = "";
                            return false;
                        }

                        _strSelectedValue = frmPickAttribute.SelectedAttribute;
                        if (blnConcatSelectedValue)
                            strSourceName += " (" + _strSelectedValue + ")";

                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strSelectedValue = " + _strSelectedValue);
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "strSourceName = " + strSourceName);

                        // Record the improvement.
                        int intMin = 0;
                        int intAug = 0;
                        int intMax = 0;
                        int intAugMax = 0;

                        // Extract the modifiers.
                        if (nodBonus["selectattribute"].InnerXml.Contains("min"))
                            intMin = ValueToInt(nodBonus["selectattribute"]["min"].InnerXml, intRating);
                        if (nodBonus["selectattribute"].InnerXml.Contains("val"))
                            intAug = ValueToInt(nodBonus["selectattribute"]["val"].InnerXml, intRating);
                        if (nodBonus["selectattribute"].InnerXml.Contains("max"))
                            intMax = ValueToInt(nodBonus["selectattribute"]["max"].InnerXml, intRating);
                        if (nodBonus["selectattribute"].InnerXml.Contains("aug"))
                            intAugMax = ValueToInt(nodBonus["selectattribute"]["aug"].InnerXml, intRating);

                        string strAttribute = frmPickAttribute.SelectedAttribute;

                        if (nodBonus["selectattribute"]["affectbase"] != null)
                            strAttribute += "Base";

                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement(strAttribute, objImprovementSource, strSourceName, Improvement.ImprovementType.Attribute, strUnique, 0, 1, intMin, intMax, intAug, intAugMax);
                    }

                    // Select a Limit.
                    if (NodeExists(nodBonus, "selectlimit"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "selectlimit");
                        // Display the Select Limit window and record which Limit was selected.
                        frmSelectLimit frmPickLimit = new frmSelectLimit();
                        if (strFriendlyName != "")
                            frmPickLimit.Description = LanguageManager.Instance.GetString("String_Improvement_SelectLimitNamed").Replace("{0}", strFriendlyName);
                        else
                            frmPickLimit.Description = LanguageManager.Instance.GetString("String_Improvement_SelectLimit");

                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "selectlimit = " + nodBonus["selectlimit"].OuterXml.ToString());

                        if (nodBonus["selectlimit"].InnerXml.Contains("<limit>"))
                        {
                            List<string> strValue = new List<string>();
                            foreach (XmlNode objXmlAttribute in nodBonus["selectlimit"].SelectNodes("limit"))
                                strValue.Add(objXmlAttribute.InnerText);
                            frmPickLimit.LimitToList(strValue);
                        }

                        if (nodBonus["selectlimit"].InnerXml.Contains("<excludelimit>"))
                        {
                            List<string> strValue = new List<string>();
                            foreach (XmlNode objXmlAttribute in nodBonus["selectlimit"].SelectNodes("excludelimit"))
                                strValue.Add(objXmlAttribute.InnerText);
                            frmPickLimit.RemoveFromList(strValue);
                        }

                        // Check to see if there is only one possible selection because of _strLimitSelection.
                        if (_strForcedValue != "")
                            _strLimitSelection = _strForcedValue;

                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strForcedValue = " + _strForcedValue);
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strLimitSelection = " + _strLimitSelection);

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
                            blnSuccess = false;
                            _strForcedValue = "";
                            _strLimitSelection = "";
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
                        if (nodBonus["selectlimit"].InnerXml.Contains("min"))
                            intMin = ValueToInt(nodBonus["selectlimit"]["min"].InnerXml, intRating);
                        if (nodBonus["selectlimit"].InnerXml.Contains("val"))
                            intAug = ValueToInt(nodBonus["selectlimit"]["val"].InnerXml, intRating);
                        if (nodBonus["selectlimit"].InnerXml.Contains("max"))
                            intMax = ValueToInt(nodBonus["selectlimit"]["max"].InnerXml, intRating);
                        if (nodBonus["selectlimit"].InnerXml.Contains("aug"))
                            intAugMax = ValueToInt(nodBonus["selectlimit"]["aug"].InnerXml, intRating);

                        string strLimit = frmPickLimit.SelectedLimit;

                        if (nodBonus["selectlimit"]["affectbase"] != null)
                            strLimit += "Base";

                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strSelectedValue = " + _strSelectedValue);
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "strSourceName = " + strSourceName);

                        LimitModifier objLimitMod = new LimitModifier(_objCharacter);
                        // string strBonus = nodBonus["limitmodifier"]["value"].InnerText;
                        int intBonus = intAug;
                        string strName = strFriendlyName;
                        TreeNode nodTemp = new TreeNode();

                        Improvement.ImprovementType objType = Improvement.ImprovementType.PhysicalLimit;
                        if (strLimit == "Mental")
                            objType = Improvement.ImprovementType.MentalLimit;
                        else if (strLimit == "Social")
                            objType = Improvement.ImprovementType.SocialLimit;

                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement(strLimit, objImprovementSource, strSourceName, objType, strFriendlyName, intBonus, 0, intMin, intMax, intAug, intAugMax);
                    }

                    // Select an Attribute.
                    if (NodeExists(nodBonus, "swapskillattribute"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "swapskillattribute");
                        // Display the Select Attribute window and record which Skill was selected.
                        frmSelectAttribute frmPickAttribute = new frmSelectAttribute();
                        if (strFriendlyName != "")
                            frmPickAttribute.Description = LanguageManager.Instance.GetString("String_Improvement_SelectAttributeNamed").Replace("{0}", strFriendlyName);
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

                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "swapskillattribute = " + nodBonus["swapskillattribute"].OuterXml.ToString());

                        if (nodBonus["swapskillattribute"].InnerXml.Contains("<attribute>"))
                        {
                            List<string> strLimitValue = new List<string>();
                            foreach (XmlNode objXmlAttribute in nodBonus["swapskillattribute"].SelectNodes("attribute"))
                                strLimitValue.Add(objXmlAttribute.InnerText);
                            frmPickAttribute.LimitToList(strLimitValue);
                        }

                        // Check to see if there is only one possible selection because of _strLimitSelection.
                        if (_strForcedValue != "")
                            _strLimitSelection = _strForcedValue;

                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strForcedValue = " + _strForcedValue);
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strLimitSelection = " + _strLimitSelection);

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
                            blnSuccess = false;
                            _strForcedValue = "";
                            _strLimitSelection = "";
                            return false;
                        }

                        _strSelectedValue = frmPickAttribute.SelectedAttribute;
                        if (blnConcatSelectedValue)
                            strSourceName += " (" + _strSelectedValue + ")";

                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strSelectedValue = " + _strSelectedValue);
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "strSourceName = " + strSourceName);

                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement(frmPickAttribute.SelectedAttribute, objImprovementSource, strSourceName, Improvement.ImprovementType.SwapSkillAttribute, strUnique);
                    }

                    // Select a Spell.
                    if (NodeExists(nodBonus, "selectspell"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "selectspell");
                        // Display the Select Spell window.
                        frmSelectSpell frmPickSpell = new frmSelectSpell(_objCharacter);

                        if (nodBonus["selectspell"].Attributes["category"] != null)
                            frmPickSpell.LimitCategory = nodBonus["selectspell"].Attributes["category"].InnerText;

                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "selectspell = " + nodBonus["selectspell"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strForcedValue = " + _strForcedValue);
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strLimitSelection = " + _strLimitSelection);

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
                            blnSuccess = false;
                            _strForcedValue = "";
                            _strLimitSelection = "";
                            return false;
                        }

                        _strSelectedValue = frmPickSpell.SelectedSpell;
                        if (blnConcatSelectedValue)
                            strSourceName += " (" + _strSelectedValue + ")";

                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strSelectedValue = " + _strSelectedValue);
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "strSourceName = " + strSourceName);

                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement(frmPickSpell.SelectedSpell, objImprovementSource, strSourceName, Improvement.ImprovementType.Text, strUnique);
                    }

                    // Affect a Specific Attribute.
                    if (NodeExists(nodBonus, "specificattribute"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "specificattribute");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "specificattribute = " + nodBonus["specificattribute"].OuterXml.ToString());
                        XmlNodeList objXmlAttributeList = nodBonus.SelectNodes("specificattribute");
                        foreach (XmlNode objXmlAttribute in objXmlAttributeList)
                        {
                            if (objXmlAttribute["name"].InnerText != "ESS")
                            {
                                // Display the Select Attribute window and record which Attribute was selected.
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
                                {
                                    if (objXmlAttribute["max"].InnerText.Contains("-natural"))
                                    {
                                        intMax = Convert.ToInt32(objXmlAttribute["max"].InnerText.Replace("-natural", string.Empty)) - _objCharacter.GetAttribute(objXmlAttribute["name"].InnerText).MetatypeMaximum;
                                    }
                                    else
                                        intMax = ValueToInt(objXmlAttribute["max"].InnerXml, intRating);
                                }
                                if (objXmlAttribute.InnerXml.Contains("aug"))
                                    intAugMax = ValueToInt(objXmlAttribute["aug"].InnerXml, intRating);

                                string strUseUnique = strUnique;
                                if (objXmlAttribute["name"].Attributes["precedence"] != null)
                                    strUseUnique = "precedence" + objXmlAttribute["name"].Attributes["precedence"].InnerText;

                                string strAttribute = objXmlAttribute["name"].InnerText;

                                if (objXmlAttribute["affectbase"] != null)
                                    strAttribute += "Base";

                                objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                                CreateImprovement(strAttribute, objImprovementSource, strSourceName, Improvement.ImprovementType.Attribute, strUseUnique, 0, 1, intMin, intMax, intAug, intAugMax);
                            }
                            else
                            {
                                objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                                CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Essence, "", Convert.ToInt32(objXmlAttribute["val"].InnerText));
                            }
                        }
                    }

                    // Change the maximum number of BP that can be spent on Nuyen.
                    if (NodeExists(nodBonus, "nuyenmaxbp"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "nuyenmaxbp");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "nuyenmaxbp = " + nodBonus["nuyenmaxbp"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.NuyenMaxBP, "", ValueToInt(nodBonus["nuyenmaxbp"].InnerText, intRating));
                    }

                    // Apply a bonus/penalty to physical limit.
                    if (NodeExists(nodBonus, "physicallimit"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "physicallimit");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "physicallimit = " + nodBonus["physicallimit"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.PhysicalLimit, "", ValueToInt(nodBonus["physicallimit"].InnerText, intRating));
                    }

                    // Apply a bonus/penalty to mental limit.
                    if (NodeExists(nodBonus, "mentallimit"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "mentallimit");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "mentallimit = " + nodBonus["mentallimit"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.MentalLimit, "", ValueToInt(nodBonus["mentallimit"].InnerText, intRating));
                    }

                    // Apply a bonus/penalty to social limit.
                    if (NodeExists(nodBonus, "sociallimit"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "sociallimit");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "sociallimit = " + nodBonus["sociallimit"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.SocialLimit, "", ValueToInt(nodBonus["sociallimit"].InnerText, intRating));
                    }

                    // Change the amount of Nuyen the character has at creation time (this can put the character over the amount they're normally allowed).
                    if (NodeExists(nodBonus, "nuyenamt"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "nuyenamt");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "nuyenamt = " + nodBonus["nuyenamt"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Nuyen, strUnique, ValueToInt(nodBonus["nuyenamt"].InnerText, intRating));
                    }

                    // Improve Condition Monitors.
                    if (NodeExists(nodBonus, "conditionmonitor"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "conditionmonitor");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "conditionmonitor = " + nodBonus["conditionmonitor"].OuterXml.ToString());
                        // Physical Condition.
                        if (nodBonus.SelectSingleNode("conditionmonitor").InnerXml.Contains("physical"))
                        {
                            objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement for Physical");
                            CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.PhysicalCM, strUnique, ValueToInt(nodBonus.SelectSingleNode("conditionmonitor")["physical"].InnerText, intRating));
                        }

                        // Stun Condition.
                        if (nodBonus.SelectSingleNode("conditionmonitor").InnerXml.Contains("stun"))
                        {
                            objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement for Stun");
                            CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.StunCM, strUnique, ValueToInt(nodBonus.SelectSingleNode("conditionmonitor")["stun"].InnerText, intRating));
                        }

                        // Condition Monitor Threshold.
                        if (NodeExists(nodBonus.SelectSingleNode("conditionmonitor"), "threshold"))
                        {
                            string strUseUnique = strUnique;
                            if (nodBonus["conditionmonitor"]["threshold"].Attributes["precedence"] != null)
                                strUseUnique = "precedence" + nodBonus["conditionmonitor"]["threshold"].Attributes["precedence"].InnerText;

                            objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement for Threshold");
                            CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.CMThreshold, strUseUnique, ValueToInt(nodBonus.SelectSingleNode("conditionmonitor")["threshold"].InnerText, intRating));
                        }

                        // Condition Monitor Threshold Offset. (Additioal boxes appear before the FIRST Condition Monitor penalty)
                        if (NodeExists(nodBonus.SelectSingleNode("conditionmonitor"), "thresholdoffset"))
                        {
                            string strUseUnique = strUnique;
                            if (nodBonus["conditionmonitor"]["thresholdoffset"].Attributes["precedence"] != null)
                                strUseUnique = "precedence" + nodBonus["conditionmonitor"]["thresholdoffset"].Attributes["precedence"].InnerText;

                            objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement for Threshold Offset");
                            CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.CMThresholdOffset, strUseUnique, ValueToInt(nodBonus.SelectSingleNode("conditionmonitor")["thresholdoffset"].InnerText, intRating));
                        }

                        // Condition Monitor Overflow.
                        if (nodBonus.SelectSingleNode("conditionmonitor").InnerXml.Contains("overflow"))
                        {
                            objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement for Overflow");
                            CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.CMOverflow, strUnique, ValueToInt(nodBonus.SelectSingleNode("conditionmonitor")["overflow"].InnerText, intRating));
                        }
                    }

                    // Improve Living Personal Attributes.
                    if (NodeExists(nodBonus, "livingpersona"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "livingpersona");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "livingpersona = " + nodBonus["livingpersona"].OuterXml.ToString());
                        // Response.
                        if (nodBonus.SelectSingleNode("livingpersona").InnerXml.Contains("response"))
                        {
                            objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement for response");
                            CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.LivingPersonaResponse, strUnique, ValueToInt(nodBonus.SelectSingleNode("livingpersona")["response"].InnerText, intRating));
                        }

                        // Signal.
                        if (nodBonus.SelectSingleNode("livingpersona").InnerXml.Contains("signal"))
                        {
                            objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement for signal");
                            CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.LivingPersonaSignal, strUnique, ValueToInt(nodBonus.SelectSingleNode("livingpersona")["signal"].InnerText, intRating));
                        }

                        // Firewall.
                        if (nodBonus.SelectSingleNode("livingpersona").InnerXml.Contains("firewall"))
                        {
                            objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement for firewall");
                            CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.LivingPersonaFirewall, strUnique, ValueToInt(nodBonus.SelectSingleNode("livingpersona")["firewall"].InnerText, intRating));
                        }

                        // System.
                        if (nodBonus.SelectSingleNode("livingpersona").InnerXml.Contains("system"))
                        {
                            objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement for system");
                            CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.LivingPersonaSystem, strUnique, ValueToInt(nodBonus.SelectSingleNode("livingpersona")["system"].InnerText, intRating));
                        }

                        // Biofeedback Filter.
                        if (nodBonus.SelectSingleNode("livingpersona").InnerXml.Contains("biofeedback"))
                        {
                            objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement for biofeedback");
                            CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.LivingPersonaBiofeedback, strUnique, ValueToInt(nodBonus.SelectSingleNode("livingpersona")["biofeedback"].InnerText, intRating));
                        }
                    }

                    // The Improvement adjusts a specific Skill.
                    if (NodeExists(nodBonus, "specificskill"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "specificskill");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "specificskill = " + nodBonus["specificskill"].OuterXml.ToString());
                        XmlNodeList objXmlSkillList = nodBonus.SelectNodes("specificskill");
                        // Run through each specific Skill since there may be more than one affected.
                        foreach (XmlNode objXmlSkill in objXmlSkillList)
                        {
                            bool blnAddToRating = false;
                            if (objXmlSkill["applytorating"] != null)
                            {
                                if (objXmlSkill["applytorating"].InnerText == "yes")
                                    blnAddToRating = true;
                            }

                            string strUseUnique = strUnique;
                            if (nodBonus["specificskill"].Attributes["precedence"] != null)
                                strUseUnique = "precedence" + nodBonus["specificskill"].Attributes["precedence"].InnerText;

                            // Record the improvement.
                            if (objXmlSkill["bonus"] != null)
                            {
                                objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement for bonus");
                                CreateImprovement(objXmlSkill["name"].InnerText, objImprovementSource, strSourceName, Improvement.ImprovementType.Skill, strUseUnique, ValueToInt(objXmlSkill["bonus"].InnerXml, intRating), 1, 0, 0, 0, 0, "", blnAddToRating);
                            }
                            if (objXmlSkill["max"] != null)
                            {
                                objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement for max");
                                CreateImprovement(objXmlSkill["name"].InnerText, objImprovementSource, strSourceName, Improvement.ImprovementType.Skill, strUseUnique, 0, 1, 0, ValueToInt(objXmlSkill["max"].InnerText, intRating), 0, 0, "", blnAddToRating);
                            }
                        }
                    }

                    // The Improvement adds a martial art
                    if (NodeExists(nodBonus, "martialart"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "martialart");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "martialart = " + nodBonus["martialart"].OuterXml.ToString());
                        XmlDocument _objXmlDocument = XmlManager.Instance.Load("martialarts.xml");
                        XmlNode objXmlArt = _objXmlDocument.SelectSingleNode("/chummer/martialarts/martialart[name = \"" + nodBonus["martialart"].InnerText + "\"]");

                        TreeNode objNode = new TreeNode();
                        MartialArt objMartialArt = new MartialArt(_objCharacter);
                        objMartialArt.Create(objXmlArt, objNode, _objCharacter);
                        objMartialArt.IsQuality = true;
                        _objCharacter.MartialArts.Add(objMartialArt);
                    }

                    // The Improvement adds a limit modifier
                    if (NodeExists(nodBonus, "limitmodifier"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "limitmodifier");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "limitmodifier = " + nodBonus["limitmodifier"].OuterXml.ToString());
                        LimitModifier objLimitMod = new LimitModifier(_objCharacter);
                        string strLimit = nodBonus["limitmodifier"]["limit"].InnerText;
                        string strBonus = nodBonus["limitmodifier"]["value"].InnerText;
                        string strCondition = "";
                        try
                        {
                            strCondition = nodBonus["limitmodifier"]["condition"].InnerText;
                        }
                        catch { }
                        int intBonus = 0;
                        if (strBonus == "Rating")
                            intBonus = intRating;
                        else
                            intBonus = Convert.ToInt32(strBonus);
                        string strName = strFriendlyName;
                        TreeNode nodTemp = new TreeNode();
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement(strLimit, objImprovementSource, strSourceName, Improvement.ImprovementType.LimitModifier, strFriendlyName, intBonus, 0, 0, 0, 0, 0, strCondition);

                    }

                    // The Improvement adjusts a Skill Category.
                    if (NodeExists(nodBonus, "skillcategory"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "skillcategory");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "skillcategory = " + nodBonus["skillcategory"].OuterXml.ToString());
                        XmlNodeList objXmlSkillList = nodBonus.SelectNodes("skillcategory");
                        // Run through each of the Skill Categories since there may be more than one affected.
                        foreach (XmlNode objXmlSkill in objXmlSkillList)
                        {
                            bool blnAddToRating = false;
                            if (objXmlSkill["applytorating"] != null)
                            {
                                if (objXmlSkill["applytorating"].InnerText == "yes")
                                    blnAddToRating = true;
                            }
                            if (objXmlSkill.InnerXml.Contains("exclude"))
                            {
                                objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement - exclude");
                                CreateImprovement(objXmlSkill["name"].InnerText, objImprovementSource, strSourceName, Improvement.ImprovementType.SkillCategory, strUnique, ValueToInt(objXmlSkill["bonus"].InnerXml, intRating), 1, 0, 0, 0, 0, objXmlSkill["exclude"].InnerText, blnAddToRating);
                            }
                            else
                            {
                                objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                                CreateImprovement(objXmlSkill["name"].InnerText, objImprovementSource, strSourceName, Improvement.ImprovementType.SkillCategory, strUnique, ValueToInt(objXmlSkill["bonus"].InnerXml, intRating), 1, 0, 0, 0, 0, "", blnAddToRating);
                            }
                        }
                    }

                    // The Improvement adjusts a Skill Group.
                    if (NodeExists(nodBonus, "skillgroup"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "skillgroup");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "skillgroup = " + nodBonus["skillgroup"].OuterXml.ToString());
                        XmlNodeList objXmlSkillList = nodBonus.SelectNodes("skillgroup");
                        // Run through each of the Skill Groups since there may be more than one affected.
                        foreach (XmlNode objXmlSkill in objXmlSkillList)
                        {
                            bool blnAddToRating = false;
                            if (objXmlSkill["applytorating"] != null)
                            {
                                if (objXmlSkill["applytorating"].InnerText == "yes")
                                    blnAddToRating = true;
                            }
                            if (objXmlSkill.InnerXml.Contains("exclude"))
                            {
                                objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement - exclude");
                                CreateImprovement(objXmlSkill["name"].InnerText, objImprovementSource, strSourceName, Improvement.ImprovementType.SkillGroup, strUnique, ValueToInt(objXmlSkill["bonus"].InnerXml, intRating), 1, 0, 0, 0, 0, objXmlSkill["exclude"].InnerText, blnAddToRating);
                            }
                            else
                            {
                                objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                                CreateImprovement(objXmlSkill["name"].InnerText, objImprovementSource, strSourceName, Improvement.ImprovementType.SkillGroup, strUnique, ValueToInt(objXmlSkill["bonus"].InnerXml, intRating), 1, 0, 0, 0, 0, "", blnAddToRating);
                            }
                        }
                    }

                    // The Improvement adjust Skills with the given Attribute.
                    if (NodeExists(nodBonus, "skillattribute"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "skillattribute");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "skillattribute = " + nodBonus["skillattribute"].OuterXml.ToString());
                        XmlNodeList objXmlSkillList = nodBonus.SelectNodes("skillattribute");
                        // Run through each of the Skill Attributes since there may be more than one affected.
                        foreach (XmlNode objXmlSkill in objXmlSkillList)
                        {
                            string strUseUnique = strUnique;
                            if (objXmlSkill["name"].Attributes["precedence"] != null)
                                strUseUnique = "precedence" + objXmlSkill["name"].Attributes["precedence"].InnerText;

                            bool blnAddToRating = false;
                            if (objXmlSkill["applytorating"] != null)
                            {
                                if (objXmlSkill["applytorating"].InnerText == "yes")
                                    blnAddToRating = true;
                            }
                            if (objXmlSkill.InnerXml.Contains("exclude"))
                            {
                                objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement - exclude");
                                CreateImprovement(objXmlSkill["name"].InnerText, objImprovementSource, strSourceName, Improvement.ImprovementType.SkillAttribute, strUseUnique, ValueToInt(objXmlSkill["bonus"].InnerXml, intRating), 1, 0, 0, 0, 0, objXmlSkill["exclude"].InnerText, blnAddToRating);
                            }
                            else
                            {
                                objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                                CreateImprovement(objXmlSkill["name"].InnerText, objImprovementSource, strSourceName, Improvement.ImprovementType.SkillAttribute, strUseUnique, ValueToInt(objXmlSkill["bonus"].InnerXml, intRating), 1, 0, 0, 0, 0, "", blnAddToRating);
                            }
                        }
                    }

                    // The Improvement comes from Enhanced Articulation (improves Physical Active Skills linked to a Physical Attribute).
                    if (NodeExists(nodBonus, "skillarticulation"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "skillarticulation");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "skillarticulation = " + nodBonus["skillarticulation"].OuterXml.ToString());
                        XmlNode objXmlSkill = nodBonus.SelectSingleNode("skillarticulation");
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.EnhancedArticulation, strUnique, ValueToInt(objXmlSkill["bonus"].InnerText, intRating));
                    }

                    // Check for Armor modifiers.
                    if (NodeExists(nodBonus, "armor"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "armor");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "armor = " + nodBonus["armor"].OuterXml.ToString());
                        XmlNode objXmlArmor = nodBonus.SelectSingleNode("armor");
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Armor, strUnique, ValueToInt(objXmlArmor.InnerText, intRating));
                    }

                    // Check for Reach modifiers.
                    if (NodeExists(nodBonus, "reach"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "reach");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "reach = " + nodBonus["reach"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Reach, strUnique, ValueToInt(nodBonus["reach"].InnerText, intRating));
                    }

                    // Check for Unarmed Damage Value modifiers.
                    if (NodeExists(nodBonus, "unarmeddv"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "unarmeddv");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "unarmeddv = " + nodBonus["unarmeddv"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.UnarmedDV, strUnique, ValueToInt(nodBonus["unarmeddv"].InnerText, intRating));
                    }

                    // Check for Unarmed Damage Value Physical.
                    if (NodeExists(nodBonus, "unarmeddvphysical"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "unarmeddvphysical");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "unarmeddvphysical = " + nodBonus["unarmeddvphysical"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.UnarmedDVPhysical, "");
                    }

                    // Check for Unarmed Armor Penetration.
                    if (NodeExists(nodBonus, "unarmedap"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "unarmedap");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "unarmedap = " + nodBonus["unarmedap"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.UnarmedAP, strUnique, ValueToInt(nodBonus["unarmedap"].InnerText, intRating));
                    }

                    // Check for Initiative modifiers.
                    if (NodeExists(nodBonus, "initiative"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "initiative");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "initiative = " + nodBonus["initiative"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Initiative, strUnique, ValueToInt(nodBonus["initiative"].InnerText, intRating));
                    }

                    // Check for Initiative Pass modifiers. Only the highest one ever applies.
                    if (NodeExists(nodBonus, "initiativepass"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "initiativepass");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "initiativepass = " + nodBonus["initiativepass"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.InitiativePass, "initiativepass", ValueToInt(nodBonus["initiativepass"].InnerText, intRating));
                    }

                    // Check for Initiative Pass modifiers. Only the highest one ever applies.
                    if (NodeExists(nodBonus, "initiativepassadd"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "initiativepassadd");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "initiativepassadd = " + nodBonus["initiativepassadd"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.InitiativePassAdd, strUnique, ValueToInt(nodBonus["initiativepassadd"].InnerText, intRating));
                    }

                    // Check for Matrix Initiative modifiers.
                    if (NodeExists(nodBonus, "matrixinitiative"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "matrixinitiative");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "matrixinitiative = " + nodBonus["matrixinitiative"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.MatrixInitiative, strUnique, ValueToInt(nodBonus["matrixinitiative"].InnerText, intRating));
                    }

                    // Check for Matrix Initiative Pass modifiers.
                    if (NodeExists(nodBonus, "matrixinitiativepass"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "matrixinitiativepass");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "matrixinitiativepass = " + nodBonus["matrixinitiativepass"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.MatrixInitiativePass, "matrixinitiativepass", ValueToInt(nodBonus["matrixinitiativepass"].InnerText, intRating));
                    }

                    // Check for Matrix Initiative Pass modifiers.
                    if (NodeExists(nodBonus, "matrixinitiativepassadd"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "matrixinitiativepassadd");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "matrixinitiativepassadd = " + nodBonus["matrixinitiativepassadd"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.MatrixInitiativePass, strUnique, ValueToInt(nodBonus["matrixinitiativepassadd"].InnerText, intRating));
                    }

                    // Check for Lifestyle cost modifiers.
                    if (NodeExists(nodBonus, "lifestylecost"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "lifestylecost");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "lifestylecost = " + nodBonus["lifestylecost"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.LifestyleCost, strUnique, ValueToInt(nodBonus["lifestylecost"].InnerText, intRating));
                    }

                    // Check for basic Lifestyle cost modifiers.
                    if (NodeExists(nodBonus, "basiclifestylecost"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "basiclifestylecost");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "basiclifestylecost = " + nodBonus["basiclifestylecost"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.BasicLifestyleCost, strUnique, ValueToInt(nodBonus["basiclifestylecost"].InnerText, intRating));
                    }

                    // Check for Genetech Cost modifiers.
                    if (NodeExists(nodBonus, "genetechcostmultiplier"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "genetechcostmultiplier");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "genetechcostmultiplier = " + nodBonus["genetechcostmultiplier"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.GenetechCostMultiplier, strUnique, ValueToInt(nodBonus["genetechcostmultiplier"].InnerText, intRating));
                    }

                    // Check for Genetech: Transgenics Cost modifiers.
                    if (NodeExists(nodBonus, "transgenicsgenetechcost"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "transgenicsgenetechcost");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "transgenicsgenetechcost = " + nodBonus["transgenicsgenetechcost"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.TransgenicsBiowareCost, strUnique, ValueToInt(nodBonus["transgenicsgenetechcost"].InnerText, intRating));
                    }

                    // Check for Basic Bioware Essence Cost modifiers.
                    if (NodeExists(nodBonus, "basicbiowareessmultiplier"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "basicbiowareessmultiplier");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "basicbiowareessmultiplier = " + nodBonus["basicbiowareessmultiplier"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.BasicBiowareEssCost, strUnique, ValueToInt(nodBonus["basicbiowareessmultiplier"].InnerText, intRating));
                    }

                    // Check for Bioware Essence Cost modifiers.
                    if (NodeExists(nodBonus, "biowareessmultiplier"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "biowareessmultiplier");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "biowareessmultiplier = " + nodBonus["biowareessmultiplier"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.BiowareEssCost, strUnique, ValueToInt(nodBonus["biowareessmultiplier"].InnerText, intRating));
                    }

                    // Check for Cybeware Essence Cost modifiers.
                    if (NodeExists(nodBonus, "cyberwareessmultiplier"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "cyberwareessmultiplier");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "cyberwareessmultiplier = " + nodBonus["cyberwareessmultiplier"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.CyberwareEssCost, strUnique, ValueToInt(nodBonus["cyberwareessmultiplier"].InnerText, intRating));
                    }

                    // Check for Uneducated modifiers.
                    if (NodeExists(nodBonus, "uneducated"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "uneducated");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "uneducated = " + nodBonus["uneducated"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Uneducated, strUnique);
                        _objCharacter.Uneducated = true;
                    }

                    // Check for Uncouth modifiers.
                    if (NodeExists(nodBonus, "uncouth"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "uncouth");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "uncouth = " + nodBonus["uncouth"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Uncouth, strUnique);
                        _objCharacter.Uncouth = true;
                    }

                    // Check for Infirm modifiers.
                    if (NodeExists(nodBonus, "infirm"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "infirm");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "infirm = " + nodBonus["infirm"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Infirm, strUnique);
                        _objCharacter.Infirm = true;
                    }

                    // Check for Adept Linguistics.
                    if (NodeExists(nodBonus, "adeptlinguistics"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "adeptlinguistics");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "adeptlinguistics = " + nodBonus["adeptlinguistics"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.AdeptLinguistics, strUnique, 1);
                    }

                    // Check for Weapon Category DV modifiers.
                    if (NodeExists(nodBonus, "weaponcategorydv"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "weaponcategorydv");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "weaponcategorydv = " + nodBonus["weaponcategorydv"].OuterXml.ToString());
                        XmlNodeList objXmlCategoryList = nodBonus.SelectNodes("weaponcategorydv");
                        XmlNode nodWeapon = nodBonus["weaponcategorydv"];

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
                                frmPickCategory.Description = LanguageManager.Instance.GetString("String_Improvement_SelectSkillNamed").Replace("{0}", strFriendlyName);
                            else
                                frmPickCategory.Description = LanguageManager.Instance.GetString("Title_SelectWeaponCategory");

                            objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strForcedValue = " + _strForcedValue);

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
                                blnSuccess = false;
                                _strForcedValue = "";
                                _strLimitSelection = "";
                                return false;
                            }

                            string strSelected = frmPickCategory.SelectedItem;

                            objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "strSelected = " + strSelected);

                            foreach (Power objPower in _objCharacter.Powers)
                            {
                                if (objPower.InternalId == strSourceName)
                                {
                                    objPower.Name = objPower.Name + " (" + strSelected + ")";
                                }
                            }

                            objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                            CreateImprovement(strSelected, objImprovementSource, strSourceName, Improvement.ImprovementType.WeaponCategoryDV, strUnique, ValueToInt(nodWeapon["bonus"].InnerXml, intRating));
                        }
                        else
                        {
                            // Run through each of the Skill Groups since there may be more than one affected.
                            foreach (XmlNode objXmlCategory in objXmlCategoryList)
                            {
                                objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                                CreateImprovement(objXmlCategory["name"].InnerText, objImprovementSource, strSourceName, Improvement.ImprovementType.WeaponCategoryDV, strUnique, ValueToInt(objXmlCategory["bonus"].InnerXml, intRating));
                            }
                        }
                    }

                    // Check for Mentor Spirit bonuses.
                    if (NodeExists(nodBonus, "selectmentorspirit"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "selectmentorspirit");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "selectmentorspirit = " + nodBonus["selectmentorspirit"].OuterXml.ToString());
                        frmSelectMentorSpirit frmPickMentorSpirit = new frmSelectMentorSpirit(_objCharacter);
                        frmPickMentorSpirit.ShowDialog();

                        // Make sure the dialogue window was not canceled.
                        if (frmPickMentorSpirit.DialogResult == DialogResult.Cancel)
                        {
                            Rollback();
                            blnSuccess = false;
                            _strForcedValue = "";
                            _strLimitSelection = "";
                            return blnSuccess;
                        }

                        _strSelectedValue = frmPickMentorSpirit.SelectedMentor;

                        string strHoldValue = _strSelectedValue;
                        if (blnConcatSelectedValue)
                            strSourceName += " (" + _strSelectedValue + ")";

                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strSelectedValue = " + _strSelectedValue);
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "strSourceName = " + strSourceName);

                        if (frmPickMentorSpirit.BonusNode != null)
                        {
                            blnSuccess = CreateImprovements(objImprovementSource, strSourceName, frmPickMentorSpirit.BonusNode, blnConcatSelectedValue, intRating, strFriendlyName);
                            if (!blnSuccess)
                            {
                                Rollback();
                                _strForcedValue = "";
                                _strLimitSelection = "";
                                return blnSuccess;
                            }
                        }

                        if (frmPickMentorSpirit.Choice1BonusNode != null)
                        {
                            objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "frmPickMentorSpirit.Choice1BonusNode = " + frmPickMentorSpirit.Choice1BonusNode.OuterXml.ToString());
                            string strForce = _strForcedValue;
                            if (!frmPickMentorSpirit.Choice1.StartsWith("Adept:") && !frmPickMentorSpirit.Choice1.StartsWith("Magician:"))
                                _strForcedValue = frmPickMentorSpirit.Choice1;
                            else
                                _strForcedValue = "";
                            objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                            blnSuccess = CreateImprovements(objImprovementSource, strSourceName, frmPickMentorSpirit.Choice1BonusNode, blnConcatSelectedValue, intRating, strFriendlyName);
                            if (!blnSuccess)
                            {
                                Rollback();
                                _strForcedValue = "";
                                _strLimitSelection = "";
                                return blnSuccess;
                            }
                            _strForcedValue = strForce;
                            _objCharacter.Improvements.Last().Notes = frmPickMentorSpirit.Choice1;
                        }

                        if (frmPickMentorSpirit.Choice2BonusNode != null)
                        {
                            objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "frmPickMentorSpirit.Choice2BonusNode = " + frmPickMentorSpirit.Choice2BonusNode.OuterXml.ToString());
                            string strForce = _strForcedValue;
                            if (!frmPickMentorSpirit.Choice2.StartsWith("Adept:") && !frmPickMentorSpirit.Choice2.StartsWith("Magician:"))
                                _strForcedValue = frmPickMentorSpirit.Choice2;
                            else
                                _strForcedValue = "";
                            objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                            blnSuccess = CreateImprovements(objImprovementSource, strSourceName, frmPickMentorSpirit.Choice2BonusNode, blnConcatSelectedValue, intRating, strFriendlyName);
                            if (!blnSuccess)
                            {
                                Rollback();
                                _strForcedValue = "";
                                _strLimitSelection = "";
                                return blnSuccess;
                            }
                            _strForcedValue = strForce;
                            _objCharacter.Improvements.Last().Notes = frmPickMentorSpirit.Choice2;
                        }

                        _strSelectedValue = strHoldValue;
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strSelectedValue = " + _strSelectedValue);
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strForcedValue = " + _strForcedValue);
                    }

                    // Check for Paragon bonuses.
                    if (NodeExists(nodBonus, "selectparagon"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "selectparagon");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "selectparagon = " + nodBonus["selectparagon"].OuterXml.ToString());
                        frmSelectMentorSpirit frmPickMentorSpirit = new frmSelectMentorSpirit(_objCharacter);
                        frmPickMentorSpirit.XmlFile = "paragons.xml";
                        frmPickMentorSpirit.ShowDialog();

                        // Make sure the dialogue window was not canceled.
                        if (frmPickMentorSpirit.DialogResult == DialogResult.Cancel)
                        {
                            Rollback();
                            blnSuccess = false;
                            _strForcedValue = "";
                            _strLimitSelection = "";
                            return blnSuccess;
                        }

                        _strSelectedValue = frmPickMentorSpirit.SelectedMentor;
                        string strHoldValue = _strSelectedValue;
                        if (blnConcatSelectedValue)
                            strSourceName += " (" + _strSelectedValue + ")";

                        if (frmPickMentorSpirit.BonusNode != null)
                        {
                            blnSuccess = CreateImprovements(objImprovementSource, strSourceName, frmPickMentorSpirit.BonusNode, blnConcatSelectedValue, intRating, strFriendlyName);
                            if (!blnSuccess)
                            {
                                Rollback();
                                _strForcedValue = "";
                                _strLimitSelection = "";
                                return blnSuccess;
                            }
                        }

                        if (frmPickMentorSpirit.Choice1BonusNode != null)
                        {
                            string strForce = _strForcedValue;
                            _strForcedValue = frmPickMentorSpirit.Choice1;
                            blnSuccess = CreateImprovements(objImprovementSource, strSourceName, frmPickMentorSpirit.Choice1BonusNode, blnConcatSelectedValue, intRating, strFriendlyName);
                            if (!blnSuccess)
                            {
                                Rollback();
                                _strForcedValue = "";
                                _strLimitSelection = "";
                                return blnSuccess;
                            }
                            _strForcedValue = strForce;
                            _objCharacter.Improvements.Last().Notes = frmPickMentorSpirit.Choice1;
                        }

                        if (frmPickMentorSpirit.Choice2BonusNode != null)
                        {
                            string strForce = _strForcedValue;
                            _strForcedValue = frmPickMentorSpirit.Choice2;
                            blnSuccess = CreateImprovements(objImprovementSource, strSourceName, frmPickMentorSpirit.Choice2BonusNode, blnConcatSelectedValue, intRating, strFriendlyName);
                            if (!blnSuccess)
                            {
                                Rollback();
                                _strForcedValue = "";
                                _strLimitSelection = "";
                                return blnSuccess;
                            }
                            _strForcedValue = strForce;
                            _objCharacter.Improvements.Last().Notes = frmPickMentorSpirit.Choice2;
                        }

                        _strSelectedValue = strHoldValue;
                    }

                    // Check for Smartlink bonus.
                    if (NodeExists(nodBonus, "smartlink"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "smartlink");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "smartlink = " + nodBonus["smartlink"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Smartlink, "smartlink");
                    }

                    // Check for Adapsin bonus.
                    if (NodeExists(nodBonus, "adapsin"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "adapsin");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "adapsin = " + nodBonus["adapsin"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Adapsin, "adapsin");
                    }

                    // Check for SoftWeave bonus.
                    if (NodeExists(nodBonus, "softweave"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "softweave");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "softweave = " + nodBonus["softweave"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.SoftWeave, "softweave");
                    }

                    // Check for Sensitive System.
                    if (NodeExists(nodBonus, "sensitivesystem"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "sensitivesystem");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "sensitivesystem = " + nodBonus["sensitivesystem"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.SensitiveSystem, "sensitivesystem");
                    }

                    // Check for Movement Percent.
                    if (NodeExists(nodBonus, "movementpercent"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "movementpercent");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "movementpercent = " + nodBonus["movementpercent"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.MovementPercent, "", ValueToInt(nodBonus["movementpercent"].InnerText, intRating));
                    }

                    // Check for Swim Percent.
                    if (NodeExists(nodBonus, "swimpercent"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "swimpercent");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "swimpercent = " + nodBonus["swimpercent"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.SwimPercent, "", ValueToInt(nodBonus["swimpercent"].InnerText, intRating));
                    }

                    // Check for Fly Percent.
                    if (NodeExists(nodBonus, "flypercent"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "flypercent");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "flypercent = " + nodBonus["flypercent"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.FlyPercent, "", ValueToInt(nodBonus["flypercent"].InnerText, intRating));
                    }

                    // Check for Fly Speed.
                    if (NodeExists(nodBonus, "flyspeed"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "flyspeed");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "flyspeed = " + nodBonus["flyspeed"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.FlySpeed, "", ValueToInt(nodBonus["flyspeed"].InnerText, intRating));
                    }

                    // Check for free Positive Qualities.
                    if (NodeExists(nodBonus, "freepositivequalities"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "freepositivequalities");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "freepositivequalities = " + nodBonus["freepositivequalities"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.FreePositiveQualities, "", ValueToInt(nodBonus["freepositivequalities"].InnerText, intRating));
                    }

                    // Check for free Negative Qualities.
                    if (NodeExists(nodBonus, "freenegativequalities"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "freenegativequalities");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "freenegativequalities = " + nodBonus["freenegativequalities"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.FreeNegativeQualities, "", ValueToInt(nodBonus["freenegativequalities"].InnerText, intRating));
                    }

                    // Check for Select Side.
                    if (NodeExists(nodBonus, "selectside"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "selectside");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "selectside = " + nodBonus["selectside"].OuterXml.ToString());
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
                            blnSuccess = false;
                            _strForcedValue = "";
                            _strLimitSelection = "";
                            return blnSuccess;
                        }

                        _strSelectedValue = frmPickSide.SelectedSide;
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strSelectedValue = " + _strSelectedValue);
                    }

                    // Check for Free Spirit Power Points.
                    if (NodeExists(nodBonus, "freespiritpowerpoints"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "freespiritpowerpoints");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "freespiritpowerpoints = " + nodBonus["freespiritpowerpoints"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.FreeSpiritPowerPoints, "", ValueToInt(nodBonus["freespiritpowerpoints"].InnerText, intRating));
                    }

                    // Check for Adept Power Points.
                    if (NodeExists(nodBonus, "adeptpowerpoints"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "adeptpowerpoints");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "adeptpowerpoints = " + nodBonus["adeptpowerpoints"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.AdeptPowerPoints, "", ValueToInt(nodBonus["adeptpowerpoints"].InnerText, intRating));
                    }

                    // Check for Adept Powers
                    if (NodeExists(nodBonus, "specificpower"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "specificpower");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "specificpower = " + nodBonus["specificpower"].OuterXml.ToString());
                        // If the character isn't an adept or mystic adept, skip the rest of this.
                        if (_objCharacter.AdeptEnabled)
                        {
                            string strSelection = "";
                            _strForcedValue = "";

                            XmlNodeList objXmlPowerList = nodBonus.SelectNodes("specificpower");
                            foreach (XmlNode objXmlSpecificPower in objXmlPowerList)
                            {
                                objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "objXmlSpecificPower = " + objXmlSpecificPower.OuterXml.ToString());

                                string strPowerName = objXmlSpecificPower["name"].InnerText;
                                int intLevels = 0;
                                if (objXmlSpecificPower["val"] != null)
                                    intLevels = Convert.ToInt32(objXmlSpecificPower["val"].InnerText);
                                bool blnFree = false;
                                if (objXmlSpecificPower["free"] != null)
                                    blnFree = (objXmlSpecificPower["free"].InnerText == "yes");

                                string strPowerNameLimit = strPowerName;
                                if (objXmlSpecificPower["selectlimit"] != null)
                                {
                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "selectlimit = " + objXmlSpecificPower["selectlimit"].OuterXml.ToString());
                                    _strForcedValue = "";
                                    // Display the Select Limit window and record which Limit was selected.
                                    frmSelectLimit frmPickLimit = new frmSelectLimit();
                                    if (strFriendlyName != "")
                                        frmPickLimit.Description = LanguageManager.Instance.GetString("String_Improvement_SelectLimitNamed").Replace("{0}", strFriendlyName);
                                    else
                                        frmPickLimit.Description = LanguageManager.Instance.GetString("String_Improvement_SelectLimit");

                                    if (nodBonus["specificpower"]["selectlimit"].InnerXml.Contains("<limit>"))
                                    {
                                        List<string> strValue = new List<string>();
                                        foreach (XmlNode objXmlAttribute in nodBonus["specificpower"]["selectlimit"].SelectNodes("limit"))
                                            strValue.Add(objXmlAttribute.InnerText);
                                        frmPickLimit.LimitToList(strValue);
                                    }

                                    if (nodBonus["specificpower"]["selectlimit"].InnerXml.Contains("<excludelimit>"))
                                    {
                                        List<string> strValue = new List<string>();
                                        foreach (XmlNode objXmlAttribute in nodBonus["specificpower"]["selectlimit"].SelectNodes("excludelimit"))
                                            strValue.Add(objXmlAttribute.InnerText);
                                        frmPickLimit.RemoveFromList(strValue);
                                    }

                                    // Check to see if there is only one possible selection because of _strLimitSelection.
                                    if (_strForcedValue != "")
                                        _strLimitSelection = _strForcedValue;

                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strForcedValue = " + _strForcedValue);
                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strLimitSelection = " + _strLimitSelection);

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
                                        blnSuccess = false;
                                        _strForcedValue = "";
                                        _strLimitSelection = "";
                                        return false;
                                    }

                                    _strSelectedValue = frmPickLimit.SelectedLimit;
                                    strSelection = _strSelectedValue;
                                    _strForcedValue = _strSelectedValue;

                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strSelectedValue = " + _strSelectedValue);
                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "strSelection = " + strSelection);
                                }

                                if (objXmlSpecificPower["selectskill"] != null)
                                {
                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "selectskill = " + objXmlSpecificPower["selectskill"].OuterXml.ToString());
                                    XmlNode nodSkill = nodBonus["specificpower"];
                                    // Display the Select Skill window and record which Skill was selected.
                                    frmSelectSkill frmPickSkill = new frmSelectSkill(_objCharacter);
                                    if (strFriendlyName != "")
                                        frmPickSkill.Description = LanguageManager.Instance.GetString("String_Improvement_SelectSkillNamed").Replace("{0}", strFriendlyName);
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

                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strForcedValue = " + _strForcedValue);
                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strLimitSelection = " + _strLimitSelection);

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
                                        blnSuccess = false;
                                        _strForcedValue = "";
                                        _strLimitSelection = "";
                                        return false;
                                    }

                                    _strSelectedValue = frmPickSkill.SelectedSkill;
                                    _strForcedValue = _strSelectedValue;
                                    strSelection = _strSelectedValue;

                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strForcedValue = " + _strForcedValue);
                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strSelectedValue = " + _strSelectedValue);
                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "strSelection = " + strSelection);
                                }

                                if (objXmlSpecificPower["selecttext"] != null)
                                {
                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "selecttext = " + objXmlSpecificPower["selecttext"].OuterXml.ToString());
                                    frmSelectText frmPickText = new frmSelectText();
                                    frmPickText.Description = LanguageManager.Instance.GetString("String_Improvement_SelectText").Replace("{0}", strFriendlyName);

                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strForcedValue = " + _strForcedValue);
                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strLimitSelection = " + _strLimitSelection);

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
                                        return false;
                                    }

                                    strSelection = frmPickText.SelectedValue;
                                    _strLimitSelection = strSelection;

                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strLimitSelection = " + _strLimitSelection);
                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "strSelection = " + strSelection);
                                }

                                if (objXmlSpecificPower["specificattribute"] != null)
                                {
                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "specificattribute = " + objXmlSpecificPower["specificattribute"].OuterXml.ToString());
                                    strSelection = objXmlSpecificPower["specificattribute"]["name"].InnerText.ToString();
                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "strSelection = " + strSelection);
                                }

                                if (objXmlSpecificPower["selectattribute"] != null)
                                {
                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "selectattribute = " + objXmlSpecificPower["selectattribute"].OuterXml.ToString());
                                    XmlNode nodSkill = objXmlSpecificPower;
                                    if (_strForcedValue.StartsWith("Adept"))
                                        _strForcedValue = "";

                                    // Display the Select Attribute window and record which Attribute was selected.
                                    frmSelectAttribute frmPickAttribute = new frmSelectAttribute();
                                    if (strFriendlyName != "")
                                        frmPickAttribute.Description = LanguageManager.Instance.GetString("String_Improvement_SelectAttributeNamed").Replace("{0}", strFriendlyName);
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

                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strForcedValue = " + _strForcedValue);
                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strLimitSelection = " + _strLimitSelection);

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
                                        blnSuccess = false;
                                        _strForcedValue = "";
                                        _strLimitSelection = "";
                                        return false;
                                    }

                                    _strSelectedValue = frmPickAttribute.SelectedAttribute;
                                    if (blnConcatSelectedValue)
                                        strSourceName += " (" + _strSelectedValue + ")";
                                    strSelection = _strSelectedValue;
                                    _strForcedValue = _strSelectedValue;

                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strSelectedValue = " + _strSelectedValue);
                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "strSourceName = " + strSourceName);
                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strForcedValue = " + _strForcedValue);
                                }

                                // Check if the character already has this power
                                objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "strSelection = " + strSelection);
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
                                    objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Adding Power " + strPowerName);
                                    // If no, add the power and mark it free or give it free levels
                                    objPower = new Power(_objCharacter);
                                    _objCharacter.Powers.Add(objPower);

                                    // Get the Power information
                                    XmlDocument objXmlDocument = new XmlDocument();
                                    objXmlDocument = XmlManager.Instance.Load("powers.xml");
                                    XmlNode objXmlPower = objXmlDocument.SelectSingleNode("/chummer/powers/power[name = \"" + strPowerName + "\"]");
                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "objXmlPower = " + objXmlPower.OuterXml.ToString());

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
                                        if (!CreateImprovements(Improvement.ImprovementSource.Power, objPower.InternalId, objPower.Bonus, false, Convert.ToInt32(objPower.Rating), objPower.DisplayNameShort))
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
                    }

                    // Select a Power.
                    if (NodeExists(nodBonus, "selectpower"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "selectpower");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strSelectedValue = " + _strSelectedValue);
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strForcedValue = " + _strForcedValue);

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
                                        decimal decLevels = Convert.ToDecimal(intRating) / 4;
                                        decLevels = Math.Floor(decLevels / objExistingPower.PointsPerLevel);
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
                            objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "selectpower = " + nodBonus.SelectSingleNode("selectpower").OuterXml.ToString());
                            frmPickPower.ShowDialog();

                            // Make sure the dialogue window was not canceled.
                            if (frmPickPower.DialogResult == DialogResult.Cancel)
                            {
                                Rollback();
                                blnSuccess = false;
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

                            objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strSelectedValue = " + _strSelectedValue);
                            objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "strSourceName = " + strSourceName);

                            XmlNode objBonus = objXmlPower["bonus"];

                            string strPowerNameLimit = _strSelectedValue;
                            if (objBonus != null)
                            {
                                if (objBonus["selectlimit"] != null)
                                {
                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "selectlimit = " + objBonus["selectlimit"].OuterXml.ToString());
                                    _strForcedValue = "";
                                    // Display the Select Limit window and record which Limit was selected.
                                    frmSelectLimit frmPickLimit = new frmSelectLimit();
                                    if (strFriendlyName != "")
                                        frmPickLimit.Description = LanguageManager.Instance.GetString("String_Improvement_SelectLimitNamed").Replace("{0}", strFriendlyName);
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

                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strForcedValue = " + _strForcedValue);
                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strLimitSelection = " + _strLimitSelection);

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
                                        blnSuccess = false;
                                        _strForcedValue = "";
                                        _strLimitSelection = "";
                                        return false;
                                    }

                                    _strSelectedValue = frmPickLimit.SelectedLimit;
                                    strSelection = _strSelectedValue;
                                    _strForcedValue = _strSelectedValue;

                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strSelectedValue = " + _strSelectedValue);
                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "strSelection = " + strSelection);
                                }

                                if (objBonus["selectskill"] != null)
                                {
                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "selectskill = " + objBonus["selectskill"].OuterXml.ToString());
                                    XmlNode nodSkill = objBonus;
                                    // Display the Select Skill window and record which Skill was selected.
                                    frmSelectSkill frmPickSkill = new frmSelectSkill(_objCharacter);
                                    if (strFriendlyName != "")
                                        frmPickSkill.Description = LanguageManager.Instance.GetString("String_Improvement_SelectSkillNamed").Replace("{0}", strFriendlyName);
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

                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strForcedValue = " + _strForcedValue);
                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strLimitSelection = " + _strLimitSelection);

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
                                        blnSuccess = false;
                                        _strForcedValue = "";
                                        _strLimitSelection = "";
                                        return false;
                                    }

                                    _strSelectedValue = frmPickSkill.SelectedSkill;
                                    _strForcedValue = _strSelectedValue;
                                    strSelection = _strSelectedValue;

                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strForcedValue = " + _strForcedValue);
                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strSelectedValue = " + _strSelectedValue);
                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "strSelection = " + strSelection);
                                }

                                if (objBonus["selecttext"] != null)
                                {
                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "selecttext = " + objBonus["selecttext"].OuterXml.ToString());
                                    frmSelectText frmPickText = new frmSelectText();
                                    frmPickText.Description = LanguageManager.Instance.GetString("String_Improvement_SelectText").Replace("{0}", strFriendlyName);

                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strForcedValue = " + _strForcedValue);
                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strLimitSelection = " + _strLimitSelection);

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
                                        return false;
                                    }

                                    strSelection = frmPickText.SelectedValue;
                                    _strLimitSelection = strSelection;

                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strLimitSelection = " + _strLimitSelection);
                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "strSelection = " + strSelection);
                                }

                                if (objBonus["specificattribute"] != null)
                                {
                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "specificattribute = " + objBonus["specificattribute"].OuterXml.ToString());
                                    strSelection = objBonus["specificattribute"]["name"].InnerText.ToString();
                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "strSelection = " + strSelection);
                                }

                                if (objBonus["selectattribute"] != null)
                                {
                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "selectattribute = " + objBonus["selectattribute"].OuterXml.ToString());
                                    XmlNode nodSkill = objBonus;
                                    if (_strForcedValue.StartsWith("Adept"))
                                        _strForcedValue = "";

                                    // Display the Select Attribute window and record which Attribute was selected.
                                    frmSelectAttribute frmPickAttribute = new frmSelectAttribute();
                                    if (strFriendlyName != "")
                                        frmPickAttribute.Description = LanguageManager.Instance.GetString("String_Improvement_SelectAttributeNamed").Replace("{0}", strFriendlyName);
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

                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strForcedValue = " + _strForcedValue);
                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strLimitSelection = " + _strLimitSelection);

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
                                        blnSuccess = false;
                                        _strForcedValue = "";
                                        _strLimitSelection = "";
                                        return false;
                                    }

                                    _strSelectedValue = frmPickAttribute.SelectedAttribute;
                                    if (blnConcatSelectedValue)
                                        strSourceName += " (" + _strSelectedValue + ")";
                                    strSelection = _strSelectedValue;
                                    _strForcedValue = _strSelectedValue;

                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strSelectedValue = " + _strSelectedValue);
                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "strSourceName = " + strSourceName);
                                    objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strForcedValue = " + _strForcedValue);
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
                                    decimal decLevels = Convert.ToDecimal(intRating) / 4;
                                    decLevels = Math.Floor(decLevels / objPower.PointsPerLevel);
                                    objPower.FreeLevels += Convert.ToInt32(decLevels);
                                    objPower.Rating += Convert.ToInt32(decLevels);
                                }
                                objPower.BonusSource = strSourceName;
                            }
                            else
                            {
                                objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Adding Power " + _strSelectedValue);
                                // Get the Power information
                                _objCharacter.Powers.Add(objPower);
                                objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "objXmlPower = " + objXmlPower.OuterXml.ToString());

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
                                    decimal decLevels = Convert.ToDecimal(intRating) / 4;
                                    decLevels = Math.Floor(decLevels / objPower.PointsPerLevel);
                                    objPower.FreeLevels += Convert.ToInt32(decLevels);
                                    if (objPower.Rating < intRating)
                                        objPower.Rating = objPower.FreeLevels;
                                }

                                if (objXmlPower.InnerXml.Contains("bonus"))
                                {
                                    objPower.Bonus = objXmlPower["bonus"];
                                    objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovements");
                                    if (!CreateImprovements(Improvement.ImprovementSource.Power, objPower.InternalId, objPower.Bonus, false, Convert.ToInt32(objPower.Rating), objPower.DisplayNameShort))
                                    {
                                        _objCharacter.Powers.Remove(objPower);
                                    }
                                }
                            }
                        }
                    }

                    // Check for Armor Encumbrance Penalty.
                    if (NodeExists(nodBonus, "armorencumbrancepenalty"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "armorencumbrancepenalty");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "armorencumbrancepenalty = " + nodBonus["armorencumbrancepenalty"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.ArmorEncumbrancePenalty, "", ValueToInt(nodBonus["armorencumbrancepenalty"].InnerText, intRating));
                    }

                    // Check for Initiation.
                    if (NodeExists(nodBonus, "initiation"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "initiation");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "initiation = " + nodBonus["initiation"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Initiation, "", ValueToInt(nodBonus["initiation"].InnerText, intRating));
                        _objCharacter.InitiateGrade += ValueToInt(nodBonus["initiation"].InnerText, intRating);
                    }

                    // Check for Submersion.
                    if (NodeExists(nodBonus, "submersion"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "submersion");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "submersion = " + nodBonus["submersion"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Submersion, "", ValueToInt(nodBonus["submersion"].InnerText, intRating));
                        _objCharacter.SubmersionGrade += ValueToInt(nodBonus["submersion"].InnerText, intRating);
                    }

                    // Check for Skillwires.
                    if (NodeExists(nodBonus, "skillwire"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "skillwire");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "skillwire = " + nodBonus["skillwire"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Skillwire, "", ValueToInt(nodBonus["skillwire"].InnerText, intRating));
                    }

                    // Check for Damage Resistance.
                    if (NodeExists(nodBonus, "damageresistance"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "damageresistance");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "damageresistance = " + nodBonus["damageresistance"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.DamageResistance, "damageresistance", ValueToInt(nodBonus["damageresistance"].InnerText, intRating));
                    }

                    // Check for Restricted Item Count.
                    if (NodeExists(nodBonus, "restricteditemcount"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "restricteditemcount");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "restricteditemcount = " + nodBonus["restricteditemcount"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.RestrictedItemCount, "", ValueToInt(nodBonus["restricteditemcount"].InnerText, intRating));
                    }

                    // Check for Judge Intentions.
                    if (NodeExists(nodBonus, "judgeintentions"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "judgeintentions");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "judgeintentions = " + nodBonus["judgeintentions"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.JudgeIntentions, "", ValueToInt(nodBonus["judgeintentions"].InnerText, intRating));
                    }

                    // Check for Composure.
                    if (NodeExists(nodBonus, "composure"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "composure");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "composure = " + nodBonus["composure"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Composure, "", ValueToInt(nodBonus["composure"].InnerText, intRating));
                    }

                    // Check for Lift and Carry.
                    if (NodeExists(nodBonus, "liftandcarry"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "liftandcarry");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "liftandcarry = " + nodBonus["liftandcarry"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.LiftAndCarry, "", ValueToInt(nodBonus["liftandcarry"].InnerText, intRating));
                    }

                    // Check for Memory.
                    if (NodeExists(nodBonus, "memory"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "memory");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "memory = " + nodBonus["memory"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Memory, "", ValueToInt(nodBonus["memory"].InnerText, intRating));
                    }

                    // Check for Concealability.
                    if (NodeExists(nodBonus, "concealability"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "concealability");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "concealability = " + nodBonus["concealability"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Concealability, "", ValueToInt(nodBonus["concealability"].InnerText, intRating));
                    }

                    // Check for Drain Resistance.
                    if (NodeExists(nodBonus, "drainresist"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "drainresist");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "drainresist = " + nodBonus["drainresist"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.DrainResistance, "", ValueToInt(nodBonus["drainresist"].InnerText, intRating));
                    }

                    // Check for Fading Resistance.
                    if (NodeExists(nodBonus, "fadingresist"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "fadingresist");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "fadingresist = " + nodBonus["fadingresist"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.FadingResistance, "", ValueToInt(nodBonus["fadingresist"].InnerText, intRating));
                    }

                    // Check for Notoriety.
                    if (NodeExists(nodBonus, "notoriety"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "notoriety");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "notoriety = " + nodBonus["notoriety"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.Notoriety, "", ValueToInt(nodBonus["notoriety"].InnerText, intRating));
                    }

                    // Check for Complex Form Limit.
                    if (NodeExists(nodBonus, "complexformlimit"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "complexformlimit");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "complexformlimit = " + nodBonus["complexformlimit"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.ComplexFormLimit, "", ValueToInt(nodBonus["complexformlimit"].InnerText, intRating));
                    }

                    // Check for Spell Limit.
                    if (NodeExists(nodBonus, "spelllimit"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "spelllimit");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "spelllimit = " + nodBonus["spelllimit"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.SpellLimit, "", ValueToInt(nodBonus["spelllimit"].InnerText, intRating));
                    }

                    // Check for Spell Category bonuses.
                    if (NodeExists(nodBonus, "spellcategory"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "spellcategory");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "spellcategory = " + nodBonus["spellcategory"].OuterXml.ToString());
                        XmlNodeList objXmlCategoryList = nodBonus.SelectNodes("spellcategory");

                        // Run through each of the Spell Categories since there may be more than one affected.
                        foreach (XmlNode nodSpellCategory in objXmlCategoryList)
                        {
                            string strUseUnique = strUnique;
                            if (nodSpellCategory["name"].Attributes["precedence"] != null)
                                strUseUnique = "precedence" + nodSpellCategory["name"].Attributes["precedence"].InnerText;

                            objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                            CreateImprovement(nodSpellCategory["name"].InnerText, objImprovementSource, strSourceName, Improvement.ImprovementType.SpellCategory, strUseUnique, ValueToInt(nodSpellCategory["val"].InnerText, intRating));
                        }
                    }

                    // Check for Throwing Range bonuses.
                    if (NodeExists(nodBonus, "throwrange"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "throwrange");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "throwrange = " + nodBonus["throwrange"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.ThrowRange, strUnique, ValueToInt(nodBonus["throwrange"].InnerText, intRating));
                    }

                    // Check for Throwing STR bonuses.
                    if (NodeExists(nodBonus, "throwstr"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "throwstr");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "throwstr = " + nodBonus["throwstr"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.ThrowSTR, strUnique, ValueToInt(nodBonus["throwstr"].InnerText, intRating));
                    }

                    // Check for Skillsoft access.
                    if (NodeExists(nodBonus, "skillsoftaccess"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "skillsoftaccess");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "skillsoftaccess = " + nodBonus["skillsoftaccess"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.SkillsoftAccess, "");
                    }

                    // Check for Quickening Metamagic.
                    if (NodeExists(nodBonus, "quickeningmetamagic"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "quickeningmetamagic");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "quickeningmetamagic = " + nodBonus["quickeningmetamagic"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.QuickeningMetamagic, "");
                    }

                    // Check for ignore Stun CM Penalty.
                    if (NodeExists(nodBonus, "ignorecmpenaltystun"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "ignorecmpenaltystun");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "ignorecmpenaltystun = " + nodBonus["ignorecmpenaltystun"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.IgnoreCMPenaltyStun, "");
                    }

                    // Check for ignore Physical CM Penalty.
                    if (NodeExists(nodBonus, "ignorecmpenaltyphysical"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "ignorecmpenaltyphysical");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "ignorecmpenaltyphysical = " + nodBonus["ignorecmpenaltyphysical"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.IgnoreCMPenaltyPhysical, "");
                    }

                    // Check for a Cyborg Essence which will permanently set the character's ESS to 0.1.
                    if (NodeExists(nodBonus, "cyborgessence"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "cyborgessence");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "cyborgessence = " + nodBonus["cyborgessence"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.CyborgEssence, "");
                    }

                    // Check for Maximum Essence which will permanently modify the character's Maximum Essence value.
                    if (NodeExists(nodBonus, "essencemax"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "essencemax");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "essencemax = " + nodBonus["essencemax"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.EssenceMax, "", ValueToInt(nodBonus["essencemax"].InnerText, intRating));
                    }

                    // Check for Select Sprite.
                    if (NodeExists(nodBonus, "selectsprite"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "selectsprite");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "selectsprite = " + nodBonus["selectsprite"].OuterXml.ToString());
                        XmlDocument objXmlDocument = XmlManager.Instance.Load("critters.xml");
                        XmlNodeList objXmlNodeList = objXmlDocument.SelectNodes("/chummer/metatypes/metatype[contains(category, \"Sprites\")]");
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
                            blnSuccess = false;
                            _strForcedValue = "";
                            _strLimitSelection = "";
                            return blnSuccess;
                        }

                        _strSelectedValue = frmPickItem.SelectedItem;

                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement(frmPickItem.SelectedItem, objImprovementSource, strSourceName, Improvement.ImprovementType.AddSprite, "");
                    }

                    // Check for Black Market Discount.
                    if (NodeExists(nodBonus, "blackmarketdiscount"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "blackmarketdiscount");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "blackmarketdiscount = " + nodBonus["blackmarketdiscount"].OuterXml.ToString());
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement("", objImprovementSource, strSourceName, Improvement.ImprovementType.BlackMarketDiscount, strUnique);
                        _objCharacter.BlackMarket = true;
                    }

                    // Select Weapon (custom entry for things like Spare Clip).
                    if (NodeExists(nodBonus, "selectweapon"))
                    {
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "selectweapon");
                        objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "selectweapon = " + nodBonus["selectweapon"].OuterXml.ToString());
                        string strSelectedValue = "";
                        if (_strForcedValue != "")
                            _strLimitSelection = _strForcedValue;

                        if (_objCharacter == null)
                        {
                            // If the character is null (this is a Vehicle), the user must enter their own string.
                            // Display the Select Item window and record the value that was entered.
                            frmSelectText frmPickText = new frmSelectText();
                            frmPickText.Description = LanguageManager.Instance.GetString("String_Improvement_SelectText").Replace("{0}", strFriendlyName);

                            objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strLimitSelection = " + _strLimitSelection);
                            objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strForcedValue = " + _strForcedValue);

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
                                return false;
                            }

                            _strSelectedValue = frmPickText.SelectedValue;
                            if (blnConcatSelectedValue)
                                strSourceName += " (" + _strSelectedValue + ")";

                            strSelectedValue = frmPickText.SelectedValue;
                            objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "_strSelectedValue = " + _strSelectedValue);
                            objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "strSelectedValue = " + strSelectedValue);
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
                            frmPickItem.Description = LanguageManager.Instance.GetString("String_Improvement_SelectText").Replace("{0}", strFriendlyName);
                            frmPickItem.GeneralItems = lstWeapons;

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
                                blnSuccess = false;
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

                        // Create the Improvement.
                        objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling CreateImprovement");
                        CreateImprovement(strSelectedValue, objImprovementSource, strSourceName, Improvement.ImprovementType.Text, strUnique);
                    }
                }

                // If we've made it this far, everything went OK, so commit the Improvements.
                objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Calling Commit");
                Commit();
                objFunctions.LogWrite(CommonFunctions.LogType.Message, "Chummer.ImprovementManager", "Returned from Commit");
                // Clear the Forced Value and Limit Selection strings once we're done to prevent these from forcing their values on other Improvements.
                _strForcedValue = "";
                _strLimitSelection = "";
            }
            catch (Exception ex)
            {
                objFunctions.LogWrite(CommonFunctions.LogType.Error, "Chummer.ImprovementManager", "ERROR Message = " + ex.Message);
                objFunctions.LogWrite(CommonFunctions.LogType.Error, "Chummer.ImprovementManager", "ERROR Source  = " + ex.Source);
                objFunctions.LogWrite(CommonFunctions.LogType.Error, "Chummer.ImprovementManager", "ERROR Trace   = " + ex.StackTrace.ToString());
                MessageBox.Show("If you're seeing this, an error just occurred that couldn't be handled. If you have the debug log turned on, please email chummerlog.txt (zipped please) to srchummer5@gmail.com. If you don't have the debug log turned on, please turn it on in the Options dialog and try to repeat what you did so the error can be captured. Thank you.");
                Rollback();
                blnSuccess = false;
            }
            objFunctions.LogWrite(CommonFunctions.LogType.Exiting, "Chummer.ImprovementManager", "CreateImprovements");
            return blnSuccess;
		}

		/// <summary>
		/// Remove all of the Improvements for an XML Node.
		/// </summary>
		/// <param name="objImprovementSource">Type of object that granted these Improvements.</param>
		/// <param name="strSourceName">Name of the item that granted these Improvements.</param>
		public void RemoveImprovements(Improvement.ImprovementSource objImprovementSource, string strSourceName)
		{
            objFunctions.LogWrite(CommonFunctions.LogType.Entering, "Chummer.ImprovementManager", "RemoveImprovements");
            objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "objImprovementSource = " + objImprovementSource.ToString());
            objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "strSourceName = " + strSourceName);

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

                // Remove "free" adept powers if any.
                if (objImprovement.ImproveType == Improvement.ImprovementType.AdeptPower)
                {
                    // Load the power from XML.
                    // objImprovement.Notes = name of the mentor spirit choice. Find the power name from here.
                    XmlDocument objXmlMentorDocument = new XmlDocument();
                    objXmlMentorDocument = XmlManager.Instance.Load("mentors.xml");
                    XmlNode objXmlMentorBonus = objXmlMentorDocument.SelectSingleNode("/chummer/mentors/mentor/choices/choice[name = \"" + objImprovement.Notes + "\"]");
                    XmlNodeList objXmlPowerList = objXmlMentorBonus["bonus"].SelectNodes("specificpower");
                    foreach (XmlNode objXmlSpecificPower in objXmlPowerList)
                    {
                        // Get the Power information
                        XmlDocument objXmlDocument = new XmlDocument();
                        objXmlDocument = XmlManager.Instance.Load("powers.xml");

                        string strPowerName = objXmlSpecificPower["name"].InnerText;

                        // Find the power (if it still exists)
                        foreach(Power objPower in _objCharacter.Powers)
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
				if (objImprovement.ImproveType == Improvement.ImprovementType.Attribute && objImprovement.UniqueName == "enableattribute")
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
								if (objCharacterImprovement.ImproveType == Improvement.ImprovementType.Attribute && objCharacterImprovement.UniqueName == "enableattribute" && objCharacterImprovement.ImprovedName == "MAG")
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
								if (objCharacterImprovement.ImproveType == Improvement.ImprovementType.Attribute && objCharacterImprovement.UniqueName == "enableattribute" && objCharacterImprovement.ImprovedName == "RES")
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
									if (objCharacterImprovement.ImproveType == Improvement.ImprovementType.SpecialTab && objCharacterImprovement.UniqueName == "enabletab" && objCharacterImprovement.ImprovedName == "Magician")
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
									if (objCharacterImprovement.ImproveType == Improvement.ImprovementType.SpecialTab && objCharacterImprovement.UniqueName == "enabletab" && objCharacterImprovement.ImprovedName == "Adept")
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
									if (objCharacterImprovement.ImproveType == Improvement.ImprovementType.SpecialTab && objCharacterImprovement.UniqueName == "enabletab" && objCharacterImprovement.ImprovedName == "Technomancer")
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
									if (objCharacterImprovement.ImproveType == Improvement.ImprovementType.SpecialTab && objCharacterImprovement.UniqueName == "enabletab" && objCharacterImprovement.ImprovedName == "Critter")
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
									if (objCharacterImprovement.ImproveType == Improvement.ImprovementType.SpecialTab && objCharacterImprovement.UniqueName == "enabletab" && objCharacterImprovement.ImprovedName == "Initiation")
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

				// Turn off the Infirm flag if it is being removed.
				if (objImprovement.ImproveType == Improvement.ImprovementType.Infirm)
				{
					bool blnFound = false;
					// See if the character has anything else that is granting them access to Infirm.
					foreach (Improvement objCharacterImprovement in _objCharacter.Improvements)
					{
						// Skip items from the current Improvement source.
						if (objCharacterImprovement.SourceName != objImprovement.SourceName)
						{
							if (objCharacterImprovement.ImproveType == Improvement.ImprovementType.Infirm)
							{
								blnFound = true;
								break;
							}
						}
					}

					if (!blnFound)
						_objCharacter.Infirm = false;
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
		public void CreateImprovement(string strImprovedName, Improvement.ImprovementSource objImprovementSource, string strSourceName, Improvement.ImprovementType objImprovementType, string strUnique,
			int intValue = 0, int intRating = 1, int intMinimum = 0, int intMaximum = 0, int intAugmented = 0, int intAugmentedMaximum = 0, string strExclude = "", bool blnAddToRating = false)
		{
            objFunctions.LogWrite(CommonFunctions.LogType.Entering, "Chummer.ImprovementManager", "CreateImprovement");
            objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "strImprovedName = " + strImprovedName);
            objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "objImprovementSource = " + objImprovementSource.ToString());
            objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "strSourceName = " + strSourceName);
            objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "objImprovementType = " + objImprovementType.ToString());
            objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "strUnique = " + strUnique);
            objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "intValue = " + intValue.ToString());
            objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "intRating = " + intRating.ToString());
            objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "intMinimum = " + intMinimum.ToString());
            objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "intMaximum = " + intMaximum.ToString());
            objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "intAugmented = " + intAugmented.ToString());
            objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "intAugmentedMaximum = " + intAugmentedMaximum.ToString());
            objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "strExclude = " + strExclude);
            objFunctions.LogWrite(CommonFunctions.LogType.Content, "Chummer.ImprovementManager", "blnAddToRating = " + blnAddToRating.ToString());
            
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