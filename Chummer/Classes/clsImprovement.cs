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
using System.Text;
using static Chummer.Backend.Skills.SkillsSection;

namespace Chummer
{
    [DebuggerDisplay("{" + nameof(DisplayDebug) + "()}")]
    public class Improvement
    {
        private string DisplayDebug()
        {
            return $"{_objImprovementType} ({_intVal}, {_intRating}) 🡐 {_objImprovementSource}, {_strSourceName}, {_strImprovedName}";
        }

        public enum ImprovementType
        {
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
            ThrowRange,
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
            BornRich,
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
            Ambidextrous,
            UnarmedReach,
            SkillSpecialization,
            NativeLanguageLimit,
            AdeptPowerFreeLevels,
            AdeptPowerFreePoints,
            AIProgram,
            CritterPowerLevel,
            CritterPower,
            SwapSkillSpecAttribute,
            SpellResistance,
            LimitSpellCategory,
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
            KnowledgeSkillKarmaCost,
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
            NumImprovementTypes, // 🡐 This one should always be the last defined enum
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
            NumImprovementSources // 🡐 This one should always be the last defined enum
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
            if (strValue.Contains("InitiativePass"))
            {
                strValue = strValue.Replace("InitiativePass","InitiativeDice");
            }
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
            Log.Enter("Save");

            objWriter.WriteStartElement("improvement");
            if (!string.IsNullOrEmpty(_strUniqueName))
                objWriter.WriteElementString("unique", _strUniqueName);
            objWriter.WriteElementString("target", _strTarget);
            objWriter.WriteElementString("improvedname", _strImprovedName);
            objWriter.WriteElementString("sourcename", _strSourceName);
            objWriter.WriteElementString("min", _intMin.ToString());
            objWriter.WriteElementString("max", _intMax.ToString());
            objWriter.WriteElementString("aug", _intAug.ToString());
            objWriter.WriteElementString("augmax", _intAugMax.ToString());
            objWriter.WriteElementString("val", _intVal.ToString());
            objWriter.WriteElementString("rating", _intRating.ToString());
            objWriter.WriteElementString("exclude", _strExclude);
            objWriter.WriteElementString("condition", _strCondition);
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

            Log.Exit("Save");
        }

        /// <summary>
        /// Load the CharacterAttribute from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            if (objNode == null)
                return;
            Log.Enter("Load");

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
            objNode.TryGetBoolFieldQuickly("custom", ref _blnCustom);
            objNode.TryGetStringFieldQuickly("customname", ref _strCustomName);
            objNode.TryGetStringFieldQuickly("customid", ref _strCustomId);
            objNode.TryGetStringFieldQuickly("customgroup", ref _strCustomGroup);
            objNode.TryGetBoolFieldQuickly("addtorating", ref _blnAddToRating);
            objNode.TryGetBoolFieldQuickly("enabled", ref _blnEnabled);
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            objNode.TryGetInt32FieldQuickly("order", ref _intOrder);

            Log.Exit("Load");
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
        public IEnumerable<Action> GetRelevantPropertyChangers()
        {
            switch (ImproveType)
            {
                case ImprovementType.Attribute:
                {
                    string strTargetAttribute = ImprovedName;
                    bool blnIsBase = strTargetAttribute.EndsWith("Base");
                    if (blnIsBase)
                        strTargetAttribute = strTargetAttribute.TrimEndOnce("Base", true);
                    if (AugmentedMaximum != 0 || Maximum != 0 || Minimum != 0)
                    {
                        foreach (CharacterAttrib objCharacterAttrib in _objCharacter.AttributeSection.AttributeList.Concat(_objCharacter.AttributeSection.SpecialAttributeList))
                        {
                            if (objCharacterAttrib.Abbrev == strTargetAttribute)
                            {
                                yield return () => objCharacterAttrib.OnPropertyChanged(nameof(CharacterAttrib.AugmentedMetatypeLimits));
                            }
                        }
                    }
                    else if (Augmented != 0)
                    {
                        foreach (CharacterAttrib objCharacterAttrib in _objCharacter.AttributeSection.AttributeList.Concat(_objCharacter.AttributeSection.SpecialAttributeList))
                        {
                            if (objCharacterAttrib.Abbrev == strTargetAttribute)
                            {
                                yield return () => objCharacterAttrib.OnPropertyChanged(nameof(CharacterAttrib.AttributeModifiers));
                            }
                        }
                    }
                    else if (!blnIsBase && Value != 0)
                    {
                        foreach (CharacterAttrib objCharacterAttrib in _objCharacter.AttributeSection.AttributeList.Concat(_objCharacter.AttributeSection.SpecialAttributeList))
                        {
                            if (objCharacterAttrib.Abbrev == strTargetAttribute)
                            {
                                yield return () => objCharacterAttrib.OnPropertyChanged(nameof(CharacterAttrib.TotalValue));
                            }
                        }
                    }
                }
                    break;
                case ImprovementType.Armor:
                    break;
                case ImprovementType.FireArmor:
                    break;
                case ImprovementType.ColdArmor:
                    break;
                case ImprovementType.ElectricityArmor:
                    break;
                case ImprovementType.AcidArmor:
                    break;
                case ImprovementType.FallingArmor:
                    break;
                case ImprovementType.Dodge:
                    break;
                case ImprovementType.Reach:
                    break;
                case ImprovementType.Nuyen:
                    break;
                case ImprovementType.PhysicalCM:
                    break;
                case ImprovementType.StunCM:
                    break;
                case ImprovementType.UnarmedDV:
                    break;
                case ImprovementType.InitiativeDice:
                    break;
                case ImprovementType.MatrixInitiative:
                    break;
                case ImprovementType.MatrixInitiativeDice:
                    break;
                case ImprovementType.LifestyleCost:
                    break;
                case ImprovementType.CMThresholdOffset:
                case ImprovementType.CMSharedThresholdOffset:
                case ImprovementType.IgnoreCMPenaltyPhysical:
                case ImprovementType.IgnoreCMPenaltyStun:
                case ImprovementType.CMThreshold:
                {
                    yield return () => _objCharacter.RefreshWoundPenalties();
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
                    break;
                case ImprovementType.DisableCyberware:
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
                    yield return () => _objCharacter.SkillsSection.OnPropertyChanged(nameof(SkillsSection.HasKnowledgePoints));
                    yield return () => _objCharacter.SkillsSection.OnPropertyChanged(nameof(SkillsSection.KnowledgeSkillPointsRemain));
                }
                    break;
                case ImprovementType.NuyenMaxBP:
                    break;
                case ImprovementType.CMOverflow:
                    break;
                case ImprovementType.FreeSpiritPowerPoints:
                    break;
                case ImprovementType.AdeptPowerPoints:
                    break;
                case ImprovementType.ArmorEncumbrancePenalty:
                    break;
                case ImprovementType.Initiation:
                    break;
                case ImprovementType.Submersion:
                    break;
                case ImprovementType.Metamagic:
                    break;
                case ImprovementType.Echo:
                    break;
                case ImprovementType.Skillwire:
                {
                    foreach (Skill objSkill in _objCharacter.SkillsSection.Skills)
                    {
                        yield return () => objSkill.OnPropertyChanged(nameof(Skill.CyberwareRating));
                    }
                }
                    break;
                case ImprovementType.DamageResistance:
                    break;
                case ImprovementType.RestrictedItemCount:
                    break;
                case ImprovementType.JudgeIntentions:
                    break;
                case ImprovementType.JudgeIntentionsOffense:
                    break;
                case ImprovementType.JudgeIntentionsDefense:
                    break;
                case ImprovementType.LiftAndCarry:
                    break;
                case ImprovementType.Memory:
                    break;
                case ImprovementType.Concealability:
                    break;
                case ImprovementType.SwapSkillAttribute:
                case ImprovementType.SwapSkillSpecAttribute:
                {
                    Skill objTargetSkill = _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.Name == ImprovedName) ??
                                           (_objCharacter.SkillsSection.Skills.OfType<ExoticSkill>().FirstOrDefault(x => x.Name + " (" + x.Specific + ')' == ImprovedName) ??
                                            (Skill)_objCharacter.SkillsSection.KnowledgeSkills.FirstOrDefault(x => x.Name == ImprovedName || x.DisplayNameMethod(GlobalOptions.Language) == ImprovedName));
                    if (objTargetSkill != null)
                    {
                        yield return () => objTargetSkill.OnPropertyChanged(nameof(Skill.PoolToolTip));
                    }
                }
                    break;
                case ImprovementType.DrainResistance:
                    break;
                case ImprovementType.FadingResistance:
                    break;
                case ImprovementType.MatrixInitiativeDiceAdd:
                    break;
                case ImprovementType.InitiativeDiceAdd:
                    break;
                case ImprovementType.Composure:
                    break;
                case ImprovementType.UnarmedAP:
                    break;
                case ImprovementType.Restricted:
                    break;
                case ImprovementType.Notoriety:
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
                    foreach (Skill objSkill in _objCharacter.SkillsSection.Skills.Concat(_objCharacter.SkillsSection.KnowledgeSkills))
                    {
                        yield return () => objSkill.OnPropertyChanged(nameof(Skill.CyberwareRating));
                    }
                }
                    break;
                case ImprovementType.AddSprite:
                    break;
                case ImprovementType.BlackMarketDiscount:
                    break;
                case ImprovementType.ComplexFormLimit:
                    break;
                case ImprovementType.SpellLimit:
                    break;
                case ImprovementType.QuickeningMetamagic:
                    break;
                case ImprovementType.BasicLifestyleCost:
                    break;
                case ImprovementType.ThrowSTR:
                    break;
                case ImprovementType.EssenceMax:
                {
                    foreach (CharacterAttrib objCharacterAttrib in _objCharacter.AttributeSection.AttributeList.Concat(_objCharacter.AttributeSection.SpecialAttributeList))
                    {
                        if (objCharacterAttrib.Abbrev == "ESS")
                        {
                            yield return () => objCharacterAttrib.OnPropertyChanged(nameof(CharacterAttrib.MetatypeMaximum));
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
                    break;
                case ImprovementType.MentalLimit:
                    break;
                case ImprovementType.SocialLimit:
                    break;
                case ImprovementType.FriendsInHighPlaces:
                    break;
                case ImprovementType.Erased:
                    break;
                case ImprovementType.BornRich:
                    break;
                case ImprovementType.Fame:
                    break;
                case ImprovementType.MadeMan:
                    break;
                case ImprovementType.Overclocker:
                    break;
                case ImprovementType.RestrictedGear:
                    break;
                case ImprovementType.TrustFund:
                    break;
                case ImprovementType.ExCon:
                    break;
                case ImprovementType.ContactForceGroup:
                    break;
                case ImprovementType.Attributelevel:
                {
                    string strTargetAttribute = ImprovedName;
                    foreach (CharacterAttrib objCharacterAttrib in _objCharacter.AttributeSection.AttributeList.Concat(_objCharacter.AttributeSection.SpecialAttributeList))
                    {
                        if (objCharacterAttrib.Abbrev == strTargetAttribute)
                        {
                            yield return () => objCharacterAttrib.OnPropertyChanged(nameof(CharacterAttrib.FreeBase));
                        }
                    }
                }
                    break;
                case ImprovementType.AddContact:
                    break;
                case ImprovementType.Seeker:
                {
                    yield return () => _objCharacter.RefreshRedliner();
                }
                    break;
                case ImprovementType.PublicAwareness:
                    break;
                case ImprovementType.PrototypeTranshuman:
                    break;
                case ImprovementType.Hardwire:
                {
                    Skill objTargetSkill = _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.Name == ImprovedName) ??
                                           (_objCharacter.SkillsSection.Skills.OfType<ExoticSkill>().FirstOrDefault(x => x.Name + " (" + x.Specific + ')' == ImprovedName) ??
                                            (Skill) _objCharacter.SkillsSection.KnowledgeSkills.FirstOrDefault(x => x.InternalId == ImprovedName || x.DisplayNameMethod(GlobalOptions.Language) == ImprovedName));
                    if (objTargetSkill != null)
                    {
                        yield return () => objTargetSkill.OnPropertyChanged(nameof(Skill.CyberwareRating));
                    }
                }
                    break;
                case ImprovementType.DealerConnection:
                    break;
                case ImprovementType.Skill:
                {
                    Skill objTargetSkill = _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.Name == ImprovedName) ??
                                           _objCharacter.SkillsSection.Skills.OfType<ExoticSkill>().FirstOrDefault(x => x.Name + " (" + x.Specific + ')' == ImprovedName);
                    if (objTargetSkill != null)
                    {
                        yield return () => objTargetSkill.OnPropertyChanged(nameof(Skill.Base));
                    }
                }
                    break;
                case ImprovementType.SkillGroup:
                case ImprovementType.BlockSkillDefault:
                {
                    foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills.Where(x => x.SkillGroup == ImprovedName))
                    {
                        yield return () => objTargetSkill.OnPropertyChanged(nameof(Skill.DisplayPool));
                    }
                }
                    break;
                case ImprovementType.SkillCategory:
                {
                    foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills.Concat(_objCharacter.SkillsSection.KnowledgeSkills).Where(x => x.SkillCategory == ImprovedName))
                    {
                        yield return () => objTargetSkill.OnPropertyChanged(nameof(Skill.DisplayPool));
                    }
                }
                    break;
                case ImprovementType.SkillLinkedAttribute:
                {
                    foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills.Concat(_objCharacter.SkillsSection.KnowledgeSkills).Where(x => x.Attribute == ImprovedName))
                    {
                        yield return () => objTargetSkill.OnPropertyChanged(nameof(Skill.DisplayPool));
                    }
                }
                    break;
                case ImprovementType.SkillLevel:
                {
                    Skill objTargetSkill = _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.Name == ImprovedName) ??
                                           _objCharacter.SkillsSection.Skills.OfType<ExoticSkill>().FirstOrDefault(x => x.Name + " (" + x.Specific + ')' == ImprovedName);
                    if (objTargetSkill != null)
                    {
                        yield return () => objTargetSkill.OnPropertyChanged(nameof(Skill.FreeKarma));
                    }
                }
                    break;
                case ImprovementType.SkillGroupLevel:
                {
                    SkillGroup objTargetGroup = _objCharacter.SkillsSection.SkillGroups.FirstOrDefault(x => x.Name == ImprovedName);
                    if (objTargetGroup != null)
                    {
                        yield return () => objTargetGroup.OnPropertyChanged(nameof(SkillGroup.FreeLevels));
                        yield return () => objTargetGroup.OnPropertyChanged(nameof(SkillGroup.Karma));
                        yield return () => objTargetGroup.OnPropertyChanged(nameof(SkillGroup.Base));
                    }
                }
                    break;
                case ImprovementType.SkillBase:
                {
                    Skill objTargetSkill = _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.Name == ImprovedName) ??
                                           _objCharacter.SkillsSection.Skills.OfType<ExoticSkill>().FirstOrDefault(x => x.Name + " (" + x.Specific + ')' == ImprovedName);
                    if (objTargetSkill != null)
                    {
                        yield return () => objTargetSkill.OnPropertyChanged(nameof(Skill.FreeBase));
                    }
                }
                    break;
                case ImprovementType.SkillGroupBase:
                {
                    SkillGroup objTargetGroup = _objCharacter.SkillsSection.SkillGroups.FirstOrDefault(x => x.Name == ImprovedName);
                    if (objTargetGroup != null)
                    {
                        yield return () => objTargetGroup.OnPropertyChanged(nameof(SkillGroup.FreeBase));
                        yield return () => objTargetGroup.OnPropertyChanged(nameof(SkillGroup.Karma));
                        yield return () => objTargetGroup.OnPropertyChanged(nameof(SkillGroup.Base));
                    }
                }
                    break;
                case ImprovementType.Skillsoft:
                {
                    KnowledgeSkill objTargetSkill = _objCharacter.SkillsSection.KnowledgeSkills.FirstOrDefault(x => x.InternalId == ImprovedName || x.DisplayNameMethod(GlobalOptions.Language) == ImprovedName);
                    if (objTargetSkill != null)
                    {
                        yield return () => objTargetSkill.OnPropertyChanged(nameof(Skill.CyberwareRating));
                    }
                }
                    break;
                case ImprovementType.Activesoft:
                {
                    Skill objTargetSkill = _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.Name == ImprovedName) ??
                                           _objCharacter.SkillsSection.Skills.OfType<ExoticSkill>().FirstOrDefault(x => x.Name + " (" + x.Specific + ')' == ImprovedName);
                    if (objTargetSkill != null)
                    {
                        yield return () => objTargetSkill.OnPropertyChanged(nameof(Skill.CyberwareRating));
                    }
                }
                    break;
                case ImprovementType.ReplaceAttribute:
                {
                    string strTargetAttribute = ImprovedName;
                    foreach (CharacterAttrib objCharacterAttrib in _objCharacter.AttributeSection.AttributeList.Concat(_objCharacter.AttributeSection.SpecialAttributeList))
                    {
                        if (objCharacterAttrib.Abbrev == strTargetAttribute)
                        {
                            yield return () => objCharacterAttrib.OnPropertyChanged(nameof(CharacterAttrib.AugmentedMetatypeLimits));
                        }
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
                        yield return () => objSkill.OnPropertyChanged(nameof(Skill.PoolModifiers));
                    }
                }
                    break;
                case ImprovementType.Ambidextrous:
                    break;
                case ImprovementType.UnarmedReach:
                    break;
                case ImprovementType.SkillSpecialization:
                    break;
                case ImprovementType.NativeLanguageLimit:
                    break;
                case ImprovementType.AdeptPowerFreeLevels:
                    break;
                case ImprovementType.AdeptPowerFreePoints:
                    break;
                case ImprovementType.AIProgram:
                    break;
                case ImprovementType.CritterPowerLevel:
                    break;
                case ImprovementType.CritterPower:
                    break;
                case ImprovementType.SpellResistance:
                    break;
                case ImprovementType.LimitSpellCategory:
                    break;
                case ImprovementType.LimitSpellDescriptor:
                    break;
                case ImprovementType.LimitSpiritCategory:
                    break;
                case ImprovementType.WalkSpeed:
                    break;
                case ImprovementType.RunSpeed:
                    break;
                case ImprovementType.SprintSpeed:
                    break;
                case ImprovementType.WalkMultiplier:
                    break;
                case ImprovementType.RunMultiplier:
                    break;
                case ImprovementType.SprintBonus:
                    break;
                case ImprovementType.WalkMultiplierPercent:
                    break;
                case ImprovementType.RunMultiplierPercent:
                    break;
                case ImprovementType.SprintBonusPercent:
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
                    // TODO: Change essence loss improvement regeneration to take place only when Essence-related improvements or Cyberware is changed instead of on every character update.
                    yield return () => _objCharacter.RefreshEssenceLossImprovements();
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
                    Skill objTargetSkill = _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.Name == ImprovedName) ??
                                           _objCharacter.SkillsSection.Skills.OfType<ExoticSkill>().FirstOrDefault(x => x.Name + " (" + x.Specific + ')' == ImprovedName);
                    if (objTargetSkill != null)
                    {
                        yield return () => objTargetSkill.OnPropertyChanged(nameof(Skill.DisplayPool));
                    }
                }
                    break;
                case ImprovementType.PhysiologicalAddictionFirstTime:
                    break;
                case ImprovementType.PsychologicalAddictionFirstTime:
                    break;
                case ImprovementType.PhysiologicalAddictionAlreadyAddicted:
                    break;
                case ImprovementType.PsychologicalAddictionAlreadyAddicted:
                    break;
                case ImprovementType.StunCMRecovery:
                    break;
                case ImprovementType.PhysicalCMRecovery:
                    break;
                case ImprovementType.AddESStoStunCMRecovery:
                    break;
                case ImprovementType.AddESStoPhysicalCMRecovery:
                    break;
                case ImprovementType.MentalManipulationResist:
                    break;
                case ImprovementType.PhysicalManipulationResist:
                    break;
                case ImprovementType.ManaIllusionResist:
                    break;
                case ImprovementType.PhysicalIllusionResist:
                    break;
                case ImprovementType.DetectionSpellResist:
                    break;
                case ImprovementType.AddLimb:
                {
                    if (!_objCharacter.Options.DontUseCyberlimbCalculation && _objCharacter.Cyberware.Any(objCyberware => objCyberware.Category == "Cyberlimb" && !string.IsNullOrWhiteSpace(objCyberware.LimbSlot) && !_objCharacter.Options.ExcludeLimbSlot.Contains(objCyberware.LimbSlot)))
                    {
                        foreach (CharacterAttrib objCharacterAttrib in _objCharacter.AttributeSection.AttributeList.Concat(_objCharacter.AttributeSection.SpecialAttributeList))
                        {
                            if (objCharacterAttrib.Abbrev == "AGI" || objCharacterAttrib.Abbrev == "STR")
                            {
                                yield return () => objCharacterAttrib.OnPropertyChanged(nameof(CharacterAttrib.TotalValue));
                            }
                        }
                    }
                }
                    break;
                case ImprovementType.StreetCredMultiplier:
                    break;
                case ImprovementType.StreetCred:
                    break;
                case ImprovementType.AttributeKarmaCostMultiplier:
                case ImprovementType.AttributeKarmaCost:
                {
                    if (!string.IsNullOrEmpty(ImprovedName))
                    {
                        string strTargetAttribute = ImprovedName;
                        foreach (CharacterAttrib objCharacterAttrib in _objCharacter.AttributeSection.AttributeList.Concat(_objCharacter.AttributeSection.SpecialAttributeList))
                        {
                            if (objCharacterAttrib.Abbrev == strTargetAttribute)
                            {
                                yield return () => objCharacterAttrib.OnPropertyChanged(nameof(CharacterAttrib.UpgradeKarmaCost));
                            }
                        }
                    }
                    else
                    {
                        foreach (CharacterAttrib objCharacterAttrib in _objCharacter.AttributeSection.AttributeList.Concat(_objCharacter.AttributeSection.SpecialAttributeList))
                        {
                            yield return () => objCharacterAttrib.OnPropertyChanged(nameof(CharacterAttrib.UpgradeKarmaCost));
                        }
                    }
                }
                    break;
                case ImprovementType.ActiveSkillKarmaCost:
                case ImprovementType.ActiveSkillKarmaCostMultiplier:
                {
                    if (!string.IsNullOrEmpty(ImprovedName))
                    {
                        Skill objTargetSkill = _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.Name == ImprovedName) ??
                                               _objCharacter.SkillsSection.Skills.OfType<ExoticSkill>().FirstOrDefault(x => x.Name + " (" + x.Specific + ')' == ImprovedName);
                        if (objTargetSkill != null)
                        {
                            yield return () => objTargetSkill.OnPropertyChanged(nameof(Skill.UpgradeKarmaCost));
                        }
                    }
                    else
                    {
                        foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills)
                        {
                            yield return () => objTargetSkill.OnPropertyChanged(nameof(Skill.UpgradeKarmaCost));
                        }
                    }
                }
                    break;
                case ImprovementType.KnowledgeSkillKarmaCost:
                case ImprovementType.KnowledgeSkillKarmaCostMultiplier:
                {
                    if (!string.IsNullOrEmpty(ImprovedName))
                    {
                        KnowledgeSkill objTargetSkill = _objCharacter.SkillsSection.KnowledgeSkills.FirstOrDefault(x => x.Name == ImprovedName || x.DisplayNameMethod(GlobalOptions.Language) == ImprovedName);
                        if (objTargetSkill != null)
                        {
                            yield return () => objTargetSkill.OnPropertyChanged(nameof(Skill.UpgradeKarmaCost));
                        }
                    }
                    else
                    {
                        foreach (KnowledgeSkill objTargetSkill in _objCharacter.SkillsSection.KnowledgeSkills)
                        {
                            yield return () => objTargetSkill.OnPropertyChanged(nameof(Skill.UpgradeKarmaCost));
                        }
                    }
                }
                    break;
                case ImprovementType.SkillGroupKarmaCost:
                case ImprovementType.SkillGroupKarmaCostMultiplier:
                {
                    if (!string.IsNullOrEmpty(ImprovedName))
                    {
                        SkillGroup objTargetGroup = _objCharacter.SkillsSection.SkillGroups.FirstOrDefault(x => x.Name == ImprovedName);
                        if (objTargetGroup != null)
                        {
                            yield return () => objTargetGroup.OnPropertyChanged(nameof(SkillGroup.UpgradeKarmaCost));
                        }
                    }
                    else
                    {
                        foreach (SkillGroup objTargetGroup in _objCharacter.SkillsSection.SkillGroups)
                        {
                            yield return () => objTargetGroup.OnPropertyChanged(nameof(SkillGroup.UpgradeKarmaCost));
                        }
                    }
                }
                    break;
                case ImprovementType.SkillGroupDisable:
                {
                    SkillGroup objTargetGroup = _objCharacter.SkillsSection.SkillGroups.FirstOrDefault(x => x.Name == ImprovedName);
                    if (objTargetGroup != null)
                    {
                        yield return () => objTargetGroup.OnPropertyChanged(nameof(SkillGroup.IsDisabled));
                        yield return () => objTargetGroup.OnPropertyChanged(nameof(SkillGroup.Rating));
                        yield return () => objTargetGroup.OnPropertyChanged(nameof(SkillGroup.BaseUnbroken));
                        yield return () => objTargetGroup.OnPropertyChanged(nameof(SkillGroup.KarmaUnbroken));
                        yield return () => objTargetGroup.OnPropertyChanged(nameof(SkillGroup.Karma));
                        yield return () => objTargetGroup.OnPropertyChanged(nameof(SkillGroup.Base));
                    }
                }
                    break;
                case ImprovementType.SkillCategorySpecializationKarmaCost:
                case ImprovementType.SkillCategorySpecializationKarmaCostMultiplier:
                {
                    foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills.Concat(_objCharacter.SkillsSection.KnowledgeSkills).Where(x => x.SkillCategory == ImprovedName))
                    {
                        yield return () => objTargetSkill.OnPropertyChanged(nameof(Skill.CanAffordSpecialization));
                    }
                }
                    break;
                case ImprovementType.SkillCategoryKarmaCost:
                case ImprovementType.SkillCategoryKarmaCostMultiplier:
                {
                    foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills.Concat(_objCharacter.SkillsSection.KnowledgeSkills).Where(x => x.SkillCategory == ImprovedName))
                    {
                        yield return () => objTargetSkill.OnPropertyChanged(nameof(Skill.UpgradeKarmaCost));
                    }
                }
                    break;
                case ImprovementType.SkillGroupCategoryDisable:
                {
                    foreach (SkillGroup objTargetGroup in _objCharacter.SkillsSection.SkillGroups.Where(x => x.GetRelevantSkillCategories.Contains(ImprovedName)))
                    {
                        yield return () => objTargetGroup.OnPropertyChanged(nameof(SkillGroup.IsDisabled));
                        yield return () => objTargetGroup.OnPropertyChanged(nameof(SkillGroup.Rating));
                        yield return () => objTargetGroup.OnPropertyChanged(nameof(SkillGroup.BaseUnbroken));
                        yield return () => objTargetGroup.OnPropertyChanged(nameof(SkillGroup.KarmaUnbroken));
                        yield return () => objTargetGroup.OnPropertyChanged(nameof(SkillGroup.Karma));
                        yield return () => objTargetGroup.OnPropertyChanged(nameof(SkillGroup.Base));
                    }
                }
                    break;
                case ImprovementType.SkillGroupCategoryKarmaCostMultiplier:
                case ImprovementType.SkillGroupCategoryKarmaCost:
                {
                    foreach (SkillGroup objTargetGroup in _objCharacter.SkillsSection.SkillGroups.Where(x => x.GetRelevantSkillCategories.Contains(ImprovedName)))
                    {
                        yield return () => objTargetGroup.OnPropertyChanged(nameof(SkillGroup.UpgradeKarmaCost));
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
                    break;
                case ImprovementType.NewComplexFormKarmaCost:
                case ImprovementType.NewComplexFormKarmaCostMultiplier:
                    break;
                case ImprovementType.NewAIProgramKarmaCost:
                case ImprovementType.NewAIProgramKarmaCostMultiplier:
                    break;
                case ImprovementType.NewAIAdvancedProgramKarmaCost:
                case ImprovementType.NewAIAdvancedProgramKarmaCostMultiplier:
                    break;
                case ImprovementType.BlockSkillSpecializations:
                {
                    if (!string.IsNullOrEmpty(ImprovedName))
                    {
                        Skill objTargetSkill = _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.Name == ImprovedName) ??
                                               _objCharacter.SkillsSection.Skills.OfType<ExoticSkill>().FirstOrDefault(x => x.Name + " (" + x.Specific + ')' == ImprovedName);
                        if (objTargetSkill != null)
                        {
                            yield return () => objTargetSkill.OnPropertyChanged(nameof(Skill.CanHaveSpecs));
                        }
                    }
                    else
                    {
                        foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills)
                        {
                            yield return () => objTargetSkill.OnPropertyChanged(nameof(Skill.CanHaveSpecs));
                        }
                    }
                }
                    break;
                case ImprovementType.BlockSkillCategorySpecializations:
                {
                    foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills.Concat(_objCharacter.SkillsSection.KnowledgeSkills).Where(x => x.SkillCategory == ImprovedName))
                    {
                        yield return () => objTargetSkill.OnPropertyChanged(nameof(Skill.CanHaveSpecs));
                    }
                }
                    break;
                case ImprovementType.FocusBindingKarmaCost:
                    break;
                case ImprovementType.FocusBindingKarmaMultiplier:
                    break;
                case ImprovementType.MagiciansWayDiscount:
                    break;
                case ImprovementType.BurnoutsWay:
                    break;
                case ImprovementType.ContactForcedLoyalty:
                    break;
                case ImprovementType.ContactMakeFree:
                    break;
                case ImprovementType.FreeWare:
                    break;
                case ImprovementType.WeaponSkillAccuracy:
                    break;
                case ImprovementType.WeaponAccuracy:
                    break;
                case ImprovementType.MetageneticLimit:
                    break;
            }
        }

        #region UI Methods
        public TreeNode CreateTreeNode(ContextMenuStrip cmsImprovement)
        {
            TreeNode nodImprovement = new TreeNode
            {
                Tag = SourceName,
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
    }

    public struct ImprovementDictionaryKey
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
                return CharacterObject == objOtherImprovementDictionaryKey.CharacterObject &&
                       ImprovementType == objOtherImprovementDictionaryKey.ImprovementType &&
                       ImprovementName == objOtherImprovementDictionaryKey.ImprovementName;
            }
            if (obj is Tuple<Character, Improvement.ImprovementType, string> objOtherTuple)
            {
                return CharacterObject == objOtherTuple.Item1 &&
                       ImprovementType == objOtherTuple.Item2 &&
                       ImprovementName == objOtherTuple.Item3;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return CharacterObject.GetHashCode() + ImprovementType.GetHashCode() + ImprovementName.GetHashCode();
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
        // String that will be used to limit the selection in Pick forms.
        private static string s_StrLimitSelection = string.Empty;

        private static string s_StrSelectedValue = string.Empty;
        private static string s_StrForcedValue = string.Empty;
        private static readonly ConcurrentDictionary<Character, List<TransactingImprovement>> s_DictionaryTransactions = new ConcurrentDictionary<Character, List<TransactingImprovement>>(8, 10);
        private static readonly ConcurrentDictionary<ImprovementDictionaryKey, int> s_DictionaryCachedValues = new ConcurrentDictionary<ImprovementDictionaryKey, int>(8, (int)Improvement.ImprovementType.NumImprovementTypes);
        private static readonly ConcurrentDictionary<ImprovementDictionaryKey, int> s_DictionaryCachedAugmentedValues = new ConcurrentDictionary<ImprovementDictionaryKey, int>(8, (int)Improvement.ImprovementType.NumImprovementTypes);

        #region Properties
        /// <summary>
        /// Limit what can be selected in Pick forms to a single value. This is typically used when selecting the Qualities for a Metavariant that has a specifiec
        /// CharacterAttribute selection for Qualities like Metagenetic Improvement.
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
        public static int ValueOf(Character objCharacter, Improvement.ImprovementType objImprovementType, bool blnAddToRating = false, string strImprovedName = "", bool blnUnconditionalOnly = true)
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
            if (strValue.StartsWith("FixedValues("))
            {
                string[] strValues = strValue.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                strValue = strValues[Math.Max(Math.Min(strValues.Length, intRating) - 1, 0)];
            }
            if (strValue.Contains("Rating") || AttributeSection.AttributeStrings.Any(strValue.Contains))
            {
                string strReturn = strValue.Replace("Rating", intRating.ToString());
                // If the value contain an CharacterAttribute name, replace it with the character's CharacterAttribute.
                foreach (string strAttribute in AttributeSection.AttributeStrings)
                {
                    strReturn = strReturn.CheapReplace(strAttribute, () => objCharacter.GetAttribute(strAttribute).TotalValue.ToString());
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
            int.TryParse(strValue, out int intReturn);
            return intReturn;
        }

        public static string DoSelectSkill(XmlNode xmlBonusNode, Character objCharacter, int intRating, string strFriendlyName, ref bool blnIsKnowledgeSkill)
        {
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
                    setAllowedNames = new HashSet<string> {ForcedValue};
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
                            lstDropdownItems.Add(new ListItem(objKnowledgeSkill.Name, objKnowledgeSkill.DisplayNameMethod(GlobalOptions.Language)));
                        }
                    }
                    setProcessedSkillNames.Add(objKnowledgeSkill.Name);
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
                    using (XmlNodeList xmlSkillList = XmlManager.Load("skills.xml", GlobalOptions.Language).SelectNodes("/chummer/knowledgeskills/skill[(not(hide)" + strFilter + ")]"))
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

                frmSelectItem frmPickSkill = new frmSelectItem
                {
                    Description = LanguageManager.GetString("Title_SelectSkill", GlobalOptions.Language)
                };
                if (setAllowedNames != null)
                    frmPickSkill.GeneralItems = lstDropdownItems;
                else
                    frmPickSkill.DropdownItems = lstDropdownItems;
                
                frmPickSkill.ShowDialog();

                if (frmPickSkill.DialogResult == DialogResult.Cancel)
                {
                    throw new AbortedException();
                }

                strSelectedSkill = frmPickSkill.SelectedItem;
            }
            else
            {
                // Display the Select Skill window and record which Skill was selected.
                frmSelectSkill frmPickSkill = new frmSelectSkill(objCharacter, strFriendlyName)
                {
                    Description = !string.IsNullOrEmpty(strFriendlyName)
                        ? LanguageManager.GetString("String_Improvement_SelectSkillNamed", GlobalOptions.Language).Replace("{0}", strFriendlyName)
                        : LanguageManager.GetString("String_Improvement_SelectSkill", GlobalOptions.Language)
                };
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

                frmPickSkill.ShowDialog();

                // Make sure the dialogue window was not canceled.
                if (frmPickSkill.DialogResult == DialogResult.Cancel)
                {
                    throw new AbortedException();
                }

                strSelectedSkill = frmPickSkill.SelectedSkill;
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
        /// <param name="nodBonus">bonus XMLXode from the source data file.</param>
        /// <param name="blnConcatSelectedValue">Whether or not any selected values should be concatinated with the SourceName string when storing.</param>
        /// <param name="intRating">Selected Rating value that is used to replace the Rating string in an Improvement.</param>
        /// <param name="strFriendlyName">Friendly name to show in any dialogue windows that ask for a value.</param>
        /// <param name="blnAddImprovementsToCharacter">If True, adds created improvements to the character. Set to false if all we need is a SelectedValue.</param>
        /// <returns>True if successfull</returns>
        public static bool CreateImprovements(Character objCharacter, Improvement.ImprovementSource objImprovementSource, string strSourceName,
            XmlNode nodBonus, bool blnConcatSelectedValue = false, int intRating = 1, string strFriendlyName = "", bool blnAddImprovementsToCharacter = true)
        {
            Log.Enter("CreateImprovements");
            Log.Info("objImprovementSource = " + objImprovementSource.ToString());
            Log.Info("strSourceName = " + strSourceName);
            Log.Info("nodBonus = " + nodBonus?.OuterXml);
            Log.Info("blnConcatSelectedValue = " + blnConcatSelectedValue.ToString());
            Log.Info("intRating = " + intRating.ToString());
            Log.Info("strFriendlyName = " + strFriendlyName);
            Log.Info("intRating = " + intRating.ToString());

            /*try
            {*/
            if (nodBonus == null)
            {
                s_StrForcedValue = string.Empty;
                s_StrLimitSelection = string.Empty;
                Log.Exit("CreateImprovements");
                return true;
            }
            
            s_StrSelectedValue = string.Empty;

            Log.Info("_strForcedValue = " + s_StrForcedValue);
            Log.Info("_strLimitSelection = " + s_StrLimitSelection);

            // If there is no character object, don't attempt to add any Improvements.
            if (objCharacter == null && blnAddImprovementsToCharacter)
            {
                Log.Info("_objCharacter = Null");
                Log.Exit("CreateImprovements");
                return true;
            }

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
                    else
                    {
                        // Display the Select Text window and record the value that was entered.
                        frmSelectText frmPickText = new frmSelectText
                        {
                            Description = LanguageManager.GetString("String_Improvement_SelectText", GlobalOptions.Language).Replace("{0}", strFriendlyName)
                        };
                        frmPickText.ShowDialog();

                        // Make sure the dialogue window was not canceled.
                        if (frmPickText.DialogResult == DialogResult.Cancel)
                        {
                            Rollback(objCharacter);
                            ForcedValue = string.Empty;
                            LimitSelection = string.Empty;
                            Log.Exit("CreateImprovements");
                            return false;
                        }

                        s_StrSelectedValue = frmPickText.SelectedValue;
                    }
                    if (blnConcatSelectedValue)
                        strSourceName += " (" + SelectedValue + ')';
                    Log.Info("_strSelectedValue = " + SelectedValue);
                    Log.Info("strSourceName = " + strSourceName);

                    // Create the Improvement.
                    Log.Info("Calling CreateImprovement");

                    CreateImprovement(objCharacter, s_StrSelectedValue, objImprovementSource, strSourceName,
                        Improvement.ImprovementType.Text,
                        strUnique);
                }

                // Check to see what bonuses the node grants.
                foreach (XmlNode bonusNode in nodBonus.ChildNodes)
                {
                    if (!ProcessBonus(objCharacter, objImprovementSource, ref strSourceName, blnConcatSelectedValue, intRating,
                        strFriendlyName, bonusNode, strUnique, !blnAddImprovementsToCharacter))
                    {
                        Rollback(objCharacter);
                        return false;
                    }
                }
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
            Log.Exit("CreateImprovements");
            return true;

        }

        private static bool ProcessBonus(Character objCharacter, Improvement.ImprovementSource objImprovementSource, ref string strSourceName,
            bool blnConcatSelectedValue, int intRating, string strFriendlyName, XmlNode bonusNode, string strUnique, bool blnIgnoreMethodNotFound = false)
        {
            if (bonusNode == null)
                return false;
            //As this became a really big nest of **** that it searched past, several places having equal paths just adding a different improvement, a more flexible method was chosen.
            //So far it is just a slower Dictionar<string, Action> but should (in theory...) be able to leverage this in the future to do it smarter with methods that are the same but
            //getting a different parameter injected

            AddImprovementCollection container = new AddImprovementCollection(objCharacter, objImprovementSource,
                strSourceName, strUnique, s_StrForcedValue, s_StrLimitSelection, SelectedValue, blnConcatSelectedValue,
                strFriendlyName, intRating);

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
                Log.Warning(new object[] {"Tried to get unknown bonus", bonusNode.OuterXml});
                return false;
            }
            return true;
        }

        public static void EnableImprovements(Character objCharacter, IList<Improvement> objImprovementList)
        {
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
                                case "Initiation":
                                    objCharacter.InitiationEnabled = true;
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
                            }
                        }
                        break;
                    case Improvement.ImprovementType.BlackMarketDiscount:
                        objCharacter.BlackMarketDiscount = true;
                        break;
                    case Improvement.ImprovementType.FriendsInHighPlaces:
                        objCharacter.FriendsInHighPlaces = true;
                        break;
                    case Improvement.ImprovementType.ExCon:
                        objCharacter.ExCon = true;
                        break;
                    case Improvement.ImprovementType.PrototypeTranshuman:
                        string strImprovedName = objImprovement.ImprovedName;
                        // Legacy compatibility
                        if (string.IsNullOrEmpty(strImprovedName))
                            objCharacter.PrototypeTranshuman = 1;
                        else
                            objCharacter.PrototypeTranshuman += Convert.ToDecimal(strImprovedName);
                        break;
                    case Improvement.ImprovementType.Erased:
                        objCharacter.Erased = true;
                        break;
                    case Improvement.ImprovementType.BornRich:
                        objCharacter.BornRich = true;
                        break;
                    case Improvement.ImprovementType.Fame:
                        objCharacter.Fame = true;
                        break;
                    case Improvement.ImprovementType.MadeMan:
                        objCharacter.MadeMan = true;
                        break;
                    case Improvement.ImprovementType.Ambidextrous:
                        objCharacter.Ambidextrous = true;
                        break;
                    case Improvement.ImprovementType.Overclocker:
                        objCharacter.Overclocker = false;
                        break;
                    case Improvement.ImprovementType.RestrictedGear:
                        objCharacter.RestrictedGear = true;
                        break;
                    case Improvement.ImprovementType.TrustFund:
                        objCharacter.TrustFund = objImprovement.Value;
                        break;
                    case Improvement.ImprovementType.Adapsin:
                        break;
                    case Improvement.ImprovementType.ContactForceGroup:
                        Contact MadeManContact = objCharacter.Contacts.FirstOrDefault(c => c.GUID == objImprovement.ImprovedName);
                        if (MadeManContact != null)
                            MadeManContact.GroupEnabled = false;
                        break;
                    case Improvement.ImprovementType.AddContact:
                        Contact NewContact = objCharacter.Contacts.FirstOrDefault(c => c.GUID == objImprovement.ImprovedName);
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
                            switch ((FilterOptions)Enum.Parse(typeof(FilterOptions), objImprovement.ImprovedName))
                            {
                                case FilterOptions.Magician:
                                case FilterOptions.Sorcery:
                                case FilterOptions.Conjuring:
                                case FilterOptions.Enchanting:
                                case FilterOptions.Adept:
                                    strCategory = "Magical Active";
                                    break;
                                case FilterOptions.Technomancer:
                                    strCategory = "Resonance Active";
                                    break;
                                default:
                                    continue;
                            }
                            
                            foreach (Skill objSkill in objCharacter.SkillsSection.Skills.Where(x => x.SkillCategory == strCategory).ToList())
                            {
                                objSkill.Enabled = true;
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
                            // TODO: Add temporarily removde skill specialization
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
                    case Improvement.ImprovementType.AdeptPowerFreeLevels:
                    case Improvement.ImprovementType.AdeptPowerFreePoints:
                        // Get the power improved by this improvement
                        Power objImprovedPower = objCharacter.Powers.FirstOrDefault(objPower => objPower.Name == objImprovement.ImprovedName && objPower.Extra == objImprovement.UniqueName);
                        objImprovedPower?.OnPropertyChanged(nameof(objImprovedPower.TotalRating));
                        break;
                    case Improvement.ImprovementType.MagiciansWayDiscount:
                        foreach (Power objLoopPower in objCharacter.Powers.Where(x => x.DiscountedAdeptWay))
                        {
                            objLoopPower.RefreshDiscountedAdeptWay(objLoopPower.AdeptWayDiscountEnabled);
                        }
                        break;
                    case Improvement.ImprovementType.FreeWare:
                        {
                            Cyberware objCyberware = objCharacter.Cyberware.FirstOrDefault(o => o.InternalId == objImprovement.ImprovedName);
                            objCyberware?.ChangeModularEquip(true);
                        }
                        break;
                    case Improvement.ImprovementType.ContactForcedLoyalty:
                        {
                            Contact objContact = objCharacter.Contacts.FirstOrDefault(x => x.GUID == objImprovement.ImprovedName);
                            if (objContact != null)
                                objContact.ForcedLoyalty = Math.Max(objContact.ForcedLoyalty, objImprovement.Value);
                        }
                        break;
                    case Improvement.ImprovementType.ContactMakeFree:
                        {
                            Contact objContact = objCharacter.Contacts.FirstOrDefault(x => x.GUID == objImprovement.ImprovedName);
                            if (objContact != null)
                                objContact.Free = true;
                        }
                        break;
                }
            }

            objImprovementList.ProcessRelevantEvents();
        }

        public static void DisableImprovements(Character objCharacter, IList<Improvement> objImprovementList)
        {
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
                                    case "Initiation":
                                        objCharacter.InitiationEnabled = false;
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
                                }
                            }
                        }
                        break;
                    case Improvement.ImprovementType.BlackMarketDiscount:
                        if (!blnHasDuplicate)
                            objCharacter.BlackMarketDiscount = false;
                        break;
                    case Improvement.ImprovementType.FriendsInHighPlaces:
                        if (!blnHasDuplicate)
                            objCharacter.FriendsInHighPlaces = false;
                        break;
                    case Improvement.ImprovementType.ExCon:
                        if (!blnHasDuplicate)
                            objCharacter.ExCon = false;
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
                            objCharacter.PrototypeTranshuman -= Convert.ToDecimal(strImprovedName);
                        break;
                    case Improvement.ImprovementType.Erased:
                        if (!blnHasDuplicate)
                            objCharacter.Erased = false;
                        break;
                    case Improvement.ImprovementType.BornRich:
                        if (!blnHasDuplicate)
                            objCharacter.BornRich = false;
                        break;
                    case Improvement.ImprovementType.Fame:
                        if (!blnHasDuplicate)
                            objCharacter.Fame = false;
                        break;
                    case Improvement.ImprovementType.MadeMan:
                        if (!blnHasDuplicate)
                            objCharacter.MadeMan = false;
                        break;
                    case Improvement.ImprovementType.Ambidextrous:
                        if (!blnHasDuplicate)
                            objCharacter.Ambidextrous = false;
                        break;
                    case Improvement.ImprovementType.Overclocker:
                        if (!blnHasDuplicate)
                            objCharacter.Overclocker = false;
                        break;
                    case Improvement.ImprovementType.RestrictedGear:
                        if (!blnHasDuplicate)
                            objCharacter.RestrictedGear = false;
                        break;
                    case Improvement.ImprovementType.TrustFund:
                        if (!blnHasDuplicate)
                            objCharacter.TrustFund = 0;
                        break;
                    case Improvement.ImprovementType.Adapsin:
                        break;
                    case Improvement.ImprovementType.ContactForceGroup:
                        if (!blnHasDuplicate)
                        {
                            Contact MadeManContact = objCharacter.Contacts.FirstOrDefault(c => c.GUID == objImprovement.ImprovedName);
                            if (MadeManContact != null)
                                MadeManContact.GroupEnabled = true;
                        }
                        break;
                    case Improvement.ImprovementType.AddContact:
                        Contact NewContact = objCharacter.Contacts.FirstOrDefault(c => c.GUID == objImprovement.ImprovedName);
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
                            switch ((FilterOptions)Enum.Parse(typeof(FilterOptions), objImprovement.ImprovedName))
                            {
                                case FilterOptions.Magician:
                                case FilterOptions.Sorcery:
                                case FilterOptions.Conjuring:
                                case FilterOptions.Enchanting:
                                case FilterOptions.Adept:
                                    strCategory = "Magical Active";
                                    break;
                                case FilterOptions.Technomancer:
                                    strCategory = "Resonance Active";
                                    break;
                                default:
                                    continue;
                            }

                            string strLoopCategory = string.Empty;
                            foreach (Improvement objLoopImprovement in objCharacter.Improvements.Where(x => x.ImproveType == Improvement.ImprovementType.SpecialSkills && x.Enabled))
                            {
                                FilterOptions eLoopFilter = (FilterOptions)Enum.Parse(typeof(FilterOptions), objLoopImprovement.ImprovedName);
                                switch (eLoopFilter)
                                {
                                    case FilterOptions.Magician:
                                    case FilterOptions.Sorcery:
                                    case FilterOptions.Conjuring:
                                    case FilterOptions.Enchanting:
                                    case FilterOptions.Adept:
                                        strLoopCategory = "Magical Active";
                                        break;
                                    case FilterOptions.Technomancer:
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
                                objSkill.Enabled = false;
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
                    case Improvement.ImprovementType.AdeptPowerFreeLevels:
                    case Improvement.ImprovementType.AdeptPowerFreePoints:
                        // Get the power improved by this improvement
                        Power objImprovedPower = objCharacter.Powers.FirstOrDefault(objPower => objPower.Name == objImprovement.ImprovedName && objPower.Extra == objImprovement.UniqueName);
                        objImprovedPower?.OnPropertyChanged(nameof(objImprovedPower.TotalRating));
                        break;
                    case Improvement.ImprovementType.MagiciansWayDiscount:
                        foreach (Power objLoopPower in objCharacter.Powers.Where(x => x.DiscountedAdeptWay))
                        {
                            objLoopPower.RefreshDiscountedAdeptWay(objLoopPower.AdeptWayDiscountEnabled);
                        }
                        break;
                    case Improvement.ImprovementType.FreeWare:
                        {
                            Cyberware objCyberware = objCharacter.Cyberware.FirstOrDefault(o => o.InternalId == objImprovement.ImprovedName);
                            objCyberware?.ChangeModularEquip(false);
                        }
                        break;
                    case Improvement.ImprovementType.ContactForcedLoyalty:
                        {
                            objCharacter.Contacts.FirstOrDefault(x => x.GUID == objImprovement.ImprovedName)?.RecalculateForcedLoyalty();
                        }
                        break;
                    case Improvement.ImprovementType.ContactMakeFree:
                        {
                            if (!blnHasDuplicate)
                            {
                                Contact objContact = objCharacter.Contacts.FirstOrDefault(x => x.GUID == objImprovement.ImprovedName);
                                if (objContact != null)
                                    objContact.Free = false;
                            }
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
            return RemoveImprovements(objCharacter, objImprovementList);
        }

        /// <summary>
        /// Remove all of the Improvements for an XML Node.
        /// </summary>
        /// <param name="objCharacter">Character from which improvements should be deleted.</param>
        /// <param name="objImprovementList">List of improvements to delete.</param>
        /// <param name="blnReapplyImprovements">Whether we're reapplying Improvements.</param>
        /// <param name="blnAllowDuplicatesFromSameSource">If we ignore checking whether a potential duplicate improvement has the same SourceName</param>
        public static decimal RemoveImprovements(Character objCharacter, IList<Improvement> objImprovementList, bool blnReapplyImprovements = false, bool blnAllowDuplicatesFromSameSource = false)
        {
            Log.Enter("RemoveImprovements");

            // If there is no character object, don't try to remove any Improvements.
            if (objCharacter == null)
            {
                Log.Exit("RemoveImprovements");
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
                                    case "Initiation":
                                        objCharacter.InitiationEnabled = false;
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
                                }
                            }
                        }
                        break;
                    case Improvement.ImprovementType.BlackMarketDiscount:
                        if (!blnHasDuplicate)
                            objCharacter.BlackMarketDiscount = false;
                        break;
                    case Improvement.ImprovementType.FriendsInHighPlaces:
                        if (!blnHasDuplicate)
                            objCharacter.FriendsInHighPlaces = false;
                        break;
                    case Improvement.ImprovementType.ExCon:
                        if (!blnHasDuplicate)
                            objCharacter.ExCon = false;
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
                            objCharacter.PrototypeTranshuman -= Convert.ToDecimal(strImprovedName);
                        break;
                    case Improvement.ImprovementType.Erased:
                        if (!blnHasDuplicate)
                            objCharacter.Erased = false;
                        break;
                    case Improvement.ImprovementType.BornRich:
                        if (!blnHasDuplicate)
                            objCharacter.BornRich = false;
                        break;
                    case Improvement.ImprovementType.Fame:
                        if (!blnHasDuplicate)
                            objCharacter.Fame = false;
                        break;
                    case Improvement.ImprovementType.MadeMan:
                        if (!blnHasDuplicate)
                            objCharacter.MadeMan = false;
                        break;
                    case Improvement.ImprovementType.Ambidextrous:
                        if (!blnHasDuplicate)
                            objCharacter.Ambidextrous = false;
                        break;
                    case Improvement.ImprovementType.Overclocker:
                        if (!blnHasDuplicate)
                            objCharacter.Overclocker = false;
                        break;
                    case Improvement.ImprovementType.RestrictedGear:
                        if (!blnHasDuplicate)
                            objCharacter.RestrictedGear = false;
                        break;
                    case Improvement.ImprovementType.TrustFund:
                        if (!blnHasDuplicate)
                            objCharacter.TrustFund = 0;
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
                    case Improvement.ImprovementType.ContactForceGroup:
                        if (!blnHasDuplicate)
                        {
                            Contact MadeManContact = objCharacter.Contacts.FirstOrDefault(c => c.GUID == objImprovement.ImprovedName);
                            if (MadeManContact != null)
                                MadeManContact.GroupEnabled = true;
                        }
                        break;
                    case Improvement.ImprovementType.AddContact:
                        Contact NewContact = objCharacter.Contacts.FirstOrDefault(c => c.GUID == objImprovement.ImprovedName);
                        if (NewContact != null)
                            objCharacter.Contacts.Remove(NewContact);
                        break;
                    case Improvement.ImprovementType.Initiation:
                        objCharacter.InitiateGrade -= objImprovement.Value;
                        break;
                    case Improvement.ImprovementType.Submersion:
                        objCharacter.SubmersionGrade -= objImprovement.Value;
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
                            objCharacter.SkillsSection.RemoveSkills((FilterOptions)Enum.Parse(typeof(FilterOptions), objImprovement.ImprovedName), !blnReapplyImprovements);
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
                                objImprovedPower.Deleting = true;
                                objCharacter.Powers.Remove(objImprovedPower);
                            }

                            objImprovedPower.OnPropertyChanged(nameof(objImprovedPower.TotalRating));
                            objImprovedPower.OnPropertyChanged(objImprovement.ImproveType == Improvement.ImprovementType.AdeptPowerFreeLevels
                                ? nameof(Power.FreeLevels) : nameof(Power.FreePoints));

                            if (objImprovedPower.Deleting)
                                objImprovedPower.UnbindPower();
                        }
                        break;
                    case Improvement.ImprovementType.MagiciansWayDiscount:
                        foreach (Power objLoopPower in objCharacter.Powers.Where(x => x.DiscountedAdeptWay))
                        {
                            objLoopPower.RefreshDiscountedAdeptWay(objLoopPower.AdeptWayDiscountEnabled);
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
                    case Improvement.ImprovementType.ContactForcedLoyalty:
                        {
                            objCharacter.Contacts.FirstOrDefault(x => x.GUID == objImprovement.ImprovedName)?.RecalculateForcedLoyalty();
                        }
                        break;
                    case Improvement.ImprovementType.ContactMakeFree:
                        {
                            if (!blnHasDuplicate)
                            {
                                Contact objContact = objCharacter.Contacts.FirstOrDefault(x => x.GUID == objImprovement.ImprovedName);
                                if (objContact != null)
                                    objContact.Free = false;
                            }
                        }
                        break;
                }
            }
            objImprovementList.ProcessRelevantEvents();

            Log.Exit("RemoveImprovements");
            return decReturn;
        }

        /// <summary>
        /// Create a new Improvement and add it to the Character.
        /// </summary>
        /// <param name="objCharacter">Character to which the improvements belong that should be processed.</param>
        /// <param name="strImprovedName">Speicific name of the Improved object - typically the name of an CharacterAttribute being improved.</param>
        /// <param name="objImprovementSource">Type of object that grants this Improvement.</param>
        /// <param name="strSourceName">Name of the item that grants this Improvement.</param>
        /// <param name="objImprovementType">Type of object the Improvement applies to.</param>
        /// <param name="strUnique">Name of the pool this Improvement should be added to - only the single higest value in the pool will be applied to the character.</param>
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
            Log.Enter("CreateImprovement");
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
                "intValue = " + intValue.ToString());
            Log.Info(
                "intRating = " + intRating.ToString());
            Log.Info(
                "intMinimum = " + intMinimum.ToString());
            Log.Info(
                "intMaximum = " + intMaximum.ToString());
            Log.Info(
                "intAugmented = " + intAugmented.ToString());
            Log.Info(
                "intAugmentedMaximum = " + intAugmentedMaximum.ToString());
            Log.Info( "strExclude = " + strExclude);
            Log.Info(
                "blnAddToRating = " + blnAddToRating.ToString());
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

            Log.Exit("CreateImprovement");
        }

        /// <summary>
        /// Clear all of the Improvements from the Transaction List.
        /// </summary>
        public static void Commit(Character objCharacter)
        {
            Log.Enter("Commit");
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

            Log.Exit("Commit");
        }

        /// <summary>
        /// Rollback all of the Improvements from the Transaction List.
        /// </summary>
        private static void Rollback(Character objCharacter)
        {
            Log.Enter("Rollback");
            if (s_DictionaryTransactions.TryGetValue(objCharacter, out List<TransactingImprovement> lstTransaction))
            {
                // Remove all of the Improvements that were added.
                foreach (TransactingImprovement objTransactingImprovement in lstTransaction)
                {
                    RemoveImprovements(objCharacter, objTransactingImprovement.ImprovementObject.ImproveSource, objTransactingImprovement.ImprovementObject.SourceName);
                    ClearCachedValue(objCharacter, objTransactingImprovement.ImprovementObject.ImproveType, objTransactingImprovement.ImprovementObject.ImprovedName);
                }

                lstTransaction.Clear();
            }

            Log.Exit("Rollback");
        }

        /// <summary>
        /// Fire off all events relevant to an enumerable of improvements, making sure each event is only fired once.
        /// </summary>
        /// <param name="lstImprovements">Enumerable of improvements whose events to fire</param>
        public static void ProcessRelevantEvents(this IEnumerable<Improvement> lstImprovements)
        {
            // Create a hashset of events to fire to make sure we only ever fire each event once
            HashSet<Action> setEventsToFire = new HashSet<Action>();
            foreach (Improvement objImprovement in lstImprovements)
            {
                foreach (Action funcEventToFire in objImprovement.GetRelevantPropertyChangers())
                    setEventsToFire.Add(funcEventToFire);
            }
            // Fire each event once
            foreach (Action funcEventToFire in setEventsToFire)
                funcEventToFire.Invoke();
        }
        #endregion
    }
}
