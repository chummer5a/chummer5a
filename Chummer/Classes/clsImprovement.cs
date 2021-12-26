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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Attributes;
using Chummer.Backend.Equipment;
using Chummer.Backend.Skills;
using Chummer.Backend.Uniques;
using Chummer.Classes;
using NLog;
using static Chummer.Backend.Skills.SkillsSection;

namespace Chummer
{
    [DebuggerDisplay("{" + nameof(DisplayDebug) + "()}")]
    public class Improvement : IHasNotes, IHasInternalId, ICanSort
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();

        private string DisplayDebug()
        {
            return string.Format(GlobalSettings.InvariantCultureInfo, "{0} ({1}, {2}) 🡐 {3}, {4}, {5}",
                _objImprovementType, _decVal, _intRating, _objImprovementSource, _strSourceName, _strImprovedName);
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
            Art,
            Metamagic,
            Echo,
            Skillwire,
            DamageResistance,
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
            Skill, //Improve pool of skill based on name
            SkillGroup, //Group
            SkillCategory, //category
            SkillAttribute, //attribute
            SkillLinkedAttribute, //linked attribute
            SkillLevel, //Karma points in skill
            SkillGroupLevel, //group
            SkillBase, //base points in skill
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
            CyberadeptDaemon,
            PenaltyFreeSustain,
            WeaponRangeModifier,
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
            CyberadeptDaemon,
            NumImprovementSources // 🡐 This one should always be the last defined enum
        }

        private readonly Character _objCharacter;
        private string _strImprovedName = string.Empty;
        private string _strSourceName = string.Empty;
        private int _intMin;
        private int _intMax;
        private decimal _decAug;
        private int _intAugMax;
        private decimal _decVal;
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
        private Color _colNotes = ColorManager.HasNotesColor;
        private bool _blnAddToRating;
        private bool _blnEnabled = true;
        private bool _blnSetupComplete;  // Start with Improvement disabled, then enable it after all properties are set up at creation
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
                strValue = strValue.Replace("InitiativePass", "InitiativeDice");
            }

            if (strValue == "ContactForceLoyalty")
                strValue = "ContactForcedLoyalty";
            return (ImprovementType)Enum.Parse(typeof(ImprovementType), strValue);
        }

        /// <summary>
        /// Convert a string to an ImprovementSource.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        public static ImprovementSource ConvertToImprovementSource(string strValue)
        {
            if (strValue == "MartialArtAdvantage")
                strValue = "MartialArtTechnique";
            return (ImprovementSource)Enum.Parse(typeof(ImprovementSource), strValue);
        }

        #endregion Helper Methods

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
            objWriter.WriteElementString("min", _intMin.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("max", _intMax.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("aug", _decAug.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("augmax", _intAugMax.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("val", _decVal.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("rating", _intRating.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("exclude", _strExclude);
            objWriter.WriteElementString("condition", _strCondition);
            objWriter.WriteElementString("improvementttype", _objImprovementType.ToString());
            objWriter.WriteElementString("improvementsource", _objImprovementSource.ToString());
            objWriter.WriteElementString("custom", _blnCustom.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("customname", _strCustomName);
            objWriter.WriteElementString("customid", _strCustomId);
            objWriter.WriteElementString("customgroup", _strCustomGroup);
            objWriter.WriteElementString("addtorating", _blnAddToRating.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("enabled", _blnEnabled.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("order", _intOrder.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("notes", System.Text.RegularExpressions.Regex.Replace(_strNotes, @"[\u0000-\u0008\u000B\u000C\u000E-\u001F]", ""));
            objWriter.WriteElementString("notesColor", ColorTranslator.ToHtml(_colNotes));
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
            objNode.TryGetDecFieldQuickly("aug", ref _decAug);
            objNode.TryGetInt32FieldQuickly("augmax", ref _intAugMax);
            objNode.TryGetDecFieldQuickly("val", ref _decVal);
            objNode.TryGetInt32FieldQuickly("rating", ref _intRating);
            objNode.TryGetStringFieldQuickly("exclude", ref _strExclude);
            objNode.TryGetStringFieldQuickly("condition", ref _strCondition);
            if (objNode["improvementttype"] != null)
                _objImprovementType = ConvertToImprovementType(objNode["improvementttype"].InnerText);
            if (objNode["improvementsource"] != null)
                _objImprovementSource = ConvertToImprovementSource(objNode["improvementsource"].InnerText);
            // Legacy shims
            if (_objCharacter.LastSavedVersion <= new Version(5, 214, 112)
                && (_objImprovementSource == ImprovementSource.Initiation || _objImprovementSource == ImprovementSource.Submersion)
                && _objImprovementType == ImprovementType.Attribute
                && _intMax > 1 && _intRating == 1)
            {
                _intRating = _intMax;
                _intMax = 1;
            }
            switch (_objImprovementType)
            {
                case ImprovementType.LimitModifier when string.IsNullOrEmpty(_strCondition) && !string.IsNullOrEmpty(_strExclude):
                    _strCondition = _strExclude;
                    _strExclude = string.Empty;
                    break;
                case ImprovementType.RestrictedGear when _decVal == 0:
                    _decVal = 24;
                    break;
            }

            objNode.TryGetBoolFieldQuickly("custom", ref _blnCustom);
            objNode.TryGetStringFieldQuickly("customname", ref _strCustomName);
            objNode.TryGetStringFieldQuickly("customid", ref _strCustomId);
            objNode.TryGetStringFieldQuickly("customgroup", ref _strCustomGroup);
            objNode.TryGetBoolFieldQuickly("addtorating", ref _blnAddToRating);
            objNode.TryGetBoolFieldQuickly("enabled", ref _blnEnabled);
            objNode.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

            string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
            objNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
            _colNotes = ColorTranslator.FromHtml(sNotesColor);

            objNode.TryGetInt32FieldQuickly("order", ref _intOrder);

            Log.Trace("Load exit");
        }

        #endregion Save and Load Methods

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
        /// Forecolor to use for Notes in treeviews.
        /// </summary>
        public Color NotesColor
        {
            get => _colNotes;
            set => _colNotes = value;
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
                        this.Yield().ProcessRelevantEvents();
                    }

                    _strImprovedName = value;

                    if (Enabled)
                        this.Yield().ProcessRelevantEvents();
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
                        this.Yield().ProcessRelevantEvents();
                    }

                    _objImprovementType = value;

                    if (Enabled)
                        this.Yield().ProcessRelevantEvents();
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
                    if (Enabled)
                        this.Yield().ProcessRelevantEvents();
                    _objImprovementSource = value;
                    if (Enabled)
                    {
                        ImprovementManager.ClearCachedValue(_objCharacter, ImproveType, ImprovedName);
                        this.Yield().ProcessRelevantEvents();
                    }
                }
            }
        }

        /// <summary>
        /// Minimum value modifier.
        /// </summary>
        public int Minimum
        {
            get => _intMin;
            set
            {
                if (_intMin != value)
                {
                    _intMin = value;
                    if (Enabled)
                    {
                        ImprovementManager.ClearCachedValue(_objCharacter, ImproveType, ImprovedName);
                        this.Yield().ProcessRelevantEvents();
                    }
                }
            }
        }

        /// <summary>
        /// Maximum value modifier.
        /// </summary>
        public int Maximum
        {
            get => _intMax;
            set
            {
                if (_intMax != value)
                {
                    _intMax = value;
                    if (Enabled)
                    {
                        ImprovementManager.ClearCachedValue(_objCharacter, ImproveType, ImprovedName);
                        this.Yield().ProcessRelevantEvents();
                    }
                }
            }
        }

        /// <summary>
        /// Augmented Maximum value modifier.
        /// </summary>
        public int AugmentedMaximum
        {
            get => _intAugMax;
            set
            {
                if (_intAugMax != value)
                {
                    _intAugMax = value;
                    if (Enabled)
                    {
                        ImprovementManager.ClearCachedValue(_objCharacter, ImproveType, ImprovedName);
                        this.Yield().ProcessRelevantEvents();
                    }
                }
            }
        }

        /// <summary>
        /// Augmented score modifier.
        /// </summary>
        public decimal Augmented
        {
            get => _decAug;
            set
            {
                if (_decAug != value)
                {
                    _decAug = value;
                    if (Enabled)
                    {
                        ImprovementManager.ClearCachedValue(_objCharacter, ImproveType, ImprovedName);
                        this.Yield().ProcessRelevantEvents();
                    }
                }
            }
        }

        /// <summary>
        /// Value modifier.
        /// </summary>
        public decimal Value
        {
            get => _decVal;
            set
            {
                if (_decVal != value)
                {
                    _decVal = value;
                    if (Enabled)
                    {
                        ImprovementManager.ClearCachedValue(_objCharacter, ImproveType, ImprovedName);
                        this.Yield().ProcessRelevantEvents();
                    }
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
                    {
                        ImprovementManager.ClearCachedValue(_objCharacter, ImproveType, ImprovedName);
                        this.Yield().ProcessRelevantEvents();
                    }
                }
            }
        }

        /// <summary>
        /// A list of child items that should not receive the Improvement's benefit (typically for excluding a Skill from a Skill Group bonus).
        /// </summary>
        public string Exclude
        {
            get => _strExclude;
            set
            {
                if (_strExclude != value)
                {
                    if (Enabled)
                        this.Yield().ProcessRelevantEvents();
                    _strExclude = value;
                    if (Enabled)
                        this.Yield().ProcessRelevantEvents();
                }
            }
        }

        /// <summary>
        /// String containing the condition for when the bonus applies (e.g. a dicepool bonus to a skill that only applies to certain types of tests).
        /// </summary>
        public string Condition
        {
            get => _strCondition;
            set
            {
                if (_strCondition != value)
                {
                    if (Enabled)
                        this.Yield().ProcessRelevantEvents();
                    _strCondition = value;
                    if (Enabled)
                        this.Yield().ProcessRelevantEvents();
                }
            }
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
                    if (Enabled)
                        this.Yield().ProcessRelevantEvents();
                    _strUniqueName = value;
                    if (Enabled)
                    {
                        ImprovementManager.ClearCachedValue(_objCharacter, ImproveType, ImprovedName);
                        this.Yield().ProcessRelevantEvents();
                    }
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
                    if (Enabled)
                        this.Yield().ProcessRelevantEvents();
                    _blnAddToRating = value;
                    if (Enabled)
                    {
                        ImprovementManager.ClearCachedValue(_objCharacter, ImproveType, ImprovedName);
                        this.Yield().ProcessRelevantEvents();
                    }
                }
            }
        }

        /// <summary>
        /// The target of an improvement, e.g. the skill whose attributes should be swapped
        /// </summary>
        public string Target
        {
            get => _strTarget;
            set
            {
                if (_strTarget != value)
                {
                    if (Enabled)
                        this.Yield().ProcessRelevantEvents();
                    _strTarget = value;
                    if (Enabled)
                        this.Yield().ProcessRelevantEvents();
                }
            }
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
                    this.Yield().ProcessRelevantEvents();
                }
            }
        }

        /// <summary>
        /// Whether or not we have completed our first setup. Needed to skip superfluous event updates at startup
        /// </summary>
        public bool SetupComplete
        {
            get => _blnSetupComplete;
            set => _blnSetupComplete = value;
        }

        /// <summary>
        /// Sort order for Custom Improvements.
        /// </summary>
        public int SortOrder
        {
            get => _intOrder;
            set => _intOrder = value;
        }

        #endregion Properties

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
                            // Keeping two enumerations separate helps avoid extra heap allocations
                            foreach (CharacterAttrib objCharacterAttrib in _objCharacter.AttributeSection.AttributeList)
                            {
                                if (objCharacterAttrib.Abbrev != strTargetAttribute)
                                    continue;
                                foreach (string strPropertyName in setAttributePropertiesChanged)
                                {
                                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(objCharacterAttrib,
                                        strPropertyName);
                                }
                            }
                            foreach (CharacterAttrib objCharacterAttrib in _objCharacter.AttributeSection.SpecialAttributeList)
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
                            nameof(Character.GetArmorRating));
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
                            nameof(Character.TotalStartingNuyen));
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
                            _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.DictionaryKey == Target)
                            ?? _objCharacter.SkillsSection.KnowledgeSkills.FirstOrDefault(x =>
                                x.DictionaryKey == Target || x.CurrentDisplayName == Target);
                        if (objTargetSkill != null)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                nameof(Skill.DefaultAttribute));
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
                        // Keeping two enumerations separate helps avoid extra heap allocations
                        foreach (Skill objSkill in _objCharacter.SkillsSection.Skills)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objSkill,
                                nameof(Skill.CyberwareRating));
                        }
                        foreach (KnowledgeSkill objSkill in _objCharacter.SkillsSection.KnowledgeSkills)
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
                        // Keeping two enumerations separate helps avoid extra heap allocations
                        foreach (CharacterAttrib objCharacterAttrib in _objCharacter.AttributeSection.AttributeList)
                        {
                            if (objCharacterAttrib.Abbrev == "ESS")
                            {
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objCharacterAttrib,
                                    nameof(CharacterAttrib.MetatypeMaximum));
                            }
                        }
                        foreach (CharacterAttrib objCharacterAttrib in _objCharacter.AttributeSection.SpecialAttributeList)
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
                        // Keeping two enumerations separate helps avoid extra heap allocations
                        foreach (CharacterAttrib objCharacterAttrib in _objCharacter.AttributeSection.AttributeList)
                        {
                            if (objCharacterAttrib.Abbrev == ImprovedName)
                            {
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objCharacterAttrib,
                                    nameof(CharacterAttrib.FreeBase));
                            }
                        }
                        foreach (CharacterAttrib objCharacterAttrib in _objCharacter.AttributeSection.SpecialAttributeList)
                        {
                            if (objCharacterAttrib.Abbrev == ImprovedName)
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
                            _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.DictionaryKey == ImprovedName)
                            ?? _objCharacter.SkillsSection.KnowledgeSkills.FirstOrDefault(x =>
                                x.InternalId == ImprovedName || x.DictionaryKey == ImprovedName || x.CurrentDisplayName == ImprovedName);
                        if (objTargetSkill != null)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                nameof(Skill.CyberwareRating));
                        }
                    }
                    break;

                case ImprovementType.DealerConnection:
                    {
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                            nameof(Character.DealerConnectionDiscount));
                    }
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
                                _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.DictionaryKey == ImprovedName)
                                ?? _objCharacter.SkillsSection.KnowledgeSkills.FirstOrDefault(x =>
                                    x.DictionaryKey == ImprovedName || x.CurrentDisplayName == ImprovedName);
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
                            _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.DictionaryKey == ImprovedName)
                            ?? _objCharacter.SkillsSection.KnowledgeSkills.FirstOrDefault(x =>
                                x.DictionaryKey == ImprovedName || x.CurrentDisplayName == ImprovedName);
                        if (objTargetSkill != null)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                nameof(Skill.Base));
                        }
                    }
                    break;

                case ImprovementType.SkillGroup:
                    {
                        foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills)
                        {
                            if (objTargetSkill.SkillGroup == ImprovedName)
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                    nameof(Skill.PoolModifiers));
                        }
                    }
                    break;

                case ImprovementType.BlockSkillDefault:
                    {
                        foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills)
                        {
                            if (objTargetSkill.SkillGroup == ImprovedName)
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                    nameof(Skill.Default));
                        }
                    }
                    break;

                case ImprovementType.SkillCategory:
                    {
                        // Keeping two enumerations separate helps avoid extra heap allocations
                        foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills)
                        {
                            if (objTargetSkill.SkillCategory == ImprovedName)
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                    nameof(Skill.PoolModifiers));
                        }
                        foreach (KnowledgeSkill objTargetSkill in _objCharacter.SkillsSection.KnowledgeSkills)
                        {
                            if (objTargetSkill.SkillCategory == ImprovedName)
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                    nameof(Skill.PoolModifiers));
                        }
                    }
                    break;

                case ImprovementType.SkillLinkedAttribute:
                    {
                        // Keeping two enumerations separate helps avoid extra heap allocations
                        foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills)
                        {
                            if (objTargetSkill.Attribute == ImprovedName)
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                    nameof(Skill.PoolModifiers));
                        }
                        foreach (KnowledgeSkill objTargetSkill in _objCharacter.SkillsSection.KnowledgeSkills)
                        {
                            if (objTargetSkill.Attribute == ImprovedName)
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                    nameof(Skill.PoolModifiers));
                        }
                    }
                    break;

                case ImprovementType.SkillLevel:
                    {
                        Skill objTargetSkill =
                            _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.DictionaryKey == ImprovedName);
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
                            _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.DictionaryKey == ImprovedName);
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
                            x.InternalId == ImprovedName || x.DictionaryKey == ImprovedName || x.CurrentDisplayName == ImprovedName);
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
                            _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.DictionaryKey == ImprovedName);
                        if (objTargetSkill != null)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                nameof(Skill.CyberwareRating));
                        }
                    }
                    break;

                case ImprovementType.ReplaceAttribute:
                    {
                        // Keeping two enumerations separate helps avoid extra heap allocations
                        foreach (CharacterAttrib objCharacterAttrib in _objCharacter.AttributeSection.AttributeList)
                        {
                            if (objCharacterAttrib.Abbrev != ImprovedName)
                                continue;
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objCharacterAttrib,
                                nameof(CharacterAttrib.MetatypeMaximum));
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objCharacterAttrib,
                                nameof(CharacterAttrib.MetatypeMinimum));
                        }
                        foreach (CharacterAttrib objCharacterAttrib in _objCharacter.AttributeSection.SpecialAttributeList)
                        {
                            if (objCharacterAttrib.Abbrev != ImprovedName)
                                continue;
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

                case ImprovementType.SkillExpertise:
                case ImprovementType.SkillSpecialization:
                    {
                        Skill objTargetSkill =
                            _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.DictionaryKey == ImprovedName);
                        if (objTargetSkill != null)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                nameof(Skill.Specializations));
                        }

                        break;
                    }

                case ImprovementType.SkillSpecializationOption:
                    {
                        Skill objTargetSkill =
                            _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.DictionaryKey == ImprovedName);
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
                            _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.DictionaryKey == ImprovedName);
                        if (objTargetSkill != null)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                nameof(Skill.GetSpecializationBonus));
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
                        // Keeping two enumerations separate helps avoid extra heap allocations
                        foreach (CharacterAttrib objCharacterAttrib in _objCharacter.AttributeSection.AttributeList)
                        {
                            if (string.IsNullOrEmpty(ImprovedName) || objCharacterAttrib.Abbrev == ImprovedName)
                            {
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objCharacterAttrib,
                                    nameof(CharacterAttrib.UpgradeKarmaCost));
                            }
                        }
                        foreach (CharacterAttrib objCharacterAttrib in _objCharacter.AttributeSection.SpecialAttributeList)
                        {
                            if (string.IsNullOrEmpty(ImprovedName) || objCharacterAttrib.Abbrev == ImprovedName)
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
                                _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.DictionaryKey == ImprovedName);
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
                                x.DictionaryKey == ImprovedName || x.CurrentDisplayName == ImprovedName);
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
                            _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.DictionaryKey == ImprovedName);
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
                        // Keeping two enumerations separate helps avoid extra heap allocations
                        foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills)
                        {
                            if (objTargetSkill.SkillCategory == ImprovedName)
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                    nameof(Skill.CanAffordSpecialization));
                        }
                        foreach (KnowledgeSkill objTargetSkill in _objCharacter.SkillsSection.KnowledgeSkills)
                        {
                            if (objTargetSkill.SkillCategory == ImprovedName)
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                    nameof(Skill.CanAffordSpecialization));
                        }
                    }
                    break;

                case ImprovementType.SkillCategoryKarmaCost:
                case ImprovementType.SkillCategoryKarmaCostMultiplier:
                    {
                        // Keeping two enumerations separate helps avoid extra heap allocations
                        foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills)
                        {
                            if (objTargetSkill.SkillCategory == ImprovedName)
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                    nameof(Skill.UpgradeKarmaCost));
                        }
                        foreach (KnowledgeSkill objTargetSkill in _objCharacter.SkillsSection.KnowledgeSkills)
                        {
                            if (objTargetSkill.SkillCategory == ImprovedName)
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                    nameof(Skill.UpgradeKarmaCost));
                        }
                    }
                    break;

                case ImprovementType.SkillGroupCategoryDisable:
                    {
                        foreach (SkillGroup objTargetGroup in _objCharacter.SkillsSection.SkillGroups)
                        {
                            if (objTargetGroup.GetRelevantSkillCategories.Contains(ImprovedName))
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetGroup,
                                    nameof(SkillGroup.IsDisabled));
                        }
                    }
                    break;

                case ImprovementType.SkillGroupCategoryKarmaCostMultiplier:
                case ImprovementType.SkillGroupCategoryKarmaCost:
                    {
                        foreach (SkillGroup objTargetGroup in _objCharacter.SkillsSection.SkillGroups)
                        {
                            if (objTargetGroup.GetRelevantSkillCategories.Contains(ImprovedName))
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
                                _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.DictionaryKey == ImprovedName);
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
                        // Keeping two enumerations separate helps avoid extra heap allocations
                        foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills)
                        {
                            if (objTargetSkill.SkillCategory == ImprovedName)
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                    nameof(Skill.CanHaveSpecs));
                        }
                        foreach (KnowledgeSkill objTargetSkill in _objCharacter.SkillsSection.KnowledgeSkills)
                        {
                            if (objTargetSkill.SkillCategory == ImprovedName)
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
                        foreach (Power objLoopPower in _objCharacter.Powers)
                        {
                            if (objLoopPower.AdeptWayDiscount != 0)
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
                case ImprovementType.CyberadeptDaemon:
                    {
                        if (_objCharacter.Settings.SpecialKarmaCostBasedOnShownValue)
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                                nameof(Character.CyberwareEssence));
                        break;
                    }
                case ImprovementType.PenaltyFreeSustain:
                    {
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                            nameof(Character.SustainingPenalty));
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
                ToolTipText = Notes.WordWrap(),
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
                    return !Enabled
                        ? ColorManager.GenerateCurrentModeDimmedColor(NotesColor)
                        : ColorManager.GenerateCurrentModeColor(NotesColor);
                }

                return !Enabled
                    ? ColorManager.GrayText
                    : ColorManager.WindowText;
            }
        }

        #endregion UI Methods

        #endregion Methods

        public string InternalId => SourceName;
    }

    public readonly struct ImprovementDictionaryKey : IEquatable<ImprovementDictionaryKey>, IEquatable<Tuple<Character, Improvement.ImprovementType, string>>
    {
        private readonly Tuple<Character, Improvement.ImprovementType, string> _objTupleKey;

        public Character CharacterObject => _objTupleKey.Item1;
        public Improvement.ImprovementType ImprovementType => _objTupleKey.Item2;
        public string ImprovementName => _objTupleKey.Item3;

        public ImprovementDictionaryKey(Character objCharacter, Improvement.ImprovementType eImprovementType, string strImprovementName)
        {
            _objTupleKey = new Tuple<Character, Improvement.ImprovementType, string>(objCharacter, eImprovementType, strImprovementName);
        }

        public override int GetHashCode()
        {
            return (CharacterObject, ImprovementType, ImprovementName).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case ImprovementDictionaryKey objOtherImprovementDictionaryKey:
                    return Equals(objOtherImprovementDictionaryKey);
                case Tuple<Character, Improvement.ImprovementType, string> objOtherTuple:
                    return Equals(objOtherTuple);
                default:
                    return false;
            }
        }

        public bool Equals(ImprovementDictionaryKey other)
        {
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

        public static bool operator ==(ImprovementDictionaryKey x, ImprovementDictionaryKey y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(ImprovementDictionaryKey x, ImprovementDictionaryKey y)
        {
            return !x.Equals(y);
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
            return x?.Equals(y) ?? false;
        }

        public static bool operator !=(object x, ImprovementDictionaryKey y)
        {
            return !(x?.Equals(y) ?? false);
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
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();

        // String that will be used to limit the selection in Pick forms.
        private static string _strLimitSelection = string.Empty;
        private static string _strSelectedValue = string.Empty;
        private static string _strForcedValue = string.Empty;
        private static readonly LockingDictionary<Character, List<TransactingImprovement>> s_DictionaryTransactions = new LockingDictionary<Character, List<TransactingImprovement>>(10);
        private static readonly LockingHashSet<Tuple<ImprovementDictionaryKey, IDictionary>> s_SetCurrentlyCalculatingValues = new LockingHashSet<Tuple<ImprovementDictionaryKey, IDictionary>>();
        private static readonly LockingDictionary<ImprovementDictionaryKey, Tuple<decimal, List<Improvement>>> s_DictionaryCachedValues = new LockingDictionary<ImprovementDictionaryKey, Tuple<decimal, List<Improvement>>>((int)Improvement.ImprovementType.NumImprovementTypes);
        private static readonly LockingDictionary<ImprovementDictionaryKey, Tuple<decimal, List<Improvement>>> s_DictionaryCachedAugmentedValues = new LockingDictionary<ImprovementDictionaryKey, Tuple<decimal, List<Improvement>>>((int)Improvement.ImprovementType.NumImprovementTypes);

        #region Properties

        /// <summary>
        /// Limit what can be selected in Pick forms to a single value. This is typically used when selecting the Qualities for a Metavariant that has a specific
        /// CharacterAttribute selection for Qualities like Metagenic Improvement.
        /// </summary>
        public static string LimitSelection
        {
            get => _strLimitSelection;
            set => _strLimitSelection = value;
        }

        /// <summary>
        /// The string that was entered or selected from any of the dialogue windows that were presented because of this Improvement.
        /// </summary>
        public static string SelectedValue
        {
            get => _strSelectedValue;
            set => _strSelectedValue = value;
        }

        /// <summary>
        /// Force any dialogue windows that open to use this string as their selected value.
        /// </summary>
        public static string ForcedValue
        {
            get => _strForcedValue;
            set => _strForcedValue = value;
        }

        public static void ClearCachedValue(Character objCharacter, Improvement.ImprovementType eImprovementType, string strImprovementName = "")
        {
            if (!string.IsNullOrEmpty(strImprovementName))
            {
                ImprovementDictionaryKey objCheckKey = new ImprovementDictionaryKey(objCharacter, eImprovementType, strImprovementName);
                if (!s_DictionaryCachedValues.TryAdd(objCheckKey,
                    new Tuple<decimal, List<Improvement>>(decimal.MinValue, new List<Improvement>())))
                {
                    List<Improvement> lstTemp = s_DictionaryCachedValues[objCheckKey].Item2;
                    lstTemp.Clear();
                    s_DictionaryCachedValues[objCheckKey] = new Tuple<decimal, List<Improvement>>(decimal.MinValue, lstTemp);
                }

                if (!s_DictionaryCachedAugmentedValues.TryAdd(objCheckKey,
                    new Tuple<decimal, List<Improvement>>(decimal.MinValue, new List<Improvement>())))
                {
                    List<Improvement> lstTemp = s_DictionaryCachedValues[objCheckKey].Item2;
                    lstTemp.Clear();
                    s_DictionaryCachedAugmentedValues[objCheckKey] = new Tuple<decimal, List<Improvement>>(decimal.MinValue, lstTemp);
                }
            }
            else
            {
                foreach (ImprovementDictionaryKey objCheckKey in s_DictionaryCachedValues.Keys.Where(x => x.CharacterObject == objCharacter && x.ImprovementType == eImprovementType).ToList())
                {
                    List<Improvement> lstTemp = s_DictionaryCachedValues[objCheckKey].Item2;
                    lstTemp.Clear();
                    s_DictionaryCachedValues[objCheckKey] = new Tuple<decimal, List<Improvement>>(decimal.MinValue, lstTemp);
                }

                foreach (ImprovementDictionaryKey objCheckKey in s_DictionaryCachedAugmentedValues.Keys.Where(x => x.CharacterObject == objCharacter && x.ImprovementType == eImprovementType).ToList())
                {
                    List<Improvement> lstTemp = s_DictionaryCachedValues[objCheckKey].Item2;
                    lstTemp.Clear();
                    s_DictionaryCachedAugmentedValues[objCheckKey] = new Tuple<decimal, List<Improvement>>(decimal.MinValue, lstTemp);
                }
            }
        }

        public static void ClearCachedValues(Character objCharacter)
        {
            foreach (ImprovementDictionaryKey objKey in s_DictionaryCachedValues.Keys.ToList())
            {
                if (objKey.CharacterObject == objCharacter && s_DictionaryCachedValues.TryRemove(objKey, out Tuple<decimal, List<Improvement>> tupTemp))
                    tupTemp.Item2.Clear(); // Just in case this helps the GC
            }

            foreach (ImprovementDictionaryKey objKey in s_DictionaryCachedAugmentedValues.Keys.ToList())
            {
                if (objKey.CharacterObject == objCharacter && s_DictionaryCachedAugmentedValues.TryRemove(objKey, out Tuple<decimal, List<Improvement>> tupTemp))
                    tupTemp.Item2.Clear(); // Just in case this helps the GC
            }

            s_DictionaryTransactions.TryRemove(objCharacter, out List<TransactingImprovement> _);
        }

        #endregion Properties

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
        public static decimal ValueOf(Character objCharacter, Improvement.ImprovementType objImprovementType,
            bool blnAddToRating = false, string strImprovedName = "",
            bool blnUnconditionalOnly = true, bool blnIncludeNonImproved = false)
        {
            return ValueOf(objCharacter, objImprovementType, out List<Improvement> _, blnAddToRating,
                strImprovedName, blnUnconditionalOnly, blnIncludeNonImproved);
        }

        /// <summary>
        /// Retrieve the total Improvement value for the specified ImprovementType.
        /// </summary>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="objImprovementType">ImprovementType to retrieve the value of.</param>
        /// <param name="blnAddToRating">Whether or not we should only retrieve values that have AddToRating enabled.</param>
        /// <param name="strImprovedName">Name to assign to the Improvement.</param>
        /// <param name="blnUnconditionalOnly">Whether to only fetch values for improvements that do not have a condition.</param>
        /// <param name="blnIncludeNonImproved">Whether to only fetch values for improvements that do not have an improvedname when specifying ImprovedNames.</param>
        /// <param name="lstUsedImprovements">List of the improvements actually used for the value</param>
        public static decimal ValueOf(Character objCharacter, Improvement.ImprovementType objImprovementType,
            out List<Improvement> lstUsedImprovements,
            bool blnAddToRating = false, string strImprovedName = "",
            bool blnUnconditionalOnly = true, bool blnIncludeNonImproved = false)
        {
            return MetaValueOf(objCharacter, objImprovementType, out lstUsedImprovements, x => x.Value,
                s_DictionaryCachedValues, blnAddToRating, strImprovedName, blnUnconditionalOnly, blnIncludeNonImproved);
        }

        /// <summary>
        /// Gets the cached list of active improvements that contribute to values of a given improvement type.
        /// </summary>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="eImprovementType">ImprovementType for which active improvements should be fetched.</param>
        /// <param name="strImprovedName">Improvements are only fetched with the given improvedname. If empty, only those with an empty ImprovedName are fetched.</param>
        /// <param name="blnIncludeNonImproved">Whether to only fetch values for improvements that do not have an improvedname when specifying <paramref name="strImprovedName"/>.</param>
        /// <returns>A cached list of all unconditional improvements that do not add to ratings and that match the conditions set by the arguments.</returns>
        public static List<Improvement> GetCachedImprovementListForValueOf(
            Character objCharacter, Improvement.ImprovementType eImprovementType, string strImprovedName = "",
            bool blnIncludeNonImproved = false)
        {
            ValueOf(objCharacter, eImprovementType, out List<Improvement> lstReturn, strImprovedName: strImprovedName, blnIncludeNonImproved: blnIncludeNonImproved);
            return lstReturn;
        }

        /// <summary>
        /// Retrieve the total Improvement Augmented x Rating for the specified ImprovementType.
        /// </summary>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="objImprovementType">ImprovementType to retrieve the value of.</param>
        /// <param name="blnAddToRating">Whether or not we should only retrieve values that have AddToRating enabled.</param>
        /// <param name="strImprovedName">Name to assign to the Improvement.</param>
        /// <param name="blnUnconditionalOnly">Whether to only fetch values for improvements that do not have a condition.</param>
        /// <param name="blnIncludeNonImproved">Whether to only fetch values for improvements that do not have an improvedname when specifying ImprovedNames.</param>
        public static decimal AugmentedValueOf(Character objCharacter, Improvement.ImprovementType objImprovementType,
            bool blnAddToRating = false, string strImprovedName = "",
            bool blnUnconditionalOnly = true, bool blnIncludeNonImproved = false)
        {
            return AugmentedValueOf(objCharacter, objImprovementType, out List<Improvement> _, blnAddToRating,
                strImprovedName, blnUnconditionalOnly, blnIncludeNonImproved);
        }

        /// <summary>
        /// Retrieve the total Improvement Augmented x Rating for the specified ImprovementType.
        /// </summary>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="objImprovementType">ImprovementType to retrieve the value of.</param>
        /// <param name="lstUsedImprovements">List of the improvements actually used for the value.</param>
        /// <param name="blnAddToRating">Whether or not we should only retrieve values that have AddToRating enabled.</param>
        /// <param name="strImprovedName">Name to assign to the Improvement.</param>
        /// <param name="blnUnconditionalOnly">Whether to only fetch values for improvements that do not have a condition.</param>
        /// <param name="blnIncludeNonImproved">Whether to only fetch values for improvements that do not have an improvedname when specifying ImprovedNames.</param>
        public static decimal AugmentedValueOf(Character objCharacter, Improvement.ImprovementType objImprovementType,
            out List<Improvement> lstUsedImprovements, bool blnAddToRating = false, string strImprovedName = "",
            bool blnUnconditionalOnly = true, bool blnIncludeNonImproved = false)
        {
            return MetaValueOf(objCharacter, objImprovementType, out lstUsedImprovements, x => x.Augmented * x.Rating,
                s_DictionaryCachedAugmentedValues, blnAddToRating, strImprovedName, blnUnconditionalOnly, blnIncludeNonImproved);
        }

        /// <summary>
        /// Gets the cached list of active improvements that contribute to augmented values of a given improvement type.
        /// </summary>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="eImprovementType">ImprovementType for which active improvements should be fetched.</param>
        /// <param name="strImprovedName">Improvements are only fetched with the given improvedname. If empty, only those with an empty ImprovedName are fetched.</param>
        /// <param name="blnIncludeNonImproved">Whether to only fetch values for improvements that do not have an improvedname when specifying <paramref name="strImprovedName"/>.</param>
        /// <returns>A cached list of all unconditional improvements that do not add to ratings and that match the conditions set by the arguments.</returns>
        public static List<Improvement> GetCachedImprovementListForAugmentedValueOf(
            Character objCharacter, Improvement.ImprovementType eImprovementType, string strImprovedName = "",
            bool blnIncludeNonImproved = false)
        {
            AugmentedValueOf(objCharacter, eImprovementType, out List<Improvement> lstReturn, strImprovedName: strImprovedName, blnIncludeNonImproved: blnIncludeNonImproved);
            return lstReturn;
        }

        /// <summary>
        /// Internal function used for fetching some sort of collected value from a character's entire set of improvements
        /// </summary>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="eImprovementType">ImprovementType to retrieve the value of.</param>
        /// <param name="dicCachedValuesToUse">The caching dictionary to use. If null, values will not be cached.</param>
        /// <param name="blnAddToRating">Whether or not we should only retrieve values that have AddToRating enabled.</param>
        /// <param name="strImprovedName">Name to assign to the Improvement.</param>
        /// <param name="blnUnconditionalOnly">Whether to only fetch values for improvements that do not have a condition.</param>
        /// <param name="blnIncludeNonImproved">Whether to only fetch values for improvements that do not have an improvedname when specifying ImprovedNames.</param>
        /// <param name="lstUsedImprovements">List of the improvements actually used for the value</param>
        /// <param name="funcValueGetter">Function for how to extract values for individual improvements.</param>
        private static decimal MetaValueOf(Character objCharacter, Improvement.ImprovementType eImprovementType,
            out List<Improvement> lstUsedImprovements, Func<Improvement, decimal> funcValueGetter, LockingDictionary<ImprovementDictionaryKey, Tuple<decimal, List<Improvement>>> dicCachedValuesToUse,
            bool blnAddToRating, string strImprovedName,
            bool blnUnconditionalOnly, bool blnIncludeNonImproved)
        {
            //Log.Info("objImprovementType = " + objImprovementType.ToString());
            //Log.Info("blnAddToRating = " + blnAddToRating.ToString());
            //Log.Info("strImprovedName = " + ("" + strImprovedName).ToString());

            if (funcValueGetter == null)
                throw new ArgumentNullException(nameof(funcValueGetter));

            if (objCharacter == null)
            {
                lstUsedImprovements = new List<Improvement>();
                return 0;
            }

            if (string.IsNullOrWhiteSpace(strImprovedName))
                strImprovedName = string.Empty;

            // These values are needed to prevent race conditions that could cause Chummer to crash
            Tuple<ImprovementDictionaryKey, IDictionary> tupMyValueToCheck
                = new Tuple<ImprovementDictionaryKey, IDictionary>(
                    new ImprovementDictionaryKey(objCharacter, eImprovementType, strImprovedName),
                    dicCachedValuesToUse);
            Tuple<ImprovementDictionaryKey, IDictionary> tupBlankValueToCheck
                = new Tuple<ImprovementDictionaryKey, IDictionary>(
                    new ImprovementDictionaryKey(objCharacter, eImprovementType, string.Empty), dicCachedValuesToUse);

            // Only cache "default" ValueOf calls, otherwise there will be way too many values to cache
            bool blnFetchAndCacheResults = !blnAddToRating && blnUnconditionalOnly;

            // If we've got a value cached for the default ValueOf call for an improvementType, let's just return that
            if (blnFetchAndCacheResults)
            {
                if (dicCachedValuesToUse != null)
                {
                    // First check to make sure an existing caching for this particular value is not already running. If one is, wait for it to finish before continuing
                    int intLoopCount = 0;
                    while (!s_SetCurrentlyCalculatingValues.TryAdd(tupMyValueToCheck) && intLoopCount < 1000)
                    {
                        ++intLoopCount;
                        Utils.SafeSleep();
                    }

                    // Emergency exit, so break if we are debugging and return the default value (just in case)
                    if (intLoopCount >= 1000)
                    {
                        Utils.BreakIfDebug();
                        lstUsedImprovements = new List<Improvement>();
                        return 0;
                    }

                    // Also make sure we block off the conditionless check because we will be adding cached keys that will be used by the conditionless check
                    if (!string.IsNullOrWhiteSpace(strImprovedName))
                    {
                        intLoopCount = 0;
                        while (!s_SetCurrentlyCalculatingValues.TryAdd(tupBlankValueToCheck) && intLoopCount < 1000)
                        {
                            ++intLoopCount;
                            Utils.SafeSleep();
                        }

                        // Emergency exit, so break if we are debugging and return the default value (just in case)
                        if (intLoopCount >= 1000)
                        {
                            Utils.BreakIfDebug();
                            lstUsedImprovements = new List<Improvement>();
                            s_SetCurrentlyCalculatingValues.Remove(tupMyValueToCheck);
                            return 0;
                        }

                        ImprovementDictionaryKey objCacheKey
                            = new ImprovementDictionaryKey(objCharacter, eImprovementType, strImprovedName);
                        if (dicCachedValuesToUse.TryGetValue(objCacheKey,
                                                             out Tuple<decimal, List<Improvement>> tupCachedValue))
                        {
                            lstUsedImprovements = tupCachedValue.Item2; // For reduced memory usage
                            if (tupCachedValue.Item1 != decimal.MinValue)
                            {
                                s_SetCurrentlyCalculatingValues.Remove(tupMyValueToCheck);
                                s_SetCurrentlyCalculatingValues.Remove(tupBlankValueToCheck);
                                lstUsedImprovements = lstUsedImprovements.ToList(); // To make sure we do not inadvertently alter the cached list
                                return tupCachedValue.Item1;
                            }
                            lstUsedImprovements.Clear();
                        }
                        else
                            lstUsedImprovements = new List<Improvement>();
                    }
                    else
                    {
                        lstUsedImprovements = new List<Improvement>();
                        bool blnDoRecalculate = true;
                        decimal decCachedValue = 0;
                        // Only fetch based on cached values if the dictionary contains at least one element with matching characters and types and none of those elements have a "reset" value of decimal.MinValue
                        foreach (KeyValuePair<ImprovementDictionaryKey, Tuple<decimal, List<Improvement>>>
                                     objLoopCachedEntry in dicCachedValuesToUse)
                        {
                            ImprovementDictionaryKey objLoopKey = objLoopCachedEntry.Key;
                            if (objLoopKey.CharacterObject != objCharacter ||
                                objLoopKey.ImprovementType != eImprovementType)
                                continue;
                            blnDoRecalculate = false;
                            decimal decLoopCachedValue = objLoopCachedEntry.Value.Item1;
                            if (decLoopCachedValue == decimal.MinValue)
                            {
                                blnDoRecalculate = true;
                                break;
                            }

                            decCachedValue += decLoopCachedValue;
                            lstUsedImprovements.AddRange(objLoopCachedEntry.Value.Item2);
                        }

                        if (!blnDoRecalculate)
                        {
                            s_SetCurrentlyCalculatingValues.Remove(tupMyValueToCheck);
                            lstUsedImprovements = lstUsedImprovements.ToList(); // To make sure we do not inadvertently alter the cached list
                            return decCachedValue;
                        }

                        lstUsedImprovements.Clear();
                    }
                }
                else
                {
                    // The code is breaking here to remind you (the programmer) to add in caching functionality for this type of value.
                    // The more often this sort of value is used, the more caching is necessary and the more often we will break here,
                    // and the annoyance of constantly having your debugger break here should push you to adding in caching functionality.
                    Utils.BreakIfDebug();
                    lstUsedImprovements = new List<Improvement>();
                }
            }
            else
                lstUsedImprovements = new List<Improvement>();

            try
            {
                Dictionary<string, HashSet<string>> dicUniqueNames = new Dictionary<string, HashSet<string>>();
                Dictionary<string, List<Tuple<string, Improvement>>> dicUniquePairs
                    = new Dictionary<string, List<Tuple<string, Improvement>>>();
                Dictionary<string, decimal> dicValues = new Dictionary<string, decimal>();
                Dictionary<string, List<Improvement>> dicImprovementsForValues
                    = new Dictionary<string, List<Improvement>>();
                foreach (Improvement objImprovement in objCharacter.Improvements)
                {
                    if (objImprovement.ImproveType != eImprovementType || !objImprovement.Enabled ||
                        objImprovement.Custom ||
                        (blnUnconditionalOnly && !string.IsNullOrEmpty(objImprovement.Condition))) continue;
                    string strLoopImprovedName = objImprovement.ImprovedName;
                    bool blnAllowed = objImprovement.ImproveType == eImprovementType &&
                                      !((eImprovementType == Improvement.ImprovementType.MatrixInitiativeDice
                                         || eImprovementType == Improvement.ImprovementType.MatrixInitiative
                                         || eImprovementType == Improvement.ImprovementType.MatrixInitiativeDiceAdd)
                                        && objImprovement.ImproveSource == Improvement.ImprovementSource.Gear
                                        && objCharacter.ActiveCommlink is Gear objCommlink
                                        && objCommlink.Name == "Living Persona") &&
                                      // Ignore items that apply to a Skill's Rating.
                                      objImprovement.AddToRating == blnAddToRating &&
                                      // If an Improved Name has been passed, only retrieve values that have this Improved Name.
                                      (string.IsNullOrEmpty(strImprovedName) || strImprovedName == strLoopImprovedName
                                                                             || blnIncludeNonImproved
                                                                             && string.IsNullOrWhiteSpace(
                                                                                 strLoopImprovedName));

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
                            dicUniqueNames.Add(strLoopImprovedName, new HashSet<string> {strUniqueName});
                        }

                        // Add the values to the UniquePair List so we can check them later.
                        if (dicUniquePairs.TryGetValue(strLoopImprovedName,
                                                       out List<Tuple<string, Improvement>> lstUniquePairs))
                        {
                            lstUniquePairs.Add(new Tuple<string, Improvement>(strUniqueName, objImprovement));
                        }
                        else
                        {
                            dicUniquePairs.Add(strLoopImprovedName,
                                               new List<Tuple<string, Improvement>>(1)
                                                   {new Tuple<string, Improvement>(strUniqueName, objImprovement)});
                        }

                        if (!dicValues.ContainsKey(strLoopImprovedName))
                        {
                            dicValues.Add(strLoopImprovedName, 0);
                            dicImprovementsForValues.Add(strLoopImprovedName, new List<Improvement>());
                        }
                    }
                    else if (dicValues.TryGetValue(strLoopImprovedName, out decimal decExistingValue))
                    {
                        dicValues[strLoopImprovedName] = decExistingValue + funcValueGetter(objImprovement);
                        dicImprovementsForValues[strLoopImprovedName].Add(objImprovement);
                    }
                    else
                    {
                        dicValues.Add(strLoopImprovedName, funcValueGetter(objImprovement));
                        dicImprovementsForValues.Add(strLoopImprovedName,
                                                     new List<Improvement>(objImprovement.Yield()));
                    }
                }

                List<Improvement> lstLoopImprovements;
                List<Improvement> lstInnerLoopImprovements = new List<Improvement>();
                foreach (KeyValuePair<string, HashSet<string>> objLoopValuePair in dicUniqueNames)
                {
                    string strLoopImprovedName = objLoopValuePair.Key;
                    bool blnValuesDictionaryContains
                        = dicValues.TryGetValue(strLoopImprovedName, out decimal decLoopValue);
                    if (blnValuesDictionaryContains)
                        dicImprovementsForValues.TryGetValue(strLoopImprovedName, out lstLoopImprovements);
                    else
                        lstLoopImprovements = new List<Improvement>();
                    if (dicUniquePairs.TryGetValue(strLoopImprovedName,
                                                   out List<Tuple<string, Improvement>> lstUniquePairs))
                    {
                        HashSet<string> lstUniqueNames = objLoopValuePair.Value;
                        lstInnerLoopImprovements.Clear();
                        if (lstUniqueNames.Contains("precedence0"))
                        {
                            // Retrieve only the highest precedence0 value.
                            // Run through the list of UniqueNames and pick out the highest value for each one.
                            Improvement objHighestImprovement = null;
                            decimal decHighest = decimal.MinValue;
                            foreach ((string strUnique, Improvement objLoopImprovement) in lstUniquePairs)
                            {
                                if (strUnique != "precedence0")
                                    continue;
                                decimal decInnerLoopValue = funcValueGetter(objLoopImprovement);
                                if (decHighest < decInnerLoopValue)
                                {
                                    decHighest = decInnerLoopValue;
                                    objHighestImprovement = objLoopImprovement;
                                }
                            }

                            if (objHighestImprovement != null)
                                lstInnerLoopImprovements.Add(objHighestImprovement);

                            if (lstUniqueNames.Contains("precedence-1"))
                            {
                                foreach ((string strUnique, Improvement objLoopImprovement) in lstUniquePairs)
                                {
                                    if (strUnique != "precedence-1")
                                        continue;
                                    decHighest += funcValueGetter(objLoopImprovement);
                                    lstInnerLoopImprovements.Add(objLoopImprovement);
                                }
                            }

                            if (decLoopValue < decHighest)
                            {
                                decLoopValue = decHighest;
                                lstLoopImprovements.Clear();
                                lstLoopImprovements.AddRange(lstInnerLoopImprovements);
                            }
                        }
                        else if (lstUniqueNames.Contains("precedence1"))
                        {
                            // Retrieve all of the items that are precedence1 and nothing else.
                            decimal decHighest = 0;
                            foreach ((string strUnique, Improvement objLoopImprovement) in lstUniquePairs)
                            {
                                if (strUnique != "precedence1" && strUnique != "precedence-1")
                                    continue;
                                decHighest += funcValueGetter(objLoopImprovement);
                                lstInnerLoopImprovements.Add(objLoopImprovement);
                            }

                            if (decLoopValue < decHighest)
                            {
                                decLoopValue = decHighest;
                                lstLoopImprovements.Clear();
                                lstLoopImprovements.AddRange(lstInnerLoopImprovements);
                            }
                        }
                        else
                        {
                            // Run through the list of UniqueNames and pick out the highest value for each one.
                            foreach (string strUniqueName in lstUniqueNames)
                            {
                                Improvement objHighestImprovement = null;
                                decimal decHighest = decimal.MinValue;
                                foreach ((string strUnique, Improvement objLoopImprovement) in lstUniquePairs)
                                {
                                    if (strUnique != strUniqueName)
                                        continue;
                                    decimal decInnerLoopValue = funcValueGetter(objLoopImprovement);
                                    if (decHighest < decInnerLoopValue)
                                    {
                                        decHighest = decInnerLoopValue;
                                        objHighestImprovement = objLoopImprovement;
                                    }
                                }

                                if (decHighest != decimal.MinValue)
                                {
                                    decLoopValue += decHighest;
                                    lstLoopImprovements.Add(objHighestImprovement);
                                }
                            }
                        }

                        if (blnValuesDictionaryContains)
                            dicValues[strLoopImprovedName] = decLoopValue;
                        else
                        {
                            dicValues.Add(strLoopImprovedName, decLoopValue);
                            dicImprovementsForValues.Add(strLoopImprovedName, lstLoopImprovements);
                        }
                    }
                }

                // Factor in Custom Improvements.
                dicUniqueNames.Clear();
                dicUniquePairs.Clear();
                Dictionary<string, decimal> dicCustomValues = new Dictionary<string, decimal>();
                Dictionary<string, List<Improvement>> dicCustomImprovementsForValues
                    = new Dictionary<string, List<Improvement>>();
                foreach (Improvement objImprovement in objCharacter.Improvements)
                {
                    if (!objImprovement.Custom || !objImprovement.Enabled ||
                        (blnUnconditionalOnly && !string.IsNullOrEmpty(objImprovement.Condition))) continue;
                    string strLoopImprovedName = objImprovement.ImprovedName;
                    bool blnAllowed = objImprovement.ImproveType == eImprovementType &&
                                      !((eImprovementType == Improvement.ImprovementType.MatrixInitiativeDice
                                         || eImprovementType == Improvement.ImprovementType.MatrixInitiative
                                         || eImprovementType == Improvement.ImprovementType.MatrixInitiativeDiceAdd)
                                        && objImprovement.ImproveSource == Improvement.ImprovementSource.Gear
                                        && objCharacter.ActiveCommlink is Gear objCommlink
                                        && objCommlink.Name == "Living Persona") &&
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
                            dicUniqueNames.Add(strLoopImprovedName, new HashSet<string> {strUniqueName});
                        }

                        // Add the values to the UniquePair List so we can check them later.
                        if (dicUniquePairs.TryGetValue(strLoopImprovedName,
                                                       out List<Tuple<string, Improvement>> lstUniquePairs))
                        {
                            lstUniquePairs.Add(new Tuple<string, Improvement>(strUniqueName, objImprovement));
                        }
                        else
                        {
                            dicUniquePairs.Add(strLoopImprovedName,
                                               new List<Tuple<string, Improvement>>(1)
                                                   {new Tuple<string, Improvement>(strUniqueName, objImprovement)});
                        }

                        if (!dicCustomValues.ContainsKey(strLoopImprovedName))
                        {
                            dicCustomValues.Add(strLoopImprovedName, 0);
                            dicCustomImprovementsForValues.Add(strLoopImprovedName, new List<Improvement>());
                        }
                    }
                    else if (dicCustomValues.TryGetValue(strLoopImprovedName, out decimal decExistingValue))
                    {
                        dicCustomValues[strLoopImprovedName] = decExistingValue + funcValueGetter(objImprovement);
                        dicCustomImprovementsForValues[strLoopImprovedName].Add(objImprovement);
                    }
                    else
                    {
                        dicCustomValues.Add(strLoopImprovedName, funcValueGetter(objImprovement));
                        dicCustomImprovementsForValues.Add(strLoopImprovedName,
                                                           new List<Improvement>(objImprovement.Yield()));
                    }
                }

                foreach (KeyValuePair<string, HashSet<string>> objLoopValuePair in dicUniqueNames)
                {
                    string strLoopImprovedName = objLoopValuePair.Key;
                    bool blnValuesDictionaryContains
                        = dicCustomValues.TryGetValue(strLoopImprovedName, out decimal decLoopValue);
                    if (blnValuesDictionaryContains)
                        dicImprovementsForValues.TryGetValue(strLoopImprovedName, out lstLoopImprovements);
                    else
                        lstLoopImprovements = new List<Improvement>();
                    if (dicUniquePairs.TryGetValue(strLoopImprovedName,
                                                   out List<Tuple<string, Improvement>> lstUniquePairs))
                    {
                        // Run through the list of UniqueNames and pick out the highest value for each one.
                        foreach (string strUniqueName in objLoopValuePair.Value)
                        {
                            Improvement objHighestImprovement = null;
                            decimal decHighest = decimal.MinValue;
                            foreach ((string strUnique, Improvement objLoopImprovement) in lstUniquePairs)
                            {
                                if (strUnique != strUniqueName)
                                    continue;
                                decimal decInnerLoopValue = funcValueGetter(objLoopImprovement);
                                if (decHighest < decInnerLoopValue)
                                {
                                    decHighest = decInnerLoopValue;
                                    objHighestImprovement = objLoopImprovement;
                                }
                            }

                            if (decHighest != decimal.MinValue)
                            {
                                decLoopValue += decHighest;
                                lstLoopImprovements.Add(objHighestImprovement);
                            }
                        }

                        if (blnValuesDictionaryContains)
                            dicCustomValues[strLoopImprovedName] = decLoopValue;
                        else
                        {
                            dicCustomValues.Add(strLoopImprovedName, decLoopValue);
                            dicCustomImprovementsForValues.Add(strLoopImprovedName, lstLoopImprovements);
                        }
                    }
                }

                foreach (KeyValuePair<string, decimal> objLoopValuePair in dicCustomValues)
                {
                    string strLoopImprovedName = objLoopValuePair.Key;
                    if (dicValues.TryGetValue(strLoopImprovedName, out decimal decExistingValue))
                    {
                        dicValues[strLoopImprovedName] = decExistingValue + objLoopValuePair.Value;
                        dicImprovementsForValues[strLoopImprovedName]
                            .AddRange(dicCustomImprovementsForValues[strLoopImprovedName]);
                    }
                    else
                    {
                        dicValues.Add(strLoopImprovedName, objLoopValuePair.Value);
                        dicImprovementsForValues.Add(strLoopImprovedName,
                                                     dicCustomImprovementsForValues[strLoopImprovedName]);
                    }
                }

                decimal decReturn = 0;

                // If this is the default ValueOf() call, let's cache the value we've calculated so that we don't have to do this all over again unless something has changed
                if (blnFetchAndCacheResults)
                {
                    foreach (KeyValuePair<string, decimal> objLoopValuePair in dicValues)
                    {
                        string strLoopImprovedName = objLoopValuePair.Key;
                        decimal decLoopValue = objLoopValuePair.Value;
                        Tuple<decimal, List<Improvement>> tupNewValue =
                            new Tuple<decimal, List<Improvement>>(decLoopValue,
                                                                  dicImprovementsForValues[strLoopImprovedName]);
                        if (dicCachedValuesToUse != null)
                        {
                            ImprovementDictionaryKey objLoopCacheKey =
                                new ImprovementDictionaryKey(objCharacter, eImprovementType, strLoopImprovedName);
                            if (!dicCachedValuesToUse.TryAdd(objLoopCacheKey, tupNewValue))
                            {
                                List<Improvement> lstTemp = dicCachedValuesToUse[objLoopCacheKey].Item2;
                                if (!ReferenceEquals(lstTemp, tupNewValue.Item2))
                                {
                                    lstTemp.Clear();
                                    lstTemp.AddRange(tupNewValue.Item2);
                                    tupNewValue = new Tuple<decimal, List<Improvement>>(decLoopValue, lstTemp);
                                }

                                dicCachedValuesToUse[objLoopCacheKey] = tupNewValue;
                            }
                        }

                        decReturn += decLoopValue;
                        lstUsedImprovements.AddRange(tupNewValue.Item2);
                    }
                }
                lstUsedImprovements = lstUsedImprovements.ToList(); // To make sure we do not inadvertently alter the cached list
                return decReturn;
            }
            finally
            {
                // As a final step, remove the tuple used to flag an improvement value as currently being cached
                s_SetCurrentlyCalculatingValues.Remove(tupMyValueToCheck);
                s_SetCurrentlyCalculatingValues.Remove(tupBlankValueToCheck);
            }
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
                string[] strValues = strValue.TrimStartOnce("FixedValues(", true).TrimEndOnce(')')
                                             .Split(',', StringSplitOptions.RemoveEmptyEntries);
                strValue = strValues[Math.Max(Math.Min(strValues.Length, intRating) - 1, 0)];
            }

            if (strValue.Contains("Rating") || AttributeSection.AttributeStrings.Any(strValue.Contains))
            {
                string strReturn = strValue.Replace("Rating", intRating.ToString(GlobalSettings.InvariantCultureInfo));
                // If the value contain an CharacterAttribute name, replace it with the character's CharacterAttribute.
                strReturn = objCharacter.AttributeSection.ProcessAttributesInXPath(strReturn);
                strReturn = strReturn.Replace("/", " div ");

                //Log.Info("strValue = " + strValue);
                //Log.Info("strReturn = " + strReturn);

                // Treat this as a decimal value so any fractions can be rounded down. This is currently only used by the Boosted Reflexes Cyberware from SR2050.
                object objProcess = CommonFunctions.EvaluateInvariantXPath(strReturn, out bool blnIsSuccess);
                int intValue = blnIsSuccess ? ((double)objProcess).StandardRound() : 0;

                //Log.Exit("ValueToInt");
                return intValue;
            }

            //Log.Exit("ValueToInt");
            int.TryParse(strValue, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out int intReturn);
            return intReturn;
        }

        /// <summary>
        /// Convert a string to a decimal, converting "Rating" to a number where appropriate.
        /// </summary>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="strValue">String value to parse.</param>
        /// <param name="intRating">Integer value to replace "Rating" with.</param>
        public static decimal ValueToDec(Character objCharacter, string strValue, int intRating)
        {
            if (string.IsNullOrEmpty(strValue))
                return 0;
            //         Log.Enter("ValueToInt");
            //         Log.Info("strValue = " + strValue);
            //Log.Info("intRating = " + intRating.ToString());
            if (strValue.StartsWith("FixedValues(", StringComparison.Ordinal))
            {
                string[] strValues = strValue.TrimStartOnce("FixedValues(", true).TrimEndOnce(')')
                                             .Split(',', StringSplitOptions.RemoveEmptyEntries);
                strValue = strValues[Math.Max(Math.Min(strValues.Length, intRating) - 1, 0)];
            }

            if (strValue.Contains("Rating") || AttributeSection.AttributeStrings.Any(strValue.Contains))
            {
                string strReturn = strValue.Replace("Rating", intRating.ToString(GlobalSettings.InvariantCultureInfo));
                // If the value contain an CharacterAttribute name, replace it with the character's CharacterAttribute.
                strReturn = objCharacter.AttributeSection.ProcessAttributesInXPath(strReturn);
                strReturn = strReturn.Replace("/", " div ");

                //Log.Info("strValue = " + strValue);
                //Log.Info("strReturn = " + strReturn);

                // Treat this as a decimal value so any fractions can be rounded down. This is currently only used by the Boosted Reflexes Cyberware from SR2050.
                object objProcess = CommonFunctions.EvaluateInvariantXPath(strReturn, out bool blnIsSuccess);
                decimal decValue = blnIsSuccess ? Convert.ToDecimal((double)objProcess) : 0;

                //Log.Exit("ValueToInt");
                return decValue;
            }

            //Log.Exit("ValueToInt");
            decimal.TryParse(strValue, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decimal decReturn);
            return decReturn;
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
                    setAllowedCategories = new HashSet<string>(strOnlyCategory.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()));
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
                    setForbiddenCategories = new HashSet<string>(strExcludeCategory.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()));
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
                        setAllowedNames = new HashSet<string>(strLimitToSkill.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()));
                    }
                }

                HashSet<string> setAllowedLinkedAttributes = null;
                string strLimitToAttribute = xmlBonusNode.SelectSingleNode("@limittoattribute")?.InnerText;
                if (!string.IsNullOrEmpty(strLimitToAttribute))
                {
                    setAllowedLinkedAttributes = new HashSet<string>(strLimitToAttribute.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()));
                }

                List<ListItem> lstDropdownItems = new List<ListItem>(objCharacter.SkillsSection.Skills.Count * 2);
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
                    StringBuilder sbdFilter = new StringBuilder();
                    if (setAllowedCategories?.Count > 0)
                    {
                        sbdFilter.Append('(');
                        foreach (string strCategory in setAllowedCategories)
                        {
                            sbdFilter.Append("category = ").Append(strCategory.CleanXPath()).Append(" or ");
                        }
                        sbdFilter.Length -= 4;
                        sbdFilter.Append(')');
                    }
                    if (setForbiddenCategories?.Count > 0)
                    {
                        sbdFilter.Append(sbdFilter.Length > 0 ? " and not(" : "not(");
                        foreach (string strCategory in setForbiddenCategories)
                        {
                            sbdFilter.Append("category = ").Append(strCategory.CleanXPath()).Append(" or ");
                        }
                        sbdFilter.Length -= 4;
                        sbdFilter.Append(')');
                    }
                    if (setAllowedNames?.Count > 0)
                    {
                        sbdFilter.Append(sbdFilter.Length > 0 ? " and (" : "(");
                        foreach (string strName in setAllowedNames)
                        {
                            sbdFilter.Append("name = ").Append(strName.CleanXPath()).Append(" or ");
                        }
                        sbdFilter.Length -= 4;
                        sbdFilter.Append(')');
                    }
                    if (setProcessedSkillNames.Count > 0)
                    {
                        sbdFilter.Append(sbdFilter.Length > 0 ? " and not(" : "not(");
                        foreach (string strName in setProcessedSkillNames)
                        {
                            sbdFilter.Append("name = ").Append(strName.CleanXPath()).Append(" or ");
                        }
                        sbdFilter.Length -= 4;
                        sbdFilter.Append(')');
                    }
                    if (setAllowedLinkedAttributes?.Count > 0)
                    {
                        sbdFilter.Append(sbdFilter.Length > 0 ? " and (" : "(");
                        foreach (string strAttribute in setAllowedLinkedAttributes)
                        {
                            sbdFilter.Append("attribute = ").Append(strAttribute.CleanXPath()).Append(" or ");
                        }
                        sbdFilter.Length -= 4;
                        sbdFilter.Append(')');
                    }

                    string strFilter = sbdFilter.Length > 0 ? ") and (" + sbdFilter : string.Empty;
                    foreach (XPathNavigator xmlSkill in objCharacter.LoadDataXPath("skills.xml").Select("/chummer/knowledgeskills/skill[(not(hide)" + strFilter + ")]"))
                    {
                        string strName = xmlSkill.SelectSingleNode("name")?.Value;
                        if (!string.IsNullOrEmpty(strName))
                            lstDropdownItems.Add(new ListItem(strName, xmlSkill.SelectSingleNode("translate")?.Value ?? strName));
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
                        ? string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("String_Improvement_SelectSkillNamed"), strFriendlyName)
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

        #endregion Helper Methods

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
            Log.Info("objImprovementSource = " + objImprovementSource);
            Log.Info("strSourceName = " + strSourceName);
            Log.Info("nodBonus = " + nodBonus?.OuterXml);
            Log.Info("intRating = " + intRating.ToString(GlobalSettings.InvariantCultureInfo));
            Log.Info("strFriendlyName = " + strFriendlyName);

            /*try
            {*/
            if (nodBonus == null)
            {
                _strForcedValue = string.Empty;
                _strLimitSelection = string.Empty;
                Log.Debug("CreateImprovements exit");
                return true;
            }

            _strSelectedValue = string.Empty;

            Log.Info("_strForcedValue = " + _strForcedValue);
            Log.Info("_strLimitSelection = " + _strLimitSelection);

            // If no friendly name was provided, use the one from SourceName.
            if (string.IsNullOrEmpty(strFriendlyName))
                strFriendlyName = strSourceName;

            if (nodBonus.HasChildNodes)
            {
                string strUnique = nodBonus.Attributes?["unique"]?.InnerText ?? string.Empty;
                Log.Info("Has Child Nodes");
                if (nodBonus["selecttext"] != null)
                {
                    Log.Info("selecttext");

                    if (!string.IsNullOrEmpty(_strForcedValue))
                    {
                        LimitSelection = _strForcedValue;
                    }
                    else if (objCharacter?.Pushtext.Count != 0)
                    {
                        LimitSelection = objCharacter?.Pushtext.Pop();
                    }

                    Log.Info("_strForcedValue = " + SelectedValue);
                    Log.Info("_strLimitSelection = " + LimitSelection);

                    if (!string.IsNullOrEmpty(LimitSelection))
                    {
                        _strSelectedValue = LimitSelection;
                    }
                    else if (nodBonus["selecttext"].Attributes.Count == 0)
                    {
                        // Display the Select Text window and record the value that was entered.
                        using (frmSelectText frmPickText = new frmSelectText
                        {
                            Description =
                                string.Format(GlobalSettings.CultureInfo,
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

                            _strSelectedValue = frmPickText.SelectedValue;
                        }
                    }
                    else if (objCharacter != null)
                    {
                        using (frmSelectItem frmSelect = new frmSelectItem
                        {
                            Description = string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("String_Improvement_SelectText"), strFriendlyName)
                        })
                        {
                            string strXPath = nodBonus.SelectSingleNode("selecttext/@xpath")?.Value;
                            if (string.IsNullOrEmpty(strXPath))
                            {
                                Rollback(objCharacter);
                                ForcedValue = string.Empty;
                                LimitSelection = string.Empty;
                                Log.Debug("CreateImprovements exit");
                                return false;
                            }
                            XPathNavigator xmlDoc = objCharacter.LoadDataXPath(nodBonus.SelectSingleNode("selecttext/@xml")?.Value);
                            List<ListItem> lstItems = new List<ListItem>(5);
                            foreach (XPathNavigator objNode in xmlDoc.Select(strXPath))
                            {
                                string strName = objNode.SelectSingleNodeAndCacheExpression("name")?.Value ?? string.Empty;
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
                                        objNode.SelectSingleNodeAndCacheExpression("translate")?.Value ?? strName));
                                }
                            }
                            //TODO: While this is a safeguard for uniques, preference should be that we're selecting distinct values in the xpath.
                            //Use XPath2.0 distinct-values operators instead. REQUIRES > .Net 4.6
                            lstItems = new List<ListItem>(lstItems.GroupBy(o => new { o.Value, o.Name }).Select(o => o.FirstOrDefault()));

                            if (lstItems.Count == 0)
                            {
                                Rollback(objCharacter);
                                ForcedValue = string.Empty;
                                LimitSelection = string.Empty;
                                Log.Debug("CreateImprovements exit");
                                return false;
                            }

                            if (Convert.ToBoolean(nodBonus.SelectSingleNode("selecttext/@allowedit")?.Value, GlobalSettings.InvariantCultureInfo))
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
                            _strSelectedValue = frmSelect.SelectedItem;
                        }
                    }
                    Log.Info("_strSelectedValue = " + SelectedValue);
                    Log.Info("strSourceName = " + strSourceName);

                    // Create the Improvement.
                    Log.Info("Calling CreateImprovement");

                    CreateImprovement(objCharacter, _strSelectedValue, objImprovementSource, strSourceName,
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
            _strForcedValue = string.Empty;
            _strLimitSelection = string.Empty;

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
                strSourceName, strUnique, _strForcedValue, _strLimitSelection, SelectedValue, strFriendlyName,
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
                _strForcedValue = container.ForcedValue;
                _strLimitSelection = container.LimitSelection;
                _strSelectedValue = container.SelectedValue;
            }
            else if (blnIgnoreMethodNotFound || bonusNode.ChildNodes.Count == 0)
            {
                return true;
            }
            else if (bonusNode.NodeType != XmlNodeType.Comment)
            {
                Utils.BreakIfDebug();
                Log.Warn(new object[] { "Tried to get unknown bonus", bonusNode.OuterXml });
                return false;
            }
            return true;
        }

        public static void EnableImprovements(Character objCharacter, IEnumerable<Improvement> objImprovementList)
        {
            EnableImprovements(objCharacter, objImprovementList.ToList());
        }

        public static void EnableImprovements(Character objCharacter, params Improvement[] objImprovementList)
        {
            EnableImprovements(objCharacter, Array.AsReadOnly(objImprovementList));
        }

        public static void EnableImprovements(Character objCharacter, IReadOnlyCollection<Improvement> objImprovementList)
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
                            foreach (KnowledgeSkill objKnowledgeSkill in objCharacter.SkillsSection.KnowsoftSkills.Where(x => x.InternalId == objImprovement.ImprovedName))
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
                        switch (objImprovement.UniqueName)
                        {
                            // Determine if access to any special tabs have been lost.
                            case "enabletab":
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

                                break;
                            // Determine if access to any special tabs has been regained
                            case "disabletab":
                                switch (objImprovement.ImprovedName)
                                {
                                    case "Cyberware":
                                        objCharacter.CyberwareDisabled = true;
                                        break;

                                    case "Initiation":
                                        objCharacter.InitiationForceDisabled = true;
                                        break;
                                }

                                break;
                        }
                        break;

                    case Improvement.ImprovementType.PrototypeTranshuman:
                        string strImprovedName = objImprovement.ImprovedName;
                        // Legacy compatibility
                        if (string.IsNullOrEmpty(strImprovedName))
                            objCharacter.PrototypeTranshuman = 1;
                        else
                            objCharacter.PrototypeTranshuman += Convert.ToDecimal(strImprovedName, GlobalSettings.InvariantCultureInfo);
                        break;

                    case Improvement.ImprovementType.Adapsin:
                        break;

                    case Improvement.ImprovementType.AddContact:
                        Contact objNewContact = objCharacter.Contacts.FirstOrDefault(c => c.UniqueId == objImprovement.ImprovedName);
                        if (objNewContact != null)
                        {
                            // TODO: Add code to enable disabled contact
                        }
                        break;

                    case Improvement.ImprovementType.Art:
                        Art objArt = objCharacter.Arts.FirstOrDefault(x => x.InternalId == objImprovement.ImprovedName);
                        if (objArt != null)
                        {
                            EnableImprovements(objCharacter, objCharacter.Improvements.Where(x => x.ImproveSource == objArt.SourceType && x.SourceName == objArt.InternalId && x.Enabled));
                        }
                        break;

                    case Improvement.ImprovementType.Metamagic:
                    case Improvement.ImprovementType.Echo:
                        Metamagic objMetamagic = objCharacter.Metamagics.FirstOrDefault(x => x.InternalId == objImprovement.ImprovedName);
                        if (objMetamagic != null)
                        {
                            Improvement.ImprovementSource eSource = objImprovement.ImproveType == Improvement.ImprovementType.Metamagic ? Improvement.ImprovementSource.Metamagic : Improvement.ImprovementSource.Echo;
                            EnableImprovements(objCharacter, objCharacter.Improvements.Where(x => x.ImproveSource == eSource && x.SourceName == objMetamagic.InternalId && x.Enabled));
                        }
                        break;

                    case Improvement.ImprovementType.CritterPower:
                        CritterPower objCritterPower = objCharacter.CritterPowers.FirstOrDefault(x => x.InternalId == objImprovement.ImprovedName || (x.Name == objImprovement.ImprovedName && x.Extra == objImprovement.UniqueName));
                        if (objCritterPower != null)
                        {
                            EnableImprovements(objCharacter, objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.CritterPower && x.SourceName == objCritterPower.InternalId && x.Enabled));
                        }
                        break;

                    case Improvement.ImprovementType.MentorSpirit:
                    case Improvement.ImprovementType.Paragon:
                        MentorSpirit objMentor = objCharacter.MentorSpirits.FirstOrDefault(x => x.InternalId == objImprovement.ImprovedName);
                        if (objMentor != null)
                        {
                            EnableImprovements(objCharacter, objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.MentorSpirit && x.SourceName == objMentor.InternalId && x.Enabled));
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
                            EnableImprovements(objCharacter, objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.Spell && x.SourceName == objSpell.InternalId && x.Enabled));
                        }
                        break;

                    case Improvement.ImprovementType.ComplexForm:
                        ComplexForm objComplexForm = objCharacter.ComplexForms.FirstOrDefault(x => x.InternalId == objImprovement.ImprovedName);
                        if (objComplexForm != null)
                        {
                            EnableImprovements(objCharacter, objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.ComplexForm && x.SourceName == objComplexForm.InternalId && x.Enabled));
                        }
                        break;

                    case Improvement.ImprovementType.MartialArt:
                        MartialArt objMartialArt = objCharacter.MartialArts.FirstOrDefault(x => x.InternalId == objImprovement.ImprovedName);
                        if (objMartialArt != null)
                        {
                            EnableImprovements(objCharacter, objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.MartialArt && x.SourceName == objMartialArt.InternalId && x.Enabled));
                            // Remove the Improvements for any Techniques for the Martial Art that is being removed.
                            foreach (MartialArtTechnique objTechnique in objMartialArt.Techniques)
                            {
                                EnableImprovements(objCharacter, objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.MartialArtTechnique && x.SourceName == objTechnique.InternalId && x.Enabled));
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

                            foreach (Skill objSkill in objCharacter.SkillsSection.Skills.Where(x => x.SkillCategory == strCategory))
                            {
                                objSkill.ForceDisabled = false;
                            }
                        }
                        break;

                    case Improvement.ImprovementType.SpecificQuality:
                        Quality objQuality = objCharacter.Qualities.FirstOrDefault(objLoopQuality => objLoopQuality.InternalId == objImprovement.ImprovedName);
                        if (objQuality != null)
                        {
                            EnableImprovements(objCharacter, objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.Quality && x.SourceName == objQuality.InternalId && x.Enabled));
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
                            DisableImprovements(objCharacter, objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.AIProgram && x.SourceName == objProgram.InternalId && x.Enabled));
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

        public static void DisableImprovements(Character objCharacter, IEnumerable<Improvement> objImprovementList)
        {
            DisableImprovements(objCharacter, objImprovementList.ToList());
        }

        public static void DisableImprovements(Character objCharacter, params Improvement[] objImprovementList)
        {
            DisableImprovements(objCharacter, Array.AsReadOnly(objImprovementList));
        }

        public static void DisableImprovements(Character objCharacter, IReadOnlyCollection<Improvement> objImprovementList)
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
                bool blnHasDuplicate = objCharacter.Improvements.Any(
                    x => x.UniqueName == objImprovement.UniqueName && x.ImprovedName == objImprovement.ImprovedName
                                                                   && x.ImproveType == objImprovement.ImproveType
                                                                   && x.SourceName != objImprovement.SourceName
                                                                   && x.Enabled);

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
                            objCharacter.SkillsSection.KnowsoftSkills.RemoveAll(
                                x => x.InternalId == objImprovement.ImprovedName);
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
                            switch (objImprovement.UniqueName)
                            {
                                case "enabletab":
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

                                    break;
                                // Determine if access to any special tabs has been regained
                                case "disabletab":
                                    switch (objImprovement.ImprovedName)
                                    {
                                        case "Cyberware":
                                            objCharacter.CyberwareDisabled = false;
                                            break;

                                        case "Initiation":
                                            objCharacter.InitiationForceDisabled = false;
                                            break;
                                    }

                                    break;
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
                            objCharacter.PrototypeTranshuman -= Convert.ToDecimal(strImprovedName, GlobalSettings.InvariantCultureInfo);
                        break;

                    case Improvement.ImprovementType.Adapsin:
                        break;

                    case Improvement.ImprovementType.AddContact:
                        Contact objNewContact = objCharacter.Contacts.FirstOrDefault(c => c.UniqueId == objImprovement.ImprovedName);
                        if (objNewContact != null)
                        {
                            // TODO: Add code to disable contact
                        }
                        break;

                    case Improvement.ImprovementType.Art:
                        Art objArt = objCharacter.Arts.FirstOrDefault(x => x.InternalId == objImprovement.ImprovedName);
                        if (objArt != null)
                        {
                            DisableImprovements(objCharacter, objCharacter.Improvements.Where(x => x.ImproveSource == objArt.SourceType && x.SourceName == objArt.InternalId && x.Enabled));
                        }
                        break;

                    case Improvement.ImprovementType.Metamagic:
                    case Improvement.ImprovementType.Echo:
                        Metamagic objMetamagic = objCharacter.Metamagics.FirstOrDefault(x => x.InternalId == objImprovement.ImprovedName);
                        if (objMetamagic != null)
                        {
                            Improvement.ImprovementSource eSource = objImprovement.ImproveType == Improvement.ImprovementType.Metamagic ? Improvement.ImprovementSource.Metamagic : Improvement.ImprovementSource.Echo;
                            DisableImprovements(objCharacter, objCharacter.Improvements.Where(x => x.ImproveSource == eSource && x.SourceName == objMetamagic.InternalId && x.Enabled));
                        }
                        break;

                    case Improvement.ImprovementType.CritterPower:
                        CritterPower objCritterPower = objCharacter.CritterPowers.FirstOrDefault(x => x.InternalId == objImprovement.ImprovedName || (x.Name == objImprovement.ImprovedName && x.Extra == objImprovement.UniqueName));
                        if (objCritterPower != null)
                        {
                            DisableImprovements(objCharacter, objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.CritterPower && x.SourceName == objCritterPower.InternalId && x.Enabled));
                        }
                        break;

                    case Improvement.ImprovementType.MentorSpirit:
                    case Improvement.ImprovementType.Paragon:
                        MentorSpirit objMentor = objCharacter.MentorSpirits.FirstOrDefault(x => x.InternalId == objImprovement.ImprovedName);
                        if (objMentor != null)
                        {
                            DisableImprovements(objCharacter, objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.MentorSpirit && x.SourceName == objMentor.InternalId && x.Enabled));
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
                            DisableImprovements(objCharacter, objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.Spell && x.SourceName == objSpell.InternalId && x.Enabled));
                        }
                        break;

                    case Improvement.ImprovementType.ComplexForm:
                        ComplexForm objComplexForm = objCharacter.ComplexForms.FirstOrDefault(x => x.InternalId == objImprovement.ImprovedName);
                        if (objComplexForm != null)
                        {
                            DisableImprovements(objCharacter, objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.ComplexForm && x.SourceName == objComplexForm.InternalId && x.Enabled));
                        }
                        break;

                    case Improvement.ImprovementType.MartialArt:
                        MartialArt objMartialArt = objCharacter.MartialArts.FirstOrDefault(x => x.InternalId == objImprovement.ImprovedName);
                        if (objMartialArt != null)
                        {
                            DisableImprovements(objCharacter, objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.MartialArt && x.SourceName == objMartialArt.InternalId && x.Enabled));
                            // Remove the Improvements for any Techniques for the Martial Art that is being removed.
                            foreach (MartialArtTechnique objTechnique in objMartialArt.Techniques)
                            {
                                DisableImprovements(objCharacter, objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.MartialArtTechnique && x.SourceName == objTechnique.InternalId && x.Enabled));
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
                            foreach (Improvement objLoopImprovement in GetCachedImprovementListForValueOf(objCharacter, Improvement.ImprovementType.SpecialSkills))
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

                            foreach (Skill objSkill in objCharacter.SkillsSection.Skills.Where(x => x.SkillCategory == strCategory))
                            {
                                objSkill.ForceDisabled = true;
                            }
                        }
                        break;

                    case Improvement.ImprovementType.SpecificQuality:
                        Quality objQuality = objCharacter.Qualities.FirstOrDefault(objLoopQuality => objLoopQuality.InternalId == objImprovement.ImprovedName);
                        if (objQuality != null)
                        {
                            DisableImprovements(objCharacter, objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.Quality && x.SourceName == objQuality.InternalId && x.Enabled));
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
                            DisableImprovements(objCharacter, objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.AIProgram && x.SourceName == objProgram.InternalId && x.Enabled));
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

            Log.Info("objImprovementSource = " + objImprovementSource);
            Log.Info("strSourceName = " + strSourceName);
            // A List of Improvements to hold all of the items that will eventually be deleted.
            List<Improvement> objImprovementList = (string.IsNullOrEmpty(strSourceName)
                ? objCharacter.Improvements.Where(objImprovement =>
                    objImprovement.ImproveSource == objImprovementSource)
                : objCharacter.Improvements.Where(objImprovement =>
                    objImprovement.ImproveSource == objImprovementSource && objImprovement.SourceName == strSourceName)).ToList();
            // Compatibility fix for when blnConcatSelectedValue was around
            if (strSourceName.IsGuid())
            {
                string strSpace = LanguageManager.GetString("String_Space");
                objImprovementList.AddRange(objCharacter.Improvements.Where(objImprovement => objImprovement.ImproveSource == objImprovementSource &&
                    (objImprovement.SourceName.StartsWith(strSourceName + strSpace, StringComparison.Ordinal) || objImprovement.SourceName.StartsWith(strSourceName + " ", StringComparison.Ordinal))));
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
                bool blnHasDuplicate = objCharacter.Improvements.Any(
                    x => x.UniqueName == objImprovement.UniqueName && x.ImprovedName == objImprovement.ImprovedName
                                                                   && x.ImproveType == objImprovement.ImproveType
                                                                   && (blnAllowDuplicatesFromSameSource
                                                                       || x.SourceName != objImprovement.SourceName));

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
                            objCharacter.SkillsSection.KnowledgeSkills.RemoveAll(
                                x => x.InternalId == objImprovement.ImprovedName);
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
                            switch (objImprovement.UniqueName)
                            {
                                case "enabletab":
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

                                    break;
                                // Determine if access to any special tabs has been regained
                                case "disabletab":
                                    switch (objImprovement.ImprovedName)
                                    {
                                        case "Cyberware":
                                            objCharacter.CyberwareDisabled = false;
                                            break;

                                        case "Initiation":
                                            objCharacter.InitiationForceDisabled = false;
                                            break;
                                    }

                                    break;
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
                            objCharacter.PrototypeTranshuman -= Convert.ToDecimal(strImprovedName, GlobalSettings.InvariantCultureInfo);
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
                        Contact objNewContact = objCharacter.Contacts.FirstOrDefault(c => c.UniqueId == objImprovement.ImprovedName);
                        if (objNewContact != null)
                            objCharacter.Contacts.Remove(objNewContact);
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
                        CritterPower objCritterPower = objCharacter.CritterPowers.FirstOrDefault(x => x.InternalId == objImprovement.ImprovedName || (x.Name == objImprovement.ImprovedName && x.Extra == objImprovement.UniqueName));
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
                            // Remove the Improvements for any Techniques for the Martial Art that is being removed.
                            foreach (MartialArtTechnique objTechnique in objMartialArt.Techniques)
                            {
                                decReturn += RemoveImprovements(objCharacter, Improvement.ImprovementSource.MartialArtTechnique, objTechnique.InternalId);
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
                            decReturn += objQuality.DeleteQuality(); // We need to add in the return cost of deleting the quality, so call this manually
                            objCharacter.Qualities.Remove(objQuality);
                        }
                        break;

                    case Improvement.ImprovementType.SkillSpecialization:
                    case Improvement.ImprovementType.SkillExpertise:
                        {
                            Skill objSkill = objCharacter.SkillsSection.GetActiveSkill(objImprovement.ImprovedName);
                            SkillSpecialization objSkillSpec = objImprovement.UniqueName.IsGuid()
                                ? objSkill?.Specializations.FirstOrDefault(x => x.InternalId == objImprovement.UniqueName)
                                // Kept for legacy reasons
                                : objSkill?.Specializations.FirstOrDefault(x => x.Name == objImprovement.UniqueName);
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
                                decReturn += objCyberware.CurrentTotalCost;
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
        /// <param name="decValue">Set a Value for the Improvement.</param>
        /// <param name="intRating">Set a Rating for the Improvement - typically used for Adept Powers.</param>
        /// <param name="intMinimum">Improve the Minimum for an CharacterAttribute by the given amount.</param>
        /// <param name="intMaximum">Improve the Maximum for an CharacterAttribute by the given amount.</param>
        /// <param name="decAugmented">Improve the Augmented value for an CharacterAttribute by the given amount.</param>
        /// <param name="intAugmentedMaximum">Improve the Augmented Maximum value for an CharacterAttribute by the given amount.</param>
        /// <param name="strExclude">A list of child items that should not receive the Improvement's benefit (typically for Skill Groups).</param>
        /// <param name="blnAddToRating">Whether or not the bonus applies to a Skill's Rating instead of the dice pool in general.</param>
        /// <param name="strTarget">What target the Improvement has, if any (e.g. a target skill whose attribute to replace).</param>
        /// <param name="strCondition">Condition for when the bonus is applied.</param>
        public static void CreateImprovement(Character objCharacter, string strImprovedName,
                                             Improvement.ImprovementSource objImprovementSource,
                                             string strSourceName, Improvement.ImprovementType objImprovementType,
                                             string strUnique,
                                             decimal decValue = 0, int intRating = 1, int intMinimum = 0,
                                             int intMaximum = 0, decimal decAugmented = 0,
                                             int intAugmentedMaximum = 0, string strExclude = "",
                                             bool blnAddToRating = false, string strTarget = "",
                                             string strCondition = "")
        {
            Log.Debug("CreateImprovement");
            Log.Info(
                "strImprovedName = " + strImprovedName);
            Log.Info(
                "objImprovementSource = " + objImprovementSource);
            Log.Info(
                "strSourceName = " + strSourceName);
            Log.Info(
                "objImprovementType = " + objImprovementType);
            Log.Info("strUnique = " + strUnique);
            Log.Info(
                "decValue = " + decValue.ToString(GlobalSettings.InvariantCultureInfo));
            Log.Info(
                "intRating = " + intRating.ToString(GlobalSettings.InvariantCultureInfo));
            Log.Info(
                "intMinimum = " + intMinimum.ToString(GlobalSettings.InvariantCultureInfo));
            Log.Info(
                "intMaximum = " + intMaximum.ToString(GlobalSettings.InvariantCultureInfo));
            Log.Info(
                "decAugmented = " + decAugmented.ToString(GlobalSettings.InvariantCultureInfo));
            Log.Info(
                "intAugmentedMaximum = " + intAugmentedMaximum.ToString(GlobalSettings.InvariantCultureInfo));
            Log.Info("strExclude = " + strExclude);
            Log.Info(
                "blnAddToRating = " + blnAddToRating.ToString(GlobalSettings.InvariantCultureInfo));
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
                    Value = decValue,
                    Rating = intRating,
                    Minimum = intMinimum,
                    Maximum = intMaximum,
                    Augmented = decAugmented,
                    AugmentedMaximum = intAugmentedMaximum,
                    Exclude = strExclude,
                    AddToRating = blnAddToRating,
                    Target = strTarget,
                    Condition = strCondition
                };
                // This is initially set to false make sure no property changers are triggered by the setters in the section above
                objImprovement.SetupComplete = true;
                // Add the Improvement to the list.
                objCharacter.Improvements.Add(objImprovement);
                ClearCachedValue(objCharacter, objImprovement.ImproveType, objImprovement.ImprovedName);

                // Add the Improvement to the Transaction List.
                if (!s_DictionaryTransactions.TryAdd(objCharacter, new List<TransactingImprovement>(1) { new TransactingImprovement(objImprovement) }))
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
                List<Improvement> lstImprovementsToProcess = new List<Improvement>(lstTransaction.Count);
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
                foreach (Improvement objTransactingImprovement in lstTransaction.ConvertAll(x => x.ImprovementObject))
                {
                    RemoveImprovements(objCharacter, objTransactingImprovement.ImproveSource, objTransactingImprovement.SourceName);
                    ClearCachedValue(objCharacter, objTransactingImprovement.ImproveType, objTransactingImprovement.ImprovedName);
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
            foreach (Improvement objImprovement in lstImprovements.Where(x => x.SetupComplete))
            {
                foreach ((INotifyMultiplePropertyChanged objToNotify, string strProperty) in objImprovement.GetRelevantPropertyChangers())
                {
                    if (dicPropertiesChanged.TryGetValue(objToNotify, out HashSet<string> setLoopPropertiesChanged))
                        setLoopPropertiesChanged.Add(strProperty);
                    else
                        dicPropertiesChanged.Add(objToNotify, new HashSet<string> { strProperty });
                }
            }
            // Fire each event once
            foreach (KeyValuePair<INotifyMultiplePropertyChanged, HashSet<string>> pairPropertiesChanged in dicPropertiesChanged)
                pairPropertiesChanged.Key.OnMultiplePropertyChanged(pairPropertiesChanged.Value);
        }

        #endregion Improvement System
    }
}
