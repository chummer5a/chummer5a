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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Uniques;

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
    public sealed class Character : INotifyMultiplePropertyChanged, IHasMugshots, IHasName
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
        private int _intCachedContactPoints = int.MinValue;
        private int _intContactPointsUsed;
        private int _intCachedRedlinerBonus = int.MinValue;
        private int _intCurrentCounterspellingDice;

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
        private static readonly string[] s_LstLimbStrings = {"skull", "torso", "arm", "leg"};
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
        private readonly Tradition _objTradition;

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

        private readonly ObservableCollection<MentorSpirit>
            _lstMentorSpirits = new ObservableCollection<MentorSpirit>();

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
        private readonly ObservableCollection<Location> _lstVehicleLocations = new ObservableCollection<Location>();
        private readonly ObservableCollection<Location> _lstWeaponLocations = new ObservableCollection<Location>();
        private readonly ObservableCollection<string> _lstImprovementGroups = new ObservableCollection<string>();
        private readonly BindingList<CalendarWeek> _lstCalendar = new BindingList<CalendarWeek>();

        private readonly TaggedObservableCollection<Drug> _lstDrugs = new TaggedObservableCollection<Drug>();

        //private List<LifeModule> _lstLifeModules = new List<LifeModule>();
        private readonly List<string> _lstInternalIdsNeedingReapplyImprovements = new List<string>();

        // Character Version
        private string _strVersionCreated = Application.ProductVersion.FastEscapeOnceFromStart("0.0.");
        Version _verSavedVersion = new Version();

        #region Initialization, Save, Load, Print, and Reset Methods

        /// <summary>
        /// Character.
        /// </summary>
        public Character()
        {
            _objOptions = new CharacterOptions(this);
            AttributeSection = new AttributeSection(this);
            AttributeSection.Reset();
            AttributeSection.PropertyChanged += AttributeSectionOnPropertyChanged;

            SkillsSection = new SkillsSection(this);
            SkillsSection.Reset();

            _lstCyberware.CollectionChanged += CyberwareOnCollectionChanged;
            _lstArmor.CollectionChanged += ArmorOnCollectionChanged;
            _lstExpenseLog.CollectionChanged += ExpenseLogOnCollectionChanged;
            _lstMentorSpirits.CollectionChanged += MentorSpiritsOnCollectionChanged;
            _lstPowers.ListChanged += PowersOnListChanged;
            _lstPowers.BeforeRemove += PowersOnBeforeRemove;

            RefreshAttributeBindings();

            CharacterDependencyGraph =
                new DependancyGraph<string>(
                    new DependancyGraphNode<string>(nameof(CharacterName),
                        new DependancyGraphNode<string>(nameof(Alias)),
                        new DependancyGraphNode<string>(nameof(Name), () => string.IsNullOrWhiteSpace(Alias))
                    ),
                    new DependancyGraphNode<string>(nameof(DisplayPowerPointsRemaining),
                        new DependancyGraphNode<string>(nameof(PowerPointsTotal),
                            new DependancyGraphNode<string>(nameof(UseMysticAdeptPPs),
                                new DependancyGraphNode<string>(nameof(IsMysticAdept),
                                    new DependancyGraphNode<string>(nameof(AdeptEnabled)),
                                    new DependancyGraphNode<string>(nameof(MagicianEnabled))
                                )
                            ),
                            new DependancyGraphNode<string>(nameof(MysticAdeptPowerPoints), () => UseMysticAdeptPPs)
                        ),
                        new DependancyGraphNode<string>(nameof(PowerPointsUsed))
                    ),
                    new DependancyGraphNode<string>(nameof(CanAffordCareerPP),
                        new DependancyGraphNode<string>(nameof(MysAdeptAllowPPCareer),
                            new DependancyGraphNode<string>(nameof(UseMysticAdeptPPs))
                        ),
                        new DependancyGraphNode<string>(nameof(MysticAdeptPowerPoints)),
                        new DependancyGraphNode<string>(nameof(Karma))
                    ),
                    new DependancyGraphNode<string>(nameof(AddInitiationsAllowed),
                        new DependancyGraphNode<string>(nameof(IgnoreRules)),
                        new DependancyGraphNode<string>(nameof(Created))
                    ),
                    new DependancyGraphNode<string>(nameof(InitiationEnabled),
                        new DependancyGraphNode<string>(nameof(MAGEnabled)),
                        new DependancyGraphNode<string>(nameof(RESEnabled)),
                        new DependancyGraphNode<string>(nameof(InitiationForceDisabled))
                    ),
                    new DependancyGraphNode<string>(nameof(InitiativeToolTip),
                        new DependancyGraphNode<string>(nameof(Initiative),
                            new DependancyGraphNode<string>(nameof(InitiativeDice)),
                            new DependancyGraphNode<string>(nameof(InitiativeValue),
                                new DependancyGraphNode<string>(nameof(WoundModifier))
                            )
                        )
                    ),
                    new DependancyGraphNode<string>(nameof(AstralInitiativeToolTip),
                        new DependancyGraphNode<string>(nameof(AstralInitiative),
                            new DependancyGraphNode<string>(nameof(AstralInitiativeDice)),
                            new DependancyGraphNode<string>(nameof(AstralInitiativeValue),
                                new DependancyGraphNode<string>(nameof(WoundModifier))
                            )
                        ),
                        new DependancyGraphNode<string>(nameof(MAGEnabled))
                    ),
                    new DependancyGraphNode<string>(nameof(MatrixInitiativeToolTip),
                        new DependancyGraphNode<string>(nameof(MatrixInitiative),
                            new DependancyGraphNode<string>(nameof(MatrixInitiativeDice),
                                new DependancyGraphNode<string>(nameof(IsAI)),
                                new DependancyGraphNode<string>(nameof(InitiativeDice), () => !IsAI)
                            ),
                            new DependancyGraphNode<string>(nameof(MatrixInitiativeValue),
                                new DependancyGraphNode<string>(nameof(IsAI)),
                                new DependancyGraphNode<string>(nameof(HomeNode), () => IsAI),
                                new DependancyGraphNode<string>(nameof(WoundModifier), () => IsAI),
                                new DependancyGraphNode<string>(nameof(InitiativeValue), () => !IsAI)
                            )
                        )
                    ),
                    new DependancyGraphNode<string>(nameof(MatrixInitiativeColdToolTip),
                        new DependancyGraphNode<string>(nameof(MatrixInitiativeCold),
                            new DependancyGraphNode<string>(nameof(IsAI)),
                            new DependancyGraphNode<string>(nameof(MatrixInitiative), () => IsAI),
                            new DependancyGraphNode<string>(nameof(ActiveCommlink), () => !IsAI),
                            new DependancyGraphNode<string>(nameof(MatrixInitiativeColdDice),
                                new DependancyGraphNode<string>(nameof(IsAI)),
                                new DependancyGraphNode<string>(nameof(MatrixInitiativeDice), () => IsAI)
                            ),
                            new DependancyGraphNode<string>(nameof(MatrixInitiativeColdValue),
                                new DependancyGraphNode<string>(nameof(ActiveCommlink), () => !IsAI),
                                new DependancyGraphNode<string>(nameof(IsAI)),
                                new DependancyGraphNode<string>(nameof(MatrixInitiativeValue), () => IsAI),
                                new DependancyGraphNode<string>(nameof(WoundModifier), () => !IsAI)
                            )
                        )
                    ),
                    new DependancyGraphNode<string>(nameof(MatrixInitiativeHotToolTip),
                        new DependancyGraphNode<string>(nameof(MatrixInitiativeHot),
                            new DependancyGraphNode<string>(nameof(IsAI)),
                            new DependancyGraphNode<string>(nameof(MatrixInitiative), () => IsAI),
                            new DependancyGraphNode<string>(nameof(ActiveCommlink), () => !IsAI),
                            new DependancyGraphNode<string>(nameof(MatrixInitiativeHotDice),
                                new DependancyGraphNode<string>(nameof(IsAI)),
                                new DependancyGraphNode<string>(nameof(MatrixInitiativeDice), () => IsAI)
                            ),
                            new DependancyGraphNode<string>(nameof(MatrixInitiativeHotValue),
                                new DependancyGraphNode<string>(nameof(ActiveCommlink), () => !IsAI),
                                new DependancyGraphNode<string>(nameof(IsAI)),
                                new DependancyGraphNode<string>(nameof(MatrixInitiativeValue), () => IsAI),
                                new DependancyGraphNode<string>(nameof(WoundModifier), () => !IsAI)
                            )
                        )
                    ),
                    new DependancyGraphNode<string>(nameof(IsSprite),
                        new DependancyGraphNode<string>(nameof(IsFreeSprite),
                            new DependancyGraphNode<string>(nameof(MetatypeCategory))
                        )
                    ),
                    new DependancyGraphNode<string>(nameof(PhysicalCMLabelText),
                        new DependancyGraphNode<string>(nameof(IsAI)),
                        new DependancyGraphNode<string>(nameof(HomeNode))
                    ),
                    new DependancyGraphNode<string>(nameof(PhysicalCMToolTip),
                        new DependancyGraphNode<string>(nameof(PhysicalCM))
                    ),
                    new DependancyGraphNode<string>(nameof(StunCMToolTip),
                        new DependancyGraphNode<string>(nameof(StunCM))
                    ),
                    new DependancyGraphNode<string>(nameof(StunCMVisible),
                        new DependancyGraphNode<string>(nameof(IsAI)),
                        new DependancyGraphNode<string>(nameof(HomeNode))
                    ),
                    new DependancyGraphNode<string>(nameof(StunCMLabelText),
                        new DependancyGraphNode<string>(nameof(IsAI)),
                        new DependancyGraphNode<string>(nameof(HomeNode))
                    ),
                    new DependancyGraphNode<string>(nameof(WoundModifier),
                        new DependancyGraphNode<string>(nameof(PhysicalCMFilled),
                            new DependancyGraphNode<string>(nameof(HomeNode))
                        ),
                        new DependancyGraphNode<string>(nameof(PhysicalCM),
                            new DependancyGraphNode<string>(nameof(IsAI)),
                            new DependancyGraphNode<string>(nameof(HomeNode))
                        ),
                        new DependancyGraphNode<string>(nameof(StunCMFilled),
                            new DependancyGraphNode<string>(nameof(HomeNode))
                        ),
                        new DependancyGraphNode<string>(nameof(StunCM),
                            new DependancyGraphNode<string>(nameof(IsAI)),
                            new DependancyGraphNode<string>(nameof(HomeNode))
                        ),
                        new DependancyGraphNode<string>(nameof(CMThreshold)),
                        new DependancyGraphNode<string>(nameof(PhysicalCMThresholdOffset),
                            new DependancyGraphNode<string>(nameof(StunCMFilled)),
                            new DependancyGraphNode<string>(nameof(CMThreshold)),
                            new DependancyGraphNode<string>(nameof(IsAI))
                        ),
                        new DependancyGraphNode<string>(nameof(StunCMThresholdOffset),
                            new DependancyGraphNode<string>(nameof(PhysicalCMFilled)),
                            new DependancyGraphNode<string>(nameof(CMThreshold)),
                            new DependancyGraphNode<string>(nameof(IsAI))
                        )
                    ),
                    new DependancyGraphNode<string>(nameof(CMThresholdOffsets),
                        new DependancyGraphNode<string>(nameof(PhysicalCMThresholdOffset)),
                        new DependancyGraphNode<string>(nameof(StunCMThresholdOffset))
                    ),
                    new DependancyGraphNode<string>(nameof(BuildMethodHasSkillPoints),
                        new DependancyGraphNode<string>(nameof(BuildMethod))
                    ),
                    new DependancyGraphNode<string>(nameof(DamageResistancePoolToolTip),
                        new DependancyGraphNode<string>(nameof(DamageResistancePool),
                            new DependancyGraphNode<string>(nameof(TotalArmorRating),
                                new DependancyGraphNode<string>(nameof(ArmorRating))
                            ),
                            new DependancyGraphNode<string>(nameof(IsAI)),
                            new DependancyGraphNode<string>(nameof(HomeNode), () => IsAI)
                        )
                    ),
                    new DependancyGraphNode<string>(nameof(IsAI),
                        new DependancyGraphNode<string>(nameof(DEPEnabled))
                    ),
                    new DependancyGraphNode<string>(nameof(SpellDefenseIndirectDodgeToolTip),
                        new DependancyGraphNode<string>(nameof(CurrentCounterspellingDice)),
                        new DependancyGraphNode<string>(nameof(SpellDefenseIndirectDodge),
                            new DependancyGraphNode<string>(nameof(TotalBonusDodgeRating))
                        )
                    ),
                    new DependancyGraphNode<string>(nameof(DisplaySpellDefenseIndirectDodge),
                        new DependancyGraphNode<string>(nameof(CurrentCounterspellingDice)),
                        new DependancyGraphNode<string>(nameof(SpellDefenseIndirectDodge))
                    ),
                    new DependancyGraphNode<string>(nameof(SpellDefenseIndirectSoakToolTip),
                        new DependancyGraphNode<string>(nameof(CurrentCounterspellingDice)),
                        new DependancyGraphNode<string>(nameof(SpellDefenseIndirectSoak),
                            new DependancyGraphNode<string>(nameof(TotalArmorRating)),
                            new DependancyGraphNode<string>(nameof(IsAI)),
                            new DependancyGraphNode<string>(nameof(HomeNode), () => IsAI),
                            new DependancyGraphNode<string>(nameof(SpellResistance))
                        )
                    ),
                    new DependancyGraphNode<string>(nameof(DisplaySpellDefenseIndirectSoak),
                        new DependancyGraphNode<string>(nameof(CurrentCounterspellingDice)),
                        new DependancyGraphNode<string>(nameof(SpellDefenseIndirectSoak))
                    ),
                    new DependancyGraphNode<string>(nameof(SpellDefenseDirectSoakManaToolTip),
                        new DependancyGraphNode<string>(nameof(CurrentCounterspellingDice)),
                        new DependancyGraphNode<string>(nameof(SpellDefenseDirectSoakMana),
                            new DependancyGraphNode<string>(nameof(SpellResistance))
                        )
                    ),
                    new DependancyGraphNode<string>(nameof(DisplaySpellDefenseDirectSoakMana),
                        new DependancyGraphNode<string>(nameof(CurrentCounterspellingDice)),
                        new DependancyGraphNode<string>(nameof(SpellDefenseDirectSoakMana))
                    ),
                    new DependancyGraphNode<string>(nameof(SpellDefenseDirectSoakPhysicalToolTip),
                        new DependancyGraphNode<string>(nameof(CurrentCounterspellingDice)),
                        new DependancyGraphNode<string>(nameof(SpellDefenseDirectSoakPhysical),
                            new DependancyGraphNode<string>(nameof(SpellResistance))
                        )
                    ),
                    new DependancyGraphNode<string>(nameof(DisplaySpellDefenseDirectSoakPhysical),
                        new DependancyGraphNode<string>(nameof(CurrentCounterspellingDice)),
                        new DependancyGraphNode<string>(nameof(SpellDefenseDirectSoakPhysical))
                    ),
                    new DependancyGraphNode<string>(nameof(SpellDefenseDetectionToolTip),
                        new DependancyGraphNode<string>(nameof(CurrentCounterspellingDice)),
                        new DependancyGraphNode<string>(nameof(SpellDefenseDetection),
                            new DependancyGraphNode<string>(nameof(SpellResistance))
                        )
                    ),
                    new DependancyGraphNode<string>(nameof(DisplaySpellDefenseDetection),
                        new DependancyGraphNode<string>(nameof(CurrentCounterspellingDice)),
                        new DependancyGraphNode<string>(nameof(SpellDefenseDetection))
                    ),
                    new DependancyGraphNode<string>(nameof(SpellDefenseDecreaseBODToolTip),
                        new DependancyGraphNode<string>(nameof(CurrentCounterspellingDice)),
                        new DependancyGraphNode<string>(nameof(SpellDefenseDecreaseBOD),
                            new DependancyGraphNode<string>(nameof(SpellResistance))
                        )
                    ),
                    new DependancyGraphNode<string>(nameof(DisplaySpellDefenseDecreaseBOD),
                        new DependancyGraphNode<string>(nameof(CurrentCounterspellingDice)),
                        new DependancyGraphNode<string>(nameof(SpellDefenseDecreaseBOD))
                    ),
                    new DependancyGraphNode<string>(nameof(SpellDefenseDecreaseAGIToolTip),
                        new DependancyGraphNode<string>(nameof(CurrentCounterspellingDice)),
                        new DependancyGraphNode<string>(nameof(SpellDefenseDecreaseAGI),
                            new DependancyGraphNode<string>(nameof(SpellResistance))
                        )
                    ),
                    new DependancyGraphNode<string>(nameof(DisplaySpellDefenseDecreaseAGI),
                        new DependancyGraphNode<string>(nameof(CurrentCounterspellingDice)),
                        new DependancyGraphNode<string>(nameof(SpellDefenseDecreaseAGI))
                    ),
                    new DependancyGraphNode<string>(nameof(SpellDefenseDecreaseREAToolTip),
                        new DependancyGraphNode<string>(nameof(CurrentCounterspellingDice)),
                        new DependancyGraphNode<string>(nameof(SpellDefenseDecreaseREA),
                            new DependancyGraphNode<string>(nameof(SpellResistance))
                        )
                    ),
                    new DependancyGraphNode<string>(nameof(DisplaySpellDefenseDecreaseREA),
                        new DependancyGraphNode<string>(nameof(CurrentCounterspellingDice)),
                        new DependancyGraphNode<string>(nameof(SpellDefenseDecreaseREA))
                    ),
                    new DependancyGraphNode<string>(nameof(SpellDefenseDecreaseSTRToolTip),
                        new DependancyGraphNode<string>(nameof(CurrentCounterspellingDice)),
                        new DependancyGraphNode<string>(nameof(SpellDefenseDecreaseSTR),
                            new DependancyGraphNode<string>(nameof(SpellResistance))
                        )
                    ),
                    new DependancyGraphNode<string>(nameof(DisplaySpellDefenseDecreaseSTR),
                        new DependancyGraphNode<string>(nameof(CurrentCounterspellingDice)),
                        new DependancyGraphNode<string>(nameof(SpellDefenseDecreaseSTR))
                    ),
                    new DependancyGraphNode<string>(nameof(SpellDefenseDecreaseCHAToolTip),
                        new DependancyGraphNode<string>(nameof(CurrentCounterspellingDice)),
                        new DependancyGraphNode<string>(nameof(SpellDefenseDecreaseCHA),
                            new DependancyGraphNode<string>(nameof(SpellResistance))
                        )
                    ),
                    new DependancyGraphNode<string>(nameof(DisplaySpellDefenseDecreaseCHA),
                        new DependancyGraphNode<string>(nameof(CurrentCounterspellingDice)),
                        new DependancyGraphNode<string>(nameof(SpellDefenseDecreaseCHA))
                    ),
                    new DependancyGraphNode<string>(nameof(SpellDefenseDecreaseINTToolTip),
                        new DependancyGraphNode<string>(nameof(CurrentCounterspellingDice)),
                        new DependancyGraphNode<string>(nameof(SpellDefenseDecreaseINT),
                            new DependancyGraphNode<string>(nameof(SpellResistance))
                        )
                    ),
                    new DependancyGraphNode<string>(nameof(DisplaySpellDefenseDecreaseINT),
                        new DependancyGraphNode<string>(nameof(CurrentCounterspellingDice)),
                        new DependancyGraphNode<string>(nameof(SpellDefenseDecreaseINT))
                    ),
                    new DependancyGraphNode<string>(nameof(SpellDefenseDecreaseLOGToolTip),
                        new DependancyGraphNode<string>(nameof(CurrentCounterspellingDice)),
                        new DependancyGraphNode<string>(nameof(SpellDefenseDecreaseLOG),
                            new DependancyGraphNode<string>(nameof(SpellResistance))
                        )
                    ),
                    new DependancyGraphNode<string>(nameof(DisplaySpellDefenseDecreaseLOG),
                        new DependancyGraphNode<string>(nameof(CurrentCounterspellingDice)),
                        new DependancyGraphNode<string>(nameof(SpellDefenseDecreaseLOG))
                    ),
                    new DependancyGraphNode<string>(nameof(SpellDefenseDecreaseWILToolTip),
                        new DependancyGraphNode<string>(nameof(CurrentCounterspellingDice)),
                        new DependancyGraphNode<string>(nameof(SpellDefenseDecreaseWIL),
                            new DependancyGraphNode<string>(nameof(SpellResistance))
                        )
                    ),
                    new DependancyGraphNode<string>(nameof(DisplaySpellDefenseDecreaseWIL),
                        new DependancyGraphNode<string>(nameof(CurrentCounterspellingDice)),
                        new DependancyGraphNode<string>(nameof(SpellDefenseDecreaseWIL))
                    ),
                    new DependancyGraphNode<string>(nameof(SpellDefenseIllusionManaToolTip),
                        new DependancyGraphNode<string>(nameof(CurrentCounterspellingDice)),
                        new DependancyGraphNode<string>(nameof(SpellDefenseIllusionMana),
                            new DependancyGraphNode<string>(nameof(SpellResistance))
                        )
                    ),
                    new DependancyGraphNode<string>(nameof(DisplaySpellDefenseIllusionMana),
                        new DependancyGraphNode<string>(nameof(CurrentCounterspellingDice)),
                        new DependancyGraphNode<string>(nameof(SpellDefenseIllusionMana))
                    ),
                    new DependancyGraphNode<string>(nameof(SpellDefenseIllusionPhysicalToolTip),
                        new DependancyGraphNode<string>(nameof(CurrentCounterspellingDice)),
                        new DependancyGraphNode<string>(nameof(SpellDefenseIllusionPhysical),
                            new DependancyGraphNode<string>(nameof(SpellResistance))
                        )
                    ),
                    new DependancyGraphNode<string>(nameof(DisplaySpellDefenseIllusionPhysical),
                        new DependancyGraphNode<string>(nameof(CurrentCounterspellingDice)),
                        new DependancyGraphNode<string>(nameof(SpellDefenseIllusionPhysical))
                    ),
                    new DependancyGraphNode<string>(nameof(SpellDefenseManipulationMentalToolTip),
                        new DependancyGraphNode<string>(nameof(CurrentCounterspellingDice)),
                        new DependancyGraphNode<string>(nameof(SpellDefenseManipulationMental),
                            new DependancyGraphNode<string>(nameof(SpellResistance))
                        )
                    ),
                    new DependancyGraphNode<string>(nameof(DisplaySpellDefenseManipulationMental),
                        new DependancyGraphNode<string>(nameof(CurrentCounterspellingDice)),
                        new DependancyGraphNode<string>(nameof(SpellDefenseManipulationMental))
                    ),
                    new DependancyGraphNode<string>(nameof(SpellDefenseManipulationPhysicalToolTip),
                        new DependancyGraphNode<string>(nameof(CurrentCounterspellingDice)),
                        new DependancyGraphNode<string>(nameof(SpellDefenseManipulationPhysical),
                            new DependancyGraphNode<string>(nameof(SpellResistance)),
                            new DependancyGraphNode<string>(nameof(IsAI)),
                            new DependancyGraphNode<string>(nameof(HomeNode), () => IsAI)
                        )
                    ),
                    new DependancyGraphNode<string>(nameof(DisplaySpellDefenseManipulationPhysical),
                        new DependancyGraphNode<string>(nameof(CurrentCounterspellingDice)),
                        new DependancyGraphNode<string>(nameof(SpellDefenseManipulationPhysical))
                    ),
                    new DependancyGraphNode<string>(nameof(TotalArmorRatingToolTip),
                        new DependancyGraphNode<string>(nameof(TotalArmorRating))
                    ),
                    new DependancyGraphNode<string>(nameof(TotalFireArmorRating),
                        new DependancyGraphNode<string>(nameof(TotalArmorRating))
                    ),
                    new DependancyGraphNode<string>(nameof(TotalColdArmorRating),
                        new DependancyGraphNode<string>(nameof(TotalArmorRating))
                    ),
                    new DependancyGraphNode<string>(nameof(TotalElectricityArmorRating),
                        new DependancyGraphNode<string>(nameof(TotalArmorRating))
                    ),
                    new DependancyGraphNode<string>(nameof(TotalAcidArmorRating),
                        new DependancyGraphNode<string>(nameof(TotalArmorRating))
                    ),
                    new DependancyGraphNode<string>(nameof(TotalFallingArmorRating),
                        new DependancyGraphNode<string>(nameof(TotalArmorRating))
                    ),
                    new DependancyGraphNode<string>(nameof(DisplayEssence),
                        new DependancyGraphNode<string>(nameof(Essence),
                            new DependancyGraphNode<string>(nameof(CyberwareEssence)),
                            new DependancyGraphNode<string>(nameof(BiowareEssence)),
                            new DependancyGraphNode<string>(nameof(PrototypeTranshumanEssenceUsed)),
                            new DependancyGraphNode<string>(nameof(EssenceHole))
                        )
                    ),
                    new DependancyGraphNode<string>(nameof(ComposureToolTip),
                        new DependancyGraphNode<string>(nameof(Composure))
                    ),
                    new DependancyGraphNode<string>(nameof(JudgeIntentionsToolTip),
                        new DependancyGraphNode<string>(nameof(JudgeIntentions))
                    ),
                    new DependancyGraphNode<string>(nameof(JudgeIntentionsResistToolTip),
                        new DependancyGraphNode<string>(nameof(JudgeIntentionsResist))
                    ),
                    new DependancyGraphNode<string>(nameof(LiftAndCarryToolTip),
                        new DependancyGraphNode<string>(nameof(LiftAndCarry))
                    ),
                    new DependancyGraphNode<string>(nameof(MemoryToolTip),
                        new DependancyGraphNode<string>(nameof(Memory))
                    ),
                    new DependancyGraphNode<string>(nameof(DisplayCyberwareEssence),
                        new DependancyGraphNode<string>(nameof(CyberwareEssence))
                    ),
                    new DependancyGraphNode<string>(nameof(DisplayBiowareEssence),
                        new DependancyGraphNode<string>(nameof(BiowareEssence))
                    ),
                    new DependancyGraphNode<string>(nameof(DisplayEssenceHole),
                        new DependancyGraphNode<string>(nameof(EssenceHole))
                    ),
                    new DependancyGraphNode<string>(nameof(DisplayPrototypeTranshumanEssenceUsed),
                        new DependancyGraphNode<string>(nameof(PrototypeTranshumanEssenceUsed)),
                        new DependancyGraphNode<string>(nameof(PrototypeTranshuman))
                    ),
                    new DependancyGraphNode<string>(nameof(IsPrototypeTranshuman),
                        new DependancyGraphNode<string>(nameof(PrototypeTranshuman))
                    ),
                    new DependancyGraphNode<string>(nameof(DisplayNuyen),
                        new DependancyGraphNode<string>(nameof(Nuyen))
                    ),
                    new DependancyGraphNode<string>(nameof(DisplayKarma),
                        new DependancyGraphNode<string>(nameof(Karma))
                    ),
                    new DependancyGraphNode<string>(nameof(DisplayTotalStartingNuyen),
                        new DependancyGraphNode<string>(nameof(TotalStartingNuyen),
                            new DependancyGraphNode<string>(nameof(StartingNuyen)),
                            new DependancyGraphNode<string>(nameof(StartingNuyenModifiers)),
                            new DependancyGraphNode<string>(nameof(NuyenBP),
                                new DependancyGraphNode<string>(nameof(TotalNuyenMaximumBP),
                                    new DependancyGraphNode<string>(nameof(NuyenMaximumBP)),
                                    new DependancyGraphNode<string>(nameof(IgnoreRules))
                                )
                            )
                        )
                    ),
                    new DependancyGraphNode<string>(nameof(DisplayCareerNuyen),
                        new DependancyGraphNode<string>(nameof(CareerNuyen))
                    ),
                    new DependancyGraphNode<string>(nameof(DisplayCareerKarma),
                        new DependancyGraphNode<string>(nameof(CareerKarma))
                    ),
                    new DependancyGraphNode<string>(nameof(ContactPoints),
                        new DependancyGraphNode<string>(nameof(ContactMultiplier))
                    ),
                    new DependancyGraphNode<string>(nameof(StreetCredTooltip),
                        new DependancyGraphNode<string>(nameof(TotalStreetCred),
                            new DependancyGraphNode<string>(nameof(StreetCred)),
                            new DependancyGraphNode<string>(nameof(CalculatedStreetCred),
                                new DependancyGraphNode<string>(nameof(CareerKarma)),
                                new DependancyGraphNode<string>(nameof(BurntStreetCred))
                            )
                        )
                    ),
                    new DependancyGraphNode<string>(nameof(CanBurnStreetCred),
                        new DependancyGraphNode<string>(nameof(TotalStreetCred))
                    ),
                    new DependancyGraphNode<string>(nameof(NotorietyTooltip),
                        new DependancyGraphNode<string>(nameof(TotalNotoriety),
                            new DependancyGraphNode<string>(nameof(Notoriety)),
                            new DependancyGraphNode<string>(nameof(CalculatedNotoriety)),
                            new DependancyGraphNode<string>(nameof(BurntStreetCred))
                        )
                    ),
                    new DependancyGraphNode<string>(nameof(PublicAwarenessTooltip),
                        new DependancyGraphNode<string>(nameof(TotalPublicAwareness),
                            new DependancyGraphNode<string>(nameof(Erased)),
                            new DependancyGraphNode<string>(nameof(CalculatedPublicAwareness),
                                new DependancyGraphNode<string>(nameof(PublicAwareness)),
                                new DependancyGraphNode<string>(nameof(TotalStreetCred),
                                    () => _objOptions.UseCalculatedPublicAwareness),
                                new DependancyGraphNode<string>(nameof(TotalNotoriety),
                                    () => _objOptions.UseCalculatedPublicAwareness)
                            )
                        )
                    ),
                    new DependancyGraphNode<string>(nameof(CareerDisplayStreetCred),
                        new DependancyGraphNode<string>(nameof(TotalStreetCred))
                    ),
                    new DependancyGraphNode<string>(nameof(CareerDisplayNotoriety),
                        new DependancyGraphNode<string>(nameof(TotalNotoriety))
                    ),
                    new DependancyGraphNode<string>(nameof(CareerDisplayPublicAwareness),
                        new DependancyGraphNode<string>(nameof(TotalPublicAwareness))
                    ),
                    new DependancyGraphNode<string>(nameof(AddBiowareEnabled),
                        new DependancyGraphNode<string>(nameof(CyberwareDisabled))
                    ),
                    new DependancyGraphNode<string>(nameof(AddCyberwareEnabled),
                        new DependancyGraphNode<string>(nameof(CyberwareDisabled))
                    ),
                    new DependancyGraphNode<string>(nameof(HasMentorSpirit),
                        new DependancyGraphNode<string>(nameof(MentorSpirits))
                    ),
                    new DependancyGraphNode<string>(nameof(CharacterGrammaticGender),
                        new DependancyGraphNode<string>(nameof(Sex))
                    ),
                    new DependancyGraphNode<string>(nameof(FirstMentorSpiritDisplayName),
                        new DependancyGraphNode<string>(nameof(MentorSpirits))
                    ),
                    new DependancyGraphNode<string>(nameof(FirstMentorSpiritDisplayInformation),
                        new DependancyGraphNode<string>(nameof(MentorSpirits))
                    ),
                    new DependancyGraphNode<string>(nameof(LimitPhysicalToolTip),
                        new DependancyGraphNode<string>(nameof(LimitPhysical),
                            new DependancyGraphNode<string>(nameof(HomeNode))
                        )
                    ),
                    new DependancyGraphNode<string>(nameof(LimitMentalToolTip),
                        new DependancyGraphNode<string>(nameof(LimitMental),
                            new DependancyGraphNode<string>(nameof(HomeNode))
                        )
                    ),
                    new DependancyGraphNode<string>(nameof(LimitSocialToolTip),
                        new DependancyGraphNode<string>(nameof(LimitSocial),
                            new DependancyGraphNode<string>(nameof(HomeNode))
                        )
                    ),
                    new DependancyGraphNode<string>(nameof(LimitAstralToolTip),
                        new DependancyGraphNode<string>(nameof(LimitAstral),
                            new DependancyGraphNode<string>(nameof(LimitMental)),
                            new DependancyGraphNode<string>(nameof(LimitSocial))
                        )
                    ),
                    new DependancyGraphNode<string>(nameof(DisplayMovement),
                        new DependancyGraphNode<string>(nameof(GetMovement),
                            new DependancyGraphNode<string>(nameof(Movement)),
                            new DependancyGraphNode<string>(nameof(CalculatedMovement),
                                new DependancyGraphNode<string>(nameof(WalkingRate),
                                    new DependancyGraphNode<string>(nameof(CurrentWalkingRateString),
                                        new DependancyGraphNode<string>(nameof(WalkString),
                                            () => AttributeSection.AttributeCategory ==
                                                  CharacterAttrib.AttributeCategory.Standard),
                                        new DependancyGraphNode<string>(nameof(WalkAltString),
                                            () => AttributeSection.AttributeCategory !=
                                                  CharacterAttrib.AttributeCategory.Standard)
                                    )
                                ),
                                new DependancyGraphNode<string>(nameof(RunningRate),
                                    new DependancyGraphNode<string>(nameof(CurrentRunningRateString),
                                        new DependancyGraphNode<string>(nameof(RunString),
                                            () => AttributeSection.AttributeCategory ==
                                                  CharacterAttrib.AttributeCategory.Standard),
                                        new DependancyGraphNode<string>(nameof(RunAltString),
                                            () => AttributeSection.AttributeCategory !=
                                                  CharacterAttrib.AttributeCategory.Standard)
                                    )
                                ),
                                new DependancyGraphNode<string>(nameof(SprintingRate),
                                    new DependancyGraphNode<string>(nameof(CurrentSprintingRateString),
                                        new DependancyGraphNode<string>(nameof(SprintString),
                                            () => AttributeSection.AttributeCategory ==
                                                  CharacterAttrib.AttributeCategory.Standard),
                                        new DependancyGraphNode<string>(nameof(SprintAltString),
                                            () => AttributeSection.AttributeCategory !=
                                                  CharacterAttrib.AttributeCategory.Standard)
                                    )
                                )
                            )
                        )
                    ),
                    new DependancyGraphNode<string>(nameof(DisplaySwim),
                        new DependancyGraphNode<string>(nameof(GetSwim),
                            new DependancyGraphNode<string>(nameof(Movement)),
                            new DependancyGraphNode<string>(nameof(CalculatedMovement))
                        )
                    ),
                    new DependancyGraphNode<string>(nameof(DisplayFly),
                        new DependancyGraphNode<string>(nameof(GetFly),
                            new DependancyGraphNode<string>(nameof(Movement)),
                            new DependancyGraphNode<string>(nameof(CalculatedMovement))
                        )
                    )
                );
            _objTradition = new Tradition(this);
        }

        private void RefreshAttributeBindings()
        {
            BOD.PropertyChanged += RefreshBODDependentProperties;
            AGI.PropertyChanged += RefreshAGIDependentProperties;
            REA.PropertyChanged += RefreshREADependentProperties;
            STR.PropertyChanged += RefreshSTRDependentProperties;
            CHA.PropertyChanged += RefreshCHADependentProperties;
            INT.PropertyChanged += RefreshINTDependentProperties;
            LOG.PropertyChanged += RefreshLOGDependentProperties;
            WIL.PropertyChanged += RefreshWILDependentProperties;
            MAG.PropertyChanged += RefreshMAGDependentProperties;
            RES.PropertyChanged += RefreshRESDependentProperties;
            DEP.PropertyChanged += RefreshDEPDependentProperties;
            ESS.PropertyChanged += RefreshESSDependentProperties;
            // This needs to be explicitly set because a MAGAdept call could redirect to MAG, and we don't want that
            AttributeSection.GetAttributeByName("MAGAdept").PropertyChanged += RefreshMAGAdeptDependentProperties;
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

        private void PowersOnBeforeRemove(object sender, RemovingOldEventArgs e)
        {
            if (Powers[e.OldIndex].AdeptWayDiscountEnabled)
                OnPropertyChanged(nameof(AnyPowerAdeptWayDiscountEnabled));
        }

        private void PowersOnListChanged(object sender, ListChangedEventArgs e)
        {
            HashSet<string> setChangedProperties = new HashSet<string>();
            switch (e.ListChangedType)
            {
                case ListChangedType.Reset:
                {
                    setChangedProperties.Add(nameof(PowerPointsUsed));
                    setChangedProperties.Add(nameof(AnyPowerAdeptWayDiscountEnabled));
                }
                    break;
                case ListChangedType.ItemAdded:
                {
                    setChangedProperties.Add(nameof(PowerPointsUsed));
                    if (Powers[e.NewIndex].AdeptWayDiscountEnabled)
                        setChangedProperties.Add(nameof(AnyPowerAdeptWayDiscountEnabled));
                }
                    break;
                case ListChangedType.ItemDeleted:
                {
                    setChangedProperties.Add(nameof(PowerPointsUsed));
                }
                    break;
                case ListChangedType.ItemChanged:
                {
                    if (e.PropertyDescriptor == null)
                    {
                        break;
                    }

                    if (e.PropertyDescriptor.Name == nameof(Power.AdeptWayDiscountEnabled))
                        setChangedProperties.Add(nameof(AnyPowerAdeptWayDiscountEnabled));
                    else if (e.PropertyDescriptor.Name == nameof(Power.PowerPoints))
                        setChangedProperties.Add(nameof(PowerPointsUsed));
                }
                    break;
            }

            OnMultiplePropertyChanged(setChangedProperties.ToArray());
        }

        private void MentorSpiritsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(MentorSpirits));
        }

        private void ExpenseLogOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            HashSet<string> setPropertiesToRefresh = new HashSet<string>();
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (ExpenseLogEntry objNewItem in e.NewItems)
                    {
                        if (objNewItem.Amount > 0 && !objNewItem.Refund)
                        {
                            setPropertiesToRefresh.Add(objNewItem.Type == ExpenseType.Nuyen
                                ? nameof(CareerNuyen)
                                : nameof(CareerKarma));
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (ExpenseLogEntry objOldItem in e.OldItems)
                    {
                        if (objOldItem.Amount > 0 && !objOldItem.Refund)
                        {
                            setPropertiesToRefresh.Add(objOldItem.Type == ExpenseType.Nuyen
                                ? nameof(CareerNuyen)
                                : nameof(CareerKarma));
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (ExpenseLogEntry objOldItem in e.OldItems)
                    {
                        if (objOldItem.Amount > 0 && !objOldItem.Refund)
                        {
                            setPropertiesToRefresh.Add(objOldItem.Type == ExpenseType.Nuyen
                                ? nameof(CareerNuyen)
                                : nameof(CareerKarma));
                        }
                    }

                    foreach (ExpenseLogEntry objNewItem in e.NewItems)
                    {
                        if (objNewItem.Amount > 0 && !objNewItem.Refund)
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

            if (setPropertiesToRefresh.Count > 0)
                OnMultiplePropertyChanged(setPropertiesToRefresh.ToArray());
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
            {
                OnPropertyChanged(nameof(ArmorRating));
                RefreshEncumbrance();
            }
        }

        private void CyberwareOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            bool blnDoCyberlimbAttributesRefresh = false;
            HashSet<string> setEssenceImprovementsToRefresh = new HashSet<string>();
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    setEssenceImprovementsToRefresh.Add(nameof(RedlinerBonus));
                    foreach (Cyberware objNewItem in e.NewItems)
                    {
                        setEssenceImprovementsToRefresh.Add(objNewItem.EssencePropertyName);
                        if (!blnDoCyberlimbAttributesRefresh && !Options.DontUseCyberlimbCalculation &&
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
                    foreach (Cyberware objOldItem in e.OldItems)
                    {
                        setEssenceImprovementsToRefresh.Add(objOldItem.EssencePropertyName);
                        if (!blnDoCyberlimbAttributesRefresh && !Options.DontUseCyberlimbCalculation &&
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
                    if (!Options.DontUseCyberlimbCalculation)
                    {
                        foreach (Cyberware objOldItem in e.OldItems)
                        {
                            setEssenceImprovementsToRefresh.Add(objOldItem.EssencePropertyName);
                            if (!blnDoCyberlimbAttributesRefresh && !Options.DontUseCyberlimbCalculation &&
                                objOldItem.Category == "Cyberlimb" && objOldItem.Parent == null &&
                                objOldItem.ParentVehicle == null &&
                                !string.IsNullOrWhiteSpace(objOldItem.LimbSlot) &&
                                !Options.ExcludeLimbSlot.Contains(objOldItem.LimbSlot))
                            {
                                blnDoCyberlimbAttributesRefresh = true;
                            }
                        }

                        foreach (Cyberware objNewItem in e.NewItems)
                        {
                            setEssenceImprovementsToRefresh.Add(objNewItem.EssencePropertyName);
                            if (!blnDoCyberlimbAttributesRefresh && !Options.DontUseCyberlimbCalculation &&
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
                    setEssenceImprovementsToRefresh.Add(nameof(Essence));
                    break;
            }

            if (blnDoCyberlimbAttributesRefresh)
            {
                foreach (CharacterAttrib objCharacterAttrib in AttributeSection.AttributeList.Concat(AttributeSection
                    .SpecialAttributeList))
                {
                    if (objCharacterAttrib.Abbrev == "AGI" || objCharacterAttrib.Abbrev == "STR")
                    {
                        objCharacterAttrib.OnPropertyChanged(nameof(CharacterAttrib.TotalValue));
                    }
                }
            }

            if (setEssenceImprovementsToRefresh.Count > 0)
            {
                OnMultiplePropertyChanged(setEssenceImprovementsToRefresh.ToArray());
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
            // <special />
            objWriter.WriteElementString("special", _intSpecial.ToString());
            // <totalspecial />
            objWriter.WriteElementString("totalspecial", _intTotalSpecial.ToString());
            // <totalattributes />
            objWriter.WriteElementString("totalattributes", _intTotalAttributes.ToString());
            // <contactpoints />
            objWriter.WriteElementString("contactpoints", _intCachedContactPoints.ToString());
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
            // <currentcounterspellingdice />
            objWriter.WriteElementString("currentcounterspellingdice", _intCurrentCounterspellingDice.ToString());
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
            objWriter.WriteElementString("startingnuyen",
                _decStartingNuyen.ToString(GlobalOptions.InvariantCultureInfo));
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
            // <initiationdisabled />
            objWriter.WriteElementString("initiationdisabled", _blnInitiationDisabled.ToString());
            // <critter />
            objWriter.WriteElementString("critter", _blnCritterEnabled.ToString());

            // <prototypetranshuman />
            objWriter.WriteElementString("prototypetranshuman",
                _decPrototypeTranshuman.ToString(GlobalOptions.InvariantCultureInfo));

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

            _objTradition?.Save(objWriter);

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

            // <armorbundles>
            objWriter.WriteStartElement("armorlocations");
            foreach (Location objLocation in _lstArmorLocations)
            {
                objLocation.Save(objWriter);
            }

            // </armorbundles>
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

        public bool IsLoading { get; set; }

        /// <summary>
        /// Load the Character from an XML file.
        /// </summary>
        /// <param name="frmLoadingForm">Instancs of frmLoading to use to update with loading progress. frmLoading::PerformStep() is called 35 times within this method, so plan accordingly.</param>
        public bool Load(frmLoading frmLoadingForm = null)
        {
            Timekeeper.Start("load_xml");
            frmLoadingForm?.PerformStep("XML");
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
                    MessageBox.Show(
                        LanguageManager.GetString("Message_FailedLoad", GlobalOptions.Language)
                            .Replace("{0}", ex.Message),
                        LanguageManager.GetString("MessageTitle_FailedLoad", GlobalOptions.Language),
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            Timekeeper.Finish("load_xml");
            Timekeeper.Start("load_char_misc");
            frmLoadingForm?.PerformStep(LanguageManager.GetString("String_Settings"));
            XmlNode objXmlCharacter = objXmlDocument.SelectSingleNode("/character");
            XPathNavigator xmlCharacterNavigator = objXmlDocument.GetFastNavigator().SelectSingleNode("/character");

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
                !string.IsNullOrEmpty(strGameEdition) && strGameEdition != "SR5")
            {
                MessageBox.Show(LanguageManager.GetString("Message_IncorrectGameVersion_SR4", GlobalOptions.Language),
                    LanguageManager.GetString("MessageTitle_IncorrectGameVersion", GlobalOptions.Language),
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
                    string strMessage =
 LanguageManager.GetString("Message_OutdatedChummerSave", GlobalOptions.Language).Replace("{0}", _verSavedVersion.ToString()).Replace("{1}", verCurrentversion.ToString());
                    DialogResult result =
 MessageBox.Show(strMessage, LanguageManager.GetString("MessageTitle_IncorrectGameVersion", GlobalOptions.Language), MessageBoxButtons.YesNo, MessageBoxIcon.Error);

                    if (result != DialogResult.Yes)
                    {
                        IsLoading = false;
                        return false;
                    }
                }
#endif
            // Get the name of the settings file in use if possible.
            xmlCharacterNavigator.TryGetStringFieldQuickly("settings", ref _strSettingsFileName);

            // Load the character's settings file.
            if (!_objOptions.Load(_strSettingsFileName))
            {
                IsLoading = false;
                return false;
            }

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
                string strMessage = LanguageManager.GetString("Message_MissingSourceBooks", GlobalOptions.Language)
                    .Replace("{0}", TranslatedBookList(strMissingBooks, GlobalOptions.Language));
                if (MessageBox.Show(strMessage,
                        LanguageManager.GetString("Message_MissingSourceBooks_Title", GlobalOptions.Language),
                        MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    IsLoading = false;
                    return false;
                }
            }

            // Get the sourcebooks that were used to create the character and throw up a warning if there's a mismatch.
            string strMissingSourceNames = string.Empty;
            //Does the list of enabled books contain the current item?
            foreach (XPathNavigator xmlDirectoryName in xmlCharacterNavigator.Select(
                "customdatadirectorynames/directoryname"))
            {
                string strLoopString = xmlDirectoryName.Value;
                if (strLoopString.Length > 0 && !_objOptions.CustomDataDirectoryNames.Contains(strLoopString))
                {
                    strMissingSourceNames += strLoopString + ';' + Environment.NewLine;
                }
            }

            if (!string.IsNullOrEmpty(strMissingSourceNames))
            {
                string strMessage = LanguageManager
                    .GetString("Message_MissingCustomDataDirectories", GlobalOptions.Language)
                    .Replace("{0}", strMissingSourceNames);
                if (MessageBox.Show(strMessage,
                        LanguageManager.GetString("Message_MissingCustomDataDirectories_Title", GlobalOptions.Language),
                        MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    IsLoading = false;
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
                if (xmlCharacterNavigator.TryGetInt32FieldQuickly("buildkarma", ref _intBuildKarma) &&
                    _intBuildKarma == 35)
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
            xmlCharacterNavigator.TryGetInt32FieldQuickly("gameplayoptionqualitylimit",
                ref _intGameplayOptionQualityLimit);

            xmlCharacterNavigator.TryGetDecFieldQuickly("nuyenmaxbp", ref _decNuyenMaximumBP);
            xmlCharacterNavigator.TryGetInt32FieldQuickly("maxavail", ref _intMaxAvail);

            XmlDocument objXmlDocumentGameplayOptions = XmlManager.Load("gameplayoptions.xml");
            XmlNode xmlGameplayOption =
                objXmlDocumentGameplayOptions.SelectSingleNode(
                    "/chummer/gameplayoptions/gameplayoption[name = \"" + GameplayOption + "\"]");
            if (xmlGameplayOption == null)
            {
                string strMessage = LanguageManager.GetString("Message_MissingGameplayOption", GlobalOptions.Language)
                    .Replace("{0}", GameplayOption);
                if (MessageBox.Show(strMessage,
                        LanguageManager.GetString("Message_MissingGameplayOption_Title", GlobalOptions.Language),
                        MessageBoxButtons.OKCancel, MessageBoxIcon.Error) == DialogResult.OK)
                {
                    frmSelectBuildMethod frmPickBP = new frmSelectBuildMethod(this, true);
                    frmPickBP.ShowDialog();

                    if (frmPickBP.DialogResult != DialogResult.OK)
                    {
                        IsLoading = false;
                        return false;
                    }
                }
                else
                {
                    IsLoading = false;
                    return false;
                }
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
                XmlNodeList xmlBannedGradesList = xmlGameplayOption?.SelectNodes("bannedwaregrades/grade");
                if (xmlBannedGradesList?.Count > 0)
                    foreach (XmlNode xmlNode in xmlBannedGradesList)
                        BannedWareGrades.Add(xmlNode.InnerText);
            }

            string strSkill1 = string.Empty;
            string strSkill2 = string.Empty;
            if (xmlCharacterNavigator.TryGetStringFieldQuickly("priorityskill1", ref strSkill1) &&
                !string.IsNullOrEmpty(strSkill1))
                _lstPrioritySkills.Add(strSkill1);
            if (xmlCharacterNavigator.TryGetStringFieldQuickly("priorityskill2", ref strSkill2) &&
                !string.IsNullOrEmpty(strSkill2))
                _lstPrioritySkills.Add(strSkill2);

            xmlCharacterNavigator.TryGetBoolFieldQuickly("iscritter", ref _blnIsCritter);
            xmlCharacterNavigator.TryGetBoolFieldQuickly("possessed", ref _blnPossessed);

            xmlCharacterNavigator.TryGetInt32FieldQuickly("contactpoints", ref _intCachedContactPoints);
            xmlCharacterNavigator.TryGetInt32FieldQuickly("contactpointsused", ref _intContactPointsUsed);
            xmlCharacterNavigator.TryGetInt32FieldQuickly("cfplimit", ref _intCFPLimit);
            xmlCharacterNavigator.TryGetInt32FieldQuickly("ainormalprogramlimit", ref _intAINormalProgramLimit);
            xmlCharacterNavigator.TryGetInt32FieldQuickly("aiadvancedprogramlimit", ref _intAIAdvancedProgramLimit);
            xmlCharacterNavigator.TryGetInt32FieldQuickly("currentcounterspellingdice",
                ref _intCurrentCounterspellingDice);
            xmlCharacterNavigator.TryGetInt32FieldQuickly("spelllimit", ref _intSpellLimit);
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
            Timekeeper.Finish("load_char_misc");
            Timekeeper.Start("load_char_mentorspirit");
            frmLoadingForm?.PerformStep(LanguageManager.GetString("String_MentorSpirit"));
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
            frmLoadingForm?.PerformStep(LanguageManager.GetString("Tab_Improvements"));
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
                if (_decEssenceAtSpecialStart == decimal.MinValue &&
                    (strImprovementSource == "EssenceLoss" || strImprovementSource == "EssenceLossChargen"))
                    continue;

                if (blnDoCheckForOrphanedImprovements)
                {
                    string strLoopSourceName = objXmlImprovement["sourcename"]?.InnerText;
                    if (!string.IsNullOrEmpty(strLoopSourceName) && strLoopSourceName.IsGuid() &&
                        objXmlImprovement["custom"]?.InnerText != bool.TrueString)
                    {
                        // Hacky way to make sure this character isn't loading in any orphaned improvements.
                        // SourceName ID will pop up minimum twice in the save if the improvement's source is actually present: once in the improvement and once in the parent that added it.
                        if (strCharacterInnerXml.IndexOf(strLoopSourceName, StringComparison.Ordinal) ==
                            strCharacterInnerXml.LastIndexOf(strLoopSourceName, StringComparison.Ordinal))
                            continue;
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
                }
                catch (ArgumentException)
                {
                    _lstInternalIdsNeedingReapplyImprovements.Add(objXmlImprovement["sourcename"]?.InnerText);
                }
            }

            Timekeeper.Finish("load_char_imp");

            Timekeeper.Start("load_char_contacts");
            frmLoadingForm?.PerformStep(LanguageManager.GetString("Label_Contacts"));
            // Contacts.
            foreach (XPathNavigator xmlContact in xmlCharacterNavigator.Select("contacts/contact"))
            {
                Contact objContact = new Contact(this);
                objContact.Load(xmlContact);
                _lstContacts.Add(objContact);
            }

            Timekeeper.Finish("load_char_contacts");
            Timekeeper.Start("load_char_quality");
            frmLoadingForm?.PerformStep(LanguageManager.GetString("String_Qualities"));
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
                                                                           objQuality.Name == "The Warrior's Way") &&
                            objQuality.Bonus?.HasChildNodes == false)
                        {
                            ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.Quality,
                                objQuality.InternalId);
                            XmlNode objNode = objQuality.GetNode();
                            if (objNode != null)
                            {
                                objQuality.Bonus = objNode["bonus"];
                                if (objQuality.Bonus != null)
                                {
                                    ImprovementManager.ForcedValue = objQuality.Extra;
                                    ImprovementManager.CreateImprovements(this, Improvement.ImprovementSource.Quality,
                                        objQuality.InternalId, objQuality.Bonus, false, 1,
                                        objQuality.DisplayNameShort(GlobalOptions.Language));
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
                                            objCheckQuality.QualityId == objQuality.QualityId &&
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
                                            objQuality.FirstLevelBonus, false, 1,
                                            objQuality.DisplayNameShort(GlobalOptions.Language));
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

                        if (LastSavedVersion <= new Version("5.200.0") && objQuality.Name == "Made Man" &&
                            objQuality.Bonus["selectcontact"] != null)
                        {
                            string selectedContactGUID = (Improvements.FirstOrDefault(x =>
                                x.SourceName == objQuality.InternalId &&
                                x.ImproveType == Improvement.ImprovementType.ContactForcedLoyalty))?.ImprovedName;
                            if (string.IsNullOrWhiteSpace(selectedContactGUID))
                            {
                                selectedContactGUID = Contacts.FirstOrDefault(x => x.Name == objQuality.Extra)?.GUID;
                            }

                            if (string.IsNullOrWhiteSpace(selectedContactGUID))
                            {
                                // Populate the Magician Traditions list.
                                List<ListItem> lstContacts = new List<ListItem>();
                                foreach (Contact objContact in Contacts.Where(contact => contact.IsGroup))
                                {
                                    lstContacts.Add(new ListItem(objContact.Name, objContact.GUID));
                                }

                                if (lstContacts.Count > 1)
                                {
                                    lstContacts.Sort(CompareListItems.CompareNames);
                                }

                                frmSelectItem frmPickItem = new frmSelectItem
                                {
                                    DropdownItems = lstContacts
                                };
                                frmPickItem.ShowDialog();

                                // Make sure the dialogue window was not canceled.
                                if (frmPickItem.DialogResult == DialogResult.Cancel)
                                {
                                    return false;
                                }

                                selectedContactGUID = frmPickItem.SelectedItem;
                                frmPickItem.Dispose();
                            }

                            objQuality.Bonus =
                                xmlRootQualitiesNode.SelectSingleNode("quality[name=\"Made Man\"]/bonus");
                            objQuality.Extra = string.Empty;
                            ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.Quality,
                                objQuality.InternalId);
                            ImprovementManager.CreateImprovement(this, string.Empty,
                                Improvement.ImprovementSource.Quality, objQuality.InternalId,
                                Improvement.ImprovementType.MadeMan,
                                objQuality.DisplayNameShort(GlobalOptions.Language));
                            ImprovementManager.CreateImprovement(this, selectedContactGUID,
                                Improvement.ImprovementSource.Quality, objQuality.InternalId,
                                Improvement.ImprovementType.AddContact,
                                objQuality.DisplayNameShort(GlobalOptions.Language));
                            ImprovementManager.CreateImprovement(this, selectedContactGUID,
                                Improvement.ImprovementSource.Quality, objQuality.InternalId,
                                Improvement.ImprovementType.ContactForcedLoyalty,
                                objQuality.DisplayNameShort(GlobalOptions.Language));
                            ImprovementManager.CreateImprovement(this, selectedContactGUID,
                                Improvement.ImprovementSource.Quality, objQuality.InternalId,
                                Improvement.ImprovementType.ContactForceGroup,
                                objQuality.DisplayNameShort(GlobalOptions.Language));
                            ImprovementManager.CreateImprovement(this, selectedContactGUID,
                                Improvement.ImprovementSource.Quality, objQuality.InternalId,
                                Improvement.ImprovementType.ContactMakeFree,
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
            Timekeeper.Finish("load_char_quality");
            frmLoadingForm?.PerformStep(LanguageManager.GetString("Label_Attributes"));
            AttributeSection.Load(objXmlCharacter);
            RefreshAttributeBindings();
            Timekeeper.Start("load_char_misc2");

            // Attempt to load the split MAG CharacterAttribute information for Mystic Adepts.
            if (_blnAdeptEnabled && _blnMagicianEnabled)
            {
                xmlCharacterNavigator.TryGetInt32FieldQuickly("magsplitadept", ref _intMAGAdept);
                xmlCharacterNavigator.TryGetInt32FieldQuickly("magsplitmagician", ref _intMAGMagician);
            }

            frmLoadingForm?.PerformStep(LanguageManager.GetString("String_Tradition"));
            // Attempt to load in the character's tradition (or equivalent for Technomancers)
            string strTemp = string.Empty;
            if (xmlCharacterNavigator.TryGetStringFieldQuickly("stream", ref strTemp) && !string.IsNullOrEmpty(strTemp) && RESEnabled)
            {
                // Legacy load a Technomancer tradition
                XmlNode xmlTraditionListDataNode = XmlManager.Load("streams.xml").SelectSingleNode("/chummer/traditions");
                if (xmlTraditionListDataNode != null)
                {
                    XmlNode xmlTraditionDataNode = xmlTraditionListDataNode.SelectSingleNode("tradition[name = \"" + strTemp + "\"]");
                    if (xmlTraditionDataNode != null)
                    {
                        if (!_objTradition.Create(xmlTraditionDataNode, true))
                            _objTradition.ResetTradition();
                    }
                    else
                    {
                        xmlTraditionDataNode = xmlTraditionListDataNode.SelectSingleNode("tradition[name = \"Default\"]");
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
                XPathNavigator xpathTraditionNavigator = xmlCharacterNavigator.SelectSingleNode("tradition");
                // Regular tradition load
                if (xpathTraditionNavigator?.SelectSingleNode("id") != null)
                {
                    _objTradition.Load(objXmlCharacter.SelectSingleNode("tradition"));
                }
                // Not null but doesn't have children -> legacy load a magical tradition
                else if (xpathTraditionNavigator != null && MAGEnabled)
                {
                    XmlNode xmlTraditionListDataNode = XmlManager.Load("traditions.xml").SelectSingleNode("/chummer/traditions");
                    if (xmlTraditionListDataNode != null)
                    {
                        xmlCharacterNavigator.TryGetStringFieldQuickly("tradition", ref strTemp);
                        XmlNode xmlTraditionDataNode = xmlTraditionListDataNode.SelectSingleNode("tradition[name = \"" + strTemp + "\"]");
                        if (xmlTraditionDataNode != null)
                        {
                            if (!_objTradition.Create(xmlTraditionDataNode))
                                _objTradition.ResetTradition();
                        }
                        else
                        {
                            xmlTraditionDataNode = xmlTraditionListDataNode.SelectSingleNode("tradition[id = \"" + Tradition.CustomMagicalTraditionGuid + "\"]");
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
            Timekeeper.Finish("load_char_misc2");
            Timekeeper.Start("load_char_skills"); //slightly messy
            frmLoadingForm?.PerformStep(LanguageManager.GetString("Tab_Skills"));
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

            Timekeeper.Start("load_char_loc");
            frmLoadingForm?.PerformStep(LanguageManager.GetString("String_Locations"));
            // Locations.
            XmlNodeList objXmlLocationList = objXmlCharacter.SelectNodes("gearlocations/gearlocation");
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

            Timekeeper.Finish("load_char_loc");
            Timekeeper.Start("load_char_abundle");

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

            Timekeeper.Finish("load_char_abundle");
            Timekeeper.Start("load_char_vloc");

            // Vehicle Locations.
            XmlNodeList objXmlVehicleLocationList = objXmlCharacter.SelectNodes("vehiclelocations/vehiclelocation");
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

            Timekeeper.Finish("load_char_vloc");
            Timekeeper.Start("load_char_wloc");

            // Weapon Locations.
            XmlNodeList objXmlWeaponLocationList = objXmlCharacter.SelectNodes("weaponlocations/weaponlocation");
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

            Timekeeper.Finish("load_char_wloc");

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
            frmLoadingForm?.PerformStep(LanguageManager.GetString("Tab_Armor"));
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
            frmLoadingForm?.PerformStep(LanguageManager.GetString("Tab_Weapons"));
            // Weapons.
            objXmlNodeList = objXmlCharacter.SelectNodes("weapons/weapon");
            foreach (XmlNode objXmlWeapon in objXmlNodeList)
            {
                Weapon objWeapon = new Weapon(this);
                objWeapon.Load(objXmlWeapon);
                _lstWeapons.Add(objWeapon);
            }

            Timekeeper.Finish("load_char_weapons");
            Timekeeper.Start("load_char_drugs");
            frmLoadingForm?.PerformStep(LanguageManager.GetString("Tab_Drugs"));
            // Drugs.
            objXmlNodeList = objXmlDocument.SelectNodes("/character/drugs/drug");
            foreach (XmlNode objXmlDrug in objXmlNodeList)
            {
                Drug objDrug = new Drug(this);
                objDrug.Load(objXmlDrug);
                _lstDrugs.Add(objDrug);
            }

            Timekeeper.Finish("load_char_drugs");
            Timekeeper.Start("load_char_ware");
            frmLoadingForm?.PerformStep(LanguageManager.GetString("Tab_Cyberware"));
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
                if (objCyberware.Name == "Myostatin Inhibitor" && LastSavedVersion <= new Version("5.195.1") &&
                     !Improvements.Any(x => x.SourceName == objCyberware.InternalId && x.ImproveType == Improvement.ImprovementType.AttributeKarmaCost))
                {
                    XmlNode objNode = objCyberware.GetNode();
                    if (objNode != null)
                    {
                        ImprovementManager.RemoveImprovements(this, objCyberware.SourceType, objCyberware.InternalId);
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
                                objCyberware.InternalId, objCyberware.Bonus, false, objCyberware.Rating,
                                objCyberware.DisplayNameShort(GlobalOptions.Language));
                            if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                                objCyberware.Extra = ImprovementManager.SelectedValue;
                        }

                        if (objCyberware.WirelessOn && objCyberware.WirelessBonus != null)
                        {
                            ImprovementManager.CreateImprovements(this, objCyberware.SourceType,
                                objCyberware.InternalId, objCyberware.WirelessBonus, false, objCyberware.Rating,
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
            if (LastSavedVersion <= new Version("5.200.0"))
            {
                foreach (Cyberware objCyberware in Cyberware)
                {
                    if (objCyberware.PairBonus?.HasChildNodes == true &&
                         !Cyberware.DeepAny(x => x.Children, x => objCyberware.IncludePair.Contains(x.Name) && x.Extra == objCyberware.Extra && x.IsModularCurrentlyEquipped &&
                                                                  Improvements.Any(y => y.SourceName == x.InternalId + "Pair")))
                    {
                        XmlNode objNode = objCyberware.GetNode();
                        if (objNode != null)
                        {
                            ImprovementManager.RemoveImprovements(this, objCyberware.SourceType, objCyberware.InternalId);
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
                                    objCyberware.InternalId, objCyberware.Bonus, false, objCyberware.Rating,
                                    objCyberware.DisplayNameShort(GlobalOptions.Language));
                                if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                                    objCyberware.Extra = ImprovementManager.SelectedValue;
                            }

                            if (objCyberware.WirelessOn && objCyberware.WirelessBonus != null)
                            {
                                ImprovementManager.CreateImprovements(this, objCyberware.SourceType,
                                    objCyberware.InternalId, objCyberware.WirelessBonus, false, objCyberware.Rating,
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
                            if (!string.IsNullOrEmpty(objCyberware.Forced) && objCyberware.Forced != "Right" &&
                                objCyberware.Forced != "Left")
                                ImprovementManager.ForcedValue = objCyberware.Forced;
                            ImprovementManager.CreateImprovements(this, objLoopCyberware.SourceType,
                                objLoopCyberware.InternalId + "Pair", objLoopCyberware.PairBonus, false,
                                objLoopCyberware.Rating, objLoopCyberware.DisplayNameShort(GlobalOptions.Language));
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

            Timekeeper.Finish("load_char_ware");
            Timekeeper.Start("load_char_spells");
            frmLoadingForm?.PerformStep(LanguageManager.GetString("Label_SelectedSpells"));
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
            frmLoadingForm?.PerformStep(LanguageManager.GetString("Tab_Adept"));
            // Powers.
            bool blnDoEnhancedAccuracyRefresh = LastSavedVersion <= new Version("5.198.26");
            List<ListItem> lstPowerOrder = new List<ListItem>();
            objXmlNodeList = objXmlCharacter.SelectNodes("powers/power");
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
                    objXmlCharacter.SelectSingleNode("powers/power[guid = \"" + objItem.Value.ToString() + "\"]");
                if (objNode != null)
                {
                    Power objPower = new Power(this);
                    objPower.Load(objNode);
                    _lstPowers.Add(objPower);
                }
            }

            Timekeeper.Finish("load_char_powers");
            Timekeeper.Start("load_char_spirits");
            frmLoadingForm?.PerformStep(LanguageManager.GetString("Label_Spirits"));
            // Spirits/Sprites.
            foreach (XPathNavigator xmlSpirit in xmlCharacterNavigator.Select("spirits/spirit"))
            {
                Spirit objSpirit = new Spirit(this);
                objSpirit.Load(xmlSpirit);
                _lstSpirits.Add(objSpirit);
            }

            Timekeeper.Finish("load_char_spirits");
            Timekeeper.Start("load_char_complex");
            frmLoadingForm?.PerformStep(LanguageManager.GetString("Label_ComplexForms"));
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
            frmLoadingForm?.PerformStep(LanguageManager.GetString("Tab_AdvancedPrograms"));
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
            frmLoadingForm?.PerformStep(LanguageManager.GetString("Tab_MartialArts"));
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
            frmLoadingForm?.PerformStep(LanguageManager.GetString("Tab_Limits"));
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
            frmLoadingForm?.PerformStep(LanguageManager.GetString("String_SelectPACKSKit_Lifestyles"));
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
            frmLoadingForm?.PerformStep(LanguageManager.GetString("Tab_Gear"));
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
                            objLivingPersonaQuality.InternalId, objLivingPersonaQuality.Bonus, false, 1,
                            objLivingPersonaQuality.DisplayNameShort(GlobalOptions.Language));
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
                                objCheckQuality.QualityId == objLivingPersonaQuality.QualityId &&
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
                            ImprovementManager.CreateImprovements(this, Improvement.ImprovementSource.Quality,
                                objLivingPersonaQuality.InternalId, objLivingPersonaQuality.FirstLevelBonus, false, 1,
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

            Timekeeper.Finish("load_char_gear");
            Timekeeper.Start("load_char_car");
            frmLoadingForm?.PerformStep(LanguageManager.GetString("Label_Vehicles"));
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
            frmLoadingForm?.PerformStep(LanguageManager.GetString("String_Metamagics"));
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
            frmLoadingForm?.PerformStep(LanguageManager.GetString("String_Arts"));
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
            frmLoadingForm?.PerformStep(LanguageManager.GetString("String_Enhancements"));
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
            frmLoadingForm?.PerformStep(LanguageManager.GetString("Tab_Critter"));
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
            frmLoadingForm?.PerformStep(LanguageManager.GetString("Label_SummaryFoci"));
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
            frmLoadingForm?.PerformStep(LanguageManager.GetString("Label_SummaryInitiation"));
            // Initiation Grades.
            objXmlNodeList = objXmlCharacter.SelectNodes("initiationgrades/initiationgrade");
            foreach (XmlNode objXmlGrade in objXmlNodeList)
            {
                InitiationGrade objGrade = new InitiationGrade(this);
                objGrade.Load(objXmlGrade);
                _lstInitiationGrades.Add(objGrade);
            }
            
            Timekeeper.Finish("load_char_init");
            frmLoadingForm?.PerformStep(LanguageManager.GetString("String_Expenses"));
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
            Timekeeper.Start("load_char_igroup");
            frmLoadingForm?.PerformStep(LanguageManager.GetString("Tab_Improvements"));
            // Improvement Groups.
            XmlNodeList objXmlGroupList = objXmlCharacter.SelectNodes("improvementgroups/improvementgroup");
            foreach (XmlNode objXmlGroup in objXmlGroupList)
            {
                _lstImprovementGroups.Add(objXmlGroup.InnerText);
            }

            Timekeeper.Finish("load_char_igroup");
            Timekeeper.Start("load_char_calendar");
            frmLoadingForm?.PerformStep(LanguageManager.GetString("Tab_Calendar"));
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
            frmLoadingForm?.PerformStep(LanguageManager.GetString("String_LegacyFixes"));
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

            Timekeeper.Finish("load_char_unarmed");
            Timekeeper.Start("load_char_dwarffix");

            // converting from old dwarven resistance to new dwarven resistance
            if (Metatype.ToLower().Equals("dwarf"))
            {
                Quality objOldQuality =
                    Qualities.FirstOrDefault(x => x.Name.Equals("Resistance to Pathogens and Toxins"));
                if (objOldQuality != null)
                {
                    Qualities.Remove(objOldQuality);
                    if (Qualities.Any(x => x.Name.Equals("Resistance to Pathogens/Toxins")) == false &&
                        Qualities.Any(x => x.Name.Equals("Dwarf Resistance")) == false)
                    {
                        XmlNode objXmlDwarfQuality =
                            xmlRootQualitiesNode.SelectSingleNode(
                                "quality[name = \"Resistance to Pathogens/Toxins\"]") ??
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
                XmlNode objXmlGameplayOption = XmlManager.Load("gameplayoptions.xml")
                    .SelectSingleNode("/chummer/gameplayoptions/gameplayoption[name = \"" + _strGameplayOption + "\"]");
                if (objXmlGameplayOption != null)
                {
                    string strKarma = objXmlGameplayOption["karma"]?.InnerText;
                    string strNuyen = objXmlGameplayOption["maxnuyen"]?.InnerText;
                    string strContactMultiplier = _objOptions.FreeContactsMultiplierEnabled
                        ? _objOptions.FreeContactsMultiplier.ToString()
                        : objXmlGameplayOption["contactmultiplier"]?.InnerText;
                    _intMaxKarma = Convert.ToInt32(strKarma);
                    _decMaxNuyen = Convert.ToDecimal(strNuyen);
                    _intContactMultiplier = Convert.ToInt32(strContactMultiplier);
                    _intCachedContactPoints = (CHA.Base + CHA.Karma) * _intContactMultiplier;
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
                if (!Improvements.Any(imp =>
                    imp.ImproveType == Improvement.ImprovementType.MentorSpirit && !string.IsNullOrEmpty(imp.ImprovedName)))
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
            Timekeeper.Start("load_char_improvementrefreshers");
            frmLoadingForm?.PerformStep(LanguageManager.GetString("String_GeneratedImprovements"));
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
        public void PrintToStream(MemoryStream objStream, XmlTextWriter objWriter, CultureInfo objCulture,
            string strLanguageToPrint)
#else
        public void PrintToStream(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
#endif
        {
            string strMetatype = string.Empty;
            string strMetavariant = string.Empty;
            // Get the name of the Metatype and Metavariant.
            XmlDocument objMetatypeDoc = XmlManager.Load("metatypes.xml", strLanguageToPrint);
            XmlNode objMetatypeNode =
                objMetatypeDoc.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + Metatype + "\"]");
            if (objMetatypeNode == null)
            {
                objMetatypeDoc = XmlManager.Load("critters.xml", strLanguageToPrint);
                objMetatypeNode =
                    objMetatypeDoc.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + Metatype + "\"]");
            }

            if (objMetatypeNode != null)
            {
                strMetatype = objMetatypeNode["translate"]?.InnerText ?? Metatype;

                if (!string.IsNullOrEmpty(Metavariant))
                {
                    objMetatypeNode =
                        objMetatypeNode.SelectSingleNode("metavariants/metavariant[name = \"" + Metavariant + "\"]");

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
                objWriter.WriteElementString("primaryarm",
                    LanguageManager.GetString("String_Ambidextrous", strLanguageToPrint));
            }
            else if (PrimaryArm == "Left")
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
            objWriter.WriteElementString("sex",
                LanguageManager.TranslateExtra(LanguageManager.ReverseTranslateExtra(Sex, GlobalOptions.Language),
                    strLanguageToPrint));
            // <age />
            objWriter.WriteElementString("age",
                LanguageManager.TranslateExtra(LanguageManager.ReverseTranslateExtra(Age, GlobalOptions.Language),
                    strLanguageToPrint));
            // <eyes />
            objWriter.WriteElementString("eyes",
                LanguageManager.TranslateExtra(LanguageManager.ReverseTranslateExtra(Eyes, GlobalOptions.Language),
                    strLanguageToPrint));
            // <height />
            objWriter.WriteElementString("height",
                LanguageManager.TranslateExtra(LanguageManager.ReverseTranslateExtra(Height, GlobalOptions.Language),
                    strLanguageToPrint));
            // <weight />
            objWriter.WriteElementString("weight",
                LanguageManager.TranslateExtra(LanguageManager.ReverseTranslateExtra(Weight, GlobalOptions.Language),
                    strLanguageToPrint));
            // <skin />
            objWriter.WriteElementString("skin",
                LanguageManager.TranslateExtra(LanguageManager.ReverseTranslateExtra(Skin, GlobalOptions.Language),
                    strLanguageToPrint));
            // <hair />
            objWriter.WriteElementString("hair",
                LanguageManager.TranslateExtra(LanguageManager.ReverseTranslateExtra(Hair, GlobalOptions.Language),
                    strLanguageToPrint));
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

            objWriter.WriteElementString("totaless", Essence().ToString(_objOptions.EssenceFormat, objCulture));

            // <tradition />
            if (MagicTradition.Type != TraditionType.None)
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
            foreach (Contact objContact in Contacts)
            {
                objContact.Print(objWriter, objCulture, strLanguageToPrint);
            }

            // </contacts>
            objWriter.WriteEndElement();

            // <limitmodifiersphys>
            objWriter.WriteStartElement("limitmodifiersphys");
            foreach (LimitModifier objLimitModifier in LimitModifiers.Where(objLimitModifier =>
                objLimitModifier.Limit == "Physical"))
            {
                objLimitModifier.Print(objWriter, strLanguageToPrint);
            }

            // Populate Limit Modifiers from Improvements
            foreach (Improvement objImprovement in Improvements.Where(objImprovement =>
                (objImprovement.ImproveType == Improvement.ImprovementType.LimitModifier &&
                 objImprovement.ImprovedName == "Physical" && objImprovement.Enabled)))
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
            foreach (LimitModifier objLimitModifier in LimitModifiers.Where(objLimitModifier =>
                objLimitModifier.Limit == "Mental"))
            {
                objLimitModifier.Print(objWriter, strLanguageToPrint);
            }

            // Populate Limit Modifiers from Improvements
            foreach (Improvement objImprovement in Improvements.Where(objImprovement =>
                (objImprovement.ImproveType == Improvement.ImprovementType.LimitModifier &&
                 objImprovement.ImprovedName == "Mental" && objImprovement.Enabled)))
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
            foreach (LimitModifier objLimitModifier in LimitModifiers.Where(objLimitModifier =>
                objLimitModifier.Limit == "Social"))
            {
                objLimitModifier.Print(objWriter, strLanguageToPrint);
            }

            // Populate Limit Modifiers from Improvements
            foreach (Improvement objImprovement in Improvements.Where(objImprovement =>
                (objImprovement.ImproveType == Improvement.ImprovementType.LimitModifier &&
                 objImprovement.ImprovedName == "Social" && objImprovement.Enabled)))
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

            // <drugs>
            objWriter.WriteStartElement("drugs");
            foreach (Drug objDrug in Drugs)
            {
                objDrug.Print(objWriter, objCulture, strLanguageToPrint);
            }

            // </drugs>
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
                foreach (Metamagic objMetamagic in Metamagics.Where(
                    objMetamagic => objMetamagic.Grade == objgrade.Grade))
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
                foreach (Enhancement objEnhancement in Enhancements.Where(objEnhancement =>
                    objEnhancement.Grade == objgrade.Grade))
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
            _lstDrugs.Clear();

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
            string strSpaceCharacter = LanguageManager.GetString("String_Space", strLanguage);
            switch (objImprovement.ImproveSource)
            {
                case Improvement.ImprovementSource.Bioware:
                case Improvement.ImprovementSource.Cyberware:
                    Cyberware objReturnCyberware = Cyberware.DeepFirstOrDefault(x => x.Children,
                        x => x.InternalId == objImprovement.SourceName);
                    if (objReturnCyberware != null)
                    {
                        string strWareReturn = objReturnCyberware.DisplayNameShort(strLanguage);
                        if (objReturnCyberware.Parent != null)
                            strWareReturn += strSpaceCharacter + '(' +
                                             objReturnCyberware.Parent.DisplayNameShort(strLanguage) + ')';
                        return strWareReturn;
                    }

                    foreach (Vehicle objVehicle in Vehicles)
                    {
                        foreach (VehicleMod objVehicleMod in objVehicle.Mods)
                        {
                            objReturnCyberware = objVehicleMod.Cyberware.DeepFirstOrDefault(x => x.Children,
                                x => x.InternalId == objImprovement.SourceName);
                            if (objReturnCyberware != null)
                            {
                                string strWareReturn = objReturnCyberware.DisplayNameShort(strLanguage);
                                if (objReturnCyberware.Parent != null)
                                    strWareReturn += strSpaceCharacter + '(' +
                                                     objVehicle.DisplayNameShort(strLanguage) + ',' +
                                                     strSpaceCharacter + objVehicleMod.DisplayNameShort(strLanguage) +
                                                     ',' + strSpaceCharacter +
                                                     objReturnCyberware.Parent.DisplayNameShort(strLanguage) + ')';
                                else
                                    strWareReturn += strSpaceCharacter + '(' +
                                                     objVehicle.DisplayNameShort(strLanguage) + ',' +
                                                     strSpaceCharacter + objVehicleMod.DisplayNameShort(strLanguage) +
                                                     ')';
                                return strWareReturn;
                            }
                        }
                    }

                    break;
                case Improvement.ImprovementSource.Gear:
                    Gear objReturnGear =
                        Gear.DeepFirstOrDefault(x => x.Children, x => x.InternalId == objImprovement.SourceName);
                    if (objReturnGear != null)
                    {
                        string strGearReturn = objReturnGear.DisplayNameShort(strLanguage);
                        if (objReturnGear.Parent != null && objReturnGear.Parent is Gear parent)
                            strGearReturn += " (" + parent.DisplayNameShort(strLanguage) + ')';
                        return strGearReturn;
                    }

                    foreach (Weapon objWeapon in Weapons.DeepWhere(x => x.Children,
                        x => x.WeaponAccessories.Any(y => y.Gear.Count > 0)))
                    {
                        foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                        {
                            objReturnGear = objAccessory.Gear.DeepFirstOrDefault(x => x.Children,
                                x => x.InternalId == objImprovement.SourceName);
                            if (objReturnGear != null)
                            {
                                string strGearReturn = objReturnGear.DisplayNameShort(strLanguage);
                                if (objReturnGear.Parent != null && objReturnGear.Parent is Gear parent)
                                    strGearReturn += strSpaceCharacter + '(' + objWeapon.DisplayNameShort(strLanguage) +
                                                     ',' + strSpaceCharacter +
                                                     objAccessory.DisplayNameShort(strLanguage) + ',' +
                                                     strSpaceCharacter + parent.DisplayNameShort(strLanguage) + ')';
                                else
                                    strGearReturn += strSpaceCharacter + '(' + objWeapon.DisplayNameShort(strLanguage) +
                                                     ',' + strSpaceCharacter +
                                                     objAccessory.DisplayNameShort(strLanguage) + ')';
                                return strGearReturn;
                            }
                        }
                    }

                    foreach (Armor objArmor in Armor)
                    {
                        objReturnGear = objArmor.Gear.DeepFirstOrDefault(x => x.Children,
                            x => x.InternalId == objImprovement.SourceName);
                        if (objReturnGear != null)
                        {
                            string strGearReturn = objReturnGear.DisplayNameShort(strLanguage);
                            if (objReturnGear.Parent != null && objReturnGear.Parent is Gear parent)
                                strGearReturn += strSpaceCharacter + '(' + objArmor.DisplayNameShort(strLanguage) +
                                                 ',' + strSpaceCharacter + parent.DisplayNameShort(strLanguage) + ')';
                            else
                                strGearReturn += strSpaceCharacter + '(' + objArmor.DisplayNameShort(strLanguage) + ')';
                            return strGearReturn;
                        }
                    }

                    foreach (Cyberware objCyberware in Cyberware.DeepWhere(x => x.Children, x => x.Gear.Count > 0))
                    {
                        objReturnGear = objCyberware.Gear.DeepFirstOrDefault(x => x.Children,
                            x => x.InternalId == objImprovement.SourceName);
                        if (objReturnGear != null)
                        {
                            string strGearReturn = objReturnGear.DisplayNameShort(strLanguage);
                            if (objReturnGear.Parent != null && objReturnGear.Parent is Gear parent)
                                strGearReturn += strSpaceCharacter + '(' + objCyberware.DisplayNameShort(strLanguage) +
                                                 ',' + strSpaceCharacter + parent.DisplayNameShort(strLanguage) + ')';
                            else
                                strGearReturn += strSpaceCharacter + '(' + objCyberware.DisplayNameShort(strLanguage) +
                                                 ')';
                            return strGearReturn;
                        }
                    }

                    foreach (Vehicle objVehicle in Vehicles)
                    {
                        objReturnGear = objVehicle.Gear.DeepFirstOrDefault(x => x.Children,
                            x => x.InternalId == objImprovement.SourceName);
                        if (objReturnGear != null)
                        {
                            string strGearReturn = objReturnGear.DisplayNameShort(strLanguage);
                            if (objReturnGear.Parent != null && objReturnGear.Parent is Gear parent)
                                strGearReturn += strSpaceCharacter + '(' + objVehicle.DisplayNameShort(strLanguage) +
                                                 ',' + strSpaceCharacter + parent.DisplayNameShort(strLanguage) + ')';
                            else
                                strGearReturn += strSpaceCharacter + '(' + objVehicle.DisplayNameShort(strLanguage) +
                                                 ')';
                            return strGearReturn;
                        }

                        foreach (Weapon objWeapon in objVehicle.Weapons.DeepWhere(x => x.Children,
                            x => x.WeaponAccessories.Any(y => y.Gear.Count > 0)))
                        {
                            foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                            {
                                objReturnGear = objAccessory.Gear.DeepFirstOrDefault(x => x.Children,
                                    x => x.InternalId == objImprovement.SourceName);
                                if (objReturnGear != null)
                                {
                                    string strGearReturn = objReturnGear.DisplayNameShort(strLanguage);
                                    if (objReturnGear.Parent != null && objReturnGear.Parent is Gear parent)
                                        strGearReturn += strSpaceCharacter + '(' +
                                                         objVehicle.DisplayNameShort(strLanguage) + ',' +
                                                         strSpaceCharacter + objWeapon.DisplayNameShort(strLanguage) +
                                                         ',' + strSpaceCharacter +
                                                         objAccessory.DisplayNameShort(strLanguage) + ',' +
                                                         strSpaceCharacter + parent.DisplayNameShort(strLanguage) + ')';
                                    else
                                        strGearReturn += strSpaceCharacter + '(' +
                                                         objVehicle.DisplayNameShort(strLanguage) + ',' +
                                                         strSpaceCharacter + objWeapon.DisplayNameShort(strLanguage) +
                                                         ',' + strSpaceCharacter +
                                                         objAccessory.DisplayNameShort(strLanguage) + ')';
                                    return strGearReturn;
                                }
                            }
                        }

                        foreach (VehicleMod objVehicleMod in objVehicle.Mods)
                        {
                            foreach (Weapon objWeapon in objVehicleMod.Weapons.DeepWhere(x => x.Children,
                                x => x.WeaponAccessories.Any(y => y.Gear.Count > 0)))
                            {
                                foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                                {
                                    objReturnGear = objAccessory.Gear.DeepFirstOrDefault(x => x.Children,
                                        x => x.InternalId == objImprovement.SourceName);
                                    if (objReturnGear != null)
                                    {
                                        string strGearReturn = objReturnGear.DisplayNameShort(strLanguage);
                                        if (objReturnGear.Parent != null && objReturnGear.Parent is Gear parent)
                                            strGearReturn += strSpaceCharacter + '(' +
                                                             objVehicle.DisplayNameShort(strLanguage) + ',' +
                                                             strSpaceCharacter +
                                                             objVehicleMod.DisplayNameShort(strLanguage) + ',' +
                                                             strSpaceCharacter +
                                                             objWeapon.DisplayNameShort(strLanguage) + ',' +
                                                             strSpaceCharacter +
                                                             objAccessory.DisplayNameShort(strLanguage) + ',' +
                                                             strSpaceCharacter + parent.DisplayNameShort(strLanguage) +
                                                             ')';
                                        else
                                            strGearReturn += strSpaceCharacter + '(' +
                                                             objVehicle.DisplayNameShort(strLanguage) + ',' +
                                                             strSpaceCharacter +
                                                             objVehicleMod.DisplayNameShort(strLanguage) + ',' +
                                                             strSpaceCharacter +
                                                             objWeapon.DisplayNameShort(strLanguage) + ',' +
                                                             strSpaceCharacter +
                                                             objAccessory.DisplayNameShort(strLanguage) + ')';
                                        return strGearReturn;
                                    }
                                }
                            }

                            foreach (Cyberware objCyberware in objVehicleMod.Cyberware.DeepWhere(x => x.Children,
                                x => x.Gear.Count > 0))
                            {
                                objReturnGear = objCyberware.Gear.DeepFirstOrDefault(x => x.Children,
                                    x => x.InternalId == objImprovement.SourceName);
                                if (objReturnGear != null)
                                {
                                    string strGearReturn = objReturnGear.DisplayNameShort(strLanguage);
                                    if (objReturnGear.Parent != null && objReturnGear.Parent is Gear parent)
                                        strGearReturn += strSpaceCharacter + '(' +
                                                         objVehicle.DisplayNameShort(strLanguage) + ',' +
                                                         strSpaceCharacter +
                                                         objVehicleMod.DisplayNameShort(strLanguage) + ',' +
                                                         strSpaceCharacter +
                                                         objCyberware.DisplayNameShort(strLanguage) + ',' +
                                                         strSpaceCharacter + parent.DisplayNameShort(strLanguage) + ')';
                                    else
                                        strGearReturn += strSpaceCharacter + '(' +
                                                         objVehicle.DisplayNameShort(strLanguage) + ',' +
                                                         strSpaceCharacter +
                                                         objVehicleMod.DisplayNameShort(strLanguage) + ',' +
                                                         strSpaceCharacter +
                                                         objCyberware.DisplayNameShort(strLanguage) + ')';
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
                                return objMod.DisplayNameShort(strLanguage) + strSpaceCharacter + '(' +
                                       objArmor.DisplayNameShort(strLanguage) + ')';
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
                        return XmlManager.Load("qualities.xml")
                                   .SelectSingleNode(
                                       "/chummer/qualities/quality[name = \"Cyber-Singularity Seeker\"]/translate")
                                   ?.InnerText ?? "Cyber-Singularity Seeker";
                    }
                    else if (objImprovement.SourceName.StartsWith("SEEKER"))
                    {
                        return XmlManager.Load("qualities.xml")
                                   .SelectSingleNode("/chummer/qualities/quality[name = \"Redliner\"]/translate")
                                   ?.InnerText ?? "Redliner";
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
                case Improvement.ImprovementSource.Tradition:
                    return LanguageManager.GetString("String_Tradition", strLanguage);
                default:
                    if (objImprovement.ImproveType == Improvement.ImprovementType.ArmorEncumbrancePenalty)
                        return LanguageManager.GetString("String_ArmorEncumbrance", strLanguage);
                    // If this comes from a custom Improvement, use the name the player gave it instead of showing a GUID.
                    if (!string.IsNullOrEmpty(objImprovement.CustomName))
                        return objImprovement.CustomName;
                    string strReturn = objImprovement.SourceName;
                    if (string.IsNullOrEmpty(strReturn) || strReturn.IsGuid())
                    {
                        string strTemp = LanguageManager.GetString("String_" + objImprovement.ImproveSource.ToString(),
                            strLanguage, false);
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

            using (XmlNodeList xmlGradeList = XmlManager
                .Load(objSource == Improvement.ImprovementSource.Bioware ? "bioware.xml" : objSource == Improvement.ImprovementSource.Drug ? "drugcomponents.xml" : "cyberware.xml")
                .SelectNodes(strXPath))
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

                int intPowerPoints = EDG.TotalValue +
                                     ImprovementManager.ValueOf(this,
                                         Improvement.ImprovementType.FreeSpiritPowerPoints);

                // If the house rule to base Power Points on the character's MAG value instead, use the character's MAG.
                if (Options.FreeSpiritPowerPointsMAG)
                    intPowerPoints = MAG.TotalValue +
                                     ImprovementManager.ValueOf(this,
                                         Improvement.ImprovementType.FreeSpiritPowerPoints);

                strReturn = string.Format(
                    "{1} ({0} " + LanguageManager.GetString("String_Remaining", GlobalOptions.Language) + ')',
                    intPowerPoints - decPowerPoints, intPowerPoints);
            }
            else
            {
                int intPowerPoints;

                if (Metatype == "Free Spirit")
                {
                    // Critter Free Spirits have a number of Power Points equal to their EDG plus any Free Spirit Power Points Improvements.
                    intPowerPoints =
                        EDG.Value + ImprovementManager.ValueOf(this, Improvement.ImprovementType.FreeSpiritPowerPoints);
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

                int intUsed = 0; // _objCharacter.CritterPowers.Count - intExisting;
                foreach (CritterPower objPower in CritterPowers)
                {
                    if (objPower.Category != "Weakness" && objPower.CountTowardsLimit)
                        intUsed += 1;
                }

                strReturn = string.Format(
                    "{1} ({0} " + LanguageManager.GetString("String_Remaining", GlobalOptions.Language) + ')',
                    intPowerPoints - intUsed, intPowerPoints);
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

            int intPowerPoints = EDG.TotalValue +
                                 ImprovementManager.ValueOf(this, Improvement.ImprovementType.FreeSpiritPowerPoints);

            return string.Format(
                "{1} ({0} " + LanguageManager.GetString("String_Remaining", GlobalOptions.Language) + ')',
                intPowerPoints - intUsedPowerPoints, intPowerPoints);
        }

        /// <summary>
        /// Construct a list of possible places to put a piece of modular cyberware. Names are display names of the given items, values are internalIDs of the given items.
        /// </summary>
        /// <param name="objModularCyberware">Cyberware for which to construct the list.</param>
        /// <returns></returns>
        public IList<ListItem> ConstructModularCyberlimbList(Cyberware objModularCyberware)
        {
            string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
            List<ListItem> lstReturn = new List<ListItem>
            {
                new ListItem("None", LanguageManager.GetString("String_None", GlobalOptions.Language))
            };

            foreach (Cyberware objLoopCyberware in Cyberware.GetAllDescendants(x => x.Children))
            {
                // Make sure this has an eligible mount location and it's not the selected piece modular cyberware
                if (objLoopCyberware.HasModularMount == objModularCyberware.PlugsIntoModularMount &&
                    (objLoopCyberware.Location == objModularCyberware.Location ||
                     string.IsNullOrEmpty(objModularCyberware.Location)) &&
                    objLoopCyberware.Grade.Name == objModularCyberware.Grade.Name &&
                    objLoopCyberware != objModularCyberware)
                {
                    // Make sure it's not the place where the mount is already occupied (either by us or something else)
                    if (objLoopCyberware.Children.All(x => x.PlugsIntoModularMount != objLoopCyberware.HasModularMount))
                    {
                        string strName = objLoopCyberware.Parent?.DisplayName(GlobalOptions.Language) ??
                                         objLoopCyberware.DisplayName(GlobalOptions.Language);
                        lstReturn.Add(new ListItem(objLoopCyberware.InternalId, strName));
                    }
                }
            }

            foreach (Vehicle objLoopVehicle in Vehicles)
            {
                foreach (VehicleMod objLoopVehicleMod in objLoopVehicle.Mods)
                {
                    foreach (Cyberware objLoopCyberware in objLoopVehicleMod.Cyberware.GetAllDescendants(
                        x => x.Children))
                    {
                        // Make sure this has an eligible mount location and it's not the selected piece modular cyberware
                        if (objLoopCyberware.HasModularMount == objModularCyberware.PlugsIntoModularMount &&
                            objLoopCyberware.Location == objModularCyberware.Location &&
                            objLoopCyberware.Grade.Name == objModularCyberware.Grade.Name &&
                            objLoopCyberware != objModularCyberware)
                        {
                            // Make sure it's not the place where the mount is already occupied (either by us or something else)
                            if (objLoopCyberware.Children.All(x =>
                                x.PlugsIntoModularMount != objLoopCyberware.HasModularMount))
                            {
                                string strName = objLoopVehicle.DisplayName(GlobalOptions.Language) +
                                                 strSpaceCharacter +
                                                 (objLoopCyberware.Parent?.DisplayName(GlobalOptions.Language) ??
                                                  objLoopVehicleMod.DisplayName(GlobalOptions.Language));
                                lstReturn.Add(new ListItem(objLoopCyberware.InternalId, strName));
                            }
                        }
                    }
                }

                foreach (WeaponMount objLoopWeaponMount in objLoopVehicle.WeaponMounts)
                {
                    foreach (VehicleMod objLoopVehicleMod in objLoopWeaponMount.Mods)
                    {
                        foreach (Cyberware objLoopCyberware in objLoopVehicleMod.Cyberware.GetAllDescendants(x =>
                            x.Children))
                        {
                            // Make sure this has an eligible mount location and it's not the selected piece modular cyberware
                            if (objLoopCyberware.HasModularMount == objModularCyberware.PlugsIntoModularMount &&
                                objLoopCyberware.Location == objModularCyberware.Location &&
                                objLoopCyberware.Grade.Name == objModularCyberware.Grade.Name &&
                                objLoopCyberware != objModularCyberware)
                            {
                                // Make sure it's not the place where the mount is already occupied (either by us or something else)
                                if (objLoopCyberware.Children.All(x =>
                                    x.PlugsIntoModularMount != objLoopCyberware.HasModularMount))
                                {
                                    string strName = objLoopVehicle.DisplayName(GlobalOptions.Language) +
                                                     strSpaceCharacter +
                                                     (objLoopCyberware.Parent?.DisplayName(GlobalOptions.Language) ??
                                                      objLoopVehicleMod.DisplayName(GlobalOptions.Language));
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
            string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
            string strMessage = LanguageManager.GetString("Message_KarmaValue", strLanguage) + Environment.NewLine;
            string strKarmaString = LanguageManager.GetString("String_Karma", strLanguage);
            int intExtraKarmaToRemoveForPointBuyComparison = 0;
            intReturn = BuildKarma;
            if (BuildMethod != CharacterBuildMethod.Karma)
            {
                // Subtract extra karma cost of a metatype in priority
                intReturn -= MetatypeBP;
            }

            strMessage += Environment.NewLine + LanguageManager.GetString("Label_Base", strLanguage) + ':' +
                          strSpaceCharacter + intReturn.ToString(GlobalOptions.CultureInfo) + strSpaceCharacter +
                          strKarmaString;

            if (BuildMethod != CharacterBuildMethod.Karma)
            {
                // Zeroed to -10 because that's Human's value at default settings
                int intMetatypeQualitiesValue = -2 * Options.KarmaAttribute;
                // Karma value of all qualities (we're ignoring metatype cost because Point Buy karma costs don't line up with other methods' values)
                foreach (Quality objQuality in Qualities.Where(x =>
                    x.OriginSource == QualitySource.Metatype || x.OriginSource == QualitySource.MetatypeRemovable))
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
                foreach (CharacterAttrib objLoopAttrib in AttributeSection.AttributeList.Concat(AttributeSection
                    .SpecialAttributeList))
                {
                    string strAttributeName = objLoopAttrib.Abbrev;
                    if (strAttributeName != "ESS" &&
                        (strAttributeName != "MAGAdept" || (IsMysticAdept && Options.MysAdeptSecondMAGAttribute)) &&
                        objLoopAttrib.MetatypeMaximum > 0)
                    {
                        int intLoopAttribValue =
                            Math.Max(objLoopAttrib.Base + objLoopAttrib.FreeBase + objLoopAttrib.RawMinimum,
                                objLoopAttrib.TotalMinimum) + objLoopAttrib.AttributeValueModifiers;
                        if (intLoopAttribValue > 1)
                        {
                            intTemp += ((intLoopAttribValue + 1) * intLoopAttribValue / 2 - 1) * Options.KarmaAttribute;
                            if (strAttributeName != "MAG" && strAttributeName != "MAGAdept" &&
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

                if (intTemp - intAttributesValue + intMetatypeQualitiesValue != 0)
                {
                    strMessage += Environment.NewLine +
                                  LanguageManager.GetString("Label_SumtoTenHeritage", strLanguage) + strSpaceCharacter +
                                  (intTemp - intAttributesValue + intMetatypeQualitiesValue).ToString(GlobalOptions
                                      .CultureInfo) + strSpaceCharacter + strKarmaString;
                }

                if (intAttributesValue != 0)
                {
                    strMessage += Environment.NewLine +
                                  LanguageManager.GetString("Label_SumtoTenAttributes", strLanguage) +
                                  strSpaceCharacter + intAttributesValue.ToString(GlobalOptions.CultureInfo) +
                                  strSpaceCharacter + strKarmaString;
                }

                intReturn += intTemp;

                intTemp = 0;
                // This is where "Talent" qualities like Adept and Technomancer get added in
                foreach (Quality objQuality in Qualities.Where(x =>
                    x.OriginSource == QualitySource.Metatype || x.OriginSource == QualitySource.MetatypeRemovable))
                {
                    XmlNode xmlQualityNode = objQuality.GetNode();
                    if (xmlQualityNode?["onlyprioritygiven"] != null)
                    {
                        intTemp += Convert.ToInt32(xmlQualityNode["karma"]?.InnerText);
                    }
                }

                if (intTemp != 0)
                {
                    strMessage += Environment.NewLine + LanguageManager.GetString("String_Qualities", strLanguage) +
                                  ':' + strSpaceCharacter + intTemp.ToString(GlobalOptions.CultureInfo) +
                                  strSpaceCharacter + strKarmaString;
                    intReturn += intTemp;
                }

                // Value from free spells
                intTemp = SpellLimit * SpellKarmaCost("Spells");
                if (intTemp != 0)
                {
                    strMessage += Environment.NewLine + LanguageManager.GetString("String_FreeSpells", strLanguage) +
                                  ':' + strSpaceCharacter + intTemp.ToString(GlobalOptions.CultureInfo) +
                                  strSpaceCharacter + strKarmaString;
                    intReturn += intTemp;
                }

                // Value from free complex forms
                intTemp = CFPLimit * ComplexFormKarmaCost;
                if (intTemp != 0)
                {
                    strMessage += Environment.NewLine + LanguageManager.GetString("String_FreeCFs", strLanguage) + ':' +
                                  strSpaceCharacter + intTemp.ToString(GlobalOptions.CultureInfo) + strSpaceCharacter +
                                  strKarmaString;
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
                                intTemp += objLoopActiveSkill.Specializations.Count(x => x.Free) *
                                           Options.KarmaSpecialization;
                            else if (!objLoopActiveSkill.BuyWithKarma)
                                intTemp += objLoopActiveSkill.Specializations.Count * Options.KarmaSpecialization;
                        }
                    }
                }

                if (intTemp != 0)
                {
                    strMessage += Environment.NewLine + LanguageManager.GetString("String_SkillPoints", strLanguage) +
                                  ':' + strSpaceCharacter + intTemp.ToString(GlobalOptions.CultureInfo) +
                                  strSpaceCharacter + strKarmaString;
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
                    strMessage += Environment.NewLine +
                                  LanguageManager.GetString("String_SkillGroupPoints", strLanguage) + ':' +
                                  strSpaceCharacter + intTemp.ToString(GlobalOptions.CultureInfo) + strSpaceCharacter +
                                  strKarmaString;
                    intReturn += intTemp;
                }

                // Starting Nuyen karma value
                intTemp = decimal.ToInt32(decimal.Ceiling(StartingNuyen / Options.NuyenPerBP));
                if (intTemp != 0)
                {
                    strMessage += Environment.NewLine +
                                  LanguageManager.GetString("Checkbox_CreatePACKSKit_StartingNuyen", strLanguage) +
                                  ':' + strSpaceCharacter + intTemp.ToString(GlobalOptions.CultureInfo) +
                                  strSpaceCharacter + strKarmaString;
                    intReturn += intTemp;
                }
            }

            int intContactPointsValue = ContactPoints * Options.KarmaContact;
            if (intContactPointsValue != 0)
            {
                strMessage += Environment.NewLine + LanguageManager.GetString("String_Contacts", strLanguage) + ':' +
                              strSpaceCharacter + intContactPointsValue.ToString(GlobalOptions.CultureInfo) +
                              strSpaceCharacter + strKarmaString;
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
                    intKnowledgePointsValue += ((intLoopRating + 1) * intLoopRating / 2 - 1) *
                                               Options.KarmaImproveKnowledgeSkill;
                    if (BuildMethod == CharacterBuildMethod.LifeModule)
                        intKnowledgePointsValue += objLoopKnowledgeSkill.Specializations.Count(x => x.Free) *
                                                   Options.KarmaKnowledgeSpecialization;
                    else if (!objLoopKnowledgeSkill.BuyWithKarma)
                        intKnowledgePointsValue += objLoopKnowledgeSkill.Specializations.Count *
                                                   Options.KarmaKnowledgeSpecialization;
                }
            }

            if (intKnowledgePointsValue != 0)
            {
                strMessage += Environment.NewLine + LanguageManager.GetString("Label_KnowledgeSkills", strLanguage) +
                              ':' + strSpaceCharacter + intKnowledgePointsValue.ToString(GlobalOptions.CultureInfo) +
                              strSpaceCharacter + strKarmaString;
                intReturn += intKnowledgePointsValue;
                intExtraKarmaToRemoveForPointBuyComparison += intKnowledgePointsValue;
            }

            strMessage += Environment.NewLine + Environment.NewLine +
                          LanguageManager.GetString("String_Total", strLanguage) + ':' + strSpaceCharacter +
                          intReturn.ToString(GlobalOptions.CultureInfo) + strSpaceCharacter + strKarmaString;
            strMessage += Environment.NewLine + Environment.NewLine +
                          LanguageManager.GetString("String_TotalComparisonWithPointBuy", strLanguage) + ':' +
                          strSpaceCharacter +
                          (intReturn - intExtraKarmaToRemoveForPointBuyComparison).ToString(GlobalOptions.CultureInfo) +
                          strSpaceCharacter + strKarmaString;

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
                    if (objImprovement.ImproveType == Improvement.ImprovementType.BlackMarketDiscount &&
                        objImprovement.Enabled)
                        setNames.Add(objImprovement.ImprovedName);
                }

                using (XmlNodeList xmlCategoryList = xmlCategoryDocument.SelectNodes("/chummer/categories/category"))
                    if (xmlCategoryList != null)
                        // For each category node, split the comma-separated blackmarket attribute (if present on the node), then add each category where any of those items matches a Black Market Pipeline improvement.
                        foreach (XmlNode xmlCategoryNode in xmlCategoryList)
                        {
                            string strBlackMarketAttribute = xmlCategoryNode.Attributes?["blackmarket"]?.InnerText;
                            if (!string.IsNullOrEmpty(strBlackMarketAttribute) &&
                                strBlackMarketAttribute.Split(',').Any(x => setNames.Contains(x)))
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
                    if (objImprovement.ImproveType == Improvement.ImprovementType.BlackMarketDiscount &&
                        objImprovement.Enabled)
                        setNames.Add(objImprovement.ImprovedName);
                }

                // For each category node, split the comma-separated blackmarket attribute (if present on the node), then add each category where any of those items matches a Black Market Pipeline improvement.
                foreach (XPathNavigator xmlCategoryNode in xmlBaseChummerNode.Select("categories/category"))
                {
                    string strBlackMarketAttribute = xmlCategoryNode.SelectSingleNode("@blackmarket")?.Value;
                    if (!string.IsNullOrEmpty(strBlackMarketAttribute) &&
                        strBlackMarketAttribute.Split(',').Any(x => setNames.Contains(x)))
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
                MessageBox.Show(strMessage,
                    LanguageManager.GetString("MessageTitle_ConfirmKarmaExpense", GlobalOptions.Language),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
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
            for (TreeNode objCheckNode = objDestination;
                objCheckNode != null && objCheckNode.Level >= objDestination.Level;
                objCheckNode = objCheckNode.Parent)
                if (objCheckNode == objGearNode)
                    return;
            if (!(objGearNode.Tag is Gear objGear))
            {
                return;
            }

            // Gear cannot be moved to one if its children.
            bool blnAllowMove = true;
            TreeNode objFindNode = objDestination;
            if (objDestination.Level > 0)
            {
                do
                {
                    objFindNode = objFindNode.Parent;
                    if (objFindNode.Tag == objGear)
                    {
                        blnAllowMove = false;
                        break;
                    }
                } while (objFindNode.Level > 0);
            }

            if (!blnAllowMove)
                return;

            // Remove the Gear from the character.
            if (objGear.Parent is IHasChildren<Gear> parent)
                parent.Children.Remove(objGear);
            else
                Gear.Remove(objGear);

            if (objDestination.Tag is Location objLocation)
            {
                // The Gear was moved to a location, so add it to the character instead.
                objGear.Location = objLocation;
                objLocation.Children.Add(objGear);
                Gear.Add(objGear);
            }
            else if (objDestination.Tag is Gear objParent)
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
            if (nodeToMove?.Tag is Gear objGear)
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
                    intNewIndex = Math.Min(intNewIndex, Gear.Count);
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
            if (objDestination != null)
            {
                TreeNode objNewParent = objDestination;
                while (objNewParent.Level > 0)
                    objNewParent = objNewParent.Parent;
                intNewIndex = objNewParent.Index;
            }

            if (intNewIndex == 0)
                return;

            if (!(nodOldNode.Tag is Location objLocation)) return;
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
            if (objDestination != null)
            {
                TreeNode objNewParent = objDestination;
                while (objNewParent.Level > 0)
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
                else if (objNewParent.Tag is string)
                {
                    objArmor.Location = null;
                    intNewIndex = Math.Min(intNewIndex, Armor.Count);
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
            if (objDestination != null)
            {
                TreeNode objNewParent = objDestination;
                while (objNewParent.Level > 0)
                    objNewParent = objNewParent.Parent;
                intNewIndex = objNewParent.Index;
            }

            if (intNewIndex == 0)
                return;

            if (!(nodOldNode.Tag is Location objLocation)) return;
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
                else if (objNewParent.Tag is string)
                {
                    objWeapon.Location = null;
                    intNewIndex = Math.Min(intNewIndex, Weapons.Count);
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
            if (objDestination != null)
            {
                TreeNode objNewParent = objDestination;
                while (objNewParent.Level > 0)
                    objNewParent = objNewParent.Parent;
                intNewIndex = objNewParent.Index;
            }

            if (intNewIndex == 0)
                return;

            if (!(nodOldNode.Tag is Location objLocation)) return;
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
            if (nodeToMove?.Tag is Vehicle objVehicle)
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
                    intNewIndex = Math.Min(intNewIndex, Weapons.Count);
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
                if (objGear.Parent is IHasChildren<Gear> parent)
                    parent.Children.Remove(objGear);
                else if (objOldCyberware != null)
                    objOldCyberware.Gear.Remove(objGear);
                else if (objOldWeaponAccessory != null)
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
                do
                {
                    nodVehicleNode = nodVehicleNode.Parent;
                } while (nodVehicleNode.Level > 1);

                // Determine if this is a Location in the destination Vehicle.
                if (nodDestination.Tag is Location objLocation)
                {
                    // Remove the Gear from the Vehicle.
                    if (objGear.Parent is IHasChildren<Gear> parent)
                        parent.Children.Remove(objGear);
                    else if (objOldCyberware != null)
                        objOldCyberware.Gear.Remove(objGear);
                    else if (objOldWeaponAccessory != null)
                        objOldWeaponAccessory.Gear.Remove(objGear);
                    else
                        objOldVehicle.Gear.Remove(objGear);

                    // Add the Gear to the Vehicle and set its Location.
                    objGear.Location = objLocation;
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
        public void ClearMagic(bool blnKeepAdeptEligible)
        {
            if (Improvements.All(x => !x.Enabled || (x.ImproveType != Improvement.ImprovementType.FreeSpells &&
                                                     x.ImproveType != Improvement.ImprovementType.FreeSpellsATT &&
                                                     x.ImproveType != Improvement.ImprovementType.FreeSpellsSkill)))
            {
                // Run through all of the Spells and remove their Improvements.
                for (int i = Spells.Count - 1; i >= 0; --i)
                {
                    if (i < Spells.Count)
                    {
                        Spell objToRemove = Spells[i];
                        if (objToRemove.Grade == 0)
                        {
                            if (blnKeepAdeptEligible && objToRemove.Category == "Rituals" &&
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
                        ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.Power,
                            objToRemove.InternalId);
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
                        ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.ComplexForm,
                            objToRemove.InternalId);
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
            set
            {
                if (_strFileName != value)
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
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether or not the character has been saved as Created and can no longer be modified using the Build system.
        /// </summary>
        public bool Created
        {
            get => _blnCreated;
            set
            {
                if (_blnCreated != value)
                {
                    _blnCreated = value;
                    OnPropertyChanged();
                }
            }
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
                    OnPropertyChanged();
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
                    value = -1;

                if (_intMainMugshotIndex != value)
                {
                    _intMainMugshotIndex = value;
                    OnPropertyChanged();
                }
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
                Parallel.For(0, lstMugshotsBase64.Count,
                    i =>
                    {
                        objMugshotImages[i] = lstMugshotsBase64[i]
                            .ToImage(System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
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
                        MessageBox.Show(LanguageManager.GetString("Message_Insufficient_Permissions_Warning",
                            GlobalOptions.Language));
                    }
                }

                Guid guiImage = Guid.NewGuid();
                string imgMugshotPath = Path.Combine(strMugshotsDirectoryPath, guiImage.ToString("N") + ".img");
                Image imgMainMugshot = MainMugshot;
                if (imgMainMugshot != null)
                {
                    imgMainMugshot.Save(imgMugshotPath);
                    // <mainmugshotpath />
                    objWriter.WriteElementString("mainmugshotpath",
                        "file://" + imgMugshotPath.Replace(Path.DirectorySeparatorChar, '/'));
                    // <mainmugshotbase64 />
                    objWriter.WriteElementString("mainmugshotbase64", imgMainMugshot.ToBase64String());
                }

                // <othermugshots>
                objWriter.WriteElementString("hasothermugshots",
                    (imgMainMugshot == null || Mugshots.Count > 1).ToString());
                objWriter.WriteStartElement("othermugshots");
                for (int i = 0; i < Mugshots.Count; ++i)
                {
                    if (i == MainMugshotIndex)
                        continue;
                    Image imgMugshot = Mugshots[i];
                    objWriter.WriteStartElement("mugshot");

                    objWriter.WriteElementString("stringbase64", imgMugshot.ToBase64String());

                    imgMugshotPath = Path.Combine(strMugshotsDirectoryPath,
                        guiImage.ToString("N") + i.ToString() + ".img");
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
        public string GameplayOption
        {
            get => _strGameplayOption;
            set
            {
                if (_strGameplayOption != value)
                {
                    _strGameplayOption = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Quality Limit conferred by the Character's Gameplay Option
        /// </summary>
        public int GameplayOptionQualityLimit
        {
            get => _intGameplayOptionQualityLimit;
            set
            {
                if (_intGameplayOptionQualityLimit != value)
                {
                    _intGameplayOptionQualityLimit = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Character's maximum karma at character creation.
        /// </summary>
        public int MaxKarma
        {
            get => _intMaxKarma;
            set
            {
                if (_intMaxKarma != value)
                {
                    _intMaxKarma = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Character's maximum nuyen at character creation.
        /// </summary>
        public decimal MaxNuyen
        {
            get => _decMaxNuyen;
            set
            {
                if (_decMaxNuyen != value)
                {
                    _decMaxNuyen = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Character's contact point multiplier.
        /// </summary>
        public int ContactMultiplier
        {
            get => _intContactMultiplier;
            set
            {
                if (_intContactMultiplier != value)
                {
                    _intContactMultiplier = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Character's Metatype Priority.
        /// </summary>
        public string MetatypePriority
        {
            get => _strPriorityMetatype;
            set
            {
                if (_strPriorityMetatype != value)
                {
                    _strPriorityMetatype = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Character's Attributes Priority.
        /// </summary>
        public string AttributesPriority
        {
            get => _strPriorityAttributes;
            set
            {
                if (_strPriorityAttributes != value)
                {
                    _strPriorityAttributes = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Character's Special Priority.
        /// </summary>
        public string SpecialPriority
        {
            get => _strPrioritySpecial;
            set
            {
                if (_strPrioritySpecial != value)
                {
                    _strPrioritySpecial = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Character's Skills Priority.
        /// </summary>
        public string SkillsPriority
        {
            get => _strPrioritySkills;
            set
            {
                if (_strPrioritySkills != value)
                {
                    _strPrioritySkills = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Character's Resources Priority.
        /// </summary>
        public string ResourcesPriority
        {
            get => _strPriorityResources;
            set
            {
                if (_strPriorityResources != value)
                {
                    _strPriorityResources = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Character's Resources Priority.
        /// </summary>
        public string TalentPriority
        {
            get => _strPriorityTalent;
            set
            {
                if (_strPriorityTalent != value)
                {
                    _strPriorityTalent = value;
                    OnPropertyChanged();
                }
            }
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
                    OnPropertyChanged();
                }
            }
        }

        private string _strCachedCharacterGrammaticGender = string.Empty;

        public string CharacterGrammaticGender
        {
            get
            {
                if (!string.IsNullOrEmpty(_strCachedCharacterGrammaticGender))
                    return _strCachedCharacterGrammaticGender;
                switch (LanguageManager.ReverseTranslateExtra(Sex, GlobalOptions.Language).ToLower())
                {
                    case "m":
                    case "male":
                    case "man":
                    case "boy":
                    case "lord":
                    case "gentleman":
                    case "guy":
                        return _strCachedCharacterGrammaticGender = "male";
                    case "f":
                    case "female":
                    case "woman":
                    case "girl":
                    case "lady":
                    case "gal":
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
                if (_strAge != value)
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
                if (_strEyes != value)
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
                if (_strHeight != value)
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
                if (_strWeight != value)
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
                if (_strSkin != value)
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
                if (_strHair != value)
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
                if (_strDescription != value)
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
                if (_strBackground != value)
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
                if (_strConcept != value)
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
                if (_strNotes != value)
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
                if (_strGameNotes != value)
                {
                    _strGameNotes = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// What is the Characters prefered hand
        /// </summary>
        public string PrimaryArm
        {
            get => _strPrimaryArm;
            set
            {
                if (_strPrimaryArm != value)
                {
                    _strPrimaryArm = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Player name.
        /// </summary>
        public string PlayerName
        {
            get => _strPlayerName;
            set
            {
                if (_strPlayerName != value)
                {
                    _strPlayerName = value;
                    OnPropertyChanged();
                }
            }
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
            set
            {
                if (_intStreetCred != value)
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
                if (_intBurntStreetCred != value)
                {
                    _intBurntStreetCred = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Notoriety.
        /// </summary>
        public int Notoriety
        {
            get => _intNotoriety;
            set
            {
                if (_intNotoriety != value)
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
                if (_intPublicAwareness != value)
                {
                    _intPublicAwareness = value;
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
                        OnPropertyChanged();
                    }
                }
                else if (_intPhysicalCMFilled != value)
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
                if (IsAI && HomeNode != null)
                {
                    // A.I. do not have a Stun Condition Monitor, but they do have a Matrix Condition Monitor if they are in their home node.
                    return HomeNode.MatrixCMFilled;
                }

                return _intStunCMFilled;
            }
            set
            {
                if (IsAI && HomeNode != null)
                {
                    // A.I. do not have a Stun Condition Monitor, but they do have a Matrix Condition Monitor if they are in their home node.
                    if (HomeNode.MatrixCMFilled != value)
                    {
                        HomeNode.MatrixCMFilled = value;
                        OnPropertyChanged();
                    }
                }
                else if (_intStunCMFilled != value)
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
        public bool IgnoreRules
        {
            get => _blnIgnoreRules;
            set
            {
                if (_blnIgnoreRules != value)
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
                if (_intCachedContactPoints == int.MinValue)
                {
                    _intCachedContactPoints = (_objOptions.UseTotalValueForFreeContacts ? CHA.TotalValue : CHA.Value) *
                                              ContactMultiplier;
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
                if (_intContactPointsUsed != value)
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
                if (_intCFPLimit != value)
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
                if (_intAINormalProgramLimit != value)
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
                if (_intAIAdvancedProgramLimit != value)
                {
                    _intAIAdvancedProgramLimit = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Spell Limit.
        /// </summary>
        public int SpellLimit
        {
            get => _intSpellLimit;
            set
            {
                if (_intSpellLimit != value)
                {
                    _intSpellLimit = value;
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
                if (_intKarma != value)
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
                if (_intSpecial != value)
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
                if (_intTotalSpecial != value)
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
                if (_intAttributes != value)
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
                if (_intTotalAttributes != value)
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
        public int CareerKarma
        {
            get
            {
                if (_intCachedCareerKarma != int.MinValue)
                    return _intCachedCareerKarma;

                int intKarma = 0;

                foreach (ExpenseLogEntry objEntry in _lstExpenseLog)
                {
                    // Since we're only interested in the amount they have earned, only count values that are greater than 0 and are not refunds.
                    if (objEntry.Type == ExpenseType.Karma && objEntry.Amount > 0 && !objEntry.Refund)
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
                if (_decCachedCareerNuyen != decimal.MinValue)
                    return _decCachedCareerNuyen;

                decimal decNuyen = 0;

                foreach (ExpenseLogEntry objEntry in _lstExpenseLog)
                {
                    // Since we're only interested in the amount they have earned, only count values that are greater than 0 and are not refunds.
                    if (objEntry.Type == ExpenseType.Nuyen && objEntry.Amount > 0 && !objEntry.Refund)
                        decNuyen += objEntry.Amount;
                }

                return _decCachedCareerNuyen = decNuyen;
            }
        }

        public string DisplayCareerNuyen =>
            CareerNuyen.ToString(_objOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';

        /// <summary>
        /// Whether or not the character is a Critter.
        /// </summary>
        public bool IsCritter
        {
            get => _blnIsCritter;
            set
            {
                if (_blnIsCritter != value)
                {
                    _blnIsCritter = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The highest number of free metagenetic qualities the character can have.
        /// </summary>
        public int MetageneticLimit => ImprovementManager.ValueOf(this, Improvement.ImprovementType.MetageneticLimit);

        /// <summary>
        /// Whether or not the character is possessed by a Spirit.
        /// </summary>
        public bool Possessed
        {
            get => _blnPossessed;
            set
            {
                if (_blnPossessed != value)
                {
                    _blnPossessed = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Maximum item Availability for new characters.
        /// </summary>
        public int MaximumAvailability
        {
            get => _intMaxAvail;
            set
            {
                if (_intMaxAvail != value)
                {
                    _intMaxAvail = value;
                    OnPropertyChanged();
                }
            }
        }

        public int SpellKarmaCost(string strCategory = "")
        {
            int intReturn = Options.KarmaSpell;

            decimal decMultiplier = 1.0m;
            foreach (Improvement objLoopImprovement in Improvements.Where(imp =>
                (imp.ImproveType == Improvement.ImprovementType.NewSpellKarmaCost ||
                 imp.ImproveType == Improvement.ImprovementType.NewSpellKarmaCostMultiplier) &&
                imp.ImprovedName == strCategory))
            {
                if (objLoopImprovement.Enabled && (string.IsNullOrEmpty(objLoopImprovement.Condition) ||
                                                   (objLoopImprovement.Condition == "career") == Created ||
                                                   (objLoopImprovement.Condition == "create") != Created))
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
                    if (objLoopImprovement.Enabled && (string.IsNullOrEmpty(objLoopImprovement.Condition) ||
                                                       (objLoopImprovement.Condition == "career") == Created ||
                                                       (objLoopImprovement.Condition == "create") != Created))
                    {
                        if (objLoopImprovement.ImproveType == Improvement.ImprovementType.NewComplexFormKarmaCost)
                            intReturn += objLoopImprovement.Value;
                        if (objLoopImprovement.ImproveType ==
                            Improvement.ImprovementType.NewComplexFormKarmaCostMultiplier)
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
                    if (objLoopImprovement.Enabled && (string.IsNullOrEmpty(objLoopImprovement.Condition) ||
                                                       (objLoopImprovement.Condition == "career") == Created ||
                                                       (objLoopImprovement.Condition == "create") != Created))
                    {
                        if (objLoopImprovement.ImproveType == Improvement.ImprovementType.NewAIProgramKarmaCost)
                            intReturn += objLoopImprovement.Value;
                        if (objLoopImprovement.ImproveType ==
                            Improvement.ImprovementType.NewAIProgramKarmaCostMultiplier)
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
                    if (objLoopImprovement.Enabled && (string.IsNullOrEmpty(objLoopImprovement.Condition) ||
                                                       (objLoopImprovement.Condition == "career") == Created ||
                                                       (objLoopImprovement.Condition == "create") != Created))
                    {
                        if (objLoopImprovement.ImproveType == Improvement.ImprovementType.NewAIAdvancedProgramKarmaCost)
                            intReturn += objLoopImprovement.Value;
                        if (objLoopImprovement.ImproveType ==
                            Improvement.ImprovementType.NewAIAdvancedProgramKarmaCostMultiplier)
                            decMultiplier *= objLoopImprovement.Value / 100.0m;
                    }
                }

                if (decMultiplier != 1.0m)
                    intReturn = decimal.ToInt32(decimal.Ceiling(intReturn * decMultiplier));

                return Math.Max(intReturn, 0);
            }
        }

        private int _intCachedAmbidextrous = -1;

        public bool Ambidextrous
        {
            get
            {
                if (_intCachedAmbidextrous < 0)
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
                            bool blnCountOnlyPriorityOrMetatypeGivenBonuses = Improvements.Any(x =>
                                x.ImproveType == Improvement.ImprovementType.Attribute && x.ImprovedName == "MAG" &&
                                (x.ImproveSource == Improvement.ImprovementSource.Metatype ||
                                 x.ImproveSource == Improvement.ImprovementSource.Metavariant ||
                                 x.ImproveSource == Improvement.ImprovementSource.Heritage) && x.Enabled);
                            Dictionary<string, decimal> dicImprovementEssencePenalties =
                                new Dictionary<string, decimal>();
                            foreach (Improvement objImprovement in Improvements)
                            {
                                if ((!blnCountOnlyPriorityOrMetatypeGivenBonuses ||
                                     objImprovement.ImproveSource == Improvement.ImprovementSource.Metatype ||
                                     objImprovement.ImproveSource == Improvement.ImprovementSource.Metavariant ||
                                     objImprovement.ImproveSource == Improvement.ImprovementSource.Heritage) &&
                                    objImprovement.Enabled)
                                {
                                    decimal decLoopEssencePenalty = 0;
                                    if (objImprovement.ImproveType == Improvement.ImprovementType.EssencePenalty)
                                    {
                                        decLoopEssencePenalty += objImprovement.Value;
                                    }
                                    else if (objImprovement.ImproveType ==
                                             Improvement.ImprovementType.EssencePenaltyT100 ||
                                             objImprovement.ImproveType ==
                                             Improvement.ImprovementType.EssencePenaltyMAGOnlyT100)
                                    {
                                        decLoopEssencePenalty += Convert.ToDecimal(objImprovement.Value) / 100.0m;
                                    }

                                    if (decLoopEssencePenalty != 0)
                                    {
                                        if (dicImprovementEssencePenalties.ContainsKey(objImprovement.SourceName))
                                            dicImprovementEssencePenalties[objImprovement.SourceName] =
                                                dicImprovementEssencePenalties[objImprovement.SourceName] +
                                                decLoopEssencePenalty;
                                        else
                                            dicImprovementEssencePenalties.Add(objImprovement.SourceName,
                                                decLoopEssencePenalty);
                                    }
                                }
                            }

                            if (dicImprovementEssencePenalties.Count > 0)
                                EssenceAtSpecialStart =
                                    ESS.MetatypeMaximum + dicImprovementEssencePenalties.Values.Min();
                            else
                                EssenceAtSpecialStart = ESS.MetatypeMaximum;
                        }
                    }
                    else
                    {
                        if (!RESEnabled)
                        {
                            ClearInitiations();
                            MagicTradition.ResetTradition();
                        }
                        else
                        {
                            XmlNode xmlTraditionListDataNode = XmlManager.Load("streams.xml").SelectSingleNode("/chummer/traditions");
                            if (xmlTraditionListDataNode != null)
                            {
                                XmlNode xmlTraditionDataNode = xmlTraditionListDataNode.SelectSingleNode("tradition[name = \"Default\"]");
                                if (xmlTraditionDataNode != null)
                                {
                                    if (!MagicTradition.Create(xmlTraditionDataNode, true))
                                        MagicTradition.ResetTradition();
                                }
                                else
                                    MagicTradition.ResetTradition();
                            }
                            else
                                MagicTradition.ResetTradition();
                        }
                        if (!Created && !RESEnabled && !DEPEnabled)
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
            (Created ? 2 : 1) * (Options.SpiritForceBasedOnTotalMAG ? MAG.TotalValue : MAG.Value);

        /// <summary>
        /// Maximum level of sprites compilable/registerable by the character. Limited to RES at creation.
        /// </summary>
        public int MaxSpriteLevel => (Created ? 2 : 1) * RES.TotalValue;

        /// <summary>
        /// Amount of Power Points for Mystic Adepts.
        /// </summary>
        public int MysticAdeptPowerPoints
        {
            get => _intMAGAdept;
            set
            {
                int intNewValue = Math.Min(value, MAG.TotalValue);
                if (_intMAGAdept != intNewValue)
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
                if (_decCachedPowerPointsUsed != decimal.MinValue)
                    return _decCachedPowerPointsUsed;
                return _decCachedPowerPointsUsed = Powers.AsParallel().Sum(objPower => objPower.PowerPoints);
            }
        }

        public string DisplayPowerPointsRemaining
        {
            get
            {
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                return PowerPointsTotal.ToString(GlobalOptions.CultureInfo) + strSpaceCharacter + '(' +
                       (PowerPointsTotal - PowerPointsUsed).ToString(GlobalOptions.CultureInfo) + strSpaceCharacter +
                       LanguageManager.GetString("String_Remaining", GlobalOptions.Language) + ')';
            }
        }

        public bool AnyPowerAdeptWayDiscountEnabled => Powers.Any(objPower => objPower.AdeptWayDiscountEnabled);

        /// <summary>
        /// Magician's Tradition.
        /// </summary>
        public Tradition MagicTradition => _objTradition;
        
        /// <summary>
        /// Initiate Grade.
        /// </summary>
        public int InitiateGrade
        {
            get => _intInitiateGrade;
            set
            {
                if (_intInitiateGrade != value)
                {
                    _intInitiateGrade = value;
                    OnPropertyChanged();
                }
            }
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
                            bool blnCountOnlyPriorityOrMetatypeGivenBonuses = Improvements.Any(x =>
                                x.ImproveType == Improvement.ImprovementType.Attribute && x.ImprovedName == "RES" &&
                                (x.ImproveSource == Improvement.ImprovementSource.Metatype ||
                                 x.ImproveSource == Improvement.ImprovementSource.Metavariant ||
                                 x.ImproveSource == Improvement.ImprovementSource.Heritage) && x.Enabled);
                            Dictionary<string, decimal> dicImprovementEssencePenalties =
                                new Dictionary<string, decimal>();
                            foreach (Improvement objImprovement in Improvements)
                            {
                                if ((!blnCountOnlyPriorityOrMetatypeGivenBonuses ||
                                     objImprovement.ImproveSource == Improvement.ImprovementSource.Metatype ||
                                     objImprovement.ImproveSource == Improvement.ImprovementSource.Metavariant ||
                                     objImprovement.ImproveSource == Improvement.ImprovementSource.Heritage) &&
                                    objImprovement.Enabled)
                                {
                                    decimal decLoopEssencePenalty = 0;
                                    if (objImprovement.ImproveType == Improvement.ImprovementType.EssencePenalty)
                                    {
                                        decLoopEssencePenalty += objImprovement.Value;
                                    }
                                    else if (objImprovement.ImproveType ==
                                             Improvement.ImprovementType.EssencePenaltyT100)
                                    {
                                        decLoopEssencePenalty += Convert.ToDecimal(objImprovement.Value) / 100.0m;
                                    }

                                    if (decLoopEssencePenalty != 0)
                                    {
                                        if (dicImprovementEssencePenalties.ContainsKey(objImprovement.SourceName))
                                            dicImprovementEssencePenalties[objImprovement.SourceName] =
                                                dicImprovementEssencePenalties[objImprovement.SourceName] +
                                                decLoopEssencePenalty;
                                        else
                                            dicImprovementEssencePenalties.Add(objImprovement.SourceName,
                                                decLoopEssencePenalty);
                                    }
                                }
                            }

                            if (dicImprovementEssencePenalties.Count > 0)
                                EssenceAtSpecialStart =
                                    ESS.MetatypeMaximum + dicImprovementEssencePenalties.Values.Min();
                            else
                                EssenceAtSpecialStart = ESS.MetatypeMaximum;
                        }
                        
                        XmlNode xmlTraditionListDataNode = XmlManager.Load("streams.xml").SelectSingleNode("/chummer/traditions");
                        if (xmlTraditionListDataNode != null)
                        {
                            XmlNode xmlTraditionDataNode = xmlTraditionListDataNode.SelectSingleNode("tradition[name = \"Default\"]");
                            if (xmlTraditionDataNode != null)
                            {
                                if (!MagicTradition.Create(xmlTraditionDataNode, true))
                                    MagicTradition.ResetTradition();
                            }
                            else
                            {
                                xmlTraditionDataNode = xmlTraditionListDataNode.SelectSingleNode("tradition");
                                if (xmlTraditionDataNode != null)
                                {
                                    if (!MagicTradition.Create(xmlTraditionDataNode, true))
                                        MagicTradition.ResetTradition();
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!MAGEnabled)
                        {
                            ClearInitiations();
                            MagicTradition.ResetTradition();
                        }
                        else
                        {
                            XmlNode xmlTraditionListDataNode = XmlManager.Load("traditions.xml").SelectSingleNode("/chummer/traditions");
                            if (xmlTraditionListDataNode != null)
                            {
                                XmlNode xmlTraditionDataNode = xmlTraditionListDataNode.SelectSingleNode("tradition[id = \"" + Tradition.CustomMagicalTraditionGuid + "\"]");
                                if (xmlTraditionDataNode != null)
                                {
                                    if (!MagicTradition.Create(xmlTraditionDataNode))
                                        MagicTradition.ResetTradition();
                                }
                                else
                                    MagicTradition.ResetTradition();
                            }
                            else
                                MagicTradition.ResetTradition();
                        }
                        if (!Created && !DEPEnabled && !MAGEnabled)
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
                            bool blnCountOnlyPriorityOrMetatypeGivenBonuses = Improvements.Any(x =>
                                x.ImproveType == Improvement.ImprovementType.Attribute && x.ImprovedName == "DEP" &&
                                (x.ImproveSource == Improvement.ImprovementSource.Metatype ||
                                 x.ImproveSource == Improvement.ImprovementSource.Metavariant ||
                                 x.ImproveSource == Improvement.ImprovementSource.Heritage) && x.Enabled);
                            Dictionary<string, decimal> dicImprovementEssencePenalties =
                                new Dictionary<string, decimal>();
                            foreach (Improvement objImprovement in Improvements)
                            {
                                if ((!blnCountOnlyPriorityOrMetatypeGivenBonuses ||
                                     objImprovement.ImproveSource == Improvement.ImprovementSource.Metatype ||
                                     objImprovement.ImproveSource == Improvement.ImprovementSource.Metavariant ||
                                     objImprovement.ImproveSource == Improvement.ImprovementSource.Heritage) &&
                                    objImprovement.Enabled)
                                {
                                    decimal decLoopEssencePenalty = 0;
                                    if (objImprovement.ImproveType == Improvement.ImprovementType.EssencePenalty)
                                    {
                                        decLoopEssencePenalty += objImprovement.Value;
                                    }
                                    else if (objImprovement.ImproveType ==
                                             Improvement.ImprovementType.EssencePenaltyT100)
                                    {
                                        decLoopEssencePenalty += Convert.ToDecimal(objImprovement.Value) / 100.0m;
                                    }

                                    if (decLoopEssencePenalty != 0)
                                    {
                                        if (dicImprovementEssencePenalties.ContainsKey(objImprovement.SourceName))
                                            dicImprovementEssencePenalties[objImprovement.SourceName] =
                                                dicImprovementEssencePenalties[objImprovement.SourceName] +
                                                decLoopEssencePenalty;
                                        else
                                            dicImprovementEssencePenalties.Add(objImprovement.SourceName,
                                                decLoopEssencePenalty);
                                    }
                                }
                            }

                            if (dicImprovementEssencePenalties.Count > 0)
                                EssenceAtSpecialStart =
                                    ESS.MetatypeMaximum + dicImprovementEssencePenalties.Values.Min();
                            else
                                EssenceAtSpecialStart = ESS.MetatypeMaximum;
                        }
                    }
                    else if (!Created && !RESEnabled && !MAGEnabled)
                        EssenceAtSpecialStart = decimal.MinValue;

                    OnPropertyChanged();
                }
            }
        }

        public bool IsAI => DEPEnabled && BOD.MetatypeMaximum == 0;

        /// <summary>
        /// Submersion Grade.
        /// </summary>
        public int SubmersionGrade
        {
            get => _intSubmersionGrade;
            set
            {
                if (_intSubmersionGrade != value)
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
                if (_blnGroupMember != value)
                {
                    _blnGroupMember = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The name of the Group the Initiate has joined.
        /// </summary>
        public string GroupName
        {
            get => _strGroupName;
            set
            {
                if (_strGroupName != value)
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
                if (_strGroupNotes != value)
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
                if (_decEssenceAtSpecialStart != value)
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
            if (!blnForMAGPenalty && _decCachedEssence != decimal.MinValue)
                return _decCachedEssence;
            // If the character has a fixed Essence Improvement, permanently fix their Essence at its value.
            if (_lstImprovements.Any(objImprovement =>
                objImprovement.ImproveType == Improvement.ImprovementType.CyborgEssence && objImprovement.Enabled))
            {
                if (blnForMAGPenalty)
                    return 0.1m;
                return _decCachedEssence = 0.1m;
            }

            decimal decESS = ESS.MetatypeMaximum;
            decESS += Convert.ToDecimal(ImprovementManager.ValueOf(this, Improvement.ImprovementType.EssencePenalty));
            decESS += Convert.ToDecimal(
                          ImprovementManager.ValueOf(this, Improvement.ImprovementType.EssencePenaltyT100)) / 100.0m;
            if (blnForMAGPenalty)
                decESS += Convert.ToDecimal(ImprovementManager.ValueOf(this,
                              Improvement.ImprovementType.EssencePenaltyMAGOnlyT100)) / 100.0m;

            // Run through all of the pieces of Cyberware and include their Essence cost.
            decESS -= Cyberware.AsParallel().Sum(objCyberware => objCyberware.CalculatedESS());

            //1781 Essence is not printing
            //ESS.Base = Convert.ToInt32(decESS); -- Disabled becauses this messes up Character Validity, and it really shouldn't be what "Base" of an attribute is supposed to be (it's supposed to be extra levels gained)

            if (blnForMAGPenalty)
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
                if (_decCachedCyberwareEssence != decimal.MinValue)
                    return _decCachedCyberwareEssence;
                // Run through all of the pieces of Cyberware and include their Essence cost. Cyberware and Bioware costs are calculated separately.
                return _decCachedCyberwareEssence = Cyberware
                    .Where(objCyberware => !objCyberware.SourceID.Equals(Backend.Equipment.Cyberware.EssenceHoleGUID) &&
                                           objCyberware.SourceType == Improvement.ImprovementSource.Cyberware)
                    .AsParallel().Sum(objCyberware => objCyberware.CalculatedESS());
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
                if (_decCachedBiowareEssence != decimal.MinValue)
                    return _decCachedBiowareEssence;
                // Run through all of the pieces of Cyberware and include their Essence cost. Cyberware and Bioware costs are calculated separately.
                return _decCachedBiowareEssence = Cyberware
                    .Where(objCyberware => !objCyberware.SourceID.Equals(Backend.Equipment.Cyberware.EssenceHoleGUID) &&
                                           objCyberware.SourceType == Improvement.ImprovementSource.Bioware)
                    .AsParallel().Sum(objCyberware => objCyberware.CalculatedESS());
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
                if (_decCachedEssenceHole != decimal.MinValue)
                    return _decCachedEssenceHole;
                // Find the total Essence Cost of all Essence Hole objects.
                return _decCachedEssenceHole = Cyberware
                    .Where(objCyberware => objCyberware.SourceID.Equals(Backend.Equipment.Cyberware.EssenceHoleGUID))
                    .AsParallel().Sum(objCyberware => objCyberware.CalculatedESS());
            }
        }

        private decimal _decCachedPrototypeTranshumanEssenceUsed = decimal.MinValue;

        /// <summary>
        /// Essence consumed by Prototype Transhuman 'ware
        /// </summary>
        public decimal PrototypeTranshumanEssenceUsed
        {
            get
            {
                if (_decCachedPrototypeTranshumanEssenceUsed != decimal.MinValue)
                    return _decCachedPrototypeTranshumanEssenceUsed;
                // Find the total Essence Cost of all Prototype Transhuman 'ware.
                return _decCachedPrototypeTranshumanEssenceUsed = Cyberware
                    .Where(objCyberware => objCyberware.PrototypeTranshuman).AsParallel()
                    .Sum(objCyberware => objCyberware.CalculatedESS(false));
            }
        }

        public string DisplayEssence => Essence().ToString(_objOptions.EssenceFormat, GlobalOptions.CultureInfo);

        public string DisplayCyberwareEssence =>
            CyberwareEssence.ToString(_objOptions.EssenceFormat, GlobalOptions.CultureInfo);

        public string DisplayBiowareEssence =>
            BiowareEssence.ToString(_objOptions.EssenceFormat, GlobalOptions.CultureInfo);

        public string DisplayEssenceHole => EssenceHole.ToString(_objOptions.EssenceFormat, GlobalOptions.CultureInfo);

        public string DisplayPrototypeTranshumanEssenceUsed =>
            PrototypeTranshumanEssenceUsed.ToString(_objOptions.EssenceFormat, GlobalOptions.CultureInfo) + " / " +
            PrototypeTranshuman.ToString(_objOptions.EssenceFormat, GlobalOptions.CultureInfo);

        #region Initiative

        #region Physical

        /// <summary>
        /// Physical Initiative.
        /// </summary>
        public string Initiative => string.Format(LanguageManager.GetString("String_Initiative", GlobalOptions.Language),
            InitiativeValue.ToString(GlobalOptions.CultureInfo),
            InitiativeDice.ToString(GlobalOptions.CultureInfo));

        public string GetInitiative(CultureInfo objCulture, string strLanguage)
        {
            return string.Format(LanguageManager.GetString("String_Initiative", strLanguage),
                InitiativeValue.ToString(objCulture),
                InitiativeDice.ToString(objCulture));
        }

        public string InitiativeToolTip
        {
            get
            {
                int intINTAttributeModifiers = INT.AttributeModifiers;
                int intREAAttributeModifiers = REA.AttributeModifiers;
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);

                string strInit = REA.DisplayAbbrev + strSpaceCharacter + '(' + REA.Value.ToString(GlobalOptions.CultureInfo) + ')'
                                 + strSpaceCharacter + '+' + strSpaceCharacter + INT.DisplayAbbrev + strSpaceCharacter + '(' + INT.Value.ToString(GlobalOptions.CultureInfo) + ')';
                if (ImprovementManager.ValueOf(this, Improvement.ImprovementType.Initiative) != 0 || intINTAttributeModifiers != 0 || intREAAttributeModifiers != 0 || WoundModifier != 0)
                {
                    strInit += strSpaceCharacter + '+' + strSpaceCharacter + LanguageManager.GetString("Tip_Modifiers", GlobalOptions.Language) + strSpaceCharacter +
                               '(' + (ImprovementManager.ValueOf(this, Improvement.ImprovementType.Initiative) + intINTAttributeModifiers + intREAAttributeModifiers + WoundModifier).ToString(GlobalOptions.CultureInfo) + ')';
                }

                return string.Format(LanguageManager.GetString("String_Initiative", GlobalOptions.Language), strInit, InitiativeDice.ToString(GlobalOptions.CultureInfo));
            }
        }

        /// <summary>
        /// Initiative Dice.
        /// </summary>
        public int InitiativeDice
        {
            get
            {
                int intExtraIP = 1 + ImprovementManager.ValueOf(this, Improvement.ImprovementType.InitiativeDice) +
                                 ImprovementManager.ValueOf(this, Improvement.ImprovementType.InitiativeDiceAdd);

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
        public string AstralInitiative => string.Format(LanguageManager.GetString("String_Initiative", GlobalOptions.Language),
                AstralInitiativeValue.ToString(GlobalOptions.CultureInfo),
                AstralInitiativeDice.ToString(GlobalOptions.CultureInfo));

        public string GetAstralInitiative(CultureInfo objCulture, string strLanguageToPrint)
        {
            return string.Format(LanguageManager.GetString("String_Initiative", strLanguageToPrint),
                AstralInitiativeValue.ToString(objCulture),
                AstralInitiativeDice.ToString(objCulture));
        }

        public string AstralInitiativeToolTip
        {
            get
            {
                if (!MAGEnabled)
                    return string.Empty;
                int intINTAttributeModifiers = INT.AttributeModifiers;
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                string strInit = INT.DisplayAbbrev + strSpaceCharacter + '(' + INT.Value.ToString(GlobalOptions.CultureInfo) + ')' +
                                 strSpaceCharacter + '×' + strSpaceCharacter + 2.ToString(GlobalOptions.CultureInfo);
                if (intINTAttributeModifiers != 0 || WoundModifier != 0)
                    strInit += LanguageManager.GetString("Tip_Modifiers", GlobalOptions.Language) + strSpaceCharacter + '(' + (intINTAttributeModifiers + WoundModifier).ToString(GlobalOptions.CultureInfo) + ')';
                return string.Format(LanguageManager.GetString("String_Initiative", GlobalOptions.Language), strInit, AstralInitiativeDice.ToString());
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
        public string MatrixInitiative => string.Format(LanguageManager.GetString("String_Initiative", GlobalOptions.Language),
            MatrixInitiativeValue.ToString(),
            MatrixInitiativeDice.ToString());

        public string GetMatrixInitiative(CultureInfo objCulture, string strLanguageToPrint)
        {
            return string.Format(LanguageManager.GetString("String_Initiative", strLanguageToPrint),
                MatrixInitiativeValue.ToString(objCulture),
                MatrixInitiativeDice.ToString(objCulture));
        }

        public string MatrixInitiativeToolTip
        {
            get
            {
                int intINTAttributeModifiers = INT.AttributeModifiers;
                
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);

                string strInit;
                if (IsAI)
                {
                    strInit = INT.DisplayAbbrev + strSpaceCharacter + '(' + INT.Value.ToString(GlobalOptions.CultureInfo) + ')';
                              
                    if (HomeNode != null)
                    {
                        int intHomeNodeDP = HomeNode.GetTotalMatrixAttribute("Data Processing");
                        if (HomeNode is Vehicle objHomeNodeVehicle)
                        {
                            int intHomeNodePilot = objHomeNodeVehicle.Pilot;
                            if (intHomeNodePilot > intHomeNodeDP)
                                intHomeNodeDP = intHomeNodePilot;
                        }

                        strInit += strSpaceCharacter + '+' + strSpaceCharacter + LanguageManager.GetString("String_DataProcessing") + strSpaceCharacter + '(' + intHomeNodeDP.ToString(GlobalOptions.CultureInfo) + ')';
                    }

                    if (intINTAttributeModifiers != 0 || WoundModifier != 0)
                    {
                        strInit += strSpaceCharacter + '+' + strSpaceCharacter + LanguageManager.GetString("Tip_Modifiers", GlobalOptions.Language) + strSpaceCharacter +
                                   '(' + (intINTAttributeModifiers + WoundModifier).ToString(GlobalOptions.CultureInfo) + ')';
                    }
                }
                else
                {
                    int intREAAttributeModifiers = REA.AttributeModifiers;

                    strInit = REA.DisplayAbbrev + strSpaceCharacter + '(' + REA.Value.ToString(GlobalOptions.CultureInfo) + ')'
                              + strSpaceCharacter + '+' + strSpaceCharacter + INT.DisplayAbbrev + strSpaceCharacter + '(' + INT.Value.ToString(GlobalOptions.CultureInfo) + ')';
                    if (ImprovementManager.ValueOf(this, Improvement.ImprovementType.Initiative) != 0 || intINTAttributeModifiers != 0 || intREAAttributeModifiers != 0 || WoundModifier != 0)
                    {
                        strInit += strSpaceCharacter + '+' + strSpaceCharacter + LanguageManager.GetString("Tip_Modifiers", GlobalOptions.Language) + strSpaceCharacter +
                                   '(' + (ImprovementManager.ValueOf(this, Improvement.ImprovementType.Initiative) + intINTAttributeModifiers + intREAAttributeModifiers + WoundModifier).ToString(GlobalOptions.CultureInfo) + ')';
                    }
                }

                return string.Format(LanguageManager.GetString("String_Initiative", GlobalOptions.Language), strInit, MatrixInitiativeDice.ToString(GlobalOptions.CultureInfo));
            }
        }

        /// <summary>
        /// AR Matrix Initiative Value.
        /// </summary>
        public int MatrixInitiativeValue
        {
            get
            {
                if (IsAI)
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
                if (IsAI)
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
                if (IsAI)
                {
                    return MatrixInitiative;
                }

                return string.Format(LanguageManager.GetString(ActiveCommlink == null ? "String_MatrixInitiative" : "String_Initiative", GlobalOptions.Language),
                    MatrixInitiativeColdValue.ToString(),
                    MatrixInitiativeColdDice.ToString());
            }
        }

        public string GetMatrixInitiativeCold(CultureInfo objCulture, string strLanguageToPrint)
        {
            if (IsAI)
            {
                return GetMatrixInitiative(objCulture, strLanguageToPrint);
            }

            return string.Format(LanguageManager.GetString(ActiveCommlink == null ? "String_MatrixInitiative" : "String_Initiative", strLanguageToPrint),
                MatrixInitiativeColdValue.ToString(objCulture),
                MatrixInitiativeColdDice.ToString(objCulture));
        }

        public string MatrixInitiativeColdToolTip
        {
            get
            {
                if (IsAI)
                {
                    return MatrixInitiativeToolTip;
                }

                int intINTAttributeModifiers = INT.AttributeModifiers;

                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                

                string strInit = INT.DisplayAbbrev + strSpaceCharacter + '(' + INT.Value.ToString(GlobalOptions.CultureInfo) + ')';
                if (ActiveCommlink != null)
                {
                    strInit += strSpaceCharacter + '+' + strSpaceCharacter + LanguageManager.GetString("String_DataProcessing") + strSpaceCharacter + '(' + ActiveCommlink.GetTotalMatrixAttribute("Data Processing").ToString(GlobalOptions.CultureInfo) + ')';
                }

                if (ImprovementManager.ValueOf(this, Improvement.ImprovementType.MatrixInitiative) != 0 || intINTAttributeModifiers != 0 || WoundModifier != 0)
                {
                    strInit += strSpaceCharacter + '+' + strSpaceCharacter + LanguageManager.GetString("Tip_Modifiers", GlobalOptions.Language) + strSpaceCharacter +
                               '(' + (ImprovementManager.ValueOf(this, Improvement.ImprovementType.MatrixInitiative) + intINTAttributeModifiers + WoundModifier).ToString(GlobalOptions.CultureInfo) + ')';
                }

                return string.Format(LanguageManager.GetString(ActiveCommlink == null ? "String_MatrixInitiativeLong" : "String_Initiative", GlobalOptions.Language),
                    strInit, MatrixInitiativeColdDice.ToString());
            }
        }

        /// <summary>
        /// Cold Sim Matrix Initiative Value.
        /// </summary>
        public int MatrixInitiativeColdValue
        {
            get
            {
                if (IsAI)
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
                if (IsAI)
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
        public string MatrixInitiativeHot
        {
            get
            {
                if (IsAI)
                {
                    return MatrixInitiative;
                }

                return string.Format(LanguageManager.GetString(ActiveCommlink == null ? "String_MatrixInitiative" : "String_Initiative", GlobalOptions.Language),
                        MatrixInitiativeHotValue.ToString(),
                        MatrixInitiativeHotDice.ToString());
            }
        }

        public string GetMatrixInitiativeHot(CultureInfo objCulture, string strLanguageToPrint)
        {
            if (IsAI)
            {
                return GetMatrixInitiative(objCulture, strLanguageToPrint);
            }

            return string.Format(LanguageManager.GetString(ActiveCommlink == null ? "String_MatrixInitiative" : "String_Initiative", strLanguageToPrint),
                    MatrixInitiativeHotValue.ToString(objCulture),
                    MatrixInitiativeHotDice.ToString(objCulture));
        }

        public string MatrixInitiativeHotToolTip
        {
            get
            {
                if (IsAI)
                {
                    return MatrixInitiativeToolTip;
                }

                int intINTAttributeModifiers = INT.AttributeModifiers;

                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);


                string strInit = INT.DisplayAbbrev + strSpaceCharacter + '(' + INT.Value.ToString(GlobalOptions.CultureInfo) + ')';
                if (ActiveCommlink != null)
                {
                    strInit += strSpaceCharacter + '+' + strSpaceCharacter + LanguageManager.GetString("String_DataProcessing") + strSpaceCharacter + '(' + ActiveCommlink.GetTotalMatrixAttribute("Data Processing").ToString(GlobalOptions.CultureInfo) + ')';
                }

                if (ImprovementManager.ValueOf(this, Improvement.ImprovementType.MatrixInitiative) != 0 || intINTAttributeModifiers != 0 || WoundModifier != 0)
                {
                    strInit += strSpaceCharacter + '+' + strSpaceCharacter + LanguageManager.GetString("Tip_Modifiers", GlobalOptions.Language) + strSpaceCharacter +
                               '(' + (ImprovementManager.ValueOf(this, Improvement.ImprovementType.MatrixInitiative) + intINTAttributeModifiers + WoundModifier).ToString(GlobalOptions.CultureInfo) + ')';
                }

                return string.Format(LanguageManager.GetString(ActiveCommlink == null ? "String_MatrixInitiativeLong" : "String_Initiative", GlobalOptions.Language),
                    strInit, MatrixInitiativeHotDice.ToString());
            }
        }

        /// <summary>
        /// Hot Sim Matrix Initiative Value.
        /// </summary>
        public int MatrixInitiativeHotValue
        {
            get
            {
                if (IsAI)
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
                if (IsAI)
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
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                StringBuilder objToolTip = new StringBuilder(
                    CHA.DisplayAbbrev + strSpaceCharacter + '(' + CHA.TotalValue.ToString(GlobalOptions.CultureInfo) +
                    ')' +
                    strSpaceCharacter + '+' + strSpaceCharacter +
                    WIL.DisplayAbbrev + strSpaceCharacter + '(' + WIL.TotalValue.ToString(GlobalOptions.CultureInfo) +
                    ')');
                foreach (Improvement objLoopImprovement in Improvements)
                {
                    if (objLoopImprovement.ImproveType == Improvement.ImprovementType.Composure &&
                        objLoopImprovement.Enabled)
                    {
                        objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                          GetObjectName(objLoopImprovement, GlobalOptions.Language) +
                                          strSpaceCharacter + '(' +
                                          objLoopImprovement.Value.ToString(GlobalOptions.CultureInfo) + ')');
                    }
                }

                return objToolTip.ToString();
            }
        }

        /// <summary>
        /// Judge Intentions (INT + CHA).
        /// </summary>
        public int JudgeIntentions => INT.TotalValue + CHA.TotalValue +
                                      ImprovementManager.ValueOf(this, Improvement.ImprovementType.JudgeIntentions) +
                                      ImprovementManager.ValueOf(this,
                                          Improvement.ImprovementType.JudgeIntentionsOffense);

        public string JudgeIntentionsToolTip
        {
            get
            {
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                StringBuilder objToolTip = new StringBuilder(
                    CHA.DisplayAbbrev + strSpaceCharacter + '(' + CHA.TotalValue.ToString(GlobalOptions.CultureInfo) +
                    ')' +
                    strSpaceCharacter + '+' + strSpaceCharacter +
                    INT.DisplayAbbrev + strSpaceCharacter + '(' + INT.TotalValue.ToString(GlobalOptions.CultureInfo) +
                    ')');
                foreach (Improvement objLoopImprovement in Improvements)
                {
                    if ((objLoopImprovement.ImproveType == Improvement.ImprovementType.JudgeIntentions ||
                         objLoopImprovement.ImproveType == Improvement.ImprovementType.JudgeIntentionsOffense) &&
                        objLoopImprovement.Enabled)
                    {
                        objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                          GetObjectName(objLoopImprovement, GlobalOptions.Language) +
                                          strSpaceCharacter + '(' +
                                          objLoopImprovement.Value.ToString(GlobalOptions.CultureInfo) + ')');
                    }
                }

                return objToolTip.ToString();
            }
        }

        /// <summary>
        /// Judge Intentions Resist (CHA + WIL).
        /// </summary>
        public int JudgeIntentionsResist => CHA.TotalValue + WIL.TotalValue +
                                            ImprovementManager.ValueOf(this,
                                                Improvement.ImprovementType.JudgeIntentions) +
                                            ImprovementManager.ValueOf(this,
                                                Improvement.ImprovementType.JudgeIntentionsDefense);

        public string JudgeIntentionsResistToolTip
        {
            get
            {
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                StringBuilder objToolTip = new StringBuilder(
                    CHA.DisplayAbbrev + strSpaceCharacter + '(' + CHA.TotalValue.ToString(GlobalOptions.CultureInfo) +
                    ')' +
                    strSpaceCharacter + '+' + strSpaceCharacter +
                    WIL.DisplayAbbrev + strSpaceCharacter + '(' + WIL.TotalValue.ToString(GlobalOptions.CultureInfo) +
                    ')');
                foreach (Improvement objLoopImprovement in Improvements)
                {
                    if ((objLoopImprovement.ImproveType == Improvement.ImprovementType.JudgeIntentions ||
                         objLoopImprovement.ImproveType == Improvement.ImprovementType.JudgeIntentionsDefense) &&
                        objLoopImprovement.Enabled)
                    {
                        objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                          GetObjectName(objLoopImprovement, GlobalOptions.Language) +
                                          strSpaceCharacter + '(' +
                                          objLoopImprovement.Value.ToString(GlobalOptions.CultureInfo) + ')');
                    }
                }

                return objToolTip.ToString();
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
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                StringBuilder objToolTip = new StringBuilder(
                    BOD.DisplayAbbrev + strSpaceCharacter + '(' + BOD.TotalValue.ToString(GlobalOptions.CultureInfo) +
                    ')' +
                    strSpaceCharacter + '+' + strSpaceCharacter +
                    STR.DisplayAbbrev + strSpaceCharacter + '(' + STR.TotalValue.ToString(GlobalOptions.CultureInfo) +
                    ')');
                foreach (Improvement objLoopImprovement in Improvements)
                {
                    if (objLoopImprovement.ImproveType == Improvement.ImprovementType.LiftAndCarry &&
                        objLoopImprovement.Enabled)
                    {
                        objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                          GetObjectName(objLoopImprovement, GlobalOptions.Language) +
                                          strSpaceCharacter + '(' +
                                          objLoopImprovement.Value.ToString(GlobalOptions.CultureInfo) + ')');
                    }
                }

                objToolTip.Append(Environment.NewLine + LanguageManager
                                      .GetString("Tip_LiftAndCarry", GlobalOptions.Language)
                                      .Replace("{0}", (STR.TotalValue * 15).ToString())
                                      .Replace("{1}", (STR.TotalValue * 10).ToString()));
                return objToolTip.ToString();
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
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                StringBuilder objToolTip = new StringBuilder(
                    LOG.DisplayAbbrev + strSpaceCharacter + '(' + LOG.TotalValue.ToString(GlobalOptions.CultureInfo) +
                    ')' +
                    strSpaceCharacter + '+' + strSpaceCharacter +
                    WIL.DisplayAbbrev + strSpaceCharacter + '(' + WIL.TotalValue.ToString(GlobalOptions.CultureInfo) +
                    ')');
                foreach (Improvement objLoopImprovement in Improvements)
                {
                    if (objLoopImprovement.ImproveType == Improvement.ImprovementType.Memory &&
                        objLoopImprovement.Enabled)
                    {
                        objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                          GetObjectName(objLoopImprovement, GlobalOptions.Language) +
                                          strSpaceCharacter + '(' +
                                          objLoopImprovement.Value.ToString(GlobalOptions.CultureInfo) + ')');
                    }
                }

                return objToolTip.ToString();
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
            if (IsAI || Improvements.Any(x =>
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
            if (IsAI || Improvements.Any(x =>
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
            if (IsAI || Improvements.Any(x =>
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
            if (IsAI || Improvements.Any(x =>
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
            if (IsAI || Improvements.Any(x =>
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
            if (IsAI || Improvements.Any(x =>
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
            if (IsAI || Improvements.Any(x =>
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
            if (IsAI || Improvements.Any(x =>
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
                if (IsAI)
                    return 0;
                int intReturn = BOD.TotalValue + WIL.TotalValue +
                                ImprovementManager.ValueOf(this, Improvement.ImprovementType.StunCMRecovery);
                if (Improvements.Any(x =>
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
                if (IsAI)
                {
                    if (HomeNode is Vehicle)
                        return 0;

                    // A.I.s can restore Core damage via Software + Depth [Data Processing] (1 day) Extended Test
                    int intDEPTotal = DEP.TotalValue;
                    int intAIReturn =
                        (SkillsSection.GetActiveSkill("Software")?.PoolOtherAttribute(intDEPTotal, "DEP") ??
                         intDEPTotal - 1) +
                        ImprovementManager.ValueOf(this, Improvement.ImprovementType.PhysicalCMRecovery);
                    if (Improvements.Any(x =>
                        x.Enabled && x.ImproveType == Improvement.ImprovementType.AddESStoPhysicalCMRecovery))
                        intAIReturn += decimal.ToInt32(decimal.Floor(Essence()));
                    return intAIReturn;
                }

                int intReturn = 2 * BOD.TotalValue +
                                ImprovementManager.ValueOf(this, Improvement.ImprovementType.PhysicalCMRecovery);
                if (Improvements.Any(x =>
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
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);

                foreach (Improvement objImprovement in _lstImprovements)
                {
                    if (objImprovement.ImproveType == Improvement.ImprovementType.StreetCred && objImprovement.Enabled)
                        objReturn.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                         GetObjectName(objImprovement, GlobalOptions.Language) + strSpaceCharacter +
                                         '(' + objImprovement.Value.ToString(GlobalOptions.CultureInfo) + ')');
                }

                objReturn.Append(strSpaceCharacter + '+' + strSpaceCharacter + '[' +
                                 LanguageManager.GetString("String_CareerKarma", GlobalOptions.Language) +
                                 strSpaceCharacter + '÷' + strSpaceCharacter +
                                 (10 + ImprovementManager.ValueOf(this,
                                      Improvement.ImprovementType.StreetCredMultiplier))
                                 .ToString(GlobalOptions.CultureInfo) + ']' + strSpaceCharacter + '(' +
                                 (CareerKarma / (10 + ImprovementManager.ValueOf(this,
                                                     Improvement.ImprovementType.StreetCredMultiplier)))
                                 .ToString(GlobalOptions.CultureInfo) + ')');

                if (BurntStreetCred != 0)
                    objReturn.Append(strSpaceCharacter + '-' + strSpaceCharacter +
                                     LanguageManager.GetString("String_BurntStreetCred", GlobalOptions.Language) +
                                     strSpaceCharacter + '(' + BurntStreetCred + ')');

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
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                int intCalculatedNotoriety = CalculatedNotoriety;
                return (intCalculatedNotoriety >= 0
                           ? strSpaceCharacter + '+' + strSpaceCharacter +
                             intCalculatedNotoriety.ToString(GlobalOptions.CultureInfo)
                           : strSpaceCharacter + '-' + strSpaceCharacter +
                             (-intCalculatedNotoriety).ToString(GlobalOptions.CultureInfo)) + strSpaceCharacter + '=' +
                       strSpaceCharacter + TotalNotoriety.ToString(GlobalOptions.CultureInfo);
            }
        }

        /// <summary>
        /// Tooltip to use for Notoriety total.
        /// </summary>
        public string NotorietyTooltip
        {
            get
            {
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                StringBuilder objReturn = new StringBuilder(Notoriety.ToString(GlobalOptions.CultureInfo));

                foreach (Improvement objImprovement in _lstImprovements)
                {
                    if (objImprovement.ImproveType == Improvement.ImprovementType.Notoriety && objImprovement.Enabled)
                        objReturn.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                         GetObjectName(objImprovement, GlobalOptions.Language) + strSpaceCharacter +
                                         '(' + objImprovement.Value.ToString(GlobalOptions.CultureInfo) + ')');
                }

                /*
                int intEnemies = Contacts.Count(x => x.EntityType == ContactType.Enemy);
                if (intEnemies > 0)
                    objReturn.Append(strSpaceCharacter + '+' + strSpaceCharacter + LanguageManager.GetString("Label_SummaryEnemies", GlobalOptions.Language) + strSpaceCharacter + '(' + intEnemies.ToString(GlobalOptions.CultureInfo) + ')');
                    */

                if (BurntStreetCred > 0)
                    objReturn.Append(strSpaceCharacter + '-' + strSpaceCharacter +
                                     LanguageManager.GetString("String_BurntStreetCred", GlobalOptions.Language) +
                                     strSpaceCharacter + '(' +
                                     (BurntStreetCred / 2).ToString(GlobalOptions.CultureInfo) + ')');

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
                int intReturn = PublicAwareness + CalculatedPublicAwareness;
                if (Erased && intReturn >= 1)
                    return 1;
                return intReturn;
            }
        }

        public string CareerDisplayPublicAwareness
        {
            get
            {
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                int intTotalPublicAwareness = TotalPublicAwareness;
                int intCalculatedPublicAwareness = intTotalPublicAwareness - PublicAwareness;
                return (intCalculatedPublicAwareness >= 0
                           ? strSpaceCharacter + '+' + strSpaceCharacter +
                             intCalculatedPublicAwareness.ToString(GlobalOptions.CultureInfo)
                           : strSpaceCharacter + '-' + strSpaceCharacter +
                             (-intCalculatedPublicAwareness).ToString(GlobalOptions.CultureInfo)) + strSpaceCharacter +
                       '=' + strSpaceCharacter + intTotalPublicAwareness.ToString(GlobalOptions.CultureInfo);
            }
        }

        /// <summary>
        /// Public Awareness Tooltip.
        /// </summary>
        public string PublicAwarenessTooltip
        {
            get
            {
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                StringBuilder objReturn = new StringBuilder(PublicAwareness.ToString(GlobalOptions.CultureInfo));

                foreach (Improvement objImprovement in _lstImprovements)
                {
                    if (objImprovement.ImproveType == Improvement.ImprovementType.PublicAwareness &&
                        objImprovement.Enabled)
                        objReturn.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                         GetObjectName(objImprovement, GlobalOptions.Language) + strSpaceCharacter +
                                         '(' + objImprovement.Value.ToString(GlobalOptions.CultureInfo) + ')');
                }

                if (_objOptions.UseCalculatedPublicAwareness)
                {
                    objReturn.Append(strSpaceCharacter + '+' + strSpaceCharacter + '[' +
                                     LanguageManager.GetString("String_StreetCred", GlobalOptions.Language) +
                                     strSpaceCharacter + '+' + strSpaceCharacter +
                                     LanguageManager.GetString("String_Notoriety", GlobalOptions.Language) + ']' +
                                     strSpaceCharacter + '÷' + strSpaceCharacter +
                                     3.ToString(GlobalOptions.CultureInfo) + strSpaceCharacter + '('
                                     + ((TotalStreetCred + TotalNotoriety) / 3).ToString(GlobalOptions.CultureInfo) +
                                     ')');
                }

                if (Erased)
                {
                    int intTotalPublicAwareness = PublicAwareness + CalculatedPublicAwareness;
                    if (intTotalPublicAwareness > 1)
                    {
                        string strErasedString = Qualities.FirstOrDefault(x => x.Name == "Erased")
                            ?.DisplayNameShort(GlobalOptions.Language);
                        if (string.IsNullOrEmpty(strErasedString))
                        {
                            XmlNode xmlErasedQuality = XmlManager.Load("qualities.xml")
                                .SelectSingleNode("chummer/qualities/quality[name = \"Erased\"]");
                            if (xmlErasedQuality != null)
                            {
                                strErasedString = xmlErasedQuality["translate"]?.InnerText ??
                                                  xmlErasedQuality["name"]?.InnerText ?? string.Empty;
                            }
                        }

                        objReturn.Append(strSpaceCharacter + '-' + strSpaceCharacter + strErasedString +
                                         strSpaceCharacter + '(' + (intTotalPublicAwareness - 1) + ')');
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
        public TaggedObservableCollection<Cyberware> Cyberware => _lstCyberware;

        /// <summary>
        /// Weapons.
        /// </summary>
        public TaggedObservableCollection<Weapon> Weapons => _lstWeapons;

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
        public TaggedObservableCollection<Vehicle> Vehicles => _lstVehicles;

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
        public ObservableCollection<Location> GearLocations => _lstGearLocations;

        /// <summary>
        /// Armor Bundles.
        /// </summary>
        public ObservableCollection<Location> ArmorLocations => _lstArmorLocations;

        /// <summary>
        /// Vehicle Locations.
        /// </summary>
        public ObservableCollection<Location> VehicleLocations => _lstVehicleLocations;

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

                if (Armor.Count == 0) return 0;
                // Run through the list of Armor currently worn and retrieve the highest total Armor rating.
                foreach (Armor objArmor in Armor.Where(objArmor =>
                    !objArmor.ArmorValue.StartsWith('+') && objArmor.Equipped))
                {
                    int intArmorValue = objArmor.TotalArmor;
                    int intCustomStackBonus = 0;
                    string strArmorName = objArmor.Name;
                    if (objArmor.Category == "High-Fashion Armor Clothing")
                    {
                        foreach (Armor a in Armor.Where(a =>
                            (a.Category == "High-Fashion Armor Clothing" || a.ArmorOverrideValue.StartsWith('+')) &&
                            a.Equipped))
                        {
                            if (a.ArmorMods.Any(objMod =>
                                objMod.Name == "Custom Fit (Stack)" && objMod.Extra == strArmorName))
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
                foreach (Armor objArmor in Armor.Where(objArmor =>
                    (objArmor.ArmorValue.StartsWith('+') || objArmor.ArmorOverrideValue.StartsWith('+')) &&
                    objArmor.Name != strHighest && objArmor.Category == "Clothing" && objArmor.Equipped))
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
                foreach (Armor objArmor in Armor.Where(objArmor =>
                    (objArmor.ArmorValue.StartsWith('+') || objArmor.ArmorOverrideValue.StartsWith('+')) &&
                    objArmor.Name != strHighest && objArmor.Category != "Clothing" && objArmor.Equipped))
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

        public int DamageResistancePool =>
            (IsAI ? (HomeNode is Vehicle objVehicle ? objVehicle.TotalBody : 0) : BOD.TotalValue) + TotalArmorRating +
            ImprovementManager.ValueOf(this, Improvement.ImprovementType.DamageResistance);

        public string DamageResistancePoolToolTip
        {
            get
            {
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                StringBuilder objToolTip = new StringBuilder();
                if (IsAI)
                {
                    objToolTip.Append(LanguageManager.GetString("String_VehicleBody", GlobalOptions.Language) +
                                      strSpaceCharacter + '(' +
                                      (HomeNode is Vehicle objVehicle ? objVehicle.TotalBody : 0).ToString(GlobalOptions
                                          .CultureInfo) + ')');
                }
                else
                {
                    objToolTip.Append(BOD.DisplayAbbrev + strSpaceCharacter + '(' +
                                      BOD.TotalValue.ToString(GlobalOptions.CultureInfo) + ')');
                }

                objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                  LanguageManager.GetString("Tip_Armor", GlobalOptions.Language) + strSpaceCharacter +
                                  '(' + TotalArmorRating.ToString(GlobalOptions.CultureInfo) + ')');
                foreach (Improvement objLoopImprovement in Improvements)
                {
                    if (objLoopImprovement.ImproveType == Improvement.ImprovementType.DamageResistance &&
                        objLoopImprovement.Enabled)
                    {
                        objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                          GetObjectName(objLoopImprovement, GlobalOptions.Language) +
                                          strSpaceCharacter + '(' +
                                          objLoopImprovement.Value.ToString(GlobalOptions.CultureInfo) + ')');
                    }
                }

                return objToolTip.ToString();
            }
        }

        public int CurrentCounterspellingDice
        {
            get => _intCurrentCounterspellingDice;
            set
            {
                if (_intCurrentCounterspellingDice != value)
                {
                    _intCurrentCounterspellingDice = value;
                    OnPropertyChanged();
                }
            }
        }

        public int SpellDefenseIndirectDodge => REA.TotalValue + INT.TotalValue + TotalBonusDodgeRating;

        public string DisplaySpellDefenseIndirectDodge => CurrentCounterspellingDice == 0
            ? SpellDefenseIndirectDodge.ToString(GlobalOptions.CultureInfo)
            : SpellDefenseIndirectDodge.ToString(GlobalOptions.CultureInfo) +
              LanguageManager.GetString("String_Space", GlobalOptions.Language) + '(' +
              (SpellDefenseIndirectDodge + CurrentCounterspellingDice).ToString(GlobalOptions.CultureInfo) + ')';

        public string SpellDefenseIndirectDodgeToolTip
        {
            get
            {
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                StringBuilder objToolTip = new StringBuilder(REA.DisplayAbbrev + strSpaceCharacter + '(' +
                                                             REA.TotalValue.ToString(GlobalOptions.CultureInfo) + ')' +
                                                             strSpaceCharacter + '+' + strSpaceCharacter +
                                                             INT.DisplayAbbrev + strSpaceCharacter + '(' +
                                                             INT.TotalValue.ToString(GlobalOptions.CultureInfo) + ')');

                if (CurrentCounterspellingDice != 0)
                    objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                      LanguageManager.GetString("Label_CounterspellingDice", GlobalOptions.Language) +
                                      strSpaceCharacter + '(' +
                                      CurrentCounterspellingDice.ToString(GlobalOptions.CultureInfo) + ')');

                int intModifiers = TotalBonusDodgeRating;

                if (intModifiers != 0)
                {
                    objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                      LanguageManager.GetString("Tip_Modifiers", GlobalOptions.Language));
                    bool blnFirstModifier = true;
                    foreach (Improvement objLoopImprovement in Improvements)
                    {
                        if (objLoopImprovement.ImproveType == Improvement.ImprovementType.Dodge &&
                            objLoopImprovement.Enabled)
                        {
                            if (blnFirstModifier)
                            {
                                blnFirstModifier = false;
                                objToolTip.Append(':');
                            }
                            else
                                objToolTip.Append(',');

                            objToolTip.Append(strSpaceCharacter +
                                              GetObjectName(objLoopImprovement, GlobalOptions.Language));
                        }
                    }

                    objToolTip.Append(strSpaceCharacter + '(' + intModifiers.ToString(GlobalOptions.CultureInfo) + ')');
                }

                return objToolTip.ToString();
            }
        }

        public int SpellDefenseIndirectSoak =>
            (IsAI ? (HomeNode is Vehicle objVehicle ? objVehicle.TotalBody : 0) : BOD.TotalValue) + TotalArmorRating +
            SpellResistance;

        public string DisplaySpellDefenseIndirectSoak => CurrentCounterspellingDice == 0
            ? SpellDefenseIndirectSoak.ToString(GlobalOptions.CultureInfo)
            : SpellDefenseIndirectSoak.ToString(GlobalOptions.CultureInfo) +
              LanguageManager.GetString("String_Space", GlobalOptions.Language) + '(' +
              (SpellDefenseIndirectSoak + CurrentCounterspellingDice).ToString(GlobalOptions.CultureInfo) + ')';

        public string SpellDefenseIndirectSoakToolTip
        {
            get
            {
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                StringBuilder objToolTip = new StringBuilder();
                if (IsAI)
                {
                    objToolTip.Append(LanguageManager.GetString("String_VehicleBody", GlobalOptions.Language) +
                                      strSpaceCharacter + '(' +
                                      (HomeNode is Vehicle objVehicle ? objVehicle.TotalBody : 0).ToString(GlobalOptions
                                          .CultureInfo) + ')');
                }
                else
                {
                    objToolTip.Append(BOD.DisplayAbbrev + strSpaceCharacter + '(' +
                                      BOD.TotalValue.ToString(GlobalOptions.CultureInfo) + ')');
                }

                objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                  LanguageManager.GetString("Tip_Armor", GlobalOptions.Language) + strSpaceCharacter +
                                  '(' + TotalArmorRating.ToString(GlobalOptions.CultureInfo) + ')');

                if (CurrentCounterspellingDice != 0)
                    objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                      LanguageManager.GetString("Label_CounterspellingDice", GlobalOptions.Language) +
                                      strSpaceCharacter + '(' +
                                      CurrentCounterspellingDice.ToString(GlobalOptions.CultureInfo) + ')');

                int intModifiers = SpellResistance;

                if (intModifiers != 0)
                {
                    objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                      LanguageManager.GetString("Tip_Modifiers", GlobalOptions.Language));
                    bool blnFirstModifier = true;
                    foreach (Improvement objLoopImprovement in Improvements)
                    {
                        if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SpellResistance &&
                            objLoopImprovement.Enabled)
                        {
                            if (blnFirstModifier)
                            {
                                blnFirstModifier = false;
                                objToolTip.Append(':');
                            }
                            else
                                objToolTip.Append(',');

                            objToolTip.Append(strSpaceCharacter +
                                              GetObjectName(objLoopImprovement, GlobalOptions.Language));
                        }
                    }

                    objToolTip.Append(strSpaceCharacter + '(' + intModifiers.ToString(GlobalOptions.CultureInfo) + ')');
                }

                return objToolTip.ToString();
            }
        }

        public int SpellDefenseDirectSoakMana => WIL.TotalValue + SpellResistance;

        public string DisplaySpellDefenseDirectSoakMana => CurrentCounterspellingDice == 0
            ? SpellDefenseDirectSoakMana.ToString(GlobalOptions.CultureInfo)
            : SpellDefenseDirectSoakMana.ToString(GlobalOptions.CultureInfo) +
              LanguageManager.GetString("String_Space", GlobalOptions.Language) + '(' +
              (SpellDefenseDirectSoakMana + CurrentCounterspellingDice).ToString(GlobalOptions.CultureInfo) + ')';

        public string SpellDefenseDirectSoakManaToolTip
        {
            get
            {
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                StringBuilder objToolTip = new StringBuilder(WIL.DisplayAbbrev + strSpaceCharacter + '(' +
                                                             WIL.TotalValue.ToString(GlobalOptions.CultureInfo) + ')');

                if (CurrentCounterspellingDice != 0)
                    objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                      LanguageManager.GetString("Label_CounterspellingDice", GlobalOptions.Language) +
                                      strSpaceCharacter + '(' +
                                      CurrentCounterspellingDice.ToString(GlobalOptions.CultureInfo) + ')');

                int intModifiers = SpellResistance;

                if (intModifiers != 0)
                {
                    objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                      LanguageManager.GetString("Tip_Modifiers", GlobalOptions.Language));
                    bool blnFirstModifier = true;
                    foreach (Improvement objLoopImprovement in Improvements)
                    {
                        if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SpellResistance &&
                            objLoopImprovement.Enabled)
                        {
                            if (blnFirstModifier)
                            {
                                blnFirstModifier = false;
                                objToolTip.Append(':');
                            }
                            else
                                objToolTip.Append(',');

                            objToolTip.Append(strSpaceCharacter +
                                              GetObjectName(objLoopImprovement, GlobalOptions.Language));
                        }
                    }

                    objToolTip.Append(strSpaceCharacter + '(' + intModifiers.ToString(GlobalOptions.CultureInfo) + ')');
                }

                return objToolTip.ToString();
            }
        }

        public int SpellDefenseDirectSoakPhysical =>
            (IsAI ? (HomeNode is Vehicle objVehicle ? objVehicle.TotalBody : 0) : BOD.TotalValue) + SpellResistance;

        public string DisplaySpellDefenseDirectSoakPhysical => CurrentCounterspellingDice == 0
            ? SpellDefenseDirectSoakPhysical.ToString(GlobalOptions.CultureInfo)
            : SpellDefenseDirectSoakPhysical.ToString(GlobalOptions.CultureInfo) +
              LanguageManager.GetString("String_Space", GlobalOptions.Language) + '(' +
              (SpellDefenseDirectSoakPhysical + CurrentCounterspellingDice).ToString(GlobalOptions.CultureInfo) + ')';

        public string SpellDefenseDirectSoakPhysicalToolTip
        {
            get
            {
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                StringBuilder objToolTip = new StringBuilder();
                if (IsAI)
                {
                    objToolTip.Append(LanguageManager.GetString("String_VehicleBody", GlobalOptions.Language) +
                                      strSpaceCharacter + '(' +
                                      (HomeNode is Vehicle objVehicle ? objVehicle.TotalBody : 0).ToString(GlobalOptions
                                          .CultureInfo) + ')');
                }
                else
                {
                    objToolTip.Append(BOD.DisplayAbbrev + strSpaceCharacter + '(' +
                                      BOD.TotalValue.ToString(GlobalOptions.CultureInfo) + ')');
                }

                if (CurrentCounterspellingDice != 0)
                    objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                      LanguageManager.GetString("Label_CounterspellingDice", GlobalOptions.Language) +
                                      strSpaceCharacter + '(' +
                                      CurrentCounterspellingDice.ToString(GlobalOptions.CultureInfo) + ')');

                int intModifiers = SpellResistance;

                if (intModifiers != 0)
                {
                    objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                      LanguageManager.GetString("Tip_Modifiers", GlobalOptions.Language));
                    bool blnFirstModifier = true;
                    foreach (Improvement objLoopImprovement in Improvements)
                    {
                        if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SpellResistance &&
                            objLoopImprovement.Enabled)
                        {
                            if (blnFirstModifier)
                            {
                                blnFirstModifier = false;
                                objToolTip.Append(':');
                            }
                            else
                                objToolTip.Append(',');

                            objToolTip.Append(strSpaceCharacter +
                                              GetObjectName(objLoopImprovement, GlobalOptions.Language));
                        }
                    }

                    objToolTip.Append(strSpaceCharacter + '(' + intModifiers.ToString(GlobalOptions.CultureInfo) + ')');
                }

                return objToolTip.ToString();
            }
        }

        public int SpellDefenseDetection => LOG.TotalValue + WIL.TotalValue + SpellResistance +
                                            ImprovementManager.ValueOf(this,
                                                Improvement.ImprovementType.DetectionSpellResist);

        public string DisplaySpellDefenseDetection => CurrentCounterspellingDice == 0
            ? SpellDefenseDetection.ToString(GlobalOptions.CultureInfo)
            : SpellDefenseDetection.ToString(GlobalOptions.CultureInfo) +
              LanguageManager.GetString("String_Space", GlobalOptions.Language) + '(' +
              (SpellDefenseDetection + CurrentCounterspellingDice).ToString(GlobalOptions.CultureInfo) + ')';

        public string SpellDefenseDetectionToolTip
        {
            get
            {
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                StringBuilder objToolTip = new StringBuilder(LOG.DisplayAbbrev + strSpaceCharacter + '(' +
                                                             LOG.TotalValue.ToString(GlobalOptions.CultureInfo) + ')' +
                                                             strSpaceCharacter + '+' + strSpaceCharacter +
                                                             WIL.DisplayAbbrev + strSpaceCharacter + '(' +
                                                             WIL.TotalValue.ToString(GlobalOptions.CultureInfo) + ')');

                if (CurrentCounterspellingDice != 0)
                    objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                      LanguageManager.GetString("Label_CounterspellingDice", GlobalOptions.Language) +
                                      strSpaceCharacter + '(' +
                                      CurrentCounterspellingDice.ToString(GlobalOptions.CultureInfo) + ')');

                int intModifiers = SpellResistance +
                                   ImprovementManager.ValueOf(this, Improvement.ImprovementType.DetectionSpellResist);

                if (intModifiers != 0)
                {
                    objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                      LanguageManager.GetString("Tip_Modifiers", GlobalOptions.Language));
                    bool blnFirstModifier = true;
                    foreach (Improvement objLoopImprovement in Improvements)
                    {
                        if ((objLoopImprovement.ImproveType == Improvement.ImprovementType.DetectionSpellResist ||
                             objLoopImprovement.ImproveType == Improvement.ImprovementType.SpellResistance) &&
                            objLoopImprovement.Enabled)
                        {
                            if (blnFirstModifier)
                            {
                                blnFirstModifier = false;
                                objToolTip.Append(':');
                            }
                            else
                                objToolTip.Append(',');

                            objToolTip.Append(strSpaceCharacter +
                                              GetObjectName(objLoopImprovement, GlobalOptions.Language));
                        }
                    }

                    objToolTip.Append(strSpaceCharacter + '(' + intModifiers.ToString(GlobalOptions.CultureInfo) + ')');
                }

                return objToolTip.ToString();
            }
        }

        public int SpellDefenseDecreaseBOD => BOD.TotalValue + WIL.TotalValue + SpellResistance;

        public string DisplaySpellDefenseDecreaseBOD => CurrentCounterspellingDice == 0
            ? SpellDefenseDecreaseBOD.ToString(GlobalOptions.CultureInfo)
            : SpellDefenseDecreaseBOD.ToString(GlobalOptions.CultureInfo) +
              LanguageManager.GetString("String_Space", GlobalOptions.Language) + '(' +
              (SpellDefenseDecreaseBOD + CurrentCounterspellingDice).ToString(GlobalOptions.CultureInfo) + ')';

        public string SpellDefenseDecreaseBODToolTip
        {
            get
            {
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                StringBuilder objToolTip = new StringBuilder(BOD.DisplayAbbrev + strSpaceCharacter + '(' +
                                                             BOD.TotalValue.ToString(GlobalOptions.CultureInfo) + ')' +
                                                             strSpaceCharacter + '+' + strSpaceCharacter +
                                                             WIL.DisplayAbbrev + strSpaceCharacter + '(' +
                                                             WIL.TotalValue.ToString(GlobalOptions.CultureInfo) + ')');

                if (CurrentCounterspellingDice != 0)
                    objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                      LanguageManager.GetString("Label_CounterspellingDice", GlobalOptions.Language) +
                                      strSpaceCharacter + '(' +
                                      CurrentCounterspellingDice.ToString(GlobalOptions.CultureInfo) + ')');

                int intModifiers = SpellResistance;

                if (intModifiers != 0)
                {
                    objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                      LanguageManager.GetString("Tip_Modifiers", GlobalOptions.Language));
                    bool blnFirstModifier = true;
                    foreach (Improvement objLoopImprovement in Improvements)
                    {
                        if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SpellResistance &&
                            objLoopImprovement.Enabled)
                        {
                            if (blnFirstModifier)
                            {
                                blnFirstModifier = false;
                                objToolTip.Append(':');
                            }
                            else
                                objToolTip.Append(',');

                            objToolTip.Append(strSpaceCharacter +
                                              GetObjectName(objLoopImprovement, GlobalOptions.Language));
                        }
                    }

                    objToolTip.Append(strSpaceCharacter + '(' + intModifiers.ToString(GlobalOptions.CultureInfo) + ')');
                }

                return objToolTip.ToString();
            }
        }

        public int SpellDefenseDecreaseAGI => AGI.TotalValue + WIL.TotalValue + SpellResistance;

        public string DisplaySpellDefenseDecreaseAGI => CurrentCounterspellingDice == 0
            ? SpellDefenseDecreaseAGI.ToString(GlobalOptions.CultureInfo)
            : SpellDefenseDecreaseAGI.ToString(GlobalOptions.CultureInfo) +
              LanguageManager.GetString("String_Space", GlobalOptions.Language) + '(' +
              (SpellDefenseDecreaseAGI + CurrentCounterspellingDice).ToString(GlobalOptions.CultureInfo) + ')';

        public string SpellDefenseDecreaseAGIToolTip
        {
            get
            {
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                StringBuilder objToolTip = new StringBuilder(AGI.DisplayAbbrev + strSpaceCharacter + '(' +
                                                             AGI.TotalValue.ToString(GlobalOptions.CultureInfo) + ')' +
                                                             strSpaceCharacter + '+' + strSpaceCharacter +
                                                             WIL.DisplayAbbrev + strSpaceCharacter + '(' +
                                                             WIL.TotalValue.ToString(GlobalOptions.CultureInfo) + ')');

                if (CurrentCounterspellingDice != 0)
                    objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                      LanguageManager.GetString("Label_CounterspellingDice", GlobalOptions.Language) +
                                      strSpaceCharacter + '(' +
                                      CurrentCounterspellingDice.ToString(GlobalOptions.CultureInfo) + ')');

                int intModifiers = SpellResistance;

                if (intModifiers != 0)
                {
                    objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                      LanguageManager.GetString("Tip_Modifiers", GlobalOptions.Language));
                    bool blnFirstModifier = true;
                    foreach (Improvement objLoopImprovement in Improvements)
                    {
                        if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SpellResistance &&
                            objLoopImprovement.Enabled)
                        {
                            if (blnFirstModifier)
                            {
                                blnFirstModifier = false;
                                objToolTip.Append(':');
                            }
                            else
                                objToolTip.Append(',');

                            objToolTip.Append(strSpaceCharacter +
                                              GetObjectName(objLoopImprovement, GlobalOptions.Language));
                        }
                    }

                    objToolTip.Append(strSpaceCharacter + '(' + intModifiers.ToString(GlobalOptions.CultureInfo) + ')');
                }

                return objToolTip.ToString();
            }
        }

        public int SpellDefenseDecreaseREA => REA.TotalValue + WIL.TotalValue + SpellResistance;

        public string DisplaySpellDefenseDecreaseREA => CurrentCounterspellingDice == 0
            ? SpellDefenseDecreaseREA.ToString(GlobalOptions.CultureInfo)
            : SpellDefenseDecreaseREA.ToString(GlobalOptions.CultureInfo) +
              LanguageManager.GetString("String_Space", GlobalOptions.Language) + '(' +
              (SpellDefenseDecreaseREA + CurrentCounterspellingDice).ToString(GlobalOptions.CultureInfo) + ')';

        public string SpellDefenseDecreaseREAToolTip
        {
            get
            {
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                StringBuilder objToolTip = new StringBuilder(REA.DisplayAbbrev + strSpaceCharacter + '(' +
                                                             REA.TotalValue.ToString(GlobalOptions.CultureInfo) + ')' +
                                                             strSpaceCharacter + '+' + strSpaceCharacter +
                                                             WIL.DisplayAbbrev + strSpaceCharacter + '(' +
                                                             WIL.TotalValue.ToString(GlobalOptions.CultureInfo) + ')');

                if (CurrentCounterspellingDice != 0)
                    objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                      LanguageManager.GetString("Label_CounterspellingDice", GlobalOptions.Language) +
                                      strSpaceCharacter + '(' +
                                      CurrentCounterspellingDice.ToString(GlobalOptions.CultureInfo) + ')');

                int intModifiers = SpellResistance;

                if (intModifiers != 0)
                {
                    objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                      LanguageManager.GetString("Tip_Modifiers", GlobalOptions.Language));
                    bool blnFirstModifier = true;
                    foreach (Improvement objLoopImprovement in Improvements)
                    {
                        if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SpellResistance &&
                            objLoopImprovement.Enabled)
                        {
                            if (blnFirstModifier)
                            {
                                blnFirstModifier = false;
                                objToolTip.Append(':');
                            }
                            else
                                objToolTip.Append(',');

                            objToolTip.Append(strSpaceCharacter +
                                              GetObjectName(objLoopImprovement, GlobalOptions.Language));
                        }
                    }

                    objToolTip.Append(strSpaceCharacter + '(' + intModifiers.ToString(GlobalOptions.CultureInfo) + ')');
                }

                return objToolTip.ToString();
            }
        }

        public int SpellDefenseDecreaseSTR => STR.TotalValue + WIL.TotalValue + SpellResistance;

        public string DisplaySpellDefenseDecreaseSTR => CurrentCounterspellingDice == 0
            ? SpellDefenseDecreaseSTR.ToString(GlobalOptions.CultureInfo)
            : SpellDefenseDecreaseSTR.ToString(GlobalOptions.CultureInfo) +
              LanguageManager.GetString("String_Space", GlobalOptions.Language) + '(' +
              (SpellDefenseDecreaseSTR + CurrentCounterspellingDice).ToString(GlobalOptions.CultureInfo) + ')';

        public string SpellDefenseDecreaseSTRToolTip
        {
            get
            {
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                StringBuilder objToolTip = new StringBuilder(STR.DisplayAbbrev + strSpaceCharacter + '(' +
                                                             STR.TotalValue.ToString(GlobalOptions.CultureInfo) + ')' +
                                                             strSpaceCharacter + '+' + strSpaceCharacter +
                                                             WIL.DisplayAbbrev + strSpaceCharacter + '(' +
                                                             WIL.TotalValue.ToString(GlobalOptions.CultureInfo) + ')');

                if (CurrentCounterspellingDice != 0)
                    objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                      LanguageManager.GetString("Label_CounterspellingDice", GlobalOptions.Language) +
                                      strSpaceCharacter + '(' +
                                      CurrentCounterspellingDice.ToString(GlobalOptions.CultureInfo) + ')');

                int intModifiers = SpellResistance;

                if (intModifiers != 0)
                {
                    objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                      LanguageManager.GetString("Tip_Modifiers", GlobalOptions.Language));
                    bool blnFirstModifier = true;
                    foreach (Improvement objLoopImprovement in Improvements)
                    {
                        if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SpellResistance &&
                            objLoopImprovement.Enabled)
                        {
                            if (blnFirstModifier)
                            {
                                blnFirstModifier = false;
                                objToolTip.Append(':');
                            }
                            else
                                objToolTip.Append(',');

                            objToolTip.Append(strSpaceCharacter +
                                              GetObjectName(objLoopImprovement, GlobalOptions.Language));
                        }
                    }

                    objToolTip.Append(strSpaceCharacter + '(' + intModifiers.ToString(GlobalOptions.CultureInfo) + ')');
                }

                return objToolTip.ToString();
            }
        }

        public int SpellDefenseDecreaseCHA => CHA.TotalValue + WIL.TotalValue + SpellResistance;

        public string DisplaySpellDefenseDecreaseCHA => CurrentCounterspellingDice == 0
            ? SpellDefenseDecreaseCHA.ToString(GlobalOptions.CultureInfo)
            : SpellDefenseDecreaseCHA.ToString(GlobalOptions.CultureInfo) +
              LanguageManager.GetString("String_Space", GlobalOptions.Language) + '(' +
              (SpellDefenseDecreaseCHA + CurrentCounterspellingDice).ToString(GlobalOptions.CultureInfo) + ')';

        public string SpellDefenseDecreaseCHAToolTip
        {
            get
            {
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                StringBuilder objToolTip = new StringBuilder(CHA.DisplayAbbrev + strSpaceCharacter + '(' +
                                                             CHA.TotalValue.ToString(GlobalOptions.CultureInfo) + ')' +
                                                             strSpaceCharacter + '+' + strSpaceCharacter +
                                                             WIL.DisplayAbbrev + strSpaceCharacter + '(' +
                                                             WIL.TotalValue.ToString(GlobalOptions.CultureInfo) + ')');

                if (CurrentCounterspellingDice != 0)
                    objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                      LanguageManager.GetString("Label_CounterspellingDice", GlobalOptions.Language) +
                                      strSpaceCharacter + '(' +
                                      CurrentCounterspellingDice.ToString(GlobalOptions.CultureInfo) + ')');

                int intModifiers = SpellResistance;

                if (intModifiers != 0)
                {
                    objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                      LanguageManager.GetString("Tip_Modifiers", GlobalOptions.Language));
                    bool blnFirstModifier = true;
                    foreach (Improvement objLoopImprovement in Improvements)
                    {
                        if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SpellResistance &&
                            objLoopImprovement.Enabled)
                        {
                            if (blnFirstModifier)
                            {
                                blnFirstModifier = false;
                                objToolTip.Append(':');
                            }
                            else
                                objToolTip.Append(',');

                            objToolTip.Append(strSpaceCharacter +
                                              GetObjectName(objLoopImprovement, GlobalOptions.Language));
                        }
                    }

                    objToolTip.Append(strSpaceCharacter + '(' + intModifiers.ToString(GlobalOptions.CultureInfo) + ')');
                }

                return objToolTip.ToString();
            }
        }

        public int SpellDefenseDecreaseINT => INT.TotalValue + WIL.TotalValue + SpellResistance;

        public string DisplaySpellDefenseDecreaseINT => CurrentCounterspellingDice == 0
            ? SpellDefenseDecreaseINT.ToString(GlobalOptions.CultureInfo)
            : SpellDefenseDecreaseINT.ToString(GlobalOptions.CultureInfo) +
              LanguageManager.GetString("String_Space", GlobalOptions.Language) + '(' +
              (SpellDefenseDecreaseINT + CurrentCounterspellingDice).ToString(GlobalOptions.CultureInfo) + ')';

        public string SpellDefenseDecreaseINTToolTip
        {
            get
            {
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                StringBuilder objToolTip = new StringBuilder(INT.DisplayAbbrev + strSpaceCharacter + '(' +
                                                             INT.TotalValue.ToString(GlobalOptions.CultureInfo) + ')' +
                                                             strSpaceCharacter + '+' + strSpaceCharacter +
                                                             WIL.DisplayAbbrev + strSpaceCharacter + '(' +
                                                             WIL.TotalValue.ToString(GlobalOptions.CultureInfo) + ')');

                if (CurrentCounterspellingDice != 0)
                    objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                      LanguageManager.GetString("Label_CounterspellingDice", GlobalOptions.Language) +
                                      strSpaceCharacter + '(' +
                                      CurrentCounterspellingDice.ToString(GlobalOptions.CultureInfo) + ')');

                int intModifiers = SpellResistance;

                if (intModifiers != 0)
                {
                    objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                      LanguageManager.GetString("Tip_Modifiers", GlobalOptions.Language));
                    bool blnFirstModifier = true;
                    foreach (Improvement objLoopImprovement in Improvements)
                    {
                        if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SpellResistance &&
                            objLoopImprovement.Enabled)
                        {
                            if (blnFirstModifier)
                            {
                                blnFirstModifier = false;
                                objToolTip.Append(':');
                            }
                            else
                                objToolTip.Append(',');

                            objToolTip.Append(strSpaceCharacter +
                                              GetObjectName(objLoopImprovement, GlobalOptions.Language));
                        }
                    }

                    objToolTip.Append(strSpaceCharacter + '(' + intModifiers.ToString(GlobalOptions.CultureInfo) + ')');
                }

                return objToolTip.ToString();
            }
        }

        public int SpellDefenseDecreaseLOG => LOG.TotalValue + WIL.TotalValue + SpellResistance;

        public string DisplaySpellDefenseDecreaseLOG => CurrentCounterspellingDice == 0
            ? SpellDefenseDecreaseLOG.ToString(GlobalOptions.CultureInfo)
            : SpellDefenseDecreaseLOG.ToString(GlobalOptions.CultureInfo) +
              LanguageManager.GetString("String_Space", GlobalOptions.Language) + '(' +
              (SpellDefenseDecreaseLOG + CurrentCounterspellingDice).ToString(GlobalOptions.CultureInfo) + ')';

        public string SpellDefenseDecreaseLOGToolTip
        {
            get
            {
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                StringBuilder objToolTip = new StringBuilder(LOG.DisplayAbbrev + strSpaceCharacter + '(' +
                                                             LOG.TotalValue.ToString(GlobalOptions.CultureInfo) + ')' +
                                                             strSpaceCharacter + '+' + strSpaceCharacter +
                                                             WIL.DisplayAbbrev + strSpaceCharacter + '(' +
                                                             WIL.TotalValue.ToString(GlobalOptions.CultureInfo) + ')');

                if (CurrentCounterspellingDice != 0)
                    objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                      LanguageManager.GetString("Label_CounterspellingDice", GlobalOptions.Language) +
                                      strSpaceCharacter + '(' +
                                      CurrentCounterspellingDice.ToString(GlobalOptions.CultureInfo) + ')');

                int intModifiers = SpellResistance;

                if (intModifiers != 0)
                {
                    objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                      LanguageManager.GetString("Tip_Modifiers", GlobalOptions.Language));
                    bool blnFirstModifier = true;
                    foreach (Improvement objLoopImprovement in Improvements)
                    {
                        if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SpellResistance &&
                            objLoopImprovement.Enabled)
                        {
                            if (blnFirstModifier)
                            {
                                blnFirstModifier = false;
                                objToolTip.Append(':');
                            }
                            else
                                objToolTip.Append(',');

                            objToolTip.Append(strSpaceCharacter +
                                              GetObjectName(objLoopImprovement, GlobalOptions.Language));
                        }
                    }

                    objToolTip.Append(strSpaceCharacter + '(' + intModifiers.ToString(GlobalOptions.CultureInfo) + ')');
                }

                return objToolTip.ToString();
            }
        }

        public int SpellDefenseDecreaseWIL => WIL.TotalValue + WIL.TotalValue + SpellResistance;

        public string DisplaySpellDefenseDecreaseWIL => CurrentCounterspellingDice == 0
            ? SpellDefenseDecreaseWIL.ToString(GlobalOptions.CultureInfo)
            : SpellDefenseDecreaseWIL.ToString(GlobalOptions.CultureInfo) +
              LanguageManager.GetString("String_Space", GlobalOptions.Language) + '(' +
              (SpellDefenseDecreaseWIL + CurrentCounterspellingDice).ToString(GlobalOptions.CultureInfo) + ')';

        public string SpellDefenseDecreaseWILToolTip
        {
            get
            {
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                StringBuilder objToolTip = new StringBuilder(WIL.DisplayAbbrev + strSpaceCharacter + '(' +
                                                             WIL.TotalValue.ToString(GlobalOptions.CultureInfo) + ')' +
                                                             strSpaceCharacter + '+' + strSpaceCharacter +
                                                             WIL.DisplayAbbrev + strSpaceCharacter + '(' +
                                                             WIL.TotalValue.ToString(GlobalOptions.CultureInfo) + ')');

                if (CurrentCounterspellingDice != 0)
                    objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                      LanguageManager.GetString("Label_CounterspellingDice", GlobalOptions.Language) +
                                      strSpaceCharacter + '(' +
                                      CurrentCounterspellingDice.ToString(GlobalOptions.CultureInfo) + ')');

                int intModifiers = SpellResistance;

                if (intModifiers != 0)
                {
                    objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                      LanguageManager.GetString("Tip_Modifiers", GlobalOptions.Language));
                    bool blnFirstModifier = true;
                    foreach (Improvement objLoopImprovement in Improvements)
                    {
                        if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SpellResistance &&
                            objLoopImprovement.Enabled)
                        {
                            if (blnFirstModifier)
                            {
                                blnFirstModifier = false;
                                objToolTip.Append(':');
                            }
                            else
                                objToolTip.Append(',');

                            objToolTip.Append(strSpaceCharacter +
                                              GetObjectName(objLoopImprovement, GlobalOptions.Language));
                        }
                    }

                    objToolTip.Append(strSpaceCharacter + '(' + intModifiers.ToString(GlobalOptions.CultureInfo) + ')');
                }

                return objToolTip.ToString();
            }
        }

        /// <summary>
        /// Custom Drugs created by the character.
        /// </summary>
        public TaggedObservableCollection<Drug> Drugs => _lstDrugs;

        #endregion

        public int SpellDefenseIllusionMana => LOG.TotalValue + WIL.TotalValue + SpellResistance +
                                               ImprovementManager.ValueOf(this,
                                                   Improvement.ImprovementType.ManaIllusionResist);

        public string DisplaySpellDefenseIllusionMana => CurrentCounterspellingDice == 0
            ? SpellDefenseIllusionMana.ToString(GlobalOptions.CultureInfo)
            : SpellDefenseIllusionMana.ToString(GlobalOptions.CultureInfo) +
              LanguageManager.GetString("String_Space", GlobalOptions.Language) + '(' +
              (SpellDefenseIllusionMana + CurrentCounterspellingDice).ToString(GlobalOptions.CultureInfo) + ')';

        public string SpellDefenseIllusionManaToolTip
        {
            get
            {
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                StringBuilder objToolTip = new StringBuilder(LOG.DisplayAbbrev + strSpaceCharacter + '(' +
                                                             LOG.TotalValue.ToString(GlobalOptions.CultureInfo) + ')' +
                                                             strSpaceCharacter + '+' + strSpaceCharacter +
                                                             WIL.DisplayAbbrev + strSpaceCharacter + '(' +
                                                             WIL.TotalValue.ToString(GlobalOptions.CultureInfo) + ')');

                if (CurrentCounterspellingDice != 0)
                    objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                      LanguageManager.GetString("Label_CounterspellingDice", GlobalOptions.Language) +
                                      strSpaceCharacter + '(' +
                                      CurrentCounterspellingDice.ToString(GlobalOptions.CultureInfo) + ')');

                int intModifiers = SpellResistance +
                                   ImprovementManager.ValueOf(this, Improvement.ImprovementType.ManaIllusionResist);

                if (intModifiers != 0)
                {
                    objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                      LanguageManager.GetString("Tip_Modifiers", GlobalOptions.Language));
                    bool blnFirstModifier = true;
                    foreach (Improvement objLoopImprovement in Improvements)
                    {
                        if ((objLoopImprovement.ImproveType == Improvement.ImprovementType.ManaIllusionResist ||
                             objLoopImprovement.ImproveType == Improvement.ImprovementType.SpellResistance) &&
                            objLoopImprovement.Enabled)
                        {
                            if (blnFirstModifier)
                            {
                                blnFirstModifier = false;
                                objToolTip.Append(':');
                            }
                            else
                                objToolTip.Append(',');

                            objToolTip.Append(strSpaceCharacter +
                                              GetObjectName(objLoopImprovement, GlobalOptions.Language));
                        }
                    }

                    objToolTip.Append(strSpaceCharacter + '(' + intModifiers.ToString(GlobalOptions.CultureInfo) + ')');
                }

                return objToolTip.ToString();
            }
        }

        public int SpellDefenseIllusionPhysical => LOG.TotalValue + INT.TotalValue + SpellResistance +
                                                   ImprovementManager.ValueOf(this,
                                                       Improvement.ImprovementType.PhysicalIllusionResist);

        public string DisplaySpellDefenseIllusionPhysical => CurrentCounterspellingDice == 0
            ? SpellDefenseIllusionPhysical.ToString(GlobalOptions.CultureInfo)
            : SpellDefenseIllusionPhysical.ToString(GlobalOptions.CultureInfo) +
              LanguageManager.GetString("String_Space", GlobalOptions.Language) + '(' +
              (SpellDefenseIllusionPhysical + CurrentCounterspellingDice).ToString(GlobalOptions.CultureInfo) + ')';

        public string SpellDefenseIllusionPhysicalToolTip
        {
            get
            {
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                StringBuilder objToolTip = new StringBuilder(LOG.DisplayAbbrev + strSpaceCharacter + '(' +
                                                             LOG.TotalValue.ToString(GlobalOptions.CultureInfo) + ')' +
                                                             strSpaceCharacter + '+' + strSpaceCharacter +
                                                             INT.DisplayAbbrev + strSpaceCharacter + '(' +
                                                             INT.TotalValue.ToString(GlobalOptions.CultureInfo) + ')');

                if (CurrentCounterspellingDice != 0)
                    objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                      LanguageManager.GetString("Label_CounterspellingDice", GlobalOptions.Language) +
                                      strSpaceCharacter + '(' +
                                      CurrentCounterspellingDice.ToString(GlobalOptions.CultureInfo) + ')');

                int intModifiers = SpellResistance +
                                   ImprovementManager.ValueOf(this, Improvement.ImprovementType.PhysicalIllusionResist);

                if (intModifiers != 0)
                {
                    objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                      LanguageManager.GetString("Tip_Modifiers", GlobalOptions.Language));
                    bool blnFirstModifier = true;
                    foreach (Improvement objLoopImprovement in Improvements)
                    {
                        if ((objLoopImprovement.ImproveType == Improvement.ImprovementType.PhysicalIllusionResist ||
                             objLoopImprovement.ImproveType == Improvement.ImprovementType.SpellResistance) &&
                            objLoopImprovement.Enabled)
                        {
                            if (blnFirstModifier)
                            {
                                blnFirstModifier = false;
                                objToolTip.Append(':');
                            }
                            else
                                objToolTip.Append(',');

                            objToolTip.Append(strSpaceCharacter +
                                              GetObjectName(objLoopImprovement, GlobalOptions.Language));
                        }
                    }

                    objToolTip.Append(strSpaceCharacter + '(' + intModifiers.ToString(GlobalOptions.CultureInfo) + ')');
                }

                return objToolTip.ToString();
            }
        }

        public int SpellDefenseManipulationMental => LOG.TotalValue + WIL.TotalValue + SpellResistance +
                                                     ImprovementManager.ValueOf(this,
                                                         Improvement.ImprovementType.MentalManipulationResist);

        public string DisplaySpellDefenseManipulationMental => CurrentCounterspellingDice == 0
            ? SpellDefenseManipulationMental.ToString(GlobalOptions.CultureInfo)
            : SpellDefenseManipulationMental.ToString(GlobalOptions.CultureInfo) +
              LanguageManager.GetString("String_Space", GlobalOptions.Language) + '(' +
              (SpellDefenseManipulationMental + CurrentCounterspellingDice).ToString(GlobalOptions.CultureInfo) + ')';

        public string SpellDefenseManipulationMentalToolTip
        {
            get
            {
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                StringBuilder objToolTip = new StringBuilder(LOG.DisplayAbbrev + strSpaceCharacter + '(' +
                                                             LOG.TotalValue.ToString(GlobalOptions.CultureInfo) + ')' +
                                                             strSpaceCharacter + '+' + strSpaceCharacter +
                                                             WIL.DisplayAbbrev + strSpaceCharacter + '(' +
                                                             WIL.TotalValue.ToString(GlobalOptions.CultureInfo) + ')');

                if (CurrentCounterspellingDice != 0)
                    objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                      LanguageManager.GetString("Label_CounterspellingDice", GlobalOptions.Language) +
                                      strSpaceCharacter + '(' +
                                      CurrentCounterspellingDice.ToString(GlobalOptions.CultureInfo) + ')');

                int intModifiers = SpellResistance +
                                   ImprovementManager.ValueOf(this,
                                       Improvement.ImprovementType.MentalManipulationResist);

                if (intModifiers != 0)
                {
                    objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                      LanguageManager.GetString("Tip_Modifiers", GlobalOptions.Language));
                    bool blnFirstModifier = true;
                    foreach (Improvement objLoopImprovement in Improvements)
                    {
                        if ((objLoopImprovement.ImproveType == Improvement.ImprovementType.MentalManipulationResist ||
                             objLoopImprovement.ImproveType == Improvement.ImprovementType.SpellResistance) &&
                            objLoopImprovement.Enabled)
                        {
                            if (blnFirstModifier)
                            {
                                blnFirstModifier = false;
                                objToolTip.Append(':');
                            }
                            else
                                objToolTip.Append(',');

                            objToolTip.Append(strSpaceCharacter +
                                              GetObjectName(objLoopImprovement, GlobalOptions.Language));
                        }
                    }

                    objToolTip.Append(strSpaceCharacter + '(' + intModifiers.ToString(GlobalOptions.CultureInfo) + ')');
                }

                return objToolTip.ToString();
            }
        }

        public int SpellDefenseManipulationPhysical =>
            (IsAI ? (HomeNode is Vehicle objVehicle ? objVehicle.TotalBody * 2 : 0) : BOD.TotalValue + STR.TotalValue) +
            SpellResistance + ImprovementManager.ValueOf(this, Improvement.ImprovementType.PhysicalManipulationResist);

        public string DisplaySpellDefenseManipulationPhysical => CurrentCounterspellingDice == 0
            ? SpellDefenseManipulationPhysical.ToString(GlobalOptions.CultureInfo)
            : SpellDefenseManipulationPhysical.ToString(GlobalOptions.CultureInfo) +
              LanguageManager.GetString("String_Space", GlobalOptions.Language) + '(' +
              (SpellDefenseManipulationPhysical + CurrentCounterspellingDice).ToString(GlobalOptions.CultureInfo) + ')';

        public string SpellDefenseManipulationPhysicalToolTip
        {
            get
            {
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                int intBody;
                int intStrength;
                string strBodyAbbrev;
                string strStrengthAbbrev;
                if (IsAI)
                {
                    intBody = intStrength = (HomeNode is Vehicle objVehicle ? objVehicle.TotalBody : 0);
                    strBodyAbbrev = strStrengthAbbrev =
                        LanguageManager.GetString("String_VehicleBody", GlobalOptions.Language);
                }
                else
                {
                    intBody = BOD.TotalValue;
                    intStrength = STR.TotalValue;
                    strBodyAbbrev = BOD.DisplayAbbrev;
                    strStrengthAbbrev = STR.DisplayAbbrev;
                }

                StringBuilder objToolTip = new StringBuilder(strBodyAbbrev + strSpaceCharacter + '(' +
                                                             intBody.ToString(GlobalOptions.CultureInfo) + ')' +
                                                             strSpaceCharacter + '+' + strSpaceCharacter +
                                                             strStrengthAbbrev + strSpaceCharacter + '(' +
                                                             intStrength.ToString(GlobalOptions.CultureInfo) + ')');

                if (CurrentCounterspellingDice != 0)
                    objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                      LanguageManager.GetString("Label_CounterspellingDice", GlobalOptions.Language) +
                                      strSpaceCharacter + '(' +
                                      CurrentCounterspellingDice.ToString(GlobalOptions.CultureInfo) + ')');

                int intModifiers = SpellResistance +
                                   ImprovementManager.ValueOf(this,
                                       Improvement.ImprovementType.PhysicalManipulationResist);

                if (intModifiers != 0)
                {
                    objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                      LanguageManager.GetString("Tip_Modifiers", GlobalOptions.Language));
                    bool blnFirstModifier = true;
                    foreach (Improvement objLoopImprovement in Improvements)
                    {
                        if ((objLoopImprovement.ImproveType == Improvement.ImprovementType.PhysicalManipulationResist ||
                             objLoopImprovement.ImproveType == Improvement.ImprovementType.SpellResistance) &&
                            objLoopImprovement.Enabled)
                        {
                            if (blnFirstModifier)
                            {
                                blnFirstModifier = false;
                                objToolTip.Append(':');
                            }
                            else
                                objToolTip.Append(',');

                            objToolTip.Append(strSpaceCharacter +
                                              GetObjectName(objLoopImprovement, GlobalOptions.Language));
                        }
                    }

                    objToolTip.Append(strSpaceCharacter + '(' + intModifiers.ToString(GlobalOptions.CultureInfo) + ')');
                }

                return objToolTip.ToString();
            }
        }

        /// <summary>
        /// The Character's total Armor Rating.
        /// </summary>
        public int TotalArmorRating =>
            ArmorRating + ImprovementManager.ValueOf(this, Improvement.ImprovementType.Armor);

        public string TotalArmorRatingToolTip
        {
            get
            {
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                StringBuilder objToolTip =
                    new StringBuilder(LanguageManager.GetString("Tip_Armor", GlobalOptions.Language) +
                                      strSpaceCharacter + '(' + ArmorRating.ToString(GlobalOptions.CultureInfo) + ')');
                foreach (Improvement objLoopImprovement in Improvements)
                {
                    if (objLoopImprovement.ImproveType == Improvement.ImprovementType.Armor &&
                        objLoopImprovement.Enabled)
                    {
                        objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                          GetObjectName(objLoopImprovement, GlobalOptions.Language) +
                                          strSpaceCharacter + '(' +
                                          objLoopImprovement.Value.ToString(GlobalOptions.CultureInfo) + ')');
                    }
                }

                return objToolTip.ToString();
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
        public int TotalElectricityArmorRating => TotalArmorRating +
                                                  ImprovementManager.ValueOf(this,
                                                      Improvement.ImprovementType.ElectricityArmor);

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
                            a.Equipped))
                        {
                            if (a.ArmorMods.Any(objMod =>
                                objMod.Name == "Custom Fit (Stack)" && objMod.Extra == strArmorName))
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
                foreach (Armor objArmor in Armor.Where(objArmor =>
                    (objArmor.ArmorValue.StartsWith('+') || objArmor.ArmorOverrideValue.StartsWith('+')) &&
                    objArmor.Name != strHighest && objArmor.Category == "Clothing" && objArmor.Equipped))
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
                    objArmor.Name != strHighest && objArmor.Category != "Clothing" && objArmor.Equipped))
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
                    return (intSTRTotalValue - intTotalA) / 2; // a negative number is expected
                return 0;
            }
        }

        #region Condition Monitors

        /// <summary>
        /// Number of Physical Condition Monitor boxes.
        /// </summary>
        public int PhysicalCM
        {
            get
            {
                int intCMPhysical = 8;
                if (IsAI)
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

        public string PhysicalCMLabelText
        {
            get
            {
                if (IsAI)
                {
                    return HomeNode == null
                        ? LanguageManager.GetString("Label_OtherCoreCM", GlobalOptions.Language)
                        : LanguageManager.GetString(HomeNode is Vehicle ? "Label_OtherPhysicalCM" : "Label_OtherCoreCM", GlobalOptions.Language);
                }
                return LanguageManager.GetString("Label_OtherPhysicalCM", GlobalOptions.Language);
            }
        }

        public string PhysicalCMToolTip
        {
            get
            {
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                string strModifiers = LanguageManager.GetString("Tip_Modifiers", GlobalOptions.Language);
                string strCM;
                int intBonus;
                if (IsAI)
                {
                    if (HomeNode is Vehicle objVehicleHomeNode)
                    {
                        strCM = objVehicleHomeNode.BasePhysicalBoxes.ToString(GlobalOptions.CultureInfo) + strSpaceCharacter + '+' + strSpaceCharacter +
                                       '(' + BOD.DisplayAbbrev + '÷' + 2.ToString(GlobalOptions.CultureInfo) + ')' + strSpaceCharacter +
                                       '(' + ((objVehicleHomeNode.TotalBody + 1) / 2).ToString(GlobalOptions.CultureInfo) + ')';

                        intBonus = objVehicleHomeNode.Mods.Sum(objMod => objMod.ConditionMonitor);
                        if (intBonus != 0)
                            strCM += strSpaceCharacter + '+' + strSpaceCharacter + strModifiers + strSpaceCharacter + '(' + intBonus.ToString(GlobalOptions.CultureInfo) + ')';
                    }
                    else
                    {
                        strCM = 8.ToString(GlobalOptions.CultureInfo) + strSpaceCharacter + '+' + strSpaceCharacter +
                                '(' + DEP.DisplayAbbrev + '÷' + 2.ToString(GlobalOptions.CultureInfo) + ')' + strSpaceCharacter +
                                '(' + ((DEP.TotalValue + 1) / 2).ToString(GlobalOptions.CultureInfo) + ')';

                        intBonus = ImprovementManager.ValueOf(this, Improvement.ImprovementType.PhysicalCM);
                        if (intBonus != 0)
                            strCM += strSpaceCharacter + '+' + strSpaceCharacter + strModifiers + strSpaceCharacter + '(' + intBonus.ToString(GlobalOptions.CultureInfo) + ')';
                    }
                }
                else
                {
                    strCM = 8.ToString(GlobalOptions.CultureInfo) + strSpaceCharacter + '+' + strSpaceCharacter +
                            '(' + BOD.DisplayAbbrev + '÷' + 2.ToString(GlobalOptions.CultureInfo) + ')' + strSpaceCharacter +
                            '(' + ((BOD.TotalValue + 1) / 2).ToString(GlobalOptions.CultureInfo) + ')';

                    intBonus = ImprovementManager.ValueOf(this, Improvement.ImprovementType.PhysicalCM);
                    if (intBonus != 0)
                        strCM += strSpaceCharacter + '+' + strSpaceCharacter + strModifiers + strSpaceCharacter + '(' + intBonus.ToString(GlobalOptions.CultureInfo) + ')';
                }

                return strCM;
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
                if (IsAI)
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

        public bool StunCMVisible => !IsAI || HomeNode != null;

        public string StunCMLabelText
        {
            get
            {
                if (IsAI)
                {
                    return HomeNode == null ? string.Empty : LanguageManager.GetString("Label_OtherMatrixCM", GlobalOptions.Language);
                }
                return LanguageManager.GetString("Label_OtherStunCM", GlobalOptions.Language);
            }
        }

        public string StunCMToolTip
        {
            get
            {
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                string strModifiers = LanguageManager.GetString("Tip_Modifiers", GlobalOptions.Language);
                string strCM = string.Empty;
                int intBonus;

                if (IsAI)
                {
                    if (HomeNode != null)
                    {
                        strCM = 8.ToString(GlobalOptions.CultureInfo) + strSpaceCharacter + '+' + strSpaceCharacter +
                                       '(' + LanguageManager.GetString("String_DeviceRating", GlobalOptions.Language) + '÷' + 2.ToString(GlobalOptions.CultureInfo) + ')' + strSpaceCharacter +
                                       '(' + ((HomeNode.GetTotalMatrixAttribute("Device Rating") + 1) / 2).ToString(GlobalOptions.CultureInfo) + ')';

                        intBonus = HomeNode.TotalBonusMatrixBoxes;
                        if (intBonus != 0)
                            strCM += strSpaceCharacter + '+' + strSpaceCharacter + strModifiers + strSpaceCharacter + '(' + intBonus.ToString(GlobalOptions.CultureInfo) + ')';
                    }
                }
                else
                {
                    strCM = 8.ToString(GlobalOptions.CultureInfo) + strSpaceCharacter + '+' + strSpaceCharacter +
                            '(' + WIL.DisplayAbbrev + '÷' + 2.ToString(GlobalOptions.CultureInfo) + ')' + strSpaceCharacter +
                            '(' + ((WIL.TotalValue + 1) / 2).ToString(GlobalOptions.CultureInfo) + ')';

                    intBonus = ImprovementManager.ValueOf(this, Improvement.ImprovementType.StunCM);
                    if (intBonus != 0)
                        strCM += strSpaceCharacter + '+' + strSpaceCharacter + strModifiers + strSpaceCharacter + '(' + intBonus.ToString(GlobalOptions.CultureInfo) + ')';
                }

                return strCM;
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
        /// Number of additioal boxes appear before the first Physical Condition Monitor penalty.
        /// </summary>
        public int PhysicalCMThresholdOffset
        {
            get
            {
                if (Improvements.Any(objImprovement =>
                    objImprovement.ImproveType == Improvement.ImprovementType.IgnoreCMPenaltyPhysical &&
                    objImprovement.Enabled))
                    return int.MaxValue;
                if (IsAI || Improvements.Any(objImprovement =>
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
        /// Number of additioal boxes appear before the first Stun Condition Monitor penalty.
        /// </summary>
        public int StunCMThresholdOffset
        {
            get
            {
                // A.I.s don't get wound penalties from Matrix damage
                if (IsAI)
                    return int.MaxValue;
                if (Improvements.Any(objImprovement =>
                    objImprovement.ImproveType == Improvement.ImprovementType.IgnoreCMPenaltyStun &&
                    objImprovement.Enabled))
                    return int.MaxValue;
                if (Improvements.Any(objImprovement =>
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
                if (!IsAI)
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
        public CharacterBuildMethod BuildMethod
        {
            get => _objBuildMethod;
            set
            {
                if (value != _objBuildMethod)
                {
                    _objBuildMethod = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool BuildMethodHasSkillPoints => BuildMethod == CharacterBuildMethod.Priority ||
                                                 BuildMethod == CharacterBuildMethod.SumtoTen;

        /// <summary>
        /// Number of Build Points that are used to create the character.
        /// </summary>
        public int SumtoTen
        {
            get => _intSumtoTen;
            set
            {
                if (_intSumtoTen != value)
                {
                    _intSumtoTen = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Amount of Karma that is used to create the character.
        /// </summary>
        public int BuildKarma
        {
            get => _intBuildKarma;
            set
            {
                if (_intBuildKarma != value)
                {
                    _intBuildKarma = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Amount of Nuyen the character has.
        /// </summary>
        public decimal Nuyen
        {
            get => _decNuyen;
            set
            {
                if (_decNuyen != value)
                {
                    _decNuyen = value;
                    OnPropertyChanged();
                }
            }
        }

        public string DisplayNuyen => Nuyen.ToString(_objOptions.NuyenFormat, GlobalOptions.CultureInfo) + '¥';

        /// <summary>
        /// Amount of Nuyen the character started with via the priority system.
        /// </summary>
        public decimal StartingNuyen
        {
            get => _decStartingNuyen;
            set
            {
                if (_decStartingNuyen != value)
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
            '=' + LanguageManager.GetString("String_Space", GlobalOptions.Language) +
            TotalStartingNuyen.ToString(Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';

        /// <summary>
        /// Number of Build Points put into Nuyen.
        /// </summary>
        public decimal NuyenBP
        {
            get => _decNuyenBP;
            set
            {
                decimal decNewValue = Math.Min(value, TotalNuyenMaximumBP);
                if (_decNuyenBP != decNewValue)
                {
                    _decNuyenBP = decNewValue;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Maximum number of Build Points that can be spent on Nuyen.
        /// </summary>
        public decimal NuyenMaximumBP
        {
            get => _decNuyenMaximumBP;
            set
            {
                if (_decNuyenMaximumBP != value)
                {
                    _decNuyenMaximumBP = value;
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
                // If UnrestrictedNueyn is enabled, return the maximum possible value
                if (IgnoreRules || Options.UnrestrictedNuyen)
                {
                    return decMaxValue;
                }

                return Math.Min(decMaxValue,
                    NuyenMaximumBP + ImprovementManager.ValueOf(this, Improvement.ImprovementType.NuyenMaxBP));
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
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                return LanguageManager.GetString("Label_Options_Maximum", GlobalOptions.Language) + strSpaceCharacter +
                       '(' +
                       LanguageManager.GetString("String_LimitMentalShort", GlobalOptions.Language) +
                       strSpaceCharacter + '[' + LimitMental + ']' + ',' + strSpaceCharacter +
                       LanguageManager.GetString("String_LimitSocialShort", GlobalOptions.Language) +
                       strSpaceCharacter + '[' + LimitSocial + ']' + ')';
            }
        }

        /// <summary>
        /// The calculated Physical Limit.
        /// </summary>
        public int LimitPhysical
        {
            get
            {
                if (IsAI)
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
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                if (IsAI)
                {
                    Vehicle objHomeNodeVehicle = HomeNode as Vehicle;
                    return LanguageManager.GetString("String_Handling", GlobalOptions.Language) + strSpaceCharacter +
                           '[' + (objHomeNodeVehicle?.Handling ?? 0).ToString(GlobalOptions.CultureInfo) + ']';
                }

                StringBuilder objToolTip = new StringBuilder(
                    '(' + STR.DisplayAbbrev + strSpaceCharacter + '[' +
                    STR.TotalValue.ToString(GlobalOptions.CultureInfo) + ']' + strSpaceCharacter + '×' +
                    strSpaceCharacter + 2.ToString(GlobalOptions.CultureInfo) +
                    strSpaceCharacter + '+' + strSpaceCharacter +
                    BOD.DisplayAbbrev + strSpaceCharacter + '[' + BOD.TotalValue.ToString(GlobalOptions.CultureInfo) +
                    ']' +
                    strSpaceCharacter + '+' + strSpaceCharacter +
                    REA.DisplayAbbrev + strSpaceCharacter + '[' + REA.TotalValue.ToString(GlobalOptions.CultureInfo) +
                    "])" + strSpaceCharacter + '/' + strSpaceCharacter + 3.ToString(GlobalOptions.CultureInfo));

                foreach (Improvement objLoopImprovement in Improvements)
                {
                    if (objLoopImprovement.ImproveType == Improvement.ImprovementType.PhysicalLimit &&
                        objLoopImprovement.Enabled)
                    {
                        objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                          GetObjectName(objLoopImprovement, GlobalOptions.Language) +
                                          strSpaceCharacter + '(' +
                                          objLoopImprovement.Value.ToString(GlobalOptions.CultureInfo) + ')');
                    }
                }

                return objToolTip.ToString();
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
                if (IsAI)
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

        public string LimitMentalToolTip
        {
            get
            {
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                StringBuilder objToolTip = new StringBuilder(
                    '(' + LOG.DisplayAbbrev + strSpaceCharacter + '[' +
                    LOG.TotalValue.ToString(GlobalOptions.CultureInfo) + ']' + strSpaceCharacter + '×' +
                    strSpaceCharacter + 2.ToString(GlobalOptions.CultureInfo) +
                    strSpaceCharacter + '+' + strSpaceCharacter +
                    INT.DisplayAbbrev + strSpaceCharacter + '[' + INT.TotalValue.ToString(GlobalOptions.CultureInfo) +
                    ']' +
                    strSpaceCharacter + '+' + strSpaceCharacter +
                    WIL.DisplayAbbrev + strSpaceCharacter + '[' + WIL.TotalValue.ToString(GlobalOptions.CultureInfo) +
                    "])" + strSpaceCharacter + '/' + strSpaceCharacter + 3.ToString(GlobalOptions.CultureInfo));

                if (IsAI)
                {
                    int intLimit = (LOG.TotalValue * 2 + INT.TotalValue + WIL.TotalValue + 2) / 3;
                    if (HomeNode != null)
                    {
                        if (HomeNode is Vehicle objHomeNodeVehicle)
                        {
                            int intHomeNodeSensor = objHomeNodeVehicle.CalculatedSensor;
                            if (intHomeNodeSensor > intLimit)
                            {
                                intLimit = intHomeNodeSensor;
                                objToolTip =
                                    new StringBuilder(
                                        LanguageManager.GetString("String_Sensor", GlobalOptions.Language) +
                                        strSpaceCharacter + '[' + intLimit.ToString(GlobalOptions.CultureInfo) + ']');
                            }
                        }

                        int intHomeNodeDP = HomeNode.GetTotalMatrixAttribute("Data Processing");
                        if (intHomeNodeDP > intLimit)
                        {
                            intLimit = intHomeNodeDP;
                            objToolTip =
                                new StringBuilder(
                                    LanguageManager.GetString("String_DataProcessing", GlobalOptions.Language) +
                                    strSpaceCharacter + '[' + intLimit.ToString(GlobalOptions.CultureInfo) + ']');
                        }
                    }
                }

                foreach (Improvement objLoopImprovement in Improvements)
                {
                    if (objLoopImprovement.ImproveType == Improvement.ImprovementType.MentalLimit &&
                        objLoopImprovement.Enabled)
                    {
                        objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                          GetObjectName(objLoopImprovement, GlobalOptions.Language) +
                                          strSpaceCharacter + '(' +
                                          objLoopImprovement.Value.ToString(GlobalOptions.CultureInfo) + ')');
                    }
                }

                return objToolTip.ToString();
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
                if (IsAI && HomeNode != null)
                {
                    int intHomeNodeDP = HomeNode.GetTotalMatrixAttribute("Data Processing");

                    if (HomeNode is Vehicle objHomeNodeVehicle)
                    {
                        int intHomeNodePilot = objHomeNodeVehicle.Pilot;
                        if (intHomeNodePilot > intHomeNodeDP)
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
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                StringBuilder objToolTip = new StringBuilder('(' + CHA.DisplayAbbrev + strSpaceCharacter + '[' +
                                                             CHA.TotalValue.ToString(GlobalOptions.CultureInfo) + ']');

                if (IsAI && HomeNode != null)
                {
                    int intHomeNodeDP = HomeNode.GetTotalMatrixAttribute("Data Processing");
                    string strDPString = LanguageManager.GetString("String_DataProcessing", GlobalOptions.Language);
                    if (HomeNode is Vehicle objHomeNodeVehicle)
                    {
                        int intHomeNodePilot = objHomeNodeVehicle.Pilot;
                        if (intHomeNodePilot > intHomeNodeDP)
                        {
                            intHomeNodeDP = intHomeNodePilot;
                            strDPString = LanguageManager.GetString("String_Pilot", GlobalOptions.Language);
                        }
                    }

                    objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter + strDPString + strSpaceCharacter +
                                      '[' + intHomeNodeDP + ']');
                }
                else
                {
                    objToolTip.Append(strSpaceCharacter + '×' + strSpaceCharacter +
                                      2.ToString(GlobalOptions.CultureInfo));
                }

                objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                  WIL.DisplayAbbrev + strSpaceCharacter + '[' +
                                  WIL.TotalValue.ToString(GlobalOptions.CultureInfo) + ']' +
                                  strSpaceCharacter + '+' + strSpaceCharacter +
                                  ESS.DisplayAbbrev + strSpaceCharacter + '[' + DisplayEssence + "])" +
                                  strSpaceCharacter + '/' + strSpaceCharacter + 3.ToString(GlobalOptions.CultureInfo));

                foreach (Improvement objLoopImprovement in Improvements)
                {
                    if (objLoopImprovement.ImproveType == Improvement.ImprovementType.SocialLimit &&
                        objLoopImprovement.Enabled)
                    {
                        objToolTip.Append(strSpaceCharacter + '+' + strSpaceCharacter +
                                          GetObjectName(objLoopImprovement, GlobalOptions.Language) +
                                          strSpaceCharacter + '(' +
                                          objLoopImprovement.Value.ToString(GlobalOptions.CultureInfo) + ')');
                    }
                }

                return objToolTip.ToString();
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
                if (MentorSpirits.Count == 0)
                    return string.Empty;

                MentorSpirit objMentorSpirit = MentorSpirits[0];
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                return LanguageManager.GetString("Label_SelectMentorSpirit_Advantage", GlobalOptions.Language) +
                       strSpaceCharacter +
                       objMentorSpirit.DisplayAdvantage(GlobalOptions.Language) + Environment.NewLine +
                       Environment.NewLine +
                       LanguageManager.GetString("Label_SelectMetamagic_Disadvantage", GlobalOptions.Language) +
                       strSpaceCharacter +
                       objMentorSpirit.Disadvantage;
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
                if (_strMetatype != value)
                {
                    _strMetatype = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Character's Metavariant.
        /// </summary>
        public string Metavariant
        {
            get => _strMetavariant;
            set
            {
                if (_strMetavariant != value)
                {
                    _strMetavariant = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Metatype Category.
        /// </summary>
        public string MetatypeCategory
        {
            get => _strMetatypeCategory;
            set
            {
                if (_strMetatypeCategory != value)
                {
                    bool blnDoCyberzombieRefresh = _strMetatypeCategory == "Cyberzombie" || value == "Cyberzombie";
                    _strMetatypeCategory = value;
                    OnPropertyChanged();
                    if (blnDoCyberzombieRefresh)
                        RefreshEssenceLossImprovements();
                }
            }
        }

        public int LimbCount(string strLimbSlot = "")
        {
            if (string.IsNullOrEmpty(strLimbSlot))
            {
                return Options.LimbCount + ImprovementManager.ValueOf(this, Improvement.ImprovementType.AddLimb);
            }

            int intReturn =
                1 + ImprovementManager.ValueOf(this, Improvement.ImprovementType.AddLimb, false, strLimbSlot);
            if (strLimbSlot == "arm" || strLimbSlot == "leg")
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
            if (Movement == "Special")
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
                if (string.IsNullOrWhiteSpace(_strMovement))
                {
                    XmlNode xmlMetatypeNode = XmlManager
                        .Load(IsCritter ? "critters.xml" : "metatypes.xml", GlobalOptions.Language)
                        .SelectSingleNode("/chummer/metatypes/metatype[name = \"" + Metatype + "\"]");
                    XmlNode xmlMetavariantNode =
                        xmlMetatypeNode?.SelectSingleNode("metavariants/metavariant[name = \"" + Metavariant + "\"]");
                    _strMovement = xmlMetavariantNode?["movement"]?.InnerText ??
                                   xmlMetatypeNode?["movement"]?.InnerText ?? string.Empty;
                }

                return _strMovement;
            }
            set
            {
                if (_strMovement != value)
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
                if (string.IsNullOrWhiteSpace(_strRun))
                {
                    XmlNode xmlMetatypeNode = XmlManager
                        .Load(IsCritter ? "critters.xml" : "metatypes.xml", GlobalOptions.Language)
                        .SelectSingleNode("/chummer/metatypes/metatype[name = \"" + Metatype + "\"]");
                    XmlNode xmlMetavariantNode =
                        xmlMetatypeNode?.SelectSingleNode("metavariants/metavariant[name = \"" + Metavariant + "\"]");
                    _strRun = xmlMetavariantNode?["run"]?.InnerText ??
                              xmlMetatypeNode?["run"]?.InnerText ?? string.Empty;
                }

                return _strRun;
            }
            set
            {
                if (_strRun != value)
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
                if (string.IsNullOrWhiteSpace(_strRunAlt))
                {
                    XmlNode xmlMetatypeNode = XmlManager
                        .Load(IsCritter ? "critters.xml" : "metatypes.xml", GlobalOptions.Language)
                        .SelectSingleNode("/chummer/metatypes/metatype[name = \"" + Metatype + "\"]");
                    XmlNode xmlMetavariantNode =
                        xmlMetatypeNode?.SelectSingleNode("metavariants/metavariant[name = \"" + Metavariant + "\"]");
                    XmlNode xmlRunNode = xmlMetavariantNode?["run"] ?? xmlMetatypeNode?["run"];
                    _strRunAlt = xmlRunNode?.Attributes?["alt"]?.InnerText ?? string.Empty;
                }

                return _strRunAlt;
            }
            set
            {
                if (_strRunAlt != value)
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
                if (string.IsNullOrWhiteSpace(_strWalk))
                {
                    XmlNode xmlMetatypeNode = XmlManager
                        .Load(IsCritter ? "critters.xml" : "metatypes.xml", GlobalOptions.Language)
                        .SelectSingleNode("/chummer/metatypes/metatype[name = \"" + Metatype + "\"]");
                    XmlNode xmlMetavariantNode =
                        xmlMetatypeNode?.SelectSingleNode("metavariants/metavariant[name = \"" + Metavariant + "\"]");
                    _strWalk = xmlMetavariantNode?["walk"]?.InnerText ??
                               xmlMetatypeNode?["walk"]?.InnerText ?? string.Empty;
                }

                return _strWalk;
            }
            set
            {
                if (_strWalk != value)
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
                if (string.IsNullOrWhiteSpace(_strWalkAlt))
                {
                    XmlNode xmlMetatypeNode = XmlManager
                        .Load(IsCritter ? "critters.xml" : "metatypes.xml", GlobalOptions.Language)
                        .SelectSingleNode("/chummer/metatypes/metatype[name = \"" + Metatype + "\"]");
                    XmlNode xmlMetavariantNode =
                        xmlMetatypeNode?.SelectSingleNode("metavariants/metavariant[name = \"" + Metavariant + "\"]");
                    XmlNode xmlWalkNode = xmlMetavariantNode?["walk"] ?? xmlMetatypeNode?["walk"];
                    _strWalkAlt = xmlWalkNode?.Attributes?["alt"]?.InnerText ?? string.Empty;
                }

                return _strWalkAlt;
            }
            set
            {
                if (_strWalkAlt != value)
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
                if (string.IsNullOrWhiteSpace(_strSprint))
                {
                    XmlNode xmlMetatypeNode = XmlManager
                        .Load(IsCritter ? "critters.xml" : "metatypes.xml", GlobalOptions.Language)
                        .SelectSingleNode("/chummer/metatypes/metatype[name = \"" + Metatype + "\"]");
                    XmlNode xmlMetavariantNode =
                        xmlMetatypeNode?.SelectSingleNode("metavariants/metavariant[name = \"" + Metavariant + "\"]");
                    _strSprint = xmlMetavariantNode?["sprint"]?.InnerText ??
                                 xmlMetatypeNode?["sprint"]?.InnerText ?? string.Empty;
                }

                return _strSprint;
            }
            set
            {
                if (_strSprint != value)
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
                if (string.IsNullOrWhiteSpace(_strSprintAlt))
                {
                    XmlNode xmlMetatypeNode = XmlManager
                        .Load(IsCritter ? "critters.xml" : "metatypes.xml", GlobalOptions.Language)
                        .SelectSingleNode("/chummer/metatypes/metatype[name = \"" + Metatype + "\"]");
                    XmlNode xmlMetavariantNode =
                        xmlMetatypeNode?.SelectSingleNode("metavariants/metavariant[name = \"" + Metavariant + "\"]");
                    XmlNode xmlSprintNode = xmlMetavariantNode?["sprint"] ?? xmlMetatypeNode?["sprint"];
                    _strSprintAlt = xmlSprintNode?.Attributes?["alt"]?.InnerText ?? string.Empty;
                }

                return _strSprintAlt;
            }
            set
            {
                if (_strSprintAlt != value)
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
            AttributeSection.AttributeCategory == CharacterAttrib.AttributeCategory.Standard ? RunString : RunAltString;

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
            foreach (Improvement objImprovement in Improvements.Where(i =>
                i.ImproveType == Improvement.ImprovementType.WalkSpeed && i.ImprovedName == strType && i.Enabled))
            {
                intTmp = Math.Max(intTmp, objImprovement.Value);
            }

            if (intTmp != int.MinValue)
                return intTmp;

            string[] strReturn = CurrentWalkingRateString.Split('/');

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
        public int RunningRate(string strType = "Ground")
        {
            int intTmp = int.MinValue;
            foreach (Improvement objImprovement in Improvements.Where(i =>
                i.ImproveType == Improvement.ImprovementType.RunSpeed && i.ImprovedName == strType && i.Enabled))
            {
                intTmp = Math.Max(intTmp, objImprovement.Value);
            }

            if (intTmp != int.MinValue)
                return intTmp;

            string[] strReturn = CurrentRunningRateString.Split('/');

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
        public decimal SprintingRate(string strType = "Ground")
        {
            decimal decTmp = decimal.MinValue;
            foreach (Improvement objImprovement in Improvements.Where(i =>
                i.ImproveType == Improvement.ImprovementType.SprintSpeed && i.ImprovedName == strType && i.Enabled))
            {
                decTmp = Math.Max(decTmp, objImprovement.Value / 100.0m);
            }

            if (decTmp != decimal.MinValue)
                return decTmp;

            string[] strReturn = CurrentSprintingRateString.Split('/');

            switch (strType)
            {
                case "Fly":
                    if (strReturn.Length > 2)
                        decimal.TryParse(strReturn[2], NumberStyles.Any, GlobalOptions.InvariantCultureInfo,
                            out decTmp);
                    break;
                case "Swim":
                    if (strReturn.Length > 1)
                        decimal.TryParse(strReturn[1], NumberStyles.Any, GlobalOptions.InvariantCultureInfo,
                            out decTmp);
                    break;
                case "Ground":
                    if (strReturn.Length > 0)
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
            if (decWalk == 0 && decRun == 0 && decSprint == 0)
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
            if (_objOptions.CyberlegMovement && blnUseCyberlegs)
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

                if (intTempAGI != int.MaxValue && intTempSTR != int.MaxValue && intLegs >= 2)
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
                strReturn = decWalk.ToString("#,0.##", objCulture) + ", " + decSprint.ToString("#,0.##", objCulture) +
                            "m/ hit";
            }
            else
            {
                decWalk *= intAGI;
                decRun *= intAGI;
                strReturn = decWalk.ToString("#,0.##", objCulture) + '/' + decRun.ToString("#,0.##", objCulture) +
                            ", " + decSprint.ToString("#,0.##", objCulture) + "m/ hit";
            }

            return strReturn;
        }

        public string DisplaySwim => GetSwim(GlobalOptions.CultureInfo, GlobalOptions.Language);

        /// <summary>
        /// Character's Swim rate.
        /// </summary>
        public string GetSwim(CultureInfo objCulture, string strLanguage)
        {
            // Don't attempt to do anything if the character's Movement is "Special" (typically for A.I.s).
            if (Movement == "Special")
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
            if (Movement == "Special")
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
            set
            {
                if (_intMetatypeBP != value)
                {
                    _intMetatypeBP = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether or not the character is a non-Free Sprite.
        /// </summary>
        public bool IsSprite
        {
            get
            {
                if (MetatypeCategory.EndsWith("Sprites") && !IsFreeSprite)
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
                if (MetatypeCategory == "Free Sprite")
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
                if (_blnAdeptEnabled != value)
                {
                    _blnAdeptEnabled = value;
                    if (!value)
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
                if (_blnMagicianEnabled != value)
                {
                    _blnMagicianEnabled = value;
                    if (!value)
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
                if (_blnTechnomancerEnabled != value)
                {
                    _blnTechnomancerEnabled = value;
                    if (!value)
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
                if (_blnAdvancedProgramsEnabled != value)
                {
                    _blnAdvancedProgramsEnabled = value;
                    if (!value)
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
                if (_blnCyberwareDisabled != value)
                {
                    _blnCyberwareDisabled = value;
                    if (value)
                    {
                        ClearCyberwareTab();
                    }

                    OnPropertyChanged();
                }
            }
        }

        public bool AddCyberwareEnabled => !CyberwareDisabled && !Improvements.Any(objImprovement =>
                                               objImprovement.ImproveType ==
                                               Improvement.ImprovementType.DisableCyberware && objImprovement.Enabled);

        public bool AddBiowareEnabled => !CyberwareDisabled && !Improvements.Any(objImprovement =>
                                             objImprovement.ImproveType == Improvement.ImprovementType.DisableBioware &&
                                             objImprovement.Enabled);

        private int _intCachedInitiationEnabled = -1;

        /// <summary>
        /// Whether or not the Initiation tab should be shown (override for BP mode).
        /// </summary>
        public bool InitiationEnabled
        {
            get
            {
                if (_intCachedInitiationEnabled == -1)
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
                if (_blnInitiationDisabled != value)
                {
                    _blnInitiationDisabled = value;
                    if (value)
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
                if (_blnCritterEnabled != value)
                {
                    _blnCritterEnabled = value;
                    if (!value)
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
                if (_intCachedBlackMarketDiscount < 0)
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
                if (_decPrototypeTranshuman != value)
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
                if (_intCachedFriendsInHighPlaces < 0)
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
                if (_intCachedExCon < 0)
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
                if (_intCachedTrustFund != int.MinValue)
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
                if (_intCachedRestrictedGear < 0)
                {
                    foreach (Improvement objImprovment in Improvements.Where(x => x.ImproveType == Improvement.ImprovementType.RestrictedGear && x.Enabled))
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
                if (_intCachedOverclocker < 0)
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
                if (_intCachedMadeMan < 0)
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
                if (_intCachedFame < 0)
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
                if (_intCachedErased < 0)
                    _intCachedErased =
                        Improvements.Any(x => x.ImproveType == Improvement.ImprovementType.Erased && x.Enabled) ? 1 : 0;

                return _intCachedErased > 0;
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
            string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
            string strInterval;
            // Find the character's Negotiation total.
            int intPool = SkillsSection.GetActiveSkill("Negotiation")?.Pool ?? 0;
            // Determine the interval based on the item's price.
            if (decCost <= 100.0m)
                strInterval = "6" + strSpaceCharacter +
                              LanguageManager.GetString("String_Hours", GlobalOptions.Language);
            else if (decCost <= 1000.0m)
                strInterval = "1" + strSpaceCharacter + LanguageManager.GetString("String_Day", GlobalOptions.Language);
            else if (decCost <= 10000.0m)
                strInterval = "2" + strSpaceCharacter +
                              LanguageManager.GetString("String_Days", GlobalOptions.Language);
            else if (decCost <= 100000.0m)
                strInterval = "1" + strSpaceCharacter +
                              LanguageManager.GetString("String_Week", GlobalOptions.Language);
            else
                strInterval = "1" + strSpaceCharacter +
                              LanguageManager.GetString("String_Month", GlobalOptions.Language);

            return intPool.ToString(GlobalOptions.CultureInfo) + strSpaceCharacter + '(' +
                   intAvailValue.ToString(GlobalOptions.CultureInfo) + ',' + strSpaceCharacter + strInterval + ')';
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
        public ICollection<Character> LinkedCharacters => _lstLinkedCharacters;

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

                        XmlNode objXmlQualityNode =
                            xmlRootQualitiesNode.SelectSingleNode(
                                "quality[name = \"" + GetQualityName(objXmlQuality.InnerText) + "\"]");

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
                    using (XmlNodeList xmlMetatypeQualityList =
                        objXmlMetatype.SelectNodes("qualities/positive/quality"))
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

                    // Negative Qualities.
                    using (XmlNodeList xmlMetatypeQualityList =
                        objXmlMetatype.SelectNodes("qualities/negative/quality"))
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

                    // Do it all over again for Metavariants.
                    if (!string.IsNullOrEmpty(_strMetavariant))
                    {
                        objXmlMetatype =
                            objXmlMetatype.SelectSingleNode("metavariants/metavariant[name = \"" + _strMetavariant +
                                                            "\"]");

                        if (objXmlMetatype != null)
                        {
                            // Positive Qualities.
                            using (XmlNodeList xmlMetatypeQualityList =
                                objXmlMetatype.SelectNodes("qualities/positive/quality"))
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

                            // Negative Qualities.
                            using (XmlNodeList xmlMetatypeQualityList =
                                objXmlMetatype.SelectNodes("qualities/negative/quality"))
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

            if (intRanks > 0)
            {
                for (int i = 0; i < intRanks; ++i)
                {
                    Quality objQuality = new Quality(this);
                    if (i == 0 && xmlOldQuality.TryGetField("guid", Guid.TryParse, out Guid guidOld))
                    {
                        ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.Quality,
                            guidOld.ToString());
                        objQuality.SetGUID(guidOld);
                    }

                    QualitySource objQualitySource =
                        Quality.ConvertToQualitySource(xmlOldQuality["qualitysource"]?.InnerText);
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
                if (_intInitPasses == int.MinValue)
                    _intInitPasses = Convert.ToInt32(InitiativeDice);
                return _intInitPasses;
            }
            set
            {
                if (_intInitPasses != value)
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
                    strReturn.Append(objXmlBook["translate"]?.InnerText ?? objXmlBook["name"]?.InnerText ??
                                     LanguageManager.GetString("String_Unknown", GlobalOptions.Language));
                    strReturn.Append($" ({objXmlBook["altcode"]?.InnerText ?? strBook})");
                }
                else
                {
                    strReturn.Append(
                        LanguageManager.GetString("String_Unknown", GlobalOptions.Language) + ' ' + strBook);
                }
            }

            return strReturn.ToString();
        }

        #endregion

        //Can't be at improvementmanager due reasons
        private readonly Lazy<Stack<string>> _pushtext = new Lazy<Stack<string>>();

        /// <summary>
        /// Push a value that will be used instad of dialog instead in next <selecttext />
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
                if (_objActiveCommlink != value)
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
                if (_objHomeNode != value)
                {
                    _objHomeNode = value;
                    OnPropertyChanged();
                }
            }
        }

        public SkillsSection SkillsSection { get; }

        public int RedlinerBonus
        {
            get
            {
                if (_intCachedRedlinerBonus == int.MinValue)
                    RefreshRedlinerImprovements();

                return _intCachedRedlinerBonus;
            }
        }

        public void RefreshRedlinerImprovements()
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
                _intCachedRedlinerBonus = 0;
                return;
            }

            //Calculate bonus from cyberlimbs
            int intCount = 0;
            foreach (Cyberware objCyberware in Cyberware)
            {
                intCount += objCyberware.GetCyberlimbCount("skull", "torso");
            }

            intCount = Math.Min(intCount / 2, 2);
            _intCachedRedlinerBonus = lstSeekerImprovements.Any(x => x.ImprovedName == "STR" || x.ImprovedName == "AGI")
                ? intCount
                : 0;

            for (int i = 0; i < lstSeekerAttributes.Count; ++i)
            {
                Improvement objImprove = lstSeekerImprovements.FirstOrDefault(x =>
                    x.SourceName == "SEEKER_" + lstSeekerAttributes[i] &&
                    x.Value == (lstSeekerAttributes[i] == "BOX" ? intCount * -3 : intCount));
                if (objImprove != null)
                {
                    lstSeekerAttributes.RemoveAt(i);
                    lstSeekerImprovements.Remove(objImprove);
                    --i;
                }
            }

            //Improvement manager defines the functions needed to manipulate improvements
            //When the locals (someday) gets moved to this class, this can be removed and use
            //the local
            if (lstSeekerImprovements.Count != 0 || lstSeekerAttributes.Count != 0)
            {
                // Remove which qualites have been removed or which values have changed
                ImprovementManager.RemoveImprovements(this, lstSeekerImprovements);

                // Add new improvements or old improvements with new values
                foreach (string strAttribute in lstSeekerAttributes)
                {
                    if (strAttribute == "BOX")
                    {
                        ImprovementManager.CreateImprovement(this, strAttribute, Improvement.ImprovementSource.Quality,
                            "SEEKER_BOX", Improvement.ImprovementType.PhysicalCM, Guid.NewGuid().ToString("D"),
                            intCount * -3);
                    }
                    else
                    {
                        ImprovementManager.CreateImprovement(this, strAttribute, Improvement.ImprovementSource.Quality,
                            "SEEKER_" + strAttribute, Improvement.ImprovementType.Attribute,
                            Guid.NewGuid().ToString("D"), intCount, 1, 0, 0, intCount);
                    }
                }

                ImprovementManager.Commit(this);
            }
        }

        public void RefreshEssenceLossImprovements()
        {
            // Don't hammer away with this method while this character is loading. Instead, it will be run once after everything has been loaded in.
            if (IsLoading)
                return;
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
                    Improvement.ImprovementSource eEssenceLossSource = Created
                        ? Improvement.ImprovementSource.EssenceLoss
                        : Improvement.ImprovementSource.EssenceLossChargen;
                    ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.EssenceLoss);
                    ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.EssenceLossChargen);
                    if (intMaxReduction != 0)
                    {
                        ImprovementManager.CreateImprovement(this, "RES", eEssenceLossSource, string.Empty,
                            Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, 0, -intMaxReduction);
                        ImprovementManager.CreateImprovement(this, "DEP", eEssenceLossSource, string.Empty,
                            Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, 0, -intMaxReduction);
                    }

                    if (intMagMaxReduction != 0)
                    {
                        ImprovementManager.CreateImprovement(this, "MAG", eEssenceLossSource, string.Empty,
                            Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, 0, -intMagMaxReduction);
                        ImprovementManager.CreateImprovement(this, "MAGAdept", eEssenceLossSource, string.Empty,
                            Improvement.ImprovementType.Attribute, string.Empty, 0, 1, 0, 0, -intMagMaxReduction);
                        // If this is a Mystic Adept using special Mystic Adept PP rules (i.e. no second MAG attribute), Mystic Adepts lose PPs even if they have fewer PPs than their MAG
                        if (UseMysticAdeptPPs)
                            ImprovementManager.CreateImprovement(this, string.Empty, eEssenceLossSource, string.Empty,
                                Improvement.ImprovementType.AdeptPowerPoints, string.Empty, -intMagMaxReduction);
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
                    foreach (Improvement objImprovement in Improvements)
                    {
                        if (objImprovement.ImproveSource == Improvement.ImprovementSource.EssenceLoss &&
                            objImprovement.ImproveType == Improvement.ImprovementType.Attribute &&
                            objImprovement.Enabled)
                        {
                            // Values get subtracted because negative modifier = positive reduction, positive modifier = negative reduction
                            // Augmented values also get factored in in case the character is switching off the option to treat essence loss as an augmented malus
                            switch (objImprovement.ImprovedName)
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
                    if (intMaxReduction > 0 || intMinReduction > 0 || intRESMaximumReduction != 0 ||
                        intDEPMaximumReduction != 0)
                    {
                        // This is the step where create mode attribute loss regarding attribute minimum loss gets factored out.
                        int intRESMinimumReduction;
                        int intDEPMinimumReduction;
                        // If only maxima would be reduced, use the attribute's current total value instead of its current maximum, as this makes sure minima will only get reduced if the maximum reduction would eat into the current value
                        if (Options.ESSLossReducesMaximumOnly)
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
                        if (intRESMinimumReductionDelta > 0)
                        {
                            // ... and adding minimum reducing-improvements wouldn't do anything, start burning karma.
                            if (intRESMinimumReduction >
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
                        if (intDEPMinimumReductionDelta > 0)
                        {
                            // ... and adding minimum reducing-improvements wouldn't do anything, start burning karma.
                            if (intDEPMinimumReduction >
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
                        if (intRESMinimumReduction != 0 || intRESMaximumReduction != 0)
                            ImprovementManager.CreateImprovement(this, "RES", Improvement.ImprovementSource.EssenceLoss,
                                string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1,
                                -intRESMinimumReduction, -intRESMaximumReduction);
                        if (intDEPMinimumReduction != 0 || intDEPMaximumReduction != 0)
                            ImprovementManager.CreateImprovement(this, "DEP", Improvement.ImprovementSource.EssenceLoss,
                                string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1,
                                -intDEPMinimumReduction, -intDEPMaximumReduction);
                    }

                    if (intMagMaxReduction > 0 || intMagMinReduction > 0 || intMAGMaximumReduction != 0 ||
                        intMAGAdeptMaximumReduction != 0)
                    {
                        // This is the step where create mode attribute loss regarding attribute minimum loss gets factored out.
                        int intMAGMinimumReduction;
                        int intMAGAdeptMinimumReduction;
                        // If only maxima would be reduced, use the attribute's current total value instead of its current maximum, as this makes sure minima will only get reduced if the maximum reduction would eat into the current value
                        if (Options.ESSLossReducesMaximumOnly)
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
                        if (intMAGMinimumReductionDelta > 0)
                        {
                            // ... and adding minimum reducing-improvements wouldn't do anything, start burning karma.
                            if (intMAGMinimumReduction >
                                MAG.Base + MAG.FreeBase + MAG.RawMinimum + MAG.AttributeValueModifiers)
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
                                intPPBurn = Math.Min(intMAGMinimumReductionDelta - intPPBurn,
                                    ImprovementManager.ValueOf(this, Improvement.ImprovementType.AdeptPowerPoints));
                                // Source needs to be EssenceLossChargen so that it doesn't get wiped in career mode.
                                if (intPPBurn != 0)
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
                        if (MAGAdept != MAG)
                        {
                            // If the new MAGAdept reduction is greater than the old one...
                            int intMAGAdeptMinimumReductionDelta =
                                intMAGAdeptMinimumReduction - intOldMAGAdeptCareerMinimumReduction;
                            if (intMAGAdeptMinimumReductionDelta > 0)
                            {
                                // ... and adding minimum reducing-improvements wouldn't do anything, start burning karma.
                                if (intMAGAdeptMinimumReduction > MAGAdept.Base + MAGAdept.FreeBase +
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
                        else if (intMAGAdeptMinimumReduction < intOldMAGAdeptCareerMinimumReduction)
                        {
                            intMAGAdeptMinimumReduction = intOldMAGAdeptCareerMinimumReduction;
                        }

                        // Create Improvements
                        if (intMAGMinimumReduction != 0 || intMAGMaximumReduction != 0)
                            ImprovementManager.CreateImprovement(this, "MAG", Improvement.ImprovementSource.EssenceLoss,
                                string.Empty, Improvement.ImprovementType.Attribute, string.Empty, 0, 1,
                                -intMAGMinimumReduction, -intMAGMaximumReduction);
                        if (intMAGAdeptMinimumReduction != 0 || intMAGAdeptMaximumReduction != 0)
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
                    if (Options.ESSLossReducesMaximumOnly)
                    {
                        intRESMinimumReduction = Math.Max(0, intMinReduction + RES.TotalValue - RES.TotalMaximum);
                        intDEPMinimumReduction = Math.Max(0, intMinReduction + DEP.TotalValue - DEP.TotalMaximum);
                        intMAGMinimumReduction = Math.Max(0, intMagMinReduction + MAG.TotalValue - MAG.TotalMaximum);
                        intMAGAdeptMinimumReduction = Math.Max(0,
                            intMagMinReduction + MAGAdept.TotalValue - MAGAdept.TotalMaximum);
                    }

                    ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.EssenceLoss);
                    ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.EssenceLossChargen);
                    if (intMaxReduction != 0 || intRESMinimumReduction != 0 || intDEPMinimumReduction != 0)
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

                    if (intMagMaxReduction != 0 || intMAGMinimumReduction != 0 || intMAGAdeptMinimumReduction != 0)
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
                        }
                    }

                    if (RESEnabled &&
                        (Options.SpecialKarmaCostBasedOnShownValue && intMaxReduction >= RES.TotalMaximum ||
                         !Options.SpecialKarmaCostBasedOnShownValue && RES.TotalMaximum < 1))
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
                ImprovementManager.RemoveImprovements(this,
                    Improvements.Where(x =>
                        x.ImproveSource == Improvement.ImprovementSource.Cyberzombie &&
                        x.ImproveType == Improvement.ImprovementType.Attribute).ToList());
                if (intESSModifier != 0)
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
            if (e.PropertyName == nameof(CharacterAttrib.TotalValue))
            {
                OnMultiplePropertyChanged(nameof(LimitPhysical),
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
                    nameof(SpellDefenseManipulationPhysical));
            }
            else if (e.PropertyName == nameof(CharacterAttrib.MetatypeMaximum))
            {
                if (DEPEnabled)
                    OnPropertyChanged(nameof(IsAI));
            }
        }

        public void RefreshAGIDependentProperties(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CharacterAttrib.TotalValue))
            {
                OnPropertyChanged(nameof(SpellDefenseDecreaseAGI));
            }
        }

        public void RefreshREADependentProperties(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CharacterAttrib.TotalValue))
            {
                OnMultiplePropertyChanged(nameof(LimitPhysical),
                    nameof(InitiativeValue),
                    nameof(SpellDefenseIndirectDodge),
                    nameof(SpellDefenseDecreaseREA));
            }
        }

        public void RefreshSTRDependentProperties(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CharacterAttrib.TotalValue))
            {
                // Encumbrance is only affected by STR.TotalValue when it comes to attributes
                RefreshEncumbrance();
                OnMultiplePropertyChanged(nameof(LimitPhysical),
                    nameof(LiftAndCarry),
                    nameof(SpellDefenseDecreaseSTR),
                    nameof(SpellDefenseManipulationPhysical));
            }
        }

        public void RefreshCHADependentProperties(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CharacterAttrib.TotalValue))
            {
                if (_objOptions.UseTotalValueForFreeContacts)
                    OnMultiplePropertyChanged(nameof(ContactPoints),
                        nameof(LimitSocial),
                        nameof(Composure),
                        nameof(JudgeIntentions),
                        nameof(JudgeIntentionsResist),
                        nameof(SpellDefenseDecreaseCHA));
                else
                    OnMultiplePropertyChanged(nameof(LimitSocial),
                        nameof(Composure),
                        nameof(JudgeIntentions),
                        nameof(JudgeIntentionsResist),
                        nameof(SpellDefenseDecreaseCHA));
            }
            else if (e.PropertyName == nameof(CharacterAttrib.Value))
            {
                if (!_objOptions.UseTotalValueForFreeContacts)
                    OnPropertyChanged(nameof(ContactPoints));
            }
        }

        public void RefreshINTDependentProperties(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CharacterAttrib.TotalValue))
            {
                OnMultiplePropertyChanged(nameof(LimitMental),
                    nameof(JudgeIntentions),
                    nameof(InitiativeValue),
                    nameof(AstralInitiativeValue),
                    nameof(MatrixInitiativeValue),
                    nameof(MatrixInitiativeColdValue),
                    nameof(MatrixInitiativeHotValue),
                    nameof(SpellDefenseIndirectDodge),
                    nameof(SpellDefenseDecreaseINT),
                    nameof(SpellDefenseIllusionPhysical));
            }
        }

        public void RefreshLOGDependentProperties(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CharacterAttrib.TotalValue))
            {
                OnMultiplePropertyChanged(nameof(LimitMental),
                    nameof(Memory),
                    nameof(PsychologicalAddictionResistFirstTime),
                    nameof(PsychologicalAddictionResistAlreadyAddicted),
                    nameof(SpellDefenseDetection),
                    nameof(SpellDefenseDecreaseLOG),
                    nameof(SpellDefenseIllusionMana),
                    nameof(SpellDefenseIllusionPhysical),
                    nameof(SpellDefenseManipulationMental));
            }
        }

        public void RefreshWILDependentProperties(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CharacterAttrib.TotalValue))
            {
                OnMultiplePropertyChanged(nameof(LimitSocial),
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
                    nameof(SpellDefenseManipulationMental));
            }
        }

        public void RefreshMAGDependentProperties(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CharacterAttrib.TotalValue))
            {
                if (!IsLoading && MysticAdeptPowerPoints > 0)
                {
                    int intMAGTotalValue = MAG.TotalValue;
                    if (MysticAdeptPowerPoints > intMAGTotalValue)
                        MysticAdeptPowerPoints = intMAGTotalValue;
                }

                HashSet<string> setPropertiesChanged = new HashSet<string>();
                if (Options.SpiritForceBasedOnTotalMAG)
                    setPropertiesChanged.Add(nameof(MaxSpiritForce));
                if (MysAdeptAllowPPCareer)
                    setPropertiesChanged.Add(nameof(CanAffordCareerPP));
                if (!UseMysticAdeptPPs && MAG == MAGAdept)
                    setPropertiesChanged.Add(nameof(PowerPointsTotal));

                OnMultiplePropertyChanged(setPropertiesChanged.ToArray());
            }
            else if (e.PropertyName == nameof(CharacterAttrib.Value))
            {
                if (!Options.SpiritForceBasedOnTotalMAG)
                    OnPropertyChanged(nameof(MaxSpiritForce));
            }
        }

        public void RefreshMAGAdeptDependentProperties(object sender, PropertyChangedEventArgs e)
        {
            if (MAG == MAGAdept)
                return;

            if (e.PropertyName == nameof(CharacterAttrib.TotalValue))
            {
                if (!UseMysticAdeptPPs)
                    OnPropertyChanged(nameof(PowerPointsTotal));
            }
        }

        public void RefreshRESDependentProperties(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CharacterAttrib.TotalValue))
                OnPropertyChanged(nameof(MaxSpriteLevel));
        }

        public void RefreshDEPDependentProperties(object sender, PropertyChangedEventArgs e)
        {
            if (IsAI && e.PropertyName == nameof(CharacterAttrib.TotalValue))
                EDG.OnPropertyChanged(nameof(CharacterAttrib.MetatypeMaximum));
        }

        public void RefreshESSDependentProperties(object sender, PropertyChangedEventArgs e)
        {
            // Only ESS.MetatypeMaximum is used for the Essence method/property when it comes to attributes
            if (e.PropertyName == nameof(CharacterAttrib.MetatypeMaximum))
            {
                OnPropertyChanged(nameof(Essence));
            }
        }

        public void RefreshEncumbrance()
        {
            // Don't hammer away with this method while this character is loading. Instead, it will be run once after everything has been loaded in.
            if (IsLoading)
                return;
            // Remove any Improvements from Armor Encumbrance.
            ImprovementManager.RemoveImprovements(this, Improvement.ImprovementSource.ArmorEncumbrance);
            if (!Options.NoArmorEncumbrance)
            {
                // Create the Armor Encumbrance Improvements.
                int intEncumbrance = ArmorEncumbrance;
                if (intEncumbrance != 0)
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
            if (IsLoading)
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
        public bool CanAffordCareerPP => MysAdeptAllowPPCareer && Karma >= _objOptions.KarmaMysticAdeptPowerPoint &&
                                         MAG.TotalValue > MysticAdeptPowerPoints;

        /// <summary>
        /// Blocked grades of cyber/bioware in Create mode.
        /// </summary>
        public HashSet<string> BannedWareGrades { get; } = new HashSet<string> {"Betaware", "Deltaware", "Gammaware"};

        public event PropertyChangedEventHandler PropertyChanged;

        private readonly DependancyGraph<string> CharacterDependencyGraph;

        [NotifyPropertyChangedInvocator]
        public void OnPropertyChanged([CallerMemberName] string strPropertyName = null)
        {
            OnMultiplePropertyChanged(strPropertyName);
        }

        public void OnMultiplePropertyChanged(params string[] lstPropertyNames)
        {
            ICollection<string> lstNamesOfChangedProperties = null;
            foreach (string strPropertyName in lstPropertyNames)
            {
                if (lstNamesOfChangedProperties == null)
                    lstNamesOfChangedProperties = CharacterDependencyGraph.GetWithAllDependants(strPropertyName);
                else
                {
                    foreach (string strLoopChangedProperty in CharacterDependencyGraph.GetWithAllDependants(
                        strPropertyName))
                        lstNamesOfChangedProperties.Add(strLoopChangedProperty);
                }
            }

            if ((lstNamesOfChangedProperties?.Count > 0) != true)
                return;

            if (lstNamesOfChangedProperties.Contains(nameof(CharacterGrammaticGender)))
            {
                _strCachedCharacterGrammaticGender = string.Empty;
            }

            if (lstNamesOfChangedProperties.Contains(nameof(ContactPoints)))
            {
                _intCachedContactPoints = int.MinValue;
            }

            if (lstNamesOfChangedProperties.Contains(nameof(TrustFund)))
            {
                _intCachedTrustFund = int.MinValue;
            }

            if (lstNamesOfChangedProperties.Contains(nameof(Ambidextrous)))
            {
                _intCachedAmbidextrous = int.MinValue;
            }

            if (lstNamesOfChangedProperties.Contains(nameof(RestrictedGear)))
            {
                _intCachedRestrictedGear = int.MinValue;
            }

            if (lstNamesOfChangedProperties.Contains(nameof(FriendsInHighPlaces)))
            {
                _intCachedFriendsInHighPlaces = int.MinValue;
            }

            if (lstNamesOfChangedProperties.Contains(nameof(ExCon)))
            {
                _intCachedExCon = int.MinValue;
            }

            if (lstNamesOfChangedProperties.Contains(nameof(MadeMan)))
            {
                _intCachedMadeMan = int.MinValue;
            }

            if (lstNamesOfChangedProperties.Contains(nameof(Fame)))
            {
                _intCachedFame = int.MinValue;
            }

            if (lstNamesOfChangedProperties.Contains(nameof(Erased)))
            {
                _intCachedErased = int.MinValue;
            }

            if (lstNamesOfChangedProperties.Contains(nameof(Overclocker)))
            {
                _intCachedOverclocker = int.MinValue;
            }

            if (lstNamesOfChangedProperties.Contains(nameof(Ambidextrous)))
            {
                _intCachedAmbidextrous = int.MinValue;
            }

            if (lstNamesOfChangedProperties.Contains(nameof(Ambidextrous)))
            {
                _intCachedAmbidextrous = int.MinValue;
            }

            if (lstNamesOfChangedProperties.Contains(nameof(BlackMarketDiscount)))
            {
                _intCachedBlackMarketDiscount = int.MinValue;
            }

            if (lstNamesOfChangedProperties.Contains(nameof(PowerPointsUsed)))
            {
                _decCachedPowerPointsUsed = decimal.MinValue;
            }

            if (lstNamesOfChangedProperties.Contains(nameof(CyberwareEssence)))
            {
                _decCachedCyberwareEssence = decimal.MinValue;
            }

            if (lstNamesOfChangedProperties.Contains(nameof(BiowareEssence)))
            {
                _decCachedPrototypeTranshumanEssenceUsed = decimal.MinValue;
                _decCachedBiowareEssence = decimal.MinValue;
            }

            if (lstNamesOfChangedProperties.Contains(nameof(EssenceHole)))
            {
                _decCachedEssenceHole = decimal.MinValue;
            }

            if (lstNamesOfChangedProperties.Contains(nameof(PrototypeTranshumanEssenceUsed)))
            {
                _decCachedBiowareEssence = decimal.MinValue;
                _decCachedPrototypeTranshumanEssenceUsed = decimal.MinValue;
            }

            if (lstNamesOfChangedProperties.Contains(nameof(CareerNuyen)))
            {
                _decCachedCareerNuyen = decimal.MinValue;
            }

            if (lstNamesOfChangedProperties.Contains(nameof(CareerKarma)))
            {
                _intCachedCareerKarma = int.MinValue;
            }

            if (lstNamesOfChangedProperties.Contains(nameof(InitiationEnabled)))
            {
                _intCachedInitiationEnabled = -1;
            }

            if (lstNamesOfChangedProperties.Contains(nameof(RedlinerBonus)))
            {
                _intCachedRedlinerBonus = int.MinValue;
                RefreshRedlinerImprovements();
            }

            if (lstNamesOfChangedProperties.Contains(nameof(Essence)))
            {
                ResetCachedEssence();
                RefreshEssenceLossImprovements();
            }

            if (lstNamesOfChangedProperties.Contains(nameof(WoundModifier)))
            {
                RefreshWoundPenalties();
            }

            if (!Created)
            {
                // If in create mode, update the Force for Spirits and Sprites (equal to Magician MAG Rating or RES Rating).
                if (lstNamesOfChangedProperties.Contains(nameof(MaxSpriteLevel)))
                {
                    foreach (Spirit objSpirit in Spirits)
                    {
                        if (objSpirit.EntityType != SpiritType.Spirit)
                            objSpirit.Force = MaxSpriteLevel;
                    }
                }

                if (lstNamesOfChangedProperties.Contains(nameof(MaxSpiritForce)))
                {
                    foreach (Spirit objSpirit in Spirits)
                    {
                        if (objSpirit.EntityType == SpiritType.Spirit)
                            objSpirit.Force = MaxSpiritForce;
                    }
                }
            }

            foreach (string strPropertyToChange in lstNamesOfChangedProperties)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strPropertyToChange));
            }

            foreach (Character objLoopOpenCharacter in Program.MainForm.OpenCharacters)
            {
                if (objLoopOpenCharacter != this && objLoopOpenCharacter.LinkedCharacters.Contains(this))
                {
                    foreach (Spirit objSpirit in objLoopOpenCharacter.Spirits)
                    {
                        if (objSpirit.LinkedCharacter == this)
                        {
                            objSpirit.OnPropertyChanged(nameof(Spirit.LinkedCharacter));
                        }
                    }

                    foreach (Contact objContact in objLoopOpenCharacter.Contacts)
                    {
                        if (objContact.LinkedCharacter == this)
                        {
                            objContact.OnPropertyChanged(nameof(Contact.LinkedCharacter));
                        }
                    }
                }
            }
        }
    }
}

