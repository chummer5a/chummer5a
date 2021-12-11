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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Annotations;

// ReSharper disable StringLiteralTypo

namespace Chummer
{
    public enum CharacterBuildMethod
    {
        Karma = 0,
        Priority = 1,
        SumtoTen = 2,
        LifeModule = 3
    }

    public static class CharacterBuildMethodExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool UsesPriorityTables(this CharacterBuildMethod eBuildMethod)
        {
            return eBuildMethod == CharacterBuildMethod.Priority || eBuildMethod == CharacterBuildMethod.SumtoTen;
        }
    }

    public sealed class CharacterSettings : INotifyMultiplePropertyChanged
    {
        private Guid _guiSourceId = Guid.Empty;
        private string _strFileName = string.Empty;
        private string _strName = "Standard";
        private bool _blnDoingCopy;

        // Settings.
        // ReSharper disable once InconsistentNaming
        private bool _blnAllow2ndMaxAttribute;

        private bool _blnAllowBiowareSuites;
        private bool _blnAllowCyberwareESSDiscounts;
        private bool _blnAllowEditPartOfBaseWeapon;
        private bool _blnAllowHigherStackedFoci;
        private bool _blnAllowInitiationInCreateMode;
        private bool _blnAllowObsolescentUpgrade;
        private bool _blnDontUseCyberlimbCalculation;
        private bool _blnAllowSkillRegrouping = true;
        private bool _blnAlternateMetatypeAttributeKarma;
        private bool _blnArmorDegradation;
        private bool _blnStrictSkillGroupsInCreateMode;
        private bool _blnAllowPointBuySpecializationsOnKarmaSkills;
        private bool _blnCyberlegMovement;
        private bool _blnDontDoubleQualityPurchaseCost;
        private bool _blnDontDoubleQualityRefundCost;
        private bool _blnEnforceCapacity = true;
        private bool _blnESSLossReducesMaximumOnly;
        private bool _blnExceedNegativeQualities;
        private bool _blnExceedNegativeQualitiesLimit;
        private bool _blnExceedPositiveQualities;
        private bool _blnExceedPositiveQualitiesCostDoubled;
        private bool _blnExtendAnyDetectionSpell;
        private bool _blnDroneArmorMultiplierEnabled;
        private bool _blnFreeSpiritPowerPointsMAG;
        private bool _blnNoArmorEncumbrance;
        private bool _blnIgnoreArt;
        private bool _blnIgnoreComplexFormLimit;
        private bool _blnUnarmedImprovementsApplyToWeapons;
        private bool _blnLicenseRestrictedItems;
        private bool _blnMaximumArmorModifications;
        private bool _blnMetatypeCostsKarma = true;
        private bool _blnMoreLethalGameplay;
        private bool _blnMultiplyForbiddenCost;
        private bool _blnMultiplyRestrictedCost;
        private bool _blnNoSingleArmorEncumbrance;
        private bool _blnRestrictRecoil = true;
        private bool _blnSpecialKarmaCostBasedOnShownValue;
        private bool _blnSpiritForceBasedOnTotalMAG;
        private bool _blnUnrestrictedNuyen;
        private bool _blnUseCalculatedPublicAwareness;
        private bool _blnUsePointsOnBrokenGroups;
        private string _strContactPointsExpression = "{CHAUnaug} * 3";
        private string _strKnowledgePointsExpression = "({INTUnaug} + {LOGUnaug}) * 2";
        private string _strChargenKarmaToNuyenExpression = "{Karma} * 2000 + {PriorityNuyen}";
        private string _strBoundSpiritExpression = "{CHA}";
        private string _strRegisteredSpriteExpression = "{LOG}";
        private bool _blnDoNotRoundEssenceInternally;
        private bool _blnEnableEnemyTracking;
        private bool _blnEnemyKarmaQualityLimit = true;
        private string _strEssenceFormat = "#,0.00";
        private int _intForbiddenCostMultiplier = 1;
        private int _intDroneArmorMultiplier = 2;
        private int _intLimbCount = 6;
        private int _intMetatypeCostMultiplier = 1;
        private decimal _decNuyenPerBPWftM = 2000.0m;
        private decimal _decNuyenPerBPWftP = 2000.0m;
        private int _intRestrictedCostMultiplier = 1;
        private bool _blnAutomaticBackstory = true;
        private bool _blnFreeMartialArtSpecialization;
        private bool _blnPrioritySpellsAsAdeptPowers;
        private bool _blnMysAdeptAllowPpCareer;
        private bool _blnMysAdeptSecondMAGAttribute;
        private bool _blnReverseAttributePriorityOrder;
        private string _strNuyenFormat = "#,0.##";
        private bool _blnCompensateSkillGroupKarmaDifference;
        private bool _blnIncreasedImprovedAbilityMultiplier;
        private bool _blnAllowFreeGrids;
        private bool _blnAllowTechnomancerSchooling;
        private bool _blnCyberlimbAttributeBonusCapOverride;
        private string _strBookXPath = string.Empty;
        private string _strExcludeLimbSlot = string.Empty;
        private int _intCyberlimbAttributeBonusCap = 4;
        private bool _blnUnclampAttributeMinimum;
        private bool _blnDroneMods;
        private bool _blnDroneModsMaximumPilot;

        // Karma variables.
        private int _intKarmaAttribute = 5;

        private int _intKarmaCarryover = 7;
        private int _intKarmaContact = 1;
        private int _intKarmaEnemy = 1;
        private int _intKarmaEnhancement = 2;
        private int _intKarmaImproveActiveSkill = 2;
        private int _intKarmaImproveKnowledgeSkill = 1;
        private int _intKarmaImproveSkillGroup = 5;
        private int _intKarmaInitiation = 3;
        private int _intKarmaInitiationFlat = 10;
        private int _intKarmaJoinGroup = 5;
        private int _intKarmaLeaveGroup = 1;
        private int _intKarmaTechnique = 5;
        private int _intKarmaMetamagic = 15;
        private int _intKarmaNewActiveSkill = 2;
        private int _intKarmaNewComplexForm = 4;
        private int _intKarmaNewKnowledgeSkill = 1;
        private int _intKarmaNewSkillGroup = 5;
        private int _intKarmaQuality = 1;
        private int _intKarmaSpecialization = 7;
        private int _intKarmaKnoSpecialization = 7;
        private int _intKarmaSpell = 5;
        private int _intKarmaSpirit = 1;
        private int _intKarmaNewAIProgram = 5;
        private int _intKarmaNewAIAdvancedProgram = 8;
        private int _intKarmaMysticAdeptPowerPoint = 5;
        private int _intKarmaSpiritFettering = 3;

        // Karma Foci variables.
        // Enchanting
        private int _intKarmaAlchemicalFocus = 3;

        private int _intKarmaDisenchantingFocus = 3;

        // Metamagic
        private int _intKarmaCenteringFocus = 3;

        private int _intKarmaFlexibleSignatureFocus = 3;
        private int _intKarmaMaskingFocus = 3;
        private int _intKarmaSpellShapingFocus = 3;

        // Power
        private int _intKarmaPowerFocus = 6;

        // Qi
        private int _intKarmaQiFocus = 2;

        // Spell
        private int _intKarmaCounterspellingFocus = 2;

        private int _intKarmaRitualSpellcastingFocus = 2;
        private int _intKarmaSpellcastingFocus = 2;
        private int _intKarmaSustainingFocus = 2;

        // Spirit
        private int _intKarmaBanishingFocus = 2;

        private int _intKarmaBindingFocus = 2;
        private int _intKarmaSummoningFocus = 2;

        // Weapon
        private int _intKarmaWeaponFocus = 3;

        //Sustaining
        private int _intDicePenaltySustaining = 2;

        // Build settings.
        private CharacterBuildMethod _eBuildMethod = CharacterBuildMethod.Priority;

        private int _intBuildPoints = 25;
        private int _intQualityKarmaLimit = 25;
        private string _strPriorityArray = "ABCDE";
        private string _strPriorityTable = "Standard";
        private int _intSumtoTen = 10;
        private decimal _decNuyenMaximumBP = 10;
        private int _intAvailability = 12;

        // Dictionary of id (or names) of custom data directories, ordered by load order with the second value element being whether or not it's enabled
        private readonly TypedOrderedDictionary<string, bool> _dicCustomDataDirectoryKeys = new TypedOrderedDictionary<string, bool>();

        // Cached lists that should be updated every time _dicCustomDataDirectoryKeys is updated
        private readonly OrderedSet<CustomDataDirectoryInfo> _setEnabledCustomDataDirectories = new OrderedSet<CustomDataDirectoryInfo>();

        private readonly HashSet<Guid> _setEnabledCustomDataDirectoryGuids = new HashSet<Guid>();

        private readonly List<string> _lstEnabledCustomDataDirectoryPaths = new List<string>();

        // Sourcebook list.
        private readonly HashSet<string> _lstBooks = new HashSet<string>();

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public void OnPropertyChanged([CallerMemberName] string strPropertyName = null)
        {
            OnMultiplePropertyChanged(strPropertyName);
        }

        public void OnMultiplePropertyChanged(params string[] lstPropertyNames)
        {
            if (_blnDoingCopy)
                return;
            HashSet<string> lstNamesOfChangedProperties = null;
            foreach (string strPropertyName in lstPropertyNames)
            {
                if (lstNamesOfChangedProperties == null)
                    lstNamesOfChangedProperties = s_CharacterSettingsDependencyGraph.GetWithAllDependents(this, strPropertyName);
                else
                {
                    foreach (string strLoopChangedProperty in s_CharacterSettingsDependencyGraph.GetWithAllDependents(this, strPropertyName))
                        lstNamesOfChangedProperties.Add(strLoopChangedProperty);
                }
            }

            if (lstNamesOfChangedProperties == null || lstNamesOfChangedProperties.Count == 0)
                return;

            if (lstNamesOfChangedProperties.Contains(nameof(MaxNuyenDecimals)))
                _intCachedMaxNuyenDecimals = -1;
            if (lstNamesOfChangedProperties.Contains(nameof(MinNuyenDecimals)))
                _intCachedMinNuyenDecimals = -1;
            if (lstNamesOfChangedProperties.Contains(nameof(EssenceDecimals)))
                _intCachedEssenceDecimals = -1;
            if (lstNamesOfChangedProperties.Contains(nameof(CustomDataDirectoryKeys)))
                RecalculateEnabledCustomDataDirectories();
            if (lstNamesOfChangedProperties.Contains(nameof(Books)))
                RecalculateBookXPath();
            foreach (string strPropertyToChange in lstNamesOfChangedProperties)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strPropertyToChange));
            }
        }

        //A tree of dependencies. Once some of the properties are changed,
        //anything they depend on, also needs to raise OnChanged
        //This tree keeps track of dependencies
        private static readonly DependencyGraph<string, CharacterSettings> s_CharacterSettingsDependencyGraph =
            new DependencyGraph<string, CharacterSettings>(
                new DependencyGraphNode<string, CharacterSettings>(nameof(EnabledCustomDataDirectoryPaths),
                    new DependencyGraphNode<string, CharacterSettings>(nameof(CustomDataDirectoryKeys))
                ),
                new DependencyGraphNode<string, CharacterSettings>(nameof(EnabledCustomDataDirectoryInfos),
                    new DependencyGraphNode<string, CharacterSettings>(nameof(CustomDataDirectoryKeys))
                ),
                new DependencyGraphNode<string, CharacterSettings>(nameof(MysAdeptSecondMAGAttributeEnabled),
                    new DependencyGraphNode<string, CharacterSettings>(nameof(MysAdeptAllowPpCareer)),
                    new DependencyGraphNode<string, CharacterSettings>(nameof(PrioritySpellsAsAdeptPowers))
                ),
                new DependencyGraphNode<string, CharacterSettings>(nameof(MaxNuyenDecimals),
                    new DependencyGraphNode<string, CharacterSettings>(nameof(NuyenFormat)),
                    new DependencyGraphNode<string, CharacterSettings>(nameof(MinNuyenDecimals))
                ),
                new DependencyGraphNode<string, CharacterSettings>(nameof(MinNuyenDecimals),
                    new DependencyGraphNode<string, CharacterSettings>(nameof(NuyenFormat)),
                    new DependencyGraphNode<string, CharacterSettings>(nameof(MaxNuyenDecimals))
                ),
                new DependencyGraphNode<string, CharacterSettings>(nameof(EssenceDecimals),
                    new DependencyGraphNode<string, CharacterSettings>(nameof(EssenceFormat))
                ),
                new DependencyGraphNode<string, CharacterSettings>(nameof(BuiltInOption),
                    new DependencyGraphNode<string, CharacterSettings>(nameof(SourceId))
                ),
                new DependencyGraphNode<string, CharacterSettings>(nameof(BuildMethodUsesPriorityTables),
                    new DependencyGraphNode<string, CharacterSettings>(nameof(BuildMethod))
                ),
                new DependencyGraphNode<string, CharacterSettings>(nameof(BuildMethodIsPriority),
                    new DependencyGraphNode<string, CharacterSettings>(nameof(BuildMethod))
                ),
                new DependencyGraphNode<string, CharacterSettings>(nameof(BuildMethodIsSumtoTen),
                    new DependencyGraphNode<string, CharacterSettings>(nameof(BuildMethod))
                ),
                new DependencyGraphNode<string, CharacterSettings>(nameof(BuildMethodIsLifeModule),
                    new DependencyGraphNode<string, CharacterSettings>(nameof(BuildMethod))
                ),
                new DependencyGraphNode<string, CharacterSettings>(nameof(DisplayName),
                    new DependencyGraphNode<string, CharacterSettings>(nameof(Name)),
                    new DependencyGraphNode<string, CharacterSettings>(nameof(SourceId))
                ),
                new DependencyGraphNode<string, CharacterSettings>(nameof(RedlinerExcludesSkull),
                    new DependencyGraphNode<string, CharacterSettings>(nameof(RedlinerExcludes))
                ),
                new DependencyGraphNode<string, CharacterSettings>(nameof(RedlinerExcludesTorso),
                    new DependencyGraphNode<string, CharacterSettings>(nameof(RedlinerExcludes))
                ),
                new DependencyGraphNode<string, CharacterSettings>(nameof(RedlinerExcludesArms),
                    new DependencyGraphNode<string, CharacterSettings>(nameof(RedlinerExcludes))
                ),
                new DependencyGraphNode<string, CharacterSettings>(nameof(RedlinerExcludesLegs),
                    new DependencyGraphNode<string, CharacterSettings>(nameof(RedlinerExcludes))
                )
            );

        #region Initialization, Save, and Load Methods

        public CharacterSettings(CharacterSettings objOther = null)
        {
            if (objOther != null)
                CopyValues(objOther);
        }
        
        public void CopyValues(CharacterSettings objOther)
        {
            if (objOther == null)
                return;
            _blnDoingCopy = true;
            List<string> lstPropertiesToUpdate = new List<string>();
            try
            {
                if (!_guiSourceId.Equals(objOther._guiSourceId))
                {
                    lstPropertiesToUpdate.Add(nameof(SourceId));
                    _guiSourceId = objOther._guiSourceId;
                }

                if (!_strFileName.Equals(objOther._strFileName))
                {
                    lstPropertiesToUpdate.Add(nameof(FileName));
                    _strFileName = objOther._strFileName;
                }

                // Copy over via properties in order to trigger OnPropertyChanged as appropriate
                PropertyInfo[] aobjProperties = GetType().GetProperties();
                PropertyInfo[] aobjOtherProperties = objOther.GetType().GetProperties();
                foreach (PropertyInfo objOtherProperty in aobjOtherProperties.Where(x => x.CanRead && x.CanWrite))
                {
                    foreach (PropertyInfo objProperty in Array.FindAll(aobjProperties,
                                                                       x => x.Name == objOtherProperty.Name
                                                                            && x.PropertyType
                                                                            == objOtherProperty.PropertyType))
                    {
                        object objMyValue = objProperty.GetValue(this);
                        object objOtherValue = objOtherProperty.GetValue(objOther);
                        if (objMyValue.Equals(objOtherValue))
                            continue;
                        lstPropertiesToUpdate.Add(objProperty.Name);
                        objProperty.SetValue(this, objOtherValue);
                    }
                }

                if (!_dicCustomDataDirectoryKeys.SequenceEqual(objOther.CustomDataDirectoryKeys))
                {
                    lstPropertiesToUpdate.Add(nameof(CustomDataDirectoryKeys));
                    _dicCustomDataDirectoryKeys.Clear();
                    foreach (KeyValuePair<string, bool> kvpOther in objOther.CustomDataDirectoryKeys)
                    {
                        _dicCustomDataDirectoryKeys.Add(kvpOther.Key, kvpOther.Value);
                    }
                }

                if (!_lstBooks.SetEquals(objOther._lstBooks))
                {
                    lstPropertiesToUpdate.Add(nameof(Books));
                    _lstBooks.Clear();
                    foreach (string strBook in objOther._lstBooks)
                    {
                        _lstBooks.Add(strBook);
                    }
                }

                if (!BannedWareGrades.SetEquals(objOther.BannedWareGrades))
                {
                    lstPropertiesToUpdate.Add(nameof(BannedWareGrades));
                    BannedWareGrades.Clear();
                    foreach (string strGrade in objOther.BannedWareGrades)
                    {
                        BannedWareGrades.Add(strGrade);
                    }
                }

                // RedlinerExcludes handled through the four RedlinerExcludes[Limb] properties
            }
            finally
            {
                _blnDoingCopy = false;
            }

            OnMultiplePropertyChanged(lstPropertiesToUpdate.ToArray());
        }

        public bool HasIdenticalSettings(CharacterSettings objOther)
        {
            if (objOther == null)
                return false;
            if (_guiSourceId != objOther._guiSourceId)
                return false;
            if (_strFileName != objOther._strFileName)
                return false;
            if (GetEquatableHashCode() != objOther.GetEquatableHashCode())
                return false;

            PropertyInfo[] aobjPropertyInfos = GetType().GetProperties();
            List<PropertyInfo> lstPropertyInfos
                = new List<PropertyInfo>(aobjPropertyInfos.Length);
            lstPropertyInfos.AddRange(aobjPropertyInfos.Where(x => x.PropertyType.IsValueType));
            aobjPropertyInfos = objOther.GetType().GetProperties();
            List<PropertyInfo> lstOtherPropertyInfos
                = new List<PropertyInfo>(aobjPropertyInfos.Length);
            lstOtherPropertyInfos.AddRange(aobjPropertyInfos.Where(x => x.PropertyType.IsValueType));

            if (lstPropertyInfos.Count != lstOtherPropertyInfos.Count)
                return false;

            for (int i = lstPropertyInfos.Count - 1; i >= 0; --i)
            {
                PropertyInfo objPropertyInfo = lstPropertyInfos[i];
                int intOtherIndex
                    = lstOtherPropertyInfos.FindIndex(x => x.Name == objPropertyInfo.Name
                                                           && x.PropertyType == objPropertyInfo.PropertyType);
                if (intOtherIndex < 0)
                    return false;
                PropertyInfo objOtherPropertyInfo = lstOtherPropertyInfos[intOtherIndex];
                object objMyValue = objPropertyInfo.GetValue(this);
                object objOtherValue = objOtherPropertyInfo.GetValue(objOther);
                if (objMyValue.Equals(objOtherValue))
                {
                    // Removed checked property from the other list, both to speed up future checks and to make last check easier
                    lstOtherPropertyInfos.RemoveAt(intOtherIndex);
                }
                else
                {
                    return false;
                }
            }
            // This will only happen if there are any properties in other that haven't been accounted for in this object
            if (lstOtherPropertyInfos.Count > 0)
                return false;

            if (!_dicCustomDataDirectoryKeys.SequenceEqual(objOther._dicCustomDataDirectoryKeys))
                return false;

            // RedlinerExcludes handled through the four RedlinerExcludes[Limb] properties

            return _lstBooks.SetEquals(objOther._lstBooks) && BannedWareGrades.SetEquals(objOther.BannedWareGrades);
        }

        /// <summary>
        /// Needed because it's not a strict replacement for GetHashCode().
        /// Gets a number based on every single private property of the setting.
        /// If two settings have unequal Hash Codes, they will never actually be equal.
        /// </summary>
        /// <returns></returns>
        public int GetEquatableHashCode()
        {
            unchecked
            {
                int hashCode = _guiSourceId.GetHashCode();
                hashCode = (hashCode * 397) ^ (_strFileName?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (_strName?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ _blnAllow2ndMaxAttribute.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnAllowBiowareSuites.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnAllowCyberwareESSDiscounts.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnAllowEditPartOfBaseWeapon.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnAllowHigherStackedFoci.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnAllowInitiationInCreateMode.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnAllowObsolescentUpgrade.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnDontUseCyberlimbCalculation.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnAllowSkillRegrouping.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnAlternateMetatypeAttributeKarma.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnArmorDegradation.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnStrictSkillGroupsInCreateMode.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnAllowPointBuySpecializationsOnKarmaSkills.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnCyberlegMovement.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnDontDoubleQualityPurchaseCost.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnDontDoubleQualityRefundCost.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnEnforceCapacity.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnESSLossReducesMaximumOnly.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnExceedNegativeQualities.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnExceedNegativeQualitiesLimit.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnExceedPositiveQualities.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnExceedPositiveQualitiesCostDoubled.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnExtendAnyDetectionSpell.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnDroneArmorMultiplierEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnFreeSpiritPowerPointsMAG.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnNoArmorEncumbrance.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnIgnoreArt.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnIgnoreComplexFormLimit.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnUnarmedImprovementsApplyToWeapons.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnLicenseRestrictedItems.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnMaximumArmorModifications.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnMetatypeCostsKarma.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnMoreLethalGameplay.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnMultiplyForbiddenCost.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnMultiplyRestrictedCost.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnNoSingleArmorEncumbrance.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnRestrictRecoil.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnSpecialKarmaCostBasedOnShownValue.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnSpiritForceBasedOnTotalMAG.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnUnrestrictedNuyen.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnUseCalculatedPublicAwareness.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnUsePointsOnBrokenGroups.GetHashCode();
                hashCode = (hashCode * 397) ^ (_strContactPointsExpression?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (_strKnowledgePointsExpression?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (_strChargenKarmaToNuyenExpression?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (_strBoundSpiritExpression?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (_strRegisteredSpriteExpression?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ _blnDoNotRoundEssenceInternally.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnEnableEnemyTracking.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnEnemyKarmaQualityLimit.GetHashCode();
                hashCode = (hashCode * 397) ^ (_strEssenceFormat?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ _intForbiddenCostMultiplier;
                hashCode = (hashCode * 397) ^ _intDroneArmorMultiplier;
                hashCode = (hashCode * 397) ^ _intLimbCount;
                hashCode = (hashCode * 397) ^ _intMetatypeCostMultiplier;
                hashCode = (hashCode * 397) ^ _decNuyenPerBPWftM.GetHashCode();
                hashCode = (hashCode * 397) ^ _decNuyenPerBPWftP.GetHashCode();
                hashCode = (hashCode * 397) ^ _intRestrictedCostMultiplier;
                hashCode = (hashCode * 397) ^ _blnAutomaticBackstory.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnFreeMartialArtSpecialization.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnPrioritySpellsAsAdeptPowers.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnMysAdeptAllowPpCareer.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnMysAdeptSecondMAGAttribute.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnReverseAttributePriorityOrder.GetHashCode();
                hashCode = (hashCode * 397) ^ (_strNuyenFormat?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ _blnCompensateSkillGroupKarmaDifference.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnIncreasedImprovedAbilityMultiplier.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnAllowFreeGrids.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnAllowTechnomancerSchooling.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnCyberlimbAttributeBonusCapOverride.GetHashCode();
                hashCode = (hashCode * 397) ^ (_strBookXPath?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (_strExcludeLimbSlot?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ _intCyberlimbAttributeBonusCap;
                hashCode = (hashCode * 397) ^ _blnUnclampAttributeMinimum.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnDroneMods.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnDroneModsMaximumPilot.GetHashCode();
                hashCode = (hashCode * 397) ^ _intKarmaAttribute;
                hashCode = (hashCode * 397) ^ _intKarmaCarryover;
                hashCode = (hashCode * 397) ^ _intKarmaContact;
                hashCode = (hashCode * 397) ^ _intKarmaEnemy;
                hashCode = (hashCode * 397) ^ _intKarmaEnhancement;
                hashCode = (hashCode * 397) ^ _intKarmaImproveActiveSkill;
                hashCode = (hashCode * 397) ^ _intKarmaImproveKnowledgeSkill;
                hashCode = (hashCode * 397) ^ _intKarmaImproveSkillGroup;
                hashCode = (hashCode * 397) ^ _intKarmaInitiation;
                hashCode = (hashCode * 397) ^ _intKarmaInitiationFlat;
                hashCode = (hashCode * 397) ^ _intKarmaJoinGroup;
                hashCode = (hashCode * 397) ^ _intKarmaLeaveGroup;
                hashCode = (hashCode * 397) ^ _intKarmaTechnique;
                hashCode = (hashCode * 397) ^ _intKarmaMetamagic;
                hashCode = (hashCode * 397) ^ _intKarmaNewActiveSkill;
                hashCode = (hashCode * 397) ^ _intKarmaNewComplexForm;
                hashCode = (hashCode * 397) ^ _intKarmaNewKnowledgeSkill;
                hashCode = (hashCode * 397) ^ _intKarmaNewSkillGroup;
                hashCode = (hashCode * 397) ^ _intKarmaQuality;
                hashCode = (hashCode * 397) ^ _intKarmaSpecialization;
                hashCode = (hashCode * 397) ^ _intKarmaKnoSpecialization;
                hashCode = (hashCode * 397) ^ _intKarmaSpell;
                hashCode = (hashCode * 397) ^ _intKarmaSpirit;
                hashCode = (hashCode * 397) ^ _intKarmaNewAIProgram;
                hashCode = (hashCode * 397) ^ _intKarmaNewAIAdvancedProgram;
                hashCode = (hashCode * 397) ^ _intKarmaMysticAdeptPowerPoint;
                hashCode = (hashCode * 397) ^ _intKarmaSpiritFettering;
                hashCode = (hashCode * 397) ^ _intKarmaAlchemicalFocus;
                hashCode = (hashCode * 397) ^ _intKarmaDisenchantingFocus;
                hashCode = (hashCode * 397) ^ _intKarmaCenteringFocus;
                hashCode = (hashCode * 397) ^ _intKarmaFlexibleSignatureFocus;
                hashCode = (hashCode * 397) ^ _intKarmaMaskingFocus;
                hashCode = (hashCode * 397) ^ _intKarmaSpellShapingFocus;
                hashCode = (hashCode * 397) ^ _intKarmaPowerFocus;
                hashCode = (hashCode * 397) ^ _intKarmaQiFocus;
                hashCode = (hashCode * 397) ^ _intKarmaCounterspellingFocus;
                hashCode = (hashCode * 397) ^ _intKarmaRitualSpellcastingFocus;
                hashCode = (hashCode * 397) ^ _intKarmaSpellcastingFocus;
                hashCode = (hashCode * 397) ^ _intKarmaSustainingFocus;
                hashCode = (hashCode * 397) ^ _intKarmaBanishingFocus;
                hashCode = (hashCode * 397) ^ _intKarmaBindingFocus;
                hashCode = (hashCode * 397) ^ _intKarmaSummoningFocus;
                hashCode = (hashCode * 397) ^ _intKarmaWeaponFocus;
                hashCode = (hashCode * 397) ^ _intDicePenaltySustaining;
                hashCode = (hashCode * 397) ^ (int) _eBuildMethod;
                hashCode = (hashCode * 397) ^ _intBuildPoints;
                hashCode = (hashCode * 397) ^ _intQualityKarmaLimit;
                hashCode = (hashCode * 397) ^ (_strPriorityArray?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (_strPriorityTable?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ _intSumtoTen;
                hashCode = (hashCode * 397) ^ _decNuyenMaximumBP.GetHashCode();
                hashCode = (hashCode * 397) ^ _intAvailability;
                hashCode = (hashCode * 397) ^ (_dicCustomDataDirectoryKeys?.GetEnsembleHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (_setEnabledCustomDataDirectories?.GetEnsembleHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (_setEnabledCustomDataDirectoryGuids?.GetOrderInvariantEnsembleHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (_lstEnabledCustomDataDirectoryPaths?.GetEnsembleHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (_lstBooks?.GetOrderInvariantEnsembleHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (BannedWareGrades?.GetOrderInvariantEnsembleHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (RedlinerExcludes?.GetOrderInvariantEnsembleHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ KarmaMAGInitiationGroupPercent.GetHashCode();
                hashCode = (hashCode * 397) ^ KarmaRESInitiationGroupPercent.GetHashCode();
                hashCode = (hashCode * 397) ^ KarmaMAGInitiationOrdealPercent.GetHashCode();
                hashCode = (hashCode * 397) ^ KarmaRESInitiationOrdealPercent.GetHashCode();
                hashCode = (hashCode * 397) ^ KarmaMAGInitiationSchoolingPercent.GetHashCode();
                hashCode = (hashCode * 397) ^ KarmaRESInitiationSchoolingPercent.GetHashCode();
                hashCode = (hashCode * 397) ^ SpecializationBonus;
                return hashCode;
            }
        }

        /// <summary>
        /// Save the current settings to the settings file.
        /// </summary>
        /// <param name="strNewFileName">New file name to use. If empty, uses the existing, built-in file name.</param>
        /// <param name="blnClearSourceGuid">Whether to clear SourceId after a successful save or not. Used to turn built-in options into custom ones.</param>
        public bool Save(string strNewFileName = "", bool blnClearSourceGuid = false)
        {
            // Create the settings directory if it does not exist.
            string settingsDirectoryPath = Path.Combine(Utils.GetStartupPath, "settings");
            if (!Directory.Exists(settingsDirectoryPath))
            {
                try
                {
                    Directory.CreateDirectory(settingsDirectoryPath);
                }
                catch (UnauthorizedAccessException)
                {
                    Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_Insufficient_Permissions_Warning"));
                    return false;
                }
            }
            if (!string.IsNullOrEmpty(strNewFileName))
                _strFileName = strNewFileName;
            string strFilePath = Path.Combine(Utils.GetStartupPath, "settings", _strFileName);
            using (FileStream objStream = new FileStream(strFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                using (XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.UTF8)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 1,
                    IndentChar = '\t'
                })
                {
                    objWriter.WriteStartDocument();

                    // <settings>
                    objWriter.WriteStartElement("settings");

                    // <id />
                    objWriter.WriteElementString("id", (blnClearSourceGuid ? Guid.Empty : _guiSourceId).ToString("D", GlobalSettings.InvariantCultureInfo));
                    // <name />
                    objWriter.WriteElementString("name", _strName);

                    // <licenserestricted />
                    objWriter.WriteElementString("licenserestricted", _blnLicenseRestrictedItems.ToString(GlobalSettings.InvariantCultureInfo));
                    // <morelethalgameplay />
                    objWriter.WriteElementString("morelethalgameplay", _blnMoreLethalGameplay.ToString(GlobalSettings.InvariantCultureInfo));
                    // <spiritforcebasedontotalmag />
                    objWriter.WriteElementString("spiritforcebasedontotalmag", _blnSpiritForceBasedOnTotalMAG.ToString(GlobalSettings.InvariantCultureInfo));
                    // <nuyenperbpwftm />
                    objWriter.WriteElementString("nuyenperbpwftm", _decNuyenPerBPWftM.ToString(GlobalSettings.InvariantCultureInfo));
                    // <nuyenperbpwftp />
                    objWriter.WriteElementString("nuyenperbpwftp", _decNuyenPerBPWftP.ToString(GlobalSettings.InvariantCultureInfo));
                    // <UnarmedImprovementsApplyToWeapons />
                    objWriter.WriteElementString("unarmedimprovementsapplytoweapons", _blnUnarmedImprovementsApplyToWeapons.ToString(GlobalSettings.InvariantCultureInfo));
                    // <allowinitiationincreatemode />
                    objWriter.WriteElementString("allowinitiationincreatemode", _blnAllowInitiationInCreateMode.ToString(GlobalSettings.InvariantCultureInfo));
                    // <usepointsonbrokengroups />
                    objWriter.WriteElementString("usepointsonbrokengroups", _blnUsePointsOnBrokenGroups.ToString(GlobalSettings.InvariantCultureInfo));
                    // <dontdoublequalities />
                    objWriter.WriteElementString("dontdoublequalities", _blnDontDoubleQualityPurchaseCost.ToString(GlobalSettings.InvariantCultureInfo));
                    // <dontdoublequalities />
                    objWriter.WriteElementString("dontdoublequalityrefunds", _blnDontDoubleQualityRefundCost.ToString(GlobalSettings.InvariantCultureInfo));
                    // <ignoreart />
                    objWriter.WriteElementString("ignoreart", _blnIgnoreArt.ToString(GlobalSettings.InvariantCultureInfo));
                    // <cyberlegmovement />
                    objWriter.WriteElementString("cyberlegmovement", _blnCyberlegMovement.ToString(GlobalSettings.InvariantCultureInfo));
                    // <allow2ndmaxattribute />
                    objWriter.WriteElementString("allow2ndmaxattribute", _blnAllow2ndMaxAttribute.ToString(GlobalSettings.InvariantCultureInfo));
                    // <contactpointsexpression />
                    objWriter.WriteElementString("contactpointsexpression", _strContactPointsExpression);
                    // <knowledgepointsexpression />
                    objWriter.WriteElementString("knowledgepointsexpression", _strKnowledgePointsExpression);
                    // <chargenkarmatonuyenexpression />
                    objWriter.WriteElementString("chargenkarmatonuyenexpression", _strChargenKarmaToNuyenExpression);
                    // <boundspiritexpression />
                    objWriter.WriteElementString("boundspiritexpression", _strBoundSpiritExpression);
                    // <compiledspriteexpression />
                    objWriter.WriteElementString("compiledspriteexpression", _strRegisteredSpriteExpression);
                    // <dronearmormultiplierenabled />
                    objWriter.WriteElementString("dronearmormultiplierenabled", _blnDroneArmorMultiplierEnabled.ToString(GlobalSettings.InvariantCultureInfo));
                    // <dronearmorflatnumber />
                    objWriter.WriteElementString("dronearmorflatnumber", _intDroneArmorMultiplier.ToString(GlobalSettings.InvariantCultureInfo));
                    // <nosinglearmorencumbrance />
                    objWriter.WriteElementString("nosinglearmorencumbrance", _blnNoSingleArmorEncumbrance.ToString(GlobalSettings.InvariantCultureInfo));
                    // <ignorecomplexformlimit />
                    objWriter.WriteElementString("ignorecomplexformlimit", _blnIgnoreComplexFormLimit.ToString(GlobalSettings.InvariantCultureInfo));
                    // <NoArmorEncumbrance />
                    objWriter.WriteElementString("noarmorencumbrance", _blnNoArmorEncumbrance.ToString(GlobalSettings.InvariantCultureInfo));
                    // <esslossreducesmaximumonly />
                    objWriter.WriteElementString("esslossreducesmaximumonly", _blnESSLossReducesMaximumOnly.ToString(GlobalSettings.InvariantCultureInfo));
                    // <allowskillregrouping />
                    objWriter.WriteElementString("allowskillregrouping", _blnAllowSkillRegrouping.ToString(GlobalSettings.InvariantCultureInfo));
                    // <metatypecostskarma />
                    objWriter.WriteElementString("metatypecostskarma", _blnMetatypeCostsKarma.ToString(GlobalSettings.InvariantCultureInfo));
                    // <metatypecostskarmamultiplier />
                    objWriter.WriteElementString("metatypecostskarmamultiplier", _intMetatypeCostMultiplier.ToString(GlobalSettings.InvariantCultureInfo));
                    // <limbcount />
                    objWriter.WriteElementString("limbcount", _intLimbCount.ToString(GlobalSettings.InvariantCultureInfo));
                    // <excludelimbslot />
                    objWriter.WriteElementString("excludelimbslot", _strExcludeLimbSlot);
                    // <allowcyberwareessdiscounts />
                    objWriter.WriteElementString("allowcyberwareessdiscounts", _blnAllowCyberwareESSDiscounts.ToString(GlobalSettings.InvariantCultureInfo));
                    // <maximumarmormodifications />
                    objWriter.WriteElementString("maximumarmormodifications", _blnMaximumArmorModifications.ToString(GlobalSettings.InvariantCultureInfo));
                    // <armordegredation />
                    objWriter.WriteElementString("armordegredation", _blnArmorDegradation.ToString(GlobalSettings.InvariantCultureInfo));
                    // <specialkarmacostbasedonshownvalue />
                    objWriter.WriteElementString("specialkarmacostbasedonshownvalue", _blnSpecialKarmaCostBasedOnShownValue.ToString(GlobalSettings.InvariantCultureInfo));
                    // <exceedpositivequalities />
                    objWriter.WriteElementString("exceedpositivequalities", _blnExceedPositiveQualities.ToString(GlobalSettings.InvariantCultureInfo));
                    // <exceedpositivequalitiescostdoubled />
                    objWriter.WriteElementString("exceedpositivequalitiescostdoubled", _blnExceedPositiveQualitiesCostDoubled.ToString(GlobalSettings.InvariantCultureInfo));

                    objWriter.WriteElementString("mysaddppcareer", MysAdeptAllowPpCareer.ToString(GlobalSettings.InvariantCultureInfo));

                    // <mysadeptsecondmagattribute />
                    objWriter.WriteElementString("mysadeptsecondmagattribute", MysAdeptSecondMAGAttribute.ToString(GlobalSettings.InvariantCultureInfo));

                    // <exceednegativequalities />
                    objWriter.WriteElementString("exceednegativequalities", _blnExceedNegativeQualities.ToString(GlobalSettings.InvariantCultureInfo));
                    // <exceednegativequalitieslimit />
                    objWriter.WriteElementString("exceednegativequalitieslimit", _blnExceedNegativeQualitiesLimit.ToString(GlobalSettings.InvariantCultureInfo));
                    // <multiplyrestrictedcost />
                    objWriter.WriteElementString("multiplyrestrictedcost", _blnMultiplyRestrictedCost.ToString(GlobalSettings.InvariantCultureInfo));
                    // <multiplyforbiddencost />
                    objWriter.WriteElementString("multiplyforbiddencost", _blnMultiplyForbiddenCost.ToString(GlobalSettings.InvariantCultureInfo));
                    // <restrictedcostmultiplier />
                    objWriter.WriteElementString("restrictedcostmultiplier", _intRestrictedCostMultiplier.ToString(GlobalSettings.InvariantCultureInfo));
                    // <forbiddencostmultiplier />
                    objWriter.WriteElementString("forbiddencostmultiplier", _intForbiddenCostMultiplier.ToString(GlobalSettings.InvariantCultureInfo));
                    // <donotroundessenceinternally />
                    objWriter.WriteElementString("donotroundessenceinternally", _blnDoNotRoundEssenceInternally.ToString(GlobalSettings.InvariantCultureInfo));
                    // <enableenemytracking />
                    objWriter.WriteElementString("enableenemytracking", _blnEnableEnemyTracking.ToString(GlobalSettings.InvariantCultureInfo));
                    // <enemykarmaqualitylimit />
                    objWriter.WriteElementString("enemykarmaqualitylimit", _blnEnemyKarmaQualityLimit.ToString(GlobalSettings.InvariantCultureInfo));
                    // <nuyenformat />
                    objWriter.WriteElementString("nuyenformat", _strNuyenFormat);
                    // <essencedecimals />
                    objWriter.WriteElementString("essenceformat", _strEssenceFormat);
                    // <enforcecapacity />
                    objWriter.WriteElementString("enforcecapacity", _blnEnforceCapacity.ToString(GlobalSettings.InvariantCultureInfo));
                    // <restrictrecoil />
                    objWriter.WriteElementString("restrictrecoil", _blnRestrictRecoil.ToString(GlobalSettings.InvariantCultureInfo));
                    // <unrestrictednuyen />
                    objWriter.WriteElementString("unrestrictednuyen", _blnUnrestrictedNuyen.ToString(GlobalSettings.InvariantCultureInfo));
                    // <allowhigherstackedfoci />
                    objWriter.WriteElementString("allowhigherstackedfoci", _blnAllowHigherStackedFoci.ToString(GlobalSettings.InvariantCultureInfo));
                    // <alloweditpartofbaseweapon />
                    objWriter.WriteElementString("alloweditpartofbaseweapon", _blnAllowEditPartOfBaseWeapon.ToString(GlobalSettings.InvariantCultureInfo));
                    // <breakskillgroupsincreatemode />
                    objWriter.WriteElementString("breakskillgroupsincreatemode", _blnStrictSkillGroupsInCreateMode.ToString(GlobalSettings.InvariantCultureInfo));
                    // <allowpointbuyspecializationsonkarmaskills />
                    objWriter.WriteElementString("allowpointbuyspecializationsonkarmaskills", _blnAllowPointBuySpecializationsOnKarmaSkills.ToString(GlobalSettings.InvariantCultureInfo));
                    // <extendanydetectionspell />
                    objWriter.WriteElementString("extendanydetectionspell", _blnExtendAnyDetectionSpell.ToString(GlobalSettings.InvariantCultureInfo));
                    //<dontusecyberlimbcalculation />
                    objWriter.WriteElementString("dontusecyberlimbcalculation", _blnDontUseCyberlimbCalculation.ToString(GlobalSettings.InvariantCultureInfo));
                    // <alternatemetatypeattributekarma />
                    objWriter.WriteElementString("alternatemetatypeattributekarma", _blnAlternateMetatypeAttributeKarma.ToString(GlobalSettings.InvariantCultureInfo));
                    // <reversekarmapriorityorder />
                    objWriter.WriteElementString("reverseattributepriorityorder", ReverseAttributePriorityOrder.ToString(GlobalSettings.InvariantCultureInfo));
                    // <allowobsolescentupgrade />
                    objWriter.WriteElementString("allowobsolescentupgrade", _blnAllowObsolescentUpgrade.ToString(GlobalSettings.InvariantCultureInfo));
                    // <allowbiowaresuites />
                    objWriter.WriteElementString("allowbiowaresuites", _blnAllowBiowareSuites.ToString(GlobalSettings.InvariantCultureInfo));
                    // <freespiritpowerpointsmag />
                    objWriter.WriteElementString("freespiritpowerpointsmag", _blnFreeSpiritPowerPointsMAG.ToString(GlobalSettings.InvariantCultureInfo));
                    // <compensateskillgroupkarmadifference />
                    objWriter.WriteElementString("compensateskillgroupkarmadifference", _blnCompensateSkillGroupKarmaDifference.ToString(GlobalSettings.InvariantCultureInfo));
                    // <autobackstory />
                    objWriter.WriteElementString("autobackstory", _blnAutomaticBackstory.ToString(GlobalSettings.InvariantCultureInfo));
                    // <freemartialartspecialization />
                    objWriter.WriteElementString("freemartialartspecialization", _blnFreeMartialArtSpecialization.ToString(GlobalSettings.InvariantCultureInfo));
                    // <priorityspellsasadeptpowers />
                    objWriter.WriteElementString("priorityspellsasadeptpowers", _blnPrioritySpellsAsAdeptPowers.ToString(GlobalSettings.InvariantCultureInfo));
                    // <usecalculatedpublicawareness />
                    objWriter.WriteElementString("usecalculatedpublicawareness", _blnUseCalculatedPublicAwareness.ToString(GlobalSettings.InvariantCultureInfo));
                    // <increasedimprovedabilitymodifier />
                    objWriter.WriteElementString("increasedimprovedabilitymodifier", _blnIncreasedImprovedAbilityMultiplier.ToString(GlobalSettings.InvariantCultureInfo));
                    // <allowfreegrids />
                    objWriter.WriteElementString("allowfreegrids", _blnAllowFreeGrids.ToString(GlobalSettings.InvariantCultureInfo));
                    // <allowtechnomancerschooling />
                    objWriter.WriteElementString("allowtechnomancerschooling", _blnAllowTechnomancerSchooling.ToString(GlobalSettings.InvariantCultureInfo));
                    // <cyberlimbattributebonuscapoverride />
                    objWriter.WriteElementString("cyberlimbattributebonuscapoverride", _blnCyberlimbAttributeBonusCapOverride.ToString(GlobalSettings.InvariantCultureInfo));
                    // <cyberlimbattributebonuscap />
                    objWriter.WriteElementString("cyberlimbattributebonuscap", _intCyberlimbAttributeBonusCap.ToString(GlobalSettings.InvariantCultureInfo));
                    // <unclampattributeminimum />
                    objWriter.WriteElementString("unclampattributeminimum", _blnUnclampAttributeMinimum.ToString(GlobalSettings.InvariantCultureInfo));
                    // <dronemods />
                    objWriter.WriteElementString("dronemods", _blnDroneMods.ToString(GlobalSettings.InvariantCultureInfo));
                    // <dronemodsmaximumpilot />
                    objWriter.WriteElementString("dronemodsmaximumpilot", _blnDroneModsMaximumPilot.ToString(GlobalSettings.InvariantCultureInfo));

                    // <DicePenaltySustaining />
                    objWriter.WriteElementString("dicepenaltysustaining", _intDicePenaltySustaining.ToString(GlobalSettings.InvariantCultureInfo));

                    // <karmacost>
                    objWriter.WriteStartElement("karmacost");
                    // <karmaattribute />
                    objWriter.WriteElementString("karmaattribute", _intKarmaAttribute.ToString(GlobalSettings.InvariantCultureInfo));
                    // <karmaquality />
                    objWriter.WriteElementString("karmaquality", _intKarmaQuality.ToString(GlobalSettings.InvariantCultureInfo));
                    // <karmaspecialization />
                    objWriter.WriteElementString("karmaspecialization", _intKarmaSpecialization.ToString(GlobalSettings.InvariantCultureInfo));
                    // <karmaknospecialization />
                    objWriter.WriteElementString("karmaknospecialization", _intKarmaKnoSpecialization.ToString(GlobalSettings.InvariantCultureInfo));
                    // <karmanewknowledgeskill />
                    objWriter.WriteElementString("karmanewknowledgeskill", _intKarmaNewKnowledgeSkill.ToString(GlobalSettings.InvariantCultureInfo));
                    // <karmanewactiveskill />
                    objWriter.WriteElementString("karmanewactiveskill", _intKarmaNewActiveSkill.ToString(GlobalSettings.InvariantCultureInfo));
                    // <karmanewskillgroup />
                    objWriter.WriteElementString("karmanewskillgroup", _intKarmaNewSkillGroup.ToString(GlobalSettings.InvariantCultureInfo));
                    // <karmaimproveknowledgeskill />
                    objWriter.WriteElementString("karmaimproveknowledgeskill", _intKarmaImproveKnowledgeSkill.ToString(GlobalSettings.InvariantCultureInfo));
                    // <karmaimproveactiveskill />
                    objWriter.WriteElementString("karmaimproveactiveskill", _intKarmaImproveActiveSkill.ToString(GlobalSettings.InvariantCultureInfo));
                    // <karmaimproveskillgroup />
                    objWriter.WriteElementString("karmaimproveskillgroup", _intKarmaImproveSkillGroup.ToString(GlobalSettings.InvariantCultureInfo));
                    // <karmaspell />
                    objWriter.WriteElementString("karmaspell", _intKarmaSpell.ToString(GlobalSettings.InvariantCultureInfo));
                    // <karmaenhancement />
                    objWriter.WriteElementString("karmaenhancement", _intKarmaEnhancement.ToString(GlobalSettings.InvariantCultureInfo));
                    // <karmanewcomplexform />
                    objWriter.WriteElementString("karmanewcomplexform", _intKarmaNewComplexForm.ToString(GlobalSettings.InvariantCultureInfo));
                    // <karmanewaiprogram />
                    objWriter.WriteElementString("karmanewaiprogram", _intKarmaNewAIProgram.ToString(GlobalSettings.InvariantCultureInfo));
                    // <karmanewaiadvancedprogram />
                    objWriter.WriteElementString("karmanewaiadvancedprogram", _intKarmaNewAIAdvancedProgram.ToString(GlobalSettings.InvariantCultureInfo));
                    // <karmacontact />
                    objWriter.WriteElementString("karmacontact", _intKarmaContact.ToString(GlobalSettings.InvariantCultureInfo));
                    // <karmaenemy />
                    objWriter.WriteElementString("karmaenemy", _intKarmaEnemy.ToString(GlobalSettings.InvariantCultureInfo));
                    // <karmacarryover />
                    objWriter.WriteElementString("karmacarryover", _intKarmaCarryover.ToString(GlobalSettings.InvariantCultureInfo));
                    // <karmaspirit />
                    objWriter.WriteElementString("karmaspirit", _intKarmaSpirit.ToString(GlobalSettings.InvariantCultureInfo));
                    // <karmamaneuver />
                    objWriter.WriteElementString("karmatechnique", _intKarmaTechnique.ToString(GlobalSettings.InvariantCultureInfo));
                    // <karmainitiation />
                    objWriter.WriteElementString("karmainitiation", _intKarmaInitiation.ToString(GlobalSettings.InvariantCultureInfo));
                    // <karmainitiationflat />
                    objWriter.WriteElementString("karmainitiationflat", _intKarmaInitiationFlat.ToString(GlobalSettings.InvariantCultureInfo));
                    // <karmametamagic />
                    objWriter.WriteElementString("karmametamagic", _intKarmaMetamagic.ToString(GlobalSettings.InvariantCultureInfo));
                    // <karmajoingroup />
                    objWriter.WriteElementString("karmajoingroup", _intKarmaJoinGroup.ToString(GlobalSettings.InvariantCultureInfo));
                    // <karmaleavegroup />
                    objWriter.WriteElementString("karmaleavegroup", _intKarmaLeaveGroup.ToString(GlobalSettings.InvariantCultureInfo));
                    // <karmaalchemicalfocus />
                    objWriter.WriteElementString("karmaalchemicalfocus", _intKarmaAlchemicalFocus.ToString(GlobalSettings.InvariantCultureInfo));
                    // <karmabanishingfocus />
                    objWriter.WriteElementString("karmabanishingfocus", _intKarmaBanishingFocus.ToString(GlobalSettings.InvariantCultureInfo));
                    // <karmabindingfocus />
                    objWriter.WriteElementString("karmabindingfocus", _intKarmaBindingFocus.ToString(GlobalSettings.InvariantCultureInfo));
                    // <karmacenteringfocus />
                    objWriter.WriteElementString("karmacenteringfocus", _intKarmaCenteringFocus.ToString(GlobalSettings.InvariantCultureInfo));
                    // <karmacounterspellingfocus />
                    objWriter.WriteElementString("karmacounterspellingfocus", _intKarmaCounterspellingFocus.ToString(GlobalSettings.InvariantCultureInfo));
                    // <karmadisenchantingfocus />
                    objWriter.WriteElementString("karmadisenchantingfocus", _intKarmaDisenchantingFocus.ToString(GlobalSettings.InvariantCultureInfo));
                    // <karmaflexiblesignaturefocus />
                    objWriter.WriteElementString("karmaflexiblesignaturefocus", _intKarmaFlexibleSignatureFocus.ToString(GlobalSettings.InvariantCultureInfo));
                    // <karmamaskingfocus />
                    objWriter.WriteElementString("karmamaskingfocus", _intKarmaMaskingFocus.ToString(GlobalSettings.InvariantCultureInfo));
                    // <karmapowerfocus />
                    objWriter.WriteElementString("karmapowerfocus", _intKarmaPowerFocus.ToString(GlobalSettings.InvariantCultureInfo));
                    // <karmaqifocus />
                    objWriter.WriteElementString("karmaqifocus", _intKarmaQiFocus.ToString(GlobalSettings.InvariantCultureInfo));
                    // <karmaritualspellcastingfocus />
                    objWriter.WriteElementString("karmaritualspellcastingfocus", _intKarmaRitualSpellcastingFocus.ToString(GlobalSettings.InvariantCultureInfo));
                    // <karmaspellcastingfocus />
                    objWriter.WriteElementString("karmaspellcastingfocus", _intKarmaSpellcastingFocus.ToString(GlobalSettings.InvariantCultureInfo));
                    // <karmaspellshapingfocus />
                    objWriter.WriteElementString("karmaspellshapingfocus", _intKarmaSpellShapingFocus.ToString(GlobalSettings.InvariantCultureInfo));
                    // <karmasummoningfocus />
                    objWriter.WriteElementString("karmasummoningfocus", _intKarmaSummoningFocus.ToString(GlobalSettings.InvariantCultureInfo));
                    // <karmasustainingfocus />
                    objWriter.WriteElementString("karmasustainingfocus", _intKarmaSustainingFocus.ToString(GlobalSettings.InvariantCultureInfo));
                    // <karmaweaponfocus />
                    objWriter.WriteElementString("karmaweaponfocus", _intKarmaWeaponFocus.ToString(GlobalSettings.InvariantCultureInfo));
                    // <karmaweaponfocus />
                    objWriter.WriteElementString("karmamysadpp", _intKarmaMysticAdeptPowerPoint.ToString(GlobalSettings.InvariantCultureInfo));
                    // <karmaspiritfettering />
                    objWriter.WriteElementString("karmaspiritfettering", _intKarmaSpiritFettering.ToString(GlobalSettings.InvariantCultureInfo));
                    // </karmacost>
                    objWriter.WriteEndElement();

                    XPathNodeIterator lstAllowedBooksCodes = XmlManager
                        .LoadXPath("books.xml", EnabledCustomDataDirectoryPaths)
                        .SelectAndCacheExpression("/chummer/books/book[not(hide)]/code");
                    HashSet<string> setAllowedBooks = new HashSet<string>(lstAllowedBooksCodes.Count);
                    foreach (XPathNavigator objAllowedBook in lstAllowedBooksCodes)
                    {
                        if (_lstBooks.Contains(objAllowedBook.Value))
                            setAllowedBooks.Add(objAllowedBook.Value);
                    }

                    // <books>
                    objWriter.WriteStartElement("books");
                    foreach (string strBook in setAllowedBooks)
                        objWriter.WriteElementString("book", strBook);
                    // </books>
                    objWriter.WriteEndElement();

                    string strCustomDataRootPath = Path.Combine(Utils.GetStartupPath, "customdata");

                    // <customdatadirectorynames>
                    objWriter.WriteStartElement("customdatadirectorynames");
                    for (int i = 0; i < _dicCustomDataDirectoryKeys.Count; ++i)
                    {
                        KeyValuePair<string, bool> kvpDirectoryInfo = _dicCustomDataDirectoryKeys[i];
                        string strDirectoryName = kvpDirectoryInfo.Key;
                        bool blnDirectoryIsEnabled = kvpDirectoryInfo.Value;
                        if (!blnDirectoryIsEnabled && GlobalSettings.CustomDataDirectoryInfos.Any(
                            x => x.DirectoryPath.StartsWith(strCustomDataRootPath)
                                 && x.Name.Equals(strDirectoryName, StringComparison.OrdinalIgnoreCase)))
                            continue; // Do not save disabled custom data directories that are in the customdata folder and would be auto-populated anyway
                        objWriter.WriteStartElement("customdatadirectoryname");
                        objWriter.WriteElementString("directoryname", strDirectoryName);
                        objWriter.WriteElementString("order", i.ToString(GlobalSettings.InvariantCultureInfo));
                        objWriter.WriteElementString("enabled", blnDirectoryIsEnabled.ToString(GlobalSettings.InvariantCultureInfo));
                        objWriter.WriteEndElement();
                    }
                    // </customdatadirectorynames>
                    objWriter.WriteEndElement();

                    // <buildmethod />
                    objWriter.WriteElementString("buildmethod", _eBuildMethod.ToString());
                    // <buildpoints />
                    objWriter.WriteElementString("buildpoints", _intBuildPoints.ToString(GlobalSettings.InvariantCultureInfo));
                    // <qualitykarmalimit />
                    objWriter.WriteElementString("qualitykarmalimit", _intQualityKarmaLimit.ToString(GlobalSettings.InvariantCultureInfo));
                    // <priorityarray />
                    objWriter.WriteElementString("priorityarray", _strPriorityArray);
                    // <prioritytable />
                    objWriter.WriteElementString("prioritytable", _strPriorityTable);
                    // <sumtoten />
                    objWriter.WriteElementString("sumtoten", _intSumtoTen.ToString(GlobalSettings.InvariantCultureInfo));
                    // <availability />
                    objWriter.WriteElementString("availability", _intAvailability.ToString(GlobalSettings.InvariantCultureInfo));
                    // <nuyenmaxbp />
                    objWriter.WriteElementString("nuyenmaxbp", _decNuyenMaximumBP.ToString(GlobalSettings.InvariantCultureInfo));

                    // <bannedwaregrades>
                    objWriter.WriteStartElement("bannedwaregrades");
                    foreach (string strGrade in BannedWareGrades)
                    {
                        objWriter.WriteElementString("grade", strGrade);
                    }
                    // </bannedwaregrades>
                    objWriter.WriteEndElement();

                    // <redlinerexclusion>
                    objWriter.WriteStartElement("redlinerexclusion");
                    foreach (string strLimb in RedlinerExcludes)
                    {
                        objWriter.WriteElementString("limb", strLimb);
                    }
                    // </redlinerexclusion>
                    objWriter.WriteEndElement();

                    // </settings>
                    objWriter.WriteEndElement();

                    objWriter.WriteEndDocument();
                }
            }

            if (blnClearSourceGuid)
                _guiSourceId = Guid.Empty;
            return true;
        }

        /// <summary>
        /// Load the settings from the settings file.
        /// </summary>
        /// <param name="strFileName">Settings file to load from.</param>
        /// <param name="blnShowDialogs">Whether or not to show message boxes on failures to load.</param>
        public bool Load(string strFileName, bool blnShowDialogs = true)
        {
            _strFileName = strFileName;
            string strFilePath = Path.Combine(Utils.GetStartupPath, "settings", _strFileName);
            XPathDocument objXmlDocument;
            // Make sure the settings file exists. If not, ask the user if they would like to use the default settings file instead. A character cannot be loaded without a settings file.
            if (File.Exists(strFilePath))
            {
                try
                {
                    using (StreamReader objStreamReader = new StreamReader(strFilePath, Encoding.UTF8, true))
                    using (XmlReader objXmlReader = XmlReader.Create(objStreamReader, GlobalSettings.SafeXmlReaderSettings))
                        objXmlDocument = new XPathDocument(objXmlReader);
                }
                catch (IOException)
                {
                    if (blnShowDialogs)
                        Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_CharacterOptions_CannotLoadCharacter"), LanguageManager.GetString("MessageText_CharacterOptions_CannotLoadCharacter"), MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    return false;
                }
                catch (XmlException)
                {
                    if (blnShowDialogs)
                        Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_CharacterOptions_CannotLoadCharacter"), LanguageManager.GetString("MessageText_CharacterOptions_CannotLoadCharacter"), MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    return false;
                }
            }
            else
            {
                if (blnShowDialogs)
                    Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_CharacterOptions_CannotLoadCharacter"), LanguageManager.GetString("MessageText_CharacterOptions_CannotLoadCharacter"), MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                return false;
            }

            return Load(objXmlDocument.CreateNavigator().SelectSingleNodeAndCacheExpression(".//settings"));
        }

        /// <summary>
        /// Load the settings from a settings node.
        /// </summary>
        /// <param name="objXmlNode">Settings node to load from.</param>
        public bool Load(XPathNavigator objXmlNode)
        {
            if (objXmlNode == null)
                return false;
            string strTemp = string.Empty;
            // Setting id.
            string strId = string.Empty;
            if (objXmlNode.TryGetStringFieldQuickly("id", ref strId) && Guid.TryParse(strId, out Guid guidTemp))
                _guiSourceId = guidTemp;
            // Setting name.
            objXmlNode.TryGetStringFieldQuickly("name", ref _strName);
            // License Restricted items.
            objXmlNode.TryGetBoolFieldQuickly("licenserestricted", ref _blnLicenseRestrictedItems);
            // More Lethal Gameplay.
            objXmlNode.TryGetBoolFieldQuickly("morelethalgameplay", ref _blnMoreLethalGameplay);
            // Spirit Force Based on Total MAG.
            objXmlNode.TryGetBoolFieldQuickly("spiritforcebasedontotalmag", ref _blnSpiritForceBasedOnTotalMAG);
            // Nuyen per Build Point
            if (!objXmlNode.TryGetDecFieldQuickly("nuyenperbpwftm", ref _decNuyenPerBPWftM))
            {
                objXmlNode.TryGetDecFieldQuickly("nuyenperbp", ref _decNuyenPerBPWftM);
                _decNuyenPerBPWftP = _decNuyenPerBPWftM;
            }
            else
                objXmlNode.TryGetDecFieldQuickly("nuyenperbpwftp", ref _decNuyenPerBPWftP);
            // Knucks use Unarmed
            objXmlNode.TryGetBoolFieldQuickly("unarmedimprovementsapplytoweapons", ref _blnUnarmedImprovementsApplyToWeapons);
            // Allow Initiation in Create Mode
            objXmlNode.TryGetBoolFieldQuickly("allowinitiationincreatemode", ref _blnAllowInitiationInCreateMode);
            // Use Points on Broken Groups
            objXmlNode.TryGetBoolFieldQuickly("usepointsonbrokengroups", ref _blnUsePointsOnBrokenGroups);
            // Don't Double the Cost of purchasing Qualities in Career Mode
            objXmlNode.TryGetBoolFieldQuickly("dontdoublequalities", ref _blnDontDoubleQualityPurchaseCost);
            // Don't Double the Cost of removing Qualities in Career Mode
            objXmlNode.TryGetBoolFieldQuickly("dontdoublequalityrefunds", ref _blnDontDoubleQualityRefundCost);
            // Ignore Art Requirements from Street Grimoire
            objXmlNode.TryGetBoolFieldQuickly("ignoreart", ref _blnIgnoreArt);
            // Use Cyberleg Stats for Movement
            objXmlNode.TryGetBoolFieldQuickly("cyberlegmovement", ref _blnCyberlegMovement);
            // Allow a 2nd Max Attribute
            objXmlNode.TryGetBoolFieldQuickly("allow2ndmaxattribute", ref _blnAllow2ndMaxAttribute);
            // XPath expression for contact points
            if (!objXmlNode.TryGetStringFieldQuickly("contactpointsexpression", ref _strContactPointsExpression))
            {
                // Legacy shim
                int intTemp = 3;
                bool blnTemp = false;
                strTemp = "{CHAUnaug}";
                if (objXmlNode.TryGetBoolFieldQuickly("usetotalvalueforcontacts", ref blnTemp) && blnTemp)
                    strTemp = "{CHA}";
                if (objXmlNode.TryGetBoolFieldQuickly("freecontactsmultiplierenabled", ref blnTemp) && blnTemp)
                    objXmlNode.TryGetInt32FieldQuickly("freekarmacontactsmultiplier", ref intTemp);
                _strContactPointsExpression = strTemp + " * " + intTemp.ToString(GlobalSettings.InvariantCultureInfo);
            }
            // XPath expression for knowledge points
            if (!objXmlNode.TryGetStringFieldQuickly("knowledgepointsexpression", ref _strKnowledgePointsExpression))
            {
                // Legacy shim
                int intTemp = 2;
                bool blnTemp = false;
                strTemp = "({INTUnaug} + {LOGUnaug})";
                if (objXmlNode.TryGetBoolFieldQuickly("usetotalvalueforknowledge", ref blnTemp) && blnTemp)
                    strTemp = "({INT} + {LOG})";
                if (objXmlNode.TryGetBoolFieldQuickly("freekarmaknowledgemultiplierenabled", ref blnTemp) && blnTemp)
                    objXmlNode.TryGetInt32FieldQuickly("freekarmaknowledgemultiplier", ref intTemp);
                _strKnowledgePointsExpression = strTemp + " * " + intTemp.ToString(GlobalSettings.InvariantCultureInfo);
            }
            // XPath expression for nuyen at chargen
            if (!objXmlNode.TryGetStringFieldQuickly("chargenkarmatonuyenexpression", ref _strChargenKarmaToNuyenExpression))
            {
                // Legacy shim
                _strChargenKarmaToNuyenExpression = "{Karma} * " + _decNuyenPerBPWftM.ToString(GlobalSettings.InvariantCultureInfo) + " + {PriorityNuyen}";
            }
            // A very hacky legacy shim, but also works as a bit of a sanity check
            else if (!_strChargenKarmaToNuyenExpression.Contains("{PriorityNuyen}"))
            {
                _strChargenKarmaToNuyenExpression = "(" + _strChargenKarmaToNuyenExpression + ") + {PriorityNuyen}";
            }
            objXmlNode.TryGetStringFieldQuickly("compiledspriteexpression", ref _strRegisteredSpriteExpression);
            objXmlNode.TryGetStringFieldQuickly("boundspiritexpression", ref _strBoundSpiritExpression);
            // Drone Armor Multiplier Enabled
            objXmlNode.TryGetBoolFieldQuickly("dronearmormultiplierenabled", ref _blnDroneArmorMultiplierEnabled);
            // Drone Armor Multiplier Value
            objXmlNode.TryGetInt32FieldQuickly("dronearmorflatnumber", ref _intDroneArmorMultiplier);
            // No Single Armor Encumbrance
            objXmlNode.TryGetBoolFieldQuickly("nosinglearmorencumbrance", ref _blnNoSingleArmorEncumbrance);
            // Ignore Armor Encumbrance
            objXmlNode.TryGetBoolFieldQuickly("noarmorencumbrance", ref _blnNoArmorEncumbrance);
            // Ignore Complex Form Limit
            objXmlNode.TryGetBoolFieldQuickly("ignorecomplexformlimit", ref _blnIgnoreComplexFormLimit);
            // Essence Loss Reduces Maximum Only.
            objXmlNode.TryGetBoolFieldQuickly("esslossreducesmaximumonly", ref _blnESSLossReducesMaximumOnly);
            // Allow Skill Regrouping.
            objXmlNode.TryGetBoolFieldQuickly("allowskillregrouping", ref _blnAllowSkillRegrouping);
            // Metatype Costs Karma.
            objXmlNode.TryGetBoolFieldQuickly("metatypecostskarma", ref _blnMetatypeCostsKarma);
            // Allow characters to spend karma before attribute points.
            objXmlNode.TryGetBoolFieldQuickly("reverseattributepriorityorder", ref _blnReverseAttributePriorityOrder);
            // Metatype Costs Karma Multiplier.
            objXmlNode.TryGetInt32FieldQuickly("metatypecostskarmamultiplier", ref _intMetatypeCostMultiplier);
            // Limb Count.
            objXmlNode.TryGetInt32FieldQuickly("limbcount", ref _intLimbCount);
            // Exclude Limb Slot.
            objXmlNode.TryGetStringFieldQuickly("excludelimbslot", ref _strExcludeLimbSlot);
            // Allow Cyberware Essence Cost Discounts.
            objXmlNode.TryGetBoolFieldQuickly("allowcyberwareessdiscounts", ref _blnAllowCyberwareESSDiscounts);
            // Use Maximum Armor Modifications.
            objXmlNode.TryGetBoolFieldQuickly("maximumarmormodifications", ref _blnMaximumArmorModifications);
            // Allow Armor Degradation.
            objXmlNode.TryGetBoolFieldQuickly("armordegredation", ref _blnArmorDegradation);
            // Whether or not Karma costs for increasing Special Attributes is based on the shown value instead of actual value.
            objXmlNode.TryGetBoolFieldQuickly("specialkarmacostbasedonshownvalue", ref _blnSpecialKarmaCostBasedOnShownValue);
            // Allow more than 35 BP in Positive Qualities.
            objXmlNode.TryGetBoolFieldQuickly("exceedpositivequalities", ref _blnExceedPositiveQualities);
            // Double all positive qualities in excess of the limit
            objXmlNode.TryGetBoolFieldQuickly("exceedpositivequalitiescostdoubled", ref _blnExceedPositiveQualitiesCostDoubled);

            objXmlNode.TryGetBoolFieldQuickly("mysaddppcareer", ref _blnMysAdeptAllowPpCareer);

            // Split MAG for Mystic Adepts so that they have a separate MAG rating for Adept Powers instead of using the special PP rules for mystic adepts
            objXmlNode.TryGetBoolFieldQuickly("mysadeptsecondmagattribute", ref _blnMysAdeptSecondMAGAttribute);

            // Grant a free specialization when taking a martial art.
            objXmlNode.TryGetBoolFieldQuickly("freemartialartspecialization", ref _blnFreeMartialArtSpecialization);
            // Can spend spells from Magic priority as power points
            objXmlNode.TryGetBoolFieldQuickly("priorityspellsasadeptpowers", ref _blnPrioritySpellsAsAdeptPowers);
            // Allow more than 35 BP in Negative Qualities.
            objXmlNode.TryGetBoolFieldQuickly("exceednegativequalities", ref _blnExceedNegativeQualities);
            // Character can still only receive 35 BP from Negative Qualities (though they can still add as many as they'd like).
            objXmlNode.TryGetBoolFieldQuickly("exceednegativequalitieslimit", ref _blnExceedNegativeQualitiesLimit);
            // Whether or not Restricted items have their cost multiplied.
            objXmlNode.TryGetBoolFieldQuickly("multiplyrestrictedcost", ref _blnMultiplyRestrictedCost);
            // Whether or not Forbidden items have their cost multiplied.
            objXmlNode.TryGetBoolFieldQuickly("multiplyforbiddencost", ref _blnMultiplyForbiddenCost);
            // Restricted cost multiplier.
            objXmlNode.TryGetInt32FieldQuickly("restrictedcostmultiplier", ref _intRestrictedCostMultiplier);
            // Forbidden cost multiplier.
            objXmlNode.TryGetInt32FieldQuickly("forbiddencostmultiplier", ref _intForbiddenCostMultiplier);
            // Only round essence when its value is displayed
            objXmlNode.TryGetBoolFieldQuickly("donotroundessenceinternally", ref _blnDoNotRoundEssenceInternally);
            // Allow use of enemies
            objXmlNode.TryGetBoolFieldQuickly("enableenemytracking", ref _blnEnableEnemyTracking);
            // Have enemies contribute to negative quality limit
            objXmlNode.TryGetBoolFieldQuickly("enemykarmaqualitylimit", ref _blnEnemyKarmaQualityLimit);
            // Format in which nuyen values are displayed
            objXmlNode.TryGetStringFieldQuickly("nuyenformat", ref _strNuyenFormat);
            // Format in which essence values should be displayed (and to which they should be rounded)
            if (!objXmlNode.TryGetStringFieldQuickly("essenceformat", ref _strEssenceFormat))
            {
                int intTemp = 2;
                // Number of decimal places to round to when calculating Essence.
                objXmlNode.TryGetInt32FieldQuickly("essencedecimals", ref intTemp);
                EssenceDecimals = intTemp;
            }
            else
            {
                int intDecimalPlaces = _strEssenceFormat.IndexOf('.');
                if (intDecimalPlaces < 2)
                {
                    if (intDecimalPlaces == -1)
                        _strEssenceFormat += ".00";
                    else
                    {
                        StringBuilder sbdZeros = new StringBuilder();
                        for (int i = _strEssenceFormat.Length - 1 - intDecimalPlaces; i < intDecimalPlaces; ++i)
                            sbdZeros.Append('0');
                        _strEssenceFormat += sbdZeros.ToString();
                    }
                }
            }
            // Whether or not Capacity limits should be enforced.
            objXmlNode.TryGetBoolFieldQuickly("enforcecapacity", ref _blnEnforceCapacity);
            // Whether or not Recoil modifiers are restricted (AR 148).
            objXmlNode.TryGetBoolFieldQuickly("restrictrecoil", ref _blnRestrictRecoil);
            // Whether or not character are not restricted to the number of points they can invest in Nuyen.
            objXmlNode.TryGetBoolFieldQuickly("unrestrictednuyen", ref _blnUnrestrictedNuyen);
            // Whether or not Stacked Foci can go a combined Force higher than 6.
            objXmlNode.TryGetBoolFieldQuickly("allowhigherstackedfoci", ref _blnAllowHigherStackedFoci);
            // Whether or not the user can change the status of a Weapon Mod or Accessory being part of the base Weapon.
            objXmlNode.TryGetBoolFieldQuickly("alloweditpartofbaseweapon", ref _blnAllowEditPartOfBaseWeapon);
            // Whether or not the user can break Skill Groups while in Create Mode.
            objXmlNode.TryGetBoolFieldQuickly("breakskillgroupsincreatemode", ref _blnStrictSkillGroupsInCreateMode);
            // Whether or not the user is allowed to buy specializations with skill points for skills only bought with karma.
            objXmlNode.TryGetBoolFieldQuickly("allowpointbuyspecializationsonkarmaskills", ref _blnAllowPointBuySpecializationsOnKarmaSkills);
            // Whether or not any Detection Spell can be taken as Extended range version.
            objXmlNode.TryGetBoolFieldQuickly("extendanydetectionspell", ref _blnExtendAnyDetectionSpell);
            // Whether or not cyberlimbs are used for augmented attribute calculation.
            objXmlNode.TryGetBoolFieldQuickly("dontusecyberlimbcalculation", ref _blnDontUseCyberlimbCalculation);
            // House rule: Treat the Metatype Attribute Minimum as 1 for the purpose of calculating Karma costs.
            objXmlNode.TryGetBoolFieldQuickly("alternatemetatypeattributekarma", ref _blnAlternateMetatypeAttributeKarma);
            // Whether or not Obsolescent can be removed/upgrade in the same manner as Obsolete.
            objXmlNode.TryGetBoolFieldQuickly("allowobsolescentupgrade", ref _blnAllowObsolescentUpgrade);
            // Whether or not Bioware Suites can be created and added.
            objXmlNode.TryGetBoolFieldQuickly("allowbiowaresuites", ref _blnAllowBiowareSuites);
            // House rule: Free Spirits calculate their Power Points based on their MAG instead of EDG.
            objXmlNode.TryGetBoolFieldQuickly("freespiritpowerpointsmag", ref _blnFreeSpiritPowerPointsMAG);
            // House rule: Whether to compensate for the karma cost difference between raising skill ratings and skill groups when increasing the rating of the last skill in the group
            objXmlNode.TryGetBoolFieldQuickly("compensateskillgroupkarmadifference", ref _blnCompensateSkillGroupKarmaDifference);
            // Optional Rule: Whether Life Modules should automatically create a character back story.
            objXmlNode.TryGetBoolFieldQuickly("autobackstory", ref _blnAutomaticBackstory);
            // House Rule: Whether Public Awareness should be a calculated attribute based on Street Cred and Notoriety.
            objXmlNode.TryGetBoolFieldQuickly("usecalculatedpublicawareness", ref _blnUseCalculatedPublicAwareness);
            // House Rule: Whether Improved Ability should be capped at 0.5 (false) or 1.5 (true) of the target skill's Learned Rating.
            objXmlNode.TryGetBoolFieldQuickly("increasedimprovedabilitymodifier", ref _blnIncreasedImprovedAbilityMultiplier);
            // House Rule: Whether lifestyles will give free grid subscriptions found in HT to players.
            objXmlNode.TryGetBoolFieldQuickly("allowfreegrids", ref _blnAllowFreeGrids);
            // House Rule: Whether Technomancers should be allowed to receive Schooling discounts in the same manner as Awakened.
            objXmlNode.TryGetBoolFieldQuickly("allowtechnomancerschooling", ref _blnAllowTechnomancerSchooling);
            // House Rule: Maximum value that cyberlimbs can have as a bonus on top of their Customization.
            objXmlNode.TryGetInt32FieldQuickly("cyberlimbattributebonuscap", ref _intCyberlimbAttributeBonusCap);
            if (!objXmlNode.TryGetBoolFieldQuickly("cyberlimbattributebonuscapoverride", ref _blnCyberlimbAttributeBonusCapOverride))
                _blnCyberlimbAttributeBonusCapOverride = _intCyberlimbAttributeBonusCap == 4;
            // House/Optional Rule: Attribute values are allowed to go below 0 due to Essence Loss.
            objXmlNode.TryGetBoolFieldQuickly("unclampattributeminimum", ref _blnUnclampAttributeMinimum);
            // Following two settings used to be stored in global options, so they are fetched from the registry if they are not present
            // Use Rigger 5.0 drone mods
            if (!objXmlNode.TryGetBoolFieldQuickly("dronemods", ref _blnDroneMods))
                GlobalSettings.LoadBoolFromRegistry(ref _blnDroneMods, "dronemods", string.Empty, true);
            // Apply maximum drone attribute improvement rule to Pilot, too
            if (!objXmlNode.TryGetBoolFieldQuickly("dronemodsmaximumpilot", ref _blnDroneModsMaximumPilot))
                GlobalSettings.LoadBoolFromRegistry(ref _blnDroneModsMaximumPilot, "dronemodsPilot", string.Empty, true);

            //House Rule: The DicePenalty per sustained spell or form
            objXmlNode.TryGetInt32FieldQuickly("dicepenaltysustaining", ref _intDicePenaltySustaining);

            XPathNavigator xmlKarmaCostNode = objXmlNode.SelectSingleNodeAndCacheExpression("karmacost");
            // Attempt to populate the Karma values.
            if (xmlKarmaCostNode != null)
            {
                xmlKarmaCostNode.TryGetInt32FieldQuickly("karmaattribute", ref _intKarmaAttribute);
                xmlKarmaCostNode.TryGetInt32FieldQuickly("karmaquality", ref _intKarmaQuality);
                xmlKarmaCostNode.TryGetInt32FieldQuickly("karmaspecialization", ref _intKarmaSpecialization);
                xmlKarmaCostNode.TryGetInt32FieldQuickly("karmaknospecialization", ref _intKarmaKnoSpecialization);
                xmlKarmaCostNode.TryGetInt32FieldQuickly("karmanewknowledgeskill", ref _intKarmaNewKnowledgeSkill);
                xmlKarmaCostNode.TryGetInt32FieldQuickly("karmanewactiveskill", ref _intKarmaNewActiveSkill);
                xmlKarmaCostNode.TryGetInt32FieldQuickly("karmanewskillgroup", ref _intKarmaNewSkillGroup);
                xmlKarmaCostNode.TryGetInt32FieldQuickly("karmaimproveknowledgeskill", ref _intKarmaImproveKnowledgeSkill);
                xmlKarmaCostNode.TryGetInt32FieldQuickly("karmaimproveactiveskill", ref _intKarmaImproveActiveSkill);
                xmlKarmaCostNode.TryGetInt32FieldQuickly("karmaimproveskillgroup", ref _intKarmaImproveSkillGroup);
                xmlKarmaCostNode.TryGetInt32FieldQuickly("karmaspell", ref _intKarmaSpell);
                xmlKarmaCostNode.TryGetInt32FieldQuickly("karmanewcomplexform", ref _intKarmaNewComplexForm);
                xmlKarmaCostNode.TryGetInt32FieldQuickly("karmanewaiprogram", ref _intKarmaNewAIProgram);
                xmlKarmaCostNode.TryGetInt32FieldQuickly("karmanewaiadvancedprogram", ref _intKarmaNewAIAdvancedProgram);
                xmlKarmaCostNode.TryGetInt32FieldQuickly("karmacontact", ref _intKarmaContact);
                xmlKarmaCostNode.TryGetInt32FieldQuickly("karmaenemy", ref _intKarmaEnemy);
                xmlKarmaCostNode.TryGetInt32FieldQuickly("karmacarryover", ref _intKarmaCarryover);
                xmlKarmaCostNode.TryGetInt32FieldQuickly("karmaspirit", ref _intKarmaSpirit);
                if (!xmlKarmaCostNode.TryGetInt32FieldQuickly("karmatechnique", ref _intKarmaTechnique))
                    xmlKarmaCostNode.TryGetInt32FieldQuickly("karmamaneuver", ref _intKarmaTechnique);
                xmlKarmaCostNode.TryGetInt32FieldQuickly("karmainitiation", ref _intKarmaInitiation);
                xmlKarmaCostNode.TryGetInt32FieldQuickly("karmainitiationflat", ref _intKarmaInitiationFlat);
                xmlKarmaCostNode.TryGetInt32FieldQuickly("karmametamagic", ref _intKarmaMetamagic);
                xmlKarmaCostNode.TryGetInt32FieldQuickly("karmajoingroup", ref _intKarmaJoinGroup);
                xmlKarmaCostNode.TryGetInt32FieldQuickly("karmaleavegroup", ref _intKarmaLeaveGroup);
                xmlKarmaCostNode.TryGetInt32FieldQuickly("karmaenhancement", ref _intKarmaEnhancement);
                xmlKarmaCostNode.TryGetInt32FieldQuickly("karmamysadpp", ref _intKarmaMysticAdeptPowerPoint);
                xmlKarmaCostNode.TryGetInt32FieldQuickly("karmaspiritfettering", ref _intKarmaSpiritFettering);

                // Attempt to load the Karma costs for Foci.
                xmlKarmaCostNode.TryGetInt32FieldQuickly("karmaalchemicalfocus", ref _intKarmaAlchemicalFocus);
                xmlKarmaCostNode.TryGetInt32FieldQuickly("karmabanishingfocus", ref _intKarmaBanishingFocus);
                xmlKarmaCostNode.TryGetInt32FieldQuickly("karmabindingfocus", ref _intKarmaBindingFocus);
                xmlKarmaCostNode.TryGetInt32FieldQuickly("karmacenteringfocus", ref _intKarmaCenteringFocus);
                xmlKarmaCostNode.TryGetInt32FieldQuickly("karmacounterspellingfocus", ref _intKarmaCounterspellingFocus);
                xmlKarmaCostNode.TryGetInt32FieldQuickly("karmadisenchantingfocus", ref _intKarmaDisenchantingFocus);
                xmlKarmaCostNode.TryGetInt32FieldQuickly("karmaflexiblesignaturefocus", ref _intKarmaFlexibleSignatureFocus);
                xmlKarmaCostNode.TryGetInt32FieldQuickly("karmamaskingfocus", ref _intKarmaMaskingFocus);
                xmlKarmaCostNode.TryGetInt32FieldQuickly("karmapowerfocus", ref _intKarmaPowerFocus);
                xmlKarmaCostNode.TryGetInt32FieldQuickly("karmaqifocus", ref _intKarmaQiFocus);
                xmlKarmaCostNode.TryGetInt32FieldQuickly("karmaritualspellcastingfocus", ref _intKarmaRitualSpellcastingFocus);
                xmlKarmaCostNode.TryGetInt32FieldQuickly("karmaspellcastingfocus", ref _intKarmaSpellcastingFocus);
                xmlKarmaCostNode.TryGetInt32FieldQuickly("karmaspellshapingfocus", ref _intKarmaSpellShapingFocus);
                xmlKarmaCostNode.TryGetInt32FieldQuickly("karmasummoningfocus", ref _intKarmaSummoningFocus);
                xmlKarmaCostNode.TryGetInt32FieldQuickly("karmasustainingfocus", ref _intKarmaSustainingFocus);
                xmlKarmaCostNode.TryGetInt32FieldQuickly("karmaweaponfocus", ref _intKarmaWeaponFocus);
            }

            XPathNavigator xmlLegacyCharacterNavigator = null;
            // Legacy sweep by looking at MRU
            if (!BuiltInOption && objXmlNode.SelectSingleNodeAndCacheExpression("books/book") == null && objXmlNode.SelectSingleNodeAndCacheExpression("customdatadirectorynames/directoryname") == null)
            {
                foreach (string strMruCharacterFile in GlobalSettings.MostRecentlyUsedCharacters)
                {
                    XPathDocument objXmlDocument;
                    if (!File.Exists(strMruCharacterFile))
                        continue;
                    try
                    {
                        using (StreamReader sr = new StreamReader(strMruCharacterFile, Encoding.UTF8, true))
                        using (XmlReader objXmlReader = XmlReader.Create(sr, GlobalSettings.SafeXmlReaderSettings))
                            objXmlDocument = new XPathDocument(objXmlReader);
                    }
                    catch (XmlException)
                    {
                        continue;
                    }
                    xmlLegacyCharacterNavigator = objXmlDocument.CreateNavigator().SelectSingleNodeAndCacheExpression("/character");

                    if (xmlLegacyCharacterNavigator == null)
                        continue;

                    string strLoopSettingsFile = xmlLegacyCharacterNavigator.SelectSingleNodeAndCacheExpression("settings")?.Value;
                    if (strLoopSettingsFile == _strFileName)
                        break;
                    xmlLegacyCharacterNavigator = null;
                }

                if (xmlLegacyCharacterNavigator == null)
                {
                    foreach (string strMruCharacterFile in GlobalSettings.FavoriteCharacters)
                    {
                        XPathDocument objXmlDocument;
                        if (!File.Exists(strMruCharacterFile))
                            continue;
                        try
                        {
                            using (StreamReader sr = new StreamReader(strMruCharacterFile, Encoding.UTF8, true))
                            using (XmlReader objXmlReader = XmlReader.Create(sr, GlobalSettings.SafeXmlReaderSettings))
                                objXmlDocument = new XPathDocument(objXmlReader);
                        }
                        catch (XmlException)
                        {
                            continue;
                        }
                        xmlLegacyCharacterNavigator = objXmlDocument.CreateNavigator().SelectSingleNodeAndCacheExpression("/character");

                        if (xmlLegacyCharacterNavigator == null)
                            continue;

                        string strLoopSettingsFile = xmlLegacyCharacterNavigator.SelectSingleNodeAndCacheExpression("settings")?.Value;
                        if (strLoopSettingsFile == _strFileName)
                            break;
                        xmlLegacyCharacterNavigator = null;
                    }
                }
            }

            // Load Books.
            _lstBooks.Clear();
            foreach (XPathNavigator xmlBook in objXmlNode.SelectAndCacheExpression("books/book"))
                _lstBooks.Add(xmlBook.Value);
            // Legacy sweep for sourcebooks
            if (xmlLegacyCharacterNavigator != null)
            {
                foreach (XPathNavigator xmlBook in xmlLegacyCharacterNavigator.SelectAndCacheExpression("sources/source"))
                {
                    if (!string.IsNullOrEmpty(xmlBook.Value))
                        _lstBooks.Add(xmlBook.Value);
                }
            }

            // Load Custom Data Directory names.
            int intTopMostOrder = 0;
            int intBottomMostOrder = 0;
            Dictionary<int, Tuple<string, bool>> dicLoadingCustomDataDirectories =
                new Dictionary<int, Tuple<string, bool>>();
            bool blnNeedToProcessInfosWithoutLoadOrder = false;
            foreach (XPathNavigator objXmlDirectoryName in objXmlNode.SelectAndCacheExpression("customdatadirectorynames/customdatadirectoryname"))
            {
                string strDirectoryKey = objXmlDirectoryName.SelectSingleNodeAndCacheExpression("directoryname")?.Value;
                if (string.IsNullOrEmpty(strDirectoryKey))
                    continue;
                string strLoopId = CustomDataDirectoryInfo.GetIdFromCharacterSettingsSaveKey(strDirectoryKey);
                // Only load in directories that are either present in our GlobalSettings or are enabled
                bool blnLoopEnabled = objXmlDirectoryName.SelectSingleNodeAndCacheExpression("enabled")?.Value == bool.TrueString;
                if (blnLoopEnabled || (string.IsNullOrEmpty(strLoopId)
                    ? GlobalSettings.CustomDataDirectoryInfos.Any(x => x.Name.Equals(strDirectoryKey, StringComparison.OrdinalIgnoreCase))
                    : GlobalSettings.CustomDataDirectoryInfos.Any(x => x.InternalId.Equals(strLoopId, StringComparison.OrdinalIgnoreCase))))
                {
                    string strOrder = objXmlDirectoryName.SelectSingleNodeAndCacheExpression("order")?.Value;
                    if (!string.IsNullOrEmpty(strOrder)
                        && int.TryParse(strOrder, NumberStyles.Integer, GlobalSettings.InvariantCultureInfo, out int intOrder))
                    {
                        while (dicLoadingCustomDataDirectories.ContainsKey(intOrder))
                            ++intOrder;
                        intTopMostOrder = Math.Max(intOrder, intTopMostOrder);
                        intBottomMostOrder = Math.Min(intOrder, intBottomMostOrder);
                        dicLoadingCustomDataDirectories.Add(intOrder,
                            new Tuple<string, bool>(strDirectoryKey, blnLoopEnabled));
                    }
                    else
                        blnNeedToProcessInfosWithoutLoadOrder = true;
                }
            }

            _dicCustomDataDirectoryKeys.Clear();
            for (int i = intBottomMostOrder; i <= intTopMostOrder; ++i)
            {
                if (!dicLoadingCustomDataDirectories.ContainsKey(i))
                    continue;
                string strDirectoryKey = dicLoadingCustomDataDirectories[i].Item1;
                string strLoopId = CustomDataDirectoryInfo.GetIdFromCharacterSettingsSaveKey(strDirectoryKey);
                if (string.IsNullOrEmpty(strLoopId))
                {
                    CustomDataDirectoryInfo objExistingInfo = null;
                    foreach (CustomDataDirectoryInfo objLoopInfo in GlobalSettings.CustomDataDirectoryInfos)
                    {
                        if (!objLoopInfo.Name.Equals(strDirectoryKey, StringComparison.OrdinalIgnoreCase))
                            continue;
                        if (objExistingInfo == null || objLoopInfo.MyVersion > objExistingInfo.MyVersion)
                            objExistingInfo = objLoopInfo;
                    }
                    if (objExistingInfo != null)
                        strDirectoryKey = objExistingInfo.CharacterSettingsSaveKey;
                }
                if (!_dicCustomDataDirectoryKeys.ContainsKey(strDirectoryKey))
                    _dicCustomDataDirectoryKeys.Add(strDirectoryKey, dicLoadingCustomDataDirectories[i].Item2);
            }

            // Legacy sweep for custom data directories
            if (xmlLegacyCharacterNavigator != null)
            {
                foreach (XPathNavigator xmlCustomDataDirectoryName in xmlLegacyCharacterNavigator.SelectAndCacheExpression("customdatadirectorynames/directoryname"))
                {
                    string strDirectoryKey = xmlCustomDataDirectoryName.Value;
                    if (string.IsNullOrEmpty(strDirectoryKey))
                        continue;
                    string strLoopId = CustomDataDirectoryInfo.GetIdFromCharacterSettingsSaveKey(strDirectoryKey);
                    if (string.IsNullOrEmpty(strLoopId))
                    {
                        CustomDataDirectoryInfo objExistingInfo = null;
                        foreach (CustomDataDirectoryInfo objLoopInfo in GlobalSettings.CustomDataDirectoryInfos)
                        {
                            if (!objLoopInfo.Name.Equals(strDirectoryKey, StringComparison.OrdinalIgnoreCase))
                                continue;
                            if (objExistingInfo == null || objLoopInfo.MyVersion > objExistingInfo.MyVersion)
                                objExistingInfo = objLoopInfo;
                        }
                        if (objExistingInfo != null)
                            strDirectoryKey = objExistingInfo.CharacterSettingsSaveKey;
                    }
                    if (!_dicCustomDataDirectoryKeys.ContainsKey(strDirectoryKey))
                        _dicCustomDataDirectoryKeys.Add(strDirectoryKey, true);
                }
            }

            // Add in the stragglers that didn't have any load order info
            if (blnNeedToProcessInfosWithoutLoadOrder)
            {
                foreach (XPathNavigator objXmlDirectoryName in objXmlNode.SelectAndCacheExpression("customdatadirectorynames/customdatadirectoryname"))
                {
                    string strDirectoryKey = objXmlDirectoryName.SelectSingleNodeAndCacheExpression("directoryname")?.Value;
                    if (string.IsNullOrEmpty(strDirectoryKey))
                        continue;
                    string strLoopId = CustomDataDirectoryInfo.GetIdFromCharacterSettingsSaveKey(strDirectoryKey);
                    string strOrder = objXmlDirectoryName.SelectSingleNodeAndCacheExpression("order")?.Value;
                    if (!string.IsNullOrEmpty(strOrder) && int.TryParse(strOrder, NumberStyles.Integer,
                        GlobalSettings.InvariantCultureInfo, out int _))
                        continue;
                    // Only load in directories that are either present in our GlobalSettings or are enabled
                    bool blnLoopEnabled = objXmlDirectoryName.SelectSingleNodeAndCacheExpression("enabled")?.Value == bool.TrueString;
                    if (blnLoopEnabled || (string.IsNullOrEmpty(strLoopId)
                        ? GlobalSettings.CustomDataDirectoryInfos.Any(x => x.Name.Equals(strDirectoryKey, StringComparison.OrdinalIgnoreCase))
                        : GlobalSettings.CustomDataDirectoryInfos.Any(x => x.InternalId.Equals(strLoopId, StringComparison.OrdinalIgnoreCase))))
                    {
                        if (string.IsNullOrEmpty(strLoopId))
                        {
                            CustomDataDirectoryInfo objExistingInfo = null;
                            foreach (CustomDataDirectoryInfo objLoopInfo in GlobalSettings.CustomDataDirectoryInfos)
                            {
                                if (!objLoopInfo.Name.Equals(strDirectoryKey, StringComparison.OrdinalIgnoreCase))
                                    continue;
                                if (objExistingInfo == null || objLoopInfo.MyVersion > objExistingInfo.MyVersion)
                                    objExistingInfo = objLoopInfo;
                            }
                            if (objExistingInfo != null)
                                strDirectoryKey = objExistingInfo.InternalId;
                        }
                        if (!_dicCustomDataDirectoryKeys.ContainsKey(strDirectoryKey))
                            _dicCustomDataDirectoryKeys.Add(strDirectoryKey, blnLoopEnabled);
                    }
                }
            }

            if (_dicCustomDataDirectoryKeys.Count == 0)
            {
                foreach (XPathNavigator objXmlDirectoryName in objXmlNode.SelectAndCacheExpression("customdatadirectorynames/directoryname"))
                {
                    string strDirectoryKey = objXmlDirectoryName.Value;
                    if (string.IsNullOrEmpty(strDirectoryKey))
                        continue;
                    string strLoopId = CustomDataDirectoryInfo.GetIdFromCharacterSettingsSaveKey(strDirectoryKey);
                    if (string.IsNullOrEmpty(strLoopId))
                    {
                        CustomDataDirectoryInfo objExistingInfo = null;
                        foreach (CustomDataDirectoryInfo objLoopInfo in GlobalSettings.CustomDataDirectoryInfos)
                        {
                            if (!objLoopInfo.Name.Equals(strDirectoryKey, StringComparison.OrdinalIgnoreCase))
                                continue;
                            if (objExistingInfo == null || objLoopInfo.MyVersion > objExistingInfo.MyVersion)
                                objExistingInfo = objLoopInfo;
                        }

                        if (objExistingInfo != null)
                            strDirectoryKey = objExistingInfo.InternalId;
                    }
                    if (!_dicCustomDataDirectoryKeys.ContainsKey(strDirectoryKey))
                        _dicCustomDataDirectoryKeys.Add(strDirectoryKey, true);
                }
            }

            // Add in any directories that are in GlobalSettings but are not present in the settings so that we may enable them if we want to
            foreach (string strCharacterSettingsSaveKey in GlobalSettings.CustomDataDirectoryInfos.Select(x => x.CharacterSettingsSaveKey))
            {
                if (!_dicCustomDataDirectoryKeys.ContainsKey(strCharacterSettingsSaveKey))
                {
                    _dicCustomDataDirectoryKeys.Add(strCharacterSettingsSaveKey, false);
                }
            }

            RecalculateEnabledCustomDataDirectories();

            foreach (XPathNavigator xmlBook in XmlManager.LoadXPath("books.xml", EnabledCustomDataDirectoryPaths)
                                                         .SelectAndCacheExpression(
                                                             "/chummer/books/book[permanent]/code"))
            {
                if (!string.IsNullOrEmpty(xmlBook.Value))
                    _lstBooks.Add(xmlBook.Value);
            }

            RecalculateBookXPath();

            // Used to legacy sweep build settings.
            XPathNavigator xmlDefaultBuildNode = objXmlNode.SelectSingleNodeAndCacheExpression("defaultbuild");
            if (objXmlNode.TryGetStringFieldQuickly("buildmethod", ref strTemp)
                && Enum.TryParse(strTemp, true, out CharacterBuildMethod eBuildMethod)
                || xmlDefaultBuildNode?.TryGetStringFieldQuickly("buildmethod", ref strTemp) == true
                && Enum.TryParse(strTemp, true, out eBuildMethod))
                _eBuildMethod = eBuildMethod;
            if (!objXmlNode.TryGetInt32FieldQuickly("buildpoints", ref _intBuildPoints))
                xmlDefaultBuildNode?.TryGetInt32FieldQuickly("buildpoints", ref _intBuildPoints);
            if (!objXmlNode.TryGetInt32FieldQuickly("qualitykarmalimit", ref _intQualityKarmaLimit) && BuildMethodUsesPriorityTables)
                _intQualityKarmaLimit = _intBuildPoints;
            objXmlNode.TryGetStringFieldQuickly("priorityarray", ref _strPriorityArray);
            objXmlNode.TryGetStringFieldQuickly("prioritytable", ref _strPriorityTable);
            objXmlNode.TryGetInt32FieldQuickly("sumtoten", ref _intSumtoTen);
            if (!objXmlNode.TryGetInt32FieldQuickly("availability", ref _intAvailability))
                xmlDefaultBuildNode?.TryGetInt32FieldQuickly("availability", ref _intAvailability);
            objXmlNode.TryGetDecFieldQuickly("nuyenmaxbp", ref _decNuyenMaximumBP);

            BannedWareGrades.Clear();
            foreach (XPathNavigator xmlGrade in objXmlNode.SelectAndCacheExpression("bannedwaregrades/grade"))
                BannedWareGrades.Add(xmlGrade.Value);

            RedlinerExcludes.Clear();
            foreach (XPathNavigator xmlLimb in objXmlNode.SelectAndCacheExpression("redlinerexclusion/limb"))
                RedlinerExcludes.Add(xmlLimb.Value);

            return true;
        }

        #endregion Initialization, Save, and Load Methods

        #region Build Properties

        /// <summary>
        /// Method being used to build the character.
        /// </summary>
        public CharacterBuildMethod BuildMethod
        {
            get => _eBuildMethod;
            set
            {
                if (value != _eBuildMethod)
                {
                    _eBuildMethod = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool BuildMethodUsesPriorityTables => BuildMethod.UsesPriorityTables();

        public bool BuildMethodIsPriority => BuildMethod == CharacterBuildMethod.Priority;

        public bool BuildMethodIsSumtoTen => BuildMethod == CharacterBuildMethod.SumtoTen;

        public bool BuildMethodIsLifeModule => BuildMethod == CharacterBuildMethod.LifeModule;

        /// <summary>
        /// The priority configuration used in Priority mode.
        /// </summary>
        public string PriorityArray
        {
            get => _strPriorityArray;
            set
            {
                if (_strPriorityArray != value)
                {
                    _strPriorityArray = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The priority table used in Priority or Sum-to-Ten mode.
        /// </summary>
        public string PriorityTable
        {
            get => _strPriorityTable;
            set
            {
                if (_strPriorityTable != value)
                {
                    _strPriorityTable = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The total value of priorities used in Sum-to-Ten mode.
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
            get => _intBuildPoints;
            set
            {
                if (_intBuildPoints != value)
                {
                    _intBuildPoints = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Limit on the amount of karma that can be spent at creation on qualities
        /// </summary>
        public int QualityKarmaLimit
        {
            get => _intQualityKarmaLimit;
            set
            {
                if (_intQualityKarmaLimit != value)
                {
                    _intQualityKarmaLimit = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Maximum item Availability for new characters.
        /// </summary>
        public int MaximumAvailability
        {
            get => _intAvailability;
            set
            {
                if (_intAvailability != value)
                {
                    _intAvailability = value;
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

        /// <summary>
        /// Blocked grades of cyber/bioware in Create mode.
        /// </summary>
        public HashSet<string> BannedWareGrades { get; } = new HashSet<string> { "Betaware", "Deltaware", "Gammaware" };

        /// <summary>
        /// Limb types excluded by redliner.
        /// </summary>
        public HashSet<string> RedlinerExcludes { get; } = new HashSet<string> { "skull", "torso" };

        public bool RedlinerExcludesSkull
        {
            get => RedlinerExcludes.Contains("skull");
            set
            {
                if (value)
                {
                    if (!RedlinerExcludes.Contains("skull"))
                    {
                        RedlinerExcludes.Add("skull");
                        OnPropertyChanged(nameof(RedlinerExcludes));
                    }
                }
                else
                {
                    if (RedlinerExcludes.Contains("skull"))
                    {
                        RedlinerExcludes.Remove("skull");
                        OnPropertyChanged(nameof(RedlinerExcludes));
                    }
                }
            }
        }

        public bool RedlinerExcludesTorso
        {
            get => RedlinerExcludes.Contains("torso");
            set
            {
                if (value)
                {
                    if (!RedlinerExcludes.Contains("torso"))
                    {
                        RedlinerExcludes.Add("torso");
                        OnPropertyChanged(nameof(RedlinerExcludes));
                    }
                }
                else
                {
                    if (RedlinerExcludes.Contains("torso"))
                    {
                        RedlinerExcludes.Remove("torso");
                        OnPropertyChanged(nameof(RedlinerExcludes));
                    }
                }
            }
        }

        public bool RedlinerExcludesArms
        {
            get => RedlinerExcludes.Contains("arm");
            set
            {
                if (value)
                {
                    if (!RedlinerExcludes.Contains("arm"))
                    {
                        RedlinerExcludes.Add("arm");
                        OnPropertyChanged(nameof(RedlinerExcludes));
                    }
                }
                else
                {
                    if (RedlinerExcludes.Contains("arm"))
                    {
                        RedlinerExcludes.Remove("arm");
                        OnPropertyChanged(nameof(RedlinerExcludes));
                    }
                }
            }
        }

        public bool RedlinerExcludesLegs
        {
            get => RedlinerExcludes.Contains("leg");
            set
            {
                if (value)
                {
                    if (!RedlinerExcludes.Contains("leg"))
                    {
                        RedlinerExcludes.Add("leg");
                        OnPropertyChanged(nameof(RedlinerExcludes));
                    }
                }
                else
                {
                    if (RedlinerExcludes.Contains("leg"))
                    {
                        RedlinerExcludes.Remove("leg");
                        OnPropertyChanged(nameof(RedlinerExcludes));
                    }
                }
            }
        }

        public string DictionaryKey => BuiltInOption ? SourceId : FileName;

        #endregion Build Properties

        #region Properties and Methods

        /// <summary>
        /// Load the Settings from the Registry (which will subsequently be converted to the XML Settings File format). Registry keys are deleted once they are read since they will no longer be used.
        /// </summary>
        public void LoadFromRegistry()
        {
            if (GlobalSettings.ChummerRegistryKey == null)
                return;

            // More Lethal Gameplay.
            GlobalSettings.LoadBoolFromRegistry(ref _blnMoreLethalGameplay, "morelethalgameplay", string.Empty, true);

            // Spirit Force Based on Total MAG.
            GlobalSettings.LoadBoolFromRegistry(ref _blnSpiritForceBasedOnTotalMAG, "spiritforcebasedontotalmag", string.Empty, true);

            // Skill Defaulting Includes modifiers.
            bool blnTemp = false;
            GlobalSettings.LoadBoolFromRegistry(ref blnTemp, "skilldefaultingincludesmodifiers", string.Empty, true);

            // Nuyen per Build Point
            GlobalSettings.LoadDecFromRegistry(ref _decNuyenPerBPWftM, "nuyenperbp", string.Empty, true);
            _decNuyenPerBPWftP = _decNuyenPerBPWftM;

            // No Single Armor Encumbrance
            GlobalSettings.LoadBoolFromRegistry(ref _blnNoSingleArmorEncumbrance, "nosinglearmorencumbrance", string.Empty, true);

            // Essence Loss Reduces Maximum Only.
            GlobalSettings.LoadBoolFromRegistry(ref _blnESSLossReducesMaximumOnly, "esslossreducesmaximumonly", string.Empty, true);

            // Allow Skill Regrouping.
            GlobalSettings.LoadBoolFromRegistry(ref _blnAllowSkillRegrouping, "allowskillregrouping", string.Empty, true);

            // Attempt to populate the Karma values.
            GlobalSettings.LoadInt32FromRegistry(ref _intKarmaAttribute, "karmaattribute", string.Empty, true);
            GlobalSettings.LoadInt32FromRegistry(ref _intKarmaQuality, "karmaquality", string.Empty, true);
            GlobalSettings.LoadInt32FromRegistry(ref _intKarmaSpecialization, "karmaspecialization", string.Empty, true);
            GlobalSettings.LoadInt32FromRegistry(ref _intKarmaKnoSpecialization, "karmaknospecialization", string.Empty, true);
            GlobalSettings.LoadInt32FromRegistry(ref _intKarmaNewKnowledgeSkill, "karmanewknowledgeskill", string.Empty, true);
            GlobalSettings.LoadInt32FromRegistry(ref _intKarmaNewActiveSkill, "karmanewactiveskill", string.Empty, true);
            GlobalSettings.LoadInt32FromRegistry(ref _intKarmaNewSkillGroup, "karmanewskillgroup", string.Empty, true);
            GlobalSettings.LoadInt32FromRegistry(ref _intKarmaImproveKnowledgeSkill, "karmaimproveknowledgeskill", string.Empty, true);
            GlobalSettings.LoadInt32FromRegistry(ref _intKarmaImproveActiveSkill, "karmaimproveactiveskill", string.Empty, true);
            GlobalSettings.LoadInt32FromRegistry(ref _intKarmaImproveSkillGroup, "karmaimproveskillgroup", string.Empty, true);
            GlobalSettings.LoadInt32FromRegistry(ref _intKarmaSpell, "karmaspell", string.Empty, true);
            GlobalSettings.LoadInt32FromRegistry(ref _intKarmaEnhancement, "karmaenhancement", string.Empty, true);
            GlobalSettings.LoadInt32FromRegistry(ref _intKarmaNewComplexForm, "karmanewcomplexform", string.Empty, true);
            GlobalSettings.LoadInt32FromRegistry(ref _intKarmaNewAIProgram, "karmanewaiprogram", string.Empty, true);
            GlobalSettings.LoadInt32FromRegistry(ref _intKarmaNewAIAdvancedProgram, "karmanewaiadvancedprogram", string.Empty, true);
            GlobalSettings.LoadInt32FromRegistry(ref _intKarmaContact, "karmacontact", string.Empty, true);
            GlobalSettings.LoadInt32FromRegistry(ref _intKarmaEnemy, "karmaenemy", string.Empty, true);
            GlobalSettings.LoadInt32FromRegistry(ref _intKarmaCarryover, "karmacarryover", string.Empty, true);
            GlobalSettings.LoadInt32FromRegistry(ref _intKarmaSpirit, "karmaspirit", string.Empty, true);
            GlobalSettings.LoadInt32FromRegistry(ref _intKarmaTechnique, "karmamaneuver", string.Empty, true);
            GlobalSettings.LoadInt32FromRegistry(ref _intKarmaInitiation, "karmainitiation", string.Empty, true);
            GlobalSettings.LoadInt32FromRegistry(ref _intKarmaInitiationFlat, "karmainitiationflat", string.Empty, true);
            GlobalSettings.LoadInt32FromRegistry(ref _intKarmaMetamagic, "karmametamagic", string.Empty, true);

            // Retrieve the sourcebooks that are in the Registry.
            string strBookList;
            object objBooksKeyValue = GlobalSettings.ChummerRegistryKey.GetValue("books");
            if (objBooksKeyValue != null)
            {
                strBookList = objBooksKeyValue.ToString();
            }
            else
            {
                // We were unable to get the Registry key which means the book options have not been saved yet, so create the default values.
                strBookList = "Shadowrun 5th Edition";
                GlobalSettings.ChummerRegistryKey.SetValue("books", strBookList);
            }
            string[] strBooks = strBookList.Split(',');

            XPathNavigator objXmlDocument = XmlManager.LoadXPath("books.xml", EnabledCustomDataDirectoryPaths);

            foreach (string strBookName in strBooks)
            {
                string strCode = objXmlDocument.SelectSingleNode("/chummer/books/book[name = " + strBookName.CleanXPath() + " and not(hide)]/code")?.Value;
                if (!string.IsNullOrEmpty(strCode))
                {
                    _lstBooks.Add(strCode);
                }
            }
            RecalculateBookXPath();

            // Delete the Registry keys ones the values have been retrieve since they will no longer be used.
            GlobalSettings.ChummerRegistryKey.DeleteValue("books");
        }

        /// <summary>
        /// Determine whether or not a given book is in use.
        /// </summary>
        /// <param name="strCode">Book code to search for.</param>
        public bool BookEnabled(string strCode)
        {
            return _lstBooks.Contains(strCode);
        }

        /// <summary>
        /// XPath query used to filter items based on the user's selected source books and optional rules.
        /// </summary>
        public string BookXPath(bool excludeHidden = true)
        {
            StringBuilder sbdPath = excludeHidden ? new StringBuilder("not(hide)") : new StringBuilder();
            
            if (string.IsNullOrWhiteSpace(_strBookXPath) && _lstBooks.Count > 0)
            {
                RecalculateBookXPath();
            }
            if (!string.IsNullOrEmpty(_strBookXPath))
            {
                if (sbdPath.Length != 0)
                    sbdPath.Append(" and ");
                sbdPath.Append(_strBookXPath);
            }
            else
            {
                // Should not ever have a situation where BookXPath remains empty after recalculation, but it's here just in case
                Utils.BreakIfDebug();
            }
            if (!DroneMods)
            {
                if (sbdPath.Length != 0)
                    sbdPath.Append(" and ");
                sbdPath.Append("not(optionaldrone)");
            }

            if (sbdPath.Length > 1)
                return '(' + sbdPath.ToString() + ')';

            // We have only the opening parentheses; return an empty string
            // The above should not happen, so break if we're debugging, as there's something weird going on with the character's setup
            Utils.BreakIfDebug();
            return string.Empty;
        }

        /// <summary>
        /// XPath query used to filter items based on the user's selected source books.
        /// </summary>
        public void RecalculateBookXPath()
        {
            StringBuilder sbdBookXPath = new StringBuilder("(");
            _strBookXPath = string.Empty;

            foreach (string strBook in _lstBooks)
            {
                if (!string.IsNullOrWhiteSpace(strBook))
                {
                    sbdBookXPath.Append("source = ").Append(strBook.CleanXPath()).Append(" or ");
                }
            }
            if (sbdBookXPath.Length >= 4)
            {
                sbdBookXPath.Length -= 4;
                sbdBookXPath.Append(')');
                _strBookXPath = sbdBookXPath.ToString();
            }
            else
                _strBookXPath = string.Empty;
        }

        public TypedOrderedDictionary<string, bool> CustomDataDirectoryKeys => _dicCustomDataDirectoryKeys;

        public IReadOnlyList<string> EnabledCustomDataDirectoryPaths => _lstEnabledCustomDataDirectoryPaths;

        public IReadOnlyList<CustomDataDirectoryInfo> EnabledCustomDataDirectoryInfos => _setEnabledCustomDataDirectories;

        /// <summary>
        /// A HashSet that can be used for fast queries, which content is (and should) always identical to the IReadOnlyList EnabledCustomDataDirectoryInfos
        /// </summary>
        public IReadOnlyCollection<Guid> EnabledCustomDataDirectoryInfoGuids => _setEnabledCustomDataDirectoryGuids;

        public void RecalculateEnabledCustomDataDirectories()
        {
            _setEnabledCustomDataDirectoryGuids.Clear();
            _setEnabledCustomDataDirectories.Clear();
            _lstEnabledCustomDataDirectoryPaths.Clear();
            foreach (KeyValuePair<string, bool> kvpCustomDataDirectoryName in _dicCustomDataDirectoryKeys)
            {
                if (!kvpCustomDataDirectoryName.Value)
                    continue;
                string strKey = kvpCustomDataDirectoryName.Key;
                string strId = CustomDataDirectoryInfo.GetIdFromCharacterSettingsSaveKey(strKey, out Version objPreferredVersion);
                CustomDataDirectoryInfo objInfoToAdd = null;
                if (string.IsNullOrEmpty(strId))
                {
                    foreach (CustomDataDirectoryInfo objLoopInfo in GlobalSettings.CustomDataDirectoryInfos)
                    {
                        if (!objLoopInfo.Name.Equals(strKey, StringComparison.OrdinalIgnoreCase))
                            continue;
                        if (objInfoToAdd == null || objLoopInfo.MyVersion > objInfoToAdd.MyVersion)
                            objInfoToAdd = objLoopInfo;
                    }
                }
                else
                {
                    foreach (CustomDataDirectoryInfo objLoopInfo in GlobalSettings.CustomDataDirectoryInfos)
                    {
                        if (!objLoopInfo.InternalId.Equals(strId, StringComparison.OrdinalIgnoreCase))
                            continue;
                        if (objInfoToAdd == null || VersionMatchScore(objLoopInfo.MyVersion) > VersionMatchScore(objInfoToAdd.MyVersion))
                            objInfoToAdd = objLoopInfo;
                    }

                    int VersionMatchScore(Version objVersion)
                    {
                        int intReturn = int.MaxValue;
                        intReturn -= (objPreferredVersion.Build - objVersion.Build).RaiseToPower(2) * 2.RaiseToPower(24);
                        intReturn -= (objPreferredVersion.Major - objVersion.Major).RaiseToPower(2) * 2.RaiseToPower(16);
                        intReturn -= (objPreferredVersion.Minor - objVersion.Minor).RaiseToPower(2) * 2.RaiseToPower(8);
                        intReturn -= (objPreferredVersion.Revision - objVersion.Revision).RaiseToPower(2);
                        return intReturn;
                    }
                }
                if (objInfoToAdd != null)
                {
                    if (!_setEnabledCustomDataDirectoryGuids.Contains(objInfoToAdd.Guid))
                        _setEnabledCustomDataDirectoryGuids.Add(objInfoToAdd.Guid);
                    _setEnabledCustomDataDirectories.Add(objInfoToAdd);
                    _lstEnabledCustomDataDirectoryPaths.Add(objInfoToAdd.DirectoryPath);
                }
                else
                    Utils.BreakIfDebug();
            }
        }

        public string SourceId => _guiSourceId.ToString("D", GlobalSettings.InvariantCultureInfo);

        public bool BuiltInOption => _guiSourceId != Guid.Empty;

        /// <summary>
        /// Whether or not the More Lethal Gameplay optional rule is enabled.
        /// </summary>
        public bool MoreLethalGameplay
        {
            get => _blnMoreLethalGameplay;
            set
            {
                if (_blnMoreLethalGameplay != value)
                {
                    _blnMoreLethalGameplay = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether or not to require licensing restricted items.
        /// </summary>
        public bool LicenseRestricted
        {
            get => _blnLicenseRestrictedItems;
            set
            {
                if (_blnLicenseRestrictedItems != value)
                {
                    _blnLicenseRestrictedItems = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether or not a Spirit's Maximum Force is based on the character's total MAG.
        /// </summary>
        public bool SpiritForceBasedOnTotalMAG
        {
            get => _blnSpiritForceBasedOnTotalMAG;
            set
            {
                if (_blnSpiritForceBasedOnTotalMAG != value)
                {
                    _blnSpiritForceBasedOnTotalMAG = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Amount of Nuyen gained per BP spent when Working for the Man.
        /// </summary>
        public decimal NuyenPerBPWftM
        {
            get => _decNuyenPerBPWftM;
            set
            {
                if (_decNuyenPerBPWftM != value)
                {
                    _decNuyenPerBPWftM = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Amount of Nuyen spent per BP gained when Working for the People.
        /// </summary>
        public decimal NuyenPerBPWftP
        {
            get => _decNuyenPerBPWftP;
            set
            {
                if (_decNuyenPerBPWftP != value)
                {
                    _decNuyenPerBPWftP = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether or not UnarmedAP, UnarmedReach and UnarmedDV Improvements apply to weapons that use the Unarmed Combat skill.
        /// </summary>
        public bool UnarmedImprovementsApplyToWeapons
        {
            get => _blnUnarmedImprovementsApplyToWeapons;
            set
            {
                if (_blnUnarmedImprovementsApplyToWeapons != value)
                {
                    _blnUnarmedImprovementsApplyToWeapons = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether or not characters may use Initiation/Submersion in Create mode.
        /// </summary>
        public bool AllowInitiationInCreateMode
        {
            get => _blnAllowInitiationInCreateMode;
            set
            {
                if (_blnAllowInitiationInCreateMode != value)
                {
                    _blnAllowInitiationInCreateMode = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether or not characters can spend skill points on broken groups.
        /// </summary>
        public bool UsePointsOnBrokenGroups
        {
            get => _blnUsePointsOnBrokenGroups;
            set
            {
                if (_blnUsePointsOnBrokenGroups != value)
                {
                    _blnUsePointsOnBrokenGroups = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether or not characters in Career Mode should pay double for qualities.
        /// </summary>
        public bool DontDoubleQualityPurchases
        {
            get => _blnDontDoubleQualityPurchaseCost;
            set
            {
                if (_blnDontDoubleQualityPurchaseCost != value)
                {
                    _blnDontDoubleQualityPurchaseCost = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether or not characters in Career Mode should pay double for removing Negative Qualities.
        /// </summary>
        public bool DontDoubleQualityRefunds
        {
            get => _blnDontDoubleQualityRefundCost;
            set
            {
                if (_blnDontDoubleQualityRefundCost != value)
                {
                    _blnDontDoubleQualityRefundCost = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether or not to ignore the art requirements from street grimoire.
        /// </summary>
        public bool IgnoreArt
        {
            get => _blnIgnoreArt;
            set
            {
                if (_blnIgnoreArt != value)
                {
                    _blnIgnoreArt = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether or not to ignore the limit on Complex Forms in Career mode.
        /// </summary>
        public bool IgnoreComplexFormLimit
        {
            get => _blnIgnoreComplexFormLimit;
            set
            {
                if (_blnIgnoreComplexFormLimit != value)
                {
                    _blnIgnoreComplexFormLimit = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether or not to use stats from Cyberlegs when calculating movement rates
        /// </summary>
        public bool CyberlegMovement
        {
            get => _blnCyberlegMovement;
            set
            {
                if (_blnCyberlegMovement != value)
                {
                    _blnCyberlegMovement = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Allow Mystic Adepts to increase their power points during career mode
        /// </summary>
        public bool MysAdeptAllowPpCareer
        {
            get => _blnMysAdeptAllowPpCareer;
            set
            {
                if (_blnMysAdeptAllowPpCareer != value)
                {
                    _blnMysAdeptAllowPpCareer = value;
                    OnPropertyChanged();
                    if (value)
                        MysAdeptSecondMAGAttribute = false;
                }
            }
        }

        /// <summary>
        /// Split MAG for Mystic Adepts so that they have a separate MAG rating for Adept Powers instead of using the special PP rules for mystic adepts
        /// </summary>
        public bool MysAdeptSecondMAGAttribute
        {
            get => _blnMysAdeptSecondMAGAttribute;
            set
            {
                if (_blnMysAdeptSecondMAGAttribute != value)
                {
                    _blnMysAdeptSecondMAGAttribute = value;
                    OnPropertyChanged();
                    if (value)
                    {
                        PrioritySpellsAsAdeptPowers = false;
                        MysAdeptAllowPpCareer = false;
                    }
                }
            }
        }

        public bool MysAdeptSecondMAGAttributeEnabled => !PrioritySpellsAsAdeptPowers && !MysAdeptAllowPpCareer;

        /// <summary>
        /// Whether or not to allow a 2nd max attribute with Exceptional Attribute
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public bool Allow2ndMaxAttribute
        {
            get => _blnAllow2ndMaxAttribute;
            set
            {
                if (_blnAllow2ndMaxAttribute != value)
                {
                    _blnAllow2ndMaxAttribute = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The XPath expression to use to determine how many contact points the character has
        /// </summary>
        public string ContactPointsExpression
        {
            get => _strContactPointsExpression;
            set
            {
                string strNewValue = value.CleanXPath().Trim('\"');
                if (_strContactPointsExpression != strNewValue)
                {
                    _strContactPointsExpression = strNewValue;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The XPath expression to use to determine how many knowledge points the character has
        /// </summary>
        public string KnowledgePointsExpression
        {
            get => _strKnowledgePointsExpression;
            set
            {
                string strNewValue = value.CleanXPath().Trim('\"');
                if (_strKnowledgePointsExpression != strNewValue)
                {
                    _strKnowledgePointsExpression = strNewValue;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The XPath expression to use to determine how much nuyen the character gets at character creation
        /// </summary>
        public string ChargenKarmaToNuyenExpression
        {
            get => _strChargenKarmaToNuyenExpression;
            set
            {
                string strNewValue = value.CleanXPath().Trim('\"');
                if (_strChargenKarmaToNuyenExpression != strNewValue)
                {
                    _strChargenKarmaToNuyenExpression = strNewValue;
                    // A safety check to make sure that we always still account for Priority-given Nuyen
                    if (SettingsManager.LoadedCharacterSettings.ContainsKey(DictionaryKey) && !_strChargenKarmaToNuyenExpression.Contains("{PriorityNuyen}"))
                    {
                        _strChargenKarmaToNuyenExpression = '(' + _strChargenKarmaToNuyenExpression + ") + {PriorityNuyen}";
                    }
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The XPath expression to use to determine how many spirits a character can bind
        /// </summary>
        public string BoundSpiritExpression
        {
            get => _strBoundSpiritExpression;
            set
            {
                string strNewValue = value.CleanXPath().Trim('\"');
                if (_strBoundSpiritExpression != strNewValue)
                {
                    _strBoundSpiritExpression = strNewValue;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The XPath expression to use to determine how many sprites a character can bind
        /// </summary>
        public string RegisteredSpriteExpression
        {
            get => _strRegisteredSpriteExpression;
            set
            {
                string strNewValue = value.CleanXPath().Trim('\"');
                if (_strRegisteredSpriteExpression != strNewValue)
                {
                    _strRegisteredSpriteExpression = strNewValue;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The Drone Body multiplier for maximal Armor
        /// </summary>
        public int DroneArmorMultiplier
        {
            get => _intDroneArmorMultiplier;
            set
            {
                if (_intDroneArmorMultiplier != value)
                {
                    _intDroneArmorMultiplier = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether or not Armor
        /// </summary>
        public bool DroneArmorMultiplierEnabled
        {
            get => _blnDroneArmorMultiplierEnabled;
            set
            {
                if (_blnDroneArmorMultiplierEnabled != value)
                {
                    _blnDroneArmorMultiplierEnabled = value;
                    OnPropertyChanged();
                    if (!value)
                        DroneArmorMultiplier = 2;
                }
            }
        }

        /// <summary>
        /// House Rule: Ignore Armor Encumbrance entirely.
        /// </summary>
        public bool NoArmorEncumbrance
        {
            get => _blnNoArmorEncumbrance;
            set
            {
                if (_blnNoArmorEncumbrance != value)
                {
                    _blnNoArmorEncumbrance = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether or not Essence loss only reduces MAG/RES maximum value, not the current value.
        /// </summary>
        public bool ESSLossReducesMaximumOnly
        {
            get => _blnESSLossReducesMaximumOnly;
            set
            {
                if (_blnESSLossReducesMaximumOnly != value)
                {
                    _blnESSLossReducesMaximumOnly = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether or not characters are allowed to put points into a Skill Group again once it is broken and all Ratings are the same.
        /// </summary>
        public bool AllowSkillRegrouping
        {
            get => _blnAllowSkillRegrouping;
            set
            {
                if (_blnAllowSkillRegrouping != value)
                {
                    _blnAllowSkillRegrouping = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Sourcebooks.
        /// </summary>
        public HashSet<string> BooksWritable => _lstBooks;

        /// <summary>
        /// Sourcebooks.
        /// </summary>
        public IReadOnlyCollection<string> Books => _lstBooks;

        /// <summary>
        /// File name of the option (if it is not a built-in one).
        /// </summary>
        public string FileName => _strFileName;

        /// <summary>
        /// Setting name.
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
        /// Setting name to display in the UI.
        /// </summary>
        public string DisplayName
        {
            get
            {
                string strReturn = Name;
                if (BuiltInOption)
                {
                    strReturn = XmlManager.LoadXPath("settings.xml").SelectSingleNode("/chummer/settings/setting[id = '" + SourceId + "']/translate")?.Value ?? strReturn;
                }
                else
                {
                    strReturn += LanguageManager.GetString("String_Space") + '[' + FileName + ']';
                }
                return strReturn;
            }
        }

        /// <summary>
        /// Whether or not Metatypes cost Karma equal to their BP when creating a character with Karma.
        /// </summary>
        public bool MetatypeCostsKarma
        {
            get => _blnMetatypeCostsKarma;
            set
            {
                if (_blnMetatypeCostsKarma != value)
                {
                    _blnMetatypeCostsKarma = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Multiplier for Metatype Karma Costs when converting from BP to Karma.
        /// </summary>
        public int MetatypeCostsKarmaMultiplier
        {
            get => _intMetatypeCostMultiplier;
            set
            {
                if (_intMetatypeCostMultiplier != value)
                {
                    _intMetatypeCostMultiplier = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Number of Limbs a standard character has.
        /// </summary>
        public int LimbCount
        {
            get => _intLimbCount;
            set
            {
                if (_intLimbCount != value)
                {
                    _intLimbCount = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Exclude a particular Limb Slot from count towards the Limb Count.
        /// </summary>
        public string ExcludeLimbSlot
        {
            get => _strExcludeLimbSlot;
            set
            {
                if (_strExcludeLimbSlot != value)
                {
                    _strExcludeLimbSlot = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Allow Cyberware Essence cost discounts.
        /// </summary>
        public bool AllowCyberwareESSDiscounts
        {
            get => _blnAllowCyberwareESSDiscounts;
            set
            {
                if (_blnAllowCyberwareESSDiscounts != value)
                {
                    _blnAllowCyberwareESSDiscounts = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether or not Armor Degradation is allowed.
        /// </summary>
        public bool ArmorDegradation
        {
            get => _blnArmorDegradation;
            set
            {
                if (_blnArmorDegradation != value)
                {
                    _blnArmorDegradation = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// If true, karma costs will not decrease from reductions due to essence loss. Effectively, essence loss becomes an augmented modifier, not one that alters minima and maxima.
        /// </summary>
        public bool SpecialKarmaCostBasedOnShownValue
        {
            get => _blnSpecialKarmaCostBasedOnShownValue;
            set
            {
                if (_blnSpecialKarmaCostBasedOnShownValue != value)
                {
                    _blnSpecialKarmaCostBasedOnShownValue = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether or not characters can have more than 25 BP in Positive Qualities.
        /// </summary>
        public bool ExceedPositiveQualities
        {
            get => _blnExceedPositiveQualities;
            set
            {
                if (_blnExceedPositiveQualities != value)
                {
                    _blnExceedPositiveQualities = value;
                    OnPropertyChanged();
                    if (!value)
                        ExceedPositiveQualitiesCostDoubled = false;
                }
            }
        }

        /// <summary>
        /// If true, the karma cost of qualities is doubled after the initial 25.
        /// </summary>
        public bool ExceedPositiveQualitiesCostDoubled
        {
            get => _blnExceedPositiveQualitiesCostDoubled;
            set
            {
                if (_blnExceedPositiveQualitiesCostDoubled != value)
                {
                    _blnExceedPositiveQualitiesCostDoubled = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether or not characters can have more than 25 BP in Negative Qualities.
        /// </summary>
        public bool ExceedNegativeQualities
        {
            get => _blnExceedNegativeQualities;
            set
            {
                if (_blnExceedNegativeQualities != value)
                {
                    _blnExceedNegativeQualities = value;
                    OnPropertyChanged();
                    if (!value)
                        ExceedNegativeQualitiesLimit = false;
                }
            }
        }

        /// <summary>
        /// If true, the character will not receive additional BP from Negative Qualities past the initial 25
        /// </summary>
        public bool ExceedNegativeQualitiesLimit
        {
            get => _blnExceedNegativeQualitiesLimit;
            set
            {
                if (_blnExceedNegativeQualitiesLimit != value)
                {
                    _blnExceedNegativeQualitiesLimit = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether or not Restricted items have their cost multiplied.
        /// </summary>
        public bool MultiplyRestrictedCost
        {
            get => _blnMultiplyRestrictedCost;
            set
            {
                if (_blnMultiplyRestrictedCost != value)
                {
                    _blnMultiplyRestrictedCost = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether or not Forbidden items have their cost multiplied.
        /// </summary>
        public bool MultiplyForbiddenCost
        {
            get => _blnMultiplyForbiddenCost;
            set
            {
                if (_blnMultiplyForbiddenCost != value)
                {
                    _blnMultiplyForbiddenCost = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Cost multiplier for Restricted items.
        /// </summary>
        public int RestrictedCostMultiplier
        {
            get => _intRestrictedCostMultiplier;
            set
            {
                if (_intRestrictedCostMultiplier != value)
                {
                    _intRestrictedCostMultiplier = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Cost multiplier for Forbidden items.
        /// </summary>
        public int ForbiddenCostMultiplier
        {
            get => _intForbiddenCostMultiplier;
            set
            {
                if (_intForbiddenCostMultiplier != value)
                {
                    _intForbiddenCostMultiplier = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _intCachedMaxNuyenDecimals = -1;

        /// <summary>
        /// Maximum number of decimal places to round to when displaying nuyen values.
        /// </summary>
        public int MaxNuyenDecimals
        {
            get
            {
                if (_intCachedMaxNuyenDecimals >= 0)
                    return _intCachedMaxNuyenDecimals;
                string strNuyenFormat = NuyenFormat;
                int intDecimalPlaces = strNuyenFormat.IndexOf('.');
                if (intDecimalPlaces == -1)
                    intDecimalPlaces = 0;
                else
                    intDecimalPlaces = strNuyenFormat.Length - intDecimalPlaces - 1;

                return _intCachedMaxNuyenDecimals = intDecimalPlaces;
            }
            set
            {
                int intNewNuyenDecimals = Math.Max(value, 0);
                if (MinNuyenDecimals > intNewNuyenDecimals)
                    MinNuyenDecimals = intNewNuyenDecimals;
                if (intNewNuyenDecimals == 0)
                    return; // Already taken care of by MinNuyenDecimals
                int intCurrentNuyenDecimals = MaxNuyenDecimals;
                if (intNewNuyenDecimals < intCurrentNuyenDecimals)
                {
                    NuyenFormat = NuyenFormat.Substring(0, NuyenFormat.Length - (intNewNuyenDecimals - intCurrentNuyenDecimals));
                }
                else if (intNewNuyenDecimals > intCurrentNuyenDecimals)
                {
                    StringBuilder objNuyenFormat = string.IsNullOrEmpty(NuyenFormat) ? new StringBuilder("#,0") : new StringBuilder(NuyenFormat);
                    if (intCurrentNuyenDecimals == 0)
                    {
                        objNuyenFormat.Append('.');
                    }
                    for (int i = intCurrentNuyenDecimals; i < intNewNuyenDecimals; ++i)
                    {
                        objNuyenFormat.Append('#');
                    }
                    NuyenFormat = objNuyenFormat.ToString();
                }
            }
        }

        private int _intCachedMinNuyenDecimals = -1;

        /// <summary>
        /// Minimum number of decimal places to round to when displaying nuyen values.
        /// </summary>
        public int MinNuyenDecimals
        {
            get
            {
                if (_intCachedMinNuyenDecimals >= 0)
                    return _intCachedMinNuyenDecimals;
                string strNuyenFormat = NuyenFormat;
                int intDecimalPlaces = strNuyenFormat.IndexOf('.');
                if (intDecimalPlaces == -1)
                    intDecimalPlaces = 0;
                else
                {
                    int intStartOptionalDecimalPlaces = strNuyenFormat.IndexOf('#', intDecimalPlaces);
                    if (intStartOptionalDecimalPlaces < 0)
                        intStartOptionalDecimalPlaces = strNuyenFormat.Length;
                    intDecimalPlaces = intStartOptionalDecimalPlaces - intDecimalPlaces - 1;
                }

                return _intCachedMinNuyenDecimals = intDecimalPlaces;
            }
            set
            {
                int intNewNuyenDecimals = Math.Max(value, 0);
                if (MaxNuyenDecimals < intNewNuyenDecimals)
                    MaxNuyenDecimals = intNewNuyenDecimals;
                int intDecimalPlaces = NuyenFormat.IndexOf('.');
                if (intNewNuyenDecimals == 0)
                {
                    if (intDecimalPlaces != -1)
                        NuyenFormat = NuyenFormat.Substring(0, intDecimalPlaces);
                    return;
                }
                int intCurrentNuyenDecimals = MinNuyenDecimals;
                if (intNewNuyenDecimals < intCurrentNuyenDecimals)
                {
                    char[] achrNuyenFormat = NuyenFormat.ToCharArray();
                    for (int i = intDecimalPlaces + 1 + intNewNuyenDecimals; i < achrNuyenFormat.Length; ++i)
                        achrNuyenFormat[i] = '0';
                    NuyenFormat = new string(achrNuyenFormat);
                }
                else if (intNewNuyenDecimals > intCurrentNuyenDecimals)
                {
                    char[] achrNuyenFormat = NuyenFormat.ToCharArray();
                    for (int i = 1; i < intNewNuyenDecimals; ++i)
                        achrNuyenFormat[intDecimalPlaces + i] = '0';
                    NuyenFormat = new string(achrNuyenFormat);
                }
            }
        }

        /// <summary>
        /// Format in which nuyen values should be displayed (does not include nuyen symbol).
        /// </summary>
        public string NuyenFormat
        {
            get => _strNuyenFormat;
            set
            {
                if (_strNuyenFormat != value)
                {
                    _strNuyenFormat = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _intCachedEssenceDecimals = -1;

        /// <summary>
        /// Number of decimal places to round to when calculating Essence.
        /// </summary>
        public int EssenceDecimals
        {
            get
            {
                if (_intCachedEssenceDecimals >= 0)
                    return _intCachedEssenceDecimals;
                string strEssenceFormat = EssenceFormat;
                int intDecimalPlaces = strEssenceFormat.IndexOf('.');
                intDecimalPlaces = strEssenceFormat.Length - intDecimalPlaces - 1;

                return _intCachedEssenceDecimals = intDecimalPlaces;
            }
            set
            {
                int intCurrentEssenceDecimals = EssenceDecimals;
                int intNewEssenceDecimals = Math.Max(value, 2);
                if (intNewEssenceDecimals < intCurrentEssenceDecimals)
                {
                    EssenceFormat = EssenceFormat.Substring(0, EssenceFormat.Length - (intCurrentEssenceDecimals - intNewEssenceDecimals));
                }
                else if (intNewEssenceDecimals > intCurrentEssenceDecimals)
                {
                    StringBuilder sbdEssenceFormat = string.IsNullOrEmpty(EssenceFormat) ? new StringBuilder("#,0") : new StringBuilder(EssenceFormat);
                    if (intCurrentEssenceDecimals == 0)
                    {
                        sbdEssenceFormat.Append('.');
                    }
                    for (int i = intCurrentEssenceDecimals; i < intNewEssenceDecimals; ++i)
                    {
                        sbdEssenceFormat.Append('0');
                    }
                    EssenceFormat = sbdEssenceFormat.ToString();
                }
            }
        }

        /// <summary>
        /// Display format for Essence.
        /// </summary>
        public string EssenceFormat
        {
            get => _strEssenceFormat;
            set
            {
                string strNewValue = value;
                int intDecimalPlaces = strNewValue.IndexOf('.');
                if (intDecimalPlaces < 2)
                {
                    if (intDecimalPlaces == -1)
                        strNewValue += ".00";
                    else
                    {
                        StringBuilder sbdZeros = new StringBuilder(strNewValue);
                        for (int i = strNewValue.Length - 1 - intDecimalPlaces; i < intDecimalPlaces; ++i)
                            sbdZeros.Append('0');
                        strNewValue = sbdZeros.ToString();
                    }
                }
                if (_strEssenceFormat != strNewValue)
                {
                    _strEssenceFormat = strNewValue;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Only round essence when its value is displayed
        /// </summary>
        public bool DontRoundEssenceInternally
        {
            get => _blnDoNotRoundEssenceInternally;
            set
            {
                if (_blnDoNotRoundEssenceInternally != value)
                {
                    _blnDoNotRoundEssenceInternally = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Allow Enemies to be bought and tracked like in 4e?
        /// </summary>
        public bool EnableEnemyTracking
        {
            get => _blnEnableEnemyTracking;
            set
            {
                if (_blnEnableEnemyTracking != value)
                {
                    _blnEnableEnemyTracking = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Do Enemies count towards Negative Quality Karma limit in create mode?
        /// </summary>
        public bool EnemyKarmaQualityLimit
        {
            get => _blnEnemyKarmaQualityLimit;
            set
            {
                if (_blnEnemyKarmaQualityLimit != value)
                {
                    _blnEnemyKarmaQualityLimit = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether or not Capacity limits should be enforced.
        /// </summary>
        public bool EnforceCapacity
        {
            get => _blnEnforceCapacity;
            set
            {
                if (_blnEnforceCapacity != value)
                {
                    _blnEnforceCapacity = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether or not Recoil modifiers are restricted (AR 148).
        /// </summary>
        public bool RestrictRecoil
        {
            get => _blnRestrictRecoil;
            set
            {
                if (_blnRestrictRecoil != value)
                {
                    _blnRestrictRecoil = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether or not characters are unrestricted in the number of points they can invest in Nuyen.
        /// </summary>
        public bool UnrestrictedNuyen
        {
            get => _blnUnrestrictedNuyen;
            set
            {
                if (_blnUnrestrictedNuyen != value)
                {
                    _blnUnrestrictedNuyen = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether or not Stacked Foci can have a combined Force higher than 6.
        /// </summary>
        public bool AllowHigherStackedFoci
        {
            get => _blnAllowHigherStackedFoci;
            set
            {
                if (_blnAllowHigherStackedFoci != value)
                {
                    _blnAllowHigherStackedFoci = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether or not the user can change the Part of Base Weapon flag for a Weapon Accessory or Mod.
        /// </summary>
        public bool AllowEditPartOfBaseWeapon
        {
            get => _blnAllowEditPartOfBaseWeapon;
            set
            {
                if (_blnAllowEditPartOfBaseWeapon != value)
                {
                    _blnAllowEditPartOfBaseWeapon = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether or not the user is allowed to break Skill Groups while in Create Mode.
        /// </summary>
        public bool StrictSkillGroupsInCreateMode
        {
            get => _blnStrictSkillGroupsInCreateMode;
            set
            {
                if (_blnStrictSkillGroupsInCreateMode != value)
                {
                    _blnStrictSkillGroupsInCreateMode = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether or not the user is allowed to buy specializations with skill points for skills only bought with karma.
        /// </summary>
        public bool AllowPointBuySpecializationsOnKarmaSkills
        {
            get => _blnAllowPointBuySpecializationsOnKarmaSkills;
            set
            {
                if (_blnAllowPointBuySpecializationsOnKarmaSkills != value)
                {
                    _blnAllowPointBuySpecializationsOnKarmaSkills = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether or not any Detection Spell can be taken as Extended range version.
        /// </summary>
        public bool ExtendAnyDetectionSpell
        {
            get => _blnExtendAnyDetectionSpell;
            set
            {
                if (_blnExtendAnyDetectionSpell != value)
                {
                    _blnExtendAnyDetectionSpell = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether or not cyberlimbs stats are used in attribute calculation
        /// </summary>
        public bool DontUseCyberlimbCalculation
        {
            get => _blnDontUseCyberlimbCalculation;
            set
            {
                if (_blnDontUseCyberlimbCalculation != value)
                {
                    _blnDontUseCyberlimbCalculation = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// House rule: Treat the Metatype Attribute Minimum as 1 for the purpose of calculating Karma costs.
        /// </summary>
        public bool AlternateMetatypeAttributeKarma
        {
            get => _blnAlternateMetatypeAttributeKarma;
            set
            {
                if (_blnAlternateMetatypeAttributeKarma != value)
                {
                    _blnAlternateMetatypeAttributeKarma = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// House rule: Whether to compensate for the karma cost difference between raising skill ratings and skill groups when increasing the rating of the last skill in the group
        /// </summary>
        public bool CompensateSkillGroupKarmaDifference
        {
            get => _blnCompensateSkillGroupKarmaDifference;
            set
            {
                if (_blnCompensateSkillGroupKarmaDifference != value)
                {
                    _blnCompensateSkillGroupKarmaDifference = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether or not Obsolescent can be removed/upgraded in the same way as Obsolete.
        /// </summary>
        public bool AllowObsolescentUpgrade
        {
            get => _blnAllowObsolescentUpgrade;
            set
            {
                if (_blnAllowObsolescentUpgrade != value)
                {
                    _blnAllowObsolescentUpgrade = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether or not Bioware Suites can be added and created.
        /// </summary>
        public bool AllowBiowareSuites
        {
            get => _blnAllowBiowareSuites;
            set
            {
                if (_blnAllowBiowareSuites != value)
                {
                    _blnAllowBiowareSuites = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// House rule: Free Spirits calculate their Power Points based on their MAG instead of EDG.
        /// </summary>
        public bool FreeSpiritPowerPointsMAG
        {
            get => _blnFreeSpiritPowerPointsMAG;
            set
            {
                if (_blnFreeSpiritPowerPointsMAG != value)
                {
                    _blnFreeSpiritPowerPointsMAG = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// House rule: Attribute values are clamped to 0 or are allowed to go below 0 due to Essence Loss.
        /// </summary>
        public bool UnclampAttributeMinimum
        {
            get => _blnUnclampAttributeMinimum;
            set
            {
                if (_blnUnclampAttributeMinimum != value)
                {
                    _blnUnclampAttributeMinimum = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Use Rigger 5.0 drone modding rules
        /// </summary>
        public bool DroneMods
        {
            get => _blnDroneMods;
            set
            {
                if (_blnDroneMods != value)
                {
                    _blnDroneMods = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Apply drone mod attribute maximum rule to Pilot, too
        /// </summary>
        public bool DroneModsMaximumPilot
        {
            get => _blnDroneModsMaximumPilot;
            set
            {
                if (_blnDroneModsMaximumPilot != value)
                {
                    _blnDroneModsMaximumPilot = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether Life Modules should automatically generate a character background.
        /// </summary>
        public bool AutomaticBackstory
        {
            get => _blnAutomaticBackstory;
            set
            {
                if (_blnAutomaticBackstory != value)
                {
                    _blnAutomaticBackstory = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether to use the rules from SR4 to calculate Public Awareness.
        /// </summary>
        public bool UseCalculatedPublicAwareness
        {
            get => _blnUseCalculatedPublicAwareness;
            set
            {
                if (_blnUseCalculatedPublicAwareness != value)
                {
                    _blnUseCalculatedPublicAwareness = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether Martial Arts grant a free specialization in a skill.
        /// </summary>
        public bool FreeMartialArtSpecialization
        {
            get => _blnFreeMartialArtSpecialization;
            set
            {
                if (_blnFreeMartialArtSpecialization != value)
                {
                    _blnFreeMartialArtSpecialization = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether Spells from Magic Priority can also be spent on power points.
        /// </summary>
        public bool PrioritySpellsAsAdeptPowers
        {
            get => _blnPrioritySpellsAsAdeptPowers;
            set
            {
                if (_blnPrioritySpellsAsAdeptPowers != value)
                {
                    _blnPrioritySpellsAsAdeptPowers = value;
                    OnPropertyChanged();
                    if (value)
                        MysAdeptSecondMAGAttribute = false;
                }
            }
        }

        /// <summary>
        /// Allows characters to spend their Karma before Priority Points.
        /// </summary>
        public bool ReverseAttributePriorityOrder
        {
            get => _blnReverseAttributePriorityOrder;
            set
            {
                if (_blnReverseAttributePriorityOrder != value)
                {
                    _blnReverseAttributePriorityOrder = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether the Improved Ability power (SR5 309) should be capped at 0.5 of current Rating or 1.5 of current Rating.
        /// </summary>
        public bool IncreasedImprovedAbilityMultiplier
        {
            get => _blnIncreasedImprovedAbilityMultiplier;
            set
            {
                if (_blnIncreasedImprovedAbilityMultiplier != value)
                {
                    _blnIncreasedImprovedAbilityMultiplier = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether lifestyles will automatically give free grid subscriptions found in (HT)
        /// </summary>
        public bool AllowFreeGrids
        {
            get => _blnAllowFreeGrids;
            set
            {
                if (_blnAllowFreeGrids != value)
                {
                    _blnAllowFreeGrids = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether Technomancers are allowed to use the Schooling discount on their initiations in the same manner as awakened.
        /// </summary>
        public bool AllowTechnomancerSchooling
        {
            get => _blnAllowTechnomancerSchooling;
            set
            {
                if (_blnAllowTechnomancerSchooling != value)
                {
                    _blnAllowTechnomancerSchooling = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Override the maximum value of bonuses that can affect cyberlimbs.
        /// </summary>
        public bool CyberlimbAttributeBonusCapOverride
        {
            get => _blnCyberlimbAttributeBonusCapOverride;
            set
            {
                if (_blnCyberlimbAttributeBonusCapOverride != value)
                {
                    _blnCyberlimbAttributeBonusCapOverride = value;
                    OnPropertyChanged();
                    if (!value)
                        CyberlimbAttributeBonusCap = 4;
                }
            }
        }

        /// <summary>
        /// Maximum value of bonuses that can affect cyberlimbs.
        /// </summary>
        public int CyberlimbAttributeBonusCap
        {
            get => _intCyberlimbAttributeBonusCap;
            set
            {
                if (_intCyberlimbAttributeBonusCap != value)
                {
                    _intCyberlimbAttributeBonusCap = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The Dice Penalty per Spell
        /// </summary>
        public int DicePenaltySustaining
        {
            get => _intDicePenaltySustaining;
            set
            {
                if (_intDicePenaltySustaining != value)
                {
                    _intDicePenaltySustaining = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion Properties and Methods

        #region Karma

        /// <summary>
        /// Karma cost to improve an Attribute = New Rating X this value.
        /// </summary>
        public int KarmaAttribute
        {
            get => _intKarmaAttribute;
            set
            {
                if (_intKarmaAttribute != value)
                {
                    _intKarmaAttribute = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost to purchase a Quality = BP Cost x this value.
        /// </summary>
        public int KarmaQuality
        {
            get => _intKarmaQuality;
            set
            {
                if (_intKarmaQuality != value)
                {
                    _intKarmaQuality = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost to purchase a Specialization for an active skill = this value.
        /// </summary>
        public int KarmaSpecialization
        {
            get => _intKarmaSpecialization;
            set
            {
                if (_intKarmaSpecialization != value)
                {
                    _intKarmaSpecialization = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost to purchase a Specialization for a knowledge skill = this value.
        /// </summary>
        public int KarmaKnowledgeSpecialization
        {
            get => _intKarmaKnoSpecialization;
            set
            {
                if (_intKarmaKnoSpecialization != value)
                {
                    _intKarmaKnoSpecialization = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost to purchase a new Knowledge Skill = this value.
        /// </summary>
        public int KarmaNewKnowledgeSkill
        {
            get => _intKarmaNewKnowledgeSkill;
            set
            {
                if (_intKarmaNewKnowledgeSkill != value)
                {
                    _intKarmaNewKnowledgeSkill = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost to purchase a new Active Skill = this value.
        /// </summary>
        public int KarmaNewActiveSkill
        {
            get => _intKarmaNewActiveSkill;
            set
            {
                if (_intKarmaNewActiveSkill != value)
                {
                    _intKarmaNewActiveSkill = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost to purchase a new Skill Group = this value.
        /// </summary>
        public int KarmaNewSkillGroup
        {
            get => _intKarmaNewSkillGroup;
            set
            {
                if (_intKarmaNewSkillGroup != value)
                {
                    _intKarmaNewSkillGroup = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost to improve a Knowledge Skill = New Rating x this value.
        /// </summary>
        public int KarmaImproveKnowledgeSkill
        {
            get => _intKarmaImproveKnowledgeSkill;
            set
            {
                if (_intKarmaImproveKnowledgeSkill != value)
                {
                    _intKarmaImproveKnowledgeSkill = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost to improve an Active Skill = New Rating x this value.
        /// </summary>
        public int KarmaImproveActiveSkill
        {
            get => _intKarmaImproveActiveSkill;
            set
            {
                if (_intKarmaImproveActiveSkill != value)
                {
                    _intKarmaImproveActiveSkill = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost to improve a Skill Group = New Rating x this value.
        /// </summary>
        public int KarmaImproveSkillGroup
        {
            get => _intKarmaImproveSkillGroup;
            set
            {
                if (_intKarmaImproveSkillGroup != value)
                {
                    _intKarmaImproveSkillGroup = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for each Spell = this value.
        /// </summary>
        public int KarmaSpell
        {
            get => _intKarmaSpell;
            set
            {
                if (_intKarmaSpell != value)
                {
                    _intKarmaSpell = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for each Enhancement = this value.
        /// </summary>
        public int KarmaEnhancement
        {
            get => _intKarmaEnhancement;
            set
            {
                if (_intKarmaEnhancement != value)
                {
                    _intKarmaEnhancement = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for a new Complex Form = this value.
        /// </summary>
        public int KarmaNewComplexForm
        {
            get => _intKarmaNewComplexForm;
            set
            {
                if (_intKarmaNewComplexForm != value)
                {
                    _intKarmaNewComplexForm = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for a new AI Program
        /// </summary>
        public int KarmaNewAIProgram
        {
            get => _intKarmaNewAIProgram;
            set
            {
                if (_intKarmaNewAIProgram != value)
                {
                    _intKarmaNewAIProgram = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for a new AI Advanced Program
        /// </summary>
        public int KarmaNewAIAdvancedProgram
        {
            get => _intKarmaNewAIAdvancedProgram;
            set
            {
                if (_intKarmaNewAIAdvancedProgram != value)
                {
                    _intKarmaNewAIAdvancedProgram = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for a Contact = (Connection + Loyalty) x this value.
        /// </summary>
        public int KarmaContact
        {
            get => _intKarmaContact;
            set
            {
                if (_intKarmaContact != value)
                {
                    _intKarmaContact = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for an Enemy = (Connection + Loyalty) x this value.
        /// </summary>
        public int KarmaEnemy
        {
            get => _intKarmaEnemy;
            set
            {
                if (_intKarmaEnemy != value)
                {
                    _intKarmaEnemy = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Maximum amount of remaining Karma that is carried over to the character once they are created.
        /// </summary>
        public int KarmaCarryover
        {
            get => _intKarmaCarryover;
            set
            {
                if (_intKarmaCarryover != value)
                {
                    _intKarmaCarryover = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for a Spirit = this value.
        /// </summary>
        public int KarmaSpirit
        {
            get => _intKarmaSpirit;
            set
            {
                if (_intKarmaSpirit != value)
                {
                    _intKarmaSpirit = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for a Martial Arts Technique = this value.
        /// </summary>
        public int KarmaTechnique
        {
            get => _intKarmaTechnique;
            set
            {
                if (_intKarmaTechnique != value)
                {
                    _intKarmaTechnique = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for an Initiation = KarmaInitiationFlat + (New Rating x this value).
        /// </summary>
        public int KarmaInitiation
        {
            get => _intKarmaInitiation;
            set
            {
                if (_intKarmaInitiation != value)
                {
                    _intKarmaInitiation = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for an Initiation = this value + (New Rating x KarmaInitiation).
        /// </summary>
        public int KarmaInitiationFlat
        {
            get => _intKarmaInitiationFlat;
            set
            {
                if (_intKarmaInitiationFlat != value)
                {
                    _intKarmaInitiationFlat = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for a Metamagic = this value.
        /// </summary>
        public int KarmaMetamagic
        {
            get => _intKarmaMetamagic;
            set
            {
                if (_intKarmaMetamagic != value)
                {
                    _intKarmaMetamagic = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost to join a Group = this value.
        /// </summary>
        public int KarmaJoinGroup
        {
            get => _intKarmaJoinGroup;
            set
            {
                if (_intKarmaJoinGroup != value)
                {
                    _intKarmaJoinGroup = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost to leave a Group = this value.
        /// </summary>
        public int KarmaLeaveGroup
        {
            get => _intKarmaLeaveGroup;
            set
            {
                if (_intKarmaLeaveGroup != value)
                {
                    _intKarmaLeaveGroup = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for Alchemical Foci.
        /// </summary>
        public int KarmaAlchemicalFocus
        {
            get => _intKarmaAlchemicalFocus;
            set
            {
                if (_intKarmaAlchemicalFocus != value)
                {
                    _intKarmaAlchemicalFocus = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for Banishing Foci.
        /// </summary>
        public int KarmaBanishingFocus
        {
            get => _intKarmaBanishingFocus;
            set
            {
                if (_intKarmaBanishingFocus != value)
                {
                    _intKarmaBanishingFocus = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for Binding Foci.
        /// </summary>
        public int KarmaBindingFocus
        {
            get => _intKarmaBindingFocus;
            set
            {
                if (_intKarmaBindingFocus != value)
                {
                    _intKarmaBindingFocus = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for Centering Foci.
        /// </summary>
        public int KarmaCenteringFocus
        {
            get => _intKarmaCenteringFocus;
            set
            {
                if (_intKarmaCenteringFocus != value)
                {
                    _intKarmaCenteringFocus = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for Counterspelling Foci.
        /// </summary>
        public int KarmaCounterspellingFocus
        {
            get => _intKarmaCounterspellingFocus;
            set
            {
                if (_intKarmaCounterspellingFocus != value)
                {
                    _intKarmaCounterspellingFocus = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for Disenchanting Foci.
        /// </summary>
        public int KarmaDisenchantingFocus
        {
            get => _intKarmaDisenchantingFocus;
            set
            {
                if (_intKarmaDisenchantingFocus != value)
                {
                    _intKarmaDisenchantingFocus = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for Flexible Signature Foci.
        /// </summary>
        public int KarmaFlexibleSignatureFocus
        {
            get => _intKarmaFlexibleSignatureFocus;
            set
            {
                if (_intKarmaFlexibleSignatureFocus != value)
                {
                    _intKarmaFlexibleSignatureFocus = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for Masking Foci.
        /// </summary>
        public int KarmaMaskingFocus
        {
            get => _intKarmaMaskingFocus;
            set
            {
                if (_intKarmaMaskingFocus != value)
                {
                    _intKarmaMaskingFocus = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for Power Foci.
        /// </summary>
        public int KarmaPowerFocus
        {
            get => _intKarmaPowerFocus;
            set
            {
                if (_intKarmaPowerFocus != value)
                {
                    _intKarmaPowerFocus = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for Qi Foci.
        /// </summary>
        public int KarmaQiFocus
        {
            get => _intKarmaQiFocus;
            set
            {
                if (_intKarmaQiFocus != value)
                {
                    _intKarmaQiFocus = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for Ritual Spellcasting Foci.
        /// </summary>
        public int KarmaRitualSpellcastingFocus
        {
            get => _intKarmaRitualSpellcastingFocus;
            set
            {
                if (_intKarmaRitualSpellcastingFocus != value)
                {
                    _intKarmaRitualSpellcastingFocus = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for Spellcasting Foci.
        /// </summary>
        public int KarmaSpellcastingFocus
        {
            get => _intKarmaSpellcastingFocus;
            set
            {
                if (_intKarmaSpellcastingFocus != value)
                {
                    _intKarmaSpellcastingFocus = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for Spell Shaping Foci.
        /// </summary>
        public int KarmaSpellShapingFocus
        {
            get => _intKarmaSpellShapingFocus;
            set
            {
                if (_intKarmaSpellShapingFocus != value)
                {
                    _intKarmaSpellShapingFocus = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for Summoning Foci.
        /// </summary>
        public int KarmaSummoningFocus
        {
            get => _intKarmaSummoningFocus;
            set
            {
                if (_intKarmaSummoningFocus != value)
                {
                    _intKarmaSummoningFocus = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for Sustaining Foci.
        /// </summary>
        public int KarmaSustainingFocus
        {
            get => _intKarmaSustainingFocus;
            set
            {
                if (_intKarmaSustainingFocus != value)
                {
                    _intKarmaSustainingFocus = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for Weapon Foci.
        /// </summary>
        public int KarmaWeaponFocus
        {
            get => _intKarmaWeaponFocus;
            set
            {
                if (_intKarmaWeaponFocus != value)
                {
                    _intKarmaWeaponFocus = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// How much Karma a single Power Point costs for a Mystic Adept.
        /// </summary>
        public int KarmaMysticAdeptPowerPoint
        {
            get => _intKarmaMysticAdeptPowerPoint;
            set
            {
                if (_intKarmaMysticAdeptPowerPoint != value)
                {
                    _intKarmaMysticAdeptPowerPoint = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for fetting a spirit (gets multiplied by Force).
        /// </summary>
        public int KarmaSpiritFettering
        {
            get => _intKarmaSpiritFettering;
            set
            {
                if (_intKarmaSpiritFettering != value)
                {
                    _intKarmaSpiritFettering = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion Karma

        #region Default Build

        /// <summary>
        /// Percentage by which adding an Initiate Grade to an Awakened is discounted if a member of a Group.
        /// </summary>
        public decimal KarmaMAGInitiationGroupPercent { get; set; } = 0.1m;

        /// <summary>
        /// Percentage by which adding a Submersion Grade to a Technomancer is discounted if a member of a Group.
        /// </summary>
        public decimal KarmaRESInitiationGroupPercent { get; set; } = 0.1m;

        /// <summary>
        /// Percentage by which adding an Initiate Grade to an Awakened is discounted if performing an Ordeal.
        /// </summary>
        public decimal KarmaMAGInitiationOrdealPercent { get; set; } = 0.1m;

        /// <summary>
        /// Percentage by which adding a Submersion Grade to a Technomancer is discounted if performing an Ordeal.
        /// </summary>
        public decimal KarmaRESInitiationOrdealPercent { get; set; } = 0.2m;

        /// <summary>
        /// Percentage by which adding an Initiate Grade to an Awakened is discounted if performing an Ordeal.
        /// </summary>
        public decimal KarmaMAGInitiationSchoolingPercent { get; set; } = 0.1m;

        /// <summary>
        /// Percentage by which adding a Submersion Grade to a Technomancer is discounted if performing an Ordeal.
        /// </summary>
        public decimal KarmaRESInitiationSchoolingPercent { get; set; } = 0.1m;

        #endregion Default Build

        #region Constant Values

        /// <summary>
        /// The value by which Specializations add to dicepool.
        /// </summary>
        public int SpecializationBonus { get; } = 2;

        #endregion Constant Values
    }
}
