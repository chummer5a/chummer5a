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
using Chummer.Backend.Attributes;
using Chummer.Backend.Equipment;
using Chummer.Backend.Skills;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

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
    [DebuggerDisplay("{CharacterName} ({FileName})")]
    public sealed class Character : INotifyPropertyChanged, IHasMugshots, IHasName
    {
        private XmlNode _oldSkillsBackup;
        private XmlNode _oldSkillGroupBackup;

        private readonly CharacterOptions _objOptions;

        private string _strFileName = string.Empty;
        private DateTime _dateFileLastWriteTime = DateTime.MinValue;
        private string _strSettingsFileName = "default.xml";
        private bool _blnIgnoreRules;
        private int _intKarma;
        private int _intTotalKarma;
        private int _intStreetCred;
        private int _intNotoriety;
        private int _intPublicAwareness;
        private int _intBurntStreetCred;
        private decimal _decNuyen;
        private decimal _decStartingNuyen;
        private int _intMaxAvail = 12;
        private decimal _decEssenceAtSpecialStart = decimal.MinValue;
        private int _intSpecial;
        private int _intTotalSpecial;
        private int _intAttributes;
        private int _intTotalAttributes;
        private int _intSpellLimit;
        private int _intCFPLimit;
        private int _intAINormalProgramLimit;
        private int _intAIAdvancedProgramLimit;
        private int _intContactPoints;
        private int _intContactPointsUsed;
        private int _intRedlinerBonus;

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
        public static ReadOnlyCollection<string> LimbStrings => Array.AsReadOnly(s_LstLimbStrings);

        // AI Home Node

        // Active Commlink

        // If true, the Character creation has been finalized and is maintained through Karma.
        private bool _blnCreated;

        // Build Points
        private int _intSumtoTen = 10;
        private decimal _decNuyenMaximumBP = 50;
        private decimal _decNuyenBP;
        private int _intBuildKarma = 800;
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
        private int _intMetatypeBP;

        // Special Flags.

        private bool _blnAdeptEnabled;
        private bool _blnMagicianEnabled;
        private bool _blnTechnomancerEnabled;
        private bool _blnAdvancedProgramsEnabled;
        private bool _blnCyberwareDisabled;
        private bool _blnInitiationEnabled;
        private bool _blnCritterEnabled;
        private bool _blnIsCritter;
        private bool _blnPossessed;
        private bool _blnBlackMarketDiscount;
        private bool _blnFriendsInHighPlaces;
        private bool _blnExCon;
        private bool _blnRestrictedGear;
        private bool _blnOverclocker;
        private bool _blnMadeMan;
        private bool _blnFame;
        private bool _blnBornRich;
        private bool _blnErased;
		private int _intTrustFund;
		private decimal _decPrototypeTranshuman;
        private bool _blnMAGEnabled;
        private bool _blnRESEnabled;
        private bool _blnDEPEnabled;
        private bool _blnGroupMember;
        private string _strGroupName = string.Empty;
        private string _strGroupNotes = string.Empty;
        private int _intInitiateGrade;
        private int _intSubmersionGrade;

        // Pseudo-Attributes use for Mystic Adepts.
        private int _intMAGMagician;
        private int _intMAGAdept;

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
        private int _intPhysicalCMFilled;
        private int _intStunCMFilled;

        // Priority Selections.
        private string _strGameplayOption = "Standard";
        private string _strPriorityMetatype = "A,4";
        private string _strPriorityAttributes = "B,3";
        private string _strPrioritySpecial = "C,2";
        private string _strPrioritySkills = "D,1";
        private string _strPriorityResources = "E,0";
        private string _strPriorityTalent = string.Empty;
        private readonly List<string> _lstPrioritySkills = new List<string>();
        private decimal _decMaxNuyen;
        private int _intMaxKarma;
        private int _intContactMultiplier;

        // Lists.
        private readonly List<string> _lstSources = new List<string>();
        private readonly ObservableCollection<Improvement> _lstImprovements = new ObservableCollection<Improvement>();
        private readonly List<MentorSpirit> _lstMentorSpirits = new List<MentorSpirit>();
        private readonly ObservableCollection<Contact> _lstContacts = new ObservableCollection<Contact>();
        private readonly ObservableCollection<Spirit> _lstSpirits = new ObservableCollection<Spirit>();
        private readonly ObservableCollection<Spell> _lstSpells = new ObservableCollection<Spell>();
        private readonly List<Focus> _lstFoci = new List<Focus>();
        private readonly List<StackedFocus> _lstStackedFoci = new List<StackedFocus>();
        private readonly CachedBindingList<Power> _lstPowers = new CachedBindingList<Power>();
        private readonly ObservableCollection<ComplexForm> _lstComplexForms = new ObservableCollection<ComplexForm>();
        private readonly ObservableCollection<AIProgram> _lstAIPrograms = new ObservableCollection<AIProgram>();
        private readonly ObservableCollection<MartialArt> _lstMartialArts = new ObservableCollection<MartialArt>();
        #if LEGACY
        private List<MartialArtManeuver> _lstMartialArtManeuvers = new List<MartialArtManeuver>();
        #endif
        private readonly ObservableCollection<LimitModifier> _lstLimitModifiers = new ObservableCollection<LimitModifier>();
        private readonly ObservableCollection<Armor> _lstArmor = new ObservableCollection<Armor>();
        private readonly ObservableCollection<Cyberware> _lstCyberware = new ObservableCollection<Cyberware>();
        private readonly ObservableCollection<Weapon> _lstWeapons = new ObservableCollection<Weapon>();
        private readonly ObservableCollection<Quality> _lstQualities = new ObservableCollection<Quality>();
        private readonly ObservableCollection<Lifestyle> _lstLifestyles = new ObservableCollection<Lifestyle>();
        private readonly ObservableCollection<Gear> _lstGear = new ObservableCollection<Gear>();
        private readonly ObservableCollection<Vehicle> _lstVehicles = new ObservableCollection<Vehicle>();
        private readonly ObservableCollection<Metamagic> _lstMetamagics = new ObservableCollection<Metamagic>();
        private readonly ObservableCollection<Art> _lstArts = new ObservableCollection<Art>();
        private readonly ObservableCollection<Enhancement> _lstEnhancements = new ObservableCollection<Enhancement>();
        private readonly ObservableCollection<ExpenseLogEntry> _lstExpenseLog = new ObservableCollection<ExpenseLogEntry>();
        private readonly ObservableCollection<CritterPower> _lstCritterPowers = new ObservableCollection<CritterPower>();
        private readonly ObservableCollection<InitiationGrade> _lstInitiationGrades = new ObservableCollection<InitiationGrade>();
        private readonly ObservableCollection<string> _lstGearLocations = new ObservableCollection<string>();
        private readonly ObservableCollection<string> _lstArmorLocations = new ObservableCollection<string>();
        private readonly ObservableCollection<string> _lstVehicleLocations = new ObservableCollection<string>();
        private readonly ObservableCollection<string> _lstWeaponLocations = new ObservableCollection<string>();
        private readonly ObservableCollection<string> _lstImprovementGroups = new ObservableCollection<string>();
        private readonly BindingList<CalendarWeek> _lstCalendar = new BindingList<CalendarWeek>();
        //private List<LifeModule> _lstLifeModules = new List<LifeModule>();
        private readonly List<string> _lstInternalIdsNeedingReapplyImprovements = new List<string>();

        // Character Version
        private string _strVersionCreated = Application.ProductVersion.FastEscapeOnceFromStart("0.0.");
        Version _verSavedVersion = new Version();
        // Events.
        public event EventHandler AdeptTabEnabledChanged;
        public event EventHandler AmbidextrousChanged;
        public event EventHandler CritterTabEnabledChanged;
        public event EventHandler MAGEnabledChanged;
        public event EventHandler BornRichChanged;
        public event EventHandler CharacterNameChanged;
        public event EventHandler ExConChanged;
        public event EventHandler InitiationTabEnabledChanged;
        public event EventHandler MadeManChanged;
        public event EventHandler MagicianTabEnabledChanged;
        public event EventHandler RESEnabledChanged;
        public event EventHandler DEPEnabledChanged;
        public event EventHandler RestrictedGearChanged;
        public event EventHandler TechnomancerTabEnabledChanged;
        public event EventHandler AdvancedProgramsTabEnabledChanged;
        public event EventHandler CyberwareTabDisabledChanged;

#region Initialization, Save, Load, Print, and Reset Methods
        /// <summary>
        /// Character.
        /// </summary>
        public Character()
        {
			_objOptions = new CharacterOptions(this);
			AttributeSection = new AttributeSection(this);
			AttributeSection.Reset();
            MAG.PropertyChanged += RefreshMaxSpiritForce;
            MAG.PropertyChanged += RefreshCanAffordCareerPP;
            RES.PropertyChanged += RefreshMaxSpriteLevel;

            SkillsSection = new SkillsSection(this);
			SkillsSection.Reset();

            _lstCyberware.CollectionChanged += CyberwareOnCollectionChanged;
            _lstArmor.CollectionChanged += ArmorOnCollectionChanged;

            STR.PropertyChanged += RefreshEncumbranceFromSTR;
        }

        private void ArmorOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            bool blnDoEncumbranceRefresh = false;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Armor objNewItem in e.NewItems)
                    {
                        if (objNewItem.Equipped)
                        {
                            blnDoEncumbranceRefresh = true;
                            break;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (Armor objOldItem in e.OldItems)
                    {
                        if (objOldItem.Equipped)
                        {
                            blnDoEncumbranceRefresh = true;
                            break;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (Armor objOldItem in e.OldItems)
                    {
                        if (objOldItem.Equipped)
                        {
                            blnDoEncumbranceRefresh = true;
                            break;
                        }
                    }

                    if (!blnDoEncumbranceRefresh)
                    {
                        foreach (Armor objNewItem in e.NewItems)
                        {
                            if (objNewItem.Equipped)
                            {
                                blnDoEncumbranceRefresh = true;
                                break;
                            }
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Reset:
                    blnDoEncumbranceRefresh = true;
                    break;
            }
            if (blnDoEncumbranceRefresh)
                RefreshEncumbrance();
        }

        private void CyberwareOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            bool blnDoCyberlimbAttributesRefresh = false;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    RefreshRedliner();
                    if (!Options.DontUseCyberlimbCalculation)
                    {
                        foreach (Cyberware objNewItem in e.NewItems)
                        {
                            if (objNewItem.Category == "Cyberlimb" && objNewItem.Parent == null && objNewItem.ParentVehicle == null &&
                                !string.IsNullOrWhiteSpace(objNewItem.LimbSlot) && !Options.ExcludeLimbSlot.Contains(objNewItem.LimbSlot))
                            {
                                blnDoCyberlimbAttributesRefresh = true;
                                break;
                            }
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Remove:
                    RefreshRedliner();
                    if (!Options.DontUseCyberlimbCalculation)
                    {
                        foreach (Cyberware objOldItem in e.OldItems)
                        {
                            if (objOldItem.Category == "Cyberlimb" && objOldItem.Parent == null && objOldItem.ParentVehicle == null &&
                                !string.IsNullOrWhiteSpace(objOldItem.LimbSlot) && !Options.ExcludeLimbSlot.Contains(objOldItem.LimbSlot))
                            {
                                blnDoCyberlimbAttributesRefresh = true;
                                break;
                            }
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Replace:
                    RefreshRedliner();
                    if (!Options.DontUseCyberlimbCalculation)
                    {
                        foreach (Cyberware objOldItem in e.OldItems)
                        {
                            if (objOldItem.Category == "Cyberlimb" && objOldItem.Parent == null && objOldItem.ParentVehicle == null &&
                                !string.IsNullOrWhiteSpace(objOldItem.LimbSlot) && !Options.ExcludeLimbSlot.Contains(objOldItem.LimbSlot))
                            {
                                blnDoCyberlimbAttributesRefresh = true;
                                break;
                            }
                        }

                        if (!blnDoCyberlimbAttributesRefresh)
                        {
                            foreach (Cyberware objNewItem in e.NewItems)
                            {
                                if (objNewItem.Category == "Cyberlimb" && objNewItem.Parent == null && objNewItem.ParentVehicle == null &&
                                    !string.IsNullOrWhiteSpace(objNewItem.LimbSlot) && !Options.ExcludeLimbSlot.Contains(objNewItem.LimbSlot))
                                {
                                    blnDoCyberlimbAttributesRefresh = true;
                                    break;
                                }
                            }
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Reset:
                    RefreshRedliner();
                    blnDoCyberlimbAttributesRefresh = !Options.DontUseCyberlimbCalculation;
                    break;
            }

            if (blnDoCyberlimbAttributesRefresh)
            {
                foreach (CharacterAttrib objCharacterAttrib in AttributeSection.AttributeList.Concat(AttributeSection.SpecialAttributeList))
                {
                    if (objCharacterAttrib.Abbrev == "AGI" || objCharacterAttrib.Abbrev == "STR")
                    {
                        objCharacterAttrib.OnPropertyChanged(nameof(CharacterAttrib.TotalValue));
                    }
                }
            }
        }

        public AttributeSection AttributeSection { get; }

        public bool IsSaving { get; set; }

	    /// <summary>
        /// Save the Character to an XML file. Returns true if successful.
        /// </summary>
        public bool Save(string strFileName = "")
	    {
	        if (IsSaving)
	            return false;
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
            objWriter.WriteElementString("appversion", Application.ProductVersion.FastEscapeOnceFromStart("0.0."));
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
            // <sumtoten />
            objWriter.WriteElementString("sumtoten", _intSumtoTen.ToString());
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
            objWriter.WriteElementString("totaless", Essence().ToString(GlobalOptions.InvariantCultureInfo));

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

            SkillsSection.Save(objWriter);

            //Write copy of old skill groups, to not totally fuck a file if error
            _oldSkillGroupBackup?.WriteTo(objWriter);
            _oldSkillsBackup?.WriteTo(objWriter);

            ///////////////////////////////////////////SKILLS

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
            foreach (string strItem in _objOptions.CustomDataDirectoryNames)
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
            catch (UnauthorizedAccessException)
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
            if (!File.Exists(_strFileName))
                return false;
            using (StreamReader sr = new StreamReader(_strFileName, Encoding.UTF8, true))
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
            Timekeeper.Start("load_char_misc");
            XmlNode objXmlCharacter = objXmlDocument.SelectSingleNode("/character");
            XPathNavigator xmlCharacterNavigator = objXmlDocument.GetFastNavigator().SelectSingleNode("/character");

            if (objXmlCharacter == null || xmlCharacterNavigator == null)
                return false;

            _dateFileLastWriteTime = File.GetLastWriteTimeUtc(_strFileName);

            xmlCharacterNavigator.TryGetBoolFieldQuickly("ignorerules", ref _blnIgnoreRules);
            xmlCharacterNavigator.TryGetBoolFieldQuickly("created", ref _blnCreated);

            ResetCharacter();

            // Get the game edition of the file if possible and make sure it's intended to be used with this version of the application.
            string strGameEdition = string.Empty;
            if (xmlCharacterNavigator.TryGetStringFieldQuickly("gameedition", ref strGameEdition) && !string.IsNullOrEmpty(strGameEdition) && strGameEdition != "SR5")
            {
                MessageBox.Show(LanguageManager.GetString("Message_IncorrectGameVersion_SR4", GlobalOptions.Language),
                    LanguageManager.GetString("MessageTitle_IncorrectGameVersion", GlobalOptions.Language), MessageBoxButtons.YesNo,
                    MessageBoxIcon.Error);
                return false;
            }

            string strVersion = string.Empty;
            //Check to see if the character was created in a version of Chummer later than the currently installed one.
            if (xmlCharacterNavigator.TryGetStringFieldQuickly("appversion", ref strVersion) && !string.IsNullOrEmpty(strVersion))
            {
                if (strVersion.StartsWith("0."))
                {
                    strVersion = strVersion.Substring(2);
                }
                Version.TryParse(strVersion, out _verSavedVersion);
            }
#if !DEBUG
                Version verCurrentversion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
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
            xmlCharacterNavigator.TryGetStringFieldQuickly("settings", ref _strSettingsFileName);

            // Load the character's settings file.
            if (!_objOptions.Load(_strSettingsFileName))
                return false;

            // Get the sourcebooks that were used to create the character and throw up a warning if there's a mismatch.
            string strMissingBooks = string.Empty;
            //Does the list of enabled books contain the current item?
            foreach (XPathNavigator xmlSourceNode in xmlCharacterNavigator.Select("sources/source"))
            {
                string strLoopString = xmlSourceNode.Value;
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

            // Get the sourcebooks that were used to create the character and throw up a warning if there's a mismatch.
            string strMissingSourceNames = string.Empty;
            //Does the list of enabled books contain the current item?
            foreach (XPathNavigator xmlDirectoryName in xmlCharacterNavigator.Select("customdatadirectorynames/directoryname"))
            {
                string strLoopString = xmlDirectoryName.Value;
                if (strLoopString.Length > 0 && !_objOptions.CustomDataDirectoryNames.Contains(strLoopString))
                {
                    strMissingSourceNames += strLoopString + ';' + Environment.NewLine;
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

            if (xmlCharacterNavigator.TryGetDecFieldQuickly("essenceatspecialstart", ref _decEssenceAtSpecialStart))
            {
                // fix to work around a mistake made when saving decimal values in previous versions.
                if (_decEssenceAtSpecialStart > ESS.MetatypeMaximum)
                    _decEssenceAtSpecialStart /= 10;
            }

            xmlCharacterNavigator.TryGetStringFieldQuickly("createdversion", ref _strVersionCreated);

            // Metatype information.
            xmlCharacterNavigator.TryGetStringFieldQuickly("metatype", ref _strMetatype);
            xmlCharacterNavigator.TryGetStringFieldQuickly("movement", ref _strMovement);

            xmlCharacterNavigator.TryGetStringFieldQuickly("walk", ref _strWalk);
            xmlCharacterNavigator.TryGetStringFieldQuickly("run", ref _strRun);
            xmlCharacterNavigator.TryGetStringFieldQuickly("sprint", ref _strSprint);

            _strRunAlt = xmlCharacterNavigator.SelectSingleNode("run/@alt")?.Value ?? string.Empty;
            _strWalkAlt = xmlCharacterNavigator.SelectSingleNode("walk/@alt")?.Value ?? string.Empty;
            _strSprintAlt = xmlCharacterNavigator.SelectSingleNode("sprint/@alt")?.Value ?? string.Empty;

            xmlCharacterNavigator.TryGetInt32FieldQuickly("metatypebp", ref _intMetatypeBP);
            xmlCharacterNavigator.TryGetStringFieldQuickly("metavariant", ref _strMetavariant);

            //Shim for characters created prior to Run Faster Errata
            if (_strMetavariant == "Cyclopean")
            {
                _strMetavariant = "Cyclops";
            }
            xmlCharacterNavigator.TryGetStringFieldQuickly("metatypecategory", ref _strMetatypeCategory);

            // General character information.
            xmlCharacterNavigator.TryGetStringFieldQuickly("name", ref _strName);
            LoadMugshots(xmlCharacterNavigator);
            xmlCharacterNavigator.TryGetStringFieldQuickly("sex", ref _strSex);
            xmlCharacterNavigator.TryGetStringFieldQuickly("age", ref _strAge);
            xmlCharacterNavigator.TryGetStringFieldQuickly("eyes", ref _strEyes);
            xmlCharacterNavigator.TryGetStringFieldQuickly("height", ref _strHeight);
            xmlCharacterNavigator.TryGetStringFieldQuickly("weight", ref _strWeight);
            xmlCharacterNavigator.TryGetStringFieldQuickly("skin", ref _strSkin);
            xmlCharacterNavigator.TryGetStringFieldQuickly("hair", ref _strHair);
            xmlCharacterNavigator.TryGetStringFieldQuickly("description", ref _strDescription);
            xmlCharacterNavigator.TryGetStringFieldQuickly("background", ref _strBackground);
            xmlCharacterNavigator.TryGetStringFieldQuickly("concept", ref _strConcept);
            xmlCharacterNavigator.TryGetStringFieldQuickly("notes", ref _strNotes);
            xmlCharacterNavigator.TryGetStringFieldQuickly("alias", ref _strAlias);
            xmlCharacterNavigator.TryGetStringFieldQuickly("playername", ref _strPlayerName);
            xmlCharacterNavigator.TryGetStringFieldQuickly("gamenotes", ref _strGameNotes);
            if (!xmlCharacterNavigator.TryGetStringFieldQuickly("primaryarm", ref _strPrimaryArm))
                _strPrimaryArm = "Right";

            if (!xmlCharacterNavigator.TryGetStringFieldQuickly("gameplayoption", ref _strGameplayOption))
            {
                if (xmlCharacterNavigator.TryGetInt32FieldQuickly("buildkarma", ref _intBuildKarma) && _intBuildKarma == 35)
                    _strGameplayOption = "Prime Runner";
                else
                    _strGameplayOption = "Standard";
            }

            xmlCharacterNavigator.TryGetField("buildmethod", Enum.TryParse, out _objBuildMethod);
            if (!xmlCharacterNavigator.TryGetDecFieldQuickly("maxnuyen", ref _decMaxNuyen) || _decMaxNuyen == 0)
                _decMaxNuyen = 25;
            xmlCharacterNavigator.TryGetInt32FieldQuickly("contactmultiplier", ref _intContactMultiplier);
            xmlCharacterNavigator.TryGetInt32FieldQuickly("sumtoten", ref _intSumtoTen);
            xmlCharacterNavigator.TryGetInt32FieldQuickly("buildkarma", ref _intBuildKarma);
            if (!xmlCharacterNavigator.TryGetInt32FieldQuickly("maxkarma", ref _intMaxKarma) || _intMaxKarma == 0)
                _intMaxKarma = _intBuildKarma;

            //Maximum number of Karma that can be spent/gained on Qualities.
            xmlCharacterNavigator.TryGetInt32FieldQuickly("gameplayoptionqualitylimit", ref _intGameplayOptionQualityLimit);

            xmlCharacterNavigator.TryGetDecFieldQuickly("nuyenmaxbp", ref _decNuyenMaximumBP);
            xmlCharacterNavigator.TryGetInt32FieldQuickly("maxavail", ref _intMaxAvail);

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

            xmlCharacterNavigator.TryGetStringFieldQuickly("prioritymetatype", ref _strPriorityMetatype);
            xmlCharacterNavigator.TryGetStringFieldQuickly("priorityattributes", ref _strPriorityAttributes);
            xmlCharacterNavigator.TryGetStringFieldQuickly("priorityspecial", ref _strPrioritySpecial);
            xmlCharacterNavigator.TryGetStringFieldQuickly("priorityskills", ref _strPrioritySkills);
            xmlCharacterNavigator.TryGetStringFieldQuickly("priorityresources", ref _strPriorityResources);
            xmlCharacterNavigator.TryGetStringFieldQuickly("prioritytalent", ref _strPriorityTalent);
            _lstPrioritySkills.Clear();
            foreach (XPathNavigator xmlSkillName in xmlCharacterNavigator.Select("priorityskills/priorityskill"))
            {
                _lstPrioritySkills.Add(xmlSkillName.Value);
            }
            BannedWareGrades.Clear();
            XPathNavigator xmlTempNode = xmlCharacterNavigator.SelectSingleNode("bannedwaregrades");
            if (xmlTempNode != null)
            {
                foreach (XPathNavigator xmlNode in xmlTempNode.Select("grade"))
                    BannedWareGrades.Add(xmlNode.Value);
            }
            else
            {
                foreach (XmlNode xmlNode in xmlGameplayOption.SelectNodes("bannedwaregrades/grade"))
                    BannedWareGrades.Add(xmlNode.InnerText);
            }
            string strSkill1 = string.Empty;
            string strSkill2 = string.Empty;
            if (xmlCharacterNavigator.TryGetStringFieldQuickly("priorityskill1", ref strSkill1) && !string.IsNullOrEmpty(strSkill1))
                _lstPrioritySkills.Add(strSkill1);
            if (xmlCharacterNavigator.TryGetStringFieldQuickly("priorityskill2", ref strSkill2) && !string.IsNullOrEmpty(strSkill2))
                _lstPrioritySkills.Add(strSkill2);

            xmlCharacterNavigator.TryGetBoolFieldQuickly("iscritter", ref _blnIsCritter);
            xmlCharacterNavigator.TryGetBoolFieldQuickly("possessed", ref _blnPossessed);

            xmlCharacterNavigator.TryGetInt32FieldQuickly("contactpoints", ref _intContactPoints);
            xmlCharacterNavigator.TryGetInt32FieldQuickly("contactpointsused", ref _intContactPointsUsed);
            xmlCharacterNavigator.TryGetInt32FieldQuickly("cfplimit", ref _intCFPLimit);
            xmlCharacterNavigator.TryGetInt32FieldQuickly("ainormalprogramlimit", ref _intAINormalProgramLimit);
            xmlCharacterNavigator.TryGetInt32FieldQuickly("aiadvancedprogramlimit", ref _intAIAdvancedProgramLimit);
            xmlCharacterNavigator.TryGetInt32FieldQuickly("spelllimit", ref _intSpellLimit);
            xmlCharacterNavigator.TryGetInt32FieldQuickly("karma", ref _intKarma);
            xmlCharacterNavigator.TryGetInt32FieldQuickly("totalkarma", ref _intTotalKarma);

            xmlCharacterNavigator.TryGetInt32FieldQuickly("special", ref _intSpecial);
            xmlCharacterNavigator.TryGetInt32FieldQuickly("totalspecial", ref _intTotalSpecial);
            //objXmlCharacter.tryGetInt32FieldQuickly("attributes", ref _intAttributes); //wonkey
            xmlCharacterNavigator.TryGetInt32FieldQuickly("totalattributes", ref _intTotalAttributes);
            xmlCharacterNavigator.TryGetInt32FieldQuickly("contactpoints", ref _intContactPoints);
            xmlCharacterNavigator.TryGetInt32FieldQuickly("contactpointsused", ref _intContactPointsUsed);
            xmlCharacterNavigator.TryGetInt32FieldQuickly("streetcred", ref _intStreetCred);
            xmlCharacterNavigator.TryGetInt32FieldQuickly("notoriety", ref _intNotoriety);
            xmlCharacterNavigator.TryGetInt32FieldQuickly("publicawareness", ref _intPublicAwareness);
            xmlCharacterNavigator.TryGetInt32FieldQuickly("burntstreetcred", ref _intBurntStreetCred);
            xmlCharacterNavigator.TryGetDecFieldQuickly("nuyen", ref _decNuyen);
            xmlCharacterNavigator.TryGetDecFieldQuickly("startingnuyen", ref _decStartingNuyen);
            xmlCharacterNavigator.TryGetDecFieldQuickly("nuyenbp", ref _decNuyenBP);
            
            xmlCharacterNavigator.TryGetBoolFieldQuickly("adept", ref _blnAdeptEnabled);
            xmlCharacterNavigator.TryGetBoolFieldQuickly("magician", ref _blnMagicianEnabled);
            xmlCharacterNavigator.TryGetBoolFieldQuickly("technomancer", ref _blnTechnomancerEnabled);
            xmlCharacterNavigator.TryGetBoolFieldQuickly("ai", ref _blnAdvancedProgramsEnabled);
            xmlCharacterNavigator.TryGetBoolFieldQuickly("cyberwaredisabled", ref _blnCyberwareDisabled);
            xmlCharacterNavigator.TryGetBoolFieldQuickly("initiationoverride", ref _blnInitiationEnabled);
            xmlCharacterNavigator.TryGetBoolFieldQuickly("critter", ref _blnCritterEnabled);

            xmlCharacterNavigator.TryGetBoolFieldQuickly("friendsinhighplaces", ref _blnFriendsInHighPlaces);
            xmlCharacterNavigator.TryGetDecFieldQuickly("prototypetranshuman", ref _decPrototypeTranshuman);
            xmlCharacterNavigator.TryGetBoolFieldQuickly("blackmarketdiscount", ref _blnBlackMarketDiscount);
            xmlCharacterNavigator.TryGetBoolFieldQuickly("excon", ref _blnExCon);
            xmlCharacterNavigator.TryGetInt32FieldQuickly("trustfund", ref _intTrustFund);
            xmlCharacterNavigator.TryGetBoolFieldQuickly("restrictedgear", ref _blnRestrictedGear);
            xmlCharacterNavigator.TryGetBoolFieldQuickly("overclocker", ref _blnOverclocker);
            xmlCharacterNavigator.TryGetBoolFieldQuickly("mademan", ref _blnMadeMan);
            xmlCharacterNavigator.TryGetBoolFieldQuickly("fame", ref _blnFame);
            xmlCharacterNavigator.TryGetBoolFieldQuickly("ambidextrous", ref _blnAmbidextrous);
            xmlCharacterNavigator.TryGetBoolFieldQuickly("bornrich", ref _blnBornRich);
            xmlCharacterNavigator.TryGetBoolFieldQuickly("erased", ref _blnErased);
            xmlCharacterNavigator.TryGetBoolFieldQuickly("magenabled", ref _blnMAGEnabled);
            xmlCharacterNavigator.TryGetInt32FieldQuickly("initiategrade", ref _intInitiateGrade);
            xmlCharacterNavigator.TryGetBoolFieldQuickly("resenabled", ref _blnRESEnabled);
            xmlCharacterNavigator.TryGetInt32FieldQuickly("submersiongrade", ref _intSubmersionGrade);
            xmlCharacterNavigator.TryGetBoolFieldQuickly("depenabled", ref _blnDEPEnabled);
            // Legacy shim
            if (!_blnCreated && !_blnMAGEnabled && !_blnRESEnabled && !_blnDEPEnabled)
                _decEssenceAtSpecialStart = decimal.MinValue;
            xmlCharacterNavigator.TryGetBoolFieldQuickly("groupmember", ref _blnGroupMember);
            xmlCharacterNavigator.TryGetStringFieldQuickly("groupname", ref _strGroupName);
            xmlCharacterNavigator.TryGetStringFieldQuickly("groupnotes", ref _strGroupNotes);
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
            // Orphaned improvements shouldn't be getting created after 5.198. If this is proven incorrect, bump up the version here.
            bool blnDoCheckForOrphanedImprovements = LastSavedVersion < new Version("5.198.0");
            foreach (XmlNode objXmlImprovement in objXmlNodeList)
            {
                string strImprovementSource = objXmlImprovement["improvementsource"]?.InnerText;
                // Do not load condition monitor improvements from older versions of Chummer
                if (strImprovementSource == "ConditionMonitor")
                    continue;

                // Do not load essence loss improvements if this character does not have any attributes affected by essence loss
                if (_decEssenceAtSpecialStart == decimal.MinValue && (strImprovementSource == "EssenceLoss" || strImprovementSource == "EssenceLossChargen"))
                    continue;

                if (blnDoCheckForOrphanedImprovements)
                {
                    string strLoopSourceName = objXmlImprovement["sourcename"]?.InnerText;
                    if (!string.IsNullOrEmpty(strLoopSourceName) && strLoopSourceName.IsGuid() && objXmlImprovement["custom"]?.InnerText != bool.TrueString)
                    {
                        // Hacky way to make sure this character isn't loading in any orphaned improvements.
                        // SourceName ID will pop up minimum twice in the save if the improvement's source is actually present: once in the improvement and once in the parent that added it.
                        if (strCharacterInnerXml.IndexOf(strLoopSourceName, StringComparison.Ordinal) == strCharacterInnerXml.LastIndexOf(strLoopSourceName, StringComparison.Ordinal))
                            continue;
                    }
                }
                
                Improvement objImprovement = new Improvement(this);
                try
                {
                    objImprovement.Load(objXmlImprovement);
                    _lstImprovements.Add(objImprovement);

                    if (objImprovement.ImproveType == Improvement.ImprovementType.SkillsoftAccess && objImprovement.Value == 0)
                    {
                        _lstInternalIdsNeedingReapplyImprovements.Add(objImprovement.SourceName);
                    }
                }
                catch (ArgumentException)
                {
                    _lstInternalIdsNeedingReapplyImprovements.Add(objXmlImprovement["sourcename"]?.InnerText);
                }
            }
            Timekeeper.Finish("load_char_imp");
            Timekeeper.Start("load_char_quality");
            // Qualities
            Quality objLivingPersonaQuality = null;
            objXmlNodeList = objXmlCharacter.SelectNodes("qualities/quality");
            bool blnHasOldQualities = false;
            XmlNode xmlRootQualitiesNode = XmlManager.Load("qualities.xml").SelectSingleNode("/chummer/qualities");
            foreach (XmlNode objXmlQuality in objXmlNodeList)
            {
                if (objXmlQuality["name"] != null)
                {
                    if (!CorrectedUnleveledQuality(objXmlQuality, xmlRootQualitiesNode))
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
            // If old Qualities are in use, they need to be converted before loading can continue.
            if (blnHasOldQualities)
                ConvertOldQualities(objXmlNodeList);
	        Timekeeper.Finish("load_char_quality");
			AttributeSection.Load(objXmlCharacter);
			Timekeeper.Start("load_char_misc2");

            // Attempt to load the split MAG CharacterAttribute information for Mystic Adepts.
            if (_blnAdeptEnabled && _blnMagicianEnabled)
            {
                xmlCharacterNavigator.TryGetInt32FieldQuickly("magsplitadept", ref _intMAGAdept);
                xmlCharacterNavigator.TryGetInt32FieldQuickly("magsplitmagician", ref _intMAGMagician);
            }

            // Attempt to load the Magic Tradition.
            xmlCharacterNavigator.TryGetStringFieldQuickly("tradition", ref _strMagicTradition);
            // Attempt to load the Magic Tradition Drain Attributes.
            string strTemp = string.Empty;
            if (xmlCharacterNavigator.TryGetStringFieldQuickly("traditiondrain", ref strTemp))
            {
                TraditionDrain = strTemp;
            }
            // Attempt to load the Magic Tradition Name.
            xmlCharacterNavigator.TryGetStringFieldQuickly("traditionname", ref _strTraditionName);
            // Attempt to load the Spirit Combat Name.
            xmlCharacterNavigator.TryGetStringFieldQuickly("spiritcombat", ref _strSpiritCombat);
            // Attempt to load the Spirit Detection Name.
            xmlCharacterNavigator.TryGetStringFieldQuickly("spiritdetection", ref _strSpiritDetection);
            // Attempt to load the Spirit Health Name.
            xmlCharacterNavigator.TryGetStringFieldQuickly("spirithealth", ref _strSpiritHealth);
            // Attempt to load the Spirit Illusion Name.
            xmlCharacterNavigator.TryGetStringFieldQuickly("spiritillusion", ref _strSpiritIllusion);
            // Attempt to load the Spirit Manipulation Name.
            xmlCharacterNavigator.TryGetStringFieldQuickly("spiritmanipulation", ref _strSpiritManipulation);
            // Attempt to load the Technomancer Stream.
            xmlCharacterNavigator.TryGetStringFieldQuickly("stream", ref _strTechnomancerStream);
            // Attempt to load the Technomancer Stream's Fading attributes.
            if (xmlCharacterNavigator.TryGetStringFieldQuickly("streamfading", ref strTemp))
            {
                TechnomancerFading = strTemp;
            }

            // Attempt to load Condition Monitor Progress.
            xmlCharacterNavigator.TryGetInt32FieldQuickly("physicalcmfilled", ref _intPhysicalCMFilled);
            xmlCharacterNavigator.TryGetInt32FieldQuickly("stuncmfilled", ref _intStunCMFilled);
            Timekeeper.Finish("load_char_misc2");
            Timekeeper.Start("load_char_skills");  //slightly messy

            _oldSkillsBackup = objXmlCharacter.SelectSingleNode("skills")?.Clone();
            _oldSkillGroupBackup = objXmlCharacter.SelectSingleNode("skillgroups")?.Clone();

            XmlNode objSkillNode = objXmlCharacter.SelectSingleNode("newskills");
            if (objSkillNode != null)
            {
                SkillsSection.Load(objSkillNode);
            }
            else
            {
                SkillsSection.Load(objXmlCharacter, true);
            }

            Timekeeper.Start("load_char_contacts");


            // Contacts.
            foreach (XPathNavigator xmlContact in xmlCharacterNavigator.Select("contacts/contact"))
            {
                Contact objContact = new Contact(this);
                objContact.Load(xmlContact);
                _lstContacts.Add(objContact);
            }

            Timekeeper.Finish("load_char_contacts");

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
                if ((objCyberware.Name == "Myostatin Inhibitor" && LastSavedVersion <= new Version("5.195.1") && !Improvements.Any(x => x.SourceName == objCyberware.InternalId && x.ImproveType == Improvement.ImprovementType.AttributeKarmaCost)) ||
                    (objCyberware.PairBonus?.HasChildNodes == true && Improvements.All(x => x.SourceName != objCyberware.InternalId + "Pair")))
                {
                    XmlNode objNode = objCyberware.GetNode();
                    if (objNode != null)
                    {
                        ImprovementManager.RemoveImprovements(this, objCyberware.SourceType, objCyberware.InternalId);
                        ImprovementManager.RemoveImprovements(this, objCyberware.SourceType, objCyberware.InternalId + "Pair");
                        objCyberware.Bonus = objNode["bonus"];
                        objCyberware.WirelessBonus = objNode["wirelessbonus"];
                        objCyberware.PairBonus = objNode["pairbonus"];
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
                        if (!objCyberware.IsModularCurrentlyEquipped)
                            objCyberware.ChangeModularEquip(false);
                        else if (objCyberware.PairBonus != null)
                        {
                            Cyberware objMatchingCyberware = dicPairableCyberwares.Keys.FirstOrDefault(x => x.Name == objCyberware.Name && x.Extra == objCyberware.Extra);
                            if (objMatchingCyberware != null)
                                dicPairableCyberwares[objMatchingCyberware] = dicPairableCyberwares[objMatchingCyberware] + 1;
                            else
                                dicPairableCyberwares.Add(objCyberware, 1);
                        }
                    }
                    else
                    {
                        _lstInternalIdsNeedingReapplyImprovements.Add(objCyberware.InternalId);
                    }
                }
            }
            // Separate Pass for PairBonuses
            foreach (KeyValuePair<Cyberware, int> objItem in dicPairableCyberwares)
            {
                Cyberware objCyberware = objItem.Key;
                int intCyberwaresCount = objItem.Value;
                List<Cyberware> lstPairableCyberwares = Cyberware.DeepWhere(x => x.Children, x => objCyberware.IncludePair.Contains(x.Name) && x.Extra == objCyberware.Extra && x.IsModularCurrentlyEquipped).ToList();
                // Need to use slightly different logic if this cyberware has a location (Left or Right) and only pairs with itself because Lefts can only be paired with Rights and Rights only with Lefts
                if (!string.IsNullOrEmpty(objCyberware.Location) && objCyberware.IncludePair.All(x => x == objCyberware.Name))
                {
                    int intMatchLocationCount = 0;
                    int intNotMatchLocationCount = 0;
                    foreach (Cyberware objPairableCyberware in lstPairableCyberwares)
                    {
                        if (objPairableCyberware.Location != objCyberware.Location)
                            intNotMatchLocationCount += 1;
                        else
                            intMatchLocationCount += 1;
                    }
                    // Set the count to the total number of cyberwares in matching pairs, which would mean 2x the number of whichever location contains the fewest members (since every single one of theirs would have a pair)
                    intCyberwaresCount = Math.Min(intNotMatchLocationCount, intMatchLocationCount) * 2;
                }
                if (intCyberwaresCount > 0)
                {
                    foreach (Cyberware objLoopCyberware in lstPairableCyberwares)
                    {
                        if (intCyberwaresCount % 2 == 0)
                        {
                            if (!string.IsNullOrEmpty(objCyberware.Forced) && objCyberware.Forced != "Right" && objCyberware.Forced != "Left")
                                ImprovementManager.ForcedValue = objCyberware.Forced;
                            ImprovementManager.CreateImprovements(this, objLoopCyberware.SourceType, objLoopCyberware.InternalId + "Pair", objLoopCyberware.PairBonus, false, objLoopCyberware.Rating, objLoopCyberware.DisplayNameShort(GlobalOptions.Language));
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
            foreach (XPathNavigator xmlSpirit in xmlCharacterNavigator.Select("spirits/spirit"))
            {
                Spirit objSpirit = new Spirit(this);
                objSpirit.Load(xmlSpirit);
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
            // If the character has a technomancer quality but no Living Persona commlink, its improvements get re-applied immediately
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
            // While expenses are to be saved in create mode due to starting nuyen and starting karma being logged as expense log entries,
            // they shouldn't get loaded in create mode because they shouldn't be there.
            if (Created)
            {
                Timekeeper.Start("load_char_elog");

                // Expense Log Entries.
                XmlNodeList objXmlExpenseList = objXmlCharacter.SelectNodes("expenses/expense");
                foreach (XmlNode objXmlExpense in objXmlExpenseList)
                {
                    ExpenseLogEntry objExpenseLogEntry = new ExpenseLogEntry(this);
                    objExpenseLogEntry.Load(objXmlExpense);
                    _lstExpenseLog.AddWithSort(objExpenseLogEntry);
                }

                Timekeeper.Finish("load_char_elog");
            }
#if DEBUG
            else
            {
                // There shouldn't be any expenses for a character loaded in create mode. This code is to help narrow down issues should expenses somehow be created.
                XmlNodeList objXmlExpenseList = objXmlCharacter.SelectNodes("expenses/expense");
                if (objXmlExpenseList?.Count > 0)
                {
                    Utils.BreakIfDebug();
                }
            }
#endif
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
                _lstCalendar.AddWithSort(objWeek, true);
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
                        XmlNode objXmlDwarfQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Resistance to Pathogens/Toxins\"]") ??
                                                     xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Dwarf Resistance\"]");

                        List<Weapon> lstWeapons = new List<Weapon>();
                        Quality objQuality = new Quality(this);

                        objQuality.Create(objXmlDwarfQuality, QualitySource.Metatype, lstWeapons);
                        foreach (Weapon objWeapon in lstWeapons)
                            _lstWeapons.Add(objWeapon);
                        _lstQualities.Add(objQuality);
                    }
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
                    string strContactMultiplier = _objOptions.FreeContactsMultiplierEnabled ? _objOptions.FreeContactsMultiplier.ToString() : objXmlGameplayOption["contactmultiplier"]?.InnerText;
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
                // This character doesn't have any improvements tied to a cached Mentor Spirit value, so re-apply the improvement that adds the Mentor spirit
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

            // Refresh certain improvements
            Timekeeper.Finish("load_char_improvementrefreshers");
            // Refresh permanent attribute changes due to essence loss
            RefreshEssenceLossImprovements();
            // Refresh dicepool modifiers due to filled condition monitor boxes
            RefreshWoundPenalties();
            Timekeeper.Finish("load_char_improvementrefreshers");

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
        /// <param name="objWriter">XmlTextWriter to write to.</param>
        /// <param name="objCulture">Culture in which to print.</param>
        /// <param name="strLanguageToPrint">Language in which to print.</param>
#if DEBUG
        /// <param name="objStream">MemoryStream to use.</param>
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
            objWriter.WriteElementString("name", !string.IsNullOrEmpty(Name) ? Name : LanguageManager.GetString("String_UnnamedCharacter", strLanguageToPrint));

            PrintMugshots(objWriter);

            // <sex />
            objWriter.WriteElementString("sex", LanguageManager.TranslateExtra(LanguageManager.ReverseTranslateExtra(Sex, GlobalOptions.Language), strLanguageToPrint));
            // <age />
            objWriter.WriteElementString("age", LanguageManager.TranslateExtra(LanguageManager.ReverseTranslateExtra(Age, GlobalOptions.Language), strLanguageToPrint));
            // <eyes />
            objWriter.WriteElementString("eyes", LanguageManager.TranslateExtra(LanguageManager.ReverseTranslateExtra(Eyes, GlobalOptions.Language), strLanguageToPrint));
            // <height />
            objWriter.WriteElementString("height", LanguageManager.TranslateExtra(LanguageManager.ReverseTranslateExtra(Height, GlobalOptions.Language), strLanguageToPrint));
            // <weight />
            objWriter.WriteElementString("weight", LanguageManager.TranslateExtra(LanguageManager.ReverseTranslateExtra(Weight, GlobalOptions.Language), strLanguageToPrint));
            // <skin />
            objWriter.WriteElementString("skin", LanguageManager.TranslateExtra(LanguageManager.ReverseTranslateExtra(Skin, GlobalOptions.Language), strLanguageToPrint));
            // <hair />
            objWriter.WriteElementString("hair", LanguageManager.TranslateExtra(LanguageManager.ReverseTranslateExtra(Hair, GlobalOptions.Language), strLanguageToPrint));
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

            objWriter.WriteElementString("totaless", Essence().ToString(strESSFormat, objCulture));
            // <tradition />
            string strTraditionName = MagicTradition;
            if (strTraditionName == "Custom")
                strTraditionName = TraditionName;
            objWriter.WriteStartElement("tradition");

            if (!string.IsNullOrEmpty(strTraditionName))
            {
                XmlDocument xmlTraditions = XmlManager.Load("traditions.xml", strLanguageToPrint);
                XmlNode objXmlTradition = xmlTraditions.SelectSingleNode("/chummer/traditions/tradition[name = \"" + MagicTradition + "\"]");

                string strName = MagicTradition;
                if (!string.IsNullOrEmpty(strName) && strName != "Custom")
                {
                    strTraditionName = objXmlTradition?["translate"]?.InnerText ?? strName;
                }
                objWriter.WriteElementString("drainattributes", DisplayTraditionDrainMethod(strLanguageToPrint));
                objWriter.WriteElementString("drain", TraditionDrainValue.ToString(objCulture));

                string strSpiritCombat = SpiritCombat;
                string strSpiritDetection = SpiritDetection;
                string strSpiritHealth = SpiritHealth;
                string strSpiritIllusion = SpiritIllusion;
                string strSpiritManipulation = SpiritManipulation;
                string strNone = LanguageManager.GetString("String_None", strLanguageToPrint);
                if (MagicTradition != "Custom")
                {
                    if (objXmlTradition == null)
                    {
                        strSpiritCombat = strNone;
                        strSpiritDetection = strNone;
                        strSpiritHealth = strNone;
                        strSpiritIllusion = strNone;
                        strSpiritManipulation = strNone;
                        
                    }
                    else
                    {
                        strSpiritCombat = objXmlTradition.SelectSingleNode("spirits/spiritcombat")?.InnerText ??
                                          strNone;
                        if (strSpiritCombat == "All")
                            strSpiritCombat = LanguageManager.GetString("String_All", strLanguageToPrint);
                        strSpiritDetection = objXmlTradition.SelectSingleNode("spirits/spiritdetection")?.InnerText ??
                                             strNone;
                        if (strSpiritDetection == "All")
                            strSpiritDetection = LanguageManager.GetString("String_All", strLanguageToPrint);
                        strSpiritHealth = objXmlTradition.SelectSingleNode("spirits/spirithealth")?.InnerText ??
                                          strNone;
                        if (strSpiritHealth == "All")
                            strSpiritHealth = LanguageManager.GetString("String_All", strLanguageToPrint);
                        strSpiritIllusion = objXmlTradition.SelectSingleNode("spirits/spiritillusion")?.InnerText ??
                                            strNone;
                        if (strSpiritIllusion == "All")
                            strSpiritIllusion = LanguageManager.GetString("String_All", strLanguageToPrint);
                        strSpiritManipulation =
                            objXmlTradition.SelectSingleNode("spirits/spiritmanipulation")?.InnerText ?? strNone;
                        if (strSpiritManipulation == "All")
                            strSpiritManipulation = LanguageManager.GetString("String_All", strLanguageToPrint);
                    }
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
                objWriter.WriteElementString("drainattributes", DisplayTechnomancerFadingMethod(strLanguageToPrint));
                objWriter.WriteElementString("drain", TechnomancerFadingValue.ToString(objCulture));
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

            bool blnIsAI = DEPEnabled && BOD.MetatypeMaximum == 0;
            bool blnPhysicalTrackIsCore = blnIsAI && !(HomeNode is Vehicle);
            // Condition Monitors.
            // <physicalcm />
            int intPhysicalCM = PhysicalCM;
            objWriter.WriteElementString("physicalcm", intPhysicalCM.ToString(objCulture));
            objWriter.WriteElementString("physicalcmiscorecm", blnPhysicalTrackIsCore.ToString(GlobalOptions.InvariantCultureInfo));
            // <stuncm />
            int intStunCM = StunCM;
            objWriter.WriteElementString("stuncm", intStunCM.ToString(objCulture));
            objWriter.WriteElementString("stuncmismatrixcm", blnIsAI.ToString(GlobalOptions.InvariantCultureInfo));

            // Condition Monitor Progress.
            // <physicalcmfilled />
            objWriter.WriteElementString("physicalcmfilled", PhysicalCMFilled.ToString(objCulture));
            // <stuncmfilled />
            objWriter.WriteElementString("stuncmfilled", StunCMFilled.ToString(objCulture));

            // <cmthreshold>
            objWriter.WriteElementString("cmthreshold", CMThreshold.ToString(objCulture));
            // <cmthresholdoffset>
            objWriter.WriteElementString("physicalcmthresholdoffset", Math.Min(PhysicalCMThresholdOffset, intPhysicalCM).ToString(objCulture));
            // <cmthresholdoffset>
            objWriter.WriteElementString("stuncmthresholdoffset", Math.Min(StunCMThresholdOffset, intStunCM).ToString(objCulture));
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
            foreach (Improvement objImprovement in Improvements.Where(objImprovement => (objImprovement.ImproveType == Improvement.ImprovementType.LimitModifier && objImprovement.ImprovedName == "Physical" && objImprovement.Enabled)))
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
            foreach (Improvement objImprovement in Improvements.Where(objImprovement => (objImprovement.ImproveType == Improvement.ImprovementType.LimitModifier && objImprovement.ImprovedName == "Mental" && objImprovement.Enabled)))
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
            foreach (Improvement objImprovement in Improvements.Where(objImprovement => (objImprovement.ImproveType == Improvement.ImprovementType.LimitModifier && objImprovement.ImprovedName == "Social" && objImprovement.Enabled)))
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
            objWriter.WriteStartElement("qualities");
            foreach (Quality objQuality in Qualities)
            {
                string strKey = objQuality.QualityId + '|' + objQuality.SourceName + '|' + objQuality.Extra;
                if (strQualitiesToPrint.TryGetValue(strKey, out int intLoopRating))
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

            // <initiationgrades>
            objWriter.WriteStartElement("initiationgrades");
            foreach (InitiationGrade objgrade in InitiationGrades)
            {
                objgrade.Print(objWriter, strLanguageToPrint);

                //TODO: Probably better to integrate this into the main print method, but eh. 
                // <metamagics>
                objWriter.WriteStartElement("metamagics");
                foreach (Metamagic objMetamagic in Metamagics.Where(objMetamagic => objMetamagic.Grade == objgrade.Grade))
                {
                    objMetamagic.Print(objWriter, objCulture, strLanguageToPrint);
                }
                // </metamagics>
                objWriter.WriteEndElement();

                // <arts>
                objWriter.WriteStartElement("arts");
                foreach (Art objArt in Arts.Where(objArt => objArt.Grade == objgrade.Grade))
                {
                    objArt.Print(objWriter, strLanguageToPrint);
                }
                // </arts>
                objWriter.WriteEndElement();

                // <enhancements>
                objWriter.WriteStartElement("enhancements");
                foreach (Enhancement objEnhancement in Enhancements.Where(objEnhancement => objEnhancement.Grade == objgrade.Grade))
                {
                    objEnhancement.Print(objWriter, strLanguageToPrint);
                }
                // </enhancements>
                objWriter.WriteEndElement();
            }
            // </initiationgrade>
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
                foreach (ExpenseLogEntry objExpense in ExpenseEntries.Reverse())
                    objExpense.Print(objWriter, objCulture, strLanguageToPrint);
                // </expenses>
                objWriter.WriteEndElement();
            }

            // </character>
            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Remove stray event handlers and clear all info used by this character
        /// </summary>
        public void DeleteCharacter()
        {
            ImprovementManager.ClearCachedValues(this);
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
            _lstCalendar.Clear();

            _lstMugshots.Clear();

            _lstLinkedCharacters.Clear();

            SkillsSection.UnbindSkillsSection();
            AttributeSection.UnbindAttributeSection();
        }

        /// <summary>
        /// Reset all of the Character information and start from scratch.
        /// </summary>
        private void ResetCharacter()
        {
            _intBuildKarma = 800;
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
            ImprovementManager.ClearCachedValues(this);
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
            _lstCalendar.Clear();

            SkillsSection.Reset();

            _intMainMugshotIndex = -1;
            _lstMugshots.Clear();

            _lstLinkedCharacters.Clear();
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
        /// <param name="strLanguage">Language in which to fetch name.</param>
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
                case Improvement.ImprovementSource.Heritage:
                    return LanguageManager.GetString("String_Priority", strLanguage);
                case Improvement.ImprovementSource.Initiation:
                    return LanguageManager.GetString("Tab_Initiation", strLanguage);
                case Improvement.ImprovementSource.Submersion:
                    return LanguageManager.GetString("Tab_Submersion", strLanguage);
                case Improvement.ImprovementSource.ArmorEncumbrance:
                    return LanguageManager.GetString("String_ArmorEncumbrance", strLanguage);
                default:
                    if (objImprovement.ImproveType == Improvement.ImprovementType.ArmorEncumbrancePenalty)
                        return LanguageManager.GetString("String_ArmorEncumbrance", strLanguage);
                    // If this comes from a custom Improvement, use the name the player gave it instead of showing a GUID.
                    if (!string.IsNullOrEmpty(objImprovement.CustomName))
                        return objImprovement.CustomName;
                    string strReturn = objImprovement.SourceName;
                    if (string.IsNullOrEmpty(strReturn) || strReturn.IsGuid())
                    {
                        string strTemp = LanguageManager.GetString("String_" + objImprovement.ImproveSource.ToString(), strLanguage, false);
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
        /// <param name="blnIgnoreBannedGrades">Whether to ignore grades banned at chargen.</param>
        public IList<Grade> GetGradeList(Improvement.ImprovementSource objSource, bool blnIgnoreBannedGrades = false)
        {
            List<Grade> lstGrades = new List<Grade>();
            StringBuilder strFilter = new StringBuilder();
            if (Options != null)
            {
                strFilter.Append('(' + Options.BookXPath() + ") and ");
            }
            if (!IgnoreRules && !Created && !blnIgnoreBannedGrades)
            {
                foreach (string strBannedGrade in BannedWareGrades)
                {
                    strFilter.Append("not(contains(name, \"" + strBannedGrade + "\")) and ");
                }
            }
            string strXPath;
            if (strFilter.Length != 0)
            {
                strFilter.Length -= 5;
                strXPath = "/chummer/grades/grade[(" + strFilter.ToString() + ")]";
            }
            else
                strXPath = "/chummer/grades/grade";
            using (XmlNodeList xmlGradeList = XmlManager.Load(objSource == Improvement.ImprovementSource.Bioware ? "bioware.xml" : "cyberware.xml").SelectNodes(strXPath))
                if (xmlGradeList != null)
                    foreach (XmlNode objNode in xmlGradeList)
                    {
                        Grade objGrade = new Grade(objSource);
                        objGrade.Load(objNode);
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
                    if (objLoopCyberware.Children.All(x => x.PlugsIntoModularMount != objLoopCyberware.HasModularMount))
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
                            if (objLoopCyberware.Children.All(x => x.PlugsIntoModularMount != objLoopCyberware.HasModularMount))
                            {
                                string strName = objLoopVehicle.DisplayName(GlobalOptions.Language) + ' ' +
                                    (objLoopCyberware.Parent?.DisplayName(GlobalOptions.Language) ?? objLoopVehicleMod.DisplayName(GlobalOptions.Language));
                                lstReturn.Add(new ListItem(objLoopCyberware.InternalId, strName));
                            }
                        }
                    }
                }
                foreach (WeaponMount objLoopWeaponMount in objLoopVehicle.WeaponMounts)
                {
                    foreach (VehicleMod objLoopVehicleMod in objLoopWeaponMount.Mods)
                    {
                        foreach (Cyberware objLoopCyberware in objLoopVehicleMod.Cyberware.GetAllDescendants(x => x.Children))
                        {
                            // Make sure this has an eligible mount location and it's not the selected piece modular cyberware
                            if (objLoopCyberware.HasModularMount == objModularCyberware.PlugsIntoModularMount && objLoopCyberware.Location == objModularCyberware.Location &&
                                objLoopCyberware.Grade.Name == objModularCyberware.Grade.Name && objLoopCyberware != objModularCyberware)
                            {
                                // Make sure it's not the place where the mount is already occupied (either by us or something else)
                                if (objLoopCyberware.Children.All(x => x.PlugsIntoModularMount != objLoopCyberware.HasModularMount))
                                {
                                    string strName = objLoopVehicle.DisplayName(GlobalOptions.Language) + ' ' +
                                        (objLoopCyberware.Parent?.DisplayName(GlobalOptions.Language) ?? objLoopVehicleMod.DisplayName(GlobalOptions.Language));
                                    lstReturn.Add(new ListItem(objLoopCyberware.InternalId, strName));
                                }
                            }
                        }
                    }
                }
            }
            return lstReturn;
        }

        public string CalculateKarmaValue(string strLanguage, out int intReturn)
        {
            string strMessage = LanguageManager.GetString("Message_KarmaValue", strLanguage) + Environment.NewLine;
            string strKarmaString = LanguageManager.GetString("String_Karma", strLanguage);
            int intExtraKarmaToRemoveForPointBuyComparison = 0;
            intReturn = BuildKarma;
            if (BuildMethod != CharacterBuildMethod.Karma)
            {
                // Subtract extra karma cost of a metatype in priority
                intReturn -= MetatypeBP;
            }
            strMessage += Environment.NewLine + LanguageManager.GetString("Label_Base", strLanguage) + ": " + intReturn.ToString() + ' ' + strKarmaString;

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
                foreach (CharacterAttrib objLoopAttrib in AttributeSection.AttributeList.Concat(AttributeSection.SpecialAttributeList))
                {
                    string strAttributeName = objLoopAttrib.Abbrev;
                    if (strAttributeName != "ESS" && (strAttributeName != "MAGAdept" || (IsMysticAdept && Options.MysAdeptSecondMAGAttribute)) && objLoopAttrib.MetatypeMaximum > 0)
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
                    strMessage += Environment.NewLine + LanguageManager.GetString("Label_SumtoTenHeritage", strLanguage) + ' ' + (intTemp - intAttributesValue + intMetatypeQualitiesValue).ToString() + ' ' + strKarmaString;
                }
                if (intAttributesValue != 0)
                {
                    strMessage += Environment.NewLine + LanguageManager.GetString("Label_SumtoTenAttributes", strLanguage) + ' ' + intAttributesValue.ToString() + ' ' + strKarmaString;
                }
                intReturn += intTemp;

                intTemp = 0;
                // This is where "Talent" qualities like Adept and Technomancer get added in
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
                    strMessage += Environment.NewLine + LanguageManager.GetString("String_Qualities", strLanguage) + ": " + intTemp.ToString() + ' ' + strKarmaString;
                    intReturn += intTemp;
                }

                // Value from free spells
                intTemp = SpellLimit * SpellKarmaCost("Spells");
                if (intTemp != 0)
                {
                    strMessage += Environment.NewLine + LanguageManager.GetString("String_FreeSpells", strLanguage) + ": " + intTemp.ToString() + ' ' + strKarmaString;
                    intReturn += intTemp;
                }

                // Value from free complex forms
                intTemp = CFPLimit * ComplexFormKarmaCost;
                if (intTemp != 0)
                {
                    strMessage += Environment.NewLine + LanguageManager.GetString("String_FreeCFs", strLanguage) + ": " + intTemp.ToString() + ' ' + strKarmaString;
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
                    strMessage += Environment.NewLine + LanguageManager.GetString("String_SkillPoints", strLanguage) + ": " + intTemp.ToString() + ' ' + strKarmaString;
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
                    strMessage += Environment.NewLine + LanguageManager.GetString("String_SkillGroupPoints", strLanguage) + ": " + intTemp.ToString() + ' ' + strKarmaString;
                    intReturn += intTemp;
                }

                // Starting Nuyen karma value
                intTemp = decimal.ToInt32(decimal.Ceiling(StartingNuyen / Options.NuyenPerBP));
                if (intTemp != 0)
                {
                    strMessage += Environment.NewLine + LanguageManager.GetString("Checkbox_CreatePACKSKit_StartingNuyen", strLanguage) + ": " + intTemp.ToString() + ' ' + strKarmaString;
                    intReturn += intTemp;
                }
            }

            int intContactPointsValue = ContactPoints * Options.KarmaContact;
            if (intContactPointsValue != 0)
            {
                strMessage += Environment.NewLine + LanguageManager.GetString("String_Contacts", strLanguage) + ": " + intContactPointsValue.ToString() + ' ' + strKarmaString;
                intReturn += intContactPointsValue;
                intExtraKarmaToRemoveForPointBuyComparison += intContactPointsValue;
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
                strMessage += Environment.NewLine + LanguageManager.GetString("Label_KnowledgeSkills", strLanguage) + ": " + intKnowledgePointsValue.ToString() + ' ' + strKarmaString;
                intReturn += intKnowledgePointsValue;
                intExtraKarmaToRemoveForPointBuyComparison += intKnowledgePointsValue;
            }

            strMessage += Environment.NewLine + Environment.NewLine + LanguageManager.GetString("String_Total", strLanguage) + ": " + intReturn.ToString() + ' ' + strKarmaString;
            strMessage += Environment.NewLine + Environment.NewLine + LanguageManager.GetString("String_TotalComparisonWithPointBuy", strLanguage) + ": " + (intReturn - intExtraKarmaToRemoveForPointBuyComparison).ToString() + ' ' + strKarmaString;

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
                using (XmlNodeList xmlCategoryList = xmlCategoryDocument.SelectNodes("/chummer/categories/category"))
                    if (xmlCategoryList != null)
                        // For each category node, split the comma-separated blackmarket attribute (if present on the node), then add each category where any of those items matches a Black Market Pipeline improvement. 
                        foreach (XmlNode xmlCategoryNode in xmlCategoryList)
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

        /// <summary>
        /// Creates a list of keywords for each category of an XML node. Used to preselect whether items of that category are discounted by the Black Market Pipeline quality.
        /// </summary>
        public HashSet<string> GenerateBlackMarketMappings(XPathNavigator xmlBaseChummerNode)
        {
            HashSet<string> setBlackMarketMaps = new HashSet<string>();
            // Character has no Black Market discount qualities. Fail out early. 
            if (BlackMarketDiscount && xmlBaseChummerNode != null)
            {
                // Get all the improved names of the Black Market Pipeline improvements. In most cases this should only be 1 item, but supports custom content.
                HashSet<string> setNames = new HashSet<string>();
                foreach (Improvement objImprovement in Improvements)
                {
                    if (objImprovement.ImproveType == Improvement.ImprovementType.BlackMarketDiscount && objImprovement.Enabled)
                        setNames.Add(objImprovement.ImprovedName);
                }
                
                // For each category node, split the comma-separated blackmarket attribute (if present on the node), then add each category where any of those items matches a Black Market Pipeline improvement. 
                foreach (XPathNavigator xmlCategoryNode in xmlBaseChummerNode.Select("categories/category"))
                {
                    string strBlackMarketAttribute = xmlCategoryNode.SelectSingleNode("@blackmarket")?.Value;
                    if (!string.IsNullOrEmpty(strBlackMarketAttribute) && strBlackMarketAttribute.Split(',').Any(x => setNames.Contains(x)))
                    {
                        setBlackMarketMaps.Add(xmlCategoryNode.Value);
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

        #region Move TreeNodes
        /// <summary>
        /// Move a Gear TreeNode after Drag and Drop, changing its parent.
        /// </summary>
        /// <param name="objGearNode">Node of gear to move.</param>
        /// <param name="objDestination">Destination Node.</param>
        public void MoveGearParent(TreeNode objDestination, TreeNode objGearNode)
        {
            // The item cannot be dropped onto itself or onto one of its children.
            for (TreeNode objCheckNode = objDestination; objCheckNode != null && objCheckNode.Level >= objDestination.Level; objCheckNode = objCheckNode.Parent)
                if (objCheckNode == objGearNode)
                    return;

            string strSelectedId = objGearNode.Tag.ToString();
            // Locate the currently selected piece of Gear.
            Gear objGear = Gear.DeepFindById(strSelectedId);

            // Gear cannot be moved to one if its children.
            bool blnAllowMove = true;
            TreeNode objFindNode = objDestination;
            if (objDestination.Level > 0)
            {
                do
                {
                    objFindNode = objFindNode.Parent;
                    if (objFindNode.Tag.ToString() == objGear.InternalId)
                    {
                        blnAllowMove = false;
                        break;
                    }
                } while (objFindNode.Level > 0);
            }

            if (!blnAllowMove)
                return;

            // Remove the Gear from the character.
            if (objGear.Parent == null)
                Gear.Remove(objGear);
            else
                objGear.Parent.Children.Remove(objGear);

            if (objDestination.Level == 0)
            {
                // The Gear was moved to a location, so add it to the character instead.
                objGear.Location = objDestination.Text;
                Gear.Add(objGear);
            }
            else
            {
                // Locate the Gear that the item was dropped on.
                Gear objParent = Gear.DeepFindById(objDestination.Tag.ToString());

                // Add the Gear as a child of the destination Node and clear its location.
                objGear.Location = string.Empty;
                objParent.Children.Add(objGear);
            }
        }

        /// <summary>
        /// Move a Gear TreeNode after Drag and Drop.
        /// </summary>
        /// <param name="intNewIndex">Node's new index.</param>
        /// <param name="objDestination">Destination Node.</param>
        /// <param name="objGearNode">Node of gear to move.</param>
        public void MoveGearNode(int intNewIndex, TreeNode objDestination, TreeNode objGearNode)
        {
            string strSelectedId = objGearNode?.Tag.ToString();
            Gear objGear = Gear.FirstOrDefault(x => x.InternalId == strSelectedId);
            if (objGear != null)
            {
                TreeNode objNewParent = objDestination;
                while (objNewParent.Level > 0)
                    objNewParent = objNewParent.Parent;

                // Change the Location on the Gear item.
                objGear.Location = objNewParent.Tag.ToString() == "Node_SelectedGear" ? string.Empty : objNewParent.Text;

                Gear.Move(Gear.IndexOf(objGear), intNewIndex);
            }
        }

        /// <summary>
        /// Move a Gear Location TreeNode after Drag and Drop.
        /// </summary>
        /// <param name="intNewIndex">Node's new index.</param>
        /// <param name="objDestination">Destination Node.</param>
        /// <param name="nodOldNode">Node of gear location to move.</param>
        public void MoveGearRoot(int intNewIndex, TreeNode objDestination, TreeNode nodOldNode)
        {
            if (objDestination != null)
            {
                TreeNode objNewParent = objDestination;
                while (objNewParent.Level > 0)
                    objNewParent = objNewParent.Parent;
                intNewIndex = objNewParent.Index;
            }

            if (intNewIndex == 0)
                return;

            string strLocation = nodOldNode.Tag.ToString();
            GearLocations.Move(GearLocations.IndexOf(strLocation), intNewIndex);
        }

        /// <summary>
        /// Move a Lifestyle TreeNode after Drag and Drop.
        /// </summary>
        /// <param name="intNewIndex">Node's new index.</param>
        /// <param name="objDestination">Destination Node.</param>
        /// <param name="nodLifestyleNode">Node of lifestyle to move.</param>
        public void MoveLifestyleNode(int intNewIndex, TreeNode objDestination, TreeNode nodLifestyleNode)
        {
            if (objDestination != null)
            {
                TreeNode objNewParent = objDestination;
                while (objNewParent.Level > 0)
                    objNewParent = objNewParent.Parent;
                intNewIndex = objNewParent.Index;
            }

            if (intNewIndex == 0)
                return;

            string strSelectedId = nodLifestyleNode.Tag.ToString();
            Lifestyle objLifestyle = Lifestyles.FirstOrDefault(x => x.InternalId == strSelectedId);
            if (objLifestyle != null)
                Lifestyles.Move(Lifestyles.IndexOf(objLifestyle), intNewIndex);
        }

        /// <summary>
        /// Move an Armor TreeNode after Drag and Drop.
        /// </summary>
        /// <param name="intNewIndex">Node's new index.</param>
        /// <param name="objDestination">Destination Node.</param>
        /// <param name="nodArmorNode">Node of armor to move.</param>
        public void MoveArmorNode(int intNewIndex, TreeNode objDestination, TreeNode nodArmorNode)
        {
            string strSelectedId = nodArmorNode?.Tag.ToString();
            // Locate the currently selected Armor.
            Armor objArmor = Armor.FindById(strSelectedId);
            if (objArmor != null)
            {
                TreeNode objNewParent = objDestination;
                while (objNewParent.Level > 0)
                    objNewParent = objNewParent.Parent;

                // Change the Location on the Armor item.
                objArmor.Location = objNewParent.Tag.ToString() == "Node_SelectedArmor" ? string.Empty : objNewParent.Text;

                Armor.Move(Armor.IndexOf(objArmor), intNewIndex);
            }
        }

        /// <summary>
        /// Move an Armor Location TreeNode after Drag and Drop.
        /// </summary>
        /// <param name="intNewIndex">Node's new index.</param>
        /// <param name="objDestination">Destination Node.</param>
        /// <param name="nodOldNode">Node of armor location to move.</param>
        public void MoveArmorRoot(int intNewIndex, TreeNode objDestination, TreeNode nodOldNode)
        {
            if (objDestination != null)
            {
                TreeNode objNewParent = objDestination;
                while (objNewParent.Level > 0)
                    objNewParent = objNewParent.Parent;
                intNewIndex = objNewParent.Index;
            }

            if (intNewIndex == 0)
                return;

            string strLocation = nodOldNode.Tag.ToString();
            ArmorLocations.Move(ArmorLocations.IndexOf(strLocation), intNewIndex);
        }

        /// <summary>
        /// Move a Weapon TreeNode after Drag and Drop.
        /// </summary>
        /// <param name="intNewIndex">Node's new index.</param>
        /// <param name="objDestination">Destination Node.</param>
        /// <param name="nodWeaponNode">Node of weapon to move.</param>
        public void MoveWeaponNode(int intNewIndex, TreeNode objDestination, TreeNode nodWeaponNode)
        {
            string strSelectedId = nodWeaponNode?.Tag.ToString();
            // Locate the currently selected Weapon.
            Weapon objWeapon = Weapons.FindById(strSelectedId);
            if (objWeapon != null)
            {
                TreeNode objNewParent = objDestination;
                while (objNewParent.Level > 0)
                    objNewParent = objNewParent.Parent;

                // Change the Location on the Armor item.
                objWeapon.Location = objNewParent.Tag.ToString() == "Node_SelectedWeapons" ? string.Empty : objNewParent.Text;

                Weapons.Move(Weapons.IndexOf(objWeapon), intNewIndex);
            }
        }

        /// <summary>
        /// Move a Weapon Location TreeNode after Drag and Drop.
        /// </summary>
        /// <param name="intNewIndex">Node's new index.</param>
        /// <param name="objDestination">Destination Node.</param>
        /// <param name="nodOldNode">Node of weapon location to move.</param>
        public void MoveWeaponRoot(int intNewIndex, TreeNode objDestination, TreeNode nodOldNode)
        {
            if (objDestination != null)
            {
                TreeNode objNewParent = objDestination;
                while (objNewParent.Level > 0)
                    objNewParent = objNewParent.Parent;
                intNewIndex = objNewParent.Index;
            }

            if (intNewIndex == 0)
                return;

            string strLocation = nodOldNode.Tag.ToString();
            WeaponLocations.Move(WeaponLocations.IndexOf(strLocation), intNewIndex);
        }

        /// <summary>
        /// Move a Vehicle TreeNode after Drag and Drop.
        /// </summary>
        /// <param name="intNewIndex">Node's new index.</param>
        /// <param name="objDestination">Destination Node.</param>
        /// <param name="nodVehicleNode">Node of vehicle to move.</param>
        public void MoveVehicleNode(int intNewIndex, TreeNode objDestination, TreeNode nodVehicleNode)
        {
            string strSelectedId = nodVehicleNode?.Tag.ToString();
            // Locate the currently selected Vehicle.
            Vehicle objVehicle = Vehicles.FindById(strSelectedId);
            if (objVehicle != null)
            {
                TreeNode objNewParent = objDestination;
                while (objNewParent.Level > 0)
                    objNewParent = objNewParent.Parent;

                // Change the Location on the Armor item.
                objVehicle.Location = objNewParent.Tag.ToString() == "Node_SelectedVehicles" ? string.Empty : objNewParent.Text;

                Vehicles.Move(Vehicles.IndexOf(objVehicle), intNewIndex);
            }
        }

        /// <summary>
        /// Move a Vehicle Gear TreeNode after Drag and Drop.
        /// </summary>
        /// <param name="nodDestination">Destination Node.</param>
        /// <param name="nodGearNode">Node of gear to move.</param>
        public void MoveVehicleGearParent(TreeNode nodDestination, TreeNode nodGearNode)
        {
            // The item cannot be dropped onto itself or onto one of its children.
            for (TreeNode objCheckNode = nodDestination; objCheckNode != null && objCheckNode.Level >= nodDestination.Level; objCheckNode = objCheckNode.Parent)
                if (objCheckNode == nodGearNode)
                    return;

            // Locate the currently selected piece of Gear.
            Gear objGear = Vehicles.FindVehicleGear(nodGearNode.Tag.ToString(), out Vehicle objOldVehicle, out WeaponAccessory objOldWeaponAccessory, out Cyberware objOldCyberware);

            if (objGear == null)
                return;

            Gear objOldParent = objGear.Parent;
            string strDestinationId = nodDestination.Tag.ToString();
            // Make sure the destination is another piece of Gear or a Location.
            Gear objDestinationGear = Vehicles.FindVehicleGear(strDestinationId);
            if (objDestinationGear != null)
            {
                // Remove the Gear from the Vehicle.
                if (objOldParent != null)
                    objOldParent.Children.Remove(objGear);
                else if (objOldCyberware != null)
                    objOldCyberware.Gear.Remove(objGear);
                else if (objOldWeaponAccessory != null)
                    objOldWeaponAccessory.Gear.Remove(objGear);
                else
                    objOldVehicle.Gear.Remove(objGear);

                // Add the Gear to its new parent.
                objGear.Location = string.Empty;
                objDestinationGear.Children.Add(objGear);
            }
            else
            {
                // Determine if this is a Location.
                TreeNode nodVehicleNode = nodDestination;
                do
                {
                    nodVehicleNode = nodVehicleNode.Parent;
                }
                while (nodVehicleNode.Level > 1);

                // Get a reference to the destination Vehicle.
                Vehicle objDestinationVehicle = Vehicles.FindById(nodVehicleNode.Tag.ToString());

                // Determine if this is a Location in the destination Vehicle.
                string strDestinationLocation = objDestinationVehicle.Locations.FirstOrDefault(x => x == strDestinationId);

                if (!string.IsNullOrEmpty(strDestinationLocation))
                {
                    // Remove the Gear from the Vehicle.
                    if (objOldParent != null)
                        objOldParent.Children.Remove(objGear);
                    else if (objOldCyberware != null)
                        objOldCyberware.Gear.Remove(objGear);
                    else if (objOldWeaponAccessory != null)
                        objOldWeaponAccessory.Gear.Remove(objGear);
                    else
                        objOldVehicle.Gear.Remove(objGear);

                    // Add the Gear to the Vehicle and set its Location.
                    objGear.Location = strDestinationLocation;
                }
            }
        }

        /// <summary>
        /// Move an Improvement TreeNode after Drag and Drop.
        /// </summary>
        /// <param name="intNewIndex">Node's new index.</param>
        /// <param name="objDestination">Destination Node.</param>
        /// <param name="nodOldNode">Node of improvement to move.</param>
        public void MoveImprovementNode(int intNewIndex, TreeNode objDestination, TreeNode nodOldNode)
        {
            string strSelectedId = nodOldNode?.Tag.ToString();
            int intOldIndex = -1;
            for (int i = 0; i < Improvements.Count; ++i)
            {
                if (Improvements[i].SourceName == strSelectedId)
                {
                    intOldIndex = i;
                    break;
                }
            }

            if (intOldIndex != -1)
            {
                TreeNode objNewParent = objDestination;
                while (objNewParent.Level > 0)
                    objNewParent = objNewParent.Parent;

                Improvement objImprovement = Improvements[intOldIndex];
                // Change the Group on the Custom Improvement.
                objImprovement.CustomGroup = objNewParent.Text;
                Improvements[intOldIndex] = objImprovement;
            }
        }

        /// <summary>
        /// Move an Improvement Group TreeNode after Drag and Drop.
        /// </summary>
        /// <param name="intNewIndex">Node's new index.</param>
        /// <param name="objDestination">Destination Node.</param>
        /// <param name="nodOldNode">Node of improvement group to move.</param>
        public void MoveImprovementRoot(int intNewIndex, TreeNode objDestination, TreeNode nodOldNode)
        {
            if (objDestination != null)
            {
                TreeNode objNewParent = objDestination;
                while (objNewParent.Level > 0)
                    objNewParent = objNewParent.Parent;
                intNewIndex = objNewParent.Index;
            }

            if (intNewIndex == 0)
                return;

            string strLocation = nodOldNode.Tag.ToString();
            ImprovementGroups.Move(ImprovementGroups.IndexOf(strLocation), intNewIndex);
        }
        #endregion

        #region Tab clearing
        /// <summary>
        /// Clear all Spell tab elements from the character.
        /// </summary>
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
        public void ClearCyberwareTab()
        {
            for (int i = Cyberware.Count - 1; i >= 0; i--)
            {
                if (i < Cyberware.Count)
                {
                    Cyberware objToRemove = Cyberware[i];
                    if (string.IsNullOrEmpty(objToRemove.ParentID))
                    {
                        objToRemove.DeleteCyberware();
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
            // Metamagics/Echoes can add addition bonus metamagics/echoes, so neither foreach nor RemoveAll() can be used
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
        public CharacterOptions Options => _objOptions;

        /// <summary>
        /// Name of the file the Character is saved to.
        /// </summary>
        public string FileName
        {
            get => _strFileName;
            set => _strFileName = value;
        }

        /// <summary>
        /// Name of the file the Character is saved to.
        /// </summary>
        public DateTime FileLastWriteTime => _dateFileLastWriteTime;

        /// <summary>
        /// Name of the settings file the Character uses.
        /// </summary>
        public string SettingsFile
        {
            get => _strSettingsFileName;
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
            get => _blnCreated;
            set => _blnCreated = value;
        }

        /// <summary>
        /// Character's name.
        /// </summary>
        public string Name
        {
            get => _strName;
            set
            {
                if (_strName != value)
                {
                    _strName = value;
                    if (CharacterNameChanged != null && string.IsNullOrWhiteSpace(Alias))
                        CharacterNameChanged.Invoke(this, EventArgs.Empty);
                }
            }
        }

		/// <summary>
		/// Character's portraits encoded using Base64.
		/// </summary>
		public IList<Image> Mugshots => _lstMugshots;

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
            get => _intMainMugshotIndex;
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

        public void LoadMugshots(XPathNavigator xmlSavedNode)
        {
            // Mugshots
            xmlSavedNode.TryGetInt32FieldQuickly("mainmugshotindex", ref _intMainMugshotIndex);
            XPathNodeIterator xmlMugshotsList = xmlSavedNode.Select("mugshots/mugshot");
            List<string> lstMugshotsBase64 = new List<string>(xmlMugshotsList.Count);
            foreach (XPathNavigator objXmlMugshot in xmlMugshotsList)
            {
                string strMugshot = objXmlMugshot.Value;
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
            // Legacy Shimmer
            if (Mugshots.Count == 0)
            {
                XPathNavigator objOldMugshotNode = xmlSavedNode.SelectSingleNode("mugshot");
                string strMugshot = objOldMugshotNode?.Value;
                if (!string.IsNullOrWhiteSpace(strMugshot))
                {
                    _lstMugshots.Add(strMugshot.ToImage(System.Drawing.Imaging.PixelFormat.Format32bppPArgb));
                    _intMainMugshotIndex = 0;
                }
            }
        }

        public void PrintMugshots(XmlTextWriter objWriter)
        {
            if (Mugshots.Count > 0)
            {
                // Since IE is retarded and can't handle base64 images before IE9, the image needs to be dumped to a temporary directory and its information rewritten.
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
            get => _strGameplayOption;
            set => _strGameplayOption = value;
        }

        /// <summary>
        /// Quality Limit conferred by the Character's Gameplay Option
        /// </summary>
        public int GameplayOptionQualityLimit
        {
            get => _intGameplayOptionQualityLimit;
            set => _intGameplayOptionQualityLimit = value;
        }

        /// <summary>
        /// Character's maximum karma at character creation.
        /// </summary>
        public int MaxKarma
        {
            get => _intMaxKarma;
            set => _intMaxKarma = value;
        }

        /// <summary>
        /// Character's maximum nuyen at character creation.
        /// </summary>
        public decimal MaxNuyen
        {
            get => _decMaxNuyen;
            set => _decMaxNuyen = value;
        }

        /// <summary>
        /// Character's contact point multiplier.
        /// </summary>
        public int ContactMultiplier
        {
            get => _intContactMultiplier;
            set => _intContactMultiplier = value;
        }

        /// <summary>
        /// Character's Metatype Priority.
        /// </summary>
        public string MetatypePriority
        {
            get => _strPriorityMetatype;
            set => _strPriorityMetatype = value;
        }

        /// <summary>
        /// Character's Attributes Priority.
        /// </summary>
        public string AttributesPriority
        {
            get => _strPriorityAttributes;
            set => _strPriorityAttributes = value;
        }

        /// <summary>
        /// Character's Special Priority.
        /// </summary>
        public string SpecialPriority
        {
            get => _strPrioritySpecial;
            set => _strPrioritySpecial = value;
        }

        /// <summary>
        /// Character's Skills Priority.
        /// </summary>
        public string SkillsPriority
        {
            get => _strPrioritySkills;
            set => _strPrioritySkills = value;
        }

        /// <summary>
        /// Character's Resources Priority.
        /// </summary>
        public string ResourcesPriority
        {
            get => _strPriorityResources;
            set => _strPriorityResources = value;
        }

        /// <summary>
        /// Character's Resources Priority.
        /// </summary>
        public string TalentPriority
        {
            get => _strPriorityTalent;
            set => _strPriorityTalent = value;
        }

        /// <summary>
        /// Character's list of priority bonus skills.
        /// </summary>
        public IList<string> PriorityBonusSkillList => _lstPrioritySkills;

        /// <summary>
        /// Character's sex.
        /// </summary>
        public string Sex
        {
            get => _strSex;
            set
            {
                if (_strSex != value)
                {
                    _strSex = value;
                    _strCharacterGrammaticGender = string.Empty;
                }
            }
        }

        private string _strCharacterGrammaticGender = string.Empty;
        public string CharacterGrammaticGender
        {
            get
            {
                if (!string.IsNullOrEmpty(_strCharacterGrammaticGender))
                    return _strCharacterGrammaticGender;
                switch (LanguageManager.ReverseTranslateExtra(Sex, GlobalOptions.Language).ToLower())
                {
                    case "m":
                    case "male":
                    case "man":
                    case "boy":
                    case "lord":
                    case "gentleman":
                    case "guy":
                        return _strCharacterGrammaticGender = "male";
                    case "f":
                    case "female":
                    case "woman":
                    case "girl":
                    case "lady":
                    case "gal":
                        return _strCharacterGrammaticGender = "female";
                    default:
                        return _strCharacterGrammaticGender = "neutral";
                }
            }
        }

        /// <summary>
        /// Character's age.
        /// </summary>
        public string Age
        {
            get => _strAge;
            set => _strAge = value;
        }

        /// <summary>
        /// Character's eyes.
        /// </summary>
        public string Eyes
        {
            get => _strEyes;
            set => _strEyes = value;
        }

        /// <summary>
        /// Character's height.
        /// </summary>
        public string Height
        {
            get => _strHeight;
            set => _strHeight = value;
        }

        /// <summary>
        /// Character's weight.
        /// </summary>
        public string Weight
        {
            get => _strWeight;
            set => _strWeight = value;
        }

        /// <summary>
        /// Character's skin.
        /// </summary>
        public string Skin
        {
            get => _strSkin;
            set => _strSkin = value;
        }

        /// <summary>
        /// Character's hair.
        /// </summary>
        public string Hair
        {
            get => _strHair;
            set => _strHair = value;
        }

        /// <summary>
        /// Character's description.
        /// </summary>
        public string Description
        {
            get => _strDescription;
            set => _strDescription = value;
        }

        /// <summary>
        /// Character's background.
        /// </summary>
        public string Background
        {
            get => _strBackground;
            set => _strBackground = value;
        }

        /// <summary>
        /// Character's concept.
        /// </summary>
        public string Concept
        {
            get => _strConcept;
            set => _strConcept = value;
        }

        /// <summary>
        /// Character notes.
        /// </summary>
        public string Notes
        {
            get => _strNotes;
            set => _strNotes = value;
        }

        /// <summary>
        /// General gameplay notes.
        /// </summary>
        public string GameNotes
        {
            get => _strGameNotes;
            set => _strGameNotes = value;
        }

        /// <summary>
        /// What is the Characters prefered hand
        /// </summary>
        public string PrimaryArm
        {
            get => _strPrimaryArm;
            set => _strPrimaryArm = value;
        }

        /// <summary>
        /// Player name.
        /// </summary>
        public string PlayerName
        {
            get => _strPlayerName;
            set => _strPlayerName = value;
        }

        /// <summary>
        /// Character's alias.
        /// </summary>
        public string Alias
        {
            get => _strAlias;
            set
            {
                if (_strAlias != value)
                {
                    _strAlias = value;
                    CharacterNameChanged?.Invoke(this, EventArgs.Empty);
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
            get => _intStreetCred;
            set => _intStreetCred = value;
        }

        /// <summary>
        /// Burnt Street Cred.
        /// </summary>
        public int BurntStreetCred
        {
            get => _intBurntStreetCred;
            set => _intBurntStreetCred = value;
        }

        /// <summary>
        /// Notoriety.
        /// </summary>
        public int Notoriety
        {
            get => _intNotoriety;
            set => _intNotoriety = value;
        }

        /// <summary>
        /// Public Awareness.
        /// </summary>
        public int PublicAwareness
        {
            get => _intPublicAwareness;
            set => _intPublicAwareness = value;
        }

        /// <summary>
        /// Number of Physical Condition Monitor Boxes that are filled.
        /// </summary>
        public int PhysicalCMFilled
        {
            get
            {
                if (HomeNode is Vehicle objVehicle)
                    return objVehicle.PhysicalCMFilled;

                return _intPhysicalCMFilled;
            }
            set
            {
                if (HomeNode is Vehicle objVehicle)
                {
                    if (objVehicle.PhysicalCMFilled != value)
                    {
                        objVehicle.PhysicalCMFilled = value;
                        RefreshWoundPenalties();
                    }
                }
                else if (_intPhysicalCMFilled != value)
                {
                    _intPhysicalCMFilled = value;
                    RefreshWoundPenalties();
                }
            }
        }

        /// <summary>
        /// Number of Stun Condition Monitor Boxes that are filled.
        /// </summary>
        public int StunCMFilled
        {
            get
            {
                if (DEPEnabled && BOD.MetatypeMaximum == 0 && HomeNode != null)
                {
                    // A.I. do not have a Stun Condition Monitor, but they do have a Matrix Condition Monitor if they are in their home node.
                    return HomeNode.MatrixCMFilled;
                }
                return _intStunCMFilled;
            }
            set
            {
                if (DEPEnabled && BOD.MetatypeMaximum == 0 && HomeNode != null)
                {
                    // A.I. do not have a Stun Condition Monitor, but they do have a Matrix Condition Monitor if they are in their home node.
                    if (HomeNode.MatrixCMFilled != value)
                    {
                        HomeNode.MatrixCMFilled = value;
                        RefreshWoundPenalties();
                    }
                }
                else if (_intStunCMFilled != value)
                {
                    _intStunCMFilled = value;
                    RefreshWoundPenalties();
                }
            }
        }

        /// <summary>
        /// Whether or not character creation rules should be ignored.
        /// </summary>
        public bool IgnoreRules
        {
            get => _blnIgnoreRules;
            set => _blnIgnoreRules = value;
        }

        /// <summary>
        /// Contact Points.
        /// </summary>
        public int ContactPoints
        {
            get => _intContactPoints;
            set => _intContactPoints = value;
        }


        /// <summary>
        /// Number of free Contact Points the character has used.
        /// </summary>
        public int ContactPointsUsed
        {
            get => _intContactPointsUsed;
            set => _intContactPointsUsed = value;
        }

        /// <summary>
        /// CFP Limit.
        /// </summary>
        public int CFPLimit
        {
            get => _intCFPLimit;
            set => _intCFPLimit = value;
        }

        /// <summary>
        /// Total AI Program Limit.
        /// </summary>
        public int AINormalProgramLimit
        {
            get => _intAINormalProgramLimit;
            set => _intAINormalProgramLimit = value;
        }

        /// <summary>
        /// AI Advanced Program Limit.
        /// </summary>
        public int AIAdvancedProgramLimit
        {
            get => _intAIAdvancedProgramLimit;
            set => _intAIAdvancedProgramLimit = value;
        }

        /// <summary>
        /// Spell Limit.
        /// </summary>
        public int SpellLimit
        {
            get => _intSpellLimit;
            set => _intSpellLimit = value;
        }

        /// <summary>
        /// Karma.
        /// </summary>
        public int Karma
        {
            get => _intKarma;
            set
            {
                if (_intKarma != value)
                {
                    _intKarma = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Karma)));
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(CanAffordCareerPP)));
                    }
                }
            }
        }

        /// <summary>
        /// Special.
        /// </summary>
        public int Special
        {
            get => _intSpecial;
            set => _intSpecial = value;
        }

        /// <summary>
        /// TotalSpecial.
        /// </summary>
        public int TotalSpecial
        {
            get => _intTotalSpecial;
            set => _intTotalSpecial = value;
        }

        /// <summary>
        /// Attributes.
        /// </summary>
        public int Attributes
        {
            get => _intAttributes;
            set => _intAttributes = value;
        }

        /// <summary>
        /// TotalAttributes.
        /// </summary>
        public int TotalAttributes
        {
            get => _intTotalAttributes;
            set => _intTotalAttributes = value;
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
            get => _blnIsCritter;
            set => _blnIsCritter = value;
        }

        /// <summary>
        /// The highest number of free metagenetic qualities the character can have.
        /// </summary>
        public int MetageneticLimit
        {
            get => ImprovementManager.ValueOf(this, Improvement.ImprovementType.MetageneticLimit);
        }

        /// <summary>
        /// Whether or not the character is possessed by a Spirit.
        /// </summary>
        public bool Possessed
        {
            get => _blnPossessed;
            set => _blnPossessed = value;
        }

        /// <summary>
        /// Maximum item Availability for new characters.
        /// </summary>
        public int MaximumAvailability
        {
            get => _intMaxAvail;
            set => _intMaxAvail = value;
        }

        public int SpellKarmaCost(string category = "")
        {
            int intReturn = Options.KarmaSpell;

            decimal decMultiplier = 1.0m;
            foreach (Improvement objLoopImprovement in Improvements.Where(imp => (imp.ImproveType == Improvement.ImprovementType.NewSpellKarmaCost ||
                                                                                  imp.ImproveType == Improvement.ImprovementType.NewSpellKarmaCostMultiplier) &&
                                                                                  imp.ImprovedName == category))
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
            get => _blnAmbidextrous;
            set
            {
                if (_blnAmbidextrous != value)
                {
                    _blnAmbidextrous = value;
                    AmbidextrousChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
#endregion

#region Attributes
        /// <summary>
        /// Get a CharacterAttribute by its name.
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
        public CharacterAttrib BOD => AttributeSection.GetAttributeByName("BOD");

        /// <summary>
        /// Agility (AGI) CharacterAttribute.
        /// </summary>
        public CharacterAttrib AGI => AttributeSection.GetAttributeByName("AGI");

        /// <summary>
        /// Reaction (REA) CharacterAttribute.
        /// </summary>
        public CharacterAttrib REA => AttributeSection.GetAttributeByName("REA");

        /// <summary>
        /// Strength (STR) CharacterAttribute.
        /// </summary>
        public CharacterAttrib STR => AttributeSection.GetAttributeByName("STR");

        /// <summary>
        /// Charisma (CHA) CharacterAttribute.
        /// </summary>
        public CharacterAttrib CHA => AttributeSection.GetAttributeByName("CHA");

        /// <summary>
        /// Intuition (INT) CharacterAttribute.
        /// </summary>
        public CharacterAttrib INT => AttributeSection.GetAttributeByName("INT");

        /// <summary>
        /// Logic (LOG) CharacterAttribute.
        /// </summary>
        public CharacterAttrib LOG => AttributeSection.GetAttributeByName("LOG");

        /// <summary>
        /// Willpower (WIL) CharacterAttribute.
        /// </summary>
        public CharacterAttrib WIL => AttributeSection.GetAttributeByName("WIL");
        
        /// <summary>
        /// Edge (EDG) CharacterAttribute.
        /// </summary>
        public CharacterAttrib EDG => AttributeSection.GetAttributeByName("EDG");

        /// <summary>
        /// Magic (MAG) CharacterAttribute.
        /// </summary>
        public CharacterAttrib MAG => AttributeSection.GetAttributeByName("MAG");

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
        public CharacterAttrib RES => AttributeSection.GetAttributeByName("RES");

        /// <summary>
        /// Depth (DEP) Attribute.
        /// </summary>
        public CharacterAttrib DEP => AttributeSection.GetAttributeByName("DEP");

        /// <summary>
        /// Essence (ESS) Attribute.
        /// </summary>
        public CharacterAttrib ESS => AttributeSection.GetAttributeByName("ESS");

        /// <summary>
        /// Is the MAG CharacterAttribute enabled?
        /// </summary>
        public bool MAGEnabled
        {
            get => _blnMAGEnabled;
            set
            {
                if (_blnMAGEnabled != value)
                {
                    _blnMAGEnabled = value;
                    if (value)
                    {
                        // Career mode, so no extra calculations need tobe done for EssenceAtSpecialStart
                        if (Created)
                        {
                            ResetCachedEssence();
                            EssenceAtSpecialStart = Essence(true);
                        }
                        // EssenceAtSpecialStart needs to be calculated by assuming that the character took the MAG-enabling quality with the highest essence penalty first, as that would be the most optimal
                        else
                        {
                            // If this character has any MAG-enabling bonuses that could be granted before all others (because they're priority and/or metatype-given),
                            // it has to be assumed those are taken first.
                            bool blnCountOnlyPriorityOrMetatypeGivenBonuses = Improvements.Any(x => x.ImproveType == Improvement.ImprovementType.Attribute && x.ImprovedName == "MAG" &&
                                                                                                    (x.ImproveSource == Improvement.ImprovementSource.Metatype ||
                                                                                                     x.ImproveSource == Improvement.ImprovementSource.Metavariant ||
                                                                                                     x.ImproveSource == Improvement.ImprovementSource.Heritage) && x.Enabled);
                            Dictionary<string, decimal> dicImprovementEssencePenalties = new Dictionary<string, decimal>();
                            foreach (Improvement objImprovement in Improvements)
                            {
                                if ((!blnCountOnlyPriorityOrMetatypeGivenBonuses ||
                                     objImprovement.ImproveSource == Improvement.ImprovementSource.Metatype ||
                                     objImprovement.ImproveSource == Improvement.ImprovementSource.Metavariant ||
                                     objImprovement.ImproveSource == Improvement.ImprovementSource.Heritage) && objImprovement.Enabled)
                                {
                                    decimal decLoopEssencePenalty = 0;
                                    if (objImprovement.ImproveType == Improvement.ImprovementType.EssencePenalty)
                                    {
                                        decLoopEssencePenalty += objImprovement.Value;
                                    }
                                    else if (objImprovement.ImproveType == Improvement.ImprovementType.EssencePenaltyT100 ||
                                             objImprovement.ImproveType == Improvement.ImprovementType.EssencePenaltyMAGOnlyT100)
                                    {
                                        decLoopEssencePenalty += Convert.ToDecimal(objImprovement.Value) / 100.0m;
                                    }

                                    if (decLoopEssencePenalty != 0)
                                    {
                                        if (dicImprovementEssencePenalties.ContainsKey(objImprovement.SourceName))
                                            dicImprovementEssencePenalties[objImprovement.SourceName] = dicImprovementEssencePenalties[objImprovement.SourceName] + decLoopEssencePenalty;
                                        else
                                            dicImprovementEssencePenalties.Add(objImprovement.SourceName, decLoopEssencePenalty);
                                    }
                                }
                            }

                            if (dicImprovementEssencePenalties.Count > 0)
                                EssenceAtSpecialStart = ESS.MetatypeMaximum + dicImprovementEssencePenalties.Values.Min();
                            else
                                EssenceAtSpecialStart = ESS.MetatypeMaximum;
                        }
                    }
                    else if (!Created && !RESEnabled && !DEPEnabled)
                        EssenceAtSpecialStart = decimal.MinValue;
                    MAGEnabledChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Maximum force of spirits summonable/bindable by the character. Limited to MAG at creation. 
        /// </summary>
        public int MaxSpiritForce => (Created ? 2 : 1) * (Options.SpiritForceBasedOnTotalMAG ? MAG.TotalValue : MAG.Value);

        public void RefreshMaxSpiritForce(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == (Options.SpiritForceBasedOnTotalMAG ? nameof(CharacterAttrib.TotalValue) : nameof(CharacterAttrib.Value)))
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MaxSpiritForce)));
        }

        /// <summary>
        /// Maximum level of sprites compilable/registerable by the character. Limited to RES at creation. 
        /// </summary>
        public int MaxSpriteLevel => (Created ? 2 : 1) * RES.TotalValue;

        public void RefreshMaxSpriteLevel(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CharacterAttrib.TotalValue))
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MaxSpriteLevel)));
        }

        /// <summary>
        /// Amount of Power Points for Mystic Adepts.
        /// </summary>
        public int MysticAdeptPowerPoints
        {
            get => _intMAGAdept;
            set
            {
                if (_intMAGAdept != value)
                {
                    _intMAGAdept = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MysticAdeptPowerPoints)));
                }
            }
        }

        /// <summary>
        /// Magician's Tradition.
        /// </summary>
        public string MagicTradition
        {
            get => _strMagicTradition;
            set
            {
                if (_strMagicTradition != value)
                {
                    _strMagicTradition = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MagicTradition)));
                }
            }
        }

        /// <summary>
        /// Magician's total amount of dice for resisting drain.
        /// </summary>
        public int TraditionDrainValue
        {
            get
            {
                string strDrainAttributes = TraditionDrain;
                StringBuilder objDrain = new StringBuilder(strDrainAttributes);
                foreach (string strAttribute in AttributeSection.AttributeStrings)
                {
                    CharacterAttrib objAttrib = GetAttribute(strAttribute);
                    objDrain.CheapReplace(strDrainAttributes, objAttrib.Abbrev, () => objAttrib.TotalValue.ToString());
                }
                string strDrain = objDrain.ToString();
                if (!int.TryParse(strDrain, out int intDrain))
                {
                    object objProcess = CommonFunctions.EvaluateInvariantXPath(strDrain, out bool blnIsSuccess);
                    if (blnIsSuccess)
                        intDrain = Convert.ToInt32(objProcess);
                }

                // Add any Improvements for Drain Resistance.
                intDrain += ImprovementManager.ValueOf(this, Improvement.ImprovementType.DrainResistance);

                return intDrain;
            }
        }

        public void RefreshTraditionDrainValue(object sender, EventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TraditionDrainValue)));
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
                if (_strTraditionDrain != value)
                {
                    foreach (string strOldDrainAttribute in AttributeSection.AttributeStrings)
                    {
                        if (_strTraditionDrain.Contains(strOldDrainAttribute))
                            GetAttribute(strOldDrainAttribute).PropertyChanged -= RefreshTraditionDrainValue;
                    }
                    _strTraditionDrain = value;
                    foreach (string strNewDrainAttribute in AttributeSection.AttributeStrings)
                    {
                        if (value.Contains(strNewDrainAttribute))
                            GetAttribute(strNewDrainAttribute).PropertyChanged += RefreshTraditionDrainValue;
                    }
                    if (PropertyChanged != null)
                    {
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(TraditionDrain)));
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayTraditionDrain)));
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(TraditionDrainValue)));
                    }
                }
            }
        }

        /// <summary>
        /// Magician's Tradition Drain Attributes for display purposes.
        /// </summary>
        public string DisplayTraditionDrain => DisplayTraditionDrainMethod(GlobalOptions.Language);

        /// <summary>
        /// Magician's Tradition Drain Attributes for display purposes.
        /// </summary>
        public string DisplayTraditionDrainMethod(string strLanguage)
        {
            string strDrain = TraditionDrain;
            foreach (string strAttribute in AttributeSection.AttributeStrings)
            {
                strDrain = strDrain.CheapReplace(strAttribute, () =>
                {
                    if (strAttribute == "MAGAdept")
                        return LanguageManager.GetString("String_AttributeMAGShort", strLanguage) + " (" + LanguageManager.GetString("String_DescAdept", strLanguage) + ')';

                    return LanguageManager.GetString($"String_Attribute{strAttribute}Short", strLanguage);
                });
            }

            return strDrain;
        }

        /// <summary>
        /// Magician's Tradition Name (for Custom Traditions).
        /// </summary>
        public string TraditionName
        {
            get => _strTraditionName;
            set
            {
                if (_strTraditionName != value)
                {
                    _strTraditionName = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TraditionName)));
                }
            } 
        }

        /// <summary>
        /// Magician's Combat Spirit (for Custom Traditions).
        /// </summary>
        public string SpiritCombat
        {
            get => _strSpiritCombat;
            set
            {
                if (_strSpiritCombat != value)
                {
                    _strSpiritCombat = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SpiritCombat)));
                }
            }
        }

        /// <summary>
        /// Magician's Detection Spirit (for Custom Traditions).
        /// </summary>
        public string SpiritDetection
        {
            get => _strSpiritDetection;
            set
            {
                if (_strSpiritDetection != value)
                {
                    _strSpiritDetection = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SpiritDetection)));
                }
            }
        }

        /// <summary>
        /// Magician's Health Spirit (for Custom Traditions).
        /// </summary>
        public string SpiritHealth
        {
            get => _strSpiritHealth;
            set
            {
                if (_strSpiritHealth != value)
                {
                    _strSpiritHealth = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SpiritHealth)));
                }
            }
        }

        /// <summary>
        /// Magician's Illusion Spirit (for Custom Traditions).
        /// </summary>
        public string SpiritIllusion
        {
            get => _strSpiritIllusion;
            set
            {
                if (_strSpiritIllusion != value)
                {
                    _strSpiritIllusion = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SpiritIllusion)));
                }
            }
        }

        /// <summary>
        /// Magician's Manipulation Spirit (for Custom Traditions).
        /// </summary>
        public string SpiritManipulation
        {
            get => _strSpiritManipulation;
            set
            {
                if (_strSpiritManipulation != value)
                {
                    _strSpiritManipulation = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SpiritManipulation)));
                }
            }
        }

        /// <summary>
        /// Technomancer's Stream.
        /// </summary>
        public string TechnomancerStream
        {
            get => _strTechnomancerStream;
            set
            {
                if (_strTechnomancerStream != value)
                {
                    _strTechnomancerStream = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TechnomancerStream)));
                }
            }
        }

        /// <summary>
        /// Magician's total amount of dice for resisting drain.
        /// </summary>
        public int TechnomancerFadingValue
        {
            get
            {
                string strFadingAttributes = TechnomancerFading;
                StringBuilder objFading = new StringBuilder(strFadingAttributes);
                foreach (string strAttribute in AttributeSection.AttributeStrings)
                {
                    CharacterAttrib objAttrib = GetAttribute(strAttribute);
                    objFading.CheapReplace(strFadingAttributes, objAttrib.Abbrev, () => objAttrib.TotalValue.ToString());
                }
                string strFading = objFading.ToString();
                if (!int.TryParse(strFading, out int intDrain))
                {
                    object objProcess = CommonFunctions.EvaluateInvariantXPath(strFading, out bool blnIsSuccess);
                    if (blnIsSuccess)
                        intDrain = Convert.ToInt32(objProcess);
                }

                // Add any Improvements for Fading Resistance.
                intDrain += ImprovementManager.ValueOf(this, Improvement.ImprovementType.FadingResistance);

                return intDrain;
            }
        }

        public void RefreshTechnomancerFadingValue(object sender, EventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TechnomancerFadingValue)));
        }

        /// <summary>
        /// Technomancer's Fading Attributes.
        /// </summary>
        public string TechnomancerFading
        {
            get => _strTechnomancerFading;
            set
            {
                if (_strTechnomancerFading != value)
                {
                    foreach (string strOldDrainAttribute in AttributeSection.AttributeStrings)
                    {
                        if (_strTechnomancerFading.Contains(strOldDrainAttribute))
                            GetAttribute(strOldDrainAttribute).PropertyChanged -= RefreshTechnomancerFadingValue;
                    }
                    _strTechnomancerFading = value;
                    foreach (string strNewDrainAttribute in AttributeSection.AttributeStrings)
                    {
                        if (value.Contains(strNewDrainAttribute))
                            GetAttribute(strNewDrainAttribute).PropertyChanged += RefreshTechnomancerFadingValue;
                    }
                    if (PropertyChanged != null)
                    {
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(TechnomancerFading)));
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayTechnomancerFading)));
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(TechnomancerFadingValue)));
                    }
                }
            }
        }

        /// <summary>
        /// Magician's Fading Attributes for display purposes.
        /// </summary>
        public string DisplayTechnomancerFading => DisplayTechnomancerFadingMethod(GlobalOptions.Language);

        /// <summary>
        /// Magician's Fading Attributes for display purposes.
        /// </summary>
        public string DisplayTechnomancerFadingMethod(string strLanguage)
        {
            string strFading = TechnomancerFading;
            foreach (string strAttribute in AttributeSection.AttributeStrings)
            {
                strFading = strFading.CheapReplace(strAttribute, () =>
                {
                    if (strAttribute == "MAGAdept")
                        return LanguageManager.GetString("String_AttributeMAGShort", strLanguage) + " (" + LanguageManager.GetString("String_DescAdept", strLanguage) + ')';

                    return LanguageManager.GetString($"String_Attribute{strAttribute}Short", strLanguage);
                });
            }

            return strFading;
        }

        /// <summary>
        /// Initiate Grade.
        /// </summary>
        public int InitiateGrade
        {
            get => _intInitiateGrade;
            set => _intInitiateGrade = value;
        }

        /// <summary>
        /// Is the RES CharacterAttribute enabled?
        /// </summary>
        public bool RESEnabled
        {
            get => _blnRESEnabled;
            set
            {
                if (_blnRESEnabled != value)
                {
                    _blnRESEnabled = value;
                    if (value)
                    {
                        // Career mode, so no extra calculations need tobe done for EssenceAtSpecialStart
                        if (Created)
                        {
                            ResetCachedEssence();
                            EssenceAtSpecialStart = Essence();
                        }
                        // EssenceAtSpecialStart needs to be calculated by assuming that the character took the RES-enabling quality with the highest essence penalty first, as that would be the most optimal
                        else
                        {
                            // If this character has any RES-enabling bonuses that could be granted before all others (because they're priority and/or metatype-given),
                            // it has to be assumed those are taken first.
                            bool blnCountOnlyPriorityOrMetatypeGivenBonuses = Improvements.Any(x => x.ImproveType == Improvement.ImprovementType.Attribute && x.ImprovedName == "RES" &&
                                                                                                    (x.ImproveSource == Improvement.ImprovementSource.Metatype ||
                                                                                                     x.ImproveSource == Improvement.ImprovementSource.Metavariant ||
                                                                                                     x.ImproveSource == Improvement.ImprovementSource.Heritage) && x.Enabled);
                            Dictionary<string, decimal> dicImprovementEssencePenalties = new Dictionary<string, decimal>();
                            foreach (Improvement objImprovement in Improvements)
                            {
                                if ((!blnCountOnlyPriorityOrMetatypeGivenBonuses ||
                                     objImprovement.ImproveSource == Improvement.ImprovementSource.Metatype ||
                                     objImprovement.ImproveSource == Improvement.ImprovementSource.Metavariant ||
                                     objImprovement.ImproveSource == Improvement.ImprovementSource.Heritage) && objImprovement.Enabled)
                                {
                                    decimal decLoopEssencePenalty = 0;
                                    if (objImprovement.ImproveType == Improvement.ImprovementType.EssencePenalty)
                                    {
                                        decLoopEssencePenalty += objImprovement.Value;
                                    }
                                    else if (objImprovement.ImproveType == Improvement.ImprovementType.EssencePenaltyT100)
                                    {
                                        decLoopEssencePenalty += Convert.ToDecimal(objImprovement.Value) / 100.0m;
                                    }

                                    if (decLoopEssencePenalty != 0)
                                    {
                                        if (dicImprovementEssencePenalties.ContainsKey(objImprovement.SourceName))
                                            dicImprovementEssencePenalties[objImprovement.SourceName] = dicImprovementEssencePenalties[objImprovement.SourceName] + decLoopEssencePenalty;
                                        else
                                            dicImprovementEssencePenalties.Add(objImprovement.SourceName, decLoopEssencePenalty);
                                    }
                                }
                            }
                            if (dicImprovementEssencePenalties.Count > 0)
                                EssenceAtSpecialStart = ESS.MetatypeMaximum + dicImprovementEssencePenalties.Values.Min();
                            else
                                EssenceAtSpecialStart = ESS.MetatypeMaximum;
                        }
                        TechnomancerStream = "Default";
                    }
                    else
                    {
                        if (!Created && !DEPEnabled && !MAGEnabled)
                            EssenceAtSpecialStart = decimal.MinValue;
                        TechnomancerStream = string.Empty;
                    }

                    ImprovementManager.ClearCachedValue(this, Improvement.ImprovementType.MatrixInitiativeDice);
                    RESEnabledChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Is the DEP CharacterAttribute enabled?
        /// </summary>
        public bool DEPEnabled
        {
            get => _blnDEPEnabled;
            set
            {
                if (_blnDEPEnabled != value)
                {
                    _blnDEPEnabled = value;
                    if (value)
                    {
                        // Career mode, so no extra calculations need tobe done for EssenceAtSpecialStart
                        if (Created)
                        {
                            ResetCachedEssence();
                            EssenceAtSpecialStart = Essence();
                        }
                        // EssenceAtSpecialStart needs to be calculated by assuming that the character took the DEP-enabling quality with the highest essence penalty first, as that would be the most optimal
                        else
                        {
                            // If this character has any DEP-enabling bonuses that could be granted before all others (because they're priority and/or metatype-given),
                            // it has to be assumed those are taken first.
                            bool blnCountOnlyPriorityOrMetatypeGivenBonuses = Improvements.Any(x => x.ImproveType == Improvement.ImprovementType.Attribute && x.ImprovedName == "DEP" &&
                                                                                                    (x.ImproveSource == Improvement.ImprovementSource.Metatype ||
                                                                                                     x.ImproveSource == Improvement.ImprovementSource.Metavariant ||
                                                                                                     x.ImproveSource == Improvement.ImprovementSource.Heritage) && x.Enabled);
                            Dictionary<string, decimal> dicImprovementEssencePenalties = new Dictionary<string, decimal>();
                            foreach (Improvement objImprovement in Improvements)
                            {
                                if ((!blnCountOnlyPriorityOrMetatypeGivenBonuses ||
                                     objImprovement.ImproveSource == Improvement.ImprovementSource.Metatype ||
                                     objImprovement.ImproveSource == Improvement.ImprovementSource.Metavariant ||
                                     objImprovement.ImproveSource == Improvement.ImprovementSource.Heritage) && objImprovement.Enabled)
                                {
                                    decimal decLoopEssencePenalty = 0;
                                    if (objImprovement.ImproveType == Improvement.ImprovementType.EssencePenalty)
                                    {
                                        decLoopEssencePenalty += objImprovement.Value;
                                    }
                                    else if (objImprovement.ImproveType == Improvement.ImprovementType.EssencePenaltyT100)
                                    {
                                        decLoopEssencePenalty += Convert.ToDecimal(objImprovement.Value) / 100.0m;
                                    }

                                    if (decLoopEssencePenalty != 0)
                                    {
                                        if (dicImprovementEssencePenalties.ContainsKey(objImprovement.SourceName))
                                            dicImprovementEssencePenalties[objImprovement.SourceName] = dicImprovementEssencePenalties[objImprovement.SourceName] + decLoopEssencePenalty;
                                        else
                                            dicImprovementEssencePenalties.Add(objImprovement.SourceName, decLoopEssencePenalty);
                                    }
                                }
                            }
                            if (dicImprovementEssencePenalties.Count > 0)
                                EssenceAtSpecialStart = ESS.MetatypeMaximum + dicImprovementEssencePenalties.Values.Min();
                            else
                                EssenceAtSpecialStart = ESS.MetatypeMaximum;
                        }
                    }
                    else if (!Created && !RESEnabled && !MAGEnabled)
                        EssenceAtSpecialStart = decimal.MinValue;
                    DEPEnabledChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Submersion Grade.
        /// </summary>
        public int SubmersionGrade
        {
            get => _intSubmersionGrade;
            set => _intSubmersionGrade = value;
        }

        /// <summary>
        /// Whether or not the character is a member of a Group or Network.
        /// </summary>
        public bool GroupMember
        {
            get => _blnGroupMember;
            set => _blnGroupMember = value;
        }

        /// <summary>
        /// The name of the Group the Initiate has joined.
        /// </summary>
        public string GroupName
        {
            get => _strGroupName;
            set => _strGroupName = value;
        }

        /// <summary>
        /// Notes for the Group the Initiate has joined.
        /// </summary>
        public string GroupNotes
        {
            get => _strGroupNotes;
            set => _strGroupNotes = value;
        }

        /// <summary>
        /// Essence the character had when the first gained access to MAG/RES.
        /// </summary>
        public decimal EssenceAtSpecialStart
        {
            get => _decEssenceAtSpecialStart;
            set => _decEssenceAtSpecialStart = value;
        }

        private decimal _decCachedEssence = decimal.MinValue;
        public void ResetCachedEssence()
        {
            _decCachedEssence = decimal.MinValue;
        }
        /// <summary>
        /// Character's Essence.
        /// </summary>
        /// <param name="blnForMAGPenalty">Whether fetched Essence is to be used to calculate the penalty MAG should receive from lost Essence (true) or not (false).</param>
        public decimal Essence(bool blnForMAGPenalty = false)
        {
            if (!blnForMAGPenalty && _decCachedEssence != decimal.MinValue)
                return _decCachedEssence;
            // If the character has a fixed Essence Improvement, permanently fix their Essence at its value.
            if (_lstImprovements.Any(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.CyborgEssence && objImprovement.Enabled))
            {
                if (blnForMAGPenalty)
                    return 0.1m;
                return _decCachedEssence = 0.1m;
            }
            decimal decESS = ESS.MetatypeMaximum;
            // Run through all of the pieces of Cyberware and include their Essence cost. Cyberware and Bioware costs are calculated separately. The higher value removes its full cost from the
            // character's ESS while the lower removes half of its cost from the character's ESS.
            decimal decCyberware = 0m;
            decimal decBioware = 0m;
            decimal decHole = 0m;
            foreach (Cyberware objCyberware in Cyberware)
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
            if (blnForMAGPenalty)
                decESS += Convert.ToDecimal(ImprovementManager.ValueOf(this, Improvement.ImprovementType.EssencePenaltyMAGOnlyT100)) / 100.0m;

            decESS -= decCyberware + decBioware;
            // Deduct the Essence Hole value.
            decESS -= decHole;

            //1781 Essence is not printing
            //ESS.Base = Convert.ToInt32(decESS); -- Disabled becauses this messes up Character Validity, and it really shouldn't be what "Base" of an attribute is supposed to be (it's supposed to be extra levels gained)

            if (blnForMAGPenalty)
                return decESS;
            return _decCachedEssence = decESS;
        }

        /// <summary>
        /// Essence consumed by Cyberware.
        /// </summary>
        public decimal CyberwareEssence
        {
            get
            {
                // Run through all of the pieces of Cyberware and include their Essence cost. Cyberware and Bioware costs are calculated separately.
                return Cyberware.Where(objCyberware => !objCyberware.SourceID.Equals(Backend.Equipment.Cyberware.EssenceHoleGUID) && objCyberware.SourceType == Improvement.ImprovementSource.Cyberware).AsParallel().Sum(objCyberware => objCyberware.CalculatedESS());
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
                return Cyberware.Where(objCyberware => !objCyberware.SourceID.Equals(Backend.Equipment.Cyberware.EssenceHoleGUID) && objCyberware.SourceType == Improvement.ImprovementSource.Bioware).AsParallel().Sum(objCyberware => objCyberware.CalculatedESS());
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
                return Cyberware.Where(objCyberware => objCyberware.SourceID.Equals(Backend.Equipment.Cyberware.EssenceHoleGUID)).AsParallel().Sum(objCyberware => objCyberware.CalculatedESS());
            }
        }
        #region Initiative
#region Physical
        /// <summary>
        /// Physical Initiative.
        /// </summary>
        public string Initiative => LanguageManager.GetString("String_Initiative", GlobalOptions.Language)
            .Replace("{0}", InitiativeValue.ToString())
            .Replace("{1}", InitiativeDice.ToString());

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
                int intINI = (INT.TotalValue + REA.TotalValue) + WoundModifier;
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
        public string AstralInitiative => LanguageManager.GetString("String_Initiative", GlobalOptions.Language)
            .Replace("{0}", AstralInitiativeValue.ToString())
            .Replace("{1}", AstralInitiativeDice.ToString());

        public string GetAstralInitiative(CultureInfo objCulture, string strLanguageToPrint)
        {
            return LanguageManager.GetString("String_Initiative", strLanguageToPrint)
                    .Replace("{0}", AstralInitiativeValue.ToString(objCulture))
                    .Replace("{1}", AstralInitiativeDice.ToString(objCulture));
        }

        /// <summary>
        /// Astral Initiative Value.
        /// </summary>
        public int AstralInitiativeValue => (INT.TotalValue * 2) + WoundModifier;

        /// <summary>
        /// Astral Initiative Dice.
        /// </summary>
        public int AstralInitiativeDice => 3;

        #endregion
#region Matrix
#region AR
        /// <summary>
        /// Formatted AR Matrix Initiative.
        /// </summary>
        public string MatrixInitiative => LanguageManager.GetString("String_Initiative", GlobalOptions.Language)
            .Replace("{0}", MatrixInitiativeValue.ToString())
            .Replace("{1}", MatrixInitiativeDice.ToString());

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
                    int intINI = (INT.TotalValue) + WoundModifier;
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
                return INT.TotalValue + intCommlinkDP + WoundModifier + ImprovementManager.ValueOf(this, Improvement.ImprovementType.MatrixInitiative);
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
                return INT.TotalValue + intCommlinkDP + WoundModifier + ImprovementManager.ValueOf(this, Improvement.ImprovementType.MatrixInitiative);
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
        public int SpellResistance => ImprovementManager.ValueOf(this, Improvement.ImprovementType.SpellResistance);

        #endregion

#region Special CharacterAttribute Tests
        /// <summary>
        /// Composure (WIL + CHA).
        /// </summary>
        public int Composure => WIL.TotalValue + CHA.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.Composure);

        /// <summary>
        /// Judge Intentions (INT + CHA).
        /// </summary>
        public int JudgeIntentions => INT.TotalValue + CHA.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.JudgeIntentions) + ImprovementManager.ValueOf(this, Improvement.ImprovementType.JudgeIntentionsOffense);

        /// <summary>
        /// Judge Intentions Resist (CHA + WIL).
        /// </summary>
        public int JudgeIntentionsResist => CHA.TotalValue + WIL.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.JudgeIntentions) + ImprovementManager.ValueOf(this, Improvement.ImprovementType.JudgeIntentionsDefense);

        /// <summary>
        /// Lifting and Carrying (STR + BOD).
        /// </summary>
        public int LiftAndCarry => STR.TotalValue + BOD.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.LiftAndCarry);

        /// <summary>
        /// Memory (LOG + WIL).
        /// </summary>
        public int Memory => LOG.TotalValue + WIL.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.Memory);

        /// <summary>
        /// Resist test to Fatigue damage (BOD + WIL).
        /// </summary>
        public int FatigueResist => BOD.TotalValue + WIL.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.FatigueResist);

        /// <summary>
        /// Resist test to Radiation damage (BOD + WIL).
        /// </summary>
        public int RadiationResist => BOD.TotalValue + WIL.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.RadiationResist);

        /// <summary>
        /// Resist test to Sonic Attacks damage (WIL).
        /// </summary>
        public int SonicResist => WIL.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.SonicResist);

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
        public int PhysiologicalAddictionResistFirstTime => BOD.TotalValue + WIL.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.PhysiologicalAddictionFirstTime);

        /// <summary>
        /// Resist test to Psychological Addiction (LOG + WIL) if you are not addicted yet.
        /// </summary>
        public int PsychologicalAddictionResistFirstTime => LOG.TotalValue + WIL.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.PsychologicalAddictionFirstTime);

        /// <summary>
        /// Resist test to Physiological Addiction (BOD + WIL) if you are already addicted.
        /// </summary>
        public int PhysiologicalAddictionResistAlreadyAddicted => BOD.TotalValue + WIL.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.PhysiologicalAddictionAlreadyAddicted);

        /// <summary>
        /// Resist test to Psychological Addiction (LOG + WIL) if you are already addicted.
        /// </summary>
        public int PsychologicalAddictionResistAlreadyAddicted => LOG.TotalValue + WIL.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.PsychologicalAddictionAlreadyAddicted);

        /// <summary>
        /// Dicepool for natural recovery from Stun CM box damage (BOD + WIL).
        /// </summary>
        public int StunCMNaturalRecovery
        {
            get
            {
                // Matrix damage for A.I.s is not naturally repaired
                if (DEPEnabled && BOD.MetatypeMaximum == 0)
                    return 0;
                int intReturn = BOD.TotalValue + WIL.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.StunCMRecovery);
                if (Improvements.Any(x => x.Enabled && x.ImproveType == Improvement.ImprovementType.AddESStoStunCMRecovery))
                    intReturn += decimal.ToInt32(decimal.Floor(Essence()));
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
                if (DEPEnabled && BOD.MetatypeMaximum == 0)
                {
                    if (HomeNode is Vehicle)
                        return 0;

                    // A.I.s can restore Core damage via Software + Depth [Data Processing] (1 day) Extended Test
                    int intDEPTotal = DEP.TotalValue;
                    int intAIReturn = (SkillsSection.GetActiveSkill("Software")?.PoolOtherAttribute(intDEPTotal, "DEP") ?? intDEPTotal - 1) + ImprovementManager.ValueOf(this, Improvement.ImprovementType.PhysicalCMRecovery);
                    if (Improvements.Any(x => x.Enabled && x.ImproveType == Improvement.ImprovementType.AddESStoPhysicalCMRecovery))
                        intAIReturn += decimal.ToInt32(decimal.Floor(Essence()));
                    return intAIReturn;
                }
                int intReturn = 2 * BOD.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.PhysicalCMRecovery);
                if (Improvements.Any(x => x.Enabled && x.ImproveType == Improvement.ImprovementType.AddESStoPhysicalCMRecovery))
                    intReturn += decimal.ToInt32(decimal.Floor(Essence()));
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
        public int TotalStreetCred => Math.Max(CalculatedStreetCred + StreetCred + ImprovementManager.ValueOf(this, Improvement.ImprovementType.StreetCred), 0);

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
        public int TotalNotoriety => CalculatedNotoriety + Notoriety - (BurntStreetCred / 2);

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
        public ObservableCollection<Improvement> Improvements => _lstImprovements;

        /// <summary>
        /// Gear.
        /// </summary>
        public IList<MentorSpirit> MentorSpirits => _lstMentorSpirits;

        /// <summary>
        /// Contacts and Enemies.
        /// </summary>
        public ObservableCollection<Contact> Contacts => _lstContacts;

        /// <summary>
        /// Spirits and Sprites.
        /// </summary>
        public ObservableCollection<Spirit> Spirits => _lstSpirits;

        /// <summary>
        /// Magician Spells.
        /// </summary>
        public ObservableCollection<Spell> Spells => _lstSpells;

        /// <summary>
        /// Foci.
        /// </summary>
        public IList<Focus> Foci => _lstFoci;

        /// <summary>
        /// Stacked Foci.
        /// </summary>
        public IList<StackedFocus> StackedFoci => _lstStackedFoci;

        /// <summary>
        /// Adept Powers.
        /// </summary>
        public CachedBindingList<Power> Powers => _lstPowers;

        /// <summary>
        /// Technomancer Complex Forms.
        /// </summary>
        public ObservableCollection<ComplexForm> ComplexForms => _lstComplexForms;

        /// <summary>
        /// AI Programs and Advanced Programs
        /// </summary>
        public ObservableCollection<AIProgram> AIPrograms => _lstAIPrograms;

        /// <summary>
        /// Martial Arts.
        /// </summary>
        public ObservableCollection<MartialArt> MartialArts => _lstMartialArts;

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
        public ObservableCollection<LimitModifier> LimitModifiers => _lstLimitModifiers;

        /// <summary>
        /// Armor.
        /// </summary>
        public ObservableCollection<Armor> Armor => _lstArmor;

        /// <summary>
        /// Cyberware and Bioware.
        /// </summary>
        public ObservableCollection<Cyberware> Cyberware => _lstCyberware;

        /// <summary>
        /// Weapons.
        /// </summary>
        public ObservableCollection<Weapon> Weapons => _lstWeapons;

        /// <summary>
        /// Lifestyles.
        /// </summary>
        public ObservableCollection<Lifestyle> Lifestyles => _lstLifestyles;

        /// <summary>
        /// Gear.
        /// </summary>
        public ObservableCollection<Gear> Gear => _lstGear;

        /// <summary>
        /// Vehicles.
        /// </summary>
        public ObservableCollection<Vehicle> Vehicles => _lstVehicles;

        /// <summary>
        /// Metamagics and Echoes.
        /// </summary>
        public ObservableCollection<Metamagic> Metamagics => _lstMetamagics;

        /// <summary>
        /// Enhancements.
        /// </summary>
        public ObservableCollection<Enhancement> Enhancements => _lstEnhancements;

        /// <summary>
        /// Arts.
        /// </summary>
        public ObservableCollection<Art> Arts => _lstArts;

        /// <summary>
        /// Critter Powers.
        /// </summary>
        public ObservableCollection<CritterPower> CritterPowers => _lstCritterPowers;

        /// <summary>
        /// Initiation and Submersion Grades.
        /// </summary>
        public ObservableCollection<InitiationGrade> InitiationGrades => _lstInitiationGrades;

        /// <summary>
        /// Expenses (Karma and Nuyen).
        /// </summary>
        public ObservableCollection<ExpenseLogEntry> ExpenseEntries => _lstExpenseLog;

        /// <summary>
        /// Qualities (Positive and Negative).
        /// </summary>
        public ObservableCollection<Quality> Qualities => _lstQualities;

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
        public ObservableCollection<string> GearLocations => _lstGearLocations;

        /// <summary>
        /// Armor Bundles.
        /// </summary>
        public ObservableCollection<string> ArmorLocations => _lstArmorLocations;

        /// <summary>
        /// Vehicle Locations.
        /// </summary>
        public ObservableCollection<string> VehicleLocations => _lstVehicleLocations;

        /// <summary>
        /// Weapon Locations.
        /// </summary>
        public ObservableCollection<string> WeaponLocations => _lstWeaponLocations;

        /// <summary>
        /// Improvement Groups.
        /// </summary>
        public ObservableCollection<string> ImprovementGroups => _lstImprovementGroups;

        /// <summary>
        /// Calendar.
        /// </summary>
        public BindingList<CalendarWeek> Calendar => _lstCalendar;

        /// <summary>
        /// List of internal IDs that need their improvements re-applied.
        /// </summary>
        public IList<string> InternalIdsNeedingReapplyImprovements => _lstInternalIdsNeedingReapplyImprovements;

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
                foreach (Armor objArmor in Armor.Where(objArmor => !objArmor.ArmorValue.StartsWith('+') && objArmor.Equipped))
                {
                    int intArmorValue = objArmor.TotalArmor;
                    int intCustomStackBonus = 0;
                    string strArmorName = objArmor.Name;
                    if (objArmor.Category == "High-Fashion Armor Clothing")
                    {
                        foreach (Armor a in Armor.Where(a => (a.Category == "High-Fashion Armor Clothing" || a.ArmorOverrideValue.StartsWith('+')) && a.Equipped))
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
                foreach (Armor objArmor in Armor.Where(objArmor => (objArmor.ArmorValue.StartsWith('+') || objArmor.ArmorOverrideValue.StartsWith('+')) && objArmor.Name != strHighest && objArmor.Category == "Clothing" && objArmor.Equipped))
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
                foreach (Armor objArmor in Armor.Where(objArmor => (objArmor.ArmorValue.StartsWith('+') || objArmor.ArmorOverrideValue.StartsWith('+')) && objArmor.Name != strHighest && objArmor.Category != "Clothing" && objArmor.Equipped))
                {
                    bool blnDoAdd = true;
                    if (objArmor.Category == "High-Fashion Armor Clothing")
                    {
                        foreach (ArmorMod objMod in objArmor.ArmorMods)
                        {
                            if (objMod.Name == "Custom Fit (Stack)")
                            {
                                blnDoAdd = objMod.Extra == strHighest && !string.IsNullOrEmpty(strHighest);
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
        public int TotalArmorRating => ArmorRating + ImprovementManager.ValueOf(this, Improvement.ImprovementType.Armor);

        /// <summary>
        /// The Character's total Armor Rating against Fire attacks.
        /// </summary>
        public int TotalFireArmorRating => TotalArmorRating + ImprovementManager.ValueOf(this, Improvement.ImprovementType.FireArmor);

        /// <summary>
        /// The Character's total Armor Rating against Cold attacks.
        /// </summary>
        public int TotalColdArmorRating => TotalArmorRating + ImprovementManager.ValueOf(this, Improvement.ImprovementType.ColdArmor);

        /// <summary>
        /// The Character's total Armor Rating against Electricity attacks.
        /// </summary>
        public int TotalElectricityArmorRating => TotalArmorRating + ImprovementManager.ValueOf(this, Improvement.ImprovementType.ElectricityArmor);

        /// <summary>
        /// The Character's total Armor Rating against Acid attacks.
        /// </summary>
        public int TotalAcidArmorRating => TotalArmorRating + ImprovementManager.ValueOf(this, Improvement.ImprovementType.AcidArmor);

        /// <summary>
        /// The Character's total Armor Rating against falling damage (AP -4 not factored in).
        /// </summary>
        public int TotalFallingArmorRating => TotalArmorRating + ImprovementManager.ValueOf(this, Improvement.ImprovementType.FallingArmor);

        /// <summary>
        /// The Character's total bonus to Dodge Rating (to add on top of REA + INT).
        /// </summary>
        public int TotalBonusDodgeRating => ImprovementManager.ValueOf(this, Improvement.ImprovementType.Dodge);

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
                foreach (Armor objArmor in Armor.Where(objArmor => objArmor.Equipped && !objArmor.ArmorValue.StartsWith('+')))
                {
                    int intLoopTotal = objArmor.TotalArmor;
                    string strArmorName = objArmor.Name;
                    if (objArmor.Category == "High-Fashion Armor Clothing")
                    {
                        foreach (Armor a in Armor.Where(a => (a.Category == "High-Fashion Armor Clothing" || a.ArmorOverrideValue.StartsWith('+')) && a.Equipped))
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
                foreach (Armor objArmor in Armor.Where(objArmor => (objArmor.ArmorValue.StartsWith('+') || objArmor.ArmorOverrideValue.StartsWith('+')) && objArmor.Name != strHighest && objArmor.Category == "Clothing" && objArmor.Equipped))
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

                foreach (Armor objArmor in Armor.Where(objArmor => (objArmor.ArmorValue.StartsWith('+') || objArmor.ArmorOverrideValue.StartsWith('+')) && objArmor.Name != strHighest && objArmor.Category != "Clothing" && objArmor.Equipped))
                {
                    bool blnDoAdd = true;
                    if (objArmor.Category == "High-Fashion Armor Clothing")
                    {
                        foreach (ArmorMod objMod in objArmor.ArmorMods)
                        {
                            if (objMod.Name == "Custom Fit (Stack)")
                            {
                                blnDoAdd = objMod.Extra == strHighest && !string.IsNullOrEmpty(strHighest);
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
                    return (intSTRTotalValue - intTotalA) / 2;  // a negative number is expected
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
                if (DEPEnabled && BOD.MetatypeMaximum == 0)
                {
                    if (HomeNode is Vehicle objVehicle)
                    {
                        return objVehicle.PhysicalCM;
                    }

                    // A.I.s use Core Condition Monitors instead of Physical Condition Monitors if they are not in a vehicle or drone.
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
                if (DEPEnabled && BOD.MetatypeMaximum == 0)
                {
                    // A.I. do not have a Stun Condition Monitor, but they do have a Matrix Condition Monitor if they are in their home node.
                    if (HomeNode != null)
                    {
                        intCMStun = HomeNode.MatrixCM;
                    }
                }
                else
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
                if (Improvements.Any(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.IgnoreCMPenaltyPhysical && objImprovement.Enabled))
                    return int.MaxValue;
                if ((DEPEnabled && BOD.MetatypeMaximum == 0) || Improvements.Any(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.IgnoreCMPenaltyStun && objImprovement.Enabled))
                    return ImprovementManager.ValueOf(this, Improvement.ImprovementType.CMThresholdOffset) + ImprovementManager.ValueOf(this, Improvement.ImprovementType.CMSharedThresholdOffset);

                int intCMThresholdOffset = ImprovementManager.ValueOf(this, Improvement.ImprovementType.CMThresholdOffset);
                // We're subtracting CM Threshold from the amount of CM boxes filled because you only need to ignore wounds up to your first wound threshold, not all wounds
                int intCMSharedThresholdOffset = intCMThresholdOffset + ImprovementManager.ValueOf(this, Improvement.ImprovementType.CMSharedThresholdOffset) - Math.Max(StunCMFilled - CMThreshold - intCMThresholdOffset, 0);
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
                // A.I.s don't get wound penalties from Matrix damage
                if (DEPEnabled && BOD.MetatypeMaximum == 0)
                    return int.MaxValue;
                if (Improvements.Any(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.IgnoreCMPenaltyStun && objImprovement.Enabled))
                    return int.MaxValue;
                if (Improvements.Any(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.IgnoreCMPenaltyPhysical && objImprovement.Enabled))
                    return ImprovementManager.ValueOf(this, Improvement.ImprovementType.CMThresholdOffset) + ImprovementManager.ValueOf(this, Improvement.ImprovementType.CMSharedThresholdOffset);

                int intCMThresholdOffset = ImprovementManager.ValueOf(this, Improvement.ImprovementType.CMThresholdOffset);
                // We're subtracting CM Threshold from the amount of CM boxes filled because you only need to ignore wounds up to your first wound threshold, not all wounds
                int intCMSharedThresholdOffset = intCMThresholdOffset + ImprovementManager.ValueOf(this, Improvement.ImprovementType.CMSharedThresholdOffset) - Math.Max(PhysicalCMFilled - CMThreshold - intCMThresholdOffset, 0);
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
                if (!DEPEnabled || BOD.MetatypeMaximum != 0)
                {
                    // Characters get a number of overflow boxes equal to their BOD (plus any Improvements). One more boxes is added to mark the character as dead.
                    intCMOverflow = BOD.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.CMOverflow) + 1;
                }
                return intCMOverflow;
            }
        }
#endregion

#region Build Properties
        /// <summary>
        /// Method being used to build the character.
        /// </summary>
        public CharacterBuildMethod BuildMethod
        {
            get => _objBuildMethod;
            set
            {
                if (value != _objBuildMethod)
                {
                    _objBuildMethod = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BuildMethodHasSkillPoints)));
                }
            }
        }

        public bool BuildMethodHasSkillPoints => BuildMethod == CharacterBuildMethod.Priority || BuildMethod == CharacterBuildMethod.SumtoTen;
        
        /// <summary>
        /// Number of Build Points that are used to create the character.
        /// </summary>
        public int SumtoTen
        {
            get => _intSumtoTen;
            set => _intSumtoTen = value;
        }
        /// <summary>
        /// Amount of Karma that is used to create the character.
        /// </summary>
        public int BuildKarma
        {
            get => _intBuildKarma;
            set => _intBuildKarma = value;
        }

        /// <summary>
        /// Amount of Nuyen the character has.
        /// </summary>
        public decimal Nuyen
        {
            get => _decNuyen;
            set => _decNuyen = value;
        }

        /// <summary>
        /// Amount of Nuyen the character started with via the priority system.
        /// </summary>
        public decimal StartingNuyen
        {
            get => _decStartingNuyen;
            set => _decStartingNuyen = value;
        }

        /// <summary>
        /// Number of Build Points put into Nuyen.
        /// </summary>
        public decimal NuyenBP
        {
            get => _decNuyenBP;
            set => _decNuyenBP = value;
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
            set => _decNuyenMaximumBP = value;
        }

        /// <summary>
        /// The calculated Astral Limit.
        /// </summary>
        public int LimitAstral => Math.Max(LimitMental, LimitSocial);

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

                    intLimit = (CHA.TotalValue + intHomeNodeDP + WIL.TotalValue + decimal.ToInt32(decimal.Ceiling(Essence())) + 2) / 3;
                }
                else
                {
                    intLimit = (CHA.TotalValue * 2 + WIL.TotalValue + decimal.ToInt32(decimal.Ceiling(Essence())) + 2) / 3;
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
            get => _strMetatype;
            set => _strMetatype = value;
        }

        /// <summary>
        /// Character's Metavariant.
        /// </summary>
        public string Metavariant
        {
            get => _strMetavariant;
            set => _strMetavariant = value;
        }

        /// <summary>
        /// Metatype Category.
        /// </summary>
        public string MetatypeCategory
        {
            get => _strMetatypeCategory;
            set => _strMetatypeCategory = value;
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
            set => _strMovement = value;
        }

        /// <summary>
        /// Character's running Movement rate.
        /// <param name="strType">Takes one of three parameters: Ground, 2 for Swim, 3 for Fly. Returns 0 if the requested type isn't found.</param>
        /// </summary>
        private int WalkingRate(string strType = "Ground")
        {
            int intTmp = 0;
            if (Improvements.Any(i => i.ImproveType == Improvement.ImprovementType.WalkSpeed && i.ImprovedName == strType && i.Enabled))
            {
                foreach (Improvement objImprovement in Improvements.Where(i => i.ImproveType == Improvement.ImprovementType.WalkSpeed && i.ImprovedName == strType && i.Enabled))
                {
                    intTmp = Math.Max(intTmp, objImprovement.Value);
                }
                return intTmp;
            }

            string[] strReturn = AttributeSection.AttributeCategory == CharacterAttrib.AttributeCategory.Standard ? _strWalk.Split('/') : _strWalkAlt.Split('/');
            
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
            if (Improvements.Any(i => i.ImproveType == Improvement.ImprovementType.RunSpeed && i.ImprovedName == strType && i.Enabled))
            {
                foreach (Improvement objImprovement in Improvements.Where(i => i.ImproveType == Improvement.ImprovementType.RunSpeed && i.ImprovedName == strType && i.Enabled))
                {
                    intTmp = Math.Max(intTmp, objImprovement.Value);
                }
                return intTmp;
            }

            string[] strReturn = AttributeSection.AttributeCategory == CharacterAttrib.AttributeCategory.Standard ? _strRun.Split('/') : _strRunAlt.Split('/');
            
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
            if (Improvements.Any(i => i.ImproveType == Improvement.ImprovementType.SprintSpeed && i.ImprovedName == strType && i.Enabled))
            {
                foreach (Improvement objImprovement in Improvements.Where(i => i.ImproveType == Improvement.ImprovementType.SprintSpeed && i.ImprovedName == strType && i.Enabled))
                {
                    decTmp = Math.Max(decTmp, objImprovement.Value);
                }
                return decTmp;
            }

            string[] strReturn = AttributeSection.AttributeCategory == CharacterAttrib.AttributeCategory.Standard ? _strSprint.Split('/') : _strSprintAlt.Split('/');
            
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
            // Everything else after this just multiplies values, so zeroes can be checked for here
            if (decWalk == 0 && decRun == 0 && decSprint == 0)
            {
                return "0";
            }
            decSprint *= 1.0m + ImprovementManager.ValueOf(this, Improvement.ImprovementType.SprintBonusPercent, false, strMovementType) / 100.0m;
            decRun *= 1.0m + ImprovementManager.ValueOf(this, Improvement.ImprovementType.RunMultiplierPercent, false, strMovementType) / 100.0m;
            decWalk *= 1.0m + ImprovementManager.ValueOf(this, Improvement.ImprovementType.WalkMultiplierPercent, false, strMovementType) / 100.0m;

            int intAGI = AGI.CalculatedTotalValue(false);
            int intSTR = STR.CalculatedTotalValue(false);
            if (_objOptions.CyberlegMovement && blnUseCyberlegs && Cyberware.Any(objCyber => objCyber.LimbSlot == "leg"))
            {
                int intTempAGI = int.MaxValue;
                int intTempSTR = int.MaxValue;
                int intLegs = 0;
                foreach (Cyberware objCyber in Cyberware.Where(objCyber => objCyber.LimbSlot == "leg"))
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
            string strReturn;
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
            get => _intMetatypeBP;
            set => _intMetatypeBP = value;
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
            get => _blnAdeptEnabled;
            set
            {
                if (_blnAdeptEnabled != value)
                {
                    _blnAdeptEnabled = value;
                    AdeptTabEnabledChanged?.Invoke(this, EventArgs.Empty);
                    if (PropertyChanged != null)
                    {
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(AdeptEnabled)));
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsMysticAdept)));
                        if (!MagicianEnabled)
                        {
                            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(TraditionDrain)));
                            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayTraditionDrain)));
                        }
                    }
                    RefreshUseMysticAdeptPPs();
                }
            }
        }

        /// <summary>
        /// Whether or not Magician options are enabled.
        /// </summary>
        public bool MagicianEnabled
        {
            get => _blnMagicianEnabled;
            set
            {
                if (_blnMagicianEnabled != value)
                {
                    _blnMagicianEnabled = value;
                    MagicianTabEnabledChanged?.Invoke(this, EventArgs.Empty);
                    if (PropertyChanged != null)
                    {
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(MagicianEnabled)));
                        PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsMysticAdept)));
                        if (AdeptEnabled)
                        {
                            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(TraditionDrain)));
                            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayTraditionDrain)));
                        }
                    }
                    RefreshUseMysticAdeptPPs();
                }
            }
        }

        /// <summary>
        /// Whether or not Technomancer options are enabled.
        /// </summary>
        public bool TechnomancerEnabled
        {
            get => _blnTechnomancerEnabled;
            set
            {
                if (_blnTechnomancerEnabled != value)
                {
                    _blnTechnomancerEnabled = value;
                    TechnomancerTabEnabledChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Whether or not Advanced Program options are enabled.
        /// </summary>
        public bool AdvancedProgramsEnabled
        {
            get => _blnAdvancedProgramsEnabled;
            set
            {
                if (_blnAdvancedProgramsEnabled != value)
                {
                    _blnAdvancedProgramsEnabled = value;
                    AdvancedProgramsTabEnabledChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Whether or not Cyberware options are disabled.
        /// </summary>
        public bool CyberwareDisabled
        {
            get => _blnCyberwareDisabled;
            set
            {
                if (_blnCyberwareDisabled != value)
                {
                    _blnCyberwareDisabled = value;
                    CyberwareTabDisabledChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Whether or not the Initiation tab should be shown (override for BP mode).
        /// </summary>
        public bool InitiationEnabled
        {
            get => _blnInitiationEnabled;
            set
            {
                if (_blnInitiationEnabled != value)
                {
                    _blnInitiationEnabled = value;
                    InitiationTabEnabledChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Whether or not Critter options are enabled.
        /// </summary>
        public bool CritterEnabled
        {
            get => _blnCritterEnabled;
            set
            {
                if (_blnCritterEnabled != value)
                {
                    _blnCritterEnabled = value;
                    CritterTabEnabledChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Whether or not Black Market Discount is enabled.
        /// </summary>
        public bool BlackMarketDiscount
        {
            get => _blnBlackMarketDiscount;
            set => _blnBlackMarketDiscount = value;
        }

        /// <summary>
        /// Whether or not user is getting free bioware from Prototype Transhuman.
        /// </summary>
        public decimal PrototypeTranshuman
        {
            get => _decPrototypeTranshuman;
            set
            {
                if (value <= 0)
                {
                    if (_decPrototypeTranshuman > 0)
                        foreach (Cyberware objCyberware in Cyberware)
                            if (objCyberware.PrototypeTranshuman)
                                objCyberware.PrototypeTranshuman = false;
                    _decPrototypeTranshuman = 0;
                }
                else
                    _decPrototypeTranshuman = value;
            }
        }

        /// <summary>
        /// Whether or not Friends in High Places is enabled.
        /// </summary>
        public bool FriendsInHighPlaces
        {
            get => _blnFriendsInHighPlaces;
            set => _blnFriendsInHighPlaces = value;
        }

        /// <summary>
        /// Whether or not ExCon is enabled.
        /// </summary>
        public bool ExCon
        {
            get => _blnExCon;
            set
            {
                if (_blnExCon != value)
                {
                    _blnExCon = value;
                    ExConChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Value of the Trust Fund quality.
        /// </summary>
        public int TrustFund
        {
            get => _intTrustFund;
            set => _intTrustFund = value;
        }

        /// <summary>
        /// Whether or not RestrictedGear is enabled.
        /// </summary>
        public bool RestrictedGear
        {
            get => _blnRestrictedGear;
            set
            {
                if (_blnRestrictedGear != value)
                {
                    _blnRestrictedGear = value;
                    RestrictedGearChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Whether or not Overclocker is enabled.
        /// </summary>
        public bool Overclocker
        {
            get => _blnOverclocker;
            set => _blnOverclocker = value;
        }
        /// <summary>
        /// Whether or not MadeMan is enabled.
        /// </summary>
        public bool MadeMan
        {
            get => _blnMadeMan;
            set
            {
                if (_blnMadeMan != value)
                {
                    _blnMadeMan = value;
                    MadeManChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Whether or not Fame is enabled.
        /// </summary>
        public bool Fame
        {
            get => _blnFame;
            set => _blnFame = value;
        }

        /// <summary>
        /// Whether or not BornRich is enabled.
        /// </summary>
        public bool BornRich
        {
            get => _blnBornRich;
            set
            {
                if (_blnBornRich != value)
                {
                    _blnBornRich = value;
                    BornRichChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Whether or not Erased is enabled.
        /// </summary>
        public bool Erased
        {
            get => _blnErased;
            set => _blnErased = value;
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
        /// <param name="decCost">Item's cost.</param>
        /// <param name="strAvail">Item's Availability.</param>
        public string AvailTest(decimal decCost, string strAvail)
        {
            bool blnShowTest = false;
            string strTestSuffix = LanguageManager.GetString("String_AvailRestricted", GlobalOptions.Language);
            if (strAvail.EndsWith(strTestSuffix))
            {
                blnShowTest = true;
                strAvail = strAvail.TrimEndOnce(strTestSuffix, true);
            }
            else
            {
                strTestSuffix = LanguageManager.GetString("String_AvailForbidden", GlobalOptions.Language);
                if (strAvail.EndsWith(strTestSuffix))
                {
                    blnShowTest = true;
                    strAvail = strAvail.TrimEndOnce(strTestSuffix, true);
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
        /// <param name="decCost">Item's cost.</param>
        /// <param name="objAvailability">Item's Availability.</param>
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
#endregion

#region Application Properties
        private readonly List<Character> _lstLinkedCharacters = new List<Character>();
        /// <summary>
        /// Characters referenced by some member of this character (usually a contact).
        /// </summary>
        public IList<Character> LinkedCharacters => _lstLinkedCharacters;

        #endregion

#region Old Quality Conversion Code
        /// <summary>
        /// Convert Qualities that are still saved in the old format.
        /// </summary>
        private void ConvertOldQualities(XmlNodeList objXmlQualityList)
        {
            XmlNode xmlRootQualitiesNode = XmlManager.Load("qualities.xml").SelectSingleNode("/chummer/qualities");

            if (xmlRootQualitiesNode != null)
            {
                // Convert the old Qualities.
                foreach (XmlNode objXmlQuality in objXmlQualityList)
                {
                    if (objXmlQuality["name"] == null)
                    {
                        string strForceValue = string.Empty;

                        XmlNode objXmlQualityNode = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"" + GetQualityName(objXmlQuality.InnerText) + "\"]");

                        if (objXmlQualityNode != null)
                        {
                            // Re-create the bonuses for the Quality.
                            if (objXmlQualityNode.InnerXml.Contains("<bonus>"))
                            {
                                // Look for the existing Improvement.
                                foreach (Improvement objImprovement in _lstImprovements)
                                {
                                    if (objImprovement.ImproveSource == Improvement.ImprovementSource.Quality &&
                                        objImprovement.SourceName == objXmlQuality.InnerText && objImprovement.Enabled)
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
                }

                // Take care of the Metatype information.
                string strXPath = "/chummer/metatypes/metatype[name = \"" + _strMetatype + "\"]";
                XmlNode objXmlMetatype = XmlManager.Load("metatypes.xml").SelectSingleNode(strXPath) ??
                                         XmlManager.Load("critters.xml").SelectSingleNode(strXPath);

                if (objXmlMetatype != null)
                {
                    // Positive Qualities.
                    using (XmlNodeList xmlMetatypeQualityList = objXmlMetatype.SelectNodes("qualities/positive/quality"))
                        if (xmlMetatypeQualityList != null)
                            foreach (XmlNode objXmlMetatypeQuality in xmlMetatypeQualityList)
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
                                    string strForceValue = objXmlMetatypeQuality.Attributes?["select"]?.InnerText ?? string.Empty;
                                    Quality objQuality = new Quality(this);

                                    XmlNode objXmlQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"" + objXmlMetatypeQuality.InnerText + "\"]");
                                    objQuality.Create(objXmlQuality, QualitySource.Metatype, _lstWeapons, strForceValue);
                                    _lstQualities.Add(objQuality);
                                }
                            }

                    // Negative Qualities.
                    using (XmlNodeList xmlMetatypeQualityList = objXmlMetatype.SelectNodes("qualities/negative/quality"))
                        if (xmlMetatypeQualityList != null)
                            foreach (XmlNode objXmlMetatypeQuality in xmlMetatypeQualityList)
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
                                    string strForceValue = objXmlMetatypeQuality.Attributes?["select"]?.InnerText ?? string.Empty;
                                    Quality objQuality = new Quality(this);

                                    XmlNode objXmlQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"" + objXmlMetatypeQuality.InnerText + "\"]");
                                    objQuality.Create(objXmlQuality, QualitySource.Metatype, _lstWeapons, strForceValue);
                                    _lstQualities.Add(objQuality);
                                }
                            }

                    // Do it all over again for Metavariants.
                    if (!string.IsNullOrEmpty(_strMetavariant))
                    {
                        objXmlMetatype = objXmlMetatype.SelectSingleNode("metavariants/metavariant[name = \"" + _strMetavariant + "\"]");

                        if (objXmlMetatype != null)
                        {
                            // Positive Qualities.
                            using (XmlNodeList xmlMetatypeQualityList = objXmlMetatype.SelectNodes("qualities/positive/quality"))
                                if (xmlMetatypeQualityList != null)
                                    foreach (XmlNode objXmlMetatypeQuality in xmlMetatypeQualityList)
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
                                            string strForceValue = objXmlMetatypeQuality.Attributes?["select"]?.InnerText ?? string.Empty;
                                            Quality objQuality = new Quality(this);

                                            XmlNode objXmlQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"" + objXmlMetatypeQuality.InnerText + "\"]");
                                            objQuality.Create(objXmlQuality, QualitySource.Metatype, _lstWeapons, strForceValue);
                                            _lstQualities.Add(objQuality);
                                        }
                                    }

                            // Negative Qualities.
                            using (XmlNodeList xmlMetatypeQualityList = objXmlMetatype.SelectNodes("qualities/negative/quality"))
                                if (xmlMetatypeQualityList != null)
                                    foreach (XmlNode objXmlMetatypeQuality in xmlMetatypeQualityList)
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
                                            string strForceValue = objXmlMetatypeQuality.Attributes?["select"]?.InnerText ?? string.Empty;
                                            Quality objQuality = new Quality(this);

                                            XmlNode objXmlQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"" + objXmlMetatypeQuality.InnerText + "\"]");
                                            objQuality.Create(objXmlQuality, QualitySource.Metatype, _lstWeapons, strForceValue);
                                            _lstQualities.Add(objQuality);
                                        }
                                    }
                        }
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
        private bool CorrectedUnleveledQuality(XmlNode xmlOldQuality, XmlNode xmlRootQualitiesNode)
        {
            XmlNode xmlNewQuality = null;
            int intRanks = 0;
            switch (xmlOldQuality["name"]?.InnerText)
            {
                case "Focused Concentration (Rating 1)":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Focused Concentration\"]");
                        intRanks = 1;
                        break;
                    }
                case "Focused Concentration (Rating 2)":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Focused Concentration\"]");
                        intRanks = 2;
                        break;
                    }
                case "Focused Concentration (Rating 3)":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Focused Concentration\"]");
                        intRanks = 3;
                        break;
                    }
                case "Focused Concentration (Rating 4)":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Focused Concentration\"]");
                        intRanks = 4;
                        break;
                    }
                case "Focused Concentration (Rating 5)":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Focused Concentration\"]");
                        intRanks = 5;
                        break;
                    }
                case "Focused Concentration (Rating 6)":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Focused Concentration\"]");
                        intRanks = 6;
                        break;
                    }
                case "High Pain Tolerance (Rating 1)":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"High Pain Tolerance\"]");
                        intRanks = 1;
                        break;
                    }
                case "High Pain Tolerance (Rating 2)":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"High Pain Tolerance\"]");
                        intRanks = 2;
                        break;
                    }
                case "High Pain Tolerance (Rating 3)":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"High Pain Tolerance\"]");
                        intRanks = 3;
                        break;
                    }
                case "Magic Resistance (Rating 1)":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Magic Resistance\"]");
                        intRanks = 1;
                        break;
                    }
                case "Magic Resistance (Rating 2)":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Magic Resistance\"]");
                        intRanks = 2;
                        break;
                    }
                case "Magic Resistance (Rating 3)":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Magic Resistance\"]");
                        intRanks = 3;
                        break;
                    }
                case "Magic Resistance (Rating 4)":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Magic Resistance\"]");
                        intRanks = 4;
                        break;
                    }
                case "Will to Live (Rating 1)":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Will to Live\"]");
                        intRanks = 1;
                        break;
                    }
                case "Will to Live (Rating 2)":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Will to Live\"]");
                        intRanks = 2;
                        break;
                    }
                case "Will to Live (Rating 3)":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Will to Live\"]");
                        intRanks = 3;
                        break;
                    }
                case "Gremlins (Rating 1)":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Gremlins\"]");
                        intRanks = 1;
                        break;
                    }
                case "Gremlins (Rating 2)":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Gremlins\"]");
                        intRanks = 2;
                        break;
                    }
                case "Gremlins (Rating 3)":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Gremlins\"]");
                        intRanks = 3;
                        break;
                    }
                case "Gremlins (Rating 4)":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Gremlins\"]");
                        intRanks = 4;
                        break;
                    }
                case "Aged (Rating 1)":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Aged\"]");
                        intRanks = 1;
                        break;
                    }
                case "Aged (Rating 2)":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Aged\"]");
                        intRanks = 2;
                        break;
                    }
                case "Aged (Rating 3)":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Aged\"]");
                        intRanks = 3;
                        break;
                    }
                case "Illness (Rating 1)":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Illness\"]");
                        intRanks = 1;
                        break;
                    }
                case "Illness (Rating 2)":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Illness\"]");
                        intRanks = 2;
                        break;
                    }
                case "Illness (Rating 3)":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Illness\"]");
                        intRanks = 3;
                        break;
                    }
                case "Perceptive I":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Perceptive\"]");
                        intRanks = 1;
                        break;
                    }
                case "Perceptive II":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Perceptive\"]");
                        intRanks = 2;
                        break;
                    }
                case "Spike Resistance I":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Spike Resistance\"]");
                        intRanks = 1;
                        break;
                    }
                case "Spike Resistance II":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Spike Resistance\"]");
                        intRanks = 2;
                        break;
                    }
                case "Spike Resistance III":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Spike Resistance\"]");
                        intRanks = 3;
                        break;
                    }
                case "Tough as Nails Physical I":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Tough as Nails (Physical)\"]");
                        intRanks = 1;
                        break;
                    }
                case "Tough as Nails Physical II":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Tough as Nails (Physical)\"]");
                        intRanks = 2;
                        break;
                    }
                case "Tough as Nails Physical III":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Tough as Nails (Physical)\"]");
                        intRanks = 3;
                        break;
                    }
                case "Tough as Nails Stun I":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Tough as Nails (Stun)\"]");
                        intRanks = 1;
                        break;
                    }
                case "Tough as Nails Stun II":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Tough as Nails (Stun)\"]");
                        intRanks = 2;
                        break;
                    }
                case "Tough as Nails Stun III":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Tough as Nails (Stun)\"]");
                        intRanks = 3;
                        break;
                    }
                case "Dimmer Bulb I":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Dimmer Bulb\"]");
                        intRanks = 1;
                        break;
                    }
                case "Dimmer Bulb II":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Dimmer Bulb\"]");
                        intRanks = 2;
                        break;
                    }
                case "Dimmer Bulb III":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Dimmer Bulb\"]");
                        intRanks = 3;
                        break;
                    }
                case "In Debt I":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"In Debt\"]");
                        intRanks = 1;
                        break;
                    }
                case "In Debt II":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"In Debt\"]");
                        intRanks = 2;
                        break;
                    }
                case "In Debt III":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"In Debt\"]");
                        intRanks = 3;
                        break;
                    }
                case "In Debt IV":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"In Debt\"]");
                        intRanks = 4;
                        break;
                    }
                case "In Debt V":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"In Debt\"]");
                        intRanks = 5;
                        break;
                    }
                case "In Debt VI":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"In Debt\"]");
                        intRanks = 6;
                        break;
                    }
                case "In Debt VII":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"In Debt\"]");
                        intRanks = 7;
                        break;
                    }
                case "In Debt VIII":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"In Debt\"]");
                        intRanks = 8;
                        break;
                    }
                case "In Debt IX":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"In Debt\"]");
                        intRanks = 9;
                        break;
                    }
                case "In Debt X":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"In Debt\"]");
                        intRanks = 10;
                        break;
                    }
                case "In Debt XI":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"In Debt\"]");
                        intRanks = 11;
                        break;
                    }
                case "In Debt XII":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"In Debt\"]");
                        intRanks = 12;
                        break;
                    }
                case "In Debt XIII":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"In Debt\"]");
                        intRanks = 13;
                        break;
                    }
                case "In Debt XIV":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"In Debt\"]");
                        intRanks = 14;
                        break;
                    }
                case "In Debt XV":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"In Debt\"]");
                        intRanks = 15;
                        break;
                    }
                case "Infirm I":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Infirm\"]");
                        intRanks = 1;
                        break;
                    }
                case "Infirm II":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Infirm\"]");
                        intRanks = 2;
                        break;
                    }
                case "Infirm III":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Infirm\"]");
                        intRanks = 3;
                        break;
                    }
                case "Infirm IV":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Infirm\"]");
                        intRanks = 4;
                        break;
                    }
                case "Infirm V":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Infirm\"]");
                        intRanks = 5;
                        break;
                    }
                case "Shiva Arms (1 Pair)":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Shiva Arms (Pair)\"]");
                        intRanks = 1;
                        break;
                    }
                case "Shiva Arms (2 Pair)":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Shiva Arms (Pair)\"]");
                        intRanks = 2;
                        break;
                    }
                case "Arcane Arrester I":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Arcane Arrester\"]");
                        intRanks = 1;
                        break;
                    }
                case "Arcane Arrester II":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Arcane Arrester\"]");
                        intRanks = 2;
                        break;
                    }
                case "Pilot Origins (Rating 1)":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Pilot Origins\"]");
                        intRanks = 1;
                        break;
                    }
                case "Pilot Origins (Rating 2)":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Pilot Origins\"]");
                        intRanks = 2;
                        break;
                    }
                case "Pilot Origins (Rating 3)":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Pilot Origins\"]");
                        intRanks = 3;
                        break;
                    }
                case "Social Appearance Anxiety (Rating 1)":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Social Appearance Anxiety\"]");
                        intRanks = 1;
                        break;
                    }
                case "Social Appearance Anxiety (Rating 2)":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Social Appearance Anxiety\"]");
                        intRanks = 2;
                        break;
                    }
                case "Social Appearance Anxiety (Rating 3)":
                    {
                        xmlNewQuality = xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Social Appearance Anxiety\"]");
                        intRanks = 3;
                        break;
                    }
            }
            if (intRanks > 0)
            {
                for (int i = 0; i < intRanks; ++i)
                {
                    Quality objQuality = new Quality(this);
                    if (i == 0 && xmlOldQuality.TryGetField("guid", Guid.TryParse, out Guid guidOld))
                    {
                        ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.Quality, guidOld.ToString());
                        objQuality.SetGUID(guidOld);
                    }

                    QualitySource objQualitySource = Quality.ConvertToQualitySource(xmlOldQuality["qualitysource"]?.InnerText);
                    objQuality.Create(xmlNewQuality, objQualitySource, _lstWeapons, xmlOldQuality["extra"]?.InnerText);
                    if (xmlOldQuality["bp"] != null && int.TryParse(xmlOldQuality["bp"].InnerText, out int intOldBP))
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
            set => _initPasses = value;
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
        public string DisplayInit => Name + " : " + InitRoll.ToString();

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
        /// <param name="strLanguage">Language to fetch</param>
        public static string TranslatedBookList(string strInput, string strLanguage)
        {
            StringBuilder strReturn = new StringBuilder();
            string[] strArray = strInput.TrimEndOnce(';').Split(';');
            // Load the Sourcebook information.
            XmlDocument objXmlDocument = XmlManager.Load("books.xml", strLanguage);

            foreach (string strBook in strArray)
            {
                XmlNode objXmlBook = objXmlDocument.SelectSingleNode("/chummer/books/book[code = \"" + strBook + "\"]");
                if (objXmlBook != null)
                {
                    strReturn.Append(objXmlBook["translate"]?.InnerText ?? objXmlBook["name"]?.InnerText ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language));
                    strReturn.Append($" ({objXmlBook["altcode"]?.InnerText ?? strBook})");
                }
                else
                {
                    strReturn.Append(LanguageManager.GetString("String_Unknown", GlobalOptions.Language) + ' ' + strBook);
                }
            }
            return strReturn.ToString();
        }

#endregion

        //Can't be at improvementmanager due reasons
        private readonly Lazy<Stack<string>> _pushtext = new Lazy<Stack<string>>();
        private bool _blnAmbidextrous;

        /// <summary>
        /// Push a value that will be used instad of dialog instead in next <selecttext />
        /// </summary>
        public Stack<string> Pushtext => _pushtext.Value;

        /// <summary>
        /// The Active Commlink of the character. Returns null if character has no active commlink.
        /// </summary>
        public IHasMatrixAttributes ActiveCommlink { get; set; }

        private IHasMatrixAttributes _objHomeNode;

        /// <summary>
        /// Home Node. Returns null if home node is not set to any item.
        /// </summary>
        public IHasMatrixAttributes HomeNode
        {
            get => _objHomeNode;
            set
            {
                if (_objHomeNode != value)
                {
                    _objHomeNode = value;
                    RefreshWoundPenalties();
                }
            }
        }

        public SkillsSection SkillsSection { get; }

        public int RedlinerBonus
        {
            get => _intRedlinerBonus;
            set => _intRedlinerBonus = value;
        }

        /// <summary>
        /// Refreshes Redliner and Cyber-Singularity Seeker.
        /// </summary>
        public void RefreshRedliner()
        {
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
            }
            
            //Calculate bonus from cyberlimbs
            int intCount = 0;
            foreach (Cyberware objCyberware in Cyberware)
            {
                intCount += objCyberware.GetCyberlimbCount("skull", "torso");
            }
            intCount = Math.Min(intCount / 2, 2);
            RedlinerBonus = lstSeekerImprovements.Any(x => x.ImprovedName == "STR" || x.ImprovedName == "AGI") ? intCount : 0;

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
            //Improvement manager defines the functions needed to manipulate improvements
            //When the locals (someday) gets moved to this class, this can be removed and use
            //the local

            if (lstSeekerImprovements.Count == 0 && lstSeekerAttributes.Count == 0)
                return;

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
        }

        public void RefreshEssenceLossImprovements()
        {
            // Only worry about essence loss attribute modifiers if this character actually has any attributes that would be affected by essence loss
            // (which means EssenceAtSpecialStart is not set to decimal.MinValue)
            if (EssenceAtSpecialStart != decimal.MinValue)
            {
                decimal decESS = Essence();
                decimal decESSMag = Essence(true);
                if (!Options.DontRoundEssenceInternally)
                {
                    int intESSDecimals = Options.EssenceDecimals;
                    decESS = decimal.Round(decESS, intESSDecimals, MidpointRounding.AwayFromZero);
                    decESSMag = decimal.Round(decESSMag, intESSDecimals, MidpointRounding.AwayFromZero);
                }

                // Reduce a character's MAG and RES from Essence Loss.
                decimal decMetatypeMaximumESS = ESS.MetatypeMaximum;
                int intMagMaxReduction = decimal.ToInt32(decimal.Ceiling(decMetatypeMaximumESS - decESSMag));
                int intMaxReduction = decimal.ToInt32(decimal.Ceiling(decMetatypeMaximumESS - decESS));
                // Character has the option set where essence loss just acts as an augmented malus, so just replace old essence loss improvements with new ones that apply an augmented malus
                // equal to the amount by which the attribute's maximum would normally be reduced.
                if (Options.SpecialKarmaCostBasedOnShownValue)
                {
                    Improvement.ImprovementSource eEssenceLossSource = Created ? Improvement.ImprovementSource.EssenceLoss : Improvement.ImprovementSource.EssenceLossChargen;
                    ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.EssenceLoss);
                    ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.EssenceLossChargen);
                    if (intMaxReduction != 0)
                    {
                        ImprovementManager.CreateImprovement(this, "RES", eEssenceLossSource, string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, 0, -intMaxReduction);
                        ImprovementManager.CreateImprovement(this, "DEP", eEssenceLossSource, string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, 0, -intMaxReduction);
                    }
                    if (intMagMaxReduction != 0)
                    {
                        ImprovementManager.CreateImprovement(this, "MAG", eEssenceLossSource, string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, 0, -intMagMaxReduction);
                        ImprovementManager.CreateImprovement(this, "MAGAdept", eEssenceLossSource, string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, 0, -intMagMaxReduction);
                        // If this is a Mystic Adept using special Mystic Adept PP rules (i.e. no second MAG attribute), Mystic Adepts lose PPs even if they have fewer PPs than their MAG
                        if (UseMysticAdeptPPs)
                            ImprovementManager.CreateImprovement(this, string.Empty, eEssenceLossSource, string.Empty, Improvement.ImprovementType.AdeptPowerPoints, string.Empty, -intMagMaxReduction);
                    }
                }
                // RAW Career mode: complicated. Similar to RAW Create mode, but with the extra possibility of burning current karma levels and/or PPs instead of pure minima reduction,
                // plus the need to account for cases where a character will burn "past" 0 (i.e. to a current value that should be negative), but then upgrade to 1 afterwards.
                else if (Created)
                {
                    // "Base" minimum reduction. This is the amount by which the character's special attribute minima would be reduced across career and create modes if there wasn't any funny business
                    int intMinReduction = decimal.ToInt32(decimal.Ceiling(EssenceAtSpecialStart - decESS));
                    int intMagMinReduction = decimal.ToInt32(decimal.Ceiling(EssenceAtSpecialStart - decESSMag));

                    // This extra code is needed for legacy shims, to convert proper attribute values for characters who would end up having a higher level than their total attribute maxima
                    // They are extra amounts by which the relevant attributes' karma levels should be burned
                    int intExtraRESBurn = Math.Max(0,
                        Math.Max(RES.Base + RES.FreeBase + RES.RawMinimum + RES.AttributeValueModifiers, RES.TotalMinimum) + RES.Karma - RES.TotalMaximum);
                    int intExtraDEPBurn = Math.Max(0,
                        Math.Max(DEP.Base + DEP.FreeBase + DEP.RawMinimum + DEP.AttributeValueModifiers, DEP.TotalMinimum) + DEP.Karma - DEP.TotalMaximum);
                    int intExtraMAGBurn = Math.Max(0,
                        Math.Max(MAG.Base + MAG.FreeBase + MAG.RawMinimum + MAG.AttributeValueModifiers, MAG.TotalMinimum) + MAG.Karma - MAG.TotalMaximum);
                    int intExtraMAGAdeptBurn = Math.Max(0,
                        Math.Max(MAGAdept.Base + MAGAdept.FreeBase + MAGAdept.RawMinimum + MAGAdept.AttributeValueModifiers, MAGAdept.TotalMinimum) + MAGAdept.Karma - MAGAdept.TotalMaximum);
                    // Old values for minimum reduction from essence loss in career mode. These are used to determine if any karma needs to get burned.
                    int intOldRESCareerMinimumReduction = 0;
                    int intOldDEPCareerMinimumReduction = 0;
                    int intOldMAGCareerMinimumReduction = 0;
                    int intOldMAGAdeptCareerMinimumReduction = 0;
                    foreach (Improvement objImprovement in Improvements)
                    {
                        if (objImprovement.ImproveSource == Improvement.ImprovementSource.EssenceLoss && objImprovement.ImproveType == Improvement.ImprovementType.Attribute && objImprovement.Enabled)
                        {
                            // Values get subtracted because negative modifier = positive reduction, positive modifier = negative reduction
                            // Augmented values also get factored in in case the character is switching off the option to treat essence loss as an augmented malus
                            switch (objImprovement.ImprovedName)
                            {
                                case "RES":
                                    intOldRESCareerMinimumReduction -= objImprovement.Minimum + objImprovement.Augmented;
                                    break;
                                case "DEP":
                                    intOldDEPCareerMinimumReduction -= objImprovement.Minimum + objImprovement.Augmented;
                                    break;
                                case "MAG":
                                    intOldMAGCareerMinimumReduction -= objImprovement.Minimum + objImprovement.Augmented;
                                    break;
                                case "MAGAdept":
                                    intOldMAGAdeptCareerMinimumReduction -= objImprovement.Minimum + objImprovement.Augmented;
                                    break;
                            }
                        }
                    }

                    // Remove any Improvements from MAG, RES, and DEP from Essence Loss that were added in career.
                    ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.EssenceLoss);

                    // Career Minimum and Maximum reduction relies on whether there's any extra reduction since chargen.
                    // This is the step where create mode attribute loss regarding attribute maximum loss gets factored out.
                    int intRESMaximumReduction = intMaxReduction + RES.TotalMaximum - RES.MaximumNoEssenceLoss();
                    int intDEPMaximumReduction = intMaxReduction + DEP.TotalMaximum - DEP.MaximumNoEssenceLoss();
                    int intMAGMaximumReduction = intMagMaxReduction + MAG.TotalMaximum - MAG.MaximumNoEssenceLoss();
                    int intMAGAdeptMaximumReduction = intMagMaxReduction + MAGAdept.TotalMaximum - MAGAdept.MaximumNoEssenceLoss();

                    // Create the Essence Loss (or gain, in case of essence restoration and increasing maxima) Improvements.
                    if (intMaxReduction > 0 || intMinReduction > 0 || intRESMaximumReduction != 0 || intDEPMaximumReduction != 0)
                    {
                        // This is the step where create mode attribute loss regarding attribute minimum loss gets factored out.
                        int intRESMinimumReduction;
                        int intDEPMinimumReduction;
                        // If only maxima would be reduced, use the attribute's current total value instead of its current maximum, as this makes sure minima will only get reduced if the maximum reduction would eat into the current value
                        if (Options.ESSLossReducesMaximumOnly)
                        {
                            intRESMinimumReduction = Math.Max(0, intMinReduction + RES.TotalValue - RES.MaximumNoEssenceLoss(true));
                            intDEPMinimumReduction = Math.Max(0, intMinReduction + DEP.TotalValue - DEP.MaximumNoEssenceLoss(true));
                        }
                        else
                        {
                            intRESMinimumReduction = intMinReduction + RES.TotalMaximum - RES.MaximumNoEssenceLoss(true);
                            intDEPMinimumReduction = intMinReduction + DEP.TotalMaximum - DEP.MaximumNoEssenceLoss(true);
                        }
                        
                        // If the new RES reduction is greater than the old one...
                        int intRESMinimumReductionDelta = intRESMinimumReduction - intOldRESCareerMinimumReduction;
                        if (intRESMinimumReductionDelta > 0)
                        {
                            // ... and adding minimum reducing-improvements wouldn't do anything, start burning karma.
                            if (intRESMinimumReduction > RES.Base + RES.FreeBase + RES.RawMinimum + RES.AttributeValueModifiers)
                            {
                                // intRESMinimumReduction is not actually reduced so that karma doesn't get burned away each time this function is called.
                                // Besides, this only fires if intRESMinimumReduction is already at a level where increasing it any more wouldn't have any effect on the character.
                                intExtraRESBurn += Math.Min(RES.Karma, intRESMinimumReductionDelta);
                                RES.Karma -= intExtraRESBurn;
                            }
                        }
                        // If the new RES reduction is less than our old one, the character doesn't actually get any new values back
                        else
                        {
                            intRESMinimumReduction = intOldRESCareerMinimumReduction;
                        }
                        // If the new DEP reduction is greater than the old one...
                        int intDEPMinimumReductionDelta = intDEPMinimumReduction - intOldDEPCareerMinimumReduction;
                        if (intDEPMinimumReductionDelta > 0)
                        {
                            // ... and adding minimum reducing-improvements wouldn't do anything, start burning karma.
                            if (intDEPMinimumReduction > DEP.Base + DEP.FreeBase + DEP.RawMinimum + DEP.AttributeValueModifiers)
                            {
                                // intDEPMinimumReduction is not actually reduced so that karma doesn't get burned away each time this function is called.
                                // Besides, this only fires if intDEPMinimumReduction is already at a level where increasing it any more wouldn't have any effect on the character.
                                intExtraDEPBurn += Math.Min(DEP.Karma, intDEPMinimumReductionDelta);
                                DEP.Karma -= intExtraDEPBurn;
                            }
                        }
                        // If the new DEP reduction is less than our old one, the character doesn't actually get any new values back
                        else
                        {
                            intDEPMinimumReduction = intOldDEPCareerMinimumReduction;
                        }
                        // Create Improvements
                        if (intRESMinimumReduction != 0 || intRESMaximumReduction != 0)
                            ImprovementManager.CreateImprovement(this, "RES", Improvement.ImprovementSource.EssenceLoss, string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1, -intRESMinimumReduction, -intRESMaximumReduction);
                        if (intDEPMinimumReduction != 0 || intDEPMaximumReduction != 0)
                            ImprovementManager.CreateImprovement(this, "DEP", Improvement.ImprovementSource.EssenceLoss, string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1, -intDEPMinimumReduction, -intDEPMaximumReduction);
                    }

                    if (intMagMaxReduction > 0 || intMagMinReduction > 0 || intMAGMaximumReduction != 0 || intMAGAdeptMaximumReduction != 0)
                    {
                        // This is the step where create mode attribute loss regarding attribute minimum loss gets factored out.
                        int intMAGMinimumReduction;
                        int intMAGAdeptMinimumReduction;
                        // If only maxima would be reduced, use the attribute's current total value instead of its current maximum, as this makes sure minima will only get reduced if the maximum reduction would eat into the current value
                        if (Options.ESSLossReducesMaximumOnly)
                        {
                            intMAGMinimumReduction = Math.Max(0, intMagMinReduction + MAG.TotalValue - MAG.MaximumNoEssenceLoss(true));
                            intMAGAdeptMinimumReduction = Math.Max(0, intMagMinReduction + MAGAdept.TotalValue - MAGAdept.MaximumNoEssenceLoss(true));
                        }
                        else
                        {
                            intMAGMinimumReduction = intMagMinReduction + MAG.TotalMaximum - MAG.MaximumNoEssenceLoss(true);
                            intMAGAdeptMinimumReduction = intMagMinReduction + MAGAdept.TotalMaximum - MAGAdept.MaximumNoEssenceLoss(true);
                        }
                        
                        // If the new MAG reduction is greater than the old one...
                        int intMAGMinimumReductionDelta = intMAGMinimumReduction - intOldMAGCareerMinimumReduction;
                        if (intMAGMinimumReductionDelta > 0)
                        {
                            // ... and adding minimum reducing-improvements wouldn't do anything, start burning karma.
                            if (intMAGMinimumReduction > MAG.Base + MAG.FreeBase + MAG.RawMinimum + MAG.AttributeValueModifiers)
                            {
                                // intMAGMinimumReduction is not actually reduced so that karma doesn't get burned away each time this function is called.
                                // Besides, this only fires if intMAGMinimumReduction is already at a level where increasing it any more wouldn't have any effect on the character.
                                intExtraMAGBurn += Math.Min(MAG.Karma, intMAGMinimumReductionDelta);
                                MAG.Karma -= intExtraMAGBurn;
                            }

                            // Mystic Adept PPs may need to be burned away based on the change of our MAG attribute
                            if (UseMysticAdeptPPs)
                            {
                                // First burn away PPs gained during chargen...
                                int intPPBurn = Math.Min(MysticAdeptPowerPoints, intMAGMinimumReductionDelta);
                                MysticAdeptPowerPoints -= intPPBurn;
                                // ... now burn away PPs gained from initiations.
                                intPPBurn = Math.Min(intMAGMinimumReductionDelta - intPPBurn, ImprovementManager.ValueOf(this, Improvement.ImprovementType.AdeptPowerPoints));
                                // Source needs to be EssenceLossChargen so that it doesn't get wiped in career mode.
                                if (intPPBurn != 0)
                                    ImprovementManager.CreateImprovement(this, string.Empty, Improvement.ImprovementSource.EssenceLossChargen, string.Empty, Improvement.ImprovementType.AdeptPowerPoints, string.Empty, -intPPBurn);
                            }
                        }
                        // If the new MAG reduction is less than our old one, the character doesn't actually get any new values back
                        else
                        {
                            intMAGMinimumReduction = intOldMAGCareerMinimumReduction;
                        }

                        // Make sure we only attempt to burn MAGAdept karma levels if it's actually a separate attribute from MAG
                        if (MAGAdept != MAG)
                        {
                            // If the new MAGAdept reduction is greater than the old one...
                            int intMAGAdeptMinimumReductionDelta = intMAGAdeptMinimumReduction - intOldMAGAdeptCareerMinimumReduction;
                            if (intMAGAdeptMinimumReductionDelta > 0)
                            {
                                // ... and adding minimum reducing-improvements wouldn't do anything, start burning karma.
                                if (intMAGAdeptMinimumReduction > MAGAdept.Base + MAGAdept.FreeBase + MAGAdept.RawMinimum + MAGAdept.AttributeValueModifiers)
                                {
                                    // intMAGAdeptMinimumReduction is not actually reduced so that karma doesn't get burned away each time this function is called.
                                    // Besides, this only fires if intMAGAdeptMinimumReduction is already at a level where increasing it any more wouldn't have any effect on the character.
                                    intExtraMAGAdeptBurn += Math.Min(MAGAdept.Karma, intMAGAdeptMinimumReductionDelta);
                                    MAGAdept.Karma -= intExtraMAGAdeptBurn;
                                }
                            }
                            // If the new MAGAdept reduction is less than our old one, the character doesn't actually get any new values back
                            else
                            {
                                intMAGAdeptMinimumReduction = intOldMAGAdeptCareerMinimumReduction;
                            }
                        }
                        // Otherwise make sure that if the new MAGAdept reduction is less than our old one, the character doesn't actually get any new values back
                        else if (intMAGAdeptMinimumReduction < intOldMAGAdeptCareerMinimumReduction)
                        {
                            intMAGAdeptMinimumReduction = intOldMAGAdeptCareerMinimumReduction;
                        }
                        // Create Improvements
                        if (intMAGMinimumReduction != 0 || intMAGMaximumReduction != 0)
                            ImprovementManager.CreateImprovement(this, "MAG", Improvement.ImprovementSource.EssenceLoss, string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1, -intMAGMinimumReduction, -intMAGMaximumReduction);
                        if (intMAGAdeptMinimumReduction != 0 || intMAGAdeptMaximumReduction != 0)
                            ImprovementManager.CreateImprovement(this, "MAGAdept", Improvement.ImprovementSource.EssenceLoss, string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1, -intMAGAdeptMinimumReduction, -intMAGAdeptMaximumReduction);
                    }
                }
                // RAW Create mode: Reduce maxima based on max ESS - current ESS, reduce minima based on their essence from the most optimal way in which they could have gotten access to special attributes
                else
                {
                    int intMinReduction = decimal.ToInt32(decimal.Ceiling(EssenceAtSpecialStart - decESS));
                    int intMagMinReduction = decimal.ToInt32(decimal.Ceiling(EssenceAtSpecialStart - decESSMag));
                    int intRESMinimumReduction = intMinReduction;
                    int intDEPMinimumReduction = intMinReduction;
                    int intMAGMinimumReduction = intMagMinReduction;
                    int intMAGAdeptMinimumReduction = intMagMinReduction;
                    if (Options.ESSLossReducesMaximumOnly)
                    {
                        intRESMinimumReduction = Math.Max(0, intMinReduction + RES.TotalValue - RES.TotalMaximum);
                        intDEPMinimumReduction = Math.Max(0, intMinReduction + DEP.TotalValue - DEP.TotalMaximum);
                        intMAGMinimumReduction = Math.Max(0, intMagMinReduction + MAG.TotalValue - MAG.TotalMaximum);
                        intMAGAdeptMinimumReduction = Math.Max(0, intMagMinReduction + MAGAdept.TotalValue - MAGAdept.TotalMaximum);
                    }

                    ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.EssenceLoss);
                    ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.EssenceLossChargen);
                    if (intMaxReduction != 0 || intRESMinimumReduction != 0 || intDEPMinimumReduction != 0)
                    {
                        ImprovementManager.CreateImprovement(this, "RES", Improvement.ImprovementSource.EssenceLossChargen, string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1, -intRESMinimumReduction, -intMaxReduction);
                        ImprovementManager.CreateImprovement(this, "DEP", Improvement.ImprovementSource.EssenceLossChargen, string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1, -intDEPMinimumReduction, -intMaxReduction);
                    }
                    if (intMagMaxReduction != 0 || intMAGMinimumReduction != 0 || intMAGAdeptMinimumReduction != 0)
                    {
                        ImprovementManager.CreateImprovement(this, "MAG", Improvement.ImprovementSource.EssenceLossChargen, string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1, -intMAGMinimumReduction, -intMagMaxReduction);
                        ImprovementManager.CreateImprovement(this, "MAGAdept", Improvement.ImprovementSource.EssenceLossChargen, string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1, -intMAGAdeptMinimumReduction, -intMagMaxReduction);
                    }
                }
                
                ImprovementManager.Commit(this);

                // If the character is in Career mode, it is possible for them to be forced to burn out.
                if (Created)
                {
                    // If the CharacterAttribute reaches 0, the character has burned out.
                    if (MAGEnabled)
                    {
                        if (Options.SpecialKarmaCostBasedOnShownValue)
                        {
                            if (Options.MysAdeptSecondMAGAttribute && IsMysticAdept)
                            {
                                if (intMagMaxReduction >= MAG.TotalMaximum)
                                {
                                    MAG.Base = MAGAdept.Base;
                                    MAG.Karma = MAGAdept.Karma;
                                    MAG.MetatypeMinimum = MAGAdept.MetatypeMinimum;
                                    MAG.MetatypeMaximum = MAGAdept.MetatypeMaximum;
                                    MAG.MetatypeAugmentedMaximum = MAGAdept.MetatypeAugmentedMaximum;
                                    MAGAdept.Base = 0;
                                    MAGAdept.Karma = 0;
                                    MAGAdept.MetatypeMinimum = 0;
                                    MAGAdept.MetatypeMaximum = 0;
                                    MAGAdept.MetatypeAugmentedMaximum = 0;

                                    MagicianEnabled = false;
                                }

                                if (intMagMaxReduction >= MAGAdept.TotalMaximum)
                                {
                                    MAGAdept.Base = 0;
                                    MAGAdept.Karma = 0;
                                    MAGAdept.MetatypeMinimum = 0;
                                    MAGAdept.MetatypeMaximum = 0;
                                    MAGAdept.MetatypeAugmentedMaximum = 0;

                                    AdeptEnabled = false;
                                }

                                if (!MagicianEnabled && !AdeptEnabled)
                                    MAGEnabled = false;
                            }
                            else if (intMagMaxReduction >= MAG.TotalMaximum)
                            {
                                MAG.Base = 0;
                                MAG.Karma = 0;
                                MAG.MetatypeMinimum = 0;
                                MAG.MetatypeMaximum = 0;
                                MAG.MetatypeAugmentedMaximum = 0;

                                MagicianEnabled = false;
                                AdeptEnabled = false;
                                MAGEnabled = false;
                            }
                        }
                        else
                        {
                            if (Options.MysAdeptSecondMAGAttribute && IsMysticAdept)
                            {
                                if (MAG.TotalMaximum < 1)
                                {
                                    MAG.Base = MAGAdept.Base;
                                    MAG.Karma = MAGAdept.Karma;
                                    MAG.MetatypeMinimum = MAGAdept.MetatypeMinimum;
                                    MAG.MetatypeMaximum = MAGAdept.MetatypeMaximum;
                                    MAG.MetatypeAugmentedMaximum = MAGAdept.MetatypeAugmentedMaximum;
                                    MAGAdept.Base = 0;
                                    MAGAdept.Karma = 0;
                                    MAGAdept.MetatypeMinimum = 0;
                                    MAGAdept.MetatypeMaximum = 0;
                                    MAGAdept.MetatypeAugmentedMaximum = 0;

                                    MagicianEnabled = false;
                                }

                                if (MAGAdept.TotalMaximum < 1)
                                {
                                    MAGAdept.Base = 0;
                                    MAGAdept.Karma = 0;
                                    MAGAdept.MetatypeMinimum = 0;
                                    MAGAdept.MetatypeMaximum = 0;
                                    MAGAdept.MetatypeAugmentedMaximum = 0;

                                    AdeptEnabled = false;
                                }

                                if (!MagicianEnabled && !AdeptEnabled)
                                    MAGEnabled = false;
                            }
                            else if (MAG.TotalMaximum < 1)
                            {
                                MAG.Base = 0;
                                MAG.Karma = 0;
                                MAG.MetatypeMinimum = 0;
                                MAG.MetatypeMaximum = 0;
                                MAG.MetatypeAugmentedMaximum = 0;

                                MagicianEnabled = false;
                                AdeptEnabled = false;
                                MAGEnabled = false;
                            }

                            if (MysticAdeptPowerPoints > 0)
                            {
                                int intMAGTotal = MAG.TotalValue;
                                if (MysticAdeptPowerPoints > intMAGTotal)
                                    MysticAdeptPowerPoints = intMAGTotal;
                            }
                        }
                    }

                    if (RESEnabled && (Options.SpecialKarmaCostBasedOnShownValue && intMaxReduction >= RES.TotalMaximum || !Options.SpecialKarmaCostBasedOnShownValue && RES.TotalMaximum < 1))
                    {
                        RES.Base = 0;
                        RES.Karma = 0;
                        RES.MetatypeMinimum = 0;
                        RES.MetatypeMinimum = 0;
                        RES.MetatypeAugmentedMaximum = 0;

                        RESEnabled = false;
                        TechnomancerEnabled = false;
                    }
                }
            }
            // Otherwise any essence loss improvements that might have been left need to be deleted (e.g. character is in create mode and had access to special attributes, but that access was removed)
            else
            {
                ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.EssenceLossChargen);
                ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.EssenceLoss);
                ImprovementManager.Commit(this);
            }

            // If the character is Cyberzombie, adjust their Attributes based on their Essence.
            if (MetatypeCategory == "Cyberzombie")
            {
                int intESSModifier = decimal.ToInt32(decimal.Ceiling(Essence() - ESS.MetatypeMaximum));
                ImprovementManager.RemoveImprovements(this, Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.Cyberzombie && x.ImproveType == Improvement.ImprovementType.Attribute).ToList());
                if (intESSModifier != 0)
                {
                    ImprovementManager.CreateImprovement(this, "BOD", Improvement.ImprovementSource.Cyberzombie, string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, intESSModifier);
                    ImprovementManager.CreateImprovement(this, "AGI", Improvement.ImprovementSource.Cyberzombie, string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, intESSModifier);
                    ImprovementManager.CreateImprovement(this, "REA", Improvement.ImprovementSource.Cyberzombie, string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, intESSModifier);
                    ImprovementManager.CreateImprovement(this, "STR", Improvement.ImprovementSource.Cyberzombie, string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, intESSModifier);
                    ImprovementManager.CreateImprovement(this, "CHA", Improvement.ImprovementSource.Cyberzombie, string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, intESSModifier);
                    ImprovementManager.CreateImprovement(this, "INT", Improvement.ImprovementSource.Cyberzombie, string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, intESSModifier);
                    ImprovementManager.CreateImprovement(this, "LOG", Improvement.ImprovementSource.Cyberzombie, string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, intESSModifier);
                    ImprovementManager.CreateImprovement(this, "WIL", Improvement.ImprovementSource.Cyberzombie, string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, intESSModifier);
                    ImprovementManager.Commit(this);
                }
            }
        }

        public void RefreshEncumbranceFromSTR(object sender, PropertyChangedEventArgs e)
        {
            // Encumbrance is only affected by STR.TotalValue when it comes to attributes
            if (e.PropertyName == nameof(CharacterAttrib.TotalValue))
            {
                RefreshEncumbrance();
            }
        }

        public void RefreshEncumbrance()
        {
            // Remove any Improvements from Armor Encumbrance.
            ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.ArmorEncumbrance);
            if (!Options.NoArmorEncumbrance)
            {
                // Create the Armor Encumbrance Improvements.
                int intEncumbrance = ArmorEncumbrance;
                if (intEncumbrance != 0)
                {
                    ImprovementManager.CreateImprovement(this, "AGI", Improvement.ImprovementSource.ArmorEncumbrance, string.Empty, Improvement.ImprovementType.Attribute, "precedence-1", 0, 1, 0, 0, intEncumbrance);
                    ImprovementManager.CreateImprovement(this, "REA", Improvement.ImprovementSource.ArmorEncumbrance, string.Empty, Improvement.ImprovementType.Attribute, "precedence-1", 0, 1, 0, 0, intEncumbrance);
                    ImprovementManager.Commit(this);
                }
            }
        }

        public void RefreshWoundPenalties()
        {
            int intPhysicalCMFilled = Math.Min(PhysicalCMFilled, PhysicalCM);
            int intStunCMFilled = Math.Min(StunCMFilled, StunCM);
            int intCMThreshold = CMThreshold;
            int intStunCMPenalty = Improvements.Any(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.IgnoreCMPenaltyStun && objImprovement.Enabled)
                ? 0
                : (StunCMThresholdOffset - intStunCMFilled) / intCMThreshold;
            int intPhysicalCMPenalty = Improvements.Any(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.IgnoreCMPenaltyPhysical && objImprovement.Enabled)
                ? 0
                : (PhysicalCMThresholdOffset - intPhysicalCMFilled) / intCMThreshold;

            WoundModifier = intPhysicalCMPenalty + intStunCMPenalty;
        }

        private int _intWoundModifier;

        /// <summary>
        /// Dicepool penalties the character has from wounds.
        /// </summary>
        public int WoundModifier
        {
            get => _intWoundModifier;
            set
            {
                if (_intWoundModifier != value)
                {
                    _intWoundModifier = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WoundModifier)));
                }
            }
        }

        public Version LastSavedVersion => _verSavedVersion;
        
        /// <summary>
        /// Is the character a mystic adept (MagicianEnabled && AdeptEnabled)? Used for databinding properties.
        /// </summary>
        public bool IsMysticAdept => AdeptEnabled && MagicianEnabled;

        /// <summary>
        /// Whether this character is using special Mystic Adept PP rules (true) or calculate PPs from Mystic Adept's Adept MAG (false)
        /// </summary>
        public bool UseMysticAdeptPPs => IsMysticAdept && !Options.MysAdeptSecondMAGAttribute;

        public void RefreshUseMysticAdeptPPs()
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(UseMysticAdeptPPs)));
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(MysAdeptAllowPPCareer)));
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(CanAffordCareerPP)));
                if (EssenceAtSpecialStart != decimal.MinValue && Options.SpecialKarmaCostBasedOnShownValue)
                {
                    RefreshEssenceLossImprovements();
                }
            }
        }

        /// <summary>
        /// Whether this character is a Mystic Adept uses PPs and can purchase PPs in career mode
        /// </summary>
        public bool MysAdeptAllowPPCareer => UseMysticAdeptPPs && Options.MysAdeptAllowPPCareer;

        public void RefreshMysAdeptAllowPPCareer()
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(MysAdeptAllowPPCareer)));
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(CanAffordCareerPP)));
            }
        }

        /// <summary>
        /// Could this character buy Power Points in career mode if the optional/house rule is enabled
        /// </summary>
        public bool CanAffordCareerPP => Options.MysAdeptAllowPPCareer && Karma >= 5 && MAG.TotalValue > MysticAdeptPowerPoints;

        public void RefreshCanAffordCareerPP(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CharacterAttrib.TotalValue))
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanAffordCareerPP)));
        }

        /// <summary>
        /// Blocked grades of cyber/bioware in Create mode. 
        /// </summary>
        public HashSet<string> BannedWareGrades { get; } = new HashSet<string>(){ "Betaware", "Deltaware", "Gammaware" };

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
