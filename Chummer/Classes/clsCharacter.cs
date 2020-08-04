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
using Chummer.Annotations;
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
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Uniques;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using Microsoft.ApplicationInsights;
using Newtonsoft.Json;
using NLog;
using Application = System.Windows.Forms.Application;
using Formatting = System.Xml.Formatting;

namespace Chummer
{
    /// <summary>
    /// Class that holds all of the information that makes up a complete Character.
    /// </summary>
    [DebuggerDisplay("{CharacterName} ({FileName})")]
    public sealed class Character : INotifyMultiplePropertyChanged, IHasMugshots, IHasName, IHasSource
    {
        private static readonly TelemetryClient TelemetryClient = new TelemetryClient();
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private XmlNode _oldSkillsBackup;
        private XmlNode _oldSkillGroupBackup;
        private string _strFileName = string.Empty;
        private string _strCharacterOptionsKey = GlobalOptions.DefaultCharacterOption;
        private DateTime _dateFileLastWriteTime = DateTime.MinValue;
        private bool _blnIgnoreRules;
        private int _intKarma;
        private int _intTotalKarma;
        private int _intStreetCred;
        private int _intNotoriety;
        private int _intPublicAwareness;
        private int _intBurntStreetCred;
        private decimal _decNuyen;
        private decimal _decStolenNuyen;
        private decimal _decStartingNuyen;
        private decimal _decEssenceAtSpecialStart = decimal.MinValue;
        private int _intSpecial;
        private int _intTotalSpecial;
        private int _intAttributes;
        private int _intTotalAttributes;
        private int _intFreeSpells;
        private int _intCFPLimit;
        private int _intAINormalProgramLimit;
        private int _intAIAdvancedProgramLimit;
        private int _intCachedContactPoints = int.MinValue;
        private int _intContactPointsUsed;
        private int _intCachedRedlinerBonus = int.MinValue;
        private int _intCurrentCounterspellingDice;

        // General character info.
        private string _strName = string.Empty;
        private readonly List<Image> _lstMugshots = new List<Image>(1);
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
        public static ReadOnlyCollection<string> LimbStrings { get; } = Array.AsReadOnly(s_LstLimbStrings);

        // AI Home Node

        // Active Commlink

        // If true, the Character creation has been finalized and is maintained through Karma.
        private bool _blnCreated;

        // Build Points
        private decimal _decNuyenBP;

        // Metatype Information.
        private string _strMetatype = "Human";
        private Guid   _guiMetatype = Guid.Empty;
        private string _strMetavariant = string.Empty;
        private Guid   _guiMetavariant = Guid.Empty;
        private string _strMetatypeCategory = "Metahuman";
        private string _strMovement = string.Empty;
        private string _strWalk = string.Empty;
        private string _strRun = string.Empty;
        private string _strSprint = string.Empty;
        private string _strWalkAlt = string.Empty;
        private string _strRunAlt = string.Empty;
        private string _strSprintAlt = string.Empty;
        private int    _intMetatypeBP;
        private string _strSource;
        private string _strPage;
        private int    _intInitiativeDice = 1;

        // Special Flags.

        private bool _blnAdeptEnabled;
        private bool _blnMagicianEnabled;
        private bool _blnTechnomancerEnabled;
        private bool _blnAdvancedProgramsEnabled;
        private bool _blnCyberwareDisabled;
        private bool _blnInitiationDisabled;
        private bool _blnCritterEnabled;
        private bool _blnIsCritter;
        private bool _blnPossessed;
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
        private Tradition _objTradition;

        // Condition Monitor Progress.
        private int _intPhysicalCMFilled;
        private int _intStunCMFilled;

        // Spirit Reputation
        private int _intBaseAstralReputation;
        private int _intBaseWildReputation;

        // Priority Selections.
        private string _strPriorityMetatype = "A";
        private string _strPriorityAttributes = "B";
        private string _strPrioritySpecial = "C";
        private string _strPrioritySkills = "D";
        private string _strPriorityResources = "E";
        private string _strPriorityTalent = string.Empty;
        private readonly List<string> _lstPrioritySkills = new List<string>(3);

        // Lists.
        private readonly List<string> _lstSources = new List<string>(30);
        private readonly ObservableCollection<Improvement> _lstImprovements = new ObservableCollection<Improvement>();

        private readonly ObservableCollection<MentorSpirit>
            _lstMentorSpirits = new ObservableCollection<MentorSpirit>();

        private readonly ObservableCollection<Contact> _lstContacts = new ObservableCollection<Contact>();
        private readonly ObservableCollection<Spirit> _lstSpirits = new ObservableCollection<Spirit>();
        private readonly ObservableCollection<Spell> _lstSpells = new ObservableCollection<Spell>();
        private readonly List<Focus> _lstFoci = new List<Focus>(5);
        private readonly List<StackedFocus> _lstStackedFoci = new List<StackedFocus>(5);
        private readonly CachedBindingList<Power> _lstPowers = new CachedBindingList<Power>();
        private readonly ObservableCollection<ComplexForm> _lstComplexForms = new ObservableCollection<ComplexForm>();
        private readonly ObservableCollection<AIProgram> _lstAIPrograms = new ObservableCollection<AIProgram>();
        private readonly ObservableCollection<MartialArt> _lstMartialArts = new ObservableCollection<MartialArt>();
#if LEGACY
        private List<MartialArtManeuver> _lstMartialArtManeuvers = new List<MartialArtManeuver>(10);
#endif
        private readonly ObservableCollection<LimitModifier> _lstLimitModifiers =
            new ObservableCollection<LimitModifier>();

        private readonly ObservableCollection<Armor> _lstArmor = new ObservableCollection<Armor>();

        private readonly TaggedObservableCollection<Cyberware> _lstCyberware =
            new TaggedObservableCollection<Cyberware>();

        private readonly TaggedObservableCollection<Weapon> _lstWeapons = new TaggedObservableCollection<Weapon>();
        private readonly ObservableCollection<Quality> _lstQualities = new ObservableCollection<Quality>();
        private readonly ObservableCollection<Lifestyle> _lstLifestyles = new ObservableCollection<Lifestyle>();
        private readonly ObservableCollection<Gear> _lstGear = new ObservableCollection<Gear>();
        private readonly TaggedObservableCollection<Vehicle> _lstVehicles = new TaggedObservableCollection<Vehicle>();
        private readonly ObservableCollection<Metamagic> _lstMetamagics = new ObservableCollection<Metamagic>();
        private readonly ObservableCollection<Art> _lstArts = new ObservableCollection<Art>();
        private readonly ObservableCollection<Enhancement> _lstEnhancements = new ObservableCollection<Enhancement>();

        private readonly ObservableCollection<ExpenseLogEntry> _lstExpenseLog =
            new ObservableCollection<ExpenseLogEntry>();

        private readonly ObservableCollection<CritterPower>
            _lstCritterPowers = new ObservableCollection<CritterPower>();

        private readonly ObservableCollection<InitiationGrade> _lstInitiationGrades =
            new ObservableCollection<InitiationGrade>();

        private readonly ObservableCollection<Location> _lstGearLocations = new ObservableCollection<Location>();
        private readonly ObservableCollection<Location> _lstArmorLocations = new ObservableCollection<Location>();
        private readonly TaggedObservableCollection<Location> _lstVehicleLocations = new TaggedObservableCollection<Location>();
        private readonly ObservableCollection<Location> _lstWeaponLocations = new ObservableCollection<Location>();
        private readonly ObservableCollection<string> _lstImprovementGroups = new ObservableCollection<string>();
        private readonly BindingList<CalendarWeek> _lstCalendar = new BindingList<CalendarWeek>();

        private readonly TaggedObservableCollection<Drug> _lstDrugs = new TaggedObservableCollection<Drug>();

        //private List<LifeModule> _lstLifeModules = new List<LifeModule>(10);
        private readonly List<string> _lstInternalIdsNeedingReapplyImprovements = new List<string>(1);

        // Character Version
        private string _strVersionCreated = Application.ProductVersion.FastEscapeOnceFromStart("0.0.");
        Version _verSavedVersion = new Version();

        [JsonIgnore]
        [XmlIgnore]
        [IgnoreDataMember]
        [CanBeNull]
        public EventHandler<Character> OnSaveCompleted
        {
            get;
            set;
        }

        #region Initialization, Save, Load, Print, and Reset Methods

        /// <summary>
        /// Character.
        /// </summary>
        public Character()
        {
            Options.PropertyChanged += OptionsOnPropertyChanged;
            AttributeSection = new AttributeSection(this);
            AttributeSection.Reset();
            AttributeSection.PropertyChanged += AttributeSectionOnPropertyChanged;

            SkillsSection = new SkillsSection(this);
            SkillsSection.Reset();

            _lstCyberware.CollectionChanged += CyberwareOnCollectionChanged;
            _lstArmor.CollectionChanged += ArmorOnCollectionChanged;
            _lstContacts.CollectionChanged += ContactsOnCollectionChanged;
            _lstExpenseLog.CollectionChanged += ExpenseLogOnCollectionChanged;
            _lstMentorSpirits.CollectionChanged += MentorSpiritsOnCollectionChanged;
            _lstPowers.ListChanged += PowersOnListChanged;
            _lstPowers.BeforeRemove += PowersOnBeforeRemove;
            _lstQualities.CollectionChanged += QualitiesCollectionChanged;

            _objTradition = new Tradition(this);
        }

        private XPathNavigator _xmlMetatypeNode;
        public XPathNavigator GetNode(bool blnReturnMetatypeOnly = false)
        {
            XmlDocument xmlDoc = LoadData(IsCritter ? "critters.xml" : "metatypes.xml");
            XPathNavigator xmlMetatypeNode = xmlDoc.CreateNavigator().SelectSingleNode(MetatypeGuid == Guid.Empty
                ? "/chummer/metatypes/metatype[name = \"" + Metatype + "\"]"
                : "/chummer/metatypes/metatype[id = \"" + MetatypeGuid.ToString("D", GlobalOptions.InvariantCultureInfo) + "\"]");
            if (blnReturnMetatypeOnly)
                return xmlMetatypeNode;
            _xmlMetatypeNode = xmlMetatypeNode;
            if (MetavariantGuid != Guid.Empty && !string.IsNullOrEmpty(Metavariant) && _xmlMetatypeNode != null)
            {
                XPathNavigator xmlMetavariantNode = _xmlMetatypeNode.SelectSingleNode(MetavariantGuid == Guid.Empty
                    ? "metavariants/metavariant[name = \"" + Metavariant + "\"]"
                    : "metavariants/metavariant[id = \"" + MetavariantGuid.ToString("D", GlobalOptions.InvariantCultureInfo) + "\"]");
                if (xmlMetavariantNode == null && MetavariantGuid != Guid.Empty)
                {
                    xmlMetavariantNode =
                        _xmlMetatypeNode.SelectSingleNode("metavariants/metavariant[name = \"" + Metavariant + "\"]");
                }
                if (xmlMetavariantNode != null)
                    _xmlMetatypeNode = xmlMetavariantNode;
            }

            return _xmlMetatypeNode;
        }

        public void RefreshAttributeBindings()
        {
            BOD.PropertyChanged += RefreshBODDependentProperties;
            AGI.PropertyChanged += RefreshAGIDependentProperties;
            REA.PropertyChanged += RefreshREADependentProperties;
            STR.PropertyChanged += RefreshSTRDependentProperties;
            CHA.PropertyChanged += RefreshCHADependentProperties;
            INT.PropertyChanged += RefreshINTDependentProperties;
            LOG.PropertyChanged += RefreshLOGDependentProperties;
            WIL.PropertyChanged += RefreshWILDependentProperties;
            EDG.PropertyChanged += RefreshEDGDependentProperties;
            MAG.PropertyChanged += RefreshMAGDependentProperties;
            RES.PropertyChanged += RefreshRESDependentProperties;
            DEP.PropertyChanged += RefreshDEPDependentProperties;
            ESS.PropertyChanged += RefreshESSDependentProperties;
            // This needs to be explicitly set because a MAGAdept call could redirect to MAG, and we don't want that
            AttributeSection.GetAttributeByName("MAGAdept").PropertyChanged += RefreshMAGAdeptDependentProperties;
        }
        private void OptionsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e?.PropertyName)
            {
                case nameof(CharacterOptions.UseCalculatedPublicAwareness):
                    OnPropertyChanged(nameof(CalculatedPublicAwareness));
                    break;
                case nameof(CharacterOptions.SpiritForceBasedOnTotalMAG):
                    OnPropertyChanged(nameof(MaxSpiritForce));
                    break;
                case nameof(CharacterOptions.CyberlegMovement):
                    OnPropertyChanged(nameof(Movement));
                    break;
                case nameof(CharacterOptions.AllowInitiationInCreateMode):
                    OnPropertyChanged(nameof(AddInitiationsAllowed));
                    break;
                case nameof(CharacterOptions.MysAdeptAllowPPCareer):
                    OnPropertyChanged(nameof(MysAdeptAllowPPCareer));
                    break;
                case nameof(CharacterOptions.MysAdeptSecondMAGAttribute):
                    OnMultiplePropertyChanged(nameof(UseMysticAdeptPPs), nameof(AllowAdeptWayPowerDiscount));
                    break;
                case nameof(CharacterOptions.ContactPointsExpression):
                    OnPropertyChanged(nameof(ContactPoints));
                    break;
                case nameof(CharacterOptions.SpecialKarmaCostBasedOnShownValue):
                    RefreshEssenceLossImprovements();
                    break;
                case nameof(CharacterOptions.NuyenFormat):
                    OnMultiplePropertyChanged(nameof(DisplayNuyen), nameof(DisplayCareerNuyen), nameof(DisplayStolenNuyen));
                    break;
                case nameof(CharacterOptions.EssenceFormat):
                case nameof(CharacterOptions.DontRoundEssenceInternally):
                    OnMultiplePropertyChanged(nameof(PrototypeTranshumanEssenceUsed), nameof(BiowareEssence), nameof(CyberwareEssence), nameof(EssenceHole));
                    break;
                case nameof(CharacterOptions.NuyenMaximumBP):
                case nameof(CharacterOptions.UnrestrictedNuyen):
                    OnPropertyChanged(nameof(TotalNuyenMaximumBP));
                    break;
                case nameof(CharacterOptions.KarmaMysticAdeptPowerPoint):
                    OnPropertyChanged(nameof(CanAffordCareerPP));
                    break;
                case nameof(CharacterOptions.BuildMethod):
                    OnPropertyChanged(nameof(EffectiveBuildMethod));
                    break;
                case nameof(CharacterOptions.AutomaticBackstory):
                    OnPropertyChanged(nameof(EnableAutomaticStoryButton));
                    break;
                case nameof(CharacterOptions.NuyenPerBP):
                    OnPropertyChanged(nameof(TotalStartingNuyen));
                    break;
                case nameof(CharacterOptions.LimbCount):
                    OnPropertyChanged(nameof(LimbCount));
                    break;
                case nameof(CharacterOptions.MetatypeCostsKarmaMultiplier):
                    OnPropertyChanged(nameof(DisplayMetatypeBP));
                    break;
                case nameof(CharacterOptions.RedlinerExcludes):
                    RefreshRedlinerImprovements();
                    break;
                case nameof(CharacterOptions.NoArmorEncumbrance):
                    RefreshEncumbrance();
                    break;
                case nameof(CharacterOptions.KarmaQuality):
                case nameof(CharacterOptions.QualityKarmaLimit):
                    OnMultiplePropertyChanged(nameof(PositiveQualityKarma), nameof(NegativeQualityKarma));
                    break;
                case nameof(CharacterOptions.ExceedPositiveQualitiesCostDoubled):
                    OnPropertyChanged(nameof(PositiveQualityKarma));
                    break;
                case nameof(CharacterOptions.EnemyKarmaQualityLimit):
                case nameof(CharacterOptions.ExceedNegativeQualitiesLimit):
                    OnPropertyChanged(nameof(NegativeQualityKarma));
                    break;
                case nameof(CharacterOptions.KarmaEnemy):
                    OnPropertyChanged(nameof(EnemyKarma));
                    break;
                case nameof(CharacterOptions.KarmaSpell):
                    if (FreeSpells > 0)
                    {
                        OnPropertyChanged(nameof(PositiveQualityKarma));
                    }
                    break;
            }
        }

        private void AttributeSectionOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AttributeSection.AttributeCategory))
            {
                OnMultiplePropertyChanged(nameof(CurrentWalkingRateString),
                    nameof(CurrentRunningRateString),
                    nameof(CurrentSprintingRateString));
            }
        }

        private void ContactsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Move)
            {
                _intCachedNegativeQualities = int.MinValue;
                _intCachedNegativeQualityLimitKarma = int.MinValue;
                _intCachedPositiveQualities = int.MinValue;
            }
        }
        private void PowersOnBeforeRemove(object sender, RemovingOldEventArgs e)
        {
            if(Powers[e.OldIndex].AdeptWayDiscountEnabled)
                OnPropertyChanged(nameof(AnyPowerAdeptWayDiscountEnabled));
        }

        private void PowersOnListChanged(object sender, ListChangedEventArgs e)
        {
            HashSet<string> setChangedProperties = new HashSet<string>();
            switch(e.ListChangedType)
            {
                case ListChangedType.Reset:
                    {
                        setChangedProperties.Add(nameof(PowerPointsUsed));
                        setChangedProperties.Add(nameof(AnyPowerAdeptWayDiscountEnabled));
                        setChangedProperties.Add(nameof(AllowAdeptWayPowerDiscount));
                    }
                    break;
                case ListChangedType.ItemAdded:
                    {
                        setChangedProperties.Add(nameof(PowerPointsUsed));
                        if (Powers[e.NewIndex].AdeptWayDiscountEnabled)
                        {
                            setChangedProperties.Add(nameof(AnyPowerAdeptWayDiscountEnabled));
                            setChangedProperties.Add(nameof(AllowAdeptWayPowerDiscount));
                        }
                    }
                    break;
                case ListChangedType.ItemDeleted:
                    {
                        setChangedProperties.Add(nameof(PowerPointsUsed));
                    }
                    break;
                case ListChangedType.ItemChanged:
                    {
                        if(e.PropertyDescriptor == null)
                        {
                            break;
                        }

                        if (e.PropertyDescriptor.Name == nameof(Power.AdeptWayDiscountEnabled))
                        {
                            setChangedProperties.Add(nameof(AnyPowerAdeptWayDiscountEnabled));
                            setChangedProperties.Add(nameof(AllowAdeptWayPowerDiscount));
                        }
                        else if (setChangedProperties.Add(nameof(Power.DiscountedAdeptWay)))
                        {
                            setChangedProperties.Add(nameof(AnyPowerAdeptWayDiscountEnabled));
                            setChangedProperties.Add(nameof(AllowAdeptWayPowerDiscount));
                            foreach (Power objPower in Powers)
                            {
                                objPower.OnPropertyChanged(nameof(Power.AdeptWayDiscountEnabled));
                            }
                        }
                        else if (e.PropertyDescriptor.Name == nameof(Power.PowerPoints))
                        {
                            setChangedProperties.Add(nameof(PowerPointsUsed));
                        }
                    }
                    break;
            }

            OnMultiplePropertyChanged(setChangedProperties.ToArray());
        }

        private void MentorSpiritsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(MentorSpirits));
        }

        private void QualitiesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.Action != NotifyCollectionChangedAction.Move)
            {
                foreach(Power objPower in Powers)
                {
                    objPower.OnPropertyChanged(nameof(Power.AdeptWayDiscountEnabled));
                }
            }
            OnPropertyChanged(nameof(Qualities));
        }

        private void ExpenseLogOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            HashSet<string> setPropertiesToRefresh = new HashSet<string>();
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach(ExpenseLogEntry objNewItem in e.NewItems)
                    {
                        if((objNewItem.Amount > 0 || objNewItem.ForceCareerVisible) && !objNewItem.Refund)
                        {
                            setPropertiesToRefresh.Add(objNewItem.Type == ExpenseType.Nuyen
                                ? nameof(CareerNuyen)
                                : nameof(CareerKarma));
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach(ExpenseLogEntry objOldItem in e.OldItems)
                    {
                        if ((objOldItem.Amount > 0 || objOldItem.ForceCareerVisible) && !objOldItem.Refund)
                        {
                            setPropertiesToRefresh.Add(objOldItem.Type == ExpenseType.Nuyen
                                ? nameof(CareerNuyen)
                                : nameof(CareerKarma));
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach(ExpenseLogEntry objOldItem in e.OldItems)
                    {
                        if ((objOldItem.Amount > 0 || objOldItem.ForceCareerVisible) && !objOldItem.Refund)
                        {
                            setPropertiesToRefresh.Add(objOldItem.Type == ExpenseType.Nuyen
                                ? nameof(CareerNuyen)
                                : nameof(CareerKarma));
                        }
                    }

                    foreach(ExpenseLogEntry objNewItem in e.NewItems)
                    {
                        if ((objNewItem.Amount > 0 || objNewItem.ForceCareerVisible) && !objNewItem.Refund)
                        {
                            setPropertiesToRefresh.Add(objNewItem.Type == ExpenseType.Nuyen
                                ? nameof(CareerNuyen)
                                : nameof(CareerKarma));
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Reset:
                    setPropertiesToRefresh.Add(nameof(CareerNuyen));
                    setPropertiesToRefresh.Add(nameof(CareerKarma));
                    break;
            }

            if(setPropertiesToRefresh.Count > 0)
                OnMultiplePropertyChanged(setPropertiesToRefresh.ToArray());
        }

        private void ArmorOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            bool blnDoEncumbranceRefresh = false;
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach(Armor objNewItem in e.NewItems)
                    {
                        if(objNewItem.Equipped)
                        {
                            blnDoEncumbranceRefresh = true;
                            break;
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach(Armor objOldItem in e.OldItems)
                    {
                        if(objOldItem.Equipped)
                        {
                            blnDoEncumbranceRefresh = true;
                            break;
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach(Armor objOldItem in e.OldItems)
                    {
                        if(objOldItem.Equipped)
                        {
                            blnDoEncumbranceRefresh = true;
                            break;
                        }
                    }

                    if(!blnDoEncumbranceRefresh)
                    {
                        foreach(Armor objNewItem in e.NewItems)
                        {
                            if(objNewItem.Equipped)
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

            if(blnDoEncumbranceRefresh)
            {
                OnPropertyChanged(nameof(ArmorRating));
                RefreshEncumbrance();
            }
        }

        private void CyberwareOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            bool blnDoCyberlimbAttributesRefresh = false;
            HashSet<string> setEssenceImprovementsToRefresh = new HashSet<string>();
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    setEssenceImprovementsToRefresh.Add(nameof(RedlinerBonus));
                    foreach(Cyberware objNewItem in e.NewItems)
                    {
                        setEssenceImprovementsToRefresh.Add(objNewItem.EssencePropertyName);
                        if(!blnDoCyberlimbAttributesRefresh && !Options.DontUseCyberlimbCalculation &&
                            objNewItem.Category == "Cyberlimb" && objNewItem.Parent == null &&
                            objNewItem.ParentVehicle == null &&
                            !string.IsNullOrWhiteSpace(objNewItem.LimbSlot) &&
                            !Options.ExcludeLimbSlot.Contains(objNewItem.LimbSlot))
                        {
                            blnDoCyberlimbAttributesRefresh = true;
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Remove:
                    setEssenceImprovementsToRefresh.Add(nameof(RedlinerBonus));
                    foreach(Cyberware objOldItem in e.OldItems)
                    {
                        setEssenceImprovementsToRefresh.Add(objOldItem.EssencePropertyName);
                        if(!blnDoCyberlimbAttributesRefresh && !Options.DontUseCyberlimbCalculation &&
                            objOldItem.Category == "Cyberlimb" && objOldItem.Parent == null &&
                            objOldItem.ParentVehicle == null &&
                            !string.IsNullOrWhiteSpace(objOldItem.LimbSlot) &&
                            !Options.ExcludeLimbSlot.Contains(objOldItem.LimbSlot))
                        {
                            blnDoCyberlimbAttributesRefresh = true;
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Replace:
                    setEssenceImprovementsToRefresh.Add(nameof(RedlinerBonus));
                    if(!Options.DontUseCyberlimbCalculation)
                    {
                        foreach(Cyberware objOldItem in e.OldItems)
                        {
                            setEssenceImprovementsToRefresh.Add(objOldItem.EssencePropertyName);
                            if(!blnDoCyberlimbAttributesRefresh && !Options.DontUseCyberlimbCalculation &&
                                objOldItem.Category == "Cyberlimb" && objOldItem.Parent == null &&
                                objOldItem.ParentVehicle == null &&
                                !string.IsNullOrWhiteSpace(objOldItem.LimbSlot) &&
                                !Options.ExcludeLimbSlot.Contains(objOldItem.LimbSlot))
                            {
                                blnDoCyberlimbAttributesRefresh = true;
                            }
                        }

                        foreach(Cyberware objNewItem in e.NewItems)
                        {
                            setEssenceImprovementsToRefresh.Add(objNewItem.EssencePropertyName);
                            if(!blnDoCyberlimbAttributesRefresh && !Options.DontUseCyberlimbCalculation &&
                                objNewItem.Category == "Cyberlimb" && objNewItem.Parent == null &&
                                objNewItem.ParentVehicle == null &&
                                !string.IsNullOrWhiteSpace(objNewItem.LimbSlot) &&
                                !Options.ExcludeLimbSlot.Contains(objNewItem.LimbSlot))
                            {
                                blnDoCyberlimbAttributesRefresh = true;
                            }
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Reset:
                    blnDoCyberlimbAttributesRefresh = !Options.DontUseCyberlimbCalculation;
                    setEssenceImprovementsToRefresh.Add(nameof(RedlinerBonus));
                    setEssenceImprovementsToRefresh.Add(nameof(PrototypeTranshumanEssenceUsed));
                    setEssenceImprovementsToRefresh.Add(nameof(BiowareEssence));
                    setEssenceImprovementsToRefresh.Add(nameof(CyberwareEssence));
                    setEssenceImprovementsToRefresh.Add(nameof(EssenceHole));
                    break;
            }

            if(blnDoCyberlimbAttributesRefresh)
            {
                GetAttribute("AGI")?.OnPropertyChanged(nameof(CharacterAttrib.TotalValue));
                GetAttribute("STR")?.OnPropertyChanged(nameof(CharacterAttrib.TotalValue));
            }

            if(setEssenceImprovementsToRefresh.Count > 0)
            {
                OnMultiplePropertyChanged(setEssenceImprovementsToRefresh.ToArray());
            }
        }

        [HubTag]
        public AttributeSection AttributeSection { get; }

        public bool IsSaving { get; set; }
        #region Create, Save, Load and Print Methods

        /// <summary>
        /// 
        /// </summary>
        public void Create(string strSelectedMetatypeCategory, string strMetatypeId, string strMetavariantId, XmlNode objXmlMetatype, int intForce, XmlNode xmlQualityDocumentQualitiesNode, XmlNode xmlCritterPowerDocumentPowersNode, XmlNode xmlSkillsDocumentKnowledgeSkillsNode, string strSelectedPossessionMethod = "", bool blnBloodSpirit = false)
        {
            if (objXmlMetatype == null)
                throw new ArgumentNullException(nameof(objXmlMetatype));
            // Remove any Improvements the character received from their Metatype.
            ImprovementManager.RemoveImprovements(this,
                Improvements.Where(objImprovement => objImprovement.ImproveSource == Improvement.ImprovementSource.Metatype
                                                     || objImprovement.ImproveSource == Improvement.ImprovementSource.Metavariant).ToList());

            // Remove any Qualities the character received from their Metatype, then remove the Quality.
            foreach (Quality objQuality in Qualities.Where(objQuality => objQuality.OriginSource == QualitySource.Metatype || objQuality.OriginSource == QualitySource.MetatypeRemovable || objQuality.OriginSource == QualitySource.MetatypeRemovedAtChargen).ToList())
            {
                ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.Quality, objQuality.InternalId);
                Qualities.Remove(objQuality);
            }

            // If this is a Shapeshifter, a Metavariant must be selected. Default to Human if None is selected.
            if (strSelectedMetatypeCategory == "Shapeshifter" && strMetavariantId == Guid.Empty.ToString())
                strMetavariantId = objXmlMetatype.SelectSingleNode("metavariants/metavariant[name = \"Human\"]/id")?.InnerText ?? string.Empty;
            XmlNode objXmlMetavariant = objXmlMetatype.SelectSingleNode("metavariants/metavariant[id = \"" + strMetavariantId + "\"]");

            // Set Metatype information.
            int intMinModifier = 0;
            int intMaxModifier = 0;
            XmlNode charNode = strSelectedMetatypeCategory == "Shapeshifter" || strMetavariantId == Guid.Empty.ToString() ? objXmlMetatype : objXmlMetavariant ?? objXmlMetatype;
            AttributeSection.Create(charNode, intForce, intMinModifier, intMaxModifier);
            MetatypeGuid = new Guid(strMetatypeId);
            Metatype = objXmlMetatype["name"]?.InnerText ?? "Human";
            MetatypeCategory = strSelectedMetatypeCategory;
            MetavariantGuid = new Guid(strMetavariantId);
            Metavariant = MetavariantGuid != Guid.Empty ? objXmlMetavariant?["name"]?.InnerText ?? "None" : "None";
            // We only reverted to the base metatype to get the attributes.
            if (strSelectedMetatypeCategory == "Shapeshifter")
            {
                charNode = objXmlMetavariant ?? objXmlMetatype;
            }
            Source = charNode["source"]?.InnerText ?? "SR5";
            Page = charNode["page"]?.InnerText ?? "0";
            charNode.TryGetInt32FieldQuickly("karma", ref _intMetatypeBP);
            charNode.TryGetInt32FieldQuickly("initiativedice", ref _intInitiativeDice);

            string strMovement = objXmlMetatype["movement"]?.InnerText;
            if (!string.IsNullOrEmpty(strMovement))
                Movement = strMovement;

            // Determine if the Metatype has any bonuses.
            XmlNode xmlBonusNode = charNode.SelectSingleNode("bonus");
            if (xmlBonusNode != null)
                ImprovementManager.CreateImprovements(this, Improvement.ImprovementSource.Metatype, strMetatypeId, xmlBonusNode, 1, strMetatypeId);

            List<Weapon> lstWeapons = new List<Weapon>(1);
            // Create the Qualities that come with the Metatype.
            if (xmlQualityDocumentQualitiesNode != null)
            {
                using (XmlNodeList xmlQualityList = charNode.SelectNodes("qualities/*/quality"))
                {
                    if (xmlQualityList != null)
                    {
                        foreach (XmlNode objXmlQualityItem in xmlQualityList)
                        {
                            XmlNode objXmlQuality = xmlQualityDocumentQualitiesNode.SelectSingleNode("quality[name = \"" + objXmlQualityItem.InnerText + "\"]");
                            Quality objQuality = new Quality(this);
                            string strForceValue = objXmlQualityItem.Attributes["select"]?.InnerText ?? string.Empty;
                            QualitySource objSource = objXmlQualityItem.Attributes["removable"]?.InnerText == bool.TrueString ? QualitySource.MetatypeRemovable : QualitySource.Metatype;
                            objQuality.Create(objXmlQuality, objSource, lstWeapons, strForceValue);
                            objQuality.ContributeToLimit = false;
                            Qualities.Add(objQuality);
                        }
                    }
                }
            }

            //Load any critter powers the character has.
            if (xmlCritterPowerDocumentPowersNode != null)
            {
                foreach (XmlNode objXmlPower in charNode.SelectNodes("powers/power"))
                {
                    XmlNode objXmlCritterPower = xmlCritterPowerDocumentPowersNode.SelectSingleNode("power[name = \"" + objXmlPower.InnerText + "\"]");
                    CritterPower objPower = new CritterPower(this);
                    string strForcedValue = objXmlPower.Attributes["select"]?.InnerText ?? string.Empty;
                    int intRating = Convert.ToInt32(objXmlPower.Attributes["rating"]?.InnerText, GlobalOptions.InvariantCultureInfo);

                    objPower.Create(objXmlCritterPower, intRating, strForcedValue);
                    objPower.CountTowardsLimit = false;
                    CritterPowers.Add(objPower);
                    ImprovementManager.CreateImprovement(this, objPower.InternalId, Improvement.ImprovementSource.Metatype, string.Empty, Improvement.ImprovementType.CritterPower, string.Empty);
                    ImprovementManager.Commit(this);
                }
            }

            //Load any natural weapons the character has.
            foreach (XmlNode objXmlNaturalWeapon in charNode.SelectNodes("nautralweapons/naturalweapon"))
            {
                Weapon objWeapon = new Weapon(this)
                {
                    Name = objXmlNaturalWeapon["name"].InnerText,
                    Category = LanguageManager.GetString("Tab_Critter"),
                    WeaponType = "Melee",
                    Reach = Convert.ToInt32(objXmlNaturalWeapon["reach"]?.InnerText, GlobalOptions.InvariantCultureInfo),
                    Damage = objXmlNaturalWeapon["damage"].InnerText,
                    AP = objXmlNaturalWeapon["ap"]?.InnerText ?? "0",
                    Mode = "0",
                    RC = "0",
                    Concealability = 0,
                    Avail = "0",
                    Cost = "0",
                    UseSkill = objXmlNaturalWeapon["useskill"]?.InnerText,
                    Source = objXmlNaturalWeapon["source"].InnerText,
                    Page = objXmlNaturalWeapon["page"].InnerText
                };

                Weapons.Add(objWeapon);
            }
            //Set the Active Skill Ratings for the Critter.
            foreach (XmlNode xmlSkill in charNode.SelectNodes("skills/skill"))
            {
                string strRating = xmlSkill.Attributes?["rating"]?.InnerText;

                if (!string.IsNullOrEmpty(strRating))
                {
                    ImprovementManager.CreateImprovement(this, xmlSkill.InnerText, Improvement.ImprovementSource.Metatype, string.Empty, Improvement.ImprovementType.SkillLevel, string.Empty, strRating == "F" ? intForce : Convert.ToInt32(strRating, GlobalOptions.InvariantCultureInfo));
                    ImprovementManager.Commit(this);
                }
                string strSkill = xmlSkill.InnerText;
                Skill objSkill = SkillsSection.GetActiveSkill(strSkill);
                if (objSkill == null) continue;
                string strSpec = xmlSkill.Attributes?["spec"]?.InnerText ?? string.Empty;
                ImprovementManager.CreateImprovement(this, strSkill, Improvement.ImprovementSource.Metatype, string.Empty, Improvement.ImprovementType.SkillSpecialization, strSpec);
                SkillSpecialization spec = new SkillSpecialization(this, strSpec, true);
                objSkill.Specializations.Add(spec);
            }
            //Set the Skill Group Ratings for the Critter.
            foreach (XmlNode xmlSkillGroup in charNode.SelectNodes("skills/group"))
            {
                string strRating = xmlSkillGroup.Attributes?["rating"]?.InnerText;
                if (!string.IsNullOrEmpty(strRating))
                {
                    ImprovementManager.CreateImprovement(this, xmlSkillGroup.InnerText, Improvement.ImprovementSource.Metatype, string.Empty, Improvement.ImprovementType.SkillGroupLevel, string.Empty, strRating == "F" ? intForce : Convert.ToInt32(strRating, GlobalOptions.InvariantCultureInfo));
                    ImprovementManager.Commit(this);
                }
            }

            //Set the Knowledge Skill Ratings for the Critter.
            if (xmlSkillsDocumentKnowledgeSkillsNode != null)
            {
                foreach (XmlNode xmlSkill in charNode.SelectNodes("skills/knowledge"))
                {
                    string strRating = xmlSkill.Attributes?["rating"]?.InnerText;
                    if (!string.IsNullOrEmpty(strRating))
                    {
                        if (SkillsSection.KnowledgeSkills.All(x => x.Name != xmlSkill.InnerText))
                        {
                            XmlNode objXmlSkillNode = xmlSkillsDocumentKnowledgeSkillsNode.SelectSingleNode("skill[name = \"" + xmlSkill.InnerText + "\"]");
                            if (objXmlSkillNode != null)
                            {
                                KnowledgeSkill objSkill = Skill.FromData(objXmlSkillNode, this) as KnowledgeSkill;
                                SkillsSection.KnowledgeSkills.Add(objSkill);
                            }
                            else
                            {
                                KnowledgeSkill objSkill = new KnowledgeSkill(this, xmlSkill.InnerText, true)
                                {
                                    Type = xmlSkill.Attributes?["category"]?.InnerText
                                };
                                SkillsSection.KnowledgeSkills.Add(objSkill);
                            }
                        }

                        ImprovementManager.CreateImprovement(this, xmlSkill.InnerText, Improvement.ImprovementSource.Metatype, string.Empty, Improvement.ImprovementType.SkillLevel, string.Empty,
                            strRating == "F" ? intForce : Convert.ToInt32(strRating, GlobalOptions.InvariantCultureInfo));
                        ImprovementManager.Commit(this);
                    }
                }
            }

            // Add any Complex Forms the Critter comes with (typically Sprites)
            XmlDocument xmlComplexFormDocument = LoadData("complexforms.xml");
            foreach (XmlNode xmlComplexForm in charNode.SelectNodes("complexforms/complexform"))
            {
                XmlNode xmlComplexFormData = xmlComplexFormDocument.SelectSingleNode("/chummer/complexforms/complexform[name = \"" + xmlComplexForm.InnerText + "\"]");
                if (xmlComplexFormData == null)
                    continue;

                ComplexForm objComplexform = new ComplexForm(this);
                objComplexform.Create(xmlComplexFormData);
                if (objComplexform.InternalId.IsEmptyGuid())
                    continue;
                objComplexform.Grade = -1;

                ComplexForms.Add(objComplexform);

                ImprovementManager.CreateImprovement(this, objComplexform.InternalId, Improvement.ImprovementSource.Metatype, string.Empty, Improvement.ImprovementType.ComplexForm, string.Empty);
                ImprovementManager.Commit(this);
            }

            //Load any cyberware the character has.
            XmlDocument xmlCyberwareDocument = LoadData("cyberware.xml");
            foreach (XmlNode node in charNode.SelectNodes("cyberwares/cyberware"))
            {
                XmlNode objXmlCyberwareNode = xmlCyberwareDocument.SelectSingleNode("chummer/cyberwares/cyberware[name = \"" + node.InnerText + "\"]");
                var objWare = new Cyberware(this);
                string strForcedValue = node.Attributes["select"]?.InnerText ?? string.Empty;
                int intRating = Convert.ToInt32(node.Attributes["rating"]?.InnerText, GlobalOptions.InvariantCultureInfo);

                objWare.Create(objXmlCyberwareNode,
                    GetGradeList(Improvement.ImprovementSource.Cyberware, true)
                        .FirstOrDefault(x => x.Name == "None"), Improvement.ImprovementSource.Metatype, intRating,
                    Weapons, Vehicles, true, true, strForcedValue);
                Cyberware.Add(objWare);
                ImprovementManager.CreateImprovement(this, objWare.InternalId, Improvement.ImprovementSource.Metatype, string.Empty, Improvement.ImprovementType.FreeWare, string.Empty);
                ImprovementManager.Commit(this);
            }

            //Load any bioware the character has.
            XmlDocument xmlBiowareDocument = LoadData("bioware.xml");
            foreach (XmlNode node in charNode.SelectNodes("biowares/bioware"))
            {
                XmlNode objXmlCyberwareNode = xmlBiowareDocument.SelectSingleNode("chummer/biowares/bioware[name = \"" + node.InnerText + "\"]");
                var objWare = new Cyberware(this);
                string strForcedValue = node.Attributes["select"]?.InnerText ?? string.Empty;
                int intRating = Convert.ToInt32(node.Attributes["rating"]?.InnerText, GlobalOptions.InvariantCultureInfo);

                objWare.Create(objXmlCyberwareNode,
                    GetGradeList(Improvement.ImprovementSource.Cyberware, true)
                        .FirstOrDefault(x => x.Name == "None"), Improvement.ImprovementSource.Metatype, intRating,
                    Weapons, Vehicles, true, true, strForcedValue);
                Cyberware.Add(objWare);
                ImprovementManager.CreateImprovement(this, objWare.InternalId, Improvement.ImprovementSource.Metatype, string.Empty, Improvement.ImprovementType.FreeWare, string.Empty);
                ImprovementManager.Commit(this);
            }

            // Add any Advanced Programs the Critter comes with (typically A.I.s)
            XmlDocument xmlAIProgramDocument = LoadData("programs.xml");
            foreach (XmlNode xmlAIProgram in charNode.SelectNodes("programs/program"))
            {
                XmlNode xmlAIProgramData = xmlAIProgramDocument.SelectSingleNode("/chummer/programs/program[name = \"" + xmlAIProgram.InnerText + "\"]");
                if (xmlAIProgramData == null)
                    continue;

                // Check for SelectText.
                string strExtra = xmlAIProgram.Attributes?["select"]?.InnerText ?? string.Empty;
                XmlNode xmlSelectText = xmlAIProgramData.SelectSingleNode("bonus/selecttext");
                if (xmlSelectText != null && !string.IsNullOrWhiteSpace(strExtra))
                {
                    using (frmSelectText frmPickText = new frmSelectText
                    {
                        Description = string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("String_Improvement_SelectText"),
                            xmlAIProgramData["translate"]?.InnerText ?? xmlAIProgramData["name"].InnerText)
                    })
                    {
                        frmPickText.ShowDialog(Program.MainForm);
                        // Make sure the dialogue window was not canceled.
                        if (frmPickText.DialogResult == DialogResult.Cancel)
                            continue;
                        strExtra = frmPickText.SelectedValue;
                    }
                }

                AIProgram objAIProgram = new AIProgram(this);
                objAIProgram.Create(xmlAIProgram, strExtra, false);
                if (objAIProgram.InternalId.IsEmptyGuid())
                    continue;

                AIPrograms.Add(objAIProgram);

                ImprovementManager.CreateImprovement(this, objAIProgram.InternalId, Improvement.ImprovementSource.Metatype, string.Empty, Improvement.ImprovementType.AIProgram, string.Empty);
                ImprovementManager.Commit(this);
            }

            // Add any Gear the Critter comes with (typically Programs for A.I.s)
            XmlDocument xmlGearDocument = LoadData("gear.xml");
            foreach (XmlNode xmlGear in charNode.SelectNodes("gears/gear"))
            {
                XmlNode xmlGearData = xmlGearDocument.SelectSingleNode("/chummer/gears/gear[name = " + xmlGear["name"].InnerText.CleanXPath() + " and category = " + xmlGear["category"].InnerText.CleanXPath() + "]");
                if (xmlGearData == null)
                    continue;

                int intRating = 1;
                if (xmlGear["rating"] != null)
                    intRating = Convert.ToInt32(xmlGear["rating"].InnerText, GlobalOptions.InvariantCultureInfo);
                decimal decQty = 1.0m;
                if (xmlGear["quantity"] != null)
                    decQty = Convert.ToDecimal(xmlGear["quantity"].InnerText, GlobalOptions.InvariantCultureInfo);
                string strForceValue = xmlGear.Attributes?["select"]?.InnerText ?? string.Empty;

                Gear objGear = new Gear(this);
                objGear.Create(xmlGearData, intRating, lstWeapons, strForceValue);

                if (objGear.InternalId.IsEmptyGuid())
                    continue;

                objGear.Quantity = decQty;

                // If a Commlink has just been added, see if the character already has one. If not, make it the active Commlink.
                if (ActiveCommlink == null && objGear.IsCommlink)
                {
                    objGear.SetActiveCommlink(this, true);
                }

                objGear.Cost = "0";
                // Create any Weapons that came with this Gear.
                foreach (Weapon objWeapon in lstWeapons)
                    Weapons.Add(objWeapon);

                objGear.ParentID = Guid.NewGuid().ToString();

                Gear.Add(objGear);

                ImprovementManager.CreateImprovement(this, objGear.InternalId, Improvement.ImprovementSource.Metatype, string.Empty, Improvement.ImprovementType.Gear, string.Empty);
                ImprovementManager.Commit(this);
            }

            // Add any created Weapons to the character.
            foreach (Weapon objWeapon in lstWeapons)
                Weapons.Add(objWeapon);

            // Sprites can never have Physical Attributes
            if (DEPEnabled || strSelectedMetatypeCategory?.EndsWith("Sprite", StringComparison.Ordinal) == true || strSelectedMetatypeCategory?.EndsWith("Sprites", StringComparison.Ordinal) == true)
            {
                BOD.AssignLimits("0", "0", "0");
                AGI.AssignLimits("0", "0", "0");
                REA.AssignLimits("0", "0", "0");
                STR.AssignLimits("0", "0", "0");
                MAG.AssignLimits("0", "0", "0");
                MAGAdept.AssignLimits("0", "0", "0");
            }


            if (strSelectedMetatypeCategory == "Spirits")
            {
                XmlNode xmlOptionalPowersNode = charNode["optionalpowers"];
                if (xmlOptionalPowersNode != null)
                {
                    //For every 3 full points of Force a spirit has, it may gain one Optional Power.
                    for (int i = intForce - 3; i >= 0; i -= 3)
                    {
                        XmlDocument objDummyDocument = new XmlDocument
                        {
                            XmlResolver = null
                        };
                        XmlNode bonusNode = objDummyDocument.CreateNode(XmlNodeType.Element, "bonus", null);
                        objDummyDocument.AppendChild(bonusNode);
                        XmlNode powerNode = objDummyDocument.ImportNode(xmlOptionalPowersNode.CloneNode(true), true);
                        objDummyDocument.ImportNode(powerNode, true);
                        bonusNode.AppendChild(powerNode);
                        ImprovementManager.CreateImprovements(this, Improvement.ImprovementSource.Metatype, strMetatypeId, bonusNode, 1, strMetatypeId);
                    }
                }
                //If this is a Blood Spirit, add their free Critter Powers.
                if (blnBloodSpirit && xmlCritterPowerDocumentPowersNode != null)
                {
                    XmlNode objXmlCritterPower;
                    CritterPower objPower;

                    //Energy Drain.
                    if (CritterPowers.All(objFindPower => objFindPower.Name != "Energy Drain"))
                    {
                        objXmlCritterPower = xmlCritterPowerDocumentPowersNode.SelectSingleNode("power[name = \"Energy Drain\"]");
                        objPower = new CritterPower(this);
                        objPower.Create(objXmlCritterPower, 0, string.Empty);
                        objPower.CountTowardsLimit = false;
                        CritterPowers.Add(objPower);
                        ImprovementManager.CreateImprovement(this, objPower.InternalId, Improvement.ImprovementSource.Metatype, string.Empty, Improvement.ImprovementType.CritterPower, string.Empty);
                        ImprovementManager.Commit(this);
                    }

                    // Fear.
                    if (CritterPowers.All(objFindPower => objFindPower.Name != "Fear"))
                    {
                        objXmlCritterPower = xmlCritterPowerDocumentPowersNode.SelectSingleNode("power[name = \"Fear\"]");
                        objPower = new CritterPower(this);
                        objPower.Create(objXmlCritterPower, 0, string.Empty);
                        objPower.CountTowardsLimit = false;
                        CritterPowers.Add(objPower);
                        ImprovementManager.CreateImprovement(this, objPower.InternalId, Improvement.ImprovementSource.Metatype, string.Empty, Improvement.ImprovementType.CritterPower, string.Empty);
                        ImprovementManager.Commit(this);
                    }

                    // Natural Weapon.
                    objXmlCritterPower = xmlCritterPowerDocumentPowersNode.SelectSingleNode("power[name = \"Natural Weapon\"]");
                    objPower = new CritterPower(this);
                    objPower.Create(objXmlCritterPower, 0, "DV " + intForce.ToString(GlobalOptions.InvariantCultureInfo) + "P, AP 0");
                    objPower.CountTowardsLimit = false;
                    CritterPowers.Add(objPower);
                    ImprovementManager.CreateImprovement(this, objPower.InternalId, Improvement.ImprovementSource.Metatype, string.Empty, Improvement.ImprovementType.CritterPower, string.Empty);
                    ImprovementManager.Commit(this);

                    // Evanescence.
                    if (CritterPowers.All(objFindPower => objFindPower.Name != "Evanescence"))
                    {
                        objXmlCritterPower = xmlCritterPowerDocumentPowersNode.SelectSingleNode("power[name = \"Evanescence\"]");
                        objPower = new CritterPower(this);
                        objPower.Create(objXmlCritterPower, 0, string.Empty);
                        objPower.CountTowardsLimit = false;
                        CritterPowers.Add(objPower);
                        ImprovementManager.CreateImprovement(this, objPower.InternalId, Improvement.ImprovementSource.Metatype, string.Empty, Improvement.ImprovementType.CritterPower, string.Empty);
                        ImprovementManager.Commit(this);
                    }
                }

                // Remove the Critter's Materialization Power if they have it. Add the Possession or Inhabitation Power if the Possession-based Tradition checkbox is checked.
                if (!string.IsNullOrEmpty(strSelectedPossessionMethod) && xmlCritterPowerDocumentPowersNode != null)
                {
                    CritterPower objMaterializationPower = CritterPowers.FirstOrDefault(x => x.Name == "Materialization");
                    if (objMaterializationPower != null)
                        CritterPowers.Remove(objMaterializationPower);

                    if (CritterPowers.All(x => x.Name != strSelectedPossessionMethod))
                    {
                        // Add the selected Power.
                        XmlNode objXmlCritterPower = xmlCritterPowerDocumentPowersNode.SelectSingleNode("power[name = \"" + strSelectedPossessionMethod + "\"]");
                        if (objXmlCritterPower != null)
                        {
                            CritterPower objPower = new CritterPower(this);
                            objPower.Create(objXmlCritterPower, 0, string.Empty);
                            objPower.CountTowardsLimit = false;
                            CritterPowers.Add(objPower);

                            ImprovementManager.CreateImprovement(this, objPower.InternalId, Improvement.ImprovementSource.Metatype, string.Empty, Improvement.ImprovementType.CritterPower, string.Empty);
                            ImprovementManager.Commit(this);
                        }
                    }
                }
                else if (CritterPowers.All(x => x.Name != "Materialization") && xmlCritterPowerDocumentPowersNode != null)
                {
                    // Add the Materialization Power.
                    XmlNode objXmlCritterPower = xmlCritterPowerDocumentPowersNode.SelectSingleNode("power[name = \"Materialization\"]");
                    if (objXmlCritterPower != null)
                    {
                        CritterPower objPower = new CritterPower(this);
                        objPower.Create(objXmlCritterPower, 0, string.Empty);
                        objPower.CountTowardsLimit = false;
                        CritterPowers.Add(objPower);

                        ImprovementManager.CreateImprovement(this, objPower.InternalId, Improvement.ImprovementSource.Metatype, string.Empty, Improvement.ImprovementType.CritterPower, string.Empty);
                        ImprovementManager.Commit(this);
                    }
                }
            }
        }

        /// <summary>
        /// Save the Character to an XML file. Returns true if successful.
        /// </summary>
        public bool Save(string strFileName = "", bool addToMRU = true, bool callOnSaveCallBack = true)
        {
            if(IsSaving)
                return false;
            if(string.IsNullOrWhiteSpace(strFileName))
            {
                strFileName = _strFileName;
                if(string.IsNullOrWhiteSpace(strFileName))
                {
                    return false;
                }
            }

            bool blnErrorFree = true;
            IsSaving = true;
            using (MemoryStream objStream = new MemoryStream())
            {
                using (XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.UTF8)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 1,
                    IndentChar = '\t'
                })
                {
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
                    objWriter.WriteElementString("settings", _strCharacterOptionsKey);
                    // <buildmethod />
                    objWriter.WriteElementString("buildmethod", Options.BuildMethod.ToString());

                    // <metatype />
                    objWriter.WriteElementString("metatype", _strMetatype);
                    // <metatypeid />
                    objWriter.WriteElementString("metatypeid", _guiMetatype.ToString("D", GlobalOptions.InvariantCultureInfo));
                    // <metatypebp />
                    objWriter.WriteElementString("metatypebp", _intMetatypeBP.ToString(GlobalOptions.InvariantCultureInfo));
                    // <metavariant />
                    objWriter.WriteElementString("metavariant", _strMetavariant);
                    // <metavariantid />
                    objWriter.WriteElementString("metavariantid", _guiMetavariant.ToString("D", GlobalOptions.InvariantCultureInfo));
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
                    // <initiativedice />
                    objWriter.WriteElementString("initiativedice", _intInitiativeDice.ToString(GlobalOptions.InvariantCultureInfo));

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
                    objWriter.WriteElementString("essenceatspecialstart",
                        _decEssenceAtSpecialStart.ToString(GlobalOptions.InvariantCultureInfo));

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
                    // <primaryarm />
                    objWriter.WriteElementString("primaryarm", _strPrimaryArm);

                    // <ignorerules />
                    if (_blnIgnoreRules)
                        objWriter.WriteElementString("ignorerules", _blnIgnoreRules.ToString(GlobalOptions.InvariantCultureInfo));
                    // <iscritter />
                    if (_blnIsCritter)
                        objWriter.WriteElementString("iscritter", _blnIsCritter.ToString(GlobalOptions.InvariantCultureInfo));
                    if (_blnPossessed)
                        objWriter.WriteElementString("possessed", _blnPossessed.ToString(GlobalOptions.InvariantCultureInfo));
                    // <karma />
                    objWriter.WriteElementString("karma", _intKarma.ToString(GlobalOptions.InvariantCultureInfo));
                    // <special />
                    objWriter.WriteElementString("special", _intSpecial.ToString(GlobalOptions.InvariantCultureInfo));
                    // <totalspecial />
                    objWriter.WriteElementString("totalspecial", _intTotalSpecial.ToString(GlobalOptions.InvariantCultureInfo));
                    // <totalattributes />
                    objWriter.WriteElementString("totalattributes", _intTotalAttributes.ToString(GlobalOptions.InvariantCultureInfo));
                    // <contactpoints />
                    objWriter.WriteElementString("contactpoints", _intCachedContactPoints.ToString(GlobalOptions.InvariantCultureInfo));
                    // <contactpoints />
                    objWriter.WriteElementString("contactpointsused", _intContactPointsUsed.ToString(GlobalOptions.InvariantCultureInfo));
                    // <spelllimit />
                    objWriter.WriteElementString("spelllimit", _intFreeSpells.ToString(GlobalOptions.InvariantCultureInfo));
                    // <cfplimit />
                    objWriter.WriteElementString("cfplimit", _intCFPLimit.ToString(GlobalOptions.InvariantCultureInfo));
                    // <totalaiprogramlimit />
                    objWriter.WriteElementString("ainormalprogramlimit", _intAINormalProgramLimit.ToString(GlobalOptions.InvariantCultureInfo));
                    // <aiadvancedprogramlimit />
                    objWriter.WriteElementString("aiadvancedprogramlimit", _intAIAdvancedProgramLimit.ToString(GlobalOptions.InvariantCultureInfo));
                    // <currentcounterspellingdice />
                    objWriter.WriteElementString("currentcounterspellingdice", _intCurrentCounterspellingDice.ToString(GlobalOptions.InvariantCultureInfo));
                    // <streetcred />
                    objWriter.WriteElementString("streetcred", _intStreetCred.ToString(GlobalOptions.InvariantCultureInfo));
                    // <notoriety />
                    objWriter.WriteElementString("notoriety", _intNotoriety.ToString(GlobalOptions.InvariantCultureInfo));
                    // <publicaware />
                    objWriter.WriteElementString("publicawareness", _intPublicAwareness.ToString(GlobalOptions.InvariantCultureInfo));
                    // <burntstreetcred />
                    objWriter.WriteElementString("burntstreetcred", _intBurntStreetCred.ToString(GlobalOptions.InvariantCultureInfo));
                    // <baseastralreputation />
                    objWriter.WriteElementString("baseastralreputation", _intBaseAstralReputation.ToString(GlobalOptions.InvariantCultureInfo));
                    // <basewildreputation />
                    objWriter.WriteElementString("basewildreputation", _intBaseWildReputation.ToString(GlobalOptions.InvariantCultureInfo));
                    // <created />
                    objWriter.WriteElementString("created", _blnCreated.ToString(GlobalOptions.InvariantCultureInfo));
                    // <nuyen />
                    objWriter.WriteElementString("nuyen", _decNuyen.ToString(GlobalOptions.InvariantCultureInfo));
                    // <startingnuyen />
                    objWriter.WriteElementString("startingnuyen", _decStartingNuyen.ToString(GlobalOptions.InvariantCultureInfo));

                    // <nuyenbp />
                    objWriter.WriteElementString("nuyenbp", _decNuyenBP.ToString(GlobalOptions.InvariantCultureInfo));

                    // <adept />
                    objWriter.WriteElementString("adept", _blnAdeptEnabled.ToString(GlobalOptions.InvariantCultureInfo));
                    // <magician />
                    objWriter.WriteElementString("magician", _blnMagicianEnabled.ToString(GlobalOptions.InvariantCultureInfo));
                    // <technomancer />
                    objWriter.WriteElementString("technomancer", _blnTechnomancerEnabled.ToString(GlobalOptions.InvariantCultureInfo));
                    // <ai />
                    objWriter.WriteElementString("ai", _blnAdvancedProgramsEnabled.ToString(GlobalOptions.InvariantCultureInfo));
                    // <cyberwaredisabled />
                    objWriter.WriteElementString("cyberwaredisabled", _blnCyberwareDisabled.ToString(GlobalOptions.InvariantCultureInfo));
                    // <initiationdisabled />
                    objWriter.WriteElementString("initiationdisabled", _blnInitiationDisabled.ToString(GlobalOptions.InvariantCultureInfo));
                    // <critter />
                    objWriter.WriteElementString("critter", _blnCritterEnabled.ToString(GlobalOptions.InvariantCultureInfo));

                    // <prototypetranshuman />
                    objWriter.WriteElementString("prototypetranshuman", _decPrototypeTranshuman.ToString(GlobalOptions.InvariantCultureInfo));

                    // <attributes>
                    objWriter.WriteStartElement("attributes");
                    AttributeSection.Save(objWriter);
                    // </attributes>
                    objWriter.WriteEndElement();

                    // <magenabled />
                    objWriter.WriteElementString("magenabled", _blnMAGEnabled.ToString(GlobalOptions.InvariantCultureInfo));
                    // <initiategrade />
                    objWriter.WriteElementString("initiategrade", _intInitiateGrade.ToString(GlobalOptions.InvariantCultureInfo));
                    // <resenabled />
                    objWriter.WriteElementString("resenabled", _blnRESEnabled.ToString(GlobalOptions.InvariantCultureInfo));
                    // <submersiongrade />
                    objWriter.WriteElementString("submersiongrade", _intSubmersionGrade.ToString(GlobalOptions.InvariantCultureInfo));
                    // <depenabled />
                    objWriter.WriteElementString("depenabled", _blnDEPEnabled.ToString(GlobalOptions.InvariantCultureInfo));
                    // <groupmember />
                    objWriter.WriteElementString("groupmember", _blnGroupMember.ToString(GlobalOptions.InvariantCultureInfo));
                    // <groupname />
                    objWriter.WriteElementString("groupname", _strGroupName);
                    // <groupnotes />
                    objWriter.WriteElementString("groupnotes", _strGroupNotes);

                    // External reader friendly stuff.
                    objWriter.WriteElementString("totaless", Essence().ToString(GlobalOptions.InvariantCultureInfo));

                    // Write out the Mystic Adept MAG split info.
                    if (_blnAdeptEnabled && _blnMagicianEnabled)
                    {
                        objWriter.WriteElementString("magsplitadept", _intMAGAdept.ToString(GlobalOptions.InvariantCultureInfo));
                        objWriter.WriteElementString("magsplitmagician", _intMAGMagician.ToString(GlobalOptions.InvariantCultureInfo));
                    }

                    _objTradition?.Save(objWriter);

                    // Condition Monitor Progress.
                    // <physicalcmfilled />
                    objWriter.WriteElementString("physicalcmfilled", _intPhysicalCMFilled.ToString(GlobalOptions.InvariantCultureInfo));
                    // <stuncmfilled />
                    objWriter.WriteElementString("stuncmfilled", _intStunCMFilled.ToString(GlobalOptions.InvariantCultureInfo));

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

                    // <drugs>
                    objWriter.WriteStartElement("drugs");
                    foreach (Drug objDrug in _lstDrugs)
                    {
                        objDrug.Save(objWriter);
                    }

                    // </drugs>
                    objWriter.WriteEndElement();

                    // <mentorspirits>
                    objWriter.WriteStartElement("mentorspirits");
                    foreach (MentorSpirit objMentor in _lstMentorSpirits)
                    {
                        objMentor.Save(objWriter);
                    }

                    // </mentorspirits>
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
                    foreach (Location objLocation in _lstGearLocations)
                    {
                        objLocation.Save(objWriter);
                    }

                    // </locations>
                    objWriter.WriteEndElement();

                    // <armorlocations>
                    objWriter.WriteStartElement("armorlocations");
                    foreach (Location objLocation in _lstArmorLocations)
                    {
                        objLocation.Save(objWriter);
                    }

                    // </armorlocations>
                    objWriter.WriteEndElement();

                    // <vehiclelocations>
                    objWriter.WriteStartElement("vehiclelocations");
                    foreach (Location objLocation in _lstVehicleLocations)
                    {
                        objLocation.Save(objWriter);
                    }

                    // </vehiclelocations>
                    objWriter.WriteEndElement();

                    // <weaponlocations>
                    objWriter.WriteStartElement("weaponlocations");
                    foreach (Location objLocation in _lstWeaponLocations)
                    {
                        objLocation.Save(objWriter);
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

                    //Plugins
                    if (Program.PluginLoader?.MyActivePlugins?.Count > 0)
                    {
                        // <plugins>
                        objWriter.WriteStartElement("plugins");
                        foreach (var plugin in Program.PluginLoader.MyActivePlugins)
                        {
                            try
                            {
                                System.Reflection.Assembly pluginAssm = plugin.GetPluginAssembly();
                                objWriter.WriteStartElement(pluginAssm.GetName().Name);
                                objWriter.WriteAttributeString("version", pluginAssm.GetName().Version.ToString());
                                objWriter.WriteString(plugin.GetSaveToFileElement(this));
                                objWriter.WriteEndElement();
                            }
                            catch (Exception e)
                            {
                                Log.Warn(e, "Exception while writing saveFileElement for plugin " + plugin + ": ");
                            }
                        }

                        //</plugins>
                        objWriter.WriteEndElement();
                    }

                    //calculatedValues
                    objWriter.WriteStartElement("calculatedvalues");
                    objWriter.WriteComment("these values are not loaded and only stored here for third parties, who parse this files (to not have to calculate them themselves)");
                    objWriter.WriteElementString("physicalcm", PhysicalCM.ToString(GlobalOptions.InvariantCultureInfo));
                    objWriter.WriteElementString("physicalcmthresholdoffset", PhysicalCMThresholdOffset.ToString(GlobalOptions.InvariantCultureInfo));
                    objWriter.WriteElementString("physicalcmoverflow", CMOverflow.ToString(GlobalOptions.InvariantCultureInfo));
                    objWriter.WriteElementString("stuncm", StunCM.ToString(GlobalOptions.InvariantCultureInfo));
                    objWriter.WriteElementString("stuncmthresholdoffset", StunCMThresholdOffset.ToString(GlobalOptions.InvariantCultureInfo));
                    objWriter.WriteEndElement();
                    // </calculatedValues>

                    // </character>
                    objWriter.WriteEndElement();

                    objWriter.WriteEndDocument();
                    objWriter.Flush();
                    objStream.Position = 0;

                    // Validate that the character can save properly. If there's no error, save the file to the listed file location.
                    try
                    {
                        XmlDocument objDoc = new XmlDocument
                        {
                            XmlResolver = null
                        };
                        using (XmlReader objXmlReader = XmlReader.Create(objStream, GlobalOptions.SafeXmlReaderSettings))
                            objDoc.Load(objXmlReader);
                        objDoc.Save(strFileName);
                    }
                    catch (IOException e)
                    {
                        Log.Error(e);
                        Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_Save_Error_Warning"));
                        blnErrorFree = false;
                    }
                    catch (XmlException)
                    {
                        Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_Save_Error_Warning"));
                        blnErrorFree = false;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_Save_Error_Warning"));
                        blnErrorFree = false;
                    }
                }
            }

            if(addToMRU)
                GlobalOptions.MostRecentlyUsedCharacters.Insert(0, FileName);

            IsSaving = false;
            _dateFileLastWriteTime = File.GetLastWriteTimeUtc(strFileName);

            if (callOnSaveCallBack)
                OnSaveCompleted?.Invoke(this, this);
            return blnErrorFree;
        }

        /// <summary>
        /// Syntactic sugar for XmlManager.LoadXPath() where we use the current enabled custom data directory list from our options file.
        /// XPathDocuments are usually faster than XmlDocuments, but are read-only and take longer to load if live custom data is enabled
        /// </summary>
        /// <param name="strFileName">Name of the XML file to load.</param>
        /// <param name="strLanguage">Language in which to load the data document.</param>
        /// <param name="blnLoadFile">Whether to force reloading content even if the file already exists.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public XPathDocument LoadDataXPath(string strFileName, string strLanguage = "", bool blnLoadFile = false)
        {
            return XmlManager.LoadXPath(strFileName, Options.EnabledCustomDataDirectoryPaths, strLanguage, blnLoadFile);
        }

        /// <summary>
        /// Syntactic sugar for XmlManager.LoadXPathAsync() where we use the current enabled custom data directory list from our options file.
        /// XPathDocuments are usually faster than XmlDocuments, but are read-only and take longer to load if live custom data is enabled
        /// </summary>
        /// <param name="strFileName">Name of the XML file to load.</param>
        /// <param name="strLanguage">Language in which to load the data document.</param>
        /// <param name="blnLoadFile">Whether to force reloading content even if the file already exists.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<XPathDocument> LoadDataXPathAsync(string strFileName, string strLanguage = "", bool blnLoadFile = false)
        {
            return await XmlManager.LoadXPathAsync(strFileName, Options.EnabledCustomDataDirectoryPaths, strLanguage, blnLoadFile).ConfigureAwait(false);
        }

        /// <summary>
        /// Syntactic sugar for XmlManager.Load() where we use the current enabled custom data directory list from our options file.
        /// </summary>
        /// <param name="strFileName">Name of the XML file to load.</param>
        /// <param name="strLanguage">Language in which to load the data document.</param>
        /// <param name="blnLoadFile">Whether to force reloading content even if the file already exists.</param>
        [NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public XmlDocument LoadData(string strFileName, string strLanguage = "", bool blnLoadFile = false)
        {
            return XmlManager.Load(strFileName, Options.EnabledCustomDataDirectoryPaths, strLanguage, blnLoadFile);
        }

        /// <summary>
        /// Syntactic sugar for XmlManager.LoadAsync() where we use the current enabled custom data directory list from our options file.
        /// </summary>
        /// <param name="strFileName">Name of the XML file to load.</param>
        /// <param name="strLanguage">Language in which to load the data document.</param>
        /// <param name="blnLoadFile">Whether to force reloading content even if the file already exists.</param>
        [NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<XmlDocument> LoadDataAsync(string strFileName, string strLanguage = "", bool blnLoadFile = false)
        {
            return await XmlManager.LoadAsync(strFileName, Options.EnabledCustomDataDirectoryPaths, strLanguage, blnLoadFile).ConfigureAwait(false);
        }

        public bool IsLoading { get; set; }

        public Queue<Action> PostLoadMethods => new Queue<Action>();

        /// <summary>
        /// Load the Character from an XML file.
        /// </summary>
        /// <param name="frmLoadingForm">Instance of frmLoading to use to update with loading progress. frmLoading::PerformStep() is called 35 times within this method, so plan accordingly.</param>
        /// <param name="showWarnings">Whether warnings about book content and other character content should be loaded.</param>
        public async Task<bool> Load(frmLoading frmLoadingForm = null, bool showWarnings = true)
        {
            if(!File.Exists(_strFileName))
                return false;
            using (var loadActivity = Timekeeper.StartSyncron("clsCharacter.Load", null, CustomActivity.OperationType.DependencyOperation, _strFileName))
            {
                try
                {
                    using (_ = Timekeeper.StartSyncron("upload_AI_options", loadActivity))
                    {
                        UploadObjectAsMetric.UploadObject(TelemetryClient, Options);
                    }
                    XmlDocument objXmlDocument = new XmlDocument
                    {
                        XmlResolver = null
                    };
                    XmlNode objXmlCharacter;
                    XPathNavigator xmlCharacterNavigator;
                    XmlNodeList objXmlNodeList;
                    XmlNodeList objXmlLocationList;
                    Quality objLivingPersonaQuality = null;
                    XmlNode xmlRootQualitiesNode;
                    using (_ = Timekeeper.StartSyncron("load_xml", loadActivity))
                    {
                        frmLoadingForm.DoThreadSafe(() => frmLoadingForm?.PerformStep("XML"));

                        if (!File.Exists(_strFileName))
                            return false;
                        try
                        {
                            using (StreamReader sr = new StreamReader(_strFileName, Encoding.UTF8, true))
                                using (XmlReader objXmlReader = XmlReader.Create(sr, GlobalOptions.SafeXmlReaderSettings))
                                    objXmlDocument.Load(objXmlReader);
                        }
                        catch (XmlException ex)
                        {
                            if (showWarnings)
                            {
                                 Program.MainForm.ShowMessageBox(
                                    string.Format(GlobalOptions.CultureInfo,
                                        LanguageManager.GetString("Message_FailedLoad"), ex.Message),
                                    string.Format(GlobalOptions.CultureInfo,
                                        LanguageManager.GetString("MessageTitle_FailedLoad"), ex.Message),
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }

                            return false;
                        }

                        //Timekeeper.Finish("load_xml");
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_misc", loadActivity))
                    {
                        frmLoadingForm.DoThreadSafe(() => frmLoadingForm?.PerformStep(LanguageManager.GetString("String_Settings")));
                        objXmlCharacter = objXmlDocument.SelectSingleNode("/character");
                        xmlCharacterNavigator =
                            objXmlDocument.GetFastNavigator().SelectSingleNode("/character");

                        if (objXmlCharacter == null || xmlCharacterNavigator == null)
                            return false;

                        IsLoading = true;

                        _dateFileLastWriteTime = File.GetLastWriteTimeUtc(_strFileName);

                        xmlCharacterNavigator.TryGetBoolFieldQuickly("ignorerules", ref _blnIgnoreRules);
                        xmlCharacterNavigator.TryGetBoolFieldQuickly("created", ref _blnCreated);

                        ResetCharacter();

                        // Get the game edition of the file if possible and make sure it's intended to be used with this version of the application.
                        string strGameEdition = string.Empty;
                        if (xmlCharacterNavigator.TryGetStringFieldQuickly("gameedition", ref strGameEdition) &&
                            !string.IsNullOrEmpty(strGameEdition) && strGameEdition != "SR5" && showWarnings &&
                            !Utils.IsUnitTest)
                        {
                            Program.MainForm.ShowMessageBox(
                                LanguageManager.GetString("Message_IncorrectGameVersion_SR4"),
                                LanguageManager.GetString("MessageTitle_IncorrectGameVersion"),
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Error);
                            IsLoading = false;
                            return false;
                        }

                        string strVersion = string.Empty;
                        //Check to see if the character was created in a version of Chummer later than the currently installed one.
                        if (xmlCharacterNavigator.TryGetStringFieldQuickly("appversion", ref strVersion) &&
                            !string.IsNullOrEmpty(strVersion))
                        {
                            if (strVersion.StartsWith("0.", StringComparison.Ordinal))
                            {
                                strVersion = strVersion.Substring(2);
                            }

                            if (!Version.TryParse(strVersion, out _verSavedVersion))
                                _verSavedVersion = new Version();
                            // Check for typo in Corrupter quality and correct it
                            else if (_verSavedVersion?.CompareTo(new Version(5, 188, 34)) == -1)
                            {
                                objXmlDocument.InnerXml = objXmlDocument.InnerXml.Replace("Corruptor", "Corrupter");
                                xmlCharacterNavigator =
                                    objXmlDocument.GetFastNavigator().SelectSingleNode("/character");
                                if (xmlCharacterNavigator == null)
                                    return false;
                            }
                        }
#if !DEBUG
                        if (!Utils.IsUnitTest)
                        {
                            Version verCurrentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                            if (verCurrentVersion.CompareTo(_verSavedVersion) == -1)
                            {
                                if (DialogResult.Yes != Program.MainForm.ShowMessageBox(
                                    string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("Message_OutdatedChummerSave"),
                                        _verSavedVersion.ToString(), verCurrentVersion.ToString()),
                                    LanguageManager.GetString("MessageTitle_IncorrectGameVersion"),
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Error))
                                {
                                    IsLoading = false;
                                    return false;
                                }
                            }
                        }
#endif
                        // Get the name of the settings file in use if possible.
                        xmlCharacterNavigator.TryGetStringFieldQuickly("settings", ref _strCharacterOptionsKey);

                        // Load the character's settings file.
                        string strDummy = string.Empty;
                        if (!xmlCharacterNavigator.TryGetStringFieldQuickly("buildmethod", ref strDummy)
                            || !Enum.TryParse(strDummy, true, out CharacterBuildMethod eSavedBuildMethod))
                        {
                            eSavedBuildMethod = OptionsManager.LoadedCharacterOptions.ContainsKey(GlobalOptions.DefaultCharacterOptionDefaultValue)
                                ? OptionsManager.LoadedCharacterOptions[GlobalOptions.DefaultCharacterOptionDefaultValue].BuildMethod
                                : CharacterBuildMethod.Priority;
                        }
                        bool blnShowSelectBP = false;
                        if (!OptionsManager.LoadedCharacterOptions.ContainsKey(_strCharacterOptionsKey)
                            || (!Created && OptionsManager.LoadedCharacterOptions[_strCharacterOptionsKey].BuildMethod != eSavedBuildMethod))
                        {
                            // Prompt if we want to switch options or leave
                            if (!Utils.IsUnitTest && showWarnings)
                            {
                                if (Program.MainForm.ShowMessageBox(string.Format(GlobalOptions.CultureInfo,
                                        LanguageManager.GetString(
                                            OptionsManager.LoadedCharacterOptions.ContainsKey(_strCharacterOptionsKey)
                                                ? "Message_CharacterOptions_DesyncBuildMethod"
                                                : "Message_CharacterOptions_CannotLoadSetting"),
                                        Path.GetFileNameWithoutExtension(_strCharacterOptionsKey)).WordWrap(),
                                    LanguageManager.GetString(
                                        OptionsManager.LoadedCharacterOptions.ContainsKey(_strCharacterOptionsKey)
                                            ? "MessageTitle_CharacterOptions_DesyncBuildMethod"
                                            : "MessageTitle_CharacterOptions_CannotLoadSetting"),
                                    MessageBoxButtons.YesNo) == DialogResult.No)
                                {
                                    IsLoading = false;
                                    return false;
                                }
                                blnShowSelectBP = true;
                            }
                            // Set up interim options for selection by build method
                            string strReplacementOptionsKey = OptionsManager.LoadedCharacterOptions.FirstOrDefault(x =>
                                x.Value.BuiltInOption && x.Value.BuildMethod == eSavedBuildMethod).Key;
                            if (string.IsNullOrEmpty(strReplacementOptionsKey))
                                strReplacementOptionsKey = GlobalOptions.DefaultCharacterOptionDefaultValue;
                            _strCharacterOptionsKey = strReplacementOptionsKey;
                        }
                        // Legacy load stuff
                        else if (!Utils.IsUnitTest
                                 && showWarnings
                                 && (xmlCharacterNavigator.SelectSingleNode("sources/source") != null
                                     || xmlCharacterNavigator.SelectSingleNode("customdatadirectorynames/directoryname") != null))
                        {
                            CharacterOptions objCurrentlyLoadedOptions = OptionsManager.LoadedCharacterOptions[_strCharacterOptionsKey];
                            HashSet<string> setBooks = new HashSet<string>();
                            foreach (XPathNavigator xmlBook in xmlCharacterNavigator.Select("sources/source"))
                                if (!string.IsNullOrEmpty(xmlBook.Value))
                                    setBooks.Add(xmlBook.Value);
                            // More books is fine, so just test if the stored book list is a subset of the current option's book list
                            bool blnPromptConfirmSetting = !setBooks.IsProperSubsetOf(objCurrentlyLoadedOptions.Books);
                            if (!blnPromptConfirmSetting)
                            {
                                List<string> lstCustomDataDirectoryNames = new List<string>();
                                foreach (XPathNavigator xmlCustomDataDirectoryName in xmlCharacterNavigator.Select("customdatadirectorynames/directoryname"))
                                    if (!string.IsNullOrEmpty(xmlCustomDataDirectoryName.Value))
                                        lstCustomDataDirectoryNames.Add(xmlCustomDataDirectoryName.Value);
                                // More custom data directories is not fine because additional ones might apply rules that weren't present before, so prompt
                                blnPromptConfirmSetting = lstCustomDataDirectoryNames.Count != objCurrentlyLoadedOptions.EnabledCustomDataDirectoryInfos.Count;
                                if (!blnPromptConfirmSetting)
                                {
                                    // Check to make sure all the names are the same
                                    for (int i = 0; i < lstCustomDataDirectoryNames.Count; ++i)
                                    {
                                        if (lstCustomDataDirectoryNames[i] != objCurrentlyLoadedOptions.EnabledCustomDataDirectoryInfos[i].Name)
                                        {
                                            blnPromptConfirmSetting = true;
                                            break;
                                        }
                                    }
                                }
                            }

                            if (blnPromptConfirmSetting)
                            {
                                DialogResult eShowBPResult = Program.MainForm.ShowMessageBox(string.Format(GlobalOptions.CultureInfo,
                                        LanguageManager.GetString("Message_CharacterOptions_DesyncBooksOrCustomData"),
                                        Path.GetFileNameWithoutExtension(_strCharacterOptionsKey)).WordWrap(),
                                    LanguageManager.GetString("MessageTitle_CharacterOptions_DesyncBooksOrCustomData"),
                                    MessageBoxButtons.YesNoCancel);
                                if (eShowBPResult == DialogResult.Cancel)
                                {
                                    IsLoading = false;
                                    return false;
                                }
                                blnShowSelectBP = eShowBPResult == DialogResult.Yes;
                            }
                        }

                        Options = OptionsManager.LoadedCharacterOptions[_strCharacterOptionsKey];

                        if (blnShowSelectBP)
                        {
                            DialogResult ePickBPResult = DialogResult.Cancel;
                            Program.MainForm.DoThreadSafe(() =>
                            {
                                using (frmSelectBuildMethod frmPickBP = new frmSelectBuildMethod(this, true))
                                {
                                    frmPickBP.ShowDialog(Program.MainForm);
                                    ePickBPResult = frmPickBP.DialogResult;
                                }
                            });
                            if (ePickBPResult != DialogResult.OK)
                            {
                                IsLoading = false;
                                return false;
                            }
                        }

                        if (xmlCharacterNavigator.TryGetDecFieldQuickly("essenceatspecialstart",
                            ref _decEssenceAtSpecialStart))
                        {
                            // fix to work around a mistake made when saving decimal values in previous versions.
                            if (_decEssenceAtSpecialStart > ESS.MetatypeMaximum)
                                _decEssenceAtSpecialStart /= 10;
                        }

                        xmlCharacterNavigator.TryGetStringFieldQuickly("createdversion", ref _strVersionCreated);

                        // Metatype information.
                        xmlCharacterNavigator.TryGetBoolFieldQuickly("iscritter", ref _blnIsCritter);
                        xmlCharacterNavigator.TryGetStringFieldQuickly("metatype", ref _strMetatype);
                        if (!xmlCharacterNavigator.TryGetGuidFieldQuickly("metatypeid", ref _guiMetatype))
                        {
                            if (!Guid.TryParse(GetNode(true)?.SelectSingleNode("id")?.Value, out _guiMetatype))
                            {
                                IsLoading = false;
                                return false;
                            }
                        }
                        xmlCharacterNavigator.TryGetStringFieldQuickly("movement", ref _strMovement);

                        xmlCharacterNavigator.TryGetStringFieldQuickly("walk", ref _strWalk);
                        xmlCharacterNavigator.TryGetStringFieldQuickly("run", ref _strRun);
                        xmlCharacterNavigator.TryGetStringFieldQuickly("sprint", ref _strSprint);

                        _strRunAlt = xmlCharacterNavigator.SelectSingleNode("run/@alt")?.Value ?? string.Empty;
                        _strWalkAlt = xmlCharacterNavigator.SelectSingleNode("walk/@alt")?.Value ?? string.Empty;
                        _strSprintAlt = xmlCharacterNavigator.SelectSingleNode("sprint/@alt")?.Value ?? string.Empty;
                        xmlCharacterNavigator.TryGetInt32FieldQuickly("initiativedice", ref _intInitiativeDice);
                        xmlCharacterNavigator.TryGetInt32FieldQuickly("metatypebp", ref _intMetatypeBP);
                        xmlCharacterNavigator.TryGetStringFieldQuickly("metavariant", ref _strMetavariant);
                        //Shim for characters created prior to Run Faster Errata
                        if (_strMetavariant == "Cyclopean")
                        {
                            _strMetavariant = "Cyclops";
                        }

                        //Shim for metavariants that were saved with an incorrect metatype string.
                        if (!string.IsNullOrEmpty(_strMetavariant) && _strMetatype == _strMetavariant)
                        {
                            _strMetatype = GetNode(true).SelectSingleNode("name")?.Value ?? "Human";
                        }


                        if (!xmlCharacterNavigator.TryGetGuidFieldQuickly("metavariantid", ref _guiMetavariant) && !string.IsNullOrEmpty(_strMetavariant))
                        {
                            _guiMetavariant = Guid.Parse(GetNode()?.SelectSingleNode("id")?.Value);
                        }

                        bool blnDoSourceFetch = !xmlCharacterNavigator.TryGetStringFieldQuickly("source", ref _strSource) || string.IsNullOrEmpty(_strSource);
                        // ReSharper disable once ConvertIfToOrExpression
                        if (!xmlCharacterNavigator.TryGetStringFieldQuickly("page", ref _strPage) || string.IsNullOrEmpty(_strPage) || _strPage == "0")
                            blnDoSourceFetch = true;
                        if (blnDoSourceFetch)
                        {
                            XPathNavigator xmlCharNode = GetNode();
                            if (xmlCharNode != null)
                            {
                                _strSource = xmlCharNode.SelectSingleNode("source")?.Value ?? _strSource;
                                _strPage = xmlCharNode.SelectSingleNode("page")?.Value ?? _strPage;
                            }
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

                        xmlCharacterNavigator.TryGetStringFieldQuickly("prioritymetatype", ref _strPriorityMetatype);
                        xmlCharacterNavigator.TryGetStringFieldQuickly("priorityattributes",
                            ref _strPriorityAttributes);
                        xmlCharacterNavigator.TryGetStringFieldQuickly("priorityspecial", ref _strPrioritySpecial);
                        xmlCharacterNavigator.TryGetStringFieldQuickly("priorityskills", ref _strPrioritySkills);
                        xmlCharacterNavigator.TryGetStringFieldQuickly("priorityresources", ref _strPriorityResources);
                        xmlCharacterNavigator.TryGetStringFieldQuickly("prioritytalent", ref _strPriorityTalent);
                        _lstPrioritySkills.Clear();
                        foreach (XPathNavigator xmlSkillName in xmlCharacterNavigator.Select(
                            "priorityskills/priorityskill")
                        )
                        {
                            _lstPrioritySkills.Add(xmlSkillName.Value);
                        }

                        string strSkill1 = string.Empty;
                        string strSkill2 = string.Empty;
                        if (xmlCharacterNavigator.TryGetStringFieldQuickly("priorityskill1", ref strSkill1) &&
                            !string.IsNullOrEmpty(strSkill1))
                            _lstPrioritySkills.Add(strSkill1);
                        if (xmlCharacterNavigator.TryGetStringFieldQuickly("priorityskill2", ref strSkill2) &&
                            !string.IsNullOrEmpty(strSkill2))
                            _lstPrioritySkills.Add(strSkill2);

                        xmlCharacterNavigator.TryGetBoolFieldQuickly("possessed", ref _blnPossessed);

                        xmlCharacterNavigator.TryGetInt32FieldQuickly("contactpoints", ref _intCachedContactPoints);
                        xmlCharacterNavigator.TryGetInt32FieldQuickly("contactpointsused", ref _intContactPointsUsed);
                        xmlCharacterNavigator.TryGetInt32FieldQuickly("cfplimit", ref _intCFPLimit);
                        xmlCharacterNavigator.TryGetInt32FieldQuickly("ainormalprogramlimit",
                            ref _intAINormalProgramLimit);
                        xmlCharacterNavigator.TryGetInt32FieldQuickly("aiadvancedprogramlimit",
                            ref _intAIAdvancedProgramLimit);
                        xmlCharacterNavigator.TryGetInt32FieldQuickly("currentcounterspellingdice",
                            ref _intCurrentCounterspellingDice);
                        xmlCharacterNavigator.TryGetInt32FieldQuickly("spelllimit", ref _intFreeSpells);
                        xmlCharacterNavigator.TryGetInt32FieldQuickly("karma", ref _intKarma);
                        xmlCharacterNavigator.TryGetInt32FieldQuickly("totalkarma", ref _intTotalKarma);

                        xmlCharacterNavigator.TryGetInt32FieldQuickly("special", ref _intSpecial);
                        xmlCharacterNavigator.TryGetInt32FieldQuickly("totalspecial", ref _intTotalSpecial);
                        //objXmlCharacter.tryGetInt32FieldQuickly("attributes", ref _intAttributes); //wonkey
                        xmlCharacterNavigator.TryGetInt32FieldQuickly("totalattributes", ref _intTotalAttributes);
                        xmlCharacterNavigator.TryGetInt32FieldQuickly("contactpoints", ref _intCachedContactPoints);
                        xmlCharacterNavigator.TryGetInt32FieldQuickly("contactpointsused", ref _intContactPointsUsed);
                        xmlCharacterNavigator.TryGetInt32FieldQuickly("streetcred", ref _intStreetCred);
                        xmlCharacterNavigator.TryGetInt32FieldQuickly("notoriety", ref _intNotoriety);
                        xmlCharacterNavigator.TryGetInt32FieldQuickly("publicawareness", ref _intPublicAwareness);
                        xmlCharacterNavigator.TryGetInt32FieldQuickly("burntstreetcred", ref _intBurntStreetCred);
                        xmlCharacterNavigator.TryGetInt32FieldQuickly("baseastralreputation", ref _intBaseAstralReputation);
                        xmlCharacterNavigator.TryGetInt32FieldQuickly("basewildreputation", ref _intBaseWildReputation);
                        xmlCharacterNavigator.TryGetDecFieldQuickly("nuyen", ref _decNuyen);
                        xmlCharacterNavigator.TryGetDecFieldQuickly("startingnuyen", ref _decStartingNuyen);
                        xmlCharacterNavigator.TryGetDecFieldQuickly("nuyenbp", ref _decNuyenBP);

                        xmlCharacterNavigator.TryGetBoolFieldQuickly("adept", ref _blnAdeptEnabled);
                        xmlCharacterNavigator.TryGetBoolFieldQuickly("magician", ref _blnMagicianEnabled);
                        xmlCharacterNavigator.TryGetBoolFieldQuickly("technomancer", ref _blnTechnomancerEnabled);
                        xmlCharacterNavigator.TryGetBoolFieldQuickly("ai", ref _blnAdvancedProgramsEnabled);
                        xmlCharacterNavigator.TryGetBoolFieldQuickly("cyberwaredisabled", ref _blnCyberwareDisabled);
                        xmlCharacterNavigator.TryGetBoolFieldQuickly("initiationdisabled", ref _blnInitiationDisabled);
                        xmlCharacterNavigator.TryGetBoolFieldQuickly("critter", ref _blnCritterEnabled);

                        xmlCharacterNavigator.TryGetDecFieldQuickly("prototypetranshuman", ref _decPrototypeTranshuman);
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
                        //end load_char_misc
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_mentorspirit", loadActivity))
                    {
                        frmLoadingForm.DoThreadSafe(() => frmLoadingForm?.PerformStep(LanguageManager.GetString("String_MentorSpirit")));
                        // Improvements.
                        objXmlNodeList = objXmlCharacter.SelectNodes("mentorspirits/mentorspirit");
                        foreach (XmlNode objXmlMentor in objXmlNodeList)
                        {
                            MentorSpirit objMentor = new MentorSpirit(this, objXmlMentor);
                            objMentor.Load(objXmlMentor);
                            _lstMentorSpirits.Add(objMentor);
                        }

                        //using finish("load_char_mentorspirit");
                    }

                    List<Improvement> lstCyberadeptSweepGrades = new List<Improvement>();
                    _lstInternalIdsNeedingReapplyImprovements.Clear();
                    using (_ = Timekeeper.StartSyncron("load_char_imp", loadActivity))
                    {
                        frmLoadingForm.DoThreadSafe(() => frmLoadingForm?.PerformStep(LanguageManager.GetString("Tab_Improvements")));
                        // Improvements.
                        objXmlNodeList = objXmlCharacter.SelectNodes("improvements/improvement");
                        string strCharacterInnerXml = objXmlCharacter.InnerXml;
                        bool removeImprovements = Utils.IsUnitTest;
                        foreach (XmlNode objXmlImprovement in objXmlNodeList)
                        {
                            string strImprovementSource = objXmlImprovement["improvementsource"]?.InnerText;
                            // Do not load condition monitor improvements from older versions of Chummer
                            if (strImprovementSource == "ConditionMonitor")
                                continue;

                            // Do not load essence loss improvements if this character does not have any attributes affected by essence loss
                            if (_decEssenceAtSpecialStart == decimal.MinValue &&
                                (strImprovementSource == "EssenceLoss" || strImprovementSource == "EssenceLossChargen"))
                                continue;

                            string strLoopSourceName = objXmlImprovement["sourcename"]?.InnerText;
                            if (!string.IsNullOrEmpty(strLoopSourceName) && strLoopSourceName.IsGuid() &&
                                objXmlImprovement["custom"]?.InnerText != bool.TrueString)
                            {
                                // Hacky way to make sure this character isn't loading in any orphaned improvements.
                                // SourceName ID will pop up minimum twice in the save if the improvement's source is actually present: once in the improvement and once in the parent that added it.
                                if (strCharacterInnerXml.IndexOf(strLoopSourceName, StringComparison.Ordinal) ==
                                    strCharacterInnerXml.LastIndexOf(strLoopSourceName, StringComparison.Ordinal))
                                {
                                    if (removeImprovements || showWarnings)
                                    {
                                        //Utils.BreakIfDebug();
                                        if (removeImprovements
                                            || Program.MainForm.ShowMessageBox(
                                                LanguageManager.GetString("Message_OrphanedImprovements"),
                                                LanguageManager.GetString("MessageTitle_OrphanedImprovements"),
                                                MessageBoxButtons.YesNo,
                                                MessageBoxIcon.Error) == DialogResult.Yes)
                                        {
                                            removeImprovements = true;
                                            continue;
                                        }

                                        return false;
                                    }
                                }
                            }

                            Improvement objImprovement = new Improvement(this);
                            try
                            {
                                objImprovement.Load(objXmlImprovement);
                                _lstImprovements.Add(objImprovement);

                                if (objImprovement.ImproveType == Improvement.ImprovementType.SkillsoftAccess &&
                                    objImprovement.Value == 0)
                                {
                                    _lstInternalIdsNeedingReapplyImprovements.Add(objImprovement.SourceName);
                                }
                                // Cyberadept fix
                                else if (LastSavedVersion <= new Version(5, 212, 78)
                                         && objImprovement.ImproveSource == Improvement.ImprovementSource.Echo
                                         && objImprovement.ImproveType == Improvement.ImprovementType.Attribute
                                         && objImprovement.ImprovedName == "RESBase"
                                         && objImprovement.Value > 0
                                         && objImprovement.Value == objImprovement.Augmented)
                                {
                                    // Cyberadept in these versions was an echo. It is no longer an echo, and so needs a more complicated reapplication
                                    if (Options.SpecialKarmaCostBasedOnShownValue)
                                        _lstImprovements.Remove(objImprovement);
                                    else
                                        lstCyberadeptSweepGrades.Add(objImprovement);
                                }
                            }
                            catch (ArgumentException)
                            {
                                _lstInternalIdsNeedingReapplyImprovements.Add(
                                    objXmlImprovement["sourcename"]?.InnerText);
                            }
                        }

                        //Timekeeper.Finish("load_char_imp");
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_contacts", loadActivity))
                    {
                        frmLoadingForm.DoThreadSafe(() => frmLoadingForm?.PerformStep(LanguageManager.GetString("Label_Contacts")));
                        // Contacts.
                        foreach (XPathNavigator xmlContact in xmlCharacterNavigator.Select("contacts/contact"))
                        {
                            Contact objContact = new Contact(this);
                            objContact.Load(xmlContact);
                            _lstContacts.Add(objContact);
                        }

                        //Timekeeper.Finish("load_char_contacts");
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_quality", loadActivity))
                    {
                        frmLoadingForm.DoThreadSafe(() => frmLoadingForm?.PerformStep(LanguageManager.GetString("String_Qualities")));
                        // Qualities

                        objXmlNodeList = objXmlCharacter.SelectNodes("qualities/quality");
                        bool blnHasOldQualities = false;
                        xmlRootQualitiesNode =
                            LoadData("qualities.xml").SelectSingleNode("/chummer/qualities");
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
                                    if (objQuality.GetNode()?.SelectSingleNode("bonus/addgear/name")?.InnerText ==
                                        "Living Persona")
                                        objLivingPersonaQuality = objQuality;
                                    // Legacy shim
                                    if (LastSavedVersion <= new Version(5, 195, 1)
                                        && (objQuality.Name == "The Artisan's Way"
                                            || objQuality.Name == "The Artist's Way"
                                            || objQuality.Name == "The Athlete's Way"
                                            || objQuality.Name == "The Burnout's Way"
                                            || objQuality.Name == "The Invisible Way"
                                            || objQuality.Name == "The Magician's Way"
                                            || objQuality.Name == "The Speaker's Way"
                                            || objQuality.Name == "The Warrior's Way")
                                        && objQuality.Bonus?.HasChildNodes == false)
                                    {
                                        ImprovementManager.RemoveImprovements(this,
                                            Improvement.ImprovementSource.Quality,
                                            objQuality.InternalId);
                                        XmlNode objNode = objQuality.GetNode();
                                        if (objNode != null)
                                        {
                                            objQuality.Bonus = objNode["bonus"];
                                            if (objQuality.Bonus != null)
                                            {
                                                ImprovementManager.ForcedValue = objQuality.Extra;
                                                ImprovementManager.CreateImprovements(this,
                                                    Improvement.ImprovementSource.Quality,
                                                    objQuality.InternalId, objQuality.Bonus, 1, objQuality.DisplayNameShort(GlobalOptions.Language));
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
                                                    if (objCheckQuality != objQuality &&
                                                        objCheckQuality.SourceIDString == objQuality.SourceIDString &&
                                                        objCheckQuality.Extra == objQuality.Extra &&
                                                        objCheckQuality.SourceName == objQuality.SourceName)
                                                    {
                                                        blnDoFirstLevel = false;
                                                        break;
                                                    }
                                                }

                                                if (blnDoFirstLevel)
                                                {
                                                    ImprovementManager.ForcedValue = objQuality.Extra;
                                                    ImprovementManager.CreateImprovements(this,
                                                        Improvement.ImprovementSource.Quality, objQuality.InternalId,
                                                        objQuality.FirstLevelBonus, 1, objQuality.DisplayNameShort(GlobalOptions.Language));
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

                                    if (LastSavedVersion <= new Version(5, 200, 0)
                                        && objQuality.Name == "Made Man"
                                        && objQuality.Bonus["selectcontact"] != null)
                                    {
                                        string selectedContactUniqueId = (Improvements.FirstOrDefault(x =>
                                                x.SourceName == objQuality.InternalId &&
                                                x.ImproveType == Improvement.ImprovementType.ContactForcedLoyalty))
                                            ?.ImprovedName;
                                        if (string.IsNullOrWhiteSpace(selectedContactUniqueId))
                                        {
                                            selectedContactUniqueId =
                                                Contacts.FirstOrDefault(x => x.Name == objQuality.Extra)?.UniqueId;
                                        }

                                        if (string.IsNullOrWhiteSpace(selectedContactUniqueId))
                                        {
                                            // Populate the Magician Traditions list.
                                            List<ListItem> lstContacts = new List<ListItem>(Contacts.Count);
                                            foreach (Contact objContact in Contacts)
                                            {
                                                if (objContact.IsGroup)
                                                    lstContacts.Add(new ListItem(objContact.Name, objContact.UniqueId));
                                            }

                                            if (lstContacts.Count > 1)
                                            {
                                                lstContacts.Sort(CompareListItems.CompareNames);
                                            }

                                            DialogResult ePickItemResult = DialogResult.Cancel;
                                            Program.MainForm.DoThreadSafe(() =>
                                            {
                                                using (frmSelectItem frmPickItem = new frmSelectItem())
                                                {
                                                    frmPickItem.SetDropdownItemsMode(lstContacts);
                                                    frmPickItem.ShowDialog(Program.MainForm);

                                                    ePickItemResult = frmPickItem.DialogResult;
                                                    selectedContactUniqueId = frmPickItem.SelectedItem;
                                                }
                                            });
                                            // Make sure the dialogue window was not canceled.
                                            if (ePickItemResult != DialogResult.OK)
                                            {
                                                IsLoading = false;
                                                return false;
                                            }
                                        }

                                        objQuality.Bonus =
                                            xmlRootQualitiesNode.SelectSingleNode("quality[name=\"Made Man\"]/bonus");
                                        objQuality.Extra = string.Empty;
                                        ImprovementManager.RemoveImprovements(this,
                                            Improvement.ImprovementSource.Quality,
                                            objQuality.InternalId);
                                        ImprovementManager.CreateImprovement(this, string.Empty,
                                            Improvement.ImprovementSource.Quality, objQuality.InternalId,
                                            Improvement.ImprovementType.MadeMan,
                                            objQuality.DisplayNameShort(GlobalOptions.Language));
                                        ImprovementManager.CreateImprovement(this, selectedContactUniqueId,
                                            Improvement.ImprovementSource.Quality, objQuality.InternalId,
                                            Improvement.ImprovementType.AddContact,
                                            objQuality.DisplayNameShort(GlobalOptions.Language));
                                        ImprovementManager.CreateImprovement(this, selectedContactUniqueId,
                                            Improvement.ImprovementSource.Quality, objQuality.InternalId,
                                            Improvement.ImprovementType.ContactForcedLoyalty,
                                            objQuality.DisplayNameShort(GlobalOptions.Language));
                                        ImprovementManager.CreateImprovement(this, selectedContactUniqueId,
                                            Improvement.ImprovementSource.Quality, objQuality.InternalId,
                                            Improvement.ImprovementType.ContactForceGroup,
                                            objQuality.DisplayNameShort(GlobalOptions.Language));
                                        ImprovementManager.CreateImprovement(this, selectedContactUniqueId,
                                            Improvement.ImprovementSource.Quality, objQuality.InternalId,
                                            Improvement.ImprovementType.ContactMakeFree,
                                            objQuality.DisplayNameShort(GlobalOptions.Language));
                                    }

                                    if (LastSavedVersion <= new Version(5, 212, 43)
                                        && objQuality.Name == "Inspired"
                                        && objQuality.Source == "SASS"
                                        && objQuality.Bonus["selectexpertise"] == null)
                                    {
                                        // Old handling of SASS' Inspired quality was both hardcoded and wrong
                                        // Since SASS' Inspired requires the player to choose a specialization, we always need a prompt,
                                        // so add the quality to the list for processing when the character is opened.
                                        _lstInternalIdsNeedingReapplyImprovements.Add(objQuality.InternalId);
                                    }

                                    if (LastSavedVersion <= new Version(5, 212, 56)
                                        && objQuality.Name == "Chain Breaker"
                                        && objQuality.Bonus == null)
                                    {
                                        // Chain Breaker bonus requires manual selection of two spirit types, so we need a prompt.
                                        _lstInternalIdsNeedingReapplyImprovements.Add(objQuality.InternalId);
                                    }

                                    if (LastSavedVersion <= new Version(5, 212, 78)
                                        && objQuality.Name == "Resonant Stream: Cyberadept"
                                        && objQuality.Bonus == null)
                                    {
                                        objQuality.Bonus =
                                            xmlRootQualitiesNode.SelectSingleNode("quality[name=\"Resonant Stream: Cyberadept\"]/bonus");
                                        ImprovementManager.RemoveImprovements(this,
                                            Improvement.ImprovementSource.Quality,
                                            objQuality.InternalId);
                                        ImprovementManager.CreateImprovement(this, string.Empty,
                                            Improvement.ImprovementSource.Quality, objQuality.InternalId,
                                            Improvement.ImprovementType.CyberadeptDaemon,
                                            objQuality.DisplayNameShort(GlobalOptions.Language));
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
                        //Timekeeper.Finish("load_char_quality");
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_attributes", loadActivity))
                    {
                        frmLoadingForm.DoThreadSafe(() => frmLoadingForm?.PerformStep(LanguageManager.GetString("Label_Attributes")));
                        AttributeSection.Load(objXmlCharacter);
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_misc2", loadActivity))
                    {

                        // Attempt to load the split MAG CharacterAttribute information for Mystic Adepts.
                        if (_blnAdeptEnabled && _blnMagicianEnabled)
                        {
                            xmlCharacterNavigator.TryGetInt32FieldQuickly("magsplitadept", ref _intMAGAdept);
                            xmlCharacterNavigator.TryGetInt32FieldQuickly("magsplitmagician", ref _intMAGMagician);
                        }

                        frmLoadingForm.DoThreadSafe(() => frmLoadingForm?.PerformStep(LanguageManager.GetString("String_Tradition")));
                        // Attempt to load in the character's tradition (or equivalent for Technomancers)
                        string strTemp = string.Empty;
                        if (xmlCharacterNavigator.TryGetStringFieldQuickly("stream", ref strTemp) &&
                            !string.IsNullOrEmpty(strTemp) && RESEnabled)
                        {
                            // Legacy load a Technomancer tradition
                            XmlNode xmlTraditionListDataNode =
                                LoadData("streams.xml").SelectSingleNode("/chummer/traditions");
                            if (xmlTraditionListDataNode != null)
                            {
                                XmlNode xmlTraditionDataNode =
                                    xmlTraditionListDataNode.SelectSingleNode("tradition[name = \"" + strTemp + "\"]");
                                if (xmlTraditionDataNode != null)
                                {
                                    if (!_objTradition.Create(xmlTraditionDataNode, true))
                                        _objTradition.ResetTradition();
                                }
                                else
                                {
                                    xmlTraditionDataNode =
                                        xmlTraditionListDataNode.SelectSingleNode("tradition[name = \"Default\"]");
                                    if (xmlTraditionDataNode != null)
                                    {
                                        if (!_objTradition.Create(xmlTraditionDataNode, true))
                                            _objTradition.ResetTradition();
                                    }
                                    else
                                    {
                                        xmlTraditionDataNode = xmlTraditionListDataNode.SelectSingleNode("tradition");
                                        if (xmlTraditionDataNode != null)
                                        {
                                            if (!_objTradition.Create(xmlTraditionDataNode, true))
                                                _objTradition.ResetTradition();
                                        }
                                    }
                                }
                            }

                            if (_objTradition.Type != TraditionType.None)
                            {
                                _objTradition.LegacyLoad(xmlCharacterNavigator);
                            }
                        }
                        else
                        {
                            XPathNavigator xpathTraditionNavigator =
                                xmlCharacterNavigator.SelectSingleNode("tradition");
                            // Regular tradition load
                            if (xpathTraditionNavigator?.SelectSingleNode("guid") != null ||
                                xpathTraditionNavigator?.SelectSingleNode("id") != null)
                            {
                                _objTradition.Load(objXmlCharacter.SelectSingleNode("tradition"));
                            }
                            // Not null but doesn't have children -> legacy load a magical tradition
                            else if (xpathTraditionNavigator != null && MAGEnabled)
                            {
                                XmlNode xmlTraditionListDataNode =
                                    LoadData("traditions.xml").SelectSingleNode("/chummer/traditions");
                                if (xmlTraditionListDataNode != null)
                                {
                                    xmlCharacterNavigator.TryGetStringFieldQuickly("tradition", ref strTemp);
                                    XmlNode xmlTraditionDataNode =
                                        xmlTraditionListDataNode.SelectSingleNode(
                                            "tradition[name = \"" + strTemp + "\"]");
                                    if (xmlTraditionDataNode != null)
                                    {
                                        if (!_objTradition.Create(xmlTraditionDataNode))
                                            _objTradition.ResetTradition();
                                    }
                                    else
                                    {
                                        xmlTraditionDataNode =
                                            xmlTraditionListDataNode.SelectSingleNode(
                                                "tradition[id = \"" + Tradition.CustomMagicalTraditionGuid + "\"]");
                                        if (xmlTraditionDataNode != null)
                                        {
                                            if (!_objTradition.Create(xmlTraditionDataNode))
                                                _objTradition.ResetTradition();
                                        }
                                    }
                                }

                                if (_objTradition.Type != TraditionType.None)
                                {
                                    _objTradition.LegacyLoad(xmlCharacterNavigator);
                                }
                            }
                        }

                        // Attempt to load Condition Monitor Progress.
                        xmlCharacterNavigator.TryGetInt32FieldQuickly("physicalcmfilled", ref _intPhysicalCMFilled);
                        xmlCharacterNavigator.TryGetInt32FieldQuickly("stuncmfilled", ref _intStunCMFilled);
                        //Timekeeper.Finish("load_char_misc2");
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_skills", loadActivity)) //slightly messy
                    {
                        frmLoadingForm.DoThreadSafe(() => frmLoadingForm?.PerformStep(LanguageManager.GetString("Tab_Skills")));
                        _oldSkillsBackup = objXmlCharacter.SelectSingleNode("skills")?.Clone();
                        _oldSkillGroupBackup = objXmlCharacter.SelectSingleNode("skillgroups")?.Clone();

                        XmlNode objSkillNode = objXmlCharacter.SelectSingleNode("newskills");
                        if (objSkillNode != null)
                        {
                            SkillsSection.Load(objSkillNode, false, loadActivity);
                        }
                        else
                        {
                            SkillsSection.Load(objXmlCharacter, true, loadActivity);
                        }

                        //Timekeeper.Finish("load_char_skills");
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_loc", loadActivity))
                    {
                        frmLoadingForm.DoThreadSafe(() => frmLoadingForm?.PerformStep(LanguageManager.GetString("String_Locations")));
                        // Locations.
                        objXmlLocationList = objXmlCharacter.SelectNodes("gearlocations/gearlocation");
                        foreach (XmlNode objXmlLocation in objXmlLocationList)
                        {
                            Location objLocation = new Location(this, _lstGearLocations);
                            objLocation.Load(objXmlLocation);
                        }

                        objXmlLocationList = objXmlCharacter.SelectNodes("locations/location");
                        foreach (XmlNode objXmlLocation in objXmlLocationList)
                        {
                            Location objLocation = new Location(this, _lstGearLocations);
                            objLocation.Load(objXmlLocation);
                        }

                        objXmlLocationList = objXmlCharacter.SelectNodes("gearlocations/location");
                        foreach (XmlNode objXmlLocation in objXmlLocationList)
                        {
                            Location objLocation = new Location(this, _lstGearLocations);
                            objLocation.Load(objXmlLocation);
                        }

                        //Timekeeper.Finish("load_char_loc");
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_abundle", loadActivity))
                    {
                        // Armor Bundles.
                        objXmlLocationList = objXmlCharacter.SelectNodes("armorbundles/armorbundle");
                        foreach (XmlNode objXmlLocation in objXmlLocationList)
                        {
                            Location objLocation = new Location(this, _lstArmorLocations);
                            objLocation.Load(objXmlLocation);
                        }

                        objXmlLocationList = objXmlCharacter.SelectNodes("armorlocations/armorlocation");
                        foreach (XmlNode objXmlLocation in objXmlLocationList)
                        {
                            Location objLocation = new Location(this, _lstArmorLocations);
                            objLocation.Load(objXmlLocation);
                        }

                        objXmlLocationList = objXmlCharacter.SelectNodes("armorlocations/location");
                        foreach (XmlNode objXmlLocation in objXmlLocationList)
                        {
                            Location objLocation = new Location(this, _lstArmorLocations);
                            objLocation.Load(objXmlLocation);
                        }

                        //Timekeeper.Finish("load_char_abundle");
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_vloc", loadActivity))
                    {
                        // Vehicle Locations.
                        XmlNodeList objXmlVehicleLocationList =
                            objXmlCharacter.SelectNodes("vehiclelocations/vehiclelocation");
                        foreach (XmlNode objXmlLocation in objXmlVehicleLocationList)
                        {
                            Location objLocation = new Location(this, _lstVehicleLocations);
                            objLocation.Load(objXmlLocation);
                        }

                        objXmlVehicleLocationList = objXmlCharacter.SelectNodes("vehiclelocations/location");
                        foreach (XmlNode objXmlLocation in objXmlVehicleLocationList)
                        {
                            Location objLocation = new Location(this, _lstVehicleLocations);
                            objLocation.Load(objXmlLocation);
                        }

                        //Timekeeper.Finish("load_char_vloc");
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_wloc", loadActivity))
                    {

                        // Weapon Locations.
                        XmlNodeList objXmlWeaponLocationList =
                            objXmlCharacter.SelectNodes("weaponlocations/weaponlocation");
                        foreach (XmlNode objXmlLocation in objXmlWeaponLocationList)
                        {
                            Location objLocation = new Location(this, _lstWeaponLocations);
                            objLocation.Load(objXmlLocation);
                        }

                        objXmlWeaponLocationList = objXmlCharacter.SelectNodes("weaponlocations/location");
                        foreach (XmlNode objXmlLocation in objXmlWeaponLocationList)
                        {
                            Location objLocation = new Location(this, _lstWeaponLocations);
                            objLocation.Load(objXmlLocation);
                        }

                        //Timekeeper.Finish("load_char_wloc");
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_sfoci", loadActivity))
                    {
                        // Stacked Foci.
                        objXmlNodeList = objXmlCharacter.SelectNodes("stackedfoci/stackedfocus");
                        foreach (XmlNode objXmlStack in objXmlNodeList)
                        {
                            StackedFocus objStack = new StackedFocus(this);
                            objStack.Load(objXmlStack);
                            _lstStackedFoci.Add(objStack);
                        }

                        //Timekeeper.Finish("load_char_sfoci");
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_armor", loadActivity))
                    {
                        frmLoadingForm.DoThreadSafe(() => frmLoadingForm?.PerformStep(LanguageManager.GetString("Tab_Armor")));
                        // Armor.
                        objXmlNodeList = objXmlCharacter.SelectNodes("armors/armor");
                        foreach (XmlNode objXmlArmor in objXmlNodeList)
                        {
                            Armor objArmor = new Armor(this);
                            objArmor.Load(objXmlArmor);
                            _lstArmor.Add(objArmor);
                        }

                        //Timekeeper.Finish("load_char_armor");
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_weapons", loadActivity))
                    {
                        frmLoadingForm.DoThreadSafe(() => frmLoadingForm?.PerformStep(LanguageManager.GetString("Tab_Weapons")));
                        // Weapons.
                        objXmlNodeList = objXmlCharacter.SelectNodes("weapons/weapon");
                        foreach (XmlNode objXmlWeapon in objXmlNodeList)
                        {
                            Weapon objWeapon = new Weapon(this);
                            objWeapon.Load(objXmlWeapon);
                            _lstWeapons.Add(objWeapon);
                        }

                        //Timekeeper.Finish("load_char_weapons");
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_drugs", loadActivity))
                    {
                        frmLoadingForm.DoThreadSafe(() => frmLoadingForm?.PerformStep(LanguageManager.GetString("Tab_Drugs")));
                        // Drugs.
                        objXmlNodeList = objXmlDocument.SelectNodes("/character/drugs/drug");
                        foreach (XmlNode objXmlDrug in objXmlNodeList)
                        {
                            Drug objDrug = new Drug(this);
                            objDrug.Load(objXmlDrug);
                            _lstDrugs.Add(objDrug);
                        }

                        //Timekeeper.Finish("load_char_drugs");
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_ware", loadActivity))
                    {
                        frmLoadingForm.DoThreadSafe(() => frmLoadingForm?.PerformStep(LanguageManager.GetString("Tab_Cyberware")));
                        // Dictionary for instantly re-applying outdated improvements for 'ware with pair bonuses in legacy shim
                        Dictionary<Cyberware, int> dicPairableCyberwares = new Dictionary<Cyberware, int>();
                        // Cyberware/Bioware.
                        objXmlNodeList = objXmlCharacter.SelectNodes("cyberwares/cyberware");
                        foreach (XmlNode objXmlCyberware in objXmlNodeList)
                        {
                            Cyberware objCyberware = new Cyberware(this);
                            objCyberware.Load(objXmlCyberware);
                            _lstCyberware.Add(objCyberware);
                            // Legacy shim #1
                            if (objCyberware.Name == "Myostatin Inhibitor" &&
                                LastSavedVersion <= new Version(5, 195, 1) &&
                                !Improvements.Any(x =>
                                    x.SourceName == objCyberware.InternalId &&
                                    x.ImproveType == Improvement.ImprovementType.AttributeKarmaCost))
                            {
                                XmlNode objNode = objCyberware.GetNode();
                                if (objNode != null)
                                {
                                    ImprovementManager.RemoveImprovements(this, objCyberware.SourceType,
                                        objCyberware.InternalId);
                                    ImprovementManager.RemoveImprovements(this, objCyberware.SourceType,
                                        objCyberware.InternalId + "Pair");
                                    objCyberware.Bonus = objNode["bonus"];
                                    objCyberware.WirelessBonus = objNode["wirelessbonus"];
                                    objCyberware.PairBonus = objNode["pairbonus"];
                                    if (!string.IsNullOrEmpty(objCyberware.Forced) && objCyberware.Forced != "Right" &&
                                        objCyberware.Forced != "Left")
                                        ImprovementManager.ForcedValue = objCyberware.Forced;
                                    if (objCyberware.Bonus != null)
                                    {
                                        ImprovementManager.CreateImprovements(this, objCyberware.SourceType,
                                            objCyberware.InternalId, objCyberware.Bonus, objCyberware.Rating, objCyberware.DisplayNameShort(GlobalOptions.Language));
                                        if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                                            objCyberware.Extra = ImprovementManager.SelectedValue;
                                    }

                                    if (objCyberware.WirelessOn && objCyberware.WirelessBonus != null)
                                    {
                                        ImprovementManager.CreateImprovements(this, objCyberware.SourceType,
                                            objCyberware.InternalId, objCyberware.WirelessBonus, objCyberware.Rating,
                                            objCyberware.DisplayNameShort(GlobalOptions.Language));
                                        if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue) &&
                                            string.IsNullOrEmpty(objCyberware.Extra))
                                            objCyberware.Extra = ImprovementManager.SelectedValue;
                                    }

                                    if (!objCyberware.IsModularCurrentlyEquipped)
                                        objCyberware.ChangeModularEquip(false);
                                    else if (objCyberware.PairBonus != null)
                                    {
                                        Cyberware objMatchingCyberware = dicPairableCyberwares.Keys.FirstOrDefault(x =>
                                            x.Name == objCyberware.Name && x.Extra == objCyberware.Extra);
                                        if (objMatchingCyberware != null)
                                            dicPairableCyberwares[objMatchingCyberware] =
                                                dicPairableCyberwares[objMatchingCyberware] + 1;
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

                        // Legacy Shim #2 (needed to be separate because we're dealing with PairBonuses here, and we don't know if something needs its PairBonus reapplied until all Cyberwares have been loaded)
                        if (LastSavedVersion <= new Version(5, 200, 0))
                        {
                            foreach (Cyberware objCyberware in Cyberware)
                            {
                                if (objCyberware.PairBonus?.HasChildNodes == true &&
                                    !Cyberware.DeepAny(x => x.Children, x =>
                                        objCyberware.IncludePair.Contains(x.Name) && x.Extra == objCyberware.Extra &&
                                        x.IsModularCurrentlyEquipped &&
                                        Improvements.Any(y => y.SourceName == x.InternalId + "Pair")))
                                {
                                    XmlNode objNode = objCyberware.GetNode();
                                    if (objNode != null)
                                    {
                                        ImprovementManager.RemoveImprovements(this, objCyberware.SourceType,
                                            objCyberware.InternalId);
                                        ImprovementManager.RemoveImprovements(this, objCyberware.SourceType,
                                            objCyberware.InternalId + "Pair");
                                        objCyberware.Bonus = objNode["bonus"];
                                        objCyberware.WirelessBonus = objNode["wirelessbonus"];
                                        objCyberware.PairBonus = objNode["pairbonus"];
                                        if (!string.IsNullOrEmpty(objCyberware.Forced) &&
                                            objCyberware.Forced != "Right" &&
                                            objCyberware.Forced != "Left")
                                            ImprovementManager.ForcedValue = objCyberware.Forced;
                                        if (objCyberware.Bonus != null)
                                        {
                                            ImprovementManager.CreateImprovements(this, objCyberware.SourceType,
                                                objCyberware.InternalId, objCyberware.Bonus, objCyberware.Rating, objCyberware.DisplayNameShort(GlobalOptions.Language));
                                            if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                                                objCyberware.Extra = ImprovementManager.SelectedValue;
                                        }

                                        if (objCyberware.WirelessOn && objCyberware.WirelessBonus != null)
                                        {
                                            ImprovementManager.CreateImprovements(this, objCyberware.SourceType,
                                                objCyberware.InternalId, objCyberware.WirelessBonus, objCyberware.Rating,
                                                objCyberware.DisplayNameShort(GlobalOptions.Language));
                                            if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue) &&
                                                string.IsNullOrEmpty(objCyberware.Extra))
                                                objCyberware.Extra = ImprovementManager.SelectedValue;
                                        }

                                        if (!objCyberware.IsModularCurrentlyEquipped)
                                            objCyberware.ChangeModularEquip(false);
                                        else if (objCyberware.PairBonus != null)
                                        {
                                            Cyberware objMatchingCyberware = dicPairableCyberwares.Keys.FirstOrDefault(
                                                x =>
                                                    x.Name == objCyberware.Name && x.Extra == objCyberware.Extra);
                                            if (objMatchingCyberware != null)
                                                dicPairableCyberwares[objMatchingCyberware] =
                                                    dicPairableCyberwares[objMatchingCyberware] + 1;
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
                        }

                        // Separate Pass for PairBonuses
                        foreach (KeyValuePair<Cyberware, int> objItem in dicPairableCyberwares)
                        {
                            Cyberware objCyberware = objItem.Key;
                            int intCyberwaresCount = objItem.Value;
                            List<Cyberware> lstPairableCyberwares = Cyberware.DeepWhere(x => x.Children,
                                x => objCyberware.IncludePair.Contains(x.Name) && x.Extra == objCyberware.Extra &&
                                     x.IsModularCurrentlyEquipped).ToList();
                            // Need to use slightly different logic if this cyberware has a location (Left or Right) and only pairs with itself because Lefts can only be paired with Rights and Rights only with Lefts
                            if (!string.IsNullOrEmpty(objCyberware.Location) &&
                                objCyberware.IncludePair.All(x => x == objCyberware.Name))
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
                                    if ((intCyberwaresCount & 1) == 0)
                                    {
                                        if (!string.IsNullOrEmpty(objCyberware.Forced) &&
                                            objCyberware.Forced != "Right" &&
                                            objCyberware.Forced != "Left")
                                            ImprovementManager.ForcedValue = objCyberware.Forced;
                                        ImprovementManager.CreateImprovements(this, objLoopCyberware.SourceType,
                                            objLoopCyberware.InternalId + "Pair", objLoopCyberware.PairBonus, objLoopCyberware.Rating,
                                            objLoopCyberware.DisplayNameShort(GlobalOptions.Language));
                                        if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue) &&
                                            string.IsNullOrEmpty(objCyberware.Extra))
                                            objCyberware.Extra = ImprovementManager.SelectedValue;
                                    }

                                    intCyberwaresCount -= 1;
                                    if (intCyberwaresCount <= 0)
                                        break;
                                }
                            }
                        }

                        //Timekeeper.Finish("load_char_ware");
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_spells", loadActivity))
                    {
                        frmLoadingForm.DoThreadSafe(() => frmLoadingForm?.PerformStep(LanguageManager.GetString("Label_SelectedSpells")));
                        // Spells.
                        objXmlNodeList = objXmlCharacter.SelectNodes("spells/spell");
                        foreach (XmlNode objXmlSpell in objXmlNodeList)
                        {
                            Spell objSpell = new Spell(this);
                            objSpell.Load(objXmlSpell);
                            _lstSpells.Add(objSpell);
                        }

                        //Timekeeper.Finish("load_char_spells");
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_powers", loadActivity))
                    {
                        frmLoadingForm.DoThreadSafe(() => frmLoadingForm?.PerformStep(LanguageManager.GetString("Tab_Adept")));
                        // Powers.
                        bool blnDoEnhancedAccuracyRefresh = LastSavedVersion <= new Version(5, 198, 26);
                        objXmlNodeList = objXmlCharacter.SelectNodes("powers/power");
                        List<ListItem> lstPowerOrder = new List<ListItem>(objXmlNodeList?.Count ?? 0);
                        // Sort the Powers in alphabetical order.
                        foreach (XmlNode xmlPower in objXmlNodeList)
                        {
                            string strGuid = xmlPower["guid"]?.InnerText;
                            string strPowerName = xmlPower["name"]?.InnerText ?? string.Empty;
                            if (blnDoEnhancedAccuracyRefresh && strPowerName == "Enhanced Accuracy (skill)")
                            {
                                _lstInternalIdsNeedingReapplyImprovements.Add(strGuid);
                            }

                            if (!string.IsNullOrEmpty(strGuid))
                                lstPowerOrder.Add(new ListItem(strGuid,
                                    strPowerName + (xmlPower["extra"]?.InnerText ?? string.Empty)));
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
                            XmlNode objNode =
                                objXmlCharacter.SelectSingleNode(
                                    "powers/power[guid = \"" + objItem.Value + "\"]");
                            if (objNode != null)
                            {
                                Power objPower = new Power(this);
                                objPower.Load(objNode);
                                _lstPowers.Add(objPower);
                            }
                        }

                        //Timekeeper.Finish("load_char_powers");
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_spirits", loadActivity))
                    {

                        frmLoadingForm.DoThreadSafe(() => frmLoadingForm?.PerformStep(LanguageManager.GetString("Label_Spirits")));
                        // Spirits/Sprites.
                        foreach (XPathNavigator xmlSpirit in xmlCharacterNavigator.Select("spirits/spirit"))
                        {
                            Spirit objSpirit = new Spirit(this);
                            objSpirit.Load(xmlSpirit);
                            _lstSpirits.Add(objSpirit);
                        }

                        if (!_lstSpirits.Any(s => s.Fettered) && Improvements.Any(imp =>
                                imp.ImproveSource == Improvement.ImprovementSource.SpiritFettering))
                        {
                            // If we don't have any Fettered spirits, make sure that we
                            ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.SpiritFettering);
                        }

                        //Timekeeper.Finish("load_char_spirits");
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_complex", loadActivity))
                    {
                        frmLoadingForm.DoThreadSafe(() => frmLoadingForm?.PerformStep(LanguageManager.GetString("Label_ComplexForms")));
                        // Compex Forms/Technomancer Programs.
                        objXmlNodeList = objXmlCharacter.SelectNodes("complexforms/complexform");
                        foreach (XmlNode objXmlComplexForm in objXmlNodeList)
                        {
                            ComplexForm objComplexForm = new ComplexForm(this);
                            objComplexForm.Load(objXmlComplexForm);
                            _lstComplexForms.Add(objComplexForm);
                        }

                        //Timekeeper.Finish("load_char_complex");
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_aiprogram", loadActivity))
                    {
                        frmLoadingForm.DoThreadSafe(() => frmLoadingForm?.PerformStep(LanguageManager.GetString("Tab_AdvancedPrograms")));
                        // Compex Forms/Technomancer Programs.
                        objXmlNodeList = objXmlCharacter.SelectNodes("aiprograms/aiprogram");
                        foreach (XmlNode objXmlProgram in objXmlNodeList)
                        {
                            AIProgram objProgram = new AIProgram(this);
                            objProgram.Load(objXmlProgram);
                            _lstAIPrograms.Add(objProgram);
                        }

                        //Timekeeper.Finish("load_char_aiprogram");
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_marts", loadActivity))
                    {
                        frmLoadingForm.DoThreadSafe(() => frmLoadingForm?.PerformStep(LanguageManager.GetString("Tab_MartialArts")));
                        // Martial Arts.
                        objXmlNodeList = objXmlCharacter.SelectNodes("martialarts/martialart");
                        foreach (XmlNode objXmlArt in objXmlNodeList)
                        {
                            MartialArt objMartialArt = new MartialArt(this);
                            objMartialArt.Load(objXmlArt);
                            _lstMartialArts.Add(objMartialArt);
                        }

                        //Timekeeper.Finish("load_char_marts");
                    }
#if LEGACY
                using (var op_load_char_mam = Timekeeper.StartSyncron("load_char_mam"))
                {

                    // Martial Art Maneuvers.
                    objXmlNodeList = objXmlCharacter.SelectNodes("martialartmaneuvers/martialartmaneuver");
                    foreach (XmlNode objXmlManeuver in objXmlNodeList)
                    {
                        MartialArtManeuver objManeuver = new MartialArtManeuver(this);
                        objManeuver.Load(objXmlManeuver);
                        _lstMartialArtManeuvers.Add(objManeuver);
                    }

                    //Timekeeper.Finish("load_char_mam");
                }
#endif
                    using (_ = Timekeeper.StartSyncron("load_char_mod", loadActivity))
                    {
                        frmLoadingForm.DoThreadSafe(() => frmLoadingForm?.PerformStep(LanguageManager.GetString("Tab_Limits")));
                        // Limit Modifiers.
                        objXmlNodeList = objXmlCharacter.SelectNodes("limitmodifiers/limitmodifier");
                        foreach (XmlNode objXmlLimit in objXmlNodeList)
                        {
                            LimitModifier obLimitModifier = new LimitModifier(this);
                            obLimitModifier.Load(objXmlLimit);
                            _lstLimitModifiers.Add(obLimitModifier);
                        }

                        //Timekeeper.Finish("load_char_mod");
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_lifestyle", loadActivity))
                    {
                        frmLoadingForm.DoThreadSafe(() => frmLoadingForm?.PerformStep(LanguageManager.GetString("String_SelectPACKSKit_Lifestyles")));
                        // Lifestyles.
                        objXmlNodeList = objXmlCharacter.SelectNodes("lifestyles/lifestyle");
                        foreach (XmlNode objXmlLifestyle in objXmlNodeList)
                        {
                            Lifestyle objLifestyle = new Lifestyle(this);
                            objLifestyle.Load(objXmlLifestyle);
                            _lstLifestyles.Add(objLifestyle);
                        }

                        //Timekeeper.Finish("load_char_lifestyle");
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_gear", loadActivity))
                    {
                        frmLoadingForm.DoThreadSafe(() => frmLoadingForm?.PerformStep(LanguageManager.GetString("Tab_Gear")));
                        // <gears>
                        objXmlNodeList = objXmlCharacter.SelectNodes("gears/gear");
                        foreach (XmlNode objXmlGear in objXmlNodeList)
                        {
                            Gear objGear = new Gear(this);
                            objGear.Load(objXmlGear);
                            _lstGear.Add(objGear);
                        }

                        // If the character has a technomancer quality but no Living Persona commlink, its improvements get re-applied immediately
                        if (objLivingPersonaQuality != null && LastSavedVersion <= new Version(5, 195, 1))
                        {
                            ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.Quality,
                                objLivingPersonaQuality.InternalId);

                            XmlNode objNode = objLivingPersonaQuality.GetNode();
                            if (objNode != null)
                            {
                                objLivingPersonaQuality.Bonus = objNode["bonus"];
                                if (objLivingPersonaQuality.Bonus != null)
                                {
                                    ImprovementManager.ForcedValue = objLivingPersonaQuality.Extra;
                                    ImprovementManager.CreateImprovements(this, Improvement.ImprovementSource.Quality,
                                        objLivingPersonaQuality.InternalId, objLivingPersonaQuality.Bonus, 1, objLivingPersonaQuality.DisplayNameShort(GlobalOptions.Language));
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
                                        if (objCheckQuality != objLivingPersonaQuality &&
                                            objCheckQuality.SourceIDString == objLivingPersonaQuality.SourceIDString &&
                                            objCheckQuality.Extra == objLivingPersonaQuality.Extra &&
                                            objCheckQuality.SourceName == objLivingPersonaQuality.SourceName)
                                        {
                                            blnDoFirstLevel = false;
                                            break;
                                        }
                                    }

                                    if (blnDoFirstLevel)
                                    {
                                        ImprovementManager.ForcedValue = objLivingPersonaQuality.Extra;
                                        ImprovementManager.CreateImprovements(this,
                                            Improvement.ImprovementSource.Quality,
                                            objLivingPersonaQuality.InternalId, objLivingPersonaQuality.FirstLevelBonus,
                                            1,
                                            objLivingPersonaQuality.DisplayNameShort(GlobalOptions.Language));
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

                        //Timekeeper.Finish("load_char_gear");
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_car", loadActivity))
                    {
                        frmLoadingForm.DoThreadSafe(() => frmLoadingForm?.PerformStep(LanguageManager.GetString("Label_Vehicles")));
                        // Vehicles.
                        objXmlNodeList = objXmlCharacter.SelectNodes("vehicles/vehicle");
                        foreach (XmlNode objXmlVehicle in objXmlNodeList)
                        {
                            Vehicle objVehicle = new Vehicle(this);
                            objVehicle.Load(objXmlVehicle);
                            _lstVehicles.Add(objVehicle);
                        }

                        //Timekeeper.Finish("load_char_car");
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_mmagic", loadActivity))
                    {
                        frmLoadingForm.DoThreadSafe(() => frmLoadingForm?.PerformStep(LanguageManager.GetString("String_Metamagics")));
                        // Metamagics/Echoes.
                        objXmlNodeList = objXmlCharacter.SelectNodes("metamagics/metamagic");
                        foreach (XmlNode objXmlMetamagic in objXmlNodeList)
                        {
                            Metamagic objMetamagic = new Metamagic(this);
                            objMetamagic.Load(objXmlMetamagic);
                            _lstMetamagics.Add(objMetamagic);
                        }

                        //Timekeeper.Finish("load_char_mmagic");
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_arts", loadActivity))
                    {
                        frmLoadingForm.DoThreadSafe(() => frmLoadingForm?.PerformStep(LanguageManager.GetString("String_Arts")));
                        // Arts
                        objXmlNodeList = objXmlCharacter.SelectNodes("arts/art");
                        foreach (XmlNode objXmlArt in objXmlNodeList)
                        {
                            Art objArt = new Art(this);
                            objArt.Load(objXmlArt);
                            _lstArts.Add(objArt);
                        }

                        //Timekeeper.Finish("load_char_arts");
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_ench", loadActivity))
                    {
                        frmLoadingForm.DoThreadSafe(() => frmLoadingForm?.PerformStep(LanguageManager.GetString("String_Enhancements")));
                        // Enhancements
                        objXmlNodeList = objXmlCharacter.SelectNodes("enhancements/enhancement");
                        foreach (XmlNode objXmlEnhancement in objXmlNodeList)
                        {
                            Enhancement objEnhancement = new Enhancement(this);
                            objEnhancement.Load(objXmlEnhancement);
                            _lstEnhancements.Add(objEnhancement);
                        }

                        //Timekeeper.Finish("load_char_ench");
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_cpow", loadActivity))
                    {
                        frmLoadingForm.DoThreadSafe(() => frmLoadingForm?.PerformStep(LanguageManager.GetString("Tab_Critter")));
                        // Critter Powers.
                        objXmlNodeList = objXmlCharacter.SelectNodes("critterpowers/critterpower");
                        foreach (XmlNode objXmlPower in objXmlNodeList)
                        {
                            CritterPower objPower = new CritterPower(this);
                            objPower.Load(objXmlPower);
                            _lstCritterPowers.Add(objPower);
                        }

                        //Timekeeper.Finish("load_char_cpow");
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_foci", loadActivity))
                    {
                        frmLoadingForm.DoThreadSafe(() => frmLoadingForm?.PerformStep(LanguageManager.GetString("Label_SummaryFoci")));
                        // Foci.
                        objXmlNodeList = objXmlCharacter.SelectNodes("foci/focus");
                        foreach (XmlNode objXmlFocus in objXmlNodeList)
                        {
                            Focus objFocus = new Focus(this);
                            objFocus.Load(objXmlFocus);
                            _lstFoci.Add(objFocus);
                        }

                        //Timekeeper.Finish("load_char_foci");
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_init", loadActivity))
                    {
                        frmLoadingForm.DoThreadSafe(() => frmLoadingForm?.PerformStep(LanguageManager.GetString("Label_SummaryInitiation")));
                        // Initiation Grades.
                        objXmlNodeList = objXmlCharacter.SelectNodes("initiationgrades/initiationgrade");
                        foreach (XmlNode objXmlGrade in objXmlNodeList)
                        {
                            InitiationGrade objGrade = new InitiationGrade(this);
                            objGrade.Load(objXmlGrade);
                            _lstInitiationGrades.Add(objGrade);
                        }

                        //Timekeeper.Finish("load_char_init");
                    }

                    frmLoadingForm.DoThreadSafe(() => frmLoadingForm?.PerformStep(LanguageManager.GetString("String_Expenses")));
                    // While expenses are to be saved in create mode due to starting nuyen and starting karma being logged as expense log entries,
                    // they shouldn't get loaded in create mode because they shouldn't be there.
                    if (Created)
                    {
                        using (_ = Timekeeper.StartSyncron("load_char_elog", loadActivity))
                        {
                            // Expense Log Entries.
                            XmlNodeList objXmlExpenseList = objXmlCharacter.SelectNodes("expenses/expense");
                            foreach (XmlNode objXmlExpense in objXmlExpenseList)
                            {
                                ExpenseLogEntry objExpenseLogEntry = new ExpenseLogEntry(this);
                                objExpenseLogEntry.Load(objXmlExpense);
                                _lstExpenseLog.AddWithSort(objExpenseLogEntry);
                            }

                            //Timekeeper.Finish("load_char_elog");
                        }
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
                    using (_ = Timekeeper.StartSyncron("load_char_igroup", loadActivity))
                    {
                        frmLoadingForm.DoThreadSafe(() => frmLoadingForm?.PerformStep(LanguageManager.GetString("Tab_Improvements")));
                        // Improvement Groups.
                        XmlNodeList objXmlGroupList = objXmlCharacter.SelectNodes("improvementgroups/improvementgroup");
                        foreach (XmlNode objXmlGroup in objXmlGroupList)
                        {
                            _lstImprovementGroups.Add(objXmlGroup.InnerText);
                        }

                        //Timekeeper.Finish("load_char_igroup");
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_calendar", loadActivity))
                    {
                        frmLoadingForm.DoThreadSafe(() => frmLoadingForm?.PerformStep(LanguageManager.GetString("Tab_Calendar")));
                        // Calendar.
                        XmlNodeList objXmlWeekList = objXmlCharacter.SelectNodes("calendar/week");
                        foreach (XmlNode objXmlWeek in objXmlWeekList)
                        {
                            CalendarWeek objWeek = new CalendarWeek();
                            objWeek.Load(objXmlWeek);
                            _lstCalendar.AddWithSort(objWeek, true);
                        }

                        //Timekeeper.Finish("load_char_calendar");
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_unarmed", loadActivity))
                    {
                        frmLoadingForm.DoThreadSafe(() => frmLoadingForm?.PerformStep(LanguageManager.GetString("String_LegacyFixes")));
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
                            XmlDocument objXmlWeaponDoc = LoadData("weapons.xml");
                            XmlNode objXmlWeapon =
                                objXmlWeaponDoc.SelectSingleNode("/chummer/weapons/weapon[name = \"Unarmed Attack\"]");
                            if (objXmlWeapon != null)
                            {
                                Weapon objWeapon = new Weapon(this);
                                objWeapon.Create(objXmlWeapon, _lstWeapons);
                                objWeapon.IncludedInWeapon = true; // Unarmed attack can never be removed
                                _lstWeapons.Add(objWeapon);
                            }
                        }

                        //Timekeeper.Finish("load_char_unarmed");
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_dwarffix", loadActivity))
                    {

                        // converting from old dwarven resistance to new dwarven resistance
                        if (Metatype.Equals("dwarf", StringComparison.OrdinalIgnoreCase))
                        {
                            Quality objOldQuality =
                                Qualities.FirstOrDefault(x => x.Name.Equals("Resistance to Pathogens and Toxins", StringComparison.Ordinal));
                            if (objOldQuality != null)
                            {
                                Qualities.Remove(objOldQuality);
                                if (Qualities.Any(x => x.Name.Equals("Resistance to Pathogens/Toxins", StringComparison.Ordinal)) == false &&
                                    Qualities.Any(x => x.Name.Equals("Dwarf Resistance", StringComparison.Ordinal)) == false)
                                {
                                    XmlNode objXmlDwarfQuality =
                                        xmlRootQualitiesNode.SelectSingleNode(
                                            "quality[name = \"Resistance to Pathogens/Toxins\"]") ??
                                        xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Dwarf Resistance\"]");

                                    List<Weapon> lstWeapons = new List<Weapon>(1);
                                    Quality objQuality = new Quality(this);

                                    objQuality.Create(objXmlDwarfQuality, QualitySource.Metatype, lstWeapons);
                                    foreach (Weapon objWeapon in lstWeapons)
                                        _lstWeapons.Add(objWeapon);
                                    _lstQualities.Add(objQuality);
                                }
                            }
                        }

                        //Timekeeper.Finish("load_char_dwarffix");
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_cyberadeptfix", loadActivity))
                    {
                        //Sweep through grades if we have any cyberadept improvements that need reassignment
                        if (lstCyberadeptSweepGrades.Count > 0)
                        {
                            foreach (Improvement objCyberadeptImprovement in lstCyberadeptSweepGrades)
                            {
                                InitiationGrade objBestGradeMatch = null;
                                foreach (InitiationGrade objInitiationGrade in InitiationGrades)
                                {
                                    if (!objInitiationGrade.Technomancer
                                        || (objInitiationGrade.Grade + 1) / 2 > objCyberadeptImprovement.Value
                                        || Metamagics.Any(x => x.Grade == objInitiationGrade.Grade)
                                        || lstCyberadeptSweepGrades.All(x => x.ImproveSource != Improvement.ImprovementSource.CyberadeptDaemon
                                                                             || x.SourceName != objInitiationGrade.InternalId))
                                        continue;
                                    if (objBestGradeMatch == null || objBestGradeMatch.Grade > objInitiationGrade.Grade)
                                        objBestGradeMatch = objInitiationGrade;
                                }

                                if (objBestGradeMatch != null)
                                {
                                    objCyberadeptImprovement.ImproveSource = Improvement.ImprovementSource.CyberadeptDaemon;
                                    objCyberadeptImprovement.SourceName = objBestGradeMatch.InternalId;
                                }
                                else
                                    _lstImprovements.Remove(objCyberadeptImprovement);
                            }
                        }

                        //Timekeeper.Finish("load_char_cyberadeptfix");
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_mentorspiritfix", loadActivity))
                    {
                        Quality objMentorQuality = Qualities.FirstOrDefault(q => q.Name == "Mentor Spirit");
                        if (objMentorQuality != null)
                        {
                            // This character doesn't have any improvements tied to a cached Mentor Spirit value, so re-apply the improvement that adds the Mentor spirit
                            if (!Improvements.Any(imp =>
                                imp.ImproveType == Improvement.ImprovementType.MentorSpirit &&
                                !string.IsNullOrEmpty(imp.ImprovedName)))
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

                        //Timekeeper.Finish("load_char_mentorspiritfix");
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_flechettefix", loadActivity))
                    {
                        //Fixes an issue where existing weapons could have been loaded with non-flechette ammunition
                        if (LastSavedVersion <= new Version(5, 212, 78))
                        {
                            foreach (Weapon objWeapon in Weapons.GetAllDescendants(x => x.Children))
                                objWeapon.DoFlechetteFix();
                            foreach (Vehicle objVehicle in Vehicles)
                            {
                                foreach (Weapon objWeapon in objVehicle.Weapons.GetAllDescendants(x => x.Children))
                                    objWeapon.DoFlechetteFix();
                                foreach (WeaponMount objWeaponMount in objVehicle.WeaponMounts)
                                {
                                    foreach (Weapon objWeapon in objWeaponMount.Weapons.GetAllDescendants(x => x.Children))
                                        objWeapon.DoFlechetteFix();

                                    foreach (VehicleMod objMod in objWeaponMount.Mods)
                                    {
                                        foreach (Weapon objWeapon in objMod.Weapons.GetAllDescendants(x => x.Children))
                                            objWeapon.DoFlechetteFix();
                                    }
                                }

                                foreach (VehicleMod objMod in objVehicle.Mods)
                                {
                                    foreach (Weapon objWeapon in objMod.Weapons.GetAllDescendants(x => x.Children))
                                        objWeapon.DoFlechetteFix();
                                }
                            }
                        }

                        //Timekeeper.Finish("load_char_flechettefix");
                    }

                    //Plugins
                    using (_ = Timekeeper.StartSyncron("load_plugins", loadActivity))
                    {
                        foreach (var plugin in Program.PluginLoader.MyActivePlugins)
                        {
                            foreach (XmlNode objXmlPlugin in objXmlCharacter.SelectNodes("plugins/" + plugin.GetPluginAssembly().GetName().Name))
                            {
                                plugin.LoadFileElement(this, objXmlPlugin.InnerText);
                            }
                        }

                        //Timekeeper.Finish("load_plugins");
                    }

                    // Refresh certain improvements
                    using (_ = Timekeeper.StartSyncron("load_char_improvementrefreshers1", loadActivity))
                    {
                        frmLoadingForm.DoThreadSafe(() => frmLoadingForm?.PerformStep(LanguageManager.GetString("String_GeneratedImprovements")));
                        IsLoading = false;
                        // Refresh permanent attribute changes due to essence loss
                        RefreshEssenceLossImprovements();
                        // Refresh dicepool modifiers due to filled condition monitor boxes
                        RefreshWoundPenalties();
                        // Refresh encumbrance penalties
                        RefreshEncumbrance();
                        // Curb Mystic Adept power points if the values that were loaded in would be illegal
                        if (MysticAdeptPowerPoints > 0)
                        {
                            int intMAGTotalValue = MAG.TotalValue;
                            if (MysticAdeptPowerPoints > intMAGTotalValue)
                                MysticAdeptPowerPoints = intMAGTotalValue;
                        }

                        if (!InitiationEnabled || !AddInitiationsAllowed)
                            ClearInitiations();
                        foreach (Action funcToCall in PostLoadMethods)
                            funcToCall.Invoke();
                        PostLoadMethods.Clear();
                        //Timekeeper.Finish("load_char_improvementrefreshers");
                    }

                    //// If the character had old Qualities that were converted, immediately save the file so they are in the new format.
                    //      if (blnHasOldQualities)
                    //      {
                    //    Timekeeper.Start("load_char_resav");  //Lets not silently save file on load?


                    //    Save();
                    //    Timekeeper.Finish("load_char_resav");


                    //}
                    loadActivity.SetSuccess(true);
                }
                catch (Exception e)
                {
                    loadActivity.SetSuccess(false);
                    Log.Error(e);
                    throw;
                }
            }

            return true;
        }

#if DEBUG
        /// <summary>
        /// Print this character information to a MemoryStream. This creates only the character object itself, not any of the opening or closing XmlDocument items.
        /// This can be used to write multiple characters to a single XmlDocument.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write to.</param>
        /// <param name="objCulture">Culture in which to print.</param>
        /// <param name="strLanguageToPrint">Language in which to print.</param>
        /// <param name="objStream">MemoryStream to use.</param>
        public void PrintToStream(MemoryStream objStream, XmlTextWriter objWriter, CultureInfo objCulture = null,
            string strLanguageToPrint = "")
#else
        /// <summary>
        /// Print this character information to a MemoryStream. This creates only the character object itself, not any of the opening or closing XmlDocument items.
        /// This can be used to write multiple characters to a single XmlDocument.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write to.</param>
        /// <param name="objCulture">Culture in which to print.</param>
        /// <param name="strLanguageToPrint">Language in which to print.</param>
        public void PrintToStream(XmlTextWriter objWriter, CultureInfo objCulture = null, string strLanguageToPrint = "")
#endif
        {
            // This line left in for debugging. Write the output to a fixed file name.
            //FileStream objStream = new FileStream("D:\\temp\\print.xml", FileMode.Create, FileAccess.Write, FileShare.ReadWrite);//(_strFileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);

            if (objWriter == null)
                throw new ArgumentNullException(nameof(objWriter));
            if (objCulture == null)
                objCulture = GlobalOptions.CultureInfo;
            if (string.IsNullOrEmpty(strLanguageToPrint))
                strLanguageToPrint = GlobalOptions.Language;
            // <character>
            objWriter.WriteStartElement("character");

            // <settings />
            objWriter.WriteElementString("settings", CharacterOptionsKey);
            // <buildmethod />
            objWriter.WriteElementString("buildmethod", Options.BuildMethod.ToString());
            // <imageformat />
            objWriter.WriteElementString("imageformat", GlobalOptions.SavedImageQuality == int.MaxValue ? "png" : "jpeg");
            // <metatype />
            objWriter.WriteElementString("metatype", DisplayMetatype(strLanguageToPrint));
            // <metatype_english />
            objWriter.WriteElementString("metatype_english", Metatype);
            // <metatype_guid />
            objWriter.WriteElementString("metatype_guid", MetatypeGuid.ToString("D", GlobalOptions.InvariantCultureInfo));
            // <metavariant />
            objWriter.WriteElementString("metavariant", DisplayMetavariant(strLanguageToPrint));
            // <metavariant_english />
            objWriter.WriteElementString("metavariant_english", Metavariant);
            // <metavariant_guid />
            objWriter.WriteElementString("metavariant_guid", MetavariantGuid.ToString("D", GlobalOptions.InvariantCultureInfo));
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
            foreach(string strSkill in PriorityBonusSkillList)
            {
                objWriter.WriteElementString("priorityskill", strSkill);
            }

            // </priorityskills>
            objWriter.WriteEndElement();

            // <handedness />
            if(Ambidextrous)
            {
                objWriter.WriteElementString("primaryarm",
                    LanguageManager.GetString("String_Ambidextrous", strLanguageToPrint));
            }
            else if(PrimaryArm == "Left")
            {
                objWriter.WriteElementString("primaryarm",
                    LanguageManager.GetString("String_Improvement_SideLeft", strLanguageToPrint));
            }
            else
            {
                objWriter.WriteElementString("primaryarm",
                    LanguageManager.GetString("String_Improvement_SideRight", strLanguageToPrint));
            }

            // If the character does not have a name, call them Unnamed Character. This prevents a transformed document from having a self-terminated title tag which causes browser to not rendering anything.
            // <name />
            objWriter.WriteElementString("name",
                !string.IsNullOrEmpty(Name)
                    ? Name
                    : LanguageManager.GetString("String_UnnamedCharacter", strLanguageToPrint));

            PrintMugshots(objWriter);

            // <sex />
            objWriter.WriteElementString("sex", TranslateExtra(ReverseTranslateExtra(Sex), strLanguageToPrint));
            // <age />
            objWriter.WriteElementString("age", TranslateExtra(ReverseTranslateExtra(Age), strLanguageToPrint));
            // <eyes />
            objWriter.WriteElementString("eyes", TranslateExtra(ReverseTranslateExtra(Eyes), strLanguageToPrint));
            // <height />
            objWriter.WriteElementString("height", TranslateExtra(ReverseTranslateExtra(Height), strLanguageToPrint));
            // <weight />
            objWriter.WriteElementString("weight", TranslateExtra(ReverseTranslateExtra(Weight), strLanguageToPrint));
            // <skin />
            objWriter.WriteElementString("skin", TranslateExtra(ReverseTranslateExtra(Skin), strLanguageToPrint));
            // <hair />
            objWriter.WriteElementString("hair", TranslateExtra(ReverseTranslateExtra(Hair), strLanguageToPrint));
            // <description />
            objWriter.WriteElementString("description", Description.RtfToHtml());
            // <background />
            objWriter.WriteElementString("background", Background.RtfToHtml());
            // <concept />
            objWriter.WriteElementString("concept", Concept.RtfToHtml());
            // <notes />
            objWriter.WriteElementString("notes", Notes.RtfToHtml());
            // <alias />
            objWriter.WriteElementString("alias", Alias);
            // <playername />
            objWriter.WriteElementString("playername", PlayerName);
            // <gamenotes />
            objWriter.WriteElementString("gamenotes", GameNotes.RtfToHtml());

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
            objWriter.WriteElementString("spelllimit", FreeSpells.ToString(objCulture));
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
            // <astralreputation />
            objWriter.WriteElementString("astralreputation", AstralReputation.ToString(objCulture));
            // <totalastralreputation />
            objWriter.WriteElementString("totalastralreputation", TotalAstralReputation.ToString(objCulture));
            // <wildreputation />
            objWriter.WriteElementString("wildreputation", WildReputation.ToString(objCulture));
            // <totalwildreputation />
            objWriter.WriteElementString("totalwildreputation", TotalWildReputation.ToString(objCulture));
            // <created />
            objWriter.WriteElementString("created", Created.ToString(GlobalOptions.InvariantCultureInfo));
            // <nuyen />
            objWriter.WriteElementString("nuyen", Nuyen.ToString(Options.NuyenFormat, objCulture));
            // <adept />
            objWriter.WriteElementString("adept", AdeptEnabled.ToString(GlobalOptions.InvariantCultureInfo));
            // <magician />
            objWriter.WriteElementString("magician", MagicianEnabled.ToString(GlobalOptions.InvariantCultureInfo));
            // <technomancer />
            objWriter.WriteElementString("technomancer", TechnomancerEnabled.ToString(GlobalOptions.InvariantCultureInfo));
            // <ai />
            objWriter.WriteElementString("ai", AdvancedProgramsEnabled.ToString(GlobalOptions.InvariantCultureInfo));
            // <cyberwaredisabled />
            objWriter.WriteElementString("cyberwaredisabled", CyberwareDisabled.ToString(GlobalOptions.InvariantCultureInfo));
            // <critter />
            objWriter.WriteElementString("critter", CritterEnabled.ToString(GlobalOptions.InvariantCultureInfo));

            objWriter.WriteElementString("totaless", Essence().ToString(Options.EssenceFormat, objCulture));

            // <tradition />
            if(MagicTradition.Type != TraditionType.None)
            {
                MagicTradition.Print(objWriter, objCulture, strLanguageToPrint);
            }

            // <attributes>
            objWriter.WriteStartElement("attributes");
            AttributeSection.Print(objWriter, objCulture, strLanguageToPrint);

            // </attributes>
            objWriter.WriteEndElement();
            // <armor />
            objWriter.WriteElementString("armor", (TotalArmorRating).ToString(objCulture));
            // <firearmor />
            objWriter.WriteElementString("firearmor", (TotalFireArmorRating).ToString(objCulture));
            // <coldarmor />
            objWriter.WriteElementString("coldarmor", (TotalColdArmorRating).ToString(objCulture));
            // <electricityarmor />
            objWriter.WriteElementString("electricityarmor", (TotalElectricityArmorRating).ToString(objCulture));
            // <acidarmor />
            objWriter.WriteElementString("acidarmor", (TotalAcidArmorRating).ToString(objCulture));
            // <fallingarmor />
            objWriter.WriteElementString("fallingarmor", (TotalFallingArmorRating).ToString(objCulture));

            int intDamageResistanceDice = ImprovementManager.ValueOf(this, Improvement.ImprovementType.DamageResistance);
            // <armordicestun />
            objWriter.WriteElementString("armordicestun",
                (BOD.TotalValue + intDamageResistanceDice + TotalArmorRating).ToString(objCulture));
            // <firearmordicestun />
            objWriter.WriteElementString("firearmordicestun",
                (BOD.TotalValue + intDamageResistanceDice + TotalFireArmorRating).ToString(objCulture));
            // <coldarmordicestun />
            objWriter.WriteElementString("coldarmordicestun",
                (BOD.TotalValue + intDamageResistanceDice + TotalColdArmorRating).ToString(objCulture));
            // <electricityarmordicestun />
            objWriter.WriteElementString("electricityarmordicestun",
                (BOD.TotalValue + intDamageResistanceDice + TotalElectricityArmorRating).ToString(objCulture));
            // <acidarmordicestun />
            objWriter.WriteElementString("acidarmordicestun",
                (BOD.TotalValue + intDamageResistanceDice + TotalAcidArmorRating).ToString(objCulture));
            // <fallingarmordicestun />
            objWriter.WriteElementString("fallingarmordicestun",
                (BOD.TotalValue + intDamageResistanceDice + TotalFallingArmorRating).ToString(objCulture));
            // <armordicephysical />
            objWriter.WriteElementString("armordicephysical",
                (BOD.TotalValue + intDamageResistanceDice + TotalArmorRating).ToString(objCulture));
            // <firearmordicephysical />
            objWriter.WriteElementString("firearmordicephysical",
                (BOD.TotalValue + intDamageResistanceDice + TotalFireArmorRating).ToString(objCulture));
            // <coldarmordicephysical />
            objWriter.WriteElementString("coldarmordicephysical",
                (BOD.TotalValue + intDamageResistanceDice + TotalColdArmorRating).ToString(objCulture));
            // <electricityarmordicephysical />
            objWriter.WriteElementString("electricityarmordicephysical",
                (BOD.TotalValue + intDamageResistanceDice + TotalElectricityArmorRating).ToString(objCulture));
            // <acidarmordicephysical />
            objWriter.WriteElementString("acidarmordicephysical",
                (BOD.TotalValue + intDamageResistanceDice + TotalAcidArmorRating).ToString(objCulture));
            // <fallingarmordicephysical />
            objWriter.WriteElementString("fallingarmordicephysical",
                (BOD.TotalValue + intDamageResistanceDice + TotalFallingArmorRating).ToString(objCulture));

            bool blnIsAI = IsAI;
            bool blnPhysicalTrackIsCore = blnIsAI && !(HomeNode is Vehicle);
            // Condition Monitors.
            // <physicalcm />
            int intPhysicalCM = PhysicalCM;
            objWriter.WriteElementString("physicalcm", intPhysicalCM.ToString(objCulture));
            objWriter.WriteElementString("physicalcmiscorecm",
                blnPhysicalTrackIsCore.ToString(GlobalOptions.InvariantCultureInfo));
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
            objWriter.WriteElementString("physicalcmthresholdoffset",
                Math.Min(PhysicalCMThresholdOffset, intPhysicalCM).ToString(objCulture));
            // <cmthresholdoffset>
            objWriter.WriteElementString("stuncmthresholdoffset",
                Math.Min(StunCMThresholdOffset, intStunCM).ToString(objCulture));
            // <cmoverflow>
            objWriter.WriteElementString("cmoverflow", CMOverflow.ToString(objCulture));

            // Calculate Initiatives.
            // Initiative.
            objWriter.WriteElementString("init", GetInitiative(objCulture, strLanguageToPrint));
            objWriter.WriteElementString("initdice", InitiativeDice.ToString(objCulture));
            objWriter.WriteElementString("initvalue", InitiativeValue.ToString(objCulture));
            objWriter.WriteElementString("initbonus",
                Math.Max(ImprovementManager.ValueOf(this, Improvement.ImprovementType.Initiative), 0)
                    .ToString(objCulture));

            // Astral Initiative.
            if(MAGEnabled)
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
            objWriter.WriteElementString("magenabled", MAGEnabled.ToString(GlobalOptions.InvariantCultureInfo));
            // <initiategrade />
            objWriter.WriteElementString("initiategrade", InitiateGrade.ToString(objCulture));
            // <resenabled />
            objWriter.WriteElementString("resenabled", RESEnabled.ToString(GlobalOptions.InvariantCultureInfo));
            // <submersiongrade />
            objWriter.WriteElementString("submersiongrade", SubmersionGrade.ToString(objCulture));
            // <depenabled />
            objWriter.WriteElementString("depenabled", DEPEnabled.ToString(GlobalOptions.InvariantCultureInfo));
            // <groupmember />
            objWriter.WriteElementString("groupmember", GroupMember.ToString(GlobalOptions.InvariantCultureInfo));
            // <groupname />
            objWriter.WriteElementString("groupname", GroupName);
            // <groupnotes />
            objWriter.WriteElementString("groupnotes", GroupNotes);
            // <surprise />
            objWriter.WriteElementString("surprise", Surprise.ToString(objCulture));
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
            objWriter.WriteElementString("toxininhalationresist",
                ToxinInhalationResist(strLanguageToPrint, objCulture));
            // <toxininjectionresist />
            objWriter.WriteElementString("toxininjectionresist", ToxinInjectionResist(strLanguageToPrint, objCulture));
            // <pathogencontactresist />
            objWriter.WriteElementString("pathogencontactresist",
                PathogenContactResist(strLanguageToPrint, objCulture));
            // <pathogeningestionresist />
            objWriter.WriteElementString("pathogeningestionresist",
                PathogenIngestionResist(strLanguageToPrint, objCulture));
            // <pathogeninhalationresist />
            objWriter.WriteElementString("pathogeninhalationresist",
                PathogenInhalationResist(strLanguageToPrint, objCulture));
            // <pathogeninjectionresist />
            objWriter.WriteElementString("pathogeninjectionresist",
                PathogenInjectionResist(strLanguageToPrint, objCulture));
            // <physiologicaladdictionresistfirsttime />
            objWriter.WriteElementString("physiologicaladdictionresistfirsttime",
                PhysiologicalAddictionResistFirstTime.ToString(objCulture));
            // <physiologicaladdictionresistalreadyaddicted />
            objWriter.WriteElementString("physiologicaladdictionresistalreadyaddicted",
                PhysiologicalAddictionResistAlreadyAddicted.ToString(objCulture));
            // <psychologicaladdictionresistfirsttime />
            objWriter.WriteElementString("psychologicaladdictionresistfirsttime",
                PsychologicalAddictionResistFirstTime.ToString(objCulture));
            // <psychologicaladdictionresistalreadyaddicted />
            objWriter.WriteElementString("psychologicaladdictionresistalreadyaddicted",
                PsychologicalAddictionResistAlreadyAddicted.ToString(objCulture));
            // <physicalcmnaturalrecovery />
            objWriter.WriteElementString("physicalcmnaturalrecovery", PhysicalCMNaturalRecovery.ToString(objCulture));
            // <stuncmnaturalrecovery />
            objWriter.WriteElementString("stuncmnaturalrecovery", StunCMNaturalRecovery.ToString(objCulture));

            // Spell Resistances
            //Indirect Dodge
            objWriter.WriteElementString("indirectdefenseresist", SpellDefenseIndirectDodge.ToString(objCulture));
            //Direct Soak - Mana
            objWriter.WriteElementString("directmanaresist", SpellDefenseDirectSoakMana.ToString(objCulture));
            //Direct Soak - Physical
            objWriter.WriteElementString("directphysicalresist", SpellDefenseDirectSoakPhysical.ToString(objCulture));
            //Detection Spells
            objWriter.WriteElementString("detectionspellresist", SpellDefenseDetection.ToString(objCulture));
            //Decrease Attribute - BOD
            objWriter.WriteElementString("decreasebodresist", SpellDefenseDecreaseBOD.ToString(objCulture));
            //Decrease Attribute - AGI
            objWriter.WriteElementString("decreaseagiresist", SpellDefenseDecreaseAGI.ToString(objCulture));
            //Decrease Attribute - REA
            objWriter.WriteElementString("decreaserearesist", SpellDefenseDecreaseREA.ToString(objCulture));
            //Decrease Attribute - STR
            objWriter.WriteElementString("decreasestrresist", SpellDefenseDecreaseSTR.ToString(objCulture));
            //Decrease Attribute - CHA
            objWriter.WriteElementString("decreasecharesist", SpellDefenseDecreaseCHA.ToString(objCulture));
            //Decrease Attribute - INT
            objWriter.WriteElementString("decreaseintresist", SpellDefenseDecreaseINT.ToString(objCulture));
            //Decrease Attribute - LOG
            objWriter.WriteElementString("decreaselogresist", SpellDefenseDecreaseLOG.ToString(objCulture));
            //Decrease Attribute - WIL
            objWriter.WriteElementString("decreasewilresist", SpellDefenseDecreaseWIL.ToString(objCulture));
            //Illusion - Mana
            objWriter.WriteElementString("illusionmanaresist", SpellDefenseIllusionMana.ToString(objCulture));
            //Illusion - Physical
            objWriter.WriteElementString("illusionphysicalresist", SpellDefenseIllusionPhysical.ToString(objCulture));
            //Manipulation - Mental
            objWriter.WriteElementString("manipulationmentalresist",
                SpellDefenseManipulationMental.ToString(objCulture));
            //Manipulation - Physical
            objWriter.WriteElementString("manipulationphysicalresist",
                SpellDefenseManipulationPhysical.ToString(objCulture));

            // <skills>
            objWriter.WriteStartElement("skills");
            SkillsSection.Print(objWriter, objCulture, strLanguageToPrint);
            objWriter.WriteEndElement();

            // <contacts>
            objWriter.WriteStartElement("contacts");
            foreach(Contact objContact in Contacts)
            {
                objContact.Print(objWriter, objCulture, strLanguageToPrint);
            }

            // </contacts>
            objWriter.WriteEndElement();

            // <limitmodifiersphys>
            objWriter.WriteStartElement("limitmodifiersphys");
            foreach(LimitModifier objLimitModifier in LimitModifiers)
            {
                if (objLimitModifier.Limit == "Physical")
                    objLimitModifier.Print(objWriter, objCulture, strLanguageToPrint);
            }

            // Populate Limit Modifiers from Improvements
            foreach(Improvement objImprovement in Improvements.Where(objImprovement =>
               (objImprovement.ImproveType == Improvement.ImprovementType.LimitModifier &&
                objImprovement.ImprovedName == "Physical" && objImprovement.Enabled)))
            {
                string strName = GetObjectName(objImprovement, strLanguageToPrint);
                if(strName == objImprovement.SourceName)
                    strName = objImprovement.UniqueName;
                strName += LanguageManager.GetString("String_Colon", strLanguageToPrint) + LanguageManager.GetString("String_Space", strLanguageToPrint);
                if(objImprovement.Value > 0)
                    strName += '+';
                strName += objImprovement.Value.ToString(objCulture);

                if(!string.IsNullOrEmpty(objImprovement.Condition))
                    strName += ',' + LanguageManager.GetString("String_Space", strLanguageToPrint) + objImprovement.Condition;

                objWriter.WriteStartElement("limitmodifier");
                objWriter.WriteElementString("name", strName);
                if(Options.PrintNotes)
                    objWriter.WriteElementString("notes", objImprovement.Notes);
                objWriter.WriteEndElement();
            }

            // </limitmodifiersphys>
            objWriter.WriteEndElement();

            // <limitmodifiersment>
            objWriter.WriteStartElement("limitmodifiersment");
            foreach(LimitModifier objLimitModifier in LimitModifiers)
            {
                if (objLimitModifier.Limit == "Mental")
                    objLimitModifier.Print(objWriter, objCulture, strLanguageToPrint);
            }

            // Populate Limit Modifiers from Improvements
            foreach(Improvement objImprovement in Improvements.Where(objImprovement =>
               (objImprovement.ImproveType == Improvement.ImprovementType.LimitModifier &&
                objImprovement.ImprovedName == "Mental" && objImprovement.Enabled)))
            {
                string strName = GetObjectName(objImprovement, strLanguageToPrint);
                if(strName == objImprovement.SourceName)
                    strName = objImprovement.UniqueName;
                strName += LanguageManager.GetString("String_Colon", strLanguageToPrint) + LanguageManager.GetString("String_Space", strLanguageToPrint);
                if(objImprovement.Value > 0)
                    strName += '+';
                strName += objImprovement.Value.ToString(objCulture);

                if(!string.IsNullOrEmpty(objImprovement.Condition))
                    strName += ',' + LanguageManager.GetString("String_Space", strLanguageToPrint) + objImprovement.Condition;

                objWriter.WriteStartElement("limitmodifier");
                objWriter.WriteElementString("name", strName);
                if(Options.PrintNotes)
                    objWriter.WriteElementString("notes", objImprovement.Notes);
                objWriter.WriteEndElement();
            }

            // </limitmodifiersment>
            objWriter.WriteEndElement();

            // <limitmodifierssoc>
            objWriter.WriteStartElement("limitmodifierssoc");
            foreach(LimitModifier objLimitModifier in LimitModifiers)
            {
                if (objLimitModifier.Limit == "Social")
                    objLimitModifier.Print(objWriter, objCulture, strLanguageToPrint);
            }

            // Populate Limit Modifiers from Improvements
            foreach(Improvement objImprovement in Improvements.Where(objImprovement =>
               (objImprovement.ImproveType == Improvement.ImprovementType.LimitModifier &&
                objImprovement.ImprovedName == "Social" && objImprovement.Enabled)))
            {
                string strName = GetObjectName(objImprovement, strLanguageToPrint);
                if(strName == objImprovement.SourceName)
                    strName = objImprovement.UniqueName;
                strName += LanguageManager.GetString("String_Colon", strLanguageToPrint) + LanguageManager.GetString("String_Space", strLanguageToPrint);
                if(objImprovement.Value > 0)
                    strName += '+';
                strName += objImprovement.Value.ToString(objCulture);

                if(!string.IsNullOrEmpty(objImprovement.Condition))
                    strName += ',' + LanguageManager.GetString("String_Space", strLanguageToPrint) + objImprovement.Condition;

                objWriter.WriteStartElement("limitmodifier");
                objWriter.WriteElementString("name", strName);
                if(Options.PrintNotes)
                    objWriter.WriteElementString("notes", objImprovement.Notes);
                objWriter.WriteEndElement();
            }

            // </limitmodifierssoc>
            objWriter.WriteEndElement();

            // <spells>
            objWriter.WriteStartElement("spells");
            foreach(Spell objSpell in Spells)
            {
                objSpell.Print(objWriter, objCulture, strLanguageToPrint);
            }

            // </spells>
            objWriter.WriteEndElement();

            // <powers>
            objWriter.WriteStartElement("powers");
            foreach(Power objPower in Powers)
            {
                objPower.Print(objWriter, objCulture, strLanguageToPrint);
            }

            // </powers>
            objWriter.WriteEndElement();

            // <spirits>
            objWriter.WriteStartElement("spirits");
            foreach(Spirit objSpirit in Spirits)
            {
                objSpirit.Print(objWriter, objCulture, strLanguageToPrint);
            }

            // </spirits>
            objWriter.WriteEndElement();

            // <complexforms>
            objWriter.WriteStartElement("complexforms");
            foreach(ComplexForm objComplexForm in ComplexForms)
            {
                objComplexForm.Print(objWriter, strLanguageToPrint);
            }

            // </complexforms>
            objWriter.WriteEndElement();

            // <aiprograms>
            objWriter.WriteStartElement("aiprograms");
            foreach(AIProgram objProgram in AIPrograms)
            {
                objProgram.Print(objWriter, strLanguageToPrint);
            }

            // </aiprograms>
            objWriter.WriteEndElement();

            // <martialarts>
            objWriter.WriteStartElement("martialarts");
            foreach(MartialArt objMartialArt in MartialArts)
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
            foreach(Armor objArmor in Armor)
            {
                objArmor.Print(objWriter, objCulture, strLanguageToPrint);
            }

            // </armors>
            objWriter.WriteEndElement();

            // <weapons>
            objWriter.WriteStartElement("weapons");
            foreach(Weapon objWeapon in Weapons)
            {
                objWeapon.Print(objWriter, objCulture, strLanguageToPrint);
            }

            // </weapons>
            objWriter.WriteEndElement();

            // <cyberwares>
            objWriter.WriteStartElement("cyberwares");
            foreach(Cyberware objCyberware in Cyberware)
            {
                objCyberware.Print(objWriter, objCulture, strLanguageToPrint);
            }

            // </cyberwares>
            objWriter.WriteEndElement();

            // <qualities>
            // Multiple instances of the same quality are combined into just one entry with a number next to it (e.g. 6 discrete entries of "Focused Concentration" become "Focused Concentration 6")
            Dictionary<string, int> strQualitiesToPrint = new Dictionary<string, int>(Qualities.Count);
            foreach(Quality objQuality in Qualities)
            {
                string strKey = objQuality.SourceIDString + '|' + objQuality.SourceName + '|' + objQuality.Extra;
                if(strQualitiesToPrint.ContainsKey(strKey))
                {
                    strQualitiesToPrint[strKey] += 1;
                }
                else
                {
                    strQualitiesToPrint.Add(strKey, 1);
                }
            }

            objWriter.WriteStartElement("qualities");
            foreach(Quality objQuality in Qualities)
            {
                string strKey = objQuality.SourceIDString + '|' + objQuality.SourceName + '|' + objQuality.Extra;
                if(strQualitiesToPrint.TryGetValue(strKey, out int intLoopRating))
                {
                    objQuality.Print(objWriter, intLoopRating, objCulture, strLanguageToPrint);
                    strQualitiesToPrint.Remove(strKey);
                }
            }

            // </qualities>
            objWriter.WriteEndElement();

            // <lifestyles>
            objWriter.WriteStartElement("lifestyles");
            foreach(Lifestyle objLifestyle in Lifestyles)
            {
                objLifestyle.Print(objWriter, objCulture, strLanguageToPrint);
            }

            // </lifestyles>
            objWriter.WriteEndElement();

            // <gears>
            objWriter.WriteStartElement("gears");
            foreach(Gear objGear in Gear)
            {
                objGear.Print(objWriter, objCulture, strLanguageToPrint);
            }

            // </gears>
            objWriter.WriteEndElement();

            // <drugs>
            objWriter.WriteStartElement("drugs");
            foreach(Drug objDrug in Drugs)
            {
                objDrug.Print(objWriter, objCulture, strLanguageToPrint);
            }

            // </drugs>
            objWriter.WriteEndElement();

            // <vehicles>
            objWriter.WriteStartElement("vehicles");
            foreach(Vehicle objVehicle in Vehicles)
            {
                objVehicle.Print(objWriter, objCulture, strLanguageToPrint);
            }

            // </vehicles>
            objWriter.WriteEndElement();

            // <initiationgrades>
            objWriter.WriteStartElement("initiationgrades");
            foreach(InitiationGrade objGrade in InitiationGrades)
            {
                objGrade.Print(objWriter, objCulture);

                //TODO: Probably better to integrate this into the main print method, but eh.
                // <metamagics>
                objWriter.WriteStartElement("metamagics");
                foreach(Metamagic objMetamagic in Metamagics)
                {
                    if (objMetamagic.Grade == objGrade.Grade)
                        objMetamagic.Print(objWriter, objCulture, strLanguageToPrint);
                }

                // </metamagics>
                objWriter.WriteEndElement();

                // <arts>
                objWriter.WriteStartElement("arts");
                foreach(Art objArt in Arts)
                {
                    if (objArt.Grade == objGrade.Grade)
                        objArt.Print(objWriter, strLanguageToPrint);
                }

                // </arts>
                objWriter.WriteEndElement();

                // <enhancements>
                objWriter.WriteStartElement("enhancements");
                foreach(Enhancement objEnhancement in Enhancements)
                {
                    if (objEnhancement.Grade == objGrade.Grade)
                        objEnhancement.Print(objWriter, strLanguageToPrint);
                }

                // </enhancements>
                objWriter.WriteEndElement();
            }

            // </initiationgrade>
            objWriter.WriteEndElement();

            // <metamagics>
            objWriter.WriteStartElement("metamagics");
            foreach(Metamagic objMetamagic in Metamagics)
            {
                objMetamagic.Print(objWriter, objCulture, strLanguageToPrint);
            }

            // </metamagics>
            objWriter.WriteEndElement();

            // <arts>
            objWriter.WriteStartElement("arts");
            foreach(Art objArt in Arts)
            {
                objArt.Print(objWriter, strLanguageToPrint);
            }

            // </arts>
            objWriter.WriteEndElement();

            // <enhancements>
            objWriter.WriteStartElement("enhancements");
            foreach(Enhancement objEnhancement in Enhancements)
            {
                objEnhancement.Print(objWriter, strLanguageToPrint);
            }

            // </enhancements>
            objWriter.WriteEndElement();

            // <critterpowers>
            objWriter.WriteStartElement("critterpowers");
            foreach(CritterPower objPower in CritterPowers)
            {
                objPower.Print(objWriter, strLanguageToPrint);
            }

            // </critterpowers>
            objWriter.WriteEndElement();

            // <calendar>
            objWriter.WriteStartElement("calendar");
            //Calendar.Sort();
            foreach(CalendarWeek objWeek in Calendar)
                objWeek.Print(objWriter, objCulture, Options.PrintNotes);
            // </expenses>
            objWriter.WriteEndElement();

            // Print the Expense Log Entries if the option is enabled.
            if(Options.PrintExpenses)
            {
                // <expenses>
                objWriter.WriteStartElement("expenses");
                foreach(ExpenseLogEntry objExpense in ExpenseEntries.Reverse())
                    objExpense.Print(objWriter, objCulture, strLanguageToPrint);
                // </expenses>
                objWriter.WriteEndElement();
            }

            // </character>
            objWriter.WriteEndElement();
        }
        #endregion

        private bool _blnDisposing;
        /// <summary>
        /// Remove stray event handlers and clear all info used by this character
        /// </summary>
        public void Dispose()
        {
            if (_blnDisposing)
                return;
            if (!Utils.IsUnitTest && (Program.MainForm.OpenCharacters.Contains(this) || Program.MainForm.OpenCharacters.Any(x => x.LinkedCharacters.Contains(this))))
                return; // Do not actually dispose any characters who are still in the open characters list or required by a character who is

            _blnDisposing = true;
            _lstLinkedCharacters.Clear(); // Clear this list because it relates to Contacts and Spirits disposal
            foreach (Image imgMugshot in _lstMugshots)
                imgMugshot.Dispose();
            foreach (Contact objContact in _lstContacts)
                objContact.Dispose();
            foreach (Spirit objSpirit in _lstSpirits)
                objSpirit.Dispose();
            ImprovementManager.ClearCachedValues(this);
            AttributeSection.Dispose();
            _blnDisposing = false;
        }

        /// <summary>
        /// Reset all of the Character information and start from scratch.
        /// </summary>
        public void ResetCharacter()
        {
            _intFreeSpells = 0;
            _intCFPLimit = 0;
            _intAINormalProgramLimit = 0;
            _intAIAdvancedProgramLimit = 0;
            _intCachedRedlinerBonus = 0;
            _intCachedContactPoints = 0;
            _intCurrentCounterspellingDice = 0;
            _intCachedInitiationEnabled = -1;
            _decCachedBiowareEssence = decimal.MinValue;
            _decCachedCyberwareEssence = decimal.MinValue;
            ResetCachedEssence();
            _decCachedEssenceHole = decimal.MinValue;
            _decCachedPowerPointsUsed = decimal.MinValue;
            _decCachedPrototypeTranshumanEssenceUsed = decimal.MinValue;
            _intContactPointsUsed = 0;
            _intKarma = 0;
            _intSpecial = 0;
            _intTotalSpecial = 0;
            _intAttributes = 0;
            _intTotalAttributes = 0;

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
            _blnInitiationDisabled = false;
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
            _objTradition.UnbindTradition();

            _lstLinkedCharacters.Clear();
            _intMainMugshotIndex = -1;
            foreach (Image imgMugshot in _lstMugshots)
                imgMugshot.Dispose();
            _lstMugshots.Clear();
            foreach (Contact objContact in _lstContacts)
                objContact.Dispose();
            _lstContacts.Clear();
            foreach (Spirit objSpirit in _lstSpirits)
                objSpirit.Dispose();
            _lstSpirits.Clear();
            // Reset all of the Lists.
            // This kills the GC
            ImprovementManager.ClearCachedValues(this);
            _lstImprovements.Clear();
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
            _lstDrugs.Clear();

            SkillsSection.Reset();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Collate and save the character's used sourcebooks. This list is cleared after loading a character to ensure that only the current items are stored.
        /// </summary>
        public void SourceProcess(string strInput)
        {
            if(!_lstSources.Contains(strInput))
            {
                _lstSources.Add(strInput);
            }
        }

        /// <summary>
        /// Retrieve the name of the Object that created an Improvement.
        /// </summary>
        /// <param name="objImprovement">Improvement to check.</param>
        /// <param name="strLanguage">Language in which to fetch name.</param>
        public string GetObjectName(Improvement objImprovement, string strLanguage = "")
        {
            if (objImprovement == null)
                return string.Empty;
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalOptions.Language;
            string strSpace = LanguageManager.GetString("String_Space", strLanguage);
            string strImprovedGuid = objImprovement.SourceName;
            bool wireless = false;

            if (strImprovedGuid.EndsWith("WirelessPair", StringComparison.Ordinal))
            {
                wireless = true;
                strImprovedGuid = strImprovedGuid.Replace("WirelessPair", string.Empty);
            }
            else if (strImprovedGuid.EndsWith("Wireless", StringComparison.Ordinal))
            {
                wireless = true;
                strImprovedGuid = strImprovedGuid.Replace("Wireless", string.Empty);
            }

            switch (objImprovement.ImproveSource)
            {
                case Improvement.ImprovementSource.Bioware:
                case Improvement.ImprovementSource.Cyberware:
                    Cyberware objReturnCyberware = Cyberware.DeepFirstOrDefault(x => x.Children,
                        x => x.InternalId == strImprovedGuid);
                    if (objReturnCyberware != null)
                    {
                        StringBuilder sbdWareReturn = new StringBuilder(objReturnCyberware.DisplayNameShort(strLanguage));
                        if (objReturnCyberware.Parent != null)
                            sbdWareReturn.Append(strSpace).Append('(').Append(objReturnCyberware.Parent.DisplayNameShort(strLanguage)).Append(')');
                        if (wireless)
                            sbdWareReturn.Append(strSpace).Append(LanguageManager.GetString("String_Wireless", strLanguage));
                        return sbdWareReturn.ToString();
                    }

                    foreach(Vehicle objVehicle in Vehicles)
                    {
                        foreach(VehicleMod objVehicleMod in objVehicle.Mods)
                        {
                            objReturnCyberware = objVehicleMod.Cyberware.DeepFirstOrDefault(x => x.Children,
                                x => x.InternalId == objImprovement.SourceName);
                            if(objReturnCyberware != null)
                            {
                                StringBuilder sbdWareReturn = new StringBuilder(objReturnCyberware.DisplayNameShort(strLanguage))
                                    .Append(strSpace).Append('(').Append(objVehicle.DisplayNameShort(strLanguage)).Append(',')
                                    .Append(strSpace).Append(objVehicleMod.DisplayNameShort(strLanguage));
                                if (objReturnCyberware.Parent != null)
                                    sbdWareReturn.Append(',').Append(strSpace).Append(objReturnCyberware.Parent.DisplayNameShort(strLanguage));
                                sbdWareReturn.Append(')');
                                if (wireless)
                                    sbdWareReturn.Append(strSpace).Append(LanguageManager.GetString("String_Wireless", strLanguage));
                                return sbdWareReturn.ToString();
                            }
                        }
                    }

                    break;
                case Improvement.ImprovementSource.Gear:
                    Gear objReturnGear =
                        Gear.DeepFirstOrDefault(x => x.Children, x => x.InternalId == objImprovement.SourceName);
                    if(objReturnGear != null)
                    {
                        StringBuilder sbdGearReturn = new StringBuilder(objReturnGear.DisplayNameShort(strLanguage));
                        if(objReturnGear.Parent != null && objReturnGear.Parent is Gear parent)
                            sbdGearReturn.Append(strSpace).Append('(').Append(parent.DisplayNameShort(strLanguage)).Append(')');
                        if (wireless)
                            sbdGearReturn.Append(strSpace).Append(LanguageManager.GetString("String_Wireless", strLanguage));
                        return sbdGearReturn.ToString();
                    }

                    foreach(Weapon objWeapon in Weapons.DeepWhere(x => x.Children,
                        x => x.WeaponAccessories.Any(y => y.Gear.Count > 0)))
                    {
                        foreach(WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                        {
                            objReturnGear = objAccessory.Gear.DeepFirstOrDefault(x => x.Children,
                                x => x.InternalId == objImprovement.SourceName);
                            if(objReturnGear != null)
                            {
                                StringBuilder sbdGearReturn = new StringBuilder(objReturnGear.DisplayNameShort(strLanguage));
                                if (objReturnGear.Parent != null && objReturnGear.Parent is Gear parent)
                                    sbdGearReturn.Append(strSpace).Append('(').Append(objWeapon.DisplayNameShort(strLanguage)).Append(',')
                                        .Append(strSpace).Append(objAccessory.DisplayNameShort(strLanguage)).Append(',')
                                        .Append(strSpace).Append(parent.DisplayNameShort(strLanguage)).Append(')');
                                else
                                    sbdGearReturn.Append(strSpace).Append('(').Append(objWeapon.DisplayNameShort(strLanguage)).Append(',')
                                        .Append(strSpace).Append(objAccessory.DisplayNameShort(strLanguage)).Append(')');
                                if (wireless)
                                    sbdGearReturn.Append(strSpace).Append(LanguageManager.GetString("String_Wireless", strLanguage));
                                return sbdGearReturn.ToString();
                            }
                        }
                    }

                    foreach(Armor objArmor in Armor)
                    {
                        objReturnGear = objArmor.Gear.DeepFirstOrDefault(x => x.Children,
                            x => x.InternalId == objImprovement.SourceName);
                        if(objReturnGear != null)
                        {
                            StringBuilder sbdGearReturn = new StringBuilder(objReturnGear.DisplayNameShort(strLanguage));
                            if (objReturnGear.Parent != null && objReturnGear.Parent is Gear parent)
                                sbdGearReturn.Append(strSpace).Append('(').Append(objArmor.DisplayNameShort(strLanguage)).Append(',')
                                    .Append(strSpace).Append(parent.DisplayNameShort(strLanguage)).Append(')');
                            else
                                sbdGearReturn.Append(strSpace).Append('(').Append(objArmor.DisplayNameShort(strLanguage)).Append(')');
                            if (wireless)
                                sbdGearReturn.Append(strSpace).Append(LanguageManager.GetString("String_Wireless", strLanguage));
                            return sbdGearReturn.ToString();
                        }
                    }

                    foreach(Cyberware objCyberware in Cyberware.DeepWhere(x => x.Children, x => x.Gear.Count > 0))
                    {
                        objReturnGear = objCyberware.Gear.DeepFirstOrDefault(x => x.Children,
                            x => x.InternalId == objImprovement.SourceName);
                        if(objReturnGear != null)
                        {
                            StringBuilder sbdGearReturn = new StringBuilder(objReturnGear.DisplayNameShort(strLanguage));
                            if (objReturnGear.Parent != null && objReturnGear.Parent is Gear parent)
                                sbdGearReturn.Append(strSpace).Append('(').Append(objCyberware.DisplayNameShort(strLanguage)).Append(',')
                                    .Append(strSpace).Append(parent.DisplayNameShort(strLanguage)).Append(')');
                            else
                                sbdGearReturn.Append(strSpace).Append('(').Append(objCyberware.DisplayNameShort(strLanguage)).Append(')');
                            if (wireless)
                                sbdGearReturn.Append(strSpace).Append(LanguageManager.GetString("String_Wireless", strLanguage));
                            return sbdGearReturn.ToString();
                        }
                    }

                    foreach(Vehicle objVehicle in Vehicles)
                    {
                        objReturnGear = objVehicle.Gear.DeepFirstOrDefault(x => x.Children,
                            x => x.InternalId == objImprovement.SourceName);
                        if(objReturnGear != null)
                        {
                            StringBuilder sbdGearReturn = new StringBuilder(objReturnGear.DisplayNameShort(strLanguage));
                            if (objReturnGear.Parent != null && objReturnGear.Parent is Gear parent)
                                sbdGearReturn.Append(strSpace).Append('(').Append(objVehicle.DisplayNameShort(strLanguage)).Append(',')
                                    .Append(strSpace).Append(parent.DisplayNameShort(strLanguage)).Append(')');
                            else
                                sbdGearReturn.Append(strSpace).Append('(').Append(objVehicle.DisplayNameShort(strLanguage)).Append(')');
                            if (wireless)
                                sbdGearReturn.Append(strSpace).Append(LanguageManager.GetString("String_Wireless", strLanguage));
                            return sbdGearReturn.ToString();
                        }

                        foreach(Weapon objWeapon in objVehicle.Weapons.DeepWhere(x => x.Children,
                            x => x.WeaponAccessories.Any(y => y.Gear.Count > 0)))
                        {
                            foreach(WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                            {
                                objReturnGear = objAccessory.Gear.DeepFirstOrDefault(x => x.Children,
                                    x => x.InternalId == objImprovement.SourceName);
                                if(objReturnGear != null)
                                {
                                    StringBuilder sbdGearReturn = new StringBuilder(objReturnGear.DisplayNameShort(strLanguage));
                                    if (objReturnGear.Parent != null && objReturnGear.Parent is Gear parent)
                                        sbdGearReturn.Append(strSpace).Append('(').Append(objVehicle.DisplayNameShort(strLanguage)).Append(',')
                                            .Append(strSpace).Append(objWeapon.DisplayNameShort(strLanguage)).Append(',')
                                            .Append(strSpace).Append(objAccessory.DisplayNameShort(strLanguage)).Append(',')
                                            .Append(strSpace).Append(parent.DisplayNameShort(strLanguage)).Append(')');
                                    else
                                        sbdGearReturn.Append(strSpace).Append('(').Append(objVehicle.DisplayNameShort(strLanguage)).Append(',')
                                            .Append(strSpace).Append(objWeapon.DisplayNameShort(strLanguage)).Append(',')
                                            .Append(strSpace).Append(objAccessory.DisplayNameShort(strLanguage)).Append(')');
                                    if (wireless)
                                        sbdGearReturn.Append(strSpace).Append(LanguageManager.GetString("String_Wireless", strLanguage));
                                    return sbdGearReturn.ToString();
                                }
                            }
                        }

                        foreach(VehicleMod objVehicleMod in objVehicle.Mods)
                        {
                            foreach(Weapon objWeapon in objVehicleMod.Weapons.DeepWhere(x => x.Children,
                                x => x.WeaponAccessories.Any(y => y.Gear.Count > 0)))
                            {
                                foreach(WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                                {
                                    objReturnGear = objAccessory.Gear.DeepFirstOrDefault(x => x.Children,
                                        x => x.InternalId == objImprovement.SourceName);
                                    if(objReturnGear != null)
                                    {
                                        StringBuilder sbdGearReturn = new StringBuilder(objReturnGear.DisplayNameShort(strLanguage));
                                        if (objReturnGear.Parent != null && objReturnGear.Parent is Gear parent)
                                            sbdGearReturn.Append(strSpace).Append('(').Append(objVehicle.DisplayNameShort(strLanguage)).Append(',')
                                                .Append(strSpace).Append(objVehicleMod.DisplayNameShort(strLanguage)).Append(',')
                                                .Append(strSpace).Append(objWeapon.DisplayNameShort(strLanguage)).Append(',')
                                                .Append(strSpace).Append(objAccessory.DisplayNameShort(strLanguage)).Append(',')
                                                .Append(strSpace).Append(parent.DisplayNameShort(strLanguage)).Append(')');
                                        else
                                            sbdGearReturn.Append(strSpace).Append('(').Append(objVehicle.DisplayNameShort(strLanguage)).Append(',')
                                                .Append(strSpace).Append(objVehicleMod.DisplayNameShort(strLanguage)).Append(',')
                                                .Append(strSpace).Append(objWeapon.DisplayNameShort(strLanguage)).Append(',')
                                                .Append(strSpace).Append(objAccessory.DisplayNameShort(strLanguage)).Append(')');
                                        if (wireless)
                                            sbdGearReturn.Append(strSpace).Append(LanguageManager.GetString("String_Wireless", strLanguage));
                                        return sbdGearReturn.ToString();
                                    }
                                }
                            }

                            foreach(Cyberware objCyberware in objVehicleMod.Cyberware.DeepWhere(x => x.Children,
                                x => x.Gear.Count > 0))
                            {
                                objReturnGear = objCyberware.Gear.DeepFirstOrDefault(x => x.Children,
                                    x => x.InternalId == objImprovement.SourceName);
                                if(objReturnGear != null)
                                {
                                    StringBuilder sbdGearReturn = new StringBuilder(objReturnGear.DisplayNameShort(strLanguage));
                                    if (objReturnGear.Parent != null && objReturnGear.Parent is Gear parent)
                                        sbdGearReturn.Append(strSpace).Append('(').Append(objVehicle.DisplayNameShort(strLanguage)).Append(',')
                                            .Append(strSpace).Append(objVehicleMod.DisplayNameShort(strLanguage)).Append(',')
                                            .Append(strSpace).Append(objCyberware.DisplayNameShort(strLanguage)).Append(',')
                                            .Append(strSpace).Append(parent.DisplayNameShort(strLanguage)).Append(')');
                                    else
                                        sbdGearReturn.Append(strSpace).Append('(').Append(objVehicle.DisplayNameShort(strLanguage)).Append(',')
                                            .Append(strSpace).Append(objVehicleMod.DisplayNameShort(strLanguage)).Append(',')
                                            .Append(strSpace).Append(objCyberware.DisplayNameShort(strLanguage)).Append(')');
                                    if (wireless)
                                        sbdGearReturn.Append(strSpace).Append(LanguageManager.GetString("String_Wireless", strLanguage));
                                    return sbdGearReturn.ToString();
                                }
                            }
                        }
                    }

                    break;
                case Improvement.ImprovementSource.Spell:
                    foreach(Spell objSpell in Spells)
                    {
                        if(objSpell.InternalId == objImprovement.SourceName)
                        {
                            return objSpell.DisplayNameShort(strLanguage);
                        }
                    }

                    break;
                case Improvement.ImprovementSource.Power:
                    foreach(Power objPower in Powers)
                    {
                        if(objPower.InternalId == objImprovement.SourceName)
                        {
                            return objPower.DisplayNameShort(strLanguage);
                        }
                    }

                    break;
                case Improvement.ImprovementSource.CritterPower:
                    foreach(CritterPower objPower in CritterPowers)
                    {
                        if(objPower.InternalId == objImprovement.SourceName)
                        {
                            return objPower.DisplayNameShort(strLanguage);
                        }
                    }

                    break;
                case Improvement.ImprovementSource.Metamagic:
                case Improvement.ImprovementSource.Echo:
                    foreach(Metamagic objMetamagic in Metamagics)
                    {
                        if(objMetamagic.InternalId == objImprovement.SourceName)
                        {
                            return objMetamagic.DisplayNameShort(strLanguage);
                        }
                    }

                    break;
                case Improvement.ImprovementSource.Art:
                    foreach(Art objArt in Arts)
                    {
                        if(objArt.InternalId == objImprovement.SourceName)
                        {
                            return objArt.DisplayNameShort(strLanguage);
                        }
                    }

                    break;
                case Improvement.ImprovementSource.Enhancement:
                    foreach(Enhancement objEnhancement in Enhancements)
                    {
                        if(objEnhancement.InternalId == objImprovement.SourceName)
                        {
                            return objEnhancement.DisplayNameShort(strLanguage);
                        }
                    }

                    break;
                case Improvement.ImprovementSource.Armor:
                    foreach(Armor objArmor in Armor)
                    {
                        if(objArmor.InternalId == objImprovement.SourceName)
                        {
                            string strReturnArmor = objArmor.DisplayNameShort(strLanguage);
                            if (wireless)
                                strReturnArmor += strSpace + LanguageManager.GetString("String_Wireless", strLanguage);
                            return strReturnArmor;
                        }
                    }

                    break;
                case Improvement.ImprovementSource.ArmorMod:
                    foreach(Armor objArmor in Armor)
                    {
                        foreach(ArmorMod objMod in objArmor.ArmorMods)
                        {
                            if(objMod.InternalId == objImprovement.SourceName)
                            {
                                StringBuilder sbdReturnArmorMod = new StringBuilder(objMod.DisplayNameShort(strLanguage))
                                    .Append(strSpace).Append('(').Append(objArmor.DisplayNameShort(strLanguage)).Append(')');
                                if (wireless)
                                    sbdReturnArmorMod.Append(strSpace).Append(LanguageManager.GetString("String_Wireless", strLanguage));
                                return sbdReturnArmorMod.ToString();
                            }
                        }
                    }

                    break;
                case Improvement.ImprovementSource.ComplexForm:
                    foreach(ComplexForm objComplexForm in ComplexForms)
                    {
                        if(objComplexForm.InternalId == objImprovement.SourceName)
                        {
                            return objComplexForm.DisplayNameShort(strLanguage);
                        }
                    }

                    break;
                case Improvement.ImprovementSource.AIProgram:
                    foreach(AIProgram objProgram in AIPrograms)
                    {
                        if(objProgram.InternalId == objImprovement.SourceName)
                        {
                            return objProgram.DisplayNameShort(strLanguage);
                        }
                    }

                    break;
                case Improvement.ImprovementSource.Quality:
                    if(objImprovement.SourceName == "SEEKER_WIL")
                    {
                        return LoadData("qualities.xml")
                                   .SelectSingleNode(
                                       "/chummer/qualities/quality[name = \"Cyber-Singularity Seeker\"]/translate")
                                   ?.InnerText ?? "Cyber-Singularity Seeker";
                    }
                    else if(objImprovement.SourceName.StartsWith("SEEKER", StringComparison.Ordinal))
                    {
                        return LoadData("qualities.xml")
                                   .SelectSingleNode("/chummer/qualities/quality[name = \"Redliner\"]/translate")
                                   ?.InnerText ?? "Redliner";
                    }

                    foreach(Quality objQuality in Qualities)
                    {
                        if(objQuality.InternalId == objImprovement.SourceName)
                        {
                            return objQuality.DisplayNameShort(strLanguage);
                        }
                    }

                    break;
                case Improvement.ImprovementSource.MartialArtTechnique:
                    foreach(MartialArt objMartialArt in MartialArts)
                    {
                        foreach(MartialArtTechnique objAdvantage in objMartialArt.Techniques)
                        {
                            if(objAdvantage.InternalId == objImprovement.SourceName)
                            {
                                return objAdvantage.DisplayName(strLanguage);
                            }
                        }
                    }

                    break;
                case Improvement.ImprovementSource.MentorSpirit:
                    foreach(MentorSpirit objMentorSpirit in MentorSpirits)
                    {
                        if(objMentorSpirit.InternalId == objImprovement.SourceName)
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
                case Improvement.ImprovementSource.Tradition:
                    return LanguageManager.GetString("String_Tradition", strLanguage);
                case Improvement.ImprovementSource.AstralReputation:
                    return LanguageManager.GetString("String_AstralReputation", strLanguage);
                case Improvement.ImprovementSource.CyberadeptDaemon:
                    return LoadData("qualities.xml", strLanguage)
                        .SelectSingleNode(
                            "/chummer/qualities/quality[name = \"Resonant Stream: Cyberadept\"]/translate")
                        ?.InnerText ?? "Resonant Stream: Cyberadept";
                default:
                    if(objImprovement.ImproveType == Improvement.ImprovementType.ArmorEncumbrancePenalty)
                        return LanguageManager.GetString("String_ArmorEncumbrance", strLanguage);
                    // If this comes from a custom Improvement, use the name the player gave it instead of showing a GUID.
                    if(!string.IsNullOrEmpty(objImprovement.CustomName))
                        return objImprovement.CustomName;
                    string strReturn = objImprovement.SourceName;
                    if(string.IsNullOrEmpty(strReturn) || strReturn.IsGuid())
                    {
                        string strTemp = LanguageManager.GetString("String_" + objImprovement.ImproveSource.ToString(),
                            strLanguage, false);
                        if(!string.IsNullOrEmpty(strTemp))
                            strReturn = strTemp;
                    }

                    return strReturn;
            }

            return string.Empty;
        }


        public void FormatImprovementModifiers(StringBuilder sbdToolTip, ICollection<Improvement.ImprovementType> improvements, string strSpace, int intModifiers)
        {
            if (sbdToolTip == null)
                return;
            sbdToolTip.Append(strSpace).Append('+').Append(strSpace).Append(LanguageManager.GetString("Tip_Modifiers"));
            bool blnFirstModifier = true;
            foreach (Improvement objLoopImprovement in Improvements.Where(imp => improvements.Contains(imp.ImproveType) && imp.Enabled))
            {
                if (blnFirstModifier)
                {
                    blnFirstModifier = false;
                    sbdToolTip.Append(LanguageManager.GetString("String_Colon"));
                }
                else
                    sbdToolTip.Append(',');

                sbdToolTip.Append(strSpace).Append(GetObjectName(objLoopImprovement));
            }

            sbdToolTip.Append(strSpace).Append('(').Append(intModifiers.ToString(GlobalOptions.CultureInfo)).Append(')');
        }

        /// <summary>
        /// Return a list of CyberwareGrades from XML files.
        /// </summary>
        /// <param name="objSource">Source to load the Grades from, either Bioware or Cyberware.</param>
        /// <param name="blnIgnoreBannedGrades">Whether to ignore grades banned at chargen.</param>
        public List<Grade> GetGradeList(Improvement.ImprovementSource objSource, bool blnIgnoreBannedGrades = false)
        {
            StringBuilder sbdFilter = new StringBuilder();
            if(Options != null)
            {
                sbdFilter.Append('(').Append(Options.BookXPath()).Append(") and ");
                if (!IgnoreRules && !Created && !blnIgnoreBannedGrades)
                {
                    foreach (string strBannedGrade in Options.BannedWareGrades)
                    {
                        sbdFilter.Append("not(contains(name, \"").Append(strBannedGrade).Append("\")) and ");
                    }
                }
            }

            string strXPath;
            if(sbdFilter.Length != 0)
            {
                sbdFilter.Length -= 5;
                strXPath = "/chummer/grades/grade[(" + sbdFilter + ")]";
            }
            else
                strXPath = "/chummer/grades/grade";

            List<Grade> lstGrades;
            using (XmlNodeList xmlGradeList = LoadData(objSource == Improvement.ImprovementSource.Bioware
                    ? "bioware.xml"
                    : objSource == Improvement.ImprovementSource.Drug
                        ? "drugcomponents.xml"
                        : "cyberware.xml").SelectNodes(strXPath))
            {
                lstGrades = new List<Grade>(xmlGradeList?.Count ?? 0);
                if (xmlGradeList?.Count > 0)
                {
                    foreach(XmlNode objNode in xmlGradeList)
                    {
                        Grade objGrade = new Grade(this, objSource);
                        objGrade.Load(objNode);
                        lstGrades.Add(objGrade);
                    }
                }
            }

            return lstGrades;
        }

        /// <summary>
        /// Calculate the number of Free Spirit Power Points used.
        /// </summary>
        public string CalculateFreeSpiritPowerPoints()
        {
            StringBuilder sbdReturn;
            string strSpace = LanguageManager.GetString("String_Space");

            if(Metatype == "Free Spirit" && !IsCritter)
            {
                // PC Free Spirit.
                decimal decPowerPoints = 0;

                foreach(CritterPower objPower in CritterPowers)
                {
                    if(objPower.CountTowardsLimit)
                        decPowerPoints += objPower.PowerPoints;
                }

                int intPowerPoints = EDG.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.FreeSpiritPowerPoints);

                // If the house rule to base Power Points on the character's MAG value instead, use the character's MAG.
                if(Options.FreeSpiritPowerPointsMAG)
                    intPowerPoints = MAG.TotalValue +
                                     ImprovementManager.ValueOf(this,
                                         Improvement.ImprovementType.FreeSpiritPowerPoints);

                sbdReturn = new StringBuilder(intPowerPoints.ToString(GlobalOptions.CultureInfo))
                    .Append(strSpace).Append('(').Append((intPowerPoints - decPowerPoints).ToString(GlobalOptions.CultureInfo))
                    .Append(strSpace).Append(LanguageManager.GetString("String_Remaining")).Append(')');
            }
            else
            {
                int intPowerPoints;

                if(Metatype == "Free Spirit")
                {
                    // Critter Free Spirits have a number of Power Points equal to their EDG plus any Free Spirit Power Points Improvements.
                    intPowerPoints = EDG.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.FreeSpiritPowerPoints);
                }
                else if(Metatype == "Ally Spirit")
                {
                    // Ally Spirits get a number of Power Points equal to their MAG.
                    intPowerPoints = MAG.TotalValue;
                }
                else
                {
                    // Spirits get 1 Power Point for every 3 full points of Force (MAG) they possess.
                    intPowerPoints = MAG.TotalValue / 3;
                }

                int intUsed = 0; // _objCharacter.CritterPowers.Count - intExisting;
                foreach(CritterPower objPower in CritterPowers)
                {
                    if(objPower.Category != "Weakness" && objPower.CountTowardsLimit)
                        intUsed += 1;
                }

                sbdReturn = new StringBuilder(intPowerPoints.ToString(GlobalOptions.CultureInfo))
                    .Append(strSpace).Append('(').Append((intPowerPoints - intUsed).ToString(GlobalOptions.CultureInfo))
                    .Append(strSpace).Append(LanguageManager.GetString("String_Remaining")).Append(')');
            }

            return sbdReturn.ToString();
        }

        /// <summary>
        /// Calculate the number of Free Sprite Power Points used.
        /// </summary>
        public string CalculateFreeSpritePowerPoints()
        {
            // Free Sprite Power Points.
            int intUsedPowerPoints = 0;

            foreach(CritterPower objPower in CritterPowers)
            {
                if(objPower.CountTowardsLimit)
                    intUsedPowerPoints += 1;
            }

            int intPowerPoints = EDG.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.FreeSpiritPowerPoints);

            string strSpace = LanguageManager.GetString("String_Space");

            return intPowerPoints.ToString(GlobalOptions.CultureInfo) + strSpace + '(' + (intPowerPoints - intUsedPowerPoints).ToString(GlobalOptions.CultureInfo)
                        + strSpace + LanguageManager.GetString("String_Remaining") + ')';
        }

        /// <summary>
        /// Construct a list of possible places to put a piece of modular cyberware. Names are display names of the given items, values are internalIDs of the given items.
        /// </summary>
        /// <param name="objModularCyberware">Cyberware for which to construct the list.</param>
        /// <param name="blnMountChangeAllowed">Whether or not <paramref name="objModularCyberware"/> can change its mount</param>
        /// <returns></returns>
        public List<ListItem> ConstructModularCyberlimbList(Cyberware objModularCyberware, out bool blnMountChangeAllowed)
        {
            if (objModularCyberware == null)
                throw new ArgumentNullException(nameof(objModularCyberware));
            string strSpace = LanguageManager.GetString("String_Space");
            //Mounted cyberware should always be allowed to be dismounted.
            //Unmounted cyberware requires that a valid mount be present.
            blnMountChangeAllowed = objModularCyberware.IsModularCurrentlyEquipped;
            List<ListItem> lstReturn = new List<ListItem>(Cyberware.Count + Vehicles.Count)
            {
                new ListItem("None", LanguageManager.GetString("String_None"))
            };

            foreach(Cyberware objLoopCyberware in Cyberware.GetAllDescendants(x => x.Children))
            {
                // Make sure this has an eligible mount location and it's not the selected piece modular cyberware
                if(objLoopCyberware.HasModularMount == objModularCyberware.PlugsIntoModularMount
                   && (objLoopCyberware.Location == objModularCyberware.Location
                       || string.IsNullOrEmpty(objModularCyberware.Location))
                   && objLoopCyberware.Grade.Name == objModularCyberware.Grade.Name
                   && objLoopCyberware != objModularCyberware)
                {
                    // Make sure it's not the place where the mount is already occupied (either by us or something else)
                    if(objLoopCyberware.Children.All(x => x.PlugsIntoModularMount != objLoopCyberware.HasModularMount))
                    {
                        string strName = objLoopCyberware.Parent?.CurrentDisplayName
                                         ?? objLoopCyberware.CurrentDisplayName;
                        lstReturn.Add(new ListItem(objLoopCyberware.InternalId, strName));
                        blnMountChangeAllowed = true;
                    }
                }
            }

            foreach(Vehicle objLoopVehicle in Vehicles)
            {
                foreach(VehicleMod objLoopVehicleMod in objLoopVehicle.Mods)
                {
                    foreach(Cyberware objLoopCyberware in objLoopVehicleMod.Cyberware.GetAllDescendants(x => x.Children))
                    {
                        // Make sure this has an eligible mount location and it's not the selected piece modular cyberware
                        if(objLoopCyberware.HasModularMount == objModularCyberware.PlugsIntoModularMount
                           && objLoopCyberware.Location == objModularCyberware.Location
                           && objLoopCyberware.Grade.Name == objModularCyberware.Grade.Name
                           && objLoopCyberware != objModularCyberware)
                        {
                            // Make sure it's not the place where the mount is already occupied (either by us or something else)
                            if(objLoopCyberware.Children.All(x => x.PlugsIntoModularMount != objLoopCyberware.HasModularMount))
                            {
                                string strName = objLoopVehicle.CurrentDisplayName
                                                 + strSpace + (objLoopCyberware.Parent?.CurrentDisplayName
                                                               ?? objLoopVehicleMod.CurrentDisplayName);
                                lstReturn.Add(new ListItem(objLoopCyberware.InternalId, strName));
                                blnMountChangeAllowed = true;
                            }
                        }
                    }
                }

                foreach(WeaponMount objLoopWeaponMount in objLoopVehicle.WeaponMounts)
                {
                    foreach(VehicleMod objLoopVehicleMod in objLoopWeaponMount.Mods)
                    {
                        foreach(Cyberware objLoopCyberware in objLoopVehicleMod.Cyberware.GetAllDescendants(x => x.Children))
                        {
                            // Make sure this has an eligible mount location and it's not the selected piece modular cyberware
                            if(objLoopCyberware.HasModularMount == objModularCyberware.PlugsIntoModularMount
                               && objLoopCyberware.Location == objModularCyberware.Location
                               && objLoopCyberware.Grade.Name == objModularCyberware.Grade.Name
                               && objLoopCyberware != objModularCyberware)
                            {
                                // Make sure it's not the place where the mount is already occupied (either by us or something else)
                                if(objLoopCyberware.Children.All(x => x.PlugsIntoModularMount != objLoopCyberware.HasModularMount))
                                {
                                    string strName = objLoopVehicle.CurrentDisplayName
                                                     + strSpace + (objLoopCyberware.Parent?.CurrentDisplayName
                                                                   ?? objLoopVehicleMod.CurrentDisplayName);
                                    lstReturn.Add(new ListItem(objLoopCyberware.InternalId, strName));
                                    blnMountChangeAllowed = true;
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
            string strColonCharacter = LanguageManager.GetString("String_Colon");
            string strSpace = LanguageManager.GetString("String_Space");
            StringBuilder sbdMessage = new StringBuilder(LanguageManager.GetString("Message_KarmaValue", strLanguage)).AppendLine();
            string strKarmaString = LanguageManager.GetString("String_Karma", strLanguage);
            int intExtraKarmaToRemoveForPointBuyComparison = 0;
            intReturn = Options.BuildKarma;
            if (EffectiveBuildMethodUsesPriorityTables)
            {
                // Subtract extra karma cost of a metatype in priority
                intReturn -= MetatypeBP;
            }

            sbdMessage.AppendLine().Append(LanguageManager.GetString("Label_Base", strLanguage)).Append(strColonCharacter)
                .Append(strSpace).Append(intReturn.ToString(GlobalOptions.CultureInfo)).Append(strSpace).Append(strKarmaString);

            if (EffectiveBuildMethodUsesPriorityTables)
            {
                // Zeroed to -10 because that's Human's value at default settings
                int intMetatypeQualitiesValue = -2 * Options.KarmaAttribute;
                // Karma value of all qualities (we're ignoring metatype cost because Point Buy karma costs don't line up with other methods' values)
                foreach(Quality objQuality in Qualities.Where(x =>
                   x.OriginSource == QualitySource.Metatype || x.OriginSource == QualitySource.MetatypeRemovable))
                {
                    XmlNode xmlQualityNode = objQuality.GetNode();
                    intMetatypeQualitiesValue += Convert.ToInt32(xmlQualityNode?["karma"]?.InnerText, GlobalOptions.InvariantCultureInfo);
                }

                intReturn += intMetatypeQualitiesValue;

                int intTemp = 0;
                int intAttributesValue = 0;
                // Value from attribute points and raised attribute minimums
                foreach(CharacterAttrib objLoopAttrib in AttributeSection.AttributeList.Concat(AttributeSection
                    .SpecialAttributeList))
                {
                    string strAttributeName = objLoopAttrib.Abbrev;
                    if(strAttributeName != "ESS" &&
                        (strAttributeName != "MAGAdept" || (IsMysticAdept && Options.MysAdeptSecondMAGAttribute)) &&
                        objLoopAttrib.MetatypeMaximum > 0)
                    {
                        int intLoopAttribValue =
                            Math.Max(objLoopAttrib.Base + objLoopAttrib.FreeBase + objLoopAttrib.RawMinimum,
                                objLoopAttrib.TotalMinimum) + objLoopAttrib.AttributeValueModifiers;
                        if(intLoopAttribValue > 1)
                        {
                            intTemp += ((intLoopAttribValue + 1) * intLoopAttribValue / 2 - 1) * Options.KarmaAttribute;
                            if(strAttributeName != "MAG" && strAttributeName != "MAGAdept" &&
                                strAttributeName != "RES" && strAttributeName != "DEP")
                            {
                                int intVanillaAttribValue =
                                    Math.Max(
                                        objLoopAttrib.Base + objLoopAttrib.FreeBase + objLoopAttrib.RawMinimum -
                                        objLoopAttrib.MetatypeMinimum + 1,
                                        objLoopAttrib.TotalMinimum - objLoopAttrib.MetatypeMinimum + 1) +
                                    objLoopAttrib.AttributeValueModifiers;
                                intAttributesValue += ((intVanillaAttribValue + 1) * intVanillaAttribValue / 2 - 1) *
                                                      Options.KarmaAttribute;
                            }
                            else
                                intAttributesValue += ((intLoopAttribValue + 1) * intLoopAttribValue / 2 - 1) *
                                                      Options.KarmaAttribute;
                        }
                    }
                }

                if(intTemp - intAttributesValue + intMetatypeQualitiesValue != 0)
                {
                    sbdMessage.AppendLine().Append(LanguageManager.GetString("Label_SumtoTenHeritage", strLanguage))
                        .Append(strSpace).Append((intTemp - intAttributesValue + intMetatypeQualitiesValue).ToString(GlobalOptions.CultureInfo))
                        .Append(strSpace).Append(strKarmaString);
                }

                if(intAttributesValue != 0)
                {
                    sbdMessage.AppendLine().Append(LanguageManager.GetString("Label_SumtoTenAttributes", strLanguage))
                        .Append(strSpace).Append(intAttributesValue.ToString(GlobalOptions.CultureInfo)).Append(strSpace).Append(strKarmaString);
                }

                intReturn += intTemp;

                intTemp = 0;
                // This is where "Talent" qualities like Adept and Technomancer get added in
                foreach(Quality objQuality in Qualities.Where(x => x.OriginSource == QualitySource.Heritage))
                {
                    XmlNode xmlQualityNode = objQuality.GetNode();
                    intTemp += Convert.ToInt32(xmlQualityNode["karma"]?.InnerText, GlobalOptions.InvariantCultureInfo);
                }

                if(intTemp != 0)
                {
                    sbdMessage.AppendLine().Append(LanguageManager.GetString("String_Qualities", strLanguage)).Append(strColonCharacter)
                        .Append(strSpace).Append(intTemp.ToString(GlobalOptions.CultureInfo)).Append(strSpace).Append(strKarmaString);
                    intReturn += intTemp;
                }

                // Value from free spells
                intTemp = FreeSpells * SpellKarmaCost("Spells");
                if(intTemp != 0)
                {
                    sbdMessage.AppendLine().Append(LanguageManager.GetString("String_FreeSpells", strLanguage)).Append(strColonCharacter)
                        .Append(strSpace).Append(intTemp.ToString(GlobalOptions.CultureInfo)).Append(strSpace).Append(strKarmaString);
                    intReturn += intTemp;
                }

                // Value from free complex forms
                intTemp = CFPLimit * ComplexFormKarmaCost;
                if(intTemp != 0)
                {
                    sbdMessage.AppendLine().Append(LanguageManager.GetString("String_FreeCFs", strLanguage)).Append(strColonCharacter)
                        .Append(strSpace).Append(intTemp.ToString(GlobalOptions.CultureInfo)).Append(strSpace).Append(strKarmaString);
                    intReturn += intTemp;
                }

                intTemp = 0;
                // Value from skill points
                foreach(Skill objLoopActiveSkill in SkillsSection.Skills)
                {
                    if(!(objLoopActiveSkill.SkillGroupObject?.Base > 0))
                    {
                        int intLoopRating = objLoopActiveSkill.Base;
                        if(intLoopRating > 0)
                        {
                            intTemp += Options.KarmaNewActiveSkill;
                            intTemp += ((intLoopRating + 1) * intLoopRating / 2 - 1) * Options.KarmaImproveActiveSkill;
                            if (EffectiveBuildMethodIsLifeModule)
                                intTemp += objLoopActiveSkill.Specializations.Count(x => x.Free) *
                                           Options.KarmaSpecialization;
                            else if(!objLoopActiveSkill.BuyWithKarma)
                                intTemp += objLoopActiveSkill.Specializations.Count * Options.KarmaSpecialization;
                        }
                    }
                }

                if(intTemp != 0)
                {
                    sbdMessage.AppendLine().Append(LanguageManager.GetString("String_SkillPoints", strLanguage)).Append(strColonCharacter)
                        .Append(strSpace).Append(intTemp.ToString(GlobalOptions.CultureInfo)).Append(strSpace).Append(strKarmaString);
                    intReturn += intTemp;
                }

                intTemp = 0;
                // Value from skill group points
                foreach(SkillGroup objLoopSkillGroup in SkillsSection.SkillGroups)
                {
                    int intLoopRating = objLoopSkillGroup.Base;
                    if(intLoopRating > 0)
                    {
                        intTemp += Options.KarmaNewSkillGroup;
                        intTemp += ((intLoopRating + 1) * intLoopRating / 2 - 1) * Options.KarmaImproveSkillGroup;
                    }
                }

                if(intTemp != 0)
                {
                    sbdMessage.AppendLine().Append(LanguageManager.GetString("String_SkillGroupPoints", strLanguage)).Append(strColonCharacter)
                        .Append(strSpace).Append(intTemp.ToString(GlobalOptions.CultureInfo)).Append(strSpace).Append(strKarmaString);
                    intReturn += intTemp;
                }

                // Starting Nuyen karma value
                intTemp = decimal.ToInt32(decimal.Ceiling(StartingNuyen / Options.NuyenPerBP));
                if(intTemp != 0)
                {
                    sbdMessage.AppendLine().Append(LanguageManager.GetString("Checkbox_CreatePACKSKit_StartingNuyen", strLanguage)).Append(strColonCharacter)
                        .Append(strSpace).Append(intTemp.ToString(GlobalOptions.CultureInfo)).Append(strSpace).Append(strKarmaString);
                    intReturn += intTemp;
                }
            }

            int intContactPointsValue = ContactPoints * Options.KarmaContact;
            if(intContactPointsValue != 0)
            {
                sbdMessage.AppendLine().Append(LanguageManager.GetString("String_Contacts", strLanguage)).Append(strColonCharacter)
                    .Append(strSpace).Append(intContactPointsValue.ToString(GlobalOptions.CultureInfo)).Append(strSpace).Append(strKarmaString);
                intReturn += intContactPointsValue;
                intExtraKarmaToRemoveForPointBuyComparison += intContactPointsValue;
            }

            int intKnowledgePointsValue = 0;
            foreach(KnowledgeSkill objLoopKnowledgeSkill in SkillsSection.KnowledgeSkills)
            {
                int intLoopRating = objLoopKnowledgeSkill.Base;
                if(intLoopRating > 0)
                {
                    intKnowledgePointsValue += Options.KarmaNewKnowledgeSkill;
                    intKnowledgePointsValue += ((intLoopRating + 1) * intLoopRating / 2 - 1) *
                                               Options.KarmaImproveKnowledgeSkill;
                    if(EffectiveBuildMethodIsLifeModule)
                        intKnowledgePointsValue += objLoopKnowledgeSkill.Specializations.Count(x => x.Free) *
                                                   Options.KarmaKnowledgeSpecialization;
                    else if(!objLoopKnowledgeSkill.BuyWithKarma)
                        intKnowledgePointsValue += objLoopKnowledgeSkill.Specializations.Count *
                                                   Options.KarmaKnowledgeSpecialization;
                }
            }

            if(intKnowledgePointsValue != 0)
            {
                sbdMessage.AppendLine().Append(LanguageManager.GetString("Label_KnowledgeSkills", strLanguage)).Append(strColonCharacter)
                    .Append(strSpace).Append(intKnowledgePointsValue.ToString(GlobalOptions.CultureInfo)).Append(strSpace).Append(strKarmaString);
                intReturn += intKnowledgePointsValue;
                intExtraKarmaToRemoveForPointBuyComparison += intKnowledgePointsValue;
            }

            sbdMessage.AppendLine().AppendLine().Append(LanguageManager.GetString("String_Total", strLanguage)).Append(strColonCharacter)
                .Append(strSpace).Append(intReturn.ToString(GlobalOptions.CultureInfo)).Append(strSpace).Append(strKarmaString)
                .AppendLine().AppendLine().Append(LanguageManager.GetString("String_TotalComparisonWithPointBuy", strLanguage)).Append(strColonCharacter)
                .Append(strSpace).Append((intReturn - intExtraKarmaToRemoveForPointBuyComparison).ToString(GlobalOptions.CultureInfo))
                .Append(strSpace).Append(strKarmaString);

            return sbdMessage.ToString();
        }

        /// <summary>
        /// Creates a list of keywords for each category of an XML node. Used to preselect whether items of that category are discounted by the Black Market Pipeline quality.
        /// </summary>
        public HashSet<string> GenerateBlackMarketMappings(XmlDocument xmlCategoryDocument)
        {
            if (xmlCategoryDocument == null)
                throw new ArgumentNullException(nameof(xmlCategoryDocument));
            HashSet<string> setBlackMarketMaps = new HashSet<string>();
            // Character has no Black Market discount qualities. Fail out early.
            if(BlackMarketDiscount)
            {
                // Get all the improved names of the Black Market Pipeline improvements. In most cases this should only be 1 item, but supports custom content.
                HashSet<string> setNames = new HashSet<string>();
                foreach(Improvement objImprovement in Improvements)
                {
                    if(objImprovement.ImproveType == Improvement.ImprovementType.BlackMarketDiscount &&
                        objImprovement.Enabled)
                        setNames.Add(objImprovement.ImprovedName);
                }

                using (XmlNodeList xmlCategoryList = xmlCategoryDocument.SelectNodes("/chummer/categories/category"))
                {
                    if (xmlCategoryList != null)
                    {
                        // For each category node, split the comma-separated blackmarket attribute (if present on the node), then add each category where any of those items matches a Black Market Pipeline improvement.
                        foreach (XmlNode xmlCategoryNode in xmlCategoryList)
                        {
                            string strBlackMarketAttribute = xmlCategoryNode.Attributes?["blackmarket"]?.InnerText;
                            if (!string.IsNullOrEmpty(strBlackMarketAttribute) &&
                                strBlackMarketAttribute.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries).Any(x => setNames.Contains(x)))
                            {
                                setBlackMarketMaps.Add(xmlCategoryNode.InnerText);
                            }
                        }
                    }
                }
            }

            return setBlackMarketMaps;
        }

        /// <summary>
        /// Creates a list of keywords for each category of an XML node. Used to preselect whether items of that category are discounted by the Black Market Pipeline quality.
        /// </summary>
        public HashSet<string> GenerateBlackMarketMappings(XPathNavigator xmlCategoryList)
        {
            HashSet<string> setBlackMarketMaps = new HashSet<string>();
            // Character has no Black Market discount qualities. Fail out early.
            if(BlackMarketDiscount)
            {
                if (xmlCategoryList == null)
                {
                    return setBlackMarketMaps;
                }
                // if the passed list is still the root, assume we're looking for default categories. Special cases like vehicle modcategories are expected to be passed through by the parameter.
                if (xmlCategoryList.Name == "chummer")
                {
                    xmlCategoryList = xmlCategoryList.SelectSingleNode("categories");
                    if (xmlCategoryList == null)
                        return new HashSet<string>();
                }
                // Get all the improved names of the Black Market Pipeline improvements. In most cases this should only be 1 item, but supports custom content.
                HashSet<string> setNames = new HashSet<string>();
                foreach(Improvement objImprovement in Improvements)
                {
                    if(objImprovement.ImproveType == Improvement.ImprovementType.BlackMarketDiscount &&
                        objImprovement.Enabled)
                        setNames.Add(objImprovement.ImprovedName);
                }

                // For each category node, split the comma-separated blackmarket attribute (if present on the node), then add each category where any of those items matches a Black Market Pipeline improvement.
                foreach(XPathNavigator xmlCategoryNode in xmlCategoryList.Select("category"))
                {
                    string strBlackMarketAttribute = xmlCategoryNode.SelectSingleNode("@blackmarket")?.Value;
                    if(!string.IsNullOrEmpty(strBlackMarketAttribute) &&
                        strBlackMarketAttribute.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries).Any(x => setNames.Contains(x)))
                    {
                        setBlackMarketMaps.Add(xmlCategoryNode.Value);
                    }
                }
            }

            return setBlackMarketMaps;
        }

        /// <summary>
        /// Book code (using the translated version if applicable) using the character's data files.
        /// </summary>
        /// <param name="strAltCode">Book code to search for.</param>
        /// <param name="strLanguage">Language to load.</param>
        public string LanguageBookCodeFromAltCode(string strAltCode, string strLanguage = "")
        {
            return CommonFunctions.LanguageBookCodeFromAltCode(strAltCode, strLanguage, this);
        }

        /// <summary>
        /// Book code (using the translated version if applicable) using the character's data files.
        /// </summary>
        /// <param name="strCode">Book code to search for.</param>
        /// <param name="strLanguage">Language to load.</param>
        public string LanguageBookShort(string strCode, string strLanguage = "")
        {
            return CommonFunctions.LanguageBookShort(strCode, strLanguage, this);
        }

        /// <summary>
        /// Book name (using the translated version if applicable) using the character's data files.
        /// </summary>
        /// <param name="strCode">Book code to search for.</param>
        /// <param name="strLanguage">Language to load.</param>
        public string LanguageBookLong(string strCode, string strLanguage = "")
        {
            return CommonFunctions.LanguageBookLong(strCode, strLanguage, this);
        }

        /// <summary>
        /// Attempt to translate any Extra text for an item using the character's data files.
        /// </summary>
        /// <param name="strExtra">Extra string to translate.</param>
        /// <param name="strIntoLanguage">Language into which the string should be translated</param>
        public string TranslateExtra(string strExtra, string strIntoLanguage = "")
        {
            return LanguageManager.TranslateExtra(strExtra, strIntoLanguage, this);
        }

        /// <summary>
        /// Attempt to translate any Extra text for an item from a foreign language to the default one using the character's data files.
        /// </summary>
        /// <param name="strExtra">Extra string to translate.</param>
        /// <param name="strFromLanguage">Language from which the string should be translated</param>
        public string ReverseTranslateExtra(string strExtra, string strFromLanguage = "")
        {
            return LanguageManager.ReverseTranslateExtra(strExtra, strFromLanguage, this);
        }
        #endregion

        #region UI Methods

        #region Move TreeNodes

        /// <summary>
        /// Move a Gear TreeNode after Drag and Drop, changing its parent.
        /// </summary>
        /// <param name="objGearNode">Node of gear to move.</param>
        /// <param name="objDestination">Destination Node.</param>
        public void MoveGearParent(TreeNode objDestination, TreeNode objGearNode)
        {
            if (objGearNode == null || objDestination == null)
                return;
            // The item cannot be dropped onto itself or onto one of its children.
            for(TreeNode objCheckNode = objDestination;
                objCheckNode != null && objCheckNode.Level >= objDestination.Level;
                objCheckNode = objCheckNode.Parent)
                if(objCheckNode == objGearNode)
                    return;
            if(!(objGearNode.Tag is Gear objGear))
            {
                return;
            }

            // Gear cannot be moved to one if its children.
            bool blnAllowMove = true;
            TreeNode objFindNode = objDestination;
            if(objDestination.Level > 0)
            {
                do
                {
                    objFindNode = objFindNode.Parent;
                    if(objFindNode.Tag == objGear)
                    {
                        blnAllowMove = false;
                        break;
                    }
                } while(objFindNode.Level > 0);
            }

            if(!blnAllowMove)
                return;

            // Remove the Gear from the character.
            if(objGear.Parent is IHasChildren<Gear> parent)
                parent.Children.Remove(objGear);
            else
                Gear.Remove(objGear);

            if(objDestination.Tag is Location objLocation)
            {
                // The Gear was moved to a location, so add it to the character instead.
                objGear.Location = objLocation;
                objLocation.Children.Add(objGear);
                Gear.Add(objGear);
            }
            else if(objDestination.Tag is Gear objParent)
            {
                // Add the Gear as a child of the destination Node and clear its location.
                objGear.Location = null;
                objParent.Children.Add(objGear);
            }
        }

        /// <summary>
        /// Move a Gear TreeNode after Drag and Drop.
        /// </summary>
        /// <param name="intNewIndex">Node's new index.</param>
        /// <param name="objDestination">Destination Node.</param>
        /// <param name="nodeToMove">Node of gear to move.</param>
        public void MoveGearNode(int intNewIndex, TreeNode objDestination, TreeNode nodeToMove)
        {
            if (objDestination == null || nodeToMove == null)
                return;
            if (nodeToMove.Tag is Gear objGear)
            {
                TreeNode objNewParent = objDestination;
                while (objNewParent.Level > 0 && !(objNewParent.Tag is Location))
                    objNewParent = objNewParent.Parent;

                if (objNewParent.Tag is Location objLocation)
                {
                    nodeToMove.Remove();
                    objGear.Location = objLocation;
                    objNewParent.Nodes.Insert(0, nodeToMove);
                }
                else if (objNewParent.Tag is string)
                {
                    objGear.Location = null;
                    intNewIndex = Math.Min(intNewIndex, Gear.Count - 1);
                    Gear.Move(Gear.IndexOf(objGear), intNewIndex);
                }
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
            if (nodOldNode == null)
                return;
            if(objDestination != null)
            {
                TreeNode objNewParent = objDestination;
                while(objNewParent.Level > 0)
                    objNewParent = objNewParent.Parent;
                intNewIndex = objNewParent.Index;
            }

            if(intNewIndex == 0)
                return;

            if(!(nodOldNode.Tag is Location objLocation)) return;
            GearLocations.Move(GearLocations.IndexOf(objLocation), intNewIndex);
        }

        /// <summary>
        /// Move a Lifestyle TreeNode after Drag and Drop.
        /// </summary>
        /// <param name="intNewIndex">Node's new index.</param>
        /// <param name="objDestination">Destination Node.</param>
        /// <param name="nodLifestyleNode">Node of lifestyle to move.</param>
        public void MoveLifestyleNode(int intNewIndex, TreeNode objDestination, TreeNode nodLifestyleNode)
        {
            if (nodLifestyleNode == null)
                return;
            if (objDestination != null)
            {
                TreeNode objNewParent = objDestination;
                while(objNewParent.Level > 0)
                    objNewParent = objNewParent.Parent;
                intNewIndex = objNewParent.Index;
            }

            if (intNewIndex == 0)
                return;

            if (nodLifestyleNode.Tag is Lifestyle objLifestyle)
                Lifestyles.Move(Lifestyles.IndexOf(objLifestyle), intNewIndex);
        }

        /// <summary>
        /// Move an Armor TreeNode after Drag and Drop.
        /// </summary>
        /// <param name="intNewIndex">Node's new index.</param>
        /// <param name="objDestination">Destination Node.</param>
        /// <param name="nodeToMove">Node of armor to move.</param>
        public void MoveArmorNode(int intNewIndex, TreeNode objDestination, TreeNode nodeToMove)
        {
            if (objDestination == null)
                return;
            if (nodeToMove?.Tag is Armor objArmor)
            {
                TreeNode objNewParent = objDestination;
                while (objNewParent.Level > 0 && !(objNewParent.Tag is Location))
                    objNewParent = objNewParent.Parent;

                if (objNewParent.Tag is Location objLocation)
                {
                    nodeToMove.Remove();
                    objArmor.Location = objLocation;
                    objNewParent.Nodes.Insert(0, nodeToMove);
                }
                else if(objNewParent.Tag is string)
                {
                    objArmor.Location = null;
                    intNewIndex = Math.Min(intNewIndex, Armor.Count - 1);
                    Armor.Move(Armor.IndexOf(objArmor), intNewIndex);
                }
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
            if (nodOldNode == null)
                return;
            if (objDestination != null)
            {
                TreeNode objNewParent = objDestination;
                while (objNewParent.Level > 0)
                    objNewParent = objNewParent.Parent;
                intNewIndex = objNewParent.Index;
            }

            if (intNewIndex == 0)
                return;

            if (!(nodOldNode.Tag is Location objLocation))
                return;
            ArmorLocations.Move(ArmorLocations.IndexOf(objLocation), intNewIndex);
        }

        /// <summary>
        /// Move a Weapon TreeNode after Drag and Drop.
        /// </summary>
        /// <param name="intNewIndex">Node's new index.</param>
        /// <param name="objDestination">Destination Node.</param>
        /// <param name="nodeToMove">Node of weapon to move.</param>
        public void MoveWeaponNode(int intNewIndex, TreeNode objDestination, TreeNode nodeToMove)
        {
            if (objDestination == null)
                return;
            if (nodeToMove?.Tag is Weapon objWeapon)
            {
                TreeNode objNewParent = objDestination;
                while (objNewParent.Level > 0 && !(objNewParent.Tag is Location))
                    objNewParent = objNewParent.Parent;

                if (objNewParent.Tag is Location objLocation)
                {
                    nodeToMove.Remove();
                    objWeapon.Location = objLocation;
                    objNewParent.Nodes.Insert(0, nodeToMove);
                }
                else if(objNewParent.Tag is string)
                {
                    objWeapon.Location = null;
                    intNewIndex = Math.Min(intNewIndex, Weapons.Count - 1);
                    Weapons.Move(Weapons.IndexOf(objWeapon), intNewIndex);
                }
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
            if (nodOldNode == null)
                return;
            if (objDestination != null)
            {
                TreeNode objNewParent = objDestination;
                while (objNewParent.Level > 0)
                    objNewParent = objNewParent.Parent;
                intNewIndex = objNewParent.Index;
            }

            if (intNewIndex == 0)
                return;

            if (!(nodOldNode.Tag is Location objLocation))
                return;
            WeaponLocations.Move(WeaponLocations.IndexOf(objLocation), intNewIndex);
        }

        /// <summary>
        /// Move a Vehicle TreeNode after Drag and Drop.
        /// </summary>
        /// <param name="intNewIndex">Node's new index.</param>
        /// <param name="objDestination">Destination Node.</param>
        /// <param name="nodeToMove">Node of vehicle to move.</param>
        public void MoveVehicleNode(int intNewIndex, TreeNode objDestination, TreeNode nodeToMove)
        {
            if (objDestination == null)
                return;
            if(nodeToMove?.Tag is Vehicle objVehicle)
            {
                TreeNode objNewParent = objDestination;
                while (objNewParent.Level > 0 && !(objNewParent.Tag is Location))
                    objNewParent = objNewParent.Parent;

                if (objNewParent.Tag is Location objLocation)
                {
                    nodeToMove.Remove();
                    objVehicle.Location = objLocation;
                    objNewParent.Nodes.Insert(0, nodeToMove);
                }
                else if (objNewParent.Tag is string)
                {
                    objVehicle.Location = null;
                    intNewIndex = Math.Min(intNewIndex, Weapons.Count - 1);
                    Vehicles.Move(Vehicles.IndexOf(objVehicle), intNewIndex);
                }
            }
        }

        /// <summary>
        /// Move a Vehicle Gear TreeNode after Drag and Drop.
        /// </summary>
        /// <param name="nodDestination">Destination Node.</param>
        /// <param name="nodGearNode">Node of gear to move.</param>
        public void MoveVehicleGearParent(TreeNode nodDestination, TreeNode nodGearNode)
        {
            if (nodDestination == null || nodGearNode == null)
                return;
            // The item cannot be dropped onto itself or onto one of its children.
            for (TreeNode objCheckNode = nodDestination;
                objCheckNode != null && objCheckNode.Level >= nodDestination.Level;
                objCheckNode = objCheckNode.Parent)
                if (objCheckNode == nodGearNode)
                    return;
            if (!(nodGearNode.Tag is IHasInternalId nodeId)) return;
            // Locate the currently selected piece of Gear.
            //TODO: Better interface for determining what the parent of a bit of gear is.
            Gear objGear = Vehicles.FindVehicleGear(nodeId.InternalId, out Vehicle objOldVehicle,
                out WeaponAccessory objOldWeaponAccessory, out Cyberware objOldCyberware);

            if (objGear == null)
                return;

            if (nodDestination.Tag is Gear objDestinationGear)
            {
                // Remove the Gear from the Vehicle.
                if(objGear.Parent is IHasChildren<Gear> parent)
                    parent.Children.Remove(objGear);
                else if(objOldCyberware != null)
                    objOldCyberware.Gear.Remove(objGear);
                else if(objOldWeaponAccessory != null)
                    objOldWeaponAccessory.Gear.Remove(objGear);
                else
                    objOldVehicle.Gear.Remove(objGear);

                // Add the Gear to its new parent.
                objGear.Location = null;
                objDestinationGear.Children.Add(objGear);
            }
            else
            {
                // Determine if this is a Location.
                TreeNode nodVehicleNode = nodDestination;
                Location objLocation = null;
                while (nodVehicleNode.Level > 1)
                {
                    if (objLocation is null && nodVehicleNode.Tag is Location loc)
                    {
                        objLocation = loc;
                    }
                    nodVehicleNode = nodVehicleNode.Parent;
                }

                // Determine if this is a Location in the destination Vehicle.
                if (nodDestination.Tag is Vehicle objNewVehicle)
                {
                    // Remove the Gear from the Vehicle.
                    if(objGear.Parent is IHasChildren<Gear> parent)
                        parent.Children.Remove(objGear);
                    else if(objOldCyberware != null)
                        objOldCyberware.Gear.Remove(objGear);
                    else if(objOldWeaponAccessory != null)
                        objOldWeaponAccessory.Gear.Remove(objGear);
                    else
                        objOldVehicle.Gear.Remove(objGear);

                    // Add the Gear to the Vehicle and set its Location.
                    objGear.Parent = objNewVehicle;
                    objNewVehicle.Gear.Add(objGear);
                    objLocation?.Children.Add(objGear);
                }
            }
        }

        /// <summary>
        /// Move an Improvement TreeNode after Drag and Drop.
        /// </summary>
        /// <param name="objDestination">Destination Node.</param>
        /// <param name="nodOldNode">Node of improvement to move.</param>
        public void MoveImprovementNode(TreeNode objDestination, TreeNode nodOldNode)
        {
            if (objDestination == null)
                return;
            if (nodOldNode?.Tag is Improvement objImprovement)
            {
                TreeNode objNewParent = objDestination;
                while (objNewParent.Level > 0)
                    objNewParent = objNewParent.Parent;

                objImprovement.CustomGroup = objNewParent.Tag.ToString() == "Node_SelectedImprovements"
                    ? string.Empty
                    : objNewParent.Text;
                Improvements[Improvements.IndexOf(objImprovement)] = objImprovement;
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
            if (nodOldNode == null)
                return;
            if (objDestination != null)
            {
                TreeNode objNewParent = objDestination;
                while(objNewParent.Level > 0)
                    objNewParent = objNewParent.Parent;
                intNewIndex = objNewParent.Index;
            }

            if(intNewIndex == 0)
                return;

            string strLocation = nodOldNode.Tag.ToString();
            ImprovementGroups.Move(ImprovementGroups.IndexOf(strLocation), intNewIndex);
        }

        #endregion

        #region Tab clearing

        /// <summary>
        /// Clear all Spell tab elements from the character.
        /// </summary>
        public void ClearMagic(bool blnKeepAdeptEligible)
        {
            if(Improvements.All(x => !x.Enabled || (x.ImproveType != Improvement.ImprovementType.FreeSpells &&
                                                    x.ImproveType != Improvement.ImprovementType.FreeSpellsATT &&
                                                    x.ImproveType != Improvement.ImprovementType.FreeSpellsSkill)))
            {
                // Run through all of the Spells and remove their Improvements.
                for(int i = Spells.Count - 1; i >= 0; --i)
                {
                    if(i < Spells.Count)
                    {
                        Spell objToRemove = Spells[i];
                        if(objToRemove.Grade == 0)
                        {
                            if(blnKeepAdeptEligible && objToRemove.Category == "Rituals" &&
                                !objToRemove.Descriptors.Contains("Spell"))
                                continue;
                            // Remove the Improvements created by the Spell.
                            ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.Spell,
                                objToRemove.InternalId);
                            Spells.RemoveAt(i);
                        }
                    }
                }
            }

            for(int i = Spirits.Count - 1; i >= 0; --i)
            {
                if(i < Spirits.Count)
                {
                    Spirit objToRemove = Spirits[i];
                    if(objToRemove.EntityType == SpiritType.Spirit)
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
            for(int i = Powers.Count - 1; i >= 0; --i)
            {
                if(i < Powers.Count)
                {
                    Power objToRemove = Powers[i];
                    if (objToRemove.FreeLevels == 0 && objToRemove.FreePoints == 0)
                    {
                        // Remove the Improvements created by the Power.
                        ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.Power,
                            objToRemove.InternalId);
                        Powers.RemoveAt(i);
                    }
                    else
                        objToRemove.Rating = 0;
                }
            }
        }

        /// <summary>
        /// Clear all Technomancer tab elements from the character.
        /// </summary>
        public void ClearResonance()
        {
            // Run through all of the Complex Forms and remove their Improvements.
            for(int i = ComplexForms.Count - 1; i >= 0; --i)
            {
                if(i < ComplexForms.Count)
                {
                    ComplexForm objToRemove = ComplexForms[i];
                    if(objToRemove.Grade == 0)
                    {
                        // Remove the Improvements created by the Spell.
                        ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.ComplexForm,
                            objToRemove.InternalId);
                        ComplexForms.RemoveAt(i);
                    }
                }
            }

            for(int i = Spirits.Count - 1; i >= 0; --i)
            {
                if(i < Spirits.Count)
                {
                    Spirit objToRemove = Spirits[i];
                    if(objToRemove.EntityType == SpiritType.Sprite)
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
            for(int i = AIPrograms.Count - 1; i >= 0; --i)
            {
                if(i < AIPrograms.Count)
                {
                    AIProgram objToRemove = AIPrograms[i];
                    if(objToRemove.CanDelete)
                    {
                        // Remove the Improvements created by the Program.
                        ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.AIProgram,
                            objToRemove.InternalId);
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
            for(int i = Cyberware.Count - 1; i >= 0; i--)
            {
                if(i < Cyberware.Count)
                {
                    Cyberware objToRemove = Cyberware[i];
                    if(string.IsNullOrEmpty(objToRemove.ParentID))
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
            for(int i = CritterPowers.Count - 1; i >= 0; i--)
            {
                if(i < CritterPowers.Count)
                {
                    CritterPower objToRemove = CritterPowers[i];
                    if(objToRemove.Grade >= 0)
                    {
                        // Remove the Improvements created by the Metamagic.
                        ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.CritterPower,
                            objToRemove.InternalId);
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
            for(int i = Metamagics.Count - 1; i >= 0; i--)
            {
                if(i < Metamagics.Count)
                {
                    Metamagic objToRemove = Metamagics[i];
                    if(objToRemove.Grade >= 0)
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

        private CharacterOptions _objOptions = OptionsManager.LoadedCharacterOptions[GlobalOptions.DefaultCharacterOption];

        /// <summary>
        /// Character Options object.
        /// </summary>
        public CharacterOptions Options
        {
            get => _objOptions;
            private set // Private to make sure this is always in sync with GameplayOption
            {
                if (_objOptions != value)
                {
                    if (_objOptions != null)
                        _objOptions.PropertyChanged -= OptionsOnPropertyChanged;
                    _objOptions = value;
                    if (_objOptions != null)
                        _objOptions.PropertyChanged -= OptionsOnPropertyChanged;
                    if (!IsLoading)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Name of the file the Character is saved to.
        /// </summary>
        public string FileName
        {
            get => _strFileName;
            set
            {
                if(_strFileName != value)
                {
                    _strFileName = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Name of the file the Character is saved to.
        /// </summary>
        public DateTime FileLastWriteTime => _dateFileLastWriteTime;

        /// <summary>
        /// Whether or not the character has been saved as Created and can no longer be modified using the Build system.
        /// </summary>
        [HubTag]
        public bool Created
        {
            get => _blnCreated;
            set
            {
                if(_blnCreated != value)
                {
                    _blnCreated = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Character's name.
        /// </summary>
        [HubTag]
        public string Name
        {
            get => _strName;
            set
            {
                if(_strName != value)
                {
                    _strName = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Character's portraits encoded using Base64.
        /// </summary>
        public List<Image> Mugshots => _lstMugshots;

        /// <summary>
        /// Character's main portrait encoded using Base64.
        /// </summary>
        public Image MainMugshot
        {
            get
            {
                if(MainMugshotIndex >= Mugshots.Count || MainMugshotIndex < 0)
                    return null;

                return Mugshots[MainMugshotIndex];
            }
            set
            {
                if(value == null)
                {
                    MainMugshotIndex = -1;
                    return;
                }

                int intNewMainMugshotIndex = Mugshots.IndexOf(value);
                if(intNewMainMugshotIndex != -1)
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
                if(value >= _lstMugshots.Count || value < -1)
                    value = -1;

                if(_intMainMugshotIndex != value)
                {
                    _intMainMugshotIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        public void SaveMugshots(XmlTextWriter objWriter)
        {
            if (objWriter == null)
                return;
            objWriter.WriteElementString("mainmugshotindex", MainMugshotIndex.ToString(GlobalOptions.InvariantCultureInfo));
            // <mugshot>
            objWriter.WriteStartElement("mugshots");
            foreach(Image imgMugshot in Mugshots)
            {
                objWriter.WriteElementString("mugshot", GlobalOptions.ImageToBase64StringForStorage(imgMugshot));
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
            foreach(XPathNavigator objXmlMugshot in xmlMugshotsList)
            {
                string strMugshot = objXmlMugshot.Value;
                if(!string.IsNullOrWhiteSpace(strMugshot))
                {
                    lstMugshotsBase64.Add(strMugshot);
                }
            }

            if(lstMugshotsBase64.Count > 1)
            {
                Image[] objMugshotImages = new Image[lstMugshotsBase64.Count];
                Parallel.For(0, lstMugshotsBase64.Count, i =>
                {
                    objMugshotImages[i] = lstMugshotsBase64[i].ToImage(PixelFormat.Format32bppPArgb);
                });
                _lstMugshots.AddRange(objMugshotImages);
            }
            else if(lstMugshotsBase64.Count == 1)
            {
                _lstMugshots.Add(lstMugshotsBase64[0].ToImage(PixelFormat.Format32bppPArgb));
            }

            // Legacy Shimmer
            if(Mugshots.Count == 0)
            {
                XPathNavigator objOldMugshotNode = xmlSavedNode.SelectSingleNode("mugshot");
                string strMugshot = objOldMugshotNode?.Value;
                if(!string.IsNullOrWhiteSpace(strMugshot))
                {
                    _lstMugshots.Add(strMugshot.ToImage(PixelFormat.Format32bppPArgb));
                    _intMainMugshotIndex = 0;
                }
            }
        }

        public void PrintMugshots(XmlTextWriter objWriter)
        {
            if (objWriter == null)
                return;
            if(Mugshots.Count > 0)
            {
                // Since IE is retarded and can't handle base64 images before IE9, the image needs to be dumped to a temporary directory and its information rewritten.
                // If you give it an extension of jpg, gif, or png, it expects the file to be in that format and won't render the image unless it was originally that type.
                // But if you give it the extension img, it will render whatever you give it (which doesn't make any damn sense, but that's IE for you).
                string strMugshotsDirectoryPath = Path.Combine(Utils.GetStartupPath, "mugshots");
                if(!Directory.Exists(strMugshotsDirectoryPath))
                {
                    try
                    {
                        Directory.CreateDirectory(strMugshotsDirectoryPath);
                    }
                    catch(UnauthorizedAccessException)
                    {
                        Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_Insufficient_Permissions_Warning",
                            GlobalOptions.Language));
                    }
                }

                Guid guiImage = Guid.NewGuid();
                string imgMugshotPath = Path.Combine(strMugshotsDirectoryPath, guiImage.ToString("N", GlobalOptions.InvariantCultureInfo) + ".img");
                Image imgMainMugshot = MainMugshot;
                if(imgMainMugshot != null)
                {
                    imgMainMugshot.Save(imgMugshotPath);
                    // <mainmugshotpath />
                    objWriter.WriteElementString("mainmugshotpath",
                        "file://" + imgMugshotPath.Replace(Path.DirectorySeparatorChar, '/'));
                    // <mainmugshotbase64 />
                    objWriter.WriteElementString("mainmugshotbase64", GlobalOptions.ImageToBase64StringForStorage(imgMainMugshot));
                }

                // <othermugshots>
                objWriter.WriteElementString("hasothermugshots",
                    (imgMainMugshot == null || Mugshots.Count > 1).ToString(GlobalOptions.InvariantCultureInfo));
                objWriter.WriteStartElement("othermugshots");
                for(int i = 0; i < Mugshots.Count; ++i)
                {
                    if(i == MainMugshotIndex)
                        continue;
                    Image imgMugshot = Mugshots[i];
                    objWriter.WriteStartElement("mugshot");

                    objWriter.WriteElementString("stringbase64", GlobalOptions.ImageToBase64StringForStorage(imgMugshot));

                    imgMugshotPath = Path.Combine(strMugshotsDirectoryPath,
                        guiImage.ToString("N", GlobalOptions.InvariantCultureInfo) + i.ToString(GlobalOptions.InvariantCultureInfo) + ".img");
                    imgMugshot.Save(imgMugshotPath);
                    objWriter.WriteElementString("temppath",
                        "file://" + imgMugshotPath.Replace(Path.DirectorySeparatorChar, '/'));

                    objWriter.WriteEndElement();
                }

                // </mugshots>
                objWriter.WriteEndElement();
            }
        }

        /// <summary>
        /// Character's Gameplay Option.
        /// </summary>
        [HubTag]
        public string CharacterOptionsKey
        {
            get => _strCharacterOptionsKey;
            set
            {
                if(_strCharacterOptionsKey != value)
                {
                    _strCharacterOptionsKey = value;
                    OnPropertyChanged();
                    Options = OptionsManager.LoadedCharacterOptions[value];
                }
            }
        }

        /// <summary>
        /// Character's Metatype Priority.
        /// </summary>
        [HubTag]
        public string MetatypePriority
        {
            get => _strPriorityMetatype;
            set
            {
                if(_strPriorityMetatype != value)
                {
                    _strPriorityMetatype = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Character's Attributes Priority.
        /// </summary>
        [HubTag]
        public string AttributesPriority
        {
            get => _strPriorityAttributes;
            set
            {
                if(_strPriorityAttributes != value)
                {
                    _strPriorityAttributes = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Character's Special Priority.
        /// </summary>
        [HubTag]
        public string SpecialPriority
        {
            get => _strPrioritySpecial;
            set
            {
                if(_strPrioritySpecial != value)
                {
                    _strPrioritySpecial = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Character's Skills Priority.
        /// </summary>
        [HubTag]
        public string SkillsPriority
        {
            get => _strPrioritySkills;
            set
            {
                if(_strPrioritySkills != value)
                {
                    _strPrioritySkills = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Character's Resources Priority.
        /// </summary>
        [HubTag]
        public string ResourcesPriority
        {
            get => _strPriorityResources;
            set
            {
                if(_strPriorityResources != value)
                {
                    _strPriorityResources = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Character's Resources Priority.
        /// </summary>
        [HubTag]
        public string TalentPriority
        {
            get => _strPriorityTalent;
            set
            {
                if(_strPriorityTalent != value)
                {
                    _strPriorityTalent = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Character's list of priority bonus skills.
        /// </summary>
        public List<string> PriorityBonusSkillList => _lstPrioritySkills;

        /// <summary>
        /// Character's sex.
        /// </summary>
        public string Sex
        {
            get => _strSex;
            set
            {
                if(_strSex != value)
                {
                    _strSex = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _strCachedCharacterGrammaticGender = string.Empty;

        public string CharacterGrammaticGender
        {
            get
            {
                if(!string.IsNullOrEmpty(_strCachedCharacterGrammaticGender))
                    return _strCachedCharacterGrammaticGender;
                switch(ReverseTranslateExtra(Sex).ToUpperInvariant())
                {
                    case "M":
                    case "MALE":
                    case "MAN":
                    case "BOY":
                    case "LORD":
                    case "GENTLEMAN":
                    case "GUY":
                        return _strCachedCharacterGrammaticGender = "male";
                    case "F":
                    case "W":
                    case "FEMALE":
                    case "WOMAN":
                    case "GIRL":
                    case "LADY":
                    case "GAL":
                        return _strCachedCharacterGrammaticGender = "female";
                    default:
                        return _strCachedCharacterGrammaticGender = "neutral";
                }
            }
        }

        /// <summary>
        /// Character's age.
        /// </summary>
        public string Age
        {
            get => _strAge;
            set
            {
                if(_strAge != value)
                {
                    _strAge = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Character's eyes.
        /// </summary>
        public string Eyes
        {
            get => _strEyes;
            set
            {
                if(_strEyes != value)
                {
                    _strEyes = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Character's height.
        /// </summary>
        public string Height
        {
            get => _strHeight;
            set
            {
                if(_strHeight != value)
                {
                    _strHeight = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Character's weight.
        /// </summary>
        public string Weight
        {
            get => _strWeight;
            set
            {
                if(_strWeight != value)
                {
                    _strWeight = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Character's skin.
        /// </summary>
        public string Skin
        {
            get => _strSkin;
            set
            {
                if(_strSkin != value)
                {
                    _strSkin = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Character's hair.
        /// </summary>
        public string Hair
        {
            get => _strHair;
            set
            {
                if(_strHair != value)
                {
                    _strHair = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Character's description.
        /// </summary>
        public string Description
        {
            get => _strDescription;
            set
            {
                if(_strDescription != value)
                {
                    _strDescription = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Character's background.
        /// </summary>
        public string Background
        {
            get => _strBackground;
            set
            {
                if(_strBackground != value)
                {
                    _strBackground = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Character's concept.
        /// </summary>
        public string Concept
        {
            get => _strConcept;
            set
            {
                if(_strConcept != value)
                {
                    _strConcept = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Character notes.
        /// </summary>
        public string Notes
        {
            get => _strNotes;
            set
            {
                if(_strNotes != value)
                {
                    _strNotes = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// General gameplay notes.
        /// </summary>
        public string GameNotes
        {
            get => _strGameNotes;
            set
            {
                if(_strGameNotes != value)
                {
                    _strGameNotes = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// What is the Characters preferred hand
        /// </summary>
        public string PrimaryArm
        {
            get => _strPrimaryArm;
            set
            {
                if(_strPrimaryArm != value)
                {
                    _strPrimaryArm = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Player name.
        /// </summary>
        [HubTag]
        public string PlayerName
        {
            get => _strPlayerName;
            set
            {
                if(_strPlayerName != value)
                {
                    _strPlayerName = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Character's alias.
        /// </summary>
        [HubTag]
        public string Alias
        {
            get => _strAlias;
            set
            {
                if(_strAlias != value)
                {
                    _strAlias = value;
                    OnPropertyChanged();
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
                if(!string.IsNullOrWhiteSpace(Alias))
                    return Alias;
                if(!string.IsNullOrWhiteSpace(Name))
                    return Name;
                return LanguageManager.GetString("String_UnnamedCharacter");
            }
        }

        /// <summary>
        /// Street Cred.
        /// </summary>
        [HubTag]
        public int StreetCred
        {
            get => _intStreetCred;
            set
            {
                if(_intStreetCred != value)
                {
                    _intStreetCred = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Burnt Street Cred.
        /// </summary>
        public int BurntStreetCred
        {
            get => _intBurntStreetCred;
            set
            {
                if(_intBurntStreetCred != value)
                {
                    _intBurntStreetCred = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Notoriety.
        /// </summary>
        [HubTag]
        public int Notoriety
        {
            get => _intNotoriety;
            set
            {
                if(_intNotoriety != value)
                {
                    _intNotoriety = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Public Awareness.
        /// </summary>
        public int PublicAwareness
        {
            get => _intPublicAwareness;
            set
            {
                if(_intPublicAwareness != value)
                {
                    _intPublicAwareness = value;
                    OnPropertyChanged();
                }
            }
        }

        private void RefreshAstralReputationImprovements()
        {
            if (IsLoading) // Not all improvements are guaranteed to have been loaded in, so just skip the refresh until the end
            {
                PostLoadMethods.Enqueue(RefreshAstralReputationImprovements);
                return;
            }
            int intCurrentTotalAstralReputation = TotalAstralReputation;
            List<Improvement> lstCurrentAstralReputationImprovements = Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.AstralReputation).ToList();
            if (lstCurrentAstralReputationImprovements.All(x => x.Value == -intCurrentTotalAstralReputation))
                return;
            ImprovementManager.RemoveImprovements(this, lstCurrentAstralReputationImprovements);
            ImprovementManager.CreateImprovement(this, "Summoning", Improvement.ImprovementSource.AstralReputation,
                nameof(TotalAstralReputation).ToUpperInvariant(), Improvement.ImprovementType.Skill, Guid.NewGuid().ToString("D", GlobalOptions.InvariantCultureInfo),
                -intCurrentTotalAstralReputation);
            ImprovementManager.CreateImprovement(this, "Binding", Improvement.ImprovementSource.AstralReputation,
                nameof(TotalAstralReputation).ToUpperInvariant(), Improvement.ImprovementType.Skill, Guid.NewGuid().ToString("D", GlobalOptions.InvariantCultureInfo),
                -intCurrentTotalAstralReputation);
            ImprovementManager.CreateImprovement(this, "Banishing", Improvement.ImprovementSource.AstralReputation,
                nameof(TotalAstralReputation).ToUpperInvariant(), Improvement.ImprovementType.Skill, Guid.NewGuid().ToString("D", GlobalOptions.InvariantCultureInfo),
                -intCurrentTotalAstralReputation);
            if (intCurrentTotalAstralReputation >= 3)
                ImprovementManager.CreateImprovement(this, "Chain Breaker", Improvement.ImprovementSource.AstralReputation,
                    nameof(TotalAstralReputation).ToUpperInvariant(), Improvement.ImprovementType.DisableQuality, Guid.NewGuid().ToString("D", GlobalOptions.InvariantCultureInfo),
                    -intCurrentTotalAstralReputation);
        }

        /// <summary>
        /// Tooltip to use for Astral Reputation total.
        /// </summary>
        public string AstralReputationTooltip
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                StringBuilder sbdReturn = new StringBuilder(AstralReputation.ToString(GlobalOptions.CultureInfo));

                foreach (Improvement objImprovement in _lstImprovements)
                {
                    if (objImprovement.ImproveType == Improvement.ImprovementType.AstralReputation && objImprovement.Enabled)
                        sbdReturn.Append(strSpace).Append('+').Append(strSpace).Append(GetObjectName(objImprovement))
                            .Append(strSpace).Append('(').Append(objImprovement.Value.ToString(GlobalOptions.CultureInfo)).Append(')');
                }

                return sbdReturn.ToString();
            }
        }

        /// <summary>
        /// Astral Reputation (SG 207).
        /// </summary>
        public int TotalAstralReputation => Math.Max(0, AstralReputation + ImprovementManager.ValueOf(this, Improvement.ImprovementType.AstralReputation));

        /// <summary>
        /// Points of Astral Reputation that have added or removed manually (latter usually by burning Wild Reputation).
        /// </summary>
        public int AstralReputation
        {
            get => _intBaseAstralReputation;
            set
            {
                if (_intBaseAstralReputation != value)
                {
                    _intBaseAstralReputation = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Tooltip to use for Wild Reputation total.
        /// </summary>
        public string WildReputationTooltip
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                StringBuilder sbdReturn = new StringBuilder(WildReputation.ToString(GlobalOptions.CultureInfo));

                foreach (Improvement objImprovement in _lstImprovements)
                {
                    if (objImprovement.ImproveType == Improvement.ImprovementType.AstralReputationWild && objImprovement.Enabled)
                        sbdReturn.Append(strSpace).Append('+').Append(strSpace).Append(GetObjectName(objImprovement))
                            .Append(strSpace).Append('(').Append(objImprovement.Value.ToString(GlobalOptions.CultureInfo)).Append(')');
                }

                return sbdReturn.ToString();
            }
        }

        /// <summary>
        /// Total Reputation with Wild Spirits (FA 175).
        /// </summary>
        public int TotalWildReputation =>
            Math.Max(0,
                WildReputation
                + ImprovementManager.ValueOf(this, Improvement.ImprovementType.AstralReputationWild));

        /// <summary>
        /// Points of Wild Reputation that have added or removed manually (latter usually by burning it to lower Astral Reputation).
        /// </summary>
        public int WildReputation
        {
            get => _intBaseWildReputation;
            set
            {
                if (_intBaseWildReputation != value)
                {
                    _intBaseWildReputation = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Number of Physical Condition Monitor Boxes that are filled.
        /// </summary>
        public int PhysicalCMFilled
        {
            get
            {
                if(HomeNode is Vehicle objVehicle)
                    return objVehicle.PhysicalCMFilled;

                return _intPhysicalCMFilled;
            }
            set
            {
                if(HomeNode is Vehicle objVehicle)
                {
                    if(objVehicle.PhysicalCMFilled != value)
                    {
                        objVehicle.PhysicalCMFilled = value;
                        OnPropertyChanged();
                    }
                }
                else if(_intPhysicalCMFilled != value)
                {
                    _intPhysicalCMFilled = value;
                    OnPropertyChanged();
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
                if(IsAI && HomeNode != null)
                {
                    // A.I. do not have a Stun Condition Monitor, but they do have a Matrix Condition Monitor if they are in their home node.
                    return HomeNode.MatrixCMFilled;
                }

                return _intStunCMFilled;
            }
            set
            {
                if(IsAI && HomeNode != null)
                {
                    // A.I. do not have a Stun Condition Monitor, but they do have a Matrix Condition Monitor if they are in their home node.
                    if(HomeNode.MatrixCMFilled != value)
                    {
                        HomeNode.MatrixCMFilled = value;
                        OnPropertyChanged();
                    }
                }
                else if(_intStunCMFilled != value)
                {
                    _intStunCMFilled = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool AddInitiationsAllowed => Created || IgnoreRules || Options.AllowInitiationInCreateMode;

        /// <summary>
        /// Whether or not character creation rules should be ignored.
        /// </summary>
        [HubTag]
        public bool IgnoreRules
        {
            get => _blnIgnoreRules;
            set
            {
                if(_blnIgnoreRules != value)
                {
                    _blnIgnoreRules = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Contact Points.
        /// </summary>
        public int ContactPoints
        {
            get
            {
                if(_intCachedContactPoints == int.MinValue)
                {
                    string strExpression = Options.ContactPointsExpression;
                    if (strExpression.IndexOfAny('{', '+', '-', '*', ',') != -1 || strExpression.Contains("div"))
                    {
                        StringBuilder objValue = new StringBuilder(strExpression);
                        AttributeSection.ProcessAttributesInXPath(objValue, strExpression);

                        // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                        object objProcess = CommonFunctions.EvaluateInvariantXPath(objValue.ToString(), out bool blnIsSuccess);
                        _intCachedContactPoints = blnIsSuccess ? Convert.ToInt32(Math.Ceiling((double)objProcess)) : 0;
                    }
                    else
                        int.TryParse(strExpression, NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out _intCachedContactPoints);
                }

                return _intCachedContactPoints;
            }
        }

        /// <summary>
        /// Number of free Contact Points the character has used.
        /// </summary>
        public int ContactPointsUsed
        {
            get => _intContactPointsUsed;
            set
            {
                if(_intContactPointsUsed != value)
                {
                    _intContactPointsUsed = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// CFP Limit.
        /// </summary>
        public int CFPLimit
        {
            get => _intCFPLimit;
            set
            {
                if(_intCFPLimit != value)
                {
                    _intCFPLimit = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Total AI Program Limit.
        /// </summary>
        public int AINormalProgramLimit
        {
            get => _intAINormalProgramLimit;
            set
            {
                if(_intAINormalProgramLimit != value)
                {
                    _intAINormalProgramLimit = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// AI Advanced Program Limit.
        /// </summary>
        public int AIAdvancedProgramLimit
        {
            get => _intAIAdvancedProgramLimit;
            set
            {
                if(_intAIAdvancedProgramLimit != value)
                {
                    _intAIAdvancedProgramLimit = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Spell Limit.
        /// </summary>
        public int FreeSpells
        {
            get => _intFreeSpells;
            set
            {
                if(_intFreeSpells != value)
                {
                    _intFreeSpells = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma.
        /// </summary>
        public int Karma
        {
            get => _intKarma;
            set
            {
                if(_intKarma != value)
                {
                    _intKarma = value;
                    OnPropertyChanged();
                }
            }
        }

        public string DisplayKarma => Karma.ToString(GlobalOptions.CultureInfo);

        /// <summary>
        /// Special.
        /// </summary>
        public int Special
        {
            get => _intSpecial;
            set
            {
                if(_intSpecial != value)
                {
                    _intSpecial = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// TotalSpecial.
        /// </summary>
        public int TotalSpecial
        {
            get => _intTotalSpecial;
            set
            {
                if(_intTotalSpecial != value)
                {
                    _intTotalSpecial = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Attributes.
        /// </summary>
        public int Attributes
        {
            get => _intAttributes;
            set
            {
                if(_intAttributes != value)
                {
                    _intAttributes = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// TotalAttributes.
        /// </summary>
        public int TotalAttributes
        {
            get => _intTotalAttributes;
            set
            {
                if(_intTotalAttributes != value)
                {
                    _intTotalAttributes = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _intCachedCareerKarma = int.MinValue;

        /// <summary>
        /// Total amount of Karma the character has earned over the career.
        /// </summary>
        [HubTag]
        public int CareerKarma
        {
            get
            {
                if(_intCachedCareerKarma != int.MinValue)
                    return _intCachedCareerKarma;

                int intKarma = 0;

                foreach(ExpenseLogEntry objEntry in _lstExpenseLog)
                {
                    // Since we're only interested in the amount they have earned, only count values that are greater than 0 and are not refunds.
                    if(objEntry.Type == ExpenseType.Karma && (objEntry.Amount > 0 || objEntry.ForceCareerVisible) && !objEntry.Refund)
                        intKarma += decimal.ToInt32(objEntry.Amount);
                }

                return _intCachedCareerKarma = intKarma;
            }
        }

        public string DisplayCareerKarma => CareerKarma.ToString(GlobalOptions.CultureInfo);

        private decimal _decCachedCareerNuyen = decimal.MinValue;

        /// <summary>
        /// Total amount of Nuyen the character has earned over the career.
        /// </summary>
        public decimal CareerNuyen
        {
            get
            {
                if(_decCachedCareerNuyen != decimal.MinValue)
                    return _decCachedCareerNuyen;

                decimal decNuyen = 0;

                foreach(ExpenseLogEntry objEntry in _lstExpenseLog)
                {
                    // Since we're only interested in the amount they have earned, only count values that are greater than 0 and are not refunds.
                    if(objEntry.Type == ExpenseType.Nuyen && objEntry.Amount > 0 && !objEntry.Refund)
                        decNuyen += objEntry.Amount;
                }

                return _decCachedCareerNuyen = decNuyen;
            }
        }

        public string DisplayCareerNuyen =>
            CareerNuyen.ToString(Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';

        /// <summary>
        /// Whether or not the character is a Critter.
        /// </summary>
        [HubTag]
        public bool IsCritter
        {
            get => _blnIsCritter;
            set
            {
                if(_blnIsCritter != value)
                {
                    _blnIsCritter = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether or not the character is a changeling.
        /// </summary>
        [HubTag]
        public bool IsChangeling => MetagenicLimit > 0;

        /// <summary>
        /// The highest number of free Metagenic qualities the character can have.
        /// </summary>
        public int MetagenicLimit => ImprovementManager.ValueOf(this, Improvement.ImprovementType.MetageneticLimit);

        /// <summary>
        /// The highest number of free Metagenic qualities the character can have.
        /// </summary>
        public int SpecialModificationLimit => ImprovementManager.ValueOf(this, Improvement.ImprovementType.SpecialModificationLimit);

        /// <summary>
        /// Whether or not the character is possessed by a Spirit.
        /// </summary>
        public bool Possessed
        {
            get => _blnPossessed;
            set
            {
                if(_blnPossessed != value)
                {
                    _blnPossessed = value;
                    OnPropertyChanged();
                }
            }
        }

        public int SpellKarmaCost(string strCategory = "")
        {
            int intReturn = Options.KarmaSpell;

            decimal decMultiplier = 1.0m;
            foreach(Improvement objLoopImprovement in Improvements.Where(imp =>
               (imp.ImproveType == Improvement.ImprovementType.NewSpellKarmaCost ||
                imp.ImproveType == Improvement.ImprovementType.NewSpellKarmaCostMultiplier) &&
               imp.ImprovedName == strCategory))
            {
                if(objLoopImprovement.Enabled && (string.IsNullOrEmpty(objLoopImprovement.Condition) ||
                                                   (objLoopImprovement.Condition == "career") == Created ||
                                                   (objLoopImprovement.Condition == "create") != Created))
                {
                    if(objLoopImprovement.ImproveType == Improvement.ImprovementType.NewSpellKarmaCost)
                        intReturn += objLoopImprovement.Value;
                    if(objLoopImprovement.ImproveType == Improvement.ImprovementType.NewSpellKarmaCostMultiplier)
                        decMultiplier *= objLoopImprovement.Value / 100.0m;
                }
            }

            if(decMultiplier != 1.0m)
                intReturn = decimal.ToInt32(decimal.Ceiling(intReturn * decMultiplier));

            return Math.Max(intReturn, 0);
        }

        public int ComplexFormKarmaCost
        {
            get
            {
                int intReturn = Options.KarmaNewComplexForm;

                decimal decMultiplier = 1.0m;
                foreach(Improvement objLoopImprovement in Improvements)
                {
                    if(objLoopImprovement.Enabled && (string.IsNullOrEmpty(objLoopImprovement.Condition) ||
                                                       (objLoopImprovement.Condition == "career") == Created ||
                                                       (objLoopImprovement.Condition == "create") != Created))
                    {
                        if(objLoopImprovement.ImproveType == Improvement.ImprovementType.NewComplexFormKarmaCost)
                            intReturn += objLoopImprovement.Value;
                        if(objLoopImprovement.ImproveType ==
                            Improvement.ImprovementType.NewComplexFormKarmaCostMultiplier)
                            decMultiplier *= objLoopImprovement.Value / 100.0m;
                    }
                }

                if(decMultiplier != 1.0m)
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
                foreach(Improvement objLoopImprovement in Improvements)
                {
                    if(objLoopImprovement.Enabled && (string.IsNullOrEmpty(objLoopImprovement.Condition) ||
                                                       (objLoopImprovement.Condition == "career") == Created ||
                                                       (objLoopImprovement.Condition == "create") != Created))
                    {
                        if(objLoopImprovement.ImproveType == Improvement.ImprovementType.NewAIProgramKarmaCost)
                            intReturn += objLoopImprovement.Value;
                        if(objLoopImprovement.ImproveType ==
                            Improvement.ImprovementType.NewAIProgramKarmaCostMultiplier)
                            decMultiplier *= objLoopImprovement.Value / 100.0m;
                    }
                }

                if(decMultiplier != 1.0m)
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
                foreach(Improvement objLoopImprovement in Improvements)
                {
                    if(objLoopImprovement.Enabled && (string.IsNullOrEmpty(objLoopImprovement.Condition) ||
                                                       (objLoopImprovement.Condition == "career") == Created ||
                                                       (objLoopImprovement.Condition == "create") != Created))
                    {
                        if(objLoopImprovement.ImproveType == Improvement.ImprovementType.NewAIAdvancedProgramKarmaCost)
                            intReturn += objLoopImprovement.Value;
                        if(objLoopImprovement.ImproveType ==
                            Improvement.ImprovementType.NewAIAdvancedProgramKarmaCostMultiplier)
                            decMultiplier *= objLoopImprovement.Value / 100.0m;
                    }
                }

                if(decMultiplier != 1.0m)
                    intReturn = decimal.ToInt32(decimal.Ceiling(intReturn * decMultiplier));

                return Math.Max(intReturn, 0);
            }
        }

        private int _intCachedAmbidextrous = -1;

        public bool Ambidextrous
        {
            get
            {
                if(_intCachedAmbidextrous < 0)
                {
                    _intCachedAmbidextrous = Improvements.Any(x =>
                        x.Enabled && x.ImproveType == Improvement.ImprovementType.Ambidextrous)
                        ? 1
                        : 0;
                }

                return _intCachedAmbidextrous > 0;
            }
        }

        #endregion

        #region Attributes

        /// <summary>
        /// Get a CharacterAttribute by its name.
        /// </summary>
        /// <param name="strAttribute">CharacterAttribute name to retrieve.</param>
        /// <param name="blnExplicit">Whether to force looking for a specific attribute name.
        /// Mostly expected to be used for gutting Mystic Adept power points.</param>
        public CharacterAttrib GetAttribute(string strAttribute, bool blnExplicit = false)
        {
            if(strAttribute == "MAGAdept" && (!IsMysticAdept || !Options.MysAdeptSecondMAGAttribute) && !blnExplicit)
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
        /// Reflection of MAG (hide it, if it is not enabled!)
        /// </summary>
        [HubTag("Magic")]
        public CharacterAttrib ReflectionMAG => MAGEnabled ? MAG : null;

        /// <summary>
        /// Magic (MAG) CharacterAttribute for Adept powers of Mystic Adepts when the appropriate house rule is enabled.
        /// </summary>
        public CharacterAttrib MAGAdept
        {
            get
            {
                if(Options.MysAdeptSecondMAGAttribute && IsMysticAdept)
                    return AttributeSection.GetAttributeByName("MAGAdept");
                return MAG;
            }
        }

        /// <summary>
        /// Reflection of MAGAdept (hide it, if it is not enabled!)
        /// </summary>
        [HubTag("MagicAdept")]
        public CharacterAttrib ReflectionMAGAdept => MAGEnabled ? MAGAdept : null;

        /// <summary>
        /// Resonance (RES) CharacterAttribute.
        /// </summary>
        public CharacterAttrib RES => AttributeSection.GetAttributeByName("RES");

        /// <summary>
        /// Reflection of RES (hide it, if it is not enabled!)
        /// </summary>
        [HubTag("Resonance")]
        public CharacterAttrib ReflectionRES => RESEnabled ? RES : null;

        /// <summary>
        /// Depth (DEP) Attribute.
        /// </summary>
        public CharacterAttrib DEP => AttributeSection.GetAttributeByName("DEP");

        /// <summary>
        /// Reflection of DEP (hide it, if it is not enabled!)
        /// </summary>
        [HubTag("Depth")]
        public CharacterAttrib ReflectionDEP => DEPEnabled ? DEP : null;

        /// <summary>
        /// Essence (ESS) Attribute.
        /// </summary>
        public CharacterAttrib ESS => AttributeSection.GetAttributeByName("ESS");

        /// <summary>
        /// Is the MAG CharacterAttribute enabled?
        /// </summary>
        [HubTag]
        public bool MAGEnabled
        {
            get => _blnMAGEnabled;
            set
            {
                if(_blnMAGEnabled != value)
                {
                    _blnMAGEnabled = value;
                    if(value)
                    {
                        // Career mode, so no extra calculations need to be done for EssenceAtSpecialStart
                        if(Created)
                        {
                            ResetCachedEssence();
                            EssenceAtSpecialStart = Essence(true);
                        }
                        // EssenceAtSpecialStart needs to be calculated by assuming that the character took the MAG-enabling quality with the highest essence penalty first, as that would be the most optimal
                        else
                        {
                            // If this character has any MAG-enabling bonuses that could be granted before all others (because they're priority and/or metatype-given),
                            // it has to be assumed those are taken first.
                            bool blnCountOnlyPriorityOrMetatypeGivenBonuses = Improvements.Any(x =>
                                x.ImproveType == Improvement.ImprovementType.Attribute
                                && x.ImprovedName == "MAG"
                                && (x.ImproveSource == Improvement.ImprovementSource.Metatype
                                    || x.ImproveSource == Improvement.ImprovementSource.Metavariant
                                    || x.ImproveSource == Improvement.ImprovementSource.Heritage)
                                && x.Enabled);
                            if (!blnCountOnlyPriorityOrMetatypeGivenBonuses)
                            {
                                List<string> lstMAGEnablingQualityIds = Improvements.Where(x =>
                                    x.ImproveType == Improvement.ImprovementType.Attribute
                                    && x.ImprovedName == "MAG"
                                    && x.ImproveSource == Improvement.ImprovementSource.Quality
                                    && x.Enabled).Select(x => x.SourceName).ToList();
                                // Can't use foreach because new items can get added to this list while it is looping
                                for (int i = 0; i < lstMAGEnablingQualityIds.Count; ++i)
                                {
                                    Quality objQuality = Qualities.FirstOrDefault(x => x.InternalId == lstMAGEnablingQualityIds[i]);
                                    if (objQuality != null)
                                    {
                                        if (objQuality.OriginSource == QualitySource.Metatype
                                            || objQuality.OriginSource == QualitySource.MetatypeRemovable
                                            || objQuality.OriginSource == QualitySource.BuiltIn
                                            || objQuality.OriginSource == QualitySource.LifeModule
                                            || objQuality.OriginSource == QualitySource.Heritage)
                                        {
                                            blnCountOnlyPriorityOrMetatypeGivenBonuses = true;
                                            break;
                                        }
                                        else if (objQuality.OriginSource == QualitySource.Improvement)
                                        {
                                            if (Improvements.Any(x =>
                                                x.ImproveType == Improvement.ImprovementType.SpecificQuality
                                                && x.ImprovedName == objQuality.InternalId
                                                && (x.ImproveSource == Improvement.ImprovementSource.Metatype
                                                    || x.ImproveSource == Improvement.ImprovementSource.Metavariant
                                                    || x.ImproveSource == Improvement.ImprovementSource.Heritage)
                                                && x.Enabled))
                                            {
                                                blnCountOnlyPriorityOrMetatypeGivenBonuses = true;
                                                break;
                                            }
                                            // Qualities that add other qualities get added to the list to be checked, too
                                            lstMAGEnablingQualityIds.AddRange(Improvements.Where(x =>
                                                x.ImproveType == Improvement.ImprovementType.SpecificQuality
                                                && x.ImprovedName == objQuality.InternalId
                                                && x.ImproveSource == Improvement.ImprovementSource.Quality
                                                && x.Enabled).Select(x => x.SourceName));
                                        }
                                    }
                                }
                            }
                            Dictionary<string, decimal> dicImprovementEssencePenalties =
                                new Dictionary<string, decimal>();
                            foreach(Improvement objImprovement in Improvements)
                            {
                                if (!objImprovement.Enabled)
                                    continue;
                                bool blnCountImprovement = !blnCountOnlyPriorityOrMetatypeGivenBonuses
                                                           || objImprovement.ImproveSource == Improvement.ImprovementSource.Metatype
                                                           || objImprovement.ImproveSource == Improvement.ImprovementSource.Metavariant
                                                           || objImprovement.ImproveSource == Improvement.ImprovementSource.Heritage;
                                if (!blnCountImprovement)
                                {
                                    if (objImprovement.ImproveSource == Improvement.ImprovementSource.Quality)
                                    {
                                        Quality objQuality = Qualities.FirstOrDefault(x => x.InternalId == objImprovement.SourceName);
                                        while (objQuality != null)
                                        {
                                            if (objQuality.OriginSource == QualitySource.Metatype
                                                || objQuality.OriginSource == QualitySource.MetatypeRemovable
                                                || objQuality.OriginSource == QualitySource.BuiltIn
                                                || objQuality.OriginSource == QualitySource.LifeModule
                                                || objQuality.OriginSource == QualitySource.Heritage)
                                            {
                                                blnCountImprovement = true;
                                                break;
                                            }
                                            else if (objQuality.OriginSource == QualitySource.Improvement)
                                            {
                                                Improvement objParentImprovement = Improvements.FirstOrDefault(x =>
                                                    x.ImproveType == Improvement.ImprovementType.SpecificQuality
                                                    && x.ImprovedName == objQuality.InternalId
                                                    && x.Enabled);
                                                if (objParentImprovement == null)
                                                    break;
                                                if (objParentImprovement.ImproveSource == Improvement.ImprovementSource.Metatype
                                                    || objParentImprovement.ImproveSource == Improvement.ImprovementSource.Metavariant
                                                    || objParentImprovement.ImproveSource == Improvement.ImprovementSource.Heritage)
                                                {
                                                    blnCountImprovement = true;
                                                    break;
                                                }
                                                if (objParentImprovement.ImproveSource == Improvement.ImprovementSource.Quality)
                                                {
                                                    // Qualities that add other qualities get added to the list to be checked, too
                                                    objQuality = Qualities.FirstOrDefault(x => x.InternalId == objParentImprovement.SourceName);
                                                }
                                                else
                                                    break;
                                            }
                                        }
                                    }
                                }
                                if (blnCountImprovement)
                                {
                                    decimal decLoopEssencePenalty = 0;
                                    if(objImprovement.ImproveType == Improvement.ImprovementType.EssencePenalty)
                                    {
                                        decLoopEssencePenalty += objImprovement.Value;
                                    }
                                    else if(objImprovement.ImproveType == Improvement.ImprovementType.EssencePenaltyT100
                                            || objImprovement.ImproveType == Improvement.ImprovementType.EssencePenaltyMAGOnlyT100)
                                    {
                                        decLoopEssencePenalty += Convert.ToDecimal(objImprovement.Value) / 100.0m;
                                    }

                                    if(decLoopEssencePenalty != 0)
                                    {
                                        if(dicImprovementEssencePenalties.ContainsKey(objImprovement.SourceName))
                                            dicImprovementEssencePenalties[objImprovement.SourceName] =
                                                dicImprovementEssencePenalties[objImprovement.SourceName] +
                                                decLoopEssencePenalty;
                                        else
                                            dicImprovementEssencePenalties.Add(objImprovement.SourceName,
                                                decLoopEssencePenalty);
                                    }
                                }
                            }

                            if(dicImprovementEssencePenalties.Count > 0)
                                EssenceAtSpecialStart =
                                    ESS.MetatypeMaximum + dicImprovementEssencePenalties.Values.Min();
                            else
                                EssenceAtSpecialStart = ESS.MetatypeMaximum;
                        }
                    }
                    else
                    {
                        if(!RESEnabled)
                        {
                            ClearInitiations();
                            MagicTradition.ResetTradition();
                        }
                        else
                        {
                            XmlNode xmlTraditionListDataNode = LoadData("streams.xml").SelectSingleNode("/chummer/traditions");
                            if(xmlTraditionListDataNode != null)
                            {
                                XmlNode xmlTraditionDataNode = xmlTraditionListDataNode.SelectSingleNode("tradition[name = \"Default\"]");
                                if(xmlTraditionDataNode != null)
                                {
                                    if(!MagicTradition.Create(xmlTraditionDataNode, true))
                                        MagicTradition.ResetTradition();
                                }
                                else
                                    MagicTradition.ResetTradition();
                            }
                            else
                                MagicTradition.ResetTradition();
                        }
                        if(!Created && !RESEnabled && !DEPEnabled)
                            EssenceAtSpecialStart = decimal.MinValue;
                    }

                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Maximum force of spirits summonable/bindable by the character. Limited to MAG at creation.
        /// </summary>
        public int MaxSpiritForce =>
            ((Options.SpiritForceBasedOnTotalMAG ? MAG.TotalValue : MAG.Value) > 0)
                ? (Created ? 2 : 1) * (Options.SpiritForceBasedOnTotalMAG ? MAG.TotalValue : MAG.Value)
                : 0;

        /// <summary>
        /// Maximum level of sprites compilable/registrable by the character. Limited to RES at creation.
        /// </summary>
        public int MaxSpriteLevel => RES.TotalValue > 0 ? (Created ? 2 : 1) * RES.TotalValue : 0;

        /// <summary>
        /// Amount of Power Points for Mystic Adepts.
        /// </summary>
        public int MysticAdeptPowerPoints
        {
            get => _intMAGAdept;
            set
            {
                int intNewValue = Math.Min(value, MAG.TotalValue);
                if(_intMAGAdept != intNewValue)
                {
                    _intMAGAdept = intNewValue;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Total Amount of Power Points this character has.
        /// </summary>
        public int PowerPointsTotal
        {
            get
            {
                int intMAG = UseMysticAdeptPPs ? MysticAdeptPowerPoints : MAGAdept.TotalValue;

                // Add any Power Point Improvements to MAG.
                intMAG += ImprovementManager.ValueOf(this, Improvement.ImprovementType.AdeptPowerPoints);

                return Math.Max(intMAG, 0);
            }
        }

        private decimal _decCachedPowerPointsUsed = decimal.MinValue;

        public decimal PowerPointsUsed
        {
            get
            {
                if(_decCachedPowerPointsUsed != decimal.MinValue)
                    return _decCachedPowerPointsUsed;
                return _decCachedPowerPointsUsed = Powers.AsParallel().Sum(objPower => objPower.PowerPoints);
            }
        }

        public string DisplayPowerPointsRemaining
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                return PowerPointsTotal.ToString(GlobalOptions.CultureInfo) + strSpace + '(' +
                       (PowerPointsTotal - PowerPointsUsed).ToString(GlobalOptions.CultureInfo) + strSpace +
                       LanguageManager.GetString("String_Remaining") + ')';
            }
        }

        public bool AnyPowerAdeptWayDiscountEnabled => Powers.Any(objPower => objPower.AdeptWayDiscountEnabled);

        /// <summary>
        /// Magician's Tradition.
        /// </summary>
        [HubTag("Tradition", "", "MagicTradition", false)]
        public Tradition MagicTradition
        {
            get => _objTradition;
            set => _objTradition = value;
        }


        /// <summary>
        /// Initiate Grade.
        /// </summary>
        [HubTag]
        public int InitiateGrade
        {
            get => _intInitiateGrade;
            set
            {
                if(_intInitiateGrade != value)
                {
                    _intInitiateGrade = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Is the RES CharacterAttribute enabled?
        /// </summary>
        [HubTag]
        public bool RESEnabled
        {
            get => _blnRESEnabled;
            set
            {
                if(_blnRESEnabled != value)
                {
                    _blnRESEnabled = value;
                    if(value)
                    {
                        // Career mode, so no extra calculations need to be done for EssenceAtSpecialStart
                        if(Created)
                        {
                            ResetCachedEssence();
                            EssenceAtSpecialStart = Essence();
                        }
                        // EssenceAtSpecialStart needs to be calculated by assuming that the character took the RES-enabling quality with the highest essence penalty first, as that would be the most optimal
                        else
                        {
                            // If this character has any RES-enabling bonuses that could be granted before all others (because they're priority and/or metatype-given),
                            // it has to be assumed those are taken first.
                            bool blnCountOnlyPriorityOrMetatypeGivenBonuses = Improvements.Any(x =>
                                x.ImproveType == Improvement.ImprovementType.Attribute
                                && x.ImprovedName == "RES"
                                && (x.ImproveSource == Improvement.ImprovementSource.Metatype
                                    || x.ImproveSource == Improvement.ImprovementSource.Metavariant
                                    || x.ImproveSource == Improvement.ImprovementSource.Heritage)
                                && x.Enabled);
                            if (!blnCountOnlyPriorityOrMetatypeGivenBonuses)
                            {
                                List<string> lstRESEnablingQualityIds = Improvements.Where(x =>
                                    x.ImproveType == Improvement.ImprovementType.Attribute
                                    && x.ImprovedName == "RES"
                                    && x.ImproveSource == Improvement.ImprovementSource.Quality
                                    && x.Enabled).Select(x => x.SourceName).ToList();
                                // Can't use foreach because new items can get added to this list while it is looping
                                for (int i = 0; i < lstRESEnablingQualityIds.Count; ++i)
                                {
                                    Quality objQuality = Qualities.FirstOrDefault(x => x.InternalId == lstRESEnablingQualityIds[i]);
                                    if (objQuality != null)
                                    {
                                        if (objQuality.OriginSource == QualitySource.Metatype
                                            || objQuality.OriginSource == QualitySource.MetatypeRemovable
                                            || objQuality.OriginSource == QualitySource.BuiltIn
                                            || objQuality.OriginSource == QualitySource.LifeModule
                                            || objQuality.OriginSource == QualitySource.Heritage)
                                        {
                                            blnCountOnlyPriorityOrMetatypeGivenBonuses = true;
                                            break;
                                        }
                                        else if (objQuality.OriginSource == QualitySource.Improvement)
                                        {
                                            if (Improvements.Any(x =>
                                                x.ImproveType == Improvement.ImprovementType.SpecificQuality
                                                && x.ImprovedName == objQuality.InternalId
                                                && (x.ImproveSource == Improvement.ImprovementSource.Metatype
                                                    || x.ImproveSource == Improvement.ImprovementSource.Metavariant
                                                    || x.ImproveSource == Improvement.ImprovementSource.Heritage)
                                                && x.Enabled))
                                            {
                                                blnCountOnlyPriorityOrMetatypeGivenBonuses = true;
                                                break;
                                            }
                                            // Qualities that add other qualities get added to the list to be checked, too
                                            lstRESEnablingQualityIds.AddRange(Improvements.Where(x =>
                                                x.ImproveType == Improvement.ImprovementType.SpecificQuality
                                                && x.ImprovedName == objQuality.InternalId
                                                && x.ImproveSource == Improvement.ImprovementSource.Quality
                                                && x.Enabled).Select(x => x.SourceName));
                                        }
                                    }
                                }
                            }
                            Dictionary<string, decimal> dicImprovementEssencePenalties =
                                new Dictionary<string, decimal>();
                            foreach (Improvement objImprovement in Improvements)
                            {
                                if (!objImprovement.Enabled)
                                    continue;
                                bool blnCountImprovement = !blnCountOnlyPriorityOrMetatypeGivenBonuses
                                                           || objImprovement.ImproveSource == Improvement.ImprovementSource.Metatype
                                                           || objImprovement.ImproveSource == Improvement.ImprovementSource.Metavariant
                                                           || objImprovement.ImproveSource == Improvement.ImprovementSource.Heritage;
                                if (!blnCountImprovement)
                                {
                                    if (objImprovement.ImproveSource == Improvement.ImprovementSource.Quality)
                                    {
                                        Quality objQuality = Qualities.FirstOrDefault(x => x.InternalId == objImprovement.SourceName);
                                        while (objQuality != null)
                                        {
                                            if (objQuality.OriginSource == QualitySource.Metatype
                                                || objQuality.OriginSource == QualitySource.MetatypeRemovable
                                                || objQuality.OriginSource == QualitySource.BuiltIn
                                                || objQuality.OriginSource == QualitySource.LifeModule
                                                || objQuality.OriginSource == QualitySource.Heritage)
                                            {
                                                blnCountImprovement = true;
                                                break;
                                            }
                                            else if (objQuality.OriginSource == QualitySource.Improvement)
                                            {
                                                Improvement objParentImprovement = Improvements.FirstOrDefault(x =>
                                                    x.ImproveType == Improvement.ImprovementType.SpecificQuality
                                                    && x.ImprovedName == objQuality.InternalId
                                                    && x.Enabled);
                                                if (objParentImprovement == null)
                                                    break;
                                                if (objParentImprovement.ImproveSource == Improvement.ImprovementSource.Metatype
                                                    || objParentImprovement.ImproveSource == Improvement.ImprovementSource.Metavariant
                                                    || objParentImprovement.ImproveSource == Improvement.ImprovementSource.Heritage)
                                                {
                                                    blnCountImprovement = true;
                                                    break;
                                                }
                                                if (objParentImprovement.ImproveSource == Improvement.ImprovementSource.Quality)
                                                {
                                                    // Qualities that add other qualities get added to the list to be checked, too
                                                    objQuality = Qualities.FirstOrDefault(x => x.InternalId == objParentImprovement.SourceName);
                                                }
                                                else
                                                    break;
                                            }
                                        }
                                    }
                                }
                                if (blnCountImprovement)
                                {
                                    decimal decLoopEssencePenalty = 0;
                                    if(objImprovement.ImproveType == Improvement.ImprovementType.EssencePenalty)
                                    {
                                        decLoopEssencePenalty += objImprovement.Value;
                                    }
                                    else if(objImprovement.ImproveType == Improvement.ImprovementType.EssencePenaltyT100)
                                    {
                                        decLoopEssencePenalty += Convert.ToDecimal(objImprovement.Value) / 100.0m;
                                    }

                                    if(decLoopEssencePenalty != 0)
                                    {
                                        if(dicImprovementEssencePenalties.ContainsKey(objImprovement.SourceName))
                                            dicImprovementEssencePenalties[objImprovement.SourceName] =
                                                dicImprovementEssencePenalties[objImprovement.SourceName] +
                                                decLoopEssencePenalty;
                                        else
                                            dicImprovementEssencePenalties.Add(objImprovement.SourceName,
                                                decLoopEssencePenalty);
                                    }
                                }
                            }

                            if(dicImprovementEssencePenalties.Count > 0)
                                EssenceAtSpecialStart =
                                    ESS.MetatypeMaximum + dicImprovementEssencePenalties.Values.Min();
                            else
                                EssenceAtSpecialStart = ESS.MetatypeMaximum;
                        }

                        XmlNode xmlTraditionListDataNode = LoadData("streams.xml").SelectSingleNode("/chummer/traditions");
                        if(xmlTraditionListDataNode != null)
                        {
                            XmlNode xmlTraditionDataNode = xmlTraditionListDataNode.SelectSingleNode("tradition[name = \"Default\"]");
                            if(xmlTraditionDataNode != null)
                            {
                                if(!MagicTradition.Create(xmlTraditionDataNode, true))
                                    MagicTradition.ResetTradition();
                            }
                            else
                            {
                                xmlTraditionDataNode = xmlTraditionListDataNode.SelectSingleNode("tradition");
                                if(xmlTraditionDataNode != null)
                                {
                                    if(!MagicTradition.Create(xmlTraditionDataNode, true))
                                        MagicTradition.ResetTradition();
                                }
                            }
                        }
                    }
                    else
                    {
                        if(!MAGEnabled)
                        {
                            ClearInitiations();
                            MagicTradition.ResetTradition();
                        }
                        else
                        {
                            XmlNode xmlTraditionListDataNode = LoadData("traditions.xml").SelectSingleNode("/chummer/traditions");
                            if(xmlTraditionListDataNode != null)
                            {
                                XmlNode xmlTraditionDataNode = xmlTraditionListDataNode.SelectSingleNode("tradition[id = \"" + Tradition.CustomMagicalTraditionGuid + "\"]");
                                if(xmlTraditionDataNode != null)
                                {
                                    if(!MagicTradition.Create(xmlTraditionDataNode))
                                        MagicTradition.ResetTradition();
                                }
                                else
                                    MagicTradition.ResetTradition();
                            }
                            else
                                MagicTradition.ResetTradition();
                        }
                        if(!Created && !DEPEnabled && !MAGEnabled)
                            EssenceAtSpecialStart = decimal.MinValue;
                    }

                    ImprovementManager.ClearCachedValue(this, Improvement.ImprovementType.MatrixInitiativeDice);
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Is the DEP CharacterAttribute enabled?
        /// </summary>
        [HubTag]
        public bool DEPEnabled
        {
            get => _blnDEPEnabled;
            set
            {
                if(_blnDEPEnabled != value)
                {
                    _blnDEPEnabled = value;
                    if(value)
                    {
                        // Career mode, so no extra calculations need to be done for EssenceAtSpecialStart
                        if(Created)
                        {
                            ResetCachedEssence();
                            EssenceAtSpecialStart = Essence();
                        }
                        // EssenceAtSpecialStart needs to be calculated by assuming that the character took the DEP-enabling quality with the highest essence penalty first, as that would be the most optimal
                        else
                        {
                            // If this character has any DEP-enabling bonuses that could be granted before all others (because they're priority and/or metatype-given),
                            // it has to be assumed those are taken first.
                            bool blnCountOnlyPriorityOrMetatypeGivenBonuses = Improvements.Any(x =>
                                x.ImproveType == Improvement.ImprovementType.Attribute
                                && x.ImprovedName == "DEP"
                                && (x.ImproveSource == Improvement.ImprovementSource.Metatype
                                    || x.ImproveSource == Improvement.ImprovementSource.Metavariant
                                    || x.ImproveSource == Improvement.ImprovementSource.Heritage)
                                && x.Enabled);
                            if (!blnCountOnlyPriorityOrMetatypeGivenBonuses)
                            {
                                List<string> lstDEPEnablingQualityIds = Improvements.Where(x =>
                                    x.ImproveType == Improvement.ImprovementType.Attribute
                                    && x.ImprovedName == "DEP"
                                    && x.ImproveSource == Improvement.ImprovementSource.Quality
                                    && x.Enabled).Select(x => x.SourceName).ToList();
                                // Can't use foreach because new items can get added to this list while it is looping
                                for (int i = 0; i < lstDEPEnablingQualityIds.Count; ++i)
                                {
                                    Quality objQuality = Qualities.FirstOrDefault(x => x.InternalId == lstDEPEnablingQualityIds[i]);
                                    if (objQuality != null)
                                    {
                                        if (objQuality.OriginSource == QualitySource.Metatype
                                            || objQuality.OriginSource == QualitySource.MetatypeRemovable
                                            || objQuality.OriginSource == QualitySource.BuiltIn
                                            || objQuality.OriginSource == QualitySource.LifeModule
                                            || objQuality.OriginSource == QualitySource.Heritage)
                                        {
                                            blnCountOnlyPriorityOrMetatypeGivenBonuses = true;
                                            break;
                                        }
                                        else if (objQuality.OriginSource == QualitySource.Improvement)
                                        {
                                            if (Improvements.Any(x =>
                                                x.ImproveType == Improvement.ImprovementType.SpecificQuality
                                                && x.ImprovedName == objQuality.InternalId
                                                && (x.ImproveSource == Improvement.ImprovementSource.Metatype
                                                    || x.ImproveSource == Improvement.ImprovementSource.Metavariant
                                                    || x.ImproveSource == Improvement.ImprovementSource.Heritage)
                                                && x.Enabled))
                                            {
                                                blnCountOnlyPriorityOrMetatypeGivenBonuses = true;
                                                break;
                                            }
                                            // Qualities that add other qualities get added to the list to be checked, too
                                            lstDEPEnablingQualityIds.AddRange(Improvements.Where(x =>
                                                x.ImproveType == Improvement.ImprovementType.SpecificQuality
                                                && x.ImprovedName == objQuality.InternalId
                                                && x.ImproveSource == Improvement.ImprovementSource.Quality
                                                && x.Enabled).Select(x => x.SourceName));
                                        }
                                    }
                                }
                            }
                            Dictionary<string, decimal> dicImprovementEssencePenalties =
                                new Dictionary<string, decimal>();
                            foreach (Improvement objImprovement in Improvements)
                            {
                                if (!objImprovement.Enabled)
                                    continue;
                                bool blnCountImprovement = !blnCountOnlyPriorityOrMetatypeGivenBonuses
                                                           || objImprovement.ImproveSource == Improvement.ImprovementSource.Metatype
                                                           || objImprovement.ImproveSource == Improvement.ImprovementSource.Metavariant
                                                           || objImprovement.ImproveSource == Improvement.ImprovementSource.Heritage;
                                if (!blnCountImprovement)
                                {
                                    if (objImprovement.ImproveSource == Improvement.ImprovementSource.Quality)
                                    {
                                        Quality objQuality = Qualities.FirstOrDefault(x => x.InternalId == objImprovement.SourceName);
                                        while (objQuality != null)
                                        {
                                            if (objQuality.OriginSource == QualitySource.Metatype
                                                || objQuality.OriginSource == QualitySource.MetatypeRemovable
                                                || objQuality.OriginSource == QualitySource.BuiltIn
                                                || objQuality.OriginSource == QualitySource.LifeModule
                                                || objQuality.OriginSource == QualitySource.Heritage)
                                            {
                                                blnCountImprovement = true;
                                                break;
                                            }
                                            else if (objQuality.OriginSource == QualitySource.Improvement)
                                            {
                                                Improvement objParentImprovement = Improvements.FirstOrDefault(x =>
                                                    x.ImproveType == Improvement.ImprovementType.SpecificQuality
                                                    && x.ImprovedName == objQuality.InternalId
                                                    && x.Enabled);
                                                if (objParentImprovement == null)
                                                    break;
                                                if (objParentImprovement.ImproveSource == Improvement.ImprovementSource.Metatype
                                                    || objParentImprovement.ImproveSource == Improvement.ImprovementSource.Metavariant
                                                    || objParentImprovement.ImproveSource == Improvement.ImprovementSource.Heritage)
                                                {
                                                    blnCountImprovement = true;
                                                    break;
                                                }
                                                if (objParentImprovement.ImproveSource == Improvement.ImprovementSource.Quality)
                                                {
                                                    // Qualities that add other qualities get added to the list to be checked, too
                                                    objQuality = Qualities.FirstOrDefault(x => x.InternalId == objParentImprovement.SourceName);
                                                }
                                                else
                                                    break;
                                            }
                                        }
                                    }
                                }
                                if (blnCountImprovement)
                                {
                                    decimal decLoopEssencePenalty = 0;
                                    if(objImprovement.ImproveType == Improvement.ImprovementType.EssencePenalty)
                                    {
                                        decLoopEssencePenalty += objImprovement.Value;
                                    }
                                    else if(objImprovement.ImproveType == Improvement.ImprovementType.EssencePenaltyT100)
                                    {
                                        decLoopEssencePenalty += Convert.ToDecimal(objImprovement.Value) / 100.0m;
                                    }

                                    if(decLoopEssencePenalty != 0)
                                    {
                                        if(dicImprovementEssencePenalties.ContainsKey(objImprovement.SourceName))
                                            dicImprovementEssencePenalties[objImprovement.SourceName] =
                                                dicImprovementEssencePenalties[objImprovement.SourceName] +
                                                decLoopEssencePenalty;
                                        else
                                            dicImprovementEssencePenalties.Add(objImprovement.SourceName,
                                                decLoopEssencePenalty);
                                    }
                                }
                            }

                            if(dicImprovementEssencePenalties.Count > 0)
                                EssenceAtSpecialStart =
                                    ESS.MetatypeMaximum + dicImprovementEssencePenalties.Values.Min();
                            else
                                EssenceAtSpecialStart = ESS.MetatypeMaximum;
                        }
                    }
                    else if(!Created && !RESEnabled && !MAGEnabled)
                        EssenceAtSpecialStart = decimal.MinValue;

                    OnPropertyChanged();
                }
            }
        }

        [HubTag]
        public bool IsAI => DEPEnabled && BOD.MetatypeMaximum == 0;

        /// <summary>
        /// Submersion Grade.
        /// </summary>
        [HubTag]
        public int SubmersionGrade
        {
            get => _intSubmersionGrade;
            set
            {
                if(_intSubmersionGrade != value)
                {
                    _intSubmersionGrade = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether or not the character is a member of a Group or Network.
        /// </summary>
        public bool GroupMember
        {
            get => _blnGroupMember;
            set
            {
                if(_blnGroupMember != value)
                {
                    _blnGroupMember = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The name of the Group the Initiate has joined.
        /// </summary>
        [HubTag]
        public string GroupName
        {
            get => _strGroupName;
            set
            {
                if(_strGroupName != value)
                {
                    _strGroupName = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Notes for the Group the Initiate has joined.
        /// </summary>
        public string GroupNotes
        {
            get => _strGroupNotes;
            set
            {
                if(_strGroupNotes != value)
                {
                    _strGroupNotes = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Essence the character had when the first gained access to MAG/RES.
        /// </summary>
        public decimal EssenceAtSpecialStart
        {
            get => _decEssenceAtSpecialStart;
            set
            {
                if(_decEssenceAtSpecialStart != value)
                {
                    _decEssenceAtSpecialStart = value;
                    RefreshEssenceLossImprovements();
                }
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
        /// <param name="blnForMAGPenalty">Whether fetched Essence is to be used to calculate the penalty MAG should receive from lost Essence (true) or not (false).</param>
        public decimal Essence(bool blnForMAGPenalty = false)
        {
            if(!blnForMAGPenalty && _decCachedEssence != decimal.MinValue)
                return _decCachedEssence;
            // If the character has a fixed Essence Improvement, permanently fix their Essence at its value.
            if(_lstImprovements.Any(objImprovement =>
               objImprovement.ImproveType == Improvement.ImprovementType.CyborgEssence && objImprovement.Enabled))
            {
                if(blnForMAGPenalty)
                    return 0.1m;
                return _decCachedEssence = 0.1m;
            }

            decimal decESS = ESS.MetatypeMaximum;
            decESS += Convert.ToDecimal(ImprovementManager.ValueOf(this, Improvement.ImprovementType.EssencePenalty));
            decESS += Convert.ToDecimal(
                          ImprovementManager.ValueOf(this, Improvement.ImprovementType.EssencePenaltyT100)) / 100.0m;
            if(blnForMAGPenalty)
                decESS += Convert.ToDecimal(ImprovementManager.ValueOf(this,
                              Improvement.ImprovementType.EssencePenaltyMAGOnlyT100)) / 100.0m;

            // Run through all of the pieces of Cyberware and include their Essence cost.
            decESS -= Cyberware.AsParallel().Sum(objCyberware => objCyberware.CalculatedESS);

            //1781 Essence is not printing
            //ESS.Base = Convert.ToInt32(decESS); -- Disabled because this messes up Character Validity, and it really shouldn't be what "Base" of an attribute is supposed to be (it's supposed to be extra levels gained)

            if(blnForMAGPenalty)
                return decESS;
            return _decCachedEssence = decESS;
        }

        private decimal _decCachedCyberwareEssence = decimal.MinValue;

        /// <summary>
        /// Essence consumed by Cyberware.
        /// </summary>
        public decimal CyberwareEssence
        {
            get
            {
                if(_decCachedCyberwareEssence != decimal.MinValue)
                    return _decCachedCyberwareEssence;
                // Run through all of the pieces of Cyberware and include their Essence cost. Cyberware and Bioware costs are calculated separately.
                return _decCachedCyberwareEssence = Cyberware
                    .Where(objCyberware => !objCyberware.SourceID.Equals(Backend.Equipment.Cyberware.EssenceHoleGUID)
                                           && !objCyberware.SourceID.Equals(Backend.Equipment.Cyberware.EssenceAntiHoleGUID)
                                           && objCyberware.SourceType == Improvement.ImprovementSource.Cyberware)
                    .AsParallel().Sum(objCyberware => objCyberware.CalculatedESS);
            }
        }

        private decimal _decCachedBiowareEssence = decimal.MinValue;

        /// <summary>
        /// Essence consumed by Bioware.
        /// </summary>
        public decimal BiowareEssence
        {
            get
            {
                if(_decCachedBiowareEssence != decimal.MinValue)
                    return _decCachedBiowareEssence;
                // Run through all of the pieces of Cyberware and include their Essence cost. Cyberware and Bioware costs are calculated separately.
                return _decCachedBiowareEssence = Cyberware
                    .Where(objCyberware => !objCyberware.SourceID.Equals(Backend.Equipment.Cyberware.EssenceHoleGUID)
                                           && !objCyberware.SourceID.Equals(Backend.Equipment.Cyberware.EssenceAntiHoleGUID)
                                           && objCyberware.SourceType == Improvement.ImprovementSource.Bioware)
                    .AsParallel().Sum(objCyberware => objCyberware.CalculatedESS);
            }
        }

        private decimal _decCachedEssenceHole = decimal.MinValue;

        /// <summary>
        /// Essence consumed by Essence Holes.
        /// </summary>
        public decimal EssenceHole
        {
            get
            {
                if(_decCachedEssenceHole != decimal.MinValue)
                    return _decCachedEssenceHole;
                // Find the total Essence Cost of all Essence Hole objects.
                return _decCachedEssenceHole = Cyberware
                    .Where(objCyberware => objCyberware.SourceID.Equals(Backend.Equipment.Cyberware.EssenceHoleGUID)
                                           || objCyberware.SourceID.Equals(Backend.Equipment.Cyberware.EssenceAntiHoleGUID))
                    .AsParallel().Sum(objCyberware => objCyberware.CalculatedESS);
            }
        }

        public void IncreaseEssenceHole(int intCentiessence, bool blnOverflowIntoHole = true)
        {
            Cyberware objAntiHole = Cyberware.FirstOrDefault(x => x.SourceID == Backend.Equipment.Cyberware.EssenceAntiHoleGUID);
            if(objAntiHole != null)
            {
                if(objAntiHole.Rating > intCentiessence)
                {
                    objAntiHole.Rating -= intCentiessence;
                    return;
                }

                intCentiessence -= objAntiHole.Rating;
                objAntiHole.DeleteCyberware();
                Cyberware.Remove(objAntiHole);
            }

            if(blnOverflowIntoHole)
            {
                Cyberware objHole = Cyberware.FirstOrDefault(x => x.SourceID == Backend.Equipment.Cyberware.EssenceHoleGUID);
                if(objHole == null)
                {
                    XmlNode xmlEssHole = LoadData("cyberware.xml").SelectSingleNode("/chummer/cyberwares/cyberware[id = \"" + Backend.Equipment.Cyberware.EssenceHoleGUID.ToString("D", GlobalOptions.InvariantCultureInfo) + "\"]");
                    objHole = new Cyberware(this);
                    List<Weapon> lstWeapons = new List<Weapon>(1);
                    List<Vehicle> lstVehicles = new List<Vehicle>(1);
                    objHole.Create(xmlEssHole, GetGradeList(Improvement.ImprovementSource.Cyberware, true).FirstOrDefault(x => x.Name == "None"), Improvement.ImprovementSource.Cyberware, intCentiessence, lstWeapons,
                        lstVehicles);

                    Cyberware.Add(objHole);

                    foreach(Weapon objWeapon in lstWeapons)
                    {
                        Weapons.Add(objWeapon);
                    }

                    foreach(Vehicle objVehicle in lstVehicles)
                    {
                        Vehicles.Add(objVehicle);
                    }
                }
                else
                {
                    objHole.Rating += intCentiessence;
                }

                if(objHole.Rating == 0 && Cyberware.Contains(objHole))
                    Cyberware.Remove(objHole);
            }

            if(objAntiHole?.Rating == 0 && Cyberware.Contains(objAntiHole))
                Cyberware.Remove(objAntiHole);
        }
        /// <summary>
        /// Decrease or create an Essence Hole, if required.
        /// </summary>
        /// <param name="intCentiessence">Hundredths of Essence to push into a new Essence Hole or Antihole.</param>
        /// <param name="blnOverflowIntoAntiHole">Should we increase or create an Essence Antihole to handle any overflow. Remember, Essence Holes are consumed first.</param>
        public void DecreaseEssenceHole(int intCentiessence, bool blnOverflowIntoAntiHole = true)
        {
            Cyberware objHole = Cyberware.FirstOrDefault(x => x.SourceID == Backend.Equipment.Cyberware.EssenceHoleGUID);

            if(objHole != null)
            {
                if(objHole.Rating > intCentiessence)
                {
                    objHole.Rating -= intCentiessence;
                    return;
                }

                intCentiessence -= objHole.Rating;
                objHole.DeleteCyberware();
                Cyberware.Remove(objHole);
            }

            if(blnOverflowIntoAntiHole && intCentiessence != 0)
            {
                Cyberware objAntiHole = Cyberware.FirstOrDefault(x => x.SourceID == Backend.Equipment.Cyberware.EssenceAntiHoleGUID);
                if(objAntiHole == null)
                {
                    XmlNode xmlEssAntiHole = LoadData("cyberware.xml").SelectSingleNode("/chummer/cyberwares/cyberware[id = \"" + Backend.Equipment.Cyberware.EssenceAntiHoleGUID.ToString("D", GlobalOptions.InvariantCultureInfo) + "\"]");
                    objAntiHole = new Cyberware(this);
                    List<Weapon> lstWeapons = new List<Weapon>(1);
                    List<Vehicle> lstVehicles = new List<Vehicle>(1);
                    objAntiHole.Create(xmlEssAntiHole, GetGradeList(Improvement.ImprovementSource.Cyberware, true).FirstOrDefault(x => x.Name == "None"), Improvement.ImprovementSource.Cyberware, intCentiessence, lstWeapons, lstVehicles);

                    Cyberware.Add(objAntiHole);

                    foreach(Weapon objWeapon in lstWeapons)
                    {
                        Weapons.Add(objWeapon);
                    }
                    foreach(Vehicle objVehicle in lstVehicles)
                    {
                        Vehicles.Add(objVehicle);
                    }
                }
                else
                {
                    objAntiHole.Rating += intCentiessence;
                }

                if(objAntiHole.Rating == 0 && Cyberware.Contains(objAntiHole))
                    Cyberware.Remove(objAntiHole);
            }

            if(objHole?.Rating == 0 && Cyberware.Contains(objHole))
                Cyberware.Remove(objHole);
        }

        private decimal _decCachedPrototypeTranshumanEssenceUsed = decimal.MinValue;

        /// <summary>
        /// Essence consumed by Prototype Transhuman 'ware
        /// </summary>
        public decimal PrototypeTranshumanEssenceUsed
        {
            get
            {
                if(_decCachedPrototypeTranshumanEssenceUsed != decimal.MinValue)
                    return _decCachedPrototypeTranshumanEssenceUsed;
                // Find the total Essence Cost of all Prototype Transhuman 'ware.
                return _decCachedPrototypeTranshumanEssenceUsed = Cyberware
                    .Where(objCyberware => objCyberware.PrototypeTranshuman).AsParallel()
                    .Sum(objCyberware => objCyberware.CalculatedESSPrototypeInvariant);
            }
        }

        public string DisplayEssence => Essence().ToString(Options.EssenceFormat, GlobalOptions.CultureInfo);

        /// <summary>
        /// This is only here for Reflection
        /// </summary>
        [HubTag("Essence")]
        public decimal EssenceDecimal => Essence();

        public string DisplayCyberwareEssence =>
            CyberwareEssence.ToString(Options.EssenceFormat, GlobalOptions.CultureInfo);

        public string DisplayBiowareEssence =>
            BiowareEssence.ToString(Options.EssenceFormat, GlobalOptions.CultureInfo);

        public string DisplayEssenceHole => EssenceHole.ToString(Options.EssenceFormat, GlobalOptions.CultureInfo);

        public string DisplayPrototypeTranshumanEssenceUsed =>
            PrototypeTranshumanEssenceUsed.ToString(Options.EssenceFormat, GlobalOptions.CultureInfo) + " / " +
            PrototypeTranshuman.ToString(Options.EssenceFormat, GlobalOptions.CultureInfo);

        #region Initiative

        #region Physical

        /// <summary>
        /// Physical Initiative.
        /// </summary>
        public string Initiative => GetInitiative(GlobalOptions.CultureInfo, GlobalOptions.Language);

        public string GetInitiative(CultureInfo objCulture, string strLanguage)
        {
            return string.Format(objCulture, LanguageManager.GetString("String_Initiative", strLanguage),
                InitiativeValue.ToString(objCulture),
                InitiativeDice.ToString(objCulture));
        }

        public string InitiativeToolTip
        {
            get
            {
                int intINTAttributeModifiers = INT.AttributeModifiers;
                int intREAAttributeModifiers = REA.AttributeModifiers;
                string strSpace = LanguageManager.GetString("String_Space");

                StringBuilder sbdInit = new StringBuilder(REA.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(REA.Value.ToString(GlobalOptions.CultureInfo)).Append(')')
                    .Append(strSpace).Append('+').Append(strSpace).Append(INT.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(INT.Value.ToString(GlobalOptions.CultureInfo)).Append(')');
                if(ImprovementManager.ValueOf(this, Improvement.ImprovementType.Initiative) != 0 || intINTAttributeModifiers != 0 || intREAAttributeModifiers != 0 || WoundModifier != 0)
                {
                    sbdInit.Append(strSpace).Append('+').Append(strSpace).Append(LanguageManager.GetString("Tip_Modifiers"))
                        .Append(strSpace).Append('(')
                        .Append((ImprovementManager.ValueOf(this, Improvement.ImprovementType.Initiative) + intINTAttributeModifiers + intREAAttributeModifiers + WoundModifier).ToString(GlobalOptions.CultureInfo))
                        .Append(')');
                }

                return string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("String_Initiative"), sbdInit.ToString(), InitiativeDice.ToString(GlobalOptions.CultureInfo));
            }
        }

        /// <summary>
        /// Initiative Dice.
        /// </summary>
        [HubTag]
        public int InitiativeDice
        {
            get
            {
                int intExtraIP = _intInitiativeDice + ImprovementManager.ValueOf(this, Improvement.ImprovementType.InitiativeDice) +
                                 ImprovementManager.ValueOf(this, Improvement.ImprovementType.InitiativeDiceAdd);

                return Math.Min(intExtraIP, 5);
            }
        }

        [HubTag]
        public int InitiativeValue
        {
            get
            {
                if ((INT == null) || (REA == null))
                {
                    Debugger.Break();
                    return 0;
                }

                int intINI = (INT.TotalValue + REA.TotalValue) + WoundModifier;
                intINI += ImprovementManager.ValueOf(this, Improvement.ImprovementType.Initiative);
                if(intINI < 0)
                    intINI = 0;
                return intINI;
            }
        }

        #endregion

        #region Astral

        /// <summary>
        /// Astral Initiative.
        /// </summary>
        public string AstralInitiative => GetAstralInitiative(GlobalOptions.CultureInfo, GlobalOptions.Language);

        public string GetAstralInitiative(CultureInfo objCulture, string strLanguageToPrint)
        {
            return string.Format(objCulture, LanguageManager.GetString("String_Initiative", strLanguageToPrint),
                AstralInitiativeValue.ToString(objCulture),
                AstralInitiativeDice.ToString(objCulture));
        }

        public string AstralInitiativeToolTip
        {
            get
            {
                if(!MAGEnabled)
                    return string.Empty;
                int intINTAttributeModifiers = INT.AttributeModifiers;
                string strSpace = LanguageManager.GetString("String_Space");
                StringBuilder sbdInit = new StringBuilder(INT.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(INT.Value.ToString(GlobalOptions.CultureInfo)).Append(')')
                    .Append(strSpace).Append('×').Append(strSpace).Append(2.ToString(GlobalOptions.CultureInfo));
                if(intINTAttributeModifiers != 0 || WoundModifier != 0)
                    sbdInit.Append(LanguageManager.GetString("Tip_Modifiers"))
                        .Append(strSpace).Append('(').Append((intINTAttributeModifiers + WoundModifier).ToString(GlobalOptions.CultureInfo)).Append(')');
                return string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("String_Initiative"),
                    sbdInit.ToString(), AstralInitiativeDice.ToString(GlobalOptions.CultureInfo));
            }
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
        public string MatrixInitiative => GetMatrixInitiative(GlobalOptions.CultureInfo, GlobalOptions.Language);

        public string GetMatrixInitiative(CultureInfo objCulture, string strLanguageToPrint)
        {
            return string.Format(objCulture, LanguageManager.GetString("String_Initiative", strLanguageToPrint),
                MatrixInitiativeValue, MatrixInitiativeDice);
        }

        public string MatrixInitiativeToolTip
        {
            get
            {
                int intINTAttributeModifiers = INT.AttributeModifiers;

                string strSpace = LanguageManager.GetString("String_Space");

                StringBuilder sbdInit;
                if(IsAI)
                {
                    sbdInit = new StringBuilder(INT.DisplayAbbrev).Append(strSpace).Append('(').Append(INT.Value.ToString(GlobalOptions.CultureInfo)).Append(')');

                    if(HomeNode != null)
                    {
                        int intHomeNodeDP = HomeNode.GetTotalMatrixAttribute("Data Processing");
                        if(HomeNode is Vehicle objHomeNodeVehicle)
                        {
                            int intHomeNodePilot = objHomeNodeVehicle.Pilot;
                            if(intHomeNodePilot > intHomeNodeDP)
                                intHomeNodeDP = intHomeNodePilot;
                        }

                        sbdInit.Append(strSpace).Append('+').Append(strSpace).Append(LanguageManager.GetString("String_DataProcessing"))
                            .Append(strSpace).Append('(').Append(intHomeNodeDP.ToString(GlobalOptions.CultureInfo)).Append(')');
                    }

                    if(intINTAttributeModifiers != 0 || WoundModifier != 0)
                    {
                        sbdInit.Append(strSpace).Append('+').Append(strSpace).Append(LanguageManager.GetString("Tip_Modifiers"))
                            .Append(strSpace).Append('(').Append((intINTAttributeModifiers + WoundModifier).ToString(GlobalOptions.CultureInfo)).Append(')');
                    }
                }
                else
                {
                    int intREAAttributeModifiers = REA.AttributeModifiers;

                    sbdInit = new StringBuilder(REA.DisplayAbbrev)
                        .Append(strSpace).Append('(').Append(REA.Value.ToString(GlobalOptions.CultureInfo)).Append(')')
                        .Append(strSpace).Append('+').Append(strSpace).Append(INT.DisplayAbbrev)
                        .Append(strSpace).Append('(').Append(INT.Value.ToString(GlobalOptions.CultureInfo)).Append(')');
                    if(ImprovementManager.ValueOf(this, Improvement.ImprovementType.Initiative) != 0 || intINTAttributeModifiers != 0 || intREAAttributeModifiers != 0 || WoundModifier != 0)
                    {
                        sbdInit.Append(strSpace).Append('+').Append(strSpace).Append(LanguageManager.GetString("Tip_Modifiers"))
                            .Append(strSpace).Append('(')
                            .Append((ImprovementManager.ValueOf(this, Improvement.ImprovementType.Initiative) + intINTAttributeModifiers + intREAAttributeModifiers + WoundModifier).ToString(GlobalOptions.CultureInfo))
                            .Append(')');
                    }
                }

                return string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("String_Initiative"),
                    sbdInit.ToString(), MatrixInitiativeDice);
            }
        }

        /// <summary>
        /// AR Matrix Initiative Value.
        /// </summary>
        public int MatrixInitiativeValue
        {
            get
            {
                if(IsAI)
                {
                    int intINI = (INT.TotalValue) + WoundModifier;
                    if(HomeNode != null)
                    {
                        int intHomeNodeDP = HomeNode.GetTotalMatrixAttribute("Data Processing");
                        if(HomeNode is Vehicle objHomeNodeVehicle)
                        {
                            int intHomeNodePilot = objHomeNodeVehicle.Pilot;
                            if(intHomeNodePilot > intHomeNodeDP)
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
                if(IsAI)
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
        public string MatrixInitiativeCold => GetMatrixInitiativeCold(GlobalOptions.CultureInfo, GlobalOptions.Language);

        public string GetMatrixInitiativeCold(CultureInfo objCulture, string strLanguageToPrint)
        {
            if(IsAI)
            {
                return GetMatrixInitiative(objCulture, strLanguageToPrint);
            }

            return string.Format(objCulture, LanguageManager.GetString(ActiveCommlink == null ? "String_MatrixInitiative" : "String_Initiative", strLanguageToPrint),
                MatrixInitiativeColdValue, MatrixInitiativeColdDice);
        }

        public string MatrixInitiativeColdToolTip
        {
            get
            {
                if(IsAI)
                {
                    return MatrixInitiativeToolTip;
                }

                int intINTAttributeModifiers = INT.AttributeModifiers;

                string strSpace = LanguageManager.GetString("String_Space");


                StringBuilder sbdInit = new StringBuilder(INT.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(INT.Value.ToString(GlobalOptions.CultureInfo)).Append(')');
                if(ActiveCommlink != null)
                {
                    sbdInit.Append(strSpace).Append('+').Append(strSpace).Append(LanguageManager.GetString("String_DataProcessing"))
                        .Append(strSpace).Append('(').Append(ActiveCommlink.GetTotalMatrixAttribute("Data Processing").ToString(GlobalOptions.CultureInfo)).Append(')');
                }

                if(ImprovementManager.ValueOf(this, Improvement.ImprovementType.MatrixInitiative) != 0 || intINTAttributeModifiers != 0 || WoundModifier != 0)
                {
                    sbdInit.Append(strSpace).Append('+').Append(strSpace).Append(LanguageManager.GetString("Tip_Modifiers"))
                        .Append(strSpace).Append('(')
                        .Append((ImprovementManager.ValueOf(this, Improvement.ImprovementType.MatrixInitiative) + intINTAttributeModifiers + WoundModifier).ToString(GlobalOptions.CultureInfo))
                        .Append(')');
                }

                return string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString(ActiveCommlink == null ? "String_MatrixInitiativeLong" : "String_Initiative"),
                    sbdInit.ToString(), MatrixInitiativeColdDice);
            }
        }

        /// <summary>
        /// Cold Sim Matrix Initiative Value.
        /// </summary>
        public int MatrixInitiativeColdValue
        {
            get
            {
                if(IsAI)
                {
                    return MatrixInitiativeValue;
                }

                int intCommlinkDP = ActiveCommlink?.GetTotalMatrixAttribute("Data Processing") ?? 0;
                return INT.TotalValue + intCommlinkDP + WoundModifier +
                       ImprovementManager.ValueOf(this, Improvement.ImprovementType.MatrixInitiative);
            }
        }

        /// <summary>
        /// Cold Sim Matrix Initiative Dice.
        /// </summary>
        public int MatrixInitiativeColdDice
        {
            get
            {
                if(IsAI)
                {
                    return MatrixInitiativeDice;
                }

                return Math.Min(3 + ImprovementManager.ValueOf(this, Improvement.ImprovementType.MatrixInitiativeDice),
                    5);
            }
        }

        #endregion

        #region Hot Sim

        /// <summary>
        /// Matrix Initiative via VR with Hot Sim.
        /// </summary>
        public string MatrixInitiativeHot => GetMatrixInitiativeHot(GlobalOptions.CultureInfo, GlobalOptions.Language);

        public string GetMatrixInitiativeHot(CultureInfo objCulture, string strLanguageToPrint)
        {
            if(IsAI)
            {
                return GetMatrixInitiative(objCulture, strLanguageToPrint);
            }

            return string.Format(objCulture, LanguageManager.GetString(ActiveCommlink == null ? "String_MatrixInitiative" : "String_Initiative", strLanguageToPrint),
                MatrixInitiativeHotValue, MatrixInitiativeHotDice);
        }

        public string MatrixInitiativeHotToolTip
        {
            get
            {
                if(IsAI)
                {
                    return MatrixInitiativeToolTip;
                }

                int intINTAttributeModifiers = INT.AttributeModifiers;

                string strSpace = LanguageManager.GetString("String_Space");


                StringBuilder sbdInit = new StringBuilder(INT.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(INT.Value.ToString(GlobalOptions.CultureInfo)).Append(')');
                if(ActiveCommlink != null)
                {
                    sbdInit.Append(strSpace).Append('+').Append(strSpace).Append(LanguageManager.GetString("String_DataProcessing"))
                        .Append(strSpace).Append('(').Append(ActiveCommlink.GetTotalMatrixAttribute("Data Processing").ToString(GlobalOptions.CultureInfo)).Append(')');
                }

                if(ImprovementManager.ValueOf(this, Improvement.ImprovementType.MatrixInitiative) != 0 || intINTAttributeModifiers != 0 || WoundModifier != 0)
                {
                    sbdInit.Append(strSpace).Append('+').Append(strSpace).Append(LanguageManager.GetString("Tip_Modifiers"))
                        .Append(strSpace).Append('(')
                        .Append((ImprovementManager.ValueOf(this, Improvement.ImprovementType.MatrixInitiative) + intINTAttributeModifiers + WoundModifier).ToString(GlobalOptions.CultureInfo))
                        .Append(')');
                }

                return string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString(ActiveCommlink == null ? "String_MatrixInitiativeLong" : "String_Initiative"),
                    sbdInit.ToString(), MatrixInitiativeHotDice);
            }
        }

        /// <summary>
        /// Hot Sim Matrix Initiative Value.
        /// </summary>
        public int MatrixInitiativeHotValue
        {
            get
            {
                if(IsAI)
                {
                    return MatrixInitiativeValue;
                }

                int intCommlinkDP = ActiveCommlink?.GetTotalMatrixAttribute("Data Processing") ?? 0;
                return INT.TotalValue + intCommlinkDP + WoundModifier +
                       ImprovementManager.ValueOf(this, Improvement.ImprovementType.MatrixInitiative);
            }
        }

        /// <summary>
        /// Hot Sim Matrix Initiative Dice.
        /// </summary>
        public int MatrixInitiativeHotDice
        {
            get
            {
                if(IsAI)
                {
                    return MatrixInitiativeDice;
                }

                return Math.Min(4 + ImprovementManager.ValueOf(this, Improvement.ImprovementType.MatrixInitiativeDice),
                    5);
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
        public int Composure => WIL.TotalValue + CHA.TotalValue +
                                ImprovementManager.ValueOf(this, Improvement.ImprovementType.Composure);

        public string ComposureToolTip
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                StringBuilder sbdToolTip = new StringBuilder(CHA.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(CHA.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(')')
                    .Append(strSpace).Append('+').Append(strSpace).Append(WIL.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(WIL.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(')');
                foreach(Improvement objLoopImprovement in Improvements)
                {
                    if(objLoopImprovement.ImproveType == Improvement.ImprovementType.Composure &&
                        objLoopImprovement.Enabled)
                    {
                        sbdToolTip.Append(strSpace).Append('+').Append(strSpace).Append(GetObjectName(objLoopImprovement))
                            .Append(strSpace).Append('(').Append(objLoopImprovement.Value.ToString(GlobalOptions.CultureInfo)).Append(')');
                    }
                }

                return sbdToolTip.ToString();
            }
        }

        /// <summary>
        /// Judge Intentions (INT + CHA).
        /// </summary>
        public int JudgeIntentions => INT.TotalValue + CHA.TotalValue +
                                      ImprovementManager.ValueOf(this, Improvement.ImprovementType.JudgeIntentions) +
                                      ImprovementManager.ValueOf(this, Improvement.ImprovementType.JudgeIntentionsOffense);

        public string JudgeIntentionsToolTip
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                StringBuilder sbdToolTip = new StringBuilder(CHA.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(CHA.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(')')
                    .Append(strSpace).Append('+').Append(strSpace).Append(INT.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(INT.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(')');
                foreach(Improvement objLoopImprovement in Improvements)
                {
                    if((objLoopImprovement.ImproveType == Improvement.ImprovementType.JudgeIntentions
                        || objLoopImprovement.ImproveType == Improvement.ImprovementType.JudgeIntentionsOffense)
                       && objLoopImprovement.Enabled)
                    {
                        sbdToolTip.Append(strSpace).Append('+').Append(strSpace).Append(GetObjectName(objLoopImprovement))
                            .Append(strSpace).Append('(').Append(objLoopImprovement.Value.ToString(GlobalOptions.CultureInfo)).Append(')');
                    }
                }

                return sbdToolTip.ToString();
            }
        }

        /// <summary>
        /// Judge Intentions Resist (CHA + WIL).
        /// </summary>
        public int JudgeIntentionsResist => CHA.TotalValue + WIL.TotalValue +
                                            ImprovementManager.ValueOf(this, Improvement.ImprovementType.JudgeIntentions) +
                                            ImprovementManager.ValueOf(this, Improvement.ImprovementType.JudgeIntentionsDefense);

        public string JudgeIntentionsResistToolTip
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                StringBuilder sbdToolTip = new StringBuilder(CHA.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(CHA.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(')')
                    .Append(strSpace).Append('+').Append(strSpace).Append(WIL.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(WIL.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(')');
                foreach(Improvement objLoopImprovement in Improvements)
                {
                    if((objLoopImprovement.ImproveType == Improvement.ImprovementType.JudgeIntentions
                        || objLoopImprovement.ImproveType == Improvement.ImprovementType.JudgeIntentionsDefense)
                       && objLoopImprovement.Enabled)
                    {
                        sbdToolTip.Append(strSpace).Append('+').Append(strSpace).Append(GetObjectName(objLoopImprovement))
                            .Append(strSpace).Append('(').Append(objLoopImprovement.Value.ToString(GlobalOptions.CultureInfo)).Append(')');
                    }
                }

                return sbdToolTip.ToString();
            }
        }

        /// <summary>
        /// Lifting and Carrying (STR + BOD).
        /// </summary>
        public int LiftAndCarry => STR.TotalValue + BOD.TotalValue +
                                   ImprovementManager.ValueOf(this, Improvement.ImprovementType.LiftAndCarry);

        public string LiftAndCarryToolTip
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                StringBuilder sbdToolTip = new StringBuilder(BOD.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(BOD.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(')')
                    .Append(strSpace).Append('+').Append(strSpace).Append(STR.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(STR.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(')');
                foreach(Improvement objLoopImprovement in Improvements)
                {
                    if(objLoopImprovement.ImproveType == Improvement.ImprovementType.LiftAndCarry
                       && objLoopImprovement.Enabled)
                    {
                        sbdToolTip.Append(strSpace).Append('+').Append(strSpace).Append(GetObjectName(objLoopImprovement))
                            .Append(strSpace).Append('(').Append(objLoopImprovement.Value.ToString(GlobalOptions.CultureInfo)).Append(')');
                    }
                }

                sbdToolTip.AppendLine().AppendFormat(GlobalOptions.CultureInfo, LanguageManager.GetString("Tip_LiftAndCarry"),
                    (STR.TotalValue * 15).ToString(GlobalOptions.CultureInfo), (STR.TotalValue * 10).ToString(GlobalOptions.CultureInfo));
                return sbdToolTip.ToString();
            }
        }

        /// <summary>
        /// Memory (LOG + WIL).
        /// </summary>
        public int Memory => LOG.TotalValue + WIL.TotalValue +
                             ImprovementManager.ValueOf(this, Improvement.ImprovementType.Memory);

        public string MemoryToolTip
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                StringBuilder sbdToolTip = new StringBuilder(LOG.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(LOG.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(')')
                    .Append(strSpace).Append('+').Append(strSpace).Append(WIL.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(WIL.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(')');
                foreach(Improvement objLoopImprovement in Improvements)
                {
                    if(objLoopImprovement.ImproveType == Improvement.ImprovementType.Memory &&
                        objLoopImprovement.Enabled)
                    {
                        sbdToolTip.Append(strSpace).Append('+').Append(strSpace).Append(GetObjectName(objLoopImprovement))
                            .Append(strSpace).Append('(').Append(objLoopImprovement.Value.ToString(GlobalOptions.CultureInfo)).Append(')');
                    }
                }

                return sbdToolTip.ToString();
            }
        }

        /// <summary>
        /// Resist test to Fatigue damage (BOD + WIL).
        /// </summary>
        public int FatigueResist => BOD.TotalValue + WIL.TotalValue +
                                    ImprovementManager.ValueOf(this, Improvement.ImprovementType.FatigueResist);

        /// <summary>
        /// Resist test to Radiation damage (BOD + WIL).
        /// </summary>
        public int RadiationResist => BOD.TotalValue + WIL.TotalValue +
                                      ImprovementManager.ValueOf(this, Improvement.ImprovementType.RadiationResist);

        /// <summary>
        /// Resist test to Sonic Attacks damage (WIL).
        /// </summary>
        public int SonicResist =>
            WIL.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.SonicResist);

        /// <summary>
        /// Resist test to Contact-vector Toxins (BOD + WIL).
        /// </summary>
        public string ToxinContactResist(string strLanguage, CultureInfo objCulture)
        {
            if(IsAI || Improvements.Any(x =>
                   x.Enabled && x.ImproveType == Improvement.ImprovementType.ToxinContactImmune))
                return LanguageManager.GetString("String_Immune", strLanguage);
            return (BOD.TotalValue + WIL.TotalValue +
                    ImprovementManager.ValueOf(this, Improvement.ImprovementType.ToxinContactResist))
                .ToString(objCulture);
        }

        /// <summary>
        /// Resist test to Ingestion-vector Toxins (BOD + WIL).
        /// </summary>
        public string ToxinIngestionResist(string strLanguage, CultureInfo objCulture)
        {
            if(IsAI || Improvements.Any(x =>
                   x.Enabled && x.ImproveType == Improvement.ImprovementType.ToxinIngestionImmune))
                return LanguageManager.GetString("String_Immune", strLanguage);
            return (BOD.TotalValue + WIL.TotalValue +
                    ImprovementManager.ValueOf(this, Improvement.ImprovementType.ToxinIngestionResist))
                .ToString(objCulture);
        }

        /// <summary>
        /// Resist test to Inhalation-vector Toxins (BOD + WIL).
        /// </summary>
        public string ToxinInhalationResist(string strLanguage, CultureInfo objCulture)
        {
            if(IsAI || Improvements.Any(x =>
                   x.Enabled && x.ImproveType == Improvement.ImprovementType.ToxinInhalationImmune))
                return LanguageManager.GetString("String_Immune", strLanguage);
            return (BOD.TotalValue + WIL.TotalValue +
                    ImprovementManager.ValueOf(this, Improvement.ImprovementType.ToxinInhalationResist))
                .ToString(objCulture);
        }

        /// <summary>
        /// Resist test to Injection-vector Toxins (BOD + WIL).
        /// </summary>
        public string ToxinInjectionResist(string strLanguage, CultureInfo objCulture)
        {
            if(IsAI || Improvements.Any(x =>
                   x.Enabled && x.ImproveType == Improvement.ImprovementType.ToxinInjectionImmune))
                return LanguageManager.GetString("String_Immune", strLanguage);
            return (BOD.TotalValue + WIL.TotalValue +
                    ImprovementManager.ValueOf(this, Improvement.ImprovementType.ToxinInjectionResist))
                .ToString(objCulture);
        }

        /// <summary>
        /// Resist test to Contact-vector Pathogens (BOD + WIL).
        /// </summary>
        public string PathogenContactResist(string strLanguage, CultureInfo objCulture)
        {
            if(IsAI || Improvements.Any(x =>
                   x.Enabled && x.ImproveType == Improvement.ImprovementType.PathogenContactImmune))
                return LanguageManager.GetString("String_Immune", strLanguage);
            return (BOD.TotalValue + WIL.TotalValue +
                    ImprovementManager.ValueOf(this, Improvement.ImprovementType.PathogenContactResist))
                .ToString(objCulture);
        }

        /// <summary>
        /// Resist test to Ingestion-vector Pathogens (BOD + WIL).
        /// </summary>
        public string PathogenIngestionResist(string strLanguage, CultureInfo objCulture)
        {
            if(IsAI || Improvements.Any(x =>
                   x.Enabled && x.ImproveType == Improvement.ImprovementType.PathogenIngestionImmune))
                return LanguageManager.GetString("String_Immune", strLanguage);
            return (BOD.TotalValue + WIL.TotalValue +
                    ImprovementManager.ValueOf(this, Improvement.ImprovementType.PathogenIngestionResist))
                .ToString(objCulture);
        }

        /// <summary>
        /// Resist test to Inhalation-vector Pathogens (BOD + WIL).
        /// </summary>
        public string PathogenInhalationResist(string strLanguage, CultureInfo objCulture)
        {
            if(IsAI || Improvements.Any(x =>
                   x.Enabled && x.ImproveType == Improvement.ImprovementType.PathogenInhalationImmune))
                return LanguageManager.GetString("String_Immune", strLanguage);
            return (BOD.TotalValue + WIL.TotalValue +
                    ImprovementManager.ValueOf(this, Improvement.ImprovementType.PathogenInhalationResist))
                .ToString(objCulture);
        }

        /// <summary>
        /// Resist test to Injection-vector Pathogens (BOD + WIL).
        /// </summary>
        public string PathogenInjectionResist(string strLanguage, CultureInfo objCulture)
        {
            if(IsAI || Improvements.Any(x =>
                   x.Enabled && x.ImproveType == Improvement.ImprovementType.PathogenInjectionImmune))
                return LanguageManager.GetString("String_Immune", strLanguage);
            return (BOD.TotalValue + WIL.TotalValue +
                    ImprovementManager.ValueOf(this, Improvement.ImprovementType.PathogenInjectionResist))
                .ToString(objCulture);
        }

        /// <summary>
        /// Resist test to Physiological Addiction (BOD + WIL) if you are not addicted yet.
        /// </summary>
        public int PhysiologicalAddictionResistFirstTime => BOD.TotalValue + WIL.TotalValue +
                                                            ImprovementManager.ValueOf(this,
                                                                Improvement.ImprovementType
                                                                    .PhysiologicalAddictionFirstTime);

        /// <summary>
        /// Resist test to Psychological Addiction (LOG + WIL) if you are not addicted yet.
        /// </summary>
        public int PsychologicalAddictionResistFirstTime => LOG.TotalValue + WIL.TotalValue +
                                                            ImprovementManager.ValueOf(this,
                                                                Improvement.ImprovementType
                                                                    .PsychologicalAddictionFirstTime);

        /// <summary>
        /// Resist test to Physiological Addiction (BOD + WIL) if you are already addicted.
        /// </summary>
        public int PhysiologicalAddictionResistAlreadyAddicted =>
            BOD.TotalValue + WIL.TotalValue + ImprovementManager.ValueOf(this,
                Improvement.ImprovementType.PhysiologicalAddictionAlreadyAddicted);

        /// <summary>
        /// Resist test to Psychological Addiction (LOG + WIL) if you are already addicted.
        /// </summary>
        public int PsychologicalAddictionResistAlreadyAddicted =>
            LOG.TotalValue + WIL.TotalValue + ImprovementManager.ValueOf(this,
                Improvement.ImprovementType.PsychologicalAddictionAlreadyAddicted);

        /// <summary>
        /// Dicepool for natural recovery from Stun CM box damage (BOD + WIL).
        /// </summary>
        public int StunCMNaturalRecovery
        {
            get
            {
                // Matrix damage for A.I.s is not naturally repaired
                if(IsAI)
                    return 0;
                int intReturn = BOD.TotalValue + WIL.TotalValue +
                                ImprovementManager.ValueOf(this, Improvement.ImprovementType.StunCMRecovery);
                if(Improvements.Any(x =>
                   x.Enabled && x.ImproveType == Improvement.ImprovementType.AddESStoStunCMRecovery))
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
                if(IsAI)
                {
                    if(HomeNode is Vehicle)
                        return 0;

                    // A.I.s can restore Core damage via Software + Depth [Data Processing] (1 day) Extended Test
                    int intDEPTotal = DEP.TotalValue;
                    int intAIReturn =
                        (SkillsSection.GetActiveSkill("Software")?.PoolOtherAttribute(intDEPTotal, "DEP") ??
                         intDEPTotal - 1) +
                        ImprovementManager.ValueOf(this, Improvement.ImprovementType.PhysicalCMRecovery);
                    if(Improvements.Any(x =>
                       x.Enabled && x.ImproveType == Improvement.ImprovementType.AddESStoPhysicalCMRecovery))
                        intAIReturn += decimal.ToInt32(decimal.Floor(Essence()));
                    return intAIReturn;
                }

                int intReturn = 2 * BOD.TotalValue +
                                ImprovementManager.ValueOf(this, Improvement.ImprovementType.PhysicalCMRecovery);
                if(Improvements.Any(x =>
                   x.Enabled && x.ImproveType == Improvement.ImprovementType.AddESStoPhysicalCMRecovery))
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
                int intReturn = CareerKarma /
                                (10 + ImprovementManager.ValueOf(this,
                                     Improvement.ImprovementType.StreetCredMultiplier));

                // Deduct burnt Street Cred.
                intReturn -= BurntStreetCred;

                return intReturn;
            }
        }

        /// <summary>
        /// Character's total amount of Street Cred (earned + GM awarded).
        /// </summary>
        public int TotalStreetCred =>
            Math.Max(
                CalculatedStreetCred + StreetCred +
                ImprovementManager.ValueOf(this, Improvement.ImprovementType.StreetCred), 0);

        public string CareerDisplayStreetCred
        {
            get
            {
                int intTotalStreetCred = TotalStreetCred;
                int intCalculatedStreetCred = intTotalStreetCred - StreetCred;
                return (intCalculatedStreetCred >= 0
                           ? " + " + intCalculatedStreetCred.ToString(GlobalOptions.CultureInfo)
                           : " - " + (-intCalculatedStreetCred).ToString(GlobalOptions.CultureInfo)) + " = " +
                       intTotalStreetCred.ToString(GlobalOptions.CultureInfo);
            }
        }

        public bool CanBurnStreetCred => Created && TotalStreetCred >= 2;

        /// <summary>
        /// Street Cred Tooltip.
        /// </summary>
        public string StreetCredTooltip
        {
            get
            {
                StringBuilder objReturn = new StringBuilder(StreetCred.ToString(GlobalOptions.CultureInfo));
                string strSpace = LanguageManager.GetString("String_Space");

                foreach(Improvement objImprovement in _lstImprovements)
                {
                    if(objImprovement.ImproveType == Improvement.ImprovementType.StreetCred && objImprovement.Enabled)
                        objReturn.Append(strSpace).Append('+').Append(strSpace).Append(GetObjectName(objImprovement))
                            .Append(strSpace).Append('(').Append(objImprovement.Value.ToString(GlobalOptions.CultureInfo)).Append(')');
                }

                objReturn.Append(strSpace).Append('+').Append(strSpace).Append('[').Append(LanguageManager.GetString("String_CareerKarma"))
                    .Append(strSpace).Append('÷').Append(strSpace)
                    .Append((10 + ImprovementManager.ValueOf(this, Improvement.ImprovementType.StreetCredMultiplier)).ToString(GlobalOptions.CultureInfo)).Append(']')
                    .Append(strSpace).Append('(')
                    .Append((CareerKarma / (10 + ImprovementManager.ValueOf(this, Improvement.ImprovementType.StreetCredMultiplier))).ToString(GlobalOptions.CultureInfo))
                    .Append(')');

                if(BurntStreetCred != 0)
                    objReturn.Append(strSpace).Append('-').Append(strSpace).Append(LanguageManager.GetString("String_BurntStreetCred"))
                        .Append(strSpace).Append('(').Append(BurntStreetCred.ToString(GlobalOptions.CultureInfo)).Append(')');

                return objReturn.ToString();
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
                int intReturn = ImprovementManager.ValueOf(this, Improvement.ImprovementType.Notoriety) -
                                (BurntStreetCred / 2); // + Contacts.Count(x => x.EntityType == ContactType.Enemy);

                return intReturn;
            }
        }

        /// <summary>
        /// Character's total amount of Notoriety (earned + GM awarded - burnt Street Cred).
        /// </summary>
        public int TotalNotoriety => CalculatedNotoriety + Notoriety;

        public string CareerDisplayNotoriety
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                int intCalculatedNotoriety = CalculatedNotoriety;
                return (intCalculatedNotoriety >= 0
                           ? strSpace + '+' + strSpace +
                             intCalculatedNotoriety.ToString(GlobalOptions.CultureInfo)
                           : strSpace + '-' + strSpace +
                             (-intCalculatedNotoriety).ToString(GlobalOptions.CultureInfo)) + strSpace + '=' +
                       strSpace + TotalNotoriety.ToString(GlobalOptions.CultureInfo);
            }
        }

        /// <summary>
        /// Tooltip to use for Notoriety total.
        /// </summary>
        public string NotorietyTooltip
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                StringBuilder objReturn = new StringBuilder(Notoriety.ToString(GlobalOptions.CultureInfo));

                foreach(Improvement objImprovement in _lstImprovements)
                {
                    if(objImprovement.ImproveType == Improvement.ImprovementType.Notoriety && objImprovement.Enabled)
                        objReturn.Append(strSpace).Append('+').Append(strSpace).Append(GetObjectName(objImprovement))
                            .Append(strSpace).Append('(').Append(objImprovement.Value.ToString(GlobalOptions.CultureInfo)).Append(')');
                }

                /*
                int intEnemies = Contacts.Count(x => x.EntityType == ContactType.Enemy);
                if (intEnemies > 0)
                    objReturn.Append(strSpace).Append('+').Append(strSpace).Append(LanguageManager.GetString("Label_SummaryEnemies")).Append(strSpace).Append('(').Append(intEnemies.ToString(GlobalOptions.CultureInfo)).Append(')');
                    */

                if(BurntStreetCred > 0)
                    objReturn.Append(strSpace).Append('-').Append(strSpace).Append(LanguageManager.GetString("String_BurntStreetCred"))
                        .Append(strSpace).Append('(').Append((BurntStreetCred / 2).ToString(GlobalOptions.CultureInfo)).Append(')');

                string strReturn = objReturn.ToString();

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
                if(Options.UseCalculatedPublicAwareness)
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
                int intReturn = PublicAwareness + CalculatedPublicAwareness;
                if(Erased && intReturn >= 1)
                    return 1;
                return intReturn;
            }
        }

        public string CareerDisplayPublicAwareness
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                int intTotalPublicAwareness = TotalPublicAwareness;
                int intCalculatedPublicAwareness = intTotalPublicAwareness - PublicAwareness;
                return (intCalculatedPublicAwareness >= 0
                           ? strSpace + '+' + strSpace +
                             intCalculatedPublicAwareness.ToString(GlobalOptions.CultureInfo)
                           : strSpace + '-' + strSpace +
                             (-intCalculatedPublicAwareness).ToString(GlobalOptions.CultureInfo)) + strSpace +
                       '=' + strSpace + intTotalPublicAwareness.ToString(GlobalOptions.CultureInfo);
            }
        }

        /// <summary>
        /// Public Awareness Tooltip.
        /// </summary>
        public string PublicAwarenessTooltip
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                StringBuilder objReturn = new StringBuilder(PublicAwareness.ToString(GlobalOptions.CultureInfo));

                foreach(Improvement objImprovement in _lstImprovements)
                {
                    if(objImprovement.ImproveType == Improvement.ImprovementType.PublicAwareness &&
                        objImprovement.Enabled)
                        objReturn.Append(strSpace).Append('+').Append(strSpace).Append(GetObjectName(objImprovement))
                            .Append(strSpace).Append('(').Append(objImprovement.Value.ToString(GlobalOptions.CultureInfo)).Append(')');
                }

                if(Options.UseCalculatedPublicAwareness)
                {
                    objReturn.Append(strSpace).Append('+').Append(strSpace).Append('[').Append(LanguageManager.GetString("String_StreetCred"))
                        .Append(strSpace).Append('+').Append(strSpace).Append(LanguageManager.GetString("String_Notoriety")).Append(']')
                        .Append(strSpace).Append('÷').Append(strSpace).Append(3.ToString(GlobalOptions.CultureInfo))
                        .Append(strSpace).Append('(').Append(((TotalStreetCred + TotalNotoriety) / 3).ToString(GlobalOptions.CultureInfo)).Append(')');
                }

                if(Erased)
                {
                    int intTotalPublicAwareness = PublicAwareness + CalculatedPublicAwareness;
                    if(intTotalPublicAwareness > 1)
                    {
                        string strErasedString = Qualities.FirstOrDefault(x => x.Name == "Erased")
                            ?.DisplayNameShort(GlobalOptions.Language);
                        if(string.IsNullOrEmpty(strErasedString))
                        {
                            XmlNode xmlErasedQuality = LoadData("qualities.xml")
                                .SelectSingleNode("chummer/qualities/quality[name = \"Erased\"]");
                            if(xmlErasedQuality != null)
                            {
                                strErasedString = xmlErasedQuality["translate"]?.InnerText
                                                  ?? xmlErasedQuality["name"]?.InnerText
                                                  ?? string.Empty;
                            }
                        }

                        objReturn.Append(strSpace).Append('-').Append(strSpace).Append(strErasedString)
                            .Append(strSpace).Append('(').Append((intTotalPublicAwareness - 1).ToString(GlobalOptions.CultureInfo)).Append(')');
                    }
                }

                return objReturn.ToString();
            }
        }

        #endregion

        #region List Properties

        /// <summary>
        /// Improvements.
        /// </summary>
        public ObservableCollection<Improvement> Improvements => _lstImprovements;

        /// <summary>
        /// Mentor spirits.
        /// </summary>
        [HubTag(true)]
        public ObservableCollection<MentorSpirit> MentorSpirits => _lstMentorSpirits;

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
        [HubTag(true)]
        public ObservableCollection<Spell> Spells => _lstSpells;

        /// <summary>
        /// Foci.
        /// </summary>
        public List<Focus> Foci => _lstFoci;

        /// <summary>
        /// Stacked Foci.
        /// </summary>
        public List<StackedFocus> StackedFoci => _lstStackedFoci;

        /// <summary>
        /// Adept Powers.
        /// </summary>
        [HubTag(true)]
        public CachedBindingList<Power> Powers => _lstPowers;

        /// <summary>
        /// Technomancer Complex Forms.
        /// </summary>
        [HubTag(true)]
        public ObservableCollection<ComplexForm> ComplexForms => _lstComplexForms;

        /// <summary>
        /// AI Programs and Advanced Programs
        /// </summary>
        [HubTag(true)]
        public ObservableCollection<AIProgram> AIPrograms => _lstAIPrograms;

        /// <summary>
        /// Martial Arts.
        /// </summary>
        public ObservableCollection<MartialArt> MartialArts => _lstMartialArts;

#if LEGACY
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
#endif

        /// <summary>
        /// Limit Modifiers.
        /// </summary>
        public ObservableCollection<LimitModifier> LimitModifiers => _lstLimitModifiers;

        /// <summary>
        /// Armor.
        /// </summary>
        [HubTag(true)]
        public ObservableCollection<Armor> Armor => _lstArmor;

        /// <summary>
        /// Cyberware and Bioware.
        /// </summary>
        [HubTag(true)]
        public TaggedObservableCollection<Cyberware> Cyberware => _lstCyberware;

        /// <summary>
        /// Weapons.
        /// </summary>
        [HubTag(true)]
        public TaggedObservableCollection<Weapon> Weapons => _lstWeapons;

        /// <summary>
        /// Lifestyles.
        /// </summary>
        public ObservableCollection<Lifestyle> Lifestyles => _lstLifestyles;

        /// <summary>
        /// Gear.
        /// </summary>
        [HubTag(true)]
        public ObservableCollection<Gear> Gear => _lstGear;

        /// <summary>
        /// Vehicles.
        /// </summary>
        [HubTag(true)]
        public TaggedObservableCollection<Vehicle> Vehicles => _lstVehicles;

        /// <summary>
        /// Metamagics and Echoes.
        /// </summary>
        [HubTag(true)]
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
        [HubTag(true)]
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
        [HubTag(true)]
        public ObservableCollection<Quality> Qualities => _lstQualities;

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
        public ObservableCollection<Location> GearLocations => _lstGearLocations;

        /// <summary>
        /// Armor Bundles.
        /// </summary>
        public ObservableCollection<Location> ArmorLocations => _lstArmorLocations;

        /// <summary>
        /// Vehicle Locations.
        /// </summary>
        public TaggedObservableCollection<Location> VehicleLocations => _lstVehicleLocations;

        /// <summary>
        /// Weapon Locations.
        /// </summary>
        public ObservableCollection<Location> WeaponLocations => _lstWeaponLocations;

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
        public List<string> InternalIdsNeedingReapplyImprovements => _lstInternalIdsNeedingReapplyImprovements;

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

                if(Armor.Count == 0) return 0;
                // Run through the list of Armor currently worn and retrieve the highest total Armor rating.
                foreach(Armor objArmor in Armor.Where(objArmor =>
                   !objArmor.ArmorValue.StartsWith('+') && objArmor.Equipped))
                {
                    int intArmorValue = objArmor.TotalArmor;
                    int intCustomStackBonus = 0;
                    string strArmorName = objArmor.Name;
                    if(objArmor.Category == "High-Fashion Armor Clothing")
                    {
                        foreach(Armor a in Armor.Where(a =>
                           (a.Category == "High-Fashion Armor Clothing" || a.ArmorOverrideValue.StartsWith('+')) &&
                           a.Equipped))
                        {
                            if(a.ArmorMods.Any(objMod =>
                               objMod.Name == "Custom Fit (Stack)" && objMod.Extra == strArmorName))
                                intCustomStackBonus += Convert.ToInt32(a.ArmorOverrideValue, GlobalOptions.InvariantCultureInfo);
                        }
                    }

                    if(intArmorValue + intCustomStackBonus > intHighest)
                    {
                        intHighest = intArmorValue + intCustomStackBonus;
                        intHighestNoCustomStack = intArmorValue;
                        strHighest = strArmorName;
                    }
                }

                int intArmor = intHighestNoCustomStack;

                // Run through the list of Armor currently worn again and look at Clothing items that start with '+' since they stack with eachother.
                int intClothing = 0;
                foreach(Armor objArmor in Armor.Where(objArmor =>
                   (objArmor.ArmorValue.StartsWith('+') || objArmor.ArmorOverrideValue.StartsWith('+')) &&
                   objArmor.Name != strHighest && objArmor.Category == "Clothing" && objArmor.Equipped))
                {
                    if(objArmor.ArmorValue.StartsWith('+'))
                        intClothing += objArmor.TotalArmor;
                    else
                        intClothing += objArmor.TotalOverrideArmor;
                }

                if(intClothing > intHighest)
                {
                    intArmor = intClothing;
                    strHighest = string.Empty;
                }

                // Run through the list of Armor currently worn again and look at non-Clothing items that start with '+' since they stack with the highest Armor.
                int intStacking = 0;
                foreach(Armor objArmor in Armor.Where(objArmor =>
                   (objArmor.ArmorValue.StartsWith('+') || objArmor.ArmorOverrideValue.StartsWith('+')) &&
                   objArmor.Name != strHighest && objArmor.Category != "Clothing" && objArmor.Equipped))
                {
                    bool blnDoAdd = true;
                    if(objArmor.Category == "High-Fashion Armor Clothing")
                    {
                        foreach(ArmorMod objMod in objArmor.ArmorMods)
                        {
                            if(objMod.Name == "Custom Fit (Stack)")
                            {
                                blnDoAdd = objMod.Extra == strHighest && !string.IsNullOrEmpty(strHighest);
                                break;
                            }
                        }
                    }

                    if(blnDoAdd)
                    {
                        if(objArmor.ArmorValue.StartsWith('+'))
                            intStacking += objArmor.TotalArmor;
                        else
                            intStacking += objArmor.TotalOverrideArmor;
                    }
                }

                return intArmor + intStacking;
            }
        }

        public int DamageResistancePool =>
            (IsAI ? (HomeNode is Vehicle objVehicle ? objVehicle.TotalBody : 0) : BOD.TotalValue) + TotalArmorRating +
            ImprovementManager.ValueOf(this, Improvement.ImprovementType.DamageResistance);

        public string DamageResistancePoolToolTip
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                StringBuilder sbdToolTip = new StringBuilder();
                if(IsAI)
                {
                    sbdToolTip.Append(LanguageManager.GetString("String_VehicleBody"))
                        .Append(strSpace).Append('(').Append((HomeNode is Vehicle objVehicle ? objVehicle.TotalBody : 0).ToString(GlobalOptions.CultureInfo)).Append(')');
                }
                else
                {
                    sbdToolTip.Append(BOD.DisplayAbbrev).Append(strSpace).Append('(').Append(BOD.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(')');
                }

                sbdToolTip.Append(strSpace).Append('+').Append(strSpace).Append(LanguageManager.GetString("Tip_Armor"))
                    .Append(strSpace).Append('(').Append(TotalArmorRating.ToString(GlobalOptions.CultureInfo)).Append(')');
                foreach(Improvement objLoopImprovement in Improvements)
                {
                    if(objLoopImprovement.ImproveType == Improvement.ImprovementType.DamageResistance &&
                        objLoopImprovement.Enabled)
                    {
                        sbdToolTip.Append(strSpace).Append('+').Append(strSpace).Append(GetObjectName(objLoopImprovement))
                            .Append(strSpace).Append('(').Append(objLoopImprovement.Value.ToString(GlobalOptions.CultureInfo)).Append(')');
                    }
                }

                return sbdToolTip.ToString();
            }
        }

        public int CurrentCounterspellingDice
        {
            get => _intCurrentCounterspellingDice;
            set
            {
                if(_intCurrentCounterspellingDice != value)
                {
                    _intCurrentCounterspellingDice = value;
                    OnPropertyChanged();
                }
            }
        }
        #region Dodge
        public int Dodge => REA.TotalValue + INT.TotalValue + TotalBonusDodgeRating;

        public string DisplayDodge => Dodge.ToString(GlobalOptions.CultureInfo);

        public string DodgeToolTip
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                StringBuilder sbdToolTip = new StringBuilder(REA.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(REA.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(')')
                    .Append(strSpace).Append('+').Append(strSpace).Append(INT.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(INT.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(')');

                int intModifiers = TotalBonusDodgeRating;

                if (intModifiers != 0)
                {
                    FormatImprovementModifiers(sbdToolTip,
                        new HashSet<Improvement.ImprovementType> { Improvement.ImprovementType.Dodge },
                        strSpace,
                        intModifiers);
                }

                return sbdToolTip.ToString();
            }
        }
        #endregion
        #region Spell Defense
        #region Indirect Dodge
        public int SpellDefenseIndirectDodge => Dodge;

        public string DisplaySpellDefenseIndirectDodge => CurrentCounterspellingDice == 0
            ? SpellDefenseIndirectDodge.ToString(GlobalOptions.CultureInfo)
            : SpellDefenseIndirectDodge.ToString(GlobalOptions.CultureInfo) +
              LanguageManager.GetString("String_Space") + '(' +
              (SpellDefenseIndirectDodge + CurrentCounterspellingDice).ToString(GlobalOptions.CultureInfo) + ')';

        public string SpellDefenseIndirectDodgeToolTip
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                StringBuilder sbdToolTip = new StringBuilder(DodgeToolTip);

                if(CurrentCounterspellingDice != 0)
                    sbdToolTip.Append(strSpace).Append('+').Append(strSpace).Append(LanguageManager.GetString("Label_CounterspellingDice"))
                        .Append(strSpace).Append('(').Append(CurrentCounterspellingDice.ToString(GlobalOptions.CultureInfo)).Append(')');

                return sbdToolTip.ToString();
            }
        }
        #endregion
        #region Indirect Soak
        public int SpellDefenseIndirectSoak =>
            (IsAI ? (HomeNode is Vehicle objVehicle ? objVehicle.TotalBody : 0) : BOD.TotalValue) + TotalArmorRating +
            SpellResistance + ImprovementManager.ValueOf(this,Improvement.ImprovementType.DamageResistance);

        public string DisplaySpellDefenseIndirectSoak => CurrentCounterspellingDice == 0
            ? SpellDefenseIndirectSoak.ToString(GlobalOptions.CultureInfo)
            : SpellDefenseIndirectSoak.ToString(GlobalOptions.CultureInfo) +
              LanguageManager.GetString("String_Space") + '(' +
              (SpellDefenseIndirectSoak + CurrentCounterspellingDice).ToString(GlobalOptions.CultureInfo) + ')';

        public string SpellDefenseIndirectSoakToolTip
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                StringBuilder sbdToolTip = new StringBuilder();
                if(IsAI)
                {
                    sbdToolTip.Append(LanguageManager.GetString("String_VehicleBody"))
                        .Append(strSpace).Append('(').Append((HomeNode is Vehicle objVehicle ? objVehicle.TotalBody : 0).ToString(GlobalOptions.CultureInfo)).Append(')');
                }
                else
                {
                    sbdToolTip.Append(BOD.DisplayAbbrev).Append(strSpace).Append('(').Append(BOD.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(')');
                }

                sbdToolTip.Append(strSpace).Append('+').Append(strSpace).Append(LanguageManager.GetString("Tip_Armor"))
                    .Append(strSpace).Append('(').Append(TotalArmorRating.ToString(GlobalOptions.CultureInfo)).Append(')');

                if(CurrentCounterspellingDice != 0)
                    sbdToolTip.Append(strSpace).Append('+').Append(strSpace).Append(LanguageManager.GetString("Label_CounterspellingDice"))
                        .Append(strSpace).Append('(').Append(CurrentCounterspellingDice.ToString(GlobalOptions.CultureInfo)).Append(')');

                int intModifiers = SpellResistance +
                                   ImprovementManager.ValueOf(this, Improvement.ImprovementType.DamageResistance);

                if(intModifiers != 0)
                {
                    FormatImprovementModifiers(sbdToolTip,
                        new HashSet<Improvement.ImprovementType> { Improvement.ImprovementType.DamageResistance },
                        strSpace,
                        intModifiers);
                }

                return sbdToolTip.ToString();
            }
        }
        #endregion
        #region Direct Soak Mana
        public int SpellDefenseDirectSoakMana => WIL.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.DirectManaSpellResist) + SpellResistance;

        public string DisplaySpellDefenseDirectSoakMana => CurrentCounterspellingDice == 0
            ? SpellDefenseDirectSoakMana.ToString(GlobalOptions.CultureInfo)
            : SpellDefenseDirectSoakMana.ToString(GlobalOptions.CultureInfo) +
              LanguageManager.GetString("String_Space") + '(' +
              (SpellDefenseDirectSoakMana + CurrentCounterspellingDice).ToString(GlobalOptions.CultureInfo) + ')';

        public string SpellDefenseDirectSoakManaToolTip
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                StringBuilder sbdToolTip = new StringBuilder(WIL.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(WIL.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(')');

                if(CurrentCounterspellingDice != 0)
                    sbdToolTip.Append(strSpace).Append('+').Append(strSpace).Append(LanguageManager.GetString("Label_CounterspellingDice"))
                        .Append(strSpace).Append('(').Append(CurrentCounterspellingDice.ToString(GlobalOptions.CultureInfo)).Append(')');

                int intModifiers = SpellResistance + ImprovementManager.ValueOf(this, Improvement.ImprovementType.DirectManaSpellResist);

                if(intModifiers != 0)
                {
                    FormatImprovementModifiers(
                        sbdToolTip,
                        new HashSet<Improvement.ImprovementType> { Improvement.ImprovementType.SpellResistance },
                        strSpace,
                        intModifiers);
                }

                return sbdToolTip.ToString();
            }
        }
        #endregion
        #region Direct Soak Physical
        public int SpellDefenseDirectSoakPhysical =>
            (IsAI ? (HomeNode is Vehicle objVehicle ? objVehicle.TotalBody : 0) : BOD.TotalValue) + ImprovementManager.ValueOf(this, Improvement.ImprovementType.DirectPhysicalSpellResist) + SpellResistance;

        public string DisplaySpellDefenseDirectSoakPhysical => CurrentCounterspellingDice == 0
            ? SpellDefenseDirectSoakPhysical.ToString(GlobalOptions.CultureInfo)
            : SpellDefenseDirectSoakPhysical.ToString(GlobalOptions.CultureInfo) +
              LanguageManager.GetString("String_Space") + '(' +
              (SpellDefenseDirectSoakPhysical + CurrentCounterspellingDice).ToString(GlobalOptions.CultureInfo) + ')';

        public string SpellDefenseDirectSoakPhysicalToolTip
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                StringBuilder sbdToolTip = new StringBuilder();
                if(IsAI)
                {
                    sbdToolTip.Append(LanguageManager.GetString("String_VehicleBody"))
                        .Append(strSpace).Append('(').Append((HomeNode is Vehicle objVehicle ? objVehicle.TotalBody : 0).ToString(GlobalOptions.CultureInfo)).Append(')');
                }
                else
                {
                    sbdToolTip.Append(BOD.DisplayAbbrev).Append(strSpace).Append('(').Append(BOD.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(')');
                }

                if(CurrentCounterspellingDice != 0)
                    sbdToolTip.Append(strSpace).Append('+').Append(strSpace).Append(LanguageManager.GetString("Label_CounterspellingDice"))
                        .Append(strSpace).Append('(').Append(CurrentCounterspellingDice.ToString(GlobalOptions.CultureInfo)).Append(')');

                int intModifiers = SpellResistance + ImprovementManager.ValueOf(this, Improvement.ImprovementType.DirectPhysicalSpellResist);

                if(intModifiers != 0)
                {
                    FormatImprovementModifiers(sbdToolTip,
                        new HashSet<Improvement.ImprovementType>
                        {
                            Improvement.ImprovementType.SpellResistance,
                            Improvement.ImprovementType.DirectPhysicalSpellResist
                        },
                        strSpace,
                        intModifiers);
                }

                return sbdToolTip.ToString();
            }
        }
        #endregion
        #region Detection
        public int SpellDefenseDetection => LOG.TotalValue + WIL.TotalValue + SpellResistance +
                                            ImprovementManager.ValueOf(this,
                                                Improvement.ImprovementType.DetectionSpellResist);

        public string DisplaySpellDefenseDetection => CurrentCounterspellingDice == 0
            ? SpellDefenseDetection.ToString(GlobalOptions.CultureInfo)
            : SpellDefenseDetection.ToString(GlobalOptions.CultureInfo) +
              LanguageManager.GetString("String_Space") + '(' +
              (SpellDefenseDetection + CurrentCounterspellingDice).ToString(GlobalOptions.CultureInfo) + ')';

        public string SpellDefenseDetectionToolTip
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                StringBuilder sbdToolTip = new StringBuilder(LOG.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(LOG.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(')')
                    .Append(strSpace).Append('+').Append(strSpace).Append(WIL.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(WIL.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(')');

                if(CurrentCounterspellingDice != 0)
                    sbdToolTip.Append(strSpace).Append('+').Append(strSpace).Append(LanguageManager.GetString("Label_CounterspellingDice"))
                        .Append(strSpace).Append('(').Append(CurrentCounterspellingDice.ToString(GlobalOptions.CultureInfo)).Append(')');

                int intModifiers = SpellResistance +
                                   ImprovementManager.ValueOf(this, Improvement.ImprovementType.DetectionSpellResist);

                if(intModifiers != 0)
                {
                    FormatImprovementModifiers(
                        sbdToolTip,
                        new HashSet<Improvement.ImprovementType>
                        {
                            Improvement.ImprovementType.SpellResistance,
                            Improvement.ImprovementType.DetectionSpellResist
                        },
                        strSpace,
                        intModifiers);
                }

                return sbdToolTip.ToString();
            }
        }
        #endregion
        #region Decrease Attributes
        public int SpellDefenseDecreaseBOD => BOD.TotalValue + WIL.TotalValue + SpellResistance + ImprovementManager.ValueOf(this, Improvement.ImprovementType.DecreaseBODResist);

        public string DisplaySpellDefenseDecreaseBOD => CurrentCounterspellingDice == 0
            ? SpellDefenseDecreaseBOD.ToString(GlobalOptions.CultureInfo)
            : SpellDefenseDecreaseBOD.ToString(GlobalOptions.CultureInfo) +
              LanguageManager.GetString("String_Space") + '(' +
              (SpellDefenseDecreaseBOD + CurrentCounterspellingDice).ToString(GlobalOptions.CultureInfo) + ')';

        public string SpellDefenseDecreaseBODToolTip
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                StringBuilder sbdToolTip = new StringBuilder(BOD.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(BOD.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(')')
                    .Append(strSpace).Append('+').Append(strSpace).Append(WIL.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(WIL.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(')');

                if(CurrentCounterspellingDice != 0)
                    sbdToolTip.Append(strSpace).Append('+').Append(strSpace).Append(LanguageManager.GetString("Label_CounterspellingDice"))
                        .Append(strSpace).Append('(').Append(CurrentCounterspellingDice.ToString(GlobalOptions.CultureInfo)).Append(')');

                int intModifiers = SpellResistance + ImprovementManager.ValueOf(this, Improvement.ImprovementType.DecreaseBODResist);

                if(intModifiers != 0)
                {
                    FormatImprovementModifiers(
                        sbdToolTip,
                        new HashSet<Improvement.ImprovementType>
                        {
                            Improvement.ImprovementType.SpellResistance,
                            Improvement.ImprovementType.DecreaseBODResist
                        },
                        strSpace,
                        intModifiers);
                }

                return sbdToolTip.ToString();
            }
        }

        public int SpellDefenseDecreaseAGI => AGI.TotalValue + WIL.TotalValue + SpellResistance + ImprovementManager.ValueOf(this, Improvement.ImprovementType.DecreaseAGIResist);

        public string DisplaySpellDefenseDecreaseAGI => CurrentCounterspellingDice == 0
            ? SpellDefenseDecreaseAGI.ToString(GlobalOptions.CultureInfo)
            : SpellDefenseDecreaseAGI.ToString(GlobalOptions.CultureInfo) +
              LanguageManager.GetString("String_Space") + '(' +
              (SpellDefenseDecreaseAGI + CurrentCounterspellingDice).ToString(GlobalOptions.CultureInfo) + ')';

        public string SpellDefenseDecreaseAGIToolTip
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                StringBuilder sbdToolTip = new StringBuilder(AGI.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(AGI.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(')')
                    .Append(strSpace).Append('+').Append(strSpace).Append(WIL.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(WIL.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(')');

                if(CurrentCounterspellingDice != 0)
                    sbdToolTip.Append(strSpace).Append('+').Append(strSpace).Append(LanguageManager.GetString("Label_CounterspellingDice"))
                        .Append(strSpace).Append('(').Append(CurrentCounterspellingDice.ToString(GlobalOptions.CultureInfo)).Append(')');

                int intModifiers = SpellResistance + ImprovementManager.ValueOf(this, Improvement.ImprovementType.DecreaseAGIResist);

                if(intModifiers != 0)
                {
                    FormatImprovementModifiers(
                        sbdToolTip,
                        new HashSet<Improvement.ImprovementType>
                        {
                            Improvement.ImprovementType.SpellResistance,
                            Improvement.ImprovementType.DecreaseAGIResist
                        },
                        strSpace,
                        intModifiers);
                }

                return sbdToolTip.ToString();
            }
        }

        public int SpellDefenseDecreaseREA => REA.TotalValue + WIL.TotalValue + SpellResistance + ImprovementManager.ValueOf(this, Improvement.ImprovementType.DecreaseREAResist);

        public string DisplaySpellDefenseDecreaseREA => CurrentCounterspellingDice == 0
            ? SpellDefenseDecreaseREA.ToString(GlobalOptions.CultureInfo)
            : SpellDefenseDecreaseREA.ToString(GlobalOptions.CultureInfo) +
              LanguageManager.GetString("String_Space") + '(' +
              (SpellDefenseDecreaseREA + CurrentCounterspellingDice).ToString(GlobalOptions.CultureInfo) + ')';

        public string SpellDefenseDecreaseREAToolTip
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                StringBuilder sbdToolTip = new StringBuilder(REA.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(REA.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(')')
                    .Append(strSpace).Append('+').Append(strSpace).Append(WIL.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(WIL.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(')');

                if(CurrentCounterspellingDice != 0)
                    sbdToolTip.Append(strSpace).Append('+').Append(strSpace).Append(LanguageManager.GetString("Label_CounterspellingDice"))
                        .Append(strSpace).Append('(').Append(CurrentCounterspellingDice.ToString(GlobalOptions.CultureInfo)).Append(')');

                int intModifiers = SpellResistance + ImprovementManager.ValueOf(this, Improvement.ImprovementType.DecreaseREAResist);

                if(intModifiers != 0)
                {
                    FormatImprovementModifiers(
                        sbdToolTip,
                        new HashSet<Improvement.ImprovementType>
                        {
                            Improvement.ImprovementType.SpellResistance,
                            Improvement.ImprovementType.DecreaseREAResist
                        },
                        strSpace,
                        intModifiers);
                }

                return sbdToolTip.ToString();
            }
        }

        public int SpellDefenseDecreaseSTR => STR.TotalValue + WIL.TotalValue + SpellResistance + ImprovementManager.ValueOf(this, Improvement.ImprovementType.DecreaseSTRResist);

        public string DisplaySpellDefenseDecreaseSTR => CurrentCounterspellingDice == 0
            ? SpellDefenseDecreaseSTR.ToString(GlobalOptions.CultureInfo)
            : SpellDefenseDecreaseSTR.ToString(GlobalOptions.CultureInfo) +
              LanguageManager.GetString("String_Space") + '(' +
              (SpellDefenseDecreaseSTR + CurrentCounterspellingDice).ToString(GlobalOptions.CultureInfo) + ')';

        public string SpellDefenseDecreaseSTRToolTip
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                StringBuilder sbdToolTip = new StringBuilder(STR.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(STR.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(')')
                    .Append(strSpace).Append('+').Append(strSpace).Append(WIL.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(WIL.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(')');

                if(CurrentCounterspellingDice != 0)
                    sbdToolTip.Append(strSpace).Append('+').Append(strSpace).Append(LanguageManager.GetString("Label_CounterspellingDice"))
                        .Append(strSpace).Append('(').Append(CurrentCounterspellingDice.ToString(GlobalOptions.CultureInfo) + ')');

                int intModifiers = SpellResistance + ImprovementManager.ValueOf(this, Improvement.ImprovementType.DecreaseSTRResist);

                if(intModifiers != 0)
                {
                    FormatImprovementModifiers(
                        sbdToolTip,
                        new HashSet<Improvement.ImprovementType>
                        {
                            Improvement.ImprovementType.SpellResistance
                        },
                        strSpace,
                        intModifiers);
                }

                return sbdToolTip.ToString();
            }
        }

        public int SpellDefenseDecreaseCHA => CHA.TotalValue + WIL.TotalValue + SpellResistance + ImprovementManager.ValueOf(this, Improvement.ImprovementType.DecreaseCHAResist);

        public string DisplaySpellDefenseDecreaseCHA => CurrentCounterspellingDice == 0
            ? SpellDefenseDecreaseCHA.ToString(GlobalOptions.CultureInfo)
            : SpellDefenseDecreaseCHA.ToString(GlobalOptions.CultureInfo) +
              LanguageManager.GetString("String_Space") + '(' +
              (SpellDefenseDecreaseCHA + CurrentCounterspellingDice).ToString(GlobalOptions.CultureInfo) + ')';

        public string SpellDefenseDecreaseCHAToolTip
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                StringBuilder sbdToolTip = new StringBuilder(CHA.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(CHA.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(')')
                    .Append(strSpace).Append('+').Append(strSpace).Append(WIL.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(WIL.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(')');

                if (CurrentCounterspellingDice != 0)
                    sbdToolTip.Append(strSpace).Append('+').Append(strSpace).Append(LanguageManager.GetString("Label_CounterspellingDice"))
                        .Append(strSpace).Append('(').Append(CurrentCounterspellingDice.ToString(GlobalOptions.CultureInfo) + ')');

                int intModifiers = SpellResistance + ImprovementManager.ValueOf(this, Improvement.ImprovementType.DecreaseCHAResist);

                if(intModifiers != 0)
                {
                    FormatImprovementModifiers(
                        sbdToolTip,
                        new HashSet<Improvement.ImprovementType>
                        {
                            Improvement.ImprovementType.SpellResistance,
                            Improvement.ImprovementType.DecreaseCHAResist
                        },
                        strSpace,
                        intModifiers);
                }

                return sbdToolTip.ToString();
            }
        }

        public int SpellDefenseDecreaseINT => INT.TotalValue + WIL.TotalValue + SpellResistance + ImprovementManager.ValueOf(this, Improvement.ImprovementType.DecreaseINTResist);

        public string DisplaySpellDefenseDecreaseINT => CurrentCounterspellingDice == 0
            ? SpellDefenseDecreaseINT.ToString(GlobalOptions.CultureInfo)
            : SpellDefenseDecreaseINT.ToString(GlobalOptions.CultureInfo) +
              LanguageManager.GetString("String_Space") + '(' +
              (SpellDefenseDecreaseINT + CurrentCounterspellingDice).ToString(GlobalOptions.CultureInfo) + ')';

        public string SpellDefenseDecreaseINTToolTip
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                StringBuilder sbdToolTip = new StringBuilder(INT.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(INT.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(')')
                    .Append(strSpace).Append('+').Append(strSpace).Append(WIL.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(WIL.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(')');

                if (CurrentCounterspellingDice != 0)
                    sbdToolTip.Append(strSpace).Append('+').Append(strSpace).Append(LanguageManager.GetString("Label_CounterspellingDice"))
                        .Append(strSpace).Append('(').Append(CurrentCounterspellingDice.ToString(GlobalOptions.CultureInfo) + ')');

                int intModifiers = SpellResistance + ImprovementManager.ValueOf(this, Improvement.ImprovementType.DecreaseINTResist);

                if(intModifiers != 0)
                {
                    FormatImprovementModifiers(
                        sbdToolTip,
                        new HashSet<Improvement.ImprovementType>
                        {
                            Improvement.ImprovementType.SpellResistance,
                            Improvement.ImprovementType.DecreaseINTResist
                        },
                        strSpace,
                        intModifiers);
                }

                return sbdToolTip.ToString();
            }
        }

        public int SpellDefenseDecreaseLOG => LOG.TotalValue + WIL.TotalValue + SpellResistance + ImprovementManager.ValueOf(this, Improvement.ImprovementType.DecreaseLOGResist);

        public string DisplaySpellDefenseDecreaseLOG => CurrentCounterspellingDice == 0
            ? SpellDefenseDecreaseLOG.ToString(GlobalOptions.CultureInfo)
            : SpellDefenseDecreaseLOG.ToString(GlobalOptions.CultureInfo) +
              LanguageManager.GetString("String_Space") + '(' +
              (SpellDefenseDecreaseLOG + CurrentCounterspellingDice).ToString(GlobalOptions.CultureInfo) + ')';

        public string SpellDefenseDecreaseLOGToolTip
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                StringBuilder sbdToolTip = new StringBuilder(LOG.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(LOG.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(')')
                    .Append(strSpace).Append('+').Append(strSpace).Append(WIL.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(WIL.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(')');

                if (CurrentCounterspellingDice != 0)
                    sbdToolTip.Append(strSpace).Append('+').Append(strSpace).Append(LanguageManager.GetString("Label_CounterspellingDice"))
                        .Append(strSpace).Append('(').Append(CurrentCounterspellingDice.ToString(GlobalOptions.CultureInfo) + ')');

                int intModifiers = SpellResistance + ImprovementManager.ValueOf(this, Improvement.ImprovementType.DecreaseLOGResist);

                if(intModifiers != 0)
                {
                    FormatImprovementModifiers(
                        sbdToolTip,
                        new HashSet<Improvement.ImprovementType>
                        {
                            Improvement.ImprovementType.SpellResistance,
                            Improvement.ImprovementType.DecreaseLOGResist
                        },
                        strSpace,
                        intModifiers);
                }

                return sbdToolTip.ToString();
            }
        }

        public int SpellDefenseDecreaseWIL => WIL.TotalValue + WIL.TotalValue + SpellResistance + ImprovementManager.ValueOf(this, Improvement.ImprovementType.DecreaseWILResist);

        public string DisplaySpellDefenseDecreaseWIL => CurrentCounterspellingDice == 0
            ? SpellDefenseDecreaseWIL.ToString(GlobalOptions.CultureInfo)
            : SpellDefenseDecreaseWIL.ToString(GlobalOptions.CultureInfo) +
              LanguageManager.GetString("String_Space") + '(' +
              (SpellDefenseDecreaseWIL + CurrentCounterspellingDice).ToString(GlobalOptions.CultureInfo) + ')';

        public string SpellDefenseDecreaseWILToolTip
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                StringBuilder sbdToolTip = new StringBuilder(WIL.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(WIL.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(')')
                    .Append(strSpace).Append('+').Append(strSpace).Append(WIL.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(WIL.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(')');

                if (CurrentCounterspellingDice != 0)
                    sbdToolTip.Append(strSpace).Append('+').Append(strSpace).Append(LanguageManager.GetString("Label_CounterspellingDice"))
                        .Append(strSpace).Append('(').Append(CurrentCounterspellingDice.ToString(GlobalOptions.CultureInfo) + ')');

                int intModifiers = SpellResistance + ImprovementManager.ValueOf(this, Improvement.ImprovementType.DecreaseWILResist);

                if(intModifiers != 0)
                {
                    FormatImprovementModifiers(
                        sbdToolTip,
                        new HashSet<Improvement.ImprovementType>
                        {
                            Improvement.ImprovementType.SpellResistance,
                            Improvement.ImprovementType.DecreaseWILResist
                        },
                        strSpace,
                        intModifiers);
                }

                return sbdToolTip.ToString();
            }
        }
        #endregion
        #endregion

        public int Surprise => REA.TotalValue + INT.TotalValue + ImprovementManager.ValueOf(this, Improvement.ImprovementType.Surprise);

        public string SurpriseToolTip
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                StringBuilder sbdToolTip = new StringBuilder(REA.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(REA.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(')')
                    .Append(strSpace).Append('+').Append(strSpace).Append(INT.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(INT.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(')');

                if (CurrentCounterspellingDice != 0)
                    sbdToolTip.Append(strSpace).Append('+').Append(strSpace).Append(LanguageManager.GetString("Label_CounterspellingDice"))
                        .Append(strSpace).Append('(').Append(CurrentCounterspellingDice.ToString(GlobalOptions.CultureInfo)).Append(')');

                int intModifiers = ImprovementManager.ValueOf(this, Improvement.ImprovementType.Surprise);

                if (intModifiers != 0)
                {
                    FormatImprovementModifiers(
                        sbdToolTip,
                        new HashSet<Improvement.ImprovementType>
                        {
                            Improvement.ImprovementType.Surprise
                        },
                        strSpace,
                        intModifiers);
                }

                return sbdToolTip.ToString();
            }
        }

        /// <summary>
        /// The Character's total Armor Rating.
        /// </summary>
        [HubTag]
        public int TotalArmorRating =>
            ArmorRating + ImprovementManager.ValueOf(this, Improvement.ImprovementType.Armor);

        public string TotalArmorRatingToolTip
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                StringBuilder sbdToolTip = new StringBuilder(LanguageManager.GetString("Tip_Armor"))
                    .Append(strSpace).Append('(').Append(ArmorRating.ToString(GlobalOptions.CultureInfo)).Append(')');
                foreach (Improvement objLoopImprovement in Improvements)
                {
                    if (objLoopImprovement.ImproveType == Improvement.ImprovementType.Armor &&
                        objLoopImprovement.Enabled)
                    {
                        sbdToolTip.Append(strSpace).Append('+').Append(strSpace).Append(GetObjectName(objLoopImprovement))
                            .Append(strSpace).Append('(').Append(objLoopImprovement.Value.ToString(GlobalOptions.CultureInfo)).Append(')');
                    }
                }

                return sbdToolTip.ToString();
            }
        }

        /// <summary>
        /// The Character's total Armor Rating against Fire attacks.
        /// </summary>
        public int TotalFireArmorRating =>
            TotalArmorRating + ImprovementManager.ValueOf(this, Improvement.ImprovementType.FireArmor);

        /// <summary>
        /// The Character's total Armor Rating against Cold attacks.
        /// </summary>
        public int TotalColdArmorRating =>
            TotalArmorRating + ImprovementManager.ValueOf(this, Improvement.ImprovementType.ColdArmor);

        /// <summary>
        /// The Character's total Armor Rating against Electricity attacks.
        /// </summary>
        public int TotalElectricityArmorRating =>
            TotalArmorRating + ImprovementManager.ValueOf(this, Improvement.ImprovementType.ElectricityArmor);

        /// <summary>
        /// The Character's total Armor Rating against Acid attacks.
        /// </summary>
        public int TotalAcidArmorRating =>
            TotalArmorRating + ImprovementManager.ValueOf(this, Improvement.ImprovementType.AcidArmor);

        /// <summary>
        /// The Character's total Armor Rating against falling damage (AP -4 not factored in).
        /// </summary>
        public int TotalFallingArmorRating =>
            TotalArmorRating + ImprovementManager.ValueOf(this, Improvement.ImprovementType.FallingArmor);

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
                string strHighest = string.Empty;
                int intHighest = 0;
                int intTotalA = 0;
                // Run through the list of Armor currently worn and retrieve the highest total Armor rating.
                // This is used for Custom-Fit armour's stacking.
                foreach (Armor objArmor in Armor.Where(objArmor =>
                    objArmor.Equipped && !objArmor.ArmorValue.StartsWith('+')))
                {
                    int intLoopTotal = objArmor.TotalArmor;
                    string strArmorName = objArmor.Name;
                    if (objArmor.Category == "High-Fashion Armor Clothing")
                    {
                        foreach (Armor a in Armor.Where(a =>
                            (a.Category == "High-Fashion Armor Clothing" || a.ArmorOverrideValue.StartsWith('+')) &&
                            a.Equipped && objArmor.Encumbrance))
                        {
                            if (a.ArmorMods.Any(objMod =>
                                objMod.Name == "Custom Fit (Stack)" && objMod.Extra == strArmorName))
                                intLoopTotal += Convert.ToInt32(a.ArmorOverrideValue, GlobalOptions.InvariantCultureInfo);
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
                foreach (Armor objArmor in Armor.Where(objArmor =>
                    (objArmor.ArmorValue.StartsWith('+') || objArmor.ArmorOverrideValue.StartsWith('+')) &&
                    objArmor.Name != strHighest && objArmor.Category == "Clothing" && objArmor.Equipped && objArmor.Encumbrance))
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

                foreach (Armor objArmor in Armor.Where(objArmor =>
                    (objArmor.ArmorValue.StartsWith('+') || objArmor.ArmorOverrideValue.StartsWith('+')) &&
                    objArmor.Name != strHighest && objArmor.Category != "Clothing" && objArmor.Equipped && objArmor.Encumbrance))
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

                // calculate armor encumbrance
                int intSTRTotalValue = STR.TotalValue;
                if (intTotalA > intSTRTotalValue + 1)
                    return (intSTRTotalValue - intTotalA) / 2; // a negative number is expected
                return 0;
            }
        }

        #endregion

        #region Spell Defense
        public int SpellDefenseIllusionMana => LOG.TotalValue + WIL.TotalValue + SpellResistance +
                                               ImprovementManager.ValueOf(this,
                                                   Improvement.ImprovementType.ManaIllusionResist);

        public string DisplaySpellDefenseIllusionMana => CurrentCounterspellingDice == 0
            ? SpellDefenseIllusionMana.ToString(GlobalOptions.CultureInfo)
            : SpellDefenseIllusionMana.ToString(GlobalOptions.CultureInfo) +
              LanguageManager.GetString("String_Space") + '(' +
              (SpellDefenseIllusionMana + CurrentCounterspellingDice).ToString(GlobalOptions.CultureInfo) + ')';

        public string SpellDefenseIllusionManaToolTip
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                StringBuilder sbdToolTip = new StringBuilder(LOG.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(LOG.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(')')
                    .Append(strSpace).Append('+').Append(strSpace).Append(WIL.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(WIL.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(')');

                if(CurrentCounterspellingDice != 0)
                    sbdToolTip.Append(strSpace).Append('+').Append(strSpace).Append(LanguageManager.GetString("Label_CounterspellingDice"))
                        .Append(strSpace).Append('(').Append(CurrentCounterspellingDice.ToString(GlobalOptions.CultureInfo)).Append(')');

                int intModifiers = SpellResistance +
                                   ImprovementManager.ValueOf(this, Improvement.ImprovementType.ManaIllusionResist);

                if(intModifiers != 0)
                {
                    FormatImprovementModifiers(
                        sbdToolTip,
                        new HashSet<Improvement.ImprovementType>
                        {
                            Improvement.ImprovementType.SpellResistance,
                            Improvement.ImprovementType.ManaIllusionResist
                        },
                        strSpace,
                        intModifiers);
                }

                return sbdToolTip.ToString();
            }
        }

        public int SpellDefenseIllusionPhysical => LOG.TotalValue + INT.TotalValue + SpellResistance +
                                                   ImprovementManager.ValueOf(this,
                                                       Improvement.ImprovementType.PhysicalIllusionResist);

        public string DisplaySpellDefenseIllusionPhysical => CurrentCounterspellingDice == 0
            ? SpellDefenseIllusionPhysical.ToString(GlobalOptions.CultureInfo)
            : SpellDefenseIllusionPhysical.ToString(GlobalOptions.CultureInfo) +
              LanguageManager.GetString("String_Space") + '(' +
              (SpellDefenseIllusionPhysical + CurrentCounterspellingDice).ToString(GlobalOptions.CultureInfo) + ')';

        public string SpellDefenseIllusionPhysicalToolTip
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                StringBuilder sbdToolTip = new StringBuilder(LOG.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(LOG.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(')')
                    .Append(strSpace).Append('+').Append(strSpace).Append(INT.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(INT.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(')');

                if(CurrentCounterspellingDice != 0)
                    sbdToolTip.Append(strSpace).Append('+').Append(strSpace).Append(LanguageManager.GetString("Label_CounterspellingDice"))
                        .Append(strSpace).Append('(').Append(CurrentCounterspellingDice.ToString(GlobalOptions.CultureInfo)).Append(')');

                int intModifiers = SpellResistance +
                                   ImprovementManager.ValueOf(this, Improvement.ImprovementType.PhysicalIllusionResist);

                if(intModifiers != 0)
                {
                    FormatImprovementModifiers(
                        sbdToolTip,
                        new HashSet<Improvement.ImprovementType>
                        {
                            Improvement.ImprovementType.SpellResistance,
                            Improvement.ImprovementType.PhysicalIllusionResist
                        },
                        strSpace,
                        intModifiers);
                }

                return sbdToolTip.ToString();
            }
        }

        public int SpellDefenseManipulationMental => LOG.TotalValue + WIL.TotalValue + SpellResistance +
                                                     ImprovementManager.ValueOf(this,
                                                         Improvement.ImprovementType.MentalManipulationResist);

        public string DisplaySpellDefenseManipulationMental => CurrentCounterspellingDice == 0
            ? SpellDefenseManipulationMental.ToString(GlobalOptions.CultureInfo)
            : SpellDefenseManipulationMental.ToString(GlobalOptions.CultureInfo) +
              LanguageManager.GetString("String_Space") + '(' +
              (SpellDefenseManipulationMental + CurrentCounterspellingDice).ToString(GlobalOptions.CultureInfo) + ')';

        public string SpellDefenseManipulationMentalToolTip
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                StringBuilder sbdToolTip = new StringBuilder(LOG.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(LOG.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(')')
                    .Append(strSpace).Append('+').Append(strSpace).Append(WIL.DisplayAbbrev)
                    .Append(strSpace).Append('(').Append(WIL.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(')');

                if(CurrentCounterspellingDice != 0)
                    sbdToolTip.Append(strSpace).Append('+').Append(strSpace).Append(LanguageManager.GetString("Label_CounterspellingDice"))
                        .Append(strSpace).Append('(').Append(CurrentCounterspellingDice.ToString(GlobalOptions.CultureInfo)).Append(')');

                int intModifiers = SpellResistance +
                                   ImprovementManager.ValueOf(this, Improvement.ImprovementType.MentalManipulationResist);

                if(intModifiers != 0)
                {
                    FormatImprovementModifiers(
                        sbdToolTip,
                        new HashSet<Improvement.ImprovementType>
                        {
                            Improvement.ImprovementType.SpellResistance,
                            Improvement.ImprovementType.MentalManipulationResist
                        },
                        strSpace,
                        intModifiers);
                }

                return sbdToolTip.ToString();
            }
        }

        public int SpellDefenseManipulationPhysical =>
            (IsAI ? (HomeNode is Vehicle objVehicle ? objVehicle.TotalBody * 2 : 0) : BOD.TotalValue + STR.TotalValue) +
            SpellResistance + ImprovementManager.ValueOf(this, Improvement.ImprovementType.PhysicalManipulationResist);

        public string DisplaySpellDefenseManipulationPhysical => CurrentCounterspellingDice == 0
            ? SpellDefenseManipulationPhysical.ToString(GlobalOptions.CultureInfo)
            : SpellDefenseManipulationPhysical.ToString(GlobalOptions.CultureInfo) +
              LanguageManager.GetString("String_Space") + '(' +
              (SpellDefenseManipulationPhysical + CurrentCounterspellingDice).ToString(GlobalOptions.CultureInfo) + ')';

        public string SpellDefenseManipulationPhysicalToolTip
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                int intBody;
                int intStrength;
                string strBodyAbbrev;
                string strStrengthAbbrev;
                if(IsAI)
                {
                    intBody = intStrength = (HomeNode is Vehicle objVehicle ? objVehicle.TotalBody : 0);
                    strBodyAbbrev = strStrengthAbbrev = LanguageManager.GetString("String_VehicleBody");
                }
                else
                {
                    intBody = BOD.TotalValue;
                    intStrength = STR.TotalValue;
                    strBodyAbbrev = BOD.DisplayAbbrev;
                    strStrengthAbbrev = STR.DisplayAbbrev;
                }

                StringBuilder sbdToolTip = new StringBuilder(strBodyAbbrev)
                    .Append(strSpace).Append('(').Append(intBody.ToString(GlobalOptions.CultureInfo)).Append(')')
                    .Append(strSpace).Append('+').Append(strSpace).Append(strStrengthAbbrev)
                    .Append(strSpace).Append('(').Append(intStrength.ToString(GlobalOptions.CultureInfo)).Append(')');

                if(CurrentCounterspellingDice != 0)
                    sbdToolTip.Append(strSpace).Append('+').Append(strSpace).Append(LanguageManager.GetString("Label_CounterspellingDice"))
                        .Append(strSpace).Append('(').Append(CurrentCounterspellingDice.ToString(GlobalOptions.CultureInfo)).Append(')');

                int intModifiers = SpellResistance +
                                   ImprovementManager.ValueOf(this, Improvement.ImprovementType.PhysicalManipulationResist);

                if(intModifiers != 0)
                {
                    FormatImprovementModifiers(
                        sbdToolTip,
                        new HashSet<Improvement.ImprovementType>
                        {
                            Improvement.ImprovementType.SpellResistance,
                            Improvement.ImprovementType.PhysicalManipulationResist
                        },
                        strSpace,
                        intModifiers);
                }

                return sbdToolTip.ToString();
            }
        }
        #endregion

        /// <summary>
        /// Custom Drugs created by the character.
        /// </summary>
        public TaggedObservableCollection<Drug> Drugs => _lstDrugs;

        #region Condition Monitors

        /// <summary>
        /// Number of Physical Condition Monitor boxes.
        /// </summary>
        public int PhysicalCM
        {
            get
            {
                int intCMPhysical = 8;
                if(IsAI)
                {
                    if(HomeNode is Vehicle objVehicle)
                    {
                        return objVehicle.PhysicalCM;
                    }
                    if (DEP != null)
                    // A.I.s use Core Condition Monitors instead of Physical Condition Monitors if they are not in a vehicle or drone.
                        intCMPhysical += (DEP.TotalValue + 1) / 2;
                }
                else
                {
                    if (BOD != null)
                        intCMPhysical += (BOD.TotalValue + 1) / 2;
                }

                // Include Improvements in the Condition Monitor values.
                intCMPhysical += ImprovementManager.ValueOf(this, Improvement.ImprovementType.PhysicalCM);
                return intCMPhysical;
            }
        }

        public string PhysicalCMLabelText
        {
            get
            {
                if(IsAI)
                {
                    return HomeNode == null
                        ? LanguageManager.GetString("Label_OtherCoreCM")
                        : LanguageManager.GetString(HomeNode is Vehicle ? "Label_OtherPhysicalCM" : "Label_OtherCoreCM");
                }
                return LanguageManager.GetString("Label_OtherPhysicalCM");
            }
        }

        public string PhysicalCMToolTip
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                string strModifiers = LanguageManager.GetString("Tip_Modifiers");
                StringBuilder sbdCM;
                int intBonus;
                if(IsAI)
                {
                    if(HomeNode is Vehicle objVehicleHomeNode)
                    {
                        sbdCM = new StringBuilder(objVehicleHomeNode.BasePhysicalBoxes.ToString(GlobalOptions.CultureInfo))
                            .Append(strSpace).Append('+').Append(strSpace).Append('(').Append(BOD.DisplayAbbrev)
                            .Append('÷').Append(2.ToString(GlobalOptions.CultureInfo)).Append(')')
                            .Append(strSpace).Append('(').Append(((objVehicleHomeNode.TotalBody + 1) / 2).ToString(GlobalOptions.CultureInfo)).Append(')');

                        intBonus = objVehicleHomeNode.Mods.Sum(objMod => objMod.ConditionMonitor);
                        if(intBonus != 0)
                            sbdCM.Append(strSpace).Append('+').Append(strSpace).Append(strModifiers)
                                .Append(strSpace).Append('(').Append(intBonus.ToString(GlobalOptions.CultureInfo)).Append(')');
                    }
                    else
                    {
                        sbdCM = new StringBuilder(8.ToString(GlobalOptions.CultureInfo))
                            .Append(strSpace).Append('+').Append(strSpace).Append('(').Append(DEP.DisplayAbbrev)
                            .Append('÷').Append(2.ToString(GlobalOptions.CultureInfo)).Append(')')
                            .Append(strSpace).Append('(').Append(((DEP.TotalValue + 1) / 2).ToString(GlobalOptions.CultureInfo)).Append(')');

                        intBonus = ImprovementManager.ValueOf(this, Improvement.ImprovementType.PhysicalCM);
                        if(intBonus != 0)
                            sbdCM.Append(strSpace).Append('+').Append(strSpace).Append(strModifiers)
                                .Append(strSpace).Append('(').Append(intBonus.ToString(GlobalOptions.CultureInfo)).Append(')');
                    }
                }
                else
                {
                    sbdCM = new StringBuilder(8.ToString(GlobalOptions.CultureInfo))
                        .Append(strSpace).Append('+').Append(strSpace).Append('(').Append(BOD.DisplayAbbrev)
                        .Append('÷').Append(2.ToString(GlobalOptions.CultureInfo)).Append(')')
                        .Append(strSpace).Append('(').Append(((BOD.TotalValue + 1) / 2).ToString(GlobalOptions.CultureInfo)).Append(')');

                    intBonus = ImprovementManager.ValueOf(this, Improvement.ImprovementType.PhysicalCM);
                    if(intBonus != 0)
                        sbdCM.Append(strSpace).Append('+').Append(strSpace).Append(strModifiers)
                            .Append(strSpace).Append('(').Append(intBonus.ToString(GlobalOptions.CultureInfo)).Append(')');
                }

                return sbdCM.ToString();
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
                if(IsAI)
                {
                    // A.I. do not have a Stun Condition Monitor, but they do have a Matrix Condition Monitor if they are in their home node.
                    if(HomeNode != null)
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

        public bool StunCMVisible => !IsAI || HomeNode != null;

        public string StunCMLabelText
        {
            get
            {
                if(IsAI)
                {
                    return HomeNode == null ? string.Empty : LanguageManager.GetString("Label_OtherMatrixCM");
                }
                return LanguageManager.GetString("Label_OtherStunCM");
            }
        }

        public string StunCMToolTip
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                string strModifiers = LanguageManager.GetString("Tip_Modifiers");
                StringBuilder sbdCM = new StringBuilder();
                int intBonus;

                if (IsAI)
                {
                    if (HomeNode != null)
                    {
                        sbdCM = new StringBuilder(8.ToString(GlobalOptions.CultureInfo))
                            .Append(strSpace).Append('+').Append(strSpace).Append('(').Append(LanguageManager.GetString("String_DeviceRating"))
                            .Append('÷').Append(2.ToString(GlobalOptions.CultureInfo)).Append(')')
                            .Append(strSpace).Append('(').Append(((HomeNode.GetTotalMatrixAttribute("Device Rating") + 1) / 2).ToString(GlobalOptions.CultureInfo)).Append(')');

                        intBonus = HomeNode.TotalBonusMatrixBoxes;
                        if (intBonus != 0)
                            sbdCM.Append(strSpace).Append('+').Append(strSpace).Append(strModifiers)
                                .Append(strSpace).Append('(').Append(intBonus.ToString(GlobalOptions.CultureInfo)).Append(')');
                    }
                }
                else
                {
                    sbdCM = new StringBuilder(8.ToString(GlobalOptions.CultureInfo))
                        .Append(strSpace).Append('+').Append(strSpace).Append('(').Append(WIL.DisplayAbbrev).Append('÷').Append(2.ToString(GlobalOptions.CultureInfo)).Append(')')
                        .Append(strSpace).Append('(').Append(((WIL.TotalValue + 1) / 2).ToString(GlobalOptions.CultureInfo)).Append(')');

                    intBonus = ImprovementManager.ValueOf(this, Improvement.ImprovementType.StunCM);
                    if (intBonus != 0)
                        sbdCM.Append(strSpace).Append('+').Append(strSpace).Append(strModifiers)
                            .Append(strSpace).Append('(').Append(intBonus.ToString(GlobalOptions.CultureInfo)).Append(')');
                }

                return sbdCM.ToString();
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
        /// Returns PhysicalCMThresholdOffset and StunCMThresholdOffset as a pair.
        /// </summary>
        public Tuple<int, int> CMThresholdOffsets =>
            new Tuple<int, int>(PhysicalCMThresholdOffset, StunCMThresholdOffset);

        /// <summary>
        /// Number of additional boxes appear before the first Physical Condition Monitor penalty.
        /// </summary>
        public int PhysicalCMThresholdOffset
        {
            get
            {
                if(Improvements.Any(objImprovement =>
                   objImprovement.ImproveType == Improvement.ImprovementType.IgnoreCMPenaltyPhysical &&
                   objImprovement.Enabled))
                    return int.MaxValue;
                if(IsAI || Improvements.Any(objImprovement =>
                       objImprovement.ImproveType == Improvement.ImprovementType.IgnoreCMPenaltyStun &&
                       objImprovement.Enabled))
                    return ImprovementManager.ValueOf(this, Improvement.ImprovementType.CMThresholdOffset) +
                           ImprovementManager.ValueOf(this, Improvement.ImprovementType.CMSharedThresholdOffset);

                int intCMThresholdOffset =
                    ImprovementManager.ValueOf(this, Improvement.ImprovementType.CMThresholdOffset);
                // We're subtracting CM Threshold from the amount of CM boxes filled because you only need to ignore wounds up to your first wound threshold, not all wounds
                int intCMSharedThresholdOffset = intCMThresholdOffset +
                                                 ImprovementManager.ValueOf(this,
                                                     Improvement.ImprovementType.CMSharedThresholdOffset) -
                                                 Math.Max(StunCMFilled - CMThreshold - intCMThresholdOffset, 0);
                return Math.Max(intCMThresholdOffset, intCMSharedThresholdOffset);
            }
        }

        /// <summary>
        /// Number of additional boxes appear before the first Stun Condition Monitor penalty.
        /// </summary>
        public int StunCMThresholdOffset
        {
            get
            {
                // A.I.s don't get wound penalties from Matrix damage
                if(IsAI)
                    return int.MaxValue;
                if(Improvements.Any(objImprovement =>
                   objImprovement.ImproveType == Improvement.ImprovementType.IgnoreCMPenaltyStun &&
                   objImprovement.Enabled))
                    return int.MaxValue;
                if(Improvements.Any(objImprovement =>
                   objImprovement.ImproveType == Improvement.ImprovementType.IgnoreCMPenaltyPhysical &&
                   objImprovement.Enabled))
                    return ImprovementManager.ValueOf(this, Improvement.ImprovementType.CMThresholdOffset) +
                           ImprovementManager.ValueOf(this, Improvement.ImprovementType.CMSharedThresholdOffset);

                int intCMThresholdOffset =
                    ImprovementManager.ValueOf(this, Improvement.ImprovementType.CMThresholdOffset);
                // We're subtracting CM Threshold from the amount of CM boxes filled because you only need to ignore wounds up to your first wound threshold, not all wounds
                int intCMSharedThresholdOffset = intCMThresholdOffset +
                                                 ImprovementManager.ValueOf(this,
                                                     Improvement.ImprovementType.CMSharedThresholdOffset) -
                                                 Math.Max(PhysicalCMFilled - CMThreshold - intCMThresholdOffset, 0);
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
                if(!IsAI)
                {
                    // Characters get a number of overflow boxes equal to their BOD (plus any Improvements). One more boxes is added to mark the character as dead.
                    intCMOverflow = BOD.TotalValue +
                                    ImprovementManager.ValueOf(this, Improvement.ImprovementType.CMOverflow) + 1;
                }

                return intCMOverflow;
            }
        }

        #endregion

        #region Build Properties

        /// <summary>
        /// Method being used to build the character.
        /// </summary>
        public CharacterBuildMethod EffectiveBuildMethod => IsCritter ? CharacterBuildMethod.Karma : Options.BuildMethod;

        public bool EffectiveBuildMethodUsesPriorityTables => EffectiveBuildMethod == CharacterBuildMethod.Priority
                                                              || EffectiveBuildMethod == CharacterBuildMethod.SumtoTen;

        public bool EffectiveBuildMethodIsLifeModule => EffectiveBuildMethod == CharacterBuildMethod.LifeModule;

        public bool EnableAutomaticStoryButton => EffectiveBuildMethodIsLifeModule && Options.AutomaticBackstory;

        /// <summary>
        /// Amount of Nuyen the character has.
        /// </summary>
        public decimal Nuyen
        {
            get => _decNuyen;
            set
            {
                if(_decNuyen != value)
                {
                    _decNuyen = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal StolenNuyen
        {
            get => _decStolenNuyen;
            set
            {
                if (_decStolenNuyen != value)
                {
                    _decStolenNuyen = value;
                    OnPropertyChanged();
                }
            }
        }

        public string DisplayNuyen => Nuyen.ToString(Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';

        public string DisplayStolenNuyen => StolenNuyen.ToString(Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';

        /// <summary>
        /// Amount of Nuyen the character started with via the priority system.
        /// </summary>
        public decimal StartingNuyen
        {
            get => _decStartingNuyen;
            set
            {
                if(_decStartingNuyen != value)
                {
                    _decStartingNuyen = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal StartingNuyenModifiers =>
            Convert.ToDecimal(ImprovementManager.ValueOf(this, Improvement.ImprovementType.Nuyen));

        public decimal TotalStartingNuyen => StartingNuyen + StartingNuyenModifiers + (NuyenBP * Options.NuyenPerBP);

        public string DisplayTotalStartingNuyen =>
            '=' + LanguageManager.GetString("String_Space") +
            TotalStartingNuyen.ToString(Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';

        /// <summary>
        /// Number of Build Points put into Nuyen.
        /// </summary>
        public decimal NuyenBP
        {
            get => _decNuyenBP;
            set
            {
                decimal decNewValue = Math.Max(Math.Min(value, TotalNuyenMaximumBP), 0);
                if(_decNuyenBP != decNewValue)
                {
                    _decNuyenBP = decNewValue;
                    OnPropertyChanged();
                }
            }
        }

        public decimal TotalNuyenMaximumBP
        {
            get
            {
                // Ensures there is no overflow in character nuyen even with max karma to nuyen and in debt quality
                const decimal decMaxValue = int.MaxValue / 2000 - 75000;
                // If UnrestrictedNuyen is enabled, return the maximum possible value
                if(IgnoreRules || Options.UnrestrictedNuyen)
                {
                    return decMaxValue;
                }

                return Math.Max(Math.Min(decMaxValue,
                    Options.NuyenMaximumBP + ImprovementManager.ValueOf(this, Improvement.ImprovementType.NuyenMaxBP)), 0);
            }
        }

        /// <summary>
        /// The calculated Astral Limit.
        /// </summary>
        public int LimitAstral => Math.Max(LimitMental, LimitSocial);

        public string LimitAstralToolTip
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                return string.Concat(LanguageManager.GetString("Label_Options_Maximum"),
                    strSpace, "(", LanguageManager.GetString("String_LimitMentalShort"),
                    strSpace, "[", LimitMental.ToString(GlobalOptions.CultureInfo), "],",
                    strSpace, LanguageManager.GetString("String_LimitSocialShort"),
                    strSpace, "[", LimitSocial.ToString(GlobalOptions.CultureInfo), "])");
            }
        }

        /// <summary>
        /// The calculated Physical Limit.
        /// </summary>
        public int LimitPhysical
        {
            get
            {
                if(IsAI)
                {
                    Vehicle objHomeNodeVehicle = HomeNode as Vehicle;
                    return objHomeNodeVehicle?.Handling ?? 0;
                }

                int intLimit = (STR.TotalValue * 2 + BOD.TotalValue + REA.TotalValue + 2) / 3;
                return intLimit + ImprovementManager.ValueOf(this, Improvement.ImprovementType.PhysicalLimit);
            }
        }

        public string LimitPhysicalToolTip
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                if(IsAI)
                {
                    Vehicle objHomeNodeVehicle = HomeNode as Vehicle;
                    return string.Concat(LanguageManager.GetString("String_Handling"),
                        strSpace, "[", (objHomeNodeVehicle?.Handling ?? 0).ToString(GlobalOptions.CultureInfo), "]");
                }

                StringBuilder sbdToolTip = new StringBuilder("(").Append(STR.DisplayAbbrev)
                    .Append(strSpace).Append('[').Append(STR.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(']')
                    .Append(strSpace).Append('×').Append(strSpace).Append(2.ToString(GlobalOptions.CultureInfo))
                    .Append(strSpace).Append('+').Append(strSpace).Append(BOD.DisplayAbbrev)
                    .Append(strSpace).Append('[').Append(BOD.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(']')
                    .Append(strSpace).Append('+').Append(strSpace).Append(REA.DisplayAbbrev)
                    .Append(strSpace).Append('[').Append(REA.TotalValue.ToString(GlobalOptions.CultureInfo)).Append("])")
                    .Append(strSpace).Append('/').Append(strSpace).Append(3.ToString(GlobalOptions.CultureInfo));

                foreach(Improvement objLoopImprovement in Improvements)
                {
                    if (objLoopImprovement.ImproveType == Improvement.ImprovementType.PhysicalLimit
                        && objLoopImprovement.Enabled)
                    {
                        sbdToolTip.Append(strSpace).Append('+').Append(strSpace).Append(GetObjectName(objLoopImprovement))
                            .Append(strSpace).Append('(').Append(objLoopImprovement.Value.ToString(GlobalOptions.CultureInfo)).Append(')');
                    }
                }

                return sbdToolTip.ToString();
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
                if(IsAI)
                {
                    if(HomeNode != null)
                    {
                        if(HomeNode is Vehicle objHomeNodeVehicle)
                        {
                            int intHomeNodeSensor = objHomeNodeVehicle.CalculatedSensor;
                            if(intHomeNodeSensor > intLimit)
                            {
                                intLimit = intHomeNodeSensor;
                            }
                        }

                        int intHomeNodeDP = HomeNode.GetTotalMatrixAttribute("Data Processing");
                        if(intHomeNodeDP > intLimit)
                        {
                            intLimit = intHomeNodeDP;
                        }
                    }
                }

                return intLimit + ImprovementManager.ValueOf(this, Improvement.ImprovementType.MentalLimit);
            }
        }

        public string LimitMentalToolTip
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                StringBuilder sbdToolTip = new StringBuilder("(").Append(LOG.DisplayAbbrev)
                    .Append(strSpace).Append('[').Append(LOG.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(']')
                    .Append(strSpace).Append('×').Append(strSpace).Append(2.ToString(GlobalOptions.CultureInfo))
                    .Append(strSpace).Append('+').Append(strSpace).Append(INT.DisplayAbbrev)
                    .Append(strSpace).Append('[').Append(INT.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(']')
                    .Append(strSpace).Append('+').Append(strSpace).Append(WIL.DisplayAbbrev)
                    .Append(strSpace).Append('[').Append(WIL.TotalValue.ToString(GlobalOptions.CultureInfo)).Append("])")
                    .Append(strSpace).Append('/').Append(strSpace).Append(3.ToString(GlobalOptions.CultureInfo));

                if(IsAI)
                {
                    int intLimit = (LOG.TotalValue * 2 + INT.TotalValue + WIL.TotalValue + 2) / 3;
                    if(HomeNode != null)
                    {
                        if(HomeNode is Vehicle objHomeNodeVehicle)
                        {
                            int intHomeNodeSensor = objHomeNodeVehicle.CalculatedSensor;
                            if(intHomeNodeSensor > intLimit)
                            {
                                intLimit = intHomeNodeSensor;
                                sbdToolTip = new StringBuilder(LanguageManager.GetString("String_Sensor"))
                                    .Append(strSpace).Append('[').Append(intLimit.ToString(GlobalOptions.CultureInfo)).Append(']');
                            }
                        }

                        int intHomeNodeDP = HomeNode.GetTotalMatrixAttribute("Data Processing");
                        if(intHomeNodeDP > intLimit)
                        {
                            intLimit = intHomeNodeDP;
                            sbdToolTip = new StringBuilder(LanguageManager.GetString("String_DataProcessing"))
                                .Append(strSpace).Append('[').Append(intLimit.ToString(GlobalOptions.CultureInfo)).Append(']');
                        }
                    }
                }

                foreach(Improvement objLoopImprovement in Improvements)
                {
                    if (objLoopImprovement.ImproveType == Improvement.ImprovementType.MentalLimit
                        && objLoopImprovement.Enabled)
                    {
                        sbdToolTip.Append(strSpace).Append('+').Append(strSpace).Append(GetObjectName(objLoopImprovement))
                            .Append(strSpace).Append('(').Append(objLoopImprovement.Value.ToString(GlobalOptions.CultureInfo)).Append(')');
                    }
                }

                return sbdToolTip.ToString();
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
                if(IsAI && HomeNode != null)
                {
                    int intHomeNodeDP = HomeNode.GetTotalMatrixAttribute("Data Processing");

                    if(HomeNode is Vehicle objHomeNodeVehicle)
                    {
                        int intHomeNodePilot = objHomeNodeVehicle.Pilot;
                        if(intHomeNodePilot > intHomeNodeDP)
                            intHomeNodeDP = intHomeNodePilot;
                    }

                    intLimit = (CHA.TotalValue + intHomeNodeDP + WIL.TotalValue +
                                decimal.ToInt32(decimal.Ceiling(Essence())) + 2) / 3;
                }
                else
                {
                    intLimit = (CHA.TotalValue * 2 + WIL.TotalValue + decimal.ToInt32(decimal.Ceiling(Essence())) + 2) /
                               3;
                }

                return intLimit + ImprovementManager.ValueOf(this, Improvement.ImprovementType.SocialLimit);
            }
        }

        public string LimitSocialToolTip
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                StringBuilder sbdToolTip = new StringBuilder("(").Append(CHA.DisplayAbbrev)
                    .Append(strSpace).Append('[').Append(CHA.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(']');

                if(IsAI && HomeNode != null)
                {
                    int intHomeNodeDP = HomeNode.GetTotalMatrixAttribute("Data Processing");
                    string strDPString = LanguageManager.GetString("String_DataProcessing");
                    if(HomeNode is Vehicle objHomeNodeVehicle)
                    {
                        int intHomeNodePilot = objHomeNodeVehicle.Pilot;
                        if(intHomeNodePilot > intHomeNodeDP)
                        {
                            intHomeNodeDP = intHomeNodePilot;
                            strDPString = LanguageManager.GetString("String_Pilot");
                        }
                    }

                    sbdToolTip.Append(strSpace).Append('+').Append(strSpace).Append(strDPString)
                        .Append(strSpace).Append('[').Append(intHomeNodeDP.ToString(GlobalOptions.CultureInfo)).Append(']');
                }
                else
                {
                    sbdToolTip.Append(strSpace).Append('×').Append(strSpace).Append(2.ToString(GlobalOptions.CultureInfo));
                }

                sbdToolTip.Append(strSpace).Append('+').Append(strSpace).Append(WIL.DisplayAbbrev)
                    .Append(strSpace).Append('[').Append(WIL.TotalValue.ToString(GlobalOptions.CultureInfo)).Append(']')
                    .Append(strSpace).Append('+').Append(strSpace).Append(ESS.DisplayAbbrev)
                    .Append(strSpace).Append('[').Append(DisplayEssence).Append("])")
                    .Append(strSpace).Append('/').Append(strSpace).Append(3.ToString(GlobalOptions.CultureInfo));

                foreach(Improvement objLoopImprovement in Improvements)
                {
                    if(objLoopImprovement.ImproveType == Improvement.ImprovementType.SocialLimit
                       && objLoopImprovement.Enabled)
                    {
                        sbdToolTip.Append(strSpace).Append('+').Append(strSpace).Append(GetObjectName(objLoopImprovement))
                            .Append(strSpace).Append('(').Append(objLoopImprovement.Value.ToString(GlobalOptions.CultureInfo)).Append(')');
                    }
                }

                return sbdToolTip.ToString();
            }
        }

        public bool HasMentorSpirit => MentorSpirits.Count > 0;

        public string FirstMentorSpiritDisplayName => MentorSpirits.Count > 0
            ? MentorSpirits[0].DisplayNameShort(GlobalOptions.Language)
            : string.Empty;

        public string FirstMentorSpiritDisplayInformation
        {
            get
            {
                if(MentorSpirits.Count == 0)
                    return string.Empty;

                MentorSpirit objMentorSpirit = MentorSpirits[0];
                string strSpace = LanguageManager.GetString("String_Space");
                StringBuilder sbdReturn = new StringBuilder(LanguageManager.GetString("Label_SelectMentorSpirit_Advantage"))
                    .Append(strSpace).AppendLine(objMentorSpirit.DisplayAdvantage(GlobalOptions.Language))
                    .AppendLine().Append(LanguageManager.GetString("Label_SelectMetamagic_Disadvantage"))
                    .Append(strSpace).AppendLine(objMentorSpirit.Disadvantage);
                return sbdReturn.ToString().WordWrap();
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
            set
            {
                if(_strMetatype != value)
                {
                    _strMetatype = value;
                    OnPropertyChanged();
                }
            }
        }

        public Guid MetatypeGuid
        {
            get => _guiMetatype;
            set => _guiMetatype = value;
        }

        /// <summary>
        /// The name of the metatype as it should appear on printouts (translated name only).
        /// </summary>
        public string DisplayMetatype(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Metatype;

            return GetNode(true)?.SelectSingleNode("translate")?.Value ?? Metatype;
        }

        /// <summary>
        /// Character's Metavariant.
        /// </summary>
        public string Metavariant
        {
            get => _strMetavariant;
            set
            {
                if(_strMetavariant != value)
                {
                    _strMetavariant = value;
                    OnPropertyChanged();
                }
            }
        }

        public Guid MetavariantGuid
        {
            get => _guiMetavariant;
            set => _guiMetavariant = value;
        }

        /// <summary>
        /// The name of the metatype as it should appear on printouts (translated name only).
        /// </summary>
        public string DisplayMetavariant(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Metavariant;

            return GetNode()?.SelectSingleNode("translate")?.Value ?? Metavariant;
        }

        public string FormattedMetatype => FormattedMetatypeMethod(GlobalOptions.Language);
        /// <summary>
        /// The metatype, including metavariant if any, in an appropriate language.
        /// </summary>
        /// <param name="strLanguage">Language to be used. Defaults to GlobalOptions.Language</param>
        /// <returns></returns>
        public string FormattedMetatypeMethod(string strLanguage = "")
        {
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalOptions.Language;
            string strMetatype = DisplayMetatype(strLanguage);

            if (MetavariantGuid != Guid.Empty)
            {
                strMetatype += LanguageManager.GetString("String_Space") + '(' + DisplayMetavariant(strLanguage) + ')';
            }

            return strMetatype;
        }

        /// <summary>
        /// Metatype Category.
        /// </summary>
        public string MetatypeCategory
        {
            get => _strMetatypeCategory;
            set
            {
                if(_strMetatypeCategory != value)
                {
                    bool blnDoCyberzombieRefresh = _strMetatypeCategory == "Cyberzombie" || value == "Cyberzombie";
                    _strMetatypeCategory = value;
                    OnPropertyChanged();
                    if(blnDoCyberzombieRefresh)
                        RefreshEssenceLossImprovements();
                }
            }
        }

        public int LimbCount(string strLimbSlot = "")
        {
            if(string.IsNullOrEmpty(strLimbSlot))
            {
                return Options.LimbCount + ImprovementManager.ValueOf(this, Improvement.ImprovementType.AddLimb);
            }

            int intReturn =
                1 + ImprovementManager.ValueOf(this, Improvement.ImprovementType.AddLimb, false, strLimbSlot);
            if(strLimbSlot == "arm" || strLimbSlot == "leg")
                intReturn += 1;
            return intReturn;
        }

        public string DisplayMovement => GetMovement(GlobalOptions.CultureInfo, GlobalOptions.Language);

        /// <summary>
        /// Character's Movement rate (Culture-dependent).
        /// </summary>
        public string GetMovement(CultureInfo objCulture, string strLanguage)
        {
            // Don't attempt to do anything if the character's Movement is "Special" (typically for A.I.s).
            if(Movement == "Special")
            {
                return LanguageManager.GetString("String_ModeSpecial", strLanguage);
            }

            return CalculatedMovement("Ground", true, objCulture);
        }

        /// <summary>
        /// Character's Movement rate data string.
        /// </summary>
        public string Movement
        {
            get
            {
                if(string.IsNullOrWhiteSpace(_strMovement))
                {
                    _strMovement = GetNode().SelectSingleNode("movement")?.Value
                                   ?? GetNode(true).SelectSingleNode("movement")?.Value
                                   ?? string.Empty;
                }

                return _strMovement;
            }
            set
            {
                if(_strMovement != value)
                {
                    _strMovement = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Character's Run rate data string.
        /// </summary>
        public string RunString
        {
            get
            {
                if(string.IsNullOrWhiteSpace(_strRun))
                {
                    _strRun = GetNode().SelectSingleNode("run")?.Value
                              ?? GetNode(true).SelectSingleNode("run")?.Value
                              ?? string.Empty;
                }

                return _strRun;
            }
            set
            {
                if(_strRun != value)
                {
                    _strRun = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Character's Alternate Run rate data string.
        /// </summary>
        public string RunAltString
        {
            get
            {
                if(string.IsNullOrWhiteSpace(_strRunAlt))
                {
                    _strRunAlt = GetNode().SelectSingleNode("run")?.GetAttribute("alt", string.Empty)
                                 ?? GetNode(true).SelectSingleNode("run")?.GetAttribute("alt", string.Empty)
                                 ?? string.Empty;
                }

                return _strRunAlt;
            }
            set
            {
                if(_strRunAlt != value)
                {
                    _strRunAlt = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Character's Walk rate data string.
        /// </summary>
        public string WalkString
        {
            get
            {
                if(string.IsNullOrWhiteSpace(_strWalk))
                {
                    _strWalk = GetNode().SelectSingleNode("walk")?.Value
                               ?? GetNode(true).SelectSingleNode("walk")?.Value
                               ?? string.Empty;
                }

                return _strWalk;
            }
            set
            {
                if(_strWalk != value)
                {
                    _strWalk = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Character's Alternate Walk rate data string.
        /// </summary>
        public string WalkAltString
        {
            get
            {
                if(string.IsNullOrWhiteSpace(_strWalkAlt))
                {
                    _strWalkAlt = GetNode().SelectSingleNode("walk")?.GetAttribute("alt", string.Empty)
                                  ?? GetNode(true).SelectSingleNode("walk")?.GetAttribute("alt", string.Empty)
                                  ?? string.Empty;
                }

                return _strWalkAlt;
            }
            set
            {
                if(_strWalkAlt != value)
                {
                    _strWalkAlt = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Character's Sprint rate data string.
        /// </summary>
        public string SprintString
        {
            get
            {
                if(string.IsNullOrWhiteSpace(_strSprint))
                {
                    _strSprint = GetNode().SelectSingleNode("sprint")?.Value
                                 ?? GetNode(true).SelectSingleNode("sprint")?.Value
                                 ?? string.Empty;
                }

                return _strSprint;
            }
            set
            {
                if(_strSprint != value)
                {
                    _strSprint = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Character's Alternate Sprint rate data string.
        /// </summary>
        public string SprintAltString
        {
            get
            {
                if(string.IsNullOrWhiteSpace(_strSprintAlt))
                {
                    _strSprintAlt = GetNode().SelectSingleNode("sprint")?.GetAttribute("alt", string.Empty)
                                    ?? GetNode(true).SelectSingleNode("sprint")?.GetAttribute("alt", string.Empty)
                                    ?? string.Empty;
                }

                return _strSprintAlt;
            }
            set
            {
                if(_strSprintAlt != value)
                {
                    _strSprintAlt = value;
                    OnPropertyChanged();
                }
            }
        }

        public string CurrentWalkingRateString =>
            AttributeSection.AttributeCategory == CharacterAttrib.AttributeCategory.Standard
                ? WalkString
                : WalkAltString;

        public string CurrentRunningRateString =>
            AttributeSection.AttributeCategory == CharacterAttrib.AttributeCategory.Standard
                ? RunString
                : RunAltString;

        public string CurrentSprintingRateString =>
            AttributeSection.AttributeCategory == CharacterAttrib.AttributeCategory.Standard
                ? SprintString
                : SprintAltString;

        /// <summary>
        /// Character's running Movement rate.
        /// <param name="strType">Takes one of three parameters: Ground, 2 for Swim, 3 for Fly. Returns 0 if the requested type isn't found.</param>
        /// </summary>
        public int WalkingRate(string strType = "Ground")
        {
            int intTmp = int.MinValue;
            foreach(Improvement objImprovement in Improvements.Where(i =>
               i.ImproveType == Improvement.ImprovementType.WalkSpeed && i.ImprovedName == strType && i.Enabled))
            {
                intTmp = Math.Max(intTmp, objImprovement.Value);
            }

            if(intTmp != int.MinValue)
                return intTmp;

            string[] strReturn = CurrentWalkingRateString.Split('/', StringSplitOptions.RemoveEmptyEntries);

            switch(strType)
            {
                case "Fly":
                    if(strReturn.Length > 2)
                        int.TryParse(strReturn[2], NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out intTmp);
                    break;
                case "Swim":
                    if(strReturn.Length > 1)
                        int.TryParse(strReturn[1], NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out intTmp);
                    break;
                case "Ground":
                    if(strReturn.Length > 0)
                        int.TryParse(strReturn[0], NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out intTmp);
                    break;
            }

            return intTmp;
        }

        /// <summary>
        /// Character's running Movement rate.
        /// <param name="strType">Takes one of three parameters: Ground, 2 for Swim, 3 for Fly. Returns 0 if the requested type isn't found.</param>
        /// </summary>
        public int RunningRate(string strType = "Ground")
        {
            int intTmp = int.MinValue;
            foreach(Improvement objImprovement in Improvements.Where(i =>
               i.ImproveType == Improvement.ImprovementType.RunSpeed && i.ImprovedName == strType && i.Enabled))
            {
                intTmp = Math.Max(intTmp, objImprovement.Value);
            }

            if(intTmp != int.MinValue)
                return intTmp;

            string[] strReturn = CurrentRunningRateString.Split('/', StringSplitOptions.RemoveEmptyEntries);

            switch(strType)
            {
                case "Fly":
                    if(strReturn.Length > 2)
                        int.TryParse(strReturn[2], NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out intTmp);
                    break;
                case "Swim":
                    if(strReturn.Length > 1)
                        int.TryParse(strReturn[1], NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out intTmp);
                    break;
                case "Ground":
                    if(strReturn.Length > 0)
                        int.TryParse(strReturn[0], NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out intTmp);
                    break;
            }

            return intTmp;
        }

        /// <summary>
        /// Character's sprinting Movement rate (meters per hit).
        /// <param name="strType">Takes one of three parameters: Ground, 2 for Swim, 3 for Fly. Returns 0 if the requested type isn't found.</param>
        /// </summary>
        public decimal SprintingRate(string strType = "Ground")
        {
            decimal decTmp = decimal.MinValue;
            foreach(Improvement objImprovement in Improvements.Where(i =>
               i.ImproveType == Improvement.ImprovementType.SprintSpeed && i.ImprovedName == strType && i.Enabled))
            {
                decTmp = Math.Max(decTmp, objImprovement.Value / 100.0m);
            }

            if(decTmp != decimal.MinValue)
                return decTmp;

            string[] strReturn = CurrentSprintingRateString.Split('/', StringSplitOptions.RemoveEmptyEntries);

            switch(strType)
            {
                case "Fly":
                    if(strReturn.Length > 2)
                        decimal.TryParse(strReturn[2], NumberStyles.Any, GlobalOptions.InvariantCultureInfo,
                            out decTmp);
                    break;
                case "Swim":
                    if(strReturn.Length > 1)
                        decimal.TryParse(strReturn[1], NumberStyles.Any, GlobalOptions.InvariantCultureInfo,
                            out decTmp);
                    break;
                case "Ground":
                    if(strReturn.Length > 0)
                        decimal.TryParse(strReturn[0], NumberStyles.Any, GlobalOptions.InvariantCultureInfo,
                            out decTmp);
                    break;
            }

            return decTmp;
        }

        public string CalculatedMovement(string strMovementType, bool blnUseCyberlegs = false,
            CultureInfo objCulture = null)
        {
            decimal decSprint = SprintingRate(strMovementType) +
                                ImprovementManager.ValueOf(this, Improvement.ImprovementType.SprintBonus, false,
                                    strMovementType) / 100.0m;
            decimal decRun = RunningRate(strMovementType) + ImprovementManager.ValueOf(this,
                                 Improvement.ImprovementType.RunMultiplier, false, strMovementType);
            decimal decWalk = WalkingRate(strMovementType) + ImprovementManager.ValueOf(this,
                                  Improvement.ImprovementType.WalkMultiplier, false, strMovementType);
            // Everything else after this just multiplies values, so zeroes can be checked for here
            if(decWalk == 0 && decRun == 0 && decSprint == 0)
            {
                return "0";
            }

            decSprint *= 1.0m + ImprovementManager.ValueOf(this, Improvement.ImprovementType.SprintBonusPercent, false,
                             strMovementType) / 100.0m;
            decRun *= 1.0m + ImprovementManager.ValueOf(this, Improvement.ImprovementType.RunMultiplierPercent, false,
                          strMovementType) / 100.0m;
            decWalk *= 1.0m + ImprovementManager.ValueOf(this, Improvement.ImprovementType.WalkMultiplierPercent, false,
                           strMovementType) / 100.0m;

            int intAGI = AGI.CalculatedTotalValue(false);
            int intSTR = STR.CalculatedTotalValue(false);
            if(Options.CyberlegMovement && blnUseCyberlegs)
            {
                int intTempAGI = int.MaxValue;
                int intTempSTR = int.MaxValue;
                int intLegs = 0;
                foreach(Cyberware objCyber in Cyberware.Where(objCyber => objCyber.LimbSlot == "leg"))
                {
                    intLegs += objCyber.LimbSlotCount;
                    intTempAGI = Math.Min(intTempAGI, objCyber.TotalAgility);
                    intTempSTR = Math.Min(intTempSTR, objCyber.TotalStrength);
                }

                if(intTempAGI != int.MaxValue && intTempSTR != int.MaxValue && intLegs >= 2)
                {
                    intAGI = intTempAGI;
                    intSTR = intTempSTR;
                }
            }

            if(objCulture == null)
                objCulture = GlobalOptions.CultureInfo;
            StringBuilder sbdReturn;
            string strSpace = LanguageManager.GetString("String_Space");
            if(strMovementType == "Swim")
            {
                decWalk *= (intAGI + intSTR) * 0.5m;
                sbdReturn = new StringBuilder(decWalk.ToString("#,0.##", objCulture)).Append(',')
                    .Append(strSpace).Append(decSprint.ToString("#,0.##", objCulture)).Append(LanguageManager.GetString("String_MetersPerHit"));
            }
            else
            {
                decWalk *= intAGI;
                decRun *= intAGI;
                sbdReturn = new StringBuilder(decWalk.ToString("#,0.##", objCulture))
                    .Append('/').Append(decRun.ToString("#,0.##", objCulture)).Append(',')
                    .Append(strSpace).Append(decSprint.ToString("#,0.##", objCulture)).Append(LanguageManager.GetString("String_MetersPerHit"));
            }

            return sbdReturn.ToString();
        }

        public string DisplaySwim => GetSwim(GlobalOptions.CultureInfo, GlobalOptions.Language);

        /// <summary>
        /// Character's Swim rate.
        /// </summary>
        public string GetSwim(CultureInfo objCulture, string strLanguage)
        {
            // Don't attempt to do anything if the character's Movement is "Special" (typically for A.I.s).
            if(Movement == "Special")
            {
                return LanguageManager.GetString("String_ModeSpecial", strLanguage);
            }

            return CalculatedMovement("Swim", false, objCulture);
        }

        public string DisplayFly => GetFly(GlobalOptions.CultureInfo, GlobalOptions.Language);

        /// <summary>
        /// Character's Fly rate.
        /// </summary>
        public string GetFly(CultureInfo objCulture, string strLanguage)
        {
            // Don't attempt to do anything if the character's Movement is "Special" (typically for A.I.s).
            if(Movement == "Special")
            {
                return LanguageManager.GetString("String_ModeSpecial", strLanguage);
            }

            return CalculatedMovement("Fly", false, objCulture);
        }

        /// <summary>
        /// Full Movement (Movement, Swim, and Fly) for printouts.
        /// </summary>
        private string FullMovement(CultureInfo objCulture, string strLanguage)
        {
            string strSpace = LanguageManager.GetString("String_Space");
            StringBuilder sbdReturn = new StringBuilder();
            string strGroundMovement = GetMovement(objCulture, strLanguage);
            string strSwimMovement = GetSwim(objCulture, strLanguage);
            string strFlyMovement = GetFly(objCulture, strLanguage);
            if(!string.IsNullOrEmpty(strGroundMovement) && strGroundMovement != "0")
                sbdReturn.Append(strGroundMovement).Append(',').Append(strSpace);
            if(!string.IsNullOrEmpty(strSwimMovement) && strSwimMovement != "0")
                sbdReturn.Append(LanguageManager.GetString("Label_OtherSwim", strLanguage)).Append(strSpace).Append(strSwimMovement).Append(',').Append(strSpace);
            if(!string.IsNullOrEmpty(strFlyMovement) && strFlyMovement != "0")
                sbdReturn.Append(LanguageManager.GetString("Label_OtherFly", strLanguage)).Append(strSpace).Append(strFlyMovement).Append(',').Append(strSpace);

            // Remove the trailing ", ".
            if(sbdReturn.Length > 0)
                sbdReturn.Length -= 2;

            return sbdReturn.ToString();
        }

        /// <summary>
        /// BP cost of character's Metatype.
        /// </summary>
        public int MetatypeBP
        {
            get => _intMetatypeBP;
            set
            {
                if(_intMetatypeBP != value)
                {
                    _intMetatypeBP = value;
                    OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// MetatypeBP as a string, including Karma string and multiplied by options as relevant.
        /// TODO: Belongs in a viewmodel for frmCreate rather than the main character class?
        /// </summary>
        public string DisplayMetatypeBP
        {
            get
            {
                string s = string.Empty;
                switch (EffectiveBuildMethod)
                {
                    case CharacterBuildMethod.Karma:
                    case CharacterBuildMethod.LifeModule:
                        s = (MetatypeBP * Options.MetatypeCostsKarmaMultiplier).ToString(GlobalOptions.CultureInfo);
                        break;
                    case CharacterBuildMethod.Priority:
                    case CharacterBuildMethod.SumtoTen:
                        s = (MetatypeBP).ToString(GlobalOptions.CultureInfo);
                        break;
                }

                s += LanguageManager.GetString("String_Space") + LanguageManager.GetString("String_Karma");
                return s;
            }
        }

        /// <summary>
        /// Whether or not the character is a non-Free Sprite.
        /// </summary>
        public bool IsSprite
        {
            get
            {
                if(MetatypeCategory.EndsWith("Sprites", StringComparison.Ordinal) && !IsFreeSprite)
                    return true;
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
                if(MetatypeCategory == "Free Sprite")
                    return true;
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
                if(_blnAdeptEnabled != value)
                {
                    _blnAdeptEnabled = value;
                    if(!value)
                    {
                        ClearAdeptPowers();
                    }

                    OnPropertyChanged();
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
                if(_blnMagicianEnabled != value)
                {
                    _blnMagicianEnabled = value;
                    if(!value)
                    {
                        ClearMagic(AdeptEnabled);
                    }

                    OnPropertyChanged();
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
                if(_blnTechnomancerEnabled != value)
                {
                    _blnTechnomancerEnabled = value;
                    if(!value)
                    {
                        ClearResonance();
                    }

                    OnPropertyChanged();
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
                if(_blnAdvancedProgramsEnabled != value)
                {
                    _blnAdvancedProgramsEnabled = value;
                    if(!value)
                    {
                        ClearAdvancedPrograms();
                    }

                    OnPropertyChanged();
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
                if(_blnCyberwareDisabled != value)
                {
                    _blnCyberwareDisabled = value;
                    if(value)
                    {
                        ClearCyberwareTab();
                    }

                    OnPropertyChanged();
                }
            }
        }

        public bool AddCyberwareEnabled => !CyberwareDisabled
                                           && !Improvements.Any(objImprovement =>
                                               objImprovement.ImproveType == Improvement.ImprovementType.DisableCyberware
                                               && objImprovement.Enabled);

        public bool AddBiowareEnabled => !CyberwareDisabled
                                         && !Improvements.Any(objImprovement =>
                                             objImprovement.ImproveType == Improvement.ImprovementType.DisableBioware
                                             && objImprovement.Enabled);

        private int _intCachedInitiationEnabled = -1;

        /// <summary>
        /// Whether or not the Initiation tab should be shown (override for BP mode).
        /// </summary>
        public bool InitiationEnabled
        {
            get
            {
                if(_intCachedInitiationEnabled == -1)
                {
                    _intCachedInitiationEnabled = !InitiationForceDisabled && (MAGEnabled || RESEnabled) ? 1 : 0;
                }

                return _intCachedInitiationEnabled == 1;
            }
        }

        public bool InitiationForceDisabled
        {
            get => _blnInitiationDisabled;
            set
            {
                if(_blnInitiationDisabled != value)
                {
                    _blnInitiationDisabled = value;
                    if(value)
                    {
                        ClearInitiations();
                    }

                    OnPropertyChanged();
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
                if(_blnCritterEnabled != value)
                {
                    _blnCritterEnabled = value;
                    if(!value)
                    {
                        ClearCritterPowers();
                    }

                    OnPropertyChanged();
                }
            }
        }

        private int _intCachedBlackMarketDiscount = -1;

        /// <summary>
        /// Whether or not Black Market Discount is enabled.
        /// </summary>
        public bool BlackMarketDiscount
        {
            get
            {
                if(_intCachedBlackMarketDiscount < 0)
                    _intCachedBlackMarketDiscount = Improvements.Any(x =>
                        x.ImproveType == Improvement.ImprovementType.BlackMarketDiscount && x.Enabled)
                        ? 1
                        : 0;

                return _intCachedBlackMarketDiscount > 0;
            }
        }

        /// <summary>
        /// Whether or not this character can quicken spells.
        /// </summary>
        public bool QuickeningEnabled => Improvements.Any(objImprovement =>
            objImprovement.ImproveType == Improvement.ImprovementType.QuickeningMetamagic && objImprovement.Enabled);

        /// <summary>
        /// Whether or not user is getting free bioware from Prototype Transhuman.
        /// </summary>
        public decimal PrototypeTranshuman
        {
            get => _decPrototypeTranshuman;
            set
            {
                if(_decPrototypeTranshuman != value)
                {
                    if(value <= 0)
                    {
                        if(_decPrototypeTranshuman > 0)
                            foreach(Cyberware objCyberware in Cyberware)
                                if(objCyberware.PrototypeTranshuman)
                                    objCyberware.PrototypeTranshuman = false;
                        _decPrototypeTranshuman = 0;
                    }
                    else
                        _decPrototypeTranshuman = value;

                    OnPropertyChanged();
                }
            }
        }

        public bool IsPrototypeTranshuman => PrototypeTranshuman > 0;

        private int _intCachedFriendsInHighPlaces = -1;

        /// <summary>
        /// Whether or not Friends in High Places is enabled.
        /// </summary>
        public bool FriendsInHighPlaces
        {
            get
            {
                if(_intCachedFriendsInHighPlaces < 0)
                    _intCachedFriendsInHighPlaces = Improvements.Any(x =>
                        x.ImproveType == Improvement.ImprovementType.FriendsInHighPlaces && x.Enabled)
                        ? 1
                        : 0;

                return _intCachedFriendsInHighPlaces > 0;
            }
        }

        private int _intCachedExCon = -1;

        /// <summary>
        /// Whether or not ExCon is enabled.
        /// </summary>
        public bool ExCon
        {
            get
            {
                if(_intCachedExCon < 0)
                    _intCachedExCon =
                        Improvements.Any(x => x.ImproveType == Improvement.ImprovementType.ExCon && x.Enabled) ? 1 : 0;

                return _intCachedExCon > 0;
            }
        }

        private int _intCachedTrustFund = int.MinValue;

        /// <summary>
        /// Value of the Trust Fund quality.
        /// </summary>
        public int TrustFund
        {
            get
            {
                if(_intCachedTrustFund != int.MinValue)
                    return _intCachedTrustFund;

                return _intCachedTrustFund = Improvements
                    .Where(x => x.ImproveType == Improvement.ImprovementType.TrustFund && x.Enabled).DefaultIfEmpty()
                    .Max(x => x?.Value ?? 0);
            }
        }

        private int _intCachedRestrictedGear = -1;

        /// <summary>
        /// Whether or not RestrictedGear is enabled.
        /// </summary>
        public int RestrictedGear
        {
            get
            {
                if(_intCachedRestrictedGear < 0)
                {
                    foreach(Improvement objImprovment in Improvements.Where(x => x.ImproveType == Improvement.ImprovementType.RestrictedGear && x.Enabled))
                    {
                        _intCachedRestrictedGear = Math.Max(_intCachedRestrictedGear, objImprovment.Value);
                    }
                }

                return _intCachedRestrictedGear;
            }
        }

        private int _intCachedOverclocker = -1;

        /// <summary>
        /// Whether or not Overclocker is enabled.
        /// </summary>
        public bool Overclocker
        {
            get
            {
                if(_intCachedOverclocker < 0)
                    _intCachedOverclocker =
                        Improvements.Any(x => x.ImproveType == Improvement.ImprovementType.Overclocker && x.Enabled)
                            ? 1
                            : 0;

                return _intCachedOverclocker > 0;
            }
        }

        private int _intCachedMadeMan = -1;

        /// <summary>
        /// Whether or not MadeMan is enabled.
        /// </summary>
        public bool MadeMan
        {
            get
            {
                if(_intCachedMadeMan < 0)
                    _intCachedMadeMan =
                        Improvements.Any(x => x.ImproveType == Improvement.ImprovementType.MadeMan && x.Enabled)
                            ? 1
                            : 0;

                return _intCachedMadeMan > 0;
            }
        }

        private int _intCachedFame = -1;

        /// <summary>
        /// Whether or not Fame is enabled.
        /// </summary>
        public bool Fame
        {
            get
            {
                if(_intCachedFame < 0)
                    _intCachedFame =
                        Improvements.Any(x => x.ImproveType == Improvement.ImprovementType.Fame && x.Enabled) ? 1 : 0;

                return _intCachedFame > 0;
            }
        }

        private int _intCachedErased = -1;

        /// <summary>
        /// Whether or not Erased is enabled.
        /// </summary>
        public bool Erased
        {
            get
            {
                if(_intCachedErased < 0)
                    _intCachedErased =
                        Improvements.Any(x => x.ImproveType == Improvement.ImprovementType.Erased && x.Enabled) ? 1 : 0;

                return _intCachedErased > 0;
            }
        }

        private int _intCachedAllowSpriteFettering = -1;

        /// <summary>
        /// Whether or not the character is allowed to Fetter sprites. See Kill Code 91 (Sprite Pet)
        /// </summary>
        public bool AllowSpriteFettering
        {
            get
            {
                if (_intCachedAllowSpriteFettering < 0)
                    _intCachedAllowSpriteFettering =
                        Improvements.Any(x => x.ImproveType == Improvement.ImprovementType.AllowSpriteFettering && x.Enabled)
                            ? 1
                            : 0;
                return _intCachedAllowSpriteFettering > 0;
            }
        }

        /// <summary>
        /// Extended Availability Test information for an item based on the character's Negotiate Skill.
        /// </summary>
        /// <param name="decCost">Item's cost.</param>
        /// <param name="strAvail">Item's Availability.</param>
        public string AvailTest(decimal decCost, string strAvail)
        {
            if (string.IsNullOrEmpty(strAvail))
                return LanguageManager.GetString("String_None");
            bool blnShowTest = false;
            string strTestSuffix = LanguageManager.GetString("String_AvailRestricted");
            if(strAvail.EndsWith(strTestSuffix, StringComparison.Ordinal))
            {
                blnShowTest = true;
                strAvail = strAvail.TrimEndOnce(strTestSuffix, true);
            }
            else
            {
                strTestSuffix = LanguageManager.GetString("String_AvailForbidden");
                if(strAvail.EndsWith(strTestSuffix, StringComparison.Ordinal))
                {
                    blnShowTest = true;
                    strAvail = strAvail.TrimEndOnce(strTestSuffix, true);
                }
            }

            if(int.TryParse(strAvail, out int intAvail) && (intAvail != 0 || blnShowTest))
            {
                return GetAvailTestString(decCost, intAvail);
            }

            return LanguageManager.GetString("String_None");
        }

        /// <summary>
        /// Extended Availability Test information for an item based on the character's Negotiate Skill.
        /// </summary>
        /// <param name="decCost">Item's cost.</param>
        /// <param name="objAvailability">Item's Availability.</param>
        public string AvailTest(decimal decCost, AvailabilityValue objAvailability)
        {
            if(objAvailability.Value != 0 || objAvailability.Suffix == 'R' || objAvailability.Suffix == 'F')
            {
                return GetAvailTestString(decCost, objAvailability.Value);
            }

            return LanguageManager.GetString("String_None");
        }

        private string GetAvailTestString(decimal decCost, int intAvailValue)
        {
            string strSpace = LanguageManager.GetString("String_Space");
            string strInterval;
            // Find the character's Negotiation total.
            int intPool = SkillsSection.GetActiveSkill("Negotiation")?.Pool ?? 0;
            // Determine the interval based on the item's price.
            if(decCost <= 100.0m)
                strInterval = "6" + strSpace + LanguageManager.GetString("String_Hours");
            else if(decCost <= 1000.0m)
                strInterval = "1" + strSpace + LanguageManager.GetString("String_Day");
            else if(decCost <= 10000.0m)
                strInterval = "2" + strSpace + LanguageManager.GetString("String_Days");
            else if(decCost <= 100000.0m)
                strInterval = "1" + strSpace + LanguageManager.GetString("String_Week");
            else
                strInterval = "1" + strSpace + LanguageManager.GetString("String_Month");

            return string.Concat(intPool.ToString(GlobalOptions.CultureInfo), strSpace, "(",
                   intAvailValue.ToString(GlobalOptions.CultureInfo), ",", strSpace, strInterval, ")");
        }

        /// <summary>
        /// Whether or not Adapsin is enabled.
        /// </summary>
        public bool AdapsinEnabled
        {
            get
            {
                return Improvements.Any(objImprovement =>
                    objImprovement.ImproveType == Improvement.ImprovementType.Adapsin && objImprovement.Enabled);
            }
        }

        /// <summary>
        /// Whether or not Burnout's Way is enabled.
        /// </summary>
        public bool BurnoutEnabled
        {
            get { return Improvements.Any(x => x.ImproveType == Improvement.ImprovementType.BurnoutsWay && x.Enabled); }
        }

        #endregion

        #region Application Properties

        private readonly HashSet<Character> _lstLinkedCharacters = new HashSet<Character>();

        /// <summary>
        /// Characters referenced by some member of this character (usually a contact).
        /// </summary>
        public HashSet<Character> LinkedCharacters => _lstLinkedCharacters;

        #endregion

        #region Old Quality Conversion Code

        /// <summary>
        /// Convert Qualities that are still saved in the old format.
        /// </summary>
        private void ConvertOldQualities(XmlNodeList objXmlQualityList)
        {
            XmlNode xmlRootQualitiesNode = LoadData("qualities.xml").SelectSingleNode("/chummer/qualities");

            if(xmlRootQualitiesNode != null)
            {
                // Convert the old Qualities.
                foreach(XmlNode objXmlQuality in objXmlQualityList)
                {
                    if(objXmlQuality["name"] == null)
                    {
                        string strForceValue = string.Empty;

                        XmlNode objXmlQualityNode =
                            xmlRootQualitiesNode.SelectSingleNode(
                                "quality[name = \"" + GetQualityName(objXmlQuality.InnerText) + "\"]");

                        if(objXmlQualityNode != null)
                        {
                            // Re-create the bonuses for the Quality.
                            if(objXmlQualityNode.InnerXml.Contains("<bonus>"))
                            {
                                // Look for the existing Improvement.
                                foreach(Improvement objImprovement in _lstImprovements)
                                {
                                    if(objImprovement.ImproveSource == Improvement.ImprovementSource.Quality &&
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
                XmlNode objXmlMetatype = LoadData("metatypes.xml").SelectSingleNode(strXPath) ??
                                         LoadData("critters.xml").SelectSingleNode(strXPath);

                if(objXmlMetatype != null)
                {
                    // Positive Qualities.
                    using (XmlNodeList xmlMetatypeQualityList =
                        objXmlMetatype.SelectNodes("qualities/positive/quality"))
                    {
                        if (xmlMetatypeQualityList != null)
                        {
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
                                    string strForceValue =
                                        objXmlMetatypeQuality.Attributes?["select"]?.InnerText ?? string.Empty;
                                    Quality objQuality = new Quality(this);

                                    XmlNode objXmlQuality =
                                        xmlRootQualitiesNode.SelectSingleNode(
                                            "quality[name = \"" + objXmlMetatypeQuality.InnerText + "\"]");
                                    objQuality.Create(objXmlQuality, QualitySource.Metatype, _lstWeapons,
                                        strForceValue);
                                    _lstQualities.Add(objQuality);
                                }
                            }
                        }
                    }

                    // Negative Qualities.
                    using (XmlNodeList xmlMetatypeQualityList =
                        objXmlMetatype.SelectNodes("qualities/negative/quality"))
                    {
                        if (xmlMetatypeQualityList != null)
                        {
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
                                    string strForceValue =
                                        objXmlMetatypeQuality.Attributes?["select"]?.InnerText ?? string.Empty;
                                    Quality objQuality = new Quality(this);

                                    XmlNode objXmlQuality =
                                        xmlRootQualitiesNode.SelectSingleNode(
                                            "quality[name = \"" + objXmlMetatypeQuality.InnerText + "\"]");
                                    objQuality.Create(objXmlQuality, QualitySource.Metatype, _lstWeapons,
                                        strForceValue);
                                    _lstQualities.Add(objQuality);
                                }
                            }
                        }
                    }

                    // Do it all over again for Metavariants.
                    if(!string.IsNullOrEmpty(_strMetavariant))
                    {
                        objXmlMetatype =
                            objXmlMetatype.SelectSingleNode("metavariants/metavariant[name = \"" + _strMetavariant +
                                                            "\"]");

                        if (objXmlMetatype != null)
                        {
                            // Positive Qualities.
                            using (XmlNodeList xmlMetatypeQualityList =
                                objXmlMetatype.SelectNodes("qualities/positive/quality"))
                            {
                                if (xmlMetatypeQualityList != null)
                                {
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
                                            string strForceValue =
                                                objXmlMetatypeQuality.Attributes?["select"]?.InnerText ?? string.Empty;
                                            Quality objQuality = new Quality(this);

                                            XmlNode objXmlQuality =
                                                xmlRootQualitiesNode.SelectSingleNode(
                                                    "quality[name = \"" + objXmlMetatypeQuality.InnerText + "\"]");
                                            objQuality.Create(objXmlQuality, QualitySource.Metatype, _lstWeapons,
                                                strForceValue);
                                            _lstQualities.Add(objQuality);
                                        }
                                    }
                                }
                            }

                            // Negative Qualities.
                            using (XmlNodeList xmlMetatypeQualityList =
                                objXmlMetatype.SelectNodes("qualities/negative/quality"))
                            {
                                if (xmlMetatypeQualityList != null)
                                {
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
                                            string strForceValue =
                                                objXmlMetatypeQuality.Attributes?["select"]?.InnerText ?? string.Empty;
                                            Quality objQuality = new Quality(this);

                                            XmlNode objXmlQuality =
                                                xmlRootQualitiesNode.SelectSingleNode(
                                                    "quality[name = \"" + objXmlMetatypeQuality.InnerText + "\"]");
                                            objQuality.Create(objXmlQuality, QualitySource.Metatype, _lstWeapons,
                                                strForceValue);
                                            _lstQualities.Add(objQuality);
                                        }
                                    }
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
            if(intPos != -1)
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
            switch(xmlOldQuality["name"]?.InnerText)
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
                        xmlNewQuality =
                            xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Tough as Nails (Physical)\"]");
                        intRanks = 1;
                        break;
                    }
                case "Tough as Nails Physical II":
                    {
                        xmlNewQuality =
                            xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Tough as Nails (Physical)\"]");
                        intRanks = 2;
                        break;
                    }
                case "Tough as Nails Physical III":
                    {
                        xmlNewQuality =
                            xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Tough as Nails (Physical)\"]");
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
                        xmlNewQuality =
                            xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Social Appearance Anxiety\"]");
                        intRanks = 1;
                        break;
                    }
                case "Social Appearance Anxiety (Rating 2)":
                    {
                        xmlNewQuality =
                            xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Social Appearance Anxiety\"]");
                        intRanks = 2;
                        break;
                    }
                case "Social Appearance Anxiety (Rating 3)":
                    {
                        xmlNewQuality =
                            xmlRootQualitiesNode.SelectSingleNode("quality[name = \"Social Appearance Anxiety\"]");
                        intRanks = 3;
                        break;
                    }
            }

            if(intRanks > 0)
            {
                for(int i = 0; i < intRanks; ++i)
                {
                    Quality objQuality = new Quality(this);
                    if(i == 0 && xmlOldQuality.TryGetField("guid", Guid.TryParse, out Guid guidOld))
                    {
                        ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.Quality,
                            guidOld.ToString());
                        objQuality.SetGUID(guidOld);
                    }

                    QualitySource objQualitySource =
                        Quality.ConvertToQualitySource(xmlOldQuality["qualitysource"]?.InnerText);
                    objQuality.Create(xmlNewQuality, objQualitySource, _lstWeapons, xmlOldQuality["extra"]?.InnerText);
                    if(xmlOldQuality["bp"] != null && int.TryParse(xmlOldQuality["bp"].InnerText, out int intOldBP))
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
                if(_intInitPasses == int.MinValue)
                    _intInitPasses = Convert.ToInt32(InitiativeDice);
                return _intInitPasses;
            }
            set
            {
                if(_intInitPasses != value)
                {
                    _intInitPasses = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _intInitPasses = int.MinValue;

        /// <summary>
        /// True iff the character is currently delaying an action
        /// <note>Dashboard</note>
        /// </summary>
        public bool Delayed { get; set; }

        /// <summary>
        /// The current name and initiative of the character
        /// </summary>
        public string DisplayInit => Name + " : " + InitRoll.ToString(GlobalOptions.CultureInfo);

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
        public string TranslatedBookList(string strInput, string strLanguage = "")
        {
            if (string.IsNullOrEmpty(strInput))
                return string.Empty;
            StringBuilder sbdReturn = new StringBuilder();
            // Load the Sourcebook information.
            XmlDocument objXmlDocument = LoadData("books.xml", strLanguage);

            foreach(string strBook in strInput.TrimEndOnce(';').SplitNoAlloc(';', StringSplitOptions.RemoveEmptyEntries))
            {
                XmlNode objXmlBook = objXmlDocument.SelectSingleNode("/chummer/books/book[code = \"" + strBook + "\"]");
                if(objXmlBook != null)
                {
                    sbdReturn.Append(objXmlBook["translate"]?.InnerText ?? objXmlBook["name"]?.InnerText ?? LanguageManager.GetString("String_Unknown", strLanguage))
                        .Append(LanguageManager.GetString("String_Space", strLanguage))
                        .Append('(')
                        .Append(objXmlBook["altcode"]?.InnerText ?? strBook)
                        .AppendLine(")");
                }
                else
                {
                    sbdReturn.Append(LanguageManager.GetString("String_Unknown", strLanguage))
                        .Append(LanguageManager.GetString("String_Space", strLanguage))
                        .AppendLine(strBook);
                }
            }

            return sbdReturn.ToString();
        }

        #endregion

        //Can't be at improvementmanager due reasons
        private readonly Lazy<Stack<string>> _pushtext = new Lazy<Stack<string>>();

        /// <summary>
        /// Push a value that will be used instead of dialog instead in next <selecttext />
        /// </summary>
        public Stack<string> Pushtext => _pushtext.Value;

        private IHasMatrixAttributes _objActiveCommlink;

        /// <summary>
        /// The Active Commlink of the character. Returns null if character has no active commlink.
        /// </summary>
        public IHasMatrixAttributes ActiveCommlink
        {
            get => _objActiveCommlink;
            set
            {
                if(_objActiveCommlink != value)
                {
                    _objActiveCommlink = value;
                    OnPropertyChanged();
                }
            }
        }

        private IHasMatrixAttributes _objHomeNode;

        /// <summary>
        /// Home Node. Returns null if home node is not set to any item.
        /// </summary>
        public IHasMatrixAttributes HomeNode
        {
            get => _objHomeNode;
            set
            {
                if(_objHomeNode != value)
                {
                    _objHomeNode = value;
                    OnPropertyChanged();
                }
            }
        }

        [HubTag]
        public SkillsSection SkillsSection { get; }


        public int RedlinerBonus
        {
            get
            {
                if(_intCachedRedlinerBonus == int.MinValue)
                    RefreshRedlinerImprovements();

                return _intCachedRedlinerBonus;
            }
        }

        private void RefreshRedlinerImprovements()
        {
            if (IsLoading) // If we are in the middle of loading, just queue a single refresh to happen at the end of the process
            {
                PostLoadMethods.Enqueue(RefreshRedlinerImprovements);
                return;
            }
            //Get attributes affected by redliner/cyber singularity seeker
            List<Improvement> lstSeekerImprovements = Improvements.Where(objLoopImprovement =>
                (objLoopImprovement.ImproveType == Improvement.ImprovementType.Attribute ||
                 objLoopImprovement.ImproveType == Improvement.ImprovementType.PhysicalCM) &&
                objLoopImprovement.SourceName.Contains("SEEKER")).ToList();
            List<string> lstSeekerAttributes = new List<string>(Improvements
                .Where(imp => imp.ImproveType == Improvement.ImprovementType.Seeker)
                .Select(objImprovement => objImprovement.ImprovedName));

            //if neither contains anything, it is safe to exit
            if(lstSeekerImprovements.Count == 0 && lstSeekerAttributes.Count == 0)
            {
                _intCachedRedlinerBonus = 0;
                return;
            }

            //Calculate bonus from cyberlimbs
            int intCount = Cyberware.Sum(objCyberware => objCyberware.GetCyberlimbCount(Options.RedlinerExcludes));

            intCount = Math.Min(intCount / 2, 2);
            _intCachedRedlinerBonus = lstSeekerAttributes.Any(x => x == "STR" || x == "AGI")
                ? intCount
                : 0;

            for(int i = lstSeekerAttributes.Count - 1; i >= 0; --i)
            {
                string strSeekerAttribute = "SEEKER_" + lstSeekerAttributes[i];
                Improvement objImprove = lstSeekerImprovements.FirstOrDefault(x =>
                    x.SourceName == strSeekerAttribute
                    && x.Value == (strSeekerAttribute == "SEEKER_BOX" ? intCount * -3 : intCount));
                if(objImprove != null)
                {
                    lstSeekerAttributes.RemoveAt(i);
                    lstSeekerImprovements.Remove(objImprove);
                }
            }

            //Improvement manager defines the functions needed to manipulate improvements
            //When the locals (someday) gets moved to this class, this can be removed and use
            //the local
            if(lstSeekerImprovements.Count != 0 || lstSeekerAttributes.Count != 0)
            {
                // Remove which qualities have been removed or which values have changed
                ImprovementManager.RemoveImprovements(this, lstSeekerImprovements);

                // Add new improvements or old improvements with new values
                foreach(string strAttribute in lstSeekerAttributes)
                {
                    if(strAttribute == "BOX")
                    {
                        ImprovementManager.CreateImprovement(this, strAttribute, Improvement.ImprovementSource.Quality,
                            "SEEKER_BOX", Improvement.ImprovementType.PhysicalCM, Guid.NewGuid().ToString("D", GlobalOptions.InvariantCultureInfo),
                            intCount * -3);
                    }
                    else
                    {
                        ImprovementManager.CreateImprovement(this, strAttribute, Improvement.ImprovementSource.Quality,
                            "SEEKER_" + strAttribute, Improvement.ImprovementType.Attribute,
                            Guid.NewGuid().ToString("D", GlobalOptions.InvariantCultureInfo), intCount, 1, 0, 0, intCount);
                    }
                }

                ImprovementManager.Commit(this);
            }
        }

        public void RefreshEssenceLossImprovements()
        {
            // Don't hammer away with this method while this character is loading. Instead, it will be run once after everything has been loaded in.
            if(IsLoading)
                return;
            // Only worry about essence loss attribute modifiers if this character actually has any attributes that would be affected by essence loss
            // (which means EssenceAtSpecialStart is not set to decimal.MinValue)
            if(EssenceAtSpecialStart != decimal.MinValue)
            {
                decimal decESS = Essence();
                decimal decESSMag = Essence(true);
                if(!Options.DontRoundEssenceInternally)
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
                if(Options.SpecialKarmaCostBasedOnShownValue)
                {
                    Improvement.ImprovementSource eEssenceLossSource = Created
                        ? Improvement.ImprovementSource.EssenceLoss
                        : Improvement.ImprovementSource.EssenceLossChargen;
                    ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.EssenceLoss);
                    ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.EssenceLossChargen);
                    // With this house rule, Cyberadept Daemon just negates a penalty from Essence based on Grade instead of restoring Resonance, so delete all old improvements
                    ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.CyberadeptDaemon);
                    if (intMaxReduction != 0)
                    {
                        int intCyberadeptDaemonBonus = 0;
                        if (TechnomancerEnabled && SubmersionGrade > 0 && Improvements.Any(x => x.ImproveType == Improvement.ImprovementType.CyberadeptDaemon && x.Enabled))
                            intCyberadeptDaemonBonus = (int)Math.Min(Math.Ceiling(0.5m * SubmersionGrade), Math.Ceiling(CyberwareEssence));
                        ImprovementManager.CreateImprovement(this, "RES", eEssenceLossSource, string.Empty,
                            Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, 0, Math.Min(0, intCyberadeptDaemonBonus - intMaxReduction));
                        ImprovementManager.CreateImprovement(this, "DEP", eEssenceLossSource, string.Empty,
                            Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, 0, -intMaxReduction);
                    }

                    if(intMagMaxReduction != 0)
                    {
                        ImprovementManager.CreateImprovement(this, "MAG", eEssenceLossSource, string.Empty,
                            Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, 0, -intMagMaxReduction);
                        ImprovementManager.CreateImprovement(this, "MAGAdept", eEssenceLossSource, string.Empty,
                            Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, 0, -intMagMaxReduction);
                        // If this is a Mystic Adept using special Mystic Adept PP rules (i.e. no second MAG attribute), Mystic Adepts lose PPs even if they have fewer PPs than their MAG
                        if(UseMysticAdeptPPs)
                            ImprovementManager.CreateImprovement(this, string.Empty, eEssenceLossSource, string.Empty,
                                Improvement.ImprovementType.AdeptPowerPoints, string.Empty, -intMagMaxReduction);
                    }
                }
                // RAW Career mode: complicated. Similar to RAW Create mode, but with the extra possibility of burning current karma levels and/or PPs instead of pure minima reduction,
                // plus the need to account for cases where a character will burn "past" 0 (i.e. to a current value that should be negative), but then upgrade to 1 afterwards.
                else if(Created)
                {
                    // "Base" minimum reduction. This is the amount by which the character's special attribute minima would be reduced across career and create modes if there wasn't any funny business
                    int intMinReduction = decimal.ToInt32(decimal.Ceiling(EssenceAtSpecialStart - decESS));
                    int intMagMinReduction = decimal.ToInt32(decimal.Ceiling(EssenceAtSpecialStart - decESSMag));

                    // This extra code is needed for legacy shims, to convert proper attribute values for characters who would end up having a higher level than their total attribute maxima
                    // They are extra amounts by which the relevant attributes' karma levels should be burned
                    int intExtraRESBurn = Math.Max(0,
                        Math.Max(RES.Base + RES.FreeBase + RES.RawMinimum + RES.AttributeValueModifiers,
                            RES.TotalMinimum) + RES.Karma - RES.TotalMaximum);
                    int intExtraDEPBurn = Math.Max(0,
                        Math.Max(DEP.Base + DEP.FreeBase + DEP.RawMinimum + DEP.AttributeValueModifiers,
                            DEP.TotalMinimum) + DEP.Karma - DEP.TotalMaximum);
                    int intExtraMAGBurn = Math.Max(0,
                        Math.Max(MAG.Base + MAG.FreeBase + MAG.RawMinimum + MAG.AttributeValueModifiers,
                            MAG.TotalMinimum) + MAG.Karma - MAG.TotalMaximum);
                    int intExtraMAGAdeptBurn = Math.Max(0,
                        Math.Max(
                            MAGAdept.Base + MAGAdept.FreeBase + MAGAdept.RawMinimum + MAGAdept.AttributeValueModifiers,
                            MAGAdept.TotalMinimum) + MAGAdept.Karma - MAGAdept.TotalMaximum);
                    // Old values for minimum reduction from essence loss in career mode. These are used to determine if any karma needs to get burned.
                    int intOldRESCareerMinimumReduction = 0;
                    int intOldDEPCareerMinimumReduction = 0;
                    int intOldMAGCareerMinimumReduction = 0;
                    int intOldMAGAdeptCareerMinimumReduction = 0;
                    foreach(Improvement objImprovement in Improvements)
                    {
                        if(objImprovement.ImproveSource == Improvement.ImprovementSource.EssenceLoss
                           && objImprovement.ImproveType == Improvement.ImprovementType.Attribute
                           && objImprovement.Enabled)
                        {
                            // Values get subtracted because negative modifier = positive reduction, positive modifier = negative reduction
                            // Augmented values also get factored in in case the character is switching off the option to treat essence loss as an augmented malus
                            switch(objImprovement.ImprovedName)
                            {
                                case "RES":
                                    intOldRESCareerMinimumReduction -=
                                        objImprovement.Minimum + objImprovement.Augmented;
                                    break;
                                case "DEP":
                                    intOldDEPCareerMinimumReduction -=
                                        objImprovement.Minimum + objImprovement.Augmented;
                                    break;
                                case "MAG":
                                    intOldMAGCareerMinimumReduction -=
                                        objImprovement.Minimum + objImprovement.Augmented;
                                    break;
                                case "MAGAdept":
                                    intOldMAGAdeptCareerMinimumReduction -=
                                        objImprovement.Minimum + objImprovement.Augmented;
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
                    int intMAGAdeptMaximumReduction =
                        intMagMaxReduction + MAGAdept.TotalMaximum - MAGAdept.MaximumNoEssenceLoss();

                    // Create the Essence Loss (or gain, in case of essence restoration and increasing maxima) Improvements.
                    if(intMaxReduction > 0
                       || intMinReduction > 0
                       || intRESMaximumReduction != 0
                       || intDEPMaximumReduction != 0)
                    {
                        // This is the step where create mode attribute loss regarding attribute minimum loss gets factored out.
                        int intRESMinimumReduction;
                        int intDEPMinimumReduction;
                        // If only maxima would be reduced, use the attribute's current total value instead of its current maximum, as this makes sure minima will only get reduced if the maximum reduction would eat into the current value
                        if(Options.ESSLossReducesMaximumOnly)
                        {
                            intRESMinimumReduction = Math.Max(0,
                                intMinReduction + RES.TotalValue - RES.MaximumNoEssenceLoss(true));
                            intDEPMinimumReduction = Math.Max(0,
                                intMinReduction + DEP.TotalValue - DEP.MaximumNoEssenceLoss(true));
                        }
                        else
                        {
                            intRESMinimumReduction =
                                intMinReduction + RES.TotalMaximum - RES.MaximumNoEssenceLoss(true);
                            intDEPMinimumReduction =
                                intMinReduction + DEP.TotalMaximum - DEP.MaximumNoEssenceLoss(true);
                        }

                        // If the new RES reduction is greater than the old one...
                        int intRESMinimumReductionDelta = intRESMinimumReduction - intOldRESCareerMinimumReduction;
                        if(intRESMinimumReductionDelta > 0)
                        {
                            // ... and adding minimum reducing-improvements wouldn't do anything, start burning karma.
                            if(intRESMinimumReduction >
                                RES.Base + RES.FreeBase + RES.RawMinimum + RES.AttributeValueModifiers)
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
                        if(intDEPMinimumReductionDelta > 0)
                        {
                            // ... and adding minimum reducing-improvements wouldn't do anything, start burning karma.
                            if(intDEPMinimumReduction >
                                DEP.Base + DEP.FreeBase + DEP.RawMinimum + DEP.AttributeValueModifiers)
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
                        if(intRESMinimumReduction != 0 || intRESMaximumReduction != 0)
                            ImprovementManager.CreateImprovement(this, "RES", Improvement.ImprovementSource.EssenceLoss,
                                string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1,
                                -intRESMinimumReduction, -intRESMaximumReduction);
                        if(intDEPMinimumReduction != 0 || intDEPMaximumReduction != 0)
                            ImprovementManager.CreateImprovement(this, "DEP", Improvement.ImprovementSource.EssenceLoss,
                                string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1,
                                -intDEPMinimumReduction, -intDEPMaximumReduction);
                    }

                    if(intMagMaxReduction > 0
                       || intMagMinReduction > 0
                       || intMAGMaximumReduction != 0
                       || intMAGAdeptMaximumReduction != 0)
                    {
                        // This is the step where create mode attribute loss regarding attribute minimum loss gets factored out.
                        int intMAGMinimumReduction;
                        int intMAGAdeptMinimumReduction;
                        // If only maxima would be reduced, use the attribute's current total value instead of its current maximum, as this makes sure minima will only get reduced if the maximum reduction would eat into the current value
                        if(Options.ESSLossReducesMaximumOnly)
                        {
                            intMAGMinimumReduction = Math.Max(0,
                                intMagMinReduction + MAG.TotalValue - MAG.MaximumNoEssenceLoss(true));
                            intMAGAdeptMinimumReduction = Math.Max(0,
                                intMagMinReduction + MAGAdept.TotalValue - MAGAdept.MaximumNoEssenceLoss(true));
                        }
                        else
                        {
                            intMAGMinimumReduction =
                                intMagMinReduction + MAG.TotalMaximum - MAG.MaximumNoEssenceLoss(true);
                            intMAGAdeptMinimumReduction =
                                intMagMinReduction + MAGAdept.TotalMaximum - MAGAdept.MaximumNoEssenceLoss(true);
                        }

                        // If the new MAG reduction is greater than the old one...
                        int intMAGMinimumReductionDelta = intMAGMinimumReduction - intOldMAGCareerMinimumReduction;
                        if(intMAGMinimumReductionDelta > 0)
                        {
                            // ... and adding minimum reducing-improvements wouldn't do anything, start burning karma.
                            if(intMAGMinimumReduction >
                                MAG.Base + MAG.FreeBase + MAG.RawMinimum + MAG.AttributeValueModifiers)
                            {
                                // intMAGMinimumReduction is not actually reduced so that karma doesn't get burned away each time this function is called.
                                // Besides, this only fires if intMAGMinimumReduction is already at a level where increasing it any more wouldn't have any effect on the character.
                                intExtraMAGBurn += Math.Min(MAG.Karma, intMAGMinimumReductionDelta);
                                MAG.Karma -= intExtraMAGBurn;
                            }

                            // Mystic Adept PPs may need to be burned away based on the change of our MAG attribute
                            if(UseMysticAdeptPPs)
                            {
                                // First burn away PPs gained during chargen...
                                int intPPBurn = Math.Min(MysticAdeptPowerPoints, intMAGMinimumReductionDelta);
                                MysticAdeptPowerPoints -= intPPBurn;
                                // ... now burn away PPs gained from initiations.
                                intPPBurn = Math.Min(intMAGMinimumReductionDelta - intPPBurn,
                                    ImprovementManager.ValueOf(this, Improvement.ImprovementType.AdeptPowerPoints));
                                // Source needs to be EssenceLossChargen so that it doesn't get wiped in career mode.
                                if(intPPBurn != 0)
                                    ImprovementManager.CreateImprovement(this, string.Empty,
                                        Improvement.ImprovementSource.EssenceLossChargen, string.Empty,
                                        Improvement.ImprovementType.AdeptPowerPoints, string.Empty, -intPPBurn);
                            }
                        }
                        // If the new MAG reduction is less than our old one, the character doesn't actually get any new values back
                        else
                        {
                            intMAGMinimumReduction = intOldMAGCareerMinimumReduction;
                        }

                        // Make sure we only attempt to burn MAGAdept karma levels if it's actually a separate attribute from MAG
                        if(MAGAdept != MAG)
                        {
                            // If the new MAGAdept reduction is greater than the old one...
                            int intMAGAdeptMinimumReductionDelta =
                                intMAGAdeptMinimumReduction - intOldMAGAdeptCareerMinimumReduction;
                            if(intMAGAdeptMinimumReductionDelta > 0)
                            {
                                // ... and adding minimum reducing-improvements wouldn't do anything, start burning karma.
                                if(intMAGAdeptMinimumReduction > MAGAdept.Base + MAGAdept.FreeBase +
                                    MAGAdept.RawMinimum + MAGAdept.AttributeValueModifiers)
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
                        else if(intMAGAdeptMinimumReduction < intOldMAGAdeptCareerMinimumReduction)
                        {
                            intMAGAdeptMinimumReduction = intOldMAGAdeptCareerMinimumReduction;
                        }

                        // Create Improvements
                        if(intMAGMinimumReduction != 0 || intMAGMaximumReduction != 0)
                            ImprovementManager.CreateImprovement(this, "MAG", Improvement.ImprovementSource.EssenceLoss,
                                string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1,
                                -intMAGMinimumReduction, -intMAGMaximumReduction);
                        if(intMAGAdeptMinimumReduction != 0 || intMAGAdeptMaximumReduction != 0)
                            ImprovementManager.CreateImprovement(this, "MAGAdept",
                                Improvement.ImprovementSource.EssenceLoss, string.Empty,
                                Improvement.ImprovementType.Attribute, string.Empty, 0, 1, -intMAGAdeptMinimumReduction,
                                -intMAGAdeptMaximumReduction);
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
                    if(Options.ESSLossReducesMaximumOnly)
                    {
                        intRESMinimumReduction = Math.Max(0, intMinReduction + RES.TotalValue - RES.TotalMaximum);
                        intDEPMinimumReduction = Math.Max(0, intMinReduction + DEP.TotalValue - DEP.TotalMaximum);
                        intMAGMinimumReduction = Math.Max(0, intMagMinReduction + MAG.TotalValue - MAG.TotalMaximum);
                        intMAGAdeptMinimumReduction = Math.Max(0,
                            intMagMinReduction + MAGAdept.TotalValue - MAGAdept.TotalMaximum);
                    }

                    ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.EssenceLoss);
                    ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.EssenceLossChargen);
                    if(intMaxReduction != 0 || intRESMinimumReduction != 0 || intDEPMinimumReduction != 0)
                    {
                        ImprovementManager.CreateImprovement(this, "RES",
                            Improvement.ImprovementSource.EssenceLossChargen, string.Empty,
                            Improvement.ImprovementType.Attribute, string.Empty, 0, 1, -intRESMinimumReduction,
                            -intMaxReduction);
                        ImprovementManager.CreateImprovement(this, "DEP",
                            Improvement.ImprovementSource.EssenceLossChargen, string.Empty,
                            Improvement.ImprovementType.Attribute, string.Empty, 0, 1, -intDEPMinimumReduction,
                            -intMaxReduction);
                    }

                    if(intMagMaxReduction != 0 || intMAGMinimumReduction != 0 || intMAGAdeptMinimumReduction != 0)
                    {
                        ImprovementManager.CreateImprovement(this, "MAG",
                            Improvement.ImprovementSource.EssenceLossChargen, string.Empty,
                            Improvement.ImprovementType.Attribute, string.Empty, 0, 1, -intMAGMinimumReduction,
                            -intMagMaxReduction);
                        ImprovementManager.CreateImprovement(this, "MAGAdept",
                            Improvement.ImprovementSource.EssenceLossChargen, string.Empty,
                            Improvement.ImprovementType.Attribute, string.Empty, 0, 1, -intMAGAdeptMinimumReduction,
                            -intMagMaxReduction);
                    }
                }

                ImprovementManager.Commit(this);

                // If the character is in Career mode, it is possible for them to be forced to burn out.
                if(Created)
                {
                    // If the CharacterAttribute reaches 0, the character has burned out.
                    if(MAGEnabled)
                    {
                        if(Options.SpecialKarmaCostBasedOnShownValue)
                        {
                            if(Options.MysAdeptSecondMAGAttribute && IsMysticAdept)
                            {
                                if(intMagMaxReduction >= MAG.TotalMaximum)
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

                                if(intMagMaxReduction >= MAGAdept.TotalMaximum)
                                {
                                    MAGAdept.Base = 0;
                                    MAGAdept.Karma = 0;
                                    MAGAdept.MetatypeMinimum = 0;
                                    MAGAdept.MetatypeMaximum = 0;
                                    MAGAdept.MetatypeAugmentedMaximum = 0;

                                    AdeptEnabled = false;
                                }

                                if(!MagicianEnabled && !AdeptEnabled)
                                    MAGEnabled = false;
                            }
                            else if(intMagMaxReduction >= MAG.TotalMaximum)
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
                        else if (Options.MysAdeptSecondMAGAttribute && IsMysticAdept)
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
                    }

                    if(RESEnabled
                       && (Options.SpecialKarmaCostBasedOnShownValue
                           && intMaxReduction >= RES.TotalMaximum
                           || !Options.SpecialKarmaCostBasedOnShownValue
                           && RES.TotalMaximum < 1))
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
            if(MetatypeCategory == "Cyberzombie")
            {
                int intESSModifier = decimal.ToInt32(decimal.Ceiling(Essence() * -1));
                ImprovementManager.RemoveImprovements(this,
                    Improvements.Where(x =>
                        x.ImproveSource == Improvement.ImprovementSource.Cyberzombie &&
                        x.ImproveType == Improvement.ImprovementType.Attribute).ToList());
                if(intESSModifier != 0)
                {
                    ImprovementManager.CreateImprovement(this, "BOD", Improvement.ImprovementSource.Cyberzombie,
                        string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, intESSModifier);
                    ImprovementManager.CreateImprovement(this, "AGI", Improvement.ImprovementSource.Cyberzombie,
                        string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, intESSModifier);
                    ImprovementManager.CreateImprovement(this, "REA", Improvement.ImprovementSource.Cyberzombie,
                        string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, intESSModifier);
                    ImprovementManager.CreateImprovement(this, "STR", Improvement.ImprovementSource.Cyberzombie,
                        string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, intESSModifier);
                    ImprovementManager.CreateImprovement(this, "CHA", Improvement.ImprovementSource.Cyberzombie,
                        string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, intESSModifier);
                    ImprovementManager.CreateImprovement(this, "INT", Improvement.ImprovementSource.Cyberzombie,
                        string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, intESSModifier);
                    ImprovementManager.CreateImprovement(this, "LOG", Improvement.ImprovementSource.Cyberzombie,
                        string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, intESSModifier);
                    ImprovementManager.CreateImprovement(this, "WIL", Improvement.ImprovementSource.Cyberzombie,
                        string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, intESSModifier);
                    ImprovementManager.Commit(this);
                }
            }
        }

        public void RefreshBODDependentProperties(object sender, PropertyChangedEventArgs e)
        {
            if(e?.PropertyName == nameof(CharacterAttrib.TotalValue))
            {
                List<string> lstProperties = new List<string>(16)
                {
                    nameof(LimitPhysical),
                    nameof(DamageResistancePool),
                    nameof(LiftAndCarry),
                    nameof(FatigueResist),
                    nameof(RadiationResist),
                    nameof(PhysiologicalAddictionResistFirstTime),
                    nameof(PhysiologicalAddictionResistAlreadyAddicted),
                    nameof(StunCMNaturalRecovery),
                    nameof(PhysicalCMNaturalRecovery),
                    nameof(PhysicalCM),
                    nameof(CMOverflow),
                    nameof(SpellDefenseIndirectSoak),
                    nameof(SpellDefenseDirectSoakPhysical),
                    nameof(SpellDefenseDecreaseBOD),
                    nameof(SpellDefenseManipulationPhysical)
                };
                if (Options.ContactPointsExpression.Contains("{BOD}"))
                    lstProperties.Add(nameof(ContactPoints));
                OnMultiplePropertyChanged(lstProperties.ToArray());
            }
            else if (e?.PropertyName == nameof(CharacterAttrib.Value))
            {
                if (Options.ContactPointsExpression.Contains("{BODUnaug}"))
                    OnPropertyChanged(nameof(ContactPoints));
            }
            else if(e?.PropertyName == nameof(CharacterAttrib.MetatypeMaximum))
            {
                if(DEPEnabled)
                    OnPropertyChanged(nameof(IsAI));
            }
        }

        public void RefreshAGIDependentProperties(object sender, PropertyChangedEventArgs e)
        {
            if(e?.PropertyName == nameof(CharacterAttrib.TotalValue))
            {
                List<string> lstProperties = new List<string>(2)
                {
                    nameof(SpellDefenseDecreaseAGI)
                };
                if (Options.ContactPointsExpression.Contains("{AGI}"))
                    lstProperties.Add(nameof(ContactPoints));
                OnMultiplePropertyChanged(lstProperties.ToArray());
            }
            else if (e?.PropertyName == nameof(CharacterAttrib.Value))
            {
                if (Options.ContactPointsExpression.Contains("{AGIUnaug}"))
                    OnPropertyChanged(nameof(ContactPoints));
            }
        }

        public void RefreshREADependentProperties(object sender, PropertyChangedEventArgs e)
        {
            if(e?.PropertyName == nameof(CharacterAttrib.TotalValue))
            {
                List<string> lstProperties = new List<string>(6)
                {
                    nameof(LimitPhysical),
                    nameof(InitiativeValue),
                    nameof(Dodge),
                    nameof(SpellDefenseDecreaseREA),
                    nameof(Surprise)
                };
                if (Options.ContactPointsExpression.Contains("{REA}"))
                    lstProperties.Add(nameof(ContactPoints));
                OnMultiplePropertyChanged(lstProperties.ToArray());
            }
            else if (e?.PropertyName == nameof(CharacterAttrib.Value))
            {
                if (Options.ContactPointsExpression.Contains("{REAUnaug}"))
                    OnPropertyChanged(nameof(ContactPoints));
            }
        }

        public void RefreshSTRDependentProperties(object sender, PropertyChangedEventArgs e)
        {
            if(e?.PropertyName == nameof(CharacterAttrib.TotalValue))
            {
                // Encumbrance is only affected by STR.TotalValue when it comes to attributes
                RefreshEncumbrance();
                List<string> lstProperties = new List<string>(5)
                {
                    nameof(LimitPhysical),
                    nameof(LiftAndCarry),
                    nameof(SpellDefenseDecreaseSTR),
                    nameof(SpellDefenseManipulationPhysical)
                };
                if (Options.ContactPointsExpression.Contains("{STR}"))
                    lstProperties.Add(nameof(ContactPoints));
                OnMultiplePropertyChanged(lstProperties.ToArray());
            }
            else if (e?.PropertyName == nameof(CharacterAttrib.Value))
            {
                if (Options.ContactPointsExpression.Contains("{STRUnaug}"))
                    OnPropertyChanged(nameof(ContactPoints));
            }
        }

        public void RefreshCHADependentProperties(object sender, PropertyChangedEventArgs e)
        {
            if(e?.PropertyName == nameof(CharacterAttrib.TotalValue))
            {
                List<string> lstProperties = new List<string>(6)
                {
                    nameof(LimitSocial),
                    nameof(Composure),
                    nameof(JudgeIntentions),
                    nameof(JudgeIntentionsResist),
                    nameof(SpellDefenseDecreaseCHA)
                };
                if (Options.ContactPointsExpression.Contains("{CHA}"))
                    lstProperties.Add(nameof(ContactPoints));
                OnMultiplePropertyChanged(lstProperties.ToArray());
            }
            else if(e?.PropertyName == nameof(CharacterAttrib.Value))
            {
                if (Options.ContactPointsExpression.Contains("{CHAUnaug}"))
                    OnPropertyChanged(nameof(ContactPoints));
            }
        }

        public void RefreshINTDependentProperties(object sender, PropertyChangedEventArgs e)
        {
            if(e?.PropertyName == nameof(CharacterAttrib.TotalValue))
            {
                List<string> lstProperties = new List<string>(12)
                {
                    nameof(LimitMental),
                    nameof(JudgeIntentions),
                    nameof(InitiativeValue),
                    nameof(AstralInitiativeValue),
                    nameof(MatrixInitiativeValue),
                    nameof(MatrixInitiativeColdValue),
                    nameof(MatrixInitiativeHotValue),
                    nameof(Dodge),
                    nameof(SpellDefenseDecreaseINT),
                    nameof(SpellDefenseIllusionPhysical),
                    nameof(Surprise)
                };
                if (Options.ContactPointsExpression.Contains("{INT}"))
                    lstProperties.Add(nameof(ContactPoints));
                OnMultiplePropertyChanged(lstProperties.ToArray());
            }
            else if (e?.PropertyName == nameof(CharacterAttrib.Value))
            {
                if (Options.ContactPointsExpression.Contains("{INTUnaug}"))
                    OnPropertyChanged(nameof(ContactPoints));
            }
        }

        public void RefreshLOGDependentProperties(object sender, PropertyChangedEventArgs e)
        {
            if(e?.PropertyName == nameof(CharacterAttrib.TotalValue))
            {
                List<string> lstProperties = new List<string>(10)
                {
                    nameof(LimitMental),
                    nameof(Memory),
                    nameof(PsychologicalAddictionResistFirstTime),
                    nameof(PsychologicalAddictionResistAlreadyAddicted),
                    nameof(SpellDefenseDetection),
                    nameof(SpellDefenseDecreaseLOG),
                    nameof(SpellDefenseIllusionMana),
                    nameof(SpellDefenseIllusionPhysical),
                    nameof(SpellDefenseManipulationMental)
                };
                if (Options.ContactPointsExpression.Contains("{LOG}"))
                    lstProperties.Add(nameof(ContactPoints));
                OnMultiplePropertyChanged(lstProperties.ToArray());
            }
            else if (e?.PropertyName == nameof(CharacterAttrib.Value))
            {
                if (Options.ContactPointsExpression.Contains("{LOGUnaug}"))
                    OnPropertyChanged(nameof(ContactPoints));
            }
        }

        public void RefreshWILDependentProperties(object sender, PropertyChangedEventArgs e)
        {
            if(e?.PropertyName == nameof(CharacterAttrib.TotalValue))
            {
                List<string> lstProperties = new List<string>(27)
                {
                    nameof(LimitSocial),
                    nameof(LimitMental),
                    nameof(Composure),
                    nameof(Memory),
                    nameof(JudgeIntentionsResist),
                    nameof(FatigueResist),
                    nameof(SonicResist),
                    nameof(RadiationResist),
                    nameof(PhysiologicalAddictionResistFirstTime),
                    nameof(PhysiologicalAddictionResistAlreadyAddicted),
                    nameof(PsychologicalAddictionResistFirstTime),
                    nameof(PsychologicalAddictionResistAlreadyAddicted),
                    nameof(StunCMNaturalRecovery),
                    nameof(StunCM),
                    nameof(SpellDefenseDirectSoakMana),
                    nameof(SpellDefenseDetection),
                    nameof(SpellDefenseDecreaseBOD),
                    nameof(SpellDefenseDecreaseAGI),
                    nameof(SpellDefenseDecreaseREA),
                    nameof(SpellDefenseDecreaseSTR),
                    nameof(SpellDefenseDecreaseCHA),
                    nameof(SpellDefenseDecreaseINT),
                    nameof(SpellDefenseDecreaseLOG),
                    nameof(SpellDefenseDecreaseWIL),
                    nameof(SpellDefenseIllusionMana),
                    nameof(SpellDefenseManipulationMental)
                };
                if (Options.ContactPointsExpression.Contains("{WIL}"))
                    lstProperties.Add(nameof(ContactPoints));
                OnMultiplePropertyChanged(lstProperties.ToArray());
            }
            else if (e?.PropertyName == nameof(CharacterAttrib.Value))
            {
                if (Options.ContactPointsExpression.Contains("{WILUnaug}"))
                    OnPropertyChanged(nameof(ContactPoints));
            }
        }

        public void RefreshEDGDependentProperties(object sender, PropertyChangedEventArgs e)
        {
            if (e?.PropertyName == nameof(CharacterAttrib.TotalValue))
            {
                if (Options.ContactPointsExpression.Contains("{EDG}"))
                    OnPropertyChanged(nameof(ContactPoints));
            }
            else if (e?.PropertyName == nameof(CharacterAttrib.Value))
            {
                if (Options.ContactPointsExpression.Contains("{EDGUnaug}"))
                    OnPropertyChanged(nameof(ContactPoints));
            }
        }

        public void RefreshMAGDependentProperties(object sender, PropertyChangedEventArgs e)
        {
            if(e?.PropertyName == nameof(CharacterAttrib.TotalValue))
            {
                if(!IsLoading && MysticAdeptPowerPoints > 0)
                {
                    int intMAGTotalValue = MAG.TotalValue;
                    if(MysticAdeptPowerPoints > intMAGTotalValue)
                        MysticAdeptPowerPoints = intMAGTotalValue;
                }

                List<string> lstProperties = new List<string>(5);
                if(Options.SpiritForceBasedOnTotalMAG)
                    lstProperties.Add(nameof(MaxSpiritForce));
                if(MysAdeptAllowPPCareer)
                    lstProperties.Add(nameof(CanAffordCareerPP));
                if(!UseMysticAdeptPPs && MAG == MAGAdept)
                    lstProperties.Add(nameof(PowerPointsTotal));
                if (AnyPowerAdeptWayDiscountEnabled)
                    lstProperties.Add(nameof(AllowAdeptWayPowerDiscount));
                if (Options.ContactPointsExpression.Contains("{MAG}"))
                    lstProperties.Add(nameof(ContactPoints));
                OnMultiplePropertyChanged(lstProperties.ToArray());
            }
            else if(e?.PropertyName == nameof(CharacterAttrib.Value))
            {
                List<string> lstProperties = new List<string>(2);
                if (!Options.SpiritForceBasedOnTotalMAG)
                    lstProperties.Add(nameof(MaxSpiritForce));
                if (Options.ContactPointsExpression.Contains("{MAGUnaug}"))
                    lstProperties.Add(nameof(ContactPoints));
                OnMultiplePropertyChanged(lstProperties.ToArray());
            }
        }

        public void RefreshMAGAdeptDependentProperties(object sender, PropertyChangedEventArgs e)
        {
            if(MAG == MAGAdept)
                return;

            if(e?.PropertyName == nameof(CharacterAttrib.TotalValue))
            {
                List<string> lstProperties = new List<string>(2);
                if (!UseMysticAdeptPPs)
                    lstProperties.Add(nameof(MaxSpiritForce));
                if (Options.ContactPointsExpression.Contains("{MAGAdept}"))
                    lstProperties.Add(nameof(ContactPoints));
                OnMultiplePropertyChanged(lstProperties.ToArray());
            }
            else if (e?.PropertyName == nameof(CharacterAttrib.Value))
            {
                if (Options.ContactPointsExpression.Contains("{MAGAdeptUnaug}"))
                    OnPropertyChanged(nameof(ContactPoints));
            }
        }

        public void RefreshRESDependentProperties(object sender, PropertyChangedEventArgs e)
        {
            if(e?.PropertyName == nameof(CharacterAttrib.TotalValue))
            {
                List<string> lstProperties = new List<string>(2)
                {
                    nameof(MaxSpriteLevel)
                };
                if (Options.ContactPointsExpression.Contains("{RES}"))
                    lstProperties.Add(nameof(ContactPoints));
                OnMultiplePropertyChanged(lstProperties.ToArray());
            }
            else if (e?.PropertyName == nameof(CharacterAttrib.Value))
            {
                if (Options.ContactPointsExpression.Contains("{RESUnaug}"))
                    OnPropertyChanged(nameof(ContactPoints));
            }
        }

        public void RefreshDEPDependentProperties(object sender, PropertyChangedEventArgs e)
        {
            if(IsAI && e?.PropertyName == nameof(CharacterAttrib.TotalValue))
                EDG.OnPropertyChanged(nameof(CharacterAttrib.MetatypeMaximum));
            else if (e?.PropertyName == nameof(CharacterAttrib.TotalValue))
            {
                if (Options.ContactPointsExpression.Contains("{DEP}"))
                    OnPropertyChanged(nameof(ContactPoints));
            }
            else if (e?.PropertyName == nameof(CharacterAttrib.Value))
            {
                if (Options.ContactPointsExpression.Contains("{DEPUnaug}"))
                    OnPropertyChanged(nameof(ContactPoints));
            }
        }

        public void RefreshESSDependentProperties(object sender, PropertyChangedEventArgs e)
        {
            // Only ESS.MetatypeMaximum is used for the Essence method/property when it comes to attributes
            if(e?.PropertyName == nameof(CharacterAttrib.MetatypeMaximum))
            {
                OnMultiplePropertyChanged(nameof(PrototypeTranshumanEssenceUsed), nameof(BiowareEssence), nameof(CyberwareEssence), nameof(EssenceHole));
            }
        }

        public void RefreshEncumbrance()
        {
            // Don't hammer away with this method while this character is loading. Instead, it will be run once after everything has been loaded in.
            if(IsLoading)
                return;
            // Remove any Improvements from Armor Encumbrance.
            ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.ArmorEncumbrance);
            if(!Options.NoArmorEncumbrance)
            {
                // Create the Armor Encumbrance Improvements.
                int intEncumbrance = ArmorEncumbrance;
                if(intEncumbrance != 0)
                {
                    ImprovementManager.CreateImprovement(this, "AGI", Improvement.ImprovementSource.ArmorEncumbrance,
                        string.Empty, Improvement.ImprovementType.Attribute, "precedence-1", 0, 1, 0, 0,
                        intEncumbrance);
                    ImprovementManager.CreateImprovement(this, "REA", Improvement.ImprovementSource.ArmorEncumbrance,
                        string.Empty, Improvement.ImprovementType.Attribute, "precedence-1", 0, 1, 0, 0,
                        intEncumbrance);
                    ImprovementManager.Commit(this);
                }
            }
        }

        public void RefreshWoundPenalties()
        {
            // Don't hammer away with this method while this character is loading. Instead, it will be run once after everything has been loaded in.
            if(IsLoading)
                return;
            int intPhysicalCMFilled = Math.Min(PhysicalCMFilled, PhysicalCM);
            int intStunCMFilled = Math.Min(StunCMFilled, StunCM);
            int intCMThreshold = CMThreshold;
            int intStunCMPenalty = Improvements.Any(objImprovement =>
                objImprovement.ImproveType == Improvement.ImprovementType.IgnoreCMPenaltyStun && objImprovement.Enabled)
                ? 0
                : Math.Min(0, StunCMThresholdOffset - intStunCMFilled) / intCMThreshold;
            int intPhysicalCMPenalty = Improvements.Any(objImprovement =>
                objImprovement.ImproveType == Improvement.ImprovementType.IgnoreCMPenaltyPhysical &&
                objImprovement.Enabled)
                ? 0
                : Math.Min(0, PhysicalCMThresholdOffset - intPhysicalCMFilled) / intCMThreshold;

            _intWoundModifier = intPhysicalCMPenalty + intStunCMPenalty;
        }

        private int _intWoundModifier;

        /// <summary>
        /// Dicepool modifier the character has from wounds. Should be a non-positive number because wound modifiers are always penalties if they are not 0.
        /// </summary>
        public int WoundModifier => _intWoundModifier;

        public Version LastSavedVersion => _verSavedVersion;

        /// <summary>
        /// Is the character a mystic adept (MagicianEnabled && AdeptEnabled)? Used for databinding properties.
        /// </summary>
        public bool IsMysticAdept => AdeptEnabled && MagicianEnabled;

        /// <summary>
        /// Whether this character is using special Mystic Adept PP rules (true) or calculate PPs from Mystic Adept's Adept MAG (false)
        /// </summary>
        public bool UseMysticAdeptPPs => IsMysticAdept && !Options.MysAdeptSecondMAGAttribute;

        /// <summary>
        /// Whether this character is a Mystic Adept uses PPs and can purchase PPs in career mode
        /// </summary>
        public bool MysAdeptAllowPPCareer => UseMysticAdeptPPs && Options.MysAdeptAllowPPCareer;

        /// <summary>
        /// Could this character buy Power Points in career mode if the optional/house rule is enabled
        /// </summary>
        public bool CanAffordCareerPP => MysAdeptAllowPPCareer
                                         && Karma >= Options.KarmaMysticAdeptPowerPoint
                                         && MAG.TotalValue > MysticAdeptPowerPoints;
        /// <summary>
        /// Whether the character is allowed to gain free spells that are limited to the Touch range.
        /// </summary>
        public Tuple<bool, bool> AllowFreeSpells
        {
            get
            {
                //Free Spells (typically from Dedicated Spellslinger or custom Improvements) are only handled manually
                //in Career Mode. Create mode manages itself.
                int intFreeGenericSpells = ImprovementManager.ValueOf(this, Improvement.ImprovementType.FreeSpells);
                int intFreeTouchOnlySpells = 0;
                foreach (Improvement imp in Improvements.Where(i =>
                    (i.ImproveType == Improvement.ImprovementType.FreeSpellsATT
                     || i.ImproveType == Improvement.ImprovementType.FreeSpellsSkill)
                    && i.Enabled))
                {
                    switch (imp.ImproveType)
                    {
                        case Improvement.ImprovementType.FreeSpellsATT:
                            int intAttValue = GetAttribute(imp.ImprovedName).TotalValue;
                            if (imp.UniqueName.Contains("half"))
                                intAttValue = (intAttValue + 1) / 2;
                            if (imp.UniqueName.Contains("touchonly"))
                                intFreeTouchOnlySpells += intAttValue;
                            else
                                intFreeGenericSpells += intAttValue;
                            break;
                        case Improvement.ImprovementType.FreeSpellsSkill:
                            Skill skill = SkillsSection.GetActiveSkill(imp.ImprovedName);
                            int intSkillValue = SkillsSection.GetActiveSkill(imp.ImprovedName).TotalBaseRating;
                            if (imp.UniqueName.Contains("half"))
                                intSkillValue = (intSkillValue + 1) / 2;
                            if (imp.UniqueName.Contains("touchonly"))
                                intFreeTouchOnlySpells += intSkillValue;
                            else
                                intFreeGenericSpells += intSkillValue;
                            //TODO: I don't like this being hardcoded, even though I know full well CGL are never going to reuse this
                            intFreeGenericSpells += skill.Specializations.Count(spec =>
                                Spells.Any(spell => spell.Category == spec.Name && !spell.FreeBonus));
                            break;
                    }
                }

                int intTotalFreeNonTouchSpellsCount = Spells.Count(spell =>
                    spell.FreeBonus && (spell.Range != "T" && spell.Range != "T (A)"));
                int intTotalFreeTouchOnlySpellsCount = Spells.Count(spell =>
                    spell.FreeBonus && (spell.Range == "T" || spell.Range == "T (A)"));
                return new Tuple<bool, bool>(intFreeTouchOnlySpells > intTotalFreeTouchOnlySpellsCount,
                    intFreeGenericSpells > intTotalFreeNonTouchSpellsCount +
                    Math.Max(intTotalFreeTouchOnlySpellsCount - intFreeTouchOnlySpells, 0));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #region Static

        //A tree of dependencies. Once some of the properties are changed,
        //anything they depend on, also needs to raise OnChanged
        //This tree keeps track of dependencies
        private static readonly DependencyGraph<string, Character> s_CharacterDependencyGraph =
            new DependencyGraph<string, Character>(
                    new DependencyGraphNode<string, Character>(nameof(CharacterName),
                        new DependencyGraphNode<string, Character>(nameof(Alias)),
                        new DependencyGraphNode<string, Character>(nameof(Name), x => string.IsNullOrWhiteSpace(x.Alias))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(DisplayPowerPointsRemaining),
                        new DependencyGraphNode<string, Character>(nameof(PowerPointsTotal),
                            new DependencyGraphNode<string, Character>(nameof(UseMysticAdeptPPs),
                                new DependencyGraphNode<string, Character>(nameof(IsMysticAdept),
                                    new DependencyGraphNode<string, Character>(nameof(AdeptEnabled)),
                                    new DependencyGraphNode<string, Character>(nameof(MagicianEnabled))
                                )
                            ),
                            new DependencyGraphNode<string, Character>(nameof(MysticAdeptPowerPoints), x => x.UseMysticAdeptPPs)
                        ),
                        new DependencyGraphNode<string, Character>(nameof(PowerPointsUsed))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(CanAffordCareerPP),
                        new DependencyGraphNode<string, Character>(nameof(MysAdeptAllowPPCareer),
                            new DependencyGraphNode<string, Character>(nameof(UseMysticAdeptPPs))
                        ),
                        new DependencyGraphNode<string, Character>(nameof(MysticAdeptPowerPoints)),
                        new DependencyGraphNode<string, Character>(nameof(Karma))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(AddInitiationsAllowed),
                        new DependencyGraphNode<string, Character>(nameof(IgnoreRules)),
                        new DependencyGraphNode<string, Character>(nameof(Created))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(InitiationEnabled),
                        new DependencyGraphNode<string, Character>(nameof(MAGEnabled)),
                        new DependencyGraphNode<string, Character>(nameof(RESEnabled)),
                        new DependencyGraphNode<string, Character>(nameof(InitiationForceDisabled))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(InitiativeToolTip),
                        new DependencyGraphNode<string, Character>(nameof(Initiative),
                            new DependencyGraphNode<string, Character>(nameof(InitiativeDice)),
                            new DependencyGraphNode<string, Character>(nameof(InitiativeValue),
                                new DependencyGraphNode<string, Character>(nameof(WoundModifier))
                            )
                        )
                    ),
                    new DependencyGraphNode<string, Character>(nameof(AstralInitiativeToolTip),
                        new DependencyGraphNode<string, Character>(nameof(AstralInitiative),
                            new DependencyGraphNode<string, Character>(nameof(AstralInitiativeDice)),
                            new DependencyGraphNode<string, Character>(nameof(AstralInitiativeValue),
                                new DependencyGraphNode<string, Character>(nameof(WoundModifier))
                            )
                        ),
                        new DependencyGraphNode<string, Character>(nameof(MAGEnabled))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(MatrixInitiativeToolTip),
                        new DependencyGraphNode<string, Character>(nameof(MatrixInitiative),
                            new DependencyGraphNode<string, Character>(nameof(MatrixInitiativeDice),
                                new DependencyGraphNode<string, Character>(nameof(IsAI)),
                                new DependencyGraphNode<string, Character>(nameof(InitiativeDice), x => !x.IsAI)
                            ),
                            new DependencyGraphNode<string, Character>(nameof(MatrixInitiativeValue),
                                new DependencyGraphNode<string, Character>(nameof(IsAI)),
                                new DependencyGraphNode<string, Character>(nameof(HomeNode), x => x.IsAI),
                                new DependencyGraphNode<string, Character>(nameof(WoundModifier), x => x.IsAI),
                                new DependencyGraphNode<string, Character>(nameof(InitiativeValue), x => !x.IsAI)
                            )
                        )
                    ),
                    new DependencyGraphNode<string, Character>(nameof(MatrixInitiativeColdToolTip),
                        new DependencyGraphNode<string, Character>(nameof(MatrixInitiativeCold),
                            new DependencyGraphNode<string, Character>(nameof(IsAI)),
                            new DependencyGraphNode<string, Character>(nameof(MatrixInitiative), x => x.IsAI),
                            new DependencyGraphNode<string, Character>(nameof(ActiveCommlink), x => !x.IsAI),
                            new DependencyGraphNode<string, Character>(nameof(MatrixInitiativeColdDice),
                                new DependencyGraphNode<string, Character>(nameof(IsAI)),
                                new DependencyGraphNode<string, Character>(nameof(MatrixInitiativeDice), x => x.IsAI)
                            ),
                            new DependencyGraphNode<string, Character>(nameof(MatrixInitiativeColdValue),
                                new DependencyGraphNode<string, Character>(nameof(ActiveCommlink), x => !x.IsAI),
                                new DependencyGraphNode<string, Character>(nameof(IsAI)),
                                new DependencyGraphNode<string, Character>(nameof(MatrixInitiativeValue), x => x.IsAI),
                                new DependencyGraphNode<string, Character>(nameof(WoundModifier), x => !x.IsAI)
                            )
                        )
                    ),
                    new DependencyGraphNode<string, Character>(nameof(MatrixInitiativeHotToolTip),
                        new DependencyGraphNode<string, Character>(nameof(MatrixInitiativeHot),
                            new DependencyGraphNode<string, Character>(nameof(IsAI)),
                            new DependencyGraphNode<string, Character>(nameof(MatrixInitiative), x => x.IsAI),
                            new DependencyGraphNode<string, Character>(nameof(ActiveCommlink), x => !x.IsAI),
                            new DependencyGraphNode<string, Character>(nameof(MatrixInitiativeHotDice),
                                new DependencyGraphNode<string, Character>(nameof(IsAI)),
                                new DependencyGraphNode<string, Character>(nameof(MatrixInitiativeDice), x => x.IsAI)
                            ),
                            new DependencyGraphNode<string, Character>(nameof(MatrixInitiativeHotValue),
                                new DependencyGraphNode<string, Character>(nameof(ActiveCommlink), x => !x.IsAI),
                                new DependencyGraphNode<string, Character>(nameof(IsAI)),
                                new DependencyGraphNode<string, Character>(nameof(MatrixInitiativeValue), x => x.IsAI),
                                new DependencyGraphNode<string, Character>(nameof(WoundModifier), x => !x.IsAI)
                            )
                        )
                    ),
                    new DependencyGraphNode<string, Character>(nameof(IsSprite),
                        new DependencyGraphNode<string, Character>(nameof(IsFreeSprite),
                            new DependencyGraphNode<string, Character>(nameof(MetatypeCategory))
                        )
                    ),
                    new DependencyGraphNode<string, Character>(nameof(DisplayMetatypeBP),
                        new DependencyGraphNode<string, Character>(nameof(MetatypeBP))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(PhysicalCMLabelText),
                        new DependencyGraphNode<string, Character>(nameof(IsAI)),
                        new DependencyGraphNode<string, Character>(nameof(HomeNode))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(PhysicalCMToolTip),
                        new DependencyGraphNode<string, Character>(nameof(PhysicalCM))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(StunCMToolTip),
                        new DependencyGraphNode<string, Character>(nameof(StunCM))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(StunCMVisible),
                        new DependencyGraphNode<string, Character>(nameof(IsAI)),
                        new DependencyGraphNode<string, Character>(nameof(HomeNode))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(StunCMLabelText),
                        new DependencyGraphNode<string, Character>(nameof(IsAI)),
                        new DependencyGraphNode<string, Character>(nameof(HomeNode))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(WoundModifier),
                        new DependencyGraphNode<string, Character>(nameof(PhysicalCMFilled),
                            new DependencyGraphNode<string, Character>(nameof(HomeNode))
                        ),
                        new DependencyGraphNode<string, Character>(nameof(PhysicalCM),
                            new DependencyGraphNode<string, Character>(nameof(IsAI)),
                            new DependencyGraphNode<string, Character>(nameof(HomeNode))
                        ),
                        new DependencyGraphNode<string, Character>(nameof(StunCMFilled),
                            new DependencyGraphNode<string, Character>(nameof(HomeNode))
                        ),
                        new DependencyGraphNode<string, Character>(nameof(StunCM),
                            new DependencyGraphNode<string, Character>(nameof(IsAI)),
                            new DependencyGraphNode<string, Character>(nameof(HomeNode))
                        ),
                        new DependencyGraphNode<string, Character>(nameof(CMThreshold)),
                        new DependencyGraphNode<string, Character>(nameof(PhysicalCMThresholdOffset),
                            new DependencyGraphNode<string, Character>(nameof(StunCMFilled)),
                            new DependencyGraphNode<string, Character>(nameof(CMThreshold)),
                            new DependencyGraphNode<string, Character>(nameof(IsAI))
                        ),
                        new DependencyGraphNode<string, Character>(nameof(StunCMThresholdOffset),
                            new DependencyGraphNode<string, Character>(nameof(PhysicalCMFilled)),
                            new DependencyGraphNode<string, Character>(nameof(CMThreshold)),
                            new DependencyGraphNode<string, Character>(nameof(IsAI))
                        )
                    ),
                    new DependencyGraphNode<string, Character>(nameof(CMThresholdOffsets),
                        new DependencyGraphNode<string, Character>(nameof(PhysicalCMThresholdOffset)),
                        new DependencyGraphNode<string, Character>(nameof(StunCMThresholdOffset))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(EffectiveBuildMethodUsesPriorityTables),
                        new DependencyGraphNode<string, Character>(nameof(EffectiveBuildMethod),
                            new DependencyGraphNode<string, Character>(nameof(IsCritter))
                        )
                    ),
                    new DependencyGraphNode<string, Character>(nameof(EnableAutomaticStoryButton),
                        new DependencyGraphNode<string, Character>(nameof(EffectiveBuildMethodIsLifeModule),
                            new DependencyGraphNode<string, Character>(nameof(EffectiveBuildMethod))
                        )
                    ),
                    new DependencyGraphNode<string, Character>(nameof(DamageResistancePoolToolTip),
                        new DependencyGraphNode<string, Character>(nameof(DamageResistancePool),
                            new DependencyGraphNode<string, Character>(nameof(TotalArmorRating),
                                new DependencyGraphNode<string, Character>(nameof(ArmorRating))
                            ),
                            new DependencyGraphNode<string, Character>(nameof(IsAI)),
                            new DependencyGraphNode<string, Character>(nameof(HomeNode), x => x.IsAI)
                        )
                    ),
                    new DependencyGraphNode<string, Character>(nameof(IsAI),
                        new DependencyGraphNode<string, Character>(nameof(DEPEnabled))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(SpellDefenseIndirectDodgeToolTip),
                        new DependencyGraphNode<string, Character>(nameof(CurrentCounterspellingDice)),
                        new DependencyGraphNode<string, Character>(nameof(SpellDefenseIndirectDodge),
                            new DependencyGraphNode<string, Character>(nameof(TotalBonusDodgeRating))
                        )
                    ),
                    new DependencyGraphNode<string, Character>(nameof(DodgeToolTip),
                        new DependencyGraphNode<string, Character>(nameof(Dodge),
                            new DependencyGraphNode<string, Character>(nameof(TotalBonusDodgeRating))
                        )
                    ),
                    new DependencyGraphNode<string, Character>(nameof(DisplaySpellDefenseIndirectDodge),
                        new DependencyGraphNode<string, Character>(nameof(CurrentCounterspellingDice)),
                        new DependencyGraphNode<string, Character>(nameof(SpellDefenseIndirectDodge))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(SpellDefenseIndirectSoakToolTip),
                        new DependencyGraphNode<string, Character>(nameof(CurrentCounterspellingDice)),
                        new DependencyGraphNode<string, Character>(nameof(SpellDefenseIndirectSoak),
                            new DependencyGraphNode<string, Character>(nameof(TotalArmorRating)),
                            new DependencyGraphNode<string, Character>(nameof(IsAI)),
                            new DependencyGraphNode<string, Character>(nameof(HomeNode), x => x.IsAI),
                            new DependencyGraphNode<string, Character>(nameof(SpellResistance))
                        )
                    ),
                    new DependencyGraphNode<string, Character>(nameof(DisplaySpellDefenseIndirectSoak),
                        new DependencyGraphNode<string, Character>(nameof(CurrentCounterspellingDice)),
                        new DependencyGraphNode<string, Character>(nameof(SpellDefenseIndirectSoak))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(SpellDefenseDirectSoakManaToolTip),
                        new DependencyGraphNode<string, Character>(nameof(CurrentCounterspellingDice)),
                        new DependencyGraphNode<string, Character>(nameof(SpellDefenseDirectSoakMana),
                            new DependencyGraphNode<string, Character>(nameof(SpellResistance))
                        )
                    ),
                    new DependencyGraphNode<string, Character>(nameof(DisplaySpellDefenseDirectSoakMana),
                        new DependencyGraphNode<string, Character>(nameof(CurrentCounterspellingDice)),
                        new DependencyGraphNode<string, Character>(nameof(SpellDefenseDirectSoakMana))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(SpellDefenseDirectSoakPhysicalToolTip),
                        new DependencyGraphNode<string, Character>(nameof(CurrentCounterspellingDice)),
                        new DependencyGraphNode<string, Character>(nameof(SpellDefenseDirectSoakPhysical),
                            new DependencyGraphNode<string, Character>(nameof(SpellResistance))
                        )
                    ),
                    new DependencyGraphNode<string, Character>(nameof(DisplaySpellDefenseDirectSoakPhysical),
                        new DependencyGraphNode<string, Character>(nameof(CurrentCounterspellingDice)),
                        new DependencyGraphNode<string, Character>(nameof(SpellDefenseDirectSoakPhysical))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(SpellDefenseDetectionToolTip),
                        new DependencyGraphNode<string, Character>(nameof(CurrentCounterspellingDice)),
                        new DependencyGraphNode<string, Character>(nameof(SpellDefenseDetection),
                            new DependencyGraphNode<string, Character>(nameof(SpellResistance))
                        )
                    ),
                    new DependencyGraphNode<string, Character>(nameof(DisplaySpellDefenseDetection),
                        new DependencyGraphNode<string, Character>(nameof(CurrentCounterspellingDice)),
                        new DependencyGraphNode<string, Character>(nameof(SpellDefenseDetection))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(SpellDefenseDecreaseBODToolTip),
                        new DependencyGraphNode<string, Character>(nameof(CurrentCounterspellingDice)),
                        new DependencyGraphNode<string, Character>(nameof(SpellDefenseDecreaseBOD),
                            new DependencyGraphNode<string, Character>(nameof(SpellResistance))
                        )
                    ),
                    new DependencyGraphNode<string, Character>(nameof(DisplaySpellDefenseDecreaseBOD),
                        new DependencyGraphNode<string, Character>(nameof(CurrentCounterspellingDice)),
                        new DependencyGraphNode<string, Character>(nameof(SpellDefenseDecreaseBOD))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(SpellDefenseDecreaseAGIToolTip),
                        new DependencyGraphNode<string, Character>(nameof(CurrentCounterspellingDice)),
                        new DependencyGraphNode<string, Character>(nameof(SpellDefenseDecreaseAGI),
                            new DependencyGraphNode<string, Character>(nameof(SpellResistance))
                        )
                    ),
                    new DependencyGraphNode<string, Character>(nameof(DisplaySpellDefenseDecreaseAGI),
                        new DependencyGraphNode<string, Character>(nameof(CurrentCounterspellingDice)),
                        new DependencyGraphNode<string, Character>(nameof(SpellDefenseDecreaseAGI))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(SpellDefenseDecreaseREAToolTip),
                        new DependencyGraphNode<string, Character>(nameof(CurrentCounterspellingDice)),
                        new DependencyGraphNode<string, Character>(nameof(SpellDefenseDecreaseREA),
                            new DependencyGraphNode<string, Character>(nameof(SpellResistance))
                        )
                    ),
                    new DependencyGraphNode<string, Character>(nameof(DisplaySpellDefenseDecreaseREA),
                        new DependencyGraphNode<string, Character>(nameof(CurrentCounterspellingDice)),
                        new DependencyGraphNode<string, Character>(nameof(SpellDefenseDecreaseREA))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(SpellDefenseDecreaseSTRToolTip),
                        new DependencyGraphNode<string, Character>(nameof(CurrentCounterspellingDice)),
                        new DependencyGraphNode<string, Character>(nameof(SpellDefenseDecreaseSTR),
                            new DependencyGraphNode<string, Character>(nameof(SpellResistance))
                        )
                    ),
                    new DependencyGraphNode<string, Character>(nameof(DisplaySpellDefenseDecreaseSTR),
                        new DependencyGraphNode<string, Character>(nameof(CurrentCounterspellingDice)),
                        new DependencyGraphNode<string, Character>(nameof(SpellDefenseDecreaseSTR))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(SpellDefenseDecreaseCHAToolTip),
                        new DependencyGraphNode<string, Character>(nameof(CurrentCounterspellingDice)),
                        new DependencyGraphNode<string, Character>(nameof(SpellDefenseDecreaseCHA),
                            new DependencyGraphNode<string, Character>(nameof(SpellResistance))
                        )
                    ),
                    new DependencyGraphNode<string, Character>(nameof(DisplaySpellDefenseDecreaseCHA),
                        new DependencyGraphNode<string, Character>(nameof(CurrentCounterspellingDice)),
                        new DependencyGraphNode<string, Character>(nameof(SpellDefenseDecreaseCHA))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(SpellDefenseDecreaseINTToolTip),
                        new DependencyGraphNode<string, Character>(nameof(CurrentCounterspellingDice)),
                        new DependencyGraphNode<string, Character>(nameof(SpellDefenseDecreaseINT),
                            new DependencyGraphNode<string, Character>(nameof(SpellResistance))
                        )
                    ),
                    new DependencyGraphNode<string, Character>(nameof(DisplaySpellDefenseDecreaseINT),
                        new DependencyGraphNode<string, Character>(nameof(CurrentCounterspellingDice)),
                        new DependencyGraphNode<string, Character>(nameof(SpellDefenseDecreaseINT))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(SpellDefenseDecreaseLOGToolTip),
                        new DependencyGraphNode<string, Character>(nameof(CurrentCounterspellingDice)),
                        new DependencyGraphNode<string, Character>(nameof(SpellDefenseDecreaseLOG),
                            new DependencyGraphNode<string, Character>(nameof(SpellResistance))
                        )
                    ),
                    new DependencyGraphNode<string, Character>(nameof(DisplaySpellDefenseDecreaseLOG),
                        new DependencyGraphNode<string, Character>(nameof(CurrentCounterspellingDice)),
                        new DependencyGraphNode<string, Character>(nameof(SpellDefenseDecreaseLOG))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(SpellDefenseDecreaseWILToolTip),
                        new DependencyGraphNode<string, Character>(nameof(CurrentCounterspellingDice)),
                        new DependencyGraphNode<string, Character>(nameof(SpellDefenseDecreaseWIL),
                            new DependencyGraphNode<string, Character>(nameof(SpellResistance))
                        )
                    ),
                    new DependencyGraphNode<string, Character>(nameof(DisplaySpellDefenseDecreaseWIL),
                        new DependencyGraphNode<string, Character>(nameof(CurrentCounterspellingDice)),
                        new DependencyGraphNode<string, Character>(nameof(SpellDefenseDecreaseWIL))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(SpellDefenseIllusionManaToolTip),
                        new DependencyGraphNode<string, Character>(nameof(CurrentCounterspellingDice)),
                        new DependencyGraphNode<string, Character>(nameof(SpellDefenseIllusionMana),
                            new DependencyGraphNode<string, Character>(nameof(SpellResistance))
                        )
                    ),
                    new DependencyGraphNode<string, Character>(nameof(DisplaySpellDefenseIllusionMana),
                        new DependencyGraphNode<string, Character>(nameof(CurrentCounterspellingDice)),
                        new DependencyGraphNode<string, Character>(nameof(SpellDefenseIllusionMana))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(SpellDefenseIllusionPhysicalToolTip),
                        new DependencyGraphNode<string, Character>(nameof(CurrentCounterspellingDice)),
                        new DependencyGraphNode<string, Character>(nameof(SpellDefenseIllusionPhysical),
                            new DependencyGraphNode<string, Character>(nameof(SpellResistance))
                        )
                    ),
                    new DependencyGraphNode<string, Character>(nameof(DisplaySpellDefenseIllusionPhysical),
                        new DependencyGraphNode<string, Character>(nameof(CurrentCounterspellingDice)),
                        new DependencyGraphNode<string, Character>(nameof(SpellDefenseIllusionPhysical))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(SpellDefenseManipulationMentalToolTip),
                        new DependencyGraphNode<string, Character>(nameof(CurrentCounterspellingDice)),
                        new DependencyGraphNode<string, Character>(nameof(SpellDefenseManipulationMental),
                            new DependencyGraphNode<string, Character>(nameof(SpellResistance))
                        )
                    ),
                    new DependencyGraphNode<string, Character>(nameof(DisplaySpellDefenseManipulationMental),
                        new DependencyGraphNode<string, Character>(nameof(CurrentCounterspellingDice)),
                        new DependencyGraphNode<string, Character>(nameof(SpellDefenseManipulationMental))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(SpellDefenseManipulationPhysicalToolTip),
                        new DependencyGraphNode<string, Character>(nameof(CurrentCounterspellingDice)),
                        new DependencyGraphNode<string, Character>(nameof(SpellDefenseManipulationPhysical),
                            new DependencyGraphNode<string, Character>(nameof(SpellResistance)),
                            new DependencyGraphNode<string, Character>(nameof(IsAI)),
                            new DependencyGraphNode<string, Character>(nameof(HomeNode), x => x.IsAI)
                        )
                    ),
                    new DependencyGraphNode<string, Character>(nameof(DisplaySpellDefenseManipulationPhysical),
                        new DependencyGraphNode<string, Character>(nameof(CurrentCounterspellingDice)),
                        new DependencyGraphNode<string, Character>(nameof(SpellDefenseManipulationPhysical))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(TotalArmorRatingToolTip),
                        new DependencyGraphNode<string, Character>(nameof(TotalArmorRating))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(TotalFireArmorRating),
                        new DependencyGraphNode<string, Character>(nameof(TotalArmorRating))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(TotalColdArmorRating),
                        new DependencyGraphNode<string, Character>(nameof(TotalArmorRating))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(TotalElectricityArmorRating),
                        new DependencyGraphNode<string, Character>(nameof(TotalArmorRating))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(TotalAcidArmorRating),
                        new DependencyGraphNode<string, Character>(nameof(TotalArmorRating))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(TotalFallingArmorRating),
                        new DependencyGraphNode<string, Character>(nameof(TotalArmorRating))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(DisplayEssence),
                        new DependencyGraphNode<string, Character>(nameof(Essence),
                            new DependencyGraphNode<string, Character>(nameof(CyberwareEssence)),
                            new DependencyGraphNode<string, Character>(nameof(BiowareEssence)),
                            new DependencyGraphNode<string, Character>(nameof(PrototypeTranshumanEssenceUsed)),
                            new DependencyGraphNode<string, Character>(nameof(EssenceHole))
                        )
                    ),
                    new DependencyGraphNode<string, Character>(nameof(ComposureToolTip),
                        new DependencyGraphNode<string, Character>(nameof(Composure))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(SurpriseToolTip),
                        new DependencyGraphNode<string, Character>(nameof(Surprise))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(JudgeIntentionsToolTip),
                        new DependencyGraphNode<string, Character>(nameof(JudgeIntentions))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(JudgeIntentionsResistToolTip),
                        new DependencyGraphNode<string, Character>(nameof(JudgeIntentionsResist))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(LiftAndCarryToolTip),
                        new DependencyGraphNode<string, Character>(nameof(LiftAndCarry))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(MemoryToolTip),
                        new DependencyGraphNode<string, Character>(nameof(Memory))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(DisplayCyberwareEssence),
                        new DependencyGraphNode<string, Character>(nameof(CyberwareEssence))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(DisplayBiowareEssence),
                        new DependencyGraphNode<string, Character>(nameof(BiowareEssence))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(DisplayEssenceHole),
                        new DependencyGraphNode<string, Character>(nameof(EssenceHole))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(DisplayPrototypeTranshumanEssenceUsed),
                        new DependencyGraphNode<string, Character>(nameof(PrototypeTranshumanEssenceUsed)),
                        new DependencyGraphNode<string, Character>(nameof(PrototypeTranshuman))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(IsPrototypeTranshuman),
                        new DependencyGraphNode<string, Character>(nameof(PrototypeTranshuman))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(DisplayNuyen),
                        new DependencyGraphNode<string, Character>(nameof(Nuyen))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(DisplayStolenNuyen),
                        new DependencyGraphNode<string, Character>(nameof(StolenNuyen))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(DisplayKarma),
                        new DependencyGraphNode<string, Character>(nameof(Karma))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(DisplayTotalStartingNuyen),
                        new DependencyGraphNode<string, Character>(nameof(TotalStartingNuyen),
                            new DependencyGraphNode<string, Character>(nameof(StartingNuyen)),
                            new DependencyGraphNode<string, Character>(nameof(StartingNuyenModifiers)),
                            new DependencyGraphNode<string, Character>(nameof(NuyenBP),
                                new DependencyGraphNode<string, Character>(nameof(TotalNuyenMaximumBP),
                                    new DependencyGraphNode<string, Character>(nameof(StolenNuyen)),
                                    new DependencyGraphNode<string, Character>(nameof(IgnoreRules))
                                )
                            )
                        )
                    ),
                    new DependencyGraphNode<string, Character>(nameof(DisplayCareerNuyen),
                        new DependencyGraphNode<string, Character>(nameof(CareerNuyen))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(DisplayCareerKarma),
                        new DependencyGraphNode<string, Character>(nameof(CareerKarma))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(StreetCredTooltip),
                        new DependencyGraphNode<string, Character>(nameof(TotalStreetCred),
                            new DependencyGraphNode<string, Character>(nameof(StreetCred)),
                            new DependencyGraphNode<string, Character>(nameof(CalculatedStreetCred),
                                new DependencyGraphNode<string, Character>(nameof(CareerKarma)),
                                new DependencyGraphNode<string, Character>(nameof(BurntStreetCred))
                            )
                        )
                    ),
                    new DependencyGraphNode<string, Character>(nameof(CanBurnStreetCred),
                        new DependencyGraphNode<string, Character>(nameof(TotalStreetCred))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(NotorietyTooltip),
                        new DependencyGraphNode<string, Character>(nameof(TotalNotoriety),
                            new DependencyGraphNode<string, Character>(nameof(Notoriety)),
                            new DependencyGraphNode<string, Character>(nameof(CalculatedNotoriety)),
                            new DependencyGraphNode<string, Character>(nameof(BurntStreetCred))
                        )
                    ),
                    new DependencyGraphNode<string, Character>(nameof(PublicAwarenessTooltip),
                        new DependencyGraphNode<string, Character>(nameof(TotalPublicAwareness),
                            new DependencyGraphNode<string, Character>(nameof(Erased)),
                            new DependencyGraphNode<string, Character>(nameof(CalculatedPublicAwareness),
                                new DependencyGraphNode<string, Character>(nameof(PublicAwareness)),
                                new DependencyGraphNode<string, Character>(nameof(TotalStreetCred),
                                    x => x.Options.UseCalculatedPublicAwareness),
                                new DependencyGraphNode<string, Character>(nameof(TotalNotoriety),
                                    x => x.Options.UseCalculatedPublicAwareness)
                            )
                        )
                    ),
                    new DependencyGraphNode<string, Character>(nameof(CareerDisplayStreetCred),
                        new DependencyGraphNode<string, Character>(nameof(TotalStreetCred))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(CareerDisplayNotoriety),
                        new DependencyGraphNode<string, Character>(nameof(TotalNotoriety))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(CareerDisplayPublicAwareness),
                        new DependencyGraphNode<string, Character>(nameof(TotalPublicAwareness))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(AddBiowareEnabled),
                        new DependencyGraphNode<string, Character>(nameof(CyberwareDisabled))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(AddCyberwareEnabled),
                        new DependencyGraphNode<string, Character>(nameof(CyberwareDisabled))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(HasMentorSpirit),
                        new DependencyGraphNode<string, Character>(nameof(MentorSpirits))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(CharacterGrammaticGender),
                        new DependencyGraphNode<string, Character>(nameof(Sex))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(FirstMentorSpiritDisplayName),
                        new DependencyGraphNode<string, Character>(nameof(MentorSpirits))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(FirstMentorSpiritDisplayInformation),
                        new DependencyGraphNode<string, Character>(nameof(MentorSpirits))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(LimitPhysicalToolTip),
                        new DependencyGraphNode<string, Character>(nameof(LimitPhysical),
                            new DependencyGraphNode<string, Character>(nameof(HomeNode))
                        )
                    ),
                    new DependencyGraphNode<string, Character>(nameof(LimitMentalToolTip),
                        new DependencyGraphNode<string, Character>(nameof(LimitMental),
                            new DependencyGraphNode<string, Character>(nameof(HomeNode))
                        )
                    ),
                    new DependencyGraphNode<string, Character>(nameof(LimitSocialToolTip),
                        new DependencyGraphNode<string, Character>(nameof(LimitSocial),
                            new DependencyGraphNode<string, Character>(nameof(HomeNode))
                        )
                    ),
                    new DependencyGraphNode<string, Character>(nameof(LimitAstralToolTip),
                        new DependencyGraphNode<string, Character>(nameof(LimitAstral),
                            new DependencyGraphNode<string, Character>(nameof(LimitMental)),
                            new DependencyGraphNode<string, Character>(nameof(LimitSocial))
                        )
                    ),
                    new DependencyGraphNode<string, Character>(nameof(DisplayMovement),
                        new DependencyGraphNode<string, Character>(nameof(GetMovement),
                            new DependencyGraphNode<string, Character>(nameof(Movement)),
                            new DependencyGraphNode<string, Character>(nameof(CalculatedMovement),
                                new DependencyGraphNode<string, Character>(nameof(WalkingRate),
                                    new DependencyGraphNode<string, Character>(nameof(CurrentWalkingRateString),
                                        new DependencyGraphNode<string, Character>(nameof(WalkString),
                                            x => x.AttributeSection.AttributeCategory ==
                                                 CharacterAttrib.AttributeCategory.Standard),
                                        new DependencyGraphNode<string, Character>(nameof(WalkAltString),
                                            x => x.AttributeSection.AttributeCategory !=
                                                 CharacterAttrib.AttributeCategory.Standard)
                                    )
                                ),
                                new DependencyGraphNode<string, Character>(nameof(RunningRate),
                                    new DependencyGraphNode<string, Character>(nameof(CurrentRunningRateString),
                                        new DependencyGraphNode<string, Character>(nameof(RunString),
                                            x => x.AttributeSection.AttributeCategory ==
                                                 CharacterAttrib.AttributeCategory.Standard),
                                        new DependencyGraphNode<string, Character>(nameof(RunAltString),
                                            x => x.AttributeSection.AttributeCategory !=
                                                 CharacterAttrib.AttributeCategory.Standard)
                                    )
                                ),
                                new DependencyGraphNode<string, Character>(nameof(SprintingRate),
                                    new DependencyGraphNode<string, Character>(nameof(CurrentSprintingRateString),
                                        new DependencyGraphNode<string, Character>(nameof(SprintString),
                                            x => x.AttributeSection.AttributeCategory ==
                                                 CharacterAttrib.AttributeCategory.Standard),
                                        new DependencyGraphNode<string, Character>(nameof(SprintAltString),
                                            x => x.AttributeSection.AttributeCategory !=
                                                 CharacterAttrib.AttributeCategory.Standard)
                                    )
                                )
                            )
                        )
                    ),
                    new DependencyGraphNode<string, Character>(nameof(DisplaySwim),
                        new DependencyGraphNode<string, Character>(nameof(GetSwim),
                            new DependencyGraphNode<string, Character>(nameof(Movement)),
                            new DependencyGraphNode<string, Character>(nameof(CalculatedMovement))
                        )
                    ),
                    new DependencyGraphNode<string, Character>(nameof(DisplayFly),
                        new DependencyGraphNode<string, Character>(nameof(GetFly),
                            new DependencyGraphNode<string, Character>(nameof(Movement)),
                            new DependencyGraphNode<string, Character>(nameof(CalculatedMovement))
                        )
                    ),
                    new DependencyGraphNode<string, Character>(nameof(DisplayNegativeQualityKarma),
                        new DependencyGraphNode<string, Character>(nameof(NegativeQualityKarma),
                            new DependencyGraphNode<string, Character>(nameof(EnemyKarma)),
                            new DependencyGraphNode<string, Character>(nameof(Contacts)),
                            new DependencyGraphNode<string, Character>(nameof(Qualities))
                        )
                    ),
                    new DependencyGraphNode<string, Character>(nameof(DisplayPositiveQualityKarma),
                        new DependencyGraphNode<string, Character>(nameof(PositiveQualityKarma),
                            new DependencyGraphNode<string, Character>(nameof(Contacts)),
                            new DependencyGraphNode<string, Character>(nameof(Qualities))
                        )
                    ),
                    new DependencyGraphNode<string, Character>(nameof(DisplayMetagenicQualityKarma),
                        new DependencyGraphNode<string, Character>(nameof(MetagenicPositiveQualityKarma),
                        new DependencyGraphNode<string, Character>(nameof(MetagenicNegativeQualityKarma),
                            new DependencyGraphNode<string, Character>(nameof(IsChangeling)),
                            new DependencyGraphNode<string, Character>(nameof(Qualities))
                        ))
                    ),
                    new DependencyGraphNode<string, Character>(nameof(AstralReputationTooltip),
                        new DependencyGraphNode<string, Character>(nameof(TotalAstralReputation),
                            new DependencyGraphNode<string, Character>(nameof(AstralReputation))
                        )
                    ),
                    new DependencyGraphNode<string, Character>(nameof(WildReputationTooltip),
                        new DependencyGraphNode<string, Character>(nameof(TotalWildReputation),
                            new DependencyGraphNode<string, Character>(nameof(WildReputation))
                        )
                    )
                );
        #endregion

        [NotifyPropertyChangedInvocator]
        public void OnPropertyChanged([CallerMemberName] string strPropertyName = null)
        {
            OnMultiplePropertyChanged(strPropertyName);
        }

        public void OnMultiplePropertyChanged(params string[] lstPropertyNames)
        {
            HashSet<string> lstNamesOfChangedProperties = null;
            foreach(string strPropertyName in lstPropertyNames)
            {
                if(lstNamesOfChangedProperties == null)
                    lstNamesOfChangedProperties = s_CharacterDependencyGraph.GetWithAllDependents(this, strPropertyName);
                else
                {
                    foreach(string strLoopChangedProperty in s_CharacterDependencyGraph.GetWithAllDependents(
                        this, strPropertyName))
                        lstNamesOfChangedProperties.Add(strLoopChangedProperty);
                }
            }

            if((lstNamesOfChangedProperties?.Count > 0) != true)
                return;

            if(lstNamesOfChangedProperties.Contains(nameof(CharacterGrammaticGender)))
            {
                _strCachedCharacterGrammaticGender = string.Empty;
            }

            if(lstNamesOfChangedProperties.Contains(nameof(ContactPoints)))
            {
                _intCachedContactPoints = int.MinValue;
            }

            if(lstNamesOfChangedProperties.Contains(nameof(TrustFund)))
            {
                _intCachedTrustFund = int.MinValue;
            }

            if(lstNamesOfChangedProperties.Contains(nameof(Ambidextrous)))
            {
                _intCachedAmbidextrous = int.MinValue;
            }

            if(lstNamesOfChangedProperties.Contains(nameof(RestrictedGear)))
            {
                _intCachedRestrictedGear = int.MinValue;
            }

            if(lstNamesOfChangedProperties.Contains(nameof(FriendsInHighPlaces)))
            {
                _intCachedFriendsInHighPlaces = int.MinValue;
            }

            if(lstNamesOfChangedProperties.Contains(nameof(ExCon)))
            {
                _intCachedExCon = int.MinValue;
            }

            if(lstNamesOfChangedProperties.Contains(nameof(MadeMan)))
            {
                _intCachedMadeMan = int.MinValue;
            }

            if(lstNamesOfChangedProperties.Contains(nameof(Fame)))
            {
                _intCachedFame = int.MinValue;
            }

            if(lstNamesOfChangedProperties.Contains(nameof(Erased)))
            {
                _intCachedErased = int.MinValue;
            }

            if (lstNamesOfChangedProperties.Contains(nameof(AllowSpriteFettering)))
            {
                _intCachedAllowSpriteFettering = int.MinValue;
            }

            if (lstNamesOfChangedProperties.Contains(nameof(Overclocker)))
            {
                _intCachedOverclocker = int.MinValue;
            }

            if(lstNamesOfChangedProperties.Contains(nameof(Ambidextrous)))
            {
                _intCachedAmbidextrous = int.MinValue;
            }

            if(lstNamesOfChangedProperties.Contains(nameof(BlackMarketDiscount)))
            {
                _intCachedBlackMarketDiscount = int.MinValue;
            }

            if(lstNamesOfChangedProperties.Contains(nameof(PowerPointsUsed)))
            {
                _decCachedPowerPointsUsed = decimal.MinValue;
            }

            if(lstNamesOfChangedProperties.Contains(nameof(CyberwareEssence)))
            {
                _decCachedCyberwareEssence = decimal.MinValue;
            }

            if(lstNamesOfChangedProperties.Contains(nameof(BiowareEssence)))
            {
                _decCachedPrototypeTranshumanEssenceUsed = decimal.MinValue;
                _decCachedBiowareEssence = decimal.MinValue;
            }

            if(lstNamesOfChangedProperties.Contains(nameof(EssenceHole)))
            {
                _decCachedEssenceHole = decimal.MinValue;
            }

            if(lstNamesOfChangedProperties.Contains(nameof(PrototypeTranshumanEssenceUsed)))
            {
                _decCachedBiowareEssence = decimal.MinValue;
                _decCachedPrototypeTranshumanEssenceUsed = decimal.MinValue;
            }

            if(lstNamesOfChangedProperties.Contains(nameof(CareerNuyen)))
            {
                _decCachedCareerNuyen = decimal.MinValue;
            }

            if(lstNamesOfChangedProperties.Contains(nameof(CareerKarma)))
            {
                _intCachedCareerKarma = int.MinValue;
            }

            if(lstNamesOfChangedProperties.Contains(nameof(InitiationEnabled)))
            {
                _intCachedInitiationEnabled = -1;
            }

            if(lstNamesOfChangedProperties.Contains(nameof(RedlinerBonus)))
            {
                _intCachedRedlinerBonus = int.MinValue;
                RefreshRedlinerImprovements();
            }

            if(lstNamesOfChangedProperties.Contains(nameof(Essence)))
            {
                ResetCachedEssence();
                RefreshEssenceLossImprovements();
            }

            if(lstNamesOfChangedProperties.Contains(nameof(WoundModifier)))
            {
                RefreshWoundPenalties();
            }

            if (lstNamesOfChangedProperties.Contains(nameof(Contacts)))
            {
                _intCachedEnemyKarma = int.MinValue;
            }

            if (lstNamesOfChangedProperties.Contains(nameof(Qualities)))
            {
                _intCachedNegativeQualities = int.MinValue;
                _intCachedNegativeQualityLimitKarma = int.MinValue;
                _intCachedPositiveQualities = int.MinValue;
                _intCachedPositiveQualitiesTotal = int.MinValue;
                _intCachedMetagenicNegativeQualities = int.MinValue;
                _intCachedMetagenicPositiveQualities = int.MinValue;
            }

            if (lstNamesOfChangedProperties.Contains(nameof(MetagenicLimit)))
            {
                _intCachedMetagenicNegativeQualities = int.MinValue;
                _intCachedMetagenicPositiveQualities = int.MinValue;
            }

            if (lstNamesOfChangedProperties.Contains(nameof(TotalAstralReputation)))
                RefreshAstralReputationImprovements();

            if (lstNamesOfChangedProperties.Contains(nameof(Options)))
                foreach (string strProperty in Options.GetType().GetProperties().Select(x => x.Name))
                    OptionsOnPropertyChanged(this, new PropertyChangedEventArgs(strProperty));

            foreach (string strPropertyToChange in lstNamesOfChangedProperties)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strPropertyToChange));
            }

            if (!Created)
            {
                // If in create mode, update the Force for Spirits and Sprites (equal to Magician MAG Rating or RES Rating).
                if (lstNamesOfChangedProperties.Contains(nameof(MaxSpriteLevel)))
                {
                    foreach (Spirit objSpirit in Spirits)
                    {
                        if(objSpirit.EntityType != SpiritType.Spirit)
                            objSpirit.Force = MaxSpriteLevel;
                    }
                }

                if (lstNamesOfChangedProperties.Contains(nameof(MaxSpiritForce)))
                {
                    foreach (Spirit objSpirit in Spirits)
                    {
                        if(objSpirit.EntityType == SpiritType.Spirit)
                            objSpirit.Force = MaxSpiritForce;
                    }
                }
            }

            if (Program.MainForm == null)
                return;
            foreach(Character objLoopOpenCharacter in Program.MainForm.OpenCharacters)
            {
                if(objLoopOpenCharacter != this && objLoopOpenCharacter.LinkedCharacters.Contains(this))
                {
                    foreach(Spirit objSpirit in objLoopOpenCharacter.Spirits)
                    {
                        if(objSpirit.LinkedCharacter == this)
                        {
                            objSpirit.OnPropertyChanged(nameof(Spirit.LinkedCharacter));
                        }
                    }

                    foreach(Contact objContact in objLoopOpenCharacter.Contacts)
                    {
                        if(objContact.LinkedCharacter == this)
                        {
                            objContact.OnPropertyChanged(nameof(Contact.LinkedCharacter));
                        }
                    }
                }
            }
        }

        #region Hero Lab Importing

        private static readonly string[] s_LstHeroLabPluginNodeNames = { "modifications", "accessories", "ammunition", "programs", "othergear" };
        public static ReadOnlyCollection<string> HeroLabPluginNodeNames { get; } = Array.AsReadOnly(s_LstHeroLabPluginNodeNames);

        /// <summary>
        /// Load the Character from an XML file.
        /// </summary>
        public async Task<bool> LoadFromHeroLabFile(string strPorFile, string strCharacterId, string strSettingsName = "")
        {
            if(!File.Exists(strPorFile))
                return false;

            Dictionary<string, Bitmap> dicImages = new Dictionary<string, Bitmap>();
            XmlDocument xmlStatBlockDocument = null;
            XmlDocument xmlLeadsDocument = null;
            List<string> lstTextStatBlockLines = null;
            using (var op_load = Timekeeper.StartSyncron("LoadFromHeroLabFile", null, CustomActivity.OperationType.DependencyOperation, strPorFile))
            {
                try
                {
                    op_load.MyDependencyTelemetry.Type = "loadHeroLab";
                    op_load.MyDependencyTelemetry.Target = strPorFile;

                    try
                    {
                        string strLeadsName = string.Empty;
                        using (ZipArchive zipArchive = ZipFile.Open(strPorFile, ZipArchiveMode.Read, Encoding.GetEncoding(850)))
                        {
                            foreach (ZipArchiveEntry entry in zipArchive.Entries)
                            {
                                string strEntryFullName = entry.FullName;
                                string strKey = Path.GetFileName(strEntryFullName);
                                if ((xmlStatBlockDocument == null && strEntryFullName.StartsWith("statblocks_xml", StringComparison.OrdinalIgnoreCase)) ||
                                    (string.IsNullOrEmpty(strLeadsName) &&
                                     strEntryFullName.EndsWith("portfolio.xml", StringComparison.OrdinalIgnoreCase)) ||
                                    lstTextStatBlockLines == null && strEntryFullName.StartsWith("statblocks_txt", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (strEntryFullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                                    {
                                        XmlDocument xmlSourceDoc = new XmlDocument
                                        {
                                            XmlResolver = null
                                        };
                                        try
                                        {
                                            using (StreamReader sr = new StreamReader(entry.Open(), true))
                                                using (XmlReader objXmlReader = XmlReader.Create(sr, new XmlReaderSettings
                                                {
                                                    XmlResolver = null,
                                                    Async = true
                                                }))
                                                    xmlSourceDoc.Load(objXmlReader);
                                            if (strEntryFullName.StartsWith("statblocks_xml", StringComparison.Ordinal))
                                            {
                                                if (xmlSourceDoc.SelectSingleNode(
                                                    "/document/public/character[@name = " +
                                                    strCharacterId.CleanXPath() + "]") != null)
                                                    xmlStatBlockDocument = xmlSourceDoc;
                                            }
                                            else
                                            {
                                                strLeadsName = xmlSourceDoc
                                                    .SelectSingleNode(
                                                        "/document/portfolio/hero[@heroname = " +
                                                        strCharacterId.CleanXPath() + "]/@leadfile")?.InnerText;
                                            }
                                        }
                                        // If we run into any problems loading the character xml files, fail out early.
                                        catch (IOException)
                                        {
                                        }
                                        catch (XmlException)
                                        {
                                        }
                                    }
                                    else if (strEntryFullName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) && !strKey.Contains('.'))
                                    {
                                        lstTextStatBlockLines = new List<string>(30);

                                        using (StreamReader objReader = File.OpenText(strEntryFullName))
                                        {
                                            string strLine;
                                            while ((strLine = objReader.ReadLine()) != null)
                                            {
                                                // Trim away the newlines and empty spaces at the beginning and end of lines
                                                strLine = strLine.Trim('\n', '\r').Trim();

                                                lstTextStatBlockLines.Add(strLine);
                                            }
                                        }
                                    }
                                }
                                else if (strEntryFullName.StartsWith("images", StringComparison.Ordinal) && strEntryFullName.Contains('.'))
                                {
                                    Bitmap bmpMugshot = new Bitmap(entry.Open(), true);
                                    if (bmpMugshot.PixelFormat == PixelFormat.Format32bppPArgb)
                                    {
                                        if (dicImages.ContainsKey(strKey))
                                        {
                                            dicImages[strKey].Dispose();
                                            dicImages[strKey] = bmpMugshot;
                                        }
                                        else
                                            dicImages.Add(strKey, bmpMugshot);
                                    }
                                    else
                                    {
                                        try
                                        {
                                            Bitmap bmpMugshotCorrected = bmpMugshot.ConvertPixelFormat(PixelFormat.Format32bppPArgb);
                                            if (dicImages.ContainsKey(strKey))
                                            {
                                                dicImages[strKey].Dispose();
                                                dicImages[strKey] = bmpMugshotCorrected;
                                            }
                                            else
                                                dicImages.Add(strKey, bmpMugshotCorrected);
                                        }
                                        finally
                                        {
                                            bmpMugshot.Dispose();
                                        }
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(strLeadsName))
                            {
                                // Need a second sweep for the Leads file
                                foreach (ZipArchiveEntry entry in zipArchive.Entries)
                                {
                                    string strEntryFullName = entry.FullName;
                                    if (strEntryFullName.EndsWith(strLeadsName, StringComparison.OrdinalIgnoreCase))
                                    {
                                        XmlDocument xmlSourceDoc = new XmlDocument
                                        {
                                            XmlResolver = null
                                        };
                                        try
                                        {
                                            using (StreamReader sr = new StreamReader(entry.Open(), true))
                                                using (XmlReader objXmlReader = XmlReader.Create(sr, new XmlReaderSettings
                                                {
                                                    XmlResolver = null,
                                                    Async = true
                                                }))
                                                    xmlSourceDoc.Load(objXmlReader);
                                            xmlLeadsDocument = xmlSourceDoc;
                                        }
                                        // If we run into any problems loading the character xml files, fail out early.
                                        catch (IOException)
                                        {
                                            continue;
                                        }
                                        catch (XmlException)
                                        {
                                            continue;
                                        }

                                        break;
                                    }
                                }
                            }
                        }
                    }
                    catch (IOException ex)
                    {
                        if (op_load != null)
                        {
                            op_load.SetSuccess(false);
                            op_load.AddBaggage(ex.GetType().Name, ex.Message);
                            Log.Error(ex);
                        }

                        Program.MainForm.ShowMessageBox(
                            LanguageManager.GetString("Message_FailedLoad")
                                .Replace("{0}", ex.Message),
                            LanguageManager.GetString("MessageTitle_FailedLoad"),
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                    catch (NotSupportedException ex)
                    {
                        if (op_load != null)
                        {
                            op_load.SetSuccess(false);
                            op_load.AddBaggage(ex.GetType().Name, ex.Message);
                            Log.Error(ex);
                        }
                        Program.MainForm.ShowMessageBox(
                            LanguageManager.GetString("Message_FailedLoad")
                                .Replace("{0}", ex.Message),
                            LanguageManager.GetString("MessageTitle_FailedLoad"),
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        if (op_load != null)
                        {
                            op_load.SetSuccess(false);
                            op_load.AddBaggage(ex.GetType().Name, ex.Message);
                            Log.Error(ex);
                        }
                        Program.MainForm.ShowMessageBox(
                            LanguageManager.GetString("Message_FailedLoad")
                                .Replace("{0}", ex.Message),
                            LanguageManager.GetString("MessageTitle_FailedLoad"),
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                    if (xmlLeadsDocument == null || xmlStatBlockDocument == null)
                    {
                        return false;
                    }

                    XmlNode xmlStatBlockBaseNode;
                    XmlNode xmlLeadsBaseNode;
                    using (_ = Timekeeper.StartSyncron("load_char_misc", op_load))
                    {
                        IsLoading = true;

                        _dateFileLastWriteTime = File.GetLastWriteTimeUtc(strPorFile);

                        xmlStatBlockBaseNode =
                            xmlStatBlockDocument.SelectSingleNode("/document/public/character[@name = " +
                                                                  strCharacterId.CleanXPath() + "]");
                        xmlLeadsBaseNode =
                            xmlLeadsDocument.SelectSingleNode("/document/hero[@heroname = " +
                                                              strCharacterId.CleanXPath() +
                                                              "]");

                        _blnCreated = (xmlStatBlockBaseNode.SelectSingleNode("karma/@total")?.InnerText ?? "0") != "0";
                        if (!_blnCreated)
                        {
                            XmlNodeList xmlJournalEntries = xmlStatBlockBaseNode.SelectNodes("journals/journal");
                            if (xmlJournalEntries.Count > 1)
                            {
                                _blnCreated = true;
                            }
                            else if (xmlJournalEntries.Count == 1 &&
                                     xmlJournalEntries[0].Attributes["name"]?.InnerText != "Title")
                            {
                                _blnCreated = true;
                            }
                        }

                        ResetCharacter();

                        // Get the name of the settings file in use if possible.
                        if (!string.IsNullOrEmpty(strSettingsName))
                        {
                            if (!OptionsManager.LoadedCharacterOptions.ContainsKey(strSettingsName))
                                return false;

                            CharacterOptionsKey = strSettingsName;
                        }

                        // Metatype information.
                        string strRaceString = xmlStatBlockBaseNode.SelectSingleNode("race/@name")?.InnerText;
                        if (!string.IsNullOrEmpty(strRaceString))
                        {
                            if (strRaceString == "Metasapient")
                                strRaceString = "A.I.";
                            foreach (XmlNode xmlMetatype in LoadData("metatypes.xml")
                                .SelectNodes("/chummer/metatypes/metatype"))
                            {
                                string strMetatypeName = xmlMetatype["name"].InnerText;
                                if (strMetatypeName == strRaceString)
                                {
                                    _strMetatype = strMetatypeName;
                                    _strMetatypeCategory = xmlMetatype["category"].InnerText;
                                    _strMetavariant = "None";

                                    XmlNode objRunNode = xmlMetatype?["run"];
                                    XmlNode objWalkNode = xmlMetatype?["walk"];
                                    XmlNode objSprintNode = xmlMetatype?["sprint"];

                                    _strMovement = xmlMetatype?["movement"]?.InnerText ?? string.Empty;
                                    _strRun = objRunNode?.InnerText ?? string.Empty;
                                    _strWalk = objWalkNode?.InnerText ?? string.Empty;
                                    _strSprint = objSprintNode?.InnerText ?? string.Empty;

                                    objRunNode = objRunNode?.Attributes?["alt"];
                                    objWalkNode = objWalkNode?.Attributes?["alt"];
                                    objSprintNode = objSprintNode?.Attributes?["alt"];
                                    _strRunAlt = objRunNode?.InnerText ?? string.Empty;
                                    _strWalkAlt = objWalkNode?.InnerText ?? string.Empty;
                                    _strSprintAlt = objSprintNode?.InnerText ?? string.Empty;
                                    break;
                                }

                                foreach (XmlNode xmlMetavariant in xmlMetatype.SelectNodes("metavariants/metavariant"))
                                {
                                    string strMetavariantName = xmlMetavariant["name"].InnerText;
                                    if (strMetavariantName == strRaceString)
                                    {
                                        _strMetatype = strMetatypeName;
                                        _strMetatypeCategory = xmlMetatype["category"].InnerText;
                                        _strMetavariant = strMetavariantName;

                                        XmlNode objRunNode = xmlMetavariant?["run"] ?? xmlMetatype?["run"];
                                        XmlNode objWalkNode = xmlMetavariant?["walk"] ?? xmlMetatype?["walk"];
                                        XmlNode objSprintNode = xmlMetavariant?["sprint"] ?? xmlMetatype?["sprint"];

                                        _strMovement = xmlMetavariant?["movement"]?.InnerText ??
                                                       xmlMetatype?["movement"]?.InnerText ?? string.Empty;
                                        _strRun = objRunNode?.InnerText ?? string.Empty;
                                        _strWalk = objWalkNode?.InnerText ?? string.Empty;
                                        _strSprint = objSprintNode?.InnerText ?? string.Empty;

                                        objRunNode = objRunNode?.Attributes?["alt"];
                                        objWalkNode = objWalkNode?.Attributes?["alt"];
                                        objSprintNode = objSprintNode?.Attributes?["alt"];
                                        _strRunAlt = objRunNode?.InnerText ?? string.Empty;
                                        _strWalkAlt = objWalkNode?.InnerText ?? string.Empty;
                                        _strSprintAlt = objSprintNode?.InnerText ?? string.Empty;
                                        break;
                                    }
                                }
                            }
                        }

                        // General character information.
                        int intAsIndex = strCharacterId.IndexOf(" as ", StringComparison.Ordinal);
                        if (intAsIndex != -1)
                        {
                            _strName = strCharacterId.Substring(0, intAsIndex);
                            _strAlias = strCharacterId.Substring(intAsIndex).TrimStart(" as ").Trim('\'');
                        }
                        else
                        {
                            _strAlias = strCharacterId;
                        }

                        XmlNode xmlPersonalNode = xmlStatBlockBaseNode.SelectSingleNode("personal");
                        if (xmlPersonalNode != null)
                        {
                            _strBackground = xmlPersonalNode["description"]?.InnerText;
                            _strHeight = xmlPersonalNode["charheight"]?.Attributes?["text"]?.InnerText;
                            _strWeight = xmlPersonalNode["charweight"]?.Attributes?["text"]?.InnerText;
                            XmlAttributeCollection xmlPersonalNodeAttributes = xmlPersonalNode.Attributes;
                            if (xmlPersonalNodeAttributes != null)
                            {
                                _strSex = xmlPersonalNodeAttributes["gender"]?.InnerText;
                                _strAge = xmlPersonalNodeAttributes["age"]?.InnerText;
                                _strHair = xmlPersonalNodeAttributes["hair"]?.InnerText;
                                _strEyes = xmlPersonalNodeAttributes["eyes"]?.InnerText;
                                _strSkin = xmlPersonalNodeAttributes["skin"]?.InnerText;
                            }
                        }

                        _strPlayerName = xmlStatBlockBaseNode.Attributes["playername"]?.InnerText;

                        foreach (XmlNode xmlImageFileNameNode in xmlStatBlockBaseNode.SelectNodes(
                            "images/image/@filename"))
                        {
                            if (dicImages.TryGetValue(xmlImageFileNameNode.InnerText, out Bitmap objOutput))
                                _lstMugshots.Add(objOutput);
                        }

                        if (_lstMugshots.Count > 0)
                            _intMainMugshotIndex = 0;

                        if (string.IsNullOrEmpty(strSettingsName))
                        {
                            string strSettingsSummary =
                                xmlStatBlockBaseNode.SelectSingleNode("settings/@summary")?.InnerText;
                            if (!string.IsNullOrEmpty(strSettingsSummary))
                            {
                                int intCharCreationSystemsIndex =
                                    strSettingsSummary.IndexOf("Character Creation Systems:", StringComparison.Ordinal);
                                int intSemicolonIndex = strSettingsSummary.IndexOf(';');
                                if (intCharCreationSystemsIndex + 28 <= intSemicolonIndex &&
                                    intCharCreationSystemsIndex != -1)
                                {
                                    string strHeroLabSettingsName = strSettingsSummary.Substring(intCharCreationSystemsIndex + 28,
                                        strSettingsSummary.IndexOf(';') - 28 - intCharCreationSystemsIndex).Trim();
                                    if (strHeroLabSettingsName == "Established Runners")
                                        strHeroLabSettingsName = "Standard";
                                    KeyValuePair<string, CharacterOptions> kvpHeroLabSettings = OptionsManager.LoadedCharacterOptions.FirstOrDefault(x => x.Value.Name == strHeroLabSettingsName);
                                    if (kvpHeroLabSettings.Value != null)
                                    {
                                        CharacterOptionsKey = kvpHeroLabSettings.Key;
                                        strSettingsName = kvpHeroLabSettings.Key;
                                    }
                                }
                            }
                        }

                        if (string.IsNullOrEmpty(strSettingsName))
                        {
                            int intKarma = Convert.ToInt32(xmlStatBlockBaseNode.SelectSingleNode("creation/bp/@total")?.InnerText, GlobalOptions.InvariantCultureInfo);

                            if (intKarma >= 100)
                            {
                                KeyValuePair<string, CharacterOptions> kvpHeroLabSettings = OptionsManager.LoadedCharacterOptions.FirstOrDefault(x => x.Value.BuiltInOption
                                                                                                                                                      && x.Value.BuildMethod == CharacterBuildMethod.Karma);
                                if (kvpHeroLabSettings.Value != null)
                                {
                                    CharacterOptionsKey = kvpHeroLabSettings.Key;
                                    strSettingsName = kvpHeroLabSettings.Key;
                                }
                            }
                            else
                            {
                                _strPriorityAttributes = ConvertPriorityString(xmlLeadsBaseNode
                                    .SelectSingleNode(
                                        "container/pick[@thing = \"priAttr\"]/field[@id = \"priOrder\"]/@value")
                                    ?.InnerText);
                                _strPrioritySpecial = ConvertPriorityString(xmlLeadsBaseNode
                                    .SelectSingleNode(
                                        "container/pick[@thing = \"priMagic\"]/field[@id = \"priOrder\"]/@value")
                                    ?.InnerText);
                                _strPriorityMetatype = ConvertPriorityString(xmlLeadsBaseNode
                                    .SelectSingleNode(
                                        "container/pick[@thing = \"priMeta\"]/field[@id = \"priOrder\"]/@value")
                                    ?.InnerText);
                                _strPriorityResources = ConvertPriorityString(xmlLeadsBaseNode
                                    .SelectSingleNode(
                                        "container/pick[@thing = \"priResourc\"]/field[@id = \"priOrder\"]/@value")
                                    ?.InnerText);
                                _strPrioritySkills = ConvertPriorityString(xmlLeadsBaseNode
                                    .SelectSingleNode(
                                        "container/pick[@thing = \"priSkill\"]/field[@id = \"priOrder\"]/@value")
                                    ?.InnerText);

                                string ConvertPriorityString(string strInput)
                                {
                                    switch (strInput)
                                    {
                                        case "1.":
                                            return "A";
                                        case "2.":
                                            return "B";
                                        case "3.":
                                            return "C";
                                        case "4.":
                                            return "D";
                                        case "5.":
                                            return "E";
                                        default:
                                            return string.Empty;
                                    }
                                }

                                if (_strPriorityAttributes == _strPrioritySpecial ||
                                    _strPriorityAttributes == _strPriorityMetatype ||
                                    _strPriorityAttributes == _strPriorityResources ||
                                    _strPriorityAttributes == _strPrioritySkills ||
                                    _strPrioritySpecial == _strPrioritySkills ||
                                    _strPrioritySpecial == _strPriorityMetatype ||
                                    _strPrioritySpecial == _strPriorityResources ||
                                    _strPriorityMetatype == _strPriorityResources ||
                                    _strPriorityMetatype == _strPrioritySpecial ||
                                    _strPriorityResources == _strPrioritySkills)
                                {
                                    KeyValuePair<string, CharacterOptions> kvpHeroLabSettings = OptionsManager.LoadedCharacterOptions.FirstOrDefault(x => x.Value.BuiltInOption
                                                                                                                                                          && x.Value.BuildMethod == CharacterBuildMethod.SumtoTen);
                                    if (kvpHeroLabSettings.Value != null)
                                    {
                                        CharacterOptionsKey = kvpHeroLabSettings.Key;
                                        strSettingsName = kvpHeroLabSettings.Key;
                                    }
                                }
                                else
                                {
                                    KeyValuePair<string, CharacterOptions> kvpHeroLabSettings = OptionsManager.LoadedCharacterOptions.FirstOrDefault(x => x.Value.BuiltInOption
                                                                                                                                                          && x.Value.BuildMethod == CharacterBuildMethod.Priority);
                                    if (kvpHeroLabSettings.Value != null)
                                    {
                                        CharacterOptionsKey = kvpHeroLabSettings.Key;
                                        strSettingsName = kvpHeroLabSettings.Key;
                                    }
                                }
                            }
                        }

                        if (string.IsNullOrEmpty(strSettingsName))
                        {
                            string strMessage = LanguageManager
                                .GetString("Message_MissingGameplayOption")
                                .Replace("{0}", CharacterOptionsKey);
                            if (Program.MainForm.ShowMessageBox(strMessage,
                                    LanguageManager.GetString("Message_MissingGameplayOption_Title",
                                        GlobalOptions.Language),
                                    MessageBoxButtons.OKCancel, MessageBoxIcon.Error) == DialogResult.OK)
                            {
                                using (frmSelectBuildMethod frmPickBP = new frmSelectBuildMethod(this, true))
                                {
                                    frmPickBP.ShowDialog(Program.MainForm);
                                    if (frmPickBP.DialogResult != DialogResult.OK)
                                        return false;
                                }
                            }
                        }

                        if (EffectiveBuildMethodUsesPriorityTables)
                        {
                            if (strRaceString == "A.I.")
                                _strPriorityTalent = "AI";
                            XmlNode xmlPriorityTalentPick =
                                xmlLeadsBaseNode.SelectSingleNode(
                                    "container/pick[starts-with(@thing, \"qu\") and @source = \"heritage\"]");
                            if (xmlPriorityTalentPick != null)
                            {
                                switch (xmlPriorityTalentPick.Attributes["thing"]?.InnerText)
                                {
                                    case "quAware":
                                        _strPriorityTalent = "Aware";
                                        break;
                                    case "quEnchanter":
                                        _strPriorityTalent = "Enchanter";
                                        break;
                                    case "quExplorer":
                                        _strPriorityTalent = "Explorer";
                                        break;
                                    case "quApprentice":
                                        _strPriorityTalent = "Apprentice";
                                        break;
                                    case "quAspectedMagician":
                                        _strPriorityTalent = "Aspected Magician";
                                        break;
                                    case "quAdept":
                                        _strPriorityTalent = "Adept";
                                        break;
                                    case "quMagician":
                                        _strPriorityTalent = "Magician";
                                        break;
                                    case "quMysticAdept":
                                        _strPriorityTalent = "Mystic Adept";
                                        break;
                                    case "quTechnoma":
                                        _strPriorityTalent = "Technomancer";
                                        break;
                                }

                                _lstPrioritySkills.Clear();
                                foreach (XmlNode xmlField in xmlPriorityTalentPick.SelectNodes("field"))
                                {
                                    string strInnerText = xmlField.InnerText;
                                    if (!string.IsNullOrEmpty(strInnerText))
                                    {
                                        _lstPrioritySkills.Add(strInnerText);
                                    }
                                }
                            }

                            using (frmPriorityMetatype frmSelectMetatype = new frmPriorityMetatype(this))
                            {
                                frmSelectMetatype.ShowDialog(Program.MainForm);
                                if (frmSelectMetatype.DialogResult == DialogResult.Cancel)
                                    return false;
                            }
                        }
                        else
                        {
                            using (frmKarmaMetatype frmSelectMetatype = new frmKarmaMetatype(this))
                            {
                                frmSelectMetatype.ShowDialog(Program.MainForm);
                                if (frmSelectMetatype.DialogResult == DialogResult.Cancel)
                                    return false;
                            }
                        }

                        XmlNode xmlKarmaNode = xmlStatBlockBaseNode.SelectSingleNode("karma");
                        if (xmlKarmaNode != null)
                        {
                            int.TryParse(xmlKarmaNode.Attributes["left"]?.InnerText, NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out _intKarma);
                            int.TryParse(xmlKarmaNode.Attributes["total"]?.InnerText, NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out _intTotalKarma);
                        }

                        XmlNode xmlReputationsNode = xmlStatBlockBaseNode.SelectSingleNode("reputations");
                        if (xmlReputationsNode != null)
                        {
                            int.TryParse(
                                xmlReputationsNode.SelectSingleNode("reputation[@name = \"Street Cred\"]/@value")
                                    .InnerText,
                                NumberStyles.Any, GlobalOptions.InvariantCultureInfo,
                                out _intStreetCred);
                            int.TryParse(
                                xmlReputationsNode.SelectSingleNode("reputation[@name = \"Notoriety\"]/@value")
                                    .InnerText,
                                NumberStyles.Any, GlobalOptions.InvariantCultureInfo,
                                out _intNotoriety);
                            int.TryParse(
                                xmlReputationsNode.SelectSingleNode("reputation[@name = \"Public Awareness\"]/@value")
                                    .InnerText, NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out _intPublicAwareness);
                        }

                        if (Created)
                        {
                            decimal.TryParse(xmlStatBlockBaseNode.SelectSingleNode("cash/@total")?.InnerText,
                                NumberStyles.Any,
                                GlobalOptions.InvariantCultureInfo, out _decNuyen);
                        }

                        /* TODO: Initiation, Submersion Grades
                        objXmlCharacter.TryGetInt32FieldQuickly("initiategrade", ref _intInitiateGrade);
                        objXmlCharacter.TryGetInt32FieldQuickly("submersiongrade", ref _intSubmersionGrade);
                        */
                        //Timekeeper.Finish("load_char_misc");
                    }

                    List<Weapon> lstWeapons = new List<Weapon>(1);
                    List<Vehicle> lstVehicles = new List<Vehicle>(1);

                    using (_ = Timekeeper.StartSyncron("load_char_quality", op_load))
                    {
                        string[] astrLevelLabels =
                        {
                            " (0)",
                            " (1)",
                            " (2)",
                            " (3)",
                            " (4)",
                            " (5)",
                            " (6)",
                            " (7)",
                            " (8)",
                            " (9)",
                            " (10)",
                            " (11)",
                            " (12)",
                            " (13)",
                            " (14)",
                            " (15)"
                        };
                        // Qualities
                        XmlDocument xmlQualitiesDocument = LoadData("qualities.xml");
                        foreach (XmlNode xmlQualityToImport in xmlStatBlockBaseNode.SelectNodes(
                            "qualities/positive/quality[traitcost/@bp != \"0\"]"))
                        {
                            string strQualityName = xmlQualityToImport.Attributes["name"]?.InnerText;
                            if (!string.IsNullOrEmpty(strQualityName))
                            {
                                int intDicepoolLabelIndex =
                                    strQualityName.LastIndexOf("dicepool", StringComparison.Ordinal);
                                if (intDicepoolLabelIndex != -1)
                                {
                                    int intCullIndex = strQualityName.LastIndexOf('(', intDicepoolLabelIndex);
                                    if (intCullIndex != -1)
                                        strQualityName = strQualityName.Substring(0, intCullIndex).Trim();
                                }

                                int intQuantity = 1;
                                for (int i = 0; i < astrLevelLabels.Length; ++i)
                                {
                                    string strLoopString = astrLevelLabels[i];
                                    if (strQualityName.EndsWith(strLoopString, StringComparison.Ordinal))
                                    {
                                        strQualityName = strQualityName.TrimEndOnce(strLoopString, true);
                                        intQuantity = i;
                                        break;
                                    }
                                }

                                string strForcedValue = string.Empty;
                                XmlNode xmlQualityDataNode =
                                    xmlQualitiesDocument.SelectSingleNode(
                                        "/chummer/qualities/quality[name = \"" + strQualityName + "\"]");
                                if (xmlQualityDataNode == null)
                                {
                                    string[] astrOriginalNameSplit = strQualityName.Split(':', StringSplitOptions.RemoveEmptyEntries);
                                    if (astrOriginalNameSplit.Length > 1)
                                    {
                                        string strName = astrOriginalNameSplit[0].Trim();
                                        xmlQualityDataNode =
                                            xmlQualitiesDocument.SelectSingleNode(
                                                "/chummer/qualities/quality[name = \"" + strName + "\"]");
                                        if (xmlQualityDataNode != null)
                                            strForcedValue = astrOriginalNameSplit[1].Trim();
                                    }
                                }

                                if (xmlQualityDataNode == null)
                                {
                                    string[] astrOriginalNameSplit = strQualityName.Split(',', StringSplitOptions.RemoveEmptyEntries);
                                    if (astrOriginalNameSplit.Length > 1)
                                    {
                                        string strName = astrOriginalNameSplit[0].Trim();
                                        xmlQualityDataNode =
                                            xmlQualitiesDocument.SelectSingleNode(
                                                "/chummer/qualities/quality[name = \"" + strName + "\"]");
                                        if (xmlQualityDataNode != null)
                                            strForcedValue = astrOriginalNameSplit[1].Trim();
                                    }
                                }

                                if (xmlQualityDataNode != null)
                                {
                                    for (int i = 0; i < intQuantity; ++i)
                                    {
                                        Quality objQuality = new Quality(this);
                                        objQuality.Create(xmlQualityDataNode, QualitySource.Selected, lstWeapons,
                                            strForcedValue);
                                        objQuality.Notes = xmlQualityToImport["description"]?.InnerText ?? string.Empty;
                                        _lstQualities.Add(objQuality);
                                    }
                                }
                            }
                        }

                        foreach (XmlNode xmlQualityToImport in xmlStatBlockBaseNode.SelectNodes(
                            "qualities/negative/quality[traitcost/@bp != \"0\"]"))
                        {
                            string strQualityName = xmlQualityToImport.Attributes["name"]?.InnerText;
                            if (!string.IsNullOrEmpty(strQualityName))
                            {
                                int intDicepoolLabelIndex =
                                    strQualityName.LastIndexOf("dicepool", StringComparison.Ordinal);
                                if (intDicepoolLabelIndex != -1)
                                {
                                    int intCullIndex = strQualityName.LastIndexOf('(', intDicepoolLabelIndex);
                                    if (intCullIndex != -1)
                                        strQualityName = strQualityName.Substring(0, intCullIndex).Trim();
                                }

                                switch (strQualityName)
                                {
                                    case "Reduced (hearing)":
                                        strQualityName = "Reduced Sense (Hearing)";
                                        break;
                                    case "Reduced (smell)":
                                        strQualityName = "Reduced Sense (Smell)";
                                        break;
                                    case "Reduced (taste)":
                                        strQualityName = "Reduced Sense (Taste)";
                                        break;
                                    case "Reduced (touch)":
                                        strQualityName = "Reduced Sense (Touch)";
                                        break;
                                    case "Reduced (sight)":
                                        strQualityName = "Reduced Sense (Sight)";
                                        break;
                                }

                                int intQuantity = 1;
                                for (int i = 0; i < astrLevelLabels.Length; ++i)
                                {
                                    string strLoopString = astrLevelLabels[i];
                                    if (strQualityName.EndsWith(strLoopString, StringComparison.Ordinal))
                                    {
                                        strQualityName = strQualityName.TrimEndOnce(strLoopString, true);
                                        intQuantity = i;
                                        break;
                                    }
                                }

                                string strForcedValue = string.Empty;
                                XmlNode xmlQualityDataNode =
                                    xmlQualitiesDocument.SelectSingleNode(
                                        "/chummer/qualities/quality[name = \"" + strQualityName + "\"]");
                                if (xmlQualityDataNode == null)
                                {
                                    string[] astrOriginalNameSplit = strQualityName.Split(':', StringSplitOptions.RemoveEmptyEntries);
                                    if (astrOriginalNameSplit.Length > 1)
                                    {
                                        string strName = astrOriginalNameSplit[0].Trim();
                                        xmlQualityDataNode =
                                            xmlQualitiesDocument.SelectSingleNode(
                                                "/chummer/qualities/quality[name = \"" + strName + "\"]");
                                        if (xmlQualityDataNode != null)
                                            strForcedValue = astrOriginalNameSplit[1].Trim();
                                    }
                                }

                                if (xmlQualityDataNode == null)
                                {
                                    string[] astrOriginalNameSplit = strQualityName.Split(',', StringSplitOptions.RemoveEmptyEntries);
                                    if (astrOriginalNameSplit.Length > 1)
                                    {
                                        string strName = astrOriginalNameSplit[0].Trim();
                                        xmlQualityDataNode =
                                            xmlQualitiesDocument.SelectSingleNode(
                                                "/chummer/qualities/quality[name = \"" + strName + "\"]");
                                        if (xmlQualityDataNode != null)
                                            strForcedValue = astrOriginalNameSplit[1].Trim();
                                    }
                                }

                                if (xmlQualityDataNode != null)
                                {
                                    for (int i = 0; i < intQuantity; ++i)
                                    {
                                        Quality objQuality = new Quality(this);
                                        objQuality.Create(xmlQualityDataNode, QualitySource.Selected, lstWeapons,
                                            strForcedValue);
                                        objQuality.Notes = xmlQualityToImport["description"]?.InnerText ?? string.Empty;
                                        _lstQualities.Add(objQuality);
                                    }
                                }
                            }
                        }

                        //Timekeeper.Finish("load_char_quality");
                    }

                    AttributeSection.LoadFromHeroLab(xmlStatBlockBaseNode, op_load);
                    using (_ = Timekeeper.StartSyncron("load_char_misc2", op_load))
                    {

                        /* TODO: Find some way to get Mystic Adept PPs from Hero Lab files
                        // Attempt to load the split MAG CharacterAttribute information for Mystic Adepts.
                        if (_blnAdeptEnabled && _blnMagicianEnabled)
                        {
                            xmlCharacterNavigator.TryGetInt32FieldQuickly("magsplitadept", ref _intMAGAdept);
                            xmlCharacterNavigator.TryGetInt32FieldQuickly("magsplitmagician", ref _intMAGMagician);
                        }
                        */

                        // Attempt to load in the character's tradition
                        if (xmlStatBlockBaseNode.SelectSingleNode("magic/tradition") != null)
                        {
                            _objTradition.LoadFromHeroLab(xmlStatBlockBaseNode.SelectSingleNode("magic/tradition"));
                        }

                        // Attempt to load Condition Monitor Progress.
                        XmlNode xmlPhysicalCMFilledNode =
                            xmlLeadsBaseNode.SelectSingleNode(
                                "usagepool[@id = \"DmgNet\" and @pickindex=\"5\"]/@quantity");
                        if (xmlPhysicalCMFilledNode != null)
                            int.TryParse(xmlPhysicalCMFilledNode.InnerText, NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out _intPhysicalCMFilled);
                        XmlNode xmlStunCMFilledNode =
                            xmlLeadsBaseNode.SelectSingleNode(
                                "usagepool[@id = \"DmgNet\" and @pickindex=\"6\"]/@quantity");
                        if (xmlStunCMFilledNode != null)
                            int.TryParse(xmlStunCMFilledNode.InnerText, NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out _intStunCMFilled);
                        //Timekeeper.Finish("load_char_misc2");
                    }

                    using (var op_load_char_skills = Timekeeper.StartSyncron("load_char_skills", op_load)) //slightly messy
                    {

                        SkillsSection.LoadFromHeroLab(xmlStatBlockBaseNode.SelectSingleNode("skills"), op_load_char_skills);

                        //Timekeeper.Finish("load_char_skills");
                    }

                    /* TODO: Add support for locations from HeroLab
                    Timekeeper.Start("load_char_loc");

                    // Locations.
                    XmlNodeList objXmlLocationList = objXmlCharacter.SelectNodes("gearlocations/gearlocation");
                    foreach (XmlNode objXmlLocation in objXmlLocationList)
                    {
                        Location objLocation = new Location(this, _lstGearLocations, string.Empty, false);
                        objLocation.Load(objXmlLocation);
                    }

                    objXmlLocationList = objXmlCharacter.SelectNodes("locations/location");
                    foreach (XmlNode objXmlLocation in objXmlLocationList)
                    {
                        Location objLocation = new Location(this, _lstGearLocations, string.Empty, false);
                        objLocation.Load(objXmlLocation);
                    }

                    objXmlLocationList = objXmlCharacter.SelectNodes("gearlocations/location");
                    foreach (XmlNode objXmlLocation in objXmlLocationList)
                    {
                        Location objLocation = new Location(this, _lstGearLocations, string.Empty, false);
                        objLocation.Load(objXmlLocation);
                    }

                    Timekeeper.Finish("load_char_loc");
                    Timekeeper.Start("load_char_abundle");

                    // Armor Bundles.
                    objXmlLocationList = objXmlCharacter.SelectNodes("armorbundles/armorbundle");
                    foreach (XmlNode objXmlLocation in objXmlLocationList)
                    {
                        Location objLocation = new Location(this, _lstArmorLocations, string.Empty, false);
                        objLocation.Load(objXmlLocation);
                    }

                    objXmlLocationList = objXmlCharacter.SelectNodes("armorlocations/armorlocation");
                    foreach (XmlNode objXmlLocation in objXmlLocationList)
                    {
                        Location objLocation = new Location(this, _lstArmorLocations, string.Empty, false);
                        objLocation.Load(objXmlLocation);
                    }

                    objXmlLocationList = objXmlCharacter.SelectNodes("armorlocations/location");
                    foreach (XmlNode objXmlLocation in objXmlLocationList)
                    {
                        Location objLocation = new Location(this, _lstArmorLocations, string.Empty, false);
                        objLocation.Load(objXmlLocation);
                    }

                    Timekeeper.Finish("load_char_abundle");
                    Timekeeper.Start("load_char_vloc");

                    // Vehicle Locations.
                    XmlNodeList objXmlVehicleLocationList = objXmlCharacter.SelectNodes("vehiclelocations/vehiclelocation");
                    foreach (XmlNode objXmlLocation in objXmlVehicleLocationList)
                    {
                        Location objLocation = new Location(this, _lstVehicleLocations, string.Empty, false);
                        objLocation.Load(objXmlLocation);
                    }

                    objXmlVehicleLocationList = objXmlCharacter.SelectNodes("vehiclelocations/location");
                    foreach (XmlNode objXmlLocation in objXmlVehicleLocationList)
                    {
                        Location objLocation = new Location(this, _lstVehicleLocations, string.Empty, false);
                        objLocation.Load(objXmlLocation);
                    }

                    Timekeeper.Finish("load_char_vloc");
                    Timekeeper.Start("load_char_wloc");

                    // Weapon Locations.
                    XmlNodeList objXmlWeaponLocationList = objXmlCharacter.SelectNodes("weaponlocations/weaponlocation");
                    foreach (XmlNode objXmlLocation in objXmlWeaponLocationList)
                    {
                        Location objLocation = new Location(this, _lstWeaponLocations, string.Empty, false);
                        objLocation.Load(objXmlLocation);
                    }

                    objXmlWeaponLocationList = objXmlCharacter.SelectNodes("weaponlocations/location");
                    foreach (XmlNode objXmlLocation in objXmlWeaponLocationList)
                    {
                        Location objLocation = new Location(this, _lstWeaponLocations, string.Empty, false);
                        objLocation.Load(objXmlLocation);
                    }

                    Timekeeper.Finish("load_char_wloc");
                    */
                    using (_ = Timekeeper.StartSyncron("load_char_contacts", op_load))
                    {

                        // Contacts.
                        foreach (XmlNode xmlContactToImport in xmlStatBlockBaseNode.SelectNodes(
                            "contacts/contact[@useradded != \"no\"]"))
                        {
                            Contact objContact = new Contact(this)
                            {
                                EntityType = ContactType.Contact
                            };
                            XmlAttributeCollection xmlImportAttributes = xmlContactToImport.Attributes;
                            objContact.Name = xmlImportAttributes["name"]?.InnerText ?? string.Empty;
                            objContact.Role = xmlImportAttributes["type"]?.InnerText ?? string.Empty;
                            objContact.Connection =
                                Convert.ToInt32(xmlImportAttributes["connection"]?.InnerText ?? "1", GlobalOptions.InvariantCultureInfo);
                            objContact.Loyalty = Convert.ToInt32(xmlImportAttributes["loyalty"]?.InnerText ?? "1", GlobalOptions.InvariantCultureInfo);
                            string strDescription = xmlContactToImport["description"]?.InnerText;
                            foreach (string strLine in strDescription.SplitNoAlloc('\n', StringSplitOptions.RemoveEmptyEntries))
                            {
                                string[] astrLineColonSplit = strLine.Split(':', StringSplitOptions.RemoveEmptyEntries);
                                switch (astrLineColonSplit[0])
                                {
                                    case "Metatype":
                                        objContact.Metatype = astrLineColonSplit[1].Trim();
                                        break;
                                    case "Sex":
                                        objContact.Sex = astrLineColonSplit[1].Trim();
                                        break;
                                    case "Age":
                                        objContact.Age = astrLineColonSplit[1].Trim();
                                        break;
                                    case "Preferred Payment Method":
                                        objContact.PreferredPayment = astrLineColonSplit[1].Trim();
                                        break;
                                    case "Hobbies/Vice":
                                        objContact.HobbiesVice = astrLineColonSplit[1].Trim();
                                        break;
                                    case "Personal Life":
                                        objContact.PersonalLife = astrLineColonSplit[1].Trim();
                                        break;
                                    case "Type":
                                        objContact.Type = astrLineColonSplit[1].Trim();
                                        break;
                                    default:
                                        objContact.Notes += strLine + Environment.NewLine;
                                        break;
                                }
                            }

                            objContact.Notes = objContact.Notes.TrimEnd(Environment.NewLine);
                            _lstContacts.Add(objContact);
                        }

                        //Timekeeper.Finish("load_char_contacts");
                    }

                    XmlDocument xmlGearDocument;
                    using (_ = Timekeeper.StartSyncron("load_char_armor", op_load))
                    {
                        // Armor.
                        xmlGearDocument = LoadData("gear.xml");
                        XmlDocument xmlArmorDocument = LoadData("armor.xml");
                        foreach (XmlNode xmlArmorToImport in xmlStatBlockBaseNode.SelectNodes(
                            "gear/armor/item[@useradded != \"no\"]"))
                        {
                            string strArmorName = xmlArmorToImport.Attributes["name"]?.InnerText;
                            if (!string.IsNullOrEmpty(strArmorName))
                            {
                                XmlNode xmlArmorData =
                                    xmlArmorDocument.SelectSingleNode(
                                        "chummer/armors/armor[name = \"" + strArmorName + "\"]");
                                if (xmlArmorData == null)
                                {
                                    string[] astrOriginalNameSplit = strArmorName.Split(':', StringSplitOptions.RemoveEmptyEntries);
                                    if (astrOriginalNameSplit.Length > 1)
                                    {
                                        string strName = astrOriginalNameSplit[0].Trim();
                                        xmlArmorData =
                                            xmlArmorDocument.SelectSingleNode(
                                                "/chummer/armors/armor[name = \"" + strName + "\"]");
                                    }

                                    if (xmlArmorData == null)
                                    {
                                        astrOriginalNameSplit = strArmorName.Split(',', StringSplitOptions.RemoveEmptyEntries);
                                        if (astrOriginalNameSplit.Length > 1)
                                        {
                                            string strName = astrOriginalNameSplit[0].Trim();
                                            xmlArmorData =
                                                xmlArmorDocument.SelectSingleNode(
                                                    "/chummer/armors/armor[name = \"" + strName + "\"]");
                                        }
                                    }
                                }

                                if (xmlArmorData != null)
                                {
                                    Armor objArmor = new Armor(this);
                                    objArmor.Create(xmlArmorData,
                                        Convert.ToInt32(xmlArmorToImport.Attributes["rating"]?.InnerText, GlobalOptions.InvariantCultureInfo), lstWeapons);
                                    objArmor.Notes = xmlArmorToImport["description"]?.InnerText;
                                    _lstArmor.Add(objArmor);

                                    foreach (string strName in HeroLabPluginNodeNames)
                                    {
                                        foreach (XmlNode xmlArmorModToImport in xmlArmorToImport.SelectNodes(
                                            strName + "/item[@useradded != \"no\"]"))
                                        {
                                            string strArmorModName = xmlArmorModToImport.Attributes["name"]?.InnerText;
                                            if (!string.IsNullOrEmpty(strArmorModName))
                                            {
                                                XmlNode xmlArmorModData =
                                                    xmlArmorDocument.SelectSingleNode(
                                                        "chummer/mods/mod[name = \"" + strArmorModName + "\"]");
                                                if (xmlArmorModData != null)
                                                {
                                                    ArmorMod objArmorMod = new ArmorMod(this);
                                                    objArmorMod.Create(xmlArmorModData,
                                                        Convert.ToInt32(xmlArmorModToImport.Attributes["rating"]
                                                            ?.InnerText, GlobalOptions.InvariantCultureInfo),
                                                        lstWeapons);
                                                    objArmorMod.Notes = xmlArmorModToImport["description"]?.InnerText;
                                                    objArmorMod.Parent = objArmor;
                                                    objArmor.ArmorMods.Add(objArmorMod);

                                                    foreach (string strPluginNodeName in HeroLabPluginNodeNames)
                                                    {
                                                        foreach (XmlNode xmlPluginToAdd in xmlArmorModToImport
                                                            .SelectNodes(
                                                                strPluginNodeName + "/item[@useradded != \"no\"]"))
                                                        {
                                                            Gear objPlugin = new Gear(this);
                                                            if (objPlugin.ImportHeroLabGear(xmlPluginToAdd,
                                                                xmlArmorModData,
                                                                lstWeapons))
                                                                objArmorMod.Gear.Add(objPlugin);
                                                        }

                                                        foreach (XmlNode xmlPluginToAdd in xmlArmorModToImport
                                                            .SelectNodes(
                                                                strPluginNodeName + "/item[@useradded = \"no\"]"))
                                                        {
                                                            string strGearName = xmlPluginToAdd.Attributes["name"]
                                                                ?.InnerText;
                                                            if (!string.IsNullOrEmpty(strGearName))
                                                            {
                                                                Gear objPlugin = objArmorMod.Gear.FirstOrDefault(x =>
                                                                    x.IncludedInParent &&
                                                                    (x.Name.Contains(strGearName) ||
                                                                     strGearName.Contains(x.Name)));
                                                                if (objPlugin != null)
                                                                {
                                                                    objPlugin.Quantity =
                                                                        Convert.ToDecimal(
                                                                            xmlPluginToAdd.Attributes["quantity"]
                                                                                ?.InnerText ??
                                                                            "1", GlobalOptions.InvariantCultureInfo);
                                                                    objPlugin.Notes = xmlPluginToAdd["description"]
                                                                        ?.InnerText;
                                                                    objPlugin.ProcessHeroLabGearPlugins(xmlPluginToAdd,
                                                                        lstWeapons);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    Gear objPlugin = new Gear(this);
                                                    if (objPlugin.ImportHeroLabGear(xmlArmorModToImport, xmlArmorData,
                                                        lstWeapons))
                                                        objArmor.Gear.Add(objPlugin);
                                                }
                                            }
                                        }

                                        foreach (XmlNode xmlArmorModToImport in xmlArmorToImport.SelectNodes(
                                            strName + "/item[@useradded = \"no\"]"))
                                        {
                                            string strArmorModName = xmlArmorModToImport.Attributes["name"]?.InnerText;
                                            if (!string.IsNullOrEmpty(strArmorModName))
                                            {
                                                ArmorMod objArmorMod = objArmor.ArmorMods.FirstOrDefault(x =>
                                                    x.IncludedInArmor &&
                                                    (x.Name.Contains(strArmorModName) ||
                                                     strArmorModName.Contains(x.Name)));
                                                if (objArmorMod != null)
                                                {
                                                    objArmorMod.Notes = xmlArmorModToImport["description"]?.InnerText;
                                                    foreach (string strPluginNodeName in HeroLabPluginNodeNames)
                                                    {
                                                        foreach (XmlNode xmlPluginToAdd in xmlArmorModToImport
                                                            .SelectNodes(
                                                                strPluginNodeName + "/item[@useradded != \"no\"]"))
                                                        {
                                                            Gear objPlugin = new Gear(this);
                                                            if (objPlugin.ImportHeroLabGear(xmlPluginToAdd,
                                                                objArmorMod.GetNode(), lstWeapons))
                                                                objArmorMod.Gear.Add(objPlugin);
                                                        }

                                                        foreach (XmlNode xmlPluginToAdd in xmlArmorModToImport
                                                            .SelectNodes(
                                                                strPluginNodeName + "/item[@useradded = \"no\"]"))
                                                        {
                                                            string strGearName = xmlPluginToAdd.Attributes["name"]
                                                                ?.InnerText;
                                                            if (!string.IsNullOrEmpty(strGearName))
                                                            {
                                                                Gear objPlugin = objArmorMod.Gear.FirstOrDefault(x =>
                                                                    x.IncludedInParent &&
                                                                    (x.Name.Contains(strGearName) ||
                                                                     strGearName.Contains(x.Name)));
                                                                if (objPlugin != null)
                                                                {
                                                                    objPlugin.Quantity =
                                                                        Convert.ToDecimal(
                                                                            xmlPluginToAdd.Attributes["quantity"]
                                                                                ?.InnerText ??
                                                                            "1", GlobalOptions.InvariantCultureInfo);
                                                                    objPlugin.Notes = xmlPluginToAdd["description"]
                                                                        ?.InnerText;
                                                                    objPlugin.ProcessHeroLabGearPlugins(xmlPluginToAdd,
                                                                        lstWeapons);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    Gear objPlugin = objArmor.Gear.FirstOrDefault(x =>
                                                        x.IncludedInParent &&
                                                        (x.Name.Contains(strArmorModName) ||
                                                         strArmorModName.Contains(x.Name)));
                                                    if (objPlugin != null)
                                                    {
                                                        objPlugin.Quantity = Convert.ToDecimal(
                                                            xmlArmorModToImport.Attributes["quantity"]?.InnerText ??
                                                            "1",
                                                            GlobalOptions.InvariantCultureInfo);
                                                        objPlugin.Notes = xmlArmorModToImport["description"]?.InnerText;
                                                        objPlugin.ProcessHeroLabGearPlugins(xmlArmorModToImport,
                                                            lstWeapons);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        //Timekeeper.Finish("load_char_armor");
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_weapons", op_load))
                    {
                        // Weapons.
                        foreach (XmlNode xmlWeaponToImport in xmlStatBlockBaseNode.SelectNodes(
                            "gear/weapons/item[@useradded != \"no\"]"))
                        {
                            Weapon objWeapon = new Weapon(this);
                            if (objWeapon.ImportHeroLabWeapon(xmlWeaponToImport, lstWeapons))
                                _lstWeapons.Add(objWeapon);
                        }

                        foreach (XmlNode xmlPluginToAdd in xmlStatBlockBaseNode.SelectNodes(
                            "gear/weapons/item[@useradded = \"no\"]"))
                        {
                            string strName = xmlPluginToAdd.Attributes["name"]?.InnerText;
                            if (!string.IsNullOrEmpty(strName))
                            {
                                Weapon objWeapon = _lstWeapons.FirstOrDefault(x =>
                                    !string.IsNullOrEmpty(x.ParentID) &&
                                    (x.Name.Contains(strName) || strName.Contains(x.Name)));
                                if (objWeapon != null)
                                {
                                    objWeapon.Notes = xmlPluginToAdd["description"]?.InnerText;
                                    objWeapon.ProcessHeroLabWeaponPlugins(xmlPluginToAdd, lstWeapons);
                                }
                            }
                        }

                        //Timekeeper.Finish("load_char_weapons");
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_ware", op_load))
                    {
                        // Cyberware/Bioware.
                        foreach (XmlNode xmlCyberwareToImport in xmlStatBlockBaseNode.SelectNodes(
                            "gear/augmentations/cyberware/item[@useradded != \"no\"]"))
                        {
                            Cyberware objCyberware = new Cyberware(this);
                            if (objCyberware.ImportHeroLabCyberware(xmlCyberwareToImport, null, lstWeapons,
                                lstVehicles))
                                _lstCyberware.Add(objCyberware);
                        }

                        foreach (XmlNode xmlPluginToAdd in xmlStatBlockBaseNode.SelectNodes(
                            "gear/augmentations/cyberware/item[@useradded = \"no\"]"))
                        {
                            string strName = xmlPluginToAdd.Attributes["name"]?.InnerText;
                            if (!string.IsNullOrEmpty(strName))
                            {
                                Cyberware objPlugin = _lstCyberware.FirstOrDefault(x =>
                                    !string.IsNullOrEmpty(x.ParentID) &&
                                    (x.Name.Contains(strName) || strName.Contains(x.Name)));
                                if (objPlugin != null)
                                {
                                    objPlugin.Notes = xmlPluginToAdd["description"]?.InnerText;
                                    objPlugin.ProcessHeroLabCyberwarePlugins(xmlPluginToAdd, objPlugin.Grade,
                                        lstWeapons,
                                        lstVehicles);
                                }
                            }
                        }

                        foreach (XmlNode xmlCyberwareToImport in xmlStatBlockBaseNode.SelectNodes(
                            "gear/augmentations/bioware/item[@useradded != \"no\"]"))
                        {
                            Cyberware objCyberware = new Cyberware(this);
                            if (objCyberware.ImportHeroLabCyberware(xmlCyberwareToImport, null, lstWeapons,
                                lstVehicles))
                                _lstCyberware.Add(objCyberware);
                        }

                        foreach (XmlNode xmlPluginToAdd in xmlStatBlockBaseNode.SelectNodes(
                            "gear/augmentations/bioware/item[@useradded = \"no\"]"))
                        {
                            string strName = xmlPluginToAdd.Attributes["name"]?.InnerText;
                            if (!string.IsNullOrEmpty(strName))
                            {
                                Cyberware objPlugin = _lstCyberware.FirstOrDefault(x =>
                                    !string.IsNullOrEmpty(x.ParentID) &&
                                    (x.Name.Contains(strName) || strName.Contains(x.Name)));
                                if (objPlugin != null)
                                {
                                    objPlugin.Notes = xmlPluginToAdd["description"]?.InnerText;
                                    objPlugin.ProcessHeroLabCyberwarePlugins(xmlPluginToAdd, objPlugin.Grade,
                                        lstWeapons,
                                        lstVehicles);
                                }
                            }
                        }

                        //Timekeeper.Finish("load_char_ware");
                    }

                    XmlNodeList xmlNodeList;
                    using (_ = Timekeeper.StartSyncron("load_char_spells", op_load))
                    {
                        // Spells.
                        xmlNodeList = xmlStatBlockBaseNode.SelectNodes("magic/spells/spell");
                        XmlDocument xmlSpellDocument = LoadData("spells.xml");
                        foreach (XmlNode xmlHeroLabSpell in xmlNodeList)
                        {
                            string strSpellName = xmlHeroLabSpell.Attributes["name"]?.InnerText;
                            if (!string.IsNullOrEmpty(strSpellName))
                            {
                                bool blnIsLimited = strSpellName.EndsWith(" (limited)", StringComparison.Ordinal);
                                if (blnIsLimited)
                                    strSpellName = strSpellName.TrimEndOnce(" (limited)");
                                string strForcedValue = string.Empty;
                                switch (strSpellName)
                                {
                                    case "Increase Body":
                                        strForcedValue = "BOD";
                                        strSpellName = "Increase [Attribute]";
                                        break;
                                    case "Increase Agility":
                                        strForcedValue = "AGI";
                                        strSpellName = "Increase [Attribute]";
                                        break;
                                    case "Increase Reaction":
                                        strForcedValue = "REA";
                                        strSpellName = "Increase [Attribute]";
                                        break;
                                    case "Increase Strength":
                                        strForcedValue = "STR";
                                        strSpellName = "Increase [Attribute]";
                                        break;
                                    case "Increase Charisma":
                                        strForcedValue = "CHA";
                                        strSpellName = "Increase [Attribute]";
                                        break;
                                    case "Increase Intuition":
                                        strForcedValue = "INT";
                                        strSpellName = "Increase [Attribute]";
                                        break;
                                    case "Increase Logic":
                                        strForcedValue = "LOG";
                                        strSpellName = "Increase [Attribute]";
                                        break;
                                    case "Increase Willpower":
                                        strForcedValue = "WIL";
                                        strSpellName = "Increase [Attribute]";
                                        break;
                                    case "Decrease Body":
                                        strForcedValue = "BOD";
                                        strSpellName = "Decrease [Attribute]";
                                        break;
                                    case "Decrease Agility":
                                        strForcedValue = "AGI";
                                        strSpellName = "Decrease [Attribute]";
                                        break;
                                    case "Decrease Reaction":
                                        strForcedValue = "REA";
                                        strSpellName = "Decrease [Attribute]";
                                        break;
                                    case "Decrease Strength":
                                        strForcedValue = "STR";
                                        strSpellName = "Decrease [Attribute]";
                                        break;
                                    case "Decrease Charisma":
                                        strForcedValue = "CHA";
                                        strSpellName = "Decrease [Attribute]";
                                        break;
                                    case "Decrease Intuition":
                                        strForcedValue = "INT";
                                        strSpellName = "Decrease [Attribute]";
                                        break;
                                    case "Decrease Logic":
                                        strForcedValue = "LOG";
                                        strSpellName = "Decrease [Attribute]";
                                        break;
                                    case "Decrease Willpower":
                                        strForcedValue = "WIL";
                                        strSpellName = "Decrease [Attribute]";
                                        break;
                                }

                                if (strSpellName.StartsWith("Detect ", StringComparison.Ordinal) &&
                                    strSpellName != "Detect Life" &&
                                    strSpellName != "Detect Life, Extended" &&
                                    strSpellName != "Detect Magic" &&
                                    strSpellName != "Detect Magic, Extended" &&
                                    strSpellName != "Detect Enemies" &&
                                    strSpellName != "Detect Enemies, Extended" &&
                                    strSpellName != "Detect Individual" &&
                                    strSpellName != "Detect Life, Extended")
                                {
                                    strForcedValue = strSpellName.TrimStartOnce("Clean ").TrimEndOnce(", Extended");
                                    if (xmlHeroLabSpell.Attributes["type"]?.InnerText == "Physical")
                                        strSpellName = "Detect [Object]";
                                    else if (strSpellName.EndsWith(", Extended", StringComparison.Ordinal))
                                        strSpellName = "Detect [Life Form], Extended";
                                    else
                                        strSpellName = "Detect [Life Form]";
                                }
                                else if (strSpellName.StartsWith("Corrode ", StringComparison.Ordinal))
                                {
                                    strForcedValue = strSpellName.TrimStartOnce("Corrode ");
                                    strSpellName = "Corrode [Object]";
                                }
                                else if (strSpellName.StartsWith("Melt ", StringComparison.Ordinal))
                                {
                                    strForcedValue = strSpellName.TrimStartOnce("Melt ");
                                    strSpellName = "Melt [Object]";
                                }
                                else if (strSpellName.StartsWith("Sludge ", StringComparison.Ordinal))
                                {
                                    strForcedValue = strSpellName.TrimStartOnce("Sludge ");
                                    strSpellName = "Sludge [Object]";
                                }
                                else if (strSpellName.StartsWith("Disrupt ", StringComparison.Ordinal))
                                {
                                    strForcedValue = strSpellName.TrimStartOnce("Disrupt ");
                                    strSpellName = "Disrupt [Object]";
                                }
                                else if (strSpellName.StartsWith("Destroy ", StringComparison.Ordinal))
                                {
                                    strForcedValue = strSpellName.TrimStartOnce("Destroy ");
                                    strSpellName = xmlHeroLabSpell.Attributes["type"]?.InnerText == "Physical"
                                        ? "Destroy [Vehicle]"
                                        : "Destroy [Free Spirit]";
                                }
                                else if (strSpellName.StartsWith("Insecticide ", StringComparison.Ordinal))
                                {
                                    strForcedValue = strSpellName.TrimStartOnce("Insecticide ");
                                    strSpellName = "Insecticide [Insect Spirit]";
                                }
                                else if (strSpellName.StartsWith("One Less ", StringComparison.Ordinal))
                                {
                                    strForcedValue = strSpellName.TrimStartOnce("One Less ");
                                    strSpellName = "One Less [Metatype/Species]";
                                }
                                else if (strSpellName.StartsWith("Slay ", StringComparison.Ordinal))
                                {
                                    strForcedValue = strSpellName.TrimStartOnce("Slay ");
                                    strSpellName = "Slay [Metatype/Species]";
                                }
                                else if (strSpellName.StartsWith("Slaughter ", StringComparison.Ordinal))
                                {
                                    strForcedValue = strSpellName.TrimStartOnce("Slaughter ");
                                    strSpellName = "Slaughter [Metatype/Species]";
                                }
                                else if (strSpellName.StartsWith("Ram ", StringComparison.Ordinal))
                                {
                                    strForcedValue = strSpellName.TrimStartOnce("Ram ");
                                    strSpellName = "Ram [Object]";
                                }
                                else if (strSpellName.StartsWith("Wreck ", StringComparison.Ordinal))
                                {
                                    strForcedValue = strSpellName.TrimStartOnce("Wreck ");
                                    strSpellName = "Wreck [Object]";
                                }
                                else if (strSpellName.StartsWith("Demolish ", StringComparison.Ordinal))
                                {
                                    strForcedValue = strSpellName.TrimStartOnce("Demolish ");
                                    strSpellName = "Demolish [Object]";
                                }
                                else if (strSpellName.EndsWith(" Cryptesthesia", StringComparison.Ordinal))
                                {
                                    strForcedValue = strSpellName.TrimEndOnce(" Cryptesthesia");
                                    strSpellName = "[Sense] Cryptesthesia";
                                }
                                else if (strSpellName.EndsWith(" Removal", StringComparison.Ordinal))
                                {
                                    strForcedValue = strSpellName.TrimStartOnce("Mass ").TrimEndOnce(" Removal");
                                    strSpellName = strSpellName.StartsWith("Mass ", StringComparison.Ordinal)
                                        ? "Mass [Sense] Removal"
                                        : "[Sense] Removal";
                                }
                                else if (strSpellName.StartsWith("Alleviate ", StringComparison.Ordinal) && strSpellName != "Alleviate Addiction")
                                {
                                    strForcedValue = strSpellName.TrimStartOnce("Alleviate ");
                                    strSpellName = "Alleviate [Allergy]";
                                }
                                else if (strSpellName.StartsWith("Clean ", StringComparison.Ordinal))
                                {
                                    strForcedValue = strSpellName.TrimStartOnce("Clean ");
                                    strSpellName = "Clean [Element]";
                                }
                                else if (strSpellName.EndsWith(" Grenade", StringComparison.Ordinal))
                                {
                                    strForcedValue = strSpellName.TrimEndOnce(" Grenade");
                                    strSpellName = "[Element] Grenade";
                                }
                                else if (strSpellName.EndsWith(" Aura", StringComparison.Ordinal))
                                {
                                    strForcedValue = strSpellName.TrimEndOnce(" Aura");
                                    strSpellName = "[Element] Aura";
                                }
                                else if (strSpellName != "Napalm Wall" && strSpellName.EndsWith(" Wall", StringComparison.Ordinal))
                                {
                                    strForcedValue = strSpellName.TrimEndOnce(" Wall");
                                    strSpellName = "[Element] Wall";
                                }
                                else if (strSpellName.StartsWith("Shape ", StringComparison.Ordinal))
                                {
                                    strForcedValue = strSpellName.TrimStartOnce("Shape ");
                                    strSpellName = "Shape [Material]";
                                }
                                else if (strSpellName.EndsWith(" Form", StringComparison.Ordinal))
                                {
                                    strForcedValue = strSpellName.TrimEndOnce(" Form");
                                    strSpellName = "[Critter] Form";
                                }
                                else if (strSpellName.StartsWith("Calling ", StringComparison.Ordinal))
                                {
                                    strForcedValue = strSpellName.TrimStartOnce("Calling ");
                                    strSpellName = "Calling [Spirit Type]";
                                }
                                else if (strSpellName != "Symbolic Link" && strSpellName.EndsWith(" Link", StringComparison.Ordinal))
                                {
                                    strForcedValue = strSpellName.TrimEndOnce(" Link");
                                    strSpellName = "[Sense] Link";
                                }

                                string strSpellCategory = xmlHeroLabSpell.Attributes["category"]?.InnerText;
                                XmlNode xmlSpellData = xmlSpellDocument.SelectSingleNode(
                                    "chummer/spells/spell[category = \"" + strSpellCategory + "\" and name = \"" +
                                    strSpellName + "\"]");
                                if (xmlSpellData == null)
                                {
                                    string[] astrOriginalNameSplit = strSpellName.Split(':', StringSplitOptions.RemoveEmptyEntries);
                                    if (astrOriginalNameSplit.Length > 1)
                                    {
                                        string strName = astrOriginalNameSplit[0].Trim();
                                        xmlSpellData = xmlSpellDocument.SelectSingleNode(
                                            "/chummer/spells/spell[category = \"" + strSpellCategory +
                                            "\" and name = \"" +
                                            strName + "\"]");
                                    }

                                    if (xmlSpellData == null)
                                    {
                                        astrOriginalNameSplit = strSpellName.Split(',', StringSplitOptions.RemoveEmptyEntries);
                                        if (astrOriginalNameSplit.Length > 1)
                                        {
                                            string strName = astrOriginalNameSplit[0].Trim();
                                            xmlSpellData = xmlSpellDocument.SelectSingleNode(
                                                "/chummer/spells/spell[category = \"" + strSpellCategory +
                                                "\" and name = \"" +
                                                strName + "\"]");
                                        }
                                    }
                                }

                                if (xmlSpellData != null)
                                {
                                    Spell objSpell = new Spell(this);
                                    objSpell.Create(xmlSpellData, strForcedValue, blnIsLimited);
                                    objSpell.Notes = xmlHeroLabSpell["description"]?.InnerText;
                                    _lstSpells.Add(objSpell);
                                }
                            }
                        }

                        //Timekeeper.Finish("load_char_spells");
                    }

                    using (var _ = Timekeeper.StartSyncron("load_char_powers", op_load))
                    {
                        // Powers.
                        xmlNodeList = xmlStatBlockBaseNode.SelectNodes("magic/adeptpowers/adeptpower");
                        XmlDocument xmlPowersDocument = LoadData("powers.xml");
                        foreach (XmlNode xmlHeroLabPower in xmlNodeList)
                        {
                            string strPowerName = xmlHeroLabPower.Attributes["name"]?.InnerText;
                            if (!string.IsNullOrEmpty(strPowerName))
                            {
                                int intRating = 1;
                                string strForcedValue = string.Empty;
                                XmlNode xmlPowerData =
                                    xmlPowersDocument.SelectSingleNode(
                                        "chummer/powers/power[contains(name, \"" + strPowerName + "\")]");
                                if (xmlPowerData == null)
                                {
                                    string[] astrOriginalNameSplit = strPowerName.Split(':', StringSplitOptions.RemoveEmptyEntries);
                                    if (astrOriginalNameSplit.Length > 1)
                                    {
                                        string strName = astrOriginalNameSplit[0].Trim();
                                        xmlPowerData =
                                            xmlPowersDocument.SelectSingleNode(
                                                "/chummer/powers/power[contains(name, \"" + strName + "\")]");

                                        strForcedValue = astrOriginalNameSplit[1].Trim();
                                        int intForcedValueParenthesesStart = strForcedValue.IndexOf('(');
                                        if (intForcedValueParenthesesStart != -1)
                                            strForcedValue =
                                                strForcedValue.Substring(0, intForcedValueParenthesesStart);
                                    }

                                    if (xmlPowerData == null)
                                    {
                                        astrOriginalNameSplit = strPowerName.Split('(', StringSplitOptions.RemoveEmptyEntries);
                                        if (astrOriginalNameSplit.Length > 1)
                                        {
                                            string strName = astrOriginalNameSplit[0].Trim();
                                            xmlPowerData =
                                                xmlPowersDocument.SelectSingleNode(
                                                    "/chummer/powers/power[contains(name, \"" + strName + "\")]");

                                            string strSecondPart = astrOriginalNameSplit[1].Trim();
                                            int intSecondPartParenthesesEnd = strSecondPart.IndexOf(')');
                                            if (intSecondPartParenthesesEnd != -1)
                                            {
                                                if (!int.TryParse(
                                                    strSecondPart.Substring(0, intSecondPartParenthesesEnd),
                                                    out intRating))
                                                    intRating = 1;
                                            }

                                            astrOriginalNameSplit = strSecondPart.Split(':', StringSplitOptions.RemoveEmptyEntries);
                                            if (astrOriginalNameSplit.Length >= 2)
                                            {
                                                strForcedValue = astrOriginalNameSplit[1].Trim();
                                                int intForcedValueParenthesesStart = strForcedValue.IndexOf('(');
                                                if (intForcedValueParenthesesStart != -1)
                                                    strForcedValue =
                                                        strForcedValue.Substring(0, intForcedValueParenthesesStart);
                                            }
                                        }
                                    }
                                }

                                if (xmlPowerData != null)
                                {
                                    Power objPower = new Power(this) {Extra = strForcedValue};
                                    objPower.Create(xmlPowerData, intRating);
                                    objPower.Notes = xmlHeroLabPower["description"]?.InnerText;
                                    _lstPowers.Add(objPower);
                                }
                            }
                        }

                        //Timekeeper.Finish("load_char_powers");
                    }

                    /* TODO: Spirit/Sprite Importing
                    Timekeeper.Start("load_char_spirits");

                    // Spirits/Sprites.
                    foreach (XPathNavigator xmlSpirit in xmlCharacterNavigator.Select("spirits/spirit"))
                    {
                        Spirit objSpirit = new Spirit(this);
                        objSpirit.Load(xmlSpirit);
                        _lstSpirits.Add(objSpirit);
                    }

                    Timekeeper.Finish("load_char_spirits");
                    */
                    using (_ = Timekeeper.StartSyncron("load_char_complex", op_load))
                    {
                        // Complex Forms/Technomancer Programs.
                        string strComplexFormsLine =
                            lstTextStatBlockLines?.FirstOrDefault(x => x.StartsWith("Complex Forms:", StringComparison.Ordinal));
                        if (!string.IsNullOrEmpty(strComplexFormsLine))
                        {
                            XmlDocument xmlComplexFormsDocument = LoadData("complexforms.xml");

                            string[] astrComplexForms =
                                strComplexFormsLine.TrimStartOnce("Complex Forms:").Trim().Split(',', StringSplitOptions.RemoveEmptyEntries);
                            foreach (string strComplexFormEntry in astrComplexForms)
                            {
                                string strComplexFormName = strComplexFormEntry.Trim();
                                string strForcedValue = string.Empty;
                                switch (strComplexFormName)
                                {
                                    case "Diffusion of Attack":
                                        strComplexFormName = "Diffusion of [Matrix Attribute]";
                                        strForcedValue = "Attack";
                                        break;
                                    case "Diffusion of Sleaze":
                                        strComplexFormName = "Diffusion of [Matrix Attribute]";
                                        strForcedValue = "Sleaze";
                                        break;
                                    case "Diffusion of Data Processing":
                                        strComplexFormName = "Diffusion of [Matrix Attribute]";
                                        strForcedValue = "Data Processing";
                                        break;
                                    case "Diffusion of Firewall":
                                        strComplexFormName = "Diffusion of [Matrix Attribute]";
                                        strForcedValue = "Firewall";
                                        break;
                                    case "Infusion of Attack":
                                        strComplexFormName = "Infusion of [Matrix Attribute]";
                                        strForcedValue = "Attack";
                                        break;
                                    case "Infusion of Sleaze":
                                        strComplexFormName = "Infusion of [Matrix Attribute]";
                                        strForcedValue = "Sleaze";
                                        break;
                                    case "Infusion of Data Processing":
                                        strComplexFormName = "Infusion of [Matrix Attribute]";
                                        strForcedValue = "Data Processing";
                                        break;
                                    case "Infusion of Firewall":
                                        strComplexFormName = "Infusion of [Matrix Attribute]";
                                        strForcedValue = "Firewall";
                                        break;
                                }

                                XmlNode xmlComplexFormData =
                                    xmlComplexFormsDocument.SelectSingleNode(
                                        "chummer/complexforms/complexform[name = \"" + strComplexFormName + "\"]");
                                if (xmlComplexFormData == null)
                                {
                                    string[] astrOriginalNameSplit = strComplexFormName.Split(':', StringSplitOptions.RemoveEmptyEntries);
                                    if (astrOriginalNameSplit.Length > 1)
                                    {
                                        string strName = astrOriginalNameSplit[0].Trim();
                                        xmlComplexFormData =
                                            xmlComplexFormsDocument.SelectSingleNode(
                                                "/chummer/complexforms/complexform[name = \"" + strName + "\"]");
                                    }

                                    if (xmlComplexFormData == null)
                                    {
                                        astrOriginalNameSplit = strComplexFormName.Split(',', StringSplitOptions.RemoveEmptyEntries);
                                        if (astrOriginalNameSplit.Length > 1)
                                        {
                                            string strName = astrOriginalNameSplit[0].Trim();
                                            xmlComplexFormData =
                                                xmlComplexFormsDocument.SelectSingleNode(
                                                    "/chummer/complexforms/complexform[name = \"" + strName + "\"]");
                                        }
                                    }
                                }

                                if (xmlComplexFormData != null)
                                {
                                    ComplexForm objComplexForm = new ComplexForm(this);
                                    objComplexForm.Create(xmlComplexFormData, strForcedValue);
                                    _lstComplexForms.Add(objComplexForm);
                                }
                            }
                        }

                        //Timekeeper.Finish("load_char_complex");
                    }

                    /* TODO: AI Advanced Program Importing
                    Timekeeper.Start("load_char_aiprogram");

                    // AI Advanced Programs.
                    objXmlNodeList = objXmlCharacter.SelectNodes("aiprograms/aiprogram");
                    foreach (XmlNode xmlHeroLabProgram in xmlNodeList)
                    {
                        AIProgram objProgram = new AIProgram(this);
                        objProgram.Load(xmlHeroLabProgram);
                        _lstAIPrograms.Add(objProgram);
                    }

                    Timekeeper.Finish("load_char_aiprogram");
                    */
                    /* TODO: Martial Arts import, which are saved in TXT and HTML statblocks but not in XML statblock
                    Timekeeper.Start("load_char_marts");

                    // Martial Arts.
                    xmlNodeList = objXmlCharacter.SelectNodes("martialarts/martialart");
                    foreach (XmlNode xmlHeroLabArt in xmlNodeList)
                    {
                        MartialArt objMartialArt = new MartialArt(this);
                        objMartialArt.Load(xmlHeroLabArt);
                        _lstMartialArts.Add(objMartialArt);
                    }

                    Timekeeper.Finish("load_char_marts");
                    */
                    using (_ = Timekeeper.StartSyncron("load_char_lifestyle", op_load))
                    {
                        // Lifestyles.
                        XmlNode xmlFakeSINDataNode =
                            xmlGearDocument.SelectSingleNode("/chummer/gears/gear[name = 'Fake SIN']");
                        XmlNode xmlFakeLicenseDataNode =
                            xmlGearDocument.SelectSingleNode("/chummer/gears/gear[name = 'Fake License']");
                        xmlNodeList = xmlStatBlockBaseNode.SelectNodes("identities/identity");
                        foreach (XmlNode xmlHeroLabIdentity in xmlNodeList)
                        {
                            string strIdentityName = xmlHeroLabIdentity.Attributes["name"]?.InnerText;
                            int intIdentityNameParenthesesStart = strIdentityName.IndexOf('(');
                            if (intIdentityNameParenthesesStart != -1)
                                strIdentityName = strIdentityName.Substring(0, intIdentityNameParenthesesStart);
                            XmlNode xmlHeroLabFakeSINNode =
                                xmlHeroLabIdentity.SelectSingleNode("license[@name = \"Fake SIN\"]");
                            if (xmlHeroLabFakeSINNode != null)
                            {
                                Gear objFakeSIN = new Gear(this);
                                objFakeSIN.Create(xmlFakeSINDataNode,
                                    Convert.ToInt32(xmlHeroLabFakeSINNode.Attributes["rating"]?.InnerText, GlobalOptions.InvariantCultureInfo), lstWeapons,
                                    strIdentityName);
                                foreach (XmlNode xmlHeroLabFakeLicenseNode in xmlHeroLabIdentity.SelectNodes(
                                    "license[@name = \"Fake License\"]"))
                                {
                                    Gear objFakeLicense = new Gear(this);
                                    objFakeLicense.Create(xmlFakeLicenseDataNode,
                                        Convert.ToInt32(xmlHeroLabFakeLicenseNode.Attributes["rating"]?.InnerText, GlobalOptions.InvariantCultureInfo),
                                        lstWeapons,
                                        xmlHeroLabFakeLicenseNode.Attributes["for"]?.InnerText);
                                    objFakeLicense.Parent = objFakeSIN;
                                    objFakeSIN.Children.Add(objFakeLicense);
                                }

                                _lstGear.Add(objFakeSIN);
                            }

                            XmlNode xmlHeroLabLifestyleNode = xmlHeroLabIdentity.SelectSingleNode("lifestyle");
                            if (xmlHeroLabLifestyleNode != null)
                            {
                                string strLifestyleType = xmlHeroLabLifestyleNode.Attributes["name"]?.InnerText
                                    .TrimEndOnce(" Lifestyle");

                                XmlNode xmlLifestyleDataNode = LoadData("lifestyles.xml")
                                    .SelectSingleNode("/chummer/lifestyles/lifestyle[name = \"" + strLifestyleType +
                                                      "\"]");

                                if (xmlLifestyleDataNode != null)
                                {
                                    Lifestyle objLifestyle = new Lifestyle(this);
                                    objLifestyle.Create(xmlLifestyleDataNode);
                                    if (int.TryParse(xmlHeroLabLifestyleNode.Attributes["months"]?.InnerText,
                                        out int intMonths))
                                    {
                                        objLifestyle.Increments = intMonths;
                                    }

                                    _lstLifestyles.Add(objLifestyle);
                                }
                            }
                        }

                        //Timekeeper.Finish("load_char_lifestyle");
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_gear", op_load))
                    {
                        // <gears>
                        foreach (XmlNode xmlGearToImport in xmlStatBlockBaseNode.SelectNodes(
                            "gear/equipment/item[@useradded != \"no\"]"))
                        {
                            Gear objGear = new Gear(this);
                            if (objGear.ImportHeroLabGear(xmlGearToImport, null, lstWeapons))
                                _lstGear.Add(objGear);
                        }

                        foreach (XmlNode xmlPluginToAdd in xmlStatBlockBaseNode.SelectNodes(
                            "gear/equipment/item[@useradded = \"no\"]"))
                        {
                            string strName = xmlPluginToAdd.Attributes["name"]?.InnerText;
                            if (!string.IsNullOrEmpty(strName))
                            {
                                Gear objPlugin = _lstGear.FirstOrDefault(x =>
                                    x.IncludedInParent && (x.Name.Contains(strName) || strName.Contains(x.Name)));
                                if (objPlugin != null)
                                {
                                    objPlugin.Quantity =
                                        Convert.ToDecimal(xmlPluginToAdd.Attributes["quantity"]?.InnerText ?? "1",
                                            GlobalOptions.InvariantCultureInfo);
                                    objPlugin.Notes = xmlPluginToAdd["description"]?.InnerText;
                                    objPlugin.ProcessHeroLabGearPlugins(xmlPluginToAdd, lstWeapons);
                                }
                            }
                        }

                        //Timekeeper.Finish("load_char_gear");
                    }

                    using (_ = Timekeeper.StartSyncron("load_char_car", op_load))
                    {
                        foreach (Vehicle objVehicle in lstVehicles)
                        {
                            _lstVehicles.Add(objVehicle);
                        }

                        /* TODO: Process HeroLab Vehicles entries, which are present in HTML and TXT statblocks but not in XML
                        // Vehicles.
                        xmlNodeList = objXmlCharacter.SelectNodes("vehicles/vehicle");
                        foreach (XmlNode xmlHeroLabVehicle in xmlNodeList)
                        {
                            Vehicle objVehicle = new Vehicle(this);
                            objVehicle.Load(xmlHeroLabVehicle);
                            _lstVehicles.Add(objVehicle);
                        }
                        */
                        //Timekeeper.Finish("load_char_car");
                    }

                    /* TODO: Process HeroLab Initiation/Submersion and related entries
                    Timekeeper.Start("load_char_mmagic");
                    // Metamagics/Echoes.
                    xmlNodeList = objXmlCharacter.SelectNodes("metamagics/metamagic");
                    foreach (XmlNode xmlHeroLabMetamagic in xmlNodeList)
                    {
                        Metamagic objMetamagic = new Metamagic(this);
                        objMetamagic.Load(xmlHeroLabMetamagic);
                        _lstMetamagics.Add(objMetamagic);
                    }

                    Timekeeper.Finish("load_char_mmagic");
                    Timekeeper.Start("load_char_arts");

                    // Arts
                    xmlNodeList = objXmlCharacter.SelectNodes("arts/art");
                    foreach (XmlNode xmlHeroLabArt in xmlNodeList)
                    {
                        Art objArt = new Art(this);
                        objArt.Load(xmlHeroLabArt);
                        _lstArts.Add(objArt);
                    }

                    Timekeeper.Finish("load_char_arts");
                    Timekeeper.Start("load_char_ench");

                    // Enhancements
                    xmlNodeList = objXmlCharacter.SelectNodes("enhancements/enhancement");
                    foreach (XmlNode xmlHeroLabEnhancement in objXmlNodeList)
                    {
                        Enhancement objEnhancement = new Enhancement(this);
                        objEnhancement.Load(xmlHeroLabEnhancement);
                        _lstEnhancements.Add(objEnhancement);
                    }

                    Timekeeper.Finish("load_char_ench");
                    Timekeeper.Start("load_char_cpow");

                    // Critter Powers.
                    xmlNodeList = objXmlCharacter.SelectNodes("critterpowers/critterpower");
                    foreach (XmlNode xmlHeroLabPower in xmlNodeList)
                    {
                        CritterPower objPower = new CritterPower(this);
                        objPower.Load(xmlHeroLabPower);
                        _lstCritterPowers.Add(objPower);
                    }

                    Timekeeper.Finish("load_char_cpow");
                    Timekeeper.Start("load_char_foci");

                    // Foci.
                    xmlNodeList = objXmlCharacter.SelectNodes("foci/focus");
                    foreach (XmlNode xmlHeroLabFocus in xmlNodeList)
                    {
                        Focus objFocus = new Focus(this);
                        objFocus.Load(xmlHeroLabFocus);
                        _lstFoci.Add(objFocus);
                    }

                    Timekeeper.Finish("load_char_foci");
                    Timekeeper.Start("load_char_init");

                    // Initiation Grades.
                    xmlNodeList = objXmlCharacter.SelectNodes("initiationgrades/initiationgrade");
                    foreach (XmlNode xmlHeroLabGrade in xmlNodeList)
                    {
                        InitiationGrade objGrade = new InitiationGrade(this);
                        objGrade.Load(xmlHeroLabGrade);
                        _lstInitiationGrades.Add(objGrade);
                    }

                    Timekeeper.Finish("load_char_init");
                    */
                    /* TODO: Import HeroLab Expense Logs, which are different from Journal entries
                    Timekeeper.Start("load_char_elog");

                    // Expense Log Entries.
                    XmlNodeList xmlExpenseList = objXmlCharacter.SelectNodes("expenses/expense");
                    foreach (XmlNode xmlHeroLabExpense in xmlExpenseList)
                    {
                        ExpenseLogEntry objExpenseLogEntry = new ExpenseLogEntry(this);
                        objExpenseLogEntry.Load(xmlHeroLabExpense);
                        _lstExpenseLog.Add(objExpenseLogEntry);
                    }

                    Timekeeper.Finish("load_char_elog");
                    */
                    _lstWeapons.AddRange(lstWeapons);

                    using (_ = Timekeeper.StartSyncron("load_char_unarmed", op_load))
                    {
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
                            XmlDocument objXmlWeaponDoc = LoadData("weapons.xml");
                            XmlNode objXmlWeapon =
                                objXmlWeaponDoc.SelectSingleNode("/chummer/weapons/weapon[name = \"Unarmed Attack\"]");
                            if (objXmlWeapon != null)
                            {
                                Weapon objWeapon = new Weapon(this);
                                objWeapon.Create(objXmlWeapon, _lstWeapons);
                                objWeapon.IncludedInWeapon = true; // Unarmed attack can never be removed
                                _lstWeapons.Add(objWeapon);
                            }
                        }

                        //Timekeeper.Finish("load_char_unarmed");
                    }

                    // Refresh certain improvements
                    using (_ = Timekeeper.StartSyncron("load_char_improvementrefreshers2", op_load))
                    {
                        IsLoading = false;
                        // Refresh permanent attribute changes due to essence loss
                        RefreshEssenceLossImprovements();
                        // Refresh dicepool modifiers due to filled condition monitor boxes
                        RefreshWoundPenalties();
                        // Refresh encumbrance penalties
                        RefreshEncumbrance();
                        // Curb Mystic Adept power points if the values that were loaded in would be illegal
                        if (MysticAdeptPowerPoints > 0)
                        {
                            int intMAGTotalValue = MAG.TotalValue;
                            if (MysticAdeptPowerPoints > intMAGTotalValue)
                                MysticAdeptPowerPoints = intMAGTotalValue;
                        }

                        if (!InitiationEnabled || !AddInitiationsAllowed)
                            ClearInitiations();
                        //Timekeeper.Finish("load_char_improvementrefreshers");
                    }


                }
                catch (Exception e)
                {
                    op_load.SetSuccess(false);
                    TelemetryClient.TrackException(e);
                    Log.Error(e);
                }
            }

            return true;
        }
        #endregion

        #region Karma Values
        private int _intCachedPositiveQualities = int.MinValue;
        /// <summary>
        /// Total value of positive qualities that count towards the maximum quality limit in create mode.
        /// </summary>
        public int PositiveQualityKarma
        {
            get
            {
                if (_intCachedPositiveQualities == int.MinValue)
                {
                    _intCachedPositiveQualities = Qualities
                        .Where(objQuality => objQuality.Type == QualityType.Positive && objQuality.ContributeToBP && objQuality.ContributeToLimit)
                        .Sum(objQuality   => objQuality.BP) * Options.KarmaQuality;
                    // Group contacts are counted as positive qualities
                    _intCachedPositiveQualities += Contacts
                        .Where(x => x.EntityType == ContactType.Contact && x.IsGroup && !x.Free)
                        .Sum(x => x.ContactPoints);
                    // Each spell costs KarmaSpell.
                    int spellCost = SpellKarmaCost("Spells");
                    // It is only karma-efficient to use spell points for Mastery qualities if real spell karma cost is not greater than unmodified spell karma cost
                    if (spellCost <= Options.KarmaSpell && FreeSpells > 0)
                    {
                        // Assume that every [spell cost] karma spent on a Mastery quality is paid for with a priority-given spell point instead, as that is the most karma-efficient.
                        int intQualityKarmaToSpellPoints = Options.KarmaSpell;
                        if (Options.KarmaSpell != 0)
                            intQualityKarmaToSpellPoints = Math.Min(FreeSpells, (Qualities.Where(objQuality => objQuality.CanBuyWithSpellPoints).Sum(objQuality => objQuality.BP) * Options.KarmaQuality) / Options.KarmaSpell);
                        // Add the karma paid for by spell points back into the available karma pool.
                        _intCachedPositiveQualities -= intQualityKarmaToSpellPoints * Options.KarmaSpell;
                    }
                    // Deduct the amount for free Qualities.
                    _intCachedPositiveQualities -=
                        ImprovementManager.ValueOf(this, Improvement.ImprovementType.FreePositiveQualities) * Options.KarmaQuality;

                    // If the character is allowed to take as many Positive Qualities as they'd like but all costs in excess are doubled, add the excess to their point cost.
                    if (Options.ExceedPositiveQualitiesCostDoubled)
                    {
                        int intPositiveQualityExcess = _intCachedPositiveQualities - Options.QualityKarmaLimit;
                        if (intPositiveQualityExcess > 0)
                        {
                            _intCachedPositiveQualities += intPositiveQualityExcess;
                        }
                    }
                }
                return _intCachedPositiveQualities;
            }
        }
        private int _intCachedPositiveQualitiesTotal = int.MinValue;
        /// <summary>
        /// Total value of ALL positive qualities, including those that don't contribute to the quality limit during character creation.
        /// </summary>
        public int PositiveQualityKarmaTotal
        {
            get
            {
                if (_intCachedPositiveQualitiesTotal == int.MinValue)
                {
                    // Qualities that count towards the Quality Limit are checked first to support the house rule allowing doubling of qualities over said limit.
                    _intCachedPositiveQualitiesTotal = Qualities
                                                      .Where(objQuality => objQuality.Type == QualityType.Positive && objQuality.ContributeToBP && objQuality.ContributeToLimit)
                                                      .Sum(objQuality => objQuality.BP) * Options.KarmaQuality;
                    // Group contacts are counted as positive qualities
                    _intCachedPositiveQualitiesTotal += Contacts
                        .Where(x => x.EntityType == ContactType.Contact && x.IsGroup && !x.Free)
                        .Sum(x => x.ContactPoints);

                    // Deduct the amount for free Qualities.
                    _intCachedPositiveQualitiesTotal -=
                        ImprovementManager.ValueOf(this, Improvement.ImprovementType.FreePositiveQualities) * Options.KarmaQuality;

                    // If the character is allowed to take as many Positive Qualities as they'd like but all costs in excess are doubled, add the excess to their point cost.
                    if (Options.ExceedPositiveQualitiesCostDoubled)
                    {
                        int intPositiveQualityExcess = _intCachedPositiveQualitiesTotal - Options.QualityKarmaLimit;
                        if (intPositiveQualityExcess > 0)
                        {
                            _intCachedPositiveQualitiesTotal += intPositiveQualityExcess;
                        }
                    }
                    // Qualities that don't count towards the cap are added afterwards.
                    _intCachedPositiveQualitiesTotal += Qualities
                                                           .Where(objQuality => objQuality.Type == QualityType.Positive && objQuality.ContributeToBP && !objQuality.ContributeToLimit)
                                                           .Sum(objQuality => objQuality.BP) * Options.KarmaQuality;
                }
                return _intCachedPositiveQualitiesTotal;
            }
        }

        public string DisplayPositiveQualityKarma
        {
            get
            {
                if (PositiveQualityKarma != PositiveQualityKarmaTotal)
                {
                    return string.Format(GlobalOptions.CultureInfo, "{0}{2}/{2}{1}{2}({3}){2}{4}",
                        PositiveQualityKarma,
                        Options.QualityKarmaLimit,
                        LanguageManager.GetString("String_Space"),
                        PositiveQualityKarmaTotal,
                        LanguageManager.GetString("String_Karma"));
                }
                return string.Format(GlobalOptions.CultureInfo, "{0}/{1}{2}{3}",
                    PositiveQualityKarma,
                    Options.QualityKarmaLimit,
                    LanguageManager.GetString("String_Space"),
                    LanguageManager.GetString("String_Karma"));
            }
        }

        private int _intCachedNegativeQualities = int.MinValue;
        public int NegativeQualityKarma
        {
            get
            {
                if (_intCachedNegativeQualities == int.MinValue)
                {
                    _intCachedNegativeQualities = Qualities
                        .Where(objQuality => objQuality.Type == QualityType.Negative && objQuality.ContributeToBP)
                        .Sum(objQuality   => objQuality.BP) * Options.KarmaQuality;
                    // Group contacts are counted as positive qualities
                    _intCachedNegativeQualities += EnemyKarma;

                    // Deduct the amount for free Qualities.
                    _intCachedNegativeQualities -=
                        ImprovementManager.ValueOf(this, Improvement.ImprovementType.FreeNegativeQualities);

                    // If the character is only allowed to gain 25 BP from Negative Qualities but allowed to take as many as they'd like, limit their refunded points.
                    if (Options.ExceedNegativeQualitiesLimit)
                    {
                        int intNegativeQualityLimit = -Options.QualityKarmaLimit;
                        if (_intCachedNegativeQualities < intNegativeQualityLimit)
                        {
                            _intCachedNegativeQualities = intNegativeQualityLimit;
                        }
                    }

                    _intCachedNegativeQualities *= -1;
                }

                return _intCachedNegativeQualities;
            }
        }

        private int _intCachedNegativeQualityLimitKarma = int.MinValue;
        /// <summary>
        /// Negative qualities that contribute to the character's Quality Limit during character creation.
        /// </summary>
        public int NegativeQualityLimitKarma
        {
            get
            {
                if (_intCachedNegativeQualityLimitKarma == int.MinValue)
                {
                    _intCachedNegativeQualityLimitKarma = Qualities
                                                      .Where(objQuality => objQuality.Type == QualityType.Negative && objQuality.ContributeToLimit)
                                                      .Sum(objQuality => objQuality.BP) * Options.KarmaQuality;
                    // Group contacts are counted as positive qualities
                    if (Options.EnemyKarmaQualityLimit)
                        _intCachedNegativeQualityLimitKarma += EnemyKarma;

                    // Deduct the amount for free Qualities.
                    _intCachedNegativeQualityLimitKarma -=
                        ImprovementManager.ValueOf(this, Improvement.ImprovementType.FreeNegativeQualities);

                    // If the character is only allowed to gain 25 BP from Negative Qualities but allowed to take as many as they'd like, limit their refunded points.
                    if (Options.ExceedNegativeQualitiesLimit)
                    {
                        int intNegativeQualityLimit = -Options.QualityKarmaLimit;
                        if (_intCachedNegativeQualityLimitKarma < intNegativeQualityLimit)
                        {
                            _intCachedNegativeQualityLimitKarma = intNegativeQualityLimit;
                        }
                    }
                    _intCachedNegativeQualityLimitKarma *= -1;
                }

                return _intCachedNegativeQualityLimitKarma;
            }
        }

        public string DisplayNegativeQualityKarma
        {
            get
            {
                if (NegativeQualityLimitKarma != NegativeQualityKarma)
                {
                    return string.Format(GlobalOptions.CultureInfo, "{0}{2}/{2}{1}{2}({3}){2}{4}",
                        NegativeQualityLimitKarma,
                        Options.QualityKarmaLimit,
                        LanguageManager.GetString("String_Space"),
                        NegativeQualityKarma,
                        LanguageManager.GetString("String_Karma"));
                }
                return string.Format(GlobalOptions.CultureInfo, "{0}/{1}{2}{3}",
                    NegativeQualityKarma,
                    Options.QualityKarmaLimit,
                    LanguageManager.GetString("String_Space"),
                    LanguageManager.GetString("String_Karma"));
            }
        }

        private int _intCachedMetagenicPositiveQualities = int.MinValue;

        public int MetagenicPositiveQualityKarma
        {
            get
            {
                if (_intCachedMetagenicPositiveQualities == int.MinValue)
                {
                    _intCachedMetagenicPositiveQualities = Qualities
                        .Where(objQuality =>
                            objQuality.Type == QualityType.Positive && objQuality.ContributeToMetagenicLimit)
                        .Sum(objQuality => objQuality.BP);
                }
                return _intCachedMetagenicPositiveQualities;
            }
        }

        private int _intCachedMetagenicNegativeQualities = int.MinValue;
        public int MetagenicNegativeQualityKarma
        {
            get
            {
                if (_intCachedMetagenicNegativeQualities == int.MinValue)
                {
                    _intCachedMetagenicNegativeQualities = Qualities
                        .Where(objQuality =>
                            objQuality.Type == QualityType.Negative && objQuality.ContributeToMetagenicLimit)
                        .Sum(objQuality => objQuality.BP);

                    // Deduct the amount for free Qualities.
                    _intCachedMetagenicNegativeQualities -=
                        ImprovementManager.ValueOf(this, Improvement.ImprovementType.FreeNegativeQualities);
                }

                return _intCachedMetagenicNegativeQualities;
            }
        }

        public string DisplayMetagenicQualityKarma
        {
            get
            {
                StringBuilder sbdReturn = new StringBuilder();
                sbdReturn.AppendFormat(GlobalOptions.CultureInfo, LanguageManager.GetString("Label_MetagenicKarmaValue"),
                    MetagenicPositiveQualityKarma, MetagenicNegativeQualityKarma, MetagenicLimit);
                if (MetagenicPositiveQualityKarma + MetagenicNegativeQualityKarma == 1)
                    sbdReturn.Append(LanguageManager.GetString("Label_MetagenicKarmaValueAppend"));

                return sbdReturn.ToString();
            }
        }

        private int _intCachedEnemyKarma = int.MinValue;
        public int EnemyKarma
        {
            get
            {
                if (_intCachedEnemyKarma == int.MinValue)
                {
                    _intCachedEnemyKarma = Contacts
                        .Where(x => x.EntityType == ContactType.Enemy && x.IsGroup && !x.Free)
                        .Sum(x => (x.Connection + x.Loyalty) * Options.KarmaEnemy);
                }

                return _intCachedEnemyKarma;
            }
        }

        public string DisplayEnemyKarma => EnemyKarma.ToString(GlobalOptions.CultureInfo)
                                           + LanguageManager.GetString("String_Space")
                                           + LanguageManager.GetString("String_Karma");

        #endregion

        #region Source

        private SourceString _objCachedSourceDetail;
        public SourceString SourceDetail => _objCachedSourceDetail = _objCachedSourceDetail ?? new SourceString(Source, DisplayPage(GlobalOptions.Language), GlobalOptions.Language, GlobalOptions.CultureInfo, this);

        /// <summary>
        /// Character's Sourcebook.
        /// </summary>
        public string Source
        {
            get => _strSource;
            set => _strSource = value;
        }

        /// <summary>
        /// Sourcebook Page Number.
        /// </summary>
        public string Page
        {
            get => _strPage;
            set => _strPage = value;
        }

        public bool AllowAdeptWayPowerDiscount
        {
            get
            {
                int intMAG;
                if (IsMysticAdept && Options.MysAdeptSecondMAGAttribute)
                {
                    // If both Adept and Magician are enabled, this is a Mystic Adept, so use the MAG amount assigned to this portion.
                    intMAG = MAGAdept.TotalValue;
                }
                else
                {
                    // The character is just an Adept, so use the full value.
                    intMAG = MAG.TotalValue;
                }

                // Add any Power Point Improvements to MAG.
                intMAG += ImprovementManager.ValueOf(this, Improvement.ImprovementType.AdeptPowerPoints);

                return AnyPowerAdeptWayDiscountEnabled && Powers.Count(p => p.DiscountedAdeptWay) < Math.Floor(Convert.ToDouble(intMAG / 2));
            }
        }

        /// <summary>
        /// Sourcebook Page Number using a given language file.
        /// Returns Page if not found or the string is empty.
        /// </summary>
        /// <param name="strLanguage">Language file keyword to use.</param>
        /// <returns></returns>
        public string DisplayPage(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Page;
            string s = GetNode()?.SelectSingleNode("altpage")?.Value ?? Page;
            return !string.IsNullOrWhiteSpace(s) ? s : Page;
        }

        /// <summary>
        /// Alias map for SourceDetail control text and tooltip assignation.
        /// </summary>
        /// <param name="sourceControl"></param>
        public void SetSourceDetail(Control sourceControl)
        {
            if (_objCachedSourceDetail?.Language != GlobalOptions.Language)
                _objCachedSourceDetail = null;
            SourceDetail.SetControl(sourceControl);
        }
        #endregion

        #region Special Methods

        public bool ConvertCyberzombie()
        {
            bool blnEssence = true;
            string strMessage = LanguageManager.GetString("Message_CyberzombieRequirements");

            // Make sure the character has an Essence lower than 0.
            if (Essence() >= 0)
            {
                strMessage += Environment.NewLine + '\t' + LanguageManager.GetString("Message_CyberzombieRequirementsEssence");
                blnEssence = false;
            }

            bool blnEnabled =
                Improvements.Any(
                    imp => imp.ImproveType == Improvement.ImprovementType.EnableCyberzombie);

            if (!blnEnabled)
                strMessage += Environment.NewLine + '\t' + LanguageManager.GetString("Message_CyberzombieRequirementsImprovement");

            if (!blnEssence || !blnEnabled)
            {
                Program.MainForm.ShowMessageBox(strMessage,
                    LanguageManager.GetString("MessageTitle_CyberzombieRequirements"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_CyberzombieConfirm"),
                    LanguageManager.GetString("MessageTitle_CyberzombieConfirm"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return false;

            int intWILResult;
            // Get the player to roll Dice to make a WIL Test and record the result.
            using (frmDiceHits frmWILHits = new frmDiceHits
            {
                Text = LanguageManager.GetString("String_CyberzombieWILText"),
                Description = LanguageManager.GetString("String_CyberzombieWILDescription"),
                Dice = WIL.TotalValue
            })
            {
                frmWILHits.ShowDialog(Program.MainForm);

                if (frmWILHits.DialogResult != DialogResult.OK)
                    return false;

                intWILResult = frmWILHits.Result;
            }

            // The character gains 10 + ((Threshold - Hits) * 10)BP worth of Negative Qualities.
            int intThreshold = 3 + decimal.ToInt32(decimal.Floor(Essence() - ESS.MetatypeMaximum));
            int intResult = 10;
            if (intWILResult < intThreshold)
            {
                intResult = (intThreshold - intWILResult) * 10;
            }
            ImprovementManager.CreateImprovement(this, string.Empty, Improvement.ImprovementSource.Cyberzombie, string.Empty, Improvement.ImprovementType.FreeNegativeQualities, string.Empty, intResult * -1);
            ImprovementManager.Commit(this);

            // Convert the character.
            // Characters lose access to Resonance.
            RESEnabled = false;

            // Gain MAG that is permanently set to 1.
            MAGEnabled = true;
            MAG.MetatypeMinimum = 1;
            MAG.MetatypeMaximum = 1;

            // Add the Cyberzombie Lifestyle if it is not already taken.
            if (Lifestyles.All(x => x.BaseLifestyle != "Cyberzombie Lifestyle Addition"))
            {
                XmlDocument objXmlLifestyleDocument = LoadData("lifestyles.xml");
                XmlNode objXmlLifestyle = objXmlLifestyleDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = \"Cyberzombie Lifestyle Addition\"]");

                if (objXmlLifestyle != null)
                {
                    Lifestyle objLifestyle = new Lifestyle(this);
                    objLifestyle.Create(objXmlLifestyle);
                    Lifestyles.Add(objLifestyle);
                }
            }

            // Change the MetatypeCategory to Cyberzombie.
            MetatypeCategory = "Cyberzombie";

            // Gain access to Critter Powers.
            CritterEnabled = true;

            // Gain the Dual Natured Critter Power if it does not yet exist.
            if (CritterPowers.All(x => x.Name != "Dual Natured"))
            {
                XmlNode objXmlPowerNode = LoadData("critterpowers.xml").SelectSingleNode("/chummer/powers/power[name = \"Dual Natured\"]");

                if (objXmlPowerNode != null)
                {
                    CritterPower objCritterPower = new CritterPower(this);
                    objCritterPower.Create(objXmlPowerNode);
                    CritterPowers.Add(objCritterPower);
                }
            }

            // Gain the Immunity (Normal Weapons) Critter Power if it does not yet exist.
            if (!CritterPowers.Any(x => x.Name == "Immunity" && x.Extra == "Normal Weapons"))
            {
                XmlNode objXmlPowerNode = LoadData("critterpowers.xml").SelectSingleNode("/chummer/powers/power[name = \"Immunity\"]");

                if (objXmlPowerNode != null)
                {
                    CritterPower objCritterPower = new CritterPower(this);
                    objCritterPower.Create(objXmlPowerNode, 0, "Normal Weapons");
                    CritterPowers.Add(objCritterPower);
                }
            }

            return true;
        }
        #endregion
    }

    /// <summary>
    /// Caches a subset of a full character's properties for loading purposes.
    /// </summary>
    [DebuggerDisplay("{CharacterName} ({FileName})")]
    public class CharacterCache
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string ErrorText { get; set; }
        public string Description { get; set; }
        public string Background { get; set; }
        public string GameNotes { get; set; }
        public string CharacterNotes { get; set; }
        public string Concept { get; set; }
        public string Karma { get; set; }
        public string Metatype { get; set; }
        public string Metavariant { get; set; }
        public string PlayerName { get; set; }
        public string CharacterName { get; set; }
        public string CharacterAlias { get; set; }
        public string BuildMethod { get; set; }
        public string Essence { get; set; }
        public override string ToString()
        {
            return FilePath;
        }


        [JsonIgnore]
        [XmlIgnore]
        [IgnoreDataMember]
        public Image Mugshot => MugshotBase64.ToImage();

        public string MugshotBase64 { get; set; } = string.Empty;

        public bool Created { get; set; }
        public string SettingsFile { get; set; }


        [JsonIgnore]
        [XmlIgnore]
        [IgnoreDataMember]
        public Dictionary<string, object> MyPluginDataDic { get; } = new Dictionary<string, object>();

        public Task<string> DownLoadRunning { get; set; }

        public CharacterCache()
        {
            SetDefaultEventHandlers();
        }

        private void SetDefaultEventHandlers()
        {
            OnMyDoubleClick += OnDefaultDoubleClick;
            OnMyAfterSelect += OnDefaultAfterSelect;
            OnMyKeyDown += OnDefaultKeyDown;
            OnMyContextMenuDeleteClick += OnDefaultContextMenuDeleteClick;
        }

        [JsonIgnore]
        [XmlIgnore]
        [IgnoreDataMember]
        public EventHandler OnMyDoubleClick;

        [JsonIgnore]
        [XmlIgnore]
        [IgnoreDataMember]
        public EventHandler OnMyContextMenuDeleteClick;

        [JsonIgnore]
        [XmlIgnore]
        [IgnoreDataMember]
        public EventHandler<TreeViewEventArgs> OnMyAfterSelect;

        [JsonIgnore]
        [XmlIgnore]
        [IgnoreDataMember]
        public EventHandler<Tuple<KeyEventArgs, TreeNode>> OnMyKeyDown;

        public async void OnDefaultDoubleClick(object sender, EventArgs e)
        {
            Character objOpenCharacter = Program.MainForm.OpenCharacters.FirstOrDefault(x => x.FileName == FileName);

            if (objOpenCharacter == null || !Program.MainForm.SwitchToOpenCharacter(objOpenCharacter, true))
            {
                objOpenCharacter = await Program.MainForm.LoadCharacter(FilePath).ConfigureAwait(true);
                Program.MainForm.OpenCharacter(objOpenCharacter);
            }
        }


        public void OnDefaultContextMenuDeleteClick(object sender, EventArgs e)
        {
            if (sender is TreeNode t)
            {
                switch (t.Parent.Tag?.ToString())
                {
                    case "Recent":
                        GlobalOptions.MostRecentlyUsedCharacters.Remove(FilePath);
                        break;
                    case "Favorite":
                        GlobalOptions.FavoritedCharacters.Remove(FilePath);
                        break;
                }
            }
        }

        public CharacterCache(string strFile)
        {
            DownLoadRunning = null;
            SetDefaultEventHandlers();
            string strErrorText = string.Empty;
            XPathNavigator xmlSourceNode;
            if (!File.Exists(strFile))
            {
                xmlSourceNode = null;
                strErrorText = LanguageManager.GetString("MessageTitle_FileNotFound");
            }
            else
            {
                // If we run into any problems loading the character cache, fail out early.
                try
                {
                    XmlDocument xmlDoc = new XmlDocument
                    {
                        XmlResolver = null
                    };
                    using (StreamReader objStreamReader = new StreamReader(strFile, Encoding.UTF8, true))
                        using (XmlReader objXmlReader = XmlReader.Create(objStreamReader, GlobalOptions.SafeXmlReaderSettings))
                            xmlDoc.Load(objXmlReader);
                    xmlSourceNode = xmlDoc.CreateNavigator().SelectSingleNode("/character");
                }
                catch (Exception ex)
                {
                    xmlSourceNode = null;
                    strErrorText = ex.ToString();
                }
            }

            if (xmlSourceNode != null)
            {
                Description = xmlSourceNode.SelectSingleNode("description")?.Value;
                BuildMethod = xmlSourceNode.SelectSingleNode("buildmethod")?.Value;
                Background = xmlSourceNode.SelectSingleNode("background")?.Value;
                CharacterNotes = xmlSourceNode.SelectSingleNode("notes")?.Value;
                GameNotes = xmlSourceNode.SelectSingleNode("gamenotes")?.Value;
                Concept = xmlSourceNode.SelectSingleNode("concept")?.Value;
                Karma = xmlSourceNode.SelectSingleNode("totalkarma")?.Value;
                Metatype = xmlSourceNode.SelectSingleNode("metatype")?.Value;
                Metavariant = xmlSourceNode.SelectSingleNode("metavariant")?.Value;
                PlayerName = xmlSourceNode.SelectSingleNode("playername")?.Value;
                CharacterName = xmlSourceNode.SelectSingleNode("name")?.Value;
                CharacterAlias = xmlSourceNode.SelectSingleNode("alias")?.Value;
                Created = xmlSourceNode.SelectSingleNode("created")?.Value == bool.TrueString;
                Essence = xmlSourceNode.SelectSingleNode("totaless")?.Value;
                string strSettings = xmlSourceNode.SelectSingleNode("settings")?.Value ?? string.Empty;
                SettingsFile = !File.Exists(Path.Combine(Utils.GetStartupPath, "settings", strSettings)) ? LanguageManager.GetString("MessageTitle_FileNotFound") : strSettings;
                MugshotBase64 = xmlSourceNode.SelectSingleNode("mugshot")?.Value ?? string.Empty;
                if (string.IsNullOrEmpty(MugshotBase64))
                {
                    XPathNavigator xmlMainMugshotIndex = xmlSourceNode.SelectSingleNode("mainmugshotindex");
                    if (xmlMainMugshotIndex != null && int.TryParse(xmlMainMugshotIndex.Value, out int intMainMugshotIndex) && intMainMugshotIndex >= 0)
                    {
                        XPathNodeIterator xmlMugshotList = xmlSourceNode.Select("mugshots/mugshot");
                        if (xmlMugshotList.Count > intMainMugshotIndex)
                        {
                            int intIndex = 0;
                            foreach (XPathNavigator xmlMugshot in xmlMugshotList)
                            {
                                if (intMainMugshotIndex == intIndex)
                                {
                                    MugshotBase64 = xmlMugshot.Value;
                                    break;
                                }

                                intIndex += 1;
                            }
                        }
                    }
                }
            }
            else
            {
                ErrorText = strErrorText;
            }

            FilePath = strFile;
            if (!string.IsNullOrEmpty(strFile))
            {
                int last = strFile.LastIndexOf(Path.DirectorySeparatorChar) + 1;
                if (strFile.Length > last)
                    FileName = strFile.Substring(last);
            }
        }

        /// <summary>
        /// Generates a name for the treenode based on values contained in the CharacterCache object.
        /// </summary>
        /// <param name="blnAddMarkerIfOpen">Whether to add an asterisk to the beginning of the name if the character is open.</param>
        /// <returns></returns>
        public string CalculatedName(bool blnAddMarkerIfOpen = true)
        {
            string strReturn;
            if (!string.IsNullOrEmpty(ErrorText))
            {
                strReturn = Path.GetFileNameWithoutExtension(FileName) + LanguageManager.GetString("String_Space") + '(' + LanguageManager.GetString("String_Error") + ')';
            }
            else
            {
                strReturn = CharacterAlias;
                if (string.IsNullOrEmpty(strReturn))
                {
                    strReturn = CharacterName;
                    if (string.IsNullOrEmpty(strReturn))
                        strReturn = LanguageManager.GetString("String_UnnamedCharacter");
                }

                string strBuildMethod = LanguageManager.GetString("String_" + BuildMethod, false);
                if (string.IsNullOrEmpty(strBuildMethod))
                    strBuildMethod = LanguageManager.GetString("String_Unknown");
                strReturn += string.Format("{0}({1}{0}-{0}{2})",
                    LanguageManager.GetString("String_Space"),
                    strBuildMethod,
                    LanguageManager.GetString(Created ? "Title_CareerMode" : "Title_CreateMode"));
            }
            if (blnAddMarkerIfOpen && Program.MainForm.OpenCharacterForms.Any(x => x.CharacterObject.FileName == FilePath))
                strReturn = "* " + strReturn;
            return strReturn;
        }

        public void OnDefaultAfterSelect(object sender, TreeViewEventArgs e)
        {
        }

        public void OnDefaultKeyDown(object sender, Tuple<KeyEventArgs, TreeNode> args)
        {
            if (args?.Item1.KeyCode == Keys.Delete)
            {
                switch (args.Item2.Parent.Tag.ToString())
                {
                    case "Recent":
                        GlobalOptions.MostRecentlyUsedCharacters.Remove(FilePath);
                        break;
                    case "Favorite":
                        GlobalOptions.FavoritedCharacters.Remove(FilePath);
                        break;
                }
            }
        }
    }
}
