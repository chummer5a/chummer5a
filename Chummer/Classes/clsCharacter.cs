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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Annotations;
using Chummer.Backend;
using Chummer.Backend.Equipment;
using Chummer.Skills;
using System.Reflection;
using Chummer.Backend.Attributes;
using Chummer.Backend.Extensions;
using System.Globalization;

namespace Chummer
{
    public enum CharacterBuildMethod
    {
        Karma = 0,
        Priority = 1,
        SumtoTen = 2,
        LifeModule = 3
    }

    /// <summary>
    /// Class that holds all of the information that makes up a complete Character.
    /// </summary>
    public class Character : INotifyPropertyChanged
    {
        private XmlNode oldSkillsBackup;
        private XmlNode oldSKillGroupBackup;

        private readonly CharacterOptions _objOptions;

        private string _strFileName = string.Empty;
        private string _strSettingsFileName = "default.xml";
        private bool _blnIgnoreRules = false;
        private int _intKarma = 0;
        private int _intTotalKarma = 0;
        private int _intStreetCred = 0;
        private int _intNotoriety = 0;
        private int _intPublicAwareness = 0;
        private int _intBurntStreetCred = 0;
        private decimal _decNuyen = 0;
        private decimal _decStartingNuyen = 0;
        private int _intMaxAvail = 12;
        private decimal _decEssenceAtSpecialStart = 6.0m;
        private int _intSpecial = 0;
        private int _intTotalSpecial = 0;
        private int _intAttributes = 0;
        private int _intTotalAttributes = 0;
        private int _intSpellLimit = 0;
        private int _intCFPLimit = 0;
        private int _intAINormalProgramLimit = 0;
        private int _intAIAdvancedProgramLimit = 0;
        private int _intContactPoints = 0;
        private int _intContactPointsUsed = 0;
        private int _intMetageneticLimit = 0;
        private int _intRedlinerBonus = 0;

        // General character info.
        private string _strName = string.Empty;
        private List<string> _lstMugshots = new List<string>();
        private int _intMainMugshotIndex = 0;
        private string _strSex = string.Empty;
        private string _strAge = string.Empty;
        private string _strEyes = string.Empty;
        private string _strHeight = string.Empty;
        private string _strWeight = string.Empty;
        private string _strSkin = string.Empty;
        private string _strHair = string.Empty;
        private string _strDescription = string.Empty;
        private string _strBackground = string.Empty;
        private string _strConcept = string.Empty;
        private string _strNotes = string.Empty;
        private string _strAlias = string.Empty;
        private string _strPlayerName = string.Empty;
        private string _strGameNotes = string.Empty;
        private string _strPrimaryArm = "Right";
        public static string[] LimbStrings = { "skull", "torso", "arm", "leg" };

        // AI Home Node
        private Commlink _objHomeNodeCommlink = null;
        private Vehicle _objHomeNodeVehicle = null;

        // If true, the Character creation has been finalized and is maintained through Karma.
        private bool _blnCreated = false;

        // Build Points
        private int _intSumtoTen = 10;
        private int _intBuildPoints = 800;
        private decimal _decNuyenMaximumBP = 50;
        private decimal _decNuyenBP = 0;
        private int _intBuildKarma = 0;
        private int _intAdeptWayDiscount = 0;
        private int _intGameplayOptionQualityLimit = 25;
        private CharacterBuildMethod _objBuildMethod = CharacterBuildMethod.Karma;

        // Metatype Information.
        private string _strMetatype = string.Empty;
        private string _strMetavariant = string.Empty;
        private string _strMetatypeCategory = string.Empty;
        private string _strMovement = string.Empty;
        private string _strWalk = string.Empty;
        private string _strRun = string.Empty;
        private string _strSprint = string.Empty;
        private string _strWalkAlt = string.Empty;
        private string _strRunAlt = string.Empty;
        private string _strSprintAlt = string.Empty;
        private int _intMetatypeBP = 0;

        // Special Flags.

        private bool _blnAdeptEnabled = false;
        private bool _blnMagicianEnabled = false;
        private bool _blnTechnomancerEnabled = false;
        private bool _blnAdvancedProgramsEnabled = false;
        private bool _blnCyberwareDisabled = false;
        private bool _blnInitiationEnabled = false;
        private bool _blnCritterEnabled = false;
        private bool _blnIsCritter = false;
        private bool _blnPossessed = false;
        private bool _blnBlackMarketDiscount = false;
        private bool _blnFriendsInHighPlaces = false;
        private bool _blnExCon = false;
        private bool _blnRestrictedGear = false;
        private bool _blnOverclocker = false;
        private bool _blnMadeMan = false;
        private bool _blnLightningReflexes = false;
        private bool _blnFame = false;
        private bool _blnBornRich = false;
        private bool _blnErased = false;
		private int _intTrustFund = 0;
		private decimal _decPrototypeTranshuman = 0m;
        private bool _blnMAGEnabled = false;
        private bool _blnRESEnabled = false;
        private bool _blnDEPEnabled = false;
        private bool _blnGroupMember = false;
        private string _strGroupName = string.Empty;
        private string _strGroupNotes = string.Empty;
        private int _intInitiateGrade = 0;
        private int _intSubmersionGrade = 0;

        // Pseudo-Attributes use for Mystic Adepts.
        private int _intMAGMagician = 0;
        private int _intMAGAdept = 0;

        // Magic Tradition.
        private string _strMagicTradition = string.Empty;
        private string _strTraditionDrain = string.Empty;
        private string _strTraditionName = string.Empty;
        private string _strSpiritCombat = string.Empty;
		private string _strSpiritDetection = string.Empty;
        private string _strSpiritHealth = string.Empty;
        private string _strSpiritIllusion = string.Empty;
        private string _strSpiritManipulation = string.Empty;
        // Technomancer Stream.
        private string _strTechnomancerStream = string.Empty;
        private string _strTechnomancerFading = "RES + WIL";

        // Condition Monitor Progress.
        private int _intPhysicalCMFilled = 0;
        private int _intStunCMFilled = 0;

        // Priority Selections.
        private string _strGameplayOption = string.Empty;
        private string _strPriorityMetatype = string.Empty;
        private string _strPriorityAttributes = string.Empty;
        private string _strPrioritySpecial = string.Empty;
        private string _strPrioritySkills = string.Empty;
        private string _strPriorityResources = string.Empty;
        private string _strPriorityTalent = string.Empty;
        private List<string> _lstPrioritySkills = new List<string>();
        private decimal _decMaxNuyen = 0;
        private int _intMaxKarma = 0;
        private int _intContactMultiplier = 0;

        // Lists.
        private List<string> _lstSources = new List<string>();
        private List<string> _lstCustomDataDirectoryNames = new List<string>();
        private List<Improvement> _lstImprovements = new List<Improvement>();
        private List<Contact> _lstContacts = new List<Contact>();
        private List<Spirit> _lstSpirits = new List<Spirit>();
        private List<Spell> _lstSpells = new List<Spell>();
        private List<Focus> _lstFoci = new List<Focus>();
        private List<StackedFocus> _lstStackedFoci = new List<StackedFocus>();
        private BindingList<Power> _lstPowers = new BindingList<Power>();
        private List<ComplexForm> _lstComplexForms = new List<ComplexForm>();
        private List<AIProgram> _lstAIPrograms = new List<AIProgram>();
        private List<MartialArt> _lstMartialArts = new List<MartialArt>();
        private List<MartialArtManeuver> _lstMartialArtManeuvers = new List<MartialArtManeuver>();
        private List<LimitModifier> _lstLimitModifiers = new List<LimitModifier>();
        private List<Armor> _lstArmor = new List<Armor>();
        private List<Cyberware> _lstCyberware = new List<Cyberware>();
        private List<Weapon> _lstWeapons = new List<Weapon>();
        private List<Quality> _lstQualities = new List<Quality>();
        private List<LifestyleQuality> _lstLifestyleQualities = new List<LifestyleQuality>();
        private List<Lifestyle> _lstLifestyles = new List<Lifestyle>();
        private List<Gear> _lstGear = new List<Gear>();
        private List<Vehicle> _lstVehicles = new List<Vehicle>();
        private List<Metamagic> _lstMetamagics = new List<Metamagic>();
        private List<Art> _lstArts = new List<Art>();
        private List<Enhancement> _lstEnhancements = new List<Enhancement>();
        private List<ExpenseLogEntry> _lstExpenseLog = new List<ExpenseLogEntry>();
        private List<CritterPower> _lstCritterPowers = new List<CritterPower>();
        private List<InitiationGrade> _lstInitiationGrades = new List<InitiationGrade>();
        private List<string> _lstOldQualities = new List<string>();
        private List<string> _lstLocations = new List<string>();
        private List<string> _lstArmorBundles = new List<string>();
        private List<string> _lstWeaponLocations = new List<string>();
        private List<string> _lstImprovementGroups = new List<string>();
        private List<CalendarWeek> _lstCalendar = new List<CalendarWeek>();
        //private List<LifeModule> _lstLifeModules = new List<LifeModule>();

        // Character Version
        private string _strVersionCreated = Application.ProductVersion.Replace("0.0.", string.Empty);
        Version _verSavedVersion = new Version();
        // Events.
        public Action<object> HomeNodeChanged;
        public Action<object> AdeptTabEnabledChanged;
        public Action<object> AmbidextrousChanged;
        public Action<object> CritterTabEnabledChanged;
        public Action<object> MAGEnabledChanged;
        public Action<object> BlackMarketEnabledChanged;
        public Action<object> BornRichChanged;
        public Action<object> CharacterNameChanged;
        public Action<object> ErasedChanged;
        public Action<object> ExConChanged;
        public Action<object> FameChanged;
        public Action<object> FriendsInHighPlacesChanged;
        public Action<object> InitiationTabEnabledChanged;
        public Action<object> LightningReflexesChanged;
        public Action<object> MadeManChanged;
        public Action<object> MagicianTabEnabledChanged;
        public Action<object> OverclockerChanged;
        public Action<object> PrototypeTranshumanChanged;
        public Action<object> RESEnabledChanged;
        public Action<object> DEPEnabledChanged;
        public Action<object> RestrictedGearChanged;
        public Action<object> TechnomancerTabEnabledChanged;
        public Action<object> AdvancedProgramsTabEnabledChanged;
        public Action<object> CyberwareTabDisabledChanged;
        public Action<object> TrustFundChanged;

        private frmViewer _frmPrintView;

        #region Initialization, Save, Load, Print, and Reset Methods
        /// <summary>
        /// Character.
        /// </summary>
        public Character()
        {
			_objOptions = new CharacterOptions(this);
			AttributeSection = new AttributeSection(this);
			AttributeSection.Reset();
			SkillsSection = new SkillsSection(this);
			SkillsSection.Reset();
        }

	    public AttributeSection AttributeSection { get; set; }

	    /// <summary>
        /// Save the Character to an XML file.
        /// </summary>
        public void Save(string strFileName = "")
        {
            if (string.IsNullOrWhiteSpace(strFileName))
            {
                strFileName = _strFileName;
            }
            MemoryStream objStream = new MemoryStream();
            XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.UTF8)
            {
                Formatting = Formatting.Indented,
                Indentation = 1,
                IndentChar = '\t'
            };
            _lstSources.Clear();
            objWriter.WriteStartDocument();

            // <character>
            objWriter.WriteStartElement("character");

            // <createdversion />
            objWriter.WriteElementString("createdversion", _strVersionCreated);
            // <appversion />
            objWriter.WriteElementString("appversion", Application.ProductVersion.Replace("0.0.", string.Empty));
            // <gameedition />
            objWriter.WriteElementString("gameedition", "SR5");

            // <settings />
            objWriter.WriteElementString("settings", _strSettingsFileName);

            // <metatype />
            objWriter.WriteElementString("metatype", _strMetatype);
            // <metatypebp />
            objWriter.WriteElementString("metatypebp", _intMetatypeBP.ToString());
            // <metavariant />
            objWriter.WriteElementString("metavariant", _strMetavariant);
            // <metatypecategory />
            objWriter.WriteElementString("metatypecategory", _strMetatypeCategory);
            // <metageneticlimit />
            objWriter.WriteElementString("metageneticlimit", _intMetageneticLimit.ToString());
            // <movement />
            objWriter.WriteElementString("movement", _strMovement);
            // <walk />
            objWriter.WriteElementString("walk", _strWalk);
            // <run />
            objWriter.WriteElementString("run", _strRun);
            // <sprint />
            objWriter.WriteElementString("sprint", _strSprint);
            // <walk />
            objWriter.WriteElementString("walkalt", _strWalk);
            // <run />
            objWriter.WriteElementString("runalt", _strRun);
            // <sprint />
            objWriter.WriteElementString("sprintalt", _strSprint);

            // <prioritymetatype />
            objWriter.WriteElementString("prioritymetatype", _strPriorityMetatype);
            // <priorityattributes />
            objWriter.WriteElementString("priorityattributes", _strPriorityAttributes);
            // <priorityspecial />
            objWriter.WriteElementString("priorityspecial", _strPrioritySpecial);
            // <priorityskills />
            objWriter.WriteElementString("priorityskills", _strPrioritySkills);
            // <priorityresources />
            objWriter.WriteElementString("priorityresources", _strPriorityResources);
            // <priorityresources />
            objWriter.WriteElementString("prioritytalent", _strPriorityTalent);
            // <priorityskills >
            objWriter.WriteStartElement("priorityskills");
            foreach (string strSkill in _lstPrioritySkills)
            {
                objWriter.WriteElementString("priorityskill", strSkill);
            }
            // </priorityskills>
            objWriter.WriteEndElement();

            // <essenceatspecialstart />
            objWriter.WriteElementString("essenceatspecialstart", _decEssenceAtSpecialStart.ToString(GlobalOptions.InvariantCultureInfo));

            // <name />
            objWriter.WriteElementString("name", _strName);
            // <mugshot>
            objWriter.WriteElementString("mainmugshotindex", MainMugshotIndex.ToString());
            objWriter.WriteStartElement("mugshots");
            foreach (string strMugshot in _lstMugshots)
            {
                objWriter.WriteElementString("mugshot", strMugshot);
            }
            // </mugshot>
            objWriter.WriteEndElement();

            // <sex />
            objWriter.WriteElementString("sex", _strSex);
            // <age />
            objWriter.WriteElementString("age", _strAge);
            // <eyes />
            objWriter.WriteElementString("eyes", _strEyes);
            // <height />
            objWriter.WriteElementString("height", _strHeight);
            // <weight />
            objWriter.WriteElementString("weight", _strWeight);
            // <skin />
            objWriter.WriteElementString("skin", _strSkin);
            // <hair />
            objWriter.WriteElementString("hair", _strHair);
            // <description />
            objWriter.WriteElementString("description", _strDescription);
            // <background />
            objWriter.WriteElementString("background", _strBackground);
            // <concept />
            objWriter.WriteElementString("concept", _strConcept);
            // <notes />
            objWriter.WriteElementString("notes", _strNotes);
            // <alias />
            objWriter.WriteElementString("alias", _strAlias);
            // <playername />
            objWriter.WriteElementString("playername", _strPlayerName);
            // <gamenotes />
            objWriter.WriteElementString("gamenotes", _strGameNotes);
            objWriter.WriteElementString("primaryarm", _strPrimaryArm);

            // <ignorerules />
            if (_blnIgnoreRules)
                objWriter.WriteElementString("ignorerules", _blnIgnoreRules.ToString());
            // <iscritter />
            if (_blnIsCritter)
                objWriter.WriteElementString("iscritter", _blnIsCritter.ToString());
            if (_blnPossessed)
                objWriter.WriteElementString("possessed", _blnPossessed.ToString());
            if (_intMetageneticLimit > 0)
                objWriter.WriteElementString("metageneticlimit", _intMetageneticLimit.ToString());
            // <karma />
            objWriter.WriteElementString("karma", _intKarma.ToString());
            // <totalkarma />
            objWriter.WriteElementString("totalkarma", CareerKarma.ToString());
            // <special />
            objWriter.WriteElementString("special", _intSpecial.ToString());
            // <totalspecial />
            objWriter.WriteElementString("totalspecial", _intTotalSpecial.ToString());
            // <totalattributes />
            objWriter.WriteElementString("totalattributes", _intTotalAttributes.ToString());
            // <contactpoints />
            objWriter.WriteElementString("contactpoints", _intContactPoints.ToString());
            // <contactpoints />
            objWriter.WriteElementString("contactpointsused", _intContactPointsUsed.ToString());
            // <spelllimit />
            objWriter.WriteElementString("spelllimit", _intSpellLimit.ToString());
            // <cfplimit />
            objWriter.WriteElementString("cfplimit", _intCFPLimit.ToString());
            // <totalaiprogramlimit />
            objWriter.WriteElementString("ainormalprogramlimit", _intAINormalProgramLimit.ToString());
            // <aiadvancedprogramlimit />
            objWriter.WriteElementString("aiadvancedprogramlimit", _intAIAdvancedProgramLimit.ToString());
            // <streetcred />
            objWriter.WriteElementString("streetcred", _intStreetCred.ToString());
            // <notoriety />
            objWriter.WriteElementString("notoriety", _intNotoriety.ToString());
            // <publicaware />
            objWriter.WriteElementString("publicawareness", _intPublicAwareness.ToString());
            // <burntstreetcred />
            objWriter.WriteElementString("burntstreetcred", _intBurntStreetCred.ToString());
            // <created />
            objWriter.WriteElementString("created", _blnCreated.ToString());
            // <maxavail />
            objWriter.WriteElementString("maxavail", _intMaxAvail.ToString());
            // <nuyen />
            objWriter.WriteElementString("nuyen", _decNuyen.ToString(GlobalOptions.InvariantCultureInfo));
            // <nuyen />
            objWriter.WriteElementString("startingnuyen", _decStartingNuyen.ToString(GlobalOptions.InvariantCultureInfo));
            // <adeptwaydiscount />
            objWriter.WriteElementString("adeptwaydiscount", _intAdeptWayDiscount.ToString());
            // <sumtoten />
            objWriter.WriteElementString("sumtoten", _intSumtoTen.ToString());
            // <buildpoints />
            objWriter.WriteElementString("bp", _intBuildPoints.ToString());
            // <buildkarma />
            objWriter.WriteElementString("buildkarma", _intBuildKarma.ToString());
            // <buildmethod />
            objWriter.WriteElementString("buildmethod", _objBuildMethod.ToString());
            // <gameplayoption />
            objWriter.WriteElementString("gameplayoption", _strGameplayOption);
            // <gameplayoptionqualitylimit />
            objWriter.WriteElementString("gameplayoptionqualitylimit", _intGameplayOptionQualityLimit.ToString());
            // <maxnuyen />
            objWriter.WriteElementString("maxnuyen", _decMaxNuyen.ToString(GlobalOptions.InvariantCultureInfo));
            // <maxkarma />
            objWriter.WriteElementString("maxkarma", _intMaxKarma.ToString());
            // <contactmultiplier />
            objWriter.WriteElementString("contactmultiplier", _intContactMultiplier.ToString());


            // <nuyenbp />
            objWriter.WriteElementString("nuyenbp", _decNuyenBP.ToString(GlobalOptions.InvariantCultureInfo));
            // <nuyenmaxbp />
            objWriter.WriteElementString("nuyenmaxbp", _decNuyenMaximumBP.ToString(GlobalOptions.InvariantCultureInfo));

            // <adept />
            objWriter.WriteElementString("adept", _blnAdeptEnabled.ToString());
            // <magician />
            objWriter.WriteElementString("magician", _blnMagicianEnabled.ToString());
            // <technomancer />
            objWriter.WriteElementString("technomancer", _blnTechnomancerEnabled.ToString());
            // <ai />
            objWriter.WriteElementString("ai", _blnAdvancedProgramsEnabled.ToString());
            // <cyberwaredisabled />
            objWriter.WriteElementString("cyberwaredisabled", _blnCyberwareDisabled.ToString());
            // <initiationoverride />
            objWriter.WriteElementString("initiationoverride", _blnInitiationEnabled.ToString());
            // <critter />
            objWriter.WriteElementString("critter", _blnCritterEnabled.ToString());

            // <friendsinhighplaces />
            objWriter.WriteElementString("friendsinhighplaces", _blnFriendsInHighPlaces.ToString());
            // <prototypetranshuman />
            objWriter.WriteElementString("prototypetranshuman", _decPrototypeTranshuman.ToString(GlobalOptions.InvariantCultureInfo));

            // <blackmarket />
            objWriter.WriteElementString("blackmarketdiscount", _blnBlackMarketDiscount.ToString());

            objWriter.WriteElementString("excon", _blnExCon.ToString());

            objWriter.WriteElementString("trustfund", _intTrustFund.ToString());



            objWriter.WriteElementString("restrictedgear", _blnRestrictedGear.ToString());

            objWriter.WriteElementString("overclocker", _blnOverclocker.ToString());

            objWriter.WriteElementString("mademan", _blnMadeMan.ToString());

            objWriter.WriteElementString("ambidextrous", _ambidextrous.ToString());

            objWriter.WriteElementString("lightningreflexes", _blnLightningReflexes.ToString());

            objWriter.WriteElementString("fame", _blnFame.ToString());

            objWriter.WriteElementString("bornrich", _blnBornRich.ToString());

            objWriter.WriteElementString("erased", _blnErased.ToString());

			// <attributes>
			objWriter.WriteStartElement("attributes");
	        AttributeSection.Save(objWriter);
            // </attributes>
            objWriter.WriteEndElement();

            // <magenabled />
            objWriter.WriteElementString("magenabled", _blnMAGEnabled.ToString());
            // <initiategrade />
            objWriter.WriteElementString("initiategrade", _intInitiateGrade.ToString());
            // <resenabled />
            objWriter.WriteElementString("resenabled", _blnRESEnabled.ToString());
            // <submersiongrade />
            objWriter.WriteElementString("submersiongrade", _intSubmersionGrade.ToString());
            // <depenabled />
            objWriter.WriteElementString("depenabled", _blnDEPEnabled.ToString());
            // <groupmember />
            objWriter.WriteElementString("groupmember", _blnGroupMember.ToString());
            // <groupname />
            objWriter.WriteElementString("groupname", _strGroupName);
            // <groupnotes />
            objWriter.WriteElementString("groupnotes", _strGroupNotes);

            // External reader friendly stuff.
            objWriter.WriteElementString("totaless", Essence.ToString(GlobalOptions.InvariantCultureInfo));

            // Write out the Mystic Adept MAG split info.
            if (_blnAdeptEnabled && _blnMagicianEnabled)
            {
                objWriter.WriteElementString("magsplitadept", _intMAGAdept.ToString());
                objWriter.WriteElementString("magsplitmagician", _intMAGMagician.ToString());
            }

            // Write the Magic Tradition.
            objWriter.WriteElementString("tradition", _strMagicTradition);
            // Write the Drain Attributes.
            objWriter.WriteElementString("traditiondrain", _strTraditionDrain);
            // Write the Tradition Name.
            objWriter.WriteElementString("traditionname", _strTraditionName);
            // Write the Tradition Spirits.
            objWriter.WriteElementString("spiritcombat", _strSpiritCombat);
            objWriter.WriteElementString("spiritdetection", _strSpiritDetection);
            objWriter.WriteElementString("spirithealth", _strSpiritHealth);
            objWriter.WriteElementString("spiritillusion", _strSpiritIllusion);
            objWriter.WriteElementString("spiritmanipulation", _strSpiritManipulation);
            // Write the Technomancer Stream.
            objWriter.WriteElementString("stream", _strTechnomancerStream);
            objWriter.WriteElementString("streamdrain", _strTechnomancerFading);

            // Condition Monitor Progress.
            // <physicalcmfilled />
            objWriter.WriteElementString("physicalcmfilled", _intPhysicalCMFilled.ToString());
            // <stuncmfilled />
            objWriter.WriteElementString("stuncmfilled", _intStunCMFilled.ToString());

            ///////////////////////////////////////////SKILLS
            ///

            SkillsSection.Save(objWriter);

            //Write copy of old skill groups, to not totally fuck a file if error
            oldSKillGroupBackup?.WriteTo(objWriter);
            oldSkillsBackup?.WriteTo(objWriter);

            ///////////////////////////////////////////SKILLS
            ///

            // <contacts>
            objWriter.WriteStartElement("contacts");
            foreach (Contact objContact in _lstContacts)
            {
                objContact.Save(objWriter);
            }
            // </contacts>
            objWriter.WriteEndElement();

            // <spells>
            objWriter.WriteStartElement("spells");
            foreach (Spell objSpell in _lstSpells)
            {
                objSpell.Save(objWriter);
            }
            // </spells>
            objWriter.WriteEndElement();

            // <foci>
            objWriter.WriteStartElement("foci");
            foreach (Focus objFocus in _lstFoci)
            {
                objFocus.Save(objWriter);
            }
            // </foci>
            objWriter.WriteEndElement();

            // <stackedfoci>
            objWriter.WriteStartElement("stackedfoci");
            foreach (StackedFocus objStack in _lstStackedFoci)
            {
                objStack.Save(objWriter);
            }
            // </stackedfoci>
            objWriter.WriteEndElement();

            // <powers>
            objWriter.WriteStartElement("powers");
            foreach (Power objPower in _lstPowers)
            {
                objPower.Save(objWriter);
            }
            // </powers>
            objWriter.WriteEndElement();

            // <spirits>
            objWriter.WriteStartElement("spirits");
            foreach (Spirit objSpirit in _lstSpirits)
            {
                objSpirit.Save(objWriter);
            }
            // </spirits>
            objWriter.WriteEndElement();

            // <complexforms>
            objWriter.WriteStartElement("complexforms");
            foreach (ComplexForm objProgram in _lstComplexForms)
            {
                objProgram.Save(objWriter);
            }
            // </complexforms>
            objWriter.WriteEndElement();

            // <aiprograms>
            objWriter.WriteStartElement("aiprograms");
            foreach (AIProgram objProgram in _lstAIPrograms)
            {
                objProgram.Save(objWriter);
            }
            // </aiprograms>
            objWriter.WriteEndElement();

            // <martialarts>
            objWriter.WriteStartElement("martialarts");
            foreach (MartialArt objMartialArt in _lstMartialArts)
            {
                objMartialArt.Save(objWriter);
            }
            // </martialarts>
            objWriter.WriteEndElement();

            // <martialartmaneuvers>
            objWriter.WriteStartElement("martialartmaneuvers");
            foreach (MartialArtManeuver objManeuver in _lstMartialArtManeuvers)
            {
                objManeuver.Save(objWriter);
            }
            // </martialartmaneuvers>
            objWriter.WriteEndElement();

            // <limitmodifiers>
            objWriter.WriteStartElement("limitmodifiers");
            foreach (LimitModifier objLimitModifier in _lstLimitModifiers)
            {
                objLimitModifier.Save(objWriter);
            }
            // </limitmodifiers>
            objWriter.WriteEndElement();

            // <armors>
            objWriter.WriteStartElement("armors");
            foreach (Armor objArmor in _lstArmor)
            {
                objArmor.Save(objWriter);
            }
            // </armors>
            objWriter.WriteEndElement();

            // <weapons>
            objWriter.WriteStartElement("weapons");
            foreach (Weapon objWeapon in _lstWeapons)
            {
                objWeapon.Save(objWriter);
            }
            // </weapons>
            objWriter.WriteEndElement();

            // <cyberwares>
            objWriter.WriteStartElement("cyberwares");
            foreach (Cyberware objCyberware in _lstCyberware)
            {
                objCyberware.Save(objWriter);
            }
            // </cyberwares>
            objWriter.WriteEndElement();

            // <qualities>
            objWriter.WriteStartElement("qualities");
            foreach (Quality objQuality in _lstQualities)
            {
                objQuality.Save(objWriter);
            }
            // </qualities>
            objWriter.WriteEndElement();

            // <lifestyles>
            objWriter.WriteStartElement("lifestyles");
            foreach (Lifestyle objLifestyle in _lstLifestyles)
            {
                objLifestyle.Save(objWriter);
            }
            // </lifestyles>
            objWriter.WriteEndElement();

            // <gears>
            objWriter.WriteStartElement("gears");
            foreach (Gear objGear in _lstGear)
            {
                // Use the Gear's SubClass if applicable.
                if (objGear.GetType() == typeof(Commlink))
                {
                    (objGear as Commlink).Save(objWriter);
                }
                else
                {
                    objGear.Save(objWriter);
                }
            }
            // </gears>
            objWriter.WriteEndElement();

            // <vehicles>
            objWriter.WriteStartElement("vehicles");
            foreach (Vehicle objVehicle in _lstVehicles)
            {
                objVehicle.Save(objWriter);
            }
            // </vehicles>
            objWriter.WriteEndElement();

            // <metamagics>
            objWriter.WriteStartElement("metamagics");
            foreach (Metamagic objMetamagic in _lstMetamagics)
            {
                objMetamagic.Save(objWriter);
            }
            // </metamagics>
            objWriter.WriteEndElement();

            // <arts>
            objWriter.WriteStartElement("arts");
            foreach (Art objArt in _lstArts)
            {
                objArt.Save(objWriter);
            }
            // </arts>
            objWriter.WriteEndElement();

            // <enhancements>
            objWriter.WriteStartElement("enhancements");
            foreach (Enhancement objEnhancement in _lstEnhancements)
            {
                objEnhancement.Save(objWriter);
            }
            // </enhancements>
            objWriter.WriteEndElement();

            // <critterpowers>
            objWriter.WriteStartElement("critterpowers");
            foreach (CritterPower objPower in _lstCritterPowers)
            {
                objPower.Save(objWriter);
            }
            // </critterpowers>
            objWriter.WriteEndElement();

            // <initiationgrades>
            objWriter.WriteStartElement("initiationgrades");
            foreach (InitiationGrade objGrade in _lstInitiationGrades)
            {
                objGrade.Save(objWriter);
            }
            // </initiationgrades>
            objWriter.WriteEndElement();

            // <improvements>
            objWriter.WriteStartElement("improvements");
            foreach (Improvement objImprovement in _lstImprovements)
            {
                objImprovement.Save(objWriter);
            }
            // </improvements>
            objWriter.WriteEndElement();



            // <expenses>
            objWriter.WriteStartElement("expenses");
            foreach (ExpenseLogEntry objExpenseLogEntry in _lstExpenseLog)
            {
                objExpenseLogEntry.Save(objWriter);
            }
            // </expenses>
            objWriter.WriteEndElement();

            // <locations>
            objWriter.WriteStartElement("locations");
            foreach (string strLocation in _lstLocations)
            {
                objWriter.WriteElementString("location", strLocation);
            }
            // </locations>
            objWriter.WriteEndElement();

            // <armorbundles>
            objWriter.WriteStartElement("armorbundles");
            foreach (string strBundle in _lstArmorBundles)
            {
                objWriter.WriteElementString("armorbundle", strBundle);
            }
            // </armorbundles>
            objWriter.WriteEndElement();

            // <weaponlocations>
            objWriter.WriteStartElement("weaponlocations");
            foreach (string strLocation in _lstWeaponLocations)
            {
                objWriter.WriteElementString("weaponlocation", strLocation);
            }
            // </weaponlocations>
            objWriter.WriteEndElement();

            // <improvementgroups>
            objWriter.WriteStartElement("improvementgroups");
            foreach (string strGroup in _lstImprovementGroups)
            {
                objWriter.WriteElementString("improvementgroup", strGroup);
            }
            // </improvementgroups>
            objWriter.WriteEndElement();

            // <calendar>
            objWriter.WriteStartElement("calendar");
            foreach (CalendarWeek objWeek in _lstCalendar)
            {
                objWeek.Save(objWriter);
            }
            objWriter.WriteEndElement();
            // </calendar>

            // <sources>
            objWriter.WriteStartElement("sources");
            foreach (string strItem in _lstSources)
            {
                objWriter.WriteElementString("source", strItem);
            }
            objWriter.WriteEndElement();
            // </sources>

            // <sources>
            objWriter.WriteStartElement("customdatadirectorynames");
            foreach (string strItem in _lstCustomDataDirectoryNames)
            {
                objWriter.WriteElementString("directoryname", strItem);
            }
            objWriter.WriteEndElement();
            // </sources>

            // </character>
            objWriter.WriteEndElement();

            objWriter.WriteEndDocument();
            objWriter.Flush();
            objStream.Flush();
            objStream.Position = 0;

            // Validate that the character can save properly. If there's no error, save the file to the listed file location.
            try
            {
                XmlDocument objDoc = new XmlDocument();
                objDoc.Load(objStream);
                objDoc.Save(strFileName);
            }
            catch (XmlException)
            {
                MessageBox.Show(LanguageManager.GetString("Message_Save_Error_Warning"));
            }
            catch (System.UnauthorizedAccessException)
            {
                MessageBox.Show(LanguageManager.GetString("Message_Save_Error_Warning"));
            }
            objWriter.Close();
            objStream.Close();
        }

        /// <summary>
        /// Load the Character from an XML file.
        /// </summary>
        public bool Load()
        {
            Timekeeper.Start("load_xml");
            XmlDocument objXmlDocument = new XmlDocument();
            if (!File.Exists(_strFileName)) return false;
            using (StreamReader sr = new StreamReader(_strFileName, true))
            {
                try
                {
                    objXmlDocument.Load(sr);
                }
                catch (XmlException ex)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_FailedLoad").Replace("{0}", ex.Message), LanguageManager.GetString("MessageTitle_FailedLoad"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            Timekeeper.Finish("load_xml");
            Timekeeper.Start("load_char_misc");
            XmlNode objXmlCharacter = objXmlDocument.SelectSingleNode("/character");

            objXmlCharacter.TryGetBoolFieldQuickly("ignorerules", ref _blnIgnoreRules);
            objXmlCharacter.TryGetBoolFieldQuickly("created", ref _blnCreated);

            ResetCharacter();

            // Get the game edition of the file if possible and make sure it's intended to be used with this version of the application.
            string strGameEdition = objXmlCharacter["gameedition"]?.InnerText ?? string.Empty;
            if (!string.IsNullOrEmpty(strGameEdition) && strGameEdition != "SR5")
            {
                MessageBox.Show(LanguageManager.GetString("Message_IncorrectGameVersion_SR4"),
                    LanguageManager.GetString("MessageTitle_IncorrectGameVersion"), MessageBoxButtons.YesNo,
                    MessageBoxIcon.Error);
                return false;
            }

            string strVersion = string.Empty;
            //Check to see if the character was created in a version of Chummer later than the currently installed one.
            if (objXmlCharacter.TryGetStringFieldQuickly("appversion", ref strVersion) && !string.IsNullOrEmpty(strVersion))
            {
                if (strVersion.StartsWith("0."))
                {
                    strVersion = strVersion.Substring(2);
                }
                Version.TryParse(strVersion, out _verSavedVersion);
            }
#if RELEASE
                Version verCurrentversion = Assembly.GetExecutingAssembly().GetName().Version;
                int intResult = verCurrentversion.CompareTo(_verSavedVersion);
                if (intResult == -1)
                {
                    string strMessage = LanguageManager.GetString("Message_OutdatedChummerSave").Replace("{0}", _verSavedVersion.ToString()).Replace("{1}", verCurrentversion.ToString());
                    DialogResult result = MessageBox.Show(strMessage, LanguageManager.GetString("MessageTitle_IncorrectGameVersion"), MessageBoxButtons.YesNo, MessageBoxIcon.Error);

                    if (result != DialogResult.Yes)
                    {
                        return false;
                    }
                }
#endif
            // Get the name of the settings file in use if possible.
            objXmlCharacter.TryGetStringFieldQuickly("settings", ref _strSettingsFileName);

            // Load the character's settings file.
            if (!_objOptions.Load(_strSettingsFileName))
                return false;

            // Get the sourcebooks that were used to create the character and throw up a warning if there's a mismatch.
            if (objXmlCharacter["sources"] != null)
            {
                string strMissingBooks = string.Empty;
                string strLoopString = string.Empty;
                //Does the list of enabled books contain the current item?
                foreach (XmlNode objXmlNode in objXmlCharacter["sources"].ChildNodes)
                {
                    strLoopString = objXmlNode.InnerText;
                    if (strLoopString.Length > 0 && !_objOptions.Books.Contains(strLoopString))
                    {
                        strMissingBooks += strLoopString + ";";
                    }
                }
                if (!string.IsNullOrEmpty(strMissingBooks))
                {
                    string strMessage = LanguageManager.GetString("Message_MissingSourceBooks").Replace("{0}", TranslatedBookList(strMissingBooks));
                    if (MessageBox.Show(strMessage, LanguageManager.GetString("Message_MissingSourceBooks_Title"), MessageBoxButtons.YesNo) == DialogResult.No)
                    {
                        return false;
                    }
                }
            }

            // Get the sourcebooks that were used to create the character and throw up a warning if there's a mismatch.
            if (objXmlCharacter["customdatadirectorynames"] != null)
            {
                string strMissingSourceNames = string.Empty;
                string strLoopString = string.Empty;
                //Does the list of enabled books contain the current item?
                foreach (XmlNode objXmlNode in objXmlCharacter["customdatadirectorynames"].ChildNodes)
                {
                    strLoopString = objXmlNode.InnerText;
                    if (strLoopString.Length > 0 && !_objOptions.CustomDataDirectoryNames.Contains(strLoopString))
                    {
                        strMissingSourceNames += strLoopString + ";\n";
                    }
                }
                if (!string.IsNullOrEmpty(strMissingSourceNames))
                {
                    string strMessage = LanguageManager.GetString("Message_MissingCustomDataDirectories").Replace("{0}", strMissingSourceNames);
                    if (MessageBox.Show(strMessage, LanguageManager.GetString("Message_MissingCustomDataDirectories_Title"), MessageBoxButtons.YesNo) == DialogResult.No)
                    {
                        return false;
                    }
                }
            }

            if (objXmlCharacter["essenceatspecialstart"] != null)
            {
                _decEssenceAtSpecialStart = Convert.ToDecimal(objXmlCharacter["essenceatspecialstart"].InnerText,
                    GlobalOptions.InvariantCultureInfo);
                // fix to work around a mistake made when saving decimal values in previous versions.
                if (_decEssenceAtSpecialStart > EssenceMaximum)
                    _decEssenceAtSpecialStart /= 10;
            }

            objXmlCharacter.TryGetStringFieldQuickly("createdversion", ref _strVersionCreated);

            // Metatype information.
            objXmlCharacter.TryGetStringFieldQuickly("metatype", ref _strMetatype);
            objXmlCharacter.TryGetStringFieldQuickly("movement", ref _strMovement);

            objXmlCharacter.TryGetStringFieldQuickly("walk", ref _strWalk);
            objXmlCharacter.TryGetStringFieldQuickly("run", ref _strRun);
            objXmlCharacter.TryGetStringFieldQuickly("sprint", ref _strSprint);

            _strRunAlt = objXmlCharacter["run"]?.Attributes["alt"]?.InnerText ?? string.Empty;
            _strWalkAlt = objXmlCharacter["walk"]?.Attributes["alt"]?.InnerText ?? string.Empty;
            _strSprintAlt = objXmlCharacter["sprint"]?.Attributes["alt"]?.InnerText ?? string.Empty;

            objXmlCharacter.TryGetInt32FieldQuickly("metatypebp", ref _intMetatypeBP);
            objXmlCharacter.TryGetStringFieldQuickly("metavariant", ref _strMetavariant);

            //Shim for characters created prior to Run Faster Errata
            if (_strMetavariant == "Cyclopean")
            {
                _strMetavariant = "Cyclops";
            }
            objXmlCharacter.TryGetStringFieldQuickly("metatypecategory", ref _strMetatypeCategory);

            // General character information.
            objXmlCharacter.TryGetStringFieldQuickly("name", ref _strName);
            // Mugshots
            objXmlCharacter.TryGetInt32FieldQuickly("mainmugshotindex", ref _intMainMugshotIndex);
            XmlNodeList objXmlMugshotsList = objXmlDocument.SelectNodes("/character/mugshots/mugshot");
            if (objXmlMugshotsList != null)
            {
                foreach (XmlNode objXmlMugshot in objXmlMugshotsList)
                {
                    Mugshots.Add(objXmlMugshot.InnerText);
                }
            }
            if (Mugshots.Count == 0)
            {
                XmlNode objOldMugshotNode = objXmlDocument.SelectSingleNode("/character/mugshot");
                if (objOldMugshotNode != null)
                {
                    Mugshots.Add(objOldMugshotNode.InnerText);
                }
            }
            objXmlCharacter.TryGetStringFieldQuickly("sex", ref _strSex);
            objXmlCharacter.TryGetStringFieldQuickly("age", ref _strAge);
            objXmlCharacter.TryGetStringFieldQuickly("eyes", ref _strEyes);
            objXmlCharacter.TryGetStringFieldQuickly("height", ref _strHeight);
            objXmlCharacter.TryGetStringFieldQuickly("weight", ref _strWeight);
            objXmlCharacter.TryGetStringFieldQuickly("skin", ref _strSkin);
            objXmlCharacter.TryGetStringFieldQuickly("hair", ref _strHair);
            objXmlCharacter.TryGetStringFieldQuickly("description", ref _strDescription);
            objXmlCharacter.TryGetStringFieldQuickly("background", ref _strBackground);
            objXmlCharacter.TryGetStringFieldQuickly("concept", ref _strConcept);
            objXmlCharacter.TryGetStringFieldQuickly("notes", ref _strNotes);
            objXmlCharacter.TryGetStringFieldQuickly("alias", ref _strAlias);
            objXmlCharacter.TryGetStringFieldQuickly("playername", ref _strPlayerName);
            objXmlCharacter.TryGetStringFieldQuickly("gamenotes", ref _strGameNotes);
            if (!objXmlCharacter.TryGetStringFieldQuickly("primaryarm", ref _strPrimaryArm))
                _strPrimaryArm = "Right";

            objXmlCharacter.TryGetStringFieldQuickly("gameplayoption", ref _strGameplayOption);
            objXmlCharacter.TryGetDecFieldQuickly("maxnuyen", ref _decMaxNuyen);
            objXmlCharacter.TryGetInt32FieldQuickly("contactmultiplier", ref _intContactMultiplier);
            objXmlCharacter.TryGetInt32FieldQuickly("maxkarma", ref _intMaxKarma);
            objXmlCharacter.TryGetStringFieldQuickly("prioritymetatype", ref _strPriorityMetatype);
            objXmlCharacter.TryGetStringFieldQuickly("priorityattributes", ref _strPriorityAttributes);
            objXmlCharacter.TryGetStringFieldQuickly("priorityspecial", ref _strPrioritySpecial);
            objXmlCharacter.TryGetStringFieldQuickly("priorityskills", ref _strPrioritySkills);
            objXmlCharacter.TryGetStringFieldQuickly("priorityresources", ref _strPriorityResources);
            objXmlCharacter.TryGetStringFieldQuickly("prioritytalent", ref _strPriorityTalent);
            _lstPrioritySkills.Clear();
            XmlNodeList objXmlPrioritySkillsList = objXmlDocument.SelectNodes("/character/priorityskills/priorityskill");
            if (objXmlPrioritySkillsList != null)
            {
                foreach (XmlNode objXmlSkillName in objXmlPrioritySkillsList)
                {
                    _lstPrioritySkills.Add(objXmlSkillName.InnerText);
                }
            }
            string strSkill1 = string.Empty;
            string strSkill2 = string.Empty;
            if (objXmlCharacter.TryGetStringFieldQuickly("priorityskill1", ref strSkill1) && !string.IsNullOrEmpty(strSkill1))
                _lstPrioritySkills.Add(strSkill1);
            if (objXmlCharacter.TryGetStringFieldQuickly("priorityskill2", ref strSkill2) && !string.IsNullOrEmpty(strSkill2))
                _lstPrioritySkills.Add(strSkill2);

            objXmlCharacter.TryGetBoolFieldQuickly("iscritter", ref _blnIsCritter);

            objXmlCharacter.TryGetInt32FieldQuickly("metageneticlimit", ref _intMetageneticLimit);
            objXmlCharacter.TryGetBoolFieldQuickly("possessed", ref _blnPossessed);

            objXmlCharacter.TryGetInt32FieldQuickly("contactpoints", ref _intContactPoints);
            objXmlCharacter.TryGetInt32FieldQuickly("contactpointsused", ref _intContactPointsUsed);
            objXmlCharacter.TryGetInt32FieldQuickly("cfplimit", ref _intCFPLimit);
            objXmlCharacter.TryGetInt32FieldQuickly("ainormalprogramlimit", ref _intAINormalProgramLimit);
            objXmlCharacter.TryGetInt32FieldQuickly("aiadvancedprogramlimit", ref _intAIAdvancedProgramLimit);
            objXmlCharacter.TryGetInt32FieldQuickly("spelllimit", ref _intSpellLimit);
            objXmlCharacter.TryGetInt32FieldQuickly("karma", ref _intKarma);
            objXmlCharacter.TryGetInt32FieldQuickly("totalkarma", ref _intTotalKarma);

            objXmlCharacter.TryGetInt32FieldQuickly("special", ref _intSpecial);
            objXmlCharacter.TryGetInt32FieldQuickly("totalspecial", ref _intTotalSpecial);
            //objXmlCharacter.tryGetInt32FieldQuickly("attributes", ref _intAttributes); //wonkey
            objXmlCharacter.TryGetInt32FieldQuickly("totalattributes", ref _intTotalAttributes);
            objXmlCharacter.TryGetInt32FieldQuickly("contactpoints", ref _intContactPoints);
            objXmlCharacter.TryGetInt32FieldQuickly("contactpointsused", ref _intContactPointsUsed);
            objXmlCharacter.TryGetInt32FieldQuickly("streetcred", ref _intStreetCred);
            objXmlCharacter.TryGetInt32FieldQuickly("notoriety", ref _intNotoriety);
            objXmlCharacter.TryGetInt32FieldQuickly("publicawareness", ref _intPublicAwareness);
            objXmlCharacter.TryGetInt32FieldQuickly("burntstreetcred", ref _intBurntStreetCred);
            objXmlCharacter.TryGetInt32FieldQuickly("maxavail", ref _intMaxAvail);
            objXmlCharacter.TryGetDecFieldQuickly("nuyen", ref _decNuyen);
            objXmlCharacter.TryGetDecFieldQuickly("startingnuyen", ref _decStartingNuyen);
            objXmlCharacter.TryGetInt32FieldQuickly("adeptwaydiscount", ref _intAdeptWayDiscount);

            // Sum to X point value.
            objXmlCharacter.TryGetInt32FieldQuickly("sumtoten", ref _intSumtoTen);
            // Build Points/Karma.
            objXmlCharacter.TryGetInt32FieldQuickly("bp", ref _intBuildPoints);
            objXmlCharacter.TryGetInt32FieldQuickly("buildkarma", ref _intBuildKarma);
                if (_intMaxKarma == 0)
                    _intMaxKarma = _intBuildKarma;
            if (_intBuildKarma == 35)
                {
                if (string.IsNullOrEmpty(_strGameplayOption))
                    _strGameplayOption = "Prime Runner";
                if (_decMaxNuyen == 0)
                    _decMaxNuyen = 25;
                }
            //Maximum number of Karma that can be spent/gained on Qualities.
            objXmlCharacter.TryGetInt32FieldQuickly("gameplayoptionqualitylimit", ref _intGameplayOptionQualityLimit);

            objXmlCharacter.TryGetField("buildmethod", Enum.TryParse, out _objBuildMethod);

            objXmlCharacter.TryGetDecFieldQuickly("nuyenbp", ref _decNuyenBP);
            objXmlCharacter.TryGetDecFieldQuickly("nuyenmaxbp", ref _decNuyenMaximumBP);
            objXmlCharacter.TryGetBoolFieldQuickly("adept", ref _blnAdeptEnabled);
            objXmlCharacter.TryGetBoolFieldQuickly("magician", ref _blnMagicianEnabled);
            objXmlCharacter.TryGetBoolFieldQuickly("technomancer", ref _blnTechnomancerEnabled);
            objXmlCharacter.TryGetBoolFieldQuickly("ai", ref _blnAdvancedProgramsEnabled);
            objXmlCharacter.TryGetBoolFieldQuickly("cyberwaredisabled", ref _blnCyberwareDisabled);
            objXmlCharacter.TryGetBoolFieldQuickly("initiationoverride", ref _blnInitiationEnabled);
            objXmlCharacter.TryGetBoolFieldQuickly("critter", ref _blnCritterEnabled);

            objXmlCharacter.TryGetBoolFieldQuickly("friendsinhighplaces", ref _blnFriendsInHighPlaces);
            objXmlCharacter.TryGetDecFieldQuickly("prototypetranshuman", ref _decPrototypeTranshuman);
            objXmlCharacter.TryGetBoolFieldQuickly("blackmarket", ref _blnBlackMarketDiscount);
            objXmlCharacter.TryGetBoolFieldQuickly("excon", ref _blnExCon);
            objXmlCharacter.TryGetInt32FieldQuickly("trustfund", ref _intTrustFund);
            objXmlCharacter.TryGetBoolFieldQuickly("restrictedgear", ref _blnRestrictedGear);
            objXmlCharacter.TryGetBoolFieldQuickly("overclocker", ref _blnOverclocker);
            objXmlCharacter.TryGetBoolFieldQuickly("mademan", ref _blnMadeMan);
            objXmlCharacter.TryGetBoolFieldQuickly("lightningreflexes", ref _blnLightningReflexes);
            objXmlCharacter.TryGetBoolFieldQuickly("fame", ref _blnFame);
            objXmlCharacter.TryGetBoolFieldQuickly("ambidextrous", ref _ambidextrous);
            objXmlCharacter.TryGetBoolFieldQuickly("bornrich", ref _blnBornRich);
            objXmlCharacter.TryGetBoolFieldQuickly("erased", ref _blnErased);
            objXmlCharacter.TryGetBoolFieldQuickly("magenabled", ref _blnMAGEnabled);
            objXmlCharacter.TryGetInt32FieldQuickly("initiategrade", ref _intInitiateGrade);
            objXmlCharacter.TryGetBoolFieldQuickly("resenabled", ref _blnRESEnabled);
            objXmlCharacter.TryGetInt32FieldQuickly("submersiongrade", ref _intSubmersionGrade);
            objXmlCharacter.TryGetBoolFieldQuickly("depenabled", ref _blnDEPEnabled);
            objXmlCharacter.TryGetBoolFieldQuickly("groupmember", ref _blnGroupMember);
            objXmlCharacter.TryGetStringFieldQuickly("groupname", ref _strGroupName);
            objXmlCharacter.TryGetStringFieldQuickly("groupnotes", ref _strGroupNotes);
            Timekeeper.Finish("load_char_misc");
            bool blnImprovementError = false;
            Timekeeper.Start("load_char_imp");
            // Improvements.
            XmlNodeList objXmlNodeList = objXmlDocument.SelectNodes("/character/improvements/improvement");
            foreach (XmlNode objXmlImprovement in objXmlNodeList)
            {
                Improvement objImprovement = new Improvement();
                try
                {
                    objImprovement.Load(objXmlImprovement);
                    _lstImprovements.Add(objImprovement);
                }
                catch (ArgumentException)
                {
                    blnImprovementError = true;
                }
            }
            Timekeeper.Finish("load_char_imp");
            if (blnImprovementError)
                MessageBox.Show(LanguageManager.GetString("Message_ImprovementLoadError"), LanguageManager.GetString("MessageTitle_ImprovementLoadError"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            Timekeeper.Start("load_char_quality");
            // Qualities
            objXmlNodeList = objXmlDocument.SelectNodes("/character/qualities/quality");
            bool blnHasOldQualities = false;
            foreach (XmlNode objXmlQuality in objXmlNodeList)
            {
                if (objXmlQuality["name"] != null)
                {
                    if (!CorrectedUnleveledQuality(objXmlQuality))
                    {
                        Quality objQuality = new Quality(this);
                        objQuality.Load(objXmlQuality);
                        _lstQualities.Add(objQuality);
                    }
                }
                else
                {
                    // If the Quality does not have a name tag, it is in the old format. Set the flag to show that old Qualities are in use.
                    blnHasOldQualities = true;
                }
            }
            // If old Qualities are in use, they need to be converted before we can continue.
            if (blnHasOldQualities)
                ConvertOldQualities(objXmlNodeList);
	        Timekeeper.Finish("load_char_quality");
			AttributeSection.Load(objXmlDocument);
			Timekeeper.Start("load_char_misc2");

            objXmlCharacter = objXmlDocument.SelectSingleNode("/character");

            // Attempt to load the split MAG CharacterAttribute information for Mystic Adepts.
            if (_blnAdeptEnabled && _blnMagicianEnabled)
            {
                objXmlCharacter.TryGetInt32FieldQuickly("magsplitadept", ref _intMAGAdept);
                objXmlCharacter.TryGetInt32FieldQuickly("magsplitmagician", ref _intMAGMagician);
                }

            // Attempt to load the Magic Tradition.
            objXmlCharacter.TryGetStringFieldQuickly("tradition", ref _strMagicTradition);
            // Attempt to load the Magic Tradition Drain Attributes.
            objXmlCharacter.TryGetStringFieldQuickly("traditiondrain", ref _strTraditionDrain);
            // Attempt to load the Magic Tradition Name.
            objXmlCharacter.TryGetStringFieldQuickly("traditionname", ref _strTraditionName);
            // Attempt to load the Spirit Combat Name.
            objXmlCharacter.TryGetStringFieldQuickly("spiritcombat", ref _strSpiritCombat);
            // Attempt to load the Spirit Detection Name.
            objXmlCharacter.TryGetStringFieldQuickly("spiritdetection", ref _strSpiritDetection);
            // Attempt to load the Spirit Health Name.
            objXmlCharacter.TryGetStringFieldQuickly("spirithealth", ref _strSpiritHealth);
            // Attempt to load the Spirit Illusion Name.
            objXmlCharacter.TryGetStringFieldQuickly("spiritillusion", ref _strSpiritIllusion);
            // Attempt to load the Spirit Manipulation Name.
            objXmlCharacter.TryGetStringFieldQuickly("spiritmanipulation", ref _strSpiritManipulation);
            // Attempt to load the Technomancer Stream.
            objXmlCharacter.TryGetStringFieldQuickly("stream", ref _strTechnomancerStream);
            // Attempt to load the Technomancer Stream's Fading attributes.
            objXmlCharacter.TryGetStringFieldQuickly("streamfading", ref _strTechnomancerFading);

            // Attempt to load Condition Monitor Progress.
            objXmlCharacter.TryGetInt32FieldQuickly("physicalcmfilled", ref _intPhysicalCMFilled);
            objXmlCharacter.TryGetInt32FieldQuickly("stuncmfilled", ref _intStunCMFilled);
            Timekeeper.Finish("load_char_misc2");
            Timekeeper.Start("load_char_skills");  //slightly messy

            oldSkillsBackup = objXmlDocument.SelectSingleNode("/character/skills")?.Clone();
            oldSKillGroupBackup = objXmlDocument.SelectSingleNode("/character/skillgroups")?.Clone();

            XmlNode SkillNode = objXmlDocument.SelectSingleNode("/character/newskills");
            if (SkillNode != null)
            {
                SkillsSection.Load(SkillNode);
            }
            else
            {
                SkillsSection.Load(objXmlCharacter, true);
            }

            Timekeeper.Start("load_char_contacts");


            // Contacts.
            objXmlNodeList = objXmlDocument.SelectNodes("/character/contacts/contact");
            foreach (XmlNode objXmlContact in objXmlNodeList)
            {
                Contact objContact = new Contact(this);
                objContact.Load(objXmlContact);
                _lstContacts.Add(objContact);
            }

            Timekeeper.Finish("load_char_contacts");
            Timekeeper.Start("load_char_armor");
            // Armor.
            objXmlNodeList = objXmlDocument.SelectNodes("/character/armors/armor");
            foreach (XmlNode objXmlArmor in objXmlNodeList)
            {
                Armor objArmor = new Armor(this);
                objArmor.Load(objXmlArmor);
                _lstArmor.Add(objArmor);
            }
            Timekeeper.Finish("load_char_armor");
            Timekeeper.Start("load_char_weapons");

            // Weapons.
            objXmlNodeList = objXmlDocument.SelectNodes("/character/weapons/weapon");
            foreach (XmlNode objXmlWeapon in objXmlNodeList)
            {
                Weapon objWeapon = new Weapon(this);
                objWeapon.Load(objXmlWeapon);
                _lstWeapons.Add(objWeapon);
            }

            Timekeeper.Finish("load_char_weapons");
            Timekeeper.Start("load_char_ware");

            // Cyberware/Bioware.
            objXmlNodeList = objXmlDocument.SelectNodes("/character/cyberwares/cyberware");
            foreach (XmlNode objXmlCyberware in objXmlNodeList)
            {
                Cyberware objCyberware = new Cyberware(this);
                objCyberware.Load(objXmlCyberware);
                _lstCyberware.Add(objCyberware);
            }

            Timekeeper.Finish("load_char_ware");
            Timekeeper.Start("load_char_spells");

            // Spells.
            objXmlNodeList = objXmlDocument.SelectNodes("/character/spells/spell");
            foreach (XmlNode objXmlSpell in objXmlNodeList)
            {
                Spell objSpell = new Spell(this);
                objSpell.Load(objXmlSpell);
                _lstSpells.Add(objSpell);
            }

            Timekeeper.Finish("load_char_spells");
            Timekeeper.Start("load_char_foci");

            // Foci.
            objXmlNodeList = objXmlDocument.SelectNodes("/character/foci/focus");
            foreach (XmlNode objXmlFocus in objXmlNodeList)
            {
                Focus objFocus = new Focus();
                objFocus.Load(objXmlFocus);
                _lstFoci.Add(objFocus);
            }

            Timekeeper.Finish("load_char_foci");
            Timekeeper.Start("load_char_sfoci");

            // Stacked Foci.
            objXmlNodeList = objXmlDocument.SelectNodes("/character/stackedfoci/stackedfocus");
            foreach (XmlNode objXmlStack in objXmlNodeList)
            {
                StackedFocus objStack = new StackedFocus(this);
                objStack.Load(objXmlStack);
                _lstStackedFoci.Add(objStack);
            }

            Timekeeper.Finish("load_char_sfoci");
            Timekeeper.Start("load_char_powers");

            // Powers.
            List<ListItem> lstPowerOrder = new List<ListItem>();
            objXmlNodeList = objXmlDocument.SelectNodes("/character/powers/power");
            // Sort the Powers in alphabetical order.
            foreach (XmlNode objXmlPower in objXmlNodeList)
            {
                ListItem objGroup = new ListItem();
                objGroup.Value = objXmlPower["extra"]?.InnerText;
                objGroup.Name = objXmlPower["name"]?.InnerText;

                lstPowerOrder.Add(objGroup);
            }
            SortListItem objSort = new SortListItem();
            lstPowerOrder.Sort(objSort.Compare);

            foreach (ListItem objItem in lstPowerOrder)
            {
                Power objPower = new Power(this);
                XmlNode objNode = objXmlDocument.SelectSingleNode("/character/powers/power[name = " + CleanXPath(objItem.Name) + " and extra = " + CleanXPath(objItem.Value) + "]");
                if (objNode != null)
                {
                objPower.Load(objNode);
                _lstPowers.Add(objPower);
            }
            }

            Timekeeper.Finish("load_char_powers");
            Timekeeper.Start("load_char_spirits");

            // Spirits/Sprites.
            objXmlNodeList = objXmlDocument.SelectNodes("/character/spirits/spirit");
            foreach (XmlNode objXmlSpirit in objXmlNodeList)
            {
                Spirit objSpirit = new Spirit(this);
                objSpirit.Load(objXmlSpirit);
                _lstSpirits.Add(objSpirit);
            }

            Timekeeper.Finish("load_char_spirits");
            Timekeeper.Start("load_char_complex");

            // Compex Forms/Technomancer Programs.
            objXmlNodeList = objXmlDocument.SelectNodes("/character/complexforms/complexform");
            foreach (XmlNode objXmlProgram in objXmlNodeList)
            {
                ComplexForm objProgram = new ComplexForm(this);
                objProgram.Load(objXmlProgram);
                _lstComplexForms.Add(objProgram);
            }

            Timekeeper.Finish("load_char_complex");
            Timekeeper.Start("load_char_aiprogram");

            // Compex Forms/Technomancer Programs.
            objXmlNodeList = objXmlDocument.SelectNodes("/character/aiprograms/aiprogram");
            foreach (XmlNode objXmlProgram in objXmlNodeList)
            {
                AIProgram objProgram = new AIProgram(this);
                objProgram.Load(objXmlProgram);
                _lstAIPrograms.Add(objProgram);
            }

            Timekeeper.Finish("load_char_aiprogram");
            Timekeeper.Start("load_char_marts");

            // Martial Arts.
            objXmlNodeList = objXmlDocument.SelectNodes("/character/martialarts/martialart");
            foreach (XmlNode objXmlArt in objXmlNodeList)
            {
                MartialArt objMartialArt = new MartialArt(this);
                objMartialArt.Load(objXmlArt);
                _lstMartialArts.Add(objMartialArt);
            }

            Timekeeper.Finish("load_char_marts");
            Timekeeper.Start("load_char_mam");

            // Martial Art Maneuvers.
            objXmlNodeList = objXmlDocument.SelectNodes("/character/martialartmaneuvers/martialartmaneuver");
            foreach (XmlNode objXmlManeuver in objXmlNodeList)
            {
                MartialArtManeuver objManeuver = new MartialArtManeuver(this);
                objManeuver.Load(objXmlManeuver);
                _lstMartialArtManeuvers.Add(objManeuver);
            }

            Timekeeper.Finish("load_char_mam");
            Timekeeper.Start("load_char_mod");

            // Limit Modifiers.
            objXmlNodeList = objXmlDocument.SelectNodes("/character/limitmodifiers/limitmodifier");
            foreach (XmlNode objXmlLimit in objXmlNodeList)
            {
                LimitModifier obLimitModifier = new LimitModifier(this);
                obLimitModifier.Load(objXmlLimit);
                _lstLimitModifiers.Add(obLimitModifier);
            }

            Timekeeper.Finish("load_char_mod");
            Timekeeper.Start("load_char_lifestyle");

            // Lifestyles.
            objXmlNodeList = objXmlDocument.SelectNodes("/character/lifestyles/lifestyle");
            foreach (XmlNode objXmlLifestyle in objXmlNodeList)
            {
                Lifestyle objLifestyle = new Lifestyle(this);
                objLifestyle.Load(objXmlLifestyle);
                _lstLifestyles.Add(objLifestyle);
            }

            Timekeeper.Finish("load_char_lifestyle");
            Timekeeper.Start("load_char_gear");

            // <gears>
            objXmlNodeList = objXmlDocument.SelectNodes("/character/gears/gear");
            foreach (XmlNode objXmlGear in objXmlNodeList)
            {
                if (objXmlGear["iscommlink"]?.InnerText == System.Boolean.TrueString || (objXmlGear["category"].InnerText == "Commlinks" ||
                    objXmlGear["category"].InnerText == "Commlink Accessories" || objXmlGear["category"].InnerText == "Cyberdecks" || objXmlGear["category"].InnerText == "Rigger Command Consoles"))
                {
                    Commlink objCommlink = new Commlink(this);
                    objCommlink.Load(objXmlGear);
                    _lstGear.Add(objCommlink);
                }
                else
                {
                    Gear objGear = new Gear(this);
                    objGear.Load(objXmlGear);
                    _lstGear.Add(objGear);
                }
            }

            Timekeeper.Finish("load_char_gear");
            Timekeeper.Start("load_char_car");

            // Vehicles.
            objXmlNodeList = objXmlDocument.SelectNodes("/character/vehicles/vehicle");
            foreach (XmlNode objXmlVehicle in objXmlNodeList)
            {
                Vehicle objVehicle = new Vehicle(this);
                objVehicle.Load(objXmlVehicle);
                _lstVehicles.Add(objVehicle);
            }

            Timekeeper.Finish("load_char_car");
            Timekeeper.Start("load_char_mmagic");
            // Metamagics/Echoes.
            objXmlNodeList = objXmlDocument.SelectNodes("/character/metamagics/metamagic");
            foreach (XmlNode objXmlMetamagic in objXmlNodeList)
            {
                Metamagic objMetamagic = new Metamagic(this);
                objMetamagic.Load(objXmlMetamagic);
                _lstMetamagics.Add(objMetamagic);
            }

            Timekeeper.Finish("load_char_mmagic");
            Timekeeper.Start("load_char_arts");

            // Arts
            objXmlNodeList = objXmlDocument.SelectNodes("/character/arts/art");
            foreach (XmlNode objXmlArt in objXmlNodeList)
            {
                Art objArt = new Art(this);
                objArt.Load(objXmlArt);
                _lstArts.Add(objArt);
            }

            Timekeeper.Finish("load_char_arts");
            Timekeeper.Start("load_char_ench");

            // Enhancements
            objXmlNodeList = objXmlDocument.SelectNodes("/character/enhancements/enhancement");
            foreach (XmlNode objXmlEnhancement in objXmlNodeList)
            {
                Enhancement objEnhancement = new Enhancement(this);
                objEnhancement.Load(objXmlEnhancement);
                _lstEnhancements.Add(objEnhancement);
            }

            Timekeeper.Finish("load_char_ench");
            Timekeeper.Start("load_char_cpow");

            // Critter Powers.
            objXmlNodeList = objXmlDocument.SelectNodes("/character/critterpowers/critterpower");
            foreach (XmlNode objXmlPower in objXmlNodeList)
            {
                CritterPower objPower = new CritterPower(this);
                objPower.Load(objXmlPower);
                _lstCritterPowers.Add(objPower);
            }

            Timekeeper.Finish("load_char_cpow");
            Timekeeper.Start("load_char_init");

            // Initiation Grades.
            objXmlNodeList = objXmlDocument.SelectNodes("/character/initiationgrades/initiationgrade");
            foreach (XmlNode objXmlGrade in objXmlNodeList)
            {
                InitiationGrade objGrade = new InitiationGrade(this);
                objGrade.Load(objXmlGrade);
                _lstInitiationGrades.Add(objGrade);
            }

            Timekeeper.Finish("load_char_init");
            Timekeeper.Start("load_char_elog");

            // Expense Log Entries.
            XmlNodeList objXmlExpenseList = objXmlDocument.SelectNodes("/character/expenses/expense");
            foreach (XmlNode objXmlExpense in objXmlExpenseList)
            {
                ExpenseLogEntry objExpenseLogEntry = new ExpenseLogEntry();
                objExpenseLogEntry.Load(objXmlExpense);
                _lstExpenseLog.Add(objExpenseLogEntry);
            }

            Timekeeper.Finish("load_char_elog");
            Timekeeper.Start("load_char_loc");

            // Locations.
            XmlNodeList objXmlLocationList = objXmlDocument.SelectNodes("/character/locations/location");
            foreach (XmlNode objXmlLocation in objXmlLocationList)
            {
                _lstLocations.Add(objXmlLocation.InnerText);
            }

            Timekeeper.Finish("load_char_loc");
            Timekeeper.Start("load_char_abundle");

            // Armor Bundles.
            XmlNodeList objXmlBundleList = objXmlDocument.SelectNodes("/character/armorbundles/armorbundle");
            foreach (XmlNode objXmlBundle in objXmlBundleList)
            {
                _lstArmorBundles.Add(objXmlBundle.InnerText);
            }

            Timekeeper.Finish("load_char_abundle");
            Timekeeper.Start("load_char_wloc");

            // Weapon Locations.
            XmlNodeList objXmlWeaponLocationList = objXmlDocument.SelectNodes("/character/weaponlocations/weaponlocation");
            foreach (XmlNode objXmlLocation in objXmlWeaponLocationList)
            {
                _lstWeaponLocations.Add(objXmlLocation.InnerText);
            }

            Timekeeper.Finish("load_char_wloc");
            Timekeeper.Start("load_char_igroup");

            // Improvement Groups.
            XmlNodeList objXmlGroupList = objXmlDocument.SelectNodes("/character/improvementgroups/improvementgroup");
            foreach (XmlNode objXmlGroup in objXmlGroupList)
            {
                _lstImprovementGroups.Add(objXmlGroup.InnerText);
            }

            Timekeeper.Finish("load_char_igroup");
            Timekeeper.Start("load_char_calendar");

            // Calendar.
            XmlNodeList objXmlWeekList = objXmlDocument.SelectNodes("/character/calendar/week");
            foreach (XmlNode objXmlWeek in objXmlWeekList)
            {
                CalendarWeek objWeek = new CalendarWeek();
                objWeek.Load(objXmlWeek);
                _lstCalendar.Add(objWeek);
            }

            Timekeeper.Finish("load_char_calendar");
            Timekeeper.Start("load_char_unarmed");

            // Look for the unarmed attack
            bool blnFoundUnarmed = false;
            foreach (Weapon objWeapon in _lstWeapons)
            {
                if (objWeapon.Name == "Unarmed Attack")
                {
                    blnFoundUnarmed = true;
                    break;
                }
            }

            if (!blnFoundUnarmed)
            {
                // Add the Unarmed Attack Weapon to the character.
                    XmlDocument objXmlWeaponDoc = XmlManager.Load("weapons.xml");
                    XmlNode objXmlWeapon = objXmlWeaponDoc.SelectSingleNode("/chummer/weapons/weapon[name = \"Unarmed Attack\"]");
                if (objXmlWeapon != null)
                {
                    TreeNode objGearWeaponNode = new TreeNode();
                    Weapon objWeapon = new Weapon(this);
                    objWeapon.Create(objXmlWeapon, objGearWeaponNode, null, null);
                    objWeapon.IncludedInWeapon = true; // Unarmed attack can never be removed
                    _lstWeapons.Add(objWeapon);
                }
            }

            Timekeeper.Finish("load_char_unarmed");
            Timekeeper.Start("load_char_dwarffix");

            // converting from old dwarven resistance to new dwarven resistance
            if (Metatype.ToLower().Equals("dwarf"))
            {
                Quality objOldQuality = Qualities.FirstOrDefault(x => x.Name.Equals("Resistance to Pathogens and Toxins"));
                if (objOldQuality != null)
                {
                    Qualities.Remove(objOldQuality);
                    if (Qualities.Any(x => x.Name.Equals("Resistance to Pathogens/Toxins")) == false &&
                        Qualities.Any(x => x.Name.Equals("Dwarf Resistance")) == false)
                    {
                        XmlNode objXmlDwarfQuality =
                            XmlManager.Load("qualities.xml")
                                .SelectSingleNode(
                                    "/chummer/qualities/quality[name = \"Resistance to Pathogens/Toxins\"]");

                        if (objXmlDwarfQuality == null)
                            objXmlDwarfQuality =
                                XmlManager.Load("qualities.xml")
                                    .SelectSingleNode("/chummer/qualities/quality[name = \"Dwarf Resistance\"]");

                TreeNode objNode = new TreeNode();
                List<Weapon> objWeapons = new List<Weapon>();
                List<TreeNode> objWeaponNodes = new List<TreeNode>();
                Quality objQuality = new Quality(this);

                        objQuality.Create(objXmlDwarfQuality, this, QualitySource.Metatype, objNode, objWeapons,
                            objWeaponNodes);
                        _lstQualities.Add(objQuality);
                    }
                blnHasOldQualities = true;
            }
            }

            Timekeeper.Finish("load_char_dwarffix");
            Timekeeper.Start("load_char_cfix");

            // load issue where the contact multiplier was set to 0
            if (_intContactMultiplier == 0 && !string.IsNullOrEmpty(_strGameplayOption))
            {
                XmlDocument objXmlDocumentPriority = XmlManager.Load("gameplayoptions.xml");
                XmlNode objXmlGameplayOption = objXmlDocumentPriority.SelectSingleNode("/chummer/gameplayoptions/gameplayoption[name = \"" + _strGameplayOption + "\"]");
                if (objXmlGameplayOption != null)
                {
                    string strKarma = objXmlGameplayOption["karma"]?.InnerText;
                    string strNuyen = objXmlGameplayOption["maxnuyen"]?.InnerText;
                    string strContactMultiplier = objXmlGameplayOption["contactmultiplier"]?.InnerText;
                    if (_objOptions.FreeContactsMultiplierEnabled)
                {
                    strContactMultiplier = _objOptions.FreeContactsMultiplier.ToString();
                }
                _intMaxKarma = Convert.ToInt32(strKarma);
                    _decMaxNuyen = Convert.ToDecimal(strNuyen);
                _intContactMultiplier = Convert.ToInt32(strContactMultiplier);
                _intContactPoints = (CHA.Base + CHA.Karma) * _intContactMultiplier;
            }
            }

            Timekeeper.Finish("load_char_cfix");
            Timekeeper.Start("load_char_maxkarmafix");
            //Fixes an issue where the quality limit was not set. In most cases this should wind up equalling 25.
            if (_intGameplayOptionQualityLimit == 0 && _intMaxKarma > 0)
            {
                _intGameplayOptionQualityLimit = _intMaxKarma;
            }
            Timekeeper.Finish("load_char_maxkarmafix");
            Timekeeper.Start("load_char_mentorspiritfix");
            // If the character doesn't have an Improvement marker that uniquely identifies what the Mentor Spirit is, create it now.
            if (Qualities.Any(q => q.Name == "Mentor Spirit") && Improvements.All(imp => imp.ImproveType != Improvement.ImprovementType.MentorSpirit))
            {
                Quality mentorQuality = Qualities.First(q => q.Name == "Mentor Spirit");
                if (!string.IsNullOrWhiteSpace(mentorQuality.Extra))
                {
                    XmlDocument doc = XmlManager.Load("mentors.xml");
                    XmlNode mentorDoc = doc.SelectSingleNode("/chummer/mentors/mentor[name = \"" + mentorQuality.Extra + "\"]");
                    ImprovementManager.CreateImprovement(this, string.Empty, Improvement.ImprovementSource.Quality, mentorQuality.InternalId,
                        Improvement.ImprovementType.MentorSpirit, mentorDoc["id"].InnerText);
                }
            }
            Timekeeper.Finish("load_char_mentorspiritfix");

            //// If the character had old Qualities that were converted, immediately save the file so they are in the new format.
            //      if (blnHasOldQualities)
            //      {
            //    Timekeeper.Start("load_char_resav");  //Lets not silently save file on load?


            //    Save();
            //    Timekeeper.Finish("load_char_resav");


            //}
            return true;
        }

        /// <summary>
        /// Print this character information to a MemoryStream. This creates only the character object itself, not any of the opening or closing XmlDocument items.
        /// This can be used to write multiple characters to a single XmlDocument.
        /// </summary>
        /// <param name="objStream">MemoryStream to use.</param>
        /// <param name="objWriter">XmlTextWriter to write to.</param>
#if DEBUG
        public void PrintToStream(MemoryStream objStream, XmlTextWriter objWriter)
#else
        public void PrintToStream(XmlTextWriter objWriter)
#endif
        {
            XmlDocument objXmlDocument;

            string strMetatype = string.Empty;
            string strMetavariant = string.Empty;
            // Get the name of the Metatype and Metavariant.
            XmlDocument objMetatypeDoc = XmlManager.Load("metatypes.xml");
            XmlNode objMetatypeNode = objMetatypeDoc.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + _strMetatype + "\"]");
                if (objMetatypeNode == null)
            {
                    objMetatypeDoc = XmlManager.Load("critters.xml");
                objMetatypeNode = objMetatypeDoc.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + _strMetatype + "\"]");
            }

            if (objMetatypeNode != null)
            {
                strMetatype = objMetatypeNode["translate"]?.InnerText ?? _strMetatype;

                if (!string.IsNullOrEmpty(_strMetavariant))
                {
                    objMetatypeNode =
                        objMetatypeNode.SelectSingleNode("metavariants/metavariant[name = \"" + _strMetavariant + "\"]");

                    if (objMetatypeNode != null)
                        strMetavariant = objMetatypeNode["translate"]?.InnerText ?? _strMetavariant;
                }
            }

            Guid guiImage = Guid.NewGuid();
            // This line left in for debugging. Write the output to a fixed file name.
            //FileStream objStream = new FileStream("D:\\temp\\print.xml", FileMode.Create, FileAccess.Write, FileShare.ReadWrite);//(_strFileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);

            // <character>
            objWriter.WriteStartElement("character");

            // <metatype />
            objWriter.WriteElementString("metatype", strMetatype);
            // <metatype_english />
            objWriter.WriteElementString("metatype_english", _strMetatype);
            // <metavariant />
            objWriter.WriteElementString("metavariant", strMetavariant);
            // <metavariant_english />
            objWriter.WriteElementString("metavariant_english", _strMetavariant);
            // <movement />
            objWriter.WriteElementString("movement", FullMovement());
            // <walk />
            objWriter.WriteElementString("walk", FullMovement());
            // <run />
            objWriter.WriteElementString("run", FullMovement());
            // <sprint />
            objWriter.WriteElementString("sprint", FullMovement());
            // <movementwalk />
            objWriter.WriteElementString("movementwalk", Movement);
            // <movementswim />
            objWriter.WriteElementString("movementswim", Swim);
            // <movementfly />
            objWriter.WriteElementString("movementfly", Fly);

            // <gameplayoption />
            objWriter.WriteElementString("gameplayoption", _strGameplayOption);
            // <maxkarma />
            objWriter.WriteElementString("maxkarma", _intMaxKarma.ToString());
            // <maxnuyen />
            objWriter.WriteElementString("maxnuyen", $"{_decMaxNuyen:###,###,##0.##}");
            // <contactmultiplier />
            objWriter.WriteElementString("contactmultiplier", _intContactMultiplier.ToString());
            // <prioritymetatype />
            objWriter.WriteElementString("prioritymetatype", _strPriorityMetatype);
            // <priorityattributes />
            objWriter.WriteElementString("priorityattributes", _strPriorityAttributes);
            // <priorityspecial />
            objWriter.WriteElementString("priorityspecial", _strPrioritySpecial);
            // <priorityskills />
            objWriter.WriteElementString("priorityskills", _strPrioritySkills);
            // <priorityresources />
            objWriter.WriteElementString("priorityresources", _strPriorityResources);
            // <priorityskills >
            objWriter.WriteStartElement("priorityskills");
            foreach (string strSkill in _lstPrioritySkills)
            {
                objWriter.WriteElementString("priorityskill", strSkill);
            }
            // </priorityskills>
            objWriter.WriteEndElement();
            // <handedness />
            objWriter.WriteElementString("primaryarm", _strPrimaryArm);

            // If the character does not have a name, call them Unnamed Character. This prevents a transformed document from having a self-terminated title tag which causes browser to not rendering anything.
            // <name />
            if (!string.IsNullOrEmpty(_strName))
                objWriter.WriteElementString("name", _strName);
            else
                objWriter.WriteElementString("name", LanguageManager.GetString("String_UnnamedCharacter"));

            // Since IE is retarded and can't handle base64 images before IE9, we need to dump the image to a temporary directory and re-write the information.
            // If you give it an extension of jpg, gif, or png, it expects the file to be in that format and won't render the image unless it was originally that type.
            // But if you give it the extension img, it will render whatever you give it (which doesn't make any damn sense, but that's IE for you).
                string mugshotsDirectoryPath = Path.Combine(Application.StartupPath, "mugshots");
            if (!Directory.Exists(mugshotsDirectoryPath))
            {
                try
                {
                    Directory.CreateDirectory(mugshotsDirectoryPath);
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_Insufficient_Permissions_Warning"));
                }
            }
            // <mainmugshotpath />
            if (MainMugshot.Length > 0)
            {
                byte[] bytImage = Convert.FromBase64String(MainMugshot);
                MemoryStream objImageStream = new MemoryStream(bytImage, 0, bytImage.Length);
                objImageStream.Write(bytImage, 0, bytImage.Length);
                Image imgMugshot = Image.FromStream(objImageStream, true);
                string imgMugshotPath = Path.Combine(mugshotsDirectoryPath, guiImage.ToString() + ".img");
                imgMugshot.Save(imgMugshotPath);
                objWriter.WriteElementString("mainmugshotpath",
                    "file://" + imgMugshotPath.Replace(Path.DirectorySeparatorChar, '/'));
                // <mainmugshotbase64 />
                objWriter.WriteElementString("mainmugshotbase64", MainMugshot);
                // <othermugshots>
                objWriter.WriteElementString("hasothermugshots", Mugshots.Count > 1 ? "yes" : "no");
                objWriter.WriteStartElement("othermugshots");
                int i = 0;
                foreach (string strMugshot in Mugshots)
                {
                    if (strMugshot == MainMugshot)
                        continue;
                    objWriter.WriteStartElement("mugshot");
                    objWriter.WriteElementString("stringbase64", strMugshot);

                    bytImage = Convert.FromBase64String(strMugshot);
                    objImageStream = new MemoryStream(bytImage, 0, bytImage.Length);
                    objImageStream.Write(bytImage, 0, bytImage.Length);
                    imgMugshot = Image.FromStream(objImageStream, true);
                    imgMugshotPath = Path.Combine(mugshotsDirectoryPath, guiImage.ToString() + i.ToString() + ".img");
                    imgMugshot.Save(imgMugshotPath);
                    objWriter.WriteElementString("temppath", "file://" + imgMugshotPath.Replace(Path.DirectorySeparatorChar, '/'));

                    objWriter.WriteEndElement();
                    ++i;
                }
                // </mugshots>
                objWriter.WriteEndElement();
            }
            // <sex />
            objWriter.WriteElementString("sex", _strSex);
            // <age />
            objWriter.WriteElementString("age", _strAge);
            // <eyes />
            objWriter.WriteElementString("eyes", _strEyes);
            // <height />
            objWriter.WriteElementString("height", _strHeight);
            // <weight />
            objWriter.WriteElementString("weight", _strWeight);
            // <skin />
            objWriter.WriteElementString("skin", _strSkin);
            // <hair />
            objWriter.WriteElementString("hair", _strHair);
            // <description />
            objWriter.WriteElementString("description", _strDescription);
            // <background />
            objWriter.WriteElementString("background", _strBackground);
            // <concept />
            objWriter.WriteElementString("concept", _strConcept);
            // <notes />
            objWriter.WriteElementString("notes", _strNotes);
            // <alias />
            objWriter.WriteElementString("alias", _strAlias);
            // <playername />
            objWriter.WriteElementString("playername", _strPlayerName);
            // <gamenotes />
            objWriter.WriteElementString("gamenotes", _strGameNotes);

            // <limitphysical />
            objWriter.WriteElementString("limitphysical", LimitPhysical.ToString());
            // <limitmental />
            objWriter.WriteElementString("limitmental", LimitMental.ToString());
            // <limitsocial />
            objWriter.WriteElementString("limitsocial", LimitSocial.ToString());
            // <limitastral />
            objWriter.WriteElementString("limitastral", LimitAstral.ToString());
            // <contactpoints />
            objWriter.WriteElementString("contactpoints", _intContactPoints.ToString());
            // <contactpointsused />
            objWriter.WriteElementString("contactpointsused", _intContactPointsUsed.ToString());
            // <cfplimit />
            objWriter.WriteElementString("cfplimit", _intCFPLimit.ToString());
            // <totalaiprogramlimit />
            objWriter.WriteElementString("ainormalprogramlimit", _intAINormalProgramLimit.ToString());
            // <aiadvancedprogramlimit />
            objWriter.WriteElementString("aiadvancedprogramlimit", _intAIAdvancedProgramLimit.ToString());
            // <spelllimit />
            objWriter.WriteElementString("spelllimit", _intSpellLimit.ToString());
            // <karma />
            objWriter.WriteElementString("karma", _intKarma.ToString());
            // <totalkarma />
            objWriter.WriteElementString("totalkarma", $"{CareerKarma:###,###,##0}");
            // <special />
            objWriter.WriteElementString("special", _intSpecial.ToString());
            // <totalspecial />
            objWriter.WriteElementString("totalspecial", $"{_intTotalSpecial:###,###,##0}");
            // <attributes />
            objWriter.WriteElementString("attributes", _intSpecial.ToString());
            // <totalattributes />
            objWriter.WriteElementString("totalattributes", $"{_intTotalAttributes:###,###,##0}");
            // <streetcred />
            objWriter.WriteElementString("streetcred", _intStreetCred.ToString());
            // <calculatedstreetcred />
            objWriter.WriteElementString("calculatedstreetcred", CalculatedStreetCred.ToString());
            // <totalstreetcred />
            objWriter.WriteElementString("totalstreetcred", TotalStreetCred.ToString());
            // <burntstreetcred />
            objWriter.WriteElementString("burntstreetcred", _intBurntStreetCred.ToString());
            // <notoriety />
            objWriter.WriteElementString("notoriety", _intNotoriety.ToString());
            // <calculatednotoriety />
            objWriter.WriteElementString("calculatednotoriety", CalculatedNotoriety.ToString());
            // <totalnotoriety />
            objWriter.WriteElementString("totalnotoriety", TotalNotoriety.ToString());
            // <publicawareness />
            objWriter.WriteElementString("publicawareness", _intPublicAwareness.ToString());
            // <calculatedpublicawareness />
            objWriter.WriteElementString("calculatedpublicawareness", CalculatedPublicAwareness.ToString());
            // <totalpublicawareness />
            objWriter.WriteElementString("totalpublicawareness", TotalPublicAwareness.ToString());
            // <created />
            objWriter.WriteElementString("created", _blnCreated.ToString());
            // <nuyen />
            objWriter.WriteElementString("nuyen", $"{_decNuyen:###,###,##0.##}");
            // <adeptwaydiscount />
            objWriter.WriteElementString("adeptwaydiscount", _intAdeptWayDiscount.ToString());
            // <adept />
            objWriter.WriteElementString("adept", _blnAdeptEnabled.ToString());
            // <magician />
            objWriter.WriteElementString("magician", _blnMagicianEnabled.ToString());
            // <technomancer />
            objWriter.WriteElementString("technomancer", _blnTechnomancerEnabled.ToString());
            // <ai />
            objWriter.WriteElementString("ai", _blnAdvancedProgramsEnabled.ToString());
            // <cyberwaredisabled />
            objWriter.WriteElementString("cyberwaredisabled", _blnCyberwareDisabled.ToString());
            // <critter />
            objWriter.WriteElementString("critter", _blnCritterEnabled.ToString());
            objWriter.WriteElementString("totaless", Essence.ToString(GlobalOptions.InvariantCultureInfo));
            // <tradition />
            string strTraditionName = MagicTradition;
            if (strTraditionName == "Custom")
                strTraditionName = TraditionName;
            objWriter.WriteStartElement("tradition");

            if (!string.IsNullOrEmpty(strTraditionName))
            {
                string strDrainAtt = TraditionDrain;
                objXmlDocument = XmlManager.Load("traditions.xml");

                XmlNode objXmlTradition = objXmlDocument.SelectSingleNode("/chummer/traditions/tradition[name = \"" + _strMagicTradition + "\"]");

                if (objXmlTradition != null)
                {
                    if (objXmlTradition["name"] != null && objXmlTradition["name"].InnerText != "Custom")
                    {
                        strTraditionName = objXmlTradition["translate"]?.InnerText ?? objXmlTradition["name"].InnerText;
                    }
                }

                XPathNavigator nav = objXmlDocument.CreateNavigator();
                string strDrain = AttributeSection.AttributeStrings.Select(GetAttribute).Aggregate(strDrainAtt, (current, objAttrib) => current.Replace(objAttrib.Abbrev, objAttrib.TotalValue.ToString()));
				if (string.IsNullOrEmpty(strDrain))
                {
                    strDrain = "0";
                }
                XPathExpression xprDrain = nav.Compile(strDrain);

                // Add any Improvements for Drain Resistance.
                int intDrain = Convert.ToInt32(nav.Evaluate(xprDrain)) + ImprovementManager.ValueOf(this, Improvement.ImprovementType.DrainResistance);

                objWriter.WriteElementString("drain", strDrainAtt + " (" + intDrain.ToString() + ")");
                objWriter.WriteStartElement("drainattribute");
                foreach (string drainAttribute in strDrainAtt.Replace('+', ' ').Split(new [] {' '} , StringSplitOptions.RemoveEmptyEntries))
                {
                    objWriter.WriteElementString("attr",drainAttribute);
                }
                objWriter.WriteEndElement();
                if (MagicTradition == "Draconic")
                {
                    objWriter.WriteElementString("spiritcombat", LanguageManager.GetString("String_All"));
                    objWriter.WriteElementString("spiritdetection", LanguageManager.GetString("String_All"));
                    objWriter.WriteElementString("spirithealth", LanguageManager.GetString("String_All"));
                    objWriter.WriteElementString("spiritillusion", LanguageManager.GetString("String_All"));
                    objWriter.WriteElementString("spiritmanipulation", LanguageManager.GetString("String_All"));
                }
                else if (MagicTradition != "Custom")
                {
                    objWriter.WriteElementString("spiritcombat",
                        objXmlTradition.SelectSingleNode("spirits/spiritcombat").InnerText);
                    objWriter.WriteElementString("spiritdetection",
                        objXmlTradition.SelectSingleNode("spirits/spiritdetection").InnerText);
                    objWriter.WriteElementString("spirithealth",
                        objXmlTradition.SelectSingleNode("spirits/spirithealth").InnerText);
                    objWriter.WriteElementString("spiritillusion",
                        objXmlTradition.SelectSingleNode("spirits/spiritillusion").InnerText);
                    objWriter.WriteElementString("spiritmanipulation",
                        objXmlTradition.SelectSingleNode("spirits/spiritmanipulation").InnerText);
                }
                else
                {
                    objWriter.WriteElementString("spiritcombat", SpiritCombat);
                    objWriter.WriteElementString("spiritdetection", SpiritDetection);
                    objWriter.WriteElementString("spirithealth", SpiritHealth);
                    objWriter.WriteElementString("spiritillusion", SpiritIllusion);
                    objWriter.WriteElementString("spiritmanipulation", SpiritManipulation);
                }

                //Spirit form, default to materialization unless field with other data persists
                string strSpiritForm = "Materialization";
                objXmlTradition.TryGetStringFieldQuickly("spiritform", ref strSpiritForm);
                objWriter.WriteElementString("spiritform", strSpiritForm);

                //Rulebook reference
                string strSource = string.Empty;
                string strPage = string.Empty;
                objXmlTradition.TryGetStringFieldQuickly("source", ref strSource);
                objXmlTradition.TryGetStringFieldQuickly("page", ref strPage);

                objWriter.WriteElementString("source", strSource);
                objWriter.WriteElementString("page", strPage);
            }
            objWriter.WriteElementString("name", strTraditionName);
            objWriter.WriteEndElement();

            // <stream />
            objWriter.WriteElementString("stream", TechnomancerStream);
            if (!string.IsNullOrEmpty(TechnomancerStream))
            {
                string strDrainAtt = string.Empty;
                objXmlDocument = new XmlDocument();
                XPathNavigator nav = objXmlDocument.CreateNavigator();
                string strDrain = AttributeSection.AttributeStrings.Select(GetAttribute).Aggregate(strDrainAtt, (current, objAttrib) => current.Replace(objAttrib.Abbrev, objAttrib.TotalValue.ToString()));
                if (string.IsNullOrEmpty(strDrain))
                    strDrain = "0";
                XPathExpression xprDrain = nav.Compile(strDrain);

                // Add any Improvements for Fading Resistance.
                int intDrain = Convert.ToInt32(nav.Evaluate(xprDrain)) + ImprovementManager.ValueOf(this, Improvement.ImprovementType.FadingResistance);

                objWriter.WriteElementString("drain", strDrainAtt + " (" + intDrain.ToString() + ")");
            }

            // <attributes>
            objWriter.WriteStartElement("attributes");
	        AttributeSection.Print(objWriter);

            // </attributes>
            objWriter.WriteEndElement();

            // <armor />
            objWriter.WriteElementString("armor", TotalArmorRating.ToString());
            // <firearmor />
            objWriter.WriteElementString("firearmor", TotalFireArmorRating.ToString());
            // <coldarmor />
            objWriter.WriteElementString("coldarmor", TotalColdArmorRating.ToString());
            // <electricityarmor />
            objWriter.WriteElementString("electricityarmor", TotalElectricityArmorRating.ToString());
            // <acidarmor />
            objWriter.WriteElementString("acidarmor", TotalAcidArmorRating.ToString());
            // <fallingarmor />
            objWriter.WriteElementString("fallingarmor", TotalFallingArmorRating.ToString());
            // <armordicestun />
            objWriter.WriteElementString("armordicestun", (BOD.TotalValue + TotalArmorRating).ToString());
            // <firearmordicestun />
            objWriter.WriteElementString("firearmordicestun", (BOD.TotalValue + TotalFireArmorRating).ToString());
            // <coldarmordicestun />
            objWriter.WriteElementString("coldarmordicestun", (BOD.TotalValue + TotalColdArmorRating).ToString());
            // <electricityarmordicestun />
            objWriter.WriteElementString("electricityarmordicestun", (BOD.TotalValue + TotalElectricityArmorRating).ToString());
            // <acidarmordicestun />
            objWriter.WriteElementString("acidarmordicestun", (BOD.TotalValue + TotalAcidArmorRating).ToString());
            // <fallingarmordicestun />
            objWriter.WriteElementString("fallingarmordicestun", (BOD.TotalValue + TotalFallingArmorRating).ToString());
            // <armordicephysical />
            objWriter.WriteElementString("armordicephysical", (BOD.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.DamageResistance) + TotalArmorRating).ToString());
            // <firearmordicephysical />
            objWriter.WriteElementString("firearmordicephysical", (BOD.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.DamageResistance) + TotalFireArmorRating).ToString());
            // <coldarmordicephysical />
            objWriter.WriteElementString("coldarmordicephysical", (BOD.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.DamageResistance) + TotalColdArmorRating).ToString());
            // <electricityarmordicephysical />
            objWriter.WriteElementString("electricityarmordicephysical", (BOD.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.DamageResistance) + TotalElectricityArmorRating).ToString());
            // <acidarmordicephysical />
            objWriter.WriteElementString("acidarmordicephysical", (BOD.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.DamageResistance) + TotalAcidArmorRating).ToString());
            // <fallingarmordicephysical />
            objWriter.WriteElementString("fallingarmordicephysical", (BOD.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.DamageResistance) + TotalFallingArmorRating).ToString());

            // Condition Monitors.
            // <physicalcm />
            objWriter.WriteElementString("physicalcm", PhysicalCM.ToString());
            // <stuncm />
            objWriter.WriteElementString("stuncm", StunCM.ToString());

            // Condition Monitor Progress.
            // <physicalcmfilled />
            objWriter.WriteElementString("physicalcmfilled", _intPhysicalCMFilled.ToString());
            // <stuncmfilled />
            objWriter.WriteElementString("stuncmfilled", _intStunCMFilled.ToString());

            // <cmthreshold>
            objWriter.WriteElementString("cmthreshold", CMThreshold.ToString());
            // <cmthresholdoffset>
            objWriter.WriteElementString("physicalcmthresholdoffset", PhysicalCMThresholdOffset.ToString());
            // <cmthresholdoffset>
            objWriter.WriteElementString("stuncmthresholdoffset", StunCMThresholdOffset.ToString());
            // <cmoverflow>
            objWriter.WriteElementString("cmoverflow", CMOverflow.ToString());

            // Calculate Initiatives.
            // Initiative.
            objWriter.WriteElementString("init", Initiative);
            objWriter.WriteElementString("initdice", InitiativeDice.ToString());
            objWriter.WriteElementString("initvalue", InitiativeValue.ToString());
            objWriter.WriteElementString("initbonus", Math.Max(ImprovementManager.ValueOf(this, Improvement.ImprovementType.Initiative), 0).ToString());

            // Astral Initiative.
            if (MAGEnabled)
            {
                objWriter.WriteElementString("astralinit", AstralInitiative);
                objWriter.WriteElementString("astralinitdice", AstralInitiativeDice.ToString());
                objWriter.WriteElementString("astralinitvalue", AstralInitiativeValue.ToString());
            }

            // Matrix Initiative (AR).
            objWriter.WriteElementString("matrixarinit", MatrixInitiative);
            objWriter.WriteElementString("matrixarinitdice", MatrixInitiativeDice.ToString());
            objWriter.WriteElementString("matrixarinitvalue", MatrixInitiativeValue.ToString());

            // Matrix Initiative (Cold).
            objWriter.WriteElementString("matrixcoldinit", MatrixInitiativeCold);
            objWriter.WriteElementString("matrixcoldinitdice", MatrixInitiativeDice.ToString());
            objWriter.WriteElementString("matrixcoldinitvalue", MatrixInitiativeValue.ToString());

            // Matrix Initiative (Hot).
            objWriter.WriteElementString("matrixhotinit", MatrixInitiativeHot);
            objWriter.WriteElementString("matrixhotinitdice", MatrixInitiativeDice.ToString());
            objWriter.WriteElementString("matrixhotinitvalue", MatrixInitiativeValue.ToString());

            // Rigger Initiative.
            objWriter.WriteElementString("riggerinit", Initiative);

            // <magenabled />
            objWriter.WriteElementString("magenabled", _blnMAGEnabled.ToString());
            // <initiategrade />
            objWriter.WriteElementString("initiategrade", _intInitiateGrade.ToString());
            // <resenabled />
            objWriter.WriteElementString("resenabled", _blnRESEnabled.ToString());
            // <submersiongrade />
            objWriter.WriteElementString("submersiongrade", _intSubmersionGrade.ToString());
            // <depenabled />
            objWriter.WriteElementString("depenabled", _blnDEPEnabled.ToString());
            // <groupmember />
            objWriter.WriteElementString("groupmember", _blnGroupMember.ToString());
            // <groupname />
            objWriter.WriteElementString("groupname", _strGroupName);
            // <groupnotes />
            objWriter.WriteElementString("groupnotes", _strGroupNotes);

            // <composure />
            objWriter.WriteElementString("composure", Composure.ToString());
            // <judgeintentions />
            objWriter.WriteElementString("judgeintentions", JudgeIntentions.ToString());
            // <judgeintentionsresist />
            objWriter.WriteElementString("judgeintentionsresist", JudgeIntentionsResist.ToString());
            // <liftandcarry />
            objWriter.WriteElementString("liftandcarry", LiftAndCarry.ToString());
            // <memory />
            objWriter.WriteElementString("memory", Memory.ToString());
            // <liftweight />
            objWriter.WriteElementString("liftweight", (STR.TotalValue * 15).ToString());
            // <carryweight />
            objWriter.WriteElementString("carryweight", (STR.TotalValue * 10).ToString());
            // <fatigueresist />
            objWriter.WriteElementString("fatigueresist", FatigueResist.ToString());
            // <radiationresist />
            objWriter.WriteElementString("radiationresist", RadiationResist.ToString());
            // <sonicresist />
            objWriter.WriteElementString("sonicresist", SonicResist.ToString());
            // <toxincontacttesist />
            objWriter.WriteElementString("toxincontacttesist", ToxinContactResist.ToString());
            // <toxiningestionresist />
            objWriter.WriteElementString("toxiningestionresist", ToxinIngestionResist.ToString());
            // <toxininhalationresist />
            objWriter.WriteElementString("toxininhalationresist", ToxinInhalationResist.ToString());
            // <toxininjectionresist />
            objWriter.WriteElementString("toxininjectionresist", ToxinInjectionResist.ToString());
            // <pathogencontactresist />
            objWriter.WriteElementString("pathogencontactresist", PathogenContactResist.ToString());
            // <pathogeningestionresist />
            objWriter.WriteElementString("pathogeningestionresist", PathogenIngestionResist.ToString());
            // <pathogeninhalationresist />
            objWriter.WriteElementString("pathogeninhalationresist", PathogenInhalationResist.ToString());
            // <pathogeninjectionresist />
            objWriter.WriteElementString("pathogeninjectionresist", PathogenInjectionResist.ToString());
            // <physiologicaladdictionresistfirsttime />
            objWriter.WriteElementString("physiologicaladdictionresistfirsttime", PhysiologicalAddictionResistFirstTime.ToString());
            // <physiologicaladdictionresistalreadyaddicted />
            objWriter.WriteElementString("physiologicaladdictionresistalreadyaddicted", PhysiologicalAddictionResistAlreadyAddicted.ToString());
            // <psychologicaladdictionresistfirsttime />
            objWriter.WriteElementString("psychologicaladdictionresistfirsttime", PsychologicalAddictionResistFirstTime.ToString());
            // <psychologicaladdictionresistalreadyaddicted />
            objWriter.WriteElementString("psychologicaladdictionresistalreadyaddicted", PsychologicalAddictionResistAlreadyAddicted.ToString());
            // <physicalcmnaturalrecovery />
            objWriter.WriteElementString("physicalcmnaturalrecovery", PhysicalCMNaturalRecovery.ToString());
            // <stuncmnaturalrecovery />
            objWriter.WriteElementString("stuncmnaturalrecovery", StunCMNaturalRecovery.ToString());

            // <skills>
            objWriter.WriteStartElement("skills");
            SkillsSection.Print(objWriter);
            objWriter.WriteEndElement();

            // <contacts>
            objWriter.WriteStartElement("contacts");
            foreach (Contact objContact in _lstContacts)
            {
                objContact.Print(objWriter);
            }
            // </contacts>
            objWriter.WriteEndElement();

            // <limitmodifiersphys>
            objWriter.WriteStartElement("limitmodifiersphys");
            foreach (LimitModifier objLimitModifier in _lstLimitModifiers.Where(objLimitModifier => objLimitModifier.Limit == "Physical"))
            {
                    objLimitModifier.Print(objWriter);
            }
            // Populate Limit Modifiers from Improvements
            foreach (Improvement objImprovement in _lstImprovements.Where(objImprovement => (objImprovement.ImproveType == Improvement.ImprovementType.LimitModifier && objImprovement.ImprovedName == "Physical")))
            {
                string strName = objImprovement.UniqueName + ": ";
                if (objImprovement.Value > 0)
                    strName += "+";
                strName += objImprovement.Value.ToString();

                if (!string.IsNullOrEmpty(objImprovement.Condition))
                    strName += ", " + objImprovement.Condition;

                objWriter.WriteStartElement("limitmodifier");
                objWriter.WriteElementString("name", strName);
                if (Options.PrintNotes)
                    objWriter.WriteElementString("notes", objImprovement.Notes);
                objWriter.WriteEndElement();
            }
            // </limitmodifiersphys>
            objWriter.WriteEndElement();

            // <limitmodifiersment>
            objWriter.WriteStartElement("limitmodifiersment");
            foreach (LimitModifier objLimitModifier in _lstLimitModifiers.Where(objLimitModifier => objLimitModifier.Limit == "Mental"))
            {
                    objLimitModifier.Print(objWriter);
            }
            // Populate Limit Modifiers from Improvements
            foreach (Improvement objImprovement in _lstImprovements.Where(objImprovement => (objImprovement.ImproveType == Improvement.ImprovementType.LimitModifier && objImprovement.ImprovedName == "Mental")))
            {
                string strName = objImprovement.UniqueName + ": ";
                if (objImprovement.Value > 0)
                    strName += "+";
                strName += objImprovement.Value.ToString();

                if (!string.IsNullOrEmpty(objImprovement.Condition))
                    strName += ", " + objImprovement.Condition;

                objWriter.WriteStartElement("limitmodifier");
                objWriter.WriteElementString("name", strName);
                if (Options.PrintNotes)
                    objWriter.WriteElementString("notes", objImprovement.Notes);
                objWriter.WriteEndElement();
            }
            // </limitmodifiersment>
            objWriter.WriteEndElement();

            // <limitmodifierssoc>
            objWriter.WriteStartElement("limitmodifierssoc");
            foreach (LimitModifier objLimitModifier in _lstLimitModifiers.Where(objLimitModifier => objLimitModifier.Limit == "Social"))
            {
                    objLimitModifier.Print(objWriter);
            }
            // Populate Limit Modifiers from Improvements
            foreach (Improvement objImprovement in _lstImprovements.Where(objImprovement => (objImprovement.ImproveType == Improvement.ImprovementType.LimitModifier && objImprovement.ImprovedName == "Social")))
            {
                string strName = objImprovement.UniqueName + ": ";
                if (objImprovement.Value > 0)
                    strName += "+";
                strName += objImprovement.Value.ToString();

                if (!string.IsNullOrEmpty(objImprovement.Condition))
                    strName += ", " + objImprovement.Condition;

                objWriter.WriteStartElement("limitmodifier");
                objWriter.WriteElementString("name", strName);
                if (Options.PrintNotes)
                    objWriter.WriteElementString("notes", objImprovement.Notes);
                objWriter.WriteEndElement();
            }
            // </limitmodifierssoc>
            objWriter.WriteEndElement();

            // <spells>
            objWriter.WriteStartElement("spells");
            foreach (Spell objSpell in _lstSpells)
            {
                objSpell.Print(objWriter);
            }
            // </spells>
            objWriter.WriteEndElement();

            // <powers>
            objWriter.WriteStartElement("powers");
            foreach (Power objPower in _lstPowers)
            {
                objPower.Print(objWriter);
            }
            // </powers>
            objWriter.WriteEndElement();

            // <spirits>
            objWriter.WriteStartElement("spirits");
            foreach (Spirit objSpirit in _lstSpirits)
            {
                objSpirit.Print(objWriter);
            }
            // </spirits>
            objWriter.WriteEndElement();

            // <complexforms>
            objWriter.WriteStartElement("complexforms");
            foreach (ComplexForm objProgram in _lstComplexForms)
            {
                objProgram.Print(objWriter);
            }
            // </complexforms>
            objWriter.WriteEndElement();

            // <aiprograms>
            objWriter.WriteStartElement("aiprograms");
            foreach (AIProgram objProgram in _lstAIPrograms)
            {
                objProgram.Print(objWriter);
            }
            // </aiprograms>
            objWriter.WriteEndElement();

            // <martialarts>
            objWriter.WriteStartElement("martialarts");
            foreach (MartialArt objMartialArt in _lstMartialArts)
            {
                objMartialArt.Print(objWriter);
            }
            // </martialarts>
            objWriter.WriteEndElement();

            // <martialartmaneuvers>
            objWriter.WriteStartElement("martialartmaneuvers");
            foreach (MartialArtManeuver objManeuver in _lstMartialArtManeuvers)
            {
                objManeuver.Print(objWriter);
            }
            // </martialartmaneuvers>
            objWriter.WriteEndElement();

            // <armors>
            objWriter.WriteStartElement("armors");
            foreach (Armor objArmor in _lstArmor)
            {
                objArmor.Print(objWriter);
            }
            // </armors>
            objWriter.WriteEndElement();

            // <weapons>
            objWriter.WriteStartElement("weapons");
            foreach (Weapon objWeapon in _lstWeapons)
            {
                objWeapon.Print(objWriter);
            }
            // </weapons>
            objWriter.WriteEndElement();

            // <cyberwares>
            objWriter.WriteStartElement("cyberwares");
            foreach (Cyberware objCyberware in _lstCyberware)
            {
                objCyberware.Print(objWriter);
            }
            // </cyberwares>
            objWriter.WriteEndElement();

            // Load the Qualities file so we can figure out whether or not each Quality should be printed.
            XmlManager.Load("qualities.xml");

            // <qualities>
            // Multiple instances of the same quality are combined into just one entry with a number next to it (e.g. 6 discrete entries of "Focused Concentration" become "Focused Concentration 6")
            Dictionary<string, int> strQualitiesToPrint = new Dictionary<string, int>(_lstQualities.Count);
            foreach (Quality objQuality in _lstQualities)
            {
                if (strQualitiesToPrint.ContainsKey(objQuality.QualityId + " " + objQuality.SourceName + " " + objQuality.Extra))
                {
                    strQualitiesToPrint[objQuality.QualityId + " " + objQuality.SourceName + " " + objQuality.Extra] += 1;
                }
                else
                {
                    strQualitiesToPrint.Add(objQuality.QualityId + " " + objQuality.SourceName + " " + objQuality.Extra, 1);
                }
            }
            int intLoopRating = 0;
            objWriter.WriteStartElement("qualities");
            foreach (Quality objQuality in _lstQualities)
            {
                if (strQualitiesToPrint.TryGetValue(objQuality.QualityId + " " + objQuality.SourceName + " " + objQuality.Extra, out intLoopRating))
                {
                    objQuality.Print(objWriter, intLoopRating);
                    strQualitiesToPrint.Remove(objQuality.QualityId + " " + objQuality.SourceName + " " + objQuality.Extra);
                }
            }
            // </qualities>
            objWriter.WriteEndElement();

            // <lifestyles>
            objWriter.WriteStartElement("lifestyles");
            foreach (Lifestyle objLifestyle in _lstLifestyles)
            {
                objLifestyle.Print(objWriter);
            }
            // </lifestyles>
            objWriter.WriteEndElement();

            // <gears>
            objWriter.WriteStartElement("gears");
            foreach (Gear objGear in _lstGear)
            {
                // Use the Gear's SubClass if applicable.
                if (objGear.GetType() == typeof(Commlink))
                //if (objGear.Category == "Commlinks" || objGear.Category == "Rigger Command Consoles" || objGear.Category == "Cyberdecks" || objGear.GetType() == typeof(Commlink))
                {
                    Commlink objCommlink = (Commlink)objGear;
                    objCommlink.Print(objWriter);
                }
                else
                {
                    objGear.Print(objWriter);
                }
            }
            // If the character is a Technomancer, write out the Living Persona "Commlink".
            if (_blnTechnomancerEnabled)
            {
                Commlink objLivingPersona = new Commlink(this);
                objLivingPersona.Name = LanguageManager.GetString("String_LivingPersona");
                objLivingPersona.Category = LanguageManager.GetString("String_Commlink");
                objLivingPersona.DeviceRating = RES.TotalValue.ToString(GlobalOptions.InvariantCultureInfo);
                objLivingPersona.Attack = CHA.TotalValue.ToString(GlobalOptions.InvariantCultureInfo);
                objLivingPersona.Sleaze = INT.TotalValue.ToString(GlobalOptions.InvariantCultureInfo);
                objLivingPersona.DataProcessing = LOG.TotalValue.ToString(GlobalOptions.InvariantCultureInfo);
                objLivingPersona.Firewall = WIL.TotalValue.ToString(GlobalOptions.InvariantCultureInfo);
                objLivingPersona.Source = _objOptions.LanguageBookShort("SR5");
                objLivingPersona.Page = "251";
                objLivingPersona.IsLivingPersona = true;

                objLivingPersona.Print(objWriter);
            }
            // </gears>
            objWriter.WriteEndElement();

            // <vehicles>
            objWriter.WriteStartElement("vehicles");
            foreach (Vehicle objVehicle in _lstVehicles)
            {
                objVehicle.Print(objWriter);
            }
            // </vehicles>
            objWriter.WriteEndElement();

            // <metamagics>
            objWriter.WriteStartElement("metamagics");
            foreach (Metamagic objMetamagic in _lstMetamagics)
            {
                objMetamagic.Print(objWriter);
            }
            // </metamagics>
            objWriter.WriteEndElement();

            // <arts>
            objWriter.WriteStartElement("arts");
            foreach (Art objArt in _lstArts)
            {
                objArt.Print(objWriter);
            }
            // </arts>
            objWriter.WriteEndElement();

            // <enhancements>
            objWriter.WriteStartElement("enhancements");
            foreach (Enhancement objEnhancement in _lstEnhancements)
            {
                objEnhancement.Print(objWriter);
            }
            // </enhancements>
            objWriter.WriteEndElement();

            // <critterpowers>
            objWriter.WriteStartElement("critterpowers");
            foreach (CritterPower objPower in _lstCritterPowers)
            {
                objPower.Print(objWriter);
            }
            // </critterpowers>
            objWriter.WriteEndElement();

            // <calendar>
            objWriter.WriteStartElement("calendar");
            //_lstCalendar.Sort();
            foreach (CalendarWeek objWeek in _lstCalendar)
                objWeek.Print(objWriter, Options.PrintNotes);
            // </expenses>
            objWriter.WriteEndElement();

            // Print the Expense Log Entries if the option is enabled.
            if (_objOptions.PrintExpenses)
            {
                // <expenses>
                objWriter.WriteStartElement("expenses");
                _lstExpenseLog.Sort(ExpenseLogEntry.CompareDate);
                foreach (ExpenseLogEntry objExpense in _lstExpenseLog)
                    objExpense.Print(objWriter);
                // </expenses>
                objWriter.WriteEndElement();
            }

            // </character>
            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Print this character and open the View Character window.
        /// </summary>
        /// <param name="blnDialog">Whether or not the window should be shown as a dialogue window.</param>
        public void Print(bool blnDialog = true)
        {
            // Write the Character information to a MemoryStream so we don't need to create any files.
            MemoryStream objStream = new MemoryStream();
            XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.UTF8);

            // Being the document.
            objWriter.WriteStartDocument();

            // </characters>
            objWriter.WriteStartElement("characters");

#if DEBUG
            PrintToStream(objStream, objWriter);
#else
            PrintToStream(objWriter);
#endif

            // </characters>
            objWriter.WriteEndElement();

            // Finish the document and flush the Writer and Stream.
            objWriter.WriteEndDocument();
            objWriter.Flush();
            objStream.Flush();

            // Read the stream.
            StreamReader objReader = new StreamReader(objStream);
            objStream.Position = 0;
            XmlDocument objCharacterXML = new XmlDocument();

            // Put the stream into an XmlDocument and send it off to the Viewer.
            string strXML = objReader.ReadToEnd();
            objCharacterXML.LoadXml(strXML);

            objWriter.Close();
            objStream.Close();

            // If a reference to the Viewer window does not yet exist for this character, open a new Viewer window and set the reference to it.
            // If a Viewer window already exists for this character, use it instead.
            if (_frmPrintView == null)
            {
                List<Character> lstCharacters = new List<Character>
                {
                    this
                };
                frmViewer frmViewCharacter = new frmViewer();
                frmViewCharacter.Characters = lstCharacters;
                frmViewCharacter.CharacterXML = objCharacterXML;
                _frmPrintView = frmViewCharacter;
                if (blnDialog)
                    frmViewCharacter.ShowDialog();
                else
                    frmViewCharacter.Show();
            }
            else
            {
                _frmPrintView.Activate();
                _frmPrintView.RefreshView();
            }
        }

        /// <summary>
        /// Reset all of the Character information and start from scratch.
        /// </summary>
        private void ResetCharacter()
        {
            _intBuildPoints = 800;
            _intSumtoTen = 10;


            _decNuyenMaximumBP = 50;
            _intSpellLimit = 0;
            _intCFPLimit = 0;
            _intAINormalProgramLimit = 0;
            _intAIAdvancedProgramLimit = 0;
            _intContactPoints = 0;
            _intContactPointsUsed = 0;
            _intKarma = 0;
            _intSpecial = 0;
            _intTotalSpecial = 0;
            _intAttributes = 0;
            _intTotalAttributes = 0;
            _intGameplayOptionQualityLimit = 25;

            // Reset Metatype Information.
            _strMetatype = string.Empty;
            _strMetavariant = string.Empty;
            _strMetatypeCategory = string.Empty;
            _intMetatypeBP = 0;
            _strMovement = string.Empty;

            // Reset Special Tab Flags.
            _blnAdeptEnabled = false;
            _blnMagicianEnabled = false;
            _blnTechnomancerEnabled = false;
            _blnAdvancedProgramsEnabled = false;
            _blnCyberwareDisabled = false;
            _blnInitiationEnabled = false;
            _blnCritterEnabled = false;

            // Reset Attributes.
	        AttributeSection.Reset();
			_blnMAGEnabled = false;
            _blnRESEnabled = false;
            _blnDEPEnabled = false;
            _blnGroupMember = false;
            _strGroupName = string.Empty;
            _strGroupNotes = string.Empty;
            _intInitiateGrade = 0;
            _intSubmersionGrade = 0;

            _intMAGAdept = 0;
            _intMAGMagician = 0;
            _strMagicTradition = string.Empty;
            _strTechnomancerStream = string.Empty;

            // Reset all of the Lists.
            // This kills the GC
            _lstImprovements = new List<Improvement>();

            _lstContacts = new List<Contact>();
            _lstSpirits = new List<Spirit>();
            _lstSpells = new List<Spell>();
            _lstFoci = new List<Focus>();
            _lstStackedFoci = new List<StackedFocus>();
            _lstPowers = new BindingList<Power>();
            _lstComplexForms = new List<ComplexForm>();
            _lstAIPrograms = new List<AIProgram>();
            _lstMartialArts = new List<MartialArt>();
            _lstMartialArtManeuvers = new List<MartialArtManeuver>();
            _lstLimitModifiers = new List<LimitModifier>();
            _lstArmor = new List<Armor>();
            _lstCyberware = new List<Cyberware>();
            _lstMetamagics = new List<Metamagic>();
            _lstArts = new List<Art>();
            _lstEnhancements = new List<Enhancement>();
            _lstWeapons = new List<Weapon>();
            _lstLifestyles = new List<Lifestyle>();
            _lstGear = new List<Gear>();
            _lstVehicles = new List<Vehicle>();
            _lstExpenseLog = new List<ExpenseLogEntry>();
            _lstCritterPowers = new List<CritterPower>();
            _lstInitiationGrades = new List<InitiationGrade>();
            _lstQualities = new List<Quality>();
            _lstOldQualities = new List<string>();
            _lstCalendar = new List<CalendarWeek>();


            SkillsSection.Reset();
        }
#endregion

#region Helper Methods
        /// <summary>
        /// Collate and save the character's used sourcebooks. This list is cleared after loading a character to ensure that only the current items are stored.
        /// </summary>
        public void SourceProcess(string strInput)
        {
            if (!_lstSources.Contains(strInput))
            {
                _lstSources.Add(strInput);
            }
        }
        /// <summary>
        /// Retrieve the name of the Object that created an Improvement.
        /// </summary>
        /// <param name="objImprovement">Improvement to check.</param>
        public string GetObjectName(Improvement objImprovement)
        {
            string strReturn = string.Empty;
            switch (objImprovement.ImproveSource)
            {
                case Improvement.ImprovementSource.Bioware:
                case Improvement.ImprovementSource.Cyberware:
                    Cyberware objReturnCyberware = _lstCyberware.DeepFirstOrDefault(x => x.Children, x => x.InternalId == objImprovement.SourceName);
                    if (objReturnCyberware != null)
                        return objReturnCyberware.DisplayNameShort;
                    foreach (Vehicle objVehicle in _lstVehicles)
                    {
                        foreach (VehicleMod objVehicleMod in objVehicle.Mods)
                        {
                            objReturnCyberware = objVehicleMod.Cyberware.DeepFirstOrDefault(x => x.Children, x => x.InternalId == objImprovement.SourceName);
                            if (objReturnCyberware != null)
                                return objReturnCyberware.DisplayNameShort;
                        }
                    }
                    break;
                case Improvement.ImprovementSource.Gear:
                    Gear objReturnGear = _lstGear.DeepFirstOrDefault(x => x.Children, x => x.InternalId == objImprovement.SourceName);
                    if (objReturnGear != null)
                        return objReturnGear.DisplayNameShort;
                    foreach (Weapon objWeapon in _lstWeapons.DeepWhere(x => x.Children, x => x.WeaponAccessories.Any(y => y.Gear.Count > 0)))
                    {
                        foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                        {
                            objReturnGear = objAccessory.Gear.DeepFirstOrDefault(x => x.Children, x => x.InternalId == objImprovement.SourceName);
                            if (objReturnGear != null)
                                return objReturnGear.DisplayNameShort;
                        }
                    }
                    foreach (Armor objArmor in _lstArmor)
                    {
                        objReturnGear = objArmor.Gear.DeepFirstOrDefault(x => x.Children, x => x.InternalId == objImprovement.SourceName);
                        if (objReturnGear != null)
                            return objReturnGear.DisplayNameShort;
                    }
                    foreach (Cyberware objCyberware in _lstCyberware)
                    {
                        foreach (Cyberware objChildCyberware in _lstCyberware.DeepWhere(x => x.Children, x => x.Gear.Count > 0))
                        {
                            objReturnGear = objChildCyberware.Gear.DeepFirstOrDefault(x => x.Children, x => x.InternalId == objImprovement.SourceName);
                            if (objReturnGear != null)
                                return objReturnGear.DisplayNameShort;
                        }
                    }
                    foreach (Vehicle objVehicle in _lstVehicles)
                    {
                        objReturnGear = objVehicle.Gear.DeepFirstOrDefault(x => x.Children, x => x.InternalId == objImprovement.SourceName);
                        if (objReturnGear != null)
                            return objReturnGear.DisplayNameShort;
                        foreach (Weapon objWeapon in objVehicle.Weapons.DeepWhere(x => x.Children, x => x.WeaponAccessories.Any(y => y.Gear.Count > 0)))
                        {
                            foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                            {
                                objReturnGear = objAccessory.Gear.DeepFirstOrDefault(x => x.Children, x => x.InternalId == objImprovement.SourceName);
                                if (objReturnGear != null)
                                    return objReturnGear.DisplayNameShort;
                            }
                        }
                        foreach (VehicleMod objVehicleMod in objVehicle.Mods)
                        {
                            foreach (Weapon objWeapon in objVehicleMod.Weapons.DeepWhere(x => x.Children, x => x.WeaponAccessories.Any(y => y.Gear.Count > 0)))
                            {
                                foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                                {
                                    objReturnGear = objAccessory.Gear.DeepFirstOrDefault(x => x.Children, x => x.InternalId == objImprovement.SourceName);
                                    if (objReturnGear != null)
                                        return objReturnGear.DisplayNameShort;
                                }
                            }
                            foreach (Cyberware objCyberware in objVehicleMod.Cyberware.DeepWhere(x => x.Children, x => x.Gear.Count > 0))
                            {
                                objReturnGear = objCyberware.Gear.DeepFirstOrDefault(x => x.Children, x => x.InternalId == objImprovement.SourceName);
                                if (objReturnGear != null)
                                    return objReturnGear.DisplayNameShort;
                            }
                        }
                    }
                    break;
                case Improvement.ImprovementSource.Spell:
                    foreach (Spell objSpell in _lstSpells)
                    {
                        if (objSpell.InternalId == objImprovement.SourceName)
                        {
                            return objSpell.DisplayNameShort;
                        }
                    }
                    break;
                case Improvement.ImprovementSource.Power:
                    foreach (Power objPower in _lstPowers)
                    {
                        if (objPower.InternalId == objImprovement.SourceName)
                        {
                            return objPower.DisplayNameShort;
                        }
                    }
                    break;
                case Improvement.ImprovementSource.CritterPower:
                    foreach (CritterPower objPower in _lstCritterPowers)
                    {
                        if (objPower.InternalId == objImprovement.SourceName)
                        {
                            return objPower.DisplayNameShort;
                        }
                    }
                    break;
                case Improvement.ImprovementSource.Metamagic:
                case Improvement.ImprovementSource.Echo:
                    foreach (Metamagic objMetamagic in _lstMetamagics)
                    {
                        if (objMetamagic.InternalId == objImprovement.SourceName)
                        {
                            return objMetamagic.DisplayNameShort;
                        }
                    }
                    break;
                case Improvement.ImprovementSource.Art:
                    foreach (Art objArt in _lstArts)
                    {
                        if (objArt.InternalId == objImprovement.SourceName)
                        {
                            return objArt.DisplayNameShort;
                        }
                    }
                    break;
                case Improvement.ImprovementSource.Enhancement:
                    foreach (Enhancement objEnhancement in _lstEnhancements)
                    {
                        if (objEnhancement.InternalId == objImprovement.SourceName)
                        {
                            return objEnhancement.DisplayNameShort;
                        }
                    }
                    break;
                case Improvement.ImprovementSource.Armor:
                    foreach (Armor objArmor in _lstArmor)
                    {
                        if (objArmor.InternalId == objImprovement.SourceName)
                        {
                            return objArmor.DisplayNameShort;
                        }
                    }
                    break;
                case Improvement.ImprovementSource.ArmorMod:
                    foreach (Armor objArmor in _lstArmor)
                    {
                        foreach (ArmorMod objMod in objArmor.ArmorMods)
                        {
                            if (objMod.InternalId == objImprovement.SourceName)
                            {
                                return objMod.DisplayNameShort;
                            }
                        }
                    }
                    break;
                case Improvement.ImprovementSource.ComplexForm:
                    foreach (ComplexForm objProgram in _lstComplexForms)
                    {
                        if (objProgram.InternalId == objImprovement.SourceName)
                        {
                            return objProgram.DisplayNameShort;
                        }
                    }
                    break;
                case Improvement.ImprovementSource.AIProgram:
                    foreach (AIProgram objProgram in _lstAIPrograms)
                    {
                        if (objProgram.InternalId == objImprovement.SourceName)
                        {
                            return objProgram.DisplayNameShort;
                        }
                    }
                    break;
                case Improvement.ImprovementSource.Quality:
                    if (objImprovement.SourceName == "SEEKER_WIL")
                        return "Cyber-Singularty Seeker";
                    else if (objImprovement.SourceName.StartsWith("SEEKER"))
                        return "Redliner";
                    foreach (Quality objQuality in _lstQualities)
                    {
                        if (objQuality.InternalId == objImprovement.SourceName)
                        {
                            return objQuality.DisplayNameShort;
                        }
                    }
                    break;
                case Improvement.ImprovementSource.MartialArtAdvantage:
                    foreach (MartialArt objMartialArt in _lstMartialArts)
                    {
                        foreach (MartialArtAdvantage objAdvantage in objMartialArt.Advantages)
                        {
                            if (objAdvantage.InternalId == objImprovement.SourceName)
                            {
                                return objAdvantage.DisplayName;
                            }
                        }
                    }
                    break;
                default:
                    if (objImprovement.SourceName == "Armor Encumbrance")
                        return LanguageManager.GetString("String_ArmorEncumbrance");
                    // If this comes from a custom Improvement, use the name the player gave it instead of showing a GUID.
                    if (!string.IsNullOrEmpty(objImprovement.CustomName))
                        return objImprovement.CustomName;
                    return objImprovement.SourceName;
            }
            return string.Empty;
        }

        /// <summary>
        /// Clean an XPath string.
        /// </summary>
        /// <param name="strValue">String to clean.</param>
        private string CleanXPath(string strValue)
        {
            string strReturn;
            string strSearch = strValue;
            char[] chrQuotes = new char[] { '\'', '"' };

            int intQuotePos = strSearch.IndexOfAny(chrQuotes);
            if (intQuotePos == -1)
            {
                strReturn = "'" + strSearch + "'";
            }
            else
            {
                strReturn = "concat(";
                while (intQuotePos != -1)
                {
                    string strSubstring = strSearch.Substring(0, intQuotePos);
                    strReturn += "'" + strSubstring + "', ";
                    if (strSearch.Substring(intQuotePos, 1) == "'")
                    {
                        strReturn += "\"'\", ";
                    }
                    else
                    {
                        //must be a double quote
                        strReturn += "'\"', ";
                    }
                    strSearch = strSearch.Substring(intQuotePos + 1, strSearch.Length - intQuotePos - 1);
                    intQuotePos = strSearch.IndexOfAny(chrQuotes);
                }
                strReturn += "'" + strSearch + "')";
            }
            return strReturn;

        }
#endregion

#region Basic Properties
        /// <summary>
        /// Character Options object.
        /// </summary>
        public CharacterOptions Options
        {
            get
            {
                return _objOptions;
            }
        }

        /// <summary>
        /// Name of the file the Character is saved to.
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
        /// Name of the settings file the Character uses.
        /// </summary>
        public string SettingsFile
        {
            get
            {
                return _strSettingsFileName;
            }
            set
            {
                _strSettingsFileName = value;
                _objOptions.Load(_strSettingsFileName);
            }
        }

        /// <summary>
        /// Whether or not the character has been saved as Created and can no longer be modified using the Build system.
        /// </summary>
        public bool Created
        {
            get
            {
                return _blnCreated;
            }
            set
            {
                _blnCreated = value;
            }
        }

        /// <summary>
        /// Character's name.
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
                CharacterNameChanged?.Invoke(this);
            }
        }

		/// <summary>
		/// Character's portraits encoded using Base64.
		/// </summary>
		public List<string> Mugshots
        {
            get
                {
                return _lstMugshots;
                }
            set
                {
                _lstMugshots = value;
            }
                }

        /// <summary>
        /// Character's main portrait encoded using Base64.
        /// </summary>
        public string MainMugshot
        {
            get
            {
                if (_intMainMugshotIndex >= _lstMugshots.Count || _intMainMugshotIndex < 0)
                    return string.Empty;
                else
                    return _lstMugshots[_intMainMugshotIndex];
            }
        }

        /// <summary>
        /// Index of Character's main portrait.
        /// </summary>
        public int MainMugshotIndex
        {
            get
            {
                return _intMainMugshotIndex;
            }
            set
            {
                _intMainMugshotIndex = value;
                if (_intMainMugshotIndex >= _lstMugshots.Count)
                    _intMainMugshotIndex = 0;
                else if (_intMainMugshotIndex < 0)
                {
                    if (_lstMugshots.Count > 0)
                        _intMainMugshotIndex = _lstMugshots.Count - 1;
                    else
                        _intMainMugshotIndex = 0;
                }
            }
        }

        /// <summary>
        /// Character's Gameplay Option.
        /// </summary>
        public string GameplayOption
        {
            get
            {
                return _strGameplayOption;
            }
            set
            {
                _strGameplayOption = value;
            }
        }

        /// <summary>
        /// Quality Limit conferred by the Character's Gameplay Option
        /// </summary>
        public int GameplayOptionQualityLimit
        {
            get
            {
                return _intGameplayOptionQualityLimit;
            }
            set
            {
                _intGameplayOptionQualityLimit = value;
            }
        }

        /// <summary>
        /// Character's maximum karma at character creation.
        /// </summary>
        public int MaxKarma
        {
            get
            {
                return _intMaxKarma;
            }
            set
            {
                _intMaxKarma = value;
            }
        }

        /// <summary>
        /// Character's maximum nuyen at character creation.
        /// </summary>
        public decimal MaxNuyen
        {
            get
            {
                return _decMaxNuyen;
            }
            set
            {
                _decMaxNuyen = value;
            }
        }

        /// <summary>
        /// Character's contact point multiplier.
        /// </summary>
        public int ContactMultiplier
        {
            get
            {
                return _intContactMultiplier;
            }
            set
            {
                _intContactMultiplier = value;
            }
        }

        /// <summary>
        /// Character's Metatype Priority.
        /// </summary>
        public string MetatypePriority
        {
            get
            {
                return _strPriorityMetatype;
            }
            set
            {
                _strPriorityMetatype = value;
            }
        }

        /// <summary>
        /// Character's Attributes Priority.
        /// </summary>
        public string AttributesPriority
        {
            get
            {
                return _strPriorityAttributes;
            }
            set
            {
                _strPriorityAttributes = value;
            }
        }

        /// <summary>
        /// Character's Special Priority.
        /// </summary>
        public string SpecialPriority
        {
            get
            {
                return _strPrioritySpecial;
            }
            set
            {
                _strPrioritySpecial = value;
            }
        }

        /// <summary>
        /// Character's Skills Priority.
        /// </summary>
        public string SkillsPriority
        {
            get
            {
                return _strPrioritySkills;
            }
            set
            {
                _strPrioritySkills = value;
            }
        }

        /// <summary>
        /// Character's Resources Priority.
        /// </summary>
        public string ResourcesPriority
        {
            get
            {
                return _strPriorityResources;
            }
            set
            {
                _strPriorityResources = value;
            }
        }

        /// <summary>
        /// Character's Resources Priority.
        /// </summary>
        public string TalentPriority
        {
            get
            {
                return _strPriorityTalent;
            }
            set
            {
                _strPriorityTalent = value;
            }
        }

        /// <summary>
        /// Character's list of priority bonus skills.
        /// </summary>
        public List<string> PriorityBonusSkillList
        {
            get
            {
                return _lstPrioritySkills;
            }
            set
            {
                _lstPrioritySkills = value;
            }
        }

        /// <summary>
        /// Character's sex.
        /// </summary>
        public string Sex
        {
            get
            {
                return _strSex;
            }
            set
            {
                _strSex = value;
            }
        }

        /// <summary>
        /// Character's age.
        /// </summary>
        public string Age
        {
            get
            {
                return _strAge;
            }
            set
            {
                _strAge = value;
            }
        }

        /// <summary>
        /// Character's eyes.
        /// </summary>
        public string Eyes
        {
            get
            {
                return _strEyes;
            }
            set
            {
                _strEyes = value;
            }
        }

        /// <summary>
        /// Character's height.
        /// </summary>
        public string Height
        {
            get
            {
                return _strHeight;
            }
            set
            {
                _strHeight = value;
            }
        }

        /// <summary>
        /// Character's weight.
        /// </summary>
        public string Weight
        {
            get
            {
                return _strWeight;
            }
            set
            {
                _strWeight = value;
            }
        }

        /// <summary>
        /// Character's skin.
        /// </summary>
        public string Skin
        {
            get
            {
                return _strSkin;
            }
            set
            {
                _strSkin = value;
            }
        }

        /// <summary>
        /// Character's hair.
        /// </summary>
        public string Hair
        {
            get
            {
                return _strHair;
            }
            set
            {
                _strHair = value;
            }
        }

        /// <summary>
        /// Character's description.
        /// </summary>
        public string Description
        {
            get
            {
                return _strDescription;
            }
            set
            {
                _strDescription = value;
            }
        }

        /// <summary>
        /// Character's background.
        /// </summary>
        public string Background
        {
            get
            {
                return _strBackground;
            }
            set
            {
                _strBackground = value;
            }
        }

        /// <summary>
        /// Character's concept.
        /// </summary>
        public string Concept
        {
            get
            {
                return _strConcept;
            }
            set
            {
                _strConcept = value;
            }
        }

        /// <summary>
        /// Character notes.
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
        /// General gameplay notes.
        /// </summary>
        public string GameNotes
        {
            get
            {
                return _strGameNotes;
            }
            set
            {
                _strGameNotes = value;
            }
        }

        /// <summary>
        /// What is the Characters prefered hand
        /// </summary>
        public string PrimaryArm
        {
            get
            {
                return _strPrimaryArm;
            }
            set
            {
                _strPrimaryArm = value;
            }
        }

        /// <summary>
        /// Player name.
        /// </summary>
        public string PlayerName
        {
            get
            {
                return _strPlayerName;
            }
            set
            {
                _strPlayerName = value;
            }
        }

        /// <summary>
        /// Character's alias.
        /// </summary>
        public string Alias
        {
            get
            {
                return _strAlias;
            }
            set
            {
                _strAlias = value;
                CharacterNameChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Street Cred.
        /// </summary>
        public int StreetCred
        {
            get
            {
                return _intStreetCred;
            }
            set
            {
                _intStreetCred = value;
            }
        }

        /// <summary>
        /// Burnt Street Cred.
        /// </summary>
        public int BurntStreetCred
        {
            get
            {
                return _intBurntStreetCred;
            }
            set
            {
                _intBurntStreetCred = value;
            }
        }

        /// <summary>
        /// Notoriety.
        /// </summary>
        public int Notoriety
        {
            get
            {
                return _intNotoriety;
            }
            set
            {
                _intNotoriety = value;
            }
        }

        /// <summary>
        /// Public Awareness.
        /// </summary>
        public int PublicAwareness
        {
            get
            {
                return _intPublicAwareness;
            }
            set
            {
                _intPublicAwareness = value;
            }
        }

        /// <summary>
        /// Number of Physical Condition Monitor Boxes that are filled.
        /// </summary>
        public int PhysicalCMFilled
        {
            get
            {
                return _intPhysicalCMFilled;
            }
            set
            {
                _intPhysicalCMFilled = value;
            }
        }

        /// <summary>
        /// Number of Stun Condition Monitor Boxes that are filled.
        /// </summary>
        public int StunCMFilled
        {
            get
            {
                return _intStunCMFilled;
            }
            set
            {
                _intStunCMFilled = value;
            }
        }

        /// <summary>
        /// Whether or not character creation rules should be ignored.
        /// </summary>
        public bool IgnoreRules
        {
            get
            {
                return _blnIgnoreRules;
            }
            set
            {
                _blnIgnoreRules = value;
            }
        }

        /// <summary>
        /// Contact Points.
        /// </summary>
        public int ContactPoints
        {
            get
            {
                return _intContactPoints;
            }
            set
            {
                _intContactPoints = value;
            }
        }


        /// <summary>
        /// Number of free Contact Points the character has used.
        /// </summary>
        public int ContactPointsUsed
        {
            get
            {
                return _intContactPointsUsed;
            }
            set
            {
                _intContactPointsUsed = value;
            }
        }

        /// <summary>
        /// CFP Limit.
        /// </summary>
        public int CFPLimit
        {
            get
            {
                return _intCFPLimit;
            }
            set
            {
                _intCFPLimit = value;
            }
        }

        /// <summary>
        /// Total AI Program Limit.
        /// </summary>
        public int AINormalProgramLimit
        {
            get
            {
                return _intAINormalProgramLimit;
            }
            set
            {
                _intAINormalProgramLimit = value;
            }
        }

        /// <summary>
        /// AI Advanced Program Limit.
        /// </summary>
        public int AIAdvancedProgramLimit
        {
            get
            {
                return _intAIAdvancedProgramLimit;
            }
            set
            {
                _intAIAdvancedProgramLimit = value;
            }
        }

        /// <summary>
        /// Spell Limit.
        /// </summary>
        public int SpellLimit
        {
            get
            {
                return _intSpellLimit;
            }
            set
            {
                _intSpellLimit = value;
            }
        }

        /// <summary>
        /// Karma.
        /// </summary>
        public int Karma
        {
            get
            {
                return _intKarma;
            }
            set
            {
                bool oldCanAffordSpec = CanAffordSpecialization;

                OnPropertyChanged(ref _intKarma, value);

                if(oldCanAffordSpec != CanAffordSpecialization)
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanAffordSpecialization)));
            }
        }

        public bool CanAffordSpecialization
        {
            get { return Karma >= Math.Min(Options.KarmaSpecialization, Options.KarmaKnowledgeSpecialization); }
        }

        /// <summary>
        /// Special.
        /// </summary>
        public int Special
        {
            get
            {
                return _intSpecial;
            }
            set
            {
                _intSpecial = value;
            }
        }

        /// <summary>
        /// TotalSpecial.
        /// </summary>
        public int TotalSpecial
        {
            get
            {
                return _intTotalSpecial;
            }
            set
            {
                _intTotalSpecial = value;
            }
        }

        /// <summary>
        /// Attributes.
        /// </summary>
        public int Attributes
        {
            get
            {
                return _intAttributes;
            }
            set
            {
                _intAttributes = value;
            }
        }

        /// <summary>
        /// TotalAttributes.
        /// </summary>
        public int TotalAttributes
        {
            get
            {
                return _intTotalAttributes;
            }
            set
            {
                _intTotalAttributes = value;
            }
        }

        /// <summary>
        /// Total amount of Karma the character has earned over the career.
        /// </summary>
        public int CareerKarma
        {
            get
            {
                int intKarma = 0;

                foreach (ExpenseLogEntry objEntry in _lstExpenseLog)
                {
                    // Since we're only interested in the amount they have earned, only count values that are greater than 0 and are not refunds.
                    if (objEntry.Type == ExpenseType.Karma && objEntry.Amount > 0 && objEntry.Refund == false)
                        intKarma += Convert.ToInt32(objEntry.Amount);
                }

                return intKarma;
            }
        }

        /// <summary>
        /// Total amount of Nuyen the character has earned over the career.
        /// </summary>
        public decimal CareerNuyen
        {
            get
            {
                decimal decNuyen = 0;

                foreach (ExpenseLogEntry objEntry in _lstExpenseLog)
                {
                    // Since we're only interested in the amount they have earned, only count values that are greater than 0 and are not refunds.
                    if (objEntry.Type == ExpenseType.Nuyen && objEntry.Amount > 0 && objEntry.Refund == false)
                        decNuyen += objEntry.Amount;
                }

                return decNuyen;
            }
        }

        /// <summary>
        /// Whether or not the character is a Critter.
        /// </summary>
        public bool IsCritter
        {
            get
            {
                return _blnIsCritter;
            }
            set
            {
                _blnIsCritter = value;
            }
        }

        /// <summary>
        /// The highest number of free metagenetic qualities the character can have.
        /// </summary>
        public int MetageneticLimit
        {
            get
            {
                return _intMetageneticLimit;
            }
            set
            {
                _intMetageneticLimit = value;
            }
        }

        /// <summary>
        /// Whether or not the character is possessed by a Spirit.
        /// </summary>
        public bool Possessed
        {
            get
            {
                return _blnPossessed;
            }
            set
            {
                _blnPossessed = value;
            }
        }

        /// <summary>
        /// Maximum item Availability for new characters.
        /// </summary>
        public int MaximumAvailability
        {
            get
            {
                return _intMaxAvail;
            }
            set
            {
                _intMaxAvail = value;
            }
        }
#endregion

#region Attributes
        /// <summary>
        /// Get an CharacterAttribute by its name.
        /// </summary>
        /// <param name="strAttribute">CharacterAttribute name to retrieve.</param>
        public CharacterAttrib GetAttribute(string strAttribute)
        {
	        return AttributeSection.GetAttributeByName(strAttribute);
        }

        /// <summary>
        /// Body (BOD) CharacterAttribute.
        /// </summary>
        public CharacterAttrib BOD
        {
            get
            {
				return AttributeSection.GetAttributeByName("BOD");
			}
        }

        /// <summary>
        /// Agility (AGI) CharacterAttribute.
        /// </summary>
        public CharacterAttrib AGI
        {
            get
            {
				return AttributeSection.GetAttributeByName("AGI");
			}
        }

        /// <summary>
        /// Reaction (REA) CharacterAttribute.
        /// </summary>
        public CharacterAttrib REA
        {
            get
            {
				return AttributeSection.GetAttributeByName("REA");
			}
        }

        /// <summary>
        /// Strength (STR) CharacterAttribute.
        /// </summary>
        public CharacterAttrib STR
        {
            get
            {
				return AttributeSection.GetAttributeByName("STR");
			}
        }

        /// <summary>
        /// Charisma (CHA) CharacterAttribute.
        /// </summary>
        public CharacterAttrib CHA
        {
            get
            {
				return AttributeSection.GetAttributeByName("CHA");
			}
        }

        /// <summary>
        /// Intuition (INT) CharacterAttribute.
        /// </summary>
        public CharacterAttrib INT
        {
            get
            {
				return AttributeSection.GetAttributeByName("INT");
			}
        }

        /// <summary>
        /// Logic (LOG) CharacterAttribute.
        /// </summary>
        public CharacterAttrib LOG
        {
            get
            {
				return AttributeSection.GetAttributeByName("LOG");
			}
        }

        /// <summary>
        /// Willpower (WIL) CharacterAttribute.
        /// </summary>
        public CharacterAttrib WIL
        {
            get
            {
				return AttributeSection.GetAttributeByName("WIL");
			}
        }

        /// <summary>
        /// Initiative (INI) CharacterAttribute.
        /// </summary>
        public CharacterAttrib INI
        {
            get
			{
				return AttributeSection.GetAttributeByName("INT");
			}
        }

        /// <summary>
        /// Edge (EDG) CharacterAttribute.
        /// </summary>
        public CharacterAttrib EDG
        {
            get
            {
				return AttributeSection.GetAttributeByName("EDG");
			}
        }

        /// <summary>
        /// Magic (MAG) CharacterAttribute.
        /// </summary>
        public CharacterAttrib MAG
        {
            get
            {
				return AttributeSection.GetAttributeByName("MAG");
			}
        }

        /// <summary>
        /// Resonance (RES) CharacterAttribute.
        /// </summary>
        public CharacterAttrib RES
        {
	        get
	        {
				return AttributeSection.GetAttributeByName("RES");
			}
		}
        
        /// <summary>
        /// Depth (DEP) Attribute.
        /// </summary>
        public CharacterAttrib DEP
		{
			get
			{
				return AttributeSection.GetAttributeByName("DEP");
			}
		}

        /// <summary>
        /// Essence (ESS) Attribute.
        /// </summary>
        public CharacterAttrib ESS
        {
			get
			{
				return AttributeSection.GetAttributeByName("ESS");
			}
		}

        /// <summary>
        /// Is the MAG CharacterAttribute enabled?
        /// </summary>
        public bool MAGEnabled
        {
            get
            {
                return _blnMAGEnabled;
            }
            set
            {
                bool blnOldValue = _blnMAGEnabled;
                _blnMAGEnabled = value;
                if (value && Created)
                    _decEssenceAtSpecialStart = Essence;
                    if (blnOldValue != value)
                    MAGEnabledChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Amount of Power Points for Mystic Adepts.
        /// </summary>
        public int MysticAdeptPowerPoints
        {
            get
            {
                return _intMAGAdept;
            }
            set
            {
                _intMAGAdept = value;
            }
        }

        /// <summary>
        /// Magician's Tradition.
        /// </summary>
        public string MagicTradition
        {
            get
            {
                return _strMagicTradition;
            }
            set
            {
                _strMagicTradition = value;
            }
        }

        /// <summary>
        /// Magician's Tradition Drain Attributes.
        /// </summary>
        public string TraditionDrain
        {
            get
            {
                if (AdeptEnabled && !MagicianEnabled)
                {
                    return "BOD + WIL";
                }
                return _strTraditionDrain;
            }
            set
            {
                _strTraditionDrain = value;
            }
        }

        /// <summary>
        /// Magician's Tradition Name (for Custom Traditions).
        /// </summary>
        public string TraditionName
        {
            get
            {
                return _strTraditionName;
            }
            set
            {
                _strTraditionName = value;
            }
        }

        /// <summary>
        /// Magician's Combat Spirit (for Custom Traditions).
        /// </summary>
        public string SpiritCombat
        {
            get
            {
                return _strSpiritCombat;
            }
            set
            {
                _strSpiritCombat = value;
            }
        }

        /// <summary>
        /// Magician's Detection Spirit (for Custom Traditions).
        /// </summary>
        public string SpiritDetection
        {
            get
            {
                return _strSpiritDetection;
            }
            set
            {
                _strSpiritDetection = value;
            }
        }

        /// <summary>
        /// Magician's Health Spirit (for Custom Traditions).
        /// </summary>
        public string SpiritHealth
        {
            get
            {
                return _strSpiritHealth;
            }
            set
            {
                _strSpiritHealth = value;
            }
        }

        /// <summary>
        /// Magician's Illusion Spirit (for Custom Traditions).
        /// </summary>
        public string SpiritIllusion
        {
            get
            {
                return _strSpiritIllusion;
            }
            set
            {
                _strSpiritIllusion = value;
            }
        }

        /// <summary>
        /// Magician's Manipulation Spirit (for Custom Traditions).
        /// </summary>
        public string SpiritManipulation
        {
            get
            {
                return _strSpiritManipulation;
            }
            set
            {
                _strSpiritManipulation = value;
            }
        }

        /// <summary>
        /// Technomancer's Stream.
        /// </summary>
        public string TechnomancerStream
        {
            get
            {
                return _strTechnomancerStream;
            }
            set
            {
                _strTechnomancerStream = value;
            }
        }

        /// <summary>
        /// Technomancer's Fading Attributes.
        /// </summary>
        public string TechnomancerFading
        {
            get
            {
                return _strTechnomancerFading;
            }
            set
            {
                _strTechnomancerFading = value;
            }
        }

        /// <summary>
        /// Initiate Grade.
        /// </summary>
        public int InitiateGrade
        {
            get
            {
                return _intInitiateGrade;
            }
            set
            {
                _intInitiateGrade = value;
            }
        }

        /// <summary>
        /// Is the RES CharacterAttribute enabled?
        /// </summary>
        public bool RESEnabled
        {
            get
            {
                return _blnRESEnabled;
            }
            set
            {
                bool blnOldValue = _blnRESEnabled;
                _blnRESEnabled = value;
                if (_blnRESEnabled)
                    TechnomancerStream = "Default";
                else
                    TechnomancerStream = string.Empty;
                ImprovementManager.ClearCachedValue(Improvement.ImprovementType.MatrixInitiativeDice);
                if (value && Created)
                    _decEssenceAtSpecialStart = Essence;
                    if (blnOldValue != value)
                    RESEnabledChanged?.Invoke(this);
            }
                }

        /// <summary>
        /// Is the DEP CharacterAttribute enabled?
        /// </summary>
        public bool DEPEnabled
        {
            get
                {
                return _blnDEPEnabled;
                }
            set
            {
                bool blnOldValue = _blnDEPEnabled;
                _blnDEPEnabled = value;
                if (value && Created)
                    _decEssenceAtSpecialStart = Essence;
                if (blnOldValue != value)
                    DEPEnabledChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Submersion Grade.
        /// </summary>
        public int SubmersionGrade
        {
            get
            {
                return _intSubmersionGrade;
            }
            set
            {
                _intSubmersionGrade = value;
            }
        }

        /// <summary>
        /// Whether or not the character is a member of a Group or Network.
        /// </summary>
        public bool GroupMember
        {
            get
            {
                return _blnGroupMember;
            }
            set
            {
                _blnGroupMember = value;
            }
        }

        /// <summary>
        /// The name of the Group the Initiate has joined.
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
        /// Notes for the Group the Initiate has joined.
        /// </summary>
        public string GroupNotes
        {
            get
            {
                return _strGroupNotes;
            }
            set
            {
                _strGroupNotes = value;
            }
        }

        /// <summary>
        /// Essence the character had when the first gained access to MAG/RES.
        /// </summary>
        public decimal EssenceAtSpecialStart
        {
            get
            {
                return _decEssenceAtSpecialStart;
            }
        }

        /// <summary>
        /// Character's Essence.
        /// </summary>
        public decimal Essence
        {
            get
            {
                // If the character has a fixed Essence Improvement, permanently fix their Essence at its value.
                if (_lstImprovements.Any(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.CyborgEssence && objImprovement.Enabled))
                {
                    return 0.1m;
                }
                decimal decESS = EssenceMaximum;
                // Run through all of the pieces of Cyberware and include their Essence cost. Cyberware and Bioware costs are calculated separately. The higher value removes its full cost from the
                // character's ESS while the lower removes half of its cost from the character's ESS.
                decimal decCyberware = 0m;
                decimal decBioware = 0m;
                decimal decHole = 0m;
                foreach (Cyberware objCyberware in _lstCyberware)
                {
                    if (objCyberware.Name == "Essence Hole")
                        decHole += objCyberware.CalculatedESS();
                    else
                    {
                        if (objCyberware.SourceType == Improvement.ImprovementSource.Cyberware)
                            decCyberware += objCyberware.CalculatedESS();
                        else if (objCyberware.SourceType == Improvement.ImprovementSource.Bioware)
                                decBioware += objCyberware.CalculatedESS();
                        }
                    }
                decESS += Convert.ToDecimal(ImprovementManager.ValueOf(this, Improvement.ImprovementType.EssencePenalty));
                decESS += Convert.ToDecimal(ImprovementManager.ValueOf(this, Improvement.ImprovementType.EssencePenaltyT100)) / 100.0m;

                decESS -= decCyberware + decBioware;
                // Deduct the Essence Hole value.
                decESS -= decHole;

                //1781 Essence is not printing
                ESS.Base = Convert.ToInt32(decESS);

                return decESS;
            }
        }

        /// <summary>
        /// Essence consumed by Cyberware.
        /// </summary>
        public decimal CyberwareEssence
        {
            get
            {
                // Run through all of the pieces of Cyberware and include their Essence cost. Cyberware and Bioware costs are calculated separately.
                return _lstCyberware.Where(objCyberware => objCyberware.Name != "Essence Hole" && objCyberware.SourceType == Improvement.ImprovementSource.Cyberware).Sum(objCyberware => objCyberware.CalculatedESS());
            }
        }

        /// <summary>
        /// Essence consumed by Bioware.
        /// </summary>
        public decimal BiowareEssence
        {
            get
            {
                // Run through all of the pieces of Cyberware and include their Essence cost. Cyberware and Bioware costs are calculated separately.
                return _lstCyberware.Where(objCyberware => objCyberware.Name != "Essence Hole" && objCyberware.SourceType == Improvement.ImprovementSource.Bioware).Sum(objCyberware => objCyberware.CalculatedESS());
            }
        }

        /// <summary>
        /// Essence consumed by Essence Holes.
        /// </summary>
        public decimal EssenceHole
        {
            get
            {
                // Find the total Essence Cost of all Essence Hole objects.
                return _lstCyberware.Where(objCyberware => objCyberware.Name == "Essence Hole").Sum(objCyberware => objCyberware.CalculatedESS());
            }
        }

        /// <summary>
        /// Character's maximum Essence.
        /// </summary>
        public decimal EssenceMaximum
        {
            get
            {
                return Convert.ToDecimal(ESS.MetatypeMaximum + ImprovementManager.ValueOf(this, Improvement.ImprovementType.EssenceMax), GlobalOptions.InvariantCultureInfo);
            }
        }

        /// <summary>
        /// Character's total Essence Loss penalty for RES or DEP.
        /// </summary>
        public int EssencePenalty
        {
            get
            {
                // Subtract the character's current Essence from its maximum. Round the remaining amount up to get the total penalty to RES and DEP.
                return Convert.ToInt32(Math.Ceiling(EssenceAtSpecialStart + Convert.ToDecimal(ImprovementManager.ValueOf(this, Improvement.ImprovementType.EssenceMax), GlobalOptions.InvariantCultureInfo) - Essence));
            }
        }

        /// <summary>
        /// Character's total Essence Loss penalty for MAG.
        /// </summary>
        public int EssencePenaltyMAG
        {
            get
            {
                // Subtract the character's current Essence from its maximum, but taking into account essence modifiers that only affect MAG. Round the remaining amount up to get the total penalty to MAG.
                return Convert.ToInt32(Math.Ceiling(EssenceAtSpecialStart + Convert.ToDecimal(ImprovementManager.ValueOf(this, Improvement.ImprovementType.EssenceMax), GlobalOptions.InvariantCultureInfo) - Essence - (Convert.ToDecimal(ImprovementManager.ValueOf(this, Improvement.ImprovementType.EssencePenaltyMAGOnlyT100), GlobalOptions.InvariantCultureInfo) / 100.0m)));
            }
        }

#region Initiative
#region Physical
        /// <summary>
        /// Physical Initiative.
        /// </summary>
        public string Initiative
        {
            get
            {
                return LanguageManager.GetString("String_Initiative")
                    .Replace("{0}", InitiativeValue.ToString())
                    .Replace("{1}", InitiativeDice.ToString());
            }
        }

        /// <summary>
        /// Initiative Dice.
        /// </summary>
        public int InitiativeDice
        {
            get
            {
                int intExtraIP = 1 + Convert.ToInt32(ImprovementManager.ValueOf(this, Improvement.ImprovementType.InitiativeDice)) + Convert.ToInt32(ImprovementManager.ValueOf(this, Improvement.ImprovementType.InitiativeDiceAdd));

                return Math.Min(intExtraIP, 5);
            }
        }

        public int InitiativeValue
        {
            get
            {
                int intINI = (INT.TotalValue + REA.TotalValue) + WoundModifiers;
                intINI += ImprovementManager.ValueOf(this, Improvement.ImprovementType.Initiative);
                if (intINI < 0)
                    intINI = 0;
                return intINI;
            }
        }
#endregion
#region Astral
        /// <summary>
        /// Astral Initiative.
        /// </summary>
        public string AstralInitiative
        {
            get
            {
                return LanguageManager.GetString("String_Initiative")
                    .Replace("{0}", AstralInitiativeValue.ToString())
                    .Replace("{1}", AstralInitiativeDice.ToString());
            }
        }

        /// <summary>
        /// Astral Initiative Value.
        /// </summary>
        public int AstralInitiativeValue
        {
            get
            {
                return (INT.TotalValue * 2) + WoundModifiers;
            }
        }

        /// <summary>
        /// Astral Initiative Dice.
        /// </summary>
        public int AstralInitiativeDice
        {
            get
            {
                //TODO: Global option assignation
                return 3;
                }
            }
#endregion
#region Matrix
#region AR
        /// <summary>
        /// Formatted AR Matrix Initiative.
        /// </summary>
        public string MatrixInitiative
        {
            get
            {
                return LanguageManager.GetString("String_Initiative")
                        .Replace("{0}", MatrixInitiativeValue.ToString())
                        .Replace("{1}", MatrixInitiativeDice.ToString());
            }

        }

        /// <summary>
        /// AR Matrix Initiative Value.
        /// </summary>
        public int MatrixInitiativeValue
        {
            get
            {
                if (_strMetatype == "A.I.")
                {
                    int intINI = (INT.TotalValue) + WoundModifiers;
                    if (HomeNodeVehicle != null || HomeNodeCommlink != null)
                    {
                        int intHomeNodePilot = HomeNodeVehicle?.Pilot ?? 0;
                        int intHomeNodeDP = Math.Max(HomeNodeCommlink?.TotalDataProcessing ?? 0, HomeNodeVehicle?.DeviceRating ?? 0);
                        if (intHomeNodeDP > intHomeNodePilot)
                        {
                            intINI += intHomeNodeDP;
                        }
                        else
                        {
                            intINI += intHomeNodePilot;
                        }
                    }
                    return intINI;
                }
                return InitiativeValue;
            }
        }

        /// <summary>
        /// AR Matrix Initiative Dice.
        /// </summary>
        public int MatrixInitiativeDice
        {
            get
            {
                int intReturn;
                // A.I.s always have 4 Matrix Initiative Dice.
                if (_strMetatype == "A.I.")
                    intReturn = 4 + ImprovementManager.ValueOf(this, Improvement.ImprovementType.MatrixInitiativeDice);
                else
                    intReturn = InitiativeDice;

                // Add in any additional Matrix Initiative Pass bonuses.
                intReturn += ImprovementManager.ValueOf(this, Improvement.ImprovementType.MatrixInitiativeDiceAdd);

                return Math.Min(intReturn, 5);
            }
        }
#endregion
#region Cold Sim
        /// <summary>
        /// Matrix Initiative via VR with Cold Sim.
        /// </summary>
        public string MatrixInitiativeCold
        {
            get
            {
                if (_strMetatype == "A.I.")
                {
                    return MatrixInitiative;
                }
                return
                    LanguageManager.GetString("String_MatrixInitiative")
                        .Replace("{0}", MatrixInitiativeColdValue.ToString())
                        .Replace("{1}", MatrixInitiativeColdDice.ToString());
            }
        }

        /// <summary>
        /// Cold Sim Matrix Initiative Value.
        /// </summary>
        public int MatrixInitiativeColdValue
        {
            get
            {
                if (_strMetatype == "A.I.")
                {
                    return MatrixInitiativeValue;
                }
                return INT.TotalValue + WoundModifiers + ImprovementManager.ValueOf(this, Improvement.ImprovementType.MatrixInitiative); ;
            }
        }

        /// <summary>
        /// Cold Sim Matrix Initiative Dice.
        /// </summary>
        public int MatrixInitiativeColdDice
        {
            get
            {
                if (_strMetatype == "A.I.")
                {
                    return MatrixInitiativeDice;
                }
                return Math.Min(3 + ImprovementManager.ValueOf(this, Improvement.ImprovementType.MatrixInitiativeDice),5);
            }
        }
#endregion
#region Hot Sim
        /// <summary>
        /// Matrix Initiative via VR with Hot Sim.
        /// </summary>
        public string MatrixInitiativeHot
        {
            get
            {
                if (_strMetatype == "A.I.")
                {
                    return MatrixInitiative;
                }
                return
                    LanguageManager.GetString("String_MatrixInitiative")
                        .Replace("{0}", MatrixInitiativeHotValue.ToString())
                        .Replace("{1}", MatrixInitiativeHotDice.ToString());
            }
        }

        /// <summary>
        /// Hot Sim Matrix Initiative Value.
        /// </summary>
        public int MatrixInitiativeHotValue
        {
            get
            {
                if (_strMetatype == "A.I.")
                {
                    return MatrixInitiativeValue;
            }
                return INT.TotalValue + WoundModifiers + ImprovementManager.ValueOf(this, Improvement.ImprovementType.MatrixInitiative); ;
            }
        }

        /// <summary>
        /// Hot Sim Matrix Initiative Dice.
        /// </summary>
        public int MatrixInitiativeHotDice
        {
            get
            {
                if (_strMetatype == "A.I.")
                {
                    return MatrixInitiativeDice;
            }
                return Math.Min(4 + ImprovementManager.ValueOf(this, Improvement.ImprovementType.MatrixInitiativeDice), 5);
            }
        }
#endregion
#endregion
#endregion

        /// <summary>
        /// Character's total Spell Resistance from qualities and metatype properties.
        /// </summary>
        public int SpellResistance
        {
            get
            {
                return ImprovementManager.ValueOf(this, Improvement.ImprovementType.SpellResistance);
            }
        }
#endregion

#region Special CharacterAttribute Tests
        /// <summary>
        /// Composure (WIL + CHA).
        /// </summary>
        public int Composure
        {
            get
            {
                return WIL.TotalValue + CHA.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.Composure);
            }
        }

        /// <summary>
        /// Judge Intentions (INT + CHA).
        /// </summary>
        public int JudgeIntentions
        {
            get
            {
                return INT.TotalValue + CHA.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.JudgeIntentions) + ImprovementManager.ValueOf(this, Improvement.ImprovementType.JudgeIntentionsOffense);
            }
        }

        /// <summary>
        /// Judge Intentions Resist (CHA + WIL).
        /// </summary>
        public int JudgeIntentionsResist
        {
            get
            {
                return CHA.TotalValue + WIL.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.JudgeIntentions) + ImprovementManager.ValueOf(this, Improvement.ImprovementType.JudgeIntentionsDefense);
            }
        }

        /// <summary>
        /// Lifting and Carrying (STR + BOD).
        /// </summary>
        public int LiftAndCarry
        {
            get
            {
                return STR.TotalValue + BOD.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.LiftAndCarry);
            }
        }

        /// <summary>
        /// Memory (LOG + WIL).
        /// </summary>
        public int Memory
        {
            get
            {
                return LOG.TotalValue + WIL.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.Memory);
            }
        }

        /// <summary>
        /// Resist test to Fatigue damage (BOD + WIL).
        /// </summary>
        public int FatigueResist
        {
            get
            {
                return BOD.TotalValue + WIL.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.FatigueResist);
            }
        }

        /// <summary>
        /// Resist test to Radiation damage (BOD + WIL).
        /// </summary>
        public int RadiationResist
        {
            get
            {
                return BOD.TotalValue + WIL.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.RadiationResist);
            }
        }

        /// <summary>
        /// Resist test to Sonic Attacks damage (WIL).
        /// </summary>
        public int SonicResist
        {
            get
            {
                return WIL.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.SonicResist);
            }
        }

        /// <summary>
        /// Resist test to Contact-vector Toxins (BOD + WIL).
        /// </summary>
        public string ToxinContactResist
        {
            get
            {
                if (Improvements.Any(x => x.Enabled && x.ImproveType == Improvement.ImprovementType.ToxinContactImmune))
                    return LanguageManager.GetString("String_Immune");
                else
                    return (BOD.TotalValue + WIL.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.ToxinContactResist)).ToString(GlobalOptions.CultureInfo);
            }
        }
        /// <summary>
        /// Resist test to Ingestion-vector Toxins (BOD + WIL).
        /// </summary>
        public string ToxinIngestionResist
        {
            get
            {
                if (Improvements.Any(x => x.Enabled && x.ImproveType == Improvement.ImprovementType.ToxinIngestionImmune))
                    return LanguageManager.GetString("String_Immune");
                else
                    return (BOD.TotalValue + WIL.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.ToxinIngestionResist)).ToString(GlobalOptions.CultureInfo);
            }
        }
        /// <summary>
        /// Resist test to Inhalation-vector Toxins (BOD + WIL).
        /// </summary>
        public string ToxinInhalationResist
        {
            get
            {
                if (Improvements.Any(x => x.Enabled && x.ImproveType == Improvement.ImprovementType.ToxinInhalationImmune))
                    return LanguageManager.GetString("String_Immune");
                else
                    return (BOD.TotalValue + WIL.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.ToxinInhalationResist)).ToString(GlobalOptions.CultureInfo);
            }
        }
        /// <summary>
        /// Resist test to Injection-vector Toxins (BOD + WIL).
        /// </summary>
        public string ToxinInjectionResist
        {
            get
            {
                if (Improvements.Any(x => x.Enabled && x.ImproveType == Improvement.ImprovementType.ToxinInjectionImmune))
                    return LanguageManager.GetString("String_Immune");
                else
                    return (BOD.TotalValue + WIL.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.ToxinInjectionResist)).ToString(GlobalOptions.CultureInfo);
            }
        }

        /// <summary>
        /// Resist test to Contact-vector Pathogens (BOD + WIL).
        /// </summary>
        public string PathogenContactResist
        {
            get
            {
                if (Improvements.Any(x => x.Enabled && x.ImproveType == Improvement.ImprovementType.PathogenContactImmune))
                    return LanguageManager.GetString("String_Immune");
                else
                    return (BOD.TotalValue + WIL.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.PathogenContactResist)).ToString(GlobalOptions.CultureInfo);
            }
        }
        /// <summary>
        /// Resist test to Ingestion-vector Pathogens (BOD + WIL).
        /// </summary>
        public string PathogenIngestionResist
        {
            get
            {
                if (Improvements.Any(x => x.Enabled && x.ImproveType == Improvement.ImprovementType.PathogenIngestionImmune))
                    return LanguageManager.GetString("String_Immune");
                else
                    return (BOD.TotalValue + WIL.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.PathogenIngestionResist)).ToString(GlobalOptions.CultureInfo);
            }
        }
        /// <summary>
        /// Resist test to Inhalation-vector Pathogens (BOD + WIL).
        /// </summary>
        public string PathogenInhalationResist
        {
            get
            {
                if (Improvements.Any(x => x.Enabled && x.ImproveType == Improvement.ImprovementType.PathogenInhalationImmune))
                    return LanguageManager.GetString("String_Immune");
                else
                    return (BOD.TotalValue + WIL.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.PathogenInhalationResist)).ToString(GlobalOptions.CultureInfo);
            }
        }
        /// <summary>
        /// Resist test to Injection-vector Pathogens (BOD + WIL).
        /// </summary>
        public string PathogenInjectionResist
        {
            get
            {
                if (Improvements.Any(x => x.Enabled && x.ImproveType == Improvement.ImprovementType.PathogenInjectionImmune))
                    return LanguageManager.GetString("String_Immune");
                else
                    return (BOD.TotalValue + WIL.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.PathogenInjectionResist)).ToString(GlobalOptions.CultureInfo);
            }
        }

        /// <summary>
        /// Resist test to Physiological Addiction (BOD + WIL) if you are not addicted yet.
        /// </summary>
        public string PhysiologicalAddictionResistFirstTime
        {
            get
            {
                return (BOD.TotalValue + WIL.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.PhysiologicalAddictionFirstTime)).ToString(GlobalOptions.CultureInfo);
            }
        }

        /// <summary>
        /// Resist test to Psychological Addiction (LOG + WIL) if you are not addicted yet.
        /// </summary>
        public string PsychologicalAddictionResistFirstTime
        {
            get
            {
                return (LOG.TotalValue + WIL.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.PsychologicalAddictionFirstTime)).ToString(GlobalOptions.CultureInfo);
            }
        }

        /// <summary>
        /// Resist test to Physiological Addiction (BOD + WIL) if you are already addicted.
        /// </summary>
        public string PhysiologicalAddictionResistAlreadyAddicted
        {
            get
            {
                return (BOD.TotalValue + WIL.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.PhysiologicalAddictionAlreadyAddicted)).ToString(GlobalOptions.CultureInfo);
            }
        }

        /// <summary>
        /// Resist test to Psychological Addiction (LOG + WIL) if you are already addicted.
        /// </summary>
        public string PsychologicalAddictionResistAlreadyAddicted
        {
            get
            {
                return (LOG.TotalValue + WIL.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.PsychologicalAddictionAlreadyAddicted)).ToString(GlobalOptions.CultureInfo);
            }
        }

        /// <summary>
        /// Dicepool for natural recovery from Stun CM box damage (BOD + WIL).
        /// </summary>
        public string StunCMNaturalRecovery
        {
            get
            {
                int intReturn = BOD.TotalValue + WIL.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.StunCMRecovery);
                if (Improvements.Any(x => x.Enabled && x.ImproveType == Improvement.ImprovementType.AddESStoStunCMRecovery))
                    intReturn += Convert.ToInt32(Math.Floor(Essence));
                return intReturn.ToString(GlobalOptions.CultureInfo);
            }
        }

        /// <summary>
        /// Dicepool for natural recovery from Physical CM box damage (2 x BOD).
        /// </summary>
        public string PhysicalCMNaturalRecovery
        {
            get
            {
                int intReturn = 2 * BOD.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.PhysicalCMRecovery);
                if (Improvements.Any(x => x.Enabled && x.ImproveType == Improvement.ImprovementType.AddESStoPhysicalCMRecovery))
                    intReturn += Convert.ToInt32(Math.Floor(Essence));
                return intReturn.ToString(GlobalOptions.CultureInfo);
            }
        }
        #endregion

        #region Reputation
        /// <summary>
        /// Amount of Street Cred the character has earned through standard means.
        /// </summary>
        public int CalculatedStreetCred
        {
            get
            {
                // Street Cred = Career Karma / 10, rounded down
                int intReturn = CareerKarma / 10;

                // Deduct burnt Street Cred.
                intReturn -= _intBurntStreetCred;

                return intReturn;
            }
        }

        /// <summary>
        /// Character's total amount of Street Cred (earned + GM awarded).
        /// </summary>
        public int TotalStreetCred
        {
            get
            {
                return Math.Max(CalculatedStreetCred + StreetCred, 0);
            }
        }

        /// <summary>
        /// Street Cred Tooltip.
        /// </summary>
        public string StreetCredTooltip
        {
            get
            {
                string strReturn = string.Empty;

                strReturn += "(" + LanguageManager.GetString("String_CareerKarma") + " ÷ 10)";
                if (BurntStreetCred != 0)
                    strReturn += " - " + LanguageManager.GetString("String_BurntStreetCred");

                return strReturn;
            }
        }

        /// <summary>
        /// Amount of Notoriety the character has earned through standard means.
        /// </summary>
        public int CalculatedNotoriety
        {
            get
            {
                // Notoriety is simply the total value of Notoriety Improvements + the number of Enemies they have.
                int intReturn = ImprovementManager.ValueOf(this, Improvement.ImprovementType.Notoriety);

                foreach (Contact objContact in _lstContacts)
                {
                    if (objContact.EntityType == ContactType.Enemy)
                        intReturn += 1;
                }

                return intReturn;
            }
        }

        /// <summary>
        /// Character's total amount of Notoriety (earned + GM awarded - burnt Street Cred).
        /// </summary>
        public int TotalNotoriety
        {
            get
            {
                return CalculatedNotoriety + Notoriety - (BurntStreetCred / 2);
            }
        }

        /// <summary>
        /// Tooltip to use for Notoriety total.
        /// </summary>
        public string NotorietyTooltip
        {
            get
            {
                string strReturn = string.Empty;
                int intEnemies = 0;

                foreach (Improvement objImprovement in _lstImprovements)
                {
                    if (objImprovement.ImproveType == Improvement.ImprovementType.Notoriety)
                        strReturn += " + " + GetObjectName(objImprovement) + " (" + objImprovement.Value.ToString() + ")";
                }

                foreach (Contact objContact in _lstContacts)
                {
                    if (objContact.EntityType == ContactType.Enemy)
                        intEnemies += 1;
                }

                if (intEnemies > 0)
                    strReturn += " + " + LanguageManager.GetString("Label_SummaryEnemies") + " (" + intEnemies.ToString() + ")";

                if (BurntStreetCred > 0)
                    strReturn += " - " + LanguageManager.GetString("String_BurntStreetCred") + " (" + (BurntStreetCred / 2).ToString() + ")";

                strReturn = strReturn.Trim();
                if (strReturn.StartsWith("+") || strReturn.StartsWith("-"))
                    strReturn = strReturn.Substring(2, strReturn.Length - 2);

                return strReturn;
            }
        }

        /// <summary>
        /// Amount of Public Awareness the character has earned through standard means.
        /// </summary>
        public int CalculatedPublicAwareness
        {
            get
            {
                int intReturn = 0;
                if (_objOptions.UseCalculatedPublicAwareness)
                {
                    // Public Awareness is calculated as (Street Cred + Notoriety) / 3, rounded down.
                    intReturn = (TotalStreetCred + TotalNotoriety) / 3;
                }

                return intReturn + ImprovementManager.ValueOf(this, Improvement.ImprovementType.PublicAwareness);
            }
        }

        /// <summary>
        /// Character's total amount of Public Awareness (earned + GM awarded).
        /// </summary>
        public int TotalPublicAwareness
        {
            get
            {
                return PublicAwareness;
            }
        }

        /// <summary>
        /// Public Awareness Tooltip.
        /// </summary>
        public string PublicAwarenessTooltip
        {
            get
            {
                string strReturn = string.Empty;

                if (_objOptions.UseCalculatedPublicAwareness)
                {
                    strReturn += "(" + LanguageManager.GetString("String_StreetCred") + " (" + TotalStreetCred.ToString() + ") + " + LanguageManager.GetString("String_Notoriety") + " (" + TotalNotoriety.ToString() + ")) ÷ 3";
                }

                return strReturn;
            }
        }
#endregion

#region List Properties
        /// <summary>
        /// Improvements.
        /// </summary>
        public List<Improvement> Improvements
        {
            get
            {
                return _lstImprovements;
            }
        }

        /// <summary>
        /// Contacts and Enemies.
        /// </summary>
        public List<Contact> Contacts
        {
            get
            {
                return _lstContacts;
            }
        }

        /// <summary>
        /// Spirits and Sprites.
        /// </summary>
        public List<Spirit> Spirits
        {
            get
            {
                return _lstSpirits;
            }
        }

        /// <summary>
        /// Magician Spells.
        /// </summary>
        public List<Spell> Spells
        {
            get
            {
                return _lstSpells;
            }
        }

        /// <summary>
        /// Foci.
        /// </summary>
        public List<Focus> Foci
        {
            get
            {
                return _lstFoci;
            }
        }

        /// <summary>
        /// Stacked Foci.
        /// </summary>
        public List<StackedFocus> StackedFoci
        {
            get
            {
                return _lstStackedFoci;
            }
        }

        /// <summary>
        /// Adept Powers.
        /// </summary>
        public BindingList<Power> Powers
        {
            get
            {
                return _lstPowers;
            }
        }

        /// <summary>
        /// Technomancer Complex Forms.
        /// </summary>
        public List<ComplexForm> ComplexForms
        {
            get
            {
                return _lstComplexForms;
            }
        }

        /// <summary>
        /// AI Programs and Advanced Programs
        /// </summary>
        public List<AIProgram> AIPrograms
        {
            get
            {
                return _lstAIPrograms;
            }
        }

        /// <summary>
        /// Martial Arts.
        /// </summary>
        public List<MartialArt> MartialArts
        {
            get
            {
                return _lstMartialArts;
            }
        }

        /// <summary>
        /// Martial Arts Maneuvers.
        /// </summary>
        public List<MartialArtManeuver> MartialArtManeuvers
        {
            get
            {
                return _lstMartialArtManeuvers;
            }
        }

        /// <summary>
        /// Limit Modifiers.
        /// </summary>
        public List<LimitModifier> LimitModifiers
        {
            get
            {
                return _lstLimitModifiers;
            }
        }

        /// <summary>
        /// Armor.
        /// </summary>
        public List<Armor> Armor
        {
            get
            {
                return _lstArmor;
            }
        }

        /// <summary>
        /// Cyberware and Bioware.
        /// </summary>
        public List<Cyberware> Cyberware
        {
            get
            {
                return _lstCyberware;
            }
        }

        /// <summary>
        /// Weapons.
        /// </summary>
        public List<Weapon> Weapons
        {
            get
            {
                return _lstWeapons;
            }
        }

        /// <summary>
        /// Lifestyles.
        /// </summary>
        public List<Lifestyle> Lifestyles
        {
            get
            {
                return _lstLifestyles;
            }
        }

        /// <summary>
        /// Gear.
        /// </summary>
        public List<Gear> Gear
        {
            get
            {
                return _lstGear;
            }
        }

        /// <summary>
        /// Vehicles.
        /// </summary>
        public List<Vehicle> Vehicles
        {
            get
            {
                return _lstVehicles;
            }
        }

        /// <summary>
        /// Metamagics and Echoes.
        /// </summary>
        public List<Metamagic> Metamagics
        {
            get
            {
                return _lstMetamagics;
            }
        }

        /// <summary>
        /// Enhancements.
        /// </summary>
        public List<Enhancement> Enhancements
        {
            get
            {
                return _lstEnhancements;
            }
        }

        /// <summary>
        /// Arts.
        /// </summary>
        public List<Art> Arts
        {
            get
            {
                return _lstArts;
            }
        }

        /// <summary>
        /// Critter Powers.
        /// </summary>
        public List<CritterPower> CritterPowers
        {
            get
            {
                return _lstCritterPowers;
            }
        }

        /// <summary>
        /// Initiation and Submersion Grades.
        /// </summary>
        public List<InitiationGrade> InitiationGrades
        {
            get
            {
                return _lstInitiationGrades;
            }
        }

        /// <summary>
        /// Expenses (Karma and Nuyen).
        /// </summary>
        public List<ExpenseLogEntry> ExpenseEntries
        {
            get
            {
                return _lstExpenseLog;
            }
        }

        /// <summary>
        /// Qualities (Positive and Negative).
        /// </summary>
        public List<Quality> Qualities
        {
            get
            {
                return _lstQualities;
            }
        }
        /// <summary>
        /// Qualities (Positive and Negative).
        /// </summary>
        public List<LifestyleQuality> LifestyleQualities
        {
            get
            {
                return _lstLifestyleQualities;
            }
        }

        /// <summary>
        /// Life modules
        /// </summary>
        //public List<LifeModule> LifeModules
        //{
        //    get { return _lstLifeModules; }
        //}

        /// <summary>
        /// Locations.
        /// </summary>
        public List<string> Locations
        {
            get
            {
                return _lstLocations;
            }
        }

        /// <summary>
        /// Armor Bundles.
        /// </summary>
        public List<string> ArmorBundles
        {
            get
            {
                return _lstArmorBundles;
            }
        }

        /// <summary>
        /// Weapon Locations.
        /// </summary>
        public List<string> WeaponLocations
        {
            get
            {
                return _lstWeaponLocations;
            }
        }

        /// <summary>
        /// Improvement Groups.
        /// </summary>
        public List<string> ImprovementGroups
        {
            get
            {
                return _lstImprovementGroups;
            }
        }

        /// <summary>
        /// Calendar.
        /// </summary>
        public List<CalendarWeek> Calendar
        {
            get
            {
                return _lstCalendar;
            }
        }
#endregion

#region Armor Properties
        /// <summary>
        /// The Character's highest Armor Rating.
        /// </summary>
        public int ArmorRating
        {
            get
            {
                int intHighest = 0;
                string strHighest = string.Empty;
                bool blnCustomFit = false;

                // Run through the list of Armor currently worn and retrieve the highest total Armor rating.
                foreach (Armor objArmor in _lstArmor.Where(objArmor => !objArmor.ArmorValue.StartsWith("+")))
                {
                    // Don't look at items that start with "+" since we'll consider those next.
                    if (objArmor.TotalArmor > intHighest && objArmor.Equipped)
                    {
                        intHighest = objArmor.TotalArmor;
                        strHighest = objArmor.Name;
                        blnCustomFit = objArmor.Category == "High-Fashion Armor Clothing";
                    }
                }

                int intArmor = intHighest;

                // Run through the list of Armor currently worn again and look at non-Clothing items that start with "+" since they stack with the highest Armor.
                int intStacking = 0;
                foreach (Armor objArmor in _lstArmor)
                {
                    if (objArmor.ArmorValue.StartsWith("+") && objArmor.Category != "Clothing" && objArmor.Equipped)
                        intStacking += objArmor.TotalArmor;
                    if (objArmor.TotalArmor > intHighest && objArmor.Equipped && !objArmor.ArmorValue.StartsWith("+"))
                    {
                        strHighest = objArmor.Name;
                        blnCustomFit = (objArmor.Category == "High-Fashion Armor Clothing");
                    }
                }

                foreach (Armor objArmor in _lstArmor.Where(objArmor => (objArmor.ArmorValue.StartsWith("+") || objArmor.ArmorOverrideValue.StartsWith("+")) && objArmor.Equipped))
                {
                    if (objArmor.Category == "High-Fashion Armor Clothing" && blnCustomFit)
                    {
                        if (objArmor.ArmorMods.Any(objMod => objMod.Name == "Custom Fit (Stack)" && objMod.Extra == strHighest))
                            intStacking += Convert.ToInt32(objArmor.TotalArmor);
                    }
                }

                // Run through the list of Armor currently worn again and look at Clothing items that start with "+" since they stack with eachother.
                int intClothing = _lstArmor.Where(objArmor => objArmor.ArmorValue.StartsWith("+") && objArmor.Category == "Clothing" && objArmor.Equipped).Sum(objArmor => objArmor.TotalArmor);

                if (intClothing > intArmor)
                    intArmor = intClothing;

                return intArmor + intStacking;
            }
        }

        /// <summary>
        /// The Character's total Armor Rating.
        /// </summary>
        public int TotalArmorRating
        {
            get
            {
                return ArmorRating + ImprovementManager.ValueOf(this, Improvement.ImprovementType.Armor);
            }
        }

        /// <summary>
        /// The Character's total Armor Rating against Fire attacks.
        /// </summary>
        public int TotalFireArmorRating
        {
            get
            {
                return TotalArmorRating + ImprovementManager.ValueOf(this, Improvement.ImprovementType.FireArmor);
            }
        }

        /// <summary>
        /// The Character's total Armor Rating against Cold attacks.
        /// </summary>
        public int TotalColdArmorRating
        {
            get
            {
                return TotalArmorRating + ImprovementManager.ValueOf(this, Improvement.ImprovementType.ColdArmor);
            }
        }

        /// <summary>
        /// The Character's total Armor Rating against Electricity attacks.
        /// </summary>
        public int TotalElectricityArmorRating
        {
            get
            {
                return TotalArmorRating + ImprovementManager.ValueOf(this, Improvement.ImprovementType.ElectricityArmor);
            }
        }

        /// <summary>
        /// The Character's total Armor Rating against Acid attacks.
        /// </summary>
        public int TotalAcidArmorRating
        {
            get
            {
                return TotalArmorRating + ImprovementManager.ValueOf(this, Improvement.ImprovementType.AcidArmor);
            }
        }

        /// <summary>
        /// The Character's total Armor Rating against falling damage (AP -4 not factored in).
        /// </summary>
        public int TotalFallingArmorRating
        {
            get
            {
                return TotalArmorRating + ImprovementManager.ValueOf(this, Improvement.ImprovementType.FallingArmor);
            }
        }

        /// <summary>
        /// The Character's total bonus to Dodge Rating (to add on top of REA + INT).
        /// </summary>
        public int TotalBonusDodgeRating
        {
            get
            {
                return ImprovementManager.ValueOf(this, Improvement.ImprovementType.Dodge);
            }
        }

        /// <summary>
        /// Armor Encumbrance modifier from Armor.
        /// </summary>
        public int ArmorEncumbrance
        {
            get
            {
                string strHighest= string.Empty;
                bool blnCustomFit = false;
                int intHighest = 0;
                int intTotalA = 0;
                // Run through the list of Armor currently worn and retrieve the highest total Armor rating.
                // This is used for Custom-Fit armour's stacking.
                foreach (Armor objArmor in _lstArmor)
                {
                    if (objArmor.TotalArmor > intHighest && objArmor.Equipped && !objArmor.ArmorValue.StartsWith("+"))
                    {
                        blnCustomFit = (objArmor.Category == "High-Fashion Armor Clothing");
                        intHighest = objArmor.TotalArmor;
                        strHighest = objArmor.Name;
                    }
                }
                foreach (Armor objArmor in _lstArmor.Where(objArmor => (objArmor.ArmorValue.StartsWith("+") || objArmor.ArmorOverrideValue.StartsWith("+")) && objArmor.Equipped))
                {
                    if (objArmor.Category == "High-Fashion Armor Clothing" && blnCustomFit)
                    {
                            foreach (ArmorMod objMod in objArmor.ArmorMods)
                            {
                                if (objMod.Name == "Custom Fit (Stack)" && objMod.Extra == strHighest)
                                {
                                    intTotalA += Convert.ToInt32(objArmor.TotalArmor);
                                }
                            }
                    }
                    else
                    {
                        intTotalA += objArmor.TotalArmor;
                    }
                }

                // calculate armor encumberance
                if (intTotalA > STR.TotalValue)
                    return (intTotalA - STR.TotalValue) / 2 * -1;  // we expect a negative number
                return 0;
            }
        }

#endregion

#region Condition Monitors
        /// <summary>
        /// Number of Physical Condition Monitor boxes.
        /// </summary>
        public int PhysicalCM
        {
            get
            {
                int intCMPhysical = 8;
                if (_strMetatype.Contains("A.I.") || _strMetatypeCategory == "Protosapients")
                {
                    // A.I.s add 1/2 their System to Physical CM since they do not have BOD.
                    intCMPhysical += (DEP.TotalValue + 1) / 2;
                }
                else
                {
                    intCMPhysical += (BOD.TotalValue + 1) / 2;
                }
                // Include Improvements in the Condition Monitor values.
                intCMPhysical += ImprovementManager.ValueOf(this, Improvement.ImprovementType.PhysicalCM);
                return intCMPhysical;
            }
        }

        /// <summary>
        /// Number of Stun Condition Monitor boxes.
        /// </summary>
        public int StunCM
        {
            get
            {
                int intCMStun = 0;
                // A.I. do not have a Stun Condition Monitor.
                if (!(_strMetatype.Contains("A.I.") || _strMetatypeCategory == "Protosapients"))
                {
                    intCMStun = 8 + (WIL.TotalValue + 1) / 2;
                // Include Improvements in the Condition Monitor values.
                intCMStun += ImprovementManager.ValueOf(this, Improvement.ImprovementType.StunCM);
                }
                return intCMStun;
            }
        }

        /// <summary>
        /// Number of Condition Monitor boxes are needed to reach a Condition Monitor Threshold.
        /// </summary>
        public int CMThreshold
        {
            get
            {
                int intCMThreshold = 3 + ImprovementManager.ValueOf(this, Improvement.ImprovementType.CMThreshold);
                return intCMThreshold;
            }
        }

        /// <summary>
        /// Number of additioal boxes appear before the first Physical Condition Monitor penalty.
        /// </summary>
        public int PhysicalCMThresholdOffset
        {
            get
            {
                int intCMThresholdOffset = ImprovementManager.ValueOf(this, Improvement.ImprovementType.CMThresholdOffset);
                // We're subtracting CM Threshold from the amount of CM boxes filled because you only need to ignore wounds up to your first wound threshold, not all wounds
                int intCMSharedThresholdOffset = intCMThresholdOffset + ImprovementManager.ValueOf(this, Improvement.ImprovementType.CMSharedThresholdOffset) - Math.Max(StunCMFilled - CMThreshold, 0);
                return Math.Max(intCMThresholdOffset, intCMSharedThresholdOffset);
            }
        }

        /// <summary>
        /// Number of additioal boxes appear before the first Stun Condition Monitor penalty.
        /// </summary>
        public int StunCMThresholdOffset
        {
            get
            {
                int intCMThresholdOffset = ImprovementManager.ValueOf(this, Improvement.ImprovementType.CMThresholdOffset);
                // We're subtracting CM Threshold from the amount of CM boxes filled because you only need to ignore wounds up to your first wound threshold, not all wounds
                int intCMSharedThresholdOffset = intCMThresholdOffset + ImprovementManager.ValueOf(this, Improvement.ImprovementType.CMSharedThresholdOffset) - Math.Max(PhysicalCMFilled - CMThreshold, 0);
                return Math.Max(intCMThresholdOffset, intCMSharedThresholdOffset);
            }
        }

        /// <summary>
        /// Number of Overflow Condition Monitor boxes.
        /// </summary>
        public int CMOverflow
        {
            get
            {
                int intCMOverflow = 0;
                // A.I. do not have an Overflow Condition Monitor.
                if (!(_strMetatype.Contains("A.I.") || _strMetatypeCategory == "Protosapients"))
                {
                // Characters get a number of overflow boxes equal to their BOD (plus any Improvements). One more boxes is added to mark the character as dead.
                    intCMOverflow = BOD.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.CMOverflow) + 1;
                }
                return intCMOverflow;
            }
        }

        /// <summary>
        /// Total modifiers from Condition Monitor damage.
        /// </summary>
        public int WoundModifiers
        {
            get
            {
                int intModifier = 0;
                foreach (Improvement objImprovement in _lstImprovements)
                {
                    if (objImprovement.ImproveSource == Improvement.ImprovementSource.ConditionMonitor && objImprovement.Enabled)
                        intModifier += objImprovement.Value;
                }

                return intModifier;
            }
        }
#endregion

#region Build Properties
        /// <summary>
        /// Method being used to build the character.
        /// </summary>
        public CharacterBuildMethod BuildMethod
        {
            get
            {
                return _objBuildMethod;
            }
            set
            {
                _objBuildMethod = value;
            }
        }

        /// <summary>
        /// Number of Build Points that are used to create the character.
        /// </summary>
        public int BuildPoints
        {
            get
            {
                return _intBuildPoints;
            }
            set
            {
                _intBuildPoints = value;
            }
        }

        /// <summary>
        /// Number of Build Points that are used to create the character.
        /// </summary>
        public int SumtoTen
        {
            get
            {
                return _intSumtoTen;
            }
            set
            {
                _intSumtoTen = value;
            }
        }
        /// <summary>
        /// Amount of Karma that is used to create the character.
        /// </summary>
        public int BuildKarma
        {
            get
            {
                return _intBuildKarma;
            }
            set
            {
                _intBuildKarma = value;
            }
        }

        /// <summary>
        /// Amount of Nuyen the character has.
        /// </summary>
        public decimal Nuyen
        {
            get
            {
                return _decNuyen;
            }
            set
            {
                _decNuyen = value;
            }
        }

        /// <summary>
        /// Amount of Nuyen the character started with via the priority system.
        /// </summary>
        public decimal StartingNuyen
        {
            get
            {
                return _decStartingNuyen;
            }
            set
            {
                _decStartingNuyen = value;
            }
        }

        /// <summary>
        /// Number of Build Points put into Nuyen.
        /// </summary>
        public decimal NuyenBP
        {
            get
            {
                return _decNuyenBP;
            }
            set
            {
                _decNuyenBP = value;
            }
        }

        /// <summary>
        /// Number of Bonded Foci discounted by an Adept Way.
        /// </summary>
        public int AdeptWayDiscount
        {
            get
            {
                return _intAdeptWayDiscount;
            }
            set
            {
                _intAdeptWayDiscount = value;
            }
        }

        /// <summary>
        /// Maximum number of Build Points that can be spent on Nuyen.
        /// </summary>
        public decimal NuyenMaximumBP
        {
            get
            {
                decimal decImprovement = ImprovementManager.ValueOf(this, Improvement.ImprovementType.NuyenMaxBP);
                if (_objBuildMethod == CharacterBuildMethod.Karma)
                    decImprovement *= 2.0m;

                // If UnrestrictedNueyn is enabled, return the number of BP or Karma the character is being built with, otherwise use the standard value attached to the character.
                if (_objOptions.UnrestrictedNuyen)
                {
                    if (_intBuildKarma > 0)
                        return _intBuildKarma;
                    else
                        return 1000.0m;
                }
                else
                    return Math.Max(_decNuyenMaximumBP, decImprovement);
            }
            set
            {
                _decNuyenMaximumBP = value;
            }
        }

        /// <summary>
        /// The calculated Astral Limit.
        /// </summary>
        public int LimitAstral
        {
            get
            {
                return Math.Max(LimitMental, LimitSocial);
            }
        }

        /// <summary>
        /// The calculated Physical Limit.
        /// </summary>
        public int LimitPhysical
        {
            get
            {
                if (_strMetatype == "A.I.")
                {
                    return HomeNodeVehicle?.Handling ?? 0;
                }
                int intLimit = (STR.TotalValue * 2 + BOD.TotalValue + REA.TotalValue + 2) / 3;
                return intLimit + ImprovementManager.ValueOf(this, Improvement.ImprovementType.PhysicalLimit);
            }
        }

        /// <summary>
        /// The calculated Mental Limit.
        /// </summary>
        public int LimitMental
        {
            get
            {
                int intLimit = (LOG.TotalValue * 2 + INT.TotalValue + WIL.TotalValue + 2) / 3;
                if (_strMetatype == "A.I.")
                {
                    if (HomeNodeVehicle != null)
                    {
                        if (HomeNodeVehicle.CalculatedSensor > intLimit)
                        {
                            intLimit = HomeNodeVehicle.CalculatedSensor;
                        }
                        if (HomeNodeVehicle.DeviceRating > intLimit)
                        {
                            intLimit = HomeNodeVehicle.DeviceRating;
                        }
                    }
                    else if (HomeNodeCommlink != null)
                    {
                        if (HomeNodeCommlink.TotalDataProcessing > intLimit)
                        {
                            intLimit = HomeNodeCommlink.TotalDataProcessing;
                        }
                    }
                }
                return intLimit + ImprovementManager.ValueOf(this, Improvement.ImprovementType.MentalLimit);
            }
        }

        /// <summary>
        /// The calculated Social Limit.
        /// </summary>
        public int LimitSocial
        {
            get
            {
                int intLimit;
                if (_strMetatype == "A.I." && (HomeNodeVehicle != null || HomeNodeCommlink != null))
                {
                    int intHomeNodePilot = _objHomeNodeVehicle?.Pilot ?? 0;
                    int intHomeNodeDP = Math.Max(_objHomeNodeCommlink?.TotalDataProcessing ?? 0, _objHomeNodeVehicle?.DeviceRating ?? 0);
                    if (intHomeNodeDP >= intHomeNodePilot)
                    {
                        intLimit = (CHA.TotalValue + intHomeNodeDP + WIL.TotalValue + Convert.ToInt32(Math.Ceiling(Essence)) + 2) / 3;
                    }
                    else
                    {
                        intLimit = (CHA.TotalValue + intHomeNodePilot + WIL.TotalValue + Convert.ToInt32(Math.Ceiling(Essence)) + 2) / 3;
                    }
                }
                else
                {
                    intLimit = (CHA.TotalValue * 2 + WIL.TotalValue + Convert.ToInt32(Math.Ceiling(Essence)) + 2) / 3;
                }
                return intLimit + ImprovementManager.ValueOf(this, Improvement.ImprovementType.SocialLimit);
            }
        }

#endregion

#region Metatype/Metavariant Information
        /// <summary>
        /// Character's Metatype.
        /// </summary>
        public string Metatype
        {
            get
            {
                return _strMetatype;
            }
            set
            {
                _strMetatype = value;
            }
        }

        /// <summary>
        /// Character's Metavariant.
        /// </summary>
        public string Metavariant
        {
            get
            {
                return _strMetavariant;
            }
            set
            {
                _strMetavariant = value;
            }
        }

        /// <summary>
        /// Metatype Category.
        /// </summary>
        public string MetatypeCategory
        {
            get
            {
                return _strMetatypeCategory;
            }
            set
            {
                _strMetatypeCategory = value;
            }
        }

        public int LimbCount(string strLimbSlot = "")
        {
            if (string.IsNullOrEmpty(strLimbSlot))
            {
                return Options.LimbCount + ImprovementManager.ValueOf(this, Improvement.ImprovementType.AddLimb);
            }
            int intReturn = 1 + ImprovementManager.ValueOf(this, Improvement.ImprovementType.AddLimb, false, strLimbSlot);
            if (strLimbSlot == "arm" || strLimbSlot == "leg")
                intReturn += 1;
            return intReturn;
        }

        /// <summary>
        /// Character's Movement rate.
        /// </summary>
        public string Movement
        {
            get
            {
                // Don't attempt to do anything if the character's Movement is "Special" (typically for A.I.s).
                if (_strMovement == "Special")
                {
                    return "Special";
                }
                if (string.IsNullOrWhiteSpace(_strWalk) || string.IsNullOrWhiteSpace(_strRun) || string.IsNullOrWhiteSpace(_strSprint) || string.IsNullOrWhiteSpace(_strMovement) || (MetatypeCategory == "Shapeshifter" && (string.IsNullOrWhiteSpace(_strWalkAlt) || string.IsNullOrWhiteSpace(_strRunAlt) || string.IsNullOrWhiteSpace(_strSprintAlt))))
                {
                    string strReturn = string.Empty;
                    XmlDocument objXmlDocument = XmlManager.Load(_blnIsCritter ? "critters.xml" : "metatypes.xml");
                    XmlNode variant = objXmlDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + _strMetatype + "\"]/metavariants/metavariant[name = \"" + _strMetavariant + "\"]");
                    XmlNode meta = objXmlDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + _strMetatype + "\"]");

                    strReturn = variant?["movement"]?.InnerText ?? meta?["movement"]?.InnerText ?? string.Empty;
                    _strRun = variant?["run"]?.InnerText ?? meta?["run"]?.InnerText ?? string.Empty;
                    _strWalk = variant?["walk"]?.InnerText ?? meta?["walk"]?.InnerText ?? string.Empty;
                    _strSprint = variant?["sprint"]?.InnerText ?? meta?["sprint"]?.InnerText ?? string.Empty;

                    _strRunAlt = variant?["run"]?.Attributes["alt"]?.InnerText ?? meta?["run"].Attributes["alt"]?.InnerText ?? string.Empty;
                    _strWalkAlt = variant?["walk"]?.Attributes["alt"]?.InnerText ?? meta?["walk"].Attributes["alt"]?.InnerText ?? string.Empty;
                    _strSprintAlt = variant?["sprint"]?.Attributes["alt"]?.InnerText ?? meta?["sprint"].Attributes["alt"]?.InnerText ?? string.Empty;
                    if (strReturn == "Special")
                    {
                        return "Special";
                    }
                }

                return CalculatedMovement("Ground", true);
            }
            set
            {
                _strMovement = value;
            }
        }
        /// <summary>
        /// Character's Movement rate in Metres per Combat Turn or Kilometres Per Hour.
        /// </summary>
        public string CalculatedMovementSpeed
        {
           get
            {
                string strReturn = Movement;
                if (strReturn.Contains(','))
                {
                    string[] strMovementWithSprint = strReturn.Split(',');
                    strReturn = strMovementWithSprint[0].Trim();
                }
                if (strReturn.Contains('/'))
                {
                    string[] strMovement = strReturn.Split('/');
                    decimal decWalking = Convert.ToDecimal(strMovement[0], GlobalOptions.CultureInfo);
                    decimal decRunning = Convert.ToDecimal(strMovement[1], GlobalOptions.CultureInfo);

                    decimal walkratekph = .001m * (60 * 20 * decWalking);
                    decimal runratekph = .001m * (60 * 20 * decRunning);
                    //decimal walkratemph = 0.6213712m * walkratekph;
                    //decimal runratemph = 0.6213712m * runratekph;

                    strReturn = string.Format(LanguageManager.GetString("Tip_CalculatedMovement"), decWalking.ToString("N2", GlobalOptions.CultureInfo), walkratekph.ToString("N2", GlobalOptions.CultureInfo), decRunning.ToString("N2", GlobalOptions.CultureInfo), runratekph.ToString("N2", GlobalOptions.CultureInfo));
                }

                return strReturn;
            }
        }

        /// <summary>
        /// Character's running Movement rate.
        /// <param name="strType">Takes one of three parameters: Ground, 2 for Swim, 3 for Fly. Returns 0 if the requested type isn't found.</param>
        /// </summary>
        private int WalkingRate(string strType = "Ground")
        {
            string[] strReturn;
            if (this.AttributeSection.AttributeCategory == CharacterAttrib.AttributeCategory.Standard)
            {
                strReturn = _strWalk.Split('/');
            }
            else
            {
                strReturn = _strWalkAlt.Split('/');
            }
            

            int intTmp = 0;
            if (Improvements.Any(i => i.ImproveType == Improvement.ImprovementType.WalkSpeed && i.ImprovedName == strType))
            {
                foreach (Improvement objImprovement in Improvements.Where(i => i.ImproveType == Improvement.ImprovementType.WalkSpeed && i.ImprovedName == strType))
                {
                    intTmp = Math.Max(intTmp, objImprovement.Value);
                }
                return intTmp;
            }
            
            switch (strType)
            {
                case "Fly":
                    if (strReturn.Length > 2)
                        int.TryParse(strReturn[2], out intTmp);
                    break;
                case "Swim":
                    if (strReturn.Length > 1)
                        int.TryParse(strReturn[1], out intTmp);
                    break;
                case "Ground":
                    if (strReturn.Length > 0)
                        int.TryParse(strReturn[0], out intTmp);
                    break;
            }
            return intTmp;
        }

        /// <summary>
        /// Character's running Movement rate.
        /// <param name="strType">Takes one of three parameters: Ground, 2 for Swim, 3 for Fly. Returns 0 if the requested type isn't found.</param>
        /// </summary>
        private int RunningRate(string strType = "Ground")
        {
            int intTmp = 0;
            if (Improvements.Any(i => i.ImproveType == Improvement.ImprovementType.RunSpeed && i.ImprovedName == strType))
            {
                Improvement imp = Improvements.First(i => i.ImproveType == Improvement.ImprovementType.RunSpeed && i.ImprovedName == strType);
                return imp.Value;
            }
            string[] strReturn;
            if (this.AttributeSection.AttributeCategory == CharacterAttrib.AttributeCategory.Standard)
            {
                strReturn = _strRun.Split('/');
            }
            else
            {
                strReturn = _strRunAlt.Split('/');
            }

            switch (strType)
            {
                case "Fly":
                    if (strReturn.Length > 2)
                        int.TryParse(strReturn[2], out intTmp);
                    break;
                case "Swim":
                    if (strReturn.Length > 1)
                        int.TryParse(strReturn[1], out intTmp);
                    break;
                case "Ground":
                    if (strReturn.Length > 0)
                        int.TryParse(strReturn[0], out intTmp);
                    break;
            }
            return intTmp;
        }

        /// <summary>
        /// Character's sprinting Movement rate (meters per hit).
        /// <param name="strType">Takes one of three parameters: Ground, 2 for Swim, 3 for Fly. Returns 0 if the requested type isn't found.</param>
        /// </summary>
        private decimal SprintingRate(string strType = "Ground")
        {
            decimal decTmp = 0;
            if (Improvements.Any(i => i.ImproveType == Improvement.ImprovementType.SprintSpeed && i.ImprovedName == strType))
            {
                Improvement imp = Improvements.First(i => i.ImproveType == Improvement.ImprovementType.SprintSpeed && i.ImprovedName == strType);
                return imp.Value;
            }
            string[] strReturn;
            if (this.AttributeSection.AttributeCategory == CharacterAttrib.AttributeCategory.Standard)
            {
                strReturn = _strSprint.Split('/');
            }
            else
            {
                strReturn = _strSprintAlt.Split('/');
            }
            int intTmp = 0;
                switch (strType)
                {
                case "Fly":
                    if (strReturn.Length > 2)
                        decimal.TryParse(strReturn[2], NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out decTmp);
                    break;
                case "Swim":
                    if (strReturn.Length > 1)
                        decimal.TryParse(strReturn[1], NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out decTmp);
                    break;
                case "Ground":
                    if (strReturn.Length > 0)
                        decimal.TryParse(strReturn[0], NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out decTmp);
                    break;
            }
            return decTmp;
        }

        private string CalculatedMovement(string strMovementType, bool blnUseCyberlegs = false)
        {
            decimal decSprint = SprintingRate(strMovementType) + ImprovementManager.ValueOf(this, Improvement.ImprovementType.SprintBonus, false, strMovementType) / 100.0m;
            decimal decRun = RunningRate(strMovementType) + ImprovementManager.ValueOf(this, Improvement.ImprovementType.RunMultiplier, false, strMovementType);
            decimal decWalk = WalkingRate(strMovementType) + ImprovementManager.ValueOf(this, Improvement.ImprovementType.WalkMultiplier, false, strMovementType);
            // Everything else after this just multiplies values, so we can check for zeroes here
            if (decWalk == 0 && decRun == 0 && decSprint == 0)
            {
                return "0";
            }
            decSprint *= 1.0m + ImprovementManager.ValueOf(this, Improvement.ImprovementType.SprintBonusPercent, false, strMovementType) / 100.0m;
            decRun *= 1.0m + ImprovementManager.ValueOf(this, Improvement.ImprovementType.RunMultiplierPercent, false, strMovementType) / 100.0m;
            decWalk *= 1.0m + ImprovementManager.ValueOf(this, Improvement.ImprovementType.WalkMultiplierPercent, false, strMovementType) / 100.0m;

            int intAGI = AGI.CalculatedTotalValue(false);
            int intSTR = STR.CalculatedTotalValue(false);
            if (_objOptions.CyberlegMovement && blnUseCyberlegs && _lstCyberware.Any(objCyber => objCyber.LimbSlot == "leg"))
            {
                int intTempAGI = int.MaxValue;
                int intTempSTR = int.MaxValue;
                int intLegs = 0;
                foreach (Cyberware objCyber in _lstCyberware.Where(objCyber => objCyber.LimbSlot == "leg"))
                {
                    intLegs += objCyber.LimbSlotCount;
                    intTempAGI = Math.Min(intTempAGI, objCyber.TotalAgility);
                    intTempSTR = Math.Min(intTempSTR, objCyber.TotalStrength);
                }
                if (intLegs >= 2)
                {
                    intAGI = intTempAGI;
                    intSTR = intTempSTR;
                }
            }

            string strReturn = string.Empty;
            if (strMovementType == "Swim")
            {
                decWalk *= (intAGI + intSTR) * 0.5m;
                strReturn = $"{decWalk:###,###,##0.##}, {decSprint:###,###,##0.##}m/ hit";
            }
            else
            {
                decWalk *= intAGI;
                decRun *= intAGI;
                strReturn = $"{decWalk:###,###,##0.##}/{decRun:###,###,##0.##}, {decSprint:###,###,##0.##}m/ hit";
            }
            return strReturn;
        }

        /// <summary>
        /// Character's Swim rate.
        /// </summary>
        public string Swim
        {
            get
            {
                // Don't attempt to do anything if the character's Movement is "Special" (typically for A.I.s).
                if (_strMovement == "Special")
                {
                    return "Special";
                }

                XmlDocument objXmlDocument = XmlManager.Load(_blnIsCritter ? "critters.xml" : "metatypes.xml");
                XmlNode objXmlNode = objXmlDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + _strMetatype + "\"]");
                if (objXmlNode != null)
                {
                    string strReturn = string.Empty;
                    objXmlNode.TryGetStringFieldQuickly("movement", ref strReturn);
                    if (strReturn == "Special")
                    {
                        return "Special";
                    }
                }
                return CalculatedMovement("Swim");
            }
        }

        /// <summary>
        /// Character's Fly rate.
        /// </summary>
        public string Fly
        {
            get
            {
                // Don't attempt to do anything if the character's Movement is "Special" (typically for A.I.s).
                if (_strMovement == "Special")
                {
                    return "Special";
                }

                XmlDocument objXmlDocument = XmlManager.Load(_blnIsCritter ? "critters.xml" : "metatypes.xml");
                XmlNode objXmlNode = objXmlDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + _strMetatype + "\"]");
                if (objXmlNode != null)
                {
                    string strReturn = string.Empty;
                    objXmlNode.TryGetStringFieldQuickly("movement", ref strReturn);
                    if (strReturn == "Special")
                    {
                    return "Special";
                    }
                }

                return CalculatedMovement("Fly");
            }
        }

        /// <summary>
        /// Full Movement (Movement, Swim, and Fly) for printouts.
        /// </summary>
        private string FullMovement()
        {
            string strReturn = string.Empty;
            string strGroundMovement = Movement;
            string strSwimMovement = Swim;
            string strFlyMovement = Fly;
            if (!string.IsNullOrEmpty(strGroundMovement) && strGroundMovement != "0")
                strReturn += strGroundMovement + ", ";
            if (!string.IsNullOrEmpty(strSwimMovement) && strSwimMovement != "0")
                strReturn += LanguageManager.GetString("Label_OtherSwim") + " " + strSwimMovement + ", ";
            if (!string.IsNullOrEmpty(strFlyMovement) && strFlyMovement != "0")
                strReturn += LanguageManager.GetString("Label_OtherFly") + " " + strFlyMovement + ", ";

            // Remove the trailing ", ".
            if (!string.IsNullOrEmpty(strReturn))
                strReturn = strReturn.Substring(0, strReturn.Length - 2);

            return strReturn;
        }

        /// <summary>
        /// BP cost of character's Metatype.
        /// </summary>
        public int MetatypeBP
        {
            get
            {
                return _intMetatypeBP;
            }
            set
            {
                _intMetatypeBP = value;
            }
        }

        /// <summary>
        /// Whether or not the character is a non-Free Sprite.
        /// </summary>
        public bool IsSprite
        {
            get
            {
                if (_strMetatypeCategory.EndsWith("Sprites") && !_strMetatypeCategory.StartsWith("Free"))
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// Whether or not the character is a Free Sprite.
        /// </summary>
        public bool IsFreeSprite
        {
            get
            {
                if (_strMetatypeCategory == "Free Sprite")
                    return true;
                else
                    return false;
            }
        }
#endregion

#region Special Functions and Enabled Check Properties

        /// <summary>
        /// Whether or not Adept options are enabled.
        /// </summary>
        public bool AdeptEnabled
        {
            get
            {
                return _blnAdeptEnabled;
            }
            set
            {
                bool blnOldValue = _blnAdeptEnabled;
                _blnAdeptEnabled = value;
                    if (blnOldValue != value)
                    AdeptTabEnabledChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Whether or not Magician options are enabled.
        /// </summary>
        public bool MagicianEnabled
        {
            get
            {
                return _blnMagicianEnabled;
            }
            set
            {
                bool blnOldValue = _blnMagicianEnabled;
                _blnMagicianEnabled = value;
                    if (blnOldValue != value)
                    MagicianTabEnabledChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Whether or not Technomancer options are enabled.
        /// </summary>
        public bool TechnomancerEnabled
        {
            get
            {
                return _blnTechnomancerEnabled;
            }
            set
            {
                bool blnOldValue = _blnTechnomancerEnabled;
                _blnTechnomancerEnabled = value;
                if (blnOldValue != value)
                    TechnomancerTabEnabledChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Whether or not Advanced Program options are enabled.
        /// </summary>
        public bool AdvancedProgramsEnabled
        {
            get
            {
                return _blnAdvancedProgramsEnabled;
            }
            set
                {
                bool blnOldValue = _blnAdvancedProgramsEnabled;
                _blnAdvancedProgramsEnabled = value;
                    if (blnOldValue != value)
                    AdvancedProgramsTabEnabledChanged?.Invoke(this);
            }
                }

        /// <summary>
        /// Whether or not Cyberware options are disabled.
        /// </summary>
        public bool CyberwareDisabled
        {
            get
                {
                return _blnCyberwareDisabled;
                }
            set
            {
                bool blnOldValue = _blnCyberwareDisabled;
                _blnCyberwareDisabled = value;
                if (blnOldValue != value)
                    CyberwareTabDisabledChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Whether or not the Initiation tab should be shown (override for BP mode).
        /// </summary>
        public bool InitiationEnabled
        {
            get
            {
                return _blnInitiationEnabled;
            }
            set
            {
                bool blnOldValue = _blnInitiationEnabled;
                _blnInitiationEnabled = value;
                    if (blnOldValue != value)
                    InitiationTabEnabledChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Whether or not Critter options are enabled.
        /// </summary>
        public bool CritterEnabled
        {
            get
            {
                return _blnCritterEnabled;
            }
            set
            {
                bool blnOldValue = _blnCritterEnabled;
                _blnCritterEnabled = value;
                    if (blnOldValue != value)
                    CritterTabEnabledChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Whether or not Black Market Discount is enabled.
        /// </summary>
        public bool BlackMarketDiscount
        {
            get
            {
                return _blnBlackMarketDiscount;
            }
            set
            {
                bool blnOldValue = _blnBlackMarketDiscount;
                _blnBlackMarketDiscount = value;
                    if (blnOldValue != value)
                    BlackMarketEnabledChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Whether or not user is getting free bioware from Prototype Transhuman.
        /// </summary>
        public decimal PrototypeTranshuman
        {
            get
            {
                return _decPrototypeTranshuman;
            }
            set
            {
                _decPrototypeTranshuman = value;
            }
        }

        /// <summary>
        /// Whether or not Friends in High Places is enabled.
        /// </summary>
        public bool FriendsInHighPlaces
        {
            get
            {
                return _blnFriendsInHighPlaces;
            }
            set
            {
                bool blnOldValue = _blnFriendsInHighPlaces;
                _blnFriendsInHighPlaces = value;
                    if (blnOldValue != value)
                    FriendsInHighPlacesChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Whether or not ExCon is enabled.
        /// </summary>
        public bool ExCon
        {
            get
            {
                return _blnExCon;
            }
            set
            {
                bool blnOldValue = _blnExCon;
                _blnExCon = value;
                    if (blnOldValue != value)
                    ExConChanged?.Invoke(this);
            }
        }
        /// <summary>
        /// Value of the Trust Fund quality.
        /// </summary>
        public int TrustFund
        {
            get
            {
                return _intTrustFund;
            }
            set
            {
                int intOldValue = _intTrustFund;
                _intTrustFund = value;
                    if (intOldValue != value)
                    TrustFundChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Whether or not RestrictedGear is enabled.
        /// </summary>
        public bool RestrictedGear
        {
            get
            {
                return _blnRestrictedGear;
            }
            set
            {
                bool blnOldValue = _blnRestrictedGear;
                _blnRestrictedGear = value;
                    if (blnOldValue != value)
                    RestrictedGearChanged?.Invoke(this);
            }
        }
        /// <summary>
        /// Whether or not Overclocker is enabled.
        /// </summary>
        public bool Overclocker
        {
            get
            {
                return _blnOverclocker;
            }
            set
            {
                bool blnOldValue = _blnOverclocker;
                _blnOverclocker = value;
                    if (blnOldValue != value)
                    OverclockerChanged?.Invoke(this);
            }
        }
        /// <summary>
        /// Whether or not MadeMan is enabled.
        /// </summary>
        public bool MadeMan
        {
            get
            {
                return _blnMadeMan;
            }
            set
            {
                bool blnOldValue = _blnMadeMan;
                _blnMadeMan = value;
                    if (blnOldValue != value)
                    MadeManChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Whether or not LightningReflexes is enabled.
        /// </summary>
        public bool LightningReflexes
        {
            get
            {
                return _blnLightningReflexes;
            }
            set
            {
                _blnLightningReflexes = value;

            }
        }
        /// <summary>
        /// Whether or not Fame is enabled.
        /// </summary>
        public bool Fame
        {
            get
            {
                return _blnFame;
            }
            set
            {
                bool blnOldValue = _blnFame;
                _blnFame = value;
                    if (blnOldValue != value)
                    FameChanged?.Invoke(this);
            }
        }
        /// <summary>
        /// Whether or not BornRich is enabled.
        /// </summary>
        public bool BornRich
        {
            get
            {
                return _blnBornRich;
            }
            set
            {
                bool blnOldValue = _blnBornRich;
                _blnBornRich = value;
                    if (blnOldValue != value)
                    BornRichChanged?.Invoke(this);
            }
        }
        /// <summary>
        /// Whether or not Erased is enabled.
        /// </summary>
        public bool Erased
        {
            get
            {
                return _blnErased;
            }
            set
            {
                bool blnOldValue = _blnErased;
                _blnErased = value;
                    if (blnOldValue != value)
                    ErasedChanged?.Invoke(this);
            }
        }
        /// <summary>
        /// Convert a string to a CharacterBuildMethod.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        public CharacterBuildMethod ConvertToCharacterBuildMethod(string strValue)
        {
            switch (strValue)
            {
                case "Karma":
                    return CharacterBuildMethod.Karma;
                case "SumtoTen":
                    return CharacterBuildMethod.SumtoTen;
                case "LifeModule":
                    return CharacterBuildMethod.LifeModule;
                default:
                    return CharacterBuildMethod.Priority;
            }
        }

        /// <summary>
        /// Extended Availability Test information for an item based on the character's Negotiate Skill.
        /// </summary>
        /// <param name="intCost">Item's cost.</param>
        /// <param name="strAvail">Item's Availability.</param>
        public string AvailTest(decimal decCost, string strAvail)
        {
            string strReturn;
            int intAvail;
            int.TryParse(strAvail.TrimEnd(LanguageManager.GetString("String_AvailRestricted")).TrimEnd(LanguageManager.GetString("String_AvailForbidden")), out intAvail);

            if (intAvail != 0 && (strAvail.EndsWith(LanguageManager.GetString("String_AvailRestricted")) || strAvail.EndsWith(LanguageManager.GetString("String_AvailForbidden"))))
            {
                string strInterval;
                int intTest = 0;
                // Determine the interval based on the item's price.
                if (decCost <= 100.0m)
                    strInterval = "12 " + LanguageManager.GetString("String_Hours");
                else if (decCost > 100.0m && decCost <= 1000.0m)
                    strInterval = "1 " + LanguageManager.GetString("String_Day");
                else if (decCost > 1000.0m && decCost <= 10000.0m)
                    strInterval = "2 " + LanguageManager.GetString("String_Days");
                else
                    strInterval = "1 " + LanguageManager.GetString("String_Week");

                // Find the character's Negotiation total.
                Skill objSkill = SkillsSection.GetActiveSkill("Negotiation");
                if (objSkill != null)
                    intTest = objSkill.Pool;

                strReturn = intTest.ToString() + " (" + intAvail.ToString() + ", " + strInterval + ")";
            }
            else
                strReturn = LanguageManager.GetString("String_None");

            return strReturn;
        }

        /// <summary>
        /// Whether or not Adapsin is enabled.
        /// </summary>
        public bool AdapsinEnabled
        {
            get
            {
                bool blnReturn = false;
                foreach (Improvement objImprovement in _lstImprovements)
                {
                    if (objImprovement.ImproveType == Improvement.ImprovementType.Adapsin && objImprovement.Enabled)
                    {
                        blnReturn = true;
                        break;
                    }
                }

                return blnReturn;
            }
        }

        /// <summary>
        /// Whether or not Burnout's Way is enabled.
        /// </summary>
        public bool BurnoutEnabled
        {
            get
            {
                return _lstQualities.Any(objQuality => objQuality.Name == "The Burnout's Way");
            }
        }

        /// <summary>
        /// Whether or not the character has access to Knowsofts and Linguasofts.
        /// </summary>
        public bool SkillsoftAccess
        {
            get
            {
                foreach (Improvement objImprovement in _lstImprovements)
                {
                    if (objImprovement.ImproveType == Improvement.ImprovementType.SkillsoftAccess && objImprovement.Enabled)
                        return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Determine whether or not the character has any Improvements of a given ImprovementType.
        /// </summary>
        /// <param name="objImprovementType">ImprovementType to search for.</param>
        public bool HasImprovement(Improvement.ImprovementType objImprovementType, bool blnRequireEnabled = false)
        {
            foreach (Improvement objImprovement in _lstImprovements)
            {
                if (objImprovement.ImproveType == objImprovementType)
                {
                    if (!blnRequireEnabled || objImprovement.Enabled)
                        return true;
                }
            }

            return false;
        }
#endregion

#region Application Properties
        /// <summary>
        /// The frmViewer window being used by the character.
        /// </summary>
        public frmViewer PrintWindow
        {
            get
            {
                return _frmPrintView;
            }
            set
            {
                _frmPrintView = value;
            }
        }
#endregion

#region Old Quality Conversion Code
        /// <summary>
        /// Convert Qualities that are still saved in the old format.
        /// </summary>
        private void ConvertOldQualities(XmlNodeList objXmlQualityList)
        {
            XmlDocument objXmlQualityDocument = XmlManager.Load("qualities.xml");
            XmlDocument objXmlMetatypeDocument = XmlManager.Load("metatypes.xml");

            // Convert the old Qualities.
            foreach (XmlNode objXmlQuality in objXmlQualityList)
            {
                if (objXmlQuality["name"] == null)
                {
                    _lstOldQualities.Add(objXmlQuality.InnerText);

                    string strForceValue = string.Empty;

                    XmlNode objXmlQualityNode = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + GetQualityName(objXmlQuality.InnerText) + "\"]");

                    // Re-create the bonuses for the Quality.
                    if (objXmlQualityNode.InnerXml.Contains("<bonus>"))
                    {
                        // Look for the existing Improvement.
                        foreach (Improvement objImprovement in _lstImprovements)
                        {
                            if (objImprovement.ImproveSource == Improvement.ImprovementSource.Quality && objImprovement.SourceName == objXmlQuality.InnerText && objImprovement.Enabled)
                            {
                                strForceValue = objImprovement.ImprovedName;
                                _lstImprovements.Remove(objImprovement);
                                break;
                            }
                        }
                    }

                    // Convert the item to the new Quality class.
                    Quality objQuality = new Quality(this);
                    List<Weapon> objWeapons = new List<Weapon>();
                    List<TreeNode> objWeaponNodes = new List<TreeNode>();
                    TreeNode objNode = new TreeNode();
                    objQuality.Create(objXmlQualityNode, this, QualitySource.Selected, objNode, objWeapons, objWeaponNodes, strForceValue);
                    _lstQualities.Add(objQuality);

                    // Add any created Weapons to the character.
                    foreach (Weapon objWeapon in objWeapons)
                        _lstWeapons.Add(objWeapon);
                }
            }

            // Take care of the Metatype information.
            XmlNode objXmlMetatype = objXmlMetatypeDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + _strMetatype + "\"]");
            if (objXmlMetatype == null)
            {
                objXmlMetatypeDocument = XmlManager.Load("critters.xml");
                objXmlMetatype = objXmlMetatypeDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + _strMetatype + "\"]");
            }

            // Positive Qualities.
            foreach (XmlNode objXmlMetatypeQuality in objXmlMetatype.SelectNodes("qualities/positive/quality"))
            {
                bool blnFound = false;
                // See if the Quality already exists in the character.
                foreach (Quality objCharacterQuality in _lstQualities)
                {
                    if (objCharacterQuality.Name == objXmlMetatypeQuality.InnerText)
                    {
                        blnFound = true;
                        break;
                    }
                }

                // If the Quality was not found, create it.
                if (!blnFound)
                {
                    string strForceValue = string.Empty;
                    TreeNode objNode = new TreeNode();
                    List<Weapon> objWeapons = new List<Weapon>();
                    List<TreeNode> objWeaponNodes = new List<TreeNode>();
                    Quality objQuality = new Quality(this);

                    if (objXmlMetatypeQuality.Attributes["select"] != null)
                        strForceValue = objXmlMetatypeQuality.Attributes["select"].InnerText;

                    XmlNode objXmlQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlMetatypeQuality.InnerText + "\"]");
                    objQuality.Create(objXmlQuality, this, QualitySource.Metatype, objNode, objWeapons, objWeaponNodes, strForceValue);
                    _lstQualities.Add(objQuality);

                    // Add any created Weapons to the character.
                    foreach (Weapon objWeapon in objWeapons)
                        _lstWeapons.Add(objWeapon);
                }
            }

            // Negative Qualities.
            foreach (XmlNode objXmlMetatypeQuality in objXmlMetatype.SelectNodes("qualities/negative/quality"))
            {
                bool blnFound = false;
                // See if the Quality already exists in the character.
                foreach (Quality objCharacterQuality in _lstQualities)
                {
                    if (objCharacterQuality.Name == objXmlMetatypeQuality.InnerText)
                    {
                        blnFound = true;
                        break;
                    }
                }

                // If the Quality was not found, create it.
                if (!blnFound)
                {
                    string strForceValue = string.Empty;
                    TreeNode objNode = new TreeNode();
                    List<Weapon> objWeapons = new List<Weapon>();
                    List<TreeNode> objWeaponNodes = new List<TreeNode>();
                    Quality objQuality = new Quality(this);

                    if (objXmlMetatypeQuality.Attributes["select"] != null)
                        strForceValue = objXmlMetatypeQuality.Attributes["select"].InnerText;

                    XmlNode objXmlQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlMetatypeQuality.InnerText + "\"]");
                    objQuality.Create(objXmlQuality, this, QualitySource.Metatype, objNode, objWeapons, objWeaponNodes, strForceValue);
                    _lstQualities.Add(objQuality);

                    // Add any created Weapons to the character.
                    foreach (Weapon objWeapon in objWeapons)
                        _lstWeapons.Add(objWeapon);
                }
            }

            // Do it all over again for Metavariants.
            if (!string.IsNullOrEmpty(_strMetavariant))
            {
                objXmlMetatype = objXmlMetatype.SelectSingleNode("metavariants/metavariant[name = \"" + _strMetavariant + "\"]");

                // Positive Qualities.
                foreach (XmlNode objXmlMetatypeQuality in objXmlMetatype.SelectNodes("qualities/positive/quality"))
                {
                    bool blnFound = false;
                    // See if the Quality already exists in the character.
                    foreach (Quality objCharacterQuality in _lstQualities)
                    {
                        if (objCharacterQuality.Name == objXmlMetatypeQuality.InnerText)
                        {
                            blnFound = true;
                            break;
                        }
                    }

                    // If the Quality was not found, create it.
                    if (!blnFound)
                    {
                        string strForceValue = string.Empty;
                        TreeNode objNode = new TreeNode();
                        List<Weapon> objWeapons = new List<Weapon>();
                        List<TreeNode> objWeaponNodes = new List<TreeNode>();
                        Quality objQuality = new Quality(this);

                        if (objXmlMetatypeQuality.Attributes["select"] != null)
                            strForceValue = objXmlMetatypeQuality.Attributes["select"].InnerText;

                        XmlNode objXmlQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlMetatypeQuality.InnerText + "\"]");
                        objQuality.Create(objXmlQuality, this, QualitySource.Metatype, objNode, objWeapons, objWeaponNodes, strForceValue);
                        _lstQualities.Add(objQuality);

                        // Add any created Weapons to the character.
                        foreach (Weapon objWeapon in objWeapons)
                            _lstWeapons.Add(objWeapon);
                    }
                }

                // Negative Qualities.
                foreach (XmlNode objXmlMetatypeQuality in objXmlMetatype.SelectNodes("qualities/negative/quality"))
                {
                    bool blnFound = false;
                    // See if the Quality already exists in the character.
                    foreach (Quality objCharacterQuality in _lstQualities)
                    {
                        if (objCharacterQuality.Name == objXmlMetatypeQuality.InnerText)
                        {
                            blnFound = true;
                            break;
                        }
                    }

                    // If the Quality was not found, create it.
                    if (!blnFound)
                    {
                        string strForceValue = string.Empty;
                        TreeNode objNode = new TreeNode();
                        List<Weapon> objWeapons = new List<Weapon>();
                        List<TreeNode> objWeaponNodes = new List<TreeNode>();
                        Quality objQuality = new Quality(this);

                        if (objXmlMetatypeQuality.Attributes["select"] != null)
                            strForceValue = objXmlMetatypeQuality.Attributes["select"].InnerText;

                        XmlNode objXmlQuality = objXmlQualityDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlMetatypeQuality.InnerText + "\"]");
                        objQuality.Create(objXmlQuality, this, QualitySource.Metatype, objNode, objWeapons, objWeaponNodes, strForceValue);
                        _lstQualities.Add(objQuality);

                        // Add any created Weapons to the character.
                        foreach (Weapon objWeapon in objWeapons)
                            _lstWeapons.Add(objWeapon);
                    }
                }
            }
        }

        /// <summary>
        /// Get the name of a Quality by parsing out its BP cost.
        /// </summary>
        /// <param name="strQuality">String to parse.</param>
        private string GetQualityName(string strQuality)
        {
            string strTemp = strQuality;
            int intPos = strTemp.IndexOf('[');

            strTemp = strTemp.Substring(0, intPos - 1);

            return strTemp;
        }

        /// <summary>
        /// Check for older instances of certain qualities that were manually numbered to be replaced with multiple instances of the first level quality (so that it works with the level system)
        /// Returns true if it's a corrected quality, false otherwise
        /// </summary>
        /// <param name="strQuality">String to parse.</param>
        private bool CorrectedUnleveledQuality(XmlNode objOldXmlQuality)
        {
            XmlDocument objXmlDocument = XmlManager.Load("qualities.xml");
            XmlNode objXmlNewQuality = null;
            int intRanks = 0;
            switch (objOldXmlQuality["name"].InnerText)
            {
                case "Focused Concentration (Rating 1)":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Focused Concentration\"]");
                        intRanks = 1;
                        break;
                    }
                case "Focused Concentration (Rating 2)":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Focused Concentration\"]");
                        intRanks = 2;
                        break;
                    }
                case "Focused Concentration (Rating 3)":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Focused Concentration\"]");
                        intRanks = 3;
                        break;
                    }
                case "Focused Concentration (Rating 4)":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Focused Concentration\"]");
                        intRanks = 4;
                        break;
                    }
                case "Focused Concentration (Rating 5)":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Focused Concentration\"]");
                        intRanks = 5;
                        break;
                    }
                case "Focused Concentration (Rating 6)":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Focused Concentration\"]");
                        intRanks = 6;
                        break;
                    }
                case "High Pain Tolerance (Rating 1)":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"High Pain Tolerance\"]");
                        intRanks = 1;
                        break;
                    }
                case "High Pain Tolerance (Rating 2)":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"High Pain Tolerance\"]");
                        intRanks = 2;
                        break;
                    }
                case "High Pain Tolerance (Rating 3)":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"High Pain Tolerance\"]");
                        intRanks = 3;
                        break;
                    }
                case "Magic Resistance (Rating 1)":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Magic Resistance\"]");
                        intRanks = 1;
                        break;
                    }
                case "Magic Resistance (Rating 2)":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Magic Resistance\"]");
                        intRanks = 2;
                        break;
                    }
                case "Magic Resistance (Rating 3)":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Magic Resistance\"]");
                        intRanks = 3;
                        break;
                    }
                case "Magic Resistance (Rating 4)":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Magic Resistance\"]");
                        intRanks = 4;
                        break;
                    }
                case "Will to Live (Rating 1)":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Will to Live\"]");
                        intRanks = 1;
                        break;
                    }
                case "Will to Live (Rating 2)":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Will to Live\"]");
                        intRanks = 2;
                        break;
                    }
                case "Will to Live (Rating 3)":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Will to Live\"]");
                        intRanks = 3;
                        break;
                    }
                case "Gremlins (Rating 1)":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Gremlins\"]");
                        intRanks = 1;
                        break;
                    }
                case "Gremlins (Rating 2)":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Gremlins\"]");
                        intRanks = 2;
                        break;
                    }
                case "Gremlins (Rating 3)":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Gremlins\"]");
                        intRanks = 3;
                        break;
                    }
                case "Gremlins (Rating 4)":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Gremlins\"]");
                        intRanks = 4;
                        break;
                    }
                case "Aged (Rating 1)":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Aged\"]");
                        intRanks = 1;
                        break;
                    }
                case "Aged (Rating 2)":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Aged\"]");
                        intRanks = 2;
                        break;
                    }
                case "Aged (Rating 3)":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Aged\"]");
                        intRanks = 3;
                        break;
                    }
                case "Illness (Rating 1)":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Illness\"]");
                        intRanks = 1;
                        break;
                    }
                case "Illness (Rating 2)":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Illness\"]");
                        intRanks = 2;
                        break;
                    }
                case "Illness (Rating 3)":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Illness\"]");
                        intRanks = 3;
                        break;
                    }
                case "Perceptive I":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Perceptive\"]");
                        intRanks = 1;
                        break;
                    }
                case "Perceptive II":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Perceptive\"]");
                        intRanks = 2;
                        break;
                    }
                case "Spike Resistance I":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Spike Resistance\"]");
                        intRanks = 1;
                        break;
                    }
                case "Spike Resistance II":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Spike Resistance\"]");
                        intRanks = 2;
                        break;
                    }
                case "Spike Resistance III":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Spike Resistance\"]");
                        intRanks = 3;
                        break;
                    }
                case "Tough as Nails Physical I":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Tough as Nails (Physical)\"]");
                        intRanks = 1;
                        break;
                    }
                case "Tough as Nails Physical II":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Tough as Nails (Physical)\"]");
                        intRanks = 2;
                        break;
                    }
                case "Tough as Nails Physical III":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Tough as Nails (Physical)\"]");
                        intRanks = 3;
                        break;
                    }
                case "Tough as Nails Stun I":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Tough as Nails (Stun)\"]");
                        intRanks = 1;
                        break;
                    }
                case "Tough as Nails Stun II":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Tough as Nails (Stun)\"]");
                        intRanks = 2;
                        break;
                    }
                case "Tough as Nails Stun III":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Tough as Nails (Stun)\"]");
                        intRanks = 3;
                        break;
                    }
                case "Dimmer Bulb I":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Dimmer Bulb\"]");
                        intRanks = 1;
                        break;
                    }
                case "Dimmer Bulb II":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Dimmer Bulb\"]");
                        intRanks = 2;
                        break;
                    }
                case "Dimmer Bulb III":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Dimmer Bulb\"]");
                        intRanks = 3;
                        break;
                    }
                case "In Debt I":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"In Debt\"]");
                        intRanks = 1;
                        break;
                    }
                case "In Debt II":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"In Debt\"]");
                        intRanks = 2;
                        break;
                    }
                case "In Debt III":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"In Debt\"]");
                        intRanks = 3;
                        break;
                    }
                case "In Debt IV":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"In Debt\"]");
                        intRanks = 4;
                        break;
                    }
                case "In Debt V":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"In Debt\"]");
                        intRanks = 5;
                        break;
                    }
                case "In Debt VI":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"In Debt\"]");
                        intRanks = 6;
                        break;
                    }
                case "In Debt VII":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"In Debt\"]");
                        intRanks = 7;
                        break;
                    }
                case "In Debt VIII":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"In Debt\"]");
                        intRanks = 8;
                        break;
                    }
                case "In Debt IX":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"In Debt\"]");
                        intRanks = 9;
                        break;
                    }
                case "In Debt X":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"In Debt\"]");
                        intRanks = 10;
                        break;
                    }
                case "In Debt XI":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"In Debt\"]");
                        intRanks = 11;
                        break;
                    }
                case "In Debt XII":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"In Debt\"]");
                        intRanks = 12;
                        break;
                    }
                case "In Debt XIII":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"In Debt\"]");
                        intRanks = 13;
                        break;
                    }
                case "In Debt XIV":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"In Debt\"]");
                        intRanks = 14;
                        break;
                    }
                case "In Debt XV":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"In Debt\"]");
                        intRanks = 15;
                        break;
                    }
                case "Infirm I":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Infirm\"]");
                        intRanks = 1;
                        break;
                    }
                case "Infirm II":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Infirm\"]");
                        intRanks = 2;
                        break;
                    }
                case "Infirm III":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Infirm\"]");
                        intRanks = 3;
                        break;
                    }
                case "Infirm IV":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Infirm\"]");
                        intRanks = 4;
                        break;
                    }
                case "Infirm V":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Infirm\"]");
                        intRanks = 5;
                        break;
                    }
                case "Shiva Arms (1 Pair)":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Shiva Arms (Pair)\"]");
                        intRanks = 1;
                        break;
                    }
                case "Shiva Arms (2 Pair)":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Shiva Arms (Pair)\"]");
                        intRanks = 2;
                        break;
                    }
                case "Arcane Arrester I":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Arcane Arrester\"]");
                        intRanks = 1;
                        break;
                    }
                case "Arcane Arrester II":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Arcane Arrester\"]");
                        intRanks = 2;
                        break;
                    }
                case "Pilot Origins (Rating 1)":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Pilot Origins\"]");
                        intRanks = 1;
                        break;
                    }
                case "Pilot Origins (Rating 2)":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Pilot Origins\"]");
                        intRanks = 2;
                        break;
                    }
                case "Pilot Origins (Rating 3)":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Pilot Origins\"]");
                        intRanks = 3;
                        break;
                    }
                case "Social Appearance Anxiety (Rating 1)":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Social Appearance Anxiety\"]");
                        intRanks = 1;
                        break;
                    }
                case "Social Appearance Anxiety (Rating 2)":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Social Appearance Anxiety\"]");
                        intRanks = 2;
                        break;
                    }
                case "Social Appearance Anxiety (Rating 3)":
                    {
                        objXmlNewQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Social Appearance Anxiety\"]");
                        intRanks = 3;
                        break;
                    }
            }
            if (intRanks > 0)
            {
                for (int i = 0; i < intRanks; ++i)
                {
                    Quality objQuality = new Quality(this);
                    Guid guidOld;
                    if (Guid.TryParse(objOldXmlQuality["guid"].InnerText, out guidOld))
                        objQuality.setGUID(guidOld);
                    QualitySource objQualitySource = QualitySource.Selected;
                    if (objOldXmlQuality["qualitysource"] != null)
                        objQualitySource = Quality.ConvertToQualitySource(objOldXmlQuality["qualitysource"].InnerText);
                    objQuality.Create(objXmlNewQuality, this, objQualitySource, new TreeNode(), new List<Weapon>(), new List<TreeNode>(), objOldXmlQuality["extra"]?.InnerText);
                    int intOldBP;
                    if (objOldXmlQuality["bp"] != null && int.TryParse(objOldXmlQuality["bp"].InnerText, out intOldBP))
                        objQuality.BP = intOldBP / intRanks;

                    _lstQualities.Add(objQuality);
                }
                return true;
            }
            return false;
        }
#endregion

#region Temporary Properties : Dashboard
        // This region is for properties that are applicable to the Dashboard
        /// <summary>
        /// The Current Initiative roll result including base Initiative
        /// <note>Dashboard</note>
        /// </summary>
        public int InitRoll { get; set; }

        /// <summary>
        /// The Initiative Passes that the player has
        /// <note>Dashboard</note>
        /// </summary>
        public int InitPasses
        {
            get
            {
                if (_initPasses == int.MinValue)
                    _initPasses = Convert.ToInt32(InitiativeDice);
                return _initPasses;
            }
            set { _initPasses = value; }
        }
        private int _initPasses = int.MinValue;

        /// <summary>
        /// True iff the character is currently delaying an action
        /// <note>Dashboard</note>
        /// </summary>
        public bool Delayed { get; set; }

        /// <summary>
        /// The current name and initiative of the character
        /// </summary>
        public string DisplayInit
        {
            get { return Name + " : " + InitRoll.ToString(); }
        }

        /// <summary>
        /// The initial Initiative of the character
        /// <note>Dashboard</note>
        /// </summary>
        public int InitialInit { get; set; }
#endregion

#region Temporary Properties

        /// <summary>
        /// Takes a semicolon-separated list of book codes and returns a formatted string with displaynames.
        /// </summary>
        /// <param name="strInput"></param>
        public string TranslatedBookList(string strInput)
        {
            string strReturn = string.Empty;
            strInput = strInput.TrimEnd(';');
            string[] strArray = strInput.Split(';');
            // Load the Sourcebook information.
            XmlDocument objXmlDocument = XmlManager.Load("books.xml");

            foreach (string strBook in strArray)
            {
                XmlNode objXmlBook = objXmlDocument.SelectSingleNode("/chummer/books/book[code = \"" + strBook + "\"]");
                if (objXmlBook != null)
                {
                    if (objXmlBook["translate"] != null)
                        strReturn += objXmlBook["translate"]?.InnerText;
                    else
                        strReturn += objXmlBook["name"]?.InnerText;
                }
                else
                {
                    strReturn += "Unknown book! ";
                }
                strReturn += $" ({objXmlBook?["code"]?.InnerText ?? strBook})";
            }
            return strReturn;
        }

#endregion

        //Can't be at improvementmanager due reasons
        private Lazy<Stack<string>> _pushtext = new Lazy<Stack<string>>();
        private bool _ambidextrous;

        /// <summary>
        /// Push a value that will be used instad of dialog instead in next <selecttext />
        /// </summary>
        public Stack<string> Pushtext
        {
            get
            {
                return _pushtext.Value;
            }
        }

        /// <summary>
        /// Commlink Home Node. Returns null if home node is not a commlink.
        /// </summary>
        public Commlink HomeNodeCommlink
        {
            get
            {
                return _objHomeNodeCommlink;
            }
            set
            {
                _objHomeNodeCommlink = value;
            }
        }

        /// <summary>
        /// Vehicle Home Node. Returns null if home node is not a vehicle.
        /// </summary>
        public Vehicle HomeNodeVehicle
        {
            get
            {
                return _objHomeNodeVehicle;
            }
            set
            {
                _objHomeNodeVehicle = value;
            }
        }

        public SkillsSection SkillsSection { get; }


        public int RedlinerBonus
        {
            get { return _intRedlinerBonus; }
            set { _intRedlinerBonus = value; }
        }

        public Version LastSavedVersion
        {
            get { return _verSavedVersion; }
        }

        public bool Ambidextrous
        {
            get { return _ambidextrous; }
            internal set
            {
                _ambidextrous = value;
                AmbidextrousChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Is the character a mystic adept (MagicianEnabled && AdeptEnabled)? Used for databinding properties.
        /// </summary>
        public bool IsMysticAdept
        {
            get { return AdeptEnabled && MagicianEnabled; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged<T>(ref T old, T value, [CallerMemberName] string propertyName = null)
        {
            if ((old == null && value != null) || value == null || !old.Equals(value))
            {
                old = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        //I also think this prevents GC. But there is no good way to do it...
        internal event Action<List<Improvement>> SkillImprovementEvent;
        internal event Action<List<Improvement>> AttributeImprovementEvent;

        //List of events that might be able to affect skills. Made quick to prevent an infinite recursion somewhere related to adding an expense so it might be shaved down.
        public static readonly HashSet<Improvement.ImprovementType> SkillRelatedImprovements = new HashSet<Improvement.ImprovementType> {
            Improvement.ImprovementType.Uneducated,
            Improvement.ImprovementType.FreeKnowledgeSkills,
            Improvement.ImprovementType.Uncouth,
            Improvement.ImprovementType.Skillwire,
            Improvement.ImprovementType.SwapSkillAttribute,
            Improvement.ImprovementType.SkillsoftAccess,
            Improvement.ImprovementType.SchoolOfHardKnocks,
            Improvement.ImprovementType.CollegeEducation,
            Improvement.ImprovementType.Linguist,
            Improvement.ImprovementType.TechSchool,
            Improvement.ImprovementType.Hardwire,
            Improvement.ImprovementType.Skill,  //Improve pool of skill based on name
            Improvement.ImprovementType.SkillGroup,  //Group
            Improvement.ImprovementType.SkillCategory, //category
            Improvement.ImprovementType.SkillAttribute, //attribute
            Improvement.ImprovementType.SkillLevel,  //Karma points in skill
            Improvement.ImprovementType.SkillGroupLevel, //group
            Improvement.ImprovementType.SkillBase,  //base points in skill
            Improvement.ImprovementType.SkillGroupBase, //group
            Improvement.ImprovementType.SkillKnowledgeForced, //A skill gained from a knowsoft
            Improvement.ImprovementType.SpecialSkills,
            Improvement.ImprovementType.ReflexRecorderOptimization,
            Improvement.ImprovementType.BlockSkillDefault,
            Improvement.ImprovementType.SkillSpecialization,
            Improvement.ImprovementType.NativeLanguageLimit,
            Improvement.ImprovementType.SwapSkillSpecAttribute,
            Improvement.ImprovementType.FreeSpellsSkill,
            Improvement.ImprovementType.DisableSpecializationEffects,
        };

        //List of events that might be able to affect attributes. Changes to these types also invoke data bindings controlling skills, since their pools are controlled by attributes.
        public static readonly HashSet<Improvement.ImprovementType> AttribRelatedImprovements = new HashSet<Improvement.ImprovementType> {
            Improvement.ImprovementType.Attribute,
            Improvement.ImprovementType.Essence,
            Improvement.ImprovementType.Infirm,
            Improvement.ImprovementType.EssenceMax,
            Improvement.ImprovementType.Attributelevel,
            Improvement.ImprovementType.Seeker,
            Improvement.ImprovementType.ReplaceAttribute,
            Improvement.ImprovementType.EssencePenalty,
            Improvement.ImprovementType.EssencePenaltyT100,
            Improvement.ImprovementType.EssencePenaltyMAGOnlyT100,
            Improvement.ImprovementType.FreeSpellsATT,
            Improvement.ImprovementType.AddLimb,
        };
        //To get when things change in improvementmanager
        //Ugly, ugly done, but we cannot get events out of it today
        // FUTURE REFACTOR HERE
        [Obsolete("Refactor this method away once improvementmanager gets outbound events")]
        internal void ImprovementHook(List<Improvement> _lstTransaction)
        {
            if (_lstTransaction.Any(x => AttribRelatedImprovements.Contains(x.ImproveType)))
            {
                AttributeImprovementEvent?.Invoke(_lstTransaction);
            }
            else if (_lstTransaction.Any(x => SkillRelatedImprovements.Contains(x.ImproveType)))
            {
                SkillImprovementEvent?.Invoke(_lstTransaction);
            }
        }

	}
}
