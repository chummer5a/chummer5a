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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Chummer.Backend.Equipment;
using Chummer.Classes;
using Chummer.Backend.Skills;
using Chummer.Backend.Attributes;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Xml.XPath;
using static Chummer.Backend.Skills.SkillsSection;
using Chummer.Backend.Uniques;
using NLog;

namespace Chummer
{
    [DebuggerDisplay("{" + nameof(DisplayDebug) + "()}")]
    public class Improvement: IHasNotes, IHasInternalId, ICanSort
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private string DisplayDebug()
        {
            return string.Format(GlobalOptions.InvariantCultureInfo, "{0} ({1}, {2}) 🡐 {3}, {4}, {5}",
                _objImprovementType, _intVal, _intRating, _objImprovementSource, _strSourceName, _strImprovedName);
        }

        public enum ImprovementType
        {
            None,
            Attribute,
            Text,
            Armor,
            FireArmor,
            ColdArmor,
            ElectricityArmor,
            AcidArmor,
            FallingArmor,
            Dodge,
            Reach,
            Nuyen,
            NuyenExpense,
            PhysicalCM,
            StunCM,
            UnarmedDV,
            InitiativeDice,
            MatrixInitiative,
            MatrixInitiativeDice,
            LifestyleCost,
            CMThreshold,
            EnhancedArticulation,
            WeaponCategoryDV,
            WeaponCategoryDice,
            WeaponSpecificDice,
            CyberwareEssCost,
            CyberwareTotalEssMultiplier,
            CyberwareEssCostNonRetroactive,
            CyberwareTotalEssMultiplierNonRetroactive,
            SpecialTab,
            Initiative,
            LivingPersonaDeviceRating,
            LivingPersonaProgramLimit,
            LivingPersonaAttack,
            LivingPersonaSleaze,
            LivingPersonaDataProcessing,
            LivingPersonaFirewall,
            LivingPersonaMatrixCM,
            Smartlink,
            BiowareEssCost,
            BiowareTotalEssMultiplier,
            BiowareEssCostNonRetroactive,
            BiowareTotalEssMultiplierNonRetroactive,
            GenetechCostMultiplier,
            BasicBiowareEssCost,
            SoftWeave,
            DisableBioware,
            DisableCyberware,
            DisableBiowareGrade,
            DisableCyberwareGrade,
            ConditionMonitor,
            UnarmedDVPhysical,
            Adapsin,
            FreePositiveQualities,
            FreeNegativeQualities,
            FreeKnowledgeSkills,
            NuyenMaxBP,
            CMOverflow,
            FreeSpiritPowerPoints,
            AdeptPowerPoints,
            ArmorEncumbrancePenalty,
            Initiation,
            Submersion,
            Art,
            Metamagic,
            Echo,
            Skillwire,
            DamageResistance,
            RestrictedItemCount,
            JudgeIntentions,
            JudgeIntentionsOffense,
            JudgeIntentionsDefense,
            LiftAndCarry,
            Memory,
            Concealability,
            SwapSkillAttribute,
            DrainResistance,
            FadingResistance,
            MatrixInitiativeDiceAdd,
            InitiativeDiceAdd,
            Composure,
            UnarmedAP,
            CMThresholdOffset,
            CMSharedThresholdOffset,
            Restricted,
            Notoriety,
            SpellCategory,
            SpellCategoryDamage,
            SpellCategoryDrain,
            SpellDescriptorDamage,
            SpellDescriptorDrain,
            SpellDicePool,
            ThrowRange,
            ThrowRangeSTR,
            SkillsoftAccess,
            AddSprite,
            BlackMarketDiscount,
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
            FriendsInHighPlaces,
            Erased,
            Fame,
            MadeMan,
            Overclocker,
            RestrictedGear,
            TrustFund,
            ExCon,
            ContactForceGroup,
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
            SkillLinkedAttribute, //linked attribute
            SkillLevel,  //Karma points in skill
            SkillGroupLevel, //group
            SkillBase,  //base points in skill
            SkillGroupBase, //group
            Skillsoft, // A knowledge or language skill gained from a knowsoft
            Activesoft, // An active skill gained from an activesoft
            ReplaceAttribute, //Alter the base metatype or metavariant of a character. Used for infected.
            SpecialSkills,
            ReflexRecorderOptimization,
            BlockSkillDefault,
            AllowSkillDefault,
            Ambidextrous,
            UnarmedReach,
            SkillSpecialization,
            SkillExpertise, // SASS' Inspired, adds a specialization that gives a +3 bonus instead of the usual +2
            SkillSpecializationOption,
            NativeLanguageLimit,
            AdeptPowerFreeLevels,
            AdeptPowerFreePoints,
            AIProgram,
            CritterPowerLevel,
            CritterPower,
            SwapSkillSpecAttribute,
            SpellResistance,
            AllowSpellCategory,
            LimitSpellCategory,
            AllowSpellRange,
            LimitSpellRange,
            BlockSpellDescriptor,
            LimitSpellDescriptor,
            LimitSpiritCategory,
            WalkSpeed,
            RunSpeed,
            SprintSpeed,
            WalkMultiplier,
            RunMultiplier,
            SprintBonus,
            WalkMultiplierPercent,
            RunMultiplierPercent,
            SprintBonusPercent,
            EssencePenalty,
            EssencePenaltyT100,
            EssencePenaltyMAGOnlyT100,
            FreeSpellsATT,
            FreeSpells,
            DrainValue,
            FadingValue,
            Spell,
            ComplexForm,
            Gear,
            Weapon,
            MentorSpirit,
            Paragon,
            FreeSpellsSkill,
            DisableSpecializationEffects, // Disable the effects of specializations for a skill
            FatigueResist,
            RadiationResist,
            SonicResist,
            ToxinContactResist,
            ToxinIngestionResist,
            ToxinInhalationResist,
            ToxinInjectionResist,
            PathogenContactResist,
            PathogenIngestionResist,
            PathogenInhalationResist,
            PathogenInjectionResist,
            ToxinContactImmune,
            ToxinIngestionImmune,
            ToxinInhalationImmune,
            ToxinInjectionImmune,
            PathogenContactImmune,
            PathogenIngestionImmune,
            PathogenInhalationImmune,
            PathogenInjectionImmune,
            PhysiologicalAddictionFirstTime,
            PsychologicalAddictionFirstTime,
            PhysiologicalAddictionAlreadyAddicted,
            PsychologicalAddictionAlreadyAddicted,
            StunCMRecovery,
            PhysicalCMRecovery,
            AddESStoStunCMRecovery,
            AddESStoPhysicalCMRecovery,
            MentalManipulationResist,
            PhysicalManipulationResist,
            ManaIllusionResist,
            PhysicalIllusionResist,
            DetectionSpellResist,
            DirectManaSpellResist,
            DirectPhysicalSpellResist,
            DecreaseBODResist,
            DecreaseAGIResist,
            DecreaseREAResist,
            DecreaseSTRResist,
            DecreaseCHAResist,
            DecreaseINTResist,
            DecreaseLOGResist,
            DecreaseWILResist,
            AddLimb,
            StreetCredMultiplier,
            StreetCred,
            AttributeKarmaCostMultiplier,
            AttributeKarmaCost,
            ActiveSkillKarmaCostMultiplier,
            SkillGroupKarmaCostMultiplier,
            KnowledgeSkillKarmaCostMultiplier,
            ActiveSkillKarmaCost,
            SkillGroupKarmaCost,
            SkillGroupDisable,
            SkillDisable,
            KnowledgeSkillKarmaCost,
            KnowledgeSkillKarmaCostMinimum,
            SkillCategorySpecializationKarmaCostMultiplier,
            SkillCategorySpecializationKarmaCost,
            SkillCategoryKarmaCostMultiplier,
            SkillCategoryKarmaCost,
            SkillGroupCategoryKarmaCostMultiplier,
            SkillGroupCategoryDisable,
            SkillGroupCategoryKarmaCost,
            AttributePointCostMultiplier,
            AttributePointCost,
            ActiveSkillPointCostMultiplier,
            SkillGroupPointCostMultiplier,
            KnowledgeSkillPointCostMultiplier,
            ActiveSkillPointCost,
            SkillGroupPointCost,
            KnowledgeSkillPointCost,
            SkillCategoryPointCostMultiplier,
            SkillCategoryPointCost,
            SkillGroupCategoryPointCostMultiplier,
            SkillGroupCategoryPointCost,
            NewSpellKarmaCostMultiplier,
            NewSpellKarmaCost,
            NewComplexFormKarmaCostMultiplier,
            NewComplexFormKarmaCost,
            NewAIProgramKarmaCostMultiplier,
            NewAIProgramKarmaCost,
            NewAIAdvancedProgramKarmaCostMultiplier,
            NewAIAdvancedProgramKarmaCost,
            BlockSkillSpecializations,
            BlockSkillCategorySpecializations,
            FocusBindingKarmaCost,
            FocusBindingKarmaMultiplier,
            MagiciansWayDiscount,
            BurnoutsWay,
            ContactForcedLoyalty,
            ContactMakeFree,
            FreeWare,
            WeaponAccuracy,
            WeaponSkillAccuracy,
            MetageneticLimit,
            Tradition,
            ActionDicePool,
            SpecialModificationLimit,
            AddSpirit,
            ContactKarmaDiscount,
            ContactKarmaMinimum,
            GenetechEssMultiplier,
            AllowSpriteFettering,
            DisableDrugGrade,
            DrugDuration,
            DrugDurationMultiplier,
            Surprise,
            EnableCyberzombie,
            AllowCritterPowerCategory,
            LimitCritterPowerCategory,
            AttributeMaxClamp,
            MetamagicLimit,
            DisableQuality,
            FreeQuality,
            AstralReputation,
            AstralReputationWild,
            NumImprovementTypes // 🡐 This one should always be the last defined enum
        }

        public enum ImprovementSource
        {
            Quality,
            Power,
            Metatype,
            Cyberware,
            Metavariant,
            Bioware,
            ArmorEncumbrance,
            Gear,
            VehicleMod,
            Spell,
            Initiation,
            Submersion,
            Metamagic,
            Echo,
            Armor,
            ArmorMod,
            EssenceLoss,
            EssenceLossChargen,
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
            Heritage,
            MartialArt,
            MartialArtTechnique,
            AIProgram,
            SpiritFettering,
            MentorSpirit,
            Drug,
            Tradition,
            Weapon,
            WeaponAccessory,
            AstralReputation,
            NumImprovementSources // 🡐 This one should always be the last defined enum
            ,
        }

        private readonly Character _objCharacter;
        private string _strImprovedName = string.Empty;
        private string _strSourceName = string.Empty;
        private int _intMin;
        private int _intMax;
        private int _intAug;
        private int _intAugMax;
        private int _intVal;
        private int _intRating = 1;
        private string _strExclude = string.Empty;
        private string _strCondition = string.Empty;
        private string _strUniqueName = string.Empty;
        private string _strTarget = string.Empty;
        private ImprovementType _objImprovementType;
        private ImprovementSource _objImprovementSource;
        private bool _blnCustom;
        private string _strCustomName = string.Empty;
        private string _strCustomId = string.Empty;
        private string _strCustomGroup = string.Empty;
        private string _strNotes = string.Empty;
        private bool _blnAddToRating;
        private bool _blnEnabled = true;
        private int _intOrder;

        #region Helper Methods

        /// <summary>
        /// Convert a string to an ImprovementType.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        public static ImprovementType ConvertToImprovementType(string strValue)
        {
            if (string.IsNullOrEmpty(strValue))
                return ImprovementType.None;
            if (strValue.Contains("InitiativePass"))
            {
                strValue = strValue.Replace("InitiativePass","InitiativeDice");
            }
            if (strValue == "ContactForceLoyalty")
                strValue = "ContactForcedLoyalty";
            return (ImprovementType) Enum.Parse(typeof (ImprovementType), strValue);
        }

        /// <summary>
        /// Convert a string to an ImprovementSource.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        public static ImprovementSource ConvertToImprovementSource(string strValue)
        {
            if (strValue == "MartialArtAdvantage")
                strValue = "MartialArtTechnique";
            return (ImprovementSource) Enum.Parse(typeof (ImprovementSource), strValue);
        }
        #endregion

        #region Save and Load Methods
        public Improvement(Character objCharacter)
        {
            _objCharacter = objCharacter;
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            if (objWriter == null)
                return;
            Log.Trace("Save enter");

            objWriter.WriteStartElement("improvement");
            if (!string.IsNullOrEmpty(_strUniqueName))
                objWriter.WriteElementString("unique", _strUniqueName);
            objWriter.WriteElementString("target", _strTarget);
            objWriter.WriteElementString("improvedname", _strImprovedName);
            objWriter.WriteElementString("sourcename", _strSourceName);
            objWriter.WriteElementString("min", _intMin.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("max", _intMax.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("aug", _intAug.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("augmax", _intAugMax.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("val", _intVal.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("rating", _intRating.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("exclude", _strExclude);
            objWriter.WriteElementString("condition", _strCondition);
            objWriter.WriteElementString("improvementttype", _objImprovementType.ToString());
            objWriter.WriteElementString("improvementsource", _objImprovementSource.ToString());
            objWriter.WriteElementString("custom", _blnCustom.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("customname", _strCustomName);
            objWriter.WriteElementString("customid", _strCustomId);
            objWriter.WriteElementString("customgroup", _strCustomGroup);
            objWriter.WriteElementString("addtorating", _blnAddToRating.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("enabled", _blnEnabled.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("order", _intOrder.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteEndElement();

            Log.Trace("Save end");
        }

        /// <summary>
        /// Load the CharacterAttribute from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            if (objNode == null)
                return;
            Log.Trace("Load enter");

            objNode.TryGetStringFieldQuickly("unique", ref _strUniqueName);
            objNode.TryGetStringFieldQuickly("target", ref _strTarget);
            objNode.TryGetStringFieldQuickly("improvedname", ref _strImprovedName);
            objNode.TryGetStringFieldQuickly("sourcename", ref _strSourceName);
            objNode.TryGetInt32FieldQuickly("min", ref _intMin);
            objNode.TryGetInt32FieldQuickly("max", ref _intMax);
            objNode.TryGetInt32FieldQuickly("aug", ref _intAug);
            objNode.TryGetInt32FieldQuickly("augmax", ref _intAugMax);
            objNode.TryGetInt32FieldQuickly("val", ref _intVal);
            objNode.TryGetInt32FieldQuickly("rating", ref _intRating);
            objNode.TryGetStringFieldQuickly("exclude", ref _strExclude);
            objNode.TryGetStringFieldQuickly("condition", ref _strCondition);
            if (objNode["improvementttype"] != null)
                _objImprovementType = ConvertToImprovementType(objNode["improvementttype"].InnerText);
            if (objNode["improvementsource"] != null)
                _objImprovementSource = ConvertToImprovementSource(objNode["improvementsource"].InnerText);
            // Legacy shim
            if (_objImprovementType == ImprovementType.LimitModifier && string.IsNullOrEmpty(_strCondition) && !string.IsNullOrEmpty(_strExclude))
            {
                _strCondition = _strExclude;
                _strExclude = string.Empty;
            }
            if (_objImprovementType == ImprovementType.RestrictedGear && _intVal == 0)
            {
                _intVal = 24;
            }
            objNode.TryGetBoolFieldQuickly("custom", ref _blnCustom);
            objNode.TryGetStringFieldQuickly("customname", ref _strCustomName);
            objNode.TryGetStringFieldQuickly("customid", ref _strCustomId);
            objNode.TryGetStringFieldQuickly("customgroup", ref _strCustomGroup);
            objNode.TryGetBoolFieldQuickly("addtorating", ref _blnAddToRating);
            objNode.TryGetBoolFieldQuickly("enabled", ref _blnEnabled);
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            objNode.TryGetInt32FieldQuickly("order", ref _intOrder);

            Log.Trace("Load exit");
        }

        #endregion

        #region Properties

        /// <summary>
        /// Whether or not this is a custom-made (manually created) Improvement.
        /// </summary>
        public bool Custom
        {
            get => _blnCustom;
            set => _blnCustom = value;
        }

        /// <summary>
        /// User-entered name for the custom Improvement.
        /// </summary>
        public string CustomName
        {
            get => _strCustomName;
            set => _strCustomName = value;
        }

        /// <summary>
        /// ID from the Improvements file. Only used for custom-made (manually created) Improvements.
        /// </summary>
        public string CustomId
        {
            get => _strCustomId;
            set => _strCustomId = value;
        }

        /// <summary>
        /// Group name for the Custom Improvement.
        /// </summary>
        public string CustomGroup
        {
            get => _strCustomGroup;
            set => _strCustomGroup = value;
        }

        /// <summary>
        /// User-entered notes for the custom Improvement.
        /// </summary>
        public string Notes
        {
            get => _strNotes;
            set => _strNotes = value;
        }

        /// <summary>
        /// Name of the Skill or CharacterAttribute that the Improvement is improving.
        /// </summary>
        public string ImprovedName
        {
            get => _strImprovedName;
            set
            {
                if (_strImprovedName != value)
                {
                    if (Enabled)
                    {
                        ImprovementManager.ClearCachedValue(_objCharacter, ImproveType, _strImprovedName);
                        ImprovementManager.ClearCachedValue(_objCharacter, ImproveType, value);
                    }

                    _strImprovedName = value;
                }
            }
        }

        /// <summary>
        /// Name of the source that granted this Improvement.
        /// </summary>
        public string SourceName
        {
            get => _strSourceName;
            set => _strSourceName = value;
        }

        /// <summary>
        /// The type of Object that the Improvement is improving.
        /// </summary>
        public ImprovementType ImproveType
        {
            get => _objImprovementType;
            set
            {
                if (_objImprovementType != value)
                {
                    if (Enabled)
                    {
                        ImprovementManager.ClearCachedValue(_objCharacter, _objImprovementType, ImprovedName);
                        ImprovementManager.ClearCachedValue(_objCharacter, value, ImprovedName);
                    }

                    _objImprovementType = value;
                }
            }
        }

        /// <summary>
        /// The type of Object that granted this Improvement.
        /// </summary>
        public ImprovementSource ImproveSource
        {
            get => _objImprovementSource;
            set
            {
                if (_objImprovementSource != value)
                {
                    _objImprovementSource = value;
                    if (Enabled)
                        ImprovementManager.ClearCachedValue(_objCharacter, ImprovementType.MatrixInitiativeDice, ImprovedName);
                }
            }
        }

        /// <summary>
        /// Minimum value modifier.
        /// </summary>
        public int Minimum
        {
            get => _intMin;
            set => _intMin = value;
        }

        /// <summary>
        /// Maximum value modifier.
        /// </summary>
        public int Maximum
        {
            get => _intMax;
            set => _intMax = value;
        }

        /// <summary>
        /// Augmented Maximum value modifier.
        /// </summary>
        public int AugmentedMaximum
        {
            get => _intAugMax;
            set => _intAugMax = value;
        }

        /// <summary>
        /// Augmented score modifier.
        /// </summary>
        public int Augmented
        {
            get => _intAug;
            set
            {
                if (_intAug != value)
                {
                    _intAug = value;
                    if (Enabled)
                        ImprovementManager.ClearCachedValue(_objCharacter, ImproveType, ImprovedName);
                }
            }
        }

        /// <summary>
        /// Value modifier.
        /// </summary>
        public int Value
        {
            get => _intVal;
            set
            {
                if (_intVal != value)
                {
                    _intVal = value;
                    if (Enabled)
                        ImprovementManager.ClearCachedValue(_objCharacter, ImproveType, ImprovedName);
                }
            }
        }

        /// <summary>
        /// The Rating value for the Improvement. This is 1 by default.
        /// </summary>
        public int Rating
        {
            get => _intRating;
            set
            {
                if (_intRating != value)
                {
                    _intRating = value;
                    if (Enabled)
                        ImprovementManager.ClearCachedValue(_objCharacter, ImproveType, ImprovedName);
                }
            }
        }

        /// <summary>
        /// A list of child items that should not receive the Improvement's benefit (typically for excluding a Skill from a Skill Group bonus).
        /// </summary>
        public string Exclude
        {
            get => _strExclude;
            set => _strExclude = value;
        }

        /// <summary>
        /// String containing the condition for when the bonus applies (e.g. a dicepool bonus to a skill that only applies to certain types of tests).
        /// </summary>
        public string Condition
        {
            get => _strCondition;
            set => _strCondition = value;
        }

        /// <summary>
        /// A Unique name for the Improvement. Only the highest value of any one Improvement that is part of this Unique Name group will be applied.
        /// </summary>
        public string UniqueName
        {
            get => _strUniqueName;
            set
            {
                if (_strUniqueName != value)
                {
                    _strUniqueName = value;
                    if (Enabled)
                        ImprovementManager.ClearCachedValue(_objCharacter, ImproveType, ImprovedName);
                }
            }
        }

        /// <summary>
        /// Whether or not the bonus applies directly to a Skill's Rating
        /// </summary>
        public bool AddToRating
        {
            get => _blnAddToRating;
            set
            {
                if (_blnAddToRating != value)
                {
                    _blnAddToRating = value;
                    if (Enabled)
                        ImprovementManager.ClearCachedValue(_objCharacter, ImproveType, ImprovedName);
                }
            }
        }

        /// <summary>
        /// The target of an improvement, e.g. the skill whose attributes should be swapped
        /// </summary>
        public string Target
        {
            get => _strTarget;
            set => _strTarget = value;
        }

        /// <summary>
        /// Whether or not the Improvement is enabled and provided its bonus.
        /// </summary>
        public bool Enabled
        {
            get => _blnEnabled;
            set
            {
                if (_blnEnabled != value)
                {
                    _blnEnabled = value;
                    ImprovementManager.ClearCachedValue(_objCharacter, ImproveType, ImprovedName);
                }
            }
        }

        /// <summary>
        /// Sort order for Custom Improvements.
        /// </summary>
        public int SortOrder
        {
            get => _intOrder;
            set => _intOrder = value;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Get an enumerable of events to fire related to this specific improvement.
        /// TODO: Merge parts or all of this function with ImprovementManager methods that enable, disable, add, or remove improvements.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Tuple<INotifyMultiplePropertyChanged, string>> GetRelevantPropertyChangers()
        {
            switch (ImproveType)
            {
                case ImprovementType.Attribute:
                {
                    string strTargetAttribute = ImprovedName;
                    HashSet<string> setAttributePropertiesChanged = new HashSet<string>();
                    if (AugmentedMaximum != 0)
                        setAttributePropertiesChanged.Add(nameof(CharacterAttrib.AugmentedMaximumModifiers));
                    if (Maximum != 0)
                        setAttributePropertiesChanged.Add(nameof(CharacterAttrib.MaximumModifiers));
                    if (Minimum != 0)
                        setAttributePropertiesChanged.Add(nameof(CharacterAttrib.MinimumModifiers));
                    if (strTargetAttribute.EndsWith("Base", StringComparison.Ordinal))
                    {
                        strTargetAttribute = strTargetAttribute.TrimEndOnce("Base", true);
                        if (Augmented != 0)
                            setAttributePropertiesChanged.Add(nameof(CharacterAttrib.AttributeValueModifiers));
                    }
                    else
                    {
                        if (Augmented != 0)
                            setAttributePropertiesChanged.Add(nameof(CharacterAttrib.AttributeModifiers));
                    }

                    if (setAttributePropertiesChanged.Count > 0)
                    {
                        foreach (CharacterAttrib objCharacterAttrib in _objCharacter.AttributeSection.AttributeList
                            .Concat(_objCharacter.AttributeSection.SpecialAttributeList))
                        {
                            if (objCharacterAttrib.Abbrev != strTargetAttribute)
                                continue;
                            foreach (string strPropertyName in setAttributePropertiesChanged)
                            {
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objCharacterAttrib,
                                    strPropertyName);
                            }
                        }
                    }
                }
                    break;
                case ImprovementType.Armor:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.TotalArmorRating));
                }
                    break;
                case ImprovementType.FireArmor:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.TotalFireArmorRating));
                }
                    break;
                case ImprovementType.ColdArmor:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.TotalColdArmorRating));
                }
                    break;
                case ImprovementType.ElectricityArmor:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.TotalElectricityArmorRating));
                }
                    break;
                case ImprovementType.AcidArmor:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.TotalAcidArmorRating));
                }
                    break;
                case ImprovementType.FallingArmor:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.TotalFallingArmorRating));
                }
                    break;
                case ImprovementType.Dodge:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.TotalBonusDodgeRating));
                }
                    break;
                case ImprovementType.Reach:
                    break;
                case ImprovementType.Nuyen:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.StartingNuyenModifiers));
                }
                    break;
                case ImprovementType.PhysicalCM:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.PhysicalCM));
                }
                    break;
                case ImprovementType.StunCM:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.StunCM));
                }
                    break;
                case ImprovementType.UnarmedDV:
                    break;
                case ImprovementType.InitiativeDiceAdd:
                case ImprovementType.InitiativeDice:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.InitiativeDice));
                }
                    break;
                case ImprovementType.MatrixInitiative:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.MatrixInitiativeValue));
                }
                    break;
                case ImprovementType.MatrixInitiativeDiceAdd:
                case ImprovementType.MatrixInitiativeDice:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.MatrixInitiativeDice));
                }
                    break;
                case ImprovementType.LifestyleCost:
                    break;
                case ImprovementType.CMThreshold:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.CMThreshold));
                }
                    break;
                case ImprovementType.IgnoreCMPenaltyPhysical:
                case ImprovementType.IgnoreCMPenaltyStun:
                case ImprovementType.CMThresholdOffset:
                case ImprovementType.CMSharedThresholdOffset:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.CMThresholdOffsets));
                }
                    break;
                case ImprovementType.EnhancedArticulation:
                    break;
                case ImprovementType.WeaponCategoryDV:
                    break;
                case ImprovementType.WeaponCategoryDice:
                    break;
                case ImprovementType.CyberwareEssCostNonRetroactive:
                    break;
                case ImprovementType.CyberwareTotalEssMultiplierNonRetroactive:
                    break;
                case ImprovementType.SpecialTab:
                    break;
                case ImprovementType.Initiative:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.InitiativeValue));
                }
                    break;
                case ImprovementType.LivingPersonaDeviceRating:
                    break;
                case ImprovementType.LivingPersonaProgramLimit:
                    break;
                case ImprovementType.LivingPersonaAttack:
                    break;
                case ImprovementType.LivingPersonaSleaze:
                    break;
                case ImprovementType.LivingPersonaDataProcessing:
                    break;
                case ImprovementType.LivingPersonaFirewall:
                    break;
                case ImprovementType.LivingPersonaMatrixCM:
                    break;
                case ImprovementType.Smartlink:
                    break;
                case ImprovementType.BiowareEssCostNonRetroactive:
                    break;
                case ImprovementType.BiowareTotalEssMultiplierNonRetroactive:
                    break;
                case ImprovementType.GenetechCostMultiplier:
                    break;
                case ImprovementType.SoftWeave:
                    break;
                case ImprovementType.DisableBioware:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.AddBiowareEnabled));
                }
                    break;
                case ImprovementType.DisableCyberware:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.AddCyberwareEnabled));
                }
                    break;
                case ImprovementType.DisableBiowareGrade:
                    break;
                case ImprovementType.DisableCyberwareGrade:
                    break;
                case ImprovementType.ConditionMonitor:
                    break;
                case ImprovementType.UnarmedDVPhysical:
                    break;
                case ImprovementType.Adapsin:
                    break;
                case ImprovementType.FreePositiveQualities:
                    break;
                case ImprovementType.FreeNegativeQualities:
                    break;
                case ImprovementType.FreeKnowledgeSkills:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter.SkillsSection,
                        nameof(SkillsSection.KnowledgeSkillPoints));
                }
                    break;
                case ImprovementType.NuyenMaxBP:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.TotalNuyenMaximumBP));
                }
                    break;
                case ImprovementType.CMOverflow:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.CMOverflow));
                }
                    break;
                case ImprovementType.FreeSpiritPowerPoints:
                    break;
                case ImprovementType.AdeptPowerPoints:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.PowerPointsTotal));
                }
                    break;
                case ImprovementType.ArmorEncumbrancePenalty:
                    break;
                case ImprovementType.Initiation:
                    break;
                case ImprovementType.Submersion:
                    break;
                case ImprovementType.Art:
                    break;
                case ImprovementType.Metamagic:
                    break;
                case ImprovementType.Echo:
                    break;
                case ImprovementType.Skillwire:
                {
                    foreach (Skill objSkill in _objCharacter.SkillsSection.Skills)
                    {
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(objSkill,
                            nameof(Skill.CyberwareRating));
                    }
                }
                    break;
                case ImprovementType.DamageResistance:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.DamageResistancePool));
                }
                    break;
                case ImprovementType.RestrictedItemCount:
                    break;
                case ImprovementType.JudgeIntentions:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.JudgeIntentions));
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.JudgeIntentionsResist));
                }
                    break;
                case ImprovementType.JudgeIntentionsOffense:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.JudgeIntentions));
                }
                    break;
                case ImprovementType.JudgeIntentionsDefense:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.JudgeIntentionsResist));
                }
                    break;
                case ImprovementType.LiftAndCarry:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.LiftAndCarry));
                }
                    break;
                case ImprovementType.Memory:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.Memory));
                }
                    break;
                case ImprovementType.Concealability:
                    break;
                case ImprovementType.SwapSkillAttribute:
                case ImprovementType.SwapSkillSpecAttribute:
                {
                    Skill objTargetSkill =
                        _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.Name == ImprovedName) ??
                        (_objCharacter.SkillsSection.Skills.OfType<ExoticSkill>()
                             .FirstOrDefault(x => x.Name + " (" + x.Specific + ')' == ImprovedName) ??
                         (Skill) _objCharacter.SkillsSection.KnowledgeSkills.FirstOrDefault(x =>
                             x.Name == ImprovedName || x.CurrentDisplayName == ImprovedName));
                    if (objTargetSkill != null)
                    {
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                            nameof(Skill.PoolToolTip));
                    }
                }
                    break;
                case ImprovementType.DrainResistance:
                case ImprovementType.FadingResistance:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter.MagicTradition,
                        nameof(Tradition.DrainValue));
                }
                    break;
                case ImprovementType.Composure:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.Composure));
                }
                    break;
                case ImprovementType.UnarmedAP:
                    break;
                case ImprovementType.Restricted:
                    break;
                case ImprovementType.Notoriety:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.CalculatedNotoriety));
                }
                    break;
                case ImprovementType.SpellCategory:
                    break;
                case ImprovementType.SpellCategoryDamage:
                    break;
                case ImprovementType.SpellCategoryDrain:
                    break;
                case ImprovementType.ThrowRange:
                    break;
                case ImprovementType.SkillsoftAccess:
                {
                    foreach (Skill objSkill in _objCharacter.SkillsSection.Skills.Concat(_objCharacter.SkillsSection
                        .KnowledgeSkills))
                    {
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(objSkill,
                            nameof(Skill.CyberwareRating));
                    }
                }
                    break;
                case ImprovementType.AddSprite:
                    break;
                case ImprovementType.BlackMarketDiscount:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.BlackMarketDiscount));
                }
                    break;
                case ImprovementType.ComplexFormLimit:
                    break;
                case ImprovementType.SpellLimit:
                    break;
                case ImprovementType.QuickeningMetamagic:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.QuickeningEnabled));
                }
                    break;
                case ImprovementType.BasicLifestyleCost:
                    break;
                case ImprovementType.ThrowSTR:
                    break;
                case ImprovementType.EssenceMax:
                {
                    foreach (CharacterAttrib objCharacterAttrib in _objCharacter.AttributeSection.AttributeList.Concat(
                        _objCharacter.AttributeSection.SpecialAttributeList))
                    {
                        if (objCharacterAttrib.Abbrev == "ESS")
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objCharacterAttrib,
                                nameof(CharacterAttrib.MetatypeMaximum));
                        }
                    }
                }
                    break;
                case ImprovementType.AdeptPower:
                    break;
                case ImprovementType.SpecificQuality:
                    break;
                case ImprovementType.MartialArt:
                    break;
                case ImprovementType.LimitModifier:
                    break;
                case ImprovementType.PhysicalLimit:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.LimitPhysical));
                }
                    break;
                case ImprovementType.MentalLimit:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.LimitMental));
                }
                    break;
                case ImprovementType.SocialLimit:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.LimitSocial));
                }
                    break;
                case ImprovementType.FriendsInHighPlaces:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.FriendsInHighPlaces));
                }
                    break;
                case ImprovementType.Erased:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.Erased));
                }
                    break;
                case ImprovementType.Fame:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.Fame));
                }
                    break;
                case ImprovementType.MadeMan:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.MadeMan));
                }
                    break;
                case ImprovementType.Overclocker:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.Overclocker));
                }
                    break;
                case ImprovementType.RestrictedGear:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.RestrictedGear));
                }
                    break;
                case ImprovementType.TrustFund:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.TrustFund));
                }
                    break;
                case ImprovementType.ExCon:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.ExCon));
                }
                    break;
                case ImprovementType.ContactForceGroup:
                {
                    Contact objTargetContact = _objCharacter.Contacts.FirstOrDefault(x => x.UniqueId == ImprovedName);
                    if (objTargetContact != null)
                    {
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetContact,
                            nameof(Contact.GroupEnabled));
                    }
                }
                    break;
                case ImprovementType.Attributelevel:
                {
                    string strTargetAttribute = ImprovedName;
                    foreach (CharacterAttrib objCharacterAttrib in _objCharacter.AttributeSection.AttributeList.Concat(
                        _objCharacter.AttributeSection.SpecialAttributeList))
                    {
                        if (objCharacterAttrib.Abbrev == strTargetAttribute)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objCharacterAttrib,
                                nameof(CharacterAttrib.FreeBase));
                        }
                    }
                }
                    break;
                case ImprovementType.AddContact:
                    break;
                case ImprovementType.Seeker:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.RedlinerBonus));
                }
                    break;
                case ImprovementType.PublicAwareness:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.CalculatedPublicAwareness));
                }
                    break;
                case ImprovementType.PrototypeTranshuman:
                    break;
                case ImprovementType.Hardwire:
                {
                    Skill objTargetSkill =
                        _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.Name == ImprovedName) ??
                        (_objCharacter.SkillsSection.Skills.OfType<ExoticSkill>()
                             .FirstOrDefault(x => x.Name + " (" + x.Specific + ')' == ImprovedName) ??
                         (Skill) _objCharacter.SkillsSection.KnowledgeSkills.FirstOrDefault(x =>
                             x.InternalId == ImprovedName ||
                             x.CurrentDisplayName == ImprovedName));
                    if (objTargetSkill != null)
                    {
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                            nameof(Skill.CyberwareRating));
                    }
                }
                    break;
                case ImprovementType.DealerConnection:
                    break;
                case ImprovementType.AllowSkillDefault:
                {
                    if (string.IsNullOrEmpty(ImprovedName))
                    {
                        // Kludgiest of kludges, but it fits spec and Sapience isn't exactly getting turned off and on constantly.
                        foreach (Skill objSkill in _objCharacter.SkillsSection.Skills)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objSkill,
                                nameof(Skill.Enabled));
                        }

                        foreach (KnowledgeSkill objSkill in _objCharacter.SkillsSection.KnowledgeSkills)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objSkill,
                                nameof(Skill.Enabled));
                        }
                    }
                    else
                    {
                        Skill objTargetSkill =
                            _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.Name == ImprovedName) ??
                            _objCharacter.SkillsSection.KnowledgeSkills.FirstOrDefault(x => x.Name == ImprovedName);
                        if (objTargetSkill != null)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                nameof(Skill.Enabled));
                        }
                    }
                }
                    break;
                case ImprovementType.Skill:
                {
                    Skill objTargetSkill =
                        _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.Name == ImprovedName) ??
                        _objCharacter.SkillsSection.Skills.OfType<ExoticSkill>()
                            .FirstOrDefault(x => x.Name + " (" + x.Specific + ')' == ImprovedName) ??
                        _objCharacter.SkillsSection.KnowledgeSkills.FirstOrDefault(x =>
                            x.Name == ImprovedName) as Skill;
                    if (objTargetSkill != null)
                    {
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                            nameof(Skill.Base));
                    }
                }
                    break;
                case ImprovementType.SkillGroup:
                case ImprovementType.BlockSkillDefault:
                {
                    foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills.Where(x =>
                        x.SkillGroup == ImprovedName))
                    {
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                            nameof(Skill.DisplayPool));
                    }
                }
                    break;
                case ImprovementType.SkillCategory:
                {
                    foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills
                        .Concat(_objCharacter.SkillsSection.KnowledgeSkills)
                        .Where(x => x.SkillCategory == ImprovedName))
                    {
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                            nameof(Skill.DisplayPool));
                    }
                }
                    break;
                case ImprovementType.SkillLinkedAttribute:
                {
                    foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills
                        .Concat(_objCharacter.SkillsSection.KnowledgeSkills)
                        .Where(x => x.Attribute == ImprovedName))
                    {
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                            nameof(Skill.DisplayPool));
                    }
                }
                    break;
                case ImprovementType.SkillLevel:
                {
                    Skill objTargetSkill =
                        _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.Name == ImprovedName) ?? _objCharacter
                            .SkillsSection.Skills.OfType<ExoticSkill>()
                            .FirstOrDefault(x => x.Name + " (" + x.Specific + ')' == ImprovedName);
                    if (objTargetSkill != null)
                    {
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                            nameof(Skill.FreeKarma));
                    }
                }
                    break;
                case ImprovementType.SkillGroupLevel:
                {
                    SkillGroup objTargetGroup =
                        _objCharacter.SkillsSection.SkillGroups.FirstOrDefault(x => x.Name == ImprovedName);
                    if (objTargetGroup != null)
                    {
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetGroup,
                            nameof(SkillGroup.FreeLevels));
                    }
                }
                    break;
                case ImprovementType.SkillBase:
                {
                    Skill objTargetSkill =
                        _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.Name == ImprovedName) ?? _objCharacter
                            .SkillsSection.Skills.OfType<ExoticSkill>()
                            .FirstOrDefault(x => x.Name + " (" + x.Specific + ')' == ImprovedName);
                    if (objTargetSkill != null)
                    {
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                            nameof(Skill.FreeBase));
                    }
                }
                    break;
                case ImprovementType.SkillGroupBase:
                {
                    SkillGroup objTargetGroup =
                        _objCharacter.SkillsSection.SkillGroups.FirstOrDefault(x => x.Name == ImprovedName);
                    if (objTargetGroup != null)
                    {
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetGroup,
                            nameof(SkillGroup.FreeBase));
                    }
                }
                    break;
                case ImprovementType.Skillsoft:
                {
                    KnowledgeSkill objTargetSkill = _objCharacter.SkillsSection.KnowledgeSkills.FirstOrDefault(x =>
                        x.InternalId == ImprovedName || x.CurrentDisplayName == ImprovedName);
                    if (objTargetSkill != null)
                    {
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                            nameof(Skill.CyberwareRating));
                    }
                }
                    break;
                case ImprovementType.Activesoft:
                {
                    Skill objTargetSkill =
                        _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.Name == ImprovedName) ?? _objCharacter
                            .SkillsSection.Skills.OfType<ExoticSkill>()
                            .FirstOrDefault(x => x.Name + " (" + x.Specific + ')' == ImprovedName);
                    if (objTargetSkill != null)
                    {
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                            nameof(Skill.CyberwareRating));
                    }
                }
                    break;
                case ImprovementType.ReplaceAttribute:
                {
                    string strTargetAttribute = ImprovedName;
                    foreach (CharacterAttrib objCharacterAttrib in _objCharacter.AttributeSection.AttributeList.Concat(
                        _objCharacter.AttributeSection.SpecialAttributeList))
                    {
                        if (objCharacterAttrib.Abbrev != strTargetAttribute) continue;
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(objCharacterAttrib,
                            nameof(CharacterAttrib.MetatypeMaximum));
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(objCharacterAttrib,
                            nameof(CharacterAttrib.MetatypeMinimum));
                    }
                }
                    break;
                case ImprovementType.SpecialSkills:
                    break;
                case ImprovementType.SkillAttribute:
                case ImprovementType.ReflexRecorderOptimization:
                {
                    foreach (Skill objSkill in _objCharacter.SkillsSection.Skills)
                    {
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(objSkill,
                            nameof(Skill.PoolModifiers));
                    }
                }
                    break;
                case ImprovementType.Ambidextrous:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.Ambidextrous));
                }
                    break;
                case ImprovementType.UnarmedReach:
                    break;
                case ImprovementType.SkillSpecialization:
                    break;
                case ImprovementType.SkillExpertise:
                    break;
                case ImprovementType.SkillSpecializationOption:
                {
                    Skill objTargetSkill =
                        _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.Name == ImprovedName) ?? _objCharacter
                            .SkillsSection.Skills.OfType<ExoticSkill>()
                            .FirstOrDefault(x => x.Name + " (" + x.Specific + ')' == ImprovedName);
                    if (objTargetSkill != null)
                    {
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                            nameof(Skill.CGLSpecializations));
                    }

                    break;
                }
                case ImprovementType.NativeLanguageLimit:
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter.SkillsSection,
                        nameof(SkillsSection.HasAvailableNativeLanguageSlots));
                    break;
                case ImprovementType.AdeptPowerFreePoints:
                {
                    // Get the power improved by this improvement
                    Power objImprovedPower = _objCharacter.Powers.FirstOrDefault(objPower =>
                        objPower.Name == ImprovedName && objPower.Extra == UniqueName);
                    if (objImprovedPower != null)
                    {
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(objImprovedPower,
                            nameof(Power.FreePoints));
                    }
                }
                    break;
                case ImprovementType.AdeptPowerFreeLevels:
                {
                    // Get the power improved by this improvement
                    Power objImprovedPower = _objCharacter.Powers.FirstOrDefault(objPower =>
                        objPower.Name == ImprovedName && objPower.Extra == UniqueName);
                    if (objImprovedPower != null)
                    {
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(objImprovedPower,
                            nameof(Power.FreeLevels));
                    }
                }
                    break;
                case ImprovementType.AIProgram:
                    break;
                case ImprovementType.CritterPowerLevel:
                    break;
                case ImprovementType.CritterPower:
                    break;
                case ImprovementType.SpellResistance:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.SpellResistance));
                }
                    break;
                case ImprovementType.LimitSpellCategory:
                    break;
                case ImprovementType.LimitSpellDescriptor:
                    break;
                case ImprovementType.LimitSpiritCategory:
                    break;
                case ImprovementType.WalkSpeed:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.WalkingRate));
                }
                    break;
                case ImprovementType.RunSpeed:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.RunningRate));
                }
                    break;
                case ImprovementType.SprintSpeed:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.SprintingRate));
                }
                    break;
                case ImprovementType.WalkMultiplier:
                case ImprovementType.WalkMultiplierPercent:
                case ImprovementType.RunMultiplier:
                case ImprovementType.RunMultiplierPercent:
                case ImprovementType.SprintBonus:
                case ImprovementType.SprintBonusPercent:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.CalculatedMovement));
                }
                    break;
                case ImprovementType.EssencePenalty:
                case ImprovementType.EssencePenaltyT100:
                case ImprovementType.EssencePenaltyMAGOnlyT100:
                case ImprovementType.CyborgEssence:
                case ImprovementType.CyberwareEssCost:
                case ImprovementType.CyberwareTotalEssMultiplier:
                case ImprovementType.BiowareEssCost:
                case ImprovementType.BiowareTotalEssMultiplier:
                case ImprovementType.BasicBiowareEssCost:
                    // Immediately reset cached essence to make sure this fires off before any other property changers would
                    _objCharacter.ResetCachedEssence();
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.Essence));
                    break;
                case ImprovementType.FreeSpellsATT:
                    break;
                case ImprovementType.FreeSpells:
                    break;
                case ImprovementType.DrainValue:
                    break;
                case ImprovementType.FadingValue:
                    break;
                case ImprovementType.Spell:
                    break;
                case ImprovementType.ComplexForm:
                    break;
                case ImprovementType.Gear:
                    break;
                case ImprovementType.Weapon:
                    break;
                case ImprovementType.MentorSpirit:
                    break;
                case ImprovementType.Paragon:
                    break;
                case ImprovementType.FreeSpellsSkill:
                    break;
                case ImprovementType.DisableSpecializationEffects:
                {
                    Skill objTargetSkill =
                        _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.Name == ImprovedName) ?? _objCharacter
                            .SkillsSection.Skills.OfType<ExoticSkill>()
                            .FirstOrDefault(x => x.Name + " (" + x.Specific + ')' == ImprovedName);
                    if (objTargetSkill != null)
                    {
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                            nameof(Skill.DisplayPool));
                    }
                }
                    break;
                case ImprovementType.PhysiologicalAddictionFirstTime:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.PhysiologicalAddictionResistFirstTime));
                }
                    break;
                case ImprovementType.PsychologicalAddictionFirstTime:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.PsychologicalAddictionResistFirstTime));
                }
                    break;
                case ImprovementType.PhysiologicalAddictionAlreadyAddicted:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.PhysiologicalAddictionResistAlreadyAddicted));
                }
                    break;
                case ImprovementType.PsychologicalAddictionAlreadyAddicted:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.PsychologicalAddictionResistAlreadyAddicted));
                }
                    break;
                case ImprovementType.AddESStoStunCMRecovery:
                case ImprovementType.StunCMRecovery:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.StunCMNaturalRecovery));
                }
                    break;
                case ImprovementType.AddESStoPhysicalCMRecovery:
                case ImprovementType.PhysicalCMRecovery:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.PhysicalCMNaturalRecovery));
                }
                    break;
                case ImprovementType.MentalManipulationResist:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.SpellDefenseManipulationMental));
                }
                    break;
                case ImprovementType.PhysicalManipulationResist:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.SpellDefenseManipulationPhysical));
                }
                    break;
                case ImprovementType.ManaIllusionResist:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.SpellDefenseIllusionMana));
                }
                    break;
                case ImprovementType.PhysicalIllusionResist:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.SpellDefenseIllusionPhysical));
                }
                    break;
                case ImprovementType.DetectionSpellResist:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.SpellDefenseDetection));
                }
                    break;
                case ImprovementType.DirectManaSpellResist:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.SpellDefenseDirectSoakMana));
                }
                    break;
                case ImprovementType.DirectPhysicalSpellResist:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.SpellDefenseDirectSoakPhysical));
                }
                    break;
                case ImprovementType.DecreaseBODResist:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.SpellDefenseDecreaseBOD));
                }
                    break;
                case ImprovementType.DecreaseAGIResist:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.SpellDefenseDecreaseAGI));
                }
                    break;
                case ImprovementType.DecreaseREAResist:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.SpellDefenseDecreaseREA));
                }
                    break;
                case ImprovementType.DecreaseSTRResist:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.SpellDefenseDecreaseSTR));
                }
                    break;
                case ImprovementType.DecreaseCHAResist:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.SpellDefenseDecreaseCHA));
                }
                    break;
                case ImprovementType.DecreaseINTResist:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.SpellDefenseDecreaseINT));
                }
                    break;
                case ImprovementType.DecreaseLOGResist:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.SpellDefenseDecreaseLOG));
                }
                    break;
                case ImprovementType.DecreaseWILResist:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.SpellDefenseDecreaseWIL));
                }
                    break;
                case ImprovementType.AddLimb:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.LimbCount));
                }
                    break;
                case ImprovementType.StreetCredMultiplier:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.CalculatedStreetCred));
                }
                    break;
                case ImprovementType.StreetCred:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.TotalStreetCred));
                }
                    break;
                case ImprovementType.AttributeKarmaCostMultiplier:
                case ImprovementType.AttributeKarmaCost:
                {
                    if (!string.IsNullOrEmpty(ImprovedName))
                    {
                        string strTargetAttribute = ImprovedName;
                        foreach (CharacterAttrib objCharacterAttrib in _objCharacter.AttributeSection.AttributeList
                            .Concat(_objCharacter.AttributeSection.SpecialAttributeList))
                        {
                            if (objCharacterAttrib.Abbrev == strTargetAttribute)
                            {
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objCharacterAttrib,
                                    nameof(CharacterAttrib.UpgradeKarmaCost));
                            }
                        }
                    }
                    else
                    {
                        foreach (CharacterAttrib objCharacterAttrib in _objCharacter.AttributeSection.AttributeList
                            .Concat(_objCharacter.AttributeSection.SpecialAttributeList))
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objCharacterAttrib,
                                nameof(CharacterAttrib.UpgradeKarmaCost));
                        }
                    }
                }
                    break;
                case ImprovementType.ActiveSkillKarmaCost:
                case ImprovementType.ActiveSkillKarmaCostMultiplier:
                {
                    if (!string.IsNullOrEmpty(ImprovedName))
                    {
                        Skill objTargetSkill =
                            _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.Name == ImprovedName) ??
                            _objCharacter.SkillsSection.Skills.OfType<ExoticSkill>()
                                .FirstOrDefault(x => x.Name + " (" + x.Specific + ')' == ImprovedName);
                        if (objTargetSkill != null)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                nameof(Skill.UpgradeKarmaCost));
                        }
                    }
                    else
                    {
                        foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                nameof(Skill.UpgradeKarmaCost));
                        }
                    }
                }
                    break;
                case ImprovementType.KnowledgeSkillKarmaCost:
                case ImprovementType.KnowledgeSkillKarmaCostMinimum:
                case ImprovementType.KnowledgeSkillKarmaCostMultiplier:
                {
                    if (!string.IsNullOrEmpty(ImprovedName))
                    {
                        KnowledgeSkill objTargetSkill = _objCharacter.SkillsSection.KnowledgeSkills.FirstOrDefault(x =>
                            x.Name == ImprovedName || x.CurrentDisplayName == ImprovedName);
                        if (objTargetSkill != null)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                nameof(Skill.UpgradeKarmaCost));
                        }
                    }
                    else
                    {
                        foreach (KnowledgeSkill objTargetSkill in _objCharacter.SkillsSection.KnowledgeSkills)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                nameof(Skill.UpgradeKarmaCost));
                        }
                    }
                }
                    break;
                case ImprovementType.SkillGroupKarmaCost:
                case ImprovementType.SkillGroupKarmaCostMultiplier:
                {
                    if (!string.IsNullOrEmpty(ImprovedName))
                    {
                        SkillGroup objTargetGroup =
                            _objCharacter.SkillsSection.SkillGroups.FirstOrDefault(x => x.Name == ImprovedName);
                        if (objTargetGroup != null)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetGroup,
                                nameof(SkillGroup.UpgradeKarmaCost));
                        }
                    }
                    else
                    {
                        foreach (SkillGroup objTargetGroup in _objCharacter.SkillsSection.SkillGroups)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetGroup,
                                nameof(SkillGroup.UpgradeKarmaCost));
                        }
                    }
                }
                    break;
                case ImprovementType.SkillGroupDisable:
                {
                    SkillGroup objTargetGroup =
                        _objCharacter.SkillsSection.SkillGroups.FirstOrDefault(x => x.Name == ImprovedName);
                    if (objTargetGroup != null)
                    {
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetGroup,
                            nameof(SkillGroup.IsDisabled));
                    }

                    break;
                }
                case ImprovementType.SkillDisable:
                {
                    Skill objTargetSkill =
                        _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.Name == ImprovedName) ?? _objCharacter
                            .SkillsSection.Skills.OfType<ExoticSkill>()
                            .FirstOrDefault(x => x.Name + " (" + x.Specific + ')' == ImprovedName);
                    if (objTargetSkill != null)
                    {
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                            nameof(Skill.Enabled));
                    }
                }
                    break;
                case ImprovementType.SkillCategorySpecializationKarmaCost:
                case ImprovementType.SkillCategorySpecializationKarmaCostMultiplier:
                {
                    foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills
                        .Concat(_objCharacter.SkillsSection.KnowledgeSkills)
                        .Where(x => x.SkillCategory == ImprovedName))
                    {
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                            nameof(Skill.CanAffordSpecialization));
                    }
                }
                    break;
                case ImprovementType.SkillCategoryKarmaCost:
                case ImprovementType.SkillCategoryKarmaCostMultiplier:
                {
                    foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills
                        .Concat(_objCharacter.SkillsSection.KnowledgeSkills)
                        .Where(x => x.SkillCategory == ImprovedName))
                    {
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                            nameof(Skill.UpgradeKarmaCost));
                    }
                }
                    break;
                case ImprovementType.SkillGroupCategoryDisable:
                {
                    foreach (SkillGroup objTargetGroup in _objCharacter.SkillsSection.SkillGroups.Where(x =>
                        x.GetRelevantSkillCategories.Contains(ImprovedName)))
                    {
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetGroup,
                            nameof(SkillGroup.IsDisabled));
                    }
                }
                    break;
                case ImprovementType.SkillGroupCategoryKarmaCostMultiplier:
                case ImprovementType.SkillGroupCategoryKarmaCost:
                {
                    foreach (SkillGroup objTargetGroup in _objCharacter.SkillsSection.SkillGroups.Where(x =>
                        x.GetRelevantSkillCategories.Contains(ImprovedName)))
                    {
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetGroup,
                            nameof(SkillGroup.UpgradeKarmaCost));
                    }
                }
                    break;
                case ImprovementType.AttributePointCost:
                case ImprovementType.AttributePointCostMultiplier:
                    break;
                case ImprovementType.ActiveSkillPointCost:
                case ImprovementType.ActiveSkillPointCostMultiplier:
                    break;
                case ImprovementType.SkillGroupPointCost:
                case ImprovementType.SkillGroupPointCostMultiplier:
                    break;
                case ImprovementType.KnowledgeSkillPointCost:
                case ImprovementType.KnowledgeSkillPointCostMultiplier:
                    break;
                case ImprovementType.SkillCategoryPointCost:
                case ImprovementType.SkillCategoryPointCostMultiplier:
                    break;
                case ImprovementType.SkillGroupCategoryPointCost:
                case ImprovementType.SkillGroupCategoryPointCostMultiplier:
                    break;
                case ImprovementType.NewSpellKarmaCost:
                case ImprovementType.NewSpellKarmaCostMultiplier:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.SpellKarmaCost));
                }
                    break;
                case ImprovementType.NewComplexFormKarmaCost:
                case ImprovementType.NewComplexFormKarmaCostMultiplier:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.ComplexFormKarmaCost));
                }
                    break;
                case ImprovementType.NewAIProgramKarmaCost:
                case ImprovementType.NewAIProgramKarmaCostMultiplier:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.AIProgramKarmaCost));
                }
                    break;
                case ImprovementType.NewAIAdvancedProgramKarmaCost:
                case ImprovementType.NewAIAdvancedProgramKarmaCostMultiplier:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.AIAdvancedProgramKarmaCost));
                }
                    break;
                case ImprovementType.BlockSkillSpecializations:
                {
                    if (!string.IsNullOrEmpty(ImprovedName))
                    {
                        Skill objTargetSkill =
                            _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.Name == ImprovedName) ??
                            _objCharacter.SkillsSection.Skills.OfType<ExoticSkill>()
                                .FirstOrDefault(x => x.Name + " (" + x.Specific + ')' == ImprovedName);
                        if (objTargetSkill != null)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                nameof(Skill.CanHaveSpecs));
                        }
                    }
                    else
                    {
                        foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                nameof(Skill.CanHaveSpecs));
                        }
                    }
                }
                    break;
                case ImprovementType.BlockSkillCategorySpecializations:
                {
                    foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills
                        .Concat(_objCharacter.SkillsSection.KnowledgeSkills)
                        .Where(x => x.SkillCategory == ImprovedName))
                    {
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                            nameof(Skill.CanHaveSpecs));
                    }
                }
                    break;
                case ImprovementType.FocusBindingKarmaCost:
                    break;
                case ImprovementType.FocusBindingKarmaMultiplier:
                    break;
                case ImprovementType.MagiciansWayDiscount:
                {
                    foreach (Power objLoopPower in _objCharacter.Powers.Where(x => x.AdeptWayDiscount != 0))
                    {
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(objLoopPower,
                            nameof(Power.AdeptWayDiscountEnabled));
                    }
                }
                    break;
                case ImprovementType.BurnoutsWay:
                    break;
                case ImprovementType.ContactForcedLoyalty:
                {
                    Contact objTargetContact = _objCharacter.Contacts.FirstOrDefault(x => x.UniqueId == ImprovedName);
                    if (objTargetContact != null)
                    {
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetContact,
                            nameof(Contact.ForcedLoyalty));
                    }
                }
                    break;
                case ImprovementType.ContactMakeFree:
                {
                    Contact objTargetContact = _objCharacter.Contacts.FirstOrDefault(x => x.UniqueId == ImprovedName);
                    if (objTargetContact != null)
                    {
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetContact,
                            nameof(Contact.Free));
                    }
                }
                    break;
                case ImprovementType.FreeWare:
                    break;
                case ImprovementType.WeaponSkillAccuracy:
                    break;
                case ImprovementType.WeaponAccuracy:
                    break;
                case ImprovementType.SpecialModificationLimit:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.SpecialModificationLimit));
                }
                    break;
                case ImprovementType.MetageneticLimit:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.MetagenicLimit));
                }
                    break;
                case ImprovementType.DisableQuality:
                {
                    Quality objQuality = _objCharacter.Qualities.FirstOrDefault(x =>
                        x.Name == ImprovedName || x.SourceIDString == ImprovedName);
                    if (objQuality != null)
                    {
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(objQuality,
                            nameof(Quality.Suppressed));
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                            nameof(Character.Qualities));
                    }
                }
                    break;
                case ImprovementType.FreeQuality:
                {
                    Quality objQuality = _objCharacter.Qualities.FirstOrDefault(x =>
                        x.Name == ImprovedName || x.SourceIDString == ImprovedName);
                    if (objQuality != null)
                    {
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(objQuality,
                            nameof(Quality.ContributeToBP));
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(objQuality,
                            nameof(Quality.ContributeToLimit));
                    }
                }
                    break;
                case ImprovementType.AllowSpriteFettering:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.AllowSpriteFettering));
                    break;
                }
                case ImprovementType.Surprise:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.Surprise));
                    break;
                }
                case ImprovementType.AstralReputation:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.AstralReputation));
                        break;
                }
                case ImprovementType.AstralReputationWild:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.WildReputation));
                        break;
                }
            }
        }

        #region UI Methods
        public TreeNode CreateTreeNode(ContextMenuStrip cmsImprovement)
        {
            TreeNode nodImprovement = new TreeNode
            {
                Tag = this,
                Text = CustomName,
                ToolTipText = Notes.WordWrap(100),
                ContextMenuStrip = cmsImprovement,
                ForeColor = PreferredColor
            };
            return nodImprovement;
        }

        public Color PreferredColor
        {
            get
            {
                if (!string.IsNullOrEmpty(Notes))
                {
                    if (Enabled)
                        return Color.SaddleBrown;
                    return Color.SandyBrown;
                }
                if (Enabled)
                    return SystemColors.WindowText;
                return SystemColors.GrayText;
            }
        }
        #endregion
        #endregion

        public string InternalId => SourceName;
    }

    public struct ImprovementDictionaryKey : IEquatable<ImprovementDictionaryKey>, IEquatable<Tuple<Character, Improvement.ImprovementType, string>>
    {
        private readonly Tuple<Character, Improvement.ImprovementType, string> _objTupleKey;

        public Character CharacterObject => _objTupleKey.Item1;
        public Improvement.ImprovementType ImprovementType => _objTupleKey.Item2;
        public string ImprovementName => _objTupleKey.Item3;

        public ImprovementDictionaryKey(Character objCharacter, Improvement.ImprovementType eImprovementType, string strImprovementName)
        {
            _objTupleKey = new Tuple<Character, Improvement.ImprovementType, string>(objCharacter, eImprovementType, strImprovementName);
        }

        public override bool Equals(object obj)
        {
            if (obj is ImprovementDictionaryKey objOtherImprovementDictionaryKey)
            {
                return Equals(objOtherImprovementDictionaryKey);
            }
            if (obj is Tuple<Character, Improvement.ImprovementType, string> objOtherTuple)
            {
                return Equals(objOtherTuple);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return new {CharacterObject, ImprovementType, ImprovementName}.GetHashCode();
        }

        public bool Equals(ImprovementDictionaryKey other)
        {
            if (other == null)
                return false;
            return CharacterObject == other.CharacterObject &&
                   ImprovementType == other.ImprovementType &&
                   ImprovementName == other.ImprovementName;
        }

        public bool Equals(Tuple<Character, Improvement.ImprovementType, string> other)
        {
            if (other == null)
                return false;
            return CharacterObject == other.Item1 &&
                   ImprovementType == other.Item2 &&
                   ImprovementName == other.Item3;
        }

        public override string ToString()
        {
            return _objTupleKey.ToString();
        }

        public static bool operator ==(ImprovementDictionaryKey x, object y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(ImprovementDictionaryKey x, object y)
        {
            return !x.Equals(y);
        }

        public static bool operator ==(object x, ImprovementDictionaryKey y)
        {
            return x?.Equals(y) ?? y == null;
        }

        public static bool operator !=(object x, ImprovementDictionaryKey y)
        {
            return !(x?.Equals(y) ?? y == null);
        }
    }

    public class TransactingImprovement
    {
        public TransactingImprovement(Improvement objImprovement)
        {
            ImprovementObject = objImprovement;
        }

        public Improvement ImprovementObject { get; }

        public bool IsCommitting { get; set; }
    }

    public static class ImprovementManager
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        // String that will be used to limit the selection in Pick forms.
        private static string s_StrLimitSelection = string.Empty;

        private static string s_StrSelectedValue = string.Empty;
        private static string s_StrForcedValue = string.Empty;
        private static readonly ConcurrentDictionary<Character, List<TransactingImprovement>> s_DictionaryTransactions = new ConcurrentDictionary<Character, List<TransactingImprovement>>(8, 10);
        private static readonly ConcurrentDictionary<ImprovementDictionaryKey, int> s_DictionaryCachedValues = new ConcurrentDictionary<ImprovementDictionaryKey, int>(8, (int)Improvement.ImprovementType.NumImprovementTypes);
        private static readonly ConcurrentDictionary<ImprovementDictionaryKey, int> s_DictionaryCachedAugmentedValues = new ConcurrentDictionary<ImprovementDictionaryKey, int>(8, (int)Improvement.ImprovementType.NumImprovementTypes);

        #region Properties
        /// <summary>
        /// Limit what can be selected in Pick forms to a single value. This is typically used when selecting the Qualities for a Metavariant that has a specific
        /// CharacterAttribute selection for Qualities like Metagenic Improvement.
        /// </summary>
        public static string LimitSelection
        {
            get => s_StrLimitSelection;
            set => s_StrLimitSelection = value;
        }

        /// <summary>
        /// The string that was entered or selected from any of the dialogue windows that were presented because of this Improvement.
        /// </summary>
        public static string SelectedValue
        {
            get => s_StrSelectedValue;
            set => s_StrSelectedValue = value;
        }

        /// <summary>
        /// Force any dialogue windows that open to use this string as their selected value.
        /// </summary>
        public static string ForcedValue
        {
            get => s_StrForcedValue;
            set => s_StrForcedValue = value;
        }

        public static void ClearCachedValue(Character objCharacter, Improvement.ImprovementType eImprovementType, string strImprovementName = "")
        {
            if (!string.IsNullOrEmpty(strImprovementName))
            {
                ImprovementDictionaryKey objCheckKey = new ImprovementDictionaryKey(objCharacter, eImprovementType, strImprovementName);
                if (!s_DictionaryCachedValues.TryAdd(objCheckKey, int.MinValue))
                    s_DictionaryCachedValues[objCheckKey] = int.MinValue;
                if (!s_DictionaryCachedAugmentedValues.TryAdd(objCheckKey, int.MinValue))
                    s_DictionaryCachedAugmentedValues[objCheckKey] = int.MinValue;
            }
            else
            {
                foreach (ImprovementDictionaryKey objCheckKey in s_DictionaryCachedValues.Keys.Where(x => x.CharacterObject == objCharacter && x.ImprovementType == eImprovementType).ToList())
                    s_DictionaryCachedValues[objCheckKey] = int.MinValue;
                foreach (ImprovementDictionaryKey objCheckKey in s_DictionaryCachedAugmentedValues.Keys.Where(x => x.CharacterObject == objCharacter && x.ImprovementType == eImprovementType).ToList())
                    s_DictionaryCachedAugmentedValues[objCheckKey] = int.MinValue;
            }
        }

        public static void ClearCachedValues(Character objCharacter)
        {
            foreach (ImprovementDictionaryKey objKey in s_DictionaryCachedValues.Keys.ToList())
            {
                if (objKey.CharacterObject == objCharacter)
                    s_DictionaryCachedValues.TryRemove(objKey, out int _);
            }
            foreach (ImprovementDictionaryKey objKey in s_DictionaryCachedAugmentedValues.Keys.ToList())
            {
                if (objKey.CharacterObject == objCharacter)
                    s_DictionaryCachedAugmentedValues.TryRemove(objKey, out int _);
            }
            s_DictionaryTransactions.TryRemove(objCharacter, out List<TransactingImprovement> _);
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Retrieve the total Improvement value for the specified ImprovementType.
        /// </summary>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="objImprovementType">ImprovementType to retrieve the value of.</param>
        /// <param name="blnAddToRating">Whether or not we should only retrieve values that have AddToRating enabled.</param>
        /// <param name="strImprovedName">Name to assign to the Improvement.</param>
        /// <param name="blnUnconditionalOnly">Whether to only fetch values for improvements that do not have a condition.</param>
        /// <param name="blnIncludeNonImproved">Whether to only fetch values for improvements that do not have an improvedname when specifying ImprovedNames.</param>
        public static int ValueOf(Character objCharacter, Improvement.ImprovementType objImprovementType, bool blnAddToRating = false, string strImprovedName = "", bool blnUnconditionalOnly = true, bool blnIncludeNonImproved = false)
        {
            //Log.Enter("ValueOf");
            //Log.Info("objImprovementType = " + objImprovementType.ToString());
            //Log.Info("blnAddToRating = " + blnAddToRating.ToString());
            //Log.Info("strImprovedName = " + ("" + strImprovedName).ToString());

            if (objCharacter == null)
            {
                //Log.Exit("ValueOf");
                return 0;
            }

            // If we've got a value cached for the default ValueOf call for an improvementType, let's just return that
            if (!blnAddToRating && blnUnconditionalOnly)
            {
                if (!string.IsNullOrEmpty(strImprovedName))
                {
                    ImprovementDictionaryKey objCacheKey = new ImprovementDictionaryKey(objCharacter, objImprovementType, strImprovedName);
                    if (s_DictionaryCachedValues.TryGetValue(objCacheKey, out int intCachedValue) && intCachedValue != int.MinValue)
                    {
                        return intCachedValue;
                    }
                }
                else
                {
                    bool blnDoRecalculate = true;
                    int intCachedValue = 0;
                    // Only fetch based on cached values if the dictionary contains at least one element with matching characters and types and none of those elements have a "reset" value of int.MinValue
                    foreach (KeyValuePair<ImprovementDictionaryKey, int> objLoopCachedEntry in s_DictionaryCachedValues)
                    {
                        ImprovementDictionaryKey objLoopKey = objLoopCachedEntry.Key;
                        if (objLoopKey.CharacterObject == objCharacter && objLoopKey.ImprovementType == objImprovementType)
                        {
                            blnDoRecalculate = false;
                            int intLoopCachedValue = objLoopCachedEntry.Value;
                            if (intLoopCachedValue == int.MinValue)
                            {
                                blnDoRecalculate = true;
                                break;
                            }
                            intCachedValue += intLoopCachedValue;
                        }
                    }
                    if (!blnDoRecalculate)
                    {
                        return intCachedValue;
                    }
                }
            }

            Dictionary<string, HashSet<string>> dicUniqueNames = new Dictionary<string, HashSet<string>>();
            Dictionary<string, List<Tuple<string, int>>> dicUniquePairs = new Dictionary<string, List<Tuple<string, int>>>();
            Dictionary<string, int> dicValues = new Dictionary<string, int>();
            foreach (Improvement objImprovement in objCharacter.Improvements)
            {
                if (objImprovement.ImproveType != objImprovementType || !objImprovement.Enabled ||
                    objImprovement.Custom ||
                    (blnUnconditionalOnly && !string.IsNullOrEmpty(objImprovement.Condition))) continue;
                string strLoopImprovedName = objImprovement.ImprovedName;
                bool blnAllowed = objImprovement.ImproveType == objImprovementType &&
                                  !(objCharacter.RESEnabled && objImprovement.ImproveSource == Improvement.ImprovementSource.Gear &&
                                    objImprovementType == Improvement.ImprovementType.MatrixInitiativeDice) &&
                                  // Ignore items that apply to a Skill's Rating.
                                  objImprovement.AddToRating == blnAddToRating &&
                                  // If an Improved Name has been passed, only retrieve values that have this Improved Name.
                                  (string.IsNullOrEmpty(strImprovedName) || strImprovedName == strLoopImprovedName ||
                                   blnIncludeNonImproved && string.IsNullOrWhiteSpace(strLoopImprovedName));

                if (!blnAllowed) continue;
                string strUniqueName = objImprovement.UniqueName;
                if (!string.IsNullOrEmpty(strUniqueName))
                {
                    // If this has a UniqueName, run through the current list of UniqueNames seen. If it is not already in the list, add it.
                    if (dicUniqueNames.TryGetValue(strLoopImprovedName, out HashSet<string> lstUniqueNames))
                    {
                        if (!lstUniqueNames.Contains(strUniqueName))
                            lstUniqueNames.Add(strUniqueName);
                    }
                    else
                    {
                        dicUniqueNames.Add(strLoopImprovedName, new HashSet<string>{strUniqueName});
                    }

                    // Add the values to the UniquePair List so we can check them later.
                    if (dicUniquePairs.TryGetValue(strLoopImprovedName, out List<Tuple<string, int>> lstUniquePairs))
                    {
                        lstUniquePairs.Add(new Tuple<string, int>(strUniqueName, objImprovement.Value));
                    }
                    else
                    {
                        dicUniquePairs.Add(strLoopImprovedName, new List<Tuple<string, int>> { new Tuple<string, int>(strUniqueName, objImprovement.Value) });
                    }

                    if (!dicValues.ContainsKey(strLoopImprovedName))
                    {
                        dicValues.Add(strLoopImprovedName, 0);
                    }
                }
                else if (dicValues.ContainsKey(strLoopImprovedName))
                {
                    dicValues[strLoopImprovedName] += objImprovement.Value;
                }
                else
                {
                    dicValues.Add(strLoopImprovedName, objImprovement.Value);
                }
            }

            foreach (KeyValuePair<string, HashSet<string>> objLoopValuePair in dicUniqueNames)
            {
                string strLoopImprovedName = objLoopValuePair.Key;
                HashSet<string> lstUniqueNames = objLoopValuePair.Value;
                bool blnValuesDictionaryContains = dicValues.TryGetValue(strLoopImprovedName, out int intLoopValue);
                if (dicUniquePairs.TryGetValue(strLoopImprovedName, out List<Tuple<string, int>> lstUniquePairs))
                {
                    if (lstUniqueNames.Contains("precedence0"))
                    {
                        // Retrieve only the highest precedence0 value.
                        // Run through the list of UniqueNames and pick out the highest value for each one.
                        int intHighest = int.MinValue;
                        foreach (Tuple<string, int> objLoopUniquePair in lstUniquePairs)
                        {
                            if (objLoopUniquePair.Item1 == "precedence0")
                                intHighest = Math.Max(intHighest, objLoopUniquePair.Item2);
                        }
                        if (lstUniqueNames.Contains("precedence-1"))
                        {
                            intHighest += lstUniquePairs.Where(strValues => strValues.Item1 == "precedence-1").Sum(strValues => strValues.Item2);
                        }
                        intLoopValue = Math.Max(intLoopValue, intHighest);
                    }
                    else if (lstUniqueNames.Contains("precedence1"))
                    {
                        // Retrieve all of the items that are precedence1 and nothing else.
                        intLoopValue = Math.Max(intLoopValue, lstUniquePairs.Where(strValues => strValues.Item1 == "precedence1" || strValues.Item1 == "precedence-1").Sum(strValues => strValues.Item2));
                    }
                    else
                    {
                        // Run through the list of UniqueNames and pick out the highest value for each one.
                        foreach (string strUniqueName in lstUniqueNames)
                        {
                            int intInnerLoopValue = int.MinValue;
                            foreach (Tuple<string, int> objLoopUniquePair in lstUniquePairs)
                            {
                                if (objLoopUniquePair.Item1 == strUniqueName)
                                    intInnerLoopValue = Math.Max(intInnerLoopValue, objLoopUniquePair.Item2);
                            }
                            if (intInnerLoopValue != int.MinValue)
                                intLoopValue += intInnerLoopValue;
                        }
                    }
                    if (blnValuesDictionaryContains)
                        dicValues[strLoopImprovedName] = intLoopValue;
                    else
                        dicValues.Add(strLoopImprovedName, intLoopValue);
                }
            }

            // Factor in Custom Improvements.
            dicUniqueNames.Clear();
            dicUniquePairs.Clear();
            Dictionary<string, int> dicCustomValues = new Dictionary<string, int>();
            foreach (Improvement objImprovement in objCharacter.Improvements)
            {
                if (!objImprovement.Custom || !objImprovement.Enabled ||
                    (blnUnconditionalOnly && !string.IsNullOrEmpty(objImprovement.Condition))) continue;
                string strLoopImprovedName = objImprovement.ImprovedName;
                bool blnAllowed = objImprovement.ImproveType == objImprovementType &&
                                  !(objCharacter.RESEnabled && objImprovement.ImproveSource == Improvement.ImprovementSource.Gear &&
                                    objImprovementType == Improvement.ImprovementType.MatrixInitiativeDice) &&
                                  // Ignore items that apply to a Skill's Rating.
                                  objImprovement.AddToRating == blnAddToRating &&
                                  // If an Improved Name has been passed, only retrieve values that have this Improved Name.
                                  (string.IsNullOrEmpty(strImprovedName) || strImprovedName == strLoopImprovedName);

                if (!blnAllowed) continue;
                string strUniqueName = objImprovement.UniqueName;
                if (!string.IsNullOrEmpty(strUniqueName))
                {
                    // If this has a UniqueName, run through the current list of UniqueNames seen. If it is not already in the list, add it.
                    if (dicUniqueNames.TryGetValue(strLoopImprovedName, out HashSet<string> lstUniqueNames))
                    {
                        if (!lstUniqueNames.Contains(strUniqueName))
                            lstUniqueNames.Add(strUniqueName);
                    }
                    else
                    {
                        dicUniqueNames.Add(strLoopImprovedName, new HashSet<string> { strUniqueName });
                    }

                    // Add the values to the UniquePair List so we can check them later.
                    if (dicUniquePairs.TryGetValue(strLoopImprovedName, out List<Tuple<string, int>> lstUniquePairs))
                    {
                        lstUniquePairs.Add(new Tuple<string, int>(strUniqueName, objImprovement.Value));
                    }
                    else
                    {
                        dicUniquePairs.Add(strLoopImprovedName, new List<Tuple<string, int>> { new Tuple<string, int>(strUniqueName, objImprovement.Value) });
                    }

                    if (!dicCustomValues.ContainsKey(strLoopImprovedName))
                    {
                        dicCustomValues.Add(strLoopImprovedName, 0);
                    }
                }
                else if (dicCustomValues.ContainsKey(strLoopImprovedName))
                {
                    dicCustomValues[strLoopImprovedName] += objImprovement.Value;
                }
                else
                {
                    dicCustomValues.Add(strLoopImprovedName, objImprovement.Value);
                }
            }

            foreach (KeyValuePair<string, HashSet<string>> objLoopValuePair in dicUniqueNames)
            {
                string strLoopImprovedName = objLoopValuePair.Key;
                HashSet<string> lstUniqueNames = objLoopValuePair.Value;
                bool blnValuesDictionaryContains = dicCustomValues.TryGetValue(strLoopImprovedName, out int intLoopValue);
                if (dicUniquePairs.TryGetValue(strLoopImprovedName, out List<Tuple<string, int>> lstUniquePairs))
                {
                    // Run through the list of UniqueNames and pick out the highest value for each one.
                    foreach (string strUniqueName in lstUniqueNames)
                    {
                        int intInnerLoopValue = int.MinValue;
                        foreach (Tuple<string, int> objLoopUniquePair in lstUniquePairs)
                        {
                            if (objLoopUniquePair.Item1 == strUniqueName)
                                intInnerLoopValue = Math.Max(intInnerLoopValue, objLoopUniquePair.Item2);
                        }
                        if (intInnerLoopValue != int.MinValue)
                            intLoopValue += intInnerLoopValue;
                    }
                    if (blnValuesDictionaryContains)
                        dicCustomValues[strLoopImprovedName] = intLoopValue;
                    else
                        dicCustomValues.Add(strLoopImprovedName, intLoopValue);
                }
            }

            foreach (KeyValuePair<string, int> objLoopValuePair in dicCustomValues)
            {
                string strLoopImprovedName = objLoopValuePair.Key;
                if (dicValues.ContainsKey(strLoopImprovedName))
                {
                    dicValues[strLoopImprovedName] += objLoopValuePair.Value;
                }
                else
                {
                    dicValues.Add(strLoopImprovedName, objLoopValuePair.Value);
                }
            }

            int intReturn = 0;

            //Log.Exit("ValueOf");
            // If this is the default ValueOf() call, let's cache the value we've calculated so that we don't have to do this all over again unless something has changed
            if (!blnAddToRating && blnUnconditionalOnly)
            {
                foreach (KeyValuePair<string, int> objLoopValuePair in dicValues)
                {
                    string strLoopImprovedName = objLoopValuePair.Key;
                    int intLoopValue = objLoopValuePair.Value;
                    ImprovementDictionaryKey objLoopCacheKey = new ImprovementDictionaryKey(objCharacter, objImprovementType, strLoopImprovedName);
                    if (!s_DictionaryCachedValues.TryAdd(objLoopCacheKey, intLoopValue))
                        s_DictionaryCachedValues[objLoopCacheKey] = intLoopValue;
                    intReturn += intLoopValue;
                }
            }

            return intReturn;
        }

        /// <summary>
        /// Retrieve the total Improvement Augmented x Rating for the specified ImprovementType.
        /// </summary>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="objImprovementType">ImprovementType to retrieve the value of.</param>
        /// <param name="blnAddToRating">Whether or not we should only retrieve values that have AddToRating enabled.</param>
        /// <param name="strImprovedName">Name to assign to the Improvement.</param>
        /// <param name="blnUnconditionalOnly">Whether to only fetch values for improvements that do not have a condition.</param>
        public static int AugmentedValueOf(Character objCharacter, Improvement.ImprovementType objImprovementType, bool blnAddToRating = false, string strImprovedName = "", bool blnUnconditionalOnly = true)
        {
            //Log.Enter("AugmentedValueOf");
            //Log.Info("objImprovementType = " + objImprovementType.ToString());
            //Log.Info("blnAddToRating = " + blnAddToRating.ToString());
            //Log.Info("strImprovedName = " + ("" + strImprovedName).ToString());

            if (objCharacter == null)
            {
                //Log.Exit("AugmentedValueOf");
                return 0;
            }

            // If we've got a value cached for the default AugmentedValueOf call for an improvementType, let's just return that
            if (!blnAddToRating && blnUnconditionalOnly)
            {
                if (!string.IsNullOrEmpty(strImprovedName))
                {
                    ImprovementDictionaryKey objCacheKey = new ImprovementDictionaryKey(objCharacter, objImprovementType, strImprovedName);
                    if (s_DictionaryCachedAugmentedValues.TryGetValue(objCacheKey, out int intCachedValue) && intCachedValue != int.MinValue)
                    {
                        return intCachedValue;
                    }
                }
                else
                {
                    bool blnDoRecalculate = true;
                    int intCachedValue = 0;
                    // Only fetch based on cached values if the dictionary contains at least one element with matching characters and types and none of those elements have a "reset" value of int.MinValue
                    foreach (KeyValuePair<ImprovementDictionaryKey, int> objLoopCachedEntry in s_DictionaryCachedAugmentedValues)
                    {
                        ImprovementDictionaryKey objLoopKey = objLoopCachedEntry.Key;
                        if (objLoopKey.CharacterObject == objCharacter && objLoopKey.ImprovementType == objImprovementType)
                        {
                            blnDoRecalculate = false;
                            int intLoopCachedValue = objLoopCachedEntry.Value;
                            if (intLoopCachedValue == int.MinValue)
                            {
                                blnDoRecalculate = true;
                                break;
                            }
                            intCachedValue += intLoopCachedValue;
                        }
                    }
                    if (!blnDoRecalculate)
                    {
                        return intCachedValue;
                    }
                }
            }

            Dictionary<string, HashSet<string>> dicUniqueNames = new Dictionary<string, HashSet<string>>();
            Dictionary<string, List<Tuple<string, int>>> dicUniquePairs = new Dictionary<string, List<Tuple<string, int>>>();
            Dictionary<string, int> dicValues = new Dictionary<string, int>();
            foreach (Improvement objImprovement in objCharacter.Improvements)
            {
                if (objImprovement.ImproveType == objImprovementType && objImprovement.Enabled && !objImprovement.Custom && (!blnUnconditionalOnly || string.IsNullOrEmpty(objImprovement.Condition)))
                {
                    string strLoopImprovedName = objImprovement.ImprovedName;
                    bool blnAllowed = objImprovement.ImproveType == objImprovementType &&
                        !(objCharacter.RESEnabled && objImprovement.ImproveSource == Improvement.ImprovementSource.Gear &&
                          objImprovementType == Improvement.ImprovementType.MatrixInitiativeDice) &&
                    // Ignore items that apply to a Skill's Rating.
                          objImprovement.AddToRating == blnAddToRating &&
                    // If an Improved Name has been passed, only retrieve values that have this Improved Name.
                          (string.IsNullOrEmpty(strImprovedName) || strImprovedName == strLoopImprovedName);

                    if (blnAllowed)
                    {
                        string strUniqueName = objImprovement.UniqueName;
                        if (!string.IsNullOrEmpty(strUniqueName))
                        {
                            // If this has a UniqueName, run through the current list of UniqueNames seen. If it is not already in the list, add it.
                            if (dicUniqueNames.TryGetValue(strLoopImprovedName, out HashSet<string> lstUniqueNames))
                            {
                                if (!lstUniqueNames.Contains(strUniqueName))
                                    lstUniqueNames.Add(strUniqueName);
                            }
                            else
                            {
                                dicUniqueNames.Add(strLoopImprovedName, new HashSet<string> { strUniqueName });
                            }

                            // Add the values to the UniquePair List so we can check them later.
                            if (dicUniquePairs.TryGetValue(strLoopImprovedName, out List<Tuple<string, int>> lstUniquePairs))
                            {
                                lstUniquePairs.Add(new Tuple<string, int>(strUniqueName, objImprovement.Augmented * objImprovement.Rating));
                            }
                            else
                            {
                                dicUniquePairs.Add(strLoopImprovedName, new List<Tuple<string, int>> { new Tuple<string, int>(strUniqueName, objImprovement.Augmented * objImprovement.Rating) });
                            }

                            if (!dicValues.ContainsKey(strLoopImprovedName))
                            {
                                dicValues.Add(strLoopImprovedName, 0);
                            }
                        }
                        else if (dicValues.ContainsKey(strLoopImprovedName))
                        {
                            dicValues[strLoopImprovedName] += objImprovement.Augmented * objImprovement.Rating;
                        }
                        else
                        {
                            dicValues.Add(strLoopImprovedName, objImprovement.Augmented * objImprovement.Rating);
                        }
                    }
                }
            }

            foreach (KeyValuePair<string, HashSet<string>> objLoopValuePair in dicUniqueNames)
            {
                string strLoopImprovedName = objLoopValuePair.Key;
                HashSet<string> lstUniqueNames = objLoopValuePair.Value;
                bool blnValuesDictionaryContains = dicValues.TryGetValue(strLoopImprovedName, out int intLoopValue);
                if (dicUniquePairs.TryGetValue(strLoopImprovedName, out List<Tuple<string, int>> lstUniquePairs))
                {
                    if (lstUniqueNames.Contains("precedence0"))
                    {
                        // Retrieve only the highest precedence0 value.
                        // Run through the list of UniqueNames and pick out the highest value for each one.
                        int intHighest = int.MinValue;
                        foreach (Tuple<string, int> objLoopUniquePair in lstUniquePairs)
                        {
                            if (objLoopUniquePair.Item1 == "precedence0")
                                intHighest = Math.Max(intHighest, objLoopUniquePair.Item2);
                        }
                        if (lstUniqueNames.Contains("precedence-1"))
                        {
                            intHighest += lstUniquePairs.Where(strValues => strValues.Item1 == "precedence-1").Sum(strValues => strValues.Item2);
                        }
                        intLoopValue = Math.Max(intLoopValue, intHighest);
                    }
                    else if (lstUniqueNames.Contains("precedence1"))
                    {
                        // Retrieve all of the items that are precedence1 and nothing else.
                        intLoopValue = Math.Max(intLoopValue, lstUniquePairs.Where(strValues => strValues.Item1 == "precedence1" || strValues.Item1 == "precedence-1").Sum(strValues => strValues.Item2));
                    }
                    else
                    {
                        // Run through the list of UniqueNames and pick out the highest value for each one.
                        foreach (string strUniqueName in lstUniqueNames)
                        {
                            int intInnerLoopValue = int.MinValue;
                            foreach (Tuple<string, int> objLoopUniquePair in lstUniquePairs)
                            {
                                if (objLoopUniquePair.Item1 == strUniqueName)
                                    intInnerLoopValue = Math.Max(intInnerLoopValue, objLoopUniquePair.Item2);
                            }
                            if (intInnerLoopValue != int.MinValue)
                                intLoopValue += intInnerLoopValue;
                        }
                    }
                    if (blnValuesDictionaryContains)
                        dicValues[strLoopImprovedName] = intLoopValue;
                    else
                        dicValues.Add(strLoopImprovedName, intLoopValue);
                }
            }

            // Factor in Custom Improvements.
            dicUniqueNames.Clear();
            dicUniquePairs.Clear();
            Dictionary<string, int> dicCustomValues = new Dictionary<string, int>();
            foreach (Improvement objImprovement in objCharacter.Improvements)
            {
                if (objImprovement.Custom && objImprovement.Enabled && (!blnUnconditionalOnly || string.IsNullOrEmpty(objImprovement.Condition)))
                {
                    string strLoopImprovedName = objImprovement.ImprovedName;
                    bool blnAllowed = objImprovement.ImproveType == objImprovementType &&
                        !(objCharacter.RESEnabled && objImprovement.ImproveSource == Improvement.ImprovementSource.Gear &&
                          objImprovementType == Improvement.ImprovementType.MatrixInitiativeDice) &&
                    // Ignore items that apply to a Skill's Rating.
                          objImprovement.AddToRating == blnAddToRating &&
                    // If an Improved Name has been passed, only retrieve values that have this Improved Name.
                          (string.IsNullOrEmpty(strImprovedName) || strImprovedName == strLoopImprovedName);

                    if (blnAllowed)
                    {
                        string strUniqueName = objImprovement.UniqueName;
                        if (!string.IsNullOrEmpty(strUniqueName))
                        {
                            // If this has a UniqueName, run through the current list of UniqueNames seen. If it is not already in the list, add it.
                            if (dicUniqueNames.TryGetValue(strLoopImprovedName, out HashSet<string> lstUniqueNames))
                            {
                                if (!lstUniqueNames.Contains(strUniqueName))
                                    lstUniqueNames.Add(strUniqueName);
                            }
                            else
                            {
                                dicUniqueNames.Add(strLoopImprovedName, new HashSet<string> { strUniqueName });
                            }

                            // Add the values to the UniquePair List so we can check them later.
                            if (dicUniquePairs.TryGetValue(strLoopImprovedName, out List<Tuple<string, int>> lstUniquePairs))
                            {
                                lstUniquePairs.Add(new Tuple<string, int>(strUniqueName, objImprovement.Augmented * objImprovement.Rating));
                            }
                            else
                            {
                                dicUniquePairs.Add(strLoopImprovedName, new List<Tuple<string, int>> { new Tuple<string, int>(strUniqueName, objImprovement.Augmented * objImprovement.Rating) });
                            }

                            if (!dicCustomValues.ContainsKey(strLoopImprovedName))
                            {
                                dicCustomValues.Add(strLoopImprovedName, 0);
                            }
                        }
                        else if (dicCustomValues.ContainsKey(strLoopImprovedName))
                        {
                            dicCustomValues[strLoopImprovedName] += objImprovement.Augmented * objImprovement.Rating;
                        }
                        else
                        {
                            dicCustomValues.Add(strLoopImprovedName, objImprovement.Augmented * objImprovement.Rating);
                        }
                    }
                }
            }

            foreach (KeyValuePair<string, HashSet<string>> objLoopValuePair in dicUniqueNames)
            {
                string strLoopImprovedName = objLoopValuePair.Key;
                HashSet<string> lstUniqueNames = objLoopValuePair.Value;
                bool blnValuesDictionaryContains = dicCustomValues.TryGetValue(strLoopImprovedName, out int intLoopValue);
                if (dicUniquePairs.TryGetValue(strLoopImprovedName, out List<Tuple<string, int>> lstUniquePairs))
                {
                    // Run through the list of UniqueNames and pick out the highest value for each one.
                    foreach (string strUniqueName in lstUniqueNames)
                    {
                        int intInnerLoopValue = int.MinValue;
                        foreach (Tuple<string, int> objLoopUniquePair in lstUniquePairs)
                        {
                            if (objLoopUniquePair.Item1 == strUniqueName)
                                intInnerLoopValue = Math.Max(intInnerLoopValue, objLoopUniquePair.Item2);
                        }
                        if (intInnerLoopValue != int.MinValue)
                            intLoopValue += intInnerLoopValue;
                    }
                    if (blnValuesDictionaryContains)
                        dicCustomValues[strLoopImprovedName] = intLoopValue;
                    else
                        dicCustomValues.Add(strLoopImprovedName, intLoopValue);
                }
            }

            foreach (KeyValuePair<string, int> objLoopValuePair in dicCustomValues)
            {
                string strLoopImprovedName = objLoopValuePair.Key;
                if (dicValues.ContainsKey(strLoopImprovedName))
                {
                    dicValues[strLoopImprovedName] += objLoopValuePair.Value;
                }
                else
                {
                    dicValues.Add(strLoopImprovedName, objLoopValuePair.Value);
                }
            }

            int intReturn = 0;

            //Log.Exit("AugmentedValueOf");
            // If this is the default AugmentedValueOf() call, let's cache the value we've calculated so that we don't have to do this all over again unless something has changed
            if (!blnAddToRating && blnUnconditionalOnly)
            {
                foreach (KeyValuePair<string, int> objLoopValuePair in dicValues)
                {
                    string strLoopImprovedName = objLoopValuePair.Key;
                    int intLoopValue = objLoopValuePair.Value;
                    ImprovementDictionaryKey objLoopCacheKey = new ImprovementDictionaryKey(objCharacter, objImprovementType, strLoopImprovedName);
                    if (!s_DictionaryCachedAugmentedValues.TryAdd(objLoopCacheKey, intLoopValue))
                        s_DictionaryCachedAugmentedValues[objLoopCacheKey] = intLoopValue;
                    intReturn += intLoopValue;
                }
            }

            return intReturn;
        }

        /// <summary>
        /// Convert a string to an integer, converting "Rating" to a number where appropriate.
        /// </summary>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="strValue">String value to parse.</param>
        /// <param name="intRating">Integer value to replace "Rating" with.</param>
        public static int ValueToInt(Character objCharacter, string strValue, int intRating)
        {
            if (string.IsNullOrEmpty(strValue))
                return 0;
            //         Log.Enter("ValueToInt");
            //         Log.Info("strValue = " + strValue);
            //Log.Info("intRating = " + intRating.ToString());
            if (strValue.StartsWith("FixedValues(", StringComparison.Ordinal))
            {
                string[] strValues = strValue.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                strValue = strValues[Math.Max(Math.Min(strValues.Length, intRating) - 1, 0)];
            }
            if (strValue.Contains("Rating") || AttributeSection.AttributeStrings.Any(strValue.Contains))
            {
                string strReturn = strValue.Replace("Rating", intRating.ToString(GlobalOptions.InvariantCultureInfo));
                // If the value contain an CharacterAttribute name, replace it with the character's CharacterAttribute.
                foreach (string strAttribute in AttributeSection.AttributeStrings)
                {
                    strReturn = strReturn.CheapReplace(strAttribute, () => objCharacter.GetAttribute(strAttribute).TotalValue.ToString(GlobalOptions.InvariantCultureInfo));
                }

                //Log.Info("strValue = " + strValue);
                //Log.Info("strReturn = " + strReturn);

                // Treat this as a decimal value so any fractions can be rounded down. This is currently only used by the Boosted Reflexes Cyberware from SR2050.
                object objProcess = CommonFunctions.EvaluateInvariantXPath(strReturn, out bool blnIsSuccess);
                int intValue = blnIsSuccess ? Convert.ToInt32(Math.Floor((double)objProcess)) : 0;

                //Log.Exit("ValueToInt");
                return intValue;
            }
            //Log.Exit("ValueToInt");
            int.TryParse(strValue, NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out int intReturn);
            return intReturn;
        }

        public static string DoSelectSkill(XmlNode xmlBonusNode, Character objCharacter, int intRating, string strFriendlyName, ref bool blnIsKnowledgeSkill)
        {
            if (xmlBonusNode == null)
                throw new ArgumentNullException(nameof(xmlBonusNode));
            if (objCharacter == null)
                throw new ArgumentNullException(nameof(objCharacter));
            string strSelectedSkill;
            blnIsKnowledgeSkill = blnIsKnowledgeSkill || xmlBonusNode.Attributes?["knowledgeskills"]?.InnerText == bool.TrueString;
            if (blnIsKnowledgeSkill)
            {
                int intMinimumRating = 0;
                string strMinimumRating = xmlBonusNode.Attributes?["minimumrating"]?.InnerText;
                if (!string.IsNullOrWhiteSpace(strMinimumRating))
                    intMinimumRating = ValueToInt(objCharacter, strMinimumRating, intRating);
                int intMaximumRating = int.MaxValue;
                string strMaximumRating = xmlBonusNode.Attributes?["maximumrating"]?.InnerText;
                string strPrompt = xmlBonusNode.Attributes?["prompt"]?.InnerText ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(strMaximumRating))
                    intMaximumRating = ValueToInt(objCharacter, strMaximumRating, intRating);

                HashSet<string> setAllowedCategories = null;
                string strOnlyCategory = xmlBonusNode.SelectSingleNode("@skillcategory")?.InnerText;
                if (!string.IsNullOrEmpty(strOnlyCategory))
                {
                    setAllowedCategories = new HashSet<string>(strOnlyCategory.Split(',').Select(x => x.Trim()));
                }
                else
                {
                    using (XmlNodeList xmlCategoryList = xmlBonusNode.SelectNodes("skillcategories/category"))
                    {
                        if (xmlCategoryList?.Count > 0)
                        {
                            setAllowedCategories = new HashSet<string>();
                            foreach (XmlNode objNode in xmlCategoryList)
                            {
                                setAllowedCategories.Add(objNode.InnerText);
                            }
                        }
                    }
                }

                HashSet<string> setForbiddenCategories = null;
                string strExcludeCategory = xmlBonusNode.SelectSingleNode("@excludecategory")?.InnerText;
                if (!string.IsNullOrEmpty(strExcludeCategory))
                {
                    setForbiddenCategories = new HashSet<string>(strExcludeCategory.Split(',').Select(x => x.Trim()));
                }
                HashSet<string> setAllowedNames = null;
                if (!string.IsNullOrEmpty(ForcedValue))
                {
                    setAllowedNames = new HashSet<string> { ForcedValue };
                }
                else if (!string.IsNullOrEmpty(strPrompt))
                {
                    setAllowedNames = new HashSet<string> { strPrompt };
                }
                else
                {
                    string strLimitToSkill = xmlBonusNode.SelectSingleNode("@limittoskill")?.InnerText;
                    if (!string.IsNullOrEmpty(strLimitToSkill))
                    {
                        setAllowedNames = new HashSet<string>(strLimitToSkill.Split(',').Select(x => x.Trim()));
                    }
                }

                HashSet<string> setAllowedLinkedAttributes = null;
                string strLimitToAttribute = xmlBonusNode.SelectSingleNode("@limittoattribute")?.InnerText;
                if (!string.IsNullOrEmpty(strLimitToAttribute))
                {
                    setAllowedLinkedAttributes = new HashSet<string>(strLimitToAttribute.Split(',').Select(x => x.Trim()));
                }

                List<ListItem> lstDropdownItems = new List<ListItem>();
                HashSet<string> setProcessedSkillNames = new HashSet<string>();
                foreach (KnowledgeSkill objKnowledgeSkill in objCharacter.SkillsSection.KnowledgeSkills)
                {
                    if (setAllowedCategories?.Contains(objKnowledgeSkill.SkillCategory) != false &&
                        setForbiddenCategories?.Contains(objKnowledgeSkill.SkillCategory) != true &&
                        setAllowedNames?.Contains(objKnowledgeSkill.Name) != false &&
                        setAllowedLinkedAttributes?.Contains(objKnowledgeSkill.Attribute) != false)
                    {
                        int intSkillRating = objKnowledgeSkill.Rating;
                        if (intSkillRating >= intMinimumRating && intRating < intMaximumRating)
                        {
                            lstDropdownItems.Add(new ListItem(objKnowledgeSkill.Name, objKnowledgeSkill.CurrentDisplayName));
                        }
                    }
                    setProcessedSkillNames.Add(objKnowledgeSkill.Name);
                }

                if (!string.IsNullOrEmpty(strPrompt) && !setProcessedSkillNames.Contains(strPrompt))
                {
                    lstDropdownItems.Add(new ListItem(strPrompt, objCharacter.TranslateExtra(strPrompt)));
                    setProcessedSkillNames.Add(strPrompt);
                }
                if (intMinimumRating <= 0)
                {
                    StringBuilder objFilter = new StringBuilder();
                    if (setAllowedCategories?.Count > 0)
                    {
                        objFilter.Append('(');
                        foreach (string strCategory in setAllowedCategories)
                        {
                            objFilter.Append("category = \"" + strCategory + "\" or ");
                        }

                        objFilter.Length -= 4;
                        objFilter.Append(')');
                    }
                    if (setForbiddenCategories?.Count > 0)
                    {
                        if (objFilter.Length > 0)
                            objFilter.Append(" and ");
                        objFilter.Append("not(");
                        foreach (string strCategory in setForbiddenCategories)
                        {
                            objFilter.Append("category = \"" + strCategory + "\" or ");
                        }

                        objFilter.Length -= 4;
                        objFilter.Append(')');
                    }
                    if (setAllowedNames?.Count > 0)
                    {
                        if (objFilter.Length > 0)
                            objFilter.Append(" and ");
                        objFilter.Append('(');
                        foreach (string strName in setAllowedNames)
                        {
                            objFilter.Append("name = \"" + strName + "\" or ");
                        }

                        objFilter.Length -= 4;
                        objFilter.Append(')');
                    }
                    if (setProcessedSkillNames.Count > 0)
                    {
                        if (objFilter.Length > 0)
                            objFilter.Append(" and ");
                        objFilter.Append("not(");
                        foreach (string strName in setProcessedSkillNames)
                        {
                            objFilter.Append("name = \"" + strName + "\" or ");
                        }

                        objFilter.Length -= 4;
                        objFilter.Append(')');
                    }
                    if (setAllowedLinkedAttributes?.Count > 0)
                    {
                        if (objFilter.Length > 0)
                            objFilter.Append(" and ");
                        objFilter.Append('(');
                        foreach (string strAttribute in setAllowedLinkedAttributes)
                        {
                            objFilter.Append("attribute = \"" + strAttribute + "\" or ");
                        }

                        objFilter.Length -= 4;
                        objFilter.Append(')');
                    }

                    string strFilter = objFilter.Length > 0 ? ") and (" + objFilter.ToString() : string.Empty;
                    using (XmlNodeList xmlSkillList = objCharacter.LoadData("skills.xml")
                        .SelectNodes("/chummer/knowledgeskills/skill[(not(hide)" + strFilter + ")]"))
                    {
                        if (xmlSkillList?.Count > 0)
                        {
                            foreach (XmlNode xmlSkill in xmlSkillList)
                            {
                                string strName = xmlSkill["name"]?.InnerText;
                                if (!string.IsNullOrEmpty(strName))
                                    lstDropdownItems.Add(new ListItem(strName, xmlSkill["translate"]?.InnerText ?? strName));
                            }
                        }
                    }
                }

                lstDropdownItems.Sort(CompareListItems.CompareNames);

                using (frmSelectItem frmPickSkill = new frmSelectItem
                {
                    Description = LanguageManager.GetString("Title_SelectSkill"),
                    AllowAutoSelect = string.IsNullOrWhiteSpace(strPrompt)
                })
                {
                    if (setAllowedNames != null && string.IsNullOrWhiteSpace(strPrompt))
                        frmPickSkill.SetGeneralItemsMode(lstDropdownItems);
                    else
                        frmPickSkill.SetDropdownItemsMode(lstDropdownItems);

                    frmPickSkill.ShowDialog(Program.MainForm);

                    if (frmPickSkill.DialogResult == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    strSelectedSkill = frmPickSkill.SelectedItem;
                }
            }
            else
            {
                // Display the Select Skill window and record which Skill was selected.
                using (frmSelectSkill frmPickSkill = new frmSelectSkill(objCharacter, strFriendlyName)
                {
                    Description = !string.IsNullOrEmpty(strFriendlyName)
                        ? string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("String_Improvement_SelectSkillNamed"), strFriendlyName)
                        : LanguageManager.GetString("String_Improvement_SelectSkill")
                })
                {
                    string strMinimumRating = xmlBonusNode.Attributes?["minimumrating"]?.InnerText;
                    if (!string.IsNullOrWhiteSpace(strMinimumRating))
                        frmPickSkill.MinimumRating = ValueToInt(objCharacter, strMinimumRating, intRating);
                    string strMaximumRating = xmlBonusNode.Attributes?["maximumrating"]?.InnerText;
                    if (!string.IsNullOrWhiteSpace(strMaximumRating))
                        frmPickSkill.MaximumRating = ValueToInt(objCharacter, strMaximumRating, intRating);

                    XmlNode xmlSkillCategories = xmlBonusNode.SelectSingleNode("skillcategories");
                    if (xmlSkillCategories != null)
                        frmPickSkill.LimitToCategories = xmlSkillCategories;
                    string strTemp = xmlBonusNode.SelectSingleNode("@skillcategory")?.InnerText;
                    if (!string.IsNullOrEmpty(strTemp))
                        frmPickSkill.OnlyCategory = strTemp;
                    strTemp = xmlBonusNode.SelectSingleNode("@skillgroup")?.InnerText;
                    if (!string.IsNullOrEmpty(strTemp))
                        frmPickSkill.OnlySkillGroup = strTemp;
                    strTemp = xmlBonusNode.SelectSingleNode("@excludecategory")?.InnerText;
                    if (!string.IsNullOrEmpty(strTemp))
                        frmPickSkill.ExcludeCategory = strTemp;
                    strTemp = xmlBonusNode.SelectSingleNode("@excludeskillgroup")?.InnerText;
                    if (!string.IsNullOrEmpty(strTemp))
                        frmPickSkill.ExcludeSkillGroup = strTemp;
                    strTemp = xmlBonusNode.SelectSingleNode("@limittoskill")?.InnerText;
                    if (!string.IsNullOrEmpty(strTemp))
                        frmPickSkill.LimitToSkill = strTemp;
                    strTemp = xmlBonusNode.SelectSingleNode("@excludeskill")?.InnerText;
                    if (!string.IsNullOrEmpty(strTemp))
                        frmPickSkill.ExcludeSkill = strTemp;
                    strTemp = xmlBonusNode.SelectSingleNode("@limittoattribute")?.InnerText;
                    if (!string.IsNullOrEmpty(strTemp))
                        frmPickSkill.LinkedAttribute = strTemp;

                    if (!string.IsNullOrEmpty(ForcedValue))
                    {
                        frmPickSkill.OnlySkill = ForcedValue;
                        frmPickSkill.Opacity = 0;
                    }

                    frmPickSkill.ShowDialog(Program.MainForm);

                    // Make sure the dialogue window was not canceled.
                    if (frmPickSkill.DialogResult == DialogResult.Cancel)
                    {
                        throw new AbortedException();
                    }

                    strSelectedSkill = frmPickSkill.SelectedSkill;
                }
            }

            return strSelectedSkill;
        }
        #endregion

        #region Improvement System

        /// <summary>
        /// Create all of the Improvements for an XML Node.
        /// </summary>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="objImprovementSource">Type of object that grants these Improvements.</param>
        /// <param name="strSourceName">Name of the item that grants these Improvements.</param>
        /// <param name="nodBonus">bonus XML Node from the source data file.</param>
        /// <param name="intRating">Selected Rating value that is used to replace the Rating string in an Improvement.</param>
        /// <param name="strFriendlyName">Friendly name to show in any dialogue windows that ask for a value.</param>
        /// <param name="blnAddImprovementsToCharacter">If True, adds created improvements to the character. Set to false if all we need is a SelectedValue.</param>
        /// 
        /// <returns>True if successful</returns>
        public static bool CreateImprovements(Character objCharacter, Improvement.ImprovementSource objImprovementSource, string strSourceName,
            XmlNode nodBonus, int intRating = 1, string strFriendlyName = "", bool blnAddImprovementsToCharacter = true)
        {
            Log.Debug("CreateImprovements enter");
            Log.Info("objImprovementSource = " + objImprovementSource.ToString());
            Log.Info("strSourceName = " + strSourceName);
            Log.Info("nodBonus = " + nodBonus?.OuterXml);
            Log.Info("intRating = " + intRating.ToString(GlobalOptions.InvariantCultureInfo));
            Log.Info("strFriendlyName = " + strFriendlyName);

            /*try
            {*/
            if (nodBonus == null)
            {
                s_StrForcedValue = string.Empty;
                s_StrLimitSelection = string.Empty;
                Log.Debug("CreateImprovements exit");
                return true;
            }

            s_StrSelectedValue = string.Empty;

            Log.Info("_strForcedValue = " + s_StrForcedValue);
            Log.Info("_strLimitSelection = " + s_StrLimitSelection);

            string strUnique = nodBonus.Attributes?["unique"]?.InnerText ?? string.Empty;
            // If no friendly name was provided, use the one from SourceName.
            if (string.IsNullOrEmpty(strFriendlyName))
                strFriendlyName = strSourceName;

            if (nodBonus.HasChildNodes)
            {
                Log.Info("Has Child Nodes");
                if (nodBonus["selecttext"] != null)
                {
                    Log.Info("selecttext");

                    if (!string.IsNullOrEmpty(s_StrForcedValue))
                    {
                        LimitSelection = s_StrForcedValue;
                    }
                    else if (objCharacter?.Pushtext.Count != 0)
                    {
                        LimitSelection = objCharacter?.Pushtext.Pop();
                    }

                    Log.Info("_strForcedValue = " + SelectedValue);
                    Log.Info("_strLimitSelection = " + LimitSelection);

                    if (!string.IsNullOrEmpty(LimitSelection))
                    {
                        s_StrSelectedValue = LimitSelection;
                    }
                    else if (nodBonus["selecttext"].Attributes.Count == 0)
                    {
                        // Display the Select Text window and record the value that was entered.
                        using (frmSelectText frmPickText = new frmSelectText
                        {
                            Description =
                                string.Format(GlobalOptions.CultureInfo,
                                    LanguageManager.GetString("String_Improvement_SelectText"),
                                    strFriendlyName)
                        })
                        {
                            frmPickText.ShowDialog(Program.MainForm);

                            // Make sure the dialogue window was not canceled.
                            if (frmPickText.DialogResult == DialogResult.Cancel)
                            {
                                Rollback(objCharacter);
                                ForcedValue = string.Empty;
                                LimitSelection = string.Empty;
                                Log.Debug("CreateImprovements exit");
                                return false;
                            }

                            s_StrSelectedValue = frmPickText.SelectedValue;
                        }
                    }
                    else
                    {
                        using (frmSelectItem frmSelect = new frmSelectItem
                        {
                            Description = string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("String_Improvement_SelectText"), strFriendlyName)
                        })
                        {
                            string strXPath = nodBonus["selecttext"].Attributes["xpath"]?.InnerText;
                            if (string.IsNullOrEmpty(strXPath))
                            {
                                Rollback(objCharacter);
                                ForcedValue = string.Empty;
                                LimitSelection = string.Empty;
                                Log.Debug("CreateImprovements exit");
                                return false;
                            }
                            XPathNavigator xmlDoc = objCharacter.LoadData(nodBonus["selecttext"].Attributes["xml"]?.InnerText)
                                .GetFastNavigator();
                            List<ListItem> lstItems = new List<ListItem>();
                            foreach (XPathNavigator objNode in xmlDoc.Select(strXPath))
                            {
                                string strName = objNode.SelectSingleNode("name")?.Value ?? string.Empty;
                                if (string.IsNullOrWhiteSpace(strName))
                                {
                                    // Assume that if we're not looking at something that has an XML node,
                                    // we're looking at a direct xpath filter or something that has proper names
                                    // like the lifemodule storybuilder macros.
                                    lstItems.Add(new ListItem(objNode.Value, objNode.Value));
                                }
                                else
                                {
                                    lstItems.Add(new ListItem(strName,
                                        objNode.SelectSingleNode("translate")?.Value ?? strName));
                                }
                            }
                            //TODO: While this is a safeguard for uniques, preference should be that we're selecting distinct values in the xpath.
                            //Use XPath2.0 distinct-values operators instead. REQUIRES > .Net 4.6
                            lstItems = new List<ListItem>(lstItems.GroupBy(o => new { o.Value, o.Name })
                                .Select(o => o.FirstOrDefault()));

                            if (lstItems.Count == 0)
                            {
                                Rollback(objCharacter);
                                ForcedValue = string.Empty;
                                LimitSelection = string.Empty;
                                Log.Debug("CreateImprovements exit");
                                return false;
                            }

                            if (Convert.ToBoolean(nodBonus["selecttext"].Attributes["allowedit"]?.InnerText, GlobalOptions.InvariantCultureInfo))
                            {
                                frmSelect.SetDropdownItemsMode(lstItems);
                            }
                            else
                            {
                                frmSelect.SetGeneralItemsMode(lstItems);
                            }

                            frmSelect.ShowDialog(Program.MainForm);

                            if (frmSelect.DialogResult == DialogResult.Cancel)
                            {
                                Rollback(objCharacter);
                                ForcedValue = string.Empty;
                                LimitSelection = string.Empty;
                                Log.Debug("CreateImprovements exit");
                                return false;
                            }
                            s_StrSelectedValue = frmSelect.SelectedItem;
                        }
                    }
                    Log.Info("_strSelectedValue = " + SelectedValue);
                    Log.Info("strSourceName = " + strSourceName);

                    // Create the Improvement.
                    Log.Info("Calling CreateImprovement");

                    CreateImprovement(objCharacter, s_StrSelectedValue, objImprovementSource, strSourceName,
                        Improvement.ImprovementType.Text,
                        strUnique);
                }

                // If there is no character object, don't attempt to add any Improvements.
                if (objCharacter == null && blnAddImprovementsToCharacter)
                {
                    Log.Info("_objCharacter = Null");
                    Log.Debug("CreateImprovements exit");
                    return true;
                }

                // Check to see what bonuses the node grants.
                foreach (XmlNode bonusNode in nodBonus.ChildNodes)
                {
                    if (!ProcessBonus(objCharacter, objImprovementSource, ref strSourceName, intRating, strFriendlyName,
                        bonusNode, strUnique, !blnAddImprovementsToCharacter))
                    {
                        Rollback(objCharacter);
                        return false;
                    }
                }
            }
            // If there is no character object, don't attempt to add any Improvements.
            else if (objCharacter == null && blnAddImprovementsToCharacter)
            {
                Log.Info("_objCharacter = Null");
                Log.Debug("CreateImprovements exit");
                return true;
            }


            // If we've made it this far, everything went OK, so commit the Improvements.

            if (blnAddImprovementsToCharacter)
            {
                Log.Info("Calling Commit");
                Commit(objCharacter);
                Log.Info("Returned from Commit");
            }
            else
            {
                Log.Info("Calling scheduled Rollback due to blnAddImprovementsToCharacter = false");
                Rollback(objCharacter);
                Log.Info("Returned from scheduled Rollback");
            }

            // If the bonus should not bubble up SelectedValues from its improvements, reset it to empty.
            if (nodBonus.Attributes?["useselected"]?.InnerText == bool.FalseString)
            {
                SelectedValue = string.Empty;
            }
            // Clear the Forced Value and Limit Selection strings once we're done to prevent these from forcing their values on other Improvements.
            s_StrForcedValue = string.Empty;
            s_StrLimitSelection = string.Empty;

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
            Log.Debug("CreateImprovements exit");
            return true;

        }

        private static bool ProcessBonus(Character objCharacter, Improvement.ImprovementSource objImprovementSource, ref string strSourceName,
            int intRating, string strFriendlyName, XmlNode bonusNode, string strUnique, bool blnIgnoreMethodNotFound = false)
        {
            if (bonusNode == null)
                return false;
            //As this became a really big nest of **** that it searched past, several places having equal paths just adding a different improvement, a more flexible method was chosen.
            //So far it is just a slower Dictionary<string, Action> but should (in theory...) be able to leverage this in the future to do it smarter with methods that are the same but
            //getting a different parameter injected

            AddImprovementCollection container = new AddImprovementCollection(objCharacter, objImprovementSource,
                strSourceName, strUnique, s_StrForcedValue, s_StrLimitSelection, SelectedValue, strFriendlyName,
                intRating);

            Action<XmlNode> objImprovementMethod = ImprovementMethods.GetMethod(bonusNode.Name.ToUpperInvariant(), container);
            if (objImprovementMethod != null)
            {
                try
                {
                    objImprovementMethod.Invoke(bonusNode);
                }
                catch (AbortedException)
                {
                    Rollback(objCharacter);
                    return false;
                }

                strSourceName = container.SourceName;
                s_StrForcedValue = container.ForcedValue;
                s_StrLimitSelection = container.LimitSelection;
                s_StrSelectedValue = container.SelectedValue;
            }
            else if (blnIgnoreMethodNotFound || bonusNode.ChildNodes.Count == 0)
            {
                return true;
            }
            else if (bonusNode.NodeType != XmlNodeType.Comment)
            {
                Utils.BreakIfDebug();
                Log.Warn(new object[] {"Tried to get unknown bonus", bonusNode.OuterXml});
                return false;
            }
            return true;
        }

        public static void EnableImprovements(Character objCharacter, ICollection<Improvement> objImprovementList)
        {
            if (objCharacter == null)
                throw new ArgumentNullException(nameof(objCharacter));
            if (objImprovementList == null)
                throw new ArgumentNullException(nameof(objImprovementList));
            foreach (Improvement objImprovement in objImprovementList)
            {
                // Enable the Improvement.
                objImprovement.Enabled = true;
            }

            bool blnCharacterHasSkillsoftAccess = ValueOf(objCharacter, Improvement.ImprovementType.SkillsoftAccess) > 0;
            // Now that the entire list is deleted from the character's improvements list, we do the checking of duplicates and extra effects
            foreach (Improvement objImprovement in objImprovementList)
            {
                switch (objImprovement.ImproveType)
                {
                    case Improvement.ImprovementType.SkillLevel:
                        //TODO: Come back here and figure out wtf this did? Think it removed nested lifemodule skills? //Didn't this handle the collapsing knowledge skills thing?
                        //for (int i = _objCharacter.SkillsSection.Skills.Count - 1; i >= 0; i--)
                        //{
                        //    //wrote as foreach first, modify collection, not want rename
                        //    Skill skill = _objCharacter.SkillsSection.Skills[i];
                        //    for (int j = skill.Fold.Count - 1; j >= 0; j--)
                        //    {
                        //        Skill fold = skill.Fold[i];
                        //        if (fold.Id.ToString() == objImprovement.ImprovedName)
                        //        {
                        //            skill.Free(fold);
                        //            _objCharacter.SkillsSection.Skills.Remove(fold);
                        //        }
                        //    }

                        //    if (skill.Id.ToString() == objImprovement.ImprovedName)
                        //    {
                        //        while(skill.Fold.Count > 0) skill.Free(skill.Fold[0]);
                        //        //empty list, can't call clear as exposed list is RO

                        //        _objCharacter.SkillsSection.Skills.Remove(skill);
                        //    }
                        //}
                        break;
                    case Improvement.ImprovementType.SkillsoftAccess:
                        foreach (KnowledgeSkill objKnowledgeSkill in objCharacter.SkillsSection.KnowsoftSkills)
                        {
                            if (!objCharacter.SkillsSection.KnowledgeSkills.Contains(objKnowledgeSkill))
                                objCharacter.SkillsSection.KnowledgeSkills.Add(objKnowledgeSkill);
                        }
                        break;
                    case Improvement.ImprovementType.Skillsoft:
                        {
                            foreach (KnowledgeSkill objKnowledgeSkill in objCharacter.SkillsSection.KnowsoftSkills.Where(x => x.InternalId == objImprovement.ImprovedName).ToList())
                            {
                                if (blnCharacterHasSkillsoftAccess && !objCharacter.SkillsSection.KnowledgeSkills.Contains(objKnowledgeSkill))
                                    objCharacter.SkillsSection.KnowledgeSkills.Add(objKnowledgeSkill);
                            }
                        }
                        break;
                    case Improvement.ImprovementType.Attribute:
                        // Determine if access to any Special Attributes have been lost.
                        if (objImprovement.UniqueName == "enableattribute")
                        {
                            switch (objImprovement.ImprovedName)
                            {
                                case "MAG":
                                    objCharacter.MAGEnabled = true;
                                    break;
                                case "RES":
                                    objCharacter.RESEnabled = true;
                                    break;
                                case "DEP":
                                    objCharacter.DEPEnabled = true;
                                    break;
                            }
                        }
                        break;
                    case Improvement.ImprovementType.SpecialTab:
                        // Determine if access to any special tabs have been lost.
                        if (objImprovement.UniqueName == "enabletab")
                        {
                            switch (objImprovement.ImprovedName)
                            {
                                case "Magician":
                                    objCharacter.MagicianEnabled = true;
                                    break;
                                case "Adept":
                                    objCharacter.AdeptEnabled = true;
                                    break;
                                case "Technomancer":
                                    objCharacter.TechnomancerEnabled = true;
                                    break;
                                case "Advanced Programs":
                                    objCharacter.AdvancedProgramsEnabled = true;
                                    break;
                                case "Critter":
                                    objCharacter.CritterEnabled = true;
                                    break;
                            }
                        }
                        // Determine if access to any special tabs has been regained
                        else if (objImprovement.UniqueName == "disabletab")
                        {
                            switch (objImprovement.ImprovedName)
                            {
                                case "Cyberware":
                                    objCharacter.CyberwareDisabled = true;
                                    break;
                                case "Initiation":
                                    objCharacter.InitiationForceDisabled = true;
                                    break;
                            }
                        }
                        break;
                    case Improvement.ImprovementType.PrototypeTranshuman:
                        string strImprovedName = objImprovement.ImprovedName;
                        // Legacy compatibility
                        if (string.IsNullOrEmpty(strImprovedName))
                            objCharacter.PrototypeTranshuman = 1;
                        else
                            objCharacter.PrototypeTranshuman += Convert.ToDecimal(strImprovedName, GlobalOptions.InvariantCultureInfo);
                        break;
                    case Improvement.ImprovementType.Adapsin:
                        break;
                    case Improvement.ImprovementType.AddContact:
                        Contact NewContact = objCharacter.Contacts.FirstOrDefault(c => c.UniqueId == objImprovement.ImprovedName);
                        if (NewContact != null)
                        {
                            // TODO: Add code to enable disabled contact
                        }
                        break;
                    case Improvement.ImprovementType.Initiation:
                        objCharacter.InitiateGrade += objImprovement.Value;
                        break;
                    case Improvement.ImprovementType.Submersion:
                        objCharacter.SubmersionGrade += objImprovement.Value;
                        break;
                    case Improvement.ImprovementType.Art:
                        Art objArt = objCharacter.Arts.FirstOrDefault(x => x.InternalId == objImprovement.ImprovedName);
                        if (objArt != null)
                        {
                            EnableImprovements(objCharacter, objCharacter.Improvements.Where(x => x.ImproveSource == objArt.SourceType && x.SourceName == objArt.InternalId && x.Enabled).ToList());
                        }
                        break;
                    case Improvement.ImprovementType.Metamagic:
                    case Improvement.ImprovementType.Echo:
                        Metamagic objMetamagic = objCharacter.Metamagics.FirstOrDefault(x => x.InternalId == objImprovement.ImprovedName);
                        if (objMetamagic != null)
                        {
                            Improvement.ImprovementSource eSource = objImprovement.ImproveType == Improvement.ImprovementType.Metamagic ? Improvement.ImprovementSource.Metamagic : Improvement.ImprovementSource.Echo;
                            EnableImprovements(objCharacter, objCharacter.Improvements.Where(x => x.ImproveSource == eSource && x.SourceName == objMetamagic.InternalId && x.Enabled).ToList());
                        }
                        break;
                    case Improvement.ImprovementType.CritterPower:
                        CritterPower objCritterPower = objCharacter.CritterPowers.FirstOrDefault(x => x.InternalId == objImprovement.ImprovedName || (x.Name == objImprovement.ImprovedName && x.Extra == objImprovement.UniqueName));
                        if (objCritterPower != null)
                        {
                            EnableImprovements(objCharacter, objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.CritterPower && x.SourceName == objCritterPower.InternalId && x.Enabled).ToList());
                        }
                        break;
                    case Improvement.ImprovementType.MentorSpirit:
                    case Improvement.ImprovementType.Paragon:
                        MentorSpirit objMentor = objCharacter.MentorSpirits.FirstOrDefault(x => x.InternalId == objImprovement.ImprovedName);
                        if (objMentor != null)
                        {
                            EnableImprovements(objCharacter, objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.MentorSpirit && x.SourceName == objMentor.InternalId && x.Enabled).ToList());
                        }
                        break;
                    case Improvement.ImprovementType.Gear:
                        Gear objGear = objCharacter.Gear.FirstOrDefault(x => x.InternalId == objImprovement.ImprovedName);
                        objGear?.ChangeEquippedStatus(true);
                        break;
                    case Improvement.ImprovementType.Weapon:
                        // TODO: Re-equip Weapons;
                        break;
                    case Improvement.ImprovementType.Spell:
                        Spell objSpell = objCharacter.Spells.FirstOrDefault(x => x.InternalId == objImprovement.ImprovedName);
                        if (objSpell != null)
                        {
                            EnableImprovements(objCharacter, objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.Spell && x.SourceName == objSpell.InternalId && x.Enabled).ToList());
                        }
                        break;
                    case Improvement.ImprovementType.ComplexForm:
                        ComplexForm objComplexForm = objCharacter.ComplexForms.FirstOrDefault(x => x.InternalId == objImprovement.ImprovedName);
                        if (objComplexForm != null)
                        {
                            EnableImprovements(objCharacter, objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.ComplexForm && x.SourceName == objComplexForm.InternalId && x.Enabled).ToList());
                        }
                        break;
                    case Improvement.ImprovementType.MartialArt:
                        MartialArt objMartialArt = objCharacter.MartialArts.FirstOrDefault(x => x.InternalId == objImprovement.ImprovedName);
                        if (objMartialArt != null)
                        {
                            EnableImprovements(objCharacter, objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.MartialArt && x.SourceName == objMartialArt.InternalId && x.Enabled).ToList());
                            // Remove the Improvements for any Advantages for the Martial Art that is being removed.
                            foreach (MartialArtTechnique objTechnique in objMartialArt.Techniques)
                            {
                                EnableImprovements(objCharacter, objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.MartialArtTechnique && x.SourceName == objTechnique.InternalId && x.Enabled).ToList());
                            }
                        }
                        break;
                    case Improvement.ImprovementType.SpecialSkills:
                        {
                            string strCategory;
                            switch ((FilterOption)Enum.Parse(typeof(FilterOption), objImprovement.ImprovedName))
                            {
                                case FilterOption.Magician:
                                case FilterOption.Sorcery:
                                case FilterOption.Conjuring:
                                case FilterOption.Enchanting:
                                case FilterOption.Adept:
                                    strCategory = "Magical Active";
                                    break;
                                case FilterOption.Technomancer:
                                    strCategory = "Resonance Active";
                                    break;
                                default:
                                    continue;
                            }

                            foreach (Skill objSkill in objCharacter.SkillsSection.Skills.Where(x => x.SkillCategory == strCategory).ToList())
                            {
                                objSkill.ForceDisabled = false;
                            }
                        }
                        break;
                    case Improvement.ImprovementType.SpecificQuality:
                        Quality objQuality = objCharacter.Qualities.FirstOrDefault(objLoopQuality => objLoopQuality.InternalId == objImprovement.ImprovedName);
                        if (objQuality != null)
                        {
                            EnableImprovements(objCharacter, objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.Quality && x.SourceName == objQuality.InternalId && x.Enabled).ToList());
                        }
                        break;
                    /*
                    case Improvement.ImprovementType.SkillSpecialization:
                        {
                            Skill objSkill = objCharacter.SkillsSection.GetActiveSkill(objImprovement.ImprovedName);
                            SkillSpecialization objSkillSpec = objSkill?.Specializations.FirstOrDefault(x => x.Name == objImprovement.UniqueName);
                            //if (objSkillSpec != null)
                            // TODO: Add temporarily remove skill specialization
                        }
                        break;
                        */
                    case Improvement.ImprovementType.AIProgram:
                        AIProgram objProgram = objCharacter.AIPrograms.FirstOrDefault(objLoopProgram => objLoopProgram.InternalId == objImprovement.ImprovedName);
                        if (objProgram != null)
                        {
                            DisableImprovements(objCharacter, objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.AIProgram && x.SourceName == objProgram.InternalId && x.Enabled).ToList());
                        }
                        break;
                    case Improvement.ImprovementType.FreeWare:
                        {
                            Cyberware objCyberware = objCharacter.Cyberware.FirstOrDefault(o => o.InternalId == objImprovement.ImprovedName);
                            objCyberware?.ChangeModularEquip(true);
                        }
                        break;
                }
            }

            objImprovementList.ProcessRelevantEvents();
        }

        public static void DisableImprovements(Character objCharacter, ICollection<Improvement> objImprovementList)
        {
            if (objCharacter == null)
                throw new ArgumentNullException(nameof(objCharacter));
            if (objImprovementList == null)
                throw new ArgumentNullException(nameof(objImprovementList));
            foreach (Improvement objImprovement in objImprovementList)
            {
                // Disable the Improvement.
                objImprovement.Enabled = false;
            }

            // Now that the entire list is deleted from the character's improvements list, we do the checking of duplicates and extra effects
            foreach (Improvement objImprovement in objImprovementList)
            {
                bool blnHasDuplicate = objCharacter.Improvements.Any(x => x.UniqueName == objImprovement.UniqueName && x.ImprovedName == objImprovement.ImprovedName && x.ImproveType == objImprovement.ImproveType && x.SourceName != objImprovement.SourceName && x.Enabled);

                switch (objImprovement.ImproveType)
                {
                    case Improvement.ImprovementType.SkillLevel:
                        //TODO: Come back here and figure out wtf this did? Think it removed nested lifemodule skills? //Didn't this handle the collapsing knowledge skills thing?
                        //for (int i = _objCharacter.SkillsSection.Skills.Count - 1; i >= 0; i--)
                        //{
                        //    //wrote as foreach first, modify collection, not want rename
                        //    Skill skill = _objCharacter.SkillsSection.Skills[i];
                        //    for (int j = skill.Fold.Count - 1; j >= 0; j--)
                        //    {
                        //        Skill fold = skill.Fold[i];
                        //        if (fold.Id.ToString() == objImprovement.ImprovedName)
                        //        {
                        //            skill.Free(fold);
                        //            _objCharacter.SkillsSection.Skills.Remove(fold);
                        //        }
                        //    }

                        //    if (skill.Id.ToString() == objImprovement.ImprovedName)
                        //    {
                        //        while(skill.Fold.Count > 0) skill.Free(skill.Fold[0]);
                        //        //empty list, can't call clear as exposed list is RO

                        //        _objCharacter.SkillsSection.Skills.Remove(skill);
                        //    }
                        //}
                        break;
                    case Improvement.ImprovementType.SkillsoftAccess:
                        if (!blnHasDuplicate)
                        {
                            foreach (KnowledgeSkill objKnowledgeSkill in objCharacter.SkillsSection.KnowsoftSkills)
                            {
                                objCharacter.SkillsSection.KnowledgeSkills.Remove(objKnowledgeSkill);
                            }
                        }
                        break;
                    case Improvement.ImprovementType.Skillsoft:
                        if (!blnHasDuplicate)
                        {
                            foreach (KnowledgeSkill objKnowledgeSkill in objCharacter.SkillsSection.KnowsoftSkills.Where(x => x.InternalId == objImprovement.ImprovedName).ToList())
                            {
                                objCharacter.SkillsSection.KnowledgeSkills.Remove(objKnowledgeSkill);
                            }
                        }
                        break;
                    case Improvement.ImprovementType.Attribute:
                        // Determine if access to any Special Attributes have been lost.
                        if (objImprovement.UniqueName == "enableattribute" && !blnHasDuplicate)
                        {
                            switch (objImprovement.ImprovedName)
                            {
                                case "MAG":
                                    objCharacter.MAGEnabled = false;
                                    break;
                                case "RES":
                                    objCharacter.RESEnabled = false;
                                    break;
                                case "DEP":
                                    objCharacter.DEPEnabled = false;
                                    break;
                            }
                        }
                        break;
                    case Improvement.ImprovementType.SpecialTab:
                        // Determine if access to any special tabs have been lost.
                        if (!blnHasDuplicate)
                        {
                            if (objImprovement.UniqueName == "enabletab")
                            {
                                switch (objImprovement.ImprovedName)
                                {
                                    case "Magician":
                                        objCharacter.MagicianEnabled = false;
                                        break;
                                    case "Adept":
                                        objCharacter.AdeptEnabled = false;
                                        break;
                                    case "Technomancer":
                                        objCharacter.TechnomancerEnabled = false;
                                        break;
                                    case "Advanced Programs":
                                        objCharacter.AdvancedProgramsEnabled = false;
                                        break;
                                    case "Critter":
                                        objCharacter.CritterEnabled = false;
                                        break;
                                }
                            }
                            // Determine if access to any special tabs has been regained
                            else if (objImprovement.UniqueName == "disabletab")
                            {
                                switch (objImprovement.ImprovedName)
                                {
                                    case "Cyberware":
                                        objCharacter.CyberwareDisabled = false;
                                        break;
                                    case "Initiation":
                                        objCharacter.InitiationForceDisabled = false;
                                        break;
                                }
                            }
                        }
                        break;
                    case Improvement.ImprovementType.PrototypeTranshuman:
                        string strImprovedName = objImprovement.ImprovedName;
                        // Legacy compatibility
                        if (string.IsNullOrEmpty(strImprovedName))
                        {
                            if (!blnHasDuplicate)
                                objCharacter.PrototypeTranshuman = 0;
                        }
                        else
                            objCharacter.PrototypeTranshuman -= Convert.ToDecimal(strImprovedName, GlobalOptions.InvariantCultureInfo);
                        break;
                    case Improvement.ImprovementType.Adapsin:
                        break;
                    case Improvement.ImprovementType.AddContact:
                        Contact NewContact = objCharacter.Contacts.FirstOrDefault(c => c.UniqueId == objImprovement.ImprovedName);
                        if (NewContact != null)
                        {
                            // TODO: Add code to disable contact
                        }
                        break;
                    case Improvement.ImprovementType.Initiation:
                        objCharacter.InitiateGrade -= objImprovement.Value;
                        break;
                    case Improvement.ImprovementType.Submersion:
                        objCharacter.SubmersionGrade -= objImprovement.Value;
                        break;
                    case Improvement.ImprovementType.Art:
                        Art objArt = objCharacter.Arts.FirstOrDefault(x => x.InternalId == objImprovement.ImprovedName);
                        if (objArt != null)
                        {
                            DisableImprovements(objCharacter, objCharacter.Improvements.Where(x => x.ImproveSource == objArt.SourceType && x.SourceName == objArt.InternalId && x.Enabled).ToList());
                        }
                        break;
                    case Improvement.ImprovementType.Metamagic:
                    case Improvement.ImprovementType.Echo:
                        Metamagic objMetamagic = objCharacter.Metamagics.FirstOrDefault(x => x.InternalId == objImprovement.ImprovedName);
                        if (objMetamagic != null)
                        {
                            Improvement.ImprovementSource eSource = objImprovement.ImproveType == Improvement.ImprovementType.Metamagic ? Improvement.ImprovementSource.Metamagic : Improvement.ImprovementSource.Echo;
                            DisableImprovements(objCharacter, objCharacter.Improvements.Where(x => x.ImproveSource == eSource && x.SourceName == objMetamagic.InternalId && x.Enabled).ToList());
                        }
                        break;
                    case Improvement.ImprovementType.CritterPower:
                        CritterPower objCritterPower = objCharacter.CritterPowers.FirstOrDefault(x => x.InternalId == objImprovement.ImprovedName || (x.Name == objImprovement.ImprovedName && x.Extra == objImprovement.UniqueName));
                        if (objCritterPower != null)
                        {
                            DisableImprovements(objCharacter, objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.CritterPower && x.SourceName == objCritterPower.InternalId && x.Enabled).ToList());
                        }
                        break;
                    case Improvement.ImprovementType.MentorSpirit:
                    case Improvement.ImprovementType.Paragon:
                        MentorSpirit objMentor = objCharacter.MentorSpirits.FirstOrDefault(x => x.InternalId == objImprovement.ImprovedName);
                        if (objMentor != null)
                        {
                            DisableImprovements(objCharacter, objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.MentorSpirit && x.SourceName == objMentor.InternalId && x.Enabled).ToList());
                        }
                        break;
                    case Improvement.ImprovementType.Gear:
                        Gear objGear = objCharacter.Gear.FirstOrDefault(x => x.InternalId == objImprovement.ImprovedName);
                        objGear?.ChangeEquippedStatus(false);
                        break;
                    case Improvement.ImprovementType.Weapon:
                        // TODO: Unequip Weapons;
                        break;
                    case Improvement.ImprovementType.Spell:
                        Spell objSpell = objCharacter.Spells.FirstOrDefault(x => x.InternalId == objImprovement.ImprovedName);
                        if (objSpell != null)
                        {
                            DisableImprovements(objCharacter, objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.Spell && x.SourceName == objSpell.InternalId && x.Enabled).ToList());
                        }
                        break;
                    case Improvement.ImprovementType.ComplexForm:
                        ComplexForm objComplexForm = objCharacter.ComplexForms.FirstOrDefault(x => x.InternalId == objImprovement.ImprovedName);
                        if (objComplexForm != null)
                        {
                            DisableImprovements(objCharacter, objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.ComplexForm && x.SourceName == objComplexForm.InternalId && x.Enabled).ToList());
                        }
                        break;
                    case Improvement.ImprovementType.MartialArt:
                        MartialArt objMartialArt = objCharacter.MartialArts.FirstOrDefault(x => x.InternalId == objImprovement.ImprovedName);
                        if (objMartialArt != null)
                        {
                            DisableImprovements(objCharacter, objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.MartialArt && x.SourceName == objMartialArt.InternalId && x.Enabled).ToList());
                            // Remove the Improvements for any Advantages for the Martial Art that is being removed.
                            foreach (MartialArtTechnique objTechnique in objMartialArt.Techniques)
                            {
                                DisableImprovements(objCharacter, objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.MartialArtTechnique && x.SourceName == objTechnique.InternalId && x.Enabled).ToList());
                            }
                        }
                        break;
                    case Improvement.ImprovementType.SpecialSkills:
                        if (!blnHasDuplicate)
                        {
                            string strCategory;
                            switch ((FilterOption)Enum.Parse(typeof(FilterOption), objImprovement.ImprovedName))
                            {
                                case FilterOption.Magician:
                                case FilterOption.Sorcery:
                                case FilterOption.Conjuring:
                                case FilterOption.Enchanting:
                                case FilterOption.Adept:
                                    strCategory = "Magical Active";
                                    break;
                                case FilterOption.Technomancer:
                                    strCategory = "Resonance Active";
                                    break;
                                default:
                                    continue;
                            }

                            string strLoopCategory = string.Empty;
                            foreach (Improvement objLoopImprovement in objCharacter.Improvements.Where(x => x.ImproveType == Improvement.ImprovementType.SpecialSkills && x.Enabled))
                            {
                                FilterOption eLoopFilter = (FilterOption)Enum.Parse(typeof(FilterOption), objLoopImprovement.ImprovedName);
                                switch (eLoopFilter)
                                {
                                    case FilterOption.Magician:
                                    case FilterOption.Sorcery:
                                    case FilterOption.Conjuring:
                                    case FilterOption.Enchanting:
                                    case FilterOption.Adept:
                                        strLoopCategory = "Magical Active";
                                        break;
                                    case FilterOption.Technomancer:
                                        strLoopCategory = "Resonance Active";
                                        break;
                                }
                                if (strLoopCategory == strCategory)
                                    break;
                            }
                            if (strLoopCategory == strCategory)
                                continue;

                            foreach (Skill objSkill in objCharacter.SkillsSection.Skills.Where(x => x.SkillCategory == strCategory).ToList())
                            {
                                objSkill.ForceDisabled = true;
                            }
                        }
                        break;
                    case Improvement.ImprovementType.SpecificQuality:
                        Quality objQuality = objCharacter.Qualities.FirstOrDefault(objLoopQuality => objLoopQuality.InternalId == objImprovement.ImprovedName);
                        if (objQuality != null)
                        {
                            DisableImprovements(objCharacter, objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.Quality && x.SourceName == objQuality.InternalId && x.Enabled).ToList());
                        }
                        break;
                    /*
                    case Improvement.ImprovementType.SkillSpecialization:
                        {
                            Skill objSkill = objCharacter.SkillsSection.GetActiveSkill(objImprovement.ImprovedName);
                            SkillSpecialization objSkillSpec = objSkill?.Specializations.FirstOrDefault(x => x.Name == objImprovement.UniqueName);
                            //if (objSkillSpec != null)
                                // TODO: Temporarily remove skill specialization
                        }
                        break;
                        */
                    case Improvement.ImprovementType.AIProgram:
                        AIProgram objProgram = objCharacter.AIPrograms.FirstOrDefault(objLoopProgram => objLoopProgram.InternalId == objImprovement.ImprovedName);
                        if (objProgram != null)
                        {
                            DisableImprovements(objCharacter, objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.AIProgram && x.SourceName == objProgram.InternalId && x.Enabled).ToList());
                        }
                        break;
                    case Improvement.ImprovementType.FreeWare:
                        {
                            Cyberware objCyberware = objCharacter.Cyberware.FirstOrDefault(o => o.InternalId == objImprovement.ImprovedName);
                            objCyberware?.ChangeModularEquip(false);
                        }
                        break;
                }
            }

            objImprovementList.ProcessRelevantEvents();
        }

        /// <summary>
        /// Remove all of the Improvements for an XML Node.
        /// </summary>
        /// <param name="objCharacter">Character from which improvements should be deleted.</param>
        /// <param name="objImprovementSource">Type of object that granted these Improvements.</param>
        /// <param name="strSourceName">Name of the item that granted these Improvements.</param>
        public static decimal RemoveImprovements(Character objCharacter, Improvement.ImprovementSource objImprovementSource, string strSourceName = "")
        {
            // If there is no character object, don't try to remove any Improvements.
            if (objCharacter == null)
            {
                return 0;
            }

            Log.Info("objImprovementSource = " + objImprovementSource.ToString());
            Log.Info("strSourceName = " + strSourceName);
            // A List of Improvements to hold all of the items that will eventually be deleted.
            List<Improvement> objImprovementList = string.IsNullOrEmpty(strSourceName)
                ? objCharacter.Improvements.Where(objImprovement => objImprovement.ImproveSource == objImprovementSource).ToList()
                : objCharacter.Improvements.Where(objImprovement => objImprovement.ImproveSource == objImprovementSource && objImprovement.SourceName == strSourceName).ToList();
            // Compatibility fix for when blnConcatSelectedValue was around
            if (strSourceName.IsGuid())
            {
                var space = LanguageManager.GetString("String_Space");
                objImprovementList.AddRange(objCharacter.Improvements.Where(objImprovement => objImprovement.ImproveSource == objImprovementSource &&
                    (objImprovement.SourceName.StartsWith(strSourceName + space, StringComparison.Ordinal) || objImprovement.SourceName.StartsWith(strSourceName + " ", StringComparison.Ordinal))));
            }
            return RemoveImprovements(objCharacter, objImprovementList);
        }

        /// <summary>
        /// Remove all of the Improvements for an XML Node.
        /// </summary>
        /// <param name="objCharacter">Character from which improvements should be deleted.</param>
        /// <param name="objImprovementList">List of improvements to delete.</param>
        /// <param name="blnReapplyImprovements">Whether we're reapplying Improvements.</param>
        /// <param name="blnAllowDuplicatesFromSameSource">If we ignore checking whether a potential duplicate improvement has the same SourceName</param>
        public static decimal RemoveImprovements(Character objCharacter, ICollection<Improvement> objImprovementList, bool blnReapplyImprovements = false, bool blnAllowDuplicatesFromSameSource = false)
        {
            Log.Debug("RemoveImprovements enter");

            // If there is no character object, don't try to remove any Improvements.
            if (objCharacter == null || objImprovementList == null)
            {
                Log.Debug("RemoveImprovements exit");
                return 0;
            }

            // Note: As attractive as it may be to replace objImprovementList with an IEnumerable, we need to iterate through it twice for performance reasons

            // Now that we have all of the applicable Improvements, remove them from the character.
            foreach (Improvement objImprovement in objImprovementList)
            {
                // Remove the Improvement.
                objCharacter.Improvements.Remove(objImprovement);
                ClearCachedValue(objCharacter, objImprovement.ImproveType, objImprovement.ImprovedName);
            }
            decimal decReturn = 0;
            // Now that the entire list is deleted from the character's improvements list, we do the checking of duplicates and extra effects
            foreach (Improvement objImprovement in objImprovementList)
            {
                // See if the character has anything else that is granting them the same bonus as this improvement
                bool blnHasDuplicate = objCharacter.Improvements.Any(x => x.UniqueName == objImprovement.UniqueName && x.ImprovedName == objImprovement.ImprovedName && x.ImproveType == objImprovement.ImproveType && (blnAllowDuplicatesFromSameSource || x.SourceName != objImprovement.SourceName));

                switch (objImprovement.ImproveType)
                {
                    case Improvement.ImprovementType.SkillLevel:
                    //TODO: Come back here and figure out wtf this did? Think it removed nested lifemodule skills? //Didn't this handle the collapsing knowledge skills thing?
                    //for (int i = _objCharacter.SkillsSection.Skills.Count - 1; i >= 0; i--)
                    //{
                    //    //wrote as foreach first, modify collection, not want rename
                    //    Skill skill = _objCharacter.SkillsSection.Skills[i];
                    //    for (int j = skill.Fold.Count - 1; j >= 0; j--)
                    //    {
                    //        Skill fold = skill.Fold[i];
                    //        if (fold.Id.ToString() == objImprovement.ImprovedName)
                    //        {
                    //            skill.Free(fold);
                    //            _objCharacter.SkillsSection.Skills.Remove(fold);
                    //        }
                    //    }

                    //    if (skill.Id.ToString() == objImprovement.ImprovedName)
                    //    {
                    //        while(skill.Fold.Count > 0) skill.Free(skill.Fold[0]);
                    //        //empty list, can't call clear as exposed list is RO

                    //        _objCharacter.SkillsSection.Skills.Remove(skill);
                    //    }
                    //}
                        break;
                    case Improvement.ImprovementType.SkillsoftAccess:
                        if (!blnHasDuplicate)
                        {
                            foreach (KnowledgeSkill objKnowledgeSkill in objCharacter.SkillsSection.KnowsoftSkills)
                            {
                                objCharacter.SkillsSection.KnowledgeSkills.Remove(objKnowledgeSkill);
                            }
                        }
                        break;
                    case Improvement.ImprovementType.Skillsoft:
                        if (!blnHasDuplicate)
                        {
                            foreach (KnowledgeSkill objKnowledgeSkill in objCharacter.SkillsSection.KnowledgeSkills.Where(x => x.InternalId == objImprovement.ImprovedName).ToList())
                            {
                                objCharacter.SkillsSection.KnowledgeSkills.Remove(objKnowledgeSkill);
                            }
                            for (int i = objCharacter.SkillsSection.KnowsoftSkills.Count - 1; i >= 0; --i)
                            {
                                KnowledgeSkill objSkill = objCharacter.SkillsSection.KnowsoftSkills[i];
                                if (objSkill.InternalId == objImprovement.ImprovedName)
                                {
                                    objCharacter.SkillsSection.KnowledgeSkills.Remove(objSkill);
                                    objSkill.UnbindSkill();
                                    objCharacter.SkillsSection.KnowsoftSkills.RemoveAt(i);
                                }
                            }
                        }
                        break;
                    case Improvement.ImprovementType.Attribute:
                        // Determine if access to any Special Attributes have been lost.
                        if (objImprovement.UniqueName == "enableattribute" && !blnHasDuplicate && !blnReapplyImprovements)
                        {
                            switch (objImprovement.ImprovedName)
                            {
                                case "MAG":
                                    objCharacter.MAGEnabled = false;
                                    break;
                                case "RES":
                                    objCharacter.RESEnabled = false;
                                    break;
                                case "DEP":
                                    objCharacter.DEPEnabled = false;
                                    break;
                            }
                        }
                        break;
                    case Improvement.ImprovementType.SpecialTab:
                        // Determine if access to any special tabs have been lost.
                        if (!blnHasDuplicate && !blnReapplyImprovements)
                        {
                            if (objImprovement.UniqueName == "enabletab")
                            {
                                switch (objImprovement.ImprovedName)
                                {
                                    case "Magician":
                                        objCharacter.MagicianEnabled = false;
                                        break;
                                    case "Adept":
                                        objCharacter.AdeptEnabled = false;
                                        break;
                                    case "Technomancer":
                                        objCharacter.TechnomancerEnabled = false;
                                        break;
                                    case "Advanced Programs":
                                        objCharacter.AdvancedProgramsEnabled = false;
                                        break;
                                    case "Critter":
                                        objCharacter.CritterEnabled = false;
                                        break;
                                }
                            }
                            // Determine if access to any special tabs has been regained
                            else if (objImprovement.UniqueName == "disabletab")
                            {
                                switch (objImprovement.ImprovedName)
                                {
                                    case "Cyberware":
                                        objCharacter.CyberwareDisabled = false;
                                        break;
                                    case "Initiation":
                                        objCharacter.InitiationForceDisabled = false;
                                        break;
                                }
                            }
                        }
                        break;
                    case Improvement.ImprovementType.PrototypeTranshuman:
                        string strImprovedName = objImprovement.ImprovedName;
                        // Legacy compatibility
                        if (string.IsNullOrEmpty(strImprovedName))
                        {
                            if (!blnHasDuplicate)
                                objCharacter.PrototypeTranshuman = 0;
                        }
                        else
                        {
                            objCharacter.PrototypeTranshuman -= Convert.ToDecimal(strImprovedName, GlobalOptions.InvariantCultureInfo);

                            if (objCharacter.PrototypeTranshuman <= 0 && !blnReapplyImprovements)
                            {
                                foreach (Cyberware objCyberware in objCharacter.Cyberware)
                                    if (objCyberware.PrototypeTranshuman)
                                        objCyberware.PrototypeTranshuman = false;
                            }
                        }
                        break;
                    case Improvement.ImprovementType.Adapsin:
                        {
                            if (!blnHasDuplicate && !blnReapplyImprovements)
                            {
                                foreach (Cyberware objCyberware in objCharacter.Cyberware.DeepWhere(x => x.Children, x => x.Grade.Adapsin))
                                {
                                    string strNewName = objCyberware.Grade.Name.FastEscapeOnceFromEnd("(Adapsin)").Trim();
                                    // Determine which GradeList to use for the Cyberware.
                                    objCyberware.Grade = objCharacter.GetGradeList(objCyberware.SourceType, true).FirstOrDefault(x => x.Name == strNewName);
                                }
                            }
                        }
                        break;
                    case Improvement.ImprovementType.AddContact:
                        Contact NewContact = objCharacter.Contacts.FirstOrDefault(c => c.UniqueId == objImprovement.ImprovedName);
                        if (NewContact != null)
                            objCharacter.Contacts.Remove(NewContact);
                        break;
                    case Improvement.ImprovementType.Initiation:
                        objCharacter.InitiateGrade -= objImprovement.Value;
                        break;
                    case Improvement.ImprovementType.Submersion:
                        objCharacter.SubmersionGrade -= objImprovement.Value;
                        break;
                    case Improvement.ImprovementType.Art:
                        Art objArt = objCharacter.Arts.FirstOrDefault(x => x.InternalId == objImprovement.ImprovedName);
                        if (objArt != null)
                        {
                            decReturn += RemoveImprovements(objCharacter, objArt.SourceType, objArt.InternalId);
                            objCharacter.Arts.Remove(objArt);
                        }
                        break;
                    case Improvement.ImprovementType.Metamagic:
                    case Improvement.ImprovementType.Echo:
                        Metamagic objMetamagic = objCharacter.Metamagics.FirstOrDefault(x => x.InternalId == objImprovement.ImprovedName);
                        if (objMetamagic != null)
                        {
                            decReturn += RemoveImprovements(objCharacter, objImprovement.ImproveType == Improvement.ImprovementType.Metamagic ? Improvement.ImprovementSource.Metamagic : Improvement.ImprovementSource.Echo, objMetamagic.InternalId);
                            objCharacter.Metamagics.Remove(objMetamagic);
                        }
                        break;
                    case Improvement.ImprovementType.LimitModifier:
                        LimitModifier limitMod = objCharacter.LimitModifiers.FirstOrDefault(x => x.InternalId == objImprovement.ImprovedName);
                        if (limitMod != null)
                        {
                            objCharacter.LimitModifiers.Remove(limitMod);
                        }
                        break;
                    case Improvement.ImprovementType.CritterPower:
                        CritterPower objCritterPower = objCharacter.CritterPowers.FirstOrDefault(x => x.InternalId == objImprovement.ImprovedName || ( x.Name == objImprovement.ImprovedName && x.Extra == objImprovement.UniqueName));
                        if (objCritterPower != null)
                        {
                            decReturn += RemoveImprovements(objCharacter, Improvement.ImprovementSource.CritterPower, objCritterPower.InternalId);
                            objCharacter.CritterPowers.Remove(objCritterPower);
                        }
                        break;
                    case Improvement.ImprovementType.MentorSpirit:
                    case Improvement.ImprovementType.Paragon:
                        MentorSpirit objMentor = objCharacter.MentorSpirits.FirstOrDefault(x => x.InternalId == objImprovement.ImprovedName);
                        if (objMentor != null)
                        {
                            decReturn += RemoveImprovements(objCharacter, Improvement.ImprovementSource.MentorSpirit, objMentor.InternalId);
                            objCharacter.MentorSpirits.Remove(objMentor);
                        }
                        break;
                    case Improvement.ImprovementType.Gear:
                        Gear objGear = objCharacter.Gear.FirstOrDefault(x => x.InternalId == objImprovement.ImprovedName);
                        if (objGear != null)
                        {
                            decReturn += objGear.DeleteGear();
                            decReturn += objGear.TotalCost;
                            objCharacter.Gear.Remove(objGear);
                        }
                        break;
                    case Improvement.ImprovementType.Weapon:
                    {
                        Vehicle objVehicle = null;
                        WeaponMount objWeaponMount = null;
                        VehicleMod objVehicleMod = null;
                        Weapon objWeapon = objCharacter.Weapons.DeepFirstOrDefault(x => x.Children, x => x.InternalId == objImprovement.ImprovedName) ??
                                           objCharacter.Vehicles.FindVehicleWeapon(objImprovement.ImprovedName, out objVehicle, out objWeaponMount, out objVehicleMod);
                        if (objWeapon != null)
                        {
                            decReturn += objWeapon.DeleteWeapon();
                            decReturn += objWeapon.TotalCost;
                            Weapon objParent = objWeapon.Parent;
                            if (objParent != null)
                                objParent.Children.Remove(objWeapon);
                            else if (objVehicleMod != null)
                                objVehicleMod.Weapons.Remove(objWeapon);
                            else if (objWeaponMount != null)
                                objWeaponMount.Weapons.Remove(objWeapon);
                            else if (objVehicle != null)
                                objVehicle.Weapons.Remove(objWeapon);
                            else
                                objCharacter.Weapons.Remove(objWeapon);
                        }
                    }
                        break;
                    case Improvement.ImprovementType.Spell:
                        Spell objSpell = objCharacter.Spells.FirstOrDefault(x => x.InternalId == objImprovement.ImprovedName);
                        if (objSpell != null)
                        {
                            decReturn += RemoveImprovements(objCharacter, Improvement.ImprovementSource.Spell, objSpell.InternalId);
                            objCharacter.Spells.Remove(objSpell);
                        }
                        break;
                    case Improvement.ImprovementType.ComplexForm:
                        ComplexForm objComplexForm = objCharacter.ComplexForms.FirstOrDefault(x => x.InternalId == objImprovement.ImprovedName);
                        if (objComplexForm != null)
                        {
                            decReturn += RemoveImprovements(objCharacter, Improvement.ImprovementSource.ComplexForm, objComplexForm.InternalId);
                            objCharacter.ComplexForms.Remove(objComplexForm);
                        }
                        break;
                    case Improvement.ImprovementType.MartialArt:
                        MartialArt objMartialArt = objCharacter.MartialArts.FirstOrDefault(x => x.InternalId == objImprovement.ImprovedName);
                        if (objMartialArt != null)
                        {
                            decReturn += RemoveImprovements(objCharacter, Improvement.ImprovementSource.MartialArt, objMartialArt.InternalId);
                            // Remove the Improvements for any Advantages for the Martial Art that is being removed.
                            foreach (MartialArtTechnique objAdvantage in objMartialArt.Techniques)
                            {
                                decReturn += RemoveImprovements(objCharacter, Improvement.ImprovementSource.MartialArtTechnique, objAdvantage.InternalId);
                            }
                            objCharacter.MartialArts.Remove(objMartialArt);
                        }
                        break;
                    case Improvement.ImprovementType.SpecialSkills:
                        if (!blnHasDuplicate)
                            objCharacter.SkillsSection.RemoveSkills((FilterOption)Enum.Parse(typeof(FilterOption), objImprovement.ImprovedName), !blnReapplyImprovements);
                        break;
                    case Improvement.ImprovementType.SpecificQuality:
                        Quality objQuality = objCharacter.Qualities.FirstOrDefault(objLoopQuality => objLoopQuality.InternalId == objImprovement.ImprovedName);
                        if (objQuality != null)
                        {
                            decReturn += RemoveImprovements(objCharacter, Improvement.ImprovementSource.Quality, objQuality.InternalId);
                            objCharacter.Qualities.Remove(objQuality);
                        }
                        break;
                    case Improvement.ImprovementType.SkillSpecialization:
                    case Improvement.ImprovementType.SkillExpertise:
                        {
                            Skill objSkill = objCharacter.SkillsSection.GetActiveSkill(objImprovement.ImprovedName);
                            SkillSpecialization objSkillSpec = objSkill?.Specializations.FirstOrDefault(x => x.Name == objImprovement.UniqueName);
                            if (objSkillSpec != null)
                                objSkill.Specializations.Remove(objSkillSpec);
                        }
                        break;
                    case Improvement.ImprovementType.AIProgram:
                        AIProgram objProgram = objCharacter.AIPrograms.FirstOrDefault(objLoopProgram => objLoopProgram.InternalId == objImprovement.ImprovedName);
                        if (objProgram != null)
                        {
                            decReturn += RemoveImprovements(objCharacter, Improvement.ImprovementSource.AIProgram, objProgram.InternalId);
                            objCharacter.AIPrograms.Remove(objProgram);
                        }
                        break;
                    case Improvement.ImprovementType.AdeptPowerFreeLevels:
                    case Improvement.ImprovementType.AdeptPowerFreePoints:
                        // Get the power improved by this improvement
                        Power objImprovedPower = objCharacter.Powers.FirstOrDefault(objPower => objPower.Name == objImprovement.ImprovedName &&
                                        objPower.Extra == objImprovement.UniqueName);
                        if (objImprovedPower != null)
                        {
                            if (objImprovedPower.TotalRating <= 0)
                            {
                                objImprovedPower.DeletePower();
                                objImprovedPower.UnbindPower();
                            }

                            objImprovedPower.OnPropertyChanged(nameof(objImprovedPower.TotalRating));
                            objImprovedPower.OnPropertyChanged(objImprovement.ImproveType == Improvement.ImprovementType.AdeptPowerFreeLevels
                                ? nameof(Power.FreeLevels) : nameof(Power.FreePoints));
                        }
                        break;
                    case Improvement.ImprovementType.FreeWare:
                        {
                            Cyberware objCyberware = objCharacter.Cyberware.FirstOrDefault(o => o.InternalId == objImprovement.ImprovedName);
                            if (objCyberware != null)
                            {
                                decReturn += objCyberware.DeleteCyberware();
                                decReturn += objCyberware.TotalCost;
                                objCharacter.Cyberware.Remove(objCyberware);
                            }
                        }
                        break;
                }
            }
            objImprovementList.ProcessRelevantEvents();

            Log.Debug("RemoveImprovements exit");
            return decReturn;
        }

        /// <summary>
        /// Create a new Improvement and add it to the Character.
        /// </summary>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="strImprovedName">Specific name of the Improved object - typically the name of an CharacterAttribute being improved.</param>
        /// <param name="objImprovementSource">Type of object that grants this Improvement.</param>
        /// <param name="strSourceName">Name of the item that grants this Improvement.</param>
        /// <param name="objImprovementType">Type of object the Improvement applies to.</param>
        /// <param name="strUnique">Name of the pool this Improvement should be added to - only the single highest value in the pool will be applied to the character.</param>
        /// <param name="intValue">Set a Value for the Improvement.</param>
        /// <param name="intRating">Set a Rating for the Improvement - typically used for Adept Powers.</param>
        /// <param name="intMinimum">Improve the Minimum for an CharacterAttribute by the given amount.</param>
        /// <param name="intMaximum">Improve the Maximum for an CharacterAttribute by the given amount.</param>
        /// <param name="intAugmented">Improve the Augmented value for an CharacterAttribute by the given amount.</param>
        /// <param name="intAugmentedMaximum">Improve the Augmented Maximum value for an CharacterAttribute by the given amount.</param>
        /// <param name="strExclude">A list of child items that should not receive the Improvement's benefit (typically for Skill Groups).</param>
        /// <param name="blnAddToRating">Whether or not the bonus applies to a Skill's Rating instead of the dice pool in general.</param>
        /// <param name="strTarget">What target the Improvement has, if any (e.g. a target skill whose attribute to replace).</param>
        /// <param name="strCondition">Condition for when the bonus is applied.</param>
        public static void CreateImprovement(Character objCharacter, string strImprovedName, Improvement.ImprovementSource objImprovementSource,
            string strSourceName, Improvement.ImprovementType objImprovementType, string strUnique,
            int intValue = 0, int intRating = 1, int intMinimum = 0, int intMaximum = 0, int intAugmented = 0,
            int intAugmentedMaximum = 0, string strExclude = "", bool blnAddToRating = false, string strTarget = "", string strCondition = "")
        {
            Log.Debug("CreateImprovement");
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
                "intValue = " + intValue.ToString(GlobalOptions.InvariantCultureInfo));
            Log.Info(
                "intRating = " + intRating.ToString(GlobalOptions.InvariantCultureInfo));
            Log.Info(
                "intMinimum = " + intMinimum.ToString(GlobalOptions.InvariantCultureInfo));
            Log.Info(
                "intMaximum = " + intMaximum.ToString(GlobalOptions.InvariantCultureInfo));
            Log.Info(
                "intAugmented = " + intAugmented.ToString(GlobalOptions.InvariantCultureInfo));
            Log.Info(
                "intAugmentedMaximum = " + intAugmentedMaximum.ToString(GlobalOptions.InvariantCultureInfo));
            Log.Info( "strExclude = " + strExclude);
            Log.Info(
                "blnAddToRating = " + blnAddToRating.ToString(GlobalOptions.InvariantCultureInfo));
            Log.Info("strCondition = " + strCondition);

            // Do not attempt to add the Improvements if the Character is null (as a result of Cyberware being added to a VehicleMod).
            if (objCharacter != null)
            {
                // Record the improvement.
                Improvement objImprovement = new Improvement(objCharacter)
                {
                    ImprovedName = strImprovedName,
                    ImproveSource = objImprovementSource,
                    SourceName = strSourceName,
                    ImproveType = objImprovementType,
                    UniqueName = strUnique,
                    Value = intValue,
                    Rating = intRating,
                    Minimum = intMinimum,
                    Maximum = intMaximum,
                    Augmented = intAugmented,
                    AugmentedMaximum = intAugmentedMaximum,
                    Exclude = strExclude,
                    AddToRating = blnAddToRating,
                    Target = strTarget,
                    Condition = strCondition
                };

                // Add the Improvement to the list.
                objCharacter.Improvements.Add(objImprovement);
                ClearCachedValue(objCharacter, objImprovement.ImproveType, objImprovement.ImprovedName);

                // Add the Improvement to the Transaction List.
                if (!s_DictionaryTransactions.TryAdd(objCharacter, new List<TransactingImprovement> { new TransactingImprovement(objImprovement) }))
                    s_DictionaryTransactions[objCharacter].Add(new TransactingImprovement(objImprovement));
            }

            Log.Debug("CreateImprovement exit");
        }

        /// <summary>
        /// Clear all of the Improvements from the Transaction List.
        /// </summary>
        public static void Commit(Character objCharacter)
        {
            Log.Debug("Commit");
            // Clear all of the Improvements from the Transaction List.
            if (s_DictionaryTransactions.TryGetValue(objCharacter, out List<TransactingImprovement> lstTransaction))
            {
                List<Improvement> lstImprovementsToProcess = new List<Improvement>();
                foreach (TransactingImprovement objLoopTransactingImprovement in lstTransaction)
                {
                    if (!objLoopTransactingImprovement.IsCommitting)
                    {
                        objLoopTransactingImprovement.IsCommitting = true;
                        lstImprovementsToProcess.Add(objLoopTransactingImprovement.ImprovementObject);
                    }
                }
                lstImprovementsToProcess.ProcessRelevantEvents();
                lstTransaction.Clear();
            }

            Log.Debug("Commit exit");
        }

        /// <summary>
        /// Rollback all of the Improvements from the Transaction List.
        /// </summary>
        private static void Rollback(Character objCharacter)
        {
            Log.Debug("Rollback enter");
            if (s_DictionaryTransactions.TryGetValue(objCharacter, out List<TransactingImprovement> lstTransaction))
            {
                // Remove all of the Improvements that were added.
                foreach (TransactingImprovement objTransactingImprovement in lstTransaction.ToList())
                {
                    RemoveImprovements(objCharacter, objTransactingImprovement.ImprovementObject.ImproveSource, objTransactingImprovement.ImprovementObject.SourceName);
                    ClearCachedValue(objCharacter, objTransactingImprovement.ImprovementObject.ImproveType, objTransactingImprovement.ImprovementObject.ImprovedName);
                }

                lstTransaction.Clear();
            }

            Log.Debug("Rollback exit");
        }

        /// <summary>
        /// Fire off all events relevant to an enumerable of improvements, making sure each event is only fired once.
        /// </summary>
        /// <param name="lstImprovements">Enumerable of improvements whose events to fire</param>
        public static void ProcessRelevantEvents(this IEnumerable<Improvement> lstImprovements)
        {
            if (lstImprovements == null)
                return;
            // Create a hashset of events to fire to make sure we only ever fire each event once
            Dictionary<INotifyMultiplePropertyChanged, HashSet<string>> dicPropertiesChanged = new Dictionary<INotifyMultiplePropertyChanged, HashSet<string>>();
            foreach (Improvement objImprovement in lstImprovements)
            {
                foreach (Tuple<INotifyMultiplePropertyChanged, string> tuplePropertyChanged in objImprovement.GetRelevantPropertyChangers())
                {
                    if (dicPropertiesChanged.TryGetValue(tuplePropertyChanged.Item1, out HashSet<string> setLoopPropertiesChanged))
                    {
                        setLoopPropertiesChanged.Add(tuplePropertyChanged.Item2);
                    }
                    else
                    {
                        dicPropertiesChanged.Add(tuplePropertyChanged.Item1, new HashSet<string> { tuplePropertyChanged.Item2 });
                    }
                }
            }
            // Fire each event once
            foreach (KeyValuePair<INotifyMultiplePropertyChanged, HashSet<string>> pairPropertiesChanged in dicPropertiesChanged)
                pairPropertiesChanged.Key.OnMultiplePropertyChanged(pairPropertiesChanged.Value.ToArray());
        }
        #endregion
    }
}
