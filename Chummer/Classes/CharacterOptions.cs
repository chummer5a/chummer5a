using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Chummer.Annotations;
using Chummer.Backend.Attributes.OptionAttributes;
using Chummer.Backend.Attributes.SaveAttributes;
using Chummer.Backend.Options;
// ReSharper disable StringLiteralTypo
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedMember.Global

namespace Chummer
{
	public class CharacterOptions
    {
        public string FileName { get; internal set; }
        #region Default Values
        // Settings.
        //TODO: Im sure they had some effect. Or remove them
        /*
		private bool _blnAllow2ndMaxAttribute;
		private bool _blnAllowAttributePointsOnExceptional;
	    private bool _blnAlternateArmorEncumbrance;
		private bool _blnAlternateComplexFormCost;
		private bool _blnAlternateMatrixAttribute;
	    private bool _blnAutomaticCopyProtection = true;
		private bool _blnAutomaticRegistration = true;
	    private bool _blnCalculateCommlinkResponse = true;
	    private bool _blnEnforceSkillMaximumModifiedRating;
		private bool _blnErgonomicProgramsLimit = true;
	    private bool _blnIgnoreArmorEncumbrance = true;
	    private bool _blnMayBuyQualities;
	    private bool _blnNoSingleArmorEncumbrance;
		private bool _blnPrintArcanaAlternates;
	    private bool _blnPrintLeadershipAlternates;
	    private bool _blnSpecialAttributeKarmaLimit;
	    private bool _blnUseContactPoints;
	    private int _intFreeContactsFlatNumber = 0;
        */

        private readonly XmlDocument _objBookDoc = new XmlDocument();
		private string _strBookXPath = "";

	    #endregion
		// Sourcebook list.

	    #region Initialization, Save, and Load Methods
		public CharacterOptions(string filename)
		{
		    FileName = filename;
		}

		#endregion

		#region Methods
		/// <summary>
		/// Convert a book code into the full name.
		/// </summary>
		/// <param name="strCode">Book code to convert.</param>
		public string BookFromCode(string strCode)
		{
			string strReturn = "";
			XmlNode objXmlBook = _objBookDoc.SelectSingleNode("/chummer/books/book[code = \"" + strCode + "\"]");
			try
			{
				strReturn = objXmlBook["name"].InnerText;
			}
			catch
			{
			}
			return strReturn;
		}

		/// <summary>
		/// Book code (using the translated version if applicable).
		/// </summary>
		/// <param name="strCode">Book code to search for.</param>
		public string LanguageBookShort(string strCode)
		{
			if (strCode == "")
				return "";

			string strReturn = "";
			XmlNode objXmlBook = _objBookDoc.SelectSingleNode("/chummer/books/book[code = \"" + strCode + "\"]");
			try
			{
				if (objXmlBook["altcode"] != null)
					strReturn = objXmlBook["altcode"].InnerText;
				else
					strReturn = strCode;
			}
			catch
			{
			}
			return strReturn;
		}

		/// <summary>
		/// Determine the book's original code by using the alternate code.
		/// </summary>
		/// <param name="strCode">Alternate code to look for.</param>
		public string BookFromAltCode(string strCode)
		{
			if (strCode == "")
				return "";

			XmlNode objXmlBook = _objBookDoc.SelectSingleNode("/chummer/books/book[altcode = \"" + strCode + "\"]");
			if (objXmlBook == null)
				return strCode;
			else
				return objXmlBook["code"].InnerText;
		}

		/// <summary>
		/// Book name (using the translated version if applicable).
		/// </summary>
		/// <param name="strCode">Book code to search for.</param>
		public string LanguageBookLong(string strCode)
		{
			if (strCode == "")
				return "";

			string strReturn = "";
			XmlNode objXmlBook = _objBookDoc.SelectSingleNode("/chummer/books/book[code = \"" + strCode + "\"]");
			try
			{
				if (objXmlBook["translate"] != null)
					strReturn = objXmlBook["translate"].InnerText;
				else
					strReturn = objXmlBook["name"].InnerText;
			}
			catch
			{
			}
			return strReturn;
		}

		/// <summary>
		/// Determine whether or not a given book is in use.
		/// </summary>
		/// <param name="strCode">Book code to search for.</param>
		public bool BookEnabled(string strCode)
		{
		    return Books[strCode];
		}

        /// <summary>
        /// XPath query used to filter items based on the user's selected source books and optional rules.
        /// </summary>
        public string BookXPath(bool excludeHidden = true)
        {
            string strPath = string.Empty;
            if (excludeHidden)
            {
	            strPath = "not(hide)";
            }
			if (string.IsNullOrWhiteSpace(_strBookXPath))
            {
                RecalculateBookXPath();
            }
            if (string.IsNullOrWhiteSpace(strPath))
            {
                strPath = _strBookXPath;
            }
            else
            {
                strPath += $" and {_strBookXPath}";
            }
            if (!GlobalOptions.Instance.Dronemods)
            {
                strPath += " and not(optionaldrone)";
			}
			return strPath;
        }

        /// <summary>
        /// XPath query used to filter items based on the user's selected source books.
        /// </summary>
        public void RecalculateBookXPath()
        {
            StringBuilder strBookXPath = new StringBuilder("(");
            _strBookXPath = string.Empty;

            foreach (string strBook in EnabledBooks())
            {
                if (!string.IsNullOrWhiteSpace(strBook))
                {
                    strBookXPath.Append("source = \"");
                    strBookXPath.Append(strBook);
                    strBookXPath.Append("\" or ");
                }
            }
            if (strBookXPath.Length >= 4)
            {
                strBookXPath.Length -= 4;
                strBookXPath.Append(')');
                _strBookXPath = strBookXPath.ToString();
            }
            else
                _strBookXPath = string.Empty;
        }


        public List<string> BookLinq()
		{
			return EnabledBooks().ToList();
		}
		#endregion

	    #region Character behavour
	    [OptionAttributes("OptionHeader_CharacterOptions")]

	    /// <summary>
	    /// Number of Limbs a standard character has.
	    /// </summary>
	    [SavePropertyAs("limbcount")]
	    
	    public LimbCount LimbCount { get; set; } = LimbCount.All;

	    /// <summary>
	    /// Exclude a particular Limb Slot from count towards the Limb Count.
	    /// </summary>
	    [DisplayIgnore] //TODO: Do something
	    [SavePropertyAs("excludelimbslot")]
	    //TODO: Handler for comboboxes
	    public string ExcludeLimbSlot { get; set; } = "";

        #region Optional Rules
        [OptionAttributes("OptionHeader_CharacterOptions/Display_HouseRules")]

        /// <summary>
        /// Whether or not to require licensing restricted items.
        /// </summary>
        [SavePropertyAs("licenserestricted")]
        public bool LicenseRestricted { get; set; }

	    /// <summary>
		/// Whether or not a Spirit's Maximum Force is based on the character's total MAG.
		/// </summary>
		[SavePropertyAs("spiritforcebasedontotalmag")]
		public bool SpiritForceBasedOnTotalMAG { get; set; }

	    /// <summary>
		/// Whether or not characters may use Initiation/Submersion in Create mode.
		/// </summary>
		[SavePropertyAs("allowinitiationincreatemode")]
		public bool AllowInitiationInCreateMode { get; set; }

        /// <summary>
        /// Whether or not Essence loss only reduces MAG/RES maximum value, not the current value.
        /// </summary>
        [SavePropertyAs("esslossreducesmaximumonly")]
		public bool ESSLossReducesMaximumOnly { get; set; }

        /// <summary>
        /// Allow Cyberware Essence cost discounts.
        /// </summary>
        [SavePropertyAs("allowcyberwareessdiscounts")]
		public bool AllowCyberwareESSDiscounts { get; set; }

	    /// <summary>
		/// Whether or not Armor Degradation is allowed.
		/// </summary>
		[SavePropertyAs("armordegredation")]
		public bool ArmorDegradation { get; set; }

	    /// <summary>
		/// Whether or not the Karma cost for increasing Special Attributes is based on the shown value instead of actual value.
		/// </summary>
		[SavePropertyAs("specialkarmacostbasedonshownvalue")]
		public bool SpecialKarmaCostBasedOnShownValue { get; set; }

        /// <summary>
        /// Whether or not Restricted items have their cost multiplied.
        /// </summary>
        [SavePropertyAs("multiplyrestrictedcost")]
        public bool MultiplyRestrictedCost { get; set; }

        /// <summary>
        /// Constrains the RestrictedCostMultiplier option to only be enabled if the MultiplyRestrictedCost rule is enabled.
        /// </summary>
        [UsedImplicitly]
        private OptionConstraint<CharacterOptions> MultiplyRestrictedCostConstraint { get; } =
            new OptionConstraint<CharacterOptions>(option => option.MultiplyRestrictedCost);

        /// <summary>
        /// Cost multiplier for Restricted items.
        /// </summary>
        [SavePropertyAs("restrictedcostmultiplier")]
		public int RestrictedCostMultiplier { get; set; } = 1;

        /// <summary>
        /// Whether or not Forbidden items have their cost multiplied.
        /// </summary>
        [SavePropertyAs("multiplyforbiddencost")]
        public bool MultiplyForbiddenCost { get; set; }

        /// <summary>
        /// Constrains the ForbiddenCostMultiplier option to only be enabled if the MultiplyForbiddenCost rule is enabled.
        /// </summary>
        [UsedImplicitly]
        private OptionConstraint<CharacterOptions> MultiplyForbiddenCostConstraint { get; } =
            new OptionConstraint<CharacterOptions>(option => option.MultiplyForbiddenCost);
        
        /// <summary>
        /// Cost multiplier for Forbidden items.
        /// </summary>
        [SavePropertyAs("forbiddencostmultiplier")]
		public int ForbiddenCostMultiplier { get; set; } = 1;

        /// <summary>
        /// Whether to use the rules from SR4 to calculate Public Awareness.
        /// </summary>
        [SavePropertyAs("usecalculatedpublicawareness")]
		public bool UseCalculatedPublicAwareness { get; set; }

        /// <summary>
        /// Whether or not to ignore the art requirements from street grimoire.
        /// </summary>
        [SavePropertyAs("ignoreart")]
		public bool IgnoreArtRequirements { get; set; }

	    /// <summary>
		/// Whether or not to use stats from Cyberlegs when calculating movement rates
		/// </summary>
		[SavePropertyAs("cyberlegmovement")]
		public bool CyberlegMovement { get; set; }

        /// <summary>
        /// Whether or not the DroneArmorMultiplier house rule is enabled.
        /// </summary>
        [SavePropertyAs("dronearmormultiplierenabled")]
        public bool DroneArmorMultiplierEnabled { get; set; }

        /// <summary>
        /// Constrains the DroneArmorMultiplier option to only be enabled if the DroneArmorMultiplierEnabled rule is enabled.
        /// </summary>
        [UsedImplicitly]
	    private OptionConstraint<CharacterOptions> DroneArmorConstraint { get; } =
	        new OptionConstraint<CharacterOptions>(option => option.DroneArmorMultiplierEnabled);

	    /// <summary>
		/// The Drone Body multiplier for maximal Armor
		/// </summary>
		[SavePropertyAs("dronearmorflatnumber")]
	    public int DroneArmorMultiplier { get; set; } = 2;
        
	    /// <summary>
		/// Whether or not Capacity limits should be enforced.
		/// </summary>
		[SavePropertyAs("enforcecapacity")]
		public bool EnforceCapacity { get; set; } = true;

	    /// <summary>
		/// Whether or not Recoil modifiers are restricted (AR 148).
		/// </summary>
		//TODO: Check this is the same as what is somewhere in R&G
	    //TODO: Should probably be an inverted option, and moved to house rule
		[SavePropertyAs("restrictrecoil")]
		public bool RestrictRecoil { get; set; } = true;

	    /// <summary>
		/// Whether or not characters are unresicted in the number of points they can invest in Nuyen.
		/// </summary>
		[SavePropertyAs("unrestrictednuyen")]
		public bool UnrestrictedNuyen { get; set; }

	    /// <summary>
		/// Whether or not the user can change the Part of Base Weapon flag for a Weapon Accessory or Mod.
		/// </summary>
		[SavePropertyAs("alloweditpartofbaseweapon")]
		public bool AllowEditPartOfBaseWeapon { get; set; }

	    /// <summary>
		/// Whether or not the user can mark any piece of Bioware as being Transgenic.
		/// </summary>
		[SavePropertyAs("allowcustomtransgenics")]
		public bool AllowCustomTransgenics { get; set; }

	    /// <summary>
		/// Whether or not the user is allowed to break Skill Groups while in Create Mode.
		/// </summary>
		[SavePropertyAs("breakskillgroupsincreatemode")]
		public bool StrictSkillGroupsInCreateMode { get; set; }

	    /// <summary>
		/// Whether or not any Detection Spell can be taken as Extended range version.
		/// </summary>
		[SavePropertyAs("extendanydetectionspell")]
		public bool ExtendAnyDetectionSpell { get; set; }

	    /// <summary>
		/// Whether or not dice rolling is allowed for Skills.
		/// </summary>
		[SavePropertyAs("allowskilldicerolling")]
		public bool AllowSkillDiceRolling { get; set; }

	    /// <summary>
		/// Whether or not cyberlimbs stats are used in attribute calculation
		/// </summary>
		[SavePropertyAs("dontusecyberlimbcalculation")]
		public bool DontUseCyberlimbCalculation { get; set; }

	    /// <summary>
		/// Whether or not Bioware Suites can be added and created.
		/// </summary>
		[SavePropertyAs("allowbiowaresuites")]
		public bool AllowBiowareSuites { get; set; }

	    /// <summary>
		/// House rule: Free Spirits calculate their Power Points based on their MAG instead of EDG.
		/// </summary>
		//TODO: Find out what this is and probably remove it
		[SavePropertyAs("freespiritpowerpointsmag")]
		public bool FreeSpiritPowerPointsMAG { get; set; }

	    /// <summary>
		/// Whether or not Technomancers can select Autosofts as Complex Forms.
		/// </summary>
		[SavePropertyAs("technomancerallowautosoft")]
		public bool TechnomancerAllowAutosoft { get; set; }

        /// <summary>
        /// Split MAG for Mystic Adepts so that they have a separate MAG rating for Adept Powers instead of using the special PP rules for mystic adepts
        /// </summary>
        [DisplayIgnore] //TODO: should actually display
        public bool MysAdeptSecondMAGAttribute { get; set; }
        private bool _blnAllowTechnomancerSchooling;
        private bool _allowFreeGrids;
        private bool _increasedImprovedAbilityMultiplier;

        [OptionAttributes("OptionHeader_CharacterOptions/Display_HouseRules/SR4")]
        /// <summary>
        /// Whether or not the More Lethal Gameplay optional rule is enabled.
        /// </summary>
        //
        [SavePropertyAs("morelethalgameplay")]
        public bool MoreLethalGameplay { get; set; }

        /// <summary>
        /// Whether or not Obsolescent can be removed/upgraded in the same way as Obsolete.
        /// </summary>
        //TODO: Does this still exist?
        [SavePropertyAs("allowobsolescentupgrade")]
        public bool AllowObsolescentUpgrade { get; set; }

        /// <summary>
        /// Whether or not characters in Career Mode should pay double for qualities.
        /// </summary>
        [OptionAttributes("OptionHeader_CharacterOptions/Display_HouseRules/String_Qualities")]
        [SavePropertyAs("dontdoublequalities")]
        public bool DontDoubleQualityPurchases { get; set; }

        /// <summary>
        /// Whether or not characters in Career Mode should pay double for removing Negative Qualities.
        /// </summary>
        [SavePropertyAs("dontdoublequalityrefunds")]
        public bool DontDoubleQualityRefunds { get; set; }

        /// <summary>
        /// Whether or not characters can have more than 25 BP in Positive Qualities.
        /// </summary>
        [SavePropertyAs("exceedpositivequalities")]
        public bool ExceedPositiveQualities { get; set; }

        /// <summary>
        /// Whether or not characters can have more than 25 BP in Negative Qualities.
        /// </summary>
        [SavePropertyAs("exceednegativequalities")]
        public bool ExceedNegativeQualities { get; set; }

        /// <summary>
        /// If true, the character will not receive additional BP from Negative Qualities past the initial 25
        /// </summary>
        [SavePropertyAs("exceednegativequalitieslimit")]
        public bool ExceedNegativeQualitiesLimit { get; set; }

        [OptionAttributes("OptionHeader_CharacterOptions/Display_OptionalRules")]
        public bool DroneMods { get; set; } = false;


        /// <summary>
        /// Allows characters to spend their Karma before Priority Points.
        /// </summary>
        [DisplayIgnore] //TODO: should actually display
        public bool ReverseAttributePriorityOrder { get; set; }

        //Its debateable if this should really be an option per character or globally. Its very much a global option, but you might want to change it
        #region Printing
        /// <summary>
        /// Whether or not all Active Skills with a total score higher than 0 should be printed.
        /// </summary>
        [OptionAttributes("OptionHeader_CharacterOptions/Display_PrintingOptions")]
        [SavePropertyAs("printzeroratingskills")]
        public bool PrintSkillsWithZeroRating { get; set; } = true;

        /// <summary>
        /// Whether or not the Karma and Nueyn Expenses should be printed on the character sheet.
        /// </summary>
        [SavePropertyAs("printexpenses")]
        public bool PrintExpenses { get; set; }

        /// <summary>
        /// Whether or not Notes should be printed.
        /// </summary>
        [SavePropertyAs("printnotes")]
        public bool PrintNotes { get; set; }
        #endregion


        #region Character Creation
        //TODO: HEADER[OptionAttributes("House Rules/Character Creation")]

        /// <summary>
        /// Whether or not the FreeContactsMultiplier house rule is enabled.
        /// </summary>
        [SavePropertyAs("freecontactsmultiplierenabled")]
        public bool FreeContactsMultiplierEnabled { get; set; }

        /// <summary>
        /// Constrains the FreeContactsMultiplier option to only be enabled if the freecontactsmultiplierenabled rule is enabled.
        /// </summary>
        [UsedImplicitly]
        private OptionConstraint<CharacterOptions> ContactsMultiplierConstraint { get; } =
            new OptionConstraint<CharacterOptions>(option => option.FreeContactsMultiplierEnabled);

        /// <summary>
        /// The CHA multiplier to be used with the Free Contacts Option.
        /// </summary>
        [SavePropertyAs("freekarmacontactsmultiplier")]
        public int FreeContactsMultiplier { get; set; } = 3;


        /// <summary>
        /// Whether or not the multiplier for Free Knowledge points are used.
        /// </summary>
        [SavePropertyAs("freekarmaknowledgemultiplierenabled")]
        public bool FreeKnowledgeMultiplierEnabled { get; set; }

        /// <summary>
        /// Constrains the FreeContactsMultiplier option to only be enabled if the freecontactsmultiplierenabled rule is enabled.
        /// </summary>
        [UsedImplicitly]
        private OptionConstraint<CharacterOptions> KnowledgeMultiplierConstraint { get; } =
            new OptionConstraint<CharacterOptions>(option => option.FreeKnowledgeMultiplierEnabled);

        /// <summary>
        /// The INT+LOG multiplier to be used with the Free Knowledge Option.
        /// </summary>
        [SavePropertyAs("freekarmaknowledgemultiplier")]
		public int FreeKnowledgeMultiplier { get; set; } = 2;

	    /// <summary>
		/// Whether or not Metatypes cost Karma.
		/// </summary>
		[SavePropertyAs("metatypecostskarma")]
		public bool MetatypeCostsKarma { get; set; } = true;

	    /// <summary>
		/// Mutiplier for Metatype Karma Costs.
		/// </summary>
		[SavePropertyAs("metatypecostskarmamultiplier")]
		public int MetatypeCostsKarmaMultiplier { get; set; } = 1;

	    /// <summary>
		/// House rule: Treat the Metatype Attribute Minimum as 1 for the purpose of calculating Karma costs.
		/// </summary>
		[SavePropertyAs("alternatemetatypeattributekarma")]
		public bool AlternateMetatypeAttributeKarma { get; set; }

	    /// <summary>
		/// Maximum amount of remaining Karma that is carried over to the character once they are created.
		/// </summary>
		[SavePropertyAs("karmacarryover")]
		public int KarmaCarryover { get; set; } = 7;

	    /// <summary>
		/// Amount of Nuyen gained per Karma spent.
		/// </summary>
		[SavePropertyAs("karmanuyenper")]
		public int NuyenPerBP { get; set; } = 2000;


	    /// <summary>
		/// Whether you benefit from augmented values for contact points.
		/// </summary>
		[SavePropertyAs("usetotalvalueforcontacts")]
		public bool UseTotalValueForFreeContacts { get; set; }

	    /// <summary>
		/// Whether you benefit from augmented values for free knowledge points.
		/// </summary>
		[SavePropertyAs("usetotalvalueforknowledge")]
		public bool UseTotalValueForFreeKnowledge { get; set; }
        #endregion

        #endregion

        #region Unused Rules
        /* These rules have no code references. Some may still be used and were disconnected accidentally. 

        /// <summary>
        /// Whether or not characters can spend skill points to break groups.
        /// </summary>
        [SavePropertyAs("usepointsonbrokengroups")]
        public bool BreakSkillGroupsWithPoints { get; set; }

        /// <summary>
        /// Whether or not total Skill ratings are capped at 20 or 2 x natural Attribute + Rating, whichever is higher.
        /// </summary>
        //[OptionAttributes("Display_CharacterOptions")]
        [SavePropertyAs("capskillrating")]
        public bool CapSkillRating { get; set; }

        /// <summary>
        /// Whether or not Defaulting on a Skill should include any Modifiers.
        /// </summary>
        [SavePropertyAs("skilldefaultingincludesmodifiers")]
        //TODO: Hook this up?
        public bool SkillDefaultingIncludesModifiers { get; set; }

        /// <summary>
        /// Whether or not characters are allowed to put points into a Skill Group again once it is broken and all Ratings are the same.
        /// </summary>
        [SavePropertyAs("allowskillregrouping")]
        public bool AllowSkillRegrouping { get; set; } = true;

        /// <summary>
        /// Whether or not a character's Strength affects Weapon Recoil.
        /// </summary>
        [SavePropertyAs("strengthaffectsrecoil")]
        public bool StrengthAffectsRecoil { get; set; }

        /// <summary>
        /// Whether or not Armor Suit Capacity is in use.
        /// </summary>
        [SavePropertyAs("armorsuitcapacity")]
        public bool ArmorSuitCapacity { get; set; }

	    /// <summary>
		/// Karma cost for Complex Form Skillsofts = Rating x this value.
		/// </summary>
		[SavePropertyAs("karmacomplexformskillsoft")]
		public int KarmaComplexFormSkillsoft { get; set; } = 1;

        */
        #endregion

        #region Karma Costs
        /// <summary>
        /// Karma cost to improve an Attribute = New Rating X this value.
        /// </summary>
        [Header("Character Creation")]
		[OptionAttributes("OptionHeader_CharacterOptions/Display_KarmaCosts")]
		[SavePropertyAs("karmaattribute")]
		public int KarmaAttribute { get; set; } = 5;

	    /// <summary>
		/// Karma cost to purchase a Quality = BP Cost x this value.
		/// </summary>
		[SavePropertyAs("karmaquality")]
		public int KarmaQuality { get; set; } = 1;

	    /// <summary>
		/// Karma cost for a Contact = (Connection + Loyalty) x this value.
		/// </summary>
		[SavePropertyAs("karmacontact")]
		public int KarmaContact { get; set; } = 1;

	    /// <summary>
		/// Karma cost for an Enemy = (Connection + Loyalty) x this value.
		/// </summary>
		[SavePropertyAs("karmaenemy")]
		public int KarmaEnemy { get; set; } = 1;

	    /// <summary>
		/// Karma cost for a Combat Maneuver = this value.
		/// </summary>
		[SavePropertyAs("karmamaneuver")]
		public int KarmaManeuver { get; set; } = 5;

	    #region Skills
		/// <summary>
		/// Karma cost to purchase a Specialization = this value.
		/// </summary>
		[Header("Skills")]
		[SavePropertyAs("karmaspecialization")]
		public int KarmaSpecialization { get; set; } = 7;

	    /// <summary>
		/// Karma cost to purchase a new Knowledge Skill = this value.
		/// </summary>
		[SavePropertyAs("karmanewknowledgeskill")]
		public int KarmaNewKnowledgeSkill { get; set; } = 1;

	    /// <summary>
		/// Karma cost to purchase a new Active Skill = this value.
		/// </summary>
		[SavePropertyAs("karmanewactiveskill")]
		public int KarmaNewActiveSkill { get; set; } = 2;

	    /// <summary>
		/// Karma cost to purchase a new Skill Group = this value.
		/// </summary>
		[SavePropertyAs("karmanewskillgroup")]
		public int KarmaNewSkillGroup { get; set; } = 5;
        private bool _cyberwareRounding;

	    /// <summary>
		/// Karma cost to improve a Knowledge Skill = New Rating x this value.
		/// </summary>
		[SavePropertyAs("karmaimproveknowledgeskill")]
		public int KarmaImproveKnowledgeSkill { get; set; } = 1;

	    /// <summary>
		/// Karma cost to improve an Active Skill = New Rating x this value.
		/// </summary>
		[SavePropertyAs("karmaimproveactiveskill")]
		public int KarmaImproveActiveSkill { get; set; } = 2;

	    /// <summary>
		/// Karma cost to improve a Skill Group = New Rating x this value.
		/// </summary>
		[SavePropertyAs("karmaimproveskillgroup")]
		public int KarmaImproveSkillGroup { get; set; } = 5;

	    #endregion
		#region Magic
		/// <summary>
		/// Karma cost for each Spell = this value.
		/// </summary>
		[Header("Magic")]
		[SavePropertyAs("karmaspell")]
		public int KarmaSpell { get; set; } = 5;

	    /// <summary>
		/// Karma cost for each Enhancement = this value.
		/// </summary>
		[SavePropertyAs("karmaenhancement")]
		public int KarmaEnhancement { get; set; } = 2;

	    /// <summary>
		/// Karma cost for a Spirit = this value.regis
		/// </summary>
		[SavePropertyAs("karmaspirit")]
		public int KarmaSpirit { get; set; } = 1;

	    /// <summary>
		/// Karma cost for a Initiation = 10 + (New Rating x this value).
		/// </summary>
		[SavePropertyAs("karmainitiation")]
		public int KarmaInitiation { get; set; } = 3;

	    /// <summary>
		/// Karma cost for a Metamagic = this value.
		/// </summary>
		[SavePropertyAs("karmametamagic")]
		public int KarmaMetamagic { get; set; } = 15;

	    /// <summary>
		/// Karma cost to join a Group = this value.
		/// </summary>
		[SavePropertyAs("karmajoingroup")]
		public int KarmaJoinGroup { get; set; } = 5;

	    /// <summary>
		/// Karma cost to leave a Group = this value.
		/// </summary>
		[SavePropertyAs("karmaleavegroup")]
		public int KarmaLeaveGroup { get; set; } = 1;

	    /// <summary>
		/// Karma cost for Alchemical Foci.
		/// </summary>
	    [Header("Foci")]
		[SavePropertyAs("karmaalchemicalfocus")]
		public int KarmaAlchemicalFocus { get; set; } = 3;

	    /// <summary>
		/// Karma cost for Banishing Foci.
		/// </summary>
		[SavePropertyAs("karmabanishingfocus")]
		public int KarmaBanishingFocus { get; set; } = 2;

	    /// <summary>
		/// Karma cost for Binding Foci.
		/// </summary>
		[SavePropertyAs("karmabindingfocus")]
		public int KarmaBindingFocus { get; set; } = 2;

	    /// <summary>
		/// Karma cost for Centering Foci.
		/// </summary>
		[SavePropertyAs("karmacenteringfocus")]
		public int KarmaCenteringFocus { get; set; } = 3;

	    /// <summary>
		/// Karma cost for Counterspelling Foci.
		/// </summary>
		[SavePropertyAs("karmacounterspellingfocus")]
		public int KarmaCounterspellingFocus { get; set; } = 2;

	    /// <summary>
		/// Karma cost for Disenchanting Foci.
		/// </summary>
		[SavePropertyAs("karmadisenchantingfocus")]
		public int KarmaDisenchantingFocus { get; set; } = 3;

	    /// <summary>
		/// Karma cost for Flexible Signature Foci.
		/// </summary>
		[SavePropertyAs("karmaflexiblesignaturefocus")]
		public int KarmaFlexibleSignatureFocus { get; set; } = 3;

	    /// <summary>
		/// Karma cost for Masking Foci.
		/// </summary>
		[SavePropertyAs("karmamaskingfocus")]
		public int KarmaMaskingFocus { get; set; } = 3;

	    /// <summary>
		/// Karma cost for Power Foci.
		/// </summary>
		[SavePropertyAs("karmapowerfocus")]
		public int KarmaPowerFocus { get; set; } = 6;

	    /// <summary>
		/// Karma cost for Qi Foci.
		/// </summary>
		[SavePropertyAs("karmaqifocus")]
		public int KarmaQiFocus { get; set; } = 2;

	    /// <summary>
		/// Karma cost for Ritual Spellcasting Foci.
		/// </summary>
		[SavePropertyAs("karmaritualspellcastingfocus")]
		public int KarmaRitualSpellcastingFocus { get; set; } = 2;

	    /// <summary>
		/// Karma cost for Spellcasting Foci.
		/// </summary>
		[SavePropertyAs("karmaspellcastingfocus")]
		public int KarmaSpellcastingFocus { get; set; } = 2;

	    /// <summary>
		/// Karma cost for Spell Shaping Foci.
		/// </summary>
		[SavePropertyAs("karmaspellshapingfocus")]
		public int KarmaSpellShapingFocus { get; set; } = 3;

	    /// <summary>
		/// Karma cost for Summoning Foci.
		/// </summary>
		[SavePropertyAs("karmasummoningfocus")]
		public int KarmaSummoningFocus { get; set; } = 2;

	    /// <summary>
		/// Karma cost for Sustaining Foci.
		/// </summary>
		[SavePropertyAs("karmasustainingfocus")]
		public int KarmaSustainingFocus { get; set; } = 2;

	    /// <summary>
		/// Karma cost for Weapon Foci.
		/// </summary>
		[SavePropertyAs("karmaweaponfocus")]
		public int KarmaWeaponFocus { get; set; } = 3;

	    #endregion
		#region Complex Forms
		/// <summary>
		/// Karma cost for a new Complex Form = this value.
		/// </summary>
		//TODO: HEADER[OptionAttributes("Karma Costs/Complex Forms")]
		[Header("Complex Forms")]
		[SavePropertyAs("karmanewcomplexform")]
		public int KarmaNewComplexForm { get; set; } = 4;

	    /// <summary>
		/// Karma cost to improve a Complex Form = New Rating x this value.
		/// </summary>
		[SavePropertyAs("karmaimprovecomplexform")]
		public int KarmaImproveComplexForm { get; set; } = 1;

	    /// <summary>
		/// Karma cost for Complex Form Options = Rating x this value.
		/// </summary>
		[SavePropertyAs("karmacomplexformoption")]
		public int KarmaComplexFormOption { get; set; } = 2;

	    #endregion
		#endregion

	    #endregion

	    /// <summary>
	    /// Sourcebooks.
	    /// </summary>
	    public Dictionary<string, bool> Books { get; } = GlobalOptions.Instance.SourcebookInfo.ToDictionary(x => x.Code, x => x.Code == "SR5");

	    public IEnumerable<string> EnabledBooks()
	    {
	        foreach (KeyValuePair<string,bool> book in Books)
	        {
	            if (book.Value)
	                yield return book.Key;
	        }
	    }

	    /// <summary>
		/// Setting name.
		/// </summary>
		[DisplayIgnore] //TODO: Do something
		public string Name { get; set; } = "Default Settings";

	    /// <summary>
		/// 
		/// </summary>
		[DisplayIgnore] //TODO: Do something
	    public string RecentImageFolder { get; set; } = "";



        //TODO: things I didn't quite decide where to put
        /// <summary>
        /// Whether or not to allow a 2nd max attribute with Exceptional Attribute
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public bool Allow2ndMaxAttribute { get; set; }


        /// <summary>
        /// Whether or not Stacked Foci can have a combined Force higher than 6.
        /// </summary>
        public bool AllowHigherStackedFoci { get; set; }

        /// <summary>
        /// Whether or not the user is allowed to buy specializations with skill points for skills only bought with karma.
        /// </summary>
        public bool AllowPointBuySpecializationsOnKarmaSkills { get; set; }

        /// <summary>
        /// Whether Life Modules should automatically generate a character background.
        /// </summary>
        public bool AutomaticBackstory { get; set; } = true;

        /// <summary>
        /// House rule: Whether to compensate for the karma cost difference between raising skill ratings and skill groups when increasing the rating of the last skill in the group
        /// </summary>
        public bool CompensateSkillGroupKarmaDifference { get; set; }

        /// <summary>
        /// Karma cost for an Initiation = this value + (New Rating x KarmaInitiation).
        /// </summary>
        public int KarmaInitiationFlat { get; set; } = 10;

        /// <summary>
        /// Karma cost to purchase a Specialization for a knowledge skill = this value.
        /// </summary>
        public int KarmaKnowledgeSpecialization { get; set; } = 7;

        /// <summary>
        /// How much Karma a single Power Point costs for a Mystic Adept.
        /// </summary>
        public int KarmaMysticAdeptPowerPoint { get; set; } = 5;

        /// <summary>
        /// Amount of Nuyen obtained per Karma point.
        /// </summary>
        public int KarmaNuyenPer { get; set; } = 5;

        /// <summary>
        /// Allow Mystic Adepts to increase their power points during career mode
        /// </summary>
        public bool MysAdeptAllowPPCareer { get; set; }


        //TODO: things that I havent sorted nicely yet


        [DisplayIgnore]
        public List<string> CustomDataDirectoryNames { get; } = new List<string>();


        /// <summary>
        /// Only round essence when its value is displayed
        /// </summary>
        public bool DontRoundEssenceInternally { get; set; }


        
       

        /// <summary>
        /// Do Enemies count towards Negative Quality Karma limit in create mode?
        /// </summary>
        public bool EnemyKarmaQualityLimit { get; set; } = true;

        /// <summary>
        /// If true, the karma cost of qualities is doubled after the initial 25.
        /// </summary>
        public bool ExceedPositiveQualitiesCostDoubled { get; set; }

        /// <summary>
        /// Whether Martial Arts grant a free specialisation in a skill.
        /// </summary>
        public bool FreeMartialArtSpecialization { get; set; }

        /// <summary>
        /// Whether or not to ignore the art requirements from street grimoire.
        /// </summary>
        public bool IgnoreArt { get; set; }

        /// <summary>
        /// Whether or not to ignore the limit on Complex Forms in Career mode.
        /// </summary>
        public bool IgnoreComplexFormLimit { get; set; }

        /// <summary>
        /// Karma cost for a new AI Program
        /// </summary>
        public int KarmaNewAIProgram { get; set; } = 5;

        /// <summary>
        /// Karma cost for a new AI Advanced Program
        /// </summary>
        public int KarmaNewAIAdvancedProgram { get; set; } = 8;

        /// <summary>
        /// House Rule: Ignore Armor Encumbrance entirely.
        /// </summary>
        public bool NoArmorEncumbrance { get; set; }

        /// <summary>
        /// Whether or not the Karma and Nuyen Expenses that have a cost of 0 should be printed on the character sheet.
        /// </summary>
        public bool PrintFreeExpenses { get; set; } = true;

        /// <summary>
        /// Whether Spells from Magic Priority can also be spent on power points.
        /// </summary>
        public bool PrioritySpellsAsAdeptPowers { get; set; } = false;

        //TODO: Global option methinks
        /// <summary>
        /// Whether searching in a selection form will limit itself to the current Category that's selected.
        /// </summary>
        public bool SearchInCategoryOnly { get; set; } = true;

        /// <summary>
        /// Whether or not UnarmedAP, UnarmedReach and UnarmedDV Improvements apply to weapons that use the Unarmed Combat skill.
        /// </summary>
        public bool UnarmedImprovementsApplyToWeapons { get; set; }



        /// <summary>
        /// Whether or not characters can spend skill points on broken groups.
        /// </summary>
        public bool UsePointsOnBrokenGroups { get; set; } = false;

        private int _nuyenDecimals = 2;
        /// <summary>
        /// Number of decimal places to round to when diplaying nuyen values.
        /// </summary>
        public int NuyenDecimals
        {
            get
            {
                return _nuyenDecimals;
            }
            set
            {
                _nuyenDecimals = value;
                EssenceFormat = "#,0" + (value > 0 ? ("." + new string('#', value)) : "");
            }
        }

        /// <summary>
        /// Format in which nuyen values should be displayed (does not include nuyen symbol).
        /// </summary>
        [DisplayIgnore]
        public string NuyenFormat { get; private set; } = "";

        private int _essenceDecimals = 2;
        /// <summary>
        /// Number of decimal places to round to when calculating Essence.
        /// </summary>
        [SavePropertyAs("essencedecimals")]
        public int EssenceDecimals
        {
            get { return _essenceDecimals; }
            set
            {
                int intCurrentEssenceDecimals = EssenceDecimals;
                int intNewEssenceDecimals = Math.Max(value, 0);
                if (intNewEssenceDecimals < intCurrentEssenceDecimals)
                {
                    if (intNewEssenceDecimals > 0)
                    {
                        int length = EssenceFormat.Length - (intCurrentEssenceDecimals - intNewEssenceDecimals);
                        if (length < 3) length = 3;
                        EssenceFormat = EssenceFormat.Substring(0, length);
                    }
                    else
                    {
                        int intDecimalPlaces = EssenceFormat.IndexOf('.');
                        if (intDecimalPlaces != -1)
                            EssenceFormat = EssenceFormat.Substring(0, intDecimalPlaces);
                    }
                }
                else if (intNewEssenceDecimals > intCurrentEssenceDecimals)
                {
                    StringBuilder objEssenceFormat = string.IsNullOrEmpty(EssenceFormat) ? new StringBuilder("#,0") : new StringBuilder(EssenceFormat);
                    if (intCurrentEssenceDecimals == 0)
                    {
                        objEssenceFormat.Append(".");
                    }
                    intNewEssenceDecimals -= intCurrentEssenceDecimals;
                    for (int i = 0; i < intNewEssenceDecimals; ++i)
                    {
                        objEssenceFormat.Append("0");
                    }
                    EssenceFormat = objEssenceFormat.ToString();
                }
            }
        }

        /// <summary>
        /// Whether the Improved Ability power (SR5 309) should be capped at 0.5 of current Rating or 1.5 of current Rating. 
        /// </summary>
        public bool IncreasedImprovedAbilityMultiplier
        {
            get => _increasedImprovedAbilityMultiplier;
            set => _increasedImprovedAbilityMultiplier = value;
        }
        /// <summary>
        /// Whether lifestyles will automatically give free grid subscriptions found in (HT)
        /// </summary>
        public bool AllowFreeGrids
        {
            get => _allowFreeGrids;
            set => _allowFreeGrids = value;
        }

        /// <summary>
        /// Whether Technomancers are allowed to use the Schooling discount on their initiations in the same manner as awakened. 
        /// </summary>
        public bool AllowTechnomancerSchooling
        {
            get => _blnAllowTechnomancerSchooling;
            set => _blnAllowTechnomancerSchooling = value;
        }
        /// <summary>
        /// The value by which Specializations add to dicepool. 
        /// </summary>
        public int SpecializationBonus = 2;

        [DisplayIgnore]
        public string EssenceFormat { get; private set; } = "0.00";

        public bool DronemodsMaximumPilot { get; set; }
    }

    public enum LimbCount
    {
        //Hack with 2 purposes. First it fixes sorting order, second it allows us to have 2 enum values with the value 5
        All = 6,
        NoHead = 0x100+5,
        NoTorso = 0x200+5,
        NoHeadNoTorso = 0x300+4
    }

    public static class LimbCountExtensions
    {
        public static int GetNumberOfLimbs(this LimbCount limbCount) => (int)limbCount & 0xff;
    }

    public class CreationOptions
    {
        public CharacterBuildMethod DefaultBuildMethod { get; set; } = CharacterBuildMethod.Priority;
        public int PointCount { get; set; } = 25;
    }
}
