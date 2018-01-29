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
using Chummer.Backend.Skills;
using System.Reflection;
using Chummer.Backend.Attributes;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

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
    public class Character : INotifyPropertyChanged, IDisposable, IHasMugshots, IHasName
    {
        private XmlNode oldSkillsBackup;
        private XmlNode oldSKillGroupBackup;

        private readonly CharacterOptions _objOptions;

        private string _strFileName = string.Empty;
        private DateTime _dateFileLastWriteTime = DateTime.MinValue;
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
        private readonly List<Image> _lstMugshots = new List<Image>();
        private int _intMainMugshotIndex = -1;
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
        private static readonly string[] s_LstLimbStrings = { "skull", "torso", "arm", "leg" };
        public static ReadOnlyCollection<string> LimbStrings { get { return Array.AsReadOnly(s_LstLimbStrings); } }

        // AI Home Node
        private IHasMatrixAttributes _objHomeNode = null;

        // Active Commlink
        private IHasMatrixAttributes _objActiveCommlink = null;

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
        private string _strMetatype = "Human";
        private string _strMetavariant = string.Empty;
        private string _strMetatypeCategory = "Metahuman";
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
        private string _strGameplayOption = "Standard";
        private string _strPriorityMetatype = "A,4";
        private string _strPriorityAttributes = "B,3";
        private string _strPrioritySpecial = "C,2";
        private string _strPrioritySkills = "D,1";
        private string _strPriorityResources = "E,0";
        private string _strPriorityTalent = string.Empty;
        private readonly List<string> _lstPrioritySkills = new List<string>();
        private decimal _decMaxNuyen = 0;
        private int _intMaxKarma = 0;
        private int _intContactMultiplier = 0;

        // Lists.
        private readonly List<string> _lstSources = new List<string>();
        private readonly List<string> _lstCustomDataDirectoryNames = new List<string>();
        private ObservableCollection<Improvement> _lstImprovements = new ObservableCollection<Improvement>();
        private List<MentorSpirit> _lstMentorSpirits = new List<MentorSpirit>();
        private ObservableCollection<Contact> _lstContacts = new ObservableCollection<Contact>();
        private ObservableCollection<Spirit> _lstSpirits = new ObservableCollection<Spirit>();
        private ObservableCollection<Spell> _lstSpells = new ObservableCollection<Spell>();
        private List<Focus> _lstFoci = new List<Focus>();
        private List<StackedFocus> _lstStackedFoci = new List<StackedFocus>();
        private BindingList<Power> _lstPowers = new BindingList<Power>();
        private ObservableCollection<ComplexForm> _lstComplexForms = new ObservableCollection<ComplexForm>();
        private ObservableCollection<AIProgram> _lstAIPrograms = new ObservableCollection<AIProgram>();
        private ObservableCollection<MartialArt> _lstMartialArts = new ObservableCollection<MartialArt>();
        #if LEGACY
        private List<MartialArtManeuver> _lstMartialArtManeuvers = new List<MartialArtManeuver>();
        #endif
        private List<LimitModifier> _lstLimitModifiers = new List<LimitModifier>();
        private List<Armor> _lstArmor = new List<Armor>();
        private BindingList<Cyberware> _lstCyberware = new BindingList<Cyberware>();
        private List<Weapon> _lstWeapons = new List<Weapon>();
        private ObservableCollection<Quality> _lstQualities = new ObservableCollection<Quality>();
        private readonly List<LifestyleQuality> _lstLifestyleQualities = new List<LifestyleQuality>();
        private ObservableCollection<Lifestyle> _lstLifestyles = new ObservableCollection<Lifestyle>();
        private List<Gear> _lstGear = new List<Gear>();
        private List<Vehicle> _lstVehicles = new List<Vehicle>();
        private List<Metamagic> _lstMetamagics = new List<Metamagic>();
        private List<Art> _lstArts = new List<Art>();
        private List<Enhancement> _lstEnhancements = new List<Enhancement>();
        private List<ExpenseLogEntry> _lstExpenseLog = new List<ExpenseLogEntry>();
        private ObservableCollection<CritterPower> _lstCritterPowers = new ObservableCollection<CritterPower>();
        private List<InitiationGrade> _lstInitiationGrades = new List<InitiationGrade>();
        private List<string> _lstOldQualities = new List<string>();
        private List<string> _lstGearLocations = new List<string>();
        private List<string> _lstArmorLocations = new List<string>();
        private List<string> _lstVehicleLocations = new List<string>();
        private List<string> _lstWeaponLocations = new List<string>();
        private List<string> _lstImprovementGroups = new List<string>();
        private BindingList<CalendarWeek> _lstCalendar = new BindingList<CalendarWeek>();
        //private List<LifeModule> _lstLifeModules = new List<LifeModule>();
        private List<string> _lstInternalIdsNeedingReapplyImprovements = new List<string>();

        // Character Version
        private string _strVersionCreated = Application.ProductVersion.Replace("0.0.", string.Empty);
        Version _verSavedVersion = new Version();
        // Events.
        public Action<object> AdeptTabEnabledChanged { get; set; }
        public Action<object> AmbidextrousChanged { get; set; }
        public Action<object> CritterTabEnabledChanged { get; set; }
        public Action<object> MAGEnabledChanged { get; set; }
        public Action<object> BornRichChanged { get; set; }
        public Action<object> CharacterNameChanged { get; set; }
        public Action<object> ExConChanged { get; set; }
        public Action<object> InitiationTabEnabledChanged { get; set; }
        public Action<object> MadeManChanged { get; set; }
        public Action<object> MagicianTabEnabledChanged { get; set; }
        public Action<object> RESEnabledChanged { get; set; }
        public Action<object> DEPEnabledChanged { get; set; }
        public Action<object> RestrictedGearChanged { get; set; }
        public Action<object> TechnomancerTabEnabledChanged { get; set; }
        public Action<object> AdvancedProgramsTabEnabledChanged { get; set; }
        public Action<object> CyberwareTabDisabledChanged { get; set; }

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
            _lstCyberware.ListChanged += (x, y) => { ResetCachedEssence(); };
        }

	    public AttributeSection AttributeSection { get; set; }

        public bool IsSaving { get; set; }

	    /// <summary>
        /// Save the Character to an XML file. Returns true if successful.
        /// </summary>
        public bool Save(string strFileName = "")
        {
            if (string.IsNullOrWhiteSpace(strFileName))
            {
                strFileName = _strFileName;
                if (string.IsNullOrWhiteSpace(strFileName))
                {
                    return false;
                }
            }
            IsSaving = true;
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
            SaveMugshots(objWriter);

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

            // <bannedwaregrades >
            objWriter.WriteStartElement("bannedwaregrades");
            foreach (string g in BannedWareGrades)
            {
                objWriter.WriteElementString("grade", g);
            }
            // </bannedwaregrades>
            objWriter.WriteEndElement();

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

            objWriter.WriteElementString("ambidextrous", _blnAmbidextrous.ToString());

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
            foreach (ComplexForm objComplexForm in _lstComplexForms)
            {
                objComplexForm.Save(objWriter);
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

            #if LEGACY
            // <martialartmaneuvers>
            objWriter.WriteStartElement("martialartmaneuvers");
            foreach (MartialArtManeuver objManeuver in _lstMartialArtManeuvers)
            {
                objManeuver.Save(objWriter);
            }
            // </martialartmaneuvers>
            objWriter.WriteEndElement();
            #endif

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
                objGear.Save(objWriter);
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

            // <improvements>
            objWriter.WriteStartElement("mentorspirits");
            foreach (MentorSpirit objMentor in _lstMentorSpirits)
            {
                objMentor.Save(objWriter);
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
            objWriter.WriteStartElement("gearlocations");
            foreach (string strLocation in _lstGearLocations)
            {
                objWriter.WriteElementString("gearlocation", strLocation);
            }
            // </locations>
            objWriter.WriteEndElement();

            // <armorbundles>
            objWriter.WriteStartElement("armorlocations");
            foreach (string strBundle in _lstArmorLocations)
            {
                objWriter.WriteElementString("armorlocation", strBundle);
            }
            // </armorbundles>
            objWriter.WriteEndElement();

            // <vehiclelocations>
            objWriter.WriteStartElement("vehiclelocations");
            foreach (string strLocation in _lstVehicleLocations)
            {
                objWriter.WriteElementString("vehiclelocation", strLocation);
            }
            // </vehiclelocations>
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
            objStream.Position = 0;

            bool blnErrorFree = true;
            // Validate that the character can save properly. If there's no error, save the file to the listed file location.
            try
            {
                XmlDocument objDoc = new XmlDocument();
                objDoc.Load(objStream);
                objDoc.Save(strFileName);
            }
            catch (XmlException)
            {
                MessageBox.Show(LanguageManager.GetString("Message_Save_Error_Warning", GlobalOptions.Language));
                blnErrorFree = false;
            }
            catch (System.UnauthorizedAccessException)
            {
                MessageBox.Show(LanguageManager.GetString("Message_Save_Error_Warning", GlobalOptions.Language));
                blnErrorFree = false;
            }
            objWriter.Close();

            IsSaving = false;
            _dateFileLastWriteTime = File.GetLastWriteTimeUtc(strFileName);
            return blnErrorFree;
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
                    MessageBox.Show(LanguageManager.GetString("Message_FailedLoad", GlobalOptions.Language).Replace("{0}", ex.Message), LanguageManager.GetString("MessageTitle_FailedLoad", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            Timekeeper.Finish("load_xml");
            _dateFileLastWriteTime = File.GetLastWriteTimeUtc(_strFileName);
            Timekeeper.Start("load_char_misc");
            XmlNode objXmlCharacter = objXmlDocument.SelectSingleNode("/character");

            objXmlCharacter.TryGetBoolFieldQuickly("ignorerules", ref _blnIgnoreRules);
            objXmlCharacter.TryGetBoolFieldQuickly("created", ref _blnCreated);

            ResetCharacter();

            // Get the game edition of the file if possible and make sure it's intended to be used with this version of the application.
            string strGameEdition = objXmlCharacter["gameedition"]?.InnerText ?? string.Empty;
            if (!string.IsNullOrEmpty(strGameEdition) && strGameEdition != "SR5")
            {
                MessageBox.Show(LanguageManager.GetString("Message_IncorrectGameVersion_SR4", GlobalOptions.Language),
                    LanguageManager.GetString("MessageTitle_IncorrectGameVersion", GlobalOptions.Language), MessageBoxButtons.YesNo,
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
#if !DEBUG
                Version verCurrentversion = Assembly.GetExecutingAssembly().GetName().Version;
                int intResult = verCurrentversion.CompareTo(_verSavedVersion);
                if (intResult == -1)
                {
                    string strMessage = LanguageManager.GetString("Message_OutdatedChummerSave", GlobalOptions.Language).Replace("{0}", _verSavedVersion.ToString()).Replace("{1}", verCurrentversion.ToString());
                    DialogResult result = MessageBox.Show(strMessage, LanguageManager.GetString("MessageTitle_IncorrectGameVersion", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Error);

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
            XmlNode xmlTempNode = objXmlCharacter["sources"];
            if (xmlTempNode != null)
            {
                string strMissingBooks = string.Empty;
                string strLoopString = string.Empty;
                //Does the list of enabled books contain the current item?
                foreach (XmlNode objXmlNode in xmlTempNode.ChildNodes)
                {
                    strLoopString = objXmlNode.InnerText;
                    if (strLoopString.Length > 0 && !_objOptions.Books.Contains(strLoopString))
                    {
                        strMissingBooks += strLoopString + ';';
                    }
                }
                if (!string.IsNullOrEmpty(strMissingBooks))
                {
                    string strMessage = LanguageManager.GetString("Message_MissingSourceBooks", GlobalOptions.Language).Replace("{0}", TranslatedBookList(strMissingBooks, GlobalOptions.Language));
                    if (MessageBox.Show(strMessage, LanguageManager.GetString("Message_MissingSourceBooks_Title", GlobalOptions.Language), MessageBoxButtons.YesNo) == DialogResult.No)
                    {
                        return false;
                    }
                }
            }

            // Get the sourcebooks that were used to create the character and throw up a warning if there's a mismatch.
            xmlTempNode = objXmlCharacter["customdatadirectorynames"];
            if (xmlTempNode != null)
            {
                string strMissingSourceNames = string.Empty;
                string strLoopString = string.Empty;
                //Does the list of enabled books contain the current item?
                foreach (XmlNode objXmlNode in xmlTempNode.ChildNodes)
                {
                    strLoopString = objXmlNode.InnerText;
                    if (strLoopString.Length > 0 && !_objOptions.CustomDataDirectoryNames.Contains(strLoopString))
                    {
                        strMissingSourceNames += strLoopString + ";\n";
                    }
                }
                if (!string.IsNullOrEmpty(strMissingSourceNames))
                {
                    string strMessage = LanguageManager.GetString("Message_MissingCustomDataDirectories", GlobalOptions.Language).Replace("{0}", strMissingSourceNames);
                    if (MessageBox.Show(strMessage, LanguageManager.GetString("Message_MissingCustomDataDirectories_Title", GlobalOptions.Language), MessageBoxButtons.YesNo) == DialogResult.No)
                    {
                        return false;
                    }
                }
            }

            xmlTempNode = objXmlCharacter["essenceatspecialstart"];
            if (xmlTempNode != null)
            {
                _decEssenceAtSpecialStart = Convert.ToDecimal(xmlTempNode.InnerText, GlobalOptions.InvariantCultureInfo);
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

            _strRunAlt = objXmlCharacter.SelectSingleNode("run/@alt")?.InnerText ?? string.Empty;
            _strWalkAlt = objXmlCharacter.SelectSingleNode("walk/@alt")?.InnerText ?? string.Empty;
            _strSprintAlt = objXmlCharacter.SelectSingleNode("sprint/@alt")?.InnerText ?? string.Empty;

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
            LoadMugshots(objXmlCharacter);
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

            if (!objXmlCharacter.TryGetStringFieldQuickly("gameplayoption", ref _strGameplayOption))
            {
                if (objXmlCharacter.TryGetInt32FieldQuickly("buildkarma", ref _intBuildKarma) && _intBuildKarma == 35)
                    _strGameplayOption = "Prime Runner";
                else
                    _strGameplayOption = "Standard";
            }

            objXmlCharacter.TryGetField("buildmethod", Enum.TryParse, out _objBuildMethod);
            if (!objXmlCharacter.TryGetDecFieldQuickly("maxnuyen", ref _decMaxNuyen) || _decMaxNuyen == 0)
                _decMaxNuyen = 25;
            objXmlCharacter.TryGetInt32FieldQuickly("contactmultiplier", ref _intContactMultiplier);
            objXmlCharacter.TryGetInt32FieldQuickly("sumtoten", ref _intSumtoTen);
            objXmlCharacter.TryGetInt32FieldQuickly("bp", ref _intBuildPoints);
            objXmlCharacter.TryGetInt32FieldQuickly("buildkarma", ref _intBuildKarma);
            if (!objXmlCharacter.TryGetInt32FieldQuickly("maxkarma", ref _intMaxKarma) || _intMaxKarma == 0)
                _intMaxKarma = _intBuildKarma;

            //Maximum number of Karma that can be spent/gained on Qualities.
            objXmlCharacter.TryGetInt32FieldQuickly("gameplayoptionqualitylimit", ref _intGameplayOptionQualityLimit);

            objXmlCharacter.TryGetDecFieldQuickly("nuyenmaxbp", ref _decNuyenMaximumBP);
            objXmlCharacter.TryGetInt32FieldQuickly("maxavail", ref _intMaxAvail);

            XmlDocument objXmlDocumentGameplayOptions = XmlManager.Load("gameplayoptions.xml");
            XmlNode xmlGameplayOption = objXmlDocumentGameplayOptions.SelectSingleNode("/chummer/gameplayoptions/gameplayoption[name = \"" + GameplayOption + "\"]");
            if (xmlGameplayOption == null)
            {
                string strMessage = LanguageManager.GetString("Message_MissingGameplayOption", GlobalOptions.Language).Replace("{0}", GameplayOption);
                if (MessageBox.Show(strMessage, LanguageManager.GetString("Message_MissingGameplayOption_Title", GlobalOptions.Language), MessageBoxButtons.OKCancel, MessageBoxIcon.Error) == DialogResult.OK)
                {
                    frmSelectBuildMethod frmPickBP = new frmSelectBuildMethod(this, true);
                    frmPickBP.ShowDialog();

                    if (frmPickBP.DialogResult != DialogResult.OK)
                        return false;
                }
                else
                    return false;
            }

            objXmlCharacter.TryGetStringFieldQuickly("prioritymetatype", ref _strPriorityMetatype);
            objXmlCharacter.TryGetStringFieldQuickly("priorityattributes", ref _strPriorityAttributes);
            objXmlCharacter.TryGetStringFieldQuickly("priorityspecial", ref _strPrioritySpecial);
            objXmlCharacter.TryGetStringFieldQuickly("priorityskills", ref _strPrioritySkills);
            objXmlCharacter.TryGetStringFieldQuickly("priorityresources", ref _strPriorityResources);
            objXmlCharacter.TryGetStringFieldQuickly("prioritytalent", ref _strPriorityTalent);
            _lstPrioritySkills.Clear();
            XmlNodeList objXmlPrioritySkillsList = objXmlCharacter.SelectNodes("priorityskills/priorityskill");
            if (objXmlPrioritySkillsList != null)
            {
                foreach (XmlNode objXmlSkillName in objXmlPrioritySkillsList)
                {
                    _lstPrioritySkills.Add(objXmlSkillName.InnerText);
                }
            }
            BannedWareGrades.Clear();
            xmlTempNode = objXmlCharacter["bannedwaregrades"];
            if (xmlTempNode != null)
            {
                if (xmlTempNode.HasChildNodes)
                    foreach (XmlNode xmlNode in xmlTempNode.SelectNodes("grade"))
                        BannedWareGrades.Add(xmlNode.InnerText);
            }
            else
            {
                foreach (XmlNode xmlNode in xmlGameplayOption.SelectNodes("bannedwaregrades/grade"))
                    BannedWareGrades.Add(xmlNode.InnerText);
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
            objXmlCharacter.TryGetDecFieldQuickly("nuyen", ref _decNuyen);
            objXmlCharacter.TryGetDecFieldQuickly("startingnuyen", ref _decStartingNuyen);
            objXmlCharacter.TryGetDecFieldQuickly("nuyenbp", ref _decNuyenBP);

            objXmlCharacter.TryGetInt32FieldQuickly("adeptwaydiscount", ref _intAdeptWayDiscount);
            objXmlCharacter.TryGetBoolFieldQuickly("adept", ref _blnAdeptEnabled);
            objXmlCharacter.TryGetBoolFieldQuickly("magician", ref _blnMagicianEnabled);
            objXmlCharacter.TryGetBoolFieldQuickly("technomancer", ref _blnTechnomancerEnabled);
            objXmlCharacter.TryGetBoolFieldQuickly("ai", ref _blnAdvancedProgramsEnabled);
            objXmlCharacter.TryGetBoolFieldQuickly("cyberwaredisabled", ref _blnCyberwareDisabled);
            objXmlCharacter.TryGetBoolFieldQuickly("initiationoverride", ref _blnInitiationEnabled);
            objXmlCharacter.TryGetBoolFieldQuickly("critter", ref _blnCritterEnabled);

            objXmlCharacter.TryGetBoolFieldQuickly("friendsinhighplaces", ref _blnFriendsInHighPlaces);
            objXmlCharacter.TryGetDecFieldQuickly("prototypetranshuman", ref _decPrototypeTranshuman);
            objXmlCharacter.TryGetBoolFieldQuickly("blackmarketdiscount", ref _blnBlackMarketDiscount);
            objXmlCharacter.TryGetBoolFieldQuickly("excon", ref _blnExCon);
            objXmlCharacter.TryGetInt32FieldQuickly("trustfund", ref _intTrustFund);
            objXmlCharacter.TryGetBoolFieldQuickly("restrictedgear", ref _blnRestrictedGear);
            objXmlCharacter.TryGetBoolFieldQuickly("overclocker", ref _blnOverclocker);
            objXmlCharacter.TryGetBoolFieldQuickly("mademan", ref _blnMadeMan);
            objXmlCharacter.TryGetBoolFieldQuickly("fame", ref _blnFame);
            objXmlCharacter.TryGetBoolFieldQuickly("ambidextrous", ref _blnAmbidextrous);
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
            Timekeeper.Start("load_char_mentorspirit");
            // Improvements.
            XmlNodeList objXmlNodeList = objXmlCharacter.SelectNodes("mentorspirits/mentorspirit");
            foreach (XmlNode objXmlMentor in objXmlNodeList)
            {
                MentorSpirit objMentor = new MentorSpirit(this);
                objMentor.Load(objXmlMentor);
                _lstMentorSpirits.Add(objMentor);
            }
            Timekeeper.Finish("load_char_mentorspirit");
            _lstInternalIdsNeedingReapplyImprovements.Clear();
            Timekeeper.Start("load_char_imp");
            // Improvements.
            objXmlNodeList = objXmlCharacter.SelectNodes("improvements/improvement");
            string strCharacterInnerXml = objXmlCharacter.InnerXml;
            foreach (XmlNode objXmlImprovement in objXmlNodeList)
            {
                string strLoopSourceName = objXmlImprovement["sourcename"]?.InnerText;
                
                if (string.IsNullOrEmpty(strLoopSourceName) || !strLoopSourceName.IsGuid() || (objXmlImprovement["custom"]?.InnerText == System.Boolean.TrueString) ||
                    // Hacky way to make sure we aren't loading in any orphaned improvements: SourceName ID will pop up minimum twice in the save if the improvement's source is actually present: once in the improvement and once in the parent that added it.
                    (strCharacterInnerXml.IndexOf(strLoopSourceName) != strCharacterInnerXml.LastIndexOf(strLoopSourceName)))
                {
                    Improvement objImprovement = new Improvement(this);
                    try
                    {
                        objImprovement.Load(objXmlImprovement);
                        _lstImprovements.Add(objImprovement);
                    }
                    catch (ArgumentException)
                    {
                        _lstInternalIdsNeedingReapplyImprovements.Add(strLoopSourceName);
                    }
                }
            }
            Timekeeper.Finish("load_char_imp");
            Timekeeper.Start("load_char_quality");
            // Qualities
            Quality objLivingPersonaQuality = null;
            objXmlNodeList = objXmlCharacter.SelectNodes("qualities/quality");
            bool blnHasOldQualities = false;
            XmlDocument xmlQualitiesDocument = XmlManager.Load("qualities.xml");
            foreach (XmlNode objXmlQuality in objXmlNodeList)
            {
                if (objXmlQuality["name"] != null)
                {
                    if (!CorrectedUnleveledQuality(objXmlQuality, xmlQualitiesDocument))
                    {
                        Quality objQuality = new Quality(this);
                        objQuality.Load(objXmlQuality);
                        // Corrects an issue arising from older versions of CorrectedUnleveledQuality()
                        if (_lstQualities.Any(x => x.InternalId == objQuality.InternalId))
                            objQuality.SetGUID(Guid.NewGuid());
                        _lstQualities.Add(objQuality);
                        if (objQuality.GetNode()?.SelectSingleNode("bonus/addgear/name")?.InnerText == "Living Persona")
                            objLivingPersonaQuality = objQuality;
                        // Legacy shim
                        if (LastSavedVersion <= new Version("5.195.1") && (objQuality.Name == "The Artisan's Way" ||
                            objQuality.Name == "The Artist's Way" ||
                            objQuality.Name == "The Athlete's Way" ||
                            objQuality.Name == "The Burnout's Way" ||
                            objQuality.Name == "The Invisible Way" ||
                            objQuality.Name == "The Magician's Way" ||
                            objQuality.Name == "The Speaker's Way" ||
                            objQuality.Name == "The Warrior's Way") && objQuality.Bonus?.HasChildNodes == false)
                        {
                            ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.Quality, objQuality.InternalId);
                            XmlNode objNode = objQuality.GetNode();
                            if (objNode != null)
                            {
                                objQuality.Bonus = objNode["bonus"];
                                if (objQuality.Bonus != null)
                                {
                                    ImprovementManager.ForcedValue = objQuality.Extra;
                                    ImprovementManager.CreateImprovements(this, Improvement.ImprovementSource.Quality, objQuality.InternalId, objQuality.Bonus, false, 1, objQuality.DisplayNameShort(GlobalOptions.Language));
                                    if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                                    {
                                        objQuality.Extra = ImprovementManager.SelectedValue;
                                    }
                                }
                                objQuality.FirstLevelBonus = objNode["firstlevelbonus"];
                                if (objQuality.FirstLevelBonus?.HasChildNodes == true)
                                {
                                    bool blnDoFirstLevel = true;
                                    foreach (Quality objCheckQuality in Qualities)
                                    {
                                        if (objCheckQuality != objQuality && objCheckQuality.QualityId == objQuality.QualityId && objCheckQuality.Extra == objQuality.Extra && objCheckQuality.SourceName == objQuality.SourceName)
                                        {
                                            blnDoFirstLevel = false;
                                            break;
                                        }
                                    }
                                    if (blnDoFirstLevel)
                                    {
                                        ImprovementManager.ForcedValue = objQuality.Extra;
                                        ImprovementManager.CreateImprovements(this, Improvement.ImprovementSource.Quality, objQuality.InternalId, objQuality.FirstLevelBonus, false, 1, objQuality.DisplayNameShort(GlobalOptions.Language));
                                        if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                                        {
                                            objQuality.Extra = ImprovementManager.SelectedValue;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // Failed to re-apply the improvements immediately, so let's just add it for processing when the character is opened
                                _lstInternalIdsNeedingReapplyImprovements.Add(objQuality.InternalId);
                            }
                        }
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
			AttributeSection.Load(objXmlCharacter);
			Timekeeper.Start("load_char_misc2");

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

            oldSkillsBackup = objXmlCharacter.SelectSingleNode("skills")?.Clone();
            oldSKillGroupBackup = objXmlCharacter.SelectSingleNode("skillgroups")?.Clone();

            XmlNode SkillNode = objXmlCharacter.SelectSingleNode("newskills");
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
            objXmlNodeList = objXmlCharacter.SelectNodes("contacts/contact");
            foreach (XmlNode objXmlContact in objXmlNodeList)
            {
                Contact objContact = new Contact(this);
                objContact.Load(objXmlContact);
                _lstContacts.Add(objContact);
            }

            Timekeeper.Finish("load_char_contacts");
            Timekeeper.Start("load_char_armor");
            // Armor.
            objXmlNodeList = objXmlCharacter.SelectNodes("armors/armor");
            foreach (XmlNode objXmlArmor in objXmlNodeList)
            {
                Armor objArmor = new Armor(this);
                objArmor.Load(objXmlArmor);
                _lstArmor.Add(objArmor);
            }
            Timekeeper.Finish("load_char_armor");
            Timekeeper.Start("load_char_weapons");

            // Weapons.
            objXmlNodeList = objXmlCharacter.SelectNodes("weapons/weapon");
            foreach (XmlNode objXmlWeapon in objXmlNodeList)
            {
                Weapon objWeapon = new Weapon(this);
                objWeapon.Load(objXmlWeapon);
                _lstWeapons.Add(objWeapon);
            }

            Timekeeper.Finish("load_char_weapons");
            Timekeeper.Start("load_char_ware");

            // Dictionary for instantly re-applying outdated improvements for 'ware with pair bonuses in legacy shim
            Dictionary<Cyberware, int> dicPairableCyberwares = new Dictionary<Cyberware, int>();
            // Cyberware/Bioware.
            objXmlNodeList = objXmlCharacter.SelectNodes("cyberwares/cyberware");
            foreach (XmlNode objXmlCyberware in objXmlNodeList)
            {
                Cyberware objCyberware = new Cyberware(this);
                objCyberware.Load(objXmlCyberware);
                _lstCyberware.Add(objCyberware);
                // Legacy shim
                if (objCyberware.Name == "Myostatin Inhibitor")
                {
                    if (LastSavedVersion <= new Version("5.195.1") && !Improvements.Any(x => x.SourceName == objCyberware.InternalId && x.ImproveType == Improvement.ImprovementType.AttributeKarmaCost))
                    {
                        XmlNode objNode = objCyberware.GetNode();
                        if (objNode != null)
                        {
                            objCyberware.Bonus = objNode["bonus"];
                            objCyberware.WirelessBonus = objNode["wirelessbonus"];
                            objCyberware.PairBonus = objNode["pairbonus"];
                            if (objCyberware.IsModularCurrentlyEquipped)
                            {
                                if (!string.IsNullOrEmpty(objCyberware.Forced) && objCyberware.Forced != "Right" && objCyberware.Forced != "Left")
                                    ImprovementManager.ForcedValue = objCyberware.Forced;
                                if (objCyberware.Bonus != null)
                                {
                                    ImprovementManager.CreateImprovements(this, objCyberware.SourceType, objCyberware.InternalId, objCyberware.Bonus, false, objCyberware.Rating, objCyberware.DisplayNameShort(GlobalOptions.Language));
                                    if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                                        objCyberware.Extra = ImprovementManager.SelectedValue;
                                }
                                if (objCyberware.WirelessOn && objCyberware.WirelessBonus != null)
                                {
                                    ImprovementManager.CreateImprovements(this, objCyberware.SourceType, objCyberware.InternalId, objCyberware.WirelessBonus, false, objCyberware.Rating, objCyberware.DisplayNameShort(GlobalOptions.Language));
                                    if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue) && string.IsNullOrEmpty(objCyberware.Extra))
                                        objCyberware.Extra = ImprovementManager.SelectedValue;
                                }
                                if (objCyberware.PairBonus != null)
                                {
                                    Cyberware objMatchingCyberware = dicPairableCyberwares.Keys.FirstOrDefault(x => x.Name == objCyberware.Name && x.Extra == objCyberware.Extra);
                                    if (objMatchingCyberware != null)
                                        dicPairableCyberwares[objMatchingCyberware] = dicPairableCyberwares[objMatchingCyberware] + 1;
                                    else
                                        dicPairableCyberwares.Add(objCyberware, 1);
                                }
                            }
                        }
                        else
                        {
                            _lstInternalIdsNeedingReapplyImprovements.Add(objCyberware.InternalId);
                        }
                    }
                }
            }
            // Separate Pass for PairBonuses
            foreach (KeyValuePair<Cyberware, int> objItem in dicPairableCyberwares)
            {
                Cyberware objCyberware = objItem.Key;
                int intCyberwaresCount = objItem.Value;
                if (!string.IsNullOrEmpty(objCyberware.Location))
                {
                    intCyberwaresCount = Math.Min(intCyberwaresCount, Cyberware.DeepCount(x => x.Children, x => objCyberware.IncludePair.Contains(x.Name) && x.Extra == objCyberware.Extra && x.Location != objCyberware.Location && x.IsModularCurrentlyEquipped));
                }
                if (intCyberwaresCount > 0)
                {
                    foreach (Cyberware objLoopCyberware in Cyberware.DeepWhere(x => x.Children, x => objCyberware.IncludePair.Contains(x.Name) && x.Extra == objCyberware.Extra && x.IsModularCurrentlyEquipped))
                    {
                        if (intCyberwaresCount % 2 == 0)
                        {
                            if (!string.IsNullOrEmpty(objCyberware.Forced) && objCyberware.Forced != "Right" && objCyberware.Forced != "Left")
                                ImprovementManager.ForcedValue = objCyberware.Forced;
                            ImprovementManager.CreateImprovements(this, objLoopCyberware.SourceType, objLoopCyberware.InternalId, objLoopCyberware.PairBonus, false, objLoopCyberware.Rating, objLoopCyberware.DisplayNameShort(GlobalOptions.Language));
                            if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue) && string.IsNullOrEmpty(objCyberware.Extra))
                                objCyberware.Extra = ImprovementManager.SelectedValue;
                        }
                        intCyberwaresCount -= 1;
                        if (intCyberwaresCount <= 0)
                            break;
                    }
                }
            }

            Timekeeper.Finish("load_char_ware");
            Timekeeper.Start("load_char_spells");

            // Spells.
            objXmlNodeList = objXmlCharacter.SelectNodes("spells/spell");
            foreach (XmlNode objXmlSpell in objXmlNodeList)
            {
                Spell objSpell = new Spell(this);
                objSpell.Load(objXmlSpell);
                _lstSpells.Add(objSpell);
            }

            Timekeeper.Finish("load_char_spells");
            Timekeeper.Start("load_char_foci");

            // Foci.
            objXmlNodeList = objXmlCharacter.SelectNodes("foci/focus");
            foreach (XmlNode objXmlFocus in objXmlNodeList)
            {
                Focus objFocus = new Focus(this);
                objFocus.Load(objXmlFocus);
                _lstFoci.Add(objFocus);
            }

            Timekeeper.Finish("load_char_foci");
            Timekeeper.Start("load_char_sfoci");

            // Stacked Foci.
            objXmlNodeList = objXmlCharacter.SelectNodes("stackedfoci/stackedfocus");
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
            objXmlNodeList = objXmlCharacter.SelectNodes("powers/power");
            // Sort the Powers in alphabetical order.
            foreach (XmlNode xmlPower in objXmlNodeList)
            {
                string strGuid = xmlPower["guid"]?.InnerText;
                if (!string.IsNullOrEmpty(strGuid))
                    lstPowerOrder.Add(new ListItem(strGuid, (xmlPower["name"]?.InnerText ?? string.Empty) + (xmlPower["extra"]?.InnerText ?? string.Empty)));
                else
                {
                    Power objPower = new Power(this);
                    objPower.Load(xmlPower);
                    _lstPowers.Add(objPower);
                }
            }
            lstPowerOrder.Sort(CompareListItems.CompareNames);

            foreach (ListItem objItem in lstPowerOrder)
            {
                XmlNode objNode = objXmlCharacter.SelectSingleNode("powers/power[guid = \"" + objItem.Value.ToString() + "\"]");
                if (objNode != null)
                {
                    Power objPower = new Power(this);
                    objPower.Load(objNode);
                    _lstPowers.Add(objPower);
                }
            }

            Timekeeper.Finish("load_char_powers");
            Timekeeper.Start("load_char_spirits");

            // Spirits/Sprites.
            objXmlNodeList = objXmlCharacter.SelectNodes("spirits/spirit");
            foreach (XmlNode objXmlSpirit in objXmlNodeList)
            {
                Spirit objSpirit = new Spirit(this);
                objSpirit.Load(objXmlSpirit);
                _lstSpirits.Add(objSpirit);
            }

            Timekeeper.Finish("load_char_spirits");
            Timekeeper.Start("load_char_complex");

            // Compex Forms/Technomancer Programs.
            objXmlNodeList = objXmlCharacter.SelectNodes("complexforms/complexform");
            foreach (XmlNode objXmlComplexForm in objXmlNodeList)
            {
                ComplexForm objComplexForm = new ComplexForm(this);
                objComplexForm.Load(objXmlComplexForm);
                _lstComplexForms.Add(objComplexForm);
            }

            Timekeeper.Finish("load_char_complex");
            Timekeeper.Start("load_char_aiprogram");

            // Compex Forms/Technomancer Programs.
            objXmlNodeList = objXmlCharacter.SelectNodes("aiprograms/aiprogram");
            foreach (XmlNode objXmlProgram in objXmlNodeList)
            {
                AIProgram objProgram = new AIProgram(this);
                objProgram.Load(objXmlProgram);
                _lstAIPrograms.Add(objProgram);
            }

            Timekeeper.Finish("load_char_aiprogram");
            Timekeeper.Start("load_char_marts");

            // Martial Arts.
            objXmlNodeList = objXmlCharacter.SelectNodes("martialarts/martialart");
            foreach (XmlNode objXmlArt in objXmlNodeList)
            {
                MartialArt objMartialArt = new MartialArt(this);
                objMartialArt.Load(objXmlArt);
                _lstMartialArts.Add(objMartialArt);
            }

            Timekeeper.Finish("load_char_marts");
            #if LEGACY
            Timekeeper.Start("load_char_mam");

            // Martial Art Maneuvers.
            objXmlNodeList = objXmlCharacter.SelectNodes("martialartmaneuvers/martialartmaneuver");
            foreach (XmlNode objXmlManeuver in objXmlNodeList)
            {
                MartialArtManeuver objManeuver = new MartialArtManeuver(this);
                objManeuver.Load(objXmlManeuver);
                _lstMartialArtManeuvers.Add(objManeuver);
            }

            Timekeeper.Finish("load_char_mam");
            #endif
            Timekeeper.Start("load_char_mod");

            // Limit Modifiers.
            objXmlNodeList = objXmlCharacter.SelectNodes("limitmodifiers/limitmodifier");
            foreach (XmlNode objXmlLimit in objXmlNodeList)
            {
                LimitModifier obLimitModifier = new LimitModifier(this);
                obLimitModifier.Load(objXmlLimit);
                _lstLimitModifiers.Add(obLimitModifier);
            }

            Timekeeper.Finish("load_char_mod");
            Timekeeper.Start("load_char_lifestyle");

            // Lifestyles.
            objXmlNodeList = objXmlCharacter.SelectNodes("lifestyles/lifestyle");
            foreach (XmlNode objXmlLifestyle in objXmlNodeList)
            {
                Lifestyle objLifestyle = new Lifestyle(this);
                objLifestyle.Load(objXmlLifestyle);
                _lstLifestyles.Add(objLifestyle);
            }

            Timekeeper.Finish("load_char_lifestyle");
            Timekeeper.Start("load_char_gear");

            // <gears>
            objXmlNodeList = objXmlCharacter.SelectNodes("gears/gear");
            foreach (XmlNode objXmlGear in objXmlNodeList)
            {
                Gear objGear = new Gear(this);
                objGear.Load(objXmlGear);
                _lstGear.Add(objGear);
            }
            // If we have a technomancer quality but no Living Persona commlink, we re-apply its improvements immediately
            if (objLivingPersonaQuality != null && LastSavedVersion <= new Version("5.195.1"))
            {
                ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.Quality, objLivingPersonaQuality.InternalId);

                XmlNode objNode = objLivingPersonaQuality.GetNode();
                if (objNode != null)
                {
                    objLivingPersonaQuality.Bonus = objNode["bonus"];
                    if (objLivingPersonaQuality.Bonus != null)
                    {
                        ImprovementManager.ForcedValue = objLivingPersonaQuality.Extra;
                        ImprovementManager.CreateImprovements(this, Improvement.ImprovementSource.Quality, objLivingPersonaQuality.InternalId, objLivingPersonaQuality.Bonus, false, 1, objLivingPersonaQuality.DisplayNameShort(GlobalOptions.Language));
                        if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                        {
                            objLivingPersonaQuality.Extra = ImprovementManager.SelectedValue;
                        }
                    }
                    objLivingPersonaQuality.FirstLevelBonus = objNode["firstlevelbonus"];
                    if (objLivingPersonaQuality.FirstLevelBonus?.HasChildNodes == true)
                    {
                        bool blnDoFirstLevel = true;
                        foreach (Quality objCheckQuality in Qualities)
                        {
                            if (objCheckQuality != objLivingPersonaQuality && objCheckQuality.QualityId == objLivingPersonaQuality.QualityId && objCheckQuality.Extra == objLivingPersonaQuality.Extra && objCheckQuality.SourceName == objLivingPersonaQuality.SourceName)
                            {
                                blnDoFirstLevel = false;
                                break;
                            }
                        }
                        if (blnDoFirstLevel)
                        {
                            ImprovementManager.ForcedValue = objLivingPersonaQuality.Extra;
                            ImprovementManager.CreateImprovements(this, Improvement.ImprovementSource.Quality, objLivingPersonaQuality.InternalId, objLivingPersonaQuality.FirstLevelBonus, false, 1, objLivingPersonaQuality.DisplayNameShort(GlobalOptions.Language));
                            if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                            {
                                objLivingPersonaQuality.Extra = ImprovementManager.SelectedValue;
                            }
                        }
                    }
                }
                else
                {
                    // Failed to re-apply the improvements immediately, so let's just add it for processing when the character is opened
                    _lstInternalIdsNeedingReapplyImprovements.Add(objLivingPersonaQuality.InternalId);
                }
            }

            Timekeeper.Finish("load_char_gear");
            Timekeeper.Start("load_char_car");

            // Vehicles.
            objXmlNodeList = objXmlCharacter.SelectNodes("vehicles/vehicle");
            foreach (XmlNode objXmlVehicle in objXmlNodeList)
            {
                Vehicle objVehicle = new Vehicle(this);
                objVehicle.Load(objXmlVehicle);
                _lstVehicles.Add(objVehicle);
            }

            Timekeeper.Finish("load_char_car");
            Timekeeper.Start("load_char_mmagic");
            // Metamagics/Echoes.
            objXmlNodeList = objXmlCharacter.SelectNodes("metamagics/metamagic");
            foreach (XmlNode objXmlMetamagic in objXmlNodeList)
            {
                Metamagic objMetamagic = new Metamagic(this);
                objMetamagic.Load(objXmlMetamagic);
                _lstMetamagics.Add(objMetamagic);
            }

            Timekeeper.Finish("load_char_mmagic");
            Timekeeper.Start("load_char_arts");

            // Arts
            objXmlNodeList = objXmlCharacter.SelectNodes("arts/art");
            foreach (XmlNode objXmlArt in objXmlNodeList)
            {
                Art objArt = new Art(this);
                objArt.Load(objXmlArt);
                _lstArts.Add(objArt);
            }

            Timekeeper.Finish("load_char_arts");
            Timekeeper.Start("load_char_ench");

            // Enhancements
            objXmlNodeList = objXmlCharacter.SelectNodes("enhancements/enhancement");
            foreach (XmlNode objXmlEnhancement in objXmlNodeList)
            {
                Enhancement objEnhancement = new Enhancement(this);
                objEnhancement.Load(objXmlEnhancement);
                _lstEnhancements.Add(objEnhancement);
            }

            Timekeeper.Finish("load_char_ench");
            Timekeeper.Start("load_char_cpow");

            // Critter Powers.
            objXmlNodeList = objXmlCharacter.SelectNodes("critterpowers/critterpower");
            foreach (XmlNode objXmlPower in objXmlNodeList)
            {
                CritterPower objPower = new CritterPower(this);
                objPower.Load(objXmlPower);
                _lstCritterPowers.Add(objPower);
            }

            Timekeeper.Finish("load_char_cpow");
            Timekeeper.Start("load_char_init");

            // Initiation Grades.
            objXmlNodeList = objXmlCharacter.SelectNodes("initiationgrades/initiationgrade");
            foreach (XmlNode objXmlGrade in objXmlNodeList)
            {
                InitiationGrade objGrade = new InitiationGrade(this);
                objGrade.Load(objXmlGrade);
                _lstInitiationGrades.Add(objGrade);
            }

            Timekeeper.Finish("load_char_init");
            Timekeeper.Start("load_char_elog");

            // Expense Log Entries.
            XmlNodeList objXmlExpenseList = objXmlCharacter.SelectNodes("expenses/expense");
            foreach (XmlNode objXmlExpense in objXmlExpenseList)
            {
                ExpenseLogEntry objExpenseLogEntry = new ExpenseLogEntry(this);
                objExpenseLogEntry.Load(objXmlExpense);
                _lstExpenseLog.Add(objExpenseLogEntry);
            }

            Timekeeper.Finish("load_char_elog");
            Timekeeper.Start("load_char_loc");

            // Locations.
            XmlNodeList objXmlLocationList = objXmlCharacter.SelectNodes("gearlocations/gearlocation");
            foreach (XmlNode objXmlLocation in objXmlLocationList)
            {
                _lstGearLocations.Add(objXmlLocation.InnerText);
            }
            objXmlLocationList = objXmlCharacter.SelectNodes("locations/location");
            foreach (XmlNode objXmlLocation in objXmlLocationList)
            {
                _lstGearLocations.Add(objXmlLocation.InnerText);
            }

            Timekeeper.Finish("load_char_loc");
            Timekeeper.Start("load_char_abundle");

            // Armor Bundles.
            XmlNodeList objXmlBundleList = objXmlCharacter.SelectNodes("armorbundles/armorbundle");
            foreach (XmlNode objXmlBundle in objXmlBundleList)
            {
                _lstArmorLocations.Add(objXmlBundle.InnerText);
            }

            objXmlBundleList = objXmlCharacter.SelectNodes("armorlocations/armorlocation");
            foreach (XmlNode objXmlBundle in objXmlBundleList)
            {
                _lstArmorLocations.Add(objXmlBundle.InnerText);
            }

            Timekeeper.Finish("load_char_abundle");
            Timekeeper.Start("load_char_vloc");

            // Vehicle Locations.
            XmlNodeList objXmlVehicleLocationList = objXmlCharacter.SelectNodes("vehiclelocations/vehiclelocation");
            foreach (XmlNode objXmlLocation in objXmlVehicleLocationList)
            {
                _lstVehicleLocations.Add(objXmlLocation.InnerText);
            }

            Timekeeper.Finish("load_char_vloc");
            Timekeeper.Start("load_char_wloc");

            // Weapon Locations.
            XmlNodeList objXmlWeaponLocationList = objXmlCharacter.SelectNodes("weaponlocations/weaponlocation");
            foreach (XmlNode objXmlLocation in objXmlWeaponLocationList)
            {
                _lstWeaponLocations.Add(objXmlLocation.InnerText);
            }

            Timekeeper.Finish("load_char_wloc");
            Timekeeper.Start("load_char_igroup");

            // Improvement Groups.
            XmlNodeList objXmlGroupList = objXmlCharacter.SelectNodes("improvementgroups/improvementgroup");
            foreach (XmlNode objXmlGroup in objXmlGroupList)
            {
                _lstImprovementGroups.Add(objXmlGroup.InnerText);
            }

            Timekeeper.Finish("load_char_igroup");
            Timekeeper.Start("load_char_calendar");

            // Calendar.
            XmlNodeList objXmlWeekList = objXmlCharacter.SelectNodes("calendar/week");
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
                    Weapon objWeapon = new Weapon(this);
                    objWeapon.Create(objXmlWeapon, _lstWeapons);
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
                        XmlNode xmlRootQualitiesNode = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities");
                        if (xmlRootQualitiesNode != null)
                        {
                            XmlNode objXmlDwarfQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Resistance to Pathogens/Toxins\"]");
                            if (objXmlDwarfQuality == null)
                                objXmlDwarfQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Dwarf Resistance\"]");
                            
                            List<Weapon> lstWeapons = new List<Weapon>();
                            Quality objQuality = new Quality(this);

                            objQuality.Create(objXmlDwarfQuality, QualitySource.Metatype, lstWeapons);
                            foreach (Weapon objWeapon in lstWeapons)
                                _lstWeapons.Add(objWeapon);
                            _lstQualities.Add(objQuality);
                        }
                    }
                    blnHasOldQualities = true;
                }
            }

            Timekeeper.Finish("load_char_dwarffix");
            Timekeeper.Start("load_char_cfix");

            // load issue where the contact multiplier was set to 0
            if (_intContactMultiplier == 0 && !string.IsNullOrEmpty(_strGameplayOption))
            {
                XmlNode objXmlGameplayOption = XmlManager.Load("gameplayoptions.xml").SelectSingleNode("/chummer/gameplayoptions/gameplayoption[name = \"" + _strGameplayOption + "\"]");
                if (objXmlGameplayOption != null)
                {
                    string strKarma = objXmlGameplayOption["karma"]?.InnerText;
                    string strNuyen = objXmlGameplayOption["maxnuyen"]?.InnerText;
                    string strContactMultiplier = string.Empty;
                    if (_objOptions.FreeContactsMultiplierEnabled)
                        strContactMultiplier = _objOptions.FreeContactsMultiplier.ToString();
                    else
                        strContactMultiplier = objXmlGameplayOption["contactmultiplier"]?.InnerText;
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
            Quality objMentorQuality = Qualities.FirstOrDefault(q => q.Name == "Mentor Spirit");
            if (objMentorQuality != null)
            {
                // We don't have any improvements tied to a cached Mentor Spirit value, so re-apply the improvement that adds the Mentor spirit
                if (!Improvements.Any(imp => imp.ImproveType == Improvement.ImprovementType.MentorSpirit && imp.ImprovedName != string.Empty))
                {
                    /* This gets confusing when selecting a mentor spirit mid-load, so just show the error and let the player manually re-apply
                    ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.Quality, objMentorQuality.InternalId);
                    string strSelected = objMentorQuality.Extra;

                    XmlNode objNode = objMentorQuality.MyXmlNode;
                    if (objNode != null)
                    {
                        if (objNode["bonus"] != null)
                        {
                            objMentorQuality.Bonus = objNode["bonus"];
                            ImprovementManager.ForcedValue = strSelected;
                            ImprovementManager.CreateImprovements(this, Improvement.ImprovementSource.Quality, objMentorQuality.InternalId, objNode["bonus"], false, 1, objMentorQuality.DisplayNameShort);
                            if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                            {
                                objMentorQuality.Extra = ImprovementManager.SelectedValue;
                            }
                        }
                    }
                    else
                    */
                    {
                        // Failed to re-apply the improvements immediately, so let's just add it for processing when the character is opened
                        _lstInternalIdsNeedingReapplyImprovements.Add(objMentorQuality.InternalId);
                    }
                }
            }
            Timekeeper.Finish("load_char_mentorspiritfix");

            RefreshRedliner();

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
        public void PrintToStream(MemoryStream objStream, XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
#else
        public void PrintToStream(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
#endif
        {
            string strMetatype = string.Empty;
            string strMetavariant = string.Empty;
            // Get the name of the Metatype and Metavariant.
            XmlDocument objMetatypeDoc = XmlManager.Load("metatypes.xml", strLanguageToPrint);
            XmlNode objMetatypeNode = objMetatypeDoc.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + Metatype + "\"]");
            if (objMetatypeNode == null)
            {
                objMetatypeDoc = XmlManager.Load("critters.xml", strLanguageToPrint);
                objMetatypeNode = objMetatypeDoc.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + Metatype + "\"]");
            }

            if (objMetatypeNode != null)
            {
                strMetatype = objMetatypeNode["translate"]?.InnerText ?? Metatype;

                if (!string.IsNullOrEmpty(Metavariant))
                {
                    objMetatypeNode = objMetatypeNode.SelectSingleNode("metavariants/metavariant[name = \"" + Metavariant + "\"]");

                    if (objMetatypeNode != null)
                        strMetavariant = objMetatypeNode["translate"]?.InnerText ?? Metavariant;
                }
            }

            // This line left in for debugging. Write the output to a fixed file name.
            //FileStream objStream = new FileStream("D:\\temp\\print.xml", FileMode.Create, FileAccess.Write, FileShare.ReadWrite);//(_strFileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);

            // <character>
            objWriter.WriteStartElement("character");

            // <metatype />
            objWriter.WriteElementString("metatype", strMetatype);
            // <metatype_english />
            objWriter.WriteElementString("metatype_english", Metatype);
            // <metavariant />
            objWriter.WriteElementString("metavariant", strMetavariant);
            // <metavariant_english />
            objWriter.WriteElementString("metavariant_english", Metavariant);
            // <movement />
            objWriter.WriteElementString("movement", FullMovement(objCulture, strLanguageToPrint));
            // <walk />
            objWriter.WriteElementString("walk", FullMovement(objCulture, strLanguageToPrint));
            // <run />
            objWriter.WriteElementString("run", FullMovement(objCulture, strLanguageToPrint));
            // <sprint />
            objWriter.WriteElementString("sprint", FullMovement(objCulture, strLanguageToPrint));
            // <movementwalk />
            objWriter.WriteElementString("movementwalk", GetMovement(objCulture, strLanguageToPrint));
            // <movementswim />
            objWriter.WriteElementString("movementswim", GetSwim(objCulture, strLanguageToPrint));
            // <movementfly />
            objWriter.WriteElementString("movementfly", GetFly(objCulture, strLanguageToPrint));

            // <gameplayoption />
            objWriter.WriteElementString("gameplayoption", GameplayOption);
            // <maxkarma />
            objWriter.WriteElementString("maxkarma", MaxKarma.ToString(objCulture));
            // <maxnuyen />
            objWriter.WriteElementString("maxnuyen", MaxNuyen.ToString(Options.NuyenFormat, objCulture));
            // <contactmultiplier />
            objWriter.WriteElementString("contactmultiplier", ContactMultiplier.ToString(objCulture));
            // <prioritymetatype />
            objWriter.WriteElementString("prioritymetatype", MetatypePriority);
            // <priorityattributes />
            objWriter.WriteElementString("priorityattributes", AttributesPriority);
            // <priorityspecial />
            objWriter.WriteElementString("priorityspecial", SpecialPriority);
            // <priorityskills />
            objWriter.WriteElementString("priorityskills", SkillsPriority);
            // <priorityresources />
            objWriter.WriteElementString("priorityresources", ResourcesPriority);
            // <priorityskills >
            objWriter.WriteStartElement("priorityskills");
            foreach (string strSkill in PriorityBonusSkillList)
            {
                objWriter.WriteElementString("priorityskill", strSkill);
            }
            // </priorityskills>
            objWriter.WriteEndElement();

            // <handedness />
            if (Ambidextrous)
            {
                objWriter.WriteElementString("primaryarm", LanguageManager.GetString("String_Ambidextrous", strLanguageToPrint));
            }
            else if (PrimaryArm == "Left")
            {
                objWriter.WriteElementString("primaryarm", LanguageManager.GetString("String_Improvement_SideLeft", strLanguageToPrint));
            }
            else
            {
                objWriter.WriteElementString("primaryarm", LanguageManager.GetString("String_Improvement_SideRight", strLanguageToPrint));
            }

            // If the character does not have a name, call them Unnamed Character. This prevents a transformed document from having a self-terminated title tag which causes browser to not rendering anything.
            // <name />
            if (!string.IsNullOrEmpty(Name))
                objWriter.WriteElementString("name", Name);
            else
                objWriter.WriteElementString("name", LanguageManager.GetString("String_UnnamedCharacter", strLanguageToPrint));

            PrintMugshots(objWriter);

            // <sex />
            objWriter.WriteElementString("sex", Sex);
            // <age />
            objWriter.WriteElementString("age", Age);
            // <eyes />
            objWriter.WriteElementString("eyes", Eyes);
            // <height />
            objWriter.WriteElementString("height", Height);
            // <weight />
            objWriter.WriteElementString("weight", Weight);
            // <skin />
            objWriter.WriteElementString("skin", Skin);
            // <hair />
            objWriter.WriteElementString("hair", Hair);
            // <description />
            objWriter.WriteElementString("description", Description);
            // <background />
            objWriter.WriteElementString("background", Background);
            // <concept />
            objWriter.WriteElementString("concept", Concept);
            // <notes />
            objWriter.WriteElementString("notes", Notes);
            // <alias />
            objWriter.WriteElementString("alias", Alias);
            // <playername />
            objWriter.WriteElementString("playername", PlayerName);
            // <gamenotes />
            objWriter.WriteElementString("gamenotes", GameNotes);

            // <limitphysical />
            objWriter.WriteElementString("limitphysical", LimitPhysical.ToString(objCulture));
            // <limitmental />
            objWriter.WriteElementString("limitmental", LimitMental.ToString(objCulture));
            // <limitsocial />
            objWriter.WriteElementString("limitsocial", LimitSocial.ToString(objCulture));
            // <limitastral />
            objWriter.WriteElementString("limitastral", LimitAstral.ToString(objCulture));
            // <contactpoints />
            objWriter.WriteElementString("contactpoints", ContactPoints.ToString(objCulture));
            // <contactpointsused />
            objWriter.WriteElementString("contactpointsused", ContactPointsUsed.ToString(objCulture));
            // <cfplimit />
            objWriter.WriteElementString("cfplimit", CFPLimit.ToString(objCulture));
            // <totalaiprogramlimit />
            objWriter.WriteElementString("ainormalprogramlimit", AINormalProgramLimit.ToString(objCulture));
            // <aiadvancedprogramlimit />
            objWriter.WriteElementString("aiadvancedprogramlimit", AIAdvancedProgramLimit.ToString(objCulture));
            // <spelllimit />
            objWriter.WriteElementString("spelllimit", SpellLimit.ToString(objCulture));
            // <karma />
            objWriter.WriteElementString("karma", Karma.ToString(objCulture));
            // <totalkarma />
            objWriter.WriteElementString("totalkarma", CareerKarma.ToString(objCulture));
            // <special />
            objWriter.WriteElementString("special", Special.ToString(objCulture));
            // <totalspecial />
            objWriter.WriteElementString("totalspecial", TotalSpecial.ToString(objCulture));
            // <attributes />
            objWriter.WriteElementString("attributes", Attributes.ToString(objCulture));
            // <totalattributes />
            objWriter.WriteElementString("totalattributes", TotalAttributes.ToString(objCulture));
            // <streetcred />
            objWriter.WriteElementString("streetcred", StreetCred.ToString(objCulture));
            // <calculatedstreetcred />
            objWriter.WriteElementString("calculatedstreetcred", CalculatedStreetCred.ToString(objCulture));
            // <totalstreetcred />
            objWriter.WriteElementString("totalstreetcred", TotalStreetCred.ToString(objCulture));
            // <burntstreetcred />
            objWriter.WriteElementString("burntstreetcred", BurntStreetCred.ToString(objCulture));
            // <notoriety />
            objWriter.WriteElementString("notoriety", Notoriety.ToString(objCulture));
            // <calculatednotoriety />
            objWriter.WriteElementString("calculatednotoriety", CalculatedNotoriety.ToString(objCulture));
            // <totalnotoriety />
            objWriter.WriteElementString("totalnotoriety", TotalNotoriety.ToString(objCulture));
            // <publicawareness />
            objWriter.WriteElementString("publicawareness", PublicAwareness.ToString(objCulture));
            // <calculatedpublicawareness />
            objWriter.WriteElementString("calculatedpublicawareness", CalculatedPublicAwareness.ToString(objCulture));
            // <totalpublicawareness />
            objWriter.WriteElementString("totalpublicawareness", TotalPublicAwareness.ToString(objCulture));
            // <created />
            objWriter.WriteElementString("created", Created.ToString());
            // <nuyen />
            objWriter.WriteElementString("nuyen", Nuyen.ToString(Options.NuyenFormat, objCulture));
            // <adeptwaydiscount />
            objWriter.WriteElementString("adeptwaydiscount", AdeptWayDiscount.ToString(objCulture));
            // <adept />
            objWriter.WriteElementString("adept", AdeptEnabled.ToString());
            // <magician />
            objWriter.WriteElementString("magician", MagicianEnabled.ToString());
            // <technomancer />
            objWriter.WriteElementString("technomancer", TechnomancerEnabled.ToString());
            // <ai />
            objWriter.WriteElementString("ai", AdvancedProgramsEnabled.ToString());
            // <cyberwaredisabled />
            objWriter.WriteElementString("cyberwaredisabled", CyberwareDisabled.ToString());
            // <critter />
            objWriter.WriteElementString("critter", CritterEnabled.ToString());

            int intESSDecimals = _objOptions.EssenceDecimals;
            string strESSFormat = "#,0";
            if (intESSDecimals > 0)
            {
                StringBuilder objESSFormat = new StringBuilder(".");
                for (int i = 0; i < intESSDecimals; ++i)
                    objESSFormat.Append('0');
                strESSFormat += objESSFormat.ToString();
            }

            objWriter.WriteElementString("totaless", Essence.ToString(strESSFormat, objCulture));
            // <tradition />
            string strTraditionName = MagicTradition;
            if (strTraditionName == "Custom")
                strTraditionName = TraditionName;
            objWriter.WriteStartElement("tradition");

            if (!string.IsNullOrEmpty(strTraditionName))
            {
                XmlDocument xmlTraditions = XmlManager.Load("traditions.xml", strLanguageToPrint);
                string strDrainAtt = TraditionDrain;
                XmlNode objXmlTradition = xmlTraditions.SelectSingleNode("/chummer/traditions/tradition[name = \"" + MagicTradition + "\"]");

                if (objXmlTradition != null)
                {
                    string strName = objXmlTradition["name"]?.InnerText;
                    if (!string.IsNullOrEmpty(strName) && strName != "Custom")
                    {
                        strTraditionName = objXmlTradition["translate"]?.InnerText ?? strName;
                    }
                }
                
                StringBuilder objDrain = new StringBuilder(strDrainAtt);
                foreach (string strAttribute in AttributeSection.AttributeStrings)
                {
                    CharacterAttrib objAttrib = GetAttribute(strAttribute);
                    objDrain.CheapReplace(strDrainAtt, objAttrib.Abbrev, () => objAttrib.TotalValue.ToString());
                }
                string strDrain = objDrain.ToString();
                if (string.IsNullOrEmpty(strDrain))
                {
                    strDrain = "0";
                }

                // Add any Improvements for Drain Resistance.
                int intDrain = Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(strDrain)) + ImprovementManager.ValueOf(this, Improvement.ImprovementType.DrainResistance);

                objWriter.WriteElementString("drain", strDrainAtt + " (" + intDrain.ToString(objCulture) + ')');
                objWriter.WriteStartElement("drainattribute");
                foreach (string drainAttribute in strDrainAtt.Replace('+', ' ').Split(new [] {' '} , StringSplitOptions.RemoveEmptyEntries))
                {
                    objWriter.WriteElementString("attr",drainAttribute);
                }
                objWriter.WriteEndElement();

                string strSpiritCombat = SpiritCombat;
                string strSpiritDetection = SpiritDetection;
                string strSpiritHealth = SpiritHealth;
                string strSpiritIllusion = SpiritIllusion;
                string strSpiritManipulation = SpiritManipulation;
                string strNone = LanguageManager.GetString("String_None", strLanguageToPrint);
                if (MagicTradition != "Custom")
                {
                    strSpiritCombat = objXmlTradition.SelectSingleNode("spirits/spiritcombat")?.InnerText ?? strNone;
                    if (strSpiritCombat == "All")
                        strSpiritCombat = LanguageManager.GetString("String_All", strLanguageToPrint);
                    strSpiritDetection = objXmlTradition.SelectSingleNode("spirits/spiritdetection")?.InnerText ?? strNone;
                    if (strSpiritDetection == "All")
                        strSpiritDetection = LanguageManager.GetString("String_All", strLanguageToPrint);
                    strSpiritHealth = objXmlTradition.SelectSingleNode("spirits/spirithealth")?.InnerText ?? strNone;
                    if (strSpiritHealth == "All")
                        strSpiritHealth = LanguageManager.GetString("String_All", strLanguageToPrint);
                    strSpiritIllusion = objXmlTradition.SelectSingleNode("spirits/spiritillusion")?.InnerText ?? strNone;
                    if (strSpiritIllusion == "All")
                        strSpiritIllusion = LanguageManager.GetString("String_All", strLanguageToPrint);
                    strSpiritManipulation = objXmlTradition.SelectSingleNode("spirits/spiritmanipulation")?.InnerText ?? strNone;
                    if (strSpiritManipulation == "All")
                        strSpiritManipulation = LanguageManager.GetString("String_All", strLanguageToPrint);
                }
                else
                {
                    if (string.IsNullOrEmpty(strSpiritCombat))
                        strSpiritCombat = strNone;
                    if (string.IsNullOrEmpty(strSpiritDetection))
                        strSpiritDetection = strNone;
                    if (string.IsNullOrEmpty(strSpiritHealth))
                        strSpiritHealth = strNone;
                    if (string.IsNullOrEmpty(strSpiritIllusion))
                        strSpiritIllusion = strNone;
                    if (string.IsNullOrEmpty(strSpiritManipulation))
                        strSpiritManipulation = strNone;
                }

                objWriter.WriteElementString("spiritcombat", xmlTraditions.SelectSingleNode("/chummer/spirits/spirit[name = \"" + strSpiritCombat + "\"]/translate")?.InnerText ?? strSpiritCombat);
                objWriter.WriteElementString("spiritdetection", xmlTraditions.SelectSingleNode("/chummer/spirits/spirit[name = \"" + strSpiritDetection + "\"]/translate")?.InnerText ?? strSpiritDetection);
                objWriter.WriteElementString("spirithealth", xmlTraditions.SelectSingleNode("/chummer/spirits/spirit[name = \"" + strSpiritHealth + "\"]/translate")?.InnerText ?? strSpiritHealth);
                objWriter.WriteElementString("spiritillusion", xmlTraditions.SelectSingleNode("/chummer/spirits/spirit[name = \"" + strSpiritIllusion + "\"]/translate")?.InnerText ?? strSpiritIllusion);
                objWriter.WriteElementString("spiritmanipulation", xmlTraditions.SelectSingleNode("/chummer/spirits/spirit[name = \"" + strSpiritManipulation + "\"]/translate")?.InnerText ?? strSpiritManipulation);

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
                string strDrainAtt = TechnomancerFading;
                StringBuilder objDrain = new StringBuilder(strDrainAtt);
                foreach (string strAttribute in AttributeSection.AttributeStrings)
                {
                    CharacterAttrib objAttrib = GetAttribute(strAttribute);
                    objDrain.CheapReplace(strDrainAtt, objAttrib.Abbrev, () => objAttrib.TotalValue.ToString());
                }
                string strDrain = objDrain.ToString();
                if (string.IsNullOrEmpty(strDrain))
                    strDrain = "0";

                // Add any Improvements for Fading Resistance.
                int intDrain = Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(strDrain)) + ImprovementManager.ValueOf(this, Improvement.ImprovementType.FadingResistance);

                objWriter.WriteElementString("drain", strDrainAtt + " (" + intDrain.ToString(objCulture) + ')');
            }

            // <attributes>
            objWriter.WriteStartElement("attributes");
	        AttributeSection.Print(objWriter, objCulture, strLanguageToPrint);

            // </attributes>
            objWriter.WriteEndElement();

            // <armor />
            objWriter.WriteElementString("armor", TotalArmorRating.ToString(objCulture));
            // <firearmor />
            objWriter.WriteElementString("firearmor", TotalFireArmorRating.ToString(objCulture));
            // <coldarmor />
            objWriter.WriteElementString("coldarmor", TotalColdArmorRating.ToString(objCulture));
            // <electricityarmor />
            objWriter.WriteElementString("electricityarmor", TotalElectricityArmorRating.ToString(objCulture));
            // <acidarmor />
            objWriter.WriteElementString("acidarmor", TotalAcidArmorRating.ToString(objCulture));
            // <fallingarmor />
            objWriter.WriteElementString("fallingarmor", TotalFallingArmorRating.ToString(objCulture));
            // <armordicestun />
            objWriter.WriteElementString("armordicestun", (BOD.TotalValue + TotalArmorRating).ToString(objCulture));
            // <firearmordicestun />
            objWriter.WriteElementString("firearmordicestun", (BOD.TotalValue + TotalFireArmorRating).ToString(objCulture));
            // <coldarmordicestun />
            objWriter.WriteElementString("coldarmordicestun", (BOD.TotalValue + TotalColdArmorRating).ToString(objCulture));
            // <electricityarmordicestun />
            objWriter.WriteElementString("electricityarmordicestun", (BOD.TotalValue + TotalElectricityArmorRating).ToString(objCulture));
            // <acidarmordicestun />
            objWriter.WriteElementString("acidarmordicestun", (BOD.TotalValue + TotalAcidArmorRating).ToString(objCulture));
            // <fallingarmordicestun />
            objWriter.WriteElementString("fallingarmordicestun", (BOD.TotalValue + TotalFallingArmorRating).ToString(objCulture));
            // <armordicephysical />
            objWriter.WriteElementString("armordicephysical", (BOD.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.DamageResistance) + TotalArmorRating).ToString(objCulture));
            // <firearmordicephysical />
            objWriter.WriteElementString("firearmordicephysical", (BOD.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.DamageResistance) + TotalFireArmorRating).ToString(objCulture));
            // <coldarmordicephysical />
            objWriter.WriteElementString("coldarmordicephysical", (BOD.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.DamageResistance) + TotalColdArmorRating).ToString(objCulture));
            // <electricityarmordicephysical />
            objWriter.WriteElementString("electricityarmordicephysical", (BOD.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.DamageResistance) + TotalElectricityArmorRating).ToString(objCulture));
            // <acidarmordicephysical />
            objWriter.WriteElementString("acidarmordicephysical", (BOD.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.DamageResistance) + TotalAcidArmorRating).ToString(objCulture));
            // <fallingarmordicephysical />
            objWriter.WriteElementString("fallingarmordicephysical", (BOD.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.DamageResistance) + TotalFallingArmorRating).ToString(objCulture));

            // Condition Monitors.
            // <physicalcm />
            objWriter.WriteElementString("physicalcm", PhysicalCM.ToString(objCulture));
            // <stuncm />
            objWriter.WriteElementString("stuncm", StunCM.ToString(objCulture));

            // Condition Monitor Progress.
            // <physicalcmfilled />
            objWriter.WriteElementString("physicalcmfilled", PhysicalCMFilled.ToString(objCulture));
            // <stuncmfilled />
            objWriter.WriteElementString("stuncmfilled", StunCMFilled.ToString(objCulture));

            // <cmthreshold>
            objWriter.WriteElementString("cmthreshold", CMThreshold.ToString(objCulture));
            // <cmthresholdoffset>
            objWriter.WriteElementString("physicalcmthresholdoffset", PhysicalCMThresholdOffset.ToString(objCulture));
            // <cmthresholdoffset>
            objWriter.WriteElementString("stuncmthresholdoffset", StunCMThresholdOffset.ToString(objCulture));
            // <cmoverflow>
            objWriter.WriteElementString("cmoverflow", CMOverflow.ToString(objCulture));

            // Calculate Initiatives.
            // Initiative.
            objWriter.WriteElementString("init", GetInitiative(objCulture, strLanguageToPrint));
            objWriter.WriteElementString("initdice", InitiativeDice.ToString(objCulture));
            objWriter.WriteElementString("initvalue", InitiativeValue.ToString(objCulture));
            objWriter.WriteElementString("initbonus", Math.Max(ImprovementManager.ValueOf(this, Improvement.ImprovementType.Initiative), 0).ToString(objCulture));

            // Astral Initiative.
            if (MAGEnabled)
            {
                objWriter.WriteElementString("astralinit", GetAstralInitiative(objCulture, strLanguageToPrint));
                objWriter.WriteElementString("astralinitdice", AstralInitiativeDice.ToString(objCulture));
                objWriter.WriteElementString("astralinitvalue", AstralInitiativeValue.ToString(objCulture));
            }

            // Matrix Initiative (AR).
            objWriter.WriteElementString("matrixarinit", GetMatrixInitiative(objCulture, strLanguageToPrint));
            objWriter.WriteElementString("matrixarinitdice", MatrixInitiativeDice.ToString(objCulture));
            objWriter.WriteElementString("matrixarinitvalue", MatrixInitiativeValue.ToString(objCulture));

            // Matrix Initiative (Cold).
            objWriter.WriteElementString("matrixcoldinit", GetMatrixInitiativeCold(objCulture, strLanguageToPrint));
            objWriter.WriteElementString("matrixcoldinitdice", MatrixInitiativeDice.ToString(objCulture));
            objWriter.WriteElementString("matrixcoldinitvalue", MatrixInitiativeValue.ToString(objCulture));

            // Matrix Initiative (Hot).
            objWriter.WriteElementString("matrixhotinit", GetMatrixInitiativeHot(objCulture, strLanguageToPrint));
            objWriter.WriteElementString("matrixhotinitdice", MatrixInitiativeDice.ToString(objCulture));
            objWriter.WriteElementString("matrixhotinitvalue", MatrixInitiativeValue.ToString(objCulture));

            // Rigger Initiative.
            objWriter.WriteElementString("riggerinit", GetInitiative(objCulture, strLanguageToPrint));

            // <magenabled />
            objWriter.WriteElementString("magenabled", MAGEnabled.ToString());
            // <initiategrade />
            objWriter.WriteElementString("initiategrade", InitiateGrade.ToString(objCulture));
            // <resenabled />
            objWriter.WriteElementString("resenabled", RESEnabled.ToString());
            // <submersiongrade />
            objWriter.WriteElementString("submersiongrade", SubmersionGrade.ToString(objCulture));
            // <depenabled />
            objWriter.WriteElementString("depenabled", DEPEnabled.ToString());
            // <groupmember />
            objWriter.WriteElementString("groupmember", GroupMember.ToString());
            // <groupname />
            objWriter.WriteElementString("groupname", GroupName);
            // <groupnotes />
            objWriter.WriteElementString("groupnotes", GroupNotes);

            // <composure />
            objWriter.WriteElementString("composure", Composure.ToString(objCulture));
            // <judgeintentions />
            objWriter.WriteElementString("judgeintentions", JudgeIntentions.ToString(objCulture));
            // <judgeintentionsresist />
            objWriter.WriteElementString("judgeintentionsresist", JudgeIntentionsResist.ToString(objCulture));
            // <liftandcarry />
            objWriter.WriteElementString("liftandcarry", LiftAndCarry.ToString(objCulture));
            // <memory />
            objWriter.WriteElementString("memory", Memory.ToString(objCulture));
            // <liftweight />
            objWriter.WriteElementString("liftweight", (STR.TotalValue * 15).ToString(objCulture));
            // <carryweight />
            objWriter.WriteElementString("carryweight", (STR.TotalValue * 10).ToString(objCulture));
            // <fatigueresist />
            objWriter.WriteElementString("fatigueresist", FatigueResist.ToString(objCulture));
            // <radiationresist />
            objWriter.WriteElementString("radiationresist", RadiationResist.ToString(objCulture));
            // <sonicresist />
            objWriter.WriteElementString("sonicresist", SonicResist.ToString(objCulture));
            // <toxincontacttesist />
            objWriter.WriteElementString("toxincontactresist", ToxinContactResist(strLanguageToPrint, objCulture));
            // <toxiningestionresist />
            objWriter.WriteElementString("toxiningestionresist", ToxinIngestionResist(strLanguageToPrint, objCulture));
            // <toxininhalationresist />
            objWriter.WriteElementString("toxininhalationresist", ToxinInhalationResist(strLanguageToPrint, objCulture));
            // <toxininjectionresist />
            objWriter.WriteElementString("toxininjectionresist", ToxinInjectionResist(strLanguageToPrint, objCulture));
            // <pathogencontactresist />
            objWriter.WriteElementString("pathogencontactresist", PathogenContactResist(strLanguageToPrint, objCulture));
            // <pathogeningestionresist />
            objWriter.WriteElementString("pathogeningestionresist", PathogenIngestionResist(strLanguageToPrint, objCulture));
            // <pathogeninhalationresist />
            objWriter.WriteElementString("pathogeninhalationresist", PathogenInhalationResist(strLanguageToPrint, objCulture));
            // <pathogeninjectionresist />
            objWriter.WriteElementString("pathogeninjectionresist", PathogenInjectionResist(strLanguageToPrint, objCulture));
            // <physiologicaladdictionresistfirsttime />
            objWriter.WriteElementString("physiologicaladdictionresistfirsttime", PhysiologicalAddictionResistFirstTime.ToString(objCulture));
            // <physiologicaladdictionresistalreadyaddicted />
            objWriter.WriteElementString("physiologicaladdictionresistalreadyaddicted", PhysiologicalAddictionResistAlreadyAddicted.ToString(objCulture));
            // <psychologicaladdictionresistfirsttime />
            objWriter.WriteElementString("psychologicaladdictionresistfirsttime", PsychologicalAddictionResistFirstTime.ToString(objCulture));
            // <psychologicaladdictionresistalreadyaddicted />
            objWriter.WriteElementString("psychologicaladdictionresistalreadyaddicted", PsychologicalAddictionResistAlreadyAddicted.ToString(objCulture));
            // <physicalcmnaturalrecovery />
            objWriter.WriteElementString("physicalcmnaturalrecovery", PhysicalCMNaturalRecovery.ToString(objCulture));
            // <stuncmnaturalrecovery />
            objWriter.WriteElementString("stuncmnaturalrecovery", StunCMNaturalRecovery.ToString(objCulture));

            // Spell Resistances
            //Indirect Dodge
            objWriter.WriteElementString("indirectdefenseresist", (INT.TotalValue + REA.TotalValue + TotalBonusDodgeRating).ToString(objCulture));
            //Direct Soak - Mana
            objWriter.WriteElementString("directmanaresist", (WIL.TotalValue + SpellResistance).ToString(objCulture));
            //Direct Soak - Physical
            objWriter.WriteElementString("directphysicalresist", (BOD.TotalValue + SpellResistance).ToString(objCulture));
            //Detection Spells
            objWriter.WriteElementString("detectionspellresist", (LOG.TotalValue + WIL.TotalValue + SpellResistance + ImprovementManager.ValueOf(this, Improvement.ImprovementType.DetectionSpellResist)).ToString(objCulture));
            //Decrease Attribute - BOD
            objWriter.WriteElementString("decreasebodresist", (BOD.TotalValue + WIL.TotalValue + SpellResistance).ToString(objCulture));
            //Decrease Attribute - AGI
            objWriter.WriteElementString("decreaseagiresist", (AGI.TotalValue + WIL.TotalValue + SpellResistance).ToString(objCulture));
            //Decrease Attribute - REA
            objWriter.WriteElementString("decreaserearesist", (REA.TotalValue + WIL.TotalValue + SpellResistance).ToString(objCulture));
            //Decrease Attribute - STR
            objWriter.WriteElementString("decreasestrresist", (STR.TotalValue + WIL.TotalValue + SpellResistance).ToString(objCulture));
            //Decrease Attribute - CHA
            objWriter.WriteElementString("decreasecharesist", (CHA.TotalValue + WIL.TotalValue + SpellResistance).ToString(objCulture));
            //Decrease Attribute - INT
            objWriter.WriteElementString("decreaseintresist", (INT.TotalValue + WIL.TotalValue + SpellResistance).ToString(objCulture));
            //Decrease Attribute - LOG
            objWriter.WriteElementString("decreaselogresist", (LOG.TotalValue + WIL.TotalValue + SpellResistance).ToString(objCulture));
            //Decrease Attribute - WIL
            objWriter.WriteElementString("decreasewilresist", (WIL.TotalValue + WIL.TotalValue + SpellResistance).ToString(objCulture));
            //Illusion - Mana
            objWriter.WriteElementString("illusionmanaresist", (WIL.TotalValue + LOG.TotalValue + SpellResistance + ImprovementManager.ValueOf(this, Improvement.ImprovementType.ManaIllusionResist)).ToString(objCulture));
            //Illusion - Physical
            objWriter.WriteElementString("illusionphysicalresist", (INT.TotalValue + LOG.TotalValue + SpellResistance + ImprovementManager.ValueOf(this, Improvement.ImprovementType.PhysicalIllusionResist)).ToString(objCulture));
            //Manipulation - Mental
            objWriter.WriteElementString("manipulationmentalresist", (WIL.TotalValue + LOG.TotalValue + SpellResistance + ImprovementManager.ValueOf(this, Improvement.ImprovementType.MentalManipulationResist)).ToString(objCulture));
            //Manipulation - Physical
            objWriter.WriteElementString("manipulationphysicalresist", (STR.TotalValue + BOD.TotalValue + SpellResistance + ImprovementManager.ValueOf(this, Improvement.ImprovementType.PhysicalManipulationResist)).ToString(objCulture));

            // <skills>
            objWriter.WriteStartElement("skills");
            SkillsSection.Print(objWriter, objCulture, strLanguageToPrint);
            objWriter.WriteEndElement();

            // <contacts>
            objWriter.WriteStartElement("contacts");
            foreach (Contact objContact in Contacts)
            {
                objContact.Print(objWriter, objCulture, strLanguageToPrint);
            }
            // </contacts>
            objWriter.WriteEndElement();

            // <limitmodifiersphys>
            objWriter.WriteStartElement("limitmodifiersphys");
            foreach (LimitModifier objLimitModifier in LimitModifiers.Where(objLimitModifier => objLimitModifier.Limit == "Physical"))
            {
                objLimitModifier.Print(objWriter, strLanguageToPrint);
            }
            // Populate Limit Modifiers from Improvements
            foreach (Improvement objImprovement in Improvements.Where(objImprovement => (objImprovement.ImproveType == Improvement.ImprovementType.LimitModifier && objImprovement.ImprovedName == "Physical")))
            {
                string strName = GetObjectName(objImprovement, strLanguageToPrint);
                if (strName == objImprovement.SourceName)
                    strName = objImprovement.UniqueName;
                strName += ": ";
                if (objImprovement.Value > 0)
                    strName += '+';
                strName += objImprovement.Value.ToString(objCulture);

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
            foreach (LimitModifier objLimitModifier in LimitModifiers.Where(objLimitModifier => objLimitModifier.Limit == "Mental"))
            {
                objLimitModifier.Print(objWriter, strLanguageToPrint);
            }
            // Populate Limit Modifiers from Improvements
            foreach (Improvement objImprovement in Improvements.Where(objImprovement => (objImprovement.ImproveType == Improvement.ImprovementType.LimitModifier && objImprovement.ImprovedName == "Mental")))
            {
                string strName = GetObjectName(objImprovement, strLanguageToPrint);
                if (strName == objImprovement.SourceName)
                    strName = objImprovement.UniqueName;
                strName += ": ";
                if (objImprovement.Value > 0)
                    strName += '+';
                strName += objImprovement.Value.ToString(objCulture);

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
            foreach (LimitModifier objLimitModifier in LimitModifiers.Where(objLimitModifier => objLimitModifier.Limit == "Social"))
            {
                objLimitModifier.Print(objWriter, strLanguageToPrint);
            }
            // Populate Limit Modifiers from Improvements
            foreach (Improvement objImprovement in Improvements.Where(objImprovement => (objImprovement.ImproveType == Improvement.ImprovementType.LimitModifier && objImprovement.ImprovedName == "Social")))
            {
                string strName = GetObjectName(objImprovement, strLanguageToPrint);
                if (strName == objImprovement.SourceName)
                    strName = objImprovement.UniqueName;
                strName += ": ";
                if (objImprovement.Value > 0)
                    strName += '+';
                strName += objImprovement.Value.ToString(objCulture);

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
            foreach (Spell objSpell in Spells)
            {
                objSpell.Print(objWriter, objCulture, strLanguageToPrint);
            }
            // </spells>
            objWriter.WriteEndElement();

            // <powers>
            objWriter.WriteStartElement("powers");
            foreach (Power objPower in Powers)
            {
                objPower.Print(objWriter, objCulture, strLanguageToPrint);
            }
            // </powers>
            objWriter.WriteEndElement();

            // <spirits>
            objWriter.WriteStartElement("spirits");
            foreach (Spirit objSpirit in Spirits)
            {
                objSpirit.Print(objWriter, objCulture, strLanguageToPrint);
            }
            // </spirits>
            objWriter.WriteEndElement();

            // <complexforms>
            objWriter.WriteStartElement("complexforms");
            foreach (ComplexForm objComplexForm in ComplexForms)
            {
                objComplexForm.Print(objWriter, strLanguageToPrint);
            }
            // </complexforms>
            objWriter.WriteEndElement();

            // <aiprograms>
            objWriter.WriteStartElement("aiprograms");
            foreach (AIProgram objProgram in AIPrograms)
            {
                objProgram.Print(objWriter, strLanguageToPrint);
            }
            // </aiprograms>
            objWriter.WriteEndElement();

            // <martialarts>
            objWriter.WriteStartElement("martialarts");
            foreach (MartialArt objMartialArt in MartialArts)
            {
                objMartialArt.Print(objWriter, objCulture, strLanguageToPrint);
            }
            // </martialarts>
            objWriter.WriteEndElement();

            #if LEGACY
            // <martialartmaneuvers>
            objWriter.WriteStartElement("martialartmaneuvers");
            foreach (MartialArtManeuver objManeuver in MartialArtManeuvers)
            {
                objManeuver.Print(objWriter, strLanguageToPrint);
            }
            // </martialartmaneuvers>
            objWriter.WriteEndElement();
            #endif

            // <armors>
            objWriter.WriteStartElement("armors");
            foreach (Armor objArmor in Armor)
            {
                objArmor.Print(objWriter, objCulture, strLanguageToPrint);
            }
            // </armors>
            objWriter.WriteEndElement();

            // <weapons>
            objWriter.WriteStartElement("weapons");
            foreach (Weapon objWeapon in Weapons)
            {
                objWeapon.Print(objWriter, objCulture, strLanguageToPrint);
            }
            // </weapons>
            objWriter.WriteEndElement();

            // <cyberwares>
            objWriter.WriteStartElement("cyberwares");
            foreach (Cyberware objCyberware in Cyberware)
            {
                objCyberware.Print(objWriter, objCulture, strLanguageToPrint);
            }
            // </cyberwares>
            objWriter.WriteEndElement();

            // <qualities>
            // Multiple instances of the same quality are combined into just one entry with a number next to it (e.g. 6 discrete entries of "Focused Concentration" become "Focused Concentration 6")
            Dictionary<string, int> strQualitiesToPrint = new Dictionary<string, int>(Qualities.Count);
            foreach (Quality objQuality in Qualities)
            {
                string strKey = objQuality.QualityId + '|' + objQuality.SourceName + '|' + objQuality.Extra;
                if (strQualitiesToPrint.ContainsKey(strKey))
                {
                    strQualitiesToPrint[strKey] += 1;
                }
                else
                {
                    strQualitiesToPrint.Add(strKey, 1);
                }
            }
            int intLoopRating = 0;
            objWriter.WriteStartElement("qualities");
            foreach (Quality objQuality in Qualities)
            {
                string strKey = objQuality.QualityId + '|' + objQuality.SourceName + '|' + objQuality.Extra;
                if (strQualitiesToPrint.TryGetValue(strKey, out intLoopRating))
                {
                    objQuality.Print(objWriter, intLoopRating, objCulture, strLanguageToPrint);
                    strQualitiesToPrint.Remove(strKey);
                }
            }
            // </qualities>
            objWriter.WriteEndElement();

            // <lifestyles>
            objWriter.WriteStartElement("lifestyles");
            foreach (Lifestyle objLifestyle in Lifestyles)
            {
                objLifestyle.Print(objWriter, objCulture, strLanguageToPrint);
            }
            // </lifestyles>
            objWriter.WriteEndElement();

            // <gears>
            objWriter.WriteStartElement("gears");
            foreach (Gear objGear in Gear)
            {
                objGear.Print(objWriter, objCulture, strLanguageToPrint);
            }
            // </gears>
            objWriter.WriteEndElement();

            // <vehicles>
            objWriter.WriteStartElement("vehicles");
            foreach (Vehicle objVehicle in Vehicles)
            {
                objVehicle.Print(objWriter, objCulture, strLanguageToPrint);
            }
            // </vehicles>
            objWriter.WriteEndElement();

            // <metamagics>
            objWriter.WriteStartElement("metamagics");
            foreach (Metamagic objMetamagic in Metamagics)
            {
                objMetamagic.Print(objWriter, objCulture, strLanguageToPrint);
            }
            // </metamagics>
            objWriter.WriteEndElement();

            // <arts>
            objWriter.WriteStartElement("arts");
            foreach (Art objArt in Arts)
            {
                objArt.Print(objWriter, strLanguageToPrint);
            }
            // </arts>
            objWriter.WriteEndElement();

            // <enhancements>
            objWriter.WriteStartElement("enhancements");
            foreach (Enhancement objEnhancement in Enhancements)
            {
                objEnhancement.Print(objWriter, strLanguageToPrint);
            }
            // </enhancements>
            objWriter.WriteEndElement();

            // <critterpowers>
            objWriter.WriteStartElement("critterpowers");
            foreach (CritterPower objPower in CritterPowers)
            {
                objPower.Print(objWriter, strLanguageToPrint);
            }
            // </critterpowers>
            objWriter.WriteEndElement();

            // <calendar>
            objWriter.WriteStartElement("calendar");
            //Calendar.Sort();
            foreach (CalendarWeek objWeek in Calendar)
                objWeek.Print(objWriter, objCulture, Options.PrintNotes);
            // </expenses>
            objWriter.WriteEndElement();

            // Print the Expense Log Entries if the option is enabled.
            if (Options.PrintExpenses)
            {
                // <expenses>
                objWriter.WriteStartElement("expenses");
                ((List<ExpenseLogEntry>)ExpenseEntries).Sort(ExpenseLogEntry.CompareDate);
                foreach (ExpenseLogEntry objExpense in ExpenseEntries)
                    objExpense.Print(objWriter, objCulture, strLanguageToPrint);
                // </expenses>
                objWriter.WriteEndElement();
            }

            // </character>
            objWriter.WriteEndElement();
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
            _intRedlinerBonus = 0;
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
            _lstImprovements.Clear();
            _lstContacts.Clear();
            _lstSpirits.Clear();
            _lstSpells.Clear();
            _lstFoci.Clear();
            _lstStackedFoci.Clear();
            _lstPowers.Clear();
            _lstComplexForms.Clear();
            _lstAIPrograms.Clear();
            _lstMartialArts.Clear();
#if LEGACY
            _lstMartialArtManeuvers.Clear();
#endif
            _lstLimitModifiers.Clear();
            _lstArmor.Clear();
            _lstCyberware.Clear();
            _lstMetamagics.Clear();
            _lstArts.Clear();
            _lstEnhancements.Clear();
            _lstWeapons.Clear();
            _lstLifestyles.Clear();
            _lstGear.Clear();
            _lstVehicles.Clear();
            _lstExpenseLog.Clear();
            _lstCritterPowers.Clear();
            _lstInitiationGrades.Clear();
            _lstQualities.Clear();
            _lstOldQualities.Clear();
            _lstCalendar.Clear();

            SkillsSection.Reset();
            _lstCyberware.ListChanged += (x, y) => { ResetCachedEssence(); };
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
        public string GetObjectName(Improvement objImprovement, string strLanguage)
        {
            switch (objImprovement.ImproveSource)
            {
                case Improvement.ImprovementSource.Bioware:
                case Improvement.ImprovementSource.Cyberware:
                    Cyberware objReturnCyberware = Cyberware.DeepFirstOrDefault(x => x.Children, x => x.InternalId == objImprovement.SourceName);
                    if (objReturnCyberware != null)
                    {
                        string strWareReturn = objReturnCyberware.DisplayNameShort(strLanguage);
                        if (objReturnCyberware.Parent != null)
                            strWareReturn += " (" + objReturnCyberware.Parent.DisplayNameShort(strLanguage) + ')';
                        return strWareReturn;
                    }
                    foreach (Vehicle objVehicle in Vehicles)
                    {
                        foreach (VehicleMod objVehicleMod in objVehicle.Mods)
                        {
                            objReturnCyberware = objVehicleMod.Cyberware.DeepFirstOrDefault(x => x.Children, x => x.InternalId == objImprovement.SourceName);
                            if (objReturnCyberware != null)
                            {
                                string strWareReturn = objReturnCyberware.DisplayNameShort(strLanguage);
                                if (objReturnCyberware.Parent != null)
                                    strWareReturn += " (" + objVehicle.DisplayNameShort(strLanguage) + ", " + objVehicleMod.DisplayNameShort(strLanguage) + ", " + objReturnCyberware.Parent.DisplayNameShort(strLanguage) + ')';
                                else
                                    strWareReturn += " (" + objVehicle.DisplayNameShort(strLanguage) + ", " + objVehicleMod.DisplayNameShort(strLanguage) + ')';
                                return strWareReturn;
                            }
                        }
                    }
                    break;
                case Improvement.ImprovementSource.Gear:
                    Gear objReturnGear = Gear.DeepFirstOrDefault(x => x.Children, x => x.InternalId == objImprovement.SourceName);
                    if (objReturnGear != null)
                    {
                        string strGearReturn = objReturnGear.DisplayNameShort(strLanguage);
                        if (objReturnGear.Parent != null)
                            strGearReturn += " (" + objReturnGear.Parent.DisplayNameShort(strLanguage) + ')'; 
                        return strGearReturn;
                    }
                    foreach (Weapon objWeapon in Weapons.DeepWhere(x => x.Children, x => x.WeaponAccessories.Any(y => y.Gear.Count > 0)))
                    {
                        foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                        {
                            objReturnGear = objAccessory.Gear.DeepFirstOrDefault(x => x.Children, x => x.InternalId == objImprovement.SourceName);
                            if (objReturnGear != null)
                            {
                                string strGearReturn = objReturnGear.DisplayNameShort(strLanguage);
                                if (objReturnGear.Parent != null)
                                    strGearReturn += " (" + objWeapon.DisplayNameShort(strLanguage) + ", " + objAccessory.DisplayNameShort(strLanguage) + ", " + objReturnGear.Parent.DisplayNameShort(strLanguage) + ')';
                                else
                                    strGearReturn += " (" + objWeapon.DisplayNameShort(strLanguage) + ", " + objAccessory.DisplayNameShort(strLanguage) + ')';
                                return strGearReturn;
                            }
                        }
                    }
                    foreach (Armor objArmor in Armor)
                    {
                        objReturnGear = objArmor.Gear.DeepFirstOrDefault(x => x.Children, x => x.InternalId == objImprovement.SourceName);
                        if (objReturnGear != null)
                        {
                            string strGearReturn = objReturnGear.DisplayNameShort(strLanguage);
                            if (objReturnGear.Parent != null)
                                strGearReturn += " (" + objArmor.DisplayNameShort(strLanguage) + ", " + objReturnGear.Parent.DisplayNameShort(strLanguage) + ')';
                            else
                                strGearReturn += " (" + objArmor.DisplayNameShort(strLanguage) + ')';
                            return strGearReturn;
                        }
                    }
                    foreach (Cyberware objCyberware in Cyberware.DeepWhere(x => x.Children, x => x.Gear.Count > 0))
                    {
                        objReturnGear = objCyberware.Gear.DeepFirstOrDefault(x => x.Children, x => x.InternalId == objImprovement.SourceName);
                        if (objReturnGear != null)
                        {
                            string strGearReturn = objReturnGear.DisplayNameShort(strLanguage);
                            if (objReturnGear.Parent != null)
                                strGearReturn += " (" + objCyberware.DisplayNameShort(strLanguage) + ", " + objReturnGear.Parent.DisplayNameShort(strLanguage) + ')';
                            else
                                strGearReturn += " (" + objCyberware.DisplayNameShort(strLanguage) + ')';
                            return strGearReturn;
                        }
                    }
                    foreach (Vehicle objVehicle in Vehicles)
                    {
                        objReturnGear = objVehicle.Gear.DeepFirstOrDefault(x => x.Children, x => x.InternalId == objImprovement.SourceName);
                        if (objReturnGear != null)
                        {
                            string strGearReturn = objReturnGear.DisplayNameShort(strLanguage);
                            if (objReturnGear.Parent != null)
                                strGearReturn += " (" + objVehicle.DisplayNameShort(strLanguage) + ", " + objReturnGear.Parent.DisplayNameShort(strLanguage) + ')';
                            else
                                strGearReturn += " (" + objVehicle.DisplayNameShort(strLanguage) + ')';
                            return strGearReturn;
                        }
                        foreach (Weapon objWeapon in objVehicle.Weapons.DeepWhere(x => x.Children, x => x.WeaponAccessories.Any(y => y.Gear.Count > 0)))
                        {
                            foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                            {
                                objReturnGear = objAccessory.Gear.DeepFirstOrDefault(x => x.Children, x => x.InternalId == objImprovement.SourceName);
                                if (objReturnGear != null)
                                {
                                    string strGearReturn = objReturnGear.DisplayNameShort(strLanguage);
                                    if (objReturnGear.Parent != null)
                                        strGearReturn += " (" + objVehicle.DisplayNameShort(strLanguage) + ", " + objWeapon.DisplayNameShort(strLanguage) + ", " + objAccessory.DisplayNameShort(strLanguage) + ", " + objReturnGear.Parent.DisplayNameShort(strLanguage) + ')';
                                    else
                                        strGearReturn += " (" + objVehicle.DisplayNameShort(strLanguage) + ", " + objWeapon.DisplayNameShort(strLanguage) + ", " + objAccessory.DisplayNameShort(strLanguage) + ')';
                                    return strGearReturn;
                                }
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
                                    {
                                        string strGearReturn = objReturnGear.DisplayNameShort(strLanguage);
                                        if (objReturnGear.Parent != null)
                                            strGearReturn += " (" + objVehicle.DisplayNameShort(strLanguage) + ", " + objVehicleMod.DisplayNameShort(strLanguage) + ", " + objWeapon.DisplayNameShort(strLanguage) + ", " + objAccessory.DisplayNameShort(strLanguage) + ", " + objReturnGear.Parent.DisplayNameShort(strLanguage) + ')';
                                        else
                                            strGearReturn += " (" + objVehicle.DisplayNameShort(strLanguage) + ", " + objVehicleMod.DisplayNameShort(strLanguage) + ", " + objWeapon.DisplayNameShort(strLanguage) + ", " + objAccessory.DisplayNameShort(strLanguage) + ')';
                                        return strGearReturn;
                                    }
                                }
                            }
                            foreach (Cyberware objCyberware in objVehicleMod.Cyberware.DeepWhere(x => x.Children, x => x.Gear.Count > 0))
                            {
                                objReturnGear = objCyberware.Gear.DeepFirstOrDefault(x => x.Children, x => x.InternalId == objImprovement.SourceName);
                                if (objReturnGear != null)
                                {
                                    string strGearReturn = objReturnGear.DisplayNameShort(strLanguage);
                                    if (objReturnGear.Parent != null)
                                        strGearReturn += " (" + objVehicle.DisplayNameShort(strLanguage) + ", " + objVehicleMod.DisplayNameShort(strLanguage) + ", " + objCyberware.DisplayNameShort(strLanguage) + ", " + objReturnGear.Parent.DisplayNameShort(strLanguage) + ')';
                                    else
                                        strGearReturn += " (" + objVehicle.DisplayNameShort(strLanguage) + ", " + objVehicleMod.DisplayNameShort(strLanguage) + ", " + objCyberware.DisplayNameShort(strLanguage) + ')';
                                    return strGearReturn;
                                }
                            }
                        }
                    }
                    break;
                case Improvement.ImprovementSource.Spell:
                    foreach (Spell objSpell in Spells)
                    {
                        if (objSpell.InternalId == objImprovement.SourceName)
                        {
                            return objSpell.DisplayNameShort(strLanguage);
                        }
                    }
                    break;
                case Improvement.ImprovementSource.Power:
                    foreach (Power objPower in Powers)
                    {
                        if (objPower.InternalId == objImprovement.SourceName)
                        {
                            return objPower.DisplayNameShort(strLanguage);
                        }
                    }
                    break;
                case Improvement.ImprovementSource.CritterPower:
                    foreach (CritterPower objPower in CritterPowers)
                    {
                        if (objPower.InternalId == objImprovement.SourceName)
                        {
                            return objPower.DisplayNameShort(strLanguage);
                        }
                    }
                    break;
                case Improvement.ImprovementSource.Metamagic:
                case Improvement.ImprovementSource.Echo:
                    foreach (Metamagic objMetamagic in Metamagics)
                    {
                        if (objMetamagic.InternalId == objImprovement.SourceName)
                        {
                            return objMetamagic.DisplayNameShort(strLanguage);
                        }
                    }
                    break;
                case Improvement.ImprovementSource.Art:
                    foreach (Art objArt in Arts)
                    {
                        if (objArt.InternalId == objImprovement.SourceName)
                        {
                            return objArt.DisplayNameShort(strLanguage);
                        }
                    }
                    break;
                case Improvement.ImprovementSource.Enhancement:
                    foreach (Enhancement objEnhancement in Enhancements)
                    {
                        if (objEnhancement.InternalId == objImprovement.SourceName)
                        {
                            return objEnhancement.DisplayNameShort(strLanguage);
                        }
                    }
                    break;
                case Improvement.ImprovementSource.Armor:
                    foreach (Armor objArmor in Armor)
                    {
                        if (objArmor.InternalId == objImprovement.SourceName)
                        {
                            return objArmor.DisplayNameShort(strLanguage);
                        }
                    }
                    break;
                case Improvement.ImprovementSource.ArmorMod:
                    foreach (Armor objArmor in Armor)
                    {
                        foreach (ArmorMod objMod in objArmor.ArmorMods)
                        {
                            if (objMod.InternalId == objImprovement.SourceName)
                            {
                                return objMod.DisplayNameShort(strLanguage) + " (" + objArmor.DisplayNameShort(strLanguage) + ')';
                            }
                        }
                    }
                    break;
                case Improvement.ImprovementSource.ComplexForm:
                    foreach (ComplexForm objComplexForm in ComplexForms)
                    {
                        if (objComplexForm.InternalId == objImprovement.SourceName)
                        {
                            return objComplexForm.DisplayNameShort(strLanguage);
                        }
                    }
                    break;
                case Improvement.ImprovementSource.AIProgram:
                    foreach (AIProgram objProgram in AIPrograms)
                    {
                        if (objProgram.InternalId == objImprovement.SourceName)
                        {
                            return objProgram.DisplayNameShort(strLanguage);
                        }
                    }
                    break;
                case Improvement.ImprovementSource.Quality:
                    if (objImprovement.SourceName == "SEEKER_WIL")
                    {
                        return XmlManager.Load("qualities.xml").SelectSingleNode("/chummer/qualities/quality[name = \"Cyber-Singularity Seeker\"]/translate")?.InnerText ?? "Cyber-Singularity Seeker";
                    }
                    else if (objImprovement.SourceName.StartsWith("SEEKER"))
                    {
                        return XmlManager.Load("qualities.xml").SelectSingleNode("/chummer/qualities/quality[name = \"Redliner\"]/translate")?.InnerText ?? "Redliner";
                    }
                    foreach (Quality objQuality in Qualities)
                    {
                        if (objQuality.InternalId == objImprovement.SourceName)
                        {
                            return objQuality.DisplayNameShort(strLanguage);
                        }
                    }
                    break;
                case Improvement.ImprovementSource.MartialArtTechnique:
                    foreach (MartialArt objMartialArt in MartialArts)
                    {
                        foreach (MartialArtTechnique objAdvantage in objMartialArt.Techniques)
                        {
                            if (objAdvantage.InternalId == objImprovement.SourceName)
                            {
                                return objAdvantage.DisplayName(strLanguage);
                            }
                        }
                    }
                    break;
                case Improvement.ImprovementSource.MentorSpirit:
                    foreach (MentorSpirit objMentorSpirit in MentorSpirits)
                    {
                        if (objMentorSpirit.InternalId == objImprovement.SourceName)
                        {
                            return objMentorSpirit.DisplayNameShort(strLanguage);
                        }
                    }
                    break;
                case Improvement.ImprovementSource.ConditionMonitor:
                    return LanguageManager.GetString("Label_ConditionMonitor", strLanguage);
                case Improvement.ImprovementSource.Heritage:
                    return LanguageManager.GetString("String_Priority", strLanguage);
                case Improvement.ImprovementSource.Initiation:
                    return LanguageManager.GetString("Tab_Initiation", strLanguage);
                case Improvement.ImprovementSource.Submersion:
                    return LanguageManager.GetString("Tab_Submersion", strLanguage);
                default:
                    if (objImprovement.ImproveType == Improvement.ImprovementType.ArmorEncumbrancePenalty)
                        return LanguageManager.GetString("String_ArmorEncumbrance", strLanguage);
                    // If this comes from a custom Improvement, use the name the player gave it instead of showing a GUID.
                    if (!string.IsNullOrEmpty(objImprovement.CustomName))
                        return objImprovement.CustomName;
                    string strReturn = objImprovement.SourceName;
                    if (string.IsNullOrEmpty(strReturn) || strReturn.IsGuid())
                    {
                        string strTemp = LanguageManager.GetString("String_" + objImprovement.ImproveType.ToString(), strLanguage, false);
                        if (!string.IsNullOrEmpty(strTemp))
                            strReturn = strTemp;
                    }
                    return strReturn;
            }
            return string.Empty;
        }

        /// <summary>
        /// Return a list of CyberwareGrades from XML files.
        /// </summary>
        /// <param name="objSource">Source to load the Grades from, either Bioware or Cyberware.</param>
        public IList<Grade> GetGradeList(Improvement.ImprovementSource objSource, bool blnIgnoreBannedGrades = false)
        {
            List<Grade> lstGrades = new List<Grade>();
            string strXPath = Options != null ? "/chummer/grades/grade[(" + Options.BookXPath() + ")]" : "/chummer/grades/grade";
            foreach (XmlNode objNode in XmlManager.Load(objSource == Improvement.ImprovementSource.Bioware ? "bioware.xml" : "cyberware.xml").SelectNodes(strXPath))
            {
                Grade objGrade = new Grade(objSource);
                objGrade.Load(objNode);
                if (IgnoreRules || Created || blnIgnoreBannedGrades || !BannedWareGrades.Any(s => objGrade.Name.Contains(s)))
                    lstGrades.Add(objGrade);
            }

            return lstGrades;
        }

        /// <summary>
        /// Calculate the number of Free Spirit Power Points used.
        /// </summary>
        public string CalculateFreeSpiritPowerPoints()
        {
            string strReturn;

            if (Metatype == "Free Spirit" && !IsCritter)
            {
                // PC Free Spirit.
                decimal decPowerPoints = 0;

                foreach (CritterPower objPower in CritterPowers)
                {
                    if (objPower.CountTowardsLimit)
                        decPowerPoints += objPower.PowerPoints;
                }

                int intPowerPoints = EDG.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.FreeSpiritPowerPoints);

                // If the house rule to base Power Points on the character's MAG value instead, use the character's MAG.
                if (Options.FreeSpiritPowerPointsMAG)
                    intPowerPoints = MAG.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.FreeSpiritPowerPoints);

                strReturn = string.Format("{1} ({0} " + LanguageManager.GetString("String_Remaining", GlobalOptions.Language) + ')', intPowerPoints - decPowerPoints, intPowerPoints);
            }
            else
            {
                int intPowerPoints;

                if (Metatype == "Free Spirit")
                {
                    // Critter Free Spirits have a number of Power Points equal to their EDG plus any Free Spirit Power Points Improvements.
                    intPowerPoints = EDG.Value + ImprovementManager.ValueOf(this, Improvement.ImprovementType.FreeSpiritPowerPoints);
                }
                else if (Metatype == "Ally Spirit")
                {
                    // Ally Spirits get a number of Power Points equal to their MAG.
                    intPowerPoints = MAG.TotalValue;
                }
                else
                {
                    // Spirits get 1 Power Point for every 3 full points of Force (MAG) they possess.
                    intPowerPoints = MAG.TotalValue / 3;
                }

                int intUsed = 0;// _objCharacter.CritterPowers.Count - intExisting;
                foreach (CritterPower objPower in CritterPowers)
                {
                    if (objPower.Category != "Weakness" && objPower.CountTowardsLimit)
                        intUsed += 1;
                }

                strReturn = string.Format("{1} ({0} " + LanguageManager.GetString("String_Remaining", GlobalOptions.Language) + ')', intPowerPoints - intUsed, intPowerPoints);
            }

            return strReturn;
        }

        /// <summary>
        /// Calculate the number of Free Sprite Power Points used.
        /// </summary>
        public string CalculateFreeSpritePowerPoints()
        {
            // Free Sprite Power Points.
            int intUsedPowerPoints = 0;

            foreach (CritterPower objPower in CritterPowers)
            {
                if (objPower.CountTowardsLimit)
                    intUsedPowerPoints += 1;
            }

            int intPowerPoints = EDG.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.FreeSpiritPowerPoints);

            return string.Format("{1} ({0} " + LanguageManager.GetString("String_Remaining", GlobalOptions.Language) + ')', intPowerPoints - intUsedPowerPoints, intPowerPoints);
        }

        /// <summary>
        /// Construct a list of possible places to put a piece of modular cyberware. Names are display names of the given items, values are internalIDs of the given items.
        /// </summary>
        /// <param name="objModularCyberware">Cyberware for which to construct the list.</param>
        /// <returns></returns>
        public IList<ListItem> ConstructModularCyberlimbList(Cyberware objModularCyberware)
        {
            List<ListItem> lstReturn = new List<ListItem>
            {
                new ListItem("None", LanguageManager.GetString("String_None", GlobalOptions.Language))
            };

            foreach (Cyberware objLoopCyberware in Cyberware.GetAllDescendants(x => x.Children))
            {
                // Make sure this has an eligible mount location and it's not the selected piece modular cyberware
                if (objLoopCyberware.HasModularMount == objModularCyberware.PlugsIntoModularMount && objLoopCyberware.Location == objModularCyberware.Location &&
                    objLoopCyberware.Grade.Name == objModularCyberware.Grade.Name && objLoopCyberware != objModularCyberware)
                {
                    // Make sure it's not the place where the mount is already occupied (either by us or something else)
                    if (!objLoopCyberware.Children.Any(x => x.PlugsIntoModularMount == objLoopCyberware.HasModularMount))
                    {
                        string strName = objLoopCyberware.Parent?.DisplayName(GlobalOptions.Language) ?? objLoopCyberware.DisplayName(GlobalOptions.Language);
                        lstReturn.Add(new ListItem(objLoopCyberware.InternalId, strName));
                    }
                }
            }
            foreach (Vehicle objLoopVehicle in Vehicles)
            {
                foreach (VehicleMod objLoopVehicleMod in objLoopVehicle.Mods)
                {
                    foreach (Cyberware objLoopCyberware in objLoopVehicleMod.Cyberware.GetAllDescendants(x => x.Children))
                    {
                        // Make sure this has an eligible mount location and it's not the selected piece modular cyberware
                        if (objLoopCyberware.HasModularMount == objModularCyberware.PlugsIntoModularMount && objLoopCyberware.Location == objModularCyberware.Location &&
                            objLoopCyberware.Grade.Name == objModularCyberware.Grade.Name && objLoopCyberware != objModularCyberware)
                        {
                            // Make sure it's not the place where the mount is already occupied (either by us or something else)
                            if (!objLoopCyberware.Children.Any(x => x.PlugsIntoModularMount == objLoopCyberware.HasModularMount))
                            {
                                string strName = objLoopVehicle.DisplayName(GlobalOptions.Language) + ' ';
                                if (objLoopCyberware.Parent != null)
                                    strName += objLoopCyberware.Parent.DisplayName(GlobalOptions.Language);
                                else
                                    strName += objLoopVehicleMod.DisplayName(GlobalOptions.Language);
                                lstReturn.Add(new ListItem(objLoopCyberware.InternalId, strName));
                            }
                        }
                    }
                }
            }
            return lstReturn;
        }

        public string CalculateKarmaValue(string strLanguage, out int intReturn)
        {
            string strMessage = LanguageManager.GetString("Message_KarmaValue", strLanguage) + '\n';

            intReturn = BuildKarma;
            if (BuildMethod != CharacterBuildMethod.Karma)
            {
                // Subtract extra karma cost of a metatype in priority
                intReturn -= MetatypeBP;
            }
            strMessage += '\n' + LanguageManager.GetString("Label_Base", strLanguage) + ": " + intReturn.ToString() + ' ' + LanguageManager.GetString("String_Karma", strLanguage);

            if (BuildMethod != CharacterBuildMethod.Karma)
            {
                // Zeroed to -10 because that's Human's value at default settings
                int intMetatypeQualitiesValue = -2 * Options.KarmaAttribute;
                // Karma value of all qualities (we're ignoring metatype cost because Point Buy karma costs don't line up with other methods' values)
                foreach (Quality objQuality in Qualities.Where(x => x.OriginSource == QualitySource.Metatype || x.OriginSource == QualitySource.MetatypeRemovable))
                {
                    XmlNode xmlQualityNode = objQuality.GetNode();
                    if (xmlQualityNode?["onlyprioritygiven"] == null)
                    {
                        intMetatypeQualitiesValue += Convert.ToInt32(xmlQualityNode?["karma"]?.InnerText);
                    }
                }
                intReturn += intMetatypeQualitiesValue;

                int intTemp = 0;
                int intAttributesValue = 0;
                // Value from attribute points and raised attribute minimums
                foreach (CharacterAttrib objLoopAttrib in AttributeSection.AttributeList)
                {
                    string strAttributeName = objLoopAttrib.Abbrev;
                    if (strAttributeName == "ESS" || strAttributeName != "MAGAdept" || (IsMysticAdept && Options.MysAdeptSecondMAGAttribute))
                    {
                        int intLoopAttribValue = Math.Max(objLoopAttrib.Base + objLoopAttrib.FreeBase + objLoopAttrib.RawMinimum, objLoopAttrib.TotalMinimum) + objLoopAttrib.AttributeValueModifiers;
                        if (intLoopAttribValue > 1)
                        {
                            intTemp += ((intLoopAttribValue + 1) * intLoopAttribValue / 2 - 1) * Options.KarmaAttribute;
                            if (strAttributeName != "MAG" && strAttributeName != "MAGAdept" && strAttributeName != "RES" && strAttributeName != "DEP")
                            {
                                int intVanillaAttribValue = Math.Max(objLoopAttrib.Base + objLoopAttrib.FreeBase + objLoopAttrib.RawMinimum - objLoopAttrib.MetatypeMinimum + 1, objLoopAttrib.TotalMinimum - objLoopAttrib.MetatypeMinimum + 1) + objLoopAttrib.AttributeValueModifiers;
                                intAttributesValue += ((intVanillaAttribValue + 1) * intVanillaAttribValue / 2 - 1) * Options.KarmaAttribute;
                            }
                            else
                                intAttributesValue += ((intLoopAttribValue + 1) * intLoopAttribValue / 2 - 1) * Options.KarmaAttribute;
                        }
                    }
                }
                if (intTemp - intAttributesValue + intMetatypeQualitiesValue != 0)
                {
                    strMessage += '\n' + LanguageManager.GetString("Label_SumtoTenHeritage", strLanguage) + ' ' + (intTemp - intAttributesValue + intMetatypeQualitiesValue).ToString() + ' ' + LanguageManager.GetString("String_Karma", strLanguage);
                }
                if (intAttributesValue != 0)
                {
                    strMessage += '\n' + LanguageManager.GetString("Label_SumtoTenAttributes", strLanguage) + ' ' + intAttributesValue.ToString() + ' ' + LanguageManager.GetString("String_Karma", strLanguage);
                }
                intReturn += intTemp;

                intTemp = 0;
                // This is where we add in "Talent" qualities like Adept and Technomancer
                foreach (Quality objQuality in Qualities.Where(x => x.OriginSource == QualitySource.Metatype || x.OriginSource == QualitySource.MetatypeRemovable))
                {
                    XmlNode xmlQualityNode = objQuality.GetNode();
                    if (xmlQualityNode?["onlyprioritygiven"] != null)
                    {
                        intTemp += Convert.ToInt32(xmlQualityNode["karma"]?.InnerText);
                    }
                }
                if (intTemp != 0)
                {
                    strMessage += '\n' + LanguageManager.GetString("String_Qualities", strLanguage) + ": " + intTemp.ToString() + ' ' + LanguageManager.GetString("String_Karma", strLanguage);
                    intReturn += intTemp;
                }

                // Value from free spells
                intTemp = SpellLimit * SpellKarmaCost;
                if (intTemp != 0)
                {
                    strMessage += '\n' + LanguageManager.GetString("String_FreeSpells", strLanguage) + ": " + intTemp.ToString() + ' ' + LanguageManager.GetString("String_Karma", strLanguage);
                    intReturn += intTemp;
                }

                // Value from free complex forms
                intTemp = CFPLimit * ComplexFormKarmaCost;
                if (intTemp != 0)
                {
                    strMessage += '\n' + LanguageManager.GetString("String_FreeCFs", strLanguage) + ": " + intTemp.ToString() + ' ' + LanguageManager.GetString("String_Karma", strLanguage);
                    intReturn += intTemp;
                }

                intTemp = 0;
                // Value from skill points
                foreach (Skill objLoopActiveSkill in SkillsSection.Skills)
                {
                    if (!(objLoopActiveSkill.SkillGroupObject?.Base > 0))
                    {
                        int intLoopRating = objLoopActiveSkill.Base;
                        if (intLoopRating > 0)
                        {
                            intTemp += Options.KarmaNewActiveSkill;
                            intTemp += ((intLoopRating + 1) * intLoopRating / 2 - 1) * Options.KarmaImproveActiveSkill;
                            if (BuildMethod == CharacterBuildMethod.LifeModule)
                                intTemp += objLoopActiveSkill.Specializations.Count(x => x.Free) * Options.KarmaSpecialization;
                            else if (!objLoopActiveSkill.BuyWithKarma)
                                intTemp += objLoopActiveSkill.Specializations.Count * Options.KarmaSpecialization;
                        }
                    }
                }
                if (intTemp != 0)
                {
                    strMessage += '\n' + LanguageManager.GetString("String_SkillPoints", strLanguage) + ": " + intTemp.ToString() + ' ' + LanguageManager.GetString("String_Karma", strLanguage);
                    intReturn += intTemp;
                }

                intTemp = 0;
                // Value from skill group points
                foreach (SkillGroup objLoopSkillGroup in SkillsSection.SkillGroups)
                {
                    int intLoopRating = objLoopSkillGroup.Base;
                    if (intLoopRating > 0)
                    {
                        intTemp += Options.KarmaNewSkillGroup;
                        intTemp += ((intLoopRating + 1) * intLoopRating / 2 - 1) * Options.KarmaImproveSkillGroup;
                    }
                }
                if (intTemp != 0)
                {
                    strMessage += '\n' + LanguageManager.GetString("String_SkillGroupPoints", strLanguage) + ": " + intTemp.ToString() + ' ' + LanguageManager.GetString("String_Karma", strLanguage);
                    intReturn += intTemp;
                }

                // Starting Nuyen karma value
                intTemp = decimal.ToInt32(decimal.Ceiling(StartingNuyen / Options.NuyenPerBP));
                if (intTemp != 0)
                {
                    strMessage += '\n' + LanguageManager.GetString("Checkbox_CreatePACKSKit_StartingNuyen", strLanguage) + ": " + intTemp.ToString() + ' ' + LanguageManager.GetString("String_Karma", strLanguage);
                    intReturn += intTemp;
                }
            }

            int intContactPointsValue = ContactPoints * Options.KarmaContact;
            if (intContactPointsValue != 0)
            {
                strMessage += '\n' + LanguageManager.GetString("String_Contacts", strLanguage) + ": " + intContactPointsValue.ToString() + ' ' + LanguageManager.GetString("String_Karma", strLanguage);
                intReturn += intContactPointsValue;
            }

            int intKnowledgePointsValue = 0;
            foreach (KnowledgeSkill objLoopKnowledgeSkill in SkillsSection.KnowledgeSkills)
            {
                int intLoopRating = objLoopKnowledgeSkill.Base;
                if (intLoopRating > 0)
                {
                    intKnowledgePointsValue += Options.KarmaNewKnowledgeSkill;
                    intKnowledgePointsValue += ((intLoopRating + 1) * intLoopRating / 2 - 1) * Options.KarmaImproveKnowledgeSkill;
                    if (BuildMethod == CharacterBuildMethod.LifeModule)
                        intKnowledgePointsValue += objLoopKnowledgeSkill.Specializations.Count(x => x.Free) * Options.KarmaKnowledgeSpecialization;
                    else if (!objLoopKnowledgeSkill.BuyWithKarma)
                        intKnowledgePointsValue += objLoopKnowledgeSkill.Specializations.Count * Options.KarmaKnowledgeSpecialization;
                }
            }
            if (intKnowledgePointsValue != 0)
            {
                strMessage += '\n' + LanguageManager.GetString("Label_KnowledgeSkills", strLanguage) + ": " + intKnowledgePointsValue.ToString() + ' ' + LanguageManager.GetString("String_Karma", strLanguage);
                intReturn += intKnowledgePointsValue;
            }

            strMessage += "\n\n" + LanguageManager.GetString("String_Total", strLanguage) + ": " + intReturn.ToString() + ' ' + LanguageManager.GetString("String_Karma", strLanguage);

            return strMessage;
        }

        /// <summary>
        /// Creates a list of keywords for each category of an XML node. Used to preselect whether items of that category are discounted by the Black Market Pipeline quality.
        /// </summary>
        public HashSet<string> GenerateBlackMarketMappings(XmlDocument xmlCategoryDocument)
        {
            HashSet<string> setBlackMarketMaps = new HashSet<string>();
            // Character has no Black Market discount qualities. Fail out early. 
            if (BlackMarketDiscount)
            {
                // Get all the improved names of the Black Market Pipeline improvements. In most cases this should only be 1 item, but supports custom content.
                HashSet<string> setNames = new HashSet<string>();
                foreach (Improvement objImprovement in Improvements)
                {
                    if (objImprovement.ImproveType == Improvement.ImprovementType.BlackMarketDiscount && objImprovement.Enabled)
                        setNames.Add(objImprovement.ImprovedName);
                }
                // For each category node, split the comma-separated blackmarket attribute (if present on the node), then add each category where any of those items matches a Black Market Pipeline improvement. 
                foreach (XmlNode xmlCategoryNode in xmlCategoryDocument.SelectNodes("/chummer/categories/category"))
                {
                    string strBlackMarketAttribute = xmlCategoryNode.Attributes?["blackmarket"]?.InnerText;
                    if (!string.IsNullOrEmpty(strBlackMarketAttribute) && strBlackMarketAttribute.Split(',').Any(x => setNames.Contains(x)))
                    {
                        setBlackMarketMaps.Add(xmlCategoryNode.InnerText);
                    }
                }
            }
            return setBlackMarketMaps;
        }
#endregion

#region UI Methods

        /// <summary>
        /// Verify that the user wants to delete an item.
        /// </summary>
        public bool ConfirmDelete(string strMessage)
        {
            return !Options.ConfirmDelete ||
                   MessageBox.Show(strMessage, LanguageManager.GetString("MessageTitle_Delete", GlobalOptions.Language),
                       MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        /// <summary>
        /// Verify that the user wants to spend their Karma and did not accidentally click the button.
        /// </summary>
        public bool ConfirmKarmaExpense(string strMessage)
        {
            if (Options.ConfirmKarmaExpense &&
                MessageBox.Show(strMessage, LanguageManager.GetString("MessageTitle_ConfirmKarmaExpense", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return false;
            else
                return true;
        }

#region Tab clearing
        /// <summary>
        /// Clear all Spell tab elements from the character.
        /// </summary>
        /// <param name="treSpells"></param>
        public void ClearMagic()
        {
            // Run through all of the Spells and remove their Improvements.
            for (int i = Spells.Count - 1; i >= 0; --i)
            {
                if (i < Spells.Count)
                {
                    Spell objToRemove = Spells[i];
                    if (objToRemove.Grade == 0)
                    {
                        // Remove the Improvements created by the Spell.
                        ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.Spell, objToRemove.InternalId);
                        Spells.RemoveAt(i);
                    }
                }
            }
            for (int i = Spirits.Count - 1; i >= 0; --i)
            {
                if (i < Spirits.Count)
                {
                    Spirit objToRemove = Spirits[i];
                    if (objToRemove.EntityType == SpiritType.Spirit)
                    {
                        Spirits.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// Clear all Adept tab elements from the character.
        /// </summary>
        public void ClearAdeptPowers()
        {
            // Run through all powers and remove the ones not added by improvements or foci
            for (int i = Powers.Count - 1; i >= 0; --i)
            {
                if (i < Powers.Count)
                {
                    Power objToRemove = Powers[i];
                    if (objToRemove.FreeLevels == 0 && objToRemove.FreePoints == 0)
                    {
                        // Remove the Improvements created by the Power.
                        ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.Power, objToRemove.InternalId);
                        Powers.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// Clear all Technomancer tab elements from the character.
        /// </summary>
        public void ClearResonance()
        {
            // Run through all of the Complex Forms and remove their Improvements.
            for (int i = ComplexForms.Count - 1; i >= 0; --i)
            {
                if (i < ComplexForms.Count)
                {
                    ComplexForm objToRemove = ComplexForms[i];
                    if (objToRemove.Grade == 0)
                    {
                        // Remove the Improvements created by the Spell.
                        ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.ComplexForm, objToRemove.InternalId);
                        ComplexForms.RemoveAt(i);
                    }
                }
            }

            for (int i = Spirits.Count - 1; i >= 0; --i)
            {
                if (i < Spirits.Count)
                {
                    Spirit objToRemove = Spirits[i];
                    if (objToRemove.EntityType == SpiritType.Sprite)
                    {
                        Spirits.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// Clear all Advanced Programs tab elements from the character.
        /// </summary>
        public void ClearAdvancedPrograms()
        {
            // Run through all advanced programs and remove the ones not added by improvements
            for (int i = AIPrograms.Count - 1; i >= 0; --i)
            {
                if (i < AIPrograms.Count)
                {
                    AIProgram objToRemove = AIPrograms[i];
                    if (objToRemove.CanDelete)
                    {
                        // Remove the Improvements created by the Program.
                        ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.AIProgram, objToRemove.InternalId);
                        AIPrograms.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// Clear all Cyberware tab elements from the character.
        /// </summary>
        public void ClearCyberwareTab(TreeView treWeapons, TreeView treVehicles)
        {
            for (int i = Cyberware.Count - 1; i >= 0; i--)
            {
                if (i < Cyberware.Count)
                {
                    Cyberware objToRemove = Cyberware[i];
                    if (string.IsNullOrEmpty(objToRemove.ParentID))
                    {
                        objToRemove.DeleteCyberware(treWeapons, treVehicles);
                        Cyberware.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// Clear all Critter tab elements from the character.
        /// </summary>
        public void ClearCritterPowers()
        {
            for (int i = CritterPowers.Count - 1; i >= 0; i--)
            {
                if (i < CritterPowers.Count)
                {
                    CritterPower objToRemove = CritterPowers[i];
                    if (objToRemove.Grade >= 0)
                    {
                        // Remove the Improvements created by the Metamagic.
                        ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.CritterPower, objToRemove.InternalId);
                        CritterPowers.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// Clear all Initiation tab elements from the character that were not added by improvements.
        /// </summary>
        public void ClearInitiations()
        {
            InitiateGrade = 0;
            SubmersionGrade = 0;
            InitiationGrades.Clear();
            // Metamagics/Echoes can add addition bonus metamagics/echoes, so we cannot use foreach or RemoveAll()
            for (int i = Metamagics.Count - 1; i >= 0; i--)
            {
                if (i < Metamagics.Count)
                {
                    Metamagic objToRemove = Metamagics[i];
                    if (objToRemove.Grade >= 0)
                    {
                        // Remove the Improvements created by the Metamagic.
                        ImprovementManager.RemoveImprovements(this, objToRemove.SourceType, objToRemove.InternalId);
                        Metamagics.RemoveAt(i);
                    }
                }
            }
        }
#endregion

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
        /// Name of the file the Character is saved to.
        /// </summary>
        public DateTime FileLastWriteTime
        {
            get
            {
                return _dateFileLastWriteTime;
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
                if (_strSettingsFileName != value)
                {
                    _strSettingsFileName = value;
                    _objOptions.Load(_strSettingsFileName);
                }
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
                if (_strName != value)
                {
                    _strName = value;
                    if (CharacterNameChanged != null && string.IsNullOrWhiteSpace(Alias))
                        CharacterNameChanged.Invoke(this);
                }
            }
        }

		/// <summary>
		/// Character's portraits encoded using Base64.
		/// </summary>
		public IList<Image> Mugshots
        {
            get
            {
                return _lstMugshots;
            }
        }

        /// <summary>
        /// Character's main portrait encoded using Base64.
        /// </summary>
        public Image MainMugshot
        {
            get
            {
                if (MainMugshotIndex >= Mugshots.Count || MainMugshotIndex < 0)
                    return null;
                else
                    return Mugshots[MainMugshotIndex];
            }
            set
            {
                if (value == null)
                {
                    MainMugshotIndex = -1;
                    return;
                }
                int intNewMainMugshotIndex = Mugshots.IndexOf(value);
                if (intNewMainMugshotIndex != -1)
                {
                    MainMugshotIndex = intNewMainMugshotIndex;
                }
                else
                {
                    Mugshots.Add(value);
                    MainMugshotIndex = Mugshots.Count - 1;
                }
            }
        }

        /// <summary>
        /// Index of Character's main portrait. -1 if set to none.
        /// </summary>
        public int MainMugshotIndex
        {
            get
            {
                return _intMainMugshotIndex;
            }
            set
            {
                if (value >= _lstMugshots.Count || value < -1)
                    _intMainMugshotIndex = -1;
                else
                    _intMainMugshotIndex = value;
            }
        }

        public void SaveMugshots(XmlTextWriter objWriter)
        {
            objWriter.WriteElementString("mainmugshotindex", MainMugshotIndex.ToString());
            // <mugshot>
            objWriter.WriteStartElement("mugshots");
            foreach (Image imgMugshot in Mugshots)
            {
                objWriter.WriteElementString("mugshot", imgMugshot.ToBase64String());
            }
            // </mugshot>
            objWriter.WriteEndElement();
        }

        public void LoadMugshots(XmlNode xmlSavedNode)
        {
            // Mugshots
            xmlSavedNode.TryGetInt32FieldQuickly("mainmugshotindex", ref _intMainMugshotIndex);
            XmlNodeList objXmlMugshotsList = xmlSavedNode.SelectNodes("mugshots/mugshot");
            if (objXmlMugshotsList != null)
            {
                List<string> lstMugshotsBase64 = new List<string>(objXmlMugshotsList.Count);
                foreach (XmlNode objXmlMugshot in objXmlMugshotsList)
                {
                    string strMugshot = objXmlMugshot.InnerText;
                    if (!string.IsNullOrWhiteSpace(strMugshot))
                    {
                        lstMugshotsBase64.Add(strMugshot);
                    }
                }
                if (lstMugshotsBase64.Count > 1)
                {
                    Image[] objMugshotImages = new Image[lstMugshotsBase64.Count];
                    Parallel.For(0, lstMugshotsBase64.Count, i =>
                    {
                        objMugshotImages[i] = lstMugshotsBase64[i].ToImage(System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                    });
                    _lstMugshots.AddRange(objMugshotImages);
                }
                else if (lstMugshotsBase64.Count == 1)
                {
                    _lstMugshots.Add(lstMugshotsBase64[0].ToImage(System.Drawing.Imaging.PixelFormat.Format32bppPArgb));
                }
            }
            // Legacy Shimmer
            if (Mugshots.Count == 0)
            {
                XmlNode objOldMugshotNode = xmlSavedNode.SelectSingleNode("mugshot");
                if (objOldMugshotNode != null)
                {
                    string strMugshot = objOldMugshotNode.InnerText;
                    if (!string.IsNullOrWhiteSpace(strMugshot))
                    {
                        _lstMugshots.Add(strMugshot.ToImage(System.Drawing.Imaging.PixelFormat.Format32bppPArgb));
                        _intMainMugshotIndex = 0;
                    }
                }
            }
        }

        public void PrintMugshots(XmlTextWriter objWriter)
        {
            if (Mugshots.Count > 0)
            {
                // Since IE is retarded and can't handle base64 images before IE9, we need to dump the image to a temporary directory and re-write the information.
                // If you give it an extension of jpg, gif, or png, it expects the file to be in that format and won't render the image unless it was originally that type.
                // But if you give it the extension img, it will render whatever you give it (which doesn't make any damn sense, but that's IE for you).
                string strMugshotsDirectoryPath = Path.Combine(Application.StartupPath, "mugshots");
                if (!Directory.Exists(strMugshotsDirectoryPath))
                {
                    try
                    {
                        Directory.CreateDirectory(strMugshotsDirectoryPath);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        MessageBox.Show(LanguageManager.GetString("Message_Insufficient_Permissions_Warning", GlobalOptions.Language));
                    }
                }
                Guid guiImage = Guid.NewGuid();
                string imgMugshotPath = Path.Combine(strMugshotsDirectoryPath, guiImage.ToString("N") + ".img");
                Image imgMainMugshot = MainMugshot;
                if (imgMainMugshot != null)
                {
                    imgMainMugshot.Save(imgMugshotPath);
                    // <mainmugshotpath />
                    objWriter.WriteElementString("mainmugshotpath", "file://" + imgMugshotPath.Replace(Path.DirectorySeparatorChar, '/'));
                    // <mainmugshotbase64 />
                    objWriter.WriteElementString("mainmugshotbase64", imgMainMugshot.ToBase64String());
                }
                // <othermugshots>
                objWriter.WriteElementString("hasothermugshots", (imgMainMugshot == null || Mugshots.Count > 1).ToString());
                objWriter.WriteStartElement("othermugshots");
                for (int i = 0; i < Mugshots.Count; ++i)
                {
                    if (i == MainMugshotIndex)
                        continue;
                    Image imgMugshot = Mugshots[i];
                    objWriter.WriteStartElement("mugshot");

                    objWriter.WriteElementString("stringbase64", imgMugshot.ToBase64String());

                    imgMugshotPath = Path.Combine(strMugshotsDirectoryPath, guiImage.ToString("N") + i.ToString() + ".img");
                    imgMugshot.Save(imgMugshotPath);
                    objWriter.WriteElementString("temppath", "file://" + imgMugshotPath.Replace(Path.DirectorySeparatorChar, '/'));

                    objWriter.WriteEndElement();
                }
                // </mugshots>
                objWriter.WriteEndElement();
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
        public IList<string> PriorityBonusSkillList
        {
            get
            {
                return _lstPrioritySkills;
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
                if (_strAlias != value)
                {
                    _strAlias = value;
                    CharacterNameChanged?.Invoke(this);
                }
            }
        }
        
        /// <summary>
        /// Character's name to use when loading them in a new tab.
        /// </summary>
        public string CharacterName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Alias))
                    return Alias;
                if (!string.IsNullOrWhiteSpace(Name))
                    return Name;
                return LanguageManager.GetString("String_UnnamedCharacter", GlobalOptions.Language);
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
                if (OnPropertyChanged(ref _intKarma, value))
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanAffordCareerPP)));
            }
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
                        intKarma += decimal.ToInt32(objEntry.Amount);
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

        public int SpellKarmaCost
        {
            get
            {
                int intReturn = Options.KarmaSpell;

                decimal decMultiplier = 1.0m;
                foreach (Improvement objLoopImprovement in Improvements)
                {
                    if (objLoopImprovement.Enabled && (string.IsNullOrEmpty(objLoopImprovement.Condition) || (objLoopImprovement.Condition == "career") == Created || (objLoopImprovement.Condition == "create") != Created))
                    {
                        if (objLoopImprovement.ImproveType == Improvement.ImprovementType.NewSpellKarmaCost)
                            intReturn += objLoopImprovement.Value;
                        if (objLoopImprovement.ImproveType == Improvement.ImprovementType.NewSpellKarmaCostMultiplier)
                            decMultiplier *= objLoopImprovement.Value / 100.0m;
                    }
                }
                if (decMultiplier != 1.0m)
                    intReturn = decimal.ToInt32(decimal.Ceiling(intReturn * decMultiplier));

                return Math.Max(intReturn, 0);
            }
        }

        public int ComplexFormKarmaCost
        {
            get
            {
                int intReturn = Options.KarmaNewComplexForm;

                decimal decMultiplier = 1.0m;
                foreach (Improvement objLoopImprovement in Improvements)
                {
                    if (objLoopImprovement.Enabled && (string.IsNullOrEmpty(objLoopImprovement.Condition) || (objLoopImprovement.Condition == "career") == Created || (objLoopImprovement.Condition == "create") != Created))
                    {
                        if (objLoopImprovement.ImproveType == Improvement.ImprovementType.NewComplexFormKarmaCost)
                            intReturn += objLoopImprovement.Value;
                        if (objLoopImprovement.ImproveType == Improvement.ImprovementType.NewComplexFormKarmaCostMultiplier)
                            decMultiplier *= objLoopImprovement.Value / 100.0m;
                    }
                }
                if (decMultiplier != 1.0m)
                    intReturn = decimal.ToInt32(decimal.Ceiling(intReturn * decMultiplier));

                return Math.Max(intReturn, 0);
            }
        }

        public int AIProgramKarmaCost
        {
            get
            {
                int intReturn = Options.KarmaNewAIProgram;

                decimal decMultiplier = 1.0m;
                foreach (Improvement objLoopImprovement in Improvements)
                {
                    if (objLoopImprovement.Enabled && (string.IsNullOrEmpty(objLoopImprovement.Condition) || (objLoopImprovement.Condition == "career") == Created || (objLoopImprovement.Condition == "create") != Created))
                    {
                        if (objLoopImprovement.ImproveType == Improvement.ImprovementType.NewAIProgramKarmaCost)
                            intReturn += objLoopImprovement.Value;
                        if (objLoopImprovement.ImproveType == Improvement.ImprovementType.NewAIProgramKarmaCostMultiplier)
                            decMultiplier *= objLoopImprovement.Value / 100.0m;
                    }
                }
                if (decMultiplier != 1.0m)
                    intReturn = decimal.ToInt32(decimal.Ceiling(intReturn * decMultiplier));

                return Math.Max(intReturn, 0);
            }
        }

        public int AIAdvancedProgramKarmaCost
        {
            get
            {
                int intReturn = Options.KarmaNewAIAdvancedProgram;
                
                decimal decMultiplier = 1.0m;
                foreach (Improvement objLoopImprovement in Improvements)
                {
                    if (objLoopImprovement.Enabled && (string.IsNullOrEmpty(objLoopImprovement.Condition) || (objLoopImprovement.Condition == "career") == Created || (objLoopImprovement.Condition == "create") != Created))
                    {
                        if (objLoopImprovement.ImproveType == Improvement.ImprovementType.NewAIAdvancedProgramKarmaCost)
                            intReturn += objLoopImprovement.Value;
                        if (objLoopImprovement.ImproveType == Improvement.ImprovementType.NewAIAdvancedProgramKarmaCostMultiplier)
                            decMultiplier *= objLoopImprovement.Value / 100.0m;
                    }
                }
                if (decMultiplier != 1.0m)
                    intReturn = decimal.ToInt32(decimal.Ceiling(intReturn * decMultiplier));

                return Math.Max(intReturn, 0);
            }
        }


        public bool Ambidextrous
        {
            get
            {
                return _blnAmbidextrous;
            }
            set
            {
                if (_blnAmbidextrous != value)
                {
                    _blnAmbidextrous = value;
                    AmbidextrousChanged?.Invoke(this);
                }
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
            if (strAttribute == "MAGAdept" && (!IsMysticAdept || !Options.MysAdeptSecondMAGAttribute))
                strAttribute = "MAG";
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
        /// Magic (MAG) CharacterAttribute for Adept powers of Mystic Adepts when the appropriate house rule is enabled.
        /// </summary>
        public CharacterAttrib MAGAdept
        {
            get
            {
                if (Options.MysAdeptSecondMAGAttribute && IsMysticAdept)
                    return AttributeSection.GetAttributeByName("MAGAdept");
                else
                    return MAG;
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
        /// Maximum force of spirits summonable/bindable by the character
        /// </summary>
        public int MaxSpiritForce
        {
            get
            {
                return 2 * (Options.SpiritForceBasedOnTotalMAG ? MAG.TotalValue : MAG.Value);
            }
        }

        /// <summary>
        /// Maximum level of sprites compilable/registerable by the character
        /// </summary>
        public int MaxSpriteLevel
        {
            get
            {
                return 2 * RES.TotalValue;
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
                ImprovementManager.ClearCachedValue(new Tuple<Character, Improvement.ImprovementType>(this, Improvement.ImprovementType.MatrixInitiativeDice));
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

        private decimal _decCachedEssence = decimal.MinValue;
        public void ResetCachedEssence()
        {
            _decCachedEssence = decimal.MinValue;
        }
        /// <summary>
        /// Character's Essence.
        /// </summary>
        public decimal Essence
        {
            get
            {
                if (_decCachedEssence != decimal.MinValue)
                    return _decCachedEssence;
                // If the character has a fixed Essence Improvement, permanently fix their Essence at its value.
                if (_lstImprovements.Any(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.CyborgEssence && objImprovement.Enabled))
                {
                    return _decCachedEssence = 0.1m;
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
                //ESS.Base = Convert.ToInt32(decESS); -- Disabled becauses this messes up Character Validity, and it really shouldn't be what "Base" of an attribute is supposed to be (it's supposed to be extra levels gained)

                return _decCachedEssence = decESS;
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
                return _lstCyberware.Where(objCyberware => objCyberware.Name != "Essence Hole" && objCyberware.SourceType == Improvement.ImprovementSource.Cyberware).AsParallel().Sum(objCyberware => objCyberware.CalculatedESS());
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
                return _lstCyberware.Where(objCyberware => objCyberware.Name != "Essence Hole" && objCyberware.SourceType == Improvement.ImprovementSource.Bioware).AsParallel().Sum(objCyberware => objCyberware.CalculatedESS());
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
                return _lstCyberware.Where(objCyberware => objCyberware.Name == "Essence Hole").AsParallel().Sum(objCyberware => objCyberware.CalculatedESS());
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
                return decimal.ToInt32(decimal.Ceiling(EssenceAtSpecialStart + Convert.ToDecimal(ImprovementManager.ValueOf(this, Improvement.ImprovementType.EssenceMax), GlobalOptions.InvariantCultureInfo) - Essence));
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
                return decimal.ToInt32(decimal.Ceiling(EssenceAtSpecialStart + Convert.ToDecimal(ImprovementManager.ValueOf(this, Improvement.ImprovementType.EssenceMax), GlobalOptions.InvariantCultureInfo) - Essence - (Convert.ToDecimal(ImprovementManager.ValueOf(this, Improvement.ImprovementType.EssencePenaltyMAGOnlyT100), GlobalOptions.InvariantCultureInfo) / 100.0m)));
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
                return LanguageManager.GetString("String_Initiative", GlobalOptions.Language)
                    .Replace("{0}", InitiativeValue.ToString())
                    .Replace("{1}", InitiativeDice.ToString());
            }
        }

        public string GetInitiative(CultureInfo objCulture, string strLanguage)
        {
            return LanguageManager.GetString("String_Initiative", strLanguage)
                    .Replace("{0}", InitiativeValue.ToString(objCulture))
                    .Replace("{1}", InitiativeDice.ToString(objCulture));
        }

        /// <summary>
        /// Initiative Dice.
        /// </summary>
        public int InitiativeDice
        {
            get
            {
                int intExtraIP = 1 + ImprovementManager.ValueOf(this, Improvement.ImprovementType.InitiativeDice) + ImprovementManager.ValueOf(this, Improvement.ImprovementType.InitiativeDiceAdd);

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
                return LanguageManager.GetString("String_Initiative", GlobalOptions.Language)
                    .Replace("{0}", AstralInitiativeValue.ToString())
                    .Replace("{1}", AstralInitiativeDice.ToString());
            }
        }

        public string GetAstralInitiative(CultureInfo objCulture, string strLanguageToPrint)
        {
            return LanguageManager.GetString("String_Initiative", strLanguageToPrint)
                    .Replace("{0}", AstralInitiativeValue.ToString(objCulture))
                    .Replace("{1}", AstralInitiativeDice.ToString(objCulture));
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
                return LanguageManager.GetString("String_Initiative", GlobalOptions.Language)
                        .Replace("{0}", MatrixInitiativeValue.ToString())
                        .Replace("{1}", MatrixInitiativeDice.ToString());
            }
        }

        public string GetMatrixInitiative(CultureInfo objCulture, string strLanguageToPrint)
        {
            return LanguageManager.GetString("String_Initiative", strLanguageToPrint)
                        .Replace("{0}", MatrixInitiativeValue.ToString(objCulture))
                        .Replace("{1}", MatrixInitiativeDice.ToString(objCulture));
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
                    if (HomeNode != null)
                    {
                        int intHomeNodeDP = HomeNode.GetTotalMatrixAttribute("Data Processing");
                        if (HomeNode is Vehicle objHomeNodeVehicle)
                        {
                            int intHomeNodePilot = objHomeNodeVehicle.Pilot;
                            if (intHomeNodePilot > intHomeNodeDP)
                                intHomeNodeDP = intHomeNodePilot;
                        }
                        intINI += intHomeNodeDP;
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
                return LanguageManager.GetString(ActiveCommlink == null ? "String_MatrixInitiative" : "String_Initiative", GlobalOptions.Language)
                        .Replace("{0}", MatrixInitiativeColdValue.ToString())
                        .Replace("{1}", MatrixInitiativeColdDice.ToString());
            }
        }

        public string GetMatrixInitiativeCold(CultureInfo objCulture, string strLanguageToPrint)
        {
            if (_strMetatype == "A.I.")
            {
                return GetMatrixInitiative(objCulture, strLanguageToPrint);
            }
            return LanguageManager.GetString(ActiveCommlink == null ? "String_MatrixInitiative" : "String_Initiative", strLanguageToPrint)
                    .Replace("{0}", MatrixInitiativeColdValue.ToString(objCulture))
                    .Replace("{1}", MatrixInitiativeColdDice.ToString(objCulture));
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
                int intCommlinkDP = ActiveCommlink?.GetTotalMatrixAttribute("Data Processing") ?? 0;
                return INT.TotalValue + intCommlinkDP + WoundModifiers + ImprovementManager.ValueOf(this, Improvement.ImprovementType.MatrixInitiative);
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
                    LanguageManager.GetString(ActiveCommlink == null ? "String_MatrixInitiative" : "String_Initiative", GlobalOptions.Language)
                        .Replace("{0}", MatrixInitiativeHotValue.ToString())
                        .Replace("{1}", MatrixInitiativeHotDice.ToString());
            }
        }

        public string GetMatrixInitiativeHot(CultureInfo objCulture, string strLanguageToPrint)
        {
            if (_strMetatype == "A.I.")
            {
                return GetMatrixInitiative(objCulture, strLanguageToPrint);
            }
            return
                LanguageManager.GetString(ActiveCommlink == null ? "String_MatrixInitiative" : "String_Initiative", strLanguageToPrint)
                    .Replace("{0}", MatrixInitiativeHotValue.ToString(objCulture))
                    .Replace("{1}", MatrixInitiativeHotDice.ToString(objCulture));
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
                int intCommlinkDP = ActiveCommlink?.GetTotalMatrixAttribute("Data Processing") ?? 0;
                return INT.TotalValue + intCommlinkDP + WoundModifiers + ImprovementManager.ValueOf(this, Improvement.ImprovementType.MatrixInitiative); ;
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
        public string ToxinContactResist(string strLanguage, CultureInfo objCulture)
        {
            if (Improvements.Any(x => x.Enabled && x.ImproveType == Improvement.ImprovementType.ToxinContactImmune))
                return LanguageManager.GetString("String_Immune", strLanguage);
            else
                return (BOD.TotalValue + WIL.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.ToxinContactResist)).ToString(objCulture);
        }
        /// <summary>
        /// Resist test to Ingestion-vector Toxins (BOD + WIL).
        /// </summary>
        public string ToxinIngestionResist(string strLanguage, CultureInfo objCulture)
        {
            if (Improvements.Any(x => x.Enabled && x.ImproveType == Improvement.ImprovementType.ToxinIngestionImmune))
                return LanguageManager.GetString("String_Immune", strLanguage);
            else
                return (BOD.TotalValue + WIL.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.ToxinIngestionResist)).ToString(objCulture);
        }
        /// <summary>
        /// Resist test to Inhalation-vector Toxins (BOD + WIL).
        /// </summary>
        public string ToxinInhalationResist(string strLanguage, CultureInfo objCulture)
        {
            if (Improvements.Any(x => x.Enabled && x.ImproveType == Improvement.ImprovementType.ToxinInhalationImmune))
                return LanguageManager.GetString("String_Immune", strLanguage);
            else
                return (BOD.TotalValue + WIL.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.ToxinInhalationResist)).ToString(objCulture);
        }
        /// <summary>
        /// Resist test to Injection-vector Toxins (BOD + WIL).
        /// </summary>
        public string ToxinInjectionResist(string strLanguage, CultureInfo objCulture)
        {
            if (Improvements.Any(x => x.Enabled && x.ImproveType == Improvement.ImprovementType.ToxinInjectionImmune))
                return LanguageManager.GetString("String_Immune", strLanguage);
            else
                return (BOD.TotalValue + WIL.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.ToxinInjectionResist)).ToString(objCulture);
        }

        /// <summary>
        /// Resist test to Contact-vector Pathogens (BOD + WIL).
        /// </summary>
        public string PathogenContactResist(string strLanguage, CultureInfo objCulture)
        {
            if (Improvements.Any(x => x.Enabled && x.ImproveType == Improvement.ImprovementType.PathogenContactImmune))
                return LanguageManager.GetString("String_Immune", strLanguage);
            else
                return (BOD.TotalValue + WIL.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.PathogenContactResist)).ToString(objCulture);
        }
        /// <summary>
        /// Resist test to Ingestion-vector Pathogens (BOD + WIL).
        /// </summary>
        public string PathogenIngestionResist(string strLanguage, CultureInfo objCulture)
        {
            if (Improvements.Any(x => x.Enabled && x.ImproveType == Improvement.ImprovementType.PathogenIngestionImmune))
                return LanguageManager.GetString("String_Immune", strLanguage);
            else
                return (BOD.TotalValue + WIL.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.PathogenIngestionResist)).ToString(objCulture);
        }
        /// <summary>
        /// Resist test to Inhalation-vector Pathogens (BOD + WIL).
        /// </summary>
        public string PathogenInhalationResist(string strLanguage, CultureInfo objCulture)
        {
            if (Improvements.Any(x => x.Enabled && x.ImproveType == Improvement.ImprovementType.PathogenInhalationImmune))
                return LanguageManager.GetString("String_Immune", strLanguage);
            else
                return (BOD.TotalValue + WIL.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.PathogenInhalationResist)).ToString(objCulture);
        }
        /// <summary>
        /// Resist test to Injection-vector Pathogens (BOD + WIL).
        /// </summary>
        public string PathogenInjectionResist(string strLanguage, CultureInfo objCulture)
        {
            if (Improvements.Any(x => x.Enabled && x.ImproveType == Improvement.ImprovementType.PathogenInjectionImmune))
                return LanguageManager.GetString("String_Immune", strLanguage);
            else
                return (BOD.TotalValue + WIL.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.PathogenInjectionResist)).ToString(objCulture);
        }

        /// <summary>
        /// Resist test to Physiological Addiction (BOD + WIL) if you are not addicted yet.
        /// </summary>
        public int PhysiologicalAddictionResistFirstTime
        {
            get
            {
                return BOD.TotalValue + WIL.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.PhysiologicalAddictionFirstTime);
            }
        }

        /// <summary>
        /// Resist test to Psychological Addiction (LOG + WIL) if you are not addicted yet.
        /// </summary>
        public int PsychologicalAddictionResistFirstTime
        {
            get
            {
                return LOG.TotalValue + WIL.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.PsychologicalAddictionFirstTime);
            }
        }

        /// <summary>
        /// Resist test to Physiological Addiction (BOD + WIL) if you are already addicted.
        /// </summary>
        public int PhysiologicalAddictionResistAlreadyAddicted
        {
            get
            {
                return BOD.TotalValue + WIL.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.PhysiologicalAddictionAlreadyAddicted);
            }
        }

        /// <summary>
        /// Resist test to Psychological Addiction (LOG + WIL) if you are already addicted.
        /// </summary>
        public int PsychologicalAddictionResistAlreadyAddicted
        {
            get
            {
                return LOG.TotalValue + WIL.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.PsychologicalAddictionAlreadyAddicted);
            }
        }

        /// <summary>
        /// Dicepool for natural recovery from Stun CM box damage (BOD + WIL).
        /// </summary>
        public int StunCMNaturalRecovery
        {
            get
            {
                int intReturn = BOD.TotalValue + WIL.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.StunCMRecovery);
                if (Improvements.Any(x => x.Enabled && x.ImproveType == Improvement.ImprovementType.AddESStoStunCMRecovery))
                    intReturn += decimal.ToInt32(decimal.Floor(Essence));
                return intReturn;
            }
        }

        /// <summary>
        /// Dicepool for natural recovery from Physical CM box damage (2 x BOD).
        /// </summary>
        public int PhysicalCMNaturalRecovery
        {
            get
            {
                int intReturn = 2 * BOD.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.PhysicalCMRecovery);
                if (Improvements.Any(x => x.Enabled && x.ImproveType == Improvement.ImprovementType.AddESStoPhysicalCMRecovery))
                    intReturn += decimal.ToInt32(decimal.Floor(Essence));
                return intReturn;
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
                int intReturn = CareerKarma / (10 + ImprovementManager.ValueOf(this, Improvement.ImprovementType.StreetCredMultiplier));

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
                return Math.Max(CalculatedStreetCred + StreetCred + ImprovementManager.ValueOf(this, Improvement.ImprovementType.StreetCred), 0);
            }
        }

        /// <summary>
        /// Street Cred Tooltip.
        /// </summary>
        public string StreetCredTooltip
        {
            get
            {
                string strReturn = $"({LanguageManager.GetString("String_CareerKarma", GlobalOptions.Language)} ÷ {10 + ImprovementManager.ValueOf(this, Improvement.ImprovementType.StreetCredMultiplier)})";
                if (BurntStreetCred != 0)
                    strReturn += $" - {LanguageManager.GetString("String_BurntStreetCred", GlobalOptions.Language)}";

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
                int intReturn = ImprovementManager.ValueOf(this, Improvement.ImprovementType.Notoriety);// + Contacts.Count(x => x.EntityType == ContactType.Enemy);

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
                StringBuilder objReturn = new StringBuilder();

                foreach (Improvement objImprovement in _lstImprovements)
                {
                    if (objImprovement.ImproveType == Improvement.ImprovementType.Notoriety)
                        objReturn.Append(" + " + GetObjectName(objImprovement, GlobalOptions.Language) + " (" + objImprovement.Value.ToString() + ')');
                }

                int intEnemies = Contacts.Count(x => x.EntityType == ContactType.Enemy);
                if (intEnemies > 0)
                    objReturn.Append(" + " + LanguageManager.GetString("Label_SummaryEnemies", GlobalOptions.Language) + " (" + intEnemies.ToString() + ')');

                if (BurntStreetCred > 0)
                    objReturn.Append(" - " + LanguageManager.GetString("String_BurntStreetCred", GlobalOptions.Language) + " (" + (BurntStreetCred / 2).ToString() + ')');

                string strReturn = objReturn.ToString();

                if (!string.IsNullOrEmpty(strReturn))
                    strReturn = strReturn.Substring(3);

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
                int intReturn = ImprovementManager.ValueOf(this, Improvement.ImprovementType.PublicAwareness);
                if (_objOptions.UseCalculatedPublicAwareness)
                {
                    // Public Awareness is calculated as (Street Cred + Notoriety) / 3, rounded down.
                    intReturn += (TotalStreetCred + TotalNotoriety) / 3;
                }

                return intReturn;
            }
        }

        /// <summary>
        /// Character's total amount of Public Awareness (earned + GM awarded).
        /// </summary>
        public int TotalPublicAwareness
        {
            get
            {
                int intReturn = PublicAwareness;
                if (Erased && intReturn >= 1)
                    return 1;
                return intReturn;
            }
        }

        /// <summary>
        /// Public Awareness Tooltip.
        /// </summary>
        public string PublicAwarenessTooltip
        {
            get
            {
                if (_objOptions.UseCalculatedPublicAwareness)
                {
                    return "(" + LanguageManager.GetString("String_StreetCred", GlobalOptions.Language) + " (" + TotalStreetCred.ToString() + ") + " + LanguageManager.GetString("String_Notoriety", GlobalOptions.Language) + " (" + TotalNotoriety.ToString() + ")) ÷ 3";
                }

                return string.Empty;
            }
        }
#endregion

#region List Properties
        /// <summary>
        /// Improvements.
        /// </summary>
        public ObservableCollection<Improvement> Improvements
        {
            get
            {
                return _lstImprovements;
            }
        }

        /// <summary>
        /// Gear.
        /// </summary>
        public IList<MentorSpirit> MentorSpirits
        {
            get
            {
                return _lstMentorSpirits;
            }
        }

        /// <summary>
        /// Contacts and Enemies.
        /// </summary>
        public ObservableCollection<Contact> Contacts
        {
            get
            {
                return _lstContacts;
            }
        }

        /// <summary>
        /// Spirits and Sprites.
        /// </summary>
        public ObservableCollection<Spirit> Spirits
        {
            get
            {
                return _lstSpirits;
            }
        }

        /// <summary>
        /// Magician Spells.
        /// </summary>
        public ObservableCollection<Spell> Spells
        {
            get
            {
                return _lstSpells;
            }
        }

        /// <summary>
        /// Foci.
        /// </summary>
        public IList<Focus> Foci
        {
            get
            {
                return _lstFoci;
            }
        }

        /// <summary>
        /// Stacked Foci.
        /// </summary>
        public IList<StackedFocus> StackedFoci
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
        public ObservableCollection<ComplexForm> ComplexForms
        {
            get
            {
                return _lstComplexForms;
            }
        }

        /// <summary>
        /// AI Programs and Advanced Programs
        /// </summary>
        public ObservableCollection<AIProgram> AIPrograms
        {
            get
            {
                return _lstAIPrograms;
            }
        }

        /// <summary>
        /// Martial Arts.
        /// </summary>
        public ObservableCollection<MartialArt> MartialArts
        {
            get
            {
                return _lstMartialArts;
            }
        }

#if LEGACY
        /// <summary>
        /// Martial Arts Maneuvers.
        /// </summary>
        public IList<MartialArtManeuver> MartialArtManeuvers
        {
            get
            {
                return _lstMartialArtManeuvers;
            }
        }
#endif

        /// <summary>
        /// Limit Modifiers.
        /// </summary>
        public IList<LimitModifier> LimitModifiers
        {
            get
            {
                return _lstLimitModifiers;
            }
        }

        /// <summary>
        /// Armor.
        /// </summary>
        public IList<Armor> Armor
        {
            get
            {
                return _lstArmor;
            }
        }

        /// <summary>
        /// Cyberware and Bioware.
        /// </summary>
        public BindingList<Cyberware> Cyberware
        {
            get
            {
                return _lstCyberware;
            }
        }

        /// <summary>
        /// Weapons.
        /// </summary>
        public IList<Weapon> Weapons
        {
            get
            {
                return _lstWeapons;
            }
        }

        /// <summary>
        /// Lifestyles.
        /// </summary>
        public ObservableCollection<Lifestyle> Lifestyles
        {
            get
            {
                return _lstLifestyles;
            }
        }

        /// <summary>
        /// Gear.
        /// </summary>
        public IList<Gear> Gear
        {
            get
            {
                return _lstGear;
            }
        }

        /// <summary>
        /// Vehicles.
        /// </summary>
        public IList<Vehicle> Vehicles
        {
            get
            {
                return _lstVehicles;
            }
        }

        /// <summary>
        /// Metamagics and Echoes.
        /// </summary>
        public IList<Metamagic> Metamagics
        {
            get
            {
                return _lstMetamagics;
            }
        }

        /// <summary>
        /// Enhancements.
        /// </summary>
        public IList<Enhancement> Enhancements
        {
            get
            {
                return _lstEnhancements;
            }
        }

        /// <summary>
        /// Arts.
        /// </summary>
        public IList<Art> Arts
        {
            get
            {
                return _lstArts;
            }
        }

        /// <summary>
        /// Critter Powers.
        /// </summary>
        public ObservableCollection<CritterPower> CritterPowers
        {
            get
            {
                return _lstCritterPowers;
            }
        }

        /// <summary>
        /// Initiation and Submersion Grades.
        /// </summary>
        public IList<InitiationGrade> InitiationGrades
        {
            get
            {
                return _lstInitiationGrades;
            }
        }

        /// <summary>
        /// Expenses (Karma and Nuyen).
        /// </summary>
        public IList<ExpenseLogEntry> ExpenseEntries
        {
            get
            {
                return _lstExpenseLog;
            }
        }

        /// <summary>
        /// Qualities (Positive and Negative).
        /// </summary>
        public ObservableCollection<Quality> Qualities
        {
            get
            {
                return _lstQualities;
            }
        }
        /// <summary>
        /// Qualities (Positive and Negative).
        /// </summary>
        public IList<LifestyleQuality> LifestyleQualities
        {
            get
            {
                return _lstLifestyleQualities;
            }
        }

        /// <summary>
        /// Life modules
        /// </summary>
        //public IList<LifeModule> LifeModules
        //{
        //    get { return _lstLifeModules; }
        //}

        /// <summary>
        /// Locations.
        /// </summary>
        public IList<string> GearLocations
        {
            get
            {
                return _lstGearLocations;
            }
        }

        /// <summary>
        /// Armor Bundles.
        /// </summary>
        public IList<string> ArmorLocations
        {
            get
            {
                return _lstArmorLocations;
            }
        }

        /// <summary>
        /// Vehicle Locations.
        /// </summary>
        public IList<string> VehicleLocations
        {
            get
            {
                return _lstVehicleLocations;
            }
        }

        /// <summary>
        /// Weapon Locations.
        /// </summary>
        public IList<string> WeaponLocations
        {
            get
            {
                return _lstWeaponLocations;
            }
        }

        /// <summary>
        /// Improvement Groups.
        /// </summary>
        public IList<string> ImprovementGroups
        {
            get
            {
                return _lstImprovementGroups;
            }
        }

        /// <summary>
        /// Calendar.
        /// </summary>
        public BindingList<CalendarWeek> Calendar
        {
            get
            {
                return _lstCalendar;
            }
        }

        /// <summary>
        /// List of internal IDs that need their improvements re-applied.
        /// </summary>
        public IList<string> InternalIdsNeedingReapplyImprovements
        {
            get
            {
                return _lstInternalIdsNeedingReapplyImprovements;
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
                int intHighestNoCustomStack = 0;
                string strHighest = string.Empty;

                // Run through the list of Armor currently worn and retrieve the highest total Armor rating.
                foreach (Armor objArmor in _lstArmor.Where(objArmor => !objArmor.ArmorValue.StartsWith('+') && objArmor.Equipped))
                {
                    int intArmorValue = objArmor.TotalArmor;
                    int intCustomStackBonus = 0;
                    string strArmorName = objArmor.Name;
                    if (objArmor.Category == "High-Fashion Armor Clothing")
                    {
                        foreach (Armor a in _lstArmor.Where(a => (a.Category == "High-Fashion Armor Clothing" || a.ArmorOverrideValue.StartsWith('+')) && a.Equipped))
                        {
                            if (a.ArmorMods.Any(objMod => objMod.Name == "Custom Fit (Stack)" && objMod.Extra == strArmorName))
                                intCustomStackBonus += Convert.ToInt32(a.ArmorOverrideValue);
                        }
                    }
                    if (intArmorValue + intCustomStackBonus > intHighest)
                    {
                        intHighest = intArmorValue + intCustomStackBonus;
                        intHighestNoCustomStack = intArmorValue;
                        strHighest = strArmorName;
                    }
                }

                int intArmor = intHighestNoCustomStack;

                // Run through the list of Armor currently worn again and look at Clothing items that start with '+' since they stack with eachother.
                int intClothing = 0;
                foreach (Armor objArmor in _lstArmor.Where(objArmor => (objArmor.ArmorValue.StartsWith('+') || objArmor.ArmorOverrideValue.StartsWith('+')) && objArmor.Name != strHighest && objArmor.Category == "Clothing" && objArmor.Equipped))
                {
                    if (objArmor.ArmorValue.StartsWith('+'))
                        intClothing += objArmor.TotalArmor;
                    else
                        intClothing += objArmor.TotalOverrideArmor;
                }

                if (intClothing > intHighest)
                {
                    intArmor = intClothing;
                    strHighest = string.Empty;
                }

                // Run through the list of Armor currently worn again and look at non-Clothing items that start with '+' since they stack with the highest Armor.
                int intStacking = 0;
                foreach (Armor objArmor in _lstArmor.Where(objArmor => (objArmor.ArmorValue.StartsWith('+') || objArmor.ArmorOverrideValue.StartsWith('+')) && objArmor.Name != strHighest && objArmor.Category != "Clothing" && objArmor.Equipped))
                {
                    bool blnDoAdd = true;
                    if (objArmor.Category == "High-Fashion Armor Clothing")
                    {
                        foreach (ArmorMod objMod in objArmor.ArmorMods)
                        {
                            if (objMod.Name == "Custom Fit (Stack)")
                            {
                                blnDoAdd = false;
                                if (objMod.Extra == strHighest && !string.IsNullOrEmpty(strHighest))
                                {
                                    blnDoAdd = true;
                                }
                                break;
                            }
                        }
                    }
                    if (blnDoAdd)
                    {
                        if (objArmor.ArmorValue.StartsWith('+'))
                            intStacking += objArmor.TotalArmor;
                        else
                            intStacking += objArmor.TotalOverrideArmor;
                    }
                }

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
                int intHighest = 0;
                int intTotalA = 0;
                // Run through the list of Armor currently worn and retrieve the highest total Armor rating.
                // This is used for Custom-Fit armour's stacking.
                foreach (Armor objArmor in _lstArmor.Where(objArmor => objArmor.Equipped && !objArmor.ArmorValue.StartsWith('+')))
                {
                    int intLoopTotal = objArmor.TotalArmor;
                    string strArmorName = objArmor.Name;
                    if (objArmor.Category == "High-Fashion Armor Clothing")
                    {
                        foreach (Armor a in _lstArmor.Where(a => (a.Category == "High-Fashion Armor Clothing" || a.ArmorOverrideValue.StartsWith('+')) && a.Equipped))
                        {
                            if (a.ArmorMods.Any(objMod => objMod.Name == "Custom Fit (Stack)" && objMod.Extra == strArmorName))
                                intLoopTotal += Convert.ToInt32(a.ArmorOverrideValue);
                        }
                    }
                    if (intLoopTotal > intHighest)
                    {
                        intHighest = intLoopTotal;
                        strHighest = strArmorName;
                    }
                }

                // Run through the list of Armor currently worn again and look at Clothing items that start with '+' since they stack with eachother.
                int intClothing = 0;
                foreach (Armor objArmor in _lstArmor.Where(objArmor => (objArmor.ArmorValue.StartsWith('+') || objArmor.ArmorOverrideValue.StartsWith('+')) && objArmor.Name != strHighest && objArmor.Category == "Clothing" && objArmor.Equipped))
                {
                    if (objArmor.ArmorValue.StartsWith('+'))
                        intClothing += objArmor.TotalArmor;
                    else
                        intClothing += objArmor.TotalOverrideArmor;
                }

                if (intClothing > intHighest)
                {
                    strHighest = string.Empty;
                }

                foreach (Armor objArmor in _lstArmor.Where(objArmor => (objArmor.ArmorValue.StartsWith('+') || objArmor.ArmorOverrideValue.StartsWith('+')) && objArmor.Name != strHighest && objArmor.Category != "Clothing" && objArmor.Equipped))
                {
                    bool blnDoAdd = true;
                    if (objArmor.Category == "High-Fashion Armor Clothing")
                    {
                        foreach (ArmorMod objMod in objArmor.ArmorMods)
                        {
                            if (objMod.Name == "Custom Fit (Stack)")
                            {
                                blnDoAdd = false;
                                if (objMod.Extra == strHighest && !string.IsNullOrEmpty(strHighest))
                                {
                                    blnDoAdd = true;
                                }
                                break;
                            }
                        }
                    }
                    if (blnDoAdd)
                    {
                        if (objArmor.ArmorValue.StartsWith('+'))
                            intTotalA += objArmor.TotalArmor;
                        else
                            intTotalA += objArmor.TotalOverrideArmor;
                    }
                }

                // Highest armor was overwritten by Clothing '+' values, so factor those '+' values into encumbrance
                if (string.IsNullOrEmpty(strHighest))
                    intTotalA += intClothing;

                // calculate armor encumberance
                int intSTRTotalValue = STR.TotalValue;
                if (intTotalA > intSTRTotalValue + 1)
                    return (intTotalA - intSTRTotalValue) / 2 * -1;  // we expect a negative number
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
                // If UnrestrictedNueyn is enabled, return the number of BP or Karma the character is being built with, otherwise use the standard value attached to the character.
                if (_objOptions.UnrestrictedNuyen)
                {
                    if (_intBuildKarma > 0)
                        return _intBuildKarma;
                    else
                        return 1000.0m;
                }
                else
                {
                    decimal decImprovement = ImprovementManager.ValueOf(this, Improvement.ImprovementType.NuyenMaxBP);
                    if (_objBuildMethod == CharacterBuildMethod.Karma)
                        decImprovement *= 2.0m;
                    return Math.Max(_decNuyenMaximumBP, decImprovement);
                }
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
                    Vehicle objHomeNodeVehicle = HomeNode as Vehicle;
                    return objHomeNodeVehicle?.Handling ?? 0;
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
                    if (HomeNode != null)
                    {
                        if (HomeNode is Vehicle objHomeNodeVehicle)
                        {
                            int intHomeNodeSensor = objHomeNodeVehicle.CalculatedSensor;
                            if (intHomeNodeSensor > intLimit)
                            {
                                intLimit = intHomeNodeSensor;
                            }
                        }
                        int intHomeNodeDP = HomeNode.GetTotalMatrixAttribute("Data Processing");
                        if (intHomeNodeDP > intLimit)
                        {
                            intLimit = intHomeNodeDP;
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
                if (_strMetatype == "A.I." && HomeNode != null)
                {
                    int intHomeNodeDP = HomeNode.GetTotalMatrixAttribute("Data Processing");

                    if (HomeNode is Vehicle objHomeNodeVehicle)
                    {
                        int intHomeNodePilot = objHomeNodeVehicle.Pilot;
                        if (intHomeNodePilot > intHomeNodeDP)
                            intHomeNodeDP = intHomeNodePilot;
                    }

                    intLimit = (CHA.TotalValue + intHomeNodeDP + WIL.TotalValue + decimal.ToInt32(decimal.Ceiling(Essence)) + 2) / 3;
                }
                else
                {
                    intLimit = (CHA.TotalValue * 2 + WIL.TotalValue + decimal.ToInt32(decimal.Ceiling(Essence)) + 2) / 3;
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
        /// Character's Movement rate (Culture-dependent).
        /// </summary>
        public string GetMovement(CultureInfo objCulture, string strLanguage)
        {
            if (string.IsNullOrWhiteSpace(_strWalk) || string.IsNullOrWhiteSpace(_strRun) || string.IsNullOrWhiteSpace(_strSprint) || string.IsNullOrWhiteSpace(_strMovement) || (MetatypeCategory == "Shapeshifter" && (string.IsNullOrWhiteSpace(_strWalkAlt) || string.IsNullOrWhiteSpace(_strRunAlt) || string.IsNullOrWhiteSpace(_strSprintAlt))))
            {
                string strReturn = string.Empty;
                XmlDocument objXmlDocument = XmlManager.Load(_blnIsCritter ? "critters.xml" : "metatypes.xml", strLanguage);
                XmlNode meta = objXmlDocument.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + _strMetatype + "\"]");
                XmlNode variant = meta?.SelectSingleNode("metavariants/metavariant[name = \"" + _strMetavariant + "\"]");
                XmlNode objRunNode = variant?["run"] ?? meta?["run"];
                XmlNode objWalkNode = variant?["walk"] ?? meta?["walk"];
                XmlNode objSprintNode = variant?["sprint"] ?? meta?["sprint"];

                _strMovement = variant?["movement"]?.InnerText ?? meta?["movement"]?.InnerText ?? string.Empty;
                _strRun = objRunNode?.InnerText ?? string.Empty;
                _strWalk = objWalkNode?.InnerText ?? string.Empty;
                _strSprint = objSprintNode?.InnerText ?? string.Empty;

                objRunNode = objRunNode?.Attributes?["alt"];
                objWalkNode = objWalkNode?.Attributes?["alt"];
                objSprintNode = objSprintNode?.Attributes?["alt"];
                _strRunAlt = objRunNode?.InnerText ?? string.Empty;
                _strWalkAlt = objWalkNode?.InnerText ?? string.Empty;
                _strSprintAlt = objSprintNode?.InnerText ?? string.Empty;
            }
            // Don't attempt to do anything if the character's Movement is "Special" (typically for A.I.s).
            if (_strMovement == "Special")
            {
                return LanguageManager.GetString("String_ModeSpecial", strLanguage);
            }

            return CalculatedMovement("Ground", true, objCulture);
        }

        /// <summary>
        /// Character's Movement rate.
        /// </summary>
        public string Movement
        {
            set
            {
                _strMovement = value;
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

            int intTmp = 0;
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

            decimal decTmp = 0;
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

        private string CalculatedMovement(string strMovementType, bool blnUseCyberlegs = false, CultureInfo objCulture = null)
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

            if (objCulture == null)
                objCulture = GlobalOptions.CultureInfo;
            string strReturn = string.Empty;
            if (strMovementType == "Swim")
            {
                decWalk *= (intAGI + intSTR) * 0.5m;
                strReturn = decWalk.ToString("#,0.##", objCulture) + ", " + decSprint.ToString("#,0.##", objCulture) + "m/ hit";
            }
            else
            {
                decWalk *= intAGI;
                decRun *= intAGI;
                strReturn = decWalk.ToString("#,0.##", objCulture) + '/' + decRun.ToString("#,0.##", objCulture) + ", " + decSprint.ToString("#,0.##", objCulture) + "m/ hit";
            }
            return strReturn;
        }

        /// <summary>
        /// Character's Swim rate.
        /// </summary>
        public string GetSwim(CultureInfo objCulture, string strLanguage)
        {
            // Don't attempt to do anything if the character's Movement is "Special" (typically for A.I.s).
            if (_strMovement == "Special")
            {
                return LanguageManager.GetString("String_ModeSpecial", strLanguage);
            }

            XmlNode objXmlNode = XmlManager.Load(_blnIsCritter ? "critters.xml" : "metatypes.xml", strLanguage).SelectSingleNode("/chummer/metatypes/metatype[name = \"" + _strMetatype + "\"]");
            if (objXmlNode != null)
            {
                string strReturn = string.Empty;
                objXmlNode.TryGetStringFieldQuickly("movement", ref strReturn);
                if (strReturn == "Special")
                {
                    return LanguageManager.GetString("String_ModeSpecial", strLanguage);
                }
            }

            return CalculatedMovement("Swim", false, objCulture);
        }

        /// <summary>
        /// Character's Fly rate.
        /// </summary>
        public string GetFly(CultureInfo objCulture, string strLanguage)
        {
            // Don't attempt to do anything if the character's Movement is "Special" (typically for A.I.s).
            if (_strMovement == "Special")
            {
                return LanguageManager.GetString("String_ModeSpecial", strLanguage);
            }

            XmlNode objXmlNode = XmlManager.Load(_blnIsCritter ? "critters.xml" : "metatypes.xml", strLanguage).SelectSingleNode("/chummer/metatypes/metatype[name = \"" + _strMetatype + "\"]");
            if (objXmlNode != null)
            {
                string strReturn = string.Empty;
                objXmlNode.TryGetStringFieldQuickly("movement", ref strReturn);
                if (strReturn == "Special")
                {
                    return LanguageManager.GetString("String_ModeSpecial", strLanguage);
                }
            }

            return CalculatedMovement("Fly", false, objCulture);
        }

        /// <summary>
        /// Full Movement (Movement, Swim, and Fly) for printouts.
        /// </summary>
        private string FullMovement(CultureInfo objCulture, string strLanguage)
        {
            string strReturn = string.Empty;
            string strGroundMovement = GetMovement(objCulture, strLanguage);
            string strSwimMovement = GetSwim(objCulture, strLanguage);
            string strFlyMovement = GetFly(objCulture, strLanguage);
            if (!string.IsNullOrEmpty(strGroundMovement) && strGroundMovement != "0")
                strReturn += strGroundMovement + ", ";
            if (!string.IsNullOrEmpty(strSwimMovement) && strSwimMovement != "0")
                strReturn += LanguageManager.GetString("Label_OtherSwim", strLanguage) + ' ' + strSwimMovement + ", ";
            if (!string.IsNullOrEmpty(strFlyMovement) && strFlyMovement != "0")
                strReturn += LanguageManager.GetString("Label_OtherFly", strLanguage) + ' ' + strFlyMovement + ", ";

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
                if (_blnAdeptEnabled != value)
                {
                    _blnAdeptEnabled = value;
                    AdeptTabEnabledChanged?.Invoke(this);
                }
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
                if (_blnMagicianEnabled != value)
                {
                    _blnMagicianEnabled = value;
                    MagicianTabEnabledChanged?.Invoke(this);
                }
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
                if (_blnTechnomancerEnabled != value)
                {
                    _blnTechnomancerEnabled = value;
                    TechnomancerTabEnabledChanged?.Invoke(this);
                }
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
                if (_blnAdvancedProgramsEnabled != value)
                {
                    _blnAdvancedProgramsEnabled = value;
                    AdvancedProgramsTabEnabledChanged?.Invoke(this);
                }
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
                if (_blnCyberwareDisabled != value)
                {
                    _blnCyberwareDisabled = value;
                    CyberwareTabDisabledChanged?.Invoke(this);
                }
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
                if (_blnInitiationEnabled != value)
                {
                    _blnInitiationEnabled = value;
                    InitiationTabEnabledChanged?.Invoke(this);
                }
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
                if (_blnCritterEnabled != value)
                {
                    _blnCritterEnabled = value;
                    CritterTabEnabledChanged?.Invoke(this);
                }
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
                _blnBlackMarketDiscount = value;
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
                _blnFriendsInHighPlaces = value;
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
                if (_blnExCon != value)
                {
                    _blnExCon = value;
                    ExConChanged?.Invoke(this);
                }
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
                _intTrustFund = value;
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
                if (_blnRestrictedGear != value)
                {
                    _blnRestrictedGear = value;
                    RestrictedGearChanged?.Invoke(this);
                }
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
                _blnOverclocker = value;
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
                if (_blnMadeMan != value)
                {
                    _blnMadeMan = value;
                    MadeManChanged?.Invoke(this);
                }
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
                _blnFame = value;
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
                if (_blnBornRich != value)
                {
                    _blnBornRich = value;
                    BornRichChanged?.Invoke(this);
                }
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
                _blnErased = value;
            }
        }
        /// <summary>
        /// Convert a string to a CharacterBuildMethod.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        public static CharacterBuildMethod ConvertToCharacterBuildMethod(string strValue)
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
            bool blnShowTest = false;
            string strTestSuffix = LanguageManager.GetString("String_AvailRestricted", GlobalOptions.Language);
            if (strAvail.EndsWith(strTestSuffix))
            {
                blnShowTest = true;
                strAvail = strAvail.TrimEnd(strTestSuffix, true);
            }
            else
            {
                strTestSuffix = LanguageManager.GetString("String_AvailForbidden", GlobalOptions.Language);
                if (strAvail.EndsWith(strTestSuffix))
                {
                    blnShowTest = true;
                    strAvail = strAvail.TrimEnd(strTestSuffix, true);
                }
            }
            if (int.TryParse(strAvail, out int intAvail) && (intAvail != 0 || blnShowTest))
            {
                return GetAvailTestString(decCost, intAvail);
            }

            return LanguageManager.GetString("String_None", GlobalOptions.Language);
        }

        /// <summary>
        /// Extended Availability Test information for an item based on the character's Negotiate Skill.
        /// </summary>
        /// <param name="intCost">Item's cost.</param>
        /// <param name="strAvail">Item's Availability.</param>
        public string AvailTest(decimal decCost, AvailabilityValue objAvailability)
        {
            if (objAvailability.Value != 0 || objAvailability.Suffix == 'R' || objAvailability.Suffix == 'F')
            {
                return GetAvailTestString(decCost, objAvailability.Value);
            }

            return LanguageManager.GetString("String_None", GlobalOptions.Language);
        }

        private string GetAvailTestString(decimal decCost, int intAvailValue)
        {
            string strInterval;
            // Find the character's Negotiation total.
            int intPool = SkillsSection.GetActiveSkill("Negotiation")?.Pool ?? 0;
            // Determine the interval based on the item's price.
            if (decCost <= 100.0m)
                strInterval = "6 " + LanguageManager.GetString("String_Hours", GlobalOptions.Language);
            else if (decCost <= 1000.0m)
                strInterval = "1 " + LanguageManager.GetString("String_Day", GlobalOptions.Language);
            else if (decCost <= 10000.0m)
                strInterval = "2 " + LanguageManager.GetString("String_Days", GlobalOptions.Language);
            else if (decCost <= 100000.0m)
                strInterval = "1 " + LanguageManager.GetString("String_Week", GlobalOptions.Language);
            else
                strInterval = "1 " + LanguageManager.GetString("String_Month", GlobalOptions.Language);

            return intPool.ToString(GlobalOptions.CultureInfo) + " (" + intAvailValue.ToString(GlobalOptions.CultureInfo) + ", " + strInterval + ')';
        }

        /// <summary>
        /// Whether or not Adapsin is enabled.
        /// </summary>
        public bool AdapsinEnabled
        {
            get
            {
                return Improvements.Any(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.Adapsin && objImprovement.Enabled);
            }
        }

        /// <summary>
        /// Whether or not Burnout's Way is enabled.
        /// </summary>
        public bool BurnoutEnabled
        {
            get
            {
                return Improvements.Any(x => x.ImproveType == Improvement.ImprovementType.BurnoutsWay && x.Enabled);
            }
        }

        /// <summary>
        /// Whether or not the character has access to Knowsofts and Linguasofts.
        /// </summary>
        public bool SkillsoftAccess
        {
            get
            {
                return Improvements.Any(x => x.ImproveType == Improvement.ImprovementType.SkillsoftAccess && x.Enabled);
            }
        }
#endregion

#region Application Properties
        private readonly List<Character> _lstLinkedCharacters = new List<Character>();
        /// <summary>
        /// Characters referenced by some member of this character (usually a contact).
        /// </summary>
        public IList<Character> LinkedCharacters
        {
            get
            {
                return _lstLinkedCharacters;
            }
        }
#endregion

#region Old Quality Conversion Code
        /// <summary>
        /// Convert Qualities that are still saved in the old format.
        /// </summary>
        private void ConvertOldQualities(XmlNodeList objXmlQualityList)
        {
            XmlNode xmlRootQualitiesNode = XmlManager.Load("qualities.xml").SelectSingleNode("/chummer/qualities");

            // Convert the old Qualities.
            foreach (XmlNode objXmlQuality in objXmlQualityList)
            {
                if (objXmlQuality["name"] == null)
                {
                    _lstOldQualities.Add(objXmlQuality.InnerText);

                    string strForceValue = string.Empty;

                    XmlNode objXmlQualityNode = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"" + GetQualityName(objXmlQuality.InnerText) + "\"]");

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
                    objQuality.Create(objXmlQualityNode, QualitySource.Selected, _lstWeapons, strForceValue);
                    _lstQualities.Add(objQuality);
                }
            }

            // Take care of the Metatype information.
            string strXPath = "/chummer/metatypes/metatype[name = \"" + _strMetatype + "\"]";
            XmlNode objXmlMetatype = XmlManager.Load("metatypes.xml").SelectSingleNode(strXPath);
            if (objXmlMetatype == null)
            {
                objXmlMetatype = XmlManager.Load("critters.xml").SelectSingleNode(strXPath);
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
                    Quality objQuality = new Quality(this);

                    if (objXmlMetatypeQuality.Attributes["select"] != null)
                        strForceValue = objXmlMetatypeQuality.Attributes["select"].InnerText;

                    XmlNode objXmlQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"" + objXmlMetatypeQuality.InnerText + "\"]");
                    objQuality.Create(objXmlQuality, QualitySource.Metatype, _lstWeapons, strForceValue);
                    _lstQualities.Add(objQuality);
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
                    Quality objQuality = new Quality(this);

                    if (objXmlMetatypeQuality.Attributes["select"] != null)
                        strForceValue = objXmlMetatypeQuality.Attributes["select"].InnerText;

                    XmlNode objXmlQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"" + objXmlMetatypeQuality.InnerText + "\"]");
                    objQuality.Create(objXmlQuality, QualitySource.Metatype, _lstWeapons, strForceValue);
                    _lstQualities.Add(objQuality);
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
                        Quality objQuality = new Quality(this);

                        if (objXmlMetatypeQuality.Attributes["select"] != null)
                            strForceValue = objXmlMetatypeQuality.Attributes["select"].InnerText;

                        XmlNode objXmlQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"" + objXmlMetatypeQuality.InnerText + "\"]");
                        objQuality.Create(objXmlQuality, QualitySource.Metatype, _lstWeapons, strForceValue);
                        _lstQualities.Add(objQuality);
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
                        Quality objQuality = new Quality(this);

                        if (objXmlMetatypeQuality.Attributes["select"] != null)
                            strForceValue = objXmlMetatypeQuality.Attributes["select"].InnerText;

                        XmlNode objXmlQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"" + objXmlMetatypeQuality.InnerText + "\"]");
                        objQuality.Create(objXmlQuality, QualitySource.Metatype, _lstWeapons, strForceValue);
                        _lstQualities.Add(objQuality);
                    }
                }
            }
        }

        /// <summary>
        /// Get the name of a Quality by parsing out its BP cost.
        /// </summary>
        /// <param name="strQuality">String to parse.</param>
        private static string GetQualityName(string strQuality)
        {
            int intPos = strQuality.IndexOf('[');
            if (intPos != -1)
                strQuality = strQuality.Substring(0, intPos - 1);
            return strQuality;
        }

        /// <summary>
        /// Check for older instances of certain qualities that were manually numbered to be replaced with multiple instances of the first level quality (so that it works with the level system)
        /// Returns true if it's a corrected quality, false otherwise
        /// </summary>
        /// <param name="strQuality">String to parse.</param>
        private bool CorrectedUnleveledQuality(XmlNode objOldXmlQuality, XmlDocument xmlQualitiesDocument)
        {
            XmlNode objXmlNewQuality = null;
            int intRanks = 0;
            switch (objOldXmlQuality["name"].InnerText)
            {
                case "Focused Concentration (Rating 1)":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Focused Concentration\"]");
                        intRanks = 1;
                        break;
                    }
                case "Focused Concentration (Rating 2)":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Focused Concentration\"]");
                        intRanks = 2;
                        break;
                    }
                case "Focused Concentration (Rating 3)":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Focused Concentration\"]");
                        intRanks = 3;
                        break;
                    }
                case "Focused Concentration (Rating 4)":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Focused Concentration\"]");
                        intRanks = 4;
                        break;
                    }
                case "Focused Concentration (Rating 5)":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Focused Concentration\"]");
                        intRanks = 5;
                        break;
                    }
                case "Focused Concentration (Rating 6)":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Focused Concentration\"]");
                        intRanks = 6;
                        break;
                    }
                case "High Pain Tolerance (Rating 1)":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"High Pain Tolerance\"]");
                        intRanks = 1;
                        break;
                    }
                case "High Pain Tolerance (Rating 2)":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"High Pain Tolerance\"]");
                        intRanks = 2;
                        break;
                    }
                case "High Pain Tolerance (Rating 3)":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"High Pain Tolerance\"]");
                        intRanks = 3;
                        break;
                    }
                case "Magic Resistance (Rating 1)":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Magic Resistance\"]");
                        intRanks = 1;
                        break;
                    }
                case "Magic Resistance (Rating 2)":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Magic Resistance\"]");
                        intRanks = 2;
                        break;
                    }
                case "Magic Resistance (Rating 3)":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Magic Resistance\"]");
                        intRanks = 3;
                        break;
                    }
                case "Magic Resistance (Rating 4)":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Magic Resistance\"]");
                        intRanks = 4;
                        break;
                    }
                case "Will to Live (Rating 1)":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Will to Live\"]");
                        intRanks = 1;
                        break;
                    }
                case "Will to Live (Rating 2)":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Will to Live\"]");
                        intRanks = 2;
                        break;
                    }
                case "Will to Live (Rating 3)":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Will to Live\"]");
                        intRanks = 3;
                        break;
                    }
                case "Gremlins (Rating 1)":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Gremlins\"]");
                        intRanks = 1;
                        break;
                    }
                case "Gremlins (Rating 2)":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Gremlins\"]");
                        intRanks = 2;
                        break;
                    }
                case "Gremlins (Rating 3)":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Gremlins\"]");
                        intRanks = 3;
                        break;
                    }
                case "Gremlins (Rating 4)":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Gremlins\"]");
                        intRanks = 4;
                        break;
                    }
                case "Aged (Rating 1)":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Aged\"]");
                        intRanks = 1;
                        break;
                    }
                case "Aged (Rating 2)":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Aged\"]");
                        intRanks = 2;
                        break;
                    }
                case "Aged (Rating 3)":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Aged\"]");
                        intRanks = 3;
                        break;
                    }
                case "Illness (Rating 1)":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Illness\"]");
                        intRanks = 1;
                        break;
                    }
                case "Illness (Rating 2)":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Illness\"]");
                        intRanks = 2;
                        break;
                    }
                case "Illness (Rating 3)":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Illness\"]");
                        intRanks = 3;
                        break;
                    }
                case "Perceptive I":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Perceptive\"]");
                        intRanks = 1;
                        break;
                    }
                case "Perceptive II":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Perceptive\"]");
                        intRanks = 2;
                        break;
                    }
                case "Spike Resistance I":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Spike Resistance\"]");
                        intRanks = 1;
                        break;
                    }
                case "Spike Resistance II":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Spike Resistance\"]");
                        intRanks = 2;
                        break;
                    }
                case "Spike Resistance III":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Spike Resistance\"]");
                        intRanks = 3;
                        break;
                    }
                case "Tough as Nails Physical I":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Tough as Nails (Physical)\"]");
                        intRanks = 1;
                        break;
                    }
                case "Tough as Nails Physical II":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Tough as Nails (Physical)\"]");
                        intRanks = 2;
                        break;
                    }
                case "Tough as Nails Physical III":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Tough as Nails (Physical)\"]");
                        intRanks = 3;
                        break;
                    }
                case "Tough as Nails Stun I":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Tough as Nails (Stun)\"]");
                        intRanks = 1;
                        break;
                    }
                case "Tough as Nails Stun II":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Tough as Nails (Stun)\"]");
                        intRanks = 2;
                        break;
                    }
                case "Tough as Nails Stun III":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Tough as Nails (Stun)\"]");
                        intRanks = 3;
                        break;
                    }
                case "Dimmer Bulb I":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Dimmer Bulb\"]");
                        intRanks = 1;
                        break;
                    }
                case "Dimmer Bulb II":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Dimmer Bulb\"]");
                        intRanks = 2;
                        break;
                    }
                case "Dimmer Bulb III":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Dimmer Bulb\"]");
                        intRanks = 3;
                        break;
                    }
                case "In Debt I":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"In Debt\"]");
                        intRanks = 1;
                        break;
                    }
                case "In Debt II":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"In Debt\"]");
                        intRanks = 2;
                        break;
                    }
                case "In Debt III":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"In Debt\"]");
                        intRanks = 3;
                        break;
                    }
                case "In Debt IV":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"In Debt\"]");
                        intRanks = 4;
                        break;
                    }
                case "In Debt V":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"In Debt\"]");
                        intRanks = 5;
                        break;
                    }
                case "In Debt VI":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"In Debt\"]");
                        intRanks = 6;
                        break;
                    }
                case "In Debt VII":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"In Debt\"]");
                        intRanks = 7;
                        break;
                    }
                case "In Debt VIII":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"In Debt\"]");
                        intRanks = 8;
                        break;
                    }
                case "In Debt IX":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"In Debt\"]");
                        intRanks = 9;
                        break;
                    }
                case "In Debt X":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"In Debt\"]");
                        intRanks = 10;
                        break;
                    }
                case "In Debt XI":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"In Debt\"]");
                        intRanks = 11;
                        break;
                    }
                case "In Debt XII":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"In Debt\"]");
                        intRanks = 12;
                        break;
                    }
                case "In Debt XIII":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"In Debt\"]");
                        intRanks = 13;
                        break;
                    }
                case "In Debt XIV":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"In Debt\"]");
                        intRanks = 14;
                        break;
                    }
                case "In Debt XV":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"In Debt\"]");
                        intRanks = 15;
                        break;
                    }
                case "Infirm I":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Infirm\"]");
                        intRanks = 1;
                        break;
                    }
                case "Infirm II":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Infirm\"]");
                        intRanks = 2;
                        break;
                    }
                case "Infirm III":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Infirm\"]");
                        intRanks = 3;
                        break;
                    }
                case "Infirm IV":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Infirm\"]");
                        intRanks = 4;
                        break;
                    }
                case "Infirm V":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Infirm\"]");
                        intRanks = 5;
                        break;
                    }
                case "Shiva Arms (1 Pair)":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Shiva Arms (Pair)\"]");
                        intRanks = 1;
                        break;
                    }
                case "Shiva Arms (2 Pair)":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Shiva Arms (Pair)\"]");
                        intRanks = 2;
                        break;
                    }
                case "Arcane Arrester I":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Arcane Arrester\"]");
                        intRanks = 1;
                        break;
                    }
                case "Arcane Arrester II":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Arcane Arrester\"]");
                        intRanks = 2;
                        break;
                    }
                case "Pilot Origins (Rating 1)":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Pilot Origins\"]");
                        intRanks = 1;
                        break;
                    }
                case "Pilot Origins (Rating 2)":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Pilot Origins\"]");
                        intRanks = 2;
                        break;
                    }
                case "Pilot Origins (Rating 3)":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Pilot Origins\"]");
                        intRanks = 3;
                        break;
                    }
                case "Social Appearance Anxiety (Rating 1)":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Social Appearance Anxiety\"]");
                        intRanks = 1;
                        break;
                    }
                case "Social Appearance Anxiety (Rating 2)":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Social Appearance Anxiety\"]");
                        intRanks = 2;
                        break;
                    }
                case "Social Appearance Anxiety (Rating 3)":
                    {
                        objXmlNewQuality = xmlQualitiesDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Social Appearance Anxiety\"]");
                        intRanks = 3;
                        break;
                    }
            }
            if (intRanks > 0)
            {
                for (int i = 0; i < intRanks; ++i)
                {
                    Quality objQuality = new Quality(this);
                    if (i == 0 && Guid.TryParse(objOldXmlQuality["guid"].InnerText, out Guid guidOld))
                        objQuality.SetGUID(guidOld);
                    QualitySource objQualitySource = Quality.ConvertToQualitySource(objOldXmlQuality["qualitysource"]?.InnerText);
                    objQuality.Create(objXmlNewQuality, objQualitySource, _lstWeapons, objOldXmlQuality["extra"]?.InnerText);
                    if (objOldXmlQuality["bp"] != null && int.TryParse(objOldXmlQuality["bp"].InnerText, out int intOldBP))
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
        public static string TranslatedBookList(string strInput, string strLanguage)
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
        private readonly Lazy<Stack<string>> _pushtext = new Lazy<Stack<string>>();
        private bool _blnAmbidextrous;

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
        /// The Active Commlink of the character. Returns null if home node is not a commlink.
        /// </summary>
        public IHasMatrixAttributes ActiveCommlink
        {
            get
            {
                return _objActiveCommlink;
            }
            set
            {
                _objActiveCommlink = value;
            }
        }

        /// <summary>
        /// Home Node. Returns null if home node is not set to any item.
        /// </summary>
        public IHasMatrixAttributes HomeNode
        {
            get
            {
                return _objHomeNode;
            }
            set
            {
                _objHomeNode = value;
            }
        }

        public SkillsSection SkillsSection { get; }


        public int RedlinerBonus
        {
            get { return _intRedlinerBonus; }
            set { _intRedlinerBonus = value; }
        }

        /// <summary>
        /// Refreshes Redliner and Cyber-Singularity Seeker. Returns false if RedlinerBonus has changed, otherwise returns true.
        /// </summary>
        /// <returns>False if RedlinerBonus has changed, True otherwise</returns>
        public bool RefreshRedliner()
        {
            int intOldRedlinerBonus = RedlinerBonus;
            List<string> lstSeekerAttributes = new List<string>();
            List<Improvement> lstSeekerImprovements = new List<Improvement>();
            //Get attributes affected by redliner/cyber singularity seeker
            foreach (Improvement objLoopImprovement in Improvements)
            {
                if (objLoopImprovement.ImproveType == Improvement.ImprovementType.Seeker)
                {
                    lstSeekerAttributes.Add(objLoopImprovement.ImprovedName);
                }
                else if ((objLoopImprovement.ImproveType == Improvement.ImprovementType.Attribute ||
                       objLoopImprovement.ImproveType == Improvement.ImprovementType.PhysicalCM) &&
                      objLoopImprovement.SourceName.Contains("SEEKER"))
                {
                    lstSeekerImprovements.Add(objLoopImprovement);
                }
            }

            //if neither contains anything, it is safe to exit
            if (lstSeekerImprovements.Count == 0 && lstSeekerAttributes.Count == 0)
            {
                RedlinerBonus = 0;
                return intOldRedlinerBonus == 0;
            }
            
            //Calculate bonus from cyberlimbs
            int intCount = 0;
            foreach (Cyberware objCyberware in Cyberware)
            {
                intCount += objCyberware.GetCyberlimbCount("skull", "torso");
            }
            intCount = Math.Min(intCount / 2, 2);
            if (lstSeekerImprovements.Any(x => x.ImprovedName == "STR" || x.ImprovedName == "AGI"))
            {
                RedlinerBonus = intCount;
            }
            else
            {
                RedlinerBonus = 0;
            }

            for (int i = 0; i < lstSeekerAttributes.Count; i++)
            {
                Improvement objImprove = lstSeekerImprovements.FirstOrDefault(x => x.SourceName == "SEEKER_" + lstSeekerAttributes[i] && x.Value == (lstSeekerAttributes[i] == "BOX" ? intCount * -3 : intCount));
                if (objImprove != null)
                {
                    lstSeekerAttributes.RemoveAt(i);
                    lstSeekerImprovements.Remove(objImprove);
                    i--;
                }
            }
            //Improvement manager defines the functions we need to manipulate improvements
            //When the locals (someday) gets moved to this class, this can be removed and use
            //the local

            // Remove which qualites have been removed or which values have changed
            ImprovementManager.RemoveImprovements(this, lstSeekerImprovements);

            // Add new improvements or old improvements with new values
            foreach (string strAttribute in lstSeekerAttributes)
            {
                if (strAttribute == "BOX")
                {
                    ImprovementManager.CreateImprovement(this, strAttribute, Improvement.ImprovementSource.Quality, "SEEKER_BOX", Improvement.ImprovementType.PhysicalCM, Guid.NewGuid().ToString("D"), intCount * -3);
                }
                else
                {
                    ImprovementManager.CreateImprovement(this, strAttribute, Improvement.ImprovementSource.Quality, "SEEKER_" + strAttribute, Improvement.ImprovementType.Attribute, Guid.NewGuid().ToString("D"), intCount, 1, 0, 0, intCount);
                }
            }
            ImprovementManager.Commit(this);
            return intOldRedlinerBonus == RedlinerBonus;
        }

        public Version LastSavedVersion
        {
            get { return _verSavedVersion; }
        }

        /// <summary>
        /// Is the character a mystic adept (MagicianEnabled && AdeptEnabled)? Used for databinding properties.
        /// </summary>
        public bool IsMysticAdept
        {
            get { return AdeptEnabled && MagicianEnabled; }
        }

        /// <summary>
        /// Could this character buy Power Points in career mode if the optional/house rule is enabled
        /// </summary>
        public bool CanAffordCareerPP
        {
            get
            {
                return Options.MysAdeptAllowPPCareer && Karma >= 5 && MAG.TotalValue > MysticAdeptPowerPoints;
            }
        }

        /// <summary>
        /// Blocked grades of cyber/bioware in Create mode. 
        /// </summary>
        public IList<string> BannedWareGrades { get; } = new List<string>(){ "Betaware", "Deltaware", "Gammaware" };

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual bool OnPropertyChanged<T>(ref T old, T value, [CallerMemberName] string propertyName = null)
        {
            if ((old == null && value != null) || value == null || !old.Equals(value))
            {
                old = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }
            return false;
        }

        //I also think this prevents GC. But there is no good way to do it...
        internal event Action<ICollection<Improvement>> SkillImprovementEvent;
        internal event Action<ICollection<Improvement>> AttributeImprovementEvent;

        //List of events that might be able to affect skills. Made quick to prevent an infinite recursion somewhere related to adding an expense so it might be shaved down.
        public static readonly HashSet<Improvement.ImprovementType> SkillRelatedImprovements = new HashSet<Improvement.ImprovementType> {
            Improvement.ImprovementType.FreeKnowledgeSkills,
            Improvement.ImprovementType.Skillwire,
            Improvement.ImprovementType.SwapSkillAttribute,
            Improvement.ImprovementType.SkillsoftAccess,
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
            Improvement.ImprovementType.ActiveSkillKarmaCostMultiplier,
            Improvement.ImprovementType.SkillGroupKarmaCostMultiplier,
            Improvement.ImprovementType.KnowledgeSkillKarmaCostMultiplier,
            Improvement.ImprovementType.ActiveSkillKarmaCost,
            Improvement.ImprovementType.SkillGroupKarmaCost,
            Improvement.ImprovementType.SkillGroupDisable,
            Improvement.ImprovementType.KnowledgeSkillKarmaCost,
            Improvement.ImprovementType.SkillCategoryKarmaCostMultiplier,
            Improvement.ImprovementType.SkillCategoryKarmaCost,
            Improvement.ImprovementType.SkillCategorySpecializationKarmaCostMultiplier,
            Improvement.ImprovementType.SkillCategorySpecializationKarmaCost,
            Improvement.ImprovementType.SkillGroupCategoryKarmaCostMultiplier,
            Improvement.ImprovementType.SkillGroupCategoryDisable,
            Improvement.ImprovementType.SkillGroupCategoryKarmaCost,
            Improvement.ImprovementType.ActiveSkillPointCostMultiplier,
            Improvement.ImprovementType.SkillGroupPointCostMultiplier,
            Improvement.ImprovementType.KnowledgeSkillPointCostMultiplier,
            Improvement.ImprovementType.ActiveSkillPointCost,
            Improvement.ImprovementType.SkillGroupPointCost,
            Improvement.ImprovementType.KnowledgeSkillPointCost,
            Improvement.ImprovementType.SkillCategoryPointCostMultiplier,
            Improvement.ImprovementType.SkillCategoryPointCost,
            Improvement.ImprovementType.SkillGroupCategoryPointCostMultiplier,
            Improvement.ImprovementType.SkillGroupCategoryPointCost,
            Improvement.ImprovementType.BlockSkillSpecializations,
            Improvement.ImprovementType.BlockSkillCategorySpecializations,
        };

        //List of events that might be able to affect attributes. Changes to these types also invoke data bindings controlling skills, since their pools are controlled by attributes.
        public static readonly HashSet<Improvement.ImprovementType> AttribRelatedImprovements = new HashSet<Improvement.ImprovementType> {
            Improvement.ImprovementType.Attribute,
            Improvement.ImprovementType.Essence,
            Improvement.ImprovementType.EssenceMax,
            Improvement.ImprovementType.Attributelevel,
            Improvement.ImprovementType.Seeker,
            Improvement.ImprovementType.ReplaceAttribute,
            Improvement.ImprovementType.EssencePenalty,
            Improvement.ImprovementType.EssencePenaltyT100,
            Improvement.ImprovementType.EssencePenaltyMAGOnlyT100,
            Improvement.ImprovementType.CyborgEssence,
            Improvement.ImprovementType.FreeSpellsATT,
            Improvement.ImprovementType.AddLimb,
            Improvement.ImprovementType.CyberwareEssCost,
            Improvement.ImprovementType.CyberwareTotalEssMultiplier,
            Improvement.ImprovementType.BiowareEssCost,
            Improvement.ImprovementType.BiowareTotalEssMultiplier,
            Improvement.ImprovementType.BasicBiowareEssCost,
            Improvement.ImprovementType.AttributeKarmaCostMultiplier,
            Improvement.ImprovementType.AttributeKarmaCost,
            Improvement.ImprovementType.AttributePointCostMultiplier,
            Improvement.ImprovementType.AttributePointCost,
        };

        //To get when things change in improvementmanager
        //Ugly, ugly done, but we cannot get events out of it today
        // FUTURE REFACTOR HERE
        [Obsolete("Refactor this method away once improvementmanager gets outbound events")]
        internal void ImprovementHook(ICollection<Improvement> _lstTransaction)
        {
            if (_lstTransaction.Any(x => AttribRelatedImprovements.Contains(x.ImproveType)))
            {
                ResetCachedEssence();
                AttributeImprovementEvent?.Invoke(_lstTransaction);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanAffordCareerPP)));
            }
            else if (_lstTransaction.Any(x => SkillRelatedImprovements.Contains(x.ImproveType)))
            {
                SkillImprovementEvent?.Invoke(_lstTransaction);
            }
        }

#region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _objOptions?.Dispose();
                }
                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
        }
#endregion

    }
}
