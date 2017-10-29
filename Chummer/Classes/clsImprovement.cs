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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend;
using Chummer.Backend.Equipment;
using Chummer.Classes;
using Chummer.Skills;
using Chummer.Backend.Attributes;

namespace Chummer
{
    [DebuggerDisplay("{DisplayDebug()}")]
    public class Improvement
    {
        private string DisplayDebug()
        {
            return $"{_objImprovementType} ({_intVal}, {_intRating}) <- {_objImprovementSource}, {_strSourceName}, {_strImprovedName}";
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
            Essence,
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
            Uneducated,
            LivingPersonaResponse,
            LivingPersonaSignal,
            LivingPersonaFirewall,
            LivingPersonaSystem,
            LivingPersonaBiofeedback,
            Smartlink,
            BiowareEssCost,
            BiowareTotalEssMultiplier,
            BiowareEssCostNonRetroactive,
            BiowareTotalEssMultiplierNonRetroactive,
            GenetechCostMultiplier,
            BasicBiowareEssCost,
            TransgenicsBiowareCost,
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
            Uncouth,
            Initiation,
            Submersion,
            Infirm,
            Skillwire,
            DamageResistance,
            RestrictedItemCount,
            AdeptLinguistics,
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
            ThrowRange,
            SkillsoftAccess,
            AddSprite,
            BlackMarketDiscount,
            SelectWeapon,
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
            SchoolOfHardKnocks,
            FriendsInHighPlaces,
            JackOfAllTrades,
            CollegeEducation,
            Erased,
            BornRich,
            Fame,
            LightningReflexes,
            Linguist,
            MadeMan,
            Overclocker,
            RestrictedGear,
            TechSchool,
            TrustFund,
            ExCon,
            BlackMarket,
            ContactMadeMan,
            SelectArmor,
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
            SkillLevel,  //Karma points in skill
            SkillGroupLevel, //group
            SkillBase,  //base points in skill
            SkillGroupBase, //group
            SkillKnowledgeForced, //A skill gained from a knowsoft
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
            SpellKarmaDiscount,
            LimitSpellCategory,
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
            Spell,
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
            // V This one should always be the last defined enum
            NumImprovementTypes
        }

        public enum ImprovementSource
        {
            Quality,
            Power,
            Metatype,
            Cyberware,
            Metavariant,
            Bioware,
            Nanotech,
            Genetech,
            ArmorEncumbrance,
            Gear,
            Spell,
            MartialArtAdvantage,
            Initiation,
            Submersion,
            Metamagic,
            Echo,
            Armor,
            ArmorMod,
            EssenceLoss,
            ConditionMonitor,
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
            AIProgram,
            SpiritFettering,
            // V This one should always be the last defined enum
            NumImprovementSources
        }

        private string _strImprovedName = string.Empty;
        private string _strSourceName = string.Empty;
        private int _intMin = 0;
        private int _intMax = 0;
        private int _intAug = 0;
        private int _intAugMax = 0;
        private int _intVal = 0;
        private int _intRating = 1;
        private string _strExclude = string.Empty;
        private string _strCondition = string.Empty;
        private string _strUniqueName = string.Empty;
        private string _strTarget = string.Empty;
        private ImprovementType _objImprovementType;
        private ImprovementSource _objImprovementSource;
        private bool _blnCustom = false;
        private string _strCustomName = string.Empty;
        private string _strCustomId = string.Empty;
        private string _strCustomGroup = string.Empty;
        private string _strNotes = string.Empty;
        private bool _blnAddToRating = false;
        private bool _blnEnabled = true;
        private int _intOrder = 0;

        #region Helper Methods

        /// <summary>
        /// Convert a string to an ImprovementType.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        private ImprovementType ConvertToImprovementType(string strValue)
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
        public ImprovementSource ConvertToImprovementSource(string strValue)
        {
            return (ImprovementSource) Enum.Parse(typeof (ImprovementSource), strValue);
        }

        #endregion

        #region Save and Load Methods

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
            get { return _blnCustom; }
            set { _blnCustom = value; }
        }

        /// <summary>
        /// User-entered name for the custom Improvement.
        /// </summary>
        public string CustomName
        {
            get { return _strCustomName; }
            set { _strCustomName = value; }
        }

        /// <summary>
        /// ID from the Improvements file. Only used for custom-made (manually created) Improvements.
        /// </summary>
        public string CustomId
        {
            get { return _strCustomId; }
            set { _strCustomId = value; }
        }

        /// <summary>
        /// Group name for the Custom Improvement.
        /// </summary>
        public string CustomGroup
        {
            get { return _strCustomGroup; }
            set { _strCustomGroup = value; }
        }

        /// <summary>
        /// User-entered notes for the custom Improvement.
        /// </summary>
        public string Notes
        {
            get { return _strNotes; }
            set { _strNotes = value; }
        }

        /// <summary>
        /// Name of the Skill or CharacterAttribute that the Improvement is improving.
        /// </summary>
        public string ImprovedName
        {
            get { return _strImprovedName; }
            set { _strImprovedName = value; }
        }

        /// <summary>
        /// Name of the source that granted this Improvement.
        /// </summary>
        public string SourceName
        {
            get { return _strSourceName; }
            set { _strSourceName = value; }
        }

        /// <summary>
        /// The type of Object that the Improvement is improving.
        /// </summary>
        public ImprovementType ImproveType
        {
            get { return _objImprovementType; }
            set
            {
                if (Enabled)
                    ImprovementManager.ClearCachedValue(_objImprovementType);
                _objImprovementType = value;
                if (Enabled)
                    ImprovementManager.ClearCachedValue(_objImprovementType);
            }
        }

        /// <summary>
        /// The type of Object that granted this Improvement.
        /// </summary>
        public ImprovementSource ImproveSource
        {
            get { return _objImprovementSource; }
            set
            {
                _objImprovementSource = value;
                if (Enabled)
                    ImprovementManager.ClearCachedValue(Improvement.ImprovementType.MatrixInitiativeDice);
            }
        }

        /// <summary>
        /// Minimum value modifier.
        /// </summary>
        public int Minimum
        {
            get { return _intMin; }
            set { _intMin = value; }
        }

        /// <summary>
        /// Maximum value modifier.
        /// </summary>
        public int Maximum
        {
            get { return _intMax; }
            set { _intMax = value; }
        }

        /// <summary>
        /// Augmented Maximum value modifier.
        /// </summary>
        public int AugmentedMaximum
        {
            get { return _intAugMax; }
            set { _intAugMax = value; }
        }

        /// <summary>
        /// Augmented score modifier.
        /// </summary>
        public int Augmented
        {
            get { return _intAug; }
            set { _intAug = value; }
        }

        /// <summary>
        /// Value modifier.
        /// </summary>
        public int Value
        {
            get { return _intVal; }
            set { _intVal = value; }
        }

        /// <summary>
        /// The Rating value for the Improvement. This is 1 by default.
        /// </summary>
        public int Rating
        {
            get { return _intRating; }
            set { _intRating = value; }
        }

        /// <summary>
        /// A list of child items that should not receive the Improvement's benefit (typically for excluding a Skill from a Skill Group bonus).
        /// </summary>
        public string Exclude
        {
            get { return _strExclude; }
            set { _strExclude = value; }
        }

        /// <summary>
        /// String containing the condition for when the bonus applies (e.g. a dicepool bonus to a skill that only applies to certain types of tests).
        /// </summary>
        public string Condition
        {
            get { return _strCondition; }
            set { _strCondition = value; }
        }

        /// <summary>
        /// A Unique name for the Improvement. Only the highest value of any one Improvement that is part of this Unique Name group will be applied.
        /// </summary>
        public string UniqueName
        {
            get { return _strUniqueName; }
            set
            {
                _strUniqueName = value;
                if (Enabled)
                    ImprovementManager.ClearCachedValue(ImproveType);
            }
        }

        /// <summary>
        /// Whether or not the bonus applies directly to a Skill's Rating
        /// </summary>
        public bool AddToRating
        {
            get { return _blnAddToRating; }
            set
            {
                _blnAddToRating = value;
                if (Enabled)
                    ImprovementManager.ClearCachedValue(ImproveType);
            }
        }

        /// <summary>
        /// The target of an improvement, e.g. the skill whose attributes should be swapped
        /// </summary>
        public string Target
        {
            get { return _strTarget; }
            set { _strTarget = value; }
        }

        /// <summary>
        /// Whether or not the Improvement is enabled and provided its bonus.
        /// </summary>
        public bool Enabled
        {
            get { return _blnEnabled; }
            set
            {
                _blnEnabled = value;
                ImprovementManager.ClearCachedValue(ImproveType);
            }
        }

        /// <summary>
        /// Sort order for Custom Improvements.
        /// </summary>
        public int SortOrder
        {
            get { return _intOrder; }
            set { _intOrder = value; }
        }

        #endregion
    }

    public static class ImprovementManager
    {
        // String that will be used to limit the selection in Pick forms.
        private static string _strLimitSelection = string.Empty;

        private static string _strSelectedValue = string.Empty;
        private static string _strForcedValue = string.Empty;
        private static readonly List<Improvement> _lstTransaction = new List<Improvement>();
        private static Dictionary<Improvement.ImprovementType, int> _dictionaryCachedValues = new Dictionary<Improvement.ImprovementType, int>((int)Improvement.ImprovementType.NumImprovementTypes);
        private static Character _objLastCachedCharacter = null;
        #region Properties

        /// <summary>
        /// Limit what can be selected in Pick forms to a single value. This is typically used when selecting the Qualities for a Metavariant that has a specifiec
        /// CharacterAttribute selection for Qualities like Metagenetic Improvement.
        /// </summary>
        public static string LimitSelection
        {
            get { return _strLimitSelection; }
            set { _strLimitSelection = value; }
        }

        /// <summary>
        /// The string that was entered or selected from any of the dialogue windows that were presented because of this Improvement.
        /// </summary>
        public static string SelectedValue
        {
            get { return _strSelectedValue; }
        }

        /// <summary>
        /// Force any dialogue windows that open to use this string as their selected value.
        /// </summary>
        public static string ForcedValue
        {
            set { _strForcedValue = value; }
        }

        public static void ClearCachedValue(Improvement.ImprovementType eImprovementType)
        {
            if (_dictionaryCachedValues.ContainsKey(eImprovementType))
                _dictionaryCachedValues[eImprovementType] = int.MinValue;
            else
                _dictionaryCachedValues.Add(eImprovementType, int.MinValue);
        }
        #endregion

        #region Helper Methods

        /// <summary>
        /// Retrieve the total Improvement value for the specified ImprovementType.
        /// </summary>
        /// <param name="objImprovementType">ImprovementType to retrieve the value of.</param>
        /// <param name="blnAddToRating">Whether or not we should only retrieve values that have AddToRating enabled.</param>
        /// <param name="strImprovedName">Name to assign to the Improvement.</param>
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
            int intCachedValue;
            if (_objLastCachedCharacter == objCharacter && !blnAddToRating && string.IsNullOrEmpty(strImprovedName) && blnUnconditionalOnly && _dictionaryCachedValues.TryGetValue(objImprovementType, out intCachedValue) && intCachedValue != int.MinValue)
            {
                return intCachedValue;
            }

            HashSet<string> lstUniqueName = new HashSet<string>();
            HashSet<Tuple<string, int>> lstUniquePair = new HashSet<Tuple<string, int>>();
            int intValue = 0;
            foreach (Improvement objImprovement in objCharacter.Improvements.Where(objImprovement => objImprovement.ImproveType == objImprovementType))
            {
                if (objImprovement.Enabled && !objImprovement.Custom && (!blnUnconditionalOnly || string.IsNullOrEmpty(objImprovement.Condition)))
                {
                    bool blnAllowed = objImprovement.ImproveType == objImprovementType &&
                        !(objCharacter.RESEnabled && objImprovement.ImproveSource == Improvement.ImprovementSource.Gear &&
                          objImprovementType == Improvement.ImprovementType.MatrixInitiativeDice) &&
                    // Ignore items that apply to a Skill's Rating.
                          objImprovement.AddToRating == blnAddToRating &&
                    // If an Improved Name has been passed, only retrieve values that have this Improved Name.
                          (string.IsNullOrEmpty(strImprovedName) || strImprovedName == objImprovement.ImprovedName);

                    if (blnAllowed)
                    {
                        string strUniqueName = objImprovement.UniqueName;
                        if (!string.IsNullOrEmpty(strUniqueName))
                        {
                            // If this has a UniqueName, run through the current list of UniqueNames seen. If it is not already in the list, add it.
                            if (!lstUniqueName.Contains(strUniqueName))
                                lstUniqueName.Add(strUniqueName);

                            // Add the values to the UniquePair List so we can check them later.
                            lstUniquePair.Add(new Tuple<string, int>(strUniqueName, objImprovement.Value));
                        }
                        else
                        {
                            intValue += objImprovement.Value;
                        }
                    }
                }
            }

            if (lstUniqueName.Contains("precedence0"))
            {
                // Retrieve only the highest precedence0 value.
                // Run through the list of UniqueNames and pick out the highest value for each one.
                int intHighest = (from strValues in lstUniquePair where strValues.Item1 == "precedence0" select strValues.Item2).Concat(new[] { int.MinValue }).Max();
                if (lstUniqueName.Contains("precedence-1"))
                {
                    intHighest += lstUniquePair.Where(strValues => strValues.Item1 == "precedence-1").Sum(strValues => strValues.Item2);
                }
                intValue = Math.Max(intValue, intHighest);
            }
            else if (lstUniqueName.Contains("precedence1"))
            {
                // Retrieve all of the items that are precedence1 and nothing else.
                intValue = Math.Max(intValue, lstUniquePair.Where(strValues => strValues.Item1 == "precedence1" || strValues.Item1 == "precedence-1").Sum(strValues => strValues.Item2));
            }
            else
            {
                // Run through the list of UniqueNames and pick out the highest value for each one.
                intValue += lstUniqueName.Sum(strName => (from strValues in lstUniquePair where strValues.Item1 == strName select strValues.Item2).Concat(new[] { int.MinValue }).Max());
            }

            // Factor in Custom Improvements.
            lstUniqueName.Clear();
            lstUniquePair.Clear();
            int intCustomValue = 0;
            foreach (Improvement objImprovement in objCharacter.Improvements)
            {
                if (objImprovement.Custom && objImprovement.Enabled && (!blnUnconditionalOnly || string.IsNullOrEmpty(objImprovement.Condition)))
                {
                    bool blnAllowed = objImprovement.ImproveType == objImprovementType &&
                        !(objCharacter.RESEnabled && objImprovement.ImproveSource == Improvement.ImprovementSource.Gear &&
                          objImprovementType == Improvement.ImprovementType.MatrixInitiativeDice) &&
                    // Ignore items that apply to a Skill's Rating.
                          objImprovement.AddToRating == blnAddToRating &&
                    // If an Improved Name has been passed, only retrieve values that have this Improved Name.
                          (string.IsNullOrEmpty(strImprovedName) || strImprovedName == objImprovement.ImprovedName);

                    if (blnAllowed)
                    {
                        string strUniqueName = objImprovement.UniqueName;
                        if (!string.IsNullOrEmpty(strUniqueName))
                        {
                            // If this has a UniqueName, run through the current list of UniqueNames seen. If it is not already in the list, add it.
                            if (!lstUniqueName.Contains(strUniqueName))
                                lstUniqueName.Add(strUniqueName);

                            // Add the values to the UniquePair List so we can check them later.
                            lstUniquePair.Add(new Tuple<string, int>(strUniqueName, objImprovement.Value));
                        }
                        else
                        {
                            intCustomValue += objImprovement.Value;
                        }
                    }
                }
            }

            // Run through the list of UniqueNames and pick out the highest value for each one.
            intCustomValue += lstUniqueName.Sum(strName => (from strValues in lstUniquePair where strValues.Item1 == strName select strValues.Item2).Concat(new[] {int.MinValue}).Max());

            //Log.Exit("ValueOf");
            // If this is the default ValueOf() call, let's cache the value we've calculated so that we don't have to do this all over again unless something has changed
            if (!blnAddToRating && string.IsNullOrEmpty(strImprovedName))
            {
                if (_objLastCachedCharacter != objCharacter)
                {
                    for(Improvement.ImprovementType eLoopImprovement = 0; eLoopImprovement < Improvement.ImprovementType.NumImprovementTypes; ++eLoopImprovement)
                    {
                        ClearCachedValue(eLoopImprovement);
                    }
                    _objLastCachedCharacter = objCharacter;
                }
                if (_dictionaryCachedValues.ContainsKey(objImprovementType))
                    _dictionaryCachedValues[objImprovementType] = intValue + intCustomValue;
                else
                    _dictionaryCachedValues.Add(objImprovementType, intValue + intCustomValue);
            }

            return intValue + intCustomValue;
        }

        /// <summary>
        /// Convert a string to an integer, converting "Rating" to a number where appropriate.
        /// </summary>
        /// <param name="strValue">String value to parse.</param>
        /// <param name="intRating">Integer value to replace "Rating" with.</param>
        private static int ValueToInt(Character objCharacter, string strValue, int intRating)
        {
            //         Log.Enter("ValueToInt");
            //         Log.Info("strValue = " + strValue);
            //Log.Info("intRating = " + intRating.ToString());
            if (strValue.Contains("FixedValues"))
            {
                string[] strValues = strValue.TrimStart("FixedValues", true).Trim("()".ToCharArray()).Split(',');
                if (strValues.Length >= intRating)
                    strValue = strValues[intRating - 1];
                else
                    strValue = strValues[strValues.Length - 1];
            }
            if (strValue.Contains("Rating") || AttributeSection.AttributeStrings.Any(strValue.Contains))
            {
                string strReturn = strValue.Replace("Rating", intRating.ToString());
                // If the value contain an CharacterAttribute name, replace it with the character's CharacterAttribute.
                foreach (string strAttribute in AttributeSection.AttributeStrings)
                {
                    strReturn = strReturn.Replace(strAttribute, objCharacter.GetAttribute(strAttribute).TotalValue.ToString());
                }

                XmlDocument objXmlDocument = new XmlDocument();
                XPathNavigator nav = objXmlDocument.CreateNavigator();
                //Log.Info("strValue = " + strValue);
                //Log.Info("strReturn = " + strReturn);
                XPathExpression xprValue = nav.Compile(strReturn);

                // Treat this as a decimal value so any fractions can be rounded down. This is currently only used by the Boosted Reflexes Cyberware from SR2050.
                double dblValue = Convert.ToDouble(nav.Evaluate(xprValue).ToString(), GlobalOptions.InvariantCultureInfo);
                int intValue = Convert.ToInt32(Math.Floor(dblValue));

                //Log.Exit("ValueToInt");
                return intValue;
            }
            else
            {
                //Log.Exit("ValueToInt");
                int intReturn = 0;
                int.TryParse(strValue, out intReturn);
                return intReturn;
            }
        }

        /// <summary>
        /// Determine whether or not an XmlNode with the specified name exists within an XmlNode.
        /// </summary>
        /// <param name="objXmlNode">XmlNode to examine.</param>
        /// <param name="strName">Name of the XmlNode to look for.</param>
        private static bool NodeExists(XmlNode objXmlNode, string strName)
        {
            //Log.Enter("NodeExists");
            //Log.Info("objXmlNode = " + objXmlNode.OuterXml.ToString());
            //Log.Info("strName = " + strName);

            return objXmlNode?.SelectSingleNode(strName) != null;
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
        /// <returns>True if successfull</returns>
        public static bool CreateImprovements(Character objCharacter, Improvement.ImprovementSource objImprovementSource, string strSourceName,
            XmlNode nodBonus, bool blnConcatSelectedValue = false, int intRating = 1, string strFriendlyName = "")
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
                    _strForcedValue = string.Empty;
                    _strLimitSelection = string.Empty;
                    Log.Exit("CreateImprovements");
                    return true;
                }

                string strUnique = string.Empty;
                if (nodBonus.Attributes?["unique"] != null)
                    strUnique = nodBonus.Attributes["unique"].InnerText;

                _strSelectedValue = string.Empty;

                Log.Info(
                    "_strForcedValue = " + _strForcedValue);
                Log.Info(
                    "_strLimitSelection = " + _strLimitSelection);

                // If no friendly name was provided, use the one from SourceName.
                if (string.IsNullOrEmpty(strFriendlyName))
                    strFriendlyName = strSourceName;

                if (nodBonus.HasChildNodes)
                {
                    Log.Info("Has Child Nodes");
                }
                if (NodeExists(nodBonus, "selecttext"))
                {
                    Log.Info("selecttext");

                    if (objCharacter != null)
                    {
                        if (!string.IsNullOrEmpty(_strForcedValue))
                        {
                            LimitSelection = _strForcedValue;
                        }
                        else if (objCharacter.Pushtext.Count != 0)
                        {
                            LimitSelection = objCharacter.Pushtext.Pop();
                        }
                    }

                    Log.Info("_strForcedValue = " + SelectedValue);
                    Log.Info("_strLimitSelection = " + LimitSelection);

                    if (!string.IsNullOrEmpty(LimitSelection))
                    {
                        _strSelectedValue = LimitSelection;
                    }
                    else
                    {
                    // Display the Select Text window and record the value that was entered.
                    frmSelectText frmPickText = new frmSelectText();
                    frmPickText.Description = LanguageManager.GetString("String_Improvement_SelectText")
                        .Replace("{0}", strFriendlyName);
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

                    _strSelectedValue = frmPickText.SelectedValue;
                    }
                    if (blnConcatSelectedValue)
                        strSourceName += " (" + SelectedValue + ")";
                    Log.Info("_strSelectedValue = " + SelectedValue);
                    Log.Info("strSourceName = " + strSourceName);

                    // Create the Improvement.
                    Log.Info("Calling CreateImprovement");

                    CreateImprovement(objCharacter, _strSelectedValue, objImprovementSource, strSourceName,
                        Improvement.ImprovementType.Text,
                        strUnique);
                }

                // If there is no character object, don't attempt to add any Improvements.
                if (objCharacter == null)
                {
                    Log.Info( "_objCharacter = Null");
                    Log.Exit("CreateImprovements");
                    return true;
                }

                // Check to see what bonuses the node grants.
                foreach (XmlNode bonusNode in nodBonus.ChildNodes)
                {
                    if (!ProcessBonus(objCharacter, objImprovementSource, ref strSourceName, blnConcatSelectedValue, intRating,
                        strFriendlyName, bonusNode, strUnique))
                    {
                        Rollback(objCharacter);
                        return false;
                    }
                }


                // If we've made it this far, everything went OK, so commit the Improvements.
                Log.Info("Calling Commit");
                Commit(objCharacter);
                Log.Info("Returned from Commit");
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
            Log.Exit("CreateImprovements");
            return true;

        }
        private static bool ProcessBonus(Character objCharacter, Improvement.ImprovementSource objImprovementSource, ref string strSourceName,
            bool blnConcatSelectedValue, int intRating, string strFriendlyName, XmlNode bonusNode, string strUnique)
        {
            if (bonusNode == null)
                return false;
            //As this became a really big nest of **** that it searched past, several places having equal paths just adding a different improvement, a more flexible method was chosen.
            //So far it is just a slower Dictionar<string, Action> but should (in theory...) be able to leverage this in the future to do it smarter with methods that are the same but
            //getting a different parameter injected

            AddImprovementCollection container = new AddImprovementCollection(objCharacter, objImprovementSource,
                strSourceName, strUnique, _strForcedValue, _strLimitSelection, SelectedValue, blnConcatSelectedValue,
                strFriendlyName, intRating, ValueToInt, Rollback);

            MethodInfo info;
            if (AddMethods.Value.TryGetValue(bonusNode.Name.ToUpperInvariant(), out info))
            {
                try
                {
                    info.Invoke(container, new object[] {bonusNode});
                }
                catch (TargetInvocationException ex) when (ex.InnerException?.GetType() == typeof(AbortedException))
                {
                    Rollback(objCharacter);
                    return false;
                }

                strSourceName = container.SourceName;
                _strForcedValue = container.ForcedValue;
                _strLimitSelection = container.LimitSelection;
                _strSelectedValue = container.SelectedValue;
            }
            else if (bonusNode.ChildNodes.Count == 0)
            {
                return true;
            }
            else if (bonusNode.NodeType != XmlNodeType.Comment)
            {
                Utils.BreakIfDebug();
                Log.Warning(new object[] {"Tried to get unknown bonus", bonusNode.OuterXml, string.Join(", ", AddMethods.Value.Keys)});
                return false;
            }
            return true;
        }

        //this should probably be somewhere else...
        private static readonly Lazy<Dictionary<string, MethodInfo>> AddMethods = new Lazy<Dictionary<string, MethodInfo>>(() =>
        {
            MethodInfo[] allMethods = typeof(AddImprovementCollection).GetMethods();

            return allMethods.ToDictionary(x => x.Name.ToUpperInvariant());
        });

        /// <summary>
        /// Remove all of the Improvements for an XML Node.
        /// </summary>
        /// <param name="objImprovementSource">Type of object that granted these Improvements.</param>
        /// <param name="strSourceName">Name of the item that granted these Improvements. If empty, deletes all improvements that match objImprovementSource</param>
        /// <param name="blnReapplyImprovements">Remove all improvements from the improvements list that would be reapplied by Reapply Improvements.</param>
        public static void RemoveImprovements(Character objCharacter, Improvement.ImprovementSource objImprovementSource, string strSourceName = "", bool blnReapplyImprovements = false)
        {
            Log.Enter("RemoveImprovements");
            if (blnReapplyImprovements)
                Log.Info("Wiping all improvements that would be refreshed by Re-Apply Improvements.");
            else
            {
                Log.Info("objImprovementSource = " + objImprovementSource.ToString());
                Log.Info("strSourceName = " + strSourceName);
            }

            // If there is no character object, don't try to remove any Improvements.
            if (objCharacter == null)
            {
                Log.Exit("RemoveImprovements");
                return;
            }

            // A List of Improvements to hold all of the items that will eventually be deleted.
            List<Improvement> objImprovementList = null;
            if (blnReapplyImprovements)
            {
                objImprovementList = objCharacter.Improvements.Where(objImprovement =>
                                                                        objImprovement.ImproveSource == Improvement.ImprovementSource.AIProgram ||
                                                                        objImprovement.ImproveSource == Improvement.ImprovementSource.Armor ||
                                                                        objImprovement.ImproveSource == Improvement.ImprovementSource.ArmorMod ||
                                                                        objImprovement.ImproveSource == Improvement.ImprovementSource.Bioware ||
                                                                        objImprovement.ImproveSource == Improvement.ImprovementSource.ComplexForm ||
                                                                        objImprovement.ImproveSource == Improvement.ImprovementSource.CritterPower ||
                                                                        objImprovement.ImproveSource == Improvement.ImprovementSource.Cyberware ||
                                                                        objImprovement.ImproveSource == Improvement.ImprovementSource.Echo ||
                                                                        objImprovement.ImproveSource == Improvement.ImprovementSource.Gear ||
                                                                        objImprovement.ImproveSource == Improvement.ImprovementSource.MartialArtAdvantage ||
                                                                        objImprovement.ImproveSource == Improvement.ImprovementSource.Metamagic ||
                                                                        objImprovement.ImproveSource == Improvement.ImprovementSource.Power ||
                                                                        objImprovement.ImproveSource == Improvement.ImprovementSource.Quality ||
                                                                        objImprovement.ImproveSource == Improvement.ImprovementSource.Spell).ToList();
            }
            else
                objImprovementList = objCharacter.Improvements.Where(objImprovement => objImprovement.ImproveSource == objImprovementSource && (string.IsNullOrEmpty(strSourceName) || objImprovement.SourceName == strSourceName)).ToList();

            // Now that we have all of the applicable Improvements, remove them from the character.
            foreach (Improvement objImprovement in objImprovementList)
            {
                // Remove the Improvement.
                objCharacter.Improvements.Remove(objImprovement);
                ClearCachedValue(objImprovement.ImproveType);
            }
            // Now that the entire list is deleted from the character's improvements list, we do the checking of duplicates and extra effects
            foreach (Improvement objImprovement in objImprovementList)
            {
                // See if the character has anything else that is granting them the same bonus as this improvement
                bool blnHasDuplicate = objCharacter.Improvements.Any(objLoopImprovement => objLoopImprovement.UniqueName == objImprovement.UniqueName && objLoopImprovement.ImprovedName == objImprovement.ImprovedName && objLoopImprovement.ImproveType == objImprovement.ImproveType && objLoopImprovement.SourceName != objImprovement.SourceName);

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
                    case Improvement.ImprovementType.SwapSkillAttribute:
                    case Improvement.ImprovementType.SwapSkillSpecAttribute:
                        objCharacter.SkillsSection.ForceProperyChangedNotificationAll(nameof(Skill.PoolToolTip));
                        break;
                    case Improvement.ImprovementType.SkillsoftAccess:
                        objCharacter.SkillsSection.KnowledgeSkills.RemoveAll(objCharacter.SkillsSection.KnowsoftSkills.Contains);
                        break;
                    case Improvement.ImprovementType.SkillKnowledgeForced:
                        Guid guid = Guid.Parse(objImprovement.ImprovedName);
                        objCharacter.SkillsSection.KnowledgeSkills.RemoveAll(skill => skill.Id == guid);
                        objCharacter.SkillsSection.KnowsoftSkills.RemoveAll(skill => skill.Id == guid);
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
                        objCharacter.AttributeSection.ForceAttributePropertyChangedNotificationAll(nameof(CharacterAttrib.AttributeModifiers),objImprovement.ImprovedName);
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
                        {
                            objCharacter.BlackMarketDiscount = false;
                            if (!objCharacter.Created)
                            {
                                foreach (Vehicle objVehicle in objCharacter.Vehicles)
                                {
                                    objVehicle.BlackMarketDiscount = false;
                                    foreach (Weapon objWeapon in objVehicle.Weapons)
                                    {
                                        objWeapon.DiscountCost = false;
                                        foreach (WeaponAccessory objWeaponAccessory in objWeapon.WeaponAccessories)
                                        {
                                            objWeaponAccessory.DiscountCost = false;
                                        }
                                    }
                                    foreach (Gear objGear in objVehicle.Gear)
                                    {
                                        objGear.DiscountCost = false;
                                    }
                                    foreach (VehicleMod objMod in objVehicle.Mods)
                                    {
                                        objMod.DiscountCost = false;
                                    }
                                }
                                foreach (Weapon objWeapon in objCharacter.Weapons)
                                {
                                    objWeapon.DiscountCost = false;
                                    foreach (WeaponAccessory objWeaponAccessory in objWeapon.WeaponAccessories)
                                    {
                                        objWeaponAccessory.DiscountCost = false;
                                    }
                                }
                                foreach (Gear objGear in objCharacter.Gear)
                                {
                                    objGear.DiscountCost = false;

                                    foreach (Gear objChild in objGear.Children)
                                    {
                                        objChild.DiscountCost = false;
                                    }
                                }
                            }
                        }
                        break;
                    case Improvement.ImprovementType.Uneducated:
                        if (!blnHasDuplicate)
                            objCharacter.SkillsSection.Uneducated = false;
                        break;
                    case Improvement.ImprovementType.Uncouth:
                        if (!blnHasDuplicate)
                            objCharacter.SkillsSection.Uncouth = false;
                        break;
                    case Improvement.ImprovementType.FriendsInHighPlaces:
                        if (!blnHasDuplicate)
                            objCharacter.FriendsInHighPlaces = false;
                        break;
                    case Improvement.ImprovementType.SchoolOfHardKnocks:
                        if (!blnHasDuplicate)
                            objCharacter.SkillsSection.SchoolOfHardKnocks = false;
                        break;
                    case Improvement.ImprovementType.ExCon:
                        if (!blnHasDuplicate)
                            objCharacter.ExCon = false;
                        break;
                    case Improvement.ImprovementType.JackOfAllTrades:
                        if (!blnHasDuplicate)
                            objCharacter.SkillsSection.JackOfAllTrades = false;
                        break;
                    case Improvement.ImprovementType.PrototypeTranshuman:
                        if (!blnHasDuplicate)
                            objCharacter.PrototypeTranshuman = 0;
                        break;
                    case Improvement.ImprovementType.CollegeEducation:
                        if (!blnHasDuplicate)
                            objCharacter.SkillsSection.CollegeEducation = false;
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
                    case Improvement.ImprovementType.LightningReflexes:
                        if (!blnHasDuplicate)
                            objCharacter.LightningReflexes = false;
                        break;
                    case Improvement.ImprovementType.Linguist:
                        if (!blnHasDuplicate)
                            objCharacter.SkillsSection.Linguist = false;
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
                    case Improvement.ImprovementType.TechSchool:
                        if (!blnHasDuplicate)
                            objCharacter.SkillsSection.TechSchool = false;
                        break;
                    case Improvement.ImprovementType.TrustFund:
                        if (!blnHasDuplicate)
                            objCharacter.TrustFund = 0;
                        break;
                    case Improvement.ImprovementType.Adapsin:
                        if (!objCharacter.AdapsinEnabled)
                        {
                            foreach (Cyberware objCyberware in objCharacter.Cyberware)
                            {
                                if (objCyberware.Grade.Adapsin)
                                {
                                    // Determine which GradeList to use for the Cyberware.
                                    GradeList objGradeList;
                                        if (objCyberware.SourceType == Improvement.ImprovementSource.Bioware)
                                        {
                                            GlobalOptions.BiowareGrades.LoadList(Improvement.ImprovementSource.Bioware, objCharacter.Options);
                                            objGradeList = GlobalOptions.BiowareGrades;
                                        }
                                        else
                                        {
                                            GlobalOptions.CyberwareGrades.LoadList(Improvement.ImprovementSource.Cyberware, objCharacter.Options);
                                            objGradeList = GlobalOptions.CyberwareGrades;
                                        }

                                    objCyberware.Grade = objGradeList.GetGrade(objCyberware.Grade.Name.Replace("(Adapsin)", string.Empty).Trim());
                                }
                            }
                        }
                        break;
                    case Improvement.ImprovementType.ContactMadeMan:
                        Contact MadeManContact = objCharacter.Contacts.FirstOrDefault(c => c.GUID == objImprovement.ImprovedName);
                        if (MadeManContact != null)
                            MadeManContact.MadeMan = false;
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
                    case Improvement.ImprovementType.CritterPower:
                        CritterPower objCritterPower = objCharacter.CritterPowers.FirstOrDefault(x => x.Name == objImprovement.ImprovedName && x.Extra == objImprovement.UniqueName);
                        if (objCritterPower != null)
                        {
                            RemoveImprovements(objCharacter, Improvement.ImprovementSource.CritterPower, objCritterPower.InternalId);
                            objCharacter.CritterPowers.Remove(objCritterPower);
                        }
                        break;
                    case Improvement.ImprovementType.Spell:
                        Spell objSpell = objCharacter.Spells.FirstOrDefault(x => x.InternalId == objImprovement.ImprovedName);
                        if (objSpell != null)
                        {
                            RemoveImprovements(objCharacter, Improvement.ImprovementSource.Spell, objSpell.InternalId);
                            objCharacter.Spells.Remove(objSpell);
                        }
                        break;
                    case Improvement.ImprovementType.MartialArt:
                        MartialArt objMartialArt = objCharacter.MartialArts.FirstOrDefault(x => x.InternalId == objImprovement.ImprovedName);
                        if (objMartialArt != null)
                        {
                            RemoveImprovements(objCharacter, Improvement.ImprovementSource.MartialArt, objMartialArt.InternalId);
                            // Remove the Improvements for any Advantages for the Martial Art that is being removed.
                            foreach (MartialArtAdvantage objAdvantage in objMartialArt.Advantages)
                            {
                                RemoveImprovements(objCharacter, Improvement.ImprovementSource.MartialArtAdvantage, objAdvantage.InternalId);
                            }
                            objCharacter.MartialArts.Remove(objMartialArt);
                        }
                        break;
                    case Improvement.ImprovementType.SpecialSkills:
                        if (!blnHasDuplicate)
                            objCharacter.SkillsSection.RemoveSkills((SkillsSection.FilterOptions)Enum.Parse(typeof(SkillsSection.FilterOptions), objImprovement.ImprovedName));
                        break;
                    case Improvement.ImprovementType.SpecificQuality:
                        Quality objQuality = objCharacter.Qualities.FirstOrDefault(objLoopQuality => objLoopQuality.InternalId == objImprovement.ImprovedName);
                        if (objQuality != null)
                        {
                            RemoveImprovements(objCharacter, Improvement.ImprovementSource.Quality, objQuality.InternalId);
                            objCharacter.Qualities.Remove(objQuality);
                        }
                        break;
                    case Improvement.ImprovementType.SkillSpecialization:
                        Skill objSkill = objCharacter.SkillsSection.GetActiveSkill(objImprovement.ImprovedName);
                        if (objSkill != null)
                        {
                            SkillSpecialization objSkillSpec = objSkill.Specializations.FirstOrDefault(x => x.Name == objImprovement.UniqueName);
                                if (objSkillSpec != null)
                            objSkill.Specializations.Remove(objSkillSpec);
                        }
                        break;
                    case Improvement.ImprovementType.AIProgram:
                        AIProgram objProgram = objCharacter.AIPrograms.FirstOrDefault(objLoopProgram => objLoopProgram.InternalId == objImprovement.ImprovedName);
                        if (objProgram != null)
                            objCharacter.AIPrograms.Remove(objProgram);
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
                        }
                        break;
                }
            }


            objCharacter.ImprovementHook(objImprovementList);

            Log.Exit("RemoveImprovements");
        }

        /// <summary>
        /// Create a new Improvement and add it to the Character.
        /// </summary>
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
                objImprovement.Target = strTarget;
                objImprovement.Condition = strCondition;

                // Add the Improvement to the list.
                objCharacter.Improvements.Add(objImprovement);
                ClearCachedValue(objImprovement.ImproveType);

                // Add the Improvement to the Transaction List.
                _lstTransaction.Add(objImprovement);
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

            objCharacter.ImprovementHook(_lstTransaction);
            _lstTransaction.Clear();
            Log.Exit("Commit");
        }

        /// <summary>
        /// Rollback all of the Improvements from the Transaction List.
        /// </summary>
        private static void Rollback(Character objCharacter)
        {
            Log.Enter("Rollback");
            // Remove all of the Improvements that were added.
            foreach (Improvement objImprovement in _lstTransaction)
            {
                RemoveImprovements(objCharacter, objImprovement.ImproveSource, objImprovement.SourceName);
                ClearCachedValue(objImprovement.ImproveType);
            }

            _lstTransaction.Clear();
            Log.Exit("Rollback");
        }

        #endregion



}

    public static class ImprovementExtensions
    {
        /// <summary>
        /// Are Skill Points enabled for the character?
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static bool HaveSkillPoints(this CharacterBuildMethod method)
        {
            return method == CharacterBuildMethod.Priority || method == CharacterBuildMethod.SumtoTen;
        }
    }
}
